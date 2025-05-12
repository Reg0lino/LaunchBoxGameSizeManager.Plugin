using System;
using System.Collections.Generic;
using System.Drawing;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxGameSizeManager.UI
{
    public class GameSizeMenuItem : IGameMenuItem
    {
        public string Caption { get; }
        public Image Icon { get; }
        public bool Enabled { get; set; } = true;
        public IEnumerable<IGameMenuItem> Children => null;

        private Action<IGame[]> _onSelectAction;
        private IGame[] _capturedSelectedGames; // Store the games this menu item pertains to

        // Constructor takes the games relevant to this specific menu item instance
        public GameSizeMenuItem(string caption, Image icon, IGame[] initiallySelectedGames, Action<IGame[]> onSelectAction)
        {
            Caption = caption;
            Icon = icon;
            _capturedSelectedGames = initiallySelectedGames; // Capture the context
            _onSelectAction = onSelectAction;
        }

        // The 'games' parameter here is what LaunchBox passes when the item is clicked.
        // It might be just the single game that was right-clicked on, even if multiple were selected
        // when GetMenuItems was called. So, we use _capturedSelectedGames.
        public void OnSelect(params IGame[] gamesOnClick) 
        {
            _onSelectAction?.Invoke(_capturedSelectedGames);
        }
    }
}