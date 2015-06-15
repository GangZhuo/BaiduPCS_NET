using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace FileExplorer
{
    public class DUWorkerPersister
    {
        public DUWorker worker { get; private set; }

        public DUWorkerPersister(DUWorker worker)
        {
            this.worker = worker;
        }

        private string GetFileName()
        {
            string filename = Path.Combine(worker.workfolder, worker.pcs.getUID(), "worker.xml");
            return filename;
        }

        public bool Restore()
        {
            string filename = GetFileName();
            return RestoreFrom(filename);
        }

        public bool RestoreFrom(string filename)
        {
            if (!File.Exists(filename))
                return false;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using(TextReader reader = new StreamReader(fs, true))
                {
                    return RestoreFrom(reader);
                }
            }
        }

        public bool RestoreFrom(TextReader reader)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(reader);
            XmlNodeList nodes = xml.SelectNodes("/items/item");
            List<OperationInfo> list = new List<OperationInfo>(nodes.Count);
            foreach (XmlNode n in nodes)
            {
                OperationInfo op = new OperationInfo();
                XmlAttribute attr;

                attr = n.Attributes["uid"];
                op.uid = attr != null ? attr.Value : "";

                attr = n.Attributes["operation"];
                op.operation = attr != null ? (Operation)Enum.Parse(typeof(Operation), attr.Value) : Operation.Download;

                attr = n.Attributes["from"];
                op.from = attr != null ? attr.Value : "";

                attr = n.Attributes["to"];
                op.to = attr != null ? attr.Value : "";

                attr = n.Attributes["status"];
                op.status = attr != null ? (OperationStatus)Enum.Parse(typeof(OperationStatus), attr.Value) : OperationStatus.Pending;

                attr = n.Attributes["errmsg"];
                op.errmsg = attr != null ? attr.Value : "";

                attr = n.Attributes["doneSize"];
                op.doneSize = attr != null ? Convert.ToInt64(attr.Value) : 0;

                attr = n.Attributes["totalSize"];
                op.totalSize = attr != null ? Convert.ToInt64(attr.Value) : 0;

                attr = n.Attributes["sliceFileName"];
                op.sliceFileName = attr != null ? attr.Value : "";

                list.Add(op);
            }
            worker.queue.Add(list.ToArray());
            return true;
        }

        public bool Save()
        {
            string filename = GetFileName();
            return SaveTo(filename);
        }

        public bool SaveTo(string filename)
        {
            string dir = System.IO.Path.GetDirectoryName(filename);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                return SaveTo(fs);
            }
        }

        public bool SaveTo(Stream stream)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Encoding = Encoding.UTF8;
            xws.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(stream, xws))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("items");

                foreach (OperationInfo op in worker.queue.list())
                {
                    #region 

                    writer.WriteStartElement("item");

                    writer.WriteStartAttribute("uid");
                    writer.WriteString(op.uid == null ? string.Empty : op.uid);
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("operation");
                    writer.WriteString(op.operation.ToString());
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("from");
                    writer.WriteString(op.from == null ? string.Empty : op.from);
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("to");
                    writer.WriteString(op.to == null ? string.Empty : op.to);
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("status");
                    writer.WriteString(op.status.ToString());
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("errmsg");
                    writer.WriteString(op.errmsg == null ? string.Empty : op.errmsg);
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("doneSize");
                    writer.WriteString(op.doneSize.ToString());
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("totalSize");
                    writer.WriteString(op.totalSize.ToString());
                    writer.WriteEndAttribute();

                    writer.WriteStartAttribute("sliceFileName");
                    writer.WriteString(op.sliceFileName);
                    writer.WriteEndAttribute();

                    writer.WriteEndElement();

                    #endregion
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return true;
        }
    }
}
