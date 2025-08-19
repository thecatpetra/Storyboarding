namespace Storyboarding.Tools

open System.Drawing
open System.Text
open MapsetParser.objects
open SbTypes
open Resources
open System.IO
open Storyboarding.Tools.Paths
open Storyboarding.Tools.SbTypes

// Goofy and not a monad actually
module SbMonad =
    type T = SB -> SB

    and SB = {
        path: string
        beatmapSet: BeatmapSet
        sprites: Sprite list
    }

    let addToLastSprite i (l : Sprite list) =
        match l with
        | h :: tl -> {h with instructions = i :: h.instructions} :: tl
        | _ -> failwith "Operation on non-existent sprite"

    let applyToLastSprite f (s : SB) =
        match s.sprites with
        | h :: tl -> {s with sprites = f h :: tl}
        | _ -> failwith "Operation on non-existent sprite"

    let applyToLastInstruction f (s : SB) =
        match s.sprites with
        | h :: tl ->
            let i =
                match h.instructions with
                | h :: tl -> f h :: tl
                | _ -> failwith "No instruction to apply to"
            {s with sprites = {h with instructions = i } :: tl}
        | _ -> failwith "Operation on non-existent sprite"

    let addInstruction genInstr (sb : SB) = {sb with sprites = addToLastSprite (genInstr sb) sb.sprites}
    let addSprite s (sb : SB) = {sb with sprites = s :: sb.sprites }

    let (>>=) : T -> T -> T = fun f g sb -> f sb |> g

    let (>?=) : bool -> T -> T = fun choice f sb ->
        if choice then f sb
        else sb

    // Push instruction (bad)
    let (>>|) s (i : T) =
        let sb = i {path = "err"; beatmapSet = null; sprites = [s] }
        sb.sprites |> Seq.head

    let monadicMap l f =
        let rec inner l =
            match l with
            | h :: tl -> f h >>= inner tl
            | [] -> id
        inner l

    let monadicMapi l f =
        let rec inner l i =
            match l with
            | h :: tl -> f i h >>= inner tl (i + 1)
            | [] -> id
        inner l 0

    let openSb path =
        let bms = BeatmapSet(FileInfo(path).DirectoryName);
        {path = path; beatmapSet = bms; sprites = [] }

    // TODO: String time and correctness check
    let isTime _ = true

    let iBasic<'a> code tStart tEnd (iFrom : 'a) (iTo : 'a) =
        assert isTime(tStart)
        let genInstr = fun _ -> { typ = code
                                  easing = Easing.None
                                  timeStart = tStart
                                  timeEnd = tEnd
                                  iFrom = iFrom
                                  iTo = iTo }
        addInstruction genInstr

    let retrieveLastTo typ def f (sb : SB) =
        let lastSprite = sb.sprites |> Seq.tryHead
        let viableInstruction = lastSprite |> Option.bind (fun s -> s.instructions |> Seq.tryFind (fun t -> t.typ = typ))
        match viableInstruction with
        | Some v -> f v.iTo sb
        | None -> f (def ()) sb

    let move = iBasic<Position> InstructionType.Move
    let movex = iBasic<int32> InstructionType.MoveX
    let movey = iBasic<int32> InstructionType.MoveY
    let fade = iBasic<float32> InstructionType.Fade
    let rotate = iBasic<float32> InstructionType.Rotate
    let scale = iBasic<float32> InstructionType.Scale
    let vectorScale = iBasic<fPosition> InstructionType.VectorScale
    let color = iBasic<Color> InstructionType.Color
    let blur = iBasic<float32> InstructionType.Blur
    let easing e = applyToLastInstruction (fun i -> {i with easing = e })
    let layer l = applyToLastSprite (fun i -> {i with layer = l })
    let origin o = applyToLastSprite (fun i -> {i with origin = o })

    let vectorScaleTo a b d = (retrieveLastTo InstructionType.VectorScale) (fun () -> (0f, 0f)) (fun c -> vectorScale a b (c :?> fPosition) d)
    let scaleTo a b d =(retrieveLastTo InstructionType.Scale) (fun () -> 0f) (fun c -> scale a b (c :?> float32) d)
    let fadeTo a b d = (retrieveLastTo InstructionType.Fade) (fun () -> 0f) (fun c -> fade a b (c :?> float32) d)

    let scaleAsBg timeStart timeStop (sb : SB) =
        let s = sb.sprites |> Seq.head
        let image = Image.FromFile(Path.Join(resourcesFolder, s.name))
        let w = 850f / (float32 image.Width)
        scale timeStart timeStop w w sb


    let parameter letter =
         let i sb =
             let startTime = sb.sprites |> Seq.head |> _.instructions |> Seq.map _.timeStart |> Seq.min
             { typ = InstructionType.Parameter
               easing = Easing.None
               timeStart = startTime
               timeEnd = startTime
               iFrom = letter
               iTo = null }
         addInstruction i

    let alpha = parameter "A"
    let horizontalFlip = parameter "H"
    let verticalFlip = parameter "V"

    let img resource =
        let i = { name = resource; layer = Layer.Background; difficulty = None; instructions = []; origin = Center; x = 320; y = 240; copy = true }
        addSprite i

    let coords (x, y) =
        applyToLastSprite (fun s -> {s with x = x; y = y})

    let noCopy = applyToLastSprite (fun s -> {s with copy = false})

    let pureSprite resource =
        { name = resource; layer = Layer.Background; difficulty = None; instructions = []; origin = Center; x = 320; y = 240; copy = true }

    let diffSpecific diff =
        applyToLastSprite (fun s -> { s with difficulty = Some(diff) })

    let forEachDiff f (sb : SB) =
        let difficulties = sb.beatmapSet.beatmaps |> Seq.cast<Beatmap> |> Seq.toList
        let makeDiffSpecific (f : Beatmap -> T) bm sb =
            let emptySb = {sb with sprites = [] }
            let r = f bm emptySb
            let dsSprites = r.sprites |> List.map (fun s -> {s with difficulty = Some bm})
            { sb with sprites = sb.sprites @ dsSprites }
        (monadicMap difficulties (makeDiffSpecific f)) sb

    let forEachHitObject (f: HitObject -> T) =
        forEachDiff (fun bm ->
        let sq = bm.hitObjects |> Seq.cast |> Seq.toList
        monadicMap sq f)

    let beatTime (time : int) (sb : SB) =
        let anyBm = sb.beatmapSet.beatmaps |> Seq.cast<Beatmap> |> Seq.head
        anyBm.timingLines |> Seq.filter (fun x -> x.offset < time) |> Seq.last |> _.msPerBeat |> abs |> int

    let timeDivisionMapi (timeStart : Time) (timeEnd : Time) (duration: Time) =
        let segmentCount = timeEnd - timeStart |> fun diff -> diff / duration
        let genSegment i = (i * duration + timeStart, (i + 1) * duration + timeStart)
        let segments = [ for i in 0..segmentCount -> genSegment i]
        monadicMapi segments

    let timeDivisionMap (timeStart : Time) (timeEnd : Time) (duration: Time) f =
        timeDivisionMapi timeStart timeEnd duration (fun _ -> f)

    let t (x : string) =
        let [|m; s; ms|] = x.Split(":")
        (int m) * 60000 + (int s) * 1000 + (int ms)

    let getCurrentSprite (sb : SB) =
        sb.sprites |> Seq.head |> _.name

    let removeBg (sb : SB) =
        // Sprite,Background,TopLeft,"background.jpg",0,0
        let location = sb.beatmapSet.beatmaps[0].backgrounds[0].path
        sb |> (img location >> origin TopLeft >> coords (0, 0) >> noCopy)
