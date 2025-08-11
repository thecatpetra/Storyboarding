﻿using MapsetParser.objects.events;
using MapsetParser.objects.hitobjects;
using MapsetParser.settings;
using MapsetParser.statics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MapsetParser.objects
{
    public class BeatmapSet
    {
        public List<Beatmap> beatmaps;
        public Osb           osb;

        public string       songPath;
        public List<string> songFilePaths  = new List<string>();

        /// <summary> Used hit sound files' relative path from the song folder. </summary>
        public List<string> hitSoundFiles;

        private struct BeatmapFile
        {
            public string name;
            public string code;

            public BeatmapFile(string name, string code)
            {
                this.name = name;
                this.code = code;
            }
        }

        public BeatmapSet(string beatmapSetPath)
        {
            Track mapsetTrack = new Track("Parsing mapset \"" + PathStatic.CutPath(beatmapSetPath) + "\"...");

            beatmaps   = new List<Beatmap>();
            osb        = null;
            songPath   = beatmapSetPath;

            Initalize(beatmapSetPath);

            Track hsTrack = new Track("Finding hit sound files...");
            hitSoundFiles = GetUsedHitSoundFiles().ToList();
            hsTrack.Complete();

            beatmaps =
                beatmaps
                    .OrderBy(beatmap => beatmap.generalSettings.mode)
                    .ThenBy(beatmap  => beatmap.GetDifficulty(true))
                    .ThenBy(beatmap  => beatmap.starRating)
                    .ThenBy(beatmap  => beatmap.GetObjectDensity()).ToList();

            mapsetTrack.Complete();
        }

        private void Initalize(string beatmapSetPath)
        {
            if (!Directory.Exists(beatmapSetPath))
                throw new DirectoryNotFoundException("The folder \"" + beatmapSetPath + "\" does not exist.");

            string[] filePaths = Directory.GetFiles(beatmapSetPath, "*.*", SearchOption.AllDirectories);

            List<BeatmapFile> beatmapFiles = new List<BeatmapFile>();
            foreach (string filePath in filePaths)
            {
                songFilePaths.Add(filePath);
                if (!filePath.EndsWith(".osu"))
                    continue;

                string fileName = filePath.Substring(songPath.Length + 1);
                string code = File.ReadAllText(filePath);

                beatmapFiles.Add(new BeatmapFile(fileName, code));
            }

            Beatmap.ClearCache();
            ConcurrentBag<Beatmap> concurrentBeatmaps = new ConcurrentBag<Beatmap>();
            Parallel.ForEach(beatmapFiles, beatmapFile =>
            {
                Track beatmapTrack = new Track("Parsing " + beatmapFile.name + "...");
                concurrentBeatmaps.Add(new Beatmap(beatmapFile.code, starRating: null, songPath, beatmapFile.name));
                beatmapTrack.Complete();
            });

            foreach (Beatmap beatmap in concurrentBeatmaps)
                beatmaps.Add(beatmap);

            string expectedOsbFileName = GetOsbFileName()?.ToLower();
            foreach (string filePath in filePaths)
            {
                string currentFileName = filePath.Substring(songPath.Length + 1);
                if (filePath.EndsWith(".osb") && currentFileName.ToLower() == expectedOsbFileName)
                {
                    Track osbTrack = new Track("Parsing " + currentFileName + "...");
                    osb = new Osb(File.ReadAllText(filePath));
                    osbTrack.Complete();
                }
            }
        }

        /// <summary> Returns the expected .osb file name based on the metadata of the first beatmap if any exists, otherwise null. </summary>
        public string GetOsbFileName()
        {
            MetadataSettings settings = beatmaps.FirstOrDefault()?.metadataSettings;
            if (settings == null)
                return null;

            string songArtist     = settings.GetFileNameFiltered(settings.artist);
            string songTitle      = settings.GetFileNameFiltered(settings.title);
            string songCreator    = settings.GetFileNameFiltered(settings.creator);
                
            return songArtist + " - " + songTitle + " (" + songCreator + ").osb";
        }

        /// <summary> Returns the full audio file path of the first beatmap in the set if one exists, otherwise null. </summary>
        public string GetAudioFilePath()
        {
            return beatmaps.FirstOrDefault(beatmap => beatmap != null)?.GetAudioFilePath() ?? null;
        }

        /// <summary> Returns the audio file name of the first beatmap in the set if one exists, otherwise null. </summary>
        public string GetAudioFileName()
        {
            return beatmaps.FirstOrDefault(beatmap => beatmap != null)?.generalSettings.audioFileName ?? null;
        }

        /// <summary> Returns the last file path matching the given search pattern, relative to the song folder.
        /// The search pattern allows two wildcards: * = 0 or more, ? = 0 or 1. </summary>
        private string GetLastMatchingFilePath(string searchPattern)
        {
            var lastMatchingPath = Directory.EnumerateFiles(songPath, searchPattern, SearchOption.AllDirectories).LastOrDefault();
            if (lastMatchingPath == null)
                return null;

            return PathStatic.RelativePath(lastMatchingPath, songPath).Replace("\\", "/");
        }

        /// <summary> Returns all used hit sound files in the folder. </summary>
        public IEnumerable<string> GetUsedHitSoundFiles()
        {
            IEnumerable<string> hsFileNames = beatmaps.SelectMany(beatmap => beatmap.hitObjects.SelectMany(hitObject => hitObject.GetUsedHitSoundFileNames())).Distinct();

            foreach (string hsFileName in hsFileNames)
            {
                var path = GetLastMatchingFilePath($"*{hsFileName}.*");
                if (path == null)
                    continue;

                yield return path;
            }
        }

        /// <summary> Returns whether the given full file path is used by the beatmapset. </summary>
        public bool IsFileUsed(string filePath)
        {
            string relativePath = PathStatic.RelativePath(filePath, songPath);
            string fileName     = relativePath.Split(new char[] { '/', '\\' }).Last().ToLower();
            string parsedPath   = PathStatic.ParsePath(relativePath);
            string strippedPath = PathStatic.ParsePath(relativePath, withoutExtension: true);

            if (beatmaps.Any(beatmap => beatmap.generalSettings.audioFileName.ToLower() == parsedPath))
                return true;

            // When the path is "go", and "go.png" is over "go.jpg" in order, then "go.jpg" will be the one used.
            // So we basically want to find the last path which matches the name.
            string lastMatchingPath = PathStatic.ParsePath(GetLastMatchingFilePath(parsedPath));

            // These are always used, but you won't be able to update them unless they have the right format.
            if (fileName.EndsWith(".osu"))
                return true;
            
            if (beatmaps.Any(beatmap =>
                    beatmap.sprites     .Any(element => element.path.ToLower() == parsedPath) ||
                    beatmap.videos      .Any(element => element.path.ToLower() == parsedPath) ||
                    beatmap.backgrounds .Any(element => element.path.ToLower() == parsedPath) ||
                    beatmap.animations  .Any(element => element.path.ToLower() == parsedPath) ||
                    beatmap.samples     .Any(element => element.path.ToLower() == parsedPath)))
                return true;

            // animations cannot be stripped of their extension
            if (beatmaps.Any(beatmap =>
                    beatmap.sprites     .Any(element => element.strippedPath == strippedPath) ||
                    beatmap.videos      .Any(element => element.strippedPath == strippedPath) ||
                    beatmap.backgrounds .Any(element => element.strippedPath == strippedPath) ||
                    beatmap.samples     .Any(element => element.strippedPath == strippedPath)) &&
                    parsedPath == lastMatchingPath)
                return true;

            if (osb != null && (
                    osb.sprites     .Any(element => element.path.ToLower() == parsedPath) || 
                    osb.videos      .Any(element => element.path.ToLower() == parsedPath) ||
                    osb.backgrounds .Any(element => element.path.ToLower() == parsedPath) ||
                    osb.animations  .Any(element => element.path.ToLower() == parsedPath) ||
                    osb.samples     .Any(anElement => anElement.path.ToLower() == parsedPath)))
                return true;

            if (osb != null && (
                    osb.sprites     .Any(element => element.strippedPath == strippedPath) ||
                    osb.videos      .Any(element => element.strippedPath == strippedPath) ||
                    osb.backgrounds .Any(element => element.strippedPath == strippedPath) ||
                    osb.samples     .Any(element => element.strippedPath == strippedPath)) &&
                    parsedPath == lastMatchingPath)
                return true;

            if (beatmaps.Any(beatmap => beatmap.hitObjects.Any(hitObject =>
                    (hitObject.filename != null ? PathStatic.ParsePath(hitObject.filename, true) : null) == strippedPath)))
                return true;

            if (hitSoundFiles.Any(hsPath => PathStatic.ParsePath(hsPath) == parsedPath))
                return true;

            if (SkinStatic.IsUsed(fileName, this))
                return true;

            if (fileName == GetOsbFileName().ToLower() && osb.IsUsed())
                return true;

            foreach (Beatmap beatmap in beatmaps)
                if (IsAnimationPathUsed(parsedPath, beatmap.animations))
                    return true;

            if (osb != null && IsAnimationPathUsed(parsedPath, osb.animations))
                return true;

            return false;
        }

        /// <summary> Returns whether the given path (case insensitive) is used by any of the given animations. </summary>
        private bool IsAnimationPathUsed(string filePath, List<Animation> animations)
        {
            foreach (Animation animation in animations)
                foreach (string framePath in animation.framePaths)
                    if (framePath.ToLower() == filePath.ToLower())
                        return true;

            return false;
        }

        /// <summary> Returns the beatmapset as a string in the format "Artist - Title (Creator)". </summary>
        public override string ToString()
        {
            if (beatmaps.Count > 0)
            {
                MetadataSettings settings = beatmaps.First().metadataSettings;

                string songArtist     = settings.GetFileNameFiltered(settings.artist);
                string songTitle      = settings.GetFileNameFiltered(settings.title);
                string songCreator    = settings.GetFileNameFiltered(settings.creator);

                return songArtist + " - " + songTitle + " (" + songCreator + ")";
            }
            return "No beatmaps in set.";
        }
    }
}
