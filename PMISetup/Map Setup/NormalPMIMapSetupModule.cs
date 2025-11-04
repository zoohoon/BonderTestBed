using LogModule;
using PMISetup.UC;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VirtualKeyboardControl;
using SubstrateObjects;
using SerializerUtil;
using MetroDialogInterfaces;
using System.Windows.Media;

namespace PMISetup
{

    public class NormalPMIMapSetupModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IPackagable
    {
        #region ==> Common PNP Declaration
        public override Guid ScreenGUID { get; } = new Guid("EAF50F0A-D36D-86BB-AB6B-633A98474483");
        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        private WaferObject Wafer
        {
            get { return (WaferObject)this.StageSupervisor().WaferObject; }
        }

        public NormalPMIMapSetupModule()
        {

        }
        //public PMIModuleDevParam PMIDevParam;

        //public  WaferObject Wafer { get; set; }

        //private ProbingSequenceModule ProbingSeq { get; set; }

        //private ProbeCard ProbeCard { get { return this.StageSupervisor().ProbeCardInfo; } }

        private WaferPMIMapSetupControlService WaferPMIMapSetup;

        private int _SelectedNormalPMIMapTemplateIndex;
        public int SelectedNormalPMIMapTemplateIndex
        {
            get { return _SelectedNormalPMIMapTemplateIndex; }
            set
            {
                if (value != _SelectedNormalPMIMapTemplateIndex)
                {
                    _SelectedNormalPMIMapTemplateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DieMapTemplate _SelectedNormalPMIMapTemplate;
        public DieMapTemplate SelectedNormalPMIMapTemplate
        {
            get { return _SelectedNormalPMIMapTemplate; }
            set
            {
                if (value != _SelectedNormalPMIMapTemplate)
                {
                    _SelectedNormalPMIMapTemplate = value;
                    RaisePropertyChanged();
                }
            }
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
                    //Header = "PMI Map Setup";

                    //Wafer = this.StageSupervisor().WaferObject as WaferObject;
                    //ProbingSeq = this.ProbingSequenceModule() as ProbingSequenceModule;

                    //PMIDevParam = ((PMIModule as IHasDevParameterizable).DevParam) as PMIModuleDevParam;

                    //WaferPMIMapSetup = new WaferPMIMapSetupControlService();

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
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [IsExecute()] : {err}");
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
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [DeInitModule()] : {err}");
            }
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "PMI Map Setup";

                _ChangePadTableCommand = new AsyncCommand<SETUP_DIRECTION>(ChangeTableTemplateIndexCommand);
                _WaferMapMoveCommand = new AsyncCommand<object>(WaferMapMoveCommand);
                _ChangeMapIndexCommand = new AsyncCommand<object>(ChangeMapIndexCommand);

                _MapSelectModeCommand = new AsyncCommand<object>(ChangeMapSelectModeCommand);
                //_AddNormalPMIMapCommand = new AsyncCommand(AddNormalPMIMapCommand);
                //_DelNormalPMIMapCommand = new AsyncCommand(DelNormalPMIMapCommand);
                _SelectAllTestDieCommand = new AsyncCommand(SelectAllTestDieCommand);
                _ClearNormalPMIMapCommand = new AsyncCommand(ClearNormalPMIMapCommand);

                #region ==> 5 Buttons
                OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/grid.png");
                //TwoButton.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //    new Uri("pack://application:,,,/ImageResourcePack;component/Images/file-plus_W.png");
                //ThreeButton.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //    new Uri("pack://application:,,,/ImageResourcePack;component/Images/Delete_Forever_W.png");
                //TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/numbox-W.png");
                ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/checkbox-multi-W.png");
                FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/clear-rect.png");
                //FiveButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/PNP_Setting.png");

                OneButton.IconCaption = "DIE/DUT";
                //TwoButton.IconCaption = "ADD MAP";
                //ThreeButton.IconCaption = "DEL MAP";
                //TwoButton.IconCaption = "N DIE";
                ThreeButton.IconCaption = "SEL ALL";
                FourButton.IconCaption = "CLEAR";
                //FiveButton.IconCaption = "Wafer";

                OneButton.IsEnabled = false;
                TwoButton.IsEnabled = false;

                OneButton.Command = _MapSelectModeCommand;
                //TwoButton.Command = _AddNormalPMIMapCommand;
                //ThreeButton.Command = _DelNormalPMIMapCommand;
                //TwoButton.Command = new AsyncCommand(SelectNDieProbedCommand);
                ThreeButton.Command = _SelectAllTestDieCommand;
                FourButton.Command = _ClearNormalPMIMapCommand;
                //FiveButton.Command = new AsyncCommand(WaferMapSetupCommand);
                #endregion

                #region ==> Prev/Next Buttons
                


                PadJogLeftDown.IconCaption = "TABLE";
                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                PadJogRightDown.IconCaption = "TABLE";
                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                PadJogLeftUp.CaptionSize = 16;
                PadJogRightUp.CaptionSize = 16;
                PadJogLeftDown.CaptionSize = 16;
                PadJogRightDown.CaptionSize = 16;

                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    PadJogLeftUp.IconCaption = "MAP";
                    PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-left.png");
                    PadJogRightUp.IconCaption = "MAP";
                    PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/arrow-right.png");

                    PadJogLeftUp.Command = _ChangeMapIndexCommand;
                    PadJogRightUp.Command = _ChangeMapIndexCommand;

                    PadJogLeftUp.CommandParameter = SETUP_DIRECTION.PREV;
                    PadJogRightUp.CommandParameter = SETUP_DIRECTION.NEXT;
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    PadJogLeftUp.IconCaption = "";
                    PadJogLeftUp.IconSource = null;
                    PadJogRightUp.IconCaption = "";
                    PadJogRightUp.IconSource = null;

                    PadJogLeftUp.Command = null;
                    PadJogRightUp.Command = null;

                    PadJogLeftUp.CommandParameter = null;
                    PadJogRightUp.CommandParameter = null;
                }
                
                PadJogLeftDown.Command = _ChangePadTableCommand;
                PadJogRightDown.Command = _ChangePadTableCommand;
                
                PadJogLeftDown.CommandParameter = SETUP_DIRECTION.PREV;
                PadJogRightDown.CommandParameter = SETUP_DIRECTION.NEXT;
                #endregion

                #region ==> Jog Buttons
                PadJogSelect.IconCaption = "SET";
                PadJogSelect.SetMiniIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/checkbox-W.png");


                PadJogSelect.Command = new AsyncCommand(PMIMapSelectCommand);
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
                

                PadJogLeft.RepeatEnable = true;
                PadJogRight.RepeatEnable = true;
                PadJogUp.RepeatEnable = true;
                PadJogDown.RepeatEnable = true;

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowLeftW.png");
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowRightW.png");
                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowUpW.png");
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowDownW.png");
                #endregion

                //SetPNPVisibility(MiniViewSwap: false, MiniViewZoom: false, LightJog: false, MotionJog: false);

                SetSideViewMode(SideViewMode.TEXTBLOCK_ONLY, "<SET-UP Information>");
                //SetSideViewTextProperties("Black", "Transparent", 18);

                SideViewWidth = 250;
                SideViewHeight = 200;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
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
                Header = "PMI Map Setup";

                double fs = 12;
                Brush fc = Brushes.White;
                Brush bc = Brushes.Transparent;

                SideViewTargetVisibility = Visibility.Visible;
                SideViewSwitchVisibility = Visibility.Hidden;

                SideViewHorizontalAlignment = HorizontalAlignment.Right;
                SideViewVerticalAlignment = VerticalAlignment.Top;

                SideViewTitle = "";

                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    retVal = InitPnpModuleStage_AdvenceSetting();

                    MakeSideTextblock($"Current Map Index : [{SelectedNormalPMIMapTemplateIndex + 1}]", fs, fc, bc);
                    MakeSideTextblock($"Current Table Index : [{SelectedPadTableTemplateIndex + 1}]", fs, fc, bc);
                    MakeSideTextblock($"Number of registered dies : [{GetPMIDieEnableCount()}]", fs, fc, bc);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    retVal = InitPnpModuleStage();

                    MakeSideTextblock($"Current Table Index : [{SelectedPadTableTemplateIndex + 1}]", fs, fc, bc);
                    MakeSideTextblock($"Number of registered dies : [{GetPMIDieEnableCount()}]", fs, fc, bc);
                }

                retVal = InitLightJog(this);

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                WaferPMIMapSetup = new WaferPMIMapSetupControlService();

                AdvanceSetupView = WaferPMIMapSetup.DialogControl;
                AdvanceSetupViewModel = WaferPMIMapSetup;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        private bool RememberMapViewCurIndexVisiablity { get; set; }

        /// <summary>
        /// PNP Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                this.StageSupervisor().WaferObject.MapViewStageSyncEnable = false;
                this.StageSupervisor().WaferObject.IsMapViewShowPMIEnable = true;
                this.StageSupervisor().WaferObject.IsMapViewShowPMITable = true;
                this.StageSupervisor().WaferObject.MapViewControlMode = MapViewMode.MapMode;

                //MachineIndex MI = new MachineIndex((this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value), (this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value));
                //this.StageSupervisor().WaferObject.SetCurrentMIndex(MI);

                CurCam.CamSystemMI.XIndex = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value;
                CurCam.CamSystemMI.YIndex = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value;

                this.StageSupervisor().WaferObject.MapViewAssignCamType.Value = EnumProberCam.UNDEFINED;

                retVal = InitPNPSetupUI();

                MainViewTarget = Wafer;

                UseUserControl = UserControlFucEnum.DEFAULT;

                //PMIInfo.SetSelectedNormalMapTemplateIndex(0);
                //PMIInfo.SetSelectedPadTableTemplateIndex(0);
                //CurMapIndex = PMIInfo.GetSelectedNormalPMIMapIndex();

                UpdateLabel();
                //TargetRectangleWidth = 128;
                //TargetRectangleHeight = 128;

                this.PMIModule().SetSubModule(this);
                

                ChangedIndexDelegate(DELEGATEONOFF.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [InitSetup()] : {err}");
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private async Task UpdateMapIndex(object newVal)
        {

            try
            {
                MachineIndex MI = new MachineIndex();

                MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();

                await GetUnderDutDices(MI);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayDie() Error occurred.");
                LoggerManager.Exception(err);
            }
        }

        private void ChangedIndexDelegate(DELEGATEONOFF flag)
        {
            try
            {
                if(flag == DELEGATEONOFF.ON)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += UpdateMapIndex;
                }
                else
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= UpdateMapIndex;
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
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [ParamValidation()] : {err}");
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
        /// Parameter Save 해주는 함수
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                EventCodeEnum retVal1 = this.PMIModule().SaveDevParameter();
                //EventCodeEnum retVal2 = Extensions_IParam.SaveParameter(Wafer);
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
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [SaveDevParameter()] : {err}");
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
                

                SideViewTargetVisibility = Visibility.Visible;
                //PackagableParams.Add(SerializeManager.SerializeToByte((this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).PMIInfo));
                //WaferObject wafer = (Wafer as WaferObject);

                SelectedPadTableTemplateIndex = 0;

                SelectedNormalPMIMapTemplateIndex = 0;

                SelectedNormalPMIMapTemplate = null;

                if (PMIInfo.NormalPMIMapTemplateInfo.Count > 0)
                {
                    SelectedNormalPMIMapTemplate = PMIInfo.NormalPMIMapTemplateInfo[SelectedNormalPMIMapTemplateIndex];
                }

                PMIInfo.SelectedNormalPMIMapTemplateIndex = SelectedNormalPMIMapTemplateIndex;

                this.StageSupervisor().WaferObject.IsMapViewShowPMIEnable = true;
                this.StageSupervisor().WaferObject.IsMapViewShowPMITable = true;

                MachineIndex MI = new MachineIndex();

                MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();

                await GetUnderDutDices(MI);
                SetPackagableParams();
                retVal = await InitSetup();
                //this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [PageSwitched()] : {err}");
                LoggerManager.Exception(err);
            }
            finally
            {
                RememberMapViewCurIndexVisiablity = Wafer.MapViewCurIndexVisiablity;
                Wafer.MapViewCurIndexVisiablity = true;
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
                this.StageSupervisor().WaferObject.IsMapViewShowPMIEnable = false;
                this.StageSupervisor().WaferObject.IsMapViewShowPMITable = false;

                retVal = this.StageSupervisor().SaveWaferObject();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[PadManualRegistrationModule] Parameter (WaferObject) save processing was not performed normally.");
                }

                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[NormalPMIMapSetupModule] [Cleanup()] : Error");
                }
                //if (IsParameterChanged() == true)
                //    retVal = await base.Cleanup(null);
                //else
                //    retVal = await base.Cleanup(parameter);

                //if (retVal == EventCodeEnum.SAVE_PARAM)
                //{
                //    retVal = SaveDevParameter();
                //}

                ChangedIndexDelegate(DELEGATEONOFF.OFF);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [CleanUp()] : {err}");
            }
            finally
            {
                Wafer.MapViewCurIndexVisiablity = RememberMapViewCurIndexVisiablity;
            }
            return retVal;
        }
        #endregion

