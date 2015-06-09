namespace FileExplorer
{
    partial class frmMetaInformation
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.txFileName = new System.Windows.Forms.TextBox();
            this.txFileType = new System.Windows.Forms.TextBox();
            this.txLocation = new System.Windows.Forms.TextBox();
            this.txSize = new System.Windows.Forms.TextBox();
            this.txCreateTime = new System.Windows.Forms.TextBox();
            this.txModifyTime = new System.Windows.Forms.TextBox();
            this.txMd5 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Location:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Size:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 193);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Create time:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 222);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Modify time:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 251);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "md5:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 67);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "File type:";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(170, 291);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(58, 25);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txFileName
            // 
            this.txFileName.Location = new System.Drawing.Point(99, 35);
            this.txFileName.Name = "txFileName";
            this.txFileName.ReadOnly = true;
            this.txFileName.Size = new System.Drawing.Size(272, 20);
            this.txFileName.TabIndex = 1;
            // 
            // txFileType
            // 
            this.txFileType.Location = new System.Drawing.Point(99, 64);
            this.txFileType.Name = "txFileType";
            this.txFileType.ReadOnly = true;
            this.txFileType.Size = new System.Drawing.Size(272, 20);
            this.txFileType.TabIndex = 2;
            // 
            // txLocation
            // 
            this.txLocation.Location = new System.Drawing.Point(99, 113);
            this.txLocation.Name = "txLocation";
            this.txLocation.ReadOnly = true;
            this.txLocation.Size = new System.Drawing.Size(272, 20);
            this.txLocation.TabIndex = 3;
            // 
            // txSize
            // 
            this.txSize.Location = new System.Drawing.Point(99, 142);
            this.txSize.Name = "txSize";
            this.txSize.ReadOnly = true;
            this.txSize.Size = new System.Drawing.Size(272, 20);
            this.txSize.TabIndex = 4;
            // 
            // txCreateTime
            // 
            this.txCreateTime.Location = new System.Drawing.Point(99, 190);
            this.txCreateTime.Name = "txCreateTime";
            this.txCreateTime.ReadOnly = true;
            this.txCreateTime.Size = new System.Drawing.Size(272, 20);
            this.txCreateTime.TabIndex = 5;
            // 
            // txModifyTime
            // 
            this.txModifyTime.Location = new System.Drawing.Point(99, 219);
            this.txModifyTime.Name = "txModifyTime";
            this.txModifyTime.ReadOnly = true;
            this.txModifyTime.Size = new System.Drawing.Size(272, 20);
            this.txModifyTime.TabIndex = 6;
            // 
            // txMd5
            // 
            this.txMd5.Location = new System.Drawing.Point(99, 248);
            this.txMd5.Name = "txMd5";
            this.txMd5.ReadOnly = true;
            this.txMd5.Size = new System.Drawing.Size(272, 20);
            this.txMd5.TabIndex = 7;
            // 
            // frmAttributes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 345);
            this.Controls.Add(this.txMd5);
            this.Controls.Add(this.txModifyTime);
            this.Controls.Add(this.txCreateTime);
            this.Controls.Add(this.txSize);
            this.Controls.Add(this.txLocation);
            this.Controls.Add(this.txFileType);
            this.Controls.Add(this.txFileName);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAttributes";
            this.Opacity = 0.92D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Meta information";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox txFileName;
        private System.Windows.Forms.TextBox txFileType;
        private System.Windows.Forms.TextBox txLocation;
        private System.Windows.Forms.TextBox txSize;
        private System.Windows.Forms.TextBox txCreateTime;
        private System.Windows.Forms.TextBox txModifyTime;
        private System.Windows.Forms.TextBox txMd5;
    }
}