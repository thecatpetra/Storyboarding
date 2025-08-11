namespace Storyboarding.Gameplay

open System.Numerics
open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module Util =
    type ArTimings = {
        preempt : int
        fade_in : int
    }

    type SliderSegment = {
        first: Position
        second: Position
        time: Time
    }

    type fSliderSegment = {
        ffirst: fPosition
        fsecond: fPosition
        time: Time
    }

    let v22p (v : Vector2) : Position = v.X |> int, v.Y |> int
    let v22fp (v : Vector2) : fPosition = v.X, v.Y

    let arToTime (ar : float32) : ArTimings =
        let preempt = if ar < 5f then 1200f + 600f * (5f - ar) / 5f
                      else 1200f - 750f * (ar - 5f) / 5f
        let fade_in = if ar < 5f then 800f + 400f * (5f - ar) / 5f
                      else 800f - 500f * (ar - 5f) / 5f
        { preempt = preempt |> int; fade_in = fade_in |> int}

    // Always includes end
    let range s step e =
        let rec helper acc = seq { if acc >= e then yield e else yield acc; yield! helper (acc + step) }
        helper s

    let sequenceToPairs sq =
        Seq.zip sq <| Seq.tail sq

    let getSliderSegments (slider : HitObject) (step : int): SliderSegment seq =
        assert (slider.GetType() = typeof<Slider>)
        let start = slider.time
        let repeats = slider.GetEdgeTimes() |> Seq.length |> fun x -> x - 1
        let slider = slider :?> Slider
        let duration = (slider.endTime |> int) - start
        let endT = start + duration / repeats
        let genSegment (st, en) =
            let firstPosition = slider.GetPathPosition(st |> float)
            let secondPosition = slider.GetPathPosition(en |> float)
            {first = v22p firstPosition; second = v22p secondPosition; time = st |> int}
        Seq.map genSegment (range start step endT |> sequenceToPairs)

    let getSliderSegmentsF (slider : HitObject) (step : int): fSliderSegment seq =
        assert (slider.GetType() = typeof<Slider>)
        let start = slider.time
        let repeats = slider.GetEdgeTimes() |> Seq.length |> fun x -> x - 1
        let slider = slider :?> Slider
        let duration = (slider.endTime |> int) - start
        let endT = start + duration / repeats
        let genSegment (st, en) =
            let firstPosition = slider.GetPathPosition(st |> float)
            let secondPosition = slider.GetPathPosition(en |> float)
            {ffirst = v22fp firstPosition; fsecond = v22fp secondPosition; time = st |> int}
        Seq.map genSegment (range start step endT |> sequenceToPairs)
