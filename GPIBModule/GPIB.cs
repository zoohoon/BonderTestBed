using ProberInterfaces;
using ProberInterfaces.Enum;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace GPIBModule
{
    using BinAnalyzerManager;
    using EventProcessModule.GPIB;
    using GPIBParamObject.System;
    using LogModule;
    using NotifyEventModule;

    using ProberErrorCode;
    using ProberInterfaces.BinData;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Communication.Tester;
    using ProberInterfaces.Event;
    using ProberInterfaces.Event.EventProcess;
    using ProberInterfaces.State;
    using ProberInterfaces.Wizard;
    using RequestCore.Query;
    using RequestCore.QueryPack.GPIB;
    using RequestInterface;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using TesterDriverModule;

    public class GPIB : SequenceServiceBase, IGPIB, INotifyPropertyChanged, IHasCommandRecipe//, IProbeEventSubscriber
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        internal BinAnalyzerManager BinAnalyzerManager = new BinAnalyzerManager();
        //internal GpibManager GpibManager = new GpibManager();

        public bool Initialized { get; set; } = false;

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.GPIB);
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
        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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

        private IGPIBSysParam _GPIBSysParam_IParam;
        public IGPIBSysParam GPIBSysParam_IParam
        {
            get { return _GPIBSysParam_IParam; }
            set
            {
                if (value != _GPIBSysParam_IParam)
                {
                    _GPIBSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private GPIBSysParam _GPIBSysParam;
        public GPIBSysParam GPIBSysParam
        {
            get { return _GPIBSysParam; }
            set
            {
                if (value != _GPIBSysParam)
                {
                    _GPIBSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        public CommandRecipe CommandRecipeRef
        {
            get { return GPIBCommandRecipe; }
        }

        private GPIBCommandRecipe _GPIBCommandRecipe;
        public GPIBCommandRecipe GPIBCommandRecipe
        {
            get { return _GPIBCommandRecipe; }
            set
            {
                if (value != _GPIBCommandRecipe)
                {
                    _GPIBCommandRecipe = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GpibRequestParam _GpibRequestParam;
        public GpibRequestParam GpibRequestParam
        {
            get { return _GpibRequestParam; }
            set
            {
                if (value != _GpibRequestParam)
                {
                    _GpibRequestParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<CommunicationRequestSet> RequestSetList
        {
            get { return GpibRequestParam.GpibRequestSetList; }
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
        private ObservableCollection<TransitionInfo> _TransitionInfo
             = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                _TransitionInfo = value;
            }
        }

        private GPIBState _GPIBModuleState;
        public GPIBState GPIBModuleState
        {
            get { return _GPIBModuleState; }
        }
        public IInnerState InnerState
        {
            get { return _GPIBModuleState; }
            set
            {
                if (value != _GPIBModuleState)
                {
                    _GPIBModuleState = value as GPIBState;
                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        private SubStepModules _SubModules
            = new SubStepModules();
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;

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
        /// <summary>
        /// 없어져도 됨. DeviceParam\GPIB\GpibRequestParam.json Deserialize 시 에러나서 남겨놓음. 
        /// </summary>
        private GPIBDataIDConverterParam _GPIBDataIDConverterParam;
        public GPIBDataIDConverterParam GPIBDataIDConverterParam
        {
            get { return _GPIBDataIDConverterParam; }
            set
            {
                if (value != _GPIBDataIDConverterParam)
                {
                    _GPIBDataIDConverterParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventProcessList SubscribeRecipeParam { get; set; }

        public List<CommunicationRequestSet> GpibRequestSetList
        {
            get { return GpibRequestParam.GpibRequestSetList; }
        }



        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;

        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        private bool _LISTEN_FLAG = true;
        public bool LISTEN_FLAG
        {
            get { return _LISTEN_FLAG; }
            set { _LISTEN_FLAG = value; }
        }

        private int _Ibsta = 0;
        public int Ibsta
        {
            get { return _Ibsta; }
            set
            {
                if (_Ibsta != value)
                {
                    _Ibsta = value;
                }
            }
        }

        //public int GetState()
        //{
        //    try
        //    {
        //        Ibsta = (int)this.TesterCommunicationManager().GetState();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return Ibsta;
        //}

        public GPIB()
        {
        }

        public EnumGpibEnable GetGPIBEnable()
        {
            EnumGpibEnable retval = GPIBSysParam.EnumGPIBEnable.Value;

            return retval;
        }

        public EnumGpibProbingMode GetProbingMode()
        {
            EnumGpibProbingMode retval = GPIBSysParam.EnumGpibProbingMode.Value;
            return retval;
        }

        public void SetGPIBEnable(EnumGpibEnable param)
        {
            try
            {
                GPIBSysParam.EnumGPIBEnable.Value = param;
                switch (param)
                {
                    case EnumGpibEnable.DISABLE:
                        DisConnect();
                        break;
                    case EnumGpibEnable.ENABLE:
                        Connect();
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

        #region <remarks> For IModule </remarks>
        public bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            return retVal;
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
                    _GPIBModuleState = new GPIBNotConnectedState(this);
                    StateTransition(new ModuleInitState(this));

                    retval = BinAnalyzerManager.LoadDevParameter();
                    retval = BinAnalyzerManager.InitModule();
                    BinAnalyzerManager.SetBinAnalyzer(GPIBSysParam.EnumBinType.Value);

                    // TODO : TEST CODE
                    // Unload 이벤트 발생 시, LOT END를 시키기 위해, 넣어놓은 테스트 코드.
                    //retval = this.EventManager().RegisterEvent(typeof(FoupReadyToUnloadEvent).FullName, "ProbeEventSubscibers", EventFired);

                    retval = Connect();

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

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is FoupReadyToUnloadEvent)
                {
                    this.CommandManager().SetCommand<ILotOpEnd>(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
            bool retVal = true;
            return retVal;
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

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum retval = ModuleStateEnum.ERROR;

            try
            {
                retval = this.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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

        #endregion

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

        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //retVal = GpibManager.InitModule();
                retVal = GPIBModuleState.Connect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = GPIBModuleState.DisConnect();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw new GpibDisconnectException();
            }
            return retVal;
        }

        public EventCodeEnum ReInitializeAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                try
                {
                    this.LISTEN_FLAG = true;

                    retVal = DisConnect(); // InitModule에서 Connect 해줌.
                    retVal = LoadSysParameter();
                    retVal = LoadDevParameter();
                    

                    // TODO : CHECK
                    //GpibManager.SetInitializedToTrue();

                    this.Initialized = false;
                    retVal = this.InitModule();
                }
                catch (GpibDisconnectException gpibDisconnErr)
                {
                    throw gpibDisconnErr;
                }
                catch (LoadSysParamException loadSysParamErr)
                {
                    throw loadSysParamErr;
                }
                catch (LoadDevParamException loadDevParamErr)
                {
                    throw loadDevParamErr;
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name} - ReInitializeAndConnect()] Fail ReInitialize. {err.Message}");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WriteSTB(int? command)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = GPIBModuleState.WriteSTB(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WriteString(string query_command)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = GPIBModuleState.WriteString(query_command);
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
                CommandManager.AddCommandParameters(GPIBCommandRecipe.Descriptors);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum RegistEventSubscribe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            IEventManager EventManager = this.EventManager();

            if (0 < (this.SubscribeRecipeParam?.Count ?? 0))
            {
                foreach (var evtname in this.SubscribeRecipeParam)
                {
                    RetVal = EventManager.RemoveEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GPIB] Remove EventSubscribe Error... Event:{evtname.EventFullName}");
                    }
                }
                this.SubscribeRecipeParam.Clear();
            }

            RetVal = this.LoadGPIBSubscribeRecipe();
            foreach (var evtname in this.SubscribeRecipeParam)
            {
                RetVal = EventManager.RegisterEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GPIB] Regist EventSubscribe Error... Event:{evtname.EventFullName}");
                }
            }

            return RetVal;
        }

        public BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            BinAnalysisDataArray BinAnalysisData = null;

            BinAnalysisData = GPIBModuleState.AnalyzeBin(binCode);

            return BinAnalysisData;
        }

        #region <remarks> Load & Save </remarks>
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                int prevBoardIndex = GPIBSysParam?.BoardIndex?.Value ?? 0;
                EnumGpibCommType prevCommType = GPIBSysParam?.EnumGpibComType?.Value ?? EnumGpibCommType.UNKNOWN;

                retVal = this.LoadParameter(ref tmpParam, typeof(GPIBSysParam));

                if (retVal == EventCodeEnum.NONE)
                {                    
                    GPIBSysParam = tmpParam as GPIBSysParam;
                    GPIBSysParam_IParam = GPIBSysParam as IGPIBSysParam;
                }

                retVal = this.LoadParameter(ref tmpParam, typeof(GPIBCommandRecipe));

                if (retVal == EventCodeEnum.NONE)
                {
                    GPIBCommandRecipe = tmpParam as GPIBCommandRecipe;
                }

                retVal = this.LoadParameter(ref tmpParam, typeof(GPIBDataIDConverterParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    GPIBDataIDConverterParam = tmpParam as GPIBDataIDConverterParam;
                }

          

                if (GPIBSysParam?.EnumGPIBEnable != null)
                {
                    GPIBSysParam.EnumGPIBEnable.ValueChangedEvent -= Enable_ValueChangedEvent;
                    GPIBSysParam.EnumGPIBEnable.ValueChangedEvent += Enable_ValueChangedEvent;
                }

                if (GPIBSysParam?.BoardIndex != null)
                {
                    GPIBSysParam.BoardIndex.ValueChangedEvent -= BoardIndex_ValueChangedEvent;
                    GPIBSysParam.BoardIndex.ValueChangedEvent += BoardIndex_ValueChangedEvent;
                }

                if (GPIBSysParam?.EnumGpibComType.Value != null)
                {
                    GPIBSysParam.EnumGpibComType.ValueChangedEvent -= CommType_ValueChangedEvent;
                    GPIBSysParam.EnumGpibComType.ValueChangedEvent += CommType_ValueChangedEvent;
                }


                if (prevBoardIndex.Equals(GPIBSysParam.BoardIndex.Value) == false)
                {
                    BoardIndex_ValueChangedEvent(prevBoardIndex, GPIBSysParam.BoardIndex.Value);
                }

                if (prevCommType.Equals(GPIBSysParam.EnumGpibComType.Value) == false)// 이 코드를 두번타면 이상하게 돌것 같은데 테스트 할것.
                {
                    CommType_ValueChangedEvent(prevCommType, GPIBSysParam.EnumGpibComType.Value);
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

            retVal = this.SaveParameter(GPIBSysParam);
            retVal = this.SaveParameter(GPIBCommandRecipe);

            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {           
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadRequestParamParameter(EnumGpibCommType commType)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            GpibRequestParam = new GpibRequestParam();

            try
            {
                //string FullPath = this.FileManager().GetDeviceParamFullPath(GpibRequestParam.FilePath, GpibRequestParam.FileName);

                string folderPath = this.FileManager().GetSystemParamFullPath(GpibRequestParam.FilePath, commType.ToString());
                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                string FullPath = Path.Combine(folderPath, GpibRequestParam.FileName);

                // TODO : Namespace(GPIB to Internal) 변경으로 다시 만들어줘야 됨.
                if (File.Exists(FullPath) == false)
                {
                    RetVal = GpibRequestParam.SetDefaultParam(commType);
                    RetVal = this.SaveParameter(GpibRequestParam, fixFullPath: FullPath);
                }
               

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(GpibRequestParam), fixFullPath: FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    GpibRequestParam = tmpParam as GpibRequestParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[GPIB] LoadRequestParamDevParameter(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw new LoadDevParamException();
            }

            return RetVal;

        }

      
        public Type[] GetRequestType()
        {
            List<Type> retList = new List<Type>();
            retList.Add(typeof(CommunicationRequestSet));
            retList.Add(typeof(RequestCore.ActionPack.Action));
            retList.Add(typeof(RequestCore.QueryPack.Query));
            retList.Add(typeof(RequestCore.Controller.Controller));

            try
            {
                var RequestBaseTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                       from type in assembly.GetTypes()
                                       where type.IsSubclassOf(typeof(RequestBase))
                                       select type;

                foreach (var RequestBaseType in RequestBaseTypes)
                {
                    retList.Add(RequestBaseType);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[EventProcessBase] [Method = GetAllType] [Error = {err}]");
            }

            return retList.ToArray();
        }


        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            retVal = SaveRequestParamDevParameter();

            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum SaveRequestParamDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string FullPath = this.FileManager().GetDeviceParamFullPath(GpibRequestParam.FilePath, GpibRequestParam.FileName);

            try
            {
                RetVal = this.SaveParameter(GpibRequestParam);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[GPIB] LoadRequestParamDevParameter(): Serialize Error");
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[GPIB] LoadRequestParamDevParameter(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);


            }

            return RetVal;

        }

        private EventCodeEnum LoadGPIBSubscribeRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SubscribeRecipeParam = new EventProcessList();

                string recipeFileName = $"Recipe_Subscribe_GPIB.json";//$"Recipe_Subscribe_GPIB_{GPIBSysParam.EnumGpibComType.ToString()}.json";
                string folderPath = this.FileManager().GetSystemParamFullPath("GPIB", GPIBSysParam.EnumGpibComType.Value.ToString());
                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }


                string FullPath = Path.Combine(folderPath, recipeFileName);

                if (File.Exists(FullPath) == false) //Todo : 임시 주석. Jake :P
                {
                    GpibEventParam eventParam = null;

                    if (GPIBSysParam.EnumGpibComType.Value == EnumGpibCommType.TEL ||
                        GPIBSysParam.EnumGpibComType.Value == EnumGpibCommType.TEL_EMUL
                        )
                    {
                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 70;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(GoToStartDieEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 67;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(ProbingZUpFirstProcessEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 89;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(CalculatePfNYieldEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 71;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(RemainSequenceEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 75;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(DontRemainSequenceEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 72;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(FoupReadyToUnloadEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 78;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(LotEndEvent).FullName, Parameter = eventParam });
                    }
                    else if (
                        GPIBSysParam.EnumGpibComType.Value == EnumGpibCommType.TSK_SPECIAL ||
                        GPIBSysParam.EnumGpibComType.Value == EnumGpibCommType.TSK_SPECIAL_EMUL
                       )
                    {
                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 121;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(LotStartEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 70;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(GoToStartDieEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 67;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(ProbingZUpProcessEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 68;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(ProbingZDownProcessEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 75;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(DontRemainSequenceEvent).FullName, Parameter = eventParam });


                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 72;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(LotEndEvent).FullName, Parameter = eventParam });
                    }
                    else if (GPIBSysParam.EnumGpibComType.Value == EnumGpibCommType.TSK ||
                             GPIBSysParam.EnumGpibComType.Value == EnumGpibCommType.TSK_EMUL
                            )
                    {

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 121;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(LotStartEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 70;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(GoToStartDieEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 67;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(ProbingZUpProcessEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 69;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(CalculatePfNYieldEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 81;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(DontRemainSequenceEvent).FullName, Parameter = eventParam });
                   
                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 80;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(WaferUnloadedEvent).FullName, Parameter = eventParam });


                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 82;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(FoupReadyToUnloadEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 94;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(LotEndEvent).FullName, Parameter = eventParam });
                        //------------------------------------------------------------





                        #region External Mode 2
                  
                       

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 71;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(LotEndDueToUnloadAllWaferEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 76;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(IncorrectDRNumberEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 98;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(PassDWCommandEvent).FullName, Parameter = eventParam });

                        eventParam = new GpibEventParam();
                        eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        eventParam.StbNumber = 99;
                        SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(FailDWCommandEvent).FullName, Parameter = eventParam });


                        ////tsk sp ver 
                        //eventParam = new GpibEventParam();
                        //eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        //eventParam.StbNumber = 75;
                        //SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(WaferEndEvent).FullName, Parameter = eventParam });
                        #endregion


                        #region YMTC - GpibEventParam
                        // "J" : return (66)
                        //eventParam = new GpibEventParam();
                        //eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        //eventParam.StbNumber = 66;
                        //SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(GoToStartDieEvent).FullName, Parameter = eventParam });

                        // "J", "Z" : return (67)
                        //eventParam = new GpibEventParam();
                        //eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        //eventParam.StbNumber = 67;
                        //SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(ProbingZUpProcessEvent).FullName, Parameter = eventParam });


                        // "M" : return (69) X
                        //eventParam = new GpibEventParam();
                        //eventParam.CommandName = typeof(ISRQ_RESPONSE).ToString();
                        //eventParam.StbNumber = 69;
                        //SubscribeRecipeParam.Add(new GPIBEventProc_StbCmdSetter() { EventFullName = typeof(ProbingZDownProcessEvent).FullName, Parameter = eventParam });
                        #endregion
                    }

                    RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[GPIB] LoadGPIBSubscribeRecipe(): Serialize Error");
                        return RetVal;
                    }
                }

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(EventProcessList), null, FullPath);

                if (RetVal != EventCodeEnum.NONE)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[GPIB] LoadGPIBSubscribeRecipe(): DeSerialize Error");
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

        #endregion


        private void Enable_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                EnumGpibEnable inputvalue;
                bool rst = EnumGpibEnable.TryParse(newValue.ToString(), out inputvalue);

                if (rst)
                {
                    switch (inputvalue)
                    {
                        case EnumGpibEnable.DISABLE:
                            this.SequenceEngineManager().StopSequence(this.GPIB() as ISequenceEngineService);
                            this.GPIB().DisConnect();
                            break;
                        case EnumGpibEnable.ENABLE:
                            this.SequenceEngineManager().RunSequence(this.GPIB() as ISequenceEngineService, "GPIB");
                            this.GPIB().Connect();
                            break;
                        default:
                            break;
                    }
                }

                this.GPIB().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void BoardIndex_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                if (this.GPIB().ModuleState?.GetState() != null)
                {
                    this.GPIB().DisConnect();
                    this.GPIB().Connect();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CommType_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                // Load Event Parameter 
                RegistEventSubscribe();
                // Load Request Parameter 
                LoadRequestParamParameter(GPIBSysParam.EnumGpibComType.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool retVal = false;
            msg = string.Empty;

            try
            {
                if (GetGPIBEnable() == EnumGpibEnable.ENABLE)
                {
                    retVal = this.ModuleState.State != ModuleStateEnum.ERROR;
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

        public BinType GetBinCalulateType() => this.GPIBSysParam.EnumBinType.Value;

        public URUWConnectorBase GetURUWConnector(int id)
        {
            URUWConnectorBase retval = null;

            try
            {
                this.GPIBDataIDConverterParam.DataIDDictinary.Value.TryGetValue(id, out retval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public EventCodeEnum CreateTesterComDriver()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (((int)(GPIBSysParam.EnumGpibComType.Value) & 0x1000) == 0x1000)
                {
                    this.TesterComDriver = new NI4882EmulDriver();
                }
                else
                {
                    this.TesterComDriver = new NI4882Driver();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum TesterDriver_Connect(object param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.TesterCommunicationManager()?.CreateTesterComInstance(EnumTesterComType.GPIB) ?? EventCodeEnum.UNDEFINED;

                if (TesterComDriver != null)
                {
                    retval = TesterComDriver.Connect(param);
                }

                if (retval == EventCodeEnum.NONE)
                {
                    CommunicationState = EnumCommunicationState.CONNECTED;
                }
            }
            catch (Exception err)
            {
                CommunicationState = EnumCommunicationState.DISCONNECT;

                LoggerManager.Exception(err);
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
                    LoggerManager.GpibCommlog(Convert.ToInt64(GpibStatusFlags.NONE), Convert.ToInt32(GpibCommunicationActionType.CONN), "GPIB disconnection succeeded");
                }
            }
            catch (Exception err)
            {
                LoggerManager.GpibCommlog(Convert.ToInt64(GpibStatusFlags.NONE), Convert.ToInt32(GpibCommunicationActionType.CONN), "GPIB disconnection failed");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public long EnumToLong(GpibStatusFlags enumVal)
        //{
        //    return (long)enumVal;
        //}

        //public int EnumToInt(GpibCommunicationActionType enumVal)
        //{
        //    return (int)enumVal;
        //}

        public int TesterDriver_GetState()
        {
            try
            {
                Ibsta = (int)TesterComDriver.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Ibsta;
        }

        public string TesterDriver_Read()
        {
            string recvData = string.Empty;

            try
            {
                recvData = TesterComDriver.Read();

                if (recvData != null)
                {
                    LoggerManager.GpibCommlog(Convert.ToInt64((GpibStatusFlags)this.Ibsta), Convert.ToInt32(GpibCommunicationActionType.READ), recvData.Replace("\r", "").Replace("\n", ""));
                    LISTEN_FLAG = false;
                }
                else
                {
                    LoggerManager.Debug($"TesterDriver_Read(): recvData is null");
                    LISTEN_FLAG = true;
                }

            }
            catch (Exception err)
            {
                recvData = string.Empty;
                LoggerManager.GpibCommlog(Convert.ToInt64((GpibStatusFlags)this.Ibsta), Convert.ToInt32(GpibCommunicationActionType.READ), "GPIB Read failed");
                LISTEN_FLAG = true;
                LoggerManager.Exception(err);
            }

            return recvData;
        }

        public void TesterDriver_WriteString(string query_command)
        {
            try
            {
                TesterComDriver.WriteString(query_command + "\r\n");
                LoggerManager.GpibCommlog(Convert.ToInt64(GpibStatusFlags.NONE), Convert.ToInt32(GpibCommunicationActionType.WRT), query_command);
            }
            catch (Exception err)
            {
                LoggerManager.GpibErrorlog("Failed Write Command.");
                LoggerManager.Exception(err);
            }
            finally
            {
                LISTEN_FLAG = true;
            }
        }

        public bool GetTesterAvailable()
        {
            bool retval = false;

            try
            {
                // TODO : 
                // 1) Eanble이 True일 것
                // 2) CommType이 사용가능한 타입으로 설정되어 있을 것. 
                // GPIBSysParam.EnumGpibComType.Value ???

                if (GetGPIBEnable() == EnumGpibEnable.ENABLE)
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

        public string GetFullSite(long mX, long mY)
        {
            string retVal = "";
            try
            {
                if (this.GEMModule().GetPIVContainer().TmpFullSite.Equals("") == true)
                {
                    GetOnTestDutInformation getOnWaferInfoObj = new GetOnTestDutInformation();
                    retVal = getOnWaferInfoObj.MakeOnWaferDutInfo(mX, mY);
                }
                else
                {
                    retVal = this.GEMModule().GetPIVContainer().TmpFullSite;
                }

                if (this.GEMModule().GetPIVContainer().IsInsertOCommand == true)
                {
                    retVal = retVal.Insert(0, "O");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ProcessReadObject(string source, out string commandName, out string argu, out CommunicationRequestSet findreqset)
        {
            EventCodeEnum retVal = EventCodeEnum.GPIB_READ_FAIL;
            commandName = "";
            argu = "";
            findreqset = null;
            try
            {
                if (source != null)
                {
                    ////////////////////////////////
                    // Todo : Test Code
                    // ModuleManager.GPIBDriver.Clear();//test
                    ////////////////////////////////

                    if (source.Length > 0)
                    {
                        source = RemoveStr(source, "\r", "\n", "!");
                        source = source.Trim();

                        findreqset = this?.GpibRequestSetList.Find(i => i.Name == source);
                        if (findreqset == null)
                        {
                            LoggerManager.Debug($"GPIB OnRecvProcessing(): {source} Not exist in GpibRequestSetList");
                        }


                        int cmdLength = 5 < source.Length ? 5 : source.Length;
                        string onlycmd = string.Empty;
                        string findString = string.Empty;
                        while (1 <= cmdLength)
                        {
                            if (findreqset == null)
                            {
                                if (cmdLength <= source.Length)
                                {
                                    findString = source.Substring(0, cmdLength);
                                    findreqset = this.GpibRequestSetList.Find(i => i.Name == findString);
                                    if (findreqset != null)
                                    {
                                        onlycmd = findreqset.Name;
                                        break;
                                    }

                                }
                            }
                            else
                            {
                                findString = source;
                                break;
                            }

                            cmdLength--;
                        }

                        if (findreqset != null)
                        {
                            commandName = findString;
                            argu = source.Replace(commandName, "");
                            findreqset.Request.Argument = argu;
                            retVal = EventCodeEnum.NONE;
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


        private string RemoveStr(string recv_data, params string[] args)
        {
            string retStr = recv_data;
            try
            {
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        retStr = retStr.Replace(arg, "");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retStr;
        }

    }

    [Serializable]
    internal class LoadDevParamException : Exception
    {
        public LoadDevParamException()
        {
        }

        public LoadDevParamException(string message) : base(message)
        {
        }

        public LoadDevParamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LoadDevParamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class LoadSysParamException : Exception
    {
        public LoadSysParamException()
        {
        }

        public LoadSysParamException(string message) : base(message)
        {
        }

        public LoadSysParamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LoadSysParamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class GpibDisconnectException : Exception
    {
        public GpibDisconnectException()
        {
        }

        public GpibDisconnectException(string message) : base(message)
        {
        }

        public GpibDisconnectException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GpibDisconnectException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
