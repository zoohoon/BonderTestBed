using LogModule;
using Microsoft.Win32;
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
using SerializerUtil;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace PMISetup
{
    using WinSize = System.Windows.Size;
    public class PadTemplateSetupModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup, IPackagable, IPMITemplateMiniViewModel, IHasAdvancedSetup
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("EAE0AD9C-EB31-72B1-BC97-BA31F2161F38");

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

        public PadTemplateSetupModule()
        {

        }

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

                    UpdateMiniViewModel();
                }
            }
        }

        //public PMIModuleDevParam PMIDevParam;

        private PadTemplateSelectionControlService TemplateSelection;

        private TemplateMiniView TemplateMiniView;

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
                LoggerManager.Debug($"[PadTemplateSetupModule] [DoExecute()] : {err}");
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [DoClearData()] : {err}");
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [DoRecovery()] : {err}");
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [DoExitRecovery()] : {err}");
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [DoRecovery()] : {err}");
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
                    //PMIModule = this.PMIModule();

                    //InitLightJog(this);

                    //Wafer = this.StageSupervisor().WaferObject as WaferObject;

                    //PMIDevParam = ((PMIModule as IHasDevParameterizable).DevParam) as PMIModuleDevParam;

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
                LoggerManager.Debug($"[PadTemplateSetupModule] [InitModule()] : {err}");
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [DeInitModule()] : {err}");
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
                Header = "Pad Template Setup";

                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);

                TemplateSelection = new PadTemplateSelectionControlService();

                AdvanceSetupView = TemplateSelection.DialogControl;
                AdvanceSetupViewModel = TemplateSelection;

                //SetPackagableParams();
                //AdvanceSetupViewModel.SetParameters(PackagableParams);

                DrawingGroup = new PMIDrawingGroup();
                DrawingGroup.Template = true;

                SharpDXLayer = this.PMIModule().InitPMIRenderLayer(this.PMIModule().GetLayerSize(), 0, 0, 0, 0);
                SharpDXLayer?.Init();

                TemplateMiniView = new TemplateMiniView();
                TemplateMiniView.DataContext = this;


                SetNodeSetupState(EnumMoudleSetupState.NONE);


                //SetPackagableParams();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Debug($"[PadTemplateSetupModule] [InitViewModel()] : {err}");
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

                retVal = InitPNPSetupUI();
                InitLightJog(this);

                retVal = this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                MainViewTarget = DisplayPort;

                MiniViewTarget = TemplateMiniView;

                MiniViewSwapVisibility = Visibility.Hidden;

                UseUserControl = UserControlFucEnum.DEFAULT;

                //TargetRectangleWidth = 128;
                //TargetRectangleHeight = 128;


                this.PMIModule().SetSubModule(this);

                UpdateOverlayDelegate(DELEGATEONOFF.ON);

                this.PMIModule().UpdateRenderLayer();


                //UpdateLabel();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [InitSetup()] : {err}");
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Pad Template Setup";

                //InitPnpModuleStage();

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                _ChangeTemplateIndexCommand = new AsyncCommand<SETUP_DIRECTION>(ChangeTemplateIndexCommand);
                _ChangeTemplateSizeCommand = new RelayCommand<JOG_DIRECTION>(ChangeTemplateSizeCommand);

                _ChangeTemplateOffsetCommand = new RelayCommand<SETUP_DIRECTION>(ChangeTemplateOffsetCommand);
                _ChangeTemplateCornerRadiusCommand = new RelayCommand<SETUP_DIRECTION>(ChangeTemplateCornerRadiusCommand);

                #region ==> 5 Buttons
                //OneButton.IsAdvanceSetupButton = true;
                //OneButton.IconCaption = "ADD";
                TwoButton.IconCaption = "DELETE";
                ThreeButton.IconCaption = "ROTATE";
                FourButton.IconCaption = "COLOR";
                //FiveButton.IconCaption = "CUSTOM";

                //OneButton.Command = new AsyncCommand(AddTemplateCommand);
                TwoButton.Command = new AsyncCommand(DeleteTemplateCommand);
                ThreeButton.Command = new AsyncCommand(RotatePadTemplateCommand);
                FourButton.Command = new AsyncCommand(SetPadColorCommand);
                //FiveButton.Command = new AsyncCommand(CustomTemplateSetupCommand);

                //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rectangle-plus.png");
                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/rectangle-remove.png");
                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/Clockwise_W.png");
                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/invert-colors.png");
                //FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/vector-polygon.png");
                FiveButton.Caption = null;
                FiveButton.IsEnabled = false;
                #endregion

                #region ==> Prev/Next Buttons
                PadJogLeftUp.IconCaption = "TEMPLATE";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "TEMPLATE";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeftUp.Command = _ChangeTemplateIndexCommand;
                PadJogRightUp.Command = _ChangeTemplateIndexCommand;

                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;

                PadJogLeftDown.Caption = "-";
                PadJogRightDown.Caption = "+";
                PadJogLeftDown.CaptionSize = 34;
                PadJogRightDown.CaptionSize = 34;

                PadJogLeftDown.Command = _ChangeTemplateOffsetCommand;
                PadJogRightDown.Command = _ChangeTemplateOffsetCommand;

                PadJogLeftDown.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightDown.CommandParameter = SETUP_DIRECTION.NEXT;

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = true;

                PadJogLeftDown.RepeatEnable = false;
                PadJogRightDown.RepeatEnable = false;
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

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/feature-search-outline_W.png");
                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PlusWhite.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/MinusWhite.png");
                #endregion

                //AdvanceSetupView = TemplateSelection.DialogControl;
                //AdvanceSetupViewModel = TemplateSelection;
                OneButton.Command = null;
                AdvanceSetupUISetting(OneButton, "ADD", "pack://application:,,,/ImageResourcePack;component/Images/rectangle-plus.png");

                MiniViewSwapVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                //TemplateMiniView = new TemplateMiniView();
                //TemplateMiniView.DataContext = this;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }

        public override void SetPackagableParams()
        {
            try
            {
                ObservableCollection<PMITemplatePack> param = (this.PMIModule().PMIModuleSysParam_IParam as PMIModuleSysParam).TemplatePacklist;

                PackagableParams.Clear();
                //PackagableParams.Add(SerializeManager.SerializeToByte(param));
                PackagableParams.Add(SerializeManager.ObjectToByte(param));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
        //        LoggerManager.Debug($"[PadTemplateSetupModule] [InitRecovery()] : {err}");
        //    }
        //    return Task.FromResult<EventCodeEnum>(retVal);
        //}

        //        /// <summary>
        //        /// PNP 화면 버튼 등을 정의.
        //        /// </summary>
        //        /// <returns></returns>
        //        private EventCodeEnum InitPnpUI()
        //        {
        //            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //            try
        //            {
        //                SideViewTargetVisibility = Visibility.Visible;
        //                SideViewDisplayMode = SideViewMode.EXPANDER_MODE;
        //                SideViewExpanderVisibility = Visibility.Visible;
        //                SideViewTextVisibility = Visibility.Hidden;
        //                ExpanderItem_01.HeaderFontSize = 10;
        //                ExpanderItem_01.DescriptionFontSize = 12;
        //                ExpanderItem_02.HeaderFontSize = 10;
        //                ExpanderItem_02.DescriptionFontSize = 14;
        //                ExpanderItem_03.HeaderFontSize = 10;
        //                ExpanderItem_03.DescriptionFontSize = 16;
        //                ExpanderItem_04.HeaderFontSize = 10;
        //                ExpanderItem_04.DescriptionFontSize = 18;
        //                ExpanderItem_05.HeaderFontSize = 10;
        //                ExpanderItem_05.DescriptionFontSize = 20;

        //                ExpanderItem_01.Header = "TWICE";
        //                ExpanderItem_01.Description = @"채영
        //나연
        //정연
        //모모
        //사나
        //지효
        //미나
        //다현
        //쯔위";
        //                ExpanderItem_01.HeaderColor = Brushes.Red;

        //                ExpanderItem_02.Header = "Hi! I'm Header 2!";
        //                ExpanderItem_02.Description = "I don't know why you expand this... Please close this immediately.";
        //                ExpanderItem_02.HeaderColor = Brushes.Green;

        //                ExpanderItem_03.Header = "Hi! I'm Header 3!";
        //                ExpanderItem_03.Description = "I don't know why you expand this... Please close this immediately.";

        //                ExpanderItem_04.Header = "4";
        //                ExpanderItem_04.Description = "44444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444";
        //                ExpanderItem_05.Header = "5";
        //                ExpanderItem_05.Description = "55555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555555";
        //                ExpanderItem_06.ExpanderVisibility = Visibility.Visible;
        //                ExpanderItem_07.ExpanderVisibility = Visibility.Visible;
        //                ExpanderItem_08.ExpanderVisibility = Visibility.Visible;
        //                ExpanderItem_09.ExpanderVisibility = Visibility.Visible;
        //                ExpanderItem_10.Header = "10";
        //                ExpanderItem_10.Description = "here is 10";

        //                SideViewTextBlock.SideTextContents = @"안녕 나야 거기 잘 지내니 이제 그만 집에가자";
        //            }
        //            catch (Exception err)
        //            {
        //                LoggerManager.Debug($"[PadTemplateSetupModule] [InitPnpUI()] : {err}");
        //                LoggerManager.Exception(err);
        //            }

        //            return retVal;
        //        }
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [ParamValidation()] : {err}");
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

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;
                foreach (var param in datas)
                {
                    object target = SerializeManager.ByteToObject(param);
                    if (target != null)
                    {
                        (this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).PMIInfo = (PMIInfo)target;

                        PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                        break;
                    }
                }
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
                LoggerManager.Debug($"[PadTemplateSetupModule] [SaveDevParameter()] : {err}");
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
                DrawingGroup.SetupMode = PMI_SETUP_MODE.PAD;
                DrawingGroup.SelectedPMIPadIndex = 0;

                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                if (PMIInfo.PadTemplateInfo.Count > 0)
                {
                    SelectedPadTemplate = PMIInfo.PadTemplateInfo[SelectedPadTemplateIndex];
                }

                //PackagableParams.Clear();
                //PackagableParams.Add(SerializeManager.ObjectToByte((this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo())));
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                retVal = await InitSetup();

                UpdateMiniViewModel();

                this.PMIModule().UpdateRenderLayer();

                ChangeButtonEnable();

                this.StageSupervisor().WaferObject.DrawDieOverlay(CurCam);

                //PMIInfo.SetupMode = PMI_SETUP_MODE.PAD;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;

                LoggerManager.Debug($"[PadTemplateSetupModule] [PageSwitched()] : {err}");
                LoggerManager.Exception(err);
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

                this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);

                retVal = this.StageSupervisor().SaveWaferObject();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[PadTemplateSetupModule] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PadTemplateSetupModule] [Cleanup()] : Error");
                }
                //PMIInfo.SetupMode = PMI_SETUP_MODE.UNDEFINED;                   

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [Cleanup()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region ==> Properties
        const double AngleIncrese = 90;

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
        #endregion

        #region ==> Commands
        private AsyncCommand<SETUP_DIRECTION> _ChangeTemplateIndexCommand;
        private RelayCommand<SETUP_DIRECTION> _ChangeTemplateOffsetCommand;
        private RelayCommand<SETUP_DIRECTION> _ChangeTemplateCornerRadiusCommand;

        private RelayCommand<JOG_DIRECTION> _ChangeTemplateSizeCommand;

        /// <summary>
        /// pad 파일 불러오는 함수 임시..
        /// </summary>
        private void FuncFileLoadCommand()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = ".bmp";
                dlg.Filter = "bmp files(*.bmp)|*.bmp|All files(*.*)|*.*";

                var rel = dlg.ShowDialog();

                if (rel == true)
                {
                    List<ImageBuffer> imgs = new List<ImageBuffer>();

                    imgs.Add(this.VisionManager().LoadImageFile(dlg.FileName));

                    this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                    this.VisionManager().DigitizerService[CurCam.GetDigitizerIndex()].GrabberService.LoadUserImageFiles(imgs);

                    //if (imgs.Count > 0)
                    //{
                    //    LoadedImage = imgs[0];
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [FuncFileLoadCommand()] : {err}");
            }
        }

        /// <summary>
        /// Display port에 label update 하는 함수.
        /// </summary>
        public override void UpdateLabel()
        {
            try
            {
                //StepLabel = "";

                //if (PMIInfo.SelectedPadTemplate != null)
                //{
                //    var PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                //    //StepLabel = String.Format("({0}/{1}) {2}, X : {3}, Y : {4}, deg : {5}, {6}", 
                //    //    (PMIInfo.GetSelectedPadTemplateIndex() + 1).ToString(),
                //    //    (PMIInfo.GetPadTemplateInfo().Count).ToString(),
                //    //    CurTemplate.ShapeName.Value,
                //    //    (CurTemplate.SizeX.Value).ToString(),
                //    //    (CurTemplate.SizeY.Value).ToString(),
                //    //    (CurTemplate.Angle.Value).ToString(),
                //    //    ((CurTemplate.Color.Value == (int)PAD_COLOR.WHITE) ? "White" : "Black"));

                //    if (PMIInfo.SelectedPadTemplate.JudgingWindowMode.Value == PAD_JUDGING_WINDOW_MODE.TWOWAY)
                //    {
                //        StepLabel = $"X : {PMIInfo.SelectedPadTemplate.SizeX.Value}, Y : {PMIInfo.SelectedPadTemplate.SizeY.Value}, Angle : {PMIInfo.SelectedPadTemplate.Angle.Value}";
                //    }
                //    else
                //    {
                //        StepLabel = $"X : {PMIInfo.SelectedPadTemplate.SizeXY.Value}, Y : {PMIInfo.SelectedPadTemplate.SizeXY.Value}, Angle : {PMIInfo.SelectedPadTemplate.Angle.Value}";
                //    }
                //}
                //else
                //{
                //    StepLabel = "";
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [UpdateLabel()] : {err}");
            }
        }

        /// <summary>
        /// 새로운 PMI pad Template를 추가하는 함수
        /// </summary>
        //private async Task AddTemplateCommand()
        //{
        //    int shape = 0;

        //    //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
        //    

        //    try
        //    {
        //        //PackagableParams.Clear();
        //        //PackagableParams.Add(SerializeManager.SerializeToByte((this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo())));

        //        await TemplateSelection.ShowDialogControl();

        //        UpdateLabel();
        //        ChangeButtonEnable();
        //        this.PMIModule().UpdateRenderLayer();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadTemplateSetupModule] [AddTemplateCommand()] : {err}");
        //    }

        //    
        //}

        /// <summary>
        /// 선택된 Template를 삭제하는 함수
        /// </summary>
        private Task DeleteTemplateCommand()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");


            try
            {

                if (PMIInfo.PadTemplateInfo.ElementAtOrDefault(SelectedPadTemplateIndex) != null)
                {
                    PMIInfo.PadTemplateInfo.RemoveAt(SelectedPadTemplateIndex);

                    if (PMIInfo.PadTemplateInfo.Count > 0)
                    {
                        if (SelectedPadTemplateIndex == 0)
                        {
                            if (PMIInfo.PadTemplateInfo.Count == 1)
                            {
                                SelectedPadTemplate = PMIInfo.PadTemplateInfo[_SelectedPadTemplateIndex];
                            }
                            else
                            {
                                SelectedPadTemplateIndex = PMIInfo.PadTemplateInfo.Count - 1;
                            }
                        }
                        else
                        {
                            SelectedPadTemplateIndex--;
                        }
                    }
                }

                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                this.PMIModule().UpdateRenderLayer();

                //this.PMIModule().DeleteTemplateCommand();

                ChangeButtonEnable();

                //UpdateLabel();                   
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [DeleteTemplateCommand()] : {err}");
            }

            return Task.CompletedTask;
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

                    this.PMIModule().UpdateRenderLayer();
                }
                //UpdateLabel();                   
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [ChangeTemplateIndexCommand()] : {err}");
            }
            finally
            {

            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Octagon, Half-Octagon Shape에서 offset 값을 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        public void ChangeTemplateOffsetCommand(SETUP_DIRECTION direction)
        {
            try
            {
                double TemplateOffsetScalingUnit = 0.01;

                if (SelectedPadTemplate != null)
                {
                    string val1 = "0";
                    string val2 = "1";

                    switch (direction)
                    {
                        case SETUP_DIRECTION.NEXT:

                            if (SelectedPadTemplate.TemplateOffset.Value + TemplateOffsetScalingUnit <= 0.5)
                            {
                                SelectedPadTemplate.TemplateOffset.Value = Math.Round(SelectedPadTemplate.TemplateOffset.Value + TemplateOffsetScalingUnit, 2);
                            }

                            break;
                        case SETUP_DIRECTION.PREV:

                            if (SelectedPadTemplate.TemplateOffset.Value - TemplateOffsetScalingUnit >= 0)
                            {
                                SelectedPadTemplate.TemplateOffset.Value = Math.Round(SelectedPadTemplate.TemplateOffset.Value - TemplateOffsetScalingUnit, 2);
                            }

                            break;
                        default:
                            break;
                    }

                    // modify path data
                    val1 = (SelectedPadTemplate.TemplateOffset.Value).ToString();
                    val2 = (1 - SelectedPadTemplate.TemplateOffset.Value).ToString();

                    if (SelectedPadTemplate.Shape.Value == PAD_SHAPE.OCTAGON)
                    {
                        SelectedPadTemplate.PathData.Value = "M0," + val1 + " L0," + val2 + " " + val1 + ",1 " + val2 + ",1 1," + val2 + " 1," + val1 + " " + val2 + ",0 " + val1 + ",0 z";
                    }
                    else if (SelectedPadTemplate.Shape.Value == PAD_SHAPE.HALF_OCTAGON)
                    {
                        SelectedPadTemplate.PathData.Value = "M0," + val1 + " L0,1 1,1 1," + val1 + " " + val2 + ",0 " + val1 + ",0 z";
                    }

                    SelectedPadTemplate.UpdateArea();

                    UpdateMiniViewModel();

                    this.PMIModule().UpdateRenderLayer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [ChangeTemplateOffsetCommand()] : {err}");
            }
        }

        public void ChangeTemplateCornerRadiusCommand(SETUP_DIRECTION direction)
        {
            try
            {
                double cornerRadiusScalingUnit = 0.01;  // This value can be adjusted to change the corner radius more or less aggressively.

                if (SelectedPadTemplate != null && SelectedPadTemplate.Shape.Value == PAD_SHAPE.ROUNDED_RECTANGLE)
                {
                    switch (direction)
                    {
                        case SETUP_DIRECTION.NEXT:
                            // Increase the corner radius but ensure it does not exceed half the width or height of the shape, assuming square bounding box for simplicity.
                            if (SelectedPadTemplate.TemplateCornerRadius.Value + cornerRadiusScalingUnit <= 0.5)
                            {
                                SelectedPadTemplate.TemplateCornerRadius.Value = Math.Round(SelectedPadTemplate.TemplateCornerRadius.Value + cornerRadiusScalingUnit, 2);
                            }
                            break;
                        case SETUP_DIRECTION.PREV:
                            // Decrease the corner radius but ensure it does not go below zero.
                            if (SelectedPadTemplate.TemplateCornerRadius.Value - cornerRadiusScalingUnit >= 0)
                            {
                                SelectedPadTemplate.TemplateCornerRadius.Value = Math.Round(SelectedPadTemplate.TemplateCornerRadius.Value - cornerRadiusScalingUnit, 2);
                            }
                            break;
                        default:
                            break;
                    }

                    // Recalculate the path for the rounded rectangle based on the new corner radius
                    double cornerRadius = SelectedPadTemplate.TemplateCornerRadius.Value;

                    string path = $"M" + cornerRadius + ",0" + " H" + (1 - cornerRadius) + " Q1,0 1," + cornerRadius + " V" + (1 - cornerRadius) + " Q1,1 " + (1 - cornerRadius) + ",1" + " H" + cornerRadius + " Q0,1 0," + (1 - cornerRadius) + " V" + cornerRadius + " Q0,0 " + cornerRadius + ",0" + " Z";

                    SelectedPadTemplate.PathData.Value = path;

                    // Recalculate area or other properties if necessary
                    SelectedPadTemplate.UpdateArea();

                    // Assuming there are methods to update the view or model state based on changes
                    UpdateMiniViewModel();
                    this.PMIModule().UpdateRenderLayer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTemplateSetupModule] [ChangeTemplateCornerRadiusCommand()] : {err}");
            }
        }

        /// <summary>
        /// 선택된 Template의 패드 색상을 변경하는 함수
        /// </summary>
        private Task SetPadColorCommand()
        {
            try
            {

                if (SelectedPadTemplate != null)
                {
                    if (SelectedPadTemplate.Color.Value == (int)PAD_COLOR.WHITE)
                    {
                        SelectedPadTemplate.Color.Value = (int)PAD_COLOR.BLACK;
                    }
                    else
                    {
                        SelectedPadTemplate.Color.Value = (int)PAD_COLOR.WHITE;
                    }
                }

                UpdateMiniViewModel();

                this.PMIModule().UpdateRenderLayer();
                //UpdateLabel();                   

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 선택된 Template의 X, Y 사이즈를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        private void ChangeTemplateSizeCommand(JOG_DIRECTION direction)
        {
            try
            {
                double TemplateSizeScalingUnit = 1;

                if (SelectedPadTemplate != null)
                {
                    var SizeX = (CurCam.GetGrabSizeWidth() * CurCam.GetRatioX());
                    var SizeY = (CurCam.GetGrabSizeHeight() * CurCam.GetRatioY());

                    if (SelectedPadTemplate.JudgingWindowMode.Value == PAD_JUDGING_WINDOW_MODE.TWOWAY)
                    {
                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:
                                if (SelectedPadTemplate.SizeY.Value + TemplateSizeScalingUnit < SizeY)
                                {
                                    SelectedPadTemplate.SizeY.Value += TemplateSizeScalingUnit;
                                }
                                break;
                            case JOG_DIRECTION.DOWN:
                                if (SelectedPadTemplate.SizeY.Value - TemplateSizeScalingUnit > 0)
                                {
                                    SelectedPadTemplate.SizeY.Value -= TemplateSizeScalingUnit;
                                }
                                break;
                            case JOG_DIRECTION.LEFT:
                                if (SelectedPadTemplate.SizeX.Value - TemplateSizeScalingUnit > 0)
                                {
                                    SelectedPadTemplate.SizeX.Value -= TemplateSizeScalingUnit;
                                }
                                break;
                            case JOG_DIRECTION.RIGHT:
                                if (SelectedPadTemplate.SizeX.Value + TemplateSizeScalingUnit < SizeX)
                                {
                                    SelectedPadTemplate.SizeX.Value += TemplateSizeScalingUnit;
                                }
                                break;
                            default:
                                break;
                        }

                        // Angle (0 ~ 90) or (180 ~ 270)
                        //if (((SelectedPadTemplate.Angle.Value >= 0) && (SelectedPadTemplate.Angle.Value < 90)) ||
                        //     (SelectedPadTemplate.Angle.Value >= 180) && (SelectedPadTemplate.Angle.Value < 270))
                        //{
                        //    switch (direction)
                        //    {
                        //        case JOG_DIRECTION.UP:
                        //            if (SelectedPadTemplate.SizeY.Value + TemplateSizeScalingUnit < SizeY)
                        //            {
                        //                SelectedPadTemplate.SizeY.Value += TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        case JOG_DIRECTION.DOWN:
                        //            if (SelectedPadTemplate.SizeY.Value - TemplateSizeScalingUnit > 0)
                        //            {
                        //                SelectedPadTemplate.SizeY.Value -= TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        case JOG_DIRECTION.LEFT:
                        //            if (SelectedPadTemplate.SizeX.Value - TemplateSizeScalingUnit > 0)
                        //            {
                        //                SelectedPadTemplate.SizeX.Value -= TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        case JOG_DIRECTION.RIGHT:
                        //            if (SelectedPadTemplate.SizeX.Value + TemplateSizeScalingUnit < SizeX)
                        //            {
                        //                SelectedPadTemplate.SizeX.Value += TemplateSizeScalingUnit;
                        //            }
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
                        //            if (SelectedPadTemplate.SizeX.Value + TemplateSizeScalingUnit < SizeX)
                        //            {
                        //                SelectedPadTemplate.SizeX.Value += TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        case JOG_DIRECTION.DOWN:
                        //            if (SelectedPadTemplate.SizeX.Value - TemplateSizeScalingUnit > 0)
                        //            {
                        //                SelectedPadTemplate.SizeX.Value -= TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        case JOG_DIRECTION.LEFT:
                        //            if (SelectedPadTemplate.SizeY.Value - TemplateSizeScalingUnit > 0)
                        //            {
                        //                SelectedPadTemplate.SizeY.Value -= TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        case JOG_DIRECTION.RIGHT:
                        //            if (SelectedPadTemplate.SizeY.Value + TemplateSizeScalingUnit < SizeY)
                        //            {
                        //                SelectedPadTemplate.SizeY.Value += TemplateSizeScalingUnit;
                        //            }
                        //            break;
                        //        default:
                        //            break;
                        //    }
                        //}
                    }
                    else
                    {
                        switch (direction)
                        {
                            case JOG_DIRECTION.UP:

                                if (SelectedPadTemplate.SizeY.Value + TemplateSizeScalingUnit < SizeY)
                                {
                                    SelectedPadTemplate.SizeY.Value += TemplateSizeScalingUnit;
                                }

                                break;
                            case JOG_DIRECTION.DOWN:

                                if (SelectedPadTemplate.SizeY.Value - TemplateSizeScalingUnit > 0)
                                {
                                    SelectedPadTemplate.SizeY.Value -= TemplateSizeScalingUnit;
                                }

                                break;
                            case JOG_DIRECTION.LEFT:

                                if (SelectedPadTemplate.SizeX.Value - TemplateSizeScalingUnit > 0)
                                {
                                    SelectedPadTemplate.SizeX.Value -= TemplateSizeScalingUnit;
                                }

                                break;
                            case JOG_DIRECTION.RIGHT:

                                if (SelectedPadTemplate.SizeX.Value + TemplateSizeScalingUnit < SizeX)
                                {
                                    SelectedPadTemplate.SizeX.Value += TemplateSizeScalingUnit;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    SelectedPadTemplate.UpdateArea();
                    //PMIInfo.PMIInfoUpdatedToLoader();

                    UpdateMiniViewModel();

                    this.PMIModule().UpdateRenderLayer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [ChangeTemplateSizeCommand()] : {err}");
                LoggerManager.Exception(err);
            }

            //try
            //{
            //    //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");

            //    JOG_DIRECTION dir = (JOG_DIRECTION)direction;

            //    this.PMIModule().ChangeTemplateSizeCommand(dir);

            //    //UpdateLabel();
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}
            //finally
            //{
            //    this.ViewModelManager().UnLock(this.GetHashCode());
            //}
        }

        /// <summary>
        /// 선택된 Template의 각도를 변경하는 함수.
        /// 기본값 시계방향으로 90도.
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

                    UpdateMiniViewModel();

                    this.PMIModule().UpdateRenderLayer();
                }

                ////.. Todo
                //if (TempPadAngle + AngleIncrese >= 360)
                //{
                //    TempPadAngle += (AngleIncrese - 360);
                //}
                //else
                //{
                //    TempPadAngle += AngleIncrese;
                //}

                //double CurrentAngle = PMIInfo.SelectedPadTemplate.Angle.Value;
                //double ChangeAngle;

                //if(CurrentAngle + AngleIncrese >= 360)
                //{
                //    ChangeAngle = CurrentAngle + (AngleIncrese - 360);
                //}
                //else
                //{
                //    ChangeAngle = CurrentAngle + AngleIncrese;
                //}

                //this.PMIModule().ChangeTemplateAngleCommand();

                //UpdateLabel();                   
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 현재 화면에서 패드를 찾는 함수
        /// </summary>
        private Task FindPadCommand()
        {
            EventCodeEnum Retval = EventCodeEnum.UNDEFINED;

            try
            {

                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");


                Retval = this.PMIModule().FindPad(SelectedPadTemplate);

                UpdateMiniViewModel();
                //UpdateLabel();                   

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 사용자 지정 Template을 만드는 함수. 냉무
        /// </summary>
        public async Task CustomTemplateSetupCommand()
        {
            try
            {
                Guid guid = new Guid("717bad3b-49a1-46d5-b0ec-dfabee758708");

                await this.ViewModelManager().ViewTransitionAsync(guid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 현재 선택된 Template에 따라 Button의 Enable을 변경하는 함수
        /// </summary>
        public void ChangeButtonEnable()
        {
            try
            {
                if (PMIInfo.PadTemplateInfo.Count > 1)
                {
                    TwoButton.IsEnabled = true;
                }
                else
                {
                    TwoButton.IsEnabled = false;
                }

                if (SelectedPadTemplate.EdgeOffsetMode.Value == PAD_EDGE_OFFSET_MODE.ENABLE ||
                    SelectedPadTemplate.CornerRadiusMode.Value == PAD_CORNERRADIUS_MODE.ENABLE)
                {
                    PadJogLeftDown.IsEnabled = true;
                    PadJogRightDown.IsEnabled = true;

                    if (SelectedPadTemplate.EdgeOffsetMode.Value == PAD_EDGE_OFFSET_MODE.ENABLE)
                    {
                        PadJogLeftDown.Command = _ChangeTemplateOffsetCommand;
                        PadJogRightDown.Command = _ChangeTemplateOffsetCommand;
                    }

                    if (SelectedPadTemplate.CornerRadiusMode.Value == PAD_CORNERRADIUS_MODE.ENABLE)
                    {
                        PadJogLeftDown.Command = _ChangeTemplateCornerRadiusCommand;
                        PadJogRightDown.Command = _ChangeTemplateCornerRadiusCommand;
                    }
                }
                else
                {
                    PadJogLeftDown.IsEnabled = false;
                    PadJogRightDown.IsEnabled = false;
                }

                //if (PMIInfo.SelectedPadTemplate.JudgingWindowMode.Value == PAD_JUDGING_WINDOW_MODE.TWOWAY)
                //{
                //    PadJogUp.IsEnabled = true;
                //    PadJogDown.IsEnabled = true;
                //}
                //else
                //{
                //    PadJogUp.IsEnabled = false;
                //    PadJogDown.IsEnabled = false;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public new void CloseAdvanceSetupView()
        {
            try
            {
                // 닫힐 때, 템플릿이 추가되었을 수 있음. 전체 개수 업데이트 해줘야 함.
                PadTemplateInfoCount = PMIInfo.PadTemplateInfo.Count;

                ChangeButtonEnable();

                UpdateMiniViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
