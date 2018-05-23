Param(
    [ValidateNotNullOrEmpty()]
    [string]$Target="Default",

    [ValidateNotNullOrEmpty()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration="Release"
)

$nugetVersion="4.6.2"
$fakeVersion="4.64.13"

$buildDir=$PSScriptRoot
$buildScript=[System.IO.Path]::Combine($buildDir, "build.fsx")

$solutionDir=(Get-Item $buildDir).FullName
$nugetPackagesDir=[System.IO.Path]::Combine($solutionDir, "packages")
$toolsDir=[System.IO.Path]::Combine($solutionDir, "tools")
$nuget=[System.IO.Path]::Combine($toolsDir, "NuGet-$nugetVersion", "nuget.exe")
$fake=[System.IO.Path]::Combine($nugetPackagesDir, "FAKE.$fakeVersion", "tools", "FAKE.exe")

Write-Host -ForegroundColor Green "*** Building $Configuration in $solutionDir"

& "$nuget" install FAKE -OutputDirectory $nugetPackagesDir -Version $fakeVersion -Verbosity quiet

Write-Host -ForegroundColor Green "***    FAKE it"
& "$fake" "$buildScript" "$Target" Configuration="$Configuration"