        #region ==> Properties
        //private int _CurMapIndex = 0;
        //public int CurMapIndex
        //{
        //    get { return _CurMapIndex; }
        //    set
        //    {
        //        if (value != _CurMapIndex)
        //        {
        //            _CurMapIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private MAP_SELECT_MODE _CurSelectMode = MAP_SELECT_MODE.DIE;
        public MAP_SELECT_MODE CurSelectMode
        {
            get { return _CurSelectMode; }
            set
            {
                if (value != _CurSelectMode)
                {
                    _CurSelectMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<IDeviceObject> _UnderDutDevs;
        //public ObservableCollection<IDeviceObject> UnderDutDevs
        //{
        //    get { return this.ProbingModule().ProbingProcessStatus.UnderDutDevs; }
        //}

        #endregion

        #region ==> Commands
        private AsyncCommand<SETUP_DIRECTION> _ChangePadTableCommand;
        private AsyncCommand<object> _ChangeMapIndexCommand;

        private AsyncCommand<object> _WaferMapMoveCommand;

        private AsyncCommand<object> _MapSelectModeCommand;

        //private AsyncCommand _AddNormalPMIMapCommand;
        //private AsyncCommand _DelNormalPMIMapCommand;

        private AsyncCommand _ClearNormalPMIMapCommand;
        private AsyncCommand _SelectAllTestDieCommand;

        public MachineIndex GetFirstDutIndex(ObservableCollection<IDeviceObject> deviceObjs = null)
        {
            MachineIndex machineIndex = null;

            try
            {
                var cardinfo = this.GetParam_ProbeCard();

                if ((cardinfo != null) & (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0) & (this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Count > 0))
                {
                    long offsetx = 0;
                    long offsety = 0;

                    var firstdut = cardinfo.ProbeCardDevObjectRef.DutList[0];
                    long maxX, minX, maxY, minY;
                    long xNum, yNum;

                    IDeviceObject[,] devicesmap = null;

                    maxX = cardinfo.ProbeCardDevObjectRef.DutList.Max(d => d.MacIndex.XIndex);
                    minX = cardinfo.ProbeCardDevObjectRef.DutList.Min(d => d.MacIndex.XIndex);
                    maxY = cardinfo.ProbeCardDevObjectRef.DutList.Max(d => d.MacIndex.YIndex);
                    minY = cardinfo.ProbeCardDevObjectRef.DutList.Min(d => d.MacIndex.YIndex);

                    if (minX < 0)
                        offsetx = Math.Abs(minX);

                    if (minY < 0)
                        offsety = Math.Abs(minY);

                    xNum = maxX - (minX + offsetx) + 1;
                    yNum = maxY - (minY + offsety) + 1;

                    devicesmap = new IDeviceObject[xNum, yNum];

                    List<IDeviceObject> deviceObjects = null;

                    long DieminX = 0;
                    long DieminY = 0;

                    if (deviceObjs == null)
                    {
                        DieminX = this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Min(index => index.DieIndexM.XIndex);
                        DieminY = this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Min(index => index.DieIndexM.YIndex);

                        deviceObjects = this.ProbingModule().ProbingProcessStatus.UnderDutDevs.ToList<IDeviceObject>();
                    }
                    else
                    {
                        DieminX = deviceObjs.Min(index => index.DieIndexM.XIndex);
                        DieminY = deviceObjs.Min(index => index.DieIndexM.YIndex);
                        deviceObjects = deviceObjs.ToList<IDeviceObject>();
                    }

                    Parallel.For(0, yNum, y =>
                    {
                        Parallel.For(0, xNum, x =>
                        {
                            IDeviceObject obj = deviceObjects.Find(dev => (dev.DieIndexM.XIndex - DieminX) == x
                             && (dev.DieIndexM.YIndex - DieminY) == y);
                            if (obj != null)
                            {
                                devicesmap[x, y] = obj;
                            }
                        });
                    });

                    if (devicesmap[(cardinfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex + offsetx), (cardinfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex + offsety)] != null)
                    {
                        machineIndex = devicesmap[(cardinfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex + offsetx), (cardinfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex + offsety)].DieIndexM;
                    }

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return machineIndex;
        }

        public async Task<EventCodeEnum> GetUnderDutDices(MachineIndex mCoord)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            List<IDeviceObject> dev = new List<IDeviceObject>();

            var cardinfo = this.GetParam_ProbeCard();

            try
            {
                ObservableCollection<IDeviceObject> dutdevs = new ObservableCollection<IDeviceObject>();

                if (CurSelectMode == MAP_SELECT_MODE.DIE)
                {
                    IDeviceObject devobj = StageSupervisor.WaferObject.GetDevices().Find(x => x.DieIndexM.Equals(mCoord));

                    if (devobj != null)
                    {
                        dutdevs.Add(devobj);
                    }
                }
                else
                {
                    if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                    {
                        Task task = new Task(() =>
                        {
                            for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                            {
                                //object tmp = cardinfo.ProbeCardDevObjectRef.GetRefOffset(dutIndex);

                                IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));

                                IDeviceObject devobj = StageSupervisor.WaferObject.GetDevices().Find(x => x.DieIndexM.Equals(retindex));

                                if (devobj != null)
                                {
                                    dev.Add(devobj);
                                    dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                                }
                                else
                                {
                                    devobj = new DeviceObject();
                                    devobj.DieIndexM.XIndex = retindex.XIndex;
                                    devobj.DieIndexM.YIndex = retindex.YIndex;

                                    dev.Add(devobj);

                                    dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                                }
                            }

                            if (dev.Count() > 0)
                            {
                                for (int devIndex = 0; devIndex < dev.Count; devIndex++)
                                {
                                    if (dev[devIndex] != null)
                                        dutdevs.Add(dev[devIndex]);
                                }
                            }
                        });
                        task.Start();
                        await task;
                        //MachineIndex mindex = GetFirstDutIndex();

                        //MXYIndex = new Point(dutdevs[0].DieIndexM.XIndex, dutdevs[0].DieIndexM.YIndex);
                        //MapIndexX = dutdevs[0].DieIndexM.XIndex;
                        //MapIndexY = dutdevs[0].DieIndexM.YIndex;
                        //this.ProbingModule().ProbingProcessStatus.UnderDutDevs = dutdevs;
                    }
                }

                this.ProbingModule().ProbingProcessStatus.UnderDutDevs = dutdevs;

                this.LoaderRemoteMediator().GetServiceCallBack()?.SetProbingDevices(this.LoaderController().GetChuckIndex(), this.ProbingModule().ProbingProcessStatus.UnderDutDevs);
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.NODATA;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

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
                }


                this.PMIModule().UpdateRenderLayer();

                UpdateLabel();

                if (!this.PnPManager().IsActivePageSwithcing())
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [ChangeTableTemplateIndexCommand()] : {err}");
            }
            finally
            {
                
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Wafer map에서 커서를 이동하는 함수
        /// </summary>
        /// <param name="direction"></param>
        private Task WaferMapMoveCommand(object direction)
        {
            try
            {

                MachineIndex MI = new MachineIndex();

                MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();

                switch ((JOG_DIRECTION)direction)
                {
                    case JOG_DIRECTION.UP:
                        MI.YIndex++;
                        break;
                    case JOG_DIRECTION.LEFT:
                        MI.XIndex--;
                        break;
                    case JOG_DIRECTION.RIGHT:
                        MI.XIndex++;
                        break;
                    case JOG_DIRECTION.DOWN:
                        MI.YIndex--;
                        break;
                    default:
                        break;
                }

                //GetUnderDutDices(CurCam.CamSystemMI);

                this.StageSupervisor().WaferObject.SetCurrentMIndex(MI);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [FuncWaferMapMoveCommand()] : {err}" + $"{direction}");
            }
            return Task.CompletedTask;
        }

      

        /// <summary>
        /// 현재 선택된 Die의 PMI Enable/Disable을 설정하는 함수
        /// </summary>
        private Task PMIMapSelectCommand()
        {
            try
            {

                var SelectedNormalPMIMap = SelectedNormalPMIMapTemplate;

                if (SelectedNormalPMIMap != null)
                {
                    var Xlength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(0);
                    var Ylength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(1);

                    // TODO : 
                    if ((SelectedNormalPMIMap.MapWidth <= 0) || (SelectedNormalPMIMap.MapHeight <= 0))
                    {
                        SelectedNormalPMIMap.MapWidth = Xlength;
                        SelectedNormalPMIMap.MapHeight = Ylength;

                        SelectedNormalPMIMap.InitMap();
                    }

                    foreach (var die in this.ProbingModule().ProbingProcessStatus.UnderDutDevs)
                    {
                        //  Index 확인 필요

                        if ((die.DieIndexM.XIndex >= 0) && (die.DieIndexM.XIndex < Xlength) &&
                            (die.DieIndexM.YIndex >= 0) && (die.DieIndexM.YIndex < Ylength))
                        {
                            var Enable = SelectedNormalPMIMap.GetEnable((int)die.DieIndexM.XIndex, (int)die.DieIndexM.YIndex);
                            var TableIndex = SelectedNormalPMIMap.GetTable((int)die.DieIndexM.XIndex, (int)die.DieIndexM.YIndex);

                            if (die.DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                if (Enable == 0x01)
                                {
                                    SelectedNormalPMIMap.SetEnable((int)die.DieIndexM.XIndex, (int)die.DieIndexM.YIndex, false);
                                    LoggerManager.Debug($"Reset as PMI(Table = {SelectedPadTableTemplateIndex}) DIE. DIE MI = ({die.DieIndexM.XIndex},{die.DieIndexM.YIndex}) state is {die.DieType.Value}");
                                }
                                else
                                {
                                    SelectedNormalPMIMap.SetEnable((int)die.DieIndexM.XIndex, (int)die.DieIndexM.YIndex, true);
                                    SelectedNormalPMIMap.SetTable((int)die.DieIndexM.XIndex, (int)die.DieIndexM.YIndex, SelectedPadTableTemplateIndex);
                                    LoggerManager.Debug($"Set as PMI(Table = {SelectedPadTableTemplateIndex}) DIE. DIE MI = ({die.DieIndexM.XIndex},{die.DieIndexM.YIndex}) state is {die.DieType.Value}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"Die Status Error. DIE MI = ({die.DieIndexM.XIndex},{die.DieIndexM.YIndex}) state is {die.DieType.Value}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Out of index X = {die.DieIndexM.XIndex}, Y = {die.DieIndexM.YIndex}, Width = {Xlength}, Height = {Ylength}");
                        }

                    }

                    if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                    {
                        // TODO: 변경된 맵을 로더로 전달해야 됨. 사용되는 곳은 Wafer

                        //int index = Wafer.PMIInfo.SelectedNormalPMIMapTemplateIndex;
                        //var PMINormalMap = Wafer.PMIInfo.NormalPMIMapTemplateInfo[index];
                        NormalPMIMapTemplateInfo info = new NormalPMIMapTemplateInfo();

                        info.SelectedNormalPMIMapTemplate = this.SelectedNormalPMIMapTemplate;
                        info.SelectedNormalPMIMapTemplateIndex = this.SelectedNormalPMIMapTemplateIndex;

                        byte[] bytes = SerializeManager.SerializeToByte(info, typeof(NormalPMIMapTemplateInfo));

                        this.LoaderRemoteMediator().GetServiceCallBack()?.UpdatgeNormalPMIMapTemplateInfo(bytes);
                    }

                    UpdateLabel();

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [PMIMapSelectCommand()] : {err}");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Wafer Map에서 선택 단위를 Die로 할지 Dut로 할지 설정하는 함수
        /// </summary>
        /// <param name="mode"></param>
        private async Task ChangeMapSelectModeCommand(object param)
        {
            try
            {
                if (CurSelectMode == MAP_SELECT_MODE.DIE)
                {
                    CurSelectMode = MAP_SELECT_MODE.DUT;
                }
                else
                {
                    CurSelectMode = MAP_SELECT_MODE.DIE;
                }

                MachineIndex MI = new MachineIndex();

                MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();

                await GetUnderDutDices(MI);

                //switch (CurSelectMode)
                //{
                //    case MAP_SELECT_MODE.DUT:

                //        //UnderDutList.Clear();

                //        break;

                //    case MAP_SELECT_MODE.DIE:

                //        //MachineIndex MI = new MachineIndex();
                //        //MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();
                //        //List<IDeviceObject> tmplist = this.ProbingModule().GetUnderDutList(MI);
                //        //ObservableCollection<IDeviceObject> myCollection = new ObservableCollection<IDeviceObject>(tmplist);

                //        //UnderDutList = myCollection;

                //        break;

                //    default:
                //        break;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [ChangeMapSelectModeCommand()]: {err}");
            }
        }

        ///// <summary>
        ///// PMI Pad Table index를 변경하는 함수
        ///// </summary>
        ///// <param name="direction"></param>
        //public void ChangePadTableCommand(SETUP_DIRECTION direction)
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
        //                    PMIInfo.GetSelectedPadTableIndex() = PMIInfo.PadTableInfo.Count - 1;
        //                }
        //                break;

        //            case SETUP_DIRECTION.NEXT:
        //                if (PMIInfo.GetSelectedPadTableIndex() < PMIInfo.PadTableInfo.Count - 1)
        //                {
        //                    PMIInfo.GetSelectedPadTableIndex()++;
        //                }
        //                else
        //                {
        //                    PMIInfo.GetSelectedPadTableIndex() = 0;
        //                }
        //                break;

        //            default:
        //                break;
        //        }
        //        UpdateLabel();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[NormalPMIMapSetupModule] [ChangePadTableCommand(" + $"{direction}" + $")] : {err}");
        //    }
        //}

        /// <summary>
        /// 선택된 PMI Map index를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        public Task ChangeMapIndexCommand(object direction)
        {
            try
            {
                if (PMIInfo.NormalPMIMapTemplateInfo.Count > 0)
                {
                    switch ((SETUP_DIRECTION)direction)
                    {
                        case SETUP_DIRECTION.PREV:

                            if (SelectedNormalPMIMapTemplateIndex > 0)
                            {
                                SelectedNormalPMIMapTemplateIndex--;
                            }
                            else
                            {
                                SelectedNormalPMIMapTemplateIndex = PMIInfo.NormalPMIMapTemplateInfo.Count - 1;
                            }
                            break;

                        case SETUP_DIRECTION.NEXT:

                            if (SelectedNormalPMIMapTemplateIndex < PMIInfo.NormalPMIMapTemplateInfo.Count - 1)
                            {
                                SelectedNormalPMIMapTemplateIndex++;
                            }
                            else
                            {
                                SelectedNormalPMIMapTemplateIndex = 0;
                            }
                            break;

                        default:
                            break;
                    }

                    SelectedNormalPMIMapTemplate = PMIInfo.NormalPMIMapTemplateInfo[SelectedNormalPMIMapTemplateIndex];
                    PMIInfo.SelectedNormalPMIMapTemplateIndex = SelectedNormalPMIMapTemplateIndex;

                    //PMIInfo.SetSelectedNormalMapTemplateIndex(CurMapIndex);

                    UpdateLabel();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [ChangeMapIndexCommand(" + $"{direction}" + $")] : {err}");
            }
            return Task.CompletedTask;
        }

        ///// <summary>
        ///// PMI map을 추가하는 함수
        ///// </summary>
        //public async Task AddNormalPMIMapCommand()
        //{
        //    try
        //    {
        //        long maxX, minX, maxY, minY;
        //        long xNum, yNum;

        //        maxX = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Max(d => d.DieIndexM.XIndex);
        //        minX = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Min(d => d.DieIndexM.XIndex);
        //        maxY = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Max(d => d.DieIndexM.YIndex);
        //        minY = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Min(d => d.DieIndexM.YIndex);

        //        xNum = maxX - minX + 1;
        //        yNum = maxY - minY + 1;

        //        //DieMapTemplate tmpMap = new DieMapTemplate(this.StageSupervisor().WaferObject.GetDevices());
        //        DieMapTemplate tmpMap = new DieMapTemplate((int)xNum, (int)yNum);

        //        if (tmpMap != null)
        //        {
        //            PMIInfo.NormalPMIMapTemplateInfo.Add(tmpMap);

        //            PMIInfo.SelectedNormalPMIMapTemplateIndex = PMIInfo.NormalPMIMapTemplateInfo.Count - 1;

        //            //CurMapIndex = PMIInfo.GetNormalPMIMapTemplateInfo().Count - 1;

        //            //PMIInfo.SetSelectedNormalMapTemplateIndex(CurMapIndex);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[NormalPMIMapSetupModule] [AddNormalPMIMapCommand()] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}

        ///// <summary>
        ///// 현재 선택된 PMI map을 삭제하는 함수
        ///// </summary>
        //public async Task DelNormalPMIMapCommand()
        //{
        //    try
        //    {
        //        if (PMIInfo.NormalPMIMapTemplateInfo.Count > 1)
        //        {
        //            var CurMapIndex = PMIInfo.SelectedNormalPMIMapTemplateIndex;

        //            PMIInfo.NormalPMIMapTemplateInfo.RemoveAt(CurMapIndex);

        //            if (CurMapIndex > PMIInfo.NormalPMIMapTemplateInfo.Count)
        //            {
        //                CurMapIndex = PMIInfo.NormalPMIMapTemplateInfo.Count - 1;
        //            }
        //            else
        //            {
        //                if (CurMapIndex > 0)
        //                {
        //                    CurMapIndex--;
        //                }

        //                //PMIInfo.SetSelectedNormalMapTemplateIndex(CurMapIndex);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[NormalPMIMapSetupModule] [DelNormalPMIMapCommand()] : {err}");
        //    }
        //}

        /// <summary>
        /// 현재 맵을 clear 하는 함수
        /// </summary>
        public Task ClearNormalPMIMapCommand()
        {
            try
            {
                //var Xlength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(0);
                //var Ylength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(1);

                var CurMap = PMIInfo.GetNormalPMIMapTemplate(SelectedNormalPMIMapTemplateIndex);

                CurMap.ClearEnable();
                CurMap.ClearTable();

                //for (int i = 0; i < Xlength; i++)
                //{
                //    for (int j = 0; j < Ylength; j++)
                //    {
                //        CurMap.SetEnable(i, j, false);
                //        //CurMap.SetTable(i, j, 0);
                //    }
                //}

                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    // TODO: 변경된 맵을 로더로 전달해야 됨. 사용되는 곳은 Wafer

                    //int index = Wafer.PMIInfo.SelectedNormalPMIMapTemplateIndex;
                    //var PMINormalMap = Wafer.PMIInfo.NormalPMIMapTemplateInfo[index];
                    NormalPMIMapTemplateInfo info = new NormalPMIMapTemplateInfo();

                    info.SelectedNormalPMIMapTemplate = this.SelectedNormalPMIMapTemplate;
                    info.SelectedNormalPMIMapTemplateIndex = this.SelectedNormalPMIMapTemplateIndex;

                    byte[] bytes = SerializeManager.SerializeToByte(info, typeof(NormalPMIMapTemplateInfo));

                    this.LoaderRemoteMediator().GetServiceCallBack()?.UpdatgeNormalPMIMapTemplateInfo(bytes);
                }

                UpdateLabel();

                if (!this.PnPManager().IsActivePageSwithcing())
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [ClearNormalPMIMapCommand()] : {err}");
            }
            return Task.CompletedTask;
        }


        public Task SelectAllTestDieCommand()
        {
            try
            {

                var Xlength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(0);
                var Ylength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(1);

                var CurMap = PMIInfo.GetNormalPMIMapTemplate(SelectedNormalPMIMapTemplateIndex);

                // TODO : 
                if ((CurMap.MapWidth <= 0) || (CurMap.MapHeight <= 0))
                {
                    CurMap.MapWidth = Xlength;
                    CurMap.MapHeight = Ylength;

                    CurMap.InitMap();
                }

                for (int i = 0; i < Xlength; i++)
                {
                    for (int j = 0; j < Ylength; j++)
                    {
                        // is it test die?
                        IDeviceObject device = this.StageSupervisor().WaferObject.GetDevices().Find(index => index.DieIndexM.XIndex == i &&
                                                                                index.DieIndexM.YIndex == j);

                        if (device != null)
                        {
                            if (device.DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                CurMap.SetEnable(i, j, true);
                                CurMap.SetTable(i, j, SelectedPadTableTemplateIndex);
                            }
                        }
                    }
                }

                if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                {
                    // TODO: 변경된 맵을 로더로 전달해야 됨. 사용되는 곳은 Wafer

                    //int index = Wafer.PMIInfo.SelectedNormalPMIMapTemplateIndex;
                    //var PMINormalMap = Wafer.PMIInfo.NormalPMIMapTemplateInfo[index];
                    NormalPMIMapTemplateInfo info = new NormalPMIMapTemplateInfo();

                    info.SelectedNormalPMIMapTemplate = this.SelectedNormalPMIMapTemplate;
                    info.SelectedNormalPMIMapTemplateIndex = this.SelectedNormalPMIMapTemplateIndex;

                    byte[] bytes = SerializeManager.SerializeToByte(info, typeof(NormalPMIMapTemplateInfo));

                    this.LoaderRemoteMediator().GetServiceCallBack()?.UpdatgeNormalPMIMapTemplateInfo(bytes);
                }

                UpdateLabel();

                if (!this.PnPManager().IsActivePageSwithcing())
                {
                    this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [SelectAllTestDieCommand()] : {err}");
            }
            return Task.CompletedTask;
        }




        /// <summary>
        /// N번 프로빙 될 때 마다 PMI하도록 DIE를 선택하는 함수
        /// </summary>
        public async Task SelectNDieProbedCommand()
        {
            try
            {
                var probingseqmodule = this.ProbingSequenceModule();

                if (probingseqmodule.ProbingSequenceCount > 0)
                {


                    //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                    

                    MachineIndex MI = new MachineIndex();

                    int interval = 1;
                    int maxVal = probingseqmodule.ProbingSequenceCount - 1;

                    //CurMapIndex = PMIInfo.GetSelectedNormalPMIMapIndex();

                    //var Xlength = PMIInfo.NormalPMIMapInfo[CurMapIndex].Map.GetLength(0);
                    //var Ylength = PMIInfo.NormalPMIMapInfo[CurMapIndex].Map.GetLength(1);

                    var Xlength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(0);
                    var Ylength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(1);

                    var CurMap = PMIInfo.GetNormalPMIMapTemplate(SelectedNormalPMIMapTemplateIndex);

                    // TODO : 
                    if ((CurMap.MapWidth <= 0) || (CurMap.MapHeight <= 0))
                    {
                        CurMap.MapWidth = Xlength;
                        CurMap.MapHeight = Ylength;

                        CurMap.InitMap();
                    }

                    if ((Xlength > 0) && (Ylength > 0))
                    {
                        interval = VirtualKeyboard.Show(interval, 1, maxVal);

                        for (int i = 0; i < this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PMIPadInfos.Count; i++)
                        {
                            if ((i == 0) || (i % interval == 0))
                            {
                                MI = probingseqmodule.ProbingSeqParameter.ProbingSeq.Value[i];

                                //var SelectedMapPos = PMIInfo.NormalPMIMapInfo[CurMapIndex].Map[MI.XIndex.Value, MI.YIndex.Value];

                                IDeviceObject device = this.StageSupervisor().WaferObject.GetDevices().Find(index => index.DieIndexM.XIndex == MI.XIndex &&
                                                                                index.DieIndexM.YIndex == MI.YIndex);

                                if (device != null)
                                {
                                    if (device.DieType.Value == DieTypeEnum.TEST_DIE)
                                    {
                                        CurMap.SetEnable((int)MI.XIndex, (int)MI.YIndex, true);
                                        CurMap.SetTable((int)MI.XIndex, (int)MI.YIndex, SelectedPadTableTemplateIndex);

                                        //SelectedMapPos.PMIEnable = true;
                                        //SelectedMapPos.PMITableIndex = selectedTableNum;
                                    }
                                }
                            }
                            else
                            {
                                CurMap.SetEnable((int)MI.XIndex, (int)MI.YIndex, false);
                                //PMIInfo.NormalPMIMapInfo[CurMapIndex].Map[MI.XIndex.Value, MI.YIndex.Value].PMIEnable.Value = false;
                            }
                        }
                    }
                }
                else
                {
                    // No Probing Sequence
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                                "Information",
                                                "Please Check Probing Sequence." + "\n"
                                                + "Probing Sequence Count is 0",
                                                EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [SelectNDieProbedCommand()] : {err}");
            }

            
        }

        /// <summary>
        /// Wafer에 PMI map을 설정하는 창을 호출하는 함수
        /// </summary>
        //public async Task WaferMapSetupCommand()
        //{
        //    try
        //    {
        //        //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
        //        //

        //        //await WaferPMIMapSetup.ShowDialogControl();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[NormalPMIMapSetupModule] [WaferMapSetupCommand()] : {err}");
        //    }

        //    
        //}

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(PMIInfo));

                    if (target != null)
                    {
                        (this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).PMIInfo = (PMIInfo)target;

                        SelectedNormalPMIMapTemplateIndex = 0;
                        SelectedNormalPMIMapTemplate = null;

                        if (PMIInfo.NormalPMIMapTemplateInfo.Count > 0)
                        {
                            SelectedNormalPMIMapTemplate = PMIInfo.NormalPMIMapTemplateInfo[SelectedNormalPMIMapTemplateIndex];
                        }

                        //PMIInfo.SelectedWaferTemplateIndex = 0;
                        //PMIInfo.SelectedPadTemplateIndex = 0;
                        //PMIInfo.SelectedNormalPMIMapTemplateIndex = 0;
                        //PMIInfo.SelectedPadTableTemplateIndex = 0;

                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetPackagableParams()
        {
            try
            {
                PMIInfo param = (this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).PMIInfo;

                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(param));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetPMIDieEnableCount()
        {
            int num = 0;

            try
            {
                if (SelectedNormalPMIMapTemplate != null)
                {
                    int Width = SelectedNormalPMIMapTemplate.MapWidth;
                    int Height = SelectedNormalPMIMapTemplate.MapHeight;

                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (SelectedNormalPMIMapTemplate.GetEnable(x, y) == 0x01)
                            {
                                num++;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return num;
        }

        public override void UpdateLabel()
        {
            try
            {
                if(SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    SideViewTextBlocks[0].SideTextContents = $"Current Map Index : [{SelectedNormalPMIMapTemplateIndex + 1}]";
                    SideViewTextBlocks[0].SideTextFontColor = Brushes.Black;
                    SideViewTextBlocks[1].SideTextContents = $"Current Table Index : [{SelectedPadTableTemplateIndex + 1}]";
                    SideViewTextBlocks[1].SideTextFontColor = Brushes.Black;
                    SideViewTextBlocks[2].SideTextContents = $"Number of registered dies : [{GetPMIDieEnableCount()}]";
                    SideViewTextBlocks[2].SideTextFontColor = Brushes.Black;
                }
                else
                {
                    SideViewTextBlocks[0].SideTextContents = $"Current Table Index : [{SelectedPadTableTemplateIndex + 1}]";
                    SideViewTextBlocks[0].SideTextFontColor = Brushes.Black;
                    SideViewTextBlocks[1].SideTextContents = $"Number of registered dies : [{GetPMIDieEnableCount()}]";                   
                    SideViewTextBlocks[1].SideTextFontColor = Brushes.Black;
                }

                //SideViewTextBlock.SideTextContents = ($"Current Map Index : [{SelectedNormalPMIMapTemplateIndex + 1}" + "]\r\n" +
                //                                     ($"Current Table Index : [{SelectedPadTableTemplateIndex + 1}]"));
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalPMIMapSetupModule] [UpdateLabel()] : {err}");
            }
        }

        #endregion
    }
}
