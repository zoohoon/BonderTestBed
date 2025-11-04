using ErrorMapping;
using ErrorParam;
using Focusing;
using LogModule;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProbingModule;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UcDisplayPort;
using Vision.DisplayModule;

namespace ExtraCameraDialogVM
{
    public class ExtraCameraDialogViewModel : IFactoryModule, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private bool _IsUsingCenterRactangleDrawer;
        public bool IsUsingCenterRactangleDrawer
        {
            get { return _IsUsingCenterRactangleDrawer; }
            set
            {
                if (value != _IsUsingCenterRactangleDrawer)
                {
                    _IsUsingCenterRactangleDrawer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableChangeCamBtn;
        public bool IsEnableChangeCamBtn
        {
            get { return _IsEnableChangeCamBtn; }
            set
            {
                if (value != _IsEnableChangeCamBtn)
                {
                    _IsEnableChangeCamBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableRestorePreviewCamBtn;
        public bool IsEnableRestorePreviewCamBtn
        {
            get { return _IsEnableRestorePreviewCamBtn; }
            set
            {
                if (value != _IsEnableRestorePreviewCamBtn)
                {
                    _IsEnableRestorePreviewCamBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CenterRectLength = 150;
        public double CenterRectLength
        {
            get { return _CenterRectLength; }
            set
            {
                if (value != _CenterRectLength)
                {
                    _CenterRectLength = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsEnableBlobBtn;
        public bool IsEnableBlobBtn
        {
            get { return _IsEnableBlobBtn; }
            set
            {
                if (_IsEnableBlobBtn != value)
                {
                    _IsEnableBlobBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableInfinityBlobCamBtn;
        public bool IsEnableInfinityBlobCamBtn
        {
            get { return _IsEnableInfinityBlobCamBtn; }
            set
            {
                if (_IsEnableInfinityBlobCamBtn != value)
                {
                    _IsEnableInfinityBlobCamBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableStopInfinityBlobCamBtn;
        public bool IsEnableStopInfinityBlobCamBtn
        {
            get { return _IsEnableStopInfinityBlobCamBtn; }
            set
            {
                if (_IsEnableStopInfinityBlobCamBtn != value)
                {
                    _IsEnableStopInfinityBlobCamBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _BlobX;
        public double BlobX
        {
            get { return _BlobX; }
            set
            {
                if (_BlobX != value)
                {
                    _BlobX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _BlobY;
        public double BlobY
        {
            get { return _BlobY; }
            set
            {
                if (_BlobY != value)
                {
                    _BlobY = value;
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
                if (_CurCam != value)
                {
                    if (_CurCam != null)
                    {
                        VisionManager?.StopGrab(_CurCam.GetChannelType());
                    }

                    _CurCam = value;
                    RaisePropertyChanged();

                    IsEnableChangeCamBtn = true;
                    IsEnableBlobBtn = true;
                    IsEnableInfinityBlobCamBtn = true;
                }
            }
        }

        private IVisionManager VisionManager { get; set; }
        public IPnpManager PnpManager { get; set; }
        public IStageSupervisor StageSupervisor { get; set; }
        public IProbingModule ProbingModule { get; set; }
        public ICoordinateManager CoordinateManager { get; set; }
        private IWaferAligner WaferAligner { get; set; }

        private PNPSetupBase _CameraViewModel;
        private PNPSetupBase CameraViewModel
        {
            get { return _CameraViewModel; }
            set
            {
                if (_CameraViewModel != value)
                {
                    _CameraViewModel = value;
                }
            }
        }

        private ObservableCollection<ICamera> _CamList
            = new ObservableCollection<ICamera>();
        public ObservableCollection<ICamera> CamList
        {
            get { return _CamList; }
            set
            {
                if (value != _CamList)
                {
                    _CamList = value;
                    RaisePropertyChanged(nameof(CamList));
                }
            }
        }


        private float _ZoomLevel;

        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MapViewMode _MapViewControlMode = MapViewMode.MapMode;
        public MapViewMode MapViewControlMode
        {
            get { return _MapViewControlMode; }
            set
            {
                if (value != _MapViewControlMode)
                {
                    _MapViewControlMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Point _MXYIndex = new System.Windows.Point(0, 0);
        public Point MXYIndex
        {
            get { return _MXYIndex; }
            set
            {
                if (value != _MXYIndex)
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    MoveToPosition(value).ContinueWith(async ret =>
                    {
                        retVal = await ret;

                        if (retVal == EventCodeEnum.NONE)
                        {
                            _MXYIndex = value;
                            RaisePropertyChanged();
                        }

                    });
                }
            }
        }

        //Map Sync Index
        private long _CurXIndex;
        public long CurXIndex
        {
            get { return _CurXIndex; }
            set
            {
                if (value != _CurXIndex)
                {
                    _CurXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CurYIndex;
        public long CurYIndex
        {
            get { return _CurYIndex; }
            set
            {
                if (value != _CurYIndex)
                {
                    _CurYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinXPos;
        public double PinXPos
        {
            get { return _PinXPos; }
            set
            {
                if (value != _PinXPos)
                {
                    _PinXPos = value;
                    RaisePropertyChanged(nameof(PinXPos));
                }
            }
        }

        private double _PinYPos;
        public double PinYPos
        {
            get { return _PinYPos; }
            set
            {
                if (value != _PinYPos)
                {
                    _PinYPos = value;
                    RaisePropertyChanged(nameof(PinYPos));
                }
            }
        }

        private double _PinZPos;
        public double PinZPos
        {
            get { return _PinZPos; }
            set
            {
                if (value != _PinZPos)
                {
                    _PinZPos = value;
                    RaisePropertyChanged(nameof(PinZPos));
                }
            }
        }

        private double _WaferXPos;
        public double WaferXPos
        {
            get { return _WaferXPos; }
            set
            {
                if (value != _WaferXPos)
                {
                    _WaferXPos = value;
                    RaisePropertyChanged(nameof(WaferXPos));
                }
            }
        }

        private double _WaferYPos;
        public double WaferYPos
        {
            get { return _WaferYPos; }
            set
            {
                if (value != _WaferYPos)
                {
                    _WaferYPos = value;
                    RaisePropertyChanged(nameof(WaferYPos));
                }
            }
        }

        private double _WaferZPos;
        public double WaferZPos
        {
            get { return _WaferZPos; }
            set
            {
                if (value != _WaferZPos)
                {
                    _WaferZPos = value;
                    RaisePropertyChanged(nameof(WaferZPos));
                }
            }
        }

        private double _PMOffsetX;
        public double PMOffsetX
        {
            get { return _PMOffsetX; }
            set
            {
                if (value != _PMOffsetX)
                {
                    _PMOffsetX = value;
                    RaisePropertyChanged(nameof(PMOffsetX));
                }
            }
        }

        private double _PMOffsetY;
        public double PMOffsetY
        {
            get { return _PMOffsetY; }
            set
            {
                if (value != _PMOffsetY)
                {
                    _PMOffsetY = value;
                    RaisePropertyChanged(nameof(PMOffsetY));
                }
            }
        }

        private double _PMOffsetZ;
        public double PMOffsetZ
        {
            get { return _PMOffsetZ; }
            set
            {
                if (value != _PMOffsetZ)
                {
                    _PMOffsetZ = value;
                    RaisePropertyChanged(nameof(PMOffsetZ));
                }
            }
        }

        private double _PMOffsetT;
        public double PMOffsetT
        {
            get { return _PMOffsetT; }
            set
            {
                if (value != _PMOffsetT)
                {
                    _PMOffsetT = value;
                    RaisePropertyChanged(nameof(PMOffsetT));
                }
            }
        }
        private double _PMTempOffsetX;
        public double PMTempOffsetX
        {
            get { return _PMTempOffsetX; }
            set
            {
                if (value != _PMTempOffsetX)
                {
                    _PMTempOffsetX = value;
                    RaisePropertyChanged(nameof(PMTempOffsetX));
                }
            }
        }

        private double _PMTempOffsetY;
        public double PMTempOffsetY
        {
            get { return _PMTempOffsetY; }
            set
            {
                if (value != _PMTempOffsetY)
                {
                    _PMTempOffsetY = value;
                    RaisePropertyChanged(nameof(PMTempOffsetY));
                }
            }
        }

        private double _PHOffsetX;
        public double PHOffsetX
        {
            get { return _PHOffsetX; }
            set
            {
                if (value != _PHOffsetX)
                {
                    _PHOffsetX = value;
                    RaisePropertyChanged(nameof(PHOffsetX));
                }
            }
        }

        private double _PHOffsetY;
        public double PHOffsetY
        {
            get { return _PHOffsetY; }
            set
            {
                if (value != _PHOffsetY)
                {
                    _PHOffsetY = value;
                    RaisePropertyChanged(nameof(PHOffsetY));
                }
            }
        }

        private double _PHOffsetZ;
        public double PHOffsetZ
        {
            get { return _PHOffsetZ; }
            set
            {
                if (value != _PHOffsetZ)
                {
                    _PHOffsetZ = value;
                    RaisePropertyChanged(nameof(PHOffsetZ));
                }
            }
        }

        private double _PHOffsetT;
        public double PHOffsetT
        {
            get { return _PHOffsetT; }
            set
            {
                if (value != _PHOffsetT)
                {
                    _PHOffsetT = value;
                    RaisePropertyChanged(nameof(PHOffsetT));
                }
            }
        }

        private double _TwistValue;
        public double TwistValue
        {
            get { return _TwistValue; }
            set
            {
                if (value != _TwistValue)
                {
                    _TwistValue = value;
                    RaisePropertyChanged(nameof(TwistValue));
                }
            }
        }

        private double _SquarenceValue;
        public double SquarenceValue
        {
            get { return _SquarenceValue; }
            set
            {
                if (value != _SquarenceValue)
                {
                    _SquarenceValue = value;
                    RaisePropertyChanged(nameof(SquarenceValue));
                }
            }
        }

        private double _DeflectX;
        public double DeflectX
        {
            get { return _DeflectX; }
            set
            {
                if (value != _DeflectX)
                {
                    _DeflectX = value;
                    RaisePropertyChanged(nameof(DeflectX));
                }
            }
        }

        private double _DeflectY;
        public double DeflectY
        {
            get { return _DeflectY; }
            set
            {
                if (value != _DeflectY)
                {
                    _DeflectY = value;
                    RaisePropertyChanged(nameof(DeflectY));
                }
            }
        }

        private double _DeflectZ;
        public double DeflectZ
        {
            get { return _DeflectZ; }
            set
            {
                if (value != _DeflectZ)
                {
                    _DeflectZ = value;
                    RaisePropertyChanged(nameof(DeflectZ));
                }
            }
        }

        private double _InclineZHor;
        public double InclineZHor
        {
            get { return _InclineZHor; }
            set
            {
                if (value != _InclineZHor)
                {
                    _InclineZHor = value;
                    RaisePropertyChanged(nameof(InclineZHor));
                }
            }
        }

        private double _InclineZVer;
        public double InclineZVer
        {
            get { return _InclineZVer; }
            set
            {
                if (value != _InclineZVer)
                {
                    _InclineZVer = value;
                    RaisePropertyChanged(nameof(InclineZVer));
                }
            }
        }

        private WriteableBitmap _OverlapImage;
        public WriteableBitmap OverlapImage
        {
            get { return _OverlapImage; }
            set
            {
                if (value != _OverlapImage)
                {
                    _OverlapImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CanvasWidth;
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set
            {
                if (value != _CanvasWidth)
                {
                    _CanvasWidth = value;
                    RaisePropertyChanged(nameof(CanvasWidth));
                    LineFromCenterToMouse.StartPoint = new Point(_CanvasWidth / 2.0, _CanvasHeight / 2.0);
                }
            }
        }

        private double _CanvasHeight;
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set
            {
                if (value != _CanvasHeight)
                {
                    _CanvasHeight = value;
                    RaisePropertyChanged(nameof(CanvasHeight));
                    LineFromCenterToMouse.StartPoint = new Point(_CanvasWidth / 2.0, _CanvasHeight / 2.0);
                }
            }
        }

        private Point _MousePosition;
        public Point MousePosition
        {
            get { return _MousePosition; }
            set
            {
                if (value != _MousePosition)
                {
                    _MousePosition = value;
                    RaisePropertyChanged(nameof(MousePosition));

                    LineFromCenterToMouse.EndPoint = new Point(_MousePosition.X, _MousePosition.Y);
                }
            }
        }

        private LineClass _LineFromCenterToMouse = new LineClass();
        public LineClass LineFromCenterToMouse
        {
            get { return _LineFromCenterToMouse; }
            set
            {
                if (value != _LineFromCenterToMouse)
                {
                    _LineFromCenterToMouse = value;
                    RaisePropertyChanged(nameof(LineFromCenterToMouse));
                }
            }
        }

        private string _ConvertPointStr;
        public string ConvertPointStr
        {
            get { return _ConvertPointStr; }
            set
            {
                if (value != _ConvertPointStr)
                {
                    _ConvertPointStr = value;
                    RaisePropertyChanged(nameof(ConvertPointStr));
                }
            }
        }

        private double _XPosRate = 1;
        public double XPosRate
        {
            get { return _XPosRate; }
            set
            {
                if (value != _XPosRate)
                {
                    _XPosRate = value;
                    RaisePropertyChanged(nameof(XPosRate));
                }
            }
        }

        private double _YPosRate = 1;
        public double YPosRate
        {
            get { return _YPosRate; }
            set
            {
                if (value != _YPosRate)
                {
                    _YPosRate = value;
                    RaisePropertyChanged(nameof(YPosRate));
                }
            }
        }

        private bool _IsUsingLineTracer;
        public bool IsUsingLineTracer
        {
            get { return _IsUsingLineTracer; }
            set
            {
                if (value != _IsUsingLineTracer)
                {
                    _IsUsingLineTracer = value;
                    RaisePropertyChanged(nameof(IsUsingLineTracer));
                }
            }
        }

        private bool _IsMeasureDistanceMode;
        public bool IsMeasureDistanceMode
        {
            get { return _IsMeasureDistanceMode; }
            set
            {
                if (value != _IsMeasureDistanceMode)
                {
                    _IsMeasureDistanceMode = value;
                    RaisePropertyChanged(nameof(IsMeasureDistanceMode));
                }
            }
        }

        #region //..CPC

        private ObservableCollection<IChuckPlaneCompParameter> _CPCParams
             = new ObservableCollection<IChuckPlaneCompParameter>();
        public ObservableCollection<IChuckPlaneCompParameter> CPCParams
        {
            get { return _CPCParams; }
            set
            {
                if (value != _CPCParams)
                {
                    _CPCParams = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _CPCPosition;
        public double CPCPosition
        {
            get { return _CPCPosition; }
            set
            {
                if (value != _CPCPosition)
                {
                    _CPCPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPCZ0;
        public double CPCZ0
        {
            get { return _CPCZ0; }
            set
            {
                if (value != _CPCZ0)
                {
                    _CPCZ0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPCZ1;
        public double CPCZ1
        {
            get { return _CPCZ1; }
            set
            {
                if (value != _CPCZ1)
                {
                    _CPCZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPCZ2;
        public double CPCZ2
        {
            get { return _CPCZ2; }
            set
            {
                if (value != _CPCZ2)
                {
                    _CPCZ2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..ErrorMapping
        private ErrorMapping2DManager errormanager;
        //private IFocusing focusingModule { get; set; }
        //private IFocusParameter focusParameter { get; set; }

        private IFocusing _FocusModel;
        public IFocusing FocusModel
        {
            get
            {
                if (_FocusModel == null)
                    _FocusModel = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());

                return _FocusModel;
            }
        }

        private IFocusParameter FocusParam;


        public IMotionManager MotionManager
        {
            get { return this.MotionManager(); }
        }

        #endregion

        #region ==> UcDispaly Port Target Rectangle

        private UserControlFucEnum _UseUserControl
           = UserControlFucEnum.DEFAULT;
        public UserControlFucEnum UseUserControl
        {
            get { return _UseUserControl; }
            set
            {
                if (value != _UseUserControl)
                {
                    _UseUserControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TargetRectangleLeft;
        public double TargetRectangleLeft
        {
            get { return _TargetRectangleLeft; }
            set
            {
                if (value != _TargetRectangleLeft)
                {
                    _TargetRectangleLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TargetRectangleTop;
        public double TargetRectangleTop
        {
            get { return _TargetRectangleTop; }
            set
            {
                if (value != _TargetRectangleTop)
                {
                    _TargetRectangleTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TargetRectangleWidth;
        public double TargetRectangleWidth
        {
            get { return _TargetRectangleWidth; }
            set
            {
                if (value != _TargetRectangleWidth)
                {
                    if (!(value < 4))
                    {
                        _TargetRectangleWidth = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        private double _TargetRectangleHeight;
        public double TargetRectangleHeight
        {
            get { return _TargetRectangleHeight; }
            set
            {
                if (value != _TargetRectangleHeight)
                {
                    if (!(value < 4))
                    {
                        _TargetRectangleHeight = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        private bool _DisplayClickToMoveEnalbe = true;
        public bool DisplayClickToMoveEnalbe
        {
            get { return _DisplayClickToMoveEnalbe; }
            set
            {
                if (value != _DisplayClickToMoveEnalbe)
                {
                    _DisplayClickToMoveEnalbe = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ChangeWidthValue = 4;
        public int ChangeHeightValue = 4;

        public double PatternSizeWidth;
        public double PatternSizeHeight;
        public double PatternSizeLeft;
        public double PatternSizeTop;


        string patternpath = @"C:\Logs\PTTestPattern\pattern.mmo";
        PatternInfomation ptinfo = null;
        System.Threading.CancellationTokenSource cancelToken = null;

        #endregion


        public ExtraCameraDialogViewModel()
        {
            try
            {
                SetCameraViewModel();

                ProbingModule = this.ProbingModule();
                StageSupervisor = this.StageSupervisor();
                VisionManager = this.VisionManager();
                CoordinateManager = this.CoordinateManager();

                IsEnableChangeCamBtn = false;
                IsEnableRestorePreviewCamBtn = true;
                IsEnableBlobBtn = false;
                IsEnableInfinityBlobCamBtn = false;
                IsUsingLineTracer = false;
                IsMeasureDistanceMode = false;

                //////////////////////
                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                this.StageSupervisor().StageModuleState.VMViewMove(0, 0, axisz.Param.HomeOffset.Value);
                //////////////////////




                foreach (var camera in VisionManager.GetCameras())
                {
                    this.CamList.Add(camera);
                }

                DisplayPort = new DisplayPort() { GUID = new Guid("24EEEAE9-620A-4D6D-9363-06EE35B6B360") };
                PnpManager = this.PnPManager();

                var RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.MAP_REF_CAM);
                if (RefCam != -1)
                {
                    VisionManager.SetDisplayChannel(VisionManager.GetCameras()[RefCam], DisplayPort);
                }
                CatCoordinates pmshift = new CatCoordinates();
                pmshift = ProbingModule.GetPMShifhtValue();

                PMOffsetX = pmshift.X.Value;
                PMOffsetY = pmshift.Y.Value;
                PMOffsetZ = pmshift.Z.Value;
                PMOffsetT = pmshift.T.Value;

                PHOffsetX = this.CoordinateManager().StageCoord.PHOffset.X.Value;
                PHOffsetY = this.CoordinateManager().StageCoord.PHOffset.Y.Value;
                PHOffsetZ = this.CoordinateManager().StageCoord.PHOffset.Z.Value;
                PHOffsetT = this.CoordinateManager().StageCoord.PHOffset.T.Value;

                TwistValue = ProbingModule.GetTwistValue();
                SquarenceValue = ProbingModule.GetSquarenceValue();
                DeflectX = ProbingModule.GetDeflectX();
                DeflectY = ProbingModule.GetDeflectY();


                InclineZHor = ProbingModule.GetInclineZHor();
                InclineZVer = ProbingModule.GetInclineZVer();

                CPCParams = ProbingModule.GetCPCValues();

                #region //..Binding DisplayPort

                TargetRectangleHeight = 128;
                TargetRectangleWidth = 128;

                Binding bindTargetRectWidth = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(TargetRectangleWidth)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Width, bindTargetRectWidth);

                Binding bindTargetRectHeight = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(TargetRectangleHeight)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Height, bindTargetRectHeight);

                Binding bindTargetRectLeft = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(TargetRectangleLeft)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Left, bindTargetRectLeft);

                Binding bindTargetRectTop = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(TargetRectangleTop)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.TargetRectangle_Top, bindTargetRectTop);

                Binding bindUseUserControl = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(UseUserControl)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.UseUserControlFuncProp, bindUseUserControl);

                Binding bindCamera = new Binding
                {
                    Path = new System.Windows.PropertyPath(nameof(CurCam)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);
                #endregion

                #region //..VisionMapping
                errormanager = new ErrorMapping2DManager();
                var table = errormanager.ErrorTable2D;

                FocusParam = new NormalFocusParameter();
                FocusParam.SetDefaultParam();
                FocusParam.FocusingCam.Value = EnumProberCam.MAP_REF_CAM;
                FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;

                int mXIndex = (int)(StageSupervisor.WaferObject.GetPhysInfo().MapCountX.Value / 2);
                int mYIndex = (int)(StageSupervisor.WaferObject.GetPhysInfo().MapCountY.Value / 2);

                _MXYIndex = new Point(mXIndex, mYIndex);

                //this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness);

                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async Task<EventCodeEnum> MoveToPosition(System.Windows.Point mxyIndex)
        {
            EventCodeEnum moveResult = EventCodeEnum.UNDEFINED;
            try
            {

                await Task.Run(() =>
                {
                    double zc = 0;
                    WaferCoordinate waferCoordinate = null;
                    //MachineCoordinate moveCoordinate = null;
                    PinCoordinate pinCoordinate = new PinCoordinate();

                    try
                    {
                        //zc = ProbingModule.ZClearence;
                        waferCoordinate = this.WaferAligner().MachineIndexConvertToProbingCoord((int)mxyIndex.X, (int)mxyIndex.Y);

                        pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                        pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                        pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                        //if (!(this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE
                        //&& this.StageSupervisor().WaferObject.GetSubsInfo().GetAlignState() == AlignStateEnum.DONE))
                        //{
                        //    var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                        //    moveCoordinate = new MachineCoordinate(0, 0, axispz.Param.ClearedPosition.Value);
                        //    var coord = this.CoordinateManager().PinHighPinConvert.Convert(moveCoordinate);
                        //    pinCoordinate.Z.Value = coord.Z.Value;

                        //    var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                        //    moveCoordinate = new MachineCoordinate(0, 0, axisz.Param.ClearedPosition.Value);
                        //    var wcoord = this.CoordinateManager().WaferHighChuckConvert.Convert(moveCoordinate);
                        //    waferCoordinate.Z.Value = wcoord.Z.Value;
                        //}

                        moveResult = StageSupervisor.StageModuleState.MovePadToPin(waferCoordinate, pinCoordinate, zc);
                        CurXIndex = Convert.ToInt64(mxyIndex.X);
                        CurYIndex = Convert.ToInt64(mxyIndex.Y);

                        //moveResult = StageSupervisor.StageModuleState.WaferHighViewIndexMove((int)mxyIndex.X, (int)mxyIndex.Y);
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                        moveResult = EventCodeEnum.UNDEFINED;
                        LoggerManager.Error("[ManualContactControlVM] MoveToPosition Error");
                    }
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moveResult;
        }


        private RelayCommand _UpdateCPCCommand;
        public ICommand UpdateCPCCommand
        {
            get
            {
                if (null == _UpdateCPCCommand) _UpdateCPCCommand = new RelayCommand(UpdateCPCCommandFunc);
                return _UpdateCPCCommand;
            }
        }

        public void UpdateCPCCommandFunc()
        {
            try
            {
                List<ChuckPlaneCompParameter> cpcparams = new List<ChuckPlaneCompParameter>();
                foreach (var param in CPCParams)
                {
                    cpcparams.Add((ChuckPlaneCompParameter)param);
                }
                (ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam).CPC.Value
                    = cpcparams;
                ProbingModule.SaveSysParameter();
                CPCParams = ProbingModule.GetCPCValues();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _AddCPCCommand;
        public ICommand AddCPCCommand
        {
            get
            {
                if (null == _AddCPCCommand) _AddCPCCommand = new RelayCommand(AddCPCCommandFunc);
                return _AddCPCCommand;
            }
        }

        public void AddCPCCommandFunc()
        {
            try
            {
                ProbingModule.AddCPCParameter(new ChuckPlaneCompParameter(CPCPosition, CPCZ0, CPCZ1, CPCZ2));
                ProbingModule.SaveSysParameter();
                CPCParams = ProbingModule.GetCPCValues();
                CPCPosition = 0;
                CPCZ0 = 0;
                CPCZ1 = 0;
                CPCZ2 = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private RelayCommand _ChangeCamCommand;
        public ICommand ChangeCamCommand
        {
            get
            {
                if (null == _ChangeCamCommand) _ChangeCamCommand = new RelayCommand(ChangeCamFunc);
                return _ChangeCamCommand;
            }
        }

        private void ChangeCamFunc()
        {
            try
            {
                EventCodeEnum grabResult = EventCodeEnum.UNDEFINED;
                if (CurCam != null)
                {
                    VisionManager.SetDisplayChannel(CurCam, DisplayPort);
                    
                    grabResult = VisionManager.StartGrab(CurCam.GetChannelType(), this);

                    if (CurCam.GetVerticalFlip() == FlipEnum.FLIP)
                        CamVerFlip = true;
                    else
                        CamVerFlip = false;

                    if (CurCam.GetHorizontalFlip() == FlipEnum.FLIP)
                        CamHorFlip = true;
                    else
                        CamHorFlip = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _StopCurCameraCommand;
        public ICommand StopCurCameraCommand
        {
            get
            {
                if (null == _StopCurCameraCommand) _StopCurCameraCommand = new RelayCommand(StopCurCameraFunc);
                return _StopCurCameraCommand;
            }
        }

        private void StopCurCameraFunc()
        {
            try
            {
                SetCameraViewModel();
                if (CameraViewModel?.CurCam?.GetChannelType() != CurCam?.GetChannelType())
                {
                    if (CurCam != null)
                        VisionManager.StopGrab(CurCam.GetChannelType());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool HasCamera()
        {
            bool retVal = false;
            try
            {

                SetCameraViewModel();

                //retVal = CameraViewModel != null;

                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SetCameraViewModel()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow;
                var mainWindowDatacontext = mainWindow.DataContext as IViewModelManager;
                var mainScreenView = (mainWindowDatacontext?.MainScreenView as FrameworkElement)?.DataContext;

                CameraViewModel = mainScreenView as PNPSetupBase;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _BlobCommand;
        public ICommand BlobCommand
        {
            get
            {
                if (null == _BlobCommand) _BlobCommand = new RelayCommand(BlobFunc);
                return _BlobCommand;
            }
        }

        bool IsBlobProcessing = false;
        private void BlobFunc()
        {
            try
            {
                if (!IsBlobProcessing)
                {
                    IsBlobProcessing = true;
                    EventCodeEnum grabResult = EventCodeEnum.UNDEFINED;
                    double imgPosX = 0.0;
                    double imgPosY = 0.0;
                    BlobResult blobResult;

                    int foundgraylevel = 0;

                    int SizeX = 0;
                    int SizeY = 50;

                    int OffsetX = (int)CurCam.GetGrabSizeWidth() / 2 - Convert.ToInt32(SizeX) / 2;
                    int OffsetY = (int)CurCam.GetGrabSizeHeight() / 2 - Convert.ToInt32(SizeY) / 2;

                    blobResult = VisionManager.FindBlob(CurCam.GetChannelType(), ref imgPosX, ref imgPosY, ref foundgraylevel, 127, 2500, 25000, OffsetX, OffsetY, SizeX, SizeY);

                    BlobX = imgPosX;
                    BlobY = imgPosY;

                    if (CurCam != null)
                    {
                        grabResult = VisionManager.StartGrab(CurCam.GetChannelType(), this);
                    }

                    IsBlobProcessing = false;
                }
            }
            catch (Exception)
            {
                IsBlobProcessing = false;
            }
        }

        public void Dispose()
        {
            try
            {
                StopCurCameraFunc();

                //this.StageSupervisor().WaferObject.LoadDevParameter();
                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _MarkOnCommand;
        public ICommand MarkOnCommand
        {
            get
            {
                if (null == _MarkOnCommand) _MarkOnCommand = new RelayCommand(MarkOnFunc);
                return _MarkOnCommand;
            }
        }

        public void MarkOnFunc()
        {
            try
            {
                VisionManager.GetCam(CurCam.GetChannelType()).SetLight(EnumLightType.AUX, 127);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _MarkOffCommand;
        public ICommand MarkOffCommand
        {
            get
            {
                if (null == _MarkOffCommand) _MarkOffCommand = new RelayCommand(MarkOffFunc);
                return _MarkOffCommand;
            }
        }

        public void MarkOffFunc()
        {
            try
            {
                VisionManager.GetCam(CurCam.GetChannelType()).SetLight(EnumLightType.AUX, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ZClearenceCommand;
        public ICommand ZClearenceCommand
        {
            get
            {
                if (null == _ZClearenceCommand) _ZClearenceCommand = new RelayCommand(ZClearenceFunc);
                return _ZClearenceCommand;
            }
        }

        public void ZClearenceFunc()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                retVal = StageSupervisor.StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ZUpCommand;
        public ICommand ZUpCommand
        {
            get
            {
                if (null == _ZUpCommand) _ZUpCommand = new RelayCommand(ZUpFunc);
                return _ZUpCommand;
            }
        }

        public void ZUpFunc()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                WaferCoordinate waferCoordinate = null;
                PinCoordinate pinCoordinate = null;
                pinCoordinate = new PinCoordinate(this.PinXPos, this.PinYPos, this.PinZPos);
                waferCoordinate = new WaferCoordinate(this.WaferXPos, this.WaferYPos, this.WaferZPos);

                pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                waferCoordinate = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MXYIndex.X, (int)MXYIndex.Y);

                try
                {
                    retVal = StageSupervisor.StageModuleState.ProbingZUP(
                        waferCoordinate,
                        pinCoordinate,
                        0);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception(retVal.ToString());
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    retVal = EventCodeEnum.UNDEFINED;
                    //throw new Exception(e.Message);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ZDownCommand;
        public ICommand ZDownCommand
        {
            get
            {
                if (null == _ZDownCommand) _ZDownCommand = new RelayCommand(ZDownFunc);
                return _ZDownCommand;
            }
        }

        public void ZDownFunc()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                WaferCoordinate waferCoordinate = null;
                PinCoordinate pinCoordinate = null;
                //MachineCoordinate moveCoordinate = null;
                double od = ProbingModule.OverDrive;
                double zc = ProbingModule.ZClearence;
                zc = ProbingModule.CalculateZClearenceUsingOD(od, zc);

                waferCoordinate = new WaferCoordinate(this.WaferXPos, this.WaferYPos, this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness);
                pinCoordinate = new PinCoordinate(this.StageSupervisor().ProbeCardInfo.GetProbeCardCenterPos().X.Value,
                    this.StageSupervisor().ProbeCardInfo.GetProbeCardCenterPos().Y.Value,
                    this.StageSupervisor().ProbeCardInfo.GetProbeCardCenterPos().Z.Value);

                //var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                //moveCoordinate = new MachineCoordinate(0, 0, axispz.Param.ClearedPosition.Value);
                //var coord = this.CoordinateManager().PinHighPinConvert.Convert(moveCoordinate);
                //pinCoordinate.Z.Value = coord.Z.Value;

                //var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                //moveCoordinate = new MachineCoordinate(0, 0, axisz.Param.ClearedPosition.Value);
                //var wcoord = this.CoordinateManager().WaferHighChuckConvert.Convert(moveCoordinate);
                ////var mwcoord = this.CoordinateManager().WaferHighChuckConvert.ConvertBack(wcoord);
                //waferCoordinate.Z.Value = wcoord.Z.Value;

                retVal = StageSupervisor.StageModuleState.ProbingZDOWN(
                    waferCoordinate,
                    pinCoordinate,
                    od, zc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _GetPinPosCommand;
        public ICommand GetPinPosCommand
        {
            get
            {
                if (null == _GetPinPosCommand) _GetPinPosCommand = new RelayCommand(GetPinPosFunc);
                return _GetPinPosCommand;
            }
        }
        public void GetPinPosFunc()
        {
            try
            {
                var RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM);

                if (RefCam != -1)
                {
                    PinXPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.X.Value ?? 0;
                    PinYPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.Y.Value ?? 0;
                    PinZPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.Z.Value ?? 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private RelayCommand _GetWaferPosCommand;
        public ICommand GetWaferPosCommand
        {
            get
            {
                if (null == _GetWaferPosCommand) _GetWaferPosCommand = new RelayCommand(GetWaferPosFunc);
                return _GetWaferPosCommand;
            }
        }
        public void GetWaferPosFunc()
        {
            try
            {
                var RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.WAFER_HIGH_CAM);

                if (RefCam != -1)
                {
                    WaferXPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.X.Value ?? 0;
                    WaferYPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.Y.Value ?? 0;
                    WaferZPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.Z.Value ?? 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private RelayCommand _PinPadMatchCommand;
        public ICommand PinPadMatchCommand
        {
            get
            {
                if (null == _PinPadMatchCommand) _PinPadMatchCommand = new RelayCommand(PinPadMatchFunc);
                return _PinPadMatchCommand;
            }
        }

        public void PinPadMatchFunc()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                WaferCoordinate waferCoordinate = null;
                PinCoordinate pinCoordinate = null;
                MachineCoordinate moveCoordinate = null;
                double od = ProbingModule.OverDrive;
                double zc = ProbingModule.ZClearence;
                zc = ProbingModule.CalculateZClearenceUsingOD(od, zc);

                waferCoordinate = new WaferCoordinate(this.WaferXPos, this.WaferYPos, this.WaferZPos);
                pinCoordinate = new PinCoordinate(this.PinXPos, this.PinYPos, this.PinZPos);

                var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                moveCoordinate = new MachineCoordinate(0, 0, axispz.Param.ClearedPosition.Value);
                var coord = this.CoordinateManager().PinHighPinConvert.Convert(moveCoordinate);
                pinCoordinate.Z.Value = coord.Z.Value;

                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                moveCoordinate = new MachineCoordinate(0, 0, axisz.Param.ClearedPosition.Value);
                var wcoord = this.CoordinateManager().WaferHighChuckConvert.Convert(moveCoordinate);
                waferCoordinate.Z.Value = wcoord.Z.Value;

                retVal = StageSupervisor.StageModuleState.MovePadToPin(waferCoordinate, pinCoordinate, zc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ChangePMOffsetCommand;
        public ICommand ChangePMOffsetCommand
        {
            get
            {
                if (null == _ChangePMOffsetCommand) _ChangePMOffsetCommand = new RelayCommand(ChangePMOffsetFunc);
                return _ChangePMOffsetCommand;
            }
        }

        public void ChangePMOffsetFunc()
        {
            try
            {
                ICoordinateManager CoordinateManager = this.CoordinateManager();
                CatCoordinates setProbeValue = new CatCoordinates();
                setProbeValue.X.Value = PMOffsetX;
                setProbeValue.Y.Value = PMOffsetY;
                setProbeValue.Z.Value = PMOffsetZ;
                setProbeValue.T.Value = PMOffsetT;

                ProbingModule.SetProbeShiftValue(setProbeValue);
                //CoordinateManager.StageCoord.PHOffset.X.Value = PMOffsetX;
                //CoordinateManager.StageCoord.PHOffset.Y.Value = PMOffsetY;
                //CoordinateManager.StageCoord.PHOffset.Z.Value = PMOffsetZ;
                //CoordinateManager.StageCoord.PHOffset.T.Value = PMOffsetT;
                ProbingModule.SaveSysParameter();
                //CoordinateManager.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ChangePMTempOffsetCommand;
        public ICommand ChangePMTempOffsetCommand
        {
            get
            {
                if (null == _ChangePMTempOffsetCommand) _ChangePMTempOffsetCommand = new RelayCommand(ChangePMTempOffsetFunc);
                return _ChangePMTempOffsetCommand;
            }
        }

        public void ChangePMTempOffsetFunc()
        {
            try
            {
                CatCoordinates setProbeValue = new CatCoordinates();
                setProbeValue.X.Value = PMTempOffsetX;
                setProbeValue.Y.Value = PMTempOffsetY;
                setProbeValue.Z.Value = 0;
                setProbeValue.T.Value = 0;

                ProbingModule.SetProbeTempShiftValue(setProbeValue);
                ProbingModule.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _NCHPSystemParameterSaveCommand;
        public ICommand NCHPSystemParameterSaveCommand
        {
            get
            {
                if (null == _NCHPSystemParameterSaveCommand) _NCHPSystemParameterSaveCommand = new RelayCommand(NCHPSystemParameterSaveCmd);
                return _NCHPSystemParameterSaveCommand;
            }
        }

        private void NCHPSystemParameterSaveCmd()
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    retval = (this.NeedleCleaner().NCHeightProfilingModule as IHasSysParameterizable).LoadSysParameter();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ChangePHOffsetCommand;
        public ICommand ChangeWHOffsetCommand
        {
            get
            {
                if (null == _ChangePHOffsetCommand) _ChangePHOffsetCommand = new RelayCommand(ChangePHOffsetFunc);
                return _ChangePHOffsetCommand;
            }
        }

        public void ChangePHOffsetFunc()
        {
            try
            {
                ICoordinateManager CoordinateManager = this.CoordinateManager();

                CoordinateManager.StageCoord.PHOffset.X.Value = PHOffsetX;
                CoordinateManager.StageCoord.PHOffset.Y.Value = PHOffsetY;
                CoordinateManager.StageCoord.PHOffset.Z.Value = PHOffsetZ;
                CoordinateManager.StageCoord.PHOffset.T.Value = PHOffsetT;


                //CoordinateManager.StageCoord.WHOffset.X.Value = WHOffsetX;
                //CoordinateManager.StageCoord.WHOffset.Y.Value = WHOffsetY;
                //CoordinateManager.StageCoord.WHOffset.Z.Value = WHOffsetZ;
                //CoordinateManager.StageCoord.WHOffset.T.Value = WHOffsetT;

                CoordinateManager.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _SetOverlapCommand;
        public ICommand SetOverlapCommand
        {
            get
            {
                if (null == _SetOverlapCommand) _SetOverlapCommand = new RelayCommand(SetOverlapFunc);
                return _SetOverlapCommand;
            }
        }

        public void SetOverlapFunc()
        {
            try
            {
                ImageBuffer imgBuf;
                CurCam.GetCurImage(out imgBuf);
                OverlapImage = ImageConverter.WriteableBitmapFromArray(
                                    imgBuf.Buffer, imgBuf.SizeX, imgBuf.SizeY, OverlapImage);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ResetOverlapCommand;
        public ICommand ResetOverlapCommand
        {
            get
            {
                if (null == _ResetOverlapCommand) _ResetOverlapCommand = new RelayCommand(ResetOverlapFunc);
                return _ResetOverlapCommand;
            }
        }

        public void ResetOverlapFunc()
        {
            OverlapImage = null;
        }

        private RelayCommand<object> _MeasureDistanceCommand;
        public ICommand MeasureDistanceCommand
        {
            get
            {
                if (null == _MeasureDistanceCommand) _MeasureDistanceCommand = new RelayCommand<object>(MeasureDistanceFunc);
                return _MeasureDistanceCommand;
            }
        }

        private double MeasureStandardXPoint = 0;
        private double MeasureStandardYPoint = 0;
        private double PanelXPos = 0;
        private double PanelYPos = 0;
        double destXPos = 0;
        double destYPos = 0;

        public void MeasureDistanceFunc(object obj)
        {
            try
            {
                if (IsMeasureDistanceMode == true)
                {
                    var RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM);

                    if (RefCam < 0)
                    {
                        RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.WAFER_HIGH_CAM);
                    }

                    if (RefCam != -1)
                    {
                        IInputElement DisplayPortInputElement = DisplayPort as IInputElement;
                        FrameworkElement DisplayPortFramewolrkElement = DisplayPort as FrameworkElement;
                        Point displayPortMousePoint = Mouse.GetPosition(DisplayPortInputElement);
                        PanelXPos = displayPortMousePoint.X - (DisplayPortFramewolrkElement.ActualWidth / 2);
                        PanelYPos = (DisplayPortFramewolrkElement.ActualHeight / 2) - displayPortMousePoint.Y;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _MeasureDistanceModeCommand;
        public ICommand MeasureDistanceModeCommand
        {
            get
            {
                if (null == _MeasureDistanceModeCommand) _MeasureDistanceModeCommand = new RelayCommand(MeasureDistanceModeFunc);
                return _MeasureDistanceModeCommand;
            }
        }

        public void MeasureDistanceModeFunc()
        {
            try
            {
                if (IsMeasureDistanceMode == false)
                {
                    var RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM);
                    if (RefCam < 0)
                    {
                        RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.WAFER_HIGH_CAM);
                    }

                    destXPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.X.Value ?? 0;
                    destYPos = VisionManager.GetCameras()[RefCam]?.CamSystemPos.Y.Value ?? 0;

                    double XPosDistance = 0;
                    double YPosDistance = 0;

                    XPosDistance = destXPos - MeasureStandardXPoint;
                    YPosDistance = destYPos - MeasureStandardYPoint;

                    XPosRate = XPosDistance / PanelXPos;
                    YPosRate = YPosDistance / PanelYPos;
                }
                else
                {
                    var RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM);

                    if (RefCam < 0)
                    {
                        RefCam = VisionManager.GetCameras().FindIndex(cam => cam.CameraChannel.Type == EnumProberCam.WAFER_HIGH_CAM);
                    }

                    MeasureStandardXPoint = VisionManager.GetCameras()[RefCam]?.CamSystemPos.X.Value ?? 0;
                    MeasureStandardYPoint = VisionManager.GetCameras()[RefCam]?.CamSystemPos.Y.Value ?? 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _MeasureDistanceOnOffCommand;
        public ICommand MeasureDistanceOnOffCommand
        {
            get
            {
                if (null == _MeasureDistanceOnOffCommand) _MeasureDistanceOnOffCommand = new RelayCommand(MeasureDistanceOnOffFunc);
                return _MeasureDistanceOnOffCommand;
            }
        }

        public void MeasureDistanceOnOffFunc()
        {
        }


        private RelayCommand _SaveValueForProbingCommand;
        public ICommand SaveValueForProbingCommand
        {
            get
            {
                if (null == _SaveValueForProbingCommand) _SaveValueForProbingCommand = new RelayCommand(SaveValueForProbingFunc);
                return _SaveValueForProbingCommand;
            }
        }

        public void SaveValueForProbingFunc()
        {
            try
            {
                EventCodeEnum saveResult = EventCodeEnum.UNDEFINED;

                ///
                ProbingModule.SetTwistValue(TwistValue);
                ProbingModule.SetSquarenceValue(SquarenceValue);
                ProbingModule.SetDeflectX(DeflectX);
                ProbingModule.SetDeflectY(DeflectY);
                ProbingModule.SetInclineZHor(InclineZHor);
                ProbingModule.SetInclineZVer(InclineZVer);
                ///

                saveResult = ProbingModule.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _UpCenterRectLengthCommand;
        public ICommand UpCenterRectLengthCommand
        {
            get
            {
                if (null == _UpCenterRectLengthCommand) _UpCenterRectLengthCommand = new RelayCommand(UpCenterRectLengthFunc);
                return _UpCenterRectLengthCommand;
            }
        }

        public void UpCenterRectLengthFunc()
        {
            try
            {
                CenterRectLength += 10;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _DownCenterRectLengthCommand;
        public ICommand DownCenterRectLengthCommand
        {
            get
            {
                if (null == _DownCenterRectLengthCommand) _DownCenterRectLengthCommand = new RelayCommand(DownCenterRectLengthFunc);
                return _DownCenterRectLengthCommand;
            }
        }

        public void DownCenterRectLengthFunc()
        {
            try
            {
                CenterRectLength -= 10;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private RelayCommand _ReloadErrTableCommand;
        public ICommand ReloadErrTableCommand
        {
            get
            {
                if (null == _ReloadErrTableCommand) _ReloadErrTableCommand = new RelayCommand(ReloadErrTableFunc);
                return _ReloadErrTableCommand;
            }
        }

        public void ReloadErrTableFunc()
        {
            try
            {
                errormanager.SaveErrorTable();
                errormanager.LoadErrorTable();
                //this.StageSupervisor().WaferObject.LoadDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private RelayCommand _InitErrTableCommand;
        public ICommand InitErrTableCommand
        {
            get
            {
                if (null == _InitErrTableCommand) _InitErrTableCommand = new RelayCommand(InitErrTableFunc);
                return _InitErrTableCommand;
            }
        }

        public void InitErrTableFunc()
        {
            try
            {
                string message = "If You Execute, Mapping Data will be lost.";
                string caption = "Mapping Data Initialization";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Question;
                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    errormanager.ResetErrorTable();
                    errormanager.SaveErrorTable();
                }
                else
                {
                    // Cancel code here  
                }
                //this.StageSupervisor().WaferObject.LoadDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        #region //..DisplayPort Rect Command & Method
        private RelayCommand _ViewRectCommand;
        public ICommand ViewRectCommand
        {
            get
            {
                if (null == _ViewRectCommand) _ViewRectCommand = new RelayCommand(FuncViewRect);
                return _ViewRectCommand;
            }
        }

        private void FuncViewRect()
        {
            UseUserControl = UserControlFucEnum.PTRECT;
        }

        private RelayCommand _UCDisplayRectWidthPlusCommand;
        public ICommand UCDisplayRectWidthPlusCommand
        {
            get
            {
                if (null == _UCDisplayRectWidthPlusCommand) _UCDisplayRectWidthPlusCommand = new RelayCommand(FuncUCDisplayRectWidthPlus);
                return _UCDisplayRectWidthPlusCommand;
            }
        }
        public void FuncUCDisplayRectWidthPlus()
        {
            try
            {
                ChangeWidthValue = Math.Abs(ChangeWidthValue);
                ModifyUCDisplayRect(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _UCDisplayRectWidthMinusCommand;
        public ICommand UCDisplayRectWidthMinusCommand
        {
            get
            {
                if (null == _UCDisplayRectWidthMinusCommand) _UCDisplayRectWidthMinusCommand = new RelayCommand(FuncUCDisplayRectWidthMinus);
                return _UCDisplayRectWidthMinusCommand;
            }
        }

        public void FuncUCDisplayRectWidthMinus()
        {
            try
            {
                ChangeWidthValue = -Math.Abs(ChangeWidthValue);
                ModifyUCDisplayRect(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _UCDisplayRectHeightMinusCommand;
        public ICommand UCDisplayRectHeightMinusCommand
        {
            get
            {
                if (null == _UCDisplayRectHeightMinusCommand) _UCDisplayRectHeightMinusCommand = new RelayCommand(FuncUCDisplayRectHeightMinus);
                return _UCDisplayRectHeightMinusCommand;
            }
        }

        public void FuncUCDisplayRectHeightMinus()
        {
            try
            {
                ChangeHeightValue = -Math.Abs(ChangeHeightValue);
                ModifyUCDisplayRect(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _UCDisplayRectHeightPlusCommand;
        public ICommand UCDisplayRectHeightPlusCommand
        {
            get
            {
                if (null == _UCDisplayRectHeightPlusCommand) _UCDisplayRectHeightPlusCommand = new RelayCommand(FuncUCDisplayRectHeightPlus);
                return _UCDisplayRectHeightPlusCommand;
            }
        }

        public void FuncUCDisplayRectHeightPlus()
        {
            try
            {
                ChangeHeightValue = Math.Abs(ChangeHeightValue);
                ModifyUCDisplayRect(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EventCodeEnum ModifyUCDisplayRect(bool iswidth = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PatternSizeWidth = TargetRectangleWidth;
                PatternSizeHeight = TargetRectangleHeight;
                PatternSizeLeft = TargetRectangleLeft;
                PatternSizeTop = TargetRectangleTop;
                if (iswidth)
                {
                    PatternSizeWidth += ChangeWidthValue;
                    PatternSizeLeft -= (ChangeWidthValue / 2);
                    TargetRectangleWidth = PatternSizeWidth;
                    PatternSizeWidth = TargetRectangleWidth;
                }
                else
                {
                    PatternSizeHeight += ChangeHeightValue;
                    PatternSizeTop -= (ChangeHeightValue / 2);
                    TargetRectangleHeight = PatternSizeHeight;
                    PatternSizeHeight = TargetRectangleHeight;
                }


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private RelayCommand _MoveToPinHighPosCommand;
        public ICommand MoveToPinHighPosCommand
        {
            get
            {
                if (null == _MoveToPinHighPosCommand) _MoveToPinHighPosCommand = new RelayCommand(MoveToPinHighPosFunc);
                return _MoveToPinHighPosCommand;
            }
        }

        public void MoveToPinHighPosFunc()
        {
            try
            {
                LoggerManager.Debug($"[TeachPinModule] InitBackupData() : Move To Ref Pin Position Start");
                this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value);
                LoggerManager.Debug($"[TeachPinModule] InitBackupData() : Move To Ref Pin Position Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private bool _CamVerFlip;
        public bool CamVerFlip
        {
            get { return _CamVerFlip; }
            set
            {
                if (value != _CamVerFlip)
                {
                    _CamVerFlip = value;
                    RaisePropertyChanged(nameof(CamVerFlip));
                }
            }
        }

        private bool _CamHorFlip;
        public bool CamHorFlip
        {
            get { return _CamHorFlip; }
            set
            {
                if (value != _CamHorFlip)
                {
                    _CamHorFlip = value;
                    RaisePropertyChanged(nameof(CamHorFlip));
                }
            }
        }


        private RelayCommand _ApplyCamFilpCommand;
        public ICommand ApplyCamFilpCommand
        {
            get
            {
                if (null == _ApplyCamFilpCommand) _ApplyCamFilpCommand = new RelayCommand(FuncApplyCamFilp);
                return _ApplyCamFilpCommand;
            }
        }

        public void FuncApplyCamFilp()
        {
            try
            {
                if (CamHorFlip)
                    CurCam.Param.HorizontalFlip.Value = FlipEnum.FLIP;
                else
                    CurCam.Param.HorizontalFlip.Value = FlipEnum.NONE;

                if (CamVerFlip)
                    CurCam.Param.VerticalFlip.Value = FlipEnum.FLIP;
                else
                    CurCam.Param.VerticalFlip.Value = FlipEnum.NONE;

                bool continusgrabflag = false;
                
                if (this.VisionManager().DigitizerService[CurCam.GetDigitizerIndex()].GrabberService.bContinousGrab)
                {
                    this.VisionManager.StopGrab(CurCam.GetChannelType());
                    continusgrabflag = true;
                }

                this.VisionManager().SettingGrab(CurCam.GetChannelType());

                if (continusgrabflag)
                {
                    this.VisionManager.StartGrab(CurCam.GetChannelType(), this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region //..VisionMapping Command & Method
        private AsyncCommand _WaferAlignCommand;
        public ICommand WaferAlignCommand
        {
            get
            {
                if (null == _WaferAlignCommand) _WaferAlignCommand = new AsyncCommand(FuncWaferAlign);
                return _WaferAlignCommand;
            }
        }

        private async Task FuncWaferAlign()
        {
            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();
                await Task.Run(() =>
                {
                    this.WaferAligner().DoManualOperation();

                });
                //this.WaferAligner().DoWaferAlign();
                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                this.StageSupervisor().StageModuleState.VMViewMove(0, 0, axisz.Param.HomeOffset.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _RegistePatternCommand;
        public ICommand RegistePatternCommand
        {
            get
            {
                if (null == _RegistePatternCommand) _RegistePatternCommand = new AsyncCommand(RegistePatternFunc);
                return _RegistePatternCommand;
            }
        }

        private async Task RegistePatternFunc()
        {
            try
            {
                await Task.Run(() =>
                {
                    RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();
                    patternparam.CamType = CurCam.GetChannelType();
                    patternparam.PatternPath = patternpath;
                    this.VisionManager().SavePattern(patternparam);
                    ptinfo = new PatternInfomation();
                    ptinfo.CamType.Value = CurCam.GetChannelType();
                    ptinfo.PMParameter = new ProberInterfaces.Vision.PMParameter();
                    ptinfo.PMParameter.ModelFilePath.Value = @"C:\Logs\PTTestPattern\pattern";
                    ptinfo.PMParameter.PatternFileExtension.Value = ".mmo";
                    UseUserControl = UserControlFucEnum.DEFAULT;
                    double xpos = 0.0;
                    double ypos = 0.0;
                    double diesizewidth = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    double diesizeheight = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                    this.MotionManager().GetRefPos(EnumAxisConstants.X, ref xpos);
                    this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref ypos);
                    errormanager.ErrorMappingDataConvert(diesizewidth, diesizeheight, xpos, ypos);
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public RegisteImageBufferParam GetDisplayPortRectInfo()
        {
            RegisteImageBufferParam ret = new RegisteImageBufferParam();
            try
            {

                if (CurCam == null)
                    return ret;


                //double width = (((CurCam.GetGrabSizeWidth() * TargetRectangleWidth) / DisplayPort.StandardOverlayCanvaseWidth));
                //double height = (((CurCam.GetGrabSizeHeight() * TargetRectangleHeight) / DisplayPort.StandardOverlayCanvaseHeight));
                //double offsetx = (((CurCam.GetGrabSizeWidth() * TargetRectangleLeft) / DisplayPort.StandardOverlayCanvaseWidth));
                //double offsety = (((CurCam.GetGrabSizeHeight() * TargetRectangleTop) / DisplayPort.StandardOverlayCanvaseHeight));

                ret.Width = (int)(CurCam.GetGrabSizeWidth() * ((480 * TargetRectangleWidth) / DisplayPort.StandardOverlayCanvaseWidth)) / 480;
                ret.Height = (int)(CurCam.GetGrabSizeHeight() * ((480 * TargetRectangleHeight) / DisplayPort.StandardOverlayCanvaseHeight)) / 480;
                ret.LocationX = (int)CurCam.GetGrabSizeWidth() / 2 - (ret.Width / 2);
                ret.LocationY = (int)CurCam.GetGrabSizeHeight() / 2 - (ret.Height / 2);

                //ret.Width =(int) (240 - (TargetRectangleWidth / 2))*2 -(72*2) ;
                //ret.Height = (int)(240 - (TargetRectangleHeight / 2))*2- (60*2);
                //ret.LocationY = (int)480 - (ret.Width / 2);
                //ret.LocationX = (int)480 - (ret.Height / 2);


            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString() }. PnpSetupBase - GetPatternRectInfo()  : Error occured. ");
            }
            return ret;
        }

        private AsyncCommand _AutoFocusCommand;
        public ICommand AutoFocusCommand
        {
            get
            {
                if (null == _AutoFocusCommand) _AutoFocusCommand = new AsyncCommand(FuncAutoFocus);
                return _AutoFocusCommand;
            }
        }

        private async Task FuncAutoFocus()
        {
            try
            {
                await Task.Run(() =>
                {
                    FocusModel.Focusing_Retry(FocusParam, false, true, true, this);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MeasureCenterXCommand;
        public ICommand MeasureCenterXCommand
        {
            get
            {
                if (null == _MeasureCenterXCommand) _MeasureCenterXCommand = new AsyncCommand(FuncMeasureCenterX);
                return _MeasureCenterXCommand;
            }
        }

        private async Task FuncMeasureCenterX()
        {
            try
            {
                await Task.Run(() =>
                {
                    //for (int index = 2; index < 21; index++)
                    //{
                    //    MeasureHorizental(this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value,
                    //        index);
                    //}
                    //for (int index = 27; index < 45; index++)
                    //{
                    //    MeasureHorizental(this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value,
                    //        index);
                    //}

                    MeasureHorizental(this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value,
                        this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MeasureCenterYCommand;
        public ICommand MeasureCenterYCommand
        {
            get
            {
                if (null == _MeasureCenterYCommand) _MeasureCenterYCommand = new AsyncCommand(FuncMeasureCenterY);
                return _MeasureCenterYCommand;
            }
        }
        private async Task FuncMeasureCenterY()
        {
            try
            {
                await Task.Run(() =>
                {
                    //this.StageSupervisor().WaferObject.LoadDevParameter();
                    MeasureVertical(this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value,
                         this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MeasureCurRowCommand;
        public ICommand MeasureCurRowCommand
        {
            get
            {
                if (null == _MeasureCurRowCommand) _MeasureCurRowCommand = new AsyncCommand(FuncMeasureCurRow);
                return _MeasureCurRowCommand;
            }
        }


        private async Task FuncMeasureCurRow()
        {
            try
            {

                await Task.Run(() =>
                {
                    //this.StageSupervisor().WaferObject.LoadDevParameter();
                    MeasureHorizental(Convert.ToInt64(MXYIndex.X), Convert.ToInt64(MXYIndex.Y));
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _StopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (null == _StopCommand) _StopCommand = new RelayCommand(FuncStop);
                return _StopCommand;
            }
        }

        private void FuncStop()
        {
            try
            {
                cancelToken.Cancel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MeasureHorizental(long xindex, long yindex)
        {
            try
            {
                if (ptinfo == null)
                {
                    LoggerManager.Debug("Not Exist Pattern");
                    return;
                }

                if (cancelToken == null)
                {
                    cancelToken = new System.Threading.CancellationTokenSource();
                }

                IWaferObject waferobject = this.StageSupervisor().WaferObject;

                byte[,] wafermap = waferobject.DevicesConvertByteArray();
                long leftIndex = -1;
                long rightIndex = -1;

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                List<IDeviceObject> devices = waferobject.GetDevices().FindAll(device => device.DieIndexM.YIndex == yindex);
                leftIndex = devices.Find(device => device.DieType.Value != DieTypeEnum.NOT_EXIST).DieIndexM.XIndex;
                rightIndex = devices.FindLast(device => device.DieType.Value != DieTypeEnum.NOT_EXIST).DieIndexM.XIndex;

                double od = 0;
                double zc = 0;
                WaferCoordinate waferCoordinate = null;

                PinCoordinate pinCoordinate = new PinCoordinate();
                zc = ProbingModule.ZClearence;
                zc = ProbingModule.CalculateZClearenceUsingOD(od, zc);

                pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                List<MeasureMappingTable> measureMappingTables = new List<MeasureMappingTable>();
                //int focusingcount = 4;
                for (long index = leftIndex; index < rightIndex; index++)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        cancelToken.Dispose();
                        cancelToken = null;
                        return;
                    }
                    //if(index == MXYIndex.X & yindex == MXYIndex.Y)
                    //{
                    waferCoordinate = this.WaferAligner().MachineIndexConvertToProbingCoord((int)index, (int)yindex);
                    LoggerManager.Debug($"MappingIndex: [{index}],[{yindex}]");
                    StageSupervisor.StageModuleState.MovePadToPin(waferCoordinate, pinCoordinate, zc);
                    StageSupervisor.StageModuleState.ProbingZUP(waferCoordinate, pinCoordinate, 0);

                    //if(++focusingcount == 5)
                    //{
                    
                    EventCodeEnum ret = FocusModel.Focusing_Retry(FocusParam, false, false, false, this);

                    //    focusingcount = 0;
                    //}
                    if (ret != EventCodeEnum.NONE)
                    {
                        //if(this.StageSupervisor().WaferObject.GetDevices().Find
                        //    (device => device.DieIndexM.XIndex == index & device.DieIndexM.YIndex == yindex).
                        //    DieType.Value == DieTypeEnum.TEST_DIE)
                        //{

                        //}
                    }

                    PMResult pmret = this.VisionManager().PatternMatching(ptinfo, this);

                    if (pmret.RetValue == EventCodeEnum.NONE)
                    {
                        AddMeasureMappingTable(measureMappingTables, pmret, index);
                    }
                    else
                    {
                        if (this.StageSupervisor().WaferObject.GetDevices().Find(
                            device => device.DieIndexM.XIndex == index & device.DieIndexM.YIndex == yindex)
                            .DieType.Value == DieTypeEnum.MARK_DIE)
                        {

                        }
                        else
                        {
                            LoggerManager.Debug($"MappingIndex Fail: [{index}],[{yindex}] ");
                        }
                        //}
                    }
                }
                var table = errormanager.ErrorTable2D;

                double encoderypos = 0.0;
                this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref encoderypos);

                List<ErrorParameter2D> LINEARTable2D = table.TBL_OX_LINEAR.ToList<ErrorParameter2D>();
                List<ErrorParameter2D> STRAIGHTTable2D = table.TBL_OX_STRAIGHT.ToList<ErrorParameter2D>();
                ErrorParameter2D linear_seltable = LINEARTable2D.OrderBy(ltable => Math.Abs(encoderypos - ltable.PositionY)).First();
                ErrorParameter2D straght_seltable = STRAIGHTTable2D.OrderBy(ltable => Math.Abs(encoderypos - ltable.PositionY)).First();

                double minxpos = 0.0;
                double maxxpos = 0.0;
                double minoffset = 0.0;
                double maxoffset = 0.0;
                double minoffsety = 0.0;
                double maxoffsety = 0.0;

                foreach (var measuredata in measureMappingTables)
                {
                    ErrorParameter2D_X linear_retparam =
                        linear_seltable.ListY.OrderBy(list => Math.Abs(measuredata.EncoderXPos - list.PositionX)).First();
                    linear_retparam.ErrorValue -= measuredata.Xoffset;
                    //linear_retparam.ErrorValue += measuredata.Xoffset;


                    ErrorParameter2D_X straght_retparam =
                        straght_seltable.ListY.OrderBy(list => Math.Abs(measuredata.EncoderXPos - list.PositionX)).First();
                    straght_retparam.ErrorValue += measuredata.Yoffset;
                    //straght_retparam.ErrorValue -= measuredata.Yoffset;


                    if (measuredata.Index == measureMappingTables.Min(value => value.Index))
                    {
                        maxxpos = linear_retparam.PositionX;
                        maxoffset = measuredata.Xoffset;
                        maxoffsety = measuredata.Yoffset;
                    }
                    if (measuredata.Index == measureMappingTables.Max(value => value.Index))
                    {
                        minxpos = linear_retparam.PositionX;
                        minoffset = measuredata.Xoffset;
                        minoffsety = measuredata.Yoffset;
                    }
                }


                //좌측 외과 데이터 보상
                for (int index = 0; index < linear_seltable.ListY.Count; index++)
                {
                    if (linear_seltable.ListY[index].PositionX >= minxpos)
                        break;
                    else
                    {
                        linear_seltable.ListY[index].ErrorValue -= minoffset;
                        straght_seltable.ListY[index].ErrorValue += minoffsety;
                    }
                }

                //우측 외곽 데이터 보상
                for (int index = linear_seltable.ListY.Count - 1; index > 0; index--)
                {
                    if (linear_seltable.ListY[index].PositionX <= maxxpos)
                        break;
                    else
                    {
                        linear_seltable.ListY[index].ErrorValue -= maxoffset;
                        straght_seltable.ListY[index].ErrorValue += maxoffsety;
                    }
                }

                ObservableCollection<ErrorParameter2D> TBL_OX_LINEAR = new ObservableCollection<ErrorParameter2D>();
                
                foreach (var data in LINEARTable2D)
                {
                    TBL_OX_LINEAR.Add(data);
                }
                table.TBL_OX_LINEAR = TBL_OX_LINEAR;

                ObservableCollection<ErrorParameter2D> TBL_OX_STRAIGHT = new ObservableCollection<ErrorParameter2D>();

                foreach (var data in STRAIGHTTable2D)
                {
                    TBL_OX_STRAIGHT.Add(data);
                }
                table.TBL_OX_STRAIGHT = TBL_OX_STRAIGHT;

                errormanager.SaveErrorTable();
                errormanager.LoadErrorTable();
                
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MeasureVertical(long xindex, long yindex)
        {
            try
            {
                if (ptinfo == null)
                {
                    LoggerManager.Debug("Not Exist Pattern");
                    return;
                }

                if (cancelToken == null)
                    cancelToken = new System.Threading.CancellationTokenSource();

                IWaferObject waferobject = this.StageSupervisor().WaferObject;
                byte[,] wafermap = waferobject.DevicesConvertByteArray();
                long upperIndex = -1;
                long lowerIndex = -1;


                List<IDeviceObject> devices = waferobject.GetDevices().FindAll(device => device.DieIndexM.XIndex == xindex);
                //lowerIndex = devices.Find(device => device.DieType.Value == DieTypeEnum.TEST_DIE).DieIndexM.YIndex;
                //upperIndex = devices.FindLast(device => device.DieType.Value == DieTypeEnum.TEST_DIE).DieIndexM.YIndex;
                lowerIndex = devices.Find(device => device.DieType.Value != DieTypeEnum.NOT_EXIST).DieIndexM.YIndex;
                upperIndex = devices.FindLast(device => device.DieType.Value != DieTypeEnum.NOT_EXIST).DieIndexM.YIndex;



                List<MeasureMappingTable> measureMappingTables = new List<MeasureMappingTable>();
                var table = errormanager.ErrorTable2D;

                double encoderypos = 0.0;
                double encoderxpos = 0.0;
                List<ErrorParameter2D> LINEARTable2D = table.TBL_OX_LINEAR.ToList<ErrorParameter2D>();
                List<ErrorParameter2D> STRAIGHTTable2D = table.TBL_OX_STRAIGHT.ToList<ErrorParameter2D>();

                double zc = 0;
                double od = 0;
                WaferCoordinate waferCoordinate = null;
                PinCoordinate pinCoordinate = new PinCoordinate();


                od = ProbingModule.OverDrive;
                zc = ProbingModule.ZClearence;
                zc = ProbingModule.CalculateZClearenceUsingOD(od, zc);

                pinCoordinate.X.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                pinCoordinate.Y.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                pinCoordinate.Z.Value = StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;



                for (long index = lowerIndex; index < upperIndex; index++)
                {
                    double xoffset = 0.0;
                    double yoffset = 0.0;
                    if (cancelToken.IsCancellationRequested)
                    {
                        cancelToken.Dispose();
                        cancelToken = null;
                        return;
                    }

                    waferCoordinate = this.WaferAligner().MachineIndexConvertToProbingCoord((int)xindex, (int)index);

                    StageSupervisor.StageModuleState.MovePadToPin(waferCoordinate, pinCoordinate, zc);
                    StageSupervisor.StageModuleState.ProbingZUP(waferCoordinate, pinCoordinate, 0);

                    FocusModel.Focusing_Retry(FocusParam, false, false, false, this);
                    
                    PMResult pmret = this.VisionManager().PatternMatching(ptinfo, this);

                    if (pmret.RetValue == EventCodeEnum.NONE)
                    {
                        //AddMeasureMappingTable(measureMappingTables, pmret, index);
                        GetXYoffset(pmret, ref xoffset, ref yoffset);

                        this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref encoderypos);
                        this.MotionManager().GetRefPos(EnumAxisConstants.X, ref encoderxpos);
                        ErrorParameter2D linear_seltable = LINEARTable2D.OrderBy(ltable => Math.Abs(encoderypos - ltable.PositionY)).First();
                        ErrorParameter2D straght_seltable = STRAIGHTTable2D.OrderBy(ltable => Math.Abs(encoderypos - ltable.PositionY)).First();

                        foreach (var listy in linear_seltable.ListY)
                        {
                            //listy.ErrorValue -= xoffset;
                            listy.ErrorValue += xoffset;
                        }

                        foreach (var listy in straght_seltable.ListY)
                        {
                            //listy.ErrorValue += yoffset;
                            listy.ErrorValue -= yoffset;

                        }
                        //ErrorMappingTable에 변경 데이터 반영
                        ObservableCollection<ErrorParameter2D> TBL_OX_LINEAR = new ObservableCollection<ErrorParameter2D>();
                        foreach (var data in LINEARTable2D)
                        {
                            TBL_OX_LINEAR.Add(data);
                        }
                        table.TBL_OX_LINEAR = TBL_OX_LINEAR;

                        ObservableCollection<ErrorParameter2D> TBL_OX_STRAIGHT = new ObservableCollection<ErrorParameter2D>();
                        foreach (var data in STRAIGHTTable2D)
                        {
                            TBL_OX_STRAIGHT.Add(data);
                        }
                        table.TBL_OX_STRAIGHT = TBL_OX_STRAIGHT;

                        errormanager.SaveErrorTable();

                    }
                    else
                    {
                        this.StageSupervisor().WaferObject.GetDevices().Find(
                            item => item.DieIndexM.XIndex == index && item.DieIndexM.YIndex == yindex).DieType.Value = DieTypeEnum.MODIFY_DIE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void AddMeasureMappingTable(List<MeasureMappingTable> tables, PMResult mResult, long index)
        {
            try
            {
                double encoderxpos = 0.0;
                double encoderypos = 0.0;
                double xoffset = 0.0;
                double yoffset = 0.0;

                this.MotionManager().GetRefPos(EnumAxisConstants.X, ref encoderxpos);
                this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref encoderypos);

                GetXYoffset(mResult, ref xoffset, ref yoffset);

                if (tables != null)
                    tables.Add(new MeasureMappingTable(encoderxpos, encoderypos, xoffset, yoffset, index));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetXYoffset(PMResult mResult, ref double xoffset, ref double yoffset)
        {
            try
            {
                if (this.VisionManager().GetCam(mResult.ResultBuffer.CamType).GetHorizontalFlip() == FlipEnum.NONE)
                    xoffset = mResult.ResultParam[0].XPoss - (mResult.ResultBuffer.SizeX / 2);
                else
                    xoffset = (mResult.ResultBuffer.SizeX / 2) - mResult.ResultParam[0].XPoss;

                if (this.VisionManager().GetCam(mResult.ResultBuffer.CamType).GetVerticalFlip() == FlipEnum.FLIP)
                    yoffset = (mResult.ResultBuffer.SizeY / 2) - mResult.ResultParam[0].YPoss;
                else
                    yoffset = mResult.ResultParam[0].YPoss - (mResult.ResultBuffer.SizeY / 2);

                xoffset *= mResult.ResultBuffer.RatioX.Value;
                yoffset *= mResult.ResultBuffer.RatioY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        internal class MeasureMappingTable
        {
            private double _EncoderXPos;

            public double EncoderXPos
            {
                get { return _EncoderXPos; }
                set { _EncoderXPos = value; }
            }

            private double _EncoderYPos;

            public double EncoderYPos
            {
                get { return _EncoderYPos; ; }
                set { _EncoderYPos = value; }
            }

            private double _Xoffset;

            public double Xoffset
            {
                get { return _Xoffset; }
                set { _Xoffset = value; }
            }

            private double _Yoffset;

            public double Yoffset
            {
                get { return _Yoffset; }
                set { _Yoffset = value; }
            }
            private long _Index;

            public long Index
            {
                get { return _Index; }
                set { _Index = value; }
            }

            public MeasureMappingTable()
            {

            }
            public MeasureMappingTable(double encoderx, double encodery, double xoffset, double yoffset, long index)
            {
                try
                {
                    EncoderXPos = encoderx;
                    EncoderYPos = encodery;
                    Xoffset = xoffset;
                    Yoffset = yoffset;
                    Index = index;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }
        #endregion
    }
}
