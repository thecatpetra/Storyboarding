namespace Storyboarding.Effects

open System
open System.Linq
open Spectrogram
open NAudio.Wave
open Storyboarding.Tools.SbMonad
open Storyboarding.Tools.SbTypes

module SbFFT =
    type FftResult = Time -> float32 -> float32

    let readMono (sb : SB) =
        let multiplier = 16000 |> float32
        let audio = sb.beatmapSet.beatmaps[0].GetAudioFilePath()
        use reader = new AudioFileReader(audio)
        let sampleRate = reader.WaveFormat.SampleRate
        let bytesPerSample = reader.WaveFormat.BitsPerSample / 8 |> Convert.ToInt64
        let sampleCount = (reader.Length / bytesPerSample) |> Convert.ToInt32
        let channelCount = reader.WaveFormat.Channels
        let audio = ResizeArray<double>(sampleCount)
        let buffer = Array.zeroCreate<float32> (sampleRate * channelCount)
        reader.Read(buffer, 0, buffer.Length) |> ignore
        let mutable samplesRead = 1
        while samplesRead > 0 do
            samplesRead <- reader.Read(buffer, 0, buffer.Length)
            audio.AddRange(buffer.Take(samplesRead).Select(fun x -> x * multiplier |> float))
        let length = reader.TotalTime.Ticks / TimeSpan.TicksPerMillisecond;
        audio.ToArray(), sampleRate, length


    let withFft (f : FftResult -> T) sb =
        let audio, sampleRate, length = readMono sb
        let sg = SpectrogramGenerator(sampleRate, fftSize=4096, stepSize=441, maxFreq=3000)
        sg.Add(audio, process=false)
        let processed = sg.Process()
        let collectArea x y xArea yArea =
            let inBounds s x = 0 <= x && x < s
            let xIndices = [x-xArea..x+xArea] |> List.filter (inBounds processed.Length)
            let yIndices = [y-yArea..y+yArea] |> List.filter (inBounds processed[0].Length)
            let areaCount = (xIndices |> List.length) * (yIndices |> List.length) |> float32
            let areaSum = List.fold (fun t1 x -> t1 + List.fold (fun t0 y -> t0 + (processed[x][y] |> float32)) 0f yIndices) 0f xIndices
            if (areaCount = 0f) then printfn "Warning! Very bad area!"
            areaSum / areaCount
        let getValue (time : Time) (freq : float32) =
            let x = ((float32 time - 500f) / (float32 length)) * (float32 processed.Length) |> int
            let y = freq * (float32 processed[0].Length) |> int
            let average = collectArea x y 4 2
            20f * MathF.Log(average, 3f)
        f getValue sb
