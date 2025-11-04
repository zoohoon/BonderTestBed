using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderDeviceChangeViewModelModule
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.ControlClass.ViewModel;
    using ProberInterfaces.Loader;
    using ProberInterfaces.Temperature;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    public class LoaderDeviceChangeViewModel : IMainScreenViewModel, IDeviceChangeVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("2DD68236-928F-FCE8-C0F2-AFC89EBA752C");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        public LoaderDeviceChangeViewModel()
        {

        }

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
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

        public Task<EventCodeEnum> InitViewModel()
        {
            // GP에서는 사용하지 않는 버튼 
            UpDownloadVisibility = Visibility.Hidden;

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
            try
            {
                ITempController TempController = this.TempController();


                double set_temp = TempController.GetSetTemp();
                Dictionary<double, double> Dic_HeaterOffset = TempController.GetHeaterOffsets();
                if(Dic_HeaterOffset != null && !(Dic_HeaterOffset.Count <= 0))
                {
                    if (Dic_HeaterOffset.ContainsKey(set_temp))
                    {
                        // 여기로 들어온 경우는 Change Device 화면에서 최종적으로 Set 된 Device의  Set Temp가 Temp table 내에 포함되어 있어서 들어온 것임.
                        // 위에서 말하는 Temp table 이란 SystemParam\Temperature\TC_CommunicationInfo.json 내부 HeaterOffsetDictionary라는 이름의 딕셔너리로 저장되어 있는 파라미터를 말함.
                        // UI위치는 Cell 화면에서 Menu - System - configuration - System - Temperature - Heater Offset Table (Loader에서는 Temp.Claibration)
                    }
                    else
                    {
                        string keys = string.Join(", ", Dic_HeaterOffset.Keys);

                        this.MetroDialogManager().ShowMessageDialog("Notify", "The SetTemp value of the device is not included in the \"HeaterOffsetDictionary\"" +
                            "\n" +
                            $"\nThe current Set Temp value of the device is \"{set_temp}\"" +
                            $"\nHeater Offset Table: {keys}" +
                            "\nThe configuration UI path is: Menu - Setting - Cell - System - Temp.Calibration- Heater Offset Table. ", EnumMessageStyle.Affirmative);
                        LoggerManager.Debug($"[LoaderDeviceChangeViewModel] Cleanup(), The SetTemp value of the device is not included in the HeaterOffsetDictionary. The current Set Temp value of the device is \"{set_temp}\" Heater Offset Table: {keys} The configuration UI path is: Menu - Setting - Cell - System - Temp.Calibration- Heater Offset Table.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"GetHeaterOffsets() is null. ");
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Cleanup(): Error occurred. Err = {err.Message}");
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region //..Property

        private IDeviceManager DeviceManager => this.GetLoaderContainer().Resolve<IDeviceManager>();

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

                    if (value != null)
                    {
                        //Task.Run(() => GetParamFromDevice(value));
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
                    if (_ShowingDevice != null)
                    {
                        Updateiscandevicechanged();
                    }
                }
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

                    //FindDirectoryUsingText(value);
                }
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

        #endregion

        #region //..Method

        public async Task GetDevList(bool isPageSwiching = false)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetType().ToString(), "Wait");

                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.DeviceChange_GetDevList(isPageSwiching);
                }

                //UpdateDeviceChangeInfo();

                DeviceInfo curDev = ShowingDeviceInfoCollection.ToList().Find(x => x.IsNowDevice == true);

                if (ShowingDevice == null)
                {
                    //Task.Run(() => GetParamFromDeviceWrapper(curDev));

                    Task task = new Task(() =>
                    {
                        GetParamFromDeviceWrapper(curDev);
                    });
                    task.Start();
                    await task;
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
                AsyncObservableCollection<DeviceInfo> showDirectoryCollection = new AsyncObservableCollection<DeviceInfo>();

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

        //private async void GetParamFromDevice(DeviceInfo device)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        IParam tmpParam = null;
        //        //this.LotOPModule().LotInfo.DeviceName.Value = device.Name;

        //        await System.Windows.Application.Current.Dispatcher.Invoke((async delegate ()
        //        {
        //            if (ShowingDevice != null)
        //            {
        //                ShowingDevice.SetDutViewControl(null, null, null);
        //            }
        //        }));

        //        TempControllerDevParam tempControllerDevParam = new TempControllerDevParam();
        //        WaferObject waferObject = null;
        //        ProbeCard probeCardInfo = null;

        //        string FullPath = DeviceManager.GetLoaderDevicePath() + "\\" + device.Name + "\\" + tempControllerDevParam.FilePath + "\\" + tempControllerDevParam.FileName;
        //        retval = this.LoadParameter(ref tmpParam, typeof(TempControllerDevParam), null, FullPath);
        //        if (retval == EventCodeEnum.NONE)
        //        {
        //            tempControllerDevParam = tmpParam as TempControllerDevParam;
        //        }

        //        tmpParam = null;
        //        waferObject = new WaferObject() { WaferDevObject = new WaferDevObject() };
        //        FullPath = DeviceManager.GetLoaderDevicePath() + "\\" + device.Name + "\\" + waferObject.WaferDevObject.FileName;
        //        retval = this.LoadParameter(ref tmpParam, typeof(WaferDevObject), null, FullPath);

        //        if (retval == EventCodeEnum.NONE)
        //        {
        //            waferObject.WaferDevObject = tmpParam as WaferDevObject;
        //            waferObject.Init();
        //        }

        //        tmpParam = null;
        //        probeCardInfo = new ProbeCard() { ProbeCardDevObject = new ProbeCardDevObject() };
        //        FullPath = DeviceManager.GetLoaderDevicePath() + "\\" + device.Name + "\\" + probeCardInfo.ProbeCardDevObjectRef.FileName;
        //        retval = this.LoadParameter(ref tmpParam, typeof(ProbeCardDevObject), null, FullPath);
        //        if (retval == EventCodeEnum.NONE)
        //        {
        //            probeCardInfo.ProbeCardDevObject = tmpParam as ProbeCardDevObject;
        //        }

        //        if (tempControllerDevParam != null
        //            || waferObject != null
        //            || probeCardInfo != null
        //            )
        //        {
        //            device.SetTemp = tempControllerDevParam?.SetTemp?.Value ?? 300;

        //            switch (waferObject.GetPhysInfo().WaferSizeEnum)
        //            {
        //                case EnumWaferSize.INCH12:
        //                    device.WaferSize = "12 Inch";
        //                    break;
        //                case EnumWaferSize.INCH8:
        //                    device.WaferSize = "8 Inch";
        //                    break;
        //                case EnumWaferSize.INCH6:
        //                    device.WaferSize = "6 Inch";
        //                    break;
        //                case EnumWaferSize.INVALID:
        //                default:
        //                    device.WaferSize = "INVALID";
        //                    break;
        //            }

        //            waferObject.ZoomLevel = (float)(waferObject.GetPhysInfo().MapCountX.Value * 1.7);
        //            device.WaferThickness = waferObject.GetPhysInfo().Thickness.Value;
        //            device.WaferNotchType = waferObject.GetPhysInfo().NotchType.Value;
        //            device.WaferNotchAngle = waferObject.GetPhysInfo().NotchAngle.Value;
        //            device.WaferMapCountX = waferObject.GetPhysInfo().MapCountX.Value;
        //            device.WaferMapCountY = waferObject.GetPhysInfo().MapCountY.Value;
        //            device.DieSizeX = waferObject.GetPhysInfo().DieSizeX.Value;
        //            device.DieSizeY = waferObject.GetPhysInfo().DieSizeY.Value;

        //            device.DutCount = probeCardInfo.ProbeCardDevObject.DutList.Count;
        //            device.SetDutViewControl(this, waferObject, probeCardInfo);

        //            await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
        //            {
        //                ShowingDevice = device;
        //            });
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //System.Threading.Thread.Sleep(2000);
        //    }
        //}


        #endregion

        #region //..Command & Command Method

        private RelayCommand _RefreshDevListCommand;
        public ICommand RefreshDevListCommand
        {
            get
            {
                if (null == _RefreshDevListCommand) _RefreshDevListCommand = new RelayCommand(FuncRefreshDevListCommand);
                return _RefreshDevListCommand;
            }
        }

        private void FuncRefreshDevListCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    _RemoteMediumProxy.DeviceChange_RefreshDevListCommand();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ClearSearchDataCommand;
        public ICommand ClearSearchDataCommand
        {
            get
            {
                if (null == _ClearSearchDataCommand) _ClearSearchDataCommand = new RelayCommand(FuncClearSearchDataCommand);
                return _ClearSearchDataCommand;
            }
        }

        private void FuncClearSearchDataCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    _RemoteMediumProxy.DeviceChange_ClearSearchDataCommand();

                    // Get Device list 
                    //UpdateDeviceChangeInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SearchTBClickCommand;
        public ICommand SearchTBClickCommand
        {
            get
            {
                if (null == _SearchTBClickCommand) _SearchTBClickCommand = new RelayCommand(FuncSearchTBClickCommand);
                return _SearchTBClickCommand;
            }
        }

        private void FuncSearchTBClickCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    _RemoteMediumProxy.DeviceChange_SearchTBClickCommand();

                    // Get Device list 
                    //UpdateDeviceChangeInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _PageSwitchingCommand;
        public ICommand PageSwitchingCommand
        {
            get
            {
                if (null == _PageSwitchingCommand) _PageSwitchingCommand = new RelayCommand(FuncPageSwitchingCommand);
                return _PageSwitchingCommand;
            }
        }

        private void FuncPageSwitchingCommand()
        {
            try
            {
                LoggerManager.Debug($"Not Implementation.");
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
                if (null == _CreateNewDeviceCommand) _CreateNewDeviceCommand = new AsyncCommand(FuncCreateNewDeviceCommand);
                return _CreateNewDeviceCommand;
            }
        }

        private async Task FuncCreateNewDeviceCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.DeviceChange_CreateNewDeviceCommand();
                }

                IStageObject selectedstage = _LoaderCommunicationManager.SelectedStage;

                if (selectedstage != null)
                {
                    _LoaderCommunicationManager.DeviceReload(selectedstage);

                    //await _LoaderCommunicationManager.GetWaferObject(selectedstage);
                    //_LoaderCommunicationManager.GetProbeCardObject(selectedstage);
                    //_LoaderCommunicationManager.GetMarkObject(selectedstage);
                }

                //GetWaferObject(_SelectedStage);
                //GetProbeCardObject(_SelectedStage);
                //GetMarkObject(_SelectedStage);

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
                if (null == _SaveAsDeviceCommand) _SaveAsDeviceCommand = new AsyncCommand(FuncSaveAsDeviceCommand);
                return _SaveAsDeviceCommand;
            }
        }

        private async Task FuncSaveAsDeviceCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.DeviceChange_SaveAsDeviceCommand();
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
                if (null == _DeleteDeviceCommand) _DeleteDeviceCommand = new AsyncCommand(FuncDeleteDeviceCommand);
                return _DeleteDeviceCommand;
            }
        }

        private async Task FuncDeleteDeviceCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    EnumMessageDialogResult ret = EnumMessageDialogResult.UNDEFIND;

                    ret = await this.MetroDialogManager().ShowMessageDialog("[Device change]", "Are you sure you want to delete the device?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        await _RemoteMediumProxy.DeviceChange_DeleteDeviceCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ChangeDeviceCommand;
        public IAsyncCommand ChangeDeviceCommand
        {
            get
            {
                if (null == _ChangeDeviceCommand) _ChangeDeviceCommand = new AsyncCommand(FuncChangeDeviceCommand);
                return _ChangeDeviceCommand;
            }
        }

        private async Task FuncChangeDeviceCommand()
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    // 현재 디바이스와 선택된 디바이스(ShowingDevice)가 다른 경우 동작하도록 로직 추가

                    DeviceInfo curDev = ShowingDeviceInfoCollection.ToList().Find(x => x.IsNowDevice == true);

                    if(ShowingDevice != null && curDev != null)
                    {
                        //if (ShowingDevice.Name != curDev.Name)
                        //{
                            await _RemoteMediumProxy.DeviceChange_ChangeDeviceCommand();

                            await _LoaderCommunicationManager.DeviceReload(_LoaderCommunicationManager.SelectedStage);
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        //private void GetDevListCommandFunc()
        //{
        //    try
        //    {
        //        if (_RemoteMediumProxy != null)
        //        {
        //            _RemoteMediumProxy.Inspection_ChangeXManualCommand();
        //        }

        //        //GetDevList();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        #endregion

        //private AsyncGenericCommand<DeviceInfo> _GetParamFromDeviceCommand;
        //public IAsyncCommand<DeviceInfo> GetParamFromDeviceCommand
        //{
        //    get
        //    {
        //        if (null == _GetParamFromDeviceCommand) _GetParamFromDeviceCommand = new AsyncGenericCommand<DeviceInfo>(GetParamFromDevice);
        //        return _GetParamFromDeviceCommand;
        //    }
        //}
        private void Updateiscandevicechanged()
        {
            try
            {
                var Loader = this.GetLoaderContainer().Resolve<ILoaderModule>();
                IAttachedModule chuckModule = Loader.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, this._LoaderCommunicationManager.SelectedStage.Index);
                IWaferOwnable ownable = chuckModule as IWaferOwnable;
                if (ownable != null && ownable.Holder.Status == EnumSubsStatus.EXIST)
                {
                    SubstrateSizeEnum size = SubstrateSizeEnum.UNDEFINED;
                    switch (ShowingDevice.WaferSize)
                    {
                        case "12 Inch":
                            size = SubstrateSizeEnum.INCH12;
                            break;
                        case "8 Inch":
                            size = SubstrateSizeEnum.INCH8;
                            break;
                        case "6 Inch":
                            size = SubstrateSizeEnum.INCH6;
                            break;
                        case "INVALID":
                        default:
                            size = SubstrateSizeEnum.INVALID;
                            break;
                    }

                    if ((size != SubstrateSizeEnum.INVALID || size != SubstrateSizeEnum.UNDEFINED) && ownable.Holder.TransferObject.Size.Value != size)
                    {
                        this.IsCanChangeDevice = false;
                    }
                    else 
                    {
                        this.IsCanChangeDevice = true;
                    }
                }
                else 
                {
                    this.IsCanChangeDevice = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<DeviceInfo> _GetParamFromDeviceCommand;
        public IAsyncCommand GetParamFromDeviceCommand
        {
            get
            {
                if (null == _GetParamFromDeviceCommand)
                {
                    _GetParamFromDeviceCommand = new AsyncCommand<DeviceInfo>(GetParamFromDevice);
                    //_GetParamFromDeviceCommand.SetJobTask(WaitGetParamFromDevice);
                }
                return _GetParamFromDeviceCommand;
            }
        }

        private bool IsFinished = false;

        public async Task WaitGetParamFromDevice()
        {
            try
            {
                IsFinished = false;

                while (IsFinished == false)
                {
                    Thread.Sleep(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GetParamFromDevice(DeviceInfo device)
        {
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.GetParamFromDevice(device);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsFinished = true;
            }
        }

        public async Task GetParamFromDeviceWrapper(DeviceInfo device)
        {
            try
            {
                //await this.GetParamFromDeviceCommand.ExecuteAsync(device);

                await GetParamFromDevice(device);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GetParamFromDevice(DeviceInfo device)
        //{
        //    //if (_RemoteMediumProxy != null)
        //    //{
        //    //    await _RemoteMediumProxy.GetParamFromDevice(device);
        //    //}
        //}

        //private void UpdateDeviceChangeInfo()
        //{
        //    try
        //    {
        //        //var info = _RemoteMediumProxy.GetDeviceChangeInfo();

        //        //if (info != null)
        //        //{
        //        //    SearchStr = info.SearchStr;
        //        //    IsSearchDataClearButtonVisible = info.IsSearchDataClearButtonVisible;
        //        //    ShowingDeviceInfoCollection = info.ShowingDeviceInfoCollection;
        //        //    //SelectedDeviceInfo = info.SelectedDeviceInfo;
        //        //    ShowingDevice = info.ShowingDevice;
        //        //}
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public DeviceChangeDataDescription GetDeviceChangeInfo()
        {
            return null;
        }

        public Task ChangeDeviceFunc()
        {
            throw new NotImplementedException();
        }

        public Task CreateNewDeviceFunc()
        {
            throw new NotImplementedException();
        }

        public Task SaveAsDeviceFunc()
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeviceFunc()
        {
            throw new NotImplementedException();
        }

        public async Task SetShowingDeviceInfoCollectio(ObservableCollection<DeviceInfo> collection)
        {
            try
            {
                this.ShowingDeviceInfoCollection = collection;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task ChangeDeviceFunc(string deviceName)
        {
            throw new NotImplementedException();
        }
    }
}

