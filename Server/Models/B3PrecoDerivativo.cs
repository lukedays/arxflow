using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Entidade para armazenar precos de derivativos
public class B3PrecoDerivativo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Ticker { get; set; } = string.Empty;

    [Required]
    public DateTime DataReferencia { get; set; }

    // Posicao em aberto
    public long? ContratosEmAberto { get; set; }

    // Primeiro preco do dia
    public decimal? PrecoInicial { get; set; }

    // Preco minimo do dia
    public decimal? PrecoMinimo { get; set; }

    // Preco maximo do dia
    public decimal? PrecoMaximo { get; set; }

    // Preco medio ponderado
    public decimal? PrecoMedioPonderado { get; set; }

    // Ultimo preco negociado
    public decimal? UltimoPreco { get; set; }

    // Quantidade de negocios regulares
    public long? QuantidadeNegociosRegulares { get; set; }

    // Preco de ajuste (PU)
    public decimal? PrecoAjuste { get; set; }

    // Taxa de ajuste
    public decimal? TaxaAjuste { get; set; }

    // Situacao do preco de ajuste (F = Final, P = Preliminar)
    [MaxLength(10)]
    public string? SituacaoPrecoAjuste { get; set; }

    // Preco de ajuste do dia anterior
    public decimal? PrecoAjusteAnterior { get; set; }

    // Taxa de ajuste do dia anterior
    public decimal? TaxaAjusteAnterior { get; set; }

    // Situacao do preco de ajuste anterior
    [MaxLength(10)]
    public string? SituacaoPrecoAjusteAnterior { get; set; }

    // Moeda
    [MaxLength(10)]
    public string? Moeda { get; set; }

    // Data/hora da importacao
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
