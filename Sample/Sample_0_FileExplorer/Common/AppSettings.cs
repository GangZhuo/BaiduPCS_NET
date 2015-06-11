using System;
using System.Xml;
using System.IO;
using System.Text;

namespace FileExplorer
{
    public static class AppSettings
    {
        public static string SettingsFileName { get; set; }

        public static int DownloadMaxThreadCount { get; set; }
        public static bool AutomaticDownloadMaxThreadCount { get; set; }

        public static int UploadMaxThreadCount { get; set; }
        public static bool AutomaticUploadMaxThreadCount { get; set; }

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
                    case "DownloadMaxThreadCount":
                        DownloadMaxThreadCount = Convert.ToInt32(value);
                        break;
                    case "AutomaticDownloadMaxThreadCount":
                        AutomaticDownloadMaxThreadCount = Convert.ToBoolean(value);
                        break;

                    case "UploadMaxThreadCount":
                        UploadMaxThreadCount = Convert.ToInt32(value);
                        break;
                    case "AutomaticUploadMaxThreadCount":
                        AutomaticUploadMaxThreadCount = Convert.ToBoolean(value);
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

                writer.WriteStartElement("item");
                writer.WriteStartAttribute("name");
                writer.WriteString("DownloadMaxThreadCount");
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("value");
                writer.WriteString(DownloadMaxThreadCount.ToString());
                writer.WriteEndAttribute();
                writer.WriteEndElement();

                writer.WriteStartElement("item");
                writer.WriteStartAttribute("name");
                writer.WriteString("AutomaticDownloadMaxThreadCount");
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("value");
                writer.WriteString(AutomaticDownloadMaxThreadCount.ToString());
                writer.WriteEndAttribute();
                writer.WriteEndElement();

                writer.WriteStartElement("item");
                writer.WriteStartAttribute("name");
                writer.WriteString("UploadMaxThreadCount");
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("value");
                writer.WriteString(UploadMaxThreadCount.ToString());
                writer.WriteEndAttribute();
                writer.WriteEndElement();

                writer.WriteStartElement("item");
                writer.WriteStartAttribute("name");
                writer.WriteString("AutomaticUploadMaxThreadCount");
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("value");
                writer.WriteString(AutomaticUploadMaxThreadCount.ToString());
                writer.WriteEndAttribute();
                writer.WriteEndElement();

                #endregion

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return true;
        }

    }
}
