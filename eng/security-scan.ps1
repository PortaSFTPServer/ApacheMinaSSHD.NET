[CmdletBinding()]
param(
    [switch]$SkipNuGet,
    [switch]$SkipOsv
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$ProjectFiles = @(
    "ApacheMinaSSHD.NET.Bindings/ApacheMinaSSHD.NET.Bindings.csproj",
    "ApacheMinaSSHD.NET.Wrapper/ApacheMinaSSHD.NET.Wrapper.csproj",
    "Sample/SimpleSSHDServer/SimpleSSHDServer.csproj"
)

$Failures = [System.Collections.Generic.List[string]]::new()

function Join-RepoPath {
    param([Parameter(Mandatory)][string]$RelativePath)
    return (Join-Path $RepoRoot $RelativePath)
}

function Write-Section {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host ""
    Write-Host "==> $Message"
}

function Add-Failure {
    param([Parameter(Mandatory)][string]$Message)

    $Failures.Add($Message)
    if ($env:GITHUB_ACTIONS -eq "true") {
        Write-Host "::error::$Message"
    }
    else {
        Write-Host "ERROR: $Message" -ForegroundColor Red
    }
}

function Get-XmlDocument {
    param([Parameter(Mandatory)][string]$Path)

    [xml]$Document = Get-Content -LiteralPath $Path -Raw
    return $Document
}

function Get-ProjectNodes {
    param(
        [Parameter(Mandatory)][xml]$Document,
        [Parameter(Mandatory)][string]$NodeName
    )

    return @($Document.SelectNodes("//*[local-name()='$NodeName']"))
}

function Get-PropertyValues {
    param(
        [Parameter(Mandatory)][AllowNull()]$Object,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $Object) {
        return @()
    }

    $Property = $Object.PSObject.Properties[$Name]
    if ($null -eq $Property -or $null -eq $Property.Value) {
        return @()
    }

    return @($Property.Value)
}

function Test-ProjectHasPackageReference {
    param([Parameter(Mandatory)][string]$ProjectPath)

    $Document = Get-XmlDocument -Path $ProjectPath
    $Nodes = @(Get-ProjectNodes -Document $Document -NodeName "PackageReference")
    return $Nodes.Count -gt 0
}

function Invoke-NuGetVulnerabilityScan {
    if ($SkipNuGet) {
        Write-Section "Skipping NuGet vulnerability scan"
        return
    }

    Write-Section "Scanning NuGet packages"
    foreach ($RelativeProjectPath in $ProjectFiles) {
        $ProjectPath = Join-RepoPath $RelativeProjectPath
        if (!(Test-Path -LiteralPath $ProjectPath)) {
            Add-Failure "Project file not found: $RelativeProjectPath"
            continue
        }

        if (!(Test-ProjectHasPackageReference -ProjectPath $ProjectPath)) {
            Write-Host "Skipping $RelativeProjectPath because it has no PackageReference items."
            continue
        }

        Write-Host "Scanning $RelativeProjectPath"
        $Output = & dotnet list $ProjectPath package --vulnerable --include-transitive --format json 2>&1
        $ExitCode = $LASTEXITCODE
        $Text = ($Output | Out-String).Trim()

        if ($ExitCode -ne 0) {
            Add-Failure "NuGet vulnerability scan failed for $RelativeProjectPath. $Text"
            continue
        }

        try {
            $Report = $Text | ConvertFrom-Json
        }
        catch {
            Add-Failure "NuGet vulnerability scan returned invalid JSON for $RelativeProjectPath. $($_.Exception.Message)"
            continue
        }

        $VulnerablePackages = [System.Collections.Generic.List[string]]::new()
        foreach ($Project in Get-PropertyValues -Object $Report -Name "projects") {
            foreach ($Framework in Get-PropertyValues -Object $Project -Name "frameworks") {
                foreach ($PackageGroupName in @("topLevelPackages", "transitivePackages")) {
                    foreach ($Package in Get-PropertyValues -Object $Framework -Name $PackageGroupName) {
                        foreach ($Vulnerability in Get-PropertyValues -Object $Package -Name "vulnerabilities") {
                            $Severity = if ($Vulnerability.severity) { $Vulnerability.severity } else { "unknown severity" }
                            $Advisory = if ($Vulnerability.advisoryurl) { $Vulnerability.advisoryurl } else { "no advisory URL" }
                            $VulnerablePackages.Add("$($Package.id) $($Package.resolvedVersion) [$Severity] $Advisory")
                        }
                    }
                }
            }
        }

        if ($VulnerablePackages.Count -gt 0) {
            Add-Failure "NuGet vulnerabilities found in $RelativeProjectPath`: $($VulnerablePackages -join '; ')"
        }
    }
}

function Get-MavenReferences {
    $References = [System.Collections.Generic.List[object]]::new()

    foreach ($RelativeProjectPath in $ProjectFiles) {
        $ProjectPath = Join-RepoPath $RelativeProjectPath
        if (!(Test-Path -LiteralPath $ProjectPath)) {
            continue
        }

        $Document = Get-XmlDocument -Path $ProjectPath
        foreach ($Node in Get-ProjectNodes -Document $Document -NodeName "MavenReference") {
            $Name = $Node.Include
            $Version = $Node.Version
            if ([string]::IsNullOrWhiteSpace($Name) -or [string]::IsNullOrWhiteSpace($Version)) {
                Add-Failure "MavenReference in $RelativeProjectPath is missing Include or Version."
                continue
            }

            $References.Add([pscustomobject]@{
                Name = $Name
                Version = $Version
                Project = $RelativeProjectPath
            })
        }
    }

    return @($References | Sort-Object Name, Version -Unique)
}

function Invoke-OsvMavenScan {
    if ($SkipOsv) {
        Write-Section "Skipping OSV Maven vulnerability scan"
        return
    }

    Write-Section "Scanning MavenReference packages with OSV"
    $References = @(Get-MavenReferences)
    if ($References.Count -eq 0) {
        Write-Host "No MavenReference packages found."
        return
    }

    $Queries = @(
        foreach ($Reference in $References) {
            @{
                package = @{
                    ecosystem = "Maven"
                    name = $Reference.Name
                }
                version = $Reference.Version
            }
        }
    )

    $Body = @{ queries = $Queries } | ConvertTo-Json -Depth 8
    try {
        $Response = Invoke-RestMethod `
            -Method Post `
            -Uri "https://api.osv.dev/v1/querybatch" `
            -ContentType "application/json" `
            -Body $Body
    }
    catch {
        Add-Failure "OSV Maven vulnerability scan failed. $($_.Exception.Message)"
        return
    }

    $Results = @($Response.results)
    for ($Index = 0; $Index -lt $References.Count; $Index++) {
        $Reference = $References[$Index]
        $Result = if ($Index -lt $Results.Count) { $Results[$Index] } else { $null }
        $Vulnerabilities = Get-PropertyValues -Object $Result -Name "vulns"

        foreach ($Vulnerability in $Vulnerabilities) {
            $Aliases = if ($Vulnerability.aliases) { " ($($Vulnerability.aliases -join ', '))" } else { "" }
            Add-Failure "Maven vulnerability found in $($Reference.Name) $($Reference.Version): $($Vulnerability.id)$Aliases"
        }
    }
}

Invoke-NuGetVulnerabilityScan
Invoke-OsvMavenScan

if ($Failures.Count -gt 0) {
    Write-Host ""
    Write-Host "Security scan failed with $($Failures.Count) issue(s)." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Security scan passed."
