namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module AscendingParticles =
    let particleLoop ts te image fcf duration alpha =
        let (fx, fy) = SbRandom.choice [(SbRandom.randX() - 200, 500)]
        let s = SbRandom.choice [0.1f; 0.1f; 0.1f; 0.2f; 0.2f; 0.3f; 0.7f] + SbRandom.randFloat() |> (*) 0.4f
        let duration = duration / ((1f + s) * (1f + s)) |> int
        let f = 1f - s * 0.4f
        let iterations = te - ts |> fun x -> x / duration
        img image
        >>= fade ts ts f f
        >>= color ts ts (fcf f) (fcf f)
        >>= scale ts ts s s
        >>= loopIterations (ts + duration) iterations (
        let ts, te = 0, duration
        movex ts te (fx + SbRandom.randInt 0 100) (fx + 200 + SbRandom.randInt 0 100) >> easing Easing.SineInOut
        >>= movey ts te fy (fy - 500))
        >>= fade (te - 200) te f 0f

    let effect ts te texture alpha (fcf: float32 -> Color) =
        printfn $"Making Ascending Particles ({ts}/{te})"
        monadicMap [0..64] (fun i -> particleLoop ts te texture fcf 5000f alpha)

    let fastEffect ts te texture alpha (fcf: float32 -> Color) =
        printfn $"Making Fast Particles ({ts}/{te})"
        monadicMap [0..64] (fun i -> particleLoop ts te texture fcf 2000f alpha)