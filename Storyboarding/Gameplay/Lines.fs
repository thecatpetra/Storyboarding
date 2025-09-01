namespace Storyboarding.Gameplay

open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Gameplay.Common
open Storyboarding.Gameplay.Util
open Storyboarding.Tools
open Storyboarding.Tools.GeometryUtils
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad

module Lines =
    let circle (hitCircle : HitObject) =
        circle hitCircle { hitCircle = gp_simple_circle; approachCircle = gp_simple_approach_circle; scale = 0.4f }

    let sliderLine (slider : HitObject) =
        assert (slider.``type`` &&& HitObject.Type.Slider = HitObject.Type.Slider)
        let step = 20
        let segments = getSliderSegments slider step |> Seq.toList
        // DrawingUtils.drawSliderBody (getSliderSegmentsF slider step |> Seq.toList)
        let diffSettings = slider.beatmap.difficultySettings
        let arData = arToTime diffSettings.approachRate
        let knee position time =
            let filledCircleScale = 0.11f
            img gp_dot32
            >>= scale (time - arData.preempt) (time + 500 + step) filledCircleScale filledCircleScale
            >>= move (time - arData.preempt) (time + 500 + step) position position
        circle slider
        >>= knee (v22p slider.Position) (slider.time - step)
        >>= monadicMap segments (fun segment ->
        let customLs = { defaultLS with height = 0.1f; widthModifier = defaultLS.widthModifier * 4f; image = gp_square32 }
        let repr = getLsParams segment.first segment.second customLs
        drawStaticLs (segment.time - arData.preempt) (segment.time + 500) repr
        >>= knee segment.second segment.time)

    let anyObject (o : HitObject) =
        if o.``type`` &&& HitObject.Type.Slider = HitObject.Type.Slider then sliderLine o
        else circle o