using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;
using ArxFlow.Server.DTOs.Boletas;

namespace ArxFlow.Server.Endpoints;

public static class BoletasEndpoints
{
    public static void MapBoletasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/boletas").WithTags("Boletas");

        // GET /api/boletas - Lista todas as boletas
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var boletas = await db.Boletas
                .Include(b => b.Ativo)
                .Include(b => b.Contraparte)
                .Include(b => b.Fundo)
                .OrderByDescending(b => b.CriadoEm)
                .Select(b => new BoletaDto
                {
                    Id = b.Id,
                    BoletaPrincipalId = b.BoletaPrincipalId,
                    AtivoId = b.AtivoId,
                    AtivoNome = b.Ativo != null ? b.Ativo.CodAtivo : null,
                    ContraparteId = b.ContraparteId,
                    ContraparteNome = b.Contraparte != null ? b.Contraparte.Nome : null,
                    FundoId = b.FundoId,
                    FundoNome = b.Fundo != null ? b.Fundo.Nome : null,
                    Ticker = b.Ticker,
                    TipoOperacao = b.TipoOperacao,
                    Volume = b.Volume,
                    Quantidade = b.Quantidade,
                    TipoPrecificacao = b.TipoPrecificacao.ToString(),
                    NtnbReferencia = b.NtnbReferencia,
                    SpreadValor = b.SpreadValor,
                    DataFixing = b.DataFixing,
                    TaxaNominal = b.TaxaNominal,
                    PU = b.PU,
                    Alocacao = b.Alocacao,
                    Usuario = b.Usuario,
                    Observacao = b.Observacao,
                    DataLiquidacao = b.DataLiquidacao,
                    Status = b.Status.ToString(),
                    CriadoEm = b.CriadoEm
                })
                .ToListAsync();

