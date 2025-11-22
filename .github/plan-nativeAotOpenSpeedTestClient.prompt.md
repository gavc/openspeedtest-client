# Native AOT OpenSpeedTest Client with Windows Forms

Build a minimal self-contained OpenSpeedTest client using .NET 8+ Native AOT with high-DPI Windows Forms, replicating the HTML5 client's multi-threaded test methodology with a packaged `config.json` configuration file.

## Steps

### 1. Create .NET 8 Native AOT solution
Initialize `OpenSpeedTestClient.sln` with `OpenSpeedTestClient.Core` (class library) and `OpenSpeedTestClient` (Windows Forms app), configure `.csproj` with `<PublishAot>true</PublishAot>`, `<InvariantGlobalization>false</InvariantGlobalization>`, add `app.manifest` with `<dpiAwareness>PerMonitorV2</dpiAwareness>`, create `config.json` (Content/CopyToOutputDirectory) with OpenSpeedTest defaults:
- `serverUrl` (`https://openspeedtest.com`)
- `downloadEndpoint` (`/downloading`)
- `uploadEndpoint` (`/upload`)
- `threads` (6)
- `downloadDuration` (10)
- `uploadDuration` (10)
- `pingSamples` (10)
- `pingTimeout` (5000)
- `uploadDataSizeMB` (10)
- `allowInsecureCerts` (false)

### 2. Implement configuration service with validation
Create `ConfigService.cs` using `JsonSerializerContext` source generator for AOT-safe deserialization, validate `config.json` exists on startup (throw if missing), validate URL format with `Uri.TryCreate()` enforcing absolute URIs (support both HTTP/HTTPS), validate numeric ranges (threads 1-32, durations 5-300, timeout 1000-30000), return typed `Config` model, configure `HttpClient` with `HttpClientHandler.ServerCertificateCustomValidationCallback` based on `allowInsecureCerts` flag for internal server support

### 3. Implement ping/jitter measurement
Create `PingService.cs` using `HttpClient` with `Stopwatch.GetTimestamp()`, send `pingSamples` sequential GET requests to `{serverUrl}{downloadEndpoint}?n={Random.Shared.Next()}`, calculate milliseconds via `Stopwatch.GetElapsedTime()`, compute jitter as average of `Math.Abs(ping[i] - ping[i-1])`, return minimum ping and average jitter

### 4. Implement download speed test
Build `DownloadService.cs` spawning `threads` concurrent `Task` instances calling `HttpClient.GetAsync(HttpCompletionOption.ResponseHeadersRead)`, stream response to `Stream.Null` via `CopyToAsync()`, accumulate bytes across threads using `Interlocked.Add()`, sample speed every 200ms storing `(totalBytes * 8) / (elapsedSeconds * 1_000_000)` in list, run for `downloadDuration` seconds, return average of all speed samples

### 5. Implement upload speed test
Create `UploadService.cs` pre-generating single shared `uploadDataSizeMB` MB buffer using `RandomNumberGenerator.Fill()`, spawn `threads` parallel tasks each looping `PostAsync(new ByteArrayContent(buffer))` to `{serverUrl}{uploadEndpoint}` for `uploadDuration` seconds, track uploaded bytes with `Interlocked.Add()`, sample speed every 200ms, return average of samples

### 6. Build dual-mode Program.cs
Parse CLI args (`--cli`, `--config <path>`, `--verbose`): CLI mode executes sequential ping → download → upload, silent outputs JSON `{"Success":true,"ComputerName":"PC01","IP":"192.168.1.10","ConnectionType":"Ethernet","DownloadMbps":500.25,"UploadMbps":100.50,"PingMs":12,"JitterMs":3,"Server":"https://openspeedtest.com","Timestamp":"2025-11-21T10:30:00Z"}` to stdout (errors as `{"Success":false,"Error":"message"}`), `--verbose` writes progress to stderr, exit 0 on success or 1 on failure; GUI mode calls `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)`, `Application.EnableVisualStyles()`, launches `MainForm`

### 7. Design high-DPI Windows Forms UI
Create `MainForm.cs` with `AutoScaleMode.Dpi`, `TableLayoutPanel` layout, server URL Label (read-only display from config), Start/Stop Button, ProgressBar for test phases, results GroupBox with Labels showing `Environment.MachineName`, local IP from first operational `NetworkInterface`, connection type via `NetworkInterfaceType` (Ethernet/Wireless80211/Wwanpp mapped to LAN/WiFi/4G), DownloadMbps, UploadMbps, PingMs, JitterMs updated thread-safe via `IProgress<TestProgress>` from async test execution

## Considerations

This plan mirrors OpenSpeedTest's methodology with support for both internal (self-signed cert) and external servers, packaged config file with validation, and dual GUI/CLI modes optimized for Windows deployment with Native AOT compilation.
