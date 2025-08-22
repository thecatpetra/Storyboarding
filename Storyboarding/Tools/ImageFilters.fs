namespace Storyboarding.Tools

open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Drawing
open SixLabors.ImageSharp.Drawing.Processing
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

module ImageFilters =
    type ImageFilter = IImageProcessingContext -> unit

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
