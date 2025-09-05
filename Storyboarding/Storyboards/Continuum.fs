namespace Storyboarding.Storyboards

open System
open System.Collections.Generic
open Storyboarding.Effects
open Storyboarding.Effects.Background
open Storyboarding.Effects.Growth
open Storyboarding.Storyboards.TheSunTheMoonTheStars
open Storyboarding.Tools
open Storyboarding.Tools.ColorUtils
open Storyboarding.Tools.GeometryUtils
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.TextUtils
open System.IO

// It has become a custom for storyboarders to add source code
// Of the given map as a representation of some "code"
// I will not deviate
module Continuum =
    let path = @"D:\D\osu!\Songs\2423610 Kardashev - Continuum\Kardashev - Continuum (me2u).osb"

    let white = fun _ -> (240, 240, 255)
    let just color = indexedSolid color 10000 |> toFun
    let black = fun _ -> (2, 4, 10)
    let red x = indexedGradientT (255, 40, 80) (255, 80, 40) x |> toFun
    
    let mainfont = font_forum

    let renderLyrics1 =
        let lyrics = [
            t "00:34:918", "Ignorance", Some(96, 115), t "00:47:463", 0.3f, white
            t "00:37:100", "Has collapsed", Some(96, 140), t "00:47:463", 0.3f, white
            t "00:39:281", "Resources are", Some(96, 190), t "00:47:463", 0.3f, white
            t "00:41:190", "Redistributed", Some (96, 215), t "00:47:463", 0.3f, white
            t "00:44:463", "To the development of", Some (96, 265), t "00:47:463", 0.3f, white
            t "00:46:099", "Machinery", Some (96, 290), t "00:47:463", 0.3f, red "Machinery"
            t "00:47:463", "Autonomous self-replication", None, t "00:50:463", 0.4f, red "Autonomous self-replication"
        ]
        let line (ts, txt, position, te, s, icf) =
            let effect = spinInOutMove (te - ts) 35 ts icf
            match position with
            | None -> textCenter mainfont txt effect s
            | Some p -> text mainfont txt effect p s
        monadicMap lyrics line
        >>= openingLine (t "00:34:918") (t "00:47:463") 800 (90, 105) (90, 150) 0.04f
        >>= openingLine (t "00:39:281") (t "00:47:463") 800 (90, 180) (90, 225) 0.04f
        >>= openingLine (t "00:44:463") (t "00:47:463") 800 (90, 255) (90, 300) 0.04f

    let renderLyrics2 =
        let lyrics = [
            t "00:52:917", "Propagation of stigma", white, 115, 0
            t "00:55:644", "And infinite traits", white, 140, -2
            t "00:59:463", "Evolutionary algorithm", red "Evolutionary algorithm", 165, 0
        ]
        let line (ts, l, icf, h, w) =
            let te = t "00:62:463"
            let effect = spinInOutMove (te - ts) 35 ts icf
            let w = textWidth mainfont l 0.3f |> (-) 550f |> int |> (+) w
            text mainfont l effect (w, h) 0.3f
        monadicMap lyrics line
        >>= openingLine (t "00:52:917") (t "00:62:917") 800 (552, 107) (552, 175) 0.04f

    let renderLyrics3 ts =
        let te = ts + (t "01:14:735") + 3000 - (t "01:06:281")
        let tm = ts - t "01:06:281"
        let lyrics = [
            t "01:06:281" + tm, "This process will mirror", -3, white
            t "01:09:826" + tm, "Natural concepts", -1, white
            t "01:12:830" + tm, "While guided by", 1, white
            t "01:14:735" + tm, "Pre-written code", 3, red "Pre-written code"
        ]
        let line (ts, l, h, icf) =
            let effect = spinInOutMove (te - ts) 35 ts icf
            let w = textWidth mainfont l 0.3f |> (/) 2f |> int |> (+) 320
            text mainfont l effect (w, 240 + 13 * h) 0.3f
        monadicMap lyrics line

    let renderCode ts te =
        let source = @"C:\Users\arthu\Documents\Storyboarding\Storyboarding\Resources\code\self replication.fs_nolint"
        let colors = @"C:\Users\arthu\Documents\Storyboarding\Storyboarding\Resources\code\self replication.colors.fs_nolint"
        // ['1', (30, 60, 120); '3', (40, 120, 30); '2', (90, 50, 80); '4', (90, 50, 128); '5', (64, 64, 64)]
        let icf = icfOfFile colors (dict ['1', (92, 169, 247)
                                          '3', (85, 230, 138)
                                          '2', (252, 93, 194)
                                          '4', (195, 122, 255)
                                          '5', (204, 212, 219)]) (209, 209, 209) |> toFun
        let lyrics = File.ReadAllLines(source) |> Seq.toList
        let fontDir = @"C:\Users\arthu\Documents\Storyboarding\Storyboarding\Resources\font\JetBrainsMono-Light.ttf\"
        let symbols = Directory.GetFiles(fontDir)
                      |> Seq.map (_.Replace(@"C:\Users\arthu\Documents\Storyboarding\Storyboarding\Resources\", @""))
                      |> Seq.rev |> Seq.take 130
                      |> Seq.rev |> Seq.take 100
        let mutable collectedLength = 0
        let withCollectedLength icf _ =
            let r = icf collectedLength
            collectedLength <- collectedLength + 1
            r
        let effect diff time timeEnd icf : CharAction = fun i image s p ->
            let effectTime = 200
            let glitchTime = randInt (time + 300) (timeEnd - 300)
            let glitchEnd = 300 + glitchTime
            let c = icf i
            img image >> coords p
            >>= scale (time + i * diff) (time + effectTime + i * diff) 0f s
            >> easing Easing.QuadOut
            >>= color time time c c
            >>= fade (glitchTime) (glitchTime + 10) 1f 0f
            >>= scale (timeEnd + i * diff / 2) (timeEnd + effectTime + i * diff / 2) s 0f
            >> easing Easing.QuadOut
            >>= fade glitchTime (glitchTime + 10) 1f 0f
            >>= fade glitchEnd (glitchEnd + 10) 0f 1f
            >>= img (choice (symbols |> Seq.toList)) >> coords p
            >>= scale glitchTime glitchEnd s s
            >>= color glitchTime glitchTime (170, 10, 30) (110, 10, 30)
        let line i txt =
            let effect = effect 35 (ts + 500 * i) te (withCollectedLength icf)
            text font_jetbrains_mono txt effect (90, 168 + i * 15) 0.15f
        monadicMapi lyrics line
        >>= openingLine (t "00:52:917") (t "00:62:917") 800 (552, 107) (552, 175) 0.04f

    let thisProcessWillMirror =
        Transition.closingSquares (t "01:03:917") (t "01:05:735") 1000 (bg_snow_mountain |> applyShader "colorswap_br.frag") (192, 192, 200)
        >> img (bg_fern |> applyShader "colorswap_gr.frag")
        >>= scale (t "01:05:735") (t "01:31:917") 0.45f 0.45f
        >>= rotate (t "01:05:735") (t "01:31:917") -0.1f 0.1f
        >>= growingVineBreadth (t "01:05:735") (t "01:31:917") (ResizeArray<Segment>()) (675, 430) (140, 120, 80)
        >>= bgMovement (t "01:05:735") (t "01:31:917")
        >>= renderLyrics3 (t "01:06:281")
        >>= renderLyrics3 (t "01:19:372")
        >>= renderCode (t "01:18:826") (t "01:31:099")
        >>= AscendingParticles.fastEffect (t "01:18:826") (t "01:31:099") dot false (lerpColor (150, 80, 10) (250, 160, 20))
        >>= Transition.threeShuttingSquares (t "01:31:099") 400
        // TODO: Cool effects all over the screen (localized)

    let initiateTheProcess =
        let noEffectWithLoading j stay time diff icf : CharAction = fun i image s p ->
            let endTime = time + stay
            let time, c = time + i * diff, icf i
            let folder = image.Replace(FileInfo(image).Name, "")
            if image.EndsWith("\\43.png") then
                let switchTime = time + ((i - 28) * 60) + j * 600
                img (folder + "/45.png")
                >>= move time time p p
                >>= scale time time s s
                >>= color time time c c
                >>= fade time switchTime 1f 1f
                >> img (folder + "/61.png")
                >>= move switchTime switchTime p p
                >>= scale switchTime switchTime s s
                >>= color switchTime switchTime (217, 213, 132) (217, 213, 132)
                >>= fade switchTime (endTime) 1f 1f
            else
                img image
                >>= move time time p p
                >>= scale time time s s
                >>= color time time c c
                >>= fade time (endTime) 1f 1f
        let firstCmd = [
            t "01:31:917", "./initiate ", just (255, 193, 122)
            t "01:33:281", "\"TheSynthesis.fs\" ", just (227, 245, 255)
            t "01:34:917", "--of ", just (255, 193, 122)
            t "01:35:190", "\"FLASH.dat\" ", just (227, 245, 255)
            t "01:36:826", "--and ", just (255, 193, 122)
            t "01:37:126", "\"TECHNOLOGY.dat\"", just (227, 245, 255)
        ]
        let logFile = @"C:\Users\arthu\Documents\Storyboarding\Storyboarding\Resources\code\self replication log.txt"
        let colors = dict ['0', (255, 193, 122); '1', (227, 245, 255); '2', (118, 227, 149); '3', (162, 168, 171); '4', (217, 213, 132); '5', (170, 30, 40)]
        let log = textAndIcfsOfFile logFile colors (114, 120, 122)
        let ts, te = t "01:31:917", t "01:47:190"
        let mutable stride = 0
        let line i (ts, txt, icf) =
            let effect = noEffectWithLoading 0 (te - ts) ts 80 icf
            let r = text font_jetbrains_mono txt effect (48 + stride, 80) 0.2f
            stride <- stride + (int <| textWidth font_jetbrains_mono txt 0.2f); r
        let logLine i (txt, icf) =
            let ts = (t "01:39:281") + i * 130
            let effect = noEffectWithLoading (i - 10) (te - ts) ts 1 icf
            text font_jetbrains_mono txt effect (48, 100 + 20 * i) 0.2f
        let i = bg_snow_mountain |> applyShader "colorswap_br.frag"
        background i ts (t "01:47:190")
        >>= monadicMapi firstCmd line
        >>= monadicMapi log logLine
        >>= backgroundRaw screen_overlay ts (t "01:47:190")
        >>= fade ts (t "01:47:190") 0.3f 0.3f
        >>= Transition.closingSquares (t "01:45:281") (t "01:47:190") 1000 i (192, 192, 200)

    let firstPart =
        background bg_snow_mountain 0 (t "00:52:644")
        >>= bgMovementSlow 0 (t "00:52:644")
        >>= renderLyrics1
        >>= growingVineBreadth (t "00:47:463") (t "00:51:008") (ResizeArray<Segment>()) (320, 240) (255, 180, 160)
        >>= Transition.blackCurtains (t "00:50:644") (t "00:52:644") (t "00:52:644") (t "00:54:735")
        >> background (bg_snow_mountain |> applyShader "colorswap_br.frag") (t "00:52:644")  (t "01:05:735")
        >>= bgMovementSlow (t "00:52:644") (t "01:05:735")
        >>= renderLyrics2
        >>= growingVineBreadth (t "00:59:190") (t "01:04:099") (ResizeArray<Segment>()) (456, 162) (255, 180, 160)
        >>= RealisticSnow.effect 0 (t "01:04:735")

    let theTheoryOfInfiniteLifelessness =
        background bg_snow_mountain (t "01:47:190") (t "02:35:188")
        >>= bgMovementSlow (t "01:47:190") (t "02:35:188")
        >>= monadicMap [0..1] (fun _ -> RealisticSnow.effect (t "01:42:281") (t "02:35:188"))

    let story =
        removeBg
        >>= firstPart
        >>= thisProcessWillMirror
        >>= theTheoryOfInfiniteLifelessness
        >>= initiateTheProcess
        >>= background vignette 0 (t "03:54:397") >> layer Foreground
        >>= fractalLSystem (600f, 480f) (-MathF.PI / 2f - 1.5f) 0 20000 @"C:\Users\arthu\Documents\Storyboarding\Storyboarding\Resources\code\fractal_tree2.lsystem"
        

    let make () = openSb path |> story |> SbCompiler.write

