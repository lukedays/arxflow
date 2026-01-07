namespace ArxFlow.Server.DTOs.Ativos;

public class AtivoResponse
{
    public int Id { get; set; }
    public string CodAtivo { get; set; } = string.Empty;
    public string? TipoAtivo { get; set; }
    public int? EmissorId { get; set; }
    public string? EmissorNome { get; set; }
    public string? AlphaToolsId { get; set; }
    public DateTime? DataVencimento { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
