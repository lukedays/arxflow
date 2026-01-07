using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

/// <summary>
/// Informacoes de instrumentos de renda fixa da B3
/// </summary>
public class B3InstrumentoRendaFixa
{
    [Key]
    public int Id { get; set; }

    public DateTime DataReferencia { get; set; }

    [MaxLength(50)]
    public string CodigoIF { get; set; } = "";

    [MaxLength(20)]
    public string? CodigoIsin { get; set; }

    [MaxLength(200)]
    public string? Emissor { get; set; }

    [MaxLength(20)]
    public string InstrumentoFinanceiro { get; set; } = "";

    [MaxLength(10)]
    public string? Incentivada { get; set; }

    [MaxLength(20)]
    public string? NumeroSerie { get; set; }

    [MaxLength(20)]
    public string? NumeroEmissao { get; set; }

    [MaxLength(50)]
    public string? Indexador { get; set; }

    public decimal? PercentualIndexador { get; set; }

    public decimal? TaxaAdicional { get; set; }

    [MaxLength(20)]
    public string? BaseCalculo { get; set; }

    public DateTime? Vencimento { get; set; }

    public long? QuantidadeEmitida { get; set; }

    public decimal? PrecoUnitarioEmissao { get; set; }

    [MaxLength(10)]
    public string? EsforcoRestrito { get; set; }

    [MaxLength(50)]
    public string? TipoEmissao { get; set; }

    [MaxLength(50)]
    public string? IndicadorOferta { get; set; }
}
