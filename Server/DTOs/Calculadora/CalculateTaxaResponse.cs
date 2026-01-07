namespace ArxFlow.Server.DTOs.Calculadora;

// Response para cálculo de Taxa
public class CalculateTaxaResponse
{
    public decimal TaxaAno { get; set; }
    public decimal PU { get; set; }
    public int DiasUteis { get; set; }
    public int DiasCorridos { get; set; }
}
