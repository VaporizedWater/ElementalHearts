$files = Get-ChildItem -Path Content\Items -Recurse -Filter *Heart.cs
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'DrawAnimationVertical\(10,') {
        $content = $content -replace 'DrawAnimationVertical\(10,', 'DrawAnimationVertical(20,'
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated $($file.Name)"
    }
}
