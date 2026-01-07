using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.DTOs.Calculadora;
using ArxFlow.Server.Services;
using ArxFlow.Server.Utils;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Endpoints;

public static class ValidacaoEndpoints
{
    public static void MapValidacaoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/validacao").WithTags("Validacao");

        // POST /api/validacao/calculadoras - Executa validacao de calculadoras
        group.MapPost("/calculadoras", async (ValidacaoRequest request, ApplicationDbContext db) =>
        {
            var response = new ValidacaoResponse();

            try
            {
                // 1. Busca as datas com dados ANBIMA disponiveis
                var datasParaValidar = await BuscarDatasComDados(db, request.DiasUteis);

                if (datasParaValidar.Count == 0)
                {
                    response.Erros.Add("Nenhuma data com dados ANBIMA encontrada");
                    return response;
                }

                // 2. Busca todos os titulos e VNAs para essas datas
                var titulos = await db.AnbimaTPFs
                    .Where(t => datasParaValidar.Contains(t.DataReferencia))
                    .ToListAsync();

                var vnas = await db.AnbimaVNAs
                    .Where(v => datasParaValidar.Contains(v.DataReferencia))
                    .ToListAsync();

                // 3. Valida cada tipo de titulo
                ValidarLTN(titulos.Where(t => t.Titulo.StartsWith("LTN")).ToList(), response);
                ValidarNTNF(titulos.Where(t => t.Titulo.StartsWith("NTN-F")).ToList(), response);
                ValidarNTNB(titulos.Where(t => t.Titulo.StartsWith("NTN-B")).ToList(), vnas, response);
                ValidarNTNC(titulos.Where(t => t.Titulo.StartsWith("NTN-C")).ToList(), vnas, response);
                ValidarLFT(titulos.Where(t => t.Titulo.StartsWith("LFT")).ToList(), vnas, response);

                // 4. Ordena resultados
                response.Resultados = response.Resultados
                    .OrderByDescending(r => r.DataReferencia)
                    .ThenBy(r => r.DataVencimento)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Erros.Add($"Erro geral: {ex.Message}");
            }

            return response;
        })
        .Produces<ValidacaoResponse>()
        .WithName("ValidarCalculadoras")
        .WithOpenApi();
    }

    private static async Task<List<DateTime>> BuscarDatasComDados(ApplicationDbContext db, int diasUteis)
    {
        var datasParaValidar = new List<DateTime>();
        var data = DateTime.Today;
        var processed = 0;

        for (int attempts = 0; processed < diasUteis && attempts < diasUteis * 3; attempts++)
        {
            var temDados = await db.AnbimaTPFs.AnyAsync(t => t.DataReferencia.Date == data.Date);
            if (temDados)
            {
                datasParaValidar.Add(data.Date);
                processed++;
            }
            data = BrazilianCalendar.PreviousBusinessDay(data, BrazilianCalendarType.Settlement);
        }

        return datasParaValidar;
    }

    private static void ValidarLTN(List<AnbimaTPF> titulos, ValidacaoResponse response)
    {
        foreach (var titulo in titulos)
        {
            try
            {
                if (!titulo.TaxaIndicativa.HasValue || !titulo.DataVencimento.HasValue || !titulo.PU.HasValue)
                    continue;

                var puCalc = BondCalculator.LTN_CalcularPU(titulo.TaxaIndicativa.Value, titulo.DataReferencia, titulo.DataVencimento.Value);
                var taxaCalc = BondCalculator.LTN_CalcularTaxa(titulo.PU.Value, titulo.DataReferencia, titulo.DataVencimento.Value);

                var diffPU = Math.Abs(titulo.PU.Value - puCalc);
                var diffTaxa = Math.Abs(titulo.TaxaIndicativa.Value - taxaCalc);
                var ok = diffPU < 0.000001m && diffTaxa < 0.0001m;

                if (ok) response.Resumo.LtnOK++; else response.Resumo.LtnFalhou++;

                response.Resultados.Add(new ResultadoValidacao
                {
                    Titulo = "LTN",
                    DataReferencia = titulo.DataReferencia,
                    DataVencimento = titulo.DataVencimento.Value,
                    TaxaMercado = titulo.TaxaIndicativa.Value,
                    TaxaCalculada = taxaCalc,
                    PUMercado = titulo.PU.Value,
                    PUCalculado = puCalc,
                    DiferenciaPU = diffPU,
                    DiferencaTaxa = diffTaxa,
                    OK = ok
                });
            }
            catch (Exception ex)
            {
                response.Erros.Add($"LTN {titulo.DataReferencia:dd/MM/yyyy} -> {titulo.DataVencimento?.ToString("dd/MM/yyyy")}: {ex.Message}");
            }
        }
    }

    private static void ValidarNTNF(List<AnbimaTPF> titulos, ValidacaoResponse response)
    {
        foreach (var titulo in titulos)
        {
            try
            {
                if (!titulo.TaxaIndicativa.HasValue || !titulo.DataVencimento.HasValue || !titulo.PU.HasValue)
                    continue;

                var puCalc = BondCalculator.NTNF_CalcularPU(titulo.TaxaIndicativa.Value, titulo.DataReferencia, titulo.DataVencimento.Value);
                var taxaCalc = BondCalculator.NTNF_CalcularTaxa(titulo.PU.Value, titulo.DataReferencia, titulo.DataVencimento.Value);

                var diffPU = Math.Abs(titulo.PU.Value - puCalc);
                var diffTaxa = Math.Abs(titulo.TaxaIndicativa.Value - taxaCalc);
                var ok = diffPU < 0.000001m && diffTaxa < 0.0001m;

                if (ok) response.Resumo.NtnfOK++; else response.Resumo.NtnfFalhou++;

                response.Resultados.Add(new ResultadoValidacao
                {
                    Titulo = "NTN-F",
                    DataReferencia = titulo.DataReferencia,
                    DataVencimento = titulo.DataVencimento.Value,
                    TaxaMercado = titulo.TaxaIndicativa.Value,
                    TaxaCalculada = taxaCalc,
                    PUMercado = titulo.PU.Value,
                    PUCalculado = puCalc,
                    DiferenciaPU = diffPU,
                    DiferencaTaxa = diffTaxa,
                    OK = ok
                });
            }
            catch (Exception ex)
            {
                response.Erros.Add($"NTN-F {titulo.DataReferencia:dd/MM/yyyy} -> {titulo.DataVencimento?.ToString("dd/MM/yyyy")}: {ex.Message}");
            }
        }
    }

    private static void ValidarNTNB(List<AnbimaTPF> titulos, List<AnbimaVNA> vnas, ValidacaoResponse response)
    {
        foreach (var titulo in titulos)
        {
            try
            {
                if (!titulo.TaxaIndicativa.HasValue || !titulo.DataVencimento.HasValue || !titulo.PU.HasValue)
                    continue;

                var vna = vnas.FirstOrDefault(v => v.DataReferencia.Date == titulo.DataReferencia.Date && v.Titulo.StartsWith("NTN-B"));
                if (vna == null)
                {
                    response.Erros.Add($"NTN-B {titulo.DataReferencia:dd/MM/yyyy}: VNA nao encontrado");
                    continue;
                }

                var puCalc = BondCalculator.NTNB_CalcularPU(titulo.TaxaIndicativa.Value, vna.VNA, titulo.DataReferencia, titulo.DataVencimento.Value);
                var taxaCalc = BondCalculator.NTNB_CalcularTaxa(titulo.PU.Value, vna.VNA, titulo.DataReferencia, titulo.DataVencimento.Value);

                var diffPU = Math.Abs(titulo.PU.Value - puCalc);
                var diffTaxa = Math.Abs(titulo.TaxaIndicativa.Value - taxaCalc);
                var ok = diffPU < 0.000001m && diffTaxa < 0.0001m;

                if (ok) response.Resumo.NtnbOK++; else response.Resumo.NtnbFalhou++;

                response.Resultados.Add(new ResultadoValidacao
                {
                    Titulo = "NTN-B",
                    DataReferencia = titulo.DataReferencia,
                    DataVencimento = titulo.DataVencimento.Value,
                    TaxaMercado = titulo.TaxaIndicativa.Value,
                    TaxaCalculada = taxaCalc,
                    PUMercado = titulo.PU.Value,
                    PUCalculado = puCalc,
                    DiferenciaPU = diffPU,
                    DiferencaTaxa = diffTaxa,
                    VNA = vna.VNA,
                    OK = ok
                });
            }
            catch (Exception ex)
            {
                response.Erros.Add($"NTN-B {titulo.DataReferencia:dd/MM/yyyy} -> {titulo.DataVencimento?.ToString("dd/MM/yyyy")}: {ex.Message}");
            }
        }
    }

    private static void ValidarNTNC(List<AnbimaTPF> titulos, List<AnbimaVNA> vnas, ValidacaoResponse response)
    {
        foreach (var titulo in titulos)
        {
            try
            {
                if (!titulo.TaxaIndicativa.HasValue || !titulo.DataVencimento.HasValue || !titulo.PU.HasValue)
                    continue;

                var vna = vnas.FirstOrDefault(v => v.DataReferencia.Date == titulo.DataReferencia.Date && v.Titulo.StartsWith("NTN-C"));
                if (vna == null)
                {
                    response.Erros.Add($"NTN-C {titulo.DataReferencia:dd/MM/yyyy}: VNA nao encontrado");
                    continue;
                }

                var puCalc = BondCalculator.NTNC_CalcularPU(titulo.TaxaIndicativa.Value, vna.VNA, titulo.DataReferencia, titulo.DataVencimento.Value);
                var taxaCalc = BondCalculator.NTNC_CalcularTaxa(titulo.PU.Value, vna.VNA, titulo.DataReferencia, titulo.DataVencimento.Value);

                var diffPU = Math.Abs(titulo.PU.Value - puCalc);
                var diffTaxa = Math.Abs(titulo.TaxaIndicativa.Value - taxaCalc);
                var ok = diffPU < 0.000001m && diffTaxa < 0.0001m;

                if (ok) response.Resumo.NtncOK++; else response.Resumo.NtncFalhou++;

                response.Resultados.Add(new ResultadoValidacao
                {
                    Titulo = "NTN-C",
                    DataReferencia = titulo.DataReferencia,
                    DataVencimento = titulo.DataVencimento.Value,
                    TaxaMercado = titulo.TaxaIndicativa.Value,
                    TaxaCalculada = taxaCalc,
                    PUMercado = titulo.PU.Value,
                    PUCalculado = puCalc,
                    DiferenciaPU = diffPU,
                    DiferencaTaxa = diffTaxa,
                    VNA = vna.VNA,
                    OK = ok
                });
            }
            catch (Exception ex)
            {
                response.Erros.Add($"NTN-C {titulo.DataReferencia:dd/MM/yyyy} -> {titulo.DataVencimento?.ToString("dd/MM/yyyy")}: {ex.Message}");
            }
        }
    }

    private static void ValidarLFT(List<AnbimaTPF> titulos, List<AnbimaVNA> vnas, ValidacaoResponse response)
    {
        foreach (var titulo in titulos)
        {
            try
            {
                if (!titulo.TaxaIndicativa.HasValue || !titulo.DataVencimento.HasValue || !titulo.PU.HasValue)
                    continue;

                var vna = vnas.FirstOrDefault(v => v.DataReferencia.Date == titulo.DataReferencia.Date && v.Titulo.StartsWith("LFT"));
                if (vna == null)
                {
                    response.Erros.Add($"LFT {titulo.DataReferencia:dd/MM/yyyy}: VNA nao encontrado");
                    continue;
                }

                var puCalc = BondCalculator.LFT_CalcularPU(titulo.TaxaIndicativa.Value, vna.VNA, titulo.DataReferencia, titulo.DataVencimento.Value);
                var taxaCalc = BondCalculator.LFT_CalcularTaxa(titulo.PU.Value, vna.VNA, titulo.DataReferencia, titulo.DataVencimento.Value);

                var diffPU = Math.Abs(titulo.PU.Value - puCalc);
                var diffTaxa = Math.Abs(titulo.TaxaIndicativa.Value - taxaCalc);
                var ok = diffPU < 0.000001m && diffTaxa < 0.0001m;

                if (ok) response.Resumo.LftOK++; else response.Resumo.LftFalhou++;

                response.Resultados.Add(new ResultadoValidacao
                {
                    Titulo = "LFT",
                    DataReferencia = titulo.DataReferencia,
                    DataVencimento = titulo.DataVencimento.Value,
                    TaxaMercado = titulo.TaxaIndicativa.Value,
                    TaxaCalculada = taxaCalc,
                    PUMercado = titulo.PU.Value,
                    PUCalculado = puCalc,
                    DiferenciaPU = diffPU,
                    DiferencaTaxa = diffTaxa,
                    VNA = vna.VNA,
                    OK = ok
                });
            }
            catch (Exception ex)
            {
                response.Erros.Add($"LFT {titulo.DataReferencia:dd/MM/yyyy} -> {titulo.DataVencimento?.ToString("dd/MM/yyyy")}: {ex.Message}");
            }
        }
    }
}
