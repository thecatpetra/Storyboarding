namespace Storyboarding.Effects.LSystem

open System.Collections.Generic
open Storyboarding.Tools.SbTypes

module LSystem =
    type LSystemExpr = string list
    type LSystemChar = string

    type LSystemProgram = {
        axiom: LSystemExpr
        rules: IDictionary<LSystemChar, LSystemExpr>
        distance: float32
        angle: float32
    }
