using System;
using System.Linq;
using System.Threading.Tasks;

namespace LogSettingVM
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Windows.Input;

    public class LogSettingViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("30be3fa8-07b7-4724-b029-1c5a3ea50dd5");

        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

        private bool _EnableDays = new bool();
        public bool EnableDays
        {
            get { return _EnableDays; }
            set
            {
                if (value != _EnableDays)
                {
                    _EnableDays = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private int _KeepDays;
        public int KeepDays
        {
            get { return _KeepDays; }
            set
            {
                if (value != _KeepDays)
                {
                    _KeepDays = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private bool _EnableSize = new bool();
        public bool EnableSize
        {
            get { return _EnableSize; }
            set
            {
                if (value != _EnableSize)
                {
                    _EnableSize = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private long _MaxLogSize;
        public long MaxLogSize
        {
            get { return _MaxLogSize; }
            set
            {
                if (value != _MaxLogSize)
                {
                    _MaxLogSize = value;
                    RaisePropertyChanged("");
                }
            }
        }


        private bool _EnableZipfile = new bool();
        public bool EnableZipfile
        {
            get { return _EnableZipfile; }
            set
            {
                if (value != _EnableZipfile)
                {
                    _EnableZipfile = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private string _FolderPath;
        public string FolderPath
        {
            get { return _FolderPath; }
            set
            {
                if (value != _FolderPath)
                {
                    _FolderPath = value;
                    RaisePropertyChanged("");
                }
            }
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                EnableDays = true;
                EnableSize = false;
                EnableZipfile = true;
                KeepDays = 90;
                MaxLogSize = 90;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        //public EventCodeEnum RollBackParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //    //retVal = GPIB.SaveSysParameter();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //         throw;
        //    }
        //    return retVal;
        //}

        //public EventCodeEnum SaveParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //    //retVal = GPIB.SaveSysParameter();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //         throw;
        //    }
        //    return retVal;
        //}

        public EventCodeEnum UpProc()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            return EventCodeEnum.NONE;
        }

        private RelayCommand _FolderSelectCommand;
        public ICommand FolderSelectCommand
        {
            get
            {
                if (null == _FolderSelectCommand) _FolderSelectCommand = new RelayCommand(FolderSelectCommandFunc);
                return _FolderSelectCommand;
            }
        }

        private void FolderSelectCommandFunc()
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.ShowDialog();
                FolderPath = dialog.SelectedPath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AutoDeleteTestCommand;
        public ICommand AutoDeleteTestCommand
        {
            get
            {
                if (null == _AutoDeleteTestCommand) _AutoDeleteTestCommand = new RelayCommand(AutoDeleteTestCommandFunc);
                return _AutoDeleteTestCommand;
            }
        }

        private void AutoDeleteTestCommandFunc()
        {
            try
            {
                DirectoryInfo logFolder = new DirectoryInfo(FolderPath);

                foreach (FileInfo file in logFolder.GetFiles())
                {
                    if (file.Extension != ".txt")
                        continue;
                    if (file.CreationTime < DateTime.Now.AddDays(-(KeepDays)))
                        file.Delete();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AutoDeleteMaxSizeTestCommand;
        public ICommand AutoDeleteMaxSizeTestCommand
        {
            get
            {
                if (null == _AutoDeleteMaxSizeTestCommand) _AutoDeleteMaxSizeTestCommand = new RelayCommand(AutoDeleteMaxSizeTestCommandFunc);
                return _AutoDeleteMaxSizeTestCommand;
            }
        }

        private void AutoDeleteMaxSizeTestCommandFunc()
        {
            try
            {
                if (FolderPath == null)
                    return;

                DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);

                FileSystemInfo[] fileSystemInfoArray = directoryInfo.GetFileSystemInfos();
                long directorySize = 0L;

                for (int i = 0; i < fileSystemInfoArray.Length; i++)
                {
                    FileInfo fileInfo = fileSystemInfoArray[i] as FileInfo;
                    if (fileInfo != null)
                    {
                        directorySize += fileInfo.Length;
                    }
                }

                double size = Math.Round((directorySize / 1024f) / 1024f);

                if (size >= MaxLogSize)
                {
                    // Remove file       
                    // 가장 오래된 10개 목록 추려냄
                    FileSystemInfo[] last_file = fileSystemInfoArray.OrderBy(fi => fi.LastWriteTime).Take(10).ToArray();
                    foreach (FileSystemInfo item in last_file)
                    {
                        File.Delete(item.FullName);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
            }
        }

        private RelayCommand _AutoBackupTestCommand;

        public LogSettingViewModel()
        {
        }

        public ICommand AutoBackupTestCommand
        {
            get
            {
                if (null == _AutoBackupTestCommand) _AutoBackupTestCommand = new RelayCommand(AutoBackupTestCommandFunc);
                return _AutoBackupTestCommand;
            }
        }

        private void AutoBackupTestCommandFunc()
        {
            try
            {
                string zipFileName = new DirectoryInfo(FolderPath).Name;
                string zipFilePath = FolderPath.Substring(0, FolderPath.LastIndexOf(("\\"))) + "\\" + zipFileName + ".zip";
                string backupFolder = FolderPath;
                using (FileStream fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        foreach (string filePath in Directory.EnumerateFiles(backupFolder, "*.*", SearchOption.AllDirectories))
                        {
                            string relativePath = filePath.Substring(backupFolder.Length + 1);

                            try
                            {
                                zipArchive.CreateEntryFromFile(filePath, relativePath);
                            }
                            catch (PathTooLongException)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        //public EventCodeEnum CheckParameterToSave()
        //{
        //    return EventCodeEnum.NONE;
        //}
    }
}
