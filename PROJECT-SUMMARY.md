# OpenSpeedTest Client - Project Summary

## Overview
Windows speed test client with GUI and CLI modes, configurable via JSON, and packaged as a single self-contained executable for win-x64.

## Highlights
- Dual-mode entry point (`Program.cs`) with verbose CLI option and JSON result output.
- Core services: config loading/validation (case-insensitive), system info, ping/jitter, download/upload tests, shared transfer sampling helper, and orchestration via `SpeedTestRunner`.
- JSON-driven configuration (`config.json`) with defaults: `pingServer`, `testServerUrl`, `uploadServerUrl`, `downloadEndpoint`, `uploadEndpoint`, `threads`, durations, samples, timeouts, upload size, and optional insecure certs.
- Windows Forms UI: resizable layout, rocket icon, jitter row visible by default, About dialog with build date.
- Build tooling: `build.ps1` publishes a compressed single-file, self-contained win-x64 executable; config copied alongside.

## Current Architecture
```
src/
  OpenSpeedTestClient.Core/
    Models/ (SpeedTestConfig, SpeedTestResult, TestProgress)
    Services/ (ConfigService, SystemInfoService, PingService,
               DownloadService, UploadService, TransferTestHelper,
               SpeedTestRunner)
  OpenSpeedTestClient/
    Program.cs (GUI/CLI entry)
    MainForm.cs / MainForm.Designer.cs (GUI)
    AboutForm.cs
    app.manifest
    config.json
.github/workflows/
build.ps1
```

## Notable Behaviors
- Per-test cancellation and proper awaiting of worker tasks; faults propagate.
- Download/upload progress sampled every 200ms via shared helper.
- Config lookup is case-insensitive and falls back to app directory when relative.

## Next Steps
- Add separate `uploadThreads` if differing parallelism is desired.
- Refresh screenshots and release links once the repository URL is finalized.
