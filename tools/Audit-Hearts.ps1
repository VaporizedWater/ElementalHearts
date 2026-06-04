#!/usr/bin/env pwsh
# Reports content-rule failures that are cheap to catch before tModLoader compiles the mod.

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$heartRoots = @(
    (Join-Path $repoRoot 'Content\Items\Vanilla'),
    (Join-Path $repoRoot 'Content\Items\CrossModHearts')
)
$tierNames = @('Common', 'Uncommon', 'Rare', 'Epic', 'Legendary', 'Exotic', 'Mythic')

function Get-HeartSourceFiles {
    foreach ($root in $heartRoots) {
        if (Test-Path -LiteralPath $root) {
            Get-ChildItem -Path $root -Recurse -Filter *.cs
        }
    }
}

function Get-RelativePath([string]$path) {
    $resolvedPath = (Resolve-Path -LiteralPath $path).Path
    $resolvedRoot = (Resolve-Path -LiteralPath $repoRoot).Path.TrimEnd('\')

    if ($resolvedPath.StartsWith($resolvedRoot, [StringComparison]::OrdinalIgnoreCase)) {
        return '.\' + $resolvedPath.Substring($resolvedRoot.Length).TrimStart('\')
    }

    return $resolvedPath
}

function Get-RepoRelativePath([string]$path) {
    (Get-RelativePath $path).TrimStart('.', '\')
}

function Get-TierFolderProblem([IO.FileInfo]$file, [string]$heartName, [string]$tier, [string]$namespace) {
    $relativeDirectory = Get-RepoRelativePath $file.DirectoryName
    $pathParts = $relativeDirectory -split '[\\/]'
    $namespaceParts = if ($namespace) { $namespace -split '\.' } else { @() }

    foreach ($part in $pathParts + $namespaceParts) {
        if ($tierNames -contains $part -and $part -ne $tier) {
            return "$heartName declares $tier but sits under tier segment '$part' in $(Get-RelativePath $file.FullName)"
        }
    }

    $vanillaIndex = [Array]::IndexOf($pathParts, 'Vanilla')
    if ($vanillaIndex -ge 0 -and $vanillaIndex + 1 -lt $pathParts.Length) {
        $next = $pathParts[$vanillaIndex + 1]
        if ($tierNames -contains $next -and $next -ne $tier) {
            return "$heartName declares $tier but is in Vanilla\$next at $(Get-RelativePath $file.FullName)"
        }
    }

    return $null
}

$effectFile = Get-Content -LiteralPath (Join-Path $repoRoot 'Common\Hearts\HeartEffectRegistry.cs') -Raw
$powerFile = Get-Content -LiteralPath (Join-Path $repoRoot 'Common\Hearts\ElementalPowerRegistry.cs') -Raw

$missingEffect = New-Object System.Collections.Generic.List[string]
$thinPalette = New-Object System.Collections.Generic.List[string]
$missingPower = New-Object System.Collections.Generic.List[string]
$hpOnActiveAbility = New-Object System.Collections.Generic.List[string]
$tierFolderMismatch = New-Object System.Collections.Generic.List[string]
$namespaceMismatch = New-Object System.Collections.Generic.List[string]
$total = 0

foreach ($cs in Get-HeartSourceFiles) {
    $text = Get-Content -LiteralPath $cs.FullName -Raw
    if (-not $text) { continue }

    $namespace = ''
    if ($text -match '(?m)^namespace\s+([A-Za-z0-9_.]+);') {
        $namespace = $matches[1]
    }

    $expectedNamespace = 'ElementalHearts.' + ((Get-RepoRelativePath $cs.DirectoryName) -replace '[\\/]', '.')
    if ($namespace -and $namespace -ne $expectedNamespace) {
        $namespaceMismatch.Add("$(Get-RelativePath $cs.FullName) declares $namespace; expected $expectedNamespace")
    }

    foreach ($classMatch in [regex]::Matches($text, 'sealed\s+class\s+(\w+Heart)\b')) {
        $heartName = $classMatch.Groups[1].Value
        $total++

        if ($powerFile -notmatch "\[`"$heartName`"\]\s*=") {
            $missingPower.Add($heartName)
        }

        if ($effectFile -match "\[`"$heartName`"\]\s*=\s*HeartEffect\.Prismatic") {
            # Prismatic hearts intentionally skip fixed palettes.
        }
        elseif ($effectFile -match "\[`"$heartName`"\]\s*=\s*new\s+HeartEffect") {
            # Rare explicit construction; trust the C# DEBUG validator for the exact colour count.
        }
        elseif ($effectFile -match "\[`"$heartName`"\]\s*=\s*Eff\(([^)]+)\)") {
            $argumentCount = ([regex]::Matches($matches[1], ',')).Count + 1
            if ($argumentCount -lt 9) {
                $thinPalette.Add($heartName)
            }
        }
        else {
            $missingEffect.Add($heartName)
        }

        $classStart = $classMatch.Index
        $nextClass = $text.IndexOf('sealed class ', $classStart + 1)
        $classText = if ($nextClass -ge 0) { $text.Substring($classStart, $nextClass - $classStart) } else { $text.Substring($classStart) }

        if ($classText -match 'public\s+override\s+bool\s+IsActiveAbility\s*=>\s*true;' -and
            $classText -notmatch 'public\s+override\s+int\s+HpGain\s*=>\s*0;') {
            $hpOnActiveAbility.Add($heartName)
        }

        if ($classText -match 'HeartTier\s+Tier\s*=>\s*HeartTier\.(\w+)') {
            $problem = Get-TierFolderProblem $cs $heartName $matches[1] $namespace
            if ($problem) {
                $tierFolderMismatch.Add($problem)
            }
        }
    }
}

$errors = 0
function Report([string]$title, [System.Collections.IEnumerable]$items) {
    $list = @($items)
    if ($list.Count -eq 0) { return }

    Write-Host "ERROR: $title" -ForegroundColor Red
    $list | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    $script:errors++
}

Report 'Missing from HeartEffectRegistry:' $missingEffect
Report 'Color-palette rule violation (fewer than 3 curated colors):' $thinPalette
Report 'Missing from ElementalPowerRegistry:' $missingPower
Report 'Active-ability hearts must override HpGain => 0:' $hpOnActiveAbility
Report 'Tier folder/namespace mismatch:' $tierFolderMismatch
Report 'Namespace does not mirror folder path:' $namespaceMismatch

if ($errors -gt 0) {
    Write-Host "Heart audit failed across $total heart(s)." -ForegroundColor Red
    exit 1
}

Write-Host "Heart audit: all $total heart(s) validated cleanly." -ForegroundColor DarkGray
