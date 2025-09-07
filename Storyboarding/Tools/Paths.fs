namespace Storyboarding.Tools

open System.IO

module Paths =
    let projectPath x = Path.Join(@"/Users/arthur/Documents/Storyboarding/Storyboarding", x)

    let soundPath = projectPath @"/Resources/sound/bonk_ex.wav"
    let resourcesFolder = projectPath @"/Resources/"
    let scriptPath = projectPath @"/Scripts"
    let savePathPrefix = projectPath @"/Resources/font"
    let fontsFolder = projectPath @"/Fonts"
    let codeFolder = projectPath @"/Resources/code"
