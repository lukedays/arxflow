using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArxFlow.Server.Models;

namespace ArxFlow.Server.Data;

// Configuracao da entidade Ativo
public class AtivoConfiguration : IEntityTypeConfiguration<Ativo>
{
    public void Configure(EntityTypeBuilder<Ativo> builder)
    {
        builder.HasIndex(e => e.CodAtivo).IsUnique().HasDatabaseName("IX_Ativo_CodAtivo");
        builder.HasIndex(e => e.EmissorId).HasDatabaseName("IX_Ativo_EmissorId");
        builder.HasIndex(e => e.AlphaToolsId).HasDatabaseName("IX_Ativo_AlphaToolsId");

        builder
            .HasOne(e => e.Issuer)
            .WithMany(i => i.Ativos)
            .HasForeignKey(e => e.EmissorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// Configuracao da entidade Boleta
public class BoletaConfiguration : IEntityTypeConfiguration<Boleta>
{
    public void Configure(EntityTypeBuilder<Boleta> entity)
    {
        entity.HasIndex(e => e.CriadoEm).HasDatabaseName("IX_Boleta_CriadoEm");
        entity.HasIndex(e => e.Ticker).HasDatabaseName("IX_Boleta_Ticker");
        entity.HasIndex(e => e.AtivoId).HasDatabaseName("IX_Boleta_AtivoId");
        entity.HasIndex(e => e.ContraparteId).HasDatabaseName("IX_Boleta_ContraparteId");
        entity.HasIndex(e => e.FundoId).HasDatabaseName("IX_Boleta_FundoId");

        entity
            .HasOne(e => e.Ativo)
            .WithMany(a => a.Boletas)
            .HasForeignKey(e => e.AtivoId)
            .OnDelete(DeleteBehavior.SetNull);

        entity
            .HasOne(e => e.Contraparte)
            .WithMany()
            .HasForeignKey(e => e.ContraparteId)
            .OnDelete(DeleteBehavior.SetNull);

        entity
            .HasOne(e => e.Fundo)
            .WithMany(f => f.Boletas)
            .HasForeignKey(e => e.FundoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

// Configuracao da entidade B3PrecoDerivativo
public class B3PrecoDerivativoConfiguration : IEntityTypeConfiguration<B3PrecoDerivativo>
{
    public void Configure(EntityTypeBuilder<B3PrecoDerivativo> builder)
    {
        builder.HasIndex(e => new { e.Ticker, e.DataReferencia })
            .IsUnique()
            .HasDatabaseName("IX_B3PrecoDerivativo_Ticker_DataReferencia");
        builder.HasIndex(e => e.DataReferencia).HasDatabaseName("IX_B3PrecoDerivativo_DataReferencia");
    }
}

// Configuracao da entidade Fundo
public class FundoConfiguration : IEntityTypeConfiguration<Fundo>
{
    public void Configure(EntityTypeBuilder<Fundo> builder)
    {
        builder.HasIndex(e => e.Nome).HasDatabaseName("IX_Fundo_Nome");
        builder.HasIndex(e => e.AlphaToolsId).HasDatabaseName("IX_Fundo_AlphaToolsId");
    }
}

// Configuracao da entidade B3InstrumentoDerivativo
public class B3InstrumentoDerivativoConfiguration : IEntityTypeConfiguration<B3InstrumentoDerivativo>
{
    public void Configure(EntityTypeBuilder<B3InstrumentoDerivativo> builder)
    {
        builder.HasIndex(e => new { e.InstrumentoFinanceiro, e.DataReferencia })
            .IsUnique()
            .HasDatabaseName("IX_B3InstrumentoDerivativo_InstrumentoFinanceiro_DataReferencia");
        builder.HasIndex(e => e.Ativo).HasDatabaseName("IX_B3InstrumentoDerivativo_Ativo");
        builder.HasIndex(e => e.DataVencimento).HasDatabaseName("IX_B3InstrumentoDerivativo_DataVencimento");
    }
}

// Configuracao da entidade Emissor
public class EmissorConfiguration : IEntityTypeConfiguration<Emissor>
{
    public void Configure(EntityTypeBuilder<Emissor> builder)
    {
        builder.HasIndex(e => e.Nome).HasDatabaseName("IX_Emissor_Nome");
        builder.HasIndex(e => e.AlphaToolsId).HasDatabaseName("IX_Emissor_AlphaToolsId");
    }
}
