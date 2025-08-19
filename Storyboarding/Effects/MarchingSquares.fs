namespace Storyboarding.Effects

open System.Numerics
open MapsetParser.objects
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module MarchingSquares =
    let rec safeZip a b =
        match (a, b) with
        | ah :: at, bh :: bt -> (ah, bh) :: safeZip at bt
        | _ -> []

    let effect timeStart timeEnd mx my disturbances sb =
        printfn "Calculating squares"
        let interval = beatTime (timeStart + 5000) sb |> (*) 1
        let initial = Array.init (mx * my) (fun _ -> 0f)
        let positions = Array.init (mx * my) (fun e -> (e % mx, e / mx))
        let inBounds (x, y) = 0 <= x && x < mx && 0 <= y && y < my
        let maxInArea arr (position : Position) =
            let steps = [(1, 0); (0, 1); (-1, 0); (0, -1); (0, 0)]
                        |> List.map ((+++) position)
                        |> List.filter inBounds
            steps |> List.map (fun (px, py) -> Array.item (px + py * mx) arr) |> List.max
        let step disturbances previous currentTime =
            let next = Array.map (fun e -> (maxInArea previous e) - 0.0999f |> max 0f) positions
            match disturbances with
            | (time, (px, py)) :: ds when time < currentTime -> Array.set next (px + py * mx) 1f |> fun _ -> ds, next
            | _ -> disturbances, next
        let timeStepCount = timeEnd - timeStart |> fun x -> (/) x interval
        let timeSteps = [0..timeStepCount] |> List.map ((*) interval) |> List.map ((+) timeStart)
        // TODO: Change to mapFold
        let mutable disturbances = disturbances |> List.filter (fun (_, x) -> inBounds x)
        let mutable previous = initial
        let behaviour : float32 array list =
            Seq.map (fun ts -> let d, n = step disturbances previous ts
                               previous <- n
                               disturbances <- d
                               n) timeSteps
            |> Seq.toList
        monadicMap [0..mx-1] (fun x ->
        monadicMap [0..my-1] (fun y ->
        img light_dot >> layer Foreground
        >>= move timeStart timeEnd (x * 60 - 100 + 3 * y, y * 60 + 35 - 3 * x) (x * 60 - 100 + 3 * y, y * 60 + 35 - 3 * x)
        >>= scale timeStart timeStart 0.1f 0.1f
        >>= color timeStart timeStart (0, 0, 0) (0, 0, 0)
        >>= alpha
        >>= monadicMapi (safeZip behaviour (List.tail behaviour)) (fun i (bp, bn) ->
        let ts = i * interval + timeStart
        let te = (i + 1) * interval + timeStart
        let index = x + y * mx
        let colorOfFloat f = (f * 127f |> int, f * 200f |> int, f * 255f |> int)
        (bn.[index] <> 0f >?= color ts te (colorOfFloat bp.[index]) (colorOfFloat bn.[index]))))) sb

    let withDisturbances timeStart timeEnd =
        let toIndices (v: Vector2) = (v.X + 200f) / 60f |> int, (v.Y + 35f) / 60f |> int
        let isNewCombo (ho : HitObject) = ho.``type`` &&& HitObject.Type.NewCombo = HitObject.Type.NewCombo
        forEachDiff (fun diff ->
        let objects = diff.hitObjects |> Seq.filter isNewCombo |> Seq.filter (fun x -> x.time >= timeStart)
        let disturbances = objects |> Seq.map (fun ho -> ho.time, toIndices ho.Position) |> Seq.toList
        effect timeStart timeEnd 15 8 disturbances)