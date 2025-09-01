namespace GpuDrawingUtils

open GpuDrawingUtils.Common
open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics

module Framebuffer =
    [<Measure>] type framebuffer

    type FrameBuffer = {
        handle: int<framebuffer>
        texture: int<texture>
    }

    let fbCreateTexture (size: Vector2i) =
        let handle = GL.GenTexture()
        GL.ActiveTexture(TextureUnit.Texture0)
        GL.BindTexture(TextureTarget.Texture2D, handle)
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Bgra, PixelType.UnsignedByte, nativeint 0)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.ClampToEdge, int TextureWrapMode.Repeat)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.ClampToEdge, int TextureWrapMode.Repeat)
        handle * 1<texture>

    let createFramebuffer (size: Vector2i) =
        let fbo = GL.GenFramebuffer()
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo)
        let texture = fbCreateTexture size
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, int texture, 0)
        { handle = fbo * 1<framebuffer>; texture = texture }

    let runWithFbo (fbo: int<framebuffer>) f =
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, int fbo)
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        f ()
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit);