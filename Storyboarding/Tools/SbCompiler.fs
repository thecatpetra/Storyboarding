namespace Storyboarding.Tools


namespace Storyboarding.Tools

open System
open System.Collections.Generic
open System.IO
open System.Text
open MapsetParser.objects
open SbMonad
open Storyboarding.Tools.Paths
open Storyboarding.Tools.SbTypes

module SbCompiler =
    let performRenaming (sb : SB) (rule : string -> string) =
        let m = Dictionary<string, string>()
        let sprites = sb.sprites |> List.filter (_.copy) |> List.map (fun s ->
            if m.ContainsKey(s.name) |> not then m.Add(s.name, rule s.name)
            assert(m.ContainsValue(m[s.name]))
            {s with name = m[s.name]}
        ) in {sb with sprites = sprites}, m

    let mutable counter = 0
    let rule (x: string) =
        let ext = x.Split(".") |> Seq.last
        counter <- counter + 1
        let name = Convert.ToString(counter, 2).Replace("0", "I").Replace("1", "l")
        $"{name}.{ext}"

    let ensureFiles (sb : SB) =
        let mapDirectory = FileInfo(sb.path).DirectoryName
        let sbDirectory = Path.Join(mapDirectory, "s")
        let newSb, naming = performRenaming sb rule
        if Directory.Exists(sbDirectory) then
            Directory.Delete(sbDirectory, true)
        let copy path =
            Directory.CreateDirectory(FileInfo(Path.Join(sbDirectory, naming[path])).DirectoryName) |> ignore
            File.Copy(Path.Join(resourcesFolder, path), Path.Join(sbDirectory, naming[path]), true)
        let files = sb.sprites |> Seq.filter (_.copy) |> Seq.map _.name |> Seq.distinct
        files |> Seq.iter copy
        newSb

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

    let renderSpritesSubset sprites =
        let builder = StringBuilder()
        let layer = function
            | Background -> "Background"
            | Foreground -> "Foreground"
            | Overlay -> "Overlay"
        let origin = function
            | Origin.Center -> "Centre"
            | Origin.BottomCentre -> "BottomCentre"
            | Origin.TopCentre -> "TopCentre"
            | Origin.TopLeft -> "TopLeft"
            | Origin.CentreRight -> "CentreRight"
            | Origin.CentreLeft -> "CentreLeft"
        let writeInstruction (i : Instruction) =
            let formatPosition (x, y) = $"{x},{y}"
            let formatColor (r, g, b) =
                assert (r >= 0 && r < 256)
                assert (g >= 0 && g < 256)
                assert (b >= 0 && b < 256)
                $"{int r},{int g},{int b}"
            let formatEasing (e: Easing) =
                assert(Easing.GetValues() |> Seq.contains e)
                int32 e
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
            builder.AppendLine($"Sprite,{layer s.layer},{origin s.origin},\"s/{s.name}\",{s.x},{s.y}") |> ignore
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
        let sb = ensureFiles sb

        // let sb = resolveCustomCommands sb
        let nonDiffSpecific = sb.sprites |> Seq.filter _.difficulty.IsNone
        let diffSpecific = sb.sprites |> Seq.filter _.difficulty.IsSome |> Seq.groupBy _.difficulty.Value
        Seq.iter (fun (bm : Beatmap, sq) -> writeToDiff (renderSpritesSubset sq) bm) diffSpecific

        File.WriteAllText(sb.path, renderSpritesSubset nonDiffSpecific |> insertIntoComments)


    let inline (!>) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)