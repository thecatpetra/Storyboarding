namespace Storyboarding.Storyboards

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

module WithABurningHeart =
    // Fine map but i hate ranked
    let bad = @"C:\Users\arthur\AppData\Local\osu!\Songs\2182276 DragonForce - Burning Heart\DragonForce - Burning Heart (Maaadbot).osb"
    let path = @"C:\Users\arthur\AppData\Local\osu!\Songs\2388814 DragonForce - Burning Heart (feat Alissa White-Gluz)\DragonForce - Burning Heart (feat. Alissa White-Gluz) (Rose Winters).osb"

    let rayEffect =
        let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64)
        forEachHitObject (fun ho ->
        let rotation = (float32 (ho.Position.X - 256f)) / 800f
        img grad_ray
        >>= layer Layer.Foreground
        >>= move (int ho.time) (int ho.time) (p ho.Position) (p ho.Position)
        >>= vectorScale (int ho.time) (int ho.time) (3f, 20f) (3f, 20f)
        >>= fade (int ho.time) (int ho.time + 2000) 0.3f 0f
        >>= rotate (int ho.time) (int ho.time) rotation rotation
        >>= alpha
        >>= color (int ho.time) (int ho.time) (randColor()) (sameColor()))

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
        let position = TextUtils.centreOfText font_text_me_one text scale
        let c1 = (120 + 40 * i, 20 + 18 * i, 0)
        let c2 = (120 + 40 * i, 30 + 33 * i, 33 * i)
        let eff = (TextUtils.neonInOut 2000 (timeStart + i * diff) c1 c2)
        TextUtils.text font_text_me_one text eff position scale) sb

    let stdLyrics lyrics =
        monadicMap (Seq.toList lyrics) (fun l ->
        let time, txt = l
        let scale = 0.25f
        let position = TextUtils.centreOfText font_monosans txt scale
        let effect = TextUtils.chromoInOut 1000 25 time
        TextUtils.text font_monosans txt effect position scale)

    let stdLyrics2 lyrics =
        monadicMap (Seq.toList lyrics) (fun l ->
        let time, txt = l
        let scale = 0.3f
        let position = TextUtils.centreOfText font_monosans txt scale
        let effect = TextUtils.neonInOut 1300 time (255, 255, 128) (255, 0, 0)
        TextUtils.text font_monosans txt effect position scale)

    let pingGen (ho : HitObject) =
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

    let chorus timeStart extend =
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
        background bg_burning_city timeStart timeEnd
        >>= bgMovement timeStart (timeEnd + 300)
        >>= color timeStart timeStart (192, 192, 192) (192, 192, 192)
        >>= stdLyrics2 lyrics
        >>= RisingSmoke.effect timeStart timeEnd
        >>= RealisticSparks.effect soloStart timeEnd
        >>= PerNoteEffect.circleEffect (PerNoteEffect.lightParamsOfColor (250, 100, 30)) soloStart timeEnd
        >>= PerNoteEffect.pingEffect pingGen soloStart timeEnd
        >>= Transition.dim timeEnd nextSection 100

    let fftSquares timeStart timeEnd =
        squareFadeSequence timeStart timeEnd

    let story =
        introSection
        >>= fillerSection1
        >>= lyricsSection1
        >>= burningHeart (t "00:54:080")
        >>= chorus (t "01:11:146") 0
        >>= lyricsSection2
        >>= burningHeart (t "02:03:413")
        >>= chorus (t "02:20:480") (t "02:47:946" - t "02:45:813")
        // exp
        >>= fftSquares (t "02:47:946") (t "03:55:146")

    let make () =
        openSb path
        |> story
        |> SbCompiler.write