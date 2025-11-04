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

namespace TouchSensorPadRefRegisterModule
{
    public class TouchSensorPadRefSetup : PNPSetupBase, IHasSysParameterizable, ITemplateModule, INotifyPropertyChanged, ISetup, IParamNode, ITouchSensorPadRefSetupModule
    {
        public override Guid ScreenGUID { get; } = new Guid("D1205560-3025-DD6C-C29C-A5C158F8BF80");

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

        private IStateModule _TouchSensorPadRefModule;
        public IStateModule TouchSensorPadRefModule
        {
            get { return _TouchSensorPadRefModule; }
            set
            {
                if (value != _TouchSensorPadRefModule)
                {
                    _TouchSensorPadRefModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TouchSensorPadRefSetup()
        {

        }
        public TouchSensorPadRefSetup(IStateModule Module)
        {
            TouchSensorPadRefModule = Module;
        }

        private IFocusing _TouchSensorPadRefFocusModel;

        public IFocusing TouchSensorPadRefFocusModel
        {
            get
            {
                if (_TouchSensorPadRefFocusModel == null)
                    _TouchSensorPadRefFocusModel = this.FocusManager().GetFocusingModel((this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusingModuleDllInfo);

                return _TouchSensorPadRefFocusModel;
            }
            set { _TouchSensorPadRefFocusModel = value; }
        }
        private IFocusParameter TouchSensorPadRefFocusParam => (this.StageSupervisor().TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter).FocusParamForPadBase;


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
                //NeedleCleanSysParam.CopyToSensoPadRefReg((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam);
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
                if (TouchSensorParam.TouchSensorPadBaseRegistered.Value == true)
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
                    if (TouchSensorPadRefFocusParam == null)
                    {
                        TouchSensorPadRefFocusParam.SetDefaultParam();
                        TouchSensorPadRefFocusParam.FocusRange.Value = 500;
                        TouchSensorPadRefFocusParam.OutFocusLimit.Value = 20;
                        TouchSensorPadRefFocusParam.FocusingROI.Value = new Rect(0, 0, 960, 960);
                        TouchSensorPadRefFocusParam.CheckDualPeak.Value = true;
                        TouchSensorPadRefFocusParam.CheckFlatness.Value = true;
                        TouchSensorPadRefFocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                        TouchSensorPadRefFocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                        TouchSensorPadRefFocusParam.FocusMaxStep.Value = 50;
                        TouchSensorPadRefFocusParam.DepthOfField.Value = 1;
                        TouchSensorPadRefFocusParam.FlatnessThreshold.Value = 60;
                    }
                    FocusingCommandCanExecute = true;
                    StageSupervisor = this.StageSupervisor();

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
                Header = "Register Sensor Check Position";

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

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                MainViewTarget = DisplayPort;



                // 파라미터 업데이트
                retVal = LoadSysParameter();

                // 카메라 변경


                // 조명 설정
                foreach (var light in TouchSensorParam.LightForPadBaseFocusSensor)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                //TO DO: 센서 REF 위치로 이동하기
                StageSupervisor.StageModuleState.ZCLEARED();

                retVal = StageSupervisor.StageModuleState.WaferHighCamCoordMoveNCpad(TouchSensorParam.SensingPadBasePos.Value, 0);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("[TouchSensorPadRefSetupModule] InitSetup(), WaferHighCamCoordMoveNCpad Error");
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
                LoggerManager.Debug($"{err.ToString()}. TouchSensorPadRefSetup - InitPnpUI() : Error occured.");
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

                if (TouchSensorPadRefFocusParam.FocusingCam.Value != EnumProberCam.WAFER_HIGH_CAM)
                    TouchSensorPadRefFocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;

                if (TouchSensorPadRefFocusParam.FocusingAxis.Value == EnumAxisConstants.Undefined)
                    TouchSensorPadRefFocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;

                if (TouchSensorPadRefFocusParam.FocusingROI.Value.Width == 0 || TouchSensorPadRefFocusParam.FocusingROI.Value.Height == 0)
                {
                    Rect tmpRect = new Rect(0, 0, 960, 960);
                    TouchSensorPadRefFocusParam.FocusingROI.Value = tmpRect;
                }

                if (TouchSensorPadRefFocusParam.FocusRange.Value <= 0)
                    TouchSensorPadRefFocusParam.FocusRange.Value = 600;

                RetVal = TouchSensorPadRefFocusModel.Focusing_Retry(TouchSensorPadRefFocusParam, false, false, false, this);

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
                    
                    SetSensorPadRefCommand();
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

        private void SetSensorPadRefCommand()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //TO DO: 화면 락 시키기

            try
            {
                TouchSensorParam.TouchSensorPadBaseRegistered.Value = false;

                // Wafer high로 전환
                if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.WAFER_LOW_CAM)
                {
                    NCCoordinate hpcoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
                    RetVal = StageSupervisor.StageModuleState.WaferHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
                }
                else if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.WAFER_HIGH_CAM)
                {
                    RetVal = EventCodeEnum.NONE;
                }
                else
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Debug("Unexpected camera type");
                    LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_Failure, RetVal);

                    return;
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    if (NC.NCSysParam != null)
                    {
                        if (NC.NCSysParam.CleanUnitAttached.Value == true)
                        {
                            if (NC.NCSysParam.NC_TYPE.Value.Equals(NC_MachineType.AIR_NC))
                            {
                                if (this.NeedleCleaner().IsCleanPadUP() == false)
                                {
                                    RetVal = this.NeedleCleaner().CleanPadUP(false);
                                    if (RetVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"Failed to up clean pad");
                                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_Failure, RetVal);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM, this);

                    // 포커싱 하기
                    RetVal = FocusingFunc();

                    //this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM);

                    if (RetVal != EventCodeEnum.NONE)
                    {
                        TouchSensorParam.TouchSensorPadBaseRegistered.Value = false;
                        SetStepSetupState();

                        LoggerManager.Debug("Focusing failed for touch sensor");
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_Failure, RetVal);
                        return;
                    }

                    NCCoordinate cur_ccord = new NCCoordinate();

                    cur_ccord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();

                    // 위치 읽기

                    TouchSensorParam.SensingPadBasePos.Value.X.Value = cur_ccord.GetX();
                    TouchSensorParam.SensingPadBasePos.Value.Y.Value = cur_ccord.GetY();
                    TouchSensorParam.SensingPadBasePos.Value.Z.Value = cur_ccord.GetZ();

                    LoggerManager.Debug("Focused sensor pad base position = (" + TouchSensorParam.SensingPadBasePos.Value.X.Value + ", " +
                        TouchSensorParam.SensingPadBasePos.Value.Y.Value + ", " + TouchSensorParam.SensingPadBasePos.Value.Z.Value + ")");
                    //}

                    //TO DO: 화면 언락하기


                    // 조명 저장하기
                    TouchSensorParam.LightForPadBaseFocusSensor.Clear();
                    foreach (var light in CurCam.LightsChannels)
                    {
                        int val = CurCam.GetLight(light.Type.Value);
                        TouchSensorParam.LightForPadBaseFocusSensor.Add(new LightValueParam(light.Type.Value, (ushort)val));
                    }

                    TouchSensorParam.TouchSensorPadBaseRegistered.Value = true;
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_OK, RetVal);
                }
                else
                {
                    LoggerManager.Debug($"{RetVal.ToString()}. TouchSensorPadRefSetup - SetSensorPadRefCommand() : Move Error.");
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. TouchSensorPadRefSetup - SetSensorPadRefCommand() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_Failure, RetVal);

            }
            finally
            {
                ParamChanged = true;
                SaveSysParameter();
                SetStepSetupState();
            }
        }

