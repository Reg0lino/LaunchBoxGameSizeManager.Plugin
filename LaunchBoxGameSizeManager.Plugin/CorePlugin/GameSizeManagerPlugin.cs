using System;
using System.Collections.Generic;
using System.Linq; // Required for FirstOrDefault
using System.Windows.Forms; // For IWin32Window for ShowPlatformSelectionDialog if we pass owner
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using LaunchBoxGameSizeManager.Services;
using LaunchBoxGameSizeManager.UI;
using LaunchBoxGameSizeManager.Utils; // For Constants


namespace LaunchBoxGameSizeManager.CorePlugin
{
    public class GameSizeManagerPlugin : ISystemMenuItemPlugin //, IGameMenuItemPlugin // Add IGameMenuItemPlugin for Phase 2
    {
        private LaunchBoxDataService _lbDataService;
        private FileSystemService _fileSystemService;
        private GameProcessingService _gameProcessingService;

        public GameSizeManagerPlugin()
        {
            // Initialize services
            _lbDataService = new LaunchBoxDataService();
            _fileSystemService = new FileSystemService();
            _gameProcessingService = new GameProcessingService(_lbDataService, _fileSystemService);
        }

        // Implementation for ISystemMenuItemPlugin
        public string Caption => "Game Size Manager"; // Text for the menu item in Tools

        public System.Drawing.Image IconImage => null; // No icon for now, or provide a 16x16 PNG

        public bool ShowInLaunchBox => true;  // Show in LaunchBox main UI

        public bool ShowInBigBox => false; // Don't show in BigBox for now

        public bool AllowInBigBoxWhenLocked => false;

        public void OnSelected()
        {
            try
            {
                PluginHelper.LogHelper.Log("Game Size Manager plugin selected.", Constants.PluginName, LogLevel.Info);

                var platformNames = _lbDataService.GetAllPlatformNames();
                if (!platformNames.Any())
                {
                    PluginUIManager.ShowInformation(Constants.PluginName, "No platforms found in your LaunchBox library.");
                    return;
                }
                
                // Pass the main LaunchBox window handle if possible, otherwise null
                // IWin32Window owner = Control.FromHandle(PluginHelper.LaunchBoxMainDbi) as IWin32Window;
                // For simplicity, let's not pass an owner for the first pass, ShowPlatformSelectionDialog will center screen.
                
                string selectedPlatform = PluginUIManager.ShowPlatformSelectionDialog(platformNames);

                if (!string.IsNullOrEmpty(selectedPlatform))
                {
                    PluginUIManager.ShowInformation(Constants.PluginName, $"Starting scan for platform: {selectedPlatform}.\nThis may take a while. Check LaunchBox logs for progress if needed.\n(Logs: LaunchBox\\Logs\\Debug.log)");
                    
                    // Asynchronously run the scan
                    _gameProcessingService.ScanPlatformGameSizesAsync(selectedPlatform, (progressMessage) =>
                    {
                        // This callback is tricky to update UI from a background thread directly.
                        // For now, progress is logged. A more advanced UI would handle this.
                        // PluginHelper.LogHelper.Log(progressMessage, Constants.PluginName, LogLevel.Debug);
                        // Could potentially use PluginHelper.ViewManager.StatusText = progressMessage; but test carefully.
                    }).ContinueWith(task =>
                    {
                        // This continuation runs after the async task completes.
                        // Ensure UI updates are on the UI thread if needed, but MessageBox is generally safe.
                        if (task.IsFaulted)
                        {
                            PluginHelper.LogHelper.Log($"Scan failed for {selectedPlatform}: {task.Exception?.Flatten().InnerExceptions.FirstOrDefault()?.Message}", Constants.PluginName, LogLevel.Error);
                            PluginUIManager.ShowError(Constants.PluginName, $"Scan failed for {selectedPlatform}.\nDetails: {task.Exception?.Flatten().InnerExceptions.FirstOrDefault()?.Message}");
                        }
                        else if (task.IsCompleted)
                        {
                            // The final "Scan complete!" message is already handled in GameProcessingService.
                            // We could show a summary here if desired.
                             PluginUIManager.ShowInformation(Constants.PluginName, $"Scan for '{selectedPlatform}' has finished processing. You may need to refresh your LaunchBox view or select a game to see the new custom fields.");
                        }
                    });
                }
                else
                {
                    PluginHelper.LogHelper.Log("Platform selection cancelled.", Constants.PluginName, LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                PluginHelper.LogHelper.Log($"An error occurred in Game Size Manager: {ex.ToString()}", Constants.PluginName, LogLevel.Error);
                PluginUIManager.ShowError(Constants.PluginName, $"An unexpected error occurred: {ex.Message}");
            }
        }

        // Stubs for IGameMenuItemPlugin (Phase 2)
        // public string CaptionForGame(IGame game) => "Manage Game Size";
        // public System.Drawing.Image IconImageForGame(IGame game) => null;
        // public bool ShowInLaunchBoxForGame(IGame game) => true;
        // public bool ShowInBigBoxForGame(IGame game) => false;
        // public bool AllowInBigBoxWhenLockedForGame(IGame game) => false;
        // public void OnSelectedForGame(IGame game) { /* ... */ }
        // public bool ShowForGames(IGame[] games) => games != null && games.Length > 0; // Or games.Length == 1 for single selection
        // public void OnSelectedForGames(IGame[] games) { /* ... */ }
    }
}