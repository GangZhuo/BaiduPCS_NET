using System;
using System.Xml;
using System.IO;
using System.Text;

namespace FileExplorer
{
    public class DUWorkerPersister
    {
        public string Filename { get; set; }

        public bool Valid()
        {
            return !string.IsNullOrEmpty(Filename);
        }

        public void RestoreWorker(DUWorker worker)
        {
            if (!Valid())
                return;
            if (!File.Exists(Filename))
                return;
            XmlDocument xml = new XmlDocument();
            xml.Load(Filename);
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
                op.finished = Convert.ToInt64(n.Attributes["finished"].Value);
                op.total = Convert.ToInt64(n.Attributes["total"].Value);
                if (op.operation == Operation.Download)
                {
                    if (op.status == OperationStatus.Success)
                        worker.completedDownload.Add(op);
                    else
                        worker.queue.queue.Enqueue(op);
                }
                else if (op.operation == Operation.Upload)
                {
                    if (op.status == OperationStatus.Success)
                        worker.completedUpload.Add(op);
                    else
                        worker.queue.queue.Enqueue(op);
                }
            }
        }

        public void SaveWorker(DUWorker worker)
        {
            if (!Valid())
                return;
            string dir = System.IO.Path.GetDirectoryName(Filename);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            using (FileStream fs = new FileStream(Filename, FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.Encoding = Encoding.UTF8;
                xws.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(fs, xws))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("items");

                    foreach (OperationInfo op in worker.queue)
                    {
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

                        writer.WriteStartAttribute("finished");
                        writer.WriteString(op.finished.ToString());
                        writer.WriteEndAttribute();

                        writer.WriteStartAttribute("total");
                        writer.WriteString(op.total.ToString());
                        writer.WriteEndAttribute();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }

    }
}
