using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET
{
    public static class NativeMethods
    {
        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_version();

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_create(IntPtr cookie_file);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void pcs_destroy(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void pcs_clone_userinfo(IntPtr dst, IntPtr src);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_sysUID(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_strerror(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_setopt(IntPtr handle, int opt, IntPtr value);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_islogin(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_login(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_logout(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_quota(IntPtr handle, IntPtr quota, IntPtr used);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_mkdir(IntPtr handle, IntPtr path);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_list(IntPtr handle, IntPtr dir, int pageindex, int pagesize, IntPtr order, byte desc);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_search(IntPtr handle, IntPtr dir, IntPtr key, byte recursion);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_meta(IntPtr handle, IntPtr path);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_delete(IntPtr handle, IntPtr slist);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_rename(IntPtr handle, IntPtr slist);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_move(IntPtr handle, IntPtr slist);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_copy(IntPtr handle, IntPtr slist);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_cat(IntPtr handle, IntPtr path, ref uint dstsz);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int pcs_download(IntPtr handle, IntPtr path, long max_speed, long resume_from);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static long pcs_get_download_filesize(IntPtr handle, IntPtr path);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_upload_buffer(IntPtr handle, IntPtr path, byte overwrite, IntPtr buffer, uint buffer_size);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_upload_slice(IntPtr handle, IntPtr buffer, uint buffer_size);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_upload_slicefile(IntPtr handle, NativePcsReadSliceFunction read_func, IntPtr userdata, uint content_size);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_create_superfile(IntPtr handle, IntPtr path, byte overwrite, IntPtr block_list);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_upload(IntPtr handle, IntPtr path, byte overwrite, IntPtr local_filename);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static long pcs_local_filesize(IntPtr handle, IntPtr path);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static byte pcs_md5_file(IntPtr handle, IntPtr path, IntPtr md5);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static byte pcs_md5_file_slice(IntPtr handle, IntPtr path, long offset, long length, IntPtr md5_buf);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_rapid_upload(IntPtr handle, IntPtr path, byte overwrite, IntPtr local_filename, IntPtr content_md5, IntPtr slice_md5);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_cookie_data(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_req_rawdata(IntPtr handle, IntPtr size, IntPtr encode);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static double pcs_speed_download(IntPtr handle);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr md5_string(IntPtr s);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr type_size();

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int str_len(IntPtr s);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void pcs_filist_destroy(IntPtr list);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void pcs_fileinfo_destroy(IntPtr fi);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void pcs_pan_api_res_destroy(IntPtr res);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr pcs_malloc_4_net(uint sz);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void pcs_free_4_net(IntPtr ptr);

        [DllImport("BaiduPCS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr time_str(long time);
    }
}
