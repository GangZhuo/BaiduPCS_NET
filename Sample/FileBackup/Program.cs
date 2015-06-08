using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using BaiduPCS_NET;

namespace FileBackup
{
    class Program
    {
        static BackupItem[] backupItems = new BackupItem[] {
            new BackupItem(
                "D:\\images",
                "/backup/images"),
        };

        static Hashtable argIndex;

        static Mail mail;

        public static void Main(string[] args)
        {
            bool noerr = false;
            try
            {
                prepare();

                WriteLog("Start");

                #region 为参数建立索引

                argIndex = new Hashtable(args.Length);
                foreach (string arg in args)
                {
                    string key = arg,
                        value = null;
                    int i = arg.IndexOf('=');
                    if (i != -1)
                    {
                        key = arg.Substring(0, i);
                        value = arg.Substring(i + 1);
                    }
                    if (!argIndex.ContainsKey(key))
                        argIndex.Add(key, value);
                    else
                        argIndex[key] = value;
                }

                #endregion

                #region 验证参数是否正确

                if (argIndex.ContainsKey("backup") && argIndex.ContainsKey("restore"))
                {
                    Console.WriteLine("Can't do backup and restore in same time. You should use \"FileBackup.exe backup\" or \"FileBackup.exe restore\" alone.");
                    WriteLog("Can't do backup and restore in same time.");
                    return;
                }

                #endregion

                backupItems = ReadBackupItems();
                if (backupItems.Length == 0)
                {
                    Console.WriteLine("Can't read config file or have no backup items: " + config_file);
                    WriteLog("Can't read config file or have no backup items: " + config_file);
                    return;
                }

                if (InitBaiduPCS())
                {
                    Run();
                }
                noerr = true;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                WriteLog("End");
                UninitBaiduPCS();
                if (mail.IsValid())
                {
                    try
                    {
                        mail.SendLogFile("All task completed" + (noerr ? " with no error" : " with error"), "See attached log file.", log_file);
                    }
                    catch (Exception ex2)
                    {
                        WriteLog("Send mail error: " + ex2.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 创建程序需要使用的目录；
        /// 创建 日志文件 路径；
        /// 创建 COOKIE 路径。
        /// </summary>
        static void prepare()
        {
            #region 建立程序使用的目录

            user_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".pcs");
            if (!Directory.Exists(user_dir))
                Directory.CreateDirectory(user_dir);

            string log_dir = Path.Combine(user_dir, "log");
            if (!Directory.Exists(log_dir))
                Directory.CreateDirectory(log_dir);

            #endregion

            log_file = Path.Combine(log_dir, DateTime.Now.ToString("yyyyMMdd") + ".log");
            cookie_file_name = Path.Combine(user_dir, "cookie.txt");
            config_file = Path.Combine(user_dir, "config.txt");
            mail_config_file = Path.Combine(user_dir, "mail.txt");

            mail = new Mail();
            mail.ReadConfig(mail_config_file);
            if (mail.IsValid())
                mail.Init();
        }

        static bool InitBaiduPCS()
        {
            PcsRes rc;

            #region 创建 BaiduPCS 对象，并尝试登录

            pcs = BaiduPCS.pcs_create(cookie_file_name);
            if (pcs == null)
            {
                Console.WriteLine("Can't create BaiduPCS.");
                WriteLog("Can't create BaiduPCS.");
                WriteLog("End");
                return false;
            }
            Console.WriteLine("Backup files to BaiduPCS (SDK " + pcs.version() + ") " + GetVerstionString());
            rc = pcs.isLogin();
            if (rc != PcsRes.PCS_LOGIN)
            {
                if (argIndex.ContainsKey("--login"))
                {
                    string errmsg;
                    string username = null,
                        password = null;
                    while (string.IsNullOrEmpty(username))
                    {
                        Console.Write("User Name: ");
                        username = Console.ReadLine();
                    }
                    while (string.IsNullOrEmpty(password))
                    {
                        Console.Write("Password: ");
                        password = ReadPassword();
                    }
                    pcs.GetCaptcha += new GetCaptchaFunction(OnGetCaptcha);

                    rc = pcs.login(username, password);
                    if (rc != PcsRes.PCS_OK)
                    {
                        errmsg = pcs.getError();
                        Console.WriteLine("Can't login BaiduPCS: " + errmsg);
                        WriteLog("Can't login BaiduPCS: " + errmsg);
                        WriteLog("End");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("The BaiduPCS is not log in.  You should use \"FileBackup.exe --login\" to login BaiduPCS once.");
                    WriteLog("The BaiduPCS is not log in.");
                    WriteLog("End");
                    return false;
                }
            }
            Console.WriteLine("UID: " + pcs.getUID());

            #endregion

            return true;
        }

        static void UninitBaiduPCS()
        {
            try
            {
                if (pcs != null)
                {
                    pcs.Dispose();
                    pcs = null;
                }
            }
            catch { }
        }

        static void Run()
        {
            IWorker worker;
            if (argIndex.ContainsKey("backup"))
            {
                foreach (BackupItem backupItem in backupItems)
                {
                    string userdir = Path.Combine(user_dir, MD5.Encrypt(backupItem.LocalPath.ToLower()));
                    WriteLog("Backup " + backupItem.LocalPath + " => " + backupItem.RemotePath
                        + "\r\nSee details at " + userdir);
                    worker = new BackupWorker(pcs, backupItem, userdir);
                    worker.Done += onWorkDone;
                    worker.Run();
                }
            }
            else if (argIndex.ContainsKey("restore"))
            {
                foreach (BackupItem backupItem in backupItems)
                {
                    string userdir = Path.Combine(user_dir, MD5.Encrypt(backupItem.LocalPath.ToLower()));
                    WriteLog("Restore " + backupItem.RemotePath + " => " + backupItem.LocalPath
                        + "\r\nSee details at " + userdir);
                    worker = new RestoreWorker(pcs, backupItem, userdir);
                    worker.Done += onWorkDone;
                    worker.Run();
                }
            }
        }

        static void onWorkDone(IWorker sender)
        {
            if (mail.IsValid())
            {
                try
                {
                    Worker worker = (Worker)sender;
                    if (worker is BackupWorker)
                    {
                        mail.SendLogFile(worker.WorkerName + " " + worker.backupItem.LocalPath + " completed",
                            worker.WorkerName + " " + worker.backupItem.LocalPath + " => " + worker.backupItem.RemotePath,
                            worker.log_file);
                    }
                    else if (worker is RestoreWorker)
                    {
                        mail.SendLogFile(worker.WorkerName + " " + worker.backupItem.RemotePath + " completed",
                            worker.WorkerName + " " + worker.backupItem.LocalPath + " => " + worker.backupItem.RemotePath,
                            worker.log_file);
                    }
                    else
                    {
                        mail.SendLogFile(worker.WorkerName + " completed",
                            worker.WorkerName + " " + worker.backupItem.LocalPath + " => " + worker.backupItem.RemotePath,
                            worker.log_file);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Send mail error: " + ex.Message);
                }
            }
        }

        static BackupItem[] ReadBackupItems()
        {
            List<BackupItem> list = new List<BackupItem>();
            if (!File.Exists(config_file))
            {
                File.AppendAllText(config_file, @"# The backup items shoud be one line one item,
# and two path split by ""=>"", like below.
#   Local Path  =>  Remote Path
# The remote path should be not end with '/', and must be full path (start with '/'), See below Sample:
#
#   # Correct
#   D:\images => /backup/images
#
#   # Correct
#   D:\books => /backup/books
#
#   # Wrong, because the remote path end with '/'.
#   D:\books => /backup/books/
#
#   # Wrong, because the remote path must be a full path (Start with '/').
#   D:\books => books
");
                return list.ToArray();
            }
            using (StreamReader sr = new StreamReader(config_file))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;
                    string[] sarr = line.Split(new string[] { "=>" }, StringSplitOptions.None);
                    BackupItem item = new BackupItem(sarr[0].Trim(), sarr[1].Trim());
                    if (!string.IsNullOrEmpty(item.LocalPath) && !string.IsNullOrEmpty(item.RemotePath))
                    {
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }

        static bool OnGetCaptcha(BaiduPCS sender, byte[] imgBytes, out string captcha, object userdata)
        {
            captcha = null;
            System.Drawing.Image img = null;
            using(MemoryStream ms = new MemoryStream(imgBytes))
            {
                img = System.Drawing.Image.FromStream(ms);
            }
            string imgfilename = Path.Combine(user_dir, "captcha.jpg");
            img.Save(imgfilename, System.Drawing.Imaging.ImageFormat.Jpeg);
            while (string.IsNullOrEmpty(captcha))
            {
                Console.WriteLine("The captcha saved to " + imgfilename + " . Please read it, and enter the captcha.");
                Console.Write("Captcha: ");
                captcha = Console.ReadLine();
            }
            return true;
        }

        static void WriteLog(string content)
        {
            File.AppendAllText(log_file, "\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + content + "\r\n", Encoding.UTF8);
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }

        public static string GetVerstionString()
        {
            return GetVerstionString(System.Reflection.Assembly.GetCallingAssembly());
        }

        public static string GetVerstionString(System.Reflection.Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly can not be null.");
            }
            Version ver = assembly.GetName().Version;
            string version = string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            return version;
        }

        /// <summary>
        /// 保存用户数据的目录
        /// </summary>
        static string user_dir;

        /// <summary>
        /// 当前使用的日志文件
        /// </summary>
        static string log_file;

        /// <summary>
        /// 保存 COOKIE 的文件
        /// </summary>
        static string cookie_file_name;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        static string config_file;

        static string mail_config_file;

        static BaiduPCS pcs;
    }
}
