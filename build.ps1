#!/usr/bin/env pwsh

Param(
    [ValidateNotNullOrEmpty()]
    [string]$Target = "Default",

    [ValidateNotNullOrEmpty()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateNotNullOrEmpty()]
    [ValidateSet("any", "linux-arm", "linux-x64", "osx-x64", "win-x64")]
    [string]$Runtime = "linux-x64",

    [string]$DotnetVerbosity = "minimal",

    [string]$Version = "1.0.0"
)

#######################################################################
# SHARED VARIABLES

$buildDir = $PSScriptRoot
$repositoryDir = (Get-Item $buildDir).FullName
$srcDir = [System.IO.Path]::Combine($repositoryDir, "src")
$dotnetSolutionFile = Get-ChildItem -Path $srcDir -Filter "*.sln" | Select-Object -First 1

$requiredDotnetVersion = "2.2"

$completedTargets = @{}

# This build system expects following solution structure:
# solution_root/
#   build.ps1                  -- PowerShell build CLI
#   src/
#     Project1/
#       Project1.csproj        -- project base filename matches directory name
#     Project1.Tests/
#       Project1.Tests.csproj  -- tests projects are xUnit-based; project name must have suffix '.Tests'
#     Project2/
#       Project2.fsproj
#       imagename.Dockerfile   -- if the `*.Dockerfile` is present, we'll build Docker image `imagename`
#     Project2.Tests/
#       Project2.Tests.fsproj
#     SolutionName.sln         -- only one '.sln' file in 'src'

#######################################################################
# LOGGING

Function LogInfo {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Green $Message
}

Function LogWarning {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "*** $Message"
}

Function LogError {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Red "*** $Message"
}

Function LogStep {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "--- STEP: $Message"
}

Function LogTarget {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Green "--- TARGET: $Message"
}

Function LogCmd {
    Param([ValidateNotNullOrEmpty()] [string]$Message)
    Write-Host -ForegroundColor Yellow "--- $Message"
}

#######################################################################
# STEPS

Function PreludeStep_ValidateDotNetCli {
    LogStep "Prelude: .NET CLI"
    # Check if a suitable .NET CLI is available
    $requiredDotnetMessage = "*** Required 'dotnet' version $requiredDotnetVersion or higher. Install .NET Core SDK from https://www.microsoft.com/net/download"
    try {
        [System.Version]$dotnetVersion = dotnet --version
        [System.Version]$minSupportedDotnetVersion = $requiredDotnetVersion

        if (-Not($dotnetVersion -ge $minSupportedDotnetVersion)) {
            LogError Red $requiredDotnetMessage
            Exit 1
        }
    }
    catch {
        LogError $requiredDotnetMessage
        Exit 1
    }
}

Function Step_PruneBuild {
    LogStep "PruneBuild"

    $pruneDir = $repositoryDir
    LogWarning "Pruning $pruneDir build artifacts"

    # Prune nested directories
    'bin', 'obj', 'publish' | ForEach-Object {
        Get-ChildItem -Path $pruneDir -Filter $_ -Directory -Recurse | ForEach-Object { $_.Delete($true) }
    }

    # Prune nested files
    '*.trx', '*.fsx.lock', '*.Tests_*.xml' | ForEach-Object {
        Get-ChildItem -Path $pruneDir -Filter $_ -File -Recurse | ForEach-Object { $_.Delete() }
    }

    # Prune top-level items
    '.fable', '.ionide', 'build/.fake', 'build/tools/fake-cli', 'build/tools/pbm', 'node_modules', 'paket-files' | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item -Path $_ -Recurse -Force
        }
    }
}

Function Step_DotnetClean {
    LogStep "dotnet clean $dotnetSolutionFile --verbosity $DotnetVerbosity"
    & dotnet clean "$dotnetSolutionFile" --verbosity $DotnetVerbosity
}

Function Step_DotnetRestore {
    LogStep "dotnet restore $dotnetSolutionFile --runtime $Runtime --verbosity $DotnetVerbosity"
    & dotnet restore "$dotnetSolutionFile" --runtime $Runtime --verbosity $DotnetVerbosity
}

Function Step_DotnetBuild {
    LogStep "dotnet build $dotnetSolutionFile --no-restore --configuration $Configuration --runtime $Runtime --verbosity $DotnetVerbosity /p:Version=$Version"
    & dotnet build "$dotnetSolutionFile" --no-restore --configuration $Configuration --runtime $Runtime --verbosity $DotnetVerbosity /p:Version=$Version
}

Function Step_DotnetPublish {
    Param([ValidateNotNullOrEmpty()] [string]$ProjectFile, [ValidateNotNullOrEmpty()] [string]$PublishOutput)
    LogStep "dotnet publish $ProjectFile --output $PublishOutput --configuration $Configuration --runtime $Runtime --verbosity $DotnetVerbosity /p:Version=$Version"
    & dotnet publish "$ProjectFile" --output "$PublishOutput" --configuration $Configuration --runtime $Runtime --verbosity $DotnetVerbosity /p:Version=$Version
}

Function Step_DotnetTest {
    Param([ValidateNotNullOrEmpty()] [string]$ProjectFile)
    LogStep "dotnet test $ProjectFile --no-build --configuration $Configuration --runtime $Runtime --test-adapter-path:. --logger:xunit"
    & dotnet test "$ProjectFile" --no-build --configuration $Configuration --runtime $Runtime
}

#######################################################################
# TARGETS

Function Target_Clean {
    if ($completedTargets["DotNet.Clean"] -eq $True) {return}
    LogTarget "DotNet.Clean"
    Step_DotnetClean
    $completedTargets["DotNet.Clean"] = $True
}

Function Target_Restore {
    if ($completedTargets["DotNet.Restore"] -eq $True) {return}
    LogTarget "DotNet.Restore"
    Step_DotnetRestore
    $completedTargets["DotNet.Restore"]  = $True
}

Function Target_Build {
    if ($completedTargets["DotNet.Build"] -eq $True) {return}
    Target_Restore

    LogTarget "DotNet.Build"
    Step_DotnetBuild
    $completedTargets["DotNet.Build"] = $True
}

Function Target_TestUnit {
    if ($completedTargets["DotNet.TestUnit"] -eq $True) {return}
    Target_Build

    LogTarget "DotNet.TestUnit"
    $projects = Get-ChildItem -Path $srcDir -Filter "*Test.Unit.csproj" -Recurse -File
    Foreach ($projectFile in $projects) {
        Step_DotnetTest $projectFile
    }
    $completedTargets["DotNet.TestUnit"] = $True
}

Function Target_TestAcceptance {
    if ($completedTargets["DotNet.TestAcceptance"] -eq $True) {return}
    Target_Build

    LogTarget "DotNet.TestAcceptance"
    $projects = Get-ChildItem -Path $srcDir -Filter "*Test.Acceptance.csproj" -Recurse -File
    Foreach ($projectFile in $projects) {
        Step_DotnetTest $projectFile
    }
    $completedTargets["DotNet.TestAcceptance"] = $True
}

Function Target_FullBuild {
    Target_Build
    Target_TestUnit
    Target_TestAcceptance

    LogTarget "FullBuild"
}

Function Target_Default {
    Target_FullBuild
    LogInfo "DONE"
}

#######################################################################
# PRUNE TARGETS

if ($Target -eq "Prune") {
    Step_PruneBuild
    Exit 0
}

#######################################################################
# PRELUDE

PreludeStep_ValidateDotNetCli

#######################################################################
# MAIN ENTRY POINT

LogInfo "*** BUILD: $Target ($Configuration) in $repositoryDir"

Invoke-Expression "Target_$Target"
