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
    public class ProbeMarkSizeSetupModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup, IPMITemplateMiniViewModel
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("96F03BEF-D3EE-05DB-464E-2A9F5C059085");

        private IStateModule _PMIModule;
        public IStateModule PMIModule
        {
            get { return _PMIModule; }
            set
            {
                if (value != _PMIModule)
                {
                    _PMIModule = value;
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

        public ProbeMarkSizeSetupModule()
        {

        }

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

        double ProbeMarkSizeScalingUnit = 1;

        public SubModuleStateBase SubModuleState { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        //private PadTemplate _CurrentPMITemplate;
        //public PadTemplate CurrentPMITemplate
        //{
        //    get { return _CurrentPMITemplate; }
        //    set
        //    {
        //        if (value != _CurrentPMITemplate)
        //        {
        //            _CurrentPMITemplate = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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

        //private PadTemplateSelectionControlService TemplateSelection;

        private TemplateMiniView TemplateMiniView;

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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [DoExecute()] : {err}");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [DoClearData()] : {err}");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [DoRecovery()] : {err}");
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
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [DoExitRecovery()] : {err}");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [IsExecute()] : {err}");
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
                    PMIModule = this.PMIModule();

                    InitLightJog(this);

                    //Wafer = this.StageSupervisor().WaferObject as WaferObject;

                    //PMIDevParam = ((PMIModule as IHasDevParameterizable).DevParam) as PMIModuleDevParam;

                    //TemplateSelection = new PadTemplateSelectionControlService();

                    DrawingGroup = new PMIDrawingGroup();
                    DrawingGroup.Template = true;
                    DrawingGroup.MarkMin = true;
                    DrawingGroup.MarkMax = false;

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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [InitModule()] : {err}");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [DeInitModule()] : {err}");
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
                Header = "Probe Mark Size Setup";

                retVal = InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                _ChangeMarkSizeCommand = new RelayCommand<JOG_DIRECTION>(ChangeMarkSizeCommand);
                _ChangeTemplateIndexCommand = new AsyncCommand<SETUP_DIRECTION>(ChangeTemplateIndexCommand);
                _ChangeMarkModeCommand = new AsyncCommand<object>(ChangeMarkModeCommand);

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
                PadJogLeftDown.Caption = "MIN";
                PadJogRightDown.Caption = "MAX";

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;

                PadJogLeftUp.Command = _ChangeTemplateIndexCommand;
                PadJogRightUp.Command = _ChangeTemplateIndexCommand;
                PadJogLeftDown.Command = _ChangeMarkModeCommand;
                PadJogRightDown.Command = _ChangeMarkModeCommand;

                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;
                PadJogLeftDown.CommandParameter = MARK_SIZE.MIN;
                PadJogRightDown.CommandParameter = MARK_SIZE.MAX;
                #endregion

                #region ==> Jog Buttons
                PadJogSelect.Caption = null;
                PadJogSelect.IsEnabled = false;

                PadJogSelect.Command = null;
                PadJogLeft.Command = _ChangeMarkSizeCommand;
                PadJogRight.Command = _ChangeMarkSizeCommand;
                PadJogUp.Command = _ChangeMarkSizeCommand;
                PadJogDown.Command = _ChangeMarkSizeCommand;

                PadJogLeft.CommandParameter = JOG_DIRECTION.LEFT;
                PadJogRight.CommandParameter = JOG_DIRECTION.RIGHT;
                PadJogUp.CommandParameter = JOG_DIRECTION.UP;
                PadJogDown.CommandParameter = JOG_DIRECTION.DOWN;

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
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

                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [InitViewModel()] : {err}");
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

                (PMIModule as IPMIModule).UpdateRenderLayer();


                UpdateOverlayDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [InitSetup()] : {err}");
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

        /// <summary>
        /// Recovery시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [InitRecovery()] : {err}");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

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
        //        LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [InitPnpUI()] : {err}");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [ParamValidation()] : {err}");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [SaveDevParameter()] : {err}");
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
                DrawingGroup.SelectedPMIPadIndex = 0;

                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                if (curMode == MARK_SIZE.MIN)
                {
                    DrawingGroup.SetupMode = PMI_SETUP_MODE.MARKMIN;
                }
                else
                {
                    DrawingGroup.SetupMode = PMI_SETUP_MODE.MARKMAX;
                }

                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[SelectedPadTemplateIndex];
                }

                retVal = await InitSetup();

                UpdateMiniViewModel();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                (PMIModule as IPMIModule).UpdateRenderLayer();

                UpdateLabel();

                //if (curMode == MARK_SIZE.MIN)
                //{
                //    //PMIInfo.SetupMode = PMI_SETUP_MODE.MARKMIN;
                //}
                //else
                //{
                //    //PMIInfo.SetupMode = PMI_SETUP_MODE.MARKMAX;
                //}

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [PageSwitched()] : {err}");
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
                    LoggerManager.Error($"[ProbeMarkSizeSetupModule] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [Cleanup()] : Error");
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
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [Cleanup()] : {err}");
            }

            return retVal;
        }
        #endregion

        #region ==> Properties
        private MARK_SIZE _curMode = MARK_SIZE.MIN;
        public MARK_SIZE curMode
        {
            get { return _curMode; }
            set
            {
                if (value != _curMode)
                {
                    _curMode = value;
                    RaisePropertyChanged();

                    if (_curMode == MARK_SIZE.MIN)
                    {
                        DrawingGroup.SetupMode = PMI_SETUP_MODE.MARKMIN;
                    }
                    else
                    {
                        DrawingGroup.SetupMode = PMI_SETUP_MODE.MARKMAX;
                    }
                }
            }
        }
        #endregion

        #region ==> Commands
        private AsyncCommand<SETUP_DIRECTION> _ChangeTemplateIndexCommand;
        private AsyncCommand<object> _ChangeMarkModeCommand;
        private RelayCommand<JOG_DIRECTION> _ChangeMarkSizeCommand;

        /// <summary>
        /// Display port에 label update 해주는 함수.
        /// </summary>
        public override void UpdateLabel()
        {
            try
            {
                //StepLabel = "";

                //if (PMIModule is IPMIModule)
                //{
                //    var SelectedTemplate = PMIInfo.SelectedPadTemplate;

                //    if (SelectedTemplate != null)
                //    {
                //        if (curMode == MARK_SIZE.MIN)
                //        {
                //            StepLabel = $"Min X {string.Format("{0:F0}", SelectedTemplate.MarkWindowMinSizeX.Value)}" +
                //                        $"Min Y {string.Format("{0:F0}", SelectedTemplate.MarkWindowMinSizeY.Value)}" +
                //                        $"{string.Format("{0:F1}", SelectedTemplate.MarkWindowMinPercent.Value)} %";

                //                //String.Format(
                //                //"{0}, Min X : {1} Min Y : {2} ({3} %)",
                //                //SelectedTemplate.ShapeName.Value,
                //                //string.Format("{0:F0}", SelectedTemplate.MarkWindowMinSizeX.Value),
                //                //string.Format("{0:F0}", SelectedTemplate.MarkWindowMinSizeY.Value),
                //                //string.Format("{0:F1}", SelectedTemplate.MarkWindowMinPercent.Value));

                //            //StepLabel = String.Format(
                //            //    "{0}, Min X : {1} Min Y : {2} ({3} %)",
                //            //    SelectedTemplate.ShapeName.Value,
                //            //    string.Format("{0:F0}", SelectedTemplate.MarkWindowMinSizeX.Value),
                //            //    string.Format("{0:F0}", SelectedTemplate.MarkWindowMinSizeY.Value),
                //            //    string.Format("{0:F1}", SelectedTemplate.MarkWindowMinPercent.Value));
                //        }
                //        else if (curMode == MARK_SIZE.MAX)
                //        {
                //            StepLabel = $"Max X {string.Format("{0:F0}", SelectedTemplate.MarkWindowMaxSizeX.Value)}" +
                //                        $"Max Y {string.Format("{0:F0}", SelectedTemplate.MarkWindowMaxSizeY.Value)}" +
                //                        $"{string.Format("{0:F1}", SelectedTemplate.MarkWindowMaxPercent.Value)} %";

                //            //StepLabel = String.Format(
                //            //    "{0}, Max X : {1} Max Y : {2} ({3} %)",
                //            //    SelectedTemplate.ShapeName.Value,
                //            //    string.Format("{0:F0}", SelectedTemplate.MarkWindowMaxSizeX.Value),
                //            //    string.Format("{0:F0}", SelectedTemplate.MarkWindowMaxSizeY.Value),
                //            //    string.Format("{0:F1}", SelectedTemplate.MarkWindowMaxPercent.Value));
                //        }
                //    }
                //    else
                //    {
                //        StepLabel = "";
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [UpdateLabel()] : {err}");
            }
        }

        /// <summary>
        /// Mark Min/Max 사이즈를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        private void ChangeMarkSizeCommand(JOG_DIRECTION direction)
        {
            try
            {
                if (SelectedPadTemplate != null)
                {
                    if (curMode == MARK_SIZE.MIN)
                    {
                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:
                                if ((SelectedPadTemplate.MarkWindowMinSizeY.Value + ProbeMarkSizeScalingUnit) < SelectedPadTemplate.SizeY.Value)
                                    SelectedPadTemplate.MarkWindowMinSizeY.Value += ProbeMarkSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.DOWN:
                                if ((SelectedPadTemplate.MarkWindowMinSizeY.Value - ProbeMarkSizeScalingUnit) >= 0)
                                    SelectedPadTemplate.MarkWindowMinSizeY.Value -= ProbeMarkSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.LEFT:
                                if ((SelectedPadTemplate.MarkWindowMinSizeX.Value - ProbeMarkSizeScalingUnit) >= 0)
                                    SelectedPadTemplate.MarkWindowMinSizeX.Value -= ProbeMarkSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.RIGHT:

                                if ((SelectedPadTemplate.MarkWindowMinSizeX.Value + ProbeMarkSizeScalingUnit) < SelectedPadTemplate.SizeX.Value)
                                {
                                    SelectedPadTemplate.MarkWindowMinSizeX.Value += ProbeMarkSizeScalingUnit;
                                }

                                break;
                            default:
                                break;
                        }

                        SelectedPadTemplate.MarkWindowMinPercent.Value = (SelectedPadTemplate.MarkWindowMinSizeX.Value * SelectedPadTemplate.MarkWindowMinSizeY.Value) / (SelectedPadTemplate.SizeX.Value * SelectedPadTemplate.SizeY.Value) * 100;
                    }
                    else
                    {
                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:
                                if ((SelectedPadTemplate.MarkWindowMaxSizeY.Value + ProbeMarkSizeScalingUnit) <= SelectedPadTemplate.SizeY.Value)
                                    SelectedPadTemplate.MarkWindowMaxSizeY.Value += ProbeMarkSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.DOWN:
                                if ((SelectedPadTemplate.MarkWindowMaxSizeY.Value - ProbeMarkSizeScalingUnit) > 0)
                                    SelectedPadTemplate.MarkWindowMaxSizeY.Value -= ProbeMarkSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.LEFT:
                                if ((SelectedPadTemplate.MarkWindowMaxSizeX.Value - ProbeMarkSizeScalingUnit) > 0)
                                    SelectedPadTemplate.MarkWindowMaxSizeX.Value -= ProbeMarkSizeScalingUnit;
                                break;
                            case JOG_DIRECTION.RIGHT:
                                if ((SelectedPadTemplate.MarkWindowMaxSizeX.Value + ProbeMarkSizeScalingUnit) <= SelectedPadTemplate.SizeX.Value)
                                    SelectedPadTemplate.MarkWindowMaxSizeX.Value += ProbeMarkSizeScalingUnit;
                                break;
                            default:
                                break;
                        }

                        SelectedPadTemplate.MarkWindowMaxPercent.Value = (SelectedPadTemplate.MarkWindowMaxSizeX.Value * SelectedPadTemplate.MarkWindowMaxSizeY.Value) / (SelectedPadTemplate.SizeX.Value * SelectedPadTemplate.SizeY.Value) * 100;
                    }


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

                //(PMIModule as IPMIModule).ChangeMarkSizeCommand(curMode, dir);

                //UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [ChangeMarkSizeCommand()] : {err}" + $" {curMode}" + $" {direction}");
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
                }

                UpdateLabel();
                UpdateMiniViewModel();

                this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [ChangeTemplateIndexCommand(" + $"{direction}" + $")] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 마크 사이즈를 지정할 Min/Max 모드를 선택하는 함수
        /// </summary>
        /// <param name="mode"></param>
        private Task ChangeMarkModeCommand(object mode)
        {
            try
            {
                curMode = (MARK_SIZE)mode;

                if (curMode == MARK_SIZE.MIN)
                {
                    //PMIInfo.SetupMode = PMI_SETUP_MODE.MARKMIN;
                }
                else
                {
                    //PMIInfo.SetupMode = PMI_SETUP_MODE.MARKMAX;
                }

                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                

                if (curMode == MARK_SIZE.MIN)
                {
                    DrawingGroup.MarkMin = true;
                    DrawingGroup.MarkMax = false;
                }
                else if (curMode == MARK_SIZE.MAX)
                {
                    DrawingGroup.MarkMin = false;
                    DrawingGroup.MarkMax = true;
                }

                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    PMITemplateMiniViewModel vm = new PMITemplateMiniViewModel();

                    vm.SelectedPadTemplateIndex = SelectedPadTemplateIndex;
                    vm.PadTemplateInfoCount = PadTemplateInfoCount;
                    vm.SelectedPadTemplate = SelectedPadTemplate;
                    vm.DrawingGroup = DrawingGroup;

                    this.LoaderRemoteMediator().GetServiceCallBack()?.UpdatgePMITemplateMiniViewModel(vm);
                }
                (PMIModule as IPMIModule).UpdateRenderLayer();

                //UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ProbeMarkSizeSetupModule] [ChangeMarkModeCommand(" + $"{mode}" + $")] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
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
