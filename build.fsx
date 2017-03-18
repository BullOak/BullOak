// include Fake lib

#r @"fake/FakeLib.dll"
open Fake

let buildDir = "./build/"

Target "Clean" (fun _ ->
  CleanDir buildDir
  trace "Cleaned buildDir")


// start build
RunTargetOrDefault "Clean"
