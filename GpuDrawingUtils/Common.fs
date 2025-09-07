namespace GpuDrawingUtils

open System
open System.Collections.Generic
open System.IO
open System.Numerics
open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

module Common =
    [<Measure>] type texture
    [<Measure>] type shader
    [<Measure>] type program

    type Program = {
        handle: int<program>
        uniforms: IDictionary<string, int>
    }

    let createTexture (size: Vector2i) : int<texture> =
        let tex_output = GL.GenTexture()
        GL.ActiveTexture(TextureUnit.Texture0)
        GL.BindTexture(TextureTarget.Texture2D, tex_output)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat)
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero)
        GL.BindImageTexture(0, tex_output, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f)
        tex_output * 1<texture>

    let textureOfImage (bmp : Image<Rgba32>) =
        let handle = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, handle);
        let pixelBytes = Array.init<byte> (bmp.Width * bmp.Height * 4) (fun _ -> 0uy)
        bmp.CopyPixelDataTo(pixelBytes)
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Bounds.Width, bmp.Bounds.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelBytes)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat)
        handle * 1<texture>


    let exportImage (tex_output : int<texture>) (size: Vector2i) path =
        let mutable fboId = 0
        GL.Ext.GenFramebuffers(1, &fboId)
        GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboId)
        GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, int tex_output, 0)
        let bytes : byte array = Array.zeroCreate (size.X * size.Y * 4) 
        GL.ReadPixels(0, 0, size.X, size.Y, PixelFormat.Bgra, PixelType.UnsignedByte, bytes)
        GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0)
        GL.Ext.DeleteFramebuffers(1, &fboId)
        use b = Image<Rgba32>.Load(bytes)
        let file = FileInfo(path)
        b.Save(file.FullName)
        printfn $"Saved to {file.FullName}"

    let createComputeShader path : int<program> =
        let toolShader = File.ReadAllText(path)
        let drawShader = GL.CreateShader(ShaderType.ComputeShader)
        let mutable code: int = 0
        GL.ShaderSource(drawShader, toolShader)
        GL.CompileShader(drawShader)
        GL.GetShader(drawShader, ShaderParameter.CompileStatus, &code)

        if (code <> int All.True) then
            printfn $"{toolShader}"
            let infoLog = GL.GetShaderInfoLog(drawShader)
            printfn $"{infoLog}"
            raise <| Exception($"Error occurred whilst compiling Shader({drawShader}).\n\n{infoLog}")

        let program = GL.CreateProgram();
        GL.AttachShader(program, drawShader);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, &code);
        if (code <> int All.True) then
            raise <| Exception($"Error occurred whilst linking Program({program})");

        let mutable numberOfUniforms: int = 0
        GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, &numberOfUniforms);

        let uniformLocations = Dictionary<string, int>();
        let mutable size: int = 0
        let mutable uType: ActiveUniformType = ActiveUniformType.Int

        for i in [0..numberOfUniforms-1] do
            let key = GL.GetActiveUniform(program, i, &size, &uType)
            let location = GL.GetUniformLocation(program, key)
            uniformLocations.Add(key, location);

        GL.DetachShader(program, drawShader)
        GL.DeleteShader(drawShader)
        program * 1<program>


    let createRenderShader vert frag : Program =

        let compileShader (shader: int<shader>) source =
            GL.CompileShader(int shader)
            let mutable code: int = 0
            GL.GetShader(int shader, ShaderParameter.CompileStatus, &code)
            if (code <> int All.True) then
                let infoLog = GL.GetShaderInfoLog(int shader)
                printfn $"{source}"
                raise <| Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}")

        let linkProgram (program: int<program>) =
            GL.LinkProgram(int program);
            let mutable code: int = 0
            GL.GetProgram(int program, GetProgramParameterName.LinkStatus, &code);
            if (code <> (int)All.True) then
                raise <| Exception($"Error occurred whilst linking Program({program})");

        let shaderSource = File.ReadAllText(vert)
        let vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, shaderSource);
        compileShader (vertexShader * 1<shader>) shaderSource
        let shaderSource = File.ReadAllText(frag)
        let fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, shaderSource);
        compileShader (fragmentShader * 1<shader>) shaderSource
        let handle = GL.CreateProgram();
        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);
        linkProgram (handle * 1<program>)
        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader)
        let mutable numberOfUniforms: int = 0
        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, &numberOfUniforms);
        let _uniformLocations = Dictionary<string, int>();
        let mutable size: int = 0
        let mutable uType: ActiveUniformType = ActiveUniformType.Int
        for i in [0..numberOfUniforms-1] do
            let key = GL.GetActiveUniform(handle, i, &size, &uType);
            let location = GL.GetUniformLocation(handle, key);
            _uniformLocations.Add(key, location);
        { handle = handle * 1<program>; uniforms = _uniformLocations }

    let useTexture (handle: int<texture>) (slot: TextureUnit) =
        GL.ActiveTexture(slot);
        GL.BindTexture(TextureTarget.Texture2D, int handle)

    let useProgram (p: int<program>) =
        GL.UseProgram(int p);

    let renderFullScreenSprite (p : Program) =
        let vertices : float32 array = [|
            -1f; -1f; 0.0f; 0.0f; 0.0f;  // bottom-left
             1f; -1f; 0.0f; 1.0f; 0.0f;  // bottom-right
             1f;  1f; 0.0f; 1.0f; 1.0f;  // top-right
            -1f;  1f; 0.0f; 0.0f; 1.0f;  // top-left
        |]
        let indices : uint array = [| 0u; 1u; 3u; 1u; 2u; 3u |]
        let vao = GL.GenVertexArray()
        GL.BindVertexArray(vao)
        let vbo = GL.GenBuffer()
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float32>, vertices, BufferUsageHint.DynamicDraw)
        let ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof<uint>, indices, BufferUsageHint.StaticDraw);
        useProgram p.handle
        let vertexLocation = GL.GetAttribLocation(int p.handle, "aPosition")
        GL.EnableVertexAttribArray(vertexLocation)
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)
        let texCoordLocation = GL.GetAttribLocation(int p.handle, "aTexCoord")
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 3 * sizeof<float32>);
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

    let compute (program : int<program>) =
        GL.UseProgram(int program)
        GL.DispatchCompute(1920, 1080, 1)
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit)

    let prelude (size: Vector2i) =
        GL.Enable(EnableCap.Texture2D)
        GL.Enable(EnableCap.Blend)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
        GL.Enable(EnableCap.DebugOutput)
        GL.Viewport(0, 0, size.X, size.Y)

    let passArray (p: Program) (data: float32 array) =
        useProgram p.handle
        let mutable ssbo: int = 0
        let binding = 0;//Should be equal to the binding specified in the shader code
        GL.GenBuffers(1, &ssbo)
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo)
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Length * sizeof<float32>, data, BufferUsageHint.DynamicRead)
        GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, binding, ssbo)
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0)