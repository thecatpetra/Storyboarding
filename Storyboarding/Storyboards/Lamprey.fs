namespace Storyboarding.Storyboards

open Storyboarding.Tools.Resources
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Effects.SbFFT
open Storyboarding.Tools.SbRandom

module Lamprey =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\beatmap-638755215394306688-Rogue Legacy OST - [12] Lamprey [Ponce de Leon] (Tower Boss)\VGMTracks - Lamprey (TheCatPetra).osb"

    let story =
        withFft (fun fft ->
        randColor() |> ignore
        monadicMap [6..399] (fun freq ->
        img square_white
        >>= move 0 0 (freq * 2, 200) (freq * 2, 200)
        >>= color 0 0 (sameColor()) (sameColor())
        >>= alpha
        >>= monadicMap [200..1000] (fun time ->
        let fftRes = fft (time * 10) ((float32 freq) / 400f) |> (*) 0.02f
        vectorScaleTo ((time - 1) * 10) (time * 10) (0.01f, fftRes)
        >>= fadeTo ((time - 1) * 10) (time * 10) (fftRes * 0.8f))))

    let make () =
        openSb path
        |> story
        |> SbCompiler.write