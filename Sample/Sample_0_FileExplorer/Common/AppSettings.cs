using System;
using System.Xml;
using System.IO;
using System.Text;

namespace FileExplorer
{
    public static class AppSettings
    {
        public static string SettingsFileName { get; set; }

        /// <summary>
        /// 获取或设置程序启动的时候，是否自动启动上传和下载线程
        /// </summary>
        public static bool ResumeDownloadAndUploadOnStartup { get; set; }

        /// <summary>
        /// 获取或设置下载时，允许的最大线程数
        /// </summary>
        public static int DownloadMaxThreadCount { get; set; }

        /// <summary>
        /// 获取或设置是否自动决定下载时使用的线程数
        /// </summary>
        public static bool AutomaticDownloadMaxThreadCount { get; set; }

        /// <summary>
        /// 获取或设置当下载失败时，是否自动重试
        /// </summary>
        public static bool RetryWhenDownloadFailed { get; set; }

        /// <summary>
        /// 获取或设置下载时的最小分片大小
        /// </summary>
        public static int MinDownloasSliceSize { get; set; }

        /// <summary>
        /// 获取或设置上传时，允许的最大线程数
        /// </summary>
        public static int UploadMaxThreadCount { get; set; }

        /// <summary>
        /// 获取或设置是否自动决定上传时使用的线程数
        /// </summary>
        public static bool AutomaticUploadMaxThreadCount { get; set; }

        /// <summary>
        /// 获取或设置当上传失败时，是否自动重试
        /// </summary>
        public static bool RetryWhenUploadFailed { get; set; }

        /// <summary>
        /// 上传时，覆写已经存在的文件
        /// </summary>
        public static bool OverWriteWhenUploadFile { get; set; }

        /// <summary>
        /// 磁盘缓存大小
        /// </summary>
        public static long MaxCacheSize { get; set; }

        public static bool Restore()
        {
            string filename = SettingsFileName;
            return RestoreFrom(filename);
        }

        public static bool RestoreFrom(string filename)
        {
            if (!File.Exists(filename))
                return false;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (TextReader reader = new StreamReader(fs, true))
                {
                    return RestoreFrom(reader);
                }
            }
        }

        public static bool RestoreFrom(TextReader reader)
        {
            Type t = typeof(AppSettings);
            XmlDocument xml = new XmlDocument();
            xml.Load(reader);
            XmlNodeList nodes = xml.SelectNodes("/items/item");
            foreach (XmlNode n in nodes)
            {
                string name = n.Attributes["name"].Value;
                string value = n.Attributes["value"].Value;
                switch(name)
                {
                    case "ResumeDownloadAndUploadOnStartup":
                        ResumeDownloadAndUploadOnStartup = Convert.ToBoolean(value);
                        break;

                    case "DownloadMaxThreadCount":
                        DownloadMaxThreadCount = Convert.ToInt32(value);
                        break;
                    case "AutomaticDownloadMaxThreadCount":
                        AutomaticDownloadMaxThreadCount = Convert.ToBoolean(value);
                        break;
                    case "RetryWhenDownloadFailed":
                        RetryWhenDownloadFailed = Convert.ToBoolean(value);
                        break;
                    case "MinDownloasSliceSize":
                        MinDownloasSliceSize = Convert.ToInt32(value);
                        break;

                    case "UploadMaxThreadCount":
                        UploadMaxThreadCount = Convert.ToInt32(value);
                        break;
                    case "AutomaticUploadMaxThreadCount":
                        AutomaticUploadMaxThreadCount = Convert.ToBoolean(value);
                        break;
                    case "RetryWhenUploadFailed":
                        RetryWhenUploadFailed = Convert.ToBoolean(value);
                        break;
                    case "OverWriteWhenUploadFile":
                        OverWriteWhenUploadFile = Convert.ToBoolean(value);
                        break;
                    case "MaxCacheSize":
                        MaxCacheSize = Convert.ToInt64(value);
                        break;

                }
            }
            return true;
        }

        public static bool Save()
        {
            string filename = SettingsFileName;
            return SaveTo(filename);
        }

        public static bool SaveTo(string filename)
        {
            string dir = System.IO.Path.GetDirectoryName(filename);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                return SaveTo(fs);
            }
        }

        public static bool SaveTo(Stream stream)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Encoding = Encoding.UTF8;
            xws.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(stream, xws))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("items");

                #region

                WriteItem(writer, "ResumeDownloadAndUploadOnStartup", ResumeDownloadAndUploadOnStartup.ToString());
                
                WriteItem(writer, "DownloadMaxThreadCount", DownloadMaxThreadCount.ToString());
                WriteItem(writer, "AutomaticDownloadMaxThreadCount", AutomaticDownloadMaxThreadCount.ToString());
                WriteItem(writer, "RetryWhenDownloadFailed", RetryWhenDownloadFailed.ToString());
                WriteItem(writer, "MinDownloasSliceSize", MinDownloasSliceSize.ToString());
                
                WriteItem(writer, "UploadMaxThreadCount", UploadMaxThreadCount.ToString());
                WriteItem(writer, "AutomaticUploadMaxThreadCount", AutomaticUploadMaxThreadCount.ToString());
                WriteItem(writer, "RetryWhenUploadFailed", RetryWhenUploadFailed.ToString());
                WriteItem(writer, "OverWriteWhenUploadFile", OverWriteWhenUploadFile.ToString());
                WriteItem(writer, "MaxCacheSize", MaxCacheSize.ToString());

                #endregion

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return true;
        }

        private static void WriteItem(XmlWriter writer, string name, string value)
        {
            writer.WriteStartElement("item");
            writer.WriteStartAttribute("name");
            writer.WriteString(name);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("value");
            writer.WriteString(value);
            writer.WriteEndAttribute();
            writer.WriteEndElement();
        }

    }
}
