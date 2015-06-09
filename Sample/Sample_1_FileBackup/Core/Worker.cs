using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BaiduPCS_NET;

namespace FileBackup
{
    public abstract class Worker : IWorker
    {
        #region 属性

        public BaiduPCS pcs { get; private set; }

        public BackupItem backupItem { get; private set; }

        /// <summary>
        /// 用户数据存储目录
        /// </summary>
        public string userDirectory { get; private set; }

        /// <summary>
        /// 当前使用的日志文件
        /// </summary>
        public string log_file { get; private set; }

        /// <summary>
        /// 存储文件列表的日志文件
        /// </summary>
        public string list_file { get; private set; }

        /// <summary>
        /// 存储分片数据的目录
        /// </summary>
        public string slice_dir { get; private set; }

        /// <summary>
        /// 网盘中引用文件的全路径
        /// </summary>
        public string remote_ref_file
        {
            get { return backupItem.RemotePath + "/.meta/ref.txt"; }
        }

        /// <summary>
        /// 一共处理了多少个文件
        /// </summary>
        public int total { get; protected set; }

        /// <summary>
        /// 跳过了多少个文件
        /// </summary>
        public int skip { get; protected set; }

        /// <summary>
        /// 失败了多少个文件
        /// </summary>
        public int fail { get; protected set; }

        /// <summary>
        /// 发生了几次重命名操作
        /// </summary>
        public int rename_total { get; protected set; }

        /// <summary>
        /// 重命名失败的次数
        /// </summary>
        public int rename_fail { get; protected set; }

        public virtual string WorkerName
        {
            get { return ""; }
        }

        public event OnWorkDone Done;


        #endregion

        public Worker(BaiduPCS pcs, BackupItem backupItem, string userDir)
        {
            this.pcs = pcs;
            this.backupItem = backupItem;
            this.userDirectory = userDir;

        }

        /// <summary>
        /// 执行工作
        /// </summary>
        protected abstract void _Run();

        public void Run()
        {
            _PreRun();
            _Run();
            _RunCompleted();
        }

        /// <summary>
        /// 执行工作开始前的准备工作
        /// </summary>
        protected virtual void _PreRun()
        {
            #region 建立各目录

            if (!Directory.Exists(userDirectory))
                Directory.CreateDirectory(userDirectory);

            string logdir = Path.Combine(userDirectory, "log");
            if (!Directory.Exists(logdir))
                Directory.CreateDirectory(logdir);

            string listdir = Path.Combine(userDirectory, "list");
            if (!Directory.Exists(listdir))
                Directory.CreateDirectory(listdir);

            slice_dir = Path.Combine(userDirectory, "slice");
            if (!Directory.Exists(slice_dir))
                Directory.CreateDirectory(slice_dir);

            #endregion

            log_file = Path.Combine(logdir, "log-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt");
            list_file = Path.Combine(listdir, "list-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt");

            string metaFilename = Path.Combine(userDirectory, "meta.txt");
            if (!File.Exists(metaFilename))
                File.AppendAllText(metaFilename, "[Backup]\r\n\tLocalPath=" + backupItem.LocalPath + "\r\n\tRemotePath=" + backupItem.RemotePath + "\r\n");

            File.AppendAllText(metaFilename, "\r\nRun [" + WorkerName + "] at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff tt"));

        }

        /// <summary>
        /// 执行工作完成后的收尾工作
        /// </summary>
        protected virtual void _RunCompleted()
        {
            if (Done != null)
                Done(this);
        }

        /// <summary>
        /// 读取网盘中存储的最后一次备份的文件列表文件
        /// </summary>
        /// <returns></returns>
        protected virtual string ReadRemoteFileList()
        {
            string listfilename = pcs.cat(remote_ref_file);

            if (!string.IsNullOrEmpty(listfilename))
            {
                string listcontent = pcs.cat(listfilename);
                return listcontent;
            }
            return null;
        }

        /// <summary>
        /// 读取网盘中文件列表及其最后修改时间
        /// </summary>
        /// <returns>返回文件路径为 Key, 最后修改时间为 Value 的字典</returns>
        protected virtual SortedDictionary<string, long> ReadRemoteFileListAsDictionary()
        {
            SortedDictionary<string, long> list = new SortedDictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);

            string listcontent = ReadRemoteFileList();
            if(!string.IsNullOrEmpty(listcontent))
            {
                using (StringReader sr = new StringReader(listcontent))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line == "" || line.StartsWith("#"))
                            continue;
                        string[] sarr = line.Split('\t');
                        long lastModifyTime = Convert.ToInt64(sarr[0]);
                        string path = sarr[1];
                        list.Add(path, lastModifyTime);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 读取网盘中文件列表及其最后修改时间
        /// </summary>
        /// <returns>返回文件列表</returns>
        protected virtual IList<RemoteFileInfo> ReadRemoteFileListAsList()
        {
            IList<RemoteFileInfo> list = new List<RemoteFileInfo>();

            string listcontent = ReadRemoteFileList();
            if (!string.IsNullOrEmpty(listcontent))
            {
                using (StringReader sr = new StringReader(listcontent))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line == "" || line.StartsWith("#"))
                            continue;
                        string[] sarr = line.Split('\t');
                        long lastModifyTime = Convert.ToInt64(sarr[0]);
                        string path = sarr[1];
                        list.Add(new RemoteFileInfo() {
                            LastModifyTime = lastModifyTime,
                            Path = path
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 记录文件列表
        /// </summary>
        /// <param name="columns"></param>
        protected virtual void WriteList(params object[] columns)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object obj in columns)
            {
                if (sb.Length > 0) sb.Append("\t");
                if (obj != null)
                    sb.Append(obj.ToString());
            }
            sb.Append("\r\n");
            File.AppendAllText(list_file, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 写内容到日志文件
        /// </summary>
        /// <param name="content"></param>
        protected virtual void WriteLog(string content)
        {
            File.AppendAllText(log_file, "\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + content + "\r\n", Encoding.UTF8);
        }

        /// <summary>
        /// 写一行文本到控制台
        /// </summary>
        /// <param name="line"></param>
        /// <param name="y"></param>
        /// <param name="x"></param>
        protected virtual void WriteConsole(string line, int y = -1, int x = -1)
        {
            if (x >= 0)
                Console.CursorLeft = x;
            if (y >= 0)
                Console.CursorTop = y;
            Console.WriteLine(line);
        }
    }
}
