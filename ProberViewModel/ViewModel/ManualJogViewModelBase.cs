using Autofac;
using CylType;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using RelayCommandBase;
using System;
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
    public class ManualJogViewModelBase : IMainScreenViewModel, IFactoryModule, INotifyPropertyChanged, ISetUpState
    {
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
                

                axis = this.MotionManager().GetAxis(EnumAxisConstants.X);
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

        //251029 yb add
        private async Task CX1PosMoveFunc()
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
                if (pos + apos < AxisObject.Param.PosSWLimit.Value) // 리밋체크 pos(움직일 거리)와 apos(기존 나의 위치)의 합이 리밋보다 작으면 동작
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    NegButtonVisibility = false;
                    //20251030 yb 주석처리
                    //retVal = this.StageSupervisor().StageModuleState.ManualRelMove(AxisObject, pos);

                    retVal = this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
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
                if (pos + apos > AxisObject.Param.NegSWLimit.Value)
                {
                    PosButtonVisibility = false;
                    //20251030 yb
                    //this.StageSupervisor().StageModuleState.ManualRelMove(AxisObject, pos);
                    this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
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

        private async Task PosRefresh()
        {
            try
            {
                IMotionManager Motionmanager = this.MotionManager();
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
                this.StageSupervisor().StageModuleState.ManualZDownMove();

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

        //251103 YB IO test
        private RelayCommand _OuputOffCommand;
        public ICommand OuputOffCommand
        {
            get
            {
                if (null == _OuputOffCommand) _OuputOffCommand = new RelayCommand(OuputOff);
                return _OuputOffCommand;
            }
        }

        private void OuputOff()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, false);
        }

        private RelayCommand _OutputOnCommand;
        public ICommand OutputOnCommand
        {
            get
            {
                if (null == _OutputOnCommand) _OutputOnCommand = new RelayCommand(OutputOn);
                return _OutputOnCommand;
            }
        }

        private void OutputOn()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, true); //air on
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1, false); 
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, true); //air off
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR1_OFF, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_MAGNETIC1, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, true); // vac on
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON1, false); 

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, true); //blow on
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF1, false); // 둘다  off

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACON2, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_VACOFF2, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, true); //air on
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2, false);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, true); //air off
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_ARM_AIR2_OFF, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_TRILEG_SUCTION, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_TRILEG_SUCTION, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_FD_VAC, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_FD_VAC, false);

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DO_EJ_VAC, false);


        }
    }
}
