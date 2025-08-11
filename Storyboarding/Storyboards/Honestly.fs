namespace Storyboarding.Storyboards

open System.Numerics
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes

module Honestly =
    let writeNames =
        forEachDiff (fun d ->
        let name = d.metadataSettings.version
        TextUtils.text font_monosans name (TextUtils.chromoInOut 1000 100 20000) (100, 100) 0.2f)

    let genRaysForHs =
        let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64)
        forEachHitObject (fun ho ->
        let rotation = (float32 (ho.Position.X - 256f)) / 800f
        img grad_ray
        >>= move (int ho.time) (int ho.time) (p ho.Position) (p ho.Position)
        >>= vectorScale (int ho.time) (int ho.time) (3f, 20f) (3f, 20f)
        >>= fade (int ho.time) (int ho.time + 2000) 0.3f 0f
        >>= rotate (int ho.time) (int ho.time) rotation rotation
        >>= alpha
        >>= color (int ho.time) (int ho.time) (randColor()) (sameColor()))

    let story =
        writeNames
        >>= genRaysForHs

    let make () =
        openSb @"C:\Users\arthur\AppData\Local\osu!\Songs\2316191 Jesus Mus1c - Chess Type Beat\Jesus Mus1c - Chess Type Beat (Izalik).osb"
        |> story
        |> SbCompiler.write

