# Test CLI output
$exe = ".\src\OpenSpeedTestClient\bin\Debug\net9.0-windows\OpenSpeedTestClient.exe"

Write-Host "Testing CLI mode with verbose output..." -ForegroundColor Cyan
Write-Host "=" * 60

& $exe --cli --verbose 2>&1 | Tee-Object -Variable output

Write-Host "`n" + ("=" * 60)
Write-Host "Exit Code: $LASTEXITCODE" -ForegroundColor $(if ($LASTEXITCODE -eq 0) { "Green" } else { "Red" })
