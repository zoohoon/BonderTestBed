using LoaderControllerBase;
using LogModule;
using MahApps.Metro.Controls;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;

namespace OPUSV3DView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OPUSV3D : MetroWindow, IMainScreenView, IDisposable
    {

        private static OPUSV3D opus3d;

        public static OPUSV3D GetInstance()
        {
            try
            {
                if (opus3d == null)
                {
                    opus3d = new OPUSV3D();
                }
                return opus3d;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        //public CameraViewPositionType CameraViewPositionType
        //{
        //    get { return (CameraViewPositionType)GetValue(CameraViewPositionTypeProperty); }
        //    set
        //    {
        //        SetValue(CameraViewPositionTypeProperty, value);
        //    }
        //}
        ////public static readonly DependencyProperty CameraViewPositionTypeProperty =
        //                DependencyProperty.Register(nameof(CameraViewPositionType),
        //                typeof(CameraViewPositionType), typeof(Viewer3DModelV),
        //                new FrameworkPropertyMetadata(new PropertyChangedCallback(CameraViewPositionTypeChanged)));

        //private static void CameraViewPositionTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    //Viewer3DModelV test = sender as Viewer3DModelV;

        //    opus3d?.CameraViewPosition(sender, e);
        //}


        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        readonly string _ViewModelType = "IOPUSV3DViewModel";
        public string ViewModelType { get { return _ViewModelType; } }

        readonly Guid _ViewGUID = new Guid("37423616-e201-4699-8163-83c10f05fcfb");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private Point3D _CamPosition = new Point3D(0, 785.84, 1255.72);
        public Point3D CamPosition
        {
            get { return _CamPosition; }
            set
            {
                if (value != _CamPosition)
                {
                    _CamPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Vector3D _CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
        public Vector3D CamLookDirection
        {
            get { return _CamLookDirection; }
            set
            {
                if (value != _CamLookDirection)
                {
                    _CamLookDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Vector3D _CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
        public Vector3D CamUpDirection
        {
            get { return _CamUpDirection; }
            set
            {
                if (value != _CamUpDirection)
                {
                    _CamUpDirection = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int ViewNUM;
        public int _ViewNUM
        {
            get { return _ViewNUM; }
            set
            {
                if (value != _ViewNUM)
                {
                    _ViewNUM = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ViewNUMx2;
        public bool ViewNUMx2
        {
            get { return _ViewNUMx2; }
            set
            {
                if (value != _ViewNUMx2)
                {
                    _ViewNUMx2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IForceMeasure _ZFM;
        public IForceMeasure ZFM
        {
            get { return _ZFM; }
            set
            {
                if (value != _ZFM)
                {
                    _ZFM = value;
                    RaisePropertyChanged();
                }
            }
        }


        public IStageSupervisor StageSupervisor { get { return this.StageSupervisor(); } }
        public IMotionManager MotionManager { get; set; }

        private IMonitoringManager _MonitoringManager;
        public IMonitoringManager MonitoringManager
        {
            get { return _MonitoringManager; }
            set
            {
                if (value != _MonitoringManager)
                {
                    _MonitoringManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        public OPUSV3D()
        {
            InitializeComponent();
            DataContext = this;
            CenterViewChange();
            ViewNUM = 0;
            //   this.Stage3DModel = new Viewer3DModelV();
            //(Stage3DModel as UserControl).DataContext = this;

            //  (Stage3DModel as UserControl).DataContext = this;
            Motion = this.MotionManager();
            LoaderController = this.LoaderController() as ILoaderControllerExtension;
            MotionManager = this.MotionManager();
            ZFM = this.GetForceMeasure();

            if (Motion != null)
            {
                U2 = Motion.GetAxis(EnumAxisConstants.U2);
                LZ = Motion.GetAxis(EnumAxisConstants.A);
                LW = Motion.GetAxis(EnumAxisConstants.W);
                XAxis = Motion.GetAxis(EnumAxisConstants.X);
                YAxis = Motion.GetAxis(EnumAxisConstants.Y);
                ZAxis = Motion.GetAxis(EnumAxisConstants.Z);
                TAxis = Motion.GetAxis(EnumAxisConstants.C);
                MNCAxis = Motion.GetAxis(EnumAxisConstants.NC);
                PZAxis = Motion.GetAxis(EnumAxisConstants.PZ);
                SC = Motion.GetAxis(EnumAxisConstants.SC);
                U1 = Motion.GetAxis(EnumAxisConstants.U1);
            }
            
            MonitoringManager = this.MonitoringManager();
            ThreeLegHeight = 0.0d;
            FoupCoverHeight = -380d;
            FoupCoverPos = -40d;
            ViewNUM = 0;

            ZUpState = this.ProbingModule().ProbingStateEnum;
            FoupController = this.FoupOpModule()?.GetFoupController(1);
            IsWaferBridgeExtended = this.IOManager()?.IO.Outputs.DOWAFERMIDDLE;

            CenterViewChange();

            DOSCAN_SENSOR_OUT = this.IOManager()?.IO.Outputs.DOSCAN_SENSOR_OUT;
            DOLOADER_DOOR = this.IOManager()?.IO.Outputs.DOLOADERDOOR_OPEN;
        }

        private double _FoupCoverPos;
        public double FoupCoverPos
        {
            get { return _FoupCoverPos; }
            set
            {
                if (value != _FoupCoverPos)
                {
                    _FoupCoverPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FoupCoverHeight;
        public double FoupCoverHeight
        {
            get { return _FoupCoverHeight; }
            set
            {
                if (value != _FoupCoverHeight)
                {
                    _FoupCoverHeight = value;
                    RaisePropertyChanged();
                }
            }
        }



        //private IStage3DModel _Stage3DModel;
        //public IStage3DModel Stage3DModel
        //{
        //    get { return _Stage3DModel; }
        //    set
        //    {
        //        if (value != _Stage3DModel)
        //        {
        //            _Stage3DModel = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        #region ==> Motion
        private IMotionManager _Motion;
        public IMotionManager Motion
        {
            get { return _Motion; }
            set
            {
                if (value != _Motion)
                {
                    _Motion = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> U1
        private ProbeAxisObject _U1;
        public ProbeAxisObject U1
        {
            get { return _U1; }
            set
            {
                if (value != _U1)
                {
                    _U1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> U2
        private ProbeAxisObject _U2;
        public ProbeAxisObject U2
        {
            get { return _U2; }
            set
            {
                if (value != _U2)
                {
                    _U2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LZ
        private AxisObject _LZ;
        public AxisObject LZ
        {
            get { return _LZ; }
            set
            {
                if (value != _LZ)
                {
                    _LZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LW
        private ProbeAxisObject _LW;
        public ProbeAxisObject LW
        {
            get { return _LW; }
            set
            {
                if (value != _LW)
                {
                    _LW = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> XAxis
        private ProbeAxisObject _XAxis;
        public ProbeAxisObject XAxis
        {
            get { return _XAxis; }
            set
            {
                if (value != _XAxis)
                {
                    _XAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> YAxis
        private ProbeAxisObject _YAxis;
        public ProbeAxisObject YAxis
        {
            get { return _YAxis; }
            set
            {
                if (value != _YAxis)
                {
                    _YAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZAxis
        private ProbeAxisObject _ZAxis;
        public ProbeAxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> TAxis
        private ProbeAxisObject _TAxis;
        public ProbeAxisObject TAxis
        {
            get { return _TAxis; }
            set
            {
                if (value != _TAxis)
                {
                    _TAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        private IFoupController _FoupController;
        public IFoupController FoupController
        {
            get { return _FoupController; }
            set
            {
                if (value != _FoupController)
                {
                    _FoupController = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> DOSCAN_SENSOR_OUT
        private IOPortDescripter<bool> _DOSCAN_SENSOR_OUT;
        public IOPortDescripter<bool> DOSCAN_SENSOR_OUT
        {
            get { return _DOSCAN_SENSOR_OUT; }
            set
            {
                if (value != _DOSCAN_SENSOR_OUT)
                {
                    _DOSCAN_SENSOR_OUT = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> DOLOADER_DOOR
        private IOPortDescripter<bool> _DOLOADER_DOOR;
        public IOPortDescripter<bool> DOLOADER_DOOR
        {
            get { return _DOLOADER_DOOR; }
            set
            {
                if (value != _DOLOADER_DOOR)
                {
                    _DOLOADER_DOOR = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ProbeAxisObject _PZAxis;
        public ProbeAxisObject PZAxis
        {
            get { return _PZAxis; }
            set
            {
                if (value != _PZAxis)
                {
                    _PZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _MNCAxis;
        public ProbeAxisObject MNCAxis
        {
            get { return _MNCAxis; }
            set
            {
                if (value != _MNCAxis)
                {
                    _MNCAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _SC;
        public ProbeAxisObject SC
        {
            get { return _SC; }
            set
            {
                if (value != _SC)
                {
                    _SC = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProbingState _ZUpState;
        public EnumProbingState ZUpState
        {
            get { return _ZUpState; }
            set
            {
                if (value != _ZUpState)
                {
                    _ZUpState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _IsWaferBridgeExtended;
        public IOPortDescripter<bool> IsWaferBridgeExtended
        {
            get { return _IsWaferBridgeExtended; }
            set
            {
                if (value != _IsWaferBridgeExtended)
                {
                    _IsWaferBridgeExtended = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ==> ThreeLegHeight
        private double _ThreeLegHeight;
        public double ThreeLegHeight
        {
            get { return _ThreeLegHeight; }
            set
            {
                if (value != _ThreeLegHeight)
                {
                    _ThreeLegHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ILoaderControllerExtension _LoaderController;
        public ILoaderControllerExtension LoaderController
        {
            get { return _LoaderController; }
            set
            {
                if (value != _LoaderController)
                {
                    _LoaderController = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void Viewx2() // 2x view
        {
            try
            {
                ViewNUMx2 = !ViewNUMx2;
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void CWViewChange() //ClockWise
        {

            try
            {
                ViewNUM = ViewNUM - 1;
                if (ViewNUM < 0)
                {
                    ViewNUM = 7;
                }
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void CenterViewChange() //FRONT
        {
            try
            {
                ViewNUM = 0;
                ViewNUMx2 = false;
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void CCWViewChange() // CounterClockWise
        {
            try
            {
                ViewNUM = ViewNUM + 1;
                if (ViewNUM > 7)
                {
                    ViewNUM = 0;
                }
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void ViewerCont()
        {
            try
            {
                if (ViewNUMx2 == false)
                {
                    if (ViewNUM == 0) // FRONT VIEW
                    {
                        CamPosition = new Point3D(121.4, 1487.7, 1830.4);
                        CamLookDirection = new Vector3D(0, -0.7, -0.8);
                        CamUpDirection = new Vector3D(0, 0.8, -0.6);
                    }
                    else if (ViewNUM == 1) // FOUP
                    {
                        CamPosition = new Point3D(1419.1, 1487.7, 1298.2);
                        CamLookDirection = new Vector3D(-0.5, -0.6, -0.6);
                        CamUpDirection = new Vector3D(-0.4, 0.8, -0.4);
                    }
                    else if (ViewNUM == 2) // LOADER
                    {
                        CamPosition = new Point3D(1960.4, 1487.7, 4.2);
                        CamLookDirection = new Vector3D(-0.8, -0.6, 0);
                        CamUpDirection = new Vector3D(-0.6, 0.8, 0);
                    }
                    else if (ViewNUM == 3) // LOADER BACK
                    {
                        CamPosition = new Point3D(1535.8, 1487.7, -1175.2);
                        CamLookDirection = new Vector3D(-0.6, -0.6, 0.5);
                        CamUpDirection = new Vector3D(-0.5, 0.8, 0.4);
                    }
                    else if (ViewNUM == 4) // BACK
                    {
                        CamPosition = new Point3D(134.3, 1487.7, -1834.7);
                        CamLookDirection = new Vector3D(0, -0.6, 0.8);
                        CamUpDirection = new Vector3D(0, 0.8, 0.6);
                    }
                    else if (ViewNUM == 5) // STAGE BACK
                    {
                        CamPosition = new Point3D(-1271.8, 1487.7, -1185);
                        CamLookDirection = new Vector3D(0.6, -0.6, 0.5);
                        CamUpDirection = new Vector3D(0.5, 0.8, 0.4);
                    }
                    else if (ViewNUM == 6) // STAGE 
                    {
                        CamPosition = new Point3D(-1704.7, 1487.7, -8.6);
                        CamLookDirection = new Vector3D(0.8, -0.6, 0);
                        CamUpDirection = new Vector3D(0.6, 0.8, 0);
                    }
                    else if (ViewNUM == 7) // STAGE 
                    {
                        CamPosition = new Point3D(-1055, 1487.7, 1397.5);
                        CamLookDirection = new Vector3D(0.5, -0.6, -0.6);
                        CamUpDirection = new Vector3D(0.4, 0.8, -0.5);
                    }
                    else
                    {

                    }
                }
                else if (ViewNUMx2 == true)
                {
                    if (ViewNUM == 0) // FRONT VIEW x2
                    {
                        CamPosition = new Point3D(123, 1148, 1412);
                        CamLookDirection = new Vector3D(0.0, -0.6, -0.8);
                        CamUpDirection = new Vector3D(0.0, 0.8, -0.6);
                    }
                    else if (ViewNUM == 1) // FOUP x2
                    {
                        CamPosition = new Point3D(1208, 1148, 911);
                        CamLookDirection = new Vector3D(-0.6, -0.6, -0.5);
                        CamUpDirection = new Vector3D(-0.5, 0.8, -0.4);
                    }
                    else if (ViewNUM == 2) // LOADER x2
                    {
                        CamPosition = new Point3D(1542, 1148, 3);
                        CamLookDirection = new Vector3D(-0.8, -0.6, 0.0);
                        CamUpDirection = new Vector3D(-0.6, 0.8, 0.0);
                    }
                    else if (ViewNUM == 3) // LOADER BACK x2
                    {
                        CamPosition = new Point3D(1041, 1148, -1083);
                        CamLookDirection = new Vector3D(-0.5, -0.6, 0.6);
                        CamUpDirection = new Vector3D(-0.4, 0.8, 0.5);
                    }
                    else if (ViewNUM == 4) // BACK x2
                    {
                        CamPosition = new Point3D(133, 1148, -1417);
                        CamLookDirection = new Vector3D(0.0, -0.6, 0.8);
                        CamUpDirection = new Vector3D(0.0, 0.8, 0.6);
                    }
                    else if (ViewNUM == 5) // STAGE BACK x2
                    {
                        CamPosition = new Point3D(-1095, 1148, -714);
                        CamLookDirection = new Vector3D(0.7, -0.6, 0.4);
                        CamUpDirection = new Vector3D(0.5, 0.8, 0.3);
                    }
                    else if (ViewNUM == 6) // STAGE x2
                    {
                        CamPosition = new Point3D(-1287, 1148, -7);
                        CamLookDirection = new Vector3D(0.8, -0.6, 0.0);
                        CamUpDirection = new Vector3D(0.6, 0.8, 0.0);
                    }
                    else if (ViewNUM == 7) // STAGE  x2
                    {
                        CamPosition = new Point3D(-876, 1148, 994);
                        CamLookDirection = new Vector3D(0.6, -0.6, -0.5);
                        CamUpDirection = new Vector3D(0.4, 0.8, -0.4);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        private void X2ViewChangeCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                Viewx2();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void CWViewChangeCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                CWViewChange();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void CenterViewChangeCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                CenterViewChange();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void CCWViewChangeCommand(object sender, RoutedEventArgs e)
        {
            try
            {
                CCWViewChange();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        public void Dispose()
        {
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}