namespace StoryboardRenderer.Renderer

open System
open System.IO
open LadaEngine.Engine.Base
open LadaEngine.Engine.Common.SpriteGroup
open Storyboarding.Tools
open Storyboarding.Tools.SbTypes

type RdrSprite(sprite : Sprite) =  

    let flatten (i : Instruction list) =
        let unrollLoop (l : LoopInstruction) =
            let addition = l.instructions |> Seq.map (fun (SimpleInstruction i) -> i.timeEnd) |> Seq.max
            List.init l.iterations (fun i -> l.instructions |> List.map (fun (SimpleInstruction inst) ->
                { inst with timeEnd = (inst.timeEnd + i * addition); timeStart = (inst.timeStart + i * addition)}
            )) |> List.concat
        let inner = function
            | Loop l -> unrollLoop l
            | SimpleInstruction i -> [i]
        List.collect inner i

    let instructions = flatten sprite.instructions
    
    let toLadaPos (x : float32, y) = Pos(x, y) 
    
    let instructionOfTime (time: Time) (types: InstructionType list) =
        instructions
        |> List.filter (fun i -> List.contains i.typ types)
        |> List.filter (fun i -> i.timeStart >= time && i.timeEnd <= time)
        |> List.sortByDescending _.timeStart
        |> List.tryHead
        
    let timeStart = instructions |> List.map _.timeStart |> List.max
    let timeEnd = instructions |> List.map _.timeEnd |> List.min
        
    let applyEasing e x =
        match e with
        | Easing.None -> x
        | Easing.In -> 1f - cos((x * MathF.PI) / 2f)
        | Easing.Out -> sin((x * MathF.PI) / 2f)
        | Easing.QuadIn -> x * x
        | Easing.QuadOut -> 1f - (1f - x) * (1f - x)
        | Easing.QuadInOut -> if x < 0.5f then 2f * x * x else 1f - MathF.Pow(-2f * x + 2f, 2f) / 2f
        | Easing.QuartOut -> 1f - (1f - x) * (1f - x) * (1f - x) * (1f - x)
        | Easing.SineIn -> 1f - cos((x * MathF.PI) / 2f)
        | Easing.SineOut -> sin((x * MathF.PI) / 2f)
        | Easing.SineInOut -> -(cos(MathF.PI * x) - 1f) / 2f
        | Easing.ExpoIn -> if x = 0f then 0f else MathF.Pow(2f, 10f * x - 10f)
        | Easing.ExpoOut -> if x = 1f then 1f else 1f - MathF.Pow(2f, -10f * x)
        | Easing.ExpoInOut -> if x = 0f then 0f else if x = 1f then 1f else if x < 0.5f then MathF.Pow(2f, 20f * x - 10f) / 2f else (2f - MathF.Pow(2f, -20f * x + 10f)) / 2f
        | Easing.BounceIn -> failwith "Who cares"
        | Easing.BounceOut -> failwith "Who cares"
        | Easing.BounceInOut -> failwith "Who cares"
        | _ -> failwithf "TODO: Take easing from https://easings.net/"
    
    let lerpFloat a b d =
        a * (1f - d) + b * d

    let lerpPosition (ax, ay) (bx, by) d =
        let ax, ay = float32 ax, float32 ay
        let bx, by = float32 bx, float32 by
        (lerpFloat ax bx d), (lerpFloat ay by d)
        
    let lerpFPosition (ax, ay) (bx, by) d =
        (lerpFloat ax bx d), (lerpFloat ay by d)
    
    let getInstructionModifier t i =
        let d = (t - i.timeStart |> float32) / (i.timeEnd - i.timeStart |> float32)
        let d = applyEasing i.easing d
        d
    
    member _.Instructions() = instructions
    
    member _.Position(t: Time) : fPosition =
        let i = instructionOfTime t [ Move ]
        let d = Option.map (getInstructionModifier t) i
        match i, d with
        | Some i, Some d -> lerpPosition (i.iFrom :?> Position) (i.iTo :?> Position) d
        | _, _ -> sprite.x |> float32, sprite.y |> float32
    
    member _.Scale(t: Time) : fPosition = 
        let scale = instructionOfTime t [Scale; VectorScale]
        match scale with
        | Some scale ->
            let d = getInstructionModifier t scale
            let ifr, it =
                match scale.typ with
                | Scale -> (scale.iFrom, scale.iFrom) :> obj, (scale.iTo, scale.iTo) :> obj
                | VectorScale -> (scale.iFrom, scale.iTo)
                | _ -> failwith "Cannot happen"
            lerpFPosition (ifr :?> fPosition) (it :?> fPosition) d
        | None -> (1f, 1f)
    
    member _.Fade(t: Time) : float32 =
        let fade = instructionOfTime t [Fade]
        match fade with
        | Some fade ->
            let d = getInstructionModifier t fade
            lerpFloat (fade.iFrom :?> float32) (fade.iTo :?> float32) d
        | None -> 1f

    member _.Exists(t: Time) : bool =
        timeStart <= t && t <= timeEnd
    
    member _.Image() : string =
        Path.Join(Paths.resourcesFolder, sprite.name)
    
    member _.Origin() : Origin = sprite.origin
    
    member _.Layer() : Layer = sprite.layer
    
    member _.TimeStart() = timeStart
    
    member _.TimeEnd() = timeEnd
    
    member x.Sprite(textureAtlas) =
        Sprite(x.Position(0) |> toLadaPos, textureAtlas, x.Image())
    
    interface IComparable with
        member this.CompareTo(obj) =
            if obj.GetType() = typeof<RdrSprite> then this.TimeStart().CompareTo((obj :?> RdrSprite).TimeStart())
            else -1