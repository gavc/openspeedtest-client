using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSpeedTestClient.Core.Models;

namespace OpenSpeedTestClient.Core.Services;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(SpeedTestConfig))]
[JsonSerializable(typeof(SpeedTestResult))]
public partial class SpeedTestJsonContext : JsonSerializerContext
{
}

public class ConfigService
{
    private const string DefaultConfigPath = "config.json";

    public SpeedTestConfig LoadConfig(string? configPath = null)
    {
        var path = configPath ?? DefaultConfigPath;

        // If path is relative and file doesn't exist in current directory,
        // try looking in the application directory
        if (!Path.IsPathRooted(path) && !File.Exists(path))
        {
            var appDir = AppContext.BaseDirectory;
            var appConfigPath = Path.Combine(appDir, path);
            if (File.Exists(appConfigPath))
            {
                path = appConfigPath;
            }
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Configuration file not found: {path}");
        }

        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize(json, SpeedTestJsonContext.Default.SpeedTestConfig);

        if (config == null)
        {
            throw new InvalidOperationException("Failed to deserialize configuration file");
        }

        ValidateConfig(config);
        return config;
    }

    private void ValidateConfig(SpeedTestConfig config)
    {
        // Validate test server URL
        if (!Uri.TryCreate(config.TestServerUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid test server URL: {config.TestServerUrl}");
        }

        if (uri.Scheme != "http" && uri.Scheme != "https")
        {
            throw new ArgumentException($"Test server URL must use HTTP or HTTPS protocol: {config.TestServerUrl}");
        }
        
        // Validate ping server (just hostname)
        if (string.IsNullOrWhiteSpace(config.PingServer))
        {
            throw new ArgumentException("Ping server cannot be empty");
        }

        // Validate numeric ranges
        if (config.Threads < 1 || config.Threads > 32)
        {
            throw new ArgumentException($"Threads must be between 1 and 32, got: {config.Threads}");
        }

        if (config.DownloadDuration < 5 || config.DownloadDuration > 300)
        {
            throw new ArgumentException($"Download duration must be between 5 and 300 seconds, got: {config.DownloadDuration}");
        }

        if (config.UploadDuration < 5 || config.UploadDuration > 300)
        {
            throw new ArgumentException($"Upload duration must be between 5 and 300 seconds, got: {config.UploadDuration}");
        }

        if (config.PingSamples < 1 || config.PingSamples > 100)
        {
            throw new ArgumentException($"Ping samples must be between 1 and 100, got: {config.PingSamples}");
        }

        if (config.PingTimeout < 1000 || config.PingTimeout > 30000)
        {
            throw new ArgumentException($"Ping timeout must be between 1000 and 30000 milliseconds, got: {config.PingTimeout}");
        }

        if (config.UploadDataSizeMB < 1 || config.UploadDataSizeMB > 100)
        {
            throw new ArgumentException($"Upload data size must be between 1 and 100 MB, got: {config.UploadDataSizeMB}");
        }
    }

    public HttpClient CreateHttpClient(SpeedTestConfig config)
    {
        var handler = new HttpClientHandler();

        if (config.AllowInsecureCerts)
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }

        var client = new HttpClient(handler)
        {
            // Use a longer timeout for the overall HttpClient (30 seconds)
            // Individual ping requests will still be quick
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Add browser-like headers
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

        return client;
    }
}
