using System.Globalization;
using System.Text;
using ArxFlow.Server.Models;
using Serilog;

namespace ArxFlow.Server.Services;

// Servico para baixar dados do Mercado Secundario de Titulos Publicos Federais (ANBIMA)
public class AnbimaDownloadService(HttpClient httpClient)
{
    // Registra provider de encoding para Windows-1252
    static AnbimaDownloadService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    // URL base do arquivo ANBIMA
    private const string BaseUrl = "https://www.anbima.com.br/informacoes/merc-sec/arqs";

    // Titulos permitidos
    private static readonly HashSet<string> TitulosPermitidos = ["LTN", "NTN-B", "NTN-C", "NTN-F", "LFT"];

    /// <summary>
    /// Baixa dados TPF da ANBIMA para uma data especifica
    /// </summary>
    /// <param name="data">Data de referencia</param>
    /// <returns>Lista de titulos publicos federais</returns>
    public async Task<List<AnbimaTPF>> FetchTPFAsync(DateTime data)
    {
        // Formato do arquivo: msYYMMDD.txt (ex: ms260102.txt para 02/01/2026)
        var fileName = $"ms{data:yyMMdd}.txt";
        var url = $"{BaseUrl}/{fileName}";

        Log.Information("Baixando arquivo ANBIMA: {Url}", url);

        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Falha ao baixar arquivo ANBIMA: {StatusCode}", response.StatusCode);
                return [];
            }

            // Arquivo vem em encoding Windows-1252 (Latin-1)
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var content = Encoding.GetEncoding("Windows-1252").GetString(bytes);

