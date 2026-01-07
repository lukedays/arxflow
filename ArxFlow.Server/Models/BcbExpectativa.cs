using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Entidade para armazenar expectativas do Banco Central (Focus IPCA Top5 mensal)
public class BcbExpectativa
{
    [Key]
    public int Id { get; set; }

    // Indicador (ex.: "IPCA")
    [Required]
    [MaxLength(50)]
    public string Indicador { get; set; } = string.Empty;

    // Data-base da coleta (ex.: "2025-12-30")
    [Required]
    public DateTime Data { get; set; }

    // Mes/ano referencia (ex.: "12/2025")
    [Required]
    [MaxLength(20)]
    public string DataReferencia { get; set; } = string.Empty;

    // Tipo de calculo (ex.: "C")
    [MaxLength(10)]
    public string? TipoCalculo { get; set; }

    // Media das expectativas
    public decimal? Media { get; set; }

    // Mediana das expectativas
    public decimal? Mediana { get; set; }

    // Desvio padrao
    public decimal? DesvioPadrao { get; set; }

    // Minimo
    public decimal? Minimo { get; set; }

    // Maximo
    public decimal? Maximo { get; set; }

    // Data/hora da importacao
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
