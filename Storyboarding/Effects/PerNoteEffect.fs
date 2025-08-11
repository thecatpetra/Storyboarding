namespace Storyboarding.Effects

open System.Numerics
open MapsetParser.objects
open Storyboarding.Tools
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.SbMonad

module PerNoteEffect =
    type LightParam = {
        startSize: float32
        endSize: float32
        lightColor: Color
        alpha: bool
    }

    type PingParams = {
        startSize: float32
        endSize: float32
        color: Color
        alpha: bool
    }

    let lightParamsOfColor c _ : LightParam option = { startSize = 1.5f; endSize = 1.2f; lightColor = c; alpha = true } |> Some
    let pingParamsOfColor c _ : PingParams option = { startSize = 0.1f; endSize = 1f; color = c; alpha = true } |> Some

    let rec circleEffect (parameters : HitObject -> LightParam option) timeStart timeEnd  =
        let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64)
        forEachHitObject (fun ho ->
        match parameters ho with
        | Some eff when ho.time >= timeStart && ho.time < timeEnd ->
            img Resources.light
            >>= layer Layer.Foreground
            >>= move (int ho.time) (int ho.time) (p ho.Position) (p ho.Position)
            >>= scale (int ho.time) (int ho.time + 2000) eff.startSize eff.endSize
            >>= fade (int ho.time) (int ho.time + 2000) 0.3f 0f
            >>= (eff.alpha >?= alpha)
            >>= color (int ho.time) (int ho.time) eff.lightColor eff.lightColor
        | _ -> id)

    let rec pingEffect (parameters : HitObject -> PingParams option) timeStart timeEnd  =
        let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64)
        forEachHitObject (fun ho ->
        match parameters ho with
        | Some eff when ho.time >= timeStart && ho.time < timeEnd ->
            img Resources.ping
            >>= layer Layer.Foreground
            >>= move (int ho.time) (int ho.time) (p ho.Position) (p ho.Position)
            >>= scale (int ho.time) (int ho.time + 300) eff.startSize eff.endSize
            >>= easing Easing.In
            >>= fade (int ho.time) (int ho.time + 300) 0.6f 0f
            >>= (eff.alpha >?= alpha)
            >>= color (int ho.time) (int ho.time) eff.color eff.color
        | _ -> id)