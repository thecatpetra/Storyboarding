namespace Storyboarding.Effects

open System.Collections.Generic
open System.IO
open Storyboarding.Tools.GeometryUtils
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes
open Storyboarding.Effects.LSystem.Interpreter
open Storyboarding.Effects.LSystem

module Growth =
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
    let growingVineBreadth ts te (lines : ResizeArray<Segment>) point clr =
        let front = Queue<Position * Time>([point, ts])
        let mutable storyboard = id
        let diff = 500
        while (front.Count > 0) do
            let point, ts = front.Dequeue()
            printfn $"Growing line {point}"
            let possible = List.choose (fun b ->
                let newPoint = point +++ (randInt -70 70, randInt -70 70)
                let inBounds (x, y) = -120 < x && x < 750 && -10 < y && y < 490
                let canGo = ts < (te - diff) && inBounds point && Seq.forall (fun (a, b) -> not <| checkIntersection point newPoint a b) lines
                if not canGo then None
                else let () = (point, newPoint) |> lines.Add in Some newPoint) [1..3]
            possible |> List.iter (fun n -> front.Enqueue(n, ts + diff))
            storyboard <- storyboard >>= img gp_filled_circle >> coords point
                          >>= scale ts te 0.006f 0.006f
                          >>= color ts ts clr clr
                          >>= fade ts te 0.5f 0.0f
            storyboard <- storyboard >>= monadicMap possible (fun newPoint ->
                openingLine ts te diff point newPoint 0.02f
                >>= fade ts te 0.5f 0.0f
                >>= color ts te clr clr
            )
        storyboard

    let fractalLSystem ip ia ts te program =
        let lSystem = File.ReadAllText(program)
        let r = Parser.parse lSystem
        match r with
        | Parser.Parsed (p, []) -> drawExpression ip ia ts te p
        | Parser.Failed (m1, m2) -> failwith $"{m1} | {m2}" 
        | Parser.Parsed (p, t) -> failwith $"Not parsed: {t}"