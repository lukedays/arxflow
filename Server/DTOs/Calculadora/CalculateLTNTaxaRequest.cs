using System.ComponentModel.DataAnnotations;

namespace ArxFlow.Server.DTOs.Calculadora;

// Request para calcular Taxa da LTN
public class CalculateLTNTaxaRequest
{
    [Required]
    public DateTime DataCotacao { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }

    [Required]
    [Range(0, 10000)]
    public decimal PU { get; set; }
}
