namespace Storyboarding.Tools


namespace Storyboarding.Tools

open System.Collections
open System.IO
open System.Text
open MapsetParser.objects
open SbMonad
open Resources
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.ImageFilters

module SbCompiler =
    let ensureFiles (sb : SB) =
        let mapDirectory = FileInfo(sb.path).DirectoryName
        let sbDirectory = Path.Join(mapDirectory, "s")
        if Directory.Exists(sbDirectory) then
            Directory.Delete(sbDirectory, true)
        let copy path =
            Directory.CreateDirectory(FileInfo(Path.Join(sbDirectory, path)).DirectoryName) |> ignore
            File.Copy(Path.Join(resourcesFolder, path), Path.Join(sbDirectory, path))
        let files = sb.sprites |> Seq.map _.name |> Seq.distinct
        files |> Seq.iter copy

    let insertIntoComments (rendered : string) =
        let builder = StringBuilder()
        builder
            .AppendLine("[Events]")
            .AppendLine("//Background and Video events")
            .AppendLine("//Storyboard Layer 0 (Background)")
            .AppendLine("//Storyboard Layer 1 (Fail)")
            .AppendLine("//Storyboard Layer 2 (Pass)")
            .Append(rendered)
            .AppendLine("//Storyboard Layer 3 (Foreground)")
            .AppendLine("//Storyboard Layer 4 (Overlay)")
            .AppendLine("//Storyboard Sound Samples")
            .ToString()

    // let rec resolveCustomCommands sb =
    //     let sectionFadesOfBlur x =
    //         match x with
    //         | x when x < 0.2f -> (1f - x / 0.2f, x / 0.2f, 0f, 0f)
    //         | x when x < 0.4f -> (0f, 1f - (x - 0.2f) / 0.2f, (x - 0.2f) / 0.2f, 0f)
    //         | x when x < 1f -> (0f, 0f, 1f - (x - 0.4f) / 0.6f, (x - 0.4f) / 0.6f)
    //         | _ -> (0f, 0f, 0f, 1f)
    //     let regions = [0.0f, 0.2f; 0.2f, 0.4f; 0.4f, 1.0f]
    //     let splitIntoInstructions tFrom tTo iFrom iTo =
    //         let timeLen = abs(tFrom - tTo)
    //         let valueLen = abs(iFrom - iTo)
    //         let rec innerForward tFrom tTo iFrom iTo left =
    //             match left with
    //             | (_, ie) :: tl when iFrom < ie ->
    //                 // Inside interval
    //                 let newTFrom = tFrom - (iFrom - ie) / valueLen * timeLen
    //                 (tFrom, newTFrom, iFrom, ie) :: innerForward newTFrom tTo ie iTo tl
    //             | _ :: tl -> innerForward tFrom tTo iFrom iTo tl
    //             | [] -> []
    //         let innerBackwards tFrom tTo iFrom iTo left =
    //             innerForward tFrom tTo (1f - iFrom) (1f - iTo) left |> List.map (fun (a, b, c, d) -> (a, b, 1f - c, 1f - d))
    //         innerForward tFrom tTo iFrom iTo regions @ innerBackwards tFrom tTo iFrom iTo regions
    //
    //
    //     let isOsbInstruction t =
    //         match t.typ with
    //         | Move
    //         | Scale
    //         | Rotate
    //         | VectorScale
    //         | Parameter
    //         | Fade
    //         | Color -> true
    //         | _ -> false
    //     let resolveCustomCommand (sprites : Sprite list) (i : Instruction) =
    //         match i.typ with
    //         | Blur ->
    //             let instructions = splitIntoInstructions (float32 i.timeStart) (float32 i.timeEnd) (i.iFrom :?> float32) (i.iTo :?> float32)
    //             let neededSprites = [
    //                 sprites;
    //                 List.map (fun s -> pureSprite (gaussBlur 5f s.name)) sprites;
    //                 List.map (fun s -> pureSprite (gaussBlur 25f s.name)) sprites;
    //                 List.map (fun s -> pureSprite (gaussBlur 100f s.name)) sprites
    //             ]
    //             let instructionsPerLayer = ResizeArray<ResizeArray<Instruction>>(Seq.init 4 (fun _ -> ResizeArray<Instruction>()))
    //             let newSprites = instructions |> List.collect (fun segment ->
    //                 match segment with
    //                 | ts, te, f, t when 0f < (f + t) / 2f && (f + t) / 2f <= 0.4f ->
    //                     instructionsPerLayer[0].Add(fade (int ts) (int te) f t)
    //                 | _, _, _, _ -> failwith "Blur outside of [0; 1]")
    //             newSprites
    //         | GrayScale -> failwith "TODO"
    //         | _ -> sprites
    //     let inner (sprite : Sprite) =
    //         let pureInstructions = sprite.instructions |> Seq.filter isOsbInstruction
    //         let customInstructions = sprite.instructions |> Seq.filter (isOsbInstruction >> not)
    //         Seq.fold resolveCustomCommand [sprite] customInstructions
    //     {sb with sprites = sb.sprites |> List.collect inner}

    let renderSpritesSubset sprites =
        let builder = StringBuilder()
        let layer =
            function
            | Background -> "Background"
            | Foreground -> "Foreground"
            | Overlay -> "Overlay"
        let writeInstruction (i : Instruction) =
            let formatPosition (x, y) = $"{x},{y}"
            let formatColor (r, g, b) = $"{r},{g},{b}"
            let formatEasing =
                function
                | Easing.None -> 0
                | Easing.In -> 1
                | Easing.Out -> 2
                | Easing.InOut -> 3
                | Easing.QuartIn -> 9
                | Easing.QuartOut -> 10
                | Easing.SineIn -> 15
                | Easing.SineOut -> 16
                | Easing.SineInOut -> 17
                | _ -> System.ArgumentOutOfRangeException() |> raise
            let inner code p1 p2 =
                builder.AppendLine($" {code},{formatEasing i.easing},{i.timeStart},{i.timeEnd},{p1},{p2}") |> ignore
            let innerParameter code p1 =
                builder.AppendLine($" {code},{formatEasing i.easing},{i.timeStart},{i.timeEnd},{p1}") |> ignore
            match i.typ with
            | Move -> inner "M" (formatPosition (i.iFrom :?> Position)) (formatPosition (i.iTo :?> Position))
            | MoveX -> inner "MX" i.iFrom i.iTo
            | MoveY -> inner "MY" i.iFrom i.iTo
            | Fade -> inner "F" i.iFrom i.iTo
            | Scale -> inner "S" i.iFrom i.iTo
            | Rotate -> inner "R" i.iFrom i.iTo
            | VectorScale -> inner "V" (formatPosition (i.iFrom :?> fPosition)) (formatPosition (i.iTo :?> fPosition))
            | Parameter -> innerParameter "P" i.iFrom
            | Color -> inner "C" (formatColor (i.iFrom :?> Color)) (formatColor (i.iTo :?> Color))
            | _ -> failwith "Unsupported command for direct compilation"
        let writeSprite (s : Sprite) =
            builder.AppendLine($"Sprite,{layer s.layer},Centre,\"s/{s.name}\",320,240") |> ignore
            s.instructions |> Seq.rev |> Seq.iter writeInstruction
        sprites |> Seq.rev |> Seq.iter writeSprite
        builder.ToString()

    let writeToDiff (sbPart : string) (bm : Beatmap) =
        let bmPath = Path.Join(bm.songPath, bm.mapPath)
        let contents = File.ReadAllLines bmPath
        let result = StringBuilder()
        let mutable ignoringLines = false
        for line in contents do
            if ignoringLines |> not then result.AppendLine line |> ignore
            if line = "//Storyboard Layer 3 (Foreground)" then
                ignoringLines <- true
                result.AppendLine sbPart |> ignore
            if line = "//Storyboard Layer 4 (Overlay)" then
                ignoringLines <- false
                result.AppendLine "//Storyboard Layer 4 (Overlay)" |> ignore
        let data = result.ToString()
        File.WriteAllText(bmPath, data)

    let write (sb : SB) =
        let customCulture = System.Threading.Thread.CurrentThread.CurrentCulture.Clone() :?> System.Globalization.CultureInfo
        customCulture.NumberFormat.NumberDecimalSeparator <- ".";
        System.Threading.Thread.CurrentThread.CurrentCulture <- customCulture
        ensureFiles sb

        // let sb = resolveCustomCommands sb
        let nonDiffSpecific = sb.sprites |> Seq.filter _.difficulty.IsNone
        let diffSpecific = sb.sprites |> Seq.filter _.difficulty.IsSome |> Seq.groupBy _.difficulty.Value
        Seq.iter (fun (bm : Beatmap, sq) -> writeToDiff (renderSpritesSubset sq) bm) diffSpecific

        File.WriteAllText(sb.path, renderSpritesSubset nonDiffSpecific |> insertIntoComments)


