using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.DTOs.YieldCurve;

namespace ArxFlow.Server.Endpoints;

public static class YieldCurveEndpoints
{
    public static void MapYieldCurveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/yield-curve").WithTags("YieldCurve");

        // GET /api/yield-curve/dates - Lista datas disponíveis
        group.MapGet("/dates", async (ApplicationDbContext db) =>
        {
            // Por enquanto retorna datas de exemplo
            var dates = new List<DateTime>
            {
                DateTime.Today,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(-7),
                DateTime.Today.AddDays(-30),
            };

            return Results.Ok(dates);
        })
        .WithName("GetYieldCurveDates")
        .WithOpenApi();

        // GET /api/yield-curve - Obtém curva de juros
        group.MapGet("/", (DateTime? date, string? curveType, string? interpolation) =>
        {
            var targetDate = date ?? DateTime.Today;
            var type = curveType ?? "DI1";
            var interp = interpolation ?? "Linear";

            // Dados de exemplo - simula curva DI1
            var points = new List<YieldCurvePoint>
            {
                new() { Days = 21, Rate = 10.65m, Maturity = targetDate.AddDays(21) },
                new() { Days = 42, Rate = 10.70m, Maturity = targetDate.AddDays(42) },
                new() { Days = 63, Rate = 10.75m, Maturity = targetDate.AddDays(63) },
                new() { Days = 126, Rate = 10.85m, Maturity = targetDate.AddDays(126) },
                new() { Days = 252, Rate = 11.00m, Maturity = targetDate.AddDays(252) },
                new() { Days = 504, Rate = 11.25m, Maturity = targetDate.AddDays(504) },
                new() { Days = 756, Rate = 11.50m, Maturity = targetDate.AddDays(756) },
                new() { Days = 1260, Rate = 11.75m, Maturity = targetDate.AddDays(1260) },
            };

            var response = new YieldCurveResponse
            {
                Date = targetDate,
                CurveType = type,
                Points = points
            };

            return Results.Ok(response);
        })
        .WithName("GetYieldCurve")
        .WithOpenApi();
    }
}
