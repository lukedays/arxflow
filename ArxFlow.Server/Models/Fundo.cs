using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Fundo de investimento
public class Fundo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Cnpj { get; set; }

    [MaxLength(50)]
    public string? AlphaToolsId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegacao
    public virtual ICollection<Boleta> Boletas { get; set; } = [];
}
