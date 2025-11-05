using Command.TCPIP;
using EventProcessModule.TCPIP;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Communication.Tester;
using ProberInterfaces.Enum;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using ProberInterfaces.State;
using RequestCore.Query;
using RequestCore.Query.TCPIP;
using RequestCore.QueryPack;
using RequestInterface;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using TCPIPParamObject;
using TCPIPParamObject.System;
using TesterDriverModule;

namespace TCPIP
{
    public class TCPIPModule : SequenceServiceBase, ITCPIP, INotifyPropertyChanged, IHasCommandRecipe, IProbeEventSubscriber
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        internal BinAnalyzerManager.BinAnalyzerManager BinAnalyzerManager = new BinAnalyzerManager.BinAnalyzerManager();

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;

        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.TCPIP);
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

        private TCPIPRequestParam _TCPIPRequestParam;
        public TCPIPRequestParam TCPIPRequestParam
        {
            get { return _TCPIPRequestParam; }
            set
            {
                if (value != _TCPIPRequestParam)
                {
                    _TCPIPRequestParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TCPIPCommandRecipe _TCPIPCommandRecipe;
        public TCPIPCommandRecipe TCPIPCommandRecipe
        {
            get { return _TCPIPCommandRecipe; }
            set
            {
                if (value != _TCPIPCommandRecipe)
                {
                    _TCPIPCommandRecipe = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CardchangeTempInfo _CardchangeTempInfo;
        public CardchangeTempInfo CardchangeTempInfo
        {
            get { return _CardchangeTempInfo; }
            set
            {
                if (value != _CardchangeTempInfo)
                {
                    _CardchangeTempInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<CommunicationRequestSet> RequestSetList
        {
            get { return TCPIPRequestParam.RequestSetList; }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                _TransitionInfo = value;
            }
        }

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        #region CommandSlot

        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }


        private CommandTokenSet _RunTokenSet = new CommandTokenSet();
        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set
            {
                _RunTokenSet = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        private IParam _TCPIPSysParam_IParam;
        public IParam TCPIPSysParam_IParam
        {
            get { return _TCPIPSysParam_IParam; }
            set
            {
                if (value != _TCPIPSysParam_IParam)
                {
                    _TCPIPSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TCPIPSysParam _TCPIPSysParam;
        public TCPIPSysParam TCPIPSysParam
        {
            get { return _TCPIPSysParam; }
            set
            {
                if (value != _TCPIPSysParam)
                {
                    _TCPIPSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TCPIPDataIDConverterParam _TCPIPDataIDConverterParam;
        public TCPIPDataIDConverterParam TCPIPDataIDConverterParam
        {
            get { return _TCPIPDataIDConverterParam; }
            set
            {
                if (value != _TCPIPDataIDConverterParam)
                {
                    _TCPIPDataIDConverterParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumCommunicationState _CommunicationState;
        public EnumCommunicationState CommunicationState
        {
            get { return _CommunicationState; }
            set
            {
                _CommunicationState = value;
                RaisePropertyChanged();
            }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                _ModuleState = value;
                RaisePropertyChanged();
            }
        }

        private TCPIPState _TCPIPModuleState;
        public TCPIPState TCPIPModuleState
        {
            get { return _TCPIPModuleState; }
        }

        public IInnerState PreInnerState { get; set; }

        public IInnerState InnerState
        {
            get { return _TCPIPModuleState; }
            set
            {
                if (value != _TCPIPModuleState)
                {
                    _TCPIPModuleState = value as TCPIPState;
                }
            }
        }

        private ITesterComDriver _TesterComDriver = null;
        public ITesterComDriver TesterComDriver
        {
            get { return _TesterComDriver; }
            set
            {
                if (value != _TesterComDriver)
                {
                    _TesterComDriver = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _DriverState = 0;
        public int DriverState
        {
            get { return _DriverState; }
            set
            {
                if (_DriverState != value)
                {
                    _DriverState = value;
                }
            }
        }

        public EventProcessList SubscribeRecipeParam { get; set; }

        //public ObservableCollection<string> CommandCollection { get; set; }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool retVal = false;
            msg = string.Empty;

            try
            {
                if (GetTCPIPOnOff() == EnumTCPIPEnable.ENABLE)
                {
                    retVal = this.ModuleState.State != ModuleStateEnum.ERROR;
                    LoggerManager.Debug($"[TCPIPModule], ERROR state");
                    msg = "TCPIPModule state is ERROR";
                }
                else
                {
                    retVal = true;
                    msg = "";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
   
        public EventCodeEnum ParamValidation()
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

        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = TCPIPModuleState.Connect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = TCPIPModuleState.DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                _TCPIPModuleState = new TCPIPNotConnectedState(this);
                StateTransition(new ModuleInitState(this));

                retval = BinAnalyzerManager.LoadDevParameter();
                retval = BinAnalyzerManager.InitModule();
                BinAnalyzerManager.SetBinAnalyzer(TCPIPSysParam.EnumBinType.Value);

                //if (this.TCPIPSysParam.EnumTCPIPOnOff.Value == EnumTCPIPEnable.ENABLE)
                //{
                //    retval = this.TesterCommunicationManager().CreateTesterComInstance(EnumTesterComType.TCPIP);
                //}

                ConnectValueChangedEventHandler();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Init();

                    if (this.TCPIPSysParam.InitializeConnect.Value == true)
                    {
                        retval = Connect();
                    }

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

        public void ConnectValueChangedEventHandler()
        {
            try
            {
                this.TCPIPSysParam.EnumBinType.ValueChangedEvent -= EnumBinType_ValueChangedEvent;
                this.TCPIPSysParam.EnumBinType.ValueChangedEvent += EnumBinType_ValueChangedEvent;

                this.TCPIPSysParam.EnumTCPIPComType.ValueChangedEvent -= EnumTCPIPComType_ValueChangedEvent;
                this.TCPIPSysParam.EnumTCPIPComType.ValueChangedEvent += EnumTCPIPComType_ValueChangedEvent;

                this.TCPIPSysParam.EnumTCPIPOnOff.ValueChangedEvent -= EnumTCPIPComType_ValueChangedEvent;
                this.TCPIPSysParam.EnumTCPIPOnOff.ValueChangedEvent += EnumTCPIPComType_ValueChangedEvent;

                ChangedBinTypeValue(this.TCPIPSysParam.EnumBinType.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Event
        private void EnumBinType_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                BinType bintype = (BinType)newValue;

                ChangedBinTypeValue(bintype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EnumTCPIPComType_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                CreateTesterComDriver();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        
        #endregion

        private void ChangedBinTypeValue(BinType bintype)
        {
            try
            {
                if (BinAnalyzerManager != null)
                {
                    BinAnalyzerManager.SetBinAnalyzer(TCPIPSysParam.EnumBinType.Value);
                }
                else
                {
                    LoggerManager.Debug($"[TCPIPModule], ChangedBinTypeValue() : BinAnalyzerManager is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum retval = ModuleStateEnum.ERROR;

            try
            {
                retval = Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;

            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();
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
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            ModuleStateEnum retval = ModuleStateEnum.UNDEFINED;

            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retval = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            ModuleStateEnum retval = ModuleStateEnum.UNDEFINED;

            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retval = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            ModuleStateEnum retval = ModuleStateEnum.UNDEFINED;

            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retval = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public ModuleStateEnum Abort()
        {
            ModuleStateEnum retval = ModuleStateEnum.UNDEFINED;

            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retval = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        internal EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = InnerState;
                InnerState = state;

                LoggerManager.Debug($"{this.GetType()}.InnerStateTransition() : STATE={InnerState.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }

            return retVal;
        }


        public EventCodeEnum SetCommandRecipe()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ICommandManager CommandManager = this.CommandManager();

                CommandManager.AddCommandParameters(TCPIPCommandRecipe.Descriptors);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RegistEventSubscribe()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IEventManager EventManager = this.EventManager();

                if (0 < (this.SubscribeRecipeParam?.Count ?? 0))
                {
                    foreach (var evtname in this.SubscribeRecipeParam)
                    {
                        retval = EventManager.RemoveEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[TCPIPModule] Remove EventSubscribe Error...");

                            break;
                        }
                    }
                    this.SubscribeRecipeParam.Clear();
                }

                retval = this.LoadTCPIPSubscribeRecipe();

                foreach (var evtname in this.SubscribeRecipeParam)
                {
                    retval = EventManager.RegisterEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[TCPIP] Regist EventSubscribe Error...");

                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeCataniaRecipe()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                TCPIPEventParam eventParam = null;
                TCPIPQueryData _TCPIPQueryData = null;
                QueryHasAffix queryHasAffix = null;

                #region ProberStart
                
                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberConnectionStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProberStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    //EventFullName = typeof(TCPIPConnectionStartEvent).FullName,
                    EventFullName = typeof(TesterConnectedEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region Allocated

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new FoupAllocated();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "Allocated";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";
                queryHasAffix.Data = new GetPortNumberByFoupAllocatedInfo();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";
                queryHasAffix.Data = new GetDeviceNameByFoupAllocatedInfo();
                
                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";
                queryHasAffix.Data = new GetLotNameByFoupAllocatedInfo();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(FoupAllocatedEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region ProbeCardidReadDone

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberCardIDReadDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProbeCardidReadDone";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;
                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#Stage";
                queryHasAffix.Data = new StageNumber();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";
                queryHasAffix.Data = new GetCardIDByProbeCardInfo();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);


                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(CardChangeVaildationStartEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region LotStart

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new LotStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "LotStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                // Postfix = "#P"

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";
                queryHasAffix.Data = new PortNumber();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(LotStartEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region WaferStart
                
                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ChipStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "WaferStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                //_TCPIPQueryData.ACK = "WaferStart";
                //_TCPIPQueryData.NACK = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                // Postfix = "#P"
                // Postfix = "#S"

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";
                queryHasAffix.Data = new PortNumber();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#S";
                queryHasAffix.Data = new CurrentSlotNumber();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    //EventFullName = typeof(GoToStartDieEvent).FullName,
                    EventFullName = typeof(WaferStartEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region PMIStart

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new Command.TCPIP.PMIStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "PMIStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(PMIStartEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region PMIEnd

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new Command.TCPIP.PMIEnd();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "PMIEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsPMIPass(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "P" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsPMIFail(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "F" },
                        Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                    }
                };

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(PMIEndEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region MoveEnd - MoveToNextIndexDone

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new MoveToNextIndexDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "MoveEnd";
                _TCPIPQueryData.ACK.postfix = "#WaferEnd";

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(DontRemainSequenceEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region MoveEnd - ZupDone

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ZupDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "MoveEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "MoveEnd";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsZUP(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "Zup" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsZDN(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "Zdn" },
                        Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                    }
                };

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#X";

                queryHasAffix.Data = new CurrentXCoordinate();

                queryHasAffix.DataFormat = "{0:+0000;-0000;0000}";

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "Y";
                queryHasAffix.Data = new CurrentYCoordinate();
                queryHasAffix.DataFormat = "{0:+0000;-0000;0000}";

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProbingZUpProcessEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region MoveEnd - ZdownDone

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ZdownDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "MoveEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "MoveEnd";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsZDN(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "Zdn" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsZUP(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "Zup" },
                        Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                    }
                };

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#X";

                queryHasAffix.Data = new CurrentXCoordinate();

                queryHasAffix.DataFormat = "{0:+0000;-0000;0000}";

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "Y";
                queryHasAffix.Data = new CurrentYCoordinate();
                queryHasAffix.DataFormat = "{0:+0000;-0000;0000}";

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProbingZDownProcessEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region LotEnd

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new LotEndDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "LotEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";
                queryHasAffix.Data = new PortNumber();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(StageDeallocatedEvent).FullName,
                    Parameter = eventParam
                }
                );
                #endregion

                #region LotSwitched

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new LotStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "LotStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";
                queryHasAffix.Data = new PortNumber();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(LotSwitchedEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region ProberError

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberErrorOccurred();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProberError";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "ProberError";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProberErrorEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region DW

                // DW
                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new DWPassDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "DWEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(PassDWCommandEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region ProberStatus

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberStatus();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProberStatus";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "ProberStatus";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsLotIdleState(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "I" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsLotRunningState(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "R" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotPausedState(),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "P" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new IsLotErrorState(),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "E" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "" },
                            }
                        }
                    }
                };

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProberStatusEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeCataniaDummyRecipe()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                TCPIPEventParam eventParam = null;
                TCPIPQueryData _TCPIPQueryData = null;
                QueryHasAffix queryHasAffix = null;

                #region ProberStart

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberConnectionStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProberStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    //EventFullName = typeof(TCPIPConnectionStartEvent).FullName,
                    EventFullName = typeof(TesterConnectedEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region LotStart

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new LotStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "LotStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                // Postfix = "#P"

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";

                // REAL
                //queryHasAffix.Data = new PortNumber();
                queryHasAffix.Data = new TCPIPDummyQueryData();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(LotStartEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region WaferStart

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ChipStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "WaferStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                // Postfix = "#P"
                // Postfix = "#S"

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";

                // REAL
                //queryHasAffix.Data = new PortNumber();
                queryHasAffix.Data = new TCPIPDummyQueryData();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#S";

                // REAL
                queryHasAffix.Data = new CurrentSlotNumber();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(GoToStartDieEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region PMIStart

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new Command.TCPIP.PMIStart();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "PMIStart";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(PMIStartEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region PMIEnd

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new Command.TCPIP.PMIEnd();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "PMIEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsPMIPass(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "P" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsPMIFail(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "F" },
                        Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                    }
                };

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(PMIEndEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region MoveEnd - MoveToNextIndexDone

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new MoveToNextIndexDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "MoveEnd";
                _TCPIPQueryData.ACK.postfix = "#WaferEnd";

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(DontRemainSequenceEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region MoveEnd - ZupDone

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ZupDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "MoveEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "MoveEnd";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                // REAL
                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsZUP(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "Zup" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsZDN(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "Zdn" },
                        Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                    }
                };
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#X";

                // REAL
                queryHasAffix.Data = new CurrentXCoordinate();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                queryHasAffix.DataFormat = "{0:+0000;-0000;0000}";

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "Y";

                // REAL
                queryHasAffix.Data = new CurrentYCoordinate();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                queryHasAffix.DataFormat = "{0:+0000;-0000;0000}";

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProbingZUpProcessEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region LotEnd

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new LotEndDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "LotEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#P";

                // REAL
                //queryHasAffix.Data = new PortNumber();
                queryHasAffix.Data = new TCPIPDummyQueryData();

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(LotEndEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                #region ProberStatus

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberStatus();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProberStatus";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "ProberStatus";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "#";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsLotIdleState(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "I" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsLotRunningState(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "R" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotPausedState(),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "P" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new IsLotErrorState(),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "E" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "" },
                            }
                        }
                    }
                };

                _TCPIPQueryData.ACKExtensionQueries.Querys.Add(queryHasAffix);

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProberStatusEvent).FullName,
                    Parameter = eventParam
                }
                );

                #endregion

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                (eventParam.Response as TCPIPDummyActionData).EventFullName = typeof(ProbingZUpProcessEvent).FullName;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_ZupDone).FullName,
                    Parameter = eventParam
                }
                );

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_StopAndAlarmDone).FullName,
                    Parameter = eventParam
                }
                );

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_MoveToNextIndexDone).FullName,
                    Parameter = eventParam
                }
                );

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_UnloadWaferDone).FullName,
                    Parameter = eventParam
                }
                );

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_BINCodeDone).FullName,
                    Parameter = eventParam
                }
                );

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_HardwareAssemblyVerifyDone).FullName,
                    Parameter = eventParam
                }
                );

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                eventParam.Response = new TCPIPDummyActionData();
                (eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(Response_LotProcessingVerifyDone).FullName,
                    Parameter = eventParam
                }
                );

                //eventParam = new TCPIPEventParam();
                //eventParam.CommandName = typeof(ITCPIP_ACTION).ToString();
                //eventParam.Response = new TCPIPDummyActionData();
                //(eventParam.Response as TCPIPQueryData).CommandType = EnumTCPIPCommandType.ACTION;

                //SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                //{
                //    EventFullName = typeof(Response_DWDone).FullName,
                //    Parameter = eventParam
                //}
                //);

                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new ProberErrorOccurred();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "ProberError";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = "ProberError";
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(ProberErrorEvent).FullName,
                    Parameter = eventParam
                }
                );

                // DW
                eventParam = new TCPIPEventParam();
                eventParam.CommandName = typeof(ITCPIP_RESPONSE).ToString();
                eventParam.Response = new DWPassDone();
                _TCPIPQueryData = eventParam.Response as TCPIPQueryData;

                _TCPIPQueryData.ACK.prefix = string.Empty;
                _TCPIPQueryData.ACK.data = "DWEnd";
                _TCPIPQueryData.ACK.postfix = string.Empty;

                _TCPIPQueryData.NACK.prefix = string.Empty;
                _TCPIPQueryData.NACK.data = string.Empty;
                _TCPIPQueryData.NACK.postfix = string.Empty;

                _TCPIPQueryData.CommandType = EnumTCPIPCommandType.INTERRUPT;

                SubscribeRecipeParam.Add(new TCPIPEventProc_CmdSetter()
                {
                    EventFullName = typeof(PassDWCommandEvent).FullName,
                    Parameter = eventParam
                }
                );
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum LoadTCPIPSubscribeRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            SubscribeRecipeParam = new EventProcessList();

            string recipeFileName = $"Recipe_Subscribe_TCPIP_{TCPIPSysParam.EnumTCPIPComType.ToString()}.json";
            string FullPath = this.FileManager().GetSystemParamFullPath("Event", recipeFileName);

            try
            {
                if (File.Exists(FullPath) == false)
                {
                    MakeCataniaRecipe();
                    //MakeCataniaDummyRecipe();

                    RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[TCPIPModule] LoadTCPIPSubscribeRecipe(): Serialize Error");
                        return RetVal;
                    }
                }

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(EventProcessList), null, FullPath);

                if (RetVal != EventCodeEnum.NONE)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[TCPIPModule] LoadTCPIPSubscribeRecipe(): DeSerialize Error");
                    return RetVal;
                }
                else
                {
                    SubscribeRecipeParam = tmpParam as EventProcessList;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[GPIB] LoadGPIBSubscribeRecipe(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                //
            }

            return RetVal;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            // TODO : 
            bool retVal = false;

            try
            {
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsBusy()
        {
            bool retVal = true;
            return retVal;
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
        public EventCodeEnum CreateTesterComDriver()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(GetTesterAvailable() == true)
                {
                    if (TCPIPSysParam.EnumTCPIPComType.Value == EnumTCPIPCommType.EMUL)
                    {
                        this.TesterComDriver = new TCPIPEmulDriver();
                    }
                    else if (TCPIPSysParam.EnumTCPIPComType.Value == EnumTCPIPCommType.REAL)
                    {
                        this.TesterComDriver = new TCPIPDriver();
                    }
                    else
                    {
                        this.TesterComDriver = null;
                    }
                }
                
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EnumTCPIPEnable GetTCPIPOnOff()
        {
            EnumTCPIPEnable retval = EnumTCPIPEnable.DISABLE;

            try
            {
                retval = TCPIPSysParam.EnumTCPIPOnOff.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                retVal = this.LoadParameter(ref tmpParam, typeof(TCPIPSysParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    TCPIPSysParam_IParam = tmpParam;
                    TCPIPSysParam = TCPIPSysParam_IParam as TCPIPSysParam;
                }

                retVal = this.LoadParameter(ref tmpParam, typeof(TCPIPCommandRecipe));

                if (retVal == EventCodeEnum.NONE)
                {
                    TCPIPCommandRecipe = tmpParam as TCPIPCommandRecipe;
                }

                retVal = this.LoadParameter(ref tmpParam, typeof(TCPIPDataIDConverterParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    TCPIPDataIDConverterParam = tmpParam as TCPIPDataIDConverterParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (BinAnalyzerManager != null)
                {
                    BinAnalyzerManager.SetBinAnalyzer(TCPIPSysParam.EnumBinType.Value);
                }

                retVal = this.SaveParameter(TCPIPSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum LoadRequestParamDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(TCPIPRequestParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    TCPIPRequestParam = tmpParam as TCPIPRequestParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }

            return RetVal;

        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = LoadRequestParamDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(TCPIPRequestParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InitDevParameter()
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

        public EventCodeEnum TesterDriver_Connect(object param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(GetTesterAvailable() == true)
                {
                    retval = this.TesterCommunicationManager().CreateTesterComInstance(EnumTesterComType.TCPIP);
                }

                if (TesterComDriver != null)
                {
                    param = TCPIPSysParam;
                    retval = TesterComDriver.Connect(param);
                }

                if (retval == EventCodeEnum.NONE)
                {
                    CommunicationState = EnumCommunicationState.CONNECTED;
                    LoggerManager.TCPIPLog("TCPIP connection succeeded");
                }
                else
                {
                    LoggerManager.TCPIPLog("TCPIP connection failed");
                }
            }
            catch (Exception err)
            {
                CommunicationState = EnumCommunicationState.DISCONNECT;

                LoggerManager.Exception(err);

                LoggerManager.TCPIPLog("TCPIP connection failed");

                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        public EventCodeEnum TesterDriver_DisConnect()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (TesterComDriver != null)
                {
                    retval = TesterComDriver.DisConnect();
                }

                if (retval == EventCodeEnum.NONE)
                {
                    this.CommunicationState = EnumCommunicationState.DISCONNECT;
                    LoggerManager.TCPIPLog("TCPIP disconnection succeeded");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.TCPIPLog("TCPIP disconnection failed");
            }

            return retval;
        }


        //public EventCodeEnum TesterDriver_Receive()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        if (TesterComDriver != null)
        //        {
        //            retval = TesterComDriver.StartReceive();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public string TesterDriver_Read()
        {
            string recvData = string.Empty;

            try
            {
                recvData = TesterComDriver.Read();

                LoggerManager.TCPIPLog("[Received]" + recvData);
            }
            catch (Exception err)
            {
                recvData = string.Empty;
                LoggerManager.Exception(err);

                LoggerManager.TCPIPLog("TCPIP read failed");
            }

            return recvData;
        }

        public int TesterDriver_GetState()
        {
            try
            {
                DriverState = (int)TesterComDriver.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return DriverState;
        }

        public EventCodeEnum WriteSTB(string command)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = TCPIPModuleState.WriteSTB(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WriteString(string command)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = TCPIPModuleState.WriteString(command);

                //LoggerManager.TCPIPLog("[Sended]" + command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void TesterDriver_WriteSTB(string command)
        {
            try
            {
                TesterComDriver.WriteSTB(command);

                LoggerManager.TCPIPLog("[Sended]" + command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void TesterDriver_WriteString(string command)
        {
            try
            {
                TesterComDriver.WriteString(command);

                LoggerManager.TCPIPLog("[Sended]" + command);
                //TesterComDriver.WriteString(command + "\r\n");
                //LoggerManager.Debug(GpibStatusFlags.NONE, GpibCommunicationActionType.WRT, query_command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ReInitializeAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = TCPIPModuleState.DisConnect();

                retVal = LoadSysParameter();
                retVal = LoadDevParameter();

                retVal = RegistEventSubscribe();

                retVal = Init();

                retVal = Connect();

                //this.Initialized = false;
                //retVal = this.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name} - ReInitializeAndConnect()] Fail ReInitialize. {err.Message}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// ProberStart 커맨드 관련..
        /// 연결이 되어 있는 경우 이벤트 발생
        /// 연결이 되어 있지 않은 경우, 연결 시도 (연결 시도 후, 정상적으로 연결 되면 이벤트 발생 됨.)
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum CheckAndConnect()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(_TCPIPModuleState.GetState() == TCPIPStateEnum.CONNECTED)
                {
                    retval = this.EventManager().RaisingEvent(typeof(TesterConnectedEvent).FullName);
                }
                else
                {
                    Connect();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_TCPIPModuleState.GetState() == TCPIPStateEnum.CONNECTED)
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    retval = this.EventManager().RaisingEvent(typeof(FoupAllocatedEvent).FullName, new ProbeEventArgs(this, semaphore, allocatedInfo));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            BinAnalysisDataArray BinAnalysisData = null;

            BinAnalysisData = TCPIPModuleState.AnalyzeBin(binCode);

            return BinAnalysisData;
        }

        public DWDataBase GetDWDataBase(string argument)
        {
            DWDataBase retval = new DWDataBase();
            bool IsValid = false;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                string recvData = argument.ToUpper();

                if (recvData.Length >= 9)
                {
                    string DWId = recvData.Replace("DW", "").Substring(0, 6);

                    string DWValud = recvData.Substring(8, recvData.Length - 8);

                    int id = 0;

                    bool CanParse = int.TryParse(DWId, out id);

                    if (CanParse == true)
                    {
                        IsValid = true;

                        DRDWConnectorBase dRDWConnectorBase = GetDRDWConnector(id);

                        if (dRDWConnectorBase != null)
                        {
                            // ReadOnly 가 True인 경우, Write를 할 수 없다.
                            if (dRDWConnectorBase.IsReadOnly == true)
                            {
                                LoggerManager.Debug($"[TCPIPModule], GetIDandValue() : Invalid. ID = {DWId}, The value can only be read.");

                                IsValid = false;
                            }
                            else
                            {
                                if (dRDWConnectorBase.WriteValidationRule != null)
                                {
                                    // TODO : IF문을 없애고 싶다...

                                    InRange IsInRange = null;

                                    IsInRange = dRDWConnectorBase.WriteValidationRule as InRange;

                                    if (IsInRange != null)
                                    {
                                        IsInRange.Argument = DWValud;
                                        IsInRange.Start.Argument = dRDWConnectorBase.ID;
                                        IsInRange.End.Argument = dRDWConnectorBase.ID;

                                        ret = dRDWConnectorBase.WriteValidationRule.Run();
                                    }
                                    else
                                    {
                                        // TODO : 
                                        // InRange가 아닌 Rule의 경우

                                        IsValid = true;
                                    }
                                }
                                else
                                {
                                    IsValid = true;
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[TCPIPModule], GetIDandValue() : Invalid. ID = {DWId}, It cannot be converted to a number.");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[TCPIPModule], GetIDandValue() : Invalid. Input length = {recvData.Length}, It must be at least 9.");
                    }

                    if (IsValid == true)
                    {
                        retval.IsValid = IsValid;
                        retval.ID = id;
                        retval.value = DWValud;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public DRDWConnectorBase GetDRDWConnector(int id)
        {
            DRDWConnectorBase retval = null;

            try
            {
                this.TCPIPDataIDConverterParam.DataIDDictinary.Value.TryGetValue(id, out retval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetTesterAvailable()
        {
            bool retval = false;

            try
            {
                // 1) Eanble이 True일 것
                // 2) CommType이 사용가능한 타입으로 설정되어 있을 것.

                if (GetTCPIPOnOff() == EnumTCPIPEnable.ENABLE &&
                    TCPIPSysParam.EnumTCPIPComType.Value != EnumTCPIPCommType.UNDEFINED)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum IsControlAvailableState(out string errorlog)
        {
            errorlog = "";
            return EventCodeEnum.UNDEFINED;
        }

        //public void ClearCommandCollection()
        //{
        //    try
        //    {
        //        if (CommandCollection == null)
        //        {
        //            CommandCollection = new ObservableCollection<string>();
        //        }

        //        CommandCollection.Clear();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}
    }
}
