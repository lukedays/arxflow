using System.ComponentModel.DataAnnotations;

namespace ArxFlow.Server.DTOs.Boletas;

public class UpdateBoletaRequest
{
    public int? BoletaPrincipalId { get; set; }
    public int? AtivoId { get; set; }
    public int? ContraparteId { get; set; }
    public int? FundoId { get; set; }

    [MaxLength(50)]
    public string? Ticker { get; set; }

    [Required]
    [MaxLength(1)]
    public string TipoOperacao { get; set; } = "C";

    public decimal Volume { get; set; }
    public decimal Quantidade { get; set; }

    [Required]
    public string TipoPrecificacao { get; set; } = "Nominal";

    [MaxLength(20)]
    public string? NtnbReferencia { get; set; }

    public decimal? SpreadValor { get; set; }
    public DateTime? DataFixing { get; set; }
    public decimal? TaxaNominal { get; set; }
    public decimal? PU { get; set; }

    [MaxLength(100)]
    public string Alocacao { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Usuario { get; set; } = string.Empty;

    public string Observacao { get; set; } = string.Empty;
    public DateTime? DataLiquidacao { get; set; }
    public string Status { get; set; } = "AguardandoFixing";
}
