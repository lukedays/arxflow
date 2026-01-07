using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Ativo negociavel
public class Ativo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CodAtivo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoAtivo { get; set; }

    public int? EmissorId { get; set; }

    [MaxLength(50)]
    public string? AlphaToolsId { get; set; }

    public DateTime? DataVencimento { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    // Navegacao
    [ForeignKey(nameof(EmissorId))]
    public virtual Emissor? Issuer { get; set; }

    public virtual ICollection<Boleta> Boletas { get; set; } = [];
}
