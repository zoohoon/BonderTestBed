using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Autofac;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using System.IO.Compression;
using System.ServiceModel;
using SerializerUtil;
using System.Collections.ObjectModel;
using LogModule;
using FileUtil;

namespace FileSystem
{
    public enum FileType
    {
        LOG,
        IMAGE
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FileManager : IFileManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //private string AssemblyPath = System.Environment.CurrentDirectory;
        private string AssemblyPath = AppDomain.CurrentDomain.BaseDirectory;

        private readonly string collectPath;

        const string DefaultSystemParamPath = @"C:\ProberSystem\Default\Parameters\SystemParam";
        const string DefaultDeviceParamPath = @"C:\ProberSystem\Default\Parameters\DeviceParam";
        const string DefaultLogPath = @"C:\Logs";
        const string DefaultDeviceName = "DEFAULTDEVNAME";

        const string ParamFileName = @"FileManager.json";

        private readonly string LogCollectFolder = @"C:\LogsCollect";
        private readonly string GemLogFolder = @"C:\PROBERFILES\SpecialLog\XCom";
        private readonly string BackupFolder = @"C:\Logs\Backup";
        private readonly string SystemInfoFolder = @"C:\ProberSystem\SystemInfo";

        private string ParamPath = string.Empty;

        public string PultCommandString { get; set; }
        public string PultDeviceString { get; set; }
        public string PultSystemString { get; set; }
        public bool Initialized { get; set; } = false;

        public FileManagerParam FileManagerParam => FileManagerSysParam_IParam as FileManagerParam;

        public List<FileSavePath> FileSavePath = new List<FileSavePath>();
        private string ImageSaveBasePath { get; set; }

        //public FileManagerParam FileManagerParam { get; private set; }