            return Results.Ok(boletas);
        })
        .WithName("GetBoletas")
        .WithOpenApi();

        // GET /api/boletas/{id} - Obtém boleta por ID
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var boleta = await db.Boletas
                .Include(b => b.Ativo)
                .Include(b => b.Contraparte)
                .Include(b => b.Fundo)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boleta is null)
                return Results.NotFound(new { message = $"Boleta com ID {id} não encontrada" });

            var dto = new BoletaDto
            {
                Id = boleta.Id,
                BoletaPrincipalId = boleta.BoletaPrincipalId,
                AtivoId = boleta.AtivoId,
                AtivoNome = boleta.Ativo?.CodAtivo,
                ContraparteId = boleta.ContraparteId,
                ContraparteNome = boleta.Contraparte?.Nome,
                FundoId = boleta.FundoId,
                FundoNome = boleta.Fundo?.Nome,
                Ticker = boleta.Ticker,
                TipoOperacao = boleta.TipoOperacao,
                Volume = boleta.Volume,
                Quantidade = boleta.Quantidade,
                TipoPrecificacao = boleta.TipoPrecificacao.ToString(),
                NtnbReferencia = boleta.NtnbReferencia,
                SpreadValor = boleta.SpreadValor,
                DataFixing = boleta.DataFixing,
                TaxaNominal = boleta.TaxaNominal,
                PU = boleta.PU,
                Alocacao = boleta.Alocacao,
                Usuario = boleta.Usuario,
                Observacao = boleta.Observacao,
                DataLiquidacao = boleta.DataLiquidacao,
                Status = boleta.Status.ToString(),
                CriadoEm = boleta.CriadoEm
            };

            return Results.Ok(dto);
        })
        .WithName("GetBoletaById")
        .WithOpenApi();

        // POST /api/boletas - Cria nova boleta
        group.MapPost("/", async (CreateBoletaRequest request, ApplicationDbContext db) =>
        {
            var boleta = new Boleta
            {
                BoletaPrincipalId = request.BoletaPrincipalId,
                AtivoId = request.AtivoId,
                ContraparteId = request.ContraparteId,
                FundoId = request.FundoId,
                Ticker = request.Ticker,
                TipoOperacao = request.TipoOperacao,
                Volume = request.Volume,
                Quantidade = request.Quantidade,
                TipoPrecificacao = Enum.Parse<TipoPrecificacao>(request.TipoPrecificacao),
                NtnbReferencia = request.NtnbReferencia,
                SpreadValor = request.SpreadValor,
                DataFixing = request.DataFixing,
                TaxaNominal = request.TaxaNominal,
                PU = request.PU,
                Alocacao = request.Alocacao,
                Usuario = request.Usuario,
                Observacao = request.Observacao,
                DataLiquidacao = request.DataLiquidacao,
                Status = Enum.Parse<StatusBoleta>(request.Status),
                CriadoEm = DateTime.Now
            };

            db.Boletas.Add(boleta);
            await db.SaveChangesAsync();

            await db.Entry(boleta).Reference(b => b.Ativo).LoadAsync();
            await db.Entry(boleta).Reference(b => b.Contraparte).LoadAsync();
            await db.Entry(boleta).Reference(b => b.Fundo).LoadAsync();

            var dto = new BoletaDto
            {
                Id = boleta.Id,
                BoletaPrincipalId = boleta.BoletaPrincipalId,
                AtivoId = boleta.AtivoId,
                AtivoNome = boleta.Ativo?.CodAtivo,
                ContraparteId = boleta.ContraparteId,
                ContraparteNome = boleta.Contraparte?.Nome,
                FundoId = boleta.FundoId,
                FundoNome = boleta.Fundo?.Nome,
                Ticker = boleta.Ticker,
                TipoOperacao = boleta.TipoOperacao,
                Volume = boleta.Volume,
                Quantidade = boleta.Quantidade,
                TipoPrecificacao = boleta.TipoPrecificacao.ToString(),
                NtnbReferencia = boleta.NtnbReferencia,
                SpreadValor = boleta.SpreadValor,
                DataFixing = boleta.DataFixing,
                TaxaNominal = boleta.TaxaNominal,
                PU = boleta.PU,
                Alocacao = boleta.Alocacao,
                Usuario = boleta.Usuario,
                Observacao = boleta.Observacao,
                DataLiquidacao = boleta.DataLiquidacao,
                Status = boleta.Status.ToString(),
                CriadoEm = boleta.CriadoEm
            };

            return Results.Created($"/api/boletas/{dto.Id}", dto);
        })
        .WithName("CreateBoleta")
        .WithOpenApi();

        // PUT /api/boletas/{id} - Atualiza boleta
        group.MapPut("/{id:int}", async (int id, UpdateBoletaRequest request, ApplicationDbContext db) =>
        {
            var boleta = await db.Boletas.FindAsync(id);

            if (boleta is null)
                return Results.NotFound(new { message = $"Boleta com ID {id} não encontrada" });

            boleta.BoletaPrincipalId = request.BoletaPrincipalId;
            boleta.AtivoId = request.AtivoId;
            boleta.ContraparteId = request.ContraparteId;
            boleta.FundoId = request.FundoId;
            boleta.Ticker = request.Ticker;
            boleta.TipoOperacao = request.TipoOperacao;
            boleta.Volume = request.Volume;
            boleta.Quantidade = request.Quantidade;
            boleta.TipoPrecificacao = Enum.Parse<TipoPrecificacao>(request.TipoPrecificacao);
            boleta.NtnbReferencia = request.NtnbReferencia;
            boleta.SpreadValor = request.SpreadValor;
            boleta.DataFixing = request.DataFixing;
            boleta.TaxaNominal = request.TaxaNominal;
            boleta.PU = request.PU;
            boleta.Alocacao = request.Alocacao;
            boleta.Usuario = request.Usuario;
            boleta.Observacao = request.Observacao;
            boleta.DataLiquidacao = request.DataLiquidacao;
            boleta.Status = Enum.Parse<StatusBoleta>(request.Status);

            await db.SaveChangesAsync();

            await db.Entry(boleta).Reference(b => b.Ativo).LoadAsync();
            await db.Entry(boleta).Reference(b => b.Contraparte).LoadAsync();
            await db.Entry(boleta).Reference(b => b.Fundo).LoadAsync();

            var dto = new BoletaDto
            {
                Id = boleta.Id,
                BoletaPrincipalId = boleta.BoletaPrincipalId,
                AtivoId = boleta.AtivoId,
                AtivoNome = boleta.Ativo?.CodAtivo,
                ContraparteId = boleta.ContraparteId,
                ContraparteNome = boleta.Contraparte?.Nome,
                FundoId = boleta.FundoId,
                FundoNome = boleta.Fundo?.Nome,
                Ticker = boleta.Ticker,
                TipoOperacao = boleta.TipoOperacao,
                Volume = boleta.Volume,
                Quantidade = boleta.Quantidade,
                TipoPrecificacao = boleta.TipoPrecificacao.ToString(),
                NtnbReferencia = boleta.NtnbReferencia,
                SpreadValor = boleta.SpreadValor,
                DataFixing = boleta.DataFixing,
                TaxaNominal = boleta.TaxaNominal,
                PU = boleta.PU,
                Alocacao = boleta.Alocacao,
                Usuario = boleta.Usuario,
                Observacao = boleta.Observacao,
                DataLiquidacao = boleta.DataLiquidacao,
                Status = boleta.Status.ToString(),
                CriadoEm = boleta.CriadoEm
            };

            return Results.Ok(dto);
        })
        .WithName("UpdateBoleta")
        .WithOpenApi();

        // DELETE /api/boletas/{id} - Remove boleta
        group.MapDelete("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var boleta = await db.Boletas.FindAsync(id);

            if (boleta is null)
                return Results.NotFound(new { message = $"Boleta com ID {id} não encontrada" });

            db.Boletas.Remove(boleta);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteBoleta")
        .WithOpenApi();
    }
}
