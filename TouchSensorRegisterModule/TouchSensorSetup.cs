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
using RelayCommandBase;
using System.Windows;
using System.Windows.Input;
using ProberInterfaces.Param;
using System.Xml.Serialization;
using Newtonsoft.Json;
using MetroDialogInterfaces;
using TouchSensorSystemParameter;
using ProberInterfaces.TouchSensor;

namespace TouchSensorRegisterModule
{
    public class TouchSensorSetup : PNPSetupBase, IHasSysParameterizable, ITemplateModule, INotifyPropertyChanged, ISetup, IParamNode, ITouchSensorTipSetupModule
    {
        public override Guid ScreenGUID { get; } = new Guid("36972B01-CC9A-6E37-10F4-73228D50521C");

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

        private IStateModule _TouchSensorModule;
        public IStateModule TouchSensorModule
        {
            get { return _TouchSensorModule; }
            set
            {
                if (value != _TouchSensorModule)
                {
                    _TouchSensorModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TouchSensorSetup()
        {

        }
        public TouchSensorSetup(IStateModule Module)
        {
            TouchSensorModule = Module;
        }

        private IFocusing _TouchSensorFocusModel;

        public IFocusing TouchSensorFocusModel
        {
            get
            {
                if (_TouchSensorFocusModel == null)
                    _TouchSensorFocusModel = this.FocusManager().GetFocusingModel((this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusingModuleDllInfo);

                return _TouchSensorFocusModel;
            }
            set { _TouchSensorFocusModel = value; }
        }
        private IFocusParameter TouchSensorFocusParam => (this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusParam;


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

        //public TouchSensorSetup(IStateModule Module)
        //{
        //    _TouchSensorModule = Module;
        //}

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
                //NeedleCleanSysParam.CopyToSensorReg((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam);
                //retVal = this.StageSupervisor.SaveNCSysObject();

                retVal = this.StageSupervisor().TouchSensorObject.SaveSysParameter();
                //retVal = this.SaveParameter(TouchSensorParam);

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
                if (TouchSensorParam.TouchSensorRegistered.Value == true)
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
                    if(TouchSensorFocusParam == null)
                    {
                        TouchSensorFocusParam.SetDefaultParam();

                        TouchSensorFocusParam.FocusRange.Value = 500;
                        TouchSensorFocusParam.OutFocusLimit.Value = 20;
                        TouchSensorFocusParam.FocusingROI.Value = new Rect(0, 0, 960, 960);
                        TouchSensorFocusParam.CheckDualPeak.Value = true;
                        TouchSensorFocusParam.CheckFlatness.Value = true;
                        TouchSensorFocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                        TouchSensorFocusParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;
                        TouchSensorFocusParam.FocusMaxStep.Value = 50;
                        TouchSensorFocusParam.DepthOfField.Value = 1;
                        TouchSensorFocusParam.FlatnessThreshold.Value = 60;
                    }

                    FocusingCommandCanExecute = true;

                    Initialized = true;
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"Unknown Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

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
                Header = "Register Touch Sensor";

                InitPnpModuleStage();

                //MainPageView = new UcNeedleCleanMainPage();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Position_Failure, retVal);
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
                InitPnpModuleStage();
                retVal = await InitSetup();
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                SetStepSetupState();
                // TO DO : 터치 센서 위치로 이동

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Position_Failure, retVal);

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

                //CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                //this.VisionManager().StartGrab(CurCam.GetChannelType());

                // MainView 화면에 Camera(Vision) 화면이 나온다.
                MainViewTarget = DisplayPort;

                // 파라미터 업데이트
                retVal = LoadSysParameter();

                // 카메라 변경

                //// 조명 설정
                //foreach (var light in NeedleCleanSysParam.LightForFocusSensor)
                //{
                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                //}

                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONEEDLECLEANAIRON, true);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONEEDLECLEANAIRON, false);

                //TO DO: 센서 위치로 이동하기
                StageSupervisor.StageModuleState.ZCLEARED();
                retVal = StageSupervisor.StageModuleState.TouchSensorHighViewMove(TouchSensorParam.SensorFocusedPos.Value.X.Value,
                                                                        TouchSensorParam.SensorFocusedPos.Value.Y.Value,
                                                                        TouchSensorParam.SensorFocusedPos.Value.Z.Value);
                if(retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("[TouchSensorTipSetupModule] InitSetup(), TouchSensorHighViewMove Error");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Move_Position_Failure, retVal);
                }

                // 이전에 마크 얼라인 동작이 있으므로 카메라 설정을 뒤에 한다.
                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                // 조명 설정
                foreach (var light in TouchSensorParam.LightForFocusSensor)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                //this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                //EnabelUseBtn();
                InitLightJog(this);
                this.PnPManager().PnpLightJog.IsUseNC = true;
                ParamValidation();
            }
            catch (Exception err)
            {
                //LoggerManager.Debug(err);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Position_Failure, retVal);

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
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Position_Failure, retVal);

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
                LoggerManager.Debug($"{err.ToString()}. TouchSensorSetup - InitPnpUI() : Error occured.");
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

