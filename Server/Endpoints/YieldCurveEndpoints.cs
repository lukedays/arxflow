using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.DTOs.YieldCurve;
using ArxFlow.Server.Utils;

namespace ArxFlow.Server.Endpoints;

public static class YieldCurveEndpoints
{
    public static void MapYieldCurveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/yield-curve").WithTags("YieldCurve");

        // GET /api/yield-curve/dates - Lista datas disponiveis
        group.MapGet("/dates", async (ApplicationDbContext db) =>
        {
            var dates = await db.B3PrecosDerivativos
                .Select(p => p.DataReferencia.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .Take(30)
                .ToListAsync();

            return dates;
        })
        .Produces<List<DateTime>>()
        .WithName("GetYieldCurveDates")
        .WithOpenApi();

        // GET /api/yield-curve - Obtem curva de juros
        group.MapGet("/", async (DateTime? date, string? curveType, string? interpolation, ApplicationDbContext db) =>
        {
            // Usa UTC para evitar problemas de timezone - a data vem como ISO UTC do frontend
            var targetDate = date.HasValue
                ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Unspecified)
                : DateTime.Today;
            var type = curveType ?? "DI1";
            var interp = interpolation ?? "FlatForward";

            // Busca precos e instrumentos do BD para a data especificada
            var precos = await db.B3PrecosDerivativos
                .Where(p => p.DataReferencia.Date == targetDate && p.Ticker.StartsWith(type))
                .ToListAsync();

            // Se nao encontrou na data, busca a data mais recente
            if (precos.Count == 0)
            {
                var ultimaData = await db.B3PrecosDerivativos
                    .Where(p => p.Ticker.StartsWith(type))
                    .OrderByDescending(p => p.DataReferencia)
                    .Select(p => p.DataReferencia.Date)
                    .FirstOrDefaultAsync();

                if (ultimaData != default)
                {
                    targetDate = ultimaData;
                    precos = await db.B3PrecosDerivativos
                        .Where(p => p.DataReferencia.Date == targetDate && p.Ticker.StartsWith(type))
                        .ToListAsync();
                }
            }

            // Busca instrumentos para obter dias uteis
            var instrumentos = await db.B3InstrumentosDerivativos
                .Where(i => i.DataReferencia.Date == targetDate && i.InstrumentoFinanceiro.StartsWith(type))
                .ToListAsync();

            // Monta pontos da curva
            var points = new List<YieldCurvePoint>();

            foreach (var preco in precos.OrderBy(p => p.Ticker))
            {
                var instrumento = instrumentos.FirstOrDefault(i => i.InstrumentoFinanceiro == preco.Ticker);

                // Pula se nao tem instrumento ou dias uteis
                if (instrumento == null || instrumento.DiasUteis == null || instrumento.DiasUteis <= 0)
                    continue;

                // Pula se nao tem taxa
                if (preco.TaxaAjuste == null)
                    continue;

                var diasUteis = instrumento.DiasUteis.Value;
                var dataVencimento = instrumento.DataVencimento ?? targetDate.AddDays(diasUteis);

                points.Add(new YieldCurvePoint
                {
                    Days = diasUteis,
                    Rate = preco.TaxaAjuste.Value,
                    Maturity = dataVencimento,
                    Ticker = preco.Ticker
                });
            }

            // Ordena por dias uteis
            points = points.OrderBy(p => p.Days).ToList();

            // Aplica interpolacao se solicitado
            if (points.Count >= 2 && interp != "None")
            {
                var interpType = interp == "Linear"
                    ? YieldCurveInterpolation.InterpolationType.Linear
                    : YieldCurveInterpolation.InterpolationType.FlatForward;

                points = YieldCurveInterpolation.GenerateInterpolatedCurve(points, targetDate, interpType);
            }

            return new YieldCurveResponse
            {
                Date = targetDate,
                CurveType = type,
                Points = points
            };
        })
        .Produces<YieldCurveResponse>()
        .WithName("GetYieldCurve")
        .WithOpenApi();
    }
}
