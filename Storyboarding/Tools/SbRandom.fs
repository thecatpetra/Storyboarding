namespace Storyboarding.Tools

open System
open Storyboarding.Tools.SbTypes

module SbRandom =
    let mutable prevPosition : Position = (0, 0)
    let mutable prevColor : Color = (0, 0, 0)
    let rng = Random(3241)

    let randX () = rng.Next(-114, 754)
    let randY () = rng.Next(-4, 500)

    let randFloat = rng.NextSingle
    let randInt a b = rng.Next(a, b)
    let randPosition () : Position =
        prevPosition <- (randX (), randY ())
        prevPosition

    let randColor () : Color =
        prevColor <- (rng.Next(0, 255), rng.Next(0, 255), rng.Next(0, 255))
        prevColor

    let samePosition () = prevPosition
    let sameColor () = prevColor

    let choice (set : 'a list) =
        List.item (List.length set |> rng.Next) set