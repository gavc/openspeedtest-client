# Developer Guide

## Prerequisites

- **Windows 10/11** (x64)
- **.NET 9 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Visual Studio 2022** (optional, recommended) or **VS Code**
- **Git** for version control

## Getting Started

### 1. Clone the Repository

```powershell
git clone https://github.com/yourusername/OpenSpeedTestClient.git
cd OpenSpeedTestClient
```

### 2. Restore Dependencies

```powershell
dotnet restore
```

### 3. Build the Solution

```powershell
# Debug build
dotnet build

# Release build
dotnet build --configuration Release
```

### 4. Run the Application

#### GUI Mode
```powershell
dotnet run --project src/OpenSpeedTestClient/OpenSpeedTestClient.csproj
```

#### CLI Mode
```powershell
dotnet run --project src/OpenSpeedTestClient/OpenSpeedTestClient.csproj -- --cli --verbose
```

## Project Structure

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
      Program.cs                       # Entry point & CLI logic
      MainForm.cs / MainForm.Designer.cs
      AboutForm.cs
      app.manifest                     # High-DPI configuration
      config.json                      # Default configuration
  docs/                                # Documentation
  .github/workflows/                   # CI/CD automation
  build.ps1                            # Local publish script
  README.md, LICENSE, CHANGELOG.md     # Top-level docs
```

## Architecture Overview

### Core Library (`OpenSpeedTestClient.Core`)

The core library is designed to be AOT-compatible using:
- **Source Generators** for JSON serialization (`System.Text.Json`)
- **No Reflection** - all types known at compile time
- **Minimal Dependencies** - only .NET BCL

#### Services

1. **ConfigService**
   - Loads and validates `config.json`
   - Creates configured `HttpClient` instances
   - Validates URL format and parameter ranges

2. **SystemInfoService**
   - Retrieves computer name (`Environment.MachineName`)
   - Detects local IP address via `NetworkInterface`
   - Determines connection type (Ethernet/WiFi/4G)

3. **PingService**
   - Measures latency using HTTP requests
   - Calculates jitter from consecutive ping samples
   - Uses `Stopwatch.GetTimestamp()` for precision

4. **DownloadService**
   - Multi-threaded download testing
   - Streams data to `Stream.Null` for accurate measurement
   - Samples speed every 200ms for average calculation

5. **UploadService**
   - Pre-generates random data using `RandomNumberGenerator`
   - Multi-threaded upload via POST requests
   - Tracks bytes uploaded with `Interlocked.Add()`

6. **SpeedTestRunner**
   - Orchestrates the full test sequence
   - Reports progress via `IProgress<TestProgress>`
   - Returns comprehensive `SpeedTestResult`

### UI Application (`OpenSpeedTestClient`)

Windows Forms application with dual-mode support:

- **GUI Mode**: Interactive testing with real-time updates
- **CLI Mode**: Silent JSON output for automation

#### Program.cs Flow

```
Main(args)
  ├─ Check for --cli flag
  │   ├─ Yes → RunCliMode(args)
  │   │   ├─ Load configuration
  │   │   ├─ Create progress reporter (if --verbose)
  │   │   ├─ Run SpeedTestRunner
  │   │   └─ Output JSON to stdout
  │   └─ No → Launch GUI
  │       ├─ SetHighDpiMode(PerMonitorV2)
  │       ├─ EnableVisualStyles()
  │       └─ Run(new MainForm())
```

## Development Workflow

### Making Changes

1. Create a feature branch
2. Make your changes
3. Build and test locally
4. Commit with descriptive messages
5. Push and create pull request

### Testing

#### Manual Testing - GUI
```powershell
dotnet run --project src/OpenSpeedTestClient
```

#### Manual Testing - CLI
```powershell
# Test CLI mode
dotnet run --project src/OpenSpeedTestClient -- --cli --verbose

# Test with custom config
dotnet run --project src/OpenSpeedTestClient -- --cli --config test-config.json
```

### Building Single-File Executable

Use the provided build script:

```powershell
# Release build (single-file, self-contained, compressed)
.\build.ps1 -Configuration Release

# Clean build
.\build.ps1 -Configuration Release -Clean

# Custom output path
.\build.ps1 -OutputPath "C:\Builds\OSTC"
```

Or manually:

```powershell
dotnet publish src/OpenSpeedTestClient/OpenSpeedTestClient.csproj `
    -c Release `
    -r win-x64 `
    --self-contained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o publish
```

