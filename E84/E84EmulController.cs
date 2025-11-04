namespace E84
{
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Communication.E84;
    using ProberInterfaces.E84;
    using ProberInterfaces.E84.ProberInterfaces;
    using ProberInterfaces.Foup;
    using ProberInterfaces.State;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Collections.Generic;

    public class E84EmulController : IE84Controller, INotifyPropertyChanged
    {
        public E84EmulController()
        {
            CommModule = new EmulCommModule();
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Property </remarks>

        public bool Initialized { get; set; } = false;
        private Thread thread;

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.E84);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84Parameters _E84ParameterObject;
        public E84Parameters E84ParameterObject
        {
            get { return _E84ParameterObject; }
            set
            {
                if (value != _E84ParameterObject)
                {
                    _E84ParameterObject = value;
                    RaisePropertyChanged();
                }
            }
        }


        private E84Parameters _E84Param;
        public E84Parameters E84Param
        {
            get { return _E84Param; }
            set
            {
                if (value != _E84Param)
                {
                    _E84Param = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private E84StateBase _E84State;
        public E84StateBase E84State
        {
            get { return _E84State; }
        }

        public IInnerState InnerState
        {
            get { return _E84State; }
            set
            {
                if (value != _E84State)
                {
                    _E84State = value as E84StateBase;
                }
            }
        }

        private ModuleStateEnum _ModulestateEnum;
        public ModuleStateEnum ModulestateEnum
        {
            get { return _ModulestateEnum; }
            set
            {
                if (value != _ModulestateEnum)
                {
                    _ModulestateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IInnerState PreInnerState { get; set; }

        private IE84BehaviorState _E84BehaviorState;

        public IE84BehaviorState E84BehaviorState
        {
            get { return _E84BehaviorState; }
            set { _E84BehaviorState = value; }
        }


        private bool _IsChangedE84Mode = false;
        public bool IsChangedE84Mode
        {
            get { return _IsChangedE84Mode; }
            set { _IsChangedE84Mode = value; }
        }

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set { _FoupIndex = value; }
        }

        private IE84CommModule _CommModule;
        public IE84CommModule CommModule
        {
            get { return _CommModule; }
            private set
            {
                if (value != _CommModule)
                {
                    _CommModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// E84 Event number
        /// </summary>
        private int _EventNumber;
        public int EventNumber
        {
            get { return _EventNumber; }
            private set
            {
                if (value != _EventNumber)
                {
                    _EventNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupController _FoupController;
        public IFoupController FoupController
        {
            get
            {
                return GetFoupController();
            }
            set { _FoupController = value; }
        }

        private IE84ModuleParameter _E84ModuleParaemter;
        public IE84ModuleParameter E84ModuleParaemter
        {
            get { return _E84ModuleParaemter; }
            set
            {
                if (value != _E84ModuleParaemter)
                {
                    _E84ModuleParaemter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupController GetFoupController()
        {
            IFoupController controller = null;
            try
            {
                if (_FoupController == null)
                {
                    _FoupController = this.FoupOpModule().GetFoupController(FoupIndex);
                    if (_FoupController.GetFoupIO().IOMap.Inputs.DI_CST_LOCK12s[FoupIndex - 1].Value)
                    {
                        SetClampSignal(true);
                    }
                    else
                    {
                        SetClampSignal(false);
                    }
                }
                else
                {
                    controller = _FoupController;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return controller;
        }

        private bool _IsClampLockSignal;
        public bool IsClampLockSignal
        {
            get { return _IsClampLockSignal; }
            set
            {
                if (value != _IsClampLockSignal)
                {
                    _IsClampLockSignal = value;
                    RaisePropertyChanged();
                }
            }
        }


        public FoupStateEnum FoupBehaviorStateEnum { get; set; }
        private ObservableCollection<E84ErrorParameter> E84Errors { get; set; }
        private E84OPModuleTypeEnum _E84OPMOduleType;

        public E84OPModuleTypeEnum E84OPMOduleType
        {
            get { return _E84OPMOduleType; }
            set { _E84OPMOduleType = value; }
        }

        public object lockobject = new object();

        #endregion


        #region <remarks> Init & DeInit </remarks>
        public EventCodeEnum InitModule(E84ModuleParameter moduleparam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    if (Initialized == false)
                    {
                        E84ModuleParaemter = moduleparam;

                        E84OPMOduleType = moduleparam.E84OPModuleType;
                        if (moduleparam.E84_Attatched == true)
                        {
                            var commmodule =  (EmulCommModule)CommModule;
                            commmodule.InitConnect();
                        }
                        FoupIndex = moduleparam.FoupIndex;
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        private bool isExcuteThread = false;
        private bool errHandled = false;
        private void ExecuteRun()
        {
            while (isExcuteThread)
            {
                try
                {
                    Execute();
                    if (errHandled == true)
                    {
                        LoggerManager.Debug($"E84Controller[{FoupIndex}] Error recovered.");
                        errHandled = false;
                    }
                    Thread.Sleep(1);
                }
                catch (Exception err)
                {
                    if (errHandled == false)
                    {
                        errHandled = true;
                        LoggerManager.Debug($"E84Controller[{FoupIndex}] Thread exception. Err = {err.Message}");
                    }
                }
            }
        }

        public void DeInitModule()
        {
            try
            {
                isExcuteThread = false;
                if (thread != null)
                {
                    thread.Join(5000);
                }
                thread = null;
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public void InitParameter()
        {

        }

        #endregion

        #region <remarks> Event </remarks>
        public void E84SignalChangedUpate(E84SignalActiveEnum activeenum, int pin, bool signal)
        {
            try
            {
                var signaltype = this.E84Module().PinConvertSignalType(activeenum, pin);
                LoggerManager.Debug($"[E84 Signal] {activeenum}  => pin : {signaltype}, signal : {signal}");

                //if(signaltype == E84SignalTypeEnum.CS_0 | signaltype == E84SignalTypeEnum.VALID)
                //{
                //    if (signal == true)
                //    {
                //        if (IsPresenceCarrier())
                //        {
                //            SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                //            E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CheckStartSequence()
        {
            bool retVal = false;
            try
            {
                if (GetSignal(E84SignalTypeEnum.CS_0) || GetSignal(E84SignalTypeEnum.VALID))
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void E84ErrorOccured(int errorCode = -1, E84EventCode code = E84EventCode.UNDEFINE)//, bool isForcedRecovery = true)
        {
            try
            {
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void FoupPresenceStateChanged(int foupIndex, bool presenceState)
        {
            try
            {
                if (IsPresenceCarrier())
                {
                    if (E84BehaviorState.GetSubStateEnum() == E84SubStateEnum.READY)
                    {
                        if ((FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR)
                            == FoupPRESENCEStateEnum.CST_DETACH)
                        {
                            if (GetSignal(E84SignalTypeEnum.CS_0) | GetSignal(E84SignalTypeEnum.VALID))
                            {
                                E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                            }
                            else
                            {
                                SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                            }
                        }
                    }
                }
                else
                {
                    if (GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                    {
                        SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> Method </remarks>

        public EventCodeEnum SetSignal(E84SignalTypeEnum signal, bool flag, E84Mode setMode = E84Mode.UNDEFIND)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int retval = -1;
                if (signal == E84SignalTypeEnum.ES)
                {
                    retval = CommModule?.SetEsSignal(flag ? 1 : 0) ?? -1;
                }
                else if (signal == E84SignalTypeEnum.HO_AVBL)
                {
                    if (CommModule != null)
                    {
                        if (flag == true && CommModule.RunMode == E84Mode.MANUAL)
                        {
                            if (setMode == E84Mode.AUTO)
                            {
                                retval = CommModule?.SetHoAvblSignal(flag ? 1 : 0) ?? -1;
                            }
                            else
                            {
                                LoggerManager.Debug($"#{FoupIndex}.[E84Signal] Cannot on HO_AVBL");
                            }
                        }
                        else
                        {
                            retval = CommModule?.SetHoAvblSignal(flag ? 1 : 0) ?? -1;
                        }
                    }
                }
                else
                {
                    retval = CommModule?.SetE84OutputSignal(this.E84Module().SignalTypeConvertPin(signal), (flag ? 1 : 0)) ?? -1;
                }

                if (retval == 0)
                {
                    LoggerManager.Debug($"#{FoupIndex}.[E84Signal] OUTPUT  => pin : {signal}, signal : {flag}");
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool GetSignal(E84SignalTypeEnum signal)
        {
            bool retVal = false;
            try
            {
                if (CommModule == null)
                    return false;

                switch (signal)
                {
                    case E84SignalTypeEnum.INVALID:
                        break;
                    case E84SignalTypeEnum.UNDEFINED:
                        break;
                    case E84SignalTypeEnum.L_REQ:
                        retVal = CommModule.E84Outputs.LReq;
                        break;
                    case E84SignalTypeEnum.U_REQ:
                        retVal = CommModule.E84Outputs.UlReq;
                        break;
                    case E84SignalTypeEnum.VA:
                        retVal = CommModule.E84Outputs.Va;
                        break;
                    case E84SignalTypeEnum.READY:
                        retVal = CommModule.E84Outputs.Ready;
                        break;
                    case E84SignalTypeEnum.VS_0:
                        retVal = CommModule.E84Outputs.VS0;
                        break;
                    case E84SignalTypeEnum.VS_1:
                        retVal = CommModule.E84Outputs.VS1;
                        break;
                    case E84SignalTypeEnum.HO_AVBL:
                        retVal = CommModule.E84Outputs.HoAvbl;
                        break;
                    case E84SignalTypeEnum.ES:
                        retVal = CommModule.E84Outputs.ES;
                        break;
                    case E84SignalTypeEnum.NC:
                        break;
                    case E84SignalTypeEnum.SELECT:
                        break;
                    case E84SignalTypeEnum.MODE:
                        break;
                    case E84SignalTypeEnum.GO:
                        retVal = CommModule.E84Inputs.Go;
                        break;
                    case E84SignalTypeEnum.VALID:
                        retVal = CommModule.E84Inputs.Valid;
                        break;
                    case E84SignalTypeEnum.CS_0:
                        retVal = CommModule.E84Inputs.CS0;
                        break;
                    case E84SignalTypeEnum.CS_1:
                        retVal = CommModule.E84Inputs.CS1;
                        break;
                    case E84SignalTypeEnum.AM_AVBL:
                        retVal = CommModule.E84Inputs.AmAvbl;
                        break;
                    case E84SignalTypeEnum.TR_REQ:
                        retVal = CommModule.E84Inputs.TrReq;
                        break;
                    case E84SignalTypeEnum.BUSY:
                        retVal = CommModule.E84Inputs.Busy;
                        break;
                    case E84SignalTypeEnum.COMPT:
                        retVal = CommModule.E84Inputs.Compt;
                        break;
                    case E84SignalTypeEnum.CONT:
                        retVal = CommModule.E84Inputs.Cont;
                        break;
                    case E84SignalTypeEnum.V24:
                        break;
                    case E84SignalTypeEnum.V24GNC:
                        break;
                    case E84SignalTypeEnum.SIGCOM:
                        break;
                    case E84SignalTypeEnum.FG:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool GetClampSignal(int foupindex = 0)
        {
            bool retVal = false;
            try
            {
                int signal = 0;
                if (CommModule != null)
                {
                    CommModule.GetClampSignal(foupindex, out signal);
                }

                retVal = signal == 0 ? false : true;
                //retVal = IsClampLockSignal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetCarrierState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetFoupBehaviorStateEnum(bool init = false)
        {
            try
            {
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetCardStateInBuffer()
        {
            return EventCodeEnum.NONE;
        }

        public void SetFoupBehaviorState(FoupStateEnum statenum)
        {
            FoupBehaviorStateEnum = statenum;
        }

        public void SetCardBehaviorStateEnum() { }

        public int SetMode(int mode)
        {
            int retVal = -1;
            try
            {
                /// 0 : Manual 
                /// 1 : Auto
                /// 
                if (InnerState?.GetModuleState() == ModuleStateEnum.ERROR)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message",
                        "E84 is in the error state so cannot be changed the mode.\r Please reset the status through the [Clear sequence] or [Initialize] menu and try again.", EnumMessageStyle.Affirmative);
                    return retVal;
                }

                retVal = CommModule.SetMode(mode);
                this.E84Module().SetFoupCassetteLockOption();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        /// <summary>
        /// Set aux output0 <br/>
        /// 0 : off <br/>
        /// 1 : on
        public EventCodeEnum SetOutput0(bool flag)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int output0 = flag ? 1 : 0;
                int ret = CommModule?.SetOutput0(output0) ?? -1;

                if (ret == 0)
                    retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetOutput0()
        {
            int retVal = -1;
            try
            {
                CommModule?.GetOutput0(out retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetClampSignal(bool onflag)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int clampon = onflag ? 1 : 0;
                if (this.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                {
                    CommModule?.SetClampSignal(0, clampon);
                    //IsClampLockSignal = onflag;
                    LoggerManager.Debug($"[E84 #{FoupIndex}] SetClampSignal => FoupIndex : {FoupIndex}, Signal :{onflag}");
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private E84EventCode eventCode = E84EventCode.UNDEFINE;
        public E84EventCode GetEventCode()
        {
            try
            {
                var param = this.E84Module().E84SysParam.E84Errors.SingleOrDefault
                    (errors => errors.ErrorNumber == EventNumber);
                if (param != null)
                {
                    eventCode = param.EventCode;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCode;
        }
        public void SetEventCode(E84EventCode code)
        {
            try
            {
                var param = this.E84Module().E84SysParam.E84Errors.SingleOrDefault
                     (errors => errors.EventCode == code);
                if (param != null)
                {
                    EventNumber = param.ErrorNumber;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int ClearEvent()
        {
            int retVal = -1;
            try
            {

                retVal = CommModule?.SetClearEvent(1) ?? -1;

                if (GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                {
                    SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                }

                EventNumber = CommModule?.GetEventNumber() ?? 0;
                ClearNotify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void ClearNotify()
        {
            try
            {
                this.NotifyManager().ClearNotify(EventCodeEnum.E84_LOAD_PORT_ACCESS_VIOLATION, E84ModuleParaemter.FoupIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ResetE84Interface()
        {
            try
            {
                if (CommModule != null)
                {
                    /// 0 : no action <br/>
                    /// 1 : reset E84 interface
                    var retVal = CommModule.ResetE84Interface(1);
                }
                else
                {
                    LoggerManager.Debug($"[E84] PORT{FoupIndex} ResetE84Interface() - CommModule is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsPresenceCarrier()
        {
            bool retVal = false;
            try
            {
                int fouparrayindex = FoupIndex - 1;
                bool presenceSensor =
                        this.FoupController.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[fouparrayindex].Value
                        && this.FoupController.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[fouparrayindex].Value;
                bool existSensor = false;
                if (this.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists != null)
                {
                    if (this.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists.Count > 0)
                    {
                        int ret = this.FoupController.GetFoupIO().MonitorForIO(
                            this.FoupController.GetFoupIO().IOMap.Inputs.DI_CST_Exists[fouparrayindex], true, 100, 300);
                        if (ret == 1)
                        {
                            existSensor = true;
                        }
                        else
                        {
                            existSensor = false;
                        }
                    }
                }
                LoggerManager.Debug($"#{FoupIndex}.[E84]Controller IsPresenceCarrier() => PresenceSensor : { presenceSensor}, ExistSensor : {existSensor}");
                retVal = presenceSensor || existSensor;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum ConnectCommodule(E84ModuleParameter moduleparam, List<E84PinSignalParameter> e84PinSignal)
        {
            var ret = EventCodeEnum.NONE;
            try
            {
                if (moduleparam.E84_Attatched == true)
                {
                    CommModule = new EmulCommModule();
                    CommModule.InitConnect();
                }
                FoupIndex = moduleparam.FoupIndex;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] : ConnectCommodule() err : {err}");
            }
            return ret;
        }

        public EventCodeEnum DisConnectCommodule()
        {
            var ret = EventCodeEnum.NONE;
            try
            {
                CommModule.StopCommunication();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] : ConnectCommodule() err : {err}");
            }
            return ret;
        }
        public void UpdateCardBufferState(bool forced_event = false)
        {
            //CardBufferModule.UpdateCardBufferState(forced_event);
        }

        #endregion

        #region <remarks> IStateModule Function </remarks>
        public ModuleStateEnum GetModuleStateEnum()
        {
            return InnerState?.GetModuleState() ?? ModuleStateEnum.UNDEFINED;
        }
        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                EventCodeEnum retVal = InnerState.Execute();

                //stat = InnerState.GetModuleState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stat;
        }
        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            InnerState.Pause();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            InnerState.Resume();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            InnerState.End();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Abort()
        {
            InnerState.Abort();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            return true;
        }
        //public bool IsBusy()
        //{
        //    bool retVal = false;
        //    try
        //    {
        //        foreach (var subModule in SubModules.SubModules)
        //        {
        //            if (subModule.GetMovingState() == MovingStateEnum.MOVING)
        //            {
        //                retVal = true;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return retVal;
        //}

        public void StateTransition(ModuleStateBase state)
        {

        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                PreInnerState = InnerState;
                InnerState = state;
                ModulestateEnum = InnerState.GetModuleState();
                LoggerManager.Debug($"#{FoupIndex}.[E84]Controller State Transition. Pre : {PreInnerState}, Curr : {InnerState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public void E84BehaviorStateTransition(IE84BehaviorState state)
        {
            try
            {
                LoggerManager.Debug($"#{FoupIndex}.[E84]Controller Behavior State Transition. Pre : {E84BehaviorState}, Curr : {state}");
                E84BehaviorState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            return retVal;
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }

        public bool IsManualOPReady(out string msg) //메뉴얼 동작시 조건 체크
        {
            msg = "";
            return true;
        }

        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            return retVal;
        }



        #endregion

        public void TempSetE84ModeStatus(bool entry)
        {
            try
            {
                var prevmode = CommModule.RunMode;
                //LoggerManager.Debug($"FoupIndex = {FoupIndex}, E84Mode = {prevmode}");
                if (entry == true)
                {
                    if (prevmode == E84Mode.AUTO)
                    {
                        CommModule.SetMode(0); //manual mode 변경
                        IsChangedE84Mode = true;

                        LoggerManager.Debug($"==Foup#{FoupIndex} Mode== Entry Foup Recovery Page: Change from E84Mode Auto to Manual");
                    }
                    else if (prevmode == E84Mode.MANUAL)
                    {
                        LoggerManager.Debug($"==Foup#{FoupIndex} Mode== Entry Foup Recovery Page: Cur E84Mode Manual");
                        IsChangedE84Mode = false;
                    }
                }
                else
                {
                    CommModule.SetMode(1); //auto mode로 변경
                    IsChangedE84Mode = false;

                    LoggerManager.Debug($"==Foup#{FoupIndex} Mode== Exit Foup Recovery Page: FoupIndex = {FoupIndex}, E84Mode is changed to Auto");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
