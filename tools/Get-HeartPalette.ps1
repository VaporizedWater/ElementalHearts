#!/usr/bin/env pwsh
# Get-HeartPalette.ps1 — extract a heart sprite's real palette as hex colours.
#
# Supports the HeartEffectRegistry color-palette RULE (see CLAUDE.md): every heart's consume-burst
# colours must come straight from its own sprite. This reads a .png (or a folder of them), tallies
# the opaque pixels by colour, and prints the dominant hues as hex — plus a ready-to-paste
# `Eff(...)` snippet of the top 3 distinct colours so you can drop it into HeartEffectRegistry and
# then hand-pick/curate from there.
#
#   ./tools/Get-HeartPalette.ps1 -Path Content/Items/Vanilla/Common/StoneHeart.png
#   ./tools/Get-HeartPalette.ps1 -Path Content/Items/Vanilla/Common   # whole folder, recursive
#   ./tools/Get-HeartPalette.ps1 -Path X.png -Top 12 -Quantize 8      # more colours, finer buckets
#
# Windows only (uses System.Drawing/GDI+, same as the rest of the tML toolchain). Not shipped:
# *.ps1 is in the build.txt buildIgnore list, and tools\ holds no compiled source.

param(
    [Parameter(Mandatory = $true)] [string]$Path,
    [int]$Top = 8,
    # Round each channel to the nearest multiple of this to merge near-identical shades
    # (anti-aliasing produces dozens of almost-equal pixels). 16 is a good default for 18x18 sprites.
    [int]$Quantize = 16,
    # Ignore pixels more transparent than this (0-255) — edge antialiasing against nothing.
    [int]$AlphaFloor = 128
)

$ErrorActionPreference = 'Stop'
try { Add-Type -AssemblyName System.Drawing -ErrorAction Stop }
catch { Write-Host 'System.Drawing is unavailable (Windows + Windows PowerShell recommended).' -ForegroundColor Red; exit 1 }

function Get-Palette([string]$file) {
    $bmp = [System.Drawing.Bitmap]::new($file)
    try {
        $tally = @{}
        for ($y = 0; $y -lt $bmp.Height; $y++) {
            for ($x = 0; $x -lt $bmp.Width; $x++) {
                $p = $bmp.GetPixel($x, $y)
                if ($p.A -lt $AlphaFloor) { continue }
                $r = [Math]::Round($p.R / $Quantize) * $Quantize; if ($r -gt 255) { $r = 255 }
                $g = [Math]::Round($p.G / $Quantize) * $Quantize; if ($g -gt 255) { $g = 255 }
                $b = [Math]::Round($p.B / $Quantize) * $Quantize; if ($b -gt 255) { $b = 255 }
                $key = "$r,$g,$b"
                if ($tally.ContainsKey($key)) { $tally[$key]++ } else { $tally[$key] = 1 }
            }
        }
    }
    finally { $bmp.Dispose() }

    $total = ($tally.Values | Measure-Object -Sum).Sum
    if (-not $total) { Write-Host "  (no opaque pixels)" -ForegroundColor DarkGray; return }

    $ranked = $tally.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First $Top
    Write-Host (Split-Path $file -Leaf) -ForegroundColor Cyan
    foreach ($e in $ranked) {
        $rgb = $e.Key -split ','
        $hex = '#{0:X2}{1:X2}{2:X2}' -f [int]$rgb[0], [int]$rgb[1], [int]$rgb[2]
        $pct = [Math]::Round(100 * $e.Value / $total, 1)
        Write-Host ("  {0}  rgb({1,3},{2,3},{3,3})  {4,5}%" -f $hex, $rgb[0], $rgb[1], $rgb[2], $pct)
    }

    $top3 = $ranked | Select-Object -First 3 | ForEach-Object { ($_.Key -replace ',', ', ') }
    if ($top3.Count -ge 3) {
        Write-Host ("  -> Eff({0})" -f ($top3 -join ', ')) -ForegroundColor Green
    }
    Write-Host ''
}

if (Test-Path -LiteralPath $Path -PathType Container) {
    Get-ChildItem -Path $Path -Recurse -Filter *.png | ForEach-Object { Get-Palette $_.FullName }
}
else {
    Get-Palette (Resolve-Path -LiteralPath $Path).Path
}
