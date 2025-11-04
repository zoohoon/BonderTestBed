using System.Linq;

namespace WaferAlign
{
    using ProberInterfaces;
    using RelayCommandBase;
    using Autofac;
    using System.Windows;
    using System.IO;
    using ProberInterfaces.Param;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using ProberInterfaces.Align;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using ProberInterfaces.WaferAlignEX;
    using ProberInterfaces.PnpSetup;
    using SubstrateObjects;
    using PnPControl;
    using ProberInterfaces.Command;
    using ProberErrorCode;
    using ProberInterfaces.Template;
    using WaferAlignParam;
    using ProberInterfaces.Wizard;
    using ProberInterfaces.State;
    using global::States;
    using LogModule;
    using PnPontrol;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using System.ServiceModel;
    using MetroDialogInterfaces;
    using ProberInterfaces.WaferAligner;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using ProbeCardObject;
    using WA_HighMagParameter_Standard;
    using WA_LowMagParameter_Standard;
    using ProberInterfaces.Command.Internal;
    using Focusing;
    using System.IO.Compression;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WaferAligner : WizardStepBase, IWaferAligner, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable
    {
        public double SafeHeightOnException { get; set; } = 9999;

        #region //..Template
        private TemplateStateCollection _Template;
        public TemplateStateCollection Template
        {
            get { return _Template; }
            set
            {
                if (value != _Template)
                {
                    _Template = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override bool Initialized { get; set; } = false;

        private ITemplateFileParam _TemplateParameter;
        public ITemplateFileParam TemplateParameter
        {
            get { return _TemplateParameter; }
            set
            {
                if (value != _TemplateParameter)
                {
                    _TemplateParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ITemplateParam LoadTemplateParam { get; set; }
        public ISubRoutine SubRoutine { get; set; }

        #endregion

        #region Property
        public WaferAligner()
        {

        }

        #region PTPA DEBUG
        private bool _IsOnDubugMode = false;
        // 해당 옵션을 true 로 하면 Low, High Align  시에 오리진 패턴위치 ( Jump Index 0,0 ) 위치의 프로세싱 결과 이미지 저장
        // Wafer Alignment 후에 저장된 패드 위치로 이동해서 이미지 저장.
        public bool IsOnDubugMode
        {
            get { return _IsOnDubugMode; }
            set
            {
                if (value != _IsOnDubugMode)
                {
                    _IsOnDubugMode = value;

                    LoggerManager.Debug($"WaferAlign - IsOnDubugMode set to {_IsOnDubugMode}");

                    RaisePropertyChanged();
                }
            }
        }

        private string _IsOnDebugImagePathBase;
        public string IsOnDebugImagePathBase
        {
            get { return _IsOnDebugImagePathBase; }
            set
            {
                if (value != _IsOnDebugImagePathBase)
                {
                    _IsOnDebugImagePathBase = value;

                    LoggerManager.Debug($"WaferAlign - IsOnDebugImagePathBase set to {IsOnDebugImagePathBase}");

                    RaisePropertyChanged();
                }
            }
        }

        private string _IsOnDebugPadPathBase;
        public string IsOnDebugPadPathBase
        {
            get { return _IsOnDebugPadPathBase; }
            set
            {
                if (value != _IsOnDebugPadPathBase)
                {
                    _IsOnDebugPadPathBase = value;

                    LoggerManager.Debug($"WaferAlign - IsOnDebugPadPathBase set to {_IsOnDebugPadPathBase}");

                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        private bool _IsNewSetup;

        public bool IsNewSetup
        {
            get { return _IsNewSetup; }
            set { _IsNewSetup = value; }
        }

        private bool _IsManualTriggered;
        public bool IsManualTriggered
        {
            get { return _IsManualTriggered; }
            set
            {
                if (value != _IsManualTriggered)
                {
                    _IsManualTriggered = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsManualOPFinished = false;
        public bool IsManualOPFinished
        {
            get { return _IsManualOPFinished; }
            set
            {
                if (value != _IsManualOPFinished)
                {
                    _IsManualOPFinished = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventCodeEnum _ManualOPResult = EventCodeEnum.UNDEFINED;
        public EventCodeEnum ManualOPResult
        {
            get { return _ManualOPResult; }
            set
            {
                if (value != _ManualOPResult)
                {
                    _ManualOPResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void SetIsNewSetup(bool flag)
        {
            IsNewSetup = flag;
        }

        private bool _IsModifySetup;

        public void SetIsModifySetup(bool flag)
        {
            _IsModifySetup = flag;
        }
        public bool GetIsModifySetup()
        {
            if (IsNewSetup)
            { _IsModifySetup = false; }
            return _IsModifySetup;
        }


        private bool _IsModify { get; set; }

        public bool GetIsModify()
        {
            bool retVal = false;

            if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
            {
                retVal = _IsModify;
            }
            else
            {
                retVal = false;
                if (WaferAlignControItems.IsManualRecoveryModifyMode)
                {
                    _IsModify = true;
                    retVal = true;
                }

                //string msg = "";
                //retVal = _IsModify | IsLotReady(out msg);
                //retVal = retVal & (IsNewSetup == false);
            }
            return retVal;
        }
        public void SetIsModify(bool flag)
        {
            _IsModify = flag;

        }
        private WaferAlignInfomation _WaferAlignInfo = new WaferAlignInfomation();
        public WaferAlignInfomation WaferAlignInfo
        {
            get { return _WaferAlignInfo; }
            set
            {
                if (value != _WaferAlignInfo)
                {
                    _WaferAlignInfo = value;
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
        private int _TotalHeightPoint = 0;
        public int TotalHeightPoint
        {
            get { return _TotalHeightPoint; }
            set
            {
                if (value != _TotalHeightPoint)
                {
                    _TotalHeightPoint = value;
                    RaisePropertyChanged();
                }
            }
        }


        WaferAlignEctFunction EctFunction;

        private WaferAlignState _WaferAlignState;
        public WaferAlignState WaferAlignState
        {
            get { return _WaferAlignState; }
        }

        public IInnerState InnerState
        {
            get { return _WaferAlignState; }
            set
            {
                if (value != _WaferAlignState)
                {
                    _WaferAlignState = value as WaferAlignState;
                    RaisePropertyChanged();
                }
            }
        }

        public IInnerState PreInnerState
        {
            get;
            set;
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _waferaligncontinus;

        public bool waferaligncontinus
        {
            get { return _waferaligncontinus; }
            set { _waferaligncontinus = value; }
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


        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        private IProbeCommandToken _RequestToken;
        public IProbeCommandToken RequestToken
        {
            get { return _RequestToken; }
            set { _RequestToken = value; }
        }



        private WARecoveryModule _RecoveryInfo = new WARecoveryModule();

        public WARecoveryModule RecoveryInfo
        {
            get { return _RecoveryInfo; }
            set { _RecoveryInfo = value; }
        }


        public void SetInjector(IStateModule obj, string HashCode)
        {

        }

        private ObservableCollection<TransitionInfo> _TransitionInfo;
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
        private EnumTemplateEnable _TemplateEnable = EnumTemplateEnable.NEED;
        public EnumTemplateEnable TemplateEnable
        {
            get { return _TemplateEnable; }
            set
            {
                if (value != _TemplateEnable)
                {
                    _TemplateEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }

        private IWaferAlignControItems _WaferAlignControItems;
        public IWaferAlignControItems WaferAlignControItems
        {
            get { return _WaferAlignControItems; }
            set { _WaferAlignControItems = value; }
        }

        private WaferAlignSysParameter _WaferAlignSysParam;

        public WaferAlignSysParameter WaferAlignSysParam
        {
            get { return _WaferAlignSysParam; }
            set { _WaferAlignSysParam = value; }
        }


        #endregion

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set
            {
                _ForcedDone = value;
                if (this.LoaderController() != null)
                {
                    this.LoaderController()?.SetForcedDoneState();
                }
            }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.WaferAlign);
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

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        private List<DutWaferIndex> DutDieIndexs = new List<DutWaferIndex>();
        public List<DutWaferIndex> GetDutDieIndexs() => DutDieIndexs;
        public void SetDefaultDutDieIndexs() => DutDieIndexs = new List<DutWaferIndex>();

        private long _TeachDieXIndex;
        public long TeachDieXIndex
        {
            get { return _TeachDieXIndex; }
            set
            {
                if (value != _TeachDieXIndex)
                {
                    _TeachDieXIndex = value;
                    RaisePropertyChanged(nameof(TeachDieXIndex));
                }
            }
        }

        private long _TeachDieYIndex;
        public long TeachDieYIndex
        {
            get { return _TeachDieYIndex; }
            set
            {
                if (value != _TeachDieYIndex)
                {
                    _TeachDieYIndex = value;
                    RaisePropertyChanged(nameof(TeachDieYIndex));
                }
            }
        }

        private WaferCoordinate[,] _HeightProfilingArray = new WaferCoordinate[3, 3];
        public WaferCoordinate[,] HeightProfilingArray
        {
            get { return _HeightProfilingArray; }
            set
            {
                if (value != _HeightProfilingArray)
                {
                    _HeightProfilingArray = value;
                    RaisePropertyChanged(nameof(HeightProfilingArray));
                }
            }
        }

        private WaferCoordinate _PlanePointCenter = new WaferCoordinate();
        public WaferCoordinate PlanePointCenter
        {
            get { return _PlanePointCenter; }
            set
            {
                if (value != _PlanePointCenter)
                {
                    _PlanePointCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ManuallAlignmentErrTxt = "";
        public string ManuallAlignmentErrTxt
        {
            get { return _ManuallAlignmentErrTxt; }
            set { _ManuallAlignmentErrTxt = value; }
        }
        private WA_LowMagParam_Standard LowStandardParam_Clone;
        public WA_HighMagParam_Standard HighStandardParam_Clone;

        private HeightPointEnum _HeightProfilingPointType;
        public HeightPointEnum HeightProfilingPointType
        {
            get { return _HeightProfilingPointType; }
            set
            {
                if (value != _HeightProfilingPointType)
                {
                    _HeightProfilingPointType = value;
                    RaisePropertyChanged(nameof(HeightProfilingPointType));
                }
            }
        }

        public new EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    EctFunction = new WaferAlignEctFunction(this);
                    retval = EctFunction.InitModule();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    InnerState = new WaferAlignIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    EnableState = new EnableIdleState(EnableState);
                    WaferAlignControItems = new WaferAlignControItems();
                    Initialized = true;

                    IsOnDebugImagePathBase = $"{LoggerManager.LoggerManagerParam.FilePath}\\{LoggerManager.LoggerManagerParam.DevFolder}\\Image\\" +
                        $"WaferAlign\\ProcessingPattern\\";
                    IsOnDebugPadPathBase = $"{LoggerManager.LoggerManagerParam.FilePath}\\{LoggerManager.LoggerManagerParam.DevFolder}\\Image\\" +
                        $"WaferAlign\\Pad\\";

                    this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired); //ISSD-4260 Event 확인 필요.

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

        public new void DeInitModule()
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


        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new WaferAlignTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(WaferAlignTemplateFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    TemplateParameter = tmpParam as WaferAlignTemplateFile;
                }

                RetVal = LoadTemplate();

                if (this.TemplateManager() != null)
                {
                    RetVal = this.TemplateManager().InitTemplate(this);
                }
                else
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(TemplateParameter);
                foreach (var submodule in Template.GetProcessingModule())
                {
                    if (submodule is IHasDevParameterizable)
                        (submodule as IHasDevParameterizable).SaveDevParameter();
                }
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

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new WaferAlignSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(WaferAlignSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    WaferAlignSysParam = tmpParam as WaferAlignSysParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(WaferAlignSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public WaferAlignInnerStateEnum GetWAInnerStateEnum()
        {
            WaferAlignInnerStateEnum state;
            try
            {
                state = (_WaferAlignState as WaferAlignStateBase).GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return state;
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
                throw;
            }
        }
        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = _WaferAlignState;
                InnerState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());

                List<ISubModule> modules = Template.GetProcessingModule();

                for (int index = 0; index < modules.Count; index++)
                {
                    retVal = modules[index].ClearData();
                }

                UpdateHeightProfiling(); //Align시작 전, HeightProfilingPointType에 따라서 AcceptFocusing 값 변경.

                LoggerManager.Debug("WaferAligner ClearState()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                WaferAlignInnerStateEnum state = (InnerState as WaferAlignState).GetState();

                retval = state.ToString();
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
                throw;
            }

            return stat;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {

                RetVal = _WaferAlignState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }


        #region //..PNP
        public void SetSetupState()
        {
            try
            {
                InnerStateTransition(new WaferAlignSetupState(this));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetModuleDoneState()
        {
            try
            {
                InnerStateTransition(new WaferAlignDoneState(this));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            try
            {
                if (ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                    return TemplateToPnpConverter.Converter(Template.TemplateModules, false);
                else
                    return TemplateToPnpConverter.Converter(Template.TemplateModules);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }
        #endregion

        #region //..Template
        public EventCodeEnum LoadTemplate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new WaferAlignTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                this.LoadParameter(ref tmpParam, typeof(WaferAlignTemplateFile));

                TemplateParameter = (WaferAlignTemplateFile)tmpParam;
                //DevParam = TemplateParameter;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SaveTemplate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (File.Exists(TemplateParameter.Param.SeletedTemplate.Path.Value) == true)
                {

                    //foreach (var category in SubModules.TemplateModules)
                    foreach (var category in Template.TemplateModules)

                    {
                        if (category is ModuleInfo)
                        {
                            (category as ModuleInfo).SetupEnableState
                                = (category as IWizardStep).StateEnable;
                        }
                        else if (category is CategoryInfo)
                        {
                            foreach (var module in (category as CategoryInfo).Categories)
                            {
                                (module as ModuleInfo).SetupEnableState
                                = (category as IWizardStep).StateEnable;
                            }
                        }

                    }


                    //}

                    Extensions_IParam.SaveParameter(null, Template.TemplateModules, null, TemplateParameter.Param.SeletedTemplate.Path.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetTemplate(int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (index > 0 && index < TemplateParameter.Param.TemplateInfos.Count)
                {
                    TemplateParameter.Param.SeletedTemplate =
                        TemplateParameter.Param.TemplateInfos[index];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ObservableCollection<IWizardStep> GetTemplate()
        {
            ObservableCollection<IWizardStep> steps = new ObservableCollection<IWizardStep>();
            try
            {

                steps = TemplateToWizardConverter.Converter(Template.TemplateModules);
            }
            catch (Exception err)
            {

                throw err;
            }
            return steps;
        }
        #endregion

        #region //..Wizard
        public EventCodeEnum LoadSummary()
        {
            return EventCodeEnum.NONE;
        }

        public override ObservableCollection<IWizardStep> GetWizardStep()
        {
            return TemplateToWizardConverter.Converter(Template.TemplateModules);
        }
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                for (int index = 0; index < Template.TemplateModules.Count; index++)
                {
                    if (Template.TemplateModules[index] is CategoryForm)
                    {
                        for (int jndex = 0; jndex < (Template.TemplateModules[index] as ICategoryNodeItem)?.Categories.Count; jndex++)
                        {

                            retVal = (Template.TemplateModules[index] as ICategoryNodeItem).Categories[jndex].ParamValidation();
                            if (retVal != EventCodeEnum.NONE)
                                return retVal;
                        }
                        retVal = Template.TemplateModules[index].ParamValidation();
                        if (retVal != EventCodeEnum.NONE)
                            return retVal;
                    }
                    else
                    {
                        retVal = Template.TemplateModules[index].ParamValidation();
                        if (retVal != EventCodeEnum.NONE)
                            return retVal;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #endregion

        #region //..HeightProfiling
        public void CreateBaseHeightProfiling(WaferCoordinate coordinate)
        {
            try
            {
                if (coordinate == null)
                    return;

                double distance = 500000;
                double edgepos = 0.0;
                edgepos = ((distance / 2) / Math.Sqrt(2));

                AddHeighPlanePoint(coordinate);
                //Left
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX() - (distance / 2), coordinate.GetY(), coordinate.GetZ()));
                //Right
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX() + (distance / 2), coordinate.GetY(), coordinate.GetZ()));
                //Upper
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX(), coordinate.GetY() + (distance / 2), coordinate.GetZ()));
                //Lower
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX(), coordinate.GetY() - (distance / 2), coordinate.GetZ()));
                //LeftUpper
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX() - edgepos, coordinate.GetY() + edgepos, coordinate.GetZ()));
                //RightUpper
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX() + edgepos, coordinate.GetY() + edgepos, coordinate.GetZ()));
                //LeftLower
                AddHeighPlanePoint(new WaferCoordinate(
                    coordinate.GetX() - edgepos, coordinate.GetY() - edgepos, coordinate.GetZ()));
                //RightLower
                AddHeighPlanePoint(new WaferCoordinate(
                 coordinate.GetX() + edgepos, coordinate.GetY() - edgepos, coordinate.GetZ()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        /// <summary>
        /// Pin_align에서 구해진 Z0, Z1, Z2을 포함하는 평면을 구하는 함수
        /// </summary>
        /// <param name="PlanePoint">PlanePoint x,y에 대한 틸트 값의 기울기를 구하기 위함</param>
        public double CalcThreePodTiltedPlane(double posx, double posy, bool logwrite = false)
        {
            double retVal = 0.0;

            try
            {
                float z0angle = this.CoordinateManager().StageCoord.Z0Angle.Value;
                float z1angle = this.CoordinateManager().StageCoord.Z1Angle.Value;
                float z2angle = this.CoordinateManager().StageCoord.Z2Angle.Value;
                double pcd = this.CoordinateManager().StageCoord.PCD.Value;

                double pillarX0 = pcd * Math.Cos(Math.PI * (z0angle) / 180);
                double pillarY0 = pcd * Math.Sin(Math.PI * (z0angle) / 180);
                double pillarZ0 = 0d;

                double pillarX1 = pcd * Math.Cos(Math.PI * (z1angle) / 180);
                double pillarY1 = pcd * Math.Sin(Math.PI * (z1angle) / 180);
                double pillarZ1 = 0d;

                double pillarX2 = pcd * Math.Cos(Math.PI * (z2angle) / 180);
                double pillarY2 = pcd * Math.Sin(Math.PI * (z2angle) / 180);
                double pillarZ2 = 0d;

                var z0Axis = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                var z1Axis = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                var z2Axis = this.MotionManager().GetAxis(EnumAxisConstants.Z2);

                pillarZ0 = z0Axis.Status.CompValue = this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[0];
                pillarZ1 = z1Axis.Status.CompValue = this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[1];
                pillarZ2 = z2Axis.Status.CompValue = this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[2];

                WaferCoordinate[] tmpPoint = new WaferCoordinate[3];

                tmpPoint[0] = new WaferCoordinate(pillarX0, pillarY0, pillarZ0);
                tmpPoint[1] = new WaferCoordinate(pillarX1, pillarY1, pillarZ1);
                tmpPoint[2] = new WaferCoordinate(pillarX2, pillarY2, pillarZ2);

                double[] pointD = new double[4];
                pointD[0] = 0;
                pointD[1] = 0;
                pointD[2] = 0;
                pointD[3] = 0;

                pointD[0] = tmpPoint[0].Y.Value * (tmpPoint[1].Z.Value - tmpPoint[2].Z.Value) +
                                    tmpPoint[1].Y.Value * (tmpPoint[2].Z.Value - tmpPoint[0].Z.Value) +
                                    tmpPoint[2].Y.Value * (tmpPoint[0].Z.Value - tmpPoint[1].Z.Value);

                pointD[1] = tmpPoint[0].Z.Value * (tmpPoint[1].X.Value - tmpPoint[2].X.Value) +
                            tmpPoint[1].Z.Value * (tmpPoint[2].X.Value - tmpPoint[0].X.Value) +
                            tmpPoint[2].Z.Value * (tmpPoint[0].X.Value - tmpPoint[1].X.Value);

                pointD[2] = tmpPoint[0].X.Value * (tmpPoint[1].Y.Value - tmpPoint[2].Y.Value) +
                            tmpPoint[1].X.Value * (tmpPoint[2].Y.Value - tmpPoint[0].Y.Value) +
                            tmpPoint[2].X.Value * (tmpPoint[0].Y.Value - tmpPoint[1].Y.Value);

                pointD[3] = -tmpPoint[0].X.Value * ((tmpPoint[1].Y.Value * tmpPoint[2].Z.Value) - (tmpPoint[2].Y.Value * tmpPoint[1].Z.Value)) -
                             tmpPoint[1].X.Value * ((tmpPoint[2].Y.Value * tmpPoint[0].Z.Value) - (tmpPoint[0].Y.Value * tmpPoint[2].Z.Value)) -
                             tmpPoint[2].X.Value * ((tmpPoint[0].Y.Value * tmpPoint[1].Z.Value) - (tmpPoint[1].Y.Value * tmpPoint[0].Z.Value));

                if (pointD[2] != 0)
                {
                    retVal = -(pointD[0] * posx + pointD[1] * posy + pointD[3]) / pointD[2];
                }
                else
                {
                    retVal = (pillarZ0 + pillarZ1 + pillarZ2) / tmpPoint.Length;
                    if (logwrite == true)
                    {
                        LoggerManager.Error($"WaferAligner() - CalcThreePodTiltedPlane(): Value Error {pointD[0]}x+{pointD[1]}y+{pointD[2]}z+{pointD[3]}, Average three pod Z: {retVal:0.00}");
                    }
                }

                //wafer size에 따른 tolerance를 계산하기 위함.
                double compLimit = PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value;
                double radiuswafer = Wafer.GetPhysInfo().WaferSize_um.Value / 2;
                double tolerance = (radiuswafer / pcd) * compLimit;
                if (retVal > tolerance)
                {
                    retVal = (pillarZ0 + pillarZ1 + pillarZ2) / tmpPoint.Length;
                    if (logwrite == true)
                    {
                        LoggerManager.Error($"WaferAligner() - CalcThreePodTiltedPlane() : Tolerance Error (X,Y) = ({posx:0.00},{posy:0.00}) GetTilted Z = {retVal:0.00}, tolerance : {tolerance:0.00}");
                    }
                }

                if (logwrite == true)
                {
                    LoggerManager.Debug($"WaferAligner() - CalcThreePodTiltedPlane() : (X,Y) = ({posx:0.00},{posy:0.00}) GetTilted Z = {retVal:0.00}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaferAligner(): Error occurred. Err = {err.Message}");
            }
            return retVal;
        }
        public void AddHeighPlanePoint(WAHeightPositionParam param = null, bool center_standard = false)
        {
            try
            {
                //ICamera preCamera;
                double radius = 0.0;
                double TiltedZ = 0.0;
                WaferCoordinate PlanePoint = new WaferCoordinate();
                if (param == null)
                {
                    PlanePoint = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else
                {
                    param.Position.CopyTo(PlanePoint);
                }
                //PlanePoint = param.Position;
                radius = Wafer.GetSubsInfo().ActualDieSize.Width.Value < Wafer.GetSubsInfo().ActualDieSize.Height.Value ?
                    Wafer.GetSubsInfo().ActualDieSize.Width.Value : Wafer.GetSubsInfo().ActualDieSize.Height.Value;
                radius *= 0.8;
                if (Wafer.WaferHeightMapping.PlanPoints != null)
                {
                    for (int i = 0; i < Wafer.WaferHeightMapping.PlanPoints.Count; i++)
                    {
                        double pointdist =
                            Point.Subtract(new Point(Wafer.WaferHeightMapping.PlanPoints[i].X.Value,
                            Wafer.WaferHeightMapping.PlanPoints[i].Y.Value),
                            new Point(PlanePoint.X.Value, PlanePoint.Y.Value)).Length;
                        if (pointdist < radius)
                        {
                            Wafer.WaferHeightMapping.PlanPoints.RemoveAt(i);
                        }
                    }
                }
                if (PlanePoint != null)
                {
                    if (center_standard == true)
                    {
                        TiltedZ = CalcThreePodTiltedPlane(this.WaferAligner().PlanePointCenter.X.Value, this.WaferAligner().PlanePointCenter.Y.Value, true);
                        //PlanePoint.Z.Value = Wafer.GetSubsInfo().ActualThickness;
                    }
                    else
                    {
                        TiltedZ = CalcThreePodTiltedPlane(PlanePoint.GetX(), PlanePoint.GetY(), true);
                    }

                    LoggerManager.Debug($"WaferAligner() - AddHeighPlanePoint() : PlanePoint Z = {PlanePoint.GetZ():0.00} - TiltedPlane Z = {TiltedZ:0.00} for (X,Y) = ({PlanePoint.GetX():0.00},{PlanePoint.GetY():0.00})", isInfo: false);

                    PlanePoint.Z.Value = PlanePoint.Z.Value - TiltedZ;

                    LoggerManager.Debug($"WaferAligner() - AddHeighPlanePoint() : AddHeightPlanePoint Z = {PlanePoint.GetZ():0.00} for (X,Y) = ({PlanePoint.GetX():0.00},{PlanePoint.GetY():0.00})", isInfo: false);

                    Wafer.WaferHeightMapping.PlanPoints.Add(PlanePoint);
                    TotalHeightPoint++;
                    SaveWaferAveThick();

                    LoggerManager.Debug($"AddHeightPlanePoint to WaferAlign. X : {PlanePoint.GetX():0.00}, Y : {PlanePoint.GetY():0.00}, Z : {PlanePoint.GetZ():0.00}", isInfo: false);
                    LoggerManager.Debug($"TotalHeightPoint to WaferAlign : {Wafer.WaferHeightMapping.PlanPoints.Count}", isInfo: false);
                }
                else
                {
                    LoggerManager.Error("AddHeightPlanPoint Failed.");
                }

                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //Edge, CreateBaseHeightProfiling Tilted값 반영하지 않는다.
        public void AddHeighPlanePoint(WaferCoordinate param = null)
        {
            try
            {
                //ICamera preCamera;
                double radius = 0.0;
                //double TiltedZ = 0.0;
                WaferCoordinate PlanePoint = new WaferCoordinate();

                if (param == null)
                {
                    PlanePoint = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else
                {
                    param.CopyTo(PlanePoint);
                }

                radius = Wafer.GetSubsInfo().ActualDieSize.Width.Value < Wafer.GetSubsInfo().ActualDieSize.Height.Value ? Wafer.GetSubsInfo().ActualDieSize.Width.Value : Wafer.GetSubsInfo().ActualDieSize.Height.Value;
                radius *= 0.8;

                if (Wafer.WaferHeightMapping.PlanPoints != null)
                {
                    for (int i = 0; i < Wafer.WaferHeightMapping.PlanPoints.Count; i++)
                    {
                        double pointdist =
                            Point.Subtract(new Point(Wafer.WaferHeightMapping.PlanPoints[i].X.Value,
                            Wafer.WaferHeightMapping.PlanPoints[i].Y.Value),
                            new Point(PlanePoint.X.Value, PlanePoint.Y.Value)).Length;
                        if (pointdist < radius)
                        {
                            Wafer.WaferHeightMapping.PlanPoints.RemoveAt(i);
                        }
                    }
                }

                Wafer.WaferHeightMapping.PlanPoints.Add(PlanePoint);
                TotalHeightPoint++;
                SaveWaferAveThick();

                LoggerManager.Debug($"AddHeightPlanePoint to WaferAlign. X : {PlanePoint.GetX():0.00}, Y : {PlanePoint.GetY():0.00}, Z : {PlanePoint.GetZ():0.00}", isInfo: true);
                LoggerManager.Debug($"TotalHeightPoint to WaferAlign : {Wafer.WaferHeightMapping.PlanPoints.Count}", isInfo: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<WaferCoordinate> GetHieghtPlanePoint()
        {
            return Wafer.WaferHeightMapping.PlanPoints.ToList<WaferCoordinate>();
        }

        public EventCodeEnum GetHeightValueAddZOffsetFromDutIndex(EnumProberCam camtype, long mach_x, long mach_y, double zoffset, out double zpos)
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                long x = 0;
                long y = 0;
                MachineCoordinate mccoord = new MachineCoordinate();
                WaferCoordinate wfcoord = new WaferCoordinate();
                WaferCoordinate wfcoord_next = new WaferCoordinate();
                WaferCoordinate wfcoord_offset = new WaferCoordinate();
                WaferCoordinate wfcoord_LL = new WaferCoordinate();

                // 현재 좌표
                ICamera cam = this.VisionManager().GetCam(camtype);
                MachineIndex mcoord = cam.GetCurCoordMachineIndex();
                x = mcoord.XIndex;
                y = mcoord.YIndex;

                if (camtype == EnumProberCam.WAFER_HIGH_CAM)
                {
                    // 현재 위치
                    wfcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else if (camtype == EnumProberCam.WAFER_LOW_CAM)
                {
                    // 현재 위치
                    wfcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                }

                // 현재 다이의 LL 위치
                wfcoord_LL = MachineIndexConvertToDieLeftCorner((int)x, (int)y);

                if (camtype == EnumProberCam.WAFER_LOW_CAM)
                {
                    wfcoord_LL.X.Value = wfcoord_LL.X.Value + this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value; //다름
                    wfcoord_LL.Y.Value = wfcoord_LL.Y.Value + this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value;//다름
                }

                // 현재 다이에서 LL 까지의 거리
                wfcoord_offset.X.Value = wfcoord.X.Value - wfcoord_LL.X.Value;
                wfcoord_offset.Y.Value = wfcoord.Y.Value - wfcoord_LL.Y.Value;

                // 이동할 위치
                wfcoord_next = MachineIndexConvertToDieLeftCorner((int)mach_x, (int)mach_y);

                if (camtype == EnumProberCam.WAFER_HIGH_CAM)
                {
                    wfcoord_next.X.Value = wfcoord_next.X.Value + wfcoord_offset.X.Value;
                    wfcoord_next.Y.Value = wfcoord_next.Y.Value + wfcoord_offset.Y.Value;
                }
                else if (camtype == EnumProberCam.WAFER_LOW_CAM)
                {
                    wfcoord_next.X.Value = wfcoord_next.X.Value + wfcoord_offset.X.Value + this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value; //다름
                    wfcoord_next.Y.Value = wfcoord_next.Y.Value + wfcoord_offset.Y.Value + this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value; //다름
                }

                wfcoord_next.Z.Value = GetHeightValueAddZOffset(wfcoord_next.X.Value, wfcoord_next.Y.Value, zoffset, true);

                //gehtheightvalue 값이 안전한지
                if (wfcoord_next.Z.Value <= 0)
                {
                    wfcoord_next.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
                }
                else if (wfcoord_next.Z.Value > this.StageSupervisor().WaferMaxThickness)
                {
                    wfcoord_next.Z.Value = this.StageSupervisor().WaferMaxThickness;
                }

                zpos = wfcoord_next.Z.Value;
                LoggerManager.Debug($"[WaferAligner] GetHeightFromDutIndex(), Dut (XIndex, YIndex) = ({mach_x},{mach_y}) Next Dut Pos (X,Y) = ({wfcoord_next.X.Value:0.00},{wfcoord_next.Y.Value:0.00}) Move Z Pos = {zpos:0.00}");
                result = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                zpos = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
                result = EventCodeEnum.EXCEPTION;
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. WaferAligner - GetHeightFromDutIndex() : Error occured.");
            }

            return result;
        }
        private readonly object HeightValuelockObj = new object();

        public double GetHeightValue(double posX, double posY, bool logwrite = false)
        {
            double retVal = 0;

            if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
            {
                retVal = Wafer.GetSubsInfo().ActualThickness;
            }

            try
            {
                lock (HeightValuelockObj)
                {
                    if (Wafer.WaferHeightMapping.PlanPoints != null && (posX != 0 || posY != 0))
                    {
                        if (Wafer.WaferHeightMapping.PlanPoints.Count != 0 && !(Wafer.WaferHeightMapping.PlanPoints.Count < 5))
                        {
                            if (!CheckHeightPosition())
                                return Wafer.GetSubsInfo().ActualThickness;

                            double[] pointD = new double[4];
                            pointD[0] = 0;
                            pointD[1] = 0;
                            pointD[2] = 0;

                            WaferCoordinate[] tmpPoint = new WaferCoordinate[3];

                            tmpPoint[0] = new WaferCoordinate();
                            tmpPoint[1] = new WaferCoordinate();
                            tmpPoint[2] = new WaferCoordinate();

                            double degree1 = 0.0;
                            double degree2 = 0.0;
                            double degree3 = 0.0;

                            int i, j;
                            bool bfound = false;

                            /*                
                            HeightProfile 함수 사용시 주의사항 !!!

                            현재 위치에서 구하고자 하는 높이가 있을 때 사용하는데 만약 현재 위치를 포함하는 3점을 구할 수 없는 경우 즉, 높이를 알고있는 영역 바깥의 높이를
                            알고자 하는 경우에 어설프게 평면의 방정식을 사용해서 높이를 구했다가는 엄청 뻥튀기 된 값이 나올 수 있다.

                            1) 리스트에 존재하는 높이 값들이 모두 가까운 한 점에 모여 있는 경우, 즉 평면의 넓이가 너무 작은 경우
                            2) 평면에서부터 현재 위치까지의 거리가 너무 먼 경우
                            3) 구해진 평면이 정삼각형에 가깝지 않고 매우 얇고 길게 직선에 가까운 모양일 경우 (즉 X나 Y 둘중 하나가 매우 작은 경우)

                            높이가 저장되는 리스트의 각 항목의 X/Y 위치가 일정치 않다고 가정하면 위의 각 경우에 대해서 항상 적합한 적당한 리미트를 설정하는것이 매우 어렵기 때문에
                            여기에서는 현재 위치를 포함하는 평면이 존재하지 않을 경우 평면의 방정식을 사용하지 않고 그냥 가까운 한 점의 높이를 구하여 그 값을 사용한다.

                            따라서 올바르게 높이를 구하기 위해서는 반드시 '항상' 현재 위치를 포함하는 평면이 나와야 한다.
                            그렇기 때문에 이 함수가 불리기 전에 반드시 '항상' 현재 위치를 포함하는 평면이 존재하도록 리스트가 초기화 되어 있어야 한다.

                            값을 초기화 할 때에는 사용 가능한 실제 X/Y 영역보다 훨씬 큰 영역에 대해 널널하게 먼 영역을 지정해 주도록 한다.
                            그래야 실제 포커싱 한 영역 바로 바깥 부분의 높이를 계산할 때 높이 값이 외곽부분에서 급격하게 변하는 것을 막을 수 있다.
                            예를 들어 웨이퍼 얼라인 높이를 설정하는 경우, 실제 사용가능한 넓이(= 반지름 150,000인 원의 넓이)보다 2배 이상 큰 영역을 모두 포함하는 8개의 포인트 높이를
                            지정하여 사용한다.

                            +                        +                          + <-- 가운데 X의 높이를 알게 되는 순간 십자 위치에 각 높이 지정을 미리 해둔다



                                                 ########  <-- 실제 사용 영역
                                                ##########
                                               ###### #####
                            +                 ###### X #####                    +
                                               ###### #####
                                                ##########
                                                 ########



                            +                        +                          +


                            */

                            List<WaferCoordinate> waferCoordinates = Wafer.WaferHeightMapping.PlanPoints.ToList<WaferCoordinate>();

                            // 리스트 내의 점들의 위치를 현재 위치에서 가까운 순으로 정렬한다.
                            waferCoordinates.Sort(delegate (WaferCoordinate wc_ccord1, WaferCoordinate wc_coord2)
                            {
                                if (Distance2D(posX, posY, wc_ccord1.X.Value, wc_ccord1.Y.Value) > Distance2D(posX, posY, wc_coord2.X.Value, wc_coord2.Y.Value)) return 1;
                                if (Distance2D(posX, posY, wc_ccord1.X.Value, wc_ccord1.Y.Value) < Distance2D(posX, posY, wc_coord2.X.Value, wc_coord2.Y.Value)) return -1;
                                return 0;
                            });

                            Wafer.WaferHeightMapping.PlanPoints.Clear();
                            foreach (var point in waferCoordinates)
                            {
                                Wafer.WaferHeightMapping.PlanPoints.Add(point);
                            }

                            // 1. 가장 가까운 점을 고른다
                            tmpPoint[0].X.Value = Wafer.WaferHeightMapping.PlanPoints[0].X.Value;
                            tmpPoint[0].Y.Value = Wafer.WaferHeightMapping.PlanPoints[0].Y.Value;
                            tmpPoint[0].Z.Value = Wafer.WaferHeightMapping.PlanPoints[0].Z.Value;


                            // 2. 현재 위치와 첫 번째 점 사이의 각도를 구한다.
                            degree1 = ((Math.Atan2(tmpPoint[0].Y.Value - posY, tmpPoint[0].X.Value - posX)) * (180 / Math.PI));
                            if (degree1 < 0) degree1 = 360 + degree1;

                            // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                            degree1 = degree1 + 180;
                            if (degree1 >= 360) degree1 = degree1 - 360;

                            for (i = 1; i <= Wafer.WaferHeightMapping.PlanPoints.Count - 1; i++)
                            {
                                if (bfound == true) break;

                                degree2 = (Math.Atan2(Wafer.WaferHeightMapping.PlanPoints[i].Y.Value - posY, Wafer.WaferHeightMapping.PlanPoints[i].X.Value - posX)) * (180 / Math.PI);
                                if (degree2 < 0) degree2 = 360 + degree2;
                                // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                                degree2 = degree2 + 180;
                                if (degree2 >= 360) degree2 = degree2 - 360;

                                // 3. 세 번째의 점을 골라 필요한 영역에 존재하는 지 확인한다.
                                for (j = i + 1; j <= Wafer.WaferHeightMapping.PlanPoints.Count - 1; j++)
                                {
                                    degree3 = (Math.Atan2(Wafer.WaferHeightMapping.PlanPoints[j].Y.Value - posY, Wafer.WaferHeightMapping.PlanPoints[j].X.Value - posX)) * (180 / Math.PI);
                                    if (degree3 < 0) degree3 = 360 + degree3;

                                    // 첫번째 고른 점과 두번째 고른 점의 각도 차이가 180도 이상 발생한다는 뜻은 세번째 점을 고를 때 360도를 넘어서 존재할 수 있다는 뜻이다. 따라서 조건식에 주의해야 한다.
                                    if (Math.Abs(degree2 - degree1) < 180)
                                    {
                                        if (degree2 > degree1)
                                        {
                                            if (degree3 > degree1 && degree3 < degree2)
                                            {
                                                tmpPoint[1].X.Value = Wafer.WaferHeightMapping.PlanPoints[i].X.Value;
                                                tmpPoint[1].Y.Value = Wafer.WaferHeightMapping.PlanPoints[i].Y.Value;
                                                tmpPoint[1].Z.Value = Wafer.WaferHeightMapping.PlanPoints[i].Z.Value;

                                                tmpPoint[2].X.Value = Wafer.WaferHeightMapping.PlanPoints[j].X.Value;
                                                tmpPoint[2].Y.Value = Wafer.WaferHeightMapping.PlanPoints[j].Y.Value;
                                                tmpPoint[2].Z.Value = Wafer.WaferHeightMapping.PlanPoints[j].Z.Value;
                                                bfound = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (degree3 > degree2 && degree3 < degree1)
                                            {
                                                tmpPoint[1].X.Value = Wafer.WaferHeightMapping.PlanPoints[i].X.Value;
                                                tmpPoint[1].Y.Value = Wafer.WaferHeightMapping.PlanPoints[i].Y.Value;
                                                tmpPoint[1].Z.Value = Wafer.WaferHeightMapping.PlanPoints[i].Z.Value;

                                                tmpPoint[2].X.Value = Wafer.WaferHeightMapping.PlanPoints[j].X.Value;
                                                tmpPoint[2].Y.Value = Wafer.WaferHeightMapping.PlanPoints[j].Y.Value;
                                                tmpPoint[2].Z.Value = Wafer.WaferHeightMapping.PlanPoints[j].Z.Value;
                                                bfound = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (degree2 > degree1)
                                        {
                                            if ((degree3 > degree2 && degree3 < 360) || (degree3 < degree1))
                                            {
                                                tmpPoint[1].X.Value = Wafer.WaferHeightMapping.PlanPoints[i].X.Value;
                                                tmpPoint[1].Y.Value = Wafer.WaferHeightMapping.PlanPoints[i].Y.Value;
                                                tmpPoint[1].Z.Value = Wafer.WaferHeightMapping.PlanPoints[i].Z.Value;

                                                tmpPoint[2].X.Value = Wafer.WaferHeightMapping.PlanPoints[j].X.Value;
                                                tmpPoint[2].Y.Value = Wafer.WaferHeightMapping.PlanPoints[j].Y.Value;
                                                tmpPoint[2].Z.Value = Wafer.WaferHeightMapping.PlanPoints[j].Z.Value;
                                                bfound = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if ((degree3 > degree1 && degree3 < 360) || (degree3 < degree2))
                                            {
                                                tmpPoint[1].X.Value = Wafer.WaferHeightMapping.PlanPoints[i].X.Value;
                                                tmpPoint[1].Y.Value = Wafer.WaferHeightMapping.PlanPoints[i].Y.Value;
                                                tmpPoint[1].Z.Value = Wafer.WaferHeightMapping.PlanPoints[i].Z.Value;

                                                tmpPoint[2].X.Value = Wafer.WaferHeightMapping.PlanPoints[j].X.Value;
                                                tmpPoint[2].Y.Value = Wafer.WaferHeightMapping.PlanPoints[j].Y.Value;
                                                tmpPoint[2].Z.Value = Wafer.WaferHeightMapping.PlanPoints[j].Z.Value;
                                                bfound = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (bfound == false)
                            {
                                // 현재 포인트를 포함하는 삼각형이 존재하지 않는다. 그냥 가까운거 한 점의 높이를 리턴한다.
                                // (현재 위치를 포함하는 평면이 존재하지 않으므로 평면의 거리가 멀거나 매우 얇은 평면이거나 할 경우
                                // 높이값이 엄청 뻥튀기 될 위험이 있으므로 평면의 방정식을 돌리지 않고 그냥 가까운 점의 높이를 리턴하여 사용한다)
                                return tmpPoint[0].Z.Value;

                                //tmpPoint[1].X.Value = Wafer.Info.WaferHeightMapping.PlanPoints[1].X.Value;
                                //tmpPoint[1].Y.Value = Wafer.Info.WaferHeightMapping.PlanPoints[1].Y.Value;
                                //tmpPoint[1].Z.Value = Wafer.Info.WaferHeightMapping.PlanPoints[1].Z.Value;

                                //tmpPoint[2].X.Value = Wafer.Info.WaferHeightMapping.PlanPoints[2].X.Value;
                                //tmpPoint[2].Y.Value = Wafer.Info.WaferHeightMapping.PlanPoints[2].Y.Value;
                                //tmpPoint[2].Z.Value = Wafer.Info.WaferHeightMapping.PlanPoints[2].Z.Value;
                            }

                            pointD[0] = tmpPoint[0].Y.Value * (tmpPoint[1].Z.Value - tmpPoint[2].Z.Value) +
                                        tmpPoint[1].Y.Value * (tmpPoint[2].Z.Value - tmpPoint[0].Z.Value) +
                                        tmpPoint[2].Y.Value * (tmpPoint[0].Z.Value - tmpPoint[1].Z.Value);

                            pointD[1] = tmpPoint[0].Z.Value * (tmpPoint[1].X.Value - tmpPoint[2].X.Value) +
                                        tmpPoint[1].Z.Value * (tmpPoint[2].X.Value - tmpPoint[0].X.Value) +
                                        tmpPoint[2].Z.Value * (tmpPoint[0].X.Value - tmpPoint[1].X.Value);

                            pointD[2] = tmpPoint[0].X.Value * (tmpPoint[1].Y.Value - tmpPoint[2].Y.Value) +
                                        tmpPoint[1].X.Value * (tmpPoint[2].Y.Value - tmpPoint[0].Y.Value) +
                                        tmpPoint[2].X.Value * (tmpPoint[0].Y.Value - tmpPoint[1].Y.Value);

                            pointD[3] = -tmpPoint[0].X.Value * ((tmpPoint[1].Y.Value * tmpPoint[2].Z.Value) - (tmpPoint[2].Y.Value * tmpPoint[1].Z.Value)) -
                                         tmpPoint[1].X.Value * ((tmpPoint[2].Y.Value * tmpPoint[0].Z.Value) - (tmpPoint[0].Y.Value * tmpPoint[2].Z.Value)) -
                                         tmpPoint[2].X.Value * ((tmpPoint[0].Y.Value * tmpPoint[1].Z.Value) - (tmpPoint[1].Y.Value * tmpPoint[0].Z.Value));

                            if (pointD[2] != 0)
                            {
                                double ret = -(pointD[0] * posX + pointD[1] * posY + pointD[3]) / pointD[2];
                                if (ret > 1500)
                                {
                                    ret = Wafer.GetSubsInfo().ActualThickness;
                                }

                                double tiltedZ = CalcThreePodTiltedPlane(posX, posY, logwrite);
                                if (logwrite == true)
                                {
                                    LoggerManager.Debug($"WaferAligner() - GetHeightValue() : ProfilingPlane Z = {ret:0.00} + TiltedPlane Z = {tiltedZ:0.00} for (X,Y) = ({posX:0.00},{posY:0.00})");
                                }
                                ret = ret + tiltedZ;
                                if (logwrite == true)
                                {
                                    LoggerManager.Debug($"WaferAligner() - GetHeightValue() : Get Height Z = {ret:0.00} for (X,Y) = ({posX:0.00},{posY:0.00})");
                                }
                                return ret;
                            }
                            else
                            {
                                //Exception
                                return SafeHeightOnException;
                            }
                        }
                    }
                    else
                    {
                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug($"{err.ToString()}. WaferAligner() - GetHeightValue() : Error occured.");

                return SafeHeightOnException;
            }

            return retVal;
        }
        public double GetHeightValueAddZOffset(double posX, double posY, double offsetZ, bool logwrite = false)
        {
            double result = 0;
            try
            {
                double getheight = GetHeightValue(posX, posY, logwrite);
                if (logwrite == true)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}]GetHeightValueAddZOffset(), Move Z Pos = GetHeightValue: {getheight:0.00} + Offset : {offsetZ:0.00}");
                }
                return getheight + offsetZ;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return result;
        }
        private bool CheckHeightPosition()
        {
            bool retVal = false;
            try
            {
                double totaly = 0.0;

                foreach (var position in Wafer.WaferHeightMapping.PlanPoints)
                {
                    totaly += Math.Abs(position.GetY());
                }
                totaly = totaly / Wafer.WaferHeightMapping.PlanPoints.Count;

                List<WaferCoordinate> waferCoordinates = Wafer.WaferHeightMapping.PlanPoints.ToList<WaferCoordinate>();

                waferCoordinates.Sort(delegate (WaferCoordinate wc_ccord1, WaferCoordinate wc_coord2)
                {
                    if (wc_ccord1 != null & wc_coord2 != null)
                    {
                        if (Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_ccord1.X.Value, wc_ccord1.Y.Value)
                            > Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_coord2.X.Value, wc_coord2.Y.Value)) return 1;
                        if (Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_ccord1.X.Value, wc_ccord1.Y.Value)
                            < Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_coord2.X.Value, wc_coord2.Y.Value)) return -1;
                    }
                    return 0;
                });

                if (totaly > (waferCoordinates[0].GetY() + (Wafer.GetSubsInfo().ActualDieSize.Height.Value * 2)))
                    retVal = true;
                //Test Code
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private double Distance2D(double X1, double Y1, double X2, double Y2)
        {
            return Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        }

        public void PlanePointChangetoFocusing5pt(double xpos, double ypos, double zpos)
        {
            try
            {
                int firstindex = 1;
                int lastindex = 8;
                WaferCoordinate closetdistance = new WaferCoordinate();
                int heightprofilingIndex = 0;
                HeightProfilignPosEnum posenum = HeightProfilignPosEnum.UNDEFINED;

                LoggerManager.Debug($"PlanePointChangetoFocusing5pt() : [POINT5,POINT9] Change the value to the Z Value of the PlanePointby focusing the PlanePoint with the Center Z value ");

                double standardplanepointX = xpos;
                double standardplanepointY = ypos;

                for (int j = firstindex; j <= lastindex; j++) //HeightMapping 1~4 중에 가까운 위치 찾기.
                {
                    double compareX = Wafer.WaferHeightMapping.PlanPoints[j].GetX();
                    double compareY = Wafer.WaferHeightMapping.PlanPoints[j].GetY();
                    //가장 가까운 위치를 찾기.
                    double distance = Distance2D(standardplanepointX, standardplanepointY, compareX, compareY);

                    if (j == 1 || distance < Distance2D(standardplanepointX, standardplanepointY, closetdistance.GetX(), closetdistance.GetY()))
                    {
                        closetdistance = Wafer.WaferHeightMapping.PlanPoints[j];
                        heightprofilingIndex = j;
                    }

                }

                if (heightprofilingIndex != 0)
                {
                    //Change
                    LoggerManager.Debug($"PlanePointChangetoFocusing5pt() : (X,Y) : {closetdistance.GetX():0.00},{closetdistance.GetY():0.00} (Z) : {closetdistance.GetZ():0.00} Change to (focusing Z) :{zpos} ");
                    Wafer.WaferHeightMapping.PlanPoints[heightprofilingIndex].Z.Value = zpos;
                }
                else
                {
                    LoggerManager.Debug($"PlanePointChangetoFocusing() : HeightProfiling Pos {posenum}, Height Mapping Focusing  was not changed because HeightMapping Index 1~4 value could not be found.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void PlanePointChangetoFocusing9pt(ObservableCollection<WAHeightPositionParam> heightparam)
        {
            LoggerManager.Debug($"PlanePointChangetoFocusing9pt() : [POINT9] Change the value to the Z Value of the PlanePointby focusing the PlanePoint with the Center Z value ");

            try
            {
                double wafercenterX = PlanePointCenter.GetX();
                double wafercenterY = PlanePointCenter.GetY();

                double distance = 500000;
                double edgepos = 0.0;

                edgepos = ((distance / 2) / Math.Sqrt(2));

                //LEFTUPPER
                var leftupperheight = Wafer.WaferHeightMapping.PlanPoints.Where(height => (height.X.Value == wafercenterX - edgepos && height.Y.Value == wafercenterY + edgepos)).FirstOrDefault();
                if (leftupperheight != null)
                {
                    double leftupperfocusing = heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.LEFTUPPER).HeightProfilingVal;

                    LoggerManager.Debug($"PlanePointChangetoFocusing9pt() : HeightProfiling Pos LEFTUPPER, Z : {leftupperheight.Z.Value:0.00} Change to Z Value =>  {leftupperfocusing:0.00}", isInfo: true);

                    leftupperheight.Z.Value = leftupperfocusing;
                }

                //RIGHTUPPER
                var rightupperheight = Wafer.WaferHeightMapping.PlanPoints.Where(height => (height.X.Value == wafercenterX + edgepos && height.Y.Value == wafercenterY + edgepos)).FirstOrDefault();
                if (rightupperheight != null)
                {
                    double rightupperfocusing = heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.RIGHTUPPER).HeightProfilingVal;

                    LoggerManager.Debug($"PlanePointChangetoFocusing9pt() : HeightProfiling Pos RIGHTUPPER, Z : {rightupperheight.Z.Value:0.00} Change to Z Value =>  {rightupperfocusing:0.00}", isInfo: true);

                    rightupperheight.Z.Value = rightupperfocusing;
                }

                //LEFTBOTTOM
                var leftlowerheight = Wafer.WaferHeightMapping.PlanPoints.Where(height => (height.X.Value == wafercenterX - edgepos && height.Y.Value == wafercenterY - edgepos)).FirstOrDefault();
                if (leftlowerheight != null)
                {
                    double leftlowerfocusing = heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.LEFTBOTTOM).HeightProfilingVal;

                    LoggerManager.Debug($"PlanePointChangetoFocusing9pt() : HeightProfiling Pos LEFTBOTTOM, Z : {leftlowerheight.Z.Value:0.00} Change to Z Value =>  {leftlowerfocusing:0.00}", isInfo: true);

                    leftlowerheight.Z.Value = leftlowerfocusing;
                }

                //RIGHTBOTTOM
                var rightlowerheight = Wafer.WaferHeightMapping.PlanPoints.Where(height => (height.X.Value == wafercenterX + edgepos && height.Y.Value == wafercenterY - edgepos)).FirstOrDefault();
                if (rightlowerheight != null)
                {
                    double rightlowerfocusing = heightparam.Single(hparam => hparam.PosEnum == HeightProfilignPosEnum.RIGHTBOTTOM).HeightProfilingVal;

                    LoggerManager.Debug($"PlanePointChangetoFocusing9pt() : HeightProfiling Pos RIGHTBOTTOM, Z : {rightlowerheight.Z.Value:0.00} Change to Z Value =>  {rightlowerfocusing:0.00}", isInfo: true);

                    leftlowerheight.Z.Value = rightlowerfocusing;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int SaveWaferAveThick()
        {
            int retVal = -1;
            double ave = 0.0;

            try
            {
                if (Wafer.WaferHeightMapping.PlanPoints.Count > 0 && Wafer.WaferHeightMapping.PlanPoints != null)
                {
                    for (int i = 0; i < Wafer.WaferHeightMapping.PlanPoints.Count; i++)
                    {
                        ave += Wafer.WaferHeightMapping.PlanPoints[i].Z.Value;
                    }

                    Wafer.GetSubsInfo().AveWaferThick = ave / Wafer.WaferHeightMapping.PlanPoints.Count;
                    retVal = 0;

                    LoggerManager.Debug($"SaveWaferAveThick(): SaveThickness, PlanPoint Count : {Wafer.WaferHeightMapping.PlanPoints.Count}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }

            return retVal;
        }

        public void ResetHeightPlanePoint()
        {
            try
            {
                TotalHeightPoint = 0;
                Wafer.WaferHeightMapping.HeightPlanPoints.Clear();
                Wafer.WaferHeightMapping.PlanPoints.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region //..Ect Coord, Index

        public Point GetLeftCornerPosition(double positionx, double positiony)
        {
            double PositionX = positionx;
            double PositionY = positiony;
            WaferCoordinate wafercenter;
            WaferCoordinate corner;

            if (Wafer.GetSubsInfo().WaferCenter == null)
                wafercenter = new WaferCoordinate();
            else
                wafercenter = Wafer.GetSubsInfo().WaferCenter;

            corner = Wafer.GetPhysInfo().LowLeftCorner;

            double cornerX = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + Wafer.GetSubsInfo().WaferCenter.GetX() + WaferAlignInfo.RecoveryParam.LCPointOffsetX);
            double cornerY = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + Wafer.GetSubsInfo().WaferCenter.GetY() + WaferAlignInfo.RecoveryParam.LCPointOffsetY);

            double offsetX = 0;
            double offsetY = 0;
            bool isright = false;
            bool istop = false;
            double indexSizeX = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
            double indexSizeY = Wafer.GetSubsInfo().ActualDieSize.Height.Value;

            try
            {
                var result = cornerX < PositionX ? isright = true : isright = false;

                if (!isright) //corner 보다 왼쪽 (-)방향으로 존재.
                {
                    offsetX = (cornerX - PositionX) * -1;

                    if (offsetX == -1)
                        offsetX = 1;

                    int offsetXIndex = Math.Abs(Convert.ToInt32(Math.Truncate(offsetX /= indexSizeX)));
                    indexSizeX = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                    cornerX += ((offsetXIndex + 1) * indexSizeX) * -1;
                }
                else
                {
                    offsetX = (PositionX - cornerX);

                    if (offsetX == -1)
                        offsetX = 1;

                    int offsetXIndex = Math.Abs(Convert.ToInt32(Math.Truncate(offsetX /= indexSizeX)));
                    indexSizeX = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                    cornerX += ((offsetXIndex) * indexSizeX);
                }

                result = cornerY < PositionY ? istop = true : istop = false;

                if (!istop) //corner 보다 아래 (-)방향으로 존재.
                {
                    offsetY = (cornerY - PositionY) * -1;

                    if (offsetY == -1)
                        offsetY = 1;

                    int offsetYIndex = Math.Abs(Convert.ToInt32(Math.Truncate(offsetY /= indexSizeY)));
                    indexSizeY = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
                    cornerY += ((offsetYIndex + 1) * indexSizeY) * -1;
                }
                else
                {
                    offsetY = (PositionY - cornerY);

                    if (offsetY == -1)
                        offsetY = 1;

                    //int offsetYIndex = Math.Abs(Convert.ToInt32(Math.Truncate(
                    //   offsetY /= indexSizeY)));
                    int offsetYIndex = Math.Abs(Convert.ToInt32(Math.Truncate(offsetY /= indexSizeY)));
                    indexSizeY = Wafer.GetSubsInfo().ActualDieSize.Height.Value;
                    cornerY += ((offsetYIndex) * indexSizeY);
                }

                //Squarness
                double sqr = 0.0;
                long yindex = Convert.ToInt64((positiony - Wafer.GetSubsInfo().WaferCenter.GetY()) / Wafer.GetSubsInfo().ActualDieSize.Height.Value);

                //long yindex = WPosToMIndex(new WaferCoordinate(PositionX, PositionY)).XIndex;
                //sqr = (yindex - this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value)
                //           * this.StageSupervisor().WaferObject.GetSubsInfo().WaferSequareness.Value * -1;

                sqr = yindex * this.StageSupervisor().WaferObject.GetSubsInfo().WaferSequareness.Value * -1;

                //sqr = 0;
                //ypos += sqr;
                cornerX -= sqr;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err} PositionFormLeftBottomCorner() : Error occurred.");
            }

            return new Point(cornerX, cornerY);
        }

        public Point GetLeftCornerPosition(WaferCoordinate position)
        {
            return GetLeftCornerPosition(position.GetX(), position.GetY());
        }

        private Point userindexleftcorenr = new Point();

        public Point UserIndexConvertLeftBottomCorner(UserIndex uindex)
        {
            try
            {
                long offsetXIndex = uindex.XIndex - Wafer.GetPhysInfo().CenU.XIndex.Value;
                long offsetYIndex = uindex.YIndex - Wafer.GetPhysInfo().CenU.YIndex.Value;

                userindexleftcorenr.X = Wafer.GetSubsInfo().RefDieLeftCorner.X.Value +
                (offsetXIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value);

                userindexleftcorenr.Y = Wafer.GetSubsInfo().RefDieLeftCorner.Y.Value +
                    (offsetYIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value);

                return userindexleftcorenr;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err} UserIndexConvertLeftBottomCorner() : Error occurred.");
                throw err;
            }
        }

        public Point UserIndexConvertLeftBottomCorner(long xindex, long yindex)
        {
            try
            {
                //MachineIndex mindex = this.WPosToMIndex(new WaferCoordinate(Wafer.Info.RefDieLeftCorner.X.Value + Wafer.Info.WaferCenter.GetX(),
                //    Wafer.Info.RefDieLeftCorner.Y.Value + Wafer.Info.WaferCenter.GetY()));

                long offsetXIndex = xindex - Wafer.GetPhysInfo().CenU.XIndex.Value;
                long offsetYIndex = yindex - Wafer.GetPhysInfo().CenU.YIndex.Value;
                //long offsetXIndex = Convert.ToInt64(xindex) - mindex.XIndex;
                //long offsetYIndex = Convert.ToInt64(yindex) - mindex.YIndex;

                Point pt = GetLeftCornerPosition(
                    Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY());

                //Wafer.Info.RefDieLeftCorner.X.Value = pt.X;
                //Wafer.Info.RefDieLeftCorner.Y.Value = pt.Y;

                //userindexleftcorenr.X = (Wafer.Info.RefDieLeftCorner.X.Value) +
                //     (offsetXIndex * Wafer.Info.ActualDieSize.Width.Value);

                //userindexleftcorenr.Y = (Wafer.Info.RefDieLeftCorner.Y.Value) +
                //    (offsetYIndex * Wafer.Info.ActualDieSize.Height.Value);
                userindexleftcorenr.X = (pt.X) +
                    (offsetXIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value);

                userindexleftcorenr.Y = (pt.Y) +
                    (offsetYIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value);

                return userindexleftcorenr;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{err} UserIndexConvertLeftBottomCorner() : Error occurred.");
                throw err;
            }
        }

        /// <summary>
        /// MahcineIndex의 LeftCorner를 Wafer좌표계로 return 해준다.(내부 공통사용함수)
        /// </summary>
        /// <param name="xindex"></param>
        /// <param name="yindex"></param>
        /// <returns></returns>
        private WaferCoordinate MIndexConverToDieLeftCorner(long xindex, long yindex)
        {

            double xpos = 0.0;
            double ypos = 0.0;
            double tpos = 0.0;
            double zpos = 0.0;
            try
            {
                long offsetXIndex = xindex - Wafer.GetPhysInfo().CenM.XIndex.Value;
                long offsetYIndex = yindex - Wafer.GetPhysInfo().CenM.YIndex.Value;

                Point pt = GetLeftCornerPosition(
                      Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY());

                xpos = (pt.X) +
                    (offsetXIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value);

                ypos = (pt.Y) +
                    (offsetYIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value);

                //Squarness
                double sqr = 0.0;
                sqr = (yindex - this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value)
                           * this.StageSupervisor().WaferObject.GetSubsInfo().WaferSequareness.Value * -1;
                //sqr = 0;
                //ypos += sqr;
                xpos -= sqr;

                //Height Profiling 
                //this.WaferAligner().HeightSearchIndex(xpos, ypos);
                zpos = 1d * this.WaferAligner().GetHeightValue(xpos, ypos, false);

                tpos = (this.WaferAligner().WaferAlignInfo.AlignAngle) * 10000.0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return new WaferCoordinate(xpos, ypos, zpos, tpos);
        }

        /// <summary>
        /// MahcineIndex의 LeftCorner를 Wafer좌표계로 return 해준다.
        /// </summary>
        /// <param name="xindex"></param>
        /// <param name="yindex"></param>
        /// <returns></returns>
        public WaferCoordinate MachineIndexConvertToDieLeftCorner(long xindex, long yindex)
        {
            return MIndexConverToDieLeftCorner(xindex, yindex);
        }

        /// <summary>
        /// MahcineIndex의 LeftCorner를 Wafer좌표계로 return 해준다.
        /// </summary>
        /// <param name="xindex"></param>
        /// <param name="yindex"></param>
        /// <returns></returns>
        public WaferCoordinate MachineIndexConvertToDieLeftCorner_NonCalcZ(long xindex, long yindex)
        {
            double xpos = 0.0;
            double ypos = 0.0;
            double tpos = 0.0;
            double zpos = 0.0;
            try
            {
                long offsetXIndex = xindex - Wafer.GetPhysInfo().CenM.XIndex.Value;
                long offsetYIndex = yindex - Wafer.GetPhysInfo().CenM.YIndex.Value;

                Point pt = GetLeftCornerPosition(
                      Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY());

                xpos = (pt.X) +
                    (offsetXIndex * Wafer.GetSubsInfo().ActualDieSize.Width.Value);

                ypos = (pt.Y) +
                    (offsetYIndex * Wafer.GetSubsInfo().ActualDieSize.Height.Value);

                //Squarness
                double sqr = 0.0;
                sqr = (yindex - this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value)
                           * this.StageSupervisor().WaferObject.GetSubsInfo().WaferSequareness.Value * -1;
                xpos -= sqr;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return new WaferCoordinate(xpos, ypos, zpos, tpos);
        }

        /// <summary>
        /// MahcineIndex의 DieCenter를 Wafer좌표계로 return 해준다.
        /// </summary>
        /// <param name="xindex"></param>
        /// <param name="yindex"></param>
        /// <returns></returns>
        public WaferCoordinate MachineIndexConvertToDieCenter(long xindex, long yindex)
        {
            WaferCoordinate retCoord = null;
            try
            {
                retCoord = MIndexConverToDieLeftCorner(xindex, yindex);
                retCoord.X.Value += (Wafer.GetSubsInfo().ActualDieSize.Width.Value / 2);
                retCoord.Y.Value += (Wafer.GetSubsInfo().ActualDieSize.Height.Value / 2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retCoord;
        }

        /// <summary>
        /// MachineIndex 의 Probing위치를 Wafer좌표계로 리턴해준다. 
        /// </summary>
        /// <param name="xindex"></param>
        /// <param name="yindex"></param>
        /// <param name="IsCenter"></param>
        /// <param name="logWrite"> log 기록 여부</param>
        /// <returns></returns>
        public WaferCoordinate MachineIndexConvertToProbingCoord(long xindex, long yindex, bool logWrite = true)
        {
            long orgxindex = Wafer.GetPhysInfo().CenM.XIndex.Value;
            long orgyindex = Wafer.GetPhysInfo().CenM.YIndex.Value;
            double xpos = 0.0;
            double ypos = 0.0;
            double tpos = 0.0;
            double zpos = 0.0;
            bool isup;
            bool isright;

            var result = orgyindex < yindex ? isup = true : isup = false;

            //  pad cen 고려 안 됨  (V)
            //  optimize angle 고려 안 됨 (V)
            //  Squareness 고려 안 됨 (V)
            //  Height profile 고려 안 됨 (V)

            if (!isup)
            {
                int indexoffsety = Convert.ToInt32(orgyindex - yindex);

                ypos = Wafer.GetSubsInfo().RefDieLeftCorner.GetY() - (indexoffsety * Wafer.GetSubsInfo().ActualDieSize.Height.Value);
                ypos += this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY;
            }
            else
            {
                int indexoffsety = Convert.ToInt32(yindex - orgyindex);

                ypos = Wafer.GetSubsInfo().RefDieLeftCorner.GetY() + (indexoffsety * Wafer.GetSubsInfo().ActualDieSize.Height.Value);
                ypos += this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY;
            }

            double sqr = 0.0;
            sqr = (yindex - this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value) * this.StageSupervisor().WaferObject.GetSubsInfo().WaferSequareness.Value * -1;

            result = orgxindex < xindex ? isright = true : isright = false;

            if (!isright)
            {
                int indexoffsetx = Convert.ToInt32(xindex - orgxindex);

                xpos = Wafer.GetSubsInfo().RefDieLeftCorner.GetX() + (indexoffsetx * Wafer.GetSubsInfo().ActualDieSize.Width.Value);
                xpos += this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX + sqr;
            }
            else
            {
                int indexoffsetx = Convert.ToInt32(orgxindex - xindex);

                xpos = Wafer.GetSubsInfo().RefDieLeftCorner.GetX() - (indexoffsetx * Wafer.GetSubsInfo().ActualDieSize.Width.Value);
                xpos += this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX + sqr;
            }

            zpos = this.WaferAligner().GetHeightValue(xpos, ypos);

            if (logWrite)
            {
                LoggerManager.Debug($"WaferAligner().MachineIndexConvertToProbingCoord({xpos:0.00},{ypos:0.00}): Target wafer Z pos. = ({zpos:0.00})");
            }

            //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle = 0;

            tpos = (this.WaferAligner().WaferAlignInfo.AlignAngle + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle) * 10000.0;

            if (logWrite)
            {
                LoggerManager.Debug($"WaferAligner(): Wafer Aligne Angle = {this.WaferAligner().WaferAlignInfo.AlignAngle:0.0000}, Probe card angle = {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle:0.0000}", isInfo: true);
            }

            var rotPos = this.CoordinateManager().GetRotatedPoint(new MachineCoordinate(xpos, ypos), new MachineCoordinate(0, 0), this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle);

            if (logWrite)
            {
                LoggerManager.Debug($"WaferAligner().MachineIndexConvertToProbingCoord({xindex}, {yindex}): Target wafer pos. = ({xpos:0.00},{ypos:0.00})", isInfo: true);
                LoggerManager.Debug($"WaferAligner().MachineIndexConvertToProbingCoord({xindex}, {yindex}): Card Compensated pos. = ({rotPos.GetX():0.00},{rotPos.GetY():0.00})", isInfo: true);
            }

            return new WaferCoordinate(rotPos.GetX(), rotPos.GetY(), zpos, tpos);
        }

        public MachineIndex WPosToMIndex(WaferCoordinate coordinate)
        {
            try
            {
                Point pt = GetLeftCornerPosition(coordinate.GetX(), coordinate.GetY());
                Point refideleftcorner = GetLeftCornerPosition(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY());

                double offsetrefdiex = (pt.X - refideleftcorner.X) / Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                double offsetrefdiey = (pt.Y - refideleftcorner.Y) / Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                long indexX = Convert.ToInt64(Wafer.GetPhysInfo().CenM.XIndex.Value + offsetrefdiex);
                long indexY = Convert.ToInt64(Wafer.GetPhysInfo().CenM.YIndex.Value + offsetrefdiey);

                return new MachineIndex(indexX, indexY);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }
        #endregion

        public void InitIdelState()
        {
            try
            {
                InnerState = new WaferAlignIdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public double GetReviseSquarness(double xpos, double ypos)
        {
            double offsetxpos = 0.0;
            try
            {
                double squarness = Wafer.GetSubsInfo().WaferSequareness.Value;
                if (squarness != 0)
                {
                    double length = Math.Sqrt(Math.Pow(xpos - Wafer.GetSubsInfo().WaferCenter.GetX(), 2.0) +
                        Math.Pow(ypos - Wafer.GetSubsInfo().WaferCenter.GetY(), 2.0));
                    offsetxpos = length * Math.Sin(squarness);

                }
                else
                {
                    offsetxpos = 0;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return offsetxpos;
        }

        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                List<ISubModule> modules = Template.GetProcessingModule();
                foreach (var subModule in modules)
                {
                    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum DoManualOperation(IProbeCommandParameter param = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.IsManualTriggered = true;
                ManualOPResult = EventCodeEnum.UNDEFINED;
                IsManualOPFinished = false;

                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    if (this.StageSupervisor().WaferObject.GetStatus() != EnumSubsStatus.EXIST)
                    {
                        retval = EventCodeEnum.WAFER_NOT_EXIST_EROOR;
                        return retval;
                    }
                }

                string msg = null;

                bool isReady = IsManualOPReady(out msg);
                if (isReady == true)
                {
                    retval = ClearState();
                }

                if (isReady == false || retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], DoManualOperation(): ClearState = {retval}, msg = {msg}");
                }

                if (retval == EventCodeEnum.NONE && isReady)
                {
                    bool isInjected = false;

                    isInjected = this.CommandManager().SetCommand<IDoManualWaferAlign>(this, param);

                    if (isInjected == true)
                    {
                        while (true)
                        {
                            if (IsManualOPFinished == true)
                            {
                                retval = this.ManualOPResult;
                                break;
                            }

                            Thread.Sleep(30);
                        }

                        if (retval == EventCodeEnum.NONE)
                        {
                            if (GetIsModify() == true)
                            {
                                SetIsModify(false);
                            }
                        }
                        else
                        {
                            WaferAlignFailProcForStatusSoaking();
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.WAFERALIGN_MANUALOPERATION_COMMAND_REJECTED;

                        LoggerManager.Debug($"[{this.GetType().Name}], DoManualOperation(): retval = {retval}");
                    }
                }
                else
                {
                    if (isReady == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Manual Wafer Align", $"Wafer alignment cannot be performed.\r\n The reason is {msg}", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.IsManualTriggered = false;
            }

            return retval;
        }

        private int WaferRetryCnt = 0;
        public EventCodeEnum DoWaferAlignProcess()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    LoggerManager.ActionLog(ModuleLogType.WAFER_ALIGN, StateLogType.START,
                        $"Lot ID: {this.LotOPModule().LotInfo.LotName.Value}, Device:{this.FileManager().GetDeviceName()}, " +
                        $"Wafer ID: {this.GetParam_Wafer().GetSubsInfo().WaferID.Value}",
                        this.LoaderController().GetChuckIndex());
                }
                else
                {
                    LoggerManager.ActionLog(ModuleLogType.WAFER_ALIGN, StateLogType.START,
                        $"Device:{this.FileManager().GetDeviceName()}, " +
                        $"Wafer ID: {this.GetParam_Wafer().GetSubsInfo().WaferID.Value}",
                        this.LoaderController().GetChuckIndex());
                }

                if (this.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    if (this.WaferAlignInfo.LotLoadingPosCheckMode)
                    {
                        WaferCoordinate coordinate = new WaferCoordinate();
                        double maximum_value_X = 0.0;
                        double maximum_value_Y = 0.0;
                        retVal = this.EdgeCheck(ref coordinate, ref maximum_value_X, ref maximum_value_Y);
                    }

                    this.UpdatePadCen();
                    this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);

                    this.InnerStateTransition(new WaferAlignDoneState(this));

                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

                bool isCurTempWithinSetTempRange = this.TempController().IsCurTempWithinSetTempRange();

                if (isCurTempWithinSetTempRange)
                {
                    //..[Trun Theta 0 ] 
                    double curtpos = 0.0;
                    ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                    curtpos = Math.Abs(curtpos);

                    int converttpos = Convert.ToInt32(curtpos);

                    //if (converttpos != 0)
                    //{
                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Wafer.GetSubsInfo().ActualThickness);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Move Error", WaferAlignState.GetType().Name)));

                            return retVal;
                        }

                        retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);

                        if (retVal != EventCodeEnum.NONE)
                        {
                            InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Move Error", WaferAlignState.GetType().Name)));

                            return retVal;
                        }

                        this.WaferAligner().WaferAlignInfo.AlignAngle = 0;

                        if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                        {
                            retVal = this.MarkAligner().DoMarkAlign();

                            if (retVal != EventCodeEnum.NONE)
                            {
                                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                                {
                                    InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Move Error", WaferAlignState.GetType().Name)));
                                }

                                return retVal;
                            }
                        }
                    //}

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(MarkAlignmentDoneBeforeWaferAlignment).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();

                    bool doneFlag = false;
                    DateTime startTime = DateTime.Now;

                    List<ISubModule> modules = Template.GetProcessingModule();

                    this.GetParam_Wafer().SetAlignState(AlignStateEnum.IDLE);

                    for (int index = 0; index < modules.Count; index++)
                    {
                        retVal = modules[index].ClearData();
                    }

                    for (int index = 0; index < modules.Count; index++)
                    {
                        ISubModule module = modules[index];

                        retVal = module.Execute();

                        if (module.GetState() == SubModuleStateEnum.RECOVERY)
                        {
                            if (this.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                            {
                                UpdatePadCen();
                                this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);
                                InnerStateTransition(new WaferAlignDoneState(this));
                                doneFlag = true;

                                return EventCodeEnum.NONE;
                            }

                            //바로 전단계 이전 단계에 Skip 이 있는지 확인
                            bool skipflag = false;
                            int skipindex = -1;

                            for (int preindex = 0; preindex < index; preindex++)
                            {
                                if (modules[preindex].GetState() == SubModuleStateEnum.SKIP)
                                {
                                    if (preindex != index - 1)
                                    {
                                        skipflag = true;
                                        skipindex = preindex;
                                    }
                                }
                            }

                            if (skipflag)
                            {
                                for (int preindex = skipindex; preindex < index; preindex++)
                                {
                                    ISubModule premodule = modules[preindex];
                                    premodule.Execute();
                                }

                                module.Execute();
                            }
                            else
                            {
                                if (index != 0)
                                {
                                    if (modules[index - 1].GetState() == SubModuleStateEnum.DONE)
                                    {
                                        modules[index - 1].Execute();
                                        modules[index].Execute();
                                    }
                                    else if (modules[index - 1].GetState() == SubModuleStateEnum.SKIP)
                                    {
                                        modules[index - 1].Execute();
                                        modules[index].Execute();

                                        if (modules[index].GetState() != SubModuleStateEnum.DONE)
                                        {
                                            if (modules[index - 1].GetState() == SubModuleStateEnum.DONE)
                                            {
                                                retVal = modules[index - 1].Execute();

                                                if (retVal == EventCodeEnum.NONE)
                                                {
                                                    retVal = modules[index].Execute();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (module.GetState() == SubModuleStateEnum.RECOVERY)
                        {
                            if (retVal == EventCodeEnum.SUB_RECOVERY || retVal != EventCodeEnum.NONE)
                            {
                                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT ||
                                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                                {
                                    semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(WaferAlignFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    if (WaferAlignSysParam.EnableSkipWafer_WhenWaferAlignFailed.Value)
                                    {
                                        // Skip Enable이 켜져 있으므로 웨이퍼의 상태를 SKIP으로 바꾸고 WaferAlignModule은 Idle로 전환한다.
                                        this.GetParam_Wafer().SetWaferState(EnumWaferState.SKIPPED);
                                        InnerStateTransition(new WaferAlignIdleState(this));
                                    }
                                    else
                                    {
                                        // 꼭 Recovery로 가야할까? 어느 State로 끝나야 다시 Trigger 되지 않을까? 
                                        // WaferObject.AlignState를 보고 있을 것 같은데 그걸 DONE으로 만들수 없으니 
                                        InnerStateTransition(new WaferAlignRecoveryState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, retVal.ToString(), WaferAlignState.GetType().Name)));
                                    }
                                    
                                }

                                this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.FAIL);

                                this.StageSupervisor().StageModuleState.ZCLEARED();
                                doneFlag = false;
                            }

                            break;
                        }
                        else if (module.GetState() == SubModuleStateEnum.ERROR)
                        {
                            if (ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                            {

                                UpdatePadCen();
                                this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);

                                InnerStateTransition(new WaferAlignDoneState(this));

                                doneFlag = true;

                                LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={this.InnerState.GetModuleState()}");

                                return EventCodeEnum.NONE;
                            }

                            this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.FAIL);

                            this.StageSupervisor().StageModuleState.ZCLEARED();


                            if (WaferAlignSysParam.EnableSkipWafer_WhenWaferAlignFailed.Value)
                            {
                                // Skip Enable이 켜져 있으므로 웨이퍼의 상태를 SKIP으로 바꾸고 WaferAlignModule은 Idle로 전환한다.
                                this.GetParam_Wafer().SetWaferState(EnumWaferState.SKIPPED);
                                InnerStateTransition(new WaferAlignIdleState(this));
                            }
                            else
                            {
                                EventCodeInfo eventCodeInfo = this.WaferAligner().ReasonOfError.GetLastEventCode();
                                if (eventCodeInfo != null && eventCodeInfo.EventCode != EventCodeEnum.UNDEFINED)
                                {
                                    InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, eventCodeInfo.EventCode, eventCodeInfo.Message, WaferAlignState.GetType().Name)));
                                }
                                else
                                {
                                    InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, retVal.ToString(), WaferAlignState.GetType().Name)));
                                }
                            }
                       

                            doneFlag = false;

                            break;
                        }
                        else
                        {
                            doneFlag = true;
                            this.SoakingModule().Idle_SoakingFailed_WaferAlign = false;
                        }

                        //if (token != null)
                        //{
                        //    if (token.IsCancellationRequested == true)
                        //    {
                        //        // TODO : 로직 확인, 정리해야되는 것이 있다면 하고...
                        //        token.Dispose();
                        //        token = null;

                        //        return EventCodeEnum.WAFERALIGN_USER_CANCEL;
                        //    }
                        //}
                    }
                    if (doneFlag)
                    {
                        UpdatePadCen();
                        bool triggermark = this.MarkAligner().GetTriggerMarkVerificationAfterWaferAlign();
                        if (WaferAlignSysParam == null || triggermark == true)
                        {
                            // Check Mark Align Processing.
                            retVal = this.MarkAligner().DoMarkAlign(true);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                retVal = this.MarkAligner().DoMarkAlign(true);
                            }

                            if (retVal == EventCodeEnum.NONE)
                            {
                                var markTolerance = this.MarkAligner().GetMarkDiffToleranceOfWA();
                                double toleranceX = markTolerance.Item1;
                                double toleranceY = markTolerance.Item2;

                                if (Math.Abs(toleranceX) < Math.Abs(this.MarkAligner().DiffMarkPosX))
                                {
                                    LoggerManager.Debug($"[Wafer Align] MarkDiffTolerance_X : {toleranceX} , Mark Diff PosX:{this.MarkAligner().DiffMarkPosX}, ManualAlignment:{this.IsManualTriggered.ToString()}", isInfo: true);

                                    this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);

                                    WaferRetryCnt++;

                                    if (WaferRetryCnt > 1 || this.IsManualTriggered)
                                    {
                                        InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, $"Wafer Align Retry Fail. \n MarkDiffTolerance_X : {toleranceX} , Mark Diff PosX:{this.MarkAligner().DiffMarkPosX}", WaferAlignState.GetType().Name)));

                                        WaferRetryCnt = 0;
                                    }

                                    if (this.IsManualTriggered)
                                    {
                                        ManuallAlignmentErrTxt = $"Wafer Align Fail\n MarkDiffTolerance_X : {toleranceX} , Mark Diff PosX:{this.MarkAligner().DiffMarkPosX}";
                                        retVal = EventCodeEnum.WAFER_ALIGN_FAIL_MARK_DIFF_TOLERANCE;
                                    }
                                }
                                else if (Math.Abs(toleranceY) < Math.Abs(this.MarkAligner().DiffMarkPosY))
                                {
                                    LoggerManager.Debug($"[Wafer Align] MarkDiffTolerance_Y : {toleranceY} , Mark Diff PosY:{this.MarkAligner().DiffMarkPosY}, ManualAlignment:{this.IsManualTriggered.ToString()}", isInfo: true);

                                    this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);

                                    WaferRetryCnt++;

                                    if (WaferRetryCnt > 1 || this.IsManualTriggered)
                                    {
                                        InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, $"Wafer Align Retry Fail. \n MarkDiffTolerance_Y : {toleranceY} , Mark Diff PosY:{this.MarkAligner().DiffMarkPosY}", WaferAlignState.GetType().Name)));

                                        WaferRetryCnt = 0;
                                    }

                                    if (this.IsManualTriggered)
                                    {
                                        ManuallAlignmentErrTxt = $"Wafer Align Fail.MarkDiffTolerance_Y : {toleranceY} , Mark Diff PosY:{this.MarkAligner().DiffMarkPosY}";
                                        retVal = EventCodeEnum.WAFER_ALIGN_FAIL_MARK_DIFF_TOLERANCE;
                                    }

                                    if (this.IsManualTriggered)
                                    {
                                        ManuallAlignmentErrTxt = $"Wafer Align Fail.MarkDiffTolerance_Y : {this.MarkAligner().GetMarkDiffTolerance_Y()} , Mark Diff PosY:{this.MarkAligner().DiffMarkPosY}";
                                        retVal = EventCodeEnum.WAFER_ALIGN_FAIL_MARK_DIFF_TOLERANCE;
                                    }
                                }
                                else
                                {
                                    // wafer thickness tolerance check                                 
                                    double diff = Math.Round(Math.Abs(this.Wafer.GetPhysInfo().Thickness.Value - this.Wafer.GetSubsInfo().AveWaferThick), 1);
                                    if (diff >= this.Wafer.GetPhysInfo().ThicknessTolerance.Value)
                                    {
                                        // tolerance 초과                    
                                        retVal = EventCodeEnum.WAFERTHICKNESS_VALIDATION_FAILED;
                                        LoggerManager.Debug($"VerifyWaferThicknessTolerance() Diff wafer thickness value : {diff} , Current wafer thickness tolerance value : {this.Wafer.GetPhysInfo().ThicknessTolerance.Value}");
                                    }
                                    else
                                    {
                                        // 정상
                                        retVal = EventCodeEnum.NONE;
                                    }

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        ManuallAlignmentErrTxt = $"Wafer Thickness Out of Tolerance.\nPlease check Wafer Thickness Tolerance parameter and Wafer Thickness parameter.\nDiff wafer thickness value : {diff} \nCurrent wafer thickness tolerance value : {this.Wafer.GetPhysInfo().ThicknessTolerance.Value}";
                                        InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, ManuallAlignmentErrTxt, WaferAlignState.GetType().Name)));
                                    }
                                    else
                                    {
                                        this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);
                                        CheckPadposition();
                                        this.StageSupervisor().StageModuleState.ZCLEARED();

                                        InnerStateTransition(new WaferAlignDoneState(this));

                                        WaferRetryCnt = 0;
                                    }
                                }

                                semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(MarkAlignmentDoneAfterWaferAlignment).FullName, new ProbeEventArgs(this, semaphore));
                                semaphore.Wait();
                            }
                            else
                            {
                                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || this.IsManualTriggered)
                                {
                                    InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, retVal.ToString(), WaferAlignState.GetType().Name)));
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Trigger Mark Verification After Wafer Alignment = {triggermark}, Skip Mark Alignment.");
                            retVal = EventCodeEnum.NONE;
                            this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.DONE);
                            CheckPadposition();
                            this.StageSupervisor().StageModuleState.ZCLEARED();
                            InnerStateTransition(new WaferAlignDoneState(this));
                            WaferRetryCnt = 0;
                        }
                    }

                    DateTime endTime = DateTime.Now;

                    LoggerManager.Debug($"Wafer Angle : {this.WaferAligner().WaferAlignInfo.AlignAngle}", isInfo: true);
                    LoggerManager.Debug($"WaferAlign Operation Time: [{(double)(endTime - startTime).Ticks / 1000000.0F}]", isInfo: true);
                }
                else
                {
                    retVal = EventCodeEnum.WAFERALIGN_TEMP_DEVIATION_OUTOFRANGE;
                    if (IsManualTriggered)
                    {
                        InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Temp Error", WaferAlignState.GetType().Name)));
                    }
                    else
                    {
                        if(this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                        {
                            //afer align 결과는 WAFERALIGN_TEMP_DEVIATION_OUTOFRANGE로 soaking은 abort가 된 상황임
                            //waferalign state를 error로 할경우 loader에 쓸데없이 notiy alarm이 가게 되므로 바로 IDLE로 전환하도록 한다.
                            InnerStateTransition(new WaferAlignIdleState(this));
                        }
                        else
                        {
                            InnerStateTransition(new WaferAlignSuspendedState(this));
                        }                        
                    }

                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                // WaferAlign Processing 중 exception이 발생한 경우 Throw를 상위로 계속 던저 주며 해당 catch부분 까지 넘어옴.
                // ZCleared()를 불러주는 이유 : OPUSVStageMove State가 WAFERHIGH나 WAFERLOW로 남아 있는 상태에서 lowviewmove나, highviewmove가 불릴 경우 카메라 베이스가 나가지 않음.
                // Resume 했을 경우 OPUSVStageMove State가 ZCLEARED state고 카메라가 나갈 수 있음.
                this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.FAIL);
                this.StageSupervisor().StageModuleState.ZCLEARED();
                InnerStateTransition(new WaferAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, err.ToString(), WaferAlignState.GetType().Name)));

                LoggerManager.Exception(err);
            }
            finally
            {
                LoggerManager.Debug($"DoWaferAlignProcess(): End.");
                this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                this.IsManualOPFinished = true;
            }

            return retVal;
        }
        /// <summary>
        /// wafer align중 실패가 발생 시 soaking module쪽을 정리할 수 있도록 처리하는 함수.
        /// </summary>
        public void WaferAlignFailProcForStatusSoaking()
        {
            bool ManualSoakingWorking = false;

            if (this.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && this.SoakingModule().ManualSoakingStart)
            {
                ManualSoakingWorking = true;
            }

            bool EnableStatusSoaking = false;
            bool IsGettingOptionSuccessul = this.SoakingModule().StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);

            if ((IsGettingOptionSuccessul && EnableStatusSoaking) || ManualSoakingWorking)
            {
                if (this.SoakingModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED &&
                    (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE) // lot op가 running일때는 lot pause가 되면 soaking module이 pause처리됨, 하지만 Lot가 Idle에서는 처리가 없으므로 별도 abort 처리함.
                    ) //soaking module이 align을 대기하기 위해 suspend 상태일때
                {
                    LoggerManager.SoakingErrLog($"Soaking Abort. because wafer align is failed");

                    //manual soaking중 실패난경우 wafer module이 error로 있지 않도록 한다.
                    SoakingStateEnum state = this.SoakingModule().GetStatusSoakingState();

                    if (SoakingStateEnum.MANUAL == state)
                    {
                        LoggerManager.SoakingErrLog("Do ClearState(ManualSoaking) - WaferAligner is error");
                        this.ClearState();
                    }

                    this.SoakingModule().Idle_SoakingFailed_WaferAlign = true; // idle soaking 중 pin align이 실패난 경우 해당 flag를 통해 지속적으로 idle상태에서 soaking을 통한 pin align을 하지 않도록 처리
                    this.SoakingModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.SOAKING_ERROR_IDLE_WAFERALIGN, "Wafer Align failed.", this.GetType().Name);
                    this.NotifyManager().Notify(EventCodeEnum.SOAKING_ERROR_IDLE_WAFERALIGN, "Can not start Idle soaking");
                    this.SoakingModule().Abort();
                }
            }
        }

        public void CheckPadposition()
        {
            try
            {
                if (this.IsOnDubugMode)
                {
                    string pathbase = this.WaferAligner().IsOnDebugPadPathBase;
                    var padsInfo = new List<DUTPadObject>(Wafer.GetSubsInfo().Pads.DutPadInfos);
                    if (padsInfo.Count != 0)
                    {
                        for (int index = 0; index < padsInfo.Count; index++)
                        {
                            WaferCoordinate wcoord = null;
                            wcoord = this.MachineIndexConvertToDieLeftCorner((int)padsInfo[index].MachineIndex.XIndex,
                                (int)padsInfo[index].MachineIndex.YIndex);

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                wcoord.GetX() + padsInfo[index].PadCenter.X.Value,
                                wcoord.GetY() + padsInfo[index].PadCenter.Y.Value,
                                Wafer.GetSubsInfo().ActualThickness);

                            LoggerManager.Debug($"[Wafer Align][Pad] {index + 1}" +
                                $"\nMIndex X : {padsInfo[index].MachineIndex.XIndex}, Y : {padsInfo[index].MachineIndex.YIndex}" +
                                $"\nLeftCorner X : {wcoord.GetX()}, Y : {wcoord.GetY()}" +
                                $"\nPadCenter X : {padsInfo[index].PadCenter.X.Value}, Y : {padsInfo[index].PadCenter.Y.Value}");

                            string NowTime = DateTime.Now.ToString("yyMMddHHmmss");
                            string path = $"{pathbase}[Pad#{index + 1}]_{NowTime}.bmp";
                            this.VisionManager().SaveImageBuffer(this.VisionManager().SingleGrab(EnumProberCam.WAFER_HIGH_CAM, this), path, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                            LoggerManager.Debug($"[Wafer Align][Pad] {index + 1} save to {path}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdatePadCen()
        {
            try
            {
                //double LLCofFirstDut_X = 0.0;
                //double LLCofFirstDut_Y = 0.0;
                //double dist_x = 0.0;
                //double dist_y = 0.0;
                double sumx = 0.0;
                double sumy = 0.0;

                if (this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count <= 0)
                {
                    LoggerManager.Debug($"Pad data is not registered yet, clear pad center...");

                    this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX = 0;
                    this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY = 0;

                    return;
                }

                //int dut_array = 0;
                //int pin_array = 0;
                //int cur_padNum = 0;

                IDut firstDut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0];

                // TODO: FLIP 검토 할것
                //if (Wafer.VisionManager().DispHorFlip == DispFlipEnum.FLIP && Wafer.VisionManager().DispVerFlip == DispFlipEnum.FLIP)
                //{
                //    firstDut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault(item =>
                //        item.MacIndex.XIndex == Wafer.GetPhysInfo().MapCountX.Value - 1 - firstDut.MacIndex.XIndex
                //        && item.MacIndex.YIndex == Wafer.GetPhysInfo().MapCountY.Value - 1 - firstDut.MacIndex.YIndex);
                //}

                // Pad Center 계산
                foreach (var PadList in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos)
                {
                    //sumx += PadList.MIndexLCWaferCoord.X.Value + PadList.PadCenter.X.Value;
                    //sumy += PadList.MIndexLCWaferCoord.Y.Value + PadList.PadCenter.Y.Value;

                    //this.StageSupervisor().ProbeCardInfo.GetArrayIndex(cur_padNum, out dut_array, out pin_array);

                    if (PadList.DutNumber != 0)// 임시방편
                    {
                        // 1번 더트의 LL위치에서 현재 패드까지의 상대거리
                        sumx += ((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[(int)PadList.DutNumber - 1].MacIndex.XIndex - firstDut.MacIndex.XIndex)
                                 * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value) + PadList.PadCenter.X.Value;
                        sumy += ((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[(int)PadList.DutNumber - 1].MacIndex.YIndex - firstDut.MacIndex.YIndex)
                                 * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value) + PadList.PadCenter.Y.Value;
                    }

                    //// 티치 더트의 LL위치에서 현재 패드까지의 상대거리
                    //sumx += ((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[(int)PadList.DutNumber - 1].MacIndex.XIndex - this.Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex)
                    //         * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value) + PadList.PadCenter.X.Value;
                    //sumy += ((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[(int)PadList.DutNumber - 1].MacIndex.YIndex - this.Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex)
                    //         * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value) + PadList.PadCenter.Y.Value;

                    //if ((PadList.DutNumber <= this.StageSupervisor().ProbeCardInfo.DutList.Count) && PadList.DutNumber >= 1)
                    //{
                    //    // 현재 패드가 속한 다이에서 1번 DUT까지의 거리
                    //    dist_x = this.StageSupervisor().ProbeCardInfo.DutList[(int)PadList.DutNumber - 1].MacIndex.XIndex 
                    //              * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    //    dist_y = this.StageSupervisor().ProbeCardInfo.DutList[(int)PadList.DutNumber - 1].MacIndex.YIndex
                    //              * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                    //    sumx = sumx + dist_x + PadList.PadCenter.X.Value;
                    //    sumy = sumy + dist_y + PadList.PadCenter.Y.Value;
                    //}
                    //else
                    //{
                    //    LoggerManager.Debug($"Error happend in CalcPadCen() while calculate pad centger : Dut information does not match with pad data. in Pad = {PadList.DutNumber}, Dut count = {this.StageSupervisor().ProbeCardInfo.DutList.Count}");
                    //    return;
                    //}                    
                }

                this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX = sumx / this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count;
                this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY = sumy / this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                CommandRecvSlot.ClearToken();
                LoggerManager.Debug($"Current pad cen = ({this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX}, {this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY})", isInfo: true);
            }
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool retVal = false;
            msg = null;

            try
            {
                if (this.Template.GetProcessingModule() == null)
                {
                    msg = Properties.Resources.CheckTemplateErrorMessage;

                    return retVal;
                }

                if (this.GetParam_Wafer().WaferAlignSetupChangedToggle.DoneState != ElementStateEnum.NEEDSETUP &&
                    this.GetParam_Wafer().WaferAlignSetupChangedToggle.DoneState != ElementStateEnum.NEEDSETUP)
                {
                    foreach (var module in this.Template.GetProcessingModule())
                    {
                        if (module is ILotReadyAble)
                        {
                            retVal = (module as ILotReadyAble).IsLotReady(out msg);

                            if (!retVal)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (this.GetParam_Wafer().WaferAlignSetupChangedToggle.DoneState != ElementStateEnum.NEEDSETUP)
                    {
                        msg = Properties.Resources.WaferAlignSetupErrorMessage;
                    }
                    else
                    {
                        msg = Properties.Resources.PadSetupErrorMessage;
                    }

                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public bool IsManualOPReady(out string msg)
        {
            bool retval = false;
            msg = string.Empty;

            try
            {
                // 조건이 바뀌게 되면 별도 로직 구성할 것.
                retval = IsLotReady(out msg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void CompleteManualOperation(EventCodeEnum retval)
        {
            try
            {
                if (IsManualTriggered)
                {
                    this.ManualOPResult = retval;
                }

                //this.IsManualOPFinished = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum SetTeachDevice(bool isMoving = true, long xindex = -1, long yindex = -1, EnumProberCam enumProberCam = EnumProberCam.WAFER_HIGH_CAM)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<IDut> duts = new List<IDut>();

                long xMinIndex = 0;
                long xMaxIndex = 0;
                long yMinIndex = 0;
                long yMaxIndex = 0;

                int dutWidthCount = 0;
                int dutHeightCount = 0;

                foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    duts.Add(dut);
                }

                xMinIndex = duts.Min(dut => dut.MacIndex.XIndex);
                xMaxIndex = duts.Max(dut => dut.MacIndex.XIndex);
                yMinIndex = duts.Min(dut => dut.MacIndex.YIndex);
                yMaxIndex = duts.Max(dut => dut.MacIndex.YIndex);

                long sdutxindex = duts.Find(dut => dut.DutNumber == 1).MacIndex.XIndex;
                long sdutyindex = duts.Find(dut => dut.DutNumber == 1).MacIndex.YIndex;

                xMinIndex = Math.Abs(sdutxindex - xMinIndex);
                xMaxIndex = Math.Abs(sdutxindex - xMaxIndex);
                yMinIndex = Math.Abs(sdutyindex - yMinIndex);
                yMaxIndex = Math.Abs(sdutyindex - yMaxIndex);

                IWaferObject WaferObject = this.StageSupervisor().WaferObject;
                IPhysicalInfo physicalInfo = WaferObject.GetPhysInfo();

                List<IDeviceObject> devs = Wafer.GetDevices();

                long dieXcount = physicalInfo.MapCountX.Value;
                long dieycount = physicalInfo.MapCountY.Value;

                long hortestdiecount = 0;
                long vertestdiecount = 0;

                long _TeachDieXIndex = physicalInfo.TeachDieMIndex.Value.XIndex;
                long _TeachDieYIndex = physicalInfo.TeachDieMIndex.Value.YIndex;

                byte[,] wafermap = WaferObject.DevicesConvertByteArray();

                long vermarkcount = 0;
                long hormarkcount = 0;

                for (int index = 0; index < wafermap.GetUpperBound(1) + 1; index++)
                {
                    DieTypeEnum dietype = (DieTypeEnum)wafermap[physicalInfo.CenM.XIndex.Value - (dutWidthCount / 2), index];

                    if (dietype == DieTypeEnum.TEST_DIE)
                    {
                        vertestdiecount++;
                    }
                    else
                    {
                        if (index < dieycount / 2)
                        {
                            vermarkcount++;
                        }
                    }
                }

                for (int index = 0; index < wafermap.GetUpperBound(0) + 1; index++)
                {
                    DieTypeEnum dietype = (DieTypeEnum)wafermap[index, physicalInfo.CenM.YIndex.Value - (dutHeightCount / 2)];

                    if (dietype == DieTypeEnum.TEST_DIE)
                    {
                        hortestdiecount++;
                    }
                    else
                    {
                        if (index < dieXcount / 2)
                        {
                            hormarkcount++;
                        }
                    }
                }

                if (hortestdiecount - _TeachDieXIndex < xMaxIndex)
                {
                    long offsetx = (xMaxIndex - xMinIndex + 1) / 2;
                    _TeachDieXIndex = _TeachDieXIndex - offsetx;
                }

                if (_TeachDieXIndex <= xMinIndex)
                {
                    //CenterX 로부터 왼쪽 공간이 부족
                    long offsetx = (xMaxIndex - xMinIndex + 1) / 2;
                    _TeachDieXIndex = _TeachDieXIndex + offsetx;
                }

                if (vertestdiecount - _TeachDieYIndex < yMaxIndex)
                {
                    //CetnerY로 부터 위 공간이 부족
                    long offsety = (yMinIndex - yMaxIndex + 1) / 2;
                    _TeachDieYIndex = _TeachDieYIndex - offsety;
                }

                if (_TeachDieYIndex <= yMinIndex)
                {
                    long offsety = (yMinIndex - yMaxIndex + 1) / 2;
                    _TeachDieYIndex = _TeachDieYIndex + offsety;
                }

                if (xindex == -1)
                {
                    if (physicalInfo.TeachDieMIndex.Value.XIndex == -1)
                    {
                        _TeachDieXIndex = Convert.ToInt64(hortestdiecount / 2) + hormarkcount;
                    }
                    else
                    {
                        _TeachDieXIndex = physicalInfo.TeachDieMIndex.Value.XIndex;
                    }
                }
                else
                {
                    _TeachDieXIndex = xindex;
                }

                if (yindex == -1)
                {
                    if (physicalInfo.TeachDieMIndex.Value.XIndex == -1)
                    {
                        _TeachDieYIndex = Convert.ToInt64(vertestdiecount / 2) + vermarkcount;
                    }
                    else
                    {
                        _TeachDieYIndex = physicalInfo.TeachDieMIndex.Value.YIndex;
                    }
                }
                else
                {
                    _TeachDieYIndex = yindex;
                }

                MachineIndex mCoord = new MachineIndex(_TeachDieXIndex, _TeachDieYIndex);

                var cardinfo = this.GetParam_ProbeCard();

                DutDieIndexs.Clear();

                if (cardinfo != null && cardinfo.ProbeCardDevObjectRef.DutList.Count > 0)
                {
                    IndexCoord retindex;

                    foreach (var dut in cardinfo.ProbeCardDevObjectRef.DutList)
                    {
                        retindex = mCoord.Add(cardinfo.GetRefOffset(dut.DutNumber - 1));

                        DutDieIndexs.Add(new DutWaferIndex(dut.MacIndex, retindex, dut.DutNumber));
                    }
                }

                if (isMoving)
                {
                    Wafer.SetDutDieMatchIndexs(DutDieIndexs);

                    WaferCoordinate coordinate = MIndexConverToDieLeftCorner(_TeachDieXIndex, _TeachDieYIndex);

                    if (enumProberCam == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetSubsInfo().ActualThickness);
                    }
                    else if (enumProberCam == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetSubsInfo().ActualThickness);
                    }
                }

                TeachDieXIndex = _TeachDieXIndex;
                TeachDieYIndex = _TeachDieYIndex;

                physicalInfo.TeachDieMIndex.Value.XIndex = TeachDieXIndex;
                physicalInfo.TeachDieMIndex.Value.YIndex = TeachDieYIndex;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void ExitRecovery()
        {
            List<ISubModule> moduls = Template.GetProcessingModule();

            foreach (var module in moduls)
            {
                if (module is IRecovery)
                {
                    EventCodeEnum retVal = module.ExitRecovery();
                    if (retVal != EventCodeEnum.NONE)
                        break;
                }
            }
        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is WaferUnloadedEvent || sender is DeviceChangedEvent)
                {
                    ClearRecoveryData();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ClearRecoveryData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.WaferAlignInfo.RecoveryParam.LCPointOffsetX = 0;
                this.WaferAlignInfo.RecoveryParam.LCPointOffsetY = 0;
                this.WaferAlignInfo.RecoveryParam.OrgLCPoint = new Point();
                this.WaferAlignInfo.RecoveryParam.OrgRefPadPoint = new Point();
                this.WaferAlignInfo.RecoveryParam.RefPadOffsetX = 0;
                this.WaferAlignInfo.RecoveryParam.RefPadOffsetY = 0;

                var procmodule = Template.GetProcessingModule();
                IWaferEdgeProcModule edgemodule = (IWaferEdgeProcModule)procmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferEdgeProcModule))).FirstOrDefault();
                if (edgemodule != null)
                {
                    edgemodule.DoClearRecoveryData();
                }

                LoggerManager.Debug("[Wafer Alignment] ClearRecoveryData(): RecoveryParam Clear Done");

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task<bool> CheckPossibleSetup(bool isrecovery = false)
        {
            bool retVal = false;
            try
            {
                if (!isrecovery)
                {
                    if (this.StageSupervisor().WaferObject.GetStatus() != EnumSubsStatus.EXIST)
                    {
                        EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Warning Message", "Wafer is not exist on the chuck", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                           "Warning Message", "Setup to initialize all existing Wafer Alignment setup data. Do you want to continue?", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            if (Extensions_IParam.ProberRunMode != RunMode.EMUL & Wafer.WaferStatus != EnumSubsStatus.EXIST)
                            {
                                await this.MetroDialogManager().ShowMessageDialog(
                                      Properties.Resources.ErrorMessageTitle, Properties.Resources.NotExistWaferMessage,
                                      EnumMessageStyle.Affirmative);
                                return retVal;
                            }

                            bool isCurTempWithinSetTempRange = this.TempController().IsCurTempWithinSetTempRange();
                            if (isCurTempWithinSetTempRange)
                            {

                                this.SetSetupState();
                                this.GetParam_Wafer().PadSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
                                this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Clear();
                                this.GetParam_Wafer().SetAlignState(AlignStateEnum.IDLE);
                                this.GetParam_ProbeCard().PinSetupChangedToggle.DoneState = ElementStateEnum.NEEDSETUP;
                                retVal = true;
                            }
                            else
                            {
                                ret = await this.MetroDialogManager().ShowMessageDialog(
                                    Properties.Resources.TempErrorMessage, Properties.Resources.TempErrorMessageTitle, EnumMessageStyle.Affirmative);
                            }
                        }
                    }

                }
                else
                {
                    if (this.StageSupervisor().WaferObject.GetStatus() != EnumSubsStatus.EXIST)
                    {
                        EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog("Warning Message", "Wafer is not exist on the chuck", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        if (this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count >= 4)
                        {
                            retVal = true;
                        }
                        else
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Check Pad Setup", "Pad setup needs to be completed.", EnumMessageStyle.Affirmative);
                            retVal = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public ModuleStateEnum GetModuleState()
        {
            return ModuleState.GetState();
        }
        public ModuleStateEnum GetPreModuleState()
        {
            return PreInnerState.GetModuleState();
        }
        public Point GetLeftCornerPositionForWAFCoord(WaferCoordinate position)
        {
            return GetLeftCornerPosition(position);
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public EventCodeEnum EdgeCheck(ref WaferCoordinate centeroffset, ref double maximum_value_X, ref double maximum_value_Y)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            maximum_value_X = 0.0;
            maximum_value_Y = 0.0;
            centeroffset = new WaferCoordinate();
            try
            {
                var procmodule = Template.GetProcessingModule();
                var edgemodule = procmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferEdgeProcModule))).FirstOrDefault();
                //ret = procmodule[1].Execute();
                if (edgemodule != null)
                {
                    ret = edgemodule.Execute();
                    if (ret == EventCodeEnum.NONE)
                    {
                        centeroffset.X.Value = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetX();
                        centeroffset.Y.Value = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetY();
                        centeroffset.Z.Value = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetZ();
                        centeroffset.T.Value = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetT();

                        LoggerManager.Debug($"{this.GetType().Name}, centeroffset X: { centeroffset.X.Value }, centeroffset Y: { centeroffset.Y.Value }, centeroffset Z: { centeroffset.Z.Value }, centeroffset T: { centeroffset.T.Value }");

                        double ratio_x = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).GetRatioX();
                        double ratio_y = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).GetRatioY();

                        double grabSize_width = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).GetGrabSizeWidth();
                        double grabSize_height = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).GetGrabSizeHeight();

                        double x_maximum = (ratio_x * grabSize_width) / 2;
                        double y_maximum = (ratio_y * grabSize_height) / 2;

                        maximum_value_X = x_maximum;
                        maximum_value_Y = y_maximum;
                    }
                    else
                    {
                        LoggerManager.Debug($"{this.GetType().Name}, Edge Failed. return value: {ret}");
                    }
                }
                else
                {
                    ret = EventCodeEnum.DOT_NET_ERROR;
                }

            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum GetHighStandardParam()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var procmodule = Template.GetProcessingModule();
                IWaferHighProcModule highmodule = (IWaferHighProcModule)procmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferHighProcModule))).FirstOrDefault();
                //ret = procmodule[1].Execute();
                if (highmodule != null)
                {
                    HighStandardParam_Clone = highmodule.HighStandard_IParam as WA_HighMagParam_Standard;
                    HeightProfilingPointType = HighStandardParam_Clone.HeightProfilingPointType.Value;
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.DOT_NET_ERROR;
                }

            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum SetHighStandardParam(HeightPointEnum heightpoint)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var procmodule = Template.GetProcessingModule();
                IWaferHighProcModule highmodule = (IWaferHighProcModule)procmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferHighProcModule))).FirstOrDefault();
                //ret = procmodule[1].Execute();
                if (highmodule != null)
                {
                    HighStandardParam_Clone = highmodule.HighStandard_IParam as WA_HighMagParam_Standard;
                    HighStandardParam_Clone.HeightProfilingPointType.Value = heightpoint;
                    HighStandardParam_Clone.HeightPosParams.Clear(); //wafermapmaker ui에서 값을 변경 한 경우, waferalign 하면서 값을 add하기 위함.
                    ret = highmodule.UpdateHeightProfiling();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum UpdateHeightProfiling()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var procmodule = Template.GetProcessingModule();

                IWaferHighProcModule highmodule = (IWaferHighProcModule)procmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferHighProcModule))).FirstOrDefault();

                if (highmodule != null)
                {
                    ret = highmodule.UpdateHeightProfiling();

                    LoggerManager.Debug("WaferAligner UpdateHeightProfiling()");
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public WaferCoordinate GetPatternWaferCenter()
        {
            WaferCoordinate patterncenter = new WaferCoordinate();
            try
            { //low pattern, high pattern에 저장되는 wafer center 값은 같음 (전제)

                var highprocmodule = Template.GetProcessingModule();
                IWaferHighProcModule highmodule = (IWaferHighProcModule)highprocmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferHighProcModule))).FirstOrDefault();
                if (highmodule != null)
                {
                    HighStandardParam_Clone = highmodule.HighStandard_IParam as WA_HighMagParam_Standard;
                    if (HighStandardParam_Clone.Patterns.Value.Count > 0)
                    {
                        HighStandardParam_Clone.Patterns.Value[0].WaferCenter.CopyTo(patterncenter);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return patterncenter;
        }
        public EventCodeEnum SaveRecoveryLowPattern()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                var lowprocmodule = Template.GetProcessingModule();
                IWaferLowProcModule lowmodule = (IWaferLowProcModule)lowprocmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferLowProcModule))).FirstOrDefault();
                if (lowmodule != null)
                {
                    LowStandardParam_Clone = lowmodule.LowStandard_IParam as WA_LowMagParam_Standard;
                    foreach (var templowpattern in WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer)
                    {
                        LowStandardParam_Clone.Patterns.Value.Add(templowpattern);
                    }
                    retval = lowmodule.SaveDevParameter(); //image save.

                    if (retval == EventCodeEnum.NONE)
                    {
                        //State 변경
                        lowmodule.SetStepSetupCompleteState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public EventCodeEnum SaveRecoveryHighPattern()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                var highprocmodule = Template.GetProcessingModule();
                IWaferHighProcModule highmodule = (IWaferHighProcModule)highprocmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferHighProcModule))).FirstOrDefault();
                if (highmodule != null)
                {
                    HighStandardParam_Clone = highmodule.HighStandard_IParam as WA_HighMagParam_Standard;
                    foreach (var temphighpattern in WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer)
                    {
                        HighStandardParam_Clone.Patterns.Value.Add(temphighpattern);
                    }
                    retval = highmodule.SaveDevParameter(); //image save.

                    if (retval == EventCodeEnum.NONE)
                    {
                        highmodule.SetStepSetupCompleteState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }



        public EventCodeEnum ClearRecoverySetupPattern()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsNewSetup == false)
                {
                    if (WaferAlignInfo.RecoveryParam?.TemporaryLowPatternBuffer?.Count > 0)
                    {
                        if (WaferAlignInfo.RecoveryParam.SetBoundaryFalg == true)
                        {
                            var patterncenter = GetPatternWaferCenter();

                            int index = 0;
                            foreach (var item in this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer)
                            {
                                index++;
                                LoggerManager.Debug($"Wafer Recovery Setup - Low Pattern Index : {index}, xpos : {item.X.Value:0.00} ypos : {item.Y.Value:0.00}" +
                                       $"LCOffset (X,Y) = ({this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX:0.00},{this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY:0.00}");

                                item.X.Value -= (this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX + this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX);
                                item.Y.Value -= (this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY + this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY);

                                item.WaferCenter.X.Value = patterncenter.GetX();
                                item.WaferCenter.Y.Value = patterncenter.GetY();
                            }
                            SaveRecoveryLowPattern();
                            InnerState.ClearState();
                            ModuleState.StateTransition(InnerState.GetModuleState());
                        }
                        else
                        {
                            this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.FAIL);
                            this.MetroDialogManager().ShowMessageDialog("Low Pattern Save Failed", $"Added low pattern not registered because boundary teaching is not complete", EnumMessageStyle.Affirmative);
                        }
                    }

                    if (WaferAlignInfo.RecoveryParam?.TemporaryHighPatternBuffer?.Count > 0)
                    {
                        if (WaferAlignInfo.RecoveryParam.SetRefPadFalg == true)
                        {
                            var patterncenter = GetPatternWaferCenter();

                            int index = 0;
                            foreach (var item in this.WaferAligner().WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer)
                            {
                                index++;
                                LoggerManager.Debug($"Wafer Recovery Setup - High Pattern Index : {index}, xpos : {item.X.Value:0.00} ypos : {item.Y.Value:0.00}" +
                                          $"LCOffset (X,Y) = ({this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX:0.00},{this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY:0.00}" +
                                          $"RefPadOffset (X,Y) = ({this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX:0.00},{this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY:0.00})");

                                item.X.Value -= (this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetX + this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetX);
                                item.Y.Value -= (this.WaferAligner().WaferAlignInfo.RecoveryParam.LCPointOffsetY + this.WaferAligner().WaferAlignInfo.RecoveryParam.RefPadOffsetY);

                                item.WaferCenter.X.Value = patterncenter.GetX();
                                item.WaferCenter.Y.Value = patterncenter.GetY();
                            }
                            SaveRecoveryHighPattern();
                            InnerState.ClearState();
                            ModuleState.StateTransition(InnerState.GetModuleState());
                        }
                        else
                        {
                            this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.FAIL);
                            this.MetroDialogManager().ShowMessageDialog("High Pattern Save Failed", $"Added high pattern not registered because ref.pad teaching is not complete", EnumMessageStyle.Affirmative);
                        }
                    }

                    if (WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Count == Wafer.GetSubsInfo().Pads.DutPadInfos.Count)
                    {
                        for (int i = 0; i < WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Count; i++)
                        {
                            Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.X.Value = WaferAlignInfo.RecoveryParam.BackupDutPadInfos[i].PadCenter.X.Value;
                            Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.Y.Value = WaferAlignInfo.RecoveryParam.BackupDutPadInfos[i].PadCenter.Y.Value;
                        }
                        WaferAlignInfo.RecoveryParam.BackupDutPadInfos.Clear();
                    }

                    var lowprocmodule = Template.GetProcessingModule();
                    IWaferLowProcModule lowmodule = (IWaferLowProcModule)lowprocmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferLowProcModule))).FirstOrDefault();
                    if (lowmodule != null)
                    {
                        WaferAlignInfo.RecoveryParam.TemporaryLowPatternBuffer.Clear();
                        lowmodule.SetRecoveryStepState(EnumMoudleSetupState.NOTCOMPLETED);
                    }

                    var highprocmodule = Template.GetProcessingModule();
                    IWaferHighProcModule highmodule = (IWaferHighProcModule)highprocmodule.Where(module => module.GetType().GetInterfaces().Contains(typeof(IWaferHighProcModule))).FirstOrDefault();
                    if (highmodule != null)
                    {
                        WaferAlignInfo.RecoveryParam.TemporaryHighPatternBuffer.Clear();
                        highmodule.SetRecoveryStepState(EnumMoudleSetupState.NOTCOMPLETED);
                    }

                    this.WaferAligner().SetRefPad(Wafer.GetSubsInfo().Pads.DutPadInfos);
                    WaferAlignInfo.RecoveryParam = new WARecoveryParam();
                }
                else
                {
                    IsNewSetup = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void SetRefPad(IList<DUTPadObject> padinfo)
        {
            try
            {
                if (Wafer.GetSubsInfo().Pads.DutPadInfos != null)
                {
                    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count() != 0)
                    {
                        Wafer.GetSubsInfo().Pads.RefPad = new DUTPadObject();
                        var refpad = padinfo.SingleOrDefault(info => info.PadNumber.Value == 1);
                        if (refpad != null)
                        {
                            Wafer.GetSubsInfo().Pads.RefPad = refpad;
                        }
                        else
                        {
                            Wafer.GetSubsInfo().Pads.RefPad = padinfo[0];
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public (double, double) GetVerifyCenterLimitXYValue()
        {
            (double, double) retVal = (0, 0);
            try
            {
                if (WaferAlignSysParam != null)
                {
                    double xLimit = WaferAlignSysParam.VerifyCenterLimitX?.Value ?? 0;
                    double yLimit = WaferAlignSysParam.VerifyCenterLimitY?.Value ?? 0;
                    retVal = (xLimit, yLimit);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetVerifyCenterLimitXYValue(double xLimit, double yLimit)
        {
            try
            {
                if (WaferAlignSysParam != null)
                {
                    if (WaferAlignSysParam.VerifyCenterLimitX != null)
                    {
                        WaferAlignSysParam.VerifyCenterLimitX.Value = xLimit;
                        LoggerManager.Debug($"SetVerifyCenterLimitXYValue() set VerifyCenterLimitX : {xLimit}");
                    }
                    if (WaferAlignSysParam.VerifyCenterLimitY != null)
                    {
                        WaferAlignSysParam.VerifyCenterLimitY.Value = yLimit;
                        LoggerManager.Debug($"SetVerifyCenterLimitXYValue() set VerifyCenterLimitY : {yLimit}");
                    }

                    SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public List<Guid> GetRecoverySteps()
        {
            List<Guid> steps = new List<Guid>();

            try
            {
                string recoveryRequiredModule = "";
                List<ISubModule> modules = Template.GetProcessingModule();

                for (int index = 0; index < modules.Count; index++)
                {

                    SubModuleStateEnum modulestate = modules[index].GetState();
                    if (modulestate == SubModuleStateEnum.ERROR || modulestate == SubModuleStateEnum.RECOVERY)
                    {
                        int indexOfLastDot = modules[index].ToString().LastIndexOf(".");
                        recoveryRequiredModule = indexOfLastDot >= 0 ? modules[index].ToString().Substring(indexOfLastDot + 1) : modules[index].ToString();
                        break;
                    }
                }

                if (recoveryRequiredModule != "")
                {
                    var template = LoadTemplateParam.RecoveryTemplates.Where(x => x.ErrorModuleName == recoveryRequiredModule).FirstOrDefault();
                    if (template != null)
                    {
                        steps = template.RecoveryStepGUID;
                    }
                }
                else
                {
                    //wafer align failed인 이유로 Button이 활성화 되기 때문에 ERROR, RECOVERY 인 Module이 없다는 것은 Expcetion.
                    //steps.Count == 0
                }
            }
            catch (Exception err)
            {
                steps.Clear();
                LoggerManager.Exception(err);
            }
            return steps;
        }
        public EventCodeEnum CalculateOffsetToAutoFocusedPosition(ICamera curcam, double padsize_x, double padsize_y)
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                var focusingModule = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());
                var focusingParam = new NormalFocusParameter();

                double focusingROIX = padsize_x / curcam.GetRatioX();
                double focusingROIY = padsize_y / curcam.GetRatioY();
                int OffsetX = curcam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(focusingROIX) / 2;
                int OffsetY = curcam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(focusingROIY) / 2;

                focusingParam.SetDefaultParam();
                focusingParam.FocusRange.Value = 50;
                focusingParam.FocusingCam.Value = curcam.GetChannelType();
                focusingParam.FocusingROI.Value = new Rect(OffsetX, OffsetY, focusingROIX, focusingROIY);
                //pixel = size / ratio

                result = focusingModule.Focusing_Retry(focusingParam, false, false, false, this);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] CalculateOffsetToAutoFocusedPosition(): Error occured.");
                LoggerManager.Exception(err);
            }
            return result;
        }
    }
}
