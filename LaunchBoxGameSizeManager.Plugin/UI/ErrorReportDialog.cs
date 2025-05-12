using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading; // Required for Thread and ApartmentState
using System.Linq; // Required for .Any()
using LaunchBoxGameSizeManager.Utils; // For Constants

namespace LaunchBoxGameSizeManager.UI
{
    public partial class ErrorReportDialog : Form
    {
        private Label lblHeader;
        private TextBox txtErrorList;
        private Button btnCopyToClipboard;
        private Button btnClose;

        public ErrorReportDialog(string contextDescription, Dictionary<string, List<string>> categorizedErrors)
        {
            InitializeComponent();
            ApplyDarkTheme();
            this.Text = $"{Constants.PluginName} - Scan Issues for {contextDescription}";
            PopulateErrors(categorizedErrors);
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.FromArgb(220, 220, 220);

            lblHeader.ForeColor = this.ForeColor;
            txtErrorList.BackColor = Color.FromArgb(30, 30, 30);
            txtErrorList.ForeColor = Color.FromArgb(210, 210, 210);

            StyleButton(btnCopyToClipboard);
            StyleButton(btnClose);
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

        private void PopulateErrors(Dictionary<string, List<string>> categorizedErrors)
        {
            if (categorizedErrors == null || !categorizedErrors.Any())
            {
                txtErrorList.Text = "No specific issues reported during the scan.";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var category in categorizedErrors.OrderBy(kvp => kvp.Key)) // Optional: sort categories
            {
                sb.AppendLine($"--- {category.Key} ({category.Value.Count} game(s)) ---");
                foreach (var item in category.Value.OrderBy(i => i)) // Optional: sort items in category
                {
                    sb.AppendLine($"- {item}");
                }
                sb.AppendLine();
            }
            txtErrorList.Text = sb.ToString();
        }

        private void InitializeComponent()
        {
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtErrorList = new System.Windows.Forms.TextBox();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(12, 9);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(205, 13);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "The following issues were encountered:";
            // 
            // txtErrorList
            // 
            this.txtErrorList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtErrorList.Location = new System.Drawing.Point(15, 30);
            this.txtErrorList.Multiline = true;
            this.txtErrorList.Name = "txtErrorList";
            this.txtErrorList.ReadOnly = true;
            this.txtErrorList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtErrorList.Size = new System.Drawing.Size(457, 220);
            this.txtErrorList.TabIndex = 1;
            this.txtErrorList.WordWrap = false;
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopyToClipboard.Location = new System.Drawing.Point(15, 260);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(120, 25);
            this.btnCopyToClipboard.TabIndex = 2;
            this.btnCopyToClipboard.Text = "Copy to Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = false;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(397, 260);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 25);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // ErrorReportDialog
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 297);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.txtErrorList);
            this.Controls.Add(this.lblHeader);
            this.MinimizeBox = false;
            this.Name = "ErrorReportDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Scan Issue Report";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtErrorList.Text))
            {
                MessageBox.Show(this, "There is no content to copy.", "Clipboard Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Thread staThread = new Thread(
                () => {
                    try
                    {
                        Clipboard.SetText(txtErrorList.Text, TextDataFormat.UnicodeText); // Specify format for robustness
                        // Successfully copied, inform user on UI thread
                        this.BeginInvoke((MethodInvoker)delegate {
                            MessageBox.Show(this, "Error report copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[ErrorReportDialog] STA Clipboard Error: {ex.Message}");
#endif
                        // Inform user of failure on UI thread
                        this.BeginInvoke((MethodInvoker)delegate {
                            MessageBox.Show(this, $"Failed to copy to clipboard: {ex.Message}", "Clipboard Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            // We don't strictly need to staThread.Join() here, 
            // the copy operation is usually fast. The user gets feedback via MessageBox.
        }
    }
}