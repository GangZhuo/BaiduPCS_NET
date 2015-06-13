using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class frmFileName : Form
    {
        public string FileName { get; set; }

        public frmFileName(string defaultName)
        {
            InitializeComponent();
            txName.Text = defaultName;
            FileName = defaultName;
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            txName.ImeMode = System.Windows.Forms.ImeMode.OnHalf;
        }

        private void frmFileName_Load(object sender, EventArgs e)
        {

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string filename = txName.Text.Trim();
            if(string.IsNullOrEmpty(filename))
            {
                MessageBox.Show("Please input the name.");
                return;
            }
            FileName = filename;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
