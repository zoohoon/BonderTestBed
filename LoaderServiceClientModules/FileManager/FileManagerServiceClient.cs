using System;

namespace LoaderServiceClientModules
{
    using Autofac;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using LogModule;
    using ProberInterfaces.Template;
    using SerializerUtil;
    using ProberInterfaces.Proxies;
    using System.Collections.Generic;

    public class FileManagerServiceClient : IFileManager, ILoaderFileManager, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }
        public EnumCommunicationState CommunicationState
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    return EnumCommunicationState.CONNECTED;
                }
                else
                {
                    return EnumCommunicationState.UNAVAILABLE;
                }
            }
            set { }
        }

        public TemplateStateCollection Template { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITemplateFileParam TemplateParameter => throw new NotImplementedException();

        public ITemplateParam LoadTemplateParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubRoutine SubRoutine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public string DevFolder => throw new NotImplementedException();

        public string PultCommandString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte[] GetFileManagerParam()
        {
            byte[] retval = null;

            IFileManagerProxy proxy = LoaderCommunicationManager.GetProxy<IFileManagerProxy>();

            if (proxy != null)
            {
                retval = proxy.GetFileManagerParam();
            }

            return retval;
        }

        public bool SetDeviceByFileName(byte[] device, string devicename)
        {
            bool retval = false;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().SetDeviceByFileName(device, devicename);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public byte[] GetDeviceByFileName(string filename)
        {
            byte[] retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDeviceByFileName(filename);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<string> GetDeviceNamelist()
        {
            ObservableCollection<string> devicenamelist = null;

            try
            {
                devicenamelist = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDeviceNamelist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return devicenamelist;
        }

        public byte[] GetDevicebytelist()
        {
            byte[] retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDevicebytelist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<SimpleDeviceInfo> GetDevicelistTest()
        {
            ObservableCollection<SimpleDeviceInfo> retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDevicelistTest();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<DeviceInfo> GetDevicelist()
        {
            ObservableCollection<DeviceInfo> retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDevicelist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private FileManagerParam _FileManagerParam;
        public FileManagerParam FileManagerParam
        {
            get
            {
                _FileManagerParam = FMParam();
                return _FileManagerParam;
            }
        }

        public FileManagerParam FMParam()
        {
            byte[] obj = GetFileManagerParam();
            object target = null;

            FileManagerParam retval = null;

            if (obj != null)
            {
                var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(FileManagerParam));
                retval = target as FileManagerParam;
            }

            return retval;
        }

        public EventCodeEnum SelectIntervalWafer()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum UnLoadPolishWafer()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadPolishWafer(string definetype)
        {
            throw new NotImplementedException();
        }

        public bool IsReadyPolishWafer()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum PolishWaferValidate(bool isExist)
        {
            throw new NotImplementedException();
        }

        public void SetDevParam(byte[] param)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

        public string GetLogRootPath()
        {
            throw new NotImplementedException();
        }

        public string GetSystemRootPath()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceRootPath()
        {
            throw new NotImplementedException();
        }

        public string GetRootParamPath()
        {
            throw new NotImplementedException();
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public string GetSystemParamFullPath(string parameterPath, string parameterName)
        {
            return LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetSystemParamFullPath(parameterPath, parameterName);
        }

        public string GetDeviceParamFullPath(string parameterPath = "", string parameterName = "", bool isContainDeviceName = true)
        {
            return LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDeviceParamFullPath(parameterPath, parameterName, isContainDeviceName);
        }

        public string GetDeviceName()
        {
            return LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetDeviceName();
        }

        public EventCodeEnum ChangeDevice(string DevName)
        {
            return LoaderCommunicationManager.GetProxy<IFileManagerProxy>().ChangeDevice(DevName);
        }

        public EventCodeEnum DeleteDevice(string DevName)
        {
            return LoaderCommunicationManager.GetProxy<IFileManagerProxy>().DeleteDevice(DevName);
        }
        public EventCodeEnum DeleteFilesInDirectory(string targetpath)
        {
            //Not Exist Loader Call
            throw new NotImplementedException();
        }

        public byte[] GetNewDeviceFolderUsingName(string devName)
        {
            throw new NotImplementedException();
        }

        public byte[] GetSaveAsDeviceUsingName(string devName)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public byte[] CompressFileToStream(string filePath, int count)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void DecompressFilesFromByteArray(byte[] param, string filepath)
        {
            throw new NotImplementedException();
        }

        public string GetImageSaveFullPath(EnumProberModule moduletype, IMAGE_SAVE_TYPE type, bool AppendCurrentTime, params string[] paths)
        {
            throw new NotImplementedException();
        }

        public string GetProberID()
        {
            return "";
        }

        public void SetProberID(string proberid)
        { }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }
        public void InitData()
        {

        }

        public void SetParamPath(string parampath)
        {
            throw new NotImplementedException();
        }

        public bool GetClipImagePath(string path, out string clippath)
        {
            throw new NotImplementedException();
        }

        public string GetImageSavePath(EnumProberModule moduletype, bool appendTrailingSlash = false, params string[] paths)
        {
            throw new NotImplementedException();
        }
   
        public void DecompressFile(byte[] compressedFileData, string outputFilePath)
        {
            return;
        }

        public byte[] GetCompressedFile(DateTime startdate, DateTime enddate, List<EnumLoggerType> logtypes, List<EnumProberModule> imagetypes, bool includeGEM = false, bool includeClip = false, bool includeLoadedDevice = false, bool inlcudeSystemparam = false, bool inlcudebackupinfo = false, bool inlcudeSysteminfo = false)
        {
            byte[] retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IFileManagerProxy>().GetCompressedFile(startdate, enddate, logtypes, imagetypes, includeGEM, includeClip, includeLoadedDevice, inlcudeSystemparam, inlcudebackupinfo, inlcudeSysteminfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public long GetFreeSpaceFromDrive(string filePath)
        {
            return -1;
        }

        public long GetFileSize(string filePath)
        {
            return -1;
        }
    }
}
