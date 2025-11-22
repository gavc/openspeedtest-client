using System.Net.NetworkInformation;
using OpenSpeedTestClient.Core.Models;

namespace OpenSpeedTestClient.Core.Services;

public class PingService
{
    private readonly SpeedTestConfig _config;

    public PingService(SpeedTestConfig config)
    {
        _config = config;
    }

    public async Task<(double MinPingMs, double AvgJitterMs)> MeasurePingAndJitterAsync(
        IProgress<TestProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var pingResults = new List<double>();
        
        // Use the dedicated ping server
        var hostname = _config.PingServer;

        using var pingSender = new Ping();
        var buffer = new byte[32]; // Standard ping data buffer

        for (int i = 0; i < _config.PingSamples; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var reply = await pingSender.SendPingAsync(hostname, _config.PingTimeout, buffer);

                if (reply.Status == IPStatus.Success)
                {
                    var pingMs = (double)reply.RoundtripTime;
                    pingResults.Add(pingMs);

                    progress?.Report(new TestProgress
                    {
                        Phase = TestPhase.Ping,
                        Progress = (i + 1) / (double)_config.PingSamples * 100,
                        CurrentSpeed = pingMs,
                        Status = $"Ping: {pingMs:F1} ms ({i + 1}/{_config.PingSamples})"
                    });
                }
                else
                {
                    progress?.Report(new TestProgress
                    {
                        Phase = TestPhase.Ping,
                        Progress = (i + 1) / (double)_config.PingSamples * 100,
                        CurrentSpeed = 0,
                        Status = $"Ping failed: {reply.Status} ({i + 1}/{_config.PingSamples})"
                    });
                }

                // Small delay between pings
                await Task.Delay(100, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the error for debugging with full details
                var errorMsg = ex.InnerException != null 
                    ? $"{ex.Message} - {ex.InnerException.Message}" 
                    : ex.Message;
                
                progress?.Report(new TestProgress
                {
                    Phase = TestPhase.Ping,
                    Progress = (i + 1) / (double)_config.PingSamples * 100,
                    CurrentSpeed = 0,
                    Status = $"Ping failed: {errorMsg}"
                });
                // Skip failed pings but continue trying
                continue;
            }
        }

        if (pingResults.Count == 0)
        {
            throw new InvalidOperationException($"Failed to collect any ping samples. Verify server '{_config.PingServer}' is reachable.");
        }

        var minPing = pingResults.Min();
        var jitter = CalculateJitter(pingResults);

        return (minPing, jitter);
    }

    private double CalculateJitter(List<double> pingResults)
    {
        if (pingResults.Count < 2)
            return 0;

        var jitterValues = new List<double>();

        for (int i = 1; i < pingResults.Count; i++)
        {
            var diff = Math.Abs(pingResults[i] - pingResults[i - 1]);
            jitterValues.Add(diff);
        }

        return jitterValues.Count > 0 ? jitterValues.Average() : 0;
    }
}
