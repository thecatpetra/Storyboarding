namespace Storyboarding.Storyboards

open GpuDrawingUtils.SliderRenderer
open Storyboarding.Effects
open Storyboarding.Effects.Background
open Storyboarding.Gameplay.UI
open Storyboarding.Tools
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.TextUtils

module TheSunTheMoonTheStars =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2411715 Aether Realm - The Sun, The Moon, The Star\Aether Realm - The Sun, The Moon, The Star (goblinlit).osb"
    let parts = ["0:0-0:14 kailigan"
                 "0:14-0:28 zoonari + jacques VST"
                 "0:28-1:00 goblin + jacques VST)"
                 "1:00-1:12 kimchi"
                 "1:39-2:19 kai + Renge Fuwa"
                 "2:19-6:16 kai"
                 "6:16-7:03 goblinlit"
                 "7:04-7:08 lucerman"
                 "7:09-7:39 goblinlit"
                 "7:40-8:33 goblinlit"
                 "8:33-9:01 goblinlit"
                 "9:01-9:55 goblinlit"
                 "9:55-10:48 koi"
                 "11:02-11:42 jacques"
                 "11:42-12:02 sha1"
                 "12:42-13:10 pelmshek"
                 "13:11-13:46 goblinlit (wip)"
                 "13:48-14:14 goblinlit"
                 "14:14-14:50 thecatpetra"
                 "14:51-16:00 koi"
                 "16:00-17:09 goblinlit"
                 "17:09-17:43 pelmeshek"]

    let mapperNames =
        let effect ts te = fun i image s p ->
            let timeStart, timeEnd = ts + i * 100, te + i * 100
            img image >> layer Foreground
            >>= move (timeStart - 350) timeStart (p +++ (0, 20)) p >> easing Easing.QuadOut
            >>= fade (timeStart - 350) timeStart 0f 1f  >> easing Easing.QuadOut
            >>= move (timeEnd - 350) timeEnd p (p +++ (0, -20)) >> easing Easing.QuadOut
            >>= fade (timeEnd - 350) timeEnd 1f 0f >> easing Easing.QuadOut
            >> scale timeStart timeStart s s
        let line txt ts te = text font_quicksand txt (effect ts te) (460, 462) 0.2f
        monadicMap parts (fun (p : string) ->
        let timeSpan = p.Split(" ")[0]
        let content = p.Replace(timeSpan, "")
        let [|timeStart; timeEnd|] = timeSpan.Split("-")
        let [|timeStart; timeEnd|] = [|timeStart; timeEnd|] |> Array.map (fun e -> t $"{e}:0")
        line content timeStart timeEnd)

    let story =
        background bg_sunmoonstars 0 (t "19:10:055")
        >>= bgMovementSlow 0 (t "19:10:055")
        >>= mapperNames
        >>= playingStatus (247, 166, 243) (t "19:10:055")
        >>= timer (t "19:10:055")
        >>= background vignette 0 (t "19:10:055")

    let make () =
        openSb path
        |> story
        |> SbCompiler.write

