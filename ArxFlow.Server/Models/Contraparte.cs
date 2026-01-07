using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArxFlow.Server.Models;

// Contraparte (corretora/banco)
public class Contraparte
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Nome { get; set; } = string.Empty;
}
