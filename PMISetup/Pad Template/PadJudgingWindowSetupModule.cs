using LogModule;
using PMISetup.UC;
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
    public class PadJudgingWindowSetupModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup, IPMITemplateMiniViewModel
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("0FE97FE5-5774-9D69-732E-9DFD9C3C93BF");

        //private IStateModule _PMIModule;
        //public IStateModule PMIModule
        //{
        //    get { return _PMIModule; }
        //    set
        //    {
        //        if (value != _PMIModule)
        //        {
        //            _PMIModule = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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
        private int _PadTemplateInfoCount;
        public int PadTemplateInfoCount
        {
            get { return _PadTemplateInfoCount; }
            set
            {
                if (value != _PadTemplateInfoCount)
                {
                    _PadTemplateInfoCount = value;
                    RaisePropertyChanged();

                    UpdateMiniViewModel();
                }
            }
        }


        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }

        }

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }
        }

        public PadJudgingWindowSetupModule()
        {

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

                    DrawingGroup.PadTemplate = _SelectedPadTemplate;

                    UpdateMiniViewModel();
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

                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[_SelectedPadTemplateIndex];
                }
            }
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

        private TemplateMiniView TemplateMiniView;

        //private PadTemplateSelectionControlService TemplateSelection;

        //private WaferObject Wafer { get; set; }

        //private ProbeCard ProbeCard { get { return this.StageSupervisor().ProbeCardInfo; } }
        #endregion

        #region ==> Common PNP Function
        /// <summary>
        /// 실제 프로세싱 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MovingState.Moving();
                if (IsExecute())
                {

                }
                //this.VisionManager().PatternMatching()
                /*
                    실제 프로세싱 코드 작성


                 */
                MovingState.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [DoExecute()] : {err}");
            }

            return retVal;
        }

        /// <summary>
        /// 현재 Parameter Check 및 Init하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [DoClearData()] : {err}");
            }

            return retVal;
        }

        /// <summary>
        /// Recovery때 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [DoRecovery()] : {err}");
            }

            return retVal;
        }

        /// <summary>
        /// Recovery 종료할 때 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [DoExitRecovery()] : {err}");
            }

            return retVal;
        }

        /// <summary>
        /// SubModule이 Processing 가능한지 판단하는 조건 
        /// </summary>
        /// <returns></returns>
        public bool IsExecute()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [IsExecute()] : {err}");
            }

            return true;
        }
        #endregion

        public override bool Initialized { get; set; } = false;

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

                    //TemplateSelection = new PadTemplateSelectionControlService();

                    DrawingGroup = new PMIDrawingGroup();
                    DrawingGroup.Template = true;
                    DrawingGroup.JudgingWindow = true;

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
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [InitModule()] : {err}");
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
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [DeInitModule()] : {err}");
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
                Header = "Pad Judging Window Setup";


                retVal = InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);


                _ChangeTemplateIndexCommand = new AsyncCommand<SETUP_DIRECTION>(ChangeTemplateIndexCommand);
                _ChangeJudgingWindowSizeCommand = new RelayCommand<JOG_DIRECTION>(ChangeJudgingWindowSizeCommand);

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
                PadJogLeftUp.IconCaption = "TEMPLATE";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "TEMPLATE";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogLeftDown.Caption = null;
                PadJogRightDown.Caption = null;
                PadJogSelect.Caption = null;

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;

                PadJogLeftUp.Command = _ChangeTemplateIndexCommand;
                PadJogRightUp.Command = _ChangeTemplateIndexCommand;
                PadJogLeftDown.Command = null;
                PadJogRightDown.Command = null;
                PadJogSelect.Command = null;

                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;

                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = false;
                PadJogSelect.IsEnabled = false;
                #endregion

                #region ==> Jog Buttons
                PadJogSelect.IsEnabled = false;

                PadJogLeft.Command = _ChangeJudgingWindowSizeCommand;
                PadJogRight.Command = _ChangeJudgingWindowSizeCommand;
                PadJogUp.Command = _ChangeJudgingWindowSizeCommand;
                PadJogDown.Command = _ChangeJudgingWindowSizeCommand;

                PadJogLeft.CommandParameter = JOG_DIRECTION.LEFT;
                PadJogRight.CommandParameter = JOG_DIRECTION.RIGHT;
                PadJogUp.CommandParameter = JOG_DIRECTION.UP;
                PadJogDown.CommandParameter = JOG_DIRECTION.DOWN;

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-collapse-horizontal.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-split-vertical.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-split-horizontal.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-collapse-vertical.png");
                #endregion

                MiniViewSwapVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                // MiniView 화면에 template 화면이 나온다.
                TemplateMiniView = new TemplateMiniView();
                TemplateMiniView.DataContext = this;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [InitViewModel()] : {err}");
            }
            return Task.FromResult<EventCodeEnum>(retVal);
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
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                // MainView 화면에 Camera(Vision) 화면이 나온다.
                MainViewTarget = DisplayPort;

                MiniViewTarget = TemplateMiniView;

                MiniViewSwapVisibility = Visibility.Hidden;

                UseUserControl = UserControlFucEnum.DEFAULT;

                //TargetRectangleWidth = 128;
                //TargetRectangleHeight = 128;

                this.PMIModule().UpdateRenderLayer();

                UpdateOverlayDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [InitSetup()] : {err}");
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void UpdateOverlay()
        {

            try
            {
                this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayDie() Error occurred.");
                LoggerManager.Exception(err);

            }
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

        ///// <summary>
        ///// Recovery시에 설정할데이터 화면등을 정의.
        ///// </summary>
        ///// <returns></returns>
        //public Task<EventCodeEnum> InitRecovery()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        InitPnpUI();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadJudgingWindowSetupModule] [InitRecovery()] : {err}");
        //    }

        //    return Task.FromResult<EventCodeEnum>(retVal);
        //}

        ///// <summary>
        ///// PNP 화면 버튼 등을 정의.
        ///// </summary>
        ///// <returns></returns>
        //private EventCodeEnum InitPnpUI()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadJudgingWindowSetupModule] [InitPnpUI()] : {err}");
        //    }

        //    return retVal;
        //}
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
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [ParamValidation()] : {err}");
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
                EventCodeEnum retVal2 = Extensions_IParam.ElementStateDefaultValidation(this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo());

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
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [SaveDevParameter()] : {err}");
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

                DrawingGroup.PadTableTemplate = null;
                DrawingGroup.SetupMode = PMI_SETUP_MODE.JUDGINGWINDOW;
                DrawingGroup.SelectedPMIPadIndex = 0;

                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[SelectedPadTemplateIndex];
                }

                retVal = await InitSetup();

                UpdateMiniViewModel();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.PMIModule().UpdateRenderLayer();

                UpdateLabel();

                ChangeButtonEnable();

                //PMIInfo.SetupMode = PMI_SETUP_MODE.JUDGINGWINDOW;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;

                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [PageSwitched()] : {err}");
            }

            return retVal;
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

                retVal = this.StageSupervisor().SaveWaferObject();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[PadJudgingWindowSetupModule] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PadJudgingWindowSetupModule] [Cleanup()] : Error");
                }

                //if (IsParameterChanged() == true)
                //    retVal = await base.Cleanup(null);
                //else
                //    retVal = await base.Cleanup(parameter);

                //if (retVal == EventCodeEnum.SAVE_PARAM)
                //{
                //    retVal = SaveDevParameter();
                //}
                //PMIInfo.SetupMode = PMI_SETUP_MODE.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [Cleanup()] : {err}");
            }

            return retVal;
        }
        #endregion

        #region ==> Properties
        #endregion

        #region ==> Commands
        private AsyncCommand<SETUP_DIRECTION> _ChangeTemplateIndexCommand;
        private RelayCommand<JOG_DIRECTION> _ChangeJudgingWindowSizeCommand;

        /// <summary>
        /// Display port에 label update 하는 함수.
        /// </summary>
        public override void UpdateLabel()
        {
            try
            {
                //StepLabel = "";

                //var CurTemplate = PMIInfo.SelectedPadTemplate;

                //if (CurTemplate != null)
                //{
                //    StepLabel = String.Format("{0}, X : {1} Y : {2}",
                //        CurTemplate.ShapeName.Value,
                //        string.Format("{0:F1}", CurTemplate.JudgingWindowSizeX.Value),
                //        string.Format("{0:F1}", CurTemplate.JudgingWindowSizeY.Value));
                //    //(Math.Truncate(CurTemplate.JudgingWindowSizeX.Value * 10) / 10).ToString("#.#"),
                //    //(Math.Truncate(CurTemplate.JudgingWindowSizeY.Value * 10) / 10).ToString("#.#"));
                //}
                //else
                //{
                //    StepLabel = "";
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [UpdateLabel()] : {err}");
            }
        }

        /// <summary>
        /// 선택된 Template의 Judging Window 사이즈를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        private void ChangeJudgingWindowSizeCommand(JOG_DIRECTION direction)
        {
            try
            {
                // TODO

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                double CameraRatioX = Math.Round(CurCam.GetRatioX(), 1);

                double JudgingWindowSizeScalingUnit = CameraRatioX;

                if (SelectedPadTemplate != null)
                {
                    if (SelectedPadTemplate.JudgingWindowMode.Value == PAD_JUDGING_WINDOW_MODE.TWOWAY)
                    {
                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:
                                if (SelectedPadTemplate.SizeY.Value / 2 - SelectedPadTemplate.JudgingWindowSizeY.Value > JudgingWindowSizeScalingUnit)
                                    SelectedPadTemplate.JudgingWindowSizeY.Value += JudgingWindowSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.DOWN:
                                if (SelectedPadTemplate.JudgingWindowSizeY.Value - JudgingWindowSizeScalingUnit >= 0)
                                    SelectedPadTemplate.JudgingWindowSizeY.Value -= JudgingWindowSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.LEFT:
                                if (SelectedPadTemplate.JudgingWindowSizeX.Value - JudgingWindowSizeScalingUnit >= 0)
                                    SelectedPadTemplate.JudgingWindowSizeX.Value -= JudgingWindowSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.RIGHT:
                                if (SelectedPadTemplate.SizeX.Value / 2 - SelectedPadTemplate.JudgingWindowSizeX.Value > JudgingWindowSizeScalingUnit)
                                    SelectedPadTemplate.JudgingWindowSizeX.Value += JudgingWindowSizeScalingUnit;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:
                                LoggerManager.Error($"This is Alvin. Please check the logic. the logic should not come in.");
                                break;
                            case JOG_DIRECTION.DOWN:
                                LoggerManager.Error($"This is Alvin. Please check the logic. the logic should not come in.");
                                break;
                            case JOG_DIRECTION.LEFT:

                                if (SelectedPadTemplate.JudgingWindowSizeX.Value - JudgingWindowSizeScalingUnit >= 0)
                                    SelectedPadTemplate.JudgingWindowSizeX.Value -= JudgingWindowSizeScalingUnit;

                                if (SelectedPadTemplate.JudgingWindowSizeY.Value - JudgingWindowSizeScalingUnit >= 0)
                                    SelectedPadTemplate.JudgingWindowSizeY.Value -= JudgingWindowSizeScalingUnit;

                                break;
                            case JOG_DIRECTION.RIGHT:

                                if (SelectedPadTemplate.SizeX.Value / 2 - SelectedPadTemplate.JudgingWindowSizeX.Value > JudgingWindowSizeScalingUnit)
                                    SelectedPadTemplate.JudgingWindowSizeX.Value += JudgingWindowSizeScalingUnit;

                                if (SelectedPadTemplate.SizeY.Value / 2 - SelectedPadTemplate.JudgingWindowSizeY.Value > JudgingWindowSizeScalingUnit)
                                    SelectedPadTemplate.JudgingWindowSizeY.Value += JudgingWindowSizeScalingUnit;

                                break;
                            default:
                                break;
                        }
                    }

                    //// Angle (0 ~ 90) or (180 ~ 270)
                    //if (((SelectedPadTemplate.Angle.Value >= 0) && (SelectedPadTemplate.Angle.Value < 90)) ||
                    //     (SelectedPadTemplate.Angle.Value >= 180) && (SelectedPadTemplate.Angle.Value < 270))
                    //{
                    //    switch (direction)
                    //    {
                    //        case JOG_DIRECTION.UP:
                    //            if (SelectedPadTemplate.SizeY.Value / 2 - SelectedPadTemplate.JudgingWindowSizeY.Value > JudgingWindowSizeScalingUnit)
                    //                SelectedPadTemplate.JudgingWindowSizeY.Value += JudgingWindowSizeScalingUnit;
                    //            break;
                    //        case JOG_DIRECTION.DOWN:
                    //            if (SelectedPadTemplate.JudgingWindowSizeY.Value - JudgingWindowSizeScalingUnit >= 0)
                    //                SelectedPadTemplate.JudgingWindowSizeY.Value -= JudgingWindowSizeScalingUnit;
                    //            break;
                    //        case JOG_DIRECTION.LEFT:
                    //            if (SelectedPadTemplate.JudgingWindowSizeX.Value - JudgingWindowSizeScalingUnit >= 0)
                    //                SelectedPadTemplate.JudgingWindowSizeX.Value -= JudgingWindowSizeScalingUnit;
                    //            break;
                    //        case JOG_DIRECTION.RIGHT:
                    //            if (SelectedPadTemplate.SizeX.Value / 2 - SelectedPadTemplate.JudgingWindowSizeX.Value > JudgingWindowSizeScalingUnit)
                    //                SelectedPadTemplate.JudgingWindowSizeX.Value += JudgingWindowSizeScalingUnit;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}
                    //// Angle (90 ~ 180) or (270 ~ 360)
                    //else
                    //{
                    //    switch (direction)
                    //    {
                    //        case JOG_DIRECTION.UP:
                    //            if (SelectedPadTemplate.SizeX.Value / 2 - SelectedPadTemplate.JudgingWindowSizeX.Value > JudgingWindowSizeScalingUnit)
                    //                SelectedPadTemplate.JudgingWindowSizeX.Value += JudgingWindowSizeScalingUnit;
                    //            break;
                    //        case JOG_DIRECTION.DOWN:
                    //            if (SelectedPadTemplate.JudgingWindowSizeX.Value - JudgingWindowSizeScalingUnit >= 0)
                    //                SelectedPadTemplate.JudgingWindowSizeX.Value -= JudgingWindowSizeScalingUnit;
                    //            break;
                    //        case JOG_DIRECTION.LEFT:
                    //            if (SelectedPadTemplate.JudgingWindowSizeY.Value - JudgingWindowSizeScalingUnit >= 0)
                    //                SelectedPadTemplate.JudgingWindowSizeY.Value -= JudgingWindowSizeScalingUnit;
                    //            break;
                    //        case JOG_DIRECTION.RIGHT:
                    //            if (SelectedPadTemplate.SizeY.Value / 2 - SelectedPadTemplate.JudgingWindowSizeY.Value > JudgingWindowSizeScalingUnit)
                    //                SelectedPadTemplate.JudgingWindowSizeY.Value += JudgingWindowSizeScalingUnit;
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}

                    if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                    {
                        PMITemplateMiniViewModel vm = new PMITemplateMiniViewModel();

                        vm.SelectedPadTemplateIndex = SelectedPadTemplateIndex;
                        vm.PadTemplateInfoCount = PadTemplateInfoCount;
                        vm.SelectedPadTemplate = SelectedPadTemplate;
                        vm.DrawingGroup = DrawingGroup;

                        this.LoaderRemoteMediator().GetServiceCallBack()?.UpdatgePMITemplateMiniViewModel(vm);
                    }

                    this.PMIModule().UpdateRenderLayer();
                }

                //JOG_DIRECTION dir = (JOG_DIRECTION)direction;

                //this.PMIModule().ChangeJudgingWindowSizeCommand(dir);

                UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [ChangeJudgingWindowSizeCommand(" + $"{direction}" + $")] : {err}");
            }
        }

        /// <summary>
        /// 선택된 Template Index를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        private Task ChangeTemplateIndexCommand(SETUP_DIRECTION direction)
        {
            try
            {
                // TemplateInfo가 존재할 때
                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                    

                    if (direction == SETUP_DIRECTION.PREV)
                    {
                        if (SelectedPadTemplateIndex == 0)
                        {
                            SelectedPadTemplateIndex = PMIInfo.PadTemplateInfo.Count - 1;
                        }
                        else
                        {
                            SelectedPadTemplateIndex--;
                        }
                    }
                    else
                    {
                        // 현재 Index가 마지막인 경우
                        if (SelectedPadTemplateIndex == PMIInfo.PadTemplateInfo.Count - 1)
                        {
                            SelectedPadTemplateIndex = 0;
                        }
                        else
                        {
                            SelectedPadTemplateIndex++;
                        }
                    }

                    ChangeButtonEnable();
                }


                UpdateLabel();

                UpdateMiniViewModel();

                this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [ChangeTemplateIndexCommand(" + $"{direction}" + $")] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 현재 선택된 Template에 따라 Button의 Enable을 변경하는 함수
        /// </summary>
        public void ChangeButtonEnable()
        {
            try
            {
                if (SelectedPadTemplate.JudgingWindowMode.Value == PAD_JUDGING_WINDOW_MODE.TWOWAY)
                {
                    PadJogUp.IsEnabled = true;
                    PadJogDown.IsEnabled = true;
                }
                else
                {
                    PadJogUp.IsEnabled = false;
                    PadJogDown.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadJudgingWindowSetupModule] [ChangeButtonEnable()] : {err}");
            }
        }

        private void UpdateMiniViewModel()
        {
            if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
            {
                PMITemplateMiniViewModel vm = new PMITemplateMiniViewModel();

                vm.SelectedPadTemplateIndex = SelectedPadTemplateIndex;
                vm.PadTemplateInfoCount = PadTemplateInfoCount;
                vm.SelectedPadTemplate = SelectedPadTemplate;
                vm.DrawingGroup = DrawingGroup;

                this.LoaderRemoteMediator().GetServiceCallBack()?.UpdatgePMITemplateMiniViewModel(vm);
            }

        }

        public void UpdatePMITemplateMiniViewModel()
        {
            try
            {
                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
