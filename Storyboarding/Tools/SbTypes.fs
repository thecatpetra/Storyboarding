namespace Storyboarding.Tools

open System
open MapsetParser.objects
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

module SbTypes =
    type Easing =
        | None = 0
        | In = 1
        | Out = 2
        | QuadIn = 3
        | QuadOut = 4
        | QuadInOut = 5
        | QuartIn = 9
        | QuartOut = 10
        | SineIn = 15
        | SineOut = 16
        | SineInOut = 17
        | ExpoIn = 18
        | ExpoOut = 19
        | ExpoInOut = 20
        | BounceIn = 32
        | BounceOut = 33
        | BounceInOut = 34

    type Origin = Center | BottomCentre | TopCentre | TopLeft | CentreLeft | CentreRight

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
        origin: Origin
        difficulty: Beatmap option
        instructions: Instruction list
        x: int
        y: int
        copy: bool
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

    let lerpColor (first : Color) (second : Color) (p : float32) =
        let fr, fg, fb = first
        let sr, sg, sb = second
        lerpTime fr sr p, lerpTime fg sg p, lerpTime fb sb p

    let inline elementWise op first second =
        (second |> fst, first |> fst) ||> op, (second |> snd, first |> snd) ||> op

    let inline (---) x = elementWise (-) x

    let inline (+++) x = elementWise (+) x

    let inline ( *** ) (s: int) ((vx, vy) : Position) = vx * s, vy * s

    let length position =
        let square x = x * x
        let sqrt = float32 >> MathF.Sqrt >> int
        ((position |> fst |> square) + (position |> snd |> square)) |> sqrt

    let angle (x, y) =
        (x |> float32, y |> float32) ||> atan2

    let rotateBy r (x, y) =
        let xNew = (MathF.Cos(r) * (float32 x) - MathF.Sin(r) * (float32 y)) |> int
        let yNew = (MathF.Sin(r) * (float32 x) + MathF.Cos(r) * (float32 y)) |> int
        xNew, yNew

    let rotateAround rotation origin v =
        v +++ origin |> rotateBy rotation |> (+++) origin