using Autofac;
using CylType;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using RelayCommandBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VirtualKeyboardControl;
using LoaderControllerBase;
using SubstrateObjects;
using ProberInterfaces.PnpSetup;
using MetroDialogInterfaces;
using ProberInterfaces.State;
using ProberViewModel.Data;
using System.Threading;
using BVisionTestViewModel;
using System.Diagnostics;
using System.Windows;
using ManualJogView; // Window가 있는 네임스페이스 확인
using System.IO;              // StreamReader용
using System.Net.Sockets;     // NetworkStream, TcpClient용
using System.Text;            // Encoding용

//260708 Remy 
using ProberViewModel.ViewModel;
using CUI;
using PI; // PI_GCS2 라이브러리 (MOV, IsMoving 등 사용)
using System.Net;
//using ProberInterfaces.ThreadSync;

namespace ManualJogViewModel
{

    public class AxisObjectVM : INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    public class MotionText : IEquatable<MotionText>
    {

        public int MotionTextvalue { get; set; }

        public override string ToString()
        {
            return "Value: " + MotionTextvalue;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MotionText objAsPart = obj as MotionText;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return MotionTextvalue;
        }
        public bool Equals(MotionText other)
        {
            if (other == null) return false;
            return (this.MotionTextvalue.Equals(other.MotionTextvalue));
        }
    }


    public sealed class EcatIoWorker : IDisposable
    {
        private readonly BlockingCollection<Action> _queue;
        private readonly Thread _thread;

        public EcatIoWorker()
        {
            _queue = new BlockingCollection<Action>();

            _thread = new Thread(() =>
            {
                foreach (Action act in _queue.GetConsumingEnumerable())
                {
                    try
                    {
                        act();
                    }
                    catch (Exception ex)
                    {
                        // Fire-and-forget IO 예외는 로그만 남기고 삼킴
                        LoggerManager.Exception(ex);
                    }
                }
            });

            _thread.IsBackground = true;
            _thread.Name = "ECAT-IO-WORKER";
            _thread.Start();
        }

        /// <summary>
        /// IO 요청만 큐에 넣고 즉시 리턴 (No Waiting)
        /// </summary>
        public void Post(Action action)
        {
            try
            {
                if (!_queue.IsAddingCompleted)
                {
                    _queue.Add(action);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
            }
        }
        public void PostAfter(Action action, int delayMs)
        {
            Timer timer = null;

            timer = new Timer(_ =>
            {
                try
                {
                    Post(action);   // delay 후 다시 큐에 넣기
                }
                catch (Exception ex)
                {
                    LoggerManager.Exception(ex);
                }
                finally
                {
                    if (timer != null)
                        timer.Dispose();
                }
            },
            null,
            delayMs,
            Timeout.Infinite);
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _queue.Dispose();
        }
    }

    public class ManualJogViewModelBase : IMainScreenViewModel, IFactoryModule, INotifyPropertyChanged, ISetUpState
    {
        //260708 remy
        private NanoStageViewModel _nanoStage3AxisVM = new NanoStageViewModel();
        private NanoStage6AxisViewModel _nanoStage6AxisVM = new NanoStage6AxisViewModel();

        protected TcpClient _connectedClient;
        protected TcpListener _server;
        protected bool _isWaitingForResponse = false;

        // 클래스 전체에서 공유되는 Vision VM
        private BVisionTestViewModelBase _VisionVM;

        // 외부에서 접근 가능하도록 속성
        public BVisionTestViewModelBase VisionVM
        {
            get => _VisionVM;
            set
            {
                if (_VisionVM != value)
                {
                    _VisionVM = value;
                }
            }
        }

        // 기존 Vision VM을 주입하는 static 메서드
        public void Set5Cam(BVisionTestViewModelBase visionVM)
        {
            _VisionVM = visionVM;
        }

        //251125 ybpark Arm Picker 위치 받아서 index 위치로 이동(Map Die)
        double FirstDiePos_X = 0.0;
        double FirstDiePos_Y = 0.0;

        int TestCount = 1;
        public bool ReverseRun = false;    // 251224 sebas : 반대움직임을 판단할 기준 값

        readonly Guid _ViewModelGUID = new Guid("A9796E36-D6D8-6EA1-349B-6E5E30A90E68");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public ILoaderControllerExtension LoaderController { get; set; }
        public bool Initialized { get; set; } = false;


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private readonly EcatIoWorker _ecatIo = new EcatIoWorker();

