namespace Storyboarding.Effects.LSystem

open Storyboarding.Effects.LSystem.LSystem
open Storyboarding.Tools.SbTypes

module Interpreter =
    type LSystemState<'a> = {
        memory: (float32 * float32) list
        depth: int
        current: LSystemExpr
        program: LSystemProgram
        position: Position
        instructions: LSystemChar list
        inner: 'a
    }

    let initialState inner position = fun (program : LSystemProgram) -> {
        memory = []
        depth = 0
        position = position
        program = program
        current = program.axiom
        instructions = ["F"]
        inner = inner
    }

    let draw initialState =
        id

    let step (program: LSystemProgram) (states : LSystemState<'a> ResizeArray) (outerStrat : LSystemState<'a> ResizeArray -> int) =
        let stateIndex = outerStrat states
        let pickedState = states[stateIndex]
        states.RemoveAt(stateIndex)
        let next = pickedState.current |> List.map (fun i -> program.rules[i]) |> List.concat
        states.Add({pickedState with depth = pickedState.depth + 1; current = next})








