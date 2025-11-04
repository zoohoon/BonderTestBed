namespace SpoolingUtil.TransferMethod
{
    /// <summary>
    /// Result MAP파일을 외부 서버에 Upload 및 Download에 대한 Interface
    /// </summary>
    public interface ITransferMethods
    {
        bool UploadFileData(string serverpath, string sourcepath, string filename, string user_name = "", string password = "", bool odtp = false);
        bool DownloadFileData(string serverpath, string filename, string downpath, string user_name = "", string password = "");
        bool PathCheck(string serverpath, string sub_folder, string user_name = "", string password = "");
        bool UploadFileProc(string destFullPath, string srcFullPath, string username, string password);
    }
}
