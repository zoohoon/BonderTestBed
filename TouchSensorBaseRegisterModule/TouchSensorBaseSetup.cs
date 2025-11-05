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
using TouchSensorObject;
using ProberInterfaces.Event;
using NotifyEventModule;
using ProberInterfaces.TouchSensor;

namespace TouchSensorBaseRegisterModule
{
    public class TouchSensorBaseSetup : PNPSetupBase, IHasSysParameterizable, ITemplateModule, INotifyPropertyChanged, ISetup, IParamNode, ITouchSensorBaseSetupModule
    {
        public override Guid ScreenGUID { get; } = new Guid("72AC2B4C-2CF9-6D37-D8E7-F91C8D87CAF7");

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

        private IStateModule _TouchSensorBaseModule;
        public IStateModule TouchSensorBaseModule
        {
            get { return _TouchSensorBaseModule; }
            set
            {
                if (value != _TouchSensorBaseModule)
                {
                    _TouchSensorBaseModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TouchSensorBaseSetup()
        {

        }
        public TouchSensorBaseSetup(IStateModule Module)
        {
            _TouchSensorBaseModule = Module;
        }

        private IFocusing _TouchSensorBaseFocusModel;

        public IFocusing TouchSensorBaseFocusModel
        {
            get
            {
                if (_TouchSensorBaseFocusModel == null)
                    //_TouchSensorBaseFocusModel = this.FocusManager().GetFocusingModel((this.NeedleCleaner().NeedleCleanSysParam_IParam as NeedleCleanSystemParameter).FocusingModuleDllInfo);

                    _TouchSensorBaseFocusModel = this.FocusManager().GetFocusingModel((this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusingModuleDllInfo);

                return _TouchSensorBaseFocusModel;
            }
            set { _TouchSensorBaseFocusModel = value; }
        }
        private IFocusParameter TouchSensorBaseFocusModelParam => (this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusParam;


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

        //public NeedleCleanObject NC { get { return (NeedleCleanObject)this.StageSupervisor().NCObject; } }
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
                RetVal = this.StageSupervisor().LoadTouchSensorObject();
                TouchSensorParam = this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;

                RetVal = EventCodeEnum.NONE;
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
                //NeedleCleanSysParam.CopyToSensorBaseReg((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam);
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
                if (TouchSensorParam.TouchSensorBaseRegistered.Value == true)
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
                LoggerManager.Exception(err);

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
                    if (TouchSensorBaseFocusModelParam == null)
                    {
                        TouchSensorBaseFocusModelParam.SetDefaultParam();

                        TouchSensorBaseFocusModelParam.FocusRange.Value = 500;
                        TouchSensorBaseFocusModelParam.OutFocusLimit.Value = 20;
                        TouchSensorBaseFocusModelParam.FocusingROI.Value = new Rect(0, 0, 960, 960);
                        TouchSensorBaseFocusModelParam.CheckDualPeak.Value = true;
                        TouchSensorBaseFocusModelParam.CheckFlatness.Value = true;
                        TouchSensorBaseFocusModelParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                        TouchSensorBaseFocusModelParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;
                        TouchSensorBaseFocusModelParam.FocusMaxStep.Value = 50;
                        TouchSensorBaseFocusModelParam.DepthOfField.Value = 1;
                        TouchSensorBaseFocusModelParam.FlatnessThreshold.Value = 60;
                    }

                    FocusingCommandCanExecute = true;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }

                //retVal = TouchSensorBaseFocusModel.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Initialized = false;
                throw err;
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
                Header = "Register Sensor Base";

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


                // 조명 설정
                foreach (var light in TouchSensorParam.LightForBaseFocusSensor)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                //TO DO: 센서 위치로 이동하기
                StageSupervisor.StageModuleState.ZCLEARED();
                retVal = StageSupervisor.StageModuleState.TouchSensorHighViewMove(TouchSensorParam.SensorBasePos.Value.X.Value,
                                                                         TouchSensorParam.SensorBasePos.Value.Y.Value,
                                                                         TouchSensorParam.SensorBasePos.Value.Z.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("[TouchSensorBaseSetupModule] InitSetup(), TouchSensorHighViewMove Error");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Move_Position_Failure, retVal);
                }

                InitLightJog(this);
                this.PnPManager().PnpLightJog.IsUseNC = true;
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
                LoggerManager.Debug($"{err.ToString()}. TouchSensorBaseSetup - InitPnpUI() : Error occured.");
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

                if (TouchSensorBaseFocusModelParam.FocusingCam.Value != EnumProberCam.PIN_HIGH_CAM)
                    TouchSensorBaseFocusModelParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;

                if (TouchSensorBaseFocusModelParam.FocusingAxis.Value == EnumAxisConstants.Undefined)
                    TouchSensorBaseFocusModelParam.FocusingAxis.Value = EnumAxisConstants.PZ;

                if (TouchSensorBaseFocusModelParam.FocusingROI.Value.Width == 0 || TouchSensorBaseFocusModelParam.FocusingROI.Value.Height == 0)
                {
                    Rect tmpRect = new Rect(0, 0, 960, 960);
                    TouchSensorBaseFocusModelParam.FocusingROI.Value = tmpRect;
                }
                if (TouchSensorBaseFocusModelParam.FocusRange.Value <= 0)
                    TouchSensorBaseFocusModelParam.FocusRange.Value = 500;

                RetVal = TouchSensorBaseFocusModel.Focusing_Retry(TouchSensorBaseFocusModelParam, false, false, false, this);
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
                    
                    SetSensorBasePosCommand();
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

        private void SetSensorBasePosCommand()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //TO DO: 화면 락 시키기

            try
            {
                TouchSensorParam.TouchSensorBaseRegistered.Value = false;

                // Pin high로 전환
                if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    PinCoordinate hpcoord = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                    RetVal = StageSupervisor.StageModuleState.PinHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
                }
                else if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    RetVal = EventCodeEnum.NONE;
                }
                else
                {
                    RetVal = EventCodeEnum.INVALID_CAMERA_CHANNEL;
                    LoggerManager.Debug("Unexpected camera type");
                    //WriteProLog(EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, RetVal);
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, RetVal);

                    return;
                }

                if(RetVal == EventCodeEnum.NONE)
                {
                    this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                    // 포커싱 하기
                    RetVal = FocusingFunc();

                    //this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM);

                    if (RetVal != EventCodeEnum.NONE)
                    {
                        SetStepSetupState();
                        LoggerManager.Debug("Focusing failed for touch sensor");
                        //WriteProLog(EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, RetVal);
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, RetVal);
                        return;
                    }

                    // 위치 읽기
                    PinCoordinate cur_ccord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                    TouchSensorParam.SensorBasePos.Value.X.Value = cur_ccord.GetX();
                    TouchSensorParam.SensorBasePos.Value.Y.Value = cur_ccord.GetY();
                    TouchSensorParam.SensorBasePos.Value.Z.Value = cur_ccord.GetZ();

                    LoggerManager.Debug("Focused sensor base position = (" + TouchSensorParam.SensorBasePos.Value.X.Value + ", " +
                        TouchSensorParam.SensorBasePos.Value.Y.Value + ", " + TouchSensorParam.SensorBasePos.Value.Z.Value + ")");

                    // 조명 저장하기
                    TouchSensorParam.LightForBaseFocusSensor.Clear();
                    foreach (var light in CurCam.LightsChannels)
                    {
                        int val = CurCam.GetLight(light.Type.Value);
                        TouchSensorParam.LightForBaseFocusSensor.Add(new LightValueParam(light.Type.Value, (ushort)val));
                    }

                    TouchSensorParam.TouchSensorBaseRegistered.Value = true;
                    //WriteProLog(EventCodeEnum.Register_Touch_Sensor_Base_Position_OK);
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Base_Position_OK, RetVal);
                }
                else
                {
                    LoggerManager.Debug($"{RetVal.ToString()}. TouchSensorBaseSetup - SetSensorBasePosCommand() : PinHighViewMove Error.");
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. TouchSensorBaseSetup - SetSensorBasePosCommand() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                //WriteProLog(EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure);
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, RetVal);
            }
            finally
            {
                ParamChanged = true;
                SaveSysParameter();
                SetStepSetupState();
            }
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum TouchSensorBaseSetupSystemInit(out double diffX, out double diffY, out double diffZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //base_ccord = new PinCoordinate();
            diffX = 0.0;
            diffY = 0.0;
            diffZ = 0.0;

            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();
                // 파라미터 업데이트
                retVal = LoadSysParameter();

                if(retVal == EventCodeEnum.NONE)
                {
                    //TO DO: 센서 위치로 이동하기
                    retVal = this.StageSupervisor().StageModuleState.TouchSensorHighViewMove(TouchSensorParam.SensorBasePos.Value.X.Value,
                                                                             TouchSensorParam.SensorBasePos.Value.Y.Value,
                                                                             TouchSensorParam.SensorBasePos.Value.Z.Value);
                    if(retVal == EventCodeEnum.NONE)
                    {
                        CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                        // 조명 설정
                        foreach (var light in TouchSensorParam.LightForBaseFocusSensor)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }

                        //ParamValidation();

                        if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                        {
                            PinCoordinate hpcoord = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                            retVal = StageSupervisor.StageModuleState.PinHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
                        }
                        else if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.INVALID_CAMERA_CHANNEL;
                            LoggerManager.Debug("[TouchSensorTipSetupModule] - TouchSensorTipSetupSystemInit() : Unexpected camera type");
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, retVal);
                            return retVal;
                        }

                        if(retVal == EventCodeEnum.NONE)
                        {
                            this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                            retVal = FocusingFunc();

                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug("[TouchSensorBaseSetupModule] TouchSensorBaseSetupSystemInit(), Focusing failed for touch sensor");
                                //WriteProLog(EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, RetVal);
                                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, retVal);
                                return retVal;
                            }

                            PinCoordinate base_ccord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                            diffX = base_ccord.GetX() - TouchSensorParam.SensorBasePos.Value.X.Value;
                            diffY = base_ccord.GetY() - TouchSensorParam.SensorBasePos.Value.Y.Value;
                            diffZ = base_ccord.GetZ() - TouchSensorParam.SensorBasePos.Value.Z.Value;

                            TouchSensorParam.SensorBasePos.Value.X.Value += diffX; // diff값 반영.
                            TouchSensorParam.SensorBasePos.Value.Y.Value += diffY; // diff값 반영.
                            TouchSensorParam.SensorBasePos.Value.Z.Value += diffZ; // diff값 반영.

                            LoggerManager.Debug($"[TouchSensorBaseSetupModule] TouchSensorBaseSetupSystemInit() : over tolerance diffX = {diffX} , diffY = {diffY}, diffZ = {diffZ}");

                            double touchsensordifflimit = 100; //touch sensor offset diff 160um
                            if (diffZ > touchsensordifflimit)
                            {
                                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Move_Position_Failure, retVal);
                                LoggerManager.Debug($"[TouchSensorBaseSetupModule] Touch Sensor Base Pos Tolerance Error :  DiffZ  = {diffZ}um, Tolerance = {touchsensordifflimit}um");
                                retVal = EventCodeEnum.Touch_Sensor_Out_Of_Tolereance;
                                return retVal;
                            }

                            retVal = EventCodeEnum.NONE;
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Base_Position_OK, retVal);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"{retVal.ToString()}. TouchSensorBaseSetup - SetSensorBasePosCommand() : PinHighViewMove Error.");
                    }
                }
                else
                {
                    LoggerManager.Debug("[TouchSensorBaseSetupModule] TouchSensorBaseSetupSystemInit(), TouchSensorHighViewMove Error");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Move_Position_Failure, retVal);
                    return retVal;
                }

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Base_Position_Failure, retVal);
                LoggerManager.Exception(err);
            }
            finally
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();
                ParamChanged = true;
                SaveSysParameter();
                SetStepSetupState();
            }

            return retVal;
        }
    }
}
