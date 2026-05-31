#!/usr/bin/env pwsh
# Export-HeartPalettes.ps1 — dump every heart sprite's dominant palette, one line per heart.
# Bulk companion to Get-HeartPalette.ps1 for the registry-wide color-palette pass. Writes to a temp
# file (outside the repo, never shipped). Each line: ClassName | #hex:pct #hex:pct ... (top N).
#
#   ./tools/Export-HeartPalettes.ps1                 # -> $env:TEMP\heart-palettes.txt
#   ./tools/Export-HeartPalettes.ps1 -Out C:\x.txt -Top 8 -Quantize 16

param(
    [string]$Out = (Join-Path $env:TEMP 'heart-palettes.txt'),
    [int]$Top = 7,
    [int]$Quantize = 16,
    [int]$AlphaFloor = 128
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$roots = @(
    (Join-Path $PSScriptRoot '..\Content\Items\Vanilla'),
    (Join-Path $PSScriptRoot '..\Content\Items\CrossModHearts')
)

$lines = New-Object System.Collections.Generic.List[string]
$n = 0
foreach ($root in $roots) {
    if (-not (Test-Path $root)) { continue }
    foreach ($png in Get-ChildItem -Path $root -Recurse -Filter *Heart.png) {
        $name = [IO.Path]::GetFileNameWithoutExtension($png.Name)
        $bmp = [System.Drawing.Bitmap]::new($png.FullName)
        try {
            $tally = @{}
            for ($y = 0; $y -lt $bmp.Height; $y++) {
                for ($x = 0; $x -lt $bmp.Width; $x++) {
                    $p = $bmp.GetPixel($x, $y)
                    if ($p.A -lt $AlphaFloor) { continue }
                    $r = [Math]::Min(255, [Math]::Round($p.R / $Quantize) * $Quantize)
                    $g = [Math]::Min(255, [Math]::Round($p.G / $Quantize) * $Quantize)
                    $b = [Math]::Min(255, [Math]::Round($p.B / $Quantize) * $Quantize)
                    $key = "$r,$g,$b"
                    if ($tally.ContainsKey($key)) { $tally[$key]++ } else { $tally[$key] = 1 }
                }
            }
        }
        finally { $bmp.Dispose() }

        $total = ($tally.Values | Measure-Object -Sum).Sum
        if (-not $total) { continue }

        $parts = $tally.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First $Top | ForEach-Object {
            $rgb = $_.Key -split ','
            $hex = '{0:X2}{1:X2}{2:X2}' -f [int]$rgb[0], [int]$rgb[1], [int]$rgb[2]
            $pct = [Math]::Round(100 * $_.Value / $total)
            "$($_.Key):$pct%"
        }
        $lines.Add("$name | $($parts -join '  ')")
        $n++
    }
}

$lines | Sort-Object | Set-Content -LiteralPath $Out -Encoding utf8
Write-Host "Wrote $n heart palettes to $Out"
