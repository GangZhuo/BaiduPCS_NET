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


    }
}
