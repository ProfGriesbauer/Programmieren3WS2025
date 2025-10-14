# Builds and runs Sebastian's TicTacToe WPF project
# Usage: Right-click -> "Run with PowerShell" or execute in PowerShell:
#   & .\run-tictactoe.ps1

$proj = Join-Path $PSScriptRoot 'SebastiansTikTacToe.csproj'
Write-Host "Project: $proj"

Write-Host "Building project..."
$build = dotnet build $proj -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Starting TicTacToe..."
# Use dotnet run to start the WPF app
dotnet run --project $proj -c Debug
