$filePath = 'src\pages\ConsultationsPage.js'
$content = Get-Content $filePath -Raw
# Remove BOM if present
if ($content.StartsWith([char]0xFEFF)) {
    $content = $content.Substring(1)
}
# Write without BOM using UTF8NoBOM
[System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)
Write-Host "BOM removed from ConsultationsPage.js"
