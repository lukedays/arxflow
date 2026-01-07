using System.IO.Compression;
using System.Xml.Serialization;
using ArxFlow.Server.Models;
using ArxFlow.Server.Utils;
using Serilog;
using Bvmf052 = ArxFlow.Server.Models.B3.Bvmf052;

namespace ArxFlow.Server.Services;

// Servico para baixar precos de derivativos da B3
public class B3DownloadService(HttpClient httpClient)
{
    private const string BaseUrl = "https://www.b3.com.br/pesquisapregao/download";
    private static readonly XmlSerializer Serializer = new(typeof(Bvmf052.Document));

    public static string FormatDateCode(DateTime date) => date.ToString("yyMMdd");

    /// <summary>
    /// Retorna o ultimo dia util (hoje se for dia util, senao o anterior)
    /// </summary>
    public static DateTime GetLastBusinessDay()
    {
        var date = DateTime.Today;
        if (BrazilianCalendar.IsBusinessDay(date, BrazilianCalendarType.Exchange))
            return date;
        return BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Exchange);
    }

    /// <summary>
    /// Retorna o dia util anterior a data informada
    /// </summary>
    public static DateTime GetPreviousBusinessDay(DateTime date) =>
        BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Exchange);

    public async Task<byte[]?> DownloadFileAsync(string dateCode)
    {
        var url = $"{BaseUrl}?filelist=SPRD{dateCode}.zip";
        Log.Information("Baixando {Url}", url);

        try
        {
            var response = await httpClient.GetAsync(url);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsByteArrayAsync()
                : null;
        }
        catch (HttpRequestException ex)
        {
            Log.Warning("Erro ao baixar: {Message}", ex.Message);
            return null;
        }
    }

    public string? ExtractXmlFromZip(byte[] zipBytes)
    {
        using var outerZip = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);

        foreach (
            var entry in outerZip.Entries.Where(e =>
                e.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            using var innerStream = new MemoryStream();
            using (var entryStream = entry.Open())
                entryStream.CopyTo(innerStream);
            innerStream.Position = 0;

            using var innerZip = new ZipArchive(innerStream, ZipArchiveMode.Read);
            var xmlEntry = innerZip.Entries.FirstOrDefault(e =>
                e.FullName.Contains("BVBG.187.01")
                && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
            );

            if (xmlEntry != null)
            {
                Log.Debug("XML encontrado: {FileName}", xmlEntry.FullName);
                using var reader = new StreamReader(xmlEntry.Open());
                return reader.ReadToEnd();
            }
        }

        var directXml = outerZip.Entries.FirstOrDefault(e =>
            e.FullName.Contains("BVBG.187.01")
            && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
        );

        if (directXml != null)
        {
            Log.Debug("XML encontrado: {FileName}", directXml.FullName);
            using var reader = new StreamReader(directXml.Open());
            return reader.ReadToEnd();
        }

        return null;
    }

    public List<B3PrecoDerivativo> ParseXml(string xmlContent, string[] prefixes)
    {
        using var reader = new StringReader(xmlContent);
        var document = (Bvmf052.Document)Serializer.Deserialize(reader)!;

        return document
                .BizFileHdr?.Xchg?.BizGrp?.Select(g => g.Document?.PricRpt)
                .Where(p => p?.SctyId?.TckrSymb != null)
                .Where(p =>
                    prefixes.Length == 0 || prefixes.Any(px => p!.SctyId!.TckrSymb!.StartsWith(px))
                )
                .Select(p => new B3PrecoDerivativo
                {
                    Ticker = p!.SctyId!.TckrSymb!,
                    DataReferencia = p.TradDt?.Dt.Date ?? DateTime.Today,
                    ContratosEmAberto =
                        p.FinInstrmAttrbts?.OpnIntrstSpecified == true
                            ? p.FinInstrmAttrbts.OpnIntrst
                            : null,
                    PrecoInicial = p.FinInstrmAttrbts?.FrstPric?.Value,
                    PrecoMinimo = p.FinInstrmAttrbts?.MinPric?.Value,
                    PrecoMaximo = p.FinInstrmAttrbts?.MaxPric?.Value,
                    PrecoMedioPonderado = p.FinInstrmAttrbts?.TradAvrgPric?.Value,
                    UltimoPreco = p.FinInstrmAttrbts?.LastPric?.Value,
                    QuantidadeNegociosRegulares =
                        p.FinInstrmAttrbts?.RglrTxsQtySpecified == true
                            ? p.FinInstrmAttrbts.RglrTxsQty
                            : null,
                    PrecoAjuste = p.FinInstrmAttrbts?.AdjstdQt?.Value,
                    TaxaAjuste = p.FinInstrmAttrbts?.AdjstdQtTax?.Value,
                    SituacaoPrecoAjuste = p.FinInstrmAttrbts?.AdjstdQtStin,
                    PrecoAjusteAnterior = p.FinInstrmAttrbts?.PrvsAdjstdQt?.Value,
                    TaxaAjusteAnterior = p.FinInstrmAttrbts?.PrvsAdjstdQtTax?.Value,
                    SituacaoPrecoAjusteAnterior = p.FinInstrmAttrbts?.PrvsAdjstdQtStin,
                    Moeda =
                        p.FinInstrmAttrbts?.FrstPric?.Ccy
                        ?? p.FinInstrmAttrbts?.AdjstdQt?.Ccy
                        ?? "BRL",
                    ImportedAt = DateTime.UtcNow,
                })
                .ToList()
            ?? [];
    }

    public async Task<List<B3PrecoDerivativo>> FetchAndParseAsync(
        string? dateCode = null,
        string[]? tickerPrefixes = null,
        int maxRetries = 5
    )
    {
        var prefixes = tickerPrefixes ?? ["DI1", "DAP"];
        var date =
            dateCode != null ? DateTime.ParseExact(dateCode, "yyMMdd", null) : GetLastBusinessDay();

        for (int i = 0; i < maxRetries; i++)
        {
            var code = FormatDateCode(date);
            Log.Information("Processando data: {DateCode}", code);

            var zipBytes = await DownloadFileAsync(code);
            if (zipBytes == null)
            {
                date = GetPreviousBusinessDay(date);
                continue;
            }

            var xml = ExtractXmlFromZip(zipBytes);
            if (xml == null)
            {
                Log.Warning("BVBG.187.01 nao encontrado");
                date = GetPreviousBusinessDay(date);
                continue;
            }

            var prices = ParseXml(xml, prefixes);
            Log.Information("Total de registros: {Count}", prices.Count);
            return prices;
        }

        Log.Warning("Nenhum arquivo encontrado apos {MaxRetries} tentativas", maxRetries);
        return [];
    }

    public async Task<List<B3PrecoDerivativo>> FetchHistoryAsync(
        int days,
        string[]? tickerPrefixes = null,
        DateTime? startDate = null
    )
    {
        var prefixes = tickerPrefixes ?? ["DI1", "DAP"];
        var date = startDate ?? GetLastBusinessDay();
        var allPrices = new List<B3PrecoDerivativo>();
        var processed = 0;

        Log.Information("Baixando historico de {Days} dias uteis", days);

        for (int attempts = 0; processed < days && attempts < days * 3; attempts++)
        {
            var code = FormatDateCode(date);
            Log.Information("[{Current}/{Total}] Data: {DateCode}", processed + 1, days, code);

            var zipBytes = await DownloadFileAsync(code);
            if (zipBytes != null)
            {
                var xml = ExtractXmlFromZip(zipBytes);
                if (xml != null)
                {
                    var prices = ParseXml(xml, prefixes);
                    if (prices.Count > 0)
                    {
                        allPrices.AddRange(prices);
                        Log.Information("{Count} registros importados", prices.Count);
                        processed++;
                    }
                }
                else
                    Log.Warning("BVBG.187.01 nao encontrado");
            }

            date = GetPreviousBusinessDay(date);
        }

        Log.Information("Total: {Count} registros de {Days} dias", allPrices.Count, processed);
        return allPrices;
    }
}
