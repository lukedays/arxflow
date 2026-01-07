using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;
using ArxFlow.Server.DTOs.Fundos;

namespace ArxFlow.Server.Endpoints;

public static class FundosEndpoints
{
    public static void MapFundosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/fundos").WithTags("Fundos");

        // GET /api/fundos
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var fundos = await db.Fundos
                .OrderBy(f => f.Nome)
                .Select(f => new FundoDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    Cnpj = f.Cnpj,
                    AlphaToolsId = f.AlphaToolsId
                })
                .ToListAsync();

            return Results.Ok(fundos);
        })
        .WithName("GetFundos")
        .WithOpenApi();

        // GET /api/fundos/{id}
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var fundo = await db.Fundos.FindAsync(id);

            if (fundo is null)
                return Results.NotFound(new { message = $"Fundo com ID {id} não encontrado" });

            var dto = new FundoDto
            {
                Id = fundo.Id,
                Nome = fundo.Nome,
                Cnpj = fundo.Cnpj,
                AlphaToolsId = fundo.AlphaToolsId
            };

            return Results.Ok(dto);
        })
        .WithName("GetFundoById")
        .WithOpenApi();

        // POST /api/fundos
        group.MapPost("/", async (CreateFundoRequest request, ApplicationDbContext db) =>
        {
            var fundo = new Fundo
            {
                Nome = request.Nome,
                Cnpj = request.Cnpj,
                AlphaToolsId = request.AlphaToolsId
            };

            db.Fundos.Add(fundo);
            await db.SaveChangesAsync();

            var dto = new FundoDto
            {
                Id = fundo.Id,
                Nome = fundo.Nome,
                Cnpj = fundo.Cnpj,
                AlphaToolsId = fundo.AlphaToolsId
            };

            return Results.Created($"/api/fundos/{dto.Id}", dto);
        })
        .WithName("CreateFundo")
        .WithOpenApi();

        // PUT /api/fundos/{id}
        group.MapPut("/{id:int}", async (int id, UpdateFundoRequest request, ApplicationDbContext db) =>
        {
            var fundo = await db.Fundos.FindAsync(id);

            if (fundo is null)
                return Results.NotFound(new { message = $"Fundo com ID {id} não encontrado" });

            fundo.Nome = request.Nome;
            fundo.Cnpj = request.Cnpj;
            fundo.AlphaToolsId = request.AlphaToolsId;

            await db.SaveChangesAsync();

            var dto = new FundoDto
            {
                Id = fundo.Id,
                Nome = fundo.Nome,
                Cnpj = fundo.Cnpj,
                AlphaToolsId = fundo.AlphaToolsId
            };

            return Results.Ok(dto);
        })
        .WithName("UpdateFundo")
        .WithOpenApi();

        // DELETE /api/fundos/{id}
        group.MapDelete("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var fundo = await db.Fundos.FindAsync(id);

            if (fundo is null)
                return Results.NotFound(new { message = $"Fundo com ID {id} não encontrado" });

            db.Fundos.Remove(fundo);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteFundo")
        .WithOpenApi();
    }
}
