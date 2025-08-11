namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.SbMonad

module RisingSmoke =
    let effect timeStart timeEnd =
        let density = 1000
        let iterations = timeEnd - timeStart |> fun x -> x / density
        let iterationTime = (timeEnd - timeStart) / iterations |> int
        monadicMap [0..iterations] (fun i ->
        let iterationStart, iterationEnd = i * iterationTime + timeStart, i * iterationTime + 4000 + timeStart
        let x = SbRandom.randX ()
        let initialPosition = (x, 600)
        let finalPosition = (x + 100, 200)
        let ascentStart = iterationStart + 500
        img Resources.fog
        >>= scale iterationStart iterationStart 0.3f 0.3f
        >>= verticalFlip
        >>= fade iterationStart ascentStart 0f 0.7f
        >>= move ascentStart iterationEnd initialPosition finalPosition
        >>= color iterationStart iterationStart (0, 0, 0) (0, 0, 0)
        >>= fade ascentStart iterationEnd 0.7f 0f)
