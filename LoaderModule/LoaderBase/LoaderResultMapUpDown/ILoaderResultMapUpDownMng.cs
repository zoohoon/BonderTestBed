namespace LoaderBase.LoaderResultMapUpDown
{
    using ProberErrorCode;
    using ProberInterfaces;

    public interface ILoaderResultMapUpDownMng : IModule
    {
        EventCodeEnum CellResultMapUploadToServer(int stageindex, string filename);
        EventCodeEnum ServerResultMapDownloadToCell(int stageindex, string filename);
        EventCodeEnum ServerResultMapPathCheck(string foldername);
        void ShowManaualUploadDlg();
        void SpoolingOptionUpdate(string upload_basePath, string username, string password, bool ftp_userpassive, int retry_count, int reupload_delayTm);        
   }
}
