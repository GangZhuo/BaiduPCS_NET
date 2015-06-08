using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET.Native
{
    #region Native Delegates

    /// <summary>
    /// 返回验证码是否成功输入。如果返回 0，则表示验证码没有输入，将中断登录进程。
    /// </summary>
    /// <param name="ptr">验证码图片的字节序</param>
    /// <param name="size">验证码图片字节序的大小，以字节为单位</param>
    /// <param name="captcha">用于接收验证码字符</param>
    /// <param name="captchaSize">captcha的最大长度</param>
    /// <param name="state">使用PCS_OPTION_CAPTCHA_FUNCTION_DATA选项设定的值原样传入</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte NativePcsGetCaptchaFunction(IntPtr ptr, uint size, IntPtr captcha, uint captchaSize, IntPtr state);

    /// <summary>
    /// 设定该回调后，Pcs每从网络获取到值，则调用该回调。例如下载时。
    /// </summary>
    /// <param name="ptr">从网络获取到的字节序</param>
    /// <param name="size">字节序的大小，以字节为单位</param>
    /// <param name="contentlength">本次请求，HTTP头中的Content-Length值</param>
    /// <param name="userdata">使用PCS_OPTION_DOWNLOAD_WRITE_FUNCTION_DATA选项设定的值原样传入</param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint NativePcsHttpWriteFunction(IntPtr ptr, uint size, uint contentlength, IntPtr userdata);

    /// <summary>
    /// 设定该回调后，Pcs每从网络获取到值，则调用该回调。
    /// 和PcsHttpWriteFunction的区别是，该回调是在获取到全部内容后触发,
    /// 而PcsHttpWriteFunction是每获取到一段字节序则触发。
    /// 每个HTTP请求，PcsHttpResponseFunction只会触发一次，而PcsHttpWriteFunction可能触发多次
    /// </summary>
    /// <param name="ptr">从网络获取到的字节序</param>
    /// <param name="size">字节序的大小，以字节为单位</param>
    /// <param name="state">使用PCS_OPTION_HTTP_RESPONSE_FUNCTION_DATE选项设定的值原样传入</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativePcsHttpResponseFunction(IntPtr ptr, uint size, IntPtr state);

    /// <summary>
    /// 设定该回调后，Pcs每上传或下载一段字节序到网络中时，则调用该回调。利用该回调可实现上传时的进度条。
    /// 注意：只有设定PCS_OPTION_PROGRESS的值为PcsTrue后才会启用进度条
    /// </summary>
    /// <param name="clientp">使用PCS_OPTION_PROGRESS_FUNCTION_DATE选项设定的值原样传入</param>
    /// <param name="dltotal">从网络中需要下载多少字节</param>
    /// <param name="dlnow">从网络中已经下载多少字节</param>
    /// <param name="ultotal">需要上传多少字节</param>
    /// <param name="ulnow">已经上传多少字节</param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int NativePcsHttpProgressFunction(IntPtr clientp, double dltotal, double dlnow, double ultotal, double ulnow);

    /// <summary>
    /// 当分片上传一个文件时，需要读取分片数据时，触发该函数。
    /// 用于读取分片的数据。
    /// 查看： http://curl.haxx.se/libcurl/c/CURLOPT_READFUNCTION.html
    /// </summary>
    /// <param name="buf">分片数据将读入到这里</param>
    /// <param name="size">每对象的大小</param>
    /// <param name="nmemb">读入多少个对象数据</param>
    /// <param name="userdata"></param>
    /// <returns>返回 0 表示已经到文件结尾，将停止上传。返回 NativeMethods.CURL_READFUNC_ABORT 将离开停止上传，并返回上传错误；返回 NativeMethods.CURL_READFUNC_PAUSE 将暂停上传。</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int NativePcsReadBlockFunction(IntPtr buf, uint size, uint nmemb, IntPtr userdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NativePcsMemLeakPrintfFunction (IntPtr ptr, string filename, int line);

    #endregion
}
