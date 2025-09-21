namespace Storyboarding.Effects.LSystem

open System
open Storyboarding.Effects.LSystem.LSystem
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.GeometryUtils

module Interpreter =
    type LSystemState<'a> =
        { memory: (Position * float32) list
          depth: int
          current: LSystemExpr
          rotation: float32
          program: LSystemProgram
          position: Position
          inner: 'a }

    type LSystemResultChar =
        | Forward of int
        | Left
        | Right
        | Save
        | Load
        | Backward of int

    type LSystemResult = LSystemResultChar list

    type LSystemActions<'a> = LSystemChar -> LSystemState<'a> -> LSystemState<'a>

    let initialState inner position =
        fun (program: LSystemProgram) ->
            { memory = []
              depth = 0
              position = position
              rotation = 0f
              program = program
              current = program.axiom
              inner = inner }

    let resolveExpression (program: LSystemProgram) : LSystemResult =
        let apply x =
            match program.rules.TryGetValue(x) with
            | true, x -> x
            | false, _ -> [ x ]

        let s = program.axiom

        let rec inner n s =
            if n = 0 then s else List.collect apply s |> inner (n - 1)

        let rec optimize acc last state =
            match last, state with
            | Some(Forward x), "F" :: tl -> optimize acc (Some <| Forward(x + 1)) tl
            | Some(Backward x), "B" :: tl -> optimize acc (Some <| Backward(x + 1)) tl
            | Some i, _ -> optimize (i :: acc) None state
            | None, "+" :: tl -> optimize (Left :: acc) None tl
            | None, "-" :: tl -> optimize (Right :: acc) None tl
            | None, "[" :: tl -> optimize (Save :: acc) None tl
            | None, "]" :: tl -> optimize (Load :: acc) None tl
            | None, "F" :: tl -> optimize acc (Forward 1 |> Some) tl
            | None, "B" :: tl -> optimize acc (Backward 1 |> Some) tl
            | None, _ :: tl -> optimize acc None tl
            | _, [] -> acc

        let isInstruction x =
            List.contains x [ "F"; "B"; "+"; "-"; "["; "]" ]

        inner program.iterations s |> List.filter isInstruction |> optimize [] None |> List.rev

    let drawExpression ip ia ts te (program: LSystemProgram) iTime forkTime =
        let s = program.scale
        let fullExpr = resolveExpression program
        let f2i (x, y) = int x, int y
        let clr t =
            let x = (float32 (t - ts) |> fun x -> x / (float32 (te - ts))) |> max 0f |> min 1f
            lerpColor (255, 255, 255) (240, 60, 90) x
        let rec inner mnd m p r t ti i =
            match i, m with
            | Forward n :: tl, _ ->
                let np = p +++ (rotateByF r (s * (float32 n), 0f))
                let mnd = mnd >>= openingLine (t - 1) te (iTime * n * ti) (f2i p) (f2i np) 0.03f
                          >> easing Easing.None
                          >>= fade t t 0.5f 0.5f
                          >>= fade (te - 1000) te 0.5f 0f
                          >>= color t t (clr t) (clr t)
                inner mnd m np r (t + iTime * n * ti) ti tl
            | Backward n :: tl, _ ->
                let np = p --- (rotateByF r (s * (float32 n), 0f))
                inner mnd m np r t ti tl
            | Save :: tl, _ -> inner mnd ((p, r, t, ti) :: m) p r (t + forkTime) (ti + 1) tl
            | Load :: tl, (lp, lr, tm, ti) :: m -> inner mnd m lp lr tm ti tl
            | Load :: _, [] -> failwith "Loading empty mem"
            | Left :: tl, _ -> inner mnd m p (r + program.angle) t ti tl
            | Right :: tl, _ -> inner mnd m p (r - program.angle) t ti tl
            | [], _ -> mnd
        inner id [] ip ia ts 4 fullExpr
        