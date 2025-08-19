namespace Storyboarding.Effects

open System
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

    let isNewCombo (ho : HitObject) =
        ho.``type`` &&& HitObject.Type.NewCombo = HitObject.Type.NewCombo

    let onlyFirstCombo f (ho : HitObject) =
        if isNewCombo ho then f ho else None

    let withComboColor (ho : HitObject) (f : Color -> 'a) : 'a =
        // very unoptimal (i dont care though)
        let comboSkip (ho : HitObject) =
            ((ho.``type`` |> int) &&& 112 >>> 4) + 1
        let colors = ho.beatmap.colourSettings.combos
        let maxIndex = colors |> Seq.length
        let selfIndex = ho.GetHitObjectIndex()
        let index = ho.beatmap.hitObjects
                    |> Seq.take (selfIndex + 1)
                    |> Seq.filter isNewCombo
                    |> Seq.fold (fun c h -> (c + comboSkip h) % maxIndex) 0
        Seq.item index colors |> (fun v -> (v.X |> int, v.Y |> int, v.Z |> int)) |> f

    let lightParamOfCombo (ho : HitObject) : LightParam option =
        withComboColor ho (fun c -> Some { startSize = 0.9f; endSize = 0.3f; lightColor = c; alpha = true })

    let lightPingOfCombo (ho : HitObject) : PingParams option =
        withComboColor ho (fun c -> Some { startSize = 0.1f; endSize = 1f; color = c; alpha = true })

    let lightParamsOfColors cs (ho : HitObject) : LightParam option =
        assert (cs <> [])
        let index = ho.GetHitObjectIndex()
        let color = cs |> List.item (index % List.length cs)
        Some { startSize = 0.1f; endSize = 1f; lightColor = color; alpha = true }

    let lightParamsOfColor c _ : LightParam option = { startSize = 1.5f; endSize = 1.2f; lightColor = c; alpha = true } |> Some
    let pingParamsOfColor c _ : PingParams option = { startSize = 0.1f; endSize = 1f; color = c; alpha = true } |> Some

    let circleEffect (parameters : HitObject -> LightParam option) timeStart timeEnd  =
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

    let pingEffect (parameters : HitObject -> PingParams option) timeStart timeEnd  =
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

    let rayEffect (parameters : HitObject -> LightParam option) timeStart timeEnd =
        let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64)
        forEachHitObject (fun ho ->
        let rotation = (float32 (ho.Position.X - 256f)) / 800f
        match parameters ho with
        | Some eff when ho.time >= timeStart && ho.time < timeEnd ->
            img Resources.grad_ray
            >>= layer Layer.Foreground
            >>= move (int ho.time) (int ho.time) (p ho.Position) (p ho.Position)
            >>= vectorScale (int ho.time) (int ho.time) (3f, 20f) (3f, 20f)
            >>= fade (int ho.time) (int ho.time + 2000) 0.3f 0f
            >>= rotate (int ho.time) (int ho.time) rotation rotation
            >>= alpha
            >>= color (int ho.time) (int ho.time) eff.lightColor eff.lightColor
        | _ -> id)

    let doubleRayEffect (parameters : HitObject -> LightParam option) timeStart timeEnd =
        let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64)
        forEachHitObject (fun ho ->
        match parameters ho with
        | Some eff when ho.time >= timeStart && ho.time < timeEnd ->
            let r = -MathF.PI / 2f + 0.05f
            let offset = r * 80f * MathF.Cos(r) |> int, r * 80f * MathF.Sin(r) |> int
            monadicMap [-1; 1] (fun d ->
            img Resources.glow_ray_rotated >>= layer Layer.Foreground
            >>= move (int ho.time) (int ho.time + 450) (p ho.Position) (p ho.Position |> (+++) (d *** offset)) >> easing Easing.In
            >>= vectorScale (int ho.time) (int ho.time) (0.2f, 1.7f) (0.2f, 1.7f)
            >>= fade (int ho.time) (int ho.time + 450) 0.4f 0f
            >>= rotate (int ho.time) (int ho.time) r r
            >>= alpha
            >>= color (int ho.time) (int ho.time) eff.lightColor eff.lightColor)
        | _ -> id)