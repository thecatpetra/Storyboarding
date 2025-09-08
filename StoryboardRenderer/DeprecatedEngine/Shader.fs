module StoryboardRenderer.Engine.Shader

open System
open System.Collections.Generic
open System.IO
open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics

type GlShader =
    struct
        val handle: int
        val uniforms: Dictionary<string, int>
        
        new(vert, frag) =

            let compileShader (shader) source =
                GL.CompileShader(int shader)
                let mutable code: int = 0
                GL.GetShader(int shader, ShaderParameter.CompileStatus, &code)
                if (code <> int All.True) then
                    let infoLog = GL.GetShaderInfoLog(int shader)
                    printfn $"{source}"
                    raise <| Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}")

            let linkProgram (program) =
                GL.LinkProgram(int program);
                let mutable code: int = 0
                GL.GetProgram(int program, GetProgramParameterName.LinkStatus, &code);
                if (code <> (int)All.True) then
                    raise <| Exception($"Error occurred whilst linking Program({program})");

            let shaderSource = File.ReadAllText(vert)
            let vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            compileShader (vertexShader) shaderSource
            let shaderSource = File.ReadAllText(frag)
            let fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            compileShader (fragmentShader) shaderSource
            let handle = GL.CreateProgram();
            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);
            linkProgram (handle)
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
            { handle = handle; uniforms = _uniformLocations }

        member t.Use() =
            GL.UseProgram(t.handle)
    end

