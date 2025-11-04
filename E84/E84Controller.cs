namespace E84
{
    using Autofac;
    using E84.CardAGV.State;
    using LoaderBase;
    using LogModule;
    using MetroDialogInterfaces;
    using Microsoft.VisualBasic;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Communication.E84;
    using ProberInterfaces.E84;
    using ProberInterfaces.E84.ProberInterfaces;
    using ProberInterfaces.Event;
    using ProberInterfaces.Foup;
    using ProberInterfaces.State;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Command.Internal;

    public class E84Controller : IE84Controller, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Fields
        private Thread thread;

        private bool isExcuteThread = false;
        private bool errHandled = false;

        private E84EventCode eventCode = E84EventCode.UNDEFINE;
        #endregion
        #region Properties
        public bool Initialized { get; set; } = false;
        private bool _PrePresenceSensor { get; set; }
        private bool _PreExistSensor { get; set; }

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
                    E84ErrorOccured(_EventNumber);
                }
            }
        }

        /// <summary>
        /// E84 Event Name
        /// </summary>
        private E84EventCode _EventCode;
        public E84EventCode EventCode
        {
            get { return _EventCode; }
            private set
            {
                if (value != _EventCode)
                {
                    _EventCode = value;
                    RaisePropertyChanged();
                    E84ErrorOccured(code: _EventCode);
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
            set
            {
                _FoupController = value;
            }
        }

        private E84ErrorParameter _CurErrorParam = new E84ErrorParameter();
        public E84ErrorParameter CurErrorParam//ISSD-2983
        {
            get { return _CurErrorParam; }
            private set
            {
                _CurErrorParam = value;
                LoggerManager.Debug($"Set CurErrorParam = {_CurErrorParam.ErrorAct}.");
            }
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

        ILoaderSupervisor LoaderMaster { get; set; }
        ICardBufferModule CardBufferModule { get; set; }
        public FoupStateEnum FoupBehaviorStateEnum { get; set; }
        public IIOManagerProxy IOManager { get; set; }
        public CardBufferOPEnum CardBehaviorStateEnum { get; set; }

        #endregion
        public E84Controller()
        {
            //Nullable용, 추후 EMotionTek이 아닌 CommModule이 생긴다면 E84Module쪽을 고치면서 변경해야 할 필요 있음.
            CommModule = new EMotionTekE84CommModule();
        }
        private IFoupController GetFoupController()
        {
            IFoupController controller = null;

            try
            {
                if (_FoupController == null)
                {
                    SetFoupController();
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

        private void SetFoupController()
        {
            try
            {
                if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                {
                    _FoupController = this.FoupOpModule().GetFoupController(E84ModuleParaemter.FoupIndex);

                    if (_FoupController != null)
                    {
                        _FoupController.GetFoupService().FoupModule.PresenceStateChangedEvent += FoupPresenceStateChanged;
                        _FoupController.GetFoupService().FoupModule.FoupClampStateChangedEvent += FoupClampStateChanged;

                        SetSignal(E84SignalTypeEnum.ES, true);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule(E84ModuleParameter moduleparam, List<E84PinSignalParameter> e84PinSignal)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //v22_merge// 코드 재확인 필요
                if (Initialized == false)
                {
                    E84ModuleParaemter = moduleparam;

                    if (moduleparam.E84_Attatched == true)
                    {
                        if (moduleparam.E84ModuleType == E84CommModuleTypeEnum.EMOTIONTEK)
                        {
                            if (moduleparam.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                            {
                                _E84State = new E84FoupIdleState(this);
                            }
                            else if (moduleparam.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                            {
                                _E84State = new E84CardAGVIdleState(this);
                            }

                            if (CommModule == null)
                            {
                                CommModule = new EMotionTekE84CommModule();
                            }
                            else
                            {
                                if (moduleparam.E84ConnType == E84ConnTypeEnum.SIMUL)
                                {
                                    CommModule = new SimulE84CommModule();
                                }

                                CommModule.E84ErrorOccured += E84ErrorOccured;
                                CommModule.E84ModeChanged += E84ModeChanged;

                                CommModule.SetParameter(moduleparam, e84PinSignal);

                                retVal = CommModule.InitModule();

                                switch (moduleparam.AccessMode.Value)
                                {
                                    case E84Mode.UNDEFIND:
                                        break;
                                    case E84Mode.MANUAL:
                                        SetMode(0);
                                        break;
                                    case E84Mode.AUTO:
                                        SetMode(1);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            bool runmode = false;

                            if (CommModule.RunMode == E84Mode.MANUAL)
                            {
                                runmode = false;
                            }
                            else if (CommModule.RunMode == E84Mode.AUTO)
                            {
                                runmode = true;
                            }

                            if (moduleparam.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                            {
                                E84BehaviorStateTransition(new E84SingleReadyState(this));

                                SetFoupController();

                                this.GEMModule().GetPIVContainer().SetFoupAccessMode(E84ModuleParaemter.FoupIndex, runmode);

                                FoupController.GetFoupService().FoupModule.UpdateFoupState();
                            }
                            else if (moduleparam.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                            {
                                CommModule.SetClampSignal(0, 0);// Card는 Clamp 가 없기 때문에 무조건 Unlclamp 상태로 만들어줌. Clamp신호를 제어하는 부분이 없어서 여기에서 한번만 Clear해줌. 
                                LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                                var LoaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();

                                CardBufferModule = LoaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, moduleparam.FoupIndex);
                                CardBufferModule.E84PresenceStateChangedEvent += CardPresenceStateChanged;
                                CardBufferModule.CardBufferStateUpdateEvent += UpdateCardBufferState;// 추후 Operation화면에 캐리어 반송을 위한 강제 이벤트 호출용으로 사용.
                                CardBufferModule.UpdateCardBufferState();

                                E84BehaviorStateTransition(new E84CardAGVSingleReadyState(this));

                                //LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                                //CardBufferModule = LoaderMaster.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, moduleparam.FoupIndex);
                                IOManager = this.GetLoaderContainer()?.Resolve<IIOManagerProxy>();

                                this.GEMModule().GetPIVContainer().GetCardBufferAccessMode(E84ModuleParaemter.FoupIndex).OriginPropertyPath = E84ModuleParaemter.AccessMode.PropertyPath;
                                this.GEMModule().GetPIVContainer().GetCardBufferAccessMode(E84ModuleParaemter.FoupIndex).ConvertToOriginTypeEvent += E84Mode_ConvertToOriginTypeEvent;

                                E84ModuleParaemter.AccessMode.ReportPropertyPath = this.GEMModule().GetPIVContainer().GetCardBufferAccessMode(E84ModuleParaemter.FoupIndex).PropertyPath;
                                this.GEMModule().GetPIVContainer().SetCardBufferAccessMode(moduleparam.FoupIndex, runmode);
                                this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(moduleparam.FoupIndex);
                            }

                            if (eventCode != 0)
                            {
                                if (GetSignal(E84SignalTypeEnum.HO_AVBL) == true)// 어차피 이 다음 E84ErrortOccured에서 AutoRecovery로 한번 처리를해줄것. 
                                {
                                    retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                                }
                            }

                            isExcuteThread = true;
                            thread = new Thread(new ThreadStart(ExecuteRun));
                            thread.Start();
                        }
                    }

                    E84ModuleParaemter.AccessMode.ConvertToReportTypeEvent += E84Mode_ConvertToReportTypeEvent;
                    E84ModuleParaemter.AccessMode.ValueChangedEvent += AccessMode_ValueChangedEvent;

                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private void ExecuteRun()
        {
            while (isExcuteThread)
            {
                try
                {
                    Execute();

                    if (errHandled == true)
                    {
                        errHandled = false;

                        LoggerManager.Debug($"E84Controller[{E84ModuleParaemter.FoupIndex}] Error recovered.");
                    }

                    Thread.Sleep(100);
                }
                catch (Exception err)
                {
                    if (errHandled == false)
                    {
                        errHandled = true;

                        LoggerManager.Debug($"E84Controller[{E84ModuleParaemter.FoupIndex}] Thread exception. Err = {err.Message}");
                    }
                }
            }
        }
        public void E84SignalChangedUpate(E84SignalActiveEnum activeenum, int pin, bool signal)
        {
            try
            {
                var signaltype = this.E84Module().PinConvertSignalType(activeenum, pin);

                LoggerManager.Debug($"[E84 {E84ModuleParaemter.E84OPModuleType}#{E84ModuleParaemter.FoupIndex} Signal] {activeenum}  => pin : {signaltype}, signal : {signal}");

                //Error 상태에서 시퀀스 시작되게되면 에러 발생.
                if (signaltype == E84SignalTypeEnum.CS_0 | signaltype == E84SignalTypeEnum.VALID)
                {
                    if (signal)
                    {
                        if (InnerState.GetModuleState() == ModuleStateEnum.ERROR)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Error Message", $"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} The sequence cannot be started in the Error state.\n Please proceed with initializing the error condition first.", EnumMessageStyle.Affirmative);
                        }
                    }
                }
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
                    //Error 상태에서 시퀀스 시작되게되면 에러 발생.

                    if (InnerState.GetModuleState() == ModuleStateEnum.ERROR)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Error Message", "[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} The sequence cannot be started in the Error state.\n Please proceed with initializing the error condition first.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        retVal = true;
                    }
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
        private EventCodeEnum E84ErrorRecovery(E84ErrorParameter param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Action changeErrorState = () =>
                {
                    if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                    {
                        InnerStateTransition(new E84FoupErrorState(this));
                        E84BehaviorStateTransition(new E84SingleReadyState(this));
                    }
                    else if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                    {
                        InnerStateTransition(new E84CardAGVErrorState(this));
                        E84BehaviorStateTransition(new E84CardAGVSingleReadyState(this));
                    }
                };

                Action changeRecoveryState = () =>
                {
                    if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                    {
                        InnerStateTransition(new E84FoupAutoRecoveryState(this));
                        E84BehaviorStateTransition(new E84AutoRecoveryState(this));
                    }
                    else if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                    {
                        // 상태 정의 제대로 안되어있으므로 ErroState로 보냄.
                        InnerStateTransition(new E84CardAGVErrorState(this));
                        E84BehaviorStateTransition(new E84CardAGVSingleReadyState(this));
                    }
                };

                E84EventCode eventCode = param.EventCode;

                //Error 처리.
                if (eventCode == E84EventCode.TP1_TIMEOUT ||
                    eventCode == E84EventCode.TP2_TIMEOUT ||
                    eventCode == E84EventCode.TP5_TIMEOUT ||
                    eventCode == E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE)
                {
                    retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    changeRecoveryState();
                }
                else if (eventCode == E84EventCode.TP3_TIMEOUT ||
                         eventCode == E84EventCode.TP4_TIMEOUT)
                {
                    retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    changeErrorState();
                }
                else if (eventCode == E84EventCode.LIGHT_CURTAIN_ERROR)
                {
                    bool isBusySignal = GetSignal(E84SignalTypeEnum.BUSY);

                    if (isBusySignal)
                    {
                        this.MetroDialogManager().ShowMessageDialog($"Recover LP by User from E84 page", $"Can not auto recovery because BUSY sensor is on", EnumMessageStyle.Affirmative);
                        changeErrorState();
                    }
                    else
                    {
                        if (InnerState.GetModuleState() == ModuleStateEnum.IDLE)
                        {
                            SetFoupBehaviorStateEnum();
                        }

                        changeRecoveryState();
                    }
                }
                else if (eventCode == E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS ||
                         eventCode == E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT ||
                         eventCode == E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS ||
                         eventCode == E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT)
                {
                    retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    changeErrorState();
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} AutoRecovery method is not defined.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public string E84Mode_ConvertToOriginTypeEvent(object input)
        {
            string retVal = input.ToString();

            try
            {
                int val = -1;
                bool intrst = int.TryParse(retVal, out val);

                if (intrst == true)
                {
                    if (val == 1)
                    {
                        retVal = E84Mode.AUTO.ToString();
                    }
                    else if (val == 0)
                    {
                        retVal = E84Mode.MANUAL.ToString();
                    }
                    else
                    {
                        retVal = val.ToString();//TODO: Convert Success/Fail 에 대한 결과 확인할 것. 
                    }

                    return retVal;
                }
                else if (input is bool)
                {
                    bool convert = bool.Parse(input.ToString());
                    if (convert == true)
                    {
                        retVal = E84Mode.AUTO.ToString();
                    }
                    else
                    {
                        retVal = E84Mode.MANUAL.ToString();
                    }

                    return retVal;
                }
                else
                {
                    retVal = val.ToString();//TODO: Convert Success/Fail 에 대한 결과 확인할 것. 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public string E84Mode_ConvertToReportTypeEvent(object input)
        {
            string retVal = input.ToString();

            try
            {
                if (input.ToString() == E84Mode.AUTO.ToString())
                {
                    retVal = "1";
                }
                else
                {
                    retVal = "0";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void AccessMode_ValueChangedEvent(Object oldValue, Object newValue, object valueChangedParam = null)
        {
            try
            {
                if (newValue.ToString() == E84Mode.AUTO.ToString())
                {
                    SetMode(1);
                }
                else
                {
                    SetMode(0);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ClearCurErrorParam()
        {
            try
            {
                _CurErrorParam = new E84ErrorParameter();

                LoggerManager.Debug($"Clear CurErrorParam. prev:{_CurErrorParam.ErrorAct}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void E84ErrorOccured(int errorCode = -1, E84EventCode code = E84EventCode.UNDEFINE)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized)
                {
                    if (errorCode == -1 && code == E84EventCode.UNDEFINE || errorCode == 0)
                    {
                        EventNumber = errorCode;
                        EventCode = code;

                        return;
                    }
                }
                else
                {
                    return;
                }

                var ccsuper = LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                Action<E84ErrorParameter> showMsgFunc = (param) =>
                {
                    string errormsg = $"E84 Error({param.ErrorNumber}) : [E84 {E84ModuleParaemter.E84OPModuleType}#{E84ModuleParaemter.FoupIndex}] {param.EventCode}, {param.CodeName}";

                    if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD && ccsuper?.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        if (ccsuper.GetRunningCCInfo().cardlpIndex == E84ModuleParaemter.FoupIndex)
                        {
                            errormsg += $"\n ** E84 Changed Pgv Status Abort. UsingCCInfo:{ccsuper.GetRunningCCInfo().cardreqmoduleType}.{E84ModuleParaemter.FoupIndex}";
                        }
                    }

                    this.MetroDialogManager().ShowMessageDialog(errormsg, $"{param.Discription}, action:{param.ErrorAct}", EnumMessageStyle.Affirmative);
                };

                Action errorstateFunc = () =>
                {
                    if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                    {
                        InnerStateTransition(new E84FoupErrorState(this));
                        E84BehaviorStateTransition(new E84SingleReadyState(this));
                    }
                    else if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                    {
                        if (ccsuper?.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                        {
                            if (ccsuper.GetRunningCCInfo().cardlpIndex == E84ModuleParaemter.FoupIndex)
                            {
                                this.E84Module().CommandManager().SetCommand<IAbortCardChangeSequence>(ccsuper);
                            }
                        }

                        InnerStateTransition(new E84CardAGVErrorState(this));
                        E84BehaviorStateTransition(new E84CardAGVSingleReadyState(this));
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} E84ErrorOccured(): E84OPMOduleType is undefinded");
                    }
                };

                int tmpErrorNumber = -1;
                E84EventCode tmpCode = E84EventCode.UNDEFINE;
                E84ErrorParameter tmpParam = null;

                bool isChanged = false;

                if (errorCode != -1 && errorCode != EventNumber)
                {
                    tmpErrorNumber = errorCode;
                    tmpParam = this.E84Module().E84SysParam.E84Errors.SingleOrDefault(errors => errors.ErrorNumber == tmpErrorNumber);

                    if (tmpParam != null)
                    {
                        tmpCode = tmpParam.EventCode;
                    }

                    isChanged = true;
                }
                else if (code != E84EventCode.UNDEFINE && code != EventCode)
                {
                    tmpCode = code;
                    tmpParam = this.E84Module().E84SysParam.E84Errors.SingleOrDefault(errors => errors.EventCode == tmpCode);

                    if (tmpParam != null)
                    {
                        tmpErrorNumber = tmpParam.ErrorNumber;
                    }

                    isChanged = true;
                }

                if (tmpParam == null)
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} E84ErrorOccured(): errorCode:{errorCode}, code:{code}");
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} E84ErrorOccured(): errorCode:{tmpErrorNumber}, code:{tmpCode}");
                }

                if (isChanged)
                {
                    if (errorCode == 0)
                    {
                        //nothing
                        EventNumber = errorCode;
                        EventCode = code;
                    }

                    if (tmpParam == null)
                    {
                        if (errorCode != 0)
                        {
                            // do action like ERROR_Ho_Off
                            this.MetroDialogManager().ShowMessageDialog($"E84 Error({code}) : {errorCode}", $"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} param is not defined. refer errorcode({errorCode}).", EnumMessageStyle.Affirmative);

                            errorstateFunc();

                            retVal = SetSignal(E84SignalTypeEnum.READY, false);
                            retVal = SetSignal(E84SignalTypeEnum.L_REQ, false);
                            retVal = SetSignal(E84SignalTypeEnum.U_REQ, false);
                            retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);

                            EventNumber = errorCode;
                            EventCode = code;
                        }
                    }
                    else
                    {
                        CurErrorParam = tmpParam;//ISSD - 2983

                        if (tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Warning)
                        {
                            if (GetSignal(E84SignalTypeEnum.READY) == false &&
                                GetSignal(E84SignalTypeEnum.CS_0) == false &&
                                GetSignal(E84SignalTypeEnum.VALID) == false &&
                                GetSignal(E84SignalTypeEnum.COMPT) == false)
                            {
                                // oht에서 모든 신호가 다 꺼졌다고 판단.
                                // E84Module에서 input 신호가 하나라도 true인게 있으면 clearevent 실패하고 70 Error 내보냄.
                                ClearEvent();
                            }
                            else
                            {
                                LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} E84ErrorOccured(): ErrorAct:{tmpParam.ErrorAct} do nothing, it will clearevent after all input signal off");
                            }
                        }

                        if (tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Req_Off ||
                            tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Ho_Off ||
                            tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Emergency)
                        {
                            showMsgFunc(tmpParam);
                            errorstateFunc();

                            retVal = SetSignal(E84SignalTypeEnum.READY, false);
                            retVal = SetSignal(E84SignalTypeEnum.L_REQ, false);
                            retVal = SetSignal(E84SignalTypeEnum.U_REQ, false);
                        }

                        if (tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Ho_Off ||
                            tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Emergency)
                        {
                            retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);

                            if (tmpParam.ErrorAct == E84ErrorActEnum.ERROR_Emergency)
                            {
                                retVal = SetSignal(E84SignalTypeEnum.ES, false);
                            }

                            if (tmpParam.AutoRecoveryEnable)
                            {
                                E84ErrorRecovery(tmpParam);
                            }
                        }

                        EventNumber = tmpErrorNumber;
                        EventCode = tmpCode;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void E84ModeChanged(int mode)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized)
                {
                    if (mode == 1)//auto mode - hoavblsignal check.
                    {
                        var foupstate = this.GEMModule().GetPIVContainer().GetFoupState(E84ModuleParaemter.FoupIndex);
                        if (foupstate == GEMFoupStateEnum.READY_TO_LOAD || foupstate == GEMFoupStateEnum.READY_TO_UNLOAD) // 무조건 HO를 켜면 안됨. ReadToLoad/ReadyUnload 상태일때만 HO를 켤 수 있음. 
                        {
                            if (GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                            {
                                retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, true, E84Mode.AUTO);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug("[E84] E84ModeChanged() : HO_AVBL on");
                                }
                                else
                                {
                                    LoggerManager.Debug($"[E84] E84ModeChanged() : PORT{E84ModuleParaemter.FoupIndex} Cannot on HO_AVBL");
                                }
                            }
                        }
                    }

                    //#Hynix_Merge: 검토 필요, 이부분 Controller에서 값 바뀌었을때 이벤트 보내야함. 지금 아래 동작 대로면 UI에서 Set할때만 업데이트함. 
                    if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                    {
                        if (mode == 0)
                        {
                            PIVInfo pivinfo = new PIVInfo(foupnumber: E84ModuleParaemter.FoupIndex);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CarrierAccessModeManualEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        else if (mode == 1)
                        {
                            PIVInfo pivinfo = new PIVInfo(foupnumber: E84ModuleParaemter.FoupIndex);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CarrierAccessModeOnlineEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                    }
                    else if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                    {
                        PIVInfo pivinfo = new PIVInfo(cardbufferindex: E84ModuleParaemter.FoupIndex);
                        if (mode == 0)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CardLPAccessModeManualEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        else if (mode == 1)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CardLPAccessModeOnlineEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                    }

                    this.E84Module().SetFoupCassetteLockOption();
                }
                else
                {
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CardPresenceStateChanged()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (InnerState != null)
                {
                    if (CardBufferModule?.CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH ||
                        CardBufferModule?.CardPRESENCEState == CardPRESENCEStateEnum.CARD_DETACH)
                    {
                        retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                    }
                    else
                    {
                        retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    }

                    bool isOnHandOffSignal = GetSignal(E84SignalTypeEnum.CS_0) || GetSignal(E84SignalTypeEnum.VALID);

                    if ((InnerState.GetModuleState() != ModuleStateEnum.RUNNING) && (isOnHandOffSignal == false))// Running이 아닐때는 항상 e84controller에 carrier를 실시간 업데이트 해준다. Running일때는 DetectedState에서 해줘야 Controller와 동기가맞음.
                    {
                        this.LoaderMaster.E84Module().GetE84Controller(E84ModuleParaemter.FoupIndex, E84OPModuleTypeEnum.CARD)?.SetCardStateInBuffer();
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} CardPresenceStateChanged ignore update. InnerState : {InnerState.GetModuleState()}, CS_0 : {GetSignal(E84SignalTypeEnum.CS_0)}, VALID : {GetSignal(E84SignalTypeEnum.VALID)}, GO : {GetSignal(E84SignalTypeEnum.GO)}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void FoupPresenceStateChanged(int foupIndex, bool presenceState, bool presenceStateChangedDone)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if(presenceStateChangedDone == true)
                {
                    bool signal_CS_0 = GetSignal(E84SignalTypeEnum.CS_0);
                    bool signal_VALID = GetSignal(E84SignalTypeEnum.VALID);
                    E84Mode e84_Mode = CommModule.RunMode;
                    if (e84_Mode == E84Mode.AUTO && signal_CS_0 == false && signal_VALID == false)
                    {
                        string msg = "";
                        if (FoupController?.FoupModuleInfo?.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_DETACH)
                        {
                            msg = "unloading";
                        }
                        else if (FoupController?.FoupModuleInfo?.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                        {
                            msg = "loading";
                        }

                        this.NotifyManager().Notify(EventCodeEnum.E84_LOAD_PORT_ACCESS_VIOLATION, $"[Load Port #{foupIndex}] Manual cassette {msg} detected while E84 mode is AUTO." +
                              $"\nFoupPRESENCEState = {FoupController.FoupModuleInfo.FoupPRESENCEState}" +
                              $"\nE84 RunMode = {e84_Mode}", foupIndex);

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(LoadPortAccessViolation).FullName, new ProbeEventArgs(this, semaphore, foupIndex));
                        semaphore.Wait();
                    }
                }

                if (IsFoupPresenceCarrier())
                {
                    if (GetSignal(E84SignalTypeEnum.BUSY) == false && E84BehaviorState.GetSubStateEnum() == E84SubStateEnum.READY)
                    {
                        if ((FoupController?.FoupModuleInfo?.FoupPRESENCEState ?? FoupPRESENCEStateEnum.ERROR) == FoupPRESENCEStateEnum.CST_DETACH)
                        {
                            if (GetSignal(E84SignalTypeEnum.CS_0) | GetSignal(E84SignalTypeEnum.VALID))
                            {
                                if (GetSignal(E84SignalTypeEnum.BUSY) == false)
                                {
                                    if (GetInputSignalComm(E84SignalTypeEnum.BUSY) == false)
                                    {
                                        E84ErrorOccured(code: E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (GetModuleStateEnum() != ModuleStateEnum.ERROR)
                    {
                        if (GetSignal(E84SignalTypeEnum.HO_AVBL) == false)
                        {
                            retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                        }
                    }
                }

                bool isOnHandOffSignal = GetSignal(E84SignalTypeEnum.CS_0) || GetSignal(E84SignalTypeEnum.VALID) || GetSignal(E84SignalTypeEnum.GO);

                if ((InnerState.GetModuleState() != ModuleStateEnum.RUNNING) && (isOnHandOffSignal == false))
                {
                    SetCarrierState();
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} FoupPresenceStateChanged ignore update. InnerState : {InnerState.GetModuleState()}, CS_0 : {GetSignal(E84SignalTypeEnum.CS_0)}, VALID : {GetSignal(E84SignalTypeEnum.VALID)}, GO : {GetSignal(E84SignalTypeEnum.GO)}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void FoupClampStateChanged(int foupIndex, bool clampState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (clampState == false)
                {
                    retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                    SetClampSignal(false);
                }
                else if (clampState == true)
                {
                    if (InnerState.GetModuleState() != ModuleStateEnum.RUNNING)
                    {
                        retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    }

                    SetClampSignal(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetSignal(E84SignalTypeEnum signal, bool flag, E84Mode setMode = E84Mode.UNDEFIND)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                int retval = -1;
                string checkFailReason = "";
                if (flag != GetSignal(signal))
                {
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
                                    if(CheckSetSignal(signal, flag, ref checkFailReason))
                                    {
                                        retval = CommModule?.SetHoAvblSignal(flag ? 1 : 0) ?? -1;
                                    }
                                }
                            }
                            else
                            {
                                if (CheckSetSignal(signal, flag, ref checkFailReason))
                                {
                                    retval = CommModule?.SetHoAvblSignal(flag ? 1 : 0) ?? -1;
                                }
                            }
                        }
                    }
                    else
                    {
                        int index = this.E84Module().SignalTypeConvertPin(signal);

                        retval = CommModule?.SetE84OutputSignal(index, flag ? 1 : 0) ?? -1;
                    }

                    if (retval == 0)
                    {
                        string signalVal = flag ? "ON" : "OFF";

                        LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} OUTPUT [{signal}] {signalVal}");

                        retVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private bool CheckSetSignal(E84SignalTypeEnum signal, bool flag, ref string checkFailReason)
        {
            bool retVal = false;
            try
            {
                switch (signal)
                {
                    case E84SignalTypeEnum.L_REQ:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.U_REQ:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.VA:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.READY:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.VS_0:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.VS_1:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.HO_AVBL:
                        {
                            if (flag)
                            {
                                // HO_AVBL 이 ON 인 경우
                                // Foup 이 UNLOAD 상태이고, Clamp 가 Unlock 상태여야 함.
                                if (FoupController != null && FoupController.FoupModuleInfo != null)
                                {
                                    FoupStateEnum foupState = FoupController.FoupModuleInfo.State;

                                    bool clampLockFlag;
                                    IFoupIOStates foupIOStates = this.FoupController.GetFoupIO();
                                    int ret = foupIOStates.ReadBit(foupIOStates.IOMap.Inputs.DI_CST_LOCK12s[E84ModuleParaemter.FoupIndex -1 ], out clampLockFlag);
                                    if (ret == 0)
                                    {
                                        string foupstateStr = "";
                                        if (this.E84Module().ValidationFoupUnloadedState(E84ModuleParaemter.FoupIndex, ref foupstateStr) == EventCodeEnum.NONE
                                                && (clampLockFlag == false) && CommModule.RunMode == E84Mode.AUTO)
                                        {
                                            retVal = true;
                                        }
                                        else
                                        {
                                            checkFailReason = $"{foupstateStr}, Clamp State : {clampLockFlag}, Run Mode : {CommModule.RunMode}";
                                        }
                                    }
                                    else
                                    {
                                        checkFailReason = $"[E84Controller] CheckSetSignal(). Foup Number: {E84ModuleParaemter.FoupIndex}, DI_CST_LOCK12s ReadBit fail :{ret}";
                                    }
                                }
                                else
                                {
                                    if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                                    {
                                        retVal = true;
                                        checkFailReason = string.Empty;
                                    }
                                    else
                                    {
                                        checkFailReason = $"Foup Controller is null";
                                    }
                                }
                            }
                            else
                            {
                                retVal = true;
                            }
                        }
                        break;
                    case E84SignalTypeEnum.ES:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.NC:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.SELECT:
                        retVal = true;
                        break;
                    case E84SignalTypeEnum.MODE:
                        retVal = true;
                        break;
                    default:
                        retVal = true;
                        break;
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
        public bool GetInputSignalComm(E84SignalTypeEnum signal)
        {
            bool retVal = false;

            try
            {
                if (CommModule != null)
                {
                    int sigVal = this.E84Module().SignalTypeConvertPin(signal);
                    int outputData;
                    int inputData;

                    int ret = CommModule.GetE84Signals(out inputData, out outputData);

                    if (ret != 0)
                    {
                        ret = CommModule.GetE84Signals(out inputData, out outputData);

                        if (ret != 0)
                        {
                            return retVal;
                        }
                    }

                    for (int bitIndex = 0; bitIndex < (int)E84MaxCount.E84_MAX_E84_INPUT; bitIndex++)
                    {
                        if (bitIndex == sigVal)
                        {
                            if (((inputData >> bitIndex) & 0x01) == 1)
                            {
                                retVal = true;

                            }
                            else
                            {
                                retVal = false;
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} GetCarrierState() - CommModule is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public bool GetCarrierState()
        {
            bool retVal = false;

            try
            {
                if (CommModule != null)
                {
                    if (CommModule.Carrier.Count > 0)
                    {
                        if (this.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                        {
                            retVal = CommModule.Carrier[0];
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} GetCarrierState() - CommModule is null");
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
                if (CommModule != null)
                {
                    if (CommModule.Clamp.Count > 0)
                    {
                        if (this.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                        {
                            retVal = CommModule.Clamp[0];
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} GetClampSignal() - CommModule is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} ResetE84Interface() - CommModule is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetLightCurtainSignal()
        {
            bool retVal = false;

            try
            {
                if (CommModule != null)
                {
                    retVal = CommModule.GetLightCurtainSignal();
                }
                else
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} GetLightCurtainSignal() - CommModule is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public E84SubSteps GetCommModuleSubStep()
        {
            E84SubSteps step = E84SubSteps.INVALID;

            try
            {
                step = CommModule?.CurrentSubStep ?? E84SubSteps.INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return step;
        }


        public EventCodeEnum SetCarrierStateInit(EventCodeEnum lastErrState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (FoupController != null)
                {
                    if (FoupController.FoupModuleInfo != null)
                    {
                        if (CommModule.Connection == E84ComStatus.CONNECTED)
                        {
                            if (FoupController.FoupModuleInfo.FoupPRESENCEState == ProberInterfaces.Foup.FoupPRESENCEStateEnum.CST_DETACH)
                            {
                                if (this.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                                {
                                    bool carrierExist = GetCarrierState();
                                    if (carrierExist == true)
                                    {
                                        if (IsFoupPresenceCarrier(false) == false)
                                        {
                                            CommModule.SetCarrierSignal(0, 0);

                                            retVal = EventCodeEnum.NONE;
                                        }
                                        else
                                        {
                                            if (lastErrState != EventCodeEnum.E84_CST_DETACH_STATE_ERROR)
                                                LoggerManager.Debug($"[E84] PORT{E84ModuleParaemter.FoupIndex} SetCarrierState error : CST PresenceState is CST_DETACH , CST Presence Check is CST_ATTACH");

                                            retVal = EventCodeEnum.E84_CST_DETACH_STATE_ERROR;
                                        }
                                    }
                                }
                            }
                            else if (FoupController.FoupModuleInfo.FoupPRESENCEState == ProberInterfaces.Foup.FoupPRESENCEStateEnum.CST_ATTACH)
                            {
                                if (this.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                                {
                                    bool carrierExist = GetCarrierState();
                                    if (carrierExist == false)
                                    {
                                        if (IsFoupPresenceCarrier(false) == true)
                                        {
                                            CommModule.SetCarrierSignal(0, 1);

                                            retVal = EventCodeEnum.NONE;
                                        }
                                        else
                                        {
                                            if (lastErrState != EventCodeEnum.E84_CST_ATTACH_STATE_ERROR)
                                                LoggerManager.Debug($"[E84] PORT{E84ModuleParaemter.FoupIndex} SetCarrierState error : CST PresenceState is CST_ATTACH , CST Presence Check is CST_DETACH");

                                            retVal = EventCodeEnum.E84_CST_ATTACH_STATE_ERROR;
                                        }
                                    }
                                }
                            }
                        }

                    }

                    if (E84BehaviorState.GetSubStateEnum() != E84SubStateEnum.TRANSFET_START && (CommModule.Connection == E84ComStatus.CONNECTED))
                    {
                        if (FoupController.GetFoupIO().IOMap.Inputs.DI_CST_LOCK12s[E84ModuleParaemter.FoupIndex - 1].Value)
                        {
                            if (GetClampSignal() == false)
                            {
                                SetClampSignal(true);
                            }
                            if (GetSignal(E84SignalTypeEnum.HO_AVBL) == true)
                            {
                                SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                            }
                        }
                        else
                        {
                            string foupstateStr = "";
                            if (this.E84Module().ValidationFoupUnloadedState(E84ModuleParaemter.FoupIndex, ref foupstateStr) == EventCodeEnum.NONE)
                            {
                                if (GetClampSignal() == true)
                                {
                                    SetClampSignal(false);
                                }
                                if (GetSignal(E84SignalTypeEnum.HO_AVBL) == false && CommModule.RunMode == E84Mode.AUTO)
                                {
                                    SetSignal(E84SignalTypeEnum.HO_AVBL, true);
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
            return retVal;
        }

        public EventCodeEnum SetCarrierState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (FoupController != null)
                {
                    if (FoupController.FoupModuleInfo != null)
                    {
                        if (CommModule.Connection == E84ComStatus.CONNECTED)
                        {
                            if (this.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                            {
                                bool carrierExist = GetCarrierState();

                                if (FoupController.FoupModuleInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_DETACH && carrierExist)
                                {
                                    CommModule.SetCarrierSignal(0, 0);

                                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetCarrier State is not exist");
                                }
                                else if (FoupController.FoupModuleInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH && !carrierExist)
                                {
                                    CommModule.SetCarrierSignal(0, 1);

                                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetCarrier State is exist");
                                }
                            }
                        }
                    }

                    if (E84BehaviorState.GetSubStateEnum() != E84SubStateEnum.TRANSFET_START && CommModule.Connection == E84ComStatus.CONNECTED)
                    {
                        IFoupIOStates foupIOStates = this.FoupController.GetFoupIO();
                        bool value;

                        int ret = foupIOStates.ReadBit(foupIOStates.IOMap.Inputs.DI_CST_LOCK12s[E84ModuleParaemter.FoupIndex - 1], out value);

                        if (ret == 0)
                        {
                            bool signal = GetClampSignal();

                            if (value != signal)
                            {
                                SetClampSignal(!signal);

                                retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, signal);
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], SetCarrierState(), ret = {ret}");
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
        public void SetFoupBehaviorStateEnum(bool init = false)
        {
            try
            {
                var prev = FoupBehaviorStateEnum.ToString();

                if (init == true)
                {
                    FoupBehaviorStateEnum = FoupStateEnum.UNDEFIND;

                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetFoupBehaviorStateEnum : {FoupBehaviorStateEnum}, PreFoupBehaviorStateEnum : {prev} ");

                    return;
                }

                #region <Summary> Check Req Signal
                if (GetSignal(E84SignalTypeEnum.L_REQ))
                {
                    FoupBehaviorStateEnum = FoupStateEnum.LOAD;

                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetFoupBehaviorStateEnum : {prev} => {FoupBehaviorStateEnum} ");
                }
                else if (GetSignal(E84SignalTypeEnum.U_REQ))
                {
                    FoupBehaviorStateEnum = FoupStateEnum.UNLOAD;

                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetFoupBehaviorStateEnum : {prev} => {FoupBehaviorStateEnum} ");
                }

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum SetCardStateInBuffer()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IOManager != null)
                {
                    if (CommModule.Connection == E84ComStatus.CONNECTED)
                    {
                        if (CardBufferModule != null)
                        {
                            if (CardBufferModule.CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH)
                            {
                                int cardExist = 0;

                                CommModule.GetCarrierSignal(0, out cardExist);

                                if (cardExist == 0)
                                {
                                    CommModule.SetCarrierSignal(0, 1);
                                    LoggerManager.Debug($"[{this.GetType().Name}], SetCardStateInBuffer() : Card SetCarrier State is exist");
                                }
                            }
                            else
                            {
                                int cardExist = 0;

                                CommModule.GetCarrierSignal(0, out cardExist);

                                if (cardExist == 1)
                                {
                                    CommModule.SetCarrierSignal(0, 0);
                                    LoggerManager.Debug($"[{this.GetType().Name}], SetCardStateInBuffer() : Card SetCarrier State is not exist");
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

            return retVal;
        }
        public void SetCardBehaviorStateEnum()
        {
            try
            {
                if (IOManager != null)
                {
                    if (CardBufferModule != null)
                    {
                        var prev = CardBehaviorStateEnum.ToString();

                        if (CardBufferModule.CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH)
                        {
                            CardBehaviorStateEnum = CardBufferOPEnum.UNLOAD;
                        }
                        else if (CardBufferModule.CardPRESENCEState == CardPRESENCEStateEnum.CARD_DETACH)
                        {
                            CardBehaviorStateEnum = CardBufferOPEnum.LOAD;
                        }
                        else
                        {
                            LoggerManager.Error($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetCardBehaviorStateEnum(): Error {CardBehaviorStateEnum}, CardPRESENCEState:{CardBufferModule.CardPRESENCEState}");
                        }

                        LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetCardBehaviorStateEnum : {prev} => {CardBehaviorStateEnum} ");
                    }
                }
                else if (GetSignal(E84SignalTypeEnum.U_REQ))
                {
                    FoupBehaviorStateEnum = FoupStateEnum.UNLOAD;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetFoupBehaviorState(FoupStateEnum statenum)
        {
            FoupBehaviorStateEnum = statenum;
        }
        public int SetMode(int mode)
        {
            int retVal = -1;

            try
            {
                /// 0 : Manual 
                /// 1 : Auto

                if (InnerState.GetModuleState() == ModuleStateEnum.ERROR)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message", "E84 is in the error state so cannot be changed the mode.\r Please reset the status through the [Clear sequence] or [Initialize] menu and try again.", EnumMessageStyle.Affirmative);

                    return retVal;
                }

                if (InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if (mode == 1)
                    {
                        retVal = CommModule.SetMode(mode);
                    }
                    else
                    {
                        if (GetSignal(E84SignalTypeEnum.CS_0) || GetSignal(E84SignalTypeEnum.VALID))
                        {
                            var param = this.E84Module().E84SysParam.E84Errors.SingleOrDefault(errors => errors.EventCode == E84EventCode.CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR);

                            if (param != null)
                            {
                                this.MetroDialogManager().ShowMessageDialog($"E84 Error : {param.CodeName}", $"{param.Discription}", EnumMessageStyle.Affirmative);
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog($"E84 Error : CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR", $"Can not change to Manual Mode", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }
                else
                {
                    retVal = CommModule.SetMode(mode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public int GetCCTargetStageIndex()
        {
            int stgnum = 0;

            try
            {
                var ccsuper = LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                if (ccsuper != null)
                {
                    stgnum = ccsuper.GetRunningCCInfo().cardreqmoduleIndex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return stgnum;
        }
        public void UpdateCardBufferState(bool forced_event = false)
        {
            CardBufferModule.UpdateCardBufferState(forced_event);
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
                {
                    retVal = EventCodeEnum.NONE;
                }
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

                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} SetClampSignal => FoupIndex : {E84ModuleParaemter.FoupIndex}, Signal :{onflag}");
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public E84EventCode GetEventCode()
        {
            try
            {
                var param = this.E84Module().E84SysParam.E84Errors.SingleOrDefault(errors => errors.ErrorNumber == EventNumber);

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
                var param = this.E84Module().E84SysParam.E84Errors.SingleOrDefault(errors => errors.EventCode == code);

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
                    var ret = SetSignal(E84SignalTypeEnum.HO_AVBL, true);
                }

                EventNumber = CommModule.GetEventNumber();
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

        public bool Read_DI_CST_Exist()
        {
            bool retval = false;

            try
            {
                IFoupIOStates foupIOStates = this.FoupController.GetFoupIO();
                int fouparrayindex = E84ModuleParaemter.FoupIndex - 1;

                if (foupIOStates.IOMap.Inputs.DI_CST_Exists != null && foupIOStates.IOMap.Inputs.DI_CST_Exists.Count > fouparrayindex)
                {
                    if (foupIOStates.IOMap.Inputs.DI_CST_Exists[fouparrayindex].IOOveride.Value == EnumIOOverride.NONE)
                    {
                        bool value;

                        int ret = foupIOStates.ReadBit(foupIOStates.IOMap.Inputs.DI_CST_Exists[fouparrayindex], out value);

                        if (ret == 0)
                        {
                            retval = value;
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], IsFoupPresenceCarrier(), Module type = {E84ModuleParaemter.E84OPModuleType}, Port = {E84ModuleParaemter.FoupIndex}, ret = {ret}");
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


        public bool IsFoupPresenceCarrier(bool write_log = true)
        {
            bool retVal = false;

            try
            {
                int fouparrayindex = E84ModuleParaemter.FoupIndex - 1;

                IFoupIOStates foupIOStates = this.FoupController.GetFoupIO();

                bool presenceSensor = false;
                bool existSensor = false;

                bool value, value2;

                int ret = foupIOStates.ReadBit(foupIOStates.IOMap.Inputs.DI_CST12_PRESs[fouparrayindex], out value);
                int ret1 = foupIOStates.ReadBit(foupIOStates.IOMap.Inputs.DI_CST12_PRES2s[fouparrayindex], out value2);

                if (ret == 0 && ret1 == 0)
                {
                    if (value && value2)
                    {
                        presenceSensor = true;
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], IsFoupPresenceCarrier(), Module type = {E84ModuleParaemter.E84OPModuleType}, Port = {E84ModuleParaemter.FoupIndex}, ret = {ret}, ret1 = {ret1}");
                }

                existSensor = Read_DI_CST_Exist();
                if (write_log)
                {
                    LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} IsPresenceCarrier() => PresenceSensor : { presenceSensor}, ExistSensor : {existSensor}");
                }

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (moduleparam.E84_Attatched == true)
                {
                    if (moduleparam.E84ModuleType == E84CommModuleTypeEnum.EMOTIONTEK)
                    {
                        _E84State = new E84FoupIdleState(this);
                        E84BehaviorStateTransition(new E84SingleReadyState(this));

                        EMotionTekE84CommModule e84module = null;

                        if (CommModule is EMotionTekE84CommModule)
                        {
                            e84module = (EMotionTekE84CommModule)CommModule;
                        }
                        else
                        {
                            CommModule = new EMotionTekE84CommModule();
                        }

                        e84module.E84ErrorOccured += E84ErrorOccured;

                        e84module.SetParameter(moduleparam, e84PinSignal);
                        retVal = e84module.InitModule();

                        // E84 Controller Connect Success
                        if (retVal == EventCodeEnum.NONE)
                        {
                            bool runmode = false;

                            if (CommModule.RunMode == E84Mode.MANUAL)
                            {
                                runmode = false;
                            }
                            else if (CommModule.RunMode == E84Mode.AUTO)
                            {
                                runmode = true;
                            }

                            if (moduleparam.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                            {
                                SetFoupController();

                                this.GEMModule().GetPIVContainer().SetFoupAccessMode(E84ModuleParaemter.FoupIndex, runmode);

                            }
                            else if (moduleparam.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                            {
                                CommModule.SetClampSignal(0, 0);// Card는 Clamp 가 없기 때문에 무조건 Unlclamp 상태로 만들어줌. Clamp신호를 제어하는 부분이 없어서 여기에서 한번만 Clear해줌. 
                                LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                                var LoaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();

                                CardBufferModule = LoaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, moduleparam.FoupIndex);
                                CardBufferModule.E84PresenceStateChangedEvent += CardPresenceStateChanged;
                                CardBufferModule.CardBufferStateUpdateEvent += UpdateCardBufferState;// 추후 Operation화면에 캐리어 반송을 위한 강제 이벤트 호출용으로 사용.
                                CardBufferModule.UpdateCardBufferState();

                                _E84State = new E84CardAGVIdleState(this);

                                E84BehaviorStateTransition(new E84CardAGVSingleReadyState(this));
                                IOManager = this.GetLoaderContainer()?.Resolve<IIOManagerProxy>();

                                this.GEMModule().GetPIVContainer().SetCardBufferAccessMode(moduleparam.FoupIndex, runmode);
                                this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(moduleparam.FoupIndex);
                            }

                            if (eventCode != 0)
                            {
                                if (GetSignal(E84SignalTypeEnum.HO_AVBL) == true)
                                {
                                    retVal = SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                                }
                            }

                            isExcuteThread = true;
                            thread = new Thread(new ThreadStart(ExecuteRun));
                            thread.Start();
                        }
                        else// E84 Controller Connect fail
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}] : ConnectCommodule(), ComModule Init fail. Return Value is {retVal}");
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
        public bool IsCardPresenceInBuffer()
        {
            bool retVal = false;

            try
            {
                if (IOManager != null)
                {
                    if (CardBufferModule != null)
                    {
                        if (CardBufferModule.CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
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
        public bool CanPutCardBuffer()
        {
            bool retVal = false;
            try
            {
                if (IOManager != null)
                {
                    if (CardBufferModule != null)
                    {
                        if (CardBufferModule.CardPRESENCEState == CardPRESENCEStateEnum.CARD_DETACH)
                        {
                            retVal = true;
                        }
                        else// ATTACH, CARRIER, EMPTY
                        {
                            retVal = false;
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
        public EventCodeEnum DisConnectCommodule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                CommModule.StopCommunication();//Stop Commmodule Thread

                CommModule.E84ErrorOccured -= E84ErrorOccured;

                if (FoupController != null)
                {
                    // Controller Event Unregistration
                    FoupController.GetFoupService().FoupModule.PresenceStateChangedEvent -= FoupPresenceStateChanged;
                    FoupController.GetFoupService().FoupModule.FoupClampStateChangedEvent -= FoupClampStateChanged;
                }

                if (CardBufferModule != null)
                {
                    CardBufferModule.E84PresenceStateChangedEvent -= CardPresenceStateChanged;
                    CardBufferModule.CardBufferStateUpdateEvent -= UpdateCardBufferState;
                }

                //Stop controller Thread
                isExcuteThread = false;

                if (thread != null)
                {
                    thread.Join(5000);
                }

                thread = null;

                LoggerManager.Debug($"DisConnectCommodule() in {this.GetType().Name}, Thread = {thread}, isExcuteThread = {isExcuteThread}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
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

                if (GetSignal(E84SignalTypeEnum.L_REQ))
                {
                    retVal = SetSignal(E84SignalTypeEnum.L_REQ, false);
                }

                if (GetSignal(E84SignalTypeEnum.READY))
                {
                    retVal = SetSignal(E84SignalTypeEnum.READY, false);
                }

                if (GetSignal(E84SignalTypeEnum.U_REQ))
                {
                    retVal = SetSignal(E84SignalTypeEnum.U_REQ, false);
                }

                if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                {
                    if (InnerState.GetModuleState() != ModuleStateEnum.IDLE)
                    {
                        InnerStateTransition(new E84FoupIdleState(this));
                    }

                    E84BehaviorStateTransition(new E84SingleReadyState(this));
                }
                else if (E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.CARD)
                {
                    if (InnerState.GetModuleState() != ModuleStateEnum.IDLE)
                    {
                        InnerStateTransition(new E84CardAGVIdleState(this));
                    }
                    E84BehaviorStateTransition(new E84CardAGVSingleReadyState(this));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = InnerState;
                InnerState = state;
                ModulestateEnum = InnerState.GetModuleState();

                LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} Controller State Transition. Pre : {PreInnerState}, Curr : {InnerState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void E84BehaviorStateTransition(IE84BehaviorState state)
        {
            try
            {
                LoggerManager.Debug($"[E84] {E84ModuleParaemter.E84OPModuleType}.PORT{E84ModuleParaemter.FoupIndex} Behavior State Transition. Pre : {E84BehaviorState}, Curr : {state}");
                E84BehaviorState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void TempSetE84ModeStatus(bool entry)
        {
            try
            {
                var prevmode = CommModule.RunMode;

                LoggerManager.Debug($"FoupIndex = {E84ModuleParaemter.FoupIndex}, E84Mode = {prevmode}");

                if (entry == true)
                {
                    if (prevmode == E84Mode.AUTO)
                    {
                        CommModule.SetMode(0); //manual mode 변경
                        IsChangedE84Mode = true;

                        LoggerManager.Debug($"Entry Foup Recovery Page: FoupIndex = {E84ModuleParaemter.FoupIndex}, E84Mode is changed to MANUAL");
                    }
                    else if (prevmode == E84Mode.MANUAL)
                    {
                        IsChangedE84Mode = false;
                    }
                    else
                    {
                        //UNDEFINED
                        IsChangedE84Mode = false;
                    }
                }
                else
                {
                    CommModule.SetMode(1); //auto mode로 변경
                    IsChangedE84Mode = false;

                    LoggerManager.Debug($"Exit Foup Recovery Page: FoupIndex = {E84ModuleParaemter.FoupIndex}, E84Mode is changed to Auto");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
