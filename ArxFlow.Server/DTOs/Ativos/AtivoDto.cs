namespace ArxFlow.Server.DTOs.Ativos;

public class AtivoDto
{
    public int Id { get; set; }
    public string CodAtivo { get; set; } = string.Empty;
    public string? TipoAtivo { get; set; }
    public int? EmissorId { get; set; }
    public string? EmissorNome { get; set; }
    public string? AlphaToolsId { get; set; }
    public DateTime? DataVencimento { get; set; }
}
