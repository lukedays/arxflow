namespace ArxFlow.Server.DTOs.YieldCurve;

public class YieldCurveResponse
{
    public DateTime Date { get; set; }
    public string CurveType { get; set; } = string.Empty;
    public List<YieldCurvePoint> Points { get; set; } = new();
}

public class YieldCurvePoint
{
    public int Days { get; set; }
    public decimal Rate { get; set; }
    public DateTime Maturity { get; set; }
    public string? Ticker { get; set; }
    public bool IsInterpolated { get; set; }
}
