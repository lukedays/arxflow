using ArxFlow.Server.DTOs.Calculadora;
using ArxFlow.Server.Services;

namespace ArxFlow.Server.Endpoints;

public static class CalculadoraEndpoints
{
    public static void MapCalculadoraEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/calculadora").WithTags("Calculadora");

        // LTN - Calcular PU
        group.MapPost("/ltn/pu", (CalculateLTNPURequest request) =>
        {
            var diasCorridos = (request.DataVencimento - request.DataCotacao).Days;

            var pu = BondCalculator.LTN_CalcularPU(
                request.TaxaAno,
                request.DataCotacao,
                request.DataVencimento
            );

            // Calcula dias úteis manualmente (simplificado)
            var diasUteis = diasCorridos * 252 / 365;

            return Results.Ok(new CalculatePUResponse
            {
                PU = pu,
                TaxaAno = request.TaxaAno,
                DiasUteis = diasUteis,
                DiasCorridos = diasCorridos
            });
        })
        .WithName("CalculateLTNPU")
        .WithOpenApi();

        // LTN - Calcular Taxa
        group.MapPost("/ltn/taxa", (CalculateLTNTaxaRequest request) =>
        {
            var diasCorridos = (request.DataVencimento - request.DataCotacao).Days;

            var taxa = BondCalculator.LTN_CalcularTaxa(
                request.PU,
                request.DataCotacao,
                request.DataVencimento
            );

            // Calcula dias úteis manualmente (simplificado)
            var diasUteis = diasCorridos * 252 / 365;

            return Results.Ok(new CalculateTaxaResponse
            {
                TaxaAno = taxa,
                PU = request.PU,
                DiasUteis = diasUteis,
                DiasCorridos = diasCorridos
            });
        })
        .WithName("CalculateLTNTaxa")
        .WithOpenApi();
    }
}
