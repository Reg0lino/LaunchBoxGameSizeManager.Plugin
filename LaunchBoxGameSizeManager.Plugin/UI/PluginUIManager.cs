using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing; // Required for Point, Size, etc.
using Unbroken.LaunchBox.Plugins; // For PluginHelper for logging potentially

namespace LaunchBoxGameSizeManager.UI
{
    public static class PluginUIManager
    {
        public static string ShowPlatformSelectionDialog(IEnumerable<string> platformNames)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 350;
                prompt.Height = 150;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Select Platform to Scan";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.ControlBox = false; // Remove minimize/maximize buttons

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Choose a platform:", Width = 300 };
                ComboBox comboBox = new ComboBox() { Left = 20, Top = 45, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
                foreach (var platformName in platformNames)
                {
                    comboBox.Items.Add(platformName);
                }
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }

                Button confirmation = new Button() { Text = "Scan", Left = 140, Width = 80, Top = 80, DialogResult = DialogResult.OK };
                Button cancellation = new Button() { Text = "Cancel", Left = 230, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

                confirmation.Click += (sender, e) => { prompt.Close(); };
                cancellation.Click += (sender, e) => { prompt.Close(); };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(comboBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancellation);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancellation;

                return prompt.ShowDialog() == DialogResult.OK && comboBox.SelectedItem != null ? comboBox.SelectedItem.ToString() : null;
            }
        }

        public static void ShowInformation(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool ShowConfirmation(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }
    }
}