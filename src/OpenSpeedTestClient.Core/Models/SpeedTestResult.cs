namespace OpenSpeedTestClient.Core.Models;

public class SpeedTestResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string ComputerName { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
    public double DownloadMbps { get; set; }
    public double UploadMbps { get; set; }
    public double PingMs { get; set; }
    public double JitterMs { get; set; }
    public string Server { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
