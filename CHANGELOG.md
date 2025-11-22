# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial release of OpenSpeedTest Client
- Dual-mode operation (GUI and CLI)
- Native AOT compilation for zero-dependency deployment
- High-DPI support with Per-Monitor V2 awareness
- Multi-threaded speed testing (ping, download, upload, jitter)
- JSON configuration file support
- PowerShell-friendly JSON output
- Support for internal servers with self-signed certificates
- Real-time progress reporting in GUI
- Configurable test parameters (threads, duration, samples)
- Comprehensive system information collection (computer name, IP, connection type)
- Cross-platform server compatibility (HTTP/HTTPS)

### Features
- **GUI Mode**: Interactive Windows Forms interface with real-time metrics
- **CLI Mode**: Silent JSON output for automation and scripting
- **Verbose Mode**: Optional progress reporting to stderr
- **Configuration**: JSON-based configuration with validation
- **Security**: Certificate validation with optional override for internal servers
- **Performance**: Parallel connection testing for accurate speed measurement

## [1.0.0] - TBD

### Initial Release
- First stable release
