using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;
using ArxFlow.Server.DTOs.Emissores;

namespace ArxFlow.Server.Endpoints;

public static class EmissoresEndpoints
{
    public static void MapEmissoresEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/emissores").WithTags("Emissores");

        // GET /api/emissores - Lista todos os emissores
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var emissores = await db.Emissores
                .OrderBy(e => e.Nome)
                .Select(e => new EmissorDto
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    Documento = e.Documento,
                    AlphaToolsId = e.AlphaToolsId
                })
                .ToListAsync();

            return Results.Ok(emissores);
        })
        .WithName("GetEmissores")
        .WithOpenApi();

        // GET /api/emissores/{id} - Obtém emissor por ID
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var emissor = await db.Emissores.FindAsync(id);

            if (emissor is null)
                return Results.NotFound(new { message = $"Emissor com ID {id} não encontrado" });

            var dto = new EmissorDto
            {
                Id = emissor.Id,
                Nome = emissor.Nome,
                Documento = emissor.Documento,
                AlphaToolsId = emissor.AlphaToolsId
            };

            return Results.Ok(dto);
        })
        .WithName("GetEmissorById")
        .WithOpenApi();

        // POST /api/emissores - Cria novo emissor
        group.MapPost("/", async (CreateEmissorRequest request, ApplicationDbContext db) =>
        {
            var emissor = new Emissor
            {
                Nome = request.Nome,
                Documento = request.Documento,
                AlphaToolsId = request.AlphaToolsId
            };

            db.Emissores.Add(emissor);
            await db.SaveChangesAsync();

            var dto = new EmissorDto
            {
                Id = emissor.Id,
                Nome = emissor.Nome,
                Documento = emissor.Documento,
                AlphaToolsId = emissor.AlphaToolsId
            };

            return Results.Created($"/api/emissores/{dto.Id}", dto);
        })
        .WithName("CreateEmissor")
        .WithOpenApi();

        // PUT /api/emissores/{id} - Atualiza emissor
        group.MapPut("/{id:int}", async (int id, UpdateEmissorRequest request, ApplicationDbContext db) =>
        {
            var emissor = await db.Emissores.FindAsync(id);

            if (emissor is null)
                return Results.NotFound(new { message = $"Emissor com ID {id} não encontrado" });

            emissor.Nome = request.Nome;
            emissor.Documento = request.Documento;
            emissor.AlphaToolsId = request.AlphaToolsId;

            await db.SaveChangesAsync();

            var dto = new EmissorDto
            {
                Id = emissor.Id,
                Nome = emissor.Nome,
                Documento = emissor.Documento,
                AlphaToolsId = emissor.AlphaToolsId
            };

            return Results.Ok(dto);
        })
        .WithName("UpdateEmissor")
        .WithOpenApi();

        // DELETE /api/emissores/{id} - Remove emissor
        group.MapDelete("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var emissor = await db.Emissores.FindAsync(id);

            if (emissor is null)
                return Results.NotFound(new { message = $"Emissor com ID {id} não encontrado" });

            db.Emissores.Remove(emissor);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteEmissor")
        .WithOpenApi();
    }
}
