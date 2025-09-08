module StoryboardRenderer.Renderer.RendererWindow

open LadaEngine.Engine.Global
open OpenTK.Graphics.OpenGL4
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.Desktop

type RendererWindow(opts1, opts2, sb) =
    inherit Window(opts1, opts2)
    
    let mutable time = 0.
    let storyboard = RdrStoryboard(sb)
    
    let render () =
        storyboard.Render(time)
    
    new(sb) =
        let nativeWindowSettings = NativeWindowSettings()
        nativeWindowSettings.ClientSize <- Vector2i(800, 600)
        nativeWindowSettings.Title <- "SB Preview"
        nativeWindowSettings.Flags <- ContextFlags.ForwardCompatible
        new RendererWindow(GameWindowSettings.Default, nativeWindowSettings, sb)

    override this.OnLoad() = base.OnLoad()
    
    override this.OnRenderFrame(args) =
        time <- time + args.Time
        render()
        base.OnRenderFrame(args)
