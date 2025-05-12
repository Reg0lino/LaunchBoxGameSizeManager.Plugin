using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LaunchBoxGameSizeManager.Utils; // For Constants

namespace LaunchBoxGameSizeManager.UI
{
    // Enum remains the same as it's still useful for the calling code
    public enum PlatformScanDialogAction
    {
        Cancel,
        ScanPlatform,
        ClearDataForPlatform
    }

    public partial class PlatformActionsDialog : Form // Renamed class
    {
        private ComboBox cmbPlatforms;
        private Label lblPlatformSelect;
        private Button btnInitiateScan; // Renamed from btnStartScan
        private Button btnClearDataForPlatform;
        private Button btnCancelDialog;

        public string SelectedPlatformName { get; private set; }
        public PlatformScanDialogAction UserAction { get; private set; } = PlatformScanDialogAction.Cancel;

        public PlatformActionsDialog(IEnumerable<string> platformNames)
        {
            InitializeComponent();
            PopulatePlatforms(platformNames);
            ApplyDarkTheme();
        }

        private void PopulatePlatforms(IEnumerable<string> platformNames)
        {
            cmbPlatforms.Items.Clear();
            if (platformNames != null && platformNames.Any())
            {
                foreach (var name in platformNames.OrderBy(n => n))
                {
                    cmbPlatforms.Items.Add(name);
                }
                if (cmbPlatforms.Items.Count > 0)
                    cmbPlatforms.SelectedIndex = 0;
            }
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.FromArgb(220, 220, 220);
            lblPlatformSelect.ForeColor = this.ForeColor;
            cmbPlatforms.BackColor = Color.FromArgb(60, 60, 63);
            cmbPlatforms.ForeColor = this.ForeColor;

            StyleButton(btnInitiateScan);
            StyleButton(btnClearDataForPlatform);
            StyleButton(btnCancelDialog);
        }

        private void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btn.FlatAppearance.BorderSize = 1;
            btn.BackColor = Color.FromArgb(60, 60, 63);
            btn.ForeColor = Color.FromArgb(220, 220, 220);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 75, 78);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 90, 93);
        }

        private void InitializeComponent()
        {
            this.cmbPlatforms = new System.Windows.Forms.ComboBox();
            this.lblPlatformSelect = new System.Windows.Forms.Label();
            this.btnInitiateScan = new System.Windows.Forms.Button();
            this.btnClearDataForPlatform = new System.Windows.Forms.Button();
            this.btnCancelDialog = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbPlatforms
            // 
            this.cmbPlatforms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlatforms.FormattingEnabled = true;
            this.cmbPlatforms.Location = new System.Drawing.Point(15, 35);
            this.cmbPlatforms.Name = "cmbPlatforms";
            this.cmbPlatforms.Size = new System.Drawing.Size(297, 21);
            this.cmbPlatforms.TabIndex = 1;
            // 
            // lblPlatformSelect
            // 
            this.lblPlatformSelect.AutoSize = true;
            this.lblPlatformSelect.Location = new System.Drawing.Point(12, 19);
            this.lblPlatformSelect.Name = "lblPlatformSelect";
            this.lblPlatformSelect.Size = new System.Drawing.Size(83, 13);
            this.lblPlatformSelect.TabIndex = 0;
            this.lblPlatformSelect.Text = "Select Platform:";
            // 
            // btnInitiateScan
            // 
            this.btnInitiateScan.Location = new System.Drawing.Point(15, 70); // Adjusted position
            this.btnInitiateScan.Name = "btnInitiateScan";
            this.btnInitiateScan.Size = new System.Drawing.Size(140, 25);
            this.btnInitiateScan.TabIndex = 3;
            this.btnInitiateScan.Text = "Scan Platform..."; // Text updated
            this.btnInitiateScan.UseVisualStyleBackColor = false;
            this.btnInitiateScan.Click += new System.EventHandler(this.btnInitiateScan_Click);
            // 
            // btnClearDataForPlatform
            // 
            this.btnClearDataForPlatform.Location = new System.Drawing.Point(172, 70); // Adjusted position
            this.btnClearDataForPlatform.Name = "btnClearDataForPlatform";
            this.btnClearDataForPlatform.Size = new System.Drawing.Size(140, 25);
            this.btnClearDataForPlatform.TabIndex = 4;
            this.btnClearDataForPlatform.Text = "Clear Size Data"; // Simplified text
            this.btnClearDataForPlatform.UseVisualStyleBackColor = false;
            this.btnClearDataForPlatform.Click += new System.EventHandler(this.btnClearDataForPlatform_Click);
            // 
            // btnCancelDialog
            // 
            this.btnCancelDialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelDialog.Location = new System.Drawing.Point(237, 105); // Adjusted position
            this.btnCancelDialog.Name = "btnCancelDialog";
            this.btnCancelDialog.Size = new System.Drawing.Size(75, 25);
            this.btnCancelDialog.TabIndex = 5;
            this.btnCancelDialog.Text = "Cancel";
            this.btnCancelDialog.UseVisualStyleBackColor = false;
            this.btnCancelDialog.Click += new System.EventHandler(this.btnCancelDialog_Click);
            // 
            // PlatformActionsDialog
            // 
            this.AcceptButton = this.btnInitiateScan;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelDialog;
            this.ClientSize = new System.Drawing.Size(324, 142); // Adjusted height, checkboxes removed
            this.Controls.Add(this.btnCancelDialog);
            this.Controls.Add(this.btnClearDataForPlatform);
            this.Controls.Add(this.btnInitiateScan);
            this.Controls.Add(this.lblPlatformSelect);
            this.Controls.Add(this.cmbPlatforms);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlatformActionsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = $"{Constants.PluginName} - Platform Actions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void btnInitiateScan_Click(object sender, EventArgs e)
        {
            if (cmbPlatforms.SelectedItem == null)
            {
                MessageBox.Show("Please select a platform.", "No Platform Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.SelectedPlatformName = cmbPlatforms.SelectedItem.ToString();
            this.UserAction = PlatformScanDialogAction.ScanPlatform;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClearDataForPlatform_Click(object sender, EventArgs e)
        {
            if (cmbPlatforms.SelectedItem == null)
            {
                MessageBox.Show("Please select a platform whose data you want to clear.", "No Platform Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.SelectedPlatformName = cmbPlatforms.SelectedItem.ToString();

            string confirmMessage = $"Are you sure you want to remove ALL Game Size Manager data for ALL games on the platform '{this.SelectedPlatformName}'?\nThis will remove:\n" +
                                    $"- {Constants.CustomFieldGameSize}\n" +
                                    $"- {Constants.CustomFieldLastScanned}\n" +
                                    $"- {Constants.CustomFieldGameSizeTier}\n\n" +
                                    "This action cannot be undone.";

            if (MessageBox.Show(confirmMessage, "Confirm Clear Platform Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                this.UserAction = PlatformScanDialogAction.ClearDataForPlatform;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancelDialog_Click(object sender, EventArgs e)
        {
            this.UserAction = PlatformScanDialogAction.Cancel;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}