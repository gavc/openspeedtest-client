namespace OpenSpeedTestClient.Core.Models;

public class TestProgress
{
    public TestPhase Phase { get; set; }
    public double Progress { get; set; }
    public double CurrentSpeed { get; set; }
    public string Status { get; set; } = string.Empty;
}

public enum TestPhase
{
    Ping,
    Download,
    Upload,
    Complete,
    Error
}
