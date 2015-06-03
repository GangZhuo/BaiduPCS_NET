using System;

namespace BaiduPCS_NET
{
    public enum PcsOption
    {
        PCS_OPTION_END = 0,

        /// <summary>
        /// 值为以0结尾的C格式字符串
        /// </summary>
        PCS_OPTION_USERNAME,

        /// <summary>
        /// 值为以0结尾的C格式字符串
        /// </summary>
        PCS_OPTION_PASSWORD,

        /// <summary>
        /// 值为PcsGetCaptcha类型的函数
        /// </summary>
        PCS_OPTION_CAPTCHA_FUNCTION,

        /// <summary>
        /// Pcs本身不使用该值，仅原样传递到PcsGetCaptcha函数中
        /// </summary>
        PCS_OPTION_CAPTCHA_FUNCTION_DATA,

        /// <summary>
        /// 值为PcsHttpWriteFunction类型的函数
        /// </summary>
        PCS_OPTION_DOWNLOAD_WRITE_FUNCTION,

        /// <summary>
        /// Pcs本身不使用该值，仅原样传递到PcsHttpWriteFunction函数中
        /// </summary>
        PCS_OPTION_DOWNLOAD_WRITE_FUNCTION_DATA,

        /// <summary>
        /// 值为PcsHttpResponseFunction类型的函数
        /// </summary>
        PCS_OPTION_HTTP_RESPONSE_FUNCTION,

        /// <summary>
        /// Pcs本身不使用该值，仅原样传递到PcsHttpResponseFunction函数中
        /// </summary>
        PCS_OPTION_HTTP_RESPONSE_FUNCTION_DATE,

        /// <summary>
        /// 值为PcsHttpProgressCallback类型的函数
        /// </summary>
        PCS_OPTION_PROGRESS_FUNCTION,

        /// <summary>
        /// Pcs本身不使用该值，仅原样传递到PcsHttpProgressCallback函数中
        /// </summary>
        PCS_OPTION_PROGRESS_FUNCTION_DATE,

        /// <summary>
        /// 设置是否启用下载或上传进度，值为PcsBool类型
        /// </summary>
        PCS_OPTION_PROGRESS,

        /// <summary>
        /// 设置USAGE，值为char类型指针
        /// </summary>
        PCS_OPTION_USAGE,

        /// <summary>
        /// 设置整个请求的超时时间，值为long类型
        /// </summary>
        PCS_OPTION_TIMEOUT,

        /// <summary>
        /// 设置连接前的等待时间，值为long类型
        /// </summary>
        PCS_OPTION_CONNECTTIMEOUT,


    }
}
