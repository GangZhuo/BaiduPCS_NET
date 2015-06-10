using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

using BaiduPCS_NET;

namespace FileExplorer
{
    class SystemIcon
    {
        public static Icon GetFolderIcon(bool isLarge)
        {
            string regIconString = null;
            string systemDirectory = Environment.SystemDirectory + "\\";
            //直接指定为文件夹图标
            regIconString = systemDirectory + "shell32.dll,3";
            string[] fileIcon = regIconString.Split(new char[] { ',' });
            Icon resultIcon = null;
            try
            {
                //调用API方法读取图标
                int[] phiconLarge = new int[1];
                int[] phiconSmall = new int[1];
                uint count = Win32.ExtractIconEx(fileIcon[0], Int32.Parse(fileIcon[1]), phiconLarge, phiconSmall, 1);
                IntPtr IconHnd = new IntPtr(isLarge ? phiconLarge[0] : phiconSmall[0]);
                resultIcon = Icon.FromHandle(IconHnd);
            }
            catch { }
            return resultIcon;
        }

        /// <summary>
        /// 给出文件扩展名（.*），返回相应图标
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="isLarge"></param>
        /// <returns></returns>
        public static Icon GetIcon(PcsFileInfo fileInfo, bool isLarge)
        {
            if (fileInfo.IsEmpty || string.IsNullOrEmpty(fileInfo.server_filename))
                return GetUnknowTypeIcon(isLarge);

            if (fileInfo.isdir)
                return GetFolderIcon(isLarge);

            string ext = Path.GetExtension(fileInfo.server_filename);
            if (string.IsNullOrEmpty(ext))
                return GetUnknowTypeIcon(isLarge);

            RegistryKey regVersion = null;
            string regFileType = null;
            string regIconString = null;
            string systemDirectory = Environment.SystemDirectory + "\\";

            try
            {
                //读系统注册表中文件类型信息
                regVersion = Registry.ClassesRoot.OpenSubKey(ext, true);
                if (regVersion != null)
                {
                    regFileType = regVersion.GetValue("") as string;
                    regVersion.Close();
                    regVersion = Registry.ClassesRoot.OpenSubKey(regFileType + @"\DefaultIcon", true);
                    if (regVersion != null)
                    {
                        regIconString = regVersion.GetValue("") as string;
                        regVersion.Close();
                    }
                }
            }
            catch { }

            Icon resultIcon = GetIcon(regIconString, isLarge);
            if (resultIcon != null)
                return resultIcon;

            //后缀不是 ".exe"， 返回未知文件类型的图标
            if (!string.Equals(ext, ".exe", StringComparison.InvariantCultureIgnoreCase))
                return GetUnknowTypeIcon(isLarge);

            regIconString = systemDirectory + "shell32.dll,2"; //返回可执行文件的通用图标
            resultIcon = GetIcon(regIconString, isLarge);
            if (resultIcon == null)
                return GetUnknowTypeIcon(isLarge);
            return resultIcon;
        }

        /// <summary>
        /// 获取未知文件类型的图标
        /// </summary>
        /// <param name="isLarge"></param>
        /// <returns></returns>
        private static Icon GetUnknowTypeIcon(bool isLarge)
        {
            string systemDirectory = Environment.SystemDirectory + "\\";
            return GetIcon(systemDirectory + "shell32.dll,0", isLarge); // 返回未知文件类型的图标
        }

        /// <summary>
        /// 根据注册表项返回图标
        /// </summary>
        /// <param name="regIconString"></param>
        /// <param name="isLarge"></param>
        /// <returns></returns>
        private static Icon GetIcon(string regIconString, bool isLarge)
        {
            if (string.IsNullOrEmpty(regIconString))
                return null;
            string[] fileIcon = regIconString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (fileIcon.Length != 2)
                return null;
            Icon resultIcon = null;
            try
            {
                //调用API方法读取图标
                int[] phiconLarge = new int[1];
                int[] phiconSmall = new int[1];
                uint count = Win32.ExtractIconEx(fileIcon[0], Int32.Parse(fileIcon[1]), phiconLarge, phiconSmall, 1);
                IntPtr IconHnd = new IntPtr(isLarge ? phiconLarge[0] : phiconSmall[0]);
                resultIcon = Icon.FromHandle(IconHnd);
            }
            catch { }
            return resultIcon;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    /// <summary>
    /// 定义调用的API方法
    /// </summary>
    class Win32
    {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        [DllImport("shell32.dll")]
        public static extern uint ExtractIconEx(string lpszFile, int nIconIndex, int[] phiconLarge, int[] phiconSmall, uint nIcons);

    }

}
