using LogModule;
using PMIModuleLoggerStandard;
using PMIModuleParameter;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Enum;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.State;
using ProberInterfaces.Template;
using ProberInterfaces.Wizard;
using SerializerUtil;
using SharpDXRender;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace PMIModule
{
    using WinSize = System.Windows.Size;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PMIModule : IPMIModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }
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

        public ITemplateParam LoadTemplateParam { get; set; }

        #endregion

        public bool Initialized { get; set; } = false;

        private ISubRoutine _SubRoutine;
        public ISubRoutine SubRoutine
        {
            get { return _SubRoutine; }
            set
            {
                if (value != _SubRoutine)
                {
                    _SubRoutine = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.PMI);
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

        private DoPMIData _DoPMIInfo;
        public DoPMIData DoPMIInfo
        {
            get { return _DoPMIInfo; }
            set
            {
                if (value != _DoPMIInfo)
                {
                    _DoPMIInfo = value;
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

        //private PMIModuleSysParam _PMIModuleSysParam_Clone;
        //public PMIModuleSysParam PMIModuleSysParam_Clone
        //{
        //    get { return _PMIModuleSysParam_Clone; }
        //    set
        //    {
        //        if (value != _PMIModuleSysParam_Clone)
        //        {
        //            _PMIModuleSysParam_Clone = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private IFocusing _PMINormalFocusModel;
        public IFocusing PMINormalFocusModel
        {
            get
            {
                if (_PMINormalFocusModel == null)
                    _PMINormalFocusModel = this.FocusManager().GetFocusingModel(PMIDevParam.FocusingDllInfo.NormalFocusingInfo);

                return _PMINormalFocusModel;
            }
        }

        private IFocusing _PMIBumpFocusModel;
        public IFocusing PMIBumpFocusModel
        {
            get
            {
                if (_PMIBumpFocusModel == null)
                    _PMIBumpFocusModel = this.FocusManager().GetFocusingModel(PMIDevParam.FocusingDllInfo.BumpFocusingInfo);

                return _PMIBumpFocusModel;
            }
        }

        private IFocusParameter NormalFocusParam => PMIDevParam.NormalFocusParam;
        private IFocusParameter BumpFocusParam => PMIDevParam.BumpFocusParam;

        private PMIModuleDevParam PMIDevParam => PMIModuleDevParam_IParam as PMIModuleDevParam;
        private PMIModuleSysParam PMISysParam => PMIModuleSysParam_IParam as PMIModuleSysParam;

        private IParam _PMIModuleDevParam_IParam;
        public IParam PMIModuleDevParam_IParam
        {
            get { return _PMIModuleDevParam_IParam; }
            set
            {
                if (value != _PMIModuleDevParam_IParam)
                {
                    _PMIModuleDevParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _PMIModuleSysParam_IParam;
        public IParam PMIModuleSysParam_IParam
        {
            get { return _PMIModuleSysParam_IParam; }
            set
            {
                if (value != _PMIModuleSysParam_IParam)
                {
                    _PMIModuleSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public WaferObject Wafer { get; set; }

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


        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private CommandTokenSet _RunTokenSet = new CommandTokenSet();
        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        private ModuleStateBase _ModuleState; //모듈의 현재 대표 State
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

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

        private PMIModuleStateBase _PMIState;
        public PMIModuleStateBase PMIState
        {
            get { return _PMIState; }
            set
            {
                if (value != _PMIState)
                {
                    _PMIState = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : Dynamic load.
        private IPMIModuleLogger _PMILogger;
        public IPMIModuleLogger PMILogger
        {
            get { return _PMILogger; }
            set
            {
                if (value != _PMILogger)
                {
                    _PMILogger = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : Dynamic load.
        private IPMIMarkInformationAnalyzer _PMIMarkAnalyzer;
        public IPMIMarkInformationAnalyzer PMIMarkAnalyzer
        {
            get { return _PMIMarkAnalyzer; }
            set
            {
                if (value != _PMIMarkAnalyzer)
                {
                    _PMIMarkAnalyzer = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IInnerState InnerState // Inner모듈의 현재 State
        {
            get { return _PMIState; }
            private set
            {
                if (value != _PMIState)
                {
                    _PMIState = value as PMIModuleStateBase;
                }
            }
        }

        public IInnerState PreInnerState { get; set; } //Inner모듈의 이전 State
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

        public bool CanExecute(IProbeCommandToken token) // 커맨드가 날라왔을때 할수 있는지 보는 행동
        {
            throw new NotImplementedException();
        }

        public void DeInitModule() //DeInit해야할때 해야 하는 행동
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

        public EventCodeEnum InitModule() // Init해야할때 해야 하는 행동
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //Wafer = this.StageSupervisor().WaferObject as WaferObject;
                    //_ReasonOfError = new ReasonOfError(ModuleEnum.PMI);
                    CommandRecvSlot = new CommandSlot();
                    CommandSendSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    InnerState = new PMIIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    //PMIDevParam = DevParam as PMIModuleDevParam;

                    retval = this.TemplateManager().InitTemplate(this);

                    if (PMILogger == null)
                    {
                        PMILogger = new PMILoggerStandard();
                    }

                    if (DoPMIInfo == null)
                    {
                        DoPMIInfo = new DoPMIData();
                    }

                    if (PMIMarkAnalyzer == null)
                    {
                        PMIMarkAnalyzer = new PMIMarkInformationAnalyzer();
                    }

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }


            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        ///ITemplateExtension
        /// <summary>
        /// 설정에 포함되지 않은 메뉴 추가하기        
        public void InjectTemplate(ITemplateParam param)
        {
            ///패드 패턴 등록 메뉴 추가
            PadPatternRegistrationInjection(param);
        }

        /// <summary>
        /// 패드 패턴 등록 메뉴가 없다면 추가하기
        /// </summary>
        private static void PadPatternRegistrationInjection(ITemplateParam param)
        {

            var padRegCat = param?.TemlateModules?.Where(x => string.Compare(PMIModuleKeyWordInfo.CategoryPadReg, x.Header) == 0).FirstOrDefault();

            if (padRegCat is CategoryInfo i)
            {
                PMITemplateFile.CategoryModuleInjection(ref i, PMIModuleKeyWordInfo.ClassPatternReg);
            }

            var pnpButtons = param.ControlTemplates?.FirstOrDefault()?.ControlPNPButtons?.FirstOrDefault();
            if (pnpButtons != null)
            {
                PMITemplateFile.PNPButtonInjection(ref pnpButtons, PMIModuleKeyWordInfo.ScreenGUID_PatternInPadRegistration);
            }
        }



        public EventCodeEnum InitPMIResult()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                long maxX, minX, maxY, minY;
                long xNum, yNum;

                long offsetx = 0;
                long offsety = 0;

                var devices = this.StageSupervisor().WaferObject.GetSubsInfo().Devices;
                var dies = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs;

                maxX = devices.Max(d => d.DieIndexM.XIndex);
                minX = devices.Min(d => d.DieIndexM.XIndex);
                maxY = devices.Max(d => d.DieIndexM.YIndex);
                minY = devices.Min(d => d.DieIndexM.YIndex);

                xNum = maxX - minX + 1;
                yNum = maxY - minY + 1;

                if (minX < 0)
                    offsetx = Math.Abs(minX);
                if (minY < 0)
                    offsety = Math.Abs(minY);

                Parallel.For(minY, maxY + 1, y =>
                {
                    Parallel.For(minX, maxX + 1, x =>
                    {
                        //DIEs[x, y] = Devices.ToList<DeviceObject>().Find(
                        //    d => d.DieIndexM.XIndex.Value == x & d.DieIndexM.YIndex.Value == y);

                        foreach (var pad in dies[x + offsetx, y + offsety].Pads.PMIPadInfos)
                        {
                            pad.PMIResults.Clear();
                        }
                    });
                });

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ResetPMIData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // 현재 웨이퍼가 새로 로드 됐을 때, 호출 된다.
                // 이전 데이터 초기화.

                DoPMIInfo.ProcessedPMIMIndex.Clear();
                DoPMIInfo.ReservedPMIMIndex.Clear();

                DoPMIInfo.IsTurnAutoLight = false;
                DoPMIInfo.IsTurnFocusing = false;
                DoPMIInfo.IsTurnOnMarkAlign = false;

                DoPMIInfo.AllPassPadCount = 0;
                DoPMIInfo.AllFailPadCount = 0;
                DoPMIInfo.Result = EventCodeEnum.UNDEFINED;

                DoPMIInfo.RememberLastRemaingPMIDies = -1;

                retval = InitPMIResult();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
            }

            return InnerState.GetModuleState();

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

        public void InnerStateTransition(IInnerState state) // Don`t Touch
        {
            try
            {

                if (state != null)
                {
                    PreInnerState = InnerState;
                    InnerState = state;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StateTransition(ModuleStateBase state) // Don`t Touch
        {
            ModuleState = state;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                PMIStateEnum state = (InnerState as PMIModuleState).GetState();

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

                //foreach (var subModule in SubModules.SubModules)
                //{
                //    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                //    {
                //        retVal = true;
                //    }
                //}
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

            }
            return retVal;
        }

        #region Parameter Load and Save Method
        public EventCodeEnum LoadDevParameter() // Parameter Type만 변경
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new PMIModuleDevParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(PMIModuleDevParam));

                if (retval == EventCodeEnum.NONE)
                {
                    PMIModuleDevParam_IParam = tmpParam;
                    //PMIModuleDevParam_Clone = PMIModuleDevParam_IParam as PMIModuleDevParam;
                }

                tmpParam = new PMITemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(PMITemplateFile));

                if (retval == EventCodeEnum.NONE)
                {
                    TemplateParameter = tmpParam as PMITemplateFile;
                }

                if (DoPMIInfo == null)
                {
                    DoPMIInfo = new DoPMIData();
                }

                //DoPMIInfo.FocusingModule = PMINormalFocusModel;
                //DoPMIInfo.FocusingParam = NormalFocusParam;

                //DevParam = PMIDevParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveDevParameter() // Don`t Touch
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(PMIModuleDevParam_IParam);
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

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new PMIModuleSysParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(PMIModuleSysParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    PMIModuleSysParam_IParam = tmpParam;
                    //PMIModuleSysParam_Clone = PMIModuleSysParam_IParam as PMIModuleSysParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(PMISysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #endregion

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            this.TemplateManager().InitTemplate(this);
            return TemplateToPnpConverter.Converter(Template.TemplateModules);
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool retVal = false;
            var PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();
            //bool flag = true;

            try
            {
                PMIModuleDevParam DevParam = PMIModuleDevParam_IParam as PMIModuleDevParam;
                PMIModuleSysParam SysParam = PMIModuleSysParam_IParam as PMIModuleSysParam;

                // Check Device parameter
                if (PMIDevParam == null)
                {
                    retVal = false;
                    msg = "PMI Device parameter is empty";

                    return retVal;
                }

                // Check System parameter
                if (SysParam == null)
                {
                    retVal = false;
                    msg = "PMI System parameter is empty";

                    return retVal;
                }

                // Check PMI Operation mode & Check PMI Map Info
                if (PMIDevParam.NormalPMI.Value == OP_MODE.Disable)
                {
                    retVal = true;
                    msg = "PMI is disabled.";

                    return retVal;
                }
                else if (PMIDevParam.NormalPMI.Value == OP_MODE.Enable)
                {
                    var PMIMap = PMIInfo.NormalPMIMapTemplateInfo;

                    if ((PMIMap == null) || (PMIMap.Count == 0))
                    {
                        retVal = false;
                        msg = "Normal PMI Map is not ready.";

                        return retVal;
                    }
                }

                if ((PMIInfo.PadTemplateInfo == null) || (PMIInfo.PadTemplateInfo.Count == 0))
                {
                    retVal = false;
                    msg = "PMI Pad Template is not ready.";

                    return retVal;
                }

                // Check Template : 최소 1개의 Template이 존재 해야 함.
                // Check PMI Wafer infomation
                if ((PMIInfo.WaferTemplateInfo == null) || (PMIInfo.WaferTemplateInfo.Count == 0))
                {
                    retVal = false;
                    msg = "PMI Wafer Template is not ready.";

                    return retVal;
                }

                // Check PMI Pad Table infomation
                if ((PMIInfo.PadTableTemplateInfo == null) || (PMIInfo.PadTableTemplateInfo.Count == 0))
                {
                    retVal = false;
                    msg = "PMI Pad Table Template is not ready.";

                    return retVal;
                }

                // (1) 현재 사용된 Wafer Map Template 획득
                // (2) 획득된 Wafer Map Template에서 사용된 Table Template 획득
                // (3) 획득된 Table Template

                // 검색 된 Table의 Grouping Data가 모두 존재하는지 
                // 모든 사용된 Table의 Grouping Data가 존재하면 Pass
                // 존재하지 않는 경우, Groping Data를 만들고, 모두 정상적으로 만들어진 경우 Pass
                // 그렇지 않은 경우, Fail

                //List<int> UsedWaferTemplateIndex = new List<int>();
                //List<int> UsedTableTemplateIndex = new List<int>();

                //foreach (var WaferTemplate in PMIInfo.WaferTemplateInfo)
                //{
                //    if (WaferTemplate.PMIEnable.Value == true)
                //    {
                //        if (UsedWaferTemplateIndex.Contains(WaferTemplate.SelectedMapIndex.Value) == false)
                //        {
                //            UsedWaferTemplateIndex.Add(WaferTemplate.SelectedMapIndex.Value);
                //        }
                //    }
                //}

                //var devices = this.StageSupervisor().WaferObject.GetSubsInfo().Devices;

                //foreach (var index in UsedWaferTemplateIndex)
                //{
                //    var map = PMIInfo.GetNormalPMIMapTemplate(index);

                //    if ((map.MapWidth > 0) && (map.MapHeight > 0))
                //    {
                //        foreach (var die in devices)
                //        {
                //            if (die.DieType.Value == DieTypeEnum.TEST_DIE)
                //            {
                //                var Xind = (int)die.DieIndexM.XIndex;
                //                var Yind = (int)die.DieIndexM.YIndex;

                //                if (map.GetEnable(Xind, Yind) == 0x01)
                //                {
                //                    var tempTableIndex = map.GetTable(Xind, Yind);

                //                    if (UsedTableTemplateIndex.Contains(tempTableIndex) == false)
                //                    {
                //                        UsedTableTemplateIndex.Add(tempTableIndex);
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                //var PadGropingTemplates = PMIInfo.PadGroupTemplateInfo.ToList();

                //// (1) 해당 Table의 Enable 개수와 Grouping Data 패드 개수 일치 해야 함.
                //// (2) Grouping Data와 Enable Index 도 일치 

                //List<int> UsedPadRealIndex = new List<int>();

                //foreach (var tableNum in UsedTableTemplateIndex)
                //{
                //    UsedPadRealIndex.Clear();

                //    var Padtable = PMIInfo.GetPadTableTemplate(tableNum);

                //    // True Count

                //    int EnableCount = Padtable.Enable.Where(x => x).Count();

                //    if (EnableCount > 0)
                //    {
                //        PadGroupTemplate grouptemplate = PadGropingTemplates.Find(x => x.UsedTableIndex.Value == tableNum);

                //        if (grouptemplate != null)
                //        {
                //            long padsum = 0;

                //            foreach (var group in grouptemplate.Groups)
                //            {
                //                padsum += group.PadDataInGroup.Count;

                //                foreach (var pad in group.PadDataInGroup)
                //                {
                //                    UsedPadRealIndex.Add(pad.PadRealIndex.Value);
                //                }
                //            }

                //            if(EnableCount == padsum)
                //            {
                //                // TODO : Need to compare RealIndex

                //                for (int i = 0; i < Padtable.Enable.Count; i++)
                //                {
                //                    if(Padtable.Enable[i] == true)
                //                    {
                //                        if (UsedPadRealIndex.Contains(i) == false)
                //                        {
                //                            retVal = false;

                //                            msg = "PMI Pad Groping Data is wrong (1).";

                //                            return retVal;
                //                        }
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                retVal = false;

                //                msg = "PMI Pad Groping Data is wrong (2).";

                //                return retVal;
                //            }
                //        }
                //    }
                //}

                //// Check PMI Pad Group infomation
                //if (PMIInfo.PadGroupingDone == false)
                //{
                //    List<PMIGroupData> PMIPadGroups = new List<PMIGroupData>();

                //    for (int i = 0; i < PMIInfo.PadTableInfo.Count; i++)
                //    {
                //        if (PadGroupingMethod(i, ref PMIPadGroups) != EventCodeEnum.NONE)
                //        {
                //            flag = false;
                //            break;
                //        }
                //    }

                //    if (flag == false)
                //    {
                //        PMIInfo.PadGroupingDone = false;

                //        retVal = false;
                //        msg = "PMI Pad Grouping Failed.";

                //        return retVal;
                //    }
                //    else
                //    {
                //        PMIInfo.PadGroupingDone = true;
                //    }
                //}

                //if (PMIInfo.PadGroupingDone == true)
                //{
                //    if ((PMIInfo.PadGroupInfo == null) || (PMIInfo.PadGroupInfo.Count == 0))
                //    {
                //        retVal = false;
                //        msg = "PMI Pad Group infomation is not ready.";

                //        return retVal;
                //    }
                //    else
                //    {
                //        foreach (var groupInfo in PMIInfo.PadGroupInfo)
                //        {
                //            if (PMIInfo.CheckUsingPadExistInTable(groupInfo.UsedTableIndex))
                //            {
                //                if (groupInfo.Groups.Count < 1)
                //                {
                //                    retVal = false;
                //                    msg = "PMI Pad Group infomation is not ready.";

                //                    return retVal;
                //                }
                //            }
                //        }
                //    }
                //}

                retVal = true;
                msg = "PMI is ready for Lot run";
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModule] [IsLotReady()] : {err}");

                msg = "PMI is not ready for Lot run";
                retVal = false;

                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }
        #region SubRoutine Wrapping Method

        //public PadTemplate GetCurrentTempalte()
        //{
        //    PadTemplate retval = null;

        //    if (SubRoutine is IPMIModuleSubRutine)
        //    {
        //        retval = (SubRoutine as IPMIModuleSubRutine).GetCurrentTempalte();
        //    }

        //    return retval;
        //}

        public RenderLayer InitPMIRenderLayer(WinSize size, float r, float g, float b, float a)
        {
            RenderLayer retval = null;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).InitPMIRenderLayer(size, r, g, b, a);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public WinSize GetLayerSize()
        {
            WinSize retval = new WinSize();
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).GetLayerSize();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        //public void ChangeTemplateIndexCommand(SETUP_DIRECTION direction)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeTemplateIndexCommand(direction);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangeTemplateColorCommand(PAD_COLOR padColor)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeTemplateColorCommand(padColor);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangeTemplateSizeCommand(JOG_DIRECTION direction)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeTemplateSizeCommand(direction);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangeTemplateOffsetCommand(SETUP_DIRECTION direction)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeTemplateOffsetCommand(direction);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangeTemplateAngleCommand()
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeTemplateAngleCommand();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangeJudgingWindowSizeCommand(JOG_DIRECTION direction)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeJudgingWindowSizeCommand(direction);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangeMarkSizeCommand(MARK_SIZE curMode, JOG_DIRECTION direction)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangeMarkSizeCommand(curMode, direction);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void ChangePadPositionCommand(SELECTION_MODE mode, JOG_DIRECTION direction)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).ChangePadPositionCommand(mode, direction);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        //public void AddTemplateCommand(PAD_SHAPE shape, string name, PAD_COLOR color, double offset = 0.1)
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).AddTemplateCommand(shape, name, color, offset);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //public void DeleteTemplateCommand()
        //{
        //    try
        //    {
        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).DeleteTemplateCommand();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //}

        public void SetSubModule(object SubModule)
        {
            try
            {
                if (SubRoutine is IPMIModuleSubRutine)
                {
                    (SubRoutine as IPMIModuleSubRutine).SetSubModule(SubModule);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public void UpdateCurrentPadIndex()
        {
            try
            {
                if (SubRoutine is IPMIModuleSubRutine)
                {
                    (SubRoutine as IPMIModuleSubRutine).UpdateCurrentPadIndex();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public void UpdateRenderLayer()
        {
            try
            {
                if (SubRoutine is IPMIModuleSubRutine)
                {
                    (SubRoutine as IPMIModuleSubRutine).UpdateRenderLayer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public bool CheckFocusingInterval(int DutNo)
        {
            bool retval = false;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).CheckFocusingInterval(DutNo);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        public Point GetRenderLayerRatio()
        {
            Point retval = new Point();
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).GetRenderLayerRatio();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        //public RenderLayer GetPMIRenderLayer()
        //{
        //    RenderLayer retval = null;

        //    if (SubRoutine is IPMIModuleSubRutine)
        //    {
        //        retval = (SubRoutine as IPMIModuleSubRutine).GetPMIRenderLayer();
        //    }

        //    return retval;
        //}

        public EventCodeEnum MovedDelegate(ImageBuffer Img)
        {
            if (SubRoutine is IPMIModuleSubRutine)
            {
                (SubRoutine as IPMIModuleSubRutine).MovedDelegate(Img);
            }

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoPMI()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).DoPMI(DoPMIInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        public EventCodeEnum EnterMovePadPosition(ref PMIPadObject MovedPadInfo)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).EnterMovePadPosition(ref MovedPadInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public EventCodeEnum DoPMIProcessing()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().UpdatePadTableTemplateInfo();

        //        UpdateGroupingInformation();

        //        EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //        this.PMIModule().InitPMIResult();

        //        // TODO: Mode에 따라서, 반복 동작해야 됨. Die Mode or Dut Mode
        //        // 현재는 Die Mode만 고려 됨, 카메라가 현재 보고 있는 Die Index를 얻어 옴
        //        //MachineIndex MI = CurCam.CamSystemMI;

        //        var param = PMIModuleDevParam_IParam as PMIModuleDevParam;
        //        var DoPMIInfo = this.PMIModule().DoPMIInfo;

        //        DoPMIInfo.ProcessedPMIMIndex.Clear();
        //        //DoPMIInfo.ProcessedPMIMIndex.Add(CurCam.CamSystemMI);
        //        //var devices = this.StageSupervisor().WaferObject.PMIInfo.NormalPMIMapTemplateInfo;
        //        IWaferObject waferObject = this.StageSupervisor().WaferObject;

        //        var devices = waferObject.GetSubsInfo().DIEs;

        //        foreach (var dev in this.StageSupervisor().WaferObject.GetDevices())
        //        {
        //            if (waferObject.PMIInfo.NormalPMIMapTemplateInfo[0].GetEnable((int)dev.DieIndexM.XIndex, (int)dev.DieIndexM.YIndex) == 0x01)
        //            {
        //                DoPMIInfo.ProcessedPMIMIndex.Add(dev.DieIndexM);
        //                LoggerManager.Debug($"EnalbePMIPad : X {dev.DieIndexM.XIndex}, Y : {dev.DieIndexM.YIndex}");
        //            }
        //        }

        //        // Light
        //        if (param.AutoLightEnable.Value == true)
        //        {
        //            DoPMIInfo.IsTurnAutoLight = true;
        //        }
        //        else
        //        {
        //            DoPMIInfo.IsTurnAutoLight = false;
        //        }

        //        // Focusing
        //        if (param.FocusingEnable.Value == true)
        //        {
        //            //Module.DoPMIInfo.IsTurnFocusing = Module.CheckFocusingInterval(index);
        //            DoPMIInfo.IsTurnFocusing = true;
        //        }
        //        else
        //        {
        //            DoPMIInfo.IsTurnFocusing = false;
        //        }

        //        DoPMIInfo.WaferMapIndex = 0;
        //        DoPMIInfo.WorkMode = PMIWORKMODE.MANUAL;

        //        retval = this.PMIModule().DoPMI();

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}

        public EventCodeEnum PadGroupingMethod(int TableNumber)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).PadGroupingMethod(TableNumber);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        public void ClearRenderObjects()
        {
            try
            {
                if (SubRoutine is IPMIModuleSubRutine)
                {
                    (SubRoutine as IPMIModuleSubRutine).ClearRenderObjects();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public EventCodeEnum MakeGroupSequence(int TableIndex)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).MakeGroupSequence(TableIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        public EventCodeEnum MoveToPad(ICamera CurCam, MachineIndex Mi, int padIndex)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).MoveToPad(CurCam, Mi, padIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retval;
        }

        public EventCodeEnum MoveToMark(ICamera CurCam, MachineIndex Mi, int padIndex, int markIndex)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retval = (SubRoutine as IPMIModuleSubRutine).MoveToMark(CurCam, Mi, padIndex, markIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadTemplate()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveTemplate()
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<IWizardStep> GetTemplate()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetTemplate(int index)
        {
            throw new NotImplementedException();
        }

        public List<MachineIndex> GetRemainingPMIDies()
        {
            List<MachineIndex> retval = new List<MachineIndex>();

            try
            {
                IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                int curWaferNum = (this.PMIModule().LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart - 1) % 25;

                if (curWaferNum < 0 || curWaferNum >= PMIInfo.WaferTemplateInfo.Count)
                {
                    LoggerManager.Error($"Invalid curWaferNum: {curWaferNum}, Collection Size: {PMIInfo.WaferTemplateInfo.Count}");
                }

                int curMapIndex = PMIInfo.WaferTemplateInfo[curWaferNum].SelectedMapIndex.Value;

                if (curMapIndex < 0 || curMapIndex >= PMIInfo.NormalPMIMapTemplateInfo.Count)
                {
                    LoggerManager.Error($"Invalid curMapIndex: {curMapIndex}, Collection Size: {PMIInfo.NormalPMIMapTemplateInfo.Count}");
                }

                var PMIMap = PMIInfo.NormalPMIMapTemplateInfo[curMapIndex];

                foreach (var dut in this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Select((value, i) => new { i, value }))
                {
                    var value = dut.value;
                    var index = dut.i;

                    MachineIndex MI = value.DieIndexM;

                    // 이미 진행했거나, 옵션에 따라 Reserved 되어 있지 않으면
                    if (!IsDieInspectedOrReserved(MI))
                    {
                        if (value.State.Value == DieStateEnum.TESTED && PMIMap.GetEnable((int)MI.XIndex, (int)MI.YIndex) == 0x01)
                        {
                            retval.Add(MI);
                        }
                    }
                }

                HandlePMITriggerOption(retval);

                if (DoPMIInfo.RememberLastRemaingPMIDies != retval.Count)
                {
                    LoggerManager.Debug($"[PMIModule], GetRemainingPMIDies() : Tested Count : {this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count}, Remaining Count : {retval.Count}");

                    DoPMIInfo.RememberLastRemaingPMIDies = retval.Count;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private bool IsDieInspectedOrReserved(MachineIndex MI)
        {
            bool retval = false;

            try
            {
                MachineIndex alreadyProcessedMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.FirstOrDefault(x => x.XIndex == MI.XIndex && x.YIndex == MI.YIndex);
                MachineIndex reservedMI = this.PMIModule().DoPMIInfo.ReservedPMIMIndex.FirstOrDefault(x => x.XIndex == MI.XIndex && x.YIndex == MI.YIndex);

                retval = alreadyProcessedMI != null || reservedMI != null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void HandlePMITriggerOption(List<MachineIndex> retval)
        {
            try
            {
                if (PMIDevParam.TriggerComponent.ExecuteAfterWaferProcessed.Value)
                {
                    int probingSequenceRemainCount = this.ProbingSequenceModule().ProbingSequenceRemainCount;

                    if (probingSequenceRemainCount > 0)
                    {
                        // 시퀀스가 남아 있는 경우
                        if (retval.Count > 0)
                        {
                            foreach (var mi in retval)
                            {
                                if (!this.PMIModule().DoPMIInfo.ReservedPMIMIndex.Any(existingMi => existingMi.XIndex == mi.XIndex && existingMi.YIndex == mi.YIndex))
                                {
                                    this.PMIModule().DoPMIInfo.ReservedPMIMIndex.Add(mi);
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{this.GetType().Name}], HandlePMITriggerOption() : Attempted to add duplicate MachineIndex with XIndex = {mi.XIndex}, YIndex = {mi.YIndex}");
                                }
                            }

                            retval.Clear();
                        }
                    }
                    else
                    {
                        // 시퀀스가 0개인 경우
                        if (this.PMIModule().DoPMIInfo.ReservedPMIMIndex.Count > 0)
                        {
                            // ReservedPMIMIndex를 역순으로 추가
                            List<MachineIndex> reversedList = new List<MachineIndex>(this.PMIModule().DoPMIInfo.ReservedPMIMIndex);
                            reversedList.Reverse();

                            foreach (var mi in reversedList)
                            {
                                // Check if the MachineIndex is already processed
                                if (!this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Any(processedMi => processedMi.XIndex == mi.XIndex && processedMi.YIndex == mi.YIndex))
                                {
                                    retval.Add(mi);
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

        public bool IsTurnOnPMIInLotRun()
        {
            bool retVal = false;

            try
            {
                if (ForcedDone == EnumModuleForcedState.Normal)
                {
                    if (DoPMIInfo.RemoteOperation == PMIRemoteOperationEnum.ITSELF || DoPMIInfo.RemoteOperation == PMIRemoteOperationEnum.FORCEDEXECUTE)
                    {
                        if ((GetPMIEnableParam()) && (ModuleState.GetState() != ModuleStateEnum.DONE))
                        {
                            if (DoPMIInfo.RemoteOperation == PMIRemoteOperationEnum.ITSELF)
                            {
                                if (DoPMIInfo.PMITrigger == PMIRemoteTriggerEnum.EVERY_WAFER_TRIGGER ||
                                   DoPMIInfo.PMITrigger == PMIRemoteTriggerEnum.TOTALNUMBER_WAFER_TRIGGER)
                                {
                                    retVal = true;
                                }
                                else
                                {
                                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                                    {
                                        if (CheckCurWaferPMIEnable() || CheckCurWaferInterval() || CheckCurTouchdownCount())
                                        {
                                            retVal = true;
                                        }
                                        else
                                        {
                                            retVal = false;
                                        }
                                    }
                                    else
                                    {
                                        if (CheckCurTouchdownCount())
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
                            else if (DoPMIInfo.RemoteOperation == PMIRemoteOperationEnum.FORCEDEXECUTE)
                            {
                                retVal = true;
                            }
                            else
                            {
                                LoggerManager.Error("Unknown branch. Please check if logic.");

                                retVal = false;
                            }
                        }
                    }
                    else if (DoPMIInfo.RemoteOperation == PMIRemoteOperationEnum.SKIP)
                    {
                        retVal = false;
                    }
                    else // UNDEFINED, Host의 명령과 관계 없이 디바이스 데이터에 의해 Trigger 된 경우
                    {
                        if ((GetPMIEnableParam()) && (ModuleState.GetState() != ModuleStateEnum.DONE))
                        {
                            if (DoPMIInfo.PMITrigger == PMIRemoteTriggerEnum.EVERY_WAFER_TRIGGER || DoPMIInfo.PMITrigger == PMIRemoteTriggerEnum.TOTALNUMBER_WAFER_TRIGGER)
                            {
                                retVal = true;
                            }
                            else
                            {
                                retVal = false;
                            }
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                }
                else
                {
                    // SKIP
                    retVal = false;
                }

                if (retVal)
                {
                    // 등록 된 다이가 존재하는지
                    // 등록 된 패드가 존재하는지

                    List<MachineIndex> remainPMIDiesCount = GetRemainingPMIDies();
                    bool CheckPad = CheckPMIPadExist();

                    if (remainPMIDiesCount != null && remainPMIDiesCount.Count > 0 && CheckPad)
                    {
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// 수행 된, PMI 결과에 따라 반환 값 할당.
        /// 성공 = True
        /// 실패 = false
        /// </summary>
        /// <returns></returns>
        public bool GetPMIResult()
        {
            bool retval = false;

            try
            {
                // TODO : 성공과 실패의 기준을 위해 사용되는 데이터?

                if (DoPMIInfo.Result == EventCodeEnum.NONE)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public bool IsLastProbingSeqProceessd()
        {
            bool retVal = false;

            MachineIndex LastMI = this.ProbingModule().ProbingLastMIndex;

            var list = DoPMIInfo.ProcessedPMIMIndex.Where(x => (x.XIndex == LastMI.XIndex) && (x.YIndex == LastMI.YIndex));

            if (list.Count() > 0)
            {
                retVal = true;
            }

            return retVal;
        }

        public bool CheckPMIPadExist()
        {
            bool retVal = false;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retVal = (SubRoutine as IPMIModuleSubRutine).CheckPMIPadExist();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        //public bool CheckPMIDieExist()
        //{
        //    bool retVal = false;
        //    try
        //    {

        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            retVal = (SubRoutine as IPMIModuleSubRutine).CheckPMIDieExist();
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //    return retVal;
        //}

        public bool CheckCurTouchdownCount()
        {
            bool retVal = false;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retVal = (SubRoutine as IPMIModuleSubRutine).CheckCurTouchdownCount();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public bool CheckCurWaferInterval()
        {
            bool retVal = false;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retVal = (SubRoutine as IPMIModuleSubRutine).CheckCurWaferInterval();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public bool CheckCurWaferPMIEnable()
        {
            bool retVal = false;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retVal = (SubRoutine as IPMIModuleSubRutine).CheckCurWaferPMIEnable();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public EventCodeEnum FindPad(PadTemplate padtemplate)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (SubRoutine is IPMIModuleSubRutine)
                {
                    retVal = (SubRoutine as IPMIModuleSubRutine).FindPad(padtemplate);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        ///// <summary>
        ///// 선택된 패드 index를 변경하는 함수
        ///// </summary>
        ///// <param name="direction"></param>
        //public async void PadIndexMoveCommand(object direction)
        //{
        //    try
        //    {
        //        if (PadInfos.PMIPadInfos.Count > 0)
        //        {
        //            switch ((SETUP_DIRECTION)direction)
        //            {
        //                case SETUP_DIRECTION.PREV:

        //                    if (PadInfos.SelectedPMIPadIndex > 0)
        //                    {
        //                        PadInfos.SelectedPMIPadIndex--;
        //                    }
        //                    else
        //                    {
        //                        PadInfos.SelectedPMIPadIndex = PadInfos.PMIPadInfos.Count - 1;
        //                    }
        //                    break;
        //                case SETUP_DIRECTION.NEXT:

        //                    if (PadInfos.SelectedPMIPadIndex + 1 < PadInfos.PMIPadInfos.Count)
        //                    {
        //                        PadInfos.SelectedPMIPadIndex++;
        //                    }
        //                    else
        //                    {
        //                        PadInfos.SelectedPMIPadIndex = 0;
        //                    }

        //                    break;
        //                default:
        //                    break;
        //            }

        //            object submodule;

        //            if (SubRoutine is IPMIModuleSubRutine)
        //            {
        //                submodule = (SubRoutine as IPMIModuleSubRutine).GetSubModule();

        //                if (submodule != null)
        //                {
        //                    var pnpmodule = (submodule as IPnpSetup);

        //                    if (pnpmodule != null)
        //                    {
        //                        MoveToPad(pnpmodule.CurCam, PadInfos.SelectedPMIPadIndex);

        //                        UpdateRenderLayer();

        //                        if (SubRoutine is IPMIModuleSubRutine)
        //                        {
        //                            (SubRoutine as IPMIModuleSubRutine).UpdateLabel();
        //                        }
        //                        else
        //                        {
        //                            LoggerManager.Error($"[PMIModule] [PadIndexMoveCommand()] : Wrong Logic process.");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        LoggerManager.Error($"[PMIModule] [PadIndexMoveCommand()] : Wrong Logic process.");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                LoggerManager.Error($"[PMIModule] [PadIndexMoveCommand()] : Wrong Logic process.");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIModule] [PadIndexMoveCommand(" + $"{direction}" + $")] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}

        ///// <summary>
        ///// 선택된 Table index를 변경하는 함수
        ///// </summary>
        ///// <param name="direction"></param>
        //public async void TableIndexMoveCommand(object direction)
        //{
        //    try
        //    {
        //        switch ((SETUP_DIRECTION)direction)
        //        {
        //            case SETUP_DIRECTION.PREV:

        //                if (PMIInfo.SelectedPadTableTemplateIndex > 0)
        //                {
        //                    PMIInfo.SelectedPadTableTemplateIndex--;

        //                    //PMIInfo.SelectedPadTableTemplateIndex = PMIInfo.SelectedPadTableTemplateIndex;
        //                }
        //                else
        //                {
        //                    PMIInfo.SelectedPadTableTemplateIndex = PMIInfo.PadTableTemplateInfo.Count() - 1;

        //                    //PMIInfo.SetSelectedPadTableTemplateIndex(PMIInfo.GetPadTableTemplateInfo().Count - 1);
        //                }
        //                break;

        //            case SETUP_DIRECTION.NEXT:

        //                if (PMIInfo.SelectedPadTableTemplateIndex < PMIInfo.PadTableTemplateInfo.Count - 1)
        //                {
        //                    PMIInfo.SelectedPadTableTemplateIndex++;

        //                    //PMIInfo.SetSelectedPadTableTemplateIndex(PMIInfo.SelectedPadTableTemplateIndex);
        //                }
        //                else
        //                {
        //                    PMIInfo.SelectedPadTableTemplateIndex = 0;

        //                    //PMIInfo.SetSelectedPadTableTemplateIndex(0);
        //                }

        //                break;

        //            default:
        //                break;
        //        }

        //        UpdateRenderLayer();

        //        if (SubRoutine is IPMIModuleSubRutine)
        //        {
        //            (SubRoutine as IPMIModuleSubRutine).UpdateLabel();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadTableSetupModule] [TableIndexMoveCommand(" + $"{direction}" + $")] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}

        #endregion

        public EventCodeEnum UpdateDisplayedDevices(ICamera Curcam, bool InitPads = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                //{
                //    return EventCodeEnum.NONE;
                //}

                WaferCoordinate LUcorner = new WaferCoordinate();
                WaferCoordinate RDcorner = new WaferCoordinate();

                double[,] DieCornerPos = new double[2, 2];
                double[,] DisplayCornerPos = new double[2, 2];

                DisplayCornerPos[0, 0] = Curcam.CamSystemPos.X.Value - (Curcam.GetRatioX() * (Curcam.GetGrabSizeWidth() / 2));
                DisplayCornerPos[0, 1] = Curcam.CamSystemPos.Y.Value - (Curcam.GetRatioY() * (Curcam.GetGrabSizeHeight() / 2));

                DisplayCornerPos[1, 0] = Curcam.CamSystemPos.X.Value + (Curcam.GetRatioX() * (Curcam.GetGrabSizeWidth() / 2));
                DisplayCornerPos[1, 1] = Curcam.CamSystemPos.Y.Value + (Curcam.GetRatioY() * (Curcam.GetGrabSizeHeight() / 2));

                LUcorner.X.Value = DisplayCornerPos[0, 0];
                LUcorner.Y.Value = DisplayCornerPos[0, 1];

                RDcorner.X.Value = DisplayCornerPos[1, 0];
                RDcorner.Y.Value = DisplayCornerPos[1, 1];

                MachineIndex LUcornerUI = this.CoordinateManager().GetCurMachineIndex(LUcorner);
                MachineIndex RDcornerUI = this.CoordinateManager().GetCurMachineIndex(RDcorner);

                //MachineIndex CurDieUI = this.CoordinateManager().GetCurMachineIndex(Curcam.CamSystemPos as WaferCoordinate);
                MachineIndex CurDieUI = Curcam.CamSystemMI;

                if ((LUcornerUI.XIndex >= 0) &&
                    (LUcornerUI.YIndex >= 0) &&
                    (RDcornerUI.XIndex >= 0) &&
                    (RDcornerUI.XIndex >= 0)
                    )
                {
                    // Start Index
                    double DieStartXIndex = (LUcornerUI.XIndex > RDcornerUI.XIndex) ? RDcornerUI.XIndex : LUcornerUI.XIndex;
                    double DieStartYIndex = (LUcornerUI.YIndex > RDcornerUI.YIndex) ? RDcornerUI.YIndex : LUcornerUI.YIndex;

                    // End Index
                    double DieEndXIndex = (LUcornerUI.XIndex > RDcornerUI.XIndex) ? LUcornerUI.XIndex : RDcornerUI.XIndex;
                    double DieEndYIndex = (LUcornerUI.YIndex > RDcornerUI.YIndex) ? LUcornerUI.YIndex : RDcornerUI.YIndex;

                    this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.Clear();

                    //for (int y = (int)DieStartYIndex; y <= (int)DieEndYIndex; y++)
                    //{
                    //    for (int x = (int)DieStartXIndex; x <= (int)DieEndXIndex; x++)

                    var Xlength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(0);
                    var Ylength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(1);

                    for (int y = (int)DieStartYIndex; y <= (int)DieEndYIndex; y++)
                    {
                        for (int x = (int)DieStartXIndex; x <= (int)DieEndXIndex; x++)
                        {
                            if ((x >= Xlength) || (y >= Ylength))
                            {

                            }
                            else
                            {
                                DeviceObject targetdev = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[x, y] as DeviceObject;

                                List<PMIPadObject> PMiPadBackup = new List<PMIPadObject>();

                                foreach (var item in targetdev.Pads.PMIPadInfos.ToList())
                                {
                                    PMIPadObject tmp = new PMIPadObject();

                                    item.CopyTo(tmp);

                                    PMiPadBackup.Add(tmp);
                                }

                                this.StageSupervisor().WaferObject.GetSubsInfo().UpdatePadsToDevice(targetdev);

                                this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.Add(targetdev);

                                foreach (var item in this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.Last().Pads.PMIPadInfos.ToList())
                                {
                                    PMIPadObject tmp = PMiPadBackup.FirstOrDefault(i => i.Index == item.Index);

                                    if (tmp != null)
                                    {
                                        item.PMIResults = tmp.PMIResults;
                                    }
                                }

                                // Current Die Index
                                if ((x == CurDieUI.XIndex) && (y == CurDieUI.YIndex))
                                {
                                    this.StageSupervisor().WaferObject.GetSubsInfo().CurrentDie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[x, y] as DeviceObject;
                                }
                            }

                        }
                    }

                    //Parallel.For((int)DieStartYIndex, (int)DieEndYIndex + 1, y =>
                    //{
                    //    Parallel.For((int)DieStartXIndex, (int)DieEndXIndex + 1, x =>
                    //    {
                    //        DeviceObject tmp = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[x, y] as DeviceObject;

                    //        this.StageSupervisor().WaferObject.GetSubsInfo().UpdatePadsToDevice(tmp);

                    //        this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.Add(tmp);

                    //        // Current Die Index
                    //        if ((x == CurDieUI.XIndex) && (y == CurDieUI.YIndex))
                    //        {
                    //            this.StageSupervisor().WaferObject.GetSubsInfo().CurrentDie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[x, y] as DeviceObject;
                    //        }
                    //    });
                    //});
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetPMIEnableParam()
        {
            bool retval = false;

            try
            {
                if (PMIDevParam.NormalPMI.Value == OP_MODE.Disable)
                {
                    retval = false;
                }
                else if (PMIDevParam.NormalPMI.Value == OP_MODE.Enable)
                {
                    retval = true;
                }
                else
                {
                    retval = false;

                    LoggerManager.Error("Unknown ERROR");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum UpdateGroupingInformation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (PMIInfo.PadTableTemplateInfo.Count > 0)
                {
                    foreach (var t in PMIInfo.PadTableTemplateInfo.Select((value, i) => new { i, value }))
                    {
                        var template = t.value;
                        var index = t.i;

                        // Grouping 정보가 없는 경우
                        if (template.GroupingDone.Value == false)
                        {
                            // 현재 테이블 인덱스의 Grouping Process를 진행 
                            retval = this.PMIModule().PadGroupingMethod(index);

                            LoggerManager.Debug($"PadGroupingMethod Success, Table : {index + 1}");

                            // Grouping이 성공적으로 수행되었을 경우, Group Sequnece 제작
                            if (retval == EventCodeEnum.NONE)
                            {
                                retval = this.PMIModule().MakeGroupSequence(index);

                                LoggerManager.Debug($"MakeGroupSequence Success, Table : {index + 1}");

                                // Sequence 제작이 성공적으로 수행되었을 경우, GropingDone 플래그를 True로 만들어 놓음
                                if (retval == EventCodeEnum.NONE)
                                {
                                    template.GroupingDone.Value = true;
                                }
                                else
                                {
                                    LoggerManager.Error($"MakeGroupSequence error.");
                                    break;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"PadGroupingMethod error.");
                                break;
                            }
                        }

                        retval = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    retval = EventCodeEnum.PMI_NOT_EXIST_TABLE;

                    LoggerManager.Debug($"Not exist PadTableTemplateInfo");
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IParam GetPMIDevIParam()
        {
            return PMIModuleDevParam_IParam;
        }

        public IParam GetPMISysIParam()
        {
            return PMIModuleSysParam_IParam;
        }

        public byte[] GetPMIDevParam()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(PMIModuleDevParam_IParam, typeof(PMIModuleDevParam));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public EventCodeEnum SetPMITrigger(PMIRemoteTriggerEnum trigger)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DoPMIInfo.PMITrigger = trigger;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void PMIInfoUpdatedToLoader()
        {
            try
            {
                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    IPMIInfo pmiinfo = this.GetParam_Wafer().GetSubsInfo().GetPMIInfo();

                    var bytes = SerializeManager.ObjectToByte(pmiinfo);
                    this.LoaderRemoteMediator().GetServiceCallBack()?.PMIInfoUpdated(bytes);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void AddPadTemplate(PadTemplate template)
        {
            try
            {
                PMIInfo.PadTemplateInfo.Add(template);
                //PMIInfo.SelectedPadTemplateIndex = PMIInfo.PadTemplateInfo.Count - 1;

                (this.PnPManager().CurStep as IPMITemplateMiniViewModel)?.UpdatePMITemplateMiniViewModel();

                // Save
                this.StageSupervisor().SaveWaferObject();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetRemoteOperation(PMIRemoteOperationEnum remotevalue)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DoPMIInfo.RemoteOperation = remotevalue;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }


        public IFocusing GetFocuisngModule()
        {
            IFocusing retval = null;

            try
            {
                retval = this.PMINormalFocusModel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PMITriggerComponent GetTriggerComponent()
        {
            return PMIDevParam.TriggerComponent;
        }
    }
}
