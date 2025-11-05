using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using PMIModuleParameter;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace PMISetup
{
    using WinSize = System.Windows.Size;

    public class PatternInPadRegistrationModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup, IPMITemplateMiniViewModel
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("C50ADE93-3B82-432E-807B-87C4E28E776A");

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }
        }

        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }
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

        public PatternInPadRegistrationModule()
        {

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

                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[_SelectedPadTemplateIndex];

                    UpdateLabel();
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
                }
            }
        }


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



        private TemplateMiniView TemplateMiniView;


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
                    DrawingGroup = new PMIDrawingGroup();
                    DrawingGroup.Template = true;
                    DrawingGroup.RegisterdPad = true;

                    SharpDXLayer = this.PMIModule().InitPMIRenderLayer(this.PMIModule().GetLayerSize(), 0, 0, 0, 0);
                    SharpDXLayer?.Init();

                    TemplateMiniView = new TemplateMiniView();
                    TemplateMiniView.DataContext = this;

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
                LoggerManager.Debug($"[PatternInPadRegistration] [InitModule()] : {err}");
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
                LoggerManager.Debug($"[PatternInPadRegistration] [DeInitModule()] : {err}");
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
                Header = "Pattern In Pad Registration";

                retVal = InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                _TemplateIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadTemplateIndexCommand);
                _PadIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadIndexCommand);
                _ChangeTemplateSizeCommand = null;// new RelayCommand<object>(FuncChangeTemplateSizeCommand);

                #region ==> 5 Buttons
                OneButton.IconCaption = "ADD";
                TwoButton.IconCaption = "DELETE";
                ThreeButton.IconCaption = "DEL ALL";
                FourButton.IconCaption = "";
                FiveButton.IconCaption = "";


                OneButton.Command = new AsyncCommand(AddPadCommand);
                TwoButton.Command = new AsyncCommand(DeletePadCommand);
                ThreeButton.Command = new AsyncCommand(DeleteAllPadCommand);
                FourButton.Command = null; // new AsyncCommand(RotatePadTemplateCommand);
                FiveButton.Command = null; //new AsyncCommand(ImportPad);

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/plus-box-outline_W.png");
                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/minus-box-outline_W.png");
                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/collapse-all-outline_W.png");
                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Clockwise_W.png");
                FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/file-import_W.png");


                FourButton.IsEnabled = false;
                FiveButton.IsEnabled = false;
                #endregion

                #region ==> Prev/Next Buttons
                PadJogLeftUp.IconCaption = "TEMPLATE";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "TEMPLATE";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogLeftDown.IconCaption = "PAD";
                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightDown.IconCaption = "PAD";
                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeftUp.Command = _TemplateIndexMoveCommand;
                PadJogRightUp.Command = _TemplateIndexMoveCommand;
                PadJogLeftDown.Command = _PadIndexMoveCommand;
                PadJogRightDown.Command = _PadIndexMoveCommand;

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;
                PadJogLeftDown.CaptionSize = 16;
                PadJogRightDown.CaptionSize = 16;

                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;
                PadJogLeftDown.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightDown.CommandParameter = SETUP_DIRECTION.NEXT;
                #endregion

                #region ==> Jog Buttons
                PadJogSelect.IconCaption = "FIND";
                PadJogSelect.Command = new AsyncCommand(FindPadCommand);
                PadJogLeft.Command = _ChangeTemplateSizeCommand;
                PadJogRight.Command = _ChangeTemplateSizeCommand;
                PadJogUp.Command = _ChangeTemplateSizeCommand;
                PadJogDown.Command = _ChangeTemplateSizeCommand;

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



                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search-outline_W.png");
                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
                #endregion

                MiniViewSwapVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Debug($"[PatternInPadRegistration] [InitViewModel()] : {err}");
                LoggerManager.Exception(err);
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

                MainViewTarget = DisplayPort;
                MiniViewTarget = TemplateMiniView;

                UseUserControl = UserControlFucEnum.DEFAULT;

                this.PMIModule().SetSubModule(this);

                UpdateOverlayDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [InitSetup()] : {err}");
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private void UpdateOverlay()
        {
            try
            {
                this.PMIModule().UpdateDisplayedDevices(this.CurCam);
                this.PMIModule().UpdateCurrentPadIndex();
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
                LoggerManager.Debug($"[PatternInPadRegistration] [ParamValidation()] : {err}");
                LoggerManager.Exception(err);
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

                DrawingGroup.PadTableTemplate = null;
                DrawingGroup.SetupMode = PMI_SETUP_MODE.PAD;
                DrawingGroup.SelectedPMIPadIndex = 0;

                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[SelectedPadTemplateIndex];
                }

                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    SelectedPMIPad = PadInfos.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                }

                retVal = await InitSetup();

                UpdateMiniViewModel();

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.StageSupervisor().WaferObject.DrawDieOverlay(CurCam);

                if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
                {
                    await CanPadRegistration(EnumMessageDialogTitle.NOTIFY, "For Pad Add or Rotate...");
                }

                UpdateOverlay();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;

                LoggerManager.Debug($"[PatternInPadRegistration] [PageSwitched()] : {err}");
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

                // Update PadTableTemaplte Information

                retVal = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().UpdatePadTableTemplateInfo();

                if (retVal == EventCodeEnum.NONE)
                {
                    this.PMIModule().UpdateGroupingInformation();
                }
                else
                {
                    LoggerManager.Error($"UpdatePadTableTemplateInfo Error.");
                }

                retVal = this.StageSupervisor().SaveWaferObject();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[PatternInPadRegistration] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PatternInPadRegistration] [Cleanup()] : Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [Cleanup()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region ==> Properties
        private double _TempDieSizeX;
        public double TempDieSizeX
        {
            get { return _TempDieSizeX; }
            set
            {
                if (value != _TempDieSizeX)
                {
                    _TempDieSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TempDieSizeY;
        public double TempDieSizeY
        {
            get { return _TempDieSizeY; }
            set
            {
                if (value != _TempDieSizeY)
                {
                    _TempDieSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TempPadAngle = 0;
        public double TempPadAngle
        {
            get { return _TempPadAngle; }
            set
            {
                if (value != _TempPadAngle)
                {
                    _TempPadAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        const double AngleIncrese = 45;

        private IList<PMIPadObject> _TempPadInfos = new List<PMIPadObject>();
        public virtual IList<PMIPadObject> TempPadInfos
        {
            get { return _TempPadInfos; }
            set
            {
                if (value != _TempPadInfos)
                {
                    _TempPadInfos = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Commands
        private RelayCommand<object> _ChangeTemplateSizeCommand;

        private AsyncCommand<SETUP_DIRECTION> _TemplateIndexMoveCommand;
        private AsyncCommand<SETUP_DIRECTION> _PadIndexMoveCommand;

        private string GetReferenceExistsMessage()
        {
            if (System.IO.File.Exists(GetSelectedPadTemplateFullPath(SelectedPadTemplateIndex)))
            {
                return $"&nbsp;(<span style=\"color: green;\">Reference Added</span>)";
            }

            return $"&nbsp; (<span style=\"color: yellow;\"> Reference Not Set</span>)";

        }

        /// <summary>
        /// Display port에 label update 하는 함수.
        /// </summary>
        public override void UpdateLabel()
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    StepLabel = $"Pad : {DrawingGroup.SelectedPMIPadIndex + 1} / {PadInfos.PMIPadInfos.Count} {GetReferenceExistsMessage()}";
                }
                else
                {
                    StepLabel = $"Registered pad does not exist.";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [UpdateLabel()] : {err}");
                LoggerManager.Exception(err);
            }
        }


        private Task ChangePadTemplateIndexCommand(SETUP_DIRECTION direction)
        {
            try
            {
                // TemplateInfo가 존재할 때
                if (PMIInfo.PadTemplateInfo.Count > 0)
                {

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

                this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [ChangePadTemplateIndexCommand()] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }

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
                LoggerManager.Debug($"[PatternInPadRegistration] [ChangePadIndexCommand()] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }


        private async Task<bool> CanPadRegistration(EnumMessageDialogTitle TitleType, string ExtraStr)
        {
            bool retval = false;
            try
            {

                bool ret1 = false;
                bool ret2 = false;

                string title = string.Empty;

                switch (TitleType)
                {
                    case EnumMessageDialogTitle.UNKNOWN:
                        title = "UNKNOWN";
                        break;
                    case EnumMessageDialogTitle.FAILED:
                        title = "FAILED";
                        break;
                    case EnumMessageDialogTitle.SUCCESS:
                        title = "SUCCESS";
                        break;
                    case EnumMessageDialogTitle.NOTIFY:
                        title = "NOTIFY";
                        break;
                    default:
                        title = "UNKNOWN";
                        break;
                }

                if (this.StageSupervisor.WaferObject.GetAlignState() == AlignStateEnum.DONE)
                {
                    ret1 = true;
                }

                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    ret2 = true;
                }

                if (ret1 == false && ret2 == false)
                {
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog($"{title}-{ExtraStr}", "1. Need to Wafer Alignment Done.\n" +
                                                                                                          "2. Need to Pad Template.",
                                                                                                           EnumMessageStyle.Affirmative);

                }
                else if (ret1 == true && ret2 == false)
                {
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog($"{title}-{ExtraStr}", "1. Need to Pad Template.",
                                                                                            EnumMessageStyle.Affirmative);
                }
                else if (ret1 == false && ret2 == true)
                {
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog($"{title}-{ExtraStr}", "1. Need to Wafer Alignment Done.",
                                                                            EnumMessageStyle.Affirmative);
                }
                else
                {
                    retval = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        private string GetSelectedPadTemplateFullPath(int selectedIndex)
        {
            return PMIModuleKeyWordInfo.PadTemplateReferencePath(this.FileManager(), selectedIndex);
        }

        /// <summary>
        /// 현재 위치에 패드를 등록하는 함수
        /// </summary>
        private async Task AddPadCommand()
        {

            ImageBuffer CurImg = null;
            ImageBuffer CropImg = null;
            CurCam.GetCurImage(out CurImg);
            double ImgCenPosPixelX = (int)(CurCam.GetGrabSizeWidth() / 2) - 1;
            double ImgCenPosPixelY = (int)(CurCam.GetGrabSizeHeight() / 2) - 1;

            int CroppedImageWidth = (int)(SelectedPadTemplate.SizeX.Value / CurCam.GetRatioX());
            int CroppedImageHeight = (int)(SelectedPadTemplate.SizeY.Value / CurCam.GetRatioY());

            double offsetX = ImgCenPosPixelX - (CroppedImageWidth / 2);
            double offsetY = ImgCenPosPixelY - (CroppedImageHeight / 2);
            //string TestSavePathFolder = @"C:\ProberSystem\Test\";
            //string TestSaveCropImage = "CropImage.bmp";

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    CropImg = this.VisionManager().VisionProcessing.Algorithmes.CropImage(CurImg, (int)offsetX, (int)offsetY, CroppedImageWidth, CroppedImageHeight);
                }
                else if (await CanPadRegistration(EnumMessageDialogTitle.FAILED, "Pad Add"))
                {

                    CropImg = this.VisionManager().VisionProcessing.Algorithmes.CropImage(CurImg, (int)offsetX, (int)offsetY, CroppedImageWidth, CroppedImageHeight);

                }

                if (CropImg != null)
                {
                    bool bEnableSaveImage = !System.IO.File.Exists(GetSelectedPadTemplateFullPath(SelectedPadTemplateIndex));

                    if (!bEnableSaveImage)
                    {
                        EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                          "Pattern In Pad Masking Filter",
                          "Reference Pad exists already. Do you want to change?",
                          EnumMessageStyle.AffirmativeAndNegative);
                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            bEnableSaveImage = true;
                        }
                    }

                    if (bEnableSaveImage)
                    {
                        this.VisionManager().SaveImageBuffer(CropImg, GetSelectedPadTemplateFullPath(SelectedPadTemplateIndex), IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                        EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                            "Pattern In Pad Masking Filter",
                            "Reference Pad is saved.",
                            EnumMessageStyle.Affirmative);
                    }
                }
                UpdateLabel();
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [AddPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }


            
        }

        public static bool PointCollectionsOverlap_Fast(PointCollection area1, PointCollection area2)
        {
            for (int i = 0; i < area1.Count; i++)
            {
                for (int j = 0; j < area2.Count; j++)
                {
                    if (lineSegmentsIntersect(area1[i], area1[(i + 1) % area1.Count], area2[j], area2[(j + 1) % area2.Count]))
                    {
                        return true;
                    }
                }
            }

            if (PointCollectionContainsPoint(area1, area2[0]) || PointCollectionContainsPoint(area2, area1[0]))
            {
                return true;
            }

            return false;
        }

        public static bool PointCollectionContainsPoint(PointCollection area, Point point)
        {
            Point start = new Point(-100, -100);
            int intersections = 0;

            for (int i = 0; i < area.Count; i++)
            {
                if (lineSegmentsIntersect(area[i], area[(i + 1) % area.Count], start, point))
                {
                    intersections++;
                }
            }

            return (intersections % 2) == 1;
        }

        private static double determinant(Vector vector1, Vector vector2)
        {
            return vector1.X * vector2.Y - vector1.Y * vector2.X;
        }

        private static bool lineSegmentsIntersect(Point _segment1_Start, Point _segment1_End, Point _segment2_Start, Point _segment2_End)
        {
            double det = determinant(_segment1_End - _segment1_Start, _segment2_Start - _segment2_End);
            double t = determinant(_segment2_Start - _segment1_Start, _segment2_Start - _segment2_End) / det;
            double u = determinant(_segment1_End - _segment1_Start, _segment2_Start - _segment1_Start) / det;
            return (t >= 0) && (u >= 0) && (t <= 1) && (u <= 1);
        }




        /// <summary>
        /// 선택된 현재 패드를 삭제하는 함수
        /// </summary>
        private Task DeletePadCommand()
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    if (System.IO.File.Exists(GetSelectedPadTemplateFullPath(SelectedPadTemplateIndex)))
                    {
                        System.IO.File.Delete(GetSelectedPadTemplateFullPath(SelectedPadTemplateIndex));
                        UpdateLabel();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [DeletePadCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;            
        }

        /// <summary>
        /// 등록된 모든 패드를 삭제하는 함수
        /// </summary>
        private async Task DeleteAllPadCommand()
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {

                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                                                "Pad Reference Image Delete All",
                                                                "Do you want to delete all pad reference images?"
                                                                + Environment.NewLine + "Ok         : Delete All"
                                                                + Environment.NewLine + "Cancel     : Cancel Exit",
                                                                EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        foreach (int idx in Enumerable.Range(0, PadInfos.PMIPadInfos.Count))
                        {
                            if (System.IO.File.Exists(GetSelectedPadTemplateFullPath(idx)))
                            {
                                System.IO.File.Delete(GetSelectedPadTemplateFullPath(idx));
                            }
                        }

                        UpdateLabel();
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PatternInPadRegistration] [DeleteAllPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }


        /// <summary>
        /// 현재 화면에서 Pad를 찾는 함수
        /// </summary>
        private Task FindPadCommand()
        {
            try
            {
                this.PMIModule().FindPad(SelectedPadTemplate);

                UpdateMiniViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        #endregion

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
    }
}
