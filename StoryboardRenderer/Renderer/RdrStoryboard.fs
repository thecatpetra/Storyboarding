namespace StoryboardRenderer.Renderer

open System.Collections.Generic
open System.Linq
open LadaEngine.Engine.Common
open LadaEngine.Engine.Common.SpriteGroup
open LadaEngine.Engine.Renderables.GroupRendering
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

type RdrStoryboard(sb : SB) =
    let sprites =
        sb.sprites
        |> List.filter (_.copy)
        |> List.map RdrSprite
        |> ResizeArray
    
    let unplayed = sprites |> Seq.sortBy _.TimeStart() |> SortedSet<RdrSprite>
    let loaded = HashSet<RdrSprite>()
    let updateIncrement = 13
    
    let resources = sprites |> Seq.map (_.Image()) |> Seq.distinct |> ResizeArray
    
    let idCam = Camera()
    let textureAtlas = TextureAtlas(resources)
    let spriteGroup = SpriteGroup(textureAtlas)
    let mutable lastUpdate = -updateIncrement
    
    member _.Sprites() = sprites
    
    member _.UpdateActiveSprites(t : Time)=
        if (t - lastUpdate) > updateIncrement then
            printfn "Loaded collection update"
            lastUpdate <- t
            // Add new
            while unplayed.Count > 0 && unplayed.Min.Exists(t) do
                let _ = loaded.Add(unplayed.Max)
                spriteGroup.AddSprite(unplayed.Max.Sprite(textureAtlas))
                unplayed.Remove(unplayed.Max) |> ignore
            // Remove old
            let toRemove = loaded.SkipWhile(_.Exists(t))
            for elem in toRemove do
                let _ = loaded.Remove(elem)
                unplayed.Add(elem) |> ignore
        else ()
    
    member this.Render(time : double) =
        let time = time * 1000. |> int
        this.UpdateActiveSprites(time)
        spriteGroup.Render(idCam)
        printfn $"{time} {loaded.Count}"