using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

/// <summary>
/// Informacoes de instrumentos de derivativos da B3
/// </summary>
public class B3InstrumentoDerivativo
{
    [Key]
    public int Id { get; set; }

    public DateTime DataReferencia { get; set; }

    [MaxLength(50)]
    public string InstrumentoFinanceiro { get; set; } = "";

    [MaxLength(20)]
    public string Ativo { get; set; } = "";

    [MaxLength(200)]
    public string? DescricaoAtivo { get; set; }

    [MaxLength(50)]
    public string? Segmento { get; set; }

    [MaxLength(50)]
    public string? Mercado { get; set; }

    [MaxLength(50)]
    public string? Categoria { get; set; }

    public DateTime? DataVencimento { get; set; }

    [MaxLength(10)]
    public string? CodigoExpiracao { get; set; }

    public DateTime? DataInicioNegocio { get; set; }

    public DateTime? DataFimNegocio { get; set; }

    [MaxLength(20)]
    public string? CodigoBase { get; set; }

    [MaxLength(100)]
    public string? CriterioConversao { get; set; }

    [MaxLength(50)]
    public string? DataMaturidadeAlvo { get; set; }

    [MaxLength(10)]
    public string? IndicadorConversao { get; set; }

    [MaxLength(20)]
    public string? CodigoIsin { get; set; }

    [MaxLength(10)]
    public string? CodigoCfi { get; set; }

    public DateTime? DataInicioAvisoEntrega { get; set; }

    public DateTime? DataFimAvisoEntrega { get; set; }

    [MaxLength(20)]
    public string? TipoOpcao { get; set; }

    public decimal? MultiplicadorContrato { get; set; }

    public decimal? QuantidadeAtivos { get; set; }

    public int? TamanhoLoteAlocacao { get; set; }

    [MaxLength(10)]
    public string? MoedaNegociada { get; set; }

    [MaxLength(50)]
    public string? TipoEntrega { get; set; }

    public int? DiasSaque { get; set; }

    public int? DiasUteis { get; set; }

    public int? DiasCorridos { get; set; }

    [MaxLength(50)]
    public string? PrecoBaseEstrategia { get; set; }

    public DateTime? DiasPosicaoFutura { get; set; }

    [MaxLength(10)]
    public string? CodigoTipoEstrategia1 { get; set; }

    [MaxLength(50)]
    public string? SimboloSubjacente1 { get; set; }

    [MaxLength(10)]
    public string? CodigoTipoEstrategia2 { get; set; }

    [MaxLength(50)]
    public string? SimboloSubjacente2 { get; set; }

    public decimal? PrecoExercicio { get; set; }

    [MaxLength(20)]
    public string? EstiloOpcao { get; set; }

    [MaxLength(50)]
    public string? TipoValor { get; set; }

    [MaxLength(10)]
    public string? IndicadorPremioAntecipado { get; set; }

    public DateTime? DataLimitePosicoesAbertas { get; set; }
}
