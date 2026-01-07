using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Entidade para armazenar dados do Mercado Secundario de Titulos Publicos Federais (ANBIMA)
public class AnbimaTPF
{
    [Key]
    public int Id { get; set; }

    // Tipo do titulo (ex.: "LTN", "NTN-B", "NTN-C", "LFT")
    [Required]
    [MaxLength(50)]
    public string Titulo { get; set; } = string.Empty;

    // Data de referencia do arquivo
    [Required]
    public DateTime DataReferencia { get; set; }

    // Codigo SELIC do titulo
    [MaxLength(20)]
    public string? CodigoSelic { get; set; }

    // Data base/emissao
    public DateTime? DataBase { get; set; }

    // Data de vencimento
    public DateTime? DataVencimento { get; set; }

    // Taxa de compra (% a.a.)
    public decimal? TaxaCompra { get; set; }

    // Taxa de venda (% a.a.)
    public decimal? TaxaVenda { get; set; }

    // Taxa indicativa (% a.a.)
    public decimal? TaxaIndicativa { get; set; }

    // Preco Unitario (PU)
    public decimal? PU { get; set; }

    // Desvio padrao
    public decimal? DesvioPadrao { get; set; }

    // Intervalo indicativo minimo (D0)
    public decimal? IntervaloIndMinD0 { get; set; }

    // Intervalo indicativo maximo (D0)
    public decimal? IntervaloIndMaxD0 { get; set; }

    // Intervalo indicativo minimo (D+1)
    public decimal? IntervaloIndMinD1 { get; set; }

    // Intervalo indicativo maximo (D+1)
    public decimal? IntervaloIndMaxD1 { get; set; }

    // Criterio
    [MaxLength(50)]
    public string? Criterio { get; set; }

    // Data/hora da importacao
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
