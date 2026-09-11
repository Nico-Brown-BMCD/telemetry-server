$configPaths = @(
    "$env:APPDATA\pyRevit\pyRevit_config.ini",
    "C:\ProgramData\pyRevit\pyRevit_config.ini"
)

$telemetrySection = @"

[telemetry]
utc_timestamps = true
active = true
telemetry_file_dir = 
telemetry_server_url = https://bmcd-pyrevit-telemetry-e0e5g2b0e0brbrf2.uksouth-01.azurewebsites.net/api/v2/scripts
include_hooks = false
active_app = true
apptelemetry_server_url = https://bmcd-pyrevit-telemetry-e0e5g2b0e0brbrf2.uksouth-01.azurewebsites.net/api/v2/events
"@

foreach ($path in $configPaths) {
    if (-not (Test-Path $path)) {
        Write-Host "Not found: $path" -ForegroundColor Yellow
        continue
    }

    $content = Get-Content $path -Raw

    if ($content -match '\[telemetry\]') {
        # Replace existing telemetry section
        $content = $content -replace '(?s)\[telemetry\].*?(?=\[|\z)', $telemetrySection.TrimStart()
        [System.IO.File]::WriteAllText($path, $content)
        Write-Host "Updated: $path" -ForegroundColor Green
    } else {
        # Append new telemetry section
        [System.IO.File]::AppendAllText($path, $telemetrySection)
        Write-Host "Appended to: $path" -ForegroundColor Green
    }
}

Write-Host "`nDone. Restart Revit for changes to take effect." -ForegroundColor Cyan
