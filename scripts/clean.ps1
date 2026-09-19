[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$paths = @(
    (Join-Path $projectRoot ".vs"),
    (Join-Path $projectRoot "src\LiquidGlassPoC\bin"),
    (Join-Path $projectRoot "src\LiquidGlassPoC\obj")
)

foreach ($path in $paths) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
    }
}

Write-Host "Caches Visual Studio, bin et obj supprimés." -ForegroundColor Green
