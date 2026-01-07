using ArxFlow.Server.DTOs.Downloads;
using ArxFlow.Server.Services;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Endpoints;

public static class DownloadsEndpoints
{
    public static void MapDownloadsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/downloads").WithTags("Downloads");

        // POST /api/downloads/b3/precos - Download preços B3
        group.MapPost("/b3/precos", async (DownloadRequest request, B3DownloadService service, ApplicationDbContext db) =>
        {
            try
            {
                // Calcula numero de dias entre as datas
                var days = (request.EndDate - request.StartDate).Days + 1;
                var prefixes = new[] { "INDXXXX", "WINXXXX", "DOLXXXX", "WDOXXXX", "DIFXXXX" };

                // Baixa historico de precos
                var precos = await service.FetchHistoryAsync(days, prefixes, request.EndDate);

                // Salva no banco usando bulk insert (se disponivel) ou AddRange
                if (precos.Count > 0)
                {
                    db.B3PrecosDerivativos.AddRange(precos);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = true,
                    Message = $"Download de preços B3 concluído",
                    RecordsProcessed = precos.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar download de preços B3",
                    Errors = new List<string> { ex.Message }
                });
            }
        })
        .WithName("DownloadB3Precos")
        .WithOpenApi();

        // POST /api/downloads/b3/instrumentos - Download instrumentos B3
        group.MapPost("/b3/instrumentos", async (DownloadRequest request, B3InstrumentsService service, ApplicationDbContext db) =>
        {
            try
            {
                // Baixa instrumentos para o periodo
                var instrumentos = await service.FetchInstrumentsAsync(request.StartDate, request.EndDate);

                // Salva no banco
                if (instrumentos.Count > 0)
                {
                    db.B3InstrumentosDerivativos.AddRange(instrumentos);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = true,
                    Message = "Download de instrumentos B3 concluído",
                    RecordsProcessed = instrumentos.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar instrumentos B3",
                    Errors = new List<string> { ex.Message }
                });
            }
        })
        .WithName("DownloadB3Instrumentos")
        .WithOpenApi();

        // POST /api/downloads/b3/rendafixa - Download renda fixa B3
        group.MapPost("/b3/rendafixa", async (DownloadRequest request, B3RendaFixaService service, ApplicationDbContext db) =>
        {
            try
            {
                // Baixa instrumentos de renda fixa para o periodo
                var instrumentos = await service.FetchInstrumentsAsync(request.StartDate, request.EndDate);

                // Salva no banco
                if (instrumentos.Count > 0)
                {
                    db.B3InstrumentosRendaFixa.AddRange(instrumentos);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = true,
                    Message = "Download de renda fixa B3 concluído",
                    RecordsProcessed = instrumentos.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar renda fixa B3",
                    Errors = new List<string> { ex.Message }
                });
            }
        })
        .WithName("DownloadB3RendaFixa")
        .WithOpenApi();

        // POST /api/downloads/bcb/expectativas - Download expectativas BCB
        group.MapPost("/bcb/expectativas", async (DownloadRequest request, BcbDownloadService service, ApplicationDbContext db) =>
        {
            try
            {
                // Baixa expectativas para o periodo
                var expectativas = await service.FetchExpectativasPeriodoAsync(request.StartDate, request.EndDate);

                // Salva no banco
                if (expectativas.Count > 0)
                {
                    db.BcbExpectativas.AddRange(expectativas);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = true,
                    Message = "Download de expectativas BCB concluído",
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
            try
            {
                // Baixa TPF para o periodo
                var tpfs = await service.FetchTPFPeriodoAsync(request.StartDate, request.EndDate);

                // Salva no banco
                if (tpfs.Count > 0)
                {
                    db.AnbimaTPFs.AddRange(tpfs);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = true,
                    Message = "Download de TPF ANBIMA concluído",
                    RecordsProcessed = tpfs.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar TPF ANBIMA",
                    Errors = new List<string> { ex.Message }
                });
            }
        })
        .WithName("DownloadAnbimaTpf")
        .WithOpenApi();

        // POST /api/downloads/anbima/vna - Download VNA ANBIMA
        group.MapPost("/anbima/vna", async (DownloadRequest request, AnbimaVNADownloadService service, ApplicationDbContext db) =>
        {
            try
            {
                var allVnas = new List<AnbimaVNA>();
                var currentDate = request.EndDate;

                // Itera por cada dia no periodo (VNA so aceita uma data por vez)
                while (currentDate >= request.StartDate)
                {
                    // Pula fins de semana
                    if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        var vnas = await service.FetchVNAAsync(currentDate);
                        if (vnas.Count > 0)
                        {
                            allVnas.AddRange(vnas);
                        }
                    }
                    currentDate = currentDate.AddDays(-1);
                }

                // Salva no banco
                if (allVnas.Count > 0)
                {
                    db.AnbimaVNAs.AddRange(allVnas);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new DownloadResponse
                {
                    Success = true,
                    Message = "Download de VNA ANBIMA concluído",
                    RecordsProcessed = allVnas.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new DownloadResponse
                {
                    Success = false,
                    Message = "Erro ao processar VNA ANBIMA",
                    Errors = new List<string> { ex.Message }
                });
            }
        })
        .WithName("DownloadAnbimaVna")
        .WithOpenApi();
    }
}
