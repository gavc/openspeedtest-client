namespace OpenSpeedTestClient.Core.Models;

public class SpeedTestConfig
{
    public string PingServer { get; set; } = string.Empty;
    public string TestServerUrl { get; set; } = string.Empty;
    public string UploadServerUrl { get; set; } = string.Empty;
    public string DownloadEndpoint { get; set; } = string.Empty;
    public string UploadEndpoint { get; set; } = string.Empty;
    public int Threads { get; set; }
    public int DownloadDuration { get; set; }
    public int UploadDuration { get; set; }
    public int PingSamples { get; set; }
    public int PingTimeout { get; set; }
    public int UploadDataSizeMB { get; set; }
    public bool AllowInsecureCerts { get; set; }
}