        private IParam _FileManagerSysParam_IParam;
        public IParam FileManagerSysParam_IParam
        {
            get { return _FileManagerSysParam_IParam; }
            set
            {
                if (value != _FileManagerSysParam_IParam)
                {
                    _FileManagerSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _DevFolder;
        public string DevFolder
        {
            get { return _DevFolder; }
            private set
            {
                if (value != _DevFolder)
                {
                    _DevFolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FileManager()
        {
            collectPath = Environment.CurrentDirectory + @"\LogData";
        }

        public FileManager(string ParamPath)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.ParamPath = ParamPath;

                string FilePath = string.Empty;

                string DeviceParamRootDirectory = Path.Combine(ParamPath, "Parameters", "DeviceParam");
                string SystemParamRootDirectory = Path.Combine(ParamPath, "Parameters", "SystemParam");

                if (!string.IsNullOrEmpty(SystemParamRootDirectory))
                {
                    FilePath = SystemParamRootDirectory;
                }
                else
                {
                    FilePath = DefaultSystemParamPath;
                }

                FilePath = Path.Combine(FilePath, ParamFileName);

                if (Directory.Exists(Path.GetDirectoryName(FilePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                }

                IParam tmpParam = null;

                retval = this.LoadParamForUnitTest(ref tmpParam, typeof(FileManagerParam), null, FilePath);

                if (retval == EventCodeEnum.NONE)
                {
                    FileManagerSysParam_IParam = tmpParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadFileManagerParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                string FilePath = "";

                string[] CommandLineArgs = null;

                if (PultCommandString != string.Empty && PultCommandString != null)
                {
                    var v = this.PultCommandString;

                    if (v.ToLower().Contains("[path]"))
                    {
                        string[] splitString = v.Split(new string[] { "[path]", "[Path]", "[PATH]" }, StringSplitOptions.RemoveEmptyEntries);

                        if (0 < splitString.Length)
                        {
                            ParamPath = splitString[0];
                        }
                    }
                }
                else
                {
                    if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                    {
                        ParamPath = "C:\\ProberSystem\\LoaderSystem\\EMUL";
                    }
                    else
                    {
                        CommandLineArgs = Environment.GetCommandLineArgs();

                        //argument로부터 Parameter Path 설정부분이 있는지 체크.
                        foreach (var v in CommandLineArgs)
                        {
                            if (v.ToLower().Contains("[path]"))
                            {
                                string[] splitString = v.Split(new string[] { "[path]", "[Path]", "[PATH]" }, StringSplitOptions.RemoveEmptyEntries);

                                if (0 < splitString.Length)
                                {
                                    ParamPath = splitString[0];
                                }
                            }
                        }
                    }
                }

                string DeviceParamRootDirectory = string.Empty;
                string SystemParamRootDirectory = string.Empty;

                if (!string.IsNullOrEmpty(ParamPath))
                {
                    DeviceParamRootDirectory = Path.Combine(ParamPath, "Parameters", "DeviceParam");
                    SystemParamRootDirectory = Path.Combine(ParamPath, "Parameters", "SystemParam");

                    PultDeviceString = DeviceParamRootDirectory;
                    PultSystemString = SystemParamRootDirectory;
                }

                string[] devfolder = ParamPath.Split('\\');
                DevFolder = devfolder[devfolder.Count() - 1];

                if (!string.IsNullOrEmpty(SystemParamRootDirectory))
                {
                    FilePath = SystemParamRootDirectory;
                }
                else
                {
                    FilePath = DefaultSystemParamPath;
                }

                FilePath = Path.Combine(FilePath, ParamFileName);

                if (Directory.Exists(Path.GetDirectoryName(FilePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                }

                if (File.Exists(FilePath) == false)
                {
                    if (FileManagerParam == null)
                    {
                        FileManagerSysParam_IParam = new FileManagerParam();
                    }

                    RetVal = Extensions_IParam.SaveParameter(null, FileManagerParam, null, FilePath);
                }

                IParam tmpParam = null;

                RetVal = this.LoadParameter(ref tmpParam, typeof(FileManagerParam), null, FilePath);

                if (RetVal == EventCodeEnum.NONE)
                {
                    FileManagerSysParam_IParam = tmpParam;

                    // TODO : Test(Overwrite)
                    RetVal = Extensions_IParam.SaveParameter(null, FileManagerParam, null, FilePath);

                    //FileManagerParam = tmpParam as FileManagerParam;
                    LoggerManager.Debug($"[FileManager] Load Device Name: {FileManagerParam.DeviceName}, ProberID = {FileManagerParam.ProberID.Value}");
                }

                if (PultCommandString != string.Empty && PultCommandString != null)
                {
                    FileManagerParam.DeviceParamRootDirectory = PultDeviceString;
                    FileManagerParam.SystemParamRootDirectory = PultSystemString;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($"[FileManager] LoadParameter(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        public EventCodeEnum ChangeDevice(string DevName)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Directory.Exists($"{GetDeviceRootPath()}\\{DevName}"))
                {
                    //this.CardChangeModule().ReleaseWaitForCardPermission();

                    string FilePath = "";

                    FileManagerParam.DeviceName = DevName;
                    if (!string.IsNullOrEmpty(FileManagerParam.SystemParamRootDirectory))
                    {
                        FilePath = FileManagerParam.SystemParamRootDirectory;
                    }
                    else
                    {
                        FilePath = DefaultSystemParamPath;
                    }
                    FilePath = Path.Combine(FilePath, FileManagerParam.FileName);

                    try
                    {
                        RetVal = Extensions_IParam.SaveParameter(null, FileManagerParam, null, FilePath);
                        if (RetVal != EventCodeEnum.NONE)
                        {
                            throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile] Faile SaveParameter");
                        }
                        this.GEMModule().GetPIVContainer().SetDeviceName(DevName);
                    }
                    catch (Exception err)
                    {
                        RetVal = EventCodeEnum.PARAM_ERROR;
                        this.NotifyManager().Notify(EventCodeEnum.DEVICE_CHANGE_FAIL);
                        //Log.Error(String.Format("[FileManager] ChangeDevice(): Error occurred while changing device. Err = {0}", err.Message));
                        LoggerManager.Exception(err);

                        throw err;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.NOT_EXIST_DEVICE;
                    this.NotifyManager().Notify(EventCodeEnum.DEVICE_CHANGE_FAIL);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum DeleteDevice(string DevName)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                string FilePath = "";

                FilePath = Path.Combine(FileManagerParam.DeviceParamRootDirectory, DevName);

                LoggerManager.Debug($"File Path = {FilePath}");

                Directory.Delete(FilePath, true);
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum DeleteFilesInDirectory(string targetpath)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Directory.Exists(targetpath))
                {
                    // 디렉터리 내의 모든 파일 가져오기
                    string[] files = Directory.GetFiles(targetpath);

                    foreach (string file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            LoggerManager.Debug($"[{this.GetType().Name}] DeleteFilesInDirectory() : Deleted File Path = {file}");
                        }
                        catch (Exception ex)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}] DeleteFilesInDirectory() : Error Message = {ex.Message}");
                        }
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.DIRECTORY_PATH_ERROR;
                    LoggerManager.Debug($"[{this.GetType().Name}] DeleteFilesInDirectory() : {RetVal}, Path = {targetpath}");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public void InitData()
        {
            try
            {
                LoggerManager.Debug($"[FileManager], InitData() : SystemMode = {SystemManager.SysteMode}");

                ImageSaveBasePath = string.Empty;

                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    ImageSaveBasePath = $"C:\\Logs\\Image\\";
                }
                else
                {
                    string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
                    ImageSaveBasePath = LoggerManager.LoggerManagerParam.ImageLoggerParam.LogDirPath + $"\\{cellNo}\\Image\\";
                }

                FileSavePath.Clear();

                EnumProberModule[] modules = (EnumProberModule[])Enum.GetValues(typeof(EnumProberModule));

                foreach (var module in modules)
                {
                    string moduleBasePath = Path.Combine(ImageSaveBasePath, module.ToString());
                    FileSavePath fileSavePath = new FileSavePath(module, moduleBasePath);
                    FileSavePath.Add(fileSavePath);

                    if (!Directory.Exists(moduleBasePath))
                    {
                        Directory.CreateDirectory(moduleBasePath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    InitData();
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetLogRootPath()
        {
            return FileManagerParam.LogRootDirectory;
        }

        public string GetSystemRootPath()
        {
            return FileManagerParam.SystemParamRootDirectory;
        }

        public string GetDeviceRootPath()
        {
            return FileManagerParam.DeviceParamRootDirectory;
        }
        public string GetResultmapRootPath()
        {
            string rootpath = @"C:\ProberSystem\LoaderSystem\EMUL\Parameters\Resultmaps";
            return rootpath;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public string GetSystemParamFullPath(string parameterPath, string parameterName)
        {
            string retPath = FileManagerParam.SystemParamRootDirectory;
            try
            {

                retPath = Path.Combine(retPath, parameterPath, parameterName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retPath;
        }

        public string GetDeviceParamFullPath(string parameterPath = "", string parameterName = "", bool isNotContainDeviceName = true)
        {
            string retPath = FileManagerParam.DeviceParamRootDirectory;
            try
            {

                if (isNotContainDeviceName == true)
                {
                    retPath = Path.Combine(retPath, FileManagerParam.DeviceName);
                }

                char c = '\\';
                if (parameterPath != null)
                {
                    if (c.Equals(parameterPath.FirstOrDefault()))
                    {
                        retPath = retPath + parameterPath + parameterName;
                    }
                    else
                    {
                        retPath += c;
                        retPath = Path.Combine(retPath, parameterPath, parameterName);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retPath;
        }

        public string GetRootParamPath()
        {
            string retVal = FileManagerParam.SystemParamRootDirectory;
            try
            {
                string[] splitStrs = FileManagerParam.SystemParamRootDirectory.Split(new string[] { @"\Parameters\SystemParam" }, StringSplitOptions.RemoveEmptyEntries);

                if (0 < splitStrs.Length)
                {
                    retVal = splitStrs[0];
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public string GetDeviceName()
        {
            return FileManagerParam.DeviceName;
        }

        /// <summary>
        /// Device의 Save As 와 같은 역활. 생성된 Device 폴더를 압축해 리턴 (Remote 에서 사용 )
        /// </summary>
        /// <returns></returns>
        public byte[] GetSaveAsDeviceUsingName(string devName)
        {
            byte[] devicearr = new byte[0];
            try
            {
                IFileManager FileManager = this.FileManager();

                Autofac.IContainer Container = this.GetContainer();

                if (!string.IsNullOrEmpty(devName))
                {
                    string tmporigindevname = FileManager.GetDeviceName();

                    FileManager.ChangeDevice(devName);

                    var modules = Container.Resolve<IEnumerable<IFactoryModule>>().Where(module => module is IHasDevParameterizable);

                    foreach (var v in modules)
                    {
                        try
                        {
                            IHasDevParameterizable module = v as IHasDevParameterizable;
                            module.SaveDevParameter();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);

                            throw new Exception($"Error Module is {v}");
                        }
                    }

                    devicearr = this.CompressFolderToByteArray(GetDeviceParamFullPath());

                    File.Delete(GetDeviceRootPath());
                    Directory.Delete(GetDeviceRootPath());
                    FileManager.ChangeDevice(tmporigindevname);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return devicearr;
        }

        public byte[] GetNewDeviceFolderUsingName(string devName)
        {
            byte[] devarr = new byte[0];
            string tmporigindevname = GetDeviceName();
            try
            {

                ChangeDevice(devName);
                var modules = this.GetContainer().Resolve<IEnumerable<IFactoryModule>>().Where(module => module is IHasDevParameterizable);
                foreach (var v in modules)
                {
                    EventCodeEnum loadDevResult = EventCodeEnum.UNDEFINED;
                    try
                    {
                        IHasDevParameterizable module = v as IHasDevParameterizable;
                        loadDevResult = module.LoadDevParameter();
                        loadDevResult = module.InitDevParameter();

                        if (module is IStageSupervisor)
                        {
                            (module as IStageSupervisor)?.SetWaferObjectStatus();
                        }

                        devarr = this.CompressFolderToByteArray(GetDeviceRootPath());

                        File.Delete(GetDeviceRootPath());
                        Directory.Delete(GetDeviceRootPath());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"Error Module is {v}");
                        LoggerManager.Exception(err);

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                ChangeDevice(tmporigindevname);
            }
            return devarr;
        }

        public byte[] GetFileManagerParam()
        {

            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(FileManagerParam, typeof(FileManagerParam));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;

        }

        public ObservableCollection<SimpleDeviceInfo> GetDevicelistTest()
        {
            ObservableCollection<SimpleDeviceInfo> DeviceInfoCollection = new ObservableCollection<SimpleDeviceInfo>();

            try
            {
                var directories = Directory.GetDirectories(FileManagerParam.DeviceParamRootDirectory);

                foreach (var directory in directories)
                {
                    var directoryNameSplit = directory.Split('\\');
                    SimpleDeviceInfo devInfo = new SimpleDeviceInfo();
                    devInfo.Name = directoryNameSplit[directoryNameSplit.Length - 1];
                    DeviceInfoCollection.Add(devInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return DeviceInfoCollection;
        }

        public ObservableCollection<string> GetDeviceNamelist()
        {
            ObservableCollection<string> devicenamelist = new ObservableCollection<string>();

            try
            {
                var directories = Directory.GetDirectories(FileManagerParam.DeviceParamRootDirectory);

                foreach (var directory in directories)
                {
                    var directoryNameSplit = directory.Split('\\');
                    devicenamelist.Add(directoryNameSplit[directoryNameSplit.Length - 1]);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return devicenamelist;
        }

        public ObservableCollection<DeviceInfo> GetDevicelist()
        {
            ObservableCollection<DeviceInfo> DeviceInfoCollection = new ObservableCollection<DeviceInfo>();

            try
            {
                var directories = Directory.GetDirectories(FileManagerParam.DeviceParamRootDirectory);

                foreach (var directory in directories)
                {
                    var directoryNameSplit = directory.Split('\\');
                    DeviceInfo devInfo = new DeviceInfo();
                    devInfo.Name = directoryNameSplit[directoryNameSplit.Length - 1];
                    DeviceInfoCollection.Add(devInfo);

                    //if (devInfo.Name == FileManager.GetDeviceName())
                    //{
                    //    devInfo.IsNowDevice = true;
                    //}
                    //else
                    //{
                    //    devInfo.IsNowDevice = false;
                    //}
                }

                //ShowingDeviceInfoCollection = DeviceInfoCollection;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return DeviceInfoCollection;
        }

        public byte[] GetDevicebytelist()
        {
            byte[] compressedData = null;

            try
            {
                AsyncObservableCollection<DeviceInfo> DeviceInfoCollection = new AsyncObservableCollection<DeviceInfo>();

                DeviceInfoCollection = GetDevicelist() as AsyncObservableCollection<DeviceInfo>;

                var bytes = SerializeManager.SerializeToByte(DeviceInfoCollection, typeof(AsyncObservableCollection<DeviceInfo>));

                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public byte[] GetDeviceByFileName(string filename)
        {
            byte[] device = new byte[0];

            try
            {
                string devcierootpath = GetDeviceRootPath();

                string fullpath = devcierootpath + "\\" + filename;
                string zippath = fullpath + ".zip";

                DirectoryInfo directory = new DirectoryInfo(fullpath);

                if (!directory.Exists)
                    return null;

                string extractPath = directory.FullName;

                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    extractPath += Path.DirectorySeparatorChar;
                }

                if (!File.Exists(zippath))
                {
                    ZipFile.CreateFromDirectory(fullpath, zippath);
                }

                device = File.ReadAllBytes(zippath);

                File.Delete(zippath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return device;
        }
        public bool SetDeviceByFileName(byte[] device, string devicename)
        {
            bool retVal = false;

            try
            {
                string devcierootpath = GetDeviceRootPath();
                string fullpath = devcierootpath + "\\" + devicename;
                string zippath = fullpath + ".zip";

                File.WriteAllBytes(zippath, device);

                if (Directory.Exists(fullpath))
                {
                    Directory.Delete(fullpath, true);
                }

                ZipFile.ExtractToDirectory(zippath, fullpath);
                File.Delete(zippath);

                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public byte[] CompressFileToStream(string filePath, int count)
        {
            byte[] byteArray = null;
            byte[] retArray = null;

            try
            {
                if (filePath != null)
                {
                    if (File.Exists(filePath))
                    {
                        LoggerManager.Debug($"[FileManager] CompressFileToStream() : Start, Path = {filePath}");

                        var bytes = File.ReadAllBytes(filePath);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Position = 0;

                            byte[] buffer = new byte[count];

                            using (MemoryStream ms = new MemoryStream())
                            {
                                int read;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }
                                byteArray = ms.ToArray();
                            }

                            retArray = Compress(byteArray);
                        }

                        LoggerManager.Debug($"[FileManager] CompressFileToStream() : End, Path = {filePath}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retArray;
        }

        public byte[] Compress(byte[] data)
        {
            byte[] retVal = null;

            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
                {
                    dstream.Write(data, 0, data.Length);
                }
                retVal = output.ToArray();
            }

            return retVal;
        }

        public byte[] Decompress(byte[] data)
        {
            byte[] retVal = null;
            try
            {
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                        {
                            dstream.CopyTo(output);
                        }
                        retVal = output.ToArray();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoadFileManagerParam();

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"LoadFileManagerParam() Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.SaveParameter(FileManagerSysParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DecompressFilesFromByteArray(byte[] param, string filepath)
        {
            try
            {
                byte[] retbytes = null;
                retbytes = Decompress(param);
                using (Stream stream = new MemoryStream(retbytes))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                        File.WriteAllText(filepath, content);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetModulePath(EnumProberModule moduletype)
        {
            string retval = string.Empty;

            try
            {
                if (FileSavePath != null && FileSavePath.Count > 0)
                {
                    var obj = FileSavePath.FirstOrDefault(t => t.ModuleType == moduletype);

                    if (obj != null)
                    {
                        retval = obj.Path;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string CombinePath(params string[] paths)
        {
            try
            {
                return Path.Combine(paths.Select(p => p.Trim('\\')).ToArray());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return string.Empty;
            }
        }

        public string GetImageSavePath(EnumProberModule moduletype, bool appendTrailingSlash = false, params string[] paths)
        {
            string retval = string.Empty;

            try
            {
                string path = GetModulePath(moduletype);

                retval = CombinePath(new[] { path }.Concat(paths).ToArray());

                if (appendTrailingSlash && !retval.EndsWith("\\"))
                {
                    retval += "\\";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public string GetImageSaveFullPath(EnumProberModule moduletype, IMAGE_SAVE_TYPE type, bool AppendCurrentTime, params string[] paths)
        {
            string retval = string.Empty;

            try
            {
                retval = GetImageSavePath(moduletype, false, paths);

                if (AppendCurrentTime)
                {
                    string dt = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    string fileName = Path.GetFileName(retval);
                    string newFileName = $"{dt}_{fileName}";
                    retval = Path.Combine(Path.GetDirectoryName(retval), newFileName);
                }

                switch (type)
                {
                    case IMAGE_SAVE_TYPE.BMP:
                        retval = Path.ChangeExtension(retval, ".bmp");
                        break;
                    case IMAGE_SAVE_TYPE.JPEG:
                        retval = Path.ChangeExtension(retval, ".jpeg");
                        break;
                    case IMAGE_SAVE_TYPE.PNG:
                        retval = Path.ChangeExtension(retval, ".png");
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string GetRelativePath(string relativeTo, string path)
        {
            string retval = string.Empty;
            try
            {
                var uri = new Uri(relativeTo);
                retval = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                if (retval.Contains(Path.DirectorySeparatorChar.ToString()) == false)
                {
                    retval = $".{ Path.DirectorySeparatorChar }{ retval }";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetClipImagePath(string path, out string clippath)
        {
            bool retval = false;
            clippath = string.Empty;

            try
            {
                foreach (var fileSavePath in FileSavePath)
                {
                    if (path.Contains(fileSavePath.Path))
                    {
                        var module = fileSavePath.ModuleType;
                        retval = true;
                        break;
                    }
                }

                if (retval)
                {
                    string relativePath = GetRelativePath(ImageSaveBasePath, path);
                    clippath = Path.Combine(ImageSaveBasePath, "CLIP", relativePath);

                    // 확장자 변경
                    clippath = Path.ChangeExtension(clippath, ".jpg");

                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string GetProberID()
        {
            return FileManagerParam?.ProberID?.Value ?? "";
        }

        public void SetProberID(string proberid)
        {
            try
            {
                if (FileManagerParam != null)
                {
                    FileManagerParam.ProberID.Value = proberid;
                    this.GEMModule().GetPIVContainer().ProberType.Value = FileManagerParam.ProberID.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetCompressedFile(DateTime startdate, DateTime enddate, List<EnumLoggerType> logtypes, List<EnumProberModule> imagetypes, bool includeGEM = false, bool includeClip = false, bool includeLoadedDevice = false, bool inlcudeSystemparam = false, bool inlcudebackupinfo = false, bool inlcudeSysteminfo = false)
        {
            byte[] retval = null;

            try
            {
                long log_Size = 0;
                long Image_Size = 0;
                long Device_DirSize = 0;
                long System_DirSize = 0;
                long BackupInfo_DirSize = 0;
                long SystemInfo_DirSize = 0;

                List<string> Logfiles = new List<string>();
                List<string> Imagefiles = new List<string>();

                List<FileInfo> fileinfos = new List<FileInfo>();

                FileUtility.DeleteDirectory(collectPath);

                // Log
                foreach (var log in logtypes)
                {
                    var dir = LoggerManager.GetLogDirPath(log);

                    if (string.IsNullOrEmpty(dir) == false)
                    {
                        var files = FileUtility.GetFilteredFilesByCreationTime(startdate, enddate, dir);

                        if (files != null && files.Count > 0)
                        {
                            foreach (var file in files)
                            {
                                Logfiles.Add(file);
                            }
                        }
                    }
                }

                // GEM log
                if (includeGEM)
                {
                    var files = FileUtility.GetFilteredFilesByCreationTime(startdate, enddate, GemLogFolder);

                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            Logfiles.Add(file);
                        }
                    }
                }

                // Image
                foreach (var image in imagetypes)
                {
                    var dir = GetModulePath(image);

                    if (string.IsNullOrEmpty(dir) == false)
                    {
                        var files = FileUtility.GetFilteredFilesByCreationTime(startdate, enddate, dir);

                        if (files != null && files.Count > 0)
                        {
                            foreach (var file in files)
                            {
                                Imagefiles.Add(file);
                            }
                        }
                    }
                }

                if (Logfiles.Count > 0)
                {
                    foreach (var log in Logfiles)
                    {
                        var fileinfo = new FileInfo(log);
                        fileinfos.Add(fileinfo);

                        log_Size += fileinfo.Length;
                    }
                }

                if (Imagefiles.Count > 0)
                {
                    foreach (var image in Imagefiles)
                    {
                        var fileinfo = new FileInfo(image);
                        fileinfos.Add(fileinfo);

                        Image_Size += fileinfo.Length;
                    }
                }


                if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    string cellPath = $"C{this.LoaderController().GetChuckIndex():D2}";

                    if (includeLoadedDevice)
                    {
                        string deviceDirPath = this.FileManager().GetDeviceRootPath() + "\\" + this.FileManager().GetDeviceName();
                        Device_DirSize = FileUtility.GetDirectorySize(deviceDirPath);
                    }

                    if (inlcudeSystemparam)
                    {
                        string systemDirPath = this.FileManager().GetSystemRootPath();
                        System_DirSize = FileUtility.GetDirectorySize(systemDirPath);

                    }

                    if (inlcudebackupinfo)
                    {
                        string backupDirPath = BackupFolder + '\\' + $"C{this.LoaderController().GetChuckIndex():D2}";
                        BackupInfo_DirSize = FileUtility.GetDirectorySize(backupDirPath);
                    }

                    if (inlcudeSysteminfo)
                    {
                        string systeminfoDirPath = SystemInfoFolder + '\\' + $"C{this.LoaderController().GetChuckIndex():D2}";
                        SystemInfo_DirSize = FileUtility.GetDirectorySize(systeminfoDirPath);
                    }

                }
                else if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (includeLoadedDevice)
                    {
                        // NOTHING
                    }

                    if (inlcudeSystemparam)
                    {
                        string systemDirPath = this.FileManager().GetSystemRootPath();
                        System_DirSize = FileUtility.GetDirectorySize(systemDirPath);
                    }

                    if (inlcudebackupinfo)
                    {
                        string backupDirPath = BackupFolder;
                        BackupInfo_DirSize = FileUtility.GetDirectorySize(backupDirPath);
                    }

                    if (inlcudeSysteminfo)
                    {
                        string systeminfoDirPath = SystemInfoFolder;
                        SystemInfo_DirSize = FileUtility.GetDirectorySize(systeminfoDirPath);
                    }
                }

                long totalSize = log_Size + Image_Size + Device_DirSize + System_DirSize + BackupInfo_DirSize + SystemInfo_DirSize;

                int totalfileSize = Convert.ToInt32(totalSize / 1024 / 1024);
                int freeSize = FileUtility.GetDiskFreeSize("C");
                int bufferSize = 100;

                LoggerManager.Debug($"[{this.GetType().Name}], GetCompressedFile() : Log Size : {totalfileSize} MB, Free Space : {freeSize} MB");

                if (totalfileSize > (freeSize + bufferSize))
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], GetCompressedFile() : Insufficient disk space.");
                }
                else
                {
                    if (fileinfos.Count > 0)
                    {
                        List<string> dsts = new List<string>();

                        foreach (var logFileInfo in fileinfos)
                        {
                            var dstFolder = collectPath + '\\' + logFileInfo.DirectoryName.Substring(3);
                            var dst = dstFolder + '\\' + logFileInfo.Name;

                            FileUtility.CreateDirectory(dstFolder);
                            FileUtility.FileCopy(logFileInfo.FullName, dst);
                            dsts.Add(dst);
                        }
                    }

                    if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                    {
                        string cellPath = $"C{this.LoaderController().GetChuckIndex():D2}";

                        if (includeLoadedDevice)
                        {
                            string deviceDirPath = this.FileManager().GetDeviceRootPath() + "\\" + this.FileManager().GetDeviceName();
                            string dstFolder = collectPath + '\\' + $"ProberSystem\\{cellPath}\\Parameters\\DeviceParam\\" + this.FileManager().GetDeviceName();

                            FileUtility.DirectoryCopy(deviceDirPath, dstFolder, true);
                        }

                        if (inlcudeSystemparam)
                        {
                            string systemDirPath = this.FileManager().GetSystemRootPath();
                            string dstFolder = collectPath + '\\' + $"ProberSystem\\{cellPath}\\Parameters\\SystemParam";

                            FileUtility.DirectoryCopy(systemDirPath, dstFolder, true);
                        }

                        if (inlcudebackupinfo)
                        {
                            string backupDirPath = BackupFolder + '\\' + $"C{this.LoaderController().GetChuckIndex():D2}";
                            string dstFolder = collectPath + '\\' + $"Logs\\Backup\\C{ this.LoaderController().GetChuckIndex():D2}";

                            FileUtility.DirectoryCopy(backupDirPath, dstFolder, true);
                        }

                        if (inlcudeSysteminfo)
                        {
                            string systeminfoDirPath = SystemInfoFolder + '\\' + $"C{this.LoaderController().GetChuckIndex():D2}";
                            string dstFolder = collectPath + '\\' + $"ProberSystem\\SystemInfo\\C{ this.LoaderController().GetChuckIndex():D2}";

                            FileUtility.DirectoryCopy(systeminfoDirPath, dstFolder, true);
                        }
                    }
                    else if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                    {
                        if (includeLoadedDevice)
                        {
                            // NOTHING
                        }

                        if (inlcudeSystemparam)
                        {
                            string systemDirPath = this.FileManager().GetSystemRootPath();
                            string dstFolder = collectPath + '\\' + $"ProberSystem\\LoaderSystem\\EMUL\\Parameters\\SystemParam";

                            FileUtility.DirectoryCopy(systemDirPath, dstFolder, true);
                        }

                        if (inlcudebackupinfo)
                        {
                            string backupDirPath = BackupFolder;
                            string dstFolder = collectPath + '\\' + $"Logs\\Backup";

                            FileUtility.DirectoryCopy(backupDirPath, dstFolder, false);
                        }

                        if (inlcudeSysteminfo)
                        {
                            string systeminfoDirPath = SystemInfoFolder;
                            string dstFolder = collectPath + '\\' + $"ProberSystem\\SystemInfo";

                            FileUtility.DirectoryCopy(systeminfoDirPath, dstFolder, false);
                        }
                    }

                    var outputPath = $"{LogCollectFolder}\\Temp.zip";

                    if (!Directory.Exists(LogCollectFolder))
                    {
                        Directory.CreateDirectory(LogCollectFolder);
                    }

                    if (FileUtility.ZipDirectory(collectPath, outputPath))
                    {
                        retval = File.ReadAllBytes(outputPath);

                        LoggerManager.Debug($"[{this.GetType().Name}], GetCompressedFile() : retval length = {retval.Length}");

                        if (File.Exists(outputPath))
                        {
                            File.Delete(outputPath);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DecompressFile(byte[] compressedFileData, string outputFilePath)
        {
            try
            {
                if (compressedFileData == null || compressedFileData.Length == 0)
                {
                    LoggerManager.Error($"[{this.GetType().Name}], DecompressFile() : The input data is null or empty");
                    return;
                }

                using (var compressedStream = new MemoryStream(compressedFileData))
                {
                    using (var zipArchive = new ZipArchive(compressedStream, ZipArchiveMode.Read))
                    {
                        if (zipArchive.Entries.Count == 0)
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], DecompressFile() : The input data does not contain any files");
                            return;
                        }

                        var entry = zipArchive.Entries[0];
                        entry.ExtractToFile(outputFilePath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private byte[] CombineByteArrays(List<byte[]> byteArrays)
        {
            byte[] retval = null;

            try
            {
                int totalLength = byteArrays.Sum(x => x.Length);

                retval = new byte[totalLength];

                int currentPosition = 0;

                foreach (byte[] byteArray in byteArrays)
                {
                    Buffer.BlockCopy(byteArray, 0, retval, currentPosition, byteArray.Length);
                    currentPosition += byteArray.Length;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public long GetFreeSpaceFromDrive(string filePath)
        {
            long freeSpace = -1;
            string driveType = "";

            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    driveType = Path.GetPathRoot(filePath);
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady && drive.Name.Contains(driveType))
                        {
                            freeSpace = drive.TotalFreeSpace;
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return freeSpace;
        }

        public long GetFileSize(string filePath)
        {
            long fileSize = -1;
            try
            {
                if (File.Exists(filePath) == false)
                {
                    // file 자체가 없는건 0kb가 아니고 파일자체가 없어서 Default로 만드는 경우는 Save되서 만들어 져야 함.
                    fileSize = 0;
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    fileSize = fileInfo.Length; // 파일의 크기를 바이트 단위로 반환
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return fileSize;
        }

        //private bool CollectLog(DateTime startdate, DateTime enddate, bool includeGemLog, bool includeImageLog)
        //{
        //    try
        //    {
        //        // TODO : 해당 경로 하위에 있는 모든 파일을 

        //        // Collect Log
        //        //var logFiles = FileUtility.DirFileSearchPeriod(startdate, enddate, logFolder);

        //        // TODO : Collect Gem Log
        //        //if (includeGemLog)
        //        //{
        //        //    logFiles.AddRange(GetLogFileList(unSelectCell, GemLogFolder, includeImageLog));
        //        //}

        //        //double totalSize = 0;

        //        //foreach (var logFile in logFiles)
        //        //{
        //        //    var file = new FileInfo(logFile);
        //        //    totalSize += file.Length;
        //        //}

        //        //int totalLogSize = Convert.ToInt32(totalSize / 1024 / 1024);
        //        //int freeSize = FileUtility.GetDiskFreeSize("C");
        //        //int bufferSize = 100;

        //        //if (totalLogSize > (freeSize + bufferSize))
        //        //{
        //        //    return false;
        //        //}

        //        //foreach (var logFile in logFiles)
        //        //{
        //        //    var file = new FileInfo(logFile);
        //        //    var dstFolder = collectPath + '\\' + file.DirectoryName.Substring(3);

        //        //    FileUtility.CreateDirectory(dstFolder);
        //        //    FileUtility.FileCopy(file.FullName, dstFolder + '\\' + file.Name);
        //        //}
        //    }
        //    catch (Exception err )
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return true;
        //}
    }
}
