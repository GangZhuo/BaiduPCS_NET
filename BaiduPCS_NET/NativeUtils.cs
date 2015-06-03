using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET
{
    public static class NativeUtils
    {
        public static IntPtr pcs_malloc(uint sz)
        {
            return NativeMethods.pcs_malloc_4_net(sz);
        }

        public static void pcs_free(IntPtr ptr)
        {
            NativeMethods.pcs_free_4_net(ptr);
        }

        /// <summary>
        /// 转换为时间字符串
        /// </summary>
        /// <param name="time"></param>
        /// <returns>返回时间对应的字符串</returns>
        public static string time_str(long time)
        {
            IntPtr r = NativeMethods.time_str(time);
            string s = str(r);
            return s;
        }

        /// <summary>
        /// 返回 s 的 MD5 值
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string md5_string(string s)
        {
            IntPtr src = str_ptr(s);
            IntPtr r = NativeMethods.md5_string(src);
            string md5 = str(r);
            free_str_ptr(src);
            return md5;
        }

        /// <summary>
        /// 把 list 送入非托管空间，返回其指针
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IntPtr slist_ptr(string[] list)
        {
            if (list == null || list.Length == 0)
                return IntPtr.Zero;
            int sz = Marshal.SizeOf(typeof(NativePcsSList));
            IntPtr lstPtr = Marshal.AllocHGlobal(sz * list.Length);
            IntPtr p = lstPtr;
            byte[] bytes;
            NativePcsSList item;
            for (int i = 0; i < list.Length; i++)
            {
                item = new NativePcsSList();
                item.str = str_ptr(list[i]);
                if (i < list.Length - 1) /*不是最后一个*/
                    item.next = IntPtr.Add(p, Marshal.SizeOf(typeof(NativePcsSList)));
                else
                    item.next = IntPtr.Zero;
                bytes = StructToBytes(item);
                Marshal.Copy(bytes, 0, p, bytes.Length);
                p = item.next;
            }
            return lstPtr;
        }

        /// <summary>
        /// 释放非托管空间的由 slist_ptr() 创建的空间
        /// </summary>
        /// <param name="ptr"></param>
        public static void free_slist_ptr(IntPtr ptr)
        {
            IntPtr p = ptr;
            while (p != IntPtr.Zero)
            {
                NativePcsSList item = (NativePcsSList)Marshal.PtrToStructure(p, typeof(NativePcsSList));
                if (item.str != IntPtr.Zero)
                    free_str_ptr(item.str);
                p = item.next;
            }
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        /// 把 list 送入非托管空间，返回其指针
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IntPtr slist2_ptr(SPair[] list)
        {
            if (list == null || list.Length == 0)
                return IntPtr.Zero;
            int sz = Marshal.SizeOf(typeof(NativePcsSList2));
            IntPtr lstPtr = Marshal.AllocHGlobal(sz * list.Length);
            IntPtr p = lstPtr;
            byte[] bytes;
            NativePcsSList2 item;
            for (int i = 0; i < list.Length; i++)
            {
                item = new NativePcsSList2();
                item.str1 = str_ptr(list[i].str1);
                item.str2 = str_ptr(list[i].str2);
                if (i < list.Length - 1) /*不是最后一个*/
                    item.next = IntPtr.Add(p, Marshal.SizeOf(typeof(NativePcsSList2)));
                else
                    item.next = IntPtr.Zero;
                bytes = StructToBytes(item);
                Marshal.Copy(bytes, 0, p, bytes.Length);
                p = item.next;
            }
            return lstPtr;
        }

        /// <summary>
        /// 释放非托管空间的由 slist2_ptr() 创建的空间
        /// </summary>
        /// <param name="ptr"></param>
        public static void free_slist2_ptr(IntPtr ptr)
        {
            IntPtr p = ptr;
            while (p != IntPtr.Zero)
            {
                NativePcsSList2 item = (NativePcsSList2)Marshal.PtrToStructure(p, typeof(NativePcsSList2));
                if (item.str1 != IntPtr.Zero)
                    free_str_ptr(item.str1);
                if (item.str2 != IntPtr.Zero)
                    free_str_ptr(item.str2);
                p = item.next;
            }
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        /// 复制 s 到非托管空间
        /// </summary>
        /// <param name="s">待复制的字符串</param>
        /// <returns>返回非托管空间的指针</returns>
        public static IntPtr str_ptr(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, IntPtr.Add(ptr, bytes.Length), 1);
            return ptr;
        }

        /// <summary>
        /// 释放掉 str_ptr() 函数创建的非托管空间
        /// </summary>
        /// <param name="ptr">指向非托管空间的指针</param>
        public static void free_str_ptr(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        /// 转换非托管空间的 C 语言格式的以 '\0' 结尾的字符串为 .NET 空间的 String 对象
        /// </summary>
        /// <param name="p">指向待转换的非托管空间的字符串开始位置的指针</param>
        /// <returns></returns>
        public static string str(IntPtr p)
        {
            if (p == IntPtr.Zero)
                return string.Empty;
            int len = NativeMethods.str_len(p);
            byte[] bytes = new byte[len];
            Marshal.Copy(p, bytes, 0, len);
            string s = Encoding.UTF8.GetString(bytes);
            return s;
        }

        /// <summary>
        /// 测试目录，获取各数据类型在非托管空间中占用的字节大小
        /// </summary>
        /// <returns></returns>
        public static type_size_t type_size()
        {
            IntPtr structPtr = NativeMethods.type_size();
            return (type_size_t)Marshal.PtrToStructure(structPtr, typeof(type_size_t));
        }

        /// <summary>
        /// 从非托管空间中复制出文件列表
        /// </summary>
        /// <param name="listPtr"></param>
        /// <returns></returns>
        public static PcsFileInfo[] GetFileListFromPcsFileInfoListPtr(IntPtr listPtr)
        {
            if (listPtr == IntPtr.Zero)
                return null;
            NativePcsFileInfoList nlist = (NativePcsFileInfoList)Marshal.PtrToStructure(listPtr, typeof(NativePcsFileInfoList));
            PcsFileInfo[] r = new PcsFileInfo[nlist.count];
            IntPtr itemPtr = nlist.link;
            for (int i = 0; itemPtr != IntPtr.Zero && i < nlist.count; i++)
            {
                NativePcsFileInfoListItem item = (NativePcsFileInfoListItem)Marshal.PtrToStructure(itemPtr, typeof(NativePcsFileInfoListItem));
                NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(item.info, typeof(NativePcsFileInfo));
                PcsFileInfo fi = new PcsFileInfo(nfi);
                r[i] = fi;
                itemPtr = item.next;
            }
            return r;
        }

        public static PcsPanApiRes GetPcsPanApiResFromPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            NativePcsPanApiRes nlist = (NativePcsPanApiRes)Marshal.PtrToStructure(ptr, typeof(NativePcsPanApiRes));
            if (nlist.error != 0)
                return new PcsPanApiRes() { error = nlist.error };
            PcsPanApiRes res = new PcsPanApiRes() { error = nlist.error };
            IntPtr p = nlist.info_list;
            List<PcsPanApiResInfo> filist = new List<PcsPanApiResInfo>();
            for (int i = 0; p != IntPtr.Zero; i++)
            {
                NativePcsPanApiResInfoList il = (NativePcsPanApiResInfoList)Marshal.PtrToStructure(p, typeof(NativePcsPanApiResInfoList));
                filist.Add(il.info);
                p = il.next;
            }
            res.info_list = filist.ToArray();
            return res;
        }

        //Serialize Struct
        public static byte[] StructToBytes(object structObj)
        {
            //Get the size of Struct
            int size = Marshal.SizeOf(structObj);
            byte[] bytes = new byte[size];
            //Alloc memory
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //Copy struct object to alloced memory
            Marshal.StructureToPtr(structObj, structPtr, false);
            //Copy alloced memory data to byte array
            Marshal.Copy(structPtr, bytes, 0, size);
            //Free alloced memory
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }

        //Deserialize Struct
        public static object BytesToStuct(byte[] bytes, Type type)
        {
            //Get the size of Struct
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
                return null;
            //Alloc memory
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //Copy byte array to alloced memory
            Marshal.Copy(bytes, 0, structPtr, size);
            //Convert alloced memory to a Struct
            object obj = Marshal.PtrToStructure(structPtr, type);
            //Free alloced memory
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

    }
}
