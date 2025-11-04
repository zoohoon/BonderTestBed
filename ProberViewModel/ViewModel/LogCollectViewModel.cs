using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Proxies;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProberViewModel.ViewModel
{
    public class CellInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }


        public CellInfo(int index, string cellName, bool isSelected, bool isEnabled)
        {
            Index = index;
            Name = cellName;
            IsSelected = isSelected;
            IsEnabled = isEnabled;
        }
    }
    public class LogChecker : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }
        public LogChecker(string name, bool IsSelected)
        {
            this.Name = name;
            this.IsSelected = IsSelected;
        }
    }
    public class ImageChecker : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ImageChecker(string name, bool IsSelected)
        {
            this.Name = name;
            this.IsSelected = IsSelected;
        }
    }
    public class LogCollectViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private readonly string LogCollectFolder = @"C:\LogsCollect";
        private readonly Guid _ViewModelGUID = new Guid("958f928a-ac5c-44ec-a718-72d36b3374ce");

        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;

        public ILoaderCommunicationManager _LoaderCommunicationManager = null;
        public ILoaderSupervisor _LoaderSupervisor = null;

        private DateTime startDate;
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
                RaisePropertyChanged();
            }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                endDate = value;
                RaisePropertyChanged();
            }
        }

        private string outputFilePath;
        public string OutputFilePath
        {
            get
            {
                return outputFilePath;
            }
            set
            {
                outputFilePath = value;
                RaisePropertyChanged();
            }
        }

        private bool _IncludeLoader;
        public bool IncludeLoader
        {
            get { return _IncludeLoader; }
            set
            {
                if (value != _IncludeLoader)
                {
                    _IncludeLoader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCellAllChecked;
        public bool IsCellAllChecked
        {
            get { return _IsCellAllChecked; }
            set
            {
                if (value != _IsCellAllChecked)
                {
                    _IsCellAllChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLogAllChecked;
        public bool IsLogAllChecked
        {
            get { return _IsLogAllChecked; }
            set
            {
                if (value != _IsLogAllChecked)
                {
                    _IsLogAllChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsImageAllChecked;
        public bool IsImageAllChecked
        {
            get { return _IsImageAllChecked; }
            set
            {
                if (value != _IsImageAllChecked)
                {
                    _IsImageAllChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAdditionalAllChecked;
        public bool IsAdditionalAllChecked
        {
            get { return _IsAdditionalAllChecked; }
            set
            {
                if (value != _IsAdditionalAllChecked)
                {
                    _IsAdditionalAllChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IncludeLoadedDevice;
        public bool IncludeLoadedDevice
        {
            get { return _IncludeLoadedDevice; }
            set
            {
                if (value != _IncludeLoadedDevice)
                {
                    _IncludeLoadedDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IncludeSystemParam;
        public bool IncludeSystemParam
        {
            get { return _IncludeSystemParam; }
            set
            {
                if (value != _IncludeSystemParam)
                {
                    _IncludeSystemParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IncludeBackupInfo;
        public bool IncludeBackupInfo
        {
            get { return _IncludeBackupInfo; }
            set
            {
                if (value != _IncludeBackupInfo)
                {
                    _IncludeBackupInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IncludeSystemInfo;
        public bool IncludeSystemInfo
        {
            get { return _IncludeSystemInfo; }
            set
            {
                if (value != _IncludeSystemInfo)
                {
                    _IncludeSystemInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CellInfo> _Cells = new ObservableCollection<CellInfo>();
        public ObservableCollection<CellInfo> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LogChecker> _LogCheckers = new ObservableCollection<LogChecker>();
        public ObservableCollection<LogChecker> LogCheckers
        {
            get { return _LogCheckers; }
            set
            {
                if (value != _LogCheckers)
                {
                    _LogCheckers = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ImageChecker> _ImageCheckers = new ObservableCollection<ImageChecker>();
        public ObservableCollection<ImageChecker> ImageCheckers
        {
            get { return _ImageCheckers; }
            set
            {
                if (value != _ImageCheckers)
                {
                    _ImageCheckers = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand<Object> _CheckCellAllCommand;
        public ICommand CheckCellAllCommand
        {
            get
            {
                if (null == _CheckCellAllCommand) _CheckCellAllCommand = new AsyncCommand<Object>(CheckCellAllCommandFunc);
                return _CheckCellAllCommand;
            }
        }
        private async Task CheckCellAllCommandFunc(Object param)
        {
            try
            {
                foreach (var cell in Cells)
                {
                    if(cell.IsEnabled)
                    {
                        cell.IsSelected = IsCellAllChecked;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _CheckLogAllCommand;
        public ICommand CheckLogAllCommand
        {
            get
            {
                if (null == _CheckLogAllCommand) _CheckLogAllCommand = new AsyncCommand<Object>(CheckLogAllCommandFunc);
                return _CheckLogAllCommand;
            }
        }
        private async Task CheckLogAllCommandFunc(Object param)
        {
            try
            {
                foreach (var log in LogCheckers)
                {
                    log.IsSelected = IsLogAllChecked;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _CheckImageAllCommand;
        public ICommand CheckImageAllCommand
        {
            get
            {
                if (null == _CheckImageAllCommand) _CheckImageAllCommand = new AsyncCommand<Object>(CheckImageAllCommandFunc);
                return _CheckImageAllCommand;
            }
        }
        private async Task CheckImageAllCommandFunc(Object param)
        {
            try
            {
                foreach (var image in ImageCheckers)
                {
                    image.IsSelected = IsImageAllChecked;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _CheckAdditionalAllCommand;
        public ICommand CheckAdditionalAllCommand
        {
            get
            {
                if (null == _CheckAdditionalAllCommand) _CheckAdditionalAllCommand = new AsyncCommand<Object>(CheckAdditionalAllCommandFunc);
                return _CheckAdditionalAllCommand;
            }
        }
        private async Task CheckAdditionalAllCommandFunc(Object param)
        {
            try
            {
                IncludeLoadedDevice = IsAdditionalAllChecked;
                IncludeSystemParam = IsAdditionalAllChecked;
                IncludeBackupInfo = IsAdditionalAllChecked;
                IncludeSystemInfo = IsAdditionalAllChecked;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _CollectLogCommand;
        public ICommand CollectLogCommand
        {
            get
            {
                if (null == _CollectLogCommand) _CollectLogCommand = new AsyncCommand<Object>(CollectLogCommandFunc);
                return _CollectLogCommand;
            }
        }
        private async Task CollectLogCommandFunc(Object param)
        {
            try
            {
                List<EnumLoggerType> logtypes = new List<EnumLoggerType>();
                List<EnumProberModule> imagetypes = new List<EnumProberModule>();

                bool includeGEM = false;
                bool includeClip = false;

                foreach (var log in LogCheckers)
                {
                    if(log.IsSelected)
                    {
                        EnumLoggerType logType;
                        var a = Enum.TryParse(log.Name, out logType);

                        if(a)
                        {
                            logtypes.Add(logType);
                        }
                        else
                        {
                            if(log.Name == "GEM")
                            {
                                includeGEM = true;
                            }
                        }
                    }
                }

                foreach (var image in ImageCheckers)
                {
                    if (image.IsSelected)
                    {
                        EnumProberModule imageType;

                        var a = Enum.TryParse(image.Name, out imageType);

                        if (a)
                        {
                            imagetypes.Add(imageType);
                        }
                        else
                        {
                            if (image.Name == "CLIP")
                            {
                                includeClip = true;
                            }
                        }
                    }
                }

                if (logtypes.Count > 0 || imagetypes.Count > 0 || IncludeLoadedDevice || IncludeSystemParam || IncludeBackupInfo || IncludeSystemInfo || includeGEM || includeClip)
                {
                    string s_dt = DateTime.Now.ToString("yyyyMMdd_hhmmss");

                    var sdate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, 0);
                    var edate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, 999);

                    List<string> outputPaths = new List<string>();

                    if (!Directory.Exists(LogCollectFolder))
                    {
                        Directory.CreateDirectory(LogCollectFolder);
                    }

                    byte[] compressedFileData;

                    // Cells
                    foreach (var cell in Cells)
                    {
                        compressedFileData = null;

                        if (cell.IsEnabled && cell.IsSelected)
                        {
                            IFileManagerProxy proxy = _LoaderCommunicationManager.GetProxy<IFileManagerProxy>(cell.Index);

                            compressedFileData = proxy?.GetCompressedFile(sdate, edate, logtypes, imagetypes, includeGEM, includeClip, IncludeLoadedDevice, IncludeSystemParam, IncludeBackupInfo, IncludeSystemInfo);

                            if (compressedFileData?.Length > 0)
                            {
                                LoggerManager.Debug($"[{this.GetType().Name}], CollectLogCommandFunc() : Collected [{cell.Name}], {compressedFileData.Length} bytes");

                                var outputPath = $"{LogCollectFolder}\\{cell.Name}_{sdate.ToString("yyyyMMdd")}_{edate.ToString("yyyyMMdd")}.zip";

                                if (File.Exists(outputPath))
                                {
                                    File.Delete(outputPath);
                                }

                                File.WriteAllBytes(outputPath, compressedFileData);

                                outputPaths.Add(outputPath);
                            }
                        }
                    }

                    // Loader
                    if (IncludeLoader)
                    {
                        compressedFileData = null;

                        compressedFileData = this.FileManager().GetCompressedFile(sdate, edate, logtypes, imagetypes, includeGEM, false, false, IncludeSystemParam, IncludeBackupInfo, IncludeSystemInfo);

                        if (compressedFileData?.Length > 0)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}], CollectLogCommandFunc() : Collected [Loader], {compressedFileData.Length} bytes");

                            var outputPath = $"{LogCollectFolder}\\Loader_{sdate.ToString("yyyyMMdd")}_{edate.ToString("yyyyMMdd")}.zip";

                            if (File.Exists(outputPath))
                            {
                                File.Delete(outputPath);
                            }

                            File.WriteAllBytes(outputPath, compressedFileData);

                            outputPaths.Add(outputPath);
                        }
                    }

                    if (outputPaths.Count > 0)
                    {
                        string e_dt = DateTime.Now.ToString("yyyyMMdd_hhmmss");
                        OutputFilePath = $"{LogCollectFolder}\\{s_dt} - {e_dt}" + ".zip";

                        using (var archive = ZipFile.Open(OutputFilePath, ZipArchiveMode.Create))
                        {
                            foreach (var path in outputPaths)
                            {
                                archive.CreateEntryFromFile(path, Path.GetFileName(path));
                            }
                        }

                        foreach (var path in outputPaths)
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], CollectLogCommandFunc() : There are no logs selected for collection.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _OpenOutputPathCommand;
        public ICommand OpenOutputPathCommand
        {
            get
            {
                if (null == _OpenOutputPathCommand) _OpenOutputPathCommand = new AsyncCommand<Object>(OpenOutputPathCommandFunc);
                return _OpenOutputPathCommand;
            }
        }
        private async Task OpenOutputPathCommandFunc(Object param)
        {
            try
            {
                if (!string.IsNullOrEmpty(OutputFilePath))
                {
                    var outputPath = Path.GetDirectoryName(OutputFilePath);
                    if (Directory.Exists(outputPath))
                    {
                        Process.Start("explorer.exe", outputPath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], DeinitModule()");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
                StartDate = DateTime.Now;
                EndDate = DateTime.Now;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                OutputFilePath = string.Empty;

                if (_LoaderCommunicationManager == null)
                {
                    _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                }

                if(_LoaderSupervisor == null)
                {
                    _LoaderSupervisor = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                }

                IsCellAllChecked = false;
                IsLogAllChecked = false;
                IsImageAllChecked = false;
                IsAdditionalAllChecked = false;

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Cells == null)
                    {
                        Cells = new ObservableCollection<CellInfo>();
                    }

                    Cells.Clear();
                    
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
                    {
                        int Cellidx = i + 1;

                        var client = _LoaderSupervisor.GetClient(Cellidx);

                        string name = "C" + Cellidx.ToString().PadLeft(2, '0');

                        if (client != null && _LoaderSupervisor.IsAliveClient(client))
                        {
                            Cells.Add(new CellInfo(Cellidx, $"{name}", false, true));

                        }
                        else
                        {
                            Cells.Add(new CellInfo(Cellidx, $"{name}", false, false));
                        }
                    }

                    if (LogCheckers == null)
                    {
                        LogCheckers = new ObservableCollection<LogChecker>();
                    }

                    LogCheckers.Clear();

                    EnumLoggerType[] Log_enums = (EnumLoggerType[])Enum.GetValues(typeof(EnumLoggerType));

                    foreach (var e in Log_enums)
                    {
                        LogCheckers.Add(new LogChecker(e.ToString(), false));
                    }
                    LogCheckers.Add(new LogChecker("GEM", false));

                    if (ImageCheckers == null)
                    {
                        ImageCheckers = new ObservableCollection<ImageChecker>();
                    }

                    ImageCheckers.Clear();

                    EnumProberModule[] Image_enums = (EnumProberModule[])Enum.GetValues(typeof(EnumProberModule));

                    foreach (var e in Image_enums)
                    {
                        ImageCheckers.Add(new ImageChecker(e.ToString(), false));
                    }
                    ImageCheckers.Add(new ImageChecker("CLIP", false));
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
}
