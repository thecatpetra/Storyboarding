namespace Storyboarding.Storyboards

open Storyboarding
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad

module M =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2242943 WAGAMAMA RAKIA - M\WAGAMAMA RAKIA - M (Hyokar).osb"

    let make () =
        openSb path
        |> forEachHitObject Gameplay.Lines.anyObject
        |> SbCompiler.write