## Adding New Features

### Adding a New Service

1. Create service class in `src/OpenSpeedTestClient.Core/Services/`
2. Ensure AOT compatibility (no reflection, use source generators)
3. Add to `SpeedTestRunner` if needed
4. Update models if new data is captured

Example:

```csharp
namespace OpenSpeedTestClient.Core.Services;

public class MyNewService
{
    private readonly HttpClient _httpClient;
    
    public MyNewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> DoSomethingAsync(CancellationToken cancellationToken = default)
    {
        // Implementation
        return "result";
    }
}
```

### Adding Configuration Options

1. Update `SpeedTestConfig.cs` model
2. Add validation in `ConfigService.ValidateConfig()`
3. Update default `config.json`
4. Document in README and CLI-REFERENCE

Example:

```csharp
public class SpeedTestConfig
{
    // Existing properties...
    
    public int MyNewSetting { get; set; } = 42;
}
```

Then in `ConfigService`:

```csharp
private void ValidateConfig(SpeedTestConfig config)
{
    // Existing validation...
    
    if (config.MyNewSetting < 1 || config.MyNewSetting > 100)
    {
        throw new ArgumentException($"MyNewSetting must be between 1 and 100");
    }
}
```

### Updating the UI

The Windows Forms UI is split between:
- `MainForm.cs` - Logic and event handlers
- `MainForm.Designer.cs` - UI layout and controls

To add new controls:

1. Update `MainForm.Designer.cs` with new controls
2. Add event handlers in `MainForm.cs`
3. Use `InvokeRequired` pattern for thread safety

## AOT Compatibility Guidelines

### DO ✅
- Use source generators for JSON serialization
- Use `typeof()` for known types at compile time
- Use `NetworkInterface` for network detection
- Use `Stopwatch` for timing
- Use `Interlocked` for thread-safe counters

### DON'T ❌
- Avoid `Reflection` (GetType(), Assembly.Load(), etc.)
- Avoid dynamic code generation
- Avoid COM interop where possible
- Avoid P/Invoke without `[UnmanagedCallersOnly]`

### JSON Serialization (AOT-Safe)

Always use the source-generated context:

```csharp
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MyType))]
public partial class MyJsonContext : JsonSerializerContext { }

// Usage
var json = JsonSerializer.Serialize(myObject, MyJsonContext.Default.MyType);
var obj = JsonSerializer.Deserialize(json, MyJsonContext.Default.MyType);
```

## Debugging

### Debug Native AOT Issues

If the application works in Debug but fails in AOT publish:

1. Check for reflection usage
2. Verify all JSON types have source generation
3. Review trimming warnings
4. Test with `dotnet publish` locally

### Enable Verbose Logging

Add logging to services for debugging:

```csharp
public async Task<double> MeasureDownloadSpeedAsync(...)
{
    Console.Error.WriteLine($"Starting download test with {_config.Threads} threads");
    // ... rest of method
}
```

In CLI mode with `--verbose`, these will appear in stderr.

## Performance Considerations

### Multi-threading

- Default to 6 threads (matches OpenSpeedTest)
- Use `Interlocked.Add()` for thread-safe counters
- Avoid locks in hot paths
- Use `CancellationToken` for cancellation

### Memory Management

- Reuse upload buffer across threads
- Stream downloads to `Stream.Null` (don't buffer)
- Dispose `HttpClient` properly
- Use `using` statements for disposable resources

### Speed Sampling

Sample every 200ms for balance:
- Too frequent: overhead affects results
- Too infrequent: misses speed fluctuations

## Troubleshooting

### Build Errors

**"Windows Forms is not supported with trimming"**
- Ensure `<PublishTrimmed>false</PublishTrimmed>` is set

**"JsonSerializerContext is inaccessible"**
- Make the context class `public` not `internal`

### Runtime Errors

**"Configuration file not found"**
- Ensure `config.json` has `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`

**Certificate validation errors**
- Set `allowInsecureCerts: true` in config for internal servers

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style

- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Reference issues and pull requests when applicable

## Resources

- [.NET Native AOT Documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [System.Text.Json Source Generators](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
- [Windows Forms High-DPI](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms)
- [OpenSpeedTest GitHub](https://github.com/openspeedtest/Speed-Test)

