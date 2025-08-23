namespace Storyboarding.Storyboards

open System.Collections.Generic
open Storyboarding.Effects
open Storyboarding.Effects.Background
open Storyboarding.Tools
open Storyboarding.Tools.ColorUtils
open Storyboarding.Tools.GeometryUtils
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.TextUtils

module Continuum =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2423610 Kardashev - Continuum\Kardashev - Continuum (me2u).osb"

    let white = fun _ -> (240, 240, 255)
    let red x = indexedGradientT (255, 40, 80) (255, 80, 40) x |> toFun

    let openingLine ts fin diff f t width =
        let te = ts + diff
        let dist = length (f --- t) |> float32 |> (*) 0.032f
        let angle = - (angle (f --- t))
        img (square_white |> resize1To 32) >> coords f >> origin TopCentre
        >>= vectorScale ts te (width, 0f) (width, dist) >> easing Easing.SineInOut
        >>= rotate ts ts angle angle
        >>= fade ts fin 1f 1f

    let renderLyrics1 =
        let lyrics = [
            t "00:34:918", "Ignorance", Some(96, 115), t "00:47:463", 0.3f, white
            t "00:37:100", "Has collapsed", Some(96, 140), t "00:47:463", 0.3f, white
            t "00:39:281", "Resources are", Some(96, 190), t "00:47:463", 0.3f, white
            t "00:41:190", "Redistributed", Some (96, 215), t "00:47:463", 0.3f, white
            t "00:44:463", "To the development of", Some (96, 265), t "00:47:463", 0.3f, white
            t "00:46:099", "Machinery", Some (96, 290), t "00:47:463", 0.3f, red "Machinery"
            t "00:47:463", "Autonomous self-replication", None, t "00:50:463", 0.4f, red "Autonomous self-replication"
        ]
        let line (ts, txt, position, te, s, icf) =
            let effect = spinInOutMove (te - ts) 35 ts icf
            match position with
            | None -> textCenter font_quicksand txt effect s
            | Some p -> text font_quicksand txt effect p s
        monadicMap lyrics line
        >>= openingLine (t "00:34:918") (t "00:47:463") 800 (90, 105) (90, 150) 0.04f
        >>= openingLine (t "00:39:281") (t "00:47:463") 800 (90, 180) (90, 225) 0.04f
        >>= openingLine (t "00:44:463") (t "00:47:463") 800 (90, 255) (90, 300) 0.04f

    let renderLyrics2 =
        let lyrics = [
            t "00:52:917", "Propagation of stigma", white, 115, 0
            t "00:55:644", "And infinite traits", white, 140, -2
            t "00:59:463", "Evolutionary algorithm", red "Evolutionary algorithm", 165, 0
        ]
        let line (ts, l, icf, h, w) =
            let te = t "00:62:463"
            let effect = spinInOutMove (te - ts) 35 ts icf
            let w = textWidth font_quicksand l 0.3f |> (-) 550f |> int |> (+) w
            text font_quicksand l effect (w, h) 0.3f
        monadicMap lyrics line
        >>= openingLine (t "00:52:917") (t "00:62:917") 800 (552, 107) (552, 175) 0.04f

    type Segment = Position * Position

    let rec growingVineDepth ts te (lines : ResizeArray<Segment>) point =
        printfn $"Growing line {point}"
        let diff = 1000
        img gp_filled_circle >> coords point
        >>= scale ts te 0.01f 0.01f
        >>= match ts with
            | ts when ts < te -> monadicMap [1..9] (fun b ->
                let newPoint = point +++ (randInt -30 30, randInt -30 30)
                let inBounds (x, y) = -120 < x && x < 750 && -10 < y && y < 490
                let canGo =
                    inBounds point &&
                    Seq.forall (fun (a, b) -> not <| checkIntersection point newPoint a b) lines
                (point, newPoint) |> lines.Add
                if canGo then
                    openingLine ts te diff point newPoint 0.02f
                    >>= fade ts ts 0.5f 0.5f
                    >>= growingVineDepth (ts + diff) te lines newPoint
                else id)
            | _ -> id

    // TODO: More functional (get the recursive BFS)
    let growingVineBreadth ts te (lines : ResizeArray<Segment>) point =
        let front = Queue<Position * Time>([point, ts])
        let mutable storyboard = id
        let diff = 250
        while (front.Count > 0) do
            let point, ts = front.Dequeue()
            printfn $"Growing line {point}"
            let possible = List.choose (fun b ->
                let newPoint = point +++ (randInt -30 30, randInt -30 30)
                let inBounds (x, y) = -120 < x && x < 750 && -10 < y && y < 490
                let canGo = ts < (te - diff) && inBounds point && Seq.forall (fun (a, b) -> not <| checkIntersection point newPoint a b) lines
                if not canGo then None
                else let () = (point, newPoint) |> lines.Add in Some newPoint) [1..3]
            possible |> List.iter (fun n -> front.Enqueue(n, ts + diff))
            storyboard <- storyboard >>= img gp_filled_circle >> coords point
                          >>= scale ts te 0.006f 0.006f
                          >>= color ts ts (255, 180, 160) (255, 160, 200)
                          >>= fade ts te 0.5f 0.0f
            storyboard <- storyboard >>= monadicMap possible (fun newPoint ->
                openingLine ts te diff point newPoint 0.02f
                >>= fade ts te 0.5f 0.0f
                >>= color ts te (255, 180, 160) (255, 160, 200)
            )
        storyboard

    let renderLyrics3 ts =
        let te = ts + (t "01:14:735") + 3000 - (t "01:06:281")
        let tm = ts - t "01:06:281"
        let lyrics = [
            t "01:06:281" + tm, "This process will mirror", -3, white
            t "01:09:826" + tm, "Natural concepts", -1, white
            t "01:12:830" + tm, "While guided by", 1, white
            t "01:14:735" + tm, "Pre-written code", 3, red "Pre-written code"
        ]
        let line (ts, l, h, icf) =
            let effect = spinInOutMove (te - ts) 35 ts icf
            let w = textWidth font_quicksand l 0.3f |> (/) 2f |> int |> (+) 320
            text font_quicksand l effect (w, 240 + 10 * h) 0.3f
        monadicMap lyrics line

    let story =
        background bg_snow_mountain 0 (t "00:52:644")
        >>= bgMovement 0 (t "00:52:644")
        >>= renderLyrics1
        >>= renderLyrics2
        >>= renderLyrics3 (t "01:06:281")
        >>= growingVineBreadth (t "00:47:463") (t "00:51:008") (ResizeArray<Segment>()) (320, 240)
        >>= growingVineBreadth (t "00:59:190") (t "01:04:099") (ResizeArray<Segment>()) (456, 162)
        >>= RealisticSnow.effect 0 (t "01:18:826")

    let make () = openSb path |> story |> SbCompiler.write

