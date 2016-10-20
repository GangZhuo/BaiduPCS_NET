using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using BaiduPCS_NET.Native;

namespace BaiduPCS_NET
{
    /// <summary>
    /// 本地代码中 PCS 对象的托管映射
    /// </summary>
    public class BaiduPCS : IDisposable
    {
        public const string USAGE = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";

        #region 属性

        /// <summary>
        /// 获取在本地代码中的句柄
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// 获取使用的 COOKIE 文件
        /// </summary>
        public string CookieFileName { get; private set; }

        /// <summary>
        /// 当需要输入验证码时触发。
        /// </summary>
        public event GetCaptchaFunction GetCaptcha
        {
            add
            {
                if (_OnGetCaptcha == null)
                {
                    IntPtr v = Marshal.GetFunctionPointerForDelegate(_onGetCaptcha);
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_CAPTCHA_FUNCTION, v);
                }
                _OnGetCaptcha += value;
            }
            remove
            {
                _OnGetCaptcha -= value;
                if (_OnGetCaptcha == null)
                {
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_CAPTCHA_FUNCTION, IntPtr.Zero);
                }
            }
        }
        public event GetInputFunction GetInput
        {
            add
            {
                if (_OnGetInput == null)
                {
                    IntPtr v = Marshal.GetFunctionPointerForDelegate(_onGetInput);
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_INPUT_FUNCTION, v);
                }
                _OnGetInput += value;
            }
            remove
            {
                _OnGetInput -= value;
                if (_OnGetInput == null)
                {
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_INPUT_FUNCTION, IntPtr.Zero);
                }
            }
        }
        /// <summary>
        /// 触发 OnGetCaptcha 方法时，传入该回调的 userdata
        /// </summary>
        public object GetCaptchaUserData { get; set; }
        private GetCaptchaFunction _OnGetCaptcha;
        private NativePcsGetCaptchaFunction _onGetCaptcha;

        public object GetInputUserData { get; set; }
        private GetInputFunction _OnGetInput;
        private NativePcsInputFunction _onGetInput;

        /// <summary>
        /// 当从网络获取到数据后触发该回调。
        /// </summary>
        public event WriteBlockFunction Write
        {
            add
            {
                if (_OnHttpWrite == null)
                {
                    IntPtr v = Marshal.GetFunctionPointerForDelegate(_onHttpWrite);
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION, v);
                }
                _OnHttpWrite += value;
            }
            remove
            {
                _OnHttpWrite -= value;
                if (_OnHttpWrite == null)
                {
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION, IntPtr.Zero);
                }
            }
        }
        /// <summary>
        /// 触发 OnHttpWrite 方法时，传入该回调的 userdata
        /// </summary>
        public object WriteUserData { get; set; }
        private WriteBlockFunction _OnHttpWrite;
        private NativePcsHttpWriteFunction _onHttpWrite;

        /// <summary>
        /// 当从网络获取到全部数据后触发该回调。
        /// 和 OnHttpWriteFunction 的区别是，本回调是在获取到全部内容后触发,
        /// 而 OnHttpWriteFunction 是每获取到一节数据后便触发。
        /// 每个HTTP请求，OnHttpResponse 只会触发一次，而 OnHttpWriteFunction 可能触发多次。
        /// </summary>
        public event OnHttpResponseFunction Response
        {
            add
            {
                if (_OnHttpResponse == null)
                {
                    IntPtr v = Marshal.GetFunctionPointerForDelegate(_onHttpResponse);
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_HTTP_RESPONSE_FUNCTION, v);
                }
                _OnHttpResponse += value;
            }
            remove
            {
                _OnHttpResponse -= value;
                if(_OnHttpResponse == null)
                {
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_HTTP_RESPONSE_FUNCTION, IntPtr.Zero);
                }
            }
        }
        /// <summary>
        /// 触发 OnHttpResponse 方法时，传入该回调的 userdata
        /// </summary>
        public object ResponseUserData { get; set; }
        private OnHttpResponseFunction _OnHttpResponse;
        private NativePcsHttpResponseFunction _onHttpResponse;

        /// <summary>
        /// 当上传或下载一节数据到网络中时，触发该回调。利用该回调可实现上传时的进度条。
        /// 注意：
        ///     只有设定 PCS_OPTION_PROGRESS 的值为 true 后才会启用进度条。
        /// </summary>
        public event OnHttpProgressFunction Progress
        {
            add
            {
                if (_OnHttpProgress == null)
                {
                    IntPtr v = Marshal.GetFunctionPointerForDelegate(_onHttpProgress);
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_PROGRESS_FUNCTION, v);
                }
                _OnHttpProgress += value;
            }
            remove
            {
                _OnHttpProgress -= value;
                if(_OnHttpProgress == null)
                {
                    NativeMethods.pcs_setopt(Handle, (int)PcsOption.PCS_OPTION_PROGRESS_FUNCTION, IntPtr.Zero);
                }
            }
        }
        /// <summary>
        /// 触发 OnHttpProgress 方法时，传入该回调的 userdata
        /// </summary>
        public object ProgressUserData { get; set; }
        private OnHttpProgressFunction _OnHttpProgress;
        private NativePcsHttpProgressFunction _onHttpProgress;

        private NativePcsReadBlockFunction _onReadBlock;

        /// <summary>
        /// 设置或获取是否启用进度跟踪。
        /// 启用后才会触发 Progress 事件，否则不会触发 Progress 该事件。
        /// </summary>
        public bool ProgressEnabled
        {
            get { return _ProgressEnabled; }
            set
            {
                _ProgressEnabled = value;
                setOption(PcsOption.PCS_OPTION_PROGRESS, _ProgressEnabled);
            }
        }
        private bool _ProgressEnabled;

        /// <summary>
        /// 设置或获取 User-Agent
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set
            {
                _UserAgent = value;
                setOption(PcsOption.PCS_OPTION_USAGE, value);
            }
        }
        private string _UserAgent;

        #endregion

        private BaiduPCS(IntPtr handle, string cookieFileName)
        {
            this.Handle = handle;
            this.CookieFileName = cookieFileName;

            this._onGetCaptcha = new NativePcsGetCaptchaFunction(onGetCaptcha);
            this._onGetInput += new NativePcsInputFunction(onGetInput);
            this._onHttpWrite = new NativePcsHttpWriteFunction(onHttpWrite);
            this._onHttpResponse = new NativePcsHttpResponseFunction(onHttpResponse);
            this._onHttpProgress = new NativePcsHttpProgressFunction(onHttpProgress);
            this._onReadBlock = new NativePcsReadBlockFunction(onReadSlice);
        }

        ~BaiduPCS()
        {
            Disposing(false);
        }

        #region Internal Methods

        protected void Disposing(bool disposing)
        {
            if (disposing)
            {
                this.CookieFileName = null;
            }
            if (Handle != IntPtr.Zero)
            {
                NativeMethods.pcs_destroy(Handle);
                Handle = IntPtr.Zero;
            }
        }

        private byte onGetCaptcha(IntPtr ptr, uint size, IntPtr captcha, uint captchaSize, IntPtr state)
        {
            if (this._OnGetCaptcha != null)
            {
                byte[] imgBytes = new byte[size];
                Marshal.Copy(ptr, imgBytes, 0, (int)size);
                string captchaStr;
                if (this._OnGetCaptcha(this, imgBytes, out captchaStr, this.GetCaptchaUserData))
                {
                    NativeUtils.str_utf8_set(captcha, captchaStr, (int)captchaSize - 1, true);
                    return NativeConst.True;
                }
            }
            return NativeConst.False;
        }

        private byte onGetInput(IntPtr ptr, string tips, IntPtr captcha, uint captchaSize, IntPtr state)
        {
            if (this._OnGetInput != null)
            {
                string captchaStr;
                if (this._OnGetInput(this, tips, out captchaStr, this.GetInputUserData))
                {
                    NativeUtils.str_set(captcha, captchaStr, (int)captchaSize - 1, true);
                    return NativeConst.True;
                }
            }
            return NativeConst.False;
        }

        private uint onHttpWrite(IntPtr ptr, uint size, uint contentlength, IntPtr userdata)
        {
            if (this._OnHttpWrite != null)
            {
                byte[] data = new byte[size];
                Marshal.Copy(ptr, data, 0, (int)size);
                uint sz = this._OnHttpWrite(this, data, contentlength, this.WriteUserData);
                return sz;
            }
            return 0;
        }

        private void onHttpResponse(IntPtr ptr, uint size, IntPtr state)
        {
            if (this._OnHttpResponse != null)
            {
                byte[] data = new byte[size];
                Marshal.Copy(ptr, data, 0, (int)size);
                this._OnHttpResponse(this, data, this.ResponseUserData);
            }
        }

        private int onHttpProgress(IntPtr clientp, double dltotal, double dlnow, double ultotal, double ulnow)
        {
            if (this._OnHttpProgress != null)
            {
                return this._OnHttpProgress(this, dltotal, dlnow, ultotal, ulnow, this.ProgressUserData);
            }
            return 0;
        }

        private int onReadSlice(IntPtr buf, uint size, uint nmemb, IntPtr userdata)
        {
            string key = null;
            if (userdata != IntPtr.Zero)
                key = NativeUtils.str(userdata); //因为使用系统默认编码送入的，因此使用同样的编码读入。
            UserState state = null;
            if (!string.IsNullOrEmpty(key))
                state = getState(key);
            if (state != null)
            {
                byte[] buffer;
                int sz = state.onReadSlice(this, out buffer, size, nmemb, state.userData);
                if (sz > 0 && sz != NativeConst.CURL_READFUNC_ABORT && sz != NativeConst.CURL_READFUNC_PAUSE)
                    Marshal.Copy(buffer, 0, buf, sz);
                return sz;
            }
            return NativeConst.CURL_READFUNC_ABORT;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 释放掉资源
        /// </summary>
        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 克隆一份新的 BaiduPCS 对象，
        /// 因此新的对象使用和本对象相同的登录身份。
        /// 注意：注册的事件并不会克隆到新对象中，你需要再次使用 setOption() 函数来设置事件。
        /// </summary>
        /// <returns>返回新对象</returns>
        public BaiduPCS clone()
        {
            BaiduPCS pcs = pcs_create(CookieFileName);
            pcs.cloneUserInfo(this);
            return pcs;
        }

        /// <summary>
        /// 获取 PCS SDK 的版本
        /// </summary>
        /// <returns>返回版本</returns>
        public string version()
        {
            return pcs_version();
        }

        /// <summary>
        /// 把 from 中的用户信息复制到本对象中，因此本对象具有和 from 一样的用户身份。
        /// </summary>
        /// <param name="from"></param>
        public void cloneUserInfo(BaiduPCS from)
        {
            pcs_clone_userinfo(this, from);
        }

        /// <summary>
        /// 获取当前登录用户的 UID
        /// </summary>
        /// <returns>返回UID</returns>
        public string getUID()
        {
            return pcs_sysUID(this);
        }

        /// <summary>
        /// 获取错误消息
        /// </summary>
        /// <returns>如果有错误发生，则返回错误消息，否则返回 null</returns>
        public string getError()
        {
            return pcs_strerror(this);
        }

        /// <summary>
        /// 设定单个选项
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="value"></param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public PcsRes setOption(PcsOption opt, object value)
        {
            PcsRes r = pcs_setopt(this, opt, value);
            return r;
        }

        /// <summary>
        /// 获取是否已经登录
        /// </summary>
        /// <returns>已经登录则返回PCS_LOGIN，否则返回PCS_NOT_LOGIN</returns>
        public PcsRes isLogin()
        {
            return pcs_islogin(this);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public PcsRes login()
        {
            PcsRes r = pcs_login(this);
            return r;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public PcsRes login(string username, string password)
        {
            setOption(PcsOption.PCS_OPTION_USERNAME, username);
            setOption(PcsOption.PCS_OPTION_PASSWORD, password);
            return login();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns>成功后返回PCS_OK，失败则返回PCS_FAIL</returns>
        public PcsRes logout()
        {
            PcsRes r = pcs_logout(this);
            return r;
        }

        /// <summary>
        /// 获取网盘配额
        /// </summary>
        /// <param name="quota">用于接收总大小</param>
        /// <param name="used">用于接收已使用值</param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public PcsRes quota(out long quota, out long used)
        {
            PcsRes r = pcs_quota(this, out quota, out used);
            return r;
        }

        /// <summary>
        /// 获取最后一次请求的原始数据。
        /// </summary>
        /// <param name="encode">用于接收原始数据的编码</param>
        /// <returns>返回原始数据</returns>
        public string getRawData(out string encode)
        {
            int size;
            return pcs_req_rawdata(this, out size, out encode);
        }

        /// <summary>
        /// 获取最后一次请求的原始数据。
        /// </summary>
        /// <returns>返回原始数据</returns>
        public string getRawData()
        {
            string encode;
            return getRawData(out encode);
        }

        /// <summary>
        /// 创建一个目录
        /// </summary>
        /// <param name="path">待创建的目录，地址需写全，如/temp</param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public PcsRes mkdir(string path)
        {
            return pcs_mkdir(this, path);
        }

        /// <summary>
        /// 列出文件
        /// </summary>
        /// <param name="dir">列出该目录下的文件，地址需写全，如/temp</param>
        /// <param name="pageindex">页索引，从1开始</param>
        /// <param name="pagesize">每页大小</param>
        /// <param name="order">排序字段，可选值：name|time|size</param>
        /// <param name="desc">true - 表示倒序排列；false - 表示正序排列</param>
        /// <returns>返回文件列表</returns>
        public PcsFileInfo[] list(string dir, int pageindex, int pagesize, string order = "name", bool desc = false)
        {
            return pcs_list(this, dir, pageindex, pagesize, order, desc);
        }

        /// <summary>
        /// 在 dir 指定的文件夹中搜索关键字 key
        /// </summary>
        /// <param name="dir">待搜索的目录，地址需写全，如/temp</param>
        /// <param name="key">关键词</param>
        /// <param name="recursion">是否递归搜索其子目录</param>
        /// <returns>返回文件列表。可根据 GetError() 的返回值判断是否出错。</returns>
        public PcsFileInfo[] search(string dir, string key, bool recursion = false)
        {
            return pcs_search(this, dir, key, recursion);
        }

        /// <summary>
        /// 获取文件或目录的元信息，该方法通过pcs_search实现。
        /// </summary>
        /// <param name="path">待获取的文件或目录，地址需写全，如/temp, /temp/file.txt</param>
        /// <returns>返回文件元数据</returns>
        public PcsFileInfo meta(string path)
        {
            return pcs_meta(this, path);
        }

        /// <summary>
        /// 删除多个文件
        /// </summary>
        /// <param name="files">待删除文件列表，地址需写全，如/temp, /temp/file.txt</param>
        /// <returns>返回每一项的删除情况，成功则其 error = 0，否则为错误编码</returns>
        public PcsPanApiRes delete(string[] files)
        {
            return pcs_delete(this, files);
        }

        /// <summary>
        /// 重命名多个文件
        /// </summary>
        /// <param name="files">每项中的str1为待重命名的文件，路径需写全（如：/temp/file.txt）；str2为新的名字，仅需文件名，例如 newfile.txt，写全路径则重命名失败。如需要移动请用pcs_move()方法。</param>
        /// <returns>返回每一项的删除情况，成功则其 error = 0，否则为错误编码</returns>
        public PcsPanApiRes rename(SPair[] files)
        {
            return pcs_rename(this, files);
        }

        /// <summary>
        /// 移动多个文件
        /// </summary>
        /// <param name="files">每项中的str1为待移动的文件，路径需写全（如：/temp/file.txt）；str2为新的名字，路径需写全，例如 /temp/new-file-name.txt。</param>
        /// <returns>返回每一项的移动情况，成功则其 error = 0，否则为错误编码</returns>
        public PcsPanApiRes move(SPair[] files)
        {
            return pcs_move(this, files);
        }

        /// <summary>
        /// 复制多个文件
        /// </summary>
        /// <param name="files">每项中的str1为待复制的文件，路径需写全（如：/temp/file.txt）；str2为新的名字，路径需写全，例如 /temp/new-file-name.txt。</param>
        /// <returns>返回每一项的移动情况，成功则其 error = 0，否则为错误编码</returns>
        public PcsPanApiRes copy(SPair[] files)
        {
            return pcs_copy(this, files);
        }

        /// <summary>
        /// 不下载文件，直接获取文本文件的内容
        /// </summary>
        /// <param name="path">待获取的文件或目录，地址需写全，如/temp, /temp/file.txt</param>
        /// <returns>返回文本内容</returns>
        public string cat(string path)
        {
            return pcs_cat(this, path);
        }

        /// <summary>
        /// 下载文件。
        /// 必须指定写入下载内容的函数，可通过 PCS_OPTION_DOWNLOAD_WRITE_FUNCTION 选项来指定。
        /// 每当下载到一部分数据时，将触发 OnHttpWrite 事件，用于写文件。
        /// </summary>
        /// <param name="path">待下载的文件，地址需写全，如/temp/file.txt</param>
        /// <param name="max_speed">下载时的最大速度，0 表示不限速</param>
        /// <param name="resume_from">从文件的哪一个位置开始下载，断点续传时，已经下载的部分不需要再次下载。</param>
        /// <param name="max_length">从 resume_from 位置开始，一共下载多少字节的数据。0 表示下载到结尾。</param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public PcsRes download(string path, long max_speed, long resume_from, long max_length)
        {
            return pcs_download(this, path, max_speed, resume_from, max_length);
        }

        /// <summary>
        /// 获取文件的字节大小
        /// </summary>
        /// <param name="path">待下载的文件，地址需写全，如/temp/file.txt</param>
        /// <returns>返回文件的大小</returns>
        public long filesize(string path)
        {
            return pcs_get_download_filesize(this, path);
        }

        /// <summary>
        /// 把内存中的字节序上传到网盘
        /// </summary>
        /// <param name="topath">目标文件，地址需写全，如/temp/file.txt</param>
        /// <param name="buffer">待上传的字节序</param>
        /// <param name="overwrite">指定是否覆盖原文件，传入PcsTrue则覆盖，传入PcsFalse，则自动重命名。</param>
        /// <returns>返回上传的文件的元数据 </returns>
        public PcsFileInfo upload(string topath, byte[] buffer, bool overwrite = false)
        {
            return pcs_upload_buffer(this, topath, buffer, overwrite);
        }

        /// <summary>
        /// 上传分片数据
        /// </summary>
        /// <param name="buffer">待上传的字节序</param>
        /// <returns>返回上传的分片文件的元数据 </returns>
        public PcsFileInfo upload_slice(byte[] buffer)
        {
            return pcs_upload_slice(this, buffer);
        }

        /// <summary>
        /// 上传分片文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="read_func">读取该分片文件的方法</param>
        /// <param name="userdata"></param>
        /// <param name="content_size">总共需要上传的大小</param>
        /// <returns>返回上传的分片文件的元数据 </returns>
        public PcsFileInfo upload_slicefile(OnReadBlockFunction read_func, object userdata, uint content_size)
        {
            return pcs_upload_slicefile(this, read_func, userdata, content_size);
        }

        /// <summary>
        /// 合并分片文件
        /// </summary>
        /// <param name="topath">合并后产生的文件路径，必须写全。</param>
        /// <param name="block_list">待合并的分片列表，每一个都是上传分片文件时返回的 md5 值</param>
        /// <param name="overwrite">如果 topath 已经存在，是否覆盖</param>
        /// <returns>返回合并后的文件的元数据</returns>
        public PcsFileInfo create_superfile(string topath, string[] block_list, bool overwrite = false)
        {
            return pcs_create_superfile(this, topath, block_list, overwrite);
        }

        /// <summary>
        /// 上传文件到网盘。
        /// 可通过 PCS_OPTION_PROGRESS_FUNCTION 选项设定进度条回调，
        /// 使用 PCS_OPTION_PROGRESS 启用该回调后，可简单实现上传进度。
        /// </summary>
        /// <param name="topath">网盘文件，地址需写全，如/temp/file.txt</param>
        /// <param name="local_filename">待上传的本地文件</param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回文件的元数据</returns>
        public PcsFileInfo upload(string topath, string local_filename, bool overwrite = false)
        {
            return pcs_upload(this, topath, local_filename, overwrite);
        }

        /// <summary>
        /// 上传文件到网盘。
        /// </summary>
        /// <param name="to_path">网盘文件，地址需写全，如/temp/file.txt</param>
        /// <param name="read_func">读取该文件的方法</param>
        /// <param name="content_size">总共需要上传的大小</param>
        /// <param name="userdata"></param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回文件的元数据</returns>
        public PcsFileInfo upload_s(string to_path, OnReadBlockFunction read_func, uint content_size, object userdata, bool overwrite = false)
        {
            return pcs_upload_s(this, to_path, read_func, content_size, userdata, overwrite);
        }

        /// <summary>
        /// 获取本地文件的大小
        /// </summary>
        /// <param name="local_path">本地文件路径</param>
        /// <returns></returns>
        public long local_filesize(string local_path)
        {
            return pcs_local_filesize(this, local_path);
        }

        /// <summary>
        /// 计算本地文件的 MD5 值
        /// </summary>
        /// <param name="local_path">本地文件路径</param>
        /// <param name="md5">用于接收计算结果</param>
        /// <returns>返回是否计算成功。</returns>
        public bool md5(string local_path, out string md5)
        {
            return pcs_md5_file(this, local_path, out md5);
        }

        /// <summary>
        /// 计算文件的MD5值，仅从文件offset偏移处开始计算，并仅计算 length 长度的数据。
        /// </summary>
        /// <param name="local_path">本地文件路径</param>
        /// <param name="offset">仅从文件offset偏移处开始计算</param>
        /// <param name="length">仅计算 length 长度的数据</param>
        /// <param name="md5">用于接收计算结果</param>
        /// <returns>返回是否计算成功。</returns>
        public bool md5(string local_path, long offset, long length, out string md5)
        {
            return pcs_md5_file_slice(this, local_path, offset, length, out md5);
        }

        /// <summary>
        /// 计算 MD5 值
        /// </summary>
        /// <param name="read_func">读取块类容的函数</param>
        /// <param name="userdata"></param>
        /// <param name="md5">用于接收计算结果</param>
        /// <returns>返回是否计算成功。</returns>
        public bool md5(OnReadBlockFunction read_func, object userdata, out string md5)
        {
            return pcs_md5_s(this, read_func, userdata, out md5);
        }

        /// <summary>
        /// 快速上传
        /// </summary>
        /// <param name="topath">网盘文件</param>
        /// <param name="local_filename">本地文件</param>
        /// <param name="file_md5">文件的md5值</param>
        /// <param name="slice_md5">验证文件的分片的md5值</param>
        /// <param name="overwrite">如果 topath 已经存在，是否覆盖。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回上传的文件的元数据</returns>
        public PcsFileInfo rapid_upload(string topath, string local_filename, ref string file_md5, ref string slice_md5, bool overwrite = false)
        {
            return pcs_rapid_upload(this, topath, local_filename, ref file_md5, ref slice_md5, overwrite);
        }

        /// <summary>
        /// 快速上传
        /// </summary>
        /// <param name="topath">网盘文件</param>
        /// <param name="filesize">本地文件的字节大小</param>
        /// <param name="file_md5">文件的md5值</param>
        /// <param name="slice_md5">验证文件的分片的md5值</param>
        /// <param name="overwrite">如果 topath 已经存在，是否覆盖。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回上传的文件的元数据</returns>
        public PcsFileInfo rapid_upload_r(string topath, long filesize, string file_md5, string slice_md5, bool overwrite = false)
        {
            return pcs_rapid_upload_r(this, topath, filesize, file_md5, slice_md5, overwrite);
        }

        /// <summary>
        /// 获取Cookie 数据。
        /// </summary>
        /// <returns>返回 Cookie 数据</returns>
        public string cookie_data()
        {
            return pcs_cookie_data(this);
        }

        /// <summary>
        /// 获取下载速度
        /// </summary>
        /// <returns>返回下载速度</returns>
        public double speed_download()
        {
            return pcs_speed_download(this);
        }


        /// <summary>
        /// 清除错误消息
        /// </summary>
        public void clear_errmsg()
        {
            pcs_clear_errmsg(this);
        }

        /// <summary>
        /// 设置错误消息
        /// </summary>
        /// <param name="errmsg">错误消息</param>
        public void set_serrmsg(string errmsg)
        {
            pcs_set_serrmsg(this, errmsg);
        }

        /// <summary>
        /// 添加错误消息到结尾
        /// </summary>
        /// <param name="errmsg">错误消息</param>
        public void cat_serrmsg(string errmsg)
        {
            pcs_cat_serrmsg(this, errmsg);
        }

        #endregion

        #region Static Methods

        public static string build_path(string parent_path, params string[] args)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            if (parent_path != "/")
            {
                if (parent_path.EndsWith("/"))
                    sb.Append(parent_path.Substring(0, parent_path.Length - 1));
                else
                    sb.Append(parent_path);
            }
            foreach (string s in args)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                sb.Append("/");
                sb.Append(s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取 PCS SDK 的版本
        /// </summary>
        /// <returns>返回版本</returns>
        public static string pcs_version()
        {
            IntPtr r = NativeMethods.pcs_version();
            return NativeUtils.utf8_str(r);
        }

        /// <summary>
        /// 获取引用的 BaiduPCS.dll 是否是 DEBUG 版本
        /// </summary>
        /// <returns>返回是否 DEBUG 版本。true - 是 DEBUG 版本; false - 不是 DEBUG 版本。</returns>
        public static bool pcs_isdebug()
        {
            string ver = pcs_version();
            return ver.IndexOf("debug") != -1;
        }

        /// <summary>
        /// 创建一个新对象。
        /// </summary>
        /// <param name="cookieFileName"></param>
        /// <returns>成功返回新对象，失败返回 null</returns>
        public static BaiduPCS pcs_create(string cookieFileName)
        {
            IntPtr cookieFileNamePtr = NativeUtils.str_ptr(cookieFileName);
            IntPtr pcsPtr = NativeMethods.pcs_create(cookieFileNamePtr);
            NativeUtils.free_str_ptr(cookieFileNamePtr);
            if (pcsPtr == IntPtr.Zero)
                return null;
            BaiduPCS pcs = new BaiduPCS(pcsPtr, cookieFileName);
            pcs_setopt(pcs, PcsOption.PCS_OPTION_USAGE, USAGE);
            pcs_setopt(pcs, PcsOption.PCS_OPTION_CONNECTTIMEOUT, 10);
            //pcs_setopt(pcs, PcsOption.PCS_OPTION_TIMEOUT, 60);
            //pcs_setopt(pcs, PcsOption.PCS_OPTION_CAPTCHA_FUNCTION, pcs._onGetCaptcha);
            //pcs_setopt(pcs, PcsOption.PCS_OPTION_PROGRESS_FUNCTION, pcs._onHttpProgress);
            //pcs_setopt(pcs, PcsOption.PCS_OPTION_PROGRESS, false);
            return pcs;
        }

        /// <summary>
        /// 复制 src 的用户信息到 dst 中，因此 dst 将使用和 src 一样的用户身份执行操作。
        /// </summary>
        /// <param name="dst">复制到该对象中</param>
        /// <param name="src">复制源</param>
        public static void pcs_clone_userinfo(BaiduPCS dst, BaiduPCS src)
        {
            NativeMethods.pcs_clone_userinfo(dst.Handle, src.Handle);
        }

        /// <summary>
        /// 获取当前登录用户的 UID
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>返回 UID</returns>
        public static string pcs_sysUID(BaiduPCS pcs)
        {
            IntPtr r = NativeMethods.pcs_sysUID(pcs.Handle);
            if (r != IntPtr.Zero)
                return NativeUtils.utf8_str(r);
            return null;
        }

        /// <summary>
        /// 获取错误消息
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>如果有错误发生，则返回错误消息，否则返回 null</returns>
        public static string pcs_strerror(BaiduPCS pcs)
        {
            IntPtr r = NativeMethods.pcs_strerror(pcs.Handle);
            if (r != IntPtr.Zero)
                return NativeUtils.utf8_str(r);
            return null;
        }

        /// <summary>
        /// 配置 pcs 对象
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="opt"></param>
        /// <param name="value"></param>
        /// <returns>成功后返回 PCS_OK，失败则返回错误编号</returns>
        public static PcsRes pcs_setopt(BaiduPCS pcs, PcsOption opt, object value)
        {
            PcsRes r = PcsRes.PCS_OK;
            switch (opt)
            {
                /* 值为以0结尾的C格式字符串 */
                case PcsOption.PCS_OPTION_USERNAME:
                    {
                        IntPtr v = NativeUtils.utf8_str_ptr((string)value);
                        r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        NativeUtils.free_str_ptr(v);
                    }
                    break;

                /* 值为以0结尾的C格式字符串 */
                case PcsOption.PCS_OPTION_PASSWORD:
                    {
                        IntPtr v = NativeUtils.utf8_str_ptr((string)value);
                        r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        NativeUtils.free_str_ptr(v);
                    }
                    break;

                /* 值为PcsGetCaptcha类型的函数 */
                case PcsOption.PCS_OPTION_CAPTCHA_FUNCTION:
                    {
                        if (value != null)
                        {
                            IntPtr v = Marshal.GetFunctionPointerForDelegate(pcs._onGetCaptcha);
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        }
                        else
                        {
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                        }
                        pcs._OnGetCaptcha = (GetCaptchaFunction)value;
                    }
                    break;

                /* Pcs本身不使用该值，仅原样传递到PcsGetCaptcha函数中 */
                case PcsOption.PCS_OPTION_CAPTCHA_FUNCTION_DATA:
                    pcs.GetCaptchaUserData = value;
                    NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                    break;

                /* 值为PcsHttpWriteFunction类型的函数 */
                case PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION:
                    {
                        if (value != null)
                        {
                            IntPtr v = Marshal.GetFunctionPointerForDelegate(pcs._onHttpWrite);
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        }
                        else
                        {
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                        }
                        pcs._OnHttpWrite = (WriteBlockFunction)value;
                    }
                    break;

                /* Pcs本身不使用该值，仅原样传递到PcsHttpWriteFunction函数中 */
                case PcsOption.PCS_OPTION_DOWNLOAD_WRITE_FUNCTION_DATA:
                    pcs.WriteUserData = value;
                    NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                    break;

                /* 值为PcsHttpResponseFunction类型的函数 */
                case PcsOption.PCS_OPTION_HTTP_RESPONSE_FUNCTION:
                    {
                        if (value != null)
                        {
                            IntPtr v = Marshal.GetFunctionPointerForDelegate(pcs._onHttpResponse);
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        }
                        else
                        {
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                        }
                        pcs._OnHttpResponse = (OnHttpResponseFunction)value;
                    }
                    break;

                /* Pcs本身不使用该值，仅原样传递到PcsHttpResponseFunction函数中 */
                case PcsOption.PCS_OPTION_HTTP_RESPONSE_FUNCTION_DATE:
                    pcs.ResponseUserData = value;
                    NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                    break;

                /* 值为PcsHttpProgressCallback类型的函数 */
                case PcsOption.PCS_OPTION_PROGRESS_FUNCTION:
                    {
                        if (value != null)
                        {
                            IntPtr v = Marshal.GetFunctionPointerForDelegate(pcs._onHttpProgress);
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        }
                        else
                        {
                            r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                        }
                        pcs._OnHttpProgress = (OnHttpProgressFunction)value;
                    }
                    break;

                /* Pcs本身不使用该值，仅原样传递到PcsHttpProgressCallback函数中 */
                case PcsOption.PCS_OPTION_PROGRESS_FUNCTION_DATE:
                    pcs.ProgressUserData = value;
                    NativeMethods.pcs_setopt(pcs.Handle, (int)opt, IntPtr.Zero);
                    break;

                /* 设置是否启用下载或上传进度，值为PcsBool类型 */
                case PcsOption.PCS_OPTION_PROGRESS:
                    {
                        pcs._ProgressEnabled = (bool)value;
                        IntPtr v = ((bool)value) ? NativeUtils.IntPtrAdd(IntPtr.Zero, 1) : IntPtr.Zero;
                        r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                    }
                    break;

                /* 设置USAGE，值为char类型指针 */
                case PcsOption.PCS_OPTION_USAGE:
                    {
                        IntPtr v = NativeUtils.str_ptr((string)value);
                        r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                        NativeUtils.free_str_ptr(v);
                        pcs._UserAgent = (string)value;
                    }
                    break;

                /*设置整个请求的超时时间，值为int类型*/
                case PcsOption.PCS_OPTION_TIMEOUT:
                    {
                        IntPtr v = NativeUtils.IntPtrAdd(IntPtr.Zero, (int)value);
                        r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                    }
                    break;

                /*设置连接前的等待时间，值为int类型*/
                case PcsOption.PCS_OPTION_CONNECTTIMEOUT:
                    {
                        IntPtr v = NativeUtils.IntPtrAdd(IntPtr.Zero, (int)value);
                        r = (PcsRes)NativeMethods.pcs_setopt(pcs.Handle, (int)opt, v);
                    }
                    break;
                default:
                    r = PcsRes.PCS_FAIL;
                    break;
            }
            return r;
        }

        /// <summary>
        /// 获取是否已经登录
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>已经登录则返回PCS_LOGIN，否则返回PCS_NOT_LOGIN</returns>
        public static PcsRes pcs_islogin(BaiduPCS pcs)
        {
            PcsRes r = (PcsRes)NativeMethods.pcs_islogin(pcs.Handle);
            return r;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>成功后返回PCS_LOGIN，失败则返回错误编号</returns>
        public static PcsRes pcs_login(BaiduPCS pcs)
        {
            PcsRes r = (PcsRes)NativeMethods.pcs_login(pcs.Handle);
            return r;
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>成功后返回PCS_OK，失败则返回PCS_FAIL</returns>
        public static PcsRes pcs_logout(BaiduPCS pcs)
        {
            PcsRes r = (PcsRes)NativeMethods.pcs_logout(pcs.Handle);
            return r;
        }

        /// <summary>
        /// 获取网盘配额
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="quota">用于接收总大小</param>
        /// <param name="used">用于接收已使用值</param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public static PcsRes pcs_quota(BaiduPCS pcs, out long quota, out long used)
        {
            IntPtr quotaPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
            IntPtr usedPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, quotaPtr, Marshal.SizeOf(typeof(long)));
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, usedPtr, Marshal.SizeOf(typeof(long)));
            PcsRes r = (PcsRes)NativeMethods.pcs_quota(pcs.Handle, quotaPtr, usedPtr);
            if (r == PcsRes.PCS_OK)
            {
                quota = Marshal.ReadInt64(quotaPtr);
                used = Marshal.ReadInt64(usedPtr);
            }
            else
            {
                quota = 0;
                used = 0;
            }
            return r;
        }

        /// <summary>
        /// 创建一个目录
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="path">待创建的目录，地址需写全，如/temp</param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public static PcsRes pcs_mkdir(BaiduPCS pcs, string path)
        {
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(path);
            PcsRes r = (PcsRes)NativeMethods.pcs_mkdir(pcs.Handle, pathPtr);
            NativeUtils.free_str_ptr(pathPtr);
            return r;
        }

        /// <summary>
        /// 列出文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="dir">列出该目录下的文件，地址需写全，如/temp</param>
        /// <param name="pageindex">页索引，从1开始</param>
        /// <param name="pagesize">每页大小</param>
        /// <param name="order">排序字段，可选值：name|time|size</param>
        /// <param name="desc">true - 表示倒序排列；false - 表示正序排列</param>
        /// <returns>返回文件列表</returns>
        public static PcsFileInfo[] pcs_list(BaiduPCS pcs, string dir, int pageindex, int pagesize, string order = "name", bool desc = false)
        {
            IntPtr dirPtr = NativeUtils.utf8_str_ptr(dir);
            IntPtr orderPtr = NativeUtils.utf8_str_ptr(order);
            IntPtr listPtr = NativeMethods.pcs_list(pcs.Handle, dirPtr, pageindex, pagesize, orderPtr, desc ? NativeConst.True : NativeConst.False);
            NativeUtils.free_str_ptr(dirPtr);
            NativeUtils.free_str_ptr(orderPtr);
            PcsFileInfo[] r = NativeUtils.GetFileListFromPcsFileInfoListPtr(listPtr);
            if (listPtr != IntPtr.Zero)
                NativeMethods.pcs_filist_destroy(listPtr);
            return r;
        }

        /// <summary>
        /// 在 dir 指定的文件夹中搜索关键字 key
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="dir">待搜索的目录，地址需写全，如/temp</param>
        /// <param name="key">关键词</param>
        /// <param name="recursion">是否递归搜索其子目录</param>
        /// <returns>返回文件列表。可根据 pcs_strerror() 的返回值判断是否出错。</returns>
        public static PcsFileInfo[] pcs_search(BaiduPCS pcs, string dir, string key, bool recursion = false)
        {
            IntPtr dirPtr = NativeUtils.utf8_str_ptr(dir);
            IntPtr keyPtr = NativeUtils.utf8_str_ptr(key);
            IntPtr listPtr = NativeMethods.pcs_search(pcs.Handle, dirPtr, keyPtr, recursion ? NativeConst.True : NativeConst.False);
            NativeUtils.free_str_ptr(dirPtr);
            NativeUtils.free_str_ptr(keyPtr);
            PcsFileInfo[] r = NativeUtils.GetFileListFromPcsFileInfoListPtr(listPtr);
            if (listPtr != IntPtr.Zero)
                NativeMethods.pcs_filist_destroy(listPtr);
            return r;
        }

        /// <summary>
        /// 获取文件或目录的元信息，该方法通过pcs_search实现。
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="path">待获取的文件或目录，地址需写全，如/temp, /temp/file.txt</param>
        /// <returns>返回文件元数据</returns>
        public static PcsFileInfo pcs_meta(BaiduPCS pcs, string path)
        {
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(path);
            IntPtr fiptr = NativeMethods.pcs_meta(pcs.Handle, pathPtr);
            NativeUtils.free_str_ptr(pathPtr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 删除多个文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="files">待删除文件列表，地址需写全，如/temp, /temp/file.txt</param>
        /// <returns>返回每一项的删除情况，成功则其 error = 0，否则为错误编码</returns>
        public static PcsPanApiRes pcs_delete(BaiduPCS pcs, string[] files)
        {
            if (files == null || files.Length == 0)
                return new PcsPanApiRes() { error = -1 };
            IntPtr listPtr = NativeUtils.slist_ptr(files);
            if (listPtr == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            IntPtr r = NativeMethods.pcs_delete(pcs.Handle, listPtr);
            NativeUtils.free_slist_ptr(listPtr);
            if (r == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            PcsPanApiRes res = NativeUtils.GetPcsPanApiResFromPtr(r);
            NativeMethods.pcs_pan_api_res_destroy(r);
            return res;
        }

        /// <summary>
        /// 重命名多个文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="files">每项中的str1为待重命名的文件，路径需写全（如：/temp/file.txt）；str2为新的名字，仅需文件名，例如 newfile.txt，写全路径则重命名失败。如需要移动请用pcs_move()方法。</param>
        /// <returns>返回每一项的删除情况，成功则其 error = 0，否则为错误编码</returns>
        public static PcsPanApiRes pcs_rename(BaiduPCS pcs, SPair[] files)
        {
            if (files == null || files.Length == 0)
                return new PcsPanApiRes() { error = -1 };
            IntPtr listPtr = NativeUtils.slist2_ptr(files);
            if (listPtr == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            IntPtr r = NativeMethods.pcs_rename(pcs.Handle, listPtr);
            NativeUtils.free_slist2_ptr(listPtr);
            if (r == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            PcsPanApiRes res = NativeUtils.GetPcsPanApiResFromPtr(r);
            NativeMethods.pcs_pan_api_res_destroy(r);
            return res;
        }

        /// <summary>
        /// 移动多个文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="files">每项中的str1为待移动的文件，路径需写全（如：/temp/file.txt）；str2为新的名字，路径需写全，例如 /temp/new-file-name.txt。</param>
        /// <returns>返回每一项的移动情况，成功则其 error = 0，否则为错误编码</returns>
        public static PcsPanApiRes pcs_move(BaiduPCS pcs, SPair[] files)
        {
            if (files == null || files.Length == 0)
                return new PcsPanApiRes() { error = -1 };
            IntPtr listPtr = NativeUtils.slist2_ptr(files);
            if (listPtr == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            IntPtr r = NativeMethods.pcs_move(pcs.Handle, listPtr);
            NativeUtils.free_slist2_ptr(listPtr);
            if (r == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            PcsPanApiRes res = NativeUtils.GetPcsPanApiResFromPtr(r);
            NativeMethods.pcs_pan_api_res_destroy(r);
            return res;
        }

        /// <summary>
        /// 复制多个文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="files">每项中的str1为待复制的文件，路径需写全（如：/temp/file.txt）；str2为新的名字，路径需写全，例如 /temp/new-file-name.txt。</param>
        /// <returns>返回每一项的移动情况，成功则其 error = 0，否则为错误编码</returns>
        public PcsPanApiRes pcs_copy(BaiduPCS pcs, SPair[] files)
        {
            if (files == null || files.Length == 0)
                return new PcsPanApiRes() { error = -1 };
            IntPtr listPtr = NativeUtils.slist2_ptr(files);
            if (listPtr == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            IntPtr r = NativeMethods.pcs_copy(pcs.Handle, listPtr);
            NativeUtils.free_slist2_ptr(listPtr);
            if (r == IntPtr.Zero)
                return new PcsPanApiRes() { error = -1 };
            PcsPanApiRes res = NativeUtils.GetPcsPanApiResFromPtr(r);
            NativeMethods.pcs_pan_api_res_destroy(r);
            return res;
        }

        /// <summary>
        /// 不下载文件，直接获取文本文件的内容
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="path">待获取的文件或目录，地址需写全，如/temp, /temp/file.txt</param>
        /// <returns>返回文本内容</returns>
        public static string pcs_cat(BaiduPCS pcs, string path)
        {
            uint sz = 0;
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(path);
            IntPtr sp = NativeMethods.pcs_cat(pcs.Handle, pathPtr, ref sz);
            NativeUtils.free_str_ptr(pathPtr);
            if (sp == IntPtr.Zero)
                return null;
            return NativeUtils.str(sp);
        }

        /// <summary>
        /// 下载文件。
        /// 必须指定写入下载内容的函数，可通过 PCS_OPTION_DOWNLOAD_WRITE_FUNCTION 选项来指定。
        /// 每当下载到一部分数据时，将触发 PcsHttpWriteFunction 事件，用于写文件。
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="path">待下载的文件，地址需写全，如/temp/file.txt</param>
        /// <param name="max_speed">下载时的最大速度，0 表示不限速</param>
        /// <param name="resume_from">从文件的哪一个位置开始下载，断点续传时，已经下载的部分不需要再次下载。</param>
        /// <param name="max_length">从 resume_from 位置开始，一共下载多少字节的数据。0 表示下载到结尾。</param>
        /// <returns>成功后返回PCS_OK，失败则返回错误编号</returns>
        public static PcsRes pcs_download(BaiduPCS pcs, string path, long max_speed, long resume_from, long max_length)
        {
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(path);
            PcsRes r = (PcsRes)NativeMethods.pcs_download(pcs.Handle, pathPtr, max_speed, resume_from, max_length);
            NativeUtils.free_str_ptr(pathPtr);
            return r;
        }

        /// <summary>
        /// 获取待下载文件的字节大小
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="path">待下载的文件，地址需写全，如/temp/file.txt</param>
        /// <returns>返回文件的大小</returns>
        public static long pcs_get_download_filesize(BaiduPCS pcs, string path)
        {
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(path);
            long r = NativeMethods.pcs_get_download_filesize(pcs.Handle, pathPtr);
            NativeUtils.free_str_ptr(pathPtr);
            return r;
        }

        /// <summary>
        /// 把内存中的字节序上传到网盘
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="path">目标文件，地址需写全，如/temp/file.txt</param>
        /// <param name="buffer">待上传的字节序</param>
        /// <param name="overwrite">指定是否覆盖原文件，传入PcsTrue则覆盖，传入PcsFalse，则自动重命名。</param>
        /// <param name="maxspeed">最大上传速度，0 为不限制上传速度。</param>
        /// <returns>返回上传的文件的元数据 </returns>
        public static PcsFileInfo pcs_upload_buffer(BaiduPCS pcs, string path, byte[] buffer, bool overwrite = false, long maxspeed = 0)
        {
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(path);
            IntPtr bufptr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, bufptr, buffer.Length);
            IntPtr fiptr = NativeMethods.pcs_upload_buffer(pcs.Handle, pathPtr, overwrite ? NativeConst.True : NativeConst.False, bufptr, (uint)buffer.Length, maxspeed);
            NativeUtils.free_str_ptr(pathPtr);
            Marshal.FreeHGlobal(bufptr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 上传分片数据
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="buffer">待上传的字节序</param>
        /// <param name="maxspeed">最大上传速度，0 为不限制上传速度。</param>
        /// <returns>返回上传的分片文件的元数据 </returns>
        public static PcsFileInfo pcs_upload_slice(BaiduPCS pcs, byte[] buffer, long maxspeed = 0)
        {
            IntPtr bufptr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, bufptr, buffer.Length);
            IntPtr fiptr = NativeMethods.pcs_upload_slice(pcs.Handle, bufptr, (uint)buffer.Length, maxspeed);
            Marshal.FreeHGlobal(bufptr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 上传分片文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="read_func">读取该分片文件的方法</param>
        /// <param name="userdata"></param>
        /// <param name="content_size">总共需要上传的大小</param>
        /// <param name="maxspeed">最大上传速度，0 为不限制上传速度。</param>
        /// <returns>返回上传的分片文件的元数据 </returns>
        public static PcsFileInfo pcs_upload_slicefile(BaiduPCS pcs, OnReadBlockFunction read_func, object userdata, uint content_size, long maxspeed = 0)
        {
            UserState state = new UserState()
            {
                onReadSlice = read_func,
                userData = userdata
            };
            string key = saveState(state);
            IntPtr keyPtr = NativeUtils.str_ptr(key);
            IntPtr fiptr = NativeMethods.pcs_upload_slicefile(pcs.Handle, pcs._onReadBlock, keyPtr, content_size, maxspeed);
            NativeUtils.free_str_ptr(keyPtr);
            removeState(key);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 合并分片文件
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="topath">合并后产生的文件路径，必须写全。</param>
        /// <param name="block_list">待合并的分片列表，每一个都是上传分片文件时返回的 md5 值</param>
        /// <param name="overwrite">如果 topath 已经存在，是否覆盖</param>
        /// <returns>返回合并后的文件的元数据</returns>
        public static PcsFileInfo pcs_create_superfile(BaiduPCS pcs, string topath, string[] block_list, bool overwrite = false)
        {
            IntPtr pathPtr = NativeUtils.utf8_str_ptr(topath);
            IntPtr slPtr = NativeUtils.slist_ptr(block_list);
            IntPtr fiptr = NativeMethods.pcs_create_superfile(pcs.Handle, pathPtr, overwrite ? NativeConst.True : NativeConst.False, slPtr);
            NativeUtils.free_str_ptr(pathPtr);
            NativeUtils.free_slist_ptr(slPtr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 上传文件到网盘。
        /// 可通过 PCS_OPTION_PROGRESS_FUNCTION 选项设定进度条回调，
        /// 使用 PCS_OPTION_PROGRESS 启用该回调后，可简单实现上传进度。
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="topath">网盘文件，地址需写全，如/temp/file.txt</param>
        /// <param name="local_filename">待上传的本地文件</param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <param name="maxspeed">最大上传速度，0 为不限制上传速度。</param>
        /// <returns>返回文件的元数据</returns>
        public static PcsFileInfo pcs_upload(BaiduPCS pcs, string topath, string local_filename, bool overwrite = false, long maxspeed = 0)
        {
            IntPtr remotePtr = NativeUtils.utf8_str_ptr(topath);
            IntPtr localPtr = NativeUtils.str_ptr(local_filename);
            IntPtr fiptr = NativeMethods.pcs_upload(pcs.Handle, remotePtr, overwrite ? NativeConst.True : NativeConst.False, localPtr, maxspeed);
            NativeUtils.free_str_ptr(remotePtr);
            NativeUtils.free_str_ptr(localPtr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 上传文件到网盘。
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="to_path">网盘文件，地址需写全，如/temp/file.txt</param>
        /// <param name="read_func">读取该文件的方法</param>
        /// <param name="content_size">总共需要上传的大小</param>
        /// <param name="userdata"></param>
        /// <param name="overwrite">如果网盘文件已经存在，是否覆盖原文件。true - 覆盖；false - 自动重命名</param>
        /// <param name="maxspeed">最大上传速度，0 为不限制上传速度。</param>
        /// <returns>返回文件的元数据</returns>
        public static PcsFileInfo pcs_upload_s(BaiduPCS pcs, string to_path, OnReadBlockFunction read_func, uint content_size, object userdata, bool overwrite = false, long maxspeed = 0)
        {
            UserState state = new UserState()
            {
                onReadSlice = read_func,
                userData = userdata
            };
            IntPtr remotePtr = NativeUtils.utf8_str_ptr(to_path);
            string key = saveState(state);
            IntPtr keyPtr = NativeUtils.str_ptr(key);
            IntPtr fiptr = NativeMethods.pcs_upload_s(pcs.Handle, remotePtr, overwrite ? NativeConst.True : NativeConst.False,
                pcs._onReadBlock, keyPtr, content_size, maxspeed);
            NativeUtils.free_str_ptr(remotePtr);
            NativeUtils.free_str_ptr(keyPtr);
            removeState(key);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 获取本地文件的大小
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="local_path">本地文件路径</param>
        /// <returns></returns>
        public static long pcs_local_filesize(BaiduPCS pcs, string local_path)
        {
            IntPtr localPtr = NativeUtils.str_ptr(local_path);
            long sz = NativeMethods.pcs_local_filesize(pcs.Handle, localPtr);
            NativeUtils.free_str_ptr(localPtr);
            return sz;
        }

        /// <summary>
        /// 计算本地文件的 MD5 值
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="local_path">本地文件路径</param>
        /// <param name="md5">用于接收计算结果</param>
        /// <returns>返回是否计算成功。</returns>
        public static bool pcs_md5_file(BaiduPCS pcs, string local_path, out string md5)
        {
            IntPtr localPtr = NativeUtils.str_ptr(local_path);
            IntPtr md5Ptr = Marshal.AllocHGlobal(36);
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, md5Ptr, 36); /* need fix? */
            bool r = NativeMethods.pcs_md5_file(pcs.Handle, localPtr, md5Ptr) != NativeConst.False;
            NativeUtils.free_str_ptr(localPtr);
            if (r)
                md5 = NativeUtils.utf8_str(md5Ptr);
            else
                md5 = string.Empty;
            Marshal.FreeHGlobal(md5Ptr);
            return r;
        }

        /// <summary>
        /// 计算 MD5 值
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="read_func">读取块类容的函数</param>
        /// <param name="userdata"></param>
        /// <param name="md5">用于接收计算结果</param>
        /// <returns>返回是否计算成功。</returns>
        public static bool pcs_md5_s(BaiduPCS pcs, OnReadBlockFunction read_func, object userdata, out string md5)
        {
            UserState state = new UserState()
            {
                onReadSlice = read_func,
                userData = userdata
            };
            string key = saveState(state);
            IntPtr keyPtr = NativeUtils.str_ptr(key);
            IntPtr md5Ptr = Marshal.AllocHGlobal(36);
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, md5Ptr, 36); /* need fix? */
            bool r = NativeMethods.pcs_md5_s(pcs.Handle, pcs._onReadBlock, keyPtr, md5Ptr) != NativeConst.False;
            NativeUtils.free_str_ptr(keyPtr);
            removeState(key);
            if (r)
                md5 = NativeUtils.utf8_str(md5Ptr);
            else
                md5 = string.Empty;
            Marshal.FreeHGlobal(md5Ptr);
            return r;
        }

        /// <summary>
        /// 计算文件的MD5值，仅从文件offset偏移处开始计算，并仅计算 length 长度的数据。
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="local_path">本地文件路径</param>
        /// <param name="offset">仅从文件offset偏移处开始计算</param>
        /// <param name="length">仅计算 length 长度的数据</param>
        /// <param name="md5">用于接收计算结果</param>
        /// <returns>返回是否计算成功。</returns>
        public static bool pcs_md5_file_slice(BaiduPCS pcs, string local_path, long offset, long length, out string md5)
        {
            IntPtr localPtr = NativeUtils.str_ptr(local_path);
            IntPtr md5Ptr = Marshal.AllocHGlobal(36);
            Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, md5Ptr, 36); /* need fix? */
            bool r = NativeMethods.pcs_md5_file_slice(pcs.Handle, localPtr, offset, length, md5Ptr) != NativeConst.False;
            NativeUtils.free_str_ptr(localPtr);
            if (r)
                md5 = NativeUtils.utf8_str(md5Ptr);
            else
                md5 = string.Empty;
            Marshal.FreeHGlobal(md5Ptr);
            return r;
        }

        /// <summary>
        /// 快速上传
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="topath">网盘文件</param>
        /// <param name="local_filename">本地文件</param>
        /// <param name="file_md5">文件的md5值</param>
        /// <param name="slice_md5">验证文件的分片的md5值</param>
        /// <param name="overwrite">如果 topath 已经存在，是否覆盖。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回上传的文件的元数据</returns>
        public static PcsFileInfo pcs_rapid_upload(BaiduPCS pcs, string topath, string local_filename, ref string file_md5, ref string slice_md5, bool overwrite = false)
        {
            IntPtr remotePtr = NativeUtils.utf8_str_ptr(topath);
            IntPtr localPtr = NativeUtils.str_ptr(local_filename);
            IntPtr fileMd5Ptr = Marshal.AllocHGlobal(36);
            IntPtr sliceMd5Ptr = Marshal.AllocHGlobal(36);
            if (!string.IsNullOrEmpty(file_md5) && file_md5.Length == 32)
                NativeUtils.str_set(fileMd5Ptr, file_md5);
            else
                Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, fileMd5Ptr, 36); /* need fix? */
            if (!string.IsNullOrEmpty(slice_md5) && slice_md5.Length == 32)
                NativeUtils.str_set(sliceMd5Ptr, slice_md5);
            else
                Marshal.Copy(NativeConst.ZERO_MATRIX_8X8, 0, sliceMd5Ptr, 36); /* need fix? */
            IntPtr fiptr = NativeMethods.pcs_rapid_upload(pcs.Handle, remotePtr, overwrite ? NativeConst.True : NativeConst.False, localPtr, fileMd5Ptr, sliceMd5Ptr);
            file_md5 = NativeUtils.utf8_str(fileMd5Ptr);
            slice_md5 = NativeUtils.utf8_str(sliceMd5Ptr);
            NativeUtils.free_str_ptr(remotePtr);
            NativeUtils.free_str_ptr(localPtr);
            Marshal.FreeHGlobal(fileMd5Ptr);
            Marshal.FreeHGlobal(sliceMd5Ptr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 快速上传
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="topath">网盘文件</param>
        /// <param name="filesize">本地文件的字节大小</param>
        /// <param name="file_md5">文件的md5值</param>
        /// <param name="slice_md5">验证文件的分片的md5值</param>
        /// <param name="overwrite">如果 topath 已经存在，是否覆盖。true - 覆盖；false - 自动重命名</param>
        /// <returns>返回上传的文件的元数据</returns>
        public static PcsFileInfo pcs_rapid_upload_r(BaiduPCS pcs, string topath, long filesize, string file_md5, string slice_md5, bool overwrite = false)
        {
            IntPtr remotePtr = NativeUtils.utf8_str_ptr(topath);
            IntPtr fileMd5Ptr = Marshal.AllocHGlobal(36);
            IntPtr sliceMd5Ptr = Marshal.AllocHGlobal(36);
            NativeUtils.str_set(fileMd5Ptr, file_md5);
            NativeUtils.str_set(sliceMd5Ptr, slice_md5);
            IntPtr fiptr = NativeMethods.pcs_rapid_upload_r(pcs.Handle, remotePtr, overwrite ? NativeConst.True : NativeConst.False,
                filesize, fileMd5Ptr, sliceMd5Ptr);
            file_md5 = NativeUtils.utf8_str(fileMd5Ptr);
            slice_md5 = NativeUtils.utf8_str(sliceMd5Ptr);
            NativeUtils.free_str_ptr(remotePtr);
            Marshal.FreeHGlobal(fileMd5Ptr);
            Marshal.FreeHGlobal(sliceMd5Ptr);
            if (fiptr == IntPtr.Zero)
                return new PcsFileInfo();
            NativePcsFileInfo nfi = (NativePcsFileInfo)Marshal.PtrToStructure(fiptr, typeof(NativePcsFileInfo));
            PcsFileInfo fi = new PcsFileInfo(nfi);
            NativeMethods.pcs_fileinfo_destroy(fiptr);
            return fi;
        }

        /// <summary>
        /// 获取Cookie 数据。
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>返回 Cookie 数据</returns>
        public static string pcs_cookie_data(BaiduPCS pcs)
        {
            IntPtr sPtr = NativeMethods.pcs_cookie_data(pcs.Handle);
            string cookie = null;
            if (sPtr != IntPtr.Zero)
            {
                cookie = NativeUtils.str(sPtr);
                NativeUtils.pcs_free(sPtr);
            }
            return cookie;
        }

        /// <summary>
        /// 获取下载速度
        /// </summary>
        /// <param name="pcs"></param>
        /// <returns>返回下载速度</returns>
        public static double pcs_speed_download(BaiduPCS pcs)
        {
            return NativeMethods.pcs_speed_download(pcs.Handle);
        }

        /// <summary>
        /// 获取最后一次请求的原始数据。
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="size">用于接收原始数据的长度</param>
        /// <param name="encode">用于接收原始数据的编码</param>
        /// <returns>返回原始数据</returns>
        public static string pcs_req_rawdata(BaiduPCS pcs, out int size, out string encode)
        {
            size = 0;
            encode = null;
            IntPtr sizePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
            IntPtr encodePtrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr sPtr = NativeMethods.pcs_req_rawdata(pcs.Handle, sizePtr, encodePtrPtr);
            string html = null;
            if (sPtr != IntPtr.Zero)
                html = NativeUtils.utf8_str(sPtr);
            size = Marshal.ReadInt32(sizePtr);
            IntPtr encodePtr = Marshal.ReadIntPtr(encodePtrPtr);
            if (encodePtr != IntPtr.Zero)
                encode = NativeUtils.str(encodePtr);
            Marshal.FreeHGlobal(sizePtr);
            Marshal.FreeHGlobal(encodePtrPtr);
            return html;
        }

        /// <summary>
        /// 清除错误消息
        /// </summary>
        /// <param name="pcs"></param>
        public static void pcs_clear_errmsg(BaiduPCS pcs)
        {
            NativeMethods.pcs_clear_errmsg(pcs.Handle);
        }

        /// <summary>
        /// 设置错误消息
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="errmsg">错误消息</param>
        public static void pcs_set_serrmsg(BaiduPCS pcs, string errmsg)
        {
            IntPtr errmsgPtr = NativeUtils.utf8_str_ptr(errmsg);
            NativeMethods.pcs_set_serrmsg(pcs.Handle, errmsgPtr);
            NativeUtils.free_str_ptr(errmsgPtr);
        }

        /// <summary>
        /// 添加错误消息到结尾
        /// </summary>
        /// <param name="pcs"></param>
        /// <param name="errmsg">错误消息</param>
        public static void pcs_cat_serrmsg(BaiduPCS pcs, string errmsg)
        {
            IntPtr errmsgPtr = NativeUtils.utf8_str_ptr(errmsg);
            NativeMethods.pcs_cat_serrmsg(pcs.Handle, errmsgPtr);
            NativeUtils.free_str_ptr(errmsgPtr);
        }

        /// <summary>
        /// 保存用户数据，返回用于读回该对象的 KEY。
        /// </summary>
        /// <param name="state">用户数据</param>
        /// <returns>返回 KEY </returns>
        private static string saveState(UserState state)
        {
            string key = Guid.NewGuid().ToString().ToLower();
            _ReadSliceStateCache.Add(key, state);
            return key;
        }

        /// <summary>
        /// 读取用户数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns>返回用户数据</returns>
        private static UserState getState(string key)
        {
            if (_ReadSliceStateCache.ContainsKey(key))
                return (UserState)_ReadSliceStateCache[key];
            return null;
        }

        /// <summary>
        /// 移除保存的用户数据
        /// </summary>
        /// <param name="key"></param>
        private static void removeState(string key)
        {
            if (_ReadSliceStateCache.ContainsKey(key))
                _ReadSliceStateCache.Remove(key);
        }

        /// <summary>
        /// 用户缓存用户数据
        /// </summary>
        private static Hashtable _ReadSliceStateCache = Hashtable.Synchronized(new Hashtable());

        private class UserState
        {
            public OnReadBlockFunction onReadSlice { get; set; }

            public object userData { get; set; }
        }

        #endregion



    }
}
