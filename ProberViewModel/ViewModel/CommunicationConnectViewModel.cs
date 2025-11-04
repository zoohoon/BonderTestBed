using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommunicationConnectViewModelModule
{
    using Autofac;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using LoaderMapView;
    using CommunicationModule;
    using MetroDialogInterfaces;
    using System.Threading;
    using System.Timers;
    using LoaderBase;
    using System.Windows;
    using System.Diagnostics;

    public class CommunicationConnectViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        #region //..Property
        public ILoaderViewModelManager LoaderViewModelManager => (ILoaderViewModelManager)this.ViewModelManager();
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        public StageObject SelectedStageObj
        {
            get { return (StageObject)_LoaderCommunicationManager.SelectedStage; }
            set
            {
                if (value != (_LoaderCommunicationManager.SelectedStage))
                {
                    _LoaderCommunicationManager.SelectedStage = (StageObject)value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ILauncherDiskObject> _LauncherDiskObjectCollection = new ObservableCollection<ILauncherDiskObject>();
        public ObservableCollection<ILauncherDiskObject> LauncherDiskObjectCollection
        {
            get { return _LauncherDiskObjectCollection; }
            set
            {
                if (value != _LauncherDiskObjectCollection)
                {
                    _LauncherDiskObjectCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IStageObject> _Stages;
        public ObservableCollection<IStageObject> Stages
        {
            get { return _Stages; }
            set
            {
                if (value != _Stages)
                {
                    _Stages = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AllEnableAutoConnFlag;
        public bool AllEnableAutoConnFlag
        {
            get { return _AllEnableAutoConnFlag; }
            set
            {
                if (value != _AllEnableAutoConnFlag)
                {
                    _AllEnableAutoConnFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderSupervisor _LoaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _LoaderMaster; }
            set
            {
                if (value != _LoaderMaster)
                {

                    _LoaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAbortGemDisConnect;
        //강제로 Gem 연결을 DisConnect 하여 재연결이 되지 않도록.
        public bool IsAbortGemDisConnect
        {
            get { return _IsAbortGemDisConnect; }
            set
            {
                if (value != _IsAbortGemDisConnect)
                {
                    _IsAbortGemDisConnect = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IGEMModule _Gem;
        public IGEMModule Gem
        {
            get { return _Gem; }
            set
            {
                if (_Gem != value)
                {
                    _Gem = value;
                    RaisePropertyChanged();
                }
            }
        }
        Thread UpdateThread;
        bool bStopUpdateThread;
        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        System.Timers.Timer _monitoringTimer;
        bool updateDatas = false;
        int updateInterval = 1000;

        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("6913771C-E179-AE5E-80CC-57108DA73228");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        public CommunicationConnectViewModel()
        {

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

        public EventCodeEnum InitModule()
        {
            try
            {
                _monitoringTimer = new System.Timers.Timer(updateInterval);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();

                bStopUpdateThread = false;

                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();
                updateDatas = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            Gem = this.GEMModule();
            Stages = _LoaderCommunicationManager.GetStages();
            updateDatas = true;
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                updateDatas = false;
                _LoaderCommunicationManager.SaveParameter();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region //..Command & Method

        #region //..CheckedStagesConnectCommand
        /// <summary>
        /// 연결 된 상태인지 , 연결 할 수 있는 상태인지 체크
        /// </summary>
        private AsyncCommand _CheckedStagesConnectCommand;
        public ICommand CheckedStagesConnectCommand
        {
            get
            {
                if (null == _CheckedStagesConnectCommand) _CheckedStagesConnectCommand = new AsyncCommand(CheckedStagesConnectCommandFunc);
                return _CheckedStagesConnectCommand;
            }
        }
        private async Task CheckedStagesConnectCommandFunc()
        {
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
            string messagestr = null;
            try
            {
                var lunchers = _LoaderCommunicationManager.GetMultiLaunchers();
                var stages = _LoaderCommunicationManager.GetStages();
                string connectedluncherstr = $"Already connected luncher numbers : ";
                string connectedstagestr = $"Already connected stage numbers : ";
                string connectableluncherstr = $"Can connect luncher numbers : ";
                string connectablestagestr = $"Can connect stage numbers : ";
                string cantconnectluncherstr = $"Can not connect luncher numbers";
                string cantconnectstagestr = $"Can not connect stage numbers : ";

                List<int> connectedlunchers = new List<int>();
                List<int> connectedstages = new List<int>();
                List<int> connectablelunchers = new List<int>();
                List<int> connectablestages = new List<int>();
                List<int> cantconnectlunchers = new List<int>();
                List<int> cantconnectstages = new List<int>();

                //await this.WaitCancelDialogService().ShowDialog("Wait");

                //Checked Luncher
                if (lunchers.Count(luncher => luncher.IsChecked) > 0)
                {
                    foreach (var luncher in lunchers)
                    {
                        if (luncher.IsChecked)
                        {
                            if (luncher.IsConnected)
                            {
                                //alreay connected Luncher
                                connectedlunchers.Add(luncher.Index);
                                connectedluncherstr += $"[{luncher.Index + 1}]";
                            }
                            else
                            {
                                var info = _LoaderCommunicationManager.GetLuncherIPPortInfo(luncher.Index);
                                if (info != null)
                                {
                                    var ret = CommunicationManager.CheckAvailabilityCommunication(info.Item1, info.Item2);
                                    if (ret)
                                    {
                                        // Connectable.
                                        connectablelunchers.Add(luncher.Index);
                                        connectableluncherstr += $"[{luncher.Index + 1}]";
                                    }
                                    else
                                    {
                                        // Unable to connect.
                                        cantconnectlunchers.Add(luncher.Index);
                                        cantconnectluncherstr += $"[{luncher.Index + 1}]";
                                    }
                                }

                            }
                        }
                    }
                }

                //Checked Stage
                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                //alreay connected stage
                                connectablestages.Add(stage.Index);
                                connectedstagestr += $"[{stage.Index + 1}]";
                            }
                            else
                            {
                                var info = _LoaderCommunicationManager.GetStageIPPortInfo(stage.Index);
                                if (info != null)
                                {
                                    var ret = CommunicationManager.CheckAvailabilityCommunication(info.Item1, info.Item2);
                                    if (ret)
                                    {
                                        // Connectable.
                                        connectablestages.Add(stage.Index);
                                        connectablestagestr += $"[{stage.Index + 1}]";
                                    }
                                    else
                                    {
                                        // Unable to connect.
                                        cantconnectstages.Add(stage.Index);
                                        cantconnectstagestr += $"[{stage.Index + 1}]";
                                    }
                                }
                            }
                        }
                    }
                }

                //await this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                if (connectedlunchers.Count != 0)
                    messagestr += connectedluncherstr + "\r\n";
                if (connectedstages.Count != 0)
                    messagestr += connectedstagestr + "\r\n";
                if (connectablelunchers.Count != 0)
                    messagestr += connectableluncherstr + "\r\n";
                if (connectablestages.Count != 0)
                    messagestr += connectablestagestr + "\r\n";
                if (cantconnectlunchers.Count != 0)
                    messagestr += cantconnectluncherstr + "\r\n";
                if (cantconnectstages.Count != 0)
                    messagestr += cantconnectstagestr + "\r\n";

                if (messagestr == null)
                    messagestr = "Connectable.";
                await this.MetroDialogManager().ShowMessageDialog("Info Message", messagestr, EnumMessageStyle.Affirmative);

            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");

            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion
        #region //..ConnectCommand
        private AsyncCommand _ConnectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (null == _ConnectCommand) _ConnectCommand = new AsyncCommand(ConnectToProgram);
                return _ConnectCommand;
            }
        }
        private async Task ConnectToProgram()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var lunchers = _LoaderCommunicationManager.GetMultiLaunchers();
                var stages = _LoaderCommunicationManager.GetStages();

                //Connect Luncher
                if (lunchers.Count(luncher => luncher.IsChecked) > 0)
                {
                    foreach (var luncher in lunchers)
                    {
                        if (luncher.IsChecked)
                        {
                            if (luncher.IsConnected)
                            {
                                //alreay connected Luncher
                            }
                            else
                            {
                                _LoaderCommunicationManager.ConnectLauncher(luncher.Index);
                            }
                        }
                    }
                }

                //Connect Stage
                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                //alreay connected stage
                            }
                            else
                            {
                                bool success = await _LoaderCommunicationManager.ConnectStage(stage.Index);
                                if (success)
                                {
                                    stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
                                    GetCellInfo(stage);
                                    _LoaderCommunicationManager.SelectedStage = stage;
                                }
                            }
                        }
                    }
                }
                else if (_LoaderCommunicationManager.SelectedStage != null)
                    await _LoaderCommunicationManager.ConnectStage(_LoaderCommunicationManager.SelectedStage.Index);



                //_LoaderCommunicationManager.ConnectToStage();
            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");

            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private void GetCellInfo(IStageObject stage)
        {
            try
            {
                stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region //..ConnectAllCommand
        private AsyncCommand _ConnectAllCommand;
        public ICommand ConnectAllCommand
        {
            get
            {
                if (null == _ConnectAllCommand) _ConnectAllCommand = new AsyncCommand(ConnectAllToProgram);
                return _ConnectAllCommand;
            }
        }
        private async Task ConnectAllToProgram()
        {
            string sFailCell = String.Empty;
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var stages = _LoaderCommunicationManager.GetStages();

                //Connect Stages
                foreach (var stage in stages)
                {
                    if (stage.StageInfo.IsConnected)
                    {
                        continue;
                    }

                    bool success = await _LoaderCommunicationManager.ConnectStage(stage.Index, true);
                    if (success)
                    {
                        stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
                        GetCellInfo(stage);
                        _LoaderCommunicationManager.SelectedStage = stage;
                    }
                    else
                    {
                        if(sFailCell.Equals(String.Empty))
                        {
                            sFailCell = $"Cell ({stage.Index}";
                        }
                        else
                        {
                            sFailCell = sFailCell + $", {stage.Index}";
                        }
                    }
                }
                if (sFailCell.Equals(String.Empty) == false)
                {
                    sFailCell = sFailCell + $")";
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", $"The {sFailCell} is in system init mode or Error, so try later.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");

            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region //..DisConnectCommand
        private AsyncCommand _DisConnectCommand;
        public ICommand DisConnectCommand
        {
            get
            {
                if (null == _DisConnectCommand) _DisConnectCommand = new AsyncCommand(DisConnectToProgram);
                return _DisConnectCommand;
            }
        }
        private async Task DisConnectToProgram()
        {
            try
            {
                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var lunchers = _LoaderCommunicationManager.GetMultiLaunchers();
                var stages = _LoaderCommunicationManager.GetStages();

                //DisConnect Stage
                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                _LoaderCommunicationManager.DisConnectStage(stage.Index);
                            }
                        }
                    }
                }
                else if (_LoaderCommunicationManager.SelectedStage != null)
                    _LoaderCommunicationManager.DisConnectStage(_LoaderCommunicationManager.SelectedStage.Index);

                //_LoaderCommunicationManager.DisConnectStage();
                //_LoaderCommunicationManager.ConnectToStage();
            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");

            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region //..DisConnectAllCommand
        private AsyncCommand _DisConnectAllCommand;
        public ICommand DisConnectAllCommand
        {
            get
            {
                if (null == _DisConnectAllCommand) _DisConnectAllCommand = new AsyncCommand(DisConnectAllToProgram);
                return _DisConnectAllCommand;
            }
        }
        private async Task DisConnectAllToProgram()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var stages = _LoaderCommunicationManager.GetStages();

                //DisConnect Stages
                foreach (var stage in stages)
                {
                    if (stage.StageInfo.IsConnected)
                    {
                        _LoaderCommunicationManager.DisConnectStage(stage.Index);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region //..StartProgramommand

        private AsyncCommand _StartProgramCommand;
        public ICommand StartProgramCommand
        {
            get
            {
                if (null == _StartProgramCommand) _StartProgramCommand = new AsyncCommand(StartProgramCommandFunc);
                return _StartProgramCommand;
            }
        }

        private async Task StartProgramCommandFunc()
        {
            try
            {

                var lunchers = _LoaderCommunicationManager.GetMultiLaunchers();
                var stages = _LoaderCommunicationManager.GetStages();

                if (lunchers.Count(luncher => luncher.IsChecked) > 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "The Luncher program can not be controlled.", EnumMessageStyle.Affirmative);
                    return;
                }

                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                List<int> stageindex = new List<int>();
                foreach (var stage in stages)
                {
                    if (stage.StageInfo.IsChecked)
                    {
                        if (!stage.StageInfo.IsExcuteProgram)
                            stageindex.Add(stage.Index);
                    }
                }
                _LoaderCommunicationManager.StartProberSystem(stageindex);
            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region //..ExitProgramCommand

        private AsyncCommand _ExitProgramCommand;
        public ICommand ExitProgramCommand
        {
            get
            {
                if (null == _ExitProgramCommand) _ExitProgramCommand = new AsyncCommand(ExitProgramCommandFunc);
                return _ExitProgramCommand;
            }
        }

        private async Task ExitProgramCommandFunc()
        {
            try
            {
                var lunchers = _LoaderCommunicationManager.GetMultiLaunchers();
                var stages = _LoaderCommunicationManager.GetStages();

                if (lunchers.Count(luncher => luncher.IsChecked) > 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "The Luncher program can not be controlled.", EnumMessageStyle.Affirmative);
                    return;
                }

                foreach (var stage in stages)
                {
                    if (stage.StageInfo.IsChecked)
                    {
                        if (stage.StageInfo.IsExcuteProgram)
                            _LoaderCommunicationManager.ExitProberSystem(stage.Index);
                    }
                }

            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");
            }
        }
        #endregion

        #region //..Connect Gem Command(Cell)

        private AsyncCommand _CellGemConnectCommand;
        public ICommand CellGemConnectCommand
        {
            get
            {
                if (null == _CellGemConnectCommand) _CellGemConnectCommand = new AsyncCommand(CellGemConnectCommandFunc);
                return _CellGemConnectCommand;
            }
        }

        private async Task CellGemConnectCommandFunc()
        {
            try
            {
                var stages = _LoaderCommunicationManager.GetStages();
                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                var stageobj = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);
                                if(stageobj != null)
                                {
                                    //Application.Current.Dispatcher.Invoke(() =>
                                    //{
                                        stageobj.InitGemConnectService();
                                    //});
                                    
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Gem Connect Error Message",
                                    "Please connect the cell.", EnumMessageStyle.Affirmative);
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
        #endregion

        #region //..DisConnect Gem Command (Cell)

        private AsyncCommand _CellGemDisConnectCommand;
        public ICommand CellGemDisConnectCommand
        {
            get
            {
                if (null == _CellGemDisConnectCommand) _CellGemDisConnectCommand = new AsyncCommand(CellGemDisConnectCommandFunc);
                return _CellGemDisConnectCommand;
            }
        }

        private async Task CellGemDisConnectCommandFunc()
        {
            try
            {
                var stages = _LoaderCommunicationManager.GetStages();
                if (stages.Count(stage => stage.StageInfo.IsChecked) > 0)
                {
                    foreach (var stage in stages)
                    {
                        if (stage.StageInfo.IsChecked)
                        {
                            if (stage.StageInfo.IsConnected)
                            {
                                var stageobj = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);
                                if (stageobj != null)
                                {
                                    //Application.Current.Dispatcher.Invoke(() =>
                                    //{
                                    stageobj.DeInitGemConnectService();
                                    //});
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Gem Connect Error Message",
                                    "Please connect the cell.", EnumMessageStyle.Affirmative);
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
        #endregion

        #region //..Connect Gem Command(Loader)

        private AsyncCommand _LoaderGemConnectCommand;
        public ICommand LoaderGemConnectCommand
        {
            get
            {
                if (null == _LoaderGemConnectCommand) _LoaderGemConnectCommand = new AsyncCommand(LoaderGemConnectCommandFunc);
                return _LoaderGemConnectCommand;
            }
        }

        private async Task LoaderGemConnectCommandFunc()
        {
            try
            {
                string sHostAppName = "SecsGemServiceHostApp";
                bool bHostAppRun = (Process.GetProcessesByName(sHostAppName).Length > 0) ? true : false;
                Process[] pXGem = Process.GetProcessesByName("XGem");

                if (this.GEMModule().CommunicationState != EnumCommunicationState.DISCONNECT || bHostAppRun || pXGem.GetLength(0) > 0)
                {                    
                    this.GEMModule().GEMHostManualAction = GEMHOST_MANUALACTION.RECONNECT; //fault event handler에서 disconnect를 하지 않기 위함 
                    if (bHostAppRun)
                    {
                        this.GEMModule().GemCommManager.DeInitConnectService();
                    }
                    else
                    {
                        if (pXGem.GetLength(0) > 0)
                        {
                            pXGem[0].Kill();
                        }
                    }                    
                }

                if (this.GEMModule().GemCommManager.InitConnectService() == EventCodeEnum.NONE)
                {
                    this.GEMModule().GEMHostManualAction = GEMHOST_MANUALACTION.CONNECT;
                    this.GEMModule().GemCommManager.CommEnable();                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //..DisConnect Gem Command (Loader)

        private AsyncCommand _LoaderGemDisConnectCommand;
        public ICommand LoaderGemDisConnectCommand
        {
            get
            {
                if (null == _LoaderGemDisConnectCommand) _LoaderGemDisConnectCommand = new AsyncCommand(LoaderGemDisConnectCommandFunc);
                return _LoaderGemDisConnectCommand;
            }
        }

        private async Task LoaderGemDisConnectCommandFunc()
        {
            try
            {                
                this.GEMModule().GEMHostManualAction = GEMHOST_MANUALACTION.DISCONNECT;
                string sHostAppName = "SecsGemServiceHostApp";
                bool bHostAppRun = (Process.GetProcessesByName(sHostAppName).Length > 0) ? true : false;
                if (bHostAppRun)
                {
                    this.GEMModule().GemCommManager.DeInitConnectService();
                }
                else
                {
                    Process[] pXGem = Process.GetProcessesByName("XGem");
                    if (pXGem.GetLength(0) > 0)
                    {
                        pXGem[0].Kill();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //..StageAutoConnectEnableCommand & StageAutoConnectDisableCommand

        private AsyncCommand<int> _StageAutoConnectEnableCommand;
        public ICommand StageAutoConnectEnableCommand
        {
            get
            {
                if(null == _StageAutoConnectEnableCommand) _StageAutoConnectEnableCommand = new AsyncCommand<int>(StageAutoConnectEnableCommandFunc);
                return _StageAutoConnectEnableCommand;
            }
        }

        private async Task StageAutoConnectEnableCommandFunc(int stageIndex)
        {
            try
            {
                var commParam = (LoaderCommunicationParameter)_LoaderCommunicationManager.LoaderCommunicationParam;
                foreach (var launcher in commParam.LauncherParams)
                {
                    var stageParam = launcher.StageParams.Where(stgparam => stgparam.Index == stageIndex);
                    if (stageParam.Count() == 1)
                    {
                        if(!stageParam.First().EnableAutoConnect)
                        {
                            stageParam.First().EnableAutoConnect = true;
                            _LoaderCommunicationManager.SaveParameter();

                            var stgObj = Stages.Where(stage => stage.Index == stageIndex);
                            if (stgObj.Count() == 1)
                            {
                                stgObj.First().StageInfo.EnableAutoConnect = true;
                            }
                        }
                    }
                    else
                    {
                        //Stages[stageIndex - 1].StageInfo.EnableAutoConnect = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _StageAutoConnectDisableCommand;
        public ICommand StageAutoConnectDisableCommand
        {
            get
            {
                if (null == _StageAutoConnectDisableCommand) _StageAutoConnectDisableCommand = new AsyncCommand<int>(StageAutoConnectDisableCommandFunc);
                return _StageAutoConnectDisableCommand;
            }
        }

        private async Task StageAutoConnectDisableCommandFunc(int stageIndex)
        {
            try
            {
                var commParam = (LoaderCommunicationParameter)_LoaderCommunicationManager.LoaderCommunicationParam;
                foreach (var launcher in commParam.LauncherParams)
                {
                    var stageParam = launcher.StageParams.Where(stgparam => stgparam.Index == stageIndex);
                    if(stageParam.Count() == 1)
                    {
                        if (stageParam.First().EnableAutoConnect)
                        {
                            stageParam.First().EnableAutoConnect = false;
                            _LoaderCommunicationManager.SaveParameter();

                            var stgObj = Stages.Where(stage => stage.Index == stageIndex);
                            if (stgObj.Count() == 1)
                            {
                                stgObj.First().StageInfo.EnableAutoConnect = false;
                            }
                        }
                    }
                    else
                    {
                        //Stages[stageIndex - 1].StageInfo.EnableAutoConnect = true;
                    }
                }

                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AllStageAutoConnectSetCommand;
        public ICommand AllStageAutoConnectSetCommand
        {
            get
            {
                if (null == _AllStageAutoConnectSetCommand) _AllStageAutoConnectSetCommand = new AsyncCommand(AllStageAutoConnectSetCommandFunc);
                return _AllStageAutoConnectSetCommand;
            }
        }

        private async Task AllStageAutoConnectSetCommandFunc()
        {
            try
            {
                var commParam = (LoaderCommunicationParameter)_LoaderCommunicationManager.LoaderCommunicationParam;
                if(AllEnableAutoConnFlag== true)
                {
                    AllEnableAutoConnFlag = false;
                }
                else
                {
                    AllEnableAutoConnFlag = true;
                }

                foreach (var launcher in commParam.LauncherParams)
                {
                    foreach (var stage in launcher.StageParams)
                    {
                        stage.EnableAutoConnect = AllEnableAutoConnFlag;
                    }
                }
                _LoaderCommunicationManager.SaveParameter();
                foreach (var stage in Stages)
                {
                    stage.StageInfo.EnableAutoConnect = AllEnableAutoConnFlag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion

        private RelayCommand _ChangeControlStateCommand;
        public ICommand ChangeControlStateCommand
        {
            get
            {
                if (null == _ChangeControlStateCommand) _ChangeControlStateCommand = new RelayCommand(ChangeControlState);
                return _ChangeControlStateCommand;
            }
        }

        public void ChangeControlState()
        {
            try
            {
                SecsEnum_ControlState controlState = this.Gem?.GemCommManager?.SecsCommInformData.ControlState ?? SecsEnum_ControlState.UNKNOWN;
                if (controlState == SecsEnum_ControlState.EQ_OFFLINE ||
                    controlState == SecsEnum_ControlState.HOST_OFFLINE)
                {
                    this.Gem?.ReqOffLine();

                }
                else if (controlState == SecsEnum_ControlState.ONLINE_LOCAL)
                {
                    this.Gem?.ReqLocal();

                }
                else if (controlState == SecsEnum_ControlState.ONLINE_REMOTE)
                {
                    this.Gem?.ReqRemote();

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private AsyncCommand<int> _StageConnectCommand;
        public ICommand StageConnectCommand
        {
            get
            {
                if (null == _StageConnectCommand) _StageConnectCommand = new AsyncCommand<int>(StageConnectCommandFunc);
                return _StageConnectCommand;
            }
        }

        private async Task StageConnectCommandFunc(int index)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                IStageObject stage = Stages.Where(x => x.Index == index).FirstOrDefault();

                // 연결이 되어 있다면
                if (stage != null && stage.StageInfo.IsConnected == false)
                {
                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Cell Connect", $"Do you want to Connect a Cell{index}?", EnumMessageStyle.AffirmativeAndNegative);

                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        bool success = await _LoaderCommunicationManager.ConnectStage(index);
                        GetCellInfo(stage);
                        _LoaderCommunicationManager.SelectedStage = stage;

                        if (success)
                        {
                            var statussoak_toglgle = LoaderMaster.SoakingModule().GetShowStatusSoakingSettingPageToggleValue();
                            if(statussoak_toglgle)
                            {
                                //Status soaking 미사용이면 알림을 띄워준다.
                                if (false == LoaderMaster.SoakingModule().GetCurrentStatusSoakingUsingFlag())
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Status Soaking", $"Status Soaking is off.\r\nPlease turn on the Status Soaking.", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool pressHiddenKey = false;
                      Application.Current.Dispatcher.Invoke(() =>
                    {
                        pressHiddenKey = Keyboard.IsKeyDown(Key.LeftCtrl);
                    });


                    var retVal = await this.MetroDialogManager().ShowMessageDialog("Cell Disconnect", $"Do you want to Disconnect a Cell{index}?", EnumMessageStyle.AffirmativeAndNegative);
                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        _LoaderCommunicationManager.DisConnectStage(index);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ConnectCommandFunc(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public void UpdateProc()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    if (updateDatas == true)
                    {
                        UpdateData();
                        areUpdateEvent.WaitOne(3000);
                    }
                    else 
                    {
                        //minskim// Loader System CPU 부하 줄이기 위해 Sleep 추가 함
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async void UpdateData()
        {
            try
            {
                Task updateTask = new Task(() =>
                {
                    foreach (var stage in Stages)
                    {
                        //GEM
                        if (this.GEMModule().GemCommManager != null)
                        {
                            stage.StageInfo.GemConnState = this.GEMModule().GemCommManager.GetRemoteConnectState(stage.Index);
                            Thread.Sleep(300);
                        }
                        
                        //Chiller

                    }
                });

                updateTask.Start();
                
                Thread.Sleep(1000);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                areUpdateEvent.Set();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion
    }
}
