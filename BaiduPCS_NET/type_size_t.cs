using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET
{
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct type_size_t
    {
        public int sz_int;
        public int sz_long;
        public int sz_long_long;
        public int sz_int64_t;
        public int sz_size_t;
        public int sz_curl_off_t;
        public int sz_time_t;
        public int sz_pcs_bool_t;
    }
}
