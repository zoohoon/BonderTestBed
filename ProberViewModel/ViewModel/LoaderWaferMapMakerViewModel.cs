using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderWaferMapMakerViewModelModule
{
    using Autofac;
    using JogViewModelModule;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LogModule;
    using MaterialDesignExtensions.Controls;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.Loader;
    using ProberInterfaces.Param;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using ServiceProxy;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using UcDisplayPort;
    using VirtualKeyboardControl;
    using WA_HighMagParameter_Standard;

    public class LoaderWaferMapMakerViewModel : IMainScreenViewModel, IWaferMapMakerVM, IUseLightJog
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private IRemoteMediumProxy RemoteMiduumProxy { get; set; }

        #region //..BindingProperty
        private HeightPointEnum _HeightPoint;
        public HeightPointEnum HeightPoint
        {
            get { return _HeightPoint; }
            set
            {
                if (value != _HeightPoint)
                {
                    _HeightPoint = value;
                    RaisePropertyChanged();
                }
            }

        }

        private ObservableCollection<EnumWaferSize> _WaferSizeEnums
        = new ObservableCollection<EnumWaferSize>();
        public ObservableCollection<EnumWaferSize> WaferSizeEnums
        {
            get { return _WaferSizeEnums; }
            set
            {
                if (value != _WaferSizeEnums)
                {
                    _WaferSizeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<WaferSubstrateTypeEnum> _WaferSubstrateTypeEnums
        = new ObservableCollection<WaferSubstrateTypeEnum>();
        public ObservableCollection<WaferSubstrateTypeEnum> WaferSubstrateTypeEnums
        {
            get { return _WaferSubstrateTypeEnums; }
            set
            {
                if (value != _WaferSubstrateTypeEnums)
                {
                    _WaferSubstrateTypeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }


        private WaferSubstrateTypeEnum _WaferSubstrateType
                = new WaferSubstrateTypeEnum();
        public WaferSubstrateTypeEnum WaferSubstrateType
        {
            get { return _WaferSubstrateType; }
            set
            {
                if (value != _WaferSubstrateType)
                {
                    _WaferSubstrateType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<WaferNotchTypeEnum> _WafrNotchTypeEnums
             = new ObservableCollection<WaferNotchTypeEnum>();
        public ObservableCollection<WaferNotchTypeEnum> WafrNotchTypeEnums
        {
            get { return _WafrNotchTypeEnums; }
            set
            {
                if (value != _WafrNotchTypeEnums)
                {
                    _WafrNotchTypeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<double> _NotchAngles = new ObservableCollection<double>();
        public ObservableCollection<double> NotchAngles
        {
            get { return _NotchAngles; }
            set
            {
                if (value != _NotchAngles)
                {
                    _NotchAngles = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NotchAngle;
        public double NotchAngle
        {
            get { return _NotchAngle; }
            set
            {
                if (value != _NotchAngle)
                {
                    _NotchAngle = value;
                    RaisePropertyChanged();
                    Wafer.GetPhysInfo().NotchAngle.Value = NotchAngle;
                    //RemoteMiduumProxy.WaferMapMaker_NotchAngle(NotchAngle);
                }
            }
        }

        private double _NotchAngleOffset;
        public double NotchAngleOffset
        {
            get { return _NotchAngleOffset; }
            set
            {
                if (value != _NotchAngleOffset)
                {
                    _NotchAngleOffset = value;
                    RaisePropertyChanged();
                    Wafer.GetPhysInfo().NotchAngleOffset.Value = NotchAngleOffset;
                }
            }
        }

        private DispFlipEnum _DispHorFlip = DispFlipEnum.NONE;

        public DispFlipEnum DispHorFlip
        {
            get { return _DispHorFlip; }
            set
            {
                _DispHorFlip = value;
                RaisePropertyChanged();
            }
        }

        private DispFlipEnum _DispVerFlip = DispFlipEnum.NONE;

        public DispFlipEnum DispVerFlip
        {
            get { return _DispVerFlip; }
            set
            {
                _DispVerFlip = value;
                RaisePropertyChanged();
            }
        }


        private WaferNotchTypeEnum _NotchType;
        public WaferNotchTypeEnum NotchType
        {
            get { return _NotchType; }
            set
            {
                if (value != _NotchType)
                {
                    _NotchType = value;
                    RaisePropertyChanged();
                    Wafer.GetPhysInfo().NotchType.Value = NotchType.ToString();
                    //RemoteMiduumProxy.WaferMapMaker_NotchType(NotchType);
                }
            }
        }

        private double _DieSizeX;
        public double DieSizeX
        {
            get { return _DieSizeX; }
            set
            {
                if (value != _DieSizeX)
                {
                    _DieSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DieSizeY;
        public double DieSizeY
        {
            get { return _DieSizeY; }
            set
            {
                if (value != _DieSizeY)
                {
                    _DieSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Thickness;
        public double Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _EdgeMargin;
        public double EdgeMargin
        {
            get { return _EdgeMargin; }
            set
            {
                if (value != _EdgeMargin)
                {
                    _EdgeMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _OrgMachineXCoord;
        public long OrgMachineXCoord
        {
            get { return _OrgMachineXCoord; }
            set
            {
                if (value != _OrgMachineXCoord)
                {
                    _OrgMachineXCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _OrgMachineYCoord;
        public long OrgMachineYCoord
        {
            get { return _OrgMachineYCoord; }
            set
            {
                if (value != _OrgMachineYCoord)
                {
                    _OrgMachineYCoord = value;
                    RaisePropertyChanged();
                }
            }
        }


        private long _OrgUserXCoord;
        public long OrgUserXCoord
        {
            get { return _OrgUserXCoord; }
            set
            {
                if (value != _OrgUserXCoord)
                {
                    _OrgUserXCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _OrgUserYCoord;
        public long OrgUserYCoord
        {
            get { return _OrgUserYCoord; }
            set
            {
                if (value != _OrgUserYCoord)
                {
                    _OrgUserYCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _MapIndexX;
        public long MapIndexX
        {
            get { return _MapIndexX; }
            set
            {
                if (value != _MapIndexX)
                {
                    _MapIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MapIndexY;
        public long MapIndexY
        {
            get { return _MapIndexY; }
            set
            {
                if (value != _MapIndexY)
                {
                    _MapIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }
        

        private double _WaferSize_Offset_um;
        public double WaferSize_Offset_um
        {
            get { return _WaferSize_Offset_um; }
            set
            {
                if (value != _WaferSize_Offset_um)
                {
                    _WaferSize_Offset_um = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferSize _WaferSize;
        public EnumWaferSize WaferSize
        {
            get { return _WaferSize; }
            set
            {
                if (value != _WaferSize)
                {
                    _WaferSize = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ICoordinateManager CoordinateManager
        {
            get { return this.CoordinateManager(); }
            set { CoordinateManager = value; }
        }

        private ObservableCollection<HeightPointEnum> _EnumHeightPoints
             = new ObservableCollection<HeightPointEnum>();
        public ObservableCollection<HeightPointEnum> EnumHeightPoints
        {
            get { return _EnumHeightPoints; }
            set
            {
                if (value != _EnumHeightPoints)
                {
                    _EnumHeightPoints = value;
                    RaisePropertyChanged();
                }
            }

        }

        private bool _IsCanChangeWaferSize = true;
        public bool IsCanChangeWaferSize
        {
            get { return _IsCanChangeWaferSize; }
            set
            {
                if (value != _IsCanChangeWaferSize)
                {
                    _IsCanChangeWaferSize = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        #region //..View

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return (this.ViewModelManager() as ILoaderViewModelManager).Camera; }
            set
            {
                _CurCam = value;
                RaisePropertyChanged();
            }
        }

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
                        LightJogVisibility = Visibility.Hidden;
                        MiniViewHorizontalAlignment = HorizontalAlignment.Left;
                        MiniViewVerticalAlignment = VerticalAlignment.Bottom;
                    }

                    else if (_MainViewTarget is IDisplayPort)
                    {
                        LightJogVisibility = Visibility.Visible;
                        MiniViewHorizontalAlignment = HorizontalAlignment.Right;
                        MiniViewVerticalAlignment = VerticalAlignment.Top;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    _MiniViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HorizontalAlignment _MiniViewHorizontalAlignment
            = HorizontalAlignment.Left;
        public HorizontalAlignment MiniViewHorizontalAlignment
        {
            get { return _MiniViewHorizontalAlignment; }
            set
            {
                if (value != _MiniViewHorizontalAlignment)
                {
                    _MiniViewHorizontalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }


        private VerticalAlignment _MiniViewVerticalAlignment
             = VerticalAlignment.Bottom;
        public VerticalAlignment MiniViewVerticalAlignment
        {
            get { return _MiniViewVerticalAlignment; }
            set
            {
                if (value != _MiniViewVerticalAlignment)
                {
                    _MiniViewVerticalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HorizontalAlignment _LightJogVerticalAlignment
           = HorizontalAlignment.Center;
        public HorizontalAlignment LightJogVerticalAlignment
        {
            get { return _LightJogVerticalAlignment; }
            set
            {
                if (value != _LightJogVerticalAlignment)
                {
                    _LightJogVerticalAlignment = value;
                    RaisePropertyChanged();
                }
            }
        }
        private VerticalAlignment _LightJogHorizontalAlignmentt
             = VerticalAlignment.Bottom;
        public VerticalAlignment LightJogHorizontalAlignmentt
        {
            get { return _LightJogHorizontalAlignmentt; }
            set
            {
                if (value != _LightJogHorizontalAlignmentt)
                {
                    _LightJogHorizontalAlignmentt = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Visibility _LightJogVisibility;
        public Visibility LightJogVisibility
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

        private IDisplayPort _DisplayPort;

        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set { _DisplayPort = value; }
        }

        public LoaderLightJogViewModel LightJog { get; set; }
        public IHexagonJogViewModel MotionJog { get; set; }

        public IStageSupervisor StageSupervisor => this.StageSupervisor();

        private IWaferObject _Wafer;
        public IWaferObject Wafer
        {
            get { return _Wafer; }
            set
            {
                if (value != _Wafer)
                {
                    _Wafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IPhysicalInfo _PhysicalInfo;
        public IPhysicalInfo PhysicalInfo
        {
            get { return _PhysicalInfo; }
            set
            {
                if (value != _PhysicalInfo)
                {
                    _PhysicalInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #endregion


        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("156C4231-360D-138C-ED86-6E586C45F359");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        public LoaderWaferMapMakerViewModel()
        {
            foreach (HeightPointEnum hpoint in Enum.GetValues(typeof(HeightPointEnum)))
            {
                if (hpoint != HeightPointEnum.UNDEFINED & hpoint != HeightPointEnum.INVALID)
                {
                    //combo_HeightPoint.Items.Add(hpoint);
                    EnumHeightPoints.Add(hpoint);
                }
            }
        }
        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        public EventCodeEnum InitModule()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public async Task<EventCodeEnum> InitViewModel()
        {
            foreach (var waferenum in Enum.GetValues(typeof(EnumWaferSize)))
            {
                if (EnumWaferSize.INVALID != (EnumWaferSize)waferenum & EnumWaferSize.UNDEFINED != (EnumWaferSize)waferenum)
                {
                    WaferSizeEnums.Add((EnumWaferSize)waferenum);
                }
            }

            foreach (var waferenum in Enum.GetValues(typeof(WaferSubstrateTypeEnum)))
            {
                if (WaferSubstrateTypeEnum.UNDEFINED != (WaferSubstrateTypeEnum)waferenum & 
                    WaferSubstrateTypeEnum.Framed != (WaferSubstrateTypeEnum)waferenum &
                    WaferSubstrateTypeEnum.Strip != (WaferSubstrateTypeEnum)waferenum &
                    WaferSubstrateTypeEnum.Tray != (WaferSubstrateTypeEnum)waferenum)
                {
                    WaferSubstrateTypeEnums.Add((WaferSubstrateTypeEnum)waferenum);
                }
            }

            foreach (var notchtype in Enum.GetValues(typeof(WaferNotchTypeEnum)))
            {
                if (WaferNotchTypeEnum.UNKNOWN != (WaferNotchTypeEnum)notchtype)
                {
                    WafrNotchTypeEnums.Add((WaferNotchTypeEnum)notchtype);
                }
            }
            DisplayPort = new DisplayPort() { GUID = new Guid("2241DB27-DB99-1F16-33C3-C23F88E468DA") };
            ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
            //(this.ViewModelManager() as ILoaderViewModelManager).RegisteDisplayPort(DisplayPort);

            Binding bindX = new Binding
            {
                Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosX"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToX, bindX);

            Binding bindY = new Binding
            {
                Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosY"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToY, bindY);

            Binding bindCamera = new Binding
            {
                Path = new System.Windows.PropertyPath("CurCam"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);


            LightJog = new LoaderLightJogViewModel(
                   maxLightValue: 255,
                   minLightValue: 0, typeof(IWaferMapMakerVM).Name);
            LightJog.SetContainer(this.GetLoaderContainer());

            MotionJog = this.PnPManager().PnpMotionJog;

            NotchAngles.Add(0);
            NotchAngles.Add(90);
            NotchAngles.Add(180);
            NotchAngles.Add(270);

            return EventCodeEnum.NONE;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                RemoteMiduumProxy = _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
                await RemoteMiduumProxy.PageSwitched(this.ViewModelManager().GetViewGuidFromViewModelGuid(this.ScreenGUID));

                if (this._LoaderCommunicationManager.SelectedStage != null)
                {
                    var Loader = this.GetLoaderContainer().Resolve<ILoaderModule>();
                    IAttachedModule chuckModule = Loader.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, this._LoaderCommunicationManager.SelectedStage.Index);
                    IWaferOwnable ownable = chuckModule as IWaferOwnable;
                    if (ownable != null && ownable.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        IsCanChangeWaferSize = false;
                    }
                    else 
                    {
                        IsCanChangeWaferSize = true;
                    }
                }

                CurCam = (this.ViewModelManager() as ILoaderViewModelManager).Camera;

                InitData();

                Task task = new Task(() =>
                {
                    HeightPoint = RemoteMiduumProxy.WaferMapMakerVM_GetHeightProfiling();

                    if (CurCam != null)
                        LightJog.InitCameraJog(this, CurCam.GetChannelType());
                });
                task.Start();
                await task;

                Wafer.MapViewAssignCamType.Value = CurCam.GetChannelType();
                Wafer.MapViewCurIndexVisiablity = true;

                this.VisionManager().SetDisplayChannel(null, DisplayPort);

                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += UpdateMapIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }

        private async Task UpdateMapIndex(object newVal)
        {

            try
            {
                MachineIndex MI = new MachineIndex();
                MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();

                this.MapIndexX = MI.XIndex;
                this.MapIndexY = MI.YIndex;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayDie() Error occurred.");
                LoggerManager.Exception(err);
            }
        }

        private void InitData()
        {
            try
            {
                Wafer = this.StageSupervisor().WaferObject;
                PhysicalInfo = Wafer.GetPhysInfo();
                if (Wafer != null)
                {
                    DieSizeX = Wafer.GetPhysInfo().DieSizeX.Value;
                    DieSizeY = Wafer.GetPhysInfo().DieSizeY.Value;
                    NotchAngle = Wafer.GetPhysInfo().NotchAngle.Value;

                    NotchAngleOffset = Wafer.GetPhysInfo().NotchAngleOffset.Value;

                    if (Wafer.GetPhysInfo().WaferSize_um.Value == 150000)
                    {
                        Wafer.GetPhysInfo().WaferSizeEnum = EnumWaferSize.INCH6;
                    }
                    else if (Wafer.GetPhysInfo().WaferSize_um.Value == 200000)
                    {
                        Wafer.GetPhysInfo().WaferSizeEnum = EnumWaferSize.INCH8;
                    }
                    else if (Wafer.GetPhysInfo().WaferSize_um.Value == 300000)
                    {
                        Wafer.GetPhysInfo().WaferSizeEnum = EnumWaferSize.INCH12;
                    }
                    WaferSize_Offset_um = Wafer.GetPhysInfo().WaferSize_Offset_um.Value;
                    WaferSize = Wafer.GetPhysInfo().WaferSizeEnum;
                    WaferSubstrateType = Wafer.GetPhysInfo().WaferSubstrateType.Value;
                    EdgeMargin = Wafer.GetPhysInfo().WaferMargin_um.Value;
                    if (Wafer.GetPhysInfo().NotchType.Value == WaferNotchTypeEnum.FLAT.ToString())
                        NotchType = WaferNotchTypeEnum.FLAT;
                    else if (Wafer.GetPhysInfo().NotchType.Value == WaferNotchTypeEnum.NOTCH.ToString())
                        NotchType = WaferNotchTypeEnum.NOTCH;
                    Thickness = Wafer.GetPhysInfo().Thickness.Value;
                    OrgMachineXCoord = Wafer.GetPhysInfo().OrgM.XIndex.Value;
                    OrgMachineYCoord = Wafer.GetPhysInfo().OrgM.YIndex.Value;
                    OrgUserXCoord = Wafer.GetPhysInfo().OrgU.XIndex.Value;
                    OrgUserYCoord = Wafer.GetPhysInfo().OrgU.YIndex.Value;

                    MapIndexX = Wafer.GetPhysInfo().CenM.XIndex.Value;
                    MapIndexY = Wafer.GetPhysInfo().CenM.YIndex.Value;
                    this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Wafer.GetPhysInfo().Thickness.Value);

                    MainViewTarget = null;
                    MainViewTarget = Wafer;
                    MiniViewTarget = DisplayPort;

                    Wafer.MapViewStageSyncEnable = true;

                    DispHorFlip = this.VisionManager().GetDispHorFlip();
                    DispVerFlip = this.VisionManager().GetDispVerFlip();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task<EventCodeEnum> Cleanup(object parameter = null)
        //{
        //    return EventCodeEnum.NONE;
        //}

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                string errorReason = "";
                if (VerifyParameter(Wafer.GetPhysInfo().WaferSizeEnum, WaferSize_Offset_um.ToString(), ref errorReason) == false)
                {
                    Wafer.GetPhysInfo().WaferSize_Offset_um.Value = 0;
                    WaferSize_Offset_um = 0;
                    this.MetroDialogManager().ShowMessageDialog("Error", $"{errorReason}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                await RemoteMiduumProxy.WaferMapMaker_Cleanup();

                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= UpdateMapIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        private bool VerifyParameter(EnumWaferSize WaferSize, string WaferSize_Offset_um, ref string errorReasonStr)
        {
            bool ret = true;
            try
            {
                double maxOffset = 0;
                maxOffset = 10000;
                //switch (WaferSize)
                //{
                //    case EnumWaferSize.INCH6:
                //        //maxOffset = 50000;
                //        break;
                //    case EnumWaferSize.INCH8:
                //        //maxOffset = 50000;
                //        break;
                //    case EnumWaferSize.INCH12:
                //        //maxOffset = 100000;
                //        break;
                //    default:
                //        errorReasonStr = "Unsupported WaferSize. Only 6, 8, 12 inches.";
                //        return false;
                //}

                if (double.TryParse(WaferSize_Offset_um, out double WaferSize_Offset))
                {
                    if (Math.Abs(WaferSize_Offset) >= maxOffset)
                    {
                        errorReasonStr = $"The offset value of the wafer must be between -{maxOffset} and {maxOffset}um.";
                        ret = false;
                    }
                    else
                    {
                        ret = true;
                    }
                }
                else
                {
                    errorReasonStr = "Please provide a valid input.";
                    ret = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }
        #endregion

        #region //..Command & Method

        private RelayCommand _MapZoomPlusCommand;
        public ICommand MapZoomPlusCommand
        {
            get
            {
                if (null == _MapZoomPlusCommand) _MapZoomPlusCommand = new RelayCommand(Plus);
                return _MapZoomPlusCommand;
            }
        }
        private void Plus()
        {
            Wafer.ZoomLevel--;
        }

        private RelayCommand _MapZoomMinusCommand;
        public ICommand MapZoomMinusCommand
        {
            get
            {
                if (null == _MapZoomMinusCommand) _MapZoomMinusCommand = new RelayCommand(Minus);
                return _MapZoomMinusCommand;
            }
        }

        private void Minus()
        {
            Wafer.ZoomLevel++;
        }

        private RelayCommand<object> _ViewSwapCommand;
        public ICommand ViewSwapCommand
        {
            get
            {
                if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
                return _ViewSwapCommand;
            }
        }
        public virtual void ViewSwapFunc(object parameter)
        {
            try
            {
                object swap = MainViewTarget;
                MainViewTarget = MiniViewTarget;
                MiniViewTarget = swap;
                LightJog.InitCameraJog(this, CurCam.GetChannelType());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<Object> _DieSizeXTextBoxClickCommand;
        public ICommand DieSizeXTextBoxClickCommand
        {
            get
            {
                if (null == _DieSizeXTextBoxClickCommand) _DieSizeXTextBoxClickCommand = new RelayCommand<Object>(DieSizeXTextBoxClickCommandFunc);
                return _DieSizeXTextBoxClickCommand;
            }
        }

        private void DieSizeXTextBoxClickCommandFunc(Object param)
        {
            try
            {
                if (RemoteMiduumProxy != null)
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL |KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    RemoteMiduumProxy.WaferMapMaker_UpdateDieSizeX(DieSizeX);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DieSizeYTextBoxClickCommand;
        public ICommand DieSizeYTextBoxClickCommand
        {
            get
            {
                if (null == _DieSizeYTextBoxClickCommand) _DieSizeYTextBoxClickCommand = new RelayCommand<Object>(DieSizeYTextBoxClickCommandFunc);
                return _DieSizeYTextBoxClickCommand;
            }
        }

        private void DieSizeYTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text,KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                RemoteMiduumProxy.WaferMapMaker_UpdateDieSizeY(DieSizeY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<Object> _ThicknessTextBoxClickCommand;
        public ICommand ThicknessTextBoxClickCommand
        {
            get
            {
                if (null == _ThicknessTextBoxClickCommand) _ThicknessTextBoxClickCommand = new RelayCommand<Object>(ThicknessTextBoxClickCommandFunc);
                return _ThicknessTextBoxClickCommand;
            }
        }

        private void ThicknessTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                //tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL,Convert.ToInt32(this.Wafer.GetPhysInfo().Thickness.GetLowerLimit()),
                //   Convert.ToInt32(this.Wafer.GetPhysInfo().Thickness.GetUpperLimit()));
                tb.Text = VirtualKeyboard.Show(tb.Text,KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 1000);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetSubsInfo().ActualThickness = Wafer.GetPhysInfo().Thickness.Value;
                RemoteMiduumProxy.WaferMapMaker_UpdateThickness(Wafer.GetPhysInfo().Thickness.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _EdgeMarginTextBoxClickCommand;
        public ICommand EdgeMarginTextBoxClickCommand
        {
            get
            {
                if (null == _EdgeMarginTextBoxClickCommand) _EdgeMarginTextBoxClickCommand = new RelayCommand<Object>(EdgeMarginTextBoxClickCommandFunc);
                return _EdgeMarginTextBoxClickCommand;
            }
        }

        private void EdgeMarginTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text,KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                RemoteMiduumProxy.WaferMapMake_UpdateEdgeMargin(EdgeMargin);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _NotchAngleOffsetTextBoxClickCommand;
        public ICommand NotchAngleOffsetTextBoxClickCommand
        {
            get
            {
                if (null == _NotchAngleOffsetTextBoxClickCommand) _NotchAngleOffsetTextBoxClickCommand = new RelayCommand<Object>(NotchAngleOffsetTextBoxClickCommandFunc);
                return _NotchAngleOffsetTextBoxClickCommand;
            }
        }

        private void NotchAngleOffsetTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 1, 4);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                RemoteMiduumProxy.WaferMapMaker_NotchAngleOffset(NotchAngleOffset);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _NotchAngleSettingCommand;
        public ICommand NotchAngleSettingCommand
        {
            get
            {
                if (null == _NotchAngleSettingCommand) _NotchAngleSettingCommand = new AsyncCommand(NotchAngleSetting);
                return _NotchAngleSettingCommand;
            }
        }
        private async Task NotchAngleSetting()
        {
            //await this.MetroDialogManager().ShowWindow(_NotchSettingViewControl);
        }

        private RelayCommand<Object> _WaferSizeOffsetTextBoxClickCommand;
        public ICommand WaferSizeOffsetTextBoxClickCommand
        {
            get
            {
                if (null == _WaferSizeOffsetTextBoxClickCommand) _WaferSizeOffsetTextBoxClickCommand = new RelayCommand<Object>(WaferSizeOffsetTextBoxClickCommandFunc);
                return _WaferSizeOffsetTextBoxClickCommand;
            }
        }
        private void WaferSizeOffsetTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().WaferSize_Offset_um.Value = WaferSize_Offset_um;
                RemoteMiduumProxy.WaferMapMaker_UpdateWaferSizeOffset(WaferSize_Offset_um);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SelectedNotchTypeCommand;
        public ICommand SelectedNotchTypeCommand
        {
            get
            {
                if (null == _SelectedNotchTypeCommand) _SelectedNotchTypeCommand = new AsyncCommand(SelectedNotchTypeCommandFunc);
                return _SelectedNotchTypeCommand;
            }
        }
        private async Task SelectedNotchTypeCommandFunc()
        {
            try
            {
                RemoteMiduumProxy.WaferMapMaker_NotchType(NotchType);
                LoggerManager.Debug($"SelectedNotchTypeCommandFunc() set notch type : {NotchType}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _SelectedNotchAngleCommand;
        public ICommand SelectedNotchAngleCommand
        {
            get
            {
                if (null == _SelectedNotchAngleCommand) _SelectedNotchAngleCommand = new AsyncCommand(SelectedNotchAngleCommandFunc);
                return _SelectedNotchAngleCommand;
            }
        }
        private async Task SelectedNotchAngleCommandFunc()
        {
            try
            {
                RemoteMiduumProxy.WaferMapMaker_NotchAngle(NotchAngle);
                LoggerManager.Debug($"SelectedNotchAngleCommandFunc() set notch angle : {NotchAngle}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _NotchAngleTextBoxClickCommand;
        public ICommand NotchAngleTextBoxClickCommand
        {
            get
            {
                if (null == _NotchAngleTextBoxClickCommand) _NotchAngleTextBoxClickCommand = new RelayCommand<Object>(NotchAngleTextBoxClickCommandFunc);
                return _NotchAngleTextBoxClickCommand;
            }
        }


        private void NotchAngleTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().NotchAngle.Value = NotchAngle;
                RemoteMiduumProxy.WaferMapMaker_NotchAngle(NotchAngle);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _ApplyCreateWaferMapCommand;
        public ICommand ApplyCreateWaferMapCommand
        {
            get
            {
                if (null == _ApplyCreateWaferMapCommand) _ApplyCreateWaferMapCommand = new AsyncCommand(ApplyCreateWaferMapCommandFunc);
                return _ApplyCreateWaferMapCommand;
            }
        }


        private async Task ApplyCreateWaferMapCommandFunc()
        {
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
            try
            {
                string errorReason = "";
                if (VerifyParameter(Wafer.GetPhysInfo().WaferSizeEnum, WaferSize_Offset_um.ToString(), ref errorReason) == false)
                {
                    Wafer.GetPhysInfo().WaferSize_Offset_um.Value = 0;
                    WaferSize_Offset_um = 0;
                    this.MetroDialogManager().ShowMessageDialog("Error", $"{errorReason}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else 
                {
                    var mapviewcamtype = Wafer.MapViewAssignCamType?.Value;
                    RemoteMiduumProxy.WaferMapMaker_UpdateWaferSize(WaferSize);
                    RemoteMiduumProxy.WaferMapMaker_UpdateWaferSizeOffset(WaferSize_Offset_um);
                    RemoteMiduumProxy.WaferMapMakerVM_WaferSubstrateType(WaferSubstrateType);
                    RemoteMiduumProxy.WaferMapMaker_NotchAngle(NotchAngle);
                    RemoteMiduumProxy.WaferMapMaker_NotchType(NotchType);

                    //await this.WaitCancelDialogService().CloseDialog();

                    await RemoteMiduumProxy.WaferMapMaker_ApplyCreateWaferMapCommand();

                    var stage = (StageSupervisorProxy)_LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                    var waferObject = stage.GetWaferObject(Wafer.GetSubsInfo().DIEs);

                    //this.StageSupervisor().WaferObject = waferObject;
                    if (waferObject != null)
                    {
                        this.StageSupervisor().WaferObject.AlignState = stage.GetAlignState(AlignTypeEnum.Wafer);
                    }

                (this.StageSupervisor().WaferObject as WaferObject).WaferDevObject = (waferObject as WaferObject).WaferDevObject;
                    var substratenonserial = stage.GetSubstrateInfoNonSerialized();
                    ISubstrateInfo subsinfo = waferObject.GetSubsInfo();
                    subsinfo.ActualDeviceSize = substratenonserial.ActualDeviceSize;
                    subsinfo.ActualDieSize = substratenonserial.ActualDieSize;
                    subsinfo.WaferCenter = substratenonserial.WaferCenter;
                    subsinfo.WaferObjectChangedToggle = substratenonserial.WaferObjectChangedToggle;

                    subsinfo.DIEs = await stage.GetConcreteDIEs();
                    subsinfo.DutCenX = substratenonserial.DutCenX;
                    subsinfo.DutCenY = substratenonserial.DutCenY;

                    MapIndexX = waferObject.GetPhysInfo().CenM.XIndex.Value;
                    MapIndexY = waferObject.GetPhysInfo().CenM.YIndex.Value;
                    //this.StageSupervisor().WaferObject.MapViewControlMode = MapViewMode.MapMode;
                    //this.StageSupervisor().WaferObject.MapViewStageSyncEnable = true;
                    //this.StageSupervisor().WaferObject.MapViewCurIndexVisiablity = true;
                    //this.StageSupervisor().WaferObject.MapViewAssignCamType = new Element<EnumProberCam>()
                    //{ Value = (EnumProberCam)mapviewcamtype };
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand _MoveToWaferThicknessCommand;
        public ICommand MoveToWaferThicknessCommand
        {
            get
            {
                if (null == _MoveToWaferThicknessCommand) _MoveToWaferThicknessCommand = new AsyncCommand(MoveToWaferThicknessCommandFunc);
                return _MoveToWaferThicknessCommand;
            }
        }
        private async Task MoveToWaferThicknessCommandFunc()
        {
            try
            {
                
                await RemoteMiduumProxy.WaferMapMaker_MoveToWaferThicknessCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        private AsyncCommand _AdjustWaferHeightCommand;
        public ICommand AdjustWaferHeightCommand
        {
            get
            {
                if (null == _AdjustWaferHeightCommand) _AdjustWaferHeightCommand = new AsyncCommand(AdjustWaferHeightCommandFunc);
                return _AdjustWaferHeightCommand;
            }
        }
        private async Task AdjustWaferHeightCommandFunc()
        {
            try
            {
                await RemoteMiduumProxy.WaferMapMaker_AdjustWaferHeightCommand();
                //Wafer.GetPhysInfo().Thickness.Value = Math.Round(CurCam.CamSystemPos.GetZ(), 3);

                Wafer.GetPhysInfo().Thickness.Value = Math.Round(CurCam.GetCurCoordPos().GetZ(), 1);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        private AsyncCommand _CmdImportWaferData;
        public ICommand CmdImportWaferData
        {
            get
            {
                if (null == _CmdImportWaferData) _CmdImportWaferData = new AsyncCommand(ImportWaferData);
                return _CmdImportWaferData;
            }
        }

        public Stream CSVFileStream { get; set; }

        OpenFileDialogResult result = null;
        private async Task ImportWaferData()
        {
            try
            {
                string filePath = null;
                Stream fileStream = null;

                OpenFileDialogArguments dialogArgs = new OpenFileDialogArguments()
                {
                    Width = 900,
                    Height = 700,
                    CurrentDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory,
                    Filters = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
                };

                result = await Application.Current.Dispatcher.Invoke<Task<OpenFileDialogResult>>(() =>
                {
                    Task<OpenFileDialogResult> dialogResult = null;
                    try
                    {
                        dialogResult = MaterialDesignExtensions.Controls.OpenFileDialog.ShowDialogAsync("dialogHost", dialogArgs);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return dialogResult;
                });

                if (result.Canceled != true)
                {
                    filePath = result.FileInfo.FullName;
                    fileStream = result.FileInfo.OpenRead();
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    RemoteMiduumProxy.WaferMapMaker_ImportFilePath(filePath);
                    EventCodeEnum retVal = await RemoteMiduumProxy.WaferMapMaker_CmdImportWaferData(fileStream);

                    if (retVal == EventCodeEnum.NONE)
                    {
                        var stage = (StageSupervisorProxy)_LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                        var waferObject = stage.GetWaferObject(Wafer.GetSubsInfo().DIEs);
                        if (waferObject != null)
                        {
                            this.StageSupervisor().WaferObject.AlignState = stage.GetAlignState(AlignTypeEnum.Wafer);
                        }
                        (this.StageSupervisor().WaferObject as WaferObject).WaferDevObject = (waferObject as WaferObject).WaferDevObject;

                        var substratenonserial = stage.GetSubstrateInfoNonSerialized();
                        ISubstrateInfo subsinfo = waferObject.GetSubsInfo();
                        subsinfo.ActualDeviceSize = substratenonserial.ActualDeviceSize;
                        subsinfo.ActualDieSize = substratenonserial.ActualDieSize;
                        subsinfo.WaferCenter = substratenonserial.WaferCenter;
                        subsinfo.WaferObjectChangedToggle = substratenonserial.WaferObjectChangedToggle;                        
                        subsinfo.DIEs = await stage.GetConcreteDIEs();
                        subsinfo.DutCenX = substratenonserial.DutCenX;
                        subsinfo.DutCenY = substratenonserial.DutCenY;
                        InitData(); 
                        
                        LoggerManager.Debug($"Success to update WaferMap from csv file { filePath}");
                        this.MetroDialogManager().ShowMessageDialog("Import Success", filePath,
                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand _CmdExportWaferData;
        public ICommand CmdExportWaferData
        {
            get
            {
                if (null == _CmdExportWaferData) _CmdExportWaferData = new AsyncCommand(ExportWaferData);
                return _CmdExportWaferData;
            }
        }

        public string CSVFilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private EnumMessageDialogResult MessageDialogResult = EnumMessageDialogResult.UNDEFIND;

        private async Task ExportWaferData()
        {
            try
            {
                var stage = (StageSupervisorProxy)_LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                int fileCount = 0;
                bool bExist = true;

                MessageDialogResult = await this.MetroDialogManager().ShowMessageDialog("Do you want to export the wafermap data?",
                               "Click OK to export",
                               EnumMessageStyle.AffirmativeAndNegative);
                if (MessageDialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    string DeviceParamRootDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory;
                    string filePath = Path.Combine(DeviceParamRootDirectory, "WaferMap", "WaferMap_" + stage.GetDeviceName() +"_"+ DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
                    if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    while (bExist)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            fileCount++;
                            string[] splitFilePath = filePath.Split('.');
                            if (splitFilePath[0].Contains('('))
                            {
                                string[] splitFilePath2 = splitFilePath[0].Split('(');
                                splitFilePath2[0] += "(" + fileCount + ")" + ".csv";
                                filePath = splitFilePath2[0];
                            }
                            else
                            {
                                splitFilePath[0].Split('(');
                                splitFilePath[0] += "(" + fileCount + ")" + ".csv";
                                filePath = splitFilePath[0];
                            }
                        }
                        else
                        {
                            bExist = false;
                        }
                    }
                    if (SetWaferMapDataToCSVFile(filePath) == EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Export Success", filePath,
                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        LoggerManager.Debug($"Success to export WaferMap to csv file { filePath}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private EventCodeEnum SetWaferMapDataToCSVFile(string csv_file_path)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool flip = false;
                if (this.StageSupervisor.CoordinateManager().GetReverseManualMoveX() &&
                    this.StageSupervisor.CoordinateManager().GetReverseManualMoveY())
                {
                    flip = true;
                }

                double notchAngle = double.Parse(Wafer.GetPhysInfo().NotchAngle.Value.ToString());
                if (flip)
                {
                    notchAngle += 180;
                    if (notchAngle >= 360)
                    {
                        notchAngle = notchAngle - 360;
                    }
                }

                using (StreamWriter sw = new StreamWriter(csv_file_path))
                {
                    String columnLine = String.Empty;

                    #region Waferinfo
                    sw.WriteLine("WaferInfo");
                    sw.Write("WaferSize");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().WaferSize_um.Value / 1000);
                    sw.Write("DieSizeX");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().DieSizeX.Value);
                    sw.Write("DieSizeY");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().DieSizeY.Value);
                    sw.Write("MoveWafPosX");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().CenDieOffset.X.Value);
                    sw.Write("MoveWafPosY");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().CenDieOffset.Y.Value);
                    sw.Write("NotchType");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().NotchType.Value);
                    sw.Write("CenterAddressX");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().CenU.XIndex.Value);
                    sw.Write("CenterAddressY");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().CenU.YIndex.Value);
                    sw.Write("NotchAngle");
                    sw.Write(",");
                    sw.WriteLine(notchAngle);
                    sw.Write("EdgeMargin");
                    sw.Write(",");
                    sw.WriteLine(Wafer.GetPhysInfo().WaferMargin_um.Value);
                    #endregion

                    sw.WriteLine("DieInfo");
                    int invertRowRatio = Wafer.GetSubsInfo().DIEs.GetUpperBound(0);
                    int colMaxIndex = Wafer.GetSubsInfo().DIEs.GetUpperBound(1);


                    for (int i = Wafer.GetSubsInfo().DIEs.GetUpperBound(1); i >= 0; i--)
                    {
                        for (int j = 0; j <= Wafer.GetSubsInfo().DIEs.GetUpperBound(0); j++)
                        {
                            if (flip)
                            {
                                columnLine = ((int)Wafer.GetSubsInfo().DIEs[invertRowRatio - j, colMaxIndex - i].DieType.Value).ToString();
                            }
                            else
                            {
                                columnLine = ((int)Wafer.GetSubsInfo().DIEs[j, i].DieType.Value).ToString();
                            }
                            
                            sw.Write(columnLine);
                            if (j < Wafer.GetSubsInfo().DIEs.GetUpperBound(0))
                            {
                                sw.Write(",");
                            }
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    retVal = EventCodeEnum.NONE;
                }
                return retVal;
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Export Failed", "This file is being used by another process",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                return EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
        }

        Task<EventCodeEnum> IWaferMapMakerVM.ImportWaferData()
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> ApplyCreateWaferMap()
        {
            throw new NotImplementedException();
        }

        Task IWaferMapMakerVM.MoveToWaferThicknessCommandFunc()
        {
            throw new NotImplementedException();
        }

        Task IWaferMapMakerVM.AdjustWaferHeightCommandFunc()
        {
            throw new NotImplementedException();
        }

        public HeightPointEnum GetHeightPoint()
        {
            return HeightPoint;
        }

        public void SaveHeightPoint(HeightPointEnum point)
        {
            try
            {
                RemoteMiduumProxy.WaferMapMakerVM_SetHighStandardParam(point);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return;
        }
        private AsyncCommand<object> _SelectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get
            {
                if (null == _SelectionChangedCommand) _SelectionChangedCommand = new AsyncCommand<object>(SelectionChangedFunc);
                return _SelectionChangedCommand;
            }
        }


        public async Task SelectionChangedFunc(object param)
        {
            try
            {
                if (param != null)
                {
                    HeightPointEnum point = (HeightPointEnum)param;
                    RemoteMiduumProxy.WaferMapMakerVM_SetHighStandardParam(point);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
