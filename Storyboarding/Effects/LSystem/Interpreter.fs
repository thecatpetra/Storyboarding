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

        inner 15 s |> List.filter isInstruction |> optimize [] None |> List.rev

    let drawExpression ip ia ts te (program: LSystemProgram) =
        let scale = 2.7f
        let forkTime = 1000
        let iTime = 300
        let fullExpr = resolveExpression program
        let f2i (x, y) = int x, int y
        let rec inner m p r t i =
            match i, m with
            | Forward n :: tl, _ ->
                let np = p +++ (rotateByF r (scale * (float32 n), 0f))
                openingLine t te (iTime) (f2i p) (f2i np) 0.03f >> easing Easing.None
                >>= inner m np r (t + iTime) tl
            | Backward n :: tl, _ ->
                let np = p --- (rotateByF r (scale * (float32 n), 0f))
                inner m np r t tl
            | Save :: tl, _ -> inner ((p, r, t) :: m) p r (t + forkTime) tl
            | Load :: tl, (lp, lr, tm) :: m -> inner m lp lr tm tl
            | Load :: _, [] -> failwith "Loading empty mem"
            | Left :: tl, _ -> inner m p (r + program.angle) t tl
            | Right :: tl, _ -> inner m p (r - program.angle) t tl
            | [], _ -> id

        inner [] ip ia ts fullExpr
