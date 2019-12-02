#!/usr/bin/env pwsh

Param(
    [ValidateNotNullOrEmpty()]
    [string]$Target = "Default",

    [ValidateNotNullOrEmpty()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Version = "0.0.1",

    [string]$DotnetVerbosity = "minimal"
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
# TARGETS MANAGEMENT

Function DependsOn {
    Param([ValidateNotNullOrEmpty()] [string]$Target)
    Invoke-Expression $Target
}

Function IsTargetCompleted {
    Param([ValidateNotNullOrEmpty()] [string]$Target)
    $completedTargets[$Target]
}

Function MarkTargetAsCompleted {
    Param([ValidateNotNullOrEmpty()] [string]$Target)
    $completedTargets[$Target] = $True
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
    LogStep "dotnet restore $dotnetSolutionFile --verbosity $DotnetVerbosity"
    & dotnet restore "$dotnetSolutionFile" --verbosity $DotnetVerbosity
}

Function Step_DotnetBuild {
    LogStep "dotnet build $dotnetSolutionFile --no-restore --configuration $Configuration --verbosity $DotnetVerbosity /p:Version=$Version"
    & dotnet build "$dotnetSolutionFile" --no-restore --configuration $Configuration --verbosity $DotnetVerbosity /p:Version="$Version"
}

Function Step_DotnetTest {
    Param([ValidateNotNullOrEmpty()] [string]$ProjectFile)
    LogStep "dotnet test $ProjectFile --no-build --configuration $Configuration"
    & dotnet test "$ProjectFile" --no-build --configuration $Configuration
}

#######################################################################
# TARGETS

Function Target_Clean {
    $targetDotnetClean = "Dotnet.Clean"
    if (IsTargetCompleted $targetDotnetClean) { return }
    LogTarget $targetDotnetClean
    Step_DotnetClean
    MarkTargetAsCompleted $targetDotnetClean
}

Function Target_Restore {
    $targetDotnetRestore = "Dotnet.Restore"
    if (IsTargetCompleted $targetDotnetRestore) { return }
    LogTarget $targetDotnetRestore
    Step_DotnetRestore
    MarkTargetAsCompleted $targetDotnetRestore
}

Function Target_Build {
    $targetDotnetBuild = "Dotnet.Build"
    if (IsTargetCompleted $targetDotnetBuild) { return }
    DependsOn Target_Restore

    LogTarget $targetDotnetBuild
    Step_DotnetBuild
    MarkTargetAsCompleted $targetDotnetBuild
}

Function Target_TestUnit {
    $targetDotnetTestUnit = "DotNet.TestUnit"
    if (IsTargetCompleted $targetDotnetTestUnit) { return }
    DependsOn Target_Build

    LogTarget $targetDotnetTestUnit
    $projects = Get-ChildItem -Path $srcDir -Filter "*Test.Unit.csproj" -Recurse -File
    Foreach ($projectFile in $projects) {
        Step_DotnetTest $projectFile
    }
    MarkTargetAsCompleted $targetDotnetTestUnit
}

Function Target_TestAcceptance {
    $targetDotnetTestAcceptance = "DotNet.TestAcceptance"
    if (IsTargetCompleted $targetDotnetTestAcceptance) { return }
    DependsOn Target_Build

    LogTarget $targetDotnetTestAcceptance
    $projects = Get-ChildItem -Path $srcDir -Filter "*Test.Acceptance.csproj" -Recurse -File
    Foreach ($projectFile in $projects) {
        Step_DotnetTest $projectFile
    }
    MarkTargetAsCompleted $targetDotnetTestAcceptance
}

Function Target_FullBuild {
    $targetFullBuild = "FullBuild"
    if (IsTargetCompleted $targetFullBuild) { return }
    DependsOn Target_Build
    DependsOn Target_TestUnit
    DependsOn Target_TestAcceptance

    LogTarget $targetFullBuild
    MarkTargetAsCompleted $targetFullBuild
}

Function Target_Default {
    DependsOn Target_FullBuild
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
