using System.ComponentModel.DataAnnotations;

namespace ArxFlow.Server.DTOs.YieldCurve;

public class YieldCurveRequest
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MaxLength(10)]
    public string CurveType { get; set; } = "DI1"; // DI1, PRE, IPCA

    [MaxLength(20)]
    public string Interpolation { get; set; } = "Linear"; // Linear, FlatForward
}
