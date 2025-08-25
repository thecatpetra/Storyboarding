namespace Storyboarding.Storyboards

open System
open Storyboarding.Effects
open Storyboarding.Effects.Background
open Storyboarding.Effects.Cube
open Storyboarding.Effects.FFTEffects
open Storyboarding.Effects.PerNoteEffect
open Storyboarding.Tools
open Storyboarding.Tools.ColorUtils
open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.TextUtils

// This one for me <3
module SidetrackedDay =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2422753 VINXIS - Sidetracked Day\VINXIS - Sidetracked Day (TheCatPetra).osb"

    let black = 0, 0, 0
    let darkest = 1, 23, 33
    // let dark = 61, 60, 65
    // let first = 152, 68, 63
    // let second = 217, 87, 63
    // let third = 251, 169, 122
    // let fourth = 248, 250, 146
    let dark = 1, 29, 74
    let first = 3, 50, 133
    let second = 62, 125, 231
    let third = 245, 154, 175
    let fourth = 253, 235, 234

    let pickedGradient n =
        let t = n / 3
        indexedGradient first second t
        >>>> indexedGradient second third t
        >>>> indexedGradient third fourth (n - 2 * t)
        |> toFun

    let opening ts te =
        let shapes =
            monadicMap [1..72] (fun td ->
            let ts = ts + td * 500
            let te = ts + 5000
            let fp = SbRandom.randPosition()
            let r = SbRandom.randInt 0 5
            let direction = (10 + 3 * r, -120 + 8 * r)
            let tp = fp +++ direction
            let rotation = SbRandom.randFloat()
            let shape = SbRandom.choice [shape_triangle; shape_cross]
            let topColor = SbRandom.choice [second; third; fourth]
            monadicMap [0f, (90, 90, 100); 1f, (111, 107, 125); 2f, (124, 119, 140); 3f, topColor] (fun (s, c) ->
            let c = ((1f / 75f) * (float32 td)) |> lerpColor black c
            let m = 1f + s * 0.006f
            let s = 0.5f + s * 0.01f
            let withParallax p m =
                (p +++ (-320, -240))
                |> fun (x, y) -> (float32 x |> (*) m |> int, float32 y |> (*) m |> int)
                |> (+++) (320, 240)
            img (shape |> resize1To 64)
            >>= move ts te (withParallax fp m) (withParallax tp m)
            >>= color ts te c c
            >>= rotate ts te 0.1f (0.7f * rotation)
            >>= fade ts (lerpTime ts te 0.2f) 0f 1f
            >>= fade (lerpTime ts te 0.8f) te 1f 0f
            >>= scale ts ts s s))
        background (square_white |> oneTenthFhd) ts te
        >>= color ts te black dark
        >>= shapes

    let signature =
        let gray = (160, 170, 190)
        let icf1 = indexedGradient first second 6 >>>> indexedSolid gray 3 >>>> indexedGradient second third 15 |> toFun
        let icf2 = indexedSolid gray 10 >>>> indexedGradient third fourth 4 >>>> indexedSolid gray 3 >>>> indexedGradient (107, 242, 202) (152, 213, 250) 11 |> toFun
        let icf3 = indexedSolid gray 14 >>>> indexedGradient (107, 242, 202) (152, 213, 250) 11 |> toFun
        let icf4 = indexedSolid gray 13 >>>> indexedGradient (193, 29, 48) (56, 79, 71) 8 |> toFun
        textCenter font_quicksand_bold "VINXIS - Sidetracked day" (flyInOut (t "00:07:819") icf1 3000) 0.4f
        >>= textCenter font_quicksand_bold "Mapped by Ziny & TheCatPetra" (flyInOut (t "00:18:032") icf2 3000) 0.4f
        >>= textCenter font_quicksand_bold "Storyboard by TheCatPetra" (flyInOut (t "00:28:245") icf3 3000) 0.4f
        >>= textCenter font_quicksand_bold "Hitsounds by ilyuchik" (flyInOut (t "00:38:457") icf4 3000) 0.4f

    let greatBreak =
        let gray = (160, 170, 190)
        let icf1 = indexedSolidT gray "You did " >>>> indexedGradient second third 5 >>>> indexedSolid gray 1 |> toFun
        let icf2 = indexedSolidT gray "Hopefully" >>>> indexedSolid gray 3 |> toFun
        let icf3 = indexedGradientT first second "Time to move on" >>>> indexedSolid gray 3 |> toFun
        let icf4 = indexedSolidT gray "This " >>>> indexedGradientT first second "slider" >>>> indexedSolidT gray " is cool ngl" |> toFun
        textCenter font_quicksand_bold "You did great!" (flyInOut (t "02:50:585") icf1 3000) 0.4f
        >>= textCenter font_quicksand_bold "Hopefully..." (flyInOut (t "03:00:798") icf2 3000) 0.4f
        >>= textCenter font_quicksand_bold "Time to move on..." (flyInOut (t "03:09:096") icf3 3000) 0.4f
        >>= textCenter font_quicksand_bold "This slider is cool ngl" (flyInOut (t "03:15:479") icf4 3000) 0.4f

    let rec middleTriangle i prevPosition position fDir ts te =
        let drawTriangleAt prevPosition position c ts te =
            // let ts, te = (t "00:41:011"), (t "01:47:394")
            let fadeAfter = ((position |> fst |> (+) (-320)) + (position |> snd |> (+) (-240) |> (*) 2))
            img shape_triangle
            >>= move ts (ts + 300) prevPosition position >> easing Easing.In
            >>= scale ts ts 0.2f 0.2f
            >>= rotate ts ts 0.04f 0.04f
            >>= color ts te c c
            >>= fade ts (ts + 300) 0f 1f
            >>= fade (te + fadeAfter) (te + fadeAfter + 300) 1f 0f
            >>= alpha
        let color = [first; second; third; fourth] |> List.item i
        let positions = [(65, -14); (-65, -14); (0, 104)] |> List.map (rotateAround 0.04f (0, 0))
        let diff = (t "00:41:489") - (t "00:41:011")
        let flipIndex = if i % 2 = 1 then 1 else -1
        let m r (x, y) = ((x |> float32) * r |> int), ((y |> float32) * r |> int)
        drawTriangleAt prevPosition position color ts te >>= (i % 2 = 0 >?= verticalFlip) >>=
        if i = 1 then id
        else monadicMapi positions (fun ii x ->
        let newPosition = position +++ (flipIndex *** x |> (m 0.4f))
        let prevPosition = lerp position newPosition (0.9f)
        (ii <> fDir) >?= middleTriangle (i - 1) prevPosition newPosition ii (ts + diff) te)

    let dopDopDop time =
        let icf = indexedGradientT fourth third "Dop Dop Dop" |> toFun
        let width = textWidth font_quicksand_bold "Dop Dop Dop" 0.4f |> fun w -> (- w |> int) / 2
        middleTriangle 3 (320, 240) (320, 240) -1 time (time + 1300)
        >>= text font_quicksand_bold "Dop Dop Dop" (spinInOutMove 860 35 time icf) (width + 320, 330) 0.4f

    let dopDopDopx3 ts =
        let center = (320, 240)
        let te = ts + 4000
        let diff = (t "00:41:489") - (t "00:41:011")
        let dopDiff = (t "03:56:330") - (t "03:55:053")
        let width = textWidth font_quicksand_bold "Dop Dop Dop" 0.4f |> fun w -> (- w |> int) / 2
        monadicMapi [(first, second); (second, third); (third, fourth)] (fun i (c1, c2) ->
        let ts = ts + i * dopDiff
        let m = i |> float32 |> (*) 0.25f |> (-) 1.2f
        let icf = indexedGradientT c1 c2 "Dop Dop Dop" |> toFun
        monadicMap [0; 1; 2] (fun index ->
            let ts = ts + index * diff
            let rotation = index |> float32 |> (*) (MathF.PI * 0.66666666666f)
            let p = center +++ ((0, 30 - 7 * i) |> rotateBy rotation)
            img shape_line >> coords p
            >>= vectorScale ts (ts + 130) (0f, 0.3f) (0.3f * m, 0.3f)
            >>= rotate ts ts rotation rotation
            >>= fade (te - 300) te 1f 0f
            >>= color ts ts c1 c1
            >>= alpha
        ) >>= text font_quicksand_bold "Dop Dop Dop" (spinInOutMove 860 35 ts icf) (width + 320, 330) 0.4f)

    let squares time =
        let timeEnd = 1300 + time
        let zipLooped c = (c, List.tail c @ [List.head c]) ||> List.zip
        let positions = [(1, -1); (1, 1); (-1, 1); (-1, -1)] |> List.map (( *** ) 23 >> rotateBy -0.04f >> (+++) (320, 240)) |> zipLooped
        let colors = [first; second; third; fourth]
        monadicMapi (List.zip positions colors) (fun i ((pPrev, pNext), col) ->
        img shape_square
        >>= move (time + 319 * i) (time + 319 * i + 300) (lerp pPrev pNext 0.9f) pNext
        >>= color (time + 319 * i) (time + 319 * i) col col
        >>= fade (time + 319 * i) (time + 319 * i + 150) 0f 1f
        >>= fade (timeEnd) (timeEnd + 50 * i + 300) 1f 0f
        >>= rotate (time + 319 * i) (time + 319 * i) -0.035f -0.035f
        >>= scale (time + 319 * i) (time + 319 * i) 0.2f 0.2f
        >>= alpha)

    let squares2 time =
        let timeEnd = 700 + time
        monadicMapi ([0.2f, second; 0.3f, third]) (fun i (s, col) ->
        img shape_square
        >>= move (time + 319 * i) (time + 319 * i) (320, 240) (320, 240)
        >>= color (time + 319 * i) (time + 319 * i) col col
        >>= fade (time + 319 * i) (time + 319 * i + 150) 0f 1f
        >>= fade (timeEnd) (timeEnd + 50 * i + 300) 1f 0f
        >>= rotate (time + 319 * i) (time + 319 * i) -0.04f -0.04f
        >>= scale (time + 319 * i) (time + 319 * i) s s
        >>= alpha)

    let squares3 ts =
        let center = (320, 240)
        let te = ts + 1400
        let diff = (t "00:41:489") - (t "00:41:011")
        monadicMapi [second; third; fourth] (fun index c ->
            let ts = ts + diff * index
            let rotation = index |> float32 |> (*) (MathF.PI * 0.66666666f)
            let p = center +++ ((0, -40) |> rotateBy rotation)
            img shape_triangle >> coords p
            >>= scale ts (ts + 130) 0.2f 0.25f
            >>= fade ts (ts + 130) 0f 1f
            >>= fade (te - 300) te 1f 0f
            >>= color ts ts c c
            >>= alpha
        )

    let yaYaYaYa time =
        let icf = indexedGradientT fourth third "Ya Ya Ya Ya" |> toFun
        let width = textWidth font_quicksand_bold "Ya Ya Ya Ya" 0.4f |> fun w -> (- w |> int) / 2
        squares time
        >>= text font_quicksand_bold "Ya Ya Ya Ya" (spinInOutMove 1000 35 time icf) (width + 320, 330) 0.4f

    let yaYaYa time =
        let icf = indexedGradientT fourth third "Ya Ya Ya" |> toFun
        let width = textWidth font_quicksand_bold "Ya Ya Ya" 0.4f |> fun w -> (- w |> int) / 2
        squares3 time
        >>= text font_quicksand_bold "Ya Ya Ya" (spinInOutMove 900 35 time icf) (width + 320, 330) 0.4f

    let yaYa time =
        let icf = indexedGradientT fourth third "Ya Ya" |> toFun
        let width = textWidth font_quicksand_bold "Ya Ya" 0.4f |> fun w -> (- w |> int) / 2
        squares2 time
        >>= text font_quicksand_bold "Ya Ya" (spinInOutMove 800 35 time icf) (width + 320, 330) 0.4f

    let sectionOne =
        let ya4Times = [t "00:49:947"; t "01:00:160"; t "01:10:372"; t "01:30:798"; t "01:41:011"; t "01:51:223"; t "02:01:436"; t "02:11:649"; t "02:21:862"]
        let ya2Times = [t "01:35:904"; t "01:46:117"; t "01:56:330"; t "02:06:542"; t "02:16:755"]
        let dop3 = getTimeDivisions (t "00:41:011") (t "01:20:585") 2553 @ getTimeDivisions (t "01:26:968") (t "01:47:394") 2553
        background bg_sidetracked_day (t "00:41:011") (t "02:28:245")
        >>= bgMovement (t "00:41:011") (t "02:28:245")
        >>= background (bg_sidetracked_day |> gaussBlur 6f) (t "00:41:011") (t "00:43:564")
        >>= fade (t "00:41:011") (t "00:43:564") 1f 0f
        >>= monadicMap dop3 dopDopDop
        >>= monadicMap ya4Times yaYaYaYa
        >>= monadicMap ya2Times yaYa
        >>= Transition.closingSquares (t "02:25:968") (t "02:28:245") 1000 bg_sidetracked_day

    let timer =
        let position = 635, 450
        let final = t "05:34:628" |> (fun t -> t / 1000)
        // 10 10 6 10 10
        let singleDigitAnimation i timeStart timeEnd position =
            let timeStart, timeEnd = timeStart * 1000 |> min (1000 * final), timeEnd * 1000 |> min (1000 * final)
            img ($"{font_quicksand}/{48 + i}.png") >> layer Foreground
            >>= move (timeStart - 350) timeStart (position +++ (0, 20)) position >> easing Easing.QuadOut
                >>= fade (timeStart - 350) timeStart 0f 1f  >> easing Easing.QuadOut
            >>= move (timeEnd - 350) timeEnd position (position +++ (0, -20)) >> easing Easing.QuadOut
            >>= fade (timeEnd - 350) timeEnd 1f 0f >> easing Easing.QuadOut
            >> scale timeStart timeStart 0.3f 0.3f
        singleDigitAnimation 10 0 (534) (position +++ (29, 0))
        >>= singleDigitAnimation 0 0 (534) (position +++ (10, 0))
        >>= monadicMap [0..5] (fun minutes ->
        let time = minutes * 60
        time < final >?= singleDigitAnimation minutes time (time + 60) (position +++ (20, 0))
        >>= monadicMap [0..5] (fun tenSeconds ->
        let time = time + tenSeconds * 10
        time < final >?= singleDigitAnimation tenSeconds time (time + 10) (position +++ (36, 0))
        >>= monadicMap [0..9] (fun oneSecond ->
            let time = time + oneSecond
            time < final >?= singleDigitAnimation oneSecond time (time + 1) (position +++ (46, 0))
        )))

    let playingStatus =
        let position = 550, 450
        let startPosition = 463, 450
        let endPosition = 635, 450
        let endTime = t "05:34:628"
        let c = third
        img (ui_progressbar_long_out |> resizeTo 720 25) >> coords position >> layer Foreground
        >>= scale 0 0 0.25f 0.25f
        >>= fade 0 endTime 1f 1f
        >> img (gp_filled_circle |> resize1To 10) >> coords startPosition >> layer Foreground
        >>= scale 0 0 0.25f 0.25f
        >>= fade 0 endTime 1f 1f
        >>= color 0 0 c c
        >> img (square_white |> resize1To 10) >> coords startPosition >> origin CentreLeft >> layer Foreground
        >> vectorScale 0 endTime (0f, 0.25f) (17.2f, 0.25f)
        >>= color 0 0 c c
        >> img (gp_filled_circle |> resize1To 10) >> coords startPosition >> layer Foreground
        >>= move 0 endTime startPosition endPosition
        >>= scale 0 0 0.25f 0.25f
        >>= fade 0 endTime 1f 1f
        >>= color 0 0 c c

    let sectionsLore =
        let effect ts te = fun i image s p ->
            let timeStart, timeEnd = ts + i * 100, te + i * 100
            img image >> layer Foreground
            >>= move (timeStart - 350) timeStart (p +++ (0, 20)) p >> easing Easing.QuadOut
            >>= fade (timeStart - 350) timeStart 0f 1f  >> easing Easing.QuadOut
            >>= move (timeEnd - 350) timeEnd p (p +++ (0, -20)) >> easing Easing.QuadOut
            >>= fade (timeEnd - 350) timeEnd 1f 0f >> easing Easing.QuadOut
            >> scale timeStart timeStart s s
        let line txt ts te = text font_quicksand txt (effect ts te) (460, 462) 0.2f
        line "Welcome to the intro!" 0 (t "00:41:011")
        >>= line "Ziny's section" (t "00:41:011") (t "01:47:394")
        >>= line "Ziny's goofy ahh jumps" (t "01:47:394") (t "02:18:032")
        >>= line "Ziny's stream prelude" (t "02:18:032") (t "02:28:245")
        >>= line "Petra's first stream" (t "02:28:245") (t "02:48:670")
        >>= line "The great break" (t "02:48:670") (t "03:09:096")
        >>= line "Petra's three-handed sliders" (t "03:09:096") (t "03:29:521")
        >>= line "Petra's mapping accident" (t "03:29:521") (t "04:11:649")
        >>= line "Ultra mega stream of death" (t "04:11:649") (t "04:47:394")
        >>= line "The last frontier (Slider Stream)" (t "04:47:394") (t "04:52:500")
        >>= line "Ziny's closing section" (t "04:52:500") (t "05:33:351")
        >>= line "The end..." (t "05:33:351") (t "05:34:628")

    let theStream bgStart ts te back =
        let sectionStart = ts
        let sectionEnd = te
        let center = (282, 265)
        let m = 4.311578947368421f
        let iteration = 1200
        let overall = 0.04f
        let stay = iteration |> float32 |> (*) m |> int

        background (bg_sidetracked_day |> sepia |> gaussBlur 2f) bgStart te
        >>= color ts te dark (192, 192, 220)

        >>= (timeDivisionMapi ts (te + iteration * 2) (iteration / 4) (fun i (ts, _) ->
            let ts = ts - iteration * 3
            let te = (iteration |> float32 |> (*) 1.6180339887f |> (*) 2.233f |> int) + ts
            let d = i % 4
            let rotation = List.item d [1; 2; 3; 4] |> float32 |> (*) (MathF.PI / 2f) |> (+) overall
            let spiral_scale = 0.55f
            let mBy f (x, y) = ((x |> float32) * f |> int), ((y |> float32) * f |> int)
            let x, y = (617, 209) |> rotateBy (rotation + MathF.PI) |> mBy (spiral_scale * 1.1138f)
            let dist = (center |> fst |> (+) x, center |> snd |> (+) y)
            img golden_spiral
            >>= scale ts te 0f spiral_scale >> easing Easing.ExpoIn
            >>= rotate ts ts rotation rotation
            >>= move ts te center dist >> easing Easing.ExpoIn
            >>= ((ts < sectionStart) >?= fade sectionStart (sectionStart + 200) 0f 1f)
            >>= ((te > sectionEnd) >?= fade sectionEnd (sectionEnd + 1) 1f 0f)
        ))

        >>= (back >?< timeDivisionMapi ts (te - iteration * 3 / 2) iteration (fun i (ts, _) ->
            let ts = ts - iteration * 2
            let te = stay + ts
            // Horizontal
            monadicMap [-11..11] (fun d ->
            let d = d * 2
            img rib >> coords (320, 240)
            >>= fade (ts + iteration * 3 / 2 |> max sectionStart) (te - iteration |> min sectionEnd) 0f 1f
            >>= color (ts + iteration * 3 / 2 |> max sectionStart) (te - iteration |> min sectionEnd) first fourth
            >>= rotate ts ts (overall + MathF.PI / 2f) (overall + MathF.PI / 2f)
            >>= vectorScale ts te (9f, 0.006f) (9f, 0.015f) >> easing Easing.ExpoIn
            >>= move ts te center (center +++ (440 * d, 0)) >> easing Easing.ExpoIn
            >>= (te > sectionEnd >?= fade sectionEnd sectionEnd 0f 0f))
            //Vertical
            >>= monadicMap [-11..11] (fun d ->
            let d = d * 2
            img rib >> coords (320, 240)
            >>= fade (ts + iteration * 3 / 2 |> max sectionStart) (te - iteration |> min sectionEnd) 0f 1f
            >>= color (ts + iteration * 3 / 2 |> max sectionStart) (te - iteration |> min sectionEnd) first fourth
            >>= rotate ts ts overall overall
            >>= vectorScale ts te (14f, 0.006f) (14f, 0.015f) >> easing Easing.ExpoIn
            >>= move ts te center (center +++ (0, 440 * d)) >> easing Easing.ExpoIn
            >>= (te > sectionEnd >?= fade sectionEnd sectionEnd 0f 0f))))

        >>= pingEffect (onlyFirstCombo lightPingOfCombo) ts te
        >>= circleEffect lightParamOfCombo ts te

    let sectionTwo =
        let ya4Times = [t "03:38:457"; t "03:48:670"; t "03:58:883"; t "04:09:096"]
        let ya3Times = [t "04:03:989"; t "03:53:777"]
        let dop1 = [t "03:29:521"; t "03:32:074"; t "03:34:628"; t "03:37:181"; t "03:39:734"; t "03:42:287"; t "03:44:840"; t "03:47:394"]
        let dop3 = [t "03:49:947"; t "03:55:053"; t "04:00:160"; t "04:05:266"]
        background bg_sidetracked_day (t "03:29:521") (t "04:11:649")
        >>= bgMovement (t "03:29:521") (t "04:11:649")
        >>= background (bg_sidetracked_day |> gaussBlur 6f) (t "03:29:521") (t "03:33:521")
        >>= fade (t "03:29:521") (t "03:33:521") 1f 0f
        >>= monadicMap dop1 dopDopDop
        >>= monadicMap dop3 dopDopDopx3
        >>= monadicMap ya4Times yaYaYaYa
        >>= monadicMap ya3Times yaYaYa
        >>= Transition.closingSquares (t "04:09:053") (t "04:11:649") 1000 bg_sidetracked_day

    let sectionThree =
        let ya4Times = [t "04:56:330"; t "05:01:436"; t "05:06:542"; t "05:16:755"; t "05:26:968"]
        let ya2Times = [t "05:11:649"; t "05:21:862"; t "05:32:074"]
        let dop1 = getTimeDivisions (t "04:52:500") (t "05:33:351") 2553
        background bg_sidetracked_day (t "04:52:500") (t "05:33:351")
        >>= bgMovement (t "04:52:500") (t "05:33:351")
        >>= background (bg_sidetracked_day |> gaussBlur 6f) (t "04:52:500") (t "04:54:500")
        >>= fade (t "04:52:500") (t "04:54:500") 1f 0f
        >>= monadicMap dop1 dopDopDop
        >>= monadicMap ya4Times yaYaYaYa
        >>= monadicMap ya2Times yaYa
        >>= Transition.chromoFlash (t "04:49:947") (t "04:52:500") (t "04:52:500") 2000
        >>= Transition.closingSquares (t "05:30:351") (t "05:33:351") 1000 bg_sidetracked_day

    let goodbye =
        let icf1 = indexedGradientT first third "Thank you"
                   >>>> indexedSolidT (192, 192, 220) " for playing! "
                   >>>> indexedGradientT fourth fourth "<3"
                   |> toFun
        textCenter font_quicksand_bold "Thank you for playing! <3" (flyInOut (t "05:37:181") icf1 3000) 0.4f

    let story =
        removeBg
        >>= opening (t "0:0:0") (t "00:41:011")
        >>= Transition.blackCurtains (t "00:38:457") (t "00:41:011") (t "00:41:011") (t "00:43:564")
        >>= signature
        >>= sectionOne
        >>= AscendingParticles.effect (t "01:20:585") (t "01:26:968") dot (lerpColor third fourth)
        >>= theStream (t "02:28:245") (t "02:28:245") (t "02:48:670") false
        >>= Transition.blackCurtains (t "02:47:394") (t "02:48:670") (t "02:48:670") (t "02:48:670")
        >>= opening (t "02:48:670") (t "03:29:521")
        >>= greatBreak
        >>= sectionTwo
        >>= Transition.blackCurtains (t "03:26:968") (t "03:29:521") (t "03:29:521") (t "03:30:798")
        >>= theStream (t "04:11:649") (t "04:11:649") (t "04:52:500") false
        >>= AscendingParticles.effect (t "04:11:649") (t "04:52:500") dot (lerpColor third fourth)
        >>= sectionThree
        >>= playingStatus
        >>= timer
        >>= sectionsLore
        >>= background vignette (t "0:0:0") (t "05:38:457") >> layer Foreground
        >>= goodbye

    let make () =
        openSb path
        |> story
        |> SbCompiler.write