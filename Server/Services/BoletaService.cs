using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Data;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Services;

// Servico para operacoes CRUD de boletas
public class BoletaService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    // Retorna todas as boletas ordenadas por data/hora decrescente
    public async Task<List<Boleta>> ObterTodasAsync()
    {
        return await _context.Set<Boleta>()
            .Include(b => b.Ativo)
                .ThenInclude(a => a!.Issuer)
            .Include(b => b.Contraparte)
            .Include(b => b.Fundo)
            .OrderByDescending(b => b.CriadoEm)
            .ToListAsync();
    }

    // Retorna uma boleta pelo ID
    public async Task<Boleta?> ObterPorIdAsync(int id)
    {
        return await _context.Set<Boleta>()
            .Include(b => b.Ativo)
                .ThenInclude(a => a!.Issuer)
            .Include(b => b.Contraparte)
            .Include(b => b.Fundo)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    // Cria uma nova boleta
    public async Task<Boleta> CriarAsync(Boleta boleta)
    {
        boleta.CriadoEm = DateTime.Now;
        _context.Set<Boleta>().Add(boleta);
        await _context.SaveChangesAsync();
        return boleta;
    }

    // Atualiza uma boleta existente
    public async Task<Boleta?> AtualizarAsync(Boleta boleta)
    {
        var existente = await _context.Set<Boleta>().FindAsync(boleta.Id);
        if (existente == null) return null;

        existente.Ticker = boleta.Ticker;
        existente.TipoOperacao = boleta.TipoOperacao;
        existente.Volume = boleta.Volume;
        existente.Quantidade = boleta.Quantidade;
        existente.TipoPrecificacao = boleta.TipoPrecificacao;
        existente.NtnbReferencia = boleta.NtnbReferencia;
        existente.SpreadValor = boleta.SpreadValor;
        existente.DataFixing = boleta.DataFixing;
        existente.TaxaNominal = boleta.TaxaNominal;
        existente.PU = boleta.PU;
        existente.ContraparteId = boleta.ContraparteId;
        existente.Alocacao = boleta.Alocacao;
        existente.Usuario = boleta.Usuario;
        existente.Observacao = boleta.Observacao;

        await _context.SaveChangesAsync();
        return existente;
    }

    // Remove uma boleta
    public async Task<bool> RemoverAsync(int id)
    {
        var boleta = await _context.Set<Boleta>().FindAsync(id);
        if (boleta == null) return false;

        _context.Set<Boleta>().Remove(boleta);
        await _context.SaveChangesAsync();
        return true;
    }
}
