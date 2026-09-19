[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Debug",

    [ValidateSet("x64", "ARM64")]
    [string] $Platform = "x64",

    [switch] $Run
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $projectRoot "src\LiquidGlassPoC\LiquidGlassPoC.csproj"
$runtimeIdentifier = if ($Platform -eq "ARM64") { "win-arm64" } else { "win-x64" }

Write-Host "Restoring LiquidGlassPoC..." -ForegroundColor Cyan
dotnet restore $project -r $runtimeIdentifier
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed." }

Write-Host "Building $Configuration / $Platform..." -ForegroundColor Cyan
dotnet build $project `
    --configuration $Configuration `
    --runtime $runtimeIdentifier `
    --no-restore `
    -p:Platform=$Platform
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

$executable = Join-Path $projectRoot "src\LiquidGlassPoC\bin\$Platform\$Configuration\net10.0-windows10.0.26100.0\$runtimeIdentifier\LiquidGlassPoC.exe"

Write-Host "Build complete: $executable" -ForegroundColor Green

if ($Run) {
    if (-not (Test-Path $executable)) {
        throw "Executable not found at $executable"
    }

    Start-Process $executable
}
