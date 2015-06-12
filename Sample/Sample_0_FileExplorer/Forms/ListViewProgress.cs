using System;
using System.Drawing;
using System.Windows.Forms;

namespace FileExplorer
{
    public class ProgressListview : ListView
    {
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            ListViewItem.ListViewSubItem oitem = e.Item.SubItems[e.ColumnIndex];
            ProgressSubItem item = null;

            if (oitem is ProgressSubItem)
                item = (ProgressSubItem)oitem;
            if (item != null && item.ShowProgress && item.ProgressMaxValue > 0)
            {
                double percent = (double)item.ProgressValue / (double)item.ProgressMaxValue;
                if (percent > 1.0f)
                    percent = 1.0f;
                Rectangle rect = item.Bounds;
                Graphics g = e.Graphics;

                //绘制进度条
                Rectangle progressRect = new Rectangle(rect.X + 1, rect.Y + 3, rect.Width - 2, rect.Height - 5);
                g.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(0x51, 0x51, 0x51)), 1), progressRect);

                //绘制进度
                int progressMaxWidth = progressRect.Width - 1;
                int width = (int)(progressMaxWidth * percent);
                if (width >= progressMaxWidth) width = progressMaxWidth;
                g.FillRectangle(new SolidBrush(Color.FromArgb(0xa4, 0xa3, 0xa3)), new Rectangle(progressRect.X + 1, progressRect.Y + 1, width, progressRect.Height - 1));

                if(item.ShowPercentOverflowProgress)
                {
                    //绘制进度百分比
                    percent *= 100;
                    string percentText = string.Format("{0}%  {1}/{2}", percent.ToString("F2"), Utils.HumanReadableSize(item.ProgressValue), Utils.HumanReadableSize(item.ProgressMaxValue));
                    Size size = TextRenderer.MeasureText(percentText.ToString(), Font);
                    int x = (progressRect.Width - size.Width) / 2;
                    int y = (progressRect.Height - size.Height) / 2 + 3;
                    if (x <= 0) x = 1;
                    if (y <= 0) y = 1;
                    int w = size.Width;
                    int h = size.Height;
                    if (w > progressRect.Width) w = progressRect.Width;
                    if (h > progressRect.Height) h = progressRect.Height;
                    g.DrawString(percentText, this.Font, new SolidBrush(Color.Black), new Rectangle(rect.X + x, rect.Y + y, w, h));
                }
            }
            else
            {
                e.DrawDefault = true;
                base.OnDrawSubItem(e);
            }
        }

        public class ProgressSubItem : ListViewItem.ListViewSubItem
        {
            public ProgressSubItem()
                : base()
            {
                ShowProgress = true;
                ShowPercentOverflowProgress = true;
                ProgressValue = 0;
                ProgressMaxValue = 100;
            }

            public ProgressSubItem(ListViewItem owner, string text)
                : base(owner, text)
            {
                ShowProgress = true;
                ShowPercentOverflowProgress = true;
                ProgressValue = 0;
                ProgressMaxValue = 100;
            }

            public ProgressSubItem(ListViewItem owner, string text, Color foreColor, Color backColor, Font font)
                : base(owner, text, foreColor, backColor, font)
            {
                ShowProgress = true;
                ShowPercentOverflowProgress = true;
                ProgressValue = 0;
                ProgressMaxValue = 100;
            }

            public bool ShowProgress { get; set; }

            public bool ShowPercentOverflowProgress { get; set; }

            public long ProgressValue { get; set; }

            public long ProgressMaxValue { get; set; }

            public ListViewItem Owner { get; set; }
        }
    }

}
