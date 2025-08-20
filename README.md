# ✨ Storyboarding ✨
Repository where I ([TheCatPetra](https://osu.ppy.sh/users/15764983)) do all my storyboards

## Basics of Storyboarding with F#
To make a storyboard with this toolset, one shall start with this code and call it in `Program.fs`.
```fsharp
let make () = openSb path |> story |> SbCompiler.write
```
Here, `story` function is the one that makes the storyboard and should be written manually, while `openSb` loads the map and creates initial state (effectively monadic `return`).

`SbCompiler.write` writes the constructed storyboard to osb and diffs if something is diff-specific, copies all files to destination folder, makes some asserts on type to ensure osu! will not make 1000 errors.

### Basic types
```fsharp
type Time = int
// Can be added and works like int, 1 = 1 millisecond
// Best used with function t : string -> Time that converts string like "01:32:201" to Time type
t "01:01:100" = 61100
lerpTime 100 200 0.3f = 130 // linear interpolation for time exists

type Position = int * int
// Supports operations +++, ---: Position -> Position -> Position
// ***: int -> Position -> Position
// lerp: Position -> Position -> float -> Position
(100, 100) +++ (100, 200) = (200, 300)       // elementwise addition / subtraction
8 *** (3, 3) = (24, 24)                      // multiplication by scalar
lerp (100, 100) (300, 300) 0.1f = (120, 120) // linear interpolation
length (3, 4) = 5                            // Length of vector from 0 0
type Color = int * int * int
// Represents color, every int is from 0 to 255, checked at storyboard compilation time
lerpColor // linear interpolation for colors
```
TODO: Change time type to units of measure to avoid confusion.


### Declaring behaviour
Building blocks for storyboard is a functions with signature `SB -> SB` or alias `SbMonad.T` and they shall be combined with the bind operator `>>=` which is essencially just function composition `>>`.

Each function takes the storyboard and return the modified copy.

Some basic storyboard functions are a subset of default [osu! storyboard commands](https://osu.ppy.sh/wiki/en/Storyboard/Scripting).
```fsharp
// Signatures
move : Time -> Time -> Position -> Position -> SbMonad.T
rotate : Time -> Time -> float -> float -> SbMonad.T
scale : Time -> Time -> float -> float -> SbMonad.T

// Usage with simple example
let squareWithBehaviour ts te =
	img square_white                        // put image square_white (declared in Resources)
	>>= color ts ts (255, 0, 0) (255, 0, 0) // then color it to red
	>>= move ts te (0, 0) (100, 100)        // then move from (0, 0) to (100, 100)
	>>= rotate ts te 0.1f 0.5f              // then rotate it from 0.1 to 0.5
```


### Monadic maps
For making a behaviour for set of elements, `monadicMap` and `monadicMapi` exist.
```fsharp
// Explanation
monadicMap [1..20] f = f 1 >>= f 2 >>= ... >>= f 20
monadicMapi [10..20] = f 0 10 >>= f 1 11 >>= ... >>= f 10 20

// Usage
let squareChromaticDecomposition ts te =
	monadicMapi [(255, 0, 0); (0, 255, 0); (0, 0, 255)] (fun index color ->
	// Will be executed with index = [0; 1; 2] and color = [red; green; blue]
	let initial = (240, 320)          // intitial position = center
	let direction = index - 1         // left (-1) / stay (0) / right (1)
	                |> (***) (100, 0) // make it (-100, 0) / (0, 0) / (100, 0)
	img square                                     // put image square_white 
	>>= move ts te initial (initial +++ direction) // move it from initial to initial + direction
	>>= color ts ts color color                    // color it to red / green / blue
	>>= fade ts te 1f 0f                           // fade object towards end
	>>= alpha)                                     // set blending to alpha for object
```

Also a common task is to do something for equally-divided time intervals. For this, `timeDivisionMap timeStart timeEnd timeStep` exists
```fsharp
// Explanation
timeDivisionMap 0 1000 20 f = f (0, 20) >>= f (20, 40) >>= f (40, 60) >>= ... >>= f (980, 1000)
timeDivisionMapi also exists (you can deduce what it does i hope)

// Usage
// Background movement from top to bottom and from bottom to top with 5000f each movement
let backgroundMovement backgound ts te =
	let interval = 5000
	let amplitude = 100f
	img backgound
	>>= timeDivisionMapi ts te interval (fun index (intervalStart, intervalStop) ->
	let position i = i % 2           // 0 and 1 for even and odd iterations
	                 |> float32          // to 0f and 1f
	                 |> (+) (-0.5f)      // to -0.5f and 0.5f
	                 |> (*) amplitude          // to -50f and 50f
	                 |> int              // to -50 and 50
	                 |> (***) (0, 1)     // to (0, -50) and (0, 50)
	                 |> (+++) (320, 240) // to (320, 190) and (320, 290)
	move intervalStart intervalStop (position index) (position (index + 1)))
```

### Diff-specific and hitobject-specific stuff
Sometimes, you want each difficulty to behave differently, for example write the name of the diff in the beginning or make effects for every circle.

For that, `forEachDiff` exists. It makes all sprites and command used inside it written to single `.osu` for each difficulty instead of shared `.osb`
```fsharp
// For example
forEachDiff (fun diff ->
let name = diff .metadataSettings.version  
TextUtils.text font_monosans name (TextUtils.chromoInOut 1000 100 20000) (100, 100) 0.2f)
// Will write text containing difficulty name for each file in set
```

Similar function exists if one wants to make an effect for each hitobject. Here, `forEachHitObject` shall be used.
```fsharp
// Utils
let p (ps : Vector2) : Position = (int ps.X + 64, int ps.Y + 64) 
// + 64 bc storyboard coords and objects coords differ for some goofy reason

// Executes code for each hit object in map
forEachHitObject (fun ho ->
img Resources.light                                      // put light texture
>>= layer Layer.Foreground                               // set layer of image to Foreground
>>= move ho.time ho.time (p ho.Position) (p ho.Position) // move it to position of ho
>>= scale ho.time (ho.time + 2000) 0.7f 0.4f             // scale it from 0.7 to 0.4 in 2 secs
>>= fade ho.time (ho.time + 2000) 0.3f 0f                // fade it from 0.3 to 0 in 2 secs
>>= alpha                                                // set blending to alpha
>>= color ho.time ho.time (255, 0, 0) (255, 0, 0))       // set color to red
```

All generic effects I use are in `Effects/PerNoteEffect.fs` module and have some quirks. These functions generally have signature containing start and end time and function `HitObject -> LightParam option` to decide on effect parameters such as color, blending, sizes.
```fsharp
// Interesting unctions in PerNoteEffect
onlyFirstCombo f ho // = f if ho is new combo else None
lightParamsOfColor c ho // = default light params with single color
lightParamOfCombo ho // = default light params with color = combo color (buggy for some reason)

// Making an effect
let kiaiEffects ts te =
  // makes light for each object with color of combo
	PerNoteEffect.circleEffect lightParamOfCombo ts te
	// makes every new combo have "ping" effect
	>>= PerNoteEffect.pingEffect (onlyFirstCombo lightPingOfCombo) ts te
```


To parse and use the map this project is used: [MapsetParser by MChecaH](https://github.com/MChecaH/MapsetParser)

### Working with text
Working with text is hard, that was the reason I made this toolset for me in the first place.

All the functions used are declared in `Tools/TextUtils.fs` module.

Function `text` renders text `txt` using the font `font` at position `position` with scale `scale` and with effect `charAction`.

CharAction is function with signature `CharAction: int -> string -> float32 -> Position -> SbMonad.T`
```fsharp
text: (font : string) ->
      (txt : string) ->
      (charAction : CharAction) ->
      (position : Position) ->
      (scale : float) -> SbMonad.T
// Effectively calling charAction for each symbol with correct index, image, scale and position
// Also renders font with resolution of 64 if font doesn't exist
// Contains the sad comment // TODO: Monadic map/fold ))

CharAction: (index : int) (glyphName : string) (scale : float32) (position : Position)
// Custom function that renders the glyph with specified effect
```
Some existing implemented effects
```fsharp
// Renders text to appear instantly and stay for 10 secs. More for debug
noEffect time -> CharAction

// Renders each glyph from (time + diff * index) to (time + diff * index + stay)
// Chromatic abberation as an effect R -> G <- B, then R <- G -> B
chromoInOut stay diff time -> CharAction
```
Usage
```fsharp
// Render text "Hello world" in the center of the screen at "00:13:025" with scale of 0.3f
// It will use chromoInOut, stay for 2000ms and have difference of 100ms for each char
let helloWorld : SbMonad.T =
	// Create effect with time = t "00:13:025"
	let effect = chromoInOut 2000 100 (t "00:13:25")
	// Util function to calculate text position in the center
	let position = textCenter font_monosans "Hello World!" 0.3f
	// Draw text
	text font_monosans "Hello World!" effect position 0.3f
```

TODO: Move time from action to text
TODO: Use unit of measure \<font\> for fonts not to confuse with strings 

Font rendering
When used, text generates font from .ttf automatically using fontforge and python scripts. Sometimes its a neccesity to render font manually. For example, japanese font with a lot of glyphs to select subset, or to get textures of high resolution, not 64px high.

The task is not common, I'll just drop this example here
```fsharp
do  
	// Collect all text to render (here 2 parts of lyrics + title)
	let allText = $"%A{lyrics}%A{lyrics2}【アポル】陽だまりの詩 歌ってみた-Hidamari no UtaApol"
	// Locate actual .ttf font
	let fontPath = Path.Join(fontsFolder, FileInfo(font_notosans_jp).Name)
	// Run wrapper for fontforge scripts with specified arguments
	// This renders font from fontPath with characters from allText and with resolution of 128px high
	createFontSubset fontPath allText 128
```


### FFT (Music spectrograms)

Sometimes its cool to create spectrogram of a song with fft series. For that, `withFft` exists
Its buggy, works strange, takes a lot of time to compute and may produce runtime exceptions, so be careful

`withFft f` lets you run function `f : FftResult -> SbMonad.T` where `FftResult: (time : Time) -> (freq : float32) -> float32` lets you sample the amplitude of frequency `freq` (from 0f to 1f) at the time `time` (actual map time)
```fsharp
// Example effect with bottom bars following the song frequencies
withFft (fun fft ->                          // Acquire FFT
monadicMap [1..50] (fun freq ->              // Make 50 bars
let position = (320 + (freq - 25) * 20, 480) // Position them from left to right
img square_white >> origin BottomCentre
>>= move timeStart timeStart position position
>>= alpha
>>= timeDivisionMap timeStart timeEnd timeStep (fun (time, endTime) -> // Run for each time period
let freqRequest = (float32 freq + 30f) / 120f // Convert from 1..50 to numbers in [0f..1f]
let fftRes = fft time freqRequest             // Get fft value for
  |> (+) -1.4f                                // GOoFy
  |> (*) 0.0081f                              // Ahh
  |> fun x -> MathF.Pow(x, 2f)                // Math to make it look cooler
vectorScaleTo time endTime (0.15f, fftRes)))) // Scale the bar
```

### Image manipulation

There are a lot of situations where image manipulation is needed, such as making background grayscale of blurred.
For this, module `ImageFilters.fs` exists

Using function from image filter generally applies filter to the image, saves it with a prefix and returns new name, so that you can use modified image and original one.

So, `bg_castle |> ImageFilters.gaussBlur 5f`, with bg_castle = "bg/castle.png" will render new image "bg/castle_b5.png" and return the image name.

Current common filters include
```fsharp
// Applies guassian blur to the image (suffix _b{modifier})
gaussBlur multiplier
// Makes image grayscale (suffix _gs)
grayscale
// Makes the image 1920x1080
resizeToFHD
```

### Overall structure
The solution exists of other projects that are used like Utils, and main project `Storyboarding.fs`
Structure:
- `Effects (sources)` -- common effects
 - `Fonts (resources)` -- .ttf font files
 - `Gamplay (sources) (wip)` -- utils for making custom gameplay
 - `Resources (resources)` -- main resource folder
 - `Resources/bg/*` -- backgrounds
 - `Resources/effects/*` -- effect textures such as lights flashes rain drops
 - `Resources/font/*` -- rendered font files
 - `Resources/gameplay/*` -- files for custom gameplay
 - `Scripts (python sources)` -- files for fontforge to work with fonts
 - `Storyboards (sources)` -- sources for actual storyboards
 - `Tools` -- main utils and tools and type definitions (types, monad, compiler, Resources)

