[CmdletBinding()]
param(
    [string]$ExpectedVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Test-SemVer([string]$Value) {
    return $Value -match '^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$'
}

function Get-VersionElementValue([string]$ProjectPath) {
    $content = Get-Content -Path $ProjectPath -Raw
    $match = [regex]::Match($content, '<Version>([^<]+)</Version>')

    if (-not $match.Success) {
        throw "No <Version> element found in '$ProjectPath'."
    }

    return $match.Groups[1].Value
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$versionFilePath = Join-Path $repoRoot "eng\release-version.txt"

if (-not (Test-Path -LiteralPath $versionFilePath)) {
    throw "Release version file not found: $versionFilePath"
}

$releaseVersion = (Get-Content -Path $versionFilePath -Raw).Trim()

if (-not (Test-SemVer -Value $releaseVersion)) {
    throw "Release version file '$versionFilePath' does not contain a valid semantic version."
}

if (-not [string]::IsNullOrWhiteSpace($ExpectedVersion)) {
    if (-not (Test-SemVer -Value $ExpectedVersion)) {
        throw "Expected version '$ExpectedVersion' is not a valid semantic version."
    }

    if ($releaseVersion -ne $ExpectedVersion) {
        throw "Release version '$releaseVersion' does not match expected version '$ExpectedVersion'."
    }
}

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

$errors = New-Object System.Collections.Generic.List[string]

foreach ($relativeProjectPath in $projectPaths) {
    $projectPath = Join-Path $repoRoot $relativeProjectPath

    if (-not (Test-Path -LiteralPath $projectPath)) {
        $errors.Add("Missing project file: $projectPath")
        continue
    }

    $projectVersion = Get-VersionElementValue -ProjectPath $projectPath

    if ($projectVersion -ne $releaseVersion) {
        $errors.Add("$relativeProjectPath has version '$projectVersion' but expected '$releaseVersion'.")
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Host $_ }
    throw "Release version validation failed for nmolecules."
}

Write-Host "Release version validation passed for nmolecules ($releaseVersion)."
