namespace StoryboardRenderer.Engine

open OpenTK.Graphics.OpenGL
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

module Texture =
    type GLTexture =
        struct
            val handle: int
            
            new(handle : int) = {handle = handle}
            
            new(image : Image<Rgba32>) =
                let handle = GL.GenTexture()
                GL.ActiveTexture(TextureUnit.Texture0)
                GL.BindTexture(TextureTarget.Texture2D, handle)
                let pixelBytes = Array.init<byte> (image.Width * image.Height * 4) (fun _ -> 0uy)
                image.CopyPixelDataTo(pixelBytes)
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Bounds.Width, image.Bounds.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelBytes)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat)
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat)
                GLTexture(handle)
                
            new(path : string) =
                let image = Image.Load<Rgba32>(path)
                GLTexture(image)
            
            member this.Use(u: TextureUnit) =
                GL.ActiveTexture(u);
                GL.BindTexture(TextureTarget.Texture2D,  this.handle)
        end
    
    