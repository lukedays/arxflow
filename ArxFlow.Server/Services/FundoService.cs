using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Services;

// Servico de acesso a fundos
public class FundoService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Fundo>> GetAllAsync()
    {
        return await _context.Set<Fundo>().OrderBy(f => f.Nome).ToListAsync();
    }

    public async Task<Fundo?> GetByIdAsync(int id)
    {
        return await _context
            .Set<Fundo>().Include(f => f.Boletas)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fundo> CreateAsync(Fundo fund)
    {
        fund.CreatedAt = DateTime.UtcNow;
        fund.UpdatedAt = DateTime.UtcNow;
        _context.Set<Fundo>().Add(fund);
        await _context.SaveChangesAsync();
        return fund;
    }

    public async Task<Fundo?> UpdateAsync(int id, Fundo fund)
    {
        var existing = await _context.Set<Fundo>().FindAsync(id);
        if (existing == null)
            return null;

        existing.Nome = fund.Nome;
        existing.Cnpj = fund.Cnpj;
        existing.AlphaToolsId = fund.AlphaToolsId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var fund = await _context.Set<Fundo>().FindAsync(id);
        if (fund == null)
            return false;

        _context.Set<Fundo>().Remove(fund);
        await _context.SaveChangesAsync();
        return true;
    }
}
