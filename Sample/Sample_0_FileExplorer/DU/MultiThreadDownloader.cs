using System;
using System.IO;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;

using BaiduPCS_NET;

namespace FileExplorer
{
    public class MultiThreadDownloader : Downloader
    {
        #region 常量

        /// <summary>
        /// 最小分片大小
        /// </summary>
        public const int MIN_SLICE_SIZE = 512 * Utils.KB; // 512 KB

        /// <summary>
        /// 最大分片大小
        /// </summary>
        public const int MAX_SLICE_SIZE = 10 * Utils.MB; // 10 MB

        #endregion

        public string WorkFolder { get; private set; }
        public int ThreadCount { get; private set; }
        public int MinSliceSize { get; private set; }
        public string SliceFileName { get; private set; }
        public List<Slice> SliceList { get; private set; }

        private IBigFileHelper bigfile;
        private long taskId = 0; //本地下载任务的 ID
        private long runningThreadCount = 0; //正在运行的线程数
        private object locker = new object();
        private object sliceFileLocker = new object();

        public MultiThreadDownloader(BaiduPCS pcs, PcsFileInfo from, string to,
            string workfolder, int threadCount, int minSliceSize = MIN_SLICE_SIZE)
            : base(pcs, from, to)
        {
            this.WorkFolder = workfolder;
            this.ThreadCount = threadCount;
            this.MinSliceSize = minSliceSize;
        }

        public override void Download()
        {
            if (Downloading)
                throw new Exception("Can't download, since the previous download is not complete.");
            DoneSize = 0;
            Success = false;
            IsCancelled = false;
            Error = null;
            Downloading = true;
            SliceFileName = null;
            SliceList = null;
            bigfile = null;
            try
            {
                SliceFileName = "download-" + from.md5 + ".slice";
                SliceFileName = Path.Combine(Path.Combine(WorkFolder, pcs.getUID()), SliceFileName);
                StateFileNameDecideEventArgs args = new StateFileNameDecideEventArgs()
                {
                    SliceFileName = SliceFileName
                };
                fireStateFileNameDecide(args);
                SliceFileName = args.SliceFileName;
                CreateOrRestoreSliceList(); // 创建或还原分片列表
                CreateDirectory(to); //创建目录
                CreateLocalFile(); // 如果需要则创建本地文件
                bigfile = new BigFileHelper(to, from.size); //映射文件到内存
                foreach (Slice slice in SliceList)
                {
                    if (slice.status != SliceStatus.Successed)
                    {
                        slice.status = SliceStatus.Pending; //重新下载未成功的分片
                        slice.doneSize = 0;
                    }
                    else
                    {
                        DoneSize += slice.doneSize;
                    }
                }
                DownloadSliceList(); // 启动线程来下载分片
                Wait(); // 等待所有线程退出
                CheckResult(); // 检查下载结果
            }
            catch (Exception ex)
            {
                Success = false;
                IsCancelled = false;
                Error = ex;
            }
            if (bigfile != null)
            {
                bigfile.Dispose();
                bigfile = null;
            }
            if (Success)
                SliceHelper.DeleteSliceFile(SliceFileName);
            Downloading = false;
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
            // 新建分片
            SliceList = SliceHelper.CreateSliceList(from.size, MinSliceSize);
            //保存一次新创建的分片列表
            SliceHelper.SaveSliceList(SliceFileName, SliceList);
        }

        private void CreateLocalFile()
        {
            if (!File.Exists(to))
            {
                //预先创建一个大文件
                using (FileStream fs = File.Create(to)) { }
            }
        }

        private void DownloadSliceList()
        {
            long tid = Interlocked.Increment(ref taskId);
            int tc = ThreadCount;
            if (tc > SliceList.Count)
                tc = SliceList.Count;
            Interlocked.Exchange(ref runningThreadCount, tc);
            for (int i = 0; i < tc; i++)
            {
                new Thread(new ParameterizedThreadStart(downloadTask)).Start(tid);
            }
        }

