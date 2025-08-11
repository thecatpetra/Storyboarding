using MapsetParser.objects.hitobjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FastDrawing.Sliders;

public class SliderDrawer
{

    //     let drawSliderBody (segments : fSliderSegment seq) =
    // let folder = @"C:\Users\arthur\Documents\Storyboarding\Storyboarding\Resources\gp_generated"
    // use image = new Image<Rgba32>(1920, 1080)
    // // TODO: Use OpenTK + compute shader...
    // image.ProcessPixelRows(fun accessor ->
    // for y in 0..(accessor.Height - 1) do
    // let pixelRow = accessor.GetRowSpan(y)
    //     for x in 0..(pixelRow.Length - 1) do
    // let d = Seq.map (fun s -> distanceLsP (s.ffirst, s.fsecond) (y |> float32, x |> float32)) segments |> Seq.toList
    //     pixelRow[x] <- Rgba32.op_Implicit(Color.FromRgba(255uy, 255uy, 255uy, d |> Seq.min |> byte))
    // )
    // image.Save(segments |> Seq.head |> _.time |> sprintf "%s\\%d_slider.png" folder)
    public static void DrawSliderImage(Slider slider)
    {
        var folder = @"C:\Users\arthur\Documents\Storyboarding\Storyboarding\Resources\gp_generated";

        // using (var image = new Image<Rgba32>(1920, 1080))
        // {
        //     for (int)
        // }
    }

}