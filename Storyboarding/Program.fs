namespace Storyboarding

open NAudio.Wave
open Storyboarding.Storyboards
open Storyboarding.Tools

module Program =
    let signalSuccess () =
        printfn $"Build finished, current time: {System.DateTime.Now}"

    [<EntryPoint>]
    let main args =
        HidamariNoUta.make ()
        signalSuccess()
        0