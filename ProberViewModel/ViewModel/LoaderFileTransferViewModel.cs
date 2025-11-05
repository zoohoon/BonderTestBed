using Autofac;
using DeviceUpDownControl;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LoaderBase.LoaderLog;
using LoaderBase.LoaderResultMapUpDown;
using LoaderLogSettingViewModule;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Loader;
using ProberInterfaces.Proxies;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace LoaderFileTransferViewModelModule
{
    public class LogfileComponent : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _RootPath;
        public string RootPath
        {
            get { return _RootPath; }
            set
            {
                if (value != _RootPath)
                {
                    _RootPath = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class LoaderFileTransferViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("6033ef20-323f-4b32-b8ca-da4e2cbf368a");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();

        public ILoaderLogManagerModule LoaderLogModule => _Container.Resolve<ILoaderLogManagerModule>();
        public ILoaderLogSplitManager LoaderLogSplitmanager => _Container.Resolve<ILoaderLogSplitManager>();        
        private ILoaderResultMapUpDownMng LoaderResultMapUpDownMng => _Container.Resolve<ILoaderResultMapUpDownMng>();

        public IDeviceManager DeviceManager => _Container.Resolve<IDeviceManager>();

        private ObservableCollection<LogfileComponent> _LogFiles;
        public ObservableCollection<LogfileComponent> LogFiles
        {
            get { return _LogFiles; }
            set
            {
                if (value != _LogFiles)
                {
                    _LogFiles = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<EnumServerPathType> _ServerPathTypes;
        public ObservableCollection<EnumServerPathType> ServerPathTypes
        {
            get { return _ServerPathTypes; }
            set
            {
                if (value != _ServerPathTypes)
                {
                    _ServerPathTypes = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumServerPathType _SelectedServerPathType;
        public EnumServerPathType SelectedServerPathType
        {
            get { return _SelectedServerPathType; }
            set
            {
                if (value != _SelectedServerPathType)
                {
                    _SelectedServerPathType = value;
                    //FuncRefreshNetworklistCommand();
                    RaisePropertyChanged();
                }
            }
        }

        private int MaximumStageCount;

        //private int _LoaderCommunicationManager.SelectedStageIndex = 0;
        //public int LoaderCommunicationManager.SelectedStageIndex
        //{
        //    get { return _LoaderCommunicationManager.SelectedStageIndex; }
        //    set
        //    {
        //        if (value != _LoaderCommunicationManager.SelectedStageIndex)
        //        {
        //            _LoaderCommunicationManager.SelectedStageIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public DeviceExplorerViewModel StageDeviceExplorer { get; set; }
        public DeviceExplorerViewModel LoaderDeviceExplorer { get; set; }
        public DeviceExplorerViewModel NetworkDeviceExplorer { get; set; }

        //private ObservableCollection<string> _StageFileNamelist = new ObservableCollection<string>();
        //public ObservableCollection<string> StageFileNamelist
        //{
        //    get { return _StageFileNamelist; }
        //    set
        //    {
        //        if (value != _StageFileNamelist)
        //        {
        //            _StageFileNamelist = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private string _SelectedStageFileName = string.Empty;
        //public string SelectedStageFileName
        //{
        //    get { return _SelectedStageFileName; }
        //    set
        //    {
        //        if (value != _SelectedStageFileName)
        //        {
        //            _SelectedStageFileName = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ObservableCollection<string> _LoaderFileNamelist = new ObservableCollection<string>();
        //public ObservableCollection<string> LoaderFileNamelist
        //{
        //    get { return _LoaderFileNamelist; }
        //    set
        //    {
        //        if (value != _LoaderFileNamelist)
        //        {
        //            _LoaderFileNamelist = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private string _SelectedLoaderFileName = string.Empty;
        //public string SelectedLoaderFileName
        //{
        //    get { return _SelectedLoaderFileName; }
        //    set
        //    {
        //        if (value != _SelectedLoaderFileName)
        //        {
        //            _SelectedLoaderFileName = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ObservableCollection<string> _NetworkFileNamelist = new ObservableCollection<string>();
        //public ObservableCollection<string> NetworkFileNamelist
        //{
        //    get { return _NetworkFileNamelist; }
        //    set
        //    {
        //        if (value != _NetworkFileNamelist)
        //        {
        //            _NetworkFileNamelist = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private LoaderLogSettingView _LDLogSettingView;

        public LoaderLogSettingView LDLogSettingView
        {
            get { return _LDLogSettingView; }
            set { _LDLogSettingView = value; }
        }


        //private string _SelectedNetworkFileName = string.Empty;
        //public string SelectedNetworkFileName
        //{
        //    get { return _SelectedNetworkFileName; }
        //    set
        //    {
        //        if (value != _SelectedNetworkFileName)
        //        {
        //            _SelectedNetworkFileName = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _DetachDevice = false;
        public bool DetachDevice
        {
            get { return _DetachDevice; }
            set
            {
                if (value != _DetachDevice)
                {
                    _DetachDevice = value;
                    RaisePropertyChanged();
                }
            }
        }



        public EventCodeEnum GetDevicelist()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.LoaderFileManager().GetDevicelist();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private LogfileComponent MakeLogfileComponent(string roothpath)
        {
            LogfileComponent tmp = null;

            try
            {
                tmp = new LogfileComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return tmp;
        }

        private void LogTestFunc()
        {
            LogFiles = new ObservableCollection<LogfileComponent>();

            //string PMIPath = @""
            //LogFiles.Add(MakeLogfileComponent();
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                StageDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/FolderIcon.png");

                StageDeviceExplorer.ChangeDeviceCountPerPage(16);

                LoaderDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/FolderIcon.png");

                LoaderDeviceExplorer.ChangeDeviceCountPerPage(16);

                NetworkDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/FolderIcon.png");

                NetworkDeviceExplorer.ChangeDeviceCountPerPage(16);
                //NetworkDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/ZipIcon.png");

                //NetworkDeviceExplorer.ChangeDeviceCountPerPage(14);

                ServerPathTypes = new ObservableCollection<EnumServerPathType>();
                ServerPathTypes.Add(EnumServerPathType.Undefined);
                ServerPathTypes.Add(EnumServerPathType.Upload);
                ServerPathTypes.Add(EnumServerPathType.Download);
                SelectedServerPathType = EnumServerPathType.Undefined;
                LogTestFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MaximumStageCount = LoaderCommunicationManager.GetStages().Count;

                await FuncRefreshStagelistCommand();

                await FuncRefreshLoaderlistCommand();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                DetachDevice = DeviceManager.GetDetachDeviceFlag();
                LoaderCommunicationManager.ChangeSelectedStageEvent -= UpdateStageDetachDeviceList;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand _ServerPathChangedCommand;
        public ICommand ServerPathChangedCommand
        {
            get
            {
                if (null == _ServerPathChangedCommand) _ServerPathChangedCommand = new AsyncCommand(ServerPathChangedCommandFunc);
                return _ServerPathChangedCommand;
            }
        }

        private async Task ServerPathChangedCommandFunc()
        {
            await FuncRefreshNetworklistCommand();
        }
        private AsyncCommand _RefreshStagelistCommand;
        public IAsyncCommand RefreshStagelistCommand
        {
            get
            {
                if (null == _RefreshStagelistCommand) _RefreshStagelistCommand = new AsyncCommand(FuncRefreshStagelistCommand);
                return _RefreshStagelistCommand;
            }
        }
        private async Task FuncRefreshStagelistCommand()
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var stages = LoaderCommunicationManager.GetStages();
                var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;

                if (LoaderCommunicationManager.SelectedStageIndex == -1)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        stageindex = -1;

                        if (StageDeviceExplorer != null)
                        {
                            StageDeviceExplorer.DeviceItemVMList.Clear();
                        }
                    });
                }

                //if (StageFileNamelist == null)
                //{
                //    StageFileNamelist = new ObservableCollection<string>();
                //}

                //StageFileNamelist.Clear();

                List<String> deviceNameList = new List<string>();

                if (stages != null && stageindex != -1)
                {
                    var stage = stages[stageindex];

                    // IsConnected
                    if (stages[stageindex].StageInfo.IsConnected)
                    {
                        // IsSelected
                        //if (stages[stageindex].StageInfo.IsChecked == true)
                        //{
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            //StageFileNamelist.Clear();

                            var FMServiceClient = LoaderCommunicationManager.GetProxy<IFileManagerProxy>(stage.Index);

                            deviceNameList = FMServiceClient?.GetDeviceNamelist().ToList();

                            if (StageDeviceExplorer != null)
                            {
                                StageDeviceExplorer.DeviceItemVMList.Clear();
                                StageDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
                            }

                            //RaisePropertyChanged(nameof(StageFileNamelist));
                        });
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
        }
        //private void RefreshSelectedStageFunc()
        //{
        //    try
        //    {
        //        var stages = LoaderCommunicationManager.GetStages();
        //        var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;

        //        if (StageFileNamelist == null)
        //        {
        //            StageFileNamelist = new ObservableCollection<string>();
        //        }

        //        StageFileNamelist.Clear();

        //        if (stages != null)
        //        {
        //            var stage = stages[stageindex];

        //            // IsConnected
        //            if (stages[stageindex].StageInfo.IsConnected)
        //            {
        //                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
        //                {

        //                    var FMServiceClient = LoaderCommunicationManager.GetFileManagerClient(stage.Index);

        //                    StageFileNamelist = FMServiceClient.GetDeviceNamelist();
        //                    //RaisePropertyChanged(nameof(StageFileNamelist));
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private AsyncCommand _RefreshLoaderlistCommand;
        public IAsyncCommand RefreshLoaderlistCommand
        {
            get
            {
                if (null == _RefreshLoaderlistCommand) _RefreshLoaderlistCommand = new AsyncCommand(FuncRefreshLoaderlistCommand);
                return _RefreshLoaderlistCommand;
            }
        }
        private async Task FuncRefreshLoaderlistCommand()
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                List<String> deviceNameList = new List<string>();

                var directories = Directory.GetDirectories(DeviceManager.GetLoaderDevicePath());

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    deviceNameList.Clear();

                    foreach (var directory in directories)
                    {
                        var directoryNameSplit = directory.Split('\\');
                        if (!directoryNameSplit[directoryNameSplit.Length - 1].Equals(DeviceManager.DetachDeviceFolderName))
                        {
                            deviceNameList.Add(directoryNameSplit[directoryNameSplit.Length - 1]);
                        }
                    }
                });

                if (LoaderDeviceExplorer != null)
                {
                    LoaderDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
        }

        private AsyncCommand _LogUploadCommand;
        public IAsyncCommand LogUploadCommand
        {
            get
            {
                if (null == _LogUploadCommand) _LogUploadCommand = new AsyncCommand(LogUploadCommandFunc);
                return _LogUploadCommand;
            }
        }
        private async Task LogUploadCommandFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = LoaderLogModule.ManualUploadStageAndLoader();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        private AsyncCommand _LoaderLogSettingCommand;
        public IAsyncCommand LoaderLogSettingCommand
        {
            get
            {
                if (null == _LoaderLogSettingCommand) _LoaderLogSettingCommand = new AsyncCommand(LoaderLogSettingFunc);
                return _LoaderLogSettingCommand;
            }
        }
        private async Task LoaderLogSettingFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _LDLogSettingView = new LoaderLogSettingView();
                    _LDLogSettingView.Owner = Application.Current.MainWindow;
                    _LDLogSettingView.Width = 800;
                    _LDLogSettingView.Height = 800;
                    _LDLogSettingView.Title = "Loader log setting Window";
                    var dlgresult = _LDLogSettingView.ShowDialog();
                }));

                //ret = LoaderLogModule.UpLoadLogToServer();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private AsyncCommand _FailedUploadResultMapShowCmd;
        public IAsyncCommand FailedUploadResultMapShowCmd
        {
            get
            {
                if (null == _FailedUploadResultMapShowCmd)
                    _FailedUploadResultMapShowCmd = new AsyncCommand(ShowFailedToUploadResultMap);
                return _FailedUploadResultMapShowCmd;
            }
        }

        private async Task ShowFailedToUploadResultMap()
        {            
            try
            {
                LoaderResultMapUpDownMng.ShowManaualUploadDlg();
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }            
        }

        private AsyncCommand _RefreshNetworklistCommand;
        public IAsyncCommand RefreshNetworklistCommand
        {
            get
            {
                if (null == _RefreshNetworklistCommand) _RefreshNetworklistCommand = new AsyncCommand(FuncRefreshNetworklistCommand);
                return _RefreshNetworklistCommand;
            }
        }
        public string GetIPadressInString(string str)
        {
            string ret = null;
            try
            {
                Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                MatchCollection result = ip.Matches(str);
                ret = result[0].ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private async Task FuncRefreshNetworklistCommand()
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                EventCodeEnum ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                //if (NetworkFileNamelist == null)
                //{
                //    NetworkFileNamelist = new ObservableCollection<string>();
                //}
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    NetworkDeviceExplorer.DeviceItemVMList.Clear();
                });
                //NetworkFileNamelist.Clear();

                //var rootpath = Path.GetPathRoot(LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value);
                //NetworkCredential credentials = new NetworkCredential(LoaderLogModule.LoaderLogParam.UserName.Value, LoaderLogModule.LoaderLogParam.Password.Value);
                //var isconnected = NetworkConnection.ConnectValidate(rootpath, credentials);
                string networkpath = LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value;
                ret = LoaderLogSplitmanager.ConnectCheck(networkpath, LoaderLogModule.LoaderLogParam.UserName.Value,
                    LoaderLogModule.LoaderLogParam.Password.Value);
                if (ret != EventCodeEnum.NONE)
                {
                    ret = LoaderLogSplitmanager.CheckFolderExist(networkpath, LoaderLogModule.LoaderLogParam.UserName.Value,
                    LoaderLogModule.LoaderLogParam.Password.Value);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = LoaderLogSplitmanager.CreateDicrectory(networkpath, LoaderLogModule.LoaderLogParam.UserName.Value,
                    LoaderLogModule.LoaderLogParam.Password.Value);
                        if (ret != EventCodeEnum.NONE)
                        {
                            if(ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                            {                                
                                this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode :\n{LoaderLogSplitmanager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);                                
                                return;
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader and path", enummessagesytel: EnumMessageStyle.Affirmative);
                                return;
                            }                         
                        }
                    }
                }


                List<String> deviceNameList = new List<string>();

                switch (SelectedServerPathType)
                {
                    case EnumServerPathType.Undefined:
                        break;
                    case EnumServerPathType.Upload:
                        try
                        {
                            string path = LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value;
                            //if (!Directory.Exists(path))
                            //{
                            //    Directory.CreateDirectory(path);
                            //}
                            ret = LoaderLogSplitmanager.CheckFolderExist(path, LoaderLogModule.LoaderLogParam.UserName.Value,
                                LoaderLogModule.LoaderLogParam.Password.Value);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = LoaderLogSplitmanager.CreateDicrectory(path, LoaderLogModule.LoaderLogParam.UserName.Value,
                                LoaderLogModule.LoaderLogParam.Password.Value);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Check to path or network status", $"have to check that network status and path", enummessagesytel: EnumMessageStyle.Affirmative);
                                    return;
                                }
                            }
                            List<string> tmplist = new List<string>();
                            LoaderLogSplitmanager.GetFolderListFromServer(path, LoaderLogModule.LoaderLogParam.UserName.Value,
                                LoaderLogModule.LoaderLogParam.Password.Value, ref tmplist);

                            string devicename = string.Empty;

                            foreach (var file in tmplist)
                            {
                                var directoryNameSplit = file.Split('\\');
                                devicename = directoryNameSplit[directoryNameSplit.Length - 1];
                                deviceNameList.Add(devicename);
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                if (NetworkDeviceExplorer != null)
                                {
                                    NetworkDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
                                }
                            });
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            LoggerManager.Exception(e);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                        break;
                    case EnumServerPathType.Download:

                        try
                        {
                            string path = LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value;
                            ret = LoaderLogSplitmanager.CheckFolderExist(path, LoaderLogModule.LoaderLogParam.UserName.Value, LoaderLogModule.LoaderLogParam.Password.Value);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = LoaderLogSplitmanager.CreateDicrectory(path, LoaderLogModule.LoaderLogParam.UserName.Value, LoaderLogModule.LoaderLogParam.Password.Value);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Check to path or network status", $"have to check that network status and path", enummessagesytel: EnumMessageStyle.Affirmative);
                                    return;
                                }
                            }

                            List<string> tmplist = new List<string>();
                            LoaderLogSplitmanager.GetFolderListFromServer(path, LoaderLogModule.LoaderLogParam.UserName.Value, LoaderLogModule.LoaderLogParam.Password.Value, ref tmplist);
                            string devicename = string.Empty;

                            foreach (var file in tmplist)
                            {
                                var directoryNameSplit = file.Split('\\');
                                devicename = directoryNameSplit[directoryNameSplit.Length - 1];

                                deviceNameList.Add(devicename);
                            }

                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                if (NetworkDeviceExplorer != null)
                                {
                                    NetworkDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
                                }
                            });
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            LoggerManager.Exception(e);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
        }

        private RelayCommand<EnumPrevNext> _SelectedStageIndexCommand;
        public ICommand SelectedStageIndexCommand
        {
            get
            {
                if (null == _SelectedStageIndexCommand) _SelectedStageIndexCommand = new RelayCommand<EnumPrevNext>(FuncSelectedStageIndexCommand);
                return _SelectedStageIndexCommand;
            }
        }

        private void FuncSelectedStageIndexCommand(EnumPrevNext obj)
        {
            try
            {
                // Range : 0 ~ (MaximumStageCount - 1)

                int IndexMaxValue = (MaximumStageCount - 1);

                switch (obj)
                {
                    case EnumPrevNext.Prev:

                        if (LoaderCommunicationManager.SelectedStageIndex <= 0)
                        {
                            LoaderCommunicationManager.SelectedStageIndex = IndexMaxValue;
                        }
                        else
                        {
                            LoaderCommunicationManager.SelectedStageIndex = LoaderCommunicationManager.SelectedStageIndex - 1;
                        }

                        break;
                    case EnumPrevNext.Next:

                        if (LoaderCommunicationManager.SelectedStageIndex >= IndexMaxValue)
                        {
                            LoaderCommunicationManager.SelectedStageIndex = 0;
                        }
                        else
                        {
                            LoaderCommunicationManager.SelectedStageIndex = LoaderCommunicationManager.SelectedStageIndex + 1;
                        }

                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private bool IsConnectedStage()
        {
            bool retval = false;

            try
            {
                var stages = LoaderCommunicationManager.GetStages();
                var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;
                if (LoaderCommunicationManager.SelectedStageIndex == -1)
                {
                    stageindex = -1;
                }
                if (stages != null && stageindex != -1)
                {
                    var stage = stages[stageindex];

                    // IsConnected
                    if (stages[stageindex].StageInfo.IsConnected)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private AsyncCommand<EnumFileTransferDirection> _DeviceFileTransferCommand;
        public IAsyncCommand DeviceFileTransferCommand
        {
            get
            {
                if (null == _DeviceFileTransferCommand) _DeviceFileTransferCommand = new AsyncCommand<EnumFileTransferDirection>(FuncDeviceFileTransferCommand);
                return _DeviceFileTransferCommand;
            }
        }

        private async Task<bool> CheckFileExistAndOverwrite(EnumFileTransferDirection direction, string Name)
        {
            bool NotExist = false;
            bool retval = false;

            try
            {
                if (direction == EnumFileTransferDirection.StageToLoader)
                {
                    if (Directory.Exists(Name))
                    {
                        NotExist = false;
                    }
                    else
                    {
                        NotExist = true;
                        retval = true;
                    }
                }
                else if (direction == EnumFileTransferDirection.LoaderToStage || direction == EnumFileTransferDirection.NetworkToStage)
                {
                    var stages = LoaderCommunicationManager.GetStages();
                    var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;

                    if (LoaderCommunicationManager.SelectedStageIndex == -1)
                    {
                        stageindex = -1;
                    }

                    if (stages != null && stageindex != -1)
                    {
                        var FMServiceClient = LoaderCommunicationManager.GetProxy<IFileManagerProxy>(stages[stageindex].Index);

                        List<String> deviceNameList = new List<string>();
                        deviceNameList = FMServiceClient?.GetDeviceNamelist().ToList();

                        if (deviceNameList.Count > 0)
                        {
                            var file = deviceNameList.FirstOrDefault(n => n == Name);

                            if (file != null)
                            {
                                NotExist = false;
                            }
                            else
                            {
                                NotExist = true;
                                retval = true;
                            }
                        }
                        else
                        {
                            NotExist = true;
                            retval = true;
                        }
                    }
                }
                else if (direction == EnumFileTransferDirection.LoaderToNetwork || direction == EnumFileTransferDirection.StageToNetwork)
                {
                    var pathExist = LoaderLogSplitmanager.CheckFolderExist(Name, LoaderLogModule.LoaderLogParam.UserName.Value, LoaderLogModule.LoaderLogParam.Password.Value);
                    if(pathExist == EventCodeEnum.NONE)
                    {
                        // device exist 
                        NotExist = false;
                    }
                    else
                    {
                        NotExist = true;
                        retval = true;
                    }
                }
                else if (direction == EnumFileTransferDirection.NetworkToLoader)
                {
                    if (Directory.Exists(Name))
                    {
                        NotExist = false;
                    }
                    else
                    {
                        NotExist = true;
                        retval = true;
                    }
                }                

                if (NotExist == false)
                {
                    EnumMessageDialogResult MessageRetVal = await this.MetroDialogManager().ShowMessageDialog("[Transfer]", "Already exist the same file name. Are you sure you want to overwrite?", EnumMessageStyle.AffirmativeAndNegative);

                    if (MessageRetVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private async Task FuncDeviceFileTransferCommand(EnumFileTransferDirection obj)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                string fullpath = string.Empty;
                string zippath = string.Empty;
                byte[] device = new byte[0];

                String uploadDevName = string.Empty;
                bool CanTransfer = false;

                switch (obj)
                {
                    case EnumFileTransferDirection.StageToLoader:

                        uploadDevName = StageDeviceExplorer.SelectedDeviceItemVM?.DeviceName;

                        if ((uploadDevName != string.Empty) && uploadDevName != null && (IsConnectedStage()))
                        {
                            var stages = LoaderCommunicationManager.GetStages();
                            var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;                            
                            if (LoaderCommunicationManager.SelectedStageIndex == -1)
                            {
                                stageindex = -1;
                            }

                            if (stages != null && stageindex != -1)
                            {
                                var FMServiceClient = LoaderCommunicationManager.GetProxy<IFileManagerProxy>(stages[stageindex].Index);

                                string loaderdevicepath = DeviceManager.GetLoaderDevicePath();
                                fullpath = loaderdevicepath + "\\" + uploadDevName;

                                CanTransfer = await CheckFileExistAndOverwrite(EnumFileTransferDirection.StageToLoader, fullpath);

                                if (CanTransfer == true)
                                {
                                    LoggerManager.Debug($"FuncDeviceFileTransferCommand type={obj}, cell={LoaderCommunicationManager.SelectedStageIndex}, device={uploadDevName} ");
                                    if (FMServiceClient != null)
                                    {
                                        device = FMServiceClient.GetDeviceByFileName(uploadDevName);

                                        if (device.Length > 0)
                                        {
                                            if (DeviceManager.GetDetachDeviceFlag())
                                            {
                                                fullpath = DeviceManager.GetLoaderDevicePath() + $"\\{DeviceManager.DetachDeviceFolderName}"
                                                + "\\c" + $"{LoaderCommunicationManager.SelectedStage.Index.ToString().PadLeft(2, '0')}\\{uploadDevName}";
                                            }

                                            zippath = fullpath + ".zip";

                                            File.WriteAllBytes(zippath, device);

                                            try
                                            {
                                                if (Directory.Exists(fullpath))
                                                {
                                                    Directory.Delete(fullpath, true);
                                                    System.Threading.Thread.Sleep(500);
                                                }

                                                ZipFile.ExtractToDirectory(zippath, fullpath);
                                            }
                                            catch (Exception err)
                                            {
                                                this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                            }

                                            File.Delete(zippath);

                                            await FuncRefreshLoaderlistCommand();

                                            //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                            //{
                                            //    FuncRefreshLoaderlistCommand();
                                            //}));
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    case EnumFileTransferDirection.LoaderToStage:

                        uploadDevName = LoaderDeviceExplorer.SelectedDeviceItemVM?.DeviceName;

                        if ((uploadDevName != string.Empty) && uploadDevName != null && (IsConnectedStage()))
                        {
                            var stages = LoaderCommunicationManager.GetStages();
                            var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;                            
                            if (LoaderCommunicationManager.SelectedStageIndex == -1)
                            {
                                stageindex = -1;
                            }

                            if (stages != null && stageindex != -1)
                            {
                                var FMServiceClient = LoaderCommunicationManager.GetProxy<IFileManagerProxy>(stages[stageindex].Index);

                                if (FMServiceClient != null)
                                {
                                    CanTransfer = await CheckFileExistAndOverwrite(EnumFileTransferDirection.LoaderToStage, uploadDevName);

                                    if (CanTransfer == true)
                                    {
                                        LoggerManager.Debug($"FuncDeviceFileTransferCommand type={obj}, cell={LoaderCommunicationManager.SelectedStageIndex}, device={uploadDevName} ");
                                        device = DeviceManager.Compress(uploadDevName, LoaderCommunicationManager.SelectedStageIndex);

                                        if (device?.Length > 0)
                                        {
                                            bool retval = false;

                                            retval = FMServiceClient.SetDeviceByFileName(device, uploadDevName);
                                            await FuncRefreshStagelistCommand();

                                            if (!retval)
                                            {
                                                this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    case EnumFileTransferDirection.LoaderToNetwork:

                        string rootpath = this.LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value;
                        //NetworkCredential credentials = new NetworkCredential(this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);

                        ret = LoaderLogSplitmanager.ConnectCheck(rootpath, this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);
                        if (ret == EventCodeEnum.NONE)
                        {
                            uploadDevName = LoaderDeviceExplorer.SelectedDeviceItemVM?.DeviceName;

                            if ((uploadDevName != string.Empty) && uploadDevName != null && SelectedServerPathType == EnumServerPathType.Upload)
                            {                                
                                CanTransfer = await CheckFileExistAndOverwrite(EnumFileTransferDirection.LoaderToNetwork, rootpath + "/" + uploadDevName);

                                if(CanTransfer)
                                {
                                    LoggerManager.Debug($"FuncDeviceFileTransferCommand type={obj}, device={uploadDevName} ");
                                    fullpath = DeviceManager.GetLoaderDevicePath() + "\\" + uploadDevName;
                                    zippath = fullpath + ".zip";

                                    if (!File.Exists(zippath))
                                    {
                                        ZipFile.CreateFromDirectory(fullpath, zippath);
                                    }

                                    ret = LoaderLogSplitmanager.LoaderDeviceUploadToServer(fullpath, this.LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value,
                                        this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);

                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                    }
                                    //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    //{
                                    await FuncRefreshNetworklistCommand();
                                    //}));

                                    // Delete local ZIP
                                    File.Delete(zippath);
                                }
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Upload", $"have to check that selected of combo box list or file selected ", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode :\n{LoaderLogSplitmanager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);
                                return;
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                        }

                        break;

                    case EnumFileTransferDirection.NetworkToLoader:

                        rootpath = this.LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value;
                        //NetworkCredential credentials = new NetworkCredential(this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);

                        //var isconnected = NetworkConnection.ConnectValidate(rootpath, credentials);
                        ret = LoaderLogSplitmanager.ConnectCheck(rootpath, this.LoaderLogModule.LoaderLogParam.UserName.Value,
                            this.LoaderLogModule.LoaderLogParam.Password.Value);
                        if (ret == EventCodeEnum.NONE)
                        {
                            uploadDevName = NetworkDeviceExplorer.SelectedDeviceItemVM?.DeviceName;

                            if ((uploadDevName != string.Empty) && uploadDevName != null && SelectedServerPathType == EnumServerPathType.Download)
                            {                                
                                string destpath = DeviceManager.GetLoaderDevicePath() + "\\" + uploadDevName;

                                CanTransfer = await CheckFileExistAndOverwrite(EnumFileTransferDirection.NetworkToLoader, destpath);

                                if(CanTransfer)
                                {
                                    LoggerManager.Debug($"FuncDeviceFileTransferCommand type={obj}, device={uploadDevName} ");
                                    //NetworkCredential credentials2 = new NetworkCredential(LoaderLogModule.LoaderLogParam.UserName.Value, LoaderLogModule.LoaderLogParam.Password.Value);


                                    //using (new NetworkConnection(LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value, credentials2))
                                    //{
                                    try
                                    {
                                        string path = null;
                                        if (LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value[LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value.Length - 1] == '/')
                                        {
                                            path = LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value + uploadDevName;
                                        }
                                        else
                                        {
                                            path = LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value + '/' + uploadDevName;
                                        }

                                        //fi = new System.IO.FileInfo(zippath);
                                        //zippath = path + uploadDevName + ".zip";
                                        ret = LoaderLogSplitmanager.LoaderDeviceDownloadFromServer(path, destpath, this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);

                                        if (ret == EventCodeEnum.UNDEFINED)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("download", $"have to check that selected of combo box list or file selected ", enummessagesytel: EnumMessageStyle.Affirmative);
                                        }
                                        else if (ret != EventCodeEnum.NONE)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                        }
                                        //File.Copy(zippath, destpath + ".zip", true);
                                    }
                                    catch (System.IO.FileNotFoundException e)
                                    {
                                        LoggerManager.Exception(e);
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                    //}

                                    //ZipFile.ExtractToDirectory(destpath + ".zip", destpath);
                                    //System.Threading.Thread.Sleep(300);

                                    //File.Delete(destpath + ".zip");
                                    await FuncRefreshLoaderlistCommand();
                                }
                                
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Download", $"have to check that selected of combo box list or file selected ", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode :\n{LoaderLogSplitmanager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);                                
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                        }
                        break;

                    case EnumFileTransferDirection.StageToNetwork:
                        // stage -> loader -> network
                        uploadDevName = StageDeviceExplorer.SelectedDeviceItemVM?.DeviceName;

                        if ((uploadDevName != string.Empty) && uploadDevName != null && (IsConnectedStage()) && SelectedServerPathType == EnumServerPathType.Upload)
                        {
                            var stages = LoaderCommunicationManager.GetStages();
                            var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;
                            if (LoaderCommunicationManager.SelectedStageIndex == -1)
                            {
                                stageindex = -1;
                            }

                            if (stages != null && stageindex != -1)
                            {
                                var FMServiceClient = LoaderCommunicationManager.GetProxy<IFileManagerProxy>(stages[stageindex].Index);
                                if (FMServiceClient != null)
                                {
                                    fullpath = DeviceManager.GetLoaderDevicePath() + "\\" + uploadDevName;
                                    rootpath = this.LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value;
                                    ret = LoaderLogSplitmanager.ConnectCheck(rootpath, this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);
                                    if (ret == EventCodeEnum.NONE)
                                    {
                                        CanTransfer = await CheckFileExistAndOverwrite(EnumFileTransferDirection.StageToNetwork, rootpath + "/" + uploadDevName);
                                        if (CanTransfer)
                                        {
                                            LoggerManager.Debug($"FuncDeviceFileTransferCommand type={obj}, cell={LoaderCommunicationManager.SelectedStageIndex}, device={uploadDevName} ");

                                            device = FMServiceClient.GetDeviceByFileName(uploadDevName);

                                            if (device.Length > 0)
                                            {
                                                if (DeviceManager.GetDetachDeviceFlag())
                                                {
                                                    fullpath = DeviceManager.GetLoaderDevicePath() + $"\\{DeviceManager.DetachDeviceFolderName}"
                                                    + "\\c" + $"{LoaderCommunicationManager.SelectedStage.Index.ToString().PadLeft(2, '0')}\\{uploadDevName}";
                                                }

                                                zippath = fullpath + ".zip";

                                                File.WriteAllBytes(zippath, device);

                                                try
                                                {
                                                    if (Directory.Exists(fullpath))
                                                    {
                                                        Directory.Delete(fullpath, true);
                                                        System.Threading.Thread.Sleep(500);
                                                    }

                                                    ZipFile.ExtractToDirectory(zippath, fullpath);
                                                }
                                                catch (Exception err)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                                }

                                                // loader -> network                                                                                                                        
                                                if (!File.Exists(zippath))
                                                {
                                                    ZipFile.CreateFromDirectory(fullpath, zippath);
                                                }

                                                ret = LoaderLogSplitmanager.LoaderDeviceUploadToServer(fullpath, this.LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value,
                                                    this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);

                                                if (ret != EventCodeEnum.NONE)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                                }
                                                await FuncRefreshNetworklistCommand();
                                                await FuncRefreshLoaderlistCommand();
                                                // Delete local ZIP
                                                File.Delete(zippath);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode :\n{LoaderLogSplitmanager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);
                                        }
                                        else
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader", enummessagesytel: EnumMessageStyle.Affirmative);
                                        }
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"FMServiceClient is null.");
                                }                                                                    
                            }
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("Upload", $"have to check that selected of combo box list or file selected ", enummessagesytel: EnumMessageStyle.Affirmative);
                        }                      
                        break;
                    case EnumFileTransferDirection.NetworkToStage:
                        // network -> loader -> stage
                        uploadDevName = NetworkDeviceExplorer.SelectedDeviceItemVM?.DeviceName;
                        rootpath = this.LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value;                                                
                        ret = LoaderLogSplitmanager.ConnectCheck(rootpath, this.LoaderLogModule.LoaderLogParam.UserName.Value,this.LoaderLogModule.LoaderLogParam.Password.Value);
                        if (ret == EventCodeEnum.NONE)
                        {                            
                            if ((uploadDevName != string.Empty) && uploadDevName != null && (IsConnectedStage()) &&  SelectedServerPathType == EnumServerPathType.Download)
                            {
                                var stages = LoaderCommunicationManager.GetStages();
                                var stageindex = LoaderCommunicationManager.SelectedStageIndex - 1;
                                if (LoaderCommunicationManager.SelectedStageIndex == -1)
                                {
                                    stageindex = -1;
                                }
                                if (stages != null && stageindex != -1)
                                {
                                    var FMServiceClient = LoaderCommunicationManager.GetProxy<IFileManagerProxy>(stages[stageindex].Index);
                                    if (FMServiceClient != null)
                                    {                                        
                                        CanTransfer = await CheckFileExistAndOverwrite(EnumFileTransferDirection.NetworkToStage, uploadDevName);

                                        if (CanTransfer)
                                        {
                                            LoggerManager.Debug($"FuncDeviceFileTransferCommand type={obj}, cell={LoaderCommunicationManager.SelectedStageIndex}, device={uploadDevName} ");

                                            try
                                            {
                                                string path = null;
                                                if (LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value[LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value.Length - 1] == '/')
                                                {
                                                    path = LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value + uploadDevName;
                                                }
                                                else
                                                {
                                                    path = LoaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value + '/' + uploadDevName;
                                                }
                                                // network -> loader
                                                string destpath = DeviceManager.GetLoaderDevicePath() + "\\" + uploadDevName;
                                                ret = LoaderLogSplitmanager.LoaderDeviceDownloadFromServer(path, destpath, this.LoaderLogModule.LoaderLogParam.UserName.Value, this.LoaderLogModule.LoaderLogParam.Password.Value);
                                                if(ret == EventCodeEnum.NONE)
                                                {                                                    
                                                    device = DeviceManager.Compress(uploadDevName, LoaderCommunicationManager.SelectedStageIndex);
                                                    if (device?.Length > 0)
                                                    {
                                                        bool retval = false;

                                                        // loader -> stage
                                                        retval = FMServiceClient.SetDeviceByFileName(device, uploadDevName);
                                                        if (!retval)
                                                        {
                                                            this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                                        }                                                                                                         
                                                    }
                                                }
                                                else if (ret == EventCodeEnum.UNDEFINED)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("download", $"have to check that selected of combo box list or file selected ", enummessagesytel: EnumMessageStyle.Affirmative);
                                                }
                                                else if (ret != EventCodeEnum.NONE)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer. Probably it is being used by another process.", enummessagesytel: EnumMessageStyle.Affirmative);
                                                }
                                            }
                                            catch (System.IO.FileNotFoundException e)
                                            {
                                                LoggerManager.Exception(e);
                                            }
                                            catch (Exception err)
                                            {
                                                LoggerManager.Exception(err);
                                            }
                                            await FuncRefreshLoaderlistCommand();
                                            await FuncRefreshStagelistCommand();
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"FMServiceClient is null.");
                                    }
                                }

                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Download", $"have to check that selected of combo box list or file selected ", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                        }
                        else
                        {
                            if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Fail", $"FtpStatusCode :\n{LoaderLogSplitmanager.showErrorMsg}\n", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Connected Fail", $"have to check that network connect between server and loader", enummessagesytel: EnumMessageStyle.Affirmative);
                            }
                        }                                               
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("File Transfer", "Cannot complete the file transfer.", enummessagesytel: EnumMessageStyle.Affirmative);
            }
            //finally
            //{
            //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
        }

        private AsyncCommand _DetachDeviceFlagChangeCommand;
        public IAsyncCommand DetachDeviceFlagChangeCommand
        {
            get
            {
                if (null == _DetachDeviceFlagChangeCommand) _DetachDeviceFlagChangeCommand = new AsyncCommand(FuncDetachDeviceFlagChangeCommand);
                return _DetachDeviceFlagChangeCommand;
            }
        }
        private async Task FuncDetachDeviceFlagChangeCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please wait");

                if (_DetachDevice)
                {
                    //Cell Device 분리
                    if (LoaderCommunicationManager.SelectedStage != null)
                    {
                        UpdateStageDetachDeviceList();
                    }
                    LoaderCommunicationManager.ChangeSelectedStageEvent += UpdateStageDetachDeviceList;
                }
                else
                {
                    // Loader Device 리스트 가져오기.
                    await FuncRefreshLoaderlistCommand();
                }

                DeviceManager.SetDetachDeviceFlag(_DetachDevice);
                DeviceManager.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private void UpdateStageDetachDeviceList()
        {
            try
            {
                List<String> deviceNameList = new List<string>();

                string path = DeviceManager.GetLoaderDevicePath() + $"\\{DeviceManager.DetachDeviceFolderName}"
                    + "\\c" + $"{LoaderCommunicationManager.SelectedStage.Index.ToString().PadLeft(2, '0')}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var directories = Directory.GetDirectories(path);
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    deviceNameList.Clear();

                    foreach (var directory in directories)
                    {
                        var directoryNameSplit = directory.Split('\\');
                        deviceNameList.Add(directoryNameSplit[directoryNameSplit.Length - 1]);
                    }
                });

                if (LoaderDeviceExplorer != null)
                {
                    LoaderDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
