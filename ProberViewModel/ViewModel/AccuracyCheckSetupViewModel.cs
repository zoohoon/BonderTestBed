using Focusing;
using HexagonJogControl;
using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberViewModel.Data;
using RelayCommandBase;
using SerializerUtil;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UcDisplayPort;
using VirtualKeyboardControl;

namespace ProberViewModel.ViewModel
{
    public class AccuracyCheckSetupViewModel : IMainScreenViewModel, IUseLightJog
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private readonly Guid _ViewModelGUID = new Guid("298f3074-0cfd-4eda-9e33-9c88a9a3de91");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;

        public IStageSupervisor StageSupervisor { get; set; }
        private IVisionManager VisionManager { get; set; }
        public ICoordinateManager CoordinateManager { get; set; }

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

        private double _Spec_X_um;
        public double Spec_X_um
        {
            get { return _Spec_X_um; }
            set
            {
                if (_Spec_X_um != value)
                {
                    _Spec_X_um = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Spec_Y_um;
        public double Spec_Y_um
        {
            get { return _Spec_Y_um; }
            set
            {
                if (_Spec_Y_um != value)
                {
                    _Spec_Y_um = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaitTime = 100;
        public int WaitTime
        {
            get { return _WaitTime; }
            set
            {
                if (_WaitTime != value)
                {
                    _WaitTime = value;
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
                if (_CurCam != value)
                {
                    _CurCam = value;
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

        private IFocusing _FocusModel;
        public IFocusing FocusModel
        {
            get { return _FocusModel; }
            set
            {
                if (value != _FocusModel)
                {
                    _FocusModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set { _FocusParam = value; }
        }

        #region ==> UcDispaly Port Target Rectangle

        private UserControlFucEnum _UseUserControl = UserControlFucEnum.DEFAULT;
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

        string patternpath = @"C:\ProberSystem\AccuracyCheck\AccuracyCheck_Pattern.mmo";
        PatternInfomation ptinfo = null;
        #endregion

        private string _FilePath;
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                _FilePath = value;
                RaisePropertyChanged();
            }
        }

        System.Threading.CancellationTokenSource cancelToken = null;

        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }

        private ObservableCollection<IndexDTO> _TargetDies = new ObservableCollection<IndexDTO>();
        public ObservableCollection<IndexDTO> TargetDies
        {
            get { return _TargetDies; }
            set
            {
                if (value != _TargetDies)
                {
                    _TargetDies = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IndexDTO _SelectedTargetDie;
        public IndexDTO SelectedTargetDie
        {
            get { return _SelectedTargetDie; }
            set
            {
                if (value != _SelectedTargetDie)
                {
                    _SelectedTargetDie = value;
                    RaisePropertyChanged();
                }
            }

        }

        private ObservableCollection<AccuracyInfo> _AccuracyInfos = new ObservableCollection<AccuracyInfo>();
        public ObservableCollection<AccuracyInfo> AccuracyInfos
        {
            get { return _AccuracyInfos; }
            set
            {
                if (value != _AccuracyInfos)
                {
                    _AccuracyInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _TargetUserIndex = new UserIndex();
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

        private UserIndex _ReferenceUserIndex = new UserIndex();
        public UserIndex ReferenceUserIndex
        {
            get { return _ReferenceUserIndex; }
            set
            {
                if (value != _ReferenceUserIndex)
                {
                    _ReferenceUserIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AccuracyInfo _SelectedAccuracyInfo;
        public AccuracyInfo SelectedAccuracyInfo
        {
            get { return _SelectedAccuracyInfo; }
            set
            {
                if (value != _SelectedAccuracyInfo)
                {
                    _SelectedAccuracyInfo = value;
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

        private Visibility _MotionJogVisibility = Visibility.Visible;
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

                    // !Pattern 
                    if(SeletedTabControlIndex != 1)
                    {
                        UseUserControl = UserControlFucEnum.DEFAULT;
                    }
                }
            }
        }

        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsFocusing;
        public bool IsFocusing
        {
            get => _IsFocusing;
            set
            {
                if (_IsFocusing != value)
                {
                    _IsFocusing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusROIMargin;
        public double FocusROIMargin
        {
            get => _FocusROIMargin;
            set
            {
                if (_FocusROIMargin != value)
                {
                    _FocusROIMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    FilePath = @"C:\ProberSystem\AccuracyCheck\DieInfos.json"; // Path to save and load the data

                    Spec_X_um = 0;
                    Spec_Y_um = 0;

                    StageSupervisor = this.StageSupervisor();
                    VisionManager = this.VisionManager();
                    CoordinateManager = this.CoordinateManager();

                    DisplayPort = new DisplayPort() { GUID = new Guid("b582cfa0-2ed6-4a70-b44a-02b4c874fcc5") };

                    foreach (enumStageCamType camtype in (enumStageCamType[])System.Enum.GetValues(typeof(enumStageCamType)))
                    {
                        if (camtype == enumStageCamType.WaferLow ||
                            camtype == enumStageCamType.WaferHigh)
                        {
                            StageCamList.Add(new StageCamera(camtype));
                        }
                    }

                    LightJog = new LightJogViewModel(maxLightValue: 255, minLightValue: 0);
                    MotionJog = new HexagonJogViewModel();

                    if (this.FocusManager() != null)
                    {
                        FocusModel = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());
                        FocusParam = new NormalFocusParameter();
                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam);
                    }

                    #region Binding DisplayPort

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

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task ChangeMapIndex(object newVal)
        {
            try
            {
                var cur_mi = this.StageSupervisor().WaferObject.GetCurrentMIndex();
                var target_ui = CoordinateManager.MachineIndexConvertToUserIndex(cur_mi);

                TargetUserIndex.XIndex = target_ui.XIndex;
                TargetUserIndex.YIndex = target_ui.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                SeletedTabControlIndex = 0;
                UseUserControl = UserControlFucEnum.DEFAULT;

                this.StageSupervisor().StageModuleState.WaferHighViewMove(Wafer.GetSubsInfo().WaferCenter.X.Value, Wafer.GetSubsInfo().WaferCenter.Y.Value, Wafer.GetSubsInfo().ActualThickness);

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                // 조명 초기화
                ushort defaultlightvalue = 128;
                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                }

                this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM, this);
                LightJog.InitCameraJog(this, EnumProberCam.WAFER_HIGH_CAM);

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += ChangeMapIndex;
                }

                LoadScanDieCommandFunc(null);

                var cur_mi = this.StageSupervisor().WaferObject.GetCurrentMIndex();
                var cur_ui = this.CoordinateManager().MachineIndexConvertToUserIndex(cur_mi);

                TargetUserIndex.XIndex = cur_ui.XIndex;
                TargetUserIndex.YIndex = cur_ui.YIndex;

                this.Wafer.MapViewCurIndexVisiablity = true;

                IsFocusing = false;
                FocusROIMargin = 10;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= ChangeMapIndex;
                }

                this.Wafer.MapViewCurIndexVisiablity = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand<Object> _OpenOutputPathCommand;
        public ICommand OpenOutputPathCommand
        {
            get
            {
                if (null == _OpenOutputPathCommand) _OpenOutputPathCommand = new AsyncCommand<Object>(OpenOutputPathCommandFunc);
                return _OpenOutputPathCommand;
            }
        }

        private async Task OpenOutputPathCommandFunc(Object param)
        {
            try
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    var outputPath = Path.GetDirectoryName(FilePath);
                    if (Directory.Exists(outputPath))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", outputPath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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
            try
            {
                if (UseUserControl != UserControlFucEnum.PTRECT)
                {
                    UseUserControl = UserControlFucEnum.PTRECT;
                }
                else
                {
                    UseUserControl = UserControlFucEnum.DEFAULT;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
            }

            return retVal;
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

        private void SetPatternInfo(int width, int height)
        {
            try
            {
                ptinfo = new PatternInfomation();
                ptinfo.CamType.Value = CurCam.GetChannelType();

                ptinfo.PMParameter = new ProberInterfaces.Vision.PMParameter();

                ptinfo.PMParameter.PattWidth.Value = width;
                ptinfo.PMParameter.PattHeight.Value = height;

                ptinfo.PMParameter.ModelFilePath.Value = @"C:\ProberSystem\AccuracyCheck\AccuracyCheck_Pattern";
                ptinfo.PMParameter.PatternFileExtension.Value = ".mmo";

                ///저장 Die의 LeftCorner로 부터위치.
                WaferCoordinate coordinate = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                Point curleftcorner = this.WaferAligner().GetLeftCornerPosition(coordinate.GetX(), coordinate.GetY());

                UserIndex curui = this.CoordinateManager().GetCurUserIndex(coordinate);

                ReferenceUserIndex.XIndex = curui.XIndex;
                ReferenceUserIndex.YIndex = curui.YIndex;

                ptinfo.X.Value = coordinate.GetX() - curleftcorner.X;
                ptinfo.Y.Value = coordinate.GetY() - curleftcorner.Y;
                ptinfo.Z.Value = coordinate.GetZ();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LoadImageWithoutLocking(string path)
        {
            try
            {
                var bitmapImage = new BitmapImage();

                // 파일에서 바이트를 읽어 BitmapImage에 제공
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var imageData = new byte[fs.Length];
                    fs.Read(imageData, 0, imageData.Length);

                    using (var ms = new MemoryStream(imageData))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                    }
                }

                // Important: Freezing the image to release the file lock
                bitmapImage.Freeze();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImageSource = bitmapImage;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task RegistePatternFunc()
        {
            try
            {
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    await Task.Run(() =>
                    {
                        RegisteImageBufferParam patternparam = GetDisplayPortRectInfo();

                        patternparam.CamType = CurCam.GetChannelType();
                        patternparam.PatternPath = patternpath;

                        this.VisionManager().SavePattern(patternparam);
                        
                        LoadImageWithoutLocking(@"C:\\ProberSystem\\AccuracyCheck\\AccuracyCheck_Pattern.bmp");

                        SetPatternInfo(patternparam.Width, patternparam.Height);
                    });
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "Camera type is wrong.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
        }

        private RelayCommand _GoPatternCommand;
        public ICommand GoPatternCommand
        {
            get
            {
                if (null == _GoPatternCommand) _GoPatternCommand = new RelayCommand(GoPatternCommandFunc);
                return _GoPatternCommand;
            }
        }

        private void GoPatternCommandFunc()
        {
            try
            {
                WaferCoordinate wcoord = null;

                double movexpos;
                double moveypos;

                var ref_mi = CoordinateManager.UserIndexConvertToMachineIndex(ReferenceUserIndex);

                wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(ref_mi.XIndex, ref_mi.YIndex);

                movexpos = wcoord.GetX() + ptinfo.X.Value;
                moveypos = wcoord.GetY() + ptinfo.Y.Value;

                this.StageSupervisor().StageModuleState.WaferHighViewMove(movexpos, moveypos);
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
                {
                    return ret;
                }

                double width = CurCam.GetGrabSizeWidth() * TargetRectangleWidth / DisplayPort.StandardOverlayCanvaseWidth;
                double height = CurCam.GetGrabSizeHeight() * TargetRectangleHeight / DisplayPort.StandardOverlayCanvaseHeight;

                double offsetx = (CurCam.GetGrabSizeWidth() / 2.0) - (width / 2.0);
                double offsety = (CurCam.GetGrabSizeHeight() / 2.0) - (height / 2.0);

                ret.Width = (int)width;
                ret.Height = (int)height;
                ret.LocationX = (int)offsetx;
                ret.LocationY = (int)offsety;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private RelayCommand<Object> _GoToCommand;
        public ICommand GoToCommand
        {
            get
            {
                if (null == _GoToCommand) _GoToCommand = new RelayCommand<Object>(GoToCommandFunc);
                return _GoToCommand;
            }
        }

        private void GoToCommandFunc(Object param)
        {
            try
            {
                if(param is AccuracyInfo info)
                {
                    // TODO : 인덱스 Move시 사용된 Z값과 동일한지 확인 필요.
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(info.MoveXPos, info.MoveYPos);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TestCommand;
        public ICommand TestCommand
        {
            get
            {
                if (null == _TestCommand) _TestCommand = new RelayCommand<Object>(TestCommandFunc);
                return _TestCommand;
            }
        }

        private void TestCommandFunc(Object param)
        {
            try
            {
                if (param is AccuracyInfo info)
                {
                    this.StageSupervisor().StageModuleState.WaferHighViewMove(info.MoveXPos, info.MoveYPos);

                    Thread.Sleep(WaitTime);

                    // TODO : Pattern Matching 후, 결과값 보여주기 

                    PMResult pmret = this.VisionManager().PatternMatching(ptinfo, this);

                    if (pmret.RetValue == EventCodeEnum.NONE)
                    {
                        // 이미지 이용한 다이얼로그 제작해보자.
                    }
                    else
                    {
                        // ERROR
                    }
                }
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
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 4);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TextBoxDoubleClickCommand;
        public ICommand TextBoxDoubleClickCommand
        {
            get
            {
                if (null == _TextBoxDoubleClickCommand) _TextBoxDoubleClickCommand = new RelayCommand<Object>(TextBoxDoubleClickCommandFunc);
                return _TextBoxDoubleClickCommand;
            }
        }

        private void TextBoxDoubleClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL, 0, 4);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                UpdateSettingCommandFunc(null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _UpdateSettingCommand;
        public ICommand UpdateSettingCommand
        {
            get
            {
                if (null == _UpdateSettingCommand) _UpdateSettingCommand = new RelayCommand<Object>(UpdateSettingCommandFunc);
                return _UpdateSettingCommand;
            }
        }

        private void UpdateSettingCommandFunc(Object param)
        {
            try
            {
                if(AccuracyInfos.Count > 0)
                {
                    foreach (var info in AccuracyInfos)
                    {
                        info.XDifference = (Convert.ToDouble(info.XOffset) - Spec_X_um).ToString("F3");
                        info.YDifference = (Convert.ToDouble(info.YOffset) - Spec_Y_um).ToString("F3");

                        if (Math.Abs(Convert.ToDouble(info.XOffset)) <= Spec_X_um && Math.Abs(Convert.ToDouble(info.YOffset)) <= Spec_Y_um)
                        {
                            info.IsSuccess = true;
                        }
                        else
                        {
                            info.IsSuccess = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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
                UserIndex tmp = new UserIndex(TargetUserIndex.XIndex, TargetUserIndex.YIndex);

                bool exists = TargetDies.Any(item => item.XIndex == tmp.XIndex && item.YIndex == tmp.YIndex);

                if (!exists)
                {
                    IndexDTO Info = new IndexDTO();
                    Info.XIndex = tmp.XIndex;
                    Info.YIndex = tmp.YIndex;

                    TargetDies.Add(Info);
                    SelectedTargetDie = Info;
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
                if (SelectedTargetDie != null)
                {
                    int index = TargetDies.IndexOf(SelectedTargetDie);

                    // Remove the selected item
                    TargetDies.Remove(SelectedTargetDie);

                    // Check if there is another item to select
                    if (TargetDies.Count > index)
                    {
                        // Select the next item if it exists
                        SelectedTargetDie = TargetDies[index];
                    }
                    else if (index > 0)
                    {
                        // If the last item was removed, select the previous item
                        SelectedTargetDie = TargetDies[index - 1];
                    }
                    else
                    {
                        // If the list is empty, keep the SelectedAccuracyInfo as null
                        SelectedTargetDie = null;
                    }
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
                if (TargetDies != null)
                {
                    TargetDies.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _SaveScanDieCommand;
        public ICommand SaveScanDieCommand
        {
            get
            {
                if (null == _SaveScanDieCommand) _SaveScanDieCommand = new RelayCommand<Object>(SaveScanDieCommandFunc);
                return _SaveScanDieCommand;
            }
        }

        private void SaveScanDieCommandFunc(Object param)
        {
            try
            {
                if(TargetDies.Count > 0)
                {
                    // Convert the list to JSON and save directly to a file
                    using (var streamWriter = new StreamWriter(FilePath))
                    {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(streamWriter, TargetDies);
                    }
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "No data available for saving.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _LoadScanDieCommand;
        public ICommand LoadScanDieCommand
        {
            get
            {
                if (null == _LoadScanDieCommand) _LoadScanDieCommand = new RelayCommand<Object>(LoadScanDieCommandFunc);
                return _LoadScanDieCommand;
            }
        }

        private void LoadScanDieCommandFunc(Object param)
        {
            try
            {
                // Check if the file exists
                if (File.Exists(FilePath))
                {
                    // Read the JSON string from the file and deserialize it
                    using (var streamReader = new StreamReader(FilePath))
                    {
                        var serializer = new JsonSerializer();
                        var loadedIndices = (List<IndexDTO>)serializer.Deserialize(streamReader, typeof(List<IndexDTO>));

                        TargetDies.Clear();

                        // Populate the TargetUserIndex properties in AccuracyInfos with the loaded data
                        for (int i = 0; i < loadedIndices.Count; i++)
                        {
                            var info = new IndexDTO();
                            info.XIndex = loadedIndices[i].XIndex;
                            info.YIndex = loadedIndices[i].YIndex;

                            TargetDies.Add(info);
                        }
                    }
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "File not found.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AccuracyCheckStartCommand;
        public ICommand AccuracyCheckStartCommand
        {
            get
            {
                if (null == _AccuracyCheckStartCommand) _AccuracyCheckStartCommand = new AsyncCommand(AccuracyCheckStartCommandFunc);
                return _AccuracyCheckStartCommand;
            }
        }

        private async Task AccuracyCheckStartCommandFunc()
        {
            try
            {
                if (TargetDies.Count == 0 || ReferenceUserIndex == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error", "Data (Die) is not exist.", EnumMessageStyle.Affirmative);
                }
                else if(ptinfo == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error", "Data (Pattern) is not exist.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    SeletedTabControlIndex = 3;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AccuracyInfos.Clear();

                        foreach (var info in TargetDies)
                        {
                            AccuracyInfo tmp = new AccuracyInfo();
                            tmp.TargetUserIndex.XIndex = info.XIndex;
                            tmp.TargetUserIndex.YIndex = info.YIndex;

                            AccuracyInfos.Add(tmp);
                        }
                    });

                    Task task = new Task(() =>
                    {
                        if (cancelToken == null)
                        {
                            cancelToken = new System.Threading.CancellationTokenSource();
                        }

                        WaferCoordinate wcoord = null;

                        double movexpos;
                        double moveypos;

                        var ref_mi = CoordinateManager.UserIndexConvertToMachineIndex(ReferenceUserIndex);

                        wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(ref_mi.XIndex, ref_mi.YIndex);
                        movexpos = wcoord.GetX() + ptinfo.X.Value;
                        moveypos = wcoord.GetY() + ptinfo.Y.Value;

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(movexpos, moveypos);

                        if (IsFocusing)
                        {
                            // (SX, SY): 중심 - 패턴 사이즈 절반 - 마진의 절반
                            // (W, H)  : 패턴 사이즈 + 마진

                            double margin_pixel = FocusROIMargin / CurCam.GetRatioX();
                            double OffsetX = (CurCam.Param.GrabSizeX.Value / 2) - ptinfo.PMParameter.PattWidth.Value - (margin_pixel / 2.0);
                            double OffsetY = (CurCam.Param.GrabSizeY.Value / 2) - ptinfo.PMParameter.PattHeight.Value - (margin_pixel / 2.0);

                            FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetX, ptinfo.PMParameter.PattWidth.Value + margin_pixel, ptinfo.PMParameter.PattHeight.Value + margin_pixel);
                            FocusModel.Focusing_Retry(FocusParam, false, true, false, this);
                        }

                        // Pattern Matching
                        PMResult pmret = this.VisionManager().PatternMatching(ptinfo, this);

                        if (pmret.RetValue == EventCodeEnum.NONE)
                        {
                            double xoffset = 0.0;
                            double yoffset = 0.0;

                            GetXYoffset(pmret, ref xoffset, ref yoffset);

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(movexpos + xoffset, moveypos + yoffset);

                            Thread.Sleep(WaitTime);

                            foreach (var info in AccuracyInfos)
                            {
                                if (cancelToken.IsCancellationRequested)
                                {
                                    cancelToken.Dispose();
                                    cancelToken = null;

                                    break;
                                }

                                var target_mi = CoordinateManager.UserIndexConvertToMachineIndex(info.TargetUserIndex);

                                this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(target_mi.XIndex, target_mi.YIndex);

                                var curpos = CurCam.GetCurCoordPos();

                                info.MoveXPos = curpos.GetX();
                                info.MoveYPos = curpos.GetY();

                                Thread.Sleep(WaitTime);

                                if (IsFocusing)
                                {
                                    FocusModel.Focusing_Retry(FocusParam, false, true, false, this);
                                }

                                // Pattern Matching
                                pmret = this.VisionManager().PatternMatching(ptinfo, this);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (pmret.RetValue == EventCodeEnum.NONE)
                                    {
                                        xoffset = 0.0;
                                        yoffset = 0.0;

                                        GetXYoffset(pmret, ref xoffset, ref yoffset);

                                        info.XOffset = xoffset.ToString("F3");
                                        info.YOffset = yoffset.ToString("F3");

                                        info.XDifference = (Convert.ToDouble(info.XOffset) - Spec_X_um).ToString("F3");
                                        info.YDifference = (Convert.ToDouble(info.YOffset) - Spec_Y_um).ToString("F3");

                                        if (Math.Abs(xoffset) <= Spec_X_um && Math.Abs(yoffset) <= Spec_Y_um)
                                        {
                                            info.IsSuccess = true;
                                        }
                                        else
                                        {
                                            info.IsSuccess = false;
                                        }
                                    }
                                    else
                                    {
                                        info.XOffset = "FAIL";
                                        info.YOffset = "FAIL";
                                        info.XDifference = "FAIL";
                                        info.YDifference = "FAIL";

                                        info.IsSuccess = false;
                                    }
                                });
                            }

                            // 초기화
                            if(IsFocusing)
                            {
                                this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam);
                            }
                        }
                        else
                        {
                            // ERROR:
                        }
                    });
                    task.Start();
                    await task;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }
        }

        private void GetXYoffset(PMResult mResult, ref double xoffset, ref double yoffset)
        {
            try
            {
                if (this.VisionManager().GetCam(mResult.ResultBuffer.CamType).GetHorizontalFlip() == FlipEnum.NONE)
                {
                    xoffset = mResult.ResultParam[0].XPoss - (mResult.ResultBuffer.SizeX / 2);
                }
                else
                {
                    xoffset = (mResult.ResultBuffer.SizeX / 2) - mResult.ResultParam[0].XPoss;
                }

                if (this.VisionManager().GetCam(mResult.ResultBuffer.CamType).GetVerticalFlip() == FlipEnum.FLIP)
                {
                    yoffset = (mResult.ResultBuffer.SizeY / 2) - mResult.ResultParam[0].YPoss;
                }
                else
                {
                    yoffset = mResult.ResultParam[0].YPoss - (mResult.ResultBuffer.SizeY / 2);
                }

                xoffset *= mResult.ResultBuffer.RatioX.Value;
                yoffset *= mResult.ResultBuffer.RatioY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _AccuracyCheckStopCommand;
        public ICommand AccuracyCheckStopCommand
        {
            get
            {
                if (null == _AccuracyCheckStopCommand) _AccuracyCheckStopCommand = new RelayCommand(AccuracyCheckStopCommandFunc);
                return _AccuracyCheckStopCommand;
            }
        }

        private void AccuracyCheckStopCommandFunc()
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

        private AsyncCommand _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(FocusingCommandFunc);
                return _FocusingCommand;
            }
        }
        public async Task FocusingCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    FocusParam.FocusingCam.Value = CurCam.GetChannelType();
                    FocusModel.Focusing_Retry(FocusParam, false, true, false, this);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    // DTO for XIndex and YIndex
    public class IndexDTO
    {
        public long XIndex { get; set; }
        public long YIndex { get; set; }
    }

    public class AccuracyInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private UserIndex _TargetUserIndex = new UserIndex();
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

        private double _MoveXPos;
        public double MoveXPos
        {
            get { return _MoveXPos; }
            set
            {
                if (value != _MoveXPos)
                {
                    _MoveXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MoveYPos;
        public double MoveYPos
        {
            get { return _MoveYPos; }
            set
            {
                if (value != _MoveYPos)
                {
                    _MoveYPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _XOffset = string.Empty;
        public string XOffset
        {
            get { return _XOffset; }
            set
            {
                if (value != _XOffset)
                {
                    _XOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _YOffset = string.Empty;
        public string YOffset
        {
            get { return _YOffset; }
            set
            {
                if (value != _YOffset)
                {
                    _YOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _XDifference = string.Empty;
        public string XDifference
        {
            get { return _XDifference; }
            set
            {
                if (value != _XDifference)
                {
                    _XDifference = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _YDifference = string.Empty;
        public string YDifference
        {
            get { return _YDifference; }
            set
            {
                if (value != _YDifference)
                {
                    _YDifference = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSuccess = false;
        public bool IsSuccess
        {
            get { return _IsSuccess; }
            set
            {
                if (value != _IsSuccess)
                {
                    _IsSuccess = value;
                    RaisePropertyChanged();

                }
            }
        }
    }
}
