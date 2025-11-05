using HexagonJogControl;
using LogModule;
using MetroDialogInterfaces;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.State;
using ProberViewModel.Data;
using ProberViewModel.View;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using UcDisplayPort;
using VirtualKeyboardControl;
using VirtualStageConnector;


namespace ProberViewModel.ViewModel
{
    public class DigitalTwinViewModel : IMainScreenViewModel, INotifyPropertyChanged, ISetUpState, IUseLightJog
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties

        private int _Current_Image_No;
        public int Current_Image_No
        {
            get { return _Current_Image_No; }
            set
            {
                if (value != _Current_Image_No)
                {
                    _Current_Image_No = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurCam_ROI_X;
        public double CurCam_ROI_X
        {
            get { return _CurCam_ROI_X; }
            set
            {
                if (value != _CurCam_ROI_X)
                {
                    _CurCam_ROI_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurCam_ROI_Y;
        public double CurCam_ROI_Y
        {
            get { return _CurCam_ROI_Y; }
            set
            {
                if (value != _CurCam_ROI_Y)
                {
                    _CurCam_ROI_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SeletedTabControlIndex;
        public int SeletedTabControlIndex
        {
            get { return _SeletedTabControlIndex; }
            set
            {
                if (value != _SeletedTabControlIndex)
                {
                    _SeletedTabControlIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICoordinateManager _CoordManager;
        public ICoordinateManager CoordManager
        {
            get { return _CoordManager; }
            set
            {
                if (value != _CoordManager)
                {
                    _CoordManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IProbingModule _Probing;
        public IProbingModule Probing
        {
            get { return _Probing; }
            set
            {
                if (value != _Probing)
                {
                    _Probing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IVisionManager _VisionManager;
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set
            {
                if (value != _VisionManager)
                {
                    _VisionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMotionManager _MotionManager;
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set
            {
                if (value != _MotionManager)
                {
                    _MotionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double? _ZoomLevel;
        public double? ZoomLevel
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

        private bool? _ShowPad;
        public bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPin;
        public bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _EnableDragMap;
        public bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPos;
        public bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPosROI;
        public bool? ShowCurrentPosROI
        {
            get { return _ShowCurrentPosROI; }
            set
            {
                if (value != _ShowCurrentPosROI)
                {
                    _ShowCurrentPosROI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowSelectedDut;
        public bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowGrid;
        public bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _AddCheckBoxIsChecked;
        public bool? AddCheckBoxIsChecked
        {
            get { return _AddCheckBoxIsChecked; }
            set
            {
                if (value != _AddCheckBoxIsChecked)
                {
                    _AddCheckBoxIsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MainViewZoomVisibility;
        public Visibility MainViewZoomVisibility
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

        private Visibility _VisibilityZoomIn;
        public Visibility VisibilityZoomIn
        {
            get { return _VisibilityZoomIn; }
            set
            {
                if (value != _VisibilityZoomIn)
                {
                    _VisibilityZoomIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomOut;
        public Visibility VisibilityZoomOut
        {
            get { return _VisibilityZoomOut; }
            set
            {
                if (value != _VisibilityZoomOut)
                {
                    _VisibilityZoomOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityMoveToCenter;
        public Visibility VisibilityMoveToCenter
        {
            get { return _VisibilityMoveToCenter; }
            set
            {
                if (value != _VisibilityMoveToCenter)
                {
                    _VisibilityMoveToCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _MiniViewZoomVisibility;
        public Visibility MiniViewZoomVisibility
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

        private Visibility _MiniViewSwapVisibility;
        public Visibility MiniViewSwapVisibility
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
                        MainViewZoomVisibility = Visibility.Visible;
                        LightJogVisibility = Visibility.Hidden;
                    }
                    else if (_MainViewTarget is IDisplayPort)
                    {
                        MainViewZoomVisibility = Visibility.Hidden;
                        LightJogVisibility = Visibility.Visible;
                    }
                    else if (_MainViewTarget is IProbeCard)
                    {
                        MainViewZoomVisibility = Visibility.Hidden;
                        LightJogVisibility = Visibility.Hidden;
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
                    if (_MiniViewTarget is IDisplayPort)
                    {
                        MiniViewZoomVisibility = Visibility.Hidden;
                    }
                    else if (_MiniViewTarget is IWaferObject)
                    {
                        MiniViewZoomVisibility = Visibility.Visible;
                    }
                    else if (_MiniViewTarget is IProbeCard)
                    {
                        MiniViewZoomVisibility = Visibility.Hidden;
                    }
                    RaisePropertyChanged();
                }
                if (_MiniViewTarget == null)
                    MiniViewSwapVisibility = Visibility.Hidden;
                else
                    MiniViewSwapVisibility = Visibility.Visible;
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

        private Point _AmoutOfOffsetFromCenter;
        public Point AmoutOfOffsetFromCenter
        {
            get { return _AmoutOfOffsetFromCenter; }
            set
            {
                if (value != _AmoutOfOffsetFromCenter)
                {
                    _AmoutOfOffsetFromCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AmoutOfOverlap_X;
        public double AmoutOfOverlap_X
        {
            get { return _AmoutOfOverlap_X; }
            set
            {
                if (value != _AmoutOfOverlap_X)
                {
                    _AmoutOfOverlap_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AmoutOfOverlap_Y;
        public double AmoutOfOverlap_Y
        {
            get { return _AmoutOfOverlap_Y; }
            set
            {
                if (value != _AmoutOfOverlap_Y)
                {
                    _AmoutOfOverlap_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ExtensionArea_X;
        public double ExtensionArea_X
        {
            get { return _ExtensionArea_X; }
            set
            {
                if (value != _ExtensionArea_X)
                {
                    _ExtensionArea_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ExtensionArea_Y;
        public double ExtensionArea_Y
        {
            get { return _ExtensionArea_Y; }
            set
            {
                if (value != _ExtensionArea_Y)
                {
                    _ExtensionArea_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PinHeightManualMode;
        public bool PinHeightManualMode
        {
            get { return _PinHeightManualMode; }
            set
            {
                if (value != _PinHeightManualMode)
                {
                    _PinHeightManualMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LocationEnum> _WaferLocationList = new ObservableCollection<LocationEnum>();
        public ObservableCollection<LocationEnum> WaferLocationList
        {
            get { return _WaferLocationList; }
            set
            {
                if (value != _WaferLocationList)
                {
                    _WaferLocationList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private enumStageCamType _SelectedCam;
        public enumStageCamType SelectedCam
        {
            get { return _SelectedCam; }
            set
            {
                if (value != _SelectedCam)
                {
                    _SelectedCam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private UserIndex _TargetUserIndex;
        public UserIndex TargetUserIndex
        {
            get { return _TargetUserIndex; }
            set
            {
                if (value != _TargetUserIndex)
                {
                    _TargetUserIndex = value;
                    RaisePropertyChanged();

                }
            }
        }

        private UserIndex _SelectedTargetUserIndex;
        public UserIndex SelectedTargetUserIndex
        {
            get { return _SelectedTargetUserIndex; }
            set
            {
                if (value != _SelectedTargetUserIndex)
                {
                    _SelectedTargetUserIndex = value;
                    RaisePropertyChanged();

                }
            }
        }

        private ObservableCollection<UserIndex> _TargetUserIndexs = new ObservableCollection<UserIndex>();
        public ObservableCollection<UserIndex> TargetUserIndexs
        {
            get { return _TargetUserIndexs; }
            set
            {
                if (value != _TargetUserIndexs)
                {
                    _TargetUserIndexs = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _TargetDutNo;
        public int TargetDutNo
        {
            get { return _TargetDutNo; }
            set
            {
                if (value != _TargetDutNo)
                {
                    _TargetDutNo = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _SelectedTargetDutNo;
        public int SelectedTargetDutNo
        {
            get { return _SelectedTargetDutNo; }
            set
            {
                if (value != _SelectedTargetDutNo)
                {
                    _SelectedTargetDutNo = value;
                    RaisePropertyChanged();

                }
            }
        }

        private ObservableCollection<int> _TargetDutNos = new ObservableCollection<int>();
        public ObservableCollection<int> TargetDutNos
        {
            get { return _TargetDutNos; }
            set
            {
                if (value != _TargetDutNos)
                {
                    _TargetDutNos = value;
                    RaisePropertyChanged();

                }
            }
        }

        private ObservableCollection<StageCamera> _StageCamList = new ObservableCollection<StageCamera>();
        public ObservableCollection<StageCamera> StageCamList
        {
            get { return _StageCamList; }
            set
            {
                if (value != _StageCamList)
                {
                    _StageCamList = value;
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
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MotionJogEnabled = true;
        public bool MotionJogEnabled
        {
            get { return _MotionJogEnabled; }
            set
            {
                if (value != _MotionJogEnabled)
                {
                    _MotionJogEnabled = value;
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


        private Visibility _MotionJogVisibility;
        public Visibility MotionJogVisibility
        {
            get { return _MotionJogVisibility; }
            set
            {
                if (value != _MotionJogVisibility)
                {
                    _MotionJogVisibility = value;
                    RaisePropertyChanged();
                }
            }
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

        private HexagonJogViewModel _MotionJog;
        public HexagonJogViewModel MotionJog
        {
            get { return _MotionJog; }
            set
            {
                if (value != _MotionJog)
                {
                    _MotionJog = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }
        public IWaferObject WaferObject { get { return this.GetParam_Wafer(); } }

        private int _millisecondsTimeoutAfterChuckMove = 0;
        public int millisecondsTimeoutAfterChuckMove
        {
            get { return _millisecondsTimeoutAfterChuckMove; }
            set
            {
                if (value != _millisecondsTimeoutAfterChuckMove)
                {
                    _millisecondsTimeoutAfterChuckMove = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _dieSizeX;
        public double dieSizeX
        {
            get { return _dieSizeX; }
            set
            {
                if (value != _dieSizeX)
                {
                    _dieSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _dieSizeY;
        public double dieSizeY
        {
            get { return _dieSizeY; }
            set
            {
                if (value != _dieSizeY)
                {
                    _dieSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _XStep;
        public int XStep
        {
            get { return _XStep; }
            set
            {
                if (value != _XStep)
                {
                    _XStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _YStep;
        public int YStep
        {
            get { return _YStep; }
            set
            {
                if (value != _YStep)
                {
                    _YStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ZStep;
        public int ZStep
        {
            get { return _ZStep; }
            set
            {
                if (value != _ZStep)
                {
                    _ZStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string outputFilePath;
        public string OutputFilePath
        {
            get
            {
                return outputFilePath;
            }
            set
            {
                outputFilePath = value;
                RaisePropertyChanged();
            }
        }

        private bool _FocusingEnable;
        public bool FocusingEnable
        {
            get { return _FocusingEnable; }
            set
            {
                if (value != _FocusingEnable)
                {
                    _FocusingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FocusingRange;
        public int FocusingRange
        {
            get { return _FocusingRange; }
            set
            {
                if (value != _FocusingRange)
                {
                    _FocusingRange = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _FocusingInterval;
        public int FocusingInterval
        {
            get { return _FocusingInterval; }
            set
            {
                if (value != _FocusingInterval)
                {
                    _FocusingInterval = value;
                    RaisePropertyChanged();

                }
            }
        }

        private bool _LightEnable;
        public bool LightEnable
        {
            get { return _LightEnable; }
            set
            {
                if (value != _LightEnable)
                {
                    _LightEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _COAXIAL_LightStart;
        public int COAXIAL_LightStart
        {
            get { return _COAXIAL_LightStart; }
            set
            {
                if (value != _COAXIAL_LightStart)
                {
                    _COAXIAL_LightStart = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _COAXIAL_LightEnd;
        public int COAXIAL_LightEnd
        {
            get { return _COAXIAL_LightEnd; }
            set
            {
                if (value != _COAXIAL_LightEnd)
                {
                    _COAXIAL_LightEnd = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _COAXIAL_LightInterval;
        public int COAXIAL_LightInterval
        {
            get { return _COAXIAL_LightInterval; }
            set
            {
                if (value != _COAXIAL_LightInterval)
                {
                    _COAXIAL_LightInterval = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _AUX_LightStart;
        public int AUX_LightStart
        {
            get { return _AUX_LightStart; }
            set
            {
                if (value != _AUX_LightStart)
                {
                    _AUX_LightStart = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _AUX_LightEnd;
        public int AUX_LightEnd
        {
            get { return _AUX_LightEnd; }
            set
            {
                if (value != _AUX_LightEnd)
                {
                    _AUX_LightEnd = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _AUX_LightInterval;
        public int AUX_LightInterval
        {
            get { return _AUX_LightInterval; }
            set
            {
                if (value != _AUX_LightInterval)
                {
                    _AUX_LightInterval = value;
                    RaisePropertyChanged();

                }
            }
        }

        private AdvancedOption _selectedOption;

        public AdvancedOption SelectedOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                RaisePropertyChanged();
            }
        }

        private bool isSettingFileSaved = false;

        private async Task OpenOutputPathCommandFunc(Object param)
        {
            try
            {
                if (!string.IsNullOrEmpty(OutputFilePath))
                {
                    var outputPath = Path.GetDirectoryName(OutputFilePath);
                    if (Directory.Exists(outputPath))
                    {
                        Process.Start("explorer.exe", outputPath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public VirtualStageConnector.IVirtualStageConnector VirtualConnector
        {
            get => VirtualStageConnector.VirtualStageConnector.Instance;
        }
        #endregion

        #region IMainScreenViewModel

        public bool Initialized { get; set; } = false;

        readonly Guid _ViewGUID = new Guid("e934c029-6efe-46d3-9e67-a39771ce7e42");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public Task<EventCodeEnum> InitViewModel()
        {
            Task<EventCodeEnum> retval = null;

            try
            {
                DisplayPort = new DisplayPort() { GUID = new Guid("0dfe8d65-3ce5-4397-9b82-f44cb1248dbd") };

                Array stagecamvalues = Enum.GetValues(typeof(StageCamEnum));

                foreach (var cam in this.VisionManager().GetCameras())
                {
                    for (int index = 0; index < stagecamvalues.Length; index++)
                    {
                        if (((StageCamEnum)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                        {
                            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                            break;
                        }
                    }
                }

                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            Task<EventCodeEnum> retval = null;

            try
            {
                dieSizeX = WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                dieSizeY = WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                WaferCoordinate wcd = new WaferCoordinate(0, 0);
                var idx = this.WaferAligner().WPosToMIndex(wcd);

                // 초기 UserIndex
                TargetUserIndex = this.CoordinateManager().WMIndexConvertWUIndex(idx.XIndex, idx.YIndex);

                if (TargetUserIndexs == null)
                {
                    TargetUserIndexs = new ObservableCollection<UserIndex>();
                }
                else
                {
                    TargetUserIndexs.Clear();
                }

                CurCam = null;
                SelectedCam = enumStageCamType.UNDEFINED;

                SeletedTabControlIndex = 0;

                MainViewTarget = DisplayPort;
                MiniViewTarget = WaferObject;

                if (StageSupervisor == null)
                {
                    StageSupervisor = this.StageSupervisor();
                }

                if (CoordManager == null)
                {
                    CoordManager = this.CoordinateManager();
                }

                if (Probing == null)
                {
                    Probing = this.ProbingModule();
                }

                if (VisionManager == null)
                {
                    VisionManager = this.VisionManager();
                }

                if (MotionManager == null)
                {
                    MotionManager = this.MotionManager();
                }


                ZoomLevel = 5;
                ShowPad = false;
                ShowPin = true;
                EnableDragMap = true;
                ShowCurrentPos = true;
                ShowCurrentPosROI = true;
                ShowSelectedDut = false;
                ShowGrid = false;

                AmoutOfOverlap_X = 100;
                AmoutOfOverlap_Y = 100;

                TargetDutNo = 1;

                ExtensionArea_X = 50;
                ExtensionArea_Y = 50;

                millisecondsTimeoutAfterChuckMove = 10;

                retval = Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            Task<EventCodeEnum> retval = null;

            try
            {
                // TODO : 
                StopGrabCommandFunc(null);

                retval = Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            Task<EventCodeEnum> retval = null;

            try
            {

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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                foreach (enumStageCamType camtype in (enumStageCamType[])System.Enum.GetValues(typeof(enumStageCamType)))
                {
                    StageCamList.Add(new StageCamera(camtype));
                }

                foreach (LocationEnum location in (LocationEnum[])System.Enum.GetValues(typeof(LocationEnum)))
                {
                    WaferLocationList.Add(location);
                }

                LightJog = new LightJogViewModel(maxLightValue: 255, minLightValue: 0);
                MotionJog = new HexagonJogViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

        public double[,] GetDieUpperLeftPosWithOutClearance(UserIndex userIndex)
        {
            double[,] retval = new double[2, 2];

            try
            {
                var mi = this.CoordinateManager().UserIndexConvertToMachineIndex(userIndex);
                var wc = this.WaferAligner().MachineIndexConvertToDieLeftCorner(mi.XIndex, mi.YIndex);

                //좌측 상단
                retval[0, 0] = wc.X.Value + (WaferObject.GetSubsInfo().DieXClearance.Value / 2.0);
                retval[0, 1] = wc.Y.Value + WaferObject.GetSubsInfo().ActualDieSize.Height.Value - (WaferObject.GetSubsInfo().DieYClearance.Value / 2.0);

                //우측 하단
                retval[1, 0] = wc.X.Value + WaferObject.GetSubsInfo().ActualDieSize.Width.Value - (WaferObject.GetSubsInfo().DieXClearance.Value / 2.0);
                retval[1, 1] = wc.Y.Value + (WaferObject.GetSubsInfo().DieYClearance.Value / 2.0);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double[,] GetDutUpperLeftPosWithOutClearance(int dutNo)
        {
            double[,] retval = new double[2, 2];

            try
            {
                PinCoordinate pc = TargetDutLeftCorner(dutNo);

                //좌측 상단
                retval[0, 0] = pc.X.Value + (WaferObject.GetSubsInfo().DieXClearance.Value / 2.0);
                retval[0, 1] = pc.Y.Value + WaferObject.GetSubsInfo().ActualDieSize.Height.Value - (WaferObject.GetSubsInfo().DieYClearance.Value / 2.0);

                //우측 하단
                retval[1, 0] = pc.X.Value + WaferObject.GetSubsInfo().ActualDieSize.Width.Value - (WaferObject.GetSubsInfo().DieXClearance.Value / 2.0);
                retval[1, 1] = pc.Y.Value + (WaferObject.GetSubsInfo().DieYClearance.Value / 2.0);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MoveToWaferPosition(double x, double y)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                EnumProberCam camType = CurCam.GetChannelType();

                if (camType == EnumProberCam.WAFER_HIGH_CAM)
                {
                    retVal = this.StageSupervisor().StageModuleState.WaferHighViewMove(x, y, WaferObject.GetSubsInfo().ActualThickness, this.WaferAligner().WaferAlignInfo.AlignAngle);
                }
                else if (camType == EnumProberCam.WAFER_LOW_CAM)
                {
                    retVal = this.StageSupervisor().StageModuleState.WaferLowViewMove(x, y, WaferObject.GetSubsInfo().ActualThickness, this.WaferAligner().WaferAlignInfo.AlignAngle);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $"Cam type is {camType}", EnumMessageStyle.Affirmative);

                    LoggerManager.Debug($"[{this.GetType().Name}], MoveToWaferPosition() : Can not move, Curcam = {camType}");
                }

                Thread.Sleep(millisecondsTimeoutAfterChuckMove);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MoveToPinPosition(double x, double y, double z)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                EnumProberCam camType = CurCam.GetChannelType();

                if (camType == EnumProberCam.PIN_HIGH_CAM)
                {
                    if (PinHeightManualMode)
                    {
                        retVal = this.StageSupervisor().StageModuleState.PinHighViewMove(x, y);
                    }
                    else
                    {
                        retVal = this.StageSupervisor().StageModuleState.PinHighViewMove(x, y, z);
                    }
                }
                else if (camType == EnumProberCam.PIN_LOW_CAM)
                {
                    if (PinHeightManualMode)
                    {
                        retVal = this.StageSupervisor().StageModuleState.PinLowViewMove(x, y);
                    }
                    else
                    {
                        retVal = this.StageSupervisor().StageModuleState.PinLowViewMove(x, y, z);
                    }
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $"Cam type is {camType}", EnumMessageStyle.Affirmative);

                    LoggerManager.Debug($"[{this.GetType().Name}], MoveToPinPosition() : Can not move, Curcam = {camType}");
                }

                Thread.Sleep(millisecondsTimeoutAfterChuckMove);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            try
            {
                double newx = 0.0;
                double newy = 0.0;
                double th = DegreeToRadian(angle);

                newx = OriPos.X.Value - RefPos.X.Value;
                newy = OriPos.Y.Value - RefPos.Y.Value;

                NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
                NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PinCoordinate TargetDutLeftCorner(int dutNo)
        {
            PinCoordinate retval = null;

            try
            {
                PinCoordinate CenPos = new PinCoordinate();

                double DutAngle = 0;

                double firstDut_LLX = 0;
                double firstDut_LLY = 0;

                double PosLL_X = 0;
                double PosLL_Y = 0;

                double[,] DutCornerPos = new double[4, 2];

                CenPos.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                CenPos.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                DutAngle = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutAngle;

                firstDut_LLX = CenPos.X.Value - ((double)(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX / 2.0) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value);
                firstDut_LLX += this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;

                firstDut_LLY = CenPos.Y.Value - ((double)(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY / 2.0) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value);
                firstDut_LLY += this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                IDut TargetDut = null;

                TargetDut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault(x => x.DutNumber == dutNo);

                if (TargetDut != null)
                {
                    retval = new PinCoordinate();

                    // 이 더트의 좌하단 위치
                    PosLL_X = firstDut_LLX + (TargetDut.MacIndex.XIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    PosLL_Y = firstDut_LLY + (TargetDut.MacIndex.YIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                    // 더트 각도 적용
                    GetRotCoordEx(ref retval, new PinCoordinate(PosLL_X, PosLL_Y), CenPos, DutAngle);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void WriteToFile(string path, string time, UserIndex userIndex = null, int dutNo = -1)
        {
            try
            {
                EnumProberCam camType = CurCam.GetChannelType();

                string xindex = string.Empty;
                string yindex = string.Empty;

                string dutn = string.Empty;

                if (userIndex != null)
                {
                    xindex = userIndex.XIndex.ToString();
                    yindex = userIndex.YIndex.ToString();
                }

                if (dutNo != -1)
                {
                    dutn = dutNo.ToString();
                }

                // Create the content for the text file
                string content = $"Cam Type : {camType}\n" +
                                 $"Die Size X : {dieSizeX} um\n" +
                                 $"Die Size Y : {dieSizeY} um\n" +
                                 $"X Index : {xindex}\n" +
                                 $"Y Index : {yindex}\n" +
                                 $"Target Dut No: {dutn}\n" +
                                 $"Amount of Overlap X : {AmoutOfOverlap_X} um, {AmoutOfOverlap_X / CurCam.GetRatioX()} px, Ratio : {CurCam.GetRatioX()}\n" +
                                 $"Amount of Overlap Y : {AmoutOfOverlap_Y} um, {AmoutOfOverlap_Y / CurCam.GetRatioY()} px, Ratio : {CurCam.GetRatioY()}\n" +
                                 $"Extension Area X : {ExtensionArea_X} um\n" +
                                 $"Extension Area Y : {ExtensionArea_Y} um\n" +
                                 $"Milliseconds Timeout After Chuck Move: {millisecondsTimeoutAfterChuckMove}";

                string fullpath = $"{path}{time}_Setting.txt";

                File.WriteAllText(fullpath, content);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Commands

        #region Die

        private RelayCommand<Object> _AddScanDieCommand;
        public ICommand AddScanDieCommand
        {
            get
            {
                if (null == _AddScanDieCommand) _AddScanDieCommand = new RelayCommand<Object>(AddScanDieCommandFunc);
                return _AddScanDieCommand;
            }
        }

        private void AddScanDieCommandFunc(Object param)
        {
            try
            {
                if (TargetUserIndexs == null)
                {
                    TargetUserIndexs = new ObservableCollection<UserIndex>();
                }

                UserIndex tmp = new UserIndex(TargetUserIndex.XIndex, TargetUserIndex.YIndex);

                bool exists = TargetUserIndexs.Any(item => item.XIndex == tmp.XIndex && item.YIndex == tmp.YIndex);

                if (!exists)
                {
                    TargetUserIndexs.Add(tmp);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "Data already exists", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DeleteScanDieCommand;
        public ICommand DeleteScanDieCommand
        {
            get
            {
                if (null == _DeleteScanDieCommand) _DeleteScanDieCommand = new RelayCommand<Object>(DeleteScanDieCommandFunc);
                return _DeleteScanDieCommand;
            }
        }


        private void DeleteScanDieCommandFunc(Object param)
        {
            try
            {
                if (SelectedTargetUserIndex != null)
                {
                    TargetUserIndexs.Remove(SelectedTargetUserIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ClearScanDieCommand;
        public ICommand ClearScanDieCommand
        {
            get
            {
                if (null == _ClearScanDieCommand) _ClearScanDieCommand = new RelayCommand<Object>(ClearScanDieCommandFunc);
                return _ClearScanDieCommand;
            }
        }


        private void ClearScanDieCommandFunc(Object param)
        {
            try
            {
                if (TargetUserIndexs != null)
                {
                    TargetUserIndexs.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region Dut

        private RelayCommand<Object> _AddScanDutCommand;
        public ICommand AddScanDutCommand
        {
            get
            {
                if (null == _AddScanDutCommand) _AddScanDutCommand = new RelayCommand<Object>(AddScanDutCommandFunc);
                return _AddScanDutCommand;
            }
        }


        private void AddScanDutCommandFunc(Object param)
        {
            try
            {
                if (TargetDutNos == null)
                {
                    TargetDutNos = new ObservableCollection<int>();
                }

                int tmp = TargetDutNo;

                bool exists = TargetDutNos.Any(item => item == tmp);

                if (!exists)
                {
                    TargetDutNos.Add(tmp);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "Data already exists", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DeleteScanDutCommand;
        public ICommand DeleteScanDutCommand
        {
            get
            {
                if (null == _DeleteScanDutCommand) _DeleteScanDutCommand = new RelayCommand<Object>(DeleteScanDutCommandFunc);
                return _DeleteScanDutCommand;
            }
        }


        private void DeleteScanDutCommandFunc(Object param)
        {
            try
            {
                if (SelectedTargetDutNo != null)
                {
                    TargetDutNos.Remove(SelectedTargetDutNo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ClearScanDutCommand;
        public ICommand ClearScanDutCommand
        {
            get
            {
                if (null == _ClearScanDutCommand) _ClearScanDutCommand = new RelayCommand<Object>(ClearScanDutCommandFunc);
                return _ClearScanDutCommand;
            }
        }


        private void ClearScanDutCommandFunc(Object param)
        {
            try
            {
                if (TargetDutNos != null)
                {
                    TargetDutNos.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _AddScanDutAllCommand;
        public ICommand AddScanDutAllCommand
        {
            get
            {
                if (null == _AddScanDutAllCommand) _AddScanDutAllCommand = new RelayCommand<Object>(AddScanDutAllCommandFunc);
                return _AddScanDutAllCommand;
            }
        }

        private void AddScanDutAllCommandFunc(Object param)
        {
            try
            {
                if (TargetDutNos != null)
                {
                    TargetDutNos.Clear();
                }

                List<TempDut> tempDuts = new List<TempDut>();

                foreach (var item in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    var dutno = item.DutNumber;
                    var pc = TargetDutLeftCorner(dutno);

                    tempDuts.Add(new TempDut(dutno, pc));
                }

                // TODO : 순서 변경 (?)
                foreach (var item in tempDuts)
                {
                    TargetDutNos.Add(item.DutNumber);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private AsyncCommand<Object> _OpenOutputPathCommand;
        public ICommand OpenOutputPathCommand
        {
            get
            {
                if (null == _OpenOutputPathCommand) _OpenOutputPathCommand = new AsyncCommand<Object>(OpenOutputPathCommandFunc);
                return _OpenOutputPathCommand;
            }
        }

        private RelayCommand<object> _VirtualMoveCommand;
        public ICommand VirtualMoveCommand
        {
            get
            {
                if (null == _VirtualMoveCommand) _VirtualMoveCommand = new RelayCommand<object>(VirtualMoveCommandFunc);
                return _VirtualMoveCommand;
            }
        }

        private void VirtualMoveCommandFunc(object obj)
        {
            try
            {
                this.MotionManager().StageRelMove(XStep, YStep, ZStep);

                //VirtualConnector.MoveRelative(XStep, YStep, ZStep, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _VirtualProbingCommand;
        public ICommand VirtualProbingCommand
        {
            get
            {
                if (null == _VirtualProbingCommand) _VirtualProbingCommand = new RelayCommand<object>(VirtualProbingCommandFunc);
                return _VirtualProbingCommand;
            }
        }

        private void VirtualProbingCommandFunc(object obj)
        {
            try
            {
                string input = (string)obj;

                if (input == "True")
                {
                    VirtualStageConnector.VirtualStageConnector.Instance.SendTCPCommand(TCPCommand.VIRTUAL_PROBING_ON);
                }
                else
                {
                    VirtualStageConnector.VirtualStageConnector.Instance.SendTCPCommand(TCPCommand.VIRTUAL_PROBING_OFF);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ServerStartCommand;
        public ICommand ServerStartCommand
        {
            get
            {
                if (null == _ServerStartCommand) _ServerStartCommand = new RelayCommand<object>(ServerStartCommandFunc);
                return _ServerStartCommand;
            }
        }

        public void ServerStartCommandFunc(object parameter)
        {
            try
            {
                VirtualConnector.Start();

                // TODO : 최초 연결 시, 현재 Camera Set의 필요?
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ServerDisconnectCommand;
        public ICommand ServerDisconnectCommand
        {
            get
            {
                if (null == _ServerDisconnectCommand) _ServerDisconnectCommand = new RelayCommand<object>(ServerDisconnectCommandFunc);
                return _ServerDisconnectCommand;
            }
        }

        public void ServerDisconnectCommandFunc(object parameter)
        {
            try
            {
                VirtualConnector.Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _GRAB_SNAP_Command;
        public ICommand GRAB_SNAP_Command
        {
            get
            {
                if (null == _GRAB_SNAP_Command) _GRAB_SNAP_Command = new RelayCommand<object>(GRAB_SNAP_CommandFunc);
                return _GRAB_SNAP_Command;
            }
        }

        public void GRAB_SNAP_CommandFunc(object parameter)
        {
            try
            {
                //VirtualImageCollector.Instance.SendCmdMsg(TCPCommand.GRAB_SNAP);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        public IAsyncCommand DutAddMouseDownCommand => null;

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
            object swap = MainViewTarget;
            //MainViewTarget = WaferObject;
            MainViewTarget = MiniViewTarget;
            MiniViewTarget = swap;
        }

        private AsyncCommand<object> _CardDutScanCommand;
        public ICommand CardDutScanCommand
        {
            get
            {
                if (null == _CardDutScanCommand) _CardDutScanCommand = new AsyncCommand<object>(FuncCardDutScanCommand);
                return _CardDutScanCommand;
            }
        }

        //private async Task FuncCardDutScanCommand(object obj)
        //{
        //    try
        //    {
        //        if (TargetDutNos.Count == 0)
        //        {
        //            await this.MetroDialogManager().ShowMessageDialog("Error", "No data has been selected.", EnumMessageStyle.Affirmative);

        //            return;
        //        }

        //        foreach (var dutNo in TargetDutNos)
        //        {
        //            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //            var dutpos = GetDutUpperLeftPosWithOutClearance(dutNo);

        //            // Extension

        //            dutpos[0, 0] = dutpos[0, 0] - ExtensionArea_X;
        //            dutpos[0, 1] = dutpos[0, 1] + ExtensionArea_Y;

        //            dutpos[1, 0] = dutpos[1, 0] + ExtensionArea_X;
        //            dutpos[1, 1] = dutpos[1, 1] - ExtensionArea_Y;

        //            Point firstMovePos = new Point();

        //            // ROI
        //            double roi_x_um = CurCam.GetRatioY() * CurCam.GetGrabSizeWidth();
        //            double roi_y_um = CurCam.GetRatioY() * CurCam.GetGrabSizeHeight();

        //            LoggerManager.Debug($"[{this.GetType().Name}], FuncCardDutScanCommand() : Upper Left ({dutpos[0, 0]}, {dutpos[0, 1]}), Lower Right ({dutpos[1, 0]}, {dutpos[1, 1]}), Dut Size = ({WaferObject.GetSubsInfo().ActualDieSize.Width.Value}, {WaferObject.GetSubsInfo().ActualDieSize.Height.Value}), Cam ROI = ({roi_x_um},{roi_y_um}) Overlap = ({AmoutOfOverlap_X}, {AmoutOfOverlap_Y})");

        //            // Half of ROI
        //            double half_roi_x_um = roi_x_um / 2.0;
        //            double half_roi_y_um = roi_y_um / 2.0;

        //            double offset_x = 0;
        //            double offset_y = 0;

        //            int sign_x = 1;
        //            int sign_y = -1;

        //            // Hard Coding (Unit : um)
        //            AmoutOfOffsetFromCenter = new Point(10, 10);

        //            firstMovePos.X = dutpos[0, 0] + half_roi_x_um - AmoutOfOffsetFromCenter.X;
        //            firstMovePos.Y = dutpos[0, 1] - half_roi_x_um + AmoutOfOffsetFromCenter.Y;

        //            bool islimit_x = false;
        //            bool islimit_y = false;

        //            offset_x = roi_x_um - AmoutOfOverlap_X;
        //            offset_y = roi_y_um - AmoutOfOverlap_Y;

        //            double TargetPosX = firstMovePos.X;
        //            double TargetPosY = firstMovePos.Y;

        //            EnumProberCam curcamtype = CurCam.GetChannelType();

        //            // Get the current date
        //            DateTime currentDate = DateTime.Now;

        //            // Format the date as YYYYMMDD
        //            string folderName = currentDate.ToString("yyyyMMdd");

        //            var savefolderpath = $"C:\\Logs\\DigitalTwin\\{curcamtype}\\{folderName}\\";
        //            var savefullpath = string.Empty;

        //            if (!Directory.Exists(savefolderpath))
        //            {
        //                Directory.CreateDirectory(savefolderpath);
        //            }

        //            OutputFilePath = savefolderpath;

        //            Current_Image_No = 0;

        //            int xidx = 0;
        //            int yidx = 0;

        //            bool isSettingFileSaved = false;

        //            do
        //            {
        //                retVal = MoveToPinPosition(TargetPosX, TargetPosY);

        //                // Save image
        //                Current_Image_No++;
        //                var current_img_buffer = this.VisionManager().SingleGrab(curcamtype, this);
        //                string timePart = current_img_buffer.CapturedTime.ToString("HHmmss");

        //                savefullpath = $"{savefolderpath}{timePart}_No#{Current_Image_No:D4}_[{xidx},{yidx}]_PosX#{TargetPosX:F2}_PosY#{TargetPosY:F2}.bmp";

        //                this.VisionManager().SaveImageBuffer(current_img_buffer, savefullpath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
        //                LoggerManager.Debug($"[{this.GetType().Name}], FuncCardDutScanCommand() : Save Image, Path = {savefullpath}");

        //                if (!isSettingFileSaved)
        //                {
        //                    WriteToFile(savefolderpath, timePart, dutNo: dutNo);
        //                    isSettingFileSaved = true;
        //                }

        //                double Current_roi_x = TargetPosX + (half_roi_x_um * sign_x);
        //                double Current_roi_y = TargetPosY + (half_roi_y_um * sign_y);

        //                if (Current_roi_y < dutpos[1, 1])
        //                {
        //                    if (Current_roi_x > dutpos[1, 0] || Current_roi_x < dutpos[0, 0])
        //                    {
        //                        break;
        //                    }
        //                }

        //                if (sign_x == 1)
        //                {
        //                    if (Current_roi_x > dutpos[1, 0])
        //                    {
        //                        sign_x *= -1;
        //                        TargetPosY += (offset_y * sign_y);
        //                        yidx++;
        //                    }
        //                    else
        //                    {
        //                        TargetPosX += (offset_x * sign_x);
        //                        xidx++;
        //                    }
        //                }
        //                else
        //                {
        //                    if (Current_roi_x < dutpos[0, 0])
        //                    {
        //                        sign_x *= -1;
        //                        TargetPosY += (offset_y * sign_y);
        //                        yidx++;
        //                    }
        //                    else
        //                    {
        //                        TargetPosX += (offset_x * sign_x);
        //                        xidx--;
        //                    }
        //                }

        //            } while (true);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private async Task FuncCardDutScanCommand(object obj)
        {
            try
            {
                if (SelectedOption == AdvancedOption.NotUse)
                {
                    if (TargetDutNos.Count == 0)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Error", "No data has been selected.", EnumMessageStyle.Affirmative);
                        return;
                    }
                }

                if (CurCam == null)
                {
                    // TODO : 카메라 정보가 부족합니다. 라는 메시지를 띄우고 리턴
                    string msg = "Camera is not selected.";
                    await this.MetroDialogManager().ShowMessageDialog("Error", msg, EnumMessageStyle.Affirmative);
                    return;
                }

                EnumProberCam curcamtype = CurCam.GetChannelType();

                List<Tuple<int, int>> lightCombinations = new List<Tuple<int, int>>();

                if (FocusingEnable)
                {
                    if (FocusingRange == 0 || FocusingInterval == 0)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Error", "Focusing Range and Interval cannot be zero.", EnumMessageStyle.Affirmative);
                        return;
                    }
                }

                if (LightEnable)
                {
                    bool useCoaxial = false;
                    bool useAUX = false;

                    if (COAXIAL_LightStart == 0 && COAXIAL_LightEnd == 0 && COAXIAL_LightInterval == 0)
                    {
                        useCoaxial = false;
                    }
                    else
                    {
                        useCoaxial = true;
                    }

                    if (AUX_LightStart == 0 && AUX_LightEnd == 0 && AUX_LightInterval == 0)
                    {
                        useAUX = false;
                    }
                    else
                    {
                        useAUX = true;
                    }

                    List<int> coaxialValues = null;
                    List<int> auxValues = null;
                    int current_coaxial_val = 0;
                    int current_aux_val = 0;

                    if (useCoaxial)
                    {
                        if (COAXIAL_LightInterval == 0)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error", "Light Interval value cannot be zero.", EnumMessageStyle.Affirmative);
                            return;
                        }

                        if (COAXIAL_LightStart > COAXIAL_LightEnd || AUX_LightStart > AUX_LightEnd)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error", "Light Start value cannot be greater than Light End value.", EnumMessageStyle.Affirmative);
                            return;
                        }

                        coaxialValues = GenerateLightValues(COAXIAL_LightStart, COAXIAL_LightEnd, COAXIAL_LightInterval);
                    }
                    else
                    {
                        current_coaxial_val = CurCam.GetLight(EnumLightType.COAXIAL);
                    }

                    if (useAUX)
                    {
                        if (AUX_LightInterval == 0)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error", "Light Interval value cannot be zero.", EnumMessageStyle.Affirmative);
                            return;
                        }

                        if (AUX_LightStart > AUX_LightEnd)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error", "Light Start value cannot be greater than Light End value.", EnumMessageStyle.Affirmative);
                            return;
                        }

                        auxValues = GenerateLightValues(AUX_LightStart, AUX_LightEnd, AUX_LightInterval);
                    }
                    else
                    {
                        current_aux_val = CurCam.GetLight(EnumLightType.AUX);
                    }

                    lightCombinations = GenerateLightCombinations(coaxialValues, auxValues, current_coaxial_val, current_aux_val);
                }
                else
                {
                    // 현재 조명값을 사용, SetLight()를 호출하지 않기 때문에, 사실 현재 조명값을 얻어올 필요는 없다.
                    lightCombinations.Add(new Tuple<int, int>(CurCam.GetLight(EnumLightType.COAXIAL), CurCam.GetLight(EnumLightType.AUX)));
                }

                // LIGHT
                foreach (var lightCombination in lightCombinations)
                {
                    int coaxialValue = lightCombination.Item1;
                    int auxValue = lightCombination.Item2;

                    if (LightEnable)
                    {
                        foreach (var lightindex in CurCam.LightsChannels)
                        {
                            EnumLightType enumLightType = lightindex.Type.Value;
                            if (enumLightType == EnumLightType.COAXIAL)
                            {
                                CurCam.SetLight(enumLightType, (ushort)coaxialValue);
                            }
                            else if (enumLightType == EnumLightType.AUX)
                            {
                                CurCam.SetLight(enumLightType, (ushort)auxValue);
                            }
                        }
                    }

                    var savefolderpath = PrepareSaveFolder(curcamtype, coaxialValue, auxValue);

                    if (SelectedOption == AdvancedOption.NotUse)
                    {
                        foreach (var dutNo in TargetDutNos)
                        {
                            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                            var dutpos = GetDutUpperLeftPosWithOutClearance(dutNo);

                            // Extension
                            AdjustDutPositionForExtension(ref dutpos);

                            Point firstMovePos = CalculateFirstMovePosition(dutpos);
                            InitializeScanParameters(firstMovePos, out double offset_x, out double offset_y, out double half_roi_x_um, out double half_roi_y_um);

                            int xidx = 0;
                            int yidx = 0;
                            bool islimit_x = false;
                            bool islimit_y = false;

                            int sign_x = 1;
                            int sign_y = -1;

                            isSettingFileSaved = false;

                            do
                            {
                                if (FocusingEnable)
                                {
                                    for (int i = -(FocusingRange / 2); i <= FocusingRange / 2; i += FocusingInterval)
                                    {
                                        double z = this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value;

                                        if (ProbeCard.GetAlignState() == AlignStateEnum.DONE)
                                        {
                                            z = ProbeCard.ProbeCardDevObjectRef.PinHeight;
                                        }

                                        z = z + i;

                                        retVal = MoveToPinPosition(firstMovePos.X, firstMovePos.Y, z);
                                        await SaveImageAsync(savefolderpath, firstMovePos.X, firstMovePos.Y, z, xidx, yidx, dutNo);
                                    }
                                }
                                else
                                {
                                    double z = this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value;

                                    if (ProbeCard.GetAlignState() == AlignStateEnum.DONE)
                                    {
                                        z = ProbeCard.ProbeCardDevObjectRef.PinHeight;
                                    }

                                    retVal = MoveToPinPosition(firstMovePos.X, firstMovePos.Y, z);

                                    await SaveImageAsync(savefolderpath, firstMovePos.X, firstMovePos.Y, z, xidx, yidx, dutNo);
                                }

                                var updateResult = UpdateScanPosition(firstMovePos, xidx, yidx, dutpos, half_roi_x_um, half_roi_y_um, offset_x, offset_y, sign_x, sign_y);

                                firstMovePos = updateResult.newTargetPos;
                                xidx = updateResult.newXIdx;
                                yidx = updateResult.newYIdx;
                                islimit_x = updateResult.isLimitX;
                                islimit_y = updateResult.isLimitY;

                            } while (!islimit_x && !islimit_y);
                        }
                    }
                    else if (SelectedOption == AdvancedOption.RegPin)
                    {
                        if (FocusingEnable)
                        {
                            foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (var tmpPinData in dut.PinList)
                                {
                                    for (int i = -(FocusingRange / 2); i <= FocusingRange / 2; i += FocusingInterval)
                                    {
                                        await ProcessPinData(dut, tmpPinData, savefolderpath, i);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (var tmpPinData in dut.PinList)
                                {
                                    await ProcessPinData(dut, tmpPinData, savefolderpath, 0);
                                }
                            }
                        }
                    }
                    else if (SelectedOption == AdvancedOption.RegKey)
                    {
                        if (FocusingEnable)
                        {
                            foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (var tmpPinData in dut.PinList)
                                {
                                    for (int i = -(FocusingRange / 2); i <= FocusingRange / 2; i += FocusingInterval)
                                    {
                                        await ProcessKeyData(dut, tmpPinData, savefolderpath, i);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (var tmpPinData in dut.PinList)
                                {
                                    await ProcessKeyData(dut, tmpPinData, savefolderpath, 0);
                                }
                            }
                        }
                    }
                    else if(SelectedOption == AdvancedOption.RegPinAndKey)
                    {
                        if (FocusingEnable)
                        {
                            foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (var tmpPinData in dut.PinList)
                                {
                                    for (int i = -(FocusingRange / 2); i <= FocusingRange / 2; i += FocusingInterval)
                                    {
                                        await ProcessPinData(dut, tmpPinData, savefolderpath, i);
                                    }

                                    for (int i = -(FocusingRange / 2); i <= FocusingRange / 2; i += FocusingInterval)
                                    {
                                        await ProcessKeyData(dut, tmpPinData, savefolderpath, i);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (var tmpPinData in dut.PinList)
                                {
                                    await ProcessPinData(dut, tmpPinData, savefolderpath, 0);
                                    await ProcessKeyData(dut, tmpPinData, savefolderpath, 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task ProcessPinData(IDut dut, IPinData tmpPinData, string savefolderpath, int zOffset)
        {
            try
            {
                PinCoordinate pc = new PinCoordinate(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value, tmpPinData.AbsPos.Z.Value + zOffset);
                var cb = this.CoordinateManager().PinHighPinConvert.ConvertBack(pc);

                if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, cb.X.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR ||
                    this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, cb.Y.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR ||
                    this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, cb.Z.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR)
                {
                    LoggerManager.Error($"PinAlign() : SW Limit Error #" + tmpPinData.PinNum.Value);
                }
                else
                {
                    double xpos = pc.X.Value;
                    double ypos = pc.Y.Value;
                    double zpos = pc.Z.Value + zOffset;

                    var retval = this.StageSupervisor().StageModuleState.PinHighViewMove(xpos, ypos, zpos);

                    if (retval == EventCodeEnum.NONE)
                    {
                        await SaveImageAsync(savefolderpath, xpos, ypos, zpos, tmpPinData.PinNum.Value, dut.DutNumber);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task ProcessKeyData(IDut dut, IPinData tmpPinData, string savefolderpath, int zOffset)
        {
            try
            {
                if (tmpPinData.PinSearchParam.AlignKeyHigh != null && tmpPinData.PinSearchParam.AlignKeyHigh.Count > 0)
                {
                    PinCoordinate pc = new PinCoordinate(tmpPinData.AbsPos.X.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value,
                                                         tmpPinData.AbsPos.Y.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value,
                                                         tmpPinData.AbsPos.Z.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value + zOffset);

                    var cb = this.CoordinateManager().PinHighPinConvert.ConvertBack(pc);

                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, cb.X.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR ||
                        this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, cb.Y.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR ||
                        this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, cb.Z.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR)
                    {
                        LoggerManager.Error($"PinAlign() : SW Limit Error #" + tmpPinData.PinNum.Value);
                    }
                    else
                    {
                        double xpos = pc.X.Value;
                        double ypos = pc.Y.Value;
                        double zpos = pc.Z.Value;

                        var retval = this.StageSupervisor().StageModuleState.PinHighViewMove(xpos, ypos, zpos);

                        if (retval == EventCodeEnum.NONE)
                        {
                            await SaveImageAsync(savefolderpath, xpos, ypos, zpos, tmpPinData.PinNum.Value, dut.DutNumber);
                        }
                    }

                }
                else
                {
                    // Key 정보가 존재하지 않는 경우.
                    var retval = EventCodeEnum.PIN_HIGH_KEY_INVAILD;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AdjustDutPositionForExtension(ref double[,] dutpos)
        {
            try
            {
                dutpos[0, 0] -= ExtensionArea_X;
                dutpos[0, 1] += ExtensionArea_Y;
                dutpos[1, 0] += ExtensionArea_X;
                dutpos[1, 1] -= ExtensionArea_Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Point CalculateFirstMovePosition(double[,] dutpos)
        {
            Point retval = new Point();

            try
            {
                double half_roi_x_um = CurCam.GetRatioY() * CurCam.GetGrabSizeWidth() / 2.0;
                double half_roi_y_um = CurCam.GetRatioY() * CurCam.GetGrabSizeHeight() / 2.0;

                AmoutOfOffsetFromCenter = new Point(10, 10);

                retval = new Point
                {
                    X = dutpos[0, 0] + half_roi_x_um - AmoutOfOffsetFromCenter.X,
                    Y = dutpos[0, 1] - half_roi_x_um + AmoutOfOffsetFromCenter.Y
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void InitializeScanParameters(Point firstMovePos, out double offset_x, out double offset_y, out double half_roi_x_um, out double half_roi_y_um)
        {
            offset_x = 0;
            offset_y = 0;
            half_roi_x_um = 0;
            half_roi_y_um = 0;

            try
            {
                double roi_x_um = CurCam.GetRatioY() * CurCam.GetGrabSizeWidth();
                double roi_y_um = CurCam.GetRatioY() * CurCam.GetGrabSizeHeight();

                LoggerManager.Debug($"[{this.GetType().Name}], FuncCardDutScanCommand() : Upper Left ({firstMovePos.X}, {firstMovePos.Y}), Dut Size = ({WaferObject.GetSubsInfo().ActualDieSize.Width.Value}, {WaferObject.GetSubsInfo().ActualDieSize.Height.Value}), Cam ROI = ({roi_x_um},{roi_y_um}) Overlap = ({AmoutOfOverlap_X}, {AmoutOfOverlap_Y})");

                half_roi_x_um = roi_x_um / 2.0;
                half_roi_y_um = roi_y_um / 2.0;
                offset_x = roi_x_um - AmoutOfOverlap_X;
                offset_y = roi_y_um - AmoutOfOverlap_Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task SaveImageAsync(string savefolderpath, double targetPosX, double targetPosY, double targetPosZ, int xidx, int yidx, int dutNo)
        {
            try
            {
                EnumProberCam curcamtype = CurCam.GetChannelType();
                Current_Image_No++;

                var current_img_buffer = this.VisionManager().SingleGrab(curcamtype, this);
                string timePart = current_img_buffer.CapturedTime.ToString("HHmmss");

                string savefullpath = string.Empty;

                savefullpath = $"{savefolderpath}{timePart}_No#{Current_Image_No:D4}_[{xidx},{yidx}]_PosX#{targetPosX:F2}_PosY#{targetPosY:F2}_PosZ#{targetPosZ}.bmp";

                this.VisionManager().SaveImageBuffer(current_img_buffer, savefullpath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                LoggerManager.Debug($"[{this.GetType().Name}], FuncCardDutScanCommand() : Save Image, Path = {savefullpath}");

                if (!isSettingFileSaved)
                {
                    WriteToFile(savefolderpath, timePart, dutNo: dutNo);
                    isSettingFileSaved = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task SaveImageAsync(string savefolderpath, double targetPosX, double targetPosY, double targetPosZ, int pinNo, int dutNo)
        {
            try
            {
                EnumProberCam curcamtype = CurCam.GetChannelType();
                Current_Image_No++;

                var current_img_buffer = this.VisionManager().SingleGrab(curcamtype, this);
                string timePart = current_img_buffer.CapturedTime.ToString("HHmmss");

                string savefullpath = string.Empty;

                savefullpath = $"{savefolderpath}{timePart}_No#{Current_Image_No:D4}_PinNo#{pinNo}_PosX#{targetPosX:F2}_PosY#{targetPosY:F2}_PosZ#{targetPosZ}.bmp";

                this.VisionManager().SaveImageBuffer(current_img_buffer, savefullpath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                LoggerManager.Debug($"[{this.GetType().Name}], FuncCardDutScanCommand() : Save Image, Path = {savefullpath}");

                if (!isSettingFileSaved)
                {
                    WriteToFile(savefolderpath, timePart, dutNo: dutNo);
                    isSettingFileSaved = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string PrepareSaveFolder(EnumProberCam curcamtype, int coaxal = 0, int aux = 0)
        {
            string retval = string.Empty;

            try
            {
                DateTime currentDate = DateTime.Now;
                string folderName = currentDate.ToString("yyyyMMdd");

                if (LightEnable)
                {
                    retval = $"C:\\Logs\\DigitalTwin\\{curcamtype}\\{folderName}\\COAXIAL{coaxal}_AUX{aux}\\";
                }
                else
                {
                    retval = $"C:\\Logs\\DigitalTwin\\{curcamtype}\\{folderName}\\";
                }

                if (!Directory.Exists(retval))
                {
                    Directory.CreateDirectory(retval);
                }

                OutputFilePath = retval;
                Current_Image_No = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private (Point newTargetPos, int newXIdx, int newYIdx, bool isLimitX, bool isLimitY) UpdateScanPosition(Point targetPos, int xidx, int yidx, double[,] dutpos, double half_roi_x_um, double half_roi_y_um, double offset_x, double offset_y, int sign_x, int sign_y)
        {
            double Current_roi_x = targetPos.X + (half_roi_x_um * sign_x);
            double Current_roi_y = targetPos.Y + (half_roi_y_um * sign_y);

            bool islimit_x = false;
            bool islimit_y = false;

            if (Current_roi_y < dutpos[1, 1])
            {
                if (Current_roi_x > dutpos[1, 0] || Current_roi_x < dutpos[0, 0])
                {
                    islimit_x = true;
                }
            }

            if (sign_x == 1)
            {
                if (Current_roi_x > dutpos[1, 0])
                {
                    sign_x *= -1;
                    targetPos.Y += (offset_y * sign_y);
                    yidx++;
                }
                else
                {
                    targetPos.X += (offset_x * sign_x);
                    xidx++;
                }
            }
            else
            {
                if (Current_roi_x < dutpos[0, 0])
                {
                    sign_x *= -1;
                    targetPos.Y += (offset_y * sign_y);
                    yidx++;
                }
                else
                {
                    targetPos.X += (offset_x * sign_x);
                    xidx--;
                }
            }

            islimit_y = (targetPos.Y < dutpos[1, 1]);

            return (targetPos, xidx, yidx, islimit_x, islimit_y);
        }

        private List<int> GenerateLightValues(int start, int end, int interval)
        {
            List<int> values = new List<int>();
            for (int value = start; value <= end; value += interval)
            {
                values.Add(value);
            }
            return values;
        }

        private List<Tuple<int, int>> GenerateLightCombinations(List<int> coaxialValues, List<int> auxValues, int currentCoaxialVal, int currentAuxVal)
        {
            List<Tuple<int, int>> combinations = new List<Tuple<int, int>>();

            if (coaxialValues == null && auxValues == null)
            {
                combinations.Add(new Tuple<int, int>(currentCoaxialVal, currentAuxVal));
            }
            else if (coaxialValues == null)
            {
                foreach (var aux in auxValues)
                {
                    combinations.Add(new Tuple<int, int>(currentCoaxialVal, aux));
                }
            }
            else if (auxValues == null)
            {
                foreach (var coaxial in coaxialValues)
                {
                    combinations.Add(new Tuple<int, int>(coaxial, currentAuxVal));
                }
            }
            else
            {
                foreach (var coaxial in coaxialValues)
                {
                    foreach (var aux in auxValues)
                    {
                        combinations.Add(new Tuple<int, int>(coaxial, aux));
                    }
                }
            }

            return combinations;
        }

        private AsyncCommand<object> _WaferDieScanCommand;
        public ICommand WaferDieScanCommand
        {
            get
            {
                if (null == _WaferDieScanCommand) _WaferDieScanCommand = new AsyncCommand<object>(FuncWaferDieScanCommand);
                return _WaferDieScanCommand;
            }
        }

        private async Task FuncWaferDieScanCommand(object obj)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                if (TargetUserIndexs.Count == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error", "No data has been selected.", EnumMessageStyle.Affirmative);

                    return;
                }


                foreach (var userindex in TargetUserIndexs)
                {
                    // 1. 좌측 상단 (Fixed) 으로 이동

                    var diepos = GetDieUpperLeftPosWithOutClearance(userindex);

                    diepos[0, 0] = diepos[0, 0] - ExtensionArea_X;
                    diepos[0, 1] = diepos[0, 1] + ExtensionArea_Y;

                    diepos[1, 0] = diepos[1, 0] + ExtensionArea_X;
                    diepos[1, 1] = diepos[1, 1] - ExtensionArea_Y;

                    Point firstMovePos = new Point();

                    // ROI
                    double roi_x_um = CurCam.GetRatioY() * CurCam.GetGrabSizeWidth();
                    double roi_y_um = CurCam.GetRatioY() * CurCam.GetGrabSizeHeight();

                    LoggerManager.Debug($"[{this.GetType().Name}], FuncWaferDieScanCommand() : Upper Left ({diepos[0, 0]}, {diepos[0, 1]}), Lower Right ({diepos[1, 0]}, {diepos[1, 1]}), Die Size = ({WaferObject.GetSubsInfo().ActualDieSize.Width.Value}, {WaferObject.GetSubsInfo().ActualDieSize.Height.Value}), Cam ROI = ({roi_x_um},{roi_y_um}) Overlap = ({AmoutOfOverlap_X}, {AmoutOfOverlap_Y})");

                    // Half of ROI
                    double half_roi_x_um = roi_x_um / 2.0;
                    double half_roi_y_um = roi_y_um / 2.0;

                    double offset_x = 0;
                    double offset_y = 0;

                    int sign_x = 1;
                    int sign_y = -1;

                    // Hard Coding (Unit : um)
                    AmoutOfOffsetFromCenter = new Point(10, 10);

                    firstMovePos.X = diepos[0, 0] + half_roi_x_um - AmoutOfOffsetFromCenter.X;
                    firstMovePos.Y = diepos[0, 1] - half_roi_x_um + AmoutOfOffsetFromCenter.Y;

                    bool islimit_x = false;
                    bool islimit_y = false;

                    offset_x = roi_x_um - AmoutOfOverlap_X;
                    offset_y = roi_y_um - AmoutOfOverlap_Y;

                    double TargetPosX = firstMovePos.X;
                    double TargetPosY = firstMovePos.Y;

                    EnumProberCam curcamtype = CurCam.GetChannelType();

                    // Get the current date
                    DateTime currentDate = DateTime.Now;

                    // Format the date as YYYYMMDD
                    string folderName = currentDate.ToString("yyyyMMdd");

                    var savefolderpath = $"C:\\Logs\\DigitalTwin\\{curcamtype}\\{folderName}\\";
                    var savefullpath = string.Empty;

                    if (!Directory.Exists(savefolderpath))
                    {
                        Directory.CreateDirectory(savefolderpath);
                    }

                    OutputFilePath = savefolderpath;

                    Current_Image_No = 0;

                    int xidx = 0;
                    int yidx = 0;

                    bool isSettingFileSaved = false;

                    do
                    {
                        retVal = MoveToWaferPosition(TargetPosX, TargetPosY);

                        // Save image
                        Current_Image_No++;
                        var current_img_buffer = this.VisionManager().SingleGrab(curcamtype, this);
                        string timePart = current_img_buffer.CapturedTime.ToString("HHmmss");

                        savefullpath = $"{savefolderpath}{timePart}_No#{Current_Image_No:D4}_[{xidx},{yidx}]_PosX#{TargetPosX:F2}_PosY#{TargetPosY:F2}.bmp";

                        this.VisionManager().SaveImageBuffer(current_img_buffer, savefullpath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                        LoggerManager.Debug($"[{this.GetType().Name}], FuncWaferDieScanCommand() : Save Image, Path = {savefullpath}");

                        if (!isSettingFileSaved)
                        {
                            WriteToFile(savefolderpath, timePart, userIndex: userindex);
                            isSettingFileSaved = true;
                        }

                        double Current_roi_x = TargetPosX + (half_roi_x_um * sign_x);
                        double Current_roi_y = TargetPosY + (half_roi_y_um * sign_y);

                        if (Current_roi_y < diepos[1, 1])
                        {
                            if (Current_roi_x > diepos[1, 0] || Current_roi_x < diepos[0, 0])
                            {
                                break;
                            }
                        }

                        if (sign_x == 1)
                        {
                            if (Current_roi_x > diepos[1, 0])
                            {
                                sign_x *= -1;
                                TargetPosY += (offset_y * sign_y);
                                yidx++;
                            }
                            else
                            {
                                TargetPosX += (offset_x * sign_x);
                                xidx++;
                            }
                        }
                        else
                        {
                            if (Current_roi_x < diepos[0, 0])
                            {
                                sign_x *= -1;
                                TargetPosY += (offset_y * sign_y);
                                yidx++;
                            }
                            else
                            {
                                TargetPosX += (offset_x * sign_x);
                                xidx--;
                            }
                        }

                    } while (true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _StartGrabCommand;
        public ICommand StartGrabCommand
        {
            get
            {
                if (null == _StartGrabCommand) _StartGrabCommand = new RelayCommand<object>(StartGrab);
                return _StartGrabCommand;
            }
        }
        private void StartGrab(object noparam)
        {
            try
            {
                EnumProberCam curcam = EnumProberCam.UNDEFINED;

                switch (SelectedCam)
                {
                    case enumStageCamType.UNDEFINED:
                        curcam = EnumProberCam.UNDEFINED;
                        break;
                    case enumStageCamType.WaferHigh:
                        curcam = EnumProberCam.WAFER_HIGH_CAM;
                        //Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        //Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        //Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        //Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        //Cam = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                        break;
                    default:
                        break;
                }

                ChangeCamAndGrab(curcam, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _StopGrabCommand;
        public ICommand StopGrabCommand
        {
            get
            {
                if (null == _StopGrabCommand) _StopGrabCommand = new RelayCommand<object>(StopGrabCommandFunc);
                return _StopGrabCommand;
            }
        }
        private void StopGrabCommandFunc(object noparam)
        {
            try
            {
                EnumProberCam curcam = EnumProberCam.UNDEFINED;

                switch (SelectedCam)
                {
                    case enumStageCamType.UNDEFINED:
                        curcam = EnumProberCam.UNDEFINED;
                        break;
                    case enumStageCamType.WaferHigh:
                        curcam = EnumProberCam.WAFER_HIGH_CAM;
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        break;
                    default:
                        break;
                }

                if (curcam != EnumProberCam.UNDEFINED)
                {
                    this.VisionManager().StopGrab(curcam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeCamAndGrab(EnumProberCam curcam, bool isContinuous = false)
        {
            try
            {
                if (curcam != EnumProberCam.UNDEFINED)
                {
                    if (isContinuous)
                    {
                        this.VisionManager().StartGrab(curcam, this);
                    }
                    else
                    {
                        this.VisionManager().SingleGrab(curcam, this);
                    }

                    LightJog.InitCameraJog(this, curcam);
                    CurCam = this.VisionManager().GetCam(curcam);

                    EnumProberCam curcamType = CurCam.GetChannelType();

                    if (curcamType == EnumProberCam.WAFER_LOW_CAM || curcamType == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        MainViewTarget = DisplayPort;
                        MiniViewTarget = WaferObject;

                        SeletedTabControlIndex = 0;
                    }
                    else if (curcamType == EnumProberCam.PIN_LOW_CAM || curcamType == EnumProberCam.PIN_HIGH_CAM)
                    {
                        MainViewTarget = DisplayPort;
                        MiniViewTarget = ProbeCard;

                        SeletedTabControlIndex = 1;
                    }

                    CurCam_ROI_X = CurCam.GetRatioX() * CurCam.GetGrabSizeWidth();
                    CurCam_ROI_Y = CurCam.GetRatioY() * CurCam.GetGrabSizeHeight();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _SingleGrabCommand;
        public ICommand SingleGrabCommand
        {
            get
            {
                if (null == _SingleGrabCommand) _SingleGrabCommand = new RelayCommand<object>(SingleGrabCommandFunc);
                return _SingleGrabCommand;
            }
        }

        private void SingleGrabCommandFunc(object noparam)
        {
            try
            {
                EnumProberCam curcam = EnumProberCam.UNDEFINED;

                switch (SelectedCam)
                {
                    case enumStageCamType.UNDEFINED:
                        curcam = EnumProberCam.UNDEFINED;
                        break;
                    case enumStageCamType.WaferHigh:
                        curcam = EnumProberCam.WAFER_HIGH_CAM;
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        break;
                    default:
                        break;
                }

                ChangeCamAndGrab(curcam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }

        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<LocationEnum> _MoveToWaferPositionCommand;
        public ICommand MoveToWaferPositionCommand
        {
            get
            {
                if (null == _MoveToWaferPositionCommand) _MoveToWaferPositionCommand = new AsyncCommand<LocationEnum>(FuncMoveToWaferPositionCommand);
                return _MoveToWaferPositionCommand;
            }
        }

        private async Task FuncMoveToWaferPositionCommand(LocationEnum location)
        {
            try
            {
                await Task.Run(() =>
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    bool isValid = false;

                    if (CurCam != null)
                    {
                        EnumProberCam camType = CurCam.GetChannelType();

                        if (camType == EnumProberCam.WAFER_LOW_CAM || camType == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            WaferCoordinate wc = null;
                            double x = 0;
                            double y = 0;

                            var mi = this.CoordinateManager().UserIndexConvertToMachineIndex(TargetUserIndex);

                            wc = this.WaferAligner().MachineIndexConvertToDieLeftCorner(mi.XIndex, mi.YIndex);

                            if (wc != null && mi.XIndex >= 0 && mi.YIndex >= 0)
                            {
                                //좌측 상단
                                double pt1_x = wc.X.Value;
                                double pt1_y = wc.Y.Value + WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                                //우측 하단
                                double pt2_x = wc.X.Value + WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                                double pt2_y = wc.Y.Value;

                                double half_Width = WaferObject.GetSubsInfo().ActualDieSize.Width.Value / 2.0;
                                double half_Height = WaferObject.GetSubsInfo().ActualDieSize.Height.Value / 2.0;

                                switch (location)
                                {
                                    case LocationEnum.UpperLeft:
                                        x = pt1_x;
                                        y = pt1_y;
                                        break;
                                    case LocationEnum.Up:
                                        x = pt1_x + half_Width;
                                        y = pt1_y;
                                        break;
                                    case LocationEnum.UpperRight:
                                        x = pt2_x;
                                        y = pt1_y;
                                        break;
                                    case LocationEnum.Left:
                                        x = pt1_x;
                                        y = pt2_y + half_Height;
                                        break;
                                    case LocationEnum.Center:
                                        x = pt1_x + half_Width;
                                        y = pt2_y + half_Height;
                                        break;
                                    case LocationEnum.Right:
                                        x = pt2_x;
                                        y = pt2_y + half_Height;
                                        break;
                                    case LocationEnum.LowerLeft:
                                        x = pt1_x;
                                        y = pt2_y;
                                        break;
                                    case LocationEnum.Down:
                                        x = pt1_x + half_Width;
                                        y = pt2_y;
                                        break;
                                    case LocationEnum.LowerRight:
                                        x = pt2_x;
                                        y = pt2_y;
                                        break;
                                    default:
                                        break;
                                }

                                retVal = this.StageSupervisor().StageModuleState.ZCLEARED();

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    retVal = MoveToWaferPosition(x, y);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[{this.GetType().Name}], FuncMoveToWaferPositionCommand() : MoveToPosition failed. retval = {retVal}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{this.GetType().Name}], FuncMoveToWaferPositionCommand() : ZCLEARED failed. retval = {retVal}");
                                }
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], FuncMoveToWaferPositionCommand() : CurCam is null.");
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<LocationEnum> _MoveToPinPositionCommand;
        public ICommand MoveToPinPositionCommand
        {
            get
            {
                if (null == _MoveToPinPositionCommand) _MoveToPinPositionCommand = new AsyncCommand<LocationEnum>(FuncMoveToPinPositionCommand);
                return _MoveToPinPositionCommand;
            }
        }

        private async Task FuncMoveToPinPositionCommand(LocationEnum location)
        {
            try
            {
                await Task.Run(() =>
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    bool isValid = false;

                    if (CurCam != null)
                    {
                        EnumProberCam camType = CurCam.GetChannelType();

                        if (camType == EnumProberCam.PIN_LOW_CAM || camType == EnumProberCam.PIN_HIGH_CAM)
                        {
                            PinCoordinate pc = null;
                            double x = 0;
                            double y = 0;

                            pc = TargetDutLeftCorner(TargetDutNo);

                            if (pc != null)
                            {
                                //좌측 상단
                                double pt1_x = pc.X.Value;
                                double pt1_y = pc.Y.Value + WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                                //우측 하단
                                double pt2_x = pc.X.Value + WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                                double pt2_y = pc.Y.Value;

                                double half_Width = WaferObject.GetSubsInfo().ActualDieSize.Width.Value / 2.0;
                                double half_Height = WaferObject.GetSubsInfo().ActualDieSize.Height.Value / 2.0;

                                switch (location)
                                {
                                    case LocationEnum.UpperLeft:
                                        x = pt1_x;
                                        y = pt1_y;
                                        break;
                                    case LocationEnum.Up:
                                        x = pt1_x + half_Width;
                                        y = pt1_y;
                                        break;
                                    case LocationEnum.UpperRight:
                                        x = pt2_x;
                                        y = pt1_y;
                                        break;
                                    case LocationEnum.Left:
                                        x = pt1_x;
                                        y = pt2_y + half_Height;
                                        break;
                                    case LocationEnum.Center:
                                        x = pt1_x + half_Width;
                                        y = pt2_y + half_Height;
                                        break;
                                    case LocationEnum.Right:
                                        x = pt2_x;
                                        y = pt2_y + half_Height;
                                        break;
                                    case LocationEnum.LowerLeft:
                                        x = pt1_x;
                                        y = pt2_y;
                                        break;
                                    case LocationEnum.Down:
                                        x = pt1_x + half_Width;
                                        y = pt2_y;
                                        break;
                                    case LocationEnum.LowerRight:
                                        x = pt2_x;
                                        y = pt2_y;
                                        break;
                                    default:
                                        break;
                                }

                                retVal = this.StageSupervisor().StageModuleState.ZCLEARED();

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    double z = this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value;

                                    if (ProbeCard.GetAlignState() == AlignStateEnum.DONE)
                                    {
                                        z = ProbeCard.ProbeCardDevObjectRef.PinHeight;
                                    }

                                    retVal = MoveToPinPosition(x, y, z);

                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[{this.GetType().Name}], FuncMoveToPinPositionCommand() : MoveToPosition failed. retval = {retVal}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{this.GetType().Name}], FuncMoveToPinPositionCommand() : ZCLEARED failed. retval = {retVal}");
                                }
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], FuncMoveToPinPositionCommand() : CurCam is null.");
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }

    public class TempDut
    {
        public int DutNumber { get; set; }
        public PinCoordinate Coordinate { get; set; }

        public TempDut(int dutNumber, PinCoordinate coordinate)
        {
            DutNumber = dutNumber;
            Coordinate = coordinate;
        }
    }
}
