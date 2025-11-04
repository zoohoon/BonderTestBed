using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using ProberErrorCode;
using LogModule;
using System.IO;
using System.Windows.Input;
using RelayCommandBase;
using System.Windows;
using Autofac;
using VirtualKeyboardControl;
using Temperature;
using SubstrateObjects;
using ProbeCardObject;
using ProberInterfaces.ControlClass.ViewModel;
using SerializerUtil;
using MetroDialogInterfaces;
using NotifyEventModule;
using System.IO.Compression;
using ProberInterfaces.Temperature;
using System.Threading;
using ProberInterfaces.Event;

namespace DeviceChangeVM
{
    public class DeviceChangeViewModel : IDeviceChangeVM, IMainScreenViewModel, INotifyPropertyChanged
    {
        private Guid _ViewModelGUID = new Guid("39d5c48c-0bd7-4b6a-ab2d-113df85dce0e");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

        public ICoordinateManager CoordinateManager { get; set; }

        public IFileManager FileManager
        {
            get { return GetFileManager(); }
        }

        private IFileManager GetFileManager()
        {
            if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            {
                return this.FileManager();
            }
            else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            {
                return this.LoaderFileManager();
            }
            else
            {
                return null;
            }
        }

        private Visibility _UpDownloadVisibility;
        public Visibility UpDownloadVisibility
        {
            get { return _UpDownloadVisibility; }
            set
            {
                if (value != _UpDownloadVisibility)
                {
                    _UpDownloadVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<DeviceInfo> _ShowingDeviceInfoCollection
            = new ObservableCollection<DeviceInfo>();
        public ObservableCollection<DeviceInfo> ShowingDeviceInfoCollection
        {
            get { return _ShowingDeviceInfoCollection; }
            set
            {
                if (value != _ShowingDeviceInfoCollection)
                {
                    _ShowingDeviceInfoCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<DeviceInfo> _DeviceInfoCollection
        //    = new ObservableCollection<DeviceInfo>();
        //public ObservableCollection<DeviceInfo> DeviceInfoCollection
        //{
        //    get { return _DeviceInfoCollection; }
        //    set
        //    {
        //        if (value != _DeviceInfoCollection)
        //        {
        //            _DeviceInfoCollection = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private AsyncObservableCollection<DeviceInfo> _DeviceInfoCollection
            = new AsyncObservableCollection<DeviceInfo>();
        public AsyncObservableCollection<DeviceInfo> DeviceInfoCollection
        {
            get { return _DeviceInfoCollection; }
            set
            {
                if (value != _DeviceInfoCollection)
                {
                    _DeviceInfoCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DeviceInfo _SelectedDeviceInfo;
        public DeviceInfo SelectedDeviceInfo
        {
            get { return _SelectedDeviceInfo; }
            set
            {
                if (value != _SelectedDeviceInfo)
                {
                    _SelectedDeviceInfo = value;
                    RaisePropertyChanged();

                    if ((value != null) && (LoaderCalled == false))
                    {
                        Task.Run(() => GetParamFromDeviceWrapper(value));
                    }
                }
            }
        }

        private DeviceInfo _ShowingDevice;
        public DeviceInfo ShowingDevice
        {
            get { return _ShowingDevice; }
            set
            {
                if (value != _ShowingDevice)
                {
                    _ShowingDevice = value;
                    RaisePropertyChanged(nameof(ShowingDevice));
                }
            }
        }

        private RelayCommand<object> _ControlUnloadedCommand;
        public ICommand ControlUnloadedCommand
        {
            get
            {
                if (null == _ControlUnloadedCommand)
                    _ControlUnloadedCommand = new RelayCommand<object>(ControlUnloaded);
                return _ControlUnloadedCommand;
            }
        }

        private void ControlUnloaded(object obj)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        //private AsyncGenericCommand<DeviceInfo> _GetParamFromDeviceCommand;
        //public IAsyncCommand<DeviceInfo> GetParamFromDeviceCommand
        //{
        //    get
        //    {
        //        if (null == _GetParamFromDeviceCommand) _GetParamFromDeviceCommand = new AsyncGenericCommand<DeviceInfo>(GetParamFromDevice);
        //        return _GetParamFromDeviceCommand;
        //    }
        //}

        private AsyncCommand<DeviceInfo> _GetParamFromDeviceCommand;
        public IAsyncCommand GetParamFromDeviceCommand
        {
            get
            {
                if (null == _GetParamFromDeviceCommand) _GetParamFromDeviceCommand = new AsyncCommand<DeviceInfo>(GetParamFromDevice);
                return _GetParamFromDeviceCommand;
            }
        }


        private bool LoaderCalled = false;

        public async void GetParamFromDeviceWrapper(DeviceInfo device)
        {
            await this.GetParamFromDeviceCommand.ExecuteAsync(device);
        }


        public async Task GetParamFromDevice(DeviceInfo device)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetType().ToString(), "Wait");

                IParam tmpParam = null;
                //this.LotOPModule().LotInfo.DeviceName.Value = device.Name;

                await System.Windows.Application.Current.Dispatcher.Invoke((async delegate ()
                {
                    if (ShowingDevice != null)
                    {
                        ShowingDevice.SetDutViewControl(null, null, null);
                    }
                }));

                //await this.WaitCancelDialogService().ShowDialog("Get Device Info");
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Get Device Info");

                TempControllerDevParam tempControllerDevParam = null;
                WaferObject waferObject = null;
                ProbeCard probeCardInfo = null;

                Task task = new Task(() =>
                {

                    if (FileManager.GetDeviceName() == device?.Name)
                    {
                        tempControllerDevParam = (this.TempController() as TempController)?.TempControllerDevParameter;
                        waferObject = this.GetParam_Wafer() as WaferObject;
                        probeCardInfo = this.GetParam_ProbeCard() as ProbeCard;
                    }
                    else
                    {
                        tempControllerDevParam = new TempControllerDevParam();
                        string FullPath = FileManager.GetDeviceParamFullPath(device.Name + "\\" + tempControllerDevParam.FilePath, tempControllerDevParam.FileName, false);
                        retval = this.LoadParameter(ref tmpParam, typeof(TempControllerDevParam), null, FullPath);

                        if (retval == EventCodeEnum.NONE)
                        {
                            tempControllerDevParam = tmpParam as TempControllerDevParam;
                        }

                        tmpParam = null;

                        waferObject = new WaferObject() { WaferDevObject = new WaferDevObject() };

                        FullPath = FileManager.GetDeviceParamFullPath(device.Name + "\\" + waferObject.WaferDevObject.FilePath, waferObject.WaferDevObject.FileName, false);
                        retval = this.LoadParameter(ref tmpParam, typeof(WaferDevObject), null, FullPath);

                        if (retval == EventCodeEnum.NONE)
                        {
                            waferObject.WaferDevObject = tmpParam as WaferDevObject;
                            waferObject.Init();
                        }

                        tmpParam = null;
                        probeCardInfo = new ProbeCard() { ProbeCardDevObject = new ProbeCardDevObject() };
                        FullPath = FileManager.GetDeviceParamFullPath(device.Name + "\\" + probeCardInfo.ProbeCardDevObjectRef.FilePath, probeCardInfo.ProbeCardDevObjectRef.FileName, false);
                        retval = this.LoadParameter(ref tmpParam, typeof(ProbeCardDevObject), null, FullPath);
                        if (retval == EventCodeEnum.NONE)
                        {
                            probeCardInfo.ProbeCardDevObject = tmpParam as ProbeCardDevObject;
                        }
                    }

                    if (tempControllerDevParam != null
                        || waferObject != null
                        || probeCardInfo != null
                        )
                    {
                        device.SetTemp = tempControllerDevParam?.SetTemp?.Value ?? 300;

                        switch (waferObject.GetPhysInfo().WaferSizeEnum)
                        {
                            case EnumWaferSize.INCH12:
                                device.WaferSize = "12 Inch";
                                break;
                            case EnumWaferSize.INCH8:
                                device.WaferSize = "8 Inch";
                                break;
                            case EnumWaferSize.INCH6:
                                device.WaferSize = "6 Inch";
                                break;
                            case EnumWaferSize.INVALID:
                            default:
                                device.WaferSize = "INVALID";
                                break;
                        }

                        waferObject.ZoomLevel = (float)(waferObject.GetPhysInfo().MapCountX.Value * 1.7);
                        device.WaferThickness = waferObject.GetPhysInfo().Thickness.Value;
                        device.WaferNotchType = waferObject.GetPhysInfo().NotchType.Value;
                        device.WaferNotchAngle = waferObject.GetPhysInfo().NotchAngle.Value;
                        device.WaferMapCountX = waferObject.GetPhysInfo().MapCountX.Value;
                        device.WaferMapCountY = waferObject.GetPhysInfo().MapCountY.Value;
                        device.DieSizeX = waferObject.GetPhysInfo().DieSizeX.Value;
                        device.DieSizeY = waferObject.GetPhysInfo().DieSizeY.Value;

                        device.DutCount = probeCardInfo.ProbeCardDevObject.DutList.Count;
                        device.SetDutViewControl(this, waferObject, probeCardInfo);

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowingDevice = device;
                        });

                        //ShowingDevice = device;
                        //ShowingDevice.ChangedDutViewControl();
                    }
                });
                task.Start();
                await task;

                if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                {
                    // Make Byte Data WaferObject

                    byte[] waferobjectdata = null;
                    waferobjectdata = SerializeManager.SerializeToByte(ShowingDevice.DutViewControl.WaferObject, typeof(WaferObject));

                    // Make Byte Data DIEs

                    byte[] diesdata = null;

                    diesdata = this.ObjectToByteArray(ShowingDevice.DutViewControl.WaferObject.GetSubsInfo().DIEs);

                    // Make 

                    ISubstrateInfo tmp = ShowingDevice.DutViewControl.WaferObject.GetSubsInfo();

                    SubstrateInfoNonSerialized SubNonSerialized = new SubstrateInfoNonSerialized();

                    SubNonSerialized.WaferObjectChangedToggle = tmp.WaferObjectChangedToggle;
                    SubNonSerialized.WaferCenter = tmp.WaferCenter;
                    SubNonSerialized.ActualDieSize = tmp.ActualDieSize;
                    SubNonSerialized.ActualDeviceSize = tmp.ActualDeviceSize;

                    // Make Byte Data ProbeCardObject

                    byte[] probecarddata = null;

                    probecarddata = SerializeManager.SerializeToByte(ShowingDevice.DutViewControl.ProbeCard, typeof(ProbeCard));

                    await this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateShowingDevice(ShowingDevice, waferobjectdata, diesdata, SubNonSerialized, probecarddata);

                    await UpdateShowingDevicelistCallback();

                    LoaderCalled = true;

                    SelectedDeviceInfo = device;
                    LoaderCalled = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetType().ToString());
            }
        }

        private string _SearchStr;
        public string SearchStr
        {
            get { return _SearchStr; }
            set
            {
                if (value != _SearchStr)
                {
                    _SearchStr = value;
                    RaisePropertyChanged();

                    FindDirectoryUsingText(value);
                }
            }
        }

        private object lockobj = new object();
        private void FindDirectoryUsingText(string searchData)
        {
            try
            {
                ShowingDeviceInfoCollection = null;
                FindAndSetDirectory(searchData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void FindAndSetDirectory(string searchData)
        {
            try
            {
                bool isSearchDataClearbtnVisible = false;
                ObservableCollection<DeviceInfo> showDirectoryCollection = new ObservableCollection<DeviceInfo>();

                if (!string.IsNullOrEmpty(searchData))
                {
                    foreach (var directory in DeviceInfoCollection)
                    {
                        if (directory.Name.ToLower().Contains(searchData.ToLower()))
                        {
                            showDirectoryCollection.Add(directory);
                        }
                    }
                    isSearchDataClearbtnVisible = true;
                }
                else
                {
                    showDirectoryCollection = DeviceInfoCollection;
                    isSearchDataClearbtnVisible = false;
                }

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ShowingDeviceInfoCollection = showDirectoryCollection;
                    IsSearchDataClearButtonVisible = isSearchDataClearbtnVisible;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _IsSearchDataClearButtonVisible;
        public bool IsSearchDataClearButtonVisible
        {
            get { return _IsSearchDataClearButtonVisible; }
            set
            {
                if (value != _IsSearchDataClearButtonVisible)
                {
                    _IsSearchDataClearButtonVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCanChangeDevice = true;
        public bool IsCanChangeDevice
        {
            get { return _IsCanChangeDevice; }
            set
            {
                if (value != _IsCanChangeDevice)
                {
                    _IsCanChangeDevice = value;
                    RaisePropertyChanged();
                }
            }
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //DutViewer = new DutViewerViewModel(null, new System.Windows.Size(892, 911));
                    CoordinateManager = this.CoordinateManager();

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
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Error($"[DeviceChangeViewModel] InitModule() Error.");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    UpDownloadVisibility = Visibility.Visible;
                }
                else
                {
                    UpDownloadVisibility = Visibility.Hidden;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                await GetDevList(true);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ITempController TempController = this.TempController();
                double set_temp = TempController.GetSetTemp();
                Dictionary<double, double> Dic_HeaterOffset = TempController.GetHeaterOffsets();
                if (Dic_HeaterOffset != null && !(Dic_HeaterOffset.Count <= 0))
                {
                    if (Dic_HeaterOffset.ContainsKey(set_temp))
                    {
                        // 여기로 들어온 경우는 Change Device 화면에서 최종적으로 Set 된 Device의  Set Temp가 Temp table 내에 포함되어 있어서 들어온 것임.
                        // 위에서 말하는 Temp table 이란 SystemParam\Temperature\TC_CommunicationInfo.json 내부 HeaterOffsetDictionary라는 이름의 딕셔너리로 저장되어 있는 파라미터를 말함.
                        // UI위치는 Cell 화면에서 Menu - System - configuration - System - Temperature - Heater Offset Table (Loader에서는 Temp.Claibration)
                    }
                    else
                    {
                        string keys = string.Join(", ", TempController.TempManager.Dic_HeaterOffset.Keys);

                        this.MetroDialogManager().ShowMessageDialog("Notify", "The SetTemp value of the device is not included in the \"HeaterOffsetDictionary\"." +
                            "\n" +
                            $"\nThe current Set Temp value of the device is \"{set_temp}\"" +
                            $"\nHeater Offset Table: {keys}" +
                            "\nThe configuration UI path is: Menu - System - Configuration - System - Temperature - Heater Offset Table. ", EnumMessageStyle.Affirmative);
                        LoggerManager.Debug($"[DeviceChangeViewModel] Cleanup(), The SetTemp value of the device is not included in the HeaterOffsetDictionary. The current Set Temp value of the device is \"{set_temp}\" Heater Offset Table: {keys} The configuration UI path is: Menu - System - Configuration - System - Temperature - Heater Offset Table. ");
                    }
                }
                else
                {
                    LoggerManager.Debug($"GetHeaterOffsets() is null. ");
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private AsyncCommand _ChangeDeviceCommand;
        public IAsyncCommand ChangeDeviceCommand
        {
            get
            {
                if (null == _ChangeDeviceCommand)
                    _ChangeDeviceCommand = new AsyncCommand(ChangeDeviceFunc);
                return _ChangeDeviceCommand;
            }
        }

        public async Task ChangeDeviceFunc()
        {
            try
            {
                if (SelectedDeviceInfo != null)
                {
                    await ChangeDeviceFuncUsingName(SelectedDeviceInfo.Name);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ChangeDeviceFunc(string deviceName)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceName))
                {
                    await ChangeDeviceFuncUsingName(deviceName);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task UpdateShowingDevicelistCallback()
        {
            try
            {
                if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                {
                    await this.LoaderRemoteMediator()?.GetServiceCallBack()?.UpdateShowingDevicelist(ShowingDeviceInfoCollection.ToList());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private async Task ChangeDeviceFuncUsingName(string devName)
        {
            //IFileManager FileManager = FileManager;

            Autofac.IContainer Container = this.GetContainer();

            try
            {
                //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Changing Device");
                //

                if (!string.IsNullOrEmpty(devName))
                {

                    //await this.WaitCancelDialogService().CloseDialog();
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    await Task.Run(async () =>
                    {
                        int count = 0;
                        try
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(StageRecipeReadStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();

                            FileManager.ChangeDevice(devName);
                            this.LotOPModule().SetDeviceName(devName);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            return;
                        }

                        //this.LotOPModule().LotInfo.DeviceName.Value = devName;

                        try
                        {
                            var stageLotInfos = this.LotOPModule().LotInfo.GetLotInfos();

                            this.ParamManager().DevDBElementDictionary.Clear();
                            var modules = Container.Resolve<IEnumerable<IFactoryModule>>().Where(module => module is IHasDevParameterizable);

                            foreach (var v in modules)
                            {
                                EventCodeEnum loadDevResult = EventCodeEnum.UNDEFINED;
                                try
                                {
                                    IHasDevParameterizable module = v as IHasDevParameterizable;
                                    loadDevResult = module.LoadDevParameter();
                                    loadDevResult = module.InitDevParameter();

                                    if (module is IStageSupervisor)
                                    {
                                        (module as IStageSupervisor)?.SetWaferObjectStatus();
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Error($"Error Module is {v}");
                                    LoggerManager.Exception(err);

                                    
                                    if(stageLotInfos != null)
                                    {
                                        var lotInfo = stageLotInfos.Find(lotinfo => lotinfo.LotID.Equals(this.LotOPModule().LotInfo.LotName.Value));
                                        
                                        if (lotInfo != null)
                                        {
                                            int foupnum = lotInfo.FoupIndex;
                                            string cstHashCode = lotInfo.CassetteHashCode;

                                            if (this.DeviceModule().IsHaveReservationRecipe(foupnum, this.LotOPModule().LotInfo.LotName.Value))
                                            {
                                                this.DeviceModule().ClearActiveDeviceDic(foupnum, this.LotOPModule().LotInfo.LotName.Value, cstHashCode);
                                            }

                                            this.DeviceModule().SetDeviceLoadResult(foupnum, this.LotOPModule().LotInfo.LotName.Value, devName, false);
                                        }
                                    }
                                }
                            }

                            this.ParamManager().LoadDevElementInfoFromDB();

                            if (stageLotInfos != null)
                            {
                                var lotInfo = stageLotInfos.Find(lotinfo => lotinfo.LotID.Equals(this.LotOPModule().LotInfo.LotName.Value));
                                
                                if (lotInfo != null)
                                {
                                    int foupnum = lotInfo.FoupIndex;
                                    string cstHashCode = lotInfo.CassetteHashCode;

                                    if (this.DeviceModule().IsHaveReservationRecipe(foupnum, this.LotOPModule().LotInfo.LotName.Value))
                                    {
                                        this.DeviceModule().ClearActiveDeviceDic(foupnum, this.LotOPModule().LotInfo.LotName.Value, cstHashCode);
                                    }

                                    this.DeviceModule().SetDeviceLoadResult(foupnum, this.LotOPModule().LotInfo.LotName.Value, devName, true);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            this.NotifyManager().Notify(EventCodeEnum.DEVICE_CHANGE_FAIL);
                            LoggerManager.Exception(err);

                        }

                        foreach (var device in DeviceInfoCollection)
                        {
                            if (device.Name == devName)
                            {
                                device.IsNowDevice = true;
                            }
                            else
                            {
                                device.IsNowDevice = false;
                            }
                        }

                        this.StageSupervisor().SetWaferObjectStatus();

                        IProbingModule ProbingModule = this.ProbingModule();
                        ProbingModule.SetProbingMachineIndexToCenter();

                        LoggerManager.Debug($"[DeviceChange] Change Device Name: {devName}");

                        this.GEMModule().GetPIVContainer().StageNumber.Value = this.LoaderController().GetChuckIndex();
                        this.GEMModule().GetPIVContainer().RecipeID.Value = devName;
                        this.EventManager().RaisingEvent(typeof(DeviceChangedEvent).FullName);

                        UpdateShowingDevicelistCallback();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                
            }

        }

        private AsyncCommand<CUI.Button> _PageSwitchingCommand;
        public ICommand PageSwitchingCommand
        {
            get
            {
                if (null == _PageSwitchingCommand)
                    _PageSwitchingCommand = new AsyncCommand<CUI.Button>(PageSwitchingFunc);
                return _PageSwitchingCommand;
            }
        }

        private async Task PageSwitchingFunc(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIServices.CUIService.GetTargetViewGUID(cuiparam.GUID);
                await this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ClearSearchDataCommand;
        public ICommand ClearSearchDataCommand
        {
            get
            {
                if (null == _ClearSearchDataCommand)
                    _ClearSearchDataCommand = new RelayCommand<object>(ClearSearchDataFunc);
                return _ClearSearchDataCommand;
            }
        }

        private void ClearSearchDataFunc(object obj)
        {
            try
            {
                SearchStr = string.Empty;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<object> _SearchTBClickCommand;
        public ICommand SearchTBClickCommand
        {
            get
            {
                if (null == _SearchTBClickCommand)
                    _SearchTBClickCommand = new RelayCommand<object>(SearchTBClickFunc);
                return _SearchTBClickCommand;
            }
        }

        private void SearchTBClickFunc(object cuiparam)
        {
            try
            {
                Window Owner = Application.Current.MainWindow;

                SearchStr = VirtualKeyboard.Show(WindowLocationType.BOTTOM, SearchStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _RefreshDevListCommand;
        public ICommand RefreshDevListCommand
        {
            get
            {
                if (null == _RefreshDevListCommand)
                    _RefreshDevListCommand = new RelayCommand(GetDevListCommandFunc);
                return _RefreshDevListCommand;
            }
        }

        private void ViewClear()
        {
            try
            {
                ShowingDevice = null;
                SearchStr = string.Empty;
                DeviceInfoCollection.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetDevListCommandFunc()
        {
            try
            {
                GetDevList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GetDevList(bool isPageSwiching = false)
        {
            try
            {
                DeviceInfoCollection.Clear();

                //IFileManager FileManager = FileManager;
                var directories = Directory.GetDirectories(FileManager.FileManagerParam.DeviceParamRootDirectory);

                foreach (var directory in directories)
                {
                    var directoryNameSplit = directory.Split('\\');
                    DeviceInfo devInfo = new DeviceInfo();
                    devInfo.Name = directoryNameSplit[directoryNameSplit.Length - 1];
                    if (devInfo.Name != "ProbeCard" && devInfo.Name != "WaferMap")
                    {
                        DeviceInfoCollection.Add(devInfo);

                        if (devInfo.Name == FileManager.GetDeviceName())
                        {
                            devInfo.IsNowDevice = true;

                            if (!isPageSwiching)
                                SelectedDeviceInfo = devInfo;
                        }
                        else
                        {
                            devInfo.IsNowDevice = false;
                        }
                    }
                }

                ShowingDeviceInfoCollection = DeviceInfoCollection;

                UpdateShowingDevicelistCallback();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CreateNewDeviceCommand;
        public IAsyncCommand CreateNewDeviceCommand
        {
            get
            {
                if (null == _CreateNewDeviceCommand)
                    _CreateNewDeviceCommand = new AsyncCommand(CreateNewDeviceFunc);
                return _CreateNewDeviceCommand;
            }
        }

        public async Task CreateNewDeviceFunc()
        {
            try
            {

                EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;


                result = await this.MetroDialogManager().ShowSingleInputDialog("Name : ", "Create");

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    string devName = string.Empty;


                    Task task = new Task(() =>
                    {
                        devName = this.MetroDialogManager().GetSingleInputData();
                    });
                    task.Start();
                    await task;

                    if (!string.IsNullOrEmpty(devName))
                    {
                        bool alreadyHasDevDir = DeviceInfoCollection.Any(devItem => devItem.Name == devName);
                        bool isProcessChangeDevice = false;

                        if (alreadyHasDevDir)
                        {
                            isProcessChangeDevice = true;
                        }
                        else
                        {
                            isProcessChangeDevice = true;
                        }

                        if (isProcessChangeDevice)
                        {
                            await ChangeDeviceFuncUsingName(devName);
                            GetDevList();
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SaveAsDeviceCommand;
        public IAsyncCommand SaveAsDeviceCommand
        {
            get
            {
                if (null == _SaveAsDeviceCommand)
                    _SaveAsDeviceCommand = new AsyncCommand(SaveAsDeviceFunc);
                return _SaveAsDeviceCommand;
            }
        }

        public async Task SaveAsDeviceFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetType().ToString(), "Wait");

                EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;
                result = await this.MetroDialogManager().ShowSingleInputDialog("Name : ", "Save As", "Cancel", $"{FileManager.GetDeviceName()}");

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    string devName = string.Empty;

                    Task task = new Task(() =>
                    {
                        devName = this.MetroDialogManager().GetSingleInputData();
                    });
                    task.Start();
                    await task;
                    if (!string.IsNullOrEmpty(devName))
                    {
                        bool alreadyHasDevDir = DeviceInfoCollection.Any(devItem => devItem.Name == devName);
                        bool isProcessChangeDevice = false;

                        if (alreadyHasDevDir)
                        {
                            result = await this.MetroDialogManager().ShowMessageDialog("Already Exists the same name", "Do you want to delete the already existing "+devName+" device file and do Save As?", EnumMessageStyle.AffirmativeAndNegative);
                            if(result == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                isProcessChangeDevice = true;
                            }
                        }
                        else
                        {
                            isProcessChangeDevice = true;
                        }

                        if (isProcessChangeDevice)
                        {
                            EventCodeEnum retval = await CopyDeviceUsingName(devName);
                            if (retval != EventCodeEnum.NONE)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Save As Failed", "Please check device file status", EnumMessageStyle.Affirmative);
                                return;
                            }

                            await ChangeDeviceFuncUsingName(devName);
                            GetDevList();
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
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetType().ToString());
            }
        }

        private async Task<EventCodeEnum> CopyDeviceUsingName(string devName)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                Autofac.IContainer Container = this.GetContainer();

                if (!string.IsNullOrEmpty(devName))
                {
                    //await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    //   await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Save Device"));

                    Task task = new Task(() =>
                    {
                        int count = 0;
                        var devpath = FileManager.GetDeviceRootPath();
                        var origindevname = FileManager.GetDeviceName();
                        string sourceDirName = devpath +"\\"+ origindevname;
                        string zipPath = devpath + "\\" + origindevname + $"_{DateTime.Today.ToString("yyyy-MM-dd")}.zip";
                    string destDirName = devpath + "\\" + devName;

                        LoggerManager.Debug($"CopyDeviceUsingName():Source Device File Name = {sourceDirName} Dest Device File Name = {destDirName}");
                        try
                        {
                            if(File.Exists(zipPath) == true)
                                File.Delete(zipPath);

                            ZipFile.CreateFromDirectory(sourceDirName, zipPath);

                            DirectoryInfo dir = new DirectoryInfo(destDirName);
                            if (dir.Exists)
                            {
                                Directory.Delete(destDirName, true);
                            }
                            ZipFile.ExtractToDirectory(zipPath, destDirName);
                            File.Delete(zipPath);


                            retval = FileManager.ChangeDevice(devName);
                        }
                        catch (Exception err)
                        {
                            throw err;
                        }
                    });
                    task.Start();
                    await task;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"CopyDeviceUsingName():{err.Message}, Save As failed.");
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        private async Task SaveAllDeviceUsingName(string devName)
        {
            try
            {
                //IFileManager FileManager = FileManager;

                Autofac.IContainer Container = this.GetContainer();

                if (!string.IsNullOrEmpty(devName))
                {

                    //await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    //    await this.WaitCancelDialogService().ShowDialog("Save Device"));

                    await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                       await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Save Device"));

                    await Task.Run(async () =>
                    {
                        int count = 0;

                        try
                        {
                            FileManager.ChangeDevice(devName);

                            var modules = Container.Resolve<IEnumerable<IFactoryModule>>().Where(module => module is IHasDevParameterizable);
                            foreach (var v in modules)
                            {
                                try
                                {
                                    IHasDevParameterizable module = v as IHasDevParameterizable;
                                    module.SaveDevParameter();

                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err);

                                    throw new Exception($"Error Module is {v}");
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            //LoggerManager.Error($"[DeviceChangeViewModel - ChangeDeviceFunc()] Error occurred while Load Device Parameter. {e.Message}");
                            LoggerManager.Exception(err);

                        }
                        finally
                        {
                            //await this.WaitCancelDialogService().CloseDialog();
                            await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DeleteDeviceCommand;
        public IAsyncCommand DeleteDeviceCommand
        {
            get
            {
                if (null == _DeleteDeviceCommand)
                    _DeleteDeviceCommand = new AsyncCommand(DeleteDeviceFunc);
                return _DeleteDeviceCommand;
            }
        }

        public async Task DeleteDeviceFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetType().ToString(), "Wait");

                if (SelectedDeviceInfo == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Delete Device", "There is no device selected.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    if (1 < ShowingDeviceInfoCollection.Count)
                    {
                        string changeDeviceName = string.Empty;
                        //IFileManager FileManager = FileManager;
                        string nowDevice = FileManager.GetDeviceName();

                        string selecteddeivcename = SelectedDeviceInfo.Name;

                        var SelectedDeviceInCollection = ShowingDeviceInfoCollection.ToList().FirstOrDefault(x => x.Name == selecteddeivcename);
                        int index = -1;

                        if(SelectedDeviceInCollection != null)
                        {
                            index = ShowingDeviceInfoCollection.IndexOf(SelectedDeviceInCollection);
                        }

                        //int index = ShowingDeviceInfoCollection.IndexOf(SelectedDeviceInfo);

                        // 선택된 디바이스가 첫 번째 디바이스라면, 그 다음 바꿀 디바이스는 그 다음 디바이스로 설정.
                        if (0 == index)
                        {
                            changeDeviceName = ShowingDeviceInfoCollection[1].Name;
                        }
                        else
                        {
                            changeDeviceName = ShowingDeviceInfoCollection[0].Name;
                        }


                        if (nowDevice == SelectedDeviceInfo?.Name)
                        {
                            if (changeDeviceName != SelectedDeviceInfo.Name)
                            {
                                await ChangeDeviceFuncUsingName(changeDeviceName);
                            }
                        }


                        Task task = new Task(() =>
                        {
                            FileManager.DeleteDevice(SelectedDeviceInfo.Name);
                            GetDevList();
                        });
                        task.Start();
                        await task;
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Can't Delete DeviceFile", "There is only one device file.", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetType().ToString());
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public DeviceChangeDataDescription GetDeviceChangeInfo()
        {
            DeviceChangeDataDescription info = new DeviceChangeDataDescription();

            try
            {
                info.SearchStr = SearchStr;
                info.IsSearchDataClearButtonVisible = IsSearchDataClearButtonVisible;
                info.ShowingDeviceInfoCollection = ShowingDeviceInfoCollection;
                //info.SelectedDeviceInfo = SelectedDeviceInfo;
                info.ShowingDevice = ShowingDevice;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return info;
        }

        public Task SetShowingDeviceInfoCollectio(ObservableCollection<DeviceInfo> collection)
        {
            throw new NotImplementedException();
        }
    }
}