        private void Wait()
        {
            int prev, running;

            prev = (int)Interlocked.Read(ref runningThreadCount);
            fireThreadChanged(new ThreadCountChangedEventArgs(prev, ThreadCount));

            while (true)
            {
                running = (int)Interlocked.Read(ref runningThreadCount);
                if (running != prev)
                {
                    prev = running;
                    fireThreadChanged(new ThreadCountChangedEventArgs(running, ThreadCount));
                }
                if (running > 0)
                    Thread.Sleep(100);
                else
                    break;
            }
        }

        private void CheckResult()
        {
            if (IsCancelled)
                Success = false;
            else if (Error != null)
                Success = false;
            else
            {
                bool succ = true;
                foreach (Slice slice in SliceList)
                {
                    if (slice.status != SliceStatus.Successed)
                    {
                        succ = false;
                        break;
                    }
                }

                Success = succ;
            }
        }

        private void downloadTask(object objTID)
        {
            long tid = (long)objTID;
            try
            {
                BaiduPCS pcs;
                Slice slice;
                PcsRes rc;
                string errmsg;
                pcs = this.pcs.clone();
                pcs.Write += onWrite;
                while (tid == Interlocked.Read(ref taskId))
                {
                    slice = popSlice();
                    if (slice == null)
                        break;
                    slice.tid = tid;
                    pcs.WriteUserData = slice;
                    do
                    {
                        errmsg = null;
                        if (slice.status == SliceStatus.Failed)
                            slice.status = SliceStatus.Retrying;
                        rc = pcs.download(from.path, 0, 
                            slice.start + slice.doneSize,
                            slice.totalSize - slice.doneSize);
                        if (rc == PcsRes.PCS_OK || slice.status == SliceStatus.Successed)
                            slice.status = SliceStatus.Successed;
                        else if (slice.status == SliceStatus.Cancelled)
                            Cancel();
                        else
                        {
                            slice.status = SliceStatus.Failed;
                            errmsg = pcs.getError();
                        }
                    } while (slice.status == SliceStatus.Failed &&
                        AppSettings.RetryWhenDownloadFailed &&
                        errmsg != null &&
                        errmsg.Contains("Timeout"));

                    if (slice.status != SliceStatus.Successed && slice.status != SliceStatus.Cancelled)
                    {
                        lock (locker) Error = new Exception(errmsg);
                        StopAllDownloadThreads();
                    }
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

        private uint onWrite(BaiduPCS sender, byte[] data, uint contentlength, object userdata)
        {
            Slice slice = (Slice)userdata;
            if (slice.tid != Interlocked.Read(ref taskId))//本次任务被取消
            {
                lock (locker)
                {
                    if (IsCancelled)
                        slice.status = SliceStatus.Cancelled;
                    else
                        slice.status = SliceStatus.Failed;
                }
                return 0;
            }
            long size = data.Length;
            if (size > slice.totalSize - slice.doneSize)
                size = slice.totalSize - slice.doneSize;
            if (size > 0)
            {
                try
                {
                    bigfile.Update(slice.start + slice.doneSize, data, 0, (int)size);
                }
                catch(Exception ex)
                {
                    throw;
                }
            }
            slice.doneSize += size;
            lock (locker) DoneSize += size;
            if (slice.doneSize == slice.totalSize) //分片已经下载完成
            {
                slice.status = SliceStatus.Successed;
                size = 0;
            }
            lock (sliceFileLocker) SliceHelper.SaveSliceList(SliceFileName, SliceList); // 保存最新的分片数据
            long downloadedSize = 0;
            lock (locker) downloadedSize = DoneSize;
            ProgressEventArgs args = new ProgressEventArgs(downloadedSize, from.size);
            fireProgress(args);
            if (args.Cancel)
            {
                slice.status = SliceStatus.Cancelled;
                return 0;
            }
            return (uint)size;
        }
    }
}
