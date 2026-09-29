$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$testProject = Join-Path $root 'Tests\Invenzo.Tests\Invenzo.Tests.csproj'
Write-Host 'Restoring xUnit test project...'
dotnet restore $testProject
Write-Host 'Running 25 controller tests...'
dotnet test $testProject --no-restore --nologo
