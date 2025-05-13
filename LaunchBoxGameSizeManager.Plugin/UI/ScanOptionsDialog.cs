// --- START OF FILE UI/ScanOptionsDialog.cs ---
using System;
using System.Drawing;
using System.Windows.Forms;
using LaunchBoxGameSizeManager.Utils; // For Constants

namespace LaunchBoxGameSizeManager.UI
{
    public partial class ScanOptionsDialog : Form
    {
        private CheckBox chkStoreGameSize;
        private Label lblStoreGameSizeInfo;
        private CheckBox chkStoreSizeTier;
        private Label lblStoreSizeTierInfo;
        private CheckBox chkFetchEstRequiredSpace;
        private Label lblFetchEstSpaceInfo1;
        private Label lblFetchEstSpaceInfo2;
        private Label lblApiKeyStatus; // New
        private CheckBox chkStoreLastScanned; // Moved
        private GroupBox grpLocalScan;
        private GroupBox grpOnlineFetch;
        private GroupBox grpGeneral;
        private Button btnStartScan;
        private Button btnCancelScan;
        private Label lblOverallNote;

        public bool StoreGameSize { get; private set; }
        public bool StoreLastScanned { get; private set; }
        public bool StoreSizeTier { get; private set; }
        public bool FetchEstRequiredSpace { get; private set; }

        private bool _isApiKeyConfigured;

        public ScanOptionsDialog(bool isApiKeyConfigured) // Constructor takes API key status
        {
            _isApiKeyConfigured = isApiKeyConfigured;
            InitializeComponent();
            SetDefaultsAndApiKeyStatus();
            ApplyDarkTheme();
        }

        private void SetDefaultsAndApiKeyStatus()
        {
            // Set defaults
            chkStoreGameSize.Checked = true;  // Default ON
            chkStoreSizeTier.Checked = false; // Default OFF
            chkStoreLastScanned.Checked = false; // Default OFF
            chkFetchEstRequiredSpace.Checked = false; // Default OFF

            if (!_isApiKeyConfigured)
            {
                chkFetchEstRequiredSpace.Enabled = false;
                chkFetchEstRequiredSpace.Checked = false; // Ensure it's off if disabled
                lblApiKeyStatus.Text = "API Key Missing/Needed - Online fetch disabled.";
                lblApiKeyStatus.ForeColor = Color.OrangeRed;
            }
            else
            {
                chkFetchEstRequiredSpace.Enabled = true;
                lblApiKeyStatus.Text = "API Key OK - Ready for online fetch.";
                lblApiKeyStatus.ForeColor = Color.ForestGreen; // Or a less alarming success color like a muted green
            }
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.FromArgb(220, 220, 220);

            // GroupBoxes
            StyleGroupBox(grpLocalScan);
            StyleGroupBox(grpOnlineFetch);
            StyleGroupBox(grpGeneral);

            // CheckBoxes and their info Labels
            chkStoreGameSize.ForeColor = this.ForeColor;
            lblStoreGameSizeInfo.ForeColor = Color.FromArgb(180, 180, 180);
            chkStoreSizeTier.ForeColor = this.ForeColor;
            lblStoreSizeTierInfo.ForeColor = Color.FromArgb(180, 180, 180);
            chkFetchEstRequiredSpace.ForeColor = this.ForeColor; // Will be slightly dimmed if disabled by default
            lblFetchEstSpaceInfo1.ForeColor = Color.FromArgb(180, 180, 180);
            lblFetchEstSpaceInfo2.ForeColor = Color.FromArgb(180, 180, 180);
            // lblApiKeyStatus color is set in SetDefaultsAndApiKeyStatus()
            chkStoreLastScanned.ForeColor = this.ForeColor;
            lblOverallNote.ForeColor = Color.FromArgb(170, 170, 170);

            // Buttons
            StyleButton(this.btnStartScan);
            StyleButton(this.btnCancelScan);
        }

