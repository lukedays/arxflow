using ArxFlow.Server.DTOs.Downloads;
using ArxFlow.Server.Services;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;
using ArxFlow.Server.Utils;
using Microsoft.EntityFrameworkCore;

namespace ArxFlow.Server.Endpoints;

public static class DownloadsEndpoints
{
    public static void MapDownloadsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/downloads").WithTags("Downloads");

        // POST /api/downloads/b3/precos - Download precos B3
        group.MapPost("/b3/precos", async (DownloadRequest request, B3DownloadService service, ApplicationDbContext db) =>
        {
            var days = Math.Max(1, (request.EndDate - request.StartDate).Days + 1);
            var allPrecos = new List<B3PrecoDerivativo>();
            var date = request.EndDate;
            var processed = 0;
            var errors = new List<string>();

            // Itera por dias uteis, tentando proxima data se falhar
            for (int attempts = 0; processed < days && attempts < days * 3; attempts++)
            {
                // Pula fins de semana
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(-1);
                    continue;
                }

                try
                {
                    var precos = await service.FetchAndParseAsync(
                        B3DownloadService.FormatDateCode(date),
                        null,
                        1 // maxRetries = 1, pois vamos iterar manualmente
                    );

                    if (precos.Count > 0)
                    {
                        // Remove existentes da mesma data
                        var dataRef = date.Date;
                        var existentes = await db.B3PrecosDerivativos
                            .Where(p => p.DataReferencia.Date == dataRef)
                            .ToListAsync();
                        db.B3PrecosDerivativos.RemoveRange(existentes);

                        db.B3PrecosDerivativos.AddRange(precos);
                        await db.SaveChangesAsync();

                        allPrecos.AddRange(precos);
                        processed++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{date:dd/MM/yyyy}: {ex.Message}");
                }

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Exchange);
            }

            return Results.Ok(new DownloadResponse
            {
                Success = allPrecos.Count > 0,
                Message = allPrecos.Count > 0
                    ? $"Download de precos B3 concluido ({processed} dias)"
                    : "Nenhum preco encontrado",
                RecordsProcessed = allPrecos.Count,
                Errors = errors.Count > 0 ? errors : null
            });
        })
        .WithName("DownloadB3Precos")
        .WithOpenApi();

        // POST /api/downloads/b3/instrumentos - Download instrumentos B3
        group.MapPost("/b3/instrumentos", async (DownloadRequest request, B3InstrumentsService service, ApplicationDbContext db) =>
        {
            var days = Math.Max(1, (request.EndDate - request.StartDate).Days + 1);
            var totalInstrumentos = 0;
            var date = request.EndDate;
            var processed = 0;
            var errors = new List<string>();

            for (int attempts = 0; processed < days && attempts < days * 3; attempts++)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(-1);
                    continue;
                }

                try
                {
                    var instrumentos = await service.FetchInstrumentsAsync(date);

                    if (instrumentos.Count > 0)
                    {
                        // Remove existentes da mesma data
                        var existentes = await db.B3InstrumentosDerivativos
                            .Where(i => i.DataReferencia == date.Date)
                            .ToListAsync();
                        db.B3InstrumentosDerivativos.RemoveRange(existentes);

                        db.B3InstrumentosDerivativos.AddRange(instrumentos);
                        await db.SaveChangesAsync();

                        totalInstrumentos += instrumentos.Count;
                        processed++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{date:dd/MM/yyyy}: {ex.Message}");
                }

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Exchange);
            }

            return Results.Ok(new DownloadResponse
            {
                Success = totalInstrumentos > 0,
                Message = totalInstrumentos > 0
                    ? $"Download de instrumentos B3 concluido ({processed} dias)"
                    : "Nenhum instrumento encontrado",
                RecordsProcessed = totalInstrumentos,
                Errors = errors.Count > 0 ? errors : null
            });
        })
        .WithName("DownloadB3Instrumentos")
        .WithOpenApi();

        // POST /api/downloads/b3/rendafixa - Download renda fixa B3
        group.MapPost("/b3/rendafixa", async (DownloadRequest request, B3RendaFixaService service, ApplicationDbContext db) =>
        {
            var days = Math.Max(1, (request.EndDate - request.StartDate).Days + 1);
            var totalInstrumentos = 0;
            var date = request.EndDate;
            var processed = 0;
            var errors = new List<string>();

            for (int attempts = 0; processed < days && attempts < days * 3; attempts++)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(-1);
                    continue;
                }

                try
                {
                    var instrumentos = await service.FetchInstrumentsAsync(date);

                    if (instrumentos.Count > 0)
                    {
                        // Remove existentes da mesma data
                        var existentes = await db.B3InstrumentosRendaFixa
                            .Where(i => i.DataReferencia == date.Date)
                            .ToListAsync();
                        db.B3InstrumentosRendaFixa.RemoveRange(existentes);

                        db.B3InstrumentosRendaFixa.AddRange(instrumentos);
                        await db.SaveChangesAsync();

                        totalInstrumentos += instrumentos.Count;
                        processed++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{date:dd/MM/yyyy}: {ex.Message}");
                }

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Exchange);
            }

            return Results.Ok(new DownloadResponse
            {
                Success = totalInstrumentos > 0,
                Message = totalInstrumentos > 0
                    ? $"Download de renda fixa B3 concluido ({processed} dias)"
                    : "Nenhum instrumento de renda fixa encontrado",
                RecordsProcessed = totalInstrumentos,
                Errors = errors.Count > 0 ? errors : null
            });
        })
        .WithName("DownloadB3RendaFixa")
        .WithOpenApi();

        // POST /api/downloads/bcb/expectativas - Download expectativas BCB
        group.MapPost("/bcb/expectativas", async (DownloadRequest request, BcbDownloadService service, ApplicationDbContext db) =>
        {
            try
            {
                var expectativas = await service.FetchExpectativasPeriodoAsync(request.StartDate, request.EndDate);

                if (expectativas.Count > 0)
                {
                    // Remove existentes do periodo
                    var datas = expectativas.Select(e => e.Data.Date).Distinct().ToList();
                    var existentes = await db.BcbExpectativas
                        .Where(e => datas.Contains(e.Data))
                        .ToListAsync();
                    db.BcbExpectativas.RemoveRange(existentes);

                    db.BcbExpectativas.AddRange(expectativas);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = expectativas.Count > 0,
                    Message = expectativas.Count > 0
                        ? "Download de expectativas BCB concluido"
                        : "Nenhuma expectativa encontrada",
                    RecordsProcessed = expectativas.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar expectativas BCB",
                    Errors = new List<string> { ex.Message }
                });
            }
        })
        .WithName("DownloadBcbExpectativas")
        .WithOpenApi();

        // POST /api/downloads/anbima/tpf - Download TPF ANBIMA
        group.MapPost("/anbima/tpf", async (DownloadRequest request, AnbimaDownloadService service, ApplicationDbContext db) =>
        {
            var days = Math.Max(1, (request.EndDate - request.StartDate).Days + 1);
            var totalTpfs = 0;
            var date = request.EndDate;
            var processed = 0;
            var errors = new List<string>();

            for (int attempts = 0; processed < days && attempts < days * 3; attempts++)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(-1);
                    continue;
                }

                try
                {
                    var tpfs = await service.FetchTPFAsync(date);

                    if (tpfs.Count > 0)
                    {
                        // Remove existentes da mesma data
                        var existentes = await db.AnbimaTPFs
                            .Where(t => t.DataReferencia == date.Date)
                            .ToListAsync();
                        db.AnbimaTPFs.RemoveRange(existentes);

                        db.AnbimaTPFs.AddRange(tpfs);
                        await db.SaveChangesAsync();

                        totalTpfs += tpfs.Count;
                        processed++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{date:dd/MM/yyyy}: {ex.Message}");
                }

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Settlement);
            }

            return Results.Ok(new DownloadResponse
            {
                Success = totalTpfs > 0,
                Message = totalTpfs > 0
                    ? $"Download de TPF ANBIMA concluido ({processed} dias)"
                    : "Nenhum TPF encontrado",
                RecordsProcessed = totalTpfs,
                Errors = errors.Count > 0 ? errors : null
            });
        })
        .WithName("DownloadAnbimaTpf")
        .WithOpenApi();

        // POST /api/downloads/anbima/vna - Download VNA ANBIMA
        group.MapPost("/anbima/vna", async (DownloadRequest request, AnbimaVNADownloadService service, ApplicationDbContext db) =>
        {
            var days = Math.Max(1, (request.EndDate - request.StartDate).Days + 1);
            var totalVnas = 0;
            var date = request.EndDate;
            var processed = 0;
            var errors = new List<string>();

            for (int attempts = 0; processed < days && attempts < days * 3; attempts++)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(-1);
                    continue;
                }

                try
                {
                    var vnas = await service.FetchVNAAsync(date);

                    if (vnas.Count > 0)
                    {
                        // Remove existentes da mesma data
                        var existentes = await db.AnbimaVNAs
                            .Where(v => v.DataReferencia == date.Date)
                            .ToListAsync();
                        db.AnbimaVNAs.RemoveRange(existentes);

                        db.AnbimaVNAs.AddRange(vnas);
                        await db.SaveChangesAsync();

                        totalVnas += vnas.Count;
                        processed++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{date:dd/MM/yyyy}: {ex.Message}");
                }

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Settlement);
            }

            return Results.Ok(new DownloadResponse
            {
                Success = totalVnas > 0,
                Message = totalVnas > 0
                    ? $"Download de VNA ANBIMA concluido ({processed} dias)"
                    : "Nenhum VNA encontrado",
                RecordsProcessed = totalVnas,
                Errors = errors.Count > 0 ? errors : null
            });
        })
        .WithName("DownloadAnbimaVna")
        .WithOpenApi();
    }
}
