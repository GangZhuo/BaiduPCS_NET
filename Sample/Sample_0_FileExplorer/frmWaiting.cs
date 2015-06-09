using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class frmWaiting : Form
    {
        public string _Title
        {
            get { return this.lblTitle.Text; }
            set { this.lblTitle.Text = value; }
        }
        public string _Description
        {
            get { return this.lblDescription.Text; }
            set { this.lblDescription.Text = value; }
        }

        public frmWaiting()
            : this("Wait", "Processing...")
        {
        }

        public frmWaiting(string title, string description)
        {
            InitializeComponent();
            this.lblTitle.Text = title;
            this.lblDescription.Text = description;
        }

        private void frmWait_Load(object sender, EventArgs e)
        {

        }

        #region Async Method

        private delegate void SetTextFunction(string text);

        public void SetTitle(string title)
        {
            if (this.lblTitle.InvokeRequired)
            {
                this.lblTitle.Invoke(new SetTextFunction(SetTitle), title);
            }
            else
            {
                this.lblTitle.Text = title;
            }
        }

        public void SetDescription(string description)
        {
            if (this.lblDescription.InvokeRequired)
            {
                this.lblDescription.Invoke(new SetTextFunction(SetDescription), description);
            }
            else
            {
                this.lblDescription.Text = description;
            }
        }

        #endregion

        #region Drag Window
        private Point mouseOffset;
        private bool isMouseDown = false;

        protected virtual bool IsTileBar(int x, int y)
        {
            return x > 0 && x < 150 && y > 0 && y < 26;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isMouseDown = false; 
            //if (IsTileBar(e.Location.X, e.Location.Y))
            //{
            //    if (e.Button == MouseButtons.Left)
            //    {
            //        isMouseDown = false;
            //    }
            //}
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isMouseDown = false;
            if (IsTileBar(e.Location.X, e.Location.Y))
            {
                int xOffset;
                int yOffset;
                if (e.Button == MouseButtons.Left)
                {
                    xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                    yOffset = -e.Y - SystemInformation.FrameBorderSize.Height;
                    mouseOffset = new Point(xOffset, yOffset);
                    isMouseDown = true;
                }
            }
        }
        #endregion
    }
}
