using Autofac;
using Autofac.Core;
using Focusing;
using LogModule;
using OpenCvSharp;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Monitoring;
using ProberInterfaces.Param;
using ProberInterfaces.Temperature;
using RelayCommandBase;
using SorterSystem.OpenCV;
using SorterSystem.VM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UcDisplayPort;
using static SorterSystem.VM.IOControlVM;
using IModule = ProberInterfaces.IModule;

namespace SorterSystem
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sf = parameter == null ? "{0:0.00}" : $"{{0:{parameter}}}";
            return string.Format(sf, value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Double.Parse(value.ToString());
        }
    }

    public class SelectorBrush : IValueConverter
    {
        private Brush _unselBrush = new SolidColorBrush(Colors.LightGray);
        private Brush _selBrush = new SolidColorBrush(Colors.Red);
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("SelectorBrush:{0}, {1}", value, parameter);
            if ("WHCam".Equals(parameter) && value.Equals(EnumProberCam.WAFER_HIGH_CAM)) return _selBrush;
            if ("WLCam".Equals(parameter) && value.Equals(EnumProberCam.WAFER_LOW_CAM)) return _selBrush;
            if ("PHCam".Equals(parameter) && value.Equals(EnumProberCam.PIN_HIGH_CAM)) return _selBrush;
            if ("PLCam".Equals(parameter) && value.Equals(EnumProberCam.PIN_LOW_CAM)) return _selBrush;
            if ("BOOL".Equals(parameter) && value.Equals(true)) return _selBrush;
            if ("INT".Equals(parameter) && !value.Equals(0)) return _selBrush;
            return _unselBrush;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;
            if (!int.TryParse(value.ToString(), out result))
            {
                result = 0;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }

    public class ExecuteItem : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ExecuteItem()
        {

        }

        public ExecuteItem(string path)
        {
            this.Path = path;
        }

        private string _Path;
        public string Path
        {
            get { return _Path; }
            set
            {
                if (value != _Path)
                {
                    _Path = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class AxisobjectVM : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //private ObservableCollection<ExecuteItem> _ExecuteItemCollection;
        //public ObservableCollection<ExecuteItem> ExecuteItemCollection
        //{
        //    get { return _ExecuteItemCollection; }
        //    set
        //    {
        //        _ExecuteItemCollection = value;
        //        NotifyPropertyChanged(nameof(ExecuteItemCollection));
        //    }
        //}

        public AxisobjectVM()
        {
            Provider = this.MotionManager().GetMotionProvider();
        }
        public IMotionProvider Provider;
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

        private double _AbsMovePosition;
        public double AbsMovePosition
        {
            get { return _AbsMovePosition; }
            set
            {
                if (value != _AbsMovePosition)
                {
                    _AbsMovePosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnablePosButton = true;
        public bool IsEnablePosButton
        {
            get { return AxisObject == null ? false : _IsEnablePosButton && AxisObject.Status.IsHomeSeted; }
            set
            {
                if (value != _IsEnablePosButton)
                {
                    _IsEnablePosButton = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsEnableAbsButton));
                }
            }
        }

        private bool _IsEnableNegButton = true;
        public bool IsEnableNegButton
        {
            get { return AxisObject == null ? false : _IsEnableNegButton && AxisObject.Status.IsHomeSeted; }
            set
            {
                if (value != _IsEnableNegButton)
                {
                    _IsEnableNegButton = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsEnableAbsButton));
                }
            }
        }

        public bool IsEnableAbsButton
        {
            get { return AxisObject == null ? false : _IsEnablePosButton && _IsEnableNegButton && AxisObject.Status.IsHomeSeted; }
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


        private RelayCommand<object> _EnableAxisCommand;
        public RelayCommand<object> EnableAxisCommand
        {
            get
            {
                if (_EnableAxisCommand == null)
                    _EnableAxisCommand = new RelayCommand<object>(OnEnableAxisCommand);
                return _EnableAxisCommand;
            }
        }
        private void OnEnableAxisCommand(object obj)
        {
            if (AxisObject.Status.AxisEnabled)
            {
                this.MotionManager().DisableAxis(AxisObject);
            }
            else
            {
                this.MotionManager().EnableAxis(AxisObject);
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
                        IsEnableNegButton = false;
                        Provider.RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                        Provider.WaitForAxisMotionDone(AxisObject, AxisObject.Param.TimeOut.Value);
                    }
                    else
                    {
                        //Sw limit
                    }
                });

                IsEnableNegButton = true;
            }
            catch (Exception err)
            {
                IsEnableNegButton = true;
                LoggerManager.Exception(err);
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
                        IsEnablePosButton = false;
                        Provider.RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                        Provider.WaitForAxisMotionDone(AxisObject, AxisObject.Param.TimeOut.Value);
                    }
                    else
                    {
                        //Sw Limit
                    }
                });
                IsEnablePosButton = true;
            }
            catch (Exception err)
            {
                IsEnablePosButton = true;
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _AbsMoveCommand;
        public ICommand AbsMoveCommand
        {
            get
            {
                if (null == _AbsMoveCommand) _AbsMoveCommand = new AsyncCommand(AbsMove);
                return _AbsMoveCommand;
            }
        }
        private async Task AbsMove()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (AbsMovePosition > AxisObject.Param.NegSWLimit.Value && AbsMovePosition < AxisObject.Param.PosSWLimit.Value)
                    {
                        IsEnablePosButton = false;
                        Provider.AbsMove(AxisObject, AbsMovePosition, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                        Provider.WaitForAxisMotionDone(AxisObject, AxisObject.Param.TimeOut.Value);
                    }
                    else
                    {
                        //Sw Limit
                    }
                });
                IsEnablePosButton = true;
            }
            catch (Exception err)
            {
                IsEnablePosButton = true;
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _HomingCommand;
        public ICommand HomingCommand
        {
            get
            {
                if (null == _HomingCommand) _HomingCommand = new AsyncCommand(Homing);
                return _HomingCommand;
            }
        }

        private async Task Homing()
        {
            try
            {
                IsEnablePosButton = false;
                IsEnableNegButton = false;
                await Task.Run(() =>
                {
                    this.MotionManager().Homing(this.AxisObject.AxisType.Value);
                });
                IsEnablePosButton = true;
                IsEnableNegButton = true;
            }
            catch (Exception err)
            {
                IsEnablePosButton = true;
                IsEnableNegButton = true;

                LoggerManager.Exception(err);
            }

        }

    }

    public class SorterModuleVM : IModule, INotifyPropertyChanged, IFactoryModule, ISorterModuleVM
    {
        public SorterModuleVM()
        {
            _VisionViewModel = new VisionVM(this);
            _SequenceViewModel = new SequenceVM(this);
        }

        public bool Initialized { get; set; }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void OverlayDisplay(ImageBuffer image, ICamera cam)
        {
        }

        private AxisobjectVM _AxisZ0;
        public AxisobjectVM AxisZ0
        {
            get { return _AxisZ0; }
            set
            {
                if (value != _AxisZ0)
                {
                    _AxisZ0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisobjectVM _AxisZ1;
        public AxisobjectVM AxisZ1
        {
            get { return _AxisZ1; }
            set
            {
                if (value != _AxisZ1)
                {
                    _AxisZ1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisobjectVM _AxisZ2;
        public AxisobjectVM AxisZ2
        {
            get { return _AxisZ2; }
            set
            {
                if (value != _AxisZ2)
                {
                    _AxisZ2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisobjectVM _AxisZ;
        public AxisobjectVM AxisZ
        {
            get { return _AxisZ; }
            set
            {
                if (value != _AxisZ)
                {
                    _AxisZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AxisZPos = 0.0;
        public double AxisZPos
        {
            get
            {
                if (AxisZ == null || AxisZ.AxisObject == null) return 0.0d;
                return AxisZ.AxisObject.Status.Position.Ref;
            }
            set
            {
                if (_AxisZPos != value)
                {
                    _AxisZPos = value;
                    RaisePropertyChanged();
                }
            }
        }


        private AxisobjectVM _AxisX;
        public AxisobjectVM AxisX
        {
            get { return _AxisX; }
            set
            {
                if (value != _AxisX)
                {
                    _AxisX = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(AxisXPos));
                }
            }
        }

        private double _AxisXPos = 0.0;
        public double AxisXPos
        {
            get
            {
                if (AxisX == null || AxisX.AxisObject == null) return 0.0d;
                return AxisX.AxisObject.Status.Position.Ref;
            }

            set
            {
                if (_AxisXPos != value)
                {
                    _AxisXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisobjectVM _AxisY;
        public AxisobjectVM AxisY
        {
            get { return _AxisY; }
            set
            {
                if (value != _AxisY)
                {
                    _AxisY = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(AxisYPos));
                }
            }
        }

        private double _AxisYPos = 0.0;
        public double AxisYPos
        {
            get
            {
                if (AxisY == null || AxisY.AxisObject == null) return 0.0d;
                return AxisY.AxisObject.Status.Position.Ref;
            }
            set
            {
                if (_AxisYPos != value)
                {
                    _AxisYPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisobjectVM _AxisT;
        public AxisobjectVM AxisT
        {
            get { return _AxisT; }
            set
            {
                if (value != _AxisT)
                {
                    _AxisT = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(AxisTPos));
                }
            }
        }

        private double _AxisTPos = 0.0;
        public double AxisTPos
        {
            get
            {
                if (AxisT == null || AxisT.AxisObject == null) return 0.0d;
                return AxisT.AxisObject.Status.Position.Ref;
            }
            set
            {
                if (_AxisTPos != value)
                {
                    _AxisTPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisobjectVM _AxisPZ;
        public AxisobjectVM AxisPZ
        {
            get { return _AxisPZ; }
            set
            {
                if (value != _AxisPZ)
                {
                    _AxisPZ = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(AxisPZPos));
                }
            }
        }

        private double _AxisPZPos = 0.0;
        public double AxisPZPos
        {
            get
            {
                if (AxisPZ == null || AxisPZ.AxisObject == null) return 0.0d;
                return AxisPZ.AxisObject.Status.Position.Ref;
            }
            set
            {
                if (_AxisPZPos != value)
                {
                    _AxisPZPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITempController _tempController;
        public ITempController TempController
        {
            get
            {
                if (_tempController == null) _tempController = Container.Resolve<ITempController>();
                return _tempController;
            }
            set
            {
                if (_tempController != value)
                {
                    _tempController = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IOControlVM _IOControlViewModel = new IOControlVM();
        public IOControlVM IOControlViewModel
        {
            get { return _IOControlViewModel; }
            set
            {
                if (_IOControlViewModel != value)
                {
                    _IOControlViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VisionVM _VisionViewModel;
        public VisionVM VisionViewModel
        {
            get { return _VisionViewModel; }
            set
            {
                if (_VisionViewModel != value)
                {
                    _VisionViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SequenceVM _SequenceViewModel;
        public SequenceVM SequenceViewModel
        {
            get { return _SequenceViewModel; }
            set
            {
                if (_SequenceViewModel != value)
                {
                    _SequenceViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                var mot = this.MotionManager();

                AttatchedCameras = new ObservableCollection<ICamera>();
                //AttatchedCameras[0].Param.ChannelType.Value
                WLCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                AttatchedCameras.Add(WLCam);
                WHCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                AttatchedCameras.Add(WHCam);
                PHCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                AttatchedCameras.Add(PHCam);
                PLCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                AttatchedCameras.Add(PLCam);

                DisplayPort = new DisplayPort();

                _DisplayPort.ShowOverlays();
                //DisplayPort.AssignedCamera = WHCam;
                //this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                //this.VisionManager().StartGrab(DisplayPort.AssignedCamera.Param.ChannelType.Value, this);

                StageAxes aes = this.MotionManager().StageAxes;
                foreach (var item in aes.ProbeAxisProviders)
                {
                    if (item.AxisType.Value == EnumAxisConstants.R || item.AxisType.Value == EnumAxisConstants.TT ||
                            item.AxisType.Value == EnumAxisConstants.Z0 || item.AxisType.Value == EnumAxisConstants.Z1 ||
                            item.AxisType.Value == EnumAxisConstants.Z2)
                    {
                        var axisObjVM = new AxisobjectVM();
                        axisObjVM.AxisObject = item;
                        axisObjVM.IsEnableNegButton = false;
                        axisObjVM.IsEnablePosButton = false;

                        StageAxisObjectVmList.Add(axisObjVM);
                    }
                    else
                    {
                        var axisObjVM = new AxisobjectVM();
                        axisObjVM.AxisObject = item;

                        StageAxisObjectVmList.Add(axisObjVM);
                    }

                    if (item.AxisType.Value == EnumAxisConstants.Z0)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisZ0 = axisVM;

                    }
                    if (item.AxisType.Value == EnumAxisConstants.Z1)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisZ1 = axisVM;
                    }
                    if (item.AxisType.Value == EnumAxisConstants.Z2)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisZ2 = axisVM;
                    }

                    if (item.AxisType.Value == EnumAxisConstants.X)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisX = axisVM;
                    }

                    if (item.AxisType.Value == EnumAxisConstants.Y)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisY = axisVM;
                    }

                    if (item.AxisType.Value == EnumAxisConstants.Z)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisZ = axisVM;
                    }

                    if (item.AxisType.Value == EnumAxisConstants.PZ)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisPZ = axisVM;
                    }

                    if (item.AxisType.Value == EnumAxisConstants.C)
                    {
                        var axisVM = new AxisobjectVM();
                        axisVM.AxisObject = item;
                        axisVM.IsEnableNegButton = false;
                        axisVM.IsEnablePosButton = false;
                        AxisT = axisVM;
                    }
                }

                foreach (var port in this.IOManager().IO.GetInputPorts())
                    if (port.Key.Value.StartsWith("DI_CH"))
                        IOControlViewModel.InputPorts.Add(port);
                foreach (var port in this.IOManager().IO.GetOutputPorts())
                    if (port.Key.Value.StartsWith("DO_CH"))
                        IOControlViewModel.OutputPorts.Add(port);

                IOControlViewModel.AnalogInputs.Clear();
                foreach (var chan in this.IOManager().IOServ.AnalogInputs)
                {
                    if (chan.IOType == EnumIOType.AI)
                    {
                        int portNum = 0;
                        foreach (var portValue in chan.Values)
                        {
                            string keyName = string.Format("AI_CH{0:D1}P{1:D2}", chan.ChannelIndex, portNum);
                            IOControlViewModel.AnalogInputs.Add(new AnalogPortDescripter(chan.ChannelIndex, portNum, keyName, chan.IOType, portValue));
                            portNum++;
                        }
                    }
                }
                TempController.SetActivatedState(true);

                errorCode = _SequenceViewModel.InitModule();
                Debug.Assert(errorCode == EventCodeEnum.NONE);

                foreach (var loc in _SequenceViewModel.SequenceLocationList)
                {
                    switch (loc.Name)
                    {
                        case "WH_DIE":
                            {
                                DiePosition = new GlmSharp.dvec4(loc.Position.X, loc.Position.Y, loc.Position.Z, loc.Position.T);
                            }
                            break;
                        case "WH_MARKER":
                            {
                                MarkerPosition = new GlmSharp.dvec3(loc.Position.X, loc.Position.Y, loc.Position.PZ);
                            }
                            break;
                        case "PL_PICKER":
                            {
                                PickerPosition = new GlmSharp.dvec3(loc.Position.X, loc.Position.Y, loc.Position.PZ);
                            }
                            break;
                    }
                }


                errorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitModule(): Error occurred. Err = {err.Message}");
            }
            return errorCode;
        }

        public void DeInitModule()
        {
            //EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                _SequenceViewModel.DeInitModule();

                foreach (var cam in AttatchedCameras)
                {
                    this.VisionManager().StopGrab(cam.Param.ChannelType.Value);
                    cam.DisplayService.DispPorts.Clear();
                }
                foreach (var cam in AttatchedCameras)
                {
                    cam.DisplayService.DispPorts.Clear();
                }
                this.IOManager().DeInitModule();

                var modules = Container.Resolve<IEnumerable<IFactoryModule>>().Reverse();
                foreach (var item in modules)
                {
                    if (item is IModule)
                    {
                        (item as IModule).DeInitModule();
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInitModule(): Error occurred. Err = {err.Message}");
            }
        }

        private ObservableCollection<ExecuteItem> _Pathlist = new ObservableCollection<ExecuteItem>();
        public ObservableCollection<ExecuteItem> Pathlist
        {
            get { return _Pathlist; }
            set
            {
                _Pathlist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<AxisobjectVM> _StageAxisObjectVmList
            = new ObservableCollection<AxisobjectVM>();
        public ObservableCollection<AxisobjectVM> StageAxisObjectVmList
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

        private bool _LockIO;
        public bool LockIO
        {
            get { return _LockIO; }
            set
            {
                if (value != _LockIO)
                {
                    _LockIO = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UcDisplayPort.DisplayPort _DisplayPort;

        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value as UcDisplayPort.DisplayPort;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _WHCam;
        public ICamera WHCam
        {
            get { return _WHCam; }
            set
            {
                if (value != _WHCam)
                {
                    _WHCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ILightChannel> _WHLights;
        public ObservableCollection<ILightChannel> WHLights
        {
            get { return _WHLights; }
            set
            {
                if (value != _WHLights)
                {
                    _WHLights = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ICamera> _AttatchedCameras;
        public ObservableCollection<ICamera> AttatchedCameras
        {
            get { return _AttatchedCameras; }
            set
            {
                if (value != _AttatchedCameras)
                {
                    _AttatchedCameras = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _WLCam;
        public ICamera WLCam
        {
            get { return _WLCam; }
            set
            {
                if (value != _WLCam)
                {
                    _WLCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ILightChannel> _WLLights;
        public ObservableCollection<ILightChannel> WLLights
        {
            get { return _WLLights; }
            set
            {
                if (value != _WLLights)
                {
                    _WLLights = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _PHCam;
        public ICamera PHCam
        {
            get { return _PHCam; }
            set
            {
                if (value != _PHCam)
                {
                    _PHCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ILightChannel> _PHLights;
        public ObservableCollection<ILightChannel> PHLights
        {
            get { return _PHLights; }
            set
            {
                if (value != _PHLights)
                {
                    _PHLights = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ICamera _PLCam;
        public ICamera PLCam
        {
            get { return _PLCam; }
            set
            {
                if (value != _PLCam)
                {
                    _PLCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ILightChannel> _PLLights;
        public ObservableCollection<ILightChannel> PLLights
        {
            get { return _PLLights; }
            set
            {
                if (value != _PLLights)
                {
                    _PLLights = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IStageSupervisor _StageSupervisor = null;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (_StageSupervisor != value)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _BtnAngleP1Command;
        public ICommand BtnAngleP1Command
        {
            get
            {
                if (null == _BtnAngleP1Command) _BtnAngleP1Command = new AsyncCommand(BtnAngleP1);
                return _BtnAngleP1Command;
            }
        }

        private AsyncCommand _BtnAngleP2Command;
        public ICommand BtnAngleP2Command
        {
            get
            {
                if (null == _BtnAngleP2Command) _BtnAngleP2Command = new AsyncCommand(BtnAngleP2);
                return _BtnAngleP2Command;
            }
        }

        private GlmSharp.dvec3 angleP1 = new GlmSharp.dvec3(0);
        private GlmSharp.dvec3 angleP2 = new GlmSharp.dvec3(0);
        private async Task BtnAngleP1()
        {
            try
            {
                double curX = Math.Abs(AxisX.AxisObject.Status.Position.Command - AxisX.AxisObject.Status.Position.Actual) < 50 ? AxisX.AxisObject.Status.Position.Command : AxisX.AxisObject.Status.Position.Actual;
                double curY = Math.Abs(AxisY.AxisObject.Status.Position.Command - AxisY.AxisObject.Status.Position.Actual) < 50 ? AxisY.AxisObject.Status.Position.Command : AxisY.AxisObject.Status.Position.Actual;

                angleP1 = new GlmSharp.dvec3(curX, curY, 0.0);
                DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_RightTop, ref angleP1))
                {
                    angleP1 += new GlmSharp.dvec3(curX, curY, 0.0);
                    relX = angleP1.x - curX;
                    relY = angleP1.y - curY;
                    await LoadMove();
                    relX = relY = 0;
                }
                AngleP1P2 = LineAngle(angleP1, angleP2) * 10000.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task BtnAngleP2()
        {
            try
            {
                double curX = Math.Abs(AxisX.AxisObject.Status.Position.Command - AxisX.AxisObject.Status.Position.Actual) < 50 ? AxisX.AxisObject.Status.Position.Command : AxisX.AxisObject.Status.Position.Actual;
                double curY = Math.Abs(AxisY.AxisObject.Status.Position.Command - AxisY.AxisObject.Status.Position.Actual) < 50 ? AxisY.AxisObject.Status.Position.Command : AxisY.AxisObject.Status.Position.Actual;

                angleP2 = new GlmSharp.dvec3(curX, curY, 0.0);
                DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_LeftTop, ref angleP2))
                {
                    angleP2 += new GlmSharp.dvec3(curX, curY, 0.0);
                    relX = angleP2.x - curX;
                    relY = angleP2.y - curY;
                    await LoadMove();
                    relX = relY = 0;
                }
                AngleP1P2 = LineAngle(angleP1, angleP2) * 10000.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private double _AngleP1P2 = 0.0d;
        public double AngleP1P2
        {
            get
            {
                return _AngleP1P2;
            }
            set
            {
                if (_AngleP1P2 != value)
                {
                    _AngleP1P2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _SelWHCamCommand;
        public ICommand SelWHCamCommand
        {
            get
            {
                if (null == _SelWHCamCommand) _SelWHCamCommand = new AsyncCommand(SelWHCam);
                return _SelWHCamCommand;
            }
        }

        private async Task SelWHCam()
        {
            try
            {
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    relX = WLtoWH.x;
                    relY = WLtoWH.y;
                    relZ = WLtoWH.z;
                }
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera != WHCam)
                {
                    DisplayPort.AssignedCamera.DrawDisplayDelegate -= OverlayDisplay;
                    DisplayPort.AssignedCamera.DrawDisplayDelegate = null;
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, 0);
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                    this.VisionManager().StopGrab(DisplayPort.AssignedCamera.GetChannelType());
                }
                DisplayPort.AssignedCamera = WHCam;
                DisplayPort.AssignedCamera.DrawDisplayDelegate += OverlayDisplay;
                DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 150);
                DisplayPort.AssignedCamera.SetLight(EnumLightType.OBLIQUE, 150);
                CurrCam = DisplayPort.AssignedCamera.GetChannelType();
                this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                this.VisionManager().StartGrab(DisplayPort.AssignedCamera.GetChannelType(), this);
                RaisePropertyChanged(nameof(CoaxialLight));
                RaisePropertyChanged(nameof(ObliqueLight));
                await LoadMove();

                VisionParameter.BlobMinArea = 100000;
                VisionParameter.ThresholdMin = 220;
                VisionParameter.MorphologyOpen = 51;
                VisionParameter.MorphologyClose = 210;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _SelWLCamCommand;
        public ICommand SelWLCamCommand
        {
            get
            {
                if (null == _SelWLCamCommand) _SelWLCamCommand = new AsyncCommand(SelWLCam);
                return _SelWLCamCommand;
            }
        }

        private async Task SelWLCam()
        {
            try
            {
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    relX = -WLtoWH.x;
                    relY = -WLtoWH.y;
                    relZ = -WLtoWH.z;
                }
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera != WLCam)
                {
                    DisplayPort.AssignedCamera.DrawDisplayDelegate -= OverlayDisplay;
                    DisplayPort.AssignedCamera.DrawDisplayDelegate = null;
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, 0);
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                    this.VisionManager().StopGrab(DisplayPort.AssignedCamera.GetChannelType());
                }
                DisplayPort.AssignedCamera = WLCam;
                DisplayPort.AssignedCamera.DrawDisplayDelegate += OverlayDisplay;
                DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 85);
                DisplayPort.AssignedCamera.SetLight(EnumLightType.OBLIQUE, 85);
                CurrCam = DisplayPort.AssignedCamera.GetChannelType();
                this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                this.VisionManager().StartGrab(DisplayPort.AssignedCamera.GetChannelType(), this);
                RaisePropertyChanged(nameof(CoaxialLight));
                RaisePropertyChanged(nameof(ObliqueLight));
                await LoadMove();

                VisionParameter.BlobMinArea = 100000;
                VisionParameter.ThresholdMin = 89;
                VisionParameter.MorphologyOpen = 51;
                VisionParameter.MorphologyClose = 5;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _SelPHCamCommand;
        public ICommand SelPHCamCommand
        {
            get
            {
                if (null == _SelPHCamCommand) _SelPHCamCommand = new AsyncCommand(SelPHCam);
                return _SelPHCamCommand;
            }
        }

        private async Task SelPHCam()
        {
            try
            {
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                {
                    relX = PLtoPH.x;
                    relY = PLtoPH.y;
                    relPZ = PLtoPH.z;
                }
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera != PHCam)
                {
                    DisplayPort.AssignedCamera.DrawDisplayDelegate -= OverlayDisplay;
                    DisplayPort.AssignedCamera.DrawDisplayDelegate = null;
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, 0);
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                    this.VisionManager().StopGrab(DisplayPort.AssignedCamera.GetChannelType());
                }
                DisplayPort.AssignedCamera = PHCam;
                DisplayPort.AssignedCamera.DrawDisplayDelegate += OverlayDisplay;
                DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 120);
                DisplayPort.AssignedCamera.SetLight(EnumLightType.OBLIQUE, 0);
                CurrCam = DisplayPort.AssignedCamera.GetChannelType();
                this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                this.VisionManager().StartGrab(DisplayPort.AssignedCamera.GetChannelType(), this);
                RaisePropertyChanged(nameof(CoaxialLight));
                RaisePropertyChanged(nameof(ObliqueLight));
                await LoadMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _SelPLCamCommand;
        public ICommand SelPLCamCommand
        {
            get
            {
                if (null == _SelPLCamCommand) _SelPLCamCommand = new AsyncCommand(SelPLCam);
                return _SelPLCamCommand;
            }
        }

        private async Task SelPLCam()
        {
            try
            {
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera.GetChannelType() == EnumProberCam.PIN_HIGH_CAM)
                {
                    relX = -PLtoPH.x;
                    relY = -PLtoPH.y;
                    relPZ = -PLtoPH.z;
                }
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera != PLCam)
                {
                    DisplayPort.AssignedCamera.DrawDisplayDelegate -= OverlayDisplay;
                    DisplayPort.AssignedCamera.DrawDisplayDelegate = null;
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, 0);
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                    this.VisionManager().StopGrab(DisplayPort.AssignedCamera.GetChannelType());
                }
                DisplayPort.AssignedCamera = PLCam;
                DisplayPort.AssignedCamera.DrawDisplayDelegate += OverlayDisplay;
                DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 50);
                DisplayPort.AssignedCamera.SetLight(EnumLightType.OBLIQUE, 0);
                CurrCam = DisplayPort.AssignedCamera.GetChannelType();
                this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                this.VisionManager().StartGrab(DisplayPort.AssignedCamera.GetChannelType(), this);
                RaisePropertyChanged(nameof(CoaxialLight));
                RaisePropertyChanged(nameof(ObliqueLight));
                await LoadMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region 기계적 오프셋 자료

        public GlmSharp.dvec3 PLtoPH = new GlmSharp.dvec3(-33092, 117, -113);
        public GlmSharp.dvec3 WLtoWH = new GlmSharp.dvec3(38769.6, 130.2, 283.8);
        public GlmSharp.dvec3 MKtoPH = new GlmSharp.dvec3(72, -113.65, 0);

        #endregion 기계적 오프셋 자료

        #region 매칭 및 상대 자료

        public GlmSharp.dvec3 MKtoPK = new GlmSharp.dvec3(-18607.0, 281019.0, 0.0);
        private GlmSharp.dvec3 _MarkerPosition = new GlmSharp.dvec3(7130.00, -246947.0, -4528.00);
        private GlmSharp.dvec3 _PickupPosition = new GlmSharp.dvec3(0, 0, -11850.0);
        private GlmSharp.dvec3 _PickupOffset = new GlmSharp.dvec3(0, 0, 0);
        private GlmSharp.dvec3 _PickerPosition;

        public GlmSharp.dvec4 DiePosition = new GlmSharp.dvec4(23409.00, -52277.00, -16403.00, 0.0);

        private bool _ChucktopAirControl = false;
        public bool ChucktopAirControl
        {
            get
            {
                return _ChucktopAirControl;
            }
            set
            {
                if (_ChucktopAirControl != value)
                {
                    _ChucktopAirControl = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion 매칭 및 상대 자료

        private AsyncCommand _BtnActionCommand;
        public ICommand BtnActionCommand
        {
            get
            {
                if (null == _BtnActionCommand) _BtnActionCommand = new AsyncCommand(BtnAction);
                return _BtnActionCommand;
            }
        }
        private async Task BtnAction()
        {
            try
            {
                await Task.Run(() =>
                {
                    Debug.WriteLine("Button Action");
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region 마커 위치로 이동 및 세팅

        public GlmSharp.dvec3 MarkerPosition
        {
            get
            {
                return _MarkerPosition;
            }

            set
            {
                if (_MarkerPosition != value)
                {
                    _MarkerPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _BtnSetMarkerActionCommand;
        public ICommand BtnSetMarkerActionCommand
        {
            get
            {
                if (null == _BtnSetMarkerActionCommand) _BtnSetMarkerActionCommand = new AsyncCommand(BtnSetMarkerAction);
                return _BtnSetMarkerActionCommand;
            }
        }
        private async Task BtnSetMarkerAction()
        {
            try
            {
                MarkerPosition = new GlmSharp.dvec3(AxisX.AxisObject.Status.Position.Command, AxisY.AxisObject.Status.Position.Command, AxisPZ.AxisObject.Status.Position.Command);
                PickerPosition = MarkerPosition + MKtoPK - PLtoPH;
                foreach (var loc in SequenceViewModel.SequenceLocationList)
                {
                    if ("WH_MARKER".Equals(loc.Name))
                    {
                        loc.Position.X = MarkerPosition.x;
                        loc.Position.Y = MarkerPosition.y;
                        loc.Position.PZ = MarkerPosition.z;
                        SequenceViewModel.SaveParameterSequence();
                        break;
                    }
                }
                await BtnGoPickerAction();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _BtnGoMarkerActionCommand;
        public ICommand BtnGoMarkerActionCommand
        {
            get
            {
                if (null == _BtnGoMarkerActionCommand) _BtnGoMarkerActionCommand = new AsyncCommand(BtnGoMarkerAction);
                return _BtnGoMarkerActionCommand;
            }
        }
        private async Task BtnGoMarkerAction()
        {
            try
            {
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera != WHCam)
                {
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, 0);
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                    this.VisionManager().StopGrab(DisplayPort.AssignedCamera.GetChannelType());
                }
                DisplayPort.AssignedCamera = WHCam;
                DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                DisplayPort.AssignedCamera.SetLight(EnumLightType.OBLIQUE, 0);
                CurrCam = DisplayPort.AssignedCamera.GetChannelType();
                this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                this.VisionManager().StartGrab(DisplayPort.AssignedCamera.GetChannelType(), this);
                PHCam.SetLight(EnumLightType.COAXIAL, 0);
                PHCam.SetLight(EnumLightType.AUX, 32);

                RaisePropertyChanged(nameof(CoaxialLight));
                RaisePropertyChanged(nameof(ObliqueLight));

                await Task.Run(async () =>
                {
                    Debug.WriteLine("Go Marker Action");
                    AxisX.AbsMovePosition = MarkerPosition.x;
                    AxisY.AbsMovePosition = MarkerPosition.y;
                    AxisPZ.AbsMovePosition = MarkerPosition.z;

                    AxisX.AxisObject.Status.Position.Ref = AxisX.AbsMovePosition;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AbsMovePosition;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AbsMovePosition;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AxisObject.Status.Position.Command;
                    RaisePropertyChanged();

                    await AbsMove();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion 마커 위치로 이동 및 세팅

        public double PickupHeight
        {
            get
            {
                return _PickupPosition.z;
            }

            set
            {
                if (_PickupPosition.z != value)
                {
                    _PickupPosition.z = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double PickupOffsetX
        {
            get
            {
                return _PickupOffset.x;
            }

            set
            {
                if (_PickupOffset.x != value)
                {
                    _PickupOffset.x = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double PickupOffsetY
        {
            get
            {
                return _PickupOffset.y;
            }

            set
            {
                if (_PickupOffset.y != value)
                {
                    _PickupOffset.y = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region 피커 위치로 이동 및 세팅

        public GlmSharp.dvec3 PickerPosition
        {
            get
            {
                return _PickerPosition;
            }

            set
            {
                if (_PickerPosition != value)
                {
                    _PickerPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _BtnSetPickerActionCommand;
        public ICommand BtnSetPickerActionCommand
        {
            get
            {
                if (null == _BtnSetPickerActionCommand) _BtnSetPickerActionCommand = new AsyncCommand(BtnSetPickerAction);
                return _BtnSetPickerActionCommand;
            }
        }
        private async Task BtnSetPickerAction()
        {
            try
            {
                PickerPosition = new GlmSharp.dvec3(AxisX.AxisObject.Status.Position.Command, AxisY.AxisObject.Status.Position.Command, AxisPZ.AxisObject.Status.Position.Command);
                MKtoPK = new GlmSharp.dvec3((PickerPosition - MarkerPosition + PLtoPH + MKtoPH).xy, 0.0);
                foreach (var loc in SequenceViewModel.SequenceLocationList)
                {
                    if ("PL_PICKER".Equals(loc.Name))
                    {
                        loc.Position.X = PickerPosition.x;
                        loc.Position.Y = PickerPosition.y;
                        loc.Position.PZ = PickerPosition.z;
                        SequenceViewModel.SaveParameterSequence();
                        break;
                    }
                }
                await Task.Run(() => { });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _BtnGoPickerActionCommand;
        public ICommand BtnGoPickerActionCommand
        {
            get
            {
                if (null == _BtnGoPickerActionCommand) _BtnGoPickerActionCommand = new AsyncCommand(BtnGoPickerAction);
                return _BtnGoPickerActionCommand;
            }
        }
        private async Task BtnGoPickerAction()
        {
            try
            {
                if (DisplayPort.AssignedCamera != null && DisplayPort.AssignedCamera != PLCam)
                {
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, 0);
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 0);
                    this.VisionManager().StopGrab(DisplayPort.AssignedCamera.GetChannelType());
                }
                DisplayPort.AssignedCamera = PLCam;
                DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, 85);
                DisplayPort.AssignedCamera.SetLight(EnumLightType.OBLIQUE, 0);
                CurrCam = DisplayPort.AssignedCamera.GetChannelType();
                this.VisionManager().SetDisplayChannel(DisplayPort.AssignedCamera, DisplayPort);
                this.VisionManager().StartGrab(DisplayPort.AssignedCamera.GetChannelType(), this);
                RaisePropertyChanged(nameof(CoaxialLight));
                RaisePropertyChanged(nameof(ObliqueLight));

                await Task.Run(async () =>
                {
                    Debug.WriteLine("Go Picker Action");
                    if (PickerPosition == null)
                    {
                        AxisX.AbsMovePosition = 21615.05;
                        AxisY.AbsMovePosition = 33958.50;
                        AxisPZ.AbsMovePosition = 1450;
                    }
                    else
                    {
                        AxisX.AbsMovePosition = PickerPosition.x;
                        AxisY.AbsMovePosition = PickerPosition.y;
                        AxisPZ.AbsMovePosition = 1450;
                    }

                    AxisX.AxisObject.Status.Position.Ref = AxisX.AbsMovePosition;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AbsMovePosition;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AbsMovePosition;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AxisObject.Status.Position.Command;
                    RaisePropertyChanged();

                    await AbsMove();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion 피커 위치로 이동 및 세팅

        #region DIE 위치로 이동

        private AsyncCommand _BtnGoDieActionCommand;
        public ICommand BtnGoDieActionCommand
        {
            get
            {
                if (null == _BtnGoDieActionCommand) _BtnGoDieActionCommand = new AsyncCommand(BtnGoDieAction);
                return _BtnGoDieActionCommand;
            }
        }
        private async Task BtnGoDieAction()
        {
            try
            {
                ICamera ACam = DisplayPort.AssignedCamera;
                EnumProberCam eCam = ACam.GetChannelType();

                if (eCam == EnumProberCam.WAFER_HIGH_CAM)
                {
                    AxisX.AxisObject.Status.Position.Ref = AxisX.AbsMovePosition = DiePosition.x;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AbsMovePosition = DiePosition.y;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AbsMovePosition = DiePosition.z;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AbsMovePosition = DiePosition.w;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Param.ClearedPosition.Value;
                }
                else if (eCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    GlmSharp.dvec4 pos = DiePosition + new GlmSharp.dvec4(-WLtoWH, 0);
                    AxisX.AxisObject.Status.Position.Ref = AxisX.AbsMovePosition = pos.x;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AbsMovePosition = pos.y;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AbsMovePosition = pos.z;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AbsMovePosition = pos.w;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Param.ClearedPosition.Value;
                }
                else if (eCam == EnumProberCam.PIN_HIGH_CAM || eCam == EnumProberCam.PIN_LOW_CAM)
                {
                    await SelWHCam();

                    AxisX.AxisObject.Status.Position.Ref = AxisX.AbsMovePosition = DiePosition.x;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AbsMovePosition = DiePosition.y;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AbsMovePosition = DiePosition.z;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AbsMovePosition = DiePosition.w;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Param.ClearedPosition.Value;
                }

                await AbsMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion DIE 위치로 이동


        #region DIE Pick 위치로 이동

        private AsyncCommand _BtnSetDieActionCommand;
        public ICommand BtnSetDieActionCommand
        {
            get
            {
                if (null == _BtnSetDieActionCommand) _BtnSetDieActionCommand = new AsyncCommand(BtnSetDieAction);
                return _BtnSetDieActionCommand;
            }
        }
        private async Task BtnSetDieAction()
        {
            try
            {
                DiePosition = new GlmSharp.dvec4(AxisX.AxisObject.Status.Position.Command, AxisY.AxisObject.Status.Position.Command, AxisZ.AxisObject.Status.Position.Command, AxisT.AxisObject.Status.Position.Command);
                if (CurrCam == EnumProberCam.WAFER_LOW_CAM) DiePosition += new GlmSharp.dvec4(WLtoWH, 0);

                _PickupPosition = new GlmSharp.dvec3(DiePosition.xy + MKtoPK.xy, _PickupPosition.z);
                foreach (var loc in SequenceViewModel.SequenceLocationList)
                {
                    if ("WH_DIE".Equals(loc.Name))
                    {
                        loc.Position.X = DiePosition.x;
                        loc.Position.Y = DiePosition.y;
                        loc.Position.Z = DiePosition.z;
                        loc.Position.T = DiePosition.w;
                        SequenceViewModel.SaveParameterSequence();
                        break;
                    }
                }
                RaisePropertyChanged(nameof(PickupHeight));
                await Task.Run(() => { });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _BtnGoDiePickActionCommand;
        public ICommand BtnGoDiePickActionCommand
        {
            get
            {
                if (null == _BtnGoDiePickActionCommand) _BtnGoDiePickActionCommand = new AsyncCommand(BtnGoDiePickAction);
                return _BtnGoDiePickActionCommand;
            }
        }
        private async Task BtnGoDiePickAction()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Debug.WriteLine("Go Move to Pickup Action");
                    AxisX.AbsMovePosition = _PickupPosition.x;
                    AxisY.AbsMovePosition = _PickupPosition.y;
                    AxisZ.AbsMovePosition = AxisZ.AxisObject.Param.ClearedPosition.Value;
                    AxisPZ.AbsMovePosition = AxisPZ.AxisObject.Param.ClearedPosition.Value;
                    AxisX.AxisObject.Status.Position.Ref = AxisX.AbsMovePosition;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AbsMovePosition;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AbsMovePosition;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AbsMovePosition;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    RaisePropertyChanged();

                    await AbsMove();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion DIE Pick 위치로 이동

        #region DIE Pick 액션

        private AsyncCommand _BtnPickDieActionCommand;
        public ICommand BtnPickDieActionCommand
        {
            get
            {
                if (null == _BtnPickDieActionCommand) _BtnPickDieActionCommand = new AsyncCommand(BtnPickDieAction);
                return _BtnPickDieActionCommand;
            }
        }
        private async Task BtnPickDieAction()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Debug.WriteLine("Button Action");

                    AxisX.AxisObject.Status.Position.Ref = AxisX.AxisObject.Status.Position.Command;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AxisObject.Status.Position.Command;
                    AxisZ.AxisObject.Status.Position.Ref = _PickupPosition.z;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Status.Position.Command;
                    await AbsMove();
                    Thread.Sleep(500);
                    if (ChucktopAirControl)
                    {
                        // 척탑 진공 해제
                        IOControlViewModel.OutputPorts[1].ResetValue();
                        Thread.Sleep(500);
                        // 피커 진공 시작
                        IOControlViewModel.OutputPorts[0].SetValue();
                    }
                    else
                    {
                        // 피커 진공 시작
                        IOControlViewModel.OutputPorts[0].SetValue();
                    }
                    Thread.Sleep(1500);
                    // 안전 위치로 이동
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AxisObject.Param.ClearedPosition.Value;
                    await AbsMove();

                    if (ChucktopAirControl)
                    {
                        // 척탑 진공 적용
                        IOControlViewModel.OutputPorts[1].SetValue();
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion DIE Pick 액션

        #region DIE Place 액션

        private AsyncCommand _BtnPlaceDieActionCommand;
        public ICommand BtnPlaceDieActionCommand
        {
            get
            {
                if (null == _BtnPlaceDieActionCommand) _BtnPlaceDieActionCommand = new AsyncCommand(BtnPlaceDieAction);
                return _BtnPlaceDieActionCommand;
            }
        }
        private async Task BtnPlaceDieAction()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Debug.WriteLine("Button Action");
                    if (ChucktopAirControl)
                    {
                        // 척탑 진공 해제
                        IOControlViewModel.OutputPorts[1].ResetValue();
                        Thread.Sleep(100);
                    }
                    // Place 높이로 이동
                    AxisX.AxisObject.Status.Position.Ref = AxisX.AxisObject.Status.Position.Command + _PickupOffset.x;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AxisObject.Status.Position.Command + _PickupOffset.y;
                    AxisZ.AxisObject.Status.Position.Ref = _PickupPosition.z;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Status.Position.Command;
                    await AbsMove();

                    Thread.Sleep(500);
                    if (ChucktopAirControl)
                    {
                        // 피커 진공 해제
                        IOControlViewModel.OutputPorts[0].ResetValue();
                        Thread.Sleep(500);
                        // 척탑 진공 적용
                        IOControlViewModel.OutputPorts[1].SetValue();
                    }
                    else
                    {
                        // 피커 진공 해제
                        IOControlViewModel.OutputPorts[0].ResetValue();
                    }
                    Thread.Sleep(1500);
                    // 안전위치로 이동
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AxisObject.Param.ClearedPosition.Value;
                    await AbsMove();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion DIE Place 액션



        private AsyncCommand _BtnSetCommand;
        public ICommand BtnSetCommand
        {
            get
            {
                if (null == _BtnSetCommand) _BtnSetCommand = new AsyncCommand(BtnSet);
                return _BtnSetCommand;
            }
        }

        private Brush _BtnSetGreen = new SolidColorBrush(Colors.Green);
        private Brush _BtnSetRed = new SolidColorBrush(Colors.Red);
        public Brush BtnSetLoad
        {
            get
            {
                return p1.Equals(GlmSharp.dvec3.Zero) ? _BtnSetGreen : _BtnSetRed;
            }
        }

        private GlmSharp.dvec3 crossPos(GlmSharp.dvec3 p1, GlmSharp.dvec3 p2, GlmSharp.dvec3 p3, GlmSharp.dvec3 p4)
        {
            var px = (p1.x - p2.x);
            var py = (p1.y - p2.y);
            var nx = (p3.x - p4.x);
            var ny = (p3.y - p4.y);
            var c1 = px * nx;
            var c2 = py * ny;
            var d = c1 - c2;
            if (d == 0) return GlmSharp.dvec3.Zero;
            var pre = (p1.x * p2.y - p1.y * p2.x);
            var post = (p3.x * p4.y - p3.y * p4.x);
            var x = (pre * (p3.x - p4.x) - (p1.x - p2.x) * post) / d;
            var y = (pre * (p3.y - p4.y) - (p1.y - p2.y) * post) / d;
            return new GlmSharp.dvec3(x, y, 0);
        }

        private GlmSharp.dvec3 rotateOrigin(GlmSharp.dvec3 p1, GlmSharp.dvec3 p3)
        {
            // RotateOrigin
            var o1 = p3 - p1;
            var o2 = o1.Length;
            var o3 = o1.NormalizedSafe;
            var o4 = new GlmSharp.dvec3(0, 0, o3.x / Math.Abs(o3.x));
            var o5 = GlmSharp.dvec3.Cross(o4, o3);
            var o6 = (p3 + p1) * 0.5;
            var oi = o6.y / Math.Abs(o6.y);
            var o7 = Math.Tan(GlmSharp.dvec3.Radians(90).x - oi * 0.5) * o2 * 0.5;
            var o8 = o5 * o7 + o6;
            return o8;
        }


        private GlmSharp.dvec3 rectCenter(GlmSharp.dvec3 p1, GlmSharp.dvec3 p2)
        {
            var o1 = p2 - p1;
            var o2 = o1.Length;
            var o3 = o1.NormalizedSafe;
            var o4 = new GlmSharp.dvec3(0, 0, -1);
            var o5 = new GlmSharp.dvec3(GlmSharp.dmat4.Rotate(GlmSharp.dvec3.Radians(-90).x, o4) * new GlmSharp.dvec4(o3, 0));
            var o6 = o5 * o2;
            var p3 = p1 + o6;
            var p4 = p2 + o6;
            var dc = p1 * 1.0;
            dc += p2;
            dc += p3;
            dc += p4;
            dc *= 0.25;
            return dc;
        }

        private GlmSharp.dvec3 dieCenter(GlmSharp.dvec3 p1, GlmSharp.dvec3 p2)
        {
            return rectCenter(p1, p2);
        }

        private double LineAngle(GlmSharp.dvec3 p1, GlmSharp.dvec3 p2)
        {
            var t0 = p2 - p1;
            var l0 = t0.Length;
            var n0 = t0.NormalizedSafe;
            var o6 = dieCenter(p1, p2);
            var oi = o6.y / Math.Abs(o6.y);
            var b0 = new GlmSharp.dvec3(0, -oi, 0);
            var i0 = GlmSharp.dvec3.Dot(b0, n0);
            var thr = GlmSharp.dvec3.Radians(90).x - Math.Acos(i0); // 회전각
            var thd = GlmSharp.dvec3.Degrees(thr).x;    // 회전각
            if (thd > 90) thd = -(180 - thd);
            return thd;
        }

        private GlmSharp.dvec3 anglePos(GlmSharp.dvec3 cp, GlmSharp.dvec3 p1, double deg)
        {
            var o1 = p1 - cp;
            var o2 = o1.NormalizedSafe;
            var o3 = o1.Length;
            var o4 = new GlmSharp.dvec3(0, 0, -1);
            var o5 = new GlmSharp.dvec3(GlmSharp.dmat4.Rotate(GlmSharp.dvec3.Radians(deg).x, o4) * new GlmSharp.dvec4(o2, 0));
            var o6 = o5 * o3;
            var o7 = o6 + cp;
            return o7;
        }

        private GlmSharp.dvec3 p1;
        private GlmSharp.dvec3 p2;
        private GlmSharp.dvec3 p3;
        private GlmSharp.dvec3 p4;
        private GlmSharp.dvec3 pCenter;
        private double tDiff = 0;

        public VisionOpenCV.Parameters VisionParameter
        {
            get { return VisionViewModel.VisionParameter; }
        }

        private bool GetEdgePosition(ImageBuffer imageBuffer, eObjDir dir, ref GlmSharp.dvec3 point)
        {
            Mat orgMat = Mat.FromPixelData(imageBuffer.SizeX, imageBuffer.SizeY, MatType.CV_8U, imageBuffer.Buffer);
            if (orgMat != null)
            {
                Mat dbgMat = orgMat.Clone();

                if (CurrCam == EnumProberCam.WAFER_HIGH_CAM)
                {
                    VisionOpenCV.OpenCVFindEdgeDeri(orgMat, dbgMat, VisionParameter, out Dictionary<string, Mat> resultMat, out List<stObject> points);
                    foreach (stObject obj in points)
                    {
                        if (obj.eDiePos == dir && !(Double.IsNaN(obj.posVertex.X) || Double.IsInfinity(obj.posVertex.X) || Double.IsNaN(obj.posVertex.Y) || Double.IsInfinity(obj.posVertex.Y)))
                        {
                            double rx = DisplayPort.AssignedCamera.GetRatioX();
                            double ry = DisplayPort.AssignedCamera.GetRatioY();
                            point.x = (obj.posVertex.X - orgMat.Width * 0.5) * rx * -1.0;
                            point.y = (obj.posVertex.Y - orgMat.Height * 0.5) * ry * +1.0;
                            return true;
                        }
                    }
                }
                else if (CurrCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    VisionOpenCV.OpenCVFindEdgeThres(orgMat, dbgMat, VisionParameter, out Dictionary<string, Mat> resultMat, out List<stObject> points);
                    foreach (stObject obj in points)
                    {
                        if (obj.eDiePos == dir && !(Double.IsNaN(obj.posVertex.X) || Double.IsInfinity(obj.posVertex.X) || Double.IsNaN(obj.posVertex.Y) || Double.IsInfinity(obj.posVertex.Y)))
                        {
                            double rx = DisplayPort.AssignedCamera.GetRatioX();
                            double ry = DisplayPort.AssignedCamera.GetRatioY();
                            point.x = (obj.posVertex.X - orgMat.Width * 0.5) * rx * -1.0;
                            point.y = (obj.posVertex.Y - orgMat.Height * 0.5) * ry * +1.0;
                            return true;
                        }
                    }
                }
                else if (CurrCam == EnumProberCam.PIN_LOW_CAM)
                {
                    VisionOpenCV.OpenCVFindEdgeThres(orgMat, dbgMat, VisionParameter, out Dictionary<string, Mat> resultMat, out List<stObject> points);
                    foreach (stObject obj in points)
                    {
                        if (obj.eDiePos == dir && !(Double.IsNaN(obj.posVertex.X) || Double.IsInfinity(obj.posVertex.X) || Double.IsNaN(obj.posVertex.Y) || Double.IsInfinity(obj.posVertex.Y)))
                        {
                            double rx = DisplayPort.AssignedCamera.GetRatioX();
                            double ry = DisplayPort.AssignedCamera.GetRatioY();
                            point.x = (obj.posVertex.X - orgMat.Width * 0.5) * rx * -1.0;
                            point.y = (obj.posVertex.Y - orgMat.Height * 0.5) * ry * +1.0;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private async Task BtnSet()
        {
            try
            {
                double curX = Math.Abs(AxisX.AxisObject.Status.Position.Command - AxisX.AxisObject.Status.Position.Actual) < 50 ? AxisX.AxisObject.Status.Position.Command : AxisX.AxisObject.Status.Position.Actual;
                double curY = Math.Abs(AxisY.AxisObject.Status.Position.Command - AxisY.AxisObject.Status.Position.Actual) < 50 ? AxisY.AxisObject.Status.Position.Command : AxisY.AxisObject.Status.Position.Actual;
                double curZ = Math.Abs(AxisZ.AxisObject.Status.Position.Command - AxisZ.AxisObject.Status.Position.Actual) < 50 ? AxisZ.AxisObject.Status.Position.Command : AxisZ.AxisObject.Status.Position.Actual;
                double curT = Math.Abs(AxisT.AxisObject.Status.Position.Command - AxisT.AxisObject.Status.Position.Actual) < 50 ? AxisT.AxisObject.Status.Position.Command : AxisT.AxisObject.Status.Position.Actual;

                if (p1.Equals(GlmSharp.dvec3.Zero))
                {
                    p1 = new GlmSharp.dvec3(curX, curY, 0.0);
                    relX = -10000;
                    DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                    if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_RightTop, ref p1))
                    {
                        p1 += new GlmSharp.dvec3(curX, curY, 0.0);
                        relX = p1.x - curX - 10000;
                        relY = p1.y - curY;
                    }
                    Debug.WriteLine("### SET 1 ###: ,p1: glm.vec3(" + p1.ToString() + ")");
                    RaisePropertyChanged(nameof(BtnSetLoad));
                }
                else if (p2.Equals(GlmSharp.dvec3.Zero))
                {
                    p2 = new GlmSharp.dvec3(curX, curY, 0.0);
                    relX = +10000;

                    DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                    if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_LeftTop, ref p2))
                    {
                        double rx = DisplayPort.AssignedCamera.GetRatioX();
                        p2 += new GlmSharp.dvec3(curX, curY, 0.0);
                        relX = p1.x - curX - rx * imageBuffer.SizeX * 0.25 * (p1.y > p2.y ? +1.0 : -1.0);
                        relY = p1.y - curY;
                        Debug.WriteLine("### SET 2 ###: ,p2: glm.vec3(" + p2.ToString() + ")");
                    }
                    tDiff = p1.y > p2.y ? -10000 : +10000;
                    relT = tDiff;
                }
                else if (p3.Equals(GlmSharp.dvec3.Zero))
                {
                    p3 = new GlmSharp.dvec3(curX, curY, 0.0);
                    relX = -10000;

                    DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                    if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_RightTop, ref p3))
                    {
                        p3 += new GlmSharp.dvec3(curX, curY, 0.0);
                        relX = p3.x - curX - 10000;
                        relY = p3.y - curY;
                    }
                    Debug.WriteLine("### SET 3 ###: ,p3: glm.vec3(" + p3.ToString() + ")");
                }
                else if (p4.Equals(GlmSharp.dvec3.Zero))
                {
                    p4 = new GlmSharp.dvec3(curX, curY, 0.0);
                    DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                    if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_LeftTop, ref p4))
                    {
                        p4 += new GlmSharp.dvec3(curX, curY, 0.0);
                    }
                    Debug.WriteLine("### SET 4 ###: ,p4: glm.vec3(" + p4.ToString() + ")");

                    pCenter = rotateOrigin(p1, p3);
                    var th = LineAngle(p1, p2);
                    var pt = anglePos(pCenter, p1, th);
                    relT = th * 10000 - tDiff;
                    relX = pt.x - curX;
                    relY = pt.y - curY;

                    Debug.WriteLine($"theta:{th}, center:{pCenter}, target:{pt}");
                }
                else
                {
                    var p5 = new GlmSharp.dvec3(curX, curY, 0.0);
                    DisplayPort.AssignedCamera.GetCurImage(out ImageBuffer imageBuffer);
                    if (GetEdgePosition(imageBuffer, eObjDir.eObjDir_RightTop, ref p5))
                    {
                        p5 += new GlmSharp.dvec3(curX, curY, 0.0);
                        relX = p5.x - curX;
                        relY = p5.y - curY;
                    }
                    Debug.WriteLine("### SET T ###: ,p5: glm.vec3(" + p5.ToString() + ")");
                    await LoadMove();

                    p1 = p2 = p3 = p4 = GlmSharp.dvec3.Zero;
                    relX = 0;
                    relY = 0;
                    relZ = 0;
                    relT = 0;

                    RaisePropertyChanged(nameof(BtnSetLoad));
                }
                await LoadMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _BtnLoadCommand;
        public ICommand BtnLoadCommand
        {
            get
            {
                if (null == _BtnLoadCommand) _BtnLoadCommand = new AsyncCommand(BtnLoad);
                return _BtnLoadCommand;
            }
        }

        private async Task BtnLoad()
        {
            try
            {
                if (CurrCam == EnumProberCam.WAFER_HIGH_CAM)
                {
                    AxisX.AxisObject.Status.Position.Ref = 36991.0;
                    AxisY.AxisObject.Status.Position.Ref = -125236.9;
                    AxisZ.AxisObject.Status.Position.Ref = -69122.4;
                    AxisT.AxisObject.Status.Position.Ref = 17114.2;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Status.Position.Actual;
                }
                else
                {
                    AxisX.AxisObject.Status.Position.Ref = -2100.1;
                    AxisY.AxisObject.Status.Position.Ref = -125372.7;
                    AxisZ.AxisObject.Status.Position.Ref = -69271.2;
                    AxisT.AxisObject.Status.Position.Ref = 17114.2;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Status.Position.Actual;
                }
                await AbsMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        double relX = 0D;
        double relY = 0D;
        double relZ = 0D;
        double relT = 0D;
        double relPZ = 0D;

        private async Task LoadMove()
        {
            try
            {
                Task LoadMoveTask = Task.Run(() =>
                {
                    lock (clickLockObject)
                    {
                        double curX = Math.Abs(AxisX.AxisObject.Status.Position.Command - AxisX.AxisObject.Status.Position.Actual) < 50 ? AxisX.AxisObject.Status.Position.Command : AxisX.AxisObject.Status.Position.Actual;
                        double curY = Math.Abs(AxisY.AxisObject.Status.Position.Command - AxisY.AxisObject.Status.Position.Actual) < 50 ? AxisY.AxisObject.Status.Position.Command : AxisY.AxisObject.Status.Position.Actual;
                        double curZ = Math.Abs(AxisZ.AxisObject.Status.Position.Command - AxisZ.AxisObject.Status.Position.Actual) < 50 ? AxisZ.AxisObject.Status.Position.Command : AxisZ.AxisObject.Status.Position.Actual;
                        double curT = Math.Abs(AxisT.AxisObject.Status.Position.Command - AxisT.AxisObject.Status.Position.Actual) < 50 ? AxisT.AxisObject.Status.Position.Command : AxisT.AxisObject.Status.Position.Actual;
                        double curPZ = Math.Abs(AxisPZ.AxisObject.Status.Position.Command - AxisPZ.AxisObject.Status.Position.Actual) < 50 ? AxisPZ.AxisObject.Status.Position.Command : AxisPZ.AxisObject.Status.Position.Actual;
                        double targetX = relX + curX;
                        double targetY = relY + curY;
                        double targetZ = relZ + curZ;
                        double targetT = relT + curT;
                        double targetPZ = relPZ + curPZ;

                        if (Math.Abs(relX) > 0.001 && targetX < AxisX.AxisObject.Param.PosSWLimit.Value && targetX > AxisX.AxisObject.Param.NegSWLimit.Value && AxisX.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            if (AxisX.AxisObject.Status.IsHomeSeted)
                            {
                                AxisX.IsEnableNegButton = false;
                                AxisX.Provider.RelMove(AxisX.AxisObject, relX, AxisX.AxisObject.Param.Speed.Value, AxisX.AxisObject.Param.Acceleration.Value);
                            }
                        }
                        if (Math.Abs(relY) > 0.001 && targetY < AxisY.AxisObject.Param.PosSWLimit.Value && targetY > AxisY.AxisObject.Param.NegSWLimit.Value && AxisY.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            if (AxisY.AxisObject.Status.IsHomeSeted)
                            {
                                AxisY.IsEnableNegButton = false;
                                AxisY.Provider.RelMove(AxisY.AxisObject, relY, AxisY.AxisObject.Param.Speed.Value, AxisY.AxisObject.Param.Acceleration.Value);
                            }
                        }
                        if (Math.Abs(relZ) > 0.001 && targetZ < AxisZ.AxisObject.Param.PosSWLimit.Value && targetZ > AxisZ.AxisObject.Param.NegSWLimit.Value && AxisZ.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            if (AxisZ.AxisObject.Status.IsHomeSeted)
                            {
                                AxisZ.IsEnableNegButton = false;
                                AxisZ.Provider.RelMove(AxisZ.AxisObject, relZ, AxisZ.AxisObject.Param.Speed.Value, AxisZ.AxisObject.Param.Acceleration.Value);
                            }
                        }
                        if (Math.Abs(relT) > 0.001 && targetT < AxisT.AxisObject.Param.PosSWLimit.Value && targetT > AxisT.AxisObject.Param.NegSWLimit.Value && AxisT.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            if (AxisT.AxisObject.Status.IsHomeSeted)
                            {
                                AxisT.IsEnableNegButton = false;
                                AxisT.Provider.RelMove(AxisT.AxisObject, relT, AxisT.AxisObject.Param.Speed.Value, AxisT.AxisObject.Param.Acceleration.Value);
                            }
                        }
                        if (Math.Abs(relPZ) > 0.001 && targetPZ < AxisPZ.AxisObject.Param.PosSWLimit.Value && targetPZ > AxisPZ.AxisObject.Param.NegSWLimit.Value && AxisPZ.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            if (AxisPZ.AxisObject.Status.IsHomeSeted)
                            {
                                AxisPZ.IsEnableNegButton = false;
                                AxisPZ.Provider.RelMove(AxisPZ.AxisObject, relPZ, AxisPZ.AxisObject.Param.Speed.Value, AxisPZ.AxisObject.Param.Acceleration.Value);
                            }
                        }
                        if (!AxisPZ.IsEnableNegButton) AxisPZ.Provider.WaitForAxisMotionDone(AxisPZ.AxisObject);
                        if (!AxisT.IsEnableNegButton) AxisT.Provider.WaitForAxisMotionDone(AxisT.AxisObject);
                        if (!AxisZ.IsEnableNegButton) AxisZ.Provider.WaitForAxisMotionDone(AxisZ.AxisObject);
                        if (!AxisY.IsEnableNegButton) AxisY.Provider.WaitForAxisMotionDone(AxisY.AxisObject);
                        if (!AxisX.IsEnableNegButton) AxisX.Provider.WaitForAxisMotionDone(AxisX.AxisObject);
                    }
                });
                await LoadMoveTask;

                AxisX.IsEnableNegButton = true;
                AxisY.IsEnableNegButton = true;
                AxisZ.IsEnableNegButton = true;
                AxisT.IsEnableNegButton = true;
                AxisPZ.IsEnableNegButton = true;
                relX = relY = relZ = relT = relPZ = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task AbsMove()
        {
            try
            {
                Task AbsMoveTask = Task.Run(() =>
                {
                    lock (clickLockObject)
                    {
                        double targetX = AxisX.AxisObject.Status.Position.Ref;
                        double targetY = AxisY.AxisObject.Status.Position.Ref;
                        double targetZ = AxisZ.AxisObject.Status.Position.Ref;
                        double targetT = AxisT.AxisObject.Status.Position.Ref;
                        double targetPZ = AxisPZ.AxisObject.Status.Position.Ref;

                        relX = targetX - AxisX.AxisObject.Status.Position.Command;
                        relY = targetY - AxisY.AxisObject.Status.Position.Command;
                        relZ = targetZ - AxisZ.AxisObject.Status.Position.Command;
                        relT = targetT - AxisT.AxisObject.Status.Position.Command;
                        relPZ = targetPZ - AxisPZ.AxisObject.Status.Position.Command;

                        if (Math.Abs(relX) > 0.5 && targetX < AxisX.AxisObject.Param.PosSWLimit.Value && targetX > AxisX.AxisObject.Param.NegSWLimit.Value && AxisX.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            AxisX.IsEnableNegButton = false;
                            AxisX.Provider.AbsMove(AxisX.AxisObject, targetX, AxisX.AxisObject.Param.Speed.Value, AxisX.AxisObject.Param.Acceleration.Value);
                        }
                        if (Math.Abs(relY) > 0.5 && targetY < AxisY.AxisObject.Param.PosSWLimit.Value && targetY > AxisY.AxisObject.Param.NegSWLimit.Value && AxisY.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            AxisY.IsEnableNegButton = false;
                            AxisY.Provider.AbsMove(AxisY.AxisObject, targetY, AxisY.AxisObject.Param.Speed.Value, AxisY.AxisObject.Param.Acceleration.Value);
                        }
                        if (Math.Abs(relZ) > 0.5 && targetZ < AxisZ.AxisObject.Param.PosSWLimit.Value && targetZ > AxisZ.AxisObject.Param.NegSWLimit.Value && AxisZ.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            AxisZ.IsEnableNegButton = false;
                            AxisZ.Provider.AbsMove(AxisZ.AxisObject, targetZ, AxisZ.AxisObject.Param.Speed.Value, AxisZ.AxisObject.Param.Acceleration.Value);
                        }
                        if (Math.Abs(relT) > 0.5 && targetT < AxisT.AxisObject.Param.PosSWLimit.Value && targetT > AxisT.AxisObject.Param.NegSWLimit.Value && AxisT.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            AxisT.IsEnableNegButton = false;
                            AxisT.Provider.AbsMove(AxisT.AxisObject, targetT, AxisT.AxisObject.Param.Speed.Value, AxisT.AxisObject.Param.Acceleration.Value);
                        }
                        if (Math.Abs(relPZ) > 0.5 && targetPZ < AxisPZ.AxisObject.Param.PosSWLimit.Value && targetPZ > AxisPZ.AxisObject.Param.NegSWLimit.Value && AxisPZ.AxisObject.Status.State != EnumAxisState.DISABLED)
                        {
                            AxisPZ.IsEnableNegButton = false;
                            AxisPZ.Provider.AbsMove(AxisPZ.AxisObject, targetPZ, AxisPZ.AxisObject.Param.Speed.Value, AxisPZ.AxisObject.Param.Acceleration.Value);
                        }
                        if (!AxisPZ.IsEnableNegButton) AxisPZ.Provider.WaitForAxisMotionDone(AxisPZ.AxisObject);
                        if (!AxisT.IsEnableNegButton) AxisT.Provider.WaitForAxisMotionDone(AxisT.AxisObject);
                        if (!AxisZ.IsEnableNegButton) AxisZ.Provider.WaitForAxisMotionDone(AxisZ.AxisObject);
                        if (!AxisX.IsEnableNegButton) AxisX.Provider.WaitForAxisMotionDone(AxisX.AxisObject);
                        if (!AxisY.IsEnableNegButton) AxisY.Provider.WaitForAxisMotionDone(AxisY.AxisObject);
                    }
                });
                await AbsMoveTask;

                relX = relY = relZ = relT = relPZ = 0;

                AxisX.IsEnableNegButton = true;
                AxisY.IsEnableNegButton = true;
                AxisZ.IsEnableNegButton = true;
                AxisT.IsEnableNegButton = true;
                AxisPZ.IsEnableNegButton = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _BtnSaveCommand;
        public ICommand BtnSaveCommand
        {
            get
            {
                if (null == _BtnSaveCommand) _BtnSaveCommand = new AsyncCommand(BtnSave);
                return _BtnSaveCommand;
            }
        }
        private async Task BtnSave()
        {
            try
            {
                ICamera ACam = DisplayPort.AssignedCamera;
                int coaxLight = ACam.GetLight(EnumLightType.COAXIAL);
                int oblqLight = ACam.GetLight(EnumLightType.OBLIQUE);
                await Task.Run(() =>
                {
                    ImageBuffer imgBuff = null;
                    ACam.GetCurImage(out imgBuff);
                    string imgPath = LoggerManager.GetLogDirPath(EnumLoggerType.INFO) + $"\\{ACam.GetChannelType().ToString()}_C{coaxLight}_O{oblqLight}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.bmp";
                    this.VisionManager().SaveImageBuffer(imgBuff, imgPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _FocusCommand;
        public ICommand FocusCommand
        {
            get
            {
                if (null == _FocusCommand) _FocusCommand = new AsyncCommand(Focusing);
                return _FocusCommand;
            }
        }

        private EnumProberCam _CurrCam;
        public EnumProberCam CurrCam
        {
            get
            {
                return _CurrCam;
            }
            set
            {
                if (_CurrCam != value)
                {
                    _CurrCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private async Task Focusing()
        {
            try
            {
                await Task.Run(() =>
                {
                    IFocusing focuser = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());
                    var focusingParam = new NormalFocusParameter();

                    AxisX.AxisObject.Status.Position.Ref = AxisX.AxisObject.Status.Position.Command;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AxisObject.Status.Position.Command;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AxisObject.Status.Position.Command;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Status.Position.Command;

                    focusingParam.SetDefaultParam();
                    focusingParam.FocusRange.Value = CurrCam == EnumProberCam.WAFER_HIGH_CAM ? 50 : 300;
                    focusingParam.FocusingCam.Value = CurrCam;
                    focusingParam.FocusingROI.Value = new System.Windows.Rect(480, 480, 200, 200);
                    if (focusingParam.FocusingCam.Value == EnumProberCam.PIN_HIGH_CAM || focusingParam.FocusingCam.Value == EnumProberCam.PIN_LOW_CAM)
                    {
                        focusingParam.FocusingAxis = AxisPZ.AxisObject.AxisType;
                    }
                    else
                    {
                        focusingParam.FocusingAxis = AxisZ.AxisObject.AxisType;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        EventCodeEnum ecode = focuser.Focusing_Retry(focusingParam, false, false, false, this);
                        Debug.WriteLine($"Focusing Result: {ecode}, {focusingParam.FocusResultPos}");
                        if (ecode == EventCodeEnum.NONE)
                        {
                            var Axis = this.MotionManager().GetAxis(focusingParam.FocusingAxis.Value);
                            Axis.Status.Position.Ref = focusingParam.FocusResultPos;
                            break;
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private Brush _MarkerLightBackground;
        public Brush MarkerLightBackground
        {
            get
            {
                return _MarkerLightBackground;
            }
            set
            {
                if (_MarkerLightBackground != value)
                {
                    _MarkerLightBackground = value;
                    RaisePropertyChanged();
                }
            }

        }
        private AsyncCommand _MarkerLightCommand;
        public ICommand MarkerLightCommand
        {
            get
            {
                if (null == _MarkerLightCommand) _MarkerLightCommand = new AsyncCommand(MarkerLight);
                return _MarkerLightCommand;
            }
        }

        private async Task MarkerLight()
        {
            try
            {
                await Task.Run(() =>
                {
                    int lt = PHCam.GetLight(EnumLightType.AUX);
                    if (lt != 0)
                    {
                        PHCam.SetLight(EnumLightType.AUX, 0);
                        MarkerLightBackground = null;
                    }
                    else
                    {
                        PHCam.SetLight(EnumLightType.AUX, (ushort)((CurrCam == EnumProberCam.WAFER_LOW_CAM) ? 24 : 64));
                        MarkerLightBackground = _BtnSetRed;
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private AsyncCommand _MarkerFocusCommand;
        public ICommand MarkerFocusCommand
        {
            get
            {
                if (null == _MarkerFocusCommand) _MarkerFocusCommand = new AsyncCommand(MarkerFocusing);
                return _MarkerFocusCommand;
            }
        }

        private async Task MarkerFocusing()
        {
            try
            {
                await Task.Run(() =>
                {
                    IFocusing focuser = this.FocusManager().GetFocusingModel(FocusingDLLInfo.GetNomalFocusingDllInfo());
                    var focusingParam = new NormalFocusParameter();

                    AxisX.AxisObject.Status.Position.Ref = AxisX.AxisObject.Status.Position.Command;
                    AxisY.AxisObject.Status.Position.Ref = AxisY.AxisObject.Status.Position.Command;
                    AxisZ.AxisObject.Status.Position.Ref = AxisZ.AxisObject.Status.Position.Command;
                    AxisT.AxisObject.Status.Position.Ref = AxisT.AxisObject.Status.Position.Command;
                    AxisPZ.AxisObject.Status.Position.Ref = AxisPZ.AxisObject.Status.Position.Command;

                    focusingParam.SetDefaultParam();
                    focusingParam.FocusRange.Value = 100;
                    focusingParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                    focusingParam.FocusingROI.Value = new System.Windows.Rect(480, 480, 200, 200);
                    focusingParam.FocusingAxis = AxisPZ.AxisObject.AxisType;

                    for (int i = 0; i < 3; i++)
                    {
                        EventCodeEnum ecode = focuser.Focusing_Retry(focusingParam, false, false, false, this);
                        Debug.WriteLine($"Focusing Result: {ecode}, {focusingParam.FocusResultPos}");
                        if (ecode == EventCodeEnum.NONE)
                        {
                            var Axis = this.MotionManager().GetAxis(focusingParam.FocusingAxis.Value);
                            Axis.Status.Position.Ref = focusingParam.FocusResultPos;
                            break;
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<object> _ClickToMoveLButtonDownCommand;
        public ICommand ClickToMoveLButtonDownCommand
        {
            get
            {
                if (null == _ClickToMoveLButtonDownCommand)
                {
                    _ClickToMoveLButtonDownCommand = new AsyncCommand<object>(ClickToMoveLButtonDown, showWaitCancel: false, showWaitMessage: "Move to clicked position");
                }

                return _ClickToMoveLButtonDownCommand;
            }
        }

        public bool EnableToMove
        {
            get { return true; }
        }

        private double cmdPosX, cmdPosY, cmdPosZ;

        private Task _ClickToMoveTask;
        private Object clickLockObject = new object();
        public async Task ClickToMoveLButtonDown(object enableClickToMove)
        {
            try
            {
                if (DisplayPort.AssignedCamera == null) return;

                bool IsEnable = (bool)enableClickToMove;
                MoveTargetPosX = this.DisplayPort.MoveXValue;
                MoveTargetPosY = this.DisplayPort.MoveYValue;
                if (CurrCam == EnumProberCam.PIN_HIGH_CAM || CurrCam == EnumProberCam.PIN_LOW_CAM)
                {
                    MoveTargetPosX = -this.DisplayPort.MoveXValue;
                    MoveTargetPosY = -this.DisplayPort.MoveYValue;
                }

                double ox = DisplayPort.StandardOverlayCanvaseWidth + 70;
                double oy = DisplayPort.StandardOverlayCanvaseHeight + 70;
                int gx = DisplayPort.AssignedCamera.Param.GrabSizeX.Value;
                int gy = DisplayPort.AssignedCamera.Param.GrabSizeY.Value;
                double rX = DisplayPort.AssignedCamera.GetRatioX();
                double rY = DisplayPort.AssignedCamera.GetRatioY();

                if (IsEnable)
                {
                    CatCoordinates coord = DisplayPort.AssignedCamera.GetCurCoordPos();

                    _ClickToMoveTask = Task.Run(() =>
                    {
                        lock (clickLockObject)
                        {
                            Debug.WriteLine($"Click to move({MoveTargetPosX},{MoveTargetPosY}) command!!");


                            double Xpos = 0, Ypos = 0;
                            this.MotionManager().GetActualPos(AxisX.AxisObject.AxisType.Value, ref Xpos);
                            this.MotionManager().GetActualPos(AxisY.AxisObject.AxisType.Value, ref Ypos);
                            Xpos += MoveTargetPosX;
                            if (Xpos < AxisX.AxisObject.Param.PosSWLimit.Value && Xpos > AxisX.AxisObject.Param.NegSWLimit.Value)
                            {
                                AxisX.IsEnableNegButton = false;
                                AxisX.Provider.RelMove(AxisX.AxisObject, MoveTargetPosX, AxisX.AxisObject.Param.Speed.Value, AxisX.AxisObject.Param.Acceleration.Value);
                            }

                            Ypos += MoveTargetPosY;
                            if (Ypos < AxisY.AxisObject.Param.PosSWLimit.Value && Ypos > AxisY.AxisObject.Param.NegSWLimit.Value)
                            {
                                AxisY.IsEnableNegButton = false;
                                AxisY.Provider.RelMove(AxisY.AxisObject, MoveTargetPosY, AxisY.AxisObject.Param.Speed.Value, AxisY.AxisObject.Param.Acceleration.Value);
                            }

                            AxisX.Provider.WaitForAxisMotionDone(AxisX.AxisObject, AxisX.AxisObject.Param.TimeOut.Value);
                            AxisY.Provider.WaitForAxisMotionDone(AxisY.AxisObject, AxisY.AxisObject.Param.TimeOut.Value);
                            cmdPosX = AxisX.AxisObject.Status.Position.Command;
                            cmdPosY = AxisY.AxisObject.Status.Position.Command;
                            cmdPosZ = AxisZ.AxisObject.Status.Position.Command;
                            Debug.WriteLine($"Postion to ({AxisX.AxisObject.Status.Position.Command},{AxisY.AxisObject.Status.Position.Command},{AxisZ.AxisObject.Status.Position.Command})");
                        }
                    });
                    await _ClickToMoveTask;

                    AxisX.IsEnableNegButton = true;
                    AxisY.IsEnableNegButton = true;
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

        private RelayCommand<object> _InitCommand;

        public ICommand InitCommand
        {
            get
            {
                if (null == _InitCommand) _InitCommand = new RelayCommand<object>(Init);
                return _InitCommand;
            }
        }

        private void Init(object obj)
        {

            // [path] C:\ProberSystem\EMUL

            //this.FileManager().PultCommandString = "[path]" + Pathlist[0].Path;
            SystemModuleCount.LoadParam();

            IFactoryModule fm = this.ParamManager() as IFactoryModule;

            SorterModuleResolver.UserModuleConstructorEvent(fm);

            this.InitModule();
        }


        private Autofac.IContainer Container;
        public EventCodeEnum SetContainer()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Container = SorterModuleResolver.ConfigureDependencies();
                this.SetContainer(Container);
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetContainer(): Error occurred. Err = {err.Message}");
            }

            return retval;
        }

        private double _MoveTargetPosX = 0.0D;
        public double MoveTargetPosX
        {
            get
            {
                return _MoveTargetPosX;
            }
            set
            {
                if (value != _MoveTargetPosX)
                {
                    _MoveTargetPosX = value;
                    RaisePropertyChanged(nameof(MoveTargetPosX));
                    Debug.WriteLine($"X:{value}");
                }
            }
        }

        private double _MoveTargetPosY = 0.0D;
        public double MoveTargetPosY
        {
            get
            {
                return _MoveTargetPosY;
            }
            set
            {
                if (value != _MoveTargetPosY)
                {
                    _MoveTargetPosY = value;
                    RaisePropertyChanged(nameof(MoveTargetPosY));
                    Debug.WriteLine($"Y:{value}");
                }
            }
        }

        private void coLightPlus()
        {
            CoaxialLight = Math.Min(255, CoaxialLight + 5);
        }
        private ICommand _CoaxialLightPlus;
        public ICommand CoaxialLightPlus
        {
            get
            {
                if (_CoaxialLightPlus == null) _CoaxialLightPlus = new RelayCommand(coLightPlus);
                return _CoaxialLightPlus;
            }
        }

        private void coLightMinus()
        {
            CoaxialLight = Math.Max(0, CoaxialLight - 5);
        }
        private ICommand _CoaxialLightMinus;
        public ICommand CoaxialLightMinus
        {
            get
            {
                if (_CoaxialLightMinus == null) _CoaxialLightMinus = new RelayCommand(coLightMinus);
                return _CoaxialLightMinus;
            }
        }

        private void obLightPlus()
        {
            ObliqueLight = Math.Min(255, ObliqueLight + 5);
        }
        private ICommand _ObliqueLightPlus;
        public ICommand ObliqueLightPlus
        {
            get
            {
                if (_ObliqueLightPlus == null) _ObliqueLightPlus = new RelayCommand(obLightPlus);
                return _ObliqueLightPlus;
            }
        }

        private void obLightMinus()
        {
            ObliqueLight = Math.Max(0, ObliqueLight - 5);
        }
        private ICommand _ObliqueLightMinus;
        public ICommand ObliqueLightMinus
        {
            get
            {
                if (_ObliqueLightMinus == null) _ObliqueLightMinus = new RelayCommand(obLightMinus);
                return _ObliqueLightMinus;
            }
        }

        public int CoaxialLight
        {
            get
            {
                if (DisplayPort == null || DisplayPort.AssignedCamera == null) return 0;
                return DisplayPort.AssignedCamera.GetLight(EnumLightType.COAXIAL);
            }
            set
            {
                if (DisplayPort != null && DisplayPort.AssignedCamera != null)
                {
                    DisplayPort.AssignedCamera.SetLight(EnumLightType.COAXIAL, (ushort)value);
                    RaisePropertyChanged();
                }
            }
        }

        public int ObliqueLight
        {
            get
            {
                if (DisplayPort == null || DisplayPort.AssignedCamera == null) return 0;
                return DisplayPort.AssignedCamera.GetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE);
            }
            set
            {
                if (DisplayPort != null && DisplayPort.AssignedCamera != null)
                {
                    DisplayPort.AssignedCamera.SetLight(DisplayPort.AssignedCamera == PHCam ? EnumLightType.AUX : EnumLightType.OBLIQUE, (ushort)value);
                    RaisePropertyChanged();
                }
            }
        }

        private bool bStop = false;
        private int _RunCount = 1;
        public int RunCount
        {
            get
            {
                return _RunCount;
            }
            set
            {
                if (_RunCount != value)
                {
                    _RunCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _BtnRunActionCommand;
        public ICommand BtnRunActionCommand
        {
            get
            {
                if (null == _BtnRunActionCommand) _BtnRunActionCommand = new AsyncCommand(BtnRunAction);
                return _BtnRunActionCommand;
            }
        }
        private async Task BtnRunAction()
        {
            try
            {
                while (RunCount > 0)
                {
                    if (bStop) break;

                    await BtnGoDiePickAction();
                    if (bStop) break;
                    await Task.Run(() => { Thread.Sleep(500); });

                    if (bStop) break;
                    await BtnPickDieAction();
                    await Task.Run(() => { Thread.Sleep(1000); });
                    await BtnPlaceDieAction();
                    if (bStop) break;
                    await Task.Run(() => { Thread.Sleep(1000); });
                    if (bStop) break;

                    await BtnGoDieAction();
                    if (bStop) break;
                    await Task.Run(() => { Thread.Sleep(2500); });

                    await BtnSave();
                    if (bStop) break;
                    Thread.Sleep(500);

                    RunCount--;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _BtnStopActionCommand;
        public ICommand BtnStopActionCommand
        {
            get
            {
                if (null == _BtnStopActionCommand) _BtnStopActionCommand = new AsyncCommand(BtnStopAction);
                return _BtnStopActionCommand;
            }
        }
        private async Task BtnStopAction()
        {
            try
            {
                bStop = true;
                await Task.Run(() => { });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _BtnResetActionCommand;
        public ICommand BtnResetActionCommand
        {
            get
            {
                if (null == _BtnResetActionCommand) _BtnResetActionCommand = new AsyncCommand(BtnResetAction);
                return _BtnResetActionCommand;
            }
        }
        private async Task BtnResetAction()
        {
            try
            {
                bStop = false;
                await Task.Run(() => { });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
