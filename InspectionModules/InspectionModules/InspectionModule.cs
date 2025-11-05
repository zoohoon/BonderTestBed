using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.WaferAlign;
using ProbingModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SubstrateObjects;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using Focusing;
using MetroDialogInterfaces;
namespace InspectionModules
{
    public class InspectionModule : IInspection, INotifyPropertyChanged
    {
        private double _XSetFromCoord;
        public double XSetFromCoord
        {
            get
            {
                return _XSetFromCoord;
            }
            set
            {
                if (value != _XSetFromCoord && value != 0)
                {
                    _XSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YSetFromCoord;
        public double YSetFromCoord
        {
            get
            {
                return _YSetFromCoord;
            }
            set
            {
                if (value != _YSetFromCoord && value != 0)
                {
                    _YSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        //#region ==> UcDispaly Port Target Rectangle
        //private double _TargetRectangleLeft;
        //public double TargetRectangleLeft
        //{
        //    get { return _TargetRectangleLeft; }
        //    set
        //    {
        //        if (value != _TargetRectangleLeft)
        //        {
        //            _TargetRectangleLeft = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _TargetRectangleTop;
        //public double TargetRectangleTop
        //{
        //    get { return _TargetRectangleTop; }
        //    set
        //    {
        //        if (value != _TargetRectangleTop)
        //        {
        //            _TargetRectangleTop = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _TargetRectangleWidth;
        //public double TargetRectangleWidth
        //{
        //    get { return _TargetRectangleWidth; }
        //    set
        //    {
        //        if (value != _TargetRectangleWidth)
        //        {
        //            //if (!(value > CurCam.GetGrabSizeWidth()) && !(value < 0))
        //            //{
        //            //    _TargetRectangleWidth = value;
        //            //    NotifyPropertyChanged("TargetRectangleWidth");
        //            //}
        //            if (!(value < 4))
        //            {
        //                _TargetRectangleWidth = value;
        //                RaisePropertyChanged();
        //            }
        //        }
        //    }
        //}

        //private double _TargetRectangleHeight;
        //public double TargetRectangleHeight
        //{
        //    get { return _TargetRectangleHeight; }
        //    set
        //    {
        //        if (value != _TargetRectangleHeight)
        //        {
        //            //if (!(value > CurCam.GetGrabSizeHeight()) && !(value < 0))
        //            //{
        //            //    _TargetRectangleHeight = value;
        //            //    NotifyPropertyChanged("TargetRectangleHeight");
        //            //}

        //            if (!(value < 4))
        //            {
        //                _TargetRectangleHeight = value;
        //                RaisePropertyChanged();
        //            }
        //        }
        //    }
        //}

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

        //public int ChangeWidthValue = 4;
        //public int ChangeHeightValue = 4;

        //public double PatternSizeWidth;
        //public double PatternSizeHeight;
        //public double PatternSizeLeft;
        //public double PatternSizeTop;

        //public void UCDisplayRectWidthPlus()
        //{
        //    try
        //    {
        //        ChangeWidthValue = Math.Abs(ChangeWidthValue);
        //        ModifyUCDisplayRect(true);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}


        //public void UCDisplayRectWidthMinus()
        //{
        //    try
        //    {
        //        ChangeWidthValue = -Math.Abs(ChangeWidthValue);
        //        ModifyUCDisplayRect(true);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //public void UCDisplayRectHeightMinus()
        //{
        //    try
        //    {
        //        ChangeHeightValue = -Math.Abs(ChangeHeightValue);
        //        ModifyUCDisplayRect(false);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        //public void UCDisplayRectHeightPlus()
        //{
        //    try
        //    {
        //        ChangeHeightValue = Math.Abs(ChangeHeightValue);
        //        ModifyUCDisplayRect(false);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //        private EventCodeEnum ModifyUCDisplayRect(bool iswidth = true)
        //        {
        //            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //            try
        //            {
        //                PatternSizeWidth = TargetRectangleWidth;
        //                PatternSizeHeight = TargetRectangleHeight;
        //                PatternSizeLeft = TargetRectangleLeft;
        //                PatternSizeTop = TargetRectangleTop;
        //                if (iswidth)
        //                {
        //                    PatternSizeWidth += ChangeWidthValue;
        //                    PatternSizeLeft -= (ChangeWidthValue / 2);
        //                    TargetRectangleWidth = PatternSizeWidth;
        //                    PatternSizeWidth = TargetRectangleWidth;
        //                }
        //                else
        //                {
        //                    PatternSizeHeight += ChangeHeightValue;
        //                    PatternSizeTop -= (ChangeHeightValue / 2);
        //                    TargetRectangleHeight = PatternSizeHeight;
        //                    PatternSizeHeight = TargetRectangleHeight;
        //                }


        //                retVal = EventCodeEnum.NONE;
        //            }
        //            catch (Exception err)
        //            {
        //                LoggerManager.Exception(err);
        //                throw err;
        //            }
        //            return retVal;
        //        }
        //#endregion
        #region ==> Inspection UI에서 사용 할 Light Jog
        //==> Light Jog
        //public LightJogViewModel InspectionLightJog { get; set; }
        //==> Motion Jog
        //public IHexagonJogViewModel InspectionMotionJog { get; set; }
        #endregion

        IDisplayPort DisplayPort { get; set; }
        enum StageCam
        {
            WAFER_HIGH_CAM,
            WAFER_LOW_CAM,
            PIN_HIGH_CAM,
            PIN_LOW_CAM,
        }
        public IWaferObject Wafer
        {
            get
            {
                return (IWaferObject)this.StageSupervisor().WaferObject;
            }
        }
        public ProbingModuleSysParam param
        {
            get { return this.ProbingModule().ProbingModuleSysParam_IParam as ProbingModuleSysParam; }
        }
        private WaferObject WaferInfo { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        //private IZoomObject _ZoomObject;
        //public IZoomObject ZoomObject
        //{
        //    get { return _ZoomObject; }
        //    set { _ZoomObject = value; }
        //}
        //private ILotOPModule _LotOPModule;
        //public ILotOPModule LotOPModule
        //{
        //    get { return _LotOPModule; }
        //    set
        //    {
        //        if (value != _LotOPModule)
        //        {
        //            _LotOPModule = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private int _ManualSetIndexX;

        //public int ManualSetIndexX
        //{
        //    get { return _ManualSetIndexX; }
        //    set
        //    {
        //        if (value != _ManualSetIndexX)
        //        {
        //            _ManualSetIndexX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private int _ManualSetIndexY;

        //public int ManualSetIndexY
        //{
        //    get { return _ManualSetIndexY; }
        //    set
        //    {
        //        if (value != _ManualSetIndexY)
        //        {
        //            _ManualSetIndexY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private bool _ManualSetIndexToggle;

        public bool ManualSetIndexToggle
        {
            get { return _ManualSetIndexToggle; }
            set
            {
                if (value != _ManualSetIndexToggle)
                {
                    _ManualSetIndexToggle = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _PreDutIndex;

        //public int PreDutIndex
        //{
        //    get { return _PreDutIndex; }
        //    set { _PreDutIndex = value; }
        //}


        private int _DutIndex;
        public int DutIndex
        {
            get { return _DutIndex; }
            set
            {
                if (value != _DutIndex)
                {
                    //PreDutIndex = _DutIndex;
                    _DutIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _DUTCount;

        public int DUTCount
        {
            get { return _DUTCount; }
            set
            {
                if (value != _DUTCount)
                {
                    _DUTCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PADCount;

        public int PADCount
        {
            get { return _PADCount; }
            set
            {
                if (value != _PADCount)
                {
                    _PADCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachinePos _MachinePosition;
        public MachinePos MachinePosition
        {
            get { return _MachinePosition; }
            set
            {
                if (value != _MachinePosition)
                {
                    _MachinePosition = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IProbingModule _ProbingModule;
        public IProbingModule ProbingModule
        {
            get { return _ProbingModule; }
            set
            {
                if (value != _ProbingModule)
                {
                    _ProbingModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ICamera _CurCam;
        //public ICamera CurCam
        //{
        //    get { return _CurCam; }
        //    set
        //    {
        //        if (value != _CurCam)
        //        {
        //            _CurCam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set
            {
                if (value != _CoordinateManager)
                {
                    _CoordinateManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IWaferAligner WaferAligner { get; set; }
        private IMotionManager MotionManager { get; set; }
        public IStageSupervisor StageSupervisor { get; set; }
        private IProberStation ProberStation { get; set; }
        public bool Initialized { get; set; } = false;
        //private String _StepLabel;
        //public String StepLabel
        //{
        //    get { return _StepLabel; }
        //    set
        //    {
        //        if (value != _StepLabel)
        //        {
        //            _StepLabel = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private object _ViewTarget;
        //public object ViewTarget
        //{
        //    get { return _ViewTarget; }
        //    set
        //    {
        //        if (value != _ViewTarget)
        //        {
        //            _ViewTarget = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        // private SetupStateBase _SetupState
        //= new NotCompletedState();
        // [XmlIgnore, JsonIgnore]
        // public SetupStateBase SetupState
        // {
        //     get { return _SetupState; }
        //     set
        //     {
        //         if (value != _SetupState)
        //         {
        //             _SetupState = value;
        //             RaisePropertyChanged();
        //         }
        //     }
        // }

        //private IndexCoord _CurIndex;

        //public IndexCoord CurIndex
        //{
        //    get { return _CurIndex; }
        //    set { _CurIndex = value; }
        //}

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void Apply()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void DecreaseX()
        {
            throw new NotImplementedException();
        }

        public void DecreaseY()
        {
            throw new NotImplementedException();
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

        public void IncreaseX()
        {
            throw new NotImplementedException();
        }

        public void IncreaseY()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (Initialized == false)
                    {
                        //InspectionLightJog = new LightJogViewModel(
                        //    maxLightValue: 255,
                        //    minLightValue: 0);
                        //InspectionMotionJog = new HexagonJogViewModel();
                        DutIndex = 0;
                        CurPadIndex = 0;
                        DUTCount = 0;
                        PADCount = 0;
                        ViewPadIndex = 1;
                        ViewDutIndex = 1;
                        //LotOPModule = this.LotOPModule();
                        ProbingModule = this.ProbingModule();
                        CoordinateManager = this.CoordinateManager();
                        WaferAligner = this.WaferAligner();
                        MotionManager = this.MotionManager();
                        StageSupervisor = this.StageSupervisor();
                        ProberStation = this.ProberStation();
                        ManualSetIndexToggle = false;

                        IProbeCard probeCard = this.GetParam_ProbeCard();
                        var PinList = probeCard.GetPinList();
                        var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;

                        //CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        //ZoomObject = Wafer;

                        this.MachinePosition = new MachinePos();


                        //System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        //{
                        //    DisplayPort = new DisplayPort() { GUID = new Guid("43F955CB-C43C-47D3-9B12-0FEE840640ED") };

                        //    Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                        //    if (this.VisionManager().CameraDescriptor != null)
                        //    {
                        //        foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                        //        {
                        //            for (int index = 0; index < stagecamvalues.Length; index++)
                        //            {
                        //                if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                        //                {
                        //                    this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                        //                    break;
                        //                }
                        //            }
                        //        }

                        //        //foreach (var cam in this.VisionManager().CameraDescriptor.Cams)
                        //        //{
                        //        //}
                        //    }

                        //    ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;
                        //    Binding bindX = new Binding
                        //    {
                        //        Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosX"),
                        //        Mode = BindingMode.TwoWay,
                        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        //    };
                        //    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToX, bindX);

                        //    Binding bindY = new Binding
                        //    {
                        //        Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosY"),
                        //        Mode = BindingMode.TwoWay,
                        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        //    };
                        //    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToY, bindY);


                        //    Binding bindCamera = new Binding
                        //    {
                        //        Path = new System.Windows.PropertyPath("CurCam"),
                        //        Mode = BindingMode.TwoWay,
                        //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        //    };
                        //    BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);
                        //}));

                        //ManualSetIndexX = (int)DisplayPort.AssignedCamera.CamSystemUI.XIndex;
                        //ManualSetIndexY = (int)DisplayPort.AssignedCamera.CamSystemUI.YIndex;

                        //ViewTarget = Wafer;
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public void MachinePositionUpdate()
        {
            try
            {
                double xPos = 0;
                double yPos = 0;
                double zPos = 0;
                double tPos = 0;

                MotionManager.GetActualPos(EnumAxisConstants.X, ref xPos);
                MotionManager.GetActualPos(EnumAxisConstants.Y, ref yPos);
                MotionManager.GetActualPos(EnumAxisConstants.Z, ref zPos);
                MotionManager.GetActualPos(EnumAxisConstants.C, ref tPos);
                MachinePosition.ChangePosition(xPos, yPos, zPos, tPos);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

        }
        public void SetFrom()
        {
            throw new NotImplementedException();
        }

        //public void ZoomIn()
        //{
        //    try
        //    {
        //        string Plus = string.Empty;
        //        Wafer.ZoomLevel--;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //public void ZoomOut()
        //{
        //    try
        //    {
        //        string Minus = string.Empty;
        //        Wafer.ZoomLevel++;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        private int _CurPadIndex = 0;
        public int CurPadIndex
        {
            get { return _CurPadIndex; }
            set
            {
                if (value != _CurPadIndex)
                {
                    _CurPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _CurBeforePadIndex = 0;
        public int CurBeforePadIndex
        {
            get { return _CurBeforePadIndex; }
            set
            {
                if (value != _CurBeforePadIndex)
                {
                    _CurBeforePadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ViewPadIndex = 0;
        public int ViewPadIndex
        {
            get { return _ViewPadIndex; }
            set
            {
                if (value != _ViewPadIndex)
                {
                    _ViewPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ViewDutIndex = 0;
        public int ViewDutIndex
        {
            get { return _ViewDutIndex; }
            set
            {
                if (value != _ViewDutIndex)
                {
                    _ViewDutIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _XDutStartIndexPoint = 0;
        public int XDutStartIndexPoint
        {
            get { return _XDutStartIndexPoint; }
            set
            {
                if (value != _XDutStartIndexPoint)
                {
                    _XDutStartIndexPoint = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _YDutStartIndexPoint = 0;
        public int YDutStartIndexPoint
        {
            get { return _YDutStartIndexPoint; }
            set
            {
                if (value != _YDutStartIndexPoint)
                {
                    _YDutStartIndexPoint = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private List<DUTPadObject> _DutPadInfos
        //    = new List<DUTPadObject>();
        //public List<DUTPadObject> DutPadInfos
        //{
        //    get { return _DutPadInfos; }
        //    set
        //    {
        //        if (value != _DutPadInfos)
        //        {
        //            _DutPadInfos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        public void DutStartPoint()
        {
            try
            {
                IProbeCard probeCard = this.GetParam_ProbeCard();

                int Xpos = (int)this.ManualContactModule().MXYIndex.X;
                int Ypos = (int)this.ManualContactModule().MXYIndex.Y;

                if (Xpos == 0 && Ypos == 0)
                {
                    Xpos = (int)Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex;
                    Ypos = (int)Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex;
                }

                XDutStartIndexPoint = Xpos;// + (int)probeCard.ProbeCardDevObjectRef.DutList[0].UserIndex.XIndex;
                YDutStartIndexPoint = Ypos;// + (int)probeCard.ProbeCardDevObjectRef.DutList[0].UserIndex.YIndex;

                DutIndex = 0;
                CurPadIndex = 0;
                DUTCount = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.Count();
                PADCount = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count();
                ViewPadIndex = 1;
                ViewDutIndex = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //public ObservableCollection<IStateModule> ModuleForInspectionCollection { get; set; } = new ObservableCollection<IStateModule>();

        //public Point MXYIndex { get; set; }

        public void PreDut(ICamera camera)
        {
            try
            {
                DUTPadObject padparam = new DUTPadObject();
                var dutList = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.ToString();
                ProberInterfaces.Element<double> Xdutsize = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutSizeX;
                ProberInterfaces.Element<double> Ydutsize = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutSizeY;
                // dutList[1].DutSizeTop.;
                int DutCount = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.Count();
                long Dutnum = padparam.DutNumber;
                if (DutCount <= 1)
                {
                    // 다이얼로그 No Dut Data

                }
                else
                {
                    DutIndex = --DutIndex;
                    if (DutIndex < 0)
                    {
                        DutIndex = DutCount - 1;
                    }
                    ViewDutIndex = DutIndex + 1;

                    DutMove(camera);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void NextDut(ICamera camera)
        {
            try
            {
                DUTPadObject padparam = new DUTPadObject();
                //var dutList = this.ProbeCardData.DutList.ToList();
                var dutList = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.ToString();


                ProberInterfaces.Element<double> Xdutsize = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutSizeX;
                ProberInterfaces.Element<double> Ydutsize = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutSizeY;
                // dutList[1].DutSizeTop.;
                int DutCount = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.Count();
                long Dutnum = padparam.DutNumber;
                if (DutCount <= 1)
                {
                    // 다이얼로그 No Dut Data
                }
                else
                {
                    DutIndex = ++DutIndex % (DutCount);
                    ViewDutIndex = DutIndex + 1;
                    DutMove(camera);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #region Dut Move
        public void DutMove(ICamera camera)   // 더트 이동 함수
        {
            try
            {
                IProbeCard probeCard = this.GetParam_ProbeCard();

                long refdutoffsetX = (long)XDutStartIndexPoint - probeCard.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex;
                long refdutoffsetY = (long)YDutStartIndexPoint - probeCard.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex;

                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                int dieYlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(1);

                long dutXIndex = refdutoffsetX + probeCard.ProbeCardDevObjectRef.DutList[DutIndex].MacIndex.XIndex;
                long dutYIndex = refdutoffsetY + probeCard.ProbeCardDevObjectRef.DutList[DutIndex].MacIndex.YIndex;

                double movezoffset = this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset;
                if (camera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    if (dutXIndex < dieXlength || dutYIndex < dieYlength)
                    {
                        double zpos = 0.0;
                        EventCodeEnum retval = this.WaferAligner().GetHeightValueAddZOffsetFromDutIndex(camera.GetChannelType(), dutXIndex, dutYIndex, movezoffset, out zpos);
                        if (retval == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(dutXIndex, dutYIndex, zpos, true);
                        }
                    }
                }
                else if (camera.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    if (dutXIndex < dieXlength || dutYIndex < dieYlength)
                    {
                        double zpos = 0.0;
                        EventCodeEnum retval = this.WaferAligner().GetHeightValueAddZOffsetFromDutIndex(camera.GetChannelType(), dutXIndex, dutYIndex, movezoffset, out zpos);
                        if (retval == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewIndexMove(dutXIndex, dutYIndex);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. InspectionModule - DutMove() : Error occured.");
            }
        }
        #endregion

        public void PrePad(ICamera camera)
        {
            try
            {
                var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;
                if (DutPadInfos.Count <= 1)
                {
                    //다이얼로그 No Pad Data
                }
                else
                {
                    //CurBeforePadIndex = CurPadIndex;
                    CurPadIndex = --CurPadIndex;
                    if (CurPadIndex < 0)
                    {
                        CurPadIndex = DutPadInfos.Count - 1;
                    }
                    ViewPadIndex = CurPadIndex + 1;
                    EventCodeEnum retVal = PadMove(camera);
                    if(retVal != EventCodeEnum.NONE)
                    {
                        CurPadIndex = ++CurPadIndex;
                        if (CurPadIndex > DutPadInfos.Count - 1)
                        {
                            CurPadIndex = 0;
                        }
                        ViewPadIndex = CurPadIndex + 1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            #region Del
            //var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;
            //if (CurPadIndex + 1 > 0)
            //{
            //    CurPadIndex--;
            //    if (CurPadIndex == 0)
            //    {
            //        CurPadIndex = DutPadInfos.Count;
            //    }

            //    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((int)DutPadInfos[CurPadIndex].MachineIndex.XIndex,
            //                      (int)DutPadInfos[CurPadIndex].MachineIndex.YIndex);

            //    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
            //        this.StageSupervisor().StageModuleState.WaferHighViewMove(
            //            wcoord.GetX() + DutPadInfos[CurPadIndex].PadCenter.X.Value,
            //            wcoord.GetY() + DutPadInfos[CurPadIndex].PadCenter.Y.Value,
            //            Wafer.GetSubsInfo().ActualThickness);
            //    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
            //        this.StageSupervisor().StageModuleState.WaferLowViewMove(
            //            wcoord.GetX() + DutPadInfos[CurPadIndex].PadCenter.X.Value,
            //            wcoord.GetY() + DutPadInfos[CurPadIndex].PadCenter.Y.Value,
            //            Wafer.GetSubsInfo().ActualThickness);

            //    TargetRectangleWidth =
            //        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
            //    TargetRectangleHeight =
            //        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();
            //}

            //StepLabel = $"Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
            #endregion
        }

        public void NextPad(ICamera camera)
        {
            try
            {
                var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;

                if (DutPadInfos.Count <= 1)
                {
                    //다이얼로그 No Pad Data
                }
                else
                {
                    //CurBeforePadIndex = CurPadIndex;
                    CurPadIndex = ++CurPadIndex % DutPadInfos.Count;
                    ViewPadIndex = CurPadIndex + 1;
                    EventCodeEnum retVal = PadMove(camera);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        CurPadIndex = --CurPadIndex % DutPadInfos.Count;
                        ViewPadIndex = CurPadIndex + 1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            #region Del
            //var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;
            //if (CurPadIndex + 1 <= DutPadInfos.Count)
            //{
            //    CurPadIndex++;
            //    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((int)DutPadInfos[CurPadIndex].MachineIndex.XIndex,
            //                       (int)DutPadInfos[CurPadIndex].MachineIndex.YIndex);

            //    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
            //        this.StageSupervisor().StageModuleState.WaferHighViewMove(
            //            wcoord.GetX() + DutPadInfos[CurPadIndex].PadCenter.X.Value,
            //            wcoord.GetY() + DutPadInfos[CurPadIndex].PadCenter.Y.Value,
            //            Wafer.GetSubsInfo().ActualThickness);
            //    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
            //        this.StageSupervisor().StageModuleState.WaferLowViewMove(
            //            wcoord.GetX() + DutPadInfos[CurPadIndex].PadCenter.X.Value,
            //            wcoord.GetY() + DutPadInfos[CurPadIndex].PadCenter.Y.Value,
            //            Wafer.GetSubsInfo().ActualThickness);

            //    TargetRectangleWidth =
            //        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeX.Value, CurCam.Param.GrabSizeX.Value) / CurCam.GetRatioX();
            //    TargetRectangleHeight =
            //        DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeY.Value, CurCam.Param.GrabSizeY.Value) / CurCam.GetRatioY();
            //    if(CurPadIndex +1 > DutPadInfos.Count)
            //    {
            //        CurPadIndex = 0;
            //    }
            //}


            //StepLabel = $"Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
            #endregion
        }
        #region PadMove
        public EventCodeEnum PadMove(ICamera camera)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //IProbeCard probeCard = this.GetParam_ProbeCard();
                //var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;

                //long Xpos = CurCam.GetCurCoordMachineIndex().XIndex;
                //long Ypos = CurCam.GetCurCoordMachineIndex().YIndex;

                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                int dieYlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(1);

                ////long dutXIndex = Xpos + probeCard.ProbeCardDevObjectRef.DutList[DutIndex].UserIndex.XIndex;
                ////long dutYIndex = Ypos + probeCard.ProbeCardDevObjectRef.DutList[DutIndex].UserIndex.YIndex;
                //long dutXIndex = Xpos;
                //long dutYIndex = Ypos;

                //int DutNum = (int)DutPadInfos[CurPadIndex].DutNumber;

                //long XpadIndex = DutPadInfos[CurPadIndex].DutMIndex.XIndex - DutPadInfos[0].DutMIndex.XIndex;
                //long XpadIndey = DutPadInfos[CurPadIndex].DutMIndex.YIndex - DutPadInfos[0].DutMIndex.YIndex;

                //int ZeroPinIndex = (int)PinList[0].DutMacIndex.;

                //add

                //WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();

                //DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorenr(dutXIndex - XpadIndex, dutYIndex - XpadIndey);

                //WaferCoordinate lastpos = new WaferCoordinate
                //(
                //    DieLowLeftCornerPos.X.Value
                //     + Wafer.GetSubsInfo().Pads.DutPadInfos[CurPadIndex].PadCenter.X.Value
                //     + (-param.ProbeMarkShift.Value.X.Value),
                //    DieLowLeftCornerPos.Y.Value
                //     + Wafer.GetSubsInfo().Pads.DutPadInfos[CurPadIndex].PadCenter.Y.Value
                //     + (-param.ProbeMarkShift.Value.Y.Value),
                //    DieLowLeftCornerPos.Z.Value
                //);

                var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;
                IProbeCard probeCard = this.GetParam_ProbeCard();

                long XpadIndex = DutPadInfos[CurPadIndex].DutMIndex.XIndex - DutPadInfos[0].DutMIndex.XIndex;
                long XpadIndey = DutPadInfos[CurPadIndex].DutMIndex.YIndex - DutPadInfos[0].DutMIndex.YIndex;

                //long PadIndexX = probeCard.ProbeCardDevObjectRef.DutList[DutNum - 1].UserIndex.XIndex;
                //long PadIndexY = probeCard.ProbeCardDevObjectRef.DutList[DutNum - 1].UserIndex.YIndex;

                //long PadIndexX = probeCard.ProbeCardDevObjectRef.DutList[DutNum - 1].MacIndex.XIndex;
                //long PadIndexY = probeCard.ProbeCardDevObjectRef.DutList[DutNum - 1].MacIndex.YIndex;

                //WaferCoordinate wcoord2 = this.WaferAligner().MachineIndexConvertToDieLeftCorenr(dutXIndex + XpadIndex, dutYIndex + XpadIndey);
                //WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorenr((long)Xpos + PadIndexX, (long)Ypos + PadIndexY);

                long refdutoffsetX = (long)XDutStartIndexPoint - probeCard.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex;
                long refdutoffsetY = (long)YDutStartIndexPoint - probeCard.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex;

                long mx = refdutoffsetX + DutPadInfos[CurPadIndex].DutMIndex.XIndex;
                long my = refdutoffsetY + DutPadInfos[CurPadIndex].DutMIndex.YIndex;

                if (mx < dieXlength &&
                    my < dieYlength &&
                    mx >= 0 &&
                    my >= 0)
                {
                    WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(mx, my);

                    bool CanMove = false;

                    double xpos = 0;
                    double ypos = 0;
                    double zpos = 0;

                    CanMove = true;

                    xpos = wcoord.GetX() + (DutPadInfos[CurPadIndex].PadCenter.X.Value) + (-(param.ProbeMarkShift.Value.X.Value + param.UserProbeMarkShift.Value.X.Value));
                    ypos = wcoord.GetY() + (DutPadInfos[CurPadIndex].PadCenter.Y.Value) + (-(param.ProbeMarkShift.Value.Y.Value + param.UserProbeMarkShift.Value.Y.Value));
                    zpos = this.WaferAligner().GetHeightValueAddZOffset(xpos, ypos, this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset, true);

                    LoggerManager.Debug($"[InsepctionModule] PadMove() : System PMShift (X,Y) = ({param.ProbeMarkShift.Value.X.Value:0.00},{param.ProbeMarkShift.Value.Y.Value:0.00})" +
                        $"User PMShift (X,Y) = ({param.UserProbeMarkShift.Value.X.Value:0.00}, {param.UserProbeMarkShift.Value.Y.Value:0.00})"
                        +$"Move Z Pos = {zpos:0.00}");

                    if (CanMove)
                    {
                        if (camera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(xpos, ypos, zpos, true);
                        }
                        else if (camera.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(xpos, ypos, zpos, true);
                        }
                    }
                    retVal = EventCodeEnum.NONE;
                    //TargetRectangleWidth = DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeX.Value, camera.Param.GrabSizeX.Value) / camera.GetRatioX();
                    //TargetRectangleHeight = DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeY.Value, camera.Param.GrabSizeY.Value) / camera.GetRatioY();

                    //StepLabel = $"Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
                }
                else
                {
                    // DIE 외부
                    LoggerManager.Debug($"[InspectionModule] PadMove() : Can not move. mx = {mx} ({0} ~ {dieXlength - 1}), my = {my} ({0} ~ {dieYlength - 1})");
                    this.MetroDialogManager().ShowMessageDialog("Pad Move Error", $"Can not move. mx = {mx} ({0} ~ {dieXlength - 1}), my = {my} ({0} ~ {dieYlength - 1})", EnumMessageStyle.Affirmative);
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion 


        public int SavePadImages(ICamera camera)
        {
            int retVal = 0;
            try
            {
                IProbeCard probeCard = this.GetParam_ProbeCard();
                var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;

                int snapIndex = 1;
                long Xpos = camera.GetCurCoordMachineIndex().XIndex;
                long Ypos = camera.GetCurCoordMachineIndex().YIndex;

                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                int dieYlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(1);

                long dutXIndex = Xpos;
                long dutYIndex = Ypos;
                var currCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner(Xpos, Ypos);
                WaferCoordinate curWafPos = null;

                if (camera.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    curWafPos = (WaferCoordinate)this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                }
                else if (camera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    curWafPos = (WaferCoordinate)this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }

                if (curWafPos != null)
                {

                    WaferCoordinate offsetFromLL = new WaferCoordinate(
                        curWafPos.X.Value - currCornerPos.X.Value,
                        curWafPos.Y.Value - currCornerPos.Y.Value);

                    var targetIndices = LoadTargetPadIndices();
                    if (targetIndices.Count == 0)
                    {
                        var pads = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;
                        foreach (var pad in pads)
                        {
                            targetIndices.Add(new MachineIndex(pad.MachineIndex.XIndex, pad.MachineIndex.YIndex));
                        }
                    }

                    var focusingModule = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());
                    var focusingParam = new NormalFocusParameter();
                    focusingParam.SetDefaultParam();

                    if (targetIndices.Count > 0)
                    {
                        foreach (var die in targetIndices)
                        {
                            var mIdx = this.CoordinateManager().UserIndexConvertToMachineIndex(new UserIndex(die.XIndex, die.YIndex));
                            long PadIndexX = die.XIndex;
                            long PadIndexY = die.YIndex;
                            if (PadIndexX >= 0 & PadIndexX < dieXlength &
                                PadIndexY >= 0 & PadIndexY < dieYlength)
                            {
                                WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();
                                WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(PadIndexX, PadIndexY);

                                if (camera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                                {
                                    if (PadIndexX < dieXlength)
                                    {
                                        if (PadIndexY < dieYlength)
                                        {
                                            double xpos = wcoord.GetX() + (offsetFromLL.X.Value);
                                            double ypos = wcoord.GetY() + (offsetFromLL.Y.Value);
                                            double zpos = this.WaferAligner().GetHeightValue(xpos, ypos);

                                            this.StageSupervisor().StageModuleState.WaferHighViewMove(xpos, ypos, zpos);


                                            //this.StageSupervisor().StageModuleState.WaferHighViewMove(wcoord.GetX() + (DutPadInfos[CurPadIndex].PadCenter.X.Value) + (-param.ProbeMarkShift.Value.X.Value),
                                            //                                                  wcoord.GetY() + (DutPadInfos[CurPadIndex].PadCenter.Y.Value) + (-param.ProbeMarkShift.Value.Y.Value),
                                            //                                                  Wafer.GetSubsInfo().ActualThickness);
                                        }
                                    }
                                }
                                else if (camera.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                                {
                                    if (dutXIndex < dieXlength)
                                    {
                                        if (dutYIndex < dieYlength)
                                        {
                                            double xpos = wcoord.GetX() + (offsetFromLL.X.Value);
                                            double ypos = wcoord.GetY() + (offsetFromLL.Y.Value);
                                            double zpos = this.WaferAligner().GetHeightValue(xpos, ypos);

                                            this.StageSupervisor().StageModuleState.WaferLowViewMove(xpos, ypos, zpos);

                                            //this.StageSupervisor().StageModuleState.WaferLowViewMove(wcoord.GetX() + (DutPadInfos[CurPadIndex].PadCenter.X.Value) + (-param.ProbeMarkShift.Value.X.Value),
                                            //                                                  wcoord.GetY() + (DutPadInfos[CurPadIndex].PadCenter.Y.Value) + (-param.ProbeMarkShift.Value.Y.Value),
                                            //                                                  Wafer.GetSubsInfo().ActualThickness);
                                        }
                                    }
                                }

                                focusingParam.FocusingCam.Value = camera.GetChannelType();
                                var focusResult = focusingModule.Focusing_Retry(focusingParam, false, false, false, this);

                                if (focusResult != EventCodeEnum.NONE)
                                {

                                }

                                try
                                {
                                    System.Threading.Thread.Sleep(500);
                                    String imageSaveDirPath = $"{LoggerManager.LoggerManagerParam.FilePath}\\{LoggerManager.LoggerManagerParam.DevFolder}\\ImageLogs";
                                    if (Directory.Exists(imageSaveDirPath) == false)
                                        Directory.CreateDirectory(imageSaveDirPath);
                                    string fileName = "";
                                    var uIdx = this.StageSupervisor().CoordinateManager().MachineIndexConvertToUserIndex(new MachineIndex(PadIndexX, PadIndexY));
                                    fileName = $"{imageSaveDirPath}\\[{snapIndex:000}] {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}_M[{PadIndexX:000},{PadIndexY:000}](U[{uIdx.XIndex:000},{uIdx.YIndex:000}]).png";

                                    ImageBuffer imageBuffer;
                                    camera.GetCurImage(out imageBuffer);
                                    
                                    //TODO : Celine 
                                    if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                                    {
                                        if(imageBuffer != null || imageBuffer.SizeX != 0 || imageBuffer.SizeY != 0)
                                        {
                                            this.VisionManager().SaveImageBuffer(imageBuffer, fileName, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE, imageBuffer.SizeX / 2, imageBuffer.SizeY / 2, imageBuffer.SizeX, imageBuffer.SizeY, 180);
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"SavePadImages() fail. ImageBuffer SizeX : {imageBuffer.SizeX}, ImageBuffer SizeY : {imageBuffer.SizeY}");
                                        }
                                    }
                                    else
                                    {
                                        this.VisionManager().SaveImageBuffer(imageBuffer, fileName, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Debug($"SavePadImages(): Error occurred while image saving. Err = {err.Message}");
                                }
                                snapIndex++;
                            }
                            else
                            {
                                LoggerManager.Debug($"SavePadImages(): Pad is out of available range. X = {PadIndexX}, Y = {PadIndexY}");
                            }
                        }
                        retVal = snapIndex;
                    }

                    //TargetRectangleWidth = DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeX.Value, camera.Param.GrabSizeX.Value) / camera.GetRatioX();
                    //TargetRectangleHeight = DisplayPort.ConvertDisplayWidth(DutPadInfos[CurPadIndex].PadSizeY.Value, camera.Param.GrabSizeY.Value) / camera.GetRatioY();

                    //StepLabel = $"Pad Count : {DutPadInfos.Count}. Cur : {CurPadIndex}";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        private List<IndexCoord> LoadTargetPadIndices()
        {
            List<IndexCoord> resultIndices = new List<IndexCoord>();
            try
            {
                string csv_file_path = $"{LoggerManager.LoggerManagerParam.FilePath}\\{LoggerManager.LoggerManagerParam.DevFolder}\\ImageLogs";
                if (Directory.Exists(csv_file_path) == false)
                    Directory.CreateDirectory(csv_file_path);
                csv_file_path = Path.Combine(csv_file_path, "InspectionDies.csv");
                if (File.Exists(csv_file_path) == true)
                {
                    using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                    {
                        csvReader.SetDelimiters(",");
                        csvReader.HasFieldsEnclosedInQuotes = true;

                        int row = 0;
                        int xIndex, yIndex = 0;
                        while (csvReader.EndOfData == false)
                        {
                            String[] fieldData = csvReader.ReadFields();

                            if (fieldData.Length == 2)
                            {
                                if (int.TryParse(fieldData[0], out xIndex) == true
                                    & int.TryParse(fieldData[1], out yIndex) == true)
                                {
                                    resultIndices.Add(new MachineIndex(xIndex, yIndex));
                                }
                                else
                                {
                                    LoggerManager.Debug($"Field data parsing error. Field = {fieldData.ToString()} @Row: {row}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"Field data parsing error. Field = {fieldData.ToString()} @Row: {row}");
                            }
                            row++;
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"LoadTargetPadIndices(): Target file does not exist.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"LoadTargetPadIndices(): Error occurred. Err = {err.Message}");
            }

            return resultIndices;
        }
    }
}
