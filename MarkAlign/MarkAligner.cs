using System;
using System.Collections.Generic;
using System.Linq;
using ProberInterfaces;
using ProberInterfaces.MarkAlign;
using ProberInterfaces.Param;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MarkAlignParamObject;
using PnPControl;
using RelayCommandBase;
using ProberInterfaces.Command;
using ProberErrorCode;
using ProberInterfaces.Template;
using ProberInterfaces.State;
using LogModule;
using System.Runtime.CompilerServices;
using System.IO;
using Focusing;

namespace MarkAlign
{
    public class MarkAligner : IMarkAligner, INotifyPropertyChanged, IHasSysParameterizable, IHasDevParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
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
        private ReasonOfError _ReasonOfError;
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

        private bool _UpdatePadCen;
        public bool UpdatePadCen
        {
            get { return _UpdatePadCen; }
            set
            {
                if (value != _UpdatePadCen)
                {
                    _UpdatePadCen = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DiffMarkPosX { get; set; }
        public double DiffMarkPosX
        {
            get { return _DiffMarkPosX; }
            set { _DiffMarkPosX = value; }
        }

        private double _DiffMarkPosY { get; set; }
        public double DiffMarkPosY
        {
            get { return _DiffMarkPosY; }
            set { _DiffMarkPosY = value; }
        }

        private bool _ForceWaferCamCylinderExtended = false;
        public bool ForceWaferCamCylinderExtended
        {
            get { return _ForceWaferCamCylinderExtended; }
            set
            {
                if (value != _ForceWaferCamCylinderExtended)
                {
                    _ForceWaferCamCylinderExtended = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Mark Align 수행 시 개별 Process를 수행하기 위한 변수 추가
        private EnumMarkAlignProcMode _MarkAlignProcMode = EnumMarkAlignProcMode.None;
        public EnumMarkAlignProcMode MarkAlignProcMode
        {
            get { return _MarkAlignProcMode; }
            set
            {
                if (value != _MarkAlignProcMode)
                {
                    _MarkAlignProcMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsOnDebugMode = false;
        public bool IsOnDebugMode
        {
            get { return _IsOnDebugMode; }
            set
            {
                if (value != _IsOnDebugMode)
                {
                    _IsOnDebugMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSaveImageCompVerify = false;
        public bool IsSaveImageCompVerify
        {
            get { return _IsSaveImageCompVerify; }
            set
            {
                if (value != _IsSaveImageCompVerify)
                {
                    _IsSaveImageCompVerify = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CompVerifyImagePathBase;
        public string CompVerifyImagePathBase
        {
            get { return _CompVerifyImagePathBase; }
            set
            {
                if (value != _CompVerifyImagePathBase)
                {
                    _CompVerifyImagePathBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double tolX, tolY = 0;


        //public IFocusing FocusingModule { get; set; }
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

        private MarkAlignParam MarkAlignSysParam
        {
            get => MarkAlignParam_IParam as MarkAlignParam;
        }

        private IParam _MarkAlignParam_IParam;
        [ParamIgnore]
        public IParam MarkAlignParam_IParam
        {
            get { return _MarkAlignParam_IParam; }
            set
            {
                if (value != _MarkAlignParam_IParam)
                {
                    _MarkAlignParam_IParam = value;
                    RaisePropertyChanged();
                }
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

        private MachineCoordinate _MarkCumulativeChangeValue = new MachineCoordinate();
        public MachineCoordinate MarkCumulativeChangeValue
        {
            get { return _MarkCumulativeChangeValue; }
            set
            {
                if (value != _MarkCumulativeChangeValue)
                {
                    _MarkCumulativeChangeValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ExecuteRetryAlignment;

        public bool ExecuteRetryAlignment
        {
            get { return _ExecuteRetryAlignment; }
            set { _ExecuteRetryAlignment = value; }
        }


        private ModuleStateBase _ModuleState;

        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private MarkAlignState _MarkAlignState;
        public MarkAlignState MarkAlignState
        {
            get
            {
                return _MarkAlignState;
            }
        }

        public IInnerState InnerState
        {
            get { return _MarkAlignState; }
            set
            {
                if (value != _MarkAlignState)
                {
                    _MarkAlignState = value as MarkAlignState;
                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        private IFocusing _MarkFocusModel;
        public IFocusing MarkFocusModel
        {
            get
            {
                if (_MarkFocusModel == null)
                    _MarkFocusModel = this.FocusManager().GetFocusingModel((MarkAlignParam_IParam as MarkAlignParam).FocusingModuleDllInfo);

                return _MarkFocusModel;
            }
        }

        //private IFocusParameter _MarkFocusParam;

        //public IFocusParameter MarkFocusParam
        //{
        //    get { return _MarkFocusParam; }
        //    set { _MarkFocusParam = value; }
        //}

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
        #region //..Template
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

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                MarkAlignStateEnum state = (InnerState as MarkAlignState).GetState();

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

        private IMarkAlignControlItems _MarkAlignControlItems;
        public IMarkAlignControlItems MarkAlignControlItems
        {
            get { return _MarkAlignControlItems; }
            set { _MarkAlignControlItems = value; }
        }

        #region //..Commnad
        //private RelayCommand _RegistePatternCommand;
        //public ICommand RegistePatternCommand
        //{
        //    get
        //    {
        //        if (null == _RegistePatternCommand) _RegistePatternCommand = new RelayCommand(
        //            CmdRegistePattern);
        //        return _RegistePatternCommand;
        //    }
        //}

        //private void CmdRegistePattern()
        //{
        //    MarkRegistration();
        //    // MarkAlignParam.MarkFocusParam.FocusingModel.ShowFocusGraph();


        //}



        //.. PNPParams
        public string ParamPath { get; set; }


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


        private CommandSlot _CommandSendSlot;
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }
        //===
        #endregion

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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
                throw;
            }
            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    MarkAlignControlItems = new MarkAlignControlItems();

                    _ReasonOfError = new ReasonOfError(ModuleEnum.MarkAlign);
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    _MarkAlignState = new MarkAlignIDLEState(this);
                    ModuleState = new ModuleUndefinedState(this);

                    this.TemplateManager().InitTemplate(this);

                    Initialized = true;
                    PinShiftComensation = true;

                    CompVerifyImagePathBase = $"{LoggerManager.LoggerManagerParam.FilePath}\\{LoggerManager.LoggerManagerParam.DevFolder}\\Image\\" +
                        $"MarkAlign\\CompVerify\\";

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

        public EventCodeEnum DoMarkAlign(bool Force = false, bool updatePadCen = false, EnumMarkAlignProcMode Mode = EnumMarkAlignProcMode.None)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.VisionManager().StartImageCollection(EnumProberModule.MARKALIGNER, EventCodeEnum.MARK_ALIGN_FAIL);

                this.UpdatePadCen = updatePadCen;

                if (Force == false)
                {
                    if (this.StageSupervisor().MarkObject.AlignState.Value == AlignStateEnum.DONE)
                    {
                        retval = EventCodeEnum.NONE;
                        return retval;
                    }
                }

                bool doneFlag = false;
                List<ISubModule> modules = this.Template.GetProcessingModule();
                MarkAlignProcMode = Mode;

                CatCoordinates originmarkpos = new CatCoordinates();
                this.CoordinateManager().StageCoord.RefMarkPos.CopyTo(originmarkpos);

                foreach (var subModule in modules)
                {
                    retval = subModule.ClearData();
                    retval = subModule.Execute();

                    if (subModule.Equals(modules.LastOrDefault()) && subModule.GetState() == SubModuleStateEnum.DONE && retval == EventCodeEnum.NONE)
                    {
                        doneFlag = true;
                        break;
                    }
                    else
                    {
                        if ((retval == EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED || retval == EventCodeEnum.MARK_ALIGN_FOCUSING_FAILED) && GetExecuteRetryAlignment() == false)
                        {
                            SetExecuteRetryAlignment(true);
                            LoggerManager.Debug($"Mark Align Error from {subModule.ToString()}. Error Code:{retval}, execute retry. retry extened focusing  range:{(MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam.FocusRange.Value}");
                            
                            retval = subModule.Execute();

                            if (subModule.Equals(modules.LastOrDefault()) && subModule.GetState() == SubModuleStateEnum.DONE && retval == EventCodeEnum.NONE)
                            {
                                doneFlag = true;
                                LoggerManager.Debug($"Mark Align Retry Success. retry extened focusing. before mark X:{originmarkpos.GetX()},Y:{originmarkpos.GetY()},Z:{originmarkpos.GetZ()}. after mark X:{this.CoordinateManager().StageCoord.RefMarkPos.GetX()},Y:{this.CoordinateManager().StageCoord.RefMarkPos.GetY()},Z:{this.CoordinateManager().StageCoord.RefMarkPos.GetZ()},range:{(MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam.FocusRange.Value}");
                                break;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Mark Align Error from {subModule.ToString()}. Error Code:{retval}, skip retry.");
                        }
                        this.MetroDialogManager().ShowMessageDialog("Error Message", "Mark Align Fail. Please check the Reference mark status.",
                            MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                }

                if (doneFlag)
                {
                    this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                    this.InnerStateTransition(new MarkAlignDoneState(this));
                    this.VisionManager().AllStageCameraStopGrab();
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    this.NotifyManager().Notify(EventCodeEnum.MARK_ALIGN_FAIL);
                }

                MarkAlignProcMode = EnumMarkAlignProcMode.None;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Exception(err);
            }
            finally
            {
                this.UpdatePadCen = false;
                MarkAlignProcMode = EnumMarkAlignProcMode.None;
                this.SetExecuteRetryAlignment(false);

                this.VisionManager().EndImageCollection();
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

        public EventCodeEnum SetNodeSetupState()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum MoveToMarkFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                RetVal = this.StageSupervisor().StageModuleState.MoveToMark();

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                RetVal = EventCodeEnum.MOTION_MOVING_ERROR;
            }

            return RetVal;
        }

        //public EventCodeEnum FocusingFunc()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.NONE;

        //    try
        //    {
        //        FocusingCommandCanExecute = false;

        //        //RetVal = MarkFocusModel.Focusing_Retry(
        //        //                FocusingType.WAFER,
        //        //               MarkAlignParam.MarkFocusParam,
        //        //                false, //==> Light Change
        //        //                false, //==> brute force retry
        //        //                false); //==> find potential 

        //        if (RetVal != EventCodeEnum.NONE)
        //        {
        //            RetVal = EventCodeEnum.FOCUS_FAILED;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        RetVal = EventCodeEnum.FOCUS_FAILED;
        //    }
        //    finally
        //    {
        //        FocusingCommandCanExecute = true;

        //    }
        //    return RetVal;
        //}

        //public EventCodeEnum PatternMatchingFunc()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    PMResult pmRet = new PMResult();

        //    var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
        //    var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
        //    double tolX = 1000000;
        //    double tolY = 1000000;

        //    pmRet = this.VisionManager().PatternMatching(MarkAlignParam.MarkPatMatParam);
        //    if (pmRet != null)
        //    {
        //        if (pmRet.ResultParam.Count == 1)
        //        {
        //            //CameraRatio= (ActualPos_Center - ActualPos_Pattern)/(Center_Pixel - Pattern_Pixel)
        //            double actualX;
        //            this.MotionManager().GetCommandPos(EnumAxisConstants.X, out actualX);

        //            double offsetX = (pmRet.ResultBuffer.SizeX / 2 - pmRet.ResultParam[0].XPoss) * (this.VisionManager().CameraDescriptor.Cams[0].GetRatioX());
        //            double pattenActualX = actualX + offsetX;

        //            double actualY;
        //            this.MotionManager().GetCommandPos(EnumAxisConstants.Y, out actualY);

        //            double offsetY = (pmRet.ResultBuffer.SizeY / 2 - pmRet.ResultParam[0].YPoss) * (this.VisionManager().CameraDescriptor.Cams[0].GetRatioY());
        //            double pattenActualY = actualY - offsetY;

        //            if (Math.Abs(pattenActualX - this.CoordinateManager().StageCoord.MarkEncPos.X.Value) > tolX || Math.Abs(pattenActualY - this.CoordinateManager().StageCoord.MarkEncPos.Y.Value) > tolY)
        //            {
        //                MarkAlignStateTransition(new MarkAlignShiftedState(this));
        //            }
        //            else
        //            {
        //                this.MotionManager().AbsMove(axisX, pattenActualX);
        //                this.MotionManager().AbsMove(axisY, pattenActualY);

        //                this.CoordinateManager().StageCoord.RefMarkPos.X.Value = pattenActualX;
        //                this.CoordinateManager().StageCoord.RefMarkPos.Y.Value = pattenActualY;

        //                double diifMarkPosX = pattenActualX - this.CoordinateManager().StageCoord.MarkEncPos.X.Value;
        //                double diifMarkPosY = pattenActualY - this.CoordinateManager().StageCoord.MarkEncPos.Y.Value;

        //                WaferObject wafer = (WaferObject)this.StageSupervisor().WaferObject;

        //                wafer.SubsInfo.WaferCenter.X.Value -= diifMarkPosX;
        //                wafer.SubsInfo.WaferCenter.Y.Value -= diifMarkPosY;

        //            }
        //            RetVal = EventCodeEnum.NONE;
        //            MarkAlignStateTransition(new MarkAlignDoneState(this));
        //        }
        //        else
        //        {
        //            RetVal = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
        //            MarkAlignStateTransition(new MarkAlignPatternFailState(this));
        //            //MarkAlignStateTransition(new MarkAl(this));
        //        }
        //    }
        //    this.VisionManager().StartGrab(MarkAlignParam.MarkPatMatParam.CamType.Value);
        //    return RetVal;
        //}



        public EventCodeEnum MoveToMark()
        {
            return MoveToMarkFunc();
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
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new MarkAlignTemplateFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(MarkAlignTemplateFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    TemplateParameter = tmpParam as MarkAlignTemplateFile;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = this.SaveParameter(TemplateParameter);

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

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new MarkAlignParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                if (tmpParam is MarkAlignParam)
                {
                    (tmpParam as MarkAlignParam).MarkCompensationEnable = true;
                    (tmpParam as MarkAlignParam).MarkDiffTolerance_X = new Element<double>();
                    (tmpParam as MarkAlignParam).MarkDiffTolerance_X.Value = 100;
                    (tmpParam as MarkAlignParam).MarkDiffTolerance_Y = new Element<double>();
                    (tmpParam as MarkAlignParam).MarkDiffTolerance_Y.Value = 100;
                }
                RetVal = this.LoadParameter(ref tmpParam, typeof(MarkAlignParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    MarkAlignParam_IParam = tmpParam;

                    var fileName = "MarkPattern";

                    var oldFilePath = @"C:\ProberSystem\Default\Parameters\SystemParam\Mark\" + fileName;

                    var newDirectoryPath = Path.Combine(this.FileManager().GetSystemRootPath(), MarkAlignParam_IParam.FilePath, "Pattern");

                    var newFilePath = Path.Combine(newDirectoryPath, fileName);

                    if ((MarkAlignParam_IParam as MarkAlignParam).MarkPatMatParam.PMParameter.ModelFilePath.Value != newFilePath)
                    {
                        if (File.Exists(oldFilePath))
                        {
                            if (!Directory.Exists(newDirectoryPath))
                            {
                                Directory.CreateDirectory(newDirectoryPath);
                            }

                            if (!File.Exists(newFilePath))
                            {
                                File.Copy(oldFilePath, newFilePath);

                                LoggerManager.Debug($"[{this.GetType().Name}], LoadSysParameter() : The mark image has successfully been copied to the new location. newPath = {newFilePath}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}], LoadSysParameter() : File not exist. Path = {oldFilePath}");
                        }

                        (MarkAlignParam_IParam as MarkAlignParam).MarkPatMatParam.PMParameter.ModelFilePath.Value = newFilePath;

                        SaveSysParameter();
                    }

                    if ((MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam == null)
                    {
                        (MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam = new NormalFocusParameter();
                        (MarkAlignParam_IParam as MarkAlignParam).FocusParam.CopyTo((MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam);

                        if ((MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam.FocusRange != null)
                        {
                            (MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam.FocusRange.Value = 500;
                        }
                        SaveSysParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(MarkAlignParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }


        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {

                RetVal = MarkAlignState.CanExecute(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }

        public EventCodeEnum FocusingFunc()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            return TemplateToPnpConverter.Converter(Template.TemplateModules);
        }
        public bool GetMarkCompensationEnable()
        {
            if (MarkAlignSysParam != null)
            {
                return MarkAlignSysParam.MarkCompensationEnable;
            }else
            {
                return true;
            }
        }
        bool PinShiftComensation = true;
        public bool GetPinCompensationEnable()
        {
            return PinShiftComensation;
        }

        public void SetPinCompensationEnable(bool enable)
        {
            PinShiftComensation= enable;
        }

        public double GetMarkDiffTolerance_X()
        {
            if (MarkAlignSysParam != null&& MarkAlignSysParam.MarkDiffTolerance_X!=null)
            {
                return MarkAlignSysParam.MarkDiffTolerance_X.Value;
            }else
            {
                return -1;
            }
        }

        public void SetMarkDiffTolerance_X(double tolerancex)
        {
            try
            {
                if (MarkAlignSysParam != null && MarkAlignSysParam.MarkDiffTolerance_X != null)
                {
                    MarkAlignSysParam.MarkDiffTolerance_X.Value = tolerancex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public double GetMarkDiffTolerance_Y()
        {
            if (MarkAlignSysParam != null && MarkAlignSysParam.MarkDiffTolerance_Y != null)
            {
                return MarkAlignSysParam.MarkDiffTolerance_Y.Value;
            }
            else
            {
                return -1;
            }
        }

        public bool GetTriggerMarkVerificationAfterWaferAlign()
        {
            bool retVal = true;
            try
            {
                if (MarkAlignSysParam != null)
                {
                    retVal = MarkAlignSysParam.MarkVerificationAfterWaferAlign.Value;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetTriggerMarkVerificationAfterWaferAlign(bool markverification)
        {
            try
            {
                if (MarkAlignSysParam != null)
                {
                    MarkAlignSysParam.MarkVerificationAfterWaferAlign.Value = markverification;
                    LoggerManager.Debug($"SetTriggerMarkVerificationAfterWaferAlign() Trigger MarkVerificationAfterWaferAlign : {markverification}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetMarkDiffTolerance_Y(double tolerancey)
        {
            try
            {
                if (MarkAlignSysParam != null && MarkAlignSysParam.MarkDiffTolerance_Y != null)
                {
                    MarkAlignSysParam.MarkDiffTolerance_Y.Value = tolerancey;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public (double,double) GetMarkDiffToleranceOfWA()
        {
            (double, double) retVal = (0, 0);
            try
            {
                if (MarkAlignSysParam != null &&
                MarkAlignSysParam.MarkDiffToleranceOfWA_X != null &&
                MarkAlignSysParam.MarkDiffToleranceOfWA_Y != null)
                {
                    retVal = (MarkAlignSysParam.MarkDiffToleranceOfWA_X.Value, MarkAlignSysParam.MarkDiffToleranceOfWA_Y.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetMarkDiffToleranceOfWA(double toleranceX, double toleranceY)
        {
            try
            {
                if (MarkAlignSysParam != null)
                {
                    if(MarkAlignSysParam.MarkDiffToleranceOfWA_X == null)
                    {
                        MarkAlignSysParam.MarkDiffToleranceOfWA_X = new Element<double>();
                    }
                    MarkAlignSysParam.MarkDiffToleranceOfWA_X.Value = toleranceX;

                    if(MarkAlignSysParam.MarkDiffToleranceOfWA_Y == null)
                    {
                        MarkAlignSysParam.MarkDiffToleranceOfWA_Y = new Element<double>();
                    }
                    MarkAlignSysParam.MarkDiffToleranceOfWA_Y.Value = toleranceY;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetDelaywaferCamCylinderExtendedBeforeFocusing()
        {
            try
            {
                if (MarkAlignSysParam != null)
                {
                    return MarkAlignSysParam.DelaywaferCamCylinderExtendedBeforeFocusing;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return 0;
            }
        }

        public void SetDelaywaferCamCylinderExtendedBeforeFocusing(int delaytime)
        {
            try
            {
                if (MarkAlignSysParam != null)
                {
                    MarkAlignSysParam.DelaywaferCamCylinderExtendedBeforeFocusing = delaytime;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetExecuteRetryAlignment(bool retryFlag)
        {
            try
            {
                if (ExecuteRetryAlignment != retryFlag)
                {
                    ExecuteRetryAlignment = retryFlag;
                    LoggerManager.Debug($"[Mark Aligner] ExecuteRetryAlignment set to {ExecuteRetryAlignment}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetExecuteRetryAlignment()
        {
            return ExecuteRetryAlignment;
        }
    }
}
