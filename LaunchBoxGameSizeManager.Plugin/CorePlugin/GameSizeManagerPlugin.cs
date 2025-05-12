using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using LaunchBoxGameSizeManager.Services;
using LaunchBoxGameSizeManager.UI;
using LaunchBoxGameSizeManager.Utils;

namespace LaunchBoxGameSizeManager.CorePlugin
{
    public class GameSizeManagerPlugin : ISystemMenuItemPlugin, IGameMultiMenuItemPlugin
    {
        // ... (Constructor, LoadIcon, ISystemMenuItemPlugin.OnSelected, IGameMultiMenuItemPlugin.GetMenuItems, 
        //      HandleCalculateGamesRequest, HandleClearGamesDataRequest - these methods remain unchanged from their previous full versions) ...
        // Ensure these are exactly as in the previous "full script" update where we introduced PlatformActionsDialog.
        private LaunchBoxDataService _lbDataService;
        private FileSystemService _fileSystemService;
        private System.Drawing.Image _icon;

        public GameSizeManagerPlugin()
        {
            _lbDataService = new LaunchBoxDataService();
            _fileSystemService = new FileSystemService();
            LoadIcon();
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] GameSizeManagerPlugin constructor finished.");
#endif
        }

        private void LoadIcon()
        {
            try
            {
                string resourceName = "LaunchBoxGameSizeManager.plugin_icon.png";
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        _icon = System.Drawing.Image.FromStream(stream);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Icon loaded successfully from: {resourceName}");
#endif
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Icon resource not found: {resourceName}. Ensure icon file is an Embedded Resource.");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Error loading icon: {ex.Message}");
#endif
                _icon = null;
            }
        }

        public string Caption => Constants.PluginName;
        public System.Drawing.Image IconImage => _icon;
        public bool ShowInLaunchBox => true;
        public bool ShowInBigBox => false;
        public bool AllowInBigBoxWhenLocked => false;

        public void OnSelected()
        {
            try
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Plugin selected via Tools menu.");
#endif
                IEnumerable<string> platformNames = _lbDataService.GetAllPlatformNames();
                if (platformNames == null || !platformNames.Any())
                {
                    PluginUIManager.ShowInformation(Constants.PluginName, "No platforms found in your LaunchBox library.");
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] No platforms found.");
#endif
                    return;
                }

                PlatformActionsDialog platformDialog = new PlatformActionsDialog(platformNames);
                if (platformDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPlatform = platformDialog.SelectedPlatformName;
                    IEnumerable<IGame> platformGamesEnumerable = _lbDataService.GetGamesForPlatform(selectedPlatform);
                    IGame[] platformGames = platformGamesEnumerable?.ToArray() ?? Array.Empty<IGame>();

                    if (platformDialog.UserAction == PlatformScanDialogAction.ScanPlatform)
                    {
                        if (platformGames.Any())
                        {
                            HandleCalculateGamesRequest(platformGames, $"platform '{selectedPlatform}'");
                        }
                        else
                        {
                            PluginUIManager.ShowInformation(Constants.PluginName, $"No games found on platform '{selectedPlatform}'.");
                        }
                    }
                    else if (platformDialog.UserAction == PlatformScanDialogAction.ClearDataForPlatform)
                    {
                        if (platformGames.Any())
                        {
                            HandleClearGamesDataRequest(platformGames, $"platform '{selectedPlatform}'");
                        }
                        else
                        {
                            PluginUIManager.ShowInformation(Constants.PluginName, $"No games found on platform '{selectedPlatform}' to clear data from.");
                        }
                    }
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Platform Actions dialog cancelled.");
#endif
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Error in Tools menu OnSelected: {ex.ToString()}");
#endif
                PluginUIManager.ShowError(Constants.PluginName, $"An unexpected error occurred in Tools menu action: {ex.Message}");
            }
        }

        public IEnumerable<IGameMenuItem> GetMenuItems(params IGame[] selectedGames)
        {
            if (selectedGames == null || selectedGames.Length == 0)
            {
                return Enumerable.Empty<IGameMenuItem>();
            }
            var menuItems = new List<IGameMenuItem>();
            menuItems.Add(new GameSizeMenuItem("Calculate Game Size(s)...", _icon, selectedGames,
                (games) => HandleCalculateGamesRequest(games, $"{games.Length} selected game(s)")));
            menuItems.Add(new GameSizeMenuItem("Clear Game Size Data", _icon, selectedGames,
                (games) => HandleClearGamesDataRequest(games, $"{games.Length} selected game(s)")));
            return menuItems;
        }

        private void HandleCalculateGamesRequest(IGame[] gamesToProcess, string contextDescription)
        {
            if (gamesToProcess == null || !gamesToProcess.Any())
            {
                PluginUIManager.ShowWarning(Constants.PluginName, $"No games provided for '{contextDescription}' to calculate size.");
                return;
            }
            ScanOptionsDialog optionsDialog = new ScanOptionsDialog();
            if (optionsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Scan options cancelled by user for: {contextDescription}");
#endif
                return;
            }
            bool storeGameSize = optionsDialog.StoreGameSize;
            bool storeLastScanned = optionsDialog.StoreLastScanned;
            bool storeSizeTier = optionsDialog.StoreSizeTier;
            if (!storeGameSize && !storeLastScanned && !storeSizeTier)
            {
                PluginUIManager.ShowInformation(Constants.PluginName, "No data fields selected to store. Scan aborted.");
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] No data fields selected by user for {contextDescription}. Scan aborted.");
#endif
                return;
            }
            ProcessGamesAndStoreData(gamesToProcess, contextDescription, storeGameSize, storeLastScanned, storeSizeTier);
        }

        private void HandleClearGamesDataRequest(IGame[] gamesToClear, string contextDescription)
        {
            if (gamesToClear == null || !gamesToClear.Any())
            {
                PluginUIManager.ShowWarning(Constants.PluginName, $"No games provided for '{contextDescription}' to clear data.");
                return;
            }
            string confirmMessage = $"Are you sure you want to remove all Game Size Manager data for {contextDescription}?\nThis will remove:\n" +
                                    $"- {Constants.CustomFieldGameSize}\n" +
                                    $"- {Constants.CustomFieldLastScanned}\n" +
                                    $"- {Constants.CustomFieldGameSizeTier}\n\nThis action cannot be undone.";
            if (PluginUIManager.ShowConfirmation("Confirm Clear", confirmMessage))
            {
                PluginUIManager.ShowInformation(Constants.PluginName, $"Starting data clear for {contextDescription}.");
                Task.Run(() =>
                {
                    int count = 0;
                    foreach (var game in gamesToClear)
                    {
                        count++;
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Clearing data for game {count}/{gamesToClear.Length}: {game.Title} (context: {contextDescription})");
#endif
                        _lbDataService.ClearGameSizePluginFields(game);
                    }
                }).ContinueWith(task => { /* ... (unchanged error/completion handling) ... */
                    if (task.IsFaulted)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Error clearing game size data for {contextDescription}: {task.Exception?.ToString()}");
#endif
                        PluginUIManager.ShowError(Constants.PluginName, "An error occurred while clearing game size data.");
                    }
                    else
                    {
                        PluginUIManager.ShowInformation(Constants.PluginName, $"Game Size Manager data cleared for {contextDescription}.");
                    }
                });
            }
            else { /* User cancelled */ }
        }

        // --- Main Processing Logic (MODIFIED to use GamePathLogic) ---
        private void ProcessGamesAndStoreData(IGame[] gamesToProcess, string contextDescription, bool storeGameSize, bool storeLastScanned, bool storeSizeTier)
        {
            int gameCount = gamesToProcess.Length;
            PluginUIManager.ShowInformation(Constants.PluginName, $"Starting size calculation for: {contextDescription}.\nThis may take a while.");

            Dictionary<string, List<string>> categorizedProblematicGames = new Dictionary<string, List<string>>();

            Task.Run(() =>
            {
                int currentProcessedCount = 0;
                foreach (IGame game in gamesToProcess)
                {
                    currentProcessedCount++;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Processing {currentProcessedCount}/{gameCount}: {game.Title} ({contextDescription})");
#endif

                    string issueCategoryForThisGame = null;
                    string issueDetailForThisGame = null;
                    long sizeResultForUpdate = LaunchBoxDataService.DO_NOT_STORE_SIZE_CODE;

                    // Use the new GamePathLogic
                    string pathToCheck = GamePathLogic.GetReliableGamePath(game, out issueCategoryForThisGame, out issueDetailForThisGame);

                    if (!string.IsNullOrEmpty(pathToCheck)) // GamePathLogic returned a path to check
                    {
                        long calculatedSize;
                        if (pathToCheck.EndsWith(".cue", StringComparison.OrdinalIgnoreCase) && File.Exists(pathToCheck))
                        {
                            calculatedSize = _fileSystemService.CalculateCueSheetAndRelatedFilesSize(pathToCheck);
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] CUE sheet size for {game.Title}: {FormatHelpers.FormatBytes(calculatedSize)} (Code: {calculatedSize})");
#endif
                        }
                        else
                        {
                            // CalculateDirectorySize now handles both files and directories
                            calculatedSize = _fileSystemService.CalculateDirectorySize(pathToCheck);
                        }

                        if (calculatedSize >= 0) // Valid size calculated
                        {
                            sizeResultForUpdate = calculatedSize;
                        }
                        else // Error during calculation from FileSystemService (-2 for error, -3 for path not found by FS)
                        {
                            issueCategoryForThisGame = "Error Calculating Size";
                            issueDetailForThisGame = $"{game.Title} (Path: {pathToCheck}, FS Error Code: {calculatedSize})";
                        }
                    }
                    // If pathToCheck was null, issueCategoryForThisGame/issueDetailForThisGame were set by GetReliableGamePath

                    // Add to categorized problems if an issue occurred for this game
                    if (issueCategoryForThisGame != null)
                    {
                        if (!categorizedProblematicGames.ContainsKey(issueCategoryForThisGame))
                        {
                            categorizedProblematicGames[issueCategoryForThisGame] = new List<string>();
                        }
                        categorizedProblematicGames[issueCategoryForThisGame].Add(issueDetailForThisGame);
                    }

                    _lbDataService.UpdateGameSizeFields(game, sizeResultForUpdate, storeGameSize, storeLastScanned, storeSizeTier);
                }
            }).ContinueWith(task =>
            {
                Action showResultsAction = () => {
                    StringBuilder completionMessage = new StringBuilder();
                    completionMessage.AppendLine($"Size calculation finished for: {contextDescription}.");

                    if (categorizedProblematicGames.Any())
                    {
                        // Show the detailed error report dialog
                        ErrorReportDialog reportDialog = new ErrorReportDialog(contextDescription, categorizedProblematicGames);
                        reportDialog.ShowDialog(); // Modal dialog
                        // Optionally add a summary to the main completion message too
                        completionMessage.AppendLine($"\n{categorizedProblematicGames.Sum(kvp => kvp.Value.Count)} game(s) had issues. See separate report for details.");
                    }
                    else
                    {
                        completionMessage.AppendLine("\nAll selected games processed without reported issues.");
                    }

                    if (task.IsFaulted)
                    {
                        var exMessage = task.Exception?.Flatten().InnerExceptions.FirstOrDefault()?.Message ?? "Unknown critical error.";
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Size calculation task faulted for {contextDescription}: {exMessage} - {task.Exception.ToString()}");
#endif
                        completionMessage.AppendLine($"\nCRITICAL ERROR during scan: {exMessage}");
                        PluginUIManager.ShowError(Constants.PluginName, completionMessage.ToString());
                    }
                    else if (task.IsCanceled)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Size calculation task cancelled for {contextDescription}.");
#endif
                        PluginUIManager.ShowWarning(Constants.PluginName, $"Size calculation was cancelled for {contextDescription}.");
                    }
                    else if (!categorizedProblematicGames.Any()) // Only show simple success if NO per-game issues AND no critical fault/cancel
                    {
                        PluginUIManager.ShowInformation(Constants.PluginName, completionMessage.ToString());
                    }
                };

                // Ensure UI updates are on the UI thread
                if (Application.OpenForms.Count > 0 && Application.OpenForms[0].InvokeRequired)
                {
                    Application.OpenForms[0].Invoke(showResultsAction);
                }
                else
                {
                    showResultsAction();
                }

            }, TaskScheduler.Default); // Using Default scheduler, explicit UI marshalling above for dialog.
        }
    }
}