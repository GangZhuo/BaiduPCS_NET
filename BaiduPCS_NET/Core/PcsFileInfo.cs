using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using BaiduPCS_NET.Native;

namespace BaiduPCS_NET
{
    public struct PcsFileInfo
    {
        public ulong fs_id;
        public string path;
        public string server_filename;
        public long server_ctime;
        public long server_mtime;
        public long local_ctime;
        public long local_mtime;
        public long size;
        public int category;
        public bool isdir;
        public bool dir_empty;
        public bool empty;
        public string md5;
        public string dlink;
        public string[] block_list;
        public bool ifhassubdir;
        public int user_flag;

        public PcsFileInfo(NativePcsFileInfo fi)
        {
            this.fs_id = fi.fs_id;
            this.path = NativeUtils.utf8_str(fi.path);
            this.server_filename = NativeUtils.utf8_str(fi.server_filename);
            this.server_ctime = fi.server_ctime;
            this.server_mtime = fi.server_mtime;
            this.local_ctime = fi.local_ctime;
            this.local_mtime = fi.local_mtime;
            this.size = fi.size;
            this.category = fi.category;
            this.isdir = fi.isdir;
            this.dir_empty = fi.dir_empty;
            this.empty = fi.empty;
            this.md5 = NativeUtils.utf8_str(fi.md5);
            this.dlink = NativeUtils.utf8_str(fi.dlink);
            this.block_list = null;
            if (fi.block_list != IntPtr.Zero)
            {
                List<string> list = new List<string>();
                for (int i = 0; ; i++)
                {
                    IntPtr p = Marshal.ReadIntPtr(fi.block_list, i * Marshal.SizeOf(typeof(IntPtr)));
                    if (p != IntPtr.Zero)
                    {
                        list.Add(NativeUtils.utf8_str(p));
                    }
                    else
                    {
                        break;
                    }
                }
                this.block_list = list.ToArray();
            }
            this.ifhassubdir = fi.ifhassubdir;
            this.user_flag = fi.user_flag;
        }

        public bool IsEmpty
        {
            get { return this.fs_id == 0; }
        }

        public string block_list_str
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (block_list != null)
                {
                    foreach (string s in block_list)
                    {
                        if (sb.Length > 0) sb.Append(", ");
                        sb.Append("\"" + s + "\"");
                    }
                }
                return "[ " + sb.ToString() + " ]";
            }
        }
    }
}
