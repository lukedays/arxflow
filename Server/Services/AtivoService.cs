using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Services;

// Servico de acesso a ativos
public class AtivoService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Ativo>> GetAllAsync()
    {
        return await _context.Set<Ativo>().Include(a => a.Issuer).OrderBy(a => a.CodAtivo).ToListAsync();
    }

    public async Task<Ativo?> GetByIdAsync(int id)
    {
        return await _context.Set<Ativo>().Include(a => a.Issuer).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Ativo?> GetByCodeAsync(string codAtivo)
    {
        return await _context
            .Set<Ativo>().Include(a => a.Issuer)
            .FirstOrDefaultAsync(a => a.CodAtivo == codAtivo);
    }

    public async Task<Ativo> CreateAsync(Ativo ativo)
    {
        ativo.CriadoEm = DateTime.UtcNow;
        ativo.AtualizadoEm = DateTime.UtcNow;
        _context.Set<Ativo>().Add(ativo);
        await _context.SaveChangesAsync();
        return ativo;
    }

    public async Task<Ativo?> UpdateAsync(int id, Ativo ativo)
    {
        var existing = await _context.Set<Ativo>().FindAsync(id);
        if (existing == null)
            return null;

        existing.CodAtivo = ativo.CodAtivo;
        existing.TipoAtivo = ativo.TipoAtivo;
        existing.EmissorId = ativo.EmissorId;
        existing.AlphaToolsId = ativo.AlphaToolsId;
        existing.DataVencimento = ativo.DataVencimento;
        existing.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ativo = await _context.Set<Ativo>().FindAsync(id);
        if (ativo == null)
            return false;

        _context.Set<Ativo>().Remove(ativo);
        await _context.SaveChangesAsync();
        return true;
    }
}
