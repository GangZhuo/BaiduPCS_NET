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
    public partial class frmProgress : Form
    {
        public bool Cancelled { get; private set; }

        public string Label1 { get; set; }

        public string Label2 { get; set; }

        public int Value { get; set; }


        public frmProgress()
        {
            InitializeComponent();
        }

        private void frmProgress_Load(object sender, EventArgs e)
        {
            Cancelled = false;
            timer1.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Cancelled = true;
            timer1.Stop();
            base.OnClosing(e);
        }

        public void CloseEx()
        {
            Cancelled = true;
            if (this.InvokeRequired)
            {
                this.Invoke(new VoidFun(delegate()
                {
                    Close();
                }));
            }
            else
            {
                Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Cancelled)
                return;
            label1.Text = Label1;
            label2.Text = Label2;
            progressBar1.Value = Value;
        }

    }
    public delegate void VoidFun();
}
