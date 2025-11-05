using System;
using System.Collections.Generic;

namespace DeviceUpDownModule
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Network;
    using System.Xml.Serialization;

    [Serializable]
    public class DeviceUpDownSysParameter : ISystemParameterizable, IParam
    {
        public Element<IPAddressVer4> FtpServerIP { get; set; }
        public Element<int> FtpServerPort { get; set; }
        public Element<String> FtpUserAccount { get; set; }
        public Element<String> FtpUserPassword { get; set; }
        public Element<String> FtpServerDownloadPath { get; set; }
        public Element<String> FtpServerUploadPath { get; set; }
        public Element<bool> ClearDeviceOption { get; set; }



        [XmlIgnore, JsonIgnore]
        public String FilePath { get; } = "";
        [XmlIgnore, JsonIgnore]
        public String FileName { get; } = "DeviceUpDownSysParam.json";
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public String Genealogy { get; set; }
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public DeviceUpDownSysParameter()
        {
            FtpServerIP = new Element<IPAddressVer4>();
            FtpServerPort = new Element<int>();
            FtpUserAccount = new Element<String>();
            FtpUserPassword = new Element<string>();
            FtpServerDownloadPath = new Element<String>();
            FtpServerUploadPath = new Element<String>();
            ClearDeviceOption = new Element<bool>();
        }
        public DeviceUpDownSysParameter(
            String ftpServerIP,
            int ftpServerPort,
            String ftpUserAccount,
            String ftpUserPassword,
            String ftpServerDownloadPath,
            String ftpServerUploadPath,
            bool clearDeviceOption,
            bool loadDeviceOption)
            : this()
        {
            FtpServerIP.Value = IPAddressVer4.GetData(ftpServerIP);

            FtpServerPort.Value = ftpServerPort;
            FtpUserAccount.Value = ftpUserAccount;
            FtpUserPassword.Value = ftpUserPassword;
            FtpServerDownloadPath.Value = ftpServerDownloadPath;
            FtpServerUploadPath.Value = ftpServerUploadPath;
            ClearDeviceOption.Value = clearDeviceOption;
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SetEmulParam()
        {
            SetDefaultParam();
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SetDefaultParam()
        {
            FtpServerIP.Value = IPAddressVer4.GetData("192.168.1.5");
            FtpServerPort.Value = 21;
            FtpUserAccount.Value = "semicsrnd";
            FtpUserPassword.Value = "semics";
            FtpServerDownloadPath.Value = "FTP/FTPServer/OPUS5/FTPDownload";
            FtpServerUploadPath.Value = "FTP/FTPServer/OPUS5/FTPDownload";
            ClearDeviceOption.Value = false;

            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }
}
