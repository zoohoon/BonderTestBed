using ProberErrorCode;
using ProberInterfaces.ODTP.ODTPFormat;
using System.Collections.Generic;
using System.Windows;

namespace ProberInterfaces.ODTP
{
    public interface IODTPManager : IFactoryModule, IModule
    {
        ODTPHeader ODTPHeader { get; set; }
        List<ODTPPCContact> ODTPPCContact { get; set; }
        string LocalUploadPath { get; }
        EventCodeEnum Upload();
        bool ODTPEnable();
    }


    public interface ILoaderODTPManager : IModule
    {
        EventCodeEnum CellODTPUploadToServer(int stageindex, string filename);
        EventCodeEnum ServerODTPPathCheck(string foldername);
        void ShowManaualUploadDlg();
        void SpoolingOptionUpdate(string upload_basePath, string username, string password, bool ftp_userpassive = false, int retry_count = 3, int reupload_delayTm = 5);
    
    }

    public class ManualProbingEventArg
    {
        public Point MXYIndex { get; set; }
        public double OverDrive { get; set; }

        public ManualProbingEventArg(Point mxyindex, double overdrive)
        {
            this.MXYIndex = mxyindex;
            this.OverDrive = overdrive;
        }
    }
}
