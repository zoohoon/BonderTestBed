using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaferMapMakerViewModel
{
    using Focusing;
    using HexagonJogControl;
    using LogModule;
    using MaterialDesignExtensions.Controls;
    using MetroDialogInterfaces;
    using Microsoft.VisualBasic.FileIO;
    using NotchSettingView;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.Param;
    using ProberInterfaces.State;
    using RelayCommandBase;
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
    using CylType;
    using ProberInterfaces.WaferAlignEX;
    using WA_HighMagParameter_Standard;

    public class WaferMapMakerVM : IMainScreenViewModel, INotifyPropertyChanged, IUseLightJog, IWaferMapMakerVM, ISetUpState
    {
        readonly Guid _ViewModelGUID = new Guid("156C4231-360D-138C-ED86-6E586C45F359");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Initialized { get; set; } = false;
        public WaferMapMakerVM()
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

        public IWaferObject Wafer { get { return this.GetParam_Wafer(); } }

        public IStageSupervisor StageSupervisor { get { return this.StageSupervisor(); } }
        private ICoordinateManager _CoordinateManager;

        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set { _CoordinateManager = value; }
        }

        private IParam _HighStandard_IParam;
        [ParamIgnore]
        public IParam HighStandard_IParam
        {
            get { return _HighStandard_IParam; }
            set
            {
                if (value != _HighStandard_IParam)
                {
                    _HighStandard_IParam = value;
                    RaisePropertyChanged();
                }
            }

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
        private double _ZoomObject;
        public double ZoomObject
        {
            get { return _ZoomObject; }
            set
            {
                if (value != _ZoomObject)
                {
                    _ZoomObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Stream _CSVFileStream;
        public Stream CSVFileStream
        {
            get { return _CSVFileStream; }
            set
            {
                if (value != _CSVFileStream)
                {
                    _CSVFileStream = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CSVFilePath;
        public string CSVFilePath
        {
            get { return _CSVFilePath; }
            set
            {
                if (value != _CSVFilePath)
                {
                    _CSVFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private long _MapIndexX;
        //public long MapIndexX
        //{
        //    get { return _MapIndexX; }
        //    set
        //    {
        //        if (value != _MapIndexX)
        //        {
        //            _MapIndexX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private long _MapIndexY;
        //public long MapIndexY
        //{
        //    get { return _MapIndexY; }
        //    set
        //    {
        //        if (value != _MapIndexY)
        //        {
        //            _MapIndexY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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

        private WaferSubstrateTypeEnum _WaferSubstrateType;
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

        //WaferSubstrateTypeEnum
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

        private WaferNotchTypeEnum _NotchType;
        public WaferNotchTypeEnum NotchType
        {
            get { return _NotchType; }
            set
            {
                if (value != _NotchType)
                {
                    _NotchType = value;
                    Wafer.GetPhysInfo().NotchType.Value = value.ToString();
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

        private IPhysicalInfo _PhysicalInfo;
        public IPhysicalInfo PhysicalInfo
        {
            get { return Wafer.GetPhysInfo(); }
            set
            {
                if (value != _PhysicalInfo)
                {
                    _PhysicalInfo = value;
                    RaisePropertyChanged();
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


        private bool RememberMapViewStageSyncEnable { get; set; }

        #region //..View

        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
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

        private LightJogViewModel _LightJog;
        public LightJogViewModel LightJog
        {
            get { return _LightJog; }
            set
            {
                if (value != _LightJog)
                {
                    _LightJog = value;
                    RaisePropertyChanged();
                }
            }
        }

        
        public IHexagonJogViewModel MotionJog { get; set; }


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
        #endregion

        public WA_HighMagParam_Standard HighStandardParam_Clone;
        public ILotOPModule LotOPModule { get; set; }

        private NotchSettingViewControl _NotchSettingViewControl { get; set; }

        private IFocusing _FocusModel;
        public IFocusing FocusModel
        {
            get
            {
                //if (_FocusModel == null)
                //    _FocusModel = this.FocusManager().GetFocusingModel(FocusingModuleDllInfo);
                return _FocusModel;
            }
        }

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set { _FocusParam = value; }
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

        private AsyncCommand _CmdExportWaferData;
        public ICommand CmdExportWaferData
        {
            get
            {
                if (null == _CmdExportWaferData) _CmdExportWaferData = new AsyncCommand(ExportWaferData);
                return _CmdExportWaferData;
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LotOPModule = this.LotOPModule();
                CoordinateManager = this.CoordinateManager();

                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                if(this.FocusManager() != null)
                {
                    _FocusModel = this.FocusManager().GetFocusingModel(FocusingModuleDllInfo);

                    FocusParam = new NormalFocusParameter();
                    this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
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

                NotchAngles.Add(0);
                NotchAngles.Add(90);
                NotchAngles.Add(180);
                NotchAngles.Add(270);

                LightJog = new LightJogViewModel(
                       maxLightValue: 255,
                       minLightValue: 0);

                MotionJog = new HexagonJogViewModel();


                //_NotchSettingViewControl = new NotchSettingViewControl();
                //_NotchSettingViewControl.DataContext = this;

                _DisplayPort = new DisplayPort() { GUID = new Guid("6A505783-6588-BBAC-5B86-EA4F72C13A9B") };
                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                foreach (var cam in this.VisionManager().GetCameras())
                {
                    this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                }
                
                if(this.MotionManager() != null)
                {
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisXPos = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisYPos = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisZPos = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisTPos = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    ((UcDisplayPort.DisplayPort)DisplayPort).AxisPZPos = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                }
                
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

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //this.SysState().SetSetUpState();
                LightJog = (LightJogViewModel)this.PnPManager().PnpLightJog;

                InitData();
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                foreach (var light in CurCam.LightsChannels)
                {
                    CurCam.SetLight(light.Type.Value, 100);
                }

                ICamera highcam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                foreach (var light in highcam.LightsChannels)
                {
                    highcam.SetLight(light.Type.Value, 100);
                }

                LightJog.InitCameraJog(this, CurCam.GetChannelType());
                Wafer.MapViewAssignCamType.Value = _CurCam.GetChannelType();

               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                RememberMapViewStageSyncEnable = Wafer.MapViewStageSyncEnable;
                Wafer.MapViewStageSyncEnable = true;
            }

            return retVal;
        }

        //private async Task UpdateMapIndex(object newVal)
        //{

        //    try
        //    {
        //        MachineIndex MI = new MachineIndex();
        //        MI = this.StageSupervisor().WaferObject.GetCurrentMIndex();

        //        this.MapIndexX = MI.XIndex;
        //        this.MapIndexY = MI.YIndex;
        //    }
        //    catch (Exception err)
        //    {
        //        //LoggerManager.Error($err, "OverlayDie() Error occurred.");
        //        LoggerManager.Exception(err);
        //    }
        //}
        
        private void InitData()
        {
            try
            {
                DieSizeX = Wafer.GetPhysInfo().DieSizeX.Value;
                DieSizeY = Wafer.GetPhysInfo().DieSizeY.Value;
                
                NotchAngle = Wafer.GetPhysInfo().NotchAngle.Value;
                
                NotchAngleOffset = Wafer.GetPhysInfo().NotchAngleOffset.Value;
                WaferSubstrateType = Wafer.GetPhysInfo().WaferSubstrateType.Value;
                WaferSize = Wafer.GetPhysInfo().WaferSizeEnum;
                WaferSize_Offset_um = Wafer.GetPhysInfo().WaferSize_Offset_um.Value;
                //Thickness = Wafer.GetPhysInfo().Thickness.Value;
                EdgeMargin = Wafer.GetPhysInfo().WaferMargin_um.Value;
                if (Wafer.GetPhysInfo().NotchType.Value.ToLower() == WaferNotchTypeEnum.FLAT.ToString().ToLower())
                    NotchType = WaferNotchTypeEnum.FLAT;
                else if (Wafer.GetPhysInfo().NotchType.Value.ToLower() == WaferNotchTypeEnum.NOTCH.ToString().ToLower())
                    NotchType = WaferNotchTypeEnum.NOTCH;

                OrgMachineXCoord = Wafer.GetPhysInfo().OrgM.XIndex.Value;
                OrgMachineYCoord = Wafer.GetPhysInfo().OrgM.YIndex.Value;
                OrgUserXCoord = Wafer.GetPhysInfo().OrgU.XIndex.Value;
                OrgUserYCoord = Wafer.GetPhysInfo().OrgU.YIndex.Value;

                //MapIndexX = Wafer.GetPhysInfo().CenM.XIndex.Value;
                //MapIndexY = Wafer.GetPhysInfo().CenM.YIndex.Value;
                this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Wafer.GetPhysInfo().Thickness.Value);

                MainViewTarget = null;
                MainViewTarget = Wafer;
                MiniViewTarget = DisplayPort;
                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                this.WaferAligner().GetHighStandardParam();
                HeightPoint = this.WaferAligner().HeightProfilingPointType;

                DispHorFlip = this.VisionManager().GetDispHorFlip();
                DispVerFlip = this.VisionManager().GetDispVerFlip();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EnumMessageDialogResult MessageDialogResult = EnumMessageDialogResult.UNDEFIND;

        private async Task ExportWaferData()
        {
            try
            {
                int fileCount = 0;
                bool bExist = true;
                MessageDialogResult = await this.MetroDialogManager().ShowMessageDialog("Do you want to export the wafermap data?",
                                    "Click OK to export",
                                    EnumMessageStyle.AffirmativeAndNegative);
                if (MessageDialogResult == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    string DeviceParamRootDirectory = this.FileManager().FileManagerParam.DeviceParamRootDirectory;
                    string filePath = Path.Combine(DeviceParamRootDirectory, "WaferMap", "WaferMap_" + this.FileManager().GetDeviceName() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
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
                    SetWaferMapDataToCSVFile(filePath);
                    this.MetroDialogManager().ShowMessageDialog("Export Success", filePath,
                    MetroDialogInterfaces.EnumMessageStyle.Affirmative);
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
        private void SetWaferMapDataToCSVFile(string csv_file_path)
        {
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
                }
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Export Failed", "This file is being used by another process",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        OpenFileDialogResult result = null;
        public async Task<EventCodeEnum> ImportWaferData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                string filePath = null;
                Stream importFile = null;
                WaferObject waferTmp = new WaferObject();
                waferTmp.WaferDevObject = new WaferDevObject();
                byte[,] mapTmp;
                if (CSVFileStream != null)
                {
                    importFile = CSVFileStream;
                    filePath = CSVFilePath;
                }
                else
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
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
                        var mRet = await this.MetroDialogManager().ShowMessageDialog("Warning Message", 
                            "This operation initializes all current wafer map , align data. Do you want to continue?", EnumMessageStyle.AffirmativeAndNegative);
                        if(mRet == EnumMessageDialogResult.AFFIRMATIVE)
                        { 
                            importFile = result.FileInfo.OpenRead();
                            filePath = result.FileInfo.FullName;
                        }
                    }
                }

                if (importFile != null)
                {
                    EventCodeEnum result = GetWaferMapDataFromCSVFile(importFile, ref waferTmp, out mapTmp);
                    if (result == EventCodeEnum.NONE)
                    {
                        Wafer.GetPhysInfo().WaferSize_um.Value = waferTmp.GetPhysInfo().WaferSize_um.Value;
                        Wafer.GetPhysInfo().WaferSizeEnum = waferTmp.GetPhysInfo().WaferSizeEnum;
                        Wafer.GetPhysInfo().DieSizeX.Value = waferTmp.GetPhysInfo().DieSizeX.Value;
                        Wafer.GetPhysInfo().DieSizeY.Value = waferTmp.GetPhysInfo().DieSizeY.Value;
                        Wafer.GetPhysInfo().NotchType.Value = waferTmp.GetPhysInfo().NotchType.Value;
                        Wafer.GetPhysInfo().CenU.XIndex.Value = waferTmp.GetPhysInfo().CenU.XIndex.Value;
                        Wafer.GetPhysInfo().CenU.YIndex.Value = waferTmp.GetPhysInfo().CenU.YIndex.Value;
                        Wafer.GetPhysInfo().CenDieOffset.X.Value = waferTmp.GetPhysInfo().CenDieOffset.X.Value;
                        Wafer.GetPhysInfo().CenDieOffset.Y.Value = waferTmp.GetPhysInfo().CenDieOffset.Y.Value;
                        Wafer.GetPhysInfo().NotchAngle.Value = waferTmp.GetPhysInfo().NotchAngle.Value;
                        Wafer.GetPhysInfo().WaferMargin_um.Value = waferTmp.GetPhysInfo().WaferMargin_um.Value;

                        Wafer.ZoomLevelInit = false;
                        (Wafer.WaferDevObjectRef as WaferDevObject).SetDefaultParam();
                        (Wafer.WaferDevObjectRef as WaferDevObject).UpdateWaferObject(mapTmp);
                        (Wafer.WaferDevObjectRef as WaferDevObject).InitMap();

                        InitData();
                        this.StageSupervisor().SaveWaferObject();
                        if (CSVFileStream == null)
                        {
                            LoggerManager.Debug($"Success to update WaferMap from csv file { filePath}");
                            this.MetroDialogManager().ShowMessageDialog("Import Success", filePath,
                            MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("Import Success", "Please check the csv file.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                    retVal = result;
                }
                return retVal;
            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("Import Failed", "This file is being used by another process",
                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                return EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
            finally
            {
                CSVFileStream = null;
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private EventCodeEnum GetWaferMapDataFromCSVFile(Stream importFile, ref WaferObject waferTmp, out byte[,] mapTmp)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            mapTmp = new byte[0, 0];
            try
            {
                int waferSize;
                double dieSizeX;
                double dieSizeY;
                double moveWafPosX;
                double moveWafPosY;
                int PresetAddressX;
                int PresetAddressY;
                double notchAngle;
                int OFPosition;
                long centerAddressX;
                long centerAddressY;
                int notchTypeChk;
                double edgeMargin;

                bool flip = false;
                if (this.StageSupervisor.CoordinateManager().GetReverseManualMoveX() &&
                        this.StageSupervisor.CoordinateManager().GetReverseManualMoveY())
                {
                    flip = true;
                }

                using (TextFieldParser csvReader = new TextFieldParser(importFile))
                {
                    csvReader.SetDelimiters(",");
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    String[] waferData = csvReader.ReadFields();
                    if (waferData != null)
                    {
                        while (!waferData[0].Equals("DieInfo"))
                        {
                            waferData = csvReader.ReadFields();
                            switch (waferData[0].ToLower())
                            {
                                case "wafersize":
                                    if (int.TryParse(waferData[1], out waferSize))
                                    {
                                        switch (waferSize)
                                        {
                                            case 150:
                                                waferTmp.GetPhysInfo().WaferSize_um.Value = 150000;
                                                waferTmp.GetPhysInfo().WaferSizeEnum = EnumWaferSize.INCH6;
                                                break;
                                            case 200:
                                                waferTmp.GetPhysInfo().WaferSize_um.Value = 200000;
                                                waferTmp.GetPhysInfo().WaferSizeEnum = EnumWaferSize.INCH8;
                                                break;
                                            case 300:
                                                waferTmp.GetPhysInfo().WaferSize_um.Value = 300000;
                                                waferTmp.GetPhysInfo().WaferSizeEnum = EnumWaferSize.INCH12;
                                                break;
                                            default:
                                                this.MetroDialogManager().ShowMessageDialog("Import Failed", "The WaferSize value of the imported CSV file is invalid.",
                                                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                                LoggerManager.Debug($"The WaferSize value of the imported CSV file is invalid. { waferSize}");
                                                return retVal;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The WaferSize type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The WaferSize type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "diesizex":
                                    if (double.TryParse(waferData[1], out dieSizeX))
                                    {
                                        waferTmp.GetPhysInfo().DieSizeX.Value = dieSizeX;
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The DieSizeX type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The DieSizeX type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "diesizey":
                                    if (double.TryParse(waferData[1], out dieSizeY))
                                    {
                                        waferTmp.GetPhysInfo().DieSizeY.Value = dieSizeY;
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The DieSizeY type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The DieSizeY type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "movewafposx":
                                    if (double.TryParse(waferData[1], out moveWafPosX))
                                    {
                                        waferTmp.GetPhysInfo().CenDieOffset.X.Value = moveWafPosX;
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The moveWafPosX type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The moveWafPosX type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "movewafposy":
                                    if (double.TryParse(waferData[1], out moveWafPosY))
                                    {
                                        waferTmp.GetPhysInfo().CenDieOffset.Y.Value = moveWafPosY;
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The moveWafPosY type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The moveWafPosY type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "notchtype":
                                    if (!int.TryParse(waferData[1], out notchTypeChk))
                                    {
                                        switch (waferData[1].ToLower())
                                        {
                                            case "flat":
                                                waferTmp.GetPhysInfo().NotchType.Value = waferData[1];
                                                break;
                                            case "notch":
                                                waferTmp.GetPhysInfo().NotchType.Value = waferData[1];
                                                break;
                                            default:
                                                this.MetroDialogManager().ShowMessageDialog("Import Failed", "The NotchType value of the imported CSV file is invalid.",
                                                MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                                LoggerManager.Debug($"The NotchType value of the imported CSV file is invalid. { waferData[1].ToString()}");
                                                return retVal;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The NotchType type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The NotchType type of the imported CSV file is invalid. { notchTypeChk.GetType().Name}");
                                        return retVal;
                                    }
                                case "centeraddressx":
                                    if (long.TryParse(waferData[1], out centerAddressX))
                                    {
                                        waferTmp.GetPhysInfo().CenU.XIndex.Value = Convert.ToInt64(waferData[1]);
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The CenterAddressX type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The CenterAddressX type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "centeraddressy":
                                    if (long.TryParse(waferData[1], out centerAddressY))
                                    {
                                        waferTmp.GetPhysInfo().CenU.YIndex.Value = Convert.ToInt64(waferData[1]);
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The CenterAddressY type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The CenterAddressY type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "notchangle":
                                    if (double.TryParse(waferData[1], out notchAngle))
                                    {
                                        if (flip)
                                        {
                                            notchAngle += 180;
                                            if (notchAngle >= 360)
                                            {
                                                notchAngle = notchAngle - 360;
                                            }

                                        }

                                        waferTmp.GetPhysInfo().NotchAngle.Value = notchAngle;


                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The NotchAngle type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The NotchAngle type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                case "edgemargin":
                                    if (double.TryParse(waferData[1], out edgeMargin))
                                    {
                                        waferTmp.GetPhysInfo().WaferMargin_um.Value = edgeMargin;
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "The EdgeMargin type of the imported CSV file is invalid.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"The EdgeMargin type of the imported CSV file is invalid. { waferData[1].GetType().Name}");
                                        return retVal;
                                    }
                                default:
                                    if (waferData[0] == "DieInfo")
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Import Failed", "Wafermap CSV File format is incorrect.",
                                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"Wafermap CSV File format is incorrect.");
                                        return retVal;
                                    }
                            }
                        }
                    }
                    int mapCntX = 0;
                    int mapCntY = 0;
                    int row = 0;
                    byte dieValue;
                    List<List<byte>> tmpMap = new List<List<byte>>();

                    while (csvReader.EndOfData == false)
                    {
                        String[] dieData = csvReader.ReadFields();
                        if (dieData != null)
                        {
                            mapCntX = dieData.Length;
                            tmpMap.Add(new List<byte>());
                            for (int col = 0; col < dieData.Length; col++)
                            {
                                if (byte.TryParse(dieData[col], out dieValue))
                                {
                                    tmpMap[row].Add(dieValue);
                                }
                                else
                                {
                                    return retVal;
                                }
                            }
                            row++;
                            mapCntY++;
                        }
                    }

                    
                 

                    int invertRowRatio = mapCntX - 1;
                    int colMaxIndex = mapCntY - 1;


                    mapTmp = new byte[mapCntX, mapCntY];
                    for (int i = mapCntY - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < mapCntX; j++)
                        {

                            if (flip)
                            {
                                mapTmp[j, mapCntY - 1 - i] = tmpMap[colMaxIndex - i][invertRowRatio - j];
                            }
                            else
                            {
                                mapTmp[j, mapCntY - 1 - i] = tmpMap[i][j];
                            }
                        }
                    }
                }
                return EventCodeEnum.NONE;
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //this.SysState().SetSetUpDoneState();
                if (this.Wafer.WaferDevObjectRef.IsParamChanged)
                {
                    string errorReason = "";
                    if (VerifyParameter(Wafer.GetPhysInfo().WaferSizeEnum, WaferSize_Offset_um.ToString(),ref errorReason) == false) 
                    {
                        Wafer.GetPhysInfo().WaferSize_Offset_um.Value = 0;
                        this.MetroDialogManager().ShowMessageDialog("Error", $"{errorReason}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }
                    this.StageSupervisor().SaveWaferObject();
                }
                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.VisionManager().StopGrab(CurCam.GetChannelType());
                StageCylinderType.MoveWaferCam.Retract();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                Wafer.MapViewStageSyncEnable = RememberMapViewStageSyncEnable;
            }

            return retVal;
        }

        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
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

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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


        #region //..Command

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
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
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
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
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
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 1000);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetSubsInfo().ActualThickness = Wafer.GetPhysInfo().Thickness.Value;
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
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
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
                Wafer.GetPhysInfo().NotchAngleOffset.Value = NotchAngleOffset;
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
            await this.MetroDialogManager().ShowWindow(_NotchSettingViewControl);
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
                string inputText = "";
                string errorReason = "";
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().WaferSize_Offset_um.Value = WaferSize_Offset_um;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool VerifyParameter(EnumWaferSize WaferSize, string WaferSize_Offset_um, ref string errorReasonStr)
        {
            bool ret = true;
            try
            {
                int maxOffset = 0;
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

                if (int.TryParse(WaferSize_Offset_um, out int WaferSize_Offset))
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


        private AsyncCommand _ApplyCreateWaferMapCommand;
        public ICommand ApplyCreateWaferMapCommand
        {
            get
            {
                if (null == _ApplyCreateWaferMapCommand) _ApplyCreateWaferMapCommand = new AsyncCommand(ApplyCreateWaferMapCommandFunc);
                return _ApplyCreateWaferMapCommand;
            }
        }

        public async Task<EventCodeEnum> ApplyCreateWaferMap()
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                Task<EventCodeEnum> task = new Task<EventCodeEnum>(() => 
                {
                    Wafer.GetPhysInfo().WaferSizeEnum = WaferSize;
                    switch (Wafer.GetPhysInfo().WaferSizeEnum)
                    {
                        case EnumWaferSize.INCH6:
                            Wafer.GetPhysInfo().WaferSize_um.Value = 150000;
                            break;
                        case EnumWaferSize.INCH8:
                            Wafer.GetPhysInfo().WaferSize_um.Value = 200000;
                            break;
                        case EnumWaferSize.INCH12:
                            Wafer.GetPhysInfo().WaferSize_um.Value = 300000;
                            break;
                        default:
                            break;
                    }
                    Wafer.GetPhysInfo().WaferSize_Offset_um.Value = WaferSize_Offset_um;

                    switch (NotchType)
                    {
                        case WaferNotchTypeEnum.FLAT:
                            Wafer.GetPhysInfo().NotchType.Value = WaferNotchTypeEnum.FLAT.ToString();
                            break;
                        case WaferNotchTypeEnum.NOTCH:
                            Wafer.GetPhysInfo().NotchType.Value = WaferNotchTypeEnum.NOTCH.ToString();
                            break;
                        default:
                            break;
                    }

                    Wafer.GetPhysInfo().DieSizeX.Value = DieSizeX;
                    Wafer.GetPhysInfo().DieSizeY.Value = DieSizeY;
                    Wafer.GetPhysInfo().NotchAngle.Value = NotchAngle;
                    
                    Wafer.GetPhysInfo().NotchAngleOffset.Value = NotchAngleOffset;

                    //Wafer.GetPhysInfo().Thickness.Value = Thickness;
                    Wafer.GetPhysInfo().WaferMargin_um.Value = EdgeMargin;

                    Wafer.ZoomLevelInit = false;

                    Wafer.UpdateData();

                    this.StageSupervisor().SaveWaferObject();

                    InitData();
                    eventCodeEnum = EventCodeEnum.NONE;
                    return eventCodeEnum;
                });
                task.Start();
                eventCodeEnum = await task;
            }
            catch (Exception err)
            {
                eventCodeEnum = EventCodeEnum.MAP_SAVE_FAIL;
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
            return eventCodeEnum;
        }

        private async Task ApplyCreateWaferMapCommandFunc()
        {

            try
            {
                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                string errorReason = "";
                if (VerifyParameter(Wafer.GetPhysInfo().WaferSizeEnum, WaferSize_Offset_um.ToString(), ref errorReason) == false)
                {
                    Wafer.GetPhysInfo().WaferSize_Offset_um.Value = 0;
                    WaferSize_Offset_um = 0;
                    this.MetroDialogManager().ShowMessageDialog("Error", $"{errorReason}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else
                {
                    Wafer.GetPhysInfo().WaferSizeEnum = WaferSize;
                    switch (Wafer.GetPhysInfo().WaferSizeEnum)
                    {
                        case EnumWaferSize.INCH6:
                            Wafer.GetPhysInfo().WaferSize_um.Value = 150000;
                            break;
                        case EnumWaferSize.INCH8:
                            Wafer.GetPhysInfo().WaferSize_um.Value = 200000;
                            break;
                        case EnumWaferSize.INCH12:
                            Wafer.GetPhysInfo().WaferSize_um.Value = 300000;
                            break;
                        default:
                            break;
                    }
                    Wafer.GetPhysInfo().WaferSize_Offset_um.Value = WaferSize_Offset_um;
                    Wafer.GetPhysInfo().WaferSubstrateType.Value = WaferSubstrateType;

                    switch (NotchType)
                    {
                        case WaferNotchTypeEnum.FLAT:
                            Wafer.GetPhysInfo().NotchType.Value = WaferNotchTypeEnum.FLAT.ToString();
                            break;
                        case WaferNotchTypeEnum.NOTCH:
                            Wafer.GetPhysInfo().NotchType.Value = WaferNotchTypeEnum.NOTCH.ToString();
                            break;
                        default:
                            break;
                    }

                    Wafer.GetPhysInfo().DieSizeX.Value = DieSizeX;
                    Wafer.GetPhysInfo().DieSizeY.Value = DieSizeY;
                    Wafer.GetPhysInfo().NotchAngle.Value = NotchAngle;

                    Wafer.GetPhysInfo().NotchAngleOffset.Value = NotchAngleOffset;

                    //Wafer.GetPhysInfo().Thickness.Value = Thickness;
                    Wafer.GetPhysInfo().WaferMargin_um.Value = EdgeMargin;

                    Wafer.ZoomLevelInit = false;

                    Wafer.UpdateData();

                    this.StageSupervisor().SaveWaferObject();

                    InitData();
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
        public async Task MoveToWaferThicknessCommandFunc()
        {
            try
            {
                
                Task task = new Task(() =>
                {
                    CatCoordinates coordinate = CurCam.GetCurCoordPos();
                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetPhysInfo().Thickness.Value);
                    else
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(coordinate.GetX(), coordinate.GetY(), Wafer.GetPhysInfo().Thickness.Value);
                });
                task.Start();
                await task;
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
        public async Task AdjustWaferHeightCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {

                    FocusParam.FocusingCam.Value = CurCam.GetChannelType();
                    FocusModel.Focusing_Retry(FocusParam, false, true, false, this);
                    Wafer.GetPhysInfo().Thickness.Value = Math.Round(CurCam.GetCurCoordPos().GetZ(), 3);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        //public EventCodeEnum GetCardPositionOffset(out double rOffsetX, out double rOffsetY)
        //{
        //    try
        //    {
        //        int i;
        //        double sumX = 0;
        //        double sumY = 0;
        //        double posX = 0;
        //        double posY = 0;
        //        double posZ = 0;
        //        int dirX = 1;
        //        int dirY = 1;
        //        double offsetx = 0.0;
        //        double offsety = 0.0;
        //        double ptxpos = 0.0;
        //        double ptypos = 0.0;
        //        double pos_sumX = 0;
        //        double pos_sumY = 0;

        //        // 카드의 마크 위치를 확인하여 카드 체인지 위치를 보정한다.
        //        // 고정값들은 파라미터로 대체할 것.

        //        // 1. 패턴 매칭 위치로 이동
        //        this.StageSupervisor().StageModuleState.ZCLEARED();
        //        this.StageSupervisor().StageModuleState.SetWaferCamBasePos(true);
        //        this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM);

        //        CurCam.SetLight(EnumLightType.COAXIAL, 255);
        //        CurCam.SetLight(EnumLightType.OBLIQUE, 255);

        //        pos_sumX = 0;
        //        pos_sumY = 0;
        //        sumX = 0;
        //        sumY = 0;
        //        for (i = 1; i <= 4; i++)
        //        {
        //            if (i == 1)
        //            {
        //                posX = 93938;
        //                posY = 109852;
        //                posZ = 13543;

        //                dirX = 1;
        //                dirY = 1;
        //            }
        //            else if (i == 2)
        //            {
        //                posX = -88077;
        //                posY = 112971;
        //                posZ = 13793;

        //                dirX = -1;
        //                dirY = 1;
        //            }
        //            else if (i == 3)
        //            {
        //                posX = -91939;
        //                posY = -109076;
        //                posZ = 13823;

        //                dirX = -1;
        //                dirY = -1;
        //            }
        //            else
        //            {
        //                posX = 90014;
        //                posY = -112161;
        //                posZ = 13533;

        //                dirX = 1;
        //                dirY = -1;
        //            }


        //            this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM);

        //            EnumTrjType trjtype;
        //            trjtype = EnumTrjType.Normal;

        //            this.StageSupervisor().StageModuleState.WaferLowViewMove(posX, posY, trjtype, 1);

        //            ProbeAxisObject axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
        //            this.StageSupervisor().StageModuleState.WaferLowViewMove(axis, posZ);

        //            // 패턴 매칭
        //            PatternInfomation patterninfo = new PatternInfomation();
        //            patterninfo.CamType.Value = EnumProberCam.WAFER_LOW_CAM;
        //            patterninfo.PMParameter.ModelFilePath.Value = $"C:\\ProberSystem\\DEFAULT\\Parameters\\SystemParam\\CardChange\\Pattern\\Ref{i}";
        //            patterninfo.PMParameter.PatternFileExtension.Value = ".mmo";

        //            // 패턴 확인
        //            PMResult pmresult = this.VisionManager().PatternMatching(patterninfo);
        //            this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM);

        //            if (pmresult.RetValue == EventCodeEnum.NONE)
        //            {
        //                // 성공
        //                ptxpos = pmresult.ResultParam[0].XPoss;
        //                ptypos = pmresult.ResultParam[0].YPoss;

        //                offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
        //                offsety = (pmresult.ResultBuffer.SizeY / 2) - ptypos;

        //                offsetx *= pmresult.ResultBuffer.RatioX.Value;
        //                offsety *= pmresult.ResultBuffer.RatioY.Value;

        //                sumX += offsetx;
        //                sumY += offsety;

        //                pos_sumX += (posX - 90000 * dirX);
        //                pos_sumY += (posY - 110000 * dirY);
        //            }
        //            else
        //            {
        //                ////rOffsetX = 0;
        //                ////rOffsetY = 0;
        //                ////LoggerManager.Debug($"GetCardPositionOffset() : Failed to search mark = " + patterninfo.PMParameter.ModelFilePath.Value + patterninfo.PMParameter.PatternFileExtension.Value);
        //                ////return EventCodeEnum.VISION_PATTERN_NOTEXIST;
        //            }
        //        }

        //        if (sumX != 0 && sumY != 0)
        //        {
        //            pos_sumX = pos_sumX / 4;
        //            pos_sumY = pos_sumY / 4;

        //            rOffsetX = (sumX / 4) + pos_sumX;
        //            rOffsetY = (sumY / 4) + pos_sumY;
        //        }
        //        else
        //        {
        //            rOffsetX = 0;
        //            rOffsetY = 0;
        //        }

        //        //this.StageSupervisor().StageModuleState.ZCLEARED();

        //        return EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        rOffsetX = 0;
        //        rOffsetY = 0;
        //        LoggerManager.Exception(err);
        //        return EventCodeEnum.UNDEFINED;
        //    }
        //}

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


        private RelayCommand<Object> _OriginMachineXTextBoxClickCommand;
        public ICommand OriginMachineXTextBoxClickCommand
        {
            get
            {
                if (null == _OriginMachineXTextBoxClickCommand) _OriginMachineXTextBoxClickCommand = new RelayCommand<Object>(OriginMachineXTextBoxClickCommandFunc);
                return _OriginMachineXTextBoxClickCommand;
            }
        }


        private void OriginMachineXTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, Wafer.GetPhysInfo().MapCountX.Value);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().OrgM.XIndex.Value = OrgMachineXCoord;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _OriginMachineYTextBoxClickCommand;
        public ICommand OriginMachineYTextBoxClickCommand
        {
            get
            {
                if (null == _OriginMachineYTextBoxClickCommand) _OriginMachineYTextBoxClickCommand = new RelayCommand<Object>(OriginMachineYTextBoxClickCommandFunc);
                return _OriginMachineYTextBoxClickCommand;
            }
        }


        private void OriginMachineYTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, Wafer.GetPhysInfo().MapCountY.Value);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().OrgM.YIndex.Value = OrgMachineYCoord;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _OriginUserXTextBoxClickCommand;
        public ICommand OriginUserXTextBoxClickCommand
        {
            get
            {
                if (null == _OriginUserXTextBoxClickCommand) _OriginUserXTextBoxClickCommand = new RelayCommand<Object>(OriginUserXTextBoxClickCommandFunc);
                return _OriginUserXTextBoxClickCommand;
            }
        }


        private void OriginUserXTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 9999999);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().OrgU.XIndex.Value = OrgUserXCoord;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _OriginUserYTextBoxClickCommand;
        public ICommand OriginUserYTextBoxClickCommand
        {
            get
            {
                if (null == _OriginUserYTextBoxClickCommand) _OriginUserYTextBoxClickCommand = new RelayCommand<Object>(OriginUserYTextBoxClickCommandFunc);
                return _OriginUserYTextBoxClickCommand;
            }
        }


        private void OriginUserYTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 9999999);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                Wafer.GetPhysInfo().OrgU.YIndex.Value = OrgUserYCoord;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private AsyncCommand _MoveUserCoordPageCommand;
        public ICommand MoveUserCoordPageCommand
        {
            get
            {
                if (null == _MoveUserCoordPageCommand) _MoveUserCoordPageCommand = new AsyncCommand(MoveUserCoordPageCommandFunc);
                return _MoveUserCoordPageCommand;
            }
        }


        private async Task MoveUserCoordPageCommandFunc()
        {
            try
            {
                List<Guid> pnpsteps = new List<Guid>();
                pnpsteps.Add(new Guid("8DC02412-4BA1-E1F4-E62E-0091081C67A8"));
                Guid viewguid = new Guid("1b96aa21-1613-108a-71d6-9bce684a4dd0");
                this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                await this.ViewModelManager().ViewTransitionAsync(viewguid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand _WaferMapModifyCommand;
        public ICommand WaferMapModifyCommand
        {
            get
            {
                if (null == _WaferMapModifyCommand) _WaferMapModifyCommand = new AsyncCommand(WaferMapModifyCommandFunc);
                return _WaferMapModifyCommand;
            }
        }

        private async Task WaferMapModifyCommandFunc()
        {
            try
            {
                this.WaferAligner().IsNewSetup = false;
                List<Guid> pnpsteps = new List<Guid>();
                pnpsteps.Add(new Guid("4203F878-B532-8CCC-2613-D5745D4ED5AE"));
                Guid viewguid = new Guid("1b96aa21-1613-108a-71d6-9bce684a4dd0");
                this.PnPManager().SetNavListToGUIDs(this.WaferAligner(), pnpsteps);
                await this.ViewModelManager().ViewTransitionAsync(viewguid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

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

                    if (point == HeightPointEnum.INVALID)
                    {
                        this.WaferAligner().GetHighStandardParam();
                        HeightPoint = this.WaferAligner().HeightProfilingPointType;
                    }
                    else
                    {
                        this.WaferAligner().SetHighStandardParam(point);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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
        public virtual void ViewSwapFunc(object parameter)
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

        #endregion

        #region //..NotchSetting Porperty& Command & Function

        private int _NotchNorthValue;
        public int NotchNorthValue
        {
            get { return _NotchNorthValue; }
            set
            {
                if (value != _NotchNorthValue)
                {
                    _NotchNorthValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _NotchEastValue;
        public int NotchEastValue
        {
            get { return _NotchEastValue; }
            set
            {
                if (value != _NotchEastValue)
                {
                    _NotchEastValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _NotchWestValue;
        public int NotchWestValue
        {
            get { return _NotchWestValue; }
            set
            {
                if (value != _NotchWestValue)
                {
                    _NotchWestValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _NotchSouthValue;
        public int NotchSouthValue
        {
            get { return _NotchSouthValue; }
            set
            {
                if (value != _NotchSouthValue)
                {
                    _NotchSouthValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _NotchSettingOKCommand;
        public ICommand NotchSettingOKCommand
        {
            get
            {
                if (null == _NotchSettingOKCommand) _NotchSettingOKCommand = new AsyncCommand(NotchSettingOKCommandFunc);
                return _NotchSettingOKCommand;
            }
        }

        private async Task NotchSettingOKCommandFunc()
        {
            try
            {
                //await this.MetroDialogManager().CloseWindow(_NotchSettingViewControl);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _NotchSettingCancelCommand;
        public ICommand NotchSettingCancelCommand
        {
            get
            {
                if (null == _NotchSettingCancelCommand) _NotchSettingCancelCommand = new AsyncCommand(NotchSettingCancelCommandFunc);
                return _NotchSettingCancelCommand;
            }
        }

        private async Task NotchSettingCancelCommandFunc()
        {
            try
            {
                //await this.MetroDialogManager().CloseWindow(_NotchSettingViewControl);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
