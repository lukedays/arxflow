using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Services;

// Servico de acesso a emissores
public class EmissorService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Emissor>> GetAllAsync()
    {
        return await _context.Set<Emissor>().OrderBy(i => i.Nome).ToListAsync();
    }

    public async Task<Emissor?> GetByIdAsync(int id)
    {
        return await _context.Set<Emissor>().Include(i => i.Ativos).FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Emissor> CreateAsync(Emissor issuer)
    {
        issuer.CreatedAt = DateTime.UtcNow;
        issuer.UpdatedAt = DateTime.UtcNow;
        _context.Set<Emissor>().Add(issuer);
        await _context.SaveChangesAsync();
        return issuer;
    }

    public async Task<Emissor?> UpdateAsync(int id, Emissor issuer)
    {
        var existing = await _context.Set<Emissor>().FindAsync(id);
        if (existing == null)
            return null;

        existing.Nome = issuer.Nome;
        existing.Documento = issuer.Documento;
        existing.AlphaToolsId = issuer.AlphaToolsId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var issuer = await _context.Set<Emissor>().FindAsync(id);
        if (issuer == null)
            return false;

        _context.Set<Emissor>().Remove(issuer);
        await _context.SaveChangesAsync();
        return true;
    }
}
