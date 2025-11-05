using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ProberErrorCode;
using RelayCommandBase;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Win32;
using LoaderParameters;
using System.Runtime.CompilerServices;
using ProberInterfaces.Foup;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Command;
using LogModule;
using UcDisplayPort;
using System.Windows.Data;
using System.Windows;
using MetroDialogInterfaces;
using LoaderControllerBase;
using ProberInterfaces.State;
//using ProberInterfaces.ThreadSync;

namespace LoaderSetupViewModel
{
    public enum enmLoaderCamType
    {
        UNDEFINED = 0,

        PACL6_CAM,
        PACL8_CAM,
        PACL12_CAM,
        ARM_6_CAM,
        ARM_8_12_CAM,
        OCR1_CAM,
        OCR2_CAM,
    }
    public class LoaderCam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public enmLoaderCamType CamType { get; set; }
        public LoaderCam(enmLoaderCamType camtype)
        {
            CamType = camtype;
        }
        public override string ToString()
        {
            return CamType.ToString();
        }
    }
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


        private double _RelMoveStepDist;
        public double RelMoveStepDist
        {
            get { return _RelMoveStepDist; }
            set
            {
                if (value != _RelMoveStepDist)
                {
                    _RelMoveStepDist = value;
                    NotifyPropertyChanged("RelMoveStepDist");
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
                    NotifyPropertyChanged("PosButtonVisibility");
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
                    NotifyPropertyChanged("NegButtonVisibility");
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
                    NotifyPropertyChanged("AxisObject");
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
                        this.MotionManager().RelMove(AxisObject, pos,
                            AxisObject.Param.Speed.Value,
                            AxisObject.Param.Acceleration.Value);
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
                        this.MotionManager().RelMove(AxisObject, pos,
                            AxisObject.Param.Speed.Value,
                            AxisObject.Param.Acceleration.Value);
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }


    public class LoaderSetupViewModelBase : IMainScreenViewModel, INotifyPropertyChanged, ISetUpState
    {
        readonly Guid _ViewModelGUID = new Guid("6F439E21-C584-9FBC-5D01-1D7AEE29F665");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public bool Initialized { get; set; } = false;

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
        private enmLoaderCamType _SelectedCam;
        public enmLoaderCamType SelectedCam
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

        private ObservableCollection<LoaderCam> _LoaderCamList
           = new ObservableCollection<LoaderCam>();
        public ObservableCollection<LoaderCam> LoaderCamList
        {
            get { return _LoaderCamList; }
            set
            {
                if (value != _LoaderCamList)
                {
                    _LoaderCamList = value;
                    RaisePropertyChanged();
                }
            }
        }


        private LoaderSystemParameter _LoaderSystemParam;

        public LoaderSystemParameter LoaderSystemParam
        {
            get { return _LoaderSystemParam; }
            set
            {
                if (value != _LoaderSystemParam)
                {
                    _LoaderSystemParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoaderDeviceParameter _LoaderDeviceParam;

        public LoaderDeviceParameter LoaderDeviceParam
        {
            get { return _LoaderDeviceParam; }
            set
            {
                if (value != _LoaderDeviceParam)
                {
                    _LoaderDeviceParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region Property

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

        private DisplayPort _CameraDisplayPort;
        public DisplayPort CameraDisplayPort
        {
            get { return _CameraDisplayPort; }
            set
            {
                if (value != _CameraDisplayPort)
                {
                    _CameraDisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        //private LockKey outPortLock = new LockKey("Loader setup VM - out port");
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

        //private LockKey inPortLock = new LockKey("Loader setup VM - in port");
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

        ILightAdmin light;

        #endregion

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
                if (Initialized == false)
                {
                    LoaderSystemParam = new LoaderSystemParameter();
                    LoaderDeviceParam = new LoaderDeviceParameter();
                    VisionManager = this.VisionManager();
                    MotionManager = this.MotionManager();
                    StageSupervisor = this.StageSupervisor();

                    CommandManager = this.CommandManager();
                    LoaderController = this.LoaderController() as ILoaderControllerExtension;

                    if(this.MotionManager() != null)
                    {
                        LoaderAxes aes = this.MotionManager().LoaderAxes;

                        foreach (var item in aes.ProbeAxisProviders)
                        {
                            var axisObjVM = new AxisObjectVM();
                            axisObjVM.AxisObject = item;

                            LoaderAxisObjectVmList.Add(axisObjVM);
                        }
                    }

                    LoaderCamList = new ObservableCollection<LoaderCam>();
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.PACL6_CAM));
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.PACL8_CAM));
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.PACL12_CAM));
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.ARM_6_CAM));
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.ARM_8_12_CAM));
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.OCR1_CAM));
                    LoaderCamList.Add(new LoaderCam(enmLoaderCamType.OCR2_CAM));

                    PropertyInfo[] propertyInfos;
                    IOPortDescripter<bool> port;
                    object propObject;

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
                vm.SwitchCamera(this.VisionManager().GetCam(SelectedChannel.Type).Param, this);
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

        public Task<EventCodeEnum> InitViewModel(object parameter = null)
        {
            //(this.LoaderController() as LoaderController.LoaderController).RetractAll();

            Task<EventCodeEnum> task = null;

            try
            {
                task = Task.Run(() =>
                {
                    return EventCodeEnum.NONE;
                });

                task.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return task;
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            Task<EventCodeEnum> task = null;
            try
            {

                (this.LoaderController() as LoaderController.LoaderController).RetractAll();

                task = Task.Run(() =>
                {
                    return EventCodeEnum.NONE;
                });
                task.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return task;
        }

        #region CamCommand

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


        private ICommandManager _CommandManager;
        public ICommandManager CommandManager
        {
            get { return _CommandManager; }
            set
            {
                if (value != _CommandManager)
                {
                    _CommandManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ScanCstCommand;
        public ICommand ScanCstCommand
        {
            get
            {
                if (null == _ScanCstCommand) _ScanCstCommand = new AsyncCommand(ScanCstFunc);
                return _ScanCstCommand;

            }
        }
        private async Task ScanCstFunc()
        {
            try
            {
                var cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();
                var foupController = this.FoupOpModule().GetFoupController(cassette.ID.Index);

                if (foupController.FoupModuleInfo.State == FoupStateEnum.UNLOAD)
                {
                    EventCodeEnum retVal = foupController.Execute(new FoupLoadCommand() { });
                }

                if (foupController.FoupModuleInfo.State != FoupStateEnum.LOAD)
                {
                    string caption = "ERROR";
                    string message = $"Foup state invalid. state={foupController.FoupModuleInfo.State}";

                    var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                    return;
                }

                if (cassette != null)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        var editor = LoaderController.GetLoaderMapEditor();
                        editor.EditorState.SetScanCassette(cassette.ID);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);

                        //if (isInjected)
                        //{
                        //    await ProcessDialog.ShowDialog("LOADER", $"Scan Cassette : target={cassette.ID}");

                        //    await Task.Run(() =>
                        //    {
                        //        EventCodeEnum retVal;
                        //        retVal = LoaderController.WaitForCommandDone();
                        //    });

                        //    await ProcessDialog.CloseDialg();
                        //}
                    }
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
                //this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                this.VisionManager().StopGrab(EnumProberCam.WAFER_LOW_CAM);
                this.VisionManager().StopGrab(EnumProberCam.WAFER_HIGH_CAM);
                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);
                this.VisionManager().StopGrab(EnumProberCam.PIN_HIGH_CAM);

                switch (SelectedCam)
                {
                    case enmLoaderCamType.UNDEFINED:
                        curcam = EnumProberCam.UNDEFINED;
                        break;
                    case enmLoaderCamType.PACL6_CAM:
                        curcam = EnumProberCam.PACL6_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PACL6_CAM);
                        break;
                    case enmLoaderCamType.PACL8_CAM:
                        curcam = EnumProberCam.PACL8_CAM;
                        this.VisionManager().GetCam(EnumProberCam.PACL8_CAM).SetLight(EnumLightType.AUX, 255);
                        Cam = this.VisionManager().GetCam(EnumProberCam.PACL8_CAM);
                        break;
                    case enmLoaderCamType.PACL12_CAM:
                        curcam = EnumProberCam.PACL12_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.PACL12_CAM);
                        break;
                    case enmLoaderCamType.ARM_6_CAM:
                        curcam = EnumProberCam.ARM_6_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.ARM_6_CAM);
                        break;
                    case enmLoaderCamType.ARM_8_12_CAM:
                        curcam = EnumProberCam.ARM_8_12_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.ARM_8_12_CAM);
                        break;
                    case enmLoaderCamType.OCR1_CAM:
                        curcam = EnumProberCam.OCR1_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.OCR1_CAM);
                        break;
                    case enmLoaderCamType.OCR2_CAM:
                        curcam = EnumProberCam.OCR2_CAM;
                        Cam = this.VisionManager().GetCam(EnumProberCam.OCR2_CAM);
                        break;
                    default:
                        break;
                }

                this.VisionManager().StartGrab(curcam, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region Treeview Command

        private RelayCommand<object> _LoadSysFileCommand;
        public ICommand LoadSysFileCommand
        {
            get
            {
                if (null == _LoadSysFileCommand) _LoadSysFileCommand = new RelayCommand<object>(LoadSysFile);
                return _LoadSysFileCommand;
            }
        }
        private void LoadSysFile(object noparam)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "JSON Files(*.json)|*.json";
                dlg.DefaultExt = ".json";

                var rel = dlg.ShowDialog();
                if (rel == true)
                {
                    EventCodeEnum retVal = EventCodeEnum.NONE;
                    IParam tmpParam = null;
                    retVal = this.LoadParameter(ref tmpParam, typeof(LoaderSystemParameter), null, dlg.FileName);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception("[LoaderSetupViewModelBase] Faile LoadLoaderControllerParameter.");
                    }
                    else
                    {
                        this.LoaderSystemParam = tmpParam as LoaderSystemParameter;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _NewSysParamCommand;
        public ICommand NewSysParamCommand
        {
            get
            {
                if (null == _NewSysParamCommand) _NewSysParamCommand = new RelayCommand<object>(NewSysParam);
                return _NewSysParamCommand;
            }
        }
        private void NewSysParam(object noparam)
        {
            try
            {
                LoaderSystemParameter tmpparam = new LoaderSystemParameter();
                tmpparam.SetDefalutParam();
                this.LoaderSystemParam = tmpparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SaveSysParamCommand;
        public ICommand SaveSysParamCommand
        {
            get
            {
                if (null == _SaveSysParamCommand) _SaveSysParamCommand = new RelayCommand<object>(SaveSysParam);
                return _SaveSysParamCommand;
            }
        }
        private void SaveSysParam(object noparam)
        {

            try
            {
                this.LightAdmin().SetLight(15, 0);
                //  (this.LoaderController() as LoaderController.LoaderController).UpdateSystemParam(this.LoaderSystemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _LoadDevFileCommand;
        public ICommand LoadDevFileCommand
        {
            get
            {
                if (null == _LoadDevFileCommand) _LoadDevFileCommand = new RelayCommand<object>(LoadDevFile);
                return _LoadDevFileCommand;
            }
        }
        private void LoadDevFile(object noparam)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "JSON Files(*.json)|*.json";
                dlg.DefaultExt = ".json";

                var rel = dlg.ShowDialog();
                if (rel == true)
                {
                    EventCodeEnum retVal = EventCodeEnum.NONE;
                    IParam tmpParam = null;
                    retVal = this.LoadParameter(ref tmpParam, typeof(LoaderDeviceParameter), null, dlg.FileName);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception("[LoaderSetupViewModelBase] Faile LoadLoaderControllerParameter.");
                    }
                    else
                    {
                        this.LoaderDeviceParam = tmpParam as LoaderDeviceParameter;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _NewDevParamCommand;
        public ICommand NewDevParamCommand
        {
            get
            {
                if (null == _NewDevParamCommand) _NewDevParamCommand = new RelayCommand<object>(NewDevParam);
                return _NewDevParamCommand;
            }
        }
        private void NewDevParam(object noparam)
        {
            try
            {
                LoaderDeviceParameter tmpparam = new LoaderDeviceParameter();
                tmpparam.SetDefaultParam();
                this.LoaderDeviceParam = tmpparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _SaveDevParamCommand;
        public ICommand SaveDevParamCommand
        {
            get
            {
                if (null == _SaveDevParamCommand) _SaveDevParamCommand = new RelayCommand<object>(SaveDevParam);
                return _SaveDevParamCommand;
            }
        }
        private void SaveDevParam(object noparam)
        {

            try
            {
                //this.LightAdmin().SetLightNoLUT(15, 255);
                this.LightAdmin().SetLight(15, 255);
                //(this.LoaderController() as LoaderController.LoaderController).UpdateDeviceParam(this.LoaderDeviceParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        #endregion

        #region OneClickPosCOmmnad

        private bool _U1Enable;
        public bool U1Enable
        {
            get { return _U1Enable; }
            set
            {
                if (value != _U1Enable)
                {
                    _U1Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _SetU1EnableCommand;
        public ICommand SetU1EnableCommand
        {
            get
            {
                if (null == _SetU1EnableCommand) _SetU1EnableCommand = new RelayCommand<object>(SetU1Enable);
                return _SetU1EnableCommand;
            }
        }
        private void SetU1Enable(object noparam)
        {
            try
            {
                var axisu1 = this.MotionManager.GetAxis(EnumAxisConstants.U1);
                this.MotionManager().EnableAxis(axisu1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetU1DisableCommand;
        public ICommand SetU1DisableCommand
        {
            get
            {
                if (null == _SetU1DisableCommand) _SetU1DisableCommand = new RelayCommand<object>(SetU1Disable);
                return _SetU1DisableCommand;
            }
        }
        private void SetU1Disable(object noparam)
        {
            try
            {
                var axisu1 = this.MotionManager.GetAxis(EnumAxisConstants.U1);
                this.MotionManager().DisableAxis(axisu1);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _U2Enable;
        public bool U2Enable
        {
            get { return _U2Enable; }
            set
            {
                if (value != _U2Enable)
                {
                    _U2Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _SetU2EnableCommand;
        public ICommand SetU2EnableCommand
        {
            get
            {
                if (null == _SetU2EnableCommand) _SetU2EnableCommand = new RelayCommand<object>(SetU2Enable);
                return _SetU2EnableCommand;
            }
        }
        private void SetU2Enable(object noparam)
        {
            try
            {
                var axisu2 = this.MotionManager.GetAxis(EnumAxisConstants.U2);
                this.MotionManager().EnableAxis(axisu2);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SetU2DisableCommand;
        public ICommand SetU2DisableCommand
        {
            get
            {
                if (null == _SetU2DisableCommand) _SetU2DisableCommand = new RelayCommand<object>(SetU2Disable);
                return _SetU2DisableCommand;
            }
        }
        private void SetU2Disable(object noparam)
        {
            try
            {
                var axisu2 = this.MotionManager.GetAxis(EnumAxisConstants.U2);
                this.MotionManager().DisableAxis(axisu2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _UAxisSkip;
        public bool UAxisSkip
        {
            get { return _UAxisSkip; }
            set
            {
                if (value != _UAxisSkip)
                {
                    _UAxisSkip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _SetUSkipTrueValueCommand;
        public ICommand SetUSkipTrueValueCommand
        {
            get
            {
                if (null == _SetUSkipTrueValueCommand) _SetUSkipTrueValueCommand = new RelayCommand<object>(SetUSkipTrueValue);
                return _SetUSkipTrueValueCommand;
            }
        }
        private void SetUSkipTrueValue(object noparam)
        {
            UAxisSkip = true;
        }

        private RelayCommand<object> _SetUSkipFalseValueCommand;
        public ICommand SetUSkipFalseValueCommand
        {
            get
            {
                if (null == _SetUSkipFalseValueCommand) _SetUSkipFalseValueCommand = new RelayCommand<object>(SetUSkipFalseValue);
                return _SetUSkipFalseValueCommand;
            }
        }
        private void SetUSkipFalseValue(object noparam)
        {
            UAxisSkip = false;
        }


        private double _CSTSlot1A;
        public double CSTSlot1A
        {
            get { return _CSTSlot1A; }
            set
            {
                if (value != _CSTSlot1A)
                {
                    _CSTSlot1A = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CSTSlot1U;
        public double CSTSlot1U
        {
            get { return _CSTSlot1U; }
            set
            {
                if (value != _CSTSlot1U)
                {
                    _CSTSlot1U = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CSTSlot1W;
        public double CSTSlot1W
        {
            get { return _CSTSlot1W; }
            set
            {
                if (value != _CSTSlot1W)
                {
                    _CSTSlot1W = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CSTSlot1E;
        public double CSTSlot1E
        {
            get { return _CSTSlot1E; }
            set
            {
                if (value != _CSTSlot1E)
                {
                    _CSTSlot1E = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ToCstSlot1Command;
        public ICommand ToCstSlot1Command
        {
            get
            {
                if (null == _ToCstSlot1Command) _ToCstSlot1Command = new AsyncCommand(ToCstSlot1);
                return _ToCstSlot1Command;
            }
        }
        private async Task ToCstSlot1()
        {
            try
            {
                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.CST, UAxisSkip, 1, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private double _ToPAA;
        public double ToPAA
        {
            get { return _ToPAA; }
            set
            {
                if (value != _ToPAA)
                {
                    _ToPAA = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToPAU;
        public double ToPAU
        {
            get { return _ToPAU; }
            set
            {
                if (value != _ToPAU)
                {
                    _ToPAU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToPAW;
        public double ToPAW
        {
            get { return _ToPAW; }
            set
            {
                if (value != _ToPAW)
                {
                    _ToPAW = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToPAE;
        public double ToPAE
        {
            get { return _ToPAE; }
            set
            {
                if (value != _ToPAE)
                {
                    _ToPAE = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ToPACommand;
        public ICommand ToPACommand
        {
            get
            {
                if (null == _ToPACommand) _ToPACommand = new AsyncCommand(ToPA);
                return _ToPACommand;
            }
        }
        private async Task ToPA()
        {
            try
            {
                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.PA, UAxisSkip, CstNum, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private double _ToSemicsOCRA;
        public double ToSemicsOCRA
        {
            get { return _ToSemicsOCRA; }
            set
            {
                if (value != _ToSemicsOCRA)
                {
                    _ToSemicsOCRA = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToSemicsOCRU;
        public double ToSemicsOCRU
        {
            get { return _ToSemicsOCRU; }
            set
            {
                if (value != _ToSemicsOCRU)
                {
                    _ToSemicsOCRU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToSemicsOCRW;
        public double ToSemicsOCRW
        {
            get { return _ToSemicsOCRW; }
            set
            {
                if (value != _ToSemicsOCRW)
                {
                    _ToSemicsOCRW = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToSemicsOCRE;
        public double ToSemicsOCRE
        {
            get { return _ToSemicsOCRE; }
            set
            {
                if (value != _ToSemicsOCRE)
                {
                    _ToSemicsOCRE = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ToSemicsOCRCommand;
        public ICommand ToSemicsOCRCommand
        {
            get
            {
                if (null == _ToSemicsOCRCommand) _ToSemicsOCRCommand = new AsyncCommand(ToSemicsOCR);
                return _ToSemicsOCRCommand;
            }
        }
        private async Task ToSemicsOCR()
        {
            try
            {
                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.SEMICSOCR, UAxisSkip, CstNum, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private double _ToChuckA;
        public double ToChuckA
        {
            get { return _ToChuckA; }
            set
            {
                if (value != _ToChuckA)
                {
                    _ToChuckA = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToChuckU;
        public double ToChuckU
        {
            get { return _ToChuckU; }
            set
            {
                if (value != _ToChuckU)
                {
                    _ToChuckU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToChuckW;
        public double ToChuckW
        {
            get { return _ToChuckW; }
            set
            {
                if (value != _ToChuckW)
                {
                    _ToChuckW = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToChuckE;
        public double ToChuckE
        {
            get { return _ToChuckE; }
            set
            {
                if (value != _ToChuckE)
                {
                    _ToChuckE = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ToChuckCommand;
        public ICommand ToChuckCommand
        {
            get
            {
                if (null == _ToChuckCommand) _ToChuckCommand = new AsyncCommand(ToChuck);
                return _ToChuckCommand;
            }
        }
        private async Task ToChuck()
        {
            try
            {
                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.CHUCK, UAxisSkip, CstNum, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private double _ToFixedTrayA;
        public double ToFixedTrayA
        {
            get { return _ToFixedTrayA; }
            set
            {
                if (value != _ToFixedTrayA)
                {
                    _ToFixedTrayA = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToFixedTrayU;
        public double ToFixedTrayU
        {
            get { return _ToFixedTrayU; }
            set
            {
                if (value != _ToFixedTrayU)
                {
                    _ToFixedTrayU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToFixedTrayW;
        public double ToFixedTrayW
        {
            get { return _ToFixedTrayW; }
            set
            {
                if (value != _ToFixedTrayW)
                {
                    _ToFixedTrayW = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToFixedTrayE;
        public double ToFixedTrayE
        {
            get { return _ToFixedTrayE; }
            set
            {
                if (value != _ToFixedTrayE)
                {
                    _ToFixedTrayE = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ToFixedTrayCommand;
        public ICommand ToFixedTrayCommand
        {
            get
            {
                if (null == _ToFixedTrayCommand) _ToFixedTrayCommand = new AsyncCommand(ToFixedTray);
                return _ToFixedTrayCommand;
            }
        }
        private async Task ToFixedTray()
        {
            try
            {
                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.FIXEDTRAY, UAxisSkip, CstNum, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private double _ToInspectionTrayA;
        public double ToInspectionTrayA
        {
            get { return _ToInspectionTrayA; }
            set
            {
                if (value != _ToInspectionTrayA)
                {
                    _ToInspectionTrayA = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToInspectionTrayU;
        public double ToInspectionTrayU
        {
            get { return _ToInspectionTrayU; }
            set
            {
                if (value != _ToInspectionTrayU)
                {
                    _ToInspectionTrayU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToInspectionTrayW;
        public double ToInspectionTrayW
        {
            get { return _ToInspectionTrayW; }
            set
            {
                if (value != _ToInspectionTrayW)
                {
                    _ToInspectionTrayW = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ToInspectionTrayE;
        public double ToInspectionTrayE
        {
            get { return _ToInspectionTrayE; }
            set
            {
                if (value != _ToInspectionTrayE)
                {
                    _ToInspectionTrayE = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _ToInspectionTrayCommand;
        public ICommand ToInspectionTrayCommand
        {
            get
            {
                if (null == _ToInspectionTrayCommand) _ToInspectionTrayCommand = new AsyncCommand(ToInspectionTray);
                return _ToInspectionTrayCommand;
            }
        }
        private async Task ToInspectionTray()
        {
            try
            {
                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.INSPECTIONTRAY, UAxisSkip, CstNum, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int _CstNum;
        public int CstNum
        {
            get { return _CstNum; }
            set
            {
                if (value != _CstNum)
                {
                    _CstNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _ToCstCommand;
        public ICommand ToCstCommand
        {
            get
            {
                if (null == _ToCstCommand) _ToCstCommand = new AsyncCommand(ToCst);
                return _ToCstCommand;
            }
        }

        private async Task ToCst()
        {
            try
            {
                if (CstNum == 0)
                {
                    CstNum = 1;
                }

                (this.LoaderController() as LoaderController.LoaderController).MoveToModuleForSetup(ModuleTypeEnum.SLOT, UAxisSkip, CstNum, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private async Task<EventCodeEnum> ScanCSTFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                var cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();

                var foupController = this.FoupOpModule().GetFoupController(cassette.ID.Index);
                if (foupController.FoupModuleInfo.State == FoupStateEnum.UNLOAD)
                {
                    EventCodeEnum retVal = foupController.Execute(new FoupLoadCommand() { });
                }

                if (foupController.FoupModuleInfo.State != FoupStateEnum.LOAD)
                {
                    string caption = "ERROR";
                    string message = $"Foup state invalid. state={foupController.FoupModuleInfo.State}";

                    var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                    return EventCodeEnum.FOUP_ERROR;
                }

                if (cassette != null)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        var editor = LoaderController.GetLoaderMapEditor();
                        editor.EditorState.SetScanCassette(cassette.ID);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);


                        if (isInjected)
                        {
                            await Task.Run(() =>
                            {
                                ret = this.LoaderController().WaitForCommandDone();
                            });
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private bool _DryRunFlag;
        public bool DryRunFlag
        {
            get { return _DryRunFlag; }
            set
            {
                if (value != _DryRunFlag)
                {
                    _DryRunFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AsyncCommand _DryRunStopCommand;
        public ICommand DryRunStopCommand
        {
            get
            {
                if (null == _DryRunStopCommand) _DryRunStopCommand = new AsyncCommand(DryRunStopFunc);
                return _DryRunStopCommand;
            }
        }


        private async Task DryRunStopFunc()
        {
            try
            {
                DryRunFlag = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _LoaderDryRunCommand;
        public ICommand LoaderDryRunCommand
        {
            get
            {
                if (null == _LoaderDryRunCommand) _LoaderDryRunCommand = new AsyncCommand(LoaderDryRunFunc);
                return _LoaderDryRunCommand;
            }
        }

        private async Task LoaderDryRunFunc()
        {
            List<int> initSlot = new List<int>();
            TransferObject selSub = null;
            ModuleID destPos;
            LoaderMapCommandParameter cmdParam;
            Queue<HolderModuleInfo> sourceQueue = new Queue<HolderModuleInfo>();
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            DryRunFlag = true;
            Task<EventCodeEnum> retval = null;

            try
            {
                await Task.Run(() =>
                {
                    while (DryRunFlag)
                    {
                        retval = ScanCSTFunc();
                        retval.Wait();

                        var cst = this.LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();

                        initSlot.Clear();
                        foreach (var slot in cst.SlotModules)
                        {
                            if (slot.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                sourceQueue.Enqueue(slot);
                                if (initSlot.Find(i => i == slot.ID.Index) == 0)
                                {
                                    initSlot.Add(slot.ID.Index);
                                }
                            }
                        }

                        foreach (var prevSlotIndex in initSlot)
                        {
                            var curSlot = cst.SlotModules.FirstOrDefault(s => s.ID.Index == prevSlotIndex);
                            if (curSlot != null)
                            {
                                if (curSlot.WaferStatus != EnumSubsStatus.EXIST)
                                {
                                    DryRunFlag = false;
                                    break;
                                }
                            }
                            else
                            {
                                DryRunFlag = false;
                                break;
                            }
                        }
                        while (sourceQueue.Count > 0)
                        {
                            #region // Transfer to PA from slot
                            var source = sourceQueue.Dequeue();
                            var pa = this.LoaderController.LoaderInfo.StateMap.PreAlignModules.FirstOrDefault();

                            if (cst.ScanState == CassetteScanStateEnum.READ)
                            {
                                var selSlot = cst.SlotModules.Where(item => item.ID.Index == source.ID.Index).FirstOrDefault();
                                if (selSlot.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                                {
                                    selSub = selSlot.Substrate;
                                }
                            }
                            else
                            {
                                //this.ViewModelManager().ShowNotifyMessage(this.GetHashCode(), "Scan error", "Dry run cancelled. Scan data not available.");
                                DryRunFlag = false;
                                break;
                            }
                            destPos = pa.ID;

                            var editor = this.LoaderController.GetLoaderMapEditor();
                            editor.EditorState.SetTransfer(selSub.ID.Value, destPos);
                            cmdParam = new LoaderMapCommandParameter();
                            cmdParam.Editor = editor;
                            bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);
                            if (isInjected == false)
                            {
                                DryRunFlag = false;
                                break;
                            }

                            ret = this.LoaderController().WaitForCommandDone();

                            if (ret != EventCodeEnum.NONE)
                            {
                                DryRunFlag = false;
                                break;
                            }

                            #endregion
                            #region // Transfer back to cassette.
                            destPos = source.ID;
                            pa = this.LoaderController.LoaderInfo.StateMap.PreAlignModules.FirstOrDefault();
                            editor.EditorState.SetTransfer(pa.Substrate.ID.Value, destPos);
                            cmdParam = new LoaderMapCommandParameter();
                            cmdParam.Editor = editor;
                            isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);
                            if (isInjected == false)
                            {
                                DryRunFlag = false;
                                break;
                            }
                            ret = this.LoaderController().WaitForCommandDone();
                            if (ret != EventCodeEnum.NONE)
                            {
                                DryRunFlag = false;
                                break;
                            }
                            #endregion
                        }

                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ClearLoaderStateCommand;
        public ICommand ClearLoaderStateCommand
        {
            get
            {
                if (null == _ClearLoaderStateCommand) _ClearLoaderStateCommand = new AsyncCommand(ClearLoaderStateFunc);
                return _ClearLoaderStateCommand;
            }
        }
      

        private async Task ClearLoaderStateFunc()
        {
            try
            {
                //this.ViewModelManager().ShowNotifyMessage(this.GetHashCode(), "Loader", "Clear loader states.");

                this.LoaderController().LoaderSystemInit();

                //this.ViewModelManager().HideNotifyMessage(this.GetHashCode());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            Task<EventCodeEnum> task = null;

            try
            {
                if (this.LoaderController() is LoaderController.LoaderController)
                {
                    (this.LoaderController() as LoaderController.LoaderController).RetractAll();
                }

                task = Task.Run(() =>
                {
                    return EventCodeEnum.NONE;
                });
                task.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return task;
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                //(this.LoaderController() as LoaderController.LoaderController).RetractAll();

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (CameraDisplayPort == null)
                    {
                        CameraDisplayPort = new DisplayPort() { GUID = new Guid("5A6E07DB-F26E-4E4D-86C2-7BE9AE38E5C9") };

                        CameraDisplayPort.MotionManager = MotionManager;
                        CameraDisplayPort.VisionManager = VisionManager;
                        //CameraDisplayPort.CoordManager = CoordinateManager;
                        CameraDisplayPort.StageSupervisor = StageSupervisor;

                        ((UcDisplayPort.DisplayPort)CameraDisplayPort).DataContext = this;
                    }
                    if (this.VisionManager().CameraDescriptor != null)
                    {
                        foreach (var item in this.VisionManager().CameraDescriptor.Cams)
                        {
                            this.VisionManager().SetDisplayChannel(item, CameraDisplayPort);
                        }
                        Binding binding = new Binding("Cam");
                        BindingOperations.SetBinding(CameraDisplayPort, DisplayPort.AssignedCamearaProperty, binding);
                    }

                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            EventCodeEnum retval = EventCodeEnum.NONE;

            //Task<EventCodeEnum> task;

            //try
            //{
            //    task = Task.Run(() =>
            //    {

            //        return EventCodeEnum.NONE;
            //    });
            //    task.Wait();
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //    throw;
            //}

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
                    this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false, 10000);
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

                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                    this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true, 10000);
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



     

        #endregion

        #region Edge Find & NOtch
        private AsyncCommand _EdgeProcCommand;
        public ICommand EdgeProcCommand
        {
            get
            {
                if (null == _EdgeProcCommand) _EdgeProcCommand = new AsyncCommand(EdgeProc);
                return _EdgeProcCommand;
            }
        }
        private async Task<EventCodeEnum> EdgeProc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        this.VisionManager().GetCam(EnumProberCam.PACL8_CAM).SetLight(EnumLightType.AUX, 255);
                        
                        var ib = this.VisionManager().SingleGrab(EnumProberCam.PACL8_CAM, this);

                        this.VisionManager().VisionProcessing.FindEdgeProcessing(ib, true);
                        this.VisionManager().GetCam(EnumProberCam.PACL8_CAM).SetLight(EnumLightType.AUX, 0);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        StageButtonsVisibility = true;
                    }

                });
            }

            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }
            finally
            {
                StageButtonsVisibility = true;
            }
            return ret;
        }

        private AsyncCommand _NotchFindProcCommand;
        public ICommand NotchFindProcCommand
        {
            get
            {
                if (null == _NotchFindProcCommand) _NotchFindProcCommand = new AsyncCommand(NotchFindProc);
                return _NotchFindProcCommand;
            }
        }
        private async Task<EventCodeEnum> NotchFindProc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        this.VisionManager().GetCam(EnumProberCam.PACL6_CAM).SetLight(EnumLightType.AUX, 255);
                        
                        var ib = this.VisionManager().SingleGrab(EnumProberCam.PACL6_CAM, this);

                        this.VisionManager().VisionProcessing.FindEdgeProcessing(ib, true);
                        var notchaxis = LoaderController.LoaderSystemParam.PreAlignModules[0].AxisType.Value;
                        var axis = MotionManager.GetAxis(notchaxis);
                        this.MotionManager().NotchFinding(axis, EnumMotorDedicatedIn.MotorDedicatedIn_1R);
                        this.VisionManager().GetCam(EnumProberCam.PACL6_CAM).SetLight(EnumLightType.AUX, 0);

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        StageButtonsVisibility = true;
                    }
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }
            finally
            {
                StageButtonsVisibility = true;
            }
            return ret;
        }
        #endregion

        #region CeteringTestFlag
        private AsyncCommand _CenteringTestFlagOnCommand;
        public ICommand CenteringTestFlagOnCommand
        {
            get
            {
                if (null == _CenteringTestFlagOnCommand) _CenteringTestFlagOnCommand = new AsyncCommand(CenteringTestFlagOn);
                return _CenteringTestFlagOnCommand;
            }
        }
        private async Task<EventCodeEnum> CenteringTestFlagOn()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        (LoaderController as LoaderController.LoaderController).SetTestCenteringFlag(true);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        StageButtonsVisibility = true;
                    }
                });
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }
            finally
            {
                StageButtonsVisibility = true;
            }
            return ret;
        }


        private AsyncCommand _CenteringTestFlagOffCommand;
        public ICommand CenteringTestFlagOffCommand
        {
            get
            {
                if (null == _CenteringTestFlagOffCommand) _CenteringTestFlagOffCommand = new AsyncCommand(CenteringTestFlagOff);
                return _CenteringTestFlagOffCommand;
            }
        }
        private async Task<EventCodeEnum> CenteringTestFlagOff()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        (LoaderController as LoaderController.LoaderController).SetTestCenteringFlag(false);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        StageButtonsVisibility = true;
                    }
                });
            }

            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }
            finally
            {
                StageButtonsVisibility = true;
            }
            return ret;
        }
        #endregion
    }
}
