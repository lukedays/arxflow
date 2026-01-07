using System.Globalization;
using System.Text;
using ArxFlow.Server.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;

namespace ArxFlow.Server.Services;

// Servico para baixar dados de VNA (Valor Nominal Atualizado) da ANBIMA
public class AnbimaVNADownloadService(HttpClient httpClient)
{
    // Registra provider de encoding para Windows-1252
    static AnbimaVNADownloadService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    // URL para download do VNA
    private const string VnaUrl = "https://www.anbima.com.br/informacoes/vna/vna-down.asp";

    /// <summary>
    /// Baixa dados de VNA da ANBIMA para uma data especifica
    /// </summary>
    /// <param name="dataReferencia">Data de referencia do VNA</param>
    /// <returns>Lista de VNAs</returns>
    public async Task<List<AnbimaVNA>> FetchVNAAsync(DateTime dataReferencia)
    {
        try
        {
            // Prepara parametros do POST
            // Data=02012026&escolha=2&Idioma=PT&saida=txt&Dt_Ref_Ver=20251226&Inicio=02/01/2026
            var parametros = new Dictionary<string, string>
            {
                { "Data", dataReferencia.ToString("ddMMyyyy") },
                { "escolha", "2" },
                { "Idioma", "PT" },
                { "saida", "txt" },
                { "Dt_Ref_Ver", dataReferencia.ToString("yyyyMMdd") },
                { "Inicio", dataReferencia.ToString("dd/MM/yyyy") }
            };

            Log.Information("Baixando VNA ANBIMA para {Data}", dataReferencia.ToString("dd/MM/yyyy"));

            var content = new FormUrlEncodedContent(parametros);
            var response = await httpClient.PostAsync(VnaUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Falha ao baixar VNA ANBIMA: {StatusCode}", response.StatusCode);
                return [];
            }

            // Arquivo vem em encoding Windows-1252 (Latin-1)
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var texto = Encoding.GetEncoding("Windows-1252").GetString(bytes);

            return ParseVNAFile(texto, dataReferencia);
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Erro HTTP ao baixar VNA ANBIMA");
            return [];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao baixar VNA ANBIMA");
            return [];
        }
    }

    /// <summary>
    /// Classe para mapeamento do CSV de VNA
    /// </summary>
    private class VnaCsvRecord
    {
        public string Titulo { get; set; } = string.Empty;
        public string? CodigoSelic { get; set; }
        public string VnaString { get; set; } = string.Empty;
        public string? IndiceString { get; set; }
        public string? Ref { get; set; }
        public string? ValidoAPartirDeString { get; set; }
    }

    /// <summary>
    /// Configuração do mapeamento CSV para VNA
    /// </summary>
    private class VnaCsvMap : ClassMap<VnaCsvRecord>
    {
        public VnaCsvMap()
        {
            Map(m => m.Titulo).Index(0).Name("Titulo", "Título");
            Map(m => m.CodigoSelic).Index(1).Name("Código Selic", "Codigo Selic").Optional();
            Map(m => m.VnaString).Index(2).Name("VNA");
            Map(m => m.IndiceString).Index(3).Name("Índice", "Indice").Optional();
            Map(m => m.Ref).Index(4).Name("Ref").Optional();
            Map(m => m.ValidoAPartirDeString).Index(5).Name("Válido a partir de", "Valido a partir de").Optional();
        }
    }

    /// <summary>
    /// Faz parse do arquivo TXT de VNA da ANBIMA usando CsvHelper
    /// Formato esperado (separado por ;):
    /// Titulo;Código Selic;VNA;Índice;Ref;Válido a partir de
    /// Exemplo: NTN-B;760199;4.578,950958;0,34;P;24/12/2025
    /// </summary>
    private static List<AnbimaVNA> ParseVNAFile(string conteudo, DateTime dataReferencia)
    {
        var vnas = new List<AnbimaVNA>();

        try
        {
            // Procura linha do header - contem "Titulo" e "VNA" separados por ;
            var linhas = conteudo.Split('\n');
            var headerIndex = -1;

            for (int i = 0; i < linhas.Length; i++)
            {
                var linha = linhas[i].Trim();
                // Busca por linha que comeca com Titulo (com ou sem acento) e contem ;VNA;
                if ((linha.StartsWith("Titulo", StringComparison.OrdinalIgnoreCase) ||
                     linha.StartsWith("Título", StringComparison.OrdinalIgnoreCase)) &&
                    linha.Contains(";VNA;", StringComparison.OrdinalIgnoreCase))
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex == -1)
            {
                Log.Warning("Header nao encontrado no arquivo VNA. Primeiras linhas: {Linhas}",
                    string.Join(" | ", linhas.Take(10).Select(l => l.Trim())));
                return vnas;
            }

            // Junta apenas as linhas a partir do header
            var csvContent = string.Join("\n", linhas.Skip(headerIndex));

            using var reader = new StringReader(csvContent);
            var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pt-BR"))
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<VnaCsvMap>();

            var records = csv.GetRecords<VnaCsvRecord>();

            foreach (var record in records)
            {
                try
                {
                    // Parse VNA (obrigatório)
                    if (!decimal.TryParse(record.VnaString, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out var vna))
                    {
                        Log.Warning("VNA inválido para título {Titulo}: {VNA}", record.Titulo, record.VnaString);
                        continue;
                    }

                    // Parse Índice (opcional)
                    decimal? indice = null;
                    if (!string.IsNullOrWhiteSpace(record.IndiceString) &&
                        decimal.TryParse(record.IndiceString, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out var indiceValue))
                    {
                        indice = indiceValue;
                    }

                    // Parse data válido a partir de (opcional)
                    DateTime? validoAPartirDe = null;
                    if (!string.IsNullOrWhiteSpace(record.ValidoAPartirDeString) &&
                        DateTime.TryParseExact(record.ValidoAPartirDeString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataValida))
                    {
                        validoAPartirDe = dataValida;
                    }

                    vnas.Add(new AnbimaVNA
                    {
                        Titulo = record.Titulo,
                        CodigoSelic = string.IsNullOrWhiteSpace(record.CodigoSelic) ? null : record.CodigoSelic,
                        DataReferencia = dataReferencia.Date,
                        VNA = vna,
                        Indice = indice,
                        Referencia = string.IsNullOrWhiteSpace(record.Ref) ? null : record.Ref,
                        ValidoAPartirDe = validoAPartirDe,
                        ImportedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Erro ao processar record VNA: {Titulo}", record.Titulo);
                }
            }

            Log.Information("VNAs parseados: {Count}", vnas.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao parsear arquivo VNA com CsvHelper");
        }

        return vnas;
    }
}
