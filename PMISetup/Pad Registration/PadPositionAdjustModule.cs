using LogModule;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace PMISetup
{
    using WinSize = System.Windows.Size;
    public class PadPositionAdjustModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("FEFE762A-4993-1370-127A-F7817B69A098");

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }
        }

        public PadPositionAdjustModule()
        {

        }

        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }

        }
        public SubModuleStateBase SubModuleState { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        private PMIDrawingGroup _DrawingGroup;
        public PMIDrawingGroup DrawingGroup
        {
            get { return _DrawingGroup; }
            set
            {
                if (value != _DrawingGroup)
                {
                    _DrawingGroup = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public PMIModuleDevParam PMIDevParam;

        //private WaferObject Wafer { get; set; }

        //private ProbeCard ProbeCard { get { return this.StageSupervisor().ProbeCardInfo; } }
        #endregion

        public override bool Initialized { get; set; } = false;

        private bool _OnDelegated;
        public bool OnDelegated
        {
            get { return _OnDelegated; }
            set
            {
                if (value != _OnDelegated)
                {
                    _OnDelegated = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> PNP Module Init
        /// <summary>
        /// PNP 모듈 init 해주는 함수 한번만 호출된다.
        /// </summary>
        /// <returns></returns>
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    InitLightJog(this);

                    //Wafer = this.StageSupervisor().WaferObject as WaferObject;

                    //PMIDevParam = ((PMIModule as IHasDevParameterizable).DevParam) as PMIModuleDevParam;

                    DrawingGroup = new PMIDrawingGroup();
                    DrawingGroup.RegisterdPad = true;

                    SharpDXLayer = this.PMIModule().InitPMIRenderLayer(this.PMIModule().GetLayerSize(), 0, 0, 0, 0);
                    SharpDXLayer?.Init();

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
                LoggerManager.Debug($"[PadPositionAdjustModule] [InitModule()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// PNP 모듈 deinit 해주는 함수
        /// </summary>
        /// <returns></returns>
        public new void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [DeInitModule()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// PNP ViewModel init 해주는 함수
        /// </summary>
        /// <returns></returns>
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Pad Position Adjustment";

                retVal = InitPnpModuleStage();
                SetNodeSetupState(EnumMoudleSetupState.NONE);


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Pad Position Adjustment";

                _PadIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadIndexCommand);
                _PadMoveModeCommand = new AsyncCommand<object>(PadMoveModeCommand);
                _ChangePadPositionCommand = new RelayCommand<object>(FuncChangePadPositionCommand);

                #region ==> 5 Buttons
                OneButton.IconSource = null;
                TwoButton.IconSource = null;
                ThreeButton.IconSource = null;
                FourButton.IconSource = null;

                OneButton.Caption = null;
                TwoButton.Caption = null;
                ThreeButton.Caption = null;
                FourButton.Caption = null;

                OneButton.Command = null;
                TwoButton.Command = null;
                ThreeButton.Command = null;
                FourButton.Command = null;
                FiveButton.Command = null;

                OneButton.IsEnabled = false;
                TwoButton.IsEnabled = false;
                ThreeButton.IsEnabled = false;
                FourButton.IsEnabled = false;
                FiveButton.IsEnabled = false;
                #endregion

                #region ==> Prev/Next Buttons
                PadJogLeftUp.IconCaption = "PAD";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "PAD";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogLeftDown.IconCaption = "ONE PAD";
                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rect-outline.png");
                PadJogRightDown.IconCaption = "ALL PAD";
                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rect-outline-multi.png");

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;
                PadJogLeftDown.CaptionSize = 16;
                PadJogRightDown.CaptionSize = 16;

                PadJogLeftUp.Command = _PadIndexMoveCommand;
                PadJogRightUp.Command = _PadIndexMoveCommand;
                PadJogLeftDown.Command = _PadMoveModeCommand;
                PadJogRightDown.Command = _PadMoveModeCommand;

                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;
                PadJogLeftDown.CommandParameter = SELECTION_MODE.SINGLE;
                PadJogRightDown.CommandParameter = SELECTION_MODE.ALL;
                #endregion

                #region ==> Jog Buttons
                PadJogSelect.Caption = null;

                PadJogSelect.Command = null;
                PadJogLeft.Command = _ChangePadPositionCommand;
                PadJogRight.Command = _ChangePadPositionCommand;
                PadJogUp.Command = _ChangePadPositionCommand;
                PadJogDown.Command = _ChangePadPositionCommand;

                if (VisionManager.GetDispHorFlip() == DispFlipEnum.FLIP)
                {
                    PadJogLeft.CommandParameter = JOG_DIRECTION.RIGHT;
                    PadJogRight.CommandParameter = JOG_DIRECTION.LEFT;
                }
                else
                {
                    PadJogLeft.CommandParameter = JOG_DIRECTION.LEFT;
                    PadJogRight.CommandParameter = JOG_DIRECTION.RIGHT;
                }

                if (VisionManager.GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    PadJogUp.CommandParameter = JOG_DIRECTION.DOWN;
                    PadJogDown.CommandParameter = JOG_DIRECTION.UP;
                }
                else
                {                    
                    PadJogUp.CommandParameter = JOG_DIRECTION.UP;
                    PadJogDown.CommandParameter = JOG_DIRECTION.DOWN;
                }

                PadJogSelect.IsEnabled = false;

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-up.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-down.png");
                #endregion

                MiniViewSwapVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// PNP Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.PMIModule().SetSubModule(this);

                PMIPadObject movedpadinfo = null;

                retVal = this.PMIModule().EnterMovePadPosition(ref movedpadinfo);

                InitLightJog(this);

                retVal = InitPNPSetupUI();

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                MainViewTarget = DisplayPort;

                //TemplateMiniView = new TemplateMiniView();
                //TemplateMiniView.DataContext = this;

                MiniViewTarget = Wafer;

                UseUserControl = UserControlFucEnum.DEFAULT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;

                this.PMIModule().UpdateRenderLayer();

                UpdateOverlayDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Debug($"[PadPositionAdjustModule] [InitViewModel()] : {err}");
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void UpdateOverlay()
        {
            try
            {
                this.PMIModule().UpdateCurrentPadIndex();
                this.PMIModule().UpdateDisplayedDevices(this.CurCam);
                this.UpdateLabel();
                this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayDie() Error occurred.");
                LoggerManager.Exception(err);

            }
        }

        #endregion

        #region ==> Parameter Check, Page Switch
        /// <summary>
        /// 현재 단계의 Parameter Setting 이 다 되었는지 확인하는 함수.
        /// IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Modify 상태가 있는지 없는지를 확인해준다.
        /// retVal = Extensions_IParam.ElementStateModifyValidation(PMIDevParam);
        /// IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Update 상태가 있는지 없는지를 확인해준다.
        /// retVal = Extensions_IParam.ElementStateUpdateValidation(PMIDevParam);
        /// IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Apply 상태가 있는지 없는지를 확인해준다.
        /// retVal = Extensions_IParam.ElementStateApplyValidation(Param);
        /// </summary>
        /// <returns></returns>
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                EventCodeEnum retVal1 = Extensions_IParam.ElementStateDefaultValidation(this.PMIModule().PMIModuleDevParam_IParam);
                EventCodeEnum retVal2 = this.StageSupervisor().SaveWaferObject();

                if ((retVal1 == EventCodeEnum.NONE) && (retVal2 == EventCodeEnum.NONE))
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    //모듈의 Setup상태를 변경해준다.
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.UNDEFINED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [ParamValidation()] : {err}");
                LoggerManager.Exception(err);

                SetNodeSetupState(EnumMoudleSetupState.UNDEFINED);
            }

            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                EventCodeEnum retVal1 = Extensions_IParam.ElementStateDefaultValidation(this.PMIModule().PMIModuleDevParam_IParam);
                EventCodeEnum retVal2 = Extensions_IParam.ElementStateDefaultValidation(PadInfos);

                if ((retVal1 == EventCodeEnum.NONE) && (retVal2 == EventCodeEnum.NONE))
                {
                    retVal = false;
                }
                else
                {
                    retVal = true;
                }


                if (retVal == false)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    //모듈의 Setup상태를 변경해준다.
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.UNDEFINED);
                }
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        /// <summary>
        /// Parameter Save 해주는 함수
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                EventCodeEnum retVal1 = this.PMIModule().SaveDevParameter();
                EventCodeEnum retVal2 = this.StageSupervisor().SaveWaferObject();


                if ((retVal1 == EventCodeEnum.NONE) && (retVal2 == EventCodeEnum.NONE))
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [SaveDevParameter()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// PNP 화면 진입 시 호출되는 함수
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                SelectedPadTemplateIndex = 0;

                SelectedPadTemplate = null;
                SelectedPMIPad = null;

                DrawingGroup.SetupMode = PMI_SETUP_MODE.PAD;
                DrawingGroup.SelectedPMIPadIndex = 0;

                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[SelectedPadTemplateIndex];
                }

                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    SelectedPMIPad = PadInfos.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                }

                retVal = await InitSetup();

                curMode = SELECTION_MODE.SINGLE;


                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.StageSupervisor().WaferObject.DrawDieOverlay(CurCam);

                UpdateOverlay();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;

                LoggerManager.Debug($"[PadPositionAdjustModule] [PageSwitched()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void UpdateOverlayDelegate(DELEGATEONOFF flag)
        {
            try
            {
                if ((flag == DELEGATEONOFF.ON) && (OnDelegated == false))
                {
                    this.CoordinateManager().OverlayUpdateDelegate += UpdateOverlay;

                    this.PMIModule().UpdateRenderLayer();

                    OnDelegated = true;
                }
                else if (OnDelegated == true)
                {
                    this.CoordinateManager().OverlayUpdateDelegate -= UpdateOverlay;

                    this.PMIModule().ClearRenderObjects();

                    OnDelegated = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }


        /// <summary>
        /// PNP 화면 전환 시 호출되는 함수
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                UpdateOverlayDelegate(DELEGATEONOFF.OFF);

                this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);

                retVal = this.StageSupervisor().SaveWaferObject();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[PadPositionAdjustModule] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PadPositionAdjustModule] [Cleanup()] : Error");
                }

                //if (IsParameterChanged() == true)
                //    retVal = await base.Cleanup(null);
                //else
                //    retVal = await base.Cleanup(parameter);

                //if (retVal == EventCodeEnum.SAVE_PARAM)
                //{
                //    retVal = SaveDevParameter();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [Cleanup()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region ==> Properties
        private SELECTION_MODE _curMode = SELECTION_MODE.SINGLE;
        public SELECTION_MODE curMode
        {
            get { return _curMode; }
            set
            {
                if (value != _curMode)
                {
                    _curMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private PadTemplate _SelectedPadTemplate;
        public PadTemplate SelectedPadTemplate
        {
            get { return _SelectedPadTemplate; }
            set
            {
                if (value != _SelectedPadTemplate)
                {
                    _SelectedPadTemplate = value;
                    RaisePropertyChanged();

                    //this.PMIModule().ChangedPadTemplate(SelectedPadTemplate);
                }
            }
        }

        private int _SelectedPadTemplateIndex;
        public int SelectedPadTemplateIndex
        {
            get { return _SelectedPadTemplateIndex; }
            set
            {
                if (value != _SelectedPadTemplateIndex)
                {
                    _SelectedPadTemplateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _SelectedPMIPadIndex;
        //public int SelectedPMIPadIndex
        //{
        //    get { return _SelectedPMIPadIndex; }
        //    set
        //    {
        //        if (value != _SelectedPMIPadIndex)
        //        {
        //            _SelectedPMIPadIndex = value;
        //            RaisePropertyChanged();

        //            DrawingGroup.SelectedPMIPadIndex = _SelectedPMIPadIndex;
        //        }
        //    }
        //}

        private PMIPadObject _SelectedPMIPad;
        public PMIPadObject SelectedPMIPad
        {
            get { return _SelectedPMIPad; }
            set
            {
                if (value != _SelectedPMIPad)
                {
                    _SelectedPMIPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> Commands
        private RelayCommand<object> _ChangePadPositionCommand;
        private AsyncCommand<object> _PadMoveModeCommand;
        private AsyncCommand<SETUP_DIRECTION> _PadIndexMoveCommand;

        private Task ChangePadIndexCommand(SETUP_DIRECTION direction)
        {
            try
            {

                // TemplateInfo가 존재할 때
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    

                    if (direction == SETUP_DIRECTION.PREV)
                    {
                        if (DrawingGroup.SelectedPMIPadIndex == 0)
                        {
                            DrawingGroup.SelectedPMIPadIndex = PadInfos.PMIPadInfos.Count - 1;
                        }
                        else
                        {
                            DrawingGroup.SelectedPMIPadIndex--;
                        }
                    }
                    else
                    {
                        // 현재 Index가 마지막인 경우
                        if (DrawingGroup.SelectedPMIPadIndex == PadInfos.PMIPadInfos.Count - 1)
                        {
                            DrawingGroup.SelectedPMIPadIndex = 0;
                        }
                        else
                        {
                            DrawingGroup.SelectedPMIPadIndex++;
                        }
                    }

                    this.PMIModule().MoveToPad(CurCam, CurCam.GetCurCoordMachineIndex(), DrawingGroup.SelectedPMIPadIndex);

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [ChangePadIndexCommand()] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Display port에 label update 해주는 함수.
        /// </summary>
        public override void UpdateLabel()
        {
            try
            {
                string Mode = string.Empty;

                if (curMode == SELECTION_MODE.SINGLE)
                {
                    Mode = "Single";
                }
                else
                {
                    Mode = "All";
                }

                StepLabel = ($"Mode : {Mode}, Pad : {DrawingGroup.SelectedPMIPadIndex + 1} / {PadInfos.PMIPadInfos.Count}");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [UpdateLabel()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 선택된 패드 위치를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        private void FuncChangePadPositionCommand(object direction)
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    Point PadLeftTop = new Point();
                    Point PadBottomRight = new Point();

                    Rect Source = new Rect();
                    Rect Target = new Rect();

                    bool ExistOutOfPad = false;

                    WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();
                    WaferCoordinate PadCenWaferCoord;

                    MachineIndex mi = CurCam.GetCurCoordMachineIndex();

                    DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(mi.XIndex, mi.YIndex);

                    // Index Move에 사용되는 값 : this.StageSupervisor().WaferObject.GetSubsInfo().ActualDeviceSize

                    Point LeftTop = new Point
                        (
                            DieLowLeftCornerPos.X.Value + (this.StageSupervisor().WaferObject.GetSubsInfo().DieXClearance.Value / 2),
                            DieLowLeftCornerPos.Y.Value + (this.StageSupervisor().WaferObject.GetSubsInfo().DieYClearance.Value / 2) + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDeviceSize.Height.Value
                        );

                    Point RightBottom = new Point
                    (
                            DieLowLeftCornerPos.X.Value + (this.StageSupervisor().WaferObject.GetSubsInfo().DieXClearance.Value / 2) + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDeviceSize.Width.Value,
                            DieLowLeftCornerPos.Y.Value + (this.StageSupervisor().WaferObject.GetSubsInfo().DieYClearance.Value / 2)
                        );

                    if (curMode == SELECTION_MODE.SINGLE)
                    {
                        PMIPadObject SelectedPad = PadInfos.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];

                        PadCenWaferCoord = new WaferCoordinate((SelectedPad.PadCenter.X.Value + DieLowLeftCornerPos.X.Value),
                                                                               (SelectedPad.PadCenter.Y.Value + DieLowLeftCornerPos.Y.Value));

                        PadLeftTop = new Point(PadCenWaferCoord.GetX() - (SelectedPad.PadInfos.SizeX.Value / 2),
                                                      PadCenWaferCoord.GetY() + (SelectedPad.PadInfos.SizeY.Value / 2));
                        PadBottomRight = new Point(PadCenWaferCoord.GetX() + (SelectedPad.PadInfos.SizeX.Value / 2),
                                                      PadCenWaferCoord.GetY() - (SelectedPad.PadInfos.SizeY.Value / 2));

                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:
                                PadLeftTop.Y++;
                                break;
                            case JOG_DIRECTION.DOWN:
                                PadBottomRight.Y--;
                                break;
                            case JOG_DIRECTION.LEFT:
                                PadLeftTop.X--;
                                break;
                            case JOG_DIRECTION.RIGHT:
                                PadBottomRight.X++;
                                break;
                            default:
                                break;
                        }

                        // DIE
                        Source = new Rect(LeftTop, RightBottom);

                        // PAD
                        Target = new Rect(PadLeftTop, PadBottomRight);

                        ExistOutOfPad = Target.IntersectsWith(Source); // IsIntersectsWith(Target, Source);

                        if (ExistOutOfPad == true)
                        {
                            switch (direction)
                            {
                                case JOG_DIRECTION.UP:
                                    SelectedPad.PadCenter.Y.Value++;
                                    break;
                                case JOG_DIRECTION.DOWN:
                                    SelectedPad.PadCenter.Y.Value--;
                                    break;
                                case JOG_DIRECTION.LEFT:
                                    SelectedPad.PadCenter.X.Value--;
                                    break;
                                case JOG_DIRECTION.RIGHT:
                                    SelectedPad.PadCenter.X.Value++;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Point LLConer = this.WaferAligner().GetLeftCornerPosition(CurCam.CamSystemPos.GetX(), CurCam.CamSystemPos.GetY());

                        // Die 경계를 넘어가는 경우, 실행 취소

                        //  --------
                        // ㅣ      ㅣ
                        // ㅣ      ㅣ
                        // ㅣ      ㅣ
                        // ㅣ      ㅣ
                        //  --------

                        double[,] CornerPos = new double[2, 2];
                        double[,] PadPos = new double[2, 2];

                        var wafersubinfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                        CornerPos[0, 0] = LLConer.X + (wafersubinfo.DieXClearance.Value / 2);
                        CornerPos[0, 1] = LLConer.Y + wafersubinfo.ActualDieSize.Height.Value - (wafersubinfo.DieYClearance.Value / 2);

                        CornerPos[1, 0] = LLConer.X + wafersubinfo.ActualDieSize.Width.Value - (wafersubinfo.DieXClearance.Value / 2);
                        CornerPos[1, 1] = LLConer.Y + (wafersubinfo.DieYClearance.Value / 2);

                        for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                        {
                            PadPos[0, 0] = CurCam.CamSystemPos.GetX() - (SelectedPadTemplate.SizeX.Value / 2);
                            PadPos[0, 1] = CurCam.CamSystemPos.GetY() + (SelectedPadTemplate.SizeY.Value / 2);

                            PadPos[1, 0] = CurCam.CamSystemPos.GetX() + (SelectedPadTemplate.SizeX.Value / 2);
                            PadPos[1, 1] = CurCam.CamSystemPos.GetY() - (SelectedPadTemplate.SizeY.Value / 2);

                            if ((PadPos[0, 0] < CornerPos[0, 0]) ||
                             (PadPos[1, 0] > CornerPos[1, 0]) ||
                             (PadPos[0, 1] > CornerPos[0, 1]) ||
                             (PadPos[1, 1] < CornerPos[1, 1])
                                )
                            {
                                ExistOutOfPad = true;
                                break;
                            }
                        }

                        if (ExistOutOfPad == false)
                        {
                            for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                            {
                                switch (direction)
                                {
                                    case JOG_DIRECTION.UP:
                                        PadInfos.PMIPadInfos[i].PadCenter.Y.Value++;
                                        break;
                                    case JOG_DIRECTION.DOWN:
                                        PadInfos.PMIPadInfos[i].PadCenter.Y.Value--;
                                        break;
                                    case JOG_DIRECTION.LEFT:
                                        PadInfos.PMIPadInfos[i].PadCenter.X.Value--;
                                        break;
                                    case JOG_DIRECTION.RIGHT:
                                        PadInfos.PMIPadInfos[i].PadCenter.X.Value++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < PMIInfo.PadTableTemplateInfo.Count; i++)
                    {
                        PMIInfo.PadTableTemplateInfo[i].GroupingDone.Value = false;
                    }

                    this.PMIModule().UpdateDisplayedDevices(CurCam);
                    this.PMIModule().UpdateRenderLayer();
                }

                //if (PadInfos.PMIPadInfos.Count > 0)
                //{
                //    //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");

                //    this.PMIModule().ChangePadPositionCommand(curMode, (JOG_DIRECTION)direction);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [FuncChangePadPositionCommand(" + $"{direction}" + $")] : {err}");
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());
            }
        }

        /// <summary>
        /// 패드 이동 모드를 변경하는 함수
        /// </summary>
        /// <param name="mode"></param>
        private Task PadMoveModeCommand(object mode)
        {
            try
            {
                curMode = (SELECTION_MODE)mode;

                UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadPositionAdjustModule] [PadMoveModeCommand(" + $"{mode}" + $")] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        ///// <summary>
        ///// 선택된 패드 index를 변경하는 함수
        ///// </summary>
        ///// <param name="direction"></param>
        //private async void PadIndexMoveCommand(object direction)
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

        //            this.PMIModule().MoveToPad(CurCam, PadInfos.SelectedPMIPadIndex);

        //            this.PMIModule().UpdateRenderLayer();

        //            UpdateLabel();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadPositionAdjustModule] [PadIndexMoveCommand(" + $"{direction}" + $")] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}
        #endregion
    }
}
