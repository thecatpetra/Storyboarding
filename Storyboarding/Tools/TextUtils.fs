namespace Storyboarding.Tools

open System.Diagnostics
open System.IO
open Storyboarding.Tools.Paths
open Storyboarding.Tools.SbTypes
open SbMonad
open System.Drawing
open Resources
open Storyboarding.Tools.ImageFilters

module TextUtils =
    let createFont font size =
        let saveFolder = Path.Join(savePathPrefix, FileInfo(font).Name)
        Directory.CreateDirectory(saveFolder) |> ignore
        let args = [|Path.Join(scriptPath, "font_renderer.py"); font; saveFolder; size.ToString()|] |> Seq.ofArray
        let p = Process.Start("ffpython", args)
        printf $"Generating font {font} with size {size}"
        p.WaitForExit()
        let args = [|Path.Join(scriptPath, "font_transparent.py"); saveFolder |] |> Seq.ofArray
        let p = Process.Start("python3", args)
        printf $"making font {font} transparent"
        p.WaitForExit()

    let createFontSubset font subset size =
        let saveFolder = Path.Join(savePathPrefix, FileInfo(font).Name)
        Directory.CreateDirectory(saveFolder) |> ignore
        let args = [|Path.Join(scriptPath, "font_subset_renderer.py"); font; saveFolder; size.ToString(); subset|] |> Seq.ofArray
        let p = Process.Start("ffpython", args)
        printf $"Generating font {font} with size {size}"
        p.WaitForExit()
        let args = [|Path.Join(scriptPath, "font_transparent.py"); saveFolder |] |> Seq.ofArray
        let p = Process.Start("python3", args)
        printf $"making font {font} transparent"
        p.WaitForExit()

    let ensureFont font =
        let fontFolder = Path.Join(resourcesFolder, font)
        let fontPath = Path.Join(fontsFolder, FileInfo(font).Name)
        if Directory.Exists(fontFolder) |> not || Directory.GetFiles fontFolder |> Array.length |> (=) 0 then
            createFont fontPath 192

    type CharAction = int -> string -> float32 -> Position -> T
    type TextEffect = Time -> CharAction

    let imageWidth path =
        let image = Image.FromFile(path)
        image.Width

    let addX (x, y) b = (x + (int32 b), y)

    let text font (txt : string) (charAction : CharAction) (position : Position) scale =
        let unicodes = txt |> Seq.toList |> List.map int
        // Lazy init font
        let fontFolder = Path.Join(resourcesFolder, font)
        let fontPath = Path.Join(fontsFolder, FileInfo(font).Name)
        ensureFont font
        if Directory.Exists(fontFolder) |> not || Directory.GetFiles fontFolder |> Array.length |> (=) 0 then
            createFont fontPath 64
        // TODO: Monadic map/fold
        let rec inner currentPosition left i =
            match left with
            | h :: tl ->
                let image = Path.Join(font, $"{h}.png")
                let fullPath = Path.Join(resourcesFolder, image)
                let width = imageWidth fullPath
                let diff = (float32 width) * scale
                let act = charAction i image scale (addX currentPosition (diff / 2f))
                act  >>= inner (addX currentPosition diff) tl (i + 1)
            | [] -> id
        inner position unicodes 0

    let textWidth font (txt : string) (scale : float32) =
        let unicodes = txt |> Seq.toList |> List.map int
        ensureFont font
        let listFolder acc e =
            let image = Path.Join(font, $"{e}.png")
            let fullPath = Path.Join(resourcesFolder, image)
            let width = imageWidth fullPath
            acc + width
        List.fold listFolder 0 unicodes |> float32 |> (*) scale

    let centreOfText font (txt : string) (scale : float32) =
        let totalWidth = textWidth font txt scale
        (- totalWidth |> int) / 2 + 320, 240

    let textCenter font txt charAction scale =
        let p = centreOfText font txt scale
        text font txt charAction p scale

    let noEffect time : CharAction = fun _ image s p ->
        img image
        >>= move time time p p
        >>= scale time time s s
        >>= fade time (time + 10000) 1f 1f

    let chromoInOut stay diff time : CharAction = fun i image s p ->
        let stride = 3f
        let effectTime = 150
        let colorOfIndex =  function
            | 0 -> (255, 0, 0)
            | 1 -> (0, 255, 0)
            | _ -> (0, 0, 255)
        let inner c dp =
            img image
            >>= move (time + i * diff) (time + i * diff + effectTime) (addX p dp) p
            >>= fade (time + i * diff) (time + effectTime + i * diff) 0f 1f
            >>= color time time c c
            >>= scale time time s s
            >>= move (stay + time + i * diff) (stay + time + i * diff + effectTime) p (addX p (-dp))
            >>= fade (stay + time + i * diff) (time + effectTime + stay + i * diff) 1f 0f
            >>= alpha
        monadicMapi [-1f; 0f; 1f] (fun i e -> inner (colorOfIndex i) (stride * e))

    let neonInOut stay time (c1 : SbTypes.Color) (c2 : SbTypes.Color) : CharAction = fun i image s p ->
        let blurred = image |> gaussBlur 6f
        let dIn = SbRandom.rng.Next(0, 200)
        let dOut = SbRandom.rng.Next(0, 600)
        img blurred
        >>= move time time p p
        >>= scale time time s s
        >>= fade (time + dIn) (time + dIn + 200) 0f 1f
        >>= fade (time + stay) (time + stay + dOut) 1f 0f
        >>= color time time c2 c2
        >>= alpha
        >> img image
        >>= move time time p p
        >>= scale time time s s
        >>= fade (time + dIn) (time + dIn + 200) 0f 1f
        >>= fade (time + stay) (time + stay + dOut) 1f 0f
        >>= color time time c1 c1
        >>= alpha

    let chromoSpinInOut stay diff time : CharAction = fun i image s p ->
        let stride = 1f
        let effectTime = 400
        let colorOfIndex =  function
            | 0 -> (255, 0, 0)
            | 1 -> (0, 255, 0)
            | _ -> (0, 0, 255)
        let inner c dp =
            img image
            >>= move (time + i * diff) (time + i * diff + effectTime) (addX p dp) p
            >>= vectorScale (time + i * diff) (time + effectTime + i * diff) (0f, s) (s, s)
            >> easing Easing.QuadIn
            >>= color time time c c
            >>= move (stay + time + i * diff) (stay + time + i * diff + effectTime) p (addX p (-dp))
            >>= vectorScale (stay + time + i * diff) (time + effectTime + stay + i * diff) (s, s) (0f, s)
            >> easing Easing.QuadOut
            >>= alpha
        monadicMapi [-1f; 0f; 1f] (fun i e -> inner (colorOfIndex i) (stride * e))