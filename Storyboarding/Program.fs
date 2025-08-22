namespace Storyboarding

open System.IO
open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Storyboards
open Storyboarding.Tools

module Program =
    let signalSuccess () =
        let player = new System.Media.SoundPlayer()
        player.SoundLocation <- @"C:\Users\arthur\Documents\Storyboarding\Storyboarding\Resources\sound\bonk_ex.wav"
        printfn $"Build finished, current time: {System.DateTime.Now}"
        player.PlaySync()

    [<EntryPoint>]
    let main args =
        SidetrackedDay.make ()
        signalSuccess()
        0