using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using RelayCommandBase;
using SharpDXRender;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UcDisplayPort;

namespace PMIManualResultVM
{
    public class PMIManualResultViewModel : IMainScreenViewModel, INotifyPropertyChanged, IUseLightJog, IHasPMIDrawingGroup, IHasRenderLayer
    {
        public Guid ScreenGUID { get; }
            = new Guid("C104DB0E-AF00-4760-B557-601C5B7B6A26");

        public bool Initialized { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #region ==> Property

        public IPMIInfo PMIInfo
        {
            get { return this.GetParam_Wafer().GetSubsInfo().GetPMIInfo(); }
        }

        private IProberStation _Prober;
        public IProberStation Prober
        {
            get { return _Prober; }
            set { _Prober = value; }
        }
        public IWaferObject WaferObject { get { return this.GetParam_Wafer(); } }

        public IProbeCard ProbeCardObject { get { return this.GetParam_ProbeCard(); } }

        public PadGroup PadInfos
        {
            get { return this.GetParam_Wafer().GetSubsInfo().Pads; }
        }

        public DeviceObject CurrentDie
        {
            get { return this.GetParam_Wafer().GetSubsInfo().CurrentDie; }
        }

        public LightJogViewModel LightJogVM { get; set; }

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

        #region ==> MainViewTarget
        private object _MainViewTarget;
        public object MainViewTarget
        {
            get { return _MainViewTarget; }
            set
            {
                if (value != _MainViewTarget)
                {
                    _MainViewTarget = value;
                    if (_MainViewTarget is IWaferObject)
                    {
                        MainViewZoomVisibility = true;
                        LightJogVisibility = false;
                    }
                    else if (_MainViewTarget is IDisplayPort)
                    {
                        MainViewZoomVisibility = false;
                        LightJogVisibility = true;
                    }

                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MiniViewTarget
        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    _MiniViewTarget = value;
                    if (_MiniViewTarget is IDisplayPort)
                    {
                        MiniViewZoomVisibility = false;
                    }
                    else if (_MiniViewTarget is IWaferObject)
                    {
                        MiniViewZoomVisibility = true;
                    }

                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _SideViewTargetVisibility;
        public bool SideViewTargetVisibility
        {
            get { return _SideViewTargetVisibility; }
            set
            {
                if (value != _SideViewTargetVisibility)
                {
                    _SideViewTargetVisibility = value;

                    if (_SideViewTargetVisibility == false)
                    {
                        SideViewSwitchVisibility = false;
                    }
                    else
                    {
                        if ((SideViewDisplayMode != SideViewMode.EXPANDER_ONLY) ||
                            (SideViewDisplayMode != SideViewMode.TEXTBLOCK_ONLY))
                        {
                            SideViewSwitchVisibility = true;
                        }
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MiniViewTargetVisibility;
        public bool MiniViewTargetVisibility
        {
            get { return _MiniViewTargetVisibility; }
            set
            {
                if (value != _MiniViewTargetVisibility)
                {
                    _MiniViewTargetVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MiniViewSwapVisibility;
        public bool MiniViewSwapVisibility
        {
            get { return _MiniViewSwapVisibility; }
            set
            {
                if (value != _MiniViewSwapVisibility)
                {
                    _MiniViewSwapVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MainViewZoomVisibility;
        public bool MainViewZoomVisibility
        {
            get { return _MainViewZoomVisibility; }
            set
            {
                if (value != _MainViewZoomVisibility)
                {
                    _MainViewZoomVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MiniViewZoomVisibility;
        public bool MiniViewZoomVisibility
        {
            get { return _MiniViewZoomVisibility; }
            set
            {
                if (value != _MiniViewZoomVisibility)
                {
                    _MiniViewZoomVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LightJogVisibility;
        public bool LightJogVisibility
        {
            get { return _LightJogVisibility; }
            set
            {
                if (value != _LightJogVisibility)
                {
                    _LightJogVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SideViewSwitchVisibility;
        public bool SideViewSwitchVisibility
        {
            get { return _SideViewSwitchVisibility; }
            set
            {
                if (value != _SideViewSwitchVisibility)
                {
                    _SideViewSwitchVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SideViewTextVisibility;
        public bool SideViewTextVisibility
        {
            get { return _SideViewTextVisibility; }
            set
            {
                if (value != _SideViewTextVisibility)
                {
                    _SideViewTextVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RenderLayer _SharpDXLayer;
        public RenderLayer SharpDXLayer
        {
            get { return _SharpDXLayer; }
            set
            {
                if (value != _SharpDXLayer)
                {
                    _SharpDXLayer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                ICamera precam = _CurCam;
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
                ConfirmDisplay(precam, _CurCam);
            }
        }

        private bool _LightJogEnable = true;
        public bool LightJogEnable
        {
            get { return _LightJogEnable; }
            set
            {
                if (value != _LightJogEnable)
                {
                    _LightJogEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PadResultMoveMode _PadResultMoveModeEnum;
        public PadResultMoveMode PadResultMoveModeEnum
        {
            get { return _PadResultMoveModeEnum; }
            set
            {
                if (value != _PadResultMoveModeEnum)
                {
                    _PadResultMoveModeEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public SideViewExpanderDescriptor ExpanderItem_01 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_02 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_03 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_04 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_05 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_06 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_07 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_08 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_09 { get; set; }
        //public SideViewExpanderDescriptor ExpanderItem_10 { get; set; }

        //public SideViewTextBlockDescriptor _SideViewTextBlock = new SideViewTextBlockDescriptor();
        //public SideViewTextBlockDescriptor SideViewTextBlock
        //{
        //    get { return _SideViewTextBlock; }
        //    set { _SideViewTextBlock = value; }
        //}

        //private string _SideViewTitle;
        //public string SideViewTitle
        //{
        //    get { return _SideViewTitle; }
        //    set
        //    {
        //        if (value != _SideViewTitle)
        //        {
        //            _SideViewTitle = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _IsAllPadOverlay;
        public bool IsAllPadOverlay
        {
            get { return _IsAllPadOverlay; }
            set
            {
                if (value != _IsAllPadOverlay)
                {
                    _IsAllPadOverlay = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsFailPadOverlay;
        public bool IsFailPadOverlay
        {
            get { return _IsFailPadOverlay; }
            set
            {
                if (value != _IsFailPadOverlay)
                {
                    _IsFailPadOverlay = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsOverlayToPad;
        public bool IsOverlayToPad
        {
            get { return _IsOverlayToPad; }
            set
            {
                if (value != _IsOverlayToPad)
                {
                    _IsOverlayToPad = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ProbeMarkInform _ProbeMarkInform;
        public ProbeMarkInform ProbeMarkInform
        {
            get { return _ProbeMarkInform; }
            set
            {
                if (value != _ProbeMarkInform)
                {
                    _ProbeMarkInform = value;
                    RaisePropertyChanged(nameof(ProbeMarkInform));
                }
            }
        }

        private PMIResultSummary _PMIResultSummary;
        public PMIResultSummary PMIResultSummary
        {
            get { return _PMIResultSummary; }
            set
            {
                if (value != _PMIResultSummary)
                {
                    _PMIResultSummary = value;
                    RaisePropertyChanged(nameof(PMIResultSummary));
                }
            }
        }
        public IStageSupervisor StageSupervisor { get; set; }
        public ICoordinateManager CoordManager { get; set; }
        public IPnpManager PnpManager { get; set; }

        public bool KeppCamOverlay = true;

        private void ConfirmDisplay(ICamera precam, ICamera curcam)
        {
            try
            {
                if (precam != null && precam.DrawDisplayDelegate != null && KeppCamOverlay)
                {
                    curcam.DrawDisplayDelegate += precam.DrawDisplayDelegate;
                    precam.InDrawOverlayDisplay();
                }
                else if (curcam.DrawDisplayDelegate != null)
                    curcam.UpdateOverlayFlag = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> ZoomObject
        private IZoomObject _ZoomObject;
        public IZoomObject ZoomObject
        {
            get { return this.StageSupervisor().WaferObject;/*return _ZoomObject*/}
            set
            {
                if (value != _ZoomObject)
                {
                    _ZoomObject = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ViewSwapCommand
        //==> Main View와 Mini View를 Swap 하기 위한 버튼과의 Binding Command
        private RelayCommand<object> _ViewSwapCommand;
        public ICommand ViewSwapCommand
        {
            get
            {
                if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
                return _ViewSwapCommand;
            }
        }
        private void ViewSwapFunc(object parameter)
        {
            try
            {
                object swap = MainViewTarget;
                //MainViewTarget = WaferObject;
                MainViewTarget = MiniViewTarget;
                MiniViewTarget = swap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> PlusCommand
        private RelayCommand _PlusCommand;
        public ICommand PlusCommand
        {
            get
            {
                if (null == _PlusCommand) _PlusCommand = new RelayCommand(Plus);
                return _PlusCommand;
            }
        }
        public virtual void Plus()
        {
            try
            {
                string Plus = string.Empty;
                if (ZoomObject != null)
                    ZoomObject.ZoomLevel--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> MinusCommand
        private RelayCommand _MinusCommand;
        public ICommand MinusCommand
        {
            get
            {
                if (null == _MinusCommand) _MinusCommand = new RelayCommand(Minus);
                return _MinusCommand;
            }
        }
        public virtual void Minus()
        {
            try
            {
                string Minus = string.Empty;
                if (ZoomObject != null)
                    ZoomObject.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> SetZeroCommand
        private RelayCommand<object> _SetZeroCommand;
        public ICommand SetZeroCommand
        {
            get
            {
                if (null == _SetZeroCommand) _SetZeroCommand = new RelayCommand<object>(SetZero);
                return _SetZeroCommand;
            }
        }

        private void SetZero(object parameter)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //#region ==> SetZeroCommand
        //private RelayCommand<object> _SetZeroCommand;
        //public ICommand SetZeroCommand
        //{
        //    get
        //    {
        //        if (null == _SetZeroCommand) _SetZeroCommand = new RelayCommand<object>(SetZero);
        //        return _SetZeroCommand;
        //    }
        //}

        //private void SetZero(object parameter)
        //{
        //    try
        //    {
        //        if (this.CoordinateManager() != null)
        //        {
        //            //_PosCoord = DisplayPort.StageSuperVisor.
        //            //    CoordinateManager.StageCoordConvertToUserCoord(DisplayPort.AssignedCamera.Param.ChannelType);
        //            double zeroposx = 0.0;
        //            double zeroposy = 0.0;
        //            double zeroposz = 0.0;

        //            this.MotionManager().GetActualPos(EnumAxisConstants.X, ref zeroposx);
        //            ((UcDisplayPort.DisplayPort)DisplayPort).ZeroPosX = zeroposx;

        //            this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref zeroposy);
        //            ((UcDisplayPort.DisplayPort)DisplayPort).ZeroPosY = zeroposy;

        //            this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zeroposz);
        //            ((UcDisplayPort.DisplayPort)DisplayPort).ZeroPosZ = zeroposz;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        
        //    }
        //}
        //#endregion

        private RelayCommand _ChangeSideViewVisibleCommand;
        public ICommand ChangeSideViewVisibleCommand
        {
            get
            {
                if (null == _ChangeSideViewVisibleCommand) _ChangeSideViewVisibleCommand = new RelayCommand(ChangeSideViewVisible);
                return _ChangeSideViewVisibleCommand;
            }
        }

        public virtual void ChangeSideViewVisible()
        {
            try
            {
                if (SideViewDisplayMode == SideViewMode.EXPANDER_MODE)
                {
                    SideViewDisplayMode = SideViewMode.TEXTBLOCK_MODE;
                    //SideViewExpanderVisibility = false;
                    SideViewTextVisibility = true;
                }
                else if (SideViewDisplayMode == SideViewMode.TEXTBLOCK_MODE)
                {
                    SideViewDisplayMode = SideViewMode.EXPANDER_MODE;
                    //SideViewExpanderVisibility = true;
                    SideViewTextVisibility = false;
                }
                //else if ((SideViewDisplayMode == SideViewMode.EXPANDER_ONLY) || 
                //         (SideViewDisplayMode == SideViewMode.TEXTBLOCK_ONLY))
                //{                    
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<SETUP_DIRECTION> _PadMoveCommand;
        public ICommand PadMoveCommand
        {
            get
            {
                if (null == _PadMoveCommand) _PadMoveCommand = new RelayCommand<SETUP_DIRECTION>(PadMoveCmd);
                return _PadMoveCommand;
            }
        }

        private void PadMoveCmd(SETUP_DIRECTION dir)
        {
            //try
            //{
            //    if (PadInfos.PMIPadInfos.Count > 0)
            //    {
            //        switch ((SETUP_DIRECTION)dir)
            //        {
            //            case SETUP_DIRECTION.PREV:

            //                if (PadInfos.SelectedPMIPadIndex > 0)
            //                {
            //                    PadInfos.SelectedPMIPadIndex--;
            //                }
            //                else
            //                {
            //                    PadInfos.SelectedPMIPadIndex = PadInfos.PMIPadInfos.Count - 1;
            //                }
            //                break;
            //            case SETUP_DIRECTION.NEXT:

            //                if (PadInfos.SelectedPMIPadIndex + 1 < PadInfos.PMIPadInfos.Count)
            //                {
            //                    PadInfos.SelectedPMIPadIndex++;
            //                }
            //                else
            //                {
            //                    PadInfos.SelectedPMIPadIndex = 0;
            //                }

            //                break;
            //            default:
            //                break;
            //        }

            //        this.PMIModule().MoveToPad(this.CurCam, PadInfos.SelectedPMIPadIndex);

            //        //UpdateOverlay();
            //    }
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}
        }

        public enum SideViewMode
        {
            TEXTBLOCK_MODE,
            EXPANDER_MODE,
            TEXTBLOCK_ONLY,
            EXPANDER_ONLY,
            NOUSE
        }

        private SideViewMode _SideViewDisplayMode;
        public SideViewMode SideViewDisplayMode
        {
            get { return _SideViewDisplayMode; }
            set
            {
                if (value != _SideViewDisplayMode)
                {
                    _SideViewDisplayMode = value;

                    if (_SideViewDisplayMode == SideViewMode.EXPANDER_MODE)
                    {
                        try
                        {
                            //SideViewExpanderVisibility = false;
                            SideViewTextVisibility = true;
                            SideViewSwitchVisibility = true;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            
                        }
                    }
                    else if (_SideViewDisplayMode == SideViewMode.TEXTBLOCK_MODE)
                    {
                        try
                        {
                            //SideViewExpanderVisibility = true;
                            SideViewTextVisibility = false;
                            SideViewSwitchVisibility = true;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            
                        }
                    }
                    else if (_SideViewDisplayMode == SideViewMode.EXPANDER_ONLY)
                    {
                        try
                        {
                            //SideViewExpanderVisibility = false;
                            SideViewTextVisibility = true;
                            SideViewSwitchVisibility = false;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            
                        }
                    }
                    else if (_SideViewDisplayMode == SideViewMode.TEXTBLOCK_ONLY)
                    {
                        try
                        {
                            //SideViewExpanderVisibility = true;
                            SideViewTextVisibility = false;
                            SideViewSwitchVisibility = false;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            
                        }
                    }

                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                UpdateOverlayDelegate(DELEGATEONOFF.OFF);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void DeInitModule()
        {
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        enum StageCam
        {
            WAFER_HIGH_CAM,
            WAFER_LOW_CAM,
            PIN_HIGH_CAM,
            PIN_LOW_CAM,
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    StageSupervisor = this.StageSupervisor();
                    CoordManager = this.CoordinateManager();
                    PnpManager = this.PnPManager();

                    LightJogVM = new LightJogViewModel(
                        maxLightValue: 255,
                        minLightValue: 0);

                    DrawingGroup = new PMIDrawingGroup();
                    DrawingGroup.RegisterdPad = true;
                    DrawingGroup.RegisterdPadIndex = true;
                    DrawingGroup.DetectedMark = true;

                    if(this.PMIModule() != null)
                    {
                        SharpDXLayer = this.PMIModule().InitPMIRenderLayer(this.PMIModule().GetLayerSize(), 0, 0, 0, 0);
                        SharpDXLayer?.Init();
                    }
                    
                    if (DisplayPort == null)
                    {
                        DisplayPort dpPort = new DisplayPort() { GUID = new Guid("B557D2A5-AA68-419A-8B67-14BFC5B6AB75") };
                        dpPort.DataContext = this;
                        DisplayPort = dpPort;

                        ((UcDisplayPort.DisplayPort)DisplayPort).RenderLayer = SharpDXLayer;

                        Binding bindRenderLayer = new Binding
                        {
                            Path = new System.Windows.PropertyPath("SharpDXLayer"),
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        };
                        BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.RenderLayerProperty, bindRenderLayer);
                    }

                    MainViewTarget = DisplayPort;

                    Initialized = true;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            { 
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.PMIModule().SetSubModule(this);

                MiniViewTarget = this.StageSupervisor().WaferObject;

                this.SideViewTargetVisibility = true;
                this.MiniViewTargetVisibility = true;
                this.LightJogVisibility = true;
                this.MainViewZoomVisibility = true;

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, 85);
                }

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                LightJogVM.InitCameraJog(this, CurCam.GetChannelType());

                // If First PMI Index value is valid, Move to the First PMI Procesed Position.

                //MachineIndex PMIProcesedFirstIndex = this.PMIModule().DoPMIInfo.FirstPMIIndex;
                MachineIndex PMIProcesedFirstIndex;

                Point MovePos = new Point();
                WaferCoordinate DieLeftCornerPos = new WaferCoordinate();
                //WaferCoordinate DieCenterPos = new WaferCoordinate();

                if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                {
                    DieLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner(this.PMIModule().DoPMIInfo.ProcessedPMIMIndex[0].XIndex, this.PMIModule().DoPMIInfo.ProcessedPMIMIndex[0].YIndex);
                    //DieCenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(PMIProcesedFirstIndex.XIndex, PMIProcesedFirstIndex.YIndex);

                    if (PadInfos.PMIPadInfos.Count > 0)
                    {
                        PMIPadObject FirstPad = PadInfos.PMIPadInfos[0];

                        MovePos.X = DieLeftCornerPos.X.Value + FirstPad.PadCenter.X.Value;
                        MovePos.Y = DieLeftCornerPos.Y.Value + FirstPad.PadCenter.Y.Value;
                    }
                    else
                    {
                        MovePos.X = DieLeftCornerPos.X.Value;
                        MovePos.Y = DieLeftCornerPos.Y.Value;
                    }
                }
                else
                {
                    MachineIndex CenterDieIndex = new MachineIndex();

                    CenterDieIndex.XIndex = this.GetParam_Wafer().GetPhysInfo().CenM.XIndex.Value;
                    CenterDieIndex.YIndex = this.GetParam_Wafer().GetPhysInfo().CenM.YIndex.Value;

                    DieLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner(CenterDieIndex.XIndex, CenterDieIndex.YIndex);
                    //DieCenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(CenterDieIndex.XIndex, CenterDieIndex.YIndex);

                    if (PadInfos.PMIPadInfos.Count > 0)
                    {
                        PMIPadObject FirstPad = PadInfos.PMIPadInfos[0];

                        MovePos.X = DieLeftCornerPos.X.Value + FirstPad.PadCenter.X.Value;
                        MovePos.Y = DieLeftCornerPos.Y.Value + FirstPad.PadCenter.Y.Value;
                    }
                    else
                    {
                        MovePos.X = DieLeftCornerPos.X.Value;
                        MovePos.Y = DieLeftCornerPos.Y.Value;
                    }
                }

                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(
                        MovePos.X,
                        MovePos.Y
                        , this.GetParam_Wafer().GetSubsInfo().ActualThickness);
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(
                        MovePos.X,
                        MovePos.Y
                        , this.GetParam_Wafer().GetSubsInfo().ActualThickness);
                }

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                UpdateOverlayDelegate(DELEGATEONOFF.ON);

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void UpdateOverlay()
        {
            try
            {

                try
                {
                    this.PMIModule().UpdateDisplayedDevices(this.CurCam);
                    this.PMIModule().UpdateCurrentPadIndex();
                    this.PMIModule().UpdateRenderLayer();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateOverlayDelegate(DELEGATEONOFF flag)
        {
            try
            {
                if (flag == DELEGATEONOFF.ON)
                {
                    this.CoordinateManager().OverlayUpdateDelegate += UpdateOverlay;
                }
                else
                {
                    this.CoordinateManager().OverlayUpdateDelegate -= UpdateOverlay;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
