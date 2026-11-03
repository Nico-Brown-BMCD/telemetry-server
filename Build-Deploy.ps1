[CmdletBinding()]
param(
    [switch]$BuildOnly,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Install the .NET SDK required by global.json before running this script.'
}

$settingsPath = Join-Path $PSScriptRoot '.vscode/settings.json'
$settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
$resourceId = $settings.'appService.defaultWebAppToDeploy'
if ($resourceId -notmatch '^/subscriptions/(?<Subscription>[^/]+)/resourceGroups/(?<ResourceGroup>[^/]+)/providers/Microsoft.Web/sites/(?<WebApp>[^/]+)$') {
    throw 'appService.defaultWebAppToDeploy must identify an Azure App Service web app.'
}

$subscription = $Matches.Subscription
$resourceGroup = $Matches.ResourceGroup
$webApp = $Matches.WebApp
$publishPath = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot $settings.'appService.deploySubpath'))
$projectPath = Join-Path $PSScriptRoot 'src/Telemetry.Api/Telemetry.Api.csproj'

Push-Location $PSScriptRoot
$temporaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ('telemetry-deploy-' + [guid]::NewGuid().ToString('N'))
try {
    if (-not $BuildOnly) {
        if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
            throw 'Azure CLI is required for script deployment. Install it and run az login, or use -BuildOnly and deploy through the Azure extension.'
        }

        $hostname = & az webapp show --subscription $subscription --resource-group $resourceGroup --name $webApp --query defaultHostName --output tsv --only-show-errors
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($hostname -join ''))) {
            throw 'Cannot access the configured web app. Run az login and verify subscription access. Extension sign-in is separate from Azure CLI sign-in.'
        }
    }

    $stagingPath = Join-Path $temporaryPath 'publish'
    $null = New-Item -ItemType Directory -Path $stagingPath -Force
    Write-Host "Publishing $Configuration build for $webApp..."
    & dotnet publish $projectPath --configuration $Configuration --output $stagingPath '-p:MigrationsProvider=SqlServer' --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw 'Publish failed. No deployment was attempted.'
    }

    foreach ($requiredFile in @('Telemetry.Api.dll', 'Telemetry.Migrations.SqlServer.dll', 'web.config')) {
        if (-not (Test-Path -LiteralPath (Join-Path $stagingPath $requiredFile) -PathType Leaf)) {
            throw "Publish is missing $requiredFile. No deployment was attempted."
        }
    }

    $null = New-Item -ItemType Directory -Path $publishPath -Force
    Get-ChildItem -LiteralPath $stagingPath -Force | Copy-Item -Destination $publishPath -Recurse -Force
    Write-Host "Fresh build copied to $publishPath"

    if ($BuildOnly) {
        Write-Host 'Build complete. The Azure extension can now deploy the configured publish folder.'
        return
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archivePath = Join-Path $temporaryPath 'telemetry.zip'
    [System.IO.Compression.ZipFile]::CreateFromDirectory($stagingPath, $archivePath)
    Write-Host "Deploying to $webApp in $resourceGroup (subscription $subscription)..."
    & az webapp deploy --subscription $subscription --resource-group $resourceGroup --name $webApp --src-path $archivePath --type zip --restart true --async false --only-show-errors --output none
    if ($LASTEXITCODE -ne 0) {
        throw 'Azure deployment failed. Review the Azure CLI output above.'
    }

    Write-Host "Deployment completed: https://$($hostname -join '')"
    Write-Host 'Deployment completion does not confirm database writes. Verify telemetry delivery separately.'
}
finally {
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Recurse -Force
    }
    Pop-Location
}