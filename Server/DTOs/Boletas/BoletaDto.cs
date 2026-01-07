namespace ArxFlow.Server.DTOs.Boletas;

public class BoletaDto
{
    public int Id { get; set; }
    public int? BoletaPrincipalId { get; set; }
    public int? AtivoId { get; set; }
    public string? AtivoNome { get; set; }
    public int? ContraparteId { get; set; }
    public string? ContraparteNome { get; set; }
    public int? FundoId { get; set; }
    public string? FundoNome { get; set; }
    public string? Ticker { get; set; }
    public string TipoOperacao { get; set; } = "C";
    public decimal Volume { get; set; }
    public decimal Quantidade { get; set; }
    public string TipoPrecificacao { get; set; } = "Nominal";
    public string? NtnbReferencia { get; set; }
    public decimal? SpreadValor { get; set; }
    public DateTime? DataFixing { get; set; }
    public decimal? TaxaNominal { get; set; }
    public decimal? PU { get; set; }
    public string Alocacao { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;
    public DateTime? DataLiquidacao { get; set; }
    public string Status { get; set; } = "AguardandoFixing";
    public DateTime CriadoEm { get; set; }
}
