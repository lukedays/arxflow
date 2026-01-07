using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using ArxFlow.Server.Models;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Serilog;

namespace ArxFlow.Server.Services;

/// <summary>
/// Servico para baixar informacoes de instrumentos de derivativos da B3
/// </summary>
public class B3InstrumentsService(HttpClient httpClient)
{
    // Registra provider de encoding para Windows-1252
    static B3InstrumentsService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private const string ExportUrl = "https://arquivos.b3.com.br/bdi/table/export/csv?lang=pt-BR";

    /// <summary>
    /// Baixa instrumentos de derivativos para um periodo
    /// </summary>
    public async Task<List<B3InstrumentoDerivativo>> FetchInstrumentsAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var payload = new ExportRequest
        {
            Name = "InstrumentsDerivatives",
            Date = startDate.ToString("yyyy-MM-dd"),
            FinalDate = endDate.ToString("yyyy-MM-dd"),
        };

        var response = await httpClient.PostAsJsonAsync(ExportUrl, payload);

        if (!response.IsSuccessStatusCode)
            return [];

        // Arquivo vem em UTF-8 com BOM - remove o BOM se existir
        var csv = await response.Content.ReadAsStringAsync();
        if (csv.Length > 0 && csv[0] == '\uFEFF')
            csv = csv[1..];