            return ParseFile(content, data);
        }
        catch (HttpRequestException ex)
        {
            Log.Warning("Erro ao baixar arquivo ANBIMA: {Message}", ex.Message);
            return [];
        }
    }

    /// <summary>
    /// Baixa dados TPF para um periodo de datas
    /// </summary>
    public async Task<List<AnbimaTPF>> FetchTPFPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var allData = new List<AnbimaTPF>();
        var currentDate = dataFim;

        while (currentDate >= dataInicio)
        {
            // Pula fins de semana
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                var data = await FetchTPFAsync(currentDate);
                if (data.Count > 0)
                {
                    allData.AddRange(data);
                    Log.Information("{Count} registros baixados para {Data:dd/MM/yyyy}", data.Count, currentDate);
                }
            }

            currentDate = currentDate.AddDays(-1);
        }

        return allData;
    }

    /// <summary>
    /// Parseia o conteudo do arquivo ANBIMA
    /// </summary>
    private List<AnbimaTPF> ParseFile(string content, DateTime dataReferencia)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<AnbimaTPF>();
        var headerFound = false;
        var columnMap = new Dictionary<string, int>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            // Procura a linha de cabecalho (contem "Titulo" e separador @)
            if (!headerFound && trimmedLine.Contains('@') &&
                (trimmedLine.Contains("Titulo", StringComparison.OrdinalIgnoreCase) ||
                 trimmedLine.Contains("Tx. Indicativas", StringComparison.OrdinalIgnoreCase)))
            {
                var headers = trimmedLine.Split('@');
                for (int i = 0; i < headers.Length; i++)
                {
                    var header = headers[i].Trim();
                    columnMap[NormalizeHeader(header)] = i;
                }
                headerFound = true;
                Log.Debug("Cabecalho encontrado: {Headers}", string.Join(", ", columnMap.Keys));
                continue;
            }

            // Pula linhas antes do cabecalho
            if (!headerFound) continue;

            // Parseia linha de dados
            var columns = trimmedLine.Split('@');
            if (columns.Length < 5) continue;

            var titulo = GetColumn(columns, columnMap, "titulo")?.Trim();
            if (string.IsNullOrEmpty(titulo)) continue;

            // Filtra apenas titulos permitidos
            if (!TitulosPermitidos.Any(t => titulo.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
                continue;

            var tpf = new AnbimaTPF
            {
                Titulo = titulo,
                DataReferencia = ParseDate(GetColumn(columns, columnMap, "datareferencia")) ?? dataReferencia,
                CodigoSelic = GetColumn(columns, columnMap, "codigoselic")?.Trim(),
                DataBase = ParseDate(GetColumn(columns, columnMap, "database")),
                DataVencimento = ParseDate(GetColumn(columns, columnMap, "datavencimento")),
                TaxaCompra = ParseDecimalPtBr(GetColumn(columns, columnMap, "txcompra")),
                TaxaVenda = ParseDecimalPtBr(GetColumn(columns, columnMap, "txvenda")),
                TaxaIndicativa = ParseDecimalPtBr(GetColumn(columns, columnMap, "txindicativas")),
                PU = ParseDecimalPtBr(GetColumn(columns, columnMap, "pu")),
                DesvioPadrao = ParseDecimalPtBr(GetColumn(columns, columnMap, "desviopadrao")),
                IntervaloIndMinD0 = ParseDecimalPtBr(GetColumn(columns, columnMap, "intervaloindd0min")),
                IntervaloIndMaxD0 = ParseDecimalPtBr(GetColumn(columns, columnMap, "intervaloindd0max")),
                IntervaloIndMinD1 = ParseDecimalPtBr(GetColumn(columns, columnMap, "intervaloindd1min")),
                IntervaloIndMaxD1 = ParseDecimalPtBr(GetColumn(columns, columnMap, "intervaloindd1max")),
                Criterio = GetColumn(columns, columnMap, "criterio")?.Trim(),
                ImportedAt = DateTime.UtcNow
            };

            result.Add(tpf);
        }

        // Agrupa por titulo para log
        var grupos = result.GroupBy(t => t.Titulo.Split(' ')[0]).ToDictionary(g => g.Key, g => g.Count());
        Log.Information("Parseados {Count} titulos: {Grupos}", result.Count, string.Join(", ", grupos.Select(g => $"{g.Key}:{g.Value}")));

        return result;
    }

    /// <summary>
    /// Normaliza nome de cabecalho removendo acentos, espacos e caracteres especiais
    /// </summary>
    private static string NormalizeHeader(string header)
    {
        // Remove acentos
        var normalized = header.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        // Remove espacos, pontos, asteriscos e converte para minusculo
        return sb.ToString()
            .Replace(" ", "")
            .Replace(".", "")
            .Replace("*", "")
            .Replace("/", "")
            .Replace("-", "")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Obtem valor de coluna pelo nome normalizado
    /// </summary>
    private static string? GetColumn(string[] columns, Dictionary<string, int> columnMap, string normalizedName)
    {
        // Procura coluna que contem o nome (parcial match)
        var key = columnMap.Keys.FirstOrDefault(k => k.Contains(normalizedName) || normalizedName.Contains(k));
        if (key != null && columnMap.TryGetValue(key, out var index) && index < columns.Length)
        {
            return columns[index];
        }
        return null;
    }

    /// <summary>
    /// Parseia data no formato YYYYMMDD ou DD/MM/YYYY
    /// </summary>
    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Trim();

        // Formato YYYYMMDD
        if (value.Length == 8 && DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date1))
            return date1;

        // Formato DD/MM/YYYY
        if (DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date2))
            return date2;

        // Formato DDMMYYYY
        if (value.Length == 8 && DateTime.TryParseExact(value, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date3))
            return date3;

        return null;
    }

    /// <summary>
    /// Parseia decimal no formato brasileiro (virgula como separador decimal)
    /// </summary>
    private static decimal? ParseDecimalPtBr(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Trim();
        if (value == "--" || value == "-" || value == "N/D") return null;

        // Substitui virgula por ponto
        value = value.Replace(".", "").Replace(",", ".");

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return null;
    }
}
