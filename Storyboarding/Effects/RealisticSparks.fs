namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module RealisticSparks =
    let singleSpark timeStart timeEnd =
        let sparkleImage = SbRandom.choice [ Resources.spark_droplet; Resources.spark_line ]
        let firstPart = SbRandom.rng.NextSingle()
        let deltaX = 400
        let startX = SbRandom.randX () - 100
        let startPosition = startX |> fun x -> (x, 500)
        let endPosition = startX + deltaX |> fun x -> (x, -4)
        let middlePosition = lerp startPosition endPosition firstPart
        let timeMiddle = lerpTime timeStart timeEnd firstPart
        let timeQuarter1 = lerpTime timeStart timeMiddle 0.5f
        let timeQuarter3 = lerpTime timeMiddle timeEnd 0.5f
        let rotH = -0.3f
        let rotY = 0.5f
        img sparkleImage
        >>= scale timeStart timeStart 0.3f 0.3f
        // Rotation 1
        >>= rotate timeStart (timeQuarter1 - 3) rotH rotY
        >>= rotate timeQuarter1 (timeMiddle - 3) rotY rotH
        // Rotation 2
        >>= rotate timeMiddle (timeQuarter3 - 3) rotH rotY
        >>= rotate timeQuarter3 (timeEnd - 3) rotY rotH
        // Movement
        >>= movex timeStart timeMiddle (startPosition |> fst) (middlePosition |> fst)
        >>= easing Easing.In
        >>= movey timeStart timeMiddle (startPosition |> snd) (middlePosition |> snd)
        >>= easing Easing.Out
        >>= movex timeMiddle timeEnd (middlePosition |> fst) (endPosition |> fst)
        >>= easing Easing.Out
        >>= movey timeMiddle timeEnd (middlePosition |> snd) (endPosition |> snd)
        >>= easing Easing.In

    let effect timeStart timeEnd =
        printfn $"Making sparks ({timeStart}/{timeEnd})"
        let density = 100
        let iterations = timeEnd - timeStart |> fun x -> x / density
        monadicMap [0..iterations] (fun i ->
        let time = i * density + timeStart
        singleSpark time (time + 2000))

