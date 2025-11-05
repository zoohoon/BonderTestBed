using LogModule;
using MetroDialogInterfaces;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Newtonsoft.Json;
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

    public class PadManualRegistrationModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup, IPMITemplateMiniViewModel
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("9777B59C-FCCB-168F-FE6C-0D378163B7FD");

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

        public PadManualRegistrationModule()
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
                }

                //SelectedPadTemplate = (PadTemplateInfo.Count > 0) ? PadTemplateInfo[_SelectedPadTemplateIndex] : null;

                //if (SelectedPadTemplate != null)
                //{
                //    Geometry g = Geometry.Parse(SelectedPadTemplate.PathData.Value);

                //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                //    {
                //        if (SelectedPadTemplatePath != null)
                //            SelectedPadTemplatePath.Data = g;
                //    });
                //}
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

        //public PMIModuleDevParam PMIDevParam;

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

        private TemplateMiniView TemplateMiniView;

        //private PadTemplateSelectionControlService TemplateSelection;

        //private WaferObject Wafer { get; set; }

        //private ProbeCard ProbeCard { get { return this.StageSupervisor().ProbeCardInfo; } }
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

                    //TemplateSelection = new PadTemplateSelectionControlService(this);

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
                LoggerManager.Debug($"[PadManualRegistrationModule] [InitModule()] : {err}");
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
                LoggerManager.Debug($"[PadManualRegistrationModule] [DeInitModule()] : {err}");
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
                Header = "Pad Manual Registration";

                //CurrentPMITemplate = this.PMIModule().CurrentTempalte();

                //SharpDXLayer = this.PMIModule().GetPMIRenderLayer();

                retVal = InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                //UpdateLabel();

                //this.PMIModule().UpdateRenderLayer();

                _TemplateIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadTemplateIndexCommand);
                _PadIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadIndexCommand);
                _ChangeTemplateSizeCommand = null;// new RelayCommand<object>(FuncChangeTemplateSizeCommand);

                #region ==> 5 Buttons
                OneButton.IconCaption = "ADD";
                TwoButton.IconCaption = "DELETE";
                ThreeButton.IconCaption = "DEL ALL";
                FourButton.IconCaption = "ROTATE";
                FiveButton.IconCaption = "IMPORT";

                OneButton.Command = new AsyncCommand(AddPadCommand);
                TwoButton.Command = new AsyncCommand(DeletePadCommand);
                ThreeButton.Command = new AsyncCommand(DeleteAllPadCommand);
                FourButton.Command = new AsyncCommand(RotatePadTemplateCommand);
                FiveButton.Command = new AsyncCommand(ImportPad);

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/plus-box-outline_W.png");
                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/minus-box-outline_W.png");
                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/collapse-all-outline_W.png");
                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Clockwise_W.png");
                FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/file-import_W.png");

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

                PadJogLeft.CommandParameter = JOG_DIRECTION.LEFT;
                PadJogRight.CommandParameter = JOG_DIRECTION.RIGHT;
                PadJogUp.CommandParameter = JOG_DIRECTION.UP;
                PadJogDown.CommandParameter = JOG_DIRECTION.DOWN;

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

                LoggerManager.Debug($"[PadManualRegistrationModule] [InitViewModel()] : {err}");
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

                //if ((PMIInfo.PadTemplateInfo.Count > 0) && (PMIInfo.SelectedPadTemplate == null))
                //{
                //    PMIInfo.SelectedPadTemplate = PMIInfo.PadTemplateInfo[0];
                //}

                this.PMIModule().SetSubModule(this);

                //this.PMIModule().UpdateRenderLayer();

                UpdateOverlayDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [InitSetup()] : {err}");
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
                LoggerManager.Debug($"[PadManualRegistrationModule] [ParamValidation()] : {err}");
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
        /// Parameter Save 해주는 함수
        /// </summary>
        /// <returns></returns>
        //public EventCodeEnum SaveDevParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        EventCodeEnum retVal1 = this.PMIModule().SaveDevParameter();
        //        EventCodeEnum retVal2 = this.StageSupervisor().SaveWaferObject();

        //        if ((retVal1 == EventCodeEnum.NONE) && (retVal2 == EventCodeEnum.NONE))
        //        {
        //            retVal = EventCodeEnum.NONE;
        //        }
        //        else
        //        {
        //            retVal = EventCodeEnum.UNDEFINED;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadTemplateSetupModule] [SaveDevParameter()] : {err}");
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

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

                LoggerManager.Debug($"[PadManualRegistrationModule] [PageSwitched()] : {err}");
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
                    LoggerManager.Error($"[PadManualRegistrationModule] Parameter (WaferObject) save processing was not performed normally.");
                }



                //foreach (var t in PMIInfo.PadTableTemplateInfo.Select((value, i) => new { i, value }))
                //{
                //    var template = t.value;
                //    var index = t.i;

                //    // 전체 개수는 같아야 함.
                //    if (template.PadEnable.Count != PadInfos.PMIPadInfos.Count)
                //    {
                //        template.PadEnable.Clear();

                //        // 디폴트 값을 True로 => Inspection을 진행하겠다는 뜻
                //        for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                //        {
                //            template.PadEnable.Add(new Element<bool> { Value = true });
                //        }

                //        // Grouping 데이터 초기화
                //        template.GroupingDone.Value = false;
                //        template.Groups.Clear();

                //        // 현재 테이블 인덱스의 Grouping Process를 진행 
                //        retVal = this.PMIModule().PadGroupingMethod(index);

                //        // Grouping이 성공적으로 수행되었을 경우, Group Sequnece 제작
                //        if (retVal == EventCodeEnum.NONE)
                //        {
                //            retVal = this.PMIModule().MakeGroupSequence(index);

                //            // Sequence 제작이 성공적으로 수행되었을 경우, GropingDone 플래그를 True로 만들어 놓음
                //            if (retVal == EventCodeEnum.NONE)
                //            {
                //                PMIInfo.PadTableTemplateInfo[index].GroupingDone.Value = true;
                //            }
                //        }
                //    }
                //}

                //this.PMIModule().UpdateRenderLayer();

                //UpdateLabel();

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PadManualRegistrationModule] [Cleanup()] : Error");
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
                LoggerManager.Debug($"[PadManualRegistrationModule] [Cleanup()] : {err}");
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

        /// <summary>
        /// Excel로 부터 Pad 위치를 가져오는 함수
        /// </summary>
        private List<PMIPadObject> GetDataTableFromCSVFile(string csv_file_path)
        {
            List<PMIPadObject> tmpPadTable = new List<PMIPadObject>();

            double PadSizeX = 0;
            double PadSizeY = 0;

            double xpos = 0;
            double ypos = 0;

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;

                    int Rows = 0;

                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //string line = csvReader.ReadLine();

                        // Pad Size Data column
                        if (Rows == 2)
                        {
                            //Making empty value as null
                            for (int row = 0; row < fieldData.Length; row++)
                            {
                                // Pad Size X
                                if (row == 1)
                                {
                                    PadSizeX = Convert.ToDouble(fieldData[row]);
                                }

                                // Pad Size Y
                                if (row == 2)
                                {
                                    PadSizeY = Convert.ToDouble(fieldData[row]);
                                }
                            }
                        }

                        // Die Size
                        if (Rows == 3)
                        {
                            for (int row = 0; row < fieldData.Length; row++)
                            {
                                if (row == 1)
                                {
                                    TempDieSizeX = Convert.ToDouble(fieldData[row]);
                                }

                                if (row == 2)
                                {
                                    TempDieSizeY = Convert.ToDouble(fieldData[row]);
                                }
                            }
                        }

                        // Pad Datas
                        if (Rows >= 4)
                        {
                            for (int Cols = 0; Cols < fieldData.Length; Cols++)
                            {
                                if (Cols == 1)
                                {
                                    xpos = Convert.ToDouble(fieldData[Cols]);
                                }

                                if (Cols == 2)
                                {
                                    ypos = Convert.ToDouble(fieldData[Cols]);
                                }

                                // Pad Size X
                                if (Cols == 3)
                                {
                                    PadSizeX = Convert.ToDouble(fieldData[Cols]);
                                }

                                // Pad Size Y
                                if (Cols == 4)
                                {
                                    PadSizeY = Convert.ToDouble(fieldData[Cols]);
                                }

                            }

                            PMIPadObject tmpPad = new PMIPadObject();

                            tmpPad.PadSizeX.Value = PadSizeX;
                            tmpPad.PadSizeY.Value = PadSizeY;

                            // WaferCoordinate
                            tmpPad.PadCenter.X.Value = xpos;
                            tmpPad.PadCenter.Y.Value = ypos;

                            int index = SelectedPadTemplateIndex;

                            tmpPad.PMIPadTemplateIndex.Value = index;

                            tmpPadTable.Add(tmpPad);
                        }

                        Rows++;
                    }
                }

                foreach (var pad in tmpPadTable)
                {
                    pad.Index.Value = tmpPadTable.IndexOf(pad) + 1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [GetDataTableFromCSVFile()] : {err}");
                LoggerManager.Exception(err);
            }

            return tmpPadTable;
        }

        /// <summary>
        /// Excel로 부터 Pad 위치를 가져오는 함수
        /// </summary>
        private Task ImportPad()
        {
            try
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();

                if (openFileDialog.ShowDialog() == true)
                {
                    List<PMIPadObject> tmp = GetDataTableFromCSVFile(openFileDialog.FileName);

                    if (tmp != null)
                    {
                        TempPadInfos.Clear();
                        TempPadInfos = tmp;

                        PadInfos.PMIPadInfos = TempPadInfos;
                    }

                    (this.StageSupervisor() as IHasDevParameterizable).SaveDevParameter();

                    this.PMIModule().UpdateRenderLayer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [ImportPad()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;            
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
                    StepLabel = ($"Pad : {DrawingGroup.SelectedPMIPadIndex + 1} / {PadInfos.PMIPadInfos.Count}");
                }
                else
                {
                    StepLabel = ($"Registered pad does not exist.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [UpdateLabel()] : {err}");
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

                this.PMIModule().UpdateRenderLayer();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [ChangePadTemplateIndexCommand()] : {err}");
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
                LoggerManager.Debug($"[PadManualRegistrationModule] [ChangePadIndexCommand()] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }

        ///// <summary>
        ///// 선택된 Pad index를 변경하는 함수
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
        //        LoggerManager.Debug($"[PadManualRegistrationModule] [PadIndexMoveCommand(" + $"{direction}" + $")] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}

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

        /// <summary>
        /// 현재 위치에 패드를 등록하는 함수
        /// </summary>
        private async Task AddPadCommand()
        {
            //.. Todo
            //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
            

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    AddNewPadFunction();
                }
                else
                {
                    if (await CanPadRegistration(EnumMessageDialogTitle.FAILED, "Pad Add"))
                    {

                        AddNewPadFunction();
                    }
                }
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [AddPadCommand()] : {err}");
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


        public void AddNewPadFunction()
        {
            try
            {
                #region ==> Init
                //bool bFound = false;
                int padRegCnt = PadInfos.PMIPadInfos.Count;

                PMIPadObject tmpPad = new PMIPadObject(SelectedPadTemplate);

                WaferCoordinate LLConer;

                double M, N;
                WaferCoordinate PadCenWaferCoord;
                Geometry geo;

                //WaferCoordinate wcoord = null;
                //WaferCoordinate CurDieLeftCorner = new WaferCoordinate();

                //if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                //    wcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                //else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                //    wcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                //Geometry Target;

                //Geometry CombinedSource;
                //Geometry CombinedTarget;

                //PathGeometry SourcePath;
                //PathGeometry TargetPath;

                //ScaleTransform ScaleSource;
                //ScaleTransform ScaleTarget;

                //TranslateTransform TranslateSource;
                //TranslateTransform TranslateTarget;

                //RotateTransform RotateSource = null;
                //RotateTransform RotateTarget = null;

                //TransformGroup TransformGroup;

                IntersectionDetail id;
                bool isintersect = false;
                bool isOverlap = false;

                //PointCollection SourcePoints = null;
                //PointCollection TargetPoints = null;
                #endregion

                Point LayerRatio = this.PMIModule().GetRenderLayerRatio();

                #region ==> Set Pad Datas
                //LLConer = this.WaferAligner().GetLeftCornerPosition(CurCam.CamSystemPos.GetX(), CurCam.CamSystemPos.GetY());

                MachineIndex tmp = CurCam.GetCurCoordMachineIndex();
                LLConer = this.WaferAligner().MachineIndexConvertToDieLeftCorner(tmp.XIndex, tmp.YIndex);

                //Point pt = this.WaferAligner().GetLeftCornerPosition(wcoord);

                //CurDieLeftCorner.X.Value = pt.X;
                //CurDieLeftCorner.Y.Value = pt.Y;

                //int padtemplateindex = PMIInfo.SelectedPadTemplateIndex;

                //PadTemplate padTemplate = PMIInfo.GetPadTemplate(padtemplateindex);


                // Die 경계를 포함하거나, 벗어난 경우, 등록되지 않아야 한다.

                //  --------
                // ㅣ      ㅣ
                // ㅣ      ㅣ
                // ㅣ      ㅣ
                // ㅣ      ㅣ
                //  --------

                double[,] CornerPos = new double[2, 2];
                double[,] PadPos = new double[2, 2];

                //CornerPos[0, 0] = LLConer.X + (Wafer.GetSubsInfo().DieXClearance.Value / 2);
                //CornerPos[0, 1] = LLConer.Y + Wafer.GetSubsInfo().ActualDieSize.Height.Value - (Wafer.GetSubsInfo().DieYClearance.Value / 2);

                //CornerPos[1, 0] = LLConer.X + Wafer.GetSubsInfo().ActualDieSize.Width.Value - (Wafer.GetSubsInfo().DieXClearance.Value / 2);
                //CornerPos[1, 1] = LLConer.Y + (Wafer.GetSubsInfo().DieYClearance.Value / 2);

                CornerPos[0, 0] = LLConer.X.Value;
                CornerPos[0, 1] = LLConer.Y.Value + Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                CornerPos[1, 0] = LLConer.X.Value + Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                CornerPos[1, 1] = LLConer.Y.Value;


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
                    isOverlap = true;
                }

                if (isOverlap == true)
                {
                    return;
                }

                tmpPad.PadCenter.X.Value = CurCam.CamSystemPos.GetX() - LLConer.X.Value;
                tmpPad.PadCenter.Y.Value = CurCam.CamSystemPos.GetY() - LLConer.Y.Value;

                tmpPad.Index.Value = padRegCnt;
                tmpPad.PMIPadTemplateIndex.Value = SelectedPadTemplateIndex;

                #endregion


                #region ==> Check intersect pad
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    geo = Geometry.Parse(tmpPad.PadInfos.PathData.Value);

                    PMIGeometry source = new PMIGeometry(new Point(tmpPad.PadInfos.SizeX.Value, tmpPad.PadInfos.SizeY.Value),
                                     tmpPad.PadInfos.Angle.Value,
                                     geo,
                                     this.PMIModule().GetRenderLayerRatio(),
                                     new Point(SharpDXLayer.LayerSize.Width, SharpDXLayer.LayerSize.Height));

                    PadCenWaferCoord = new WaferCoordinate((tmpPad.PadCenter.X.Value + LLConer.X.Value),
                                                           (tmpPad.PadCenter.Y.Value + LLConer.Y.Value));

                    M = (SharpDXLayer.LayerSize.Width / 2) - ((CurCam.CamSystemPos.X.Value - PadCenWaferCoord.X.Value) * this.PMIModule().GetRenderLayerRatio().X);
                    N = (SharpDXLayer.LayerSize.Height / 2) + ((CurCam.CamSystemPos.Y.Value - PadCenWaferCoord.Y.Value) * this.PMIModule().GetRenderLayerRatio().Y);

                    source.CalcScalePoint();
                    source.CalcOffsetPoint(M, N);
                    source.CalcCenterPoint();
                    source.GetTransformedGeometry(source.ScaleT, source.OffsetT, source.CenterT);

                    foreach (var pad in PadInfos.PMIPadInfos)
                    {
                        geo = Geometry.Parse(pad.PadInfos.PathData.Value);
                        PMIGeometry Target = new PMIGeometry(new Point(pad.PadInfos.SizeX.Value, pad.PadInfos.SizeY.Value),
                                             pad.PadInfos.Angle.Value,
                                             geo,
                                            this.PMIModule().GetRenderLayerRatio(),
                                            new Point(SharpDXLayer.LayerSize.Width, SharpDXLayer.LayerSize.Height));

                        PadCenWaferCoord = new WaferCoordinate((pad.PadCenter.X.Value + LLConer.X.Value),
                                                               (pad.PadCenter.Y.Value + LLConer.Y.Value));

                        M = (SharpDXLayer.LayerSize.Width / 2) - ((CurCam.CamSystemPos.X.Value - PadCenWaferCoord.X.Value) * this.PMIModule().GetRenderLayerRatio().X);
                        N = (SharpDXLayer.LayerSize.Height / 2) + ((CurCam.CamSystemPos.Y.Value - PadCenWaferCoord.Y.Value) * this.PMIModule().GetRenderLayerRatio().Y);

                        Target.CalcScalePoint();
                        Target.CalcOffsetPoint(M, N);
                        Target.CalcCenterPoint();
                        Target.GetTransformedGeometry(Target.ScaleT, Target.OffsetT, Target.CenterT);

                        id = Target.TransfomedGeo.FillContainsWithDetail(source.TransfomedGeo);

                        if (id == IntersectionDetail.Empty)
                        {
                            isintersect = false;
                        }
                        else if (id == IntersectionDetail.NotCalculated)
                        {
                            isintersect = true;
                            LoggerManager.Debug("Intersection Calculate Error.");
                            break;
                        }
                        else
                        {
                            isintersect = true;
                            LoggerManager.Debug("Intersection Pad Detected...");
                            break;
                        }
                    }
                }
                #endregion

                #region ==> Add to PMI Pad List
                if (isintersect == false)
                {
                    PadInfos.PMIPadInfos.Add(tmpPad);

                    foreach (var table in PMIInfo.PadTableTemplateInfo)
                    {
                        table.GroupingDone.Value = false;
                    }

                    DrawingGroup.SelectedPMIPadIndex = PadInfos.PMIPadInfos.Count - 1;
                    //PadInfos.SelectedPMIPadIndex = PadInfos.PMIPadInfos.Count - 1;

                    //// Update PMI pad Table
                    //if (PMIInfo.PadTableTemplateInfo.Count <= 0)
                    //{
                    //    for (int j = 0; j < 6; j++)
                    //    {
                    //        PadTableTemplate TableInfo = new PadTableTemplate();

                    //        for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                    //        {
                    //            TableInfo.Enable.Add(true);
                    //        }

                    //        PMIInfo.PadTableTemplateInfo.Add(TableInfo);
                    //    }
                    //}
                    //else
                    //{
                    //    for (int j = 0; j < 6; j++)
                    //    {
                    //        PMIInfo.PadTableTemplateInfo[j].Enable.Add(true);
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
                    UpdateLabel();
                }
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [AddNewPadFunction()] : {err}");
            }
        }



        /// <summary>
        /// 선택된 현재 패드를 삭제하는 함수
        /// </summary>
        private Task DeletePadCommand()
        {
            try
            {
                var padlist = PadInfos.PMIPadInfos;

                // Move Axis
                if (padlist.Count > 0)
                {
                    //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                    

                    //if (PadInfos.SelectedPMIPadIndex > padlist.Count - 1)
                    //{
                    //    PadInfos.SelectedPMIPadIndex = padlist.Count - 1;
                    //}

                    if (padlist.ElementAtOrDefault(DrawingGroup.SelectedPMIPadIndex) != null)
                    {
                        padlist.RemoveAt(DrawingGroup.SelectedPMIPadIndex);

                        foreach (var table in PMIInfo.PadTableTemplateInfo)
                        {
                            table.GroupingDone.Value = false;
                        }
                    }

                    if (DrawingGroup.SelectedPMIPadIndex == 0)
                    {
                        DrawingGroup.SelectedPMIPadIndex = padlist.Count - 1;
                    }
                    else
                    {
                        DrawingGroup.SelectedPMIPadIndex--;
                    }

                    if (DrawingGroup.SelectedPMIPadIndex > 0)
                    {
                        this.PMIModule().MoveToPad(CurCam, CurCam.GetCurCoordMachineIndex(), DrawingGroup.SelectedPMIPadIndex);
                    }
                    else
                    {
                        LoggerManager.Debug($"PadManualRegistrationModule, DeletePadCommand() Selected pad index = {DrawingGroup.SelectedPMIPadIndex}");
                    }

                    //#region ==> Update PMI pad Table
                    //if (PMIInfo.PadTableTemplateInfo.Count <= 0)
                    //{
                    //    for (int j = 0; j < 6; j++)
                    //    {
                    //        PadTableTemplate TableInfo = new PadTableTemplate();

                    //        for (int i = 0; i < padlist.Count; i++)
                    //        {
                    //            TableInfo.Enable.Add(true);
                    //        }

                    //        PMIInfo.PadTableTemplateInfo.Add(TableInfo);
                    //    }
                    //}
                    //else
                    //{
                    //    for (int j = 0; j < 6; j++)
                    //    {
                    //        PMIInfo.PadTableTemplateInfo[j].Enable.RemoveAt(PadInfos.SelectedPMIPadIndex);
                    //    }
                    //}
                    //#endregion

                    // update index value
                    for (int index = 0; index < padlist.Count(); index++)
                    {
                        padlist[index].Index.Value = index;
                    }


                    this.PMIModule().UpdateCurrentPadIndex();
                    this.PMIModule().UpdateDisplayedDevices(this.CurCam);
                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [DeletePadCommand()] : {err}");
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
                                                                "Pad Delete All",
                                                                "Do you want to delete all pads?"
                                                                + Environment.NewLine + "Ok         : Delete All"
                                                                + Environment.NewLine + "Cancel     : Cancel Exit",
                                                                EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                        

                        PadInfos.PMIPadInfos.Clear();
                        PMIInfo.PadTableTemplateInfo.Clear();
                        //PadInfos.SelectedPMIPadIndex = -1;

                        this.PMIModule().UpdateCurrentPadIndex();
                        this.PMIModule().UpdateDisplayedDevices(this.CurCam);
                        this.PMIModule().UpdateRenderLayer();

                        UpdateLabel();
                    }
                    else if (ret == EnumMessageDialogResult.NEGATIVE)
                    {
                        // cancel
                        //..Do nothing..
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [DeleteAllPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        /// <summary>
        /// 현재 선택된 Template를 시계방향으로 회전시키는 함수
        /// </summary>
        private Task RotatePadTemplateCommand()
        {
            try
            {
                if (SelectedPadTemplate != null)
                {
                    double AngleIncrese = 45.0;
                    double ChangeAngle;

                    if (SelectedPadTemplate.Angle.Value + AngleIncrese >= 360)
                    {
                        ChangeAngle = SelectedPadTemplate.Angle.Value + (AngleIncrese - 360);
                    }
                    else
                    {
                        ChangeAngle = SelectedPadTemplate.Angle.Value + AngleIncrese;
                    }

                    SelectedPadTemplate.Angle.Value = ChangeAngle;

                    this.PMIModule().UpdateRenderLayer();
                }

                //if (TempPadAngle + AngleIncrese >= 360)
                //{
                //    TempPadAngle += (AngleIncrese - 360);
                //}
                //else
                //{
                //    TempPadAngle += AngleIncrese;
                //}

                //this.PMIModule().ChangeTemplateAngleCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [RotatePadTemplateCommand()] : {err}");
                LoggerManager.Exception(err);
            }

            UpdateLabel();

            return Task.CompletedTask;
        }

        /// <summary>
        /// 현재 화면에서 Pad를 찾는 함수
        /// </summary>
        private Task FindPadCommand()
        {
            EventCodeEnum Retval = EventCodeEnum.UNDEFINED;

            try
            {
                Retval = this.PMIModule().FindPad(SelectedPadTemplate);

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
