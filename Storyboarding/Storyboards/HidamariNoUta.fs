namespace Storyboarding.Storyboards

open System.IO
open Storyboarding.Effects
open Storyboarding.Effects.Background
open Storyboarding.Effects.PerNoteEffect
open Storyboarding.Effects.Transition
open Storyboarding.Tools
open Storyboarding.Tools.ColorUtils
open Storyboarding.Tools.Paths
open Storyboarding.Tools.Resources
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.TextUtils

module HidamariNoUta =
    let path = @"D:\D\osu!\Songs\2420040 Apol - Hidamari no Uta\Apol - Hidamari no Uta (Asahina Oleshev).osb"

    let lyrics = [t "01:24:360", $"{char 12131}まれた事さえ"
                  t "01:27:890", "後悔するんだ"
                  t "01:31:419", "知りたくなかった"
                  t "01:34:948", "気持ちがあるんだ"
                  t "01:38:478", "教えてもらった"
                  t "01:42:007", "コーヒーの味が"
                  t "01:45:537", "切ないくらいに"
                  t "01:49:066", "胸を締め付ける"
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

    // do
    //     let all_text = $"%A{lyrics}%A{lyrics2}【アポル】陽だまりの詩 歌ってみた-Hidamari no UtaApol"
    //     let fontPath = Path.Join(fontsFolder, FileInfo(font_notosans_jp).Name)
    //     createFontSubset fontPath all_text 128
    // do
    //     let all_text = [$"長い　時間の中で…今も… "; "死ぬこと|の意味を|理解した|僕は"; "この世の|全てを|愛そう|と決めた"; "教えて|もらった|何もかも|全て"; "陽だまりの|中に|「ありがとう」と|響く…"] |> String.concat ""
    //     let fontPath = Path.Join(fontsFolder, FileInfo(font_notoserif_jp).Name)
    //     createFontSubset fontPath all_text 128

    let doubleChromoFlash a b c d =
        chromoFlash a b c d
        >>= chromoFlash a b c d

    let intro =
        let blurredMax = bg_hidamari_1 |> ImageFilters.gaussBlur 40f
        let blurredAvg = bg_hidamari_1 |> ImageFilters.gaussBlur 20f
        background blurredAvg 0 (t "00:56:566")
        >> background blurredMax 0 (t "00:56:566")
        >>= fade (t "00:28:331") (t "00:56:566") 1f 0f
        >>= MarchingSquares.waves 0 (t "00:56:566")
        >>= color (t "00:00:000") (t "00:38:919") (0, 0, 0) (192, 192, 192)
        >>= doubleChromoFlash (t "00:51:919") (t "00:56:566") (t "00:56:566") 300
        >> parallax (t "00:56:566") (t "01:24:360") [ "bg/hidamari/1_3.jpg", 6f; "bg/hidamari/1_2.png", 11f; "bg/hidamari/1_1.png", 20f ]
        >>= doubleRayEffect (onlyFirstCombo lightParamOfCombo) (t "00:56:566") (t "01:24:360")
        >>= blackCurtains (t "01:21:272") (t "01:24:360") (t "01:24:360") (t "01:25:684")

    let firstSection =
        parallax (t "01:24:360") (t "01:52:595") [ "bg/hidamari/2_3.jpg", 4f; "bg/hidamari/2_2.png", 7f; "bg/hidamari/2_1.png", 14f ]
        >>= blackCurtains (t "01:49:066") (t "01:52:595") (t "01:52:595") (t "01:53:478")
        >>= doubleChromoFlash (t "02:20:169") (t "02:21:272") (t "02:23:698") 300
        >>= blackCurtains (t "02:21:272") (t "02:24:140") (t "02:24:140") (t "02:25:243")

    let kiaiEffects ts te =
        circleEffect lightParamOfCombo ts te
        >>= pingEffect (onlyFirstCombo lightPingOfCombo) ts te

    let kiai1 =
        parallax (t "01:52:595") (t "02:23:037") [ "bg/hidamari/3_3.jpg", 6f; "bg/hidamari/3_2.png", 11f; "bg/hidamari/3_1.png", 20f ]
        >>= kiaiEffects (t "01:52:595") (t "02:23:037")

    let kiai2 =
        parallax (t "04:12:007") (t "04:40:683") [ "bg/hidamari/3_3.jpg", 6f; "bg/hidamari/3_2.png", 11f; "bg/hidamari/3_1.png", 20f ]
        >>= kiaiEffects (t "04:12:007") (t "04:40:683")
        >>= doubleChromoFlash (t "04:39:580") (t "04:40:683") (t "04:40:683") 1500

    let nearKiai =
        parallax (t "03:15:978") (t "03:44:213") [ "bg/hidamari/3_3.jpg", 6f; "bg/hidamari/3_2.png", 11f; "bg/hidamari/3_1.png", 20f ]
        >>= blackCurtains (t "03:42:228") (t "03:43:772") (t "03:43:772") (t "03:45:095")

    let secondSection =
        parallax (t "02:24:140") (t "02:53:037") [ "bg/hidamari/2_3.jpg", 4f; "bg/hidamari/2_2.png", 7f; "bg/hidamari/2_1.png", 14f ]
        >>= blackCurtains (t "02:52:154") (t "02:53:037") (t "02:53:037") (t "02:55:037")

    let stdLyrics lyrics =
        monadicMap (Seq.toList lyrics) (fun l ->
        let time, txt = l
        let scale = 0.20f
        let position = centreOfText font_notosans_jp txt scale
        let effect = chromoInOut 2700 100 time
        text font_notosans_jp txt effect position scale)

    let stdLyrics2 lyrics =
        monadicMap lyrics (fun (l1, l2) ->
        monadicMap [l1, -1; l2, 1] (fun (l, p) ->
        let time, txt = l
        let scale = 0.33f
        let position = centreOfText font_notosans_jp txt scale +++ (0, p * 30)
        let effect = chromoSpinInOut (3530 / 2 * (3 - p) - 800) 20 time
        text font_notosans_jp txt effect position scale))

    let renderLyrics =
        stdLyrics lyrics

    let renderLyrics2 =
        let diff = (t "04:12:007") - (t "01:52:595")
        stdLyrics2 (lyrics2 @ (lyrics2 |> List.map (fun ((t1, txt1), (t2, txt2)) -> (t1 + diff, txt1), (t2 + diff, txt2))))

    let goofyAhhSection =
        let timeStart = (t "02:53:037")
        let timeEnd = (t "03:15:978")
        let bgEffect sb =
            let iteration = beatTime timeStart sb |> (*) 64
            let inner direction c = monadicMap [0..4] (fun i ->
                img square_white
                >>= color timeStart timeStart c c
                >>= rotate timeStart timeStart (0.2f * (float32 direction)) (0.2f * (float32 direction))
                >>= vectorScale timeStart timeStart (0.8f, 4.5f) (0.8f, 4.5f)
                >>= fade (t "03:15:978") (t "03:15:979") 1f 0f
                >>= timeDivisionMap timeStart timeEnd iteration (fun (ts, te) ->
                let prevPosition = (320 - direction * (-520 + 200 * i), 240)
                let nextPosition = (320 - direction * (-520 + 200 * (i + 1)), 240)
                move ts te prevPosition nextPosition))
            (inner 1 (245, 245, 245) >>= inner -1 (235, 235, 235)) sb
        let goofySectionTextEffect stay time = fun i image s p ->
            let effectTime = 1200
            let diff = 220 * i
            monadicMap [-1] (fun ii ->
            img image
            >>= move (time + diff) (time + effectTime + diff) (p +++ (0, 20 * ii)) p >> easing Easing.In
            >>= fade (time + diff) (time + effectTime + diff) 0f 1f
            >>= fade (time + stay) (time + stay + effectTime) 1f 0f
            >>= scale (time + diff) (time + stay) s s
            >>= color (time + diff) (time + stay) (0, 0, 0) (0, 0, 0))
        let nagai time =
            let effect = goofySectionTextEffect 1800 time
            TextUtils.textCenter font_notoserif_jp $"{char 11985}い  {char 11985}い" effect 0.3f
        let jikan time =
            let effect = goofySectionTextEffect 2500 time
            TextUtils.textCenter font_notoserif_jp "時間の中で…" effect 0.3f
        let imamo time =
            let effect = goofySectionTextEffect 2000 time
            TextUtils.textCenter font_notoserif_jp "今も…" effect 0.3f
        background square_white timeStart timeEnd
        >>= bgEffect
        >>= nagai (t "02:53:037") >>= jikan (t "02:55:242")
        >>= nagai (t "02:58:331") >>= jikan (t "03:00:537")
        >>= nagai (t "03:03:184") >>= jikan (t "03:05:831")
        >>= nagai (t "03:08:478") >>= jikan (t "03:11:125")
        >>= imamo (t "03:14:213")
        >>= doubleChromoFlash (t "03:15:095") (t "03:15:978") (t "03:15:978") 2000

    let silenceSection =
        let timeStart = t "03:43:772"
        let timeEnd = t "04:12:007"
        let lyrics = ["死ぬ.こと|の意.味を|理解.した|僕.は"; "この.世の|全.てを|愛.そう|と決.めた"; "教.えて|もら.った|何も.かも|全.て"; "陽だ.まりの|中.に|「ありがとう」.|と響.く…"]
        let effect stay time = fun i image s p ->
            img (image |> ImageFilters.gaussBlur 1f |> ImageFilters.maskOnWhite)
            >>= move (time + i * 220) (time + stay |> min timeEnd) p p
            >>= scale (time + i * 220) (time + i * 220) s s
        let txt =
            monadicMapi lyrics (fun i part ->
            monadicMapi (part.Split("|") |> Seq.toList) (fun j syl ->
            let [ft; sd] = syl.Split(".") |> Seq.toList
            let t = (1764 * j + 7058 * i) + timeStart
            let horizontal = TextUtils.textWidth font_notoserif_jp ft 0.4f |> int
            TextUtils.text font_notoserif_jp ft (effect (3528 * 2) t) (64, 144 + 80 * j) 0.4f
            >>= TextUtils.text font_notoserif_jp sd (effect (3528 * 2 - 880) (t + 880)) (64 + horizontal, 144 + 80 * j) 0.4f))
        background square_black timeStart (t "04:12:007")
        >>= txt
        >>= background screen_overlay timeStart (t "04:12:007")
        >>= blackCurtains (t "04:08:477") (t "04:12:007") (t "04:12:007") (t "04:14:007")

    let finSection =
        parallax (t "04:40:683") (t "05:08:919") [ "bg/hidamari/4_3.jpg", 7f; "bg/hidamari/4_2.png", 11f; "bg/hidamari/4_1.png", 20f ]
        >>= kiaiEffects (t "04:54:801") (t "05:08:919")
        >>= doubleChromoFlash (t "05:07:154") (t "05:08:919") (t "05:08:919") 2000

    let closingSection =
        let blurredMax = bg_hidamari_1 |> ImageFilters.gaussBlur 40f
        let blurredAvg = bg_hidamari_1 |> ImageFilters.gaussBlur 20f
        background blurredAvg (t "05:08:919") (t "05:28:676")
        >> background blurredMax (t "05:08:919") (t "05:28:676")
        >>= fade (t "05:08:919") (t "05:28:676") 0f 1f
        >>= color (t "05:08:919") (t "05:28:676") (192, 192, 192) (0, 0, 0)
        >>= MarchingSquares.waves (t "05:08:919") (t "05:28:676")

    let signature =
        let jpAuthor = "【アポル】"
        let jpTitle = "陽だまりの詩 歌ってみた"
        let enAuthor = "Apol"
        let enTitle = "Hidamari no Uta"
        let s = 0.2f
        let jp =
            let jpAll = jpAuthor + "-  " + jpTitle
            let jpIcf = indexedGradient (79, 227, 118) (79, 160, 227) 5 >>>> indexedSolid (192, 192, 192) 3 >>>> indexedGradient (79, 224, 227) (84, 79, 227) 12 |> toFun
            textCenter font_notosans_jp jpAll (neonInOutColorFun (t "00:03:184") (t "00:00:096") jpIcf jpIcf) s
        let en =
            let enAll = enAuthor + " - " + enTitle
            let enIcf = indexedGradient (79, 227, 118) (79, 160, 227) 4 >>>> indexedSolid (192, 192, 192) 3 >>>> indexedGradient (79, 224, 227) (84, 79, 227) 15 |> toFun
            textCenter font_monosans enAll (neonInOutColorFun (t "00:03:184") (t "00:03:625") enIcf enIcf) s
        let mapper =
            let mAll = "Mapped by: " + "Asahina Oleshev"
            let enIcf = indexedSolid (192, 192, 192) 11 >>>> indexedGradient (255, 198, 161) (255, 159, 133) 15 |> toFun
            textCenter font_monosans mAll (neonInOutColorFun (t "00:03:184") (t "00:07:154") enIcf enIcf) s
        let storyboarder =
            let sAll = "Storyboard by: " + "TheCatPetra"
            let enIcf = indexedSolid (192, 192, 192) 15 >>>> indexedGradient (107, 242, 202) (152, 213, 250) 11 |> toFun
            textCenter font_monosans sAll (neonInOutColorFun (t "00:03:184") (t "00:10:684") enIcf enIcf) s
        jp >>= en >>= mapper >>= storyboarder

    let story =
        removeBg
        >>= intro
        >>= signature
        >>= firstSection
        >>= kiai1
        >>= secondSection
        >>= goofyAhhSection
        >>= nearKiai
        >>= silenceSection
        >>= kiai2
        >>= finSection
        >>= closingSection
        >>= renderLyrics
        >>= renderLyrics2
        >>= background vignette 0 (t "05:28:676") >> layer Foreground

    let make () =
        openSb path
        |> story
        |> SbCompiler.write
