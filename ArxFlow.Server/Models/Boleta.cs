using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Entidade principal para registro de boletas
public class Boleta
{
    [Key]
    public int Id { get; set; }

    // Relacionamento com boleta principal (para boletas ligadas)
    public int? BoletaPrincipalId { get; set; }

    // Relacionamento com Ativo
    public int? AtivoId { get; set; }

    // Relacionamento com Contraparte
    public int? ContraparteId { get; set; }

    // Relacionamento com Fund
    public int? FundoId { get; set; }

    // Codigo do ativo
    [MaxLength(50)]
    public string? Ticker { get; set; }

    // C = Compra, V = Venda
    [MaxLength(1)]
    public string TipoOperacao { get; set; } = "C";

    // Volume em R$ milhoes
    public decimal Volume { get; set; }

    // Quantidade de contratos/papeis
    public decimal Quantidade { get; set; }

    // Tipo de precificacao: Spread ou Nominal
    public TipoPrecificacao TipoPrecificacao { get; set; }

    // Referencia NTNB (usado quando TipoPrecificacao = Spread)
    [MaxLength(20)]
    public string? NtnbReferencia { get; set; }

    // Valor do spread em bps (usado quando TipoPrecificacao = Spread)
    public decimal? SpreadValor { get; set; }

    // Data de fixing (usado quando TipoPrecificacao = Spread)
    public DateTime? DataFixing { get; set; }

    // Taxa nominal (usado quando TipoPrecificacao = Nominal)
    public decimal? TaxaNominal { get; set; }

    // Preco Unitario
    public decimal? PU { get; set; }

    // Campo livre para alocacao
    [MaxLength(100)]
    public string Alocacao { get; set; } = string.Empty;

    // Usuario que criou a boleta
    [MaxLength(100)]
    public string Usuario { get; set; } = string.Empty;

    // Observacoes adicionais
    public string Observacao { get; set; } = string.Empty;

    // Data de liquidacao
    public DateTime? DataLiquidacao { get; set; }

    // Status da boleta
    public StatusBoleta Status { get; set; } = StatusBoleta.AguardandoFixing;

    // Data de criacao do registro
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    // Navegacao
    [ForeignKey(nameof(AtivoId))]
    public virtual Ativo? Ativo { get; set; }

    [ForeignKey(nameof(ContraparteId))]
    public virtual Contraparte? Contraparte { get; set; }

    [ForeignKey(nameof(FundoId))]
    public virtual Fundo? Fundo { get; set; }

    [ForeignKey(nameof(BoletaPrincipalId))]
    public virtual Boleta? BoletaPrincipal { get; set; }

    public virtual ICollection<Boleta> BoletasLigadas { get; set; } = new List<Boleta>();
}

// Enum para tipo de precificacao
public enum TipoPrecificacao
{
    Nominal,
    Spread
}

// Enum para status da boleta
public enum StatusBoleta
{
    AguardandoFixing,
    AguardandoBoletagem,
    Boletada,
    Liquidada
}
