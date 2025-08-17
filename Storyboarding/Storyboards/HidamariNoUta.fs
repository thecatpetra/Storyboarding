namespace Storyboarding.Storyboards

open System.IO
open Storyboarding.Effects
open Storyboarding.Effects.Background
open Storyboarding.Effects.PerNoteEffect
open Storyboarding.Tools
open Storyboarding.Tools.Paths
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module HidamariNoUta =
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2420040 Apol - Hidamari no Uta\Apol - Hidamari no Uta (Asahina Oleshev).osb"

    let lyrics = [t "01:24:360", $"{char 12131}まれた事さえ"
                  t "01:27:890", "後悔するんだ"
                  t "01:31:419", "知りたくなかった"
                  t "01:34:948", "気持ちがあるんだ"
                  t "01:38:478", "教えてもらった"
                  t "01:42:007", "コーヒーの味が"
                  t "01:45:537", "切ないくらいに"
                  t "01:49:066", "を締め付ける"
                  ////
                  t "02:24:360", "創造出来ない"
                  t "02:27:890", "苦しみの中で"
                  t "02:31:419", $"{char 0x2F92}つけた時間を"
                  t "02:34:948", "幸せと呼んだ"
                  t "02:38:478", "教えてもらった"
                  t "02:42:007", "コーヒーの味は"
                  t "02:45:537", $"今でも{char 0x2F3C}に"
                  t "02:49:066", "刻まれているよ"]

    let lyrics2 = [(t "01:52:595", "命の終わりを"), (t "01:56:125", $"知った{char 12103}に")
                   (t "01:59:654", $"{char 12013}{char 12190}の{char 12211}が"), (t "02:03:184", "重くなる")
                   (t "02:06:713", "陽だまりの中に"), (t "02:10:243", "映りこむ")
                   (t "02:13:772", $"あなたの{char 12131}きてた"), (t "02:17:301", "その証")]

    do
        let all_text = $"%A{lyrics}%A{lyrics2}"
        let fontPath = Path.Join(fontsFolder, FileInfo(font_notosans_jp).Name)
        TextUtils.createFontSubset fontPath all_text 128

    let intro =
        let blurredMax = bg_hidamari_1 |> ImageFilters.gaussBlur 40f
        let blurredAvg = bg_hidamari_1 |> ImageFilters.gaussBlur 20f
        let blurredMin = bg_hidamari_1 |> ImageFilters.gaussBlur 7f
        background blurredMin 0 (t "00:56:566")
        >> background blurredMin 0 (t "00:56:566")
        >>= fade (t "00:47:743") (t "00:56:566") 1f 0f
        >> background blurredAvg 0 (t "00:56:566")
        >>= fade (t "00:38:919") (t "00:47:743") 1f 0f
        >> background blurredMax 0 (t "00:56:566")
        >>= fade (t "00:28:331") (t "00:38:919") 1f 0f
        >>= color (t "00:00:000") (t "00:38:919") (0, 0, 0) (192, 192, 192)
        >>= Transition.chromoFlash (t "00:51:919") (t "00:56:566") (t "00:56:566") 300
        >>= Transition.chromoFlash (t "00:51:919") (t "00:56:566") (t "00:56:566") 300
        >> parallax (t "00:56:566") (t "01:24:360") [ "bg/hidamari/1_3.jpg", 7f; "bg/hidamari/1_2.png", 13f; "bg/hidamari/1_1.png", 40f ]
        >>= doubleRayEffect (onlyFirstCombo lightParamOfCombo) (t "00:56:566") (t "01:24:360")
        >>= Transition.blackCurtains (t "01:21:272") (t "01:24:360") (t "01:24:360") (t "01:25:684")

    let firstSection =
        parallax (t "01:24:360") (t "01:52:595") [ "bg/hidamari/2_3.jpg", 4f; "bg/hidamari/2_2.png", 7f; "bg/hidamari/2_1.png", 14f ]
        >>= Transition.blackCurtains (t "01:49:066") (t "01:52:595") (t "01:52:595") (t "01:53:478")
        >>= Transition.chromoFlash (t "02:20:169") (t "02:21:272") (t "02:23:037") 300
        >>= Transition.chromoFlash (t "02:20:169") (t "02:21:272") (t "02:23:037") 300

    let stdLyrics lyrics =
        monadicMap (Seq.toList lyrics) (fun l ->
        let time, txt = l
        let scale = 0.20f
        let position = TextUtils.centreOfText font_notosans_jp txt scale
        let effect = TextUtils.chromoInOut 2700 100 time
        TextUtils.text font_notosans_jp txt effect position scale)

    let stdLyrics2 lyrics =
        monadicMap lyrics (fun (l1, l2) ->
        monadicMap [l1, -1; l2, 1] (fun (l, p) ->
        let time, txt = l
        let scale = 0.33f
        let position = TextUtils.centreOfText font_notosans_jp txt scale +++ (0, p * 30)
        let effect = TextUtils.neonInOut (2700 / 2 * (3 - p)) time (255, 255, 255) (0, 128, 255)
        TextUtils.text font_notosans_jp txt effect position scale))

    let renderLyrics =
        stdLyrics lyrics

    let renderLyrics2 =
        stdLyrics2 lyrics2

    let story =
        intro
        >>= firstSection
        >>= renderLyrics
        >>= renderLyrics2
        >>= background vignette 0 (t "05:28:676") >> layer Foreground

    let make () =
        openSb path
        |> story
        |> SbCompiler.write