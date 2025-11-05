using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;

namespace ProberInterfaces
{
    public interface ILoaderFileManager : IFactoryModule
    {

    }

    [ServiceContract]
    public interface IFileManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        FileManagerParam FileManagerParam { get; }
        string DevFolder { get; }

        string GetLogRootPath();
        string GetSystemRootPath();
        string GetDeviceRootPath();

        string GetRootParamPath();
        string GetImageSavePath(EnumProberModule moduletype, bool appendTrailingSlash = false, params string[] paths);
        string GetImageSaveFullPath(EnumProberModule moduletype, IMAGE_SAVE_TYPE type, bool AppendCurrentTime, params string[] paths);

        bool GetClipImagePath(string path, out string clippath);

        [OperationContract]
        bool IsServiceAvailable();

        [OperationContract]
        string GetSystemParamFullPath(string parameterPath, string parameterName);

        [OperationContract]
        string GetDeviceParamFullPath(string parameterPath = "", string parameterName = "", bool isContainDeviceName = true);

        [OperationContract]
        string GetDeviceName();

        [OperationContract]
        EventCodeEnum ChangeDevice(string DevName);
        [OperationContract]
        EventCodeEnum DeleteDevice(string DevName);
        EventCodeEnum DeleteFilesInDirectory(string targetpath);

        string PultCommandString { get; set; }

        byte[] GetNewDeviceFolderUsingName(string devName);
        byte[] GetSaveAsDeviceUsingName(string devName);

        [OperationContract]
        ObservableCollection<SimpleDeviceInfo> GetDevicelistTest();

        [OperationContract]
        byte[] GetFileManagerParam();
        [OperationContract]
        ObservableCollection<DeviceInfo> GetDevicelist();

        [OperationContract]
        byte[] GetDevicebytelist();
        [OperationContract]
        ObservableCollection<string> GetDeviceNamelist();

        [OperationContract]
        byte[] GetDeviceByFileName(string filename);
        [OperationContract]
        bool SetDeviceByFileName(byte[] device, string devicename);

        [OperationContract]
        byte[] CompressFileToStream(string filePath, int count);

        [OperationContract]
        byte[] Decompress(byte[] data);

        [OperationContract]
        void DecompressFilesFromByteArray(byte[] param, string filepath);
        [OperationContract]
        string GetProberID();
        [OperationContract]
        void SetProberID(string proberid);
        [OperationContract]
        void InitData();
        [OperationContract]
        byte[] GetCompressedFile(DateTime startdate, DateTime enddate, List<EnumLoggerType> logtypes, List<EnumProberModule> imagetypes, bool includeGEM = false, bool includeClip = false, bool includeLoadedDevice = false, bool inlcudeSystemparam = false, bool inlcudebackupinfo = false, bool inlcudeSysteminfo = false);

