namespace Storyboarding.Storyboards

open Storyboarding.Tools
open Storyboarding.Tools.SbMonad

module Isolation =
    let path = @"NH22 - Isolation (Official LIMBO Remix) (TheCatPetra).osb"

    let story =
        id

    let make () =
        openSb path
        |> story
        |> SbCompiler.write
