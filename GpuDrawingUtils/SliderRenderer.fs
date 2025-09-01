namespace GpuDrawingUtils

open System.Collections.Generic
open GpuDrawingUtils.Common
open GpuDrawingUtils.FakeWindow
open GpuDrawingUtils.Framebuffer
open MapsetParser.objects.hitobjects
open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics

module SliderRenderer =
    let drawSlidersInner (points : (float32 array * string) seq) =
        let size = Vector2i(2048, 2048)

        let mutable frame = 0

        let load w =
            prelude size
            let program = createRenderShader "Shaders/usual.vert" "Shaders/slider.frag"
            let defaultProgram = createRenderShader "Shaders/usual.vert" "Shaders/usual.frag"
            let fbo = createFramebuffer size
            let texture = createTexture size
            (texture, program, fbo, defaultProgram)

        let render (w: FakeWindow<_>) (texture, program : Program, fbo, defaultProgram : Program) =
            match Seq.tryItem frame points with
            | Some (sliderPoints, name) ->
                runWithFbo fbo.handle (fun () ->
                useTexture texture TextureUnit.Texture0
                useProgram program.handle
                assert (sliderPoints.Length < 8096)
                passArray program sliderPoints
                renderFullScreenSprite program
                exportImage fbo.texture size name)
                useTexture fbo.texture TextureUnit.Texture0
                useProgram defaultProgram.handle
                renderFullScreenSprite program
                frame <- frame + 1
            | None ->
                w.Close()

        withFakeWindow size load render

    let renderSliders mapId (hs: Slider seq) : IDictionary<int, string> =
        let folder = @"C:\Users\arthur\Documents\Storyboarding\Storyboarding\Resources\gp_generated"
        let vectorsToArray (vs: System.Numerics.Vector2 seq) = [| for v in vs do yield v.X; yield v.Y |]
        let allSliders = hs |> Seq.map (fun i -> vectorsToArray i.pathPxPositions, $"{folder}/{mapId}/slider_at_{i.time}.png")
        drawSlidersInner allSliders
        let result = hs |> Seq.map (fun i -> i.time, $"gp_generated/{mapId}/slider_at_{i.time}.png")
        result |> dict