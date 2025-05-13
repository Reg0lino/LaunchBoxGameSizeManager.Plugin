// --- START OF FILE Services/LaunchBoxDataService.cs ---
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using LaunchBoxGameSizeManager.Utils;

namespace LaunchBoxGameSizeManager.Services
{
    public class LaunchBoxDataService
    {
        public const long DO_NOT_STORE_SIZE_CODE = long.MinValue; // For local calculation issues

        // Helper method to set or add a custom field
        private bool SetCustomFieldValue(IGame game, string fieldName, string fieldValue)
        {
            if (game == null || string.IsNullOrEmpty(fieldName)) return false;

            ICustomField fieldToUpdate = null;
            var customFields = game.GetAllCustomFields(); // Get current fields
            if (customFields != null)
            {
                fieldToUpdate = customFields.FirstOrDefault(cf => cf.Name == fieldName);
            }

            if (fieldValue == null) // If trying to set a null value, treat as removal
            {
                return RemoveCustomField(game, fieldName);
            }

            if (fieldToUpdate != null)
            {
                if (fieldToUpdate.Value != fieldValue)
                {
                    fieldToUpdate.Value = fieldValue;
                    return true; // Value changed
                }
                return false; // Value was already same
            }
            else
            {
                // Only add if there's a non-null value to set
                var newField = game.AddNewCustomField();
                newField.Name = fieldName;
                newField.Value = fieldValue;
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] ADDED custom field '{fieldName}' for game '{game.Title}' with value '{fieldValue}'.");
#endif
                return true; // New field added
            }
        }

