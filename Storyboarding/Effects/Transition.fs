namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module Transition =
    // Defaults: fadeTime = 100
    let dim startTime endTime fadeTime =
        let allScreenScale = 10f
        img square_black
        >>= scale startTime startTime allScreenScale allScreenScale
        >>= fade startTime (startTime + fadeTime) 0f 1f
        >>= fade endTime endTime 1f 0f

    let chromoFlash timeStart timeEnd endFin stay =
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
        dim dimStart dimEnd (dimEnd - dimStart)
        >> img square_black >> origin BottomCentre >> layer Foreground
        >>= move openStart openStart (320, 480) (320, 500)
        >>= vectorScale openStart openEnd (7f, 2f) (7f, 0f) >> easing Easing.QuartOut
        >>= rotate openStart openEnd 0.01f 0.04f
        >> img square_black >> origin TopCentre >> layer Foreground
        >>= move openStart openStart (320, 0) (320, -20)
        >>= vectorScale openStart openEnd (7f, 2f) (7f, 0f) >> easing Easing.QuartOut
        >>= rotate openStart openEnd 0.01f 0.04f
