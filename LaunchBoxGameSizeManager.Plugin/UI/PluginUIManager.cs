using System;
using System.Collections.Generic;
using System.Windows.Forms; 
// using Unbroken.LaunchBox.Plugins; // Not needed if PluginHelper logging is removed or replaced
using LaunchBoxGameSizeManager.Utils; 

namespace LaunchBoxGameSizeManager.UI
{
    public static class PluginUIManager
    {
        public static string ShowPlatformSelectionDialog(IEnumerable<string> platformNames)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"{Constants.PluginName} - Select Platform",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 440, Text = "Available Platforms (select one and click OK, or type if list is too long):" };
            ListBox listBox = new ListBox() { Left = 20, Top = 45, Width = 440, Height = 60 };
            foreach (var platform in platformNames)
            {
                listBox.Items.Add(platform);
            }
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }

            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 110, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(listBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            System.Diagnostics.Debug.WriteLine($"[{Constants.PluginName}] Displaying platform selection dialog."); // Fallback logging
            return prompt.ShowDialog() == DialogResult.OK && listBox.SelectedItem != null ? listBox.SelectedItem.ToString() : string.Empty;
        }

        public static void ShowInformation(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool ShowConfirmation(string title, string message)
        {
            DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }
    }
}