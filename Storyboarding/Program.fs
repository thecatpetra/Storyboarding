namespace Storyboarding

open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Storyboards

module Program =
    let signalSuccess () =
        let player = new System.Media.SoundPlayer()
        player.SoundLocation <- @"C:\Users\arthur\Documents\Storyboarding\Storyboarding\Resources\sound\bonk_ex.wav"
        printfn $"Build finished, current time: {System.DateTime.Now}"
        player.PlaySync()

    [<EntryPoint>]
    let main args =
        WithABurningHeart.make ()
        signalSuccess()
        0