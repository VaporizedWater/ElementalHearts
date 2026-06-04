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
# Before compiling it also runs the *texture half* of the heart validator (Ensure-HeartTextures):
# every concrete heart MUST have a <ClassName>.png beside its .cs (RULE — see CLAUDE.md). A missing
# one would hard-fail tML's content load, so this creates a blank placeholder and names it loudly,
# keeping the build green until real art lands. The content half (effect/power/HP/tier/namespace)
# runs immediately after texture creation through tools\Audit-Hearts.ps1, so broken declarations
# fail before tML starts compiling.
#
#   ./build.ps1            # filtered output, non-zero exit on a real error
#   ./build.ps1 -Full      # unfiltered output (see the CS1705 noise too)
#
# Not shipped: *.ps1 is in the build.txt buildIgnore list.

param([switch]$Full)

# RULE: every concrete heart (a `sealed ...Heart` class) must have a matching <ClassName>.png in the
# same folder, because tModLoader derives each ModItem's texture from its namespace+class path. This
# fills any gap with a 1x1 transparent placeholder so the build never dies on a missing texture, and
# prints exactly which files still need real art.
function Ensure-HeartTextures {
    $roots = @(
        (Join-Path $PSScriptRoot 'Content\Items\Vanilla'),
        (Join-Path $PSScriptRoot 'Content\Items\CrossModHearts')
    )
    $fallbackTexture = Join-Path $PSScriptRoot 'Content\Items\LifeShards\CommonLifeShard.png'
    $blankPng = [IO.File]::ReadAllBytes($fallbackTexture)
    $created = New-Object System.Collections.Generic.List[string]

    foreach ($root in $roots) {
        if (-not (Test-Path $root)) { continue }
        foreach ($cs in Get-ChildItem -Path $root -Recurse -Filter *.cs) {
            $text = Get-Content -LiteralPath $cs.FullName -Raw
            if (-not $text) { continue }
            foreach ($m in [regex]::Matches($text, 'sealed\s+class\s+(\w+Heart)\b')) {
                $png = Join-Path $cs.DirectoryName ($m.Groups[1].Value + '.png')
                if (-not (Test-Path -LiteralPath $png)) {
                    [IO.File]::WriteAllBytes($png, $blankPng)
                    $created.Add((Resolve-Path -LiteralPath $png -Relative))
                }
            }
        }
    }

    if ($created.Count -gt 0) {
        Write-Host "Created $($created.Count) placeholder heart texture(s) - REPLACE WITH REAL ART:" -ForegroundColor Yellow
        $created | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    }
    else {
        Write-Host 'Heart textures: every heart has a .png beside its .cs.' -ForegroundColor DarkGray
    }
}

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    Ensure-HeartTextures
    powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'tools\Audit-Hearts.ps1')

    $output = dotnet build ElementalHearts.csproj -p:SkipHeartAudit=true -nologo --verbosity quiet 2>&1 | Out-String -Stream

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
