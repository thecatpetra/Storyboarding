namespace Storyboarding.Gameplay

open MapsetParser.objects
open Storyboarding.Gameplay.Util
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module Common =
    type CircleRepr = {
        hitCircle: string
        approachCircle: string
        scale: float32
    }

    let circle (hitCircle : HitObject) (circleRepr : CircleRepr) =
        let diffSettings = hitCircle.beatmap.difficultySettings
        let textureScale = 0.027f * circleRepr.scale
        let adjustedForCs = 54.4f - 4.48f * diffSettings.circleSize |> (*) textureScale
        let position = hitCircle.Position.X |> int |> (+) 64, hitCircle.Position.Y |> int |> (+) 54
        let circle = circleRepr.hitCircle
        let approach = circleRepr.approachCircle
        let arData = arToTime diffSettings.approachRate
        let time = hitCircle.time
        let out = 200
        let approachRateMod = 4f
        let approachOut = 1.1f
        img circle >> layer Overlay
        >>= move time time position position
        >>= fade (time - arData.preempt) (time - arData.fade_in) 0f 1f
        >>= scale time (time + out) adjustedForCs (1.5f * adjustedForCs)
        >>= fade time (time + out) 1f 0f
        >> img approach >> layer Overlay
        >>= move time time position position
        >>= fade (time - arData.preempt) time 0f 1f
        >>= fade time (time + out) 1f 0f
        >>= scale (time - arData.preempt) time (approachRateMod * adjustedForCs) adjustedForCs
        >>= scale time (time + out) adjustedForCs (adjustedForCs * approachOut)