        private StageState _StageMove;
        public StageState StageMove
        {
            get { return _StageMove; }
            set { _StageMove = value; }
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
        public WaferObject Wafer { get { return this.StageSupervisor().WaferObject as WaferObject; } }

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

        #region // Properties
        private ObservableCollection<IOPortDescripter<bool>> _OutputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> OutputPorts
        {
            get { return _OutputPorts; }
            set
            {
                if (value != _OutputPorts)
                {
                    _OutputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _InputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> InputPorts
        {
            get { return _InputPorts; }
            set
            {
                if (value != _InputPorts)
                {
                    _InputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private LockKey outPortLock = new LockKey("Manual jog VM - out port");
        private object outPortLock = new object();

        private ObservableCollection<IOPortDescripter<bool>> _FilteredOutputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredOutputPorts
        {
            get { return _FilteredOutputPorts; }
            set
            {
                if (value != _FilteredOutputPorts)
                {
                    _FilteredOutputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private LockKey inPortLock = new LockKey("Manual jog VM - in port");
        private object inPortLock = new object();

        private ObservableCollection<IOPortDescripter<bool>> _FilteredInputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredInputPorts
        {
            get { return _FilteredInputPorts; }
            set
            {
                if (value != _FilteredInputPorts)
                {
                    _FilteredInputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _SearchKeyword = string.Empty;
        public string SearchKeyword
        {
            get { return _SearchKeyword; }
            set
            {
                if (value != _SearchKeyword)
                {
                    _SearchKeyword = value;
                    RaisePropertyChanged();
                    SearchMatched();
                }
            }
        }


        private int _LightValue;
        public int LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                    UpdateLight();
                }
            }
        }
        //private int _SelectedLightChannel;
        //public int SelectedLightChannel
        //{
        //    get { return _SelectedLightChannel; }
        //    set
        //    {
        //        if (value != _SelectedLightChannel)
        //        {
        //            _SelectedLightChannel = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<LightChannelType> _Lights
            = new ObservableCollection<LightChannelType>();
        public ObservableCollection<LightChannelType> Lights
        {
            get { return _Lights; }
            set
            {
                if (value != _Lights)
                {
                    _Lights = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<CameraChannelType> _CamChannels = new ObservableCollection<CameraChannelType>();
        public ObservableCollection<CameraChannelType> CamChannels
        {
            get { return _CamChannels; }
            set
            {
                if (value != _CamChannels)
                {
                    _CamChannels = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CameraChannelType _SelectedChannel;
        public CameraChannelType SelectedChannel
        {
            get { return _SelectedChannel; }
            set
            {
                if (value != _SelectedChannel)
                {
                    _SelectedChannel = value;
                    RaisePropertyChanged();
                }
            }
        }


        private LightChannelType _SelectedLight;
        public LightChannelType SelectedLight
        {
            get { return _SelectedLight; }
            set
            {
                if (value != _SelectedLight)
                {
                    _SelectedLight = value;
                    RaisePropertyChanged();
                }
            }
        }


        private RelayCommand _SearchTextChangedCommand;
        public ICommand SearchTextChangedCommand
        {
            get
            {
                if (null == _SearchTextChangedCommand) _SearchTextChangedCommand = new RelayCommand(SearchMatched);
                return _SearchTextChangedCommand;
            }
        }


        private RelayCommand<object> _ChannelChangeCommand;
        public ICommand ChannelChangeCommand
        {
            get
            {
                if (null == _ChannelChangeCommand) _ChannelChangeCommand = new RelayCommand<object>(ChangeChannel);
                return _ChannelChangeCommand;
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

        //private RelayCommand _OuputOffCommand;
        //public ICommand OuputOffCommand
        //{
        //    get
        //    {
        //        if (null == _OuputOffCommand) _OuputOffCommand = new RelayCommand(OuputOff);
        //        return _OuputOffCommand;
        //    }
        //}

        //private void OuputOff()
        //{
        //    throw new NotImplementedException();
        //}

        //private RelayCommand _OutputOnCommand;
        //public ICommand OutputOnCommand
        //{
        //    get
        //    {
        //        if (null == _OutputOnCommand) _OutputOnCommand = new RelayCommand(OutputOn);
        //        return _OutputOnCommand;
        //    }
        //}

        //private void OutputOn()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
        ILightAdmin light;
        public ManualJogViewModelBase()
        {
            SearchKeyword = "";
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

        private ObservableCollection<AxisObjectVM> _LoaderAxisObjectVmList
            = new ObservableCollection<AxisObjectVM>();
        public ObservableCollection<AxisObjectVM> LoaderAxisObjectVmList
        {
            get { return _LoaderAxisObjectVmList; }
            set
            {
                if (value != _LoaderAxisObjectVmList)
                {
                    _LoaderAxisObjectVmList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _axis;
        public ProbeAxisObject axis
        {
            get { return _axis; }
            set
            {
                if (value != _axis)
                {
                    _axis = value;
                    RaisePropertyChanged();
                }
            }
        }
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
        #region Move ButtonCommand

        #region Stage Move
        private AsyncCommand _XPosMoveCommand;
        public ICommand XPosMoveCommand
        {
            get
            {
                if (null == _XPosMoveCommand) _XPosMoveCommand = new AsyncCommand(XPosMoveFunc);
                return _XPosMoveCommand;
            }
        }
        private AsyncCommand _XNegMoveCommand;
        public ICommand XNegMoveCommand
        {
            get
            {
                if (null == _XNegMoveCommand) _XNegMoveCommand = new AsyncCommand(XNegMoveFunc);
                return _XNegMoveCommand;
            }
        }
        private AsyncCommand _YPosMoveCommand;
        public ICommand YPosMoveCommand
        {
            get
            {
                if (null == _YPosMoveCommand) _YPosMoveCommand = new AsyncCommand(YPosMoveFunc);
                return _YPosMoveCommand;
            }
        }
        private AsyncCommand _YNegMoveCommand;
        public ICommand YNegMoveCommand
        {
            get
            {
                if (null == _YNegMoveCommand) _YNegMoveCommand = new AsyncCommand(YNegMoveFunc);
                return _YNegMoveCommand;
            }
        }
        private AsyncCommand _ZPosMoveCommand;
        public ICommand ZPosMoveCommand
        {
            get
            {
                if (null == _ZPosMoveCommand) _ZPosMoveCommand = new AsyncCommand(ZPosMoveFunc);
                return _ZPosMoveCommand;
            }
        }
        private AsyncCommand _ZNegMoveCommand;
        public ICommand ZNegMoveCommand
        {
            get
            {
                if (null == _ZNegMoveCommand) _ZNegMoveCommand = new AsyncCommand(ZNegMoveFunc);
                return _ZNegMoveCommand;
            }
        }
        private AsyncCommand _CPosMoveCommand;
        public ICommand CPosMoveCommand
        {
            get
            {
                if (null == _CPosMoveCommand) _CPosMoveCommand = new AsyncCommand(CPosMoveFunc);
                return _CPosMoveCommand;
            }
        }
        private AsyncCommand _CNegMoveCommand;
        public ICommand CNegMoveCommand
        {
            get
            {
                if (null == _CNegMoveCommand) _CNegMoveCommand = new AsyncCommand(CNegMoveFunc);
                return _CNegMoveCommand;
            }
        }
        private AsyncCommand _TriPosMoveCommand;
        public ICommand TriPosMoveCommand
        {
            get
            {
                if (null == _TriPosMoveCommand) _TriPosMoveCommand = new AsyncCommand(TriPosMoveFunc);
                return _TriPosMoveCommand;
            }
        }
        private AsyncCommand _TriNegMoveCommand;
        public ICommand TriNegMoveCommand
        {
            get
            {
                if (null == _TriNegMoveCommand) _TriNegMoveCommand = new AsyncCommand(TriNegMoveFunc);
                return _TriNegMoveCommand;
            }
        }
        private AsyncCommand _PzPosMoveCommand;
        public ICommand PzPosMoveCommand
        {
            get
            {
                if (null == _PzPosMoveCommand) _PzPosMoveCommand = new AsyncCommand(PzPosMoveFunc);
                return _PzPosMoveCommand;
            }
        }
        private AsyncCommand _PzNegMoveCommand;
        public ICommand PzNegMoveCommand
        {
            get
            {
                if (null == _PzNegMoveCommand) _PzNegMoveCommand = new AsyncCommand(PzNegMoveFunc);
                return _PzNegMoveCommand;
            }
        }

        //251029 yb add
        private AsyncCommand _CX1PosMoveCommand;
        public ICommand CX1PosMoveCommand
        {
            get
            {
                if (null == _CX1PosMoveCommand) _CX1PosMoveCommand = new AsyncCommand(CX1PosMoveFunc);
                return _CX1PosMoveCommand;
            }
        }
        private AsyncCommand _CX1NegMoveCommand;
        public ICommand CX1NegMoveCommand
        {
            get
            {
                if (null == _CX1NegMoveCommand) _CX1NegMoveCommand = new AsyncCommand(CX1NegMoveFunc);
                return _CX1NegMoveCommand;
            }
        }
        private AsyncCommand _CY1PosMoveCommand;
        public ICommand CY1PosMoveCommand
        {
            get
            {
                if (null == _CY1PosMoveCommand) _CY1PosMoveCommand = new AsyncCommand(CY1PosMoveFunc);
                return _CY1PosMoveCommand;
            }
        }
        private AsyncCommand _CY1NegMoveCommand;
        public ICommand CY1NegMoveCommand
        {
            get
            {
                if (null == _CY1NegMoveCommand) _CY1NegMoveCommand = new AsyncCommand(CY1NegMoveFunc);
                return _CY1NegMoveCommand;
            }
        }
        private AsyncCommand _CZ1PosMoveCommand;
        public ICommand CZ1PosMoveCommand
        {
            get
            {
                if (null == _CZ1PosMoveCommand) _CZ1PosMoveCommand = new AsyncCommand(CZ1PosMoveFunc);
                return _CZ1PosMoveCommand;
            }
        }
        private AsyncCommand _CZ1NegMoveCommand;
        public ICommand CZ1NegMoveCommand
        {
            get
            {
                if (null == _CZ1NegMoveCommand) _CZ1NegMoveCommand = new AsyncCommand(CZ1NegMoveFunc);
                return _CZ1NegMoveCommand;
            }
        }

        private AsyncCommand _CX2PosMoveCommand;
        public ICommand CX2PosMoveCommand
        {
            get
            {
                if (null == _CX2PosMoveCommand) _CX2PosMoveCommand = new AsyncCommand(CX2PosMoveFunc);
                return _CX2PosMoveCommand;
            }
        }
        private AsyncCommand _CX2NegMoveCommand;
        public ICommand CX2NegMoveCommand
        {
            get
            {
                if (null == _CX2NegMoveCommand) _CX2NegMoveCommand = new AsyncCommand(CX2NegMoveFunc);
                return _CX2NegMoveCommand;
            }
        }
        private AsyncCommand _CY2PosMoveCommand;
        public ICommand CY2PosMoveCommand
        {
            get
            {
                if (null == _CY2PosMoveCommand) _CY2PosMoveCommand = new AsyncCommand(CY2PosMoveFunc);
                return _CY2PosMoveCommand;
            }
        }
        private AsyncCommand _CY2NegMoveCommand;
        public ICommand CY2NegMoveCommand
        {
            get
            {
                if (null == _CY2NegMoveCommand) _CY2NegMoveCommand = new AsyncCommand(CY2NegMoveFunc);
                return _CY2NegMoveCommand;
            }
        }
        private AsyncCommand _CZ2PosMoveCommand;
        public ICommand CZ2PosMoveCommand
        {
            get
            {
                if (null == _CZ2PosMoveCommand) _CZ2PosMoveCommand = new AsyncCommand(CZ2PosMoveFunc);
                return _CZ2PosMoveCommand;
            }
        }
        private AsyncCommand _CZ2NegMoveCommand;
        public ICommand CZ2NegMoveCommand
        {
            get
            {
                if (null == _CZ2NegMoveCommand) _CZ2NegMoveCommand = new AsyncCommand(CZ2NegMoveFunc);
                return _CZ2NegMoveCommand;
            }
        }

        private AsyncCommand _CX3PosMoveCommand;
        public ICommand CX3PosMoveCommand
        {
            get
            {
                if (null == _CX3PosMoveCommand) _CX3PosMoveCommand = new AsyncCommand(CX3PosMoveFunc);
                return _CX3PosMoveCommand;
            }
        }
        private AsyncCommand _CX3NegMoveCommand;
        public ICommand CX3NegMoveCommand
        {
            get
            {
                if (null == _CX3NegMoveCommand) _CX3NegMoveCommand = new AsyncCommand(CX3NegMoveFunc);
                return _CX3NegMoveCommand;
            }
        }
        private AsyncCommand _CY3PosMoveCommand;
        public ICommand CY3PosMoveCommand
        {
            get
            {
                if (null == _CY3PosMoveCommand) _CY3PosMoveCommand = new AsyncCommand(CY3PosMoveFunc);
                return _CY3PosMoveCommand;
            }
        }
        private AsyncCommand _CY3NegMoveCommand;
        public ICommand CY3NegMoveCommand
        {
            get
            {
                if (null == _CY3NegMoveCommand) _CY3NegMoveCommand = new AsyncCommand(CY3NegMoveFunc);
                return _CY3NegMoveCommand;
            }
        }
        private AsyncCommand _CZ3PosMoveCommand;
        public ICommand CZ3PosMoveCommand
        {
            get
            {
                if (null == _CZ3PosMoveCommand) _CZ3PosMoveCommand = new AsyncCommand(CZ3PosMoveFunc);
                return _CZ3PosMoveCommand;
            }
        }
        private AsyncCommand _CZ3NegMoveCommand;
        public ICommand CZ3NegMoveCommand
        {
            get
            {
                if (null == _CZ3NegMoveCommand) _CZ3NegMoveCommand = new AsyncCommand(CZ3NegMoveFunc);
                return _CZ3NegMoveCommand;
            }
        }

        private AsyncCommand _CX4PosMoveCommand;
        public ICommand CX4PosMoveCommand
        {
            get
            {
                if (null == _CX4PosMoveCommand) _CX4PosMoveCommand = new AsyncCommand(CX4PosMoveFunc);
                return _CX4PosMoveCommand;
            }
        }
        private AsyncCommand _CX4NegMoveCommand;
        public ICommand CX4NegMoveCommand
        {
            get
            {
                if (null == _CX4NegMoveCommand) _CX4NegMoveCommand = new AsyncCommand(CX4NegMoveFunc);
                return _CX4NegMoveCommand;
            }
        }
        private AsyncCommand _CY4PosMoveCommand;
        public ICommand CY4PosMoveCommand
        {
            get
            {
                if (null == _CY4PosMoveCommand) _CY4PosMoveCommand = new AsyncCommand(CY4PosMoveFunc);
                return _CY4PosMoveCommand;
            }
        }
        private AsyncCommand _CY4NegMoveCommand;
        public ICommand CY4NegMoveCommand
        {
            get
            {
                if (null == _CY4NegMoveCommand) _CY4NegMoveCommand = new AsyncCommand(CY4NegMoveFunc);
                return _CY4NegMoveCommand;
            }
        }
        private AsyncCommand _CZ4PosMoveCommand;
        public ICommand CZ4PosMoveCommand
        {
            get
            {
                if (null == _CZ4PosMoveCommand) _CZ4PosMoveCommand = new AsyncCommand(CZ4PosMoveFunc);
                return _CZ4PosMoveCommand;
            }
        }
        private AsyncCommand _CZ4NegMoveCommand;
        public ICommand CZ4NegMoveCommand
        {
            get
            {
                if (null == _CZ4NegMoveCommand) _CZ4NegMoveCommand = new AsyncCommand(CZ4NegMoveFunc);
                return _CZ4NegMoveCommand;
            }
        }

        private AsyncCommand _CZ5PosMoveCommand;
        public ICommand CZ5PosMoveCommand
        {
            get
            {
                if (null == _CZ5PosMoveCommand) _CZ5PosMoveCommand = new AsyncCommand(CZ5PosMoveFunc);
                return _CZ5PosMoveCommand;
            }
        }
        private AsyncCommand _CZ5NegMoveCommand;
        public ICommand CZ5NegMoveCommand
        {
            get
            {
                if (null == _CZ5NegMoveCommand) _CZ5NegMoveCommand = new AsyncCommand(CZ5NegMoveFunc);
                return _CZ5NegMoveCommand;
            }
        }

        private AsyncCommand _Z1PosMoveCommand;
        public ICommand Z1PosMoveCommand
        {
            get
            {
                if (null == _Z1PosMoveCommand) _Z1PosMoveCommand = new AsyncCommand(Z1PosMoveFunc);
                return _Z1PosMoveCommand;
            }
        }
        private AsyncCommand _Z1NegMoveCommand;
        public ICommand Z1NegMoveCommand
        {
            get
            {
                if (null == _Z1NegMoveCommand) _Z1NegMoveCommand = new AsyncCommand(Z1NegMoveFunc);
                return _Z1NegMoveCommand;
            }
        }

        private AsyncCommand _Z2PosMoveCommand;
        public ICommand Z2PosMoveCommand
        {
            get
            {
                if (null == _Z2PosMoveCommand) _Z2PosMoveCommand = new AsyncCommand(Z2PosMoveFunc);
                return _Z2PosMoveCommand;
            }
        }
        private AsyncCommand _Z2NegMoveCommand;
        public ICommand Z2NegMoveCommand
        {
            get
            {
                if (null == _Z2NegMoveCommand) _Z2NegMoveCommand = new AsyncCommand(Z2NegMoveFunc);
                return _Z2NegMoveCommand;
            }
        }

        private AsyncCommand _Z0PosMoveCommand;
        public ICommand Z0PosMoveCommand
        {
            get
            {
                if (null == _Z0PosMoveCommand) _Z0PosMoveCommand = new AsyncCommand(Z0PosMoveFunc);
                return _Z0PosMoveCommand;
            }
        }
        private AsyncCommand _Z0NegMoveCommand;
        public ICommand Z0NegMoveCommand
        {
            get
            {
                if (null == _Z0NegMoveCommand) _Z0NegMoveCommand = new AsyncCommand(Z0NegMoveFunc);
                return _Z0NegMoveCommand;
            }
        }

        private AsyncCommand _FDT1PosMoveCommand;
        public ICommand FDT1PosMoveCommand
        {
            get
            {
                if (null == _FDT1PosMoveCommand) _FDT1PosMoveCommand = new AsyncCommand(FDT1PosMoveFunc);
                return _FDT1PosMoveCommand;
            }
        }
        private AsyncCommand _FDT1NegMoveCommand;
        public ICommand FDT1NegMoveCommand
        {
            get
            {
                if (null == _FDT1NegMoveCommand) _FDT1NegMoveCommand = new AsyncCommand(FDT1NegMoveFunc);
                return _FDT1NegMoveCommand;
            }
        }

        private AsyncCommand _FDZ1PosMoveCommand;
        public ICommand FDZ1PosMoveCommand
        {
            get
            {
                if (null == _FDZ1PosMoveCommand) _FDZ1PosMoveCommand = new AsyncCommand(FDZ1PosMoveFunc);
                return _FDZ1PosMoveCommand;
            }
        }
        private AsyncCommand _FDZ1NegMoveCommand;
        public ICommand FDZ1NegMoveCommand
        {
            get
            {
                if (null == _FDZ1NegMoveCommand) _FDZ1NegMoveCommand = new AsyncCommand(FDZ1NegMoveFunc);
                return _FDZ1NegMoveCommand;
            }
        }

        private AsyncCommand _EJX1PosMoveCommand;
        public ICommand EJX1PosMoveCommand
        {
            get
            {
                if (null == _EJX1PosMoveCommand) _EJX1PosMoveCommand = new AsyncCommand(EJX1PosMoveFunc);
                return _EJX1PosMoveCommand;
            }
        }
        private AsyncCommand _EJX1NegMoveCommand;
        public ICommand EJX1NegMoveCommand
        {
            get
            {
                if (null == _EJX1NegMoveCommand) _EJX1NegMoveCommand = new AsyncCommand(EJX1NegMoveFunc);
                return _EJX1NegMoveCommand;
            }
        }

        private AsyncCommand _EJY1PosMoveCommand;
        public ICommand EJY1PosMoveCommand
        {
            get
            {
                if (null == _EJY1PosMoveCommand) _EJY1PosMoveCommand = new AsyncCommand(EJY1PosMoveFunc);
                return _EJY1PosMoveCommand;
            }
        }
        private AsyncCommand _EJY1NegMoveCommand;
        public ICommand EJY1NegMoveCommand
        {
            get
            {
                if (null == _EJY1NegMoveCommand) _EJY1NegMoveCommand = new AsyncCommand(EJY1NegMoveFunc);
                return _EJY1NegMoveCommand;
            }
        }

        private AsyncCommand _EJZ1PosMoveCommand;
        public ICommand EJZ1PosMoveCommand
        {
            get
            {
                if (null == _EJZ1PosMoveCommand) _EJZ1PosMoveCommand = new AsyncCommand(EJZ1PosMoveFunc);
                return _EJZ1PosMoveCommand;
            }
        }
        private AsyncCommand _EJZ1NegMoveCommand;
        public ICommand EJZ1NegMoveCommand
        {
            get
            {
                if (null == _EJZ1NegMoveCommand) _EJZ1NegMoveCommand = new AsyncCommand(EJZ1NegMoveFunc);
                return _EJZ1NegMoveCommand;
            }
        }

        private AsyncCommand _EJPZ1PosMoveCommand;
        public ICommand EJPZ1PosMoveCommand
        {
            get
            {
                if (null == _EJPZ1PosMoveCommand) _EJPZ1PosMoveCommand = new AsyncCommand(EJPZ1PosMoveFunc);
                return _EJPZ1PosMoveCommand;
            }
        }
        private AsyncCommand _EJPZ1NegMoveCommand;
        public ICommand EJPZ1NegMoveCommand
        {
            get
            {
                if (null == _EJPZ1NegMoveCommand) _EJPZ1NegMoveCommand = new AsyncCommand(EJPZ1NegMoveFunc);
                return _EJPZ1NegMoveCommand;
            }
        }

        private AsyncCommand _NZD1PosMoveCommand;
        public ICommand NZD1PosMoveCommand
        {
            get
            {
                if (null == _NZD1PosMoveCommand) _NZD1PosMoveCommand = new AsyncCommand(NZD1PosMoveFunc);
                return _NZD1PosMoveCommand;
            }
        }
        private AsyncCommand _NZD1NegMoveCommand;
        public ICommand NZD1NegMoveCommand
        {
            get
            {
                if (null == _NZD1NegMoveCommand) _NZD1NegMoveCommand = new AsyncCommand(NZD1NegMoveFunc);
                return _NZD1NegMoveCommand;
            }
        }

        private AsyncCommand _NSZ1PosMoveCommand;
        public ICommand NSZ1PosMoveCommand
        {
            get
            {
                if (null == _NSZ1PosMoveCommand) _NSZ1PosMoveCommand = new AsyncCommand(NSZ1PosMoveFunc);
                return _NSZ1PosMoveCommand;
            }
        }
        private AsyncCommand _NSZ1NegMoveCommand;
        public ICommand NSZ1NegMoveCommand
        {
            get
            {
                if (null == _NSZ1NegMoveCommand) _NSZ1NegMoveCommand = new AsyncCommand(NSZ1NegMoveFunc);
                return _NSZ1NegMoveCommand;
            }
        }

        //20260224 remy add
        private AsyncCommand _NSZ2PosMoveCommand;
        public ICommand NSZ2PosMoveCommand
        {
            get
            {
                if (null == _NSZ2PosMoveCommand) _NSZ2PosMoveCommand = new AsyncCommand(NSZ2PosMoveFunc);
                return _NSZ2PosMoveCommand;
            }
        }
        private AsyncCommand _NSZ2NegMoveCommand;
        public ICommand NSZ2NegMoveCommand
        {
            get
            {
                if (null == _NSZ2NegMoveCommand) _NSZ2NegMoveCommand = new AsyncCommand(NSZ2NegMoveFunc);
                return _NSZ2NegMoveCommand;
            }
        }

        #endregion

        #region Loader Move
        private AsyncCommand _APosMoveCommand;
        public ICommand APosMoveCommand
        {
            get
            {
                if (null == _APosMoveCommand) _APosMoveCommand = new AsyncCommand(APosMoveFunc);
                return _APosMoveCommand;
            }
        }
        private AsyncCommand _ANegMoveCommand;
        public ICommand ANegMoveCommand
        {
            get
            {
                if (null == _ANegMoveCommand) _ANegMoveCommand = new AsyncCommand(ANegMoveFunc);
                return _ANegMoveCommand;
            }
        }
        private AsyncCommand _U1PosMoveCommand;
        public ICommand U1PosMoveCommand
        {
            get
            {
                if (null == _U1PosMoveCommand) _U1PosMoveCommand = new AsyncCommand(U1PosMoveFunc);
                return _U1PosMoveCommand;
            }
        }
        private AsyncCommand _U1NegMoveCommand;
        public ICommand U1NegMoveCommand
        {
            get
            {
                if (null == _U1NegMoveCommand) _U1NegMoveCommand = new AsyncCommand(U1NegMoveFunc);
                return _U1NegMoveCommand;
            }
        }
        private AsyncCommand _U2PosMoveCommand;
        public ICommand U2PosMoveCommand
        {
            get
            {
                if (null == _U2PosMoveCommand) _U2PosMoveCommand = new AsyncCommand(U2PosMoveFunc);
                return _U2PosMoveCommand;
            }
        }
        private AsyncCommand _U2NegMoveCommand;
        public ICommand U2NegMoveCommand
        {
            get
            {
                if (null == _U2NegMoveCommand) _U2NegMoveCommand = new AsyncCommand(U2NegMoveFunc);
                return _U2NegMoveCommand;
            }
        }
        private AsyncCommand _WPosMoveCommand;
        public ICommand WPosMoveCommand
        {
            get
            {
                if (null == _WPosMoveCommand) _WPosMoveCommand = new AsyncCommand(WPosMoveFunc);
                return _WPosMoveCommand;
            }
        }
        private AsyncCommand _WNegMoveCommand;
        public ICommand WNegMoveCommand
        {
            get
            {
                if (null == _WNegMoveCommand) _WNegMoveCommand = new AsyncCommand(WNegMoveFunc);
                return _WNegMoveCommand;
            }
        }
        private AsyncCommand _VPosMoveCommand;
        public ICommand VPosMoveCommand
        {
            get
            {
                if (null == _VPosMoveCommand) _VPosMoveCommand = new AsyncCommand(VPosMoveFunc);
                return _VPosMoveCommand;
            }
        }
        private AsyncCommand _VNegMoveCommand;
        public ICommand VNegMoveCommand
        {
            get
            {
                if (null == _VNegMoveCommand) _VNegMoveCommand = new AsyncCommand(VNegMoveFunc);
                return _VNegMoveCommand;
            }
        }
        private AsyncCommand _ScPosMoveCommand;
        public ICommand ScPosMoveCommand
        {
            get
            {
                if (null == _ScPosMoveCommand) _ScPosMoveCommand = new AsyncCommand(ScPosMoveFunc);
                return _ScPosMoveCommand;
            }
        }
        private AsyncCommand _ScNegMoveCommand;
        public ICommand ScNegMoveCommand
        {
            get
            {
                if (null == _ScNegMoveCommand) _ScNegMoveCommand = new AsyncCommand(ScNegMoveFunc);
                return _ScNegMoveCommand;
            }
        }
        //251208 ybpark add
        private AsyncCommand _EPosMoveCommand;
        public ICommand EPosMoveCommand
        {
            get
            {
                if (null == _EPosMoveCommand) _EPosMoveCommand = new AsyncCommand(EPosMoveFunc);
                return _EPosMoveCommand;
            }
        }
        private AsyncCommand _ENegMoveCommand;
        public ICommand ENegMoveCommand
        {
            get
            {
                if (null == _ENegMoveCommand) _ENegMoveCommand = new AsyncCommand(ENegMoveFunc);
                return _ENegMoveCommand;
            }
        }

        private AsyncCommand _FVPosMoveCommand;
        public ICommand FVPosMoveCommand
        {
            get
            {
                if (null == _FVPosMoveCommand) _FVPosMoveCommand = new AsyncCommand(FVPosMoveFunc);
                return _EPosMoveCommand;
            }
        }
        private AsyncCommand _FVNegMoveCommand;
        public ICommand FVNegMoveCommand
        {
            get
            {
                if (null == _FVNegMoveCommand) _FVNegMoveCommand = new AsyncCommand(FVNegMoveFunc);
                return _FVNegMoveCommand;
            }
        }
        #endregion
        #region TextVal
        private int _XTextVal = 0;

        public int XTextVal
        {
            get { return _XTextVal; }
            set
            {
                if (value != _XTextVal)
                {
                    _XTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> XTextBoxClickCommand
        private RelayCommand<Object> _XTextBoxClickCommand;
        public ICommand XTextBoxClickCommand
        {
            get
            {
                if (null == _XTextBoxClickCommand) _XTextBoxClickCommand = new RelayCommand<Object>(XTextBoxClickCommandFunc);
                return _XTextBoxClickCommand;
            }
        }

        private void XTextBoxClickCommandFunc(Object param)
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
        #endregion

        private int _YTextVal = 0;

        public int YTextVal
        {
            get { return _YTextVal; }
            set
            {
                if (value != _YTextVal)
                {
                    _YTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> YTextBoxClickCommand
        private RelayCommand<Object> _YTextBoxClickCommand;
        public ICommand YTextBoxClickCommand
        {
            get
            {
                if (null == _YTextBoxClickCommand) _YTextBoxClickCommand = new RelayCommand<Object>(YTextBoxClickCommandFunc);
                return _YTextBoxClickCommand;
            }
        }

        private void YTextBoxClickCommandFunc(Object param)
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
        #endregion


        private int _ZTextVal = 0;

        public int ZTextVal
        {
            get { return _ZTextVal; }
            set
            {
                if (value != _ZTextVal)
                {
                    _ZTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> ZTextBoxClickCommand
        private RelayCommand<Object> _ZTextBoxClickCommand;
        public ICommand ZTextBoxClickCommand
        {
            get
            {
                if (null == _ZTextBoxClickCommand) _ZTextBoxClickCommand = new RelayCommand<Object>(ZTextBoxClickCommandFunc);
                return _ZTextBoxClickCommand;
            }
        }

        private void ZTextBoxClickCommandFunc(Object param)
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
        #endregion

        private int _CTextVal = 0;

        public int CTextVal
        {
            get { return _CTextVal; }
            set
            {
                if (value != _CTextVal)
                {
                    _CTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CTextBoxClickCommand
        private RelayCommand<Object> _CTextBoxClickCommand;
        public ICommand CTextBoxClickCommand
        {
            get
            {
                if (null == _CTextBoxClickCommand) _CTextBoxClickCommand = new RelayCommand<Object>(CTextBoxClickCommandFunc);
                return _CTextBoxClickCommand;
            }
        }

        private void CTextBoxClickCommandFunc(Object param)
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
                throw;
            }
        }
        #endregion

        private int _TriTextVal = 0;

        public int TriTextVal
        {
            get { return _TriTextVal; }
            set
            {
                if (value != _TriTextVal)
                {
                    _TriTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> TextBoxClickCommand
        private RelayCommand<Object> _TriTextBoxClickCommand;
        public ICommand TriTextBoxClickCommand
        {
            get
            {
                if (null == _TriTextBoxClickCommand) _TriTextBoxClickCommand = new RelayCommand<Object>(TriTextBoxClickCommandFunc);
                return _TriTextBoxClickCommand;
            }
        }

        private void TriTextBoxClickCommandFunc(Object param)
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
        #endregion


        private int _PzTextVal = 0;

        public int PzTextVal
        {
            get { return _PzTextVal; }
            set
            {
                if (value != _PzTextVal)
                {
                    _PzTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> PZTextBoxClickCommand
        private RelayCommand<Object> _PZTextBoxClickCommand;
        public ICommand PZTextBoxClickCommand
        {
            get
            {
                if (null == _PZTextBoxClickCommand) _PZTextBoxClickCommand = new RelayCommand<Object>(PZTextBoxClickCommandFunc);
                return _PZTextBoxClickCommand;
            }
        }

        private void PZTextBoxClickCommandFunc(Object param)
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

        //251029 yb add
        private int _CX1TextVal = 0;

        public int CX1TextVal
        {
            get { return _CX1TextVal; }
            set
            {
                if (value != _CX1TextVal)
                {
                    _CX1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CX1TextBoxClickCommand
        private RelayCommand<Object> _CX1TextBoxClickCommand;
        public ICommand CX1TextBoxClickCommand
        {
            get
            {
                if (null == _CX1TextBoxClickCommand) _CX1TextBoxClickCommand = new RelayCommand<Object>(CX1TextBoxClickCommandFunc);
                return _CX1TextBoxClickCommand;
            }
        }

        private void CX1TextBoxClickCommandFunc(Object param)
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
        #endregion
        private int _CY1TextVal = 0;

        public int CY1TextVal
        {
            get { return _CY1TextVal; }
            set
            {
                if (value != _CY1TextVal)
                {
                    _CY1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CY1TextBoxClickCommand
        private RelayCommand<Object> _CY1TextBoxClickCommand;
        public ICommand CY1TextBoxClickCommand
        {
            get
            {
                if (null == _CY1TextBoxClickCommand) _CY1TextBoxClickCommand = new RelayCommand<Object>(CY1TextBoxClickCommandFunc);
                return _CY1TextBoxClickCommand;
            }
        }

        private void CY1TextBoxClickCommandFunc(Object param)
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
        #endregion
        private int _CZ1TextVal = 0;

        public int CZ1TextVal
        {
            get { return _CZ1TextVal; }
            set
            {
                if (value != _CZ1TextVal)
                {
                    _CZ1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CZ1TextBoxClickCommand
        private RelayCommand<Object> _CZ1TextBoxClickCommand;
        public ICommand CZ1TextBoxClickCommand
        {
            get
            {
                if (null == _CZ1TextBoxClickCommand) _CZ1TextBoxClickCommand = new RelayCommand<Object>(CZ1TextBoxClickCommandFunc);
                return _CZ1TextBoxClickCommand;
            }
        }

        private void CZ1TextBoxClickCommandFunc(Object param)
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
        #endregion

        private int _CX2TextVal = 0;

        public int CX2TextVal
        {
            get { return _CX2TextVal; }
            set
            {
                if (value != _CX2TextVal)
                {
                    _CX2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CX2TextBoxClickCommand
        private RelayCommand<Object> _CX2TextBoxClickCommand;
        public ICommand CX2TextBoxClickCommand
        {
            get
            {
                if (null == _CX2TextBoxClickCommand) _CX2TextBoxClickCommand = new RelayCommand<Object>(CX2TextBoxClickCommandFunc);
                return _CX2TextBoxClickCommand;
            }
        }

        private void CX2TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 200);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private int _CY2TextVal = 0;

        public int CY2TextVal
        {
            get { return _CY2TextVal; }
            set
            {
                if (value != _CY2TextVal)
                {
                    _CY2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CY2TextBoxClickCommand
        private RelayCommand<Object> _CY2TextBoxClickCommand;
        public ICommand CY2TextBoxClickCommand
        {
            get
            {
                if (null == _CY2TextBoxClickCommand) _CY2TextBoxClickCommand = new RelayCommand<Object>(CY2TextBoxClickCommandFunc);
                return _CY2TextBoxClickCommand;
            }
        }

        private void CY2TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 200);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private int _CZ2TextVal = 0;

        public int CZ2TextVal
        {
            get { return _CZ2TextVal; }
            set
            {
                if (value != _CZ2TextVal)
                {
                    _CZ2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CZ2TextBoxClickCommand
        private RelayCommand<Object> _CZ2TextBoxClickCommand;
        public ICommand CZ2TextBoxClickCommand
        {
            get
            {
                if (null == _CZ2TextBoxClickCommand) _CZ2TextBoxClickCommand = new RelayCommand<Object>(CZ2TextBoxClickCommandFunc);
                return _CZ2TextBoxClickCommand;
            }
        }

        private void CZ2TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 200);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _CX3TextVal = 0;

        public int CX3TextVal
        {
            get { return _CX3TextVal; }
            set
            {
                if (value != _CX3TextVal)
                {
                    _CX3TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CX3TextBoxClickCommand
        private RelayCommand<Object> _CX3TextBoxClickCommand;
        public ICommand CX3TextBoxClickCommand
        {
            get
            {
                if (null == _CX3TextBoxClickCommand) _CX3TextBoxClickCommand = new RelayCommand<Object>(CX3TextBoxClickCommandFunc);
                return _CX3TextBoxClickCommand;
            }
        }

        private void CX3TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 300);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private int _CY3TextVal = 0;

        public int CY3TextVal
        {
            get { return _CY3TextVal; }
            set
            {
                if (value != _CY3TextVal)
                {
                    _CY3TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CY3TextBoxClickCommand
        private RelayCommand<Object> _CY3TextBoxClickCommand;
        public ICommand CY3TextBoxClickCommand
        {
            get
            {
                if (null == _CY3TextBoxClickCommand) _CY3TextBoxClickCommand = new RelayCommand<Object>(CY3TextBoxClickCommandFunc);
                return _CY3TextBoxClickCommand;
            }
        }

        private void CY3TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 300);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private int _CZ3TextVal = 0;

        public int CZ3TextVal
        {
            get { return _CZ3TextVal; }
            set
            {
                if (value != _CZ3TextVal)
                {
                    _CZ3TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CZ3TextBoxClickCommand
        private RelayCommand<Object> _CZ3TextBoxClickCommand;
        public ICommand CZ3TextBoxClickCommand
        {
            get
            {
                if (null == _CZ3TextBoxClickCommand) _CZ3TextBoxClickCommand = new RelayCommand<Object>(CZ3TextBoxClickCommandFunc);
                return _CZ3TextBoxClickCommand;
            }
        }

        private void CZ3TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 300);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _CX4TextVal = 0;

        public int CX4TextVal
        {
            get { return _CX4TextVal; }
            set
            {
                if (value != _CX4TextVal)
                {
                    _CX4TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CX4TextBoxClickCommand
        private RelayCommand<Object> _CX4TextBoxClickCommand;
        public ICommand CX4TextBoxClickCommand
        {
            get
            {
                if (null == _CX4TextBoxClickCommand) _CX4TextBoxClickCommand = new RelayCommand<Object>(CX4TextBoxClickCommandFunc);
                return _CX4TextBoxClickCommand;
            }
        }

        private void CX4TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 400);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private int _CY4TextVal = 0;

        public int CY4TextVal
        {
            get { return _CY4TextVal; }
            set
            {
                if (value != _CY4TextVal)
                {
                    _CY4TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CY4TextBoxClickCommand
        private RelayCommand<Object> _CY4TextBoxClickCommand;
        public ICommand CY4TextBoxClickCommand
        {
            get
            {
                if (null == _CY4TextBoxClickCommand) _CY4TextBoxClickCommand = new RelayCommand<Object>(CY4TextBoxClickCommandFunc);
                return _CY4TextBoxClickCommand;
            }
        }

        private void CY4TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 400);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        private int _CZ4TextVal = 0;

        public int CZ4TextVal
        {
            get { return _CZ4TextVal; }
            set
            {
                if (value != _CZ4TextVal)
                {
                    _CZ4TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CZ4TextBoxClickCommand
        private RelayCommand<Object> _CZ4TextBoxClickCommand;
        public ICommand CZ4TextBoxClickCommand
        {
            get
            {
                if (null == _CZ4TextBoxClickCommand) _CZ4TextBoxClickCommand = new RelayCommand<Object>(CZ4TextBoxClickCommandFunc);
                return _CZ4TextBoxClickCommand;
            }
        }

        private void CZ4TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 400);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _CZ5TextVal = 0;

        public int CZ5TextVal
        {
            get { return _CZ5TextVal; }
            set
            {
                if (value != _CZ5TextVal)
                {
                    _CZ5TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CZ5TextBoxClickCommand
        private RelayCommand<Object> _CZ5TextBoxClickCommand;
        public ICommand CZ5TextBoxClickCommand
        {
            get
            {
                if (null == _CZ5TextBoxClickCommand) _CZ5TextBoxClickCommand = new RelayCommand<Object>(CZ5TextBoxClickCommandFunc);
                return _CZ5TextBoxClickCommand;
            }
        }

        private void CZ5TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _Z1TextVal = 0;

        public int Z1TextVal
        {
            get { return _Z1TextVal; }
            set
            {
                if (value != _Z1TextVal)
                {
                    _Z1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> Z1TextBoxClickCommand
        private RelayCommand<Object> _Z1TextBoxClickCommand;
        public ICommand Z1TextBoxClickCommand
        {
            get
            {
                if (null == _Z1TextBoxClickCommand) _Z1TextBoxClickCommand = new RelayCommand<Object>(Z1TextBoxClickCommandFunc);
                return _Z1TextBoxClickCommand;
            }
        }

        private void Z1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _Z2TextVal = 0;

        public int Z2TextVal
        {
            get { return _Z2TextVal; }
            set
            {
                if (value != _Z2TextVal)
                {
                    _Z2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> Z2TextBoxClickCommand
        private RelayCommand<Object> _Z2TextBoxClickCommand;
        public ICommand Z2TextBoxClickCommand
        {
            get
            {
                if (null == _Z2TextBoxClickCommand) _Z2TextBoxClickCommand = new RelayCommand<Object>(Z2TextBoxClickCommandFunc);
                return _Z2TextBoxClickCommand;
            }
        }

        private void Z2TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _Z0TextVal = 0;

        public int Z0TextVal
        {
            get { return _Z0TextVal; }
            set
            {
                if (value != _Z0TextVal)
                {
                    _Z0TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> Z0TextBoxClickCommand
        private RelayCommand<Object> _Z0TextBoxClickCommand;
        public ICommand Z0TextBoxClickCommand
        {
            get
            {
                if (null == _Z0TextBoxClickCommand) _Z0TextBoxClickCommand = new RelayCommand<Object>(Z0TextBoxClickCommandFunc);
                return _Z0TextBoxClickCommand;
            }
        }

        private void Z0TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _FDT1TextVal = 0;

        public int FDT1TextVal
        {
            get { return _FDT1TextVal; }
            set
            {
                if (value != _FDT1TextVal)
                {
                    _FDT1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> FDT1TextBoxClickCommand
        private RelayCommand<Object> _FDT1TextBoxClickCommand;
        public ICommand FDT1TextBoxClickCommand
        {
            get
            {
                if (null == _FDT1TextBoxClickCommand) _FDT1TextBoxClickCommand = new RelayCommand<Object>(FDT1TextBoxClickCommandFunc);
                return _FDT1TextBoxClickCommand;
            }
        }

        private void FDT1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _FDZ1TextVal = 0;

        public int FDZ1TextVal
        {
            get { return _FDZ1TextVal; }
            set
            {
                if (value != _FDZ1TextVal)
                {
                    _FDZ1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> FDZ1TextBoxClickCommand
        private RelayCommand<Object> _FDZ1TextBoxClickCommand;
        public ICommand FDZ1TextBoxClickCommand
        {
            get
            {
                if (null == _FDZ1TextBoxClickCommand) _FDZ1TextBoxClickCommand = new RelayCommand<Object>(FDZ1TextBoxClickCommandFunc);
                return _FDZ1TextBoxClickCommand;
            }
        }

        private void FDZ1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _EJX1TextVal = 0;

        public int EJX1TextVal
        {
            get { return _EJX1TextVal; }
            set
            {
                if (value != _EJX1TextVal)
                {
                    _EJX1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> EJX1TextBoxClickCommand
        private RelayCommand<Object> _EJX1TextBoxClickCommand;
        public ICommand EJX1TextBoxClickCommand
        {
            get
            {
                if (null == _EJX1TextBoxClickCommand) _EJX1TextBoxClickCommand = new RelayCommand<Object>(EJX1TextBoxClickCommandFunc);
                return _EJX1TextBoxClickCommand;
            }
        }

        private void EJX1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _EJY1TextVal = 0;

        public int EJY1TextVal
        {
            get { return _EJY1TextVal; }
            set
            {
                if (value != _EJY1TextVal)
                {
                    _EJY1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> EJY1TextBoxClickCommand
        private RelayCommand<Object> _EJY1TextBoxClickCommand;
        public ICommand EJY1TextBoxClickCommand
        {
            get
            {
                if (null == _EJY1TextBoxClickCommand) _EJY1TextBoxClickCommand = new RelayCommand<Object>(EJY1TextBoxClickCommandFunc);
                return _EJY1TextBoxClickCommand;
            }
        }

        private void EJY1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _EJZ1TextVal = 0;

        public int EJZ1TextVal
        {
            get { return _EJZ1TextVal; }
            set
            {
                if (value != _EJZ1TextVal)
                {
                    _EJZ1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> EJZ1TextBoxClickCommand
        private RelayCommand<Object> _EJZ1TextBoxClickCommand;
        public ICommand EJZ1TextBoxClickCommand
        {
            get
            {
                if (null == _EJZ1TextBoxClickCommand) _EJZ1TextBoxClickCommand = new RelayCommand<Object>(EJZ1TextBoxClickCommandFunc);
                return _EJZ1TextBoxClickCommand;
            }
        }

        private void EJZ1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _EJPZ1TextVal = 0;

        public int EJPZ1TextVal
        {
            get { return _EJPZ1TextVal; }
            set
            {
                if (value != _EJPZ1TextVal)
                {
                    _EJPZ1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> EJPZ1TextBoxClickCommand
        private RelayCommand<Object> _EJPZ1TextBoxClickCommand;
        public ICommand EJPZ1TextBoxClickCommand
        {
            get
            {
                if (null == _EJPZ1TextBoxClickCommand) _EJPZ1TextBoxClickCommand = new RelayCommand<Object>(EJPZ1TextBoxClickCommandFunc);
                return _EJPZ1TextBoxClickCommand;
            }
        }

        private void EJPZ1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _NZD1TextVal = 0;

        public int NZD1TextVal
        {
            get { return _NZD1TextVal; }
            set
            {
                if (value != _NZD1TextVal)
                {
                    _NZD1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> NZD1TextBoxClickCommand
        private RelayCommand<Object> _NZD1TextBoxClickCommand;
        public ICommand NZD1TextBoxClickCommand
        {
            get
            {
                if (null == _NZD1TextBoxClickCommand) _NZD1TextBoxClickCommand = new RelayCommand<Object>(NZD1TextBoxClickCommandFunc);
                return _NZD1TextBoxClickCommand;
            }
        }

        private void NZD1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //251124 ybpark Test Count Add
        #region ==> TestCountTextBoxClickCommand
        private RelayCommand<Object> _TestCountTextBoxClickCommand;
        public ICommand TestCountTextBoxClickCommand
        {
            get
            {
                if (null == _TestCountTextBoxClickCommand) _TestCountTextBoxClickCommand = new RelayCommand<Object>(TestCountTextBoxClickCommandFunc);
                return _TestCountTextBoxClickCommand;
            }
        }

        private void TestCountTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _NSZ1TextVal = 0;

        public int NSZ1TextVal
        {
            get { return _NSZ1TextVal; }
            set
            {
                if (value != _NSZ1TextVal)
                {
                    _NSZ1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        //20260224 remy add
        private int _NSZ2TextVal = 0;

        public int NSZ2TextVal
        {
            get { return _NSZ2TextVal; }
            set
            {
                if (value != _NSZ2TextVal)
                {
                    _NSZ2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> NSZ1TextBoxClickCommand
        private RelayCommand<Object> _NSZ1TextBoxClickCommand;
        public ICommand NSZ1TextBoxClickCommand
        {
            get
            {
                if (null == _NSZ1TextBoxClickCommand) _NSZ1TextBoxClickCommand = new RelayCommand<Object>(NSZ1TextBoxClickCommandFunc);
                return _NSZ1TextBoxClickCommand;
            }
        }

        private void NSZ1TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        //260706 Remy
        #region ==> NSZ2TextBoxClickCommand
        private RelayCommand<Object> _NSZ2TextBoxClickCommand;
        public ICommand NSZ2TextBoxClickCommand
        {
            get
            {
                if (null == _NSZ2TextBoxClickCommand) _NSZ2TextBoxClickCommand = new RelayCommand<Object>(NSZ2TextBoxClickCommandFunc);
                return _NSZ2TextBoxClickCommand;
            }
        }

        private void NSZ2TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 500);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #endregion


        private int _ATextVal = 0;

        public int ATextVal
        {
            get { return _ATextVal; }
            set
            {
                if (value != _ATextVal)
                {
                    _ATextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _U1TextVal = 0;

        public int U1TextVal
        {
            get { return _U1TextVal; }
            set
            {
                if (value != _U1TextVal)
                {
                    _U1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _U2TextVal = 0;

        public int U2TextVal
        {
            get { return _U2TextVal; }
            set
            {
                if (value != _U2TextVal)
                {
                    _U2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _WTextVal = 0;

        public int WTextVal
        {
            get { return _WTextVal; }
            set
            {
                if (value != _WTextVal)
                {
                    _WTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _VTextVal = 0;

        public int VTextVal
        {
            get { return _VTextVal; }
            set
            {
                if (value != _VTextVal)
                {
                    _VTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SCTextVal = 0;

        public int ScTextVal
        {
            get { return _SCTextVal; }
            set
            {
                if (value != _SCTextVal)
                {
                    _SCTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        //251208 ybpark add
        private int _ETextVal = 0;

        public int ETextVal
        {
            get { return _ETextVal; }
            set
            {
                if (value != _ETextVal)
                {
                    _ETextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        //251208 ybpark add
        private int _FVTextVal = 0;

        public int FVTextVal
        {
            get { return _FVTextVal; }
            set
            {
                if (value != _FVTextVal)
                {
                    _FVTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region ActualVal
        private AxisObject _XAxis;

        public AxisObject XAxis
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
        private AxisObject _YAxis;

        public AxisObject YAxis
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
        private AxisObject _ZAxis;

        public AxisObject ZAxis
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
        private AxisObject _CAxis;

        public AxisObject CAxis
        {
            get { return _CAxis; }
            set
            {
                if (value != _CAxis)
                {
                    _CAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _TriAxis;

        public AxisObject TriAxis
        {
            get { return _TriAxis; }
            set
            {
                if (value != _TriAxis)
                {
                    _TriAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _PzAxis;

        public AxisObject PzAxis
        {
            get { return _PzAxis; }
            set
            {
                if (value != _PzAxis)
                {
                    _PzAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _AAxis;

        public AxisObject AAxis
        {
            get { return _AAxis; }
            set
            {
                if (value != _AAxis)
                {
                    _AAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _U1Axis;

        public AxisObject U1Axis
        {
            get { return _U1Axis; }
            set
            {
                if (value != _U1Axis)
                {
                    _U1Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _U2Axis;

        public AxisObject U2Axis
        {
            get { return _U2Axis; }
            set
            {
                if (value != _U2Axis)
                {
                    _U2Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _WAxis;

        public AxisObject WAxis
        {
            get { return _WAxis; }
            set
            {
                if (value != _WAxis)
                {
                    _WAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _VAxis;

        public AxisObject VAxis
        {
            get { return _VAxis; }
            set
            {
                if (value != _VAxis)
                {
                    _VAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _SCAxis;

        public AxisObject SCAxis
        {
            get { return _SCAxis; }
            set
            {
                if (value != _SCAxis)
                {
                    _SCAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        //251208 ybpark add
        private AxisObject _EAxis;

        public AxisObject EAxis
        {
            get { return _EAxis; }
            set
            {
                if (value != _EAxis)
                {
                    _EAxis = value;
                    RaisePropertyChanged();
                }
            }
        }

        //251208 ybpark add
        private AxisObject _FVAxis;

        public AxisObject FVAxis
        {
            get { return _FVAxis; }
            set
            {
                if (value != _FVAxis)
                {
                    _FVAxis = value;
                    RaisePropertyChanged();
                }
            }
        }

        //251029 yb add
        private AxisObject _CX1;

        public AxisObject CX1
        {
            get { return _CX1; }
            set
            {
                if (value != _CX1)
                {
                    _CX1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CY1;

        public AxisObject CY1
        {
            get { return _CY1; }
            set
            {
                if (value != _CY1)
                {
                    _CY1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CZ1;

        public AxisObject CZ1
        {
            get { return _CZ1; }
            set
            {
                if (value != _CZ1)
                {
                    _CZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _CX2;

        public AxisObject CX2
        {
            get { return _CX2; }
            set
            {
                if (value != _CX2)
                {
                    _CX2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CY2;

        public AxisObject CY2
        {
            get { return _CY2; }
            set
            {
                if (value != _CY2)
                {
                    _CY2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CZ2;

        public AxisObject CZ2
        {
            get { return _CZ2; }
            set
            {
                if (value != _CZ2)
                {
                    _CZ2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _CX3;

        public AxisObject CX3
        {
            get { return _CX3; }
            set
            {
                if (value != _CX3)
                {
                    _CX3 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CY3;

        public AxisObject CY3
        {
            get { return _CY3; }
            set
            {
                if (value != _CY3)
                {
                    _CY3 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CZ3;

        public AxisObject CZ3
        {
            get { return _CZ3; }
            set
            {
                if (value != _CZ3)
                {
                    _CZ3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _CX4;

        public AxisObject CX4
        {
            get { return _CX4; }
            set
            {
                if (value != _CX4)
                {
                    _CX4 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CY4;

        public AxisObject CY4
        {
            get { return _CY4; }
            set
            {
                if (value != _CY4)
                {
                    _CY4 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CZ4;

        public AxisObject CZ4
        {
            get { return _CZ4; }
            set
            {
                if (value != _CZ4)
                {
                    _CZ4 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _CZ5;

        public AxisObject CZ5
        {
            get { return _CZ5; }
            set
            {
                if (value != _CZ5)
                {
                    _CZ5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _Z1;

        public AxisObject Z1
        {
            get { return _Z1; }
            set
            {
                if (value != _Z1)
                {
                    _Z1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _Z2;

        public AxisObject Z2
        {
            get { return _Z2; }
            set
            {
                if (value != _Z2)
                {
                    _Z2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _Z0;

        public AxisObject Z0
        {
            get { return _Z0; }
            set
            {
                if (value != _Z0)
                {
                    _Z0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _FDT1;

        public AxisObject FDT1
        {
            get { return _FDT1; }
            set
            {
                if (value != _FDT1)
                {
                    _FDT1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _FDZ1;

        public AxisObject FDZ1
        {
            get { return _FDZ1; }
            set
            {
                if (value != _FDZ1)
                {
                    _FDZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _EJX1;

        public AxisObject EJX1
        {
            get { return _EJX1; }
            set
            {
                if (value != _EJX1)
                {
                    _EJX1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _EJY1;

        public AxisObject EJY1
        {
            get { return _EJY1; }
            set
            {
                if (value != _EJY1)
                {
                    _EJY1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _EJZ1;

        public AxisObject EJZ1
        {
            get { return _EJZ1; }
            set
            {
                if (value != _EJZ1)
                {
                    _EJZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _EJPZ1;

        public AxisObject EJPZ1
        {
            get { return _EJPZ1; }
            set
            {
                if (value != _EJPZ1)
                {
                    _EJPZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _NZD1;

        public AxisObject NZD1
        {
            get { return _NZD1; }
            set
            {
                if (value != _NZD1)
                {
                    _NZD1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _NSZ1;

        public AxisObject NSZ1
        {
            get { return _NSZ1; }
            set
            {
                if (value != _NSZ1)
                {
                    _NSZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Command 
        private async Task XPosMoveFunc()
        {
            try
            {

                double apos = 0;
                axis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                //var axisCCS = this.MotionManager().GetActualPos(EnumAxisConstants.X, ref apos);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = XTextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task XNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = XTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task YPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = YTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task YNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = YTextVal;
                    Negmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task ZPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ZTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task ZNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ZTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task CPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task CNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task TriPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.TRI);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = TriTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task TriNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.TRI);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = TriTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task PzPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = PzTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task PzNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = PzTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task APosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.A);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ATextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task ANegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.A);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ATextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task U1PosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.U1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U1TextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task U1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.U1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task U2PosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.U2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U2TextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task U2NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.U2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task WPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.W);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = WTextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task WNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.W);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = WTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task VPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.V);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = VTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task VNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.V);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = VTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task ScPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.SC);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ScTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task ScNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.SC);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ScTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }

        //251205 ybpark add
        private async Task EPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.E);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ETextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task ENegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.E);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ETextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task FVPosMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.FV);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = FVTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        private async Task FVNegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.FV);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = FVTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }
        //251029 yb add
        private async Task CX1PosMoveFunc()
        {
            EventCodeEnum ret = EventCodeEnum.NODATA;
            try
            {
                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        // <-- 251105 sebas homing test
        protected EventCodeEnum ResultValidate(object funcname, EventCodeEnum retcode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            ret = retcode;

            if (retcode != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"ResultValidate Fail :  Error code = {retcode.ToString()}, fucntion name = {funcname.ToString()}");

                throw new Exception($"FunctionName: {funcname.ToString()} Returncode: {retcode.ToString()} Error occurred");
            }

            return ret;
        }
        // -->
        private async Task CX1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task CX2PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX2TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CX2NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY2PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY2TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY2NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ2PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ2TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ2NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task CX3PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX3);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX3TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CX3NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX3);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX3TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY3PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY3);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY3TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY3NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY3);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY3TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ3PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ3);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ3TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ3NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ3);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ3TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task CX4PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX4);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX4TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CX4NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CX4);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CX4TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY4PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY4);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY4TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CY4NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CY4);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CY4TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ4PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ4);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ4TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ4NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ4);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ4TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task CZ5PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ5);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ5TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task CZ5NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.CZ5);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CZ5TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task Z1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = Z1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task Z1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = Z1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task Z2PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = Z2TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task Z2NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = Z2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task Z0PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = Z0TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task Z0NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = Z0TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task FDT1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = FDT1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task FDT1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = FDT1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task FDZ1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = FDZ1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task FDZ1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = FDZ1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task EJX1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJX1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJX1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task EJX1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJX1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJX1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task EJY1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJY1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJY1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task EJY1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJY1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJY1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task EJZ1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJZ1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task EJZ1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJZ1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task EJPZ1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJPZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJPZ1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task EJPZ1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.EJPZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = EJPZ1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private async Task NZD1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = NZD1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task NZD1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = NZD1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        //20260224 remy add
        private async Task NSZ2PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.NSZ2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = NSZ2TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task NSZ2NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.NSZ2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = NSZ2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }



        private async Task NSZ1PosMoveFunc()
        {
            try
            {


                axis = this.MotionManager().GetAxis(EnumAxisConstants.NSZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = NSZ1TextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }
        private async Task NSZ1NegMoveFunc()
        {
            try
            {

                axis = this.MotionManager().GetAxis(EnumAxisConstants.NSZ1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = NSZ1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        #endregion
        #region Move Code
        private async Task Posmove()
        {

            try
            {
                double apos = 0;

                AxisObject = axis;
                apos = axis.Status.RawPosition.Ref;
                //this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos); //AxisObject.AxisType.Value 축 Enum //  apos : 축위치
                double pos = Math.Abs(RelMoveStepDist); // 움직일 거리의 절대값

                // 20260115 Nick Limit 임시 제거
                //if (pos + apos < AxisObject.Param.PosSWLimit.Value) // 리밋체크 pos(움직일 거리)와 apos(기존 나의 위치)의 합이 리밋보다 작으면 동작
                if (true)
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    NegButtonVisibility = false;
                    //20251030 yb 주석처리
                    //retVal = this.StageSupervisor().StageModuleState.ManualRelMove(AxisObject, pos);

                    retVal = this.MotionManager().RelMove_Wating(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                    Thread.Sleep(250);
                }
                else
                {
                    //Sw limit
                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "SW Limit", EnumMessageStyle.Affirmative);

                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                PosRefresh();
            }

        }
        private async Task Negmove()
        {

            try
            {
                double apos = 0;
                AxisObject = axis;
                apos = axis.Status.RawPosition.Ref;
                //this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos);
                double pos = Math.Abs(RelMoveStepDist) * -1;

                // 260115 Nick limit 임시 제거
                //if (pos + apos > AxisObject.Param.NegSWLimit.Value)
                if (true)
                {
                    PosButtonVisibility = false;
                    //20251030 yb
                    //this.StageSupervisor().StageModuleState.ManualRelMove(AxisObject, pos);
                    this.MotionManager().RelMove_Wating(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                }
                else
                {
                    //Sw Limit
                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "SW Limit", EnumMessageStyle.Affirmative);

                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                PosRefresh();
            }

        }
        #endregion
        #region Position PosRefresh
        public double _XActualVal = 0.0;
        public double XActualVal
        {
            get
            {
                return _XActualVal;
            }
            set
            {
                if (_XActualVal != value)
                {
                    _XActualVal = value;
                    RaisePropertyChanged("XActualVal");
                }
            }
        }
        public double _YActualVal = 0.0;
        public double YActualVal
        {
            get
            {
                return _YActualVal;
            }
            set
            {
                if (_YActualVal != value)
                {
                    _YActualVal = value;
                    RaisePropertyChanged("YActualVal");
                }
            }
        }

        public double _CActualVal = 0.0;
        public double CActualVal
        {
            get
            {
                return _CActualVal;
            }
            set
            {
                if (_CActualVal != value)
                {
                    _CActualVal = value;
                    RaisePropertyChanged("CActualVal");
                }
            }
        }

        public double _TRIActualVal = 0.0;
        public double TRIActualVal
        {
            get
            {
                return _TRIActualVal;
            }
            set
            {
                if (_TRIActualVal != value)
                {
                    _TRIActualVal = value;
                    RaisePropertyChanged("TRIActualVal");
                }
            }
        }

        public double _Z0ActualVal = 0.0;
        public double Z0ActualVal
        {
            get
            {
                return _Z0ActualVal;
            }
            set
            {
                if (_Z0ActualVal != value)
                {
                    _Z0ActualVal = value;
                    RaisePropertyChanged("Z0ActualVal");
                }
            }
        }

        public double _Z1ActualVal = 0.0;
        public double Z1ActualVal
        {
            get
            {
                return _Z1ActualVal;
            }
            set
            {
                if (_Z1ActualVal != value)
                {
                    _Z1ActualVal = value;
                    RaisePropertyChanged("Z1ActualVal");
                }
            }
        }

        public double _Z2ActualVal = 0.0;
        public double Z2ActualVal
        {
            get
            {
                return _Z2ActualVal;
            }
            set
            {
                if (_Z2ActualVal != value)
                {
                    _Z2ActualVal = value;
                    RaisePropertyChanged("Z2ActualVal");
                }
            }
        }

        public double _NSZ1ActualVal = 0.0;
        public double NSZ1ActualVal
        {
            get
            {
                return _NSZ1ActualVal;
            }
            set
            {
                if (_NSZ1ActualVal != value)
                {
                    _NSZ1ActualVal = value;
                    RaisePropertyChanged("NSZ1ActualVal");
                }
            }
        }
        //20260224 remy 5cam add
        public double _CX1ActualVal = 0.0;
        public double CX1ActualVal
        {
            get
            {
                return _CX1ActualVal;
            }
            set
            {
                if (_CX1ActualVal != value)
                { 
                    _CX1ActualVal = value;
                    RaisePropertyChanged("CX1ActualVal");
                }
            }
        }

        public double _CY1ActualVal = 0.0;
        public double CY1ActualVal
        {
            get
            {
                return _CY1ActualVal;
            }
            set
            {
                if (_CY1ActualVal != value)
                {
                    _CY1ActualVal = value;
                    RaisePropertyChanged("CY1ActualVal");
                }
            }
        }

        public double _CZ1ActualVal = 0.0;
        public double CZ1ActualVal
        {
            get
            {
                return _CZ1ActualVal;
            }
            set
            {
                if (_CZ1ActualVal != value)
                {
                    _CZ1ActualVal = value;
                    RaisePropertyChanged("CZ1ActualVal");
                }
            }
        }

        public double _CX2ActualVal = 0.0;
        public double CX2ActualVal
        {
            get
            {
                return _CX2ActualVal;
            }
            set
            {
                if (_CX2ActualVal != value)
                {
                    _CX2ActualVal = value;
                    RaisePropertyChanged("CX2ActualVal");
                }
            }
        }

        public double _CY2ActualVal = 0.0;
        public double CY2ActualVal
        {
            get
            {
                return _CY2ActualVal;
            }
            set
            {
                if (_CY2ActualVal != value)
                {
                    _CY2ActualVal = value;
                    RaisePropertyChanged("CY2ActualVal");
                }
            }
        }

        public double _CZ2ActualVal = 0.0;
        public double CZ2ActualVal
        {
            get
            {
                return _CZ2ActualVal;
            }
            set
            {
                if (_CZ2ActualVal != value)
                {
                    _CZ2ActualVal = value;
                    RaisePropertyChanged("CZ2ActualVal");
                }
            }
        }

        public double _CX3ActualVal = 0.0;
        public double CX3ActualVal
        {
            get
            {
                return _CX3ActualVal;
            }
            set
            {
                if (_CX3ActualVal != value)
                {
                    _CX3ActualVal = value;
                    RaisePropertyChanged("CX3ActualVal");
                }
            }
        }

        public double _CY3ActualVal = 0.0;
        public double CY3ActualVal
        {
            get
            {
                return _CY3ActualVal;
            }
            set
            {
                if (_CY3ActualVal != value)
                {
                    _CY3ActualVal = value;
                    RaisePropertyChanged("CY3ActualVal");
                }
            }
        }

        public double _CZ3ActualVal = 0.0;
        public double CZ3ActualVal
        {
            get
            {
                return _CZ3ActualVal;
            }
            set
            {
                if (_CZ3ActualVal != value)
                {
                    _CZ3ActualVal = value;
                    RaisePropertyChanged("CZ3ActualVal");
                }
            }
        }

        public double _CX4ActualVal = 0.0;
        public double CX4ActualVal
        {
            get
            {
                return _CX4ActualVal;
            }
            set
            {
                if (_CX4ActualVal != value)
                {
                    _CX4ActualVal = value;
                    RaisePropertyChanged("CX4ActualVal");
                }
            }
        }

        public double _CY4ActualVal = 0.0;
        public double CY4ActualVal
        {
            get
            {
                return _CY4ActualVal;
            }
            set
            {
                if (_CY4ActualVal != value)
                {
                    _CY4ActualVal = value;
                    RaisePropertyChanged("CY4ActualVal");
                }
            }
        }

        public double _CZ4ActualVal = 0.0;
        public double CZ4ActualVal
        {
            get
            {
                return _CZ4ActualVal;
            }
            set
            {
                if (_CZ4ActualVal != value)
                {
                    _CZ4ActualVal = value;
                    RaisePropertyChanged("CZ4ActualVal");
                }
            }
        }

        public double _CZ5ActualVal = 0.0;
        public double CZ5ActualVal
        {
            get
            {
                return _CZ5ActualVal;
            }
            set
            {
                if (_CZ5ActualVal != value)
                {
                    _CZ5ActualVal = value;
                    RaisePropertyChanged("CZ5ActualVal");
                }
            }
        }

        public double _ZActualVal = 0.0;
        public double ZActualVal
        {
            get
            {
                return _ZActualVal;
            }
            set
            {
                if (_ZActualVal != value)
                {
                    _ZActualVal = value;
                    RaisePropertyChanged("ZActualVal");
                }
            }
        }





        //20260224 remy FD 카메라 추가
        public double _NSZ2ActualVal = 0.0;
        public double NSZ2ActualVal
        {
            get
            {
                return _NSZ2ActualVal;
            }
            set
            {
                if (_NSZ2ActualVal != value)
                {
                    _NSZ2ActualVal = value;
                    RaisePropertyChanged("NSZ2ActualVal");
                }
            }
        }

        public double _FDZ1ActualVal = 0.0;
        public double FDZ1ActualVal
        {
            get
            {
                return _FDZ1ActualVal;
            }
            set
            {
                if (_FDZ1ActualVal != value)
                {
                    _FDZ1ActualVal = value;
                    RaisePropertyChanged("FDZ1ActualVal");
                }
            }
        }

        public double _FDT1ActualVal = 0.0;
        public double FDT1ActualVal
        {
            get
            {
                return _FDT1ActualVal;
            }
            set
            {
                if (_FDT1ActualVal != value)
                {
                    _FDT1ActualVal = value;
                    RaisePropertyChanged("FDT1ActualVal");
                }
            }
        }

        public double _NZD1ActualVal = 0.0;
        public double NZD1ActualVal
        {
            get
            {
                return _NZD1ActualVal;
            }
            set
            {
                if (_NZD1ActualVal != value)
                {
                    _NZD1ActualVal = value;
                    RaisePropertyChanged("NZD1ActualVal");
                }
            }
        }
        private async Task PosRefresh() //실시간 엔코더 값 읽어옴 
        {
            try
            {
                await Task.Run(() =>
                {
                    //251111 ybpark GetActualPos은 실시간 엔코더 값을 읽어서 pulse 값으로 나타냄. pluse -> mm 단위로 표시하기위해 각 축별 모터 정보를 받아 mm 로 환산 추가
                    IMotionManager Motionmanager = this.MotionManager();

                    double currentPulseValue = 0.0;

                    double currentPulseValue1 = 0.0;
                    double currentPulseValue2 = 0.0;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.X).AxisType.Value, ref currentPulseValue1);
                    XActualVal = currentPulseValue1;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Y).AxisType.Value, ref currentPulseValue2);
                    YActualVal = currentPulseValue2;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.C).AxisType.Value, ref currentPulseValue);
                    CActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.TRI).AxisType.Value, ref currentPulseValue);
                    TRIActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z0).AxisType.Value, ref currentPulseValue);
                    Z0ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z1).AxisType.Value, ref currentPulseValue);
                    Z1ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z2).AxisType.Value, ref currentPulseValue);
                    Z2ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.NSZ1).AxisType.Value, ref currentPulseValue);
                    NSZ1ActualVal = currentPulseValue;

                    //20260224  remy 
                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.NSZ2).AxisType.Value, ref currentPulseValue);
                    NSZ2ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.FDZ1).AxisType.Value, ref currentPulseValue);
                    FDZ1ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.FDT1).AxisType.Value, ref currentPulseValue);
                    FDT1ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CX1).AxisType.Value, ref currentPulseValue);
                    CX1ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CY1).AxisType.Value, ref currentPulseValue);
                    CY1ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CZ1).AxisType.Value, ref currentPulseValue);
                    CZ1ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CX2).AxisType.Value, ref currentPulseValue);
                    CX2ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CY2).AxisType.Value, ref currentPulseValue);
                    CY2ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CZ2).AxisType.Value, ref currentPulseValue);
                    CZ2ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CX3).AxisType.Value, ref currentPulseValue);
                    CX3ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CY3).AxisType.Value, ref currentPulseValue);
                    CY3ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CZ3).AxisType.Value, ref currentPulseValue);
                    CZ3ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CX4).AxisType.Value, ref currentPulseValue);
                    CX4ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CY4).AxisType.Value, ref currentPulseValue);
                    CY4ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CZ4).AxisType.Value, ref currentPulseValue);
                    CZ4ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.CZ5).AxisType.Value, ref currentPulseValue);
                    CZ5ActualVal = currentPulseValue;

                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z).AxisType.Value, ref currentPulseValue);
                    ZActualVal = currentPulseValue;



                    // 260206 sebas : Ejection 축 삭제로 주석처리
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJX1).AxisType.Value, ref currentPulseValue);
                    //EJX1ActualVal = currentPulseValue;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJY1).AxisType.Value, ref currentPulseValue);
                    //EJY1ActualVal = currentPulseValue;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJZ1).AxisType.Value, ref currentPulseValue);
                    //EJZ1ActualVal = currentPulseValue;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJPZ1).AxisType.Value, ref currentPulseValue);
                    //EJPZ1ActualVal = currentPulseValue;

                    // 260226 sebas : 아직 DD모터 연결이 안되었으므로 일단 주석처리
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.NZD1).AxisType.Value, ref currentPulseValue);
                    //NZD1ActualVal = currentPulseValue;

                    //기존 주석 처리 되어있었음.
                    //XActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.X).Status.Position.Actual,2);
                    //YActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.Y).Status.Position.Actual, 2);
                    //ZActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.Z).Status.Position.Actual, 2);
                    //CActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.C).Status.Position.Actual, 2);
                    //TriActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.TRI).Status.Position.Actual, 2);
                    //PzActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.PZ).Status.Position.Actual, 2);
                    //AActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.A).Status.Position.Actual, 2);
                    //U1ActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.U2).Status.Position.Actual, 2);
                    //U2ActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.U1).Status.Position.Actual, 2);
                    //WActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.W).Status.Position.Actual, 2);
                    //VActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.V).Status.Position.Actual, 2);
                    //ScActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.SC).Status.Position.Actual, 2);
                    //double Pos = 0;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.X).AxisType.Value, ref Pos);
                    //XActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Y).AxisType.Value, ref Pos);
                    //YActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z).AxisType.Value, ref Pos);
                    //ZActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.C).AxisType.Value, ref Pos);
                    //CActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.TRI).AxisType.Value, ref Pos);
                    //TriActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.PZ).AxisType.Value, ref Pos);
                    //PzActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.A).AxisType.Value, ref Pos);
                    //AActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.U1).AxisType.Value, ref Pos);
                    //U1ActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.U2).AxisType.Value, ref Pos);
                    //U2ActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.W).AxisType.Value, ref Pos);
                    //WActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.V).AxisType.Value, ref Pos);
                    //VActualVal = Pos;
                    //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.SC).AxisType.Value, ref Pos);
                    //ScActualVal = Pos;
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }

        }
        #endregion
        #endregion

        #region TextBox.Value
        private List<MotionText> _MotionTextVal;
        public List<MotionText> MotionTextVal
        {
            get { return _MotionTextVal; }
            set
            {
                if (value != _MotionTextVal)
                {
                    _MotionTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


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
                    this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos); //AxisObject.AxisType.Value 축 Enum //  apos : 축위치
                    double pos = Math.Abs(RelMoveStepDist); // 움직일 거리의 절대값
                    if (pos + apos < AxisObject.Param.PosSWLimit.Value) // 리밋체크 pos(움직일 거리)와 apos(기존 나의 위치)의 합이 리밋보다 작으면 동작
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
            catch (Exception err)
            {
                NegButtonVisibility = true;
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
            EnumMessageDialogResult ret;
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #region ==> TextBoxClickCommand
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
        #endregion

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    PropertyInfo[] propertyInfos;
                    IOPortDescripter<bool> port;
                    object propObject;
                    double axisX, axisY, axisZ, axisC, axisTRI, axisR, axisTT, axisPZ, axisCT, axisCCM, axisCCS, axisCCG, axisA, axisU1, axisU2, axisW, axisV, axisSC;

                    //StageAxes aes = this.MotionManager().StageAxes;
                    //LoaderAxes les = this.MotionManager().LoaderAxes;

                    StageAxisObjectVmList = new ObservableCollection<AxisObjectVM>();

                    LoaderController = this.LoaderController() as ILoaderControllerExtension;

                    ViewModelManager = this.ViewModelManager();

                    Stage3DModel = this.ViewModelManager().Stage3DModel;

                    ViewNUM = 0;

                    CenterView();

                    IsItDisplayed2RateMagnification = false;
                    //{
                    //    if (item.AxisType.Value == EnumAxisConstants.R || item.AxisType.Value == EnumAxisConstants.TT)
                    //    {

                    //        var axisObjVM = new AxisObjectVM();
                    //        axisObjVM.AxisObject = item;
                    //        axisObjVM.NegButtonVisibility = false;
                    //        axisObjVM.PosButtonVisibility = false;

                    //        StageAxisObjectVmList.Add(axisObjVM);
                    //    }
                    //    else
                    //    {
                    //        var axisObjVM = new AxisObjectVM();
                    //        axisObjVM.AxisObject = item;

                    //        StageAxisObjectVmList.Add(axisObjVM);
                    //    }

                    //}
                    #region Axis Getval
                    IMotionManager Motionmanager = this.MotionManager();

                    if (Motionmanager != null)
                    {
                        XAxis = Motionmanager.GetAxis(EnumAxisConstants.X);
                        YAxis = Motionmanager.GetAxis(EnumAxisConstants.Y);
                        ZAxis = Motionmanager.GetAxis(EnumAxisConstants.Z);
                        CAxis = Motionmanager.GetAxis(EnumAxisConstants.C); //Wafer Chuck Rot.
                        TriAxis = Motionmanager.GetAxis(EnumAxisConstants.TRI);
                        PzAxis = Motionmanager.GetAxis(EnumAxisConstants.PZ);
                        AAxis = Motionmanager.GetAxis(EnumAxisConstants.A);
                        U1Axis = Motionmanager.GetAxis(EnumAxisConstants.U1);
                        U2Axis = Motionmanager.GetAxis(EnumAxisConstants.U2);
                        WAxis = Motionmanager.GetAxis(EnumAxisConstants.W);
                        VAxis = Motionmanager.GetAxis(EnumAxisConstants.V);
                        SCAxis = Motionmanager.GetAxis(EnumAxisConstants.SC);
                        //251029 yb add
                        CX1 = Motionmanager.GetAxis(EnumAxisConstants.CX1);
                        CY1 = Motionmanager.GetAxis(EnumAxisConstants.CY1);
                        CZ1 = Motionmanager.GetAxis(EnumAxisConstants.CZ1);

                        CX2 = Motionmanager.GetAxis(EnumAxisConstants.CX2);
                        CY2 = Motionmanager.GetAxis(EnumAxisConstants.CY2);
                        CZ2 = Motionmanager.GetAxis(EnumAxisConstants.CZ2);

                        CX3 = Motionmanager.GetAxis(EnumAxisConstants.CX3);
                        CY3 = Motionmanager.GetAxis(EnumAxisConstants.CY3);
                        CZ3 = Motionmanager.GetAxis(EnumAxisConstants.CZ3);

                        CX4 = Motionmanager.GetAxis(EnumAxisConstants.CX4);
                        CY4 = Motionmanager.GetAxis(EnumAxisConstants.CY4);
                        CZ4 = Motionmanager.GetAxis(EnumAxisConstants.CZ4);

                        CZ5 = Motionmanager.GetAxis(EnumAxisConstants.CZ5);

                        Z0 = Motionmanager.GetAxis(EnumAxisConstants.Z0);
                        Z1 = Motionmanager.GetAxis(EnumAxisConstants.Z1);
                        Z2 = Motionmanager.GetAxis(EnumAxisConstants.Z2);

                        FDT1 = Motionmanager.GetAxis(EnumAxisConstants.FDT1);
                        FDZ1 = Motionmanager.GetAxis(EnumAxisConstants.FDZ1);
                        EJX1 = Motionmanager.GetAxis(EnumAxisConstants.EJX1);
                        EJY1 = Motionmanager.GetAxis(EnumAxisConstants.EJY1);
                        EJZ1 = Motionmanager.GetAxis(EnumAxisConstants.EJZ1);
                        EJPZ1 = Motionmanager.GetAxis(EnumAxisConstants.EJPZ1);

                        NZD1 = Motionmanager.GetAxis(EnumAxisConstants.NZD1);

                        NSZ1 = Motionmanager.GetAxis(EnumAxisConstants.NSZ1);
                    }

                    #endregion
                    LoaderAxisObjectVmList = new ObservableCollection<AxisObjectVM>();
                    //foreach (var item in les.ProbeAxisProviders)
                    //{
                    //    var axisObjVM = new AxisObjectVM();
                    //    axisObjVM.AxisObject = item;

                    //    LoaderAxisObjectVmList.Add(axisObjVM);
                    //}

                    PosRefresh();

                    StageCamList = new ObservableCollection<StageCamera>();
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferHigh));
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferLow));
                    StageCamList.Add(new StageCamera(enumStageCamType.PinHigh));
                    StageCamList.Add(new StageCamera(enumStageCamType.PinLow));
                    StageCamList.Add(new StageCamera(enumStageCamType.MAP_REF));
                    StageCamList.Add(new StageCamera(enumStageCamType.UNDEFINED));

                    if (this.IOManager() != null)
                    {
                        OutputPorts.Clear();
                        InputPorts.Clear();
                        propertyInfos = this.IOManager().IO.Outputs.GetType().GetProperties();
                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.IOManager().IO.Outputs);
                                port = (IOPortDescripter<bool>)propObject;
                                OutputPorts.Add(port);
                                FilteredOutputPorts.Add(port);
                            }
                        }
                        propertyInfos = this.IOManager().IO.Inputs.GetType().GetProperties();
                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.IOManager().IO.Inputs);
                                port = (IOPortDescripter<bool>)propObject;
                                InputPorts.Add(port);
                                FilteredInputPorts.Add(port);
                            }
                        }
                        //port.Key
                    }

                    light = this.LightAdmin();
                    //foreach (var item in light.Lights)
                    //{
                    //    light.SetLight(item.ChannelMapIdx, (ushort)LightValue);
                    //    Lights.Add(item);
                    //}
                    for (int i = 0; i < 8; i++)
                    {
                        Lights.Add(new LightChannelType(EnumLightType.UNDEFINED, i));
                    }
                    SelectedLight = Lights[0];

                    for (int i = ((int)EnumProberCam.UNDEFINED + 1); i < ((int)EnumProberCam.CAM_LAST); i++)
                    {
                        CamChannels.Add(new CameraChannelType((EnumProberCam)i, i));
                    }
                    SelectedChannel = CamChannels[0];

                    if (this.MotionManager() != null)
                    {
                        if (this.MotionManager().GetAxis(EnumAxisConstants.R) == null || this.MotionManager().GetAxis(EnumAxisConstants.TT) == null)
                        {
                            EnableTiltElement = false;
                        }
                        else
                        {
                            EnableTiltElement = true;
                        }
                    }

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

                retval = EventCodeEnum.SYSTEM_ERROR;
            }

            return retval;
        }
        private void ChangeChannel(object obj)
        {
            try
            {
                var vm = this.VisionManager();
                vm.SwitchCamera(vm.GetCam(SelectedChannel.Type).Param, this);
                //vm.GetCam(SelectedChannel.Type).SwitchCamera();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void UpdateLight()
        {
            try
            {
                //light.SetLight(0, (ushort)LightValue);
                //light.SetLight(1, (ushort)LightValue);
                //light.SetLight(2, (ushort)LightValue);
                //light.SetLight(3, (ushort)LightValue);
                //light.SetLight(4, (ushort)LightValue);
                //light.SetLight(5, (ushort)LightValue);
                //light.SetLight(6, (ushort)LightValue);
                //light.SetLight(7, (ushort)LightValue);
                ushort lightValue = (ushort)LightValue;
                light.SetLight(SelectedLight.ChannelMapIdx.Value, lightValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async void SearchMatched()
        {
            try
            {
                string upper = SearchKeyword.ToUpper();
                string lower = SearchKeyword.ToLower();

                await Task.Run(() =>
                {
                    if (SearchKeyword.Length > 0)
                    {
                        var outs = OutputPorts.Where(
                            t => t.Key.Value.StartsWith(upper) ||
                            t.Key.Value.StartsWith(lower) ||
                            t.Key.Value.ToUpper().Contains(upper));
                        var filtered = new ObservableCollection<IOPortDescripter<bool>>(outs);

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredOutputPorts.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredOutputPorts.Add(item);
                                }
                            });
                        }


                        var inputs = InputPorts.Where(
                            t => t.Key.Value.StartsWith(upper) ||
                            t.Key.Value.StartsWith(lower) ||
                            t.Key.Value.ToUpper().Contains(upper));
                        filtered = new ObservableCollection<IOPortDescripter<bool>>(inputs);

                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredInputPorts.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredInputPorts.Add(item);
                                }
                            });

                        }
                    }
                    else
                    {
                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredInputPorts.Clear();
                                foreach (var item in InputPorts)
                                {
                                    FilteredInputPorts.Add(item);
                                }
                            });
                        }

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredOutputPorts.Clear();
                                foreach (var item in OutputPorts)
                                {
                                    FilteredOutputPorts.Add(item);
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private IZoomObject _ZoomObject;

        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set { _ZoomObject = value; }
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ZoomObject = Wafer;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                PosRefresh();
                //this.SysState().SetSetUpState();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stage3DModel = null;
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                });

                CenterView();
                IsItDisplayed2RateMagnification = false;
                // 251118 sebas : 매뉴얼조그 진입시 Wait 10초 뜨는거 제거
                // this.StageSupervisor().StageModuleState.ManualZDownMove();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        private RelayCommand<object> _SwitchPage;
        public ICommand SwitchPage
        {
            get
            {
                if (null == _SwitchPage) _SwitchPage = new RelayCommand<object>(PageSwitching);
                return _SwitchPage;
            }
        }
        private RelayCommand<CUI.Button> _OperatorPageSwitchCommand;
        public ICommand OperatorPageSwitchCommand
        {
            get
            {
                if (null == _OperatorPageSwitchCommand) _OperatorPageSwitchCommand = new RelayCommand<CUI.Button>(FuncOperatorPageSwitchCommand);
                return _OperatorPageSwitchCommand;
            }
        }

        private void FuncOperatorPageSwitchCommand(CUI.Button cuiparam)
        {
            try
            {
                this.ViewModelManager().ChangeFlyOutControlStatus(true);

                Guid ViewGUID = CUIServices.CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void PageSwitching(object obj)
        {
            try
            {
                this.ViewModelManager().ViewTransitionAsync(new Guid(obj.ToString()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.StageSupervisor().StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        #region Move
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
                    this.StageSupervisor().StageModuleState.ManualAbsMove(0, yaxis.Param.PosSWLimit.Value - 100, zaxis.Param.HomeOffset.Value);
                    //this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                //LoggerManager.Error($ex.Message);
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
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

                    this.MotionManager().StageMove(0, 0, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);

            }
            finally
            {
                PosRefresh();
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

                    this.MotionManager().StageMove(0, yaxis.Param.NegSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                PosRefresh();
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
                StageButtonsVisibility = false;
                double zoffset = 0;
                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.MoveLoadingPosition(zoffset);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
        }
        private AsyncCommand _UnLoadFromBackCommand;
        public ICommand UnLoadFromBackCommand
        {
            get
            {
                if (null == _UnLoadFromBackCommand) _UnLoadFromBackCommand = new AsyncCommand(UnLoadFromBackCommandPos);
                return _UnLoadFromBackCommand;
            }
        }
        private async Task UnLoadFromBackCommandPos()
        {
            try
            {

                EnumMessageDialogResult ret;

                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });

                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Hand UnLoading", "Turn Off the Chuck Vacuum??", EnumMessageStyle.AffirmativeAndNegative);


                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    //척 베큠 off
                }
                else
                {
                    //Dialog 쏘자
                }


            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {


            }
        }

        private AsyncCommand _LoadFromBackCommand;
        public ICommand LoadFromBackCommand
        {
            get
            {
                if (null == _LoadFromBackCommand) _LoadFromBackCommand = new AsyncCommand(LoadFromBackCommandPos);
                return _LoadFromBackCommand;
            }
        }
        private async Task LoadFromBackCommandPos()
        {
            try
            {

                EnumMessageDialogResult ret;

                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });

                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Hand UnLoading", "Turn On the Chuck Vacuum??", EnumMessageStyle.AffirmativeAndNegative);

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    //척 베큠 On
                }
                else
                {
                    //Dialog 쏘자
                }

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {


            }
        }
        public IViewModelManager ViewModelManager { get; set; }

        private int _ViewNUM;
        public int ViewNUM
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

        private bool _IsItDisplayed2RateMagnification;
        public bool IsItDisplayed2RateMagnification
        {
            get { return _IsItDisplayed2RateMagnification; }
            set
            {
                if (value != _IsItDisplayed2RateMagnification)
                {
                    _IsItDisplayed2RateMagnification = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void Viewx2() // 2x view
        {
            try
            {
                IsItDisplayed2RateMagnification = !IsItDisplayed2RateMagnification;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CWVIEW() //CW
        {
            try
            {
                ViewNUM = ((Enum.GetNames(typeof(CameraViewPoint)).Length) + (--ViewNUM)) % Enum.GetNames(typeof(CameraViewPoint)).Length;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CenterView() //FRONT
        {
            try
            {
                ViewNUM = 0;
                IsItDisplayed2RateMagnification = false;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CCWView() // CCW
        {
            try
            {
                ViewNUM = Math.Abs(++ViewNUM % Enum.GetNames(typeof(CameraViewPoint)).Length);
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _X2ViewChangeCommand;
        public RelayCommand X2ViewChangeCommand
        {
            get
            {
                if (null == _X2ViewChangeCommand) _X2ViewChangeCommand = new RelayCommand(Viewx2);
                return _X2ViewChangeCommand;
            }
        }


        private RelayCommand _CWViewChangeCommand;
        public ICommand CWViewChangeCommand
        {
            get
            {
                if (null == _CWViewChangeCommand) _CWViewChangeCommand = new RelayCommand(CWVIEW);
                return _CWViewChangeCommand;
            }
        }


        private RelayCommand _CenterViewChangeCommand;
        public ICommand CenterViewChangeCommand
        {
            get
            {
                if (null == _CenterViewChangeCommand) _CenterViewChangeCommand = new RelayCommand(CenterView);
                return _CenterViewChangeCommand;
            }
        }


        private RelayCommand _CCWViewChangeCommand;
        public ICommand CCWViewChangeCommand
        {
            get
            {
                if (null == _CCWViewChangeCommand) _CCWViewChangeCommand = new RelayCommand(CCWView);
                return _CCWViewChangeCommand;
            }
        }
        private void ChuckVacuum(string ONOFF)
        {
            try
            {
                if (ONOFF == "ON")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                    }
                }
                else if (ONOFF == "OFF")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ChuckVacuumCheck(string ONOFF)
        {
            try
            {
                if (ONOFF == "ON")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_12, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, true);
                    }
                }
                else if (ONOFF == "OFF")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_12, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, false);
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _ManualWaferCommand;
        public ICommand ManualWaferCommand
        {
            get
            {
                if (null == _ManualWaferCommand) _ManualWaferCommand = new AsyncCommand(ManualWaferCommandFunc);
                return _ManualWaferCommand;
            }
        }
        private async Task ManualWaferCommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;

                await Task.Run(async () =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, 0, zaxis.Param.HomeOffset.Value);
                    if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Unloading", "Do you want to remove the wafer from the chuck? ", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            // Size 별로 베큠 센서 조작 VAC OFF
                            ChuckVacuum("OFF");
                            ChuckVacuumCheck("OFF");

                            this.StageSupervisor().StageModuleState.Handlerhold(10000);

                            ret2 = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Unloading", "Did you remove the wafer from the chuck? ", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret2 == EnumMessageDialogResult.AFFIRMATIVE) // OK button
                            {
                                //3PIN Down
                                ChuckVacuum("ON");
                                this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "The wafer was not removed from the chuck.", EnumMessageStyle.Affirmative);
                                }
                                else if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                                {
                                    ChuckVacuum("OFF");

                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Remove wafer done.", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                //척 배큠 ON
                                ChuckVacuum("ON");

                                this.StageSupervisor().StageModuleState.Handlerrelease(10000);

                                if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "The wafer was not removed from the chuck.", EnumMessageStyle.Affirmative);
                                }
                                else if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                                {
                                    ChuckVacuum("OFF");

                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Unload wafer done.", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                        else
                        {

                        }

                    }

                    else if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)  // Load 해야하는 상황
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Hand Loading", "Do you want to load the wafer onto Chuck ? ", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            ChuckVacuum("OFF");
                            ChuckVacuumCheck("OFF");

                            this.StageSupervisor().StageModuleState.Handlerhold(10000);

                            ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Loading", "Did you put the wafer on Three Pin? ", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                ChuckVacuum("ON");
                                this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Load wafer done.", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    ChuckVacuum("OFF");
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "No Wafer on chuck.", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                // 준비 ㄴㄴ
                                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Loading", "Cancel the wafer loading? ", EnumMessageStyle.AffirmativeAndNegative);

                                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Cancel.", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Loading", "Did you put the wafer on Three Pin? ", EnumMessageStyle.AffirmativeAndNegative);

                                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                                    {
                                        ChuckVacuum("ON");
                                        this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                        if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST)
                                        {
                                            await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Load wafer done.", EnumMessageStyle.Affirmative);
                                        }
                                        else
                                        {
                                            ChuckVacuum("OFF");
                                            await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "No Wafer on chuck.", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                    else
                                    {
                                        this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                        await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Cancel.", EnumMessageStyle.Affirmative);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ////Motion Cancel
                        }

                    }
                });


            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {


            }
        }

        private AsyncCommand _ManualPreCommand;
        public ICommand ManualPreCommand
        {
            get
            {
                if (null == _ManualPreCommand) _ManualPreCommand = new AsyncCommand(ManualPreCommandFunc);
                return _ManualPreCommand;
            }
        }
        private async Task ManualPreCommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;

                await Task.Run(async () =>
                {
                    if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOSUBCHUCKAIRON, true);
                    }
                    else if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOSUBCHUCKAIRON, false);
                    }

                });

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {


            }
        }
        private AsyncCommand _ManualArmCommand;
        public ICommand ManualArmCommand
        {
            get
            {
                if (null == _ManualArmCommand) _ManualArmCommand = new AsyncCommand(ManualArmCommandFunc);
                return _ManualArmCommand;
            }
        }
        private async Task ManualArmCommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;

                await Task.Run(async () =>
                {
                    if (LoaderController.LoaderInfo.StateMap.ARMModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARMAIRON, true);
                    }
                    else if (LoaderController.LoaderInfo.StateMap.ARMModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARMAIRON, false);
                    }

                });

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {


            }
        }
        private AsyncCommand _ManualArm2Command;
        public ICommand ManualArm2Command
        {
            get
            {
                if (null == _ManualArm2Command) _ManualArm2Command = new AsyncCommand(ManualArm2CommandFunc);
                return _ManualArm2Command;
            }
        }
        private async Task ManualArm2CommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;

                await Task.Run(async () =>
                {
                    if (LoaderController.LoaderInfo.StateMap.ARMModules[1].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARM2AIRON, true);
                    }
                    else if (LoaderController.LoaderInfo.StateMap.ARMModules[1].WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARM2AIRON, false);
                    }

                });

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {


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
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
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
            }
            catch (Exception ex)
            {
                StageButtonsVisibility = true;
                LoggerManager.Error(ex.Message);

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
                await Task.Run(() =>
                {
                    switch (SelectedCam)
                    {
                        case enumStageCamType.UNDEFINED:
                            break;
                        case enumStageCamType.WaferHigh:
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, Thickness);
                            break;
                        case enumStageCamType.WaferLow:
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Thickness);
                            break;
                        case enumStageCamType.PinHigh:
                            this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, pinHeight);
                            break;
                        case enumStageCamType.PinLow:
                            this.StageSupervisor().StageModuleState.PinLowViewMove(0, 0, pinHeight);
                            break;
                        case enumStageCamType.MAP_REF:
                            this.StageSupervisor().StageModuleState.PinLowViewMove(0, 0, pinHeight);
                            break;
                        default:
                            break;
                    }

                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
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
                //LoggerManager.Error($ex.Message);
            }
        }

        private AsyncCommand _AutoTiltCommand;
        public ICommand AutoTiltCommand
        {
            get
            {
                if (null == _AutoTiltCommand) _AutoTiltCommand = new AsyncCommand(AutoTiltFunc);
                return _AutoTiltCommand;
            }
        }
        private async Task AutoTiltFunc()
        {
        }

        private AsyncCommand _AutoTiltStopCommand;
        public ICommand AutoTiltStopCommand
        {
            get
            {
                if (null == _AutoTiltStopCommand) _AutoTiltStopCommand = new AsyncCommand(AutoTiltStopFunc);
                return _AutoTiltStopCommand;
            }
        }
        private async Task AutoTiltStopFunc()
        {
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
                //LoggerManager.Error($err.Message);
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
                //LoggerManager.Error($err.Message);
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
                //LoggerManager.Error($err.Message);
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
            this.MotionManager().SetDualLoop(false);
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
            try
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

        private RelayCommand<object> _LoaderDoorCloseCommand;
        public ICommand LoaderDoorCloseCommand
        {
            get
            {
                if (_LoaderDoorCloseCommand == null) _LoaderDoorCloseCommand = new RelayCommand<object>(LoaderDoorCloseCmdFunc);
                return _LoaderDoorCloseCommand;
            }
        }

        private void LoaderDoorCloseCmdFunc(object noparam)
        {

            try
            {
                var ret = this.StageSupervisor().StageModuleState.LoaderDoorClose();
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }



        }

        private RelayCommand<object> _LoaderDoorOpenCommand;
        public ICommand LoaderDoorOpenCommand
        {
            get
            {
                if (_LoaderDoorOpenCommand == null) _LoaderDoorOpenCommand = new RelayCommand<object>(LoaderDoorOpenCmdFunc);
                return _LoaderDoorOpenCommand;
            }
        }

        private void LoaderDoorOpenCmdFunc(object noparam)
        {
            try
            {
                var ret = this.StageSupervisor().StageModuleState.LoaderDoorOpen();
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _FrontDoorUnLockCommand;
        public ICommand FrontDoorUnLockCommand
        {
            get
            {
                if (_FrontDoorUnLockCommand == null) _FrontDoorUnLockCommand = new RelayCommand<object>(FrontDoorUnLockCmdFunc);
                return _FrontDoorUnLockCommand;
            }
        }

        private void FrontDoorUnLockCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] FrontDoorUnLockCmdFunc is not implemented.");
        }

        private RelayCommand<object> _FrontDoorLockCommand;
        public ICommand FrontDoorLockCommand
        {
            get
            {
                if (_FrontDoorLockCommand == null) _FrontDoorLockCommand = new RelayCommand<object>(FrontDoorLockCmdFunc);
                return _FrontDoorLockCommand;
            }
        }

        private void FrontDoorLockCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] FrontDoorLockCmdFunc is not implemented.");
        }

        private RelayCommand<object> _TriDNCommand;
        public ICommand TriDNCommand
        {
            get
            {
                if (_TriDNCommand == null) _TriDNCommand = new RelayCommand<object>(TriDNCmdFunc);
                return _TriDNCommand;
            }
        }

        private void TriDNCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] TriDNCmdFunc is not implemented.");
        }

        private RelayCommand<object> _TriUPCommand;
        public ICommand TriUPCommand
        {
            get
            {
                if (_TriUPCommand == null) _TriUPCommand = new RelayCommand<object>(TriUPCmdFunc);
                return _TriUPCommand;
            }
        }

        private void TriUPCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] TriUPCmdFunc is not implemented.");
        }

        private RelayCommand<object> _ChuckVacOffCommand;
        public ICommand ChuckVacOffCommand
        {
            get
            {
                if (_ChuckVacOffCommand == null) _ChuckVacOffCommand = new RelayCommand<object>(ChuckVacOffCmdFunc);
                return _ChuckVacOffCommand;
            }
        }

        private void ChuckVacOffCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] ChuckVacOffCmdFunc is not implemented.");
        }

        private RelayCommand<object> _ChuckVacOnCommand;
        public ICommand ChuckVacOnCommand
        {
            get
            {
                if (_ChuckVacOnCommand == null) _ChuckVacOnCommand = new RelayCommand<object>(ChuckVacOnCmdFunc);
                return _ChuckVacOnCommand;
            }
        }

        private void ChuckVacOnCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] ChuckVacOnCmdFunc is not implemented.");
        }

        private RelayCommand<object> _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (_FocusingCommand == null) _FocusingCommand = new RelayCommand<object>(FocusingCmdFunc);
                return _FocusingCommand;
            }
        }

        private void FocusingCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] FocusingCmdFunc is not implemented.");
        }



        #endregion

        #region I/O ON OFF
        //251103 YB IO test
        private RelayCommand _Arm_Air1_OFFCommand;
        public ICommand Arm_Air1_OFFCommand
        {
            get
            {
                if (null == _Arm_Air1_OFFCommand) _Arm_Air1_OFFCommand = new RelayCommand(Arm_Air1_OFF);
                return _Arm_Air1_OFFCommand;
            }
        }

        private void Arm_Air1_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, true); //air off
            Thread.Sleep(250);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, false);
        }

        private RelayCommand _Arm_Air1_ONCommand;
        public ICommand Arm_Air1_ONCommand
        {
            get
            {
                if (null == _Arm_Air1_ONCommand) _Arm_Air1_ONCommand = new RelayCommand(Arm_Air1_ON);
                return _Arm_Air1_ONCommand;
            }
        }

        private void Arm_Air1_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, true); //air on
            Thread.Sleep(100);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, false);

        }

        private RelayCommand _MAGNETIC1_OFFCommand;
        public ICommand MAGNETIC1_OFFCommand
        {
            get
            {
                if (null == _MAGNETIC1_OFFCommand) _MAGNETIC1_OFFCommand = new RelayCommand(MAGNETIC1_OFF);
                return _MAGNETIC1_OFFCommand;
            }
        }

        private void MAGNETIC1_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, false);
        }

        private RelayCommand _MAGNETIC1_ONCommand;
        public ICommand MAGNETIC1_ONCommand
        {
            get
            {
                if (null == _MAGNETIC1_ONCommand) _MAGNETIC1_ONCommand = new RelayCommand(MAGNETIC1_ON);
                return _MAGNETIC1_ONCommand;
            }
        }

        private void MAGNETIC1_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, true);
        }

        private RelayCommand _ARM_VACOFF1Command;
        public ICommand ARM_VACOFF1Command
        {
            get
            {
                if (null == _ARM_VACOFF1Command) _ARM_VACOFF1Command = new RelayCommand(ARM_VACOFF1);
                return _ARM_VACOFF1Command;
            }
        }

        private void ARM_VACOFF1()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, false);
        }

        private RelayCommand _ARM_VACON1Command;
        public ICommand ARM_VACON1Command
        {
            get
            {
                if (null == _ARM_VACON1Command) _ARM_VACON1Command = new RelayCommand(ARM_VACON1);
                return _ARM_VACON1Command;
            }
        }

        private void ARM_VACON1()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, false);
        }

        private RelayCommand _ARM_BlowOFF1Command;
        public ICommand ARM_BlowOFF1Command
        {
            get
            {
                if (null == _ARM_BlowOFF1Command) _ARM_BlowOFF1Command = new RelayCommand(ARM_BlowOFF1);
                return _ARM_BlowOFF1Command;
            }
        }

        private void ARM_BlowOFF1()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, false);
        }

        private RelayCommand _ARM_BlowON1Command;
        public ICommand ARM_BlowON1Command
        {
            get
            {
                if (null == _ARM_BlowON1Command) _ARM_BlowON1Command = new RelayCommand(ARM_BlowON1);
                return _ARM_BlowON1Command;
            }
        }

        private void ARM_BlowON1()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, false);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, true);
        }

        private RelayCommand _ARM_VACOFF2Command;
        public ICommand ARM_VACOFF2Command
        {
            get
            {
                if (null == _ARM_VACOFF2Command) _ARM_VACOFF2Command = new RelayCommand(ARM_VACOFF2);
                return _ARM_VACOFF2Command;
            }
        }

        private void ARM_VACOFF2()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, true);
            Thread.Sleep(40);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, false);
        }

        private RelayCommand _ARM_VACON2Command;
        public ICommand ARM_VACON2Command
        {
            get
            {
                if (null == _ARM_VACON2Command) _ARM_VACON2Command = new RelayCommand(ARM_VACON2);
                return _ARM_VACON2Command;
            }
        }

        private void ARM_VACON2()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, false);
        }

        private RelayCommand _ARM_VAC_BlowOFF2Command;
        public ICommand ARM_VAC_BlowOFF2Command
        {
            get
            {
                if (null == _ARM_VAC_BlowOFF2Command) _ARM_VAC_BlowOFF2Command = new RelayCommand(ARM_VAC_BlowOFF2);
                return _ARM_VAC_BlowOFF2Command;
            }
        }

        private void ARM_VAC_BlowOFF2()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, false);
        }

        private RelayCommand _ARM_VAC_BlowON2Command;
        public ICommand ARM_VAC_BlowON2Command
        {
            get
            {
                if (null == _ARM_VAC_BlowON2Command) _ARM_VAC_BlowON2Command = new RelayCommand(ARM_VAC_BlowON2);
                return _ARM_VAC_BlowON2Command;
            }
        }

        private void ARM_VAC_BlowON2()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, false);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, true);
        }

        private RelayCommand _Arm_Air2_OFFCommand;
        public ICommand Arm_Air2_OFFCommand
        {
            get
            {
                if (null == _Arm_Air2_OFFCommand) _Arm_Air2_OFFCommand = new RelayCommand(Arm_Air2_OFF);
                return _Arm_Air2_OFFCommand;
            }
        }

        private void Arm_Air2_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, true); //air off
            Thread.Sleep(250);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, false);
        }

        private RelayCommand _Arm_Air2_ONCommand;
        public ICommand Arm_Air2_ONCommand
        {
            get
            {
                if (null == _Arm_Air2_ONCommand) _Arm_Air2_ONCommand = new RelayCommand(Arm_Air2_ON);
                return _Arm_Air2_ONCommand;
            }
        }

        private void Arm_Air2_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, true); //air on
            Thread.Sleep(250);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, false);

        }

        private RelayCommand _DOCHUCKAIRON_0_OFFCommand;
        public ICommand DOCHUCKAIRON_0_OFFCommand
        {
            get
            {
                if (null == _DOCHUCKAIRON_0_OFFCommand) _DOCHUCKAIRON_0_OFFCommand = new RelayCommand(DOCHUCKAIRON_0_OFF);
                return _DOCHUCKAIRON_0_OFFCommand;
            }
        }

        private void DOCHUCKAIRON_0_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
        }

        private RelayCommand _DOCHUCKAIRON_0Command;
        public ICommand DOCHUCKAIRON_0Command
        {
            get
            {
                if (null == _DOCHUCKAIRON_0Command) _DOCHUCKAIRON_0Command = new RelayCommand(DOCHUCKAIRON_0);
                return _DOCHUCKAIRON_0Command;
            }
        }

        private void DOCHUCKAIRON_0()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
        }

        private RelayCommand _DOCHUCKAIRON_2_OFFCommand;
        public ICommand DOCHUCKAIRON_2_OFFCommand
        {
            get
            {
                if (null == _DOCHUCKAIRON_2_OFFCommand) _DOCHUCKAIRON_2_OFFCommand = new RelayCommand(DOCHUCKAIRON_2_OFF);
                return _DOCHUCKAIRON_2_OFFCommand;
            }
        }

        private void DOCHUCKAIRON_2_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);
        }

        private RelayCommand _DOCHUCKAIRON_2Command;
        public ICommand DOCHUCKAIRON_2Command
        {
            get
            {
                if (null == _DOCHUCKAIRON_2Command) _DOCHUCKAIRON_2Command = new RelayCommand(DOCHUCKAIRON_2);
                return _DOCHUCKAIRON_2Command;
            }
        }

        private void DOCHUCKAIRON_2()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
        }

        private RelayCommand _TRILEG_SUCTION_OFFCommand;
        public ICommand TRILEG_SUCTION_OFFCommand
        {
            get
            {
                if (null == _TRILEG_SUCTION_OFFCommand) _TRILEG_SUCTION_OFFCommand = new RelayCommand(TRILEG_SUCTION_OFF);
                return _TRILEG_SUCTION_OFFCommand;
            }
        }

        private void TRILEG_SUCTION_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_TRILEG_SUCTION, false);
        }

        private RelayCommand _TRILEG_SUCTION_ONCommand;
        public ICommand TRILEG_SUCTION_ONCommand
        {
            get
            {
                if (null == _TRILEG_SUCTION_ONCommand) _TRILEG_SUCTION_ONCommand = new RelayCommand(TRILEG_SUCTION_ON);
                return _TRILEG_SUCTION_ONCommand;
            }
        }

        private void TRILEG_SUCTION_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_TRILEG_SUCTION, true);
        }

        private RelayCommand _FD_VAC_OFFCommand;
        public ICommand FD_VAC_OFFCommand
        {
            get
            {
                if (null == _FD_VAC_OFFCommand) _FD_VAC_OFFCommand = new RelayCommand(FD_VAC_OFF);
                return _FD_VAC_OFFCommand;
            }
        }

        private void FD_VAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_FD_VAC, false);
        }

        private RelayCommand _FD_VAC_ONCommand;
        public ICommand FD_VAC_ONCommand
        {
            get
            {
                if (null == _FD_VAC_ONCommand) _FD_VAC_ONCommand = new RelayCommand(FD_VAC_ON);
                return _FD_VAC_ONCommand;
            }
        }

        private void FD_VAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_FD_VAC, true);
        }

        private RelayCommand _EJ_VAC_OFFCommand;
        public ICommand EJ_VAC_OFFCommand
        {
            get
            {
                if (null == _EJ_VAC_OFFCommand) _EJ_VAC_OFFCommand = new RelayCommand(EJ_VAC_OFF);
                return _EJ_VAC_OFFCommand;
            }
        }

        private void EJ_VAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _EJ_VAC_ONCommand;
        public ICommand EJ_VAC_ONCommand
        {
            get
            {
                if (null == _EJ_VAC_ONCommand) _FD_VAC_ONCommand = new RelayCommand(EJ_VAC_ON);
                return _EJ_VAC_ONCommand;
            }
        }

        private void EJ_VAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }

        //251208 ybpark
        private RelayCommand _FDHolder3Leg_OFFCommand;
        public ICommand FDHolder3Leg_OFFCommand
        {
            get
            {
                if (null == _FDHolder3Leg_OFFCommand) _FDHolder3Leg_OFFCommand = new RelayCommand(FDHolder3Leg_OFF);
                return _FDHolder3Leg_OFFCommand;
            }
        }

        private void FDHolder3Leg_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _FDHolder3Leg_ONCommand;
        public ICommand FDHolder3Leg_ONCommand
        {
            get
            {
                if (null == _FDHolder3Leg_ONCommand) _FDHolder3Leg_ONCommand = new RelayCommand(FDHolder3Leg_ON);
                return _FDHolder3Leg_ONCommand;
            }
        }

        private void FDHolder3Leg_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }

        private RelayCommand _FDHolder1Leg_OFFCommand;
        public ICommand FDHolder1Leg_OFFCommand
        {
            get
            {
                if (null == _FDHolder1Leg_OFFCommand) _FDHolder1Leg_OFFCommand = new RelayCommand(FDHolder1Leg_OFF);
                return _FDHolder1Leg_OFFCommand;
            }
        }

        private void FDHolder1Leg_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _FDHolder1Leg_ONCommand;
        public ICommand FDHolder1Leg_ONCommand
        {
            get
            {
                if (null == _FDHolder1Leg_ONCommand) _FDHolder1Leg_ONCommand = new RelayCommand(FDHolder1Leg_ON);
                return _FDHolder1Leg_ONCommand;
            }
        }

        private void FDHolder1Leg_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }
        private RelayCommand _ARM40_OFFCommand;
        public ICommand ARM40_OFFCommand
        {
            get
            {
                if (null == _ARM40_OFFCommand) _ARM40_OFFCommand = new RelayCommand(ARM40_OFF);
                return _ARM40_OFFCommand;
            }
        }

        private void ARM40_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _ARM40_ONCommand;
        public ICommand ARM40_ONCommand
        {
            get
            {
                if (null == _ARM40_ONCommand) _ARM40_ONCommand = new RelayCommand(ARM40_ON);
                return _ARM40_ONCommand;
            }
        }

        private void ARM40_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }

        private RelayCommand _PREVAC_OFFCommand;
        public ICommand PREVAC_OFFCommand
        {
            get
            {
                if (null == _PREVAC_OFFCommand) _PREVAC_OFFCommand = new RelayCommand(PREVAC_OFF);
                return _PREVAC_OFFCommand;
            }
        }

        private void PREVAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _PREVAC_ONCommand;
        public ICommand PREVAC_ONCommand
        {
            get
            {
                if (null == _PREVAC_ONCommand) _PREVAC_ONCommand = new RelayCommand(PREVAC_ON);
                return _PREVAC_ONCommand;
            }
        }

        private void PREVAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }

        private RelayCommand _ARM1VAC_OFFCommand;
        public ICommand ARM1VAC_OFFCommand
        {
            get
            {
                if (null == _ARM1VAC_OFFCommand) _ARM1VAC_OFFCommand = new RelayCommand(ARM1VAC_OFF);
                return _ARM1VAC_OFFCommand;
            }
        }

        private void ARM1VAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _ARM1VAC_ONCommand;
        public ICommand ARM1VAC_ONCommand
        {
            get
            {
                if (null == _ARM1VAC_ONCommand) _ARM1VAC_ONCommand = new RelayCommand(ARM1VAC_ON);
                return _ARM1VAC_ONCommand;
            }
        }

        private void ARM1VAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }

        private RelayCommand _ARM2VAC_OFFCommand;
        public ICommand ARM2VAC_OFFCommand
        {
            get
            {
                if (null == _ARM2VAC_OFFCommand) _ARM2VAC_OFFCommand = new RelayCommand(ARM2VAC_OFF);
                return _ARM2VAC_OFFCommand;
            }
        }

        private void ARM2VAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        }

        private RelayCommand _ARM2VAC_ONCommand;
        public ICommand ARM2VAC_ONCommand
        {
            get
            {
                if (null == _ARM2VAC_ONCommand) _ARM2VAC_ONCommand = new RelayCommand(ARM2VAC_ON);
                return _ARM2VAC_ONCommand;
            }
        }

        private void ARM2VAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
        }

        // 260706 Remy
        private RelayCommand _HOLD1VAC_OFFCommand;
        public ICommand HOLD1VAC_OFFCommand
        {
            get
            {
                if (null == _HOLD1VAC_OFFCommand) _HOLD1VAC_OFFCommand = new RelayCommand(HOLD1VAC_OFF);
                return _HOLD1VAC_OFFCommand;
            }
        }

        private void HOLD1VAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF1, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF1, false);
        }

        private RelayCommand _HOLD1VAC_ONCommand;
        public ICommand HOLD1VAC_ONCommand
        {
            get
            {
                if (null == _HOLD1VAC_ONCommand) _HOLD1VAC_ONCommand = new RelayCommand(HOLD1VAC_ON);
                return _HOLD1VAC_ONCommand;
            }
        }

        private void HOLD1VAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON1, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON1, false);
        }
        // 260706 Remy
        private RelayCommand _HOLD2VAC_OFFCommand;
        public ICommand HOLD2VAC_OFFCommand
        {
            get
            {
                if (null == _HOLD2VAC_OFFCommand) _HOLD2VAC_OFFCommand = new RelayCommand(HOLD2VAC_OFF);
                return _HOLD2VAC_OFFCommand;
            }
        }

        private void HOLD2VAC_OFF()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF2, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF2, false);
        }

        private RelayCommand _HOLD2VAC_ONCommand;
        public ICommand HOLD2VAC_ONCommand
        {
            get
            {
                if (null == _HOLD2VAC_ONCommand) _HOLD2VAC_ONCommand = new RelayCommand(HOLD2VAC_ON);
                return _HOLD2VAC_ONCommand;
            }
        }

        private void HOLD2VAC_ON()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON2, true);
            Thread.Sleep(50);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON2, false);
        }

        #endregion

        private AsyncCommand _CAM_HomingCommand;
        public ICommand CAM_HomingCommand
        {
            get
            {
                if (null == _CAM_HomingCommand) _CAM_HomingCommand = new AsyncCommand(CAMH_oming_Func);
                return _CAM_HomingCommand;
            }
        }

        private async Task CAMH_oming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.CX1, EnumAxisConstants.CY1, EnumAxisConstants.CZ1,
                                                            EnumAxisConstants.CX2, EnumAxisConstants.CY2, EnumAxisConstants.CZ2,
                                                            EnumAxisConstants.CX3, EnumAxisConstants.CY3, EnumAxisConstants.CZ3,
                                                            EnumAxisConstants.CX4, EnumAxisConstants.CY4, EnumAxisConstants.CZ4,
                                                            EnumAxisConstants.CZ5);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }

        private AsyncCommand _NANO_HomingCommand;
        public ICommand NANO_HomingCommand
        {
            get
            {
                if (null == _NANO_HomingCommand) _NANO_HomingCommand = new AsyncCommand(NANOHoming_Func);
                return _NANO_HomingCommand;
            }
        }

        private async Task NANOHoming_Func()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                EventCodeEnum ret = EventCodeEnum.NODATA;
                ProbeAxisObject axisNSZ1 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ1);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.NSZ1);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                Thread.Sleep(250);

                // Nano Z 위로 이동 (대기 위치)
                double pos = 1500;
                retVal = this.MotionManager().RelMove(axisNSZ1, pos, axisNSZ1.Param.Speed.Value, axisNSZ1.Param.Acceleration.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }

        private AsyncCommand _DD_HomingCommand;
        public ICommand DD_HomingCommand
        {
            get
            {
                if (null == _DD_HomingCommand) _DD_HomingCommand = new AsyncCommand(DDHoming_Func);
                return _DD_HomingCommand;
            }
        }

        private async Task DDHoming_Func()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                EventCodeEnum ret = EventCodeEnum.NODATA;
                ProbeAxisObject axisNZD1 = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.NZD1);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                Thread.Sleep(250);

                // 260705 sebas : Arm 정렬을 위한 DD pos 이동 보정
                ProbeAxisObject AxisObjectNZD1 = axisNZD1;
                double pos = 300;
                retVal = this.MotionManager().RelMove_Wating(AxisObjectNZD1, pos, AxisObjectNZD1.Param.Speed.Value, AxisObjectNZD1.Param.Acceleration.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }

        private AsyncCommand _Wafer_HomingCommand;
        public ICommand Wafer_HomingCommand
        {
            get
            {
                if (null == _Wafer_HomingCommand) _Wafer_HomingCommand = new AsyncCommand(WaferHoming_Func);
                return _Wafer_HomingCommand;
            }
        }

        private async Task WaferHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.Z0, EnumAxisConstants.Z1, EnumAxisConstants.Z2);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.C);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.TRI);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }

        private AsyncCommand _FD_HomingCommand;
        public ICommand FD_HomingCommand
        {
            get
            {
                if (null == _FD_HomingCommand) _FD_HomingCommand = new AsyncCommand(FDHoming_Func);
                return _FD_HomingCommand;
            }
        }

        private async Task FDHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //var sw = System.Diagnostics.Stopwatch.StartNew();

                //int totalGrabCount = 0;
                //int timeoutCount = 0;

                //while (sw.Elapsed < TimeSpan.FromSeconds(1))
                //{
                //    _VisionVM.RequestSaveNextFrameRaw(3);

                //    // 프레임 복사 완료를 최대 50ms 대기
                //    bool pass = _VisionVM.WaitNextFrameCopiedAck(3, 50);

                //    if (pass)
                //    {
                //        totalGrabCount++;
                //    }
                //    else
                //    {
                //        timeoutCount++;
                //        LoggerManager.Debug("Next frame copy ACK timeout (<=50ms). Continue Z Up.");
                //    }
                //}

                //sw.Stop();

                //double elapsedSec = sw.Elapsed.TotalSeconds;
                //double fps = totalGrabCount / elapsedSec;

                //LoggerManager.Debug("========== Grab Test Result ==========");
                //LoggerManager.Debug($"Test Time        : {elapsedSec:F3} sec");
                //LoggerManager.Debug($"Total Grab Count : {totalGrabCount}");
                //LoggerManager.Debug($"Timeout Count    : {timeoutCount}");
                //LoggerManager.Debug($"Average FPS      : {fps:F2}");
                //LoggerManager.Debug("======================================");

                // 20260630 LJH 주석
                // 260206 sebas : 개조 후 이젝션 제외
                //ProbeAxisObject axisEJZ1 = this.MotionManager().GetAxis(EnumAxisConstants.EJZ1);
                //Thread.Sleep(250);
                //// Ejection Z축
                //ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.EJZ1);
                //ResultValidate(MethodBase.GetCurrentMethod(), ret);

                //Thread.Sleep(250);

                //// Ejection Z 위로 이동 (relmove)
                //ProbeAxisObject AxisObjectEJZ1 = axisEJZ1;
                //double pos = 4000; //251119 ybpark 기존 4에서 4000으로 수정

                //retVal = this.MotionManager().RelMove(AxisObjectEJZ1, pos, AxisObjectEJZ1.Param.Speed.Value, AxisObjectEJZ1.Param.Acceleration.Value);

                ProbeAxisObject axisFDZ1 = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);
                // FD 척 Z 축
                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.FDZ1);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
                Thread.Sleep(250);

                // FD 척 위로 이동 (relmove)
                ProbeAxisObject AxisObjectFDZ1 = axisFDZ1;
                double pos = 4770; // 20251118 Nick um제어를 위해 (4.77 -> 4770)

                retVal = this.MotionManager().RelMove(AxisObjectFDZ1, pos, AxisObjectFDZ1.Param.Speed.Value, AxisObjectFDZ1.Param.Acceleration.Value);
                Thread.Sleep(250);

                if (retVal == EventCodeEnum.NONE)
                {
                    //// Ejection X, Y 및 FD척 theta
                    //ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.EJX1, EnumAxisConstants.EJY1, EnumAxisConstants.EJPZ1);
                    //ResultValidate(MethodBase.GetCurrentMethod(), ret);


                    // 260705 sebas : PickCAM Z축 호밍 추가
                    ProbeAxisObject axisNSZ2 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ2);
                    ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.NSZ2);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                    Thread.Sleep(250);

                    // 260705 sebas : PickCAM Z축 호밍 후 이동 추가
                    ProbeAxisObject AxisObjectNSZ2 = axisNSZ2;
                    pos = 1200;
                    retVal = this.MotionManager().RelMove(AxisObjectNSZ2, pos, AxisObjectNSZ2.Param.Speed.Value, AxisObjectNSZ2.Param.Acceleration.Value);
                    Thread.Sleep(250);

                    ProbeAxisObject axisFDT1 = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);
                    ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.FDT1);
                    ProbeAxisObject AxisObjectFDT1 = axisFDT1;
                    pos = 50000000;    // 260224 sebas : 5000 -> 50000000 얼라인 후 +-세타 움직임때문
                    retVal = this.MotionManager().RelMove(AxisObjectFDT1, pos, AxisObjectFDT1.Param.Speed.Value, AxisObjectFDT1.Param.Acceleration.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {

            }
        }

        private AsyncCommand _XY_HomingCommand;
        public ICommand XY_HomingCommand
        {
            get
            {
                if (null == _XY_HomingCommand) _XY_HomingCommand = new AsyncCommand(XYHoming_Func);
                return _XY_HomingCommand;
            }
        }

        private async Task XYHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.X);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.X);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);

                Thread.Sleep(250);

                // X축 중앙으로 이동 (relmove)
                AxisObject = axis;
                double pos = 50000; // 20251118 Nick (um 단위로 변경을 위해 50 -> 50000)
                retVal = this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);

                if (retVal == EventCodeEnum.NONE)
                {
                    ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.Y);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        //251106 yb 전체 호밍 추가
        private AsyncCommand _TotalModuleHomingCommand;
        public ICommand TotalModuleHomingCommand
        {
            get
            {
                if (null == _TotalModuleHomingCommand) _TotalModuleHomingCommand = new AsyncCommand(TotalModuleHoming_Func);
                return _TotalModuleHomingCommand;
            }
        }

        private async Task TotalModuleHoming_Func()
        {
            //CAMH_oming_Func();
            //NANOHoming_Func();
            //DDHoming_Func();
            //WaferHoming_Func();
            //FDHoming_Func();
            //XYHoming_Func();

            EventCodeEnum ret = EventCodeEnum.NODATA;

            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.CX1, EnumAxisConstants.CY1, EnumAxisConstants.CZ1,
                                                        EnumAxisConstants.CX2, EnumAxisConstants.CY2, EnumAxisConstants.CZ2,
                                                        EnumAxisConstants.CX3, EnumAxisConstants.CY3, EnumAxisConstants.CZ3,
                                                        EnumAxisConstants.CX4, EnumAxisConstants.CY4, EnumAxisConstants.CZ4,
                                                        EnumAxisConstants.CZ5, EnumAxisConstants.X, EnumAxisConstants.EJZ1, EnumAxisConstants.FDZ1, EnumAxisConstants.FDT1,
                                                        EnumAxisConstants.C, EnumAxisConstants.TRI, EnumAxisConstants.Z0, EnumAxisConstants.Z1,
                                                        EnumAxisConstants.Z2, EnumAxisConstants.EJPZ1);
            ResultValidate(MethodBase.GetCurrentMethod(), ret);

            // 호밍그룹1
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var axisEnums = new EnumAxisConstants[]
            {
                EnumAxisConstants.CX1, EnumAxisConstants.CY1, EnumAxisConstants.CZ1,
                EnumAxisConstants.CX2, EnumAxisConstants.CY2, EnumAxisConstants.CZ2,
                EnumAxisConstants.CX3, EnumAxisConstants.CY3, EnumAxisConstants.CZ3,
                EnumAxisConstants.CX4, EnumAxisConstants.CY4, EnumAxisConstants.CZ4,
                EnumAxisConstants.CZ5, EnumAxisConstants.X, EnumAxisConstants.EJZ1, EnumAxisConstants.FDZ1,EnumAxisConstants.FDT1
            };

            ProbeAxisObject[] axes = axisEnums
                .Select(axis => this.MotionManager().GetAxis(axis))
                .ToArray();

            double pos = 15000; //251119 ybpark 기존 15 에서 15000 이하 밑에 pos부분은 기존 값에서 1000곱한 값임!

            for (int i = 0; i < 17; i++)
            {
                if (i == 13) //X 
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos + 170000, axes[i].Param.Speed.Value * 2, axes[i].Param.Acceleration.Value * 2);
                }
                else if (i == 14) //EJZ1
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos - 5000, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
                else if (i == 15)//FDZ1
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos - 10000, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
                else if (i == 16)//FDT1
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos - 10000, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
                else
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
            }

            Arms_Air_On();

            // 호밍그룹2 : Base Y , Nano Z , Eject X, Eject Y
            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.Y, EnumAxisConstants.NSZ1, EnumAxisConstants.EJX1, EnumAxisConstants.EJY1);

            var axisEnums1 = new EnumAxisConstants[]
            {
                EnumAxisConstants.Y, EnumAxisConstants.NSZ1, EnumAxisConstants.NZD1
            };

            ProbeAxisObject[] axes1 = axisEnums1
                .Select(axis1 => this.MotionManager().GetAxis(axis1))
                .ToArray();

            //for (int i = 0; i < 2; i++) // 251119 기구수정으로 호밍 후 나노 z 이동 삭제
            //{
            //    if (i == 1) // Nano Z : 2.8 up
            //    {
            //        retVal = this.MotionManager().RelMove(axes1[i], pos - 12200, axes1[i].Param.Speed.Value * 3, axes1[i].Param.Acceleration.Value * 3);
            //    }
            //}

            // DD motor
            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.NZD1);

            pos = -5369.035; //251119 ybpark 기존 -5.369035 에서 -5369.035 수정  
            retVal = this.MotionManager().RelMove(axes1[2], pos, axes1[2].Param.Speed.Value, axes1[2].Param.Acceleration.Value);

            Thread.Sleep(3000);
            Arms_Air_Off();
        }

        //251208 ybpark add
        private AsyncCommand _A_HomingCommand;
        public ICommand A_HomingCommand
        {
            get
            {
                if (null == _A_HomingCommand) _A_HomingCommand = new AsyncCommand(AHoming_Func);
                return _A_HomingCommand;
            }
        }

        private async Task AHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.A);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.A);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _E_HomingCommand;
        public ICommand E_HomingCommand
        {
            get
            {
                if (null == _E_HomingCommand) _E_HomingCommand = new AsyncCommand(EHoming_Func);
                return _E_HomingCommand;
            }
        }

        private async Task EHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.E);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.E);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _W_HomingCommand;
        public ICommand W_HomingCommand
        {
            get
            {
                if (null == _W_HomingCommand) _W_HomingCommand = new AsyncCommand(WHoming_Func);
                return _W_HomingCommand;
            }
        }

        private async Task WHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.W);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.W);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _FV_HomingCommand;
        public ICommand FV_HomingCommand
        {
            get
            {
                if (null == _FV_HomingCommand) _FV_HomingCommand = new AsyncCommand(FVHoming_Func);
                return _FV_HomingCommand;
            }
        }

        private async Task FVHoming_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.FV);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.FV);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _U1_HomingCommand;
        public ICommand U1_HomingCommand
        {
            get
            {
                if (null == _U1_HomingCommand) _U1_HomingCommand = new AsyncCommand(U1Homing_Func);
                return _U1_HomingCommand;
            }
        }

        private async Task U1Homing_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.U1);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.U1);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _U2_HomingCommand;
        public ICommand U2_HomingCommand
        {
            get
            {
                if (null == _U2_HomingCommand) _U2_HomingCommand = new AsyncCommand(U2Homing_Func);
                return _U2_HomingCommand;
            }
        }

        private async Task U2Homing_Func()
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.NODATA;
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                axis = this.MotionManager().GetAxis(EnumAxisConstants.U2);

                ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.U2);
                ResultValidate(MethodBase.GetCurrentMethod(), ret);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _LoaderTotalModuleHomingCommand;
        public ICommand LoaderTotalModuleHomingCommand
        {
            get
            {
                if (null == _LoaderTotalModuleHomingCommand) _LoaderTotalModuleHomingCommand = new AsyncCommand(LoaderTotalModuleHoming_Func);
                return _LoaderTotalModuleHomingCommand;
            }
        }

        private async Task LoaderTotalModuleHoming_Func()
        {
            //CAMH_oming_Func();
            //NANOHoming_Func();
            //DDHoming_Func();
            //WaferHoming_Func();
            //FDHoming_Func();
            //XYHoming_Func();

            EventCodeEnum ret = EventCodeEnum.NODATA;

            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.CX1, EnumAxisConstants.CY1, EnumAxisConstants.CZ1,
                                                        EnumAxisConstants.CX2, EnumAxisConstants.CY2, EnumAxisConstants.CZ2,
                                                        EnumAxisConstants.CX3, EnumAxisConstants.CY3, EnumAxisConstants.CZ3,
                                                        EnumAxisConstants.CX4, EnumAxisConstants.CY4, EnumAxisConstants.CZ4,
                                                        EnumAxisConstants.CZ5, EnumAxisConstants.X, EnumAxisConstants.EJZ1, EnumAxisConstants.FDZ1, EnumAxisConstants.FDT1,
                                                        EnumAxisConstants.C, EnumAxisConstants.TRI, EnumAxisConstants.Z0, EnumAxisConstants.Z1,
                                                        EnumAxisConstants.Z2, EnumAxisConstants.EJPZ1);
            ResultValidate(MethodBase.GetCurrentMethod(), ret);

            // 호밍그룹1
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var axisEnums = new EnumAxisConstants[]
            {
                EnumAxisConstants.CX1, EnumAxisConstants.CY1, EnumAxisConstants.CZ1,
                EnumAxisConstants.CX2, EnumAxisConstants.CY2, EnumAxisConstants.CZ2,
                EnumAxisConstants.CX3, EnumAxisConstants.CY3, EnumAxisConstants.CZ3,
                EnumAxisConstants.CX4, EnumAxisConstants.CY4, EnumAxisConstants.CZ4,
                EnumAxisConstants.CZ5, EnumAxisConstants.X, EnumAxisConstants.EJZ1, EnumAxisConstants.FDZ1,EnumAxisConstants.FDT1
            };

            ProbeAxisObject[] axes = axisEnums
                .Select(axis => this.MotionManager().GetAxis(axis))
                .ToArray();

            double pos = 15000; //251119 ybpark 기존 15 에서 15000 이하 밑에 pos부분은 기존 값에서 1000곱한 값임!

            for (int i = 0; i < 17; i++)
            {
                if (i == 13) //X 
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos + 170000, axes[i].Param.Speed.Value * 2, axes[i].Param.Acceleration.Value * 2);
                }
                else if (i == 14) //EJZ1
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos - 5000, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
                else if (i == 15)//FDZ1
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos - 10000, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
                else if (i == 16)//FDT1
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos - 10000, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
                else
                {
                    retVal = this.MotionManager().RelMove(axes[i], pos, axes[i].Param.Speed.Value, axes[i].Param.Acceleration.Value);
                }
            }

            Arms_Air_On();

            // 호밍그룹2 : Base Y , Nano Z , Eject X, Eject Y
            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.Y, EnumAxisConstants.NSZ1, EnumAxisConstants.EJX1, EnumAxisConstants.EJY1);

            var axisEnums1 = new EnumAxisConstants[]
            {
                EnumAxisConstants.Y, EnumAxisConstants.NSZ1, EnumAxisConstants.NZD1
            };

            ProbeAxisObject[] axes1 = axisEnums1
                .Select(axis1 => this.MotionManager().GetAxis(axis1))
                .ToArray();

            //for (int i = 0; i < 2; i++) // 251119 기구수정으로 호밍 후 나노 z 이동 삭제
            //{
            //    if (i == 1) // Nano Z : 2.8 up
            //    {
            //        retVal = this.MotionManager().RelMove(axes1[i], pos - 12200, axes1[i].Param.Speed.Value * 3, axes1[i].Param.Acceleration.Value * 3);
            //    }
            //}

            // DD motor
            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.NZD1);

            pos = -5369.035; //251119 ybpark 기존 -5.369035 에서 -5369.035 수정  
            retVal = this.MotionManager().RelMove(axes1[2], pos, axes1[2].Param.Speed.Value, axes1[2].Param.Acceleration.Value);

            Thread.Sleep(3000);
            Arms_Air_Off();
        }

        // 251112 ybpark 단동 시험 버튼 추가 
        private AsyncCommand _AcceptanceCommand;
        public ICommand AcceptanceCommand
        {
            get
            {
                if (null == _AcceptanceCommand) _AcceptanceCommand = new AsyncCommand(AcceptanceCommand_Func);
                return _AcceptanceCommand;
            }
        }

        //// 251112 ybpark 단동 시험 버튼 추가 
        //private AsyncCommand _AcceptanceCommand;
        //public ICommand AcceptanceCommand
        //{
        //    get
        //    {
        //        if (null == _AcceptanceCommand) _AcceptanceCommand = new AsyncCommand(AcceptanceCommand_Domabam_Func);
        //        return _AcceptanceCommand;
        //    }
        //}

        #region => 251119 sebas sequence add
        private async Task AcceptanceCommand_Func()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"AcceptanceCommand Start");

                bool startFlag = true; // false : ARM1 , true : ARM2

                // =========================
                // Ready
                // =========================
                var ret = MovePickPos_SafeZone_First();
                if (ret != EventCodeEnum.NONE)
                    throw new Exception("MovePickPos_SafeZone_First() Error");

                for (TestCount = 1; TestCount <= TestCountActualVal; TestCount++)
                {
                    LoggerManager.Event($"Sequence Start {TestCount}");

                    // 병렬 구간에서 흔들리지 않게 사이클 시작값
                    bool cycleStartFlag = startFlag;

                    // =========================
                    // Pick & Place 병렬
                    // =========================
                    LoggerManager.Event($"Pick/Place Start {TestCount}");

                    Task pickTask = Task.Run(async () => await PickAsync(cycleStartFlag, TestCount));
                    Task placeTask = Task.Run(async () => await PlaceAsync(cycleStartFlag, TestCount));

                    await Task.WhenAll(pickTask, placeTask);

                    LoggerManager.Event($"Pick/Place All End {TestCount}");

                    // 마지막 다이 내려 놓고 정리
                    if (TestCount == 33)
                    {
                        LoggerManager.Event($"Sequence End {TestCount}");
                        break;
                    }

                    // =========================
                    // Rotate
                    // =========================

                    LoggerManager.Event($"Rotate Start {TestCount}");

                    // 마지막 다이는 안 움직임.
                    if (TestCount < 32)
                    {
                        LoggerManager.Event($"MovePickPos_SafeZone_Next Start");
                        retVal = MovePickPos_SafeZone_Next();
                        LoggerManager.Event($"MovePickPos_SafeZone_Next End");
                        if (retVal != EventCodeEnum.NONE)
                            throw new Exception("MovePickPos_SafeZone_Next() Function Error");
                    }

                    try
                    {
                        // 현재 Nano 위치 읽기
                        double currentPos = GetNZD1Pulse();

                        const double DtoP = 2.913;
                        const double MotorPulse = 524288.0;

                        // 5도에 해당하는 Pulse
                        double fiveDegreePulse = (MotorPulse / DtoP) / 180.0 * 50;

                        double threshold;

                        if (!cycleStartFlag)
                        {
                            // Minus 방향 : 현재 위치에서 -10도
                            threshold = currentPos - fiveDegreePulse;

                            NanostageUpDownMonitor(false, threshold);

                            LoggerManager.Event($"Rotate_Minus Start (Current={currentPos:F3}, Threshold={threshold:F3})");

                            ret = Rotate_Minus();

                            LoggerManager.Event("Rotate_Minus End");
                        }
                        else
                        {
                            // Plus 방향 : 현재 위치에서 +10도
                            threshold = currentPos + fiveDegreePulse;

                            NanostageUpDownMonitor(true, threshold);

                            LoggerManager.Event($"Rotate_Plus Start (Current={currentPos:F3}, Threshold={threshold:F3})");

                            ret = Rotate_Plus();

                            LoggerManager.Event("Rotate_Plus End");
                        }
                    }
                    finally
                    {
                        // Rotate 끝났으면 모니터 즉시 종료(중첩/누적 방지)
                        StopNanoMonitor();
                    }

                    if (ret != EventCodeEnum.NONE)
                        throw new Exception("Rotate Function Error");

                    // 다음 사이클용 StartFlag 토글 (병렬 구간 밖)
                    startFlag = !cycleStartFlag;

                    LoggerManager.Event($"Rotate End {TestCount}");
                    LoggerManager.Event($"Sequence End {TestCount}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            Arm1_Vac_Off_NoWating();
            Arm2_Vac_Off_NoWating();
        }

        private async Task RemyServoing_Func()
        {
            try
            {
                LoggerManager.Debug($"RemyServoing Start");

                bool startFlag = false; // false : ARM1 , true : ARM2

                // =========================
                // Ready
                // =========================
                var ret = MovePickPos_SafeZone_First();
                if (ret != EventCodeEnum.NONE)
                    throw new Exception("MovePickPos_SafeZone_First() Error");

                for (TestCount = 1; TestCount <= TestCountActualVal; TestCount++)
                {
                    LoggerManager.Event($"Sequence Start {TestCount}");

                    // 병렬 구간에서 흔들리지 않게 사이클 시작값
                    bool cycleStartFlag = startFlag;

                    // =========================
                    // Pick
                    // =========================
                    LoggerManager.Debug($"Pick Start {TestCount}");

                    LoggerManager.Debug($"MovePickPos_DangerZone Start");
                    ret = MovePickPos_DangerZone(cycleStartFlag);
                    LoggerManager.Debug($"MovePickPos_DangerZone End");
                    if (ret != EventCodeEnum.NONE)
                        throw new Exception("MovePickPos_DangerZone() Function Error");

                    // Rotate 해도 괜찮은 위치로 복귀
                    LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick Start");
                    ret = MovePickPos_SafeZone_AfterPick();
                    LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick End");
                    if (ret != EventCodeEnum.NONE)
                        throw new Exception("MovePickPos_SafeZone_AfterPick() Function Error");

                    LoggerManager.Debug($"Pick End {TestCount}");


                    // =========================
                    // Place
                    // =========================
                    LoggerManager.MonitoringLog($"Place Start {TestCount}");

                    LoggerManager.MonitoringLog($"나노스테이지 Z Down Start");
                    ret = DoPlace_Nano_ZDown();
                    LoggerManager.MonitoringLog($"나노스테이지 Z Down End");
                    if (ret != EventCodeEnum.NONE)
                        throw new Exception("DoPlace_Nano_ZDown() Function Error");

                    // Vacuum Off (Place)
                    if (cycleStartFlag == true)
                    {
                        LoggerManager.MonitoringLog($"Arm1_Vac_Off Start");
                        Arm1_Vac_Off_NoWating();
                        LoggerManager.MonitoringLog($"Arm1_Vac_Off End");
                    }
                    else
                    {
                        LoggerManager.MonitoringLog($"Arm2_Vac_Off Start");
                        Arm2_Vac_Off_NoWating();
                        LoggerManager.MonitoringLog($"Arm2_Vac_Off End");
                    }

                    // 이미지 그랩 추가 필요 (Die Grab)
                    //

                    LoggerManager.MonitoringLog($"나노스테이지 Z Up Start");
                    ret = DoPlace_Nano_ZUp();
                    LoggerManager.MonitoringLog($"나노스테이지 Z Up End");
                    if (ret != EventCodeEnum.NONE)
                        throw new Exception("DoPlace_Nano_ZUp() Function Error");

                    LoggerManager.MonitoringLog($"Place End {TestCount}");

                    LoggerManager.Event($"Rotate Start {TestCount}");

                    // =========================
                    // Rotate
                    // =========================
                    try
                    {
                        if (TestCount < 15)
                        {
                            // 현재 Nano 위치 읽기
                            double currentPos = GetNZD1Pulse();

                            // 1도에 해당하는 Pulse
                            const double DtoP = 2.913;
                            const double MotorPulse = 524288.0;
                            double oneDegreePulse = (MotorPulse / DtoP) / 180.0;   // ≒ 1000.084

                            double threshold;

                            if (!cycleStartFlag)
                            {
                                // Minus 방향 : 현재 위치에서 -1도
                                threshold = currentPos - oneDegreePulse;

                                NanostageUpDownMonitor(false, threshold);

                                LoggerManager.Event($"Rotate_Minus Start (Current={currentPos:F3}, Threshold={threshold:F3})");

                                ret = Rotate_Minus();

                                LoggerManager.Event("Rotate_Minus End");
                            }
                            else
                            {
                                // Plus 방향 : 현재 위치에서 +1도
                                threshold = currentPos + oneDegreePulse;

                                NanostageUpDownMonitor(true, threshold);

                                LoggerManager.Event($"Rotate_Plus Start (Current={currentPos:F3}, Threshold={threshold:F3})");

                                ret = Rotate_Plus();

                                LoggerManager.Event("Rotate_Plus End");
                            }
                        }
                        else
                        {
                            EventCodeEnum rv;

                            LoggerManager.Event($"나노스테이지 Z Down Start");
                            rv = DoPlace_Nano_ZDown();
                            LoggerManager.Event($"나노스테이지 Z Down End");
                            if (rv != EventCodeEnum.NONE)
                                throw new Exception("DoPlace_Nano_ZDown() Function Error");

                            // 이미지 초점을 위한
                            Thread.Sleep(20);

                            // 이미지 촬영
                            _VisionVM.RequestSaveNextFrameRaw(2);

                            // 저장 완료가 아니라 "프레임 복사 완료"만 최대 50ms 대기
                            bool pass = _VisionVM.WaitNextFrameCopiedAck(4, 50);
                            if (!pass)
                            {
                                LoggerManager.Debug("Next frame copy ACK timeout (<=50ms). Continue Z Up.");
                            }

                            LoggerManager.Event($"나노스테이지 Z Up Start");
                            rv = DoPlace_Nano_ZUp();
                            LoggerManager.Event($"나노스테이지 Z Up End");
                            if (rv != EventCodeEnum.NONE)
                                throw new Exception("DoPlace_Nano_ZUp() Function Error");
                        }
                    }
                    finally
                    {
                        // Rotate 끝났으면 모니터 즉시 종료(중첩/누적 방지)
                        StopNanoMonitor();
                    }

                    if (ret != EventCodeEnum.NONE)
                        throw new Exception("Rotate Function Error");

                    // 다음 사이클용 StartFlag 토글 (병렬 구간 밖)
                    startFlag = !cycleStartFlag;

                    LoggerManager.Event($"Rotate End {TestCount}");
                    LoggerManager.Event($"Sequence End {TestCount}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            Arm1_Vac_Off_NoWating();
            Arm2_Vac_Off_NoWating();
        }

        // =========================
        // Pick (Async) - Task.Run 제거, Thread.Sleep 제거
        // =========================
        private async Task PickAsync(bool cycleStartFlag, int testCount)
        {
            LoggerManager.Debug($"Pick Start {testCount}");

            LoggerManager.Debug($"MovePickPos_DangerZone Start");
            var ret = MovePickPos_DangerZone(cycleStartFlag);
            LoggerManager.Debug($"MovePickPos_DangerZone End");
            if (ret != EventCodeEnum.NONE)
                throw new Exception("MovePickPos_DangerZone() Function Error");

            // Rotate 해도 괜찮은 위치로 복귀
            LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick Start");
            ret = MovePickPos_SafeZone_AfterPick();
            LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick End");
            if (ret != EventCodeEnum.NONE)
                throw new Exception("MovePickPos_SafeZone_AfterPick() Function Error");

            LoggerManager.Debug($"Pick End {testCount}");
        }

        // =========================
        // Place (Async) - Task.Run 제거
        // =========================
        private async Task PlaceAsync(bool cycleStartFlag, int testCount)
        {
            LoggerManager.MonitoringLog($"Place Start {testCount}");

            LoggerManager.MonitoringLog($"나노스테이지 Z Down Start");
            var ret = DoPlace_Nano_ZDown();
            LoggerManager.MonitoringLog($"나노스테이지 Z Down End");
            if (ret != EventCodeEnum.NONE)
                throw new Exception("DoPlace_Nano_ZDown() Function Error");

            // Vacuum Off (Place)
            if (cycleStartFlag == true)
            {
                LoggerManager.MonitoringLog($"Arm1_Vac_Off Start");
                //Arm1_Vac_Off_NoWating();
                Arm1_Vac_Off();
                LoggerManager.MonitoringLog($"Arm1_Vac_Off End");
            }
            else
            {
                LoggerManager.MonitoringLog($"Arm2_Vac_Off Start");
                //Arm2_Vac_Off_NoWating();
                Arm2_Vac_Off();
                LoggerManager.MonitoringLog($"Arm2_Vac_Off End");
            }

            LoggerManager.MonitoringLog($"나노스테이지 Z Up Start");
            ret = DoPlace_Nano_ZUp();
            LoggerManager.MonitoringLog($"나노스테이지 Z Up End");
            if (ret != EventCodeEnum.NONE)
                throw new Exception("DoPlace_Nano_ZUp() Function Error");

            // 이미지 그랩
            // 컨텍스트 양보(과도한 점유 방지)
            //await Task.Yield();

            LoggerManager.MonitoringLog($"Place End {testCount}");
        }

        //private async Task AcceptanceCommand_Domabam_Func()
        //{
        //    try
        //    {
        //        LoggerManager.Debug($"AcceptanceCommand Start");
        //        DomabamFlag = true;
        //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //        ProbeAxisObject axisEJPZ1 = this.MotionManager().GetAxis(EnumAxisConstants.EJPZ1);

        //        bool StartFlag = false;     // false : ARM1 , true : ARM2
        //        double pos = 0.0;

        //        #region Ready
        //        retVal = MovePickPos_SafeZone_First();
        //        if (retVal != EventCodeEnum.NONE)
        //            throw new Exception("MovePickPos_SafeZone_First() Function Error");
        //        #endregion

        //        for (TestCount = 1; TestCount <= TestCountActualVal; TestCount++)
        //        {
        //            LoggerManager.Event($"Sequence Start {TestCount}");

        //            // 병렬 구간에서 흔들리지 않게 사이클 시작값 캡처
        //            bool cycleStartFlag = StartFlag;        // cycleStartFlag = false (ARM1) , cycleStartFlag = true (ARM2)

        //            // ============================
        //            // 1) Pick Task (동시에 시작)
        //            // ============================
        //            Task pickTask = Task.Run(() =>
        //            {
        //                double localPos;
        //                LoggerManager.Debug($"Pick Start {TestCount}");

        //                // Pick 위치 (StartFlag에 따라 다른 위치라면 캡처값 사용)
        //                LoggerManager.Debug($"MovePickPos_DangerZone Start");
        //                retVal = MovePickPos_DangerZone(cycleStartFlag);
        //                LoggerManager.Debug($"MovePickPos_DangerZone End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("MovePickPos_DangerZone() Function Error");

        //                // Rotate 해도 괜찮은 위치로 복귀
        //                LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick Start");
        //                retVal = MovePickPos_SafeZone_AfterPick();
        //                LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("MovePickPos_SafeZone_AfterPick() Function Error");

        //                Thread.Sleep(100);

        //                LoggerManager.Debug($"Pick End {TestCount}");
        //            });

        //            // ============================
        //            // 2) Place Task (동시에 시작)
        //            // ============================
        //            Task placeTask = Task.Run(() =>
        //            {
        //                LoggerManager.MonitoringLog($"Place Start {TestCount}");
        //                LoggerManager.MonitoringLog($"Magnetic_On Start");
        //                retVal = Magnetic_On();
        //                LoggerManager.MonitoringLog($"Magnetic_On End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("Magnetic_On() Function Error");

        //                LoggerManager.MonitoringLog($"나노스테이지 Z Down Start");
        //                retVal = DoPlace_Nano_ZDown();
        //                LoggerManager.MonitoringLog($"나노스테이지 Z Down End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("DoPlace_Nano_ZDown() Function Error");

        //                // Vacuum Off (Place) : 캡처한 StartFlag 기준으로 결정
        //                if (cycleStartFlag == true)
        //                {
        //                    LoggerManager.MonitoringLog($"Arm1_Vac_Off Start");
        //                    Arm1_Vac_Off();
        //                    LoggerManager.MonitoringLog($"Arm1_Vac_Off End");
        //                }
        //                else
        //                {
        //                    LoggerManager.MonitoringLog($"Arm2_Vac_Off Start");
        //                    Arm2_Vac_Off();
        //                    LoggerManager.MonitoringLog($"Arm2_Vac_Off End");
        //                }

        //                LoggerManager.MonitoringLog($"나노스테이지 Z Up Start");
        //                retVal = DoPlace_Nano_ZUp();
        //                LoggerManager.MonitoringLog($"나노스테이지 Z Up End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("DoPlace_Nano_ZUp() Function Error");

        //                LoggerManager.MonitoringLog($"Magnetic_Off Start");
        //                retVal = Magnetic_Off();
        //                LoggerManager.MonitoringLog($"Magnetic_Off End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("Magnetic_Off() Function Error");

        //                LoggerManager.MonitoringLog($"Place End {TestCount}");
        //            });

        //            // 3) Pick & Place 둘 다 완료될 때까지 대기
        //            await Task.WhenAll(pickTask, placeTask);

        //            // 마지막 다이 내려 놓고 정리
        //            if (16 == TestCount)
        //            {
        //                LoggerManager.Event($"Sequence End {TestCount}");
        //                break;
        //            }

        //            LoggerManager.Event($"Rotate Start {TestCount}");

        //            // Rotate 시간을 줄이기 위해 Place로 옮김.
        //            LoggerManager.Event($"Arms_Air_On Start");
        //            Arms_Air_On();
        //            LoggerManager.Event($"Arms_Air_On End");

        //            // 마지막 다이 움직일 필요 없음.
        //            if (TestCount < 15)
        //            {
        //                LoggerManager.Event($"MovePickPos_SafeZone_Next Start");
        //                retVal = MovePickPos_SafeZone_Next();
        //                LoggerManager.Event($"MovePickPos_SafeZone_Next End");
        //                if (retVal != EventCodeEnum.NONE)
        //                    throw new Exception("MovePickPos_SafeZone_Next() Function Error");
        //            }

        //            if (cycleStartFlag == false)
        //            {
        //                NanostageUpDownMonitor(cycleStartFlag, 79901.2);  // 2도 움직였을때 나노스테이지 업, 다운

        //                LoggerManager.Event($"Rotate_Minus Start");
        //                retVal = Rotate_Minus();
        //                LoggerManager.Event($"Rotate_Minus End");
        //            }
        //            else
        //            {
        //                NanostageUpDownMonitor(cycleStartFlag, -96081); // 2도 움직였을때 나노스테이지 업, 다운

        //                LoggerManager.Event($"Rotate_Plus Start");
        //                retVal = Rotate_Plus();
        //                LoggerManager.Event($"Rotate_Plus End");
        //            }

        //            if (retVal != EventCodeEnum.NONE)
        //                throw new Exception("Rotate Function Error");

        //            LoggerManager.Event($"Arms_Air_Off Start");
        //            Arms_Air_Off();
        //            LoggerManager.Event($"Arms_Air_Off End");

        //            // 5) 다음 사이클용 StartFlag 토글 (병렬 구간 밖에서)
        //            StartFlag = !cycleStartFlag;
        //            LoggerManager.Event($"Rotate End {TestCount}");
        //            LoggerManager.Event($"Sequence End {TestCount}");
        //        }

        //        Arms_Air_Off();
        //        Arm1_Vac_Off();
        //        Arm2_Vac_Off();
        //        DomabamFlag = false;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //private async Task AcceptanceCommand_Func()
        //{
        //    try
        //    {
        //        LoggerManager.Debug($"AcceptanceCommand Start");

        //        // 251112 sebas sequence add
        //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //        ProbeAxisObject axisEJPZ1 = this.MotionManager().GetAxis(EnumAxisConstants.EJPZ1);

        //        int RotateCount = 1;
        //        bool IOCheck = false;
        //        bool StartFlag = false;     // false : ARM1 , true : ARM2
        //        double pos = 0.0;

        //        #region Ready
        //        retVal = MovePickPos_SafeZone_First();  // 첫번째 시작 다이를 Pick 아래까지 이동 (FD척, Wafer척)

        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("MovePickPos_SafeZone() Function Error");
        //        }
        //        #endregion

        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("Rotate_Edit() Function Error");
        //        }

        //        for (TestCount = 1; TestCount <= TestCountActualVal; TestCount++)     // X축만 강제 3회 이동
        //        {
        //            LoggerManager.Debug($"Sequence Start {TestCount}");

        //            #region Pick Process
        //            //251125 ybpark 첫번째 다이가 아닌 두번째 다이 부터 x,y 
        //            if (TestCount > 1)
        //            {
        //                LoggerManager.Debug($"MovePickPos_SafeZone_Next(이젝션 다음다이로 이동) Start");
        //                retVal = MovePickPos_SafeZone_Next();
        //                LoggerManager.Debug($"MovePickPos_SafeZone_Next(이젝션 다음다이로 이동) End");
        //                if (retVal != EventCodeEnum.NONE)
        //                {
        //                    throw new Exception("MovePickPos_SafeZone_Next() Function Error");
        //                }
        //            }

        //            // Ejection Pin 나오는 동작 포함
        //            LoggerManager.Debug($"MovePickPos_DangerZone Start");
        //            retVal = MovePickPos_DangerZone(StartFlag);  // Pick할 수 있는 위치
        //            LoggerManager.Debug($"MovePickPos_DangerZone End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("MovePickPos_DangerZone() Function Error");
        //            }

        //            // Ejection Pin Down (상대값 이동)
        //            pos = -300;    // = 3,000 Ejection Pin Z 축 DtoP = 10
        //            LoggerManager.Debug($"Ejection Pin Down Start");
        //            retVal = this.MotionManager().RelMove_Wating(axisEJPZ1, pos, axisEJPZ1.Param.Speed.Value, axisEJPZ1.Param.Acceleration.Value);
        //            LoggerManager.Debug($"Ejection Pin Down End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Ejection Pin Z RelMove Error");
        //            }

        //            // Rotate 해도 괜찮은 위치로 복귀
        //            LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick Start");
        //            retVal = MovePickPos_SafeZone_AfterPick();  // 일단 Ejection Z Down만 Down
        //            LoggerManager.Debug($"MovePickPos_SafeZone_AfterPick End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("MovePickPos_SafeZone() Function Error");
        //            }
        //            #endregion

        //            #region Rotate Process
        //            //if (IOCheck)
        //            if (true)
        //            {
        //                LoggerManager.Debug($"Arms_Air_On Start");
        //                Arms_Air_On();  // arm1, arm2 air on
        //                LoggerManager.Debug($"Arms_Air_On End");

        //                if (false == StartFlag)
        //                {
        //                    LoggerManager.Debug($"Rotate_Minus(CW) Start");
        //                    retVal = Rotate_Minus(); // DD모터가 +방향(정면기준 시계 반대방향)으로 회전
        //                    LoggerManager.Debug($"Rotate_Minus(CW) End");
        //                    if (retVal != EventCodeEnum.NONE)
        //                    {
        //                        throw new Exception("Rotate_Plus() Function Error");
        //                    }
        //                }
        //                else
        //                {
        //                    LoggerManager.Debug($"Rotate_Plus(CW) Start");
        //                    retVal = Rotate_Plus(); // DD모터가 +방향(정면기준 시계 반대방향)으로 회전
        //                    LoggerManager.Debug($"Rotate_Plus(CW) End");
        //                    if (retVal != EventCodeEnum.NONE)
        //                    {
        //                        throw new Exception("Rotate_Minus() Function Error");
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region Place Process
        //            LoggerManager.Debug($"Magnetic_On Start");
        //            retVal = Magnetic_On();
        //            LoggerManager.Debug($"Magnetic_On End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Magnetic_On() Function Error");
        //            }

        //            // Arms air off
        //            LoggerManager.Debug($"Arms_Air_Off Start");
        //            Arms_Air_Off();
        //            LoggerManager.Debug($"Arms_Air_Off End");

        //            // Nano Stage Z Down
        //            LoggerManager.Debug($"DoPlace_Nano_ZDown Start");
        //            retVal = DoPlace_Nano_ZDown();
        //            LoggerManager.Debug($"DoPlace_Nano_ZDown End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("DoPlace_Nano_ZDown() Function Error");
        //            }

        //            // Vacuum Off 동작 (Place)
        //            if(false == StartFlag)
        //            {
        //                LoggerManager.Debug($"Arm2_Vac_Off Start");
        //                Arm1_Vac_Off();  // Place 동작 (Vacuum)
        //                LoggerManager.Debug($"Arm2_Vac_Off End");
        //                StartFlag = true;
        //            }
        //            else
        //            {
        //                LoggerManager.Debug($"Arm2_Vac_Off Start");
        //                Arm2_Vac_Off();  // Place 동작 (Vacuum)
        //                LoggerManager.Debug($"Arm2_Vac_Off End");
        //                StartFlag = false;
        //            }

        //            // Nano Stage Z Up
        //            LoggerManager.Debug($"DoPlace_Nano_ZUp Start");
        //            retVal = DoPlace_Nano_ZUp();
        //            LoggerManager.Debug($"DoPlace_Nano_ZUp End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("DoPlace_Nano_ZUp() Function Error");
        //            }

        //            // Place (Magnetic Off)
        //            LoggerManager.Debug($"Magnetic_Off Start");
        //            retVal = Magnetic_Off();
        //            LoggerManager.Debug($"Magnetic_Off End");
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Magnetic_Off() Function Error");
        //            }

        //            LoggerManager.Debug($"Sequence End {TestCount}");
        //            #endregion
        //        }

        //        //retVal = Wafer_Chuck_EndZone();
        //        //if (retVal != EventCodeEnum.NONE)
        //        //{
        //        //    throw new Exception("Wafer_Chuck_EndZone() Function Error");
        //        //}

        //        Arm1_Vac_Off();  // Place 동작 (Vacuum)
        //        Arm2_Vac_Off();  // Place 동작 (Vacuum)
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //private async Task AcceptanceCommand_Func()
        //{
        //    try
        //    {
        //        LoggerManager.Debug($"AcceptanceCommand Start");

        //        // 251112 sebas sequence add
        //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //        ProbeAxisObject axisEJPZ1 = this.MotionManager().GetAxis(EnumAxisConstants.EJPZ1);

        //        int RotateCount = 1;
        //        bool IOCheck = false;
        //        double pos = 0.0;

        //        #region Ready
        //        retVal = MovePickPos_SafeZone_First();  // 첫번째 시작 다이를 Pick 아래까지 이동 (FD척, Wafer척)

        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("MovePickPos_SafeZone() Function Error");
        //        }
        //        #endregion


        //        for (int i = 0; i < 4; i++)     // X축만 강제 3회 이동
        //        {
        //            #region Move Next
        //            LoggerManager.Debug($"Sequence Start {i}");

        //            if (RotateCount > 1)
        //            {
        //                retVal = MovePickPos_SafeZone_Next();

        //                if (retVal != EventCodeEnum.NONE)
        //                {
        //                    throw new Exception("MovePickPos_SafeZone_Next() Function Error");
        //                }
        //            }
        //            #endregion

        //            #region Pick Process
        //            Arms_Air_On();  // arm1, arm2 air on

        //            // Ejection Pin 나오는 동작 포함
        //            retVal = MovePickPos_DangerZone();  // Pick할 수 있는 위치
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("MovePickPos_DangerZone() Function Error");
        //            }

        //            if (RotateCount % 2 == 1)    // 홀수면 Arm1
        //            {
        //                // Arm1_Vac_On();  // Pick 동작 (Vacuum) , Arm1 사용 불가
        //                Arm2_Vac_On();  // Pick 동작 (Vacuum)
        //            }
        //            else   // 짝수면 Arm2
        //            {
        //                Arm2_Vac_On();  // Pick 동작 (Vacuum)
        //            }

        //            // Ejection Pin Down (상대값 이동)
        //            pos = 300;    // = 3,000 Ejection Pin Z 축 DtoP = 10
        //            retVal = this.MotionManager().RelMove_Wating(axisEJPZ1, pos, axisEJPZ1.Param.Speed.Value, axisEJPZ1.Param.Acceleration.Value);
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Ejection Pin Z RelMove Error");
        //            }

        //            // Ejection Pin Vacuum Off
        //            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);
        //            Thread.Sleep(250);

        //            // Rotate 해도 괜찮은 위치로 복귀
        //            retVal = MovePickPos_SafeZone_AfterPick();  // 일단 Ejection Z Down만 Down
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("MovePickPos_SafeZone() Function Error");
        //            }
        //            #endregion
        //            // 251119 드라이런으로 IO 체크 임시 제거
        //            // IOCheck = IsCanRotate();     // 조건 체크 : Nano Z , Air , Magnetic  +  FD Chuck Z 높이 추가 예정

        //            #region Rotate Process
        //            //if (IOCheck)
        //            if (true)
        //            {
        //                // 251119 드라이런으로 IO 체크 임시 제거
        //                //this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DI_ARM_VAC_SENSOR2, out IOCheck);      // (Interlock) Arm2 Vacuum On / Off 체크

        //                //if (IOCheck)
        //                if (true)
        //                {
        //                    // Rotate 동작
        //                    if (RotateCount % 2 == 1)    // 홀수면 +방향
        //                    {
        //                        retVal = Rotate_Plus(); // DD모터가 +방향(정면기준 시계 반대방향)으로 회전
        //                        if (retVal != EventCodeEnum.NONE)
        //                        {
        //                            throw new Exception("Rotate_Plus() Function Error");
        //                        }
        //                        RotateCount++;
        //                    }
        //                    else   // 짝수면 - 방향
        //                    {
        //                        retVal = Rotate_Minus(); // DD모터가 -방향(정면기준 시계 방향)으로 회전
        //                        if (retVal != EventCodeEnum.NONE)
        //                        {
        //                            throw new Exception("Rotate_Plus() Function Error");
        //                        }
        //                        RotateCount++;
        //                    }
        //                }
        //                else
        //                {
        //                    this.MetroDialogManager().ShowMessageDialog("Sequence Error", "Die is not existed on Arm2", EnumMessageStyle.Affirmative);
        //                    throw new Exception("Die is not existed on Arm2");
        //                }
        //            }
        //            #endregion


        //            #region Place Process
        //            retVal = Magnetic_On();
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Magnetic_On() Function Error");
        //            }

        //            // Arms air off
        //            Arms_Air_Off();

        //            // Wafer stage Up
        //            retVal = Wafer_Chuck_DangerZone();  // Wafer Chuck이 arm 바로 아래까지 Up
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Wafer_Chuck_DangerZone() Function Error");
        //            }

        //            // Nano Stage Z Down
        //            retVal = DoPlace_Nano_ZDown();
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("DoPlace_Nano_ZDown() Function Error");
        //            }

        //            // Arm1 Vacuum Off 동작 (Place)
        //            // 위에 Rotate에서 +1 했기 때문에 Pick 때와는 반대
        //            if (RotateCount % 2 == 1)    // 홀수면 Arm2
        //            {
        //                Arm1_Vac_Off();  // Place 동작 (Vacuum)
        //            }
        //            else   // 짝수면 Arm1
        //            {
        //                Arm2_Vac_Off();  // Place 동작 (Vacuum)
        //            }

        //            // Nano Stage Z Up
        //            retVal = DoPlace_Nano_ZUp();
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("DoPlace_Nano_ZUp() Function Error");
        //            }

        //            // Wafer stage Down
        //            retVal = Wafer_Chuck_SafeZone();
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Wafer_Chuck_SafeZone() Function Error");
        //            }

        //            // Place (Magnetic Off)
        //            retVal = Magnetic_Off();
        //            if (retVal != EventCodeEnum.NONE)
        //            {
        //                throw new Exception("Magnetic_Off() Function Error");
        //            }

        //            LoggerManager.Debug($"Sequence End {i}");
        //            #endregion
        //        }

        //        Arm1_Vac_Off();  // Place 동작 (Vacuum)
        //        Arm2_Vac_Off();  // Place 동작 (Vacuum)
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        // 20251121 Nick Pick Test
        public EventCodeEnum MovePickPos_SafeZone_First()
        {
            // Ejection Z , FD Z 축만 움직임. 첫 다이 위치로 가는 X , Y , Ejection X , Ejection Y 이동 값은 수동으로 이동한다.
            // X , Y를 움직여도 충돌이 없는 안전영역(대기영역)
            // pos 위치는 Pick을 하기위해 대기하는 위치로 고정좌표 값. 이 후 다음 다이로 이동하는 방법은 인덱스 사용

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisFDZ1 = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);
                ProbeAxisObject axisNSZ2 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ2);

                double pos = 0.0;           // 이동할 고정값을 넣는 변수 (덮어씌워짐)
                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;

                // 260705 sebas : pos 값 변경
                pos = 21600;    // = 91,000,000 FD stage Z 축 DtoP = 4194.304    // 기존 : 23000.0, 변경 : 21800에서 닿기도 하고 안 닿기도 함 (1mm = 1000)
                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.FDZ1).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                retVal = this.MotionManager().RelMove_Wating(axisFDZ1, pos - currentPos, axisFDZ1.Param.Speed.Value, axisFDZ1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("FD stage Z RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        //public EventCodeEnum MovePickPos_SafeZone_First()
        //{
        //    // Ejection X ,Y ,FD Z , base X , base Y , 3POD 모두 동시에 움직이고 맨 마지막으로 Ejection Z가 완료될 때까지 기다림
        //    // Z축 높이는 Picker 바로 아래가 아니며 X , Y를 움직여도 충돌이 없는 안전영역(대기영역)
        //    // pos 위치는 Pick을 하기위해 대기하는 위치로 고정좌표 값. 이 후 다음 다이로 이동하는 방법은 인덱스 사용

        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        ProbeAxisObject axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
        //        ProbeAxisObject axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
        //        ProbeAxisObject axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
        //        ProbeAxisObject axisFDZ1 = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);

        //        ProbeAxisObject axisEJX1 = this.MotionManager().GetAxis(EnumAxisConstants.EJX1);
        //        ProbeAxisObject axisEJY1 = this.MotionManager().GetAxis(EnumAxisConstants.EJY1);
        //        ProbeAxisObject axisFDT1 = this.MotionManager().GetAxis(EnumAxisConstants.FDT1);
        //        ProbeAxisObject axisEJZ1 = this.MotionManager().GetAxis(EnumAxisConstants.EJZ1);

        //        double pos = 0.0;   // 이동할 고정값을 넣는 변수 (덮어씌워짐)
        //        double currentPos = 0.0;    // 현재 위치값 읽기
        //        double AcualPos = 0;

        //        this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJX1).AxisType.Value, ref AcualPos);
        //        currentPos = AcualPos;
        //        pos = -58000;   // = -58,000 , Ejection X 축 DtoP = 1
        //        retVal = this.MotionManager().RelMove(axisEJX1, pos - currentPos, axisEJX1.Param.Speed.Value, axisEJX1.Param.Acceleration.Value);
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("EJX1 RelMove Error");
        //        }

        //        pos = 38250;    // = 153,000 Ejection Y 축 DtoP = 4
        //        this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJY1).AxisType.Value, ref AcualPos);
        //        currentPos = AcualPos;

        //        retVal = this.MotionManager().RelMove(axisEJY1, pos - currentPos, axisEJY1.Param.Speed.Value, axisEJY1.Param.Acceleration.Value);
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("EJY1 RelMove Error");
        //        }

        //        pos = 125781.25;    // = 12,880,000 Base X 축 DtoP = 102.400
        //        this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.X).AxisType.Value, ref AcualPos);
        //        currentPos = AcualPos;

        //        retVal = this.MotionManager().RelMove(axisX, pos - currentPos, axisX.Param.Speed.Value, axisX.Param.Acceleration.Value);
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("Base X RelMove Error");
        //        }

        //        pos = -366708.984;    // = -37,551,000 Base Y 축 DtoP = 102.400
        //        this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Y).AxisType.Value, ref AcualPos);
        //        currentPos = AcualPos;

        //        retVal = this.MotionManager().RelMove(axisY, pos - currentPos, axisY.Param.Speed.Value, axisY.Param.Acceleration.Value);
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("Base Y RelMove Error");
        //        }

        //        pos = 21696.091;    // = 91,000,000 FD stage Z 축 DtoP = 4194.304
        //        this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.FDZ1).AxisType.Value, ref AcualPos);
        //        currentPos = AcualPos;

        //        retVal = this.MotionManager().RelMove(axisFDZ1, pos - currentPos, axisFDZ1.Param.Speed.Value, axisFDZ1.Param.Acceleration.Value);
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("FD stage Z RelMove Error");
        //        }

        //        retVal = Wafer_Chuck_SafeZone();
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("Wafer_Chuck_SafeZone Error");
        //        }

        //        pos = 31000;    // = 155,000 Ejection Z 축 DtoP = 5
        //        this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJZ1).AxisType.Value, ref AcualPos);
        //        currentPos = AcualPos;
        //        // 움직임 완료까지 대기하는 동작
        //        retVal = this.MotionManager().RelMove_Wating(axisEJZ1, pos - currentPos, axisEJZ1.Param.Speed.Value, axisEJZ1.Param.Acceleration.Value);
        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            throw new Exception("Ejection Z RelMove Error");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return retVal;
        //}

        #region 251125 ybpark Map die Index 함수 추가
        //public static (double X, double Y) GetPos(int index)      // Map die 수정전
        //{
        //    if (index < 0 || index > 15)
        //        throw new ArgumentOutOfRangeException(nameof(index));

        //    double[] Xs = { 0, -16.5, -32.5, -49 };
        //    double[] Ys = { 0, 16.5, 32.5, 49 };


        //    int[,] map =
        //    {
        //        { 0, 1, 2, 3 },
        //        { 7, 6, 5, 4 },
        //        { 8, 9, 10, 11 },
        //        { 15, 14, 13, 12 }
        //    }; 

        //    int row = 0, col = 0;

        //    for(int r = 0; r < 4; r++)
        //        for(int c = 0; c < 4; c++)
        //            if(map[r, c] == index)
        //            {
        //                row = r;
        //                col = c;
        //            }

        //    double X = Xs[row];
        //    double Y = Ys[col];

        //    return (X, Y);
        //}

        //public static (double X, double Y) GetPos_EJ(int index)
        //{
        //    if (index < 0 || index > 15)
        //        throw new ArgumentOutOfRangeException(nameof(index));

        //    double[] Xs = { 0, 16.5, 32.5, 49 };
        //    double[] Ys = { 0, -16.5, -32.5, -49 };


        //    int[,] map =
        //    {
        //        { 0, 1, 2, 3 },
        //        { 7, 6, 5, 4 },
        //        { 8, 9, 10, 11 },
        //        { 15, 14, 13, 12 }
        //    };

        //    int row = 0, col = 0;

        //    for (int r = 0; r < 4; r++)
        //        for (int c = 0; c < 4; c++)
        //            if (map[r, c] == index)
        //            {
        //                row = r;
        //                col = c;
        //            }

        //    double EJ_X = Xs[row];
        //    double EJ_Y = Ys[col];

        //    return (EJ_X, EJ_Y);
        //}

        #endregion

        #region 251223 ybpark Map die Index 함수 수정
        //public static (double X, double Y) GetPos(int index)
        //{
        //    const int cols = 3;   // X 방향 개수
        //    const int rows = 5;   // Y 방향 개수

        //    if (index < 0 || index >= cols * rows)
        //        throw new ArgumentOutOfRangeException(nameof(index));

        //    // 열 계산 (세로 기준)
        //    int col = index / rows;

        //    // 행 계산 (지그재그)
        //    int rowInCol = index % rows;
        //    int row;

        //    if (col % 2 == 0)
        //        row = rowInCol;                 // 정방향
        //    else
        //        row = rows - 1 - rowInCol;      // 역방향

        //    double X = -16.5 * col;
        //    double Y = 16.5 * row;

        //    return (X, Y);
        //}

        //public static (double X, double Y) GetPos_EJ(int index)
        //{
        //    const int cols = 3;
        //    const int rows = 5;

        //    if (index < 0 || index >= cols * rows)
        //        throw new ArgumentOutOfRangeException(nameof(index));

        //    int col = index / rows;

        //    int rowInCol = index % rows;
        //    int row;

        //    if (col % 2 == 0)
        //        row = rowInCol;
        //    else
        //        row = rows - 1 - rowInCol;

        //    //  부호 반전
        //    double EJ_X = 16.5 * col;   // 기존 -16.5 → +16.5
        //    double EJ_Y = -16.5 * row;   // 기존 +16.5 → -16.5

        //    return (EJ_X, EJ_Y);
        //}

        // ybpark 260209 Map die index
        //public static (double X, double Y) GetPos(int index)
        //{
        //    const int cols = 5;          // 가로 방향(Y) 개수
        //    const int rows = 3;          // 세로 방향(X) 개수
        //    const double pitch = 12.0;   // Die(10) + Gap(2) = 12mm

        //    if (index < 0 || index >= cols * rows)
        //        throw new ArgumentOutOfRangeException(nameof(index));

        //    // 현재 몇 번째 행(세로), 몇 번째 열(가로)인지 계산
        //    int row = index / cols;      // 0, 1, 2 (X축 결정)
        //    int colInRow = index % cols; // 0, 1, 2, 3, 4 (Y축 결정)

        //    double x = -pitch * row;     // 아래로 갈수록 좌표값이 작아짐 (0, -12, -24)
        //    double y;

        //    // 행(row)이 짝수일 때는 정방향(0->4), 홀수일 때는 역방향(4->0)
        //    if (row % 2 == 0)
        //    {
        //        y = pitch * colInRow;    // 정방향 (0, 12, 24, 36, 48)
        //    }
        //    else
        //    {
        //        y = pitch * (cols - 1 - colInRow); // 역방향 (48, 36, 24, 12, 0)
        //    }

        //    return (x, y);
        //}


        // 260707 sebas Map die 수정
        public static (double X, double Y) GetPos(int index)
        {
            const int cols = 6;          // 가로 방향(X) 개수
            const int rows = 6;          // 세로 방향(Y) 개수
            const double pitch = 11.5;   // Die Pitch

            if (index < 0 || index >= cols * rows)
                throw new ArgumentOutOfRangeException(nameof(index));

            // 현재 몇 번째 행(Y), 몇 번째 열(X)인지 계산
            int row = index / cols;      // 0,1,2,3,4,5 (Y축 결정)
            int colInRow = index % cols; // 0,1,2,3,4,5 (X축 결정)

            // 아래에서 위로 증가
            double y = pitch * row;

            double x;

            // 짝수 행 : 오른쪽 -> 왼쪽
            if (row % 2 == 0)
            {
                x = -pitch * colInRow;
            }
            // 홀수 행 : 왼쪽 -> 오른쪽
            else
            {
                x = -pitch * (cols - 1 - colInRow);
            }

            return (x, y);
        }

        public static (double X, double Y) GetPos_EJ(int index)
        {
            const int cols = 5;          // 가로 방향 개수
            const int rows = 3;          // 세로 방향 개수
            const double pitch = 12.0;   // Die(10) + Gap(2) = 12mm

            if (index < 0 || index >= cols * rows)
                throw new ArgumentOutOfRangeException(nameof(index));

            // 행(row)과 열(col) 계산
            int row = index / cols;
            int colInRow = index % cols;

            // 부호 반전 적용
            // 기존 x = -pitch * row  ==> 수정 x = pitch * row (아래로 갈수록 +)
            double x = pitch * row;

            double y;
            // 기존 y 로직에 전체적으로 -1을 곱함 (오른쪽으로 갈수록 -)
            if (row % 2 == 0)
            {
                // 정방향: 0, -12, -24, -36, -48
                y = -pitch * colInRow;
            }
            else
            {
                // 역방향: -48, -36, -24, -12, 0E
                y = -pitch * (cols - 1 - colInRow);
            }

            return (x, y);
        }
        #endregion

        public EventCodeEnum MovePickPos_SafeZone_Next()
        {
            // 일단 다이 크기만큼 +X 방향으로 움직임(상대이동)
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //var IndexPos = GetPos((int)TestCountActualVal);
            double X = GetPos((int)TestCount).X - (GetPos((int)TestCount - 1).X);
            double Y = GetPos((int)TestCount).Y - (GetPos((int)TestCount - 1).Y);

            try
            {
                ProbeAxisObject axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                // 속도 업
                axisX.Param.Speed.Value = 40000;
                axisX.Param.Acceleration.Value = 600000;
                axisX.Param.AccelerationJerk.Value = 12000000;

                axisY.Param.Speed.Value = 40000;
                axisY.Param.Acceleration.Value = 600000;
                axisY.Param.AccelerationJerk.Value = 12000000;
                //

                double posX = 0.0;   // 이동할 고정값을 넣는 변수
                double posY = 0.0;   // 이동할 고정값을 넣는 변수

                // 20260707 Nick 빠르게 변경
                //axisX.Param.Speed = ;
                //axisX.Param.Acceleration = ;
                //axisX.Param.AccelerationJerk = ;

                //axisY.Param.Speed = ;
                //axisY.Param.Acceleration = ;
                //axisY.Param.AccelerationJerk = ;
                //

                //251125 ybpark X,Y die index 위치로 이동
                posX = -(X * 1000);    //Y 는 MapDie 좌표계기준이라서 X축 move   // 260707 sebas 기존 Y -> X 로 
                posY = -(Y * 1000);    //X 는 MapDie 좌표계기준이라서 Y축 move   // 260707 sebas 기존 X -> Y 로

                if (posX != 0)
                {
                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) Start");
                    retVal = this.MotionManager().RelMove(axisY, posY, axisY.Param.Speed.Value, axisY.Param.Acceleration.Value);
                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) End");

                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) Start");
                    retVal = this.MotionManager().RelMove(axisX, posX, axisX.Param.Speed.Value, axisX.Param.Acceleration.Value);
                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) End");
                }
                else
                {
                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) Start");
                    retVal = this.MotionManager().RelMove(axisX, posX, axisX.Param.Speed.Value, axisX.Param.Acceleration.Value);
                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) End");

                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) Start");
                    retVal = this.MotionManager().RelMove(axisY, posY, axisY.Param.Speed.Value, axisY.Param.Acceleration.Value);
                    LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) End");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        // 251224 sebas : dryrun용
        public EventCodeEnum MovePickPos_SafeZone_NextReverse()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            double X = 0;
            double Y = 0;
            double EJ_X = 0;
            double EJ_Y = 0;

            if (ReverseRun == false) // 정방향
            {
                X = GetPos((int)TestCount).X - (GetPos((int)TestCount - 1).X);
                Y = GetPos((int)TestCount).Y - (GetPos((int)TestCount - 1).Y);

                EJ_X = GetPos_EJ((int)TestCount).X - (GetPos_EJ((int)TestCount - 1).X);
                EJ_Y = GetPos_EJ((int)TestCount).Y - (GetPos_EJ((int)TestCount - 1).Y);
            }
            else  // 역방향 : 출력값에 - 씌움
            {
                X = -(GetPos((int)TestCount).X - (GetPos((int)TestCount - 1).X));
                Y = -(GetPos((int)TestCount).Y - (GetPos((int)TestCount - 1).Y));

                EJ_X = -(GetPos_EJ((int)TestCount).X - (GetPos_EJ((int)TestCount - 1).X));
                EJ_Y = -(GetPos_EJ((int)TestCount).Y - (GetPos_EJ((int)TestCount - 1).Y));
            }

            try
            {
                ProbeAxisObject axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                ProbeAxisObject axisEJX1 = this.MotionManager().GetAxis(EnumAxisConstants.EJX1);
                ProbeAxisObject axisEJY1 = this.MotionManager().GetAxis(EnumAxisConstants.EJY1);

                double pos = 0.0;   // 이동할 고정값을 넣는 변수 (덮어씌워짐)

                //pos = EJ_Y * 1000;    //Y 는 MapDie 좌표계기준이라서 X축 move
                //LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base EJ X Move) Start");
                //retVal = this.MotionManager().RelMove(axisEJX1, pos, axisEJX1.Param.Speed.Value, axisEJX1.Param.Acceleration.Value);
                //LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base EJ X Move) End");

                //if (retVal != EventCodeEnum.NONE)
                //{
                //    throw new Exception("Base EJX RelMove Error");
                //}

                ////251125 ybpark X,Y die index 위치로 이동
                //pos = EJ_X * 1000;    //X 는 MapDie 좌표계기준이라서 Y축 move
                //LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base EJ Y Move) Start");
                //retVal = this.MotionManager().RelMove(axisEJY1, pos, axisEJY1.Param.Speed.Value, axisEJY1.Param.Acceleration.Value);
                //LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base EJ Y Move) End");
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    throw new Exception("Base EJY RelMove Error");
                //}

                pos = Y * 1000;    //Y 는 MapDie 좌표계기준이라서 X축 move
                LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base X Move) Start");
                retVal = this.MotionManager().RelMove(axisX, pos, axisX.Param.Speed.Value, axisX.Param.Acceleration.Value);
                LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base X Move) End");

                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Base X RelMove Error");
                }

                //251125 ybpark X,Y die index 위치로 이동
                pos = X * 1000;    //X 는 MapDie 좌표계기준이라서 Y축 move
                LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base Y Move) Start");
                retVal = this.MotionManager().RelMove_Wating(axisY, pos, axisY.Param.Speed.Value, axisY.Param.Acceleration.Value);
                LoggerManager.Debug($"MovePickPos_SafeZone_Next(Base Y Move) End");

                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Base Y RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum MovePickPos_SafeZone_AfterPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNSZ2 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ2);

                // ArmPick Z Up 추가
                double posArmPick = 6500.0;    // = 2,000,000 Nano Stage Z 축 DtoP = 4194.304
                LoggerManager.Debug($"Arm Pick Z Up Start");
                retVal = this.MotionManager().RelMove_Wating(axisNSZ2, posArmPick, axisNSZ2.Param.Speed.Value, axisNSZ2.Param.Acceleration.Value);
                LoggerManager.Debug($"Arm Pick Z Up End");
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Arm Pick Z RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum MovePickPos_DangerZone(bool armFlag)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNSZ2 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ2);

                double pos = 0.0;   // 이동할 고정값을 넣는 변수 (덮어씌워짐)
                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;
                bool IOCheck = false;

                // Arm Pick Down
                double posArmPick = -6500.0;     // = Arm Pick Down Z 축 DtoP = 4194.304
                LoggerManager.Debug($"MovePickPos_DangerZone Arm Pick Z Down Start");
                retVal = this.MotionManager().RelMove_Wating(axisNSZ2, posArmPick, axisNSZ2.Param.Speed.Value, axisNSZ2.Param.Acceleration.Value);
                LoggerManager.Debug($"MovePickPos_DangerZone Arm Pick Z Down End");
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Arm Pick Z RelMove Error");
                }

                if (false == armFlag)
                {
                    LoggerManager.Debug($"Arm1_Vac_On Start");
                    //Arm1_Vac_On_NoWating();  // Pick 동작 (Vacuum)
                    Arm1_Vac_On();  // Pick 동작 (Vacuum)
                    LoggerManager.Debug($"Arm1_Vac_On End");
                }
                else
                {
                    LoggerManager.Debug($"Arm2_Vac_On Start");
                    //Arm2_Vac_On_NoWating();  // Pick 동작 (Vacuum)
                    Arm2_Vac_On();  // Pick 동작 (Vacuum)
                    LoggerManager.Debug($"Arm2_Vac_On End");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Arms_Air_On()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm1 , Arm2 Air On
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, true);
                Thread.Sleep(5);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, false);
                Thread.Sleep(5);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, true);
                Thread.Sleep(5);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum Arms_Air_Off()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm1 , Arm2 Air Off
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, true);
                Thread.Sleep(5);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, false);
                Thread.Sleep(5);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, true);
                Thread.Sleep(5);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public void Arms_Air_On_NoWaiting()
        {
            _ecatIo.Post(() =>
            {
                var io = this.IOManager().IOServ;

                io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, true);

                _ecatIo.PostAfter(() =>
                {
                    io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, false);

                    _ecatIo.PostAfter(() =>
                    {
                        io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, true);

                        _ecatIo.PostAfter(() =>
                        {
                            io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, false);
                        }, 5);

                    }, 5);

                }, 5);
            });
        }
        public void Arms_Air_Off_NoWating()
        {
            _ecatIo.Post(() =>
            {
                var io = this.IOManager().IOServ;

                // Arm1 Air OFF ON
                io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, true);

                // 5ms 후 Arm2 Air OFF ON
                _ecatIo.PostAfter(() =>
                {
                    io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, true);

                    // 5ms 후 Arm1 Air OFF OFF
                    _ecatIo.PostAfter(() =>
                    {
                        io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, false);

                        // 5ms 후 Arm2 Air OFF OFF
                        _ecatIo.PostAfter(() =>
                        {
                            io.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, false);
                        }, 5);

                    }, 5);

                }, 5);
            });
        }

        public EventCodeEnum Arm1_Vac_On()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm1 Vacuum On
                if(DryRepeat == false)  // 251223 sebas : Dryrun repeat 일 때는 arm vac on/off 안하기 때문
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, true);
                    Thread.Sleep(70);
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum Arm2_Vac_On()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm1 Vacuum On
                if (DryRepeat == false)  // 251223 sebas : Dryrun repeat 일 때는 arm vac on/off 안하기 때문
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, true);
                    Thread.Sleep(70);
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Arm1_Arm2_Vac_Off(bool ArmFlag)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IORet ioret = IORet.UNKNOWN;

                if (false == ArmFlag)
                {
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, false);
                }
                else
                {
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Arm1_Vac_Off()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm1 Vacuum Off
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, true);
                //Thread.Sleep(1);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, false);
                //Thread.Sleep(1);

                if (DryRepeat == false)  // 251223 sebas : Dryrun repeat 일 때는 arm vac on/off 안하기 때문
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, true);
                    Thread.Sleep(70);
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Arm2_Vac_Off()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm2 Vacuum Off
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, true);
                //Thread.Sleep(1);
                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, false);
                //Thread.Sleep(1);

                if (DryRepeat == false)  // 251223 sebas : Dryrun repeat 일 때는 arm vac on/off 안하기 때문
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, true);
                    Thread.Sleep(70);
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public void Arm1_Vac_Off_NoWating()
        {
            //if (DryRepeat)
            //    return;

            // ON 즉시 실행
            _ecatIo.Post(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, true);
            });

            // OFF는 50ms 뒤 예약 (Sleep 제거)
            _ecatIo.PostAfter(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, false);
            }, 50);
        }

        public void Arm2_Vac_Off_NoWating()
        {
            //if (DryRepeat)
            //    return;

            // ON 즉시 실행
            _ecatIo.Post(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, true);
            });

            // OFF는 50ms 뒤 예약 (Sleep 제거)
            _ecatIo.PostAfter(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, false);
            }, 50);
        }

        public void Arm1_Vac_On_NoWating()
        {
            //if (DryRepeat)
            //    return;

            // ON 즉시
            _ecatIo.Post(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, true);
            });

            // OFF 50ms 후 (Sleep 제거)
            _ecatIo.PostAfter(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, false);
            }, 50);
        }

        public void Arm2_Vac_On_NoWating()
        {
            //if (DryRepeat)
            //    return;

            // ON 즉시
            _ecatIo.Post(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, true);
            });

            // OFF 50ms 후 (Sleep 제거)
            _ecatIo.PostAfter(() =>
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, false);
            }, 50);
        }

        public bool IsCanRotate()
        {
            // 조건 체크 : Nano Z
            bool ret = false;
            double AcualPos = 0.0;
            double NanoStagePosCheck = -3000.0;         // 나노스테이지 Z축 안전 위치

            try
            {
                // (Interlock) Nano Stage Z Position Check
                for(long i = 1; i < 4; i++)
                {
                    this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.NSZ1).AxisType.Value, ref AcualPos);

                    if (AcualPos < NanoStagePosCheck)
                    {
                        LoggerManager.Event($"현재 나노스테이지 Z축 간섭 위치, 현재위치 : {AcualPos}, {i} / 3");
                        if (i > 3)
                        {
                            LoggerManager.Event($"회전 할 수 없는 상태 (나노스테이지 확인 필요)");
                            ret = false;
                            return ret;
                        }
                    }
                    else
                    {
                        ret = true;
                        return ret;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum Rotate_Edit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNZD1 = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);

                double pos = 0.0;   //이동할 고정값을 넣는 변수 (덮어씌워짐)

                pos = 16748 / 2.913;    // = 524,288 DD motor 회전 DtoP = 2.913
                LoggerManager.Debug($"DD 회전 Start");
                retVal = this.MotionManager().RelMove_Wating(axisNZD1, pos, axisNZD1.Param.Speed.Value, axisNZD1.Param.Acceleration.Value);
                LoggerManager.Debug($"DD 회전 End");

                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("DD motor RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Rotate_Plus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNZD1 = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);

                double pos = 0.0;   //이동할 고정값을 넣는 변수 (덮어씌워짐)

                pos = 524288 / 2.913;    // = 524,288 DD motor 회전 DtoP = 2.913
                LoggerManager.Debug($"DD 회전 Start");
                retVal = this.MotionManager().RelMove_Wating(axisNZD1, pos, axisNZD1.Param.Speed.Value, axisNZD1.Param.Acceleration.Value);
                LoggerManager.Debug($"DD 회전 End");

                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("DD motor RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum Rotate_Minus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNZD1 = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);

                double pos = 0.0;   //이동할 고정값을 넣는 변수 (덮어씌워짐)

                pos = -524288 / 2.913;    // = 524,288 DD motor 회전 DtoP = 2.913 / 524288
                retVal = this.MotionManager().RelMove_Wating(axisNZD1, pos, axisNZD1.Param.Speed.Value, axisNZD1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("DD motor RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Magnetic_On()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Magnetic On
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal = EventCodeEnum.NONE;
        }

        public EventCodeEnum Magnetic_Off()
        {
            // Vacuum 끄고 Place 끝난 후 단계로 다음 순서인 Pick 직전

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Magnetic Off
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal = EventCodeEnum.NONE;
        }

        public void Magnetic_On_NoWating()
        {
            _ecatIo.Post(() =>
            {
                // Magnetic On
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, true);
            });
        }

        public void Magnetic_Off_NoWating()
        {
            // Vacuum 끄고 Place 끝난 후 단계로 다음 순서인 Pick 직전
            _ecatIo.Post(() =>
            {
                // Magnetic Off
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, false);
            });
        }

        public EventCodeEnum Wafer_Chuck_DangerZone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;

                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                double pos = 130000000;   // = 350,000 Z 축 DtoP = 0.0025    (상판부터 척까지 높이 20.8) // 기존 144,000,000 -> 도마뱀테스트 
                LoggerManager.Debug($"Wafer_Chuck_Up Start");
                retVal = this.MotionManager().RelMove_Wating(axisZ, pos - currentPos, axisZ.Param.Speed.Value, axisZ.Param.Acceleration.Value);
                LoggerManager.Debug($"Wafer_Chuck_Up End");
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Stage Z Up RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum Wafer_Chuck_SafeZone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;

                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                double pos = 128000000;   // = 320,000 Z 축 DtoP = 0.0025
                LoggerManager.Debug($"Wafer_Chuck_Down Start");
                retVal = this.MotionManager().RelMove_Wating(axisZ, pos - currentPos, axisZ.Param.Speed.Value, axisZ.Param.Acceleration.Value);
                LoggerManager.Debug($"Wafer_Chuck_Down End");

                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Stage Z Up RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum Wafer_Chuck_EndZone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;

                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                double pos = 100000000;   // = 320,000 Z 축 DtoP = 0.0025
                retVal = this.MotionManager().RelMove(axisZ, pos - currentPos, axisZ.Param.Speed.Value, axisZ.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Stage Z Up RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum DoPlace_Nano_ZDown()
        {
            // 나노스테이지 다운 ( 고정값이 아닌 상대값 이동 )

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNSZ1 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ1);

                double pos = -3500;   // = -2,000,000 Nano Stage Z 축 DtoP = 4194.304 (기존) -2000.0 -> (변경) -3995, 초점을 맞추기 위함 (Wafer)
                retVal = this.MotionManager().RelMove_Wating(axisNSZ1, pos, axisNSZ1.Param.Speed.Value, axisNSZ1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Nano Stage Z Down RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum DoPlace_Nano_ZUp()
        {
            // 나노스테이지 업 ( 고정값이 아닌 상대값 이동 )

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisNSZ1 = this.MotionManager().GetAxis(EnumAxisConstants.NSZ1);

                double pos = 3500;   // = 2,000,000 Nano Stage Z 축 DtoP = 4194.304 (기존) 2000.0 -> (변경) 3995 (Wafer) 초점을 맞추기 위함
                retVal = this.MotionManager().RelMove_Wating(axisNSZ1, pos, axisNSZ1.Param.Speed.Value, axisNSZ1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Nano Stage Z Up RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        #endregion

        #region 251124 ybpark Normal, FD Wafer Out 
        private AsyncCommand _DryRunRepeatCommand;
        public ICommand DryRunRepeatCommand
        {
            get
            {
                if (null == _DryRunRepeatCommand) _DryRunRepeatCommand = new AsyncCommand(DryRunRepeatCommand_Func);
                return _DryRunRepeatCommand;
            }
        }

        public bool DryRepeat = false;  // 251223 sebas add : false일 때는 vac 실행, true일 때는 vac 안함
        private async Task DryRunRepeatCommand_Func()
        {
            try
            {
                LoggerManager.Debug($"AcceptanceCommand Start");
                
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                bool StartFlag = false;     // false : ARM1 , true : ARM2
                double pos = 0.0;

                #region Ready
                retVal = MovePickPos_SafeZone_First();
                if (retVal != EventCodeEnum.NONE)
                    throw new Exception("MovePickPos_SafeZone_First() Function Error");
                #endregion

                int repeatNum = 1;  // 251224 sebas 홀수 = 정방향 , 짝수 = 역방향
                int maxNum = 1000;   // 무한반복이지만, 일단 최대치 설정해 놓음

                DryRepeat = true;
                while (repeatNum < maxNum)   // 251224 sebas : while 추가
                {
                    if (repeatNum % 2 == 0)
                    {
                        ReverseRun = true;
                    }
                    else
                    {
                        ReverseRun = false;
                    }
                    repeatNum++;

                    for (TestCount = 1; TestCount <= 15; TestCount++)
                    {
                        LoggerManager.Debug($"Sequence Start {TestCount}");

                        // 병렬 구간에서 흔들리지 않게 사이클 시작값
                        bool cycleStartFlag = StartFlag;        // cycleStartFlag = false (ARM1) , cycleStartFlag = true (ARM2)

                        // =========================
                        // Pick & Place 병렬 (최소화)
                        // =========================
                        Task pickTask = PickAsync(cycleStartFlag, TestCount);
                        Task placeTask = PlaceAsync(cycleStartFlag, TestCount);

                        await Task.WhenAll(pickTask, placeTask);

                        // =========================
                        // Rotate 준비
                        // =========================
                        LoggerManager.Debug($"Rotate Start {TestCount}");

                        LoggerManager.Event($"Arms_Air_On Start");
                        Arms_Air_On_NoWaiting();
                        LoggerManager.Event($"Arms_Air_On End");

                        // 나노스테이지 위치 인터락
                        //bool RotateIntorlock = IsCanRotate();
                        //if(false == RotateIntorlock)
                        //{
                        //    LoggerManager.Event($"회전 할 수 없는 상태(나노스테이지 확인 필요)");
                        //    return;
                        //}

                        // 마지막 다이 움직일 필요 없음.
                        if (TestCount < 15)
                        {
                            LoggerManager.Debug($"MovePickPos_SafeZone_Next Start");
                            retVal = MovePickPos_SafeZone_NextReverse();
                            LoggerManager.Debug($"MovePickPos_SafeZone_Next End");
                            if (retVal != EventCodeEnum.NONE)
                                throw new Exception("MovePickPos_SafeZone_Next() Function Error");
                        }

                        try
                        {
                            if (cycleStartFlag == false)
                            {
                                //NanostageUpDownMonitor(cycleStartFlag, 80901);  // 1도 (81901에서 -98081로 가는 방향 기준)

                                LoggerManager.Debug($"Rotate_Minus Start");
                                retVal = Rotate_Minus();
                                LoggerManager.Debug($"Rotate_Minus End");
                            }
                            else
                            {
                                NanostageUpDownMonitor(cycleStartFlag, -97081); // 1도 (-98081에서 81901로 가는 방향 기준)

                                LoggerManager.Debug($"Rotate_Plus Start");
                                retVal = Rotate_Plus();
                                LoggerManager.Debug($"Rotate_Plus End");
                            }
                        }
                        finally
                        {
                            // Rotate 끝났으면 모니터 즉시 종료(중첩/누적 방지)
                            //StopNanoMonitor();
                        }

                        if (retVal != EventCodeEnum.NONE)
                            throw new Exception("Rotate Function Error");

                        LoggerManager.Event($"Arms_Air_Off Start");
                        Arms_Air_Off_NoWating();
                        LoggerManager.Event($"Arms_Air_Off End");

                        // 다음 사이클용 StartFlag 토글 (병렬 구간 밖에서)
                        StartFlag = !cycleStartFlag;

                        if (dryRunStop == true)
                            break;  // dryrun fisnish

                        LoggerManager.Debug($"Rotate End {TestCount}");
                        LoggerManager.Debug($"Sequence End {TestCount}");
                    }
                }

                DryRepeat = false;
                dryRunStop = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _RotateAndCaptureCommand;
        public ICommand RotateAndCaptureCommand
        {
            get
            {
                if (null == _RotateAndCaptureCommand) _RotateAndCaptureCommand = new AsyncCommand(RotateAndCapture_Func);
                return _RotateAndCaptureCommand;
            }
        }

        private double GetNZD1Pulse()
        {
            double pulse = 0.0;
            var axis = this.MotionManager().GetAxis(EnumAxisConstants.NZD1);
            this.MotionManager().GetActualPos(axis.AxisType.Value, ref pulse);
            return pulse;
        }

        private CancellationTokenSource _capCts;
        private Task _capTask;
        /// <summary>
        /// rotateFlag = false; 일때 정방향 회전 (임계값 아래->위 통과 시 캡처)
        /// rotateFlag = true;  일때 역방향 회전 (임계값 위->아래 통과 시 캡처)
        /// </summary>
        private void StartCaptureMonitor(bool rotateFlag, double threshold)
        {
            // 이전 감시 종료
            //StopCaptureMonitor();

            _capCts = new CancellationTokenSource();
            var ct = _capCts.Token;

            _capTask = Task.Run(async () =>
            {
                try
                {
                    double prev = GetNZD1Pulse();

                    while (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(4, ct); // 폴링 주기(필요시 조절)
                        double curr = GetNZD1Pulse();

                        // "통과" 순간에 NanoStage Down/Up
                        if(false == rotateFlag)
                        {
                            if (curr > threshold)
                            {
                                LoggerManager.Debug("(P) Camera Capture Start");
                                _VisionVM.CaptureCamera(0);
                                LoggerManager.Debug("(P) Camera Capture End");
                                break;
                            }
                        }
                        else
                        {
                            if (curr < threshold)
                            {
                                LoggerManager.Debug("(R) Camera Capture Start");
                                _VisionVM.CaptureCamera(0);
                                LoggerManager.Debug("(R) Camera Capture End");
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 정상 취소
                }
                catch (Exception ex)
                {
                    LoggerManager.Exception(ex);
                }
            }, ct);
        }

        private CancellationTokenSource _nanoCts;
        private Task _nanoMonitorTask;
        private int _nanoTriggered;

        private readonly ConcurrentQueue<long> _nanoEventQueue = new ConcurrentQueue<long>();
        private int _nanoWorkerRunning;

        // 20260106 Nick Test
        // =========================
        // Nano Monitor (GetNZD1Pulse 1회/loop 버전)
        // =========================
        private void NanostageUpDownMonitor(bool rotateFlag, double threshold, int pollMs = 2)
        {
            StopNanoMonitor();

            _nanoTriggered = 0;
            _nanoCts = new CancellationTokenSource();
            var ct = _nanoCts.Token;

            _nanoMonitorTask = Task.Run(() =>
            {
                LoggerManager.Event("Nano Monitor Thread Start");

                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;

                    while (!ct.IsCancellationRequested)
                    {
                        double curr = GetNZD1Pulse();

                        bool trigger = !rotateFlag
                            ? curr <= threshold
                            : curr >= threshold;

                        if (trigger)
                        {
                            if (Interlocked.Exchange(ref _nanoTriggered, 1) == 0)
                            {
                                LoggerManager.Event($"Nano Detected curr={curr:F3}, threshold={threshold:F3}");
                                FireNanoThresholdEventFast(true);
                            }
                            break;
                        }

                        Thread.Sleep(pollMs);
                    }
                }
                catch (Exception ex)
                {
                    LoggerManager.Exception(ex);
                }
            }, ct);
        }

        private void StopNanoMonitor()
        {
            try
            {
                if (_nanoCts != null)
                {
                    _nanoCts.Cancel();
                    _nanoCts.Dispose();
                    _nanoCts = null;
                }
            }
            catch { }
        }

        private void FireNanoThresholdEventFast(bool flag) // True : Wafer, False : Die
        {
            // 감지 시각 기록 (정밀)
            _nanoEventQueue.Enqueue(Stopwatch.GetTimestamp());

            // Worker 중복 실행 방지
            if (Interlocked.Exchange(ref _nanoWorkerRunning, 1) == 0)
            {
                Task.Run(() =>
                {
                    try
                    {
                        while (_nanoEventQueue.TryDequeue(out var ts))
                        {
                            LoggerManager.Event("NanoStage Threshold Reached");
                            OnNanoThresholdCrossed(flag); // 실제 처리 (여기서 오래 걸려도 감지엔 영향 최소)
                        }
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _nanoWorkerRunning, 0);
                    }
                });
            }
        }

        private void OnNanoThresholdCrossed(bool flag) // True : Wafer, False : Die
        {
            try
            {
                EventCodeEnum rv;

                if (true == flag)
                {
                    LoggerManager.Event("나노스테이지 Z Down Start");
                    rv = DoPlace_Nano_ZDown();
                    LoggerManager.Event("나노스테이지 Z Down End");

                    if (rv != EventCodeEnum.NONE)
                        throw new Exception("DoPlace_Nano_ZDown() Function Error");

                    // 캡처 저장 요청
                    //if (_VisionVM != null)
                    //{
                    //    _VisionVM.CaptureCameraAsyncSave(2);
                    //}

                    LoggerManager.Event("나노스테이지 Z Up Start");
                    rv = DoPlace_Nano_ZUp();
                    LoggerManager.Event("나노스테이지 Z Up End");

                    if (rv != EventCodeEnum.NONE)
                        throw new Exception("DoPlace_Nano_ZUp() Function Error");
                }
                else
                {
                    //for(int i = 0; i < 20; i++)
                    //{
                    //    Thread.Sleep(100);

                    //    // 캡처 저장 요청
                    //    if (_VisionVM != null)
                    //    {
                    //        _VisionVM.CaptureCameraAsyncSave(2);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
            }
        }

        public bool dryRunStop = false;    // 251224 sebas
        private async Task RotateAndCapture_Func()
        {
            dryRunStop = true;  // 251224 sebas

            //EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //try
            //{
            //    // NanoStage 인터락
            //    bool NanoInterlock = IsCanRotate();
            //    if (false == NanoInterlock)
            //        return;

            //    // Air On
            //    Arms_Air_On();  // arm1, arm2 air on

            //    // 회전
            //    if (false == RotateFlag)
            //    {
            //        // 회전 시작 전에 감시 시작
            //        StartCaptureMonitor(RotateFlag, threshold: 10000.0);

            //        LoggerManager.Debug($"Rotate_Plus(CW) Start");
            //        retVal = Rotate_Plus(); // DD모터가 +방향(정면기준 시계 반대방향)으로 회전
            //        LoggerManager.Debug($"Rotate_Plus(CW) End");

            //        // 회전 끝나면 감시 종료
            //        StopCaptureMonitor();

            //        if (retVal != EventCodeEnum.NONE)
            //        {
            //            throw new Exception("Rotate_Plus() Function Error");
            //        }

            //        RotateFlag = true;
            //    }
            //    else
            //    {
            //        // 회전 시작 전에 감시 시작
            //        StartCaptureMonitor(RotateFlag, threshold: 70000.0);

            //        LoggerManager.Debug($"Rotate_Minus(CW) Start");
            //        retVal = Rotate_Minus(); // DD모터가 -방향으로 회전(주석은 실제와 맞게 수정 권장)
            //        LoggerManager.Debug($"Rotate_Minus(CW) End");

            //        // 회전 끝나면 감시 종료
            //        StopCaptureMonitor();

            //        if (retVal != EventCodeEnum.NONE)
            //            throw new Exception("Rotate_Minus() Function Error");

            //        RotateFlag = false;
            //    }

            //    // Air Off
            //    Arms_Air_Off();

            //    // Magnet On
            //    LoggerManager.Debug($"Magnetic_On Start");
            //    retVal = Magnetic_On();
            //    LoggerManager.Debug($"Magnetic_On End");
            //    if (retVal != EventCodeEnum.NONE)
            //    {
            //        throw new Exception("Magnetic_On() Function Error");
            //    }

            //    // Nano Stage Z Down
            //    LoggerManager.Debug($"DoPlace_Nano_ZDown Start");
            //    retVal = DoPlace_Nano_ZDown();
            //    LoggerManager.Debug($"DoPlace_Nano_ZDown End");
            //    if (retVal != EventCodeEnum.NONE)
            //    {
            //        throw new Exception("DoPlace_Nano_ZDown() Function Error");
            //    }

            //    // Nano Stage Z Up
            //    LoggerManager.Debug($"DoPlace_Nano_ZUp Start");
            //    retVal = DoPlace_Nano_ZUp();
            //    LoggerManager.Debug($"DoPlace_Nano_ZUp End");
            //    if (retVal != EventCodeEnum.NONE)
            //    {
            //        throw new Exception("DoPlace_Nano_ZUp() Function Error");
            //    }

            //    // Magnet Off
            //    LoggerManager.Debug($"Magnetic_Off Start");
            //    retVal = Magnetic_Off();
            //    LoggerManager.Debug($"Magnetic_Off End");
            //    if (retVal != EventCodeEnum.NONE)
            //    {
            //        throw new Exception("Magnetic_Off() Function Error");
            //    }
            //}
            //catch (Exception err)
            //{
            //    // 혹시 예외로 빠져도 감시 Task 정리
            //    StopCaptureMonitor();

            //    LoggerManager.Exception(err);
            //    throw;
            //}
        }
        #endregion

        #region 251124 ybpark Test Count
        public double _TestCountActualVal = 0.0;
        public double TestCountActualVal
        {
            get
            {
                return _TestCountActualVal;
            }
            set
            {
                if (_TestCountActualVal != value)
                {
                    _TestCountActualVal = value;
                    RaisePropertyChanged("TestCountActualVal");
                }
            }
        }
        #endregion

        #region 260706 remy Manualjog에서 nanostage 창열기
        private ICommand _NanoStageControlCommand;
        public ICommand NanoStageControlCommand
        {
            get
            {
                if (_NanoStageControlCommand == null)
                {
                    _NanoStageControlCommand = new RelayCommand(NanoStageControlCommand_Func);
                }
                return _NanoStageControlCommand;
            }
        }

        private void NanoStageControlCommand_Func()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 1. 팝업 윈도우 인스턴스 생성
                ManualJogView.NanoStageControlWindow nanoWindow = new ManualJogView.NanoStageControlWindow();

                // 2. 마스터 커맨드 정의 (부모가 들고 있는 인스턴스를 직접 조준)
                var masterDisconnectCommand = new NanoStage6AxisViewModel.localRelayCommand(p =>
                {
                    _nanoStage3AxisVM.DisconnectCommand.Execute(null);
                    _nanoStage6AxisVM.DisconnectCommand.Execute(null);
                });

                var masterTotalMoveCommand = new NanoStage6AxisViewModel.localRelayCommand(p =>
                {
                    _nanoStage3AxisVM.TotalMoveCommand.Execute(null);
                    _nanoStage6AxisVM.TotalMoveCommand.Execute(null);
                });

                var masterClearLogCommand = new NanoStage6AxisViewModel.localRelayCommand(p =>
                {
                    _nanoStage3AxisVM.ErrorClearCommand.Execute(null);
                    _nanoStage6AxisVM.ErrorClearCommand.Execute(null);
                });

                // 3. 💡 [핵심] 리플렉션 싹 다 걷어내고 부모의 인스턴스를 패키징 주입
                nanoWindow.DataContext = new
                {
                    Axis3VM = _nanoStage3AxisVM,
                    Axis6VM = _nanoStage6AxisVM,
                    MainDisconnectCommand = masterDisconnectCommand,
                    MainTotalMoveCommand = masterTotalMoveCommand,
                    MainClearLogCommand = masterClearLogCommand
                };

                if (System.Windows.Application.Current.MainWindow != null)
                {
                    nanoWindow.Owner = System.Windows.Application.Current.MainWindow;
                }

                // 모달리스로 오픈 (양쪽 다 클릭 가능)
                nanoWindow.Show();
            });
        }

        private AsyncCommand _EjectionCenterCommand;
        public ICommand EjectionCenterCommand
        {
            get
            {
                if (null == _EjectionCenterCommand) _EjectionCenterCommand = new AsyncCommand(EjectionCenterCommand_Func);
                return _EjectionCenterCommand;
            }
        }
        private async Task EjectionCenterCommand_Func()
        {
            // 20251124 Nick 이젝션 센터 찾기

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Arm Test
                // Vacuum On
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON1, true);
                Thread.Sleep(500);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON2, true);
                Thread.Sleep(500);

                // DD Motor Test (Target 0.25 이내)
                LoggerManager.Event($"Rotate Start {TestCount}");

                // 마지막 다이는 안 움직임.
                LoggerManager.Event($"MovePickPos_SafeZone_Next Start");
                retVal = MovePickPos_SafeZone_Next();
                LoggerManager.Event($"MovePickPos_SafeZone_Next End");
                if (retVal != EventCodeEnum.NONE)
                    throw new Exception("MovePickPos_SafeZone_Next() Function Error");

                try
                {
                    // 현재 Nano 위치 읽기
                    double currentPos = GetNZD1Pulse();

                    const double DtoP = 2.913;
                    const double MotorPulse = 524288.0;

                    // ~도에 해당하는 Pulse
                    double DegreePulse = (MotorPulse / DtoP) / 180.0 * 20.0;

                    double threshold;

                    // Minus 방향 : 현재 위치에서 -10도
                    threshold = currentPos - DegreePulse;
                    NanostageUpDownMonitor(false, threshold);
                    LoggerManager.Event($"Rotate_Minus Start (Current={currentPos:F3}, Threshold={threshold:F3})");

                    retVal = Rotate_Minus();

                    LoggerManager.Event("Rotate_Minus End");

                    Thread.Sleep(5000);

                    // Plus 방향 : 현재 위치에서 +10도
                    currentPos = GetNZD1Pulse();
                    threshold = currentPos + DegreePulse;
                    NanostageUpDownMonitor(true, threshold);

                    LoggerManager.Event($"Rotate_Plus Start (Current={currentPos:F3}, Threshold={threshold:F3})");

                    retVal = Rotate_Plus();

                    LoggerManager.Event("Rotate_Plus End");
                }
                finally
                {
                    // Rotate 끝났으면 모니터 즉시 종료(중첩/누적 방지)
                    StopNanoMonitor();
                }

                // Stage Test (Taget 0.15 이내)
                //ProbeAxisObject axisX1 = this.MotionManager().GetAxis(EnumAxisConstants.X);
                //ProbeAxisObject axisY1 = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                //double posX1 = 0.0;   // 이동할 고정값을 넣는 변수
                //double posY1 = 0.0;   // 이동할 고정값을 넣는 변수
                //double posX2 = 0.0;   // 이동할 고정값을 넣는 변수
                //double posY2 = 0.0;   // 이동할 고정값을 넣는 변수

                ////251125 ybpark X,Y die index 위치로 이동
                //posX1 = 11500;
                //posY1 = 11500;
                //posX2 = -11500;
                //posY2 = -11500;

                //Thread.Sleep(1000);

                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) Start");
                //retVal = this.MotionManager().RelMove_Wating(axisX1, posX1, axisX1.Param.Speed.Value, axisX1.Param.Acceleration.Value);
                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) End");

                //Thread.Sleep(1000);

                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) Start");
                //retVal = this.MotionManager().RelMove_Wating(axisX1, posX2, axisX1.Param.Speed.Value, axisX1.Param.Acceleration.Value);
                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base X Move) End");

                //Thread.Sleep(1000);

                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) Start");
                //retVal = this.MotionManager().RelMove_Wating(axisY1, posY1, axisY1.Param.Speed.Value, axisY1.Param.Acceleration.Value);
                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) End");

                //Thread.Sleep(1000);

                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) Start");
                //retVal = this.MotionManager().RelMove_Wating(axisY1, posY2, axisY1.Param.Speed.Value, axisY1.Param.Acceleration.Value);
                //LoggerManager.Event($"MovePickPos_SafeZone_Next(Base Y Move) End");

                // Arm Test
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON1, false);
                Thread.Sleep(500);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACON2, false);
                Thread.Sleep(500);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF1, true);
                Thread.Sleep(500);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF2, true);
                Thread.Sleep(500);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF1, false);
                Thread.Sleep(500);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_HOLD_VACOFF2, false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _FirstDieCommand;
        public ICommand FirstDieCommand
        {
            get
            {
                if (null == _FirstDieCommand) _FirstDieCommand = new AsyncCommand(FirstDieCommand_Func);
                return _FirstDieCommand;
            }
        }
        private async Task FirstDieCommand_Func()
        {
            // 260705 sebas 버튼 수정

            // 20251124 Nick 첫 다이 찾기
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //ProbeAxisObject axisEJZ1 = this.MotionManager().GetAxis(EnumAxisConstants.EJZ1);
                ProbeAxisObject axisX1 = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject axisY1 = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject axisFDZ1 = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);

                double pos = 0.0;   // 이동할 고정값을 넣는 변수 (덮어씌워짐)
                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;

                //// Ejection Z Down (간섭을 피하기 위해 내림)
                //pos = 4770;   // 티칭 값
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJZ1).AxisType.Value, ref AcualPos);
                //currentPos = AcualPos;

                //retVal = this.MotionManager().RelMove_Wating(axisEJZ1, pos - currentPos, axisEJZ1.Param.Speed.Value, axisEJZ1.Param.Acceleration.Value);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    throw new Exception("Ejection Z RelMove Error");
                //}

                // Base X
                pos = 94340;    // 티칭 값
                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.X).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                retVal = this.MotionManager().RelMove_Wating(axisX1, pos - currentPos, axisX1.Param.Speed.Value, axisX1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Base X RelMove Error");
                }

                // Base Y
                pos = -188630;  // 티칭 값
                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Y).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                retVal = this.MotionManager().RelMove_Wating(axisY1, pos - currentPos, axisY1.Param.Speed.Value, axisY1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Base Y RelMove Error");
                }

                // FD Z Up (첫 다이 찾기 위해)
                pos = 22620;    // 티칭 값
                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.FDZ1).AxisType.Value, ref AcualPos);
                currentPos = AcualPos;

                retVal = this.MotionManager().RelMove_Wating(axisFDZ1, pos - currentPos, axisFDZ1.Param.Speed.Value, axisFDZ1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("FD Z RelMove Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _EjectionPositionCommand;
        public ICommand EjectionPositionCommand
        {
            get
            {
                if (null == _EjectionPositionCommand) _EjectionPositionCommand = new AsyncCommand(EjectionPositionCommand_Func);
                return _EjectionPositionCommand;
            }
        }
        private async Task EjectionPositionCommand_Func()
        {
            //// 20251124 Nick 이젝션 위치 보정
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // 260707 sebas test button
                for (TestCount = 1; TestCount <= 35; TestCount++)
                {
                    MovePickPos_SafeZone_Next();
                }

                // <-- 260707 이전 내용 주석처리
                //ProbeAxisObject axisFDZ1 = this.MotionManager().GetAxis(EnumAxisConstants.FDZ1);
                //ProbeAxisObject axisEJX1 = this.MotionManager().GetAxis(EnumAxisConstants.EJX1);
                //ProbeAxisObject axisEJY1 = this.MotionManager().GetAxis(EnumAxisConstants.EJY1);

                //double pos = 0.0;   // 이동할 고정값을 넣는 변수 (덮어씌워짐)
                //double currentPos = 0.0;    // 현재 위치값 읽기
                //double AcualPos = 0;

                //// FD Z Down (간섭을 피하기 위해 내림)
                //pos = 4770;   // 티칭 값
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.FDZ1).AxisType.Value, ref AcualPos);
                //currentPos = AcualPos;

                //retVal = this.MotionManager().RelMove_Wating(axisFDZ1, pos - currentPos, axisFDZ1.Param.Speed.Value, axisFDZ1.Param.Acceleration.Value);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    throw new Exception("FD Z RelMove Error");
                //}

                //// EJ X
                //pos = -39642;    // 티칭 값
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJX1).AxisType.Value, ref AcualPos);
                //currentPos = AcualPos;

                //retVal = this.MotionManager().RelMove_Wating(axisEJX1, pos - currentPos, axisEJX1.Param.Speed.Value, axisEJX1.Param.Acceleration.Value);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    throw new Exception("EJ X RelMove Error");
                //}

                //// EJ Y
                //pos = 24299;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.EJY1).AxisType.Value, ref AcualPos);
                //currentPos = AcualPos;

                //retVal = this.MotionManager().RelMove_Wating(axisEJY1, pos - currentPos, axisEJY1.Param.Speed.Value, axisEJY1.Param.Acceleration.Value);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    throw new Exception("EJ Y  RelMove Error");
                //}
                // --> 260707
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _ArmPickerCommand;
        public ICommand ArmPickerCommand
        {
            get
            {
                if (null == _ArmPickerCommand) _ArmPickerCommand = new AsyncCommand(ArmPickerCommand_Func);
                return _ArmPickerCommand;
            }
        }
        private async Task ArmPickerCommand_Func()
        {
            // 20251124 Nick Arm Picker 위치로 이동

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbeAxisObject axisX1 = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject axisY1 = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                double pos = 0.0;   // 이동할 고정값을 넣는 변수 (덮어씌워짐)
                double currentPos = 0.0;    // 현재 위치값 읽기
                double AcualPos = 0;

                // Base X 
                pos = 19750;   // 티칭 값 기존 : 19500 -> 변경 : 19750
                retVal = this.MotionManager().RelMove_Wating(axisX1, pos, axisX1.Param.Speed.Value, axisX1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Base X RelMove Error");
                }

                // Base Y
                pos = 198500;    // 티칭 값 기존 : 196500 -> 변경 : 198500
                retVal = this.MotionManager().RelMove_Wating(axisY1, pos, axisY1.Param.Speed.Value, axisY1.Param.Acceleration.Value);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception("Base Y RelMove Error");
                }

                //251125 ybpark Arm Picker 위치 받아서 index 위치로 이동(Map Die)
                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.X).AxisType.Value, ref FirstDiePos_X);

                this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Y).AxisType.Value, ref FirstDiePos_Y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion
        /////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////
        ///Nano Stage Move 함수//////////////////////////////////
        /////////////////////////////////////////////////////////
        ///
        protected int _id = -1;
        // 단위 변환 상수 (Degree <-> Micro-radian)
        protected const double DegToUrad = (Math.PI / 180.0) * 1000000.0;
        protected const double UradToDeg = 1.0 / DegToUrad;

        /// <summary>
        /// 💡 [3축 시퀀스 이동 함수]
        /// </summary>
        public async Task<double> Move3AxisAsync(double targetZ, double targetRx, double targetRy)
        {
            // 💡 [변경] 3축 뷰모델에 박혀있는 진짜 하드웨어 ID를 리플렉션 없이 안전한 프로퍼티나 필드로 우회 조회합니다.
            // NanoStageViewModel 내부의 private _id 값을 안전하게 읽어옵니다.
            int currentId = (int)typeof(NanoStageViewModel).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_nanoStage3AxisVM);

            if (currentId < 0)
            {
                LoggerManager.Error("[3-Axis Sequence] Error: PI Controller not connected. Please connect from popup window first.");
                return -1;
            }

            double fZ = Math.Round(targetZ, 3);
            double fRx = Math.Round(targetRx, 5);
            double fRy = Math.Round(targetRy, 5);

            double hwZ = fZ;
            double hwRx = fRx * DegToUrad;
            double hwRy = fRy * DegToUrad;

            // 리미트 체크 시에도 현재 발급된 ID를 주입
            if (!CheckAxisLimit(currentId, "1", hwZ) || !CheckAxisLimit(currentId, "2", hwRx) || !CheckAxisLimit(currentId, "3", hwRy))
            {
                return -1;
            }

            try
            {
                GCS2.SVO(currentId, "1 2 3", new int[] { 1, 1, 1 });
                double[] targets = { hwZ, hwRx, hwRy };

                if (GCS2.MOV(currentId, "1 2 3", targets) == 1)
                {
                    LoggerManager.Debug($"[Seq-3Axis] MOV Started -> Z:{fZ:F3}, Rx:{fRx:F5}, Ry:{fRy:F5}");
                    return await WaitStageStopAsync(currentId, "1 2 3", 3);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
            }
            return -1;
        }

        /// <summary>
        /// 💡 [6축 시퀀스 이동 함수]
        /// </summary>
        public async Task<double> Move6AxisAsync(double targetX, double targetY, double targetZ, double targetRx, double targetRy, double targetRz)
        {
            int currentId = (int)typeof(NanoStage6AxisViewModel).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_nanoStage6AxisVM);

            if (currentId < 0)
            {
                LoggerManager.Error("[6-Axis Sequence] Error: PI Controller not connected.");
                return -1;
            }

            double fX = Math.Round(targetX, 3);
            double fY = Math.Round(targetY, 3);
            double fZ = Math.Round(targetZ, 3);
            double fRx = Math.Round(targetRx, 5);
            double fRy = Math.Round(targetRy, 5);
            double fRz = Math.Round(targetRz, 5);

            double[] hwTargets = { fX, fY, fRz * DegToUrad, fZ, fRx * DegToUrad, fRy * DegToUrad };

            if (!CheckAxisLimit(currentId, "1", hwTargets[0]) || !CheckAxisLimit(currentId, "2", hwTargets[1]) ||
                !CheckAxisLimit(currentId, "3", hwTargets[2]) || !CheckAxisLimit(currentId, "4", hwTargets[3]) ||
                !CheckAxisLimit(currentId, "5", hwTargets[4]) || !CheckAxisLimit(currentId, "6", hwTargets[5]))
            {
                return -1;
            }

            try
            {
                GCS2.SVO(currentId, "1 2 3 4 5 6", new int[] { 1, 1, 1, 1, 1, 1 });

                if (GCS2.MOV(currentId, "1 2 3 4 5 6", hwTargets) == 1)
                {
                    LoggerManager.Debug($"[Seq-6Axis] MOV Started -> X:{fX:F3}, Y:{fY:F3}, Z:{fZ:F3}");
                    return await WaitStageStopAsync(currentId, "1 2 3 4 5 6", 6);
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
            }
            return -1;
        }

        private async Task<double> WaitStageStopAsync(int currentId, string axisString, int axisCount)
        {
            int[] isMoving = new int[axisCount];
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < 5000)
            {
                if (GCS2.IsMoving(currentId, axisString, isMoving) == 0) break;

                bool stillMoving = false;
                for (int i = 0; i < axisCount; i++)
                {
                    if (isMoving[i] == 1) { stillMoving = true; break; }
                }
                if (!stillMoving) break;

                await Task.Delay(5);
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private bool CheckAxisLimit(int currentId, string axis, double target)
        {
            double[] minLimit = new double[1];
            double[] maxLimit = new double[1];
            if (GCS2.qTMN(currentId, axis, minLimit) == 0 || GCS2.qTMX(currentId, axis, maxLimit) == 0) return false;

            double margin = (axis == "1" || axis == "2" || axis == "4") ? 0.0 : 0.0;
            if (target < minLimit[0] + margin || target > maxLimit[0] - margin)
            {
                LoggerManager.Error($"[PI SoftLimit Error] Axis {axis} Range Out! Target:{target:F4}");
                return false;
            }
            return true;
        }

        //////////////////////////TCP통신 //////////////////
        public async Task StartServerAsync()
        {
            if (_server != null) return; // 이미 서버가 실행 중이면 무시

            try
            {
                // 1. 서버 시작
                _server = new TcpListener(IPAddress.Any, 5000);
                _server.Start();
                //AddCommLog(">>> [Server] 서버 시작됨. 5000번 포트 대기 중...");

                // 2. 클라이언트 접속 루프 (백그라운드에서 계속 실행)
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            // 비전 장비가 접속할 때까지 여기서 대기
                            TcpClient client = await _server.AcceptTcpClientAsync();

                            // 접속 성공 시 해당 클라이언트를 _connectedClient에 할당
                            _connectedClient = client;
                            //AddCommLog(">>> [Server] 비전 장비 접속 완료.");

                            // 여기서부터는 ListenForDataAsync(client)를 호출하여 
                            // 비전 장비가 보내는 데이터를 감시할 수 있습니다.
                            // (이미 구현하신 루프가 있다면 그것을 여기서 호출하세요)
                        }
                        catch (Exception ex)
                        {
                            //AddCommLog($"!!! [Server] 접속 에러: {ex.Message}");
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                //AddCommLog($"!!! [Server] 서버 실행 실패: {ex.Message}");
            }
        }

        protected async Task<string> SendAndGetResultAsync(string cmd, int timeoutMs = 10000)
        {
            // 1. 연결 확인
            if (_connectedClient == null || !_connectedClient.Connected)
            {
                //AddCommLog("!!! [Seq] 비전 장비가 연결되어 있지 않습니다.");
                return null;
            }

            try
            {
                NetworkStream stream = _connectedClient.GetStream();

                // 2. 명령 전송
                byte[] data = Encoding.ASCII.GetBytes(cmd + "\n");
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
                //AddCommLog($">>> [Seq] Sent: {cmd}");

                // 3. 응답 대기
                var reader = new StreamReader(stream, Encoding.ASCII);
                var readTask = reader.ReadLineAsync();
                var delayTask = Task.Delay(timeoutMs);

                if (await Task.WhenAny(readTask, delayTask) == readTask)
                {
                    string response = await readTask;
                    //AddCommLog($"<<< [Seq] Received: {response}");
                    return response; // 성공 시 전체 문자열 반환
                }
                else
                {
                    //AddCommLog($"!!! [Seq] 응답 타임아웃 ({cmd})");
                    return null;
                }
            }
            catch (Exception ex)
            {
                //AddCommLog($"!!! [Seq] 통신 에러: {ex.Message}");
                return null;
            }
        }
    }
}

