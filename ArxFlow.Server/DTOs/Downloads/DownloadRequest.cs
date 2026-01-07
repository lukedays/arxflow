using System.ComponentModel.DataAnnotations;

namespace ArxFlow.Server.DTOs.Downloads;

public class DownloadRequest
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
