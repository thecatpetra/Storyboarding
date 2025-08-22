namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad

module Cube =
    let sqStay c ts te =
        let position = 320, 240
        img square_white
        >>= move ts te position position
        >>= color ts ts c c

    // let sqRotate cPrev cNext ts te =
    //     let position = 320, 240
    //     let rWidth = 64
    //     // Prev face
    //     img square_white >> origin Origin.CentreLeft >> coords (position +++ (-rWidth, 0))
        // >>= color ts ts cPrev cPrev
        // >>= vectorScale ts te (1f, 1f) (0f, farScale) >> easing Easing.SineIn
        // >>= alpha
        // // Next Face
        // >> img square_white >> origin Origin.CentreRight >> coords (position +++ (rWidth, 0))
        // >>= color ts ts cNext cNext
        // >>= vectorScale ts te (0f, farScale) (1f, 1f) >> easing Easing.SineIn
        // >>= alpha
        // monadicMap (triangles) (fun (o, corner, c) ->
        // img triangle_white >> origin o >> coords corner
        // >>= color ts ts c c
        // >>= vectorScale ts te (1f, 1f) (0f, farScale) >> easing Easing.SineIn)
