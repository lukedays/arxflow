using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;
using ArxFlow.Server.DTOs.Ativos;

namespace ArxFlow.Server.Endpoints;

public static class AtivosEndpoints
{
    public static void MapAtivosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ativos").WithTags("Ativos");

        // GET /api/ativos - Lista todos os ativos
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var ativos = await db.Ativos
                .Include(a => a.Issuer)
                .OrderBy(a => a.CodAtivo)
                .Select(a => new AtivoDto
                {
                    Id = a.Id,
                    CodAtivo = a.CodAtivo,
                    TipoAtivo = a.TipoAtivo,
                    EmissorId = a.EmissorId,
                    EmissorNome = a.Issuer != null ? a.Issuer.Nome : null,
                    AlphaToolsId = a.AlphaToolsId,
                    DataVencimento = a.DataVencimento
                })
                .ToListAsync();

            return Results.Ok(ativos);
        })
        .WithName("GetAtivos")
        .WithOpenApi();

        // GET /api/ativos/search?q={query} - Autocomplete de ativos
        group.MapGet("/search", async (string? q, ApplicationDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.Ok(Array.Empty<AtivoDto>());

            var ativos = await db.Ativos
                .Include(a => a.Issuer)
                .Where(a => a.CodAtivo.Contains(q))
                .OrderBy(a => a.CodAtivo)
                .Take(50)
                .Select(a => new AtivoDto
                {
                    Id = a.Id,
                    CodAtivo = a.CodAtivo,
                    TipoAtivo = a.TipoAtivo,
                    EmissorId = a.EmissorId,
                    EmissorNome = a.Issuer != null ? a.Issuer.Nome : null,
                    AlphaToolsId = a.AlphaToolsId,
                    DataVencimento = a.DataVencimento
                })
                .ToListAsync();

            return Results.Ok(ativos);
        })
        .WithName("SearchAtivos")
        .WithOpenApi();

        // GET /api/ativos/{id} - Obtém ativo por ID
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var ativo = await db.Ativos
                .Include(a => a.Issuer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ativo is null)
                return Results.NotFound(new { message = $"Ativo com ID {id} não encontrado" });

            var dto = new AtivoDto
            {
                Id = ativo.Id,
                CodAtivo = ativo.CodAtivo,
                TipoAtivo = ativo.TipoAtivo,
                EmissorId = ativo.EmissorId,
                EmissorNome = ativo.Issuer?.Nome,
                AlphaToolsId = ativo.AlphaToolsId,
                DataVencimento = ativo.DataVencimento
            };

            return Results.Ok(dto);
        })
        .WithName("GetAtivoById")
        .WithOpenApi();

        // POST /api/ativos - Cria novo ativo
        group.MapPost("/", async (CreateAtivoRequest request, ApplicationDbContext db) =>
        {
            // Verifica se já existe ativo com o mesmo código
            var existente = await db.Ativos
                .FirstOrDefaultAsync(a => a.CodAtivo == request.CodAtivo);

            if (existente != null)
                return Results.BadRequest(new { message = $"Já existe um ativo com o código {request.CodAtivo}" });

            var ativo = new Ativo
            {
                CodAtivo = request.CodAtivo,
                TipoAtivo = request.TipoAtivo,
                EmissorId = request.EmissorId,
                AlphaToolsId = request.AlphaToolsId,
                DataVencimento = request.DataVencimento
            };

            db.Ativos.Add(ativo);
            await db.SaveChangesAsync();

            // Carrega o emissor para incluir no DTO
            await db.Entry(ativo).Reference(a => a.Issuer).LoadAsync();

            var dto = new AtivoDto
            {
                Id = ativo.Id,
                CodAtivo = ativo.CodAtivo,
                TipoAtivo = ativo.TipoAtivo,
                EmissorId = ativo.EmissorId,
                EmissorNome = ativo.Issuer?.Nome,
                AlphaToolsId = ativo.AlphaToolsId,
                DataVencimento = ativo.DataVencimento
            };

            return Results.Created($"/api/ativos/{dto.Id}", dto);
        })
        .WithName("CreateAtivo")
        .WithOpenApi();

        // PUT /api/ativos/{id} - Atualiza ativo
        group.MapPut("/{id:int}", async (int id, UpdateAtivoRequest request, ApplicationDbContext db) =>
        {
            var ativo = await db.Ativos.FindAsync(id);

            if (ativo is null)
                return Results.NotFound(new { message = $"Ativo com ID {id} não encontrado" });

            // Verifica se já existe outro ativo com o mesmo código
            var existente = await db.Ativos
                .FirstOrDefaultAsync(a => a.CodAtivo == request.CodAtivo && a.Id != id);

            if (existente != null)
                return Results.BadRequest(new { message = $"Já existe outro ativo com o código {request.CodAtivo}" });

            ativo.CodAtivo = request.CodAtivo;
            ativo.TipoAtivo = request.TipoAtivo;
            ativo.EmissorId = request.EmissorId;
            ativo.AlphaToolsId = request.AlphaToolsId;
            ativo.DataVencimento = request.DataVencimento;
            ativo.AtualizadoEm = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // Carrega o emissor para incluir no DTO
            await db.Entry(ativo).Reference(a => a.Issuer).LoadAsync();

            var dto = new AtivoDto
            {
                Id = ativo.Id,
                CodAtivo = ativo.CodAtivo,
                TipoAtivo = ativo.TipoAtivo,
                EmissorId = ativo.EmissorId,
                EmissorNome = ativo.Issuer?.Nome,
                AlphaToolsId = ativo.AlphaToolsId,
                DataVencimento = ativo.DataVencimento
            };

            return Results.Ok(dto);
        })
        .WithName("UpdateAtivo")
        .WithOpenApi();

        // DELETE /api/ativos/{id} - Remove ativo
        group.MapDelete("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var ativo = await db.Ativos.FindAsync(id);

            if (ativo is null)
                return Results.NotFound(new { message = $"Ativo com ID {id} não encontrado" });

            db.Ativos.Remove(ativo);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteAtivo")
        .WithOpenApi();
    }
}
