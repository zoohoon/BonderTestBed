using AccountModule;
using LogModule;
using MarkAlignParamObject;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using PnPControl;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AlignEX;
using ProberInterfaces.MarkAlign;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MarkAlignOpusVProcess
{
    public class MKOpusVProcess : PNPSetupBase, ISetup, IProcessingModule
    {
        public override Guid ScreenGUID { get; } = new Guid("D81C5A65-044E-22BD-897F-75AC68BA4EFD");
        public MKOpusVProcess()
        {
        }

        private bool IsInfo = true;

        public override bool Initialized { get; set; } = false;
        private bool MarkInitiated = false;
        #region ..//Pattern Property
        private double _PatternSizeLeft;
        public new double PatternSizeLeft
        {
            get { return _PatternSizeLeft; }
            set
            {
                if (value != _PatternSizeLeft)
                {
                    _PatternSizeLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PatternSizeTop;
        public new double PatternSizeTop
        {
            get { return _PatternSizeTop; }
            set
            {
                if (value != _PatternSizeTop)
                {
                    _PatternSizeTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PatternSizeWidth;
        public new double PatternSizeWidth
        {
            get { return _PatternSizeWidth; }
            set
            {
                if (value != _PatternSizeWidth)
                {
                    _PatternSizeWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PatternSizeHeight;
        public new double PatternSizeHeight
        {
            get { return _PatternSizeHeight; }
            set
            {
                if (value != _PatternSizeHeight)
                {
                    _PatternSizeHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> UcDispaly Port Target Rectangle

        #endregion
        public AlginParamBase Param { get; set; }
        public SubModuleStateBase SubModuleState { get; set; }
        public SubModuleMovingStateBase MovingState { get; set; }
        //public IParam DevParam { get; set; }
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public new List<object> Nodes { get; set; }
        public new ICategoryNodeItem Parent { get; set; }
        public new ObservableCollection<ICategoryNodeItem> Categories { get; set; }

        public new void DeInitModule()
        {

        }
        private int _ChangeWidthValue;
        private int _ChangeHeightValue;
        private void PatterinSizePlus()
        {
            try
            {
                _ChangeWidthValue = 12;
                _ChangeHeightValue = 12;
                RectModify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private void PatterinSizeMinus()
        {
            try
            {
                _ChangeWidthValue = -12;
                _ChangeHeightValue = -12;
                RectModify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private MarkAlignParam _MarkAlignParam { get; set; }
        public MarkAlignParam MarkAlignParam
        {
            get { return _MarkAlignParam; }
            set { _MarkAlignParam = value; }
        }


        private List<(EnumProberCam, LightValueParam)> _LightBackupValues
             = new List<(EnumProberCam, LightValueParam)>();
        /// <summary>
        /// Mark Alignment 전 Pin High camera 이외의 조명들을 0 으로 설정 함.
        /// 0으로 설정 하기 전 조명정보를 가지고 있다가 Alignment 후 해당 데이터로 다시 조명을 설정하고자 함.
        /// </summary>
        public List<(EnumProberCam, LightValueParam)> LightBackupValues
        {
            get { return _LightBackupValues; }
            set { _LightBackupValues = value; }
        }

        private EventCodeEnum RectModify()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PatternSizeWidth = TargetRectangleWidth;
                PatternSizeHeight = TargetRectangleHeight;
                PatternSizeLeft = TargetRectangleLeft;
                PatternSizeTop = TargetRectangleTop;

                PatternSizeWidth += _ChangeWidthValue;
                PatternSizeHeight += _ChangeHeightValue;
                PatternSizeLeft -= (_ChangeWidthValue / 2);
                PatternSizeTop -= (_ChangeHeightValue / 2);

                TargetRectangleWidth = PatternSizeWidth;
                TargetRectangleHeight = PatternSizeHeight;

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Modify() : Error occured.");
                LoggerManager.Exception(err);

            }
            return retVal;
        }
        //Don`t Touch
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }



        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (Initialized == false)
                    {

                        SubModuleState = new SubModuleIdleState(this);
                        MovingState = new SubModuleStopState(this);
                        _MarkAlignParam = this.MarkAligner().MarkAlignParam_IParam as MarkAlignParam;

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

        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public EventCodeEnum Modify()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ClearData()
        {
            return EventCodeEnum.NONE;

        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        //public EventCodeEnum SaveDevParameter()
        //{
        //    return EventCodeEnum.NONE;

        //}
        public EventCodeEnum StateTransition(SubModuleStateBase state)
        {
            return EventCodeEnum.NONE;

        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                RestoreLightsForCameras();
                await base.Cleanup(parameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public bool IsExecute()
        {
            bool retVal = false;

            try
            {
                if (this.StageSupervisor().MarkObject.GetAlignState() == AlignStateEnum.IDLE)
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
                throw;
            }

            return retVal;
        }
        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        private AsyncCommand _RegistePatternCommand;
        public ICommand RegistePatternCommand
        {
            get
            {
                if (null == _RegistePatternCommand) _RegistePatternCommand = new AsyncCommand(
                    CmdRegistePattern);
                return _RegistePatternCommand;
            }
        }

        private async Task CmdRegistePattern()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                await Task.Run(() =>
                {
                    retVal = MarkRegistration();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = FocusingFunc();

                        if (retVal == EventCodeEnum.NONE)
                        {
                            PMResult pmRet;
                            pmRet = DoPatternMatching();
                            if (pmRet != null)
                            {

                                if (pmRet.ResultParam.Count == 1)
                                {
                                    double actualX = 0;
                                    this.MotionManager().GetRefPos(EnumAxisConstants.X, ref actualX);

                                    double actualY = 0;
                                    this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref actualY);

                                    double actualPZ = 0;
                                    this.MotionManager().GetRefPos(EnumAxisConstants.PZ, ref actualPZ);


                                    this.CoordinateManager().StageCoord.MarkEncPos.X.Value = actualX;
                                    this.CoordinateManager().StageCoord.MarkEncPos.Y.Value = actualY;
                                    this.CoordinateManager().StageCoord.MarkEncPos.Z.Value = actualPZ;
                                    this.CoordinateManager().SaveSysParameter();
                                    this.CoordinateManager().StageCoord.RefMarkPos.X.Value = actualX;
                                    this.CoordinateManager().StageCoord.RefMarkPos.Y.Value = actualY;
                                    this.CoordinateManager().StageCoord.RefMarkPos.Z.Value = actualPZ;
                                    this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                                }
                                else
                                {
                                    retVal = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
                            }
                            this.VisionManager().StartGrab(MarkAlignParam.MarkPatMatParam.CamType.Value, this);
                        }
                        else
                        {
                            retVal = EventCodeEnum.FOCUS_FAILED;
                        }
                    }
                });

                if (retVal == EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Mark Pattern Regist", "Mark pattern is registered successfully", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Mark Pattern Regist", "Mark pattern registration failed.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(CmdFocusing, new Func<bool>(() => !FocusingCommandCanExecute));
                return _FocusingCommand;
            }
        }

        private AsyncCommand _DoMarkAlignCommand;
        public ICommand DoMarkAlignCommand
        {
            get
            {
                if (null == _DoMarkAlignCommand) _DoMarkAlignCommand = new AsyncCommand(DoMarkAlgin);
                return _DoMarkAlignCommand;
            }
        }

        private async Task<EventCodeEnum> DoMarkAlgin()
        {
            try
            {
                EventCodeEnum ret = DoExecute();
                SetStepSetupState();

                if (ret == EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("MarkAlign", "MarkAlign is done successfully", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("MarkAlign", "MarkAlign failed.", EnumMessageStyle.Affirmative);
                }
                return ret;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
        }



        private bool _FocusingCommandCanExecute;
        public bool FocusingCommandCanExecute
        {
            get { return _FocusingCommandCanExecute; }
            set
            {
                if (value != _FocusingCommandCanExecute)
                {
                    _FocusingCommandCanExecute = value;
                    RaisePropertyChanged();
                }
            }
        }


        private async Task<EventCodeEnum> CmdFocusing()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                ret = FocusingFunc();

                if (ret == EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Mark Focusing", "Mark focusing is done successfully.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Mark Focusing", "Mark focusing failed.", EnumMessageStyle.Affirmative);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return ret;
        }

        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public override bool IsParameterChanged(bool issave = false)
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
        private AsyncCommand _MoveToMarkCommand;
        public ICommand MoveToMarkCommand
        {
            get
            {
                if (null == _MoveToMarkCommand) _MoveToMarkCommand = new AsyncCommand(MoveToMarkFunc);
                return _MoveToMarkCommand;
            }
        }

        public void SetLightForMark()
        {

            try
            {
                BackupLightsForCameras();

                if (this.MarkAlignParam.MarkPatMatParam.LightParams == null)
                {
                    var phRefLight = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 255);
                }
                else
                {

                    this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, this.MarkAlignParam.MarkPatMatParam.LightParams[0].Value.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void BackupLightsForCameras()
        {
            try
            {
                var cams = this.VisionManager().GetCameras();

                foreach (var cam in cams)
                {
                    var channelType = cam.GetChannelType();

                    if (channelType == EnumProberCam.PIN_HIGH_CAM ||
                        channelType == EnumProberCam.PIN_LOW_CAM ||
                        channelType == EnumProberCam.WAFER_HIGH_CAM ||
                        channelType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        cam.BackupLights(true);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void RestoreLightsForCameras()
        {
            try
            {
                var cams = this.VisionManager().GetCameras();

                foreach (var cam in cams)
                {
                    var channelType = cam.GetChannelType();

                    if (channelType == EnumProberCam.PIN_HIGH_CAM ||
                        channelType == EnumProberCam.PIN_LOW_CAM ||
                        channelType == EnumProberCam.WAFER_HIGH_CAM ||
                        channelType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        cam.RestoreLights();
                    }
                }
                this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> MoveToMarkFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SetLightForMark();

                RetVal = this.StageSupervisor().StageModuleState.MoveToMark();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                RetVal = EventCodeEnum.MOTION_MOVING_ERROR;
            }

            return Task.FromResult<EventCodeEnum>(RetVal);
        }
        public EventCodeEnum MoveToMarkFunc1()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SetLightForMark();

                RetVal = this.StageSupervisor().StageModuleState.MoveToMark();

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                RetVal = EventCodeEnum.MOTION_MOVING_ERROR;
            }

            return RetVal;
        }


        public EventCodeEnum FocusingFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                FocusingCommandCanExecute = false;

                FocusParameter focusParam = null;

                if (this.MarkAligner().GetExecuteRetryAlignment() == false)
                {
                    focusParam = (this.MarkAligner().MarkAlignParam_IParam as MarkAlignParam).FocusParam;
                }
                else
                {
                    focusParam = (this.MarkAligner().MarkAlignParam_IParam as MarkAlignParam).RetryFocusParam;
                }

                if (focusParam.FlatnessThreshold.Value < this.VisionManager().GetMaxFocusFlatnessValue())
                {
                    focusParam.FlatnessThreshold.Value = this.VisionManager().GetMaxFocusFlatnessValue();
                }

                LoggerManager.Debug($"[MKOpusVProcess] FocusingFunc(): Set Retry = {this.MarkAligner().GetExecuteRetryAlignment()}, Focusing Range = {focusParam.FocusRange.Value}, FlatnessThreshold = {focusParam.FlatnessThreshold.Value}" +
                    $"FocusMaxStep = {focusParam.FocusMaxStep.Value}, FocusThershold = {focusParam.FocusThreshold.Value}");

                //ISSD-3566 두번 째 옵션 true로 주어 Focusing 실패 시 한번 더 돌도록 옵션 값 변경 함.
                RetVal = this.MarkAligner().MarkFocusModel.Focusing_Retry(focusParam, false, true, false, this);

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[MKOpusVProcess] FocusingFunc(): {RetVal} - debug");

                    RetVal = EventCodeEnum.FOCUS_FAILED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, $"[MKOpusVProcess] FocusingFunc(): {RetVal} - exception");
                RetVal = EventCodeEnum.FOCUS_FAILED;
            }
            finally
            {
                FocusingCommandCanExecute = true;
            }

            return RetVal;
        }
        public EventCodeEnum MarkRegistration()
        {
            EventCodeEnum retVal;
            try
            {
                this.VisionManager().StopGrab(_MarkAlignParam.MarkPatMatParam.CamType.Value);
                ImageBuffer img = null;
                this.VisionManager().GetCam(_MarkAlignParam.MarkPatMatParam.CamType.Value).GetCurImage(out img);
                RegisteImageBufferParam rparam = GetDisplayPortRectInfo();
                rparam.CamType = _MarkAlignParam.MarkPatMatParam.CamType.Value;
                rparam.PatternPath = _MarkAlignParam.MarkPatMatParam.PMParameter.ModelFilePath.Value;
                rparam.ImageBuffer = img;

                retVal = this.VisionManager().SavePattern(rparam);

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = this.VisionManager().StartGrab(_MarkAlignParam.MarkPatMatParam.CamType.Value, this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public PMResult DoPatternMatching()
        {
            PMResult retPmResult = null;
            try
            {
                if (this.VisionManager().ConfirmDigitizerEmulMode(MarkAlignParam.MarkPatMatParam.CamType.Value))
                {
                    this.VisionManager().LoadImageFromFileToGrabber(@"C:\ProberSystem\EmulImages\MarkAlign\MarkImage.bmp", MarkAlignParam.MarkPatMatParam.CamType.Value);
                }
                retPmResult = this.VisionManager().PatternMatching(MarkAlignParam.MarkPatMatParam, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retPmResult;
        }


        public EventCodeEnum DoExecute()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Start);


                bool waferCamCylinderExtended = false;

                if ((this.IOManager().IO.Inputs.DIWAFERCAMMIDDLE.Value == false) &&
                        (this.IOManager().IO.Inputs.DIWAFERCAMREAR.Value == true))
                {
                    // 접혀 있음
                    waferCamCylinderExtended = false;
                }
                else if ((this.IOManager().IO.Inputs.DIWAFERCAMMIDDLE.Value == true) &&
                    (this.IOManager().IO.Inputs.DIWAFERCAMREAR.Value == false))
                {
                    // 펴져 있음
                    waferCamCylinderExtended = true;
                }

                // MarkAlignProcMode가 None도 아니고 OnlyMoveToMark 모드도 아니면 Skip한다.
                if (EnumMarkAlignProcMode.None != this.MarkAligner().MarkAlignProcMode && EnumMarkAlignProcMode.OnlyMoveToMark != this.MarkAligner().MarkAlignProcMode)
                {
                    LoggerManager.Debug($"[Mark Align] {MethodBase.GetCurrentMethod().Name} is skipped. (MarkAlignProcMod : {this.MarkAligner().MarkAlignProcMode})");
                    RetVal = EventCodeEnum.NONE;
                }
                else if (EnumMarkAlignProcMode.OnlyMoveToMark == this.MarkAligner().MarkAlignProcMode) // OnlyMoveToMark 모드면 MoveToMarkFunc1만 하고 return한다.
                {
                    // MoveToMarkFunc1에 많은 시퀀스가 섞여있어 ComponentVerification 에서는 Move 동작만 분리해서 사용중. OnlyMoveToMark 모드는 현재 사용되지 않는다.
                    RetVal = MoveToMarkFunc1();

                    if (EventCodeEnum.NONE == RetVal)
                    {
                        this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                        SubModuleState = new SubModuleDoneState(this);
                    }
                    else
                    {
                        //Move To Mark ERROR
                        this.NotifyManager().Notify(EventCodeEnum.MARK_ALIGN_MOVE_ERROR);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Move_Failure);
                        LoggerManager.Debug("[MarkAlign] MoveToMark Error.  ErrorCode=" + RetVal);
                        SubModuleState = new SubModuleErrorState(this);
                    }
                    return RetVal;
                }
                else
                {
                    LoggerManager.ActionLog(ModuleLogType.MARK_ALIGN, StateLogType.START, $"Mark Align Start", this.LoaderController().GetChuckIndex());
                    RetVal = MoveToMarkFunc1();
                }

                // MARK_ALIGN_MOVE_ERROR for Testing
                if (this.MarkAligner().MarkAlignControlItems.MARK_ALIGN_MOVE_ERROR == true)
                {
                    RetVal = EventCodeEnum.MARK_ALIGN_MOVE_ERROR;
                }
                var param = (MarkAlignParam)this.MarkAligner().MarkAlignParam_IParam;

                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Move_OK);

                    if (waferCamCylinderExtended == false || this.MarkAligner().ForceWaferCamCylinderExtended == true)
                    {
                        int delayTimeSec = (int)TimeSpan.FromSeconds(this.MarkAligner().GetDelaywaferCamCylinderExtendedBeforeFocusing()).TotalMilliseconds;
                        Thread.Sleep(delayTimeSec);

                        LoggerManager.Debug($"Wait Before Fosuing, DelaywaferCamCylinderExtendedBeforeFocusing(sec) : {delayTimeSec}");
                    }

                    // MarkAlignProcMode가 None도 아니고 OnlyFocusing 모드도 아니면 Skip한다.
                    if (EnumMarkAlignProcMode.None != this.MarkAligner().MarkAlignProcMode && EnumMarkAlignProcMode.OnlyFocusing != this.MarkAligner().MarkAlignProcMode)
                    {
                        LoggerManager.Debug($"[Mark Align] {MethodBase.GetCurrentMethod().Name} is skipped. (MarkAlignProcMod : {this.MarkAligner().MarkAlignProcMode})");
                        RetVal = EventCodeEnum.NONE;
                    }
                    else if (EnumMarkAlignProcMode.OnlyFocusing == this.MarkAligner().MarkAlignProcMode) // OnlyFocusing 모드면 MoveToMarkFunc1만 하고 return한다.
                    {
                        // OnlyFocusing 모드인 경우 조명이 꺼진상태로 시작되므로 항상 조명을 켜준다.
                        if (this.MarkAlignParam.MarkPatMatParam.LightParams == null)
                        {
                            var phRefLight = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 255);
                        }
                        else
                        {
                            this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, this.MarkAlignParam.MarkPatMatParam.LightParams[0].Value.Value);
                        }

                        RetVal = FocusingFunc();

                        if (EventCodeEnum.NONE == RetVal)
                        {
                            this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                            SubModuleState = new SubModuleDoneState(this);
                        }
                        else
                        {
                            //Focusing Error
                            this.NotifyManager().Notify(EventCodeEnum.MARK_ALIGN_FOCUSING_FAILED);
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Focusing_Failure);
                            LoggerManager.Debug("[MarkAlign] Focusing Error.  ErrorCode=" + RetVal);
                            SubModuleState = new SubModuleErrorState(this);
                        }
                        return RetVal;
                    }
                    else
                    {
                        RetVal = FocusingFunc();
                    }

                    // MARK_ALIGN_FOCUSING_FAILED for Testing
                    if (this.MarkAligner().MarkAlignControlItems.MARK_ALIGN_FOCUSING_FAILED == true)
                    {
                        RetVal = EventCodeEnum.MARK_ALIGN_FOCUSING_FAILED;
                    }

                    // MarkAlignProcMode가 None도 아니고 OnlyPatternMatching 모드도 아니면 Skip한다.
                    if (EnumMarkAlignProcMode.None != this.MarkAligner().MarkAlignProcMode && EnumMarkAlignProcMode.OnlyPatternMatching != this.MarkAligner().MarkAlignProcMode)
                    {
                        LoggerManager.Debug($"[Mark Align] {MethodBase.GetCurrentMethod().Name} is skipped. (MarkAlignProcMod : {this.MarkAligner().MarkAlignProcMode})");

                        if (RetVal == EventCodeEnum.NONE)
                        {
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Focusing_OK);
                        }
                        this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                        SubModuleState = new SubModuleDoneState(this);
                    }
                    else if (RetVal == EventCodeEnum.NONE)
                    {
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Focusing_OK);
                        PMResult pmRet;

                        // OnlyPatternMatching 모드인 경우 조명이 꺼진상태로 시작되므로 항상 조명을 켜준다.
                        if (EnumMarkAlignProcMode.OnlyPatternMatching == this.MarkAligner().MarkAlignProcMode)
                        {
                            if (this.MarkAlignParam.MarkPatMatParam.LightParams == null)
                            {
                                var phRefLight = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 255);
                            }
                            else
                            {
                                this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, this.MarkAlignParam.MarkPatMatParam.LightParams[0].Value.Value);
                            }
                        }

                        pmRet = DoPatternMatching();

                        // MARK_Pattern_Failure for Testing
                        if (this.MarkAligner().MarkAlignControlItems.MARK_Pattern_Failure == true)
                        {
                            RetVal = EventCodeEnum.MARK_Pattern_Failure;
                        }

                        if (pmRet != null && pmRet.ResultParam != null && RetVal != EventCodeEnum.MARK_Pattern_Failure)
                        {
                            // MARK_ALGIN_PATTERN_MATCH_FAILED for Testing
                            if (this.MarkAligner().MarkAlignControlItems.MARK_ALGIN_PATTERN_MATCH_FAILED == true)
                            {
                                RetVal = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
                            }

                            if (pmRet.ResultParam.Count == 1 && RetVal != EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED)
                            {
                                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Pattern_OK);
                                var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                                var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                                double tolX = 150.0;
                                double tolY = 150.0;

                                tolX = param.MarkDiffTolerance_X.Value;
                                tolY = param.MarkDiffTolerance_Y.Value;

                                if (tolX < 1.0)
                                {
                                    tolX = 1.0;
                                }
                                else if (tolX > param.MarkDiffTolerance_X.UpperLimit)
                                {
                                    tolX = param.MarkDiffTolerance_X.UpperLimit;
                                }

                                if (tolY < 1.0)
                                {
                                    tolY = 1.0;
                                }
                                else if (tolY > param.MarkDiffTolerance_Y.UpperLimit)
                                {
                                    tolY = param.MarkDiffTolerance_Y.UpperLimit;
                                }

                                double actualX = 0;
                                this.MotionManager().GetRefPos(EnumAxisConstants.X, ref actualX);

                                double offsetX = (pmRet.ResultBuffer.SizeX / 2 - pmRet.ResultParam[0].XPoss) * (this.VisionManager().CameraDescriptor.Cams[0].GetRatioX());
                                double pattenActualX = actualX + offsetX;

                                double actualY = 0;
                                this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref actualY);

                                double actualPZ = 0;
                                this.MotionManager().GetRefPos(EnumAxisConstants.PZ, ref actualPZ);

                                double offsetY = (pmRet.ResultBuffer.SizeY / 2 - pmRet.ResultParam[0].YPoss) * (this.VisionManager().CameraDescriptor.Cams[0].GetRatioY());
                                double pattenActualY = actualY - offsetY;

                                LoggerManager.Debug($"Mark Shift Ratio X :{this.VisionManager().CameraDescriptor.Cams[0].GetRatioX()}, Ratio Y : {this.VisionManager().CameraDescriptor.Cams[0].GetRatioY()}", isInfo: IsInfo);
                                LoggerManager.Debug($"Mark Shift Pixel X :{pmRet.ResultParam[0].XPoss}, Pixel Y : {pmRet.ResultParam[0].YPoss}", isInfo: IsInfo);
                                LoggerManager.Debug($"Mark Shift Offset Pixel X :{(pmRet.ResultBuffer.SizeX / 2 - pmRet.ResultParam[0].XPoss)}, Offset Pixel Y : {(pmRet.ResultBuffer.SizeY / 2 - pmRet.ResultParam[0].YPoss)}", isInfo: IsInfo);
                                LoggerManager.Debug($"Mark Shift Offset X :{offsetX}, Offset Y : {offsetY}", isInfo: IsInfo);

                                LoggerManager.CompVerifyLog($"Mark Shift Ratio X :{this.VisionManager().CameraDescriptor.Cams[0].GetRatioX()}, Ratio Y : {this.VisionManager().CameraDescriptor.Cams[0].GetRatioY()}");
                                LoggerManager.CompVerifyLog($"Mark Shift Pixel X :{pmRet.ResultParam[0].XPoss}, Pixel Y : {pmRet.ResultParam[0].YPoss}");
                                LoggerManager.CompVerifyLog($"Mark Shift Offset Pixel X :{(pmRet.ResultBuffer.SizeX / 2 - pmRet.ResultParam[0].XPoss)}, Offset Pixel Y : {(pmRet.ResultBuffer.SizeY / 2 - pmRet.ResultParam[0].YPoss)}");
                                LoggerManager.CompVerifyLog($"Mark Shift Offset X :{offsetX}, Offset Y : {offsetY}");

                                if (this.MarkAligner().IsOnDebugMode || this.MarkAligner().IsSaveImageCompVerify)
                                {
                                    string pathbase = this.WaferAligner().IsOnDebugImagePathBase;
                                    var resultImageBuf = pmRet.ResultBuffer.Buffer;
                                    if (resultImageBuf != null)
                                    {
                                        string NowTime = DateTime.Now.ToString("yyMMddHHmmss");
                                        string path = $"{pathbase}[MA-PM]_XPOS#{pattenActualX}_YPOS#{pattenActualY}_{NowTime}.bmp";

                                        var imgBuffer = new ImageBuffer(
                                            resultImageBuf, pmRet.ResultBuffer.SizeX, pmRet.ResultBuffer.SizeY,
                                            pmRet.ResultBuffer.Band, 24);

                                        if (this.MarkAligner().IsOnDebugMode)
                                        {
                                            this.VisionManager().SaveImageBuffer(imgBuffer, path, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                        }

                                        if (this.MarkAligner().IsSaveImageCompVerify)
                                        {
                                            pathbase = this.MarkAligner().CompVerifyImagePathBase;
                                            path = $"{pathbase}CompVerify_{NowTime}.bmp";
                                            this.VisionManager().SaveImageBuffer(imgBuffer, path, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                        }

                                        LoggerManager.Debug($"[Mark Align][PM] result save to {path}");
                                    }
                                }

                                if (RetVal != EventCodeEnum.MARK_ALIGN_SHIFT)
                                {
                                    this.MotionManager().AbsMove(axisX, pattenActualX);
                                    this.MotionManager().AbsMove(axisY, pattenActualY);

                                    //double diifMarkPosX = pattenActualX - this.CoordinateManager().StageCoord.MarkEncPos.X.Value;
                                    //double diifMarkPosY = pattenActualY - this.CoordinateManager().StageCoord.MarkEncPos.Y.Value;

                                    double diifMarkPosX = 0.0;
                                    double diifMarkPosY = 0.0;
                                    double diifMarkPosZ = 0.0;

                                    diifMarkPosX = pattenActualX - this.CoordinateManager().StageCoord.RefMarkPos.X.Value;
                                    diifMarkPosY = pattenActualY - this.CoordinateManager().StageCoord.RefMarkPos.Y.Value;
                                    diifMarkPosZ = actualPZ - this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;


                                    LoggerManager.Debug($"Previous Aligned Mark position : ({this.CoordinateManager().StageCoord.RefMarkPos.X.Value}, {this.CoordinateManager().StageCoord.RefMarkPos.Y.Value}, {this.CoordinateManager().StageCoord.RefMarkPos.Z.Value})", isInfo: IsInfo);
                                    LoggerManager.ActionLog(ModuleLogType.MARK_ALIGN, StateLogType.DONE, $"Now Aligned Mark position : ({pattenActualX}, {pattenActualY}, {actualPZ})", this.LoaderController().GetChuckIndex());
                                    LoggerManager.Debug($"Mark position is changed ({diifMarkPosX}, {diifMarkPosY}, {diifMarkPosZ})", isInfo: IsInfo);

                                    LoggerManager.CompVerifyLog($"Previous Aligned Mark position : ({this.CoordinateManager().StageCoord.RefMarkPos.X.Value}, {this.CoordinateManager().StageCoord.RefMarkPos.Y.Value}, {this.CoordinateManager().StageCoord.RefMarkPos.Z.Value})");
                                    LoggerManager.CompVerifyLog($"Now Aligned Mark position : ({pattenActualX}, {pattenActualY}, {actualPZ})");
                                    LoggerManager.CompVerifyLog($"Mark position is changed ({diifMarkPosX}, {diifMarkPosY}, {diifMarkPosZ})");

                                    this.CoordinateManager().StageCoord.RefMarkPos.X.Value = pattenActualX;
                                    this.CoordinateManager().StageCoord.RefMarkPos.Y.Value = pattenActualY;
                                    this.CoordinateManager().StageCoord.RefMarkPos.Z.Value = actualPZ;

                                    this.MarkAligner().MarkCumulativeChangeValue.X.Value += diifMarkPosX;
                                    this.MarkAligner().MarkCumulativeChangeValue.Y.Value += diifMarkPosY;
                                    this.MarkAligner().MarkCumulativeChangeValue.Z.Value += diifMarkPosZ;

                                    this.MarkAligner().DiffMarkPosX = diifMarkPosX;
                                    this.MarkAligner().DiffMarkPosY = diifMarkPosY;

                                    this.GEMModule().GetPIVContainer().SetMarkChangeValue(diifMarkPosX, diifMarkPosY, diifMarkPosZ);

                                    LoggerManager.Debug($"Updated Aligned Mark position : ({this.CoordinateManager().StageCoord.RefMarkPos.X.Value}, {this.CoordinateManager().StageCoord.RefMarkPos.Y.Value}, {this.CoordinateManager().StageCoord.RefMarkPos.Z.Value})", isInfo: IsInfo);

                                    LoggerManager.Debug($"Mark position cumulative change value : ({this.MarkAligner().MarkCumulativeChangeValue.X.Value}, {this.MarkAligner().MarkCumulativeChangeValue.Y.Value}, {this.MarkAligner().MarkCumulativeChangeValue.Z.Value})", isInfo: IsInfo);
                                    LoggerManager.CompVerifyLog($"Mark position cumulative change value : ({this.MarkAligner().MarkCumulativeChangeValue.X.Value}, {this.MarkAligner().MarkCumulativeChangeValue.Y.Value}, {this.MarkAligner().MarkCumulativeChangeValue.Z.Value})");

                                    WaferObject wafer = (WaferObject)this.StageSupervisor().WaferObject;

                                    if (this.MarkAligner().UpdatePadCen)
                                    {
                                        LoggerManager.Debug($"Shift wafer data... (x : {diifMarkPosX}, y : {offsetY})", isInfo: IsInfo);
                                        LoggerManager.Debug($"Before Wafer Center (x : {wafer.GetSubsInfo().WaferCenter.X.Value}, y : {wafer.GetSubsInfo().WaferCenter.Y.Value})", isInfo: IsInfo);
                                        LoggerManager.Debug($"Before RefDieLeftCorner data... (x : {wafer.GetSubsInfo().RefDieLeftCorner.X.Value}, y : {wafer.GetSubsInfo().RefDieLeftCorner.Y.Value})", isInfo: IsInfo);

                                        wafer.GetSubsInfo().WaferCenter.X.Value += diifMarkPosX;
                                        wafer.GetSubsInfo().WaferCenter.Y.Value += diifMarkPosY;

                                        wafer.GetSubsInfo().RefDieLeftCorner.X.Value += diifMarkPosX;
                                        wafer.GetSubsInfo().RefDieLeftCorner.Y.Value += diifMarkPosY;

                                        LoggerManager.Debug($"After Wafer Center (x : {wafer.GetSubsInfo().WaferCenter.X.Value}, y : {wafer.GetSubsInfo().WaferCenter.Y.Value})", isInfo: IsInfo);
                                        LoggerManager.Debug($"After RefDieLeftCorner data... (x : {wafer.GetSubsInfo().RefDieLeftCorner.X.Value}, y : {wafer.GetSubsInfo().RefDieLeftCorner.Y.Value})", isInfo: IsInfo);
                                    }

                                    string line = string.Format("X: {0,-15:0.000} Y: {1,-15:0.000} PZ: {2,-15:0.000}", pattenActualX, pattenActualY, actualPZ);

                                    var PinAlignParam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

                                    // Update pin position
                                    if (this.MarkAligner().GetMarkCompensationEnable() || this.MarkAligner().GetPinCompensationEnable())
                                    {
                                        if (MarkInitiated)
                                        {
                                            this.StageSupervisor().ProbeCardInfo.ShiftPindata((diifMarkPosX * -1), (diifMarkPosY * -1), (diifMarkPosZ * -1));

                                            if (PinAlignParam.UserPinHeight.Value == USERPINHEIGHT.LOWEST)
                                            {
                                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight = this.StageSupervisor().ProbeCardInfo.CalcLowestPin();
                                            }
                                            else if (PinAlignParam.UserPinHeight.Value == USERPINHEIGHT.HIGHEST)
                                            {
                                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight = this.StageSupervisor().ProbeCardInfo.CalcHighestPin();
                                            }
                                            else if (PinAlignParam.UserPinHeight.Value == USERPINHEIGHT.AVERAGE)
                                            {
                                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight = this.StageSupervisor().ProbeCardInfo.CalcPinAverageHeight(); // this.StageSupervisor().ProbeCardInfo.PinAverageHeight;
                                            }
                                            this.StageSupervisor().SaveProberCard();
                                            LoggerManager.Debug($"pin position updated ({this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX:0.00}, " +
                                                                $"{this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY:0.00}," +
                                                                $"{this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight:0.00})", isInfo: IsInfo);
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Initial mark update. Skip pin coordinate updates.", isInfo: IsInfo);
                                            LoggerManager.Debug($"Preserve pin positions ({this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX:0.00}, " +
                                                                $"{this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY:0.00}," +
                                                                $"{this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight:0.00})", isInfo: IsInfo);
                                            MarkInitiated = true;
                                        }

                                    }
                                    else
                                    {
                                        LoggerManager.Debug("MarkCompensationEnable is False");
                                    }

                                    if (MarkInitiated)
                                    {
                                        // NC 터치센서 위치 업데이트
                                        this.StageSupervisor().NCObject.SensorPos.Value.X.Value -= diifMarkPosX;
                                        this.StageSupervisor().NCObject.SensorPos.Value.Y.Value -= diifMarkPosY;
                                        this.StageSupervisor().NCObject.SensorPos.Value.Z.Value -= diifMarkPosZ;
                                        this.StageSupervisor().NCObject.SensorFocusedPos.Value.X.Value -= diifMarkPosX;
                                        this.StageSupervisor().NCObject.SensorFocusedPos.Value.Y.Value -= diifMarkPosY;
                                        this.StageSupervisor().NCObject.SensorFocusedPos.Value.Z.Value -= diifMarkPosZ;
                                        this.StageSupervisor().NCObject.SensorBasePos.Value.X.Value -= diifMarkPosX;
                                        this.StageSupervisor().NCObject.SensorBasePos.Value.Y.Value -= diifMarkPosY;
                                        this.StageSupervisor().NCObject.SensorBasePos.Value.Z.Value -= diifMarkPosZ;
                                        this.StageSupervisor().NCObject.SensingPadBasePos.Value.X.Value += diifMarkPosX;
                                        this.StageSupervisor().NCObject.SensingPadBasePos.Value.Y.Value += diifMarkPosY;
                                        this.StageSupervisor().NCObject.SensingPadBasePos.Value.Z.Value += diifMarkPosZ;
                                        LoggerManager.Debug($"Update NC sensor coordinate. ({this.StageSupervisor().NCObject.SensorPos.Value.X.Value:0.00}, {this.StageSupervisor().NCObject.SensorPos.Value.Y.Value:0.00}, {this.StageSupervisor().NCObject.SensorPos.Value.Z.Value:0.00}", isInfo: IsInfo);
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"Preserve NC sensor coordinate. ({this.StageSupervisor().NCObject.SensorPos.Value.X.Value:0.00}, {this.StageSupervisor().NCObject.SensorPos.Value.Y.Value:0.00}, {this.StageSupervisor().NCObject.SensorPos.Value.Z.Value:0.00}", isInfo: IsInfo);
                                    }

                                    if (Math.Abs(this.MarkAligner().DiffMarkPosX) > tolX
                                        || Math.Abs(this.MarkAligner().DiffMarkPosY) > tolY
                                        || this.MarkAligner().MarkAlignControlItems.MARK_ALIGN_SHIFT)
                                    {
                                        RetVal = EventCodeEnum.MARK_ALIGN_SHIFT;
                                        LoggerManager.Debug($"MKOpusVProcess.DoExecute(): Diff. = ({this.MarkAligner().DiffMarkPosX}, {this.MarkAligner().DiffMarkPosY}), Tol. = ({tolX:0.0}, {tolY:0.0})", isInfo: IsInfo);
                                        this.NotifyManager().Notify(RetVal);
                                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_ALIGN_SHIFT);
                                        LoggerManager.Debug("[MarkAlign] MARK_ALIGN_SHIFT Error.  ErrorCode=MARK_ALIGN_SHIFT");
                                        SubModuleState = new SubModuleErrorState(this);
                                    }
                                    else
                                    {
                                        RetVal = EventCodeEnum.NONE;
                                        this.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                                        SubModuleState = new SubModuleDoneState(this);
                                    }
                                }
                                else
                                {
                                    RetVal = EventCodeEnum.MARK_ALIGN_SHIFT;
                                    this.NotifyManager().Notify(RetVal);
                                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_ALIGN_SHIFT);
                                    LoggerManager.Debug("[MarkAlign] MARK_ALIGN_SHIFT Error.  ErrorCode=MARK_ALIGN_SHIFT");
                                    SubModuleState = new SubModuleErrorState(this);
                                }
                            }
                            else
                           {
                                RetVal = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
                                this.NotifyManager().Notify(RetVal);
                                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Pattern_Failure);
                                LoggerManager.Debug("[MarkAlign] PatternMatching Error.  ErrorCode=MARK_ALGIN_PATTERN_MATCH_FAILED");
                                SubModuleState = new SubModuleErrorState(this);
                            }
                        }
                        else
                        {
                            RetVal = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
                            this.NotifyManager().Notify(RetVal);
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Pattern_Failure);

                            SubModuleState = new SubModuleErrorState(this);
                        }

                        this.VisionManager().StartGrab(MarkAlignParam.MarkPatMatParam.CamType.Value, this);
                    }
                    else
                    {
                        //Focusing Error
                        RetVal = EventCodeEnum.MARK_ALIGN_FOCUSING_FAILED;
                        this.NotifyManager().Notify(RetVal);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Focusing_Failure);
                        LoggerManager.Debug("[MarkAlign] Focusing Error.  ErrorCode=" + RetVal);
                        SubModuleState = new SubModuleErrorState(this);
                    }
                }
                else
                {
                    //Move To Mark ERROR
                    RetVal = EventCodeEnum.MARK_ALIGN_MOVE_ERROR;
                    this.NotifyManager().Notify(RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.MARK_Move_Failure);
                    LoggerManager.Debug("[MarkAlign] MoveToMark Error.  ErrorCode=" + RetVal);
                    SubModuleState = new SubModuleErrorState(this);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                RetVal = EventCodeEnum.MARK_ALIGN_FAIL;
                this.NotifyManager().Notify(RetVal);

                SubModuleState = new SubModuleErrorState(this);
            }
            finally
            {
                if (!MarkInitiated)
                {
                    LoggerManager.Debug($"Mark initial update failed.");
                }

                if (this.MarkAligner().ForceWaferCamCylinderExtended)
                {
                    this.MarkAligner().ForceWaferCamCylinderExtended = false;
                    LoggerManager.Debug($"MarkAligner ForceWaferCamCylinderExtended set to {this.MarkAligner().ForceWaferCamCylinderExtended}");
                }
                RestoreLightsForCameras();

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.ActionLog(ModuleLogType.MARK_ALIGN, StateLogType.ERROR, $"Mark Align Fail({RetVal})", this.LoaderController().GetChuckIndex());
                }
            }
            return RetVal;
        }

        public override Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MainViewTarget = DisplayPort;

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                InitLightJog(this);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                FiveButton.IconSource = null;
                FiveButton.IconCaption = "";
                FiveButton.Command = null;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        Task<EventCodeEnum> ISetup.InitSetup()
        {
            Task<EventCodeEnum> task;
            try
            {

                task = Task.Run(() =>
                {
                    return EventCodeEnum.NONE;
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return task;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "MarkAligner";

                retVal = InitPnpModuleStage();
                retVal = InitPNPSetupUI();

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public override void SetStepSetupState(string header = null)
        {
            try
            {
                if (this.StageSupervisor().MarkObject.AlignState.Value == AlignStateEnum.DONE)
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                else
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
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
                SetEnableState(ProberInterfaces.State.EnumEnableState.ENABLE);
                //StateEnable = ProberInterfaces.State.EnumEnableState.ENABLE;
                //ObservableCollection<ObservableCollection<ICategoryNodeItem>> items
                //     = new ObservableCollection<ObservableCollection<ICategoryNodeItem>>();
                //items.Add(new ObservableCollection<ICategoryNodeItem>())
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return TemplateToPnpConverter.Converter(this, true);
        }
        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                UseUserControl = UserControlFucEnum.PTRECT;
                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");

                //PadJogLeft.Command = new RelayCommand(PatterinSizeMinus);
                //PadJogRight.Command = new RelayCommand(PatterinSizePlus);

                PadJogLeft.Command = new RelayCommand(PatterinSizeMinus);
                PadJogRight.Command = new RelayCommand(PatterinSizePlus);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Add.png");
                //OneButton.MaskingLevel = 3;
                OneButton.IconCaption = "REGIST";
                OneButton.Command = RegistePatternCommand;

                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Move.png");
                //ThreeButton.MaskingLevel = 3;
                TwoButton.IconCaption = "MOVE";
                TwoButton.Command = (ICommand)MoveToMarkCommand;

                //FourButton.MaskingLevel = 3;
                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                ThreeButton.IconCaption = "FOCUSING";
                ThreeButton.Command = (ICommand)FocusingCommand;

                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/checkbox-multi-W.png");
                FourButton.IconCaption = "DOALIGN";
                FourButton.Command = (ICommand)DoMarkAlignCommand;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitPnpUI() : Error occrued.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        //public EventCodeEnum LoadDevParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
    }
}
