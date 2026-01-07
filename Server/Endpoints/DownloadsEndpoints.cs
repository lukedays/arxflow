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
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;
            var allPrecos = new List<B3PrecoDerivativo>();
            var date = endDate;
            var processed = 0;
            var errors = new List<string>();

            while (date >= startDate)
            {
                try
                {
                    var precos = await service.FetchAndParseAsync(
                        B3DownloadService.FormatDateCode(date),
                        null,
                        1
                    );

                    if (precos.Count > 0)
                    {
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

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Settlement);
            }

            return new DownloadResponse
            {
                Success = allPrecos.Count > 0,
                Message = allPrecos.Count > 0
                    ? $"Download de precos B3 concluido ({processed} dias)"
                    : "Nenhum preco encontrado",
                RecordsProcessed = allPrecos.Count,
                Errors = errors.Count > 0 ? errors : null
            };
        })
        .Produces<DownloadResponse>()
        .WithName("DownloadB3Precos")
        .WithOpenApi();

        // POST /api/downloads/b3/instrumentos - Download instrumentos B3
        group.MapPost("/b3/instrumentos", async (DownloadRequest request, B3InstrumentsService service, ApplicationDbContext db) =>
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;
            var totalInstrumentos = 0;
            var date = endDate;
            var processed = 0;
            var errors = new List<string>();

            while (date >= startDate)
            {
                try
                {
                    var instrumentos = await service.FetchInstrumentsAsync(date);

                    if (instrumentos.Count > 0)
                    {
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

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Settlement);
            }

            return new DownloadResponse
            {
                Success = totalInstrumentos > 0,
                Message = totalInstrumentos > 0
                    ? $"Download de instrumentos B3 concluido ({processed} dias)"
                    : "Nenhum instrumento encontrado",
                RecordsProcessed = totalInstrumentos,
                Errors = errors.Count > 0 ? errors : null
            };
        })
        .Produces<DownloadResponse>()
        .WithName("DownloadB3Instrumentos")
        .WithOpenApi();

        // POST /api/downloads/b3/rendafixa - Download renda fixa B3
        group.MapPost("/b3/rendafixa", async (DownloadRequest request, B3RendaFixaService service, ApplicationDbContext db) =>
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;
            var totalInstrumentos = 0;
            var date = endDate;
            var processed = 0;
            var errors = new List<string>();

            while (date >= startDate)
            {
                try
                {
                    var instrumentos = await service.FetchInstrumentsAsync(date);

                    if (instrumentos.Count > 0)
                    {
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

                date = BrazilianCalendar.PreviousBusinessDay(date, BrazilianCalendarType.Settlement);
            }

            return new DownloadResponse
            {
                Success = totalInstrumentos > 0,
                Message = totalInstrumentos > 0
                    ? $"Download de renda fixa B3 concluido ({processed} dias)"
                    : "Nenhum instrumento de renda fixa encontrado",
                RecordsProcessed = totalInstrumentos,
                Errors = errors.Count > 0 ? errors : null
            };
        })
        .Produces<DownloadResponse>()
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
                    var datas = expectativas.Select(e => e.Data.Date).Distinct().ToList();
                    var existentes = await db.BcbExpectativas
                        .Where(e => datas.Contains(e.Data))
                        .ToListAsync();
                    db.BcbExpectativas.RemoveRange(existentes);

                    db.BcbExpectativas.AddRange(expectativas);
                    await db.SaveChangesAsync();
                }

                return new DownloadResponse
                {
                    Success = expectativas.Count > 0,
                    Message = expectativas.Count > 0
                        ? "Download de expectativas BCB concluido"
                        : "Nenhuma expectativa encontrada",
                    RecordsProcessed = expectativas.Count
                };
            }
            catch (Exception ex)
            {
                return new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar expectativas BCB",
                    Errors = [ex.Message]
                };
            }
        })
        .Produces<DownloadResponse>()
        .WithName("DownloadBcbExpectativas")
        .WithOpenApi();

        // POST /api/downloads/anbima/tpf - Download TPF ANBIMA
        group.MapPost("/anbima/tpf", async (DownloadRequest request, AnbimaDownloadService service, ApplicationDbContext db) =>
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;
            var totalTpfs = 0;
            var date = endDate;
            var processed = 0;
            var errors = new List<string>();

            while (date >= startDate)
            {
                try
                {
                    var tpfs = await service.FetchTPFAsync(date);

                    if (tpfs.Count > 0)
                    {
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

            return new DownloadResponse
            {
                Success = totalTpfs > 0,
                Message = totalTpfs > 0
                    ? $"Download de TPF ANBIMA concluido ({processed} dias)"
                    : "Nenhum TPF encontrado",
                RecordsProcessed = totalTpfs,
                Errors = errors.Count > 0 ? errors : null
            };
        })
        .Produces<DownloadResponse>()
        .WithName("DownloadAnbimaTpf")
        .WithOpenApi();

        // POST /api/downloads/anbima/vna - Download VNA ANBIMA
        group.MapPost("/anbima/vna", async (DownloadRequest request, AnbimaVNADownloadService service, ApplicationDbContext db) =>
        {
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;
            var totalVnas = 0;
            var date = endDate;
            var processed = 0;
            var errors = new List<string>();

            while (date >= startDate)
            {
                try
                {
                    var vnas = await service.FetchVNAAsync(date);

                    if (vnas.Count > 0)
                    {
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

            return new DownloadResponse
            {
                Success = totalVnas > 0,
                Message = totalVnas > 0
                    ? $"Download de VNA ANBIMA concluido ({processed} dias)"
                    : "Nenhum VNA encontrado",
                RecordsProcessed = totalVnas,
                Errors = errors.Count > 0 ? errors : null
            };
        })
        .Produces<DownloadResponse>()
        .WithName("DownloadAnbimaVna")
        .WithOpenApi();
    }
}
