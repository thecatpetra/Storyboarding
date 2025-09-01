namespace GpuDrawingUtils

open OpenTK.Graphics.OpenGL
open OpenTK.Mathematics
open OpenTK.Windowing.Common
open OpenTK.Windowing.Desktop

module FakeWindow =

    type FakeWindow<'a>(gameWindowSettings, nativeWindowSettings, onLoad, onRender, size) =
        inherit GameWindow(gameWindowSettings, nativeWindowSettings)
        let mutable onLoadResult: 'a Option = None

        override win.OnRenderFrame(args) =
            base.OnRenderFrame(args)
            GL.Clear(ClearBufferMask.ColorBufferBit)
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f)
            onRender win onLoadResult.Value
            base.SwapBuffers()

        override win.OnLoad() =
            base.OnLoad()
            onLoadResult <- Some <| onLoad win

    let createWindow<'a> f g size =
        let nativeWindowSettings = NativeWindowSettings()
        nativeWindowSettings.ClientSize <- Vector2i(100, 100)
        nativeWindowSettings.Title <- "WINDOW FOR RENDERING"
        nativeWindowSettings.Flags <- ContextFlags.ForwardCompatible
        new FakeWindow<'a>(GameWindowSettings.Default, nativeWindowSettings, f, g, size)

    let withFakeWindow<'a> size (onLoad : FakeWindow<'a> -> 'a) (onRender : FakeWindow<'a> -> 'a -> unit) =
        let win = createWindow onLoad onRender size
        win.Run()
