namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad

module RealisticSnow =
    let flakeLoop ts te image =
        let (fx, fy) = SbRandom.choice [(SbRandom.randX() + 100, -100)]
        let s = SbRandom.choice [0.1f; 0.1f; 0.1f; 0.2f; 0.2f; 0.3f; 0.7f] + SbRandom.randFloat() |> (*) 0.4f |> fun x -> x * x
        let duration = 5000f / ((1f + s) * (1f + s)) |> int
        let f = 0.7f - s * 0.6f
        let s = s * 0.6f
        let iterations = te - ts |> fun x -> x / duration
        img image
        >>= monadicMap [1..iterations] (fun i ->
        let ts, te = ts + duration * i, ts + duration * (i + 1)
        scale ts ts s s
        >>= fade ts ts f f
        >>= rotate ts te 0f -0.2f
        >>= movey ts te (fy + SbRandom.randInt 0 100) (fy + 580 + SbRandom.randInt 0 100) >> easing Easing.SineInOut
        >>= movex ts te (fx) (fx - 300))

    let effect ts te =
        let image = gp_filled_circle |> resize1To 32 |> pad 6 |> gaussBlur 4f
        printfn $"Making snow ({ts}/{te})"
        monadicMap [0..128] (fun i -> flakeLoop (ts + (SbRandom.randInt 0 (min 5000 (te - ts)))) te image)