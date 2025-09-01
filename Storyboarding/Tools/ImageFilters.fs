namespace Storyboarding.Tools

open System
open System.Drawing
open System.IO
open GpuDrawingUtils
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Drawing
open SixLabors.ImageSharp.Drawing.Processing
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open Storyboarding.Tools.SbTypes

module ImageFilters =
    type ImageFilter = IImageProcessingContext -> unit

    // Format: Key | Value
    let imageFilterCache = "image_cache.txt"

    let cache =
        let lines = File.ReadAllLines(imageFilterCache)
        lines |> Seq.map (_.Split(" | ", 2)) |> Seq.map (fun [|a; b|] -> a, b) |> dict

    let writeToCache key value =
        File.AppendAllLines(imageFilterCache, [$"{key} | {value}"])

    let readCached key =
        cache[key]

    let doOnImage suffix path f =
        let fullPath = Path.Join(Paths.resourcesFolder, path)
        let imageFileInfo = FileInfo(fullPath)
        let localPath = Path.GetFileNameWithoutExtension(fullPath) + $"{suffix}{imageFileInfo.Extension}"
        let newFilename = Path.Join(imageFileInfo.DirectoryName, localPath)
        if File.Exists(newFilename) then newFilename.Replace(Paths.resourcesFolder, "")
        else use image = Image.Load(fullPath)
             f image
             image.Save(newFilename)
             newFilename.Replace(Paths.resourcesFolder, "")

    let makeNewImage suffix path (f: Image -> Image) =
        let fullPath = Path.Join(Paths.resourcesFolder, path)
        let imageFileInfo = FileInfo(fullPath)
        let localPath = Path.GetFileNameWithoutExtension(fullPath) + $"{suffix}{imageFileInfo.Extension}"
        let newFilename = Path.Join(imageFileInfo.DirectoryName, localPath)
        use image = Image.Load(fullPath)
        let r = f image
        r.Save(newFilename)
        newFilename.Replace(Paths.resourcesFolder, "")

    let makeNewImageNoSave suffix path (f: Image<Rgba32> -> String -> Unit) : String =
        let fullPath = Path.Join(Paths.resourcesFolder, path)
        let imageFileInfo = FileInfo(fullPath)
        let localPath = Path.GetFileNameWithoutExtension(fullPath) + $"{suffix}{imageFileInfo.Extension}"
        let newFilename = Path.Join(imageFileInfo.DirectoryName, localPath)
        use image = Image.Load<Rgba32>(fullPath)
        if not <| File.Exists(newFilename) then f image newFilename
        newFilename.Replace(Paths.resourcesFolder, "")

    let commonFilter (filter : ImageFilter) suffix path =
        doOnImage suffix path (_.Mutate(filter))

    let gaussBlur multiplier =
        commonFilter (fun o -> o.GaussianBlur(multiplier) |> ignore) $"_b{multiplier}"

    let grayscale =
        commonFilter (fun o -> o.Grayscale() |> ignore) "_gs"

    let resizeToFHD path =
        doOnImage "_fhd" path (_.Mutate(fun o -> o.Resize(1920, 1080) |> ignore))

    let oneTenthFhd path =
        doOnImage "_otfhd" path (_.Mutate(fun o -> o.Resize(192, 108) |> ignore))

    let twoByTwo path =
        doOnImage "_tbt" path (_.Mutate(fun o -> o.Resize(2, 2) |> ignore))

    let resizeTo (x: int) y path =
        doOnImage $"_r1s{x}x{y}" path (_.Mutate(fun o -> o.Resize(x, y) |> ignore))

    let resize1To x path =
        doOnImage $"_r1s{x}" path (_.Mutate(fun o -> o.Resize(x, x) |> ignore))

    let kodachrome path =
        doOnImage $"_kdchr" path (_.Mutate(fun o -> o.Kodachrome() |> ignore))

    let negative path =
        doOnImage $"_neg" path (_.Mutate(fun o -> o.Invert() |> ignore))

    let sepia path =
        doOnImage $"_sp" path (_.Mutate(fun o -> o.Sepia() |> ignore))

    let oilPaint path =
        doOnImage $"_olpnt" path (_.Mutate(fun o -> o.OilPaint() |> ignore))

    let partialFills path =
        let partialFill radius =
            doOnImage $"_pf{radius}" path (fun g -> g.Mutate(fun i ->
                use mask = new Image<Rgba32>(g.Width, g.Height);
                let ellipse = EllipsePolygon(0f, 0f, radius)
                mask.Mutate(fun x -> x.Fill(Color.White, ellipse) |> ignore)
                mask.Save("mask_67342.png")
                i.DrawImage(mask, GraphicsOptions(AlphaCompositionMode = PixelAlphaCompositionMode.SrcIn)) |> ignore
            ))
        [10f; 30f; 50f; 70f; 100f; 150f; 200f] |> List.map partialFill

    let maskOnWhite path =
        makeNewImage "_mow" path (fun g ->
            let r = new Image<Rgba32>(g.Width, g.Height)
            r.Mutate(fun x -> x.Fill(Color.White) |> ignore)
            r.Mutate(fun m -> m.DrawImage(g, GraphicsOptions(AlphaCompositionMode = PixelAlphaCompositionMode.Xor)) |> ignore)
            r)

    let randomGrouping size count path =
        makeNewImage $"_rg{size}x{count}" path (fun g ->
            let random = Random()
            let r = new Image<Rgba32>(size, size)
            r.Mutate(fun x -> x.Fill(Color.Transparent) |> ignore)
            let boundary = size - g.Height * 2
            assert(boundary > 0)
            for _ in [1..count] do
                let point = Point(random.Next(boundary) + g.Width, random.Next(boundary) + g.Height)
                r.Mutate(fun m -> m.DrawImage(g, point, GraphicsOptions(AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver)) |> ignore)
            r)

    let pad size path =
        makeNewImage $"_pad{size}" path (fun g ->
            let r = new Image<Rgba32>(size * 2 + g.Width, size * 2 + g.Height)
            r.Mutate(fun x -> x.Fill(Color.Transparent) |> ignore)
            let point = Point(size, size)
            r.Mutate(fun m -> m.DrawImage(g, point, GraphicsOptions(AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver)) |> ignore)
            r)

    let cropImage path =
        let fullPath = Path.Join(Paths.resourcesFolder, path)
        let suffix = $"_cropped"
        let imageFileInfo = FileInfo(fullPath)
        let localPath = Path.GetFileNameWithoutExtension(fullPath) + $"{suffix}{imageFileInfo.Extension}"
        let newFilename = Path.Join(imageFileInfo.DirectoryName, localPath)
        if File.Exists(newFilename) then
            let size = readCached $"OFFSET OF {newFilename}" |> _.Split(", ") |> Seq.map int |> Seq.toList |> (fun [a; b] -> a, b)
            size, newFilename.Replace(Paths.resourcesFolder, "")
        else
            use g = Image.Load<Rgba32>(fullPath)
            let mutable minX = g.Width - 1
            let mutable minY = g.Height - 1
            let mutable maxX = 0
            let mutable maxY = 0
            for x in [0..g.Width-1] do
                for y in [0..g.Height-1] do
                    if g[x, y].A <> 0uy then
                        minX <- min minX x
                        minY <- min minY y
                        maxX <- max maxX x
                        maxY <- max maxY y
            g.Mutate(fun g -> g.Crop(Rectangle(Point(minX, minY), Size(maxX - minX, maxY - minY))) |> ignore)
            g.Save(newFilename)
            writeToCache $"OFFSET OF {newFilename}" $"{minX}, {minY}"
            (minX, minY), newFilename.Replace(Paths.resourcesFolder, "")

    let renderTextToImage text anyImage size =
        failwith "TODO"

    let applyShader shader image =
        let suffix = $"_shadered_{shader}"
        makeNewImageNoSave suffix image (fun n g -> ShaderFilter.shaderFilter shader n g)