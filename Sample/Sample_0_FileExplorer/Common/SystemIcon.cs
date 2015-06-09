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
            if (fileIcon.Length != 2)
            {
                //系统注册表中注册的标图不能直接提取，则返回可执行文件的通用图标
                fileIcon = new string[] { systemDirectory + "shell32.dll", "2" };
            }
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
            if (fileInfo.IsEmpty || string.IsNullOrEmpty(fileInfo.server_filename)) return null;

            RegistryKey regVersion = null;
            string regFileType = null;
            string regIconString = null;
            string systemDirectory = Environment.SystemDirectory + "\\";

            if (fileInfo.isdir)
            {
                //直接指定为文件夹图标
                regIconString = systemDirectory + "shell32.dll,3";
            }
            else
            {
                string ext = Path.GetExtension(fileInfo.server_filename);
                if (string.IsNullOrEmpty(ext))
                {
                    //指定为未知文件类型的图标
                    regIconString = systemDirectory + "shell32.dll,0";
                }
                else
                {
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
                    if (regIconString == null)
                    {
                        //没有读取到文件类型注册信息，指定为未知文件类型的图标
                        regIconString = systemDirectory + "shell32.dll,0";
                    }
                }
            }
            
            string[] fileIcon = regIconString.Split(new char[] { ',' });
            if (fileIcon.Length != 2)
            {
                //系统注册表中注册的标图不能直接提取，则返回可执行文件的通用图标
                fileIcon = new string[] { systemDirectory + "shell32.dll", "2" };
            }
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
