using System;
using System.Runtime.InteropServices;

namespace BaiduPCS_NET
{
    #region Delegates

    /// <summary>
    /// 当需要输入验证码时触发。
    /// </summary>
    /// <param name="imgBytes">验证码图片</param>
    /// <param name="captcha">用于接收验证码字符</param>
    /// <param name="userdata"></param>
    /// <returns>返回验证码是否正确输入。true - 表示正确输入；false - 表示未输入。当返回 false 时，将中断登录进程。</returns>
    public delegate bool GetCaptchaFunction(BaiduPCS sender, byte[] imgBytes, out string captcha, object userdata);

    /// <summary>
    /// 当从网络获取到数据后触发该回调。
    /// </summary>
    /// <param name="data">从网络获取到的字节序</param>
    /// <param name="contentlength">HTTP头中的 Content-Length 的值</param>
    /// <param name="userdata"></param>
    /// <returns>返回写入的数据长度，字节为单位，必须和 data.Length 相同，否则将会中断下载。</returns>
    public delegate uint WriteBlockFunction(BaiduPCS sender, byte[] data, uint contentlength, object userdata);

    /// <summary>
    /// 当从网络获取到全部数据后触发该回调。
    /// 和 OnHttpWriteFunction 的区别是，本回调是在获取到全部内容后触发,
    /// 而 OnHttpWriteFunction 是每获取到一节数据后便触发。
    /// 每个HTTP请求，OnHttpResponse 只会触发一次，而 OnHttpWriteFunction 可能触发多次。
    /// </summary>
    /// <param name="data">从网络获取到的字节序</param>
    /// <param name="userdata"></param>
    public delegate void OnHttpResponseFunction(BaiduPCS sender, byte[] data, object userdata);

    /// <summary>
    /// 当上传或下载一节数据到网络中时，触发该回调。利用该回调可实现上传时的进度条。
    /// 注意：只有设定 PCS_OPTION_PROGRESS 的值为 true 后才会启用进度条。
    /// </summary>
    /// <param name="dltotal">从网络中需要下载多少字节</param>
    /// <param name="dlnow">从网络中已经下载多少字节</param>
    /// <param name="ultotal">需要上传多少字节</param>
    /// <param name="ulnow">已经上传多少字节</param>
    /// <param name="userdata"></param>
    /// <returns>返回非零值，将导致中断上传或下载</returns>
    public delegate int OnHttpProgressFunction(BaiduPCS sender, double dltotal, double dlnow, double ultotal, double ulnow, object userdata);

    /// <summary>
    /// 当分片上传一个文件时，需要读取分片数据时，触发该函数。
    /// 用于读取分片的数据
    /// 查看： http://curl.haxx.se/libcurl/c/CURLOPT_READFUNCTION.html
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="buf">分片数据将读入到这里</param>
    /// <param name="size">每对象的大小</param>
    /// <param name="nmemb">读入多少个对象数据</param>
    /// <param name="userdata"></param>
    /// <returns>返回 0 表示已经到文件结尾，将停止上传。返回 NativeConst.CURL_READFUNC_ABORT 将离开停止上传，并返回上传错误；返回 NativeConst.CURL_READFUNC_PAUSE 将暂停上传。</returns>
    public delegate int OnReadBlockFunction(BaiduPCS sender, out byte[] buf, uint size, uint nmemb, object userdata);

    #endregion
}
