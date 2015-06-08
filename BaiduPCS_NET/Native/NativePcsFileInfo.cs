using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    /// <summary>
    /// 用于存储网盘中文件的元数据
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct NativePcsFileInfo
    {
        public ulong fs_id;
        public IntPtr path;
        public IntPtr server_filename;
        public long server_ctime;
        public long server_mtime;
        public long local_ctime;
        public long local_mtime;
        public long size;
        public int category;
        [MarshalAs(UnmanagedType.U1)]
        public bool isdir;
        [MarshalAs(UnmanagedType.U1)]
        public bool dir_empty;
        [MarshalAs(UnmanagedType.U1)]
        public bool empty;
        public IntPtr md5;
        public IntPtr dlink;
        public IntPtr block_list;
        [MarshalAs(UnmanagedType.U1)]
        public bool ifhassubdir;

        public int user_flag;
    }
}
