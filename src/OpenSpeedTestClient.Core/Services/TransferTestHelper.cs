using System.Diagnostics;
using OpenSpeedTestClient.Core.Models;

namespace OpenSpeedTestClient.Core.Services;

internal static class TransferTestHelper
{
    internal sealed record TransferResult(double AverageMbps, long TotalBytes, int SampleCount);

    public static async Task<TransferResult> RunAsync(
        int threadCount,
        int durationSeconds,
        Func<CancellationToken, Task> workerFactory,
        Func<long> totalBytesAccessor,
        IProgress<TestProgress>? progress,
        TestPhase phase,
        Func<double, string> statusFormatter,
        CancellationToken cancellationToken)
    {
        using var testCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var testToken = testCts.Token;

        var speedSamples = new List<double>();
        var startTime = Stopwatch.GetTimestamp();
        var lastSampleTime = startTime;
        long lastSampleBytes = 0;

        var workerTasks = new List<Task>(threadCount);
        for (int i = 0; i < threadCount; i++)
        {
            workerTasks.Add(workerFactory(testToken));
        }

        var monitorTask = Task.Run(async () =>
        {
            while (!testToken.IsCancellationRequested)
            {
                await Task.Delay(200, testToken);

                var currentTime = Stopwatch.GetTimestamp();
                var elapsedTotal = Stopwatch.GetElapsedTime(startTime).TotalSeconds;

                if (elapsedTotal >= durationSeconds)
                {
                    break;
                }

                var elapsedSample = Stopwatch.GetElapsedTime(lastSampleTime).TotalSeconds;
                var currentBytes = totalBytesAccessor();
                var bytesDiff = currentBytes - lastSampleBytes;

                if (elapsedSample > 0 && bytesDiff > 0)
                {
                    var speedMbps = (bytesDiff * 8) / (elapsedSample * 1_000_000);
                    speedSamples.Add(speedMbps);

                    progress?.Report(new TestProgress
                    {
                        Phase = phase,
                        Progress = (elapsedTotal / durationSeconds) * 100,
                        CurrentSpeed = speedMbps,
                        Status = statusFormatter(speedMbps)
                    });

                    lastSampleTime = currentTime;
                    lastSampleBytes = currentBytes;
                }
            }
        }, testToken);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(durationSeconds), cancellationToken);
        }
        finally
        {
            testCts.Cancel();
        }

        // Await workers to surface faults; monitor cancellation is expected
        await Task.WhenAll(workerTasks);
        try
        {
            await monitorTask;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Expected when duration elapses
        }

        var totalBytes = totalBytesAccessor();
        var avgSpeed = speedSamples.Count > 0 ? speedSamples.Average() : 0;
        return new TransferResult(avgSpeed, totalBytes, speedSamples.Count);
    }
}
