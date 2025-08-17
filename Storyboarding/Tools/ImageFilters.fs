namespace Storyboarding.Tools

open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing

module ImageFilters =
    type ImageFilter = IImageProcessingContext -> unit

    let doOnImage suffix path f =
        let fullPath = Path.Join(Paths.resourcesFolder, path)
        let imageFileInfo = FileInfo(fullPath)
        let localPath = Path.GetFileNameWithoutExtension(fullPath) + $"{suffix}{imageFileInfo.Extension}"
        let newFilename = Path.Join(imageFileInfo.DirectoryName, localPath)
        use image = Image.Load(fullPath)
        f image
        image.Save(newFilename)
        newFilename.Replace(Paths.resourcesFolder, "")

    let commonFilter (filter : ImageFilter) suffix path =
        doOnImage suffix path (_.Mutate(filter))

    let gaussBlur multiplier =
        commonFilter (fun o -> o.GaussianBlur(multiplier) |> ignore) $"_b{multiplier}"

    let grayscale =
        commonFilter (fun o -> o.Grayscale() |> ignore) "_gs"

    let resizeToFHD path =
        doOnImage "_fhd" path (_.Mutate(fun o -> o.Resize(1920, 1080) |> ignore))