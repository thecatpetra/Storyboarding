namespace Storyboarding.Gameplay

open System.Numerics
open GpuDrawingUtils.SliderRenderer
open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Gameplay.Common
open Storyboarding.Gameplay.Util
open Storyboarding.Tools
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module NormalGameplay =
    // TODO: Move to gameplay
    let initSliderBodies (d : Beatmap) (sliders: Slider seq)=
        renderSliders d.metadataSettings.beatmapId sliders

    let sliderBall (s: Slider) =
        let p2p (v: Vector2) = (v.X |> int |> (+) 64, v.Y |> int |> (+) 55)
        let skipped = s.pathPxPositions
                      |> Seq.mapi (fun i x -> if i % 20 = 19 then Some(x) else None)
                      |> Seq.choose id
                      // |> Seq.append ([s.pathPxPositions[s.pathPxPositions.Count - 1]])
                      |> Seq.toList
        let mutable prev = p2p s.Position
        img gp_simple_circle >> layer Overlay
        >>= scale s.time s.time 0.27f 0.27f
        >>= monadicMapi skipped (fun i point ->
            let before = (float32 i) / (float32 skipped.Length)
            let after = (float32 (i + 1)) / (float32 skipped.Length)
            let endTime = s.endTime |> int
            let position = p2p point
            let x = move (lerpTime s.time endTime before) (lerpTime s.time endTime after) prev position
            prev <- p2p point
            x
        )

    let createSlidersFor (d : Beatmap) =
        let sliders = d.hitObjects
                      |> Seq.filter (fun x -> x.``type`` &&& HitObject.Type.Slider = HitObject.Type.Slider)
                      |> Seq.map (fun x -> x :?> Slider)
                      |> Seq.rev
        let textures = initSliderBodies d sliders
        monadicMap (Seq.toList sliders) (fun s ->
        let initial = (-2, -10)
        let arData = arToTime d.difficultySettings.approachRate
        let croppedAt, tex = textures[s.time] |> ImageFilters.cropImage
        let croppedAt = (croppedAt |> fst, croppedAt |> snd )
        let x, y = (initial +++ (0.314444f **** croppedAt))
        img tex >> origin TopLeft >> layer Overlay
        >>= fade (s.time - arData.preempt) (s.time - arData.fade_in) 0f 1f
        >>= fade (int s.endTime) (int s.endTime + 500) 1f 0f
        >>= move s.time s.time (x, y) (x, y)
        >>= scale s.time (int s.endTime) 0.314444f 0.314444f
        >>= sliderBall s)

    let createCirclesFor (d : Beatmap) =
        let circles = d.hitObjects |> Seq.rev
        monadicMap (Seq.toList circles) (fun s ->
            circle s { hitCircle = "gameplay/circle_cropped.png"; approachCircle = "gameplay/approach_circle.png"; scale = 0.314444f }
        )

