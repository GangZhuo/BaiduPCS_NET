using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class frmInput : Form
    {
        public string Value
        {
            get { return textBox1.Text.Trim(); }
        }

        public string Tips
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public frmInput()
        {
            InitializeComponent();
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void frmInput_Load(object sender, EventArgs e)
        {

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
