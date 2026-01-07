using ArxFlow.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ArxFlow.Server.Data;

/// <summary>
/// Seed de dados iniciais do banco de dados
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Aplica seed de dados se o banco estiver vazio
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // Seed de Contrapartes
        if (!await db.Set<Contraparte>().AnyAsync())
        {
            db.Set<Contraparte>().AddRange(
                new Contraparte { Nome = "Ativa" },
                new Contraparte { Nome = "BTG Pactual" },
                new Contraparte { Nome = "XP Investimentos" },
                new Contraparte { Nome = "Itau BBA" },
                new Contraparte { Nome = "Bradesco BBI" }
            );
            await db.SaveChangesAsync();
        }

        // Seed de Issuers (Emissores)
        if (!await db.Set<Emissor>().AnyAsync())
        {
            var now = DateTime.UtcNow;
            db.Set<Emissor>().AddRange(
                new Emissor
                {
                    Nome = "Bradesco",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Emissor
                {
                    Nome = "Rede Dor",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Emissor
                {
                    Nome = "EQUATORIAL PARA DISTRIBUIDORA DE ENERGIA S.A.",
                    Documento = "04.895.728/0001-80",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Emissor
                {
                    Nome = "ENERGISA SUL-SUDESTE - DISTRIBUIDORA DE ENERGIA SA",
                    Documento = "07.282.377/0001-20",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        // Seed de Funds (Fundos)
        if (!await db.Set<Fundo>().AnyAsync())
        {
            var now = DateTime.UtcNow;
            db.Set<Fundo>().AddRange(
                new Fundo
                {
                    Nome = "Fundo Master FIC FIM",
                    Cnpj = "12.345.678/0001-90",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Fundo
                {
                    Nome = "Fundo Plus FIA",
                    Cnpj = "98.765.432/0001-10",
                    AlphaToolsId = "FND-001",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
            await db.SaveChangesAsync();
        }

        // Seed de Assets (Ativos)
        if (!await db.Set<Ativo>().AnyAsync())
        {
            var now = DateTime.UtcNow;

            // Busca issuers para associar
            var equatorial = await db.Set<Emissor>().FirstOrDefaultAsync(i => i.Nome.Contains("EQUATORIAL"));
            var energisa = await db.Set<Emissor>().FirstOrDefaultAsync(i => i.Nome.Contains("ENERGISA"));

            db.Set<Ativo>().AddRange(
                new Ativo
                {
                    CodAtivo = "EQPA19",
                    EmissorId = equatorial?.Id,
                    CriadoEm = now,
                    AtualizadoEm = now
                },
                new Ativo
                {
                    CodAtivo = "ESSDA5",
                    EmissorId = energisa?.Id,
                    CriadoEm = now,
                    AtualizadoEm = now
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
