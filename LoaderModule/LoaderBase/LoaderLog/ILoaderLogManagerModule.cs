using System;
using System.Collections.Generic;

namespace LoaderBase.LoaderLog
{
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Param;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public interface ILoaderLogManagerModule : IModule
    {
        LoaderLogParameter LoaderLogParam { get; }
        EventCodeEnum StagesLogUploadServer(DateTime startdate, DateTime enddate);
        EventCodeEnum StagesPinTipSizeValidationImageUploadServer(DateTime startdate, DateTime enddate);
        EventCodeEnum ManualStageLogUploadServer();
        EventCodeEnum LoaderLogUploadServer(DateTime startdate, DateTime enddate);
        EventCodeEnum ManualLoaderLogUploadServer();
        EventCodeEnum ManualUploadStageAndLoader();

        EventCodeEnum SaveParameter();
        EventCodeEnum PMIImageUploadStageToLoader(int stageindex, byte[] data, string filename);
        Task<EventCodeEnum> PMIImageUploadLoaderToServer(int stageindex);
        EventCodeEnum PINImageUploadLoaderToServer(int stageindex, byte[] images);
        EventCodeEnum CellLogUploadToServer(int stageindex, EnumUploadLogType logtype);        
        EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid);
        List<CardImageBuffer> DownloadCardPatternImages(string devicename, int downimgcnt, string cardid);
        EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard);
        ProberCardListParameter DownloadProbeCardInfo(string cardID);
        void SetIntervalForLogUpload(int interval);
        void UpdateLogUploadListForLoader(EnumUploadLogType logtype);
        void UpdateLogUploadListForStage(int cellindex, EnumUploadLogType logType);
        void UploadRecentLogs(int cellindex = -1);
    }
    public class NetworkConnection : IDisposable
    {
        string _networkName;

        public NetworkConnection(string networkName,
            NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            if (result != 0)
            {
                //todo fixed
                //throw new Win32Exception(result);
            }
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }
        public static bool ConnectValidate(string networkName, NetworkCredential credentials)
        {
            bool valid = true;
            string networkname = null;
            try
            {
                networkname = networkName;

                var netResource = new NetResource()
                {
                    Scope = ResourceScope.GlobalNetwork,
                    ResourceType = ResourceType.Disk,
                    DisplayType = ResourceDisplaytype.Share,
                    RemoteName = networkName
                };

                var userName = string.IsNullOrEmpty(credentials.Domain)
                    ? credentials.UserName
                    : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

                var result = WNetAddConnection2(
                    netResource,
                    credentials.Password,
                    userName,
                    0);

                if (result != 0)
                {
                    valid = false;
                }
            }
            catch (Exception)
            {
                valid = false;
            }

            return valid;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplaytype DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    public enum ResourceScope : int
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    };

    public enum ResourceType : int
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }

    public enum ResourceDisplaytype : int
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }
}
