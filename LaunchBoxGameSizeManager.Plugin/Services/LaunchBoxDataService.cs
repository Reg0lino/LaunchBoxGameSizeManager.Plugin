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
        // Special value for sizeInBytes to indicate not to store actual size/tier fields,
        // but other fields like LastScanned can still be processed.
        public const long DO_NOT_STORE_SIZE_CODE = long.MinValue;

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

        public void UpdateGameSizeFields(IGame game, long sizeInBytes, bool storeGameSize, bool storeLastScanned, bool storeSizeTier)
        {
            if (game == null) return;

            try
            {
                bool changed = false;
                ICustomField[] customFields = game.GetAllCustomFields();

                // --- Store "Game Size" ---
                if (storeGameSize)
                {
                    string valueToStoreForGameSize = string.Empty;
                    bool shouldUpdateGameSize = false;

                    if (sizeInBytes == DO_NOT_STORE_SIZE_CODE)
                    {
                        // If size calculation was skipped/errored, and user wants to "store" it,
                        // we effectively clear it or set to a non-value like "N/A"
                        // For this iteration, let's set it to empty or a placeholder if it exists,
                        // or don't create it if it doesn't.
                        // A simple approach: if it's an issue, we don't set a size value.
                        // If the field already existed with a value, we could clear it here.
                        ICustomField existingGameSizeField = customFields?.FirstOrDefault(f => f.Name == Constants.CustomFieldGameSize);
                        if (existingGameSizeField != null && !string.IsNullOrEmpty(existingGameSizeField.Value))
                        {
                            // Optionally clear it, or set to a specific placeholder like "N/A"
                            // For now, let's just not update it with a new valid size.
                            // If you want to explicitly clear it:
                            // existingGameSizeField.Value = ""; changed = true;
                        }
                        // No positive size to set, so shouldUpdateGameSize remains false unless we decide to clear.
                    }
                    else // sizeInBytes is a valid size (>=0)
                    {
                        valueToStoreForGameSize = FormatHelpers.FormatBytes(sizeInBytes);
                        shouldUpdateGameSize = true;
                    }

                    if (shouldUpdateGameSize)
                    {
                        ICustomField gameSizeField = customFields?.FirstOrDefault(f => f.Name == Constants.CustomFieldGameSize);
                        if (gameSizeField == null)
                        {
                            gameSizeField = game.AddNewCustomField();
                            gameSizeField.Name = Constants.CustomFieldGameSize;
                            gameSizeField.Value = valueToStoreForGameSize;
                            changed = true;
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] ADDED custom field '{Constants.CustomFieldGameSize}' for game '{game.Title}' with value '{valueToStoreForGameSize}'.");
#endif
                        }
                        else if (gameSizeField.Value != valueToStoreForGameSize)
                        {
                            gameSizeField.Value = valueToStoreForGameSize;
                            changed = true;
                        }
                    }
                }

                // --- Store "Game Size Last Scanned" ---
                if (storeLastScanned)
                {
                    string dateString = DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    ICustomField lastScannedField = customFields?.FirstOrDefault(f => f.Name == Constants.CustomFieldLastScanned);
                    if (lastScannedField == null)
                    {
                        lastScannedField = game.AddNewCustomField();
                        lastScannedField.Name = Constants.CustomFieldLastScanned;
                        lastScannedField.Value = dateString;
                        changed = true;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] ADDED custom field '{Constants.CustomFieldLastScanned}' for game '{game.Title}' with value '{dateString}'.");
#endif
                    }
                    else if (lastScannedField.Value != dateString)
                    {
                        lastScannedField.Value = dateString;
                        changed = true;
                    }
                }

                // --- Store "Game Size Tier" ---
                if (storeSizeTier)
                {
                    string valueToStoreForSizeTier = string.Empty;
                    bool shouldUpdateSizeTier = false;

                    if (sizeInBytes == DO_NOT_STORE_SIZE_CODE)
                    {
                        // Similar to Game Size, if calculation skipped/errored, don't set a tier.
                        // Optionally clear existing tier.
                        // ICustomField existingSizeTierField = customFields?.FirstOrDefault(f => f.Name == Constants.CustomFieldGameSizeTier);
                        // if (existingSizeTierField != null && !string.IsNullOrEmpty(existingSizeTierField.Value)) { /* existingSizeTierField.Value = ""; changed = true; */ }
                    }
                    else // sizeInBytes is a valid size (>=0)
                    {
                        valueToStoreForSizeTier = SizeTierGenerator.GetSizeTier(sizeInBytes);
                        shouldUpdateSizeTier = true;
                    }

                    if (shouldUpdateSizeTier)
                    {
                        ICustomField gameSizeTierField = customFields?.FirstOrDefault(f => f.Name == Constants.CustomFieldGameSizeTier);
                        if (gameSizeTierField == null)
                        {
                            gameSizeTierField = game.AddNewCustomField();
                            gameSizeTierField.Name = Constants.CustomFieldGameSizeTier;
                            gameSizeTierField.Value = valueToStoreForSizeTier;
                            changed = true;
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] ADDED custom field '{Constants.CustomFieldGameSizeTier}' for game '{game.Title}' with value '{valueToStoreForSizeTier}'.");
#endif
                        }
                        else if (gameSizeTierField.Value != valueToStoreForSizeTier)
                        {
                            gameSizeTierField.Value = valueToStoreForSizeTier;
                            changed = true;
                        }
                    }
                }

                if (changed)
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

        public string GetFormattedGameSizeField(IGame game)
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

        public void ClearGameSizePluginFields(IGame game)
        {
            if (game == null) return;
            bool changed = false;
            try
            {
                ICustomField[] customFields = game.GetAllCustomFields();

                string[] fieldsToRemove = {
                    Constants.CustomFieldGameSize,
                    Constants.CustomFieldLastScanned,
                    Constants.CustomFieldGameSizeTier
                };

                foreach (string fieldName in fieldsToRemove)
                {
                    // Important: Need to re-fetch the field from the potentially modified collection 
                    // if TryRemoveCustomField modifies the underlying source of GetAllCustomFields immediately.
                    // However, usually you operate on the initially fetched list.
                    // A safer loop if removal modifies the collection being iterated:
                    // for (int i = customFields.Length - 1; i >= 0; i--)
                    // {
                    //    if (fieldsToRemove.Contains(customFields[i].Name)) { /* remove customFields[i] */ }
                    // }
                    // For now, FirstOrDefault should be fine as we process one by one.

                    ICustomField field = customFields?.FirstOrDefault(f => f.Name == fieldName);
                    if (field != null)
                    {
                        if (game.TryRemoveCustomField(field))
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] Removed custom field '{fieldName}' for game '{game.Title}'.");
#endif
                            changed = true;
                        }
                        else
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[LaunchBoxDataService] FAILED to remove custom field '{fieldName}' for game '{game.Title}'.");
#endif
                        }
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