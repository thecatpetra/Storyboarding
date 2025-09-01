namespace GpuDrawingUtils

open System.Collections.Generic
open GpuDrawingUtils.Common
open GpuDrawingUtils.FakeWindow
open GpuDrawingUtils.Framebuffer
open MapsetParser.objects.hitobjects
open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

module ShaderFilter =
    let shaderFilter shader (image : Image<Rgba32>) path =
        let size = Vector2i(image.Bounds.Width, image.Bounds.Height)

        let load w =
            prelude size
            let program = createRenderShader "Shaders/usual.vert" $"Shaders/{shader}"
            let defaultProgram = createRenderShader "Shaders/usual.vert" "Shaders/usual.frag"
            let fbo = createFramebuffer size
            let texture = textureOfImage image
            (texture, program, fbo, defaultProgram)

        let render (w: FakeWindow<_>) (texture, program : Program, fbo, defaultProgram : Program) =
            runWithFbo fbo.handle (fun () ->
            useTexture texture TextureUnit.Texture0
            useProgram program.handle
            renderFullScreenSprite program
            exportImage fbo.texture size path)
            useTexture fbo.texture TextureUnit.Texture0
            useProgram defaultProgram.handle
            renderFullScreenSprite program
            w.Close()

        withFakeWindow size load render
