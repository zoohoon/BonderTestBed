using System;
using System.Collections.Generic;
using System.Linq;

namespace PinAlign
{
    using LoaderController.GPController;
    using LogModule;
    using MetroDialogInterfaces;
    using Newtonsoft.Json;
    using NotifyEventModule;
    using PinAlignParam;
    using PnPControl;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Align;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Event;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using ProberInterfaces.Template;
    using RelayCommandBase;
    using SerializerUtil;
    using SinglePinAlign;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Xml.Serialization;


    /*
        핀 얼라인 관련 파라미터/오브젝트 개념 정리

        1) this.StageSupervisor().ProbeCardInfo : 물리적인 개념으로 분류된 오브젝트. 
                           카드 정보 + 더트 정보 + 각 더트 별 포함된 핀 정보 + 핀의 위치 정보 + 각 핀 별 얼라인 설정 정보
                           (개념적으로 디바이스 파라미터의 속성을 가지며, 시스템 파라미터일 수 없다)

        2) this.PinAligner().AlignInfo     : 핀 얼라인 모듈의 파라미터 그룹. 디바이스 파라미터와 시스템 파라미터 모두를 가질 수 있다.
                                             카드의 물리적인 특성과 상관 없는 얼라인 인터벌과 같은 운영 파라미터들을 포함한다.

        3) this.PinAligner().AlignResult   : 바로 이전에 진행되었던 핀 얼라인 결과가 담긴다. 매번 값이 바뀔 수 있으며 파일에서 로드하지 않는다.
                                             (추가적인 기능으로 나중에 별도로 저장할 수는 있다)
                           

    */
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PinAligner : IPinAligner, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.PinAlign);
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

        public DateTime _LastAlignDoneTime;
        public DateTime LastAlignDoneTime
        {
            get { return _LastAlignDoneTime; }
            set
            {
                if (value != _LastAlignDoneTime)
                {
                    _LastAlignDoneTime = value;
                }
            }
        }

        private IParam _PinAlignDevParam;
        public IParam PinAlignDevParam
        {
            get { return _PinAlignDevParam; }
            set
            {
                if (value != _PinAlignDevParam)
                {
                    _PinAlignDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IPinAlignInfo _PinAlignInfo = new PinAlignInfo();
        public IPinAlignInfo PinAlignInfo
        {
            get { return _PinAlignInfo; }
            set
            {
                if (value != _PinAlignInfo)
                {
                    _PinAlignInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PinAlignRunning;
        public bool PinAlignRunning
        {
            get { return _PinAlignRunning; }
            private set
            {
                if (value != _PinAlignRunning)
                {
                    _PinAlignRunning = value;
                    RaisePropertyChanged();
                }
            }
        }


        [NonSerialized]
        [XmlIgnore, JsonIgnore]
        private bool _IsRecoveryStarted;
        public bool IsRecoveryStarted
        {
            get { return _IsRecoveryStarted; }
            set
            {
                if (value != _IsRecoveryStarted)
                {
                    _IsRecoveryStarted = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ISequenceEngineManager SequenceEngineManager { get; set; }

        //private bool _IsChangedSource = false;
        //public bool IsChangedSource
        //{
        //    get { return _IsChangedSource; }
        //    set
        //    {
        //        if (value != _IsChangedSource)
        //        {
        //            _IsChangedSource = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private PINALIGNSOURCE _PinAlignSource;
        public PINALIGNSOURCE PinAlignSource
        {
            get { return _PinAlignSource; }
            set
            {
                if (value != _PinAlignSource)
                {
                    _PinAlignSource = value;
                    //IsChangedSource = true;

                    RaisePropertyChanged();
                }
            }
        }

        private bool _UseSoakingSamplePinAlign = false;
        public bool UseSoakingSamplePinAlign
        {
            get { return _UseSoakingSamplePinAlign; }
            set
            {
                if (value != _UseSoakingSamplePinAlign)
                {
                    _UseSoakingSamplePinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeCardGraphicsModule _ProbeCardGraphicsContext = new ProbeCardGraphicsModule();
        public ProbeCardGraphicsModule ProbeCardGraphicsContext
        {
            get { return _ProbeCardGraphicsContext; }
            set
            {
                if (value != _ProbeCardGraphicsContext)
                {
                    _ProbeCardGraphicsContext = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinAlignAdditionalFunctionClass _PinAlignFunc = new PinAlignAdditionalFunctionClass();
        public PinAlignAdditionalFunctionClass PinAlignFunc
        {
            get { return _PinAlignFunc; }
            set
            {
                if (value != _PinAlignFunc)
                {
                    _PinAlignFunc = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ILotOPModule LotOPModule
        {
            get
            {
                return this.LotOPModule();
            }
        }

        public IFileManager FileManager
        {
            get
            {
                return this.FileManager();
            }
        }

        public IParamManager ParamManager
        {
            get
            {
                return this.ParamManager();
            }
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

        private PinAlignState _PinAlignState;
        public PinAlignState PinAlignState
        {
            get { return _PinAlignState; }
        }

        public IInnerState InnerState
        {
            get { return _PinAlignState; }
            set
            {
                if (value != _PinAlignState)
                {
                    _PinAlignState = value as PinAlignState;
                    RaisePropertyChanged();

                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        private ModuleStateBase _ModuleState;

        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private PARecoveryModule _RecoveryModules = new PARecoveryModule();

        public PARecoveryModule RecoveryModules
        {
            get { return _RecoveryModules; }
            set { _RecoveryModules = value; }
        }

        //.. IHasTemplate

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
        private TemplateStateCollection _SinglePinTemplate;
        public TemplateStateCollection SinglePinTemplate
        {
            get { return _SinglePinTemplate; }
            set
            {
                if (value != _SinglePinTemplate)
                {
                    _SinglePinTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }
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

        private ITemplateFileParam _SinglePinTemplateParameter;
        public ITemplateFileParam SinglePinTemplateParameter
        {
            get { return _SinglePinTemplateParameter; }
            set
            {
                if (value != _SinglePinTemplateParameter)
                {
                    _SinglePinTemplateParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _WaferTransferRunning = false;
        public bool WaferTransferRunning
        {
            get => _WaferTransferRunning;
            set => _WaferTransferRunning = value;
        }
        private bool _PIN_ALIGN_Failure;
        public bool PIN_ALIGN_Failure
        {
            get { return _PIN_ALIGN_Failure; }
            set
            {
                if (value != _PIN_ALIGN_Failure)
                {
                    _PIN_ALIGN_Failure = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Each_Pin_Failure;
        public bool Each_Pin_Failure
        {
            get { return _Each_Pin_Failure; }
            set
            {
                if (value != _Each_Pin_Failure)
                {
                    _Each_Pin_Failure = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ITemplateParam LoadTemplateParam { get; set; }
        public ITemplateParam SinglePinLoadTemplateParam { get; set; }
        public ISubRoutine SubRoutine { get; set; }
        //====

        #region Add by gordon 1711219 for New Pin Align,,,?
        public int curPinIndex;

        //PinAlignDataSet pinAlignDataSet = new PinAlignDataSet();

        private int _CurSetupIndex;
        public int CurSetupIndex
        {
            get { return _CurSetupIndex; }
            set
            {
                if (value != _CurSetupIndex)
                {
                    _CurSetupIndex = value;
                    ChangeSetupProcedule();
                    RaisePropertyChanged();
                }
            }
        }

        private object _MapViewTarget;
        public object MapViewTarget
        {
            get { return _MapViewTarget; }
            set
            {
                if (value != _MapViewTarget)
                {
                    _MapViewTarget = value;
                    ChangeSetupProcedule();
                    RaisePropertyChanged();
                }
            }
        }

        private string _ScreenLabel;
        public string Label
        {
            get { return _ScreenLabel; }
            set
            {
                if (value != _ScreenLabel)
                {
                    _ScreenLabel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _SeletedItemChangedCommand;
        public ICommand SeletedItemChangedCommand
        {
            get
            {
                if (null == _SeletedItemChangedCommand) _SeletedItemChangedCommand = new RelayCommand(SeletedItemChanged);
                return _SeletedItemChangedCommand;
            }
        }



        private ISubModule _CurrentProcedure;
        [ParamIgnore]
        public ISubModule CurrentProcedure
        {
            get { return _CurrentProcedure; }
            set
            {
                if (value != _CurrentProcedure)
                {
                    _CurrentProcedure = value;

                    //this.CurrentProcedure.SwitchSetupProcess();

                    RaisePropertyChanged();
                }
            }
        }

        public IPnpSetup CurrentSetupProcedure
        {
            get { return (IPnpSetup)CurrentProcedure; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
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


        private EventCodeEnum ChangeSetupProcedule()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                List<ISubModule> modules = Template.GetProcessingModule();
                this.CurrentProcedure = modules[CurSetupIndex];
                //this.CurrentProcedure.SwitchSetupProcess();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        private void SeletedItemChanged()
        {
            try
            {
                //ProberViewModuel.ScreenTestkeyValuePair.TryGetValue(SetupsTreeView, out _ProberMain);
                // _ProberMain.InitModule(Container);
                //ProberMainScreens = _ProberMain;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private string _ParamPath;
        public string ParamPath
        {
            get { return _ParamPath; }
            set
            {
                if (value != _ParamPath)
                {
                    _ParamPath = value;
                    RaisePropertyChanged();
                }
            }
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

        /// <summary>
        /// original list
        /// </summary>
        private List<PinSizeValidateResult> _Monitoring_Original_List = new List<PinSizeValidateResult>();
        public List<PinSizeValidateResult> ValidPinTipSize_Original_List
        {
            get { return _Monitoring_Original_List; }
            set
            {
                if (value != _Monitoring_Original_List)
                {
                    _Monitoring_Original_List = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tip Monitoring을 하는 중에 설정한 크기 범위에서 벗어난 Pin들의 List
        /// </summary>
        private List<PinSizeValidateResult> _Monitoring_OutOfSize_List = new List<PinSizeValidateResult>();
        public List<PinSizeValidateResult> ValidPinTipSize_OutOfSize_List
        {
            get { return _Monitoring_OutOfSize_List; }
            set
            {
                if (value != _Monitoring_OutOfSize_List)
                {
                    _Monitoring_OutOfSize_List = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// </summary>
        private List<PinSizeValidateResult> _Monitoring_SizeInRange_List = new List<PinSizeValidateResult>();
        public List<PinSizeValidateResult> ValidPinTipSize_SizeInRange_List
        {
            get { return _Monitoring_SizeInRange_List; }
            set
            {
                if (value != _Monitoring_SizeInRange_List)
                {
                    _Monitoring_SizeInRange_List = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region added for SoakingPintolarance param by Tony 180323
        private PinCoordinate _SoakingPinTolerance;
        public PinCoordinate SoakingPinTolerance
        {
            get { return _SoakingPinTolerance; }
            set { _SoakingPinTolerance = value; }
        }

        private ISinglePinAlign _SinglePinAligner = null;
        public ISinglePinAlign SinglePinAligner
        {
            get { return _SinglePinAligner; }
            set { _SinglePinAligner = value; }
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




        #endregion


        public PinAligner()
        {
        }

        public EventCodeEnum StateTranstionSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                InnerStateTransition(new PinAlignIdleState(this));
                List<ISubModule> modules = Template.GetProcessingModule();
                foreach (IProcessingModule module in modules)
                {
                    //err = module.Modify();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }


            return retVal;
        }

        public bool IsExistParamFile(String paramPath)
        {
            if (Directory.Exists(Path.GetDirectoryName(paramPath)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(paramPath));

            return File.Exists(paramPath);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            LoggerManager.Debug($"[PinAligner] InitModule() : Init Module Start");

            try
            {
                if (Initialized == false)
                {
                    SequenceEngineManager = this.SequenceEngineManager();

                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    InnerState = new PinAlignIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    LoadPinAlignTemplateFile();

                    InitSinglePinTemplate();

                    _SinglePinAligner = new BlobSinglePinAlign();

                    Label = "PinAligner";
                    Name = "PinAligner";

                    retval = this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotStartEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotEndEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(CardChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(CardDockEvent).FullName, "ProbeEventSubscibers", EventFired);

                    Initialized = true;

                    ConnectValueChangedEventHandler();

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

            LoggerManager.Debug($"[PinAligner] InitModule() : Init Module Done");

            return retval;
        }

        public void ConnectValueChangedEventHandler()
        {
            try
            {
                this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardType.ValueChangedEvent -= ProbeCardType_ValueChangedEvent;
                this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardType.ValueChangedEvent += ProbeCardType_ValueChangedEvent;

                CheckPinAlignTemplate(this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardType.Value);
                CheckSinglePinAlignTemplate(this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardType.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CheckSinglePinAlignTemplate(PROBECARD_TYPE cardtype)
        {
            try
            {
                // TODO : SinglePinTemplate이 새롭게 로드되어야 함.
                // TODO : SinglePinTemplateParameter이 새롭게 로드되어야 함.
                // TODO : String 하드코딩 변경해야 됨.

                string SinglePinAlignType = string.Empty;

                if (cardtype == PROBECARD_TYPE.Cantilever_Standard)
                {
                    SinglePinAlignType = "SinglePinAlignStandard";
                }
                else if (cardtype == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                {
                    SinglePinAlignType = "SinglePinAlignMEMSType";
                }
                else if (cardtype == PROBECARD_TYPE.VerticalType)
                {
                    SinglePinAlignType = "SinglePinAlignVertical";
                }

                if (SinglePinTemplateParameter != null && SinglePinAlignType != string.Empty)
                {
                    if ((SinglePinTemplateParameter.Param != null) && (SinglePinTemplateParameter.Param.TemplateInfos != null) && (SinglePinTemplateParameter.Param.TemplateInfos.Count > 0))
                    {
                        TemplateInfo template = SinglePinTemplateParameter.Param.TemplateInfos.FirstOrDefault(x => x.Name.Value == SinglePinAlignType);

                        if (template != null)
                        {
                            SinglePinTemplateParameter.Param.SeletedTemplate = template;

                            SaveSinglePinalignTemplate();

                            InitSinglePinTemplate();

                            LoadSinglePinAlignTemplateFile();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CheckPinAlignTemplate(PROBECARD_TYPE cardtype)
        {
            try
            {
                string PinAlignType = string.Empty;

                if (cardtype == PROBECARD_TYPE.Cantilever_Standard)
                {
                    PinAlignType = "Standard";
                }
                else if (cardtype == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                {
                    PinAlignType = "MEMSType";
                }
                else if (cardtype == PROBECARD_TYPE.VerticalType)
                {
                    PinAlignType = "Vertical";
                }

                if (TemplateParameter != null && PinAlignType != string.Empty)
                {
                    if ((TemplateParameter.Param != null) && (TemplateParameter.Param.TemplateInfos != null) && (TemplateParameter.Param.TemplateInfos.Count > 0))
                    {
                        TemplateInfo template = TemplateParameter.Param.TemplateInfos.FirstOrDefault(x => x.Name.Value == PinAlignType);

                        if (template != null)
                        {
                            TemplateParameter.Param.SeletedTemplate = template;

                            SavePinalignTemplate();

                            LoadPinAlignTemplateFile();

                            //InitSinglePinTemplate();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ProbeCardType_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                PROBECARD_TYPE cardtype;
                if (newValue is string)
                {
                    string sValue = (string)newValue;
                    cardtype = (PROBECARD_TYPE)Enum.Parse(typeof(PROBECARD_TYPE), sValue);
                }
                else
                {
                    cardtype = (PROBECARD_TYPE)newValue;
                }

                CheckPinAlignTemplate(cardtype);

                CheckSinglePinAlignTemplate(cardtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitSinglePinTemplate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IParam tmpParam = null;

            try
            {
                retVal = this.LoadParameter(ref tmpParam, typeof(TemplateFileParam), null, this.FileManager().GetDeviceParamFullPath(SinglePinTemplateParameter.Param.SeletedTemplate.Path.Value));

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PinAligner] InitSinglePinTemplate() : SeletedTemplate.Path {SinglePinTemplateParameter.Param.SeletedTemplate.Path.Value}");
                    SinglePinLoadTemplateParam = tmpParam as TemplateFileParam;
                    ITemplateCollection templatemodule = TemplatModuleService.LoadTemplateModule(this.GetContainer(), this, SinglePinLoadTemplateParam, true);
                    SinglePinTemplate = (TemplateStateCollection)templatemodule;
                }
                else
                {
                    retVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error(String.Format("[PinAligner] InitSinglePinTemplate(): DeSerialize Error"));
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
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

                    isInjected = this.CommandManager().SetCommand<IDoManualPinAlign>(this, param);

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
                    }
                    else
                    {
                        retval = EventCodeEnum.PIN_MANUALOPERATION_COMMAND_REJECTED;

                        LoggerManager.Debug($"[{this.GetType().Name}], DoManualOperation(): retval = {retval}");
                    }
                }
                else
                {
                    if (isReady == false)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Manual Pin Align", $"Pin alignment cannot be performed.\r\n The reason is {msg}", EnumMessageStyle.Affirmative);
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

        public EventCodeEnum DoPinAlignProcess()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    LoggerManager.ActionLog(ModuleLogType.PIN_ALIGN, StateLogType.START,
                    $"Lot ID: {this.LotOPModule().LotInfo.LotName.Value}, " +
                    $"Device:{this.FileManager().GetDeviceName()}, " +
                    $"Card ID: {this.CardChangeModule().GetProbeCardID()}",
                    this.LoaderController().GetChuckIndex());
                }
                else
                {
                    LoggerManager.ActionLog(ModuleLogType.PIN_ALIGN, StateLogType.START,
                    $"Device:{this.FileManager().GetDeviceName()}, " +
                    $"Card ID: {this.CardChangeModule().GetProbeCardID()}",
                    this.LoaderController().GetChuckIndex());
                }

                PinAlignRunning = true;
                LoggerManager.Debug($"DoPinAlignProcess(): Start. PinAlignRunning = {PinAlignRunning}");
                var PinAlignParam = this.PinAlignDevParam as PinAlignDevParameters;

                this.VisionManager().VisionLib.RecipeInit();

                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE || this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    // Manual Operation
                    // PinAlignSource의 값이 PIN_REGISTRATION이라는 것은 현재 셋업 화면에서 호출 된 것. 이 때, 소스를 변경하면 안된다.

                    if (this.PinAligner().PinAlignSource != PINALIGNSOURCE.PIN_REGISTRATION)
                    {
                        // 현재 코드 상, 카드 체인지가 되어, 이벤트가 발생되면, 디바이스 체인지 플래그가 같이 켜짐.
                        // 따라서, 이곳에서 구분해서 소스를 변경하려면, 비교 순서가 아래와 같이, DeviceChange를 먼저 봐야 됨.
                        if (PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterDeviceChange == false || PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange == false)
                        {
                            if (PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterDeviceChange == false)
                            {
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.DEVICE_CHANGE;
                            }

                            if (PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange == false)
                            {
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.CARD_CHANGE;
                            }
                        }
                    }
                }

                if (PIN_ALIGN_Failure)
                {
                    this.NotifyManager().Notify(EventCodeEnum.PIN_FIND_PIN_FAIL);

                    InnerStateTransition(new PinAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, EventCodeEnum.PIN_ALIGN_FAILED, "Pin Align Error", PinAlignState.GetType().Name)));
                    LoggerManager.Error($"DoPinAlignProcess() PIN_ALIGN_Failure = {PIN_ALIGN_Failure}");
                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                    retval = EventCodeEnum.PIN_ALIGN_FAILED;
                    return retval;
                }

                if (this.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.DONE);
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);

                    this.LastAlignDoneTime = DateTime.Now;
                    PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer = true;
                    this.InnerStateTransition(new PinAlignDoneState(this));

                    retval = EventCodeEnum.NONE;
                    return retval;
                }

                List<ISubModule> modules = this.Template.GetProcessingModule();

                this.LotOPModule().VisionScreenToLotScreen();
                this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.IDLE);

                foreach (IProcessingModule module in modules)
                {
                    retval = EventCodeEnum.NONE;
                    retval = module.ClearData();

                    if (retval != EventCodeEnum.NONE)
                    {
                        break;
                    }
                }

                if (retval == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                    var retVal = this.MarkAligner().DoMarkAlign(true);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                            this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE ||
                            this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            InnerStateTransition(new PinAlignErrorState(this, new EventCodeInfo(this.ReasonOfError.ModuleType, retVal, "Mark Align Error", PinAlignState.GetType().Name)));
                        }

                        return retVal;
                    }
                    else
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(MarkAlignmentDoneBeforePinAlignment).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }

                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    foreach (IProcessingModule module in modules)
                    {
                        if (module.SubModuleState == null)
                        {
                            module.SubModuleState = new SubModuleIdleState(module);
                        }
                    }

                    int processModuleIndex = -1;

                    foreach (IProcessingModule module in modules)
                    {
                        retval = module.SubModuleState.Execute();
                        processModuleIndex++;
                        if (retval != EventCodeEnum.NONE)
                        {
                            if (module.GetState() == SubModuleStateEnum.RECOVERY)
                            {
                                if (PinAlignParam.PinLowAlignRetryAfterPinHighEnable.Value)
                                {
                                    bool skipfalg = false;
                                    int skipindex = -1;

                                    for (int preindex = 0; preindex < processModuleIndex; preindex++)
                                    {
                                        if (modules[preindex].GetState() == SubModuleStateEnum.SKIP)
                                        {
                                            if (preindex != processModuleIndex)
                                            {
                                                skipfalg = true;
                                                skipindex = preindex;
                                            }
                                        }
                                    }

                                    if (skipfalg)
                                    {
                                        for (int preindex = skipindex; preindex < processModuleIndex; preindex++)
                                        {
                                            ISubModule premodule = modules[preindex];
                                            retval = premodule.Execute();
                                        }
                                        if (retval == EventCodeEnum.NONE)
                                        {
                                            retval = module.Execute();
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    }

                    if (retval == EventCodeEnum.NONE)
                    {
                        this.LastAlignDoneTime = DateTime.Now;
                        this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.DONE);
                        this.InnerStateTransition(new PinAlignDoneState(this));
                    }
                    else
                    {
                        if (this.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.DONE);
                            this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);
                            PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer = true;
                            this.LastAlignDoneTime = DateTime.Now;

                            this.InnerStateTransition(new PinAlignDoneState(this));

                            retval = EventCodeEnum.NONE;
                            return retval;
                        }
                        else
                        {
                            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                                this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(PinAlignFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                semaphore.Wait();

                                this.InnerStateTransition(new PinAlignRecoveryState(this, new EventCodeInfo(ReasonOfError.ModuleType, retval, retval.ToString(), this._PinAlignState.GetType().Name)));
                            }
                            else
                            {
                                this.InnerStateTransition(new PinAlignErrorState(this, new EventCodeInfo(ReasonOfError.ModuleType, retval, retval.ToString(), this._PinAlignState.GetType().Name)));
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($": Pinalign : Can not Execute pin alignment.");

                    if (this.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                    {
                        this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.DONE);
                        this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);

                        PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer = true;

                        if (GetPlaneAdjustEnabled() == true)
                        {
                            if (this.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                            {
                                this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                                LoggerManager.Debug($"Wafer Align state invalidate for plane adjustment.");
                            }
                        }

                        this.InnerStateTransition(new PinAlignDoneState(this));

                        retval = EventCodeEnum.NONE;
                        return retval;
                    }
                    else
                    {
                        this.InnerStateTransition(new PinAlignErrorState(this, new EventCodeInfo(ReasonOfError.ModuleType, retval, "Can not Execute pin alignment", this._PinAlignState.GetType().Name)));
                    }
                }

                this.LotOPModule().MapScreenToLotScreen();

                return retval;
            }
            catch (Exception err)
            {
                this.InnerStateTransition(new PinAlignErrorState(this, new EventCodeInfo(ReasonOfError.ModuleType, EventCodeEnum.PIN_ALIGN_FAILED, "Can not Execute pin alignment", this._PinAlignState.GetType().Name)));

                LoggerManager.Exception(err);

                return EventCodeEnum.UNDEFINED;
            }
            finally
            {
                this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                PinAlignRunning = false;

                LoggerManager.Debug($"DoPinAlignProcess(): End. PinAlignRunning = {PinAlignRunning}");
            }
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

        public double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew)
        {
            //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함
            double originDegree = Math.Atan2(
                 pointOld.Y.Value - pivot.Y.Value,
                 pointOld.X.Value - pivot.X.Value)
                 * 180 / Math.PI;

            double updateDegree = Math.Atan2(
                 pointNew.Y.Value - pivot.Y.Value,
                 pointNew.X.Value - pivot.X.Value)
                 * 180 / Math.PI;

            //==> 프로버 카드가 틀어진 θ 각
            return updateDegree - originDegree;
        }

        public EventCodeEnum DrawDutOverlay(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (cam != null)
                {
                    StopDrawDutOverlay(cam);
                    ProbeCardGraphicsContext.OnOff = true;
                    cam.DrawDisplayDelegate += (ImageBuffer img, ICamera camera) =>
                    {
                        ProbeCardGraphicsContext.DrawDutOverlay(img, camera);
                    };
                    cam.UpdateOverlayFlag = true;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayPad() Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum StopDrawDutOverlay(ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeCardGraphicsContext.OnOff = false;

                if (cam != null)
                {
                    if (cam.DrawDisplayDelegate != null)
                    {
                        cam.DrawDisplayDelegate -= (ImageBuffer img, ICamera camera) =>
                        {
                            ProbeCardGraphicsContext.DrawDutOverlay(img, camera);
                        };
                        cam.DrawDisplayDelegate = null;
                        cam.InDrawOverlayDisplay();
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayPad() Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        // Draw Pin 임시
        //public EventCodeEnum DrawPinOverlay(ICamera cam)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        cam.RemoveOverlayContextFlag = true;
        //        ProbeCardGraphicsContext.OnOff = true;
        //        cam.DrawDisplayDelegate += (ImageBuffer img, ICamera camera) =>
        //        {
        //            ProbeCardGraphicsContext.DrawPinOverlay(img, camera);
        //        };

        //    }
        //    catch (Exception err)
        //    {
        //        //LoggerManager.Error($err, "OverlayPad() Error occurred.");
        //        LoggerManager.Exception(err);

        //    }
        //    return retVal;
        //}
        //public EventCodeEnum StopDrawPinOverlay(ICamera cam)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        ProbeCardGraphicsContext.OnOff = false;
        //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            cam.DisplayService.OverlayCanvas.Children.Clear();
        //        }));

        //        if (cam.DrawDisplayDelegate != null)
        //        {
        //            cam.DrawDisplayDelegate -= (ImageBuffer img, ICamera camera) =>
        //            {
        //                ProbeCardGraphicsContext.DrawPinOverlay(img, camera);
        //            };
        //            cam.DrawDisplayDelegate = null;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        //LoggerManager.Error($err, "OverlayPad() Error occurred.");
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}


        public void GetTransformationPin(PinCoordinate crossPin, PinCoordinate orgPin, double degree, out PinCoordinate updatePin)
        {
            try
            {
                if (crossPin == null)
                {
                    updatePin = orgPin;
                    return;
                }
                //==> 좌표의 회전 변환
                //==> x' = (x-cx)cosθ - (y-cy)sinθ
                //==> y' = (y-cx)sinθ + (y-cy)cosθ
                //double radian = (double)(((degree) / 180) * Math.PI);
                double radian = Math.PI * degree / 180.0;
                //double radian = degree;

                double cosq = Math.Cos(radian);
                double sinq = Math.Sin(radian);
                double sx = orgPin.X.Value - crossPin.X.Value;
                double sy = orgPin.Y.Value - crossPin.Y.Value;
                double rx = (sx * cosq - sy * sinq) + crossPin.X.Value; // 결과 좌표 x
                double ry = (sx * sinq + sy * cosq) + crossPin.Y.Value; // 결과 좌표 y
                updatePin = new PinCoordinate(rx, ry, orgPin.Z.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
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
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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

        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SavePinAlignDevParam();

                retval = SavePinalignTemplate();

                retval = SaveSinglePinalignTemplate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public EventCodeEnum SavePinalignTemplate()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(TemplateParameter);
                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Save Ok");
                }
                else
                {
                    LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Save Fail Error Code = " + RetVal.ToString());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum SaveSinglePinalignTemplate()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(SinglePinTemplateParameter);
                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Save Ok");
                }
                else
                {
                    LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Save Fail Error Code = " + RetVal.ToString());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }



        private void ResetPlaneOffsets()
        {
            if (this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Count != 3)
            {
                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Clear();
                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Add(0);
                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Add(0);
                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Add(0);
            }
            this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[0] = 0;
            this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[1] = 0;
            this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[2] = 0;
            this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
        }
        public EventCodeEnum SavePinAlignDevParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(PinAlignDevParam);
                if (((PinAlignDevParameters)PinAlignDevParam).PinPlaneAdjustParam.EnablePinPlaneCompensation.Value == false)
                {
                    ResetPlaneOffsets();
                }
                //if (RetVal == EventCodeEnum.NONE)
                //{
                //    LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Device Parameter Save Ok");
                //}
                //else
                //{
                //    LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Device Parameter Save Fail Error Code = " + RetVal.ToString());
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[PinAligner] LoadDevParameter() : Device Parameter Load Start");

                retVal = LoadPinAlignDevParameter();

                retVal = LoadPinAlignTemplateFile();

                retVal = LoadSinglePinAlignTemplateFile();

                InitSinglePinTemplate();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadPinAlignTemplateFile()
        {
            EventCodeEnum retVal;
            IParam tmpParam = null;
            retVal = this.LoadParameter(ref tmpParam, typeof(PinAlignTemplateFile));

            if (retVal == EventCodeEnum.NONE)
            {
                TemplateParameter = tmpParam as PinAlignTemplateFile;
                LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Load Ok");

                this.TemplateManager().InitTemplate(this);
            }
            else
            {
                LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Load Fail Error Code = " + retVal.ToString());
            }

            return retVal;
        }

        public EventCodeEnum LoadSinglePinAlignTemplateFile()
        {
            EventCodeEnum retVal;
            IParam tmpParam = null;
            retVal = this.LoadParameter(ref tmpParam, typeof(SinglePinAlignTemplateFile));
            if (retVal == EventCodeEnum.NONE)
            {
                SinglePinTemplateParameter = tmpParam as SinglePinAlignTemplateFile;
                LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Load Ok");
            }
            else
            {
                LoggerManager.Debug($"[PinAligner] SaveDevParameter() : Templete Parameter Load Fail Error Code = " + retVal.ToString());
            }

            return retVal;
        }

        public EventCodeEnum CollectElement()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.CollectElement(PinAlignDevParam);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum LoadPinAlignDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                retVal = this.LoadParameter(ref tmpParam, typeof(PinAlignDevParameters));
                if (retVal == EventCodeEnum.NONE)
                {
                    _PinAlignDevParam = tmpParam as PinAlignDevParameters;
                    LoggerManager.Debug($"[PinAligner] LoadPinAlignDevParameter() : Device Parameter Load Ok");

                    PinLowAlignParameter lowparam = null;

                    lowparam = (_PinAlignDevParam as PinAlignDevParameters).PinLowAlignParam;

                    if (lowparam != null)
                    {
                        if (lowparam.Patterns.Count > 0)
                        {
                            string PatternbasePath = "\\PinAlignParam\\PinLowPattern\\";

                            foreach (var patterninfo in lowparam.Patterns)
                            {
                                if (patterninfo.PMParameter != null)
                                {
                                    int order = (int)patterninfo.PatternOrder.Value;

                                    string CorrectPath = PatternbasePath + "_PLP_" + order.ToString();

                                    if (patterninfo.PMParameter.ModelFilePath.Value != CorrectPath)
                                    {
                                        LoggerManager.Debug($"[PinAligner] LoadPinAlignDevParameter() : The saved low pattern ({patterninfo.PatternOrder.Value}) data's path is invalid.");
                                        patterninfo.PMParameter.ModelFilePath.Value = CorrectPath;
                                    }
                                }
                            }
                        }
                    }

                    List<double> before_tolParam = new List<double>();
                    List<double> after_tolParam = new List<double>();
                    foreach (var source in ((PinAlignDevParameters)PinAlignDevParam).PinAlignSettignParam)
                    {
                        before_tolParam.Add(source.SamplePinToleranceX.Value);
                        before_tolParam.Add(source.SamplePinToleranceY.Value);
                        before_tolParam.Add(source.SamplePinToleranceZ.Value);
                    }
                    foreach (var source in ((PinAlignDevParameters)PinAlignDevParam).PinAlignSettignParam)
                    {
                        // adjust Tol X
                        source.SamplePinToleranceX.Value = Math.Max(source.SamplePinToleranceX.LowerLimit, Math.Min(source.SamplePinToleranceX.Value, source.SamplePinToleranceX.UpperLimit));

                        // adjust Tol Y
                        source.SamplePinToleranceY.Value = Math.Max(source.SamplePinToleranceY.LowerLimit, Math.Min(source.SamplePinToleranceY.Value, source.SamplePinToleranceY.UpperLimit));

                        // adjust Tol Z
                        source.SamplePinToleranceZ.Value = Math.Max(source.SamplePinToleranceZ.LowerLimit, Math.Min(source.SamplePinToleranceZ.Value, source.SamplePinToleranceZ.UpperLimit));
                        
                        after_tolParam.Add(source.SamplePinToleranceX.Value);
                        after_tolParam.Add(source.SamplePinToleranceY.Value);
                        after_tolParam.Add(source.SamplePinToleranceZ.Value);
                    }
                    if(before_tolParam.SequenceEqual(after_tolParam) == false)
                    {
                        retVal = SavePinAlignDevParam();
                    }
                }
                else
                {
                    LoggerManager.Debug($"[PinAligner] LoadPinAlignDevParameter() : Device Parameter Load Fail Error Code = " + retVal.ToString());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                /*
                디바이스가 변경 되었을 때

                1) 현재 얼라인 옵셋 초기화 필요
                2) 얼라인 결과 데이터 초기화 필요
                3) 핀 얼라인 상태 초기화 필요
                4) 핀 얼라인 모듈 상태 초기화 필요
                5) 핀 그룹 초기화 (그룹 모드가 AUTO일 경우)

                */
                int TotalPin = 0;
                IPinData tmpPinData;
                GroupData groupData = null;

                TotalPin = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                groupData = new GroupData();

                for (int i = 0; i <= TotalPin - 1; i++)
                {
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);

                    if (tmpPinData != null)
                    {
                        tmpPinData.AlignedOffset.X.Value = 0;
                        tmpPinData.AlignedOffset.Y.Value = 0;
                        tmpPinData.AlignedOffset.Z.Value = 0;

                        tmpPinData.MarkCumulativeCorrectionValue.X.Value = 0;
                        tmpPinData.MarkCumulativeCorrectionValue.Y.Value = 0;
                        tmpPinData.MarkCumulativeCorrectionValue.Z.Value = 0;

                        tmpPinData.LowCompensatedOffset.X.Value = 0;
                        tmpPinData.LowCompensatedOffset.Y.Value = 0;
                        tmpPinData.LowCompensatedOffset.Z.Value = 0;

                        groupData.PinNumList.Add(i + 1);
                        groupData.GroupResult = PINGROUPALIGNRESULT.CONTINUE;
                    }
                }

                this.PinAlignInfo.AlignResult.EachPinResultes.Clear();
                this.PinAlignInfo.AlignResult.TotalAlignPinCount = 0;
                this.PinAlignInfo.AlignResult.TotalFailPinCount = 0;
                this.PinAlignInfo.AlignResult.TotalPassPinCount = 0;
                this.PinAlignInfo.AlignResult.PassPercentage = 0;

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                this.PinAlignInfo.AlignResult.Result = EventCodeEnum.NONE;
                this.InnerStateTransition(new PinAlignIdleState(this));

                //if (GroupMode == Manual)
                //{
                // 아래 두 줄은 핀 그룹을 사용자가 AUTO로 지정했을 경우에만 동작해야 한다. 
                // 아직 메뉴얼 지정 모드가 만들어지기 이전이므로 일단은 조건식 없이 돌고 있는데 나중에 고쳐야 한다.
                groupData.GroupNum = 1;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Clear();
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Add(groupData);
                //}

                var PinAlignDevParameter = this.PinAlignDevParam as PinAlignDevParameters;
                PinAlignDevParameter.PinAlignInterval.MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount;
                PinAlignDevParameter.PinAlignInterval.MarkedDieCountVal = (long)this.LotOPModule().SystemInfo.DieCount;
                this.PinAligner().LastAlignDoneTime = DateTime.Now;

                //PinAlignDevParameter.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;   // Reg 다시 해야 함

                PinAlignDevParameter.PinAlignInterval.FlagAlignProcessedAfterDeviceChange = false;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            var PinAlignDevParameter = this.PinAlignDevParam as PinAlignDevParameters;
            try
            {
                if (sender is LotEndEvent || sender is LotStartEvent)
                {
                    PinAlignDevParameter.PinAlignInterval.FlagAlignProcessedForThisWafer = false;
                }
                else if (sender is WaferUnloadedEvent)
                {
                    PinAlignDevParameter.PinAlignInterval.FlagAlignProcessedForThisWafer = false;
                }
                else if (sender is CardChangedEvent)
                {
                    PinAlignDevParameter.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;

                    InitDevParameter();
                }
                else if (sender is CardDockEvent)
                {
                    PinAlignDevParameter.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;

                    InitDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[PinAligner] IsLotReady() : Check Lot Ready Start");
            List<ISubModule> modules = Template.GetProcessingModule();
            bool isReady = false;
            try
            {
                msg = "";

                var pinAlignDevParam = PinAlignDevParam as PinAlignDevParameters;

                if (!CheckMachineCondition())
                {
                    msg = $"Check Lot Ready No";
                    LoggerManager.Debug($"[PinAligner] IsLotReady() : Check Lot Ready No");
                    return false;
                }

                if (modules != null && Template.SchedulingModule != null)
                {
                    foreach (var subModule in modules)
                    {
                        if (subModule.IsExecute())
                        {
                            RetVal = subModule.ClearData();

                            if (RetVal == EventCodeEnum.NONE)
                            {
                                ClearState();

                                msg = "";
                                isReady = true;

                                LoggerManager.Debug($"[PinAligner] IsLotReady() : Check Lot Ready Ok");
                            }
                            else
                            {
                                LoggerManager.Debug($"[PinAligner] IsLotReady() : Check Lot Ready No");

                                this.InnerStateTransition(new PinAlignErrorState(this, new EventCodeInfo(ReasonOfError.ModuleType, RetVal, "Check Lot Ready No", this._PinAlignState.GetType().Name)));

                                msg = "Pin Align Parameter is not readied";

                                isReady = false;
                            }

                            break;
                        }
                    }
                }
                LoggerManager.Debug($"[PinAligner] IsLotReady() : Check Lot Ready Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isReady;
        }

        public bool IsManualOPReady(out string msg)
        {
            bool retval = false;
            msg = string.Empty;

            try
            {
                retval = true;
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

                this.IsManualOPFinished = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool CheckMachineCondition()
        {
            //이 곳에 각종 센서 및 시스템 에러 확인하는 코드를 추가하시면 됩니다.
            //false = Bad,  true = Good
            return true;
        }
        public EventCodeEnum LoadTemplate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new PinAlignTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(PinAlignTemplateFile));

                TemplateParameter = (PinAlignTemplateFile)tmpParam;
                //DevParam = TemplateParameter;

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "LoadTemplate() : Error occured");
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public void DefaultPNPParamSetting()
        {
            try
            {
                Label = "PinAligner";
                ParamPath = FileManager.FileManagerParam.DeviceParamRootDirectory + "\\PNPParam.json";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            bool retval = false;

            try
            {
                retval = _PinAlignState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }
        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            if (_PinAlignState.GetModuleState() == ModuleStateEnum.RECOVERY)
            {
                return TemplateToPnpConverter.Converter(Template.TemplateModules, false);
            }
            return TemplateToPnpConverter.Converter(Template.TemplateModules);
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                PreInnerState = _PinAlignState;
                InnerState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                PinAlignInnerStateEnum state = (InnerState as PinAlignState).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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

        public PinAlignInnerStateEnum GetPAInnerStateEnum()
        {
            PinAlignInnerStateEnum state;

            try
            {
                state = (_PinAlignState as PinAlignStateBase).GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return state;
        }
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public bool GetPlaneAdjustEnabled()
        {
            return ((PinAlignDevParameters)PinAlignDevParam).PinPlaneAdjustParam.EnablePinPlaneCompensation.Value;
        }
        public IParam GetPinAlignerIParam()
        {
            return this.PinAlignDevParam;
        }

        public void SetPinAlignerIParam(byte[] param)
        {
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(param, out target, typeof(PinAlignDevParameters));

                if (target != null)
                {
                    this.PinAlignDevParam = target as PinAlignDevParameters;
                    this.SaveDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetPinAlignerIParam function is faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public byte[] GetPinAlignerParam()
        {

            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(PinAlignDevParam, typeof(PinAlignDevParameters));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPinAlignParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;

        }

        public byte[] GetTemplateParam()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(TemplateParameter, typeof(PinAlignTemplateFile));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetTemplateParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;

        }


        public EventCodeEnum ChangeAlignKeySetupControlFlag(PinSetupMode mode, bool flag)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                switch (mode)
                {
                    case PinSetupMode.POSITION:
                        break;
                    case PinSetupMode.SIZE:
                        break;
                    case PinSetupMode.BASEFOCUSINGAREA:
                        ProbeCardGraphicsContext.PinSetupControl.ShowBaseFocusingArea = flag;
                        break;
                    case PinSetupMode.TIPSEARCHAREA:
                        ProbeCardGraphicsContext.PinSetupControl.ShowTipSearchArea = flag;
                        break;
                    case PinSetupMode.TIPANDKEYSEARCHAREA:
                        ProbeCardGraphicsContext.PinSetupControl.ShowKeySearchArea = flag;
                        ProbeCardGraphicsContext.PinSetupControl.ShowTipSearchArea = flag;
                        break;
                    case PinSetupMode.THRESHOLD:
                        break;
                    case PinSetupMode.TIPBLOBMIN:
                        ProbeCardGraphicsContext.PinSetupControl.ShowTipBlobMin = flag;
                        break;
                    case PinSetupMode.TIPBLOBMAX:
                        ProbeCardGraphicsContext.PinSetupControl.ShowTipBlobMax = flag;
                        break;
                    case PinSetupMode.KEYBLOBMIN:
                        ProbeCardGraphicsContext.PinSetupControl.ShowKeyBlobMin = flag;
                        ProbeCardGraphicsContext.PinSetupControl.ShowTipBlobMin = flag;
                        break;
                    case PinSetupMode.KEYBLOBMAX:
                        ProbeCardGraphicsContext.PinSetupControl.ShowKeyBlobMax = flag;
                        ProbeCardGraphicsContext.PinSetupControl.ShowTipBlobMax = flag;
                        break;
                    case PinSetupMode.KEYFOCUSINGAREA:
                        ProbeCardGraphicsContext.PinSetupControl.ShowKeyFocusingArea = flag;
                        break;
                    default:
                        break;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetAlignKeySetupControlFlag(PinSetupMode mode)
        {
            bool retVal = false;
            try
            {
                switch (mode)
                {
                    case PinSetupMode.POSITION:
                        break;
                    case PinSetupMode.SIZE:
                        break;
                    case PinSetupMode.TIPANDKEYSEARCHAREA:
                        retVal = ProbeCardGraphicsContext.PinSetupControl.ShowKeySearchArea;
                        break;
                    case PinSetupMode.THRESHOLD:
                        break;
                    case PinSetupMode.KEYBLOBMIN:
                        retVal = ProbeCardGraphicsContext.PinSetupControl.ShowKeyBlobMin;
                        break;
                    case PinSetupMode.KEYBLOBMAX:
                        retVal = ProbeCardGraphicsContext.PinSetupControl.ShowKeyBlobMax;
                        break;
                    case PinSetupMode.KEYFOCUSINGAREA:
                        retVal = ProbeCardGraphicsContext.PinSetupControl.ShowKeyFocusingArea;
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

        public string MakeFailDescription()
        {
            string retval = string.Empty;

            try
            {
                PinAlignResultes alignresult = this.PinAlignInfo.AlignResult;

                PinAlignSettignParameter UsedPinAlignSetting = (this.PinAlignDevParam as PinAlignDevParameters).PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == alignresult.AlignSource);

                if (alignresult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE)
                {
                    if (UsedPinAlignSetting != null)
                    {
                        retval = $"Tolerance Information : X = {UsedPinAlignSetting.CardCenterToleranceX.Value,4:0.0}um, Y = {UsedPinAlignSetting.CardCenterToleranceY.Value,4:0.0}um, Z = {UsedPinAlignSetting.CardCenterToleranceZ.Value,4:0.0}um" +
                            $"Center Diff X: {alignresult.CardCenterDiffX,4:0.0}um, Y: {alignresult.CardCenterDiffY,4:0.0}um, Z:{alignresult.CardCenterDiffZ,4:0.0}um";
                    }
                }
                else if (alignresult.Result == EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE)
                {
                    if (UsedPinAlignSetting != null)
                    {
                        retval = $"Tolerance Information : Min-Max (Z) = {UsedPinAlignSetting.MinMaxZDiffLimit.Value,4:0.0}um";
                    }
                }
                else if (alignresult.Result == EventCodeEnum.PIN_EACH_PIN_TOLERANCE_FAIL)
                {
                    if (UsedPinAlignSetting != null)
                    {
                        retval = $"Tolerance Information : X = {UsedPinAlignSetting.EachPinToleranceX.Value,4:0.0}um, Y = {UsedPinAlignSetting.EachPinToleranceY.Value,4:0.0}um, Z = {UsedPinAlignSetting.EachPinToleranceZ.Value,4:0.0}um";
                    }
                }
                else if (alignresult.Result == EventCodeEnum.PIN_TIP_ALIGN_FAILED)
                {
                    if (UsedPinAlignSetting != null)
                    {
                        retval = $"Pin Tip Failed.";
                    }
                }
                else
                {
                    LoggerManager.Debug($"[PinAligner] MakeFailDescription() : Unknown Result. {alignresult.Result}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// Tip Monitoring 할 때 Blob결과를 받아 사용자가 설정한 크기 범위 내에 들어오는지 확인하는 함수.
        /// True : 범위 벗어남
        /// False: 범위 내에 들어옴
        /// </summary>
        /// <param name="blob_result"></param>
        /// <param name="singlepinalign"></param>
        /// <returns></returns>
        public TIP_SIZE_VALIDATION_RESULT Validation_Pin_Tip_Size(BlobResult blob_result, ISinglePinAlign singlepinalign, double ratio_x, double ratio_y)
        {
            TIP_SIZE_VALIDATION_RESULT returnVal = TIP_SIZE_VALIDATION_RESULT.UNDEFINED;

            try
            {
                if (this.PinAlignDevParam != null)
                {
                    var pin_align_param = this.PinAlignDevParam as PinAlignDevParameters;

                    if (blob_result.DevicePositions.Count == 1)
                    {
                        var blobRet = blob_result.DevicePositions[0];

                        double blobSizeX = blobRet.SizeX * ratio_x;
                        double blobSizeY = blobRet.SizeY * ratio_y;

                        double Val_MaxX = pin_align_param.PinHighAlignParam.PinTipSizeValidation_Max_X.Value;
                        double Val_MinX = pin_align_param.PinHighAlignParam.PinTipSizeValidation_Min_X.Value;
                        double Val_MaxY = pin_align_param.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value;
                        double Val_MinY = pin_align_param.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value;

                        if (blobSizeX > Val_MaxX ||
                            blobSizeX < Val_MinX ||
                            blobSizeY > Val_MaxY ||
                            blobSizeY < Val_MinY)
                        {
                            returnVal = TIP_SIZE_VALIDATION_RESULT.OUT_OF_RANGE;
                        }
                        else
                        {
                            returnVal = TIP_SIZE_VALIDATION_RESULT.IN_RANGE;
                        }

                        string description = string.Empty;

                        if (returnVal == TIP_SIZE_VALIDATION_RESULT.OUT_OF_RANGE)
                        {
                            description = "Out of size.";
                        }
                        else if (returnVal == TIP_SIZE_VALIDATION_RESULT.IN_RANGE)
                        {
                            description = "Size is within range.";
                        }
                        else
                        {
                            description = "Undefined.";
                        }

                        LoggerManager.PinLog($"[TIP SIZE VALIDATION], {description} Dut No : #{singlepinalign.AlignPin.DutNumber.Value}, Pin No: #{singlepinalign.AlignPin.PinNum.Value}, Result Tip Blob Size (X, Y): [{blobSizeX:0.000}um, {blobSizeY:0.000}um], " +
                                                                                                  $"Validation Minimum Size (X, Y) : [{Val_MinX}, {Val_MinY}], " +
                                                                                                  $"Validation Maximum Size (X, Y) : [{Val_MaxX}, {Val_MaxY}]" +
                                                                                                  $"return value : {returnVal}");
                    }
                    else
                    {
                        LoggerManager.PinLog($"[TIP SIZE VALIDATION], The value for this parameter is not valid. detected blob count : {blob_result.DevicePositions.Count}");
                    }
                }
                else
                {
                    LoggerManager.PinLog($"[TIP SIZE VALIDATION], The value for this parameter is not valid. ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return returnVal;
        }

        object pinImageUploadLockObj = new object();
        /// <summary>
        /// 모든 Pin Key를 Align한 후에 Out of range인 Tip이 검출 되었으면 서버로 당시에 체크했던 Tip Image들을 올린다.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum PinAlignResultServerUpload(List<PinSizeValidateResult> originList, List<PinSizeValidateResult> outList, List<PinSizeValidateResult> inList)
        {
            lock (pinImageUploadLockObj)
            {
                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                List<PinSizeValidateResult> validation_pin_list = new List<PinSizeValidateResult>();

                try
                {
                    LoggerManager.Debug($"PinAlignResultServerUpload Start");
                    if ((this.LoaderController() as GP_LoaderController).GPLoaderService != null) 
                    {
                        // log upload
                        ret = (this.LoaderController() as GP_LoaderController).GPLoaderService.LogUpload(this.LoaderController().GetChuckIndex(), EnumUploadLogType.PIN);

                        // image upload
                        string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "SIZE_VALIDATION");
                        string localzippath = @"C:\Logs\CellUpload" + "\\" + $"Cell{ this.LoaderController().GetChuckIndex().ToString().PadLeft(2, '0')}" + ".zip";
                        var localserverpath = @"C:\Logs\CellUpload" + "\\" + $"Cell{this.LoaderController().GetChuckIndex().ToString().PadLeft(2, '0')}";
                        bool isUploadFiles = false;

                        if (!Directory.Exists(SaveBasePath))
                        {
                            Directory.CreateDirectory(SaveBasePath);
                        }

                        string[] folders = Directory.GetDirectories(SaveBasePath);

                        if (folders.Length != 0)
                        {
                            foreach (var folder in folders)
                            {
                                validation_pin_list.Clear();

                                ret = EventCodeEnum.UNDEFINED;
                                string SaveBasePath_inside_folder = folder;

                                DirectoryInfo directory = new DirectoryInfo(SaveBasePath_inside_folder);
                                var files = directory.GetFiles();

                                string final_folder_name = new DirectoryInfo(SaveBasePath_inside_folder).Name;

                                if (final_folder_name == "ORIGINAL")
                                {
                                    foreach (var result in originList)
                                    {
                                        validation_pin_list.Add(result);
                                    }
                                }
                                else if (final_folder_name == "OUT_OF_RANGE")
                                {
                                    foreach (var result in outList)
                                    {
                                        validation_pin_list.Add(result);
                                    }
                                }
                                else if (final_folder_name == "SIZE_IN_RANGE")
                                {
                                    foreach (var result in inList)
                                    {
                                        validation_pin_list.Add(result);
                                    }
                                }
                                else
                                {
                                    //
                                }

                                foreach (var result in validation_pin_list)
                                {
                                    string uploadFile = FindUploadFile(SaveBasePath_inside_folder, result.filePath);
                                    byte[] pinTipimges = new byte[0];

                                    FileInfo fi = null;
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(uploadFile))
                                        {
                                            fi = new FileInfo(uploadFile);
                                            pinTipimges = File.ReadAllBytes(fi.FullName);
                                            string filename = Path.GetFileNameWithoutExtension(fi.FullName);

                                            //이번 Pin Align에 대한 결과 이미지만 따로 빼기
                                            string tempsubpath = localserverpath + "\\" + final_folder_name;
                                            if (!Directory.Exists(tempsubpath))
                                            {
                                                Directory.CreateDirectory(tempsubpath);
                                            }

                                            fi.CopyTo(tempsubpath + "\\" + $"{fi.Name}", true);
                                            isUploadFiles = true;
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                }
                                ret = EventCodeEnum.NONE;
                            }

                            if (isUploadFiles)
                            {
                                if (!File.Exists(localzippath))
                                {
                                    ZipFile.CreateFromDirectory(localserverpath, localzippath);
                                }

                                var image = File.ReadAllBytes(localzippath);

                                ret = (this.LoaderController() as GP_LoaderController).GPLoaderService.PINImageUploadLoaderToServer(this.LoaderController().GetChuckIndex(), image);

                                //파일들 삭제 하기 
                                File.Delete(localzippath);
                                Directory.Delete(localserverpath, true);
                            }
                            else
                            {
                                LoggerManager.Debug($"There are no image files to upload.");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"The file does not exist in {SaveBasePath}");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"Upload failed. Loader and cell are not connected.");
                    }
                    LoggerManager.Debug($"PinAlignResultServerUpload End");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return ret;
            }
        }
        private string FindUploadFile(string folder_path, string filePath)
        {
            string retVal = string.Empty;
            try
            {
                // 폴더 경로에서 모든 이미지 파일 가져오기
                string[] files = Directory.GetFiles(folder_path);
                string imageFile = string.Empty;
                // contain_Value 를 포함하고 있는 파일들 중에서 최근에 수정된 파일 찾기
                if (files.Length > 0)
                {
                    imageFile = files.Where(img_file => img_file == filePath)
                        .FirstOrDefault();
                }
                retVal = imageFile;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool CheckSubModulesInTheSkipstate(IProcessingModule subModule)
        {
            bool ret = false;
            try
            {
                var modules = this.Template.GetProcessingModule();
                var moduleIdx = modules.FindIndex(module => module.GetType().Name == subModule.GetType().Name);

                if (moduleIdx != -1)
                {
                    ret = modules.Take(moduleIdx).Any(module => module.GetState() == SubModuleStateEnum.SKIP);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
