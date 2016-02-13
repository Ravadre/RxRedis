#r "packages/FAKE/tools/Fakelib.dll"
open Fake

let runPacket args =
    directExec (
        fun p ->
            p.Arguments <- args
            p.FileName <- ".\\.paket\\paket.exe")

Target "Pack" (fun _ -> 
    runPacket "pack output ." |> ignore
)

Target "Paket" (fun() ->
    runPacket "install" |> ignore
)

Target "Help" (fun _ ->
    trace ""
    trace "Targets: "
    trace "\tPack   -  Creates nuget package"
    trace "\tPaket  -  Installs nuget dependencies"
    trace ""
)

RunTargetOrDefault "Help"