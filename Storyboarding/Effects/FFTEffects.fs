namespace Storyboarding.Effects

open System
open Storyboarding.Effects.SbFFT
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes

module FFTEffects =
    let positionOnCircle radius elements (position : Position) index =
        let rotation = (float32 index) / (float32 elements) * 2f * MathF.PI
        let relative = ((MathF.Cos rotation) * radius) |> int, ((MathF.Sin rotation) * radius) |> int
        let p = position +++ relative
        p, rotation

    let circleFft timeStart timeEnd =
        let timeStep = 66
        let black_circle_predone =
            img black_circle
            >>= move timeStart timeStart (320, 240) (320, 240)
            >>= scale timeStart timeStart 0.385f 0.385f
        black_circle_predone
        >>= fade timeEnd timeEnd 1f 0f
        >>= withFft (fun fft ->
        monadicMap [4..29] (fun freq ->
        let position, rotation = positionOnCircle 100f 25 (320, 240) freq
        let c = lerpColor (242, 78, 63) (242, 255, 105) ((float32 freq - 4f) / 25f)
        img light_squashed
        >>= move timeStart timeStart position position
        >>= rotate timeStart timeStart rotation rotation
        >>= color timeStart timeStart c c
        >>= alpha
        >>= timeDivisionMap timeStart timeEnd timeStep (fun (time, endTime) ->
        // Adjust these
        let freqRequest = (float32 freq) / 60f
                          |> fun x -> MathF.Pow(x, 3f)
        let fftRes = fft time freqRequest
                     |> (+) -1.4f
                     |> (*) 0.0081f
                     |> fun x -> MathF.Pow(x, 2f)
        scaleTo time endTime fftRes
        >>= fadeTo time endTime (fftRes * 1.2f))))
        >> black_circle_predone
        >>= fade timeEnd timeEnd 0.6f 0f

    let bottomFft timeStart timeEnd =
        let timeStep = 66
        withFft (fun fft ->
        monadicMap [1..50] (fun freq ->
        let position = (320 + (freq - 25) * 20, 480)
        let c = lerpColor (89, 111, 255) (240, 62, 116) ((float32 freq) / 50f)
        img square_white >> origin BottomCentre
        >>= move timeStart timeStart position position
        >>= color timeStart timeStart c c
        >>= alpha
        >>= timeDivisionMap timeStart timeEnd timeStep (fun (time, endTime) ->
        // Adjust these
        let freqRequest = (float32 freq + 30f) / 120f
                          |> fun x -> MathF.Pow(x, 3f)
        let fftRes = fft time freqRequest
                     |> (+) -1.4f
                     |> (*) 0.0081f
                     |> fun x -> MathF.Pow(x, 2f)
        vectorScaleTo time endTime (0.15f, fftRes)
        >>= fadeTo time endTime (fftRes * 1.0f))))