$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install a .NET 10 SDK first.' }
$version = & dotnet --version
if (-not $version.StartsWith('10.')) { throw "A .NET 10 SDK is required. Selected SDK: $version" }
& dotnet restore EmployeeManagement.slnx
if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
& dotnet build EmployeeManagement.slnx -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
& dotnet test EmployeeManagement.slnx -c Release --no-build --logger 'trx;LogFileName=results.trx'
if ($LASTEXITCODE -ne 0) { throw 'Tests failed. Review the reported failures and TestResults.' }
Write-Host 'Build and automated tests passed. Complete the SQL Server/browser acceptance checks in docs/VALIDATION.md.'
