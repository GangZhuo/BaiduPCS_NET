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
    public partial class frmCaptcha : Form
    {
        public string Captcha
        {
            get { return textBox1.Text.Trim(); }
        }

        public frmCaptcha(byte[] imgBytes)
        {
            InitializeComponent();

            using(System.IO.MemoryStream ms = new System.IO.MemoryStream(imgBytes))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                pictureBox1.Image = img;
            }
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void frmCaptcha_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
