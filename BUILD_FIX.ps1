$ErrorActionPreference = "Stop"
Write-Host "=== Invenzo clean build helper ===" -ForegroundColor Cyan
$project = Join-Path $PSScriptRoot "InventoryManagementSystem.csproj"
if (!(Test-Path $project)) { throw "Run this script from the v19work project folder." }

Write-Host "Stopping any running Invenzo debug process..." -ForegroundColor Yellow
Get-Process -Name InventoryManagementSystem -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

Write-Host "Cleaning local build output..."
Remove-Item (Join-Path $PSScriptRoot "bin") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $PSScriptRoot "obj") -Recurse -Force -ErrorAction SilentlyContinue

$drive = (Get-Item $PSScriptRoot).PSDrive.Name
$free = (Get-PSDrive $drive).Free / 1GB
Write-Host ("Free space on {0}: {1:N2} GB" -f $drive,$free)
if ($free -lt 1) { Write-Warning "Less than 1 GB is free. Free disk space before building." }

Write-Host "Restoring packages..."
dotnet restore $project
Write-Host "Building..."
dotnet build $project --no-restore
Write-Host "SUCCESS: restore and build completed." -ForegroundColor Green
