using System;
using System.Drawing;
using System.Windows.Forms;
using LaunchBoxGameSizeManager.Utils; // For Constants

namespace LaunchBoxGameSizeManager.UI
{
    public partial class ScanOptionsDialog : Form
    {
        private CheckBox chkStoreGameSize;
        private CheckBox chkStoreLastScanned;
        private CheckBox chkStoreSizeTier;
        private Button btnStartScan;
        private Button btnCancelScan;
        private Label lblInfo;

        public bool StoreGameSize { get; private set; }
        public bool StoreLastScanned { get; private set; }
        public bool StoreSizeTier { get; private set; }

        public ScanOptionsDialog()
        {
            InitializeComponent();
            // Set defaults
            chkStoreGameSize.Checked = true;
            chkStoreLastScanned.Checked = true;
            chkStoreSizeTier.Checked = true;

            // Apply dark theme styling
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.FromArgb(220, 220, 220);
            
            this.lblInfo.ForeColor = this.ForeColor;
            this.chkStoreGameSize.ForeColor = this.ForeColor;
            this.chkStoreLastScanned.ForeColor = this.ForeColor;
            this.chkStoreSizeTier.ForeColor = this.ForeColor;

            // Style buttons
            StyleButton(this.btnStartScan);
            StyleButton(this.btnCancelScan);
        }

        private void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80); // Darker border
            btn.FlatAppearance.BorderSize = 1;
            btn.BackColor = Color.FromArgb(60, 60, 63);     // Slightly darker button background
            btn.ForeColor = Color.FromArgb(220, 220, 220); // Light text
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 75, 78);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 90, 93);
        }

        private void InitializeComponent()
        {
            this.chkStoreGameSize = new System.Windows.Forms.CheckBox();
            this.chkStoreLastScanned = new System.Windows.Forms.CheckBox();
            this.chkStoreSizeTier = new System.Windows.Forms.CheckBox();
            this.btnStartScan = new System.Windows.Forms.Button();
            this.btnCancelScan = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 19); // Adjusted Top for a bit more space
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(248, 13); // Adjusted size to fit text potentially
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "Select data fields to calculate and store:";
            // 
            // chkStoreGameSize
            // 
            this.chkStoreGameSize.AutoSize = true;
            this.chkStoreGameSize.Location = new System.Drawing.Point(15, 45); // Adjusted Top
            this.chkStoreGameSize.Name = "chkStoreGameSize";
            this.chkStoreGameSize.Size = new System.Drawing.Size(190, 17);
            this.chkStoreGameSize.TabIndex = 1;
            this.chkStoreGameSize.Text = $"Store \"{Constants.CustomFieldGameSize}\"";
            this.chkStoreGameSize.UseVisualStyleBackColor = true;
            // 
            // chkStoreLastScanned
            // 
            this.chkStoreLastScanned.AutoSize = true;
            this.chkStoreLastScanned.Location = new System.Drawing.Point(15, 68); // Adjusted Top
            this.chkStoreLastScanned.Name = "chkStoreLastScanned";
            this.chkStoreLastScanned.Size = new System.Drawing.Size(220, 17);
            this.chkStoreLastScanned.TabIndex = 2;
            this.chkStoreLastScanned.Text = $"Store \"{Constants.CustomFieldLastScanned}\"";
            this.chkStoreLastScanned.UseVisualStyleBackColor = true;
            // 
            // chkStoreSizeTier
            // 
            this.chkStoreSizeTier.AutoSize = true;
            this.chkStoreSizeTier.Location = new System.Drawing.Point(15, 91); // Adjusted Top
            this.chkStoreSizeTier.Name = "chkStoreSizeTier";
            this.chkStoreSizeTier.Size = new System.Drawing.Size(190, 17);
            this.chkStoreSizeTier.TabIndex = 3;
            this.chkStoreSizeTier.Text = $"Store \"{Constants.CustomFieldGameSizeTier}\"";
            this.chkStoreSizeTier.UseVisualStyleBackColor = true;
            // 
            // btnStartScan
            // 
            this.btnStartScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartScan.Location = new System.Drawing.Point(101, 124); // Adjusted position
            this.btnStartScan.Name = "btnStartScan";
            this.btnStartScan.Size = new System.Drawing.Size(80, 25); // Adjusted size
            this.btnStartScan.TabIndex = 4;
            this.btnStartScan.Text = "Start Scan";
            this.btnStartScan.UseVisualStyleBackColor = false; // Important for custom BackColor
            this.btnStartScan.Click += new System.EventHandler(this.btnStartScan_Click);
            // 
            // btnCancelScan
            // 
            this.btnCancelScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelScan.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelScan.Location = new System.Drawing.Point(187, 124); // Adjusted position
            this.btnCancelScan.Name = "btnCancelScan";
            this.btnCancelScan.Size = new System.Drawing.Size(75, 25); // Adjusted size
            this.btnCancelScan.TabIndex = 5;
            this.btnCancelScan.Text = "Cancel";
            this.btnCancelScan.UseVisualStyleBackColor = false; // Important for custom BackColor
            this.btnCancelScan.Click += new System.EventHandler(this.btnCancelScan_Click);
            // 
            // ScanOptionsDialog
            // 
            this.AcceptButton = this.btnStartScan;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancelScan;
            this.ClientSize = new System.Drawing.Size(284, 161); // Adjusted height
            this.Controls.Add(this.btnCancelScan);
            this.Controls.Add(this.btnStartScan);
            this.Controls.Add(this.chkStoreSizeTier);
            this.Controls.Add(this.chkStoreLastScanned);
            this.Controls.Add(this.chkStoreGameSize);
            this.Controls.Add(this.lblInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScanOptionsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = $"{Constants.PluginName} - Scan Options";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            this.StoreGameSize = chkStoreGameSize.Checked;
            this.StoreLastScanned = chkStoreLastScanned.Checked;
            this.StoreSizeTier = chkStoreSizeTier.Checked;
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