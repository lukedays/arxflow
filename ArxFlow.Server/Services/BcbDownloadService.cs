using System.Text.Json;
using ArxFlow.Server.Models;
using Serilog;

namespace ArxFlow.Server.Services;

// Servico para baixar expectativas de mercado do Banco Central (Focus IPCA Top5 mensal)
public class BcbDownloadService(HttpClient httpClient)
{
    // URL base da API Olinda do BCB
    private const string BaseUrl = "https://olinda.bcb.gov.br/olinda/servico/Expectativas/versao/v1/odata";

    /// <summary>
    /// Baixa expectativas IPCA Top5 mensal para uma data especifica
    /// </summary>
    /// <param name="data">Data de coleta (YYYY-MM-DD)</param>
    /// <param name="indicador">Indicador (padrao: IPCA)</param>
    /// <returns>Lista de expectativas</returns>
    public async Task<List<BcbExpectativa>> FetchExpectativasAsync(DateTime data, string indicador = "IPCA")
    {
        var dataFormatada = data.ToString("yyyy-MM-dd");
        var url = $"{BaseUrl}/ExpectativasMercadoTop5Mensais?" +
                  $"$filter=Indicador%20eq%20'{indicador}'%20and%20Data%20eq%20'{dataFormatada}'" +
                  $"&$format=json" +
                  $"&$select=Indicador,Data,DataReferencia,tipoCalculo,Media,Mediana,DesvioPadrao,Minimo,Maximo";

        Log.Information("Baixando expectativas BCB: {Url}", url);

        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Falha ao baixar expectativas BCB: {StatusCode}", response.StatusCode);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BcbODataResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Value == null || result.Value.Count == 0)
            {
                Log.Warning("Nenhuma expectativa encontrada para {Data}", dataFormatada);
                return [];
            }

            // Remove duplicatas (mesma Data + Indicador + DataReferencia) e converte para lista
            var expectativas = result.Value
                .GroupBy(item => new { item.Data, item.Indicador, item.DataReferencia })
                .Select(g => g.First())
                .Select(item => new BcbExpectativa
                {
                    Indicador = item.Indicador ?? indicador,
                    Data = DateTime.Parse(item.Data ?? dataFormatada),
                    DataReferencia = item.DataReferencia ?? string.Empty,
                    TipoCalculo = item.TipoCalculo,
                    Media = ParseDecimal(item.Media),
                    Mediana = ParseDecimal(item.Mediana),
                    DesvioPadrao = ParseDecimal(item.DesvioPadrao),
                    Minimo = ParseDecimal(item.Minimo),
                    Maximo = ParseDecimal(item.Maximo),
                    ImportedAt = DateTime.UtcNow
                }).ToList();

            Log.Information("{Count} expectativas baixadas para {Data}", expectativas.Count, dataFormatada);
            return expectativas;
        }
        catch (HttpRequestException ex)
        {
            Log.Error("Erro ao baixar expectativas BCB: {Message}", ex.Message);
            return [];
        }
        catch (JsonException ex)
        {
            Log.Error("Erro ao parsear JSON do BCB: {Message}", ex.Message);
            return [];
        }
    }

    /// <summary>
    /// Baixa expectativas para um periodo de datas
    /// </summary>
    public async Task<List<BcbExpectativa>> FetchExpectativasPeriodoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        string indicador = "IPCA")
    {
        var dataInicioFormatada = dataInicio.ToString("yyyy-MM-dd");
        var dataFimFormatada = dataFim.ToString("yyyy-MM-dd");

        var url = $"{BaseUrl}/ExpectativasMercadoTop5Mensais?" +
                  $"$filter=Indicador%20eq%20'{indicador}'%20and%20Data%20ge%20'{dataInicioFormatada}'%20and%20Data%20le%20'{dataFimFormatada}'" +
                  $"&$format=json" +
                  $"&$select=Indicador,Data,DataReferencia,tipoCalculo,Media,Mediana,DesvioPadrao,Minimo,Maximo";

        Log.Information("Baixando expectativas BCB periodo: {DataInicio} a {DataFim}", dataInicioFormatada, dataFimFormatada);

        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Falha ao baixar expectativas BCB: {StatusCode}", response.StatusCode);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BcbODataResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Value == null || result.Value.Count == 0)
            {
                Log.Warning("Nenhuma expectativa encontrada para o periodo");
                return [];
            }

            // Remove duplicatas (mesma Data + Indicador + DataReferencia) e converte para lista
            var expectativas = result.Value
                .GroupBy(item => new { item.Data, item.Indicador, item.DataReferencia })
                .Select(g => g.First())
                .Select(item => new BcbExpectativa
            {
                Indicador = item.Indicador ?? indicador,
                Data = DateTime.Parse(item.Data ?? dataInicioFormatada),
                DataReferencia = item.DataReferencia ?? string.Empty,
                TipoCalculo = item.TipoCalculo,
                Media = ParseDecimal(item.Media),
                Mediana = ParseDecimal(item.Mediana),
                DesvioPadrao = ParseDecimal(item.DesvioPadrao),
                Minimo = ParseDecimal(item.Minimo),
                Maximo = ParseDecimal(item.Maximo),
                ImportedAt = DateTime.UtcNow
            }).ToList();

            Log.Information("{Count} expectativas baixadas para o periodo", expectativas.Count);
            return expectativas;
        }
        catch (HttpRequestException ex)
        {
            Log.Error("Erro ao baixar expectativas BCB: {Message}", ex.Message);
            return [];
        }
        catch (JsonException ex)
        {
            Log.Error("Erro ao parsear JSON do BCB: {Message}", ex.Message);
            return [];
        }
    }

    private static decimal? ParseDecimal(object? value)
    {
        if (value == null) return null;

        return value switch
        {
            JsonElement element => element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(element.GetString(), out var d) => d,
                _ => null
            },
            decimal d => d,
            double d => (decimal)d,
            string s when decimal.TryParse(s, out var d) => d,
            _ => null
        };
    }

    // Classes para deserializacao do JSON OData
    private class BcbODataResponse
    {
        public List<BcbExpectativaItem>? Value { get; set; }
    }

    private class BcbExpectativaItem
    {
        public string? Indicador { get; set; }
        public string? Data { get; set; }
        public string? DataReferencia { get; set; }
        public string? TipoCalculo { get; set; }
        public object? Media { get; set; }
        public object? Mediana { get; set; }
        public object? DesvioPadrao { get; set; }
        public object? Minimo { get; set; }
        public object? Maximo { get; set; }
    }
}
