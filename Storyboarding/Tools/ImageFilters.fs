namespace Storyboarding.Tools

open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing

module ImageFilters =
    type ImageFilter = IImageProcessingContext -> unit

    let commonFilter (filter : ImageFilter) suffix path =
        let fullPath = Path.Join(Resources.resourcesFolder, path)
        let imageFileInfo = FileInfo(fullPath)
        let localPath = Path.GetFileNameWithoutExtension(fullPath) + $"{suffix}{imageFileInfo.Extension}"
        let newFilename = Path.Join(imageFileInfo.DirectoryName, localPath)
        use image = Image.Load(fullPath)
        image.Mutate(filter)
        image.Save(newFilename)
        newFilename.Replace(Resources.resourcesFolder, "")

    let gaussBlur multiplier =
        commonFilter (fun o -> o.GaussianBlur(multiplier) |> ignore) $"_b{multiplier}"

    let grayscale =
        commonFilter (fun o -> o.Grayscale() |> ignore) "_gs"