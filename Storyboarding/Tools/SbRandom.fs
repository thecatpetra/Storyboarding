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

    let randEdgePosition () : Position =
        match randInt 0 4 with
        | 0 -> (randX(), 0)
        | 1 -> (randX(), 480)
        | 2 -> (-114, randY())
        | _ -> (754, randY())

    let randColor () : Color =
        prevColor <- (rng.Next(0, 255), rng.Next(0, 255), rng.Next(0, 255))
        prevColor

    let samePosition () = prevPosition
    let sameColor () = prevColor

    let skip n = Seq.iter (fun _ -> randInt 1 3 |> ignore) [1..n]
    let choice (set : 'a list) =
        List.item (List.length set |> rng.Next) set

    let randomVector (length : float32) : Position =
        ((MathF.Cos(randFloat() * MathF.PI * 4f) * length) |> int, (MathF.Sin(randFloat() * MathF.PI * 4f) * length) |> int)