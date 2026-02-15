# CLI Quick Reference

## Basic Usage

```powershell
# GUI Mode (default)
.\OpenSpeedTestClient.exe

# CLI Mode (silent, JSON output to stdout)
.\OpenSpeedTestClient.exe --cli

# CLI Mode with verbose progress
.\OpenSpeedTestClient.exe --cli --verbose

# Custom configuration file
.\OpenSpeedTestClient.exe --cli --config "path\to\config.json"
```

## Command-Line Arguments

| Argument | Short | Description |
|----------|-------|-------------|
| `--cli` | `-c` | Run in CLI mode (headless) |
| `--verbose` | `-v` | Show progress messages to stderr |
| `--config <path>` | | Use custom config file path |

## Exit Codes

- `0` - Success
- `1` - Error (check JSON output for details)

## Output Format

### Success Output
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

### Error Output
```json
{
  "Success": false,
  "Error": "Connection timeout",
  "ComputerName": "",
  "IP": "",
  "ConnectionType": "",
  "DownloadMbps": 0.0,
  "UploadMbps": 0.0,
  "PingMs": 0.0,
  "JitterMs": 0.0,
  "Server": "",
  "Timestamp": "2025-11-21T10:30:00Z"
}
```

## PowerShell Examples

### Parse JSON Output
```powershell
$result = .\OpenSpeedTestClient.exe --cli | ConvertFrom-Json

if ($result.Success) {
    Write-Host "Download: $($result.DownloadMbps) Mbps"
    Write-Host "Upload: $($result.UploadMbps) Mbps"
    Write-Host "Ping: $($result.PingMs) ms"
    Write-Host "Jitter: $($result.JitterMs) ms"
} else {
    Write-Error "Test failed: $($result.Error)"
}
```

### Run on Multiple Computers
```powershell
$computers = @("PC01", "PC02", "PC03")

$results = Invoke-Command -ComputerName $computers -ScriptBlock {
    C:\Tools\OpenSpeedTestClient.exe --cli | ConvertFrom-Json
}

# Display results
$results | Format-Table PSComputerName, DownloadMbps, UploadMbps, PingMs, ConnectionType
```

### Export to CSV
```powershell
$result = .\OpenSpeedTestClient.exe --cli | ConvertFrom-Json
$result | Export-Csv -Path "speedtest-results.csv" -Append -NoTypeInformation
```

### Scheduled Monitoring
```powershell
# Run every hour and log results
while ($true) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $result = .\OpenSpeedTestClient.exe --cli | ConvertFrom-Json
    
    "$timestamp | Download: $($result.DownloadMbps) Mbps | Upload: $($result.UploadMbps) Mbps" | 
        Out-File -FilePath "speedtest-log.txt" -Append
    
    Start-Sleep -Seconds 3600  # Wait 1 hour
}
```

### Conditional Alerting
```powershell
$result = .\OpenSpeedTestClient.exe --cli | ConvertFrom-Json

# Alert if download speed below threshold
if ($result.DownloadMbps -lt 100) {
    Send-MailMessage -To "admin@company.com" `
        -From "monitoring@company.com" `
        -Subject "Low Speed Alert" `
        -Body "Download speed is $($result.DownloadMbps) Mbps on $($result.ComputerName)" `
        -SmtpServer "smtp.company.com"
}
```

### Remote Collection with Error Handling
```powershell
$computers = Get-Content "computers.txt"

foreach ($computer in $computers) {
    try {
        $result = Invoke-Command -ComputerName $computer -ScriptBlock {
            C:\Tools\OpenSpeedTestClient.exe --cli | ConvertFrom-Json
        } -ErrorAction Stop
        
        if ($result.Success) {
            [PSCustomObject]@{
                Computer = $computer
                Download = $result.DownloadMbps
                Upload = $result.UploadMbps
                Ping = $result.PingMs
                Type = $result.ConnectionType
                Status = "OK"
            }
        } else {
            [PSCustomObject]@{
                Computer = $computer
                Status = "Failed: $($result.Error)"
            }
        }
    }
    catch {
        [PSCustomObject]@{
            Computer = $computer
            Status = "Unreachable: $($_.Exception.Message)"
        }
    }
} | Export-Csv "network-report.csv" -NoTypeInformation
```

## Configuration File

Default location: `config.json` (same directory as executable)

```json
{
  "pingServer": "192.168.1.23",
  "serverUrl": "http://192.168.1.23:3000",
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

### Configuration Parameters

- **pingServer**: Hostname/IP to ping for latency measurement
- **serverUrl**: Base URL used for downloads (HTTP or HTTPS)
- **uploadServerUrl**: Optional base URL used for uploads (falls back to `serverUrl` when empty)
- **downloadEndpoint**: Download test endpoint (default: `/downloading`)
- **uploadEndpoint**: Upload test endpoint (default: `/upload`)
- **threads**: Parallel connections (1-32, default: 6)
- **downloadDuration**: Download test seconds (5-300, default: 10)
- **uploadDuration**: Upload test seconds (5-300, default: 10)
- **pingSamples**: Number of pings (1-100, default: 5)
- **pingTimeout**: Timeout in milliseconds (1000-30000, default: 5000)
- **uploadDataSizeMB**: Upload chunk size MB (1-100, default: 10)
- **allowInsecureCerts**: Allow self-signed certs (default: false)
- **testServerUrl**: Legacy alias for `serverUrl` (backward compatibility)

## Troubleshooting

### Common Issues

**Error: "Configuration file not found"**
- Ensure `config.json` is in the same directory as the executable
- Or specify path with `--config` parameter

**Error: "Invalid server URL"**
- Check URL format in config.json
- Ensure URL includes protocol (http:// or https://)

**Error: "Connection timeout"**
- Verify server is reachable
- Check firewall settings
- Increase `pingTimeout` in config.json

**Self-signed certificate error**
- Set `allowInsecureCerts: true` in config.json for internal servers
- Only use for trusted internal servers

### Debug Mode

Use `--verbose` to see detailed progress:

```powershell
.\OpenSpeedTestClient.exe --cli --verbose 2>&1
```

This shows progress messages while keeping JSON output clean on stdout.
