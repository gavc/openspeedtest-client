# OpenSpeedTest Client

A minimal, self-contained network speed test client for Windows with both GUI and CLI modes. Built with .NET 9 and published as a single-file, self-contained executable for zero-dependency deployment.

## Features

- **Dual Mode Operation**: GUI for interactive testing, CLI for automation and remote execution
- **Single-File Deployment**: Self-contained executable with no runtime dependencies
- **High-DPI Support**: Per-monitor DPI awareness for modern displays
- **Comprehensive Metrics**: Download/Upload speed, Ping, and Jitter measurements
- **PowerShell-Friendly**: JSON output for easy scripting and remote monitoring
- **Configurable**: JSON configuration file for server and test parameters
- **Multi-threaded Testing**: Parallel connections for accurate speed measurement

## Requirements

- Windows 10/11 (x64)
- OpenSpeedTest server (compatible with https://openspeedtest.com)

## Installation

1. Download the latest release from the [Releases](https://github.com/yourusername/OpenSpeedTestClient/releases) page
2. Extract the ZIP file
3. Edit `config.json` to point to your OpenSpeedTest server (optional)
4. Run `OpenSpeedTestClient.exe`

## Configuration

Edit `config.json` to customize test parameters:

```json
{
  "pingServer": "192.168.1.23",
  "testServerUrl": "http://192.168.1.23:3000",
  "uploadServerUrl": "http://192.168.1.23:3000",
  "downloadEndpoint": "/downloading",
  "uploadEndpoint": "/upload",
  "threads": 6,
  "downloadDuration": 10,
  "uploadDuration": 10,
  "pingSamples": 5,
  "pingTimeout": 5000,
  "uploadDataSizeMB": 10,
  "allowInsecureCerts": false
}
```

### Configuration Options

- **pingServer**: Hostname/IP to ping for latency measurement
- **testServerUrl**: Base URL used for downloads (HTTP or HTTPS)
- **uploadServerUrl**: Base URL used for uploads (falls back to `testServerUrl` when empty)
- **downloadEndpoint**: Path for download tests (default: `/downloading`)
- **uploadEndpoint**: Path for upload tests (default: `/upload`)
- **threads**: Parallel connections (1-32, default: 6)
- **downloadDuration**: Download test duration in seconds (5-300, default: 10)
- **uploadDuration**: Upload test duration in seconds (5-300, default: 10)
- **pingSamples**: Number of ping samples to collect (1-100, default: 5)
- **pingTimeout**: Ping request timeout in milliseconds (1000-30000, default: 5000)
- **uploadDataSizeMB**: Upload chunk size in megabytes (1-100, default: 10)
- **allowInsecureCerts**: Allow self-signed certificates for internal servers (default: false)

## Usage

### GUI Mode

Simply run the executable:

```powershell
.\OpenSpeedTestClient.exe
```

Click "Start Test" to begin testing. Results will display in real-time.

### CLI Mode

For automation and remote monitoring:

```powershell
# Basic test (silent, JSON output)
.\OpenSpeedTestClient.exe --cli

# Verbose output (progress to stderr, JSON to stdout)
.\OpenSpeedTestClient.exe --cli --verbose

# Custom config file
.\OpenSpeedTestClient.exe --cli --config "C:\path\to\config.json"
```

#### CLI Output Format

Success:
```json
{
  "Success": true,
  "ComputerName": "DESKTOP-01",
  "IP": "192.168.1.100",
  "ConnectionType": "Ethernet",
  "DownloadMbps": 500.25,
  "UploadMbps": 100.50,
  "PingMs": 12.0,
  "JitterMs": 3.0,
  "Server": "https://openspeedtest.com",
  "Timestamp": "2025-11-21T10:30:00Z"
}
```

Error:
```json
{
  "Success": false,
  "Error": "Connection timeout",
  "Timestamp": "2025-11-21T10:30:00Z"
}
```

Exit codes: `0` = success, `1` = failure

### PowerShell Examples

#### Local Testing
```powershell
$result = .\OpenSpeedTestClient.exe --cli | ConvertFrom-Json

if ($result.Success) {
    Write-Host "Download: $($result.DownloadMbps) Mbps"
    Write-Host "Upload: $($result.UploadMbps) Mbps"
    Write-Host "Ping: $($result.PingMs) ms"
}
```

#### Remote Testing
```powershell
$computers = @("Server01", "Server02", "Workstation01")

$results = foreach ($computer in $computers) {
    Invoke-Command -ComputerName $computer -ScriptBlock {
        C:\Tools\OpenSpeedTestClient.exe --cli | ConvertFrom-Json
    }
}

$results | Format-Table ComputerName, DownloadMbps, UploadMbps, PingMs
```

#### Scheduled Monitoring
```powershell
# Create scheduled task for hourly speed tests
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-Command `"C:\Tools\OpenSpeedTestClient.exe --cli | Out-File C:\Logs\speedtest.log -Append`""

$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours 1)

Register-ScheduledTask -TaskName "SpeedTest" -Action $action -Trigger $trigger
```

## Building from Source

### Prerequisites
- .NET 9 SDK or later
- Windows 10/11 SDK

### Build Instructions

```powershell
# Clone repository
git clone https://github.com/yourusername/OpenSpeedTestClient.git
cd OpenSpeedTestClient

# Restore dependencies
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Publish single-file executable (matches build.ps1)
dotnet publish src/OpenSpeedTestClient/OpenSpeedTestClient.csproj `
    -c Release `
    -r win-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true
```

The compiled executable will be in:
`src/OpenSpeedTestClient/bin/Release/net9.0-windows/win-x64/publish/`

## Architecture

```
OpenSpeedTestClient/
  src/
    OpenSpeedTestClient.Core/          # Core library
      Models/                          # Data models
        SpeedTestConfig.cs
        SpeedTestResult.cs
        TestProgress.cs
      Services/                        # Business logic
        ConfigService.cs               # Configuration & validation
        DownloadService.cs             # Download speed test
        PingService.cs                 # Ping & jitter measurement
        SpeedTestRunner.cs             # Test orchestration
        SystemInfoService.cs           # System information
        TransferTestHelper.cs          # Shared transfer sampling
        UploadService.cs               # Upload speed test
    OpenSpeedTestClient/               # Windows Forms UI & CLI
      MainForm.cs                      # GUI
      MainForm.Designer.cs
      Program.cs                       # Entry point & CLI
      AboutForm.cs                     # About dialog
      app.manifest                     # High-DPI configuration
      config.json                      # Default configuration
  .github/workflows/                   # CI/CD automation
```

## How It Works

1. **Ping Test**: Sends multiple HTTP requests to measure latency and jitter
2. **Download Test**: Creates parallel HTTP streams, measures bytes received over time
3. **Upload Test**: Generates random data, sends via parallel POST requests
4. **Speed Calculation**: Samples instantaneous speed every 200ms, averages for accuracy

The client replicates OpenSpeedTest's HTML5 methodology for consistent results across platforms.

## License

MIT License - see [LICENSE](LICENSE) file for details

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Support

For issues or questions:
- GitHub Issues: https://github.com/yourusername/OpenSpeedTestClient/issues
- OpenSpeedTest: https://openspeedtest.com

## Acknowledgments

- [OpenSpeedTest](https://openspeedtest.com) - HTML5 speed test server
- Built with .NET 9 single-file publish



