namespace ArxFlow.Server.DTOs.Calculadora;

// Response para cálculo de PU
public class CalculatePUResponse
{
    public decimal PU { get; set; }
    public decimal TaxaAno { get; set; }
    public int DiasUteis { get; set; }
    public int DiasCorridos { get; set; }
}
