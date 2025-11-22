# Quick Start Guide

Get up and running with OpenSpeedTest Client in 5 minutes!

## For End Users

### Installation

1. **Download** the latest release from [Releases](https://github.com/yourusername/OpenSpeedTestClient/releases)
2. **Extract** the ZIP file to a folder (e.g., `C:\Tools\OpenSpeedTestClient`)
3. **Done!** No installation required.

### First Test - GUI Mode

1. Double-click `OpenSpeedTestClient.exe`
2. Click "Start Test"
3. Watch your network speed results appear!

### First Test - CLI Mode

Open PowerShell in the extracted folder:

```powershell
.\OpenSpeedTestClient.exe --cli --verbose
```

You'll see JSON output with your speed test results.

### Configure Your Server

Edit `config.json` to point to your own OpenSpeedTest server:

```json
{
  "pingServer": "192.168.1.23",
  "testServerUrl": "https://speedtest.yourcompany.com",
  "uploadServerUrl": "https://speedtest.yourcompany.com",
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

Save and restart the application.

## For IT Administrators

### Deploy to Multiple Computers

1. **Copy** the folder to a network share (e.g., `\\FileServer\Tools\OpenSpeedTestClient`)

2. **Create** a deployment script:

```powershell
# deploy-speedtest.ps1
$source = "\\FileServer\Tools\OpenSpeedTestClient"
$destination = "C:\Tools\OpenSpeedTestClient"

Copy-Item $source -Destination $destination -Recurse -Force

# Test installation
& "$destination\OpenSpeedTestClient.exe" --cli
```

3. **Deploy** via Group Policy or SCCM

### Remote Monitoring Script

Create a monitoring script:

```powershell
# monitor-network.ps1
$computers = Get-Content "computers.txt"

foreach ($computer in $computers) {
    $result = Invoke-Command -ComputerName $computer -ScriptBlock {
        C:\Tools\OpenSpeedTestClient\OpenSpeedTestClient.exe --cli
    } | ConvertFrom-Json
    
    [PSCustomObject]@{
        Computer = $computer
        Download = "$($result.DownloadMbps) Mbps"
        Upload = "$($result.UploadMbps) Mbps"
        Ping = "$($result.PingMs) ms"
        Connection = $result.ConnectionType
    }
} | Format-Table -AutoSize
```

Save as `monitor-network.ps1` and run:

```powershell
.\monitor-network.ps1
```

### Schedule Regular Tests

Create a scheduled task:

```powershell
$action = New-ScheduledTaskAction -Execute "C:\Tools\OpenSpeedTestClient\OpenSpeedTestClient.exe" -Argument "--cli"
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -TaskName "DailySpeedTest" -Action $action -Trigger $trigger
```

## For Developers

### Clone and Build

```powershell
# Clone repository
git clone https://github.com/yourusername/OpenSpeedTestClient.git
cd OpenSpeedTestClient

# Build
dotnet build

# Run
dotnet run --project src/OpenSpeedTestClient
```

### Build Native AOT Executable

```powershell
.\build.ps1 -Configuration Release
```

Output will be in `.\publish\`

### See Also

- [Developer Guide](docs/DEVELOPER-GUIDE.md) - Detailed development docs
- [CLI Reference](docs/CLI-REFERENCE.md) - Command-line usage
- [README.md](README.md) - Full documentation

## Troubleshooting

### "Config file not found"
**Solution**: Ensure `config.json` is in the same folder as `OpenSpeedTestClient.exe`

### "Cannot connect to server"
**Solution**: 
1. Check `testServerUrl`/`uploadServerUrl` in `config.json`
2. Verify firewall allows outbound connections
3. Test URL in browser first

### Self-signed certificate error
**Solution**: Set `"allowInsecureCerts": true` in `config.json` (internal servers only!)

### Slow speeds / Test hangs
**Solution**: 
1. Reduce `threads` in config (try 3-4)
2. Increase `pingTimeout` for slow connections
3. Check server is OpenSpeedTest compatible

## Getting Help

- **Issues**: https://github.com/yourusername/OpenSpeedTestClient/issues
- **Discussions**: https://github.com/yourusername/OpenSpeedTestClient/discussions
- **OpenSpeedTest**: https://openspeedtest.com

## Next Steps

- Read the [README](README.md) for full feature list
- Check [CLI Reference](docs/CLI-REFERENCE.md) for PowerShell automation
- Review [configuration options](README.md#configuration) for customization
- Join the community and share feedback!

---

Made for network administrators and power users

