using System;
using System.Collections.Generic;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using LaunchBoxGameSizeManager.Utils; // For Constants

namespace LaunchBoxGameSizeManager.Services
{
    public class LaunchBoxDataService
    {
        public IEnumerable<string> GetAllPlatformNames()
        {
            try
            {
                return PluginHelper.DataManager.GetAllPlatforms().Select(p => p.Name).OrderBy(name => name);
            }
            catch (Exception ex)
            {
                PluginHelper.LogHelper.Log($"Error getting all platform names: {ex.Message}", "LaunchBoxDataService", LogLevel.Error);
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<IGame> GetGamesForPlatform(string platformName)
        {
            try
            {
                var platform = PluginHelper.DataManager.GetPlatformByName(platformName);
                if (platform != null)
                {
                    return platform.GetAllGames(includeHidden: false, includeBroken: false);
                }
                PluginHelper.LogHelper.Log($"Platform not found: {platformName}", "LaunchBoxDataService", LogLevel.Warning);
                return Enumerable.Empty<IGame>();
            }
            catch (Exception ex)
            {
                PluginHelper.LogHelper.Log($"Error getting games for platform '{platformName}': {ex.Message}", "LaunchBoxDataService", LogLevel.Error);
                return Enumerable.Empty<IGame>();
            }
        }
        
        public string GetApplicationPath(IGame game)
        {
            return game.ApplicationPath;
        }

        public string GetPlatformName(IGame game)
        {
            return game.Platform;
        }

        public void UpdateGameSizeFields(IGame game, long sizeInBytes)
        {
            if (game == null) return;

            try
            {
                game.SetCustomField(Constants.CustomFieldCalculatedSize, sizeInBytes.ToString()); // Store as string, parse when reading if needed for math
                game.SetCustomField(Constants.CustomFieldCalculatedSizeFormatted, FormatHelpers.FormatBytes(sizeInBytes));
                game.SetCustomField(Constants.CustomFieldLastScanned, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                // Important: Save the game changes
                PluginHelper.DataManager.Save(); 
                // Note: Saving after every game can be slow for large batches.
                // Consider a bulk save at the end of a platform scan if performance is an issue.
                // However, PluginHelper.DataManager.Save() saves ALL data, so frequent calls might be okay
                // if LaunchBox handles it efficiently or if only changed data is written.
                // For now, save per game for simplicity and data safety if scan is interrupted.
            }
            catch (Exception ex)
            {
                PluginHelper.LogHelper.Log($"Error updating custom fields for game '{game.Title}': {ex.Message}", "LaunchBoxDataService", LogLevel.Error);
            }
        }
        
        public long GetGameSizeField(IGame game)
        {
            if (game == null) return -1;
            string sizeStr = game.GetCustomField(Constants.CustomFieldCalculatedSize);
            if (long.TryParse(sizeStr, out long size))
            {
                return size;
            }
            return -1; // Not found or not a valid long
        }

        // Stubs for Phase 2
        public IEnumerable<string> GetGameMediaPaths(IGame game)
        {
            // This will require investigating IGame properties for media
            // e.g., game.CoverImagePath, game.ScreenshotImagePath, etc.
            // Or if there's a method like game.GetAllMediaPaths()
            PluginHelper.LogHelper.Log($"GetGameMediaPaths called for {game.Title} - NOT IMPLEMENTED", "LaunchBoxDataService", LogLevel.Info);
            return Enumerable.Empty<string>();
        }

        public bool RemoveGameFromLaunchBox(IGame game)
        {
            // This requires investigating PluginHelper.DataManager for a remove/delete method
            // e.g., PluginHelper.DataManager.RemoveGame(game);
            PluginHelper.LogHelper.Log($"RemoveGameFromLaunchBox called for {game.Title} - NOT IMPLEMENTED", "LaunchBoxDataService", LogLevel.Info);
            return false; // Placeholder
        }
    }
}