namespace Storyboarding.Gameplay

open Storyboarding.Tools.ImageFilters
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module UI =
    let timer timeEnd =
        let position = 635, 450
        let final = timeEnd |> (fun t -> t / 1000)
        // 10 10 6 10 10
        let singleDigitAnimation i timeStart timeEnd position =
            let timeStart, timeEnd = timeStart * 1000 |> min (1000 * final), timeEnd * 1000 |> min (1000 * final)
            img ($"{font_quicksand}/{48 + i}.png") >> layer Foreground
            >>= move (timeStart - 350) timeStart (position +++ (0, 20)) position >> easing Easing.QuadOut
                >>= fade (timeStart - 350) timeStart 0f 1f  >> easing Easing.QuadOut
            >>= move (timeEnd - 350) timeEnd position (position +++ (0, -20)) >> easing Easing.QuadOut
            >>= fade (timeEnd - 350) timeEnd 1f 0f >> easing Easing.QuadOut
            >> scale timeStart timeStart 0.3f 0.3f
        singleDigitAnimation 10 0 timeEnd (position +++ (29, 0))
        >>= monadicMap [0..5] (fun tenMinutes ->
        let time = tenMinutes * 600
        time < final >?= singleDigitAnimation tenMinutes time (time + 600) (position +++ (10, 0))
        >>= monadicMap [0..9] (fun minutes ->
        let time = time + minutes * 60
        time < final >?= singleDigitAnimation minutes time (time + 60) (position +++ (20, 0))
        >>= monadicMap [0..5] (fun tenSeconds ->
        let time = time + tenSeconds * 10
        time < final >?= singleDigitAnimation tenSeconds time (time + 10) (position +++ (36, 0))
        >>= monadicMap [0..9] (fun oneSecond ->
            let time = time + oneSecond
            time < final >?= singleDigitAnimation oneSecond time (time + 1) (position +++ (46, 0))
        ))))

    let playingStatus barColor endTime =
        let position = 550, 450
        let startPosition = 463, 450
        let endPosition = 635, 450
        img (ui_progressbar_long_out |> resizeTo 720 25) >> coords position >> layer Foreground
        >>= scale 0 0 0.25f 0.25f
        >>= fade 0 endTime 1f 1f
        >> img (gp_filled_circle |> resize1To 10) >> coords startPosition >> layer Foreground
        >>= scale 0 0 0.25f 0.25f
        >>= fade 0 endTime 1f 1f
        >>= color 0 0 barColor barColor
        >> img (square_white |> resize1To 10) >> coords startPosition >> origin CentreLeft >> layer Foreground
        >> vectorScale 0 endTime (0f, 0.25f) (17.2f, 0.25f)
        >>= color 0 0 barColor barColor
        >> img (gp_filled_circle |> resize1To 10) >> coords startPosition >> layer Foreground
        >>= move 0 endTime startPosition endPosition
        >>= scale 0 0 0.25f 0.25f
        >>= fade 0 endTime 1f 1f
        >>= color 0 0 barColor barColor