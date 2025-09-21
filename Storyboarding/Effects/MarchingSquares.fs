namespace Storyboarding.Effects

open System.Numerics
open MapsetParser.objects
open Storyboarding.Tools
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module MarchingSquares =
    let rec safeZip a b =
        match (a, b) with
        | ah :: at, bh :: bt -> (ah, bh) :: safeZip at bt
        | _ -> []

    let maxInArea mx my inBounds arr (position : Position) =
            let steps = [(1, 0); (0, 1); (-1, 0); (0, -1); (0, 0)]
                        |> List.map ((+++) position)
                        |> List.filter inBounds
            steps |> List.map (fun (px, py) -> Array.item (px + py * mx) arr) |> List.max
    let waveStep mx my inBounds (positions : Position array) disturbances previous currentTime =
        let next = Array.map (fun e -> (maxInArea mx my inBounds previous e) - 0.0999f |> max 0f) positions
        match disturbances with
        | (time, (px, py)) :: ds when time < currentTime -> Array.set next (px + py * mx) 1f |> fun _ -> ds, next
        | _ -> disturbances, next

    let sumInArea mx my inbounds arr position =
        let x, y = position
        let lookup (x, y) = Array.item ((x % mx) + (y % my) * mx) arr > 0.99f
        let around = [mx - 1; 0; 1]
                     |> List.collect (fun ax -> [my - 1; 0; 1] |> List.map (fun ay -> ax + x, ay + y))
                     |> List.filter (fun (a, b) -> a <> x || b <> y)
        List.map lookup around |> List.map (fun b -> if b then 1 else 0) |> List.sum
        
    let shouldBeAlive mx my inbounds arr position =
        let lookup (x, y) = Array.item ((x % mx) + (y % my) * mx) arr
        let sum = sumInArea mx my inbounds arr position
        let self = lookup position
        let life = if self > 0.99f then sum >= 2 && sum <= 3 else sum = 3
        if life then 1f else max (self * 0.86f - 0.001f) 0f
        
    let lifeStep mx my inBounds (positions : Position array) disturbances previous currentTime =
        let next = Array.map (shouldBeAlive mx my inBounds previous) positions
        match disturbances with
        | (time, (px, py)) :: ds when time < currentTime ->
            // positions |> Array.iter (fun (x, y) -> Array.set next (((px + x) % mx) + ((py + y) % my) * mx) 1f)
            // [mx - 2; mx - 1; 0; 1; 2] |> Seq.iter (fun x ->
            // [my - 2; my - 1; 0; 1; 2] |> Seq.iter (fun y ->
            // let res = SbRandom.choice [0f; 0f; 1f]
            // Array.set next (((px + x) % mx) + ((py + y) % my) * mx) res
            // ))
            ds, next
        | _ -> disturbances, next

    let effect timeStart timeEnd mx my disturbances step colors  l sb =
        printfn "Calculating squares"
        let interval = beatTime (timeStart + 5000) sb |> (*) 1
        let initial = Array.init (mx * my) (fun _ -> SbRandom.choice [0f; 0f; 1f])
        let positions = Array.init (mx * my) (fun e -> (e % mx, e / mx))
        let inBounds (x, y) = 0 <= x && x < mx && 0 <= y && y < my
        let timeStepCount = timeEnd - timeStart |> fun x -> (/) x interval
        let timeSteps = [0..timeStepCount] |> List.map ((*) interval) |> List.map ((+) timeStart)
        // TODO: Change to mapFold
        let mutable disturbances = disturbances |> List.filter (fun (_, x) -> inBounds x)
        let mutable previous = initial
        let behaviour : float32 array list =
            Seq.map (fun ts -> let d, n = step mx my inBounds positions disturbances previous ts
                               previous <- n
                               disturbances <- d
                               n) timeSteps
            |> Seq.toList
        let ca, cb, cc = colors
        monadicMapi behaviour (fun i b ->
        monadicMap [0..mx-1] (fun x ->
        monadicMap [0..my-1] (fun y ->
        let px, py = (mx / 2 - x) * (840 / mx) + 320, (my / 2 - y) * (840 / mx) + 240
        let index = x + y * mx
        let ts = i * interval + timeStart
        if b[index] = 1f then
            img (gp_filled_circle |> ImageFilters.resize1To 32) >> coords (px, py) >> layer l
            >>= scale ts ts 0.25f 0.25f
            >>= color ts (ts + interval) ca cb
            >>= color (ts + interval) (ts + 4 * interval) cb cc
            >>= fade (ts + interval) (ts + 4 * interval) 1f 0f
            >>= alpha
        else id
        ))) sb

    let withDisturbances step colors layer timeStart timeEnd=
        let mx = 45f
        let toIndices (v: Vector2) = (v.X + 200f) / (800f / mx) |> int, (v.Y + 35f) / (800f / mx) |> int
        let isNewCombo (ho : HitObject) = ho.``type`` &&& HitObject.Type.NewCombo = HitObject.Type.NewCombo
        forEachDiff (fun diff ->
        let objects = diff.hitObjects |> Seq.filter isNewCombo |> Seq.filter (fun x -> x.time >= timeStart)
        let disturbances = objects |> Seq.map (fun ho -> ho.time, toIndices ho.Position) |> Seq.toList
        effect timeStart timeEnd 35 19 disturbances step colors layer)
    
    let waves = withDisturbances waveStep ((255, 255, 255), (255, 255, 255), (0, 0, 0)) Layer.Background
    
    
    // ((194, 165, 79), (159, 101, 40), (0, 0, 0))
    let life = withDisturbances lifeStep ((170, 80, 60), (150, 50, 50), (0, 0, 0)) Layer.Foreground
    
    let dim a = lerpColor (0, 0, 0) a 0.3f
    let lifeBlue = withDisturbances lifeStep (dim (194, 190, 243), dim (139, 150, 200), (0, 0, 0)) Layer.Foreground
    
    