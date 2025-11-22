# Build script for OpenSpeedTest Client
# Creates a self-contained single-file executable

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [string]$OutputPath = '.\publish',
    
    [Parameter()]
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

Write-Host "OpenSpeedTest Client - Build Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path $OutputPath) {
        Remove-Item $OutputPath -Recurse -Force
    }
    dotnet clean --configuration $Configuration
    Write-Host "Clean complete." -ForegroundColor Green
    Write-Host ""
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Restore failed"
    exit 1
}
Write-Host "Restore complete." -ForegroundColor Green
Write-Host ""

# Publish self-contained single-file
Write-Host "Publishing self-contained executable..." -ForegroundColor Yellow
Write-Host "  Configuration: $Configuration" -ForegroundColor Gray
Write-Host "  Runtime: win-x64" -ForegroundColor Gray
Write-Host "  Output: $OutputPath" -ForegroundColor Gray
Write-Host ""

dotnet publish src/OpenSpeedTestClient/OpenSpeedTestClient.csproj `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    --output $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed"
    exit 1
}

Write-Host ""
Write-Host "Publish complete!" -ForegroundColor Green
Write-Host ""

# Display output info
$exePath = Join-Path $OutputPath "OpenSpeedTestClient.exe"
$configPath = Join-Path $OutputPath "config.json"

if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    $sizeInMB = [math]::Round($fileInfo.Length / 1MB, 2)
    
    Write-Host "Output Information:" -ForegroundColor Cyan
    Write-Host "  Executable: $exePath" -ForegroundColor White
    Write-Host "  Size: $sizeInMB MB" -ForegroundColor White
    Write-Host "  Config: $configPath" -ForegroundColor White
    Write-Host ""
    
    Write-Host "To test the executable:" -ForegroundColor Cyan
    Write-Host "  GUI Mode: $exePath" -ForegroundColor White
    Write-Host "  CLI Mode: $exePath --cli --verbose" -ForegroundColor White
    Write-Host ""
} else {
    Write-Warning "Executable not found at expected path: $exePath"
}

Write-Host "Build completed successfully!" -ForegroundColor Green
