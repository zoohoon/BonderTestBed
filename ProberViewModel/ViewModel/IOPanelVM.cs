using Autofac;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
//using ProberInterfaces.ThreadSync;

namespace IOPanelViewModel
{
    public class IOPanelVM : IMainScreenViewModel
    {
        readonly Guid _ViewModelGUID = new Guid("30BE2FDA-E484-D816-3F5E-405677FE3F2E");
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

        //private LockKey outPortLock = new LockKey("IO Panel VM - out port");
        private object outPortLock = new object();
        private AsyncObservableCollection<IOPortDescripter<bool>> _FilteredOutputPorts
            = new AsyncObservableCollection<IOPortDescripter<bool>>();
        public AsyncObservableCollection<IOPortDescripter<bool>> FilteredOutputPorts
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

        //private LockKey inPortLock = new LockKey("IO Panel VM - in port");
        private object inPortLock = new object();

        private AsyncObservableCollection<IOPortDescripter<bool>> _FilteredInputPorts
            = new AsyncObservableCollection<IOPortDescripter<bool>>();
        public AsyncObservableCollection<IOPortDescripter<bool>> FilteredInputPorts
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

        public IOPanelVM()
        {
            _SearchKeyword = "";
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

                    if (this.IOManager() != null)
                    {
                        OutputPorts.Clear();
                        InputPorts.Clear();
                        //Stage,Loader OutPut
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
                        //Foup OutPut
                        propertyInfos = this.FoupOpModule().GetFoupIOMap(1).Outputs.GetType().GetProperties();

                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.FoupOpModule().GetFoupIOMap(1).Outputs);
                                port = (IOPortDescripter<bool>)propObject;
                                OutputPorts.Add(port);
                                FilteredOutputPorts.Add(port);
                            }
                        }


                        //Stage,Loader InPut
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

                        //Foup InPut
                        propertyInfos = this.FoupOpModule().GetFoupIOMap(1).Inputs.GetType().GetProperties();
                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.FoupOpModule().GetFoupIOMap(1).Inputs);
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
                        var ouputfiltered = new ObservableCollection<IOPortDescripter<bool>>(outs);


                        var inputs = InputPorts.Where(
                         t => t.Key.Value.StartsWith(upper) ||
                         t.Key.Value.StartsWith(lower) ||
                         t.Key.Value.ToUpper().Contains(upper));
                        var inputfiltered = new ObservableCollection<IOPortDescripter<bool>>(inputs);

                        //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            //{
                            FilteredOutputPorts.Clear();
                            foreach (var item in ouputfiltered)
                            {
                                FilteredOutputPorts.Add(item);
                            }
                            //});
                        }




                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            //{
                            FilteredInputPorts.Clear();
                            foreach (var item in inputfiltered)
                            {
                                FilteredInputPorts.Add(item);
                            }
                            //});

                        }
                        //});

                    }
                    else
                    {
                        //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        //{
                        lock (inPortLock)
                        {
                            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            //{
                            FilteredInputPorts.Clear();
                            foreach (var item in InputPorts)
                            {
                                FilteredInputPorts.Add(item);
                            }
                            //});
                        }

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            //System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            //{
                            FilteredOutputPorts.Clear();
                            foreach (var item in OutputPorts)
                            {
                                FilteredOutputPorts.Add(item);
                            }
                            //});
                        }
                        //});
                        //using (Locker locker = new Locker(inPortLock))
                        //{

                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum InitPage(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
}
