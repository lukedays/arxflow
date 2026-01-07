using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ArxFlow.Server.Models;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Serilog;

namespace ArxFlow.Server.Services;

/// <summary>
/// Servico para baixar informacoes de instrumentos de renda fixa da B3
/// </summary>
public class B3RendaFixaService(HttpClient httpClient)
{
    private const string ExportUrl = "https://arquivos.b3.com.br/bdi/table/export/csv?lang=pt-BR";

    // Instrumentos de renda fixa permitidos
    private static readonly HashSet<string> InstrumentosPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "CRA", "CRI", "DEB", "LF", "LFSC", "LFSN", "CFF", "NC"
    };

    /// <summary>
    /// Baixa instrumentos de renda fixa para um periodo
    /// </summary>
    public async Task<List<B3InstrumentoRendaFixa>> FetchInstrumentsAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        Log.Information(
            "Baixando instrumentos renda fixa de {Start:yyyy-MM-dd} a {End:yyyy-MM-dd}",
            startDate,
            endDate
        );

        var payload = new ExportRequest
        {
            Name = "InstrumentRegistration",
            Date = startDate.ToString("yyyy-MM-dd"),
            FinalDate = endDate.ToString("yyyy-MM-dd"),
        };

        Log.Information("Enviando request para {Url} com payload: {Payload}", ExportUrl,
            System.Text.Json.JsonSerializer.Serialize(payload));

        var response = await httpClient.PostAsJsonAsync(ExportUrl, payload);

        Log.Information("Response status: {Status}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Log.Warning("Falha ao baixar instrumentos renda fixa: {Status} - {Error}", response.StatusCode, errorContent);
            return [];
        }

        var csv = await response.Content.ReadAsStringAsync();
        Log.Information("CSV recebido: {Length} caracteres, primeiros 500: {Preview}",
            csv.Length, csv.Length > 500 ? csv[..500] : csv);

        var allInstruments = ParseCsv(csv, startDate);
        Log.Information("Total parseado: {Count} instrumentos", allInstruments.Count);

        // Filtra apenas os instrumentos permitidos
        var instruments = allInstruments
            .Where(i => InstrumentosPermitidos.Contains(i.InstrumentoFinanceiro))
            .ToList();
        Log.Information("Filtrado para tipos permitidos: {Count} instrumentos", instruments.Count);

        return instruments;
    }

    /// <summary>
    /// Baixa instrumentos para uma data especifica
    /// </summary>
    public Task<List<B3InstrumentoRendaFixa>> FetchInstrumentsAsync(DateTime date) =>
        FetchInstrumentsAsync(date, date);

    private static List<B3InstrumentoRendaFixa> ParseCsv(string csv, DateTime referenceDate)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // CSV tem 2 linhas de cabecalho (descricao + glossario) antes dos dados
        if (lines.Length < 4)
        {
            Log.Warning("CSV com menos de 4 linhas: {Lines}", lines.Length);
            return [];
        }

        // Verifica se ha dados
        if (lines.Any(l => l.Trim() == "Nenhum resultado"))
        {
            Log.Information("Nenhum resultado encontrado no CSV");
            return [];
        }

        // Pula as primeiras 2 linhas (descricao e glossario) e usa a terceira como cabecalho
        var csvWithoutHeader = string.Join('\n', lines.Skip(2));

        var config = new CsvConfiguration(new CultureInfo("pt-BR"))
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
        };

        using var reader = new StringReader(csvWithoutHeader);
        using var csvReader = new CsvReader(reader, config);

        csvReader.Context.RegisterClassMap<B3InstrumentoRendaFixaMap>();
        csvReader.Context.TypeConverterCache.AddConverter<DateTime?>(new B3DateConverter());
        csvReader.Context.TypeConverterCache.AddConverter<decimal?>(new B3DecimalConverter());
        csvReader.Context.TypeConverterCache.AddConverter<long?>(new B3LongConverter());

        var instruments = new List<B3InstrumentoRendaFixa>();

        try
        {
            foreach (var record in csvReader.GetRecords<B3InstrumentoRendaFixa>())
            {
                record.DataReferencia = referenceDate;
                instruments.Add(record);
            }
        }
        catch (Exception ex)
        {
            Log.Warning("Erro ao parsear CSV: {Error}", ex.Message);
        }

        return instruments;
    }

    private class ExportRequest
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("FinalDate")]
        public string FinalDate { get; set; } = "";

        [JsonPropertyName("ClientId")]
        public string ClientId { get; set; } = "";

        [JsonPropertyName("Filters")]
        public object Filters { get; set; } = new { };
    }
}

/// <summary>
/// Mapeamento de colunas CSV para B3InstrumentoRendaFixa usando nomes de cabecalho
/// </summary>
public sealed class B3InstrumentoRendaFixaMap : ClassMap<B3InstrumentoRendaFixa>
{
    public B3InstrumentoRendaFixaMap()
    {
        // Nomes exatos dos cabecalhos do CSV da B3
        Map(m => m.CodigoIF).Name("Código IF");
        Map(m => m.CodigoIsin).Name("Código ISIN");
        Map(m => m.Emissor).Name("Emissor");
        Map(m => m.InstrumentoFinanceiro).Name("Instrumento financeiro");
        Map(m => m.Incentivada).Name("Incentivada");
        Map(m => m.NumeroSerie).Name("Número de série");
        Map(m => m.NumeroEmissao).Name("Número de emissão");
        Map(m => m.Indexador).Name("Indexador");
        Map(m => m.PercentualIndexador).Name("Percentual indexador").TypeConverter<B3DecimalConverter>();
        Map(m => m.TaxaAdicional).Name("Taxa adicional").TypeConverter<B3DecimalConverter>();
        Map(m => m.BaseCalculo).Name("Base cálculo");
        Map(m => m.Vencimento).Name("Vencimento").TypeConverter<B3DateConverter>();
        Map(m => m.QuantidadeEmitida).Name("Quantidade emitida").TypeConverter<B3LongConverter>();
        Map(m => m.PrecoUnitarioEmissao).Name("Preço unitário de emissão").TypeConverter<B3DecimalConverter>();
        Map(m => m.EsforcoRestrito).Name("Esforço restrito");
        Map(m => m.TipoEmissao).Name("Tipo de emissão");
        Map(m => m.IndicadorOferta).Name("Indicador oferta");

        // Ignora colunas que nao existem no CSV
        Map(m => m.Id).Ignore();
        Map(m => m.DataReferencia).Ignore();
    }
}

/// <summary>
/// Conversor de long com suporte a separador de milhar pt-BR
/// </summary>
public class B3LongConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text) || text == "-")
            return null;

        // Remove separador de milhar pt-BR (ponto)
        var cleanValue = text.Replace(".", "");
        if (long.TryParse(cleanValue, out var num))
            return num;

        return null;
    }
}
