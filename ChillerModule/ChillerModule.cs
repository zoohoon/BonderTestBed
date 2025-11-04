using ChillerModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Temperature.Temp.Chiller
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderServiceBase;
    //using global::ChillerModule.Chago;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.Chiller;
    using SerializerUtil;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    public class ChillerModule : IChillerModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        public IChillerComm ChillerComm { get; set; }
        public IChillerParameter ChillerParam { get; set; }
        public bool Initialized { get; set; } = false;

        private ChillerInfo _ChillerInfo;
        public IChillerInfo ChillerInfo
        {
            get { return _ChillerInfo; }
            set
            {
                if (_ChillerInfo != value)
                {
                    _ChillerInfo = (ChillerInfo) value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (_IsConnected != value)
                {
                    bool preVal = _IsConnected;                    
                    _IsConnected = value;
                    RaisePropertyChanged();

                    LoggerManager.Debug($"[CHI][Chiller #{_ChillerInfo.Index}] IsConnected is changed {preVal} to {_IsConnected}.");
                    IsConnectedStateChanged(preVal, _IsConnected);
                }
            }
        }

        private bool _ChillerMaintenanceFlag;
        public bool ChillerMaintenanceFlag
        {
            get { return _ChillerMaintenanceFlag; }
            set
            {
                if (_ChillerMaintenanceFlag != value)
                {
                    _ChillerMaintenanceFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _StageSetSV;
        public double StageSetSV
        {
            get { return _StageSetSV; }
            set
            {
                if (value != _StageSetSV)
                {
                    _StageSetSV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _UnLockStageSvBtn;
        public bool UnLockStageSvBtn
        {
            get { return _UnLockStageSvBtn; }
            set
            {
                if (value != _UnLockStageSvBtn)
                {
                    _UnLockStageSvBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChillerSVSetValue;
        public double ChillerSVSetValue
        {
            get { return _ChillerSVSetValue; }
            set
            {
                if (value != _ChillerSVSetValue)
                {
                    _ChillerSVSetValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _UIUpdatedTime;
        public DateTime UIUpdatedTime
        {
            get { return _UIUpdatedTime; }
            set
            {
                if (value != _UIUpdatedTime)
                {
                    _UIUpdatedTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _LastUpdatedTime;
        public DateTime LastUpdatedTime
        {
            get { return _LastUpdatedTime; }
            set
            {
                if (value != _LastUpdatedTime)
                {
                    _LastUpdatedTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool bIsUpdating = true;


        private EnumCommunicationState _CommunicationState;
        public EnumCommunicationState CommunicationState
        {
            get { return _CommunicationState; }
            set
            {
                if (_CommunicationState != value)
                {
                    _CommunicationState = value;
                    RaisePropertyChanged();
                }
            }
        }

        //internal double ChillerTargetTemp { get; set; } = 0; // 로직을 위한 타겟 온도 변수.
        Thread UpdateThread = null;
        public const int ZeroTemp = 0;

        #region .. Creator & Init & DeInit
        public ChillerModule(IChillerParameter chillerParam, int index)
        {
            try
            {
                ChillerParam = chillerParam;
                _ChillerInfo = new ChillerInfo();
                _ChillerInfo.Index = index;
                _ChillerInfo.WarningReport.CollectionChanged += WarningReportCollectionChangedHandler;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ChillerComm != null)
                {
                    ChillerComm.InitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.REMOTE)
                    {
                        ChillerComm = new ChillerRemote();
                        retVal = ChillerComm.InitModule();
                    }
                    else if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.HUBER
                        || ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.SOLARDIN)
                    {
                        ChillerComm = new HuberChillerComm(this);
                        retVal = ChillerComm.InitModule();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            _ChillerInfo.TargetTemp = ChillerComm.GetSetTempValue(0);
                            setOperationLockTime = DateTime.Now;
                            ChillerComm.SetOperatingLock(true, false, 0);
                            _ChillerInfo.SetOperationLockFlag = true;
                        }
                    }
                    else if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.EMUL)
                    {
                        ChillerComm = new ChillCommEmul(this);
                        retVal = ChillerComm.InitModule();

                        setOperationLockTime = DateTime.Now;
                        ChillerComm.SetOperatingLock(true, false, 0);
                        _ChillerInfo.SetOperationLockFlag = true;
                    }
                    //else if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.CHAGO)
                    //{
                    //    ChillerComm = new ChagoChillerComm(this.EnvControlManager().ChillerManager.GetChillerAdapter(), ChillerParam.SubIndex);
                    //    retVal = ChillerComm.InitModule();
                    //}

                    Initialized = true;
                }
                else
                {
                    //retVal = ChillerComm?.InitModule() ?? EventCodeEnum.CHILLER_INIT_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void DeInitModule()
        {
            try
            {
                PrepareThreadStop();
                this.Dispose();
                Initialized = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// ChillerRemote 인 경우 Commander 로 부터 Parameter 를 얻어와 Setting 하기 위한 함수.
        /// </summary>
        /// <param name="chillerParam"></param>
        public void InitParam(IChillerParameter chillerparam, bool setremotechange = false)
        {
            try
            {
                if (chillerparam != null & ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.REMOTE)
                {
                    bool diffoffsetdic = false;
                    if (ChillerParam.CoolantOffsetDictionary.Value.Count == chillerparam.CoolantOffsetDictionary.Value.Count
                         && !ChillerParam.CoolantOffsetDictionary.Value.Except(chillerparam.CoolantOffsetDictionary.Value).Any())
                    {
                        diffoffsetdic = false;  // CoolantOffsetDictionary 이 같다.
                    }
                    else
                    {
                        diffoffsetdic = true; // CoolantOffsetDictionary 이 다르다.
                    }

                    if (ChillerParam.ChillerHotLimitTemp.Value != chillerparam.ChillerHotLimitTemp.Value
                        || ChillerParam.CoolantInTemp.Value != chillerparam.CoolantInTemp.Value
                        || ChillerParam.Tolerance.Value != chillerparam.Tolerance.Value
                        || ChillerParam.ActivatableHighTemp.Value != chillerparam.ActivatableHighTemp.Value
                        || ChillerParam.InRangeWindowTemp.Value != chillerparam.InRangeWindowTemp.Value
                        || ChillerParam.AmbientTemp.Value != chillerparam.AmbientTemp.Value
                        || ChillerParam.SlowPumpSpeed.Value != chillerparam.SlowPumpSpeed.Value
                        || ChillerParam.NormalPumpSpeed.Value != chillerparam.NormalPumpSpeed.Value
                        || ChillerParam.FastPumpSpeed.Value != chillerparam.FastPumpSpeed.Value
                        || diffoffsetdic)
                    {
                        ChillerParam.ChillerHotLimitTemp.Value = chillerparam.ChillerHotLimitTemp.Value;
                        ChillerParam.CoolantInTemp.Value = chillerparam.CoolantInTemp.Value;
                        ChillerParam.Tolerance.Value = chillerparam.Tolerance.Value;
                        ChillerParam.ActivatableHighTemp.Value = chillerparam.ActivatableHighTemp.Value;
                        ChillerParam.InRangeWindowTemp.Value = chillerparam.InRangeWindowTemp.Value;
                        ChillerParam.AmbientTemp.Value = chillerparam.AmbientTemp.Value;
                        ChillerParam.SlowPumpSpeed.Value = chillerparam.SlowPumpSpeed.Value;
                        ChillerParam.NormalPumpSpeed.Value = chillerparam.NormalPumpSpeed.Value;
                        ChillerParam.FastPumpSpeed.Value = chillerparam.FastPumpSpeed.Value;
                        ChillerParam.CoolantOffsetDictionary = chillerparam.CoolantOffsetDictionary;
                        this.EnvControlManager().SaveSysParameter();
                    }

                    if (setremotechange)
                    {
                        this.TempController().SetActivatedState(false);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetChillerParam(int stageindex = -1)
        {
            byte[] param = null;
            try
            {
                param = SerializeManager.SerializeToByte(ChillerParam, typeof(ChillerParameter));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }
        #endregion

        private DateTime setOperationLockTime { get; set; }
        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);

        private void ResetCommAndStartUpdate()
        {
            DisConnect();
            this.MetroDialogManager().ShowMessageDialog("Error Message",
                $"A chiller #{_ChillerInfo.Index} error has occurred when update data." +
                $" The program disconnected from the chiller \r\n",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
        }

        public void UpdateProc()
        {
            try
            {
                while (bIsUpdating)
                {
                    UpdateChillerData();
                    if (!bIsUpdating)
                        break;
                    areUpdateEvent.WaitOne(3000);
                }
                UpdateThread = null;
            }
            catch (Exception err)
            {
                if (err is ThreadAbortException)
                {
                    //chiller 또는 loader(cell에서)에 새로 연결 하는 시점에서 기존 동작 중인 thread를 abort한 경우 임
                    //abort의 용도는 새로 연결을 하거나 프로그램 종료시 호출되고 있으므로 다른 exception과 달리 disconnect 및 변수 초기화를 하지 않는다.
                    //ResetCommAndStartUpdate 울 호출하거나 UpdateThread에 null을 할당할 경우 abort이후 새로 수행한 thread가 종료되고
                    //실제 chiller connection이 끊어져 버리게 된다.
                    LoggerManager.Debug($"ChillerModule.UpdateProc(): Thread Abort chiller #{_ChillerInfo.Index}");
                }
                else
                {
                    LoggerManager.Exception(err);
                    Task task = new Task(ResetCommAndStartUpdate);
                    LoggerManager.Debug($"ChillerModule.UpdateProc(): Error occurred on chiller #{_ChillerInfo.Index}. Err = {err.Message}");
                    task.Start();
                    LoggerManager.Debug($"ChillerModule.UpdateProc(): Comm. Cleanup for chiller #{_ChillerInfo.Index} started...");
                    UpdateThread = null;
                }
            }
        }

        private void UpdateChillerData()
        {
            try
            {
                if (ChillerComm.GetCommState(0) == EnumCommunicationState.CONNECTED)
                {
                    IsConnected = true;
                }
                else
                {
                    IsConnected = false;
                }

                if (IsConnected)
                {

                    if (!bIsUpdating) return;
                    GetSetTempValue();
                    areUpdateEvent.WaitOne(300);

                    if (!bIsUpdating) return;
                    GetInternalTempValue();
                    areUpdateEvent.WaitOne(300);

                    if (!bIsUpdating) return;
                    IsTempControlActive();
                    areUpdateEvent.WaitOne(300);

                    if (!bIsUpdating) return;
                    UpdateErrorData();
                    areUpdateEvent.WaitOne(300);

                    if (!bIsUpdating) return;
                    UpdateWarningData();
                    areUpdateEvent.WaitOne(300);

                    if (_ChillerInfo.ErrorReport == 0 && _ChillerInfo.WarningReport.Count == 0)
                    {
                        _ChillerInfo.IsErrorState = false;
                    }
                    else
                    {
                        _ChillerInfo.IsErrorState = true;
                    }

                    if (ChillerParam.ChillerModuleMode.Value != EnumChillerModuleMode.REMOTE)
                    {
                        if (_ChillerInfo.SetOperationLockFlag == true)//Maestro 상에는 Lock상태 유지인데 실제 Chiller장비에서 Unlock이 되는 상황일 때를 고려한 조건문
                        {
                            var timeoffset = (DateTime.Now - setOperationLockTime).TotalSeconds;
                            if (timeoffset > 20)
                            {
                                if (!bIsUpdating) return;
                                if (IsOperatingLock().Item1 == false)//실제 장비의 Lock정보를 가져온 후 false인 경우(Unlock) Lock해준다.
                                {
                                    ChillerComm.SetOperatingLock(true, false, 0);
                                }
                                setOperationLockTime = DateTime.Now;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private bool UpdateErrorData()
        {
            bool retVal = false;
            try
            {
                if (ChillerComm.GetCommState(0) != EnumCommunicationState.CONNECTED)
                    return retVal;

                var error = GetErrorReport();

                if (_ChillerInfo.ErrorReport != error)
                {
                    if (error != 0 && Extensions_IParam.LoadProgramFlag == true)
                    {
                        _ChillerInfo.ErrorReport = error;
                        this.NotifyManager().Notify(EventCodeEnum.CHILLER_ERROR_OCCURRED);

                        if (ChillerParam.ChillerModuleMode.Value != EnumChillerModuleMode.REMOTE)
                        {
                            //프로그램 알람
                            LoggerManager.Debug($"[CHILLER DEBUG]Error Occur Number: {error}");
                            string defaultMessage = "Please check the chiller and try to reconnect it on the chiller page";                                    
                            string message = this.EnvControlManager().ChillerManager.GetErrorMessage(error);
                            if (message == null || message.Equals(""))
                            {
                                message = defaultMessage;
                            }
                            this.MetroDialogManager().ShowMessageDialog("Error Message",
                                $"A chiller #{_ChillerInfo.Index} error has occurred. ErrorNumber : {error} \r\n" +
                                $"{message}",
                                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                    }
                    else //Clear되었다고 판단.
                    {
                        _ChillerInfo.ErrorReport = error;
                        this.NotifyManager().ClearNotify(EventCodeEnum.CHILLER_ERROR_OCCURRED);
                        LoggerManager.Debug($"[CHILLER DEBUG] Chiller Error Cleared.");//ChillerModule을 상태로 보내거나 IDLE상태로 보내서 현재 상태에 맞는 State로 보내야하는데 현재 Excute를 사용하지 않고 있으므로 로그만 찍는다.
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool UpdateWarningData()
        {
            bool retVal = false;
            try
            {
                if (ChillerComm.GetCommState(0) != EnumCommunicationState.CONNECTED)
                    return retVal;

                var warning = GetWarningMessage();
                if (warning != 0 && Extensions_IParam.LoadProgramFlag == true)
                {
                    if (_ChillerInfo.WarningReport.Where(reports => reports == warning).Count() == 0)
                    {
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _ChillerInfo.WarningReport.Add(warning);
                        });
                        this.NotifyManager().Notify(EventCodeEnum.CHILLER_WARNING_OCCURRED);

                        if (ChillerParam.ChillerModuleMode.Value != EnumChillerModuleMode.REMOTE)
                        {
                            LoggerManager.Debug($"[CHILLER DEBUG]Warning Occur Number: {warning}");
                            string message = this.EnvControlManager().ChillerManager.GetErrorMessage(warning);
                            this.MetroDialogManager().ShowMessageDialog("Warning Message",
                                $"A chiller #{_ChillerInfo.Index} warning has occurred. Please check the chiller. Warning number : {warning} \r\n" +
                                $"{message}",
                                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                    }
                }

                
                if (_ChillerInfo.WarningReport.Count != 0 && warning == 0)
                {
                    _ChillerInfo.WarningReport.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void WarningReportCollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if(((ObservableCollection<int>)sender).Count == 0)
                {
                    this.NotifyManager().ClearNotify(EventCodeEnum.CHILLER_WARNING_OCCURRED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IsConnectedStateChanged(bool oldValue, bool newValue)
        {
            try
            {
                // EnumChillerModuleMode.REMOTE: CELL
                if (ChillerParam.ChillerModuleMode.Value != EnumChillerModuleMode.REMOTE)      // only loader - chiller disconnect
                {
                    if (!oldValue && newValue)
                    {
                        this.NotifyManager().ClearNotify(EventCodeEnum.CHILLER_NOT_CONNECTED, _ChillerInfo.Index);
                    }
                    else if (oldValue && !newValue)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.CHILLER_NOT_CONNECTED, _ChillerInfo.Index);   // chiller index 1,2,3 // ALID 증가할때 +0으로 한다.
                    }
                }
                else
                {                    
                    if (!oldValue && newValue)
                    {
                        this.NotifyManager().ClearNotify(EventCodeEnum.CHILLER_REMOTE_NOT_CONNECTED);
                    }
                    else if (oldValue && !newValue)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.CHILLER_REMOTE_NOT_CONNECTED);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Start(bool bInit = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                _ChillerInfo.ErrorReport = 0;
                if (!bInit) //program init 시점(ui thread 활성화 전)에서는 invoke에서 block 발생할 수 있음
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _ChillerInfo.WarningReport.Clear();
                    });
                }

                if (String.IsNullOrEmpty(ChillerParam.IP) == false || ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.EMUL
                    || ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.REMOTE)
                {
                    if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.REMOTE)
                    {
                        if (this.LoaderController().GetconnectFlag() == false)
                            return EventCodeEnum.CHILLER_REMOTE_CONNECT_ERROR;
                    }
                    retVal = ChillerComm?.Connect(ChillerParam.IP, ChillerParam.Port) ?? EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    CommunicationState = GetCommState();

                    if (CommunicationState == EnumCommunicationState.CONNECTED)
                    {
                        if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.HUBER
                            || ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.EMUL)
                        {
                            bool tempActiveFlag = IsTempControlActive();
                            //IsAbortActivate 가 true 라면 IsTempControlActive() 가 false 여야함.
                            //IsAbortActivate 가 false 라면 IsTempControlActive() 가 true 여야함.
                            //IsAbortActivate 와 IsTempControlActive 의 값이 같다는 것은 프로그램 실행 중일때와 칠러의 상태가 달라졌다는 것이므로
                            //칠러의 상태를 최신상태로 한다.
                            if (ChillerParam.IsAbortActivate == tempActiveFlag)
                            {
                                ChillerParam.IsAbortActivate = tempActiveFlag;
                                //this.EnvControlManager().SaveSysParameter();
                            }
                        }
                        _ChillerInfo.TargetTemp = ChillerComm.GetSetTempValue(0);

                        if (UpdateThread?.IsAlive == true)
                        {
                            //log 추가 필요
                            LoggerManager.Debug($"[CHI][Chiller #{_ChillerInfo.Index}] UpdateThread abnormal state when start ");
                            PrepareThreadStop();
                            UpdateThread?.Abort();
                            UpdateThread = null;
                        }
                        UpdateThread = new Thread(new ThreadStart(UpdateProc));
                        bIsUpdating = true;
                        UpdateThread.Name = $"Chiller #{_ChillerInfo.Index} Update thread";
                        UpdateThread.Start();
                        LoggerManager.Debug($"[CHI][Chiller #{_ChillerInfo.Index}] Start Thread ");


                        _ChillerInfo.SetTemp = ChillerComm.GetSetTempValue(0);
                        Thread.Sleep(500);

                        _ChillerInfo.ChillerInternalTemp = ChillerComm.GetInternalTempValue(0);
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum DisConnect(bool bNormalEnd = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (bNormalEnd)
                {
                    //사용자가 UI로 셀과 로더를 끊은 경우이다. 이경우에는 notify를 하지 않는다.(notify buffering 되어 이후 재연결시 message가 표시되게 된다.)
                    //별도 thread에서 IsConnected 상태는 정상적으로 변경되게 된다.                    
                }
                else
                {
                    //로더 셀 연결 상태에서 chiller 연결만 끊긴 경우 이다.
                    IsConnected = false;
                }
                PrepareThreadStop();
                ChillerComm?.DisConnect();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _ChillerInfo.WarningReport.Clear();
                });

                _ChillerInfo.ErrorReport = 0;
                CommunicationState = EnumCommunicationState.DISCONNECT;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool isDispossing)
        {
            if (!this.isDisposed)
            {
                if (isDispossing)
                {
                    PrepareThreadStop();
                    if (UpdateThread?.IsAlive == true)
                    {
                        UpdateThread?.Join();
                    }
                    ChillerComm?.Dispose();
                    ChillerComm = null;
                }
                else
                {

                }
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}, ChillerIndex = {this._ChillerInfo.Index}");
            }
        }

        public EventCodeEnum Activate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Change Activate Mode
                if (_ChillerInfo.ChillerMode == CillerModeEnum.ONLINE)
                {
                    int pumpSpeed = 0;
                    if (ChillerParam.ChillerModuleMode.Value == EnumChillerModuleMode.CHAGO)
                        pumpSpeed = 0;
                    else
                        pumpSpeed = GetPumpSpeed();
                    if (pumpSpeed < 150)
                    {
                        ChillerComm.SetCircuationActive(true, 0);//ISSD-3554 임시수정. DewPoint 고려 필요
                        ChillerComm.SetTempActiveMode(true, 0);
                        _ChillerInfo.SetActiveState(true);
                        LoggerManager.Debug($"[CHILLER DEBUG]Activate success");
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.CHILLER_PUMP_SPPED_NOT_ZERO;
                        LoggerManager.Debug($"[CHILLER DEBUG]Activate fail. Error code is {retVal}");
                    }
                }
                else
                {
                    retVal = EventCodeEnum.CHILLER_MODE_ISNOT_ONLINE;
                    LoggerManager.Debug($"[CHILLER DEBUG]Activate fail. Error code is {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum Inactivate(bool isEMO = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_ChillerInfo.ChillerMode == CillerModeEnum.ONLINE)
                {
                    if (IsTempControlActive() == true)
                    {
                        if (isEMO == true)
                        {
                            ChillerComm.SetCircuationActive(false, 0);//ISSD-3554 임시수정. DewPoint 고려 필요
                            ChillerComm.SetTempActiveMode(false, 0);
                            _ChillerInfo.SetActiveState(false);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            if (this.EnvControlManager().CanInactivateChiller(ChillerParam.StageIndexs))
                            {
                                ChillerComm.SetCircuationActive(false, 0);//ISSD-3554 임시수정. DewPoint 고려 필요
                                ChillerComm.SetTempActiveMode(false, 0);
                                _ChillerInfo.SetActiveState(false);
                                LoggerManager.Debug($"[CHILLER DEBUG]Inactivate success");
                                retVal = EventCodeEnum.NONE;
                            }
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.CHILLER_MODE_ISNOT_ONLINE;
                    LoggerManager.Debug($"[CHILLER DEBUG]Inactivate fail. Error code is {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }



        //Timeout 은 통신 타임아웃보다 2배 이상되어야함. 통신문제일 경우 통신에러를 먼저 띄우기 위해서..
        //ChillerSysParam.PingTimeout = 300msec
        public void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00, int timeoutMsec = 600)
        {
            try
            {
                ChillerComm.SetCircuationActive(bValue, subModuleIndex);

                DateTime startTime = DateTime.Now;
                int retrycnt = 0;
                int retryMax = 3;

                Thread.Sleep(10);//TODO: chiller 동작 시간 delay 파라미터 제어 필요
                bool getVal = IsCirculationActive();


                while (retrycnt < retryMax)
                {
                    Thread.Sleep(10);

                    if (getVal != bValue)
                    {
                        Thread.Sleep(10);

                        if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMsec)
                        {
                            startTime = DateTime.Now;
                            ChillerComm.SetCircuationActive(bValue, subModuleIndex);

                            retrycnt += 1;
                            LoggerManager.Debug($"[Chiller #{_ChillerInfo.Index}].SetCircuationActive(): retry({retrycnt}) target:{bValue}, cur:{getVal} ");

                            getVal = IsCirculationActive();
                        }
                    }
                    else
                    {
                        //retVal = EventCodeEnum.NONE;
                        break;
                    }
                }

                if (getVal != bValue)
                {
                    LoggerManager.Debug($"[Chiller #{_ChillerInfo.Index}].SetCircuationActive(): Failed. target:{bValue}, cur:{getVal} ");
                }



            }
            catch (Exception error)
            {
                LoggerManager.Exception(error, $"[Chiller #{_ChillerInfo.Index}] SetCircuationActive failed.");
            }
        }

        private object checkCanUseChillerLockObj = new object();
        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_ChillerInfo.ChillerMode == CillerModeEnum.ONLINE)
                {
                    if (GetChillerMode() == EnumChillerModuleMode.REMOTE)
                    {
                        retVal = ChillerComm.CheckCanUseChiller(sendVal, stageindex, offvalve, forcedSetValue);
                    }
                    else if (GetChillerMode() == EnumChillerModuleMode.HUBER || GetChillerMode() == EnumChillerModuleMode.SOLARDIN || GetChillerMode() == EnumChillerModuleMode.EMUL)
                    {
                        lock (checkCanUseChillerLockObj)
                        {
                            double targetTemp = sendVal;
                            double chillertargetTemp = targetTemp + GetChillerTempoffset(targetTemp);

                            /// Chiller Group 확인
                            var anotherStageIndex = ChillerParam.StageIndexs.Where(index => index != stageindex);
                            if (anotherStageIndex.Count() > 0 && SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                            {
                                ILoaderSupervisor loaderSupervisor = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                                ILoaderCommunicationManager loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                                List<(IStageObject, ILoaderServiceCallback)> connectedCellInfos = new List<(IStageObject, ILoaderServiceCallback)>();

                                //if (_ChillerInfo.ChillerTargetTempOfStage != -999 && (_ChillerInfo.ChillerTargetTempOfStage != _ChillerInfo.SetTemp))
                                if (_ChillerInfo.IsOccupied)
                                {
                                    if (_ChillerInfo.TargetTempSetStageIndex != 0)
                                    {
                                        if (_ChillerInfo.TargetTempSetStageIndex != stageindex)
                                        {
                                            /// Chiller 온도 설정하려는 셀의 정보가 있지만 아직 Chiller 의 온도가 변경 되지 않은경우
                                            /// 온도 변경을 하려고 하나 아직 온도 변경이 완료 되지 않은 것으로 보고 기다려야 한다.
                                            /// 해당 셀이 정상적으로 연결되어 있을 때만 유효함.
                                            var cellinfo = loaderCommunicationManager.GetStage(_ChillerInfo.TargetTempSetStageIndex);
                                            if (cellinfo != null && cellinfo.StageInfo.IsConnected)
                                            {
                                                retVal = EventCodeEnum.CHILLER_CHECK_ALREADY_OCCUPY;
                                                return retVal;
                                            }
                                            else
                                            {
                                                _ChillerInfo.ResetTargetTempSetStageInfo();
                                            }
                                        }
                                        else
                                        {
                                            /// 온도 변경한 셀이 물어본다면 동일하게 NONE 을 반환함.
                                            retVal = EventCodeEnum.NONE;
                                            return retVal;
                                        }
                                    }
                                }

                                // Stage 정보가 없거나 연결되어 있지 않은 셀index 정보
                                //List<int> inValidCellIndexs = new List<int>();

                                /// Cell 정보 가져오기 (loader 와 연결되어 있는 Cell 들만 유효한 상태로 본다.)
                                foreach (var index in anotherStageIndex)
                                {
                                    var cellinfo = loaderCommunicationManager.GetStage(index);
                                    var client = loaderSupervisor.GetClient(index);

                                    if (cellinfo != null && client != null && cellinfo.StageInfo.IsConnected)
                                    {
                                        connectedCellInfos.Add((cellinfo, client));
                                    }
                                    //else
                                    //{
                                    //    inValidCellIndexs.Add(index);
                                    //}
                                }

                                /// <summary> Chiller Group 칠러 점유 우선순의
                                /// 칠러 점유 : 셀이 원하는 온도로 칠러온도를 설정할 수 있다
                                /// 1. LOT 가 Running 중인 경우
                                /// 2. AmbientTemp도 미만의 저온의 chiller 온도를 사용
                                /// 3. 먼저 칠러를 사용하고 있던 경우 ( coolant IN valve 가 open 되어 있는 경우)
                                /// </summary>

                                var runningCells = connectedCellInfos.FindAll(cellinfo => cellinfo.Item1.StageState != ModuleStateEnum.IDLE);

                                if (runningCells != null && runningCells.Count > 0)
                                {
                                    /// LOT 가 Idle 상태가 아니더라도 Target 온도가 일치한다면 true, 한 셀 이상이라도 Target 온도가 일치하지 않는 온도가 있다면 false
                                    bool isTargetTempMatched = true;
                                    /// 할당 된 Cell 중 Wafer 가 Load 된 셀이 없다면 true.
                                    bool isNotExistProcessingLotAssingStateCell = true;

                                    foreach (var info in runningCells)
                                    {
                                        IStageObject stageObject = info.Item1;
                                        ILoaderServiceCallback loaderServiceCallback = info.Item2;

                                        bool iswaferLoaded = loaderServiceCallback.IsHasProcessingLotAssignState();
                                        if (iswaferLoaded)
                                        {
                                            /// Processing 상태라면 (wafer load 이후)
                                            isNotExistProcessingLotAssingStateCell = false;
                                            break;
                                        }
                                        else
                                        {
                                            double targetTempOfTargetCell = loaderServiceCallback.GetStageInfo()?.TargetTemp ?? -999;
                                            if (targetTemp != -999 && targetTempOfTargetCell != -999)
                                            {
                                                if (targetTemp != targetTempOfTargetCell)
                                                {
                                                    isTargetTempMatched = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    /// Processing 상태인 Cell 이 없다면 온도 변경 가능
                                    if (isNotExistProcessingLotAssingStateCell)
                                    {
                                        ///Idle 상태가 아니어도 Target 온도가 같다면 온도 변경 가능.
                                        if (isTargetTempMatched)
                                        {
                                            retVal = EventCodeEnum.NONE;
                                        }
                                        else
                                        {
                                            retVal = EventCodeEnum.CHILLER_CHECK_CHILLER_GROUP_ERROR;
                                        }
                                    }
                                    else
                                    {
                                        ///Processing 인 Cell 들이 있지만, 칠러와 같은 온도로 이미 Valve 가 열려있다면 동작 허용
                                        if (_ChillerInfo.SetTemp == chillertargetTemp)
                                        {
                                            if (this.EnvControlManager().GetValveState(EnumValveType.IN, stageindex))
                                            {
                                                retVal = EventCodeEnum.NONE;
                                            }
                                            else
                                            {
                                                retVal = EventCodeEnum.CHILLER_CHECK_CHILLER_GROUP_ERROR;
                                            }
                                        }
                                        else
                                        {
                                            retVal = EventCodeEnum.CHILLER_CHECK_CHILLER_GROUP_ERROR;
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    ///다른 Cell 들이 모두 Idle 상태라면 조건 확인
                                    /// 1. LOT 를 할당 받았는지 중인 경우
                                    /// 2. AmbientTemp도 미만의 저온의 chiller 온도를 사용
                                    /// 3. 먼저 칠러를 사용하고 있던 경우 (coolant IN valve 가 open 되어 있는 경우)

                                    if (_ChillerInfo.SetTemp == chillertargetTemp)
                                    {
                                        /// Chiller target 온도가 동일하면 none.
                                        retVal = EventCodeEnum.NONE;
                                    }
                                    else
                                    {
                                        var requestClient = loaderSupervisor.GetClient(stageindex);
                                        var requestStageLotInfos = requestClient.GetStageLotInfos();
                                        var requestStagecellinfo = loaderCommunicationManager.GetStage(stageindex);

                                        bool requestClientIsLotAssigned = false;
                                        if (requestStageLotInfos != null)
                                        {
                                            requestClientIsLotAssigned = (requestStageLotInfos.FindAll(info => info.StageLotAssignState == LotAssignStateEnum.ASSIGNED).Count > 0) ? true : false;
                                        }

                                        if (requestClientIsLotAssigned && requestStagecellinfo.StageMode == GPCellModeEnum.ONLINE)
                                        {
                                            retVal = EventCodeEnum.NONE;
                                            /// 랏드 할당 받았더라도 다른 셀도 랏드 할당 되어 있고, Target 온도가 다른 경우에는 온도 변경 불가능.
                                            foreach (var cellinfo in connectedCellInfos)
                                            {
                                                var stageLotInfos = cellinfo.Item2.GetStageLotInfos();
                                                if(stageLotInfos != null)
                                                {
                                                    bool lotAssigned = (stageLotInfos.FindAll(info => info.StageLotAssignState == LotAssignStateEnum.ASSIGNED).Count > 0) ? true : false;
                                                    if(lotAssigned)
                                                    {
                                                        double stageSetTemp = this.GetLoaderContainer().Resolve<ILoaderSupervisor>().StageSetTemp[cellinfo.Item1.Index - 1];
                                                        var stageChillerTargetTemp = stageSetTemp + GetChillerTempoffset(stageSetTemp);
                                                        if(_ChillerInfo.SetTemp == stageChillerTargetTemp)
                                                        {
                                                            retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_DIFF_ACTIVE;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /// Chiller Group 중 연결되어있는 cell들의 정보 확인
                                            foreach (var cellinfo in connectedCellInfos)
                                            {
                                                int cellIdx = cellinfo.Item1.Index;

                                                double stageSetTemp = this.GetLoaderContainer().Resolve<ILoaderSupervisor>().StageSetTemp[cellIdx - 1];
                                                var stageChillerTargetTemp = stageSetTemp + GetChillerTempoffset(stageSetTemp);
                                                var stagecellinfo = loaderCommunicationManager.GetStage(cellIdx);
                                                var stageLotInfos = cellinfo.Item2.GetStageLotInfos();

                                                if (stageLotInfos != null)
                                                {
                                                    requestClientIsLotAssigned = (stageLotInfos.FindAll(info => info.StageLotAssignState == LotAssignStateEnum.ASSIGNED).Count > 0) ? true : false;
                                                }

                                                //(1). 다른 셀이 LOT 할당 & ONLINE 상태면 온도 변경 못함.
                                                if (requestClientIsLotAssigned && cellinfo.Item1.StageMode == GPCellModeEnum.ONLINE)
                                                {
                                                    retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_DIFF_ACTIVE;
                                                    break;
                                                }

                                                // (2). AmbientTemp 미만의 온도를 사용하고 있는 셀이 있다면 칠러 온도 설정 불가
                                                if (stageSetTemp < ChillerParam.AmbientTemp.Value)
                                                {
                                                    if (_ChillerInfo.TargetTempSetStageIndex != 0 && _ChillerInfo.SetTemp == stageChillerTargetTemp)
                                                    {
                                                        retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_DIFF_ACTIVE;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    // (3). AmbientTemp 이상의 온도를 사용하고 있는 셀이 있고 칠러 온도와 칠러설정온도가 동일하면 철러 온도 설정 불가
                                                    if (this.EnvControlManager().GetValveState(EnumValveType.IN, cellIdx))
                                                    {
                                                        if (_ChillerInfo.TargetTempSetStageIndex != 0 && _ChillerInfo.SetTemp == stageChillerTargetTemp)
                                                        {
                                                            // (4). 설정 목표 온도가 Ambient 이상이면 온도 설정 불가
                                                            if (targetTemp >= ChillerParam.AmbientTemp.Value)
                                                            {
                                                                retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_DIFF_ACTIVE;
                                                                break;
                                                            }
                                                            // (5). 설정 목표 온도가 Ambient 미만이면 온도 설정 가능
                                                        }
                                                    }
                                                }
                                            }


                                            if (retVal != EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_DIFF_ACTIVE)
                                            {
                                                retVal = EventCodeEnum.NONE;
                                            }
                                        }
                                    }
                                }

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    if (_ChillerInfo.ChillerTargetTempOfStage != chillertargetTemp)
                                    {
                                        _ChillerInfo.SetTargetTempSetStageInfo(stageindex, sendVal, chillertargetTemp);
                                    }
                                }
                            }
                            else
                            {
                                /// Chiller Group 에 다른 Stage 없는 경우
                                retVal = EventCodeEnum.NONE;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EnumChillerModuleMode GetChillerMode()
        {
            EnumChillerModuleMode enumChillerModuleMode = EnumChillerModuleMode.NONE;
            try
            {
                if (ChillerParam != null)
                {
                    enumChillerModuleMode = ChillerParam.ChillerModuleMode.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return enumChillerModuleMode;
        }

        public bool CanRunningLot()
        {
            bool retVal = false;
            try
            {
                if (ChillerParam.ChillerModuleMode.Value != EnumChillerModuleMode.NONE)
                {
                    bool isConnected = IsConnected;
                    double coolantInTemp = ChillerParam.CoolantInTemp.Value;
                    double sv = this.TempController().GetSetTemp();
                    bool isErrorState = _ChillerInfo.IsErrorState;

                    //  0: Temperature control not active.
                    //  1: Temperature control active.
                    bool coolantActiveState = _ChillerInfo.CoolantActivate;
                    //start or stop circulation
                    //0: Circulation operationg mode not active.
                    //1: Circulation operationg mode active.
                    //Note: If the temperatue control is active, 
                    //  circulation is also carried out, but circulation operationg mode is not active.
                    bool isCirculationActiveState = IsCirculationActive();
                    bool inValveState = this.EnvControlManager().GetValveState(EnumValveType.IN);
                    bool outValveState = this.EnvControlManager().GetValveState(EnumValveType.OUT);

                    if (coolantInTemp < sv)
                    {
                        retVal = true;
                    }
                    else
                    {
                        if (isConnected)
                        {
                            if (coolantInTemp >= sv)
                            {
                                if (isErrorState == false)
                                {
                                    retVal = true;
                                }
                            }
                            else
                            {
                                // 칠러가 동작하지 않는 상태에서는 Active 상태를 확인한다.
                                if (coolantActiveState == false)
                                {
                                    retVal = true;
                                }
                                else
                                {
                                    // TODO : 칠러를 사용하지 않는 온도에서 순환모드를 확인하도록 하기
                                    retVal = true;

                                    //칠러가 Active 상태이지만 내부순환 모드이고 Valve 가 닫혀있으면 안전한 상태
                                    //if(isCirculationActiveState == false)
                                    //{
                                    //    if(inValveState == false && outValveState == false)
                                    //    {
                                    //        retVal = true;
                                    //    }
                                    //}
                                }
                            }
                        }
                    }
                    if (retVal == false)
                    {
                        LoggerManager.Debug($"[CHILLER DEBUG] ChillerModule#{_ChillerInfo.Index} CanRunningLot Fail. " +
                            $"Connect State : {isConnected}, CoolantInTemp : {coolantInTemp}, SV : {sv},  ErrorState : {isErrorState}, " +
                            $"CoolantActiveState : {coolantActiveState}, CirculationActiveState : {isCirculationActiveState}" +
                            $"InValveState : {inValveState}, OutValveState : {outValveState}");
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsMatchedTargetTemp(double targetTemp)
        {
            bool retVal = false;
            try
            {
                double chillerTargetTemp = targetTemp + GetChillerTempoffset(targetTemp);
                if (_ChillerInfo.SetTemp == -999 || chillerTargetTemp == _ChillerInfo.SetTemp)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public double ConvertTargetTempApplyOffset(double targetTemp)
        {
            double chillerTargetTemp = -999;
            try
            {
                chillerTargetTemp = targetTemp + GetChillerTempoffset(targetTemp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return chillerTargetTemp;
        }

        public void SetPumpSpeed(ChillerPumpingSpeedType pumpSpeedType)
        {
            double pumpSpeed = 0;
            if (pumpSpeedType == ChillerPumpingSpeedType.NORMAL)
            {
                pumpSpeed = ChillerParam.NormalPumpSpeed.Value;
            }
            else if (pumpSpeedType == ChillerPumpingSpeedType.SLOW)
            {
                pumpSpeed = ChillerParam.SlowPumpSpeed.Value;
            }
            else if (pumpSpeedType == ChillerPumpingSpeedType.FAST)
            {
                pumpSpeed = ChillerParam.FastPumpSpeed.Value;
            }

            ChillerComm.SetSetTempPumpSpeed((int)pumpSpeed, 0);
        }

        public void SetPumpSpeed(double pumpSpeed)
        {
            ChillerComm.SetSetTempPumpSpeed((int)pumpSpeed, 0);
        }

        public EventCodeEnum SetSlowChilling(double targetTemp, TempValueType targetTempValueType)
        {
            _ChillerInfo.TargetTemp = ConvertTargetTempApplyOffset(targetTemp);
            return ChillerComm.SetTargetTemp(targetTemp, targetTempValueType, 0);
        }

        public EventCodeEnum SetNormalChilling(double targetTemp, TempValueType targetTempValueType, bool forcedSetValue = false)
        {
            _ChillerInfo.TargetTemp = ConvertTargetTempApplyOffset(targetTemp);
            return ChillerComm.SetTargetTemp(targetTemp, targetTempValueType, 0, forcedSetValue);
        }

        public EventCodeEnum SetFastChilling(double targetTemp, TempValueType targetTempValueType)
        {
            _ChillerInfo.TargetTemp = ConvertTargetTempApplyOffset(targetTemp);
            return ChillerComm.SetTargetTemp(targetTemp, targetTempValueType, 0);
        }

        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior)
        {
            try
            {
                ChillerComm.SetOperatingLock(bOperatinglock, bWatchdogBehavior, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public double GetChillerTempoffset(double targetTemp)
        {
            double offsetVal = 0.0;
            try
            {
                if (ChillerParam.CoolantOffsetDictionary.Value.ContainsKey(targetTemp))
                {
                    offsetVal = ChillerParam.CoolantOffsetDictionary.Value[targetTemp];
                }
                else
                {
                    offsetVal = ChillerParam.CoolantOffsetDictionary.Value.OrderBy(e => Math.Abs(e.Key - targetTemp)).FirstOrDefault().Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return offsetVal;
        }

        public ICommunicationMeans GetCommunicationObj()
        {
            ICommunicationMeans commObj = null;
            try
            {
                commObj = ChillerComm.GetCommunicationObj();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return commObj;
        }

        public object GetCommLockObj()
        {
            object lockObj = null;
            try
            {
                lockObj = ChillerComm.GetCommLockObj();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lockObj;
        }
        #region ==> Set Data to Chiller
        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_ChillerInfo.ChillerMode == CillerModeEnum.ONLINE)
                {
                    lock (checkCanUseChillerLockObj)
                    {
                        double targetTemp = ConvertTargetTempApplyOffset(sendVal);
                        _ChillerInfo.TargetTemp = targetTemp;

                        LoggerManager.Debug($"[Chiller #{_ChillerInfo.Index}] Chiller tartettemp set to {_ChillerInfo.TargetTemp}.");

                        // 연결안된 셀들의 벨브를 닫는다. 
                        CloseCoolantValveForDisconnectCells();
                        retVal = ChillerComm.SetTargetTemp(targetTemp, sendTempValueType, 0);

                        //if (retVal == EventCodeEnum.NONE)
                        //{
                        //    _ChillerInfo.TargetTemp = targetTemp;
                        //}
                    }
                }
                else
                    retVal = EventCodeEnum.CHILLER_MODE_ISNOT_ONLINE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private void CloseCoolantValveForDisconnectCells()
        {
            try
            {
                if(ChillerParam.StageIndexs!= null && ChillerParam.StageIndexs.Count > 1)
                {
                    foreach (var index in ChillerParam.StageIndexs)
                    {
                        ILoaderCommunicationManager loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                        var cellinfo = loaderCommunicationManager.GetStage(index);
                        
                        if ((cellinfo != null && cellinfo.StageInfo.IsConnected) == false)
                        {
                            if(this.EnvControlManager().GetValveState(EnumValveType.IN, index))
                            {
                                this.EnvControlManager().SetValveState(false, EnumValveType.IN, index);
                                LoggerManager.Debug($"[CHI][Chiller #{_ChillerInfo.Index}] CloseCoolantValveForDisconnectCells() target cell index : {index}.");
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


        public void SetSetTempPumpSpeed(int iValue)
        {
            ChillerComm.SetSetTempPumpSpeed(iValue, 0);
        }
        #endregion

        #region ==> Get Data from Chiller

        public EnumCommunicationState GetCommState()
        {
            return ChillerComm?.GetCommState(0) ?? EnumCommunicationState.DISCONNECT;
        }
        public double GetSetTempValue()
        {
            double retVal = 0;

            try
            {
                retVal = ChillerComm?.GetSetTempValue(0) ?? -1;
                _ChillerInfo.SetTemp = retVal;

                if (_ChillerInfo.IsOccupied && _ChillerInfo.ChillerTargetTempOfStage == _ChillerInfo.SetTemp)
                {
                    _ChillerInfo.ChangedSetTemp();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
        public double GetReturnTempVal()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetReturnTempVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetProcessTempVal()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetProcessTempVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetInternalTempValue()
        {

            //return ChillerComm.GetInternalTempValue();
            double retVal = 0;
            try
            {
                if(ChillerComm != null)
                {
                    _ChillerInfo.ChillerInternalTemp = Math.Round(ChillerComm.GetInternalTempValue(0), 3);
                }
                retVal = _ChillerInfo.ChillerInternalTemp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetExtMoveVal()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetExtMoveVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetMinSetTemp()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetMinSetTemp(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetMaxSetTemp()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetMaxSetTemp(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetCurrentPower()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetCurrentPower(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetPumpPressureVal()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetPumpPressureVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetSetTempPumpSpeed()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetSetTempPumpSpeed(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetPumpSpeed()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetPumpSpeed(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetErrorReport()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetErrorReport(0) ?? 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetWarningMessage()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetWarningMessage(0) ?? 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetStatusOfThermostat()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetStatusOfThermostat(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsAutoPID()
        {
            bool retVal = false;
            try
            {
                retVal = ChillerComm?.IsAutoPID(0) ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsTempControlProcessMode()
        {
            bool retVal = false;
            try
            {
                retVal = ChillerComm?.IsTempControlProcessMode(0) ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsTempControlActive()
        {
            bool retVal = false;
            try
            {
                _ChillerInfo.CoolantActivate = ChillerComm?.IsTempControlActive(0) ?? false;
                retVal = _ChillerInfo.CoolantActivate;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            //return ChillerComm.IsTempControlActive();
            return retVal;
        }
        public (bool, bool) GetProcTempActValSetMode()
        {
            (bool, bool) retVal = (false, false);
            try
            {
                retVal = ChillerComm?.GetProcTempActValSetMode(0) ?? (false, false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetSerialNumLow()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetSerialNumLow(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetSerialNumHigh()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetSerialNumHigh(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public int GetSerialNumber()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerComm?.GetSerialNumber(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsCirculationActive()
        {
            return ChillerComm?.IsCirculationActive(0) ?? false;
        }
        public (bool, bool) IsOperatingLock()
        {
            return ChillerComm?.IsOperatingLock(0) ?? (false, false);
        }
        public double GetUpperAlramInternalLimit()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetUpperAlramInternalLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetLowerAlramInternalLimit()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetLowerAlramInternalLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetUpperAlramProcessLimit()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetUpperAlramProcessLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public double GetLowerAlramProcessLimit()
        {
            double retVal = 0;
            try
            {
                retVal = ChillerComm?.GetLowerAlramProcessLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private void PrepareThreadStop()
        {
            bIsUpdating = false;
            areUpdateEvent.Set();
        }
        #endregion
    }
}
