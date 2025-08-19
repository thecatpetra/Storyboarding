namespace Storyboarding.Storyboards

open System
open System.Numerics
open MapsetParser.objects
open Microsoft.FSharp.Linq
open Storyboarding.Effects
open Storyboarding.Effects.SquareFading
open Storyboarding.Tools.Resources
open Storyboarding.Tools
open Storyboarding.Tools.SbMonad
open Storyboarding.Effects.Background
open Storyboarding.Tools.SbRandom
open Storyboarding.Tools.SbTypes
open Storyboarding.Tools.TextUtils

module WithABurningHeart =
    // Fine map but i hate ranked
    let bad = @"C:\Users\arthur\AppData\Local\osu!\Songs\2182276 DragonForce - Burning Heart\DragonForce - Burning Heart (Maaadbot).osb"
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2388814 DragonForce - Burning Heart (feat Alissa White-Gluz)\DragonForce - Burning Heart (feat. Alissa White-Gluz) (Rose Winters).osb"

    let introSection =
        let sectionEnd = t "00:10:080"
        let bad_bg = bg_burning_heart_raw |> ImageFilters.grayscale |> ImageFilters.gaussBlur 30f
        let bg = bg_burning_heart_raw |> ImageFilters.gaussBlur 15f
        background bg 0 sectionEnd
        >>= background bad_bg 0 sectionEnd
        >>= color 0 sectionEnd (40, 40, 40) (150, 140, 120)
        >>= fade 0 sectionEnd 1f 0.7f
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColor (200, 190, 180)) 0 sectionEnd

    let fillerSection1 =
        let sectionStart = t "00:10:080"
        let sectionEnd = t "00:27:680"
        let bgExplosionEnd = sectionStart + 400
        let bg_explosion = bg_burning_heart_raw
        let bg = bg_burning_heart_raw
        background bg sectionStart sectionEnd
        >>= bgMovement sectionStart sectionEnd
        >>= background bg_explosion sectionStart bgExplosionEnd
        >>= alpha
        >>= scale sectionStart bgExplosionEnd 0.4571f 0.6f
        >>= fade sectionStart bgExplosionEnd 0.2f 0f
        >>= RealisticSparks.effect sectionStart (sectionEnd - 5000)
        >>= RisingSmoke.effect sectionStart (sectionEnd - 5000)
        >>= Transition.dim (t "00:27:146") (t "00:28:213") 100

    let withABurningHeart timeStart sb =
        let diff = beatTime timeStart sb |> (*) 50
        monadicMap [0..3] (fun i ->
        let text = "WITH A BURNING HEART"
        let scale = 0.18f + 0.02f * (float32 i)
        let position = centreOfText font_text_me_one text scale
        let c1 = (120 + 40 * i, 20 + 18 * i, 0)
        let c2 = (120 + 40 * i, 30 + 33 * i, 33 * i)
        let eff = (neonInOut 2000 (timeStart + i * diff) c1 c2)
        TextUtils.text font_text_me_one text eff position scale) sb

    let stdLyrics lyrics =
        monadicMap (Seq.toList lyrics) (fun l ->
        let time, txt = l
        let scale = 0.25f
        let position = centreOfText font_monosans txt scale
        let effect = chromoInOut 1000 25 time
        text font_monosans txt effect position scale)

    let stdLyrics2 lyrics =
        monadicMap (Seq.toList lyrics) (fun l ->
        let time, txt = l
        let scale = 0.3f
        let position = centreOfText font_monosans txt scale
        let effect = neonInOut 1300 time (255, 255, 128) (255, 0, 0)
        text font_monosans txt effect position scale)

    let pingGen (ho : HitObject) =
        let isNewCombo = ho.``type`` &&& HitObject.Type.NewCombo = HitObject.Type.NewCombo
        match isNewCombo with
        | true -> PerNoteEffect.pingParamsOfColor (200, 190, 180) ()
        | _ -> None

    let rayGen (ho : HitObject) =
        let isNewCombo = ho.``type`` &&& HitObject.Type.NewCombo = HitObject.Type.NewCombo
        match isNewCombo with
        | true -> PerNoteEffect.pingParamsOfColor (200, 190, 180) ()
        | _ -> None

    let lyricsSection1 =
        let timeStart, timeEnd = t "00:28:213", t "00:54:880"
        let lyrics = [t "00:28:213", "Trapped within this hideaway"
                      t "00:30:346", "The world above burned down in flames"
                      t "00:32:480", "In the night"
                      t "00:34:080", "A lonely satellite"
                      t "00:36:746", "Left to die out here alone"
                      t "00:38:880", "Defenders of an empty throne"
                      t "00:41:013", "The last command"
                      t "00:42:613", "Here is where we make"]
        let ourFinalPosition = TextUtils.centreOfText font_monosans "Our final" 0.35f
        let standPosition = TextUtils.centreOfText font_monosans "Stand" 0.42f
        let lastLyricsEffect time stay = TextUtils.chromoInOut stay 25 time
        let lyrics =
            stdLyrics lyrics
            >>= TextUtils.text font_monosans "Our final" (lastLyricsEffect (t "00:43:946") 700) ourFinalPosition 0.35f
            >>= TextUtils.text font_monosans "Stand" (lastLyricsEffect (t "00:45:280") 2300) standPosition 0.42f
        background bg_warrior_on_a_log timeStart timeEnd
        >>= bgMovement timeStart timeEnd
        >>= color timeStart timeStart (192, 192, 192) (192, 192, 192)
        >>= lyrics
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColor (250, 100, 30)) (t "00:45:280") timeEnd
        >>= PerNoteEffect.pingEffect pingGen (t "00:45:280") timeEnd
        >>= RealisticSparks.effect (t "00:45:280") (timeEnd - 5000)
        >>= RisingSmoke.effect (t "00:45:280") (timeEnd - 5000)
        >>= Transition.dim (t "00:53:813") (t "01:02:346") 100

    let burningHeart timeStart =
        let timeEnd = (t "01:10:880") - (t "00:54:080") + timeStart
        let nextSection = (t "01:11:946") - (t "00:54:080") + timeStart
        let bg_burning_heart_blur = bg_burning_heart_raw |> ImageFilters.gaussBlur 30f |> ImageFilters.grayscale
        let bg_burning_heart_mid_blur = bg_burning_heart_raw |> ImageFilters.gaussBlur 13f |> ImageFilters.grayscale
        let bgTimeStart = timeStart - (t "00:54:080") + (t "01:02:346")
        let bgTimeMid = (timeEnd - bgTimeStart) / 2 + bgTimeStart
        background bg_burning_heart_raw bgTimeStart (timeEnd + 100)
        >> background bg_burning_heart_mid_blur bgTimeStart timeEnd
        >>= color bgTimeStart timeEnd (128, 128, 128) (255, 255, 255)
        >>= fade bgTimeMid timeEnd 1f 0f
        >> background bg_burning_heart_blur bgTimeStart bgTimeMid
        >>= color bgTimeStart timeEnd (0, 0, 0) (128, 128, 128)
        >>= fade bgTimeStart bgTimeMid 1f 0f
        >>= withABurningHeart timeStart
        >>= Transition.dim timeEnd nextSection 100

    let withABurningHeartPath timeStart sb=
        let beat = beatTime timeStart sb
        let wab = "With a burning"
        let wabEffect = TextUtils.chromoInOut 500 10 timeStart
        let wabScale = 0.25f
        let wabPosition = TextUtils.centreOfText font_monosans wab wabScale
        let h = "HEART"
        let hEffect = TextUtils.neonInOut 500 (timeStart + 9 * beat) (240, 200, 150) (240, 200, 150)
        let hScale = 0.3f
        let hPosition = TextUtils.centreOfText font_text_me_one h hScale
        (TextUtils.text font_monosans wab wabEffect wabPosition wabScale
        >>= TextUtils.text font_text_me_one h hEffect hPosition hScale) sb

    let chorus timeStart extend lastPart =
        let adjustTime = fun x -> x - (t "01:11:146") + timeStart
        let timeEnd = timeStart - (t "01:11:146") + (t "01:36:480") + extend
        let soloStart = timeStart - (t "01:11:146") + (t "01:27:946")
        let nextSection = timeStart - (t "01:11:146") + (t "01:37:546") + extend
        let wabDiff = t "01:11:146" |> (-) (t "01:15:413")
        let lyrics = [t "01:13:013", "Eyes of fire, cold as ice"
                      t "01:17:013", "See how the winds will change"
                      t "01:19:413", "If a moment's all we are"
                      t "01:21:546", "In the sky of a million stars"
                      t "01:23:946", "With a burning heart"
                      t "01:25:546", "We'll fight another day"]
                     @ if lastPart then [t "01:28:213", "With a burning heart"; t "01:29:813", "We'll fight another day"] else []
                     |> Seq.map (fun (a, b) -> adjustTime a, b)
        background bg_burning_heart_raw (timeStart + 800) (timeEnd + 100)
        >>= bgMovement (timeStart + 800) timeEnd
        >>= withABurningHeartPath timeStart
        >>= withABurningHeartPath (timeStart + wabDiff)
        >>= stdLyrics lyrics
        >>= RealisticSparks.effect soloStart timeEnd
        >>= RisingSmoke.effect soloStart timeEnd
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColor (250, 100, 30)) timeStart timeEnd
        >>= PerNoteEffect.pingEffect pingGen timeStart timeEnd
        >>= Transition.dim timeEnd nextSection 100

    let lyricsSection2 =
        let timeStart = t "01:37:546"
        let timeEnd = t "02:03:146"
        let soloStart = t "01:54:613"
        let nextSection = t "02:11:680"
        let lyrics = [t "01:37:546", "Wars are raging in my mind"
                      t "01:39:680", "Soon to cross the danger line"
                      t "01:41:813", "High and dry"
                      t "01:43:413", "But we still feel alive"
                      t "01:46:080", "Kill the outcasts, slay the hordes"
                      t "01:48:213", "Crush The Dark Knight's overlords"
                      t "01:50:346", "Genocide"
                      t "01:51:946", "Now it's time to say"
                      t "01:53:280", "Your last goodbyes"]
        background bg_burning_city timeStart (timeEnd + 1300)
        >>= bgMovement timeStart (timeEnd + 1500)
        >>= stdLyrics2 lyrics
        >>= RisingSmoke.effect timeStart timeEnd
        >>= RealisticSparks.effect soloStart timeEnd
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColor (250, 100, 30)) soloStart timeEnd
        >>= PerNoteEffect.pingEffect pingGen soloStart timeEnd
        >>= Transition.dim timeEnd nextSection 100

    let ascendingShapes timeStart timeEnd sb =
        let step = beatTime timeStart sb |> (*) 3 |> (*) 4
        let amount = 30
        monadicMap [1..4] (fun l ->
        monadicMap [1..amount] (fun s ->
        let x = randX ()
        let bottomColor = (230, 67, 55)
        let topColor = (79, 76, 176)
        let initialPosition = x, 380 + randInt 0 200 + 100 * l
        let endPosition = x + (x - 320) * 2, -120 - 100 * l
        let behaviour =
            alpha
            >>= timeDivisionMapi (timeStart + s * step) (timeStart + (s + 14) * step) step (fun i (ts, te) ->
            let ts, te = ts + (s % 2) * step / 2, te + (s % 2) * step / 2
            let lp = (float32 i) / 15f
            let lc = (float32 i + 1f) / 15f
            let prev = lerp initialPosition endPosition lp
            let current = lerp initialPosition endPosition lc
            let prevColor = lerpColor bottomColor topColor (lp |> (*) 3f |> fun x -> (-) x 1f |> min 1f |> max 0f)
            let currColor = lerpColor bottomColor topColor (lc |> (*) 3f |> fun x -> (-) x 1f |> min 1f |> max 0f)
            color ts te prevColor currColor
            >>= move ts te prev current
            >>= easing Easing.QuartOut
            >>= rotate ts te 0f (MathF.PI / 2f - MathF.PI * float32 (s % 2))
            >>= easing Easing.QuartOut)
        img square_white
        >>= scale timeStart timeEnd (0.1f * (float32 l)) (0.1f * (float32 l))
        >>= fade timeStart timeEnd (0.1f * (float32 l)) (0.1f * (float32 l))
        >>= behaviour
        >> img light
        >>= scale timeStart timeEnd (0.2f * (float32 l)) (0.2f * (float32 l))
        >>= fade timeStart timeEnd (0.2f * (float32 l)) (0.2f * (float32 l))
        >>= behaviour)) sb

    let soloSection1 =
        let timeStart, timeEnd = (t "02:47:946"), (t "03:57:279")
        let blurred = bg_desert_sunset |> ImageFilters.gaussBlur 5f
        background bg_desert_sunset timeStart timeEnd
        >> background blurred timeStart (t "02:57:546")
        >>= fade (t "02:47:946") (t "02:57:546") 1f 0f
        >>= ascendingShapes (t "03:07:146") (t "03:55:146")
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColor (255, 231, 110)) (t "03:26:346") timeEnd
        >>= PerNoteEffect.pingEffect pingGen (t "03:26:346") timeEnd
        >>= FFTEffects.circleFft timeStart timeEnd
        >>= Transition.chromoFlash (t "03:55:946") (t "03:57:279") (t "03:57:279") 1000
        >>= Transition.chromoFlash (t "03:55:946") (t "03:57:279") (t "03:57:279") 1000

    let soloSection2 =
        let timeStart, timeEnd = (t "03:57:279"), (t "04:47:412")
        let blurred = bg_mountains_sunset |> ImageFilters.gaussBlur 5f
        let comboGrad = [0..20] |> Seq.map float32 |> Seq.map ((*) 0.05f) |> Seq.map (lerpColor (230, 67, 55) (79, 76, 176)) |> Seq.toList
        background bg_mountains_sunset timeStart timeEnd
        >> background blurred timeStart (t "03:59:412")
        >>= fade timeStart (t "03:59:412") 1f 0f
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColors comboGrad) (t "03:57:279") timeEnd
        >>= PerNoteEffect.pingEffect pingGen (t "03:57:279") timeEnd
        >>= FFTEffects.bottomFft timeStart timeEnd
        >>= PerNoteEffect.rayEffect (PerNoteEffect.lightParamsOfColors comboGrad) (t "03:57:279") timeEnd
        >>= Transition.chromoFlash (t "04:45:346") (t "04:46:879") (t "04:47:412") 5000
        >>= Transition.chromoFlash (t "04:45:879") (t "04:47:412") (t "04:47:412") 5000

    let chillSection =
        let lyrics = [ t "04:55:946", "Dark minds, hearts of fire";
                       t "04:58:079", "Prisoners of this city";
                       t "05:00:479", "In the world alone";
                       t "05:02:346", "Find a way to escape" ]
        let slightlyBlurred = bg_campfire |> ImageFilters.gaussBlur 25f
        let blurred = bg_campfire |> ImageFilters.gaussBlur 55f
        background bg_campfire (t "04:47:412") (t "05:04:746")
        >> background blurred (t "04:47:412") (t "04:51:679")
        >>= fade (t "04:47:412") (t "04:53:679") 1f 0f
        >> background slightlyBlurred (t "04:47:412") (t "04:55:946")
        >>= fade (t "04:51:679") (t "04:55:946") 1f 0f
        >>= PerNoteEffect.pingEffect pingGen (t "05:02:346") (t "05:04:746")
        >>= stdLyrics lyrics

    let withABurningHeartLast t1 t2 sb =
        let text = "WITH A BURNING HEART"
        let text2 = "Eyes of fire, cold as ice"
        let position1 = centreOfText font_text_me_one text 0.22f +++ (0, -20)
        let position2 = centreOfText font_text_me_one text2 0.18f +++ (0, 20)
        let c1 = (190, 90, 33)
        let c2 = (160, 60, 33)
        let c3 = (112, 255, 241)
        let c4 = (36, 204, 237)
        let eff1 = (neonInOut 4000 t1 c1 c2)
        let eff2 = (neonInOut 2000 t2 c3 c4)
        (TextUtils.text font_text_me_one text eff1 position1 0.22f
        >>= TextUtils.text font_text_me_one text2 eff2 position2 0.18f) sb

    let lastBurningHeart =
        let timeStart, timeEnd = (t "05:47:146"), (t "05:55:946")
        let blurred = bg_desert_sunset |> ImageFilters.gaussBlur 5f
        Transition.dim (t "05:03:479") (t "05:04:479") 1000
        >>= background square_black (t "05:03:479") (t "05:09:812")
        >>= withABurningHeartLast (t "05:04:746") (t "05:06:612")
        >>= chorus (t "05:09:012") 0 false
        >>= Transition.blackCurtains (t "05:25:279") (t "05:26:879") (t "05:26:879") (t "05:28:946")
        >>= chorus (t "05:26:079") 0 true
        >>= Transition.blackCurtains (t "05:46:079") (t "05:47:146") (t "05:47:146") (t "05:50:212")
        >>= background bg_desert_sunset timeStart timeEnd
        >> background blurred timeStart (t "05:55:946")
        >>= fade (t "05:47:146") (t "05:49:279") 1f 0f
        >>= FFTEffects.circleFft timeStart timeEnd
        >>= Transition.dim (t "05:52:346") (t "05:55:986") ((t "05:55:986") - (t "05:52:346"))

    let signature =
        let titleCenter = centreOfText font_monosans "Dragonforce - Burning Heart" 0.3f
        text font_monosans "Dragonforce - Burning Heart" (neonInOut 8500 (t "00:00:480") (255, 130, 90) (160, 60, 33)) titleCenter 0.3f
        >>= text font_monosans "Map by: " (neonInOut 4300 (t "00:04:746") (192, 192, 192) (0, 0, 0)) (70, 320) 0.15f
        >>= text font_monosans "Rose Winters" (neonInOut 4300 (t "00:04:746") (112, 255, 241) (36, 204, 237)) (140, 320) 0.15f
        >>= text font_monosans "SB by: " (neonInOut 4300 (t "00:04:746") (192, 192, 192) (0, 0, 0)) (70, 350) 0.15f
        >>= text font_monosans "TheCatPetra" (neonInOut 4300 (t "00:04:746") (154, 219, 191) (87, 194, 180)) (125, 350) 0.15f

    let story =
        introSection
        >>= signature
        >>= fillerSection1
        >>= lyricsSection1
        >>= burningHeart (t "00:54:080")
        >>= chorus (t "01:11:146") 0 false
        >>= lyricsSection2
        >>= burningHeart (t "02:03:413")
        >>= chorus (t "02:20:480") (t "02:47:946" - t "02:45:813") false
        >>= soloSection1
        >>= soloSection2
        >>= chillSection
        >>= lastBurningHeart
        >>= background vignette 0 (t "05:55:946") >> layer Foreground

    let make () =
        openSb path
        |> story
        |> SbCompiler.write