        var allInstruments = ParseCsv(csv, startDate);
        var instruments = allInstruments.Where(i => i.Ativo is "DI1" or "DAP").ToList();
        return instruments;
    }

    /// <summary>
    /// Baixa instrumentos para uma data especifica
    /// </summary>
    public Task<List<B3InstrumentoDerivativo>> FetchInstrumentsAsync(DateTime date) =>
        FetchInstrumentsAsync(date, date);

    private static List<B3InstrumentoDerivativo> ParseCsv(string csv, DateTime referenceDate)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Pula a primeira linha (descricao) e usa a segunda como cabecalho
        if (lines.Length < 3)
        {
            Log.Warning("CSV com menos de 3 linhas: {Lines}", lines.Length);
            return [];
        }

        // Reconstroi o CSV apenas com cabecalho e dados (sem linha de descricao)
        var csvWithoutDescription = string.Join('\n', lines.Skip(1));

        var config = new CsvConfiguration(new CultureInfo("pt-BR"))
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
        };

        using var reader = new StringReader(csvWithoutDescription);
        using var csvReader = new CsvReader(reader, config);

        csvReader.Context.RegisterClassMap<B3InstrumentoDerivativoMap>();
        csvReader.Context.TypeConverterCache.AddConverter<DateTime?>(new B3DateConverter());
        csvReader.Context.TypeConverterCache.AddConverter<decimal?>(new B3DecimalConverter());
        csvReader.Context.TypeConverterCache.AddConverter<int?>(new B3IntConverter());

        var instruments = new List<B3InstrumentoDerivativo>();

        try
        {
            while (csvReader.Read())
            {
                try
                {
                    var record = csvReader.GetRecord<B3InstrumentoDerivativo>();
                    if (record != null)
                    {
                        record.DataReferencia = referenceDate;
                        instruments.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    // Loga erro mas continua processando outras linhas
                    Log.Debug("Erro ao parsear linha {Row}: {Error}", csvReader.Context.Parser?.Row, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning("Erro geral ao parsear CSV: {Error}", ex.Message);
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
/// Mapeamento de colunas CSV para B3InstrumentoDerivativo usando nomes de cabecalho
/// </summary>
public sealed class B3InstrumentoDerivativoMap : ClassMap<B3InstrumentoDerivativo>
{
    public B3InstrumentoDerivativoMap()
    {
        // Nomes exatos dos cabecalhos do CSV da B3 (com acentos especificos)
        Map(m => m.InstrumentoFinanceiro).Name("Instrumento financeiro");
        Map(m => m.Ativo).Name("Ativo");
        Map(m => m.DescricaoAtivo).Name("Descrição do ativo");
        Map(m => m.Segmento).Name("Segmento");
        Map(m => m.Mercado).Name("Mercado");
        Map(m => m.Categoria).Name("Categoria");
        Map(m => m.DataVencimento).Name("Data de expiração").TypeConverter<B3DateConverter>();
        Map(m => m.CodigoExpiracao).Name("Código de expiração");
        Map(m => m.DataInicioNegocio).Name("Data início negócio").TypeConverter<B3DateConverter>();
        Map(m => m.DataFimNegocio).Name("Data fim negócio").TypeConverter<B3DateConverter>();
        Map(m => m.CodigoBase).Name("Código-base");
        Map(m => m.CriterioConversao).Name("Critério de conversão");
        Map(m => m.DataMaturidadeAlvo).Name("Data de maturidade alvo");
        Map(m => m.IndicadorConversao).Name("Indicador de conversão");
        Map(m => m.CodigoIsin).Name("Código ISIN");
        Map(m => m.CodigoCfi).Name("Código CFI");
        Map(m => m.DataInicioAvisoEntrega).Name("Data início aviso de entrega").TypeConverter<B3DateConverter>();
        Map(m => m.DataFimAvisoEntrega).Name("Data fim aviso de entrega").TypeConverter<B3DateConverter>();
        Map(m => m.TipoOpcao).Name("Tipo de opção");
        Map(m => m.MultiplicadorContrato).Name("Multiplicador do contrato").TypeConverter<B3DecimalConverter>();
        Map(m => m.QuantidadeAtivos).Name("Quantidade de ativos").TypeConverter<B3DecimalConverter>();
        Map(m => m.TamanhoLoteAlocacao).Name("Tamanho de lote de alocação").TypeConverter<B3IntConverter>();
        Map(m => m.MoedaNegociada).Name("Moeda negociada");
        Map(m => m.TipoEntrega).Name("Tipo de entrega");
        Map(m => m.DiasSaque).Name("Dias de saque").TypeConverter<B3IntConverter>();
        Map(m => m.DiasUteis).Name("Dias úteis").TypeConverter<B3IntConverter>();
        Map(m => m.DiasCorridos).Name("Dias corridos").TypeConverter<B3IntConverter>();
        Map(m => m.PrecoBaseEstrategia).Name("Preço base valor estratégia");
        Map(m => m.DiasPosicaoFutura).Name("Dias para posição futura").TypeConverter<B3IntConverter>();
        Map(m => m.CodigoTipoEstrategia1).Name("Código tipo estratégia 1");
        Map(m => m.SimboloSubjacente1).Name("Símbolo subjacente 1");
        Map(m => m.CodigoTipoEstrategia2).Name("Código tipo estratégia 2");
        Map(m => m.SimboloSubjacente2).Name("Símbolo subjacente 2");
        Map(m => m.PrecoExercicio).Name("Preço de exercício").TypeConverter<B3DecimalConverter>();
        Map(m => m.EstiloOpcao).Name("Estilo de opção");
        Map(m => m.TipoValor).Name("Tipo de valor");
        Map(m => m.IndicadorPremioAntecipado).Name("Indicador de prêmio antecipado");
        Map(m => m.DataLimitePosicoesAbertas).Name("Data limite posições em aberto").TypeConverter<B3DateConverter>();

        // Ignora colunas que nao existem no CSV
        Map(m => m.Id).Ignore();
        Map(m => m.DataReferencia).Ignore();
    }
}

/// <summary>
/// Conversor de data no formato dd/MM/yyyy da B3
/// </summary>
public class B3DateConverter : DefaultTypeConverter
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text) || text == "-")
            return null;

        if (DateTime.TryParseExact(text, "dd/MM/yyyy", PtBr, DateTimeStyles.None, out var date))
            return date;

        return null;
    }
}

/// <summary>
/// Conversor de decimal com suporte a formato pt-BR
/// </summary>
public class B3DecimalConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text) || text == "-")
            return null;

        // Tenta formato invariante primeiro (1234.56)
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            return num;

        // Tenta formato pt-BR (1.234,56)
        if (decimal.TryParse(text, NumberStyles.Any, new CultureInfo("pt-BR"), out num))
            return num;

        return null;
    }
}

/// <summary>
/// Conversor de inteiro com suporte a separador de milhar pt-BR
/// </summary>
public class B3IntConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text) || text == "-")
            return null;

        // Remove separador de milhar pt-BR (ponto)
        var cleanValue = text.Replace(".", "");
        if (int.TryParse(cleanValue, out var num))
            return num;

        return null;
    }
}
