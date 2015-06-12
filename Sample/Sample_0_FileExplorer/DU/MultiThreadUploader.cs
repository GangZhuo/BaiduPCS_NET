using System;
using System.IO;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;

using BaiduPCS_NET;
using BaiduPCS_NET.Native;

namespace FileExplorer
{
    public class MultiThreadUploader : Uploader
    {
        #region 常量

        /// <summary>
        /// 最小分片大小
        /// </summary>
        public const int MIN_SLICE_SIZE = 512 * Utils.KB; // 512 KB

        /// <summary>
        /// 最多可分多少个分片
        /// </summary>
        public const int MAX_SLICE_COUNT = 1024; // 10 MB

        #endregion

        public string WorkFolder { get; private set; }
        public int ThreadCount { get; private set; }
        public string SliceFileName { get; private set; }
        public List<Slice> SliceList { get; private set; }

        public string FileMD5 { get; set; }

        private FileInfo fromFileInfo;

        private long taskId = 0; //本地下载任务的 ID
        private long runningThreadCount = 0; //正在运行的线程数

        private object locker = new object();
        private object sliceFileLocker = new object();

        public MultiThreadUploader(BaiduPCS pcs, string from, string to,
            string workfolder, int threadCount)
            : base(pcs, from, to)
        {
            this.WorkFolder = workfolder;
            this.ThreadCount = threadCount;
        }

        public override void Upload()
        {
            if (Uploading)
                throw new Exception("Can't upload, since the previous upload is not complete.");
            DoneSize = 0;
            Success = false;
            IsCancelled = false;
            Error = null;
            Uploading = true;
            SliceFileName = null;
            SliceList = null;
            fromFileInfo = null;
            try
            {
                BaiduPCS pcs = this.pcs.clone();
                string key;
                if (string.IsNullOrEmpty(FileMD5))
                {
                    string validate_md5, validate2_md5;
                    fromFileInfo = new FileInfo(from);
                    if (!pcs.md5(from, 0, MIN_SLICE_SIZE, out validate_md5))
                        throw new Exception("Can't calculate md5 for " + from + ".");
                    if (!pcs.md5(from, fromFileInfo.Length - MIN_SLICE_SIZE, MIN_SLICE_SIZE, out validate2_md5))
                        throw new Exception("Can't calculate md5 for " + from + ".");
                    key = from + " => " + to + ", size=" + fromFileInfo.Length.ToString() + ", validate1_md5=" + validate_md5 + ", validate2_md5=" + validate2_md5;
                    key = Utils.md5(key.ToLower());
                }
                else
                    key = FileMD5;
                SliceFileName = "upload-" + key + ".slice";
                SliceFileName = Path.Combine(WorkFolder, pcs.getUID(), SliceFileName);
                StateFileNameDecideEventArgs args = new StateFileNameDecideEventArgs()
                {
                    SliceFileName = SliceFileName
                };
                fireStateFileNameDecide(args);
                SliceFileName = args.SliceFileName;
                CreateOrRestoreSliceList(); // 创建或还原分片列表
                foreach (Slice slice in SliceList)
                {
                    if (slice.status != SliceStatus.Successed)
                    {
                        slice.status = SliceStatus.Pending; //重新上传未成功的分片
                        slice.doneSize = 0;
                    }
                    else
                    {
                        DoneSize += slice.doneSize;
                    }
                }
                UploadSliceList(); // 启动线程来下载分片
                Wait(); // 等待所有线程退出
                List<string> md5list = new List<string>();
                if (CheckResult(md5list)) // 检查下载结果
                {
                    Result = pcs.create_superfile(to, md5list.ToArray(), false);
                    if (!Result.IsEmpty)
                    {
                        Success = true;
                        IsCancelled = false;
                    }
                    else
                    {
                        Success = false;
                        IsCancelled = false;
                        Error = new Exception(pcs.getError());
                    }
                }
                else
                {
                    Success = false;
                }
            }
            catch (Exception ex)
            {
                Success = false;
                IsCancelled = false;
                Error = ex;
            }
            if (Success)
                SliceHelper.DeleteSliceFile(SliceFileName);
            Uploading = false;
            fireCompleted(new CompletedEventArgs(Success, IsCancelled, Error));
        }

        public override void Cancel()
        {
            lock (locker) IsCancelled = true;
            StopAllDownloadThreads();
        }

        private void CreateOrRestoreSliceList()
        {
            // 分片文件存在，则从该文件中还原分片信息
            if (File.Exists(SliceFileName))
            {
                SliceList = SliceHelper.RestoreSliceList(SliceFileName);
                return;
            }
            long slizeSize = SliceHelper.CalculateSliceSize(fromFileInfo.Length, MIN_SLICE_SIZE, MAX_SLICE_COUNT);
            // 新建分片
            SliceList = SliceHelper.CreateSliceList(fromFileInfo.Length, (int)slizeSize);
            //保存一次新创建的分片列表
            SliceHelper.SaveSliceList(SliceFileName, SliceList);
        }

