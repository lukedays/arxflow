using Microsoft.EntityFrameworkCore;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Data;

// Contexto do banco de dados SQLite
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<Emissor> Emissores { get; set; }
    public DbSet<Ativo> Ativos { get; set; }
    public DbSet<Fundo> Fundos { get; set; }
    public DbSet<Boleta> Boletas { get; set; }
    public DbSet<Contraparte> Contrapartes { get; set; }
    public DbSet<B3InstrumentoDerivativo> B3InstrumentosDerivativos { get; set; }
    public DbSet<B3PrecoDerivativo> B3PrecosDerivativos { get; set; }
    public DbSet<B3InstrumentoRendaFixa> B3InstrumentosRendaFixa { get; set; }
    public DbSet<BcbExpectativa> BcbExpectativas { get; set; }
    public DbSet<AnbimaTPF> AnbimaTPFs { get; set; }
    public DbSet<AnbimaVNA> AnbimaVNAs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configuracoes do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        // Configuracao do Emissor
        modelBuilder.Entity<Emissor>(entity =>
        {
            entity.HasIndex(e => e.Nome).HasDatabaseName("IX_Emissor_Nome");
            entity.HasIndex(e => e.AlphaToolsId).HasDatabaseName("IX_Emissor_AlphaToolsId");
        });

        // Configuracao do Ativo
        modelBuilder.Entity<Ativo>(entity =>
        {
            entity.HasIndex(e => e.CodAtivo).IsUnique().HasDatabaseName("IX_Ativo_CodAtivo");
            entity.HasIndex(e => e.EmissorId).HasDatabaseName("IX_Ativo_EmissorId");
            entity.HasIndex(e => e.AlphaToolsId).HasDatabaseName("IX_Ativo_AlphaToolsId");

            entity
                .HasOne(e => e.Issuer)
                .WithMany(i => i.Ativos)
                .HasForeignKey(e => e.EmissorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuracao do Fundo
        modelBuilder.Entity<Fundo>(entity =>
        {
            entity.HasIndex(e => e.Nome).HasDatabaseName("IX_Fundo_Nome");
            entity.HasIndex(e => e.AlphaToolsId).HasDatabaseName("IX_Fundo_AlphaToolsId");
        });

        // Configuracao do B3InstrumentoDerivativo
        modelBuilder.Entity<B3InstrumentoDerivativo>(entity =>
        {
            entity.HasIndex(e => new { e.InstrumentoFinanceiro, e.DataReferencia })
                .IsUnique()
                .HasDatabaseName("IX_B3InstrumentoDerivativo_InstrumentoFinanceiro_DataReferencia");
            entity.HasIndex(e => e.Ativo).HasDatabaseName("IX_B3InstrumentoDerivativo_Ativo");
            entity.HasIndex(e => e.DataVencimento).HasDatabaseName("IX_B3InstrumentoDerivativo_DataVencimento");
        });

        // Configuracao do B3PrecoDerivativo
        modelBuilder.Entity<B3PrecoDerivativo>(entity =>
        {
            entity.HasIndex(e => new { e.Ticker, e.DataReferencia })
                .IsUnique()
                .HasDatabaseName("IX_B3PrecoDerivativo_Ticker_DataReferencia");
            entity.HasIndex(e => e.DataReferencia).HasDatabaseName("IX_B3PrecoDerivativo_DataReferencia");
        });

        // Configuracao do B3InstrumentoRendaFixa
        modelBuilder.Entity<B3InstrumentoRendaFixa>(entity =>
        {
            entity.HasIndex(e => new { e.CodigoIF, e.DataReferencia })
                .IsUnique()
                .HasDatabaseName("IX_B3InstrumentoRendaFixa_CodigoIF_DataReferencia");
            entity.HasIndex(e => e.InstrumentoFinanceiro).HasDatabaseName("IX_B3InstrumentoRendaFixa_InstrumentoFinanceiro");
            entity.HasIndex(e => e.Vencimento).HasDatabaseName("IX_B3InstrumentoRendaFixa_Vencimento");
            entity.HasIndex(e => e.Emissor).HasDatabaseName("IX_B3InstrumentoRendaFixa_Emissor");
        });

        // Configuracao do BcbExpectativa
        modelBuilder.Entity<BcbExpectativa>(entity =>
        {
            entity.HasIndex(e => new { e.Data, e.Indicador, e.DataReferencia })
                .IsUnique()
                .HasDatabaseName("IX_BcbExpectativa_Data_Indicador_DataReferencia");
            entity.HasIndex(e => e.Data).HasDatabaseName("IX_BcbExpectativa_Data");
            entity.HasIndex(e => e.Indicador).HasDatabaseName("IX_BcbExpectativa_Indicador");
        });

        // Configuracao do AnbimaTPF
        modelBuilder.Entity<AnbimaTPF>(entity =>
        {
            entity.HasIndex(e => new { e.DataReferencia, e.Titulo, e.DataVencimento })
                .IsUnique()
                .HasDatabaseName("IX_AnbimaTPF_DataReferencia_Titulo_DataVencimento");
            entity.HasIndex(e => e.DataReferencia).HasDatabaseName("IX_AnbimaTPF_DataReferencia");
            entity.HasIndex(e => e.Titulo).HasDatabaseName("IX_AnbimaTPF_Titulo");
            entity.HasIndex(e => e.DataVencimento).HasDatabaseName("IX_AnbimaTPF_DataVencimento");
        });

        // Configuracao do AnbimaVNA
        modelBuilder.Entity<AnbimaVNA>(entity =>
        {
            entity.HasIndex(e => new { e.DataReferencia, e.Titulo })
                .IsUnique()
                .HasDatabaseName("IX_AnbimaVNA_DataReferencia_Titulo");
            entity.HasIndex(e => e.DataReferencia).HasDatabaseName("IX_AnbimaVNA_DataReferencia");
            entity.HasIndex(e => e.Titulo).HasDatabaseName("IX_AnbimaVNA_Titulo");
        });
    }
}
