using LogModule;
using NeedleCleanerModuleParameter;
using Newtonsoft.Json;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SerializerUtil;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using UCNeedleClean;

namespace NeedleCleanPadModule
{
    public class CleanPadSetup : PNPSetupBase, IHasSysParameterizable, ITemplateModule, INotifyPropertyChanged, ISetup, IParamNode, IPackagable
    {
        public override Guid ScreenGUID { get; } = new Guid("8F2CD01F-2548-C143-95DD-1A2713570B3B");

        public override bool Initialized { get; set; } = false;

        private IStateModule _CleanPadModule;
        public IStateModule CleanPadModule
        {
            get { return _CleanPadModule; }
            set
            {
                if (value != _CleanPadModule)
                {
                    _CleanPadModule = value;
                    RaisePropertyChanged();
                }
            }
        }



        public IParam DevParam { get; set; }
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
        public SubModuleStateBase SubModuleState { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        public CleanPadSetup()
        {

        }
        public CleanPadSetup(IStateModule Module)
        {
            _CleanPadModule = Module;
        }

        private NeedleCleanDeviceParameter _NeedleCleanDevParam;
        public NeedleCleanDeviceParameter NeedleCleanDevParam
        {
            get { return _NeedleCleanDevParam; }
            set
            {
                if (value != _NeedleCleanDevParam)
                {
                    _NeedleCleanDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanSystemParameter _NeedleCleanSysParam;
        public NeedleCleanSystemParameter NeedleCleanSysParam
        {
            get { return _NeedleCleanSysParam; }
            set
            {
                if (value != _NeedleCleanSysParam)
                {
                    _NeedleCleanSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private UcNeedleCleanMainPage _MainPageView;
        public UcNeedleCleanMainPage MainPageView
        {
            get { return _MainPageView; }
            set
            {
                if (value != _MainPageView)
                {
                    _MainPageView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Point _LastPoint;
        public Point LastPoint
        {
            get { return _LastPoint; }
            set
            {
                if (value != _LastPoint)
                {
                    _LastPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NeedleCleanPadWidth;
        public double NeedleCleanPadWidth
        {
            get { return _NeedleCleanPadWidth; }
            set
            {
                if (value != _NeedleCleanPadWidth)
                {
                    _NeedleCleanPadWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NeedleCleanPadHeight;
        public double NeedleCleanPadHeight
        {
            get { return _NeedleCleanPadHeight; }
            set
            {
                if (value != _NeedleCleanPadHeight)
                {
                    _NeedleCleanPadHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanObject _NC;
        public new NeedleCleanObject NC { 
            get { return _NC; }
            set
            {
                if (value != _NC)
                {
                    _NC = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool DragInProgress = false;
        private bool ParamChanged = false;
        private IWaferObject Wafer { get { return this.StageSupervisor().WaferObject; } }
        private new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }
        private double _RatioX;
        private double _RatioY;
        private float _WinSizeX;
        private float _WinSizeY;


        public EventCodeEnum DoExecute() //실제 프로세싱 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MovingState.Moving();

            /*
                실제 프로세싱 코드 작성
             
             
             */
            MovingState.Stop();
            return retVal;
        }
        public EventCodeEnum DoClearData() //현재 Parameter Check 및 Init하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
            //  return ParamValidation();
        }
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Parameter 확인한다.

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Update 상태가 있는지 없는지를 확인해준다.
                ///retVal = Extensions_IParam.ElementStateUpdateValidation(Param);

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Apply 상태가 있는지 없는지를 확인해준다.
                ///retVal = Extensions_IParam.ElementStateApplyValidation(Param);

                //모듈의  Setup상태를 변경해준다.

                if (retVal == EventCodeEnum.NONE)
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
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
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

        #region Don`t Touch Code
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }
        public EventCodeEnum ExitRecovery()
        {

            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        #endregion
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    NeedleCleanSystemParameter NCsysParam = (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam;

                    if(NC == null)
                    {
                        NC = (NeedleCleanObject)this.StageSupervisor().NCObject;
                    }
                    //init 하는 코드 하고 나서 Idle 상태로 초기화
                    //SubModuleState = new SubModuleIdleState(this);
                    //MovingState = new SubModuleStopState(this);

                    //NC = (NeedleCleanObject)this.StageSupervisor().NCObject;

                    // Need to default Width & Height parameter ( ex, 200 / 100 )
                    //((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.SysParam).NeedleCleanPadWidth.Value = 200000;
                    //((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.SysParam).NeedleCleanPadHeight.Value = 100000;

                    if (NCsysParam.NeedleCleanPadWidth.Value == 200000 &&
                        NCsysParam.NeedleCleanPadHeight.Value == 100000)
                    {
                        _WinSizeX = 800;
                        _WinSizeY = 400;
                    }
                    else if (NCsysParam.NeedleCleanPadWidth.Value == 160000 &&
                        NCsysParam.NeedleCleanPadHeight.Value == 160000)
                    {
                        _WinSizeX = 640;
                        _WinSizeY = 640;
                    }
                    else
                    {
                        _WinSizeX = 800;
                        _WinSizeY = 640;
                    }

                    _RatioX = _WinSizeX / NCsysParam.NeedleCleanPadWidth.Value;
                    _RatioY = _WinSizeY / NCsysParam.NeedleCleanPadHeight.Value;

                    NeedleCleanPadWidth = NCsysParam.NeedleCleanPadWidth.Value * _RatioX;
                    NeedleCleanPadHeight = NCsysParam.NeedleCleanPadHeight.Value * _RatioY;

                    for (int i = 0; i < NCsysParam.MaxCleanPadNum.Value; i++)
                    {
                        NCSheetVMDefinition ncSheet = new NCSheetVMDefinition();

                        ncSheet.SheetWidth = (NCsysParam.SheetDefs[i].Range.Value.X.Value * 2 * _RatioX);
                        ncSheet.SheetHeight = (NCsysParam.SheetDefs[i].Range.Value.Y.Value * 2 * _RatioY);
                        ncSheet.SheetLeft = (NCsysParam.SheetDefs[i].Range.Value.X.Value * _RatioX) - (ncSheet.SheetWidth / 2);
                        ncSheet.SheetTop = (NCsysParam.SheetDefs[i].Range.Value.Y.Value * _RatioY) - (ncSheet.SheetHeight / 2);
                        ncSheet.ResultVisibility = Visibility.Collapsed;

                        NC.NCSheetVMDef.Index = i;

                        if (i == 0 && NC.NCSheetVMDefs.Count > 0)
                        {
                            NC.NCSheetVMDefs.Clear();
                        }
                        NC.NCSheetVMDefs.Insert(i, ncSheet);
                    }

                    this.NC.NCSheetVMDef = NC.NCSheetVMDefs[0];
                    this.NC.NCSheetVMDef.Thickness = 3;
                    this.NC.NCSheetVMDef.SelectIndex = 1;
                    this.NC.NCSheetVMDef.ResultVisibility = Visibility.Visible;

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

        public new void DeInitModule()
        {
        }
        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(NeedleCleanObject));
                    if (target != null)
                    {
                        //NeedleCleanDevParam = (NeedleCleanDeviceParameter)target;
                        //DevParam = (NeedleCleanDeviceParameter)target;
                        NC = target as NeedleCleanObject;
                        NC.NCSheetVMDef = NC.NCSheetVMDefs[NC.NCSheetVMDef.Index];
                        break;
                    }
                }


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
                tmpParam = new NeedleCleanDeviceParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(NeedleCleanDeviceParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    NeedleCleanDevParam = tmpParam as NeedleCleanDeviceParameter;
                }

                DevParam = NeedleCleanDevParam;
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
                retVal = this.SaveParameter(NeedleCleanDevParam);
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
                //  this.StageSupervisor().NCObject.LoadSysParameter();
                //if(NeedleCleanSysParam == null)
                //    NeedleCleanSysParam = new NeedleCleanSystemParameter();
                //.CopyTo(NeedleCleanSysParam);

                //var ncModule = (IHasDevParameterizable)this.NeedleCleaner();
                //NeedleCleanSysParam = this.StageSupervisor().NCObject.NCSysParam_IParam.Copy() as NeedleCleanSystemParameter;

                this.StageSupervisor().NCObject.LoadSysParameter();
                
               // RetVal = this.StageSupervisor().LoadNCSysObject();
                NeedleCleanSysParam = this.StageSupervisor().NCObject.NCSysParam_IParam as NeedleCleanSystemParameter;
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //var ncModule = (IHasSysParameterizable)this.NeedleCleaner();

                //ncModule.SysParam = (NeedleCleanSystemParameter)ObjectExtensions.DeepClone(NC.NCSysParam);
                //retVal = Extensions_IParam.SaveParameter(NC.NCSysParam);


                //var ncModule = (IHasSysParameterizable)this.NeedleCleaner();

                // TODO: Check Parameter Save & Load
                //ncModule.SysParam = NC.NCSysParam as NeedleCleanSystemParameter;

                //this.NeedleCleaner().NeedleCleanSysParam_IParam = NC.NCSysParam as NeedleCleanSystemParameter;
                this.StageSupervisor().NCObject = NC;
                this.StageSupervisor().NCObject.SaveSysParameter();


                //retVal = this.SaveParameter(NC.NCSysParam);

                //var retECE = ncModule.LoadSysParameter();

                //NeedleCleanSysParam.CopyTo((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.SysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Setup CleanPad";

                retVal = InitPnpModuleStage_AdvenceSetting();
                InitLightJog(this, EnumProberCam.WAFER_HIGH_CAM);

                MainPageView = new UcNeedleCleanMainPage();
                //AdvanceSetupView = new UcNeedleCleanPad();
                AdvanceSetupView = new CleanPadAdvanceSetup.View.CleanPadAdvanceSetupView();
                AdvanceSetupViewModel = new CleanPadAdvanceSetup.ViewModel.CleanPadAdvanceSetupViewModel();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
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
                NC = (NeedleCleanObject)this.StageSupervisor().NCObject;
                retVal = await InitSetup();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = InitPnpUI();

                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                // MainView 화면에 Camera(Vision) 화면이 나온다.
                MainViewTarget = MainPageView;
                // MiniView 화면에 WaferMap 화면이 나온다.
                //MiniViewTarget = Wafer;
                // MiniView 화면에 Dut 화면이 나온다.
                //MiniViewTarget = ProbeCard;

                MiniViewTargetVisibility = Visibility.Hidden;

                MiniViewSwapVisibility = Visibility.Hidden;
                LightJogVisibility = Visibility.Hidden;
                MotionJogVisibility = Visibility.Hidden;
                MainViewZoomVisibility = Visibility.Hidden;

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                //FiveButton.Command = new AsyncCommand(ShowAdvanceSetupView);


                //for (int i = 0; i < NC.NCSysParam.MaxCleanPadNum.Value; i++)
                //{
                //    NCSheetVMDefinition ncSheet = new NCSheetVMDefinition();

                //    ncSheet.SheetWidth = (NC.NCSysParam.SheetDefs[i].Range.Value.X.Value * 2 * _RatioX);
                //    ncSheet.SheetHeight = (NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value * 2 * _RatioY);
                //    ncSheet.SheetLeft = (NC.NCSysParam.SheetDefs[i].Range.Value.X.Value * _RatioX) - (ncSheet.SheetWidth / 2);
                //    ncSheet.SheetTop = (NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value * _RatioY) - (ncSheet.SheetHeight / 2);
                //    ncSheet.ResultVisibility = Visibility.Collapsed;

                //    NC.NCSheetVMDef.Index = i;

                //    //NC.NCSheetVMDefs.Remove(ncSheet);
                //    NC.NCSheetVMDefs.RemoveAt(i);
                //    NC.NCSheetVMDefs.Insert(i, ncSheet);
                //}

                float width;
                float height;
                float padsizeratio;

                width = (float)NC.NCSysParam.NeedleCleanPadWidth.Value;
                height = (float)NC.NCSysParam.NeedleCleanPadHeight.Value;
                padsizeratio = (width / height);

                // 화면에 표시되는 영역은 가로 800, 세로 400이다. 따라서 가로 세로의 비율은 2:1며 표시하고 싶은 패드 사이즈의 비율에 따라 더 
                // 긴쪽을 기준으로 디스플레이 될 기준을 잡는다.
                if (padsizeratio > 2)
                {
                    // 가로의 길이가 세로의 길이보다 2배 이상 긴 경우. 가로를 먼저 맞춘다
                    _WinSizeX = 800;
                    _WinSizeY = 800 / padsizeratio;
                }
                else
                {
                    // 가로의 길이가 세로의 길이보다 2배 미만인 경우 혹은 같거나 세로가 더 긴 경우. 세로를 먼저 맞춘다.
                    _WinSizeY = 400;
                    _WinSizeX = 400 * padsizeratio;
                }

                _RatioX = _WinSizeX / NC.NCSysParam.NeedleCleanPadWidth.Value;
                _RatioY = _WinSizeY / NC.NCSysParam.NeedleCleanPadHeight.Value;

                //NeedleCleanPadWidth = NCsysParam.NeedleCleanPadWidth.Value * _RatioX;
                //NeedleCleanPadHeight = NCsysParam.NeedleCleanPadHeight.Value * _RatioY;



                for (int i = 0; i < NC.NCSysParam.MaxCleanPadNum.Value; i++)
                {
                    NC.NCSheetVMDefs[i].SheetLeft = (_WinSizeX/ 2) + ((NC.NCSysParam.SheetDefs[i].Offset.Value.X.Value - NC.NCSysParam.SheetDefs[i].Range.Value.X.Value) * _RatioX);
                    NC.NCSheetVMDefs[i].SheetTop = (_WinSizeY/ 2) - ((NC.NCSysParam.SheetDefs[i].Offset.Value.Y.Value + NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value) * _RatioY);
                    NC.NCSheetVMDefs[i].SheetWidth = (NC.NCSysParam.SheetDefs[i].Range.Value.X.Value * 2 * _RatioX);
                    NC.NCSheetVMDefs[i].SheetHeight = (NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value * 2 * _RatioY);

                    //NC.NCSheetVMDefs[i].SheetLeft = (NC.NCSysParam.SheetDefs[i].Range.Value.X.Value * _RatioX) - (NC.NCSheetVMDefs[i].SheetWidth / 2);
                    //NC.NCSheetVMDefs[i].SheetTop = (NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value * _RatioY) - (NC.NCSheetVMDefs[i].SheetHeight / 2);

                    //SheetDefs[i].SheetWidth = NC.NCSheetVMDefs[i].SheetWidth;
                    //SheetDefs[i].SheetHeight = NC.NCSheetVMDefs[i].SheetHeight;
                    //SheetDefs[i].SheetLeft = NC.NCSheetVMDefs[i].SheetLeft;
                    //SheetDefs[i].SheetTop = NC.NCSheetVMDefs[i].SheetTop;
                }

                NC.InitCleanPadRender();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        /// <summary>
        /// Recovery시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InitPnpUI ();
            }
            catch (Exception err)
            {
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

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Focusing.png");
                OneButton.Command = new AsyncCommand(FocusingCommand);
                OneButton.IconCaption = "FOCUS";

                //TwoButton.IconSource = null;
                //TwoButton.Command = null;

                ThreeButton.IconSource = null;
                ThreeButton.Command = null;

                FourButton.Caption = null;
                FourButton.Command = null;

                PadJogLeftUp.Caption = "PREV";
                PadJogLeftUp.Command = new RelayCommand(PadPrevCommand);

                PadJogRightUp.Caption = "NEXT";
                PadJogRightUp.Command = new RelayCommand(PadNextCommand);

                PadJogLeftDown.Caption = null;
                PadJogLeftDown.Command = null;

                PadJogRightDown.Caption = null;
                PadJogRightDown.Command = null;

                PadJogSelect.Caption = null;
                PadJogSelect.Command = null;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");

                //PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                //PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                //PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                //PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                PadJogLeft.Command = new RelayCommand(SizeLeftCommandFunc);
                PadJogRight.Command = new RelayCommand(SizeRightCommandFunc);
                PadJogUp.Command = new RelayCommand(SizeUpCommandFunc);
                PadJogDown.Command = new RelayCommand(SizeDownCommandFunc);

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                EnableUseBtn();

                retVal = EventCodeEnum.NONE;
                //ChangeWidthValue , ChangeHeightValue 값 변경 으로 Rect 사이즈 얼마씩 조절할기 설정할수 있다.
                //Ex ) ChangeWidthValue = 8;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = IsParamChanged || ParamChanged;
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
                if (this.NC.NCSheetVMDefs.Count() >= 1)
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

        private void SaveParameter()
        {
            try
            {
                //NC = this.StageSupervisor().NCObject as NeedleCleanObject;

                for (int i = 0; i < NC.NCSysParam.MaxCleanPadNum.Value; i++)
                {
                    double sheetcenterX = NC.NCSheetVMDefs[i].SheetLeft + (NC.NCSheetVMDefs[i].SheetWidth / 2);
                    double sheetcenterY = NC.NCSheetVMDefs[i].SheetTop + (NC.NCSheetVMDefs[i].SheetHeight / 2);

                    NC.NCSysParam.SheetDefs[i].Range.Value.X.Value = NC.NCSheetVMDefs[i].SheetWidth / 2 / NC.RatioX;
                    NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value = NC.NCSheetVMDefs[i].SheetHeight / 2 / NC.RatioY;

                    NC.NCSysParam.SheetDefs[i].Offset.Value.X.Value = (sheetcenterX - (NC.WinSizeX / 2)) / NC.RatioX;
                    NC.NCSysParam.SheetDefs[i].Offset.Value.Y.Value = ((NC.WinSizeY / 2) - sheetcenterY) / NC.RatioY;

                    //NC.NCSysParam.SheetDefs[i].Offset.Value.Y.Value = (sheetcenterY - (NC.NCSheetVMDefs[i].SheetHeight / 2)) / _RatioY;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "SaveParameter() : Error occurred.");
            }
        }

        #region //..Command Method        

        //private void PadAddCommand()
        //{
        //    try
        //    {
        //        if (SheetDefs.Count() > 2)
        //            return;

        //        NCSheetDefinition ncSheet = new NCSheetDefinition();

        //        ncSheet.SheetWidth = (SheetDef.Range.Value.X.Value * _RatioX);
        //        ncSheet.SheetHeight = (SheetDef.Range.Value.Y.Value * _RatioY);
        //        ncSheet.SheetLeft = (SheetDef.Range.Value.X.Value / 2 * _RatioX);
        //        ncSheet.SheetTop = (SheetDef.Range.Value.Y.Value / 2 * _RatioY);

        //        ncSheet.Index = SheetDefs.Count() + 1;

        //        this.SheetDef = ncSheet;
        //        SheetDefs.Add(SheetDef);

        //        for (int i = 0; i < SheetDefs.Count(); i++)
        //        {
        //            if (this._SheetDef == SheetDefs[i])
        //            {
        //                SheetDefs[i].Thickness = 3;                        
        //            }
        //            else
        //            {
        //                SheetDefs[i].Thickness = 0;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.DebugError(err.ToString() + "PadAddCommand() : Error occurred.");                
        //    }            
        //}

        //private void PadDeleteCommand()
        //{
        //    try
        //    {
        //        SheetDefs.Remove(this.SheetDef);
        //        //SheetDef.Index = SheetDefs.Count() - 1;

        //        for (int i = 0; i < SheetDefs.Count(); i++)
        //        {
        //            if (this._SheetDef == SheetDefs[i])
        //            {
        //                SheetDefs[i].Thickness = 3;                       
        //            }
        //            else
        //            {
        //                SheetDefs[i].Thickness = 0;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.DebugError(err.ToString() + "PadDeleteCommand() : Error occurred.");                
        //    }            
        //}

        private void PadPrevCommand()
        {
            try
            {
                int index = this.NC.NCSheetVMDef.Index;
                index--;
                index = index < 0 ? NC.NCSheetVMDefs.Count() - 1 : index;

                for (int i = 0; i < NC.NCSheetVMDefs.Count(); i++)
                {
                    if (index == i)
                    {
                        this.NC.NCSheetVMDef = NC.NCSheetVMDefs[index];
                        NC.NCSheetVMDefs[i].Thickness = 3;
                        NC.NCSheetVMDef.Index = index;
                        NC.NCSheetVMDefs[i].SelectIndex = 1;
                        NC.NCSheetVMDefs[i].ResultVisibility = Visibility.Visible;
                    }
                    else
                    {
                        NC.NCSheetVMDefs[i].Thickness = 0;
                        NC.NCSheetVMDefs[i].SelectIndex = 0;
                        NC.NCSheetVMDefs[i].ResultVisibility = Visibility.Collapsed;
                    }
                }
                NC.InitCleanPadRender();
                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadPrevCommand() : Error occurred.");
            }

        }

        private void PadNextCommand()
        {
            try
            {
                int index = this.NC.NCSheetVMDef.Index;
                index++;

                index = NC.NCSheetVMDefs.Count() <= index ? 0 : index;

                for (int i = 0; i < NC.NCSheetVMDefs.Count; i++)
                {
                    if (index == i)
                    {
                        this.NC.NCSheetVMDef = NC.NCSheetVMDefs[index];
                        NC.NCSheetVMDefs[i].Thickness = 3;
                        NC.NCSheetVMDef.Index = index;
                        NC.NCSheetVMDefs[i].SelectIndex = 1;
                        NC.NCSheetVMDefs[i].ResultVisibility = Visibility.Visible;
                    }
                    else
                    {
                        NC.NCSheetVMDefs[i].Thickness = 0;
                        NC.NCSheetVMDefs[i].SelectIndex = 0;
                        NC.NCSheetVMDefs[i].ResultVisibility = Visibility.Collapsed;
                    }
                }
                NC.InitCleanPadRender();
                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadNextCommand() : Error occurred.");
            }

        }

        private async Task FocusingCommand()
        {
            try
            {
                

                EventCodeEnum retVal = await this.NeedleCleaner().Focusing5pt(NC.NCSheetVMDef.Index);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("Focusing failed for touch sensor");

                    return;
                }

                //foreach (var list in NC.NCSheetVMDefs[NC.NCSheetVMDef.Index].Heights)
                //{
                //    this.NC.NCSheetVMDef.Heights.Add(list);
                //}

                //this.NC.NCSheetVMDef.ResultVisibility = Visibility.Hidden;
                this.NC.NCSheetVMDef.ResultVisibility = Visibility.Visible;

                //NC.NCSheetVMDefs.RemoveAt(this.NC.NCSheetVMDef.Index);
                //NC.NCSheetVMDefs.Add(this.NC.NCSheetVMDef);                        
                //NC.NCSheetVMDefs.Insert(this.NC.NCSheetVMDef.Index, this.NC.NCSheetVMDef);

                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.ToString() + "FocusingCommand() : Error occurred.");
            }

            finally
            {
                
            }
        }

        private void SizeLeftCommandFunc()
        {
            try
            {
                var NCSheetVMDef = NC.NCSheetVMDef;
                if (NCSheetVMDef.SheetWidth <= 40)
                    NCSheetVMDef.SheetWidth = 40;
                else
                {
                    ChangeWidthValue = -Math.Abs(ChangeWidthValue);
                    NCSheetVMDef.SheetWidth += ChangeWidthValue;
                    NCSheetVMDef.SheetLeft -= (ChangeWidthValue / 2);
                }

                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "SizeLeftCommandFunc() : Error occurred.");
            }
        }

        private void SizeRightCommandFunc()
        {
            try
            {
                var NCSheetVMDef = NC.NCSheetVMDef;

                if (NCSheetVMDef.SheetWidth >= NeedleCleanPadWidth)
                    NCSheetVMDef.SheetWidth = NeedleCleanPadWidth;

                if (NCSheetVMDef.SheetLeft > 0 && (NCSheetVMDef.SheetLeft + NCSheetVMDef.SheetWidth) < NeedleCleanPadWidth)
                {
                    ChangeWidthValue = Math.Abs(ChangeWidthValue);
                    NCSheetVMDef.SheetWidth += ChangeWidthValue;
                    NCSheetVMDef.SheetLeft -= (ChangeWidthValue / 2);
                }
                else if (NCSheetVMDef.SheetLeft <= 0)
                {
                    ChangeWidthValue = Math.Abs(ChangeWidthValue);
                    NCSheetVMDef.SheetWidth += ChangeWidthValue;
                }
                else if ((NCSheetVMDef.SheetLeft + NCSheetVMDef.SheetWidth) >= NeedleCleanPadWidth)
                {
                    ChangeWidthValue = Math.Abs(ChangeWidthValue);
                    NCSheetVMDef.SheetWidth += (ChangeWidthValue / 2);
                    NCSheetVMDef.SheetLeft -= (ChangeWidthValue / 2);
                }
                
                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "SizeRightCommandFunc() : Error occurred.");
            }
        }

        private void SizeUpCommandFunc()
        {
            try
            {
                var NCSheetVMDef = NC.NCSheetVMDef;
                if (NCSheetVMDef.SheetHeight >= NeedleCleanPadHeight)
                    NCSheetVMDef.SheetHeight = NeedleCleanPadHeight;

                if (NCSheetVMDef.SheetTop > 0 && (NCSheetVMDef.SheetTop + NCSheetVMDef.SheetHeight) < NeedleCleanPadHeight)
                {
                    ChangeHeightValue = Math.Abs(ChangeHeightValue);
                    NCSheetVMDef.SheetHeight += ChangeHeightValue;
                    NCSheetVMDef.SheetTop -= (ChangeHeightValue / 2);
                }
                else if (NCSheetVMDef.SheetTop <= 0)
                {
                    ChangeHeightValue = Math.Abs(ChangeHeightValue);
                    NCSheetVMDef.SheetHeight += ChangeHeightValue;
                }
                else if ((NCSheetVMDef.SheetTop + NCSheetVMDef.SheetHeight) >= NeedleCleanPadHeight)
                {
                    ChangeHeightValue = Math.Abs(ChangeHeightValue);
                    NCSheetVMDef.SheetHeight += (ChangeHeightValue / 2);
                    NCSheetVMDef.SheetTop -= (ChangeHeightValue / 2);
                }

                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "SizeUpCommandFunc() : Error occurred.");
            }
        }

        private void SizeDownCommandFunc()
        {
            try
            {
                if (NC.NCSheetVMDef.SheetHeight <= 40)
                    NC.NCSheetVMDef.SheetHeight = 40;
                else
                {
                    ChangeHeightValue = -Math.Abs(ChangeHeightValue);
                    NC.NCSheetVMDef.SheetHeight += ChangeHeightValue;
                    NC.NCSheetVMDef.SheetTop -= (ChangeHeightValue / 2);
                }

                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();

            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "SizeDownCommandFunc() : Error occurred.");
            }
        }

        #region ==>MouseDownCommand
        private RelayCommand<object> _MouseDownCommand;
        public ICommand MouseDownCommand
        {
            get
            {
                if (null == _MouseDownCommand) _MouseDownCommand = new RelayCommand<object>(MouseDownCommandFunc);
                return _MouseDownCommand;
            }
        }
        private void MouseDownCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.Canvas rec = (System.Windows.Controls.Canvas)param;
                LastPoint = Mouse.GetPosition(rec);

                // Test code
                //double posX = 0;
                //double posY = 0;
                //NCCoordinate mccoord = new NCCoordinate();
                //posX = (LastPoint.X - 400) * 250;
                //posY = (LastPoint.Y - 200) * -250;

                ////mccoord.Z.Value = this.NeedleCleaner().NCHeightProfilingModule.GetPZErrorComp(posX, posY, -30000);

                //mccoord.Z.Value = this.NeedleCleaner().GetMeasuredNcPadHeight(0, posX, posY);

                //LoggerManager.Debug($"value = {posX}, {posY}, {mccoord.Z.Value}");




                DragInProgress = true;
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.ToString() + "MouseDownCommandFunc() : Error occurred.");
            }

        }

        #endregion

        #region ==>MouseMoveCommand
        private RelayCommand<object> _MouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get
            {
                if (null == _MouseMoveCommand) _MouseMoveCommand = new RelayCommand<object>(MouseMoveCommandFunc);
                return _MouseMoveCommand;
            }
        }
        [PreventLogging]
        private void MouseMoveCommandFunc(object param)
        {
            try
            {
                if (DragInProgress == true)
                {
                    System.Windows.Controls.Canvas rec = (System.Windows.Controls.Canvas)param;
                    CleanPadSetup data = (CleanPadSetup)rec.DataContext;
                    Point point = Mouse.GetPosition(rec);

                    double offset_x = point.X - LastPoint.X;
                    double offset_y = point.Y - LastPoint.Y;

                    double new_x = NC.NCSheetVMDef.SheetLeft;
                    double new_y = NC.NCSheetVMDef.SheetTop;
                    double new_width = NC.NCSheetVMDef.SheetWidth;
                    double new_height = NC.NCSheetVMDef.SheetHeight;

                    if ((new_width > 0) && ((new_x + offset_x + NC.NCSheetVMDef.SheetWidth) < NeedleCleanPadWidth) && (new_x + offset_x > 0))
                    {
                        NC.NCSheetVMDef.SheetLeft = new_x + offset_x;
                        LastPoint = point;
                    }

                    if ((new_height > 0) && ((new_y + offset_y + NC.NCSheetVMDef.SheetHeight) < NeedleCleanPadHeight) && (new_y + offset_y > 0))
                    {
                        NC.NCSheetVMDef.SheetTop = new_y + offset_y;
                        LastPoint = point;
                    }

                    //if ((new_width > -5) && (new_height > -5) && (new_x + offset_x + SheetDef.SheetWidth) < (NeedleCleanPadWidth + 5)
                    //     && (new_y + offset_y + SheetDef.SheetHeight) < (NeedleCleanPadHeight + 5) && new_x + offset_x > -5 && new_y + offset_y > -5)
                    //{
                    //    SheetDef.SheetLeft = new_x + offset_x;
                    //    SheetDef.SheetTop = new_y + offset_y;
                    //    LastPoint = point;
                    //}
                }

                SaveParameter();
                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "MouseMoveCommandFunc() : Error occurred.");
            }
        }
        #endregion

        #region ==>MouseUpCommand
        private RelayCommand<object> _MouseUpCommand;
        public ICommand MouseUpCommand
        {
            get
            {
                if (null == _MouseUpCommand) _MouseUpCommand = new RelayCommand<object>(MouseUpCommandFunc);
                return _MouseUpCommand;
            }
        }

        private void MouseUpCommandFunc(object param)
        {
            try
            {
                DragInProgress = false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.ToString() + "MouseUpCommandFunc() : Error occurred.");
            }

        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(NC));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #endregion
    }
}
