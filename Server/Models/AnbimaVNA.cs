using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Entidade para armazenar dados de VNA (Valor Nominal Atualizado) de Titulos Publicos
public class AnbimaVNA
{
    [Key]
    public int Id { get; set; }

    // Tipo do titulo (ex.: "NTN-B", "NTN-C")
    [Required]
    [MaxLength(50)]
    public string Titulo { get; set; } = string.Empty;

    // Codigo SELIC do titulo
    [MaxLength(20)]
    public string? CodigoSelic { get; set; }

    // Data de referencia do VNA
    [Required]
    public DateTime DataReferencia { get; set; }

    // Valor Nominal Atualizado (VNA)
    [Required]
    public decimal VNA { get; set; }

    // Indice correspondente (IPCA, IGP-M, etc.)
    public decimal? Indice { get; set; }

    // Referencia do indice (mes/ano)
    [MaxLength(20)]
    public string? Referencia { get; set; }

    // Valido a partir de (data inicio da validade)
    public DateTime? ValidoAPartirDe { get; set; }

    // Data/hora da importacao
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
