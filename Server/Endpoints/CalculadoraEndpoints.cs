using ArxFlow.Server.DTOs.Calculadora;
using ArxFlow.Server.Services;
using ArxFlow.Server.Utils;

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
            var diasUteis = BrazilianCalendar.BusinessDaysBetween(
                request.DataCotacao,
                request.DataVencimento,
                BrazilianCalendarType.Settlement
            );

            var pu = BondCalculator.LTN_CalcularPU(
                request.TaxaAno,
                request.DataCotacao,
                request.DataVencimento
            );

            return new CalculatePUResponse
            {
                PU = pu,
                TaxaAno = request.TaxaAno,
                DiasUteis = diasUteis,
                DiasCorridos = diasCorridos
            };
        })
        .Produces<CalculatePUResponse>()
        .WithName("CalculateLTNPU")
        .WithOpenApi();

        // LTN - Calcular Taxa
        group.MapPost("/ltn/taxa", (CalculateLTNTaxaRequest request) =>
        {
            var diasCorridos = (request.DataVencimento - request.DataCotacao).Days;
            var diasUteis = BrazilianCalendar.BusinessDaysBetween(
                request.DataCotacao,
                request.DataVencimento,
                BrazilianCalendarType.Settlement
            );

            var taxa = BondCalculator.LTN_CalcularTaxa(
                request.PU,
                request.DataCotacao,
                request.DataVencimento
            );

            return new CalculateTaxaResponse
            {
                TaxaAno = taxa,
                PU = request.PU,
                DiasUteis = diasUteis,
                DiasCorridos = diasCorridos
            };
        })
        .Produces<CalculateTaxaResponse>()
        .WithName("CalculateLTNTaxa")
        .WithOpenApi();
    }
}