        void DecompressFile(byte[] compressedFileData, string outputFilePath);
        [OperationContract]
        long GetFreeSpaceFromDrive(string filePath);
        [OperationContract]
        long GetFileSize(string filePath);
    }

    [Serializable]
    public class FileManagerParam : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore]
        public const string DefaultSystemParamPath = @"C:\ProberSystem\Default\Parameters\SystemParam";
        [JsonIgnore]
        public const string DefaultDeviceParamPath = @"C:\ProberSystem\Default\Parameters\DeviceParam";
        [JsonIgnore]
        public const string DefaultLogPath = @"C:\Logs";
        [JsonIgnore]
        public const string DefaultDeviceName = "DEFAULTDEVNAME";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        public FileManagerParam()
        {
            PinAlignHighKeySaveOriginalImage.Value = true;
            PinAlignHighKeySavePassImage.Value = true;
            PinAlignHighKeySaveFailImage.Value = true;
        }

        public string LogRootDirectory { get; set; }
        public string SystemParamRootDirectory { get; set; }
        public string DeviceParamRootDirectory { get; set; }
        public string DeviceName { get; set; }

        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = @"FileManager.json";

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }

        //private Element<int> _TestIntParam = new Element<int>();
        //public Element<int> TestIntParam
        //{
        //    get { return _TestIntParam; }
        //    set
        //    {
        //        if (value != _TestIntParam)
        //        {
        //            _TestIntParam = value;
        //        }
        //    }
        //}

        private Element<string> _ProberID = new Element<string>();
        public Element<string> ProberID
        {
            get { return _ProberID; }
            set
            {
                if (value != _ProberID)
                {
                    _ProberID = value;
                }
            }
        }

        private Element<bool> _PinAlignHighKeySaveOriginalImage = new Element<bool>();
        [DataMember]
        public Element<bool> PinAlignHighKeySaveOriginalImage
        {
            get { return _PinAlignHighKeySaveOriginalImage; }
            set
            {
                if (value != _PinAlignHighKeySaveOriginalImage)
                {
                    _PinAlignHighKeySaveOriginalImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PinAlignHighKeySavePassImage = new Element<bool>();
        [DataMember]
        public Element<bool> PinAlignHighKeySavePassImage
        {
            get { return _PinAlignHighKeySavePassImage; }
            set
            {
                if (value != _PinAlignHighKeySavePassImage)
                {
                    _PinAlignHighKeySavePassImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PinAlignHighKeySaveFailImage = new Element<bool>();
        [DataMember]
        public Element<bool> PinAlignHighKeySaveFailImage
        {
            get { return _PinAlignHighKeySaveFailImage; }
            set
            {
                if (value != _PinAlignHighKeySaveFailImage)
                {
                    _PinAlignHighKeySaveFailImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _SaveClipImage = new Element<bool>();
        [DataMember]
        public Element<bool> SaveClipImage
        {
            get { return _SaveClipImage; }
            set
            {
                if (value != _SaveClipImage)
                {
                    _SaveClipImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _AlwaysSaveClipImage = new Element<bool>();
        [DataMember]
        public Element<bool> AlwaysSaveClipImage
        {
            get { return _AlwaysSaveClipImage; }
            set
            {
                if (value != _AlwaysSaveClipImage)
                {
                    _AlwaysSaveClipImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ResizeRatioEnum> _ClipImageResizeRatio = new Element<ResizeRatioEnum>(ResizeRatioEnum.OneFourth);
        [DataMember]
        public Element<ResizeRatioEnum> ClipImageResizeRatio
        {
            get { return _ClipImageResizeRatio; }
            set
            {
                if (value != _ClipImageResizeRatio)
                {
                    _ClipImageResizeRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            try
            {
                if (this.SystemParamRootDirectory == null)
                {
                    this.SystemParamRootDirectory = DefaultSystemParamPath;
                }

                if (this.DeviceParamRootDirectory == null)
                {
                    this.DeviceParamRootDirectory = DefaultDeviceParamPath;
                }

                if (this.LogRootDirectory == null)
                {
                    this.LogRootDirectory = DefaultLogPath;
                }

                if (this.DeviceName == null)
                {
                    this.DeviceName = DefaultDeviceName;
                }

                if (ProberID.Value == null)
                {
                    ProberID.Value = "Undefined";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            SystemParamRootDirectory = @"C:\ProberSystem\Default\Parameters\SystemParam";
            DeviceParamRootDirectory = @"C:\ProberSystem\Default\Parameters\DeviceParam";
            LogRootDirectory = @"C:\Logs";
            DeviceName = "DEFAULTDEVNAME";

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            SetDefaultParam();

            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            try
            {
                PinAlignHighKeySaveOriginalImage.CategoryID = "00010018";
                PinAlignHighKeySaveOriginalImage.ElementName = "Pin align - high align key image save (Original)";
                PinAlignHighKeySaveOriginalImage.Description = "Related to image (Original) save during high key processing";

                PinAlignHighKeySavePassImage.CategoryID = "00010018";
                PinAlignHighKeySavePassImage.ElementName = "Pin align - high align key image save (Pass)";
                PinAlignHighKeySavePassImage.Description = "Related to image (Pass) save during high key processing";

                PinAlignHighKeySaveFailImage.CategoryID = "00010018";
                PinAlignHighKeySaveFailImage.ElementName = "Pin align - high align key image save (Fail)";
                PinAlignHighKeySaveFailImage.Description = "Related to image (Fail) save during high key processing";

                SaveClipImage.CategoryID = "00010018";
                SaveClipImage.ElementName = "Save Clip Image";
                SaveClipImage.Description = "When saving the fail image, a clip image is additionally saved.";

                AlwaysSaveClipImage.CategoryID = "00010018";
                AlwaysSaveClipImage.ElementName = "Always Save Clip Image";
                AlwaysSaveClipImage.Description = "If set to true, will save a clip image for all captured images that meet the specified criteria, regardless of the logtype(NORMAL, PASS, FAIL) value.";

                ClipImageResizeRatio.CategoryID = "00010018";
                ClipImageResizeRatio.ElementName = "Clip Image Resize Ratio";
                ClipImageResizeRatio.Description = "Allows the user to specify the resizing ratio of the clip image before saving, with options for 50%, 25%, or original size.";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
