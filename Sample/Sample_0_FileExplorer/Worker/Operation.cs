using System;

namespace FileExplorer
{
    public enum Operation
    {
        Download,
        Upload
    }

    public enum OperationStatus
    {
        Pending,
        Processing,
        Cancel,
        Success,
        Fail
    }

    public class OperationInfo
    {
        public string uid { get; set; }

        public Operation operation { get; set; }

        public string from { get; set; }

        public string to { get; set; }

        public OperationStatus status { get; set; }

        public string errmsg { get; set; }

        /// <summary>
        /// 完成量
        /// </summary>
        public long doneSize { get; set; }

        /// <summary>
        /// 总量
        /// </summary>
        public long totalSize { get; set; }

        /// <summary>
        /// 分片文件路径
        /// </summary>
        public string sliceFileName { get; set; }

        public object Tag { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is OperationInfo))
                return false;
            OperationInfo op = (OperationInfo)obj;
            if (op.operation != this.operation)
                return false;
            return string.Equals(this.from, op.from, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            string s = operation.ToString() + " " + from + " to " + to;
            return s.GetHashCode();
        }

        public override string ToString()
        {
            string s = "[" + status.ToString() + "] " + operation.ToString() + " " + from + " => " + to;

            if (status == OperationStatus.Fail)
                s += ": " + errmsg;
            else if (status == OperationStatus.Processing)
                s += ": " + Utils.HumanReadableSize(doneSize) + "/" + Utils.HumanReadableSize(totalSize);
            return s;
        }
    }
}