        private void StyleGroupBox(GroupBox grp)
        {
            grp.ForeColor = this.ForeColor; // For the title text of the groupbox
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
            this.grpLocalScan = new System.Windows.Forms.GroupBox();
            this.lblStoreSizeTierInfo = new System.Windows.Forms.Label();
            this.chkStoreSizeTier = new System.Windows.Forms.CheckBox();
            this.lblStoreGameSizeInfo = new System.Windows.Forms.Label();
            this.chkStoreGameSize = new System.Windows.Forms.CheckBox();
            this.grpOnlineFetch = new System.Windows.Forms.GroupBox();
            this.lblApiKeyStatus = new System.Windows.Forms.Label();
            this.lblFetchEstSpaceInfo2 = new System.Windows.Forms.Label();
            this.lblFetchEstSpaceInfo1 = new System.Windows.Forms.Label();
            this.chkFetchEstRequiredSpace = new System.Windows.Forms.CheckBox();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.chkStoreLastScanned = new System.Windows.Forms.CheckBox();
            this.btnStartScan = new System.Windows.Forms.Button();
            this.btnCancelScan = new System.Windows.Forms.Button();
            this.lblOverallNote = new System.Windows.Forms.Label();
            this.grpLocalScan.SuspendLayout();
            this.grpOnlineFetch.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpLocalScan
            // 
            this.grpLocalScan.Controls.Add(this.lblStoreSizeTierInfo);
            this.grpLocalScan.Controls.Add(this.chkStoreSizeTier);
            this.grpLocalScan.Controls.Add(this.lblStoreGameSizeInfo);
            this.grpLocalScan.Controls.Add(this.chkStoreGameSize);
            this.grpLocalScan.Location = new System.Drawing.Point(12, 12);
            this.grpLocalScan.Name = "grpLocalScan";
            this.grpLocalScan.Size = new System.Drawing.Size(360, 110);
            this.grpLocalScan.TabIndex = 0;
            this.grpLocalScan.TabStop = false;
            this.grpLocalScan.Text = "Local Game Installation Scan";
            // 
            // lblStoreSizeTierInfo
            // 
            this.lblStoreSizeTierInfo.AutoSize = true;
            this.lblStoreSizeTierInfo.Location = new System.Drawing.Point(25, 84);
            this.lblStoreSizeTierInfo.Name = "lblStoreSizeTierInfo";
            this.lblStoreSizeTierInfo.Size = new System.Drawing.Size(165, 13);
            this.lblStoreSizeTierInfo.TabIndex = 3;
            this.lblStoreSizeTierInfo.Text = "(Category for sorting by local size)";
            // 
            // chkStoreSizeTier
            // 
            this.chkStoreSizeTier.AutoSize = true;
            this.chkStoreSizeTier.Location = new System.Drawing.Point(9, 64);
            this.chkStoreSizeTier.Name = "chkStoreSizeTier";
            this.chkStoreSizeTier.Size = new System.Drawing.Size(190, 17); // Text updated in ApplyDarkTheme or constructor
            this.chkStoreSizeTier.TabIndex = 1;
            this.chkStoreSizeTier.Text = $"Store \"{Constants.CustomFieldGameSizeTier}\"";
            this.chkStoreSizeTier.UseVisualStyleBackColor = true;
            // 
            // lblStoreGameSizeInfo
            // 
            this.lblStoreGameSizeInfo.AutoSize = true;
            this.lblStoreGameSizeInfo.Location = new System.Drawing.Point(25, 43);
            this.lblStoreGameSizeInfo.Name = "lblStoreGameSizeInfo";
            this.lblStoreGameSizeInfo.Size = new System.Drawing.Size(202, 13);
            this.lblStoreGameSizeInfo.TabIndex = 1;
            this.lblStoreGameSizeInfo.Text = "(Calculates size of installed ROMs/Game Folders)";
            // 
            // chkStoreGameSize
            // 
            this.chkStoreGameSize.AutoSize = true;
            this.chkStoreGameSize.Location = new System.Drawing.Point(9, 23);
            this.chkStoreGameSize.Name = "chkStoreGameSize";
            this.chkStoreGameSize.Size = new System.Drawing.Size(190, 17); // Text updated
            this.chkStoreGameSize.TabIndex = 0;
            this.chkStoreGameSize.Text = $"Store \"{Constants.CustomFieldGameSize}\"";
            this.chkStoreGameSize.UseVisualStyleBackColor = true;
            // 
            // grpOnlineFetch
            // 
            this.grpOnlineFetch.Controls.Add(this.lblApiKeyStatus);
            this.grpOnlineFetch.Controls.Add(this.lblFetchEstSpaceInfo2);
            this.grpOnlineFetch.Controls.Add(this.lblFetchEstSpaceInfo1);
            this.grpOnlineFetch.Controls.Add(this.chkFetchEstRequiredSpace);
            this.grpOnlineFetch.Location = new System.Drawing.Point(12, 128); // Adjusted Y position
            this.grpOnlineFetch.Name = "grpOnlineFetch";
            this.grpOnlineFetch.Size = new System.Drawing.Size(360, 115); // Adjusted height
            this.grpOnlineFetch.TabIndex = 1;
            this.grpOnlineFetch.TabStop = false;
            this.grpOnlineFetch.Text = "Online Data Fetch (RAWG API)";
            // 
            // lblApiKeyStatus
            // 
            this.lblApiKeyStatus.AutoSize = true;
            this.lblApiKeyStatus.Location = new System.Drawing.Point(25, 88); // Position for API key status
            this.lblApiKeyStatus.Name = "lblApiKeyStatus";
            this.lblApiKeyStatus.Size = new System.Drawing.Size(100, 13); // Placeholder size
            this.lblApiKeyStatus.TabIndex = 3;
            this.lblApiKeyStatus.Text = "Status: Checking..."; // Initial text
            // 
            // lblFetchEstSpaceInfo2
            // 
            this.lblFetchEstSpaceInfo2.AutoSize = true;
            this.lblFetchEstSpaceInfo2.Location = new System.Drawing.Point(25, 65);
            this.lblFetchEstSpaceInfo2.Name = "lblFetchEstSpaceInfo2";
            this.lblFetchEstSpaceInfo2.Size = new System.Drawing.Size(173, 13);
            this.lblFetchEstSpaceInfo2.TabIndex = 2;
            this.lblFetchEstSpaceInfo2.Text = "(Requires internet. Uses RAWG API.)";
            // 
            // lblFetchEstSpaceInfo1
            // 
            this.lblFetchEstSpaceInfo1.AutoSize = true;
            this.lblFetchEstSpaceInfo1.Location = new System.Drawing.Point(25, 44);
            this.lblFetchEstSpaceInfo1.Name = "lblFetchEstSpaceInfo1";
            this.lblFetchEstSpaceInfo1.Size = new System.Drawing.Size(200, 13);
            this.lblFetchEstSpaceInfo1.TabIndex = 1;
            this.lblFetchEstSpaceInfo1.Text = "(Gets developer's listed disk space)";
            // 
            // chkFetchEstRequiredSpace
            // 
            this.chkFetchEstRequiredSpace.AutoSize = true;
            this.chkFetchEstRequiredSpace.Location = new System.Drawing.Point(9, 24);
            this.chkFetchEstRequiredSpace.Name = "chkFetchEstRequiredSpace";
            this.chkFetchEstRequiredSpace.Size = new System.Drawing.Size(260, 17); // Text updated
            this.chkFetchEstRequiredSpace.TabIndex = 2; // Tab index after local scan options
            this.chkFetchEstRequiredSpace.Text = $"Fetch \"{Constants.CustomFieldEstRequiredSpace}\"";
            this.chkFetchEstRequiredSpace.UseVisualStyleBackColor = true;
            // 
            // grpGeneral
            //
            this.grpGeneral.Controls.Add(this.chkStoreLastScanned);
            this.grpGeneral.Location = new System.Drawing.Point(12, 249); // Adjusted Y
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(360, 50);
            this.grpGeneral.TabIndex = 2;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General Options";
            //
            // chkStoreLastScanned
            //
            this.chkStoreLastScanned.AutoSize = true;
            this.chkStoreLastScanned.Location = new System.Drawing.Point(9, 22);
            this.chkStoreLastScanned.Name = "chkStoreLastScanned";
            this.chkStoreLastScanned.Size = new System.Drawing.Size(220, 17); // Text updated
            this.chkStoreLastScanned.TabIndex = 3; // Tab index
            this.chkStoreLastScanned.Text = $"Update \"{Constants.CustomFieldLastScanned}\" date";
            this.chkStoreLastScanned.UseVisualStyleBackColor = true;
            //
            // lblOverallNote
            //
            this.lblOverallNote.Location = new System.Drawing.Point(12, 308); // Adjusted Y
            this.lblOverallNote.Name = "lblOverallNote";
            this.lblOverallNote.Size = new System.Drawing.Size(360, 30);
            this.lblOverallNote.TabIndex = 5;
            this.lblOverallNote.Text = "Note: Local scan requires valid game paths. Online fetch attempts to find data for any selected game.";
            //
            // btnStartScan
            //
            this.btnStartScan.Location = new System.Drawing.Point(200, 345); // Adjusted Y
            this.btnStartScan.Name = "btnStartScan";
            this.btnStartScan.Size = new System.Drawing.Size(80, 25);
            this.btnStartScan.TabIndex = 3;
            this.btnStartScan.Text = "Start Scan";
            this.btnStartScan.UseVisualStyleBackColor = false;
            this.btnStartScan.Click += new System.EventHandler(this.btnStartScan_Click);
            // 
            // btnCancelScan
            // 
            this.btnCancelScan.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelScan.Location = new System.Drawing.Point(292, 345); // Adjusted Y
            this.btnCancelScan.Name = "btnCancelScan";
            this.btnCancelScan.Size = new System.Drawing.Size(75, 25);
            this.btnCancelScan.TabIndex = 4;
            this.btnCancelScan.Text = "Cancel";
            this.btnCancelScan.UseVisualStyleBackColor = false;
            this.btnCancelScan.Click += new System.EventHandler(this.btnCancelScan_Click);
            // 
            // ScanOptionsDialog
            // 
            this.AcceptButton = this.btnStartScan;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelScan;
            this.ClientSize = new System.Drawing.Size(384, 381); // Adjusted Height
            this.Controls.Add(this.lblOverallNote);
            this.Controls.Add(this.grpGeneral);
            this.Controls.Add(this.grpOnlineFetch);
            this.Controls.Add(this.grpLocalScan);
            this.Controls.Add(this.btnCancelScan);
            this.Controls.Add(this.btnStartScan);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScanOptionsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = $"{Constants.PluginName} - Scan Options";
            this.grpLocalScan.ResumeLayout(false);
            this.grpLocalScan.PerformLayout();
            this.grpOnlineFetch.ResumeLayout(false);
            this.grpOnlineFetch.PerformLayout();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            this.ResumeLayout(false);
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            this.StoreGameSize = chkStoreGameSize.Checked;
            this.StoreLastScanned = chkStoreLastScanned.Checked;
            this.StoreSizeTier = chkStoreSizeTier.Checked;
            // FetchEstRequiredSpace is only true if enabled AND checked
            this.FetchEstRequiredSpace = chkFetchEstRequiredSpace.Enabled && chkFetchEstRequiredSpace.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelScan_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
// --- END OF FILE UI/ScanOptionsDialog.cs ---