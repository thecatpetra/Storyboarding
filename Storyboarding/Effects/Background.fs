namespace Storyboarding.Effects

open System
open System.Collections
open Delaunator.Interfaces
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.GeometryUtils
open System.IO
open SixLabors.ImageSharp
open Storyboarding.Tools.SbTypes

module Background =
    let getOptimalSize filename =
        let fullPath = Path.Join(Paths.resourcesFolder, filename)
        use image = Image.Load(fullPath)
        880f / (float32 image.Width)

    let background filename timeStart timeEnd =
        let optimalSize = getOptimalSize filename
        img filename
        >>= scale timeStart timeStart 0f optimalSize
        >>= fade timeStart timeStart 0f 1f
        >>= fade timeEnd timeEnd 1f 0f

    let createTriangulation (points : Position list) : (Position * Position * Position) list =
        let translatedPoints = points |> List.map (fun e -> Delaunator.Models.Point(e |> fst |> float, e |> snd |> float))
        let pointArray = translatedPoints |> Seq.toArray
        let delaunator = Delaunator.Delaunator(pointArray)
        let triangulation = delaunator.GetTriangles() |> Seq.toList
        let pointToPosition (p: IPoint) = p.X |> int, p.Y |> int
        triangulation
        |> Seq.toList
        |> List.map _.Points
        |> List.map Seq.toList
        |> List.map (fun [a; b; c] -> (pointToPosition a, pointToPosition b, pointToPosition c))

    let bgLayerMovement (timeStart : Time) (timeEnd : Time) amplitude sb =
        let beat = beatTime timeStart sb
        let iterationTime = beat * 32
        let rotateDiff = 0.03f
        let iteration i (ts, te) =
            let position = float32 >> (*) 3.2f >> (+) 1.7f >> (fun i -> MathF.Cos(i) * amplitude |> int, MathF.Sin(i) * amplitude |> int) >> (+++) (320, 240)
            let rotation i = i % 2 |> float32 |> (-) 0.5f |> (*) rotateDiff
            rotate ts te (rotation i) (rotation <| i + 1)
            >> easing Easing.QuadInOut
            >>= move ts te (position i) (position <| i + 1)
            >> easing Easing.QuadInOut
        timeDivisionMapi timeStart timeEnd iterationTime iteration sb

    let bgMovement (timeStart : Time) (timeEnd : Time) =
        bgLayerMovement timeStart timeEnd 8f

    let parallax (ts : Time) (te : Time) sprites =
        monadicMap sprites (fun (sprite, amplitude) -> background sprite ts te >> bgLayerMovement ts te amplitude)