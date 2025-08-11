namespace Storyboarding.Effects

open Storyboarding.Effects.SbFFT
open Storyboarding.Tools
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module SquareFading =
    let squareFadeSequence timeStart timeEnd sb =
        let updateFrequency = beatTime (timeStart + 3) sb
        withFft (fun fft ->
        monadicMap [0..9] (fun index ->
        let selfFreq = (float32 index) * 0.1f
        let startPosition = SbRandom.randPosition()
        let endPosition = SbRandom.samePosition() +++ (30, 200)
        img square_white
        >>= move timeStart timeEnd startPosition endPosition
        >>= timeDivisionMap timeStart timeEnd updateFrequency (fun (fadeTs, fadeTe) ->
        fade fadeTs fadeTe (fft fadeTs selfFreq |> (*) 0.02f) (fft fadeTe selfFreq |> (*) 0.02f)))) sb