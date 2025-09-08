module StoryboardRenderer.Engine.Quad

open System
open StoryboardRenderer.Engine.Position
open StoryboardRenderer.Engine.Shader
open StoryboardRenderer.Engine.Texture
open Storyboarding.Tools.SbTypes

type Quad =
    struct
        val texture: GLTexture
        val program: GlShader
        val vertices: float32 array
        val indices: uint array
        val mutable position: GlPosition
        val mutable rotation: float32
        val mutable scale: GlPosition
        val mutable relAngles: float32 array
        val mutable usedVertices: float32 array
        
        new (texture, program) =
            let vertices : float32 array = [|
                -1f; -1f; 0.0f; 0.0f; 0.0f;  // bottom-left
                 1f; -1f; 0.0f; 1.0f; 0.0f;  // bottom-right
                 1f;  1f; 0.0f; 1.0f; 1.0f;  // top-right
                -1f;  1f; 0.0f; 0.0f; 1.0f;  // top-left
            |]
            let indices = [| 0u; 1u; 3u; 1u; 2u; 3u |]
            { texture = texture
              program = program
              vertices = vertices
              indices = indices
              position = GlPosition()
              rotation = 0f
              scale = GlPosition()
              relAngles = [| 0f; MathF.PI / 2f; MathF.PI; 3f * MathF.PI / 2f |] 
              usedVertices = vertices.Clone() :?> float32 array }
        
        member x.Width = x.scale.x
        member x.Height = x.scale.y
        
        member x.Refresh() =
            for i = 0 to 19 do x.usedVertices[i] <- x.vertices[i]
        
        member x.ScaleTo(scale: fPosition) =
            x.scale <- GlPosition(scale)
        
        member x.MoveTo(position : fPosition) =
            x.position <- GlPosition(position)
        
        member x.RotateTo(angle: float32) =
            let rad = MathF.Sqrt(x.Width * x.Width + x.Height * x.Height) / 2f
            let ab = GlPosition(x.Height, x.Width);
            let ac = GlPosition(x.Height, -x.Width);

            x.relAngles[0] <- MathF.Atan2(ab.x, ab.y);
            x.relAngles[1] <- MathF.Atan2(ac.x, ac.y);
            x.relAngles[2] <- MathF.Atan2(ab.x, ab.y) + MathF.PI;
            x.relAngles[3] <- MathF.Atan2(ac.x, ac.y) + MathF.PI;

            let transposed = x.position;

            let angle = MathF.PI - angle
            x.usedVertices[0] <- transposed.x + rad * MathF.Cos(angle + x.relAngles[2]);
            x.usedVertices[1] <- transposed.y + rad * MathF.Sin(angle + x.relAngles[2]);
            x.usedVertices[5] <- transposed.x + rad * MathF.Cos(angle + x.relAngles[1]);
            x.usedVertices[6] <- transposed.y + rad * MathF.Sin(angle + x.relAngles[1]);
            x.usedVertices[10] <- transposed.x + rad * MathF.Cos(angle + x.relAngles[0]);
            x.usedVertices[11] <- transposed.y + rad * MathF.Sin(angle + x.relAngles[0]);
            x.usedVertices[15] <- transposed.x + rad * MathF.Cos(angle + x.relAngles[3]);
            x.usedVertices[16] <- transposed.y + rad * MathF.Sin(angle + x.relAngles[3]);
    end
    
    
type GlQuad(texture: string) =
    let inner = Quad()
    