        // Helper method to remove a custom field
        private bool RemoveCustomField(IGame game, string fieldName)
        {
            if (game == null || string.IsNullOrEmpty(fieldName)) return false;

            var customFields = game.GetAllCustomFields();
            if (customFields == null) return false;

            ICustomField fieldToRemove = customFields.FirstOrDefault(cf => cf.Name == fieldName);
            if (fieldToRemove != null)
            {
                if (game.TryRemoveCustomField(fieldToRemove))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Removed custom field '{fieldName}' for game '{game.Title}'.");
#endif
                    return true; // Field removed
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] FAILED to remove custom field '{fieldName}' for game '{game.Title}'.");
#endif
                }
            }
            return false; // Field not found or failed to remove
        }


        // MODIFIED METHOD
        public void UpdateGameSizeFields(IGame game,
                                         long localSizeInBytes,
                                         long? estRequiredSpaceInBytes, // New parameter for storefront size
                                         bool storeGameSize,
                                         bool storeLastScanned,
                                         bool storeSizeTier,
                                         bool storeEstRequiredSpace) // New flag to control storing est. space
        {
            if (game == null) return;

            bool anyFieldChanged = false;

            try
            {
                // --- Store "Game Size" (Local Scan) ---
                if (storeGameSize)
                {
                    if (localSizeInBytes != DO_NOT_STORE_SIZE_CODE) // Valid local size calculated
                    {
                        string formattedSize = FormatHelpers.FormatBytes(localSizeInBytes);
                        if (SetCustomFieldValue(game, Constants.CustomFieldGameSize, formattedSize))
                            anyFieldChanged = true;
                    }
                    else // Issue with local calculation, clear existing local size field
                    {
                        if (RemoveCustomField(game, Constants.CustomFieldGameSize))
                            anyFieldChanged = true;
                    }
                }

                // --- Store "Game Size Tier" (Based on Local Scan) ---
                if (storeSizeTier)
                {
                    if (localSizeInBytes != DO_NOT_STORE_SIZE_CODE) // Valid local size calculated
                    {
                        string tier = SizeTierGenerator.GetSizeTier(localSizeInBytes);
                        if (SetCustomFieldValue(game, Constants.CustomFieldGameSizeTier, tier))
                            anyFieldChanged = true;
                    }
                    else // Issue with local calculation, clear existing tier field
                    {
                        if (RemoveCustomField(game, Constants.CustomFieldGameSizeTier))
                            anyFieldChanged = true;
                    }
                }

                // --- Store "Est. Required Space" (Storefront API) ---
                if (storeEstRequiredSpace) // Only process if user opted to store this
                {
                    if (estRequiredSpaceInBytes.HasValue) // API call was successful and returned a value
                    {
                        string formattedEstSize = FormatHelpers.FormatBytes(estRequiredSpaceInBytes.Value);
                        if (SetCustomFieldValue(game, Constants.CustomFieldEstRequiredSpace, formattedEstSize))
                            anyFieldChanged = true;
                    }
                    else // API call failed, was not attempted for this game, or user didn't opt-in via checkbox initially
                    {
                        // Always try to remove if no valid value, to clean up old entries or ensure it's not present.
                        if (RemoveCustomField(game, Constants.CustomFieldEstRequiredSpace))
                            anyFieldChanged = true;
                    }
                }
                else // User did not check the box for "Est. Required Space" for this scan run
                {
                    // Optionally, you could decide to *not* clear it here if the user simply didn't ask to update it.
                    // However, to keep logic simple: if not storing, ensure it's not there from a previous run.
                    // Or, only clear if estRequiredSpaceInBytes was explicitly null due to a failed *attempted* fetch.
                    // For now, let's be explicit: if storeEstRequiredSpace is false, we don't touch this field.
                    // The main plugin logic should pass null for estRequiredSpaceInBytes if fetchStorefrontSize was false.
                }


                // --- Store "Game Size Last Scanned" ---
                // This is always updated if any scan attempt (local or storefront) was made for the game,
                // regardless of success or which fields were selected for storage, as long as storeLastScanned is true.
                if (storeLastScanned)
                {
                    string dateString = DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    if (SetCustomFieldValue(game, Constants.CustomFieldLastScanned, dateString))
                        anyFieldChanged = true;
                }

                if (anyFieldChanged)
                {
                    PluginHelper.DataManager.Save(true);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Custom fields saved for game '{game.Title}'.");
#endif
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Error updating custom fields for game '{game.Title}': {ex.Message} - {ex.StackTrace}");
#endif
            }
        }

        // MODIFIED METHOD
        public void ClearGameSizePluginFields(IGame game)
        {
            if (game == null) return;
            bool changed = false;
            try
            {
                // Add the new custom field to the list of fields to remove
                string[] fieldsToRemove = {
                    Constants.CustomFieldGameSize,
                    Constants.CustomFieldLastScanned,
                    Constants.CustomFieldGameSizeTier,
                    Constants.CustomFieldEstRequiredSpace // Added new field
                };

                foreach (string fieldName in fieldsToRemove)
                {
                    if (RemoveCustomField(game, fieldName))
                    {
                        changed = true;
                    }
                }

                if (changed)
                {
                    PluginHelper.DataManager.Save(true);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Saved changes after clearing fields for game '{game.Title}'.");
#endif
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Error clearing custom fields for game '{game.Title}': {ex.Message} - {ex.StackTrace}");
#endif
            }
        }

        // --- UNCHANGED METHODS BELOW (for completeness, but no modifications needed here) ---
        public IEnumerable<string> GetAllPlatformNames()
        {
            try
            {
                IPlatform[] platforms = PluginHelper.DataManager.GetAllPlatforms();
                return platforms?.Select(p => p.Name).OrderBy(name => name) ?? Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Error getting all platform names: {ex.Message} - {ex.StackTrace}");
#endif
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<IGame> GetGamesForPlatform(string platformName)
        {
            try
            {
                IPlatform platform = PluginHelper.DataManager.GetPlatformByName(platformName);
                if (platform != null)
                {
                    return platform.GetAllGames(includeHidden: false, includeBroken: false);
                }
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Platform not found: {platformName}");
#endif
                return Enumerable.Empty<IGame>();
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Error getting games for platform '{platformName}': {ex.Message} - {ex.StackTrace}");
#endif
                return Enumerable.Empty<IGame>();
            }
        }

        public string GetApplicationPath(IGame game)
        {
            return game?.ApplicationPath;
        }

        public string GetPlatformName(IGame game)
        {
            return game?.Platform;
        }

        public string GetFormattedGameSizeField(IGame game) // This method remains as is
        {
            if (game == null) return string.Empty;
            try
            {
                ICustomField[] customFields = game.GetAllCustomFields();
                ICustomField sizeField = customFields?.FirstOrDefault(f => f.Name == Constants.CustomFieldGameSize);
                return sizeField?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Error getting custom field '{Constants.CustomFieldGameSize}' for game '{game.Title}': {ex.Message} - {ex.StackTrace}");
#endif
            }
            return string.Empty;
        }

        public IEnumerable<string> GetGameMediaPaths(IGame game)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] GetGameMediaPaths called for {game?.Title} - NOT IMPLEMENTED");
#endif
            return Enumerable.Empty<string>();
        }

        public bool RemoveGameFromLaunchBox(IGame game)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] RemoveGameFromLaunchBox called for {game?.Title} - NOT IMPLEMENTED");
#endif
            return false;
        }
    }
}
// --- END OF FILE Services/LaunchBoxDataService.cs ---