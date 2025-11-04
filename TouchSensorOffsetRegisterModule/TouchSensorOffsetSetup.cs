using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using System.ComponentModel;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using LogModule;
using NeedleCleanerModuleParameter;
using RelayCommandBase;
using System.Windows;
using System.Windows.Input;
using ProberInterfaces.Param;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SubstrateObjects;
using MetroDialogInterfaces;
using TouchSensorSystemParameter;
using ProberInterfaces.TouchSensor;

namespace TouchSensorOffsetRegisterModule
{
    public class TouchSensorOffsetSetup : PNPSetupBase, IHasSysParameterizable, ITemplateModule, INotifyPropertyChanged, ISetup, IParamNode, ITouchSensorCalcOffsetModule
    {
        public override Guid ScreenGUID { get; } = new Guid("2AA2C7F4-8171-1A9D-0EBD-C8ED49A3E6A0");

        public override bool Initialized { get; set; } = false;

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Parameter 확인한다.                
            }
            catch (Exception err)
            {
                //LoggerManager.Debug(err);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private IStateModule _TouchSensorOffsetModule;
        public IStateModule TouchSensorOffsetModule
        {
            get { return _TouchSensorOffsetModule; }
            set
            {
                if (value != _TouchSensorOffsetModule)
                {
                    _TouchSensorOffsetModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TouchSensorOffsetSetup()
        {

        }
        public TouchSensorOffsetSetup(IStateModule Module)
        {
            TouchSensorOffsetModule = Module;
        }

        private IFocusing _TouchSensorOffsetFocusModel;

        public IFocusing TouchSensorOffsetFocusModel
        {
            get
            {
                if (_TouchSensorOffsetFocusModel == null)
                    _TouchSensorOffsetFocusModel = this.FocusManager().GetFocusingModel((this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusingModuleDllInfo);

                return _TouchSensorOffsetFocusModel;
            }
            set { TouchSensorOffsetFocusModel = value; }
        }
        private IFocusParameter TouchSensorOffsetFocusParam => (this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusParam;

        private AsyncCommand _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(CmdFocusing, new Func<bool>(() => !FocusingCommandCanExecute));
                return _FocusingCommand;
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

            Task task = new Task(() =>
            {
                ret = FocusingFunc();
            });
            task.Start();
            await task;
            return ret;
        }

        //private IParam _DevParam;
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
        //private IParam _SysParam;
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

        public new NeedleCleanObject NC { get { return (NeedleCleanObject)this.StageSupervisor().NCObject; } }

        public new List<object> Nodes { get; set; }
        public SubModuleStateBase SubModuleState { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }


        private TouchSensorSysParameter _TouchSensorParam;
        public TouchSensorSysParameter TouchSensorParam
        {
            get { return _TouchSensorParam; }
            set
            {
                if (value != _TouchSensorParam)
                {
                    _TouchSensorParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {


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

                try
                {
                    //RetVal = this.StageSupervisor().LoadNCSysObject();
                    //if (NeedleCleanSysParam == null)
                    //    NeedleCleanSysParam = new NeedleCleanSystemParameter();

                    //((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).CopyTo(NeedleCleanSysParam);

                    //NeedleCleanSysParam = this.SageSupervisor().NCObject.NCSysParam_IParam.Copy() as NeedleCleanSystemParameter;
                    
                    RetVal = this.StageSupervisor().LoadTouchSensorObject();
                    TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;
                    RetVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 
            }
            return RetVal;
        }

        private bool ParamChanged = false;

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //NeedleCleanSysParam.CopyToSensoOffsetReg((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam);
                //retVal = this.StageSupervisor.SaveNCSysObject();

                retVal = this.StageSupervisor().TouchSensorObject.SaveSysParameter();


                ParamChanged = false;
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 
            }
            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = IsParamChanged & ParamChanged;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {
                if (TouchSensorParam.TouchSensorOffsetRegistered.Value == true)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private IWaferObject Wafer { get { return this.StageSupervisor().WaferObject; } }
        private new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }

        public EventCodeEnum DoExecute() //실제 프로세싱 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            /*
                실제 프로세싱 코드 작성   
             */
            return retVal;
        }

        public EventCodeEnum DoClearData() //현재 Parameter Check 및 Init하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
            //  return ParamValidation();
        }

        public EventCodeEnum DoRecovery() // Recovery때 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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
        public bool IsExecute() //SubModule이 Processing 가능한지 판단하는 조건 
        {
            return true;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if (TouchSensorOffsetFocusParam == null)
                    {
                        TouchSensorOffsetFocusParam.SetDefaultParam();

                        TouchSensorOffsetFocusParam.FocusRange.Value = 500;
                        TouchSensorOffsetFocusParam.OutFocusLimit.Value = 20;
                        TouchSensorOffsetFocusParam.FocusingROI.Value = new Rect(0, 0, 960, 960);
                        TouchSensorOffsetFocusParam.CheckDualPeak.Value = true;
                        TouchSensorOffsetFocusParam.CheckFlatness.Value = true;
                        TouchSensorOffsetFocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                        TouchSensorOffsetFocusParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;
                        TouchSensorOffsetFocusParam.FocusMaxStep.Value = 50;
                        TouchSensorOffsetFocusParam.DepthOfField.Value = 1;
                        TouchSensorOffsetFocusParam.FlatnessThreshold.Value = 60;
                    }

                    FocusingCommandCanExecute = true;
                    this.StageSupervisor = this.StageSupervisor();

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
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - InitModule() : Error occured.");
                LoggerManager.Exception(err);
                retval = EventCodeEnum.UNKNOWN_EXCEPTION;
                //MovingState.Stop();
            }

            return retval;
        }

        public override void DeInitModule()
        {
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Calculate Sensor Offset";

                retVal = InitPnpModuleStage();

                //MainPageView = new UcNeedleCleanMainPage();

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                //LoggerManager.Debug(err);
                LoggerManager.Exception(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await InitSetup();
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                SetStepSetupState();
                // TO DO : 터치 센서 위치로 이동

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                //LoggerManager.Debug(err);
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitPnpUI();

                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                // MainView 화면에 Camera(Vision) 화면이 나온다.
                MainViewTarget = DisplayPort;

                // 파라미터 업데이트
                retVal = LoadSysParameter();

                // 카메라 변경
                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                // 조명 설정
                foreach (var light in TouchSensorParam.LightForFocusSensor)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }
                InitLightJog(this);
                ParamValidation();
            }
            catch (Exception err)
            {
                //LoggerManager.Debug(err);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InitPnpUI();
            }
            catch (Exception err)
            {
                //LoggerManager.Debug(err);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPnpUI()
        {
            // Button 들에서 설정할수 있는것들.
            // Caption : 버튼에 보여줄 문자
            // CaptionSize : 문자 크기
            // IconSource : 버튼에 보여줄 이미지
            // RepeatEnable : true - 버튼에 Repeat 기능 활성화 / false - 버튼에 Repeat 기능 비활성화
            // Visibility : Visibility.Visible - 버튼 보임 / Visibility.Hidden - 버튼 안보임
            // IsEnabled : true - 버튼 활성화 / false - 버튼 비활성화

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //StepLabel = "";

                OneButton.Caption = null;
                OneButton.Command = null;
                OneButton.IsEnabled = false;
                //OneButton.Visibility = System.Windows.Visibility.Visible;

                TwoButton.Caption = null;
                TwoButton.Command = null;
                TwoButton.IsEnabled = false;

                ThreeButton.Caption = null;
                ThreeButton.Command = null;
                ThreeButton.IsEnabled = false;

                FourButton.Caption = null;
                FourButton.Command = null;
                FourButton.IsEnabled = false;

                PadJogLeftUp.Caption = null;
                PadJogLeftUp.Command = null;
                PadJogLeftUp.IsEnabled = false;

                PadJogRightUp.Caption = null;
                PadJogRightUp.Command = null;
                PadJogRightUp.IsEnabled = false;

                PadJogLeftDown.Caption = null;
                PadJogLeftDown.Command = null;
                PadJogLeftDown.IsEnabled = false;

                PadJogRightDown.Caption = null;
                PadJogRightDown.Command = null;
                PadJogRightDown.IsEnabled = false;

                PadJogSelect.Caption = "SET";
                PadJogSelect.Command = new AsyncCommand(SetupCommand);

                PadJogLeft.Caption = null;
                PadJogLeft.Command = null;
                PadJogLeft.IsEnabled = false;

                PadJogRight.Caption = null;
                PadJogRight.Command = null;
                PadJogRight.IsEnabled = false;

                PadJogUp.Caption = null;
                PadJogUp.Command = null;
                PadJogUp.IsEnabled = false;

                PadJogDown.Caption = null;
                PadJogDown.Command = null;
                PadJogDown.IsEnabled = false;
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - InitPnpUI() : Error occured.");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //MovingState.Stop();
            }

            return retVal;
        }

        public EventCodeEnum FocusingFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                FocusingCommandCanExecute = false;

                RetVal = TouchSensorOffsetFocusModel.Focusing_Retry(TouchSensorOffsetFocusParam, false, false, false, this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            finally
            {
                FocusingCommandCanExecute = true;

            }
            return RetVal;
        }

        private double CurNcHeight()
        {
            NCCoordinate nccoord = new NCCoordinate();

            try
            {
                nccoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
                return nccoord.GetZ();
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - CurNcHeight() : Error occured.");
                throw new NotImplementedException();

            }

        }
        private double CurPinHeight()
        {
            PinCoordinate pincoord = new PinCoordinate();

            try
            {
                pincoord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                return pincoord.GetZ();
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - CurNcHeight() : Error occured.");
                throw new NotImplementedException();

            }

        }

        private EventCodeEnum NcRelMoveZ(double inc)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = this.MotionManager().RelMove(EnumAxisConstants.PZ, inc);
                System.Threading.Thread.Sleep(100);
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - NcRelMoveZ() : Error occured.");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }

            return retVal;
        }

        public EventCodeEnum DoAutoDetect(double pzclearnce)
        {
            // This focusing function must be used whenb nc disk position is valid
            // In case nc disk position is not set yet, use rough focusing

            LoggerManager.Debug($"Begin DoAutoDetect()");

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            if (this.MotionManager().IsEmulMode(this.MotionManager().GetAxis(EnumAxisConstants.PZ)))
            {
                return EventCodeEnum.NONE;
            }

            try
            {
                double orgHeight;
                double curZ;
                bool bFound;

                orgHeight = CurNcHeight();

                if (this.NeedleCleaner().IsNCSensorON() == true)
                {
                    // Already touch sensor is ON, move down firstly
                    for (int m = 1; m <= pzclearnce/10; m++)
                    {
                        RetVal = NcRelMoveZ(-100);
                        if (RetVal != EventCodeEnum.NONE) { return RetVal; }

                        if (this.NeedleCleaner().IsNCSensorON() == false)
                        {
                            // For clearance, z down one more time
                            RetVal = NcRelMoveZ(-(pzclearnce/5));
                            if (RetVal != EventCodeEnum.NONE) { return RetVal; }
                            break;
                        }
                    }
                }

                bFound = false;
                for (int m = 1; m <= pzclearnce/10; m++)
                {
                    RetVal = NcRelMoveZ(100);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }

                    if (this.NeedleCleaner().IsNCSensorON() == true)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    // Error exception
                    // No response from sensor, break operation
                    LoggerManager.Debug($"Could not find sensed height in step 1");
                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;

                    curZ = CurNcHeight();
                    RetVal = NcRelMoveZ(orgHeight - curZ);

                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;
                    return RetVal;
                }

                bFound = false;
                for (int m = 1; m <= pzclearnce/10; m++)
                {
                    RetVal = NcRelMoveZ(-10);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }

                    if (this.NeedleCleaner().IsNCSensorON() == false)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    // Error exception
                    // No response from sensor, break operation
                    LoggerManager.Debug($"Could not find sensed height in step 2");
                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;

                    curZ = CurNcHeight();
                    RetVal = NcRelMoveZ(orgHeight - curZ);

                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;
                    return RetVal;
                }

                bFound = false;
                for (int m = 1; m <= pzclearnce/5; m++)
                {
                    RetVal = NcRelMoveZ(1);
                    if (RetVal != EventCodeEnum.NONE) { return RetVal; }

                    if (this.NeedleCleaner().IsNCSensorON() == true)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    // Error exception
                    // No response from sensor, break operation
                    LoggerManager.Debug($"Could not find sensed height in step 3");
                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;

                    curZ = CurNcHeight();
                    RetVal = NcRelMoveZ(orgHeight - curZ);

                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_DETECTED;
                    return RetVal;
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - DoAutoDetect() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;

            }

            return RetVal;
        }

        private async Task SetupCommand()
        {
            try
            {
                if (this.StageSupervisor().MarkObject.AlignState.Value == AlignStateEnum.IDLE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Operation Failure", "Mark alignment was failed.\nIt must be done before this process.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    
                    SetCalcOffsetCommand();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                
            }
        }

        public EventCodeEnum SetCalcOffsetCommand()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            double expected = 0;
            double actual = 0;
            double offset = 0;
            double pzclearnce = 0.0;
            try
            {
                TouchSensorParam.TouchSensorOffsetRegistered.Value = false;

                // TODO : 알람 메세지 띄우기
                if (TouchSensorParam.TouchSensorRegistered.Value != true || TouchSensorParam.TouchSensorBaseRegistered.Value != true ||
                TouchSensorParam.TouchSensorPadBaseRegistered.Value != true)
                {
                    LoggerManager.Debug("Previous registration steps are not finished yet!");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Offset_Failure, RetVal);
                    RetVal = EventCodeEnum.TOUCH_SENSOR_NOT_READY;
                    return RetVal;
                }

                //TO DO: 화면 락 시키기

                // 패드 베이스 위치에 터치 센서로 실제 찍어보기
                StageSupervisor.StageModuleState.ZCLEARED();

                // Zdown to safe height 
                if (NC.NCSysParam != null)
                {
                    if (NC.NCSysParam.CleanUnitAttached.Value == true)
                    {
                        RetVal = this.NeedleCleaner().CleanPadDown(true);

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Failed to move down clean pad");
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Offset_Failure, RetVal);

                            return RetVal;
                        }
                    }
                }

                pzclearnce = 500;

                RetVal = StageSupervisor.StageModuleState.TouchSensorSensingMoveNCPad(TouchSensorParam.SensingPadBasePos.Value, TouchSensorParam.SensorFocusedPos.Value, -pzclearnce);
                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("[TouchSensorOffsetSetupModule] SetCalcOffsetCommand(), TouchSensorSensingMoveNCPad Error");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Move_Position_Failure, RetVal);
                    return RetVal;
                }

                // 현재 높이 저장
                expected = CurNcHeight() - pzclearnce;          // 50마이크론 아래에서 센싱을 시작했으므로 원래 가려고 했던 높이에는 +500 해준다.

                // 정확한 높이 측정            
                RetVal = DoAutoDetect(pzclearnce);                   //PZ Move.
                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("Sensing failed for touch sensor");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Offset_Failure, RetVal);

                    return RetVal;
                }

                // 오차 계산  (= 센서 옵셋)
                actual = CurNcHeight();
                offset = expected - actual;
                LoggerManager.Debug("Touch sensor offset = " + offset + ", Start Z Pos = " + expected + ", Sensing Z Pos = " + actual);

                if(Math.Abs(offset) <= 150)
                {
                    TouchSensorParam.TouchSensorOffset.Value = offset;

                    // 파라미터 적용
                    TouchSensorParam.SensorPos.Value.X.Value = TouchSensorParam.SensorFocusedPos.Value.X.Value;
                    TouchSensorParam.SensorPos.Value.Y.Value = TouchSensorParam.SensorFocusedPos.Value.Y.Value;
                    TouchSensorParam.SensorPos.Value.Z.Value = TouchSensorParam.SensorFocusedPos.Value.Z.Value + offset;

                    LoggerManager.Debug("SetCalcOffsetCommand() : Focused sensor position = (" + TouchSensorParam.SensorPos.Value.X.Value + ", " +
                        TouchSensorParam.SensorPos.Value.Y.Value + ", " + TouchSensorParam.SensorPos.Value.Z.Value + ")");

                    //TO DO: 화면 언락하기
                    TouchSensorParam.TouchSensorOffsetRegistered.Value = true;

                    RetVal = EventCodeEnum.NONE;
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Offset_OK, RetVal);
                }
                else
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Touch_Sensor_Out_Of_Tolereance, RetVal);
                    return RetVal;
                }

            }

            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. TouchSensorOffsetSetup - SetCalcOffsetCommand() : Error occured.");
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Offset_Failure, RetVal);
            }
            finally
            {
                StageSupervisor.StageModuleState.ZCLEARED();
                ParamChanged = true;
                SaveSysParameter();
                SetStepSetupState();
            }
            return RetVal;
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum TouchSensorCalcOffsetSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            double expected = 0;
            double actual = 0;
            double offset = 0;
            double pzclearnce = 500;

            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();

                // 파라미터 업데이트
                retVal = LoadSysParameter();

                if(retVal == EventCodeEnum.NONE)
                {
                    // 패드 베이스 위치에 터치 센서로 실제 찍어보기
                    retVal = StageSupervisor.StageModuleState.TouchSensorSensingMoveNCPad(TouchSensorParam.SensingPadBasePos.Value, TouchSensorParam.SensorFocusedPos.Value, -pzclearnce);

                    if(retVal == EventCodeEnum.NONE)
                    {
                        CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                        // 조명 설정
                        foreach (var light in TouchSensorParam.LightForFocusSensor)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }

                        ParamValidation();

                        // 현재 높이 저장
                        expected = CurNcHeight() - pzclearnce;          // 50마이크론 아래에서 센싱을 시작했으므로 원래 가려고 했던 높이에는 +500 해준다.
                                                                        // 정확한 높이 측정            
                        retVal = DoAutoDetect(pzclearnce);
                        if (retVal != EventCodeEnum.NONE)
                        {
                            StageSupervisor.StageModuleState.ZCLEARED();
                            LoggerManager.Debug("[TouchSensorCalcOffsetModule] - TouchSensorCalcOffsetSystemInit() : Sensing failed for touch sensor");
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Offset_Failure, retVal);
                            return retVal;
                        }

                        // 오차 계산  (= 센서 옵셋)
                        actual = CurNcHeight();
                        offset = expected - actual;

                        double sensorpos_diff = Math.Abs(offset - TouchSensorParam.TouchSensorOffset.Value);
                        LoggerManager.Debug("[TouchSensorCalcOffsetModule] - TouchSensorCalcOffsetSystemInit() : Touch sensor update offset = " + offset + ", Diff = " + sensorpos_diff);

                        //TO DO: tolerance 
                        if (Math.Abs(offset) <= 150 || sensorpos_diff <= 100)//base tolereance 100um
                        {
                            TouchSensorParam.TouchSensorOffset.Value = offset;

                            // 파라미터 적용
                            TouchSensorParam.SensorPos.Value.X.Value = TouchSensorParam.SensorFocusedPos.Value.X.Value;
                            TouchSensorParam.SensorPos.Value.Y.Value = TouchSensorParam.SensorFocusedPos.Value.Y.Value;
                            TouchSensorParam.SensorPos.Value.Z.Value = TouchSensorParam.SensorFocusedPos.Value.Z.Value + offset;

                            LoggerManager.Debug("[TouchSensorCalcOffsetModule] - TouchSensorCalcOffsetSystemInit() : Focused sensor position = (" + TouchSensorParam.SensorPos.Value.X.Value + ", " +
                                TouchSensorParam.SensorPos.Value.Y.Value + ", " + TouchSensorParam.SensorPos.Value.Z.Value + ")");


                            //TO DO: 화면 언락하기
                            retVal = EventCodeEnum.NONE;
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Offset_OK, retVal);
                        }
                        else
                        {
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Touch_Sensor_Out_Of_Tolereance, retVal);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"{retVal.ToString()}. TouchSensorBaseSetup - SetSensorBasePosCommand() : Move Error.");
                    }
                }
                else
                {
                    LoggerManager.Debug("[TouchSensorCalcOffsetModule] TouchSensorCalcOffsetSystemInit(), TouchSensorSensingMoveNCPad Error");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Move_Position_Failure, retVal);
                    return retVal;
                }

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            finally
            {
                ParamChanged = true;
                SaveSysParameter();
                SetStepSetupState();
            }
            return retVal;
        }
    }
}
