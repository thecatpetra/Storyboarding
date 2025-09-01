namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.ColorUtils
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module Transition =
    // Defaults: fadeTime = 100
    let dim startTime endTime fadeTime =
        printfn $"Transition dim ({endTime}/{fadeTime})"
        let allScreenScale = 10f
        img square_black >> layer Foreground
        >>= scale startTime startTime allScreenScale allScreenScale
        >>= fade startTime (startTime + fadeTime) 0f 1f
        >>= fade endTime endTime 1f 0f

    let chromoFlash timeStart timeEnd endFin stay =
        printfn $"Transition chromo flash ({timeStart}/{timeEnd}/{endFin}/{stay})"
        let e = Easing.SineIn
        img gradient >> origin BottomCentre >> move timeStart timeStart (240, 650) (240, 650) >> layer Foreground
        >>= fade timeStart timeEnd 0f 1f >>= easing e
        >>= vectorScale timeStart timeEnd (18f, 0f) (18f, 30f) >>= easing e
        >>= rotate timeStart timeStart 0.4f 0.4f
        >>= alpha
        >>= color timeStart timeStart (255, 0, 0) (255, 0, 0)
        >>= fade endFin (endFin + stay) 1f 0f
        >> img gradient >> origin BottomCentre >> move timeStart timeStart (480, 650) (480, 650) >> layer Foreground
        >>= fade timeStart timeEnd 0f 1f >>= easing e
        >>= vectorScale timeStart timeEnd (18f, 0f) (18f, 30f) >>= easing e
        >>= rotate timeStart timeStart -0.4f -0.4f
        >>= alpha
        >>= color timeStart timeStart (0, 0, 255) (0, 0, 255)
        >>= fade endFin (endFin + stay) 1f 0f
        >> img gradient >> origin BottomCentre >> move timeStart timeStart (360, 650) (360, 650) >> layer Foreground
        >>= fade timeStart timeEnd 0f 1f >>= easing e
        >>= vectorScale timeStart timeEnd (18f, 0f) (18f, 30f) >>= easing e
        >>= rotate timeStart timeStart 0f 0f
        >>= alpha
        >>= color timeStart timeStart (0, 255, 0) (0, 255, 0)
        >>= fade endFin (endFin + stay) 1f 0f

    let blackCurtains dimStart dimEnd openStart openEnd =
        printfn $"Transition black curtains ({dimStart}/{dimEnd}/{openStart}/{openEnd})"
        dim dimStart dimEnd (dimEnd - dimStart)
        >> img square_black >> origin BottomCentre >> layer Foreground
        >>= move openStart openStart (320, 480) (320, 500)
        >>= vectorScale openStart openEnd (7f, 2f) (7f, 0f) >> easing Easing.QuartOut
        >>= rotate openStart openEnd 0.01f 0.04f
        >> img square_black >> origin TopCentre >> layer Foreground
        >>= move openStart openStart (320, 0) (320, -20)
        >>= vectorScale openStart openEnd (7f, 2f) (7f, 0f) >> easing Easing.QuartOut
        >>= rotate openStart openEnd 0.01f 0.04f

    let closingSquares ts te openTime background finalColor =
        withAccessPixelColors background (fun clr ->
        printfn $"Transition closing squares ({ts}/{te}/{openTime})"
        monadicMap [1..16] (fun y ->
        monadicMap [-3..24] (fun x ->
        let ts = ts + y * 60 + x * 60 - 500
        let normalized = x |> float32 |> fun x -> (x + 3.5f) / 28f, y |> float32 |> (fun y -> (y - 0.5f) / 16f)
        img square_white >>= coords (x * 32 - 16, y * 32 - 16) >> layer Foreground
        >>= fade te (te + openTime) 1f 0f
        >>= scale ts te 0f 0.25f >> easing Easing.QuadIn
        >>= color (te - 800) te (clr normalized) finalColor >> easing Easing.QuadIn
        )))

    let threeShuttingSquares ts openTime sb =
        let beatTime = beatTime ts sb |> (*) 3
        monadicMap [-1..1] (fun i ->
        let te = ts + beatTime * 3
        let ts = beatTime * (i + 1) + ts
        img square_black >> coords (320 + i * 300, 240) >> layer Foreground
        >>= vectorScale ts (ts + beatTime / 4) (0f, 4f) (2.345f, 4f)
        >>= fade (te) (te + openTime) 1f 0f
        ) sb