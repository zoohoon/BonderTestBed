using Autofac;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UcDisplayPort;
using System.Threading;

namespace SystemPult
{
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



        private RelayCommand<object> _FindPathCommand;

        public ICommand FindPathCommand
        {
            get
            {
                if (null == _FindPathCommand) _FindPathCommand = new RelayCommand<object>(FindPath);
                return _FindPathCommand;
            }
        }

        private void FindPath(object obj)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (!string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        Path = dialog.SelectedPath;
                    }
                }
                //RequestSave?.Invoke();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                        Provider.RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                        Provider.WaitForAxisMotionDone(AxisObject, AxisObject.Param.TimeOut.Value);

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
                        PosButtonVisibility = false;
                        Provider.RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                        Provider.WaitForAxisMotionDone(AxisObject, AxisObject.Param.TimeOut.Value);
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
                PosButtonVisibility = false;
                NegButtonVisibility = false;
                await Task.Run(() =>
                {
                    this.MotionManager().Homing(this.AxisObject.AxisType.Value);
                });
                PosButtonVisibility = true;
                NegButtonVisibility = true;
            }
            catch (Exception err)
            {
                PosButtonVisibility = true;
                NegButtonVisibility = true;

                LoggerManager.Exception(err);
            }

        }

    }
    public class PultViewModel : IModule, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region // Properties
        System.Timers.Timer _monitoringTimer;
        private static int MonitoringInterValInms = 50;
        AutoResetEvent ResetEvent = new AutoResetEvent(false);

        public bool Initialized { get; set; }

        public PultViewModel()
        {
            ExecuteItem a = new ExecuteItem("C:\\ProberSystem\\Default");

            Pathlist.Add(a);
        }
        private void _monitoringTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //_monitoringTimer.Stop();

            try
            {
                //if (setState == true)
                //{
                //    mreUpdateEvent.Reset();
                //    setState = false;
                //}
                //else
                //{
                //    mreUpdateEvent.Set();
                //    setState = true;
                //}
                ResetEvent.Set();
                //_monitoringTimer.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPOutputs;
        public ObservableCollection<IOPortDescripter<bool>> GPOutputs
        {
            get { return _GPOutputs; }
            set
            {
                if (value != _GPOutputs)
                {
                    _GPOutputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPInputs;
        public ObservableCollection<IOPortDescripter<bool>> GPInputs
        {
            get { return _GPInputs; }
            set
            {
                if (value != _GPInputs)
                {
                    _GPInputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _GP_Foup_Outputs;
        public ObservableCollection<IOPortDescripter<bool>> GP_Foup_Outputs
        {
            get { return _GP_Foup_Outputs; }
            set
            {
                if (value != _GP_Foup_Outputs)
                {
                    _GP_Foup_Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GP_Foup_Inputs;
        public ObservableCollection<IOPortDescripter<bool>> GP_Foup_Inputs
        {
            get { return _GP_Foup_Inputs; }
            set
            {
                if (value != _GP_Foup_Inputs)
                {
                    _GP_Foup_Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DisplayFoupIOTab;
        public bool DisplayFoupIOTab
        {
            get { return _DisplayFoupIOTab; }
            set
            {
                if (value != _DisplayFoupIOTab)
                {
                    _DisplayFoupIOTab = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _SelectedPorts;
        public ObservableCollection<IOPortDescripter<bool>> SelectedPorts
        {
            get { return _SelectedPorts; }
            set
            {
                if (value != _SelectedPorts)
                {
                    _SelectedPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<Channel> _IOChannels;
        public ObservableCollection<Channel> IOChannels
        {
            get { return _IOChannels; }
            set
            {
                if (value != _IOChannels)
                {
                    _IOChannels = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _InSearchKeyword;
        public string InSearchKeyword
        {
            get { return _InSearchKeyword; }
            set
            {
                if (value != _InSearchKeyword)
                {
                    _InSearchKeyword = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _OutSearchKeyword;
        public string OutSearchKeyword
        {
            get { return _OutSearchKeyword; }
            set
            {
                if (value != _OutSearchKeyword)
                {
                    _OutSearchKeyword = value;
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

        private IOPortDescripter<bool> _BridgeRetract;
        public IOPortDescripter<bool> BridgeRetract
        {
            get { return _BridgeRetract; }
            set
            {
                if (value != _BridgeRetract)
                {
                    _BridgeRetract = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _BridgeExtand;
        public IOPortDescripter<bool> BridgeExtand
        {
            get { return _BridgeExtand; }
            set
            {
                if (value != _BridgeExtand)
                {
                    _BridgeExtand = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _CarrierUp;
        public IOPortDescripter<bool> CarrierUp
        {
            get { return _CarrierUp; }
            set
            {
                if (value != _CarrierUp)
                {
                    _CarrierUp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IOPortDescripter<bool> _CarrierDn;
        public IOPortDescripter<bool> CarrierDn
        {
            get { return _CarrierDn; }
            set
            {
                if (value != _CarrierDn)
                {
                    _CarrierDn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _CamViewPort;
        public IDisplayPort CamViewPort
        {
            get { return _CamViewPort; }
            set
            {
                if (value != _CamViewPort)
                {
                    _CamViewPort = value;
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

            PultModuleResolver.UserModuleConstructorEvent(fm);

            this.InitModule();
        }

        private AsyncCommand<IDataObject> _AddPortViewCommand;
        public ICommand AddPortViewCommand
        {
            get
            {
                if (null == _AddPortViewCommand) _AddPortViewCommand = new AsyncCommand<IDataObject>(AddPortView);
                return _AddPortViewCommand;
            }
        }

        private AsyncCommand<object> _ResetSelectedPorts;
        public ICommand ResetSelectedPorts
        {
            get
            {
                if (null == _ResetSelectedPorts) _ResetSelectedPorts = new AsyncCommand<object>(ResetPortView);
                return _ResetSelectedPorts;
            }
        }


        private AsyncCommand<ICamera> _SetCameraViewCommand;
        public ICommand SetCameraViewCommand
        {
            get
            {
                if (null == _SetCameraViewCommand) _SetCameraViewCommand = new AsyncCommand<ICamera>(SetCamViewMethod);
                return _SetCameraViewCommand;
            }
        }

        private AsyncCommand<ILightChannel> _SetLightValueCommand;
        public ICommand SetLightValueCommand
        {
            get
            {
                if (null == _SetLightValueCommand) _SetLightValueCommand = new AsyncCommand<ILightChannel>(SetLightValueMethod);
                return _SetLightValueCommand;
            }
        }

        private Task SetLightValueMethod(object obj)
        {
            try
            {
                ILightChannel lightChannel = null;
                if (obj is ILightChannel)
                {
                    lightChannel = (ILightChannel)obj;
                    lightChannel.SetLight(lightChannel.CurLightValue);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private Task SetCamViewMethod(object obj)
        {
            ICamera camera = null;
            if (obj is ICamera)
            {
                camera = (ICamera)obj;
                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                this.VisionManager().StopGrab(cam.GetChannelType());
                cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                this.VisionManager().StopGrab(cam.GetChannelType());
                cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StopGrab(cam.GetChannelType());
                cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                this.VisionManager().StopGrab(cam.GetChannelType());
                cam = this.VisionManager().GetCam(camera.Param.ChannelType.Value);
                ViewCamera = cam;

                this.VisionManager().SetDisplayChannel(ViewCamera, CamViewPort);

                this.VisionManager().StartGrab(cam.GetChannelType(), this);
            }
            return Task.CompletedTask;
        }
        private ICamera _ViewCamera;
        public ICamera ViewCamera
        {
            get { return _ViewCamera; }
            set
            {
                if (value != _ViewCamera)
                {
                    _ViewCamera = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Task ResetPortView(object obj)
        {
            try
            {
                Properties.Settings.Default.PreselectedPorts = string.Empty;
                Properties.Settings.Default.Save();
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    SelectedPorts.Clear();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ResetPortView(): Error occurred. Err = {err.Message}");
            }
            return Task.CompletedTask;
        }

        private Task AddPortView(object param)
        {
            IDataObject ido = param as IDataObject;
            if (null == ido)
                return Task.CompletedTask;
            try
            {
                var formats = ido.GetFormats();
                if (formats.Count() > 0)
                {
                    var data = ido.GetData(formats[0]) as string;

                    //var port = GPInputs.FirstOrDefault(p => p.Key.Value == data);

                    var port = GetIOPortByKey(data);
                    if (port != null)
                    {
                        if (SelectedPorts.FirstOrDefault(p => p.Key.Value == data) == null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SelectedPorts.Add(port);
                            });
                            StringBuilder stb = new StringBuilder();
                            var prop = Properties.Settings.Default.PreselectedPorts.ToString();
                            prop.Replace("  ", string.Empty);

                            stb.Append(prop);
                            if (stb.Length > 0)
                            {
                                stb.Append(",");
                            }
                            stb.Append(data);
                            Properties.Settings.Default.PreselectedPorts = stb.ToString();
                            Properties.Settings.Default.Save();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"AddPortView({param.ToString()}): Error occurred. Err = {err.Message}");
            }
            return Task.CompletedTask;
        }
        private IOPortDescripter<bool> GetIOPortByKey(string key)
        {
            var port = GPInputs.FirstOrDefault(p => p.Key.Value == key);
            if (port == null)
            {
                port = GPOutputs.FirstOrDefault(p => p.Key.Value == key);
            }
            return port;
        }

        //private ObservableCollection<IOPortDescripter<bool>> _FilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
        //public ObservableCollection<IOPortDescripter<bool>> FilteredPorts
        //{
        //    get { return _FilteredPorts; }
        //    set
        //    {
        //        if (value != _FilteredPorts)
        //        {
        //            _FilteredPorts = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private ObservableCollection<IOPortDescripter<bool>> _InFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> InFilteredPorts
        {
            get { return _InFilteredPorts; }
            set
            {
                if (value != _InFilteredPorts)
                {
                    _InFilteredPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _OutFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> OutFilteredPorts
        {
            get { return _OutFilteredPorts; }
            set
            {
                if (value != _OutFilteredPorts)
                {
                    _OutFilteredPorts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _InFilteredFoupPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> InFilteredFoupPorts
        {
            get { return _InFilteredFoupPorts; }
            set
            {
                if (value != _InFilteredFoupPorts)
                {
                    _InFilteredFoupPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _OutFilteredFoupPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> OutFilteredFoupPorts
        {
            get { return _OutFilteredFoupPorts; }
            set
            {
                if (value != _OutFilteredFoupPorts)
                {
                    _OutFilteredFoupPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        private RelayCommand<object> _SetOrgValueCommand;

        public ICommand SetOrgValueCommand
        {
            get
            {
                if (null == _SetOrgValueCommand) _SetOrgValueCommand = new RelayCommand<object>(SetOrgValue);
                return _SetOrgValueCommand;
            }
        }
        private void SetOrgValue(object obj)
        {
            try
            {
                FSensorOrg0 = AxisZ0.Status.AuxPosition;
                FSensorOrg1 = AxisZ1.Status.AuxPosition;
                FSensorOrg2 = AxisZ2.Status.AuxPosition;
                fcal_actpos = AxisZ.Status.RawPosition.Actual;

                fcal_Tpos = 0;
                fcal_Z0Delt = 0;
                fcal_Z1Delt = 0;
                fcal_Z2Delt = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //------------
        double fcalVal1;
        double fcalVal2;
        double fcalVal3;
        double fcal_BVal1;
        double fcal_BVal2;
        double fcal_BVal3;
        //double fcal_refpos;
        double fcal_Tpos;
        double fcal_Z0VAl;
        double fcal_Z1VAl;
        double fcal_Z2VAl;
        double fcal_Z0Delt;
        double fcal_Z1Delt;
        double fcal_Z2Delt;
        //double fcal_temp;
        double fcal_actpos;

        private void CalcForceValue()
        {
            try
            {

                // FSensorDelt0 = FSensorOrg0 - AxisZ0.Status.AuxPosition;
                // FSensorDelt1 = FSensorOrg1 - AxisZ1.Status.AuxPosition;
                // FSensorDelt2 = FSensorOrg2 - AxisZ2.Status.AuxPosition;
                //FSensorDelt0 = AxisZ0.Status.AuxPosition - FSensorOrg0;
                //FSensorDelt1 = AxisZ1.Status.AuxPosition - FSensorOrg1;
                //FSensorDelt2 = AxisZ2.Status.AuxPosition - FSensorOrg2;

                if ((AxisZ0 != null) &&
                    (AxisZ1 != null) &&
                    (AxisZ2 != null)
                    )
                {
                    fcalVal1 = fcalVal1 + (AxisZ0.Status.AuxPosition - fcal_BVal1) * 0.7;
                    fcalVal2 = fcalVal2 + (AxisZ1.Status.AuxPosition - fcal_BVal2) * 0.7;
                    fcalVal3 = fcalVal3 + (AxisZ2.Status.AuxPosition - fcal_BVal3) * 0.7;
                    fcal_BVal1 = fcalVal1;
                    fcal_BVal2 = fcalVal2;
                    fcal_BVal3 = fcalVal3;

                    FSensorDelt0 = fcalVal1 - FSensorOrg0;
                    FSensorDelt1 = fcalVal2 - FSensorOrg1;
                    FSensorDelt2 = fcalVal3 - FSensorOrg2;

                    //  FSensorDelt0 *= SubDtoPRatio;
                    // FSensorDelt1 *= SubDtoPRatio;
                    //FSensorDelt2 *= SubDtoPRatio;

                    //ForcedValue = (FSensorDelt0 + FSensorDelt1 + FSensorDelt2);
                    //ForcedValue = Math.Round(ForcedValue, 1);

                    // if (AxisZ.Status.State ==  EnumAxisState.IDLE)

                    //fcal_actpos = AxisZ.Status.RawPosition.Actual; 
                    //if ((AxisZ.Status.State == EnumAxisState.IDLE) & (fcal_actpos != AxisZ.Status.RawPosition.Actual))
                    //{
                    if ((Math.Abs(FSensorDelt0) > 524.288) || (Math.Abs(FSensorDelt1) > 524.288) || (Math.Abs(FSensorDelt2) > 524.288))
                    {
                        fcal_Tpos = (AxisZ.Status.RawPosition.Actual - fcal_actpos) * 524.288;
                        fcal_Z0VAl = FSensorOrg0 + fcal_Tpos;
                        fcal_Z1VAl = FSensorOrg1 + fcal_Tpos;
                        fcal_Z2VAl = FSensorOrg2 + fcal_Tpos;

                        fcal_Z0Delt = (fcalVal1 - fcal_Z0VAl) * SubDtoPRatio;            //     AxisZ0.Status.AuxPosition;
                        fcal_Z1Delt = (fcalVal2 - fcal_Z1VAl) * SubDtoPRatio;            // AxisZ1.Status.AuxPosition;
                        fcal_Z2Delt = (fcalVal3 - fcal_Z2VAl) * SubDtoPRatio;            // AxisZ2.Status.AuxPosition;

                        //FSensorOrg0 = AxisZ0.Status.AuxPosition;
                        // FSensorOrg1 = AxisZ1.Status.AuxPosition;
                        // FSensorOrg2 = AxisZ2.Status.AuxPosition;
                        //fcal_actpos = AxisZ.Status.RawPosition.Actual;
                    }
                    // else
                    //{
                    //    fcal_Z0Delt = fcal_Z1Delt = fcal_Z2Delt = 0;
                    //}
                    //}
                    ForcedValue = (fcal_Z0Delt + fcal_Z1Delt + fcal_Z2Delt);
                    ForcedValue = Math.Round(ForcedValue, 1);
                    /*
                    if (AxisZ.Status.State != fcal_ZState_prev)
                     {
                       if (AxisZ.Status.RawPosition.Command != 0)
                        {
                            fcal_Tpos = 1000 * 524.288;
                            fcal_Z0VAl = FSensorOrg0 + fcal_Tpos;
                            fcal_Z1VAl = FSensorOrg1 + fcal_Tpos;
                            fcal_Z2VAl = FSensorOrg2 + fcal_Tpos;

                            fcal_Z0Delt = fcal_Z0VAl - AxisZ0.Status.AuxPosition;
                            fcal_Z1Delt = fcal_Z1VAl - AxisZ1.Status.AuxPosition;
                            fcal_Z2Delt = fcal_Z2VAl - AxisZ2.Status.AuxPosition;

                            fcal_temp = AxisZ.Status.RawPosition.Ref; 
                        }

                    }
                    */

                    // ForcedValue = (fcal_Z0Delt + fcal_Z1Delt + fcal_Z2Delt) * SubDtoPRatio;
                    // ForcedValue = Math.Round(ForcedValue, 1);
                    //var retrunvalue = this.MotionManager().WaitForAxisMotionDone(axisz, 50000);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        private ObservableCollection<AxisobjectVM> _LoaderAxisObjectVmList
            = new ObservableCollection<AxisobjectVM>();
        public ObservableCollection<AxisobjectVM> LoaderAxisObjectVmList
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

        private ProbeAxisObject _AxisZ0;
        public ProbeAxisObject AxisZ0
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

        private ProbeAxisObject _AxisZ1;
        public ProbeAxisObject AxisZ1
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

        private ProbeAxisObject _AxisZ2;
        public ProbeAxisObject AxisZ2
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

        private ProbeAxisObject _AxisZ;
        public ProbeAxisObject AxisZ
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
        private double _FSensorDelt0;
        public double FSensorDelt0
        {
            get { return _FSensorDelt0; }
            set
            {
                if (value != _FSensorDelt0)
                {
                    _FSensorDelt0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FSensorDelt1;
        public double FSensorDelt1
        {
            get { return _FSensorDelt1; }
            set
            {
                if (value != _FSensorDelt1)
                {
                    _FSensorDelt1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FSensorDelt2;
        public double FSensorDelt2
        {
            get { return _FSensorDelt2; }
            set
            {
                if (value != _FSensorDelt2)
                {
                    _FSensorDelt2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FSensorOrg0;
        public double FSensorOrg0
        {
            get { return _FSensorOrg0; }
            set
            {
                if (value != _FSensorOrg0)
                {
                    _FSensorOrg0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FSensorOrg1;
        public double FSensorOrg1
        {
            get { return _FSensorOrg1; }
            set
            {
                if (value != _FSensorOrg1)
                {
                    _FSensorOrg1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FSensorOrg2;
        public double FSensorOrg2
        {
            get { return _FSensorOrg2; }
            set
            {
                if (value != _FSensorOrg2)
                {
                    _FSensorOrg2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ForcedValue;
        public double ForcedValue
        {
            get { return _ForcedValue; }
            set
            {
                if (value != _ForcedValue)
                {
                    _ForcedValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SubDtoPRatio;
        public double SubDtoPRatio
        {
            get { return _SubDtoPRatio; }
            set
            {
                if (value != _SubDtoPRatio)
                {
                    _SubDtoPRatio = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Autofac.IContainer Container;

        public void DeInitModule()
        {
            //EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                this.IOManager().DeInitModule();
                foreach (var cam in AttatchedCameras)
                {
                    this.VisionManager().StopGrab(cam.Param.ChannelType.Value);
                }

                var modules = Container.Resolve<IEnumerable<IFactoryModule>>();
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

        public EventCodeEnum SetContainer()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Container = PultModuleResolver.ConfigureDependencies();
                this.SetContainer(Container);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetContainer(): Error occurred. Err = {err.Message}");
            }

            return retval;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                //Container = PultModuleResolver.ConfigureDependencies();
                //this.SetContainer(Container);
                GPInputs = new ObservableCollection<IOPortDescripter<bool>>();
                GPOutputs = new ObservableCollection<IOPortDescripter<bool>>();
                //FilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                InFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                OutFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                SelectedPorts = new ObservableCollection<IOPortDescripter<bool>>();
                IOChannels = new ObservableCollection<Channel>();
                InSearchKeyword = "";
                OutSearchKeyword = "";
                UpdateInputPorts();
                UpdateOutputPorts();

                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    GP_Foup_Inputs = new ObservableCollection<IOPortDescripter<bool>>();
                    GP_Foup_Outputs = new ObservableCollection<IOPortDescripter<bool>>();
                    InFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                    OutFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                    UpdateFoupInputPorts();
                    UpdateFoupOutputPorts();
                }

                var portKeyProperty = Properties.Settings.Default.PreselectedPorts;
                string[] portKeys = portKeyProperty.Split(',');

                foreach (var key in portKeys)
                {
                    var port = GetIOPortByKey(key);
                    if (port != null)
                    {
                        if (SelectedPorts.FirstOrDefault(p => p.Key.Value == key) == null)
                        {
                            SelectedPorts.Add(port);
                        }
                    }
                }

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
                CamViewPort = new DisplayPort();
                //WLCam.LightModules
                //WLCam.LightModules[0].
                //WHCam.LightsChannels[0].Type.Value
                //WHLights = new ObservableCollection<ILightChannel>();
                //foreach (var light in WHCam.LightsChannels)
                //{
                //    var chn = light.ChannelMapIdx.Value;
                //    WHLights.Add(this.LightAdmin().GetLightChannel(chn));
                //    //WHLights[0].CurLightValue
                //}
                //WLLights = new ObservableCollection<ILightChannel>();
                //foreach (var light in WLCam.LightsChannels)
                //{
                //    var chn = light.ChannelMapIdx.Value;
                //    WLLights.Add(this.LightAdmin().GetLightChannel(chn));
                //}
                //PHLights = new ObservableCollection<ILightChannel>();
                //foreach (var light in PHCam.LightsChannels)
                //{
                //    var chn = light.ChannelMapIdx.Value;
                //    PHLights.Add(this.LightAdmin().GetLightChannel(chn));
                //}
                //PLLights = new ObservableCollection<ILightChannel>();
                //foreach (var light in PLCam.LightsChannels)
                //{
                //    var chn = light.ChannelMapIdx.Value;
                //    PLLights.Add(this.LightAdmin().GetLightChannel(chn));
                //}

                //this.VisionManager().SetDisplayChannel(WLCam, CamViewPort);

                StageAxes aes = this.MotionManager().StageAxes;
                foreach (var item in aes.ProbeAxisProviders)
                {
                    if (item.AxisType.Value == EnumAxisConstants.R || item.AxisType.Value == EnumAxisConstants.TT ||
                            item.AxisType.Value == EnumAxisConstants.Z0 || item.AxisType.Value == EnumAxisConstants.Z1 ||
                            item.AxisType.Value == EnumAxisConstants.Z2)
                    {
                        var axisObjVM = new AxisobjectVM();
                        axisObjVM.AxisObject = item;
                        axisObjVM.NegButtonVisibility = false;
                        axisObjVM.PosButtonVisibility = false;

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
                        AxisZ0 = item;
                    }
                    if (item.AxisType.Value == EnumAxisConstants.Z1)
                    {
                        AxisZ1 = item;
                    }
                    if (item.AxisType.Value == EnumAxisConstants.Z2)
                    {
                        AxisZ2 = item;
                    }
                    if (item.AxisType.Value == EnumAxisConstants.Z)
                    {
                        AxisZ = item;
                    }
                }

                LoaderAxes loaderaxes = this.MotionManager().LoaderAxes;
                foreach (var item in loaderaxes.ProbeAxisProviders)
                {
                    var axisObjVM = new AxisobjectVM();
                    axisObjVM.AxisObject = item;

                    LoaderAxisObjectVmList.Add(axisObjVM);
                }

                _monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();

                //UpdateForceValue();

                errorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitModule(): Error occurred. Err = {err.Message}");
            }
            return errorCode;
        }

        private void UpdateForceValue()
        {
            try
            {
                Task.Run(() =>
                {
                    do
                    {
                        CalcForceValue();
                        ResetEvent.WaitOne(200);

                    } while (true);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _StageInitCommand;
        public ICommand StageInitCommand
        {
            get
            {
                if (null == _StageInitCommand) _StageInitCommand = new AsyncCommand(StageInit);
                return _StageInitCommand;
            }
        }

        private async Task StageInit()
        {
            try
            {
                await this.StageSupervisor().SystemInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void UpdateOutputPorts()
        {
            try
            {
                var outputs = this.IOManager().IO.Outputs;
                var outputType = outputs.GetType();
                var outprops = outputType.GetProperties();

                foreach (var item in outprops)
                {

                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                    if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                    {
                        var ios = item.GetValue(outputs) as List<IOPortDescripter<bool>>;
                        if (ios != null)
                        {
                            foreach (var port in ios)
                            {
                                if (port is IOPortDescripter<bool>)
                                {
                                    GPOutputs.Add(port);
                                }
                            }
                        }
                    }
                    else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        var iodesc = item.GetValue(outputs) as IOPortDescripter<bool>;
                        if (iodesc != null)
                            GPOutputs.Add(iodesc);
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        private void UpdateInputPorts()
        {
            try
            {
                var inputs = this.IOManager().IO.Inputs;
                var inputType = inputs.GetType();
                var props = inputType.GetProperties();

                foreach (var item in props)
                {

                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                    if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                    {
                        var ios = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                        if (ios != null)
                        {
                            foreach (var port in ios)
                            {
                                if (port is IOPortDescripter<bool>)
                                {
                                    GPInputs.Add(port);
                                }
                            }
                        }

                    }
                    else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        var iodesc = item.GetValue(inputs) as IOPortDescripter<bool>;
                        if (iodesc != null)
                            GPInputs.Add(iodesc);
                    }
                }
                BridgeRetract = inputs.DIWAFERCAMREAR;
                BridgeExtand = inputs.DIWAFERCAMMIDDLE;
                CarrierUp = inputs.DICH_CARRIER_UP;
                CarrierDn = inputs.DICARRIER_POS;

                //SelectedPorts = new ObservableCollection<IOPortDescripter<bool>>();
                //SelectedPorts.Add(inputs.DIWAFERCAMREAR);
                //SelectedPorts.Add(inputs.DIWAFERCAMMIDDLE);
                #region // Port status
                var serv = this.IOManager().IOServ;
                foreach (var inChannel in serv.Inputs)
                {
                    IOChannels.Add(inChannel);
                }
                foreach (var outChannel in serv.Outputs)
                {
                    IOChannels.Add(outChannel);
                }

                #endregion
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private void UpdateFoupOutputPorts()
        {
            try
            {
                if (this.FoupOpModule().FoupControllers == null)
                    return;
                if (this.FoupOpModule().FoupControllers.Count == 0)
                    return;

                var outputs = this.FoupOpModule().FoupControllers[0].GetFoupIOMap().Outputs;
                var outputType = outputs.GetType();
                var outprops = outputType.GetProperties();

                foreach (var item in outprops)
                {

                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                    if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                    {
                        var ios = item.GetValue(outputs) as List<IOPortDescripter<bool>>;
                        if (ios != null)
                        {
                            foreach (var port in ios)
                            {
                                if (port is IOPortDescripter<bool>)
                                {
                                    GP_Foup_Outputs.Add(port);
                                }
                            }
                        }
                    }
                    else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        var iodesc = item.GetValue(outputs) as IOPortDescripter<bool>;
                        if (iodesc != null)
                            GP_Foup_Outputs.Add(iodesc);
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        private void UpdateFoupInputPorts()
        {
            try
            {
                if (this.FoupOpModule().FoupControllers == null)
                    return;
                if (this.FoupOpModule().FoupControllers.Count == 0)
                    return;
                var inputs = this.FoupOpModule().FoupControllers[0].GetFoupIOMap().Inputs;
                var inputType = inputs.GetType();
                var props = inputType.GetProperties();

                foreach (var item in props)
                {

                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                    if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                    {
                        var ios = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                        if (ios != null)
                        {
                            foreach (var port in ios)
                            {
                                if (port is IOPortDescripter<bool>)
                                {
                                    GP_Foup_Inputs.Add(port);
                                }
                            }
                        }
                    }
                    else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        var iodesc = item.GetValue(inputs) as IOPortDescripter<bool>;
                        if (iodesc != null)
                            GP_Foup_Inputs.Add(iodesc);
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }


            //#region // Port status
            //var serv = this.IOManager().IOServ;
            //foreach (var inChannel in serv.Inputs)
            //{
            //    IOChannels.Add(inChannel);
            //}
            //foreach (var outChannel in serv.Outputs)
            //{
            //    IOChannels.Add(outChannel);
            //}
            //#endregion
        }
    }
}
