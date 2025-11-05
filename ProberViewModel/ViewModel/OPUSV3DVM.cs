using LoaderControllerBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberSimulator;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OPUSV3DViewModel
{
    public class OPUSV3DVM : IMainScreenViewModel
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

        readonly Guid _ViewModelGUID = new Guid("d447db6b-ff94-4826-86b3-1d00ef154d59");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public OPUSV3DVM()
        {
        }

        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get { return _Stage3DModel; }
            set
            {
                if (value != _Stage3DModel)
                {
                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }
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



        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public void DeInitModule()
        {

        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized == false)
            {
                Motion = this.MotionManager();
                LoaderController = this.LoaderController() as ILoaderControllerExtension;

                if (Motion != null)
                {
                    U1 = Motion.GetAxis(EnumAxisConstants.U1);
                    U2 = Motion.GetAxis(EnumAxisConstants.U2);
                    LZ = Motion.GetAxis(EnumAxisConstants.A);
                    LW = Motion.GetAxis(EnumAxisConstants.W);
                    XAxis = Motion.GetAxis(EnumAxisConstants.X);
                    YAxis = Motion.GetAxis(EnumAxisConstants.Y);
                    ZAxis = Motion.GetAxis(EnumAxisConstants.Z);
                }

                DOSCAN_SENSOR_OUT = this.IOManager()?.IO.Outputs.DOSCAN_SENSOR_OUT;

                Stage3DModel = new OPUS5_3D();

                retval = EventCodeEnum.NONE;
            }
            else
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                retval = EventCodeEnum.DUPLICATE_INVOCATION;
            }

            return retval;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
}