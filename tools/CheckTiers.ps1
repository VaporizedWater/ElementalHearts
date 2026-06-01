$roots = @("c:\Users\vince\Documents\My Games\Terraria\tModLoader\ModSources\ElementalHearts\Content\Items\Vanilla", "c:\Users\vince\Documents\My Games\Terraria\tModLoader\ModSources\ElementalHearts\Content\Items\CrossModHearts")
foreach ($root in $roots) {
    Get-ChildItem -Path $root -Recurse -Filter *.cs | ForEach-Object {
        $file = $_.FullName
        $text = Get-Content $file -Raw
        if ($text -match 'HeartTier\s+Tier\s*=>\s*HeartTier\.(\w+)') {
            $tier = $matches[1]
            $parentName = $_.Directory.Name
            $grandParentName = $_.Directory.Parent.Name
            
            # Category folders like "Pacified" or "Potions" don't match tier name,
            # but tier folders should match.
            # Let's just output anything where neither parent nor grandparent is the tier name.
            if ($parentName -ne $tier -and $grandParentName -ne $tier) {
                Write-Host "File: $($file) | Found Tier: $tier | Parent: $parentName | Grandparent: $grandParentName"
            }
        }
    }
}
