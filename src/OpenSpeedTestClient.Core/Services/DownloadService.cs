using System.Diagnostics;
using OpenSpeedTestClient.Core.Models;

namespace OpenSpeedTestClient.Core.Services;

public class DownloadService
{
    private readonly HttpClient _httpClient;
    private readonly SpeedTestConfig _config;
    private long _totalBytesDownloaded;

    public DownloadService(HttpClient httpClient, SpeedTestConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<double> MeasureDownloadSpeedAsync(
        IProgress<TestProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _totalBytesDownloaded = 0;
        var endpoint = $"{_config.TestServerUrl}{_config.DownloadEndpoint}";
        
        // Log the endpoint being used
        progress?.Report(new TestProgress
        {
            Phase = TestPhase.Download,
            Progress = 0,
            Status = $"Testing: {endpoint}"
        });

        var result = await TransferTestHelper.RunAsync(
            _config.Threads,
            _config.DownloadDuration,
            token => DownloadThreadAsync(endpoint, token),
            () => Interlocked.Read(ref _totalBytesDownloaded),
            progress,
            TestPhase.Download,
            speed => $"Download: {speed:F2} Mbps",
            cancellationToken);

        progress?.Report(new TestProgress
        {
            Phase = TestPhase.Download,
            Progress = 100,
            Status = $"Downloaded {result.TotalBytes / (1024.0 * 1024.0):F2} MB, {result.SampleCount} samples"
        });

        return result.AverageMbps;
    }

    private async Task DownloadThreadAsync(string endpoint, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        int requestCount = 0;
        int successCount = 0;
        int errorCount = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                requestCount++;
                var url = $"{endpoint}?n={Random.Shared.Next()}";
                
                try
                {
                    using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        errorCount++;
                        var errorMsg = $"Download failed: HTTP {(int)response.StatusCode} {response.ReasonPhrase} from {url}";
                        System.Diagnostics.Debug.WriteLine(errorMsg);
                        
                        // Throw on first error to help diagnose
                        if (errorCount == 1)
                        {
                            throw new HttpRequestException(errorMsg, null, response.StatusCode);
                        }
                        
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    
                    int bytesRead;
                    long totalBytesThisRequest = 0;
                    while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        Interlocked.Add(ref _totalBytesDownloaded, bytesRead);
                        totalBytesThisRequest += bytesRead;

                        if (cancellationToken.IsCancellationRequested)
                            break;
                    }
                    
                    if (totalBytesThisRequest > 0)
                    {
                        successCount++;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Download thread completed request {requestCount}: {totalBytesThisRequest} bytes");
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    System.Diagnostics.Debug.WriteLine($"Download request exception: {ex.Message}");
                    throw; // Re-throw to report the error
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when test completes
        }
        
        System.Diagnostics.Debug.WriteLine($"Download thread finished: {requestCount} requests, {successCount} successful, {errorCount} errors");
    }
}
