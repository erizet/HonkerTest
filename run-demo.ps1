#!/usr/bin/env pwsh
# Builds the solution, then launches HonkerReader and HonkerWriter in separate terminal windows.
#
# Usage:
#   .\run-demo.ps1              # use demo.db in the repo root
#   .\run-demo.ps1 -Fresh       # delete demo.db first, then run
#   .\run-demo.ps1 -DbPath my.db

param(
    [string] $DbPath = "demo.db",
    [switch] $Fresh
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

if ($Fresh) {
    $fullDb = Join-Path $root $DbPath
    if (Test-Path $fullDb) {
        Remove-Item $fullDb
        Write-Host "Cleared $DbPath" -ForegroundColor Yellow
    }
}

Write-Host "Building..." -ForegroundColor Cyan
dotnet build $root --configuration Release -v q
if ($LASTEXITCODE -ne 0) { exit 1 }

$dbFull     = Join-Path $root $DbPath
$readerCmd  = "cd '$root'; dotnet run --project src/HonkerReader --configuration Release --no-build -- '$dbFull'"
$writerCmd  = "cd '$root'; dotnet run --project src/HonkerWriter --configuration Release --no-build -- '$dbFull'"

# Start reader first so it's ready before the writer publishes anything
Start-Process powershell -ArgumentList @("-NoExit", "-Command", $readerCmd)
Start-Sleep -Milliseconds 800
Start-Process powershell -ArgumentList @("-NoExit", "-Command", $writerCmd)

Write-Host "Launched Reader and Writer in separate windows." -ForegroundColor Green
Write-Host "Press Ctrl+C in each window to stop, or close the windows."
