namespace Storyboarding.Storyboards

open Storyboarding.Tools
open Storyboarding.Tools.SbMonad

module HidamariNoUta =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2420040 Apol - Hidamari no Uta\Apol - Hidamari no Uta (Asahina Oleshev).osb"

    let story =
        id

    let make () =
        openSb path
        |> story
        |> SbCompiler.write