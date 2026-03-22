$filePath = 'src\pages\ConsultationsPage.js'
$lines = @(Get-Content $filePath)
$newLines = $lines[0..370]
$newLines | Out-File $filePath -Encoding UTF8
Write-Host "File truncated to 371 lines"
