using CylType;
using Focusing;
using LogModule;
using MetroDialogInterfaces;
using Microsoft.Win32;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberInterfaces.State;
using ProberInterfaces.Vision;
using ProberViewModel.Data;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using UcDisplayPort;

namespace VisionTestViewModel
{
    public class AxisObjectVM : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _RelMoveStepDist;
        public double RelMoveStepDist
        {
            get { return _RelMoveStepDist; }
            set
            {
                if (value != _RelMoveStepDist)
                {
                    _RelMoveStepDist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PosButtonVisibility = true;
        public bool PosButtonVisibility
        {
            get { return _PosButtonVisibility; }
            set
            {
                if (value != _PosButtonVisibility)
                {
                    _PosButtonVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _NegButtonVisibility = true;
        public bool NegButtonVisibility
        {
            get { return _NegButtonVisibility; }
            set
            {
                if (value != _NegButtonVisibility)
                {
                    _NegButtonVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _AxisObject;
        public ProbeAxisObject AxisObject
        {
            get { return _AxisObject; }
            set
            {
                if (value != _AxisObject)
                {
                    _AxisObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _PosRelMoveCommand;
        public ICommand PosRelMoveCommand
        {
            get
            {
                if (null == _PosRelMoveCommand) _PosRelMoveCommand = new AsyncCommand(PosRelMove);
                return _PosRelMoveCommand;
            }
        }
        private async Task PosRelMove()
        {
            try
            {
                await Task.Run(() =>
                {
                    double apos = 0;
                    this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos);
                    double pos = Math.Abs(RelMoveStepDist);
                    if (pos + apos < AxisObject.Param.PosSWLimit.Value)
                    {
                        NegButtonVisibility = false;
                        this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                    }
                    else
                    {
                        //Sw limit
                    }
                });

                NegButtonVisibility = true;
            }
            catch (Exception ex)
            {
                NegButtonVisibility = true;
                //throw;
            }
        }

        private AsyncCommand _NegRelMoveCommand;
        public ICommand NegRelMoveCommand
        {
            get
            {
                if (null == _NegRelMoveCommand) _NegRelMoveCommand = new AsyncCommand(NegRelMove);
                return _NegRelMoveCommand;
            }
        }
        private async Task NegRelMove()
        {
            try
            {
                await Task.Run(() =>
                {
                    double apos = 0;
                    this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos);
                    double pos = Math.Abs(RelMoveStepDist) * -1;
                    if (pos + apos > AxisObject.Param.NegSWLimit.Value)
                    {
                        PosButtonVisibility = false;
                        this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                    }
                    else
                    {
                        //Sw Limit
                    }
                });
                PosButtonVisibility = true;
            }
            catch (Exception err)
            {
                PosButtonVisibility = true;
                // throw;
            }

        }

        private AsyncCommand _StopMoveCommand;
        public ICommand StopMoveCommand
        {
            get
            {
                if (null == _StopMoveCommand) _StopMoveCommand = new AsyncCommand(StopMove);
                return _StopMoveCommand;
            }
        }
        private async Task StopMove()
        {
            try
            {
                await Task.Run(() =>
                {
                    this.MotionManager().Stop(AxisObject);
                });
            }
            catch (Exception)
            {
                throw;
            }

        }
    }

    public class VisionTestViewModelBase : IMainScreenViewModel, INotifyPropertyChanged, ISetUpState, IUseLightJog
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; } = false;

        readonly Guid _ViewModelGUID = new Guid("01613590-0FAD-CE9F-9A79-F13B71BA2A05");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private ObservableCollection<AxisObjectVM> _StageAxisObjectVmList
            = new ObservableCollection<AxisObjectVM>();
        public ObservableCollection<AxisObjectVM> StageAxisObjectVmList
        {
            get { return _StageAxisObjectVmList; }
            set
            {
                if (value != _StageAxisObjectVmList)
                {
                    _StageAxisObjectVmList = value;
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

        private IFocusing _FocusingModule;
        public IFocusing FocusingModule
        {
            get
            {
                if (_FocusingModule == null)
                    _FocusingModule = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());

                return _FocusingModule;
            }
        }

        private FocusParameter FocusingParam { get; set; }

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
        private bool _TestRepeat;
        public bool TestRepeat
        {
            get { return _TestRepeat; }
            set
            {
                if (value != _TestRepeat)
                {
                    _TestRepeat = value;
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    FocusingParam = new NormalFocusParameter();
                    FocusingParam.SetDefaultParam();
                    FocusingParam.FocusRange.Value = 3000;

                    //FocusingParam.FocusingAxis.Value = EnumAxisConstants.Z;

                    VisionManager = this.VisionManager();
                    MotionManager = this.MotionManager();
                    StageSupervisor = this.StageSupervisor();

                    if (this.MotionManager() != null)
                    {
                        StageAxes aes = this.MotionManager().StageAxes;
                        StageAxisObjectVmList = new ObservableCollection<AxisObjectVM>();

                        foreach (var item in aes.ProbeAxisProviders)
                        {
                            if (item.AxisType.Value == EnumAxisConstants.R || item.AxisType.Value == EnumAxisConstants.TT ||
                                item.AxisType.Value == EnumAxisConstants.Z0 || item.AxisType.Value == EnumAxisConstants.Z1 ||
                                item.AxisType.Value == EnumAxisConstants.Z2)
                            {

                                var axisObjVM = new AxisObjectVM();
                                axisObjVM.AxisObject = item;
                                axisObjVM.NegButtonVisibility = false;
                                axisObjVM.PosButtonVisibility = false;

                                StageAxisObjectVmList.Add(axisObjVM);
                            }
                            else
                            {
                                var axisObjVM = new AxisObjectVM();
                                axisObjVM.AxisObject = item;

                                StageAxisObjectVmList.Add(axisObjVM);
                            }
                        }
                    }

                    StageCamList = new ObservableCollection<StageCamera>();
                    
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferHigh));
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferLow));
                    StageCamList.Add(new StageCamera(enumStageCamType.PinHigh));
                    StageCamList.Add(new StageCamera(enumStageCamType.PinLow));
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferHighNC));
                    StageCamList.Add(new StageCamera(enumStageCamType.MAP_REF));
                    StageCamList.Add(new StageCamera(enumStageCamType.UNDEFINED));

                    PosList = new List<CatCoordinates>();

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
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            Task<EventCodeEnum> task = null;

            try
            {
                task = Task.Run(() =>
                {
                    return EventCodeEnum.NONE;
                });
                task.Wait();


                DisplayPort = new DisplayPort() { GUID = new Guid("9ACC1870-87E2-4A6E-9F3B-5CF8D55C09D0") };
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
                    Path = new System.Windows.PropertyPath("Cam"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);

                LightJog = new LightJogViewModel(
                       maxLightValue: 255,
                       minLightValue: 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return task;
        }
        private bool _ZUPLampChecked;
        public bool ZUPLampChecked
        {
            get { return _ZUPLampChecked; }
            set
            {
                if (value != _ZUPLampChecked)
                {
                    _ZUPLampChecked = value;
                    RaisePropertyChanged();
                }
            }
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            //this.SysState().SetSetUpState();
            this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
            ZUPLampChecked = this.IOManager().IO.Outputs.DOZUPLAMPON.Value;
            Radius = this.CoordinateManager().StageCoord.PCD.Value;
            LightJog = (LightJogViewModel)this.PnPManager().PnpLightJog;
            LightJog.InitCameraJog(this);

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        #region Property
        private double _MinZValue;
        public double MinZValue
        {
            get { return _MinZValue; }
            set
            {
                if (value != _MinZValue)
                {
                    _MinZValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MaxZValue;
        public double MaxZValue
        {
            get { return _MaxZValue; }
            set
            {
                if (value != _MaxZValue)
                {
                    _MaxZValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MinDegree;
        public int MinDegree
        {
            get { return _MinDegree; }
            set
            {
                if (value != _MinDegree)
                {
                    _MinDegree = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _MaxDegree;
        public int MaxDegree
        {
            get { return _MaxDegree; }
            set
            {
                if (value != _MaxDegree)
                {
                    _MaxDegree = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _MeasuermentXPos;
        public double MeasuermentXPos
        {
            get { return _MeasuermentXPos; }
            set
            {
                if (value != _MeasuermentXPos)
                {
                    _MeasuermentXPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _MeasuermentYPos;
        public double MeasuermentYPos
        {
            get { return _MeasuermentYPos; }
            set
            {
                if (value != _MeasuermentYPos)
                {
                    _MeasuermentYPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Sensor1Pos;
        public double Sensor1Pos
        {
            get { return _Sensor1Pos; }
            set
            {
                if (value != _Sensor1Pos)
                {
                    _Sensor1Pos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Sensor2Pos;
        public double Sensor2Pos
        {
            get { return _Sensor2Pos; }
            set
            {
                if (value != _Sensor2Pos)
                {
                    _Sensor2Pos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Sensor3Pos;
        public double Sensor3Pos
        {
            get { return _Sensor3Pos; }
            set
            {
                if (value != _Sensor3Pos)
                {
                    _Sensor3Pos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ShfitRValue;
        public int ShfitRValue
        {
            get { return _ShfitRValue; }
            set
            {
                if (value != _ShfitRValue)
                {
                    _ShfitRValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StageButtonsVisibility = true;
        public bool StageButtonsVisibility
        {
            get { return _StageButtonsVisibility; }
            set
            {
                if (value != _StageButtonsVisibility)
                {
                    _StageButtonsVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnableTiltElement;
        public bool EnableTiltElement
        {
            get { return _EnableTiltElement; }
            set
            {
                if (value != _EnableTiltElement)
                {
                    _EnableTiltElement = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RPosDist;
        public int RPosDist
        {
            get { return _RPosDist; }
            set
            {
                if (value != _RPosDist)
                {
                    _RPosDist = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _TTPosDist;
        public double TTPosDist
        {
            get { return _TTPosDist; }
            set
            {
                if (value != _TTPosDist)
                {
                    _TTPosDist = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _TiltCommand;
        public bool TiltCommand
        {
            get { return _TiltCommand; }
            set
            {
                if (value != _TiltCommand)
                {
                    _TiltCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<CatCoordinates> _PosList;

        public List<CatCoordinates> PosList
        {
            get { return _PosList; }
            set { _PosList = value; }
        }

        private double _XValue;
        public double XValue
        {
            get { return _XValue; }
            set
            {
                if (value != _XValue)
                {
                    _XValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YValue;
        public double YValue
        {
            get { return _YValue; }
            set
            {
                if (value != _YValue)
                {
                    _YValue = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ICamera _Cam;
        public ICamera Cam
        {
            get { return _Cam; }
            set
            {
                if (value != _Cam)
                {
                    _Cam = value;
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



        #endregion


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



        #region Command

        private RelayCommand<object> _LoadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (null == _LoadCommand) _LoadCommand = new RelayCommand<object>(Load);
                return _LoadCommand;
            }
        }
        private void Load(object noparam)
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

                    Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    this.VisionManager().StartGrab(Cam.GetChannelType(), this);

                    this.VisionManager().DigitizerService[Cam.GetDigitizerIndex()].GrabberService.LoadUserImageFiles(imgs);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _ImgProcessing;
        public ICommand ImgProcessing
        {
            get
            {
                if (null == _ImgProcessing) _ImgProcessing = new RelayCommand<object>(Processing);
                return _ImgProcessing;
            }
        }
        private void Processing(object noparam)
        {
            try
            {
                this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM, this);
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
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                        break;
                    default:
                        break;
                }

                this.VisionManager().StartGrab(curcam, this);

                LightJog.InitCameraJog(this, curcam);
                CurCam = this.VisionManager().GetCam(curcam);
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
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                        break;
                    default:
                        break;
                }

                this.VisionManager().StopGrab(curcam);
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
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                        break;
                    default:
                        break;
                }

                this.VisionManager().SingleGrab(curcam, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _ConnectCameraCommand;
        public ICommand ConnectCameraCommand
        {
            get
            {
                if (null == _ConnectCameraCommand) _ConnectCameraCommand = new RelayCommand<object>(ConnectCameraCommandFunc);
                return _ConnectCameraCommand;
            }
        }
        private void ConnectCameraCommandFunc(object noparam)
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
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                        break;
                    default:
                        break;
                }

                this.VisionManager().DeAllocateCamera(curcam);
                this.VisionManager().AllocateCamera(curcam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SaveGrabimgCommand;
        public ICommand SaveGrabimgCommand
        {
            get
            {
                if (null == _SaveGrabimgCommand) _SaveGrabimgCommand = new AsyncCommand(SaveGrabimgFunc);
                return _SaveGrabimgCommand;
            }
        }
        private async Task<EventCodeEnum> SaveGrabimgFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
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
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        break;
                    case enumStageCamType.WaferLow:
                        curcam = EnumProberCam.WAFER_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        break;
                    case enumStageCamType.PinHigh:
                        curcam = EnumProberCam.PIN_HIGH_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                        break;
                    case enumStageCamType.PinLow:
                        curcam = EnumProberCam.PIN_LOW_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                        break;
                    case enumStageCamType.MAP_REF:
                        curcam = EnumProberCam.MAP_REF_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.MAP_REF_CAM);
                        break;
                    default:
                        break;
                }

                if (curcam != EnumProberCam.UNDEFINED)
                {
                    bool signaled = false;
                    ImageBuffer image = new ImageBuffer();

                    try
                    {
                        image = this.VisionManager().SingleGrab(Cam.GetChannelType(), this);

                        signaled = this.VisionManager().DigitizerService[Cam.GetDigitizerIndex()].GrabberService.WaitOne(60000);
                        var roi = new System.Windows.Rect(0, 0, 960, 960);
                        int focusval = this.VisionManager().GetFocusValue(image, roi);
                        image.FocusLevelValue = focusval;

                        // Save
                        string SaveBasePath = $"C:\\Logs\\Image\\{curcam.ToString()}\\{curcam.ToString()}_{image.CapturedTime.ToString("yyMMddHHmmss")}.bmp";
                        this.VisionManager().SaveImageBuffer(image, SaveBasePath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                        LoggerManager.Debug($"[{curcam.ToString()}], SaveGrabimgFunc() : {SaveBasePath}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    
                    this.VisionManager().StartGrab(curcam, this);

                    LightJog.InitCameraJog(this, curcam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        private RelayCommand<object> _TiltingTestCommand;
        public ICommand TiltingTestCommand
        {
            get
            {
                if (null == _TiltingTestCommand) _TiltingTestCommand = new RelayCommand<object>(TiltingTest);
                return _TiltingTestCommand;
            }
        }
        private void TiltingTest(object noparam)
        {
            //Stopwatch stw = new Stopwatch();
            //stw.Start();
            //bool run = true;
            //while (run)
            //{
            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(0, -100);
            //    System.Threading.Thread.Sleep(2000);
            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(0, 100);
            //    System.Threading.Thread.Sleep(2000);


            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(90, -100);
            //    System.Threading.Thread.Sleep(2000);
            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(90, 100);
            //    System.Threading.Thread.Sleep(2000);


            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(180, -100);
            //    System.Threading.Thread.Sleep(2000);
            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(180, 100);
            //    System.Threading.Thread.Sleep(2000);


            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(270, -100);
            //    System.Threading.Thread.Sleep(2000);
            //    this.StageSupervisor().StageModuleState.ChuckTiltMove(270, 100);
            //    System.Threading.Thread.Sleep(2000);

            //    if (stw.Elapsed.Minutes > 60)
            //    {
            //        run = false;
            //    }

            //}
            focusingLoopEnable = false;
        }

        private AsyncCommand _MarkAlignCommand;
        public ICommand MarkAlignCommand
        {
            get
            {
                if (null == _MarkAlignCommand) _MarkAlignCommand = new AsyncCommand(DoMarkAlgin);
                return _MarkAlignCommand;
            }
        }

        private async Task<EventCodeEnum> DoMarkAlgin()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor.StageModuleState.ZCLEARED();
                this.StageSupervisor.StageModuleState.SetWaferCamBasePos(true);

                while (TestRepeat)
                {
                    ret = await Task.Run(() => this.MarkAligner().DoMarkAlign());
                }

                this.StageSupervisor.StageModuleState.SetWaferCamBasePos(false);
                this.StageSupervisor.StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        private AsyncCommand _WaferTestCommand;
        public ICommand WaferTestCommand
        {
            get
            {
                if (null == _WaferTestCommand) _WaferTestCommand = new AsyncCommand(WaferAlignTestCommandFunc);
                return _WaferTestCommand;
            }
        }

        private async Task<EventCodeEnum> WaferAlignTestCommandFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                bool isError = false;
                int totalCnt = 0;
                int maxCnt = 3000;

                while (TestRepeat || (totalCnt > maxCnt))
                {
                    ret = this.PinAligner().DoManualOperation();

                    if (ret != EventCodeEnum.NONE)
                    {
                        TestRepeat = false;
                        isError = true;
                        await this.MetroDialogManager().ShowMessageDialog("PinPadMatch Test PinAlign Fail", $"Test TotalCount: {totalCnt}", EnumMessageStyle.Affirmative);
                        return ret;
                    }

                    ret = this.WaferAligner().DoManualOperation();

                    if (ret != EventCodeEnum.NONE)
                    {
                        TestRepeat = false;
                        isError = true;
                        await this.MetroDialogManager().ShowMessageDialog("PinPadMatch Test WaferAlign Fail", $"Test TotalCount: {totalCnt}", EnumMessageStyle.Affirmative);
                        return ret;
                    }
                    int retVal = StageCylinderType.MoveWaferCam.Retract();
                    if (retVal != 0)
                    {
                        TestRepeat = false;
                        isError = true;
                        return ret;
                    }


                    var wafer_object = (WaferObject)this.StageSupervisor().WaferObject;
                    WaferCoordinate wafercoord = new WaferCoordinate();
                    PinCoordinate pincoord = new PinCoordinate();
                    //Wafer Center로 갈건지 아니면 PadCenter로 갈건지 계산해야됨.
                    wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                    wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                    MachineIndex MI = new MachineIndex();
                    try
                    {
                        ret = this.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);

                        if (ret == EventCodeEnum.NONE)
                        {
                            var Wafer = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);                            
                            wafercoord.X.Value = Wafer.X.Value;
                            wafercoord.Y.Value = Wafer.Y.Value;
                            wafercoord.T.Value = Wafer.T.Value;
                            LoggerManager.Debug($"[Test] Used GetFirstSequence Position");
                        }
                        else
                        {
                            wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                            wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                            LoggerManager.Debug($"[Test] Used WaferCenter Position");
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"[SoakingModule] Probing GetFirstSequence() Error. Used WaferCenter Position");
                        wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                        wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                    }


                    wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;

                    pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                    pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                    pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                    LoggerManager.Debug($"[Test PinPad] PinPadPosition(), wafercoord(X, Y ,Z) = { wafercoord.X.Value}, {wafercoord.Y.Value}, {wafercoord.Z.Value}, ");
                    LoggerManager.Debug($"[Test PinPad] PinPadPosition(), pincoord(X, Y ,Z) = { pincoord.X.Value}, {pincoord.Y.Value}, {pincoord.Z.Value}");
                    var zclearance = -10000;

                    LoggerManager.Debug($"[Test PinPad] zclearance= { zclearance}");


                    ret = this.StageSupervisor().StageModuleState.MoveToSoaking(wafercoord, pincoord, zclearance);

                    if (ret != EventCodeEnum.NONE)
                    {
                        TestRepeat = false;
                        isError = true;
                        await this.MetroDialogManager().ShowMessageDialog("PinPadMatch Test MoveToPinPad Fail", $"Test TotalCount: {totalCnt}", EnumMessageStyle.Affirmative);
                        return ret;
                    }


                    ret = this.StageSupervisor().StageModuleState.ZCLEARED();

                    if (ret != EventCodeEnum.NONE)
                    {
                        TestRepeat = false;
                        isError = true;
                        await this.MetroDialogManager().ShowMessageDialog("PinPadMatch Test ZCLEARED Fail", $"Test TotalCount: {totalCnt}", EnumMessageStyle.Affirmative);
                        return ret;
                    }


                    System.Threading.Thread.Sleep(10000);

                    totalCnt++;
                }
                if (!isError)
                {
                    await this.MetroDialogManager().ShowMessageDialog("WaferAlign Test Success", $"Test TotalCount: {totalCnt}", EnumMessageStyle.Affirmative);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private void MarkAlignCmdFunc(object noparam)
        {
            try
            {
                this.StageSupervisor.StageModuleState.ZCLEARED();
                this.StageSupervisor.StageModuleState.SetWaferCamBasePos(true);

                while (TestRepeat)
                {
                    this.MarkAligner().DoMarkAlign();
                }

                this.StageSupervisor.StageModuleState.SetWaferCamBasePos(false);
                this.StageSupervisor.StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _MarkAlignCountMoveCommand;
        public ICommand MarkAlignCountMoveCommand
        {
            get
            {
                if (null == _MarkAlignCountMoveCommand) _MarkAlignCountMoveCommand = new RelayCommand<object>(MarkAlignCountMoveCmdFunc);
                return _MarkAlignCountMoveCommand;
            }
        }
        private void MarkAlignCountMoveCmdFunc(object noparam)
        {
        }


        private AsyncCommand _SetCenterCommand;
        public ICommand SetCenterCommand
        {
            get
            {
                if (null == _SetCenterCommand) _SetCenterCommand = new AsyncCommand(SetCenterCommandFunc);
                return _SetCenterCommand;
            }
        }
        private async Task SetCenterCommandFunc()
        {
            double positionLimit = 2500;
            try
            {
                if (CurCam != null)
                {
                    WaferCoordinate coordinate = new WaferCoordinate();
                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    {
                        coordinate = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                    }
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        coordinate = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    }
                    else
                    {
                        var mret = this.MetroDialogManager().ShowMessageDialog
                            ("Error Message", $"Invalid camera type. Cur. cam = {CurCam.GetChannelType()}", EnumMessageStyle.Affirmative).Result;
                        return;
                    }

                    if (Math.Abs(coordinate.X.Value) < positionLimit & Math.Abs(coordinate.Y.Value) < positionLimit)
                    {

                        var mret = await this.MetroDialogManager().ShowMessageDialog("Success Message", $"Set Center Result:\nX : {coordinate.X.Value:0.00}, Y: {coordinate.Y.Value:0.00}", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");
                        if (mret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            string temp = "";
                            temp = coordinate.X.Value.ToString("N2");
                            this.CoordinateManager().StageCoord.ChuckCenterX.Value = double.Parse(temp);

                            temp = coordinate.Y.Value.ToString("N2");
                            this.CoordinateManager().StageCoord.ChuckCenterY.Value = double.Parse(temp);
                            this.SaveParameter(this.CoordinateManager().StageCoord);
                            LoggerManager.Debug($"Save Parameter");
                        }
                        else
                        {
                            LoggerManager.Debug($"Cancel");
                        }
                    }
                    else
                    {
                        var mret = await this.MetroDialogManager().ShowMessageDialog("Fail Message", $"The Center point is out of range.(+- 2.5mm) \nSet Center Result X : {coordinate.X.Value:0.00}, Y: {coordinate.Y.Value:0.00}", EnumMessageStyle.Affirmative);
                    }
                    LoggerManager.Debug($"Set Center Result X : {coordinate.GetX():0.00}, Y: {coordinate.GetY():0.00}");
                }
                else
                {
                    LoggerManager.Debug($"Curcam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region Move
        private AsyncCommand _CCLockRotLockCommand;
        public ICommand CCLockRotLockCommand
        {
            get
            {
                if (null == _CCLockRotLockCommand) _CCLockRotLockCommand = new AsyncCommand(CCRotLock);
                return _CCLockRotLockCommand;
            }
        }

        private async Task CCRotLock()
        {
            int ret = -1;
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    try
                    {

                        if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                        {
                            this.StageSupervisor().StageModuleState.CCRotLock(60000);
                        }
                        else
                        {
                            this.StageSupervisor().StageModuleState.ZCLEARED();// stop soaking thread
                            this.StageSupervisor().StageModuleState.LockCCState(); // change to ccstate

                            ret = StageCylinderType.MoveWaferCam.Retract();
                            if (ret != 0)
                            {
                                //ERrror
                            }
                            else // ret == 0
                            {
                                this.StageSupervisor().StageModuleState.CCRotLock(60000);
                            }
                        }

                    }
                    catch (Exception taskerr)
                    {
                        // 그대로 CCState 상태. 
                        LoggerManager.Debug(taskerr.Message);
                    }
                    finally
                    {
                        // 그대로 CCNoWaferCamState 상태. 
                        StageButtonsVisibility = true;
                    }


                });


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _CCLockRotUnLockCommand;
        public ICommand CCLockRotUnLockCommand
        {
            get
            {
                if (null == _CCLockRotUnLockCommand) _CCLockRotUnLockCommand = new AsyncCommand(CCRotUnLock);
                return _CCLockRotUnLockCommand;
            }
        }

        private async Task CCRotUnLock()
        {
            int ret = -1;
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    try
                    {
                        if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                        {
                            this.StageSupervisor().StageModuleState.LockCCState(); // change to ccstate

                            this.StageSupervisor().StageModuleState.CCRotUnLock(60000);
                        }
                        else
                        {
                            ret = StageCylinderType.MoveWaferCam.Retract();
                            if (ret != 0)
                            {
                                //ERrror
                            }
                            else // ret == 0
                            {
                                this.StageSupervisor().StageModuleState.CCRotUnLock(60000);
                            }
                        }
                        StageButtonsVisibility = true;
                    }
                    catch (Exception taskerr)
                    {
                        LoggerManager.Debug(taskerr.Message);
                    }


                });


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _BernoulliTestCommand;
        public ICommand BernoulliTestCommand
        {
            get
            {
                if (null == _BernoulliTestCommand) _BernoulliTestCommand = new AsyncCommand(BernoulliTest);
                return _BernoulliTestCommand;
            }
        }
        bool bernoulliTestRun = false;
        private async Task BernoulliTest()
        {
            int ret = -1;
            try
            {
                if (bernoulliTestRun == true)
                {
                    StageButtonsVisibility = false;
                    bernoulliTestRun = false;
                }
                else
                {
                    bernoulliTestRun = true;
                    Task.Run(() =>
                    {
                        EventCodeEnum result = EventCodeEnum.UNDEFINED;
                        int runCount = 0;
                        try
                        {
                            while (bernoulliTestRun == true)
                            {
                                if (bernoulliTestRun == false)
                                {
                                    StageButtonsVisibility = false;
                                }
                                result = this.StageSupervisor().StageModuleState.Handlerhold(10000);
                                if (result != EventCodeEnum.NONE)
                                {
                                    bernoulliTestRun = false;
                                }
                                else
                                {
                                    if (bernoulliTestRun == false)
                                    {
                                        StageButtonsVisibility = false;
                                    }
                                    result = this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                    if (result != EventCodeEnum.NONE)
                                    {
                                        bernoulliTestRun = false;
                                    }
                                }
                                runCount++;
                                LoggerManager.Debug($"Bernoulli Test Run. Count = {runCount}");
                            }
                            StageButtonsVisibility = true;
                        }
                        catch (Exception taskerr)
                        {
                            StageButtonsVisibility = true;
                            bernoulliTestRun = false;
                            LoggerManager.Debug(taskerr.Message);
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _MoveToBackCommand;
        public ICommand MoveToBackCommand
        {
            get
            {
                if (null == _MoveToBackCommand) _MoveToBackCommand = new AsyncCommand(MoveToBack);
                return _MoveToBackCommand;
            }
        }
        private async Task MoveToBack()
        {
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value - 1000, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _MoveToCenterCommand;
        public ICommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new AsyncCommand(MoveToCenter);
                return _MoveToCenterCommand;
            }
        }
        private async Task MoveToCenter()
        {
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                    this.MotionManager().StageMove(0, 0, zaxis.Param.ClearedPosition.Value, 0);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _MoveToFrontCommand;
        public ICommand MoveToFrontCommand
        {
            get
            {
                if (null == _MoveToFrontCommand) _MoveToFrontCommand = new AsyncCommand(MoveToFront);
                return _MoveToFrontCommand;
            }
        }
        private async Task MoveToFront()
        {
            try
            {
                StageButtonsVisibility = false;

                await Task.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    this.StageSupervisor().StageModuleState.ZCLEARED();


                    this.MotionManager().StageMove(0, yaxis.Param.NegSWLimit.Value + 1000, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _MoveToLoadPosCommand;
        public ICommand MoveToLoadPosCommand
        {
            get
            {
                if (null == _MoveToLoadPosCommand) _MoveToLoadPosCommand = new AsyncCommand(MoveToLoadPos);
                return _MoveToLoadPosCommand;
            }
        }
        private async Task MoveToLoadPos()
        {
            try
            {
                double offsetvalue = 0;
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.MoveLoadingPosition(offsetvalue);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _AxisZHomingCommand;
        public ICommand AxisZHomingCommand
        {
            get
            {
                if (null == _AxisZHomingCommand) _AxisZHomingCommand = new AsyncCommand(AxisZHoming);
                return _AxisZHomingCommand;
            }
        }
        private async Task AxisZHoming()
        {
            try
            {
                StageButtonsVisibility = false;
                int ret = -1;

                await Task.Run(() =>
                {
                    if (ret == 0)
                    {
                        this.MotionManager().HomingTaskRun(EnumAxisConstants.Z);
                    }
                });

                StageButtonsVisibility = true;

            }
            catch (Exception ex)
            {
                StageButtonsVisibility = true;
                LoggerManager.Error(ex.Message);

            }
        }
        private AsyncCommand _LoaderMachineInitCommand;
        public ICommand LoaderMachineInitCommand
        {
            get
            {
                if (null == _LoaderMachineInitCommand) _LoaderMachineInitCommand = new AsyncCommand(LoaderInit);
                return _LoaderMachineInitCommand;
            }
        }
        private async Task LoaderInit()
        {
            try
            {
                StageButtonsVisibility = false;

                await Task.Run(() =>
                {
                    this.LoaderController().LoaderSystemInit();
                });

                StageButtonsVisibility = true;

            }
            catch (Exception ex)
            {
                StageButtonsVisibility = true;
                LoggerManager.Error(ex.Message);

            }
        }

        private AsyncCommand _StageMachineInitCommand;
        public ICommand StageMachineInitCommand
        {
            get
            {
                if (null == _StageMachineInitCommand) _StageMachineInitCommand = new AsyncCommand(StageInit);
                return _StageMachineInitCommand;
            }
        }
        private async Task StageInit()
        {
            try
            {
                StageButtonsVisibility = false;
                int ret = -1;
                await Task.Run(() =>
                {
                    ret = this.MotionManager().ForcedZDown();
                });


                await Task.Run(() =>
                {
                    if (ret == 0)
                    {
                        this.MotionManager().StageSystemInit();
                    }
                });

                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _ZAxisHomingCommand;
        public ICommand ZAxisHomingCommand
        {
            get
            {
                if (null == _ZAxisHomingCommand) _ZAxisHomingCommand = new AsyncCommand(ZAxisHoming);
                return _ZAxisHomingCommand;
            }
        }
        private async Task ZAxisHoming()
        {
            try
            {
                StageButtonsVisibility = false;

                await Task.Run(() =>
                {
                    this.MotionManager().HomingTaskRun(EnumAxisConstants.Z);
                });

                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _CamMoveCommand;
        public ICommand CamMoveCommand
        {
            get
            {
                if (null == _CamMoveCommand) _CamMoveCommand = new AsyncCommand(CamMove);
                return _CamMoveCommand;
            }
        }
        private async Task CamMove()
        {
            //double Thickness = this.StageSupervisor().WaferObject.PhysInfoGetter.Thickness.Value;
            double Thickness = 0;
            //double pinHeight = this.StageSupervisor().ProbeCardInfo.PinDefaultHeight.Value;
            double pinHeight = -10000;
            try
            {
                StageButtonsVisibility = false;
                this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 0);

                await Task.Run(() =>
                {
                    switch (SelectedCam)
                    {
                        case enumStageCamType.UNDEFINED:
                            break;
                        case enumStageCamType.WaferHigh:
                            this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 96);
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, Thickness);
                            break;
                        case enumStageCamType.WaferLow:
                            this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).SetLight(EnumLightType.COAXIAL, 96);
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Thickness);
                            break;
                        case enumStageCamType.PinHigh:
                            this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 50);
                            this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, pinHeight);
                            break;
                        case enumStageCamType.PinLow:
                            this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM).SetLight(EnumLightType.COAXIAL, 50);
                            this.StageSupervisor().StageModuleState.PinLowViewMove(0, 0, pinHeight);
                            break;
                        case enumStageCamType.WaferHighNC:
                            this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 96);
                            NCCoordinate nccoord = new NCCoordinate(0, 0, 0);
                            this.StageSupervisor().StageModuleState.WaferHighCamCoordMoveNCpad(nccoord, 0);
                            break;
                        default:
                            break;
                    }

                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _MoveToMarkCommand;
        public ICommand MoveToMarkCommand
        {
            get
            {
                if (null == _MoveToMarkCommand) _MoveToMarkCommand = new AsyncCommand(MoveToMark);
                return _MoveToMarkCommand;
            }
        }
        private async Task MoveToMark()
        {
            try
            {
                StageButtonsVisibility = false;

                //List < LightChannelType >  lights = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).LightsChannels;
                var phRefLight = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 255);
                this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 0);

                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 0);
                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.OBLIQUE, 150);

                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.MoveToMark();
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TiltMoveCommand;
        public ICommand TiltMoveCommand
        {
            get
            {
                if (null == _TiltMoveCommand) _TiltMoveCommand = new AsyncCommand(ChuckTiltMove);
                return _TiltMoveCommand;
            }
        }
        private async Task ChuckTiltMove()
        {
            try
            {

                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.ChuckTiltMove(RPosDist, TTPosDist);
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AutoTiltCommand;
        public ICommand AutoTiltCommand
        {
            get
            {
                if (null == _AutoTiltCommand) _AutoTiltCommand = new AsyncCommand(AutoTilt);
                return _AutoTiltCommand;
            }
        }
        private async Task AutoTilt()
        {
            try
            {
                TiltCommand = true;
                int cnt = 0;
                double rposition = 0;

                await Task.Run(() =>
                {
                    while (TiltCommand)
                    {
                        if (cnt == 0)
                        {
                            rposition = 0;
                        }
                        else if (cnt == 1)
                        {
                            rposition = 45;
                        }
                        else if (cnt == 2)
                        {
                            rposition = 90;
                        }
                        else if (cnt == 3)
                        {
                            rposition = 135;
                        }
                        this.StageSupervisor().StageModuleState.ChuckTiltMove(rposition, TTPosDist);
                        Thread.Sleep(500);
                        this.StageSupervisor().StageModuleState.ChuckTiltMove(rposition + 180, TTPosDist);
                        Thread.Sleep(500);

                        cnt++;
                        if (cnt == 4)
                        {
                            cnt = 0;
                        }
                    }
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AutoTiltStopCommand;
        public ICommand AutoTiltStopCommand
        {
            get
            {
                if (null == _AutoTiltStopCommand) _AutoTiltStopCommand = new AsyncCommand(AutoTiltStop);
                return _AutoTiltStopCommand;
            }
        }
        private async Task AutoTiltStop()
        {
            try
            {
                await Task.Run(() =>
                {
                    TiltCommand = false;
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SensorSetZeroCommand;
        public ICommand SensorSetZeroCommand
        {
            get
            {
                if (_SensorSetZeroCommand == null) _SensorSetZeroCommand = new RelayCommand<object>(SensorSetZero);
                return _SensorSetZeroCommand;
            }
        }

        private void SensorSetZero(object noparam)
        {
            try
            {
                this.MotionManager().SetLoadCellZero();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DualLoopOnCommand;
        public ICommand DualLoopOnCommand
        {
            get
            {
                if (_DualLoopOnCommand == null) _DualLoopOnCommand = new RelayCommand<object>(DualLoopOn);
                return _DualLoopOnCommand;
            }
        }

        private void DualLoopOn(object noparam)
        {
            try
            {
                this.MotionManager().SetDualLoop(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand<object> _DualLoopOffCommand;
        public ICommand DualLoopOffCommand
        {
            get
            {
                if (_DualLoopOffCommand == null) _DualLoopOffCommand = new RelayCommand<object>(DualLoopOff);
                return _DualLoopOffCommand;
            }
        }

        private void DualLoopOff(object noparam)
        {
            try
            {
                this.MotionManager().SetDualLoop(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _WaferMoveMiddleCommand;
        public ICommand WaferMoveMiddleCommand
        {
            get
            {
                if (null == _WaferMoveMiddleCommand) _WaferMoveMiddleCommand = new AsyncCommand(WaferMoveMiddle);
                return _WaferMoveMiddleCommand;
            }
        }
        private async Task WaferMoveMiddle()
        {
            int ret = -1;

            try
            {
                double axisZsafeOffset = 15000; //마크를 봤을때 핀하이, 웨이퍼하이의 거리는 35.5mm이다 마크 보는 포지션에서 척은 pz보다 20mm높다. 
                double axisPZsafeOffset = 35000;
                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                var markRefposZ = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;
                if (axisz.Status.RawPosition.Actual > markRefposZ + axisZsafeOffset)
                {
                    this.StageSupervisor.StageModuleState.ZCLEARED();
                }
                if (axispz.Status.RawPosition.Actual > markRefposZ + axisPZsafeOffset)
                {
                    this.StageSupervisor.StageModuleState.ZCLEARED();
                }
                ret = StageCylinderType.MoveWaferCam.Extend();
                if (ret != 0)
                {
                    //ERrror
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _WaferMoveRearCommand;
        public ICommand WaferMoveRearCommand
        {
            get
            {
                if (null == _WaferMoveRearCommand) _WaferMoveRearCommand = new AsyncCommand(WaferMoveRear);
                return _WaferMoveRearCommand;
            }
        }
        private async Task WaferMoveRear()
        {
            int ret = -1;

            try
            {
                ret = StageCylinderType.MoveWaferCam.Retract();
                if (ret != 0)
                {
                    //ERrror
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MeasurementChuckPlainCommand;
        public ICommand MeasurementChuckPlainCommand
        {
            get
            {
                if (null == _MeasurementChuckPlainCommand) _MeasurementChuckPlainCommand = new AsyncCommand(Test);
                return _MeasurementChuckPlainCommand;
            }
        }

        private async Task Test()
        {

            //double centerx;
            //double centery;
            //this.MotionManager().GetActualPos(EnumAxisConstants.X, out centerx);
            //this.MotionManager().GetActualPos(EnumAxisConstants.Y, out centery);
            //double dx = 0;
            //double dy = 0;
            //double posx;
            //double posy;
            //double zpos = 0;
            //this.StageSupervisor().StageModuleState.WaferHighViewMove(0,0, zpos);

            //for (int i = 0; i <= 36; i++)
            //{
            //    dx = 150000 * Math.Cos(Math.PI * (i * 10) / 180);
            //    dy = 150000 * Math.Sin(Math.PI * (i * 10) / 180);
            //    posx = centerx + (dx * -1);
            //    posy = centery + dy;
            //    MachineCoordinate mccoord = new MachineCoordinate();
            //    WaferCoordinate wafercoord = new WaferCoordinate();

            //    mccoord.X.Value = posx;
            //    mccoord.Y.Value = posy;
            //    wafercoord = this.CoordinateManager().WaferHighChuckConvert.Convert(mccoord);
            //    this.StageSupervisor().StageModuleState.WaferHighViewMove(wafercoord.X.Value, wafercoord.Y.Value);

            //}

            ////degree = 0;
            ////for (int i = 0; i < 18; i++)
            ////{
            ////    degree = Math.PI * (degree * i) / 180;
            ////    dx = 150000 * Math.Cos(degree);
            ////    dy = 150000 * Math.Sin(degree);
            ////    posx = centerx + dx;
            ////    posy = centery + (dy * -1);

            ////    this.MotionManager().StageMove(posx, posy);
            ////    degree += 10;
            ////}

            //this.StageSupervisor().StageModuleState.WaferHighViewMove(0,0, zpos);
            try
            {
                double a = 0.0;
                double b = 0.0;
                double c = 0.0;
                double d = 0.0;

                double zheight = 0.0;
                double zOffset = 0.0;
                double zValue = 0.0;
                //Calc
                CatCoordinates pos1 = new CatCoordinates();
                CatCoordinates pos2 = new CatCoordinates();
                CatCoordinates pos3 = new CatCoordinates();
                List<CatCoordinates> poslist = new List<CatCoordinates>();
                pos1.X.Value = -94000;
                pos1.Y.Value = 227000;
                pos1.Z.Value = 396;

                pos2.X.Value = 75000;
                pos2.Y.Value = 231000;
                pos2.Z.Value = -386;


                pos3.X.Value = -6000;
                pos3.Y.Value = 92000;
                pos3.Z.Value = 32;

                poslist.Add(pos1);
                poslist.Add(pos2);
                poslist.Add(pos3);

                double x1 = -94000;
                double y1 = 227000;
                double x2 = 75000;
                double y2 = 231000;
                double x3 = -6000;
                double y3 = 92000;

                double Dx = 0;
                double Dy = 0;
                double Ex = 0;
                double Ey = 0;
                double Fx = 0;
                double Fy = 0;

                Dx = GetCenterPoint(x1, x2);
                Dy = GetCenterPoint(y1, y2);
                Ex = GetCenterPoint(x2, x3);
                Ey = GetCenterPoint(y2, y3);
                Fx = GetCenterPoint(x3, x1);
                Fy = GetCenterPoint(y3, y1);

                double slope1 = GetSlope(x1, y1, x2, y2);
                double slope2 = GetSlope(x2, y2, x3, y3);
                double slope3 = GetSlope(x3, y3, x1, y1);

                slope1 = GetReciprocal(slope1);
                slope2 = GetReciprocal(slope2);
                slope3 = GetReciprocal(slope3);

                double resultX = 0;
                double resultY = 0;

                resultX = ((slope1 * Dx) - (slope2 * Ex) - Dy + Ey) / (slope1 - slope2);
                resultY = ((slope1 * slope2) / (slope2 - slope1)) * (((-slope1 * Dx) / slope1) + (Dy / slope1) + ((slope2 * Ex) / slope2) - (Ey / slope2));

                double r1 = GetRadius(resultX, resultY, x1, y1);
                double r2 = GetRadius(resultX, resultY, x2, y2);
                double r3 = GetRadius(resultX, resultY, x3, y3);

                double xposition = 81656 * Math.PI * Math.Cos(90) / 180;
                double yposition = 81656 * Math.PI * Math.Sin(90) / 180;

                LoggerManager.Debug($"First Point = {poslist[0].X.Value}, {poslist[0].Y.Value}, {poslist[0].Z.Value}");
                LoggerManager.Debug($"Second Point = {poslist[1].X.Value}, {poslist[1].Y.Value}, {poslist[1].Z.Value}");
                LoggerManager.Debug($"Third Point = {poslist[2].X.Value}, {poslist[2].Y.Value}, {poslist[2].Z.Value}");

                a = poslist[0].Y.Value * (poslist[1].Z.Value - poslist[2].Z.Value) + poslist[1].Y.Value
                    * (poslist[2].Z.Value - poslist[0].Z.Value) + poslist[2].Y.Value * (poslist[0].Z.Value - poslist[1].Z.Value);

                b = poslist[0].Z.Value * (poslist[1].X.Value - poslist[2].X.Value) + poslist[1].Z.Value
                    * (poslist[2].X.Value - poslist[0].X.Value) + poslist[2].Z.Value * (poslist[0].X.Value - poslist[1].X.Value);

                c = poslist[0].X.Value * (poslist[1].Y.Value - poslist[2].Y.Value) + poslist[1].X.Value
                    * (poslist[2].Y.Value - poslist[0].Y.Value) + poslist[2].X.Value * (poslist[0].Y.Value - poslist[1].Y.Value);

                d = -poslist[0].X.Value * (poslist[1].Y.Value * poslist[2].Z.Value - poslist[2].Y.Value * poslist[1].Z.Value)
                    - poslist[1].X.Value * (poslist[2].Y.Value * poslist[0].Z.Value - poslist[0].Y.Value * poslist[2].Z.Value)
                    - poslist[2].X.Value * (poslist[0].Y.Value * poslist[1].Z.Value - poslist[1].Y.Value * poslist[0].Z.Value);

                zheight = -(a * xposition + b * yposition + d) / c;
                //zOffset = zheight - Wafer.SubsInfo.AveWaferThick;
                //zValue = zpos;//+ zOffset;
                //LoggerManager.Debug($string.Format("input zpos = {0} zOffset = {1} ReturnValue = {2}", zpos, zOffset, zValue));

                List<CatCoordinates> catlist = new List<CatCoordinates>();

                for (int i = 0; i < 359; i++)
                {
                    double xpos = 94587.7 * Math.Cos(Math.PI * i / 180);
                    double ypos = 94587.7 * Math.Sin(Math.PI * i / 180);

                    a = poslist[0].Y.Value * (poslist[1].Z.Value - poslist[2].Z.Value) + poslist[1].Y.Value
                            * (poslist[2].Z.Value - poslist[0].Z.Value) + poslist[2].Y.Value * (poslist[0].Z.Value - poslist[1].Z.Value);

                    b = poslist[0].Z.Value * (poslist[1].X.Value - poslist[2].X.Value) + poslist[1].Z.Value
                        * (poslist[2].X.Value - poslist[0].X.Value) + poslist[2].Z.Value * (poslist[0].X.Value - poslist[1].X.Value);

                    c = poslist[0].X.Value * (poslist[1].Y.Value - poslist[2].Y.Value) + poslist[1].X.Value
                        * (poslist[2].Y.Value - poslist[0].Y.Value) + poslist[2].X.Value * (poslist[0].Y.Value - poslist[1].Y.Value);

                    d = -poslist[0].X.Value * (poslist[1].Y.Value * poslist[2].Z.Value - poslist[2].Y.Value * poslist[1].Z.Value)
                        - poslist[1].X.Value * (poslist[2].Y.Value * poslist[0].Z.Value - poslist[0].Y.Value * poslist[2].Z.Value)
                        - poslist[2].X.Value * (poslist[0].Y.Value * poslist[1].Z.Value - poslist[1].Y.Value * poslist[0].Z.Value);

                    zheight = -(a * xpos + b * ypos + d) / c;

                    CatCoordinates cat = new CatCoordinates();
                    cat.X.Value = xpos * -1d;
                    cat.Y.Value = ypos * -1d;
                    cat.Z.Value = zheight;
                    catlist.Add(cat);

                    //this.MotionManager().StageMove(cat.X.Value, cat.Y.Value, -86500);
                }
                var minindex = catlist.FindIndex(item => item.Z.Value == catlist.Min(value => value.Z.Value));
                var maxindex = catlist.FindIndex(item => item.Z.Value == catlist.Max(value => value.Z.Value));
                var minzvalue = catlist.Min(item => item.Z.Value);
                var maxzvalue = catlist.Max(item => item.Z.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public double GetSlope(double x1, double y1, double x2, double y2)
        {
            double retVal = 0.0;

            try
            {
                retVal = (y2 - y1) / (x2 - x1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetReciprocal(double source)
        {
            double retVal = 0.0;

            try
            {
                retVal = (1 / source) * -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetCenterPoint(double startPoint, double endPoint)
        {
            double retVal = 0.0;

            try
            {
                retVal = (startPoint + endPoint) / 2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetRadius(double x1, double y1, double x2, double y2)
        {
            double retVal = 0.0;

            try
            {
                retVal = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y1 - y2), 2));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private AsyncCommand _AddMeasurmentPosCommand;
        public ICommand AddMeasurmentPosCommand
        {
            get
            {
                if (null == _AddMeasurmentPosCommand) _AddMeasurmentPosCommand = new AsyncCommand(AddMeasermentPos);
                return _AddMeasurmentPosCommand;
            }
        }
        private async Task AddMeasermentPos()
        {
            CatCoordinates pos = new CatCoordinates();

            try
            {
                pos.X.Value = MeasuermentXPos;
                pos.Y.Value = MeasuermentYPos;

                PosList.Add(pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AddSensorPosCommand;
        public ICommand AddSensorPosCommand
        {
            get
            {
                if (null == _AddSensorPosCommand) _AddSensorPosCommand = new AsyncCommand(AddSensorPos);
                return _AddSensorPosCommand;
            }
        }
        private async Task AddSensorPos()
        {

        }

        private AsyncCommand _CalcRValueCommand;
        public ICommand CalcRValueCommand
        {
            get
            {
                if (null == _CalcRValueCommand) _CalcRValueCommand = new AsyncCommand(CalcRValue);
                return _CalcRValueCommand;
            }
        }
        private async Task CalcRValue()
        {
            try
            {
                PosList.Clear();
                PosList.Add(new CatCoordinates(-101100, 233700, Sensor1Pos));
                PosList.Add(new CatCoordinates(71900, 233000, Sensor2Pos));
                PosList.Add(new CatCoordinates(-7100, 88700, Sensor3Pos));

                var radius = GetCircumRadius(PosList[0], PosList[1], PosList[2]);
                double Dx = 0;
                double Dy = 0;
                double Ex = 0;
                double Ey = 0;
                double Fx = 0;
                double Fy = 0;

                Dx = GetCenterPoint(PosList[0].X.Value, PosList[1].X.Value);
                Dy = GetCenterPoint(PosList[0].Y.Value, PosList[1].Y.Value);
                Ex = GetCenterPoint(PosList[1].X.Value, PosList[2].X.Value);
                Ey = GetCenterPoint(PosList[1].Y.Value, PosList[2].Y.Value);
                Fx = GetCenterPoint(PosList[2].X.Value, PosList[0].X.Value);
                Fy = GetCenterPoint(PosList[2].Y.Value, PosList[0].Y.Value);

                double slope1 = GetSlope(PosList[0].X.Value, PosList[0].Y.Value, PosList[1].X.Value, PosList[1].Y.Value);
                double slope2 = GetSlope(PosList[1].X.Value, PosList[1].Y.Value, PosList[2].X.Value, PosList[2].Y.Value);
                double slope3 = GetSlope(PosList[2].X.Value, PosList[2].Y.Value, PosList[0].X.Value, PosList[0].Y.Value);


                slope1 = GetReciprocal(slope1);
                slope2 = GetReciprocal(slope2);
                slope3 = GetReciprocal(slope3);

                double resultX = 0;
                double resultY = 0;

                resultX = ((slope1 * Dx) - (slope2 * Ex) - Dy + Ey) / (slope1 - slope2);
                resultY = ((slope1 * slope2) / (slope2 - slope1)) * (((-slope1 * Dx) / slope1) + (Dy / slope1) + ((slope2 * Ex) / slope2) - (Ey / slope2));

                double r1 = GetRadius(resultX, resultY, PosList[0].X.Value, PosList[0].Y.Value);
                double r2 = GetRadius(resultX, resultY, PosList[1].X.Value, PosList[1].Y.Value);
                double r3 = GetRadius(resultX, resultY, PosList[2].X.Value, PosList[2].Y.Value);

                double resultr = (r1 + r2 + r3) / 3;
                List<CatCoordinates> indexlist = new List<CatCoordinates>();
                List<CatCoordinates> poslist = new List<CatCoordinates>();
                CatCoordinates pos1 = new CatCoordinates();
                CatCoordinates pos2 = new CatCoordinates();
                CatCoordinates pos3 = new CatCoordinates();


                pos1.X.Value = this.CoordinateManager().StageCoord.RefMarkPos.X.Value -
                                this.CoordinateManager().StageCoord.WHOffset.X.Value -
                                this.CoordinateManager().StageCoord.PHOffset.X.Value -
                                this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value
                                - PosList[0].X.Value;
                pos1.Y.Value = this.CoordinateManager().StageCoord.RefMarkPos.Y.Value -
                                this.CoordinateManager().StageCoord.WHOffset.Y.Value -
                                this.CoordinateManager().StageCoord.PHOffset.Y.Value -
                                this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value
                                - PosList[0].Y.Value;
                pos1.Z.Value = Sensor1Pos;

                pos2.X.Value = this.CoordinateManager().StageCoord.RefMarkPos.X.Value -
                                this.CoordinateManager().StageCoord.WHOffset.X.Value -
                                this.CoordinateManager().StageCoord.PHOffset.X.Value -
                                this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value - PosList[1].X.Value;
                pos2.Y.Value = this.CoordinateManager().StageCoord.RefMarkPos.Y.Value -
                                this.CoordinateManager().StageCoord.WHOffset.Y.Value -
                                this.CoordinateManager().StageCoord.PHOffset.Y.Value -
                                this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value - PosList[1].Y.Value;
                pos2.Z.Value = Sensor2Pos;

                pos3.X.Value = this.CoordinateManager().StageCoord.RefMarkPos.X.Value -
                                this.CoordinateManager().StageCoord.WHOffset.X.Value -
                                this.CoordinateManager().StageCoord.PHOffset.X.Value -
                                this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value - PosList[2].X.Value;
                pos3.Y.Value = this.CoordinateManager().StageCoord.RefMarkPos.Y.Value -
                                this.CoordinateManager().StageCoord.WHOffset.Y.Value -
                                this.CoordinateManager().StageCoord.PHOffset.Y.Value -
                                this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value - PosList[2].Y.Value;
                pos3.Z.Value = Sensor3Pos;

                poslist.Add(pos1);
                poslist.Add(pos2);
                poslist.Add(pos3);

                double a = 0;
                double b = 0;
                double c = 0;
                double d = 0;
                double zheight = 0;
                indexlist.Clear();

                for (int i = 0; i < 359; i++)
                {
                    double xpos = resultr * Math.Cos((Math.PI * i) / 180);
                    double ypos = resultr * Math.Sin((Math.PI * i) / 180);

                    a = (poslist[0].Y.Value * poslist[1].Z.Value) - (poslist[0].Y.Value * poslist[2].Z.Value) +
                        (poslist[1].Y.Value * poslist[2].Z.Value) - (poslist[1].Y.Value * poslist[0].Z.Value) +
                        (poslist[2].Y.Value * poslist[0].Z.Value) - (poslist[2].Y.Value * poslist[1].Z.Value);

                    b = (poslist[0].Z.Value * poslist[1].X.Value) - (poslist[0].Z.Value * poslist[2].X.Value) +
                        (poslist[1].Z.Value * poslist[2].X.Value) - (poslist[1].Z.Value * poslist[0].X.Value) +
                        (poslist[2].Z.Value * poslist[0].X.Value) - (poslist[2].Z.Value * poslist[1].X.Value);

                    c = (poslist[0].X.Value * poslist[1].Y.Value) - (poslist[0].X.Value * poslist[2].Y.Value) +
                        (poslist[1].X.Value * poslist[2].Y.Value) - (poslist[1].X.Value * poslist[0].Y.Value) +
                        (poslist[2].X.Value * poslist[0].Y.Value) - (poslist[2].X.Value * poslist[1].Y.Value);

                    d = (poslist[0].X.Value * (poslist[1].Y.Value * poslist[2].Z.Value)) - (poslist[0].X.Value * (poslist[2].Y.Value * poslist[1].Z.Value)) +
                        (poslist[1].X.Value * (poslist[2].Y.Value * poslist[0].Z.Value)) - (poslist[1].X.Value * (poslist[0].Y.Value * poslist[2].Z.Value)) +
                        (poslist[2].X.Value * (poslist[0].Y.Value * poslist[1].Z.Value)) - (poslist[2].X.Value * (poslist[1].Y.Value * poslist[0].Z.Value));
                    d = d * -1;



                    zheight = -(a * xpos + b * ypos + d) / c;
                    CatCoordinates cat = new CatCoordinates();
                    cat.X.Value = xpos * -1d;
                    cat.Y.Value = ypos * -1d;
                    cat.Z.Value = zheight;
                    indexlist.Add(cat);
                }
                var minindex = indexlist.FindIndex(item => item.Z.Value == indexlist.Min(value => value.Z.Value));
                var maxindex = indexlist.FindIndex(item => item.Z.Value == indexlist.Max(value => value.Z.Value));
                var minzvalue = indexlist.Min(item => item.Z.Value);
                var maxzvalue = indexlist.Max(item => item.Z.Value);
                indexlist.Clear();

                for (int i = 0; i < 359; i++)
                {
                    double xpos = resultr * Math.Cos((Math.PI * i) / 180);
                    double ypos = resultr * Math.Sin((Math.PI * i) / 180);
                    xpos = xpos * -1;
                    ypos = ypos * -1;
                    a = poslist[0].Y.Value * (poslist[1].Z.Value - poslist[2].Z.Value) + poslist[1].Y.Value
                            * (poslist[2].Z.Value - poslist[0].Z.Value) + poslist[2].Y.Value * (poslist[0].Z.Value - poslist[1].Z.Value);

                    b = poslist[0].Z.Value * (poslist[1].X.Value - poslist[2].X.Value) + poslist[1].Z.Value
                        * (poslist[2].X.Value - poslist[0].X.Value) + poslist[2].Z.Value * (poslist[0].X.Value - poslist[1].X.Value);

                    c = poslist[0].X.Value * (poslist[1].Y.Value - poslist[2].Y.Value) + poslist[1].X.Value
                        * (poslist[2].Y.Value - poslist[0].Y.Value) + poslist[2].X.Value * (poslist[0].Y.Value - poslist[1].Y.Value);

                    d = -poslist[0].X.Value * (poslist[1].Y.Value * poslist[2].Z.Value - poslist[2].Y.Value * poslist[1].Z.Value)
                        - poslist[1].X.Value * (poslist[2].Y.Value * poslist[0].Z.Value - poslist[0].Y.Value * poslist[2].Z.Value)
                        - poslist[2].X.Value * (poslist[0].Y.Value * poslist[1].Z.Value - poslist[1].Y.Value * poslist[0].Z.Value);

                    zheight = -(a * xpos + b * ypos + d) / c;

                    CatCoordinates cat = new CatCoordinates();
                    cat.X.Value = xpos;
                    cat.Y.Value = ypos;
                    cat.Z.Value = zheight;
                    indexlist.Add(cat);

                    //this.MotionManager().StageMove(cat.X.Value, cat.Y.Value, -86500);
                }
                minindex = indexlist.FindIndex(item => item.Z.Value == indexlist.Min(value => value.Z.Value));
                maxindex = indexlist.FindIndex(item => item.Z.Value == indexlist.Max(value => value.Z.Value));
                minzvalue = indexlist.Min(item => item.Z.Value);
                maxzvalue = indexlist.Max(item => item.Z.Value);

                if (Math.Abs(maxindex - minindex) == 180)
                {
                    ShfitRValue = 360 - minindex;
                }
                //ShfitRValue = Math.Abs(maxindex - minindex);

                PosList.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private double GetCircumRadius(CatCoordinates pointA, CatCoordinates pointB, CatCoordinates pointC)
        {

            double retVal = 0;

            try
            {
                double ax = pointC.X.Value - pointB.X.Value;
                double ay = pointC.Y.Value - pointB.Y.Value;
                double bx = pointA.X.Value - pointC.X.Value;
                double by = pointA.Y.Value - pointC.Y.Value;

                double crossab = ax * by - ay * bx;
                if (crossab != 0)
                {
                    double a = Math.Sqrt((ax * ax)) + (ay * ay);
                    double b = Math.Sqrt((bx * bx)) + (by * by);
                    double cx = pointB.X.Value - pointA.X.Value;
                    double cy = pointB.Y.Value - pointA.Y.Value;
                    double c = Math.Sqrt((cx * cx) + (cy * cy));
                    retVal = ((0.5 * a * b * c) / Math.Abs(crossab));
                }
                else
                {
                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private bool _FirstDegree;
        public bool FirstDegree
        {
            get { return _FirstDegree; }
            set
            {
                if (value != _FirstDegree)
                {
                    _FirstDegree = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SecondDegree;
        public bool SecondDegree
        {
            get { return _SecondDegree; }
            set
            {
                if (value != _SecondDegree)
                {
                    _SecondDegree = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ThirdDgree;
        public bool ThirdDgree
        {
            get { return _ThirdDgree; }
            set
            {
                if (value != _ThirdDgree)
                {
                    _ThirdDgree = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PosZ2;
        public double PosZ2
        {
            get { return _PosZ2; }
            set
            {
                if (value != _PosZ2)
                {
                    _PosZ2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PosZ1;
        public double PosZ1
        {
            get { return _PosZ1; }
            set
            {
                if (value != _PosZ1)
                {
                    _PosZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PosZ0;
        public double PosZ0
        {
            get { return _PosZ0; }
            set
            {
                if (value != _PosZ0)
                {
                    _PosZ0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AVGHeight;
        public double AVGHeight
        {
            get { return _AVGHeight; }
            set
            {
                if (value != _AVGHeight)
                {
                    _AVGHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChuckCenterOffsetX;
        public double ChuckCenterOffsetX
        {
            get { return _ChuckCenterOffsetX; }
            set
            {
                if (value != _ChuckCenterOffsetX)
                {
                    _ChuckCenterOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ChuckCenterOffsetY;
        public double ChuckCenterOffsetY
        {
            get { return _ChuckCenterOffsetY; }
            set
            {
                if (value != _ChuckCenterOffsetY)
                {
                    _ChuckCenterOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DegreeZ0;
        public double DegreeZ0
        {
            get { return _DegreeZ0; }
            set
            {
                if (value != _DegreeZ0)
                {
                    _DegreeZ0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DegreeZ1;
        public double DegreeZ1
        {
            get { return _DegreeZ1; }
            set
            {
                if (value != _DegreeZ1)
                {
                    _DegreeZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DegreeZ2;
        public double DegreeZ2
        {
            get { return _DegreeZ2; }
            set
            {
                if (value != _DegreeZ2)
                {
                    _DegreeZ2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Radius;
        public double Radius
        {
            get { return _Radius; }
            set
            {
                if (value != _Radius)
                {
                    _Radius = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _ChuckOutCornerFocusingCommand;
        public ICommand ChuckOutCornerFocusingCommand
        {
            get
            {
                if (null == _ChuckOutCornerFocusingCommand) _ChuckOutCornerFocusingCommand = new AsyncCommand(ChuckOutCornerFocusing);
                return _ChuckOutCornerFocusingCommand;
            }
        }
        private async Task<EventCodeEnum> ChuckOutCornerFocusing()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                double dx = 0;
                double dy = 0;
                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zpos = axisz.Param.HomeOffset.Value;
                List<CatCoordinates> cornerPosList = new List<CatCoordinates>();
                //Task<EventCodeEnum> task;
                //MachineCoordinate mc = new MachineCoordinate();
                //WaferCoordinate wf = new WaferCoordinate();
                //mc.X.Value = 150000;
                //mc.Y.Value = 0;
                //mc.Z.Value = zpos;
                //wf = this.CoordinateManager().WaferHighChuckConvert.Convert(mc);
                //this.StageSupervisor().StageModuleState.WaferHighViewMove(wf.X.Value, wf.Y.Value, wf.Z.Value);
                int cnt = 3;

                for (int i = 0; i < cnt; i++)
                {
                    //if (FirstDegree)
                    //{
                    //    dx = 110000 * Math.Cos(Math.PI * (i * 10) / 180);
                    //    dy = 110000 * Math.Sin(Math.PI * (i * 10) / 180);
                    //}
                    //else if (SecondDegree)
                    //{
                    //    dx = 110000 * Math.Cos(Math.PI * (i * 30) / 180);
                    //    dy = 110000 * Math.Sin(Math.PI * (i * 30) / 180);
                    //}
                    //else if (ThirdDgree)
                    //{
                    double radius = Radius;
                    if (i == 0)
                    {
                        dx = radius * Math.Cos(Math.PI * DegreeZ2 / 180) * -1;
                        dy = radius * Math.Sin(Math.PI * DegreeZ2 / 180) * -1;
                    }
                    else if (i == 1)
                    {
                        dx = radius * Math.Cos(Math.PI * DegreeZ1 / 180) * -1;
                        dy = radius * Math.Sin(Math.PI * DegreeZ1 / 180) * -1;
                    }
                    else if (i == 2)
                    {
                        dx = radius * Math.Cos(Math.PI * DegreeZ0 / 180) * -1;
                        dy = radius * Math.Sin(Math.PI * DegreeZ0 / 180) * -1;
                    }
                    //}
                    else
                    {
                        return EventCodeEnum.NONE;
                    }


                    dx = dx;
                    dy = dy;
                    dx = ChuckCenterOffsetX + dx;
                    dy = ChuckCenterOffsetY + dy;
                    MachineCoordinate mccoord = new MachineCoordinate();
                    WaferCoordinate wafercoord = new WaferCoordinate();

                    mccoord.X.Value = dx;
                    mccoord.Y.Value = dy;
                    mccoord.Z.Value = this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zpos);
                    mccoord.Z.Value = zpos;

                    wafercoord = this.CoordinateManager().WaferHighChuckConvert.Convert(mccoord);


                    await Task.Run(() =>
                    {
                        ret = this.StageSupervisor().StageModuleState.WaferHighViewMove(wafercoord.X.Value, wafercoord.Y.Value, wafercoord.Z.Value);
                        if (ret == EventCodeEnum.NONE)
                        {
                            FocusingParam.FlatnessThreshold.Value = 95.0;
                            FocusingParam.FocusRange.Value = 500;
                            FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);
                        }
                        else
                        {

                        }
                        CatCoordinates pos = new CatCoordinates();
                        pos.X.Value = mccoord.X.Value;
                        pos.Y.Value = mccoord.Y.Value;
                        double actZpos = 0;
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref actZpos);
                        pos.Z.Value = actZpos;
                        cornerPosList.Add(pos);
                    });
                }

                StageButtonsVisibility = true;
                double aveZpos = 0;

                for (int i = 0; i < cornerPosList.Count; i++)
                {
                    aveZpos += cornerPosList[i].Z.Value;
                }

                aveZpos = aveZpos / cornerPosList.Count;

                AVGHeight = aveZpos;

                PosZ2 = cornerPosList[0].Z.Value;
                PosZ1 = cornerPosList[1].Z.Value;
                PosZ0 = cornerPosList[2].Z.Value;
            }

            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _AutoFocusingCommand;
        public ICommand AutoFocusingCommand
        {
            get
            {
                if (null == _AutoFocusingCommand) _AutoFocusingCommand = new AsyncCommand(AutoFocusing);
                return _AutoFocusingCommand;
            }
        }
        private async Task<EventCodeEnum> AutoFocusing()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                double dx = 0;
                double dy = 0;
                var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zpos = 0;

                MachineCoordinate mccoord = new MachineCoordinate();
                WaferCoordinate wafercoord = new WaferCoordinate();

                FocusingParam.FocusRange.Value = 200;
                FocusingParam.FlatnessThreshold.Value = 95.0;
                await Task.Run(() =>
                {
                    FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);
                });
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zpos);
                double azpos = zpos;
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
            }

            return ret;
        }


        private AsyncCommand _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(Focusing);
                return _FocusingCommand;
            }
        }
        bool focusingLoopEnable = false;
        private async Task Focusing()
        {
            try
            {
                if (focusingLoopEnable == false)
                {
                    StageButtonsVisibility = false;
                    focusingLoopEnable = true;
                    EnumProberCam cam = EnumProberCam.UNDEFINED;
                    await Task.Run(() =>
                    {
                        switch (SelectedCam)
                        {
                            case enumStageCamType.UNDEFINED:
                                break;
                            case enumStageCamType.WaferHigh:
                                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 96);
                                cam = EnumProberCam.WAFER_HIGH_CAM;
                                break;
                            case enumStageCamType.WaferLow:
                                this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).SetLight(EnumLightType.COAXIAL, 96);
                                cam = EnumProberCam.WAFER_LOW_CAM;
                                break;
                            case enumStageCamType.PinHigh:
                                this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 50);
                                cam = EnumProberCam.PIN_HIGH_CAM;
                                break;
                            case enumStageCamType.PinLow:
                                this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM).SetLight(EnumLightType.COAXIAL, 50);
                                cam = EnumProberCam.PIN_LOW_CAM;
                                break;
                            case enumStageCamType.WaferHighNC:
                                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 96);
                                cam = EnumProberCam.WAFER_HIGH_CAM;
                                break;
                            default:
                                break;
                        }

                        FocusingParam.FocusingAxis.Value = EnumAxisConstants.Z;
                        FocusingParam.FocusRange.Value = 200;
                        FocusingParam.FocusingCam.Value = cam;
                        FocusingParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                        WaferCoordinate wfc = new WaferCoordinate();
                        StageButtonsVisibility = true;
                        while (focusingLoopEnable)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, 0, false, EnumTrjType.Normal, 1);
                            FocusingModule.Focusing_Retry(FocusingParam, false, false, false, this);
                            wfc = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                            LoggerManager.Debug($"WaferCoordinate Z height : {wfc.Z.Value}");
                            Thread.Sleep(1500);
                        }

                        //FocusingModule.FocusParameter.FocusingAxis.Value = EnumAxisConstants.Z;
                        //FocusingModule.FocusParameter.FocusingCam.Value = cam;
                        //FocusingModule.Focusing_Retry(false, false, false);

                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                StageButtonsVisibility = true;
            }
        }

        private AsyncCommand _WaferEdgeExcuteCommand;
        public ICommand WaferEdgeExcuteCommand
        {
            get
            {
                if (null == _WaferEdgeExcuteCommand) _WaferEdgeExcuteCommand = new AsyncCommand(WaferEdgeExcuteCommandFunc);
                return _WaferEdgeExcuteCommand;
            }
        }

        private AsyncCommand _TestEdgePattExcuteCommand;
        public ICommand TestEdgePattExcuteCommand
        {
            get
            {
                if (null == _TestEdgePattExcuteCommand) _TestEdgePattExcuteCommand = new AsyncCommand(TestEdgePattExcuteFunc);
                return _TestEdgePattExcuteCommand;
            }
        }

        private async Task WaferEdgeExcuteCommandFunc()
        {
            try
            {
                StageButtonsVisibility = false;
                WaferCoordinate wafercenter = new WaferCoordinate();
                double maximum_Value_X= 0.0;
                double maximum_Value_Y = 0.0;
                var ret = this.WaferAligner().EdgeCheck(ref wafercenter, ref maximum_Value_X, ref maximum_Value_Y);
                if (ret == EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Wafer Edge Success",
                        $"Center X : {wafercenter.GetX()}, Center Y : {wafercenter.GetY()}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Wafer Edge Fail", $"", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                StageButtonsVisibility = true;
            }
        }

        private async Task TestEdgePattExcuteFunc()
        {
            try
            {
                StageButtonsVisibility = false;
                ImageBuffer imageBuffer = null;

                this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM).GetCurImage(out imageBuffer);

                if (this.VisionManager().VisionProcessing.IsValidModLicense() == false)
                {
                    LoggerManager.Debug($"TestEdgePattExcuteFunc(): Model Finder License Is Invalid. Do Not Use Model Find");
                }
                else
                {
                    LoggerManager.Debug($"TestEdgePattExcuteFunc(): Model Finder License Is Valid.");

                    ICardChangeDevParam CCDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                    List<MFParameter> mFParameters = CCDevParam.ModelInfos;
                    List<ModelFinderResult> mfresult = null;
                    if (mFParameters != null)
                    {
                        foreach (MFParameter mf in mFParameters)
                        {
                            var rectResult = this.VisionManager().VisionProcessing.ModelFind(imageBuffer,
                           mf.ModelTargetType.Value, mf.ForegroundType.Value,
                           new System.Windows.Size(mf.ModelWidth.Value / Cam.GetRatioX(), mf.ModelHeight.Value / Cam.GetRatioY()),
                           mf.Acceptance.Value,
                           scale_min: 0.95, scale_max: 1.05,
                           smoothness: mf.Smoothness.Value
                           );
                            if (rectResult.Count > 0)
                            {
                                int margin = 10;
                                for (int i = 0; i < rectResult.Count; i++)
                                {
                                    var cirResult = this.VisionManager().VisionProcessing.ModelFind(imageBuffer,
                                                    mf.Child.ModelTargetType.Value, mf.Child.ForegroundType.Value,
                                                    new System.Windows.Size(mf.Child.ModelWidth.Value / Cam.GetRatioX(), mf.Child.ModelHeight.Value / Cam.GetRatioY()),
                                                    mf.Child.Acceptance.Value,
                                                    rectResult[i].Position.X.Value, rectResult[i].Position.Y.Value + margin,
                                                    mf.ModelWidth.Value / Cam.GetRatioX(), mf.ModelHeight.Value / Cam.GetRatioY(),
                                                    scale_min: 0.95, scale_max: 1.05, smoothness:mf.Smoothness.Value);

                                    if (cirResult.Count > 0)
                                    {
                                        double RectCenX = rectResult[i].Position.X.Value; //+ (mf.ModelWidth.Value / Cam.GetRatioX()) / 2;
                                        double RectCenY = rectResult[i].Position.Y.Value; //+ (mf.ModelHeight.Value / Cam.GetRatioY()) / 2;

                                        double CirCenX = cirResult[0].Position.X.Value;
                                        double CirCenY = cirResult[0].Position.Y.Value;
                                        double x2 = (RectCenX - CirCenX) * (RectCenX - CirCenX);
                                        double y2 = (RectCenY - CirCenY) * (RectCenY - CirCenY);
                                        double distance = Math.Sqrt(x2 + y2);

                                        LoggerManager.Debug($"TestEdgePattExcuteFunc(): Model Finder Distance:{distance:0.00} RectIndex:{i}, CirIndex:{0}");
                                        if (distance < 5)
                                        {
                                            mfresult = cirResult;

                                            string imgPath2 = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, $"\\MFImage\\Success\\PassImage_Distance{distance}_Rect{rectResult[i].Score:0.0}_Cir{cirResult[0].Score:0.00}");
                                            this.VisionManager().SaveImageBuffer(cirResult[0].ResultBuffer, imgPath2, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        string imgPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage");
                                        this.VisionManager().SaveImageBuffer(imageBuffer, imgPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                                        LoggerManager.Debug($"ModelFind({mf.Child.ModelTargetType}): Fail Modelfinder find 0 models. Saved Image Path: " + imgPath);

                                        LoggerManager.Debug($"GetModelPosition(): Child Count {cirResult.Count}. baseresults Index:{i}");
                                    }
                                }

                                if (mfresult == null)
                                {
                                    string imgPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage");
                                    this.VisionManager().SaveImageBuffer(imageBuffer, imgPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                                    
                                    LoggerManager.Debug($"ModelFind():Distance Fail Modelfinder find 0 models. Saved Image Path: " + imgPath);
                                }
                            }
                            else
                            {
                                string imgPath = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage");
                                this.VisionManager().SaveImageBuffer(imageBuffer, imgPath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                                LoggerManager.Debug($"ModelFind({mf.ModelTargetType}): Fail Modelfinder find 0 models. Saved Image Path: " + imgPath);

                                LoggerManager.Debug($"GetModelPosition(): Base Count {rectResult.Count}. ");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                StageButtonsVisibility = true;
            }
        }

        #endregion


        #region Tab2
        private int _WaferCamMoveCount;
        public int WaferCamMoveCount
        {
            get { return _WaferCamMoveCount; }
            set
            {
                if (value != _WaferCamMoveCount)
                {
                    _WaferCamMoveCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MarkMoveCount;
        public int MarkMoveCount
        {
            get { return _MarkMoveCount; }
            set
            {
                if (value != _MarkMoveCount)
                {
                    _MarkMoveCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DifMarkPosX;
        public double DifMarkPosX
        {
            get { return _DifMarkPosX; }
            set
            {
                if (value != _DifMarkPosX)
                {
                    _DifMarkPosX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _DifMarkPosY;
        public double DifMarkPosY
        {
            get { return _DifMarkPosY; }
            set
            {
                if (value != _DifMarkPosY)
                {
                    _DifMarkPosY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _WaferCamCountMoveCommand;
        public ICommand WaferCamCountMoveCommand
        {
            get
            {
                if (null == _WaferCamCountMoveCommand) _WaferCamCountMoveCommand = new AsyncCommand(WaferCamCountMove);
                return _WaferCamCountMoveCommand;
            }
        }
        private async Task<EventCodeEnum> WaferCamCountMove()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            int retval = -1;

            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < WaferCamMoveCount; i++)
                    {
                        retval = StageCylinderType.MoveWaferCam.Extend();
                        Thread.Sleep(1000);
                        if (ret != 0)
                        {
                            //ERrror
                        }
                        retval = StageCylinderType.MoveWaferCam.Retract();
                        Thread.Sleep(1000);
                        if (ret != 0)
                        {
                            //ERrror
                        }
                    }

                    retval = StageCylinderType.MoveWaferCam.Extend();
                    if (ret != 0)
                    {
                        //ERrror
                    }
                    //ret = this.StageSupervisor().StageModuleState.MoveToMark();
                    Thread.Sleep(2000);

                });

                this.VisionManager().StartGrab(EnumProberCam.WAFER_HIGH_CAM, this);

                Cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
            }

            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
            }

            return ret;
        }


        private AsyncCommand _CCMoveINCommand;
        public ICommand CCMoveINCommand
        {
            get
            {
                if (null == _CCMoveINCommand) _CCMoveINCommand = new AsyncCommand(CCMoveIN);
                return _CCMoveINCommand;
            }
        }
        private async Task<EventCodeEnum> CCMoveIN()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.CardChageMoveToIN();
                });


            }

            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _CCMoveOUTCommand;
        public ICommand CCMoveOUTCommand
        {
            get
            {
                if (null == _CCMoveOUTCommand) _CCMoveOUTCommand = new AsyncCommand(CCMoveOUT);
                return _CCMoveOUTCommand;
            }
        }
        private async Task<EventCodeEnum> CCMoveOUT()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.CardChageMoveToOUT();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _CCMoveIDLECOmmand;
        public ICommand CCMoveIDLECommand
        {
            get
            {
                if (null == _CCMoveIDLECOmmand) _CCMoveIDLECOmmand = new AsyncCommand(CCMoveIDLE);
                return _CCMoveIDLECOmmand;
            }
        }
        private async Task<EventCodeEnum> CCMoveIDLE()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.CardChageMoveToIDLE();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
            }

            return ret;
        }
        #endregion


        #region VAC& TRI
        private AsyncCommand _ChuckVacOffCommand;
        public ICommand ChuckVacOffCommand
        {
            get
            {
                if (null == _ChuckVacOffCommand) _ChuckVacOffCommand = new AsyncCommand(ChuckVacOff);
                return _ChuckVacOffCommand;
            }
        }
        private async Task<EventCodeEnum> ChuckVacOff()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);

                    this.GetParam_Wafer().SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW, true);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW_12, true);

                    System.Threading.Thread.Sleep((int)this.IOManager().IO.Outputs.DOCHUCK_BLOW.MaintainTime.Value);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW_12, false);


                    //this.StageSupervisor().SetWaferObjectStatus();
                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false, 10000);
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _ChuckVacOnCommand;
        public ICommand ChuckVacOnCommand
        {
            get
            {
                if (null == _ChuckVacOnCommand) _ChuckVacOnCommand = new AsyncCommand(ChuckVacOn);
                return _ChuckVacOnCommand;
            }
        }
        private async Task<EventCodeEnum> ChuckVacOn()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW, false);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCK_BLOW_12, false);

                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true, 10000);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true, 10000);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);


                    this.StageSupervisor().SetWaferObjectStatus();
                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true, 10000);
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _TriUPCommand;
        public ICommand TriUPCommand
        {
            get
            {
                if (null == _TriUPCommand) _TriUPCommand = new AsyncCommand(TriUP);
                return _TriUPCommand;
            }
        }
        private async Task<EventCodeEnum> TriUP()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.Handlerhold(10000);
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _TriDNCommand;
        public ICommand TriDNCommand
        {
            get
            {
                if (null == _TriDNCommand) _TriDNCommand = new AsyncCommand(TriDN);
                return _TriDNCommand;
            }
        }
        private async Task<EventCodeEnum> TriDN()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _FrontDoorLockCommand;
        public ICommand FrontDoorLockCommand
        {
            get
            {
                if (null == _FrontDoorLockCommand) _FrontDoorLockCommand = new AsyncCommand(FrontDoorLock);
                return _FrontDoorLockCommand;
            }
        }
        private async Task<EventCodeEnum> FrontDoorLock()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.FrontDoorLock();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _FrontDoorUnLockCommand;
        public ICommand FrontDoorUnLockCommand
        {
            get
            {
                if (null == _FrontDoorUnLockCommand) _FrontDoorUnLockCommand = new AsyncCommand(FrontDoorUnLock);
                return _FrontDoorUnLockCommand;
            }
        }
        private async Task<EventCodeEnum> FrontDoorUnLock()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.FrontDoorUnLock();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _LoaderDoorOpenCommand;
        public ICommand LoaderDoorOpenCommand
        {
            get
            {
                if (null == _LoaderDoorOpenCommand) _LoaderDoorOpenCommand = new AsyncCommand(LoaderDoorOpen);
                return _LoaderDoorOpenCommand;
            }
        }
        private async Task<EventCodeEnum> LoaderDoorOpen()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.LoaderDoorOpen();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _LoaderDoorCloseCommand;
        public ICommand LoaderDoorCloseCommand
        {
            get
            {
                if (null == _LoaderDoorCloseCommand) _LoaderDoorCloseCommand = new AsyncCommand(LoaderDoorClose);
                return _LoaderDoorCloseCommand;
            }
        }
        private async Task<EventCodeEnum> LoaderDoorClose()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.LoaderDoorClose();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }
        private AsyncCommand _CardDoorOpenCommand;
        public ICommand CardDoorOpenCommand
        {
            get
            {
                if (null == _CardDoorOpenCommand) _CardDoorOpenCommand = new AsyncCommand(CardDoorOpen);
                return _CardDoorOpenCommand;
            }
        }
        private async Task<EventCodeEnum> CardDoorOpen()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.CardDoorOpen();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }
        private AsyncCommand _CardDoorCloseCommand;
        public ICommand CardDoorCloseCommand
        {
            get
            {
                if (null == _CardDoorCloseCommand) _CardDoorCloseCommand = new AsyncCommand(CardDoorClose);
                return _CardDoorCloseCommand;
            }
        }
        private async Task<EventCodeEnum> CardDoorClose()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.StageSupervisor().StageModuleState.CardDoorClose();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }
        private AsyncCommand _ZUPLampOpenCommand;
        public ICommand ZUPLampOpenCommand
        {
            get
            {
                if (null == _ZUPLampOpenCommand) _ZUPLampOpenCommand = new AsyncCommand(ZUPLAMPOpen);
                return _ZUPLampOpenCommand;
            }
        }
        private async Task<EventCodeEnum> ZUPLAMPOpen()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOZUPLAMPON, true);
                });
            }

            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _ZUPLampCloseCommand;
        public ICommand ZUPLampCloseCommand
        {
            get
            {
                if (null == _ZUPLampCloseCommand) _ZUPLampCloseCommand = new AsyncCommand(ZUPLampClose);
                return _ZUPLampCloseCommand;
            }
        }
        private async Task<EventCodeEnum> ZUPLampClose()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOZUPLAMPON, false);
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }
        #endregion

        #region EMGSTOP

        private AsyncCommand _StageEMGStopCommand;
        public ICommand StageEMGStopCommand
        {
            get
            {
                if (null == _StageEMGStopCommand) _StageEMGStopCommand = new AsyncCommand(StageEMGStop);
                return _StageEMGStopCommand;
            }
        }
        private async Task<EventCodeEnum> StageEMGStop()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.MonitoringManager().StageEmergencyStop();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private AsyncCommand _LoaderEMGStopCommand;
        public ICommand LoaderEMGStopCommand
        {
            get
            {
                if (null == _LoaderEMGStopCommand) _LoaderEMGStopCommand = new AsyncCommand(LoaderEMGStop);
                return _LoaderEMGStopCommand;
            }
        }



        private async Task<EventCodeEnum> LoaderEMGStop()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    ret = this.MonitoringManager().LoaderEmergencyStop();
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

            return ret;
        }
        #endregion
    }
}
