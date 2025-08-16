namespace Storyboarding.Effects

open Storyboarding.Tools
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module Transition =
    // Defaults: fadeTime = 100
    let dim startTime endTime fadeTime =
        let allScreenScale = 10f
        img Resources.square_black
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
    // easing = 4
    // rotation = 0.4
    // red = "255,0,0"
    // green = "0,255,0"
    // blue = "0,0,255"
    // c_offset = 50
    // osb.write(f"Sprite,Foreground,BottomCentre,\"{gradient}\",240,650\n")
    // osb.write(f" F,{easing},{start},{_end},0,1\n")
    // osb.write(f" V,{easing},{start},{_end},18,0,18,30\n")
    // osb.write(f" R,0,{start},{start},{rotation},{rotation}\n")
    // osb.write(f" P,0,0,0,A\n")
    // osb.write(f" C,0,{start},{start},{green},{green}\n")
    // osb.write(f" F,{easing},{_end},{_end + 1200},1,0\n")
    //
    // osb.write(f"Sprite,Foreground,BottomCentre,\"{gradient}\",480,650\n")
    // osb.write(f" F,{easing},{start},{_end},0,1\n")
    // osb.write(f" V,{easing},{start + c_offset},{_end},18,0,18,30\n")
    // osb.write(f" R,0,{start},{start},{-rotation},{-rotation}\n")
    // osb.write(f" P,0,0,0,A\n")
    // osb.write(f" C,0,{start},{start},{blue},{blue}\n")
    // osb.write(f" F,{easing},{_end},{_end + 1200},1,0\n")
    //
    // osb.write(f"Sprite,Foreground,BottomCentre,\"{gradient}\",360,650\n")
    // osb.write(f" F,{easing},{start},{_end},0,1\n")
    // osb.write(f" V,{easing},{start + 2 * c_offset},{_end},18,0,18,30\n")
    // osb.write(f" R,0,{start},{start},{0},{0}\n")
    // osb.write(f" P,0,0,0,A\n")
    // osb.write(f" C,0,{start},{start},{red},{red}\n")
    // osb.write(f" F,{easing},{_end},{_end + 1200},1,0\n")