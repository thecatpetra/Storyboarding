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
        printfn $"Making {filename} background"
        let optimalSize = getOptimalSize filename
        img filename
        >>= scale timeStart timeStart 0f optimalSize
        >>= fade timeStart timeStart 0f 1f
        >>= fade timeEnd timeEnd 1f 0f

    let backgroundRaw filename timeStart timeEnd =
        printfn $"Making {filename} background"
        let optimalSize = getOptimalSize filename
        img filename
        >>= scale timeStart timeStart 0f optimalSize

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

    let bgLayerMovement (timeStart : Time) (timeEnd : Time) amplitude beatIteration sb =
        printfn $"Calculating layer movement ({timeStart}/{timeEnd}/{amplitude})"
        let beat = beatTime 60000 sb
        let iterationTime = beat * beatIteration
        let rotateDiff = 0.03f
        let iteration =
            let ts, te = 0, iterationTime * 2
            let position = float32 >> (*) MathF.PI >> (+) 1.7f >> (fun i -> MathF.Cos(i) * amplitude |> int, MathF.Sin(i) * amplitude |> int) >> (+++) (320, 240)
            let rotation i = i % 2 |> float32 |> (-) 0.5f |> (*) rotateDiff
            rotate ts iterationTime (rotation 0) (rotation 1) >> easing Easing.QuadInOut
            >>= move ts iterationTime (position 0) (position 1) >> easing Easing.QuadInOut
            >>= rotate iterationTime te (rotation 1) (rotation 0) >> easing Easing.QuadInOut
            >>= move iterationTime te (position 1) (position 0) >> easing Easing.QuadInOut
        loopTimeEnd timeStart timeEnd iteration sb

    let bgMovement (timeStart : Time) (timeEnd : Time) =
        printfn $"Making bg movement ({timeStart}/{timeEnd})"
        bgLayerMovement timeStart timeEnd 8f 32

    let bgMovementSlow (timeStart : Time) (timeEnd : Time) =
        printfn $"Making bg movement ({timeStart}/{timeEnd})"
        bgLayerMovement timeStart timeEnd 8f 64

    let parallax (ts : Time) (te : Time) sprites =
        printfn $"Making parallax effect ({ts}/{te}/%A{sprites})"
        monadicMap sprites (fun (sprite, amplitude) -> background sprite ts te >> bgLayerMovement ts te amplitude 32)
    
    let bgMovementRotate (ts : Time) (te : Time) sb=
        let beat = beatTime 60000 sb
        let iterationTime = beat * 64
        let rotateDiff = 0.03f
        let iteration =
            rotate 0 iterationTime (- rotateDiff) rotateDiff >> easing Easing.QuadInOut
            >>= rotate iterationTime (2 * iterationTime) (rotateDiff) -rotateDiff >> easing Easing.QuadInOut
        loopTimeEnd ts te iteration sb