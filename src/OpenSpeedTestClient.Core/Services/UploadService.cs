using System.Diagnostics;
using System.Security.Cryptography;
using OpenSpeedTestClient.Core.Models;

namespace OpenSpeedTestClient.Core.Services;

public class UploadService
{
    private readonly HttpClient _httpClient;
    private readonly SpeedTestConfig _config;
    private long _totalBytesUploaded;

    public UploadService(HttpClient httpClient, SpeedTestConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<double> MeasureUploadSpeedAsync(
        IProgress<TestProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _totalBytesUploaded = 0;
        var endpoint = $"{_config.ServerUrl}{_config.UploadEndpoint}";

        // Log the endpoint being used
        progress?.Report(new TestProgress
        {
            Phase = TestPhase.Upload,
            Progress = 0,
            Status = $"Testing: {endpoint}"
        });

        // Pre-generate upload data (shared across all threads for efficiency)
        var uploadData = GenerateRandomData(_config.UploadDataSizeMB * 1024 * 1024);

        progress?.Report(new TestProgress
        {
            Phase = TestPhase.Upload,
            Progress = 0,
            Status = $"Generated {uploadData.Length / (1024.0 * 1024.0):F2} MB of upload data"
        });

        var result = await TransferTestHelper.RunAsync(
            _config.Threads,
            _config.UploadDuration,
            token => UploadThreadAsync(endpoint, uploadData, token),
            () => Interlocked.Read(ref _totalBytesUploaded),
            progress,
            TestPhase.Upload,
            speed => $"Upload: {speed:F2} Mbps",
            cancellationToken);

        progress?.Report(new TestProgress
        {
            Phase = TestPhase.Upload,
            Progress = 100,
            Status = $"Uploaded {result.TotalBytes / (1024.0 * 1024.0):F2} MB, {result.SampleCount} samples"
        });

        return result.AverageMbps;
    }

    private byte[] GenerateRandomData(int size)
    {
        var data = new byte[size];
        RandomNumberGenerator.Fill(data);
        return data;
    }

    private async Task UploadThreadAsync(string endpoint, byte[] uploadData, CancellationToken cancellationToken)
    {
        int requestCount = 0;
        int successCount = 0;
        int errorCount = 0;
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                requestCount++;
                
                try
                {
                    using var content = new ByteArrayContent(uploadData);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    // Add query parameter like the browser does
                    var url = $"{endpoint}?n={Random.Shared.NextDouble()}";
                    
                    using var response = await _httpClient.PostAsync(url, content, cancellationToken);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        errorCount++;
                        var errorMsg = $"Upload failed: HTTP {(int)response.StatusCode} {response.ReasonPhrase} to {endpoint}";
                        System.Diagnostics.Debug.WriteLine(errorMsg);
                        
                        // Throw on first error to help diagnose
                        if (errorCount == 1)
                        {
                            throw new HttpRequestException(errorMsg, null, response.StatusCode);
                        }
                        
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    Interlocked.Add(ref _totalBytesUploaded, uploadData.Length);
                    successCount++;
                    System.Diagnostics.Debug.WriteLine($"Upload thread completed request {requestCount}: {uploadData.Length} bytes");
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    System.Diagnostics.Debug.WriteLine($"Upload request exception: {ex.Message}");
                    throw; // Re-throw to report the error
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when test completes
        }
        
        System.Diagnostics.Debug.WriteLine($"Upload thread finished: {requestCount} requests, {successCount} successful, {errorCount} errors");
    }
}
