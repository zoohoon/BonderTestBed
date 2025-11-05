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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VirtualKeyboardControl;

namespace PMISetup
{
    using WinSize = System.Windows.Size;
    public class PadTableSetupModule : PNPSetupBase, INotifyPropertyChanged, ISetup, IHasPMIDrawingGroup
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("EF7414A4-1663-7B72-F039-8E0853B32DD1");

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }
        }

        public PadTableSetupModule()
        {

        }

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

        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }
        }

        private int _SelectedPadTableTemplateIndex;
        public int SelectedPadTableTemplateIndex
        {
            get { return _SelectedPadTableTemplateIndex; }
            set
            {
                if (value != _SelectedPadTableTemplateIndex)
                {
                    _SelectedPadTableTemplateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PadTableTemplate _SelectedPadTableTemplate;
        public PadTableTemplate SelectedPadTableTemplate
        {
            get { return _SelectedPadTableTemplate; }
            set
            {
                if (value != _SelectedPadTableTemplate)
                {
                    _SelectedPadTableTemplate = value;
                    RaisePropertyChanged();

                    DrawingGroup.PadTableTemplate = _SelectedPadTableTemplate;
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
                LoggerManager.Debug($"[PadTableSetupModule] [InitModule()] : {err}");
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
                LoggerManager.Debug($"[PadTableSetupModule] [DeInitModule()] : {err}");
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
                Header = "Pad Table Setup";

                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);

                SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Debug($"[PadTableSetupModule] [InitViewModel()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Pad Table Setup";
                //_SelectDeselectAllPadCommand = new RelayCommand<PAD_USING>(SelectDeselectAllPadCommand);
                _TableIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangeTableTemplateIndexCommand);
                _PadIndexMoveCommand = new AsyncCommand<SETUP_DIRECTION>(ChangePadIndexCommand);
                _CreatePadTableTemplateCommand = new AsyncCommand(CreatePadTableTemplateCommand);
                _RemovePadTableTemplateCommand = new AsyncCommand(REMOVEPadTableTemplateCommand);

                #region ==> 5 Buttons
                OneButton.IconCaption = "ALL";
                TwoButton.IconCaption = "CLEAR";
                ThreeButton.IconCaption = null;
                FourButton.IconCaption = null;

                ThreeButton.Caption = "ADD";
                FourButton.Caption = "DELETE";

                OneButton.Command = new AsyncCommand(SelectAllPadCommand);
                TwoButton.Command = new AsyncCommand(DeselectAllPadCommand);

                //ThreeButton.Command = new AsyncCommand(SelectEveryNPadCommand);
                ThreeButton.Command = _CreatePadTableTemplateCommand;
                FourButton.Command = _RemovePadTableTemplateCommand;

                //FourButton.Command = _CreatePadTableTemplateCommand;

                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/checkbox-multi-W.png");
                TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/unmark-multi-W.png");
                //ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/numbox-W.png");
                //FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/plus-box-outline_W.png");

                //new Uri("pack://application:,,,/ImageResourcePack;component/Images/dice-5.png");

                FiveButton.IsEnabled = false;
                #endregion

                #region ==> Prev/Next Buttons
                PadJogLeftUp.IconCaption = "TABLE";
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightUp.IconCaption = "TABLE";
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");
                PadJogLeftDown.IconCaption = "PAD";
                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightDown.IconCaption = "PAD";
                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;
                PadJogLeftDown.CaptionSize = 16;
                PadJogRightDown.CaptionSize = 16;

                PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;
                PadJogLeftDown.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightDown.CommandParameter = SETUP_DIRECTION.NEXT;

                PadJogLeftUp.Command = _TableIndexMoveCommand;
                PadJogRightUp.Command = _TableIndexMoveCommand;
                PadJogLeftDown.Command = _PadIndexMoveCommand;
                PadJogRightDown.Command = _PadIndexMoveCommand;
                #endregion

                #region ==> Jog Buttons
                PadJogSelect.IconCaption = "SET";
                PadJogSelect.Command = new AsyncCommand(PadEnableToggleCommand);
                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/checkbox-W.png");

                PadJogLeft.IconSource = null;
                PadJogRight.IconSource = null;
                PadJogUp.IconSource = null;
                PadJogDown.IconSource = null;

                PadJogLeft.Command = null;
                PadJogRight.Command = null;
                PadJogUp.Command = null;
                PadJogDown.Command = null;

                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
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

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                retVal = InitPNPSetupUI();

                MainViewTarget = DisplayPort;

                MiniViewTarget = Wafer;

                UseUserControl = UserControlFucEnum.DEFAULT;

                // Create First Table Template
                if (PMIInfo.PadTableTemplateInfo.Count <= 0)
                {
                    CreatePadTableTemplateCommand();
                }

                this.PMIModule().UpdateRenderLayer();

                //this.CoordinateManager().OverlayUpdateDelegate = () =>
                //{
                //    this.PMIModule().UpdateRenderLayer(PMIInfo.SelectedPadTableTemplateIndex);
                //};

                UpdateLabel();

                UpdateOverlayDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [InitSetup()] : {err}");
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
                LoggerManager.Debug($"[PadTableSetupModule] [ParamValidation()] : {err}");
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
                EventCodeEnum retVal2 = Extensions_IParam.ElementStateDefaultValidation(PMIInfo);

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
                DrawingGroup.SetupMode = PMI_SETUP_MODE.TABLE;

                SelectedPadTableTemplateIndex = 0;

                SelectedPadTableTemplate = null;
                SelectedPMIPad = null;

                DrawingGroup.PadTableTemplate = null;
                DrawingGroup.SelectedPMIPadIndex = 0;

                if (PMIInfo.PadTableTemplateInfo.Count > 0)
                {
                    SelectedPadTableTemplate = PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex];
                }

                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    SelectedPMIPad = PadInfos.PMIPadInfos[DrawingGroup.SelectedPMIPadIndex];
                }

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.StageSupervisor().WaferObject.DrawDieOverlay(CurCam);

                retVal = await InitSetup();

                UpdateOverlay();

                //PMIInfo.RenderTableMode = PMI_RENDER_TABLE_MODE.ENABLE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Debug($"[PadTableSetupModule] [PageSwitched()] : {err}");
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
                DrawingGroup.SetupMode = PMI_SETUP_MODE.UNDEFINED;
                DrawingGroup.PadTableTemplate = null;

                UpdateOverlayDelegate(DELEGATEONOFF.OFF);

                this.StageSupervisor().WaferObject.StopDrawDieOberlay(CurCam);

                //PMIInfo.RenderTableMode = PMI_RENDER_TABLE_MODE.DISABLE;

                // Grouping process
                if (this.StageSupervisor.WaferObject.GetAlignState() == AlignStateEnum.DONE)
                {
                    for (int i = 0; i < PMIInfo.PadTableTemplateInfo.Count; i++)
                    {
                        if ((PMIInfo.PadTableTemplateInfo[i].GroupingDone.Value == false) &&
                                (PMIInfo.PadTableTemplateInfo[i].PadEnable.Count > 0))
                        {
                            retVal = this.PMIModule().PadGroupingMethod(i);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = this.PMIModule().MakeGroupSequence(i);

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    PMIInfo.PadTableTemplateInfo[i].GroupingDone.Value = true;
                                }
                            }
                        }
                    }
                }

                retVal = this.StageSupervisor().SaveWaferObject();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[PadTableSetupModule] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PadTableSetupModule] [Cleanup()] : Error");
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
                LoggerManager.Debug($"[PadTableSetupModule] [Cleanup()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region ==> Properties
        #endregion

        #region ==> Commands
        //private AsyncCommand<object> _SelectDeselectAllPadCommand;

        private AsyncCommand<SETUP_DIRECTION> _TableIndexMoveCommand;
        private AsyncCommand<SETUP_DIRECTION> _PadIndexMoveCommand;

        private AsyncCommand _CreatePadTableTemplateCommand;
        private AsyncCommand _RemovePadTableTemplateCommand;

        private Task ChangeTableTemplateIndexCommand(SETUP_DIRECTION direction)
        {
            try
            {
                // PadTableTemplateInfo 존재할 때
                if (PMIInfo.PadTableTemplateInfo.Count > 0)
                {
                    

                    if (direction == SETUP_DIRECTION.PREV)
                    {
                        if (SelectedPadTableTemplateIndex == 0)
                        {
                            SelectedPadTableTemplateIndex = PMIInfo.PadTableTemplateInfo.Count - 1;
                        }
                        else
                        {
                            SelectedPadTableTemplateIndex--;
                        }
                    }
                    else
                    {
                        // 현재 Index가 마지막인 경우
                        if (SelectedPadTableTemplateIndex == PMIInfo.PadTableTemplateInfo.Count - 1)
                        {
                            SelectedPadTableTemplateIndex = 0;
                        }
                        else
                        {
                            SelectedPadTableTemplateIndex++;
                        }
                    }

                    SelectedPadTableTemplate = PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex];
                }

                this.PMIModule().UpdateRenderLayer();

                UpdateLabel();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadManualRegistrationModule] [ChangeTemplateIndexCommand()] : {err}");
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
                LoggerManager.Debug($"[PadTableSetupModule] [ChangePadIndexCommand()] : {err}");
            }
            finally
            {
                
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
                string padenable = string.Empty;

                if (SelectedPadTableTemplate != null)
                {
                    if (SelectedPadTableTemplate.PadEnable.Count > 0)
                    {
                        if (SelectedPadTableTemplate.PadEnable[DrawingGroup.SelectedPMIPadIndex].Value == true)
                        {
                            padenable = "O";
                        }
                        else
                        {
                            padenable = "X";
                        }

                        StepLabel = ($"Table ({SelectedPadTableTemplateIndex + 1} / {PMIInfo.PadTableTemplateInfo.Count}), Pad: {DrawingGroup.SelectedPMIPadIndex + 1} ({padenable})/ {PadInfos.PMIPadInfos.Count}");
                    }
                    else
                    {
                        StepLabel = $"Pad count is zero.";
                    }
                }
                else
                {
                    StepLabel = $"Unknown Label";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [UpdateLabel()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 현재 선택된 Pad의 사용 여부를 변경하는 함수
        /// </summary>
        private Task PadEnableToggleCommand()
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    if (SelectedPadTableTemplate.PadEnable[DrawingGroup.SelectedPMIPadIndex].Value == true)
                    {
                        SelectedPadTableTemplate.PadEnable[DrawingGroup.SelectedPMIPadIndex].Value = false;
                    }
                    else
                    {
                        SelectedPadTableTemplate.PadEnable[DrawingGroup.SelectedPMIPadIndex].Value = true;
                    }

                    // 데이터 변경으로, 그룹핑을 다시하기위해 
                    SelectedPadTableTemplate.GroupingDone.Value = false;

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [SelectDeselectPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 전체 Pad의 사용 여부를 변경하는 함수
        /// </summary>
        /// <param name="padUsing"></param>
        private Task SelectDeselectAllPadCommand(PAD_USING padUsing)
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    int CurTableIndex = SelectedPadTableTemplateIndex;

                    for (int i = 0; i < PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable.Count; i++)
                    {
                        PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[i].Value = (padUsing == PAD_USING.ENABLE ? true : false);
                    }

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [SelectDeselectAllPadCommand()] : {err}" + $" {padUsing}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;            
        }

        /// <summary>
        /// 전체 Pad의 사용 여부를 변경하는 함수
        /// </summary>
        /// <param name="padUsing"></param>
        private Task SelectAllPadCommand()
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    int CurTableIndex = SelectedPadTableTemplateIndex;
                    for (int i = 0; i < PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable.Count; i++)
                    {
                        PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[i].Value = true;
                    }

                    PMIInfo.PadTableTemplateInfo[CurTableIndex].GroupingDone.Value = false;

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [SelectAllPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;            
        }

        /// <summary>
        /// 전체 Pad의 사용 여부를 변경하는 함수
        /// </summary>
        /// <param name="padUsing"></param>
        private Task DeselectAllPadCommand()
        {
            try
            {
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    int CurTableIndex = SelectedPadTableTemplateIndex;

                    for (int i = 0; i < PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable.Count; i++)
                    {
                        PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[i].Value = false;
                    }

                    PMIInfo.PadTableTemplateInfo[CurTableIndex].GroupingDone.Value = false;

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [DeselectAllPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// N개의 pad 마다 선택하는 함수
        /// </summary>
        private Task SelectEveryNPadCommand()
        {
            try
            {
                int interval = 1;
                int maxVal = PadInfos.PMIPadInfos.Count() - 1;

                int CurTableIndex = SelectedPadTableTemplateIndex;

                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        interval = VirtualKeyboard.Show(interval, 1, maxVal);
                    });

                    for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                    {
                        if ((i == 0) || (i % interval == 0))
                        {
                            PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[i].Value = true;
                        }
                        else
                        {
                            PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[i].Value = false;
                        }
                    }

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [SelectEveryNPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        public Task CreatePadTableTemplateCommand()
        {
            try
            {
                PadTableTemplate TableInfo = new PadTableTemplate();

                for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                {
                    TableInfo.PadEnable.Add(new Element<bool> { Value = true });
                }

                PMIInfo.PadTableTemplateInfo.Add(TableInfo);
                
                // 추가된 Table이 SelectedPadTableTemplateIndex 로 변경되기 때문에 아래 로직이 필요함
                SelectedPadTableTemplateIndex = PMIInfo.PadTableTemplateInfo.Count - 1;
                if (SelectedPadTableTemplateIndex >= 0)
                {
                    SelectedPadTableTemplate = PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex];
                }

                this.PMIModule().UpdateRenderLayer();

                UpdateLabel();

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [CreatePadTableTemplateCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        public Task REMOVEPadTableTemplateCommand()
        {
            try
            {
                // TODO : 현재 테이블 삭제하고, 추가 작업

                if (PMIInfo.PadTableTemplateInfo.Count > 1)
                {
                    PMIInfo.PadTableTemplateInfo.RemoveAt(SelectedPadTableTemplateIndex);

                    if (SelectedPadTableTemplateIndex == 0)
                    {
                        SelectedPadTableTemplateIndex = PMIInfo.PadTableTemplateInfo.Count - 1;
                    }
                    else
                    {
                        SelectedPadTableTemplateIndex = SelectedPadTableTemplateIndex - 1;
                    }

                    if (PMIInfo.PadTableTemplateInfo.Count > 0)
                    {
                        SelectedPadTableTemplate = PMIInfo.PadTableTemplateInfo[SelectedPadTableTemplateIndex];
                    }


                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [REMOVEPadTableTemplateCommand()] : {err}");
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// N개의 pad를 random으로 선택하는 함수
        /// </summary>
        public Task SelectRandomPadCommand()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
            

            try
            {
                int randCnt = 1;
                int maxVal = PadInfos.PMIPadInfos.Count() - 1;

                int CurTableIndex = SelectedPadTableTemplateIndex;

                List<int> randList = new List<int>();
                //bool isSame = false;

                Random rand = new Random();

                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    randCnt = VirtualKeyboard.Show(randCnt, 1, maxVal);

                    List<Int32> result = new List<int>();
                    result = (GetRandomNumbers(0, maxVal, randCnt)).ToList();

                    //for (int i = 0; i < randCnt; i++)
                    //{
                    //    while (true)
                    //    {
                    //        randList.Add(rand.Next(1, maxVal));

                    //        isSame = false;

                    //        for (int j = 0; j < i; j++)
                    //        {
                    //            if (randList[j] == randList[i])
                    //            {
                    //                isSame = true;
                    //                break;
                    //            }
                    //        }

                    //        if (!isSame) break;                            
                    //    }
                    //}

                    for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                    {
                        PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[i].Value = false;
                    }

                    for (int j = 0; j < result.Count; j++)
                    {
                        int index = result[j];
                        PMIInfo.PadTableTemplateInfo[CurTableIndex].PadEnable[index].Value = true;
                    }

                    this.PMIModule().UpdateRenderLayer();

                    UpdateLabel();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PadTableSetupModule] [SelectRandomPadCommand()] : {err}");
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 지정된 숫자의 범위 갯수만큼 중복되지 않는 난수 배열을 생성합니다.<br />
        /// 난수 범위: start ~ end (* end - 1 이 아님에 유의)
        /// </summary>
        /// <param name="start">난수의 시작 값 입니다.</param>
        /// <param name="end">난수의 끝 값 입니다.</param>
        public static Int32[] GetRandomNumbers(Int32 start, Int32 end, int count)
        {
            // 시작 인덱스, 종료 인덱스, 숫자 갯수, 반복 조건식 값
            // 그리고 원본 숫자 및 결과 숫자를 저장할 목록(List)을 초기화한다.
            Int32 startIndex = start > end ? end : start;
            Int32 endIndex = start > end ? start : end;
            //Int32 nCount = endIndex - startIndex + 1;
            Int32 nLoopCount = startIndex + endIndex + 1;
            List<Int32> numberList = new List<Int32>(count);
            List<Int32> resultList = new List<Int32>(count);

            // 원본 목록에 start 부터 end 까지의 값을 집어넣는다.
            for (Int32 i = startIndex; i < nLoopCount; i++)
                numberList.Add(i);

            Stopwatch sw = new Stopwatch();

            // 스톱워치 시작
            sw.Start();

            // 원본 목록에 값이 있을 경우 계속 반복한다.
            while (numberList.Count > 0)
            {

                // 흐른 틱 값과 결과 목록의 항목 갯수를 더해서 시드 값을 생성한다.
                Random rGen = new Random((Int32)sw.ElapsedTicks + resultList.Count);

                // 0 부터 원본 목록의 항목 갯수까지의 난수를 생성한다.
                Int32 pickedIndex = rGen.Next(0, numberList.Count);

                // 원본 목록에서 값을 가져온 다음 결과 목록에 추가하고
                // 가져온 값을 제거한다.
                resultList.Add(numberList[pickedIndex]);
                numberList.RemoveAt(pickedIndex);
            }

            // 스톱워치 정지
            sw.Stop();

            // 결과 목록을 배열로 만들어서 반환한다.
            return resultList.ToArray();
        }

        ///// <summary>
        ///// 선택된 Table index를 변경하는 함수
        ///// </summary>
        ///// <param name="direction"></param>
        //private void TableIndexMoveCommand(SETUP_DIRECTION direction)
        //{
        //    try
        //    {
        //        var PMIInfo = PMIInfo;

        //        var SelectedPadTableIndex = PMIInfo.GetSelectedPadTableIndex();

        //        switch (direction)
        //        {
        //            case SETUP_DIRECTION.PREV:

        //                if (SelectedPadTableIndex > 0)
        //                {
        //                    SelectedPadTableIndex--;

        //                    PMIInfo.SetSeletedPadTableTemplateIndex(SelectedPadTableIndex);
        //                }
        //                else
        //                {
        //                    PMIInfo.SetSeletedPadTableTemplateIndex(PMIInfo.GetPadTableInfo().Count - 1);
        //                }
        //                break;

        //            case SETUP_DIRECTION.NEXT:
        //                if (SelectedPadTableIndex < PMIInfo.GetPadTableInfo().Count - 1)
        //                {
        //                    SelectedPadTableIndex++;

        //                    PMIInfo.SetSeletedPadTableTemplateIndex(SelectedPadTableIndex);
        //                }
        //                else
        //                {
        //                    PMIInfo.SetSeletedPadTableTemplateIndex(0);
        //                }
        //                break;

        //            default:
        //                break;
        //        }

        //        this.PMIModule().UpdateRenderLayer(null, PMIInfo.GetSelectedPadTableIndex());
        //        UpdateLabel();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadTableSetupModule] [TableIndexMoveCommand(" + $"{direction}" + $")] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}

        ///// <summary>
        ///// 선택된 Pad index를 변경하는 함수
        ///// </summary>
        ///// <param name="direction"></param>
        //private void PadIndexMoveCommand(SETUP_DIRECTION direction)
        //{
        //    try
        //    {
        //        if ((PadInfos.PMIPadInfos.Count > 0) && (direction is SETUP_DIRECTION))
        //        {
        //            switch (direction)
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

        //            this.PMIModule().UpdateRenderLayer(PMIInfo.SelectedPadTableTemplateIndex);

        //            UpdateLabel();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PadTableSetupModule] [PadIndexMoveCommand(" + $"{direction}" + $")] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}
        #endregion
    }
}
