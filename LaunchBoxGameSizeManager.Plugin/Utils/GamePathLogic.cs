using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unbroken.LaunchBox.Plugins.Data; // For IGame

namespace LaunchBoxGameSizeManager.Utils
{
    public static class GamePathLogic
    {
        // Case-insensitive HashSets for efficient lookups
        private static readonly HashSet<string> PcPlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Windows", "MS-DOS", "Linux" };

        private static readonly HashSet<string> RomBasedSingleFilePlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "Nintendo Entertainment System", "Super Nintendo Entertainment System", "Nintendo 64",
            "Sega Genesis", "Sega Mega Drive", "Sega Master System", "Nintendo Game Boy", "Nintendo Game Boy Color",
            "Nintendo Game Boy Advance", "Atari 2600", "ColecoVision", "Intellivision", "Neo Geo Pocket", "Neo Geo Pocket Color",
            "Sega Game Gear", "PC Engine", "TurboGrafx-16"
            // Add more platforms that are typically single ROM files
        };

        private static readonly HashSet<string> DiscImageBasedSingleFilePlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "Sony Playstation", "Sony Playstation 2", "Sega CD", "Sega Saturn", "Sega Dreamcast",
            "Nintendo GameCube", "Nintendo Wii", "Nintendo Wii U", "Microsoft Xbox", "Microsoft Xbox 360",
            "PC Engine CD", "TurboGrafx-CD"
        };

        // For GenericSubFolderNames, if we need case-insensitive contains often, a HashSet is better.
        // Otherwise, LINQ .Any with StringComparison is fine for a List.
        private static readonly HashSet<string> GenericSubFolderNamesHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "bin", "bins", "binaries", "win32", "win64", "x86", "x64", "system", "system32",
            "executable", "game", "dist", "data", "files", "retail", "shipping"
        };

        public static string GetReliableGamePath(IGame game, out string issueCategory, out string issueDetailMessage)
        {
            issueCategory = null;
            issueDetailMessage = null;
            string finalPathToScan = null;

            if (game == null)
            {
                issueCategory = "Internal Error";
                issueDetailMessage = "Game object was null.";
                return null;
            }

            string appPath = game.ApplicationPath;
            string rootFolder = game.RootFolder;
            string platform = game.Platform;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Determining path for: {game.Title} (Platform: {platform}, AppPath: '{appPath}', RootFolder: '{rootFolder}')");
#endif

            if (!string.IsNullOrEmpty(appPath))
            {
                if (appPath.StartsWith("steam://", StringComparison.OrdinalIgnoreCase) ||
                    appPath.StartsWith("com.epicgames.launcher://", StringComparison.OrdinalIgnoreCase) ||
                    appPath.StartsWith("amazon-games://", StringComparison.OrdinalIgnoreCase) ||
                    appPath.StartsWith("goggalaxy://", StringComparison.OrdinalIgnoreCase) ||
                    appPath.StartsWith("origin://", StringComparison.OrdinalIgnoreCase) ||
                    appPath.StartsWith("uplay://", StringComparison.OrdinalIgnoreCase))
                {
                    issueCategory = "Storefront Game";
                    issueDetailMessage = $"{game.Title} (Launcher URL: {appPath})";
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Storefront URL detected for {game.Title}.");
#endif
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(rootFolder))
            {
                if (Directory.Exists(rootFolder))
                {
                    if (!IsOverlyGenericRoot(rootFolder, platform, appPath))
                    {
                        finalPathToScan = rootFolder;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Using RootFolder for {game.Title}: {finalPathToScan}");
#endif
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] RootFolder '{rootFolder}' deemed too generic for {game.Title}, will analyze ApplicationPath.");
#endif
                    }
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic] RootFolder '{rootFolder}' for {game.Title} does not exist or is not a directory.");
#endif
                }
            }

            if (string.IsNullOrEmpty(finalPathToScan) && !string.IsNullOrEmpty(appPath))
            {
                if (File.Exists(appPath))
                {
                    if (RomBasedSingleFilePlatforms.Contains(platform) || // HashSet.Contains is case-insensitive due to constructor
                        (DiscImageBasedSingleFilePlatforms.Contains(platform) && ( // HashSet.Contains
                            appPath.EndsWith(".chd", StringComparison.OrdinalIgnoreCase) ||
                            appPath.EndsWith(".iso", StringComparison.OrdinalIgnoreCase) ||
                            appPath.EndsWith(".gdi", StringComparison.OrdinalIgnoreCase) ||
                            appPath.EndsWith(".rvz", StringComparison.OrdinalIgnoreCase) ||
                            appPath.EndsWith(".ciso", StringComparison.OrdinalIgnoreCase) ||
                            appPath.EndsWith(".wbfs", StringComparison.OrdinalIgnoreCase)
                        )))
                    {
                        finalPathToScan = appPath;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Using ApplicationPath (ROM/SingleDiscImage) for {game.Title}: {finalPathToScan}");
#endif
                    }
                    else if (appPath.EndsWith(".cue", StringComparison.OrdinalIgnoreCase))
                    {
                        finalPathToScan = appPath;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Using ApplicationPath (CUE sheet) for {game.Title}: {finalPathToScan}");
#endif
                    }
                    else if (PcPlatforms.Contains(platform)) // HashSet.Contains
                    {
                        string parentDir = Path.GetDirectoryName(appPath);
                        finalPathToScan = AscendToTrueGameRoot(parentDir, game.Title, 2);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] PC Platform. Ascended from AppPath. Final path for {game.Title}: {finalPathToScan ?? "null"}");
#endif
                    }
                    else
                    {
                        finalPathToScan = appPath;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Using ApplicationPath (Fallback File) for {game.Title}: {finalPathToScan}");
#endif
                    }
                }
                else if (Directory.Exists(appPath))
                {
                    if (PcPlatforms.Contains(platform) || // HashSet.Contains
                        (string.Equals(platform, "ScummVM Games", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(finalPathToScan)) ||
                        (string.Equals(platform, "Commodore Amiga", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(finalPathToScan) && appPath.IndexOf("WHDLoad", StringComparison.OrdinalIgnoreCase) >= 0)
                        )
                    {
                        finalPathToScan = appPath;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Using ApplicationPath (Direct Directory) for {game.Title}: {finalPathToScan}");
#endif
                    }
                    else
                    {
                        issueCategory = "Ambiguous Directory as ApplicationPath";
                        issueDetailMessage = $"{game.Title} (AppPath '{appPath}' is directory on platform '{platform}')";
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[GamePathLogic] {issueDetailMessage}. Path determination failed.");
#endif
                        return null;
                    }
                }
                else
                {
                    issueCategory = "Application Path Not Found";
                    issueDetailMessage = $"{game.Title} (ApplicationPath '{appPath}' does not exist)";
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic] {issueDetailMessage}. Path determination failed.");
#endif
                    return null;
                }
            }

            if (string.IsNullOrEmpty(finalPathToScan))
            {
                // If issueCategory wasn't set before, set a generic one now
                if (string.IsNullOrEmpty(issueCategory))
                {
                    issueCategory = "Path Not Determined";
                    issueDetailMessage = $"{game.Title} (Could not determine a valid path from RootFolder or ApplicationPath)";
                }
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[GamePathLogic] {issueDetailMessage}. Path determination failed for {game.Title}.");
#endif
                return null;
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[GamePathLogic] Final path determined for {game.Title}: {finalPathToScan}");
#endif
            return finalPathToScan;
        }

        private static bool IsOverlyGenericRoot(string folderPath, string platformName, string appPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return true;

            string normalizedPath = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string folderName = Path.GetFileName(normalizedPath);

            DirectoryInfo dirInfo = null;
            try { dirInfo = new DirectoryInfo(normalizedPath); } catch { return true; /* Invalid path */ }
            if (!dirInfo.Exists) return true;

            DirectoryInfo parentDirInfo = dirInfo.Parent;

            if (parentDirInfo == null) return true;

            string grandParentPath = parentDirInfo.Parent?.FullName;
            if ((string.Equals(folderName, "Games", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(folderName, "ROMs", StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(platformName) && string.Equals(folderName, platformName, StringComparison.OrdinalIgnoreCase)) ||
                 (string.Equals(parentDirInfo.Name, "LaunchBox", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(platformName) && string.Equals(folderName, platformName, StringComparison.OrdinalIgnoreCase)))
                && (grandParentPath == null || parentDirInfo.Parent.Parent == null))
            {
                if (!string.IsNullOrEmpty(appPath))
                {
                    try
                    {
                        string fullAppPath = Path.GetFullPath(appPath);
                        if (fullAppPath.StartsWith(normalizedPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        {
                            string relativeAppPath = fullAppPath.Substring(normalizedPath.Length).TrimStart(Path.DirectorySeparatorChar);
                            if (relativeAppPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length <= 2)
                            {
#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"[GamePathLogic.IsOverlyGenericRoot] RootFolder '{folderPath}' considered too generic (shallow appPath).");
#endif
                                return true;
                            }
                        }
                    }
                    catch { /* Invalid appPath, treat root as potentially generic */ }
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic.IsOverlyGenericRoot] RootFolder '{folderPath}' considered too generic (no appPath to qualify).");
#endif
                    return true;
                }
            }
            return false;
        }

        private static string AscendToTrueGameRoot(string currentDir, string gameTitle, int maxAscents)
        {
            if (string.IsNullOrEmpty(currentDir) || !Directory.Exists(currentDir))
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Initial directory '{currentDir}' is invalid for '{gameTitle}'.");
#endif
                return null;
            }

            string bestCandidatePath = currentDir;
            int ascentsMade = 0;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Starting ascent from '{currentDir}' for game '{gameTitle}'. Max ascents: {maxAscents}");
#endif

            string initialDirName = Path.GetFileName(currentDir);
            if (IsGoodTitleMatch(initialDirName, gameTitle) && !GenericSubFolderNamesHashSet.Contains(initialDirName)) // Use HashSet
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Initial directory '{initialDirName}' is a good title match and not generic. Considering it as root for '{gameTitle}'.");
#endif
                return currentDir;
            }

            string tempPath = currentDir;
            while (ascentsMade < maxAscents)
            {
                DirectoryInfo parentDirInfo = Directory.GetParent(tempPath);
                if (parentDirInfo == null)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Reached file system root or invalid parent from '{tempPath}' for '{gameTitle}'. Stopping ascent.");
#endif
                    break;
                }

                string parentName = parentDirInfo.Name;
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Ascended to '{parentDirInfo.FullName}'. Parent name: '{parentName}' for '{gameTitle}'.");
#endif

                if (IsGoodTitleMatch(parentName, gameTitle) && !GenericSubFolderNamesHashSet.Contains(parentName)) // Use HashSet
                {
                    bestCandidatePath = parentDirInfo.FullName;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Parent '{parentName}' is a good title match and not generic. New best candidate for '{gameTitle}': {bestCandidatePath}");
#endif
                    break;
                }

                string ascendedFromName = Path.GetFileName(tempPath);
                if (GenericSubFolderNamesHashSet.Contains(ascendedFromName)) // Use HashSet
                {
                    bestCandidatePath = parentDirInfo.FullName; // Parent of a generic folder is a better candidate
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Ascended from generic folder '{ascendedFromName}'. New best candidate for '{gameTitle}': {bestCandidatePath}");
#endif
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Parent '{parentName}' not a better title match, and ascended from non-generic '{ascendedFromName}'. Stopping ascent for '{gameTitle}'. Sticking with {bestCandidatePath}.");
#endif
                    break;
                }

                tempPath = parentDirInfo.FullName;
                ascentsMade++;
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[GamePathLogic.Ascend] Final best candidate for '{gameTitle}' after {ascentsMade} ascents: {bestCandidatePath}");
#endif
            return bestCandidatePath;
        }

        private static bool IsGoodTitleMatch(string folderName, string gameTitle)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(gameTitle)) return false;

            Func<string, string> normalize = s =>
                new string(s.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray()).Replace(" ", "").ToLowerInvariant();

            string normFolderName = normalize(folderName);
            string normGameTitle = normalize(gameTitle);

            if (string.IsNullOrEmpty(normFolderName)) return false;

            // Check if a significant part of the game title is in the folder name or vice-versa
            // e.g. "River City Ransom Underground" vs "RiverCityRansomUnderground" or "RiverCityRansom"
            if (normGameTitle.Contains(normFolderName) || normFolderName.Contains(normGameTitle))
            {
                // Add a length check to avoid trivial matches like "a" in "game"
                if (Math.Min(normFolderName.Length, normGameTitle.Length) > 3) // Require at least 4 common chars
                    return true;
            }

            // A slightly more lenient check: if folder name is a substring of title and folder name is reasonably long
            if (normGameTitle.Contains(normFolderName) && normFolderName.Length > 4) return true;
            if (normFolderName.Contains(normGameTitle) && normGameTitle.Length > 4) return true;


            return false;
        }
    }
}