        private void UploadSliceList()
        {
            long tid = Interlocked.Increment(ref taskId);
            int tc = ThreadCount;
            if (tc > SliceList.Count)
                tc = SliceList.Count;
            Interlocked.Exchange(ref runningThreadCount, tc);
            for (int i = 0; i < tc; i++)
            {
                new Thread(new ParameterizedThreadStart(uploadTask)).Start(tid);
            }
        }

        private void Wait()
        {
            while (true)
            {
                if (Interlocked.Read(ref runningThreadCount) > 0)
                    Thread.Sleep(100);
                else
                    break;
            }
        }

        private bool CheckResult(IList<string> md5list)
        {
            bool succ;
            if (IsCancelled)
                succ = false;
            else if (Error != null)
                succ = false;
            else
            {
                succ = true;
                foreach (Slice slice in SliceList)
                {
                    if (slice.status != SliceStatus.Successed)
                    {
                        succ = false;
                        break;
                    }
                    else
                    {
                        md5list.Add(slice.md5);
                    }
                }
            }
            return succ;
        }

        private void uploadTask(object objTID)
        {
            long tid = (long)objTID;
            try
            {
                BaiduPCS pcs;
                Slice slice;
                PcsFileInfo rf;
                pcs = this.pcs.clone();
                while (tid == Interlocked.Read(ref taskId))
                {
                    slice = popSlice();
                    if (slice == null)
                        break;
                    slice.tid = tid;
                    pcs.WriteUserData = slice;
                    rf = pcs.upload_slicefile(new OnReadBlockFunction(Read), slice, (uint)slice.totalSize);
                    if (!string.IsNullOrEmpty(rf.md5))
                    {
                        slice.md5 = rf.md5;
                        slice.status = SliceStatus.Successed;
                    }
                    else if (slice.status == SliceStatus.Cancelled)
                        Cancel();
                    else
                    {
                        slice.status = SliceStatus.Failed;
                        lock (locker) Error = new Exception(pcs.getError());
                        StopAllDownloadThreads();
                    }
                    lock (sliceFileLocker) SliceHelper.SaveSliceList(SliceFileName, SliceList);
                }
            }
            catch (Exception ex)
            {
                lock (locker) Error = ex;
                StopAllDownloadThreads();
            }
            Interlocked.Decrement(ref runningThreadCount);
        }

        private void StopAllDownloadThreads()
        {
            Interlocked.Increment(ref taskId);
        }

        private Slice popSlice()
        {
            Slice slice = null;
            lock (SliceList)
            {
                for (int i = 0; i < SliceList.Count; i++)
                {
                    if (SliceList[i].status == SliceStatus.Pending)
                    {
                        slice = SliceList[i];
                        slice.status = SliceStatus.Running;
                        break;
                    }
                }
            }
            return slice;
        }

        // <returns>返回 0 表示已经到文件结尾，将停止上传。
        // 返回 NativeConst.CURL_READFUNC_ABORT 将离开停止上传，并返回上传错误；
        // 返回 NativeConst.CURL_READFUNC_PAUSE 将暂停上传。</returns>
        private int Read(BaiduPCS sender, out byte[] buf, uint size, uint nmemb, object userdata)
        {
            Slice slice = (Slice)userdata;
            buf = null;
            if (slice.tid != Interlocked.Read(ref taskId))//本次任务被取消
            {
                slice.status = SliceStatus.Cancelled;
                return NativeConst.CURL_READFUNC_ABORT;
            }
            int sz = (int)(size * nmemb);
            if (sz > slice.totalSize - slice.doneSize)
                sz = (int)(slice.totalSize - slice.doneSize);
            buf = new byte[sz];
            if (sz > 0)
            {
                using (FileStream fs = new FileStream(from, FileMode.Open, FileAccess.Read))
                {
                    fs.Position = slice.start + slice.doneSize;
                    sz = fs.Read(buf, 0, sz);
                }
            }
            slice.doneSize += sz;
            long uploadedSize = 0;
            lock (locker)
            {
                DoneSize += sz;
                uploadedSize = DoneSize;
            }
            ProgressEventArgs args = new ProgressEventArgs(uploadedSize, fromFileInfo.Length);
            fireProgress(args);
            if (args.Cancel)
            {
                slice.status = SliceStatus.Cancelled;
                return NativeConst.CURL_READFUNC_ABORT;
            }
            return sz;
        }
    }
}
