namespace Storyboarding.Tools

open System
open MapsetParser.objects
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

module SbTypes =
    type Easing =
        | None = 0
        | In = 1
        | Out = 2
        | InOut = 3
        | QuartIn = 9
        | QuartOut = 10
        | SineIn = 15
        | SineOut = 16
        | SineInOut = 17

    type Layer = Background | Foreground | Overlay

    type InstructionType =
        | Move
        | MoveX
        | MoveY
        | Scale
        | Rotate
        | VectorScale
        | Parameter
        | Fade
        | Color
        | Blur
        | GrayScale

    type Time = int

    type Instruction = {
        typ: InstructionType
        easing: Easing
        timeStart: Time
        timeEnd: Time
        iFrom: obj
        iTo: obj
    }

    type Sprite = {
        name: string
        layer: Layer
        difficulty: Beatmap option
        instructions: Instruction list
    }

    type Position = int * int
    type Color = int * int * int
    type fPosition = float32 * float32

    let lerp (first: Position) (second: Position) (p : float32) =
        let fx, fy = first
        let sx, sy = second
        ((float32 fx) * (1f - p) + (float32 sx) * p) |> int, ((float32 fy) * (1f - p) + (float32 sy) * p) |> int

    let lerpTime (first: int) (second: int) (p : float32) =
        ((float32 first) * (1f - p) + (float32 second) * p) |> int

    let inline elementWise op first second =
        (second |> fst, first |> fst) ||> op, (second |> snd, first |> snd) ||> op

    let inline (---) x = elementWise (-) x

    let inline (+++) x = elementWise (+) x

    let inline ( *** ) s (vx, vy) = vx * s, vy * s

    let length position =
        let square x = x * x
        let sqrt = float32 >> MathF.Sqrt >> int
        ((position |> fst |> square) + (position |> snd |> square)) |> sqrt
