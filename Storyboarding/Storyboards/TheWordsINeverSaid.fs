namespace Storyboarding.Storyboards

open System
open Storyboarding.Tools.Resources
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes


module TheWordsINeverSaid =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\Mage - The Words I Never Said In D&B — копия\Mage - The Words I Never Said In D&B (TheCatPetra).osb"

    let dark = (120, 150, 100)
    let black = (0, 0, 0)

    let droplet i =
        let time = i * 20
        let p_x, p_y = randPosition()
        let p_y_modified = MathF.Pow((float32 p_y) / 400f, 2f) |> (*) 400f |> int
        let position = p_x, p_y_modified
        let size = (float32 p_y_modified) / 400f
        let startPosition = p_x + 200, p_y_modified - 600
        let dropletSize = size * 0.3f
        let dropletFlyTime = 400f * size |> int
        let endPosition = p_x + (int <| dropletSize * 50f), p_y_modified - (int <| dropletSize * 150f)
        img rain_droplet
        >>= scale (time - dropletFlyTime) time dropletSize dropletSize
        >>= move (time - dropletFlyTime) time startPosition endPosition
        >>= color time time dark dark
        >> img rain_drop_w
        >>= scale time (time + 2000) 0f (1.5f * size)
        >>= fade time (time + 700) 1f 0f
        >>= move time (time + 2000) position position
        >>= color time time dark dark
        >>= alpha
        >> img rain_drop_b
        >>= move time (time + 2000) position position
        >>= scale time (time + 2000) 0f (1.5f * size)
        >>= fade time (time + 700) 1f 0f
        >>= easing Easing.Out

    let surface_animation i =
        let timeStart = 0
        let timeEnd = 0 + 120000
        let px1, py1 = randPosition()
        let px2, py2 = randPosition()
        let relativeScale x =
            let r = 1550f - float32 x |> (/) 1550f
            r * 0.6f
        img water_normal
        >>= move timeStart timeEnd (px1 + 5, py1) (px2 + 5, py2)
        >>= scale timeStart timeEnd (relativeScale py1) (relativeScale py2)
        >>= color timeStart timeEnd dark dark
        >>= alpha

    let normalEffect =
        TextUtils.chromoInOut 1000 15

    let headerEffect =
        TextUtils.chromoInOut 1000 15

    let subtitles =
        ["00:23:210", "Always", headerEffect
         "00:23:807", "in a rush", normalEffect
         "00:24:854", "Never", headerEffect
         "00:25:552", "stay on the phone long enough", normalEffect
         "00:27:645", "Why", headerEffect
         "00:28:691", "am I so", normalEffect
         "00:29:389", "self important", normalEffect
         "00:34:273", "Said", headerEffect
         "00:34:796", "I'd see you soon", normalEffect
         "00:36:017", "That", headerEffect
         "00:36:540", "was, oh, maybe a year ago", normalEffect
         "00:38:807", "Didn't know", headerEffect
         "00:39:854", "time was of the essence", normalEffect]

    let writeSubtitles =
        let subtitles = subtitles |> List.map (fun (a, b, c) -> t a, b, c)
        monadicMap subtitles (fun (time, txt, effect) ->
        let centre = TextUtils.centreOfText font_monomach txt 0.1f
        TextUtils.text font_monomach txt (effect time) centre 0.1f)


    let story =
        img square_black
        >>= scale 15 315479 10f 10f
        >> img (ImageFilters.gaussBlur 3f bg_twins_upper)
        >>= color 15 15 dark dark
        >>= scaleAsBg 15 315479
        >>= fade 15 315479 1f 1f
        >>= alpha
        >>= verticalFlip
        >> monadicMap [1..3000] droplet
        >> monadicMap [1..3] surface_animation
        >>= writeSubtitles

    let make () =
        openSb path
        |> story
        |> SbCompiler.write

