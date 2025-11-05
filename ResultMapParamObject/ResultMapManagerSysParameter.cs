using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Network;
using ProberInterfaces.ResultMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ResultMapParamObject
{
    public class ResultMapManagerSysParameter : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> IParam
        public string FilePath { get; set; } = "ResultMap";
        public string FileName { get; set; } = nameof(ResultMapManagerSysParameter) + ".json";
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; } = new List<object>();
        #endregion

        private HDDTransferInfo _HDDInfo;
        public HDDTransferInfo HDDInfo
        {
            get { return _HDDInfo; }
            set
            {
                if (value != _HDDInfo)
                {
                    _HDDInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FTPTransferInfo _FTPInfo;
        public FTPTransferInfo FTPInfo
        {
            get { return _FTPInfo; }
            set
            {
                if (value != _FTPInfo)
                {
                    _FTPInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _ODTPEnable = new Element<bool>() { Value = false };
        public Element<bool> ODTPEnable
        {
            get { return _ODTPEnable; }
            set
            {
                if (value != _ODTPEnable)
                {
                    _ODTPEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _uploadHDDEnable = new Element<bool>();
        public Element<bool> UploadHDDEnable
        {
            get { return _uploadHDDEnable; }
            set
            {
                if (value != _uploadHDDEnable)
                {
                    _uploadHDDEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _uploadFTPEnable = new Element<bool>();
        public Element<bool> UploadFTPEnable
        {
            get { return _uploadFTPEnable; }
            set
            {
                if (value != _uploadFTPEnable)
                {
                    _uploadFTPEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<FileManagerType> _downloadType = new Element<FileManagerType>();
        public Element<FileManagerType> DownloadType
        {
            get { return _downloadType; }
            set
            {
                if (value != _downloadType)
                {
                    _downloadType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.HDDInfo == null)
                {
                    this.HDDInfo = new HDDTransferInfo();

                    this.HDDInfo.UploadPath.Value = string.Empty;
                    this.HDDInfo.DownloadPath.Value = string.Empty;
                }

                if (this.FTPInfo == null)
                {
                    this.FTPInfo = new FTPTransferInfo();

                    FTPInfo.IP.Value = default(IPAddressVer4);
                    FTPInfo.Port.Value = 0;
                    FTPInfo.UserName.Value = string.Empty;
                    FTPInfo.UserPassword.Value = string.Empty;

                    FTPInfo.DownloadPath.Value = string.Empty;
                    FTPInfo.UploadPath.Value = string.Empty;
                }

                ODTPEnable.Value = false;
                UploadHDDEnable.Value = false;
                UploadFTPEnable.Value = false;

                DownloadType.Value = FileManagerType.UNDEFINED;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.HDDInfo == null)
                {
                    this.HDDInfo = new HDDTransferInfo();

                    this.HDDInfo.UploadPath.Value = @"C:\ProberSystem\ResultMap\Upload\";
                    this.HDDInfo.DownloadPath.Value = @"C:\ProberSystem\ResultMap\Upload\";
                }

                if (this.FTPInfo == null)
                {
                    this.FTPInfo = new FTPTransferInfo();

                    FTPInfo.IP.Value = IPAddressVer4.GetData("192.168.1.5");
                    FTPInfo.Port.Value = 21;
                    FTPInfo.UserName.Value = "semicsrnd";
                    FTPInfo.UserPassword.Value = "semics";

                    FTPInfo.DownloadPath.Value = "FTP/FTPServer/FTPDownload";
                    FTPInfo.UploadPath.Value = "FTP/FTPServer/FTPDownload";
                }

                UploadHDDEnable.Value = false;
                UploadFTPEnable.Value = false;

                DownloadType.Value = FileManagerType.HDD;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetElementMetaData()
        {
            try
            {
                HDDInfo.UploadPath.CategoryID = "00080003";
                HDDInfo.UploadPath.ElementName = "Upload path (HDD)";
                HDDInfo.UploadPath.Description = "Path used when uploading files to HDD";

                HDDInfo.DownloadPath.CategoryID = "00080003";
                HDDInfo.DownloadPath.ElementName = "Download path (HDD)";
                HDDInfo.DownloadPath.Description = "Path used when downloading files to HDD";

                FTPInfo.UploadPath.CategoryID = "00080003";
                FTPInfo.UploadPath.ElementName = "Upload path (FTP)";
                FTPInfo.UploadPath.Description = "Path used when uploading files to FTP server";

                FTPInfo.DownloadPath.CategoryID = "00080003";
                FTPInfo.DownloadPath.ElementName = "Download path (FTP)";
                FTPInfo.DownloadPath.Description = "Path used when downloading files to FTP server";

                FTPInfo.IP.CategoryID = "00080003";
                FTPInfo.IP.ElementName = "IP (FTP)";
                FTPInfo.IP.Description = "IP used when connecting to FTP server";

                FTPInfo.Port.CategoryID = "00080003";
                FTPInfo.Port.ElementName = "Port (FTP)";
                FTPInfo.Port.Description = "Port used when connecting to FTP server";

                FTPInfo.UserName.CategoryID = "00080003";
                FTPInfo.UserName.ElementName = "User name (FTP)";
                FTPInfo.UserName.Description = "User id used when connecting to FTP server";

                FTPInfo.UserPassword.CategoryID = "00080003";
                FTPInfo.UserPassword.ElementName = "User password (FTP)";
                FTPInfo.UserPassword.Description = "User password used when connecting to FTP server";

                UploadHDDEnable.CategoryID = "00080003";
                UploadHDDEnable.ElementName = "Upload enable (HDD)";
                UploadHDDEnable.Description = "Whether to use HDD upload";

                UploadFTPEnable.CategoryID = "00080003";
                UploadFTPEnable.ElementName = "Upload enable (FTP)";
                UploadFTPEnable.Description = "Whether to use FTP upload";

                DownloadType.CategoryID = "00080003";
                DownloadType.ElementName = "Download type";
                DownloadType.Description = "When downloading, set the location to get the result map";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