                if (TouchSensorFocusParam.FocusingCam.Value != EnumProberCam.PIN_HIGH_CAM)
                    TouchSensorFocusParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;

                if (TouchSensorFocusParam.FocusingAxis.Value == EnumAxisConstants.Undefined)
                    TouchSensorFocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;

                if (TouchSensorFocusParam.FocusingROI.Value.Width == 0 || TouchSensorFocusParam.FocusingROI.Value.Height == 0)
                {
                    Rect tmpRect = new Rect(0, 0, 960, 960);
                    TouchSensorFocusParam.FocusingROI.Value = tmpRect;
                }
                if (TouchSensorFocusParam.FocusRange.Value <= 0)
                    TouchSensorFocusParam.FocusRange.Value = 500;

                double flatnessbackup = 0.0;
                flatnessbackup = TouchSensorFocusParam.FlatnessThreshold.Value;
                TouchSensorFocusParam.FlatnessThreshold.Value = 90;

                RetVal = TouchSensorFocusModel.Focusing_Retry(TouchSensorFocusParam, false, false, false, this);

                TouchSensorFocusParam.FlatnessThreshold.Value = flatnessbackup; //touchsensor tip flantnessthreshold 90
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
                    
                    SetSensorPosCommand();
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

        private void SetSensorPosCommand()
         {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //TO DO: 화면 락 시키기

            // Test Code
            //string line;
            //string dirPath = @"C:\ProberSystem\PZTest.txt";

            try
            {
                TouchSensorParam.TouchSensorRegistered.Value = false;

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
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, RetVal);
                    LoggerManager.Debug("Unexpected camera type");
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
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, RetVal);
                        return;
                    }

                    // 위치 읽기
                    PinCoordinate cur_ccord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                    // Test Code                
                    //line = $"{cur_ccord.GetZ()}";

                    //using (StreamWriter sw = File.AppendText(dirPath))
                    //{
                    //    sw.WriteLine(line);
                    //}
                    TouchSensorParam.SensorFocusedPos.Value.X.Value = cur_ccord.GetX();
                    TouchSensorParam.SensorFocusedPos.Value.Y.Value = cur_ccord.GetY();
                    TouchSensorParam.SensorFocusedPos.Value.Z.Value = cur_ccord.GetZ();

                    LoggerManager.Debug("Focused sensor position = (" + TouchSensorParam.SensorFocusedPos.Value.X.Value + ", " +
                        TouchSensorParam.SensorFocusedPos.Value.Y.Value + ", " + TouchSensorParam.SensorFocusedPos.Value.Z.Value + ")");


                    //}

                    //TO DO: 화면 언락하기


