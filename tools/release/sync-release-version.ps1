[CmdletBinding()]
param(
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Test-SemVer([string]$Value) {
    return $Value -match '^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$'
}

function Get-ReleaseVersion([string]$VersionFilePath, [string]$ExplicitVersion) {
    if (-not [string]::IsNullOrWhiteSpace($ExplicitVersion)) {
        if (-not (Test-SemVer -Value $ExplicitVersion)) {
            throw "Invalid semantic version '$ExplicitVersion'."
        }

        Set-Content -Path $VersionFilePath -Value $ExplicitVersion -Encoding ascii
        return $ExplicitVersion
    }

    if (-not (Test-Path -LiteralPath $VersionFilePath)) {
        throw "Release version file not found: $VersionFilePath"
    }

    $resolvedVersion = (Get-Content -Path $VersionFilePath -Raw).Trim()

    if (-not (Test-SemVer -Value $resolvedVersion)) {
        throw "Release version file '$VersionFilePath' does not contain a valid semantic version."
    }

    return $resolvedVersion
}

function Set-ProjectVersion([string]$ProjectPath, [string]$ReleaseVersion) {
    $content = Get-Content -Path $ProjectPath -Raw

    if ($content -notmatch '<Version>[^<]+</Version>') {
        throw "No <Version> element found in '$ProjectPath'."
    }

    $updatedContent = [regex]::Replace(
        $content,
        '<Version>[^<]+</Version>',
        "<Version>$ReleaseVersion</Version>",
        1)

    Set-Content -Path $ProjectPath -Value $updatedContent -Encoding utf8NoBOM
    Write-Host "Updated $ProjectPath -> $ReleaseVersion"
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$versionFilePath = Join-Path $repoRoot "eng\release-version.txt"
$releaseVersion = Get-ReleaseVersion -VersionFilePath $versionFilePath -ExplicitVersion $Version
$projectPaths = @(
    "src\nMolecules.Architecture\nMolecules.Architecture.csproj",
    "src\nMolecules.Architecture.Cqrs\nMolecules.Architecture.Cqrs.csproj",
    "src\nMolecules.Architecture.EventStorming\nMolecules.Architecture.EventStorming.csproj",
    "src\nMolecules.Architecture.Hexagonal\nMolecules.Architecture.Hexagonal.csproj",
    "src\nMolecules.Architecture.Layered\nMolecules.Architecture.Layered.csproj",
    "src\nMolecules.Architecture.Microservices\nMolecules.Architecture.Microservices.csproj",
    "src\nMolecules.Architecture.Mvvm\nMolecules.Architecture.Mvvm.csproj",
    "src\nMolecules.Architecture.Onion\nMolecules.Architecture.Onion.csproj",
    "src\nMolecules.Bricks\nMolecules.Bricks.csproj",
    "src\nMolecules.DDD\nMolecules.DDD.csproj",
    "src\nMolecules.Events\nMolecules.Events.csproj",
    "src\nMolecules.Persistence.EntityFramework\nMolecules.Persistence.EntityFramework.csproj"
)

foreach ($relativeProjectPath in $projectPaths) {
    $projectPath = Join-Path $repoRoot $relativeProjectPath

    if (-not (Test-Path -LiteralPath $projectPath)) {
        throw "Project file not found: $projectPath"
    }

    Set-ProjectVersion -ProjectPath $projectPath -ReleaseVersion $releaseVersion
}

Write-Host ""
Write-Host "Release version sync completed for nmolecules ($releaseVersion)."
