namespace StoryboardRenderer

open StoryboardRenderer.Renderer
open Storyboarding.Storyboards
open Storyboarding.Tools.SbMonad

module Program =
    let macPath = Continuum.macPath

    [<EntryPoint>]
    let main _ =
        let sb = openSb macPath |> Continuum.story
        use w = new RendererWindow.RendererWindow(sb)
        w.Run()
        0