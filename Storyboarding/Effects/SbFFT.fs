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
        let sg = SpectrogramGenerator(sampleRate, fftSize=4096, stepSize=500, maxFreq=3000)
        sg.Add(audio, process=false)
        let processed = sg.Process()
        let getValue (time : Time) (freq : float32) =
            let x = ((float32 time - 500f) / (float32 length)) * (float32 processed.Length) |> int
            let y = freq * (float32 processed[0].Length) |> int
            20f * MathF.Log(processed[x][y] |> float32, 5f)
        f getValue sb
