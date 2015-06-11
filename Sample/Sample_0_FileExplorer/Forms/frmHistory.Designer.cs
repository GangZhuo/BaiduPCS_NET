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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDownloading = new System.Windows.Forms.TabPage();
            this.lvDownloading = new FileExplorer.ProgressListview();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabCompletedDownload = new System.Windows.Forms.TabPage();
            this.lvCompletedDownload = new FileExplorer.ProgressListview();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabUploading = new System.Windows.Forms.TabPage();
            this.lvUploading = new FileExplorer.ProgressListview();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabCompletedUpload = new System.Windows.Forms.TabPage();
            this.lvCompletedUpload = new FileExplorer.ProgressListview();
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabDownloading.SuspendLayout();
            this.tabCompletedDownload.SuspendLayout();
            this.tabUploading.SuspendLayout();
            this.tabCompletedUpload.SuspendLayout();
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
            this.tabControl1.Size = new System.Drawing.Size(437, 492);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDownloading
            // 
            this.tabDownloading.Controls.Add(this.lvDownloading);
            this.tabDownloading.Location = new System.Drawing.Point(4, 22);
            this.tabDownloading.Name = "tabDownloading";
            this.tabDownloading.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloading.Size = new System.Drawing.Size(429, 466);
            this.tabDownloading.TabIndex = 0;
            this.tabDownloading.Text = "Downloading";
            this.tabDownloading.UseVisualStyleBackColor = true;
            // 
            // lvDownloading
            // 
            this.lvDownloading.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvDownloading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDownloading.Location = new System.Drawing.Point(3, 3);
            this.lvDownloading.Name = "lvDownloading";
            this.lvDownloading.OwnerDraw = true;
            this.lvDownloading.ShowItemToolTips = true;
            this.lvDownloading.Size = new System.Drawing.Size(423, 460);
            this.lvDownloading.TabIndex = 0;
            this.lvDownloading.UseCompatibleStateImageBehavior = false;
            this.lvDownloading.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "From";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "To";
            this.columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Status";
            this.columnHeader3.Width = 80;
            // 
            // tabCompletedDownload
            // 
            this.tabCompletedDownload.Controls.Add(this.lvCompletedDownload);
            this.tabCompletedDownload.Location = new System.Drawing.Point(4, 22);
            this.tabCompletedDownload.Name = "tabCompletedDownload";
            this.tabCompletedDownload.Padding = new System.Windows.Forms.Padding(3);
            this.tabCompletedDownload.Size = new System.Drawing.Size(429, 466);
            this.tabCompletedDownload.TabIndex = 1;
            this.tabCompletedDownload.Text = "Completed Download";
            this.tabCompletedDownload.UseVisualStyleBackColor = true;
            // 
            // lvCompletedDownload
            // 
            this.lvCompletedDownload.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lvCompletedDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvCompletedDownload.Location = new System.Drawing.Point(3, 3);
            this.lvCompletedDownload.Name = "lvCompletedDownload";
            this.lvCompletedDownload.OwnerDraw = true;
            this.lvCompletedDownload.ShowItemToolTips = true;
            this.lvCompletedDownload.Size = new System.Drawing.Size(423, 460);
            this.lvCompletedDownload.TabIndex = 1;
            this.lvCompletedDownload.UseCompatibleStateImageBehavior = false;
            this.lvCompletedDownload.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "From";
            this.columnHeader4.Width = 150;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "To";
            this.columnHeader5.Width = 150;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Status";
            this.columnHeader6.Width = 80;
            // 
            // tabUploading
            // 
            this.tabUploading.Controls.Add(this.lvUploading);
            this.tabUploading.Location = new System.Drawing.Point(4, 22);
            this.tabUploading.Name = "tabUploading";
            this.tabUploading.Size = new System.Drawing.Size(429, 466);
            this.tabUploading.TabIndex = 2;
            this.tabUploading.Text = "Uploading";
            this.tabUploading.UseVisualStyleBackColor = true;
            // 
            // lvUploading
            // 
            this.lvUploading.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9});
            this.lvUploading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvUploading.Location = new System.Drawing.Point(0, 0);
            this.lvUploading.Name = "lvUploading";
            this.lvUploading.OwnerDraw = true;
            this.lvUploading.ShowItemToolTips = true;
            this.lvUploading.Size = new System.Drawing.Size(429, 466);
            this.lvUploading.TabIndex = 1;
            this.lvUploading.UseCompatibleStateImageBehavior = false;
            this.lvUploading.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "From";
            this.columnHeader7.Width = 150;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "To";
            this.columnHeader8.Width = 150;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Status";
            this.columnHeader9.Width = 80;
            // 
            // tabCompletedUpload
            // 
            this.tabCompletedUpload.Controls.Add(this.lvCompletedUpload);
            this.tabCompletedUpload.Location = new System.Drawing.Point(4, 22);
            this.tabCompletedUpload.Name = "tabCompletedUpload";
            this.tabCompletedUpload.Size = new System.Drawing.Size(429, 466);
            this.tabCompletedUpload.TabIndex = 3;
            this.tabCompletedUpload.Text = "Completed Upload";
            this.tabCompletedUpload.UseVisualStyleBackColor = true;
            // 
            // lvCompletedUpload
            // 
            this.lvCompletedUpload.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12});
            this.lvCompletedUpload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvCompletedUpload.Location = new System.Drawing.Point(0, 0);
            this.lvCompletedUpload.Name = "lvCompletedUpload";
            this.lvCompletedUpload.OwnerDraw = true;
            this.lvCompletedUpload.ShowItemToolTips = true;
            this.lvCompletedUpload.Size = new System.Drawing.Size(429, 466);
            this.lvCompletedUpload.TabIndex = 1;
            this.lvCompletedUpload.UseCompatibleStateImageBehavior = false;
            this.lvCompletedUpload.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "From";
            this.columnHeader10.Width = 150;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "To";
            this.columnHeader11.Width = 150;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Status";
            this.columnHeader12.Width = 80;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 492);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(437, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // frmHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 514);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "frmHistory";
            this.Text = "History";
            this.Load += new System.EventHandler(this.frmHistory_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabDownloading.ResumeLayout(false);
            this.tabCompletedDownload.ResumeLayout(false);
            this.tabUploading.ResumeLayout(false);
            this.tabCompletedUpload.ResumeLayout(false);
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
        private ProgressListview lvDownloading;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private ProgressListview lvCompletedDownload;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private ProgressListview lvUploading;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private ProgressListview lvCompletedUpload;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.Timer timer1;
    }
}