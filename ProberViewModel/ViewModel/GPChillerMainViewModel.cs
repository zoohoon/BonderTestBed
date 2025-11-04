namespace ProberViewModel.ViewModel
{
    using Autofac;
    using EnvControlModule.Parameter;
    using LoaderBase;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using RelayCommandBase;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Input;
    using MetroDialogInterfaces;
    using System.Windows;
    using VirtualKeyboardControl;
    using ChillerConnSettingDialog;
    using ProberInterfaces.Temperature.Chiller;
    using WinAPIWrapper;
    using System.Collections.Generic;
    using AccountModule;

    public class GPChillerMainViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region ..Property
        private readonly Guid _ViewModelGUID = new Guid("D2963229-A397-A7E7-292E-ADED32FEB0A4");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }
        public bool Initialized { get; set; } = false;

        private ILoaderCommunicationManager LoaderCommManager { get; set; }
        private ILoaderSupervisor Master;
        private EnvControlParameter EnvControlParam { get; set; }

        private int _SelectedChillerIndex;
        public int SelectedChillerIndex
        {
            get { return _SelectedChillerIndex; }
            set
            {
                if (value != _SelectedChillerIndex)
                {
                    _SelectedChillerIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IChillerModule _SelectedChiller;
        public IChillerModule SelectedChiller
        {
            get { return _SelectedChiller; }
            set
            {
                if (value != _SelectedChiller)
                {
                    _SelectedChiller = value;
                    SelectedChillerIndex = _SelectedChiller.ChillerInfo.Index;
                    RaisePropertyChanged();
                }
            }
        }

        private IEnvControlManager _EnvControlManager;
        public IEnvControlManager EnvControlManager
        {
            get { return _EnvControlManager; }
            set
            {
                if (value != _EnvControlManager)
                {
                    _EnvControlManager = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Guid CurViewGUID { get; set; }

        Thread UpdateThread;
        bool bStopUpdateThread;
        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        AutoResetEvent areUpdateEvent2 = new AutoResetEvent(false);
        bool updateDatas = false;

        #endregion        

        #region ..IMainScreenViewModel Method
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                EnvControlParam = (EnvControlParameter)this.EnvControlManager().EnvSysParam;
                for (int index = 1; index <= EnvControlParam.ChillerSysParam.ChillerParams.Count; index++)
                {
                    var param = EnvControlParam.ChillerSysParam.ChillerParams[index - 1];
                    //Chillers.Add
                    //    (
                    //        new ChillerColdInfo(index)
                    //        {
                    //            ChillerHotLimitTemp = param.ChillerHotLimitTemp,
                    //            CoolantInTemp = param.CoolantInTemp,
                    //            Tolerance = param.Tolerance,
                    //            ActivatableHighTemp = param.ActivatableHighTemp,
                    //            InRangeWindowTemp = param.InRangeWindowTemp,
                    //            AmbientTemp = param.AmbientTemp,
                    //            IsEnableUsePurge = param.IsEnableUsePurge
                    //        }
                    //    );
                    foreach (var stageidx in param.StageIndexs)
                    {
                        Stages.Add
                            (
                                new StageColdInfo(stageidx)
                                {
                                    ChillerNum = index,
                                }
                        );
                    }
                }

                Stages = new ObservableCollection<StageColdInfo>(Stages.OrderBy(stage => stage.Index));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderCommManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

                bStopUpdateThread = false;

                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();
                updateDatas = false;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                EnvControlParam = (EnvControlParameter)this.EnvControlManager().EnvSysParam;
                //for (int index = 1; index <= EnvControlParam.ChillerSysParam.ChillerParams.Count; index++)
                //{
                //    var param = EnvControlParam.ChillerSysParam.ChillerParams[index - 1];
                //    Chillers[index - 1].ChillerHotLimitTemp = param.ChillerHotLimitTemp;
                //    Chillers[index - 1].CoolantInTemp = param.CoolantInTemp;
                //    Chillers[index - 1].Tolerance = param.Tolerance;
                //    Chillers[index - 1].ActivatableHighTemp = param.ActivatableHighTemp;
                //    Chillers[index - 1].InRangeWindowTemp = param.InRangeWindowTemp;
                //    Chillers[index - 1].AmbientTemp = param.AmbientTemp;
                //    Chillers[index - 1].IsEnableUsePurge = param.IsEnableUsePurge;
                //}

                CurViewGUID = this.ViewModelManager().MainScreenView.ScreenGUID;

                EnvControlManager = this.EnvControlManager();
                

                if (CurViewGUID.ToString().ToUpper() == "7797A3B8-5CF5-FCCD-22BB-F612618C4B34" || CurViewGUID.ToString().ToUpper() == "3817ed23-c61a-47a1-8e97-6fc2c3a210c4")
                {
                    //SelectedChiller = Chillers.FirstOrDefault();
                    SelectedChiller = EnvControlManager.ChillerManager.GetChillerModules().FirstOrDefault();
                }

                updateDatas = true;

                var valveManager = EnvControlManager?.ValveManager;
                if(valveManager != null)
                {
                    foreach (var stage in Stages)
                    {
                        if(stage.ValveStates == null)
                        {
                            stage.ValveStates = valveManager.GetValveStateOfStage(stage.Index);
                        }
                    }
                }

                //UpdateData();

                //_monitoringTimer = new System.Timers.Timer(1500);
                //_monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                //_monitoringTimer.Start();

                //bStopUpdateThread = false;

                //UpdateThread = new Thread(new ThreadStart(UpdateProc));
                //UpdateThread.Name = this.GetType().Name;
                //UpdateThread.Start();



                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                updateDatas = false;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }

        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }
        #endregion

        public void UpdateData()
        {
            try
            {
                foreach (var stage in Stages)
                {
                    var stageobj = LoaderCommManager.GetStage(stage.Index);
                    if ((stage.IsConnected = stageobj?.StageInfo?.IsConnected ?? false) == true)
                    {
                        var tempClient = LoaderCommManager.GetProxy<ITempControllerProxy>(stage.Index);

                        //stage.PV = tempClient?.GetTemperature() ?? 9999;
                        stage.PV = LoaderCommManager.GetStage(stage.Index).StageInfo.PV;
                        stage.SV = tempClient?.GetSetTemp() ?? 9999;
                        stage.DewPoint = LoaderCommManager.GetStage(stage.Index).StageInfo.DewPoint;
                        stage.MV = tempClient?.GetMV() ?? 9999;
                        //stage.DewPointTolerance = tempClient?.GetDewPointTolerance() ?? 9999;
                        stage.HeaterState = tempClient?.GetHeaterOutPutState() ?? false;
                        stage.TemperatureState = tempClient.GetTempControllerState();
                        if (stage.TemperatureState == EnumTemperatureState.Inactivated || stage.TemperatureState == EnumTemperatureState.Error)
                        {
                            stage.IsEmergencyError = true;
                        }
                        else
                        {
                            stage.IsEmergencyError = false;
                        }

                        if(stageobj.StageInfo.LotData != null)
                        {
                            stage.TempControlState = stageobj.StageInfo.LotData.TempState;
                        }
                    }

                    Thread.Sleep(33);
                }

                //if (SetupStages != null)
                //{
                //    foreach (var stage in SetupStages)
                //    {
                //        if (CurViewGUID.ToString() == "0c1ca138-115f-24a9-e172-378c0e6d9b28" & stage.IsSelected)
                //        {
                //            IGPLoader gpLoader = this.GetGPLoader();
                //            stage.CoolantInValveState = this.EnvControlManager().GetValveState(EnumValveType.IN, stage.Index);
                //            stage.CoolantOutValveState = this.EnvControlManager().GetValveState(EnumValveType.IN, stage.Index);
                //            stage.DryAirValveState = this.EnvControlManager().GetValveState(EnumValveType.DRYAIR, stage.Index);
                //            stage.DrainValveState = this.EnvControlManager().GetValveState(EnumValveType.DRAIN, stage.Index);
                //            if (EnvControlParam.ValveSysParam.ValveModuleType.Value == EnumValveModuleType.LOADER)
                //            {
                //                stage.PurgeValveState = this.EnvControlManager().GetValveState(EnumValveType.PURGE, stage.Index);
                //            }
                //            else if (EnvControlParam.ValveSysParam.ValveModuleType.Value == EnumValveModuleType.MODBUS)
                //            {
                //                stage.PurgeValveState = this.EnvControlManager().GetValveState(EnumValveType.MANUAL_PURGE, stage.Index);
                //            }
                //            stage.LeakValveState = this.EnvControlManager().GetValveState(EnumValveType.Leak, stage.Index);
                //            stage.InOutValvestate = stage.CoolantInValveState & stage.CoolantOutValveState;
                //        }
                //    }
                //}

                if (bUpdateErrorHandled == true)
                {
                    LoggerManager.Debug($"[GPChillerMainViewModel]Stage status update error recovered.");
                }

                bUpdateErrorHandled = false;
            }
            catch (Exception err)
            {
                if (bUpdateErrorHandled == false)
                {
                    LoggerManager.Exception(err);
                    bUpdateErrorHandled = true;
                }
            }
        }
        bool bUpdateErrorHandled = false;

        public void UpdateProc()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    if (updateDatas == true)
                    {
                        areUpdateEvent2.Reset();

                        UpdateData();

                        areUpdateEvent2.Set();
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        #region .. MainViewModel

        #region Property

        //private ObservableCollection<ChillerColdInfo> _Chillers
        //     = new ObservableCollection<ChillerColdInfo>();
        //public ObservableCollection<ChillerColdInfo> Chillers
        //{
        //    get { return _Chillers; }
        //    set
        //    {
        //        if (value != _Chillers)
        //        {
        //            _Chillers = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<StageColdInfo> _Stages
             = new ObservableCollection<StageColdInfo>();
        public ObservableCollection<StageColdInfo> Stages
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
        #endregion

        #region Command

        #region ChillerConnectCommand
        private AsyncCommand<object> _ChillerConnectCommand;
        public ICommand ChillerConnectCommand
        {
            get
            {
                if (null == _ChillerConnectCommand) _ChillerConnectCommand = new AsyncCommand<object>(ChillerConnectCommandFunc);
                return _ChillerConnectCommand;
            }
        }
        private async Task ChillerConnectCommandFunc(object parameter)
        {
            try
            {
                if (parameter is int)
                {
                    int chillerIdx = (int)parameter;
                    var chiller = EnvControlManager.ChillerManager.GetChillerModules().Find(x => x.ChillerInfo.Index == (int)parameter);
                    if (!chiller.IsConnected)
                    {
                        var retmsg = await this.MetroDialogManager().ShowMessageDialog("Information Message",
                           "Do you want to connect chiller?", EnumMessageStyle.AffirmativeAndNegative);
                        if (retmsg == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            var chillerObj = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                            if (chillerObj != null)
                            {
                                if (chillerObj.GetCommState() == EnumCommunicationState.DISCONNECT)
                                    chillerObj.Start();
                            }
                        }
                    }
                    else
                    {
                        var retmsg = await this.MetroDialogManager().ShowMessageDialog("Information Message",
                            "Do you want to disconnect chiller?", EnumMessageStyle.AffirmativeAndNegative);
                        if (retmsg == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            var chillerObj = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                            if (chillerObj != null)
                            {
                                chillerObj.DisConnect();
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


        private RelayCommand<object> _ShowChillerConnSetupCommand;
        public ICommand ShowChillerConnSetupCommand
        {
            get
            {
                if (null == _ShowChillerConnSetupCommand) _ShowChillerConnSetupCommand = new RelayCommand<object>(ShowChillerConnSetupCommandFunc);
                return _ShowChillerConnSetupCommand;
            }
        }
        private async void ShowChillerConnSetupCommandFunc(object parameter)
        {
            try
            {
                if (parameter is int)
                {
                    int chillerIdx = (int)parameter;
                    var chillermodule = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                    //if(chillermodule != null)
                    //{
                    //    if(chillermodule.ChillerInfo.SetOperationLockFalg == true)
                    //    {
                    //        chillermodule.ChillerInfo.SetOperationLockFalg = false;
                    //        chillermodule.SetOperatingLock(false, false);
                    //    }
                    //    else
                    //    {
                    //        chillermodule.ChillerInfo.SetOperationLockFalg = true;
                    //        chillermodule.SetOperatingLock(true, true);
                    //    }
                    //}

                    string windowName = $"Chiller #{chillerIdx} Setting Window";
                    bool bFindWindow = Win32APIWrapper.CheckWindowExists(windowName);
                    if (!bFindWindow && chillermodule != null)
                    {
                        ChillerConnSettingView chillerConnSettingView = null;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            chillerConnSettingView = new ChillerConnSettingView();
                            chillerConnSettingView.DataContext = new ChillerConnSettingDialgVM(chillermodule);
                            Window container = new Window();
                            container.Content = chillerConnSettingView;
                            container.Width = 800;
                            container.Height = 680;
                            container.WindowStyle = WindowStyle.ToolWindow;
                            container.Title = windowName;
                            container.Topmost = true;
                            container.VerticalAlignment = VerticalAlignment.Center;
                            container.HorizontalAlignment = HorizontalAlignment.Center;
                            container.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            container.Show();
                        });
                    }
                    // await this.MetroDialogManager().ShowWindow(chillerConnSettingView);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region SetupPageSwitchCommand

        private AsyncCommand _SetupPageSwitchCommand;
        public ICommand SetupPageSwitchCommand
        {
            get
            {
                if (null == _SetupPageSwitchCommand) _SetupPageSwitchCommand = new AsyncCommand(SetupPageSwitchCommandFunc);
                return _SetupPageSwitchCommand;
            }
        }
        private async Task SetupPageSwitchCommandFunc()
        {
            try
            {
                //SetupChiller = Chillers[SelectedChillerIndex - 1];
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetupStages.Clear();
                    foreach (var stageIdx in EnvControlParam.ChillerSysParam.ChillerParams[SelectedChillerIndex - 1].StageIndexs)
                    {
                        var stage = Stages.ToList<StageColdInfo>().Find(info => info.Index == stageIdx);
                        stage.IsSelected = true;
                        var colddata = this.EnvControlManager().GetEnvControlClient(stageIdx)?.GetRemoteColdData();
                        if (colddata != null)
                        {
                            stage.DryAirActivatableHighTemp = colddata.DryAirActivatableHighTemp;
                            stage.DewPointTolerance = colddata.DewPointTolerance;
                            stage.DewPointTimeOut = colddata.DewPointTimeOut;
                        }
                        SetupStages.Add(stage);
                    }
                });
                await this.ViewModelManager().ViewTransitionAsync(new Guid("0C1CA138-115F-24A9-E172-378C0E6D9B28"));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ChillerSVSetCommand ( UI 제거 )

        //private AsyncCommand<object> _ChillerSVSetCommand;
        //public ICommand ChillerSVSetCommand
        //{
        //    get
        //    {
        //        if (null == _ChillerSVSetCommand) _ChillerSVSetCommand = new AsyncCommand<object>(ChillerSVSetCommandFunc);
        //        return _ChillerSVSetCommand;
        //    }
        //}
        //private async Task ChillerSVSetCommandFunc(object parameter)
        //{
        //    try
        //    {
        //        //parameter is chiller index
        //        if (parameter is int)
        //        {
        //            bool notUseChillerFormStages = true;
        //            int chillerIdx = (int)parameter;
        //            var connectStages = Stages.ToList<StageColdInfo>().FindAll(info => info.ChillerNum == chillerIdx);
        //            var chiller = EnvControlManager.ChillerManager.GetChillerModules().Find(x => x.ChillerInfo.Index == (int)parameter);

        //            //foreach (var stage in connectStages)
        //            //{
        //            //    if (stage.IsConnected)
        //            //    {
        //            //        bool inValveState = this.EnvControlManager().GetValveState(EnumValveType.IN, stage.Index);
        //            //        bool outValveState = this.EnvControlManager().GetValveState(EnumValveType.OUT, stage.Index);
        //            //        bool purgeValveState = this.EnvControlManager().GetValveState(EnumValveType.PRUGE, stage.Index);
        //            //        bool drainValveState = this.EnvControlManager().GetValveState(EnumValveType.DRAIN, stage.Index);
        //            //        if((inValveState | outValveState | purgeValveState | drainValveState))
        //            //        {
        //            //            notUseChillerFormStages = false;
        //            //            return;
        //            //        }
        //            //    }
        //            //    else
        //            //        notUseChillerFormStages = true;
        //            //}

        //            if (notUseChillerFormStages)
        //            {
        //                this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx)?.SetTargetTemp(chiller.ChillerSVSetValue, ProberInterfaces.Temperature.TempValueType.HUBER);

        //                //Chillers[chillerIdx - 1].ChillerSetTemp = Chillers[chillerIdx - 1].ChillerSVSetValue;

        //            }
        //            else
        //            {
        //                await this.MetroDialogManager().ShowMessageDialog("Error Message", "Close the valves on all stages of the chiller and set them again.", EnumMessageStyle.Affirmative);

        //                chiller.ChillerSVSetValue = chiller.ChillerInfo.SetTemp;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        #endregion

        #region ChillerSVTextBoxClickCommand 

        private RelayCommand<Object> _ChillerSVTextBoxClickCommand;
        public ICommand ChillerSVTextBoxClickCommand
        {
            get
            {
                if (null == _ChillerSVTextBoxClickCommand) _ChillerSVTextBoxClickCommand = new RelayCommand<Object>(FuncChillerSVTextBoxClickCommand);
                return _ChillerSVTextBoxClickCommand;
            }
        }

        private void FuncChillerSVTextBoxClickCommand(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.SPECIAL, -50, 150);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region StageSVChangeLockUnLockCommand

        private AsyncCommand<object> _StageSVChangeLockUnLockCommand;
        public ICommand StageSVChangeLockUnLockCommand
        {
            get
            {
                if (null == _StageSVChangeLockUnLockCommand) _StageSVChangeLockUnLockCommand = new AsyncCommand<object>(StageSVChangeLockUnLockCommandFunc);
                return _StageSVChangeLockUnLockCommand;
            }
        }
        private async Task StageSVChangeLockUnLockCommandFunc(object parameter)
        {
            try
            {
                //var chiller = Chillers.ToList<ChillerColdInfo>().Find(info => info.Index == (int)parameter);
                var chiller = EnvControlManager.ChillerManager.GetChillerModules().Find(x => x.ChillerInfo.Index == (int)parameter);
                if (chiller != null)
                {
                    if (chiller.UnLockStageSvBtn)
                    {
                        chiller.UnLockStageSvBtn = false;
                        return;
                    }


                    var msgRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                    "Change the temperature settings of the stage using the chiller at once." +
                    " The settings of the stage that is not connected will not be changed and please note the usage.",
                    EnumMessageStyle.AffirmativeAndNegative);
                    if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        chiller.UnLockStageSvBtn = true;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region StageSVChangeCommand

        private AsyncCommand<object> _StageSVChangeCommand;
        public ICommand StageSVChangeCommand
        {
            get
            {
                if (null == _StageSVChangeCommand) _StageSVChangeCommand = new AsyncCommand<object>(StageSVChangeCommandFunc);
                return _StageSVChangeCommand;
            }
        }
        private async Task StageSVChangeCommandFunc(object parameter)
        {
            try
            {


                //var chiller = Chillers.ToList<ChillerColdInfo>().Find(info => info.Index == (int)parameter);
                var chiller = EnvControlManager.ChillerManager.GetChillerModules().Find(x => x.ChillerInfo.Index == (int)parameter);

                if (chiller != null)
                {
                    if (chiller.UnLockStageSvBtn)
                        chiller.UnLockStageSvBtn = false;


                    bool isAllConnectedCellMaintenanceMode = true;
                    List<int> canChangeTempCellIndexList = new List<int>();
                    List<int> canNotChangeTempCellIndexList = new List<int>();
                    foreach (var stageIdx in EnvControlParam.ChillerSysParam.ChillerParams[((int)parameter) - 1].StageIndexs)
                    {
                        var stage = Stages.ToList<StageColdInfo>().Find(info => info.Index == stageIdx);
                        if (stage.IsConnected)
                        {
                            var stageInfo = LoaderCommManager.GetStage(stageIdx);
                            if (stageInfo.StageMode == GPCellModeEnum.MAINTENANCE)
                            {
                                canChangeTempCellIndexList.Add(stageIdx);
                            }
                            else
                            {
                                canNotChangeTempCellIndexList.Add(stageIdx);
                                isAllConnectedCellMaintenanceMode = false;
                            }
                        }
                    }

                    //SetTemp to cell
                    if (isAllConnectedCellMaintenanceMode == false)
                    {
                        string stateStr = String.Join(", ", canNotChangeTempCellIndexList);
                        canChangeTempCellIndexList.Clear();
                        canNotChangeTempCellIndexList.Clear();
                        await this.MetroDialogManager().ShowMessageDialog("Error Message",
                            $"The temperature of the cell can be changed only when the chiller group is all set to maintenance mode. \rCheck the mode of the cell #{stateStr}.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        var msgRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message", "This can affect the chiller control valve on the stage. Do you want to continue?",
                            EnumMessageStyle.AffirmativeAndNegative);

                        if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            updateDatas = false;

                            areUpdateEvent2.WaitOne(10000);

                            foreach (var stageIdx in canChangeTempCellIndexList)
                            {
                                var tempClient = LoaderCommManager.GetProxy<ITempControllerProxy>(stageIdx);

                                if (tempClient != null)
                                {
                                    tempClient.SetSV(ProberInterfaces.Temperature.TemperatureChangeSource.TEMP_EXTERNAL, chiller.StageSetSV, willYouSaveSetValue: false);
                                }
                            }

                            canChangeTempCellIndexList.Clear();
                            canNotChangeTempCellIndexList.Clear();

                            // Update
                            UpdateData();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                updateDatas = true;
            }
        }

        #endregion

        #region ChillerActiveOPCommand

        private AsyncCommand<object> _ChillerActiveOPCommand;
        public ICommand ChillerActiveOPCommand
        {
            get
            {
                if (null == _ChillerActiveOPCommand) _ChillerActiveOPCommand = new AsyncCommand<object>(ChillerActiveOPCommandFunc);
                return _ChillerActiveOPCommand;
            }
        }
        private async Task ChillerActiveOPCommandFunc(object param)
        {
            try
            {
                var chiller = EnvControlManager.ChillerManager.GetChillerModules().Find(x => x.ChillerInfo.Index == (int)param);
                int chillerIdx = (int)param;
                if (chillerIdx != 0)
                {
                    var selectedchiller = chiller;
                    if (selectedchiller.IsConnected)
                    {
                        if (selectedchiller.ChillerInfo.CoolantActivate)
                        {
                            //STOP
                            var chillerObj = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                            if (chillerObj != null)
                            {
                                //확인메세지
                                var messageRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                                    "Stopping the chiller can affect running stages And chiller changed maintenance mode. Do you want to continue? ",
                                    EnumMessageStyle.AffirmativeAndNegative);
                                if (messageRet == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    foreach (var index in chillerObj.ChillerParam.StageIndexs)
                                    {
                                        var envClient = this.EnvControlManager().GetEnvControlClient(index);
                                        if (envClient != null)
                                        {
                                            envClient.SetChillerAbortMode(true);
                                        }
                                    }

                                    chillerObj.Inactivate();
                                }

                                selectedchiller.ChillerMaintenanceFlag = true;
                                selectedchiller.ChillerInfo.CoolantActivate = false;
                            }
                        }
                        else
                        {
                            //START
                            var chillerObj = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: chillerIdx);
                            if (chillerObj != null)
                            {
                                //확인메세지
                                var messageRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                                    "Restarting the chiller can affect stages who Stage Manual OP set to true. Do you want to continue?",
                                    EnumMessageStyle.AffirmativeAndNegative);
                                if (messageRet == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    if (selectedchiller.ChillerMaintenanceFlag)
                                    {
                                        foreach (var index in chillerObj.ChillerParam.StageIndexs)
                                        {
                                            var stageObj = Stages.ToList<StageColdInfo>().Find(stage => stage.Index == index);
                                            if (stageObj != null)
                                            {
                                                if (stageObj.ChillerMaintenanceActiveFalg)
                                                {
                                                    var envClient = this.EnvControlManager().GetEnvControlClient(index);
                                                    if (envClient != null)
                                                    {
                                                        envClient.SetChillerAbortMode(false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var index in chillerObj.ChillerParam.StageIndexs)
                                        {
                                            var envClient = this.EnvControlManager().GetEnvControlClient(index);
                                            if (envClient != null)
                                            {
                                                envClient.SetChillerAbortMode(false);
                                            }
                                        }
                                    }
                                    var retActivate = chillerObj.Activate();
                                    if (retActivate == EventCodeEnum.NONE)
                                    {
                                        selectedchiller.ChillerInfo.CoolantActivate = true;
                                    }
                                    else
                                    {
                                        await this.MetroDialogManager().ShowMessageDialog("Error Message",
                                            "Please try again after the chiller pump has stopped completely.",
                                            EnumMessageStyle.Affirmative);
                                    }
                                }
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

        #region StageEndTempEmergencyError

        private AsyncCommand<int> _StageEndTempEmergencyErrorCommand;
        public ICommand StageEndTempEmergencyErrorCommand
        {
            get
            {
                if (null == _StageEndTempEmergencyErrorCommand) _StageEndTempEmergencyErrorCommand = new AsyncCommand<int>(StageEndTempEmergencyErrorCommandFunc);
                return _StageEndTempEmergencyErrorCommand;
            }
        }
        private async Task StageEndTempEmergencyErrorCommandFunc(int stageindex)
        {
            try
            {
                var msgRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                    "Do you process to reset the temperature error state?", EnumMessageStyle.AffirmativeAndNegative);
                if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (stageindex != 0)
                    {
                        var stageObj = Stages.Where(stage => stage.Index == stageindex).FirstOrDefault();
                        if (stageObj != null)
                        {
                            this.LoaderCommManager.GetProxy<ITempControllerProxy>(stageindex).SetEndTempEmergencyErrorCommand();
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

        #region StageSetActivatedState

        private AsyncCommand<int> _StageSetActivatedStateCommand;
        public ICommand StageSetActivatedStateCommand
        {
            get
            {
                if (null == _StageSetActivatedStateCommand) _StageSetActivatedStateCommand = new AsyncCommand<int>(StageSetActivatedStateCommandFunc);
                return _StageSetActivatedStateCommand;
            }
        }
        private async Task StageSetActivatedStateCommandFunc(int stageindex)
        {
            try
            {
                if (stageindex != 0)
                {
                    var stageObj = Stages.Where(stage => stage.Index == stageindex).FirstOrDefault();
                    if (stageObj != null)
                    {
                        var chiller = EnvControlManager.ChillerManager.GetChillerModule(stageIndex: stageindex);
                        if (chiller != null)
                        {
                            double stageSV = stageObj.SV;
                            double stageTargetTemp = stageSV + chiller.GetChillerTempoffset(stageSV);
                            if (chiller.ChillerInfo.TargetTemp != stageTargetTemp)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Error Message",
                                "The temperature of the chiller that you want to set in the cell does not match the temperature of the chiller.\n It can only operate when the two temperatures match.", EnumMessageStyle.Affirmative);
                                return;
                            }
                        }
                    }
                }

                string text = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    text = VirtualKeyboard.Show(text, KB_TYPE.ALPHABET | KB_TYPE.PASSWORD);
                });
                String superPassword = AccountManager.MakeSuperAccountPassword();
                if (text.ToLower().CompareTo(superPassword) == 0)
                {
                    var msgRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                    "Do you want to activate coolant circualtion ?\nCan cause heat impact to activated cell(s). Do you want to continue?", EnumMessageStyle.AffirmativeAndNegative);
                    if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        var stageObj = Stages.Where(stage => stage.Index == stageindex).FirstOrDefault();
                        if (stageObj != null)
                        {
                            this.LoaderCommManager.GetProxy<ITempControllerProxy>(stageindex).SetActivatedState(true);
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

        #endregion

        #endregion

        #region .. Setup ViewModel

        #region Property
        //private ChillerColdInfo _SetupChiller;
        //public ChillerColdInfo SetupChiller
        //{
        //    get { return _SetupChiller; }
        //    set
        //    {
        //        if (value != _SetupChiller)
        //        {
        //            _SetupChiller = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<StageColdInfo> _SetupStages
             = new ObservableCollection<StageColdInfo>();
        public ObservableCollection<StageColdInfo> SetupStages
        {
            get { return _SetupStages; }
            set
            {
                if (value != _SetupStages)
                {
                    _SetupStages = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region Command

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(FuncTextBoxClickCommand);
                return _TextBoxClickCommand;
            }
        }

        private void FuncTextBoxClickCommand(Object param)
        {
            try
            {
                (System.Windows.Controls.TextBox, IElement) paramVal = ((System.Windows.Controls.TextBox, IElement))param;
                System.Windows.Controls.TextBox textbox = paramVal.Item1;
                IElement element = paramVal.Item2;

                if (textbox != null)
                {
                    if (element != null)
                    {
                        if (element.UpperLimit == 0)
                            element.UpperLimit = 200;
                        textbox.Text = VirtualKeyboard.Show(textbox.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, (int)element.LowerLimit, (int)element.UpperLimit);
                    }
                    else
                    {
                        textbox.Text = VirtualKeyboard.Show(textbox.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 150);
                    }
                    textbox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool CheckStageMaintananceMode(int index)
        {
            bool retVal = false;
            try
            {
                if (LoaderCommManager.Cells[index - 1].StageInfo.IsConnected)
                {
                    if (LoaderCommManager.Cells[index - 1].StageMode == GPCellModeEnum.MAINTENANCE)
                        retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private RelayCommand<Object> _StageTextBoxClickCommand;
        public ICommand StageTextBoxClickCommand
        {
            get
            {
                if (null == _StageTextBoxClickCommand) _StageTextBoxClickCommand = new RelayCommand<Object>(FuncStageTextBoxClickCommand);
                return _StageTextBoxClickCommand;
            }
        }

        private void FuncStageTextBoxClickCommand(Object param)
        {
            try
            {
                (System.Windows.Controls.TextBox, int) paramVal = ((System.Windows.Controls.TextBox, int))param;
                System.Windows.Controls.TextBox textbox = paramVal.Item1;
                textbox.Text = textbox.Text.Replace("℃", "").Trim();
                int index = paramVal.Item2;

                if (textbox != null)
                {

                    textbox.Text = VirtualKeyboard.Show(textbox.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 150);
                    textbox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    //Stage Data Update
                    var stage = SetupStages.ToList<StageColdInfo>().Find(info => info.Index == index);
                    if (stage != null)
                    {
                        RemoteStageColdSetupData data = new RemoteStageColdSetupData(stage.DewPointTolerance, stage.DryAirActivatableHighTemp, stage.DewPointTimeOut);
                        this.EnvControlManager().GetEnvControlClient(index)?.SetRemoteColdData(data);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private int ConvertIndex(int obj)
        {
            int stageIndex = 0;
            try
            {
                if (SystemManager.SystemType == SystemTypeEnum.Opera || SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    switch ((int)obj)
                    {
                        case 1:
                            stageIndex = 9; break;
                        case 2:
                            stageIndex = 10; break;
                        case 3:
                            stageIndex = 11; break;
                        case 4:
                            stageIndex = 12; break;
                        case 5:
                            stageIndex = 5; break;
                        case 6:
                            stageIndex = 6; break;
                        case 7:
                            stageIndex = 7; break;
                        case 8:
                            stageIndex = 8; break;
                        case 9:
                            stageIndex = 1; break;
                        case 10:
                            stageIndex = 2; break;
                        case 11:
                            stageIndex = 3; break;
                        case 12:
                            stageIndex = 4; break;
                        default:
                            break;
                    }
                }
                else if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    switch ((int)obj)
                    {
                        case 1:
                            stageIndex = 1; break;
                        case 2:
                            stageIndex = 2; break;
                        case 3:
                            stageIndex = 5; break;
                        case 4:
                            stageIndex = 6; break;
                        case 5:
                            stageIndex = 9; break;
                        case 6:
                            stageIndex = 10; break;
                        case 7:
                            stageIndex = 3; break;
                        case 8:
                            stageIndex = 4; break;
                        case 9:
                            stageIndex = 7; break;
                        case 10:
                            stageIndex = 8; break;
                        case 11:
                            stageIndex = 11; break;
                        case 12:
                            stageIndex = 12; break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stageIndex;
        }

        #region ..InValveOn & Off Command

        private AsyncCommand<object> _InValveOnCommand;
        public ICommand InValveOnCommand
        {
            get
            {
                if (null == _InValveOnCommand) _InValveOnCommand = new AsyncCommand<object>(InValveOnCommandFunc);
                return _InValveOnCommand;
            }
        }
        private async Task InValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.IN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _InValveOffCommand;
        public ICommand InValveOffCommand
        {
            get
            {
                if (null == _InValveOffCommand) _InValveOffCommand = new AsyncCommand<object>(InValveOffCommandFunc);
                return _InValveOffCommand;
            }
        }
        private async Task InValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.IN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region .. OutVlaveOn & Off Command

        private AsyncCommand<object> _OutValveOnCommand;
        public ICommand OutValveOnCommand
        {
            get
            {
                if (null == _OutValveOnCommand) _OutValveOnCommand = new AsyncCommand<object>(OutValveOnCommandFunc);
                return _OutValveOnCommand;
            }
        }
        private async Task OutValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.OUT;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _OutValveOffCommand;
        public ICommand OutValveOffCommand
        {
            get
            {
                if (null == _OutValveOffCommand) _OutValveOffCommand = new AsyncCommand<object>(OutValveOffCommandFunc);
                return _OutValveOffCommand;
            }
        }
        private async Task OutValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.OUT;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ..DryAir VlaveOn & Off Command

        private AsyncCommand<object> _DryAirValveOnCommand;
        public ICommand DryAirValveOnCommand
        {
            get
            {
                if (null == _DryAirValveOnCommand) _DryAirValveOnCommand = new AsyncCommand<object>(DryAirValveOnCommandFunc);
                return _DryAirValveOnCommand;
            }
        }
        private async Task DryAirValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.DRYAIR;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _DryAirValveOffCommand;
        public ICommand DryAirValveOffCommand
        {
            get
            {
                if (null == _DryAirValveOffCommand) _DryAirValveOffCommand = new AsyncCommand<object>(DryAirValveOffCommandFunc);
                return _DryAirValveOffCommand;
            }
        }
        private async Task DryAirValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.DRYAIR;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ..Purge VlaveOn & Off Command

        private AsyncCommand<object> _PurgeValveOnCommand;
        public ICommand PurgeValveOnCommand
        {
            get
            {
                if (null == _PurgeValveOnCommand) _PurgeValveOnCommand = new AsyncCommand<object>(PurgeValveOnCommandFunc);
                return _PurgeValveOnCommand;
            }
        }
        private async Task PurgeValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.PURGE;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _PurgeValveOffCommand;
        public ICommand PurgeValveOffCommand
        {
            get
            {
                if (null == _PurgeValveOffCommand) _PurgeValveOffCommand = new AsyncCommand<object>(PurgeValveOffCommandFunc);
                return _PurgeValveOffCommand;
            }
        }
        private async Task PurgeValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.PURGE;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ..DrainAir VlaveOn & Off Command

        private AsyncCommand<object> _DrainAirValveOnCommand;
        public ICommand DrainAirValveOnCommand
        {
            get
            {
                if (null == _DrainAirValveOnCommand) _DrainAirValveOnCommand = new AsyncCommand<object>(DrainAirValveOnCommandFunc);
                return _DrainAirValveOnCommand;
            }
        }
        private async Task DrainAirValveOnCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = true;
                EnumValveType valveType = EnumValveType.DRAIN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<object> _DrainAirValveOffCommand;
        public ICommand DrainAirValveOffCommand
        {
            get
            {
                if (null == _DrainAirValveOffCommand) _DrainAirValveOffCommand = new AsyncCommand<object>(DrainAirValveOffCommandFunc);
                return _DrainAirValveOffCommand;
            }
        }
        private async Task DrainAirValveOffCommandFunc(object obj)
        {
            try
            {
                int index = (int)obj; ;
                bool state = false;
                EnumValveType valveType = EnumValveType.DRAIN;

                Master.EnvControlManager().SetValveState(state, valveType, index);
                LoggerManager.Debug($"[GPChillerVM] SetValve. Cell : {index}, ValveType : {valveType}, State : {state}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private AsyncCommand _SaveChillerParamCommand;
        public ICommand SaveChillerParamCommand
        {
            get
            {
                if (null == _SaveChillerParamCommand) _SaveChillerParamCommand = new AsyncCommand(SaveChillerParamCommandFunc);
                return _SaveChillerParamCommand;
            }
        }

        public async Task SaveChillerParamCommandFunc()
        {
            var messageRet = await this.MetroDialogManager().ShowMessageDialog(
                "Warning Message",
                "This can affect stages using that chiller. Do you want to continue?",
                EnumMessageStyle.AffirmativeAndNegative);
            if (messageRet == EnumMessageDialogResult.AFFIRMATIVE)
            {
                //Chiller Env 원본 파라미터 변경 및 저장.
                var chillerParam = EnvControlParam.ChillerSysParam.ChillerParams[SelectedChillerIndex - 1];

                //chillerParam.ActivatableHighTemp.Value = SelectedChiller.ActivatableHighTemp.Value;
                //chillerParam.AmbientTemp.Value = SelectedChiller.AmbientTemp.Value;
                //chillerParam.ChillerHotLimitTemp.Value = SelectedChiller.ChillerHotLimitTemp.Value;
                //chillerParam.CoolantInTemp.Value = SelectedChiller.CoolantInTemp.Value;
                //chillerParam.InRangeWindowTemp.Value = SelectedChiller.InRangeWindowTemp.Value;
                //chillerParam.Tolerance.Value = SelectedChiller.Tolerance.Value;
                //chillerParam.IsEnableUsePurge.Value = SelectedChiller.IsEnableUsePurge.Value;

                chillerParam.ActivatableHighTemp.Value = SelectedChiller.ChillerParam.ActivatableHighTemp.Value;
                chillerParam.AmbientTemp.Value = SelectedChiller.ChillerParam.AmbientTemp.Value;
                chillerParam.ChillerHotLimitTemp.Value = SelectedChiller.ChillerParam.ChillerHotLimitTemp.Value;
                chillerParam.CoolantInTemp.Value = SelectedChiller.ChillerParam.CoolantInTemp.Value;
                chillerParam.InRangeWindowTemp.Value = SelectedChiller.ChillerParam.InRangeWindowTemp.Value;
                chillerParam.Tolerance.Value = SelectedChiller.ChillerParam.Tolerance.Value;
                chillerParam.IsEnableUsePurge.Value = SelectedChiller.ChillerParam.IsEnableUsePurge.Value;

                this.EnvControlManager().EnvSysParam = EnvControlParam;
                this.EnvControlManager().SaveSysParameter();

                //Stage 들에 Chiller Param 다시 넘겨주기.
                foreach (var stage in SetupStages)
                {
                    if (stage.IsConnected)
                    {
                        var stageClient = this.EnvControlManager().GetEnvControlClient(stage.Index);
                        if (stageClient != null)
                        {
                            var chillerModule = this.EnvControlManager().ChillerManager.GetChillerModule(chillerIndex: SelectedChillerIndex);
                            if (chillerModule != null)
                            {
                                stageClient.SetChillerData(chillerModule.GetChillerParam(), true);
                            }
                        }
                    }
                }
            }
            else
            {
                //if (EnvControlParam.ChillerSysParam.ChillerParams.Count > SelectedChillerIndex - 1)
                //{
                //    var chillerParam = EnvControlParam.ChillerSysParam.ChillerParams[SelectedChillerIndex - 1];
                //    Chillers[SelectedChillerIndex - 1].ChillerHotLimitTemp = chillerParam.ChillerHotLimitTemp;
                //    Chillers[SelectedChillerIndex - 1].CoolantInTemp = chillerParam.CoolantInTemp;
                //    Chillers[SelectedChillerIndex - 1].Tolerance = chillerParam.Tolerance;
                //    Chillers[SelectedChillerIndex - 1].ActivatableHighTemp = chillerParam.ActivatableHighTemp;
                //    Chillers[SelectedChillerIndex - 1].InRangeWindowTemp = chillerParam.InRangeWindowTemp;
                //    Chillers[SelectedChillerIndex - 1].AmbientTemp = chillerParam.AmbientTemp;
                //    Chillers[SelectedChillerIndex - 1].IsEnableUsePurge = chillerParam.IsEnableUsePurge;
                //}
            }
        }

        #endregion

        #endregion
    }

    //public class ChillerColdInfo : INotifyPropertyChanged
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void RaisePropertyChanged([CallerMemberName] string propName = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    //    }
    //    #endregion

    //    public ChillerColdInfo(int index)
    //    {
    //        Index = index;
    //    }

    //    #region .. Chiller Info

    //    private int _Index;
    //    public int Index
    //    {
    //        get { return _Index; }
    //        set
    //        {
    //            if (value != _Index)
    //            {
    //                _Index = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _IsConnected;
    //    public bool IsConnected
    //    {
    //        get { return _IsConnected; }
    //        set
    //        {
    //            if (value != _IsConnected)
    //            {
    //                _IsConnected = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _ChillerInternalTemp;
    //    public double ChillerInternalTemp
    //    {
    //        get { return _ChillerInternalTemp; }
    //        set
    //        {
    //            if (value != _ChillerInternalTemp)
    //            {
    //                _ChillerInternalTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _ChillerProcessTemp;
    //    public double ChillerProcessTemp
    //    {
    //        get { return _ChillerProcessTemp; }
    //        set
    //        {
    //            if (value != _ChillerProcessTemp)
    //            {
    //                _ChillerProcessTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _ChillerSetTemp;
    //    public double ChillerSetTemp
    //    {
    //        get { return _ChillerSetTemp; }
    //        set
    //        {
    //            if (value != _ChillerSetTemp)
    //            {
    //                _ChillerSetTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _ChillerSVSetValue;
    //    //TextBox Binding Property
    //    public double ChillerSVSetValue
    //    {
    //        get { return _ChillerSVSetValue; }
    //        set
    //        {
    //            if (value != _ChillerSVSetValue)
    //            {
    //                _ChillerSVSetValue = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }


    //    private double _SerialNum;
    //    public double SerialNum
    //    {
    //        get { return _SerialNum; }
    //        set
    //        {
    //            if (value != _SerialNum)
    //            {
    //                _SerialNum = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _CommState;
    //    public bool CommState
    //    {
    //        get { return _CommState; }
    //        set
    //        {
    //            if (value != _CommState)
    //            {
    //                _CommState = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _ActiveFlag;
    //    public bool ActiveFlag
    //    {
    //        get { return _ActiveFlag; }
    //        set
    //        {
    //            if (value != _ActiveFlag)
    //            {
    //                _ActiveFlag = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    //Chiller State (Heating/ Cooling ...)

    //    #endregion

    //    #region Chiller Parameter
    //    private Element<double> _ChillerHotLimitTemp;
    //    public Element<double> ChillerHotLimitTemp
    //    {
    //        get { return _ChillerHotLimitTemp; }
    //        set
    //        {
    //            if (value != _ChillerHotLimitTemp)
    //            {
    //                _ChillerHotLimitTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private Element<double> _CoolantInTemp
    //        = new Element<double>();
    //    public Element<double> CoolantInTemp
    //    {
    //        get { return _CoolantInTemp; }
    //        set
    //        {
    //            if (value != _CoolantInTemp)
    //            {
    //                _CoolantInTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }


    //    private Element<double> _ChillerOffset
    //         = new Element<double>() { Value = -5.0 };
    //    public Element<double> ChillerOffset
    //    {
    //        get { return _ChillerOffset; }
    //        set
    //        {
    //            if (value != _ChillerOffset)
    //            {
    //                _ChillerOffset = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private Element<double> _Tolerance
    //         = new Element<double>() { Value = 0 };
    //    public Element<double> Tolerance
    //    {
    //        get { return _Tolerance; }
    //        set
    //        {
    //            if (value != _Tolerance)
    //            {
    //                _Tolerance = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private Element<double> _ActivatableHighTemp
    //         = new Element<double>() { Value = 30.0 };
    //    public Element<double> ActivatableHighTemp
    //    {
    //        get { return _ActivatableHighTemp; }
    //        set
    //        {
    //            if (value != _ActivatableHighTemp)
    //            {
    //                _ActivatableHighTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private Element<double> _InRangeWindowTemp
    //         = new Element<double>() { Value = 5.0 };
    //    public Element<double> InRangeWindowTemp
    //    {
    //        get { return _InRangeWindowTemp; }
    //        set
    //        {
    //            if (value != _InRangeWindowTemp)
    //            {
    //                _InRangeWindowTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }


    //    private Element<double> _AmbientTemp
    //        = new Element<double>() { Value = 26.0 };
    //    public Element<double> AmbientTemp
    //    {
    //        get { return _AmbientTemp; }
    //        set
    //        {
    //            if (value != _AmbientTemp)
    //            {
    //                _AmbientTemp = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private Element<bool> _IsEnableUsePurge
    //        = new Element<bool>() { Value = false };
    //    public Element<bool> IsEnableUsePurge
    //    {
    //        get { return _IsEnableUsePurge; }
    //        set
    //        {
    //            if (value != _IsEnableUsePurge)
    //            {
    //                _IsEnableUsePurge = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }


    //    #endregion

    //    private double _StageSetSV;
    //    public double StageSetSV
    //    {
    //        get { return _StageSetSV; }
    //        set
    //        {
    //            if (value != _StageSetSV)
    //            {
    //                _StageSetSV = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _UnLockStageSvBtn = false;
    //    public bool UnLockStageSvBtn
    //    {
    //        get { return _UnLockStageSvBtn; }
    //        set
    //        {
    //            if (value != _UnLockStageSvBtn)
    //            {
    //                _UnLockStageSvBtn = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private bool _ChillerMaintenanceFlag = false;
    //    public bool ChillerMaintenanceFlag
    //    {
    //        get { return _ChillerMaintenanceFlag; }
    //        set
    //        {
    //            if (value != _ChillerMaintenanceFlag)
    //            {
    //                _ChillerMaintenanceFlag = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //}

    public class StageColdInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public StageColdInfo(int index)
        {
            Index = index;
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }


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

        private double _MV;
        public double MV
        {
            get { return _MV; }
            set
            {
                if (value != _MV)
                {
                    _MV = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _DewPoint;
        public double DewPoint
        {
            get { return _DewPoint; }
            set
            {
                if (value != _DewPoint)
                {
                    _DewPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DewPointTolerance;
        public double DewPointTolerance
        {
            get { return _DewPointTolerance; }
            set
            {
                if (value != _DewPointTolerance)
                {
                    _DewPointTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DewPointOffset;
        public double DewPointOffset
        {
            get { return _DewPointOffset; }
            set
            {
                if (value != _DewPointOffset)
                {
                    _DewPointOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DewPointTimeOut;
        public double DewPointTimeOut
        {
            get { return _DewPointTimeOut; }
            set
            {
                if (value != _DewPointTimeOut)
                {
                    _DewPointTimeOut = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _SV;
        public double SV
        {
            get { return _SV; }
            set
            {
                if (value != _SV)
                {
                    _SV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PV;
        public double PV
        {
            get { return _PV; }
            set
            {
                if (value != _PV)
                {
                    _PV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _HeaterState;
        public bool HeaterState
        {
            get { return _HeaterState; }
            set
            {
                if (value != _HeaterState)
                {
                    _HeaterState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _TempControlState;
        public String TempControlState
        {
            get { return _TempControlState; }
            set
            {
                if (value != _TempControlState)
                {
                    _TempControlState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _ChillerNum;
        public int ChillerNum
        {
            get { return _ChillerNum; }
            set
            {
                if (value != _ChillerNum)
                {
                    _ChillerNum = value;
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

        private bool _ChillerMaintenanceActiveFalg = true;
        public bool ChillerMaintenanceActiveFalg
        {
            get { return _ChillerMaintenanceActiveFalg; }
            set
            {
                if (value != _ChillerMaintenanceActiveFalg)
                {
                    _ChillerMaintenanceActiveFalg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEmergencyError;
        public bool IsEmergencyError
        {
            get { return _IsEmergencyError; }
            set
            {
                if (value != _IsEmergencyError)
                {
                    _IsEmergencyError = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _DryAirActivatableHighTemp;
        public double DryAirActivatableHighTemp
        {
            get { return _DryAirActivatableHighTemp; }
            set
            {
                if (value != _DryAirActivatableHighTemp)
                {
                    _DryAirActivatableHighTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ValveStateOfStage _ValveStates;
        public ValveStateOfStage ValveStates
        {
            get { return _ValveStates; }
            set
            {
                if (value != _ValveStates)
                {
                    _ValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumTemperatureState _TemperatureState;
        public EnumTemperatureState TemperatureState
        {
            get { return _TemperatureState; }
            set
            {
                if (value != _TemperatureState)
                {
                    _TemperatureState = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
}