                    // 조명 저장하기
                    TouchSensorParam.LightForFocusSensor.Clear();
                    foreach (var light in CurCam.LightsChannels)
                    {
                        int val = CurCam.GetLight(light.Type.Value);
                        TouchSensorParam.LightForFocusSensor.Add(new LightValueParam(light.Type.Value, (ushort)val));
                    }

                    TouchSensorParam.TouchSensorRegistered.Value = true;
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Position_OK, RetVal);
                }
                else
                {
                    LoggerManager.Debug($"{RetVal.ToString()}. TouchSensorBaseSetup - SetSensorBasePosCommand() : Move Error.");
                }
            }

            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. TouchSensorSetup - SetSensorPosCommand() : Error occured.");
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Position_Failure, RetVal);
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

        public EventCodeEnum TouchSensorTipSetupSystemInit(double diffX, double diffY,double diffZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();

                retVal = LoadSysParameter();

                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONEEDLECLEANAIRON, true);

                if(retVal == EventCodeEnum.NONE)
                {
                    retVal = this.StageSupervisor().StageModuleState.TouchSensorHighViewMove(TouchSensorParam.SensorFocusedPos.Value.X.Value + diffX,
                                                                            TouchSensorParam.SensorFocusedPos.Value.Y.Value + diffY,
                                                                            TouchSensorParam.SensorFocusedPos.Value.Z.Value + diffZ);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        // 이전에 마크 얼라인 동작이 있으므로 카메라 설정을 뒤에 한다.
                        CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                        // 조명 설정
                        foreach (var light in TouchSensorParam.LightForFocusSensor)
                        {
                            CurCam.SetLight(light.Type.Value, light.Value.Value);
                        }

                        ParamValidation();

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

                            // 포커싱 하기
                            retVal = FocusingFunc();

                            //this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug("[TouchSensorTipSetupModule] - TouchSensorTipSetupSystemInit() : Focusing failed for touch sensor");
                                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, retVal);
                                return retVal;
                            }

                            // 위치 읽기
                            PinCoordinate tip_ccord = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                            double tipOffsetX = tip_ccord.GetX() - TouchSensorParam.SensorFocusedPos.Value.X.Value;
                            double tipOffsetY = tip_ccord.GetY() - TouchSensorParam.SensorFocusedPos.Value.Y.Value;
                            double tipOffsetZ = tip_ccord.GetZ() - TouchSensorParam.SensorFocusedPos.Value.Z.Value;

                            LoggerManager.Debug("[TouchSensorTipSetupModule] - TouchSensorTipSetupSystemInit() : Focused sensor position offset = (" + tipOffsetX + ", " +
                                tipOffsetY + ", " + tipOffsetZ + ")");

                            TouchSensorParam.SensorFocusedPos.Value.X.Value = tip_ccord.GetX();
                            TouchSensorParam.SensorFocusedPos.Value.Y.Value = tip_ccord.GetY();
                            TouchSensorParam.SensorFocusedPos.Value.Z.Value = tip_ccord.GetZ();

                            LoggerManager.Debug("[TouchSensorTipSetupModule] - TouchSensorTipSetupSystemInit() : Update Tip Pos X  = (" + TouchSensorParam.SensorFocusedPos.Value.X.Value + "), Y = (" + TouchSensorParam.SensorFocusedPos.Value.Y.Value + "), Z = (" + TouchSensorParam.SensorFocusedPos.Value.Z.Value + ")");

                            retVal = EventCodeEnum.NONE;
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Position_OK, retVal);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"{retVal.ToString()}. TouchSensorBaseSetup - SetSensorBasePosCommand() : Move Error.");
                    }

                }
                else
                {
                    LoggerManager.Debug("[TouchSensorTipSetupModule] TouchSensorTipSetupSystemInit(), TouchSensorHighViewMove Error");
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
                this.StageSupervisor().StageModuleState.ZCLEARED();
                ParamChanged = true;
                SaveSysParameter();
                SetStepSetupState();
            }

            return retVal;
        }
    }
}
