using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    public static class NativeUtils
    {
        public static IntPtr pcs_malloc(uint sz)
        {
            if (BaiduPCS.pcs_isdebug())
                return NativeMethods.pcs_mem_malloc_arg1(sz);
            else
                return NativeMethods.pcs_mem_malloc_raw(sz);
        }

        public static void pcs_free(IntPtr ptr)
        {
            if (BaiduPCS.pcs_isdebug())
                NativeMethods.pcs_mem_free(ptr);
            else
                NativeMethods.pcs_mem_free_raw(ptr);
        }

        /// <summary>
        /// 转换为时间字符串
        /// </summary>
        /// <param name="time"></param>
        /// <returns>返回时间对应的字符串</returns>
        public static string pcs_time2str(long time)
        {
            IntPtr r = NativeMethods.pcs_time2str(time);
            string s = utf8_str(r);
            return s;
        }

        /// <summary>
        /// 返回 s 的 MD5 值
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string pcs_md5_string(string s)
        {
            IntPtr src = str_ptr(s);
            IntPtr r = NativeMethods.pcs_md5_string(src);
            string md5 = utf8_str(r);
            free_str_ptr(src);
            return md5;
        }

        /// <summary>
        /// 返回 bytes 的 MD5 值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string pcs_md5(byte[] bytes)
        {
            IntPtr src = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, src, bytes.Length);
            IntPtr r = NativeMethods.pcs_md5_bytes(src, bytes.Length);
            Marshal.FreeHGlobal(src);
            string md5 = utf8_str(r);
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
                item.str = utf8_str_ptr(list[i]);
                if (i < list.Length - 1) /*不是最后一个*/
                    item.next = IntPtrAdd(p, Marshal.SizeOf(typeof(NativePcsSList)));
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
                item.str1 = utf8_str_ptr(list[i].str1);
                item.str2 = utf8_str_ptr(list[i].str2);
                if (i < list.Length - 1) /*不是最后一个*/
                    item.next = IntPtrAdd(p, Marshal.SizeOf(typeof(NativePcsSList2)));
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
        /// 复制 s 到非托管空间。
        /// 使用系统默认编码送入数据。
        /// </summary>
        /// <param name="s">待复制的字符串</param>
        /// <returns>返回非托管空间的指针</returns>
        public static IntPtr str_ptr(string s)
        {
            return str_ptr(s, Encoding.Default);
        }

        /// <summary>
        /// 复制 s 到非托管空间。
        /// 使用 UTF-8 编码。
        /// </summary>
        /// <param name="s">待复制的字符串</param>
        /// <returns>返回非托管空间的指针</returns>
        public static IntPtr utf8_str_ptr(string s)
        {
            return str_ptr(s, Encoding.UTF8);
        }

        /// <summary>
        /// 复制 s 到非托管空间。
        /// 使用 encoding 指定的编码。
        /// </summary>
        /// <param name="s">待复制的字符串</param>
        /// <param name="encoding">非托管空间的字符编码</param>
        /// <returns>返回非托管空间的指针</returns>
        public static IntPtr str_ptr(string s, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(s); // 非托管空间
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, IntPtrAdd(ptr, bytes.Length), 1);
            return ptr;
        }

        /// <summary>
        /// 复制 s 到 ptr 指定的非托管空间。
        /// </summary>
        /// <param name="ptr">非托管空间的指针</param>
        /// <param name="s">待复制的字符串</param>
        /// <returns>返回非托管空间的指针</returns>
        public static IntPtr str_set(IntPtr ptr, string s, int maxSize = -1, bool setTerminal = true)
        {
            // 非托管空间的字节编码是操作系统的ASCII代码页编码，因此我应使用同样的编码来送入字符串。
            // 如果明确要求使用 UTF-8 等其他编码送入字符的话，请使用 str_utf8_set() 函数。
            return str_set(ptr, s, Encoding.Default, maxSize, setTerminal);
        }

        public static IntPtr str_utf8_set(IntPtr ptr, string s, int maxSize = -1, bool setTerminal = true)
        {
            return str_set(ptr, s, Encoding.UTF8, maxSize, setTerminal);
        }

        /// <summary>
        /// 复制 s 到 ptr 指定的非托管空间。
        /// </summary>
        /// <param name="ptr">非托管空间指针</param>
        /// <param name="s">待复制的字符串</param>
        /// <param name="encoding">非托管空间的字符编码</param>
        /// <param name="maxSize">最多复制多少个字节，不包含 '\0'</param>
        /// <param name="setTerminal">是否设置结尾符'\0'</param>
        /// <returns></returns>
        public static IntPtr str_set(IntPtr ptr, string s, Encoding encoding, int maxSize = -1, bool setTerminal = true)
        {
            byte[] bytes = encoding.GetBytes(s);
            if (maxSize == -1)
                maxSize = bytes.Length;
            else if (maxSize > bytes.Length)
                maxSize = bytes.Length;
            Marshal.Copy(bytes, 0, ptr, maxSize);
            //set NULL terminal
            if (setTerminal)
                Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, NativeUtils.IntPtrAdd(ptr, maxSize), 1);
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
        /// 转换非托管空间的 C 语言格式的以 '\0' 结尾的字符串为 .NET 空间的 String 对象。
        /// 使用系统默认编码解码，如果检测到 UTF-8 的 BOM，则使用 UTF-8 解码。
        /// </summary>
        /// <param name="p">指向待转换的非托管空间的字符串开始位置的指针</param>
        /// <returns>返回字符串</returns>
        public static string str(IntPtr p)
        {
            return str(p, Encoding.Default);
        }

        /// <summary>
        /// 转换非托管空间的 C 语言格式的以 '\0' 结尾的字符串为 .NET 空间的 String 对象。
        /// 使用使用 UTF-8 解码。
        /// </summary>
        /// <param name="p">指向待转换的非托管空间的字符串开始位置的指针</param>
        /// <returns>返回字符串</returns>
        public static string utf8_str(IntPtr p)
        {
            //非托管空间中返回的字符是 UTF-8 编码的，因此我们应该使用此函数
            return str(p, Encoding.UTF8);
        }

        /// <summary>
        /// 转换非托管空间的 C 语言格式的以 '\0' 结尾的字符串为 .NET 空间的 String 对象。
        /// 使用 encoding 指定的编码解码，如果检测到 UTF-8 的 BOM，则使用 UTF-8 解码。
        /// </summary>
        /// <param name="p">指向待转换的非托管空间的字符串开始位置的指针</param>
        /// <param name="encoding">编码类型</param>
        /// <returns>返回字符串</returns>
        public static string str(IntPtr p, Encoding encoding)
        {
            if (p == IntPtr.Zero)
                return string.Empty;
            int len = NativeMethods.pcs_strlen(p);
            byte[] bytes = new byte[len];
            Marshal.Copy(p, bytes, 0, len);
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            else
                return encoding.GetString(bytes);
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
                filist.Add(new PcsPanApiResInfo(il.info));
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

        public static IntPtr IntPtrAdd(IntPtr ptr, int offset)
        {
            return new IntPtr(ptr.ToInt64() + offset);
        }

    }
}