        private StageCoords _StageCoord;
        public StageCoords StageCoord
        {
            get { return _StageCoord; }
            set
            {
                if (value != _StageCoord)
                {
                    _StageCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum TouchSensorPadRefSetupSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //padref_coord = new NCCoordinate();

            try
            {
                StageSupervisor.StageModuleState.ZCLEARED();

                // 파라미터 업데이트
                retVal = LoadSysParameter();
                // 카메라 변경
                //TO DO: 센서 REF 위치로 이동하기

                if(retVal == EventCodeEnum.NONE)
                {
                    retVal = StageSupervisor.StageModuleState.WaferHighCamCoordMoveNCpad(TouchSensorParam.SensingPadBasePos.Value, 0);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        StageSupervisor.StageModuleState.ZCLEARED();
                        LoggerManager.Debug($"[TouchSensorPadRefSetup] - TouchSensorPadRefSetupSystemInit() : {retVal} Stage Move Error");
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_Failure, retVal);
                        return retVal;
                    }

                    CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                    // 조명 설정
                    foreach (var light in TouchSensorParam.LightForPadBaseFocusSensor)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    ParamValidation();

                    // Wafer high로 전환
                    if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.WAFER_LOW_CAM)
                    {
                        NCCoordinate hpcoord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
                        retVal = StageSupervisor.StageModuleState.WaferHighViewMove(hpcoord.GetX(), hpcoord.GetY(), hpcoord.GetZ());
                    }
                    else if (this.VisionManager().DigitizerService[CurCam.Param.DigiNumber.Value].CurCamera.CameraChannel.Type == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.INVALID_CAMERA_CHANNEL;
                        LoggerManager.Debug("[TouchSensorPadRefSetupModule] - TouchSensorPadRefSetupSystemInit() : Unexpected camera type");
                        LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, retVal);
                        return retVal;
                    }

                    if(retVal == EventCodeEnum.NONE)
                    {
                        this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM, this);

                        // 포커싱 하기
                        retVal = FocusingFunc();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug("[TouchSensorPadRefSetupModule] - TouchSensorPadRefSetupSystemInit() : Focusing failed for touch sensor");
                            LoggerManager.Prolog(PrologType.OPERRATION_ALARM, EventCodeEnum.Touch_Sensor_Focusing_Failure, retVal);
                            return retVal;
                        }

                        NCCoordinate padref_coord = this.CoordinateManager().WaferHighNCPadConvert.CurrentPosConvert();
                        double trialposOffsetX = padref_coord.GetX() - TouchSensorParam.SensingPadBasePos.Value.X.Value;
                        double trialposOffsetY = padref_coord.GetY() - TouchSensorParam.SensingPadBasePos.Value.Y.Value;
                        double trialposOffsetZ = padref_coord.GetZ() - TouchSensorParam.SensingPadBasePos.Value.Z.Value;

                        LoggerManager.Debug("[TouchSensorPadRefSetupModule] - TouchSensorPadRefSetupSystemInit() : Focused sensor pad base position offset = (" + trialposOffsetX + ", " +
                            trialposOffsetY + ", " + trialposOffsetZ + ")");
                        //}
                        retVal = EventCodeEnum.NONE;
                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Pad_Ref_Position_OK, retVal);
                    }
                }
                else
                {
                    LoggerManager.Debug("[TouchSensorPadRefSetupModule] TouchSensorPadRefSetupSystemInit(), WaferHighCamCoordMoveNCpad Error");
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
