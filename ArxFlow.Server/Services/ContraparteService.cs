using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Services;

// Servico de acesso a contrapartes
public class ContraparteService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Contraparte>> GetAllAsync()
    {
        return await _context.Set<Contraparte>().OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<Contraparte?> GetByIdAsync(int id)
    {
        return await _context.Set<Contraparte>().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contraparte> CreateAsync(Contraparte contraparte)
    {
        _context.Set<Contraparte>().Add(contraparte);
        await _context.SaveChangesAsync();
        return contraparte;
    }

    public async Task<Contraparte?> UpdateAsync(int id, Contraparte contraparte)
    {
        var existing = await _context.Set<Contraparte>().FindAsync(id);
        if (existing == null)
            return null;

        existing.Nome = contraparte.Nome;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contraparte = await _context.Set<Contraparte>().FindAsync(id);
        if (contraparte == null)
            return false;

        _context.Set<Contraparte>().Remove(contraparte);
        await _context.SaveChangesAsync();
        return true;
    }
}
