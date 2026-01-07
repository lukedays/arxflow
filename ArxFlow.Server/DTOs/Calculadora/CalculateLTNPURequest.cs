using System.ComponentModel.DataAnnotations;

namespace ArxFlow.Server.DTOs.Calculadora;

// Request para calcular PU da LTN
public class CalculateLTNPURequest
{
    [Required]
    public DateTime DataCotacao { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal TaxaAno { get; set; }
}
