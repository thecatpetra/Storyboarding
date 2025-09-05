namespace Storyboarding

open System.IO
open GpuDrawingUtils
open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Storyboards
open Storyboarding.Tools

module Program =
    let signalSuccess () =
        let player = new System.Media.SoundPlayer()
        player.SoundLocation <- Paths.soundPath
        printfn $"Build finished, current time: {System.DateTime.Now}"
        player.PlaySync()

    [<EntryPoint>]
    let main args =
        HidamariNoUta.make ()
        signalSuccess()
        0