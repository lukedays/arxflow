using System.ComponentModel.DataAnnotations;

namespace ArxFlow.Server.DTOs.Ativos;

public class UpdateAtivoRequest
{
    [Required(ErrorMessage = "Código do ativo é obrigatório")]
    [MaxLength(50)]
    public string CodAtivo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoAtivo { get; set; }

    public int? EmissorId { get; set; }

    [MaxLength(50)]
    public string? AlphaToolsId { get; set; }

    public DateTime? DataVencimento { get; set; }
}
