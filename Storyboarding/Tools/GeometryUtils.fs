namespace Storyboarding.Tools

open System
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.SbMonad

module GeometryUtils =
    type LineSegmentParams = {
        image: string
        widthModifier: float32
        height: float32
        color: Color option
    }

    type LineSegmentRepr = {
        parameters: LineSegmentParams
        points: Position * Position
        position: Position
        scale: float32 * float32
        rotation: float32
    }

    type TriangleParams = {
        image: string
        sizeModifier: float32
        color: Color option
    }

    type TriangleRepr = {
        parameters: TriangleParams
        points: Position * Position * Position
    }

    let defaultLS =
        { image = Resources.square_white
          widthModifier = 0.00785f
          height = 0.01f
          color = None }

    let defaultTriangle =
        { image = Resources.triangle_white
          sizeModifier = 0.00786f
          color = None }

    let getLsParams pointA pointB (lsParams: LineSegmentParams) =
        let middlePoint = lerp pointA pointB 0.5f
        let rotation = Math.Atan2((pointA |> snd) - (pointB |> snd) |> float, ((pointA |> fst) - (pointB |> fst)) |> float)
        let length = length (pointA --- pointB) |> float32
        let calculatedScale = (lsParams.widthModifier |> (*) length, lsParams.height)
        { parameters = lsParams
          points = pointA, pointB
          position = middlePoint
          rotation = rotation |> float32
          scale = calculatedScale }

    let drawStaticLs timeStart timeEnd (ls : LineSegmentRepr) =
        img ls.parameters.image
        >>= vectorScale timeStart timeEnd ls.scale ls.scale
        >>= move timeStart timeEnd ls.position ls.position
        >>= rotate timeStart timeEnd ls.rotation ls.rotation

    let drawMovingLs timeStart timeEnd initial final =
        vectorScale timeStart timeEnd initial.scale final.scale
        >>= move timeStart timeEnd initial.position final.position
        >>= rotate timeStart timeEnd initial.rotation final.rotation

    let drawSequence drawFn (sequence : (Time * 'a) seq) =
        let mutable prev = sequence |> Seq.head
        sequence |> Seq.map (fun next ->
        let timeStart, first = prev
        let timeEnd, second = next
        drawFn timeStart timeEnd first second
        prev <- next)

    let isLeft a b c =
        ((b |> fst) - (a |> fst)) * ((c |> snd) - (a |> snd)) - ((b |> snd) - (a |> snd)) * ((c |> fst) - (a|> fst)) > 0

    let createTriangleInner timeStart timeEnd pointA pointB pointC (triangleParams : TriangleParams) =
        let pointD = lerp pointA pointB 0.5f
        let pointE = lerp pointA pointC 0.5f
        let BC = pointB --- pointC
        let lAB = pointA --- pointB |> length |> float32
        let lAC = pointA --- pointC |> length |> float32
        let lBC = BC |> length |> float32
        let p = (lAB + lAC + lBC) |> (*) 0.5f
        let S = MathF.Sqrt(p * (p - lAB) * (p - lAC) * (p - lBC))
        let rot = MathF.Atan2(BC |> snd |> float32, BC |> fst |> float32) + MathF.PI * 1.5f
        let h = S / lBC * 2f
        let lCF = MathF.Sqrt(lAC * lAC - h * h)
        let lBF = lBC - lCF
        let firstSize = triangleParams.sizeModifier * h, triangleParams.sizeModifier  * lBF
        let secondSize = triangleParams.sizeModifier * h, triangleParams.sizeModifier * lCF
        let shouldFlip = isLeft pointB pointC pointA
        img triangleParams.image
        >>= move timeStart timeEnd pointD pointD
        >>= rotate timeStart timeEnd rot rot
        >>= vectorScale timeStart timeEnd firstSize firstSize
        >>= verticalFlip
        >>= (shouldFlip >?= horizontalFlip)
        >>= (triangleParams.color.IsSome
        >?= color timeStart timeEnd triangleParams.color.Value triangleParams.color.Value)
        >>= img triangleParams.image
        >>= move timeStart timeEnd pointE pointE
        >>= rotate timeStart timeEnd rot rot
        >>= vectorScale timeStart timeEnd secondSize secondSize
        >>= (shouldFlip >?= horizontalFlip)
        >>= (triangleParams.color.IsSome
        >?= color timeStart timeEnd triangleParams.color.Value triangleParams.color.Value)

    let rec createTriangle timeStart timeEnd pointA pointB pointC =
        let lAB = pointA --- pointB |> length |> float32, pointC
        let lAC = pointA --- pointC |> length |> float32, pointB
        let lBC = pointB --- pointC |> length |> float32, pointA
        let lst = [lAB; lAC; lBC]
        let chosen = List.maxBy fst lst |> snd
        let other = lst |> List.filter (fun x -> (snd x) <> chosen) |> List.map snd |> List.sortBy fst
        createTriangleInner timeStart timeEnd chosen (List.item 0 other) (List.item 1 other)

    let debugPoint time position =
        let red = (255, 0, 0)
        img Resources.square_white
        >>= color time (time + 1000) red red
        >>= scale time time 0.01f 0.01f
        >>= move time time position position

    type FsPoint = { X: int; Y: int }

    let checkIntersection (aX, aY) (bX, bY) (cX, cY) (dX, dY) =
        let shrink a b =
            (lerp a b 0.01f), (lerp a b 0.99f)

        let (aX, aY), (bX, bY) = shrink (aX, aY) (bX, bY)
        let (cX, cY), (dX, dY) = shrink (cX, cY) (dX, dY)

        let crossProduct (p1: FsPoint) (p2: FsPoint) (p3: FsPoint) =
            (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X)

        let onSegment (p1: FsPoint) (p2: FsPoint) (p: FsPoint) =
            p.X <= max p1.X p2.X && p.X >= min p1.X p2.X &&
            p.Y <= max p1.Y p2.Y && p.Y >= min p1.Y p2.Y

        let doSegmentsIntersect (p1: FsPoint) (p2: FsPoint) (p3: FsPoint) (p4: FsPoint) =
            let d1 = crossProduct p3 p4 p1
            let d2 = crossProduct p3 p4 p2
            let d3 = crossProduct p1 p2 p3
            let d4 = crossProduct p1 p2 p4
            if ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)) then true
            else if d1 = 0 && onSegment p3 p4 p1 then true
            else if d2 = 0 && onSegment p3 p4 p2 then true
            else if d3 = 0 && onSegment p1 p2 p3 then true
            else d4 = 0 && onSegment p1 p2 p4

        let a = { X = aX; Y = aY }
        let b = { X = bX; Y = bY }
        let c = { X = cX; Y = cY }
        let d = { X = dX; Y = dY }

        doSegmentsIntersect a b c d

    let openingLine ts fin diff f t width =
        let te = ts + diff
        let dist = length (f --- t) |> float32 |> (*) 0.0325f
        let angle = - (angle (f --- t))
        img (square_white |> resize1To 32) >> coords f >> origin TopCentre
        >>= rotate ts fin angle angle
        // >>= fade ts fin 1f 1f
        >>= vectorScale ts te (width, 0f) (width, dist) >> easing Easing.SineInOut
