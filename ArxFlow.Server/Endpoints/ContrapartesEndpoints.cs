using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;
using ArxFlow.Server.DTOs.Contrapartes;

namespace ArxFlow.Server.Endpoints;

public static class ContrapartesEndpoints
{
    public static void MapContrapartesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contrapartes").WithTags("Contrapartes");

        // GET /api/contrapartes
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var contrapartes = await db.Contrapartes
                .OrderBy(c => c.Nome)
                .Select(c => new ContraparteDto
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .ToListAsync();

            return Results.Ok(contrapartes);
        })
        .WithName("GetContrapartes")
        .WithOpenApi();

        // GET /api/contrapartes/{id}
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var contraparte = await db.Contrapartes.FindAsync(id);

            if (contraparte is null)
                return Results.NotFound(new { message = $"Contraparte com ID {id} não encontrado" });

            var dto = new ContraparteDto
            {
                Id = contraparte.Id,
                Nome = contraparte.Nome
            };

            return Results.Ok(dto);
        })
        .WithName("GetContraparteById")
        .WithOpenApi();

        // POST /api/contrapartes
        group.MapPost("/", async (CreateContraparteRequest request, ApplicationDbContext db) =>
        {
            var contraparte = new Contraparte
            {
                Nome = request.Nome
            };

            db.Contrapartes.Add(contraparte);
            await db.SaveChangesAsync();

            var dto = new ContraparteDto
            {
                Id = contraparte.Id,
                Nome = contraparte.Nome
            };

            return Results.Created($"/api/contrapartes/{dto.Id}", dto);
        })
        .WithName("CreateContraparte")
        .WithOpenApi();

        // PUT /api/contrapartes/{id}
        group.MapPut("/{id:int}", async (int id, UpdateContraparteRequest request, ApplicationDbContext db) =>
        {
            var contraparte = await db.Contrapartes.FindAsync(id);

            if (contraparte is null)
                return Results.NotFound(new { message = $"Contraparte com ID {id} não encontrado" });

            contraparte.Nome = request.Nome;

            await db.SaveChangesAsync();

            var dto = new ContraparteDto
            {
                Id = contraparte.Id,
                Nome = contraparte.Nome
            };

            return Results.Ok(dto);
        })
        .WithName("UpdateContraparte")
        .WithOpenApi();

        // DELETE /api/contrapartes/{id}
        group.MapDelete("/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            var contraparte = await db.Contrapartes.FindAsync(id);

            if (contraparte is null)
                return Results.NotFound(new { message = $"Contraparte com ID {id} não encontrado" });

            db.Contrapartes.Remove(contraparte);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteContraparte")
        .WithOpenApi();
    }
}
