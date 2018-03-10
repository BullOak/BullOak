#r @"./packages/FAKE.4.64.10/tools/FakeLib.dll"

open Fake
open System
open System.Diagnostics
open System.IO

module Properties =
    let buildConfiguration = getBuildParamOrDefault "Configuration" "Release"

    module Internal =

        // Absolute path to solution locations
        let repositoryDir = DirectoryInfo(__SOURCE_DIRECTORY__).FullName
        let packagesDir = Path.Combine(repositoryDir, "packages")
        let sourceDir = Path.Combine(repositoryDir, "src")
        let solutionFile = Path.Combine(sourceDir, "BullOak.sln")

        // Tests
        let testsProjectPathPatternByType = fun t -> sprintf @"%s\**\*.Test.%s.csproj" sourceDir t
        let runTestsInProject = fun x -> DotNetCli.Test (fun p ->
            {   p with
                    Project = x
                    Configuration = buildConfiguration
                    AdditionalArgs = [ "--logger"; "trx"; ]
            })

///////////////////////////////////////////////////////////////////////
// Targets
///////////////////////////////////////////////////////////////////////

module Targets =
    open Properties
    open Properties.Internal

    Target "Purge" (fun _ ->
        let script = sprintf "$startPath = '%s'; Get-ChildItem -Path $startPath -Filter 'bin' -Directory -Recurse | Foreach { $_.Delete($true) }; Get-ChildItem -Path $startPath -Filter 'obj' -Directory -Recurse | Foreach { $_.Delete($true) }" repositoryDir
        let powershellWrapper = sprintf """/c powershell -ExecutionPolicy Unrestricted -Command "%s" """ script

        let p = new Process()
        p.StartInfo.FileName <- "cmd.exe"
        p.StartInfo.Arguments <- powershellWrapper
        p.StartInfo.RedirectStandardOutput <- true
        p.StartInfo.UseShellExecute <- false
        p.Start() |> ignore

        printfn "%s" (p.StandardOutput.ReadToEnd())
    )

    Target "Clean" (fun _ ->
        DotNetCli.RunCommand (fun p ->
             { p with
                 TimeOut = TimeSpan.FromMinutes 10.
             }) (sprintf "clean \"%s\"" solutionFile)
    )

    Target "Restore" (fun _ ->
        solutionFile
        |> RestoreMSSolutionPackages (fun p ->
            {   p with
                    OutputPath = packagesDir
                    Retries = 3
            })
    )

    Target "Build" (fun _ ->
        DotNetCli.Build (fun p ->
            { p with
                Project = solutionFile
                Configuration = buildConfiguration
            })
    )

    Target "UnitTests" (fun _ ->
        !! (testsProjectPathPatternByType "Unit")
        |> Seq.iter runTestsInProject
    )

    Target "AcceptanceTests" (fun _ ->
        !! (testsProjectPathPatternByType "Acceptance")
        |> Seq.iter runTestsInProject
    )

    Target "IntegrationTests" (fun _ ->
        !! (testsProjectPathPatternByType "Integration")
        |> Seq.iter runTestsInProject
    )

    // Labels to manage dependencies
    Target "FullBuild" DoNothing
    Target "Default" DoNothing

///////////////////////////////////////////////////////////////////////
// Dependencies
///////////////////////////////////////////////////////////////////////

"Restore" ==> "Build"
"Build" ==> "UnitTests"
"Build" ==> "AcceptanceTests"
"Build" ==> "IntegrationTests"
"UnitTests" ==> "FullBuild"
"AcceptanceTests" ==> "FullBuild"
"IntegrationTests" ==> "FullBuild"

// Default target
"FullBuild" ==> "Default"

// Start
RunTargetOrDefault "Default"
