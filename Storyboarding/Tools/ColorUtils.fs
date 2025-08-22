namespace Storyboarding.Tools

open Storyboarding.Tools.SbTypes

module ColorUtils =
    let toFun = snd

    type IndexedColorFun = int * (int -> Color)

    let indexedGradient cFrom cTo amount : IndexedColorFun =
        amount, fun i ->
            assert (i < amount)
            lerpColor cFrom cTo (float32 i / float32 amount)

    let indexedSolid color amount : IndexedColorFun =
        amount, fun i ->
            assert (i < amount)
            color

    let indexedGradientT cFrom cTo string = String.length string |> indexedGradient cFrom cTo
    let indexedSolidT color string = String.length string |> indexedSolid color

    let combine (f : IndexedColorFun) (g : IndexedColorFun) : IndexedColorFun =
        let af, f = f
        let ag, g = g
        af + ag, fun i -> if i < af then f i else g (i - af)

    let (>>>>) = combine

    let hsv (red, green, blue) =
        let max3 a b c = max a b |> max c
        let rNorm = (float32 red) / 255f
        let gNorm = (float32 green) / 255f
        let bNorm = (float32 blue) / 255f
        let cMax = max3 rNorm gNorm bNorm
        let cMin = max3 rNorm gNorm bNorm
        let diff = cMax - cMin
        let h =
            match cMax with
            | cMax when cMax = cMin -> 0
            | cMax when cMax = rNorm -> (int (60f * ((gNorm - bNorm) / diff) + 360f)) % 360
            | cMax when cMax = rNorm -> (int (60f * ((bNorm - rNorm) / diff) + 120f)) % 360
            | cMax when cMax = rNorm -> (int (60f * ((rNorm - gNorm) / diff) + 240f)) % 360
            | _ -> 0
        let s = if cMax = 0f then 0 else (diff / cMax |> int)
        let v = cMax * 100f |> int
        (h, s, v)