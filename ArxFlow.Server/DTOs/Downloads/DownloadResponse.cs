namespace ArxFlow.Server.DTOs.Downloads;

public class DownloadResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public List<string> Errors { get; set; } = new();
}
