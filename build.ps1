#!/usr/bin/env pwsh
# build.ps1 - compile-check the mod from the CLI and surface only *real* source errors.
#
# The csproj imports tModLoader's targets, so `dotnet build` runs tML's mod builder. Two quirks
# this wrapper handles:
#   * TML003 - tML refuses to build from the CLI while tModLoader is running. Close it (or disable
#     the mod in-game) first. We detect this and say so, rather than reporting a phantom error.
#   * CS1705 - tML's reference assemblies emit "assembly ... uses a version higher than referenced"
#     noise that is environment-only and never reflects a problem in this mod's source. We filter it
#     out so a genuine compile error stands alone. The in-game build remains the final word.
#
#   ./build.ps1            # filtered output, non-zero exit on a real error
#   ./build.ps1 -Full      # unfiltered output (see the CS1705 noise too)
#
# Not shipped: *.ps1 is in the build.txt buildIgnore list.

param([switch]$Full)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $output = dotnet build ElementalHearts.csproj -nologo --verbosity quiet 2>&1 | Out-String -Stream

    if ($output -match 'TML003') {
        Write-Host 'tModLoader is running - close it (or disable Elemental Hearts in-game) to build from the CLI.' -ForegroundColor Yellow
        exit 2
    }

    $filtered = $output | Where-Object { $_ -notmatch 'CS1705' }
    if ($Full) { $output } else { $filtered }

    if ($filtered | Where-Object { $_ -match ': error ' }) {
        Write-Host "`nReal source errors found (above)." -ForegroundColor Red
        exit 1
    }

    Write-Host "`nNo real source errors (CS1705 environment noise filtered out)." -ForegroundColor Green
}
finally {
    Pop-Location
}
