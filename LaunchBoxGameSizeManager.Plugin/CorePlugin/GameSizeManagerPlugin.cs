// --- START OF FILE CorePlugin/GameSizeManagerPlugin.cs ---
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
        private LaunchBoxDataService _lbDataService;
        private FileSystemService _fileSystemService;
        private StorefrontInfoService _storefrontInfoService;
        private System.Drawing.Image _icon;

        public GameSizeManagerPlugin()
        {
            // These are essential init logs
            FileLogger.Initialize(Constants.PluginName);
            FileLogger.Log($"Plugin DLL is running from: {System.Reflection.Assembly.GetExecutingAssembly().Location}");
            FileLogger.Log("Plugin Constructor: Starting initialization...");

            try
            {
                FileLogger.Log("Plugin Constructor: Instantiating LaunchBoxDataService...");
                _lbDataService = new LaunchBoxDataService();
                FileLogger.Log("Plugin Constructor: LaunchBoxDataService instantiated.");

                FileLogger.Log("Plugin Constructor: Instantiating FileSystemService...");
                _fileSystemService = new FileSystemService();
                FileLogger.Log("Plugin Constructor: FileSystemService instantiated.");

                FileLogger.Log("Plugin Constructor: Instantiating StorefrontInfoService...");
                _storefrontInfoService = new StorefrontInfoService(); // Relies on StorefrontInfoService static constructor for API key logging
                FileLogger.Log("Plugin Constructor: StorefrontInfoService instantiated.");

                FileLogger.Log("Plugin Constructor: Calling LoadIcon()...");
                LoadIcon(); // LoadIcon has its own logging
            }
            catch (Exception ex)
            {
                FileLogger.LogError("CRITICAL ERROR IN PLUGIN CONSTRUCTOR (outer catch)", ex);
            }
            FileLogger.Log("Plugin Constructor: Initialization sequence finished.");
        }

        private void LoadIcon()
        {
            FileLogger.Log("LoadIcon: Attempting to load icon resource..."); // Essential
            try
            {
                string resourceName = "LaunchBoxGameSizeManager.plugin_icon.png";
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        _icon = System.Drawing.Image.FromStream(stream);
                        FileLogger.Log($"LoadIcon: Icon loaded successfully from: {resourceName}"); // Essential
                    }
                    else
                    {
                        FileLogger.Log($"LoadIcon: Icon resource not found: {resourceName}."); // Essential Warning
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("LoadIcon: Error loading icon", ex); // Essential Error
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
            FileLogger.Log("OnSelected (Tools Menu) triggered."); // Essential
            try
            {
                IEnumerable<string> platformNames = _lbDataService.GetAllPlatformNames();
                if (platformNames == null || !platformNames.Any())
                {
                    FileLogger.Log("OnSelected: No platforms found."); // Essential
                    PluginUIManager.ShowInformation(Constants.PluginName, "No platforms found in your LaunchBox library.");
                    return;
                }

                PlatformActionsDialog platformDialog = new PlatformActionsDialog(platformNames);
                if (platformDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPlatform = platformDialog.SelectedPlatformName;
                    FileLogger.Log($"OnSelected: Platform '{selectedPlatform}' selected with action '{platformDialog.UserAction}'."); // Essential
                    // ... (rest of OnSelected logic is fine) ...
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
                            FileLogger.Log($"OnSelected: No games found on platform '{selectedPlatform}' to scan.");
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
                            FileLogger.Log($"OnSelected: No games found on platform '{selectedPlatform}' to clear data from.");
                            PluginUIManager.ShowInformation(Constants.PluginName, $"No games found on platform '{selectedPlatform}' to clear data from.");
                        }
                    }
                }
                else
                {
                    FileLogger.Log("OnSelected: PlatformActionsDialog cancelled."); // Useful info
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("Error in Tools menu OnSelected", ex); // Essential Error
                PluginUIManager.ShowError(Constants.PluginName, $"An unexpected error occurred in Tools menu action: {ex.Message}");
            }
        }

        public IEnumerable<IGameMenuItem> GetMenuItems(params IGame[] selectedGames)
        {
#if DEBUG
            FileLogger.Log($"GetMenuItems called for {selectedGames?.Length ?? 0} games."); // Verbose
#endif
            if (selectedGames == null || selectedGames.Length == 0)
            {
                return Enumerable.Empty<IGameMenuItem>();
            }
            var menuItems = new List<IGameMenuItem>();
            menuItems.Add(new GameSizeMenuItem("Calculate Game Size(s)...", _icon, selectedGames,
                (games) => HandleCalculateGamesRequest(games, $"{games.Length} selected game(s)")));
            menuItems.Add(new GameSizeMenuItem("Clear Game Size Data", _icon, selectedGames,
                (games) => HandleClearGamesDataRequest(games, $"{games.Length} selected game(s)")));
#if DEBUG
            FileLogger.Log($"GetMenuItems returning {menuItems.Count} items."); // Verbose
#endif
            return menuItems;
        }

        private void HandleCalculateGamesRequest(IGame[] gamesToProcess, string contextDescription)
        {
            // Essential start of action
            FileLogger.Log($"HandleCalculateGamesRequest: Context: '{contextDescription}', Games: {gamesToProcess?.Length ?? 0}.");
            if (gamesToProcess == null || !gamesToProcess.Any())
            {
                PluginUIManager.ShowWarning(Constants.PluginName, $"No games provided for '{contextDescription}' to calculate size.");
                return;
            }

            bool apiKeyOk = StorefrontInfoService.IsApiKeyConfigured;
            // Essential info for diagnosing scan option behavior
            FileLogger.Log($"HandleCalculateGamesRequest: API Key Configured Status: {apiKeyOk}");

            ScanOptionsDialog optionsDialog = new ScanOptionsDialog(apiKeyOk);
            if (optionsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                FileLogger.Log($"HandleCalculateGamesRequest: Scan options cancelled for '{contextDescription}'."); // Useful info
                return;
            }
            bool storeGameSize = optionsDialog.StoreGameSize;
            bool storeLastScanned = optionsDialog.StoreLastScanned;
            bool storeSizeTier = optionsDialog.StoreSizeTier;
            bool fetchEstSizeSelectedByUser = optionsDialog.FetchEstRequiredSpace;
            // Essential options selected by user
            FileLogger.Log($"HandleCalculateGamesRequest: Options - StoreSize:{storeGameSize}, StoreLastScanned:{storeLastScanned}, StoreTier:{storeSizeTier}, FetchEstSize:{fetchEstSizeSelectedByUser}");

            if (!storeGameSize && !storeLastScanned && !storeSizeTier && !fetchEstSizeSelectedByUser)
            {
                FileLogger.Log("HandleCalculateGamesRequest: No data fields selected. Scan aborted."); // Useful info
                PluginUIManager.ShowInformation(Constants.PluginName, "No data fields selected to store or fetch. Scan aborted.");
                return;
            }
            ProcessGamesAndStoreData(gamesToProcess, contextDescription, storeGameSize, storeLastScanned, storeSizeTier, fetchEstSizeSelectedByUser);
        }

        private void HandleClearGamesDataRequest(IGame[] gamesToClear, string contextDescription)
        {
            FileLogger.Log($"HandleClearGamesDataRequest: Context: '{contextDescription}', Games: {gamesToClear?.Length ?? 0}."); // Essential
            // ... (rest of method is fine with existing logs) ...
            if (gamesToClear == null || !gamesToClear.Any())
            {
                PluginUIManager.ShowWarning(Constants.PluginName, $"No games provided for '{contextDescription}' to clear data.");
                return;
            }
            string confirmMessage = $"Are you sure you want to remove all Game Size Manager data for {contextDescription}?\nThis will remove:\n" +
                                    $"- \"{Constants.CustomFieldGameSize}\"\n" +
                                    $"- \"{Constants.CustomFieldLastScanned}\"\n" +
                                    $"- \"{Constants.CustomFieldGameSizeTier}\"\n" +
                                    $"- \"{Constants.CustomFieldEstRequiredSpace}\"\n\nThis action cannot be undone.";
            if (PluginUIManager.ShowConfirmation("Confirm Clear", confirmMessage))
            {
                FileLogger.Log($"HandleClearGamesDataRequest: User confirmed data clear for '{contextDescription}'.");
                PluginUIManager.ShowInformation(Constants.PluginName, $"Starting data clear for {contextDescription}.");
                Task.Run(() =>
                {
                    int count = 0;
                    foreach (var game in gamesToClear)
                    {
                        count++;
#if DEBUG
                        FileLogger.Log($"HandleClearGamesDataRequest: Clearing data for game {count}/{gamesToClear.Length}: {game.Title}"); // Verbose per-game
#endif
                        _lbDataService.ClearGameSizePluginFields(game);
                    }
                }).ContinueWith(task => {
                    if (task.IsFaulted)
                    {
                        FileLogger.LogError($"Error clearing game size data for {contextDescription}", task.Exception);
                        PluginUIManager.ShowError(Constants.PluginName, "An error occurred while clearing game size data.");
                    }
                    else
                    {
                        FileLogger.Log($"HandleClearGamesDataRequest: Data cleared successfully for '{contextDescription}'.");
                        PluginUIManager.ShowInformation(Constants.PluginName, $"Game Size Manager data cleared for {contextDescription}.");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                FileLogger.Log($"HandleClearGamesDataRequest: User cancelled data clear for '{contextDescription}'.");
            }
        }

        private void ProcessGamesAndStoreData(IGame[] gamesToProcess, string contextDescription,
                                              bool storeGameSize, bool storeLastScanned, bool storeSizeTier,
                                              bool fetchEstSizeSelectedByUser)
        {
            // Essential start of major process
            FileLogger.Log($"ProcessGamesAndStoreData: Starting for '{contextDescription}'. Games: {gamesToProcess.Length}. Options: StoreSize:{storeGameSize}, StoreLastScanned:{storeLastScanned}, StoreTier:{storeSizeTier}, FetchEstSize:{fetchEstSizeSelectedByUser}");
            PluginUIManager.ShowInformation(Constants.PluginName, $"Starting size calculation for: {contextDescription}.\nThis may take a while, especially if fetching online data.");
            Dictionary<string, List<string>> categorizedProblematicGames = new Dictionary<string, List<string>>();

            Task.Run(async () =>
            {
                FileLogger.Log("ProcessGamesAndStoreData: Background task started."); // Essential
                int currentProcessedCount = 0;
                foreach (IGame game in gamesToProcess)
                {
                    currentProcessedCount++;
                    string gameTitleForLog = game?.Title ?? "NULL_GAME_OBJECT";
#if DEBUG // Per-game processing is verbose
                    FileLogger.Log($"ProcessGamesAndStoreData: Processing {currentProcessedCount}/{gamesToProcess.Length}: '{gameTitleForLog}'.");
#endif

                    long localSizeResultForUpdate = LaunchBoxDataService.DO_NOT_STORE_SIZE_CODE;
                    long? estRequiredSpaceResultForUpdate = null;
                    string pathfindingIssueCategory = null;
                    string pathfindingIssueDetail = null;

#if DEBUG // GamePathLogic details are verbose
                    FileLogger.Log($"ProcessGamesAndStoreData: Getting reliable game path for '{gameTitleForLog}'.");
#endif
                    string pathToCheck = GamePathLogic.GetReliableGamePath(game, out pathfindingIssueCategory, out pathfindingIssueDetail);
#if DEBUG
                    FileLogger.Log($"ProcessGamesAndStoreData: Path for '{gameTitleForLog}': '{pathToCheck ?? "NULL"}'. Initial IssueCategory: '{pathfindingIssueCategory ?? "None"}'. Detail: '{pathfindingIssueDetail ?? "None"}'");
#endif

                    if (!string.IsNullOrEmpty(pathToCheck))
                    {
#if DEBUG
                        FileLogger.Log($"ProcessGamesAndStoreData: Calculating local size for '{gameTitleForLog}' at path '{pathToCheck}'.");
#endif
                        long calculatedSize = pathToCheck.EndsWith(".cue", StringComparison.OrdinalIgnoreCase) && File.Exists(pathToCheck)
                            ? _fileSystemService.CalculateCueSheetAndRelatedFilesSize(pathToCheck)
                            : _fileSystemService.CalculateDirectorySize(pathToCheck);

                        if (calculatedSize >= 0)
                        {
                            localSizeResultForUpdate = calculatedSize;
#if DEBUG
                            FileLogger.Log($"ProcessGamesAndStoreData: Local size for '{gameTitleForLog}': {FormatHelpers.FormatBytes(localSizeResultForUpdate)}.");
#endif
                        }
                        else
                        {
                            string localErrorKey = "Error Calculating Local Size";
                            // Error, keep for release
                            FileLogger.Log($"ProcessGamesAndStoreData: {localErrorKey} for '{gameTitleForLog}'. Path: '{pathToCheck}', FS Error Code: {calculatedSize}.");
                            if (!categorizedProblematicGames.ContainsKey(localErrorKey))
                                categorizedProblematicGames[localErrorKey] = new List<string>();
                            categorizedProblematicGames[localErrorKey].Add($"{gameTitleForLog} (Path: {pathToCheck}, FS Error: {calculatedSize})");
                        }
                    }

                    if (fetchEstSizeSelectedByUser)
                    {
                        // Info that API call is happening, keep for release
                        FileLogger.Log($"ProcessGamesAndStoreData: User opted to fetch est. size for '{gameTitleForLog}'. Attempting API lookup.");
                        var (fetchedSize, apiErrorMessage) = await _storefrontInfoService.GetEstRequiredDiskSpaceAsync(game); // StorefrontInfoService logs its internal steps
                        if (fetchedSize.HasValue)
                        {
                            estRequiredSpaceResultForUpdate = fetchedSize.Value;
                            // Success, keep for release
                            FileLogger.Log($"ProcessGamesAndStoreData: Online lookup SUCCESS for '{gameTitleForLog}'. Size: {FormatHelpers.FormatBytes(estRequiredSpaceResultForUpdate.Value)}.");
                        }
                        else
                        {
                            string apiErrorKey = $"Online Lookup Failed ({apiErrorMessage ?? "Unknown API Error"})";
                            // API Error, keep for release
                            FileLogger.Log($"ProcessGamesAndStoreData: Online lookup FAILED for '{gameTitleForLog}'. API Error: {apiErrorMessage}. Original Pathfinding Detail: '{pathfindingIssueDetail ?? "None"}'");
                            if (!categorizedProblematicGames.ContainsKey(apiErrorKey))
                                categorizedProblematicGames[apiErrorKey] = new List<string>();

                            string reportDetail = gameTitleForLog;
                            if (pathfindingIssueCategory == "Storefront Game" && !string.IsNullOrEmpty(pathfindingIssueDetail) && pathfindingIssueDetail.Contains(gameTitleForLog))
                            {
                                reportDetail = pathfindingIssueDetail;
                            }
                            categorizedProblematicGames[apiErrorKey].Add(reportDetail);
                        }
                    }
                    else if (pathfindingIssueCategory == "Storefront Game")
                    {
                        string skippedKey = "Online Lookup Skipped (Storefront Game)";
                        // Info on skipped operation, keep for release
                        FileLogger.Log($"ProcessGamesAndStoreData: Online lookup skipped by user for storefront game '{gameTitleForLog}'. Detail: '{pathfindingIssueDetail}'.");
                        if (!categorizedProblematicGames.ContainsKey(skippedKey))
                            categorizedProblematicGames[skippedKey] = new List<string>();
                        categorizedProblematicGames[skippedKey].Add(pathfindingIssueDetail);
                    }

                    if (!string.IsNullOrEmpty(pathfindingIssueCategory) &&
                        pathfindingIssueCategory != "Storefront Game" &&
                        localSizeResultForUpdate == LaunchBoxDataService.DO_NOT_STORE_SIZE_CODE &&
                        (!fetchEstSizeSelectedByUser || (fetchEstSizeSelectedByUser && !estRequiredSpaceResultForUpdate.HasValue)))
                    {
                        // Report original path issue, keep for release if it's relevant
                        FileLogger.Log($"ProcessGamesAndStoreData: Reporting original pathfinding issue for '{gameTitleForLog}'. Category: '{pathfindingIssueCategory}', Detail: '{pathfindingIssueDetail}'.");
                        if (!categorizedProblematicGames.ContainsKey(pathfindingIssueCategory))
                            categorizedProblematicGames[pathfindingIssueCategory] = new List<string>();
                        if (!(pathfindingIssueCategory == "Storefront Game" && fetchEstSizeSelectedByUser))
                            categorizedProblematicGames[pathfindingIssueCategory].Add(pathfindingIssueDetail);
                    }

#if DEBUG // Updating LB fields is a detail
                    FileLogger.Log($"ProcessGamesAndStoreData: Updating LB fields for '{gameTitleForLog}'. LocalSizeRaw: {localSizeResultForUpdate}, EstSizeRaw: {estRequiredSpaceResultForUpdate?.ToString() ?? "N/A"}. StoreEstSizeFlag: {fetchEstSizeSelectedByUser}");
#endif
                    _lbDataService.UpdateGameSizeFields(game,
                                                        localSizeResultForUpdate,
                                                        estRequiredSpaceResultForUpdate,
                                                        storeGameSize,
                                                        storeLastScanned,
                                                        storeSizeTier,
                                                        fetchEstSizeSelectedByUser);
                }
                FileLogger.Log("ProcessGamesAndStoreData: Background processing loop finished for all games."); // Essential summary
            }).ContinueWith(task =>
            {
                FileLogger.Log("ProcessGamesAndStoreData: ContinueWith task started (UI updates)."); // Essential
                // ... (rest of ContinueWith is fine with existing logs, they are mainly summaries or error reports) ...
                Action showResultsAction = () => {
                    StringBuilder completionMessage = new StringBuilder();
                    completionMessage.AppendLine($"Size calculation finished for: {contextDescription}.");

                    if (categorizedProblematicGames.Any())
                    {
                        FileLogger.Log($"ProcessGamesAndStoreData: Displaying ErrorReportDialog. Issues found: {categorizedProblematicGames.Sum(kvp => kvp.Value.Count)}.");
                        ErrorReportDialog reportDialog = new ErrorReportDialog(contextDescription, categorizedProblematicGames);
                        reportDialog.ShowDialog();
                        completionMessage.AppendLine($"\n{categorizedProblematicGames.Sum(kvp => kvp.Value.Count)} game(s) had issues. See separate report for details.");
                    }
                    else
                    {
                        FileLogger.Log("ProcessGamesAndStoreData: No problematic games reported.");
                        completionMessage.AppendLine("\nAll selected games processed without reported issues.");
                    }

                    if (task.IsFaulted)
                    {
                        FileLogger.LogError($"Size calculation task faulted for {contextDescription}", task.Exception);
                        var exMessage = task.Exception?.Flatten().InnerExceptions.FirstOrDefault()?.Message ?? "Unknown critical error.";
                        completionMessage.AppendLine($"\nCRITICAL ERROR during scan: {exMessage}");
                        PluginUIManager.ShowError(Constants.PluginName, completionMessage.ToString());
                    }
                    else if (task.IsCanceled)
                    {
                        FileLogger.Log($"ProcessGamesAndStoreData: Size calculation task cancelled for {contextDescription}.");
                        PluginUIManager.ShowWarning(Constants.PluginName, $"Size calculation was cancelled for {contextDescription}.");
                    }
                    else
                    {
                        FileLogger.Log($"ProcessGamesAndStoreData: Scan completed for '{contextDescription}'.");
                        if (!categorizedProblematicGames.Any())
                            PluginUIManager.ShowInformation(Constants.PluginName, completionMessage.ToString());
                        else
                            PluginUIManager.ShowInformation(Constants.PluginName, $"Scan complete for {contextDescription}. Check report for any issues.");
                    }
                };

                Form mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.IsHandleCreated && !f.IsDisposed && f.Visible);
                if (mainForm != null && mainForm.InvokeRequired)
                {
#if DEBUG
                    FileLogger.Log("ProcessGamesAndStoreData: Invoking UI update on main form's thread.");
#endif
                    mainForm.Invoke(showResultsAction);
                }
                else
                {
#if DEBUG
                    FileLogger.Log("ProcessGamesAndStoreData: Executing UI update directly (already on UI thread or no suitable form found).");
#endif
                    showResultsAction();
                }
                FileLogger.Log("ProcessGamesAndStoreData: UI update action completed."); // Essential
            }, TaskScheduler.Default);
        }
    }
}
// --- END OF FILE CorePlugin/GameSizeManagerPlugin.cs ---