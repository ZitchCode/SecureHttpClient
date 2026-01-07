#!/usr/bin/env pwsh
#Requires -Version 7.0

<#
.SYNOPSIS
    Cross-platform build script for SecureHttpClient
.DESCRIPTION
    Builds, tests, and packages SecureHttpClient library
.PARAMETER Target
    The build target: clean, restore, build, test, pack, or all (default)
.PARAMETER Configuration
    The build configuration: Debug or Release (default)
.EXAMPLE
    ./build.ps1
    ./build.ps1 -Target build -Configuration Debug
#>

param(
    [Parameter(Position=0)]
    [ValidateSet('clean', 'restore', 'build', 'test', 'pack', 'all')]
    [string]$Target = 'all',
    
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$LibName = 'SecureHttpClient'
$Version = Get-Content -Path "$RootDir/version.txt" -Raw | ForEach-Object { $_.Trim() }

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "Building $LibName v$Version" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host

function Invoke-Clean {
    Write-Host "Cleaning..." -ForegroundColor Yellow
    dotnet clean "$RootDir/$LibName/$LibName.csproj" -c $Configuration -v m
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }
}

function Invoke-Restore {
    Write-Host "Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore "$RootDir/$LibName/$LibName.csproj"
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
}

function Invoke-Build {
    Write-Host "Building..." -ForegroundColor Yellow
    # Build for net10.0 only (cross-platform)
    dotnet build "$RootDir/$LibName/$LibName.csproj" `
        -f net10.0 `
        -c $Configuration `
        -v m `
        -p:Version=$Version `
        -p:AssemblyVersion=$Version `
        -p:AssemblyFileVersion=$Version `
        --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
}

function Invoke-Test {
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test "$RootDir/$LibName.Test/$LibName.Test.csproj" `
        -c $Configuration `
        -v m `
        --no-build
    if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
}

function Invoke-Pack {
    Write-Host "Creating NuGet package..." -ForegroundColor Yellow
    dotnet pack "$RootDir/$LibName/$LibName.csproj" `
        -c $Configuration `
        -v m `
        --no-build `
        -o $RootDir `
        -p:PackageVersion=$Version `
        -p:IncludeSymbols=true `
        -p:SymbolPackageFormat=snupkg
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }
}

function Invoke-All {
    Invoke-Clean
    Invoke-Restore
    Invoke-Build
    Write-Host "Build completed successfully!" -ForegroundColor Green
}

# Main script logic
try {
    switch ($Target) {
        'clean' { Invoke-Clean }
        'restore' { Invoke-Restore }
        'build' { Invoke-Build }
        'test' { Invoke-Test }
        'pack' { Invoke-Pack }
        'all' { Invoke-All }
    }
    
    Write-Host
    Write-Host "======================================================================" -ForegroundColor Cyan
    Write-Host "Done!" -ForegroundColor Green
    Write-Host "======================================================================" -ForegroundColor Cyan
    exit 0
}
catch {
    Write-Host
    Write-Host "======================================================================" -ForegroundColor Red
    Write-Host "Build failed: $_" -ForegroundColor Red
    Write-Host "======================================================================" -ForegroundColor Red
    exit 1
}
