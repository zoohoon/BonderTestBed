

namespace DeviceModule
{
    using Autofac;
    using LogModule;
    using LoaderController.GPController;
    using ProberInterfaces.LoaderController;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Device;
    using ProberInterfaces.State;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using ProberInterfaces.Event;
    using ProberInterfaces.Command.Internal;

    public class DeviceModule : INotifyPropertyChanged, IDeviceModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Property

        #region .. IStateModule Property
        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.AirCooling);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }

        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }




        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private DeviceModuleState _DeviceModuleState;
        public DeviceModuleState DeviceModuleState
        {
            get { return _DeviceModuleState; }
        }

        public IInnerState InnerState
        {
            get { return _DeviceModuleState; }
            set
            {
                if (value != _DeviceModuleState)
                {
                    _DeviceModuleState = value as DeviceModuleState;
                    RaisePropertyChanged();
                }
            }
        }

        public IInnerState PreInnerState
        {
            get;
            set;
        }

        private List<DeviceInformation> _DeviceInfos
            = new List<DeviceInformation>();
        /// <summary>
        /// Load 할 Device, Reserve 된 Device 정보를 가지고 있음. ( Download 에 성공했을 때 List에 추가되고, Load 가 끝나면 List 에서 지워짐)
        /// </summary>
        public List<DeviceInformation> DeviceInfos
        {
            get { return _DeviceInfos; }
            set { _DeviceInfos = value; }
        }

        private DeviceInformation _NeedLoadDeviceInfo;
        // Load 하기위한 Device 정보
        public DeviceInformation NeedLoadDeviceInfo
        {
            get { return _NeedLoadDeviceInfo; }
            set { _NeedLoadDeviceInfo = value; }
        }

        private DeviceInformation _LoadedDeviceInfo;
        // DeviceModule 을 통해 Load 한 Device 정보 (마지막 적용된 Devie 정보)
        public DeviceInformation LoadedDeviceInfo
        {
            get { return _LoadedDeviceInfo; }
            set { _LoadedDeviceInfo = value; }
        }


        private bool _ReadyRecipe { get; set; } = true;

        //private DeviceLoadCheckInfomatin _DeviceLoadCheckInfo = new DeviceLoadCheckInfomatin();

        //public DeviceLoadCheckInfomatin DeviceLoadCheckInfo
        //{
        //    get { return _DeviceLoadCheckInfo; }
        //    set { _DeviceLoadCheckInfo = value; }
        //}


        #endregion

        #region .. IModule & IStateModule Method

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CommandRecvSlot = new CommandSlot();
                RunTokenSet = new CommandTokenSet();

                InnerState = new DeviceModuleIdleState(this);
                ModuleState = new ModuleUndefinedState(this);
                ModuleState.StateTransition(InnerState.GetModuleState());

                //ActiveDevices = new List<ActiveDevInfo>();
                //int foupnum = 3;
                //for (int idx = 1; idx <= foupnum; idx++)
                //{
                //    ActiveDevices.Add(new ActiveDevInfo(idx));
                //}
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
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

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {
                RetVal = _DeviceModuleState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum ClearState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;

            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }

            return stat;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                retval = InnerState.GetModuleState().ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                if (_ReadyRecipe == true)
                    retVal = true;
                else
                    retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool retVal = true;
            msg = null;
            try
            {
                bool devReady = false;
                // 즉시 Load 가 필요한 Device 가 없어야 한다.
                // LOT 시작 전에 Load 상태를 확인 한다. 
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    devReady = this.LotOPModule().LotInfo.GetDevResult(
                        foupnumber: this.GEMModule().GetPIVContainer().FoupNumber.Value,
                        lotid: this.LotOPModule().LotInfo.LotName.Value, getloadresult: true);
                    retVal = devReady && ModuleState.GetState() == ModuleStateEnum.IDLE;
                    if (retVal == false)
                    {
                        msg = $"Lot Device Download Fail.(Lot state : IDLE) ModuleState : {ModuleState.GetState()}" +
                            $" Lot id: {this.LotOPModule().LotInfo.LotName.Value}, " +
                            $" Foup number: {this.GEMModule().GetPIVContainer().FoupNumber.Value}\n";
                    }
                }
                else
                {
                    // LOT가 IDLE 이 아닌 경우에는 
                    devReady = this.LotOPModule().LotInfo.GetDevResult(
                          foupnumber: this.GEMModule().GetPIVContainer().FoupNumber.Value,
                        lotid: this.LotOPModule().LotInfo.LotName.Value, getdownresult: true);
                    if (devReady == false)
                    {
                        retVal = false;
                        msg = $"Lot Device Download Fail." +
                            $" Lot id: {this.LotOPModule().LotInfo.LotName.Value}, " +
                            $" Foup number: {this.GEMModule().GetPIVContainer().FoupNumber.Value}\n";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return InnerState.GetModuleState();
        }

        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        #endregion

        #region ..Method

        public bool GetReadyReicpeState()
        {
            return _ReadyRecipe;
        }
        public void SetReadyReicpeState(bool flag)
        {
            _ReadyRecipe = flag;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = _DeviceModuleState;
                InnerState = state;
                LoggerManager.Debug($"[Device Module] State Transition {PreInnerState.ToString()} => {InnerState.ToString()}");
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        public EventCodeEnum SetLoadReserveDevice(int foupnumber, string lotid, string cstHashCode)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ModuleState.GetState() != ModuleStateEnum.SUSPENDED | ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    if (NeedLoadDeviceInfo == null)
                    {
                        var deviceInfo = DeviceInfos.Find(info => info.FoupNumber == foupnumber && info.LotID.Equals(lotid) && info.LotCstHashCode.Equals(cstHashCode));

                        if (deviceInfo != null)
                        {
                            SetNeedLoadDeviceInfo(deviceInfo);
                        }

                        if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                        {
                            LoggerManager.Debug($"[SetLoadReserveDevice] LOTEND");
                            LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.DONE,
                                       $"Lot ID: {this.LotOPModule().LotInfo.LotName.Value}, Device:{this.FileManager().GetDeviceName()}," +
                                       $"Card ID:{this.CardChangeModule().GetProbeCardID()}"
                                       , this.LoaderController().GetChuckIndex());
                        }
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private object lockObj = new object();
        public bool _SetDevice = false;

        /// <summary>
        /// Device Download 명령이 들어왔을 때 현재 상태를 보고 Load 상황을 판단함.
        /// </summary>
        public EventCodeEnum SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool manualDownload = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            bool downloadInBuffer = false; // false : download 이후 즉시 load 안됨, true : download 이후 즉시 load 할 수 있음.

            try
            {
                LoggerManager.Debug($"[STAGE SET DEVICE] FoupNumber : {foupnumber}, LOTID : {lotid}");

                lock (lockObj)
                {
                    _SetDevice = true;

                    // Device Download 
                    string deviceZipName = $"{devicename}_{lotid}_ReserveFoup{foupnumber}";
                    string deviceHash = DateTime.Now.Ticks.GetHashCode().ToString();
                    string zippath = Path.Combine(this.FileManager().GetDeviceRootPath(), $"{deviceZipName}_{deviceHash}.zip");
                    

                    if (device != null)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(RecipeDownloadStartEvent).FullName, new ProbeEventArgs(this, semaphore, new PIVInfo() { FoupNumber = foupnumber, LotID = lotid, RecipeID = devicename }));
                        semaphore.Wait();
                        
                        //DeviceInfos 에 동일한 Foup에 대해서 예약된 Device 있는지 확인하고 있으면 삭제 함.
                        var sameFoupDevice = DeviceInfos.Find(info => info.FoupNumber == foupnumber && foupnumber > 0 & info.LotID.Equals(lotid));
                        if (sameFoupDevice != null)
                        {                           
                            if (sameFoupDevice.DeviceZipPath != "")
                            {
                                try
                                {
                                    var filenames = Directory.GetFiles(this.FileManager().GetDeviceRootPath(), "*.zip");
                                    foreach (var item in filenames)
                                    {
                                        if (item.Contains(deviceZipName) && item.Contains(NeedLoadDeviceInfo.DeviceHashcode) == false)
                                        {
                                            if (File.Exists(item))
                                            {
                                                File.Delete(item);
                                                LoggerManager.Debug($"[STAGE SET DEVICE] Delete ZipFile(sameFoupDevice) => path : {item}");

                                                DeviceInfos.Remove(sameFoupDevice);
                                                LoggerManager.Debug($"[Device Module] Remove device in list. because have duplicate foup number." +
                                                    $" LOTID : {sameFoupDevice.LotID}, FOUP NUM : {sameFoupDevice.FoupNumber}," +
                                                    $" DEVICE ID : {sameFoupDevice.DeviceName}, CST HASh CODE : {sameFoupDevice.LotCstHashCode}, DEVICE HASH:{deviceHash}");
                                            }
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err);
                                }

                            }
                        }

                        File.WriteAllBytes(zippath, device);
                        LoggerManager.Debug($"[STAGE SET DEVICE] Create ZipFile => path : {zippath}");

                        // Device Download 상태 판단. 
                        CheckDownloadCondition(out downloadInBuffer, foupnumber, lotCstHashCode);

                        DeviceInfos.Add(new DeviceInformation(foupnumber, lotid, lotCstHashCode, devicename, zippath, !downloadInBuffer, deviceHash));

                        LoggerManager.Debug($"[DeviceModule].DeviceInfos Added." +
                            $" LOTID : {lotid}, FOUP NUM : {foupnumber}, CST HASh CODE : {lotCstHashCode}, DEVICE ID : {devicename}, DEVICE HASH:{deviceHash}");

                        SetDeviceDownloadResult(foupnumber, lotid, devicename, true, downloadInBuffer);
                    }
                    else
                    {
                        SetDeviceDownloadResult(foupnumber, lotid, devicename, false, downloadInBuffer);
                    }
                }
            }
            catch (Exception err)
            {
                SetDeviceDownloadResult(foupnumber, lotid, devicename, false, downloadInBuffer);

                //Error Message & Buzzer
                this.MetroDialogManager().ShowMessageDialog("Error Message", $"Stage Receipe Download Fail", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                this.LoaderController().BroadcastLotState(true);

                this.NotifyManager().Notify(EventCodeEnum.DEVICE_CHANGE_FAIL);
                LoggerManager.Exception(err);
            }
            finally
            {
                _SetDevice = false;
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).ConfigureAwait(false);
                LoggerManager.Debug($"[STAGE SET DEVICE END] FoupNumber : {foupnumber}, LOTID : {lotid}");
            }
            return retVal;
        }

        public EventCodeEnum LoadDevice()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string devicename = NeedLoadDeviceInfo.DeviceName;
            int foupnumber = NeedLoadDeviceInfo.FoupNumber;
            string lotid = NeedLoadDeviceInfo.LotID;
            string device_hash = NeedLoadDeviceInfo.DeviceHashcode;
            string deviceZipPath = NeedLoadDeviceInfo.DeviceZipPath;
            try
            {
                this.LoaderController().SetTitleMessage(this.LoaderController().GetChuckIndex(), "LOAD DEVICE START");
                this.LotOPModule().LotInfo.SetDevLoadResult(false, foupnumber);

                // <!-- zip file 로 있던 파일을 압축 해제하여 device folder 로 만듬.  -->
                // 이전에 동일한 이름의 device 가 있었다면 삭제 후, 압축 해제 함.
                
                try
                {
                    if (File.Exists(deviceZipPath) == true)
                    {
                        string devicePath = this.FileManager().GetDeviceRootPath() + "\\" + devicename;
                        DeleteDirectory(devicePath);
                        LoggerManager.Debug($"[STAGE SET DEVICE] Delet Exist Device => device : {devicename}");


                        ZipFile.ExtractToDirectory(deviceZipPath, Path.Combine(this.FileManager().GetDeviceRootPath(), devicename));
                        LoggerManager.Debug($"[STAGE SET DEVICE] Extract Device => zippath : {deviceZipPath}, device : {devicename}");
                    }
                    else
                    {
                        SetDeviceLoadResult(foupnumber, lotid, devicename, false);
                        retVal = EventCodeEnum.UNDEFINED;
                        return retVal;
                    }
                }
                catch (Exception err)
                {
                    SetDeviceLoadResult(foupnumber, lotid, devicename, false);
                    LoggerManager.Exception(err);
                    retVal = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                LoggerManager.Debug($"[STAGE LOAD DEVICE] FoupNumber : {foupnumber}, DEVICE : {devicename}");
                PIVInfo pivinfo = new PIVInfo() { FoupNumber = foupnumber, LotID = lotid, RecipeID = devicename };
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(StageRecipeReadStartEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();

                if (devicename?.Length > 0)
                {
                    this.ParamManager().DevDBElementDictionary.Clear();
                    var modules = this.GetContainer().Resolve<IEnumerable<IFactoryModule>>().Where(module => module is IHasDevParameterizable);
                    var waferid = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;
                    var proercardid = this.CardChangeModule().GetProbeCardID();
                    var waferstatus = this.GetParam_Wafer().WaferStatus;

                    retVal = this.FileManager().ChangeDevice(devicename);

                    if (retVal == EventCodeEnum.NONE)
                    {
                        foreach (var v in modules)
                        {
                            try
                            {
                                IHasDevParameterizable module = v as IHasDevParameterizable;
                                retVal = module.LoadDevParameter();
                                retVal = module.InitDevParameter();
                            }
                            catch (Exception err)
                            {
                                retVal = EventCodeEnum.DEVICE_CHANGE_FAIL;
                                LoggerManager.Exception(err);
                                throw new Exception($"Error Module is {v}");
                            }
                        }


                        this.ParamManager().LoadDevElementInfoFromDB();
                        this.SoakingModule().SetChangedDeviceName(this.LotOPModule().LotInfo.DeviceName.Value, devicename);
                        this.GEMModule().GetPIVContainer().SetLotID(lotid);
                        this.GEMModule().GetPIVContainer().ProberType.Value = this.FileManager().GetProberID();
                        this.EventManager().RaisingEvent(typeof(DeviceChangedEvent).FullName);
                        this.LotOPModule().SetDeviceName(devicename);



                        IProbingModule ProbingModule = this.ProbingModule();
                        ProbingModule.SetProbingMachineIndexToCenter();

                        //this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardID.Value = proercardid;

                        this.LotOPModule().UpdateWaferID(waferid);
                        this.GetParam_Wafer().SetWaferStatus(waferstatus);

                        this.GEMModule().GetPIVContainer().SetWaferID(this.GetParam_Wafer().GetSubsInfo().WaferID.Value);

                        if (this.CardChangeModule() != null && this.CardChangeModule().GetProbeCardID() != null)
                        {
                            this.GEMModule().GetPIVContainer().SetProberCardID(this.CardChangeModule().GetProbeCardID());
                        }
                        this.GEMModule().GetPIVContainer().SetTemperature.Value = this.TempController().TempInfo.SetTemp.Value;

                        this.LoaderController().SetDeviceName(this.LoaderController().GetChuckIndex(), this.FileManager().GetDeviceName());
                        int originfoupnumber = this.GEMModule().GetPIVContainer().FoupNumber.Value;
                        if (this.FileManager().GetDeviceName().Equals(devicename))
                        {
                            SetDeviceLoadResult(foupnumber, lotid, devicename, true);
                        }
                        else
                        {
                            SetDeviceLoadResult(foupnumber, lotid, devicename, false);
                        }

                        if (NeedLoadDeviceInfo.NeedChangeParameterInfo != null)
                        {
                            SetParemterFromNeedChangeParameter(NeedLoadDeviceInfo.NeedChangeParameterInfo);
                        }

                        ///Load 가 끝난 Recipe는 리스트에서 삭제
                        if (retVal == EventCodeEnum.NONE)
                        {
                            DeviceInfos.Remove(NeedLoadDeviceInfo);

                            LoadedDeviceInfo = new DeviceInformation(NeedLoadDeviceInfo.FoupNumber, NeedLoadDeviceInfo.LotID, NeedLoadDeviceInfo.DeviceName, NeedLoadDeviceInfo.DeviceZipPath, NeedLoadDeviceInfo.NeedImmediateLoad);
                            LoggerManager.Debug($"[Device Module] Remove device in list. LOTID : {lotid}, FOUP NUM : {foupnumber}, DEVICE ID : {devicename}, Device Hash:{NeedLoadDeviceInfo.DeviceHashcode}");

                            if (NeedLoadDeviceInfo.DeviceZipPath != "")
                            {
                                File.Delete(NeedLoadDeviceInfo.DeviceZipPath);
                                LoggerManager.Debug($"[STAGE SET DEVICE] Delete ZipFile => path : {NeedLoadDeviceInfo.DeviceZipPath}");
                            }
                        }
                        else
                        {
                            // TODO : H사 시나리오, Remove
                            if (string.IsNullOrEmpty(NeedLoadDeviceInfo.LotID) && NeedLoadDeviceInfo.FoupNumber == 0)
                            {
                                DeviceInfos.Remove(NeedLoadDeviceInfo);
                            }
                        }
                    }
                    else if (retVal == EventCodeEnum.NOT_EXIST_DEVICE)
                    {
                        SetDeviceLoadResult(foupnumber, lotid, devicename, false);
                        retVal = EventCodeEnum.DEVICE_CHANGE_FAIL;
                    }
                }

                void DeleteDirectory(string path)
                {
                    try
                    {
                        if (Directory.Exists(path) == true)
                        {
                            foreach (string directory in Directory.GetDirectories(path))
                            {
                                DeleteDirectory(directory);
                            }

                            try
                            {
                                Directory.Delete(path, true);
                            }
                            catch (IOException)
                            {
                                Directory.Delete(path, true);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                Directory.Delete(path, true);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[STAGE LOAD DEVICE END - OK] FoupNumber : {foupnumber}, DEVICE : {devicename}");
                    this.LoaderController().SetTitleMessage(this.LoaderController().GetChuckIndex(), "LOAD DEVICE END");
                }
                else
                {
                    LoggerManager.Debug($"[STAGE LOAD DEVICE END - FAIL] FoupNumber : {foupnumber}, DEVICE : {devicename}");
                    this.LoaderController().SetTitleMessage(this.LoaderController().GetChuckIndex(), "LOAD DEVICE FAIL");
                    this.NotifyManager().Notify(EventCodeEnum.DEVICE_CHANGE_FAIL);
                }
            }
            return retVal;
        }

        public void SetDeviceDownloadResult(int foupNumber, string lotId, string deviceName, bool result, bool isReserve, bool manualDownload = false)
        {
            try
            {
                SemaphoreSlim semaphore = null;
                int originfoupnumber = this.GEMModule().GetPIVContainer().FoupNumber.Value;
                PIVInfo pivinfo = new PIVInfo(foupnumber: foupNumber, originloadportnumber: originfoupnumber, receipeid: deviceName, lotid: lotId);

                this.LotOPModule().LotInfo.SetDevDownResult(result, foupNumber, deviceName, lotId);

                if (manualDownload == false)
                {
                    if (isReserve)
                    {
                        if (result)
                        {
                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(RecipeDownloadSucceededEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(StageRecipeDownloadSuccededEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        else
                        {
                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(RecipeDownloadFailedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(StageRecipeDownloadFailedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            this.MetroDialogManager().ShowMessageDialog("Error Message", $"Stage Receipe Download Fail", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                            this.LoaderController().BroadcastLotState(true);
                        }
                    }
                    else
                    {
                        if (result)
                        {
                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(RecipeDownloadSucceededEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(StageRecipeDownloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        else
                        {
                            semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(RecipeDownloadFailedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                    }
                }

                if (isReserve == false && result == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message", $"Stage Receipe Download Fail", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    this.LoaderController().BroadcastLotState(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDeviceLoadResult(int foupNumber, string lotId, string deviceName, bool result)
        {
            try
            {
                SemaphoreSlim semaphore = null;
                int originfoupnumber = this.GEMModule().GetPIVContainer().FoupNumber.Value;
                PIVInfo pivinfo = new PIVInfo(foupnumber: foupNumber, originloadportnumber: originfoupnumber, receipeid: deviceName, lotid: lotId);


                this.LotOPModule().LotInfo.SetDevLoadResult(result, foupNumber, deviceName, lotId);
                if (result)
                {
                    semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageRecipeReadCompleteEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    //Module 이 Error 상태고, Load 해야 했던 Device 이름과 Load 한 Device 가 동일하다면 Load를 해야하는 Device 정보를 초기화 하고, 상태를 초기화 한다.
                    if (ModuleState.GetState() == ModuleStateEnum.ERROR)
                    {
                        if (NeedLoadDeviceInfo != null)
                        {
                            if (NeedLoadDeviceInfo.DeviceName.Equals(deviceName))
                            {
                                NeedLoadDeviceInfo = null;
                                ClearState();
                            }
                        }
                    }
                }
                else
                {
                    semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageRecipeReadFailedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }

                this.LoaderController().SetDeviceLoadResult(result);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CheckDownloadCondition(out bool downloadInBuffer, int foupNumber = 0, bool isWriteLog = true)
        {
            bool retVal = false;
            downloadInBuffer = true;

            try
            {
                var GetRunState = this.SequenceEngineManager().GetRunState();  //RunList중 Running, Error, Pending인게 있거나
                var IsRequested_IDOWAFERALIGN = this.WaferAligner().CommandRecvSlot.IsRequested<IDOWAFERALIGN>();
                var IsRequested_IDOSamplePinAlignForSoaking = this.PinAligner().CommandRecvSlot.IsRequested<IDOSamplePinAlignForSoaking>();
                var IsRequested_IDOPinAlignAfterSoaking = this.PinAligner().CommandRecvSlot.IsRequested<IDOPinAlignAfterSoaking>();
                var IsRequested_IDOPINALIGN = this.PinAligner().CommandRecvSlot.IsRequested<IDOPINALIGN>();

                var TransferReservationAboutPolishWafer = this.LotOPModule().TransferReservationAboutPolishWafer;//Soaking PW 이송 Flag
                var SoakingModuleState = this.SoakingModule().ModuleState.GetState();

                // Lot 가 Idle 상태일때 Soaking 이 동작중인지를 확인하기 위한 조건
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE
                    && (GetRunState == false
                    || SoakingModuleState == ModuleStateEnum.RUNNING //GetRunState()에서 확인하지만 Double Check
                    || IsRequested_IDOWAFERALIGN //Wafer Align Command가 요청되어 있거나
                    || IsRequested_IDOSamplePinAlignForSoaking //Sample Pin aling Command가 요청되어 있거나
                    || IsRequested_IDOPinAlignAfterSoaking //Soaking후 동작하는 Pin align Command가 요청되어 있거나
                    || IsRequested_IDOPINALIGN//Pin align Command가 요청되어 있거나
                    || (TransferReservationAboutPolishWafer == true && SoakingModuleState == ModuleStateEnum.SUSPENDED)))//Soaking에 의한 PW가 이송중이거나
                {
                    downloadInBuffer = false;
                    if (isWriteLog)
                    {
                        LoggerManager.Debug($"[DeviceModule] SetDevice(), checkRunState is false. GetRunState:{GetRunState}, IsRequested_IDOWAFERALIGN : {IsRequested_IDOWAFERALIGN}, IsRequested_IDOSamplePinAlignForSoaking : {IsRequested_IDOSamplePinAlignForSoaking}," +
                                              $"IsRequested_IDOPinAlignAfterSoaking : {IsRequested_IDOPinAlignAfterSoaking}, IsRequested_IDOPINALIGN : {IsRequested_IDOPINALIGN}" +
                                              $"TransferReservationAboutPolishWafer : {TransferReservationAboutPolishWafer}, SoakingModule State : {SoakingModuleState}");
                    }
                }
                else
                {
                    if (isWriteLog)
                    {
                        LoggerManager.Debug($"[DeviceModule] SetDevice(), checkRunState is true. GetRunState:{GetRunState}, IsRequested_IDOWAFERALIGN : {IsRequested_IDOWAFERALIGN}, IsRequested_IDOSamplePinAlignForSoaking : {IsRequested_IDOSamplePinAlignForSoaking}," +
                                              $"IsRequested_IDOPinAlignAfterSoaking : {IsRequested_IDOPinAlignAfterSoaking}, IsRequested_IDOPINALIGN : {IsRequested_IDOPINALIGN}" +
                                              $"TransferReservationAboutPolishWafer : {TransferReservationAboutPolishWafer}, SoakingModule State : {SoakingModuleState}");
                    }
                }
                //------------------------

                ///Load 된 Device 의 Lot정보가 있다면 해당 Lot 가 끝날때까지 다른 Device 가 Load 되면 안된다.
                bool existLotOfLoadedDeviceAssigned = false;
                var stageLotInfos = this.LotOPModule().LotInfo.GetLotInfos();
                if (LoadedDeviceInfo != null)
                {
                    var assignLotInfo = stageLotInfos.Find(lotInfo => lotInfo.CassetteHashCode.Equals(LoadedDeviceInfo.LotCstHashCode)
                    && lotInfo.LotID.Equals(LoadedDeviceInfo.LotID) && lotInfo.FoupIndex == LoadedDeviceInfo.FoupNumber);
                    if (assignLotInfo != null)
                    {
                        existLotOfLoadedDeviceAssigned = true;
                        if (downloadInBuffer == false)
                        {
                            downloadInBuffer = true;
                        }
                    }
                }

                bool existOtherDevice = false;
                foreach (var actdevice in DeviceInfos)
                {
                    if (actdevice.FoupNumber != foupNumber)
                    {
                        if (actdevice.DeviceName != "")
                        {
                            existOtherDevice = true;
                            LoggerManager.Debug($"SetDevice() FoupNumber : {actdevice.FoupNumber}, Device : {actdevice.DeviceName} is exist");
                        }
                    }
                }


                ModuleStateEnum lotState = this.LotOPModule().ModuleState.GetState();
                GPCellModeEnum stageMode = this.StageSupervisor().StageMode;
                bool isRecvLotOPStartCommand = this.LotOPModule().CommandRecvSlot.IsRequested<ILotOpStart>();

                ///Recipe가 Load 중이거나 Lot 가 Run 중일때는 로드하지 않고, Recipe 보관
                ///_SetDevice 는 Download 중인 Device 가 있다는 것.
                /// existOtherDevice 는 예약되거나, Load 중인 Device 가 존재하는지에 대한 확인용 ( true면 다른 Device 가 존재한 다는 것)
                /// IsNeedLoadTmpDevice 는 soaking 으로 인해 
                /// existOtherAssignLot : Load 된 Device 의 LOT 가 아직 LOT 할당이 해제 되지 않은 상태
                if (lotState != ModuleStateEnum.IDLE || isRecvLotOPStartCommand || existOtherDevice || ModuleState.GetState() != ModuleStateEnum.IDLE || existLotOfLoadedDeviceAssigned)
                {
                    downloadInBuffer = true;
                }
                else
                {
                    downloadInBuffer = false;
                }

                LoggerManager.Debug($"[DeviceModule] Check");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public bool CheckDownloadCondition(out bool downloadInBuffer, int foupNumber, string cstHashCode)
        {
            bool retVal = false;
            downloadInBuffer = true;

            try
            {
                ///Load 된 Device 의 Lot정보가 있다면 해당 Lot 가 끝날때까지 다른 Device 가 Load 되면 안된다.
                /// TODO : LOT 의 할당상태를 확인하는 조건 추가 필요
                bool existLotOfLoadedDeviceAssigned = false;
                var stageLotInfos = this.LotOPModule().LotInfo.GetLotInfos();
                foreach (var lotInfo in stageLotInfos)
                {
                    if (lotInfo.FoupIndex != foupNumber && !lotInfo.CassetteHashCode.Equals(cstHashCode) && lotInfo.FoupIndex != 0)
                    {
                        /// 먼저 할당 된 다른 LOT 정보가 있으면 안됨,
                        existLotOfLoadedDeviceAssigned = true;
                        break;
                    }
                    else
                    {
                        if (lotInfo.FoupIndex == foupNumber && lotInfo.CassetteHashCode.Equals(cstHashCode))
                        {
                            /// Download 하려는 정보와 동일한 정보까지만 확인하면 됨.
                            break;
                        }
                    }
                }

                bool existOtherDevice = false;
                foreach (var actdevice in DeviceInfos)
                {
                    if (actdevice.FoupNumber != foupNumber && foupNumber != 0)
                    {
                        if (actdevice.DeviceName != "")
                        {
                            existOtherDevice = true;
                            LoggerManager.Debug($"SetDevice() FoupNumber : {actdevice.FoupNumber}, Device : {actdevice.DeviceName} is exist");
                        }
                    }
                }


                ModuleStateEnum lotState = this.LotOPModule().ModuleState.GetState();
                GPCellModeEnum stageMode = this.StageSupervisor().StageMode;
                bool isRecvLotOPStartCommand = this.LotOPModule().CommandRecvSlot.IsRequested<ILotOpStart>();

                ///Recipe가 Load 중이거나 Lot 가 Run 중일때는 로드하지 않고, Recipe 보관
                ///_SetDevice 는 Download 중인 Device 가 있다는 것.
                /// existOtherDevice 는 예약되거나, Load 중인 Device 가 존재하는지에 대한 확인용 ( true면 다른 Device 가 존재한 다는 것)
                /// IsNeedLoadTmpDevice 는 soaking 으로 인해 
                /// existOtherAssignLot : Load 된 Device 의 LOT 가 아직 LOT 할당이 해제 되지 않은 상태
                if (lotState != ModuleStateEnum.IDLE || ModuleState.GetState() != ModuleStateEnum.IDLE || isRecvLotOPStartCommand || existOtherDevice || existLotOfLoadedDeviceAssigned)
                {
                    downloadInBuffer = true;
                }
                else
                {
                    downloadInBuffer = false;
                }

                LoggerManager.Debug($"[DeviceModule] Check");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        /// <summary>
        /// DeviceList 에서 Load 가 필요한 Device 가 있는지 확인 및 설정.
        /// </summary>
        public bool IsNeedDeviceLoad()
        {
            bool retVal = false;
            try
            {
                if (NeedLoadDeviceInfo == null)
                {
                    /// LOT 할당 Device 가 아니라면 LOT 정보 상관 없이 Device 를 Load 할 수 있도록 데이터를 설정한다. 
                    var devInfo = DeviceInfos.Find(info => info.FoupNumber == 0);

                    if (devInfo != null)
                    {
                        SetNeedLoadDeviceInfo(devInfo);
                        retVal = true;
                    }

                    if (NeedLoadDeviceInfo == null)
                    {
                        /// StageLotInfo 의 첫번째 Lot 데이터의 Device 가 Download 는 되었지만 Load 는 안되었고,
                        /// DeviceInfos 데이터와 일치하는 데이터가 있다면 해당 Device 를 Load 한다.
                        /// TODO : LOT 의 할당상태를 확인하는 조건 추가 필요

                        var stageLotInfos = this.LotOPModule().LotInfo.GetLotInfos();

                        if (stageLotInfos != null)
                        {
                            var lotInfo = stageLotInfos.FirstOrDefault();

                            if (lotInfo != null)
                            {
                                if (lotInfo.DevDownResult && !lotInfo.DevLoadResult)
                                {
                                    if (String.IsNullOrEmpty(lotInfo.CassetteHashCode) == false)
                                    {
                                        devInfo = DeviceInfos.Find(info => info.LotCstHashCode.Equals(lotInfo.CassetteHashCode));
                                    }
                                    else
                                    {
                                        devInfo = DeviceInfos.Find(info => info.FoupNumber.Equals(lotInfo.FoupIndex));
                                    }

                                    if (devInfo != null)
                                    {
                                        if (lotInfo.RecipeID.Equals(devInfo.DeviceName) && lotInfo.LotID.Equals(devInfo.LotID))
                                        {
                                            SetNeedLoadDeviceInfo(devInfo);
                                            retVal = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (NeedLoadDeviceInfo == null)
                    {
                        /// 현재 LOT 정보와 일치하는 Device 가 있는지 확인 한다. 
                        var needLoadDevice = DeviceInfos.Find(info => info.LotID.Equals(this.LotOPModule().LotInfo.LotName.Value) && info.LotCstHashCode.Equals(this.LotOPModule().LotInfo.CSTHashCode));

                        if (needLoadDevice != null)
                        {
                            SetNeedLoadDeviceInfo(needLoadDevice);
                            retVal = true;
                        }
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// Device 를 Load 해도 되는 상태인지 확인.
        /// retVal1 : Device Load하기 위한 조건
        /// retVal2 : Device Load하기 위한 Soaking Module의 조건
        /// retVal3 : Wafer Exist 상태에 따른 조건
        /// </summary>
        public DeviceLoadCheckInformation IsCanDeviceLoad()
        {
            DeviceLoadCheckInformation deviceLoadCheckInfo = new DeviceLoadCheckInformation();
            try
            {
                lock (lockObj)
                {
                    //if (DeviceLoadCheckInfo == null)
                    //{
                    //    DeviceLoadCheckInfo = new DeviceLoadCheckInfomatin();
                    //}
                    if (NeedLoadDeviceInfo != null)
                    {
                        var isLotModuleState = this.LotOPModule().ModuleState.GetState();

                        if (isLotModuleState != ModuleStateEnum.ABORT
                            && this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE
                            && this.StageSupervisor().SysState().GetSysState() != EnumSysState.SETUP
                            && this.MonitoringManager().IsMachineInitDone == true
                            && _SetDevice == false
                            && this.SequenceEngineManager().GetRunState()
                            && IsLotHasSuspendedState() == false)
                        {
                            bool isAutomationRunning = (this.LoaderController() as GP_LoaderController).GPLoaderService.IsActiveCCAllocatedState(this.LoaderController().GetChuckIndex());

                            if (isAutomationRunning == false)
                            {
                                deviceLoadCheckInfo.SequenceEngineResult = true;
                            }

                        }

                        SoakingStateEnum stateEnum = SoakingStateEnum.UNDEFINED;
                        deviceLoadCheckInfo.SoakingResult = this.SoakingModule().IsDeviceLoadpossible(out stateEnum);

                        if (this.GetParam_Wafer().GetStatus() == EnumSubsStatus.NOT_EXIST)
                        {
                            deviceLoadCheckInfo.WaferStatusResult = true;
                        }
                        else
                        {
                            // Chuck 에 Wafer 가 있더라도 Wafer의 Origin위치와 LOT 정보가 일치하면 device load를 허용한다.
                            string cstHashCodeInSlotInformation = this.StageSupervisor().GetSlotInfo().CSTHashCode;

                            if (NeedLoadDeviceInfo.LotCstHashCode.Equals(cstHashCodeInSlotInformation))
                            {
                                deviceLoadCheckInfo.WaferStatusResult = true;
                            }
                            else
                            {
                                deviceLoadCheckInfo.WaferStatusResult = false;
                            }
                        }

                        bool isLotModuleIdle = isLotModuleState == ModuleStateEnum.IDLE;
                        bool isLotIdEmpty = string.IsNullOrEmpty(NeedLoadDeviceInfo.LotID);

                        deviceLoadCheckInfo.LotIDResult = isLotModuleIdle || !isLotIdEmpty;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return deviceLoadCheckInfo;
        }

        public bool IsHaveDontCareLotReservationRecipe()
        {
            try
            {
                var reserveRecipe = DeviceInfos.FindAll(info => info.FoupNumber <= 0).Count();
                if (reserveRecipe > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }


        public bool IsLotHasSuspendedState()
        {
            bool isExistSuspendStateModule = false;
            try
            {

                int suspendedStateCount = 0;

                suspendedStateCount = this.LotOPModule().RunList.Count(item => item?.ModuleState.GetState() == ModuleStateEnum.SUSPENDED);

                if (this.ModuleState.GetState() == ModuleStateEnum.SUSPENDED)
                {
                    suspendedStateCount -= 1;
                }

                if (this.SoakingModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED)
                {
                    suspendedStateCount -= 1;
                }

                isExistSuspendStateModule = suspendedStateCount > 0 ? true : false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return isExistSuspendStateModule;
        }

        public void ClearActiveDeviceDic(int foupnumber, string lotid, string cstHashCode)
        {
            try
            {
                if (ModuleState.GetState() == ModuleStateEnum.ERROR)
                {
                    if (NeedLoadDeviceInfo != null)
                    {
                        if (NeedLoadDeviceInfo.DeviceName.Equals(this.FileManager().GetDeviceName()))
                        {
                            ClearState();
                            LoggerManager.Debug($"[DeviceModule] ClearActiveDeviceDic() - ClearState();");
                        }
                    }
                }

                var activeDev = DeviceInfos.Find(dev => dev.FoupNumber == foupnumber && dev.LotID.Equals(lotid) && dev.LotCstHashCode.Equals(cstHashCode));
                if (activeDev != null)
                {
                    DeviceInfos.Remove(activeDev);

                    if (NeedLoadDeviceInfo == activeDev)
                    {
                        SetNeedLoadDeviceInfo(null);
                    }

                    LoggerManager.Debug($"[DeviceModule].ClearDeviceInfo success. foupnumber :{foupnumber}, lotid : {lotid}, cstHashCode :{cstHashCode} not exist");
                }
                else
                {
                    activeDev = DeviceInfos.Find(dev => dev.FoupNumber == foupnumber && dev.LotID.Equals(lotid));

                    LoggerManager.Debug($"[DeviceModule].ClearDeviceInfo fail. foupnumber :{foupnumber}, lotid : {lotid} not exist");

                    if (activeDev != null)
                    {
                        DeviceInfos.Remove(activeDev);

                        if (NeedLoadDeviceInfo == activeDev)
                        {
                            SetNeedLoadDeviceInfo(null);
                        }

                        LoggerManager.Debug($"[DeviceModule].ClearDeviceInfo fail. foupnumber :{foupnumber}, exist lotid : {activeDev.LotID}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsHaveReservationRecipe(int foupnumber, string lotid, string cstHashCode = "")
        {
            try
            {
                DeviceInformation reserveRecipe = null;

                if (lotid != "")
                {
                    if (cstHashCode != "")
                    {
                        reserveRecipe = DeviceInfos.Find(info => info.FoupNumber == foupnumber && info.LotID.Equals(lotid) && info.LotCstHashCode.Equals(cstHashCode));
                    }
                    else
                    {
                        reserveRecipe = DeviceInfos.Find(info => info.FoupNumber == foupnumber && info.LotID.Equals(lotid));
                    }
                }
                else
                {
                    reserveRecipe = DeviceInfos.Find(info => info.FoupNumber == foupnumber);
                }

                if (reserveRecipe != null)
                {
                    if (ModuleState.GetState() != ModuleStateEnum.RUNNING && this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                    {
                        if (NeedLoadDeviceInfo != null)
                        {
                            LoggerManager.Debug($"[DEVICE MODULE] Have Reservation Reciepe => FoupNumber : {foupnumber}, LOTID : {reserveRecipe.LotID}");
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        public void SetNeedLoadDeviceInfo(DeviceInformation devInfo)
        {
            try
            {
                if (devInfo != null)
                {
                    NeedLoadDeviceInfo = devInfo;
                    LoggerManager.Debug($"[DeviceModule] SetNeedLoadDeviceInfo(). LOTID : {devInfo.LotID}, FOUP NUM : {devInfo.FoupNumber}, DEVICE ID : {devInfo.DeviceName}");
                }
                else
                {
                    NeedLoadDeviceInfo = devInfo;
                    LoggerManager.Debug($"[DeviceModule] SetNeedLoadDeviceInfo(). NeedLoadDeviceInfo Set to null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 예약된 Device 의 압축 파일들을 삭제하고 리스트를 초기화 시킨다. 
        /// </summary>
        public void RemoveAllReservationRecipe()
        {
            try
            {
                if (DeviceInfos != null)
                {
                    foreach (var info in DeviceInfos)
                    {
                        LoggerManager.Debug($"[DEVICE MODULE] RemoveAllReservationRecipe() LOTID : {info.LotID}, FOUPIDX : {info.FoupNumber}, DEVICE : {info.DeviceName}");              

                        if (File.Exists(info.DeviceZipPath))
                        {
                            try
                            {
                                File.Delete(info.DeviceZipPath);
                                LoggerManager.Debug($"[DEVICE MODULE] RemoveAllReservationRecipe() Delete ZipFile => path : {info.DeviceZipPath}");
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                                LoggerManager.Debug($"[DEVICE MODULE] RemoveAllReservationRecipe() occur error when delete file => path : {info.DeviceZipPath}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[DEVICE MODULE] RemoveAllReservationRecipe() Not exist ZipFile => path : {info.DeviceZipPath}");
                        }
                    }

                    DeviceInfos.Clear();

                    LoggerManager.Debug("[DEVICE MODULE] RemoveAllReservationRecipe() : Clear DeviceInfos");

                    if (NeedLoadDeviceInfo != null)
                    {
                        SetNeedLoadDeviceInfo(null);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void RemoveSpecificReservationRecipe()
        {
            try
            {
                if (DeviceInfos != null && DeviceInfos.Count > 0)
                {
                    var itemsToRemove = new List<DeviceInformation>();

                    // 조건에 맞지 않는 항목을 찾아 리스트에 추가
                    foreach (var info in DeviceInfos)
                    {
                        bool hasValidLotID = !string.IsNullOrEmpty(info.LotID);
                        bool hasValidFoupNumber = info.FoupNumber > 0;

                        if (hasValidLotID && hasValidFoupNumber)
                        {
                            itemsToRemove.Add(info);
                        }
                    }

                    // 조건에 맞지 않는 항목을 DeviceInfos에서 삭제
                    foreach (var item in itemsToRemove)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], RemoveSpecificReservationRecipe(), LOTID : {item.LotID}, FOUPIDX : {item.FoupNumber}, DEVICE : {item.DeviceName}");

                        DeviceInfos.Remove(item);                        

                        try
                        {
                            if (File.Exists(item.DeviceZipPath))
                            {
                                File.Delete(item.DeviceZipPath);

                                LoggerManager.Debug($"[{this.GetType().Name}], RemoveSpecificReservationRecipe(), Delete ZipFile => path : {item.DeviceZipPath}");
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}], RemoveSpecificReservationRecipe(), Removed {itemsToRemove.Count} specific DeviceInfos based on conditions");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (lockObj)
                {
                    if (needChangeParameter != null)
                    {
                        int foupNumber = needChangeParameter.FoupNumber;
                        string lotID = needChangeParameter.LOTID;
                        string deviceId = needChangeParameter.DeviceName;

                        //예약된 Device 정보에 일치하는 정보가 있는지 먼저 확인
                        var deviceInfo = DeviceInfos.Find(info => info.FoupNumber == foupNumber && info.LotID.Equals(lotID) && info.DeviceName.Equals(deviceId));

                        if (deviceInfo != null)
                        {
                            deviceInfo.NeedChangeParameterInfo = needChangeParameter;

                            LoggerManager.Debug($"[Device Module] SetNeedChangeParaemterInDeviceInfo() set NeedChangeParameterInfo. LOTID : {lotID}, FOUP NUM : {foupNumber}, DEVICE ID : {deviceId}");

                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            if (LoadedDeviceInfo != null)
                            {
                                if (LoadedDeviceInfo.FoupNumber == foupNumber && LoadedDeviceInfo.LotID.Equals(lotID) && LoadedDeviceInfo.DeviceName.Equals(deviceId))
                                {
                                    if (ModuleState.GetState() == ModuleStateEnum.IDLE || ModuleState.GetState() == ModuleStateEnum.DONE)
                                    {
                                        // 적용되어 있는 Device 의 정보와 같다면, 바로 적용
                                        SetParemterFromNeedChangeParameter(needChangeParameter);
                                        retVal = EventCodeEnum.NONE;
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[Device Module] SetNeedChangeParaemterInDeviceInfo() exception. DeviceInfos has no information, does not match the LoadedDeviceInfo information. LOTID : {lotID}, FOUP NUM : {foupNumber}, DEVICE ID : {deviceId}");
                                }
                            }
                            else
                            {
                                // 이곳으로 들어오는 경우는 예외상황임 LOT 에연관된 Device 가 없는데 Paraemter 만 설정 할 수 없음.
                                LoggerManager.Debug($"[Device Module] SetNeedChangeParaemterInDeviceInfo() exception. DeviceInfos and LoadedDeviceInfo has no information. LOTID : {lotID}, FOUP NUM : {foupNumber}, DEVICE ID : {deviceId}");
                            }

                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[Device Module] SetNeedChangeParaemterInDeviceInfo() exception. needChangeParameter is null.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetParemterFromNeedChangeParameter(NeedChangeParameterInDevice needChangeParameter)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (needChangeParameter != null)
                {
                    if (needChangeParameter.ElementParameters != null)
                    {
                        foreach (var elementParam in needChangeParameter.ElementParameters)
                        {
                            if (elementParam != null)
                            {
                                string elementPropertyPath = "";
                                if (String.IsNullOrEmpty(elementParam.PropertyPath) == false)
                                {
                                    elementPropertyPath = elementParam.PropertyPath;
                                }
                                else if (elementParam.VID != 0)
                                {
                                    elementPropertyPath = this.ParamManager().GetPropertyPathFromVID(elementParam.VID);
                                }

                                if (String.IsNullOrEmpty(elementPropertyPath) == false)
                                {
                                    bool isNeedApplyValueFromHost = true;
                                    this.ParamManager().SetOriginValue(elementParam.PropertyPath, elementParam.Value, isEqualsValue: false, valueChangedParam: isNeedApplyValueFromHost);

                                    LoggerManager.Debug($"[Device Module] SetParemterFromNeedChangeParameter() SetValue. ElementPath : {elementParam.PropertyPath}, Value : {elementParam.Value}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsExistSetTempParemterFromNeedChangeParameter()
        {
            bool retVal = false;
            try
            {
                if (ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    if (NeedLoadDeviceInfo != null && NeedLoadDeviceInfo.NeedChangeParameterInfo != null)
                    {
                        ElementParameterInfomation setTempElementInfo = NeedLoadDeviceInfo.NeedChangeParameterInfo.ElementParameters.Find(parameter => parameter.PropertyPath.Contains("TempController.TempControllerDevParam.SetTemp"));

                        if (setTempElementInfo != null)
                        {
                            retVal = true;
                            LoggerManager.Debug($"[DeviceModule] IsExistSetTempParemterFromNeedChangeParameter() retVal : true, SV = {setTempElementInfo.Value} ");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion
    }

    public class DeviceLoadCheckInformation
    {
        private bool _SequenceEngineResult;
        public bool SequenceEngineResult
        {
            get { return _SequenceEngineResult; }
            set { _SequenceEngineResult = value; }
        }

        private bool _SoakingResult;
        public bool SoakingResult
        {
            get { return _SoakingResult; }
            set { _SoakingResult = value; }
        }

        private bool _WaferStatusResult;
        public bool WaferStatusResult
        {
            get { return _WaferStatusResult; }
            set { _WaferStatusResult = value; }
        }

        private bool _LotIDResult;
        public bool LotIDResult
        {
            get { return _LotIDResult; }
            set { _LotIDResult = value; }
        }
    }
}
