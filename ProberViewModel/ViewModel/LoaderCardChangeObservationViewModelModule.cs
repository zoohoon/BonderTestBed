using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.FactoryModules.ViewModelModule;
using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Param;
using ProberInterfaces.ViewModel;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace LoaderCardChangeObservationViewModelModule
{
    public class LoaderCardChangeObservationViewModelModule : INotifyPropertyChanged, IFactoryModule, IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region ..IMainScreenViewModel
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        public ILoaderSupervisor LoaderMaster => this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
        public ILoaderCommunicationManager LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        readonly Guid _ViewModelGUID = new Guid("00685057-d747-45ab-a0bc-15bd91772fbc");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set { _DisplayPort = value; }
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
        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    //_VacuumUpdateTimer.Stop();
                    VacUpdateThread.Join();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        Thread VacUpdateThread;
        public EventCodeEnum InitModule()
        {
            try
            {
                //_VacuumUpdateTimer = new System.Timers.Timer();
                //_VacuumUpdateTimer.Interval = VacUpdateInterValInms;
                //_VacuumUpdateTimer.Elapsed += _VacUpdateTimer_Elapsed;
                VacUpdateThread = new Thread(new ThreadStart(VacUpdateData));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        private void VacUpdateData()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    var vacdata = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                    if (vacdata != null)
                    {
                        this.IsCardPodVac = vacdata.CardOnPogoPod;
                        this.IsTesterVac = vacdata.TesterPogoTouched;
                        this.IsPogoVac = vacdata.CardPogoTouched;
                        this.IsCardPodUP = vacdata.IsLeftUpModuleUp || vacdata.IsRightUpModuleUp;
                    }

                    bool isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand(false);
                    bool isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand(false);
                    if (isOpen == true && isClose == false)
                    {
                        IsDoorOpen = true;
                    }
                    else
                    {
                        IsDoorOpen = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        //System.Timers.Timer _VacuumUpdateTimer;
        //private static int VacUpdateInterValInms = 250;

        //private void _VacUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{

        //    try
        //    {
        //        if (_RemoteMediumProxy != null)
        //        {
        //            VacUpdateInterValInms = 2000;
        //            _VacuumUpdateTimer.Interval = VacUpdateInterValInms;
        //            //await Task.Run(() =>
        //            //{
        //                var vacdata = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
        //                if (vacdata != null)
        //                {
        //                    this.IsCardPodVac = vacdata.CardOnPogoPod;
        //                    this.IsTesterVac = vacdata.TesterPogoTouched;
        //                    this.IsPogoVac = vacdata.CardPogoTouched;
        //                    this.IsCardPodUP = vacdata.IsLeftUpModuleUp || vacdata.IsRightUpModuleUp;
        //                }
        //            //});
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private IStageObject _SelectedStage;
        public IStageObject SelectedStage
        {
            get { return _SelectedStage; }
            set
            {
                if (value != _SelectedStage)
                {
                    _SelectedStage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateClickToMoveEnable()
        {
            try
            {
                // Pogo Tab Click 상태 -> Arm 이 들어와 있으면 Move 할 수 없음. 
                if (CARDSetupBtnEnable && !POGOSetupBtnEnable)
                {
                    EventCodeEnum retVal = LoaderMaster.IsShutterClose(this._LoaderCommunicationManager.SelectedStage.Index);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        EnableClickToMove = true;
                    }
                    else
                    {
                        EnableClickToMove = false;
                    }
                }
                // Card Tab Click 상태 -> Card Pod 이 올라와 있으면 Move 할 수 없음.
                else
                {
                    bool isPodUp = false;
                    isPodUp = _RemoteMediumProxy.GPCC_OP_IsLCardExistCommand();
                    if (isPodUp)
                    {
                        EnableClickToMove = false;
                    }
                    else
                    {
                        EnableClickToMove = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.GPCC_Observation_PageSwitchCommand();

                    Manual_ZTickValue = 1;//==> 10^1, ZValue
                    Manual_PZTickValue = 1;//==> 10^1, ZValue

                    await Task.Run(() =>
                    {
                        var vacstatus = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                        this.IsCardPodVac = vacstatus.CardOnPogoPod;
                        this.IsTesterVac = vacstatus.TesterPogoTouched;
                        this.IsPogoVac = vacstatus.CardPogoTouched;
                        this.IsCardPodUP = vacstatus.IsLeftUpModuleUp || vacstatus.IsRightUpModuleUp;
                        bool isOpen = false;
                        bool isClose = false;
                        isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand();
                        isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand();
                        if (isOpen == true && isClose == false)
                        {
                            IsDoorOpen = true;
                        }
                        else
                        {
                            IsDoorOpen = false;
                        }
                    });
                    var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                    CardID = null;
                    string probeCardID = LoaderMaster.Loader.LoaderMaster.CardIDLastTwoWord;

                    if (probeCardID == null)
                    {
                        probeCardID = "DefaultCard";
                    }
                    CardID = probeCardID;
                    ProberCardListParameter proberCard = sysdata.ProberCardList.First(x => x.CardID == probeCardID);

                    //CardCenterOffsetX1 = sysdata.GP_CardCenterOffsetX1;
                    //CardCenterOffsetX2 = sysdata.GP_CardCenterOffsetX2;
                    //CardCenterOffsetY1 = sysdata.GP_CardCenterOffsetY1;
                    //CardCenterOffsetY2 = sysdata.GP_CardCenterOffsetY2;
                    CardCenterOffsetX1 = proberCard.FiducialMarInfos[0].CardCenterOffset.GetX();
                    CardCenterOffsetX2 = proberCard.FiducialMarInfos[1].CardCenterOffset.GetX();
                    CardCenterOffsetY1 = proberCard.FiducialMarInfos[0].CardCenterOffset.GetY();
                    CardCenterOffsetY2 = proberCard.FiducialMarInfos[1].CardCenterOffset.GetY();

                    CardPodCenterX = sysdata.GP_CardPodCenterX;
                    CardPodCenterY = sysdata.GP_CardPodCenterY;

                    CardLoadZLimit = sysdata.GP_CardLoadZLimit;
                    CardLoadZInterval = sysdata.GP_CardLoadZInterval;
                    CardUnloadZOffset = sysdata.GP_CardUnloadZOffset;
                    PatternFocusingRange = sysdata.GP_CardFocusRange;
                    PatternRetryCount = sysdata.GP_CardAlignRetryCount;

                    DockOffsetZ = sysdata.GP_ContactCorrectionZ;
                    UndockOffsetZ = sysdata.GP_UndockCorrectionZ;

                    PogoAlignPoint = sysdata.PogoAlignPoint;

                    if (VacUpdateThread.ThreadState == ThreadState.Unstarted)
                    {
                        VacUpdateThread.Start();
                    }
                    else if (VacUpdateThread.ThreadState == ThreadState.Suspended)
                    {
                        VacUpdateThread.Resume();
                    }

                    await _RemoteMediumProxy.GPCC_Observation_PogoAlignPointCommand(PogoAlignPoint);

                    Task task = new Task(() =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            CARDSetupBtnEnable = true;
                            POGOSetupBtnEnable = false;
                            AssignedCamera = (this.ViewModelManager() as ILoaderViewModelManager).Camera;
                            GetMarkPositionList();
                            UpdateBindingData();
                            UpdateClickToMoveEnable();
                            SelectedTabIndex = 0;
                        }));
                    });
                    task.Start();
                    await task;

                    SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];
                    //_VacuumUpdateTimer.Start();

                    //await Task.Run(() =>
                    //{
                    //    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    //    {

                    //    }));
                    //});
                    //return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;

        }
        private void UpdateBindingData()
        {
            if (_RemoteMediumProxy != null)
            {
                var data = _RemoteMediumProxy.GPCC_Observation_GetGPCCDataCommand();
                if (data != null)
                {
                    this.PatternWidth = data.PatternWidth;
                    this.PatternHeight = data.PatternHeight;
                    this.SelectedProberCam = data.SelectCam;
                    this.LightValue = data.LightValue;
                    this.ZTickValue = data.ZTickValue;
                    this.ZDistanceValue = data.ZDistanceValue;
                    //   this.LightTickValue = data.LightTickValue;
                    this.SelectedLightType = data.SelectedLightType;
                    this.ZActualPos = Math.Round(data.ZActualPos, 2);
                    this.PZActualPos = Math.Round(data.PZActualPos, 2);
                }
            }

        }

        private void UpdateLightValue()
        {
            if (_RemoteMediumProxy != null)
            {
                // Prober의 LightValue 값이 변경 완료된 시점을 알 수 없음 -> retry (최대 7500ms)
                for (int retry = 0; retry < 15; retry++)
                {
                    ushort val;
                    EnumLightType type;

                    var data = _RemoteMediumProxy.GPCC_Observation_GetGPCCDataCommand();
                    if (data != null)
                    {
                        val = data.LightValue;
                        type = data.SelectedLightType;

                        if ((LightValue != val && SelectedLightType != type) ||
                            SelectedLightType == EnumLightType.UNDEFINED)
                        {
                            this.LightValue = val;
                            break;
                        }
                        else
                        {
                            // 각 Type의 LightValue가 실제로 동일한 경우, 
                            // 실제 설정값이 동일한건지 아니면 아직 LightValue가 업데이트되지 않은 건지 알 수 없음 -> 딜레이 발생
                        }
                    }

                    Thread.Sleep(50);
                }
            }
        }

        private void GetMarkPositionList()
        {
            if (_RemoteMediumProxy != null)
            {
                MarkPositionList = new ObservableCollection<MarkPosition>();
                MarkPositionList.Clear();
                var data = _RemoteMediumProxy.GPCC_Observation_GetGPCCDataCommand();
                if (data != null)
                {
                    foreach (var markpos in data.MarkPositions)
                    {
                        CatCoordinates tempmarkpos = new CatCoordinates(markpos.XPos, markpos.YPos, markpos.ZPos, markpos.TPos);
                        MarkPosition markinfo = new MarkPosition(data.SelectCam, tempmarkpos, markpos.Index);
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            MarkPositionList.Add(markinfo);
                        }));
                    }
                }
            }
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.GPCC_Observation_CleanUpCommand();
                    //_VacuumUpdateTimer.Stop();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                VacUpdateThread.Suspend();
            }
            return EventCodeEnum.NONE;
            //return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        public IStageSupervisor StageSupervisor => this.StageSupervisor();

        #region //Command CardSetting

        private AsyncCommand _CardSettingCommand;
        public ICommand CardSettingCommand
        {
            get
            {
                if (null == _CardSettingCommand) _CardSettingCommand = new AsyncCommand(CardSettingCommandFunc);
                return _CardSettingCommand;
            }
        }
        public async Task CardSettingCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_Observation_CardSettingCommand();

                Task task = new Task(() =>
                {
                    CARDSetupBtnEnable = false;
                    POGOSetupBtnEnable = true;
                    PodSetupBtnEnable = true;
                    GetMarkPositionList();
                    UpdateBindingData();
                    UpdateClickToMoveEnable();
                });
                task.Start();
                await task;
            }
        }
        #endregion

        #region //PogoSetting Command

        private AsyncCommand _PogoSettingCommand;
        public IAsyncCommand PogoSettingCommand
        {
            get
            {
                if (null == _PogoSettingCommand) _PogoSettingCommand = new AsyncCommand(PogoSettingCommandFunc);
                return _PogoSettingCommand;
            }
        }
        public async Task PogoSettingCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_Observation_PogoSettingCommand();

                Task task = new Task(() =>
                {
                    CARDSetupBtnEnable = true;
                    POGOSetupBtnEnable = false;
                    GetMarkPositionList();
                    UpdateBindingData();
                    UpdateClickToMoveEnable();
                });
                task.Start();
                await task;
            }
        }
        #endregion
        #region //PodSetting Command

        private AsyncCommand _PodSettingCommand;
        public IAsyncCommand PodSettingCommand
        {
            get
            {
                if (null == _PodSettingCommand) _PodSettingCommand = new AsyncCommand(PodSettingCommandFunc);
                return _PodSettingCommand;
            }
        }
        public async Task PodSettingCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_Observation_PodSettingCommand();

                Task task = new Task(() =>
                {
                    CARDSetupBtnEnable = true;
                    PodSetupBtnEnable = false;
                    POGOSetupBtnEnable = true;
                    GetMarkPositionList();
                    UpdateBindingData();
                });
                task.Start();
                await task;
            }
        }
        #endregion
        #region //PatternWidthPlusCommand

        private RelayCommand _PatternWidthPlusCommand;
        public ICommand PatternWidthPlusCommand
        {
            get
            {
                if (null == _PatternWidthPlusCommand) _PatternWidthPlusCommand = new RelayCommand(PatternWidthPlusCommandFunc);
                return _PatternWidthPlusCommand;
            }
        }
        public void PatternWidthPlusCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                _RemoteMediumProxy.GPCC_Observation_PatternWidthPlusCommand();
                UpdateBindingData();
            }
        }
        #endregion

        #region //PatternWidthMinusCommand

        private RelayCommand _PatternWidthMinusCommand;
        public ICommand PatternWidthMinusCommand
        {
            get
            {
                if (null == _PatternWidthMinusCommand) _PatternWidthMinusCommand = new RelayCommand(PatternWidthMinusCommandFunc);
                return _PatternWidthMinusCommand;
            }
        }
        public void PatternWidthMinusCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                _RemoteMediumProxy.GPCC_Observation_PatternWidthMinusCommand();
                UpdateBindingData();

                //await Task.Run(() =>
                //{
                //    _RemoteMediumProxy.GPCC_Observation_PatternWidthMinusCommand();
                //    UpdateBindingData();
                //});
            }
        }
        #endregion

        #region //PatternHeightPlusCommand

        private RelayCommand _PatternHeightPlusCommand;
        public ICommand PatternHeightPlusCommand
        {
            get
            {
                if (null == _PatternHeightPlusCommand) _PatternHeightPlusCommand = new RelayCommand(PatternHeightPlusCommandFunc);
                return _PatternHeightPlusCommand;
            }
        }
        public void PatternHeightPlusCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                _RemoteMediumProxy.GPCC_Observation_PatternHeightPlusCommand();
                UpdateBindingData();

                //await Task.Run(() =>
                //{
                //    _RemoteMediumProxy.GPCC_Observation_PatternHeightPlusCommand();
                //    UpdateBindingData();
                //});
            }
        }
        #endregion

        #region //PatternHeightMinusCommand

        private RelayCommand _PatternHeightMinusCommand;
        public ICommand PatternHeightMinusCommand
        {
            get
            {
                if (null == _PatternHeightMinusCommand) _PatternHeightMinusCommand = new RelayCommand(PatternHeightMinusCommandFunc);
                return _PatternHeightMinusCommand;
            }
        }
        public void PatternHeightMinusCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                _RemoteMediumProxy.GPCC_Observation_PatternHeightMinusCommand();
                UpdateBindingData();

                //await Task.Run(() =>
                //{
                //    _RemoteMediumProxy.GPCC_Observation_PatternHeightMinusCommand();
                //    UpdateBindingData();
                //});
            }
        }
        #endregion

        #region //WaferCamExtendCommand

        private AsyncCommand _WaferCamExtendCommand;
        public IAsyncCommand WaferCamExtendCommand
        {
            get
            {
                if (null == _WaferCamExtendCommand) _WaferCamExtendCommand = new AsyncCommand(WaferCamExtendCommandFunc);
                return _WaferCamExtendCommand;
            }
        }
        public async Task WaferCamExtendCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();

                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_WaferCamExtendCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }


            }
        }
        #endregion

        #region //WaferCamFoldCommand

        private AsyncCommand _WaferCamFoldCommand;
        public IAsyncCommand WaferCamFoldCommand
        {
            get
            {
                if (null == _WaferCamFoldCommand) _WaferCamFoldCommand = new AsyncCommand(WaferCamFoldCommandFunc);
                return _WaferCamFoldCommand;
            }
        }
        public async Task WaferCamFoldCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_WaferCamFoldCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }
            }
        }
        #endregion

        #region //MoveToCenterCommand

        private AsyncCommand _MoveToCenterCommand;
        public ICommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new AsyncCommand(MoveToCenterCommandFunc);
                return _MoveToCenterCommand;
            }
        }
        public async Task MoveToCenterCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_MoveToCenterCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }


            }
        }
        #endregion

        #region //ReadyToGetCardCommand

        private AsyncCommand _ReadyToGetCardCommand;
        public IAsyncCommand ReadyToGetCardCommand
        {
            get
            {
                if (null == _ReadyToGetCardCommand) _ReadyToGetCardCommand = new AsyncCommand(ReadyToGetCardCommandFunc);
                return _ReadyToGetCardCommand;
            }
        }
        public async Task ReadyToGetCardCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_ReadyToGetCardCommand();

                    //await Task.Run(() =>
                    //{
                    //    _RemoteMediumProxy.GPCC_Observation_ReadyToGetCardCommand();

                    //});
                }
            }
        }
        #endregion

        #region //RaiseZCommand

        private AsyncCommand _RaiseZCommand;
        public IAsyncCommand RaiseZCommand
        {
            get
            {
                if (null == _RaiseZCommand) _RaiseZCommand = new AsyncCommand(RaiseZCommandFunc);
                return _RaiseZCommand;
            }
        }
        public async Task RaiseZCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_RaiseZCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }
            }
        }
        #endregion

        #region //SetMFModelLightsCommand

        private AsyncCommand _SetMFModelLightsCommand;
        public ICommand SetMFModelLightsCommand
        {
            get
            {
                if (null == _SetMFModelLightsCommand) _SetMFModelLightsCommand = new AsyncCommand(SetMFModelLightsFunc);
                return _SetMFModelLightsCommand;
            }
        }
        public async Task SetMFModelLightsFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_SetMFModelLightsCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }


            }
        }
        #endregion
        #region //SetMFChildLightsCommand

        private AsyncCommand _SetMFChildLightsCommand;
        public ICommand SetMFChildLightsCommand
        {
            get
            {
                if (null == _SetMFChildLightsCommand) _SetMFChildLightsCommand = new AsyncCommand(SetMFChildLightsFunc);
                return _SetMFChildLightsCommand;
            }
        }
        public async Task SetMFChildLightsFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_SetMFChildLightsCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }
            }
        }
        #endregion
        #region //DropZCommand

        private AsyncCommand _DropZCommand;
        public ICommand DropZCommand
        {
            get
            {
                if (null == _DropZCommand) _DropZCommand = new AsyncCommand(DropZCommandFunc);
                return _DropZCommand;
            }
        }
        public async Task DropZCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_DropZCommand();


                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;

                    //await Task.Run(() =>
                    //{
                    //    UpdateBindingData();
                    //});
                }


            }
        }
        #endregion

        #region //IncreaseLightIntensityCommand

        private RelayCommand _IncreaseLightIntensityCommand;
        public ICommand IncreaseLightIntensityCommand
        {
            get
            {
                if (null == _IncreaseLightIntensityCommand) _IncreaseLightIntensityCommand = new RelayCommand(IncreaseLightIntensityCommandFunc);
                return _IncreaseLightIntensityCommand;
            }
        }
        public void IncreaseLightIntensityCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                _RemoteMediumProxy.GPCC_Observation_IncreaseLightIntensityCommand();
                UpdateBindingData();

                //await Task.Run(() =>
                //{
                //    _RemoteMediumProxy.GPCC_Observation_IncreaseLightIntensityCommand();
                //    UpdateBindingData();
                //});
            }
        }
        #endregion

        #region //DecreaseLightIntensityCommand

        private RelayCommand _DecreaseLightIntensityCommand;
        public ICommand DecreaseLightIntensityCommand
        {
            get
            {
                if (null == _DecreaseLightIntensityCommand) _DecreaseLightIntensityCommand = new RelayCommand(DecreaseLightIntensityCommandFunc);
                return _DecreaseLightIntensityCommand;
            }
        }
        public void DecreaseLightIntensityCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                _RemoteMediumProxy.GPCC_Observation_DecreaseLightIntensityCommand();
                UpdateBindingData();

                //await Task.Run(() =>
                //{
                //    _RemoteMediumProxy.GPCC_Observation_DecreaseLightIntensityCommand();
                //    UpdateBindingData();
                //});
            }
        }
        #endregion

        #region //RegisterPatternCommand

        private AsyncCommand _RegisterPatternCommand;
        public ICommand RegisterPatternCommand
        {
            get
            {
                if (null == _RegisterPatternCommand) _RegisterPatternCommand = new AsyncCommand(RegisterPatternCommandFunc);
                return _RegisterPatternCommand;
            }
        }
        public async Task RegisterPatternCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_RegisterPatternCommand();

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;
                }
            }
        }


        #endregion


        #region //RegisterPosCommand

        private AsyncCommand _RegisterPosCommand;
        public ICommand RegisterPosCommand
        {
            get
            {
                if (null == _RegisterPosCommand) _RegisterPosCommand = new AsyncCommand(RegisterPosCommandFunc);
                return _RegisterPosCommand;
            }
        }
        public async Task RegisterPosCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_RegisterPosCommand();

                    Task task = new Task(() =>
                    {
                        SelectedMarkPosition.Description = _RemoteMediumProxy.GPCC_OP_GetPosition();
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;
                }
            }
        }


        #endregion

        #region //AlignCommand

        private AsyncCommand _AlignCommand;
        public IAsyncCommand AlignCommand
        {
            get
            {
                if (null == _AlignCommand) _AlignCommand = new AsyncCommand(AlignCommandFunc);
                return _AlignCommand;
            }
        }
        public async Task AlignCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_Observation_AlignCommand();

                        await CardSettingCommandFunc();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //Focusing Command

        private AsyncCommand _FocusingCommand;
        public IAsyncCommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(FocusingCommandFunc);
                return _FocusingCommand;
            }
        }
        public async Task FocusingCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();

                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        GPCardChangeVMData ccdata;
                        //await Task.Run(() =>
                        //{
                        await _RemoteMediumProxy.GPCC_Observation_FocusingCommand();
                        //});
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //SelectedMarkPosiongChangedCommand

        private AsyncCommand _SelectedMarkPosiongChangedCommand;
        public IAsyncCommand SelectedMarkPosiongChangedCommand
        {
            get
            {
                if (null == _SelectedMarkPosiongChangedCommand) _SelectedMarkPosiongChangedCommand = new AsyncCommand(SelectedMarkPosiongChangedCommandFunc);
                return _SelectedMarkPosiongChangedCommand;
            }
        }
        public async Task SelectedMarkPosiongChangedCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    if (SelectedMarkPosition != null)
                    {
                        await _RemoteMediumProxy.GPCC_Observation_SetSelectedMarkPosCommand(SelectedMarkPosition.Index);

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //TabClickCommand

        private AsyncCommand<Object> _TabClickCommand;
        public IAsyncCommand TabClickCommand
        {
            get
            {
                if (null == _TabClickCommand) _TabClickCommand = new AsyncCommand<Object>(TabClickCommandFunc);
                return _TabClickCommand;
            }
        }
        public async Task TabClickCommandFunc(object header)
        {
            string tabName = header.ToString();
            try
            {
                if (tabName == "CC Setting")
                {
                    EnableClickToMove = false;
                }
                else
                {
                    UpdateClickToMoveEnable();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region //Binding Property

        public EnumProberCam SelectedProberCam { get; set; }

        #region ==> SelectedLightType
        private EnumLightType _SelectedLightType;
        public EnumLightType SelectedLightType
        {
            get { return _SelectedLightType; }
            set
            {
                if (value != _SelectedLightType)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_Observation_SetSelectLightTypeCommand(value);
                        UpdateLightValue();
                    }
                    
                    _SelectedLightType = value; // UpdateLightValue() 완료후 변경.
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> LightValue
        private ushort _LightValue;
        public ushort LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_Observation_SetLightValueCommand(_LightValue);
                    }
                    //UpdateBindingData();

                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> AssignedCamera
        private ICamera _AssignedCamera;
        public ICamera AssignedCamera
        {
            get { return _AssignedCamera; }
            set
            {
                if (value != _AssignedCamera)
                {
                    _AssignedCamera = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        private bool _CARDSetupBtnEnable;
        public bool CARDSetupBtnEnable
        {
            get { return _CARDSetupBtnEnable; }
            set
            {
                if (value != _CARDSetupBtnEnable)
                {
                    _CARDSetupBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _POGOSetupBtnEnable;
        public bool POGOSetupBtnEnable
        {
            get { return _POGOSetupBtnEnable; }
            set
            {
                if (value != _POGOSetupBtnEnable)
                {
                    _POGOSetupBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _PodSetupBtnEnable;
        public bool PodSetupBtnEnable
        {
            get { return _PodSetupBtnEnable; }
            set
            {
                if (value != _PodSetupBtnEnable)
                {
                    _PodSetupBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumPogoAlignPoint _PogoAlignPoint;
        public EnumPogoAlignPoint PogoAlignPoint
        {
            get { return _PogoAlignPoint; }
            set
            {
                if (value != _PogoAlignPoint)
                {
                    _PogoAlignPoint = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ==> MarkPositionList
        private ObservableCollection<MarkPosition> _MarkPositionList;
        public ObservableCollection<MarkPosition> MarkPositionList
        {
            get { return _MarkPositionList; }
            set
            {
                if (value != _MarkPositionList)
                {
                    _MarkPositionList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> 
        private IGPCCObservationMarkPosition _SelectedMarkPosition;
        public IGPCCObservationMarkPosition SelectedMarkPosition
        {
            get { return _SelectedMarkPosition; }
            set
            {
                if (value != _SelectedMarkPosition)
                {
                    _SelectedMarkPosition = value;
                    RaisePropertyChanged();

                    if (_SelectedMarkPosition == null)
                    {
                        return;
                    }

                    foreach (MarkPosition item in MarkPositionList)
                    {
                        if (item == _SelectedMarkPosition)
                        {
                            _SelectedMarkPosition.Index = item.Index;
                            continue;
                        }
                        item.ButtonEnable = false;
                    }
                    _SelectedMarkPosition.ButtonEnable = true;
                    //if (_RemoteMediumProxy != null)
                    //{
                    //    UpdateBindingData();
                    //    _RemoteMediumProxy.GPCC_Observation_SetSelectedMarkPosCommand(_SelectedMarkPosition);
                    //}
                }
            }
        }
        #endregion

        #region ==> PatternWidth
        private double _PatternWidth;
        public double PatternWidth
        {
            get { return _PatternWidth; }
            set
            {
                if (value != _PatternWidth)
                {
                    _PatternWidth = value;
                    //UpdateBindingData();

                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PatternHeight
        private double _PatternHeight;
        public double PatternHeight
        {
            get { return _PatternHeight; }
            set
            {
                if (value != _PatternHeight)
                {
                    _PatternHeight = value;
                    //UpdateBindingData();
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> ZTickValue
        private int _ZTickValue;
        public int ZTickValue
        {
            get { return _ZTickValue; }
            set
            {
                if (value != _ZTickValue)
                {
                    _ZTickValue = value;

                    ZDistanceValue = Math.Pow(10, _ZTickValue);
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_Observation_SetZTickValueCommand(_ZTickValue);
                    }
                    //UpdateBindingData();
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> Manual_ZTickValue
        private int _Manual_ZTickValue;
        public int Manual_ZTickValue
        {
            get { return _Manual_ZTickValue; }
            set
            {
                if (value != _Manual_ZTickValue)
                {
                    _Manual_ZTickValue = value;

                    Manual_ZDistanceValue = Math.Pow(10, _Manual_ZTickValue);
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Manual_ZDistanceValue
        private double _Manual_ZDistanceValue;
        public double Manual_ZDistanceValue
        {
            get { return _Manual_ZDistanceValue; }
            set
            {
                if (value != _Manual_ZDistanceValue)
                {
                    _Manual_ZDistanceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Manual_PZTickValue
        private int _Manual_PZTickValue;
        public int Manual_PZTickValue
        {
            get { return _Manual_PZTickValue; }
            set
            {
                if (value != _Manual_PZTickValue)
                {
                    _Manual_PZTickValue = value;

                    Manual_PZDistanceValue = Math.Pow(10, _Manual_PZTickValue);
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Manual_PZDistanceValue
        private double _Manual_PZDistanceValue;
        public double Manual_PZDistanceValue
        {
            get { return _Manual_PZDistanceValue; }
            set
            {
                if (value != _Manual_PZDistanceValue)
                {
                    _Manual_PZDistanceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> ZDistanceValue
        private double _ZDistanceValue;
        public double ZDistanceValue
        {
            get { return _ZDistanceValue; }
            set
            {
                if (value != _ZDistanceValue)
                {
                    _ZDistanceValue = value;

                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_Observation_SetZDistanceValueCommand(_ZDistanceValue);
                    }

                    //UpdateBindingData();
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LightTickValue
        private int _LightTickValue = 1; // VmGPCardChangeMainPage의 Tick 초기값과 동일하게 맞추어야함.
        public int LightTickValue
        {
            get { return _LightTickValue; }
            set
            {
                if (value != _LightTickValue)
                {
                    _LightTickValue = value;

                    if (_LightTickValue == 0)
                    {
                        SelectedLightType = EnumLightType.COAXIAL;
                    }
                    else
                    {
                        SelectedLightType = EnumLightType.OBLIQUE;
                    }

                    //Commandgogo 
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_Observation_SetLightTickValueCommand(_LightTickValue);
                    }
                    //if (this.VisionManager() != null)
                    //{
                    //    ICamera cam = this.VisionManager().GetCam(SelectedProberCam);
                    //    LightValue = (ushort)cam.GetLight(SelectedLightType);
                    //}

                    //UpdateBindingData();
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> ZActualPos
        private double _ZActualPos;
        public double ZActualPos
        {
            get { return _ZActualPos; }
            set
            {
                if (value != _ZActualPos)
                {
                    _ZActualPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PZActualPos
        private double _PZActualPos;
        public double PZActualPos
        {
            get { return _PZActualPos; }
            set
            {
                if (value != _PZActualPos)
                {
                    _PZActualPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> IsDoorOpen
        private bool _IsDoorOpen;
        public bool IsDoorOpen
        {
            get { return _IsDoorOpen; }
            set
            {
                if (value != _IsDoorOpen)
                {
                    _IsDoorOpen = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> IsCardPodUP
        private bool _IsCardPodUP;
        public bool IsCardPodUP
        {
            get { return _IsCardPodUP; }
            set
            {
                if (value != _IsCardPodUP)
                {
                    _IsCardPodUP = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> IsCardPodVac
        private bool _IsCardPodVac;
        public bool IsCardPodVac
        {
            get { return _IsCardPodVac; }
            set
            {
                if (value != _IsCardPodVac)
                {
                    _IsCardPodVac = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> IsTesterVac
        private bool _IsTesterVac;
        public bool IsTesterVac
        {
            get { return _IsTesterVac; }
            set
            {
                if (value != _IsTesterVac)
                {
                    _IsTesterVac = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> IsPogoVac
        private bool _IsPogoVac;
        public bool IsPogoVac
        {
            get { return _IsPogoVac; }
            set
            {
                if (value != _IsPogoVac)
                {
                    _IsPogoVac = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardCenterOffsetX1
        private double _CardCenterOffsetX1;
        public double CardCenterOffsetX1
        {
            get { return _CardCenterOffsetX1; }
            set
            {
                if (value != _CardCenterOffsetX1)
                {
                    _CardCenterOffsetX1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardCenterOffsetX2
        private double _CardCenterOffsetX2;
        public double CardCenterOffsetX2
        {
            get { return _CardCenterOffsetX2; }
            set
            {
                if (value != _CardCenterOffsetX2)
                {
                    _CardCenterOffsetX2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardCenterOffsetY1
        private double _CardCenterOffsetY1;
        public double CardCenterOffsetY1
        {
            get { return _CardCenterOffsetY1; }
            set
            {
                if (value != _CardCenterOffsetY1)
                {
                    _CardCenterOffsetY1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardCenterOffsetY2
        private double _CardCenterOffsetY2;
        public double CardCenterOffsetY2
        {
            get { return _CardCenterOffsetY2; }
            set
            {
                if (value != _CardCenterOffsetY2)
                {
                    _CardCenterOffsetY2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardPodCenterX
        private double _CardPodCenterX;
        public double CardPodCenterX
        {
            get { return _CardPodCenterX; }
            set
            {
                if (value != _CardPodCenterX)
                {
                    _CardPodCenterX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardPodCenterY
        private double _CardPodCenterY;
        public double CardPodCenterY
        {
            get { return _CardPodCenterY; }
            set
            {
                if (value != _CardPodCenterY)
                {
                    _CardPodCenterY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardLoadZLimit
        private double _CardLoadZLimit;
        public double CardLoadZLimit
        {
            get { return _CardLoadZLimit; }
            set
            {
                if (value != _CardLoadZLimit)
                {
                    _CardLoadZLimit = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardLoadZInterval
        private double _CardLoadZInterval;
        public double CardLoadZInterval
        {
            get { return _CardLoadZInterval; }
            set
            {
                if (value != _CardLoadZInterval)
                {
                    _CardLoadZInterval = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardUnloadZOffset
        private double _CardUnloadZOffset;
        public double CardUnloadZOffset
        {
            get { return _CardUnloadZOffset; }
            set
            {
                if (value != _CardUnloadZOffset)
                {
                    _CardUnloadZOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> PatternFocusingRange
        private double _PatternFocusingRange;
        public double PatternFocusingRange
        {
            get { return _PatternFocusingRange; }
            set
            {
                if (value != _PatternFocusingRange)
                {
                    _PatternFocusingRange = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> PatternRetryCount
        private int _PatternRetryCount;
        public int PatternRetryCount
        {
            get { return _PatternRetryCount; }
            set
            {
                if (value != _PatternRetryCount)
                {
                    _PatternRetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> DockOffsetZ
        private double _DockOffsetZ;
        public double DockOffsetZ
        {
            get { return _DockOffsetZ; }
            set
            {
                if (value != _DockOffsetZ)
                {
                    _DockOffsetZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> UndockOffsetZ
        private double _UndockOffsetZ;
        public double UndockOffsetZ
        {
            get { return _UndockOffsetZ; }
            set
            {
                if (value != _UndockOffsetZ)
                {
                    _UndockOffsetZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardID
        private string _CardID;
        public string CardID
        {
            get { return _CardID; }
            set
            {
                if (value != _CardID)
                {
                    _CardID = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> AlignPosStr
        private string _AlignPosStr;
        public string AlignPosStr
        {
            get { return _AlignPosStr; }
            set
            {
                if (value != _AlignPosStr)
                {
                    _AlignPosStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DockPosStr
        private string _DockPosStr;
        public string DockPosStr
        {
            get { return _DockPosStr; }
            set
            {
                if (value != _DockPosStr)
                {
                    _DockPosStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> EnableClickToMove
        private bool _EnableClickToMove = true;
        public bool EnableClickToMove
        {
            get { return _EnableClickToMove; }
            set
            {
                if (value != _EnableClickToMove)
                {
                    _EnableClickToMove = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> EnableClickToMove
        private int _SelectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set
            {
                if (value != _SelectedTabIndex)
                {
                    _SelectedTabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //MoveToLoaderLoadCommand

        private AsyncCommand _MoveToLoaderLoadCommand;
        public IAsyncCommand MoveToLoaderLoadCommand
        {
            get
            {
                if (null == _MoveToLoaderLoadCommand) _MoveToLoaderLoadCommand = new AsyncCommand(MoveToLoaderLoadCommandFunc);
                return _MoveToLoaderLoadCommand;
            }
        }
        public async Task MoveToLoaderLoadCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Loader Card LoadingPosition",
                            "Do you want Move to Loader Card LoadingPosition Position?",
                            EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    var loader = this.LoaderMaster.Loader;
                    var scanjob = loader.GetLoaderInfo().StateMap;

                    bool isPodUp = false;
                    isPodUp = _RemoteMediumProxy.GPCC_OP_IsLCardExistCommand();
                    if (isPodUp == false)
                    {
                        await Task.Run(() =>
                        {

                            var modules = loader.ModuleManager;
                            var CardChange = loader.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, this.LoaderCommunicationManager.SelectedStageIndex);
                            var arm = loader.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                            try
                            {
                                loader.GetLoaderCommands().CardMoveLoadingPosition(CardChange, arm);
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Error($"CardLoadMoveFunc(): Exception occurred. Err = {err.Message}");
                            }
                        });
                    }
                    else
                    {
                        await (this).MetroDialogManager().ShowMessageDialog("Card Pod is UP",
                            "It cannot operate while the card pod is up.",
                            EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //MoveToArmExtendCommand

        private AsyncCommand _MoveToArmExtendCommand;
        public IAsyncCommand MoveToArmExtendCommand
        {
            get
            {
                if (null == _MoveToArmExtendCommand) _MoveToArmExtendCommand = new AsyncCommand(MoveToArmExtendCommandFunc);
                return _MoveToArmExtendCommand;
            }
        }
        public async Task MoveToArmExtendCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Loader CardArm Extend Position",
                            "Do you want Move to Loader CardArm Extend Position?",
                            EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    //   this.LoaderMaster.GetClient(0).
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_OP_MoveToZClearedCommand();
                        _RemoteMediumProxy.GPCC_OP_LoaderDoorOpenCommand();
                        Task.Delay(1500).Wait();
                        bool isOpen = false;
                        bool isClose = false;
                        bool isCardExist = false;

                        isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand();
                        isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand();
                        isCardExist = _RemoteMediumProxy.GPCC_OP_IsLCardExistCommand();
                        if (isOpen == true && isClose == false && isCardExist == false)
                        {
                            await Task.Run(() =>
                            {
                                var loader = this.LoaderMaster.Loader;
                                var position = this.LoaderMaster.Loader.SystemParameter.CCModules[this.LoaderCommunicationManager.SelectedStageIndex - 1].AccessParams.FirstOrDefault(i => i.SubstrateSize.Value == SubstrateSizeEnum.INCH12);
                                double dist = position.Position.U.Value;

                                var actPos = loader.MotionManager.GetAxis(EnumAxisConstants.LCC).Status.Position.Actual;
                                dist -= actPos;

                                try
                                {
                                    loader.GetLoaderCommands().JogMove(loader.MotionManager.GetAxis(EnumAxisConstants.LCC), dist);
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Error($"CardLoadMoveFunc(): Exception occurred. Err = {err.Message}");
                                }
                            });
                        }
                        else
                        {
                            if (isOpen == false)
                            {
                                await (this).MetroDialogManager().ShowMessageDialog("Loader Door Open Error",
                                    "Please Check Loader Door",
                                    EnumMessageStyle.Affirmative);
                            }
                            else if (isCardExist == true)
                            {
                                await (this).MetroDialogManager().ShowMessageDialog("Card Pod Error",
                                    "It cannot operate while the card pod is up.",
                                    EnumMessageStyle.Affirmative);
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


        #region //MoveToArmRetractCommand

        private AsyncCommand _MoveToArmRetractCommand;
        public IAsyncCommand MoveToArmRetractCommand
        {
            get
            {
                if (null == _MoveToArmRetractCommand) _MoveToArmRetractCommand = new AsyncCommand(MoveToArmRetractCommandFunc);
                return _MoveToArmRetractCommand;
            }
        }
        public async Task MoveToArmRetractCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Loader CardArm Retract Position",
                            "Do you want Loader CardArm Retract Position?",
                            EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    _RemoteMediumProxy.GPCC_OP_MoveToZClearedCommand();
                    _RemoteMediumProxy.GPCC_OP_LoaderDoorOpenCommand();
                    bool isOpen = false;
                    bool isClose = false;
                    bool isCardExist = false;

                    isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand();
                    isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand();
                    isCardExist = _RemoteMediumProxy.GPCC_OP_IsLCardExistCommand();
                    if (isOpen == true && isClose == false && isCardExist == false)
                    {
                        await Task.Run(() =>
                        {
                            var loader = this.LoaderMaster.Loader;
                            double dist = 0;

                            var actPos = loader.MotionManager.GetAxis(EnumAxisConstants.LCC).Status.Position.Actual;
                            dist -= actPos;

                            try
                            {
                                loader.GetLoaderCommands().JogMove(loader.MotionManager.GetAxis(EnumAxisConstants.LCC), dist);
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Error($"CardLoadMoveFunc(): Exception occurred. Err = {err.Message}");
                            }
                        });
                    }
                    else
                    {
                        if (isOpen == false)
                        {
                            await (this).MetroDialogManager().ShowMessageDialog("Loader Door Open Error",
                                "Please Check Loader Door",
                                EnumMessageStyle.Affirmative);
                        }
                        else if (isCardExist == true)
                        {
                            await (this).MetroDialogManager().ShowMessageDialog("Card Pod is UP",
                                "It cannot operate while the card pod is up.",
                                EnumMessageStyle.Affirmative);
                        }
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //MoveToCardAlignPosCommand

        private AsyncCommand _MoveToCardAlignPosCommand;
        public IAsyncCommand MoveToCardAlignPosCommand
        {
            get
            {
                if (null == _MoveToCardAlignPosCommand) _MoveToCardAlignPosCommand = new AsyncCommand(MoveToCardAlignPosCommandFunc);
                return _MoveToCardAlignPosCommand;
            }
        }
        public async Task MoveToCardAlignPosCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Align Position",
                                  "Do you want Move to Align Position?",
                                  EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null && !IsCardPodUP)
                    {
                        await _RemoteMediumProxy.GPCC_OP_MoveToLoaderCommand();

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        AlignPosStr = $"X : { Math.Round(sysdata.GP_CardAlignPosX, 0)}  , Y : {Math.Round(sysdata.GP_CardAlignPosY, 0)} , T :{Math.Round(sysdata.GP_CardAlignPosT, 0)}";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //MoveToCardDockPosCommand

        private AsyncCommand _MoveToCardDockPosCommand;
        public IAsyncCommand MoveToCardDockPosCommand
        {
            get
            {
                if (null == _MoveToCardDockPosCommand) _MoveToCardDockPosCommand = new AsyncCommand(MoveToCardDockPosCommandFunc);
                return _MoveToCardDockPosCommand;
            }
        }
        public async Task MoveToCardDockPosCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Dock Position",
                                "Do you want Move to Dock Position?",
                                EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_CardAlignCommand();

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        DockPosStr = $"X : {Math.Round(sysdata.GP_DockMatchedPosX, 0)}  , Y : {Math.Round(sysdata.GP_DockMatchedPosY, 0)} , Z :{Math.Round(sysdata.GP_DockMatchedPosZ, 0)} , T :{Math.Round(sysdata.GP_DockMatchedPosT, 0)}";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion



        #region //MoveToCardDockPosCommand

        private AsyncCommand _MoveToZClearedCommand;
        public IAsyncCommand MoveToZClearedCommand
        {
            get
            {
                if (null == _MoveToZClearedCommand) _MoveToZClearedCommand = new AsyncCommand(MoveToZClearedCommandFunc);
                return _MoveToZClearedCommand;
            }
        }
        public async Task MoveToZClearedCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Z Cleared Position",
                                 "Do you want Move to Z Cleared Position?",
                                 EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_OP_MoveToZClearedCommand();

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        #region //DoorOpenCommand

        private AsyncCommand _DoorOpenCommand;
        public IAsyncCommand DoorOpenCommand
        {
            get
            {
                if (null == _DoorOpenCommand) _DoorOpenCommand = new AsyncCommand(DoorOpenCommandFunc);
                return _DoorOpenCommand;
            }
        }
        public async Task DoorOpenCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Open Door",
                                  "Do you want to open the door?",
                                  EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_OP_LoaderDoorOpenCommand();

                        bool isOpen = false;
                        bool isClose = false;
                        Task.Delay(1000).Wait();
                        isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand();
                        isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand();
                        if (isOpen == true && isClose == false)
                        {
                            IsDoorOpen = true;
                        }
                        else
                        {
                            IsDoorOpen = false;
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

        #region //DoorCloseCommand

        private AsyncCommand _DoorCloseCommand;
        public IAsyncCommand DoorCloseCommand
        {
            get
            {
                if (null == _DoorCloseCommand) _DoorCloseCommand = new AsyncCommand(DoorCloseCommandFunc);
                return _DoorCloseCommand;
            }
        }
        public async Task DoorCloseCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Door Close",
                                  "Do you want to close the door?",
                                  EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        _RemoteMediumProxy.GPCC_OP_LoaderDoorCloseCommand();
                        bool isOpen = false;
                        bool isClose = false;
                        isOpen = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorOpenCommand();
                        isClose = _RemoteMediumProxy.GPCC_OP_IsLoaderDoorCloseCommand();
                        if (isOpen == true && isClose == false)
                        {
                            IsDoorOpen = true;
                        }
                        else
                        {
                            IsDoorOpen = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion


        #region //PodUpCommand

        private AsyncCommand _PodUpCommand;
        public IAsyncCommand PodUpCommand
        {
            get
            {
                if (null == _PodUpCommand) _PodUpCommand = new AsyncCommand(PodUpCommandFunc);
                return _PodUpCommand;
            }
        }
        public async Task PodUpCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Card Pod Up",
                                  "Do you want to raise pod?",
                                  EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_RaisePodAfterMoveCardAlignPosCommand();

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //PodDownCommand

        private AsyncCommand _PodDownCommand;
        public IAsyncCommand PodDownCommand
        {
            get
            {
                if (null == _PodDownCommand) _PodDownCommand = new AsyncCommand(PodDownCommandFunc);
                return _PodDownCommand;
            }
        }
        public async Task PodDownCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Card Pod Down",
                              "Do you want to drop pod?",
                              EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_DropPodCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //PodVacOnCommand

        private AsyncCommand _PodVacOnCommand;
        public ICommand PodVacOnCommand
        {
            get
            {
                if (null == _PodVacOnCommand) _PodVacOnCommand = new AsyncCommand(PodVacOnCommandFunc);
                return _PodVacOnCommand;
            }
        }
        public async Task PodVacOnCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Card Pod Vac On",
                 "Do you want to Card Pod Vac On?",
                 EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_PCardPodVacuumOnCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //PodVacOffCommand

        private AsyncCommand _PodVacOffCommand;
        public ICommand PodVacOffCommand
        {
            get
            {
                if (null == _PodVacOffCommand) _PodVacOffCommand = new AsyncCommand(PodVacOffCommandFunc);
                return _PodVacOffCommand;
            }
        }
        public async Task PodVacOffCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Card Pod Vac Off",
                    "Do you want to Card Pod Vac Off?",
                    EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_PCardPodVacuumOffCommand();

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //TesterVacOnCommand

        private AsyncCommand _TesterVacOnCommand;
        public ICommand TesterVacOnCommand
        {
            get
            {
                if (null == _TesterVacOnCommand) _TesterVacOnCommand = new AsyncCommand(TesterVacOnCommandFunc);
                return _TesterVacOnCommand;
            }
        }
        public async Task TesterVacOnCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Teseter Vac On",
                             "Do you want to Teseter Vac On?",
                             EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_UpPlateTesterContactVacuumOnCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //TesterVacOffCommand

        private AsyncCommand _TesterVacOffCommand;
        public ICommand TesterVacOffCommand
        {
            get
            {
                if (null == _TesterVacOffCommand) _TesterVacOffCommand = new AsyncCommand(TesterVacOffCommandFunc);
                return _TesterVacOffCommand;
            }
        }
        public async Task TesterVacOffCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Teseter Vac Off",
                                "Do you want to Teseter Vac Off?",
                                EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_UpPlateTesterCOfftactVacuumOffCommand();

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        #region //PogoVacOnCommand

        private AsyncCommand _PogoVacOnCommand;
        public ICommand PogoVacOnCommand
        {
            get
            {
                if (null == _PogoVacOnCommand) _PogoVacOnCommand = new AsyncCommand(PogoVacOnCommandFunc);
                return _PogoVacOnCommand;
            }
        }
        public async Task PogoVacOnCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Pogo Vac On",
                                "Do you want to Pogo Vac On?",
                                EnumMessageStyle.AffirmativeAndNegative);
                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_PogoVacuumOnCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //PogoVacOffCommand

        private AsyncCommand _PogoVacOffCommand;
        public ICommand PogoVacOffCommand
        {
            get
            {
                if (null == _PogoVacOffCommand) _PogoVacOffCommand = new AsyncCommand(PogoVacOffCommandFunc);
                return _PogoVacOffCommand;
            }
        }
        public async Task PogoVacOffCommandFunc()
        {
            try
            {
                var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Pogo Vac Off",
                           "Do you want to Pogo Vac Off?",
                           EnumMessageStyle.AffirmativeAndNegative);

                if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_PogoVacuumOffCommand();
                        //  UpdateBindingData();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Manual_ZUPCommand
        private AsyncCommand _Manual_ZUPCommand;
        public IAsyncCommand Manual_ZUPCommand
        {
            get
            {
                if (null == _Manual_ZUPCommand) _Manual_ZUPCommand = new AsyncCommand(Manual_ZUPCommandFunc);
                return _Manual_ZUPCommand;
            }
        }
        public async Task Manual_ZUPCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_Observation_ManualZMoveCommand(EnumProberCam.WAFER_LOW_CAM, Manual_ZDistanceValue);

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Manual_ZDownCommand
        private AsyncCommand _Manual_ZDownCommand;
        public IAsyncCommand Manual_ZDownCommand
        {
            get
            {
                if (null == _Manual_ZDownCommand) _Manual_ZDownCommand = new AsyncCommand(Manual_ZDownCommandFunc);
                return _Manual_ZDownCommand;
            }
        }
        public async Task Manual_ZDownCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_Observation_ManualZMoveCommand(EnumProberCam.WAFER_LOW_CAM, Manual_ZDistanceValue * -1);

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion




        #region ==> Manual_PZUPCommand
        private AsyncCommand _Manual_PZUPCommand;
        public IAsyncCommand Manual_PZUPCommand
        {
            get
            {
                if (null == _Manual_PZUPCommand) _Manual_PZUPCommand = new AsyncCommand(Manual_PZUPCommandFunc);
                return _Manual_PZUPCommand;
            }
        }
        public async Task Manual_PZUPCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_Observation_ManualZMoveCommand(EnumProberCam.PIN_LOW_CAM, Manual_PZDistanceValue);

                        Task task = new Task(() =>
                        {
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Manual_PZDownCommand
        private AsyncCommand _Manual_PZDownCommand;
        public IAsyncCommand Manual_PZDownCommand
        {
            get
            {
                if (null == _Manual_PZDownCommand) _Manual_PZDownCommand = new AsyncCommand(Manual_PZDownCommandFunc);
                return _Manual_PZDownCommand;
            }
        }
        public async Task Manual_PZDownCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                if (stageBusy == false)
                {
                    var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_Observation_ManualZMoveCommand(EnumProberCam.PIN_LOW_CAM, Manual_PZDistanceValue * -1);

                    Task task = new Task(() =>
                    {
                        UpdateBindingData();
                    });
                    task.Start();
                    await task;
                }
            }
        }
        #endregion


        #endregion

        #region ==> CardCenterOffsetX1TextBoxClickCommand
        private AsyncCommand _CardCenterOffsetX1TextBoxClickCommand;
        public ICommand CardCenterOffsetX1TextBoxClickCommand
        {
            get
            {
                if (null == _CardCenterOffsetX1TextBoxClickCommand) _CardCenterOffsetX1TextBoxClickCommand = new AsyncCommand(CardCenterOffsetX1TextBoxClickCommandFunc);
                return _CardCenterOffsetX1TextBoxClickCommand;
            }
        }
        private async Task CardCenterOffsetX1TextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardCenterOffsetX1.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardCenterOffsetX1Command(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();

                        string probeCardID = LoaderMaster.Loader.LoaderMaster.CardIDLastTwoWord;
                        if(probeCardID == null)
                        {
                            probeCardID = CardID;
                        }
                        ProberCardListParameter proberCard = sysdata.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);
                        
                        if(proberCard != null)
                        {
                            CardCenterOffsetX1 = proberCard.FiducialMarInfos[0].CardCenterOffset.GetX();
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
        #region ==> CardCenterOffsetX2TextBoxClickCommand
        private AsyncCommand _CardCenterOffsetX2TextBoxClickCommand;
        public ICommand CardCenterOffsetX2TextBoxClickCommand
        {
            get
            {
                if (null == _CardCenterOffsetX2TextBoxClickCommand) _CardCenterOffsetX2TextBoxClickCommand = new AsyncCommand(CardCenterOffsetX2TextBoxClickCommandFunc);
                return _CardCenterOffsetX2TextBoxClickCommand;
            }
        }
        private async Task CardCenterOffsetX2TextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardCenterOffsetX2.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardCenterOffsetX2Command(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();

                        string probeCardID = LoaderMaster.Loader.LoaderMaster.CardIDLastTwoWord;
                        if (probeCardID == null)
                        {
                            probeCardID = CardID;
                        }
                        ProberCardListParameter proberCard = sysdata.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                        if(proberCard != null)
                        {
                            CardCenterOffsetX2 = proberCard.FiducialMarInfos[1].CardCenterOffset.GetX();
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
        #region ==> CardCenterOffsetY1TextBoxClickCommand
        private AsyncCommand _CardCenterOffsetY1TextBoxClickCommand;
        public ICommand CardCenterOffsetY1TextBoxClickCommand
        {
            get
            {
                if (null == _CardCenterOffsetY1TextBoxClickCommand) _CardCenterOffsetY1TextBoxClickCommand = new AsyncCommand(CardCenterOffsetY1TextBoxClickCommandFunc);
                return _CardCenterOffsetY1TextBoxClickCommand;
            }
        }
        private async Task CardCenterOffsetY1TextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardCenterOffsetY1.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardCenterOffsetY1Command(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();

                        string probeCardID = LoaderMaster.Loader.LoaderMaster.CardIDLastTwoWord;
                        if (probeCardID == null)
                        {
                            probeCardID = CardID;
                        }
                        ProberCardListParameter proberCard = sysdata.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                        if (proberCard != null)
                        {
                            CardCenterOffsetY1 = proberCard.FiducialMarInfos[0].CardCenterOffset.GetY();
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
        #region ==> CardCenterOffsetY2TextBoxClickCommand
        private AsyncCommand _CardCenterOffsetY2TextBoxClickCommand;
        public ICommand CardCenterOffsetY2TextBoxClickCommand
        {
            get
            {
                if (null == _CardCenterOffsetY2TextBoxClickCommand) _CardCenterOffsetY2TextBoxClickCommand = new AsyncCommand(CardCenterOffsetY2TextBoxClickCommandFunc);
                return _CardCenterOffsetY2TextBoxClickCommand;
            }
        }
        private async Task CardCenterOffsetY2TextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardCenterOffsetY2.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardCenterOffsetY2Command(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();

                        string probeCardID = LoaderMaster.Loader.LoaderMaster.CardIDLastTwoWord;
                        if (probeCardID == null)
                        {
                            probeCardID = CardID;
                        }
                        ProberCardListParameter proberCard = sysdata.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                        if(proberCard != null)
                        {
                            CardCenterOffsetY2 = proberCard.FiducialMarInfos[1].CardCenterOffset.GetY();
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
        #region ==> CardPodCenterXTextBoxClickCommand
        private AsyncCommand _CardPodCenterXTextBoxClickCommand;
        public ICommand CardPodCenterXTextBoxClickCommand
        {
            get
            {
                if (null == _CardPodCenterXTextBoxClickCommand) _CardPodCenterXTextBoxClickCommand = new AsyncCommand(CardPodCenterXTextBoxClickCommandFunc);
                return _CardPodCenterXTextBoxClickCommand;
            }
        }
        private async Task CardPodCenterXTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardPodCenterX.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardPodCenterXCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        CardPodCenterX = sysdata.GP_CardPodCenterX;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> CardPodCenterYTextBoxClickCommand
        private AsyncCommand _CardPodCenterYTextBoxClickCommand;
        public ICommand CardPodCenterYTextBoxClickCommand
        {
            get
            {
                if (null == _CardPodCenterYTextBoxClickCommand) _CardPodCenterYTextBoxClickCommand = new AsyncCommand(CardPodCenterYTextBoxClickCommandFunc);
                return _CardPodCenterYTextBoxClickCommand;
            }
        }
        private async Task CardPodCenterYTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardPodCenterY.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardPodCenterYCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        CardPodCenterY = sysdata.GP_CardPodCenterY;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CardLoadZLimitTextBoxClickCommand
        private AsyncCommand _CardLoadZLimitTextBoxClickCommand;
        public ICommand CardLoadZLimitTextBoxClickCommand
        {
            get
            {
                if (null == _CardLoadZLimitTextBoxClickCommand) _CardLoadZLimitTextBoxClickCommand = new AsyncCommand(CardLoadZLimitTextBoxClickCommandFunc);
                return _CardLoadZLimitTextBoxClickCommand;
            }
        }
        private async Task CardLoadZLimitTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardLoadZLimit.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardLoadZLimitCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        CardLoadZLimit = sysdata.GP_CardLoadZLimit;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CardLoadZIntervalTextBoxClickCommand
        private AsyncCommand _CardLoadZIntervalTextBoxClickCommand;
        public ICommand CardLoadZIntervalTextBoxClickCommand
        {
            get
            {
                if (null == _CardLoadZIntervalTextBoxClickCommand) _CardLoadZIntervalTextBoxClickCommand = new AsyncCommand(CardLoadZIntervalTextBoxClickCommandFunc);
                return _CardLoadZIntervalTextBoxClickCommand;
            }
        }
        private async Task CardLoadZIntervalTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardLoadZInterval.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardLoadZIntervalCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        CardLoadZInterval = sysdata.GP_CardLoadZInterval;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        #region ==> CardUnloadZOffsetTextBoxClickCommand
        private AsyncCommand _CardUnloadZOffsetTextBoxClickCommand;
        public ICommand CardUnloadZOffsetTextBoxClickCommand
        {
            get
            {
                if (null == _CardUnloadZOffsetTextBoxClickCommand) _CardUnloadZOffsetTextBoxClickCommand = new AsyncCommand(CardUnloadZOffsetTextBoxClickCommandFunc);
                return _CardUnloadZOffsetTextBoxClickCommand;
            }
        }
        private async Task CardUnloadZOffsetTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(CardUnloadZOffset.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetCardUnloadZOffsetCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        CardUnloadZOffset = sysdata.GP_CardUnloadZOffset;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PatternFocusingRangeTextBoxClickCommand
        private AsyncCommand _PatternFocusingRangeTextBoxClickCommand;
        public ICommand PatternFocusingRangeTextBoxClickCommand
        {
            get
            {
                if (null == _PatternFocusingRangeTextBoxClickCommand) _PatternFocusingRangeTextBoxClickCommand = new AsyncCommand(PatternFocusingRangeTextBoxClickCommandFunc);
                return _PatternFocusingRangeTextBoxClickCommand;
            }
        }
        private async Task PatternFocusingRangeTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(PatternFocusingRange.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetFocusRangeValueCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        PatternFocusingRange = sysdata.GP_CardFocusRange;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PatternRetryCountTextBoxClickCommand
        private AsyncCommand _PatternRetryCountTextBoxClickCommand;
        public ICommand PatternRetryCountTextBoxClickCommand
        {
            get
            {
                if (null == _PatternRetryCountTextBoxClickCommand) _PatternRetryCountTextBoxClickCommand = new AsyncCommand(PatternRetryCountTextBoxClickCommandFunc);
                return _PatternRetryCountTextBoxClickCommand;
            }
        }
        private async Task PatternRetryCountTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(PatternRetryCount.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                int val = 0;
                if (Int32.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetAlignRetryCountCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        PatternRetryCount = sysdata.GP_CardAlignRetryCount;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> DockOffsetZTextBoxClickCommand
        private AsyncCommand _DockOffsetZTextBoxClickCommand;
        public ICommand DockOffsetZTextBoxClickCommand
        {
            get
            {
                if (null == _DockOffsetZTextBoxClickCommand) _DockOffsetZTextBoxClickCommand = new AsyncCommand(DockOffsetZTextBoxClickCommandFunc);
                return _DockOffsetZTextBoxClickCommand;
            }
        }
        private async Task DockOffsetZTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(DockOffsetZ.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetContactOffsetZValueCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        DockOffsetZ = sysdata.GP_ContactCorrectionZ;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> UndockOffsetZTextBoxClickCommand
        private AsyncCommand _UndockOffsetZTextBoxClickCommand;
        public ICommand UndockOffsetZTextBoxClickCommand
        {
            get
            {
                if (null == _UndockOffsetZTextBoxClickCommand) _UndockOffsetZTextBoxClickCommand = new AsyncCommand(UndockOffsetZTextBoxClickCommandFunc);
                return _UndockOffsetZTextBoxClickCommand;
            }
        }
        private async Task UndockOffsetZTextBoxClickCommandFunc()
        {
            try
            {
                String valString = VirtualKeyboard.STA_Show(UndockOffsetZ.ToString(), KB_TYPE.DECIMAL);

                if (String.IsNullOrEmpty(valString))
                    return;
                double val = 0;
                if (Double.TryParse(valString, out val))
                {
                    if (_RemoteMediumProxy != null)
                    {
                        await _RemoteMediumProxy.GPCC_OP_SetUndockContactOffsetZValueCommand(val);
                        var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                        UndockOffsetZ = sysdata.GP_UndockCorrectionZ;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> SelectPogoAlign4PCommand
        private AsyncCommand _SelectPogoAlign4PCommand;
        public ICommand SelectPogoAlign4PCommand
        {
            get
            {
                if (null == _SelectPogoAlign4PCommand) _SelectPogoAlign4PCommand = new AsyncCommand(SelectPogoAlign4PCommandFunc);
                return _SelectPogoAlign4PCommand;
            }
        }
        private async Task SelectPogoAlign4PCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    PogoAlignPoint = EnumPogoAlignPoint.POINT_4;

                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_Observation_PogoAlignPointCommand(PogoAlignPoint);

                        Task task = new Task(() =>
                        {
                            GetMarkPositionList();
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> SelectPogoAlign3PCommand
        private AsyncCommand _SelectPogoAlign3PCommand;
        public ICommand SelectPogoAlign3PCommand
        {
            get
            {
                if (null == _SelectPogoAlign3PCommand) _SelectPogoAlign3PCommand = new AsyncCommand(SelectPogoAlign3PCommandFunc);
                return _SelectPogoAlign3PCommand;
            }
        }
        private async Task SelectPogoAlign3PCommandFunc()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    PogoAlignPoint = EnumPogoAlignPoint.POINT_3;

                    var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_Observation_PogoAlignPointCommand(PogoAlignPoint);

                        Task task = new Task(() =>
                        {
                            GetMarkPositionList();
                            UpdateBindingData();
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }

    [Serializable]
    public class MarkPosition : INotifyPropertyChanged, IGPCCObservationMarkPosition, IFactoryModule
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        [field: NonSerialized, JsonIgnore]
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        #region ==> Description
        private String _Description;
        public String Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ButtonEnable
        private bool _ButtonEnable;
        public bool ButtonEnable
        {
            get { return _ButtonEnable; }
            set
            {
                if (value != _ButtonEnable)
                {
                    _ButtonEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegisterPatternCommand
        [field: NonSerialized, JsonIgnore]
        private AsyncCommand _RegisterPatternCommand;
        public ICommand RegisterPatternCommand
        {
            get
            {
                if (null == _RegisterPatternCommand) _RegisterPatternCommand = new AsyncCommand(RegisterPatternCommandFunc);
                return _RegisterPatternCommand;
            }
        }
        public async Task RegisterPatternCommandFunc()
        {
            //this.GPCardAligner().RegisterPattern(_ProberCam, Index);
            //SaveCurrentPos();
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.GPCC_Observation_RegisterPatternCommand();
                }

                Task task = new Task(() =>
                {
                    var data = _RemoteMediumProxy.GPCC_Observation_GetGPCCDataCommand();
                    if (data != null)
                    {
                        var updateMarkData = data.MarkPositions.SingleOrDefault(infos => infos.Index == this.Index);
                        this.Description = $"X: {updateMarkData.XPos}, Y: {updateMarkData.YPos}";
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion


        #region ==> RegisterPosCommand
        [field: NonSerialized, JsonIgnore]
        private AsyncCommand _RegisterPosCommand;
        public ICommand RegisterPosCommand
        {
            get
            {
                if (null == _RegisterPosCommand) _RegisterPosCommand = new AsyncCommand(RegisterPosCommandFunc);
                return _RegisterPosCommand;
            }
        }

        public async Task RegisterPosCommandFunc()
        {
            //this.GPCardAligner().RegisterPattern(_ProberCam, Index);
            //SaveCurrentPos();
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_Observation_RegisterPosCommand();

                Task task = new Task(() =>
                {
                    Description = _RemoteMediumProxy.GPCC_OP_GetPosition();
                });
                task.Start();
                await task;
            }
        }
        #endregion
        public void MoveToMark()
        {

        }
        private void SaveCurrentPos()
        {

        }

        public EnumProberCam _ProberCam;
        public CatCoordinates _Position;
        public int Index { get; set; }
        public MarkPosition(EnumProberCam proberCam, CatCoordinates position, int index)
        {
            _ProberCam = proberCam;
            _Position = position;
            Index = index;
            Description = $"X : {string.Format("{0:0.00}", (_Position.X.Value))}  Y :  {string.Format("{0:0.00}", (_Position.Y.Value))}";
            ButtonEnable = false;
        }
    }
}
