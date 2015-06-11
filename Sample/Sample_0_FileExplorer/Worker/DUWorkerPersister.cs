using System;
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
            foreach (XmlNode n in nodes)
            {
                OperationInfo op = new OperationInfo();
                op.uid = n.Attributes["uid"].Value;
                op.operation = (Operation)Enum.Parse(typeof(Operation), n.Attributes["operation"].Value);
                op.from = n.Attributes["from"].Value;
                op.to = n.Attributes["to"].Value;
                op.status = (OperationStatus)Enum.Parse(typeof(OperationStatus), n.Attributes["status"].Value);
                op.errmsg = n.Attributes["errmsg"].Value;
                op.doneSize = Convert.ToInt64(n.Attributes["doneSize"].Value);
                op.totalSize = Convert.ToInt64(n.Attributes["totalSize"].Value);
                if (op.operation == Operation.Download)
                    worker.queue.Enqueue(op);
                else if (op.operation == Operation.Upload)
                    worker.queue.Enqueue(op);
            }
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
