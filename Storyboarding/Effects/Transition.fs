namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.SbMonad

module Transition =
    // Defaults: fadeTime = 100
    let dim startTime endTime fadeTime =
        let allScreenScale = 10f
        img Resources.square_black
        >>= scale startTime startTime allScreenScale allScreenScale
        >>= fade startTime (startTime + fadeTime) 0f 1f
        >>= fade endTime endTime 1f 0f