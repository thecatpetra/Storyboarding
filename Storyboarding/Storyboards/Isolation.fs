namespace Storyboarding.Storyboards

open System.Numerics
open GpuDrawingUtils.SliderRenderer
open MapsetParser.objects
open MapsetParser.objects.hitobjects
open Storyboarding.Effects.Background
open Storyboarding.Gameplay.NormalGameplay
open Storyboarding.Gameplay.Util
open Storyboarding.Tools
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.SbMonad

module Isolation =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\beatmap-638907849626988884-audio\NH22 - Isolation (Official LIMBO Remix) (TheCatPetra).osb"

    let s d =
        background bg_isolation 0 (t "03:28:839") >> layer Overlay
        >>= createSlidersFor d
        >>= createCirclesFor d

    let story =
        forEachDiff (fun d -> if d.metadataSettings.version = "Compiled" then s d else id)

    let make () =
        openSb path
        |> story
        |> SbCompiler.write
