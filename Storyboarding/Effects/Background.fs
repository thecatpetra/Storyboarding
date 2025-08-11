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
        let fullPath = Path.Join(Resources.resourcesFolder, filename)
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

    let bgMovement (timeStart : Time) (timeEnd : Time) sb =
        let beat = beatTime timeStart sb
        let iterationTime = beat * 64
        let iterations = (timeEnd - timeStart) / iterationTime |> int
        let stride = 20f
        let angleDiff = 1.5f
        let mutable prevMovePosition = (320, 240)
        let currentSprite = getCurrentSprite sb
        let size = getOptimalSize currentSprite |> (*) 1.15f
        (scale (timeStart + 1) (timeStart + 1) size size
        >>= monadicMap [0..iterations] (fun i ->
        let fi = float32 i
        let stride = (stride * MathF.Sin(fi * angleDiff) |> int, stride * MathF.Cos(fi * angleDiff) |> int)
        let nextMovePosition = (320, 240) +++ stride
        let iterStart = i * iterationTime + timeStart
        let iterEnd = (i + 1) * iterationTime + timeStart
        let r = move iterStart iterEnd prevMovePosition nextMovePosition  >>= easing Easing.SineInOut
        prevMovePosition <- nextMovePosition
        r)) sb

