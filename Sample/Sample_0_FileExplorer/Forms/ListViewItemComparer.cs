using System;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using BaiduPCS_NET;

namespace FileExplorer
{
    public class ListViewItemComparer : IComparer
    {
        public enum ComparerBy
        {
            None,
            FileName,
            FileSize,
            FileType,
            FileModifyTime,
        }

        public int Column { get; set; }
        public ComparerBy By { get; set; }
        public bool Descending { get; set; }

        private int CompareByFileName(PcsFileInfo x, PcsFileInfo y)
        {
            if (x.isdir && y.isdir)
                return string.Compare(x.server_filename, y.server_filename);
            else if (x.isdir)
                return -1;
            else if (x.isdir)
                return 1;
            else
                return string.Compare(x.server_filename, y.server_filename);
        }

        private int CompareByFileSize(PcsFileInfo x, PcsFileInfo y)
        {
            if (x.isdir && y.isdir)
                return 0;
            else if (x.isdir)
                return -1;
            else if (y.isdir)
                return 1;
            else
                return x.size < y.size ? -1 : (x.size > y.size ? 1 : 0);
        }

        private int CompareByFileType(PcsFileInfo x, PcsFileInfo y)
        {
            if (x.isdir && y.isdir)
                return 0;
            else if (x.isdir)
                return -1;
            else if (y.isdir)
                return 1;
            else
                return string.Compare(Path.GetExtension(x.server_filename), Path.GetExtension(y.server_filename));
        }

        private int CompareByFileModifyTime(PcsFileInfo x, PcsFileInfo y)
        {
            if (x.isdir && y.isdir)
                return 0;
            else if (x.isdir)
                return -1;
            else if (y.isdir)
                return 1;
            else
                return x.server_mtime < y.server_mtime ? -1 : (x.server_mtime > y.server_mtime ? 1 : 0);
        }

        public int Compare(object x, object y)
        {
            int r, wt = Descending ? -1 : 1;
            ListViewItem xitem = (ListViewItem)x;
            ListViewItem yitem = (ListViewItem)y;
            if (xitem.Tag != null && yitem.Tag != null)
            {
                if ((xitem.Tag is PcsFileInfo) && (yitem.Tag is PcsFileInfo))
                {
                    PcsFileInfo xfi = (PcsFileInfo)xitem.Tag;
                    PcsFileInfo yfi = (PcsFileInfo)yitem.Tag;
                    switch (By)
                    {
                        case ComparerBy.FileName:
                            r = CompareByFileName(xfi, yfi);
                            break;
                        case ComparerBy.FileSize:
                            r = CompareByFileSize(xfi, yfi);
                            break;
                        case ComparerBy.FileType:
                            r = CompareByFileType(xfi, yfi);
                            break;
                        case ComparerBy.FileModifyTime:
                            r = CompareByFileModifyTime(xfi, yfi);
                            break;
                        default:
                            r = string.Compare(xitem.SubItems[Column].Text, yitem.SubItems[Column].Text);
                            break;
                    }
                    return r * wt;
                }
            }

            return wt * string.Compare(xitem.SubItems[Column].Text, yitem.SubItems[Column].Text);
        }
    }
}
