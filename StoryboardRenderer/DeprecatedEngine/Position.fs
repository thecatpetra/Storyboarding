module StoryboardRenderer.Engine.Position

open Storyboarding.Tools.SbTypes

type GlPosition =
    struct
        val x: float32
        val y: float32
        
        new(x, y) = {x = x; y = y}
        new(x) = {x = x; y = x}
        
        new((x, y) : fPosition) = {x = x; y = y}
    end