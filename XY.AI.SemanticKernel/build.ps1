param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$libProject = Join-Path $PSScriptRoot 'XY.AI.SemanticKernel.csproj'
$testProject = Join-Path $root 'XY.AI.SemanticKernelTests\XY.AI.SemanticKernelTests.csproj'
$outputDir = Join-Path $root 'artifacts\packages'

Write-Host "[1/4] Restore library..."
dotnet restore $libProject

Write-Host "[2/4] Build library..."
dotnet build $libProject -c $Configuration --no-restore

Write-Host "[3/4] Run tests..."
dotnet test $testProject -c $Configuration --no-restore

Write-Host "[4/4] Pack library..."
New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
dotnet pack $libProject -c $Configuration --no-build -o $outputDir

Write-Host "Done. Package output: $outputDir"
