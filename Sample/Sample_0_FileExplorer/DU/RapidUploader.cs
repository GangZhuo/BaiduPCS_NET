using System;

using BaiduPCS_NET;

namespace FileExplorer
{
    public class RapidUploader : Uploader
    {
        public string FileMD5 { get; private set; }

        public RapidUploader(BaiduPCS pcs, string from, string to)
            : base(pcs, from, to)
        {
        }

        public override void Upload()
        {
            if (Uploading)
                throw new Exception("Can't upload, since the previous upload is not complete.");
            UploadedSize = 0;
            Success = false;
            IsCancelled = false;
            Error = null;
            Uploading = true;
            try
            {
                BaiduPCS pcs = this.pcs.clone();
                string filemd5 = null;
                string slicemd5 = null;
                Result = pcs.rapid_upload(to, from, ref filemd5, ref slicemd5, false);
                FileMD5 = filemd5;
                if (!Result.IsEmpty)
                {
                    Success = true;
                    IsCancelled = false;
                }
                else if (IsCancelled)
                {
                    Success = false;
                    IsCancelled = true;
                }
                else
                {
                    Success = false;
                    IsCancelled = false;
                    if (Error == null)
                        Error = new Exception(pcs.getError());
                }
            }
            catch (Exception ex)
            {
                Success = false;
                IsCancelled = false;
                Error = ex;
            }
            Uploading = false;
            fireOnCompleted(new CompletedEventArgs(Success, IsCancelled, Error));
        }

    }
}
