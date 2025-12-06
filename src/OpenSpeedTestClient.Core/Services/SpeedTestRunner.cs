using OpenSpeedTestClient.Core.Models;

namespace OpenSpeedTestClient.Core.Services;

public class SpeedTestRunner
{
    private readonly SpeedTestConfig _config;
    private readonly HttpClient _httpClient;
    private readonly SystemInfoService _systemInfoService;

    public SpeedTestRunner(SpeedTestConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
        _systemInfoService = new SystemInfoService();
    }

    public async Task<SpeedTestResult> RunTestAsync(
        IProgress<TestProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new SpeedTestResult
        {
            Success = true,
            ComputerName = _systemInfoService.GetComputerName(),
            IP = _systemInfoService.GetLocalIPAddress(),
            ConnectionType = _systemInfoService.GetConnectionType(),
            Server = _config.ServerUrl,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Ping Test
            progress?.Report(new TestProgress
            {
                Phase = TestPhase.Ping,
                Progress = 0,
                Status = "Starting ping test..."
            });

            var pingService = new PingService(_config);
            var (minPing, avgJitter) = await pingService.MeasurePingAndJitterAsync(progress, cancellationToken);
            result.PingMs = Math.Round(minPing, 1);
            result.JitterMs = Math.Round(avgJitter, 1);

            // Download Test
            progress?.Report(new TestProgress
            {
                Phase = TestPhase.Download,
                Progress = 0,
                Status = "Starting download test..."
            });

            var downloadService = new DownloadService(_httpClient, _config);
            var downloadSpeed = await downloadService.MeasureDownloadSpeedAsync(progress, cancellationToken);
            result.DownloadMbps = Math.Round(downloadSpeed, 2);

            // Upload Test
            progress?.Report(new TestProgress
            {
                Phase = TestPhase.Upload,
                Progress = 0,
                Status = "Starting upload test..."
            });

            var uploadService = new UploadService(_httpClient, _config);
            var uploadSpeed = await uploadService.MeasureUploadSpeedAsync(progress, cancellationToken);
            result.UploadMbps = Math.Round(uploadSpeed, 2);

            progress?.Report(new TestProgress
            {
                Phase = TestPhase.Complete,
                Progress = 100,
                Status = "Test complete!"
            });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;

            progress?.Report(new TestProgress
            {
                Phase = TestPhase.Error,
                Progress = 0,
                Status = $"Error: {ex.Message}"
            });
        }

        return result;
    }
}
