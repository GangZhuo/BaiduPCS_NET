namespace FileExplorer
{
    partial class frmSettings
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
            this.tabDownloadOptions = new System.Windows.Forms.TabPage();
            this.tabUploadOptions = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.numDownloadMaxThreadCount = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.numUploadMaxThreadCount = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.ckAutomaticDownloadMaxThreadCount = new System.Windows.Forms.CheckBox();
            this.ckAutomaticUploadMaxThreadCount = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabDownloadOptions.SuspendLayout();
            this.tabUploadOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDownloadMaxThreadCount)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numUploadMaxThreadCount)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDownloadOptions);
            this.tabControl1.Controls.Add(this.tabUploadOptions);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(367, 293);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDownloadOptions
            // 
            this.tabDownloadOptions.Controls.Add(this.ckAutomaticDownloadMaxThreadCount);
            this.tabDownloadOptions.Controls.Add(this.numDownloadMaxThreadCount);
            this.tabDownloadOptions.Controls.Add(this.label1);
            this.tabDownloadOptions.Location = new System.Drawing.Point(4, 22);
            this.tabDownloadOptions.Name = "tabDownloadOptions";
            this.tabDownloadOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloadOptions.Size = new System.Drawing.Size(359, 267);
            this.tabDownloadOptions.TabIndex = 0;
            this.tabDownloadOptions.Text = "Download Options";
            this.tabDownloadOptions.UseVisualStyleBackColor = true;
            // 
            // tabUploadOptions
            // 
            this.tabUploadOptions.Controls.Add(this.ckAutomaticUploadMaxThreadCount);
            this.tabUploadOptions.Controls.Add(this.numUploadMaxThreadCount);
            this.tabUploadOptions.Controls.Add(this.label2);
            this.tabUploadOptions.Location = new System.Drawing.Point(4, 22);
            this.tabUploadOptions.Name = "tabUploadOptions";
            this.tabUploadOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabUploadOptions.Size = new System.Drawing.Size(359, 267);
            this.tabUploadOptions.TabIndex = 1;
            this.tabUploadOptions.Text = "Upload Options";
            this.tabUploadOptions.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Max thread count: ";
            // 
            // numDownloadMaxThreadCount
            // 
            this.numDownloadMaxThreadCount.Location = new System.Drawing.Point(122, 14);
            this.numDownloadMaxThreadCount.Name = "numDownloadMaxThreadCount";
            this.numDownloadMaxThreadCount.Size = new System.Drawing.Size(120, 20);
            this.numDownloadMaxThreadCount.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 293);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(367, 41);
            this.panel1.TabIndex = 1;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(195, 9);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 99;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(276, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 100;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // numUploadMaxThreadCount
            // 
            this.numUploadMaxThreadCount.Location = new System.Drawing.Point(122, 14);
            this.numUploadMaxThreadCount.Name = "numUploadMaxThreadCount";
            this.numUploadMaxThreadCount.Size = new System.Drawing.Size(120, 20);
            this.numUploadMaxThreadCount.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Max thread count: ";
            // 
            // ckAutomaticDownloadMaxThreadCount
            // 
            this.ckAutomaticDownloadMaxThreadCount.AutoSize = true;
            this.ckAutomaticDownloadMaxThreadCount.Location = new System.Drawing.Point(49, 50);
            this.ckAutomaticDownloadMaxThreadCount.Name = "ckAutomaticDownloadMaxThreadCount";
            this.ckAutomaticDownloadMaxThreadCount.Size = new System.Drawing.Size(115, 17);
            this.ckAutomaticDownloadMaxThreadCount.TabIndex = 2;
            this.ckAutomaticDownloadMaxThreadCount.Text = "Automatic decision";
            this.ckAutomaticDownloadMaxThreadCount.UseVisualStyleBackColor = true;
            // 
            // ckAutomaticUploadMaxThreadCount
            // 
            this.ckAutomaticUploadMaxThreadCount.AutoSize = true;
            this.ckAutomaticUploadMaxThreadCount.Location = new System.Drawing.Point(49, 50);
            this.ckAutomaticUploadMaxThreadCount.Name = "ckAutomaticUploadMaxThreadCount";
            this.ckAutomaticUploadMaxThreadCount.Size = new System.Drawing.Size(115, 17);
            this.ckAutomaticUploadMaxThreadCount.TabIndex = 3;
            this.ckAutomaticUploadMaxThreadCount.Text = "Automatic decision";
            this.ckAutomaticUploadMaxThreadCount.UseVisualStyleBackColor = true;
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 334);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabDownloadOptions.ResumeLayout(false);
            this.tabDownloadOptions.PerformLayout();
            this.tabUploadOptions.ResumeLayout(false);
            this.tabUploadOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDownloadMaxThreadCount)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numUploadMaxThreadCount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDownloadOptions;
        private System.Windows.Forms.TabPage tabUploadOptions;
        private System.Windows.Forms.NumericUpDown numDownloadMaxThreadCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.NumericUpDown numUploadMaxThreadCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ckAutomaticDownloadMaxThreadCount;
        private System.Windows.Forms.CheckBox ckAutomaticUploadMaxThreadCount;
    }
}