namespace FileExplorer
{
    partial class frmHistory
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDownloading = new System.Windows.Forms.TabPage();
            this.tabCompletedDownload = new System.Windows.Forms.TabPage();
            this.tabUploading = new System.Windows.Forms.TabPage();
            this.tabCompletedUpload = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDownloading);
            this.tabControl1.Controls.Add(this.tabCompletedDownload);
            this.tabControl1.Controls.Add(this.tabUploading);
            this.tabControl1.Controls.Add(this.tabCompletedUpload);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(413, 492);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDownloading
            // 
            this.tabDownloading.Location = new System.Drawing.Point(4, 22);
            this.tabDownloading.Name = "tabDownloading";
            this.tabDownloading.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloading.Size = new System.Drawing.Size(405, 466);
            this.tabDownloading.TabIndex = 0;
            this.tabDownloading.Text = "Downloading";
            this.tabDownloading.UseVisualStyleBackColor = true;
            // 
            // tabCompletedDownload
            // 
            this.tabCompletedDownload.Location = new System.Drawing.Point(4, 22);
            this.tabCompletedDownload.Name = "tabCompletedDownload";
            this.tabCompletedDownload.Padding = new System.Windows.Forms.Padding(3);
            this.tabCompletedDownload.Size = new System.Drawing.Size(405, 488);
            this.tabCompletedDownload.TabIndex = 1;
            this.tabCompletedDownload.Text = "Completed Download";
            this.tabCompletedDownload.UseVisualStyleBackColor = true;
            // 
            // tabUploading
            // 
            this.tabUploading.Location = new System.Drawing.Point(4, 22);
            this.tabUploading.Name = "tabUploading";
            this.tabUploading.Size = new System.Drawing.Size(405, 488);
            this.tabUploading.TabIndex = 2;
            this.tabUploading.Text = "Uploading";
            this.tabUploading.UseVisualStyleBackColor = true;
            // 
            // tabCompletedUpload
            // 
            this.tabCompletedUpload.Location = new System.Drawing.Point(4, 22);
            this.tabCompletedUpload.Name = "tabCompletedUpload";
            this.tabCompletedUpload.Size = new System.Drawing.Size(405, 488);
            this.tabCompletedUpload.TabIndex = 3;
            this.tabCompletedUpload.Text = "Completed Upload";
            this.tabCompletedUpload.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 492);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(413, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // frmHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 514);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "frmHistory";
            this.Text = "History";
            this.Load += new System.EventHandler(this.frmHistory_Load);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDownloading;
        private System.Windows.Forms.TabPage tabCompletedDownload;
        private System.Windows.Forms.TabPage tabUploading;
        private System.Windows.Forms.TabPage tabCompletedUpload;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}