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

        NormalizeConfig(config);
        ValidateConfig(config);
        return config;
    }

    private static void NormalizeConfig(SpeedTestConfig config)
    {
        // Backward compatibility for older docs/configs.
        if (string.IsNullOrWhiteSpace(config.ServerUrl) && !string.IsNullOrWhiteSpace(config.TestServerUrl))
        {
            config.ServerUrl = config.TestServerUrl;
        }

        if (!string.IsNullOrWhiteSpace(config.ServerUrl))
        {
            config.ServerUrl = config.ServerUrl.TrimEnd('/');
        }

        if (!string.IsNullOrWhiteSpace(config.UploadServerUrl))
        {
            config.UploadServerUrl = config.UploadServerUrl.TrimEnd('/');
        }
    }

    private void ValidateConfig(SpeedTestConfig config)
    {
        // Validate server URLs
        var downloadServerUri = ValidateServerUrl(config.ServerUrl, "Server URL");
        var uploadServerUrl = string.IsNullOrWhiteSpace(config.UploadServerUrl) ? config.ServerUrl : config.UploadServerUrl;
        var uploadServerUri = ValidateServerUrl(uploadServerUrl!, "Upload server URL");

        // Ensure endpoints cannot redirect to another host
        ValidateEndpoint(config.DownloadEndpoint, downloadServerUri, "Download endpoint");
        ValidateEndpoint(config.UploadEndpoint, uploadServerUri, "Upload endpoint");
        
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

    private static Uri ValidateServerUrl(string url, string name)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid {name}: {url}");
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"{name} must use HTTP or HTTPS protocol: {url}");
        }

        return uri;
    }

    private static void ValidateEndpoint(string endpoint, Uri baseUri, string name)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException($"{name} cannot be empty");
        }

        // Must be a relative path so it cannot change the host/scheme
        if (!Uri.TryCreate(endpoint, UriKind.Relative, out var relativeUri) || relativeUri.IsAbsoluteUri)
        {
            throw new ArgumentException($"{name} must be a relative path, got: {endpoint}");
        }

        if (endpoint.StartsWith("//"))
        {
            throw new ArgumentException($"{name} cannot start with '//' because it would override the host");
        }

        if (!Uri.TryCreate(baseUri, relativeUri, out var combined))
        {
            throw new ArgumentException($"Invalid {name} for base URL {baseUri}: {endpoint}");
        }

        if (!string.Equals(combined.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{name} must stay on host {baseUri.Host}, got {combined.Host}");
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
            // Let test-scoped CancellationToken control duration.
            Timeout = Timeout.InfiniteTimeSpan
        };

        // Add browser-like headers
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

        return client;
    }
}
