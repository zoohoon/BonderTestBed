using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.LoaderLog;
using LoaderCore;
using LoaderMaster.ExternalStates;
using LoaderMaster.InternalStates;
using LoaderMaster.LoaderSupervisorStates;
using LoaderMaster.StageEmul;
using LoaderParameters;
using LoaderParameters.Data;
using LoaderServiceBase;
using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.DialogControl;
using ProberInterfaces.Enum;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using SecsGemServiceInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using ProberInterfaces.Proxies;
using System.Diagnostics;
using System.IO;
using ProberInterfaces.Monitoring;
using ProberInterfaces.Device;
using LoaderRecoveryControl;

namespace LoaderMaster
{
    public class LoaderSupervisor : ILoaderSupervisor, INotifyPropertyChanged
    {

        public static readonly int StageCount = (SystemModuleCount.ModuleCnt.StageCount > 0) ? SystemModuleCount.ModuleCnt.StageCount : 12;
        public bool Initialized { get; set; } = false;
        public bool LoaderSystemInitFailure { get; set; } = false;
        public Autofac.IContainer cont;
        public static Dictionary<string, ILoaderServiceCallback> Clients = new Dictionary<string, ILoaderServiceCallback>();
        public bool[] isLotStartFlag = new bool[12];
        public DateTime LotStartTime;
        Dictionary<string, ILoaderServiceCallback> ILoaderSupervisor.ClientList => Clients;
        public Dictionary<int, object> ClientsLock = new Dictionary<int, object>();
        Dictionary<int, object> ILoaderSupervisor.ClientListLock => ClientsLock;

        private IGPLoader _GPLoader => cont?.Resolve<IGPLoader>() ?? null;

        public IGPLoader GPLoader
        {
            get { return _GPLoader; }
        }


        private ILoaderModule _Loader => cont?.Resolve<ILoaderModule>() ?? null;

        public ILoaderModule Loader
        {
            get { return _Loader; }
        }
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        private bool isStopThreadReq = false;

        public int PMIIntervalCount = 5;
        private ILoaderCommunicationManager loaderCommunicationManager => cont.Resolve<ILoaderCommunicationManager>();
        public ILoaderLogManagerModule LoaderLogManager => cont.Resolve<ILoaderLogManagerModule>();
        public ILoaderDoorDisplayDialogService LoaderDoorDialog => cont.Resolve<ILoaderDoorDisplayDialogService>();

        public IEnvControlManager EnvControlManager => cont.Resolve<IEnvControlManager>();

        public LoaderMapSlicer MapSlicer { get; set; }
        private LoaderSupervisorStateBase _StateObj;

        public LoaderSupervisorStateBase StateObj
        {
            get { return _StateObj; }
            set
            {
                _StateObj = value;
                if (_StateObj is LoaderSupervisorInternalStateBase)
                {
                    Mode = LoaderMasterMode.Internal;
                }
                else if (_StateObj is LoaderSupervisorExternalStateBase)
                {
                    Mode = LoaderMasterMode.External;
                }
            }

        }

        private List<ActiveLotInfo> _ActiveLotInfos = new List<ActiveLotInfo>();
        public List<ActiveLotInfo> ActiveLotInfos
        {
            get { return _ActiveLotInfos; }
            set
            {
                if (value != _ActiveLotInfos)
                {
                    _ActiveLotInfos = value;
                }
            }
        }
        private List<ActiveLotInfo> _Prev_ActiveLotInfos = new List<ActiveLotInfo>();
        public List<ActiveLotInfo> Prev_ActiveLotInfos
        {
            get { return _Prev_ActiveLotInfos; }
            set
            {
                if (value != _Prev_ActiveLotInfos)
                {
                    _Prev_ActiveLotInfos = value;
                }
            }
        }

        private List<ActiveLotInfo> _BackupActiveLotInfos = new List<ActiveLotInfo>();
        //Foup Unload 이후, Cassette 가 없어지기 전까지의 Lot 정보를 Backup 해둔다.
        public List<ActiveLotInfo> BackupActiveLotInfos
        {
            get { return _BackupActiveLotInfos; }
            set
            {
                if (value != _BackupActiveLotInfos)
                {
                    _BackupActiveLotInfos = value;
                }
            }
        }

        private List<AbortStageInformation> _LotAbortStageInfos = new List<AbortStageInformation>();
        public List<AbortStageInformation> LotAbortStageInfos
        {
            get { return _LotAbortStageInfos; }
            set
            {
                if (value != _LotAbortStageInfos)
                {
                    _LotAbortStageInfos = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private Dictionary<String, List<ActiveLotInfo>> _ExternalLotJob;
        //public Dictionary<String, List<ActiveLotInfo>> ExternalLotJob
        //{
        //    get { return _ExternalLotJob; }
        //    set
        //    {
        //        if (value != _ExternalLotJob)
        //        {
        //            _ExternalLotJob = value;
        //        }
        //    }
        //}


        private Dictionary<String, List<ActiveLotInfo>> _ExternalLotJob;
        public Dictionary<String, List<ActiveLotInfo>> ExternalLotJob
        {
            get { return _ExternalLotJob; }
            set
            {
                if (value != _ExternalLotJob)
                {
                    _ExternalLotJob = value;
                }
            }
        }
        private List<ILoaderServiceCallback> _IgnoreCellList = new List<ILoaderServiceCallback>();
        public List<ILoaderServiceCallback> IgnoreCellList
        {
            get { return _IgnoreCellList; }
            set
            {
                if (value != _IgnoreCellList)
                {
                    _IgnoreCellList = value;
                }
            }
        }

        private ActiveLotInfo _SelectedLotInfo;
        public ActiveLotInfo SelectedLotInfo
        {
            get { return _SelectedLotInfo; }
            set
            {
                if (value != _SelectedLotInfo)
                {
                    _SelectedLotInfo = value;
                }
            }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Loader);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                }
            }
        }

        public CommandSlot CommandRecvSlot { get; set; } = new CommandSlot();
        public CommandSlot CommandRecvProcSlot { get; set; } = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot { get; set; } = new CommandSlot();
        public CommandSlot CommandSendSlot { get; set; } = new CommandSlot();

        public CommandTokenSet RunTokenSet { get; set; } = new CommandTokenSet();
        public CommandInformation CommandInfo { get; set; }
        private ModuleStateEnum _CurrentModuleState;
        public ModuleStateEnum CurrentModuleState
        {
            get { return _CurrentModuleState; }
            set
            {
                if (value != _CurrentModuleState)
                {
                    _CurrentModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<ModuleStateEnum> StageStates { get; } = new List<ModuleStateEnum>();

        public List<bool> StagesIsTempReady { get; } = new List<bool>();


        public List<double> StageSetTemp { get; set; } = new List<double>();


        public ModuleStateEnum StageSetSoakingState { get; set; } = new ModuleStateEnum();


        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get
            {
                return _ModuleState;
            }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    CurrentModuleState = _ModuleState.GetState();
                    RaisePropertyChanged();
                }
            }
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



        private ObservableCollection<IStageObject> _CellsInfo = new ObservableCollection<IStageObject>();
        public ObservableCollection<IStageObject> CellsInfo
        {
            get { return (loaderCommunicationManager).Cells; }
            set
            {
                if (value != (loaderCommunicationManager).Cells)
                {
                    //((LoaderCommunicationManager)loaderCommunicationManager).Cells = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<int, bool> _StopBeforeProbingDictionary = new Dictionary<int, bool>();
        public Dictionary<int, bool> StopBeforeProbingDictionary
        {
            get { return _StopBeforeProbingDictionary; }
            set
            {
                if (value != _StopBeforeProbingDictionary)
                {
                    _StopBeforeProbingDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Dictionary<int, bool> _StopAfterProbingDictionary = new Dictionary<int, bool>();
        public Dictionary<int, bool> StopAfterProbingDictionary
        {
            get { return _StopAfterProbingDictionary; }
            set
            {
                if (value != _StopAfterProbingDictionary)
                {
                    _StopAfterProbingDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EnumModuleForcedState ForcedDone { get; set; } = EnumModuleForcedState.Normal;


        public HolderModuleInfo[] BufferModuleInfo = new HolderModuleInfo[4];

        public LoaderLotSysParameter LotSysParam { get; set; }

        public bool StatusSoakingUpdateInfoStop { get; set; } = false;

        public bool IsLoaderJobDoneWait { get; set; } = false;

        public bool StageWatchDogStop { get; set; } = false;
        public bool MapSlicerErrorFlag = false;
        //추후 HOST에 대한 Action으로 만들려면 새로운 Enum 정의해서 사용하는 변수로 바꾸면 좋을 듯 함. 현재는 wafer change 에 대한 동작에 대한 옵션으로 사용됨.
        public bool HostInitiatedWaferChangeInProgress { get; set; } = false; 
        public LoaderSupervisor()
        {

        }

        /// <summary>
        /// Client 의 chuckIndex 는 1 부터 시작.
        /// </summary>
        /// <param name="chuckIndex"></param>
        /// <param name="bCalledDisconnect">Disconnect에서 호출했는지 여부</param>
        /// <returns></returns>
        public ILoaderServiceCallback GetClient(int chuckIndex, bool bCalledDisconnect = false)
        {

            ILoaderServiceCallback client = null;
            try
            {
                string chuckkey = $"CHUCK{chuckIndex}";
                Clients.TryGetValue(chuckkey, out client);
                if (client != null)
                {
                    // 자동 연결이 켜켜 있는 상태라면 이라는 조건 추가
                    if (client is StageEmulation)
                    {

                    }
                    else if (bCalledDisconnect || !this.GEMModule().GemCommManager.GetRemoteConnectState(chuckIndex)
                        || (client as ICommunicationObject).State == CommunicationState.Faulted
                        || (client as ICommunicationObject).State == CommunicationState.Closed)
                    {
                        //Brett// stagewatchdog thread에서 ClientList를 참조하여 Reconnect 시도하므로 리스트에서 삭제는 하지 않는다.
                        client = null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return client;
        }
        public StageLotData GetStageLotData(int stageIndex)
        {
            StageLotData data = null;

            try
            {
                var client = GetClient(stageIndex);

                if (client == null)
                {
                    data = new StageLotData();
                    data.ConnectState = "Off";
                }
                else
                {
                    data = GetStageLotDataClient(client);
                }

                if (data != null)
                {
                    data.RenewTime = DateTime.Now;

                    data.DataCollect();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return data;
        }
        public bool GetTesterAvailableData(int stageIndex)
        {
            bool retval = false;

            try
            {
                var client = GetClient(stageIndex);

                if (client != null)
                {
                    if (IsAliveClient(client))
                    {
                        retval = client.GetTesterAvailableData();
                    }
                    else
                    {
                        LoggerManager.Error($"[LoaderSupervisor], GetTesterAvailableData() : Failed");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum SetStageMoveLockState(int stageIndex, ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    retVal = SetStageMoveLockStateClient(client, reason);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetStageMoveUnLockState(int stageIndex, ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    retVal = SetStageMoveUnLockStateClient(client, reason);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetStageMode(int stageIndex, GPCellModeEnum mode)

        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    retVal = SetStageModeClient(client, mode);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetCellModeChanging(int stageIndex)

        {
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    SetCellModeChangingClient(client);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ResetCellModeChanging(int stageIndex)

        {
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    ResetCellModeChangingClient(client);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public GPCellModeEnum GetStageMode(int stageIndex)

        {
            GPCellModeEnum retVal = GPCellModeEnum.DISCONNECT;
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    var stageInfo = client.GetStageInfo();
                    retVal = stageInfo.CellMode;
                    LoggerManager.Debug($"GetStageMode CellIndex={stageIndex} ,StageMode:{retVal}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetStreamingMode(int stageIndex, StreamingModeEnum mode)
        {
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    SetStreamingModeClient(client, mode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void RemoveClientAtList(int chuckIndex)
        {
            try
            {
                string chuckkey = $"CHUCK{chuckIndex}";
                Clients.Remove(chuckkey);
                //ClientsLock 은 remove 하지 않고 재활용 하기 위해 남겨 둔다. 어차피 최대 cell 개수 만큼만 할당됨
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private int GetIndexFromStageIP(string ip)
        {
            int index = -1;
            try
            {
                if (ip.Equals("192.168.3.14"))
                    return 1;
                else if (ip.Equals("192.168.3.20"))
                    return 2;
                else if (ip.Equals("192.168.3.12"))
                    return 3;
                else if (ip.Equals("192.168.3.13"))
                    return 4;
                else if (ip.Equals("192.168.3.30"))
                    return 12;
                else if (ip.Equals("::1"))
                    return 4;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return index;
        }


        public EventCodeEnum Connect(string sessionId, ILoaderServiceCallback loaderController)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                var index = loaderController.GetChuckID();
                Clients[sessionId] = loaderController;                
                loaderCommunicationManager.Cells[index.Index - 1].StageMode = GPCellModeEnum.ONLINE;
                loaderCommunicationManager.Cells[index.Index - 1].IsStageModeChanged = true;

                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method executed Cell({index}) Connected");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum CellWaferRefresh(int cellIdx)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                var WaferState = GetChuckWaferStatusClient(GetClient(cellIdx));

                if (WaferState == EnumSubsStatus.EXIST)
                {
                    var ChuckModules = Loader.ModuleManager.FindModules<IChuckModule>();
                    var chuckModule = ChuckModules.FirstOrDefault(i => i.ID.Index == cellIdx);
                    chuckModule.Holder.SetAllocate();
                    //chuckModule.Holder.SetUnload();
                    chuckModule.Holder.TransferObject.OCR.Value = GetWaferIDClient(GetClient(cellIdx));
                    int slotIndex = GetSlotIndexClient(GetClient(cellIdx));

                    chuckModule.Holder.TransferObject.OriginHolder = Loader.GetLoaderInfo().StateMap.ChuckModules[cellIdx - 1].Substrate.OriginHolder;

                    // ISSD-4838
                    // Loader Homing 또는 Refresh 동작에서 Chuck Module이 가지고 있는 TransferObject 객체가 새로 만들어 지면서
                    // 메인 화면에 바인딩 되어 있는 Loader.Foups.Slot.WaferObj 가 업데이트 되지 않아 UI가 갱신되지 않는 문제로 인해
                    // 해당 부분에서 새로 만들어진 TransferObject를 업데이트 해준다.
                    int slotNum = 0;
                    int foupNum = 0;
                    if (chuckModule.Holder.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        slotNum = chuckModule.Holder.TransferObject.OriginHolder.Index % 25;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        foupNum = ((chuckModule.Holder.TransferObject.OriginHolder.Index + offset) / 25) + 1;

                        int slotCnt = Loader.Foups[foupNum - 1].Slots.Count();

                        Loader.Foups[foupNum - 1].Slots[slotCnt - slotNum].WaferObj = chuckModule.Holder.TransferObject;
                    }
                    
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    var ChuckModules = Loader.ModuleManager.FindModules<IChuckModule>();
                    var chuckModule = ChuckModules.FirstOrDefault(i => i.ID.Index == cellIdx);
                    chuckModule.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum CellCardRefresh(int cellIdx)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                var CardState = GetCardStatusClient(GetClient(cellIdx), out cardStateEnum);

                if (CardState == EnumSubsStatus.EXIST)
                {
                    var CCModules = Loader.ModuleManager.FindModules<ICCModule>();
                    var CCModule = CCModules.FirstOrDefault(i => i.ID.Index == cellIdx);
                    CCModule.Holder.SetAllocate();
                    CCModule.Holder.TransferObject.ProbeCardID.Value = GetProbeCardIDClient(GetClient(cellIdx));
                    CCModule.Holder.TransferObject.WaferState = cardStateEnum;
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    var CCModules = Loader.ModuleManager.FindModules<ICCModule>();
                    var CCModule = CCModules.FirstOrDefault(i => i.ID.Index == cellIdx);
                    CCModule.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void StageSoakingMode()
        {
            try
            {
                ILoaderServiceCallback client = this.GetClient(loaderCommunicationManager.SelectedStageIndex);

                StageSetSoakingState = client.GetSetSoakingState();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"StageMode(): Error occurred. Failed to get soaking status from stage. Err = {err.Message}");
            }
        }

        public void InitStageWaferObject(int nCellIndex)
        {
            string chuckkey = $"CHUCK{nCellIndex}";
            ILoaderServiceCallback client = null;
            bool ExistKey = Clients.TryGetValue(chuckkey, out client);
            if (!ExistKey)
            {
                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Cell{nCellIndex} is not connected");
                return;
            }
            try
            {
                //bool isWaferMissing = false;
                var WaferState = GetChuckWaferStatusClient(client);
                if (WaferState == EnumSubsStatus.EXIST)
                {
                    bool bNeedUpdate = false;
                    IStageObject stage = null;
                    stage = this.loaderCommunicationManager.GetStage(nCellIndex);
                    if (stage.WaferStatus != EnumSubsStatus.EXIST)
                    {
                        LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Cell{nCellIndex} wafer status change {stage.WaferStatus}=>{EnumSubsStatus.EXIST}");
                        stage.WaferStatus = EnumSubsStatus.EXIST;
                        bNeedUpdate = true;
                    }

                    if (IsHandlerholdWafer(client) == true)
                    {
                        var ChuckModules = Loader.ModuleManager.FindModules<IChuckModule>();
                        var chuckModule = ChuckModules.FirstOrDefault(i => i.ID.Index == GetChuckIDClient(client).Index);
                        if (chuckModule.Holder.Status != EnumSubsStatus.EXIST)
                        {
                            chuckModule.Holder.SetAllocate();
                            chuckModule.Holder.TransferObject.OCR.Value = GetWaferIDClient(client);
                            int slotIndex = GetSlotIndexClient(client);
                            chuckModule.Holder.TransferObject.OriginHolder = GetOriginHolderClient(client);
                        }
                        chuckModule.Holder.IsWaferOnHandler = true;
                    }
                    else
                    {
                        var ChuckModules = Loader.ModuleManager.FindModules<IChuckModule>();
                        var chuckModule = ChuckModules.FirstOrDefault(i => i.ID.Index == GetChuckIDClient(client).Index);

                        //loader가 정보를 가지고 있다면 그대로 사용하고 없다면 cell의 정보를 가져와 사용하도록 한다.
                        if (chuckModule.Holder.Status != EnumSubsStatus.EXIST)
                        {
                            chuckModule.Holder.SetAllocate();
                            chuckModule.Holder.TransferObject.OCR.Value = GetWaferIDClient(client);
                            int slotIndex = GetSlotIndexClient(client);
                            chuckModule.Holder.TransferObject.OriginHolder = GetOriginHolderClient(client);
                        }
                        Loader.BroadcastLoaderInfo();
                    }
                    if (bNeedUpdate)
                    {
                        var device = client.GetDeviceInfo();
                        if (stage.WaferObj != null)  //waferobject는 broadcastloaderinfo에서 채워 준다.
                        {
                            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Cell{nCellIndex} wafer type change {stage.WaferObj.WaferType.Value}=>{device.WaferType.Value}");
                            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Cell{nCellIndex} wafer size change {stage.WaferObj.Size.Value}=>{device.Size.Value}");
                            stage.WaferObj.WaferType.Value = device.WaferType.Value;
                            stage.WaferObj.Size.Value = device.Size.Value;
                            Loader.BroadcastLoaderInfo();
                        }
                    }
                }
                else
                {
                    if (IsHandlerholdWafer(client) == true)
                    {
                        var ChuckModules = Loader.ModuleManager.FindModules<IChuckModule>();
                        var chuckModule = ChuckModules.FirstOrDefault(i => i.ID.Index == GetChuckIDClient(client).Index);
                        if (chuckModule.Holder.Status != EnumSubsStatus.EXIST)
                        {
                            chuckModule.Holder.SetAllocate();
                            chuckModule.Holder.TransferObject.OCR.Value = GetWaferIDClient(client);
                            int slotIndex = GetSlotIndexClient(client);
                            chuckModule.Holder.TransferObject.OriginHolder = GetOriginHolderClient(client);
                        }
                        chuckModule.Holder.IsWaferOnHandler = true;
                    }
                    else
                    {
                        var ChuckModules = Loader.ModuleManager.FindModules<IChuckModule>();
                        var chuckModule = ChuckModules.FirstOrDefault(i => i.ID.Index == GetChuckIDClient(client).Index);
                        if (WaferState == EnumSubsStatus.UNKNOWN)
                        {
                            chuckModule.Holder.SetUnknown();
                            //isWaferMissing = true;
                        }
                        else
                        {
                            chuckModule.Holder.SetUnload();
                        }
                    }
                    Loader.BroadcastLoaderInfo();

                }
                EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                var CardState = GetCardStatusClient(client, out cardStateEnum);
                if (CardState == EnumSubsStatus.EXIST)
                {
                    var CCModules = Loader.ModuleManager.FindModules<ICCModule>();
                    var CCModule = CCModules.FirstOrDefault(i => i.ID.Index == GetChuckIDClient(client).Index);
                    CCModule.Holder.SetAllocate();
                    CCModule.Holder.TransferObject.ProbeCardID.Value = GetProbeCardIDClient(client);
                    CCModule.Holder.TransferObject.WaferState = cardStateEnum;
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    var CCModules = Loader.ModuleManager.FindModules<ICCModule>();
                    var CCModule = CCModules.FirstOrDefault(i => i.ID.Index == GetChuckIDClient(client).Index);
                    CCModule.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();
                }

                //if (isWaferMissing)
                //{
                //    GPLoader.LoaderBuzzer(true);
                //    this.MetroDialogManager().ShowMessageDialog($"Cell{chuckModule.ID.Index} Wafer Missing Error", $"Please check the existence of Wafer. Is there a Wafer inside Cell{chuckModule.ID.Index}?", EnumMessageStyle.AffirmativeAndNegative,"EXIST","NOT EXIST");

                //}


            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Cell{nCellIndex} get wafer or card status failed.");
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 실시간 데이터 업데이트를 위함.
        /// </summary>
        public void UpdateGetWaferObject()
        {
            try
            {
                for (int index = 0; index < StageCount; index++)
                {
                    string chuckkey = $"CHUCK{index + 1}";

                    ILoaderServiceCallback client = null;

                    bool ExistKey = Clients.TryGetValue(chuckkey, out client);

                    if (ExistKey == true)
                    {
                        var stageobj = loaderCommunicationManager.Cells[index];
                        //Check Set Temp & Reserve Pause state
                        if (!stageobj.StageInfo.IsConnectChecking && IsAliveClient(client))
                        {
                            StageSetTemp[index] = client.GetSetTemp();
                            if (stageobj.StageInfo.IsReservePause)
                            {
                                stageobj.StageInfo.IsReservePause = client.GetReservePause();
                            }

                            //Check Temp state & Error
                            var tempobj = loaderCommunicationManager.GetProxy<ITempControllerProxy>(index + 1);
                            if (tempobj != null)
                            {
                                var cellInfo = loaderCommunicationManager.Cells[index].StageInfo;
                                //var isTimeOut = tempobj.GetIsOccurTimeOutError();
                                //cellInfo.IsOccurTempError = isTimeOut;
                                //if(isTimeOut)
                                //{
                                cellInfo.DewPoint = tempobj.GetCurDewPointValue();
                                cellInfo.PV = tempobj.GetTemperature();
                                //}
                            }
                        }
                    }
                    else
                    {
                        StageSetTemp[index] = -999;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GEMDisconnectCallback(long lStageID)
        {
            //추후 MatchedToTestEvent raise와 wafer load/unload transfer중 네트워크 끊김에 대한 추가 처리가 되면 loader에 wcf 채널을 일괄적으로 끊는 로직을 다시 살리도록 한다.
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Abort Proxy Cell({lStageID}) Start");
            bool ret = loaderCommunicationManager.AbortStage((int)lStageID, false);
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Abort Proxy Cell({lStageID}) End result={ret}");
        }

        /// <summary>
        /// Connected 상태에서 연결이 끊어진 Cell에 대해 재연결 여부를 시도하는 함수
        /// </summary>
        public async Task StageWatchDog()
        {
            int nSleepTime = 10000;
            try
            {
                for (int index = 0; index < StageCount; index++)
                {
                    int nCellIndex = index + 1;
                    string chuckkey = $"CHUCK{nCellIndex}";
                    ILoaderServiceCallback client = null;
                    bool ExistKey = Clients.TryGetValue(chuckkey, out client);
                    if (ExistKey)
                    {
                        IStageObject obj = loaderCommunicationManager.Cells[index];
                        if (obj == null)
                            continue;

                        if (!obj.StageInfo.IsConnectChecking && !IsAliveClient(client, 30))
                        {
                            /*
                            brett gem이 종료된경우 끊어진 경우에만 reconnecting 시도를 하도록 한다.
                            if (this.GEMModule().GemCommManager.GetRemoteConnectState(nCellIndex))
                            {
                                continue;
                            }
                            */

                            nSleepTime = 30000;
                            try
                            {
                                obj.Reconnecting = true;
                                if (loaderCommunicationManager.IsAliveStageSupervisor(nCellIndex))
                                {
                                    Thread.Sleep(1000);
                                    //brett// 다른 thread(사용자 action)으로 다시 연결하는  경우 도 있을 수 있다.
                                    if (obj.StageInfo.IsConnected && !obj.StageInfo.IsConnectChecking)
                                    {
                                        loaderCommunicationManager.AbortStage(nCellIndex); //비정상적인 기존 proxy를 abort 한다.
                                        Thread.Sleep(1000);

                                        LoggerManager.Debug($"StageWatchDog() try Reconnect Stage({nCellIndex})");
                                        bool success = await loaderCommunicationManager.ConnectStage(nCellIndex, true);
                                        if (success)
                                        {
                                            //lot data 정보를 새로 얻어와야 한다.
                                            obj.StageInfo.LotData = GetStageLotData(nCellIndex);
                                        }
                                        else
                                        {
                                            loaderCommunicationManager.AbortStage(nCellIndex); //연결실패한 잔재 proxy를 abort한다.
                                        }
                                        LoggerManager.Debug($"StageWatchDog() Reconnect Stage({nCellIndex}) result={success}");
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                LoggerManager.Debug($"StageWatchDog() Reconnect Fail stage({nCellIndex})");
                                loaderCommunicationManager.AbortStage(nCellIndex); //연결실패한 잔재 proxy를 abort한다.
                            }
                        }
                        else
                        {
                            obj.Reconnecting = false;
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
                for (int i = 0; i < nSleepTime; i++)
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Status Soaking Chilling Time을 실시간 업데이트 하기 위함.
        /// </summary>
        public void UpdateGetStatusSoakingInfo()
        {
            try
            {
                for (int index = 0; index < StageCount; index++)
                {
                    string chuckkey = $"CHUCK{index + 1}";

                    ILoaderServiceCallback client = null;

                    bool ExistKey = Clients.TryGetValue(chuckkey, out client);

                    if (!ExistKey || loaderCommunicationManager.Cells[index].StageInfo.IsConnectChecking || !IsAliveClient(client))
                    {
                        continue;
                    }

                    long _ChillingTimeMax = 100;
                    long _ChillingTime = 0;
                    SoakingStateEnum CurStatusSoaking_State = SoakingStateEnum.UNDEFINED;
                    client.ReadStatusSoakingChillingTime(ref _ChillingTimeMax, ref _ChillingTime, ref CurStatusSoaking_State);

                    loaderCommunicationManager.Cells[index].ChillingTimeMax = _ChillingTimeMax;
                    loaderCommunicationManager.Cells[index].ChillingTime = _ChillingTime;

                    Visibility ChillingTimeProgressBarVisibility = Visibility.Hidden;
                    if (CurStatusSoaking_State == SoakingStateEnum.MAINTAIN)
                        ChillingTimeProgressBarVisibility = Visibility.Visible;
                    else if (CurStatusSoaking_State == SoakingStateEnum.STATUS_EVENT_SOAK || CurStatusSoaking_State == SoakingStateEnum.MANUAL)
                    {
                        //Event soaking이나 Manual soaking 일때는 _ChillingTime이 0이 아닌지를 보고 hide 처리 여부를 결정한다.
                        if (_ChillingTime > 0)
                            ChillingTimeProgressBarVisibility = Visibility.Visible;
                    }

                    loaderCommunicationManager.Cells[index].ChillingTimeProgressBar_Visibility = ChillingTimeProgressBarVisibility;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        System.Timers.Timer _monitoringTimer;
        LoaderInfo LoaderInfo;
        Thread UpdateThread;
        Thread StatusSoakingInfoUpdateThread;
        Thread StageWatchDogThread;
        private static int MonitoringInterValInms = 100;

        private bool _ForcedLotEndFlag;
        public bool ForcedLotEndFlag
        {
            get { return _ForcedLotEndFlag; }
            set
            {
                if (value != _ForcedLotEndFlag)
                {
                    if (value == true)
                    {
                        if (this.ModuleState != null && this.ModuleState.State == ModuleStateEnum.ABORT)
                        {
                            _ForcedLotEndFlag = value;
                        }
                        else
                        {
                            _ForcedLotEndFlag = false;
                        }
                    }
                    else
                    {
                        _ForcedLotEndFlag = value;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ContinueLotFlag;
        public bool ContinueLotFlag
        {
            get { return _ContinueLotFlag; }
            set
            {
                if (value != _ContinueLotFlag)
                {
                    if (IsSuperUser)
                    {
                        _ContinueLotFlag = value;
                        LoggerManager.Debug($"[Lot Option Setting] Continue LOT : {_ContinueLotFlag}");
                    }
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsSuperUser;
        public bool IsSuperUser
        {
            get { return _IsSuperUser; }
            set
            {
                _IsSuperUser = value;
                RaisePropertyChanged();
            }
        }

        private string _CardIDLastTwoWord;
        public string CardIDLastTwoWord
        {
            get { return _CardIDLastTwoWord; }
            set
            {
                _CardIDLastTwoWord = value;
                RaisePropertyChanged();
            }
        }

        private string _CardIDFullWord;
        public string CardIDFullWord
        {
            get { return _CardIDFullWord; }
            set
            {
                _CardIDFullWord = value;
                RaisePropertyChanged();
            }
        }

        private bool _OCRDebugginFlag;
        public bool OCRDebugginFlag
        {
            get { return _OCRDebugginFlag; }
            set
            {
                if (value != _OCRDebugginFlag)
                {

                    if (value != _OCRDebugginFlag)
                    {

                        if (IsSuperUser)
                        {
                            _OCRDebugginFlag = value;
                            LoggerManager.Debug($"[Lot Option Setting] OCRDebugging Mode : {_ContinueLotFlag}");
                        }
                        RaisePropertyChanged();
                    }
                }
            }
        }

        private LoaderMasterMode _Mode;
        public LoaderMasterMode Mode
        {
            get { return _Mode; }
            set
            {
                if (value != _Mode)
                {

                    _Mode = value;

                    RaisePropertyChanged();
                }
            }
        }

        private DynamicModeEnum _DynamicMode;
        public DynamicModeEnum DynamicMode
        {
            get { return _DynamicMode; }
            set
            {
                if (value != _DynamicMode)
                {

                    _DynamicMode = value;
                    RaisePropertyChanged();
                    if (this.GEMModule() != null)
                    {
                        if (_DynamicMode == DynamicModeEnum.DYNAMIC)
                            this.GEMModule().LoadPortModeEnable = true;
                        else
                            this.GEMModule().LoadPortModeEnable = false;
                    }
                }
            }
        }





        public EventCodeEnum FoupShiftMode_AfterValueChangedBehavior(string propertypath, IElement element, Object oldVal, Object val, out string errorlog, bool ecvchangedevent = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errmsg = "";
            try
            {
                this.GEMModule().GetPIVContainer().FoupShiftMode.Value = (int)element.GetValue();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errorlog = errmsg;
            }
            return retVal;

        }

        public string ConvertIntToFoupShift(object input)
        {
            string retVal = input.ToString();
            try
            {
                if (input.ToString() == ((int)FoupShiftModeEnum.NORMAL).ToString())
                {
                    retVal = FoupShiftModeEnum.NORMAL.ToString();
                }
                else if (input.ToString() == ((int)FoupShiftModeEnum.SHIFT).ToString())
                {
                    retVal = FoupShiftModeEnum.SHIFT.ToString();
                }
                else
                {
                    //input.ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string ConvertToFoupShiftToInt(object input)
        {
            string retVal = input.ToString();
            int ret = 0;
            try
            {
                var isSameReport = int.TryParse(input.ToString(), out ret);
                if (isSameReport == false)
                {
                    if (input.ToString() == FoupShiftModeEnum.NORMAL.ToString())
                    {
                        retVal = "1";
                    }
                    else if (input.ToString() == FoupShiftModeEnum.SHIFT.ToString())
                    {
                        retVal = "0";
                    }
                    else
                    {
                        //input.ToString();
                    }
                }
                else
                {
                    //same current retVal
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }





        public void SetFoupShiftMode(int mode)
        {
            if (mode == 1)
            {
                LotSysParam.FoupShiftMode.Value = FoupShiftModeEnum.SHIFT;
            }
            else if (mode == 0)
            {
                LotSysParam.FoupShiftMode.Value = FoupShiftModeEnum.NORMAL;
            }
        }


        public void SetMapSlicerLotPause(bool val)
        {
            try
            {
                if (MapSlicer != null)
                {
                    MapSlicer.isLotPause = val;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool _IsExternal;
        public bool IsExternal
        {
            get { return _IsExternal; }
            set
            {
                if (value != _IsExternal)
                {

                    _IsExternal = value;
                    if (_IsExternal == false)
                    {
                        if (!(StateObj is LoaderSupervisorInternalStateBase))
                        {
                            StateObj = InternalStateBase;
                        }
                    }
                    else
                    {
                        if (!(StateObj is LoaderSupervisorExternalStateBase))
                        {
                            StateObj = ExternalStateBase;
                        }
                    }
                    RaisePropertyChanged();
                }
            }
        }
        private void _monitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            areUpdateEvent.Set();
        }
        public void ChangeInternalMode()
        {
            StateObj = InternalStateBase;
        }
        public void ChangeExternalMode()
        {
            StateObj = ExternalStateBase;
        }
        private LoaderSupervisorInternalStateBase _InternalStateBase;
        public LoaderSupervisorInternalStateBase InternalStateBase
        {
            get { return _InternalStateBase; }
            set
            {
                if (value != _InternalStateBase)
                {

                    _InternalStateBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoaderSupervisorExternalStateBase _ExternalStateBase;
        public LoaderSupervisorExternalStateBase ExternalStateBase
        {
            get { return _ExternalStateBase; }
            set
            {
                if (value != _ExternalStateBase)
                {

                    _ExternalStateBase = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsSelectedLoader;
        public bool IsSelectedLoader
        {
            get { return _IsSelectedLoader; }
            set
            {
                if (value != _IsSelectedLoader)
                {

                    _IsSelectedLoader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAbortError;
        public bool IsAbortError
        {
            get { return _IsAbortError; }
            set
            {
                if (value != _IsAbortError)
                {

                    _IsAbortError = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum Initialize(Autofac.IContainer container)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                cont = container;
                MapSlicer = new LoaderMapSlicer();

                for (int i = 0; i < BufferModuleInfo.Count(); i++)
                {
                    BufferModuleInfo[i] = new HolderModuleInfo();
                    BufferModuleInfo[i].WaferStatus = EnumSubsStatus.NOT_EXIST;
                }

                InternalStateBase = new Internal_Idle(this);
                ExternalStateBase = new External_Idle(this);

                StateObj = ExternalStateBase;

                var gemmodule = this.GEMModule();
                var seqmodule = this.SequenceEngineManager();

                _monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
                _monitoringTimer.Elapsed += _monitoringTimer_Elapsed;
                _monitoringTimer.Start();
                UpdateThread = new Thread(new ThreadStart(DoWork));
                StatusSoakingInfoUpdateThread = new Thread(new ThreadStart(StatusInfoUpdateThreadProc));
                StageWatchDogThread = new Thread(new ThreadStart(StageWatchDogThreadProc));
                UpdateThread.Name = this.GetType().Name;
                StatusSoakingInfoUpdateThread.Name = "StatusSoaking_InfoUpdate";
                StageWatchDogThread.Name = "StageWatchDog";
                UpdateThread.Start();
                StatusSoakingInfoUpdateThread.Start();
                StageWatchDogThread.Start();
                DynamicMode = DynamicModeEnum.NORMAL;
                ModuleState = new ModuleUndefinedState(this);
                ModuleState.StateTransition(StateObj.GetModuleState());

                for (int i = 0; i < StageCount; i++)
                {
                    StageStates.Add(ModuleStateEnum.UNDEFINED);
                    StagesIsTempReady.Add(false);
                    StageSetTemp.Add(-999);
                }

                int foupControllercount = SystemModuleCount.ModuleCnt.FoupCount;

                for (int i = 0; i < foupControllercount; i++)
                {
                    this.ActiveLotInfos.Add(new ActiveLotInfo(i, this));
                    this.BackupActiveLotInfos.Add(new ActiveLotInfo(i, this));
                }

                //_ExternalLotJob = new Dictionary<string, List<ActiveLotInfo>>();

                this.GetGPLoader().LoaderBuzzer(false);
                Loader.SetModuleEnable();
                //Clients["CHUCK4"] = new StageEmulation(4, Loader);
                //Clients["CHUCK3"] = new StageEmulation(3, Loader);

                //Clients["CHUCK1"] = new StageEmulation(1, Loader);
                //Clients["CHUCK8"] = new StageEmulation(8, Loader);
                //Clients["CHUCK10"] = new StageEmulation(10, Loader);
                //Clients["CHUCK9"] = new StageEmulation(9, Loader);
                //Clients["CHUCK5"] = new StageEmulation(5, Loader);
                //Clients["CHUCK11"] = new StageEmulation(11, Loader);
                //Clients["CHUCK6"] = new StageEmulation(6, Loader);

                //Clients["CHUCK12"] = new StageEmulation(12, Loader);
                //Clients["CHUCK2"] = new StageEmulation(2, Loader);

                //Clients["CHUCK7"] = new StageEmulation(7, Loader);
                //for (int i = 0; i < 12; i++)
                //{
                //    loaderCommunicationManager.Cells[i].StageMode = GPCellModeEnum.ONLINE;
                //}
                InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        int priorityNum = 0;
        public async Task<EventCodeEnum> CassetteUnload(int foupNum)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var Cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupNum);
                    CassetteScanStateEnum cassetteScanStateEnum = Cassette.ScanState;
                    string foupstateStr = "";
                    if ((ValidationFoupLoadedState(foupNum, ref foupstateStr) == EventCodeEnum.NONE)
                    && (cassetteScanStateEnum != CassetteScanStateEnum.READING && cassetteScanStateEnum != CassetteScanStateEnum.RESERVED))
                    {
                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].SetLock(false);
                        //   retVal = this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                        this.ActiveLotInfos[Cassette.ID.Index - 1].FoupUnLoad();
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderSupervisor], CassetteUnload() can't execute. " +
                            $"ScanState : {cassetteScanStateEnum}.");
                    }
                }
                  );

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task<EventCodeEnum> CassetteNormalUnload(int foupNum)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    if (this.GPLoader.GetCSTState(foupNum - 1) == EnumCSTState.LOADING
                   || this.GPLoader.GetCSTState(foupNum - 1) == EnumCSTState.UNLOADING)
                    {
                        LoggerManager.Debug($"CassetteNormalUnload Foup#{foupNum} CST State is {this.GPLoader.GetCSTState(foupNum - 1)}");
                    }
                    else
                    {
                        var Cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupNum);
                        if (Cassette.FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                        {
                            if (Extensions_IParam.ProberRunMode == RunMode.EMUL || this.GPLoader.GetCSTState(foupNum - 1) == EnumCSTState.LOADED) //nCSTState 가 Null인경우는 에뮬인경우
                            {
                                retVal = this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupNormalUnloadCommand());
                            }
                            else
                            {
                                LoggerManager.Debug($"CassetteNormalUnload notWorking Foup#{foupNum} CST State is {this.GPLoader.GetCSTState(foupNum - 1)}");
                            }
                        }
                        if (ActiveLotInfos.Count > 0 && ActiveLotInfos.Count >= foupNum)
                        {
                            ActiveLotInfos[foupNum - 1].IsActiveFromHost = false;
                            LoggerManager.Debug($"CassetteNormalUnload Foup#{foupNum} IsActiveFormHost change to false");
                        }
                    }
                }
                  );

            }
            catch (Exception)
            {

            }
            return retVal;
        }


        public EventCodeEnum CarrierCancel(int foupIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (foupIndex <= 0 | foupIndex > ActiveLotInfos.Count)
                {
                    //Index Error.
                    return retVal;
                }

                LoggerManager.ActionLog(ModuleLogType.LOT_SETTING, StateLogType.START, $"CARRIERCANCEL, FOUP: {foupIndex} ", isLoaderMap: true);

                ActiveLotInfo lotinfo = ActiveLotInfos[foupIndex - 1];

                if (lotinfo.State == LotStateEnum.Running)
                {
                    ActiveLotInfos[foupIndex - 1].State = LotStateEnum.Cancel;
                    Loader.Foups[foupIndex - 1].LotState = ActiveLotInfos[foupIndex - 1].State;
                    ActiveLotInfos[foupIndex - 1].IsManaulEnd = false;
                }
                else
                {
                    if (ActiveLotInfos.Count > 0 && ActiveLotInfos.Count >= foupIndex)
                    {
                        if (lotinfo.IsActiveFromHost == false)
                        {
                            //Manual 동작중 이거나, Lot 가 끝나고 Foup이 Unload된 상태
                            LoggerManager.Debug($"[LoaderSupervisor](CarrierCancel) Foup#{foupIndex} IsActiveFormHost is true");
                            //return EventCodeEnum.NONE;
                        }
                    }

                    var Cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, ActiveLotInfos[foupIndex - 1].FoupNumber);

                    string foupstateStr = "";
                    if (ValidationFoupLoadedState(foupIndex, ref foupstateStr) == EventCodeEnum.NONE)
                    {
                        var ret = CassetteUnload(ActiveLotInfos[foupIndex - 1].FoupNumber).Result;

                    }
                    else
                    {
                        if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1] != null)
                        {
                            bool lockstate = this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].GetFoupIO()?.IOMap?.Inputs.DI_CST_LOCK12s[foupIndex - 1]?.Value ?? false;

                            if (lockstate)
                            {
                                bool DPOutFlag = false;
                                int readBitRetVal = -1;

                                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                {
                                    if (Cassette.FoupState == FoupStateEnum.LOAD)
                                    {
                                        DPOutFlag = false;
                                    }
                                    else if (Cassette.FoupState == FoupStateEnum.UNLOAD)
                                    {
                                        DPOutFlag = true;
                                    }
                                }
                                else
                                {
                                    readBitRetVal = this.GetFoupIO().ReadBit(this.GetFoupIO().IOMap.Inputs.DI_DP_OUTs[foupIndex - 1], out DPOutFlag);
                                }

                                if (readBitRetVal == 0)
                                {
                                    //Foup 이 Unload 상태로 Lock 만 되어있는 상황에서 Foup State 는 ERROR , Cassette State 는 LOADING 이이서 ValidationUnloaded 를 사용할 수 없음.
                                    if (lockstate && DPOutFlag)
                                    {
                                        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupDockingPlateUnlockCommand());
                                        LoggerManager.Debug($"[LoaderSupervisor](CarrierCancel) Foup#{Cassette.ID.Index} CancelCarrier Carrier Unlock");
                                        //lotinfo.SetAssignLotState(LotAssignStateEnum.CANCELD);
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LoaderSupervisor](CarrierCancel) Foup#{Cassette.ID.Index} CancelCarrier Clamp Unlock State: {lockstate}, DP Out State : {DPOutFlag}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[LoaderSupervisor](CarrierCancel) Foup#{Cassette.ID.Index} CancelCarrier DP Out Read Bit Fail, RetVal : {readBitRetVal}.");
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[LoaderSupervisor](CarrierCancel) Foup#{Cassette.ID.Index} Foupcontroller is null");
                        }
                    }

                    //TODO Ann 이벤트 정리 되면 EventFired() 에서 처리 해야 함
                    foreach (var lotsetting in Loader.Foups[Cassette.ID.Index - 1].LotSettings)
                    {
                        if (lotsetting.IsSelected == true)
                        {
                            lotsetting.Clear(Loader.GetUseLotProcessingVerify());
                        }
                    }
                    Loader.Foups[Cassette.ID.Index - 1].PreLotID = string.Empty;
                    foreach (var slot in Loader.Foups[Cassette.ID.Index - 1].Slots)
                    {
                        slot.IsPreSelected = false;
                    }

                    //==<Cell Check>==
                    NotifyLotEndToCell(ActiveLotInfos[foupIndex - 1].FoupNumber, ActiveLotInfos[foupIndex - 1].LotID);
                    //================

                    Loader.Foups[foupIndex - 1].LotState = LotStateEnum.Idle;
                    Loader.Foups[foupIndex - 1].AllocatedCellInfo = "";

                    if (LotSysParam.IsCancelCarrierEventNotRuning)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: foupIndex);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }


                    //var lockobj = GetLoaderPIV().GetPIVDataLockObject();
                    //lock (lockobj)
                    //{
                    //    GetLoaderPIV().SetFoupState(foupIndex, GEMFoupStateEnum.READY_TO_UNLOAD);
                    //    GetLoaderPIV().SetFoupInfo(foupIndex, stagelist: "", slotlist: "");
                    //    GetLoaderPIV().UpdateFoupInfo(foupIndex);
                    //    this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(CarrierCanceledEvent).FullName));
                    //}
                }
                lotinfo.SetAssignLotState(LotAssignStateEnum.CANCEL);
            }
            catch (Exception)
            {

            }
            return retVal;
        }
        public EventCodeEnum ActiveProcess(ActiveProcessActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                int FoupNumber = actReqData.FoupNumber;

                ActiveLotInfo currentActiveLotInfo = ActiveLotInfos[FoupNumber - 1];

                currentActiveLotInfo.IsActiveFromHost = true;
                LoggerManager.Debug($"ActiveProcess Foup#{actReqData.FoupNumber} IsActiveFormHost change to true");
                currentActiveLotInfo.State = LotStateEnum.Idle;
                //currentActiveLotInfo.UsingStageList.Clear();
                //currentActiveLotInfo.LotOutStageIndexList.Clear();
                //currentActiveLotInfo.IsReserveCancel = false;
                currentActiveLotInfo.LotID = actReqData.LotID;
                currentActiveLotInfo.UsingStageIdxList = actReqData.UseStageNumbers;
                currentActiveLotInfo.AssignedUsingStageIdxList = actReqData.UseStageNumbers.ToList();

                var pivContainer = this.GEMModule().GetPIVContainer();
                pivContainer.SetLotID(actReqData.LotID);
                this.GEMModule().GetPIVContainer().SetLoaderLotIds(FoupNumber, actReqData.LotID != null ? actReqData.LotID : "");
                pivContainer.FoupNumber.Value = actReqData.FoupNumber;
                pivContainer.ListOfStages.Value = actReqData.UseStageNumbers_str;
                pivContainer.SetFoupState(pivContainer.FoupNumber.Value, GEMFoupStateEnum.ACTIVED);
                pivContainer.SetFoupInfo(actReqData.FoupNumber, lotid: actReqData.LotID, stagelist: actReqData.UseStageNumbers_str);

                LoggerManager.Debug($"actReqData.FoupNumber : {actReqData.FoupNumber}");
                LoggerManager.Debug($"actReqData.UseStageNumbers_str : {actReqData.UseStageNumbers_str}");
                LoggerManager.Debug($"actReqData.LotID : {actReqData.LotID}");

                string cstHashCode = "";
                var cstObj = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, actReqData.FoupNumber);
                if (cstObj != null)
                {
                    if (currentActiveLotInfo.AssignState != LotAssignStateEnum.ASSIGNED)
                    {
                        cstObj.SetHashCode(true);
                    }
                    cstHashCode = cstObj.HashCode;
                }
                currentActiveLotInfo.CST_HashCode = cstHashCode;

                currentActiveLotInfo.SetAssignLotState(LotAssignStateEnum.ASSIGNED);
                string str_stagelist = null;

                foreach (var stage in currentActiveLotInfo.UsingStageIdxList)
                {
                    str_stagelist = str_stagelist + $"[#{stage}] ";
                }
                LoggerManager.Debug($"ActiveProcess Using Stage Numbers : {str_stagelist}");
                LoggerManager.ActionLog(ModuleLogType.LOT_SETTING, StateLogType.START, $"ACTIVE PROCESS," +
                    $" FOUP: {actReqData.FoupNumber} , USING STAGE: {str_stagelist} , LOT ID: {actReqData.LotID} , CST HASH CODE : {cstHashCode}", isLoaderMap: true);
                for (int i = 0; i < actReqData.UseStageNumbers.Count; i++)
                {
                    try
                    {
                        var client = GetClient(actReqData.UseStageNumbers[i]);

                        //if (client != null)
                        //{
                        //    currentActiveLotInfo.UsingStageList.Add(client);
                        //}
                        LoggerManager.Debug($"Before PIVContainer.ListOfStages.Value : {pivContainer.ListOfStages.Value}");

                        if (pivContainer.ListOfStages.Value.Count() > 0)
                        {
                            pivContainer.ListOfStages.Value = pivContainer.ListOfStages.Value.Remove(actReqData.UseStageNumbers[i] - 1, 1);
                        }


                        pivContainer.ListOfStages.Value = pivContainer.ListOfStages.Value.Insert(actReqData.UseStageNumbers[i] - 1, "1");

                        client?.SetActiveLotInfo(actReqData.FoupNumber, actReqData.LotID, cstHashCode, Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, actReqData.FoupNumber).FoupID);

                        LoggerManager.Debug($"After pivContainer.ListOfStages.Value : {pivContainer.ListOfStages.Value}");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                currentActiveLotInfo.RingBuffer = new Queue<int>();

                //var cells = currentActiveLotInfo.UsingStageList.OrderBy(i => i.GetChuckID().Index).ToList();

                foreach (var stageidx in currentActiveLotInfo.UsingStageIdxList)
                {
                    currentActiveLotInfo.RingBuffer.Enqueue(stageidx);
                }

                string allocatedCells = "";

                for (int i = 0; i < currentActiveLotInfo.UsingStageIdxList.Count; i++)
                {
                    allocatedCells += currentActiveLotInfo.UsingStageIdxList[i] + "";

                    if (i != currentActiveLotInfo.UsingStageIdxList.Count - 1)
                    {
                        allocatedCells += ", ";
                    }
                }

                Loader.Foups[FoupNumber - 1].AllocatedCellInfo = allocatedCells;// #Hynix_Merge: 검토 필요, Hynix에서는 FoupNumber-1 였음.
                currentActiveLotInfo.IsFoupEnd = false;
                currentActiveLotInfo.RemainCount = currentActiveLotInfo.RingBuffer.Count();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum DeactiveProcess(int activeIdx, string lotid = "", int foupnumber = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.FoupOpModule().FoupControllers[activeIdx].SetLock(false);
                ActiveLotInfos[activeIdx].State = LotStateEnum.Done;
                ActiveLotInfos[activeIdx].IsFoupEnd = false;
                ActiveLotInfos[activeIdx].LotPriority = 0;
                ActiveLotInfos[activeIdx].CellDeviceInfoDic.Clear();

                Loader.Foups[activeIdx].LotState = ActiveLotInfos[activeIdx].State;
                Loader.Foups[activeIdx].LotPriority = 0;

                if (ActiveLotInfos[activeIdx].AssignState != LotAssignStateEnum.CANCEL)
                {
                    ActiveLotInfos[activeIdx].SetAssignLotState(LotAssignStateEnum.JOB_FINISHED);
                }                
                DeactiveDevice(activeIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DeactiveDevice(int activeIdx)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // LOT 와 연관된 Device 에 대해 보관해놓은 zip 파일 삭제
                if (ActiveLotInfos[activeIdx].OriginalDeviceZipName != null)
                {
                    foreach (var devzipPath in ActiveLotInfos[activeIdx].OriginalDeviceZipName)
                    {
                        if (File.Exists(devzipPath.Value))
                        {
                            File.Delete(devzipPath.Value);
                            LoggerManager.Debug($"DeactiveProcess() Delete active device zip file : {devzipPath}");
                        }
                    }
                    ActiveLotInfos[activeIdx].OriginalDeviceZipName.Clear();
                }

                if (ActiveLotInfos[activeIdx].CellDeviceInfoDic != null)
                {
                    ActiveLotInfos[activeIdx].CellDeviceInfoDic.Clear();
                }

                if(ActiveLotInfos[activeIdx].AssignState != LotAssignStateEnum.CANCEL)
                {
                    ActiveLotInfos[activeIdx].SetAssignLotState(LotAssignStateEnum.JOB_FINISHED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetRecipeToDevice(DownloadStageRecipeActReqData data)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (data.RecipeDic.Values.Count != 0 && data.FoupNumber > 0)
                {
                    if (data.RecipeDic.Values.ElementAt(0) != null)
                    {
                        //var device = data.RecipeDic.Values.ElementAt(0).Substring(0, 4);
                        SetDeviceName(data.FoupNumber, data.RecipeDic.Values.ElementAt(0));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }
        public EventCodeEnum SetDeviceName(int foupNumber, string DeviceName)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                ActiveLotInfos[foupNumber - 1].DeviceName = DeviceName;
                this.GEMModule().GetPIVContainer().SetFoupInfo(foupNumber, devicename: DeviceName);
                this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EventCodeEnum SetCaseetteHashCodeToStage(int foupnumber, string cassetteHashCode)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var activeLotInfo = ActiveLotInfos.Find(info => info.FoupNumber == foupnumber);
                if (activeLotInfo != null)
                {
                    foreach (var stageidx in activeLotInfo.UsingStageIdxList)
                    {
                        var stage = GetClient(stageidx);
                        if (stage != null)
                        {
                            stage.SetCassetteHashCode(activeLotInfo.FoupNumber, activeLotInfo.LotID, cassetteHashCode);
                            LoggerManager.Debug($"[LoaderSupervisor] SetCaseetteHashCodeToStage() set to stage #{stage.GetChuckID().Index}, CSTHashCode : {cassetteHashCode}");
                        }
                    }
                }
                LoggerManager.Debug($"[LoaderSupervisor] SetCaseetteHashCodeToStage() foupnumber: {foupnumber}, CSTHashCode : {cassetteHashCode}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

   
        public EventCodeEnum UsingPMICalc(ISlotModule slot)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (StateObj is External_Running)
                {
                    int foupNum = 0;
                    int slotIdx = 0;

                    if (slot.ID.Index % 25 == 0)
                    {
                        foupNum = slot.ID.Index / 25;
                        slotIdx = 25;
                    }
                    else
                    {
                        foupNum = slot.ID.Index / 25 + 1;
                        slotIdx = slot.ID.Index % 25;
                    }

                    if (ActiveLotInfos[foupNum - 1].UsingPMIList.Where(idx => idx == slotIdx).FirstOrDefault() > 0)
                    {
                        if (ActiveLotInfos[foupNum - 1].DoPMICount > 0)
                        {
                            slot.Holder.TransferObject.PMITirgger = PMIRemoteTriggerEnum.TOTALNUMBER_WAFER_TRIGGER;
                            slot.Holder.TransferObject.DoPMIFlag = true;
                        }
                        else if (ActiveLotInfos[foupNum - 1].PMIEveryInterval > 0)
                        {
                            slot.Holder.TransferObject.PMITirgger = PMIRemoteTriggerEnum.EVERY_WAFER_TRIGGER;
                            slot.Holder.TransferObject.DoPMIFlag = true;
                        }
                        else
                        {
                            slot.Holder.TransferObject.PMITirgger = PMIRemoteTriggerEnum.UNDIFINED;
                            slot.Holder.TransferObject.DoPMIFlag = false;
                        }
                    }
                    else
                    {
                        slot.Holder.TransferObject.PMITirgger = PMIRemoteTriggerEnum.UNDIFINED;
                        slot.Holder.TransferObject.DoPMIFlag = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }
        public EventCodeEnum ErrorEndCell(int cellIdx, bool canUnloadWhilePaused = false)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                var client = GetClient(cellIdx);
                retVal = ErrorEndClient(client, cellIdx, canUnloadWhilePaused);
                //if(retVal != EventCodeEnum.NONE)
                //{
                //    PIVInfo pIVInfo = new PIVInfo(stagenumber: cellIdx);
                //    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                //    this.EventManager().RaisingEvent(typeof(CellAbortFail).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
                //    semaphore.Wait();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetWaferIDs(AssignWaferIDMap waferIdData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED; ;
            try
            {
                if (ActiveLotInfos[waferIdData.FoupNumber - 1].WaferIDInfo == null)
                {
                    ActiveLotInfos[waferIdData.FoupNumber - 1].WaferIDInfo = new List<WaferIDInfo>();
                }
                ActiveLotInfos[waferIdData.FoupNumber - 1].WaferIDInfo.Clear();
                for (int i = 1; i <= waferIdData.WaferIDs.Count; i++)
                {
                    ActiveLotInfos[waferIdData.FoupNumber - 1].WaferIDInfo.Add(new WaferIDInfo(waferIdData.FoupNumber, i, waferIdData.WaferIDs[i - 1]));
                    //if (Loader.Foups[waferIdData.FoupNumber - 1].ScanState == CassetteScanStateEnum.READ)
                    if (Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, waferIdData.FoupNumber).ScanState == CassetteScanStateEnum.READ)
                    {
                        int slotIndex = (waferIdData.FoupNumber - 1) * 25 + i;
                        TransferObject transferObj = Loader.ModuleManager.GetTransferObjectAll().Where(item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && item.OriginHolder.Index == slotIndex).FirstOrDefault();
                        if (transferObj != null)
                        {
                            transferObj.Pre_OCRID = waferIdData.WaferIDs[i - 1];
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

        public bool WaferIdConfirm(TransferObject transferObj, string ocr)
        {
            bool isvalid = false;
            try
            {
                Stopwatch elapsedStopWatch = new Stopwatch();
                bool runflag = true;
                int timeout = GetWaitForWaferIdConfirmTimeout();

                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();

                LoggerManager.Debug($"WaferIdConfirm(): Wait Start. timeout:{timeout}");
                int foupNum = (transferObj.OriginHolder.Index - 1) / 25 + 1;
                int slotNum = (transferObj.OriginHolder.Index % 25 == 0) ? 25 : transferObj.OriginHolder.Index % 25;
                string predefinedId = GetPreDefindWaferId(foupNum, slotNum);

                do
                {
                    try
                    {
                        var wafer = Loader.ModuleManager.GetTransferObjectAll().Where(
                           item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                   item.OriginHolder.Index == transferObj.OriginHolder.Index
                           ).ToList();

                        if (wafer.Count > 0)
                        {
                            if (wafer.First().WFWaitFlag == false)
                            {
                                isvalid = true;
                                if (predefinedId == ocr)
                                {
                                    LoggerManager.Debug($"WaferIdConfirm(): Matched. ocr:{ocr}, Pre_OCRID:{predefinedId}");
                                }
                                else
                                {
                                    LoggerManager.Debug($"WaferIdConfirm(): UnMatched. ocr:{ocr}, Pre_OCRID:{predefinedId}");
                                }

                                break;
                            }
                            if (this.ActiveLotInfos[foupNum - 1].State == LotStateEnum.Cancel ||
                                   this.ActiveLotInfos[foupNum - 1].State == LotStateEnum.End ||
                                   this.ActiveLotInfos[foupNum - 1].State == LotStateEnum.Done)
                            {
                                isvalid = false;
                                LoggerManager.Debug($"WaferIdConfirm(): break, loaderModule.ModuleState:{this.Loader.ModuleState}");
                                break;
                            }

                        }


                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                        {
                            isvalid = false;
                            runflag = false;

                            PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(OcrConfirmFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                            LoggerManager.Debug($"WaferIdConfirm(): command time out {timeout}");
                        }

                        Thread.Sleep(20);
                    }
                    catch (Exception err)
                    {
                        runflag = false;
                        LoggerManager.Exception(err);
                    }

                } while (runflag);

                LoggerManager.Debug($"WaferIdConfirm(): Wait End");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaferIdConfirm(): Exception occurred. Err = {err.Message}");
            }

            return isvalid;
        }


        public EventCodeEnum OcrReadStateRisingEvent(TransferObject transferObj, ModuleID pa, int pwIDReadResult)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                int slotNum = 0;
                int foupNum = 0;

                if (transferObj.WaferType.Value == EnumWaferType.STANDARD || transferObj.WaferType.Value == EnumWaferType.TCW)
                {
                    slotNum = transferObj.OriginHolder.Index % 25;
                    int offset = 0;
                    if (slotNum == 0)
                    {
                        slotNum = 25;
                        offset = -1;
                    }
                    foupNum = ((transferObj.OriginHolder.Index + offset) / 25) + 1;
                }

                var cstModule = this.Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, foupNum) as ICassetteModule;
        
                PIVInfo pivinfo = null;

                if (pwIDReadResult == -1)
                {
                    pivinfo = new PIVInfo()
                    {
                        FoupNumber = foupNum,
                        WaferID = transferObj.OCR.Value,
                        StageNumber = 0,//selectedStages != null ? selectedStages[0] : 0, 의미 없는 값
                        RecipeID = cstModule?.Device.AllocateDeviceInfo.DeviceName.Value,
                        PreLoadingSlotNum = slotNum,
                        SlotNumber = slotNum,
                        PreAlignNumber = pa.Index,
                    };
                }
                else
                {
                    // Wafer change
                    pivinfo = new PIVInfo()
                    {
                        WaferID = transferObj.OCR.Value,
                        PolishWaferIDReadResult = pwIDReadResult
                    };
                }
                if (transferObj.OCRReadState == OCRReadStateEnum.DONE)
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    if (transferObj.WaferType.Value == EnumWaferType.TCW)
                    {
                        this.EventManager().RaisingEvent(typeof(TcwOcrReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    }
                    else if(transferObj.WaferType.Value == EnumWaferType.POLISH)
                    {
                        this.EventManager().RaisingEvent(typeof(PolishWaferIDReadResultEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    }
                    else
                    {
                        this.EventManager().RaisingEvent(typeof(OcrReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    }
                    semaphore.Wait();
                    retVal = EventCodeEnum.NONE;
                }
                else if (transferObj.OCRReadState == OCRReadStateEnum.ABORT)
                {
                    transferObj.WaferState = EnumWaferState.SKIPPED;

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    if (transferObj.WaferType.Value == EnumWaferType.TCW)
                    {
                        this.EventManager().RaisingEvent(typeof(TcwOcrReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));

                    }
                    else if (transferObj.WaferType.Value == EnumWaferType.POLISH)
                    {
                        this.EventManager().RaisingEvent(typeof(PolishWaferIDReadResultEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    }
                    else
                    {
                        this.EventManager().RaisingEvent(typeof(OcrReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    }

                    semaphore.Wait();
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"OcrReadStateRisingEvent(): Skip Rising Event. OCRReadState:{transferObj.OCRReadState}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string GetPreDefindWaferId(int foupNum, int slotNum)
        {
            try
            {
                if (ActiveLotInfos != null)
                {
                    return ActiveLotInfos[foupNum - 1].WaferIDInfo[slotNum - 1].WaferID;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return string.Empty;
        }
        public EventCodeEnum SetErrorState(string reason = "")
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"Set Loader Master Error State. Reason = {reason}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public FoupShiftModeEnum GetFoupShiftMode()
        {
            return LotSysParam?.FoupShiftMode.Value ?? FoupShiftModeEnum.NORMAL;
        }

        public void DeAllocate(FoupObject foup)
        {
            try
            {
                var cells = foup.AllocatedCellInfo.Split(',');
                LoggerManager.Debug($"[DeAllocate] DeAllocate: {foup.AllocatedCellInfo}");
                int[] ints = Array.ConvertAll(cells, s => int.TryParse(s, out var x) ? x : -1);

                foreach (var index in ints)
                {
                    if (index != -1)
                    {
                        var lotsetting = foup.LotSettings.FirstOrDefault(x => x.Index == index);

                        if (lotsetting != null)
                        {
                            lotsetting.Clear((Loader as LoaderModule).GetUseLotProcessingVerify());
                        }
                    }
                }
                Loader.ClearAlreadyAssignedStages();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 정상종료 되었음에도 불구하고 LotID가 맞지 않아 Cell이 Lot End가 안되는 경우를 처리 하기 위한 함수
        /// Lot가 정상종료 되었을 때, 아직도 Running인 Cell이 다른 Lot 중이기 때문에 Running인지 확인 후 Lot End 시키기 위함
        /// </summary>
        /// <param name="foup"></param>
        public void CheckAndEndLotIfRunningCellsExist(FoupObject foup)
        {
            try
            {
                var cells = foup.AllocatedCellInfo.Split(',');
                LoggerManager.Debug($"[CheckAndEndLotIfRunningCellsExist] Allocated Cell: {foup.AllocatedCellInfo}");
                int[] ints = Array.ConvertAll(cells, s => int.TryParse(s, out var x) ? x : -1);
                foreach (var cellIndex in ints)
                {
                    if (cellIndex != -1)
                    {
                        var stg = GetClient(cellIndex);
                        if (stg?.GetLotState() == ModuleStateEnum.RUNNING)
                        {
                            int count = 0;
                            foreach (var lotInfo in ActiveLotInfos)
                            {
                                if (lotInfo.State == LotStateEnum.Running || lotInfo.State == LotStateEnum.Cancel ||
                                    (lotInfo.State == LotStateEnum.Idle && lotInfo.AssignState == LotAssignStateEnum.ASSIGNED))
                                {
                                    //AssignedUsingStageIdxList을 사용하지 않음 할당되어 동작하고 있는 Stage만 LotOPEnd 하기 위함.
                                    int cellIdx = lotInfo.UsingStageIdxList.Find(cell => cell == cellIndex);
                                    if (cellIdx == cellIndex)
                                    {
                                        count++;
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[CheckAndEndLotIfRunningCellsExist] Allocated Cell#{cellIndex}, Already assigned to another Lot(Foup):{lotInfo.FoupNumber}");
                                    }
                                }
                                
                            }

                            if (count == 0)
                            {
                                if (stg.GetChuckWaferStatus() == EnumSubsStatus.EXIST)
                                {
                                    // Wafer 가 EXSIT 상태라면 강제 LOT END 가 되지 않도록 한다.
                                    LoggerManager.Debug($"[CheckAndEndLotIfRunningCellsExist] Allocated Cell#{cellIndex}, Wafer Stateus is EXIST.");
                                    continue;
                                }
                                //var isSucess = LotOPEndClient(stg, foup.Index);
                                isLotStartFlag[cellIndex - 1] = false;
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum NotifyLotEndToCell(int foupNumber, string lotID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var stageidx in ActiveLotInfos[foupNumber - 1].UsingStageIdxList)
                {
                    var stage = GetClient(stageidx);
                    if (stage != null)
                    {
                        stage.NotifyLotEnd(foupNumber, lotID);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum Select_Slot_Stages(int foupNumber, string lotID, Dictionary<int, List<int>> usingStageBySlot, List<SlotCellInfo> slotCellInfos = null)
        {
            //v22_merge// slotCellInfos null일 경우 추가(hynix)
            EventCodeEnum retVal = EventCodeEnum.NONE;

            if (ActiveLotInfos[foupNumber - 1].LotID == lotID)
            {
                var pivContainer = this.GEMModule().GetPIVContainer();
                ActiveLotInfos[foupNumber - 1].UsingStagesBySlot = usingStageBySlot;

                if (slotCellInfos != null)
                {
                    string listOflost = "0000000000000000000000000";

                    for (int i = 0; i < slotCellInfos.Count; i++)
                    {
                        if (slotCellInfos[i].CellIndexs.Count != 0)
                        {
                            int retindex = slotCellInfos[i].CellIndexs.Find(index => index != 0);
                            if (retindex != 0)
                            {
                                listOflost = listOflost.Remove(i, 1);
                                listOflost = listOflost.Insert(i, "1");
                            }

                        }
                    }
                    pivContainer.SetFoupInfo(foupNumber, slotlist: listOflost);
                }

                LoggerManager.Debug($"[LOADER SUPERVISOR] Select_Slot_Stages - foup number : {foupNumber} ");
                LoggerManager.ActionLog(ModuleLogType.LOT_SETTING, StateLogType.START, $"SELECT SLOT, FOUP:{foupNumber} , SLOT LIST: {pivContainer.ListOfSlot.Value}", isLoaderMap: true);
            }
            else
            {
                LoggerManager.Debug($"[LOADER SUPERVISOR] SelectSlot LOTID Mismatching Error- foup number : {foupNumber} ActiveProcess LOTID:{ActiveLotInfos[foupNumber - 1].LotID} Select Slot LOTID:{lotID}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }
        public EventCodeEnum SelectSlot(int foupNumber, string lotID, List<int> slots)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            var pivContainer = this.GEMModule().GetPIVContainer();
            pivContainer.ListOfSlot.Value = "0000000000000000000000000";

            if (foupNumber <= 0 | foupNumber > ActiveLotInfos.Count)
            {
                LoggerManager.Debug($"[LOADER SUPERVISOR] SelectSlot Error- foup number : {foupNumber}");
                return retVal;
            }

            bool lotidCheck = true;
            if (ActiveLotInfos[foupNumber - 1].LotID == "")
            {
                lotidCheck = true;
            }
            else
            {
                lotidCheck = false;
            }
            if (ActiveLotInfos[foupNumber - 1].LotID == lotID || lotidCheck)
            {
                ActiveLotInfos[foupNumber - 1].UsingPMIList.Clear();
                ActiveLotInfos[foupNumber - 1].UsingSlotList = slots.ToList();
                ActiveLotInfos[foupNumber - 1].NotDoneSlotList = slots.ToList();

                if (ActiveLotInfos[foupNumber - 1].DoPMICount > 0 && ActiveLotInfos[foupNumber - 1].UsingSlotList.Count > 0)
                {
                    int interval = ActiveLotInfos[foupNumber - 1].UsingSlotList.Count / ActiveLotInfos[foupNumber - 1].DoPMICount;

                    int doPMICnt = 0;

                    if (interval > 0)
                    {
                        for (int i = 0; i < ActiveLotInfos[foupNumber - 1].UsingSlotList.Count; i++)
                        {
                            if ((i + 1) % interval == 0)
                            {
                                doPMICnt++;
                                ActiveLotInfos[foupNumber - 1].UsingPMIList.Add(ActiveLotInfos[foupNumber - 1].UsingSlotList[i]);
                                LoggerManager.Debug($"PMI Trigger: PMI Wafer Per LOT , FoupNumber:{foupNumber}, SlotNumber:{ActiveLotInfos[foupNumber - 1].UsingSlotList[i]}");
                            }

                            if (doPMICnt >= ActiveLotInfos[foupNumber - 1].DoPMICount)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (ActiveLotInfos[foupNumber - 1].PMIEveryInterval > 0 && ActiveLotInfos[foupNumber - 1].UsingSlotList.Count > 0)
                {
                    int interval = ActiveLotInfos[foupNumber - 1].PMIEveryInterval;
                    if (interval > 0)
                    {
                        for (int i = 0; i < ActiveLotInfos[foupNumber - 1].UsingSlotList.Count; i++)
                        {
                            if ((i + 1) % interval == 0)
                            {
                                ActiveLotInfos[foupNumber - 1].UsingPMIList.Add(ActiveLotInfos[foupNumber - 1].UsingSlotList[i]);
                                LoggerManager.Debug($"PMI Trigger: PMI EveryWaferInterval , FoupNumber:{foupNumber}, SlotNumber:{ActiveLotInfos[foupNumber - 1].UsingSlotList[i]}");
                            }

                        }
                    }
                }

                if (this.FoupOpModule().GetFoupController(foupNumber).GetCassetteType() == CassetteTypeEnum.FOUP_13)
                {
                    //1. 14보다 값이 작은지 확인                        
                    if (slots.Count < 14)
                    {
                        // 135791113151719 이런 형태를 다시 123456789 이런 형태로 
                        for (int i = 0; i < slots.Count; i++)
                        {
                            slots[i] = slots[i] / 2 + 1;                            
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[LOADER SUPERVISOR] SelectSlot Error- foup number : {foupNumber} / The slot information received from the host should not contain any slots beyond the 13th.");
                        return retVal;
                    }
                }
                for (int i = 0; i < slots.Count; i++)
                {
                    pivContainer.ListOfSlot.Value = pivContainer.ListOfSlot.Value.Remove(slots[i] - 1, 1);
                    pivContainer.ListOfSlot.Value = pivContainer.ListOfSlot.Value.Insert(slots[i] - 1, "1");

                }
                //this.StageSupervisor().LotOPModule().UpdateCstWaferState(slots);

                //pivContainer.SetFoupInfo(foupNumber, slotlist: pivContainer.ListOfSlot.Value);
                pivContainer.SetListOfSlot(foupNumber, pivContainer.ListOfSlot.Value);

                LoggerManager.Debug($"[LOADER SUPERVISOR] SelectSlot - slot list : {pivContainer.ListOfSlot.Value}, foup number : {foupNumber} ");
                LoggerManager.ActionLog(ModuleLogType.LOT_SETTING, StateLogType.START, $"SELECT SLOT, FOUP:{foupNumber} , SLOT LIST: {pivContainer.ListOfSlot.Value}", isLoaderMap: true);

                if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS")
                        && (SystemManager.SystemType == SystemTypeEnum.GOP))
                {
                    // [STM_CATANIA] Allocated 상태 아이콘 UI 업데이트
                    Loader.Foups[foupNumber - 1].Slots.ToList().ForEach(x => x.IsPreSelected = ActiveLotInfos[foupNumber - 1].UsingSlotList.Contains(x.Index));
                }
            }
            else
            {
                LoggerManager.Debug($"[LOADER SUPERVISOR] SelectSlot LOTID Mismatching Error- foup number : {foupNumber} ActiveProcess LOTID:{ActiveLotInfos[foupNumber - 1].LotID} Select Slot LOTID:{lotID}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        public EventCodeEnum VerifyParam(int foupNumber, string lotID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (foupNumber <= 0 | foupNumber > ActiveLotInfos.Count)
                {
                    LoggerManager.Debug($"[LOADER SUPERVISOR] VerifyParam Foup Number Matched Error - foup number : {foupNumber}");
                    return retVal;
                }

                bool lotidCheck = false;
                if (ActiveLotInfos[foupNumber - 1].LotID == "")
                {
                    lotidCheck = true;
                }
                else
                {
                    lotidCheck = false;
                }

                if (ActiveLotInfos[foupNumber - 1].LotID.Equals(lotID) || lotidCheck)
                {
                    bool successflag = true;
                    int[] verifyParamResult = Enumerable.Repeat<int>(0, StageCount).ToArray<int>();

                    var targetActiveInfo = ActiveLotInfos[foupNumber - 1];

                    foreach (var stageidx in targetActiveInfo.UsingStageIdxList)
                    {
                        var ret = loaderCommunicationManager.GetProxy<IParamManagerProxy>(stageidx)?.VerifyLotVIDsCheckBeforeLot() ?? EventCodeEnum.UNDEFINED;

                        if (ret == EventCodeEnum.NONE)
                        {
                            if (verifyParamResult.Count() > stageidx - 1)
                            {
                                verifyParamResult[stageidx - 1] = 1;
                            }
                        }
                        else
                        {
                            successflag = false;
                        }
                    }

                    string verifyParamMap = string.Join("", verifyParamResult.Select(x => x.ToString()).ToArray());

                    PIVInfo pIVInfo = new PIVInfo(foupnumber: foupNumber, verifyparammap: verifyParamMap);
                    if (successflag)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(ParameterVerifySuccessEvent).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
                        semaphore.Wait();
                    }
                    else
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(ParameterVerifyFailEvent).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
                        semaphore.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private int LotPriority = 1;
        public EventCodeEnum ExternalLotOPStart(int foupNum, string lotID)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (ActiveLotInfos[foupNum - 1].LotID == lotID)
                {
                    bool isFirst = true;
                    foreach (var info in ActiveLotInfos)
                    {
                        if (info.State == LotStateEnum.Running)
                        {
                            isFirst = false;
                        }
                    }
                    if (isFirst)
                    {
                        LotPriority = 1; //모든 셀이 러닝스테이트가 아닐 경우 우선순위를 변경한다.
                    }

                    if (ActiveLotInfos.Where(item => item.LotPriority == LotPriority).ToList().Count > 0)
                    {
                        LotPriority = ActiveLotInfos.OrderBy(item => item.LotPriority).Last().LotPriority + 1;
                        LoggerManager.Debug($"ActiveLotInfos LotPriority is exist. Change Foup{foupNum} LotPriority:{LotPriority}");
                    }

                    this.FoupOpModule().FoupControllers[foupNum - 1].SetLock(true);

                    var slots = Loader.ModuleManager.GetTransferObjectAll().FindAll(
                            obj => obj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                             (obj.OriginHolder.Index >= ((foupNum - 1) * 25 + 1)) && (obj.OriginHolder.Index <= ((foupNum) * 25)));
                    foreach (var slot in slots)
                    {
                        var existSlot = ActiveLotInfos[foupNum - 1].UsingSlotList.Find(slotinfo => (slotinfo + ((foupNum - 1) * 25)) == slot.OriginHolder.Index);
                        if (existSlot != 0)
                        {
                            slot.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            slot.LOTID = lotID;                            
                        }
                    }

                    //ISSD-2953
                    ICassetteModule cstModule = this.Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, foupNum) as ICassetteModule;
                    var usingstage = GetClient(ActiveLotInfos[foupNum - 1].UsingStageIdxList[0]);
                    var device = usingstage.GetDeviceInfo();

                    if (ActiveLotInfos[foupNum - 1].CellDeviceInfoDic == null ||
                       ActiveLotInfos[foupNum - 1].CellDeviceInfoDic?.Count() == 0)
                    {

                        OCRDevParameter ocrParam = new OCRDevParameter();
                        var getparam = cont.Resolve<ProberInterfaces.Loader.IDeviceManager>().GetOCRDevParameter(device.DeviceName.Value);
                        if (getparam.retVal == EventCodeEnum.NONE)
                        {
                            ocrParam = getparam.param as OCRDevParameter;
                        }

                        this.Loader.ModuleManager.SetCstDevice(foupNum,
                                                                device.DeviceName.Value,
                                                                device.NotchAngle.Value,
                                                                device.SlotNotchAngle.Value,
                                                                device.Size.Value,
                                                                device.NotchType,
                                                                ocrParam
                                                                );
                        SetDeviceName(foupNum, device.DeviceName.Value);

                        var scanslots = Loader.ModuleManager.FindSlots(cstModule);
                        foreach (var slot in scanslots)
                        {
                            if (slot.Holder.TransferObject != null)
                            {
                                slot.Holder.ChangeDeviceInfo(slot.Holder.TransferObject);
                            }
                        }
                    }

                    if (ActiveLotInfos.All(lot => lot.State == LotStateEnum.Idle))
                    {
                        isTryFirstLoadSeq = true;
                    }

                    ActiveLotInfos[foupNum - 1].LotPriority = LotPriority++;
                    Loader.Foups[foupNum - 1].LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;
                    ActiveLotInfos[foupNum - 1].State = LotStateEnum.Running;
                    Loader.Foups[foupNum - 1].LotState = ActiveLotInfos[foupNum - 1].State;
                    Loader.Foups[foupNum - 1].DoPMICount = ActiveLotInfos[foupNum - 1].DoPMICount;

                    Loader.Foups[foupNum - 1].LotStartTime = DateTime.Now;
                    Loader.Foups[foupNum - 1].LotEndTime = default(DateTime);

                    LoggerManager.Debug($"[LOT START]  FOUP:{foupNum} LOT ID: {lotID}");
                    LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.START, $"FOUP:{foupNum} LOT ID: {lotID}", isLoaderMap: true);
                    LoggerManager.SetLotInfo(foupNum, lotID);

                    var cassettemodules = Loader.ModuleManager.FindModules<ICassetteModule>();
                    for (int f = 0; f < cassettemodules.Count(); f++)
                    {
                        LoggerManager.SetSlotInfo(f + 1, Loader.ModuleManager.FindSlots(cassettemodules[f]).Count());
                    }

                    PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum, lotid: lotID);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LoaderLotStartEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    ExternalCellLotOPStart(foupNum, lotID, ActiveLotInfos[foupNum - 1].CST_HashCode);
                    if (StateObj.ModuleState == ModuleStateEnum.IDLE)
                    {
                        ChangeExternalMode();
                        Thread.Sleep(1000);
                        (this).CommandManager().SetCommand<IGPLotOpStart>(this);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ExternalTCW_Start(int foupNum, string lotID)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (ActiveLotInfos[foupNum - 1].LotID == lotID)
                {
                    // SelectedLotInfo = ActiveLotInfos[foupNum - 1];

                    bool isFirst = true;
                    foreach (var info in ActiveLotInfos)
                    {
                        if (info.State == LotStateEnum.Running)
                        {
                            isFirst = false;
                        }
                    }
                    if (isFirst)
                    {
                        LotPriority = 1; //모든 셀이 러닝스테이트가 아닐 경우 우선순위를 변경한다.
                    }

                    if (ActiveLotInfos.Where(item => item.LotPriority == LotPriority).ToList().Count > 0)
                    {
                        LotPriority = ActiveLotInfos.OrderBy(item => item.LotPriority).Last().LotPriority + 1;
                        LoggerManager.Debug($"ActiveLotInfos LotPriority is exist. Change Foup{foupNum} LotPriority:{LotPriority}");
                    }

                    ActiveLotInfos[foupNum - 1].LotPriority = LotPriority++;
                    Loader.Foups[foupNum - 1].LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;
                    ActiveLotInfos[foupNum - 1].State = LotStateEnum.Running;
                    Loader.Foups[foupNum - 1].LotState = ActiveLotInfos[foupNum - 1].State;
                    Loader.Foups[foupNum - 1].DoPMICount = ActiveLotInfos[foupNum - 1].DoPMICount;
                    ICassetteModule cstModule = this.Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, foupNum) as ICassetteModule;


                    this.FoupOpModule().FoupControllers[foupNum - 1].SetLock(true);

                    var slots = Loader.ModuleManager.GetTransferObjectAll().FindAll(
                            obj => obj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                             (obj.OriginHolder.Index >= ((foupNum - 1) * 25 + 1)) && (obj.OriginHolder.Index <= ((foupNum) * 25)));
                    foreach (var slot in slots)
                    {


                        var existSlot = ActiveLotInfos[foupNum - 1].UsingSlotList.Find(slotinfo => (slotinfo + ((foupNum - 1) * 25)) == slot.OriginHolder.Index);
                        if (existSlot != 0)
                        {

                            slot.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            slot.UsingStageList = ActiveLotInfos[foupNum - 1].UsingStageIdxList;
                            slot.LOTID = lotID;
                        }
                    }

                    LoggerManager.Debug($"[TCW START]  FOUP:{foupNum} LOT ID: {lotID}");
                    LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.START, $"FOUP:{foupNum} LOT ID: {lotID}", isLoaderMap: true);
                    LoggerManager.SetLotInfo(foupNum, lotID);
                    for (int f = 0; f < LoaderInfo.StateMap.CassetteModules.Length; f++)
                    {
                        LoggerManager.SetSlotInfo(f + 1, LoaderInfo.StateMap.CassetteModules[f].SlotModules.Length);
                    }

                    Loader.Foups[foupNum - 1].LotStartTime = DateTime.Now;
                    Loader.Foups[foupNum - 1].LotEndTime = default(DateTime);

                    //ISSD-2953
                    var usingstage = GetClient(ActiveLotInfos[foupNum - 1].UsingStageIdxList[0]);
                    var device = usingstage.GetDeviceInfo();

                    //if(ActiveLotInfos[foupNum - 1].DeviceName == "DEFAULTDEVNAME" ||
                    //    ActiveLotInfos[foupNum - 1].DeviceName == null ||
                    //    ActiveLotInfos[foupNum - 1].DeviceName == string.Empty)
                    {

                        OCRDevParameter ocrParam = new OCRDevParameter();
                        var getparam = cont.Resolve<ProberInterfaces.Loader.IDeviceManager>().GetOCRDevParameter(device.DeviceName.Value);
                        if (getparam.retVal == EventCodeEnum.NONE)
                        {
                            ocrParam = getparam.param as OCRDevParameter;
                        }

                        this.Loader.ModuleManager.SetCstDevice(foupNum,
                                                                device.DeviceName.Value,
                                                                device.NotchAngle.Value,
                                                                device.SlotNotchAngle.Value,
                                                                device.Size.Value,
                                                                device.NotchType,
                                                                ocrParam
                                                                );
                        SetDeviceName(foupNum, device.DeviceName.Value); // 주석풀기

                        var scanslots = Loader.ModuleManager.FindSlots(cstModule);
                        foreach (var slot in scanslots)
                        {
                            if (slot.Holder.TransferObject != null)
                            {
                                slot.Holder.ChangeDeviceInfo(slot.Holder.TransferObject);

                            }

                        }
                    }
                    foreach (var slotIdx in ActiveLotInfos[foupNum - 1].UsingSlotList)
                    {
                        var realWafer = Loader.ModuleManager.GetTransferObjectAll().Find(wafer => wafer.CurrHolder.ModuleType == ModuleTypeEnum.SLOT && wafer.CurrHolder.Index == slotIdx + ((foupNum - 1) * 25));
                        realWafer.WaferType.Value = EnumWaferType.TCW;
                        realWafer.ChuckNotchAngle.Value = Loader.SystemParameter.TCW_LoadingAngle.Value;
                        realWafer.NotchAngle.Value = Loader.SystemParameter.TCW_LoadingAngle.Value;
                        realWafer.SlotNotchAngle.Value = Loader.SystemParameter.TCW_UnloadingAngle.Value;
                    }

                    PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum, lotid: lotID);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LoaderLotStartEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    ExternalCellLotOPStart(foupNum, lotID, ActiveLotInfos[foupNum - 1].CST_HashCode);
                    if (StateObj.ModuleState == ModuleStateEnum.IDLE)
                    {
                        ChangeExternalMode();
                        //delays.DelayFor(1000);
                        Thread.Sleep(1000);
                        (this).CommandManager().SetCommand<IGPLotOpStart>(this);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void ExternalCellLotOPStart(int foupNumber, string lotID, string cstHashCode)
        {
            try
            {
                var ccsupervisor = cont.Resolve<ICardChangeSupervisor>();
                if (ActiveLotInfos[foupNumber - 1].State == LotStateEnum.Running)
                {
                    for (int i = 0; i < ActiveLotInfos[foupNumber - 1].UsingStageIdxList.Count; i++)
                    {
                        var client = GetClient(ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]);
                        if (client != null && IsAliveClient(client))
                        {
                            client.SetLotOut(false);
                            client.SetAbort(false);
                            client.SetLotStarted(foupNumber, ActiveLotInfos[foupNumber - 1].LotID, ActiveLotInfos[foupNumber - 1].CST_HashCode);


                            if (StageStates[ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i] - 1] == ModuleStateEnum.IDLE)
                            {
                                if (loaderCommunicationManager.Cells[ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i] - 1].StageMode == GPCellModeEnum.ONLINE
                                    && ccsupervisor.IsAllocatedStage(ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]) == false)
                                {
                                    var isSucess = LotOPStartClient(client, lotID: lotID, foupnumber: foupNumber, cstHashCodeOfRequestLot: cstHashCode);
                                    isLotStartFlag[ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i] - 1] = isSucess;
                                    LoggerManager.Debug($"Client #{ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]} LotStart sucessed ");
                                    //  LoggerManager.LOTLog($"[Cell{ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]}] [LOT] LotStart sucessed ");
                                }
                                else
                                {
                                    isLotStartFlag[ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i] - 1] = false;
                                    bool ClientAbort = LotOPResetAbortClient(client);
                                    string reason = null;
                                    if (IsAliveClient(client) == false)
                                        reason = "The client is not available,";
                                    if (loaderCommunicationManager.Cells[ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i] - 1].StageMode != GPCellModeEnum.ONLINE)
                                        reason += "The target is not int the ONLINE state,";
                                    if (ccsupervisor.IsAllocatedStage(ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]) != false)
                                        reason += "Is allocated automation cardchange,";
                                    LoggerManager.Debug($"Client #{ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]} LotStart failed : {reason} Reset Abort {ClientAbort}");
                                    LoggerManager.ActionLog(ModuleLogType.LOT_SETTING, StateLogType.ABORT, $"Reason : {reason}", ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i], isLoaderMap: true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public int RunningStageCount(int cellIndex)
        {
            int count = 0;
            try
            {
                for (int i = 0; i < ActiveLotInfos.Count; i++)
                {
                    if (ActiveLotInfos[i].State == LotStateEnum.Running || ActiveLotInfos[i].State == LotStateEnum.Cancel ||
                        (ActiveLotInfos[i].State == LotStateEnum.Idle && ActiveLotInfos[i].AssignState == LotAssignStateEnum.ASSIGNED))
                    {
                        int cellIdx = ActiveLotInfos[i].UsingStageIdxList.Find(cell => cell == cellIndex);
                        if (cellIdx == cellIndex)
                        {
                            count++;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return count;
        }
        public void ExternalLotOPEnd(int foupNumber)
        {
            try
            {
                for (int i = priorityNum; i < ActiveLotInfos[foupNumber - 1].UsingStageIdxList.Count; i++) //UsingStageIdxList : cell01,02
                {
                    if (RunningStageCount(ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]) <= 1)
                    {
                        ILoaderServiceCallback callback = GetClient(ActiveLotInfos[foupNumber - 1].UsingStageIdxList[i]);

                        if (IsAliveClient(callback))
                        {
                            ModuleStateEnum modulestate = callback.GetLotState();

                            if (modulestate != ModuleStateEnum.IDLE)
                            {
                                var currentMap = LoaderInfo.StateMap;
                                foreach (var chuckModule in currentMap.ChuckModules)
                                {
                                    var client = GetClient(chuckModule.ID.Index);

                                    if (client != null)
                                    {
                                        if (loaderCommunicationManager.Cells[chuckModule.ID.Index - 1].LockMode == StageLockMode.UNLOCK)
                                        {
                                            if ((chuckModule.WaferStatus == EnumSubsStatus.EXIST &&
                                                (chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT | chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)))
                                            {
                                                if (DynamicMode == DynamicModeEnum.NORMAL)
                                                {
                                                    if (chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                                    {
                                                        int slotNum = chuckModule.Substrate.OriginHolder.Index % 25;
                                                        int offset = 0;
                                                        if (slotNum == 0)
                                                        {
                                                            slotNum = 25;
                                                            offset = -1;
                                                        }

                                                        int foupNum = ((chuckModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                                                        if (foupNum == foupNumber & client != null & isLotStartFlag[chuckModule.ID.Index - 1] != false)
                                                        {
                                                            //var isSucess = LotOPEndClient(GetClient(chuckModule.ID.Index), foupNumber);
                                                            //isLotStartFlag[chuckModule.ID.Index - 1] = !isSucess;
                                                            isLotStartFlag[chuckModule.ID.Index - 1] = false;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (client != null)
                                                        {
                                                            string cellLotID = client.GetStageInfo().LotID ?? "";
                                                            if (cellLotID.Equals(ActiveLotInfos[foupNumber - 1].LotID) & isLotStartFlag[chuckModule.ID.Index - 1] != false)
                                                            {
                                                                //var isSucess = LotOPEndClient(client, foupNumber);
                                                                //isLotStartFlag[chuckModule.ID.Index - 1] = !isSucess;
                                                                isLotStartFlag[chuckModule.ID.Index - 1] = false;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (client != null)
                                                {
                                                    //string cellLotID = client.GetStageInfo().LotID ?? "";
                                                    //if (cellLotID.Equals(ActiveLotInfos[foupNumber - 1].LotID))
                                                    ActiveLotInfo activeLotInfo = ActiveLotInfos[foupNumber - 1];
                                                    if (activeLotInfo != null)
                                                    {
                                                        if (client.CheckCurrentAssignLotInfo(activeLotInfo.LotID, activeLotInfo.CST_HashCode))
                                                        {
                                                            //var isSucess = LotOPEndClient(client, foupNumber);
                                                            //isLotStartFlag[chuckModule.ID.Index - 1] = !isSucess;
                                                            isLotStartFlag[chuckModule.ID.Index - 1] = false;
                                                        }
                                                        else
                                                        {
                                                            //checkend.
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GPLoader.LoaderBuzzer(true);
                                            this.MetroDialogManager().ShowMessageDialog("Cell LOT End Failed", $"Cell{chuckModule.ID.Index} BackSide Door is Opened.", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[LoaderSupervisor], ExternalLotOPEnd() : Failed,");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Queue<ActiveLotInfo> QueueActiveInfo = new Queue<ActiveLotInfo>();
        public void ActiveLotQueueReset()
        {
            try
            {
                QueueActiveInfo.Clear();
                for (int i = 0; i < ActiveLotInfos.Count; i++)
                {
                    QueueActiveInfo.Enqueue(ActiveLotInfos[i]);
                }
            }
            catch (Exception)
            {

            }
        }
        public Queue<int> PreJobRingBuffer = new Queue<int>();

        public void InternalLotInit()
        {
            try
            {
                for (int i = 0; i < Clients.Values.Count; i++)
                {
                    var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Name.Contains(Clients.Values.ElementAt(i).GetChuckID().Index.ToString()));
                    //var cellInfo = cells.ToList<StageObject>().Find(cell => cell.Name.Contains(Clients.Keys.ElementAt(i)));
                }
            }
            catch (Exception)
            {

            }
        }
        private bool IsAllErrorEndStage(ActiveLotInfo lotInfo)
        {
            bool retVal = true;

            try
            {
                ErrorEndStateEnum state = ErrorEndStateEnum.NONE;

                foreach (var stageidx in lotInfo.UsingStageIdxList)
                {
                    var stage = GetClient(stageidx);
                    if (stage != null && IsAliveClient(stage))
                    {
                        state = stage.GetErrorEndState();
                    }
                    else
                    {
                        //IsAliveClient 및 stagewatchdog에서 끊기는 시점에 로그를 남긴다.
                        //LoggerManager.Error($"[LoaderSupervisor], IsAllErrorEndStage() : Stage #{stageidx} is not alive.");
                    }

                    if (state == ErrorEndStateEnum.NONE) // #Hynix_Merge 이부분 Dev_Integrated랑 다름. 보완해야할 부분으로 보임.
                    {
                        retVal = false;
                        return retVal;
                    }
                    else if (stage.GetWaferState() == EnumWaferState.PROBING || stage.GetWaferState() == EnumWaferState.TESTED)
                    {
                        retVal = false;
                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                retVal = false;
                LoggerManager.Exception(err);
            }

            return retVal;
        }        
        
        public bool IsAllStageOut(ActiveLotInfo lotinfo)
        {
            bool retVal = true;

            try
            {
                foreach (var usingstageidx in lotinfo.UsingStageIdxList)
                {
                    var usingstage = GetClient(usingstageidx);
                    if (IsAliveClient(usingstage))
                    {
                        if (usingstage.GetLotOutState() == false)
                        {
                            retVal = false;
                            break;
                        }
                    }
                    else
                    {
                        retVal = false;
                        break;
                    }

                }
            }
            catch (Exception err)
            {
                retVal = false;
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public LoaderMap ExternalRequestJob1(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            isLotEnd = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotPause = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotInitFlag = new bool[SystemModuleCount.ModuleCnt.FoupCount];

            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
                isLotPause[i] = false;
            }

            isLoaderEnd = false;
            Queue<String> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();

            try
            {
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();
                LoaderInfo = Loader.GetLoaderInfo();
                CheckUsingStage();
                CheckActiveLotInfo();
                List<string> lotRuningList = new List<string>();
                List<LotStateEnum> lotStateEnums = new List<LotStateEnum>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    lotStateEnums.Add(ActiveLotInfos[lotIdx].State);
                }


                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running ||
                       lotStateEnums[lotIdx] == LotStateEnum.Abort ||
                       lotStateEnums[lotIdx] == LotStateEnum.Cancel ||
                       lotStateEnums[lotIdx] == LotStateEnum.Suspend ||
                       lotStateEnums[lotIdx] == LotStateEnum.Pause)
                    {
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[lotIdx].CST_HashCode;

                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running)
                    {
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.LOTRunning_CSTHash_List.Contains(w.CST_HashCode)).FirstOrDefault();
                            if (wafer == null)
                            {
                                //Lot End Scan 불일치
                                Task.Run(() =>
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Selected Wafer Not Matched Error", $"The selected wafer does not Exist. \nCassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", EnumMessageStyle.Affirmative).Result;

                                });
                                isLotPause[lotIdx] = true;

                                PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.ERROR, $"Selected Wafer Not Matched Error. Cassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", isLoaderMap: true);

                                return null;
                            }

                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;

                            if (wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                }
                            }
                            else
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            int chuckIndx = ActiveLotInfos[lotIdx].UsingStageIdxList[i];

                            var ExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.EXIST && module.Substrate.WaferType.Value == EnumWaferType.POLISH).FirstOrDefault();
                            if (ExistChuckModule != null)
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++)
                        {
                            var client = GetClient(ActiveLotInfos[lotIdx].UsingStageIdxList[i]);

                            if (IsAliveClient(client))
                            {
                                bool IsLotEndReady = client.IsLotEndReady();

                                if (!IsLotEndReady)
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                        }

                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;
                        bool isFoupEnd = false;

                        var loaderJob = UnloadRequestJob(lotIdx + 1, out isFoupEnd);

                        if (isFoupEnd == true && loaderJob == null)
                        {
                            isLotEnd[lotIdx] = true;
                            return loaderJob;
                        }
                        else if (loaderJob == null && isFoupEnd == false)
                        {
                            isLotEnd[lotIdx] = false;
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }
                }

                bool isAcessBuffer = false;

                if (this.LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST) >= LoaderInfo.StateMap.BufferModules.Count() - 1)
                {
                    isAcessBuffer = true;
                }

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (!this.LoaderInfo.StateMap.BufferModules[i].Enable)
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        if ((!isAcessBuffer) && this.LoaderInfo.StateMap.BufferModules[i].WaferStatus == EnumSubsStatus.EXIST)
                        {
                            this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                        }
                        else
                        {
                            this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                        }
                    }
                }
                var PAModules = this.Loader.ModuleManager.FindModules<IPreAlignModule>();
                for (int i = 0; i < PAModules.Count; i++)
                {
                    var pa = this.LoaderInfo.StateMap.PreAlignModules.Where(p => p.ID.Index == PAModules[i].ID.Index).FirstOrDefault();
                    if (pa != null)
                    {
                        if (PAModules[i].PAStatus != ProberInterfaces.PreAligner.EnumPAStatus.Idle)
                        {
                            pa.ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                        }
                        else
                        {
                            pa.ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                        }
                    }
                }

                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;
                bool isPAExistWafer = true;
                bool isAbortWafer = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                LoaderMap isPAExistMap = null;

                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                isPAExistMap = IsExistLoaderPA_Normal(LoaderInfo.StateMap, out isPAExistWafer);


                if (isArmExistWafer)
                {
                    LoaderInfo.StateMap = isExistMap;
                }
                else if (isPAExistWafer)
                {
                    LoaderInfo.StateMap = isPAExistMap;
                }
                else
                {
                    isExistMap = IsOCRAbort(LoaderInfo.StateMap, out isAbortWafer);

                    if (isAbortWafer)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {
                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;
                        List<int> PreJobRingBufferCellIndexs = new List<int>();
                        if (PreJobRingBuffer.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                {
                                    var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                    if (avaibleBuffer != null)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";
                                        cell = null;
                                        LoaderMap map = null;

                                        if (PreJobRingBuffer.Count > 0)
                                        {
                                            int chuckidx = PreJobRingBuffer.Peek();
                                            cell = GetClient(chuckidx);
                                            PreJobRingBufferCellIndexs.Add(chuckidx);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        if (cell != null)
                                        {
                                            map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                            if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                            {
                                                LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                map = null;
                                            }

                                            if (map != null)
                                            {
                                                LoaderInfo.StateMap = map;
                                                allwafer = map.GetTransferObjectAll();
                                                foreach (var wafer in allwafer)
                                                {
                                                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                    ModuleID dstPos = wafer.CurrPos;
                                                    if (currSubObj.CurrPos != dstPos)
                                                    {
                                                        currSubObj.DstPos = dstPos;
                                                        if (wafer.Priority > 999)
                                                        {
                                                            if (isExchange)
                                                            {
                                                                exchangeCnt++;
                                                                bufferCount++;
                                                                wafer.Priority = exchangePriority++;
                                                            }
                                                            else
                                                            {
                                                                bufferCount++;
                                                                wafer.Priority = Priority++;
                                                            }
                                                        }
                                                    }
                                                }
                                                ReservationJob(map, avaibleBuffer);
                                                loaderMapList.Add(map);
                                                PreJobRingBuffer.Dequeue();
                                                if (exchangeCnt >= 2)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                                if (MapSlicerErrorFlag)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                PreJobRingBuffer.Dequeue();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                LoggerManager.Debug("PreJobRingBuffer exception Error");
                                PreJobRingBuffer.Dequeue();
                            }
                        }

                        if (!jobdone)
                        {
                            //웨이퍼 없는 Cell부터 공급..
                            for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                            {
                                var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);
                                if (activeLotInfo.State == LotStateEnum.Running)
                                {
                                    for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                        if (avaibleBuffer != null)
                                        {
                                            int chuckIndx = activeLotInfo.UsingStageIdxList[i];

                                            var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                                            LoaderMap map = null;

                                            if (PreJobRingBufferCellIndexs.Contains(chuckIndx))
                                            {
                                                continue;
                                            }
                                            else if (notExistChuckModule != null)
                                            {
                                                cell = GetClient(activeLotInfo.UsingStageIdxList[i]);
                                                if (cell != null && IsAliveClient(cell))
                                                {
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                        map = null;
                                                    }

                                                    if (bufferCount >= 4)
                                                    {
                                                        jobdone = true;
                                                        break;
                                                    }

                                                    if (map != null)
                                                    {
                                                        isExistWafer = false;
                                                        ReservationJob(map, avaibleBuffer);
                                                        LoaderInfo.StateMap = map;
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                currSubObj.DstPos = dstPos;
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                        loaderMapList.Add(map);

                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else if (isNeedWafer)
                                                    {
                                                        isExistWafer = false;
                                                        map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                currSubObj.DstPos = dstPos;
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        ReservationJob(map, avaibleBuffer);
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                }

                                if (jobdone)
                                {
                                    break;
                                }
                            }

                            if (isExistWafer)
                            {
                                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = QueueActiveInfo.Peek();
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        //if (activeLotInfo.RemainCount > activeLotInfo.UsingStageIdxList.Count)
                                        //{
                                        //    activeLotInfo.RemainCount = activeLotInfo.UsingStageIdxList.Count;
                                        //}

                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                            if (avaibleBuffer != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";

                                                int chuckIndx = 0;

                                                chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);
                                                if (cell == null || !IsAliveClient(cell) || PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                                {
                                                    //연결이 끊긴 cell에 대해 RingBuffer내의 index 조절을 위해 dequeue/Enqueue 추가
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                }
                                                else
                                                {
                                                    LoaderMap map = null;

                                                    if (!ClientList.Contains(cell))
                                                    {
                                                        ClientList.Add(cell);

                                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                        if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                        {
                                                            LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                            map = null;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        continue;
                                                    }

                                                    if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }

                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                currSubObj.DstPos = dstPos;
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }

                                                        LoaderInfo.StateMap = map;
                                                        ReservationJob(map, avaibleBuffer);
                                                        loaderMapList.Add(map);
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        PreJobRingBuffer.Enqueue(chuckIndx);

                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (map != null)
                                                        {
                                                            ReservationJob(map, avaibleBuffer);
                                                            LoaderInfo.StateMap = map;
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    currSubObj.DstPos = dstPos;
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        if (isExchange)
                                                                        {
                                                                            exchangeCnt++;
                                                                            bufferCount++;
                                                                            wafer.Priority = exchangePriority++;
                                                                        }
                                                                        else
                                                                        {
                                                                            bufferCount++;
                                                                            wafer.Priority = Priority++;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);

                                                            if (exchangeCnt > 2)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }
                                                        //else if 로직은 탈 수 있는 로직이 아닌 것으로 판단 됨.
                                                        else if (isNeedWafer)
                                                        {
                                                            map = PreLoadingJobMake(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                            //activeLotInfo.RingBuffer.Dequeue();
                                                            //activeLotInfo.RingBuffer.Enqueue(cell);

                                                            if (map == null)
                                                            {
                                                                continue;
                                                            }

                                                            if (bufferCount >= 4)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    currSubObj.DstPos = dstPos;
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        bufferCount++;
                                                                        wafer.Priority = PreLoadingPriority++;
                                                                    }
                                                                }
                                                            }
                                                            ReservationJob(map, avaibleBuffer);
                                                            LoaderInfo.StateMap = map;
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                            PreJobRingBuffer.Enqueue(chuckIndx);
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }

                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);
                                }
                            }
                        }

                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                // TODO :

                                bool LotEndFlag = false;

                                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                {
                                    if (isLotEnd[i] == true)
                                    {
                                        LotEndFlag = true;
                                    }
                                    else
                                    {
                                        LotEndFlag = false;
                                        break;
                                    }
                                }

                                if (LotEndFlag)
                                {
                                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                    {
                                        if (ActiveLotInfos[i].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }

                                //if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                //{
                                //    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                //    {
                                //        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                //    }
                                //    else
                                //    {
                                //        isLoaderEnd = true;
                                //    }
                                //}

                                return null;
                            }
                            else
                            {
                                HolderModuleInfo avaibleBuffer = null;
                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                int lowLotPriortyCellIndex = 0;
                                int lowLotPriortyCellsCount = 0;
                                int existDeviceCount = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();
                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();
                                for (int i = 0; i < ActiveLotInfos.Count; i++)
                                {

                                    var activeLotInfo = ActiveLotInfos[i];
                                    if (activeLotInfo.State == LotStateEnum.Running && (isLotEnd[i] == true && isLotInitFlag[i] == true)) //Lot End Trigger가 될 수 있는 조건에서는 Buffering 하지 못하도록 함.
                                    {
                                        if (activeLotInfo.UsingStageIdxList.Count > 0)
                                        {
                                            if (lowLotPriorty.LotPriority == activeLotInfo.LotPriority)
                                            {
                                                lowLotPriortyCellIndex = activeLotInfo.UsingStageIdxList[0];
                                                lowLotPriortyCellsCount = activeLotInfo.UsingStageIdxList.Count;
                                                DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                                CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                            }
                                            DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                            CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                        }
                                    }
                                }
                                int jobCount = 0;// 버퍼에 다 담는게 아니고 2개씩 끊어서 담아야한다.
                                do
                                {
                                    if (jobCount >= 2)
                                    {
                                        break;
                                    }

                                    avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                    if (avaibleBuffer != null)
                                    {
                                        if (DeviceNames.Count > 0)
                                        {
                                            devName = DeviceNames.Peek();
                                            DeviceNames.Dequeue();
                                        }
                                        if (CellIndexs.Count > 0)
                                        {
                                            cellIdx = CellIndexs.Peek();
                                            CellIndexs.Dequeue();
                                        }

                                        if (lowLotPriortyCellIndex == cellIdx)
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(lowLotPriortyCellIndex));
                                            if (existDeviceCount < 2 || (lowLotPriortyCellsCount == LoaderInfo.StateMap.ChuckModules.Count())) // 버퍼 2개까지 허용 또는 모든 Stage가 LOT에 할당되었을때는 Full Buffer
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }
                                        else
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(cellIdx));
                                            if (existDeviceCount < 1) // 버퍼 1개까지 허용
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }

                                        if (map != null)
                                        {
                                            ReservationJob(map, avaibleBuffer);
                                            LoaderInfo.StateMap = map;
                                            jobCount++;

                                            if (MapSlicerErrorFlag)
                                            {
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                    }
                                } while (avaibleBuffer != null && DeviceNames.Count > 0);
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        isLoaderEnd = true;
                                    }
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

            return LoaderInfo.StateMap;
        }

        private bool isTryFirstLoadSeq = false;
        private bool isFirstLoadSeq = false;
        public LoaderMap ExternalRequestJob_DRAX(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            isLotEnd = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotInitFlag = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotPause = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
                isLotPause[i] = false;
            }

            isLoaderEnd = false;

            Queue<String> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();

            try
            {
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                int UnProcessedWaferCount = 0;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                LoaderInfo = Loader.GetLoaderInfo();
                CheckUsingStage();
                CheckActiveLotInfo();
                List<string> lotRuningList = new List<string>();
                List<LotStateEnum> lotStateEnums = new List<LotStateEnum>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    lotStateEnums.Add(ActiveLotInfos[lotIdx].State);
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running ||
                       lotStateEnums[lotIdx] == LotStateEnum.Abort ||
                       lotStateEnums[lotIdx] == LotStateEnum.Cancel ||
                       lotStateEnums[lotIdx] == LotStateEnum.Suspend ||
                       lotStateEnums[lotIdx] == LotStateEnum.Pause)
                    {
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[lotIdx].CST_HashCode;

                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;

                    if (lotStateEnums[lotIdx] == LotStateEnum.Running)
                    {
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.LOTRunning_CSTHash_List.Contains(w.CST_HashCode)).FirstOrDefault();

                            if (wafer == null)
                            {
                                //Lot End Scan 불일치
                                Task.Run(() =>
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Selected Wafer Not Matched Error", $"The selected wafer does not Exist. \nCassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", EnumMessageStyle.Affirmative).Result;
                                });
                                isLotPause[lotIdx] = true;

                                //     isLotEnd[lotIdx] = true;

                                PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.ERROR, $"Selected Wafer Not Matched Error. Cassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", isLoaderMap: true);

                                //isLotEnd[lotIdx] = true;
                                return null;
                            }

                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                            {
                                UnProcessedWaferCount++;
                            }
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);

                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;

                            if (wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                }
                            }
                            else
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            int chuckIndx = ActiveLotInfos[lotIdx].UsingStageIdxList[i];

                            var ExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.EXIST && module.Substrate.WaferType.Value == EnumWaferType.POLISH).FirstOrDefault();

                            if (ExistChuckModule != null)
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++)
                        {
                            var client = GetClient(ActiveLotInfos[lotIdx].UsingStageIdxList[i]);

                            if (client != null && IsAliveClient(client))
                            {
                                bool IsLotEndReady = client.IsLotEndReady();

                                if (!IsLotEndReady)
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[LoaderSupervisor], ExternalRequestJob1() : Failed");
                            }
                        }
                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;
                        bool isFoupEnd = false;
                        SetSkipWaferOnChuck(ActiveLotInfos[lotIdx]);
                        var loaderJob = UnloadRequestJob(lotIdx + 1, out isFoupEnd);

                        if (isFoupEnd == true && loaderJob == null)
                        {
                            isLotEnd[lotIdx] = true;
                            return loaderJob;
                        }
                        else if (loaderJob == null && isFoupEnd == false)
                        {
                            isLotEnd[lotIdx] = false;
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }
                }


                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (this.LoaderInfo.StateMap.BufferModules[i].Enable)
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                }

                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;
                bool isPAExistWafer = true;
                bool isAbortWafer = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                LoaderMap isPAExistMap = null;

                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                isPAExistMap = IsExistLoaderPA_Normal(LoaderInfo.StateMap, out isPAExistWafer);



                bool isFullBufferLoadSeq = false;

                bool isFullArm = false;
                bool isFullBuffer = false;
                int fullBufferLotIndex = 0;
                List<int> loadStageOrderList = new List<int>();
                loadStageOrderList.Add(1);
                loadStageOrderList.Add(2);
                loadStageOrderList.Add(3);
                loadStageOrderList.Add(4);
                loadStageOrderList.Add(5);
                loadStageOrderList.Add(6);
                loadStageOrderList.Add(7);
                loadStageOrderList.Add(8);
                loadStageOrderList.Add(9);
                loadStageOrderList.Add(10);
                loadStageOrderList.Add(11);
                loadStageOrderList.Add(12);

                if(!isPAExistWafer)
                {
                    for (int i = 0; i < ActiveLotInfos.Count; i++)
                    {
                        if (ActiveLotInfos[i].State == LotStateEnum.Running && UnProcessedWaferCount >= ActiveLotInfos[i].UsingStageIdxList.Count())
                        {

                            if (ActiveLotInfos[i].UsingStageIdxList.Count() == LoaderInfo.StateMap.ChuckModules.Count()) //LOT에 합류된 Stage 숫자가 전체 Stage랑 같은 경우 
                            {
                                var notExistChuckModuleCount = LoaderInfo.StateMap.ChuckModules.Where(module => module.WaferStatus == EnumSubsStatus.NOT_EXIST).Count();
                                var notExistBufferModuleCount = LoaderInfo.StateMap.BufferModules.Where(module => module.WaferStatus == EnumSubsStatus.NOT_EXIST).Count();
                                var notExistArmCount = LoaderInfo.StateMap.ARMModules.Where(module => module.WaferStatus == EnumSubsStatus.NOT_EXIST).Count();
                                if (notExistChuckModuleCount != 0)
                                {

                                    if (notExistChuckModuleCount >= notExistBufferModuleCount) //LOT에 합류된 Stage에 Wafer가 모두 없는 경우
                                    {
                                        fullBufferLotIndex = i;
                                        isFirstLoadSeq = true;

                                        if (notExistBufferModuleCount >= LoaderInfo.StateMap.BufferModules.Count() - LoaderInfo.StateMap.ARMModules.Count())
                                        {
                                            isFullBufferLoadSeq = true;
                                        }
                                        else if (notExistArmCount == 0)
                                        {
                                            isFullArm = true;
                                        }
                                        else if (notExistBufferModuleCount <= LoaderInfo.StateMap.BufferModules.Count() - LoaderInfo.StateMap.ARMModules.Count())
                                        {
                                            isFullBuffer = true;
                                        }

                                    }
                                    else if (notExistBufferModuleCount <= LoaderInfo.StateMap.BufferModules.Count() - LoaderInfo.StateMap.ARMModules.Count())
                                    {
                                        isFullBuffer = true;
                                    }
                                    else if (notExistArmCount == 0)
                                    {
                                        isFullArm = true;
                                    }
                                }
                                else
                                {
                                    isFirstLoadSeq = false;
                                }
                            }
                        }
                    }
                }


                if (isFirstLoadSeq && isFullBufferLoadSeq)
                {
                    for (int i = 0; i < ActiveLotInfos[fullBufferLotIndex].UsingStageIdxList.Count; i++)
                    {
                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";

                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                        int chuckIndx = ActiveLotInfos[fullBufferLotIndex].UsingStageIdxList[i];
                        var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();

                        LoaderMap map = null;
                        if (avaibleBuffer != null)
                        {
                            if (notExistChuckModule != null)
                            {
                                var cell = GetClient(ActiveLotInfos[fullBufferLotIndex].UsingStageIdxList[i]);

                                if (cell != null && IsAliveClient(cell))
                                {
                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                    {
                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                    }

                                    if (map != null)
                                    {
                                        isExistWafer = false;
                                        LoaderInfo.StateMap = map;
                                        allwafer = map.GetTransferObjectAll();
                                        foreach (var wafer in allwafer)
                                        {
                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                            ModuleID dstPos = wafer.CurrPos;
                                            if (currSubObj.CurrPos != dstPos)
                                            {
                                                currSubObj.DstPos = dstPos;
                                                if (wafer.Priority > 999)
                                                {
                                                    bufferCount++;
                                                    wafer.Priority = Priority++;
                                                }
                                            }
                                        }
                                        ReservationJob(map, avaibleBuffer);
                                        loaderMapList.Add(map);
                                        jobdone = true;
                                        if (MapSlicerErrorFlag)
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                    else if (isNeedWafer)
                                    {
                                        isExistWafer = false;
                                        map = PreLoadingJobMake_BufferCount_Minus_ArmCount(LoaderInfo.StateMap, ActiveLotInfos[fullBufferLotIndex].DeviceName, chuckIndx);

                                        if (map == null)
                                        {
                                            continue;
                                        }
                                        allwafer = map.GetTransferObjectAll();
                                        foreach (var wafer in allwafer)
                                        {
                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                            ModuleID dstPos = wafer.CurrPos;
                                            if (currSubObj.CurrPos != dstPos)
                                            {
                                                currSubObj.DstPos = dstPos;
                                                if (wafer.Priority > 999)
                                                {
                                                    bufferCount++;
                                                    wafer.Priority = PreLoadingPriority++;
                                                }
                                            }
                                        }
                                        ReservationJob(map, avaibleBuffer);
                                        LoaderInfo.StateMap = map;
                                        loaderMapList.Add(map);
                                        jobdone = true;
                                        if (MapSlicerErrorFlag)
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }

                                }
                                else
                                {
                                    continue;
                                }

                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (isFirstLoadSeq && isFullArm)
                {
                    for (int i = 0; i < loadStageOrderList.Count; i++)
                    {
                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";

                        int chuckIndx = loadStageOrderList[i];
                        var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                        var waferExistArmModule = LoaderInfo.StateMap.ARMModules.Where(module => module.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();
                        LoaderMap map = null;

                        if (notExistChuckModule != null && waferExistArmModule != null)
                        {
                            var cell = GetClient(chuckIndx);

                            if (cell != null && IsAliveClient(cell))
                            {
                                map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                {
                                    LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                }
                                if (map != null && waferExistArmModule != null)
                                {
                                    map = LoadWafer_In_Arm(LoaderInfo.StateMap, ActiveLotInfos[fullBufferLotIndex].DeviceName, chuckIndx);
                                    LoaderInfo.StateMap = map;
                                    allwafer = map.GetTransferObjectAll();
                                    foreach (var wafer in allwafer)
                                    {
                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                        ModuleID dstPos = wafer.CurrPos;
                                        if (currSubObj.CurrPos != dstPos)
                                        {
                                            currSubObj.DstPos = dstPos;
                                            if (wafer.Priority > 999)
                                            {
                                                wafer.Priority = Priority++;
                                            }
                                        }
                                    }
                                    loaderMapList.Add(map);
                                    if (MapSlicerErrorFlag)
                                    {
                                        jobdone = true;
                                        break;
                                    }
                                }
                                else if (map != null)
                                {
                                    LoaderInfo.StateMap = map;
                                    allwafer = map.GetTransferObjectAll();
                                    foreach (var wafer in allwafer)
                                    {
                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                        ModuleID dstPos = wafer.CurrPos;
                                        if (currSubObj.CurrPos != dstPos)
                                        {
                                            currSubObj.DstPos = dstPos;
                                            if (wafer.Priority > 999)
                                            {
                                                wafer.Priority = Priority++;
                                            }
                                        }
                                    }
                                    loaderMapList.Add(map);

                                    if (MapSlicerErrorFlag)
                                    {
                                        jobdone = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }

                        }
                        else
                        {
                            continue;
                        }
                    }

                }
                else if (isFirstLoadSeq && isFullBuffer)
                {


                    for (int i = 0; i < loadStageOrderList.Count; i++)
                    {
                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";

                        int chuckIndx = loadStageOrderList[i];
                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);
                        var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                        LoaderMap map = null;

                        if (avaibleBuffer != null && notExistChuckModule != null)
                        {
                            var cell = GetClient(chuckIndx);

                            if (cell != null && IsAliveClient(cell))
                            {
                                map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                {
                                    LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                }

                                if (map != null)
                                {
                                    LoaderInfo.StateMap = map;
                                    allwafer = map.GetTransferObjectAll();
                                    foreach (var wafer in allwafer)
                                    {
                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                        ModuleID dstPos = wafer.CurrPos;
                                        if (currSubObj.CurrPos != dstPos)
                                        {
                                            currSubObj.DstPos = dstPos;
                                            if (wafer.Priority > 999)
                                            {
                                                wafer.Priority = Priority++;
                                            }
                                        }
                                    }
                                    loaderMapList.Add(map);
                                    if (MapSlicerErrorFlag)
                                    {
                                        jobdone = true;
                                        break;
                                    }
                                }
                                else if (isNeedWafer)
                                {
                                    isExistWafer = false;
                                    map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, ActiveLotInfos[fullBufferLotIndex].DeviceName, chuckIndx);

                                    if (map == null)
                                    {
                                        continue;
                                    }
                                    allwafer = map.GetTransferObjectAll();
                                    foreach (var wafer in allwafer)
                                    {
                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                        ModuleID dstPos = wafer.CurrPos;
                                        if (currSubObj.CurrPos != dstPos)
                                        {
                                            currSubObj.DstPos = dstPos;
                                            if (wafer.Priority > 999)
                                            {
                                                bufferCount++;
                                                wafer.Priority = PreLoadingPriority++;
                                            }
                                        }
                                    }
                                    ReservationJob(map, avaibleBuffer);
                                    LoaderInfo.StateMap = map;
                                    loaderMapList.Add(map);
                                    jobdone = true;
                                    if (MapSlicerErrorFlag)
                                    {
                                        jobdone = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }

                        }
                        else
                        {
                            continue;
                        }
                    }

                }
                else if (isArmExistWafer)
                {
                    LoaderInfo.StateMap = isExistMap;
                }
                else if (isPAExistWafer)
                {
                    LoaderInfo.StateMap = isPAExistMap;
                }
                else
                {
                    isExistMap = IsOCRAbort(LoaderInfo.StateMap, out isAbortWafer);

                    if (isAbortWafer)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {
                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;
                        List<int> PreJobRingBufferCellIndexs = new List<int>();
                        //웨이퍼 없는 Cell부터 공급..
                        for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                        {
                            var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);

                            if (activeLotInfo.State == LotStateEnum.Running)
                            {
                                for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                {
                                    bool isExchange = false;
                                    bool isNeedWafer = false;
                                    bool isTempReady = false;
                                    string cstHashCodeOfRequestLot = "";

                                    var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                    int chuckIndx = activeLotInfo.UsingStageIdxList[i];
                                    var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();

                                    LoaderMap map = null;
                                    if (avaibleBuffer != null)
                                    {
                                        if (notExistChuckModule != null)
                                        {
                                            cell = GetClient(activeLotInfo.UsingStageIdxList[i]);

                                            if (cell != null && IsAliveClient(cell))
                                            {
                                                map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                {
                                                    LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                    map = null;
                                                }

                                                if (map != null)
                                                {
                                                    isExistWafer = false;
                                                    LoaderInfo.StateMap = map;
                                                    allwafer = map.GetTransferObjectAll();
                                                    foreach (var wafer in allwafer)
                                                    {
                                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                        ModuleID dstPos = wafer.CurrPos;
                                                        if (currSubObj.CurrPos != dstPos)
                                                        {
                                                            currSubObj.DstPos = dstPos;
                                                            if (wafer.Priority > 999)
                                                            {
                                                                bufferCount++;
                                                                wafer.Priority = Priority++;
                                                            }
                                                        }
                                                    }
                                                    ReservationJob(map, avaibleBuffer);
                                                    loaderMapList.Add(map);
                                                    jobdone = true;
                                                    if (MapSlicerErrorFlag)
                                                    {
                                                        jobdone = true;
                                                        break;
                                                    }
                                                }
                                                else if (isNeedWafer)
                                                {
                                                    isExistWafer = false;
                                                    map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                    if (map == null)
                                                    {
                                                        continue;
                                                    }
                                                    allwafer = map.GetTransferObjectAll();
                                                    foreach (var wafer in allwafer)
                                                    {
                                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                        ModuleID dstPos = wafer.CurrPos;
                                                        if (currSubObj.CurrPos != dstPos)
                                                        {
                                                            currSubObj.DstPos = dstPos;
                                                            if (wafer.Priority > 999)
                                                            {
                                                                bufferCount++;
                                                                wafer.Priority = PreLoadingPriority++;
                                                            }
                                                        }
                                                    }
                                                    ReservationJob(map, avaibleBuffer);
                                                    LoaderInfo.StateMap = map;
                                                    loaderMapList.Add(map);
                                                    jobdone = true;
                                                    if (MapSlicerErrorFlag)
                                                    {
                                                        jobdone = true;
                                                        break;
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                continue;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        jobdone = true;
                                        break;
                                    }
                                }
                            }

                            if (jobdone)
                            {
                                break;
                            }
                        }

                        if (!jobdone)
                        {
                            //Illia//
                            //Full Buffer 시나리오에서 Unload,Load 안되는 문제가 있어 WaferStatus 조건으로 Buffer Module ReservationState 처리 하는 로직 주석 하였음.

                            if (PreJobRingBuffer.Count > 0)
                            {
                                try
                                {
                                    for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                    {
                                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                        if (avaibleBuffer != null)
                                        {
                                            bool isExchange = false;
                                            bool isNeedWafer = false;
                                            bool isTempReady = false;
                                            string cstHashCodeOfRequestLot = "";

                                            cell = null;
                                            LoaderMap map = null;

                                            if (PreJobRingBuffer.Count > 0)
                                            {
                                                int chuckidx = PreJobRingBuffer.Peek();
                                                cell = GetClient(chuckidx);
                                                PreJobRingBufferCellIndexs.Add(chuckidx);
                                            }
                                            else
                                            {
                                                break;
                                            }

                                            if (cell != null)
                                            {
                                                map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                {
                                                    LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                    map = null;
                                                }

                                                if (map != null)
                                                {
                                                    LoaderInfo.StateMap = map;
                                                    allwafer = map.GetTransferObjectAll();
                                                    foreach (var wafer in allwafer)
                                                    {
                                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                        ModuleID dstPos = wafer.CurrPos;
                                                        if (currSubObj.CurrPos != dstPos)
                                                        {
                                                            currSubObj.DstPos = dstPos;
                                                            if (wafer.Priority > 999)
                                                            {
                                                                if (isExchange)
                                                                {
                                                                    exchangeCnt++;
                                                                    bufferCount++;
                                                                    wafer.Priority = exchangePriority++;
                                                                }
                                                                else
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    ReservationJob(map, avaibleBuffer);
                                                    loaderMapList.Add(map);
                                                    PreJobRingBuffer.Dequeue();
                                                    if (exchangeCnt >= 2 || MapSlicerErrorFlag)
                                                    {
                                                        jobdone = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    PreJobRingBuffer.Dequeue();
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    LoggerManager.Debug("PreJobRingBuffer exception Error");
                                    PreJobRingBuffer.Dequeue();
                                }
                            }

                            if (isExistWafer && !jobdone)
                            {
                                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = QueueActiveInfo.Peek();

                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                            if (avaibleBuffer != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";

                                                int chuckIndx = 0;

                                                chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);

                                                if (cell == null || !IsAliveClient(cell) || PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                                {
                                                    //연결이 끊긴 cell에 대해 RingBuffer내의 index 조절을 위해 dequeue/Enqueue 추가
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                }
                                                else
                                                {
                                                    LoaderMap map = null;

                                                    if (!ClientList.Contains(cell))
                                                    {
                                                        ClientList.Add(cell);

                                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                        if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                        {
                                                            LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                            map = null;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);

                                                        continue;
                                                    }

                                                    if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                currSubObj.DstPos = dstPos;
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }

                                                        LoaderInfo.StateMap = map;
                                                        ReservationJob(map, avaibleBuffer);
                                                        loaderMapList.Add(map);
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        PreJobRingBuffer.Enqueue(chuckIndx);
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (map != null)
                                                        {
                                                            ReservationJob(map, avaibleBuffer);
                                                            LoaderInfo.StateMap = map;
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    currSubObj.DstPos = dstPos;
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        if (isExchange)
                                                                        {
                                                                            exchangeCnt++;
                                                                            bufferCount++;
                                                                            wafer.Priority = exchangePriority++;
                                                                        }
                                                                        else
                                                                        {
                                                                            bufferCount++;
                                                                            wafer.Priority = Priority++;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);

                                                            if (exchangeCnt >= 2)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }

                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);


                                }
                            }
                        }

                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                // TODO :

                                bool LotEndFlag = false;

                                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                {
                                    if (isLotEnd[i] == true)
                                    {
                                        LotEndFlag = true;
                                    }
                                    else
                                    {
                                        LotEndFlag = false;
                                        break;
                                    }
                                }

                                if (LotEndFlag)
                                {
                                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                    {
                                        if (ActiveLotInfos[i].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }

                                //if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                //{
                                //    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                //    {
                                //        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                //    }
                                //    else
                                //    {
                                //        isLoaderEnd = true;
                                //    }
                                //}

                                return null;
                            }
                            else
                            {
                                HolderModuleInfo avaibleBuffer = null;
                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                int lowLotPriortyCellIndex = 0;
                                int lowLotPriortyCellsCount = 0;
                                int existDeviceCount = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();
                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();
                                for (int i = 0; i < ActiveLotInfos.Count; i++)
                                {

                                    var activeLotInfo = ActiveLotInfos[i];
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        if (activeLotInfo.UsingStageIdxList.Count > 0)
                                        {
                                            if (lowLotPriorty.LotPriority == activeLotInfo.LotPriority)
                                            {
                                                lowLotPriortyCellIndex = activeLotInfo.UsingStageIdxList[0];
                                                lowLotPriortyCellsCount = activeLotInfo.UsingStageIdxList.Count;
                                                DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                                CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                            }
                                            DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                            CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                        }
                                    }
                                }
                                int jobCount = 0;// 버퍼에 다 담는게 아니고 2개씩 끊어서 담아야한다.
                                do
                                {
                                    if (jobCount >= 2)
                                    {
                                        break;
                                    }
                                    avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                    if (avaibleBuffer != null)
                                    {
                                        if (DeviceNames.Count > 0)
                                        {
                                            devName = DeviceNames.Peek();
                                            DeviceNames.Dequeue();
                                        }
                                        if (CellIndexs.Count > 0)
                                        {
                                            cellIdx = CellIndexs.Peek();
                                            CellIndexs.Dequeue();
                                        }

                                        if (lowLotPriortyCellIndex == cellIdx)
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(lowLotPriortyCellIndex));
                                            if (existDeviceCount < 2 || (lowLotPriortyCellsCount == LoaderInfo.StateMap.ChuckModules.Count())) // 버퍼 2개까지 허용 또는 모든 Stage가 LOT에 할당되었을때는 Full Buffer
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }
                                        else
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(cellIdx));
                                            if (existDeviceCount < 1) // 버퍼 1개까지 허용
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }

                                        if (map != null)
                                        {
                                            ReservationJob(map, avaibleBuffer);
                                            LoaderInfo.StateMap = map;
                                            jobCount++;
                                            if (MapSlicerErrorFlag)
                                            {
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                    }
                                } while (avaibleBuffer != null && DeviceNames.Count > 0);
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        isLoaderEnd = true;
                                    }
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

            return LoaderInfo.StateMap;
        }

        public LoaderMap ExternalRequestJob(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            isLotEnd = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotPause = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotInitFlag = new bool[SystemModuleCount.ModuleCnt.FoupCount];

            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
                isLotPause[i] = false;
            }

            isLoaderEnd = false;
            Queue<string> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();

            try
            {
                int UnProcessedWaferCount = 0; //throughput 개선 관련 flag
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();
                ICardChangeSupervisor cardsuper = cont.Resolve<ICardChangeSupervisor>();
                LoaderInfo = Loader.GetLoaderInfo();
                CheckUsingStage();
                CheckActiveLotInfo();
                List<string> lotRuningList = new List<string>();
                List<LotStateEnum> lotStateEnums = new List<LotStateEnum>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    lotStateEnums.Add(ActiveLotInfos[lotIdx].State);
                }


                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running ||
                       lotStateEnums[lotIdx] == LotStateEnum.Abort ||
                       lotStateEnums[lotIdx] == LotStateEnum.Cancel ||
                       lotStateEnums[lotIdx] == LotStateEnum.Suspend ||
                       lotStateEnums[lotIdx] == LotStateEnum.Pause)
                    {
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[lotIdx].CST_HashCode;

                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running)
                    {
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = ((foupNum - 1) * 25) + selecedSlot[i];
                            var wafer = allwafer.FirstOrDefault(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.LOTRunning_CSTHash_List.Contains(w.CST_HashCode));
                            if (wafer == null)
                            {
                                //Lot End Scan 불일치
                                Task.Run(() =>
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Selected Wafer Not Matched Error", $"The selected wafer does not Exist. \nCassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", EnumMessageStyle.Affirmative).Result;

                                });
                                isLotPause[lotIdx] = true;


                                PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.ERROR, $"Selected Wafer Not Matched Error. Cassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", isLoaderMap: true);

                                return null;
                            }

                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                            {
                                UnProcessedWaferCount++;
                            }
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;

                            if (wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                }
                            }
                            else
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            int chuckIndx = ActiveLotInfos[lotIdx].UsingStageIdxList[i];

                            var ExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.EXIST && module.Substrate.WaferType.Value == EnumWaferType.POLISH).FirstOrDefault();
                            if (ExistChuckModule != null)
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++)
                        {
                            var client = GetClient(ActiveLotInfos[lotIdx].UsingStageIdxList[i]);

                            if (IsAliveClient(client))
                            {
                                bool IsLotEndReady = client.IsLotEndReady();

                                if (!IsLotEndReady)
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                        }

                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;
                        bool isFoupEnd = false;
                        SetSkipWaferOnChuck(ActiveLotInfos[lotIdx]);
                        var loaderJob = UnloadRequestJob(lotIdx + 1, out isFoupEnd);

                        if (isFoupEnd == true && loaderJob == null)
                        {
                            isLotEnd[lotIdx] = true;
                            return loaderJob;
                        }
                        else if (loaderJob == null && isFoupEnd == false)
                        {
                            isLotEnd[lotIdx] = false;
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }
                }

                // buffer의 reseve 상태를 초기화 한다. 이후 map 만들때 필요시(buffer에 있는걸 cell로 로드시, slot에서 buffer로 로드시) reseve 처리한다.
                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (this.LoaderInfo.StateMap.BufferModules[i].Enable)
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                }

                var PAModules = this.Loader.ModuleManager.FindModules<IPreAlignModule>();
                for (int i = 0; i < PAModules.Count; i++)
                {
                    var pa = this.LoaderInfo.StateMap.PreAlignModules.Where(p => p.ID.Index == PAModules[i].ID.Index).FirstOrDefault();
                    if (pa != null)
                    {
                        if (PAModules[i].PAStatus != ProberInterfaces.PreAligner.EnumPAStatus.Idle)
                        {
                            pa.ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                        }
                        else
                        {
                            pa.ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                        }
                    }
                }

                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;
                bool isPAExistWafer = true;
                bool isAbortWafer = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                LoaderMap isPAExistMap = null;

                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                isPAExistMap = IsExistLoaderPA_Normal(LoaderInfo.StateMap, out isPAExistWafer);

                bool isFullBufferLoadSeq = false;
                bool isFullArm = false;
                bool isFullBuffer = false;
                int fullBufferLotIndex = 0;
                List<int> loadStageOrderList = new List<int>();
                for (int k = 1; k <= SystemModuleCount.ModuleCnt.StageCount; k++)
                {
                    loadStageOrderList.Add(k);
                }

                var notExistChuckModuleCount = LoaderInfo.StateMap.ChuckModules.Where(module => module.WaferStatus == EnumSubsStatus.NOT_EXIST).Count();
                var notExistBufferModuleCount = LoaderInfo.StateMap.BufferModules.Where(module => module.WaferStatus == EnumSubsStatus.NOT_EXIST).Count();
                var notExistArmCount = LoaderInfo.StateMap.ARMModules.Where(module => module.WaferStatus == EnumSubsStatus.NOT_EXIST).Count();

                if (!isPAExistWafer)
                {
                    for (int i = 0; i < ActiveLotInfos.Count; i++)
                    {
                        if (ActiveLotInfos[i].State == LotStateEnum.Running && UnProcessedWaferCount >= ActiveLotInfos[i].UsingStageIdxList.Count())
                        {
                            if (ActiveLotInfos[i].UsingStageIdxList.Count() == LoaderInfo.StateMap.ChuckModules.Count()) //LOT에 합류된 Stage 숫자가 전체 Stage랑 같은 경우 
                            {
                                if (notExistChuckModuleCount != 0)
                                {
                                    if (notExistChuckModuleCount >= notExistBufferModuleCount) //LOT에 합류된 Stage에 Wafer가 모두 없는 경우
                                    {
                                        fullBufferLotIndex = i;
                                        isFirstLoadSeq = true;

                                        if (notExistBufferModuleCount >= LoaderInfo.StateMap.BufferModules.Count() - LoaderInfo.StateMap.ARMModules.Count())
                                        {
                                            isFullBufferLoadSeq = !isArmExistWafer;
                                        }
                                        else if (notExistArmCount < 2)
                                        {
                                            isFullArm = true;
                                        }
                                        else if (notExistBufferModuleCount <= LoaderInfo.StateMap.BufferModules.Count() - LoaderInfo.StateMap.ARMModules.Count())
                                        {
                                            isFullBuffer = !isArmExistWafer;
                                        }
                                    }
                                    else if (notExistArmCount < 2)
                                    {
                                        isFullArm = true;
                                    }
                                    else if (notExistBufferModuleCount <= LoaderInfo.StateMap.BufferModules.Count() - LoaderInfo.StateMap.ARMModules.Count())
                                    {
                                        isFullBuffer = !isArmExistWafer;
                                    }
                                }
                                else
                                {
                                    isFirstLoadSeq = false;
                                    isTryFirstLoadSeq = false;
                                }
                            }
                        }
                    }
                }

                void SetProirty(LoaderMap map, bool loadunload = false, bool preload = false, bool exchange = false, bool unbuffermapcheck = false)
                {
                    allwafer = map.GetTransferObjectAll();
                    foreach (var wafer in allwafer)
                    {
                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                        ModuleID dstPos = wafer.CurrPos;
                        if (currSubObj.CurrPos != dstPos)
                        {
                            currSubObj.DstPos = dstPos;
                            if (wafer.Priority > 999)
                            {
                                if (loadunload)
                                {
                                    bufferCount++;
                                    wafer.Priority = Priority++;
                                }
                                else if (preload)
                                {
                                    bufferCount++;
                                    wafer.Priority = PreLoadingPriority++;
                                }
                                else if (exchange)
                                {
                                    exchangeCnt++;
                                    bufferCount++;
                                    wafer.Priority = exchangePriority++;
                                }
                                else //firstload sequence
                                {
                                    wafer.Priority = Priority++;
                                }

                                if (unbuffermapcheck)
                                {
                                    if (currSubObj.CurrPos.ModuleType != ModuleTypeEnum.BUFFER)
                                    {
                                        isTryFirstLoadSeq = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (isPAExistWafer)
                {
                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] isPAExistWafer MC:1 EC:0 RB:0 ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                    LoaderInfo.StateMap = isPAExistMap;
                }
                else if (isTryFirstLoadSeq && isFirstLoadSeq && isFullBufferLoadSeq)
                {
                    for (int i = 0; i < ActiveLotInfos[fullBufferLotIndex].UsingStageIdxList.Count; i++)
                    {
                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";

                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                        int chuckIndx = ActiveLotInfos[fullBufferLotIndex].UsingStageIdxList[i];
                        var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();

                        LoaderMap map = null;
                        if (avaibleBuffer != null)
                        {
                            if (notExistChuckModule != null)
                            {
                                var cell = GetClient(ActiveLotInfos[fullBufferLotIndex].UsingStageIdxList[i]);

                                if (cell != null && IsAliveClient(cell))
                                {
                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                    {
                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                        map = null;
                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, Cell{chuckIndx} Lot IDLE State");
                                        isTryFirstLoadSeq = false;
                                        break;
                                    }

                                    if (map != null)
                                    {
                                        isExistWafer = false;
                                        SetProirty(map);
                                        LoaderInfo.StateMap = map;
                                        loaderMapList.Add(map);

                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, create unbuffered map");
                                        isTryFirstLoadSeq = false; //first load 상황에서 buffer가 아닌 cell load(pw trigger등) map이 생성되면 first load sequence를 중지한다.
                                        break;
                                    }
                                    else if (isNeedWafer)
                                    {
                                        map = PreLoadingJobMake_BufferCount_Minus_ArmCount(LoaderInfo.StateMap, ActiveLotInfos[fullBufferLotIndex].DeviceName, chuckIndx);

                                        if (map == null)
                                        {
                                            continue;
                                        }
                                        isExistWafer = false;
                                        ReservationJob(map, avaibleBuffer);
                                        SetProirty(map, preload: true);
                                        LoaderInfo.StateMap = map;
                                        loaderMapList.Add(map);

                                        if (MapSlicerErrorFlag)
                                        {
                                            LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, map slice error");
                                            isTryFirstLoadSeq = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] isFirstLoadSeq and isFullBufferLoadSeq MC:{loaderMapList.Count} EC:0 RB:{bufferCount} ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                }
                else if (isTryFirstLoadSeq && isFirstLoadSeq && isFullArm)
                {
                    for (int i = 0; i < loadStageOrderList.Count; i++)
                    {
                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";

                        int chuckIndx = loadStageOrderList[i];
                        var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                        var waferExistArmModule = LoaderInfo.StateMap.ARMModules.Where(module => module.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();

                        if (waferExistArmModule == null) //arm to cell map complete
                        {
                            break;
                        }

                        LoaderMap map = null;
                        if (notExistChuckModule != null)
                        {
                            var cell = GetClient(chuckIndx);

                            if (cell != null && IsAliveClient(cell))
                            {
                                map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                {
                                    LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                    map = null;
                                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, Cell{chuckIndx} Lot IDLE State");
                                    isTryFirstLoadSeq = false;
                                    break;
                                }
                                if (map != null)
                                {
                                    //arm에 wafer가 있는 상태이니 buffer를 사용하지 않는 map(PW Load)인 경우 first load sequence를 중지 한다.
                                    allwafer = map.GetTransferObjectAll();
                                    foreach (var wafer in allwafer)
                                    {
                                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                        ModuleID dstPos = wafer.CurrPos;
                                        if (currSubObj.CurrPos != dstPos && wafer.Priority > 999 && currSubObj.CurrPos.ModuleType != ModuleTypeEnum.BUFFER)
                                        {
                                            isTryFirstLoadSeq = false;
                                            break;
                                        }
                                    }
                                    if (!isTryFirstLoadSeq)
                                    {
                                        map = null;
                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, create unbuffered map");
                                        break;
                                    }

                                    map = LoadWafer_In_Arm(LoaderInfo.StateMap, ActiveLotInfos[fullBufferLotIndex].DeviceName, chuckIndx);
                                    SetProirty(map);
                                    LoaderInfo.StateMap = map;
                                    loaderMapList.Add(map);
                                    if (MapSlicerErrorFlag)
                                    {
                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, map slice error");
                                        isTryFirstLoadSeq = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] isFirstLoadSeq and isFullArm MC:{loaderMapList.Count} EC:0 RB:0 ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                }
                else if (isTryFirstLoadSeq && isFirstLoadSeq && isFullBuffer)
                {
                    for (int i = 0; i < loadStageOrderList.Count; i++)
                    {
                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";

                        int chuckIndx = loadStageOrderList[i];
                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);
                        var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                        LoaderMap map = null;

                        if (avaibleBuffer != null)
                        {
                            if (notExistChuckModule != null)
                            {
                                var cell = GetClient(chuckIndx);
                                if (cell != null && IsAliveClient(cell))
                                {
                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                    {
                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                        map = null;
                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, Cell{chuckIndx} Lot IDLE State");
                                        isTryFirstLoadSeq = false;
                                        break;
                                    }

                                    if (map != null)
                                    {
                                        // ReservationJob(map, avaibleBuffer); firstload fullbuffer 시나리오에서는 cell load시 buffer reserve를 하지 않는다. 한번에 tocell tobuffer map을 만들기 위함
                                        LoaderInfo.StateMap = map;
                                        SetProirty(map, unbuffermapcheck: true);
                                        loaderMapList.Add(map);

                                        if (!isTryFirstLoadSeq)
                                        {
                                            LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, create unbuffered map");
                                            break;
                                        }
                                        if (MapSlicerErrorFlag)
                                        {
                                            LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, map slice error");
                                            isTryFirstLoadSeq = false;
                                            break;
                                        }
                                    }
                                    else if (isNeedWafer)
                                    {
                                        map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, ActiveLotInfos[fullBufferLotIndex].DeviceName, chuckIndx);
                                        if (map == null)
                                        {
                                            continue;
                                        }
                                        isExistWafer = false;
                                        ReservationJob(map, avaibleBuffer);
                                        SetProirty(map, preload: true);
                                        LoaderInfo.StateMap = map;
                                        loaderMapList.Add(map);
                                        jobdone = true;
                                        if (MapSlicerErrorFlag)
                                        {
                                            LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, map slice error");
                                            isTryFirstLoadSeq = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] first Load Sequence force stop, map is null (ex. PW Waiting, cell{chuckIndx} error)");
                                        isTryFirstLoadSeq = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] isFirstLoadSeq and isFullBuffer MC:{loaderMapList.Count} EC:0 RB:{bufferCount} ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                }
                else if (isArmExistWafer)
                {
                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] isArmExistWafer MC:1 EC:0 RB:0 ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                    LoaderInfo.StateMap = isExistMap;
                }
                else
                {
                    isExistMap = IsOCRAbort(LoaderInfo.StateMap, out isAbortWafer);

                    if (isAbortWafer)
                    {
                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] basic sequence - isAbortWafer MC:1 EC:0 RB:0 ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {
                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;
                        List<int> PreJobRingBufferCellIndexs = new List<int>();
                        if (PreJobRingBuffer.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                {
                                    var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                    if (avaibleBuffer != null)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";
                                        cell = null;
                                        LoaderMap map = null;

                                        if (PreJobRingBuffer.Count > 0)
                                        {
                                            int chuckidx = PreJobRingBuffer.Peek();
                                            cell = GetClient(chuckidx);
                                            PreJobRingBufferCellIndexs.Add(chuckidx);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        if (cell != null)
                                        {
                                            map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                            if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                            {
                                                LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                map = null;
                                            }

                                            if (map != null)
                                            {
                                                SetProirty(map, exchange: isExchange, loadunload: !isExchange);
                                                LoaderInfo.StateMap = map;
                                                ReservationJob(map, avaibleBuffer);
                                                loaderMapList.Add(map);
                                                PreJobRingBuffer.Dequeue();
                                                if (exchangeCnt >= 2 || MapSlicerErrorFlag)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                PreJobRingBuffer.Dequeue();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                LoggerManager.Debug("PreJobRingBuffer exception Error");
                                PreJobRingBuffer.Dequeue();
                            }

                            if (LoaderInfo.StateMap != null)
                            {
                                if (loaderMapList.Count > 0)
                                {
                                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] basic sequence - PreJobRingbuffer MC:{loaderMapList.Count} EC:{exchangeCnt} RB:{bufferCount} ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                                }
                            }
                        }

                        if (!jobdone)
                        {
                            //웨이퍼 없는 Cell부터 공급..
                            for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                            {
                                var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);
                                if (activeLotInfo.State == LotStateEnum.Running)
                                {
                                    for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                        if (avaibleBuffer != null)
                                        {
                                            int chuckIndx = activeLotInfo.UsingStageIdxList[i];

                                            var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                                            LoaderMap map = null;

                                            if (PreJobRingBufferCellIndexs.Contains(chuckIndx))
                                            {
                                                continue;
                                            }
                                            else if (notExistChuckModule != null)
                                            {
                                                cell = GetClient(activeLotInfo.UsingStageIdxList[i]);
                                                if (cell != null && IsAliveClient(cell))
                                                {
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                        map = null;
                                                    }

                                                    if (map != null)
                                                    {
                                                        isExistWafer = false;
                                                        SetProirty(map, loadunload: true);
                                                        ReservationJob(map, avaibleBuffer);
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);
                                                        jobdone = true;

                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }
                                                        isExistWafer = false;
                                                        SetProirty(map, preload: true);
                                                        ReservationJob(map, avaibleBuffer);
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);
                                                        jobdone = true;
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                }

                                if (jobdone)
                                {
                                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] basic sequence - Load Sequence MC:{loaderMapList.Count} EC:0 RB:{bufferCount} ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                                    break;
                                }
                            }

                            if (isExistWafer)
                            {
                                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = QueueActiveInfo.Peek();
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                                            if (avaibleBuffer != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";

                                                int chuckIndx = 0;

                                                chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);
                                                if (cell == null || !IsAliveClient(cell) || PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                                {
                                                    //연결이 끊긴 cell에 대해 RingBuffer내의 index 조절을 위해 dequeue/Enqueue 추가
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                }
                                                else
                                                {
                                                    LoaderMap map = null;

                                                    if (!ClientList.Contains(cell))
                                                    {
                                                        ClientList.Add(cell);

                                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                        if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                        {
                                                            LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                            map = null;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        continue;
                                                    }

                                                    if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMakeFullBuffer(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }
                                                        SetProirty(map, preload: true);
                                                        LoaderInfo.StateMap = map;
                                                        ReservationJob(map, avaibleBuffer);
                                                        loaderMapList.Add(map);
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        PreJobRingBuffer.Enqueue(chuckIndx);

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }

                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (map != null)
                                                        {
                                                            SetProirty(map, exchange: isExchange, loadunload: !isExchange);
                                                            ReservationJob(map, avaibleBuffer);
                                                            LoaderInfo.StateMap = map;
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);

                                                            if (exchangeCnt >= 2 || bufferCount >= 4)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }

                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }

                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);
                                }
                                if (LoaderInfo.StateMap != null)
                                {
                                    if (loaderMapList.Count > 0)
                                    {
                                        LoggerManager.LoaderMapLog($"[ExternalRequestJob] basic sequence - load or Unload or Exchange MC:{loaderMapList.Count} EC:{exchangeCnt} RB:{bufferCount} ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                                    }
                                }
                            }
                        }

                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                // TODO :

                                bool LotEndFlag = false;

                                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                {
                                    if (isLotEnd[i] == true)
                                    {
                                        LotEndFlag = true;
                                    }
                                    else
                                    {
                                        LotEndFlag = false;
                                        break;
                                    }
                                }

                                if (LotEndFlag)
                                {
                                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                    {
                                        if (ActiveLotInfos[i].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }

                                return null;
                            }
                            else
                            {
                                HolderModuleInfo avaibleBuffer = null;
                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                int lowLotPriortyCellIndex = 0;
                                int lowLotPriortyCellsCount = 0;
                                int existDeviceCount = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();
                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();
                                for (int i = 0; i < ActiveLotInfos.Count; i++)
                                {

                                    var activeLotInfo = ActiveLotInfos[i];
                                    if (activeLotInfo.State == LotStateEnum.Running && (isLotEnd[i] == true && isLotInitFlag[i] == true)) //Lot End Trigger가 될 수 있는 조건에서는 Buffering 하지 못하도록 함.
                                    {
                                        if (activeLotInfo.UsingStageIdxList.Count > 0)
                                        {
                                            if (lowLotPriorty.LotPriority == activeLotInfo.LotPriority)
                                            {
                                                lowLotPriortyCellIndex = activeLotInfo.UsingStageIdxList[0];
                                                lowLotPriortyCellsCount = activeLotInfo.UsingStageIdxList.Count;
                                                DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                                CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                            }
                                            DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                            CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                        }
                                    }
                                }
                                int jobCount = 0;// 버퍼에 다 담는게 아니고 2개씩 끊어서 담아야한다.
                                do
                                {
                                    if (jobCount >= 2)
                                    {
                                        break;
                                    }

                                    avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                    if (avaibleBuffer != null)
                                    {
                                        if (DeviceNames.Count > 0)
                                        {
                                            devName = DeviceNames.Peek();
                                            DeviceNames.Dequeue();
                                        }
                                        if (CellIndexs.Count > 0)
                                        {
                                            cellIdx = CellIndexs.Peek();
                                            CellIndexs.Dequeue();
                                        }

                                        if (lowLotPriortyCellIndex == cellIdx)
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(lowLotPriortyCellIndex));
                                            if (existDeviceCount < 2 || (lowLotPriortyCellsCount == LoaderInfo.StateMap.ChuckModules.Count())) // 버퍼 2개까지 허용 또는 모든 Stage가 LOT에 할당되었을때는 Full Buffer
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }
                                        else
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(cellIdx));
                                            if (existDeviceCount < 1) // 버퍼 1개까지 허용
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }

                                        if (map != null)
                                        {
                                            ReservationJob(map, avaibleBuffer);
                                            LoaderInfo.StateMap = map;
                                            jobCount++;

                                            if (MapSlicerErrorFlag)
                                            {
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                    }
                                } while (avaibleBuffer != null && DeviceNames.Count > 0);

                                if (jobCount != 0)
                                {
                                    LoggerManager.LoaderMapLog($"[ExternalRequestJob] basic sequence - Buffering MC:{jobCount} EC:0 RB:{jobCount} ES:{notExistChuckModuleCount} EB:{notExistBufferModuleCount} EA:{notExistArmCount}");
                                }
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        isLoaderEnd = true;
                                    }
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

            return LoaderInfo.StateMap;
        }
        public LoaderMap ExternalRequestJobNoBuffer(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            isLotEnd = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotInitFlag = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotPause = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            List<ReservationInfo> isAvaibleArms = new List<ReservationInfo>();

            for (int i = 0; i < SystemModuleCount.ModuleCnt.ArmCount; i++)
            {
                isAvaibleArms.Add(new ReservationInfo());
            }

            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
                isLotPause[i] = false;
            }

            isLoaderEnd = false;
            Queue<String> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();

            try
            {
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();
                LoaderInfo = Loader.GetLoaderInfo();

                List<string> lotRuningList = new List<string>();
                List<LotStateEnum> lotStateEnums = new List<LotStateEnum>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    lotStateEnums.Add(ActiveLotInfos[lotIdx].State);
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running ||
                       lotStateEnums[lotIdx] == LotStateEnum.Abort ||
                       lotStateEnums[lotIdx] == LotStateEnum.Cancel ||
                       lotStateEnums[lotIdx] == LotStateEnum.Suspend ||
                       lotStateEnums[lotIdx] == LotStateEnum.Pause)
                    {
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[lotIdx].CST_HashCode;

                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running)
                    {
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.LOTRunning_CSTHash_List.Contains(w.CST_HashCode)).FirstOrDefault();
                            if (wafer == null)
                            {
                                //Lot End Scan 불일치
                                var retVal = (this).MetroDialogManager().ShowMessageDialog("Selected Wafer Not Matched Error", $"The selected wafer does not Exist. \nCassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", EnumMessageStyle.Affirmative).Result;

                                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    isLotEnd[lotIdx] = true;

                                    PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();

                                    LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.ERROR, $"Selected Wafer Not Matched Error. Cassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", isLoaderMap: true);
                                }

                                isLotEnd[lotIdx] = true;
                                return null;
                            }
                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;

                            if (wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                }
                            }
                            else
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            int chuckIndx = ActiveLotInfos[lotIdx].UsingStageIdxList[i];

                            var ExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.EXIST && module.Substrate.WaferType.Value == EnumWaferType.POLISH).FirstOrDefault();
                            if (ExistChuckModule != null)
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            var client = GetClient(ActiveLotInfos[lotIdx].UsingStageIdxList[i]);

                            if (client != null && IsAliveClient(client))
                            {
                                bool IsLotEndReady = client.IsLotEndReady();

                                if (!IsLotEndReady)
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[LoaderSupervisor], ExternalRequestJobNoBuffer() : Failed");
                            }
                        }
                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;
                        bool isFoupEnd = false;
                        SetSkipWaferOnChuck(ActiveLotInfos[lotIdx]);
                        var loaderJob = UnloadRequestJobNoBuffer(lotIdx + 1, out isFoupEnd);
                        if (isFoupEnd == true && loaderJob == null)
                        {
                            isLotEnd[lotIdx] = true;
                            return loaderJob;
                        }
                        else if (loaderJob == null && isFoupEnd == false)
                        {
                            isLotEnd[lotIdx] = false;
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }
                }

                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;
                bool isLoaderMapExist = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                if (isArmExistWafer)
                {
                    LoaderInfo.StateMap = isExistMap;
                }
                else
                {
                    isExistMap = IsLoaderMapByState(LoaderInfo.StateMap, out isLoaderMapExist);//ocr abort 된 wafer와 fixed 에 있는 wafer를 찾음.
                    if (isLoaderMapExist)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {
                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;
                        List<int> PreJobRingBufferCellIndexs = new List<int>();
                        for (int i = 0; i < isAvaibleArms.Count(); i++)
                        {
                            isAvaibleArms[i].ReservationState = EnumReservationState.NOT_RESERVE;
                        }

                        if (PreJobRingBuffer.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                {
                                    var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                    if (avaibleArm != null)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        cell = null;
                                        LoaderMap map = null;

                                        if (PreJobRingBuffer.Count > 0)
                                        {
                                            int chuckidx = PreJobRingBuffer.Peek();
                                            cell = GetClient(chuckidx);
                                            PreJobRingBufferCellIndexs.Add(chuckidx);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        if (cell != null)
                                        {
                                            map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                            if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                            {
                                                LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                map = null;
                                            }

                                            if (map != null)
                                            {
                                                LoaderInfo.StateMap = map;
                                                allwafer = map.GetTransferObjectAll();
                                                foreach (var wafer in allwafer)
                                                {
                                                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                    ModuleID dstPos = wafer.CurrPos;
                                                    if (currSubObj.CurrPos != dstPos)
                                                    {
                                                        currSubObj.DstPos = dstPos;
                                                        if (wafer.Priority > 999)
                                                        {
                                                            if (isExchange)
                                                            {
                                                                exchangeCnt++;
                                                                bufferCount++;
                                                                wafer.Priority = exchangePriority++;
                                                            }
                                                            else
                                                            {
                                                                bufferCount++;
                                                                wafer.Priority = Priority++;
                                                            }
                                                        }
                                                    }
                                                }
                                                avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                //  LoaderInfo.StateMap.ARMModules[avaibleArm.ID.Index-1].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                                                loaderMapList.Add(map);
                                                PreJobRingBuffer.Dequeue();
                                                if (exchangeCnt >= 1)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                                if (MapSlicerErrorFlag)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                PreJobRingBuffer.Dequeue();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                LoggerManager.Debug("PreJobRingBuffer exception Error");
                                PreJobRingBuffer.Dequeue();
                            }
                        }

                        if (!jobdone)
                        {
                            for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                            {
                                var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);
                                if (activeLotInfo.State == LotStateEnum.Running)
                                {
                                    for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                        if (avaibleArm != null)
                                        {
                                            int chuckIndx = activeLotInfo.UsingStageIdxList[i];
                                            var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                                            LoaderMap map = null;
                                            if (PreJobRingBufferCellIndexs.Contains(chuckIndx))
                                            {
                                                continue;
                                            }
                                            else if (notExistChuckModule != null)
                                            {
                                                cell = GetClient(activeLotInfo.UsingStageIdxList[i]);
                                                if (cell != null && IsAliveClient(cell))
                                                {
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                        map = null;
                                                    }

                                                    if (map != null)
                                                    {
                                                        isExistWafer = false;

                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                        loaderMapList.Add(map);
                                                        jobdone = true;
                                                        break;
                                                    }
                                                    if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, isNeedWafer, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }
                                                        isExistWafer = false;
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;

                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                currSubObj.DstPos = dstPos;
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);
                                                        jobdone = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                }

                                if (jobdone)
                                {
                                    break;
                                }
                            }

                            if (isExistWafer)
                            {
                                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = QueueActiveInfo.Peek();
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        //if (activeLotInfo.RemainCount > activeLotInfo.UsingStageIdxList.Count)
                                        //{
                                        //    activeLotInfo.RemainCount = activeLotInfo.UsingStageIdxList.Count;
                                        //}

                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                            if (avaibleArm != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";

                                                int chuckIndx = 0;

                                                chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);
                                                if (cell == null || !IsAliveClient(cell) || PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                                {
                                                    //연결이 끊긴 cell에 대해 RingBuffer내의 index 조절을 위해 dequeue/Enqueue 추가
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                }
                                                else
                                                {
                                                    LoaderMap map = null;
                                                    if (!ClientList.Contains(cell))
                                                    {

                                                        ClientList.Add(cell);
                                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                        if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                        {
                                                            LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                            map = null;
                                                        }

                                                        if (isNeedWafer)
                                                        {
                                                            map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, isNeedWafer, activeLotInfo.DeviceName, chuckIndx);
                                                            //activeLotInfo.RingBuffer.Dequeue();
                                                            //activeLotInfo.RingBuffer.Enqueue(cell);
                                                            if (map == null)
                                                            {
                                                                continue;
                                                            }

                                                            if (LoaderInfo.StateMap.PreAlignModules.Count(pa => pa.Substrate == null) == 0)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        wafer.Priority = PreLoadingPriority++;
                                                                    }
                                                                }
                                                            }
                                                            LoaderInfo.StateMap = map;
                                                            avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                            PreJobRingBuffer.Enqueue(chuckIndx);
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            break;
                                                        }
                                                        else if (map != null)
                                                        {
                                                            avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                            LoaderInfo.StateMap = map;
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        wafer.Priority = Priority++;
                                                                    }
                                                                }
                                                            }
                                                            loaderMapList.Add(map);
                                                            if (isExchange)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        continue;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }

                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);
                                }
                            }
                        }

                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                if (isLotEnd.Length == 1)
                                {
                                    if (isLotEnd[0])
                                    {
                                        if (ActiveLotInfos[0].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }
                                else if (isLotEnd.Length == 2)
                                {
                                    if (isLotEnd[0] && isLotEnd[1])
                                    {
                                        if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }
                                else if (isLotEnd.Length == 3)
                                {
                                    if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                    {
                                        if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }

                                return null;
                            }
                            else
                            {
                                //HolderModuleInfo avaibleBuffer = null;

                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                int lowLotPriortyCellIndex = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();
                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();
                                for (int i = 0; i < ActiveLotInfos.Count; i++)
                                {

                                    var activeLotInfo = ActiveLotInfos[i];
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        if (lowLotPriorty.LotPriority == activeLotInfo.LotPriority)
                                        {
                                            lowLotPriortyCellIndex = activeLotInfo.UsingStageIdxList[0];
                                            DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                            CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                        }
                                        DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                        CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                    }
                                }
                                if (LoaderInfo.StateMap.PreAlignModules.Count(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST) >= 1)
                                {
                                    devName = DeviceNames.Peek();
                                    cellIdx = CellIndexs.Peek();
                                    DeviceNames.Dequeue();
                                    CellIndexs.Dequeue();
                                    map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, false, devName, cellIdx);
                                    LoaderInfo.StateMap = map;
                                }
                                //int jobCount = 0;// 버퍼에 다 담는게 아니고 2개씩 끊어서 담아야한다.
                                //do
                                //{
                                //    if(LoaderInfo.StateMap.PreAlignModules.Count(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST)>=2)
                                //    { 
                                //    var avaiblePA = LoaderInfo.StateMap.PreAlignModules.Count(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                //    if (avaiblePA != null)
                                //    {
                                //        devName = DeviceNames.Peek();
                                //        cellIdx = CellIndexs.Peek();
                                //        DeviceNames.Dequeue();
                                //        CellIndexs.Dequeue();

                                //        if (lowLotPriortyCellIndex == cellIdx)
                                //        {
                                //            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(lowLotPriortyCellIndex));
                                //            if (existDeviceCount < 2) // 버퍼 2개까지 허용
                                //            {
                                //                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                //            }
                                //            else
                                //            {
                                //                map = null;
                                //            }
                                //        }
                                //        else
                                //        {
                                //            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(cellIdx));
                                //            if (existDeviceCount < 1) // 버퍼 1개까지 허용
                                //            {
                                //                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                //            }
                                //            else
                                //            {
                                //                map = null;
                                //            }
                                //        }

                                //        if (map != null)
                                //        {
                                //            ReservationJob(map, avaibleBuffer);
                                //            LoaderInfo.StateMap = map;
                                //            jobCount++;
                                //        }

                                //    }
                                //} while (avaibleBuffer != null && DeviceNames.Count > 0);
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                if (isLotEnd[0] && isLotEnd[1])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        isLoaderEnd = true;
                                    }
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

            return LoaderInfo.StateMap;
        }

        public LoaderMap ExternalRequestJob_Hynix(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            //v22_merge// 전체 검토 필요
            isLotEnd = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotInitFlag = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            isLotPause = new bool[SystemModuleCount.ModuleCnt.FoupCount];
            List<ReservationInfo> isAvaibleArms = new List<ReservationInfo>();

            for (int i = 0; i < SystemModuleCount.ModuleCnt.ArmCount; i++)
            {
                isAvaibleArms.Add(new ReservationInfo());
            }

            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
                isLotPause[i] = false;
            }

            isLoaderEnd = false;
            Queue<String> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();

            try
            {
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();
                ICardChangeSupervisor cardsuper = cont.Resolve<ICardChangeSupervisor>();
                LoaderInfo = Loader.GetLoaderInfo();
                CheckUsingStage();
                CheckActiveLotInfo();
                List<string> lotRuningList = new List<string>();
                List<LotStateEnum> lotStateEnums = new List<LotStateEnum>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    lotStateEnums.Add(ActiveLotInfos[lotIdx].State);
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running ||
                       lotStateEnums[lotIdx] == LotStateEnum.Abort ||
                       lotStateEnums[lotIdx] == LotStateEnum.Cancel ||
                       lotStateEnums[lotIdx] == LotStateEnum.Suspend ||
                       lotStateEnums[lotIdx] == LotStateEnum.Pause)
                    {
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[lotIdx].CST_HashCode;

                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;

                    if (lotStateEnums[lotIdx] == LotStateEnum.Running)
                    {
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[foupNum - 1].CST_HashCode;

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.LOTRunning_CSTHash_List.Contains(w.CST_HashCode)).FirstOrDefault();
                            if (wafer == null)
                            {
                                //Lot End Scan 불일치
                                Task.Run(() =>
                                {
                                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Selected Wafer Not Matched Error", $"The selected wafer does not Exist. \nCassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", EnumMessageStyle.Affirmative).Result;
                                });
                                isLotPause[lotIdx] = true;

                                PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(CarrierCanceledEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();

                                LoggerManager.ActionLog(ModuleLogType.LOT, StateLogType.ERROR, $"Selected Wafer Not Matched Error. Cassette Number : {foupNum} , SLOT Number : {selecedSlot[i]}", isLoaderMap: true);

                                return null;
                            }
                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                                realWafer.CST_HashCode = ActiveLotInfos[lotIdx].CST_HashCode;
                                realWafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            }

                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;

                            if (wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                }
                            }
                            else
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            int chuckIndx = ActiveLotInfos[lotIdx].UsingStageIdxList[i];

                            var ExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.EXIST && module.Substrate.WaferType.Value == EnumWaferType.POLISH).FirstOrDefault();
                            if (ExistChuckModule != null)
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }

                        ////#PAbort
                        //RemoveUsingStage(ActiveLotInfos[lotIdx]);
                        //if (ActiveLotInfos[lotIdx].UsingStageIdxList.Count() == 0
                        //    && ActiveLotInfos[lotIdx].State == LotStateEnum.Running)
                        //{
                        //    ActiveLotInfos[lotIdx].IsReserveCancel = true;
                        //    SetSkipUnprocessedWafer(Loader.GetLoaderInfo().StateMap, ActiveLotInfos[lotIdx]);
                        //}
                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            var client = GetClient(ActiveLotInfos[lotIdx].UsingStageIdxList[i]);
                           
                            if (client != null && IsAliveClient(client))
                            {
                                if(client.IsNeedLotEnd() == false)
                                {
                                    client.UpdateIsNeedLotEnd(LoaderInfo);
                                }

                                bool IsLotEndReady = client.IsLotEndReady();

                                if (!IsLotEndReady)
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[LoaderSupervisor], ExternalRequestJobNoBuffer() : Failed");
                            }
                        }
                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;
                        bool isFoupEnd = false;
                        SetSkipWaferOnChuck(ActiveLotInfos[lotIdx]);
                        SetSkipUnprocessedWafer(LoaderInfo.StateMap, ActiveLotInfos[lotIdx]);
                        var loaderJob = UnloadRequestJobNoBuffer(lotIdx + 1, out isFoupEnd);//OriginHolder가 동일함.
                        if (isFoupEnd == true && loaderJob == null)
                        {
                            isLotEnd[lotIdx] = true;// 회수 완료된 시점.
                            return loaderJob;
                        }
                        else if (loaderJob == null && isFoupEnd == false)
                        {
                            isLotEnd[lotIdx] = false;
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }
                }

                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;

                bool isLoaderMapExist = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                if (isArmExistWafer)
                {
                    LoaderInfo.StateMap = isExistMap;
                }
                else
                {
                    isExistMap = IsLoaderMapByState(LoaderInfo.StateMap, out isLoaderMapExist);//ocr abort 된 wafer와 fixed 에 있는 wafer를 찾음.
                    if (isLoaderMapExist)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {
                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;
                        List<int> PreJobRingBufferCellIndexs = new List<int>();
                        for (int i = 0; i < isAvaibleArms.Count(); i++)
                        {
                            isAvaibleArms[i].ReservationState = EnumReservationState.NOT_RESERVE;
                        }

                        if (PreJobRingBuffer.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                {
                                    var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                    if (avaibleArm != null)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        cell = null;
                                        LoaderMap map = null;

                                        if (PreJobRingBuffer.Count > 0)
                                        {
                                            int chuckidx = PreJobRingBuffer.Peek();
                                            cell = GetClient(chuckidx);
                                            PreJobRingBufferCellIndexs.Add(chuckidx);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        if (cell != null && IsAliveClient(cell) && cardsuper.IsAllocatedStage(cell.GetChuckID().Index) == false)
                                        {
                                            map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                            if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                            {
                                                LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                map = null;
                                            }

                                            if (map != null)
                                            {
                                                LoaderInfo.StateMap = map;
                                                allwafer = map.GetTransferObjectAll();
                                                foreach (var wafer in allwafer)
                                                {
                                                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                    ModuleID dstPos = wafer.CurrPos;
                                                    if (currSubObj.CurrPos != dstPos)
                                                    {
                                                        currSubObj.DstPos = dstPos;
                                                        if (wafer.Priority > 999)
                                                        {
                                                            if (isExchange)
                                                            {
                                                                exchangeCnt++;
                                                                bufferCount++;
                                                                wafer.Priority = exchangePriority++;
                                                            }
                                                            else
                                                            {
                                                                bufferCount++;
                                                                wafer.Priority = Priority++;
                                                            }
                                                        }
                                                    }
                                                }
                                                avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                //  LoaderInfo.StateMap.ARMModules[avaibleArm.ID.Index-1].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                                                loaderMapList.Add(map);
                                                PreJobRingBuffer.Dequeue();
                                                if (exchangeCnt >= 1)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                                if (MapSlicerErrorFlag)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                PreJobRingBuffer.Dequeue();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                LoggerManager.Debug("PreJobRingBuffer exception Error");
                                PreJobRingBuffer.Dequeue();
                            }
                        }

                        if (!jobdone)
                        {
                            for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                            {
                                var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);
                                if (activeLotInfo.State == LotStateEnum.Running)
                                {
                                    //RemoveUsingStage(ActiveLotInfos[lotIdx]);
                                    //if (ActiveLotInfos[lotIdx].UsingStageIdxList.Count() == 0
                                    //    && ActiveLotInfos[lotIdx].State != LotStateEnum.Idle)
                                    //{
                                    //    ActiveLotInfos[lotIdx].IsReserveCancel = true;
                                    //    SetSkipUnprocessedWafer(Loader.GetLoaderInfo().StateMap, ActiveLotInfos[lotIdx]);
                                    //}

                                    for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                        if (avaibleArm != null)
                                        {
                                            int chuckIndx = activeLotInfo.UsingStageIdxList[i];
                                            var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                                            LoaderMap map = null;
                                            if (PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                            {
                                                continue;
                                            }
                                            else if (notExistChuckModule != null)
                                            {
                                                cell = GetClient(activeLotInfo.UsingStageIdxList[i]);
                                                if (cell != null && IsAliveClient(cell) && cardsuper.IsAllocatedStage(cell.GetChuckID().Index) == false)
                                                {
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                        map = null;
                                                    }

                                                    if (map != null)
                                                    {
                                                        isExistWafer = false;

                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                        loaderMapList.Add(map);
                                                        jobdone = true;
                                                        break;
                                                    }
                                                    var ocrWaferCnt = LoaderInfo.StateMap.CognexOCRModules.Count(ocr => ocr.Substrate != null);
                                                    var paWaferCnt = LoaderInfo.StateMap.PreAlignModules.Count(pa => pa.Substrate != null);
                                                   if (SystemModuleCount.ModuleCnt.PACount == 2 && SystemModuleCount.ModuleCnt.PACount <= ocrWaferCnt + paWaferCnt)
                                                    {
                                                        activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                        jobdone = true;
                                                        break;
                                                    }


                                                    if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, isNeedWafer, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }
                                                        isExistWafer = false;
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;

                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                currSubObj.DstPos = dstPos;
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);

                                                        // 사용 가능 한 buffer가 없을 때 fixed tray load 되고 pa 를 모두 채우는 경우 map이 생성되지 않도록 함.
                                                        var usingFixedTray = LoaderInfo.StateMap.FixedTrayModules.FirstOrDefault(x => x.Substrate == null && x.Enable && x.CanUseBuffer);
                                                        ocrWaferCnt = LoaderInfo.StateMap.CognexOCRModules.Count(ocr => ocr.Substrate != null);
                                                        paWaferCnt = LoaderInfo.StateMap.PreAlignModules.Count(pa => pa.Substrate != null);
                                                        if (usingFixedTray == null && SystemModuleCount.ModuleCnt.PACount - 1 <= ocrWaferCnt + paWaferCnt)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }

                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                }

                                if (jobdone)
                                {
                                    break;
                                }
                            }

                            if (isExistWafer)
                            {
                                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = QueueActiveInfo.Peek();
                                    if (activeLotInfo.State == LotStateEnum.Running) 
                                    {
                                        //Illia , 아래 조건문이 있다면 abort된 stage 가 남은 stage보다 index가 작은 경우 wafer unload 하지 못하는 경우가 있음.
                                        //Abort 되면 UsingStageIndexList에서는 제외 되어 running인 stage가 아닌 idle인 stage에만 request 할 수 있음.
                                        //if (activeLotInfo.RemainCount > activeLotInfo.UsingStageIdxList.Count)
                                        //{
                                        //    activeLotInfo.RemainCount = activeLotInfo.UsingStageIdxList.Count;
                                        //}

                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                            if (avaibleArm != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";

                                                int chuckIndx = 0;

                                                chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);
                                                if (cell == null || !IsAliveClient(cell) || PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                                {
                                                    //연결이 끊긴 cell에 대해 RingBuffer내의 index 조절을 위해 dequeue/Enqueue 추가
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                }
                                                else
                                                {
                                                    LoaderMap map = null;
                                                    if (!ClientList.Contains(cell) && cardsuper.IsAllocatedStage(cell.GetChuckID().Index) == false)
                                                    {

                                                        ClientList.Add(cell);
                                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                        if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                        {
                                                            LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                            map = null;
                                                        }

                                                        var ocrWaferCnt = LoaderInfo.StateMap.CognexOCRModules.Count(ocr => ocr.Substrate != null);
                                                        var paWaferCnt = LoaderInfo.StateMap.PreAlignModules.Count(pa => pa.Substrate != null);
                                                        if (SystemModuleCount.ModuleCnt.PACount == 2 && SystemModuleCount.ModuleCnt.PACount <= ocrWaferCnt + paWaferCnt)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }

                                                        if (isNeedWafer)
                                                        {
                                                            map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, isNeedWafer, activeLotInfo.DeviceName, chuckIndx);
                                                            //activeLotInfo.RingBuffer.Dequeue();
                                                            //activeLotInfo.RingBuffer.Enqueue(cell);
                                                            if (map == null)
                                                            {
                                                                continue;
                                                            }

                                                            //STM은 PA가 0개남으면 SKIP해야함. 
                                                            //그외 장비는 1개 남으면 SKIP해야함. 
                                                            var emptyocrWaferCnt = LoaderInfo.StateMap.CognexOCRModules.Count(ocr => ocr.Substrate == null);
                                                            var emptypaWaferCnt = LoaderInfo.StateMap.PreAlignModules.Count(pa => pa.Substrate == null);
                                                            if ((SystemModuleCount.ModuleCnt.PACount == 1 && emptyocrWaferCnt + emptypaWaferCnt == 0) ||
                                                                (SystemModuleCount.ModuleCnt.PACount > 1 && emptyocrWaferCnt + emptypaWaferCnt == 1))
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }

                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        wafer.Priority = PreLoadingPriority++;
                                                                    }
                                                                }
                                                            }
                                                            LoaderInfo.StateMap = map;
                                                            avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                            PreJobRingBuffer.Enqueue(chuckIndx);
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            break;
                                                        }
                                                        else if (map != null)
                                                        {
                                                            avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                            LoaderInfo.StateMap = map;
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        wafer.Priority = Priority++;
                                                                    }
                                                                }
                                                            }
                                                            loaderMapList.Add(map);
                                                            if (isExchange)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        continue;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }

                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);
                                }
                            }
                        }

                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                bool LotEndFlag = false;

                                for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                {
                                    if (isLotEnd[i] == true)
                                    {
                                        LotEndFlag = true;
                                    }
                                    else
                                    {
                                        LotEndFlag = false;
                                        break;
                                    }
                                }

                                if (LotEndFlag)
                                {
                                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                                    {
                                        if (ActiveLotInfos[i].State == LotStateEnum.End)
                                        {
                                            isLoaderEnd = false;
                                            break;
                                        }
                                        else
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }

                                return null;
                            }
                            else
                            {
                                HolderModuleInfo avaibleFixedTray = null;
                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();
                                int existDeviceCount = 0;
                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();
                                var sortedActiveLotInfos = ActiveLotInfos.OrderBy(x => x.LotPriority).ToList();
                                foreach(var lotinfo in sortedActiveLotInfos)
                                {
                                    if (lotinfo.State == LotStateEnum.Running)
                                    { 
                                        //1st lot에 할당된 cell만큼 buffering이 되도록 한다.
                                        if (lowLotPriorty.LotPriority == lotinfo.LotPriority)
                                        {
                                            if (lotinfo.UsingStageIdxList.Count > 0)
                                            {
                                                foreach (var item in lotinfo.UsingStageIdxList)
                                                {
                                                    DeviceNames.Enqueue(lotinfo.DeviceName);
                                                    CellIndexs.Enqueue(item);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // 이전 lotinfo에서 할당된 usingstageidxlist 값을 제외하고 cellindexs에 add하자.
                                            if (lotinfo.UsingStageIdxList.Count > 0)
                                            {
                                                foreach (var item in lotinfo.UsingStageIdxList)
                                                {
                                                    if (!CellIndexs.Contains(item))
                                                    {
                                                        DeviceNames.Enqueue(lotinfo.DeviceName);
                                                        CellIndexs.Enqueue(item);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                bool IsExistentPreLoadingJob = false;
                                int jobCount = 0;// 버퍼(FixedTray)에 다 담는게 아니고 2개씩 끊어서 담아야한다.
                                do
                                {
                                    //fixed tray canusebuffer true인 경우에만 buffering 동작 함. MD - NoBuffer 동작과 상관 없을 것으로 보임.
                                    if (jobCount >= 2)
                                    {
                                        break;
                                    }
                                    avaibleFixedTray = LoaderInfo.StateMap.FixedTrayModules.FirstOrDefault(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST && i.CanUseBuffer);
                                    if (avaibleFixedTray != null)
                                    {
                                        devName = DeviceNames.Peek();
                                        cellIdx = CellIndexs.Peek();
                                        DeviceNames.Dequeue();
                                        CellIndexs.Dequeue();

                                        //var find = ActiveLotInfos.Where(x => x.UsingStageIdxList.Contains(cellIdx)).OrderByDescending(x => x.LotPriority).FirstOrDefault();
                                        //int count = find?.UsingStageIdxList.Count ?? 0;
                                        existDeviceCount = LoaderInfo.StateMap.FixedTrayModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.CanUseBuffer && i.Substrate.UsingStageList.Contains(cellIdx));
                                        if (lowLotPriorty.UsingStageIdxList.Contains(cellIdx))
                                        {
                                            int lowlot_bufferinglimit = lowLotPriorty.UsingStageIdxList.Count;
                                            if (existDeviceCount < lowlot_bufferinglimit)
                                            {
                                                map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, true, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }

                                        }
                                        else
                                        {
                                            //cellIdx가 포함된 ActiveLotInfos를 찾기. 1st lot에 포함된 cell index를 제외하고. (다른 lot에 할당된 cell의 개수 만큼 가능하도록 하자.)
                                            var activeLotsWithCellIdx = ActiveLotInfos.Where(lot => lot.UsingStageIdxList.Contains(cellIdx)).FirstOrDefault();
                                            int lot_bufferinglimit = activeLotsWithCellIdx.UsingStageIdxList.Where(x => !lowLotPriorty.UsingStageIdxList.Contains(x)).ToList().Count; 
                                            if (existDeviceCount < lot_bufferinglimit)
                                            {
                                                map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, true, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }

                                        if (map != null)
                                        {
                                            IsExistentPreLoadingJob = true;
                                            ReservationJob(map, avaibleFixedTray);
                                            LoaderInfo.StateMap = map;
                                            jobCount++;
                                        }

                                    }
                                } while (avaibleFixedTray != null && DeviceNames.Count > 0);

                                if (false == IsExistentPreLoadingJob)
                                {
                                    LoaderInfo.StateMap = null;
                                }
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                if (isLotEnd[0] && isLotEnd[1])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        isLoaderEnd = true;
                                    }
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

            return LoaderInfo.StateMap;
        }

        //[FOUP_SHIFT]*
        public LoaderMap ExternalRequestJob_FoupShift_NoBuffer(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            //v22_merge// 전체 검토 필요
            isLotEnd = new bool[ActiveLotInfos.Count];
            isLotInitFlag = new bool[ActiveLotInfos.Count];
            isLotPause = new bool[SystemModuleCount.ModuleCnt.FoupCount];

            List<ReservationInfo> isAvaibleArms = new List<ReservationInfo>();
            for (int i = 0; i < SystemModuleCount.ModuleCnt.ArmCount; i++)
            {
                isAvaibleArms.Add(new ReservationInfo());
            }
            for (int i = 0; i < ActiveLotInfos.Count; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
            }
            isLoaderEnd = false;
            Queue<String> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();
            try
            {
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                //[FOUP_SHIFT]
                LoaderInfo = Loader.GetLoaderInfo();
                CheckUsingStage();
                CheckActiveLotInfo();

                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;
                    if ((ActiveLotInfos[lotIdx].State == LotStateEnum.Running) || (Prev_ActiveLotInfos.Count() > 0))
                    {
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[foupNum - 1].CST_HashCode;
                        string cst_HashCode = ActiveLotInfos[lotIdx].CST_HashCode;


                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.CST_HashCode == cst_HashCode).FirstOrDefault();
                            if (wafer == null)
                            {
                                isLotEnd[lotIdx] = false;
                                //Lot End Scan 불일치
                                continue;
                            }
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;

                            // [FOUP_SHIFT]*

                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            //wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStagesBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 스테이지를 지정한다.
                            //wafer.DeviceName.Value = ActiveLotInfos[lotIdx].UsingDeviceNameBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 Device 지정한다.



                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            //웨이퍼가 아직 할당인 안된경우는 Lot를 끝내선 안된다.
                            if (wafer.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                                wafer.WaferState == EnumWaferState.UNPROCESSED &&
                                wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                //#PAbort
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                    //else
                                    //{
                                    //    //#PAbort
                                    //    ActiveLotInfos[lotIdx].State = LotStateEnum.Cancel;
                                    //}
                                }
                            }
                            else
                            {
                                if (LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.SHIFT)
                                {
                                    if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                    {
                                        if (!isAllErrorEnd)
                                        {
                                            isLotEnd[lotIdx] = false;
                                        }
                                        //else
                                        //{
                                        //    //#PAbort
                                        //    ActiveLotInfos[lotIdx].State = LotStateEnum.Cancel;
                                        //}
                                    }
                                    else
                                    {
                                        var pairwafer = allwafer.Where(w => w.CST_HashCode == cst_HashCode && w.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                                        var endPairwafer = pairwafer.Where(w => w.WaferState != EnumWaferState.UNPROCESSED && w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);
                                        if (pairwafer.Count() != endPairwafer.Count())
                                        {
                                            isLotEnd[lotIdx] = false;
                                        }
                                        else
                                        {

                                        }
                                    }


                                }

                            }
                        }

                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            int chuckIndx = ActiveLotInfos[lotIdx].UsingStageIdxList[i];

                            var ExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.EXIST && module.Substrate.WaferType.Value == EnumWaferType.POLISH).FirstOrDefault();
                            if (ExistChuckModule != null)
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }


                        for (int i = 0; i < ActiveLotInfos[lotIdx].UsingStageIdxList.Count; i++) //웨이퍼가 있는지 체크( ex . polishWafer check)
                        {
                            var client = GetClient(ActiveLotInfos[lotIdx].UsingStageIdxList[i]);

                            if (IsAliveClient(client))
                            {
                                bool IsLotEndReady = client.IsLotEndReady();

                                if (!IsLotEndReady)
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[LoaderSupervisor], ExternalRequestJobNoBuffer() : Failed");
                                //ActiveLotInfos[lotIdx].UsingStageList.Remove(cleint);

                            }

                            //if (!cleint.IsLotEndReady())
                            //{
                            //    isLotEnd[lotIdx] = false;
                            //}
                        }

                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;

                        //#region LotCancel 일때 ProcessingEnable 필요해서 추가함.

                        //if(FoupShiftMode.Value == FoupShiftModeEnum.SHIFT)
                        //{
                        //    List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        //    var slotList = selecedSlot.ToList();

                        //    for (int i = 0; i < selecedSlot.Count; i++)
                        //    {
                        //        slotList[i] = (lotIdx) * 25 + selecedSlot[i];
                        //        var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.CST_HashCode == ActiveLotInfos[lotIdx].CST_HashCode).FirstOrDefault();
                        //        if (wafer == null)
                        //        {                                    
                        //            //Lot End Scan 불일치
                        //            continue;
                        //        }
                        //        wafer.LotPriority = ActiveLotInfos[lotIdx].LotPriority;
                        //        wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                        //        //wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                        //        //wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                        //        var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                        //        if (realWafer != null)
                        //        {
                        //            realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                        //        }
                        //    }
                        //}

                        //#endregion



                        //bool isFoupEnd = false;
                        //var loaderJob = UnloadRequestJobFoupShiftNoBuffer(lotIdx + 1, out isFoupEnd);
                        //if (isFoupEnd == true && loaderJob == null)
                        //{
                        //    isLotEnd[lotIdx] = true;
                        //    return loaderJob;
                        //}
                        //else if (loaderJob == null && isFoupEnd == false)
                        //{
                        //    isLotEnd[lotIdx] = false;
                        //}
                        //else if (loaderJob != null)
                        //{
                        //    isLotEnd[lotIdx] = false;
                        //    return loaderJob;
                        //}
                    }
                }

                if (LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.SHIFT)//Cancel시에
                {
                    //모든 lot에 대해서 고려해야함.
                    List<ActiveLotInfo> TotalCanceledLotInfos = null;
                    TotalCanceledLotInfos = ActiveLotInfos.FindAll(l => l.State == LotStateEnum.Cancel);
                    TotalCanceledLotInfos.AddRange(Prev_ActiveLotInfos.FindAll(l => l.State == LotStateEnum.Cancel));

                    //foreach (var lotinfo in TotalCanceledLotInfos)
                    //{
                    //    //#PAbort Cancel되었을 경우 Unprocessed wafer는 Skip 처리해야함. allwafer.Where 조건문에 문제 있어서 아직 적용X
                    //    List<int> selecedSlot = lotinfo.UsingSlotList;
                    //    for (int i = 0; i < selecedSlot.Count; i++)
                    //    {                           
                    //        var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == selecedSlot[i] && w.CST_HashCode == lotinfo.CST_HashCode).FirstOrDefault();
                    //        if (wafer == null)
                    //        {
                    //            continue;
                    //        }
                    //        wafer.LotPriority = lotinfo.LotPriority;
                    //        wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;

                    //        var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                    //        if (realWafer != null)
                    //        {
                    //            realWafer.LOTID = lotinfo.LotID;
                    //            if(realWafer.WaferState == EnumWaferState.UNPROCESSED)
                    //            {
                    //                realWafer.WaferState = EnumWaferState.SKIPPED;
                    //            }
                    //        }
                    //    }
                    //}

                    for (int lotIdx = 0; lotIdx < TotalCanceledLotInfos.Count; lotIdx++)
                    {
                        bool isFoupEnd = false;
                        SetSkipWaferOnChuck(TotalCanceledLotInfos[lotIdx]);
                        var loaderJob = UnloadRequestJobFoupShiftNoBuffer(TotalCanceledLotInfos[lotIdx].FoupNumber, out isFoupEnd);
                        if (loaderJob == null)
                        {
                            if (isFoupEnd)
                            {
                                isLotEnd[lotIdx] = true;
                                return loaderJob;
                            }
                            else
                            {
                                isLotEnd[lotIdx] = false;
                            }
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }

                }



                for (int lotIdx = 0; lotIdx < Prev_ActiveLotInfos.Count; lotIdx++)
                {
                    ActiveLotInfo cur_prev_lotinfo = Prev_ActiveLotInfos[lotIdx];
                    //lock (Prev_ActiveLotInfos)
                    //{
                    if (cur_prev_lotinfo.State == LotStateEnum.Running)
                    {
                        int foupNum = cur_prev_lotinfo.FoupNumber;
                        List<int> selecedSlot = cur_prev_lotinfo.UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(cur_prev_lotinfo);
                        string cst_HashCode = cur_prev_lotinfo.CST_HashCode;

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.CST_HashCode == cst_HashCode).FirstOrDefault();
                            if (wafer == null)
                            {
                                //isLotEnd[lotIdx] = false;
                                continue;
                            }
                            wafer.LotPriority = cur_prev_lotinfo.LotPriority;
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;                         // [FOUP_SHIFT]
                            wafer.UsingStageList = cur_prev_lotinfo.UsingStageIdxList.ToList();//Prev_ActiveLotInfos[lotIdx].UsingStagesBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 스테이지를 지정한다.
                            wafer.DeviceName.Value = cur_prev_lotinfo.DeviceName;//Prev_ActiveLotInfos[lotIdx].UsingDeviceNameBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 Device 지정한다.

                            if (LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.SHIFT)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                    //else
                                    //{
                                    //    //#PAbort 흠... 확신이..
                                    //    ActiveLotInfos[lotIdx].State = LotStateEnum.Cancel;
                                    //}
                                }
                                else
                                {
                                    var pairwafer = allwafer.Where(w => w.CST_HashCode == cst_HashCode && w.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                                    var endPairwafer = pairwafer.Where(w => w.WaferState != EnumWaferState.UNPROCESSED && w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);
                                    if (pairwafer.Count() != endPairwafer.Count())
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                    else
                                    {

                                    }
                                }


                            }

                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = cur_prev_lotinfo.LotID;
                            }

                        }
                    }
                    //}

                }

                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;
                bool isLoaderMapExist = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                if (isArmExistWafer)
                {
                    LoaderInfo.StateMap = isExistMap;
                }
                else
                {
                    Func<bool[], bool> GetIsLoaderEnd = (bool[] lotEnd) =>
                    {
                        var loaderEnd = false;

                        //LotEnd - ActiveLotinfo != Running, Prev UNPROCESSED 가 없을때 

                        var notrunnig = ActiveLotInfos.Where(x => x.State == LotStateEnum.Idle ||
                                                                   x.State == LotStateEnum.Done ||
                                                                   x.State == LotStateEnum.End).Count();

                        //var runnig = ActiveLotInfos.Where(x => x.State == LotStateEnum.Running).Count();
                        var wafersNotInCst = Loader.GetLoaderInfo().StateMap.GetTransferObjectAll().Where(w => w.CurrPos.ModuleType != ModuleTypeEnum.SLOT &&
                                                                                                               w.WaferState == EnumWaferState.UNPROCESSED);

                        if (Prev_ActiveLotInfos.Count() > 0)
                        {
                            return false;
                        }

                        if (notrunnig == ActiveLotInfos.Count() && wafersNotInCst.Count() == 0)
                        {
                            foreach (var lotinfo in ActiveLotInfos)
                            {
                                foreach (var stageIndex in lotinfo.UsingStageIdxList)
                                {
                                    var stage = GetClient(lotinfo.UsingStageIdxList[stageIndex]);
                                    //#PAbort
                                    if (IsAliveClient(stage))
                                    {
                                        if (StageStates[stage.GetChuckID().Index - 1] != ModuleStateEnum.IDLE)//&& stage.GetErrorEndState() != ErrorEndStateEnum.NONE)
                                        {
                                            stage.LotOPResume();
                                        }
                                    }
                                }

                            }
                            return true;
                        }

                        if (lotEnd.Count(x => x == false) > 0)
                        {
                            return false;
                        }

                        for (var i = 0; i < lotEnd.Length; i++)
                        {

                            if (ActiveLotInfos[i].State == LotStateEnum.End || ActiveLotInfos[i].State == LotStateEnum.Cancel)
                            {
                                loaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                break;
                            }
                            else if (Prev_ActiveLotInfos.Count() > 0)
                            {
                                loaderEnd = false; //Prev_ActiveLotInfos 가 안끝난것이 있으면 기다려야한다. 
                                break;
                            }
                            else
                            {
                                loaderEnd = true;
                            }

                        }


                        return loaderEnd;
                    };

                    // [FOUP_SHIFT]*  
                    // Wafer가 Unload될 위치 찾는 함수 추가
                    // 1. Unload 가능한 Foup 찾기 (Cassette가 있어야 하고, 점유되지(Processed 또는 Skipped Wafer가 없는 Cassette) 않아야 하고, ...)
                    // 2. Unload 가능한 Slot 번호 찾기 (1번에서 찾은 Cassette의 비어있는 Slot 찾기)

                    /////////////////////////////////////////////////////////////////////
                    // RCMD로 PAbort를 받았을 때 PAbort를 받은 Cell을 가진 Foup의 Active Cell List Count가 0이면,
                    // Cell에 있는 Wafer는 Zup 전이면 Skipped, Zup 이후면 Processed 상태로 처리하고 Unload 되도록 한다.
                    // 이때 Cell에 있는 Wafer와 동일한 Hash Code를 가진 다른 Wafer의 상태가 Processed가 아니면 Skipped 처리하고 Unload 될 수 있도록 한다.
                    // Wafer를 Unload 하려고 할때 Unload할 Cassette가 없으면 Fixed Tray로 보내고 있으면 Cassette로 보낸다.
                    // PA에 Skipped 인 Wafer가 있으면, Unload할 Cassette가 없으면 Fixed Tray로 보내고 있으면 Cassette로 보낸다.

                    /////////////////////////////////////////////////////////////////////
                    //[FOUP_SHIFT]*
                    //if(IsFixedTrayExist) 랩핑으로...?
                    //Fixed Tray에 Unprocessed 가 아닌 웨이퍼가 있고 && 같은 Cst에서 나온 웨이퍼가 모두 Unprocessed가 아니면 && Wafer인 TransferObject.WaferType 이 Standard이면 
                    // Fixed Tray -> 점유되어있지 않은 Foup으로 Map 반환.
                    //else ... 아래 내용들 else로 넣기

                    /*
                    isExistMap = IsOCRAbortNoBuffer(LoaderInfo.StateMap, out isAbortWafer);
                    if (isAbortWafer)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    */

                    isExistMap = IsLoaderMapByState(LoaderInfo.StateMap, out isLoaderMapExist);
                    if (isLoaderMapExist)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {
                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;

                        for (int i = 0; i < isAvaibleArms.Count(); i++)
                        {
                            isAvaibleArms[i].ReservationState = EnumReservationState.NOT_RESERVE;
                        }
                        if (PreJobRingBuffer.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                {

                                    var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                    if (avaibleArm != null)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";
                                        cell = null;
                                        LoaderMap map = null;
                                        if (PreJobRingBuffer.Count > 0)
                                        {
                                            int chuckidx = PreJobRingBuffer.Peek();
                                            cell = GetClient(chuckidx);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);

                                        if (isNeedWafer)
                                        {
                                        }
                                        else if (map != null)
                                        {
                                            LoaderInfo.StateMap = map;
                                            allwafer = map.GetTransferObjectAll();
                                            foreach (var wafer in allwafer)
                                            {
                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                ModuleID dstPos = wafer.CurrPos;
                                                if (currSubObj.CurrPos != dstPos)
                                                {
                                                    if (wafer.Priority > 999)
                                                    {
                                                        if (isExchange)
                                                        {
                                                            exchangeCnt++;
                                                            bufferCount++;
                                                            wafer.Priority = exchangePriority++;
                                                        }
                                                        else
                                                        {
                                                            bufferCount++;
                                                            wafer.Priority = Priority++;
                                                        }
                                                    }
                                                }
                                            }
                                            avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                            //  LoaderInfo.StateMap.ARMModules[avaibleArm.ID.Index-1].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                                            loaderMapList.Add(map);
                                            PreJobRingBuffer.Dequeue();
                                            if (exchangeCnt >= 1)
                                            {
                                                jobdone = true;
                                                break;
                                            }
                                            if (MapSlicerErrorFlag)
                                            {
                                                jobdone = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            PreJobRingBuffer.Dequeue();
                                        }
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                LoggerManager.Debug("PreJobRingBuffer exception Error");
                                PreJobRingBuffer.Dequeue();
                            }
                        }



                        if (!jobdone)
                        {
                            for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                            {
                                var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);
                                if ((activeLotInfo.State == LotStateEnum.Running) || (Prev_ActiveLotInfos.Count() > 0))
                                {
                                    for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";
                                        var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                        if (avaibleArm != null)
                                        {
                                            int chuckIndx = activeLotInfo.UsingStageIdxList[i];
                                            var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                                            LoaderMap map = null;

                                            if (notExistChuckModule != null)
                                            {
                                                cell = GetClient(chuckIndx);
                                                if (cell != null && IsAliveClient(cell))// && cell.GetErrorEndState() == ErrorEndStateEnum.NONE)
                                                {
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                    }
                                                    if (isNeedWafer && (activeLotInfo.State == LotStateEnum.Running))
                                                    {
                                                        map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, false, activeLotInfo.DeviceName, chuckIndx);

                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }
                                                        isExistWafer = false;
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;

                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else if (map != null)
                                                    {
                                                        isExistWafer = false;

                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                        loaderMapList.Add(map);

                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                }

                                if (jobdone)
                                {
                                    break;
                                }
                            }

                            if (isExistWafer)
                            {
                                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = QueueActiveInfo.Peek();
                                    if ((activeLotInfo.State == LotStateEnum.Running) || (Prev_ActiveLotInfos.Count() > 0))
                                    {
                                        //if (activeLotInfo.RemainCount > activeLotInfo.UsingStageIdxList.Count)
                                        //{
                                        //    activeLotInfo.RemainCount = activeLotInfo.UsingStageIdxList.Count;
                                        //}
                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleArm = isAvaibleArms.FirstOrDefault(arm => arm.ReservationState == EnumReservationState.NOT_RESERVE);
                                            if (avaibleArm != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";
                                                int chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);

                                                LoaderMap map = null;
                                                if (!ClientList.Contains(cell))
                                                {

                                                    ClientList.Add(cell);
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                    }
                                                    if (isNeedWafer && (activeLotInfo.State == LotStateEnum.Running))
                                                    {
                                                        map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, isNeedWafer, activeLotInfo.DeviceName, chuckIndx);
                                                        //activeLotInfo.RingBuffer.Dequeue();
                                                        //activeLotInfo.RingBuffer.Enqueue(cell);
                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }

                                                        if (LoaderInfo.StateMap.PreAlignModules.Count(pa => pa.Substrate == null) < 2)// && SystemModuleCount.ModuleCnt.PACount > 2)// 잘못된 코드 였음. PA가 1개 이하로 남으면 skip해야함.
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        LoaderInfo.StateMap = map;
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        loaderMapList.Add(map);
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        if (IsAliveClient(cell))
                                                        {
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                            PreJobRingBuffer.Enqueue(chuckIndx);
                                                        }
                                                        else
                                                        {

                                                        }
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }

                                                    }
                                                    else if (map != null)
                                                    {
                                                        avaibleArm.ReservationState = EnumReservationState.RESERVE;
                                                        LoaderInfo.StateMap = map;
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                        loaderMapList.Add(map);
                                                        if (isExchange)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    if (IsAliveClient(cell))
                                                    {
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                    }
                                                    else
                                                    {

                                                    }
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }

                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);
                                }
                            }
                        }
                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();
                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                isLoaderEnd = GetIsLoaderEnd(isLotEnd);

                                return null;
                            }
                            else
                            {
                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                int lowLotPriortyCellIndex = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();

                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();

                                foreach (var activeLotInfo in ActiveLotInfos)
                                {
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        if (lowLotPriorty.LotPriority == activeLotInfo.LotPriority)
                                        {
                                            lowLotPriortyCellIndex = activeLotInfo.UsingStageIdxList[0];
                                            DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                            CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                        }
                                        DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                        CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                    }
                                }

                                if (LoaderInfo.StateMap.PreAlignModules.Count(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST) >= 2)
                                {
                                    devName = DeviceNames.Count() > 0 ? DeviceNames.Peek() : "";
                                    cellIdx = CellIndexs.Count() > 0 ? CellIndexs.Peek() : 0;

                                    if (DeviceNames.Count() > 0)
                                    {
                                        DeviceNames.Dequeue();
                                    }

                                    if (CellIndexs.Count > 0)
                                    {
                                        CellIndexs.Dequeue();
                                    }

                                    map = PreLoadingJobMakeNoBuffer(LoaderInfo.StateMap, false, devName, cellIdx);
                                    LoaderInfo.StateMap = map;
                                   
                                }
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                isLoaderEnd = GetIsLoaderEnd(isLotEnd);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return LoaderInfo.StateMap;
        }


        public LoaderMap ExternalRequestJob_Dynamic(out bool[] isLotEnd, out bool[] isLotInitFlag, out bool isLoaderEnd, out bool[] isLotPause)
        {
            isLotEnd = new bool[ActiveLotInfos.Count];
            isLotInitFlag = new bool[ActiveLotInfos.Count];
            isLotPause = new bool[ActiveLotInfos.Count];
            for (int i = 0; i < ActiveLotInfos.Count; i++)
            {
                isLotEnd[i] = false;
                isLotInitFlag[i] = false;
                isLotPause[i] = false;

            }
            isLoaderEnd = false;

            Queue<String> DeviceNames = new Queue<string>();
            Queue<int> CellIndexs = new Queue<int>();
            List<ActiveLotInfo> tmp_All_ActiveLotInfos = new List<ActiveLotInfo>();

            try
            {
                int exchangePriority = -1000;
                int Priority = 1;
                int PreLoadingPriority = 10;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();
                LoaderInfo = Loader.GetLoaderInfo();
                CheckUsingStage();
                CheckActiveLotInfo();
                List<string> lotRuningList = new List<string>();
                List<LotStateEnum> lotStateEnums = new List<LotStateEnum>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    lotStateEnums.Add(ActiveLotInfos[lotIdx].State);
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running ||
                       lotStateEnums[lotIdx] == LotStateEnum.Abort ||
                       lotStateEnums[lotIdx] == LotStateEnum.Cancel ||
                       lotStateEnums[lotIdx] == LotStateEnum.Suspend ||
                       lotStateEnums[lotIdx] == LotStateEnum.Pause)
                    {
                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    isLotEnd[lotIdx] = true;
                    if (lotStateEnums[lotIdx] == LotStateEnum.Running || lotStateEnums[lotIdx] == LotStateEnum.Abort)
                    {
                        tmp_All_ActiveLotInfos.Add(ActiveLotInfos[lotIdx]);
                        isLotInitFlag[lotIdx] = true;
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(ActiveLotInfos[lotIdx]);
                        ActiveLotInfos[lotIdx].CST_HashCode = LoaderInfo.StateMap.CassetteModules[foupNum - 1].CST_HashCode;
                        string cst_HashCode = ActiveLotInfos[lotIdx].CST_HashCode;
                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.CST_HashCode == cst_HashCode).FirstOrDefault();
                            if (wafer == null)
                            {
                                continue;
                                //Lot End Scan 불일치
                            }
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;

                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStagesBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 스테이지를 지정한다.
                            if (wafer.UsingStageList == null || wafer.UsingStageList.Count == 0)
                            {
                                wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            }
                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].UsingDeviceNameBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 Device 지정한다.
                            if (wafer.DeviceName.Value == null)
                            {
                                wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            }
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            //웨이퍼가 아직 할당인 안된경우는 Lot를 끝내선 안된다.
                            if (wafer.CurrHolder.ModuleType == wafer.OriginHolder.ModuleType && wafer.CurrHolder.Index == wafer.OriginHolder.Index)
                            {
                                if (wafer.WaferState == EnumWaferState.UNPROCESSED)
                                {
                                    if (!isAllErrorEnd)
                                    {
                                        isLotEnd[lotIdx] = false;
                                    }
                                }
                            }
                            else
                            {
                                if (wafer.CurrHolder.ModuleType == ModuleTypeEnum.SLOT && wafer.WaferState != EnumWaferState.UNPROCESSED)
                                {

                                }
                                else
                                {
                                    isLotEnd[lotIdx] = false;
                                }
                            }
                        }
                    }
                    else if (ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel)
                    {
                        isLotInitFlag[lotIdx] = true;
                        bool isFoupEnd = false;
                        SetSkipWaferOnChuck(ActiveLotInfos[lotIdx]);
                        var loaderJob = UnloadRequestJob(lotIdx + 1, out isFoupEnd);
                        if (isFoupEnd == true && loaderJob == null)
                        {
                            isLotEnd[lotIdx] = true;
                            return loaderJob;
                        }
                        else if (loaderJob == null && isFoupEnd == false)
                        {
                            isLotEnd[lotIdx] = false;
                        }
                        else if (loaderJob != null)
                        {
                            isLotEnd[lotIdx] = false;
                            return loaderJob;
                        }
                    }
                }


                bool isAcessBuffer = false;
                if (this.LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST) == LoaderInfo.StateMap.BufferModules.Count() - 1)
                {
                    isAcessBuffer = true;
                }

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (i == (LoaderInfo.StateMap.BufferModules.Count() - 1) && (!isAcessBuffer) || !this.LoaderInfo.StateMap.BufferModules[i].Enable)
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        if ((!isAcessBuffer) && this.LoaderInfo.StateMap.BufferModules[i].WaferStatus == EnumSubsStatus.EXIST || !this.LoaderInfo.StateMap.BufferModules[i].Enable)
                        {
                            this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                        }
                        else
                        {
                            this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                        }
                    }
                }


                for (int lotIdx = 0; lotIdx < Prev_ActiveLotInfos.Count; lotIdx++)
                {
                    if (Prev_ActiveLotInfos[lotIdx].State == LotStateEnum.Running)
                    {
                        tmp_All_ActiveLotInfos.Add(Prev_ActiveLotInfos[lotIdx]);
                        int foupNum = Prev_ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = Prev_ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();
                        bool isAllErrorEnd = IsAllErrorEndStage(Prev_ActiveLotInfos[lotIdx]);
                        string cst_HashCode = Prev_ActiveLotInfos[lotIdx].CST_HashCode;

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.CST_HashCode == cst_HashCode).FirstOrDefault();
                            if (wafer == null)
                            {
                                //isLotEnd[lotIdx] = false;
                                continue;
                            }
                            wafer.LotPriority = Prev_ActiveLotInfos[lotIdx].LotPriority;
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            wafer.UsingStageList = Prev_ActiveLotInfos[lotIdx].UsingStagesBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 스테이지를 지정한다.
                            if (wafer.UsingStageList == null || wafer.UsingStageList.Count == 0)
                            {
                                wafer.UsingStageList = Prev_ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            }
                            wafer.DeviceName.Value = Prev_ActiveLotInfos[lotIdx].UsingDeviceNameBySlot.FirstOrDefault(slot => slot.Key == selecedSlot[i]).Value; // 슬롯에 Device 지정한다.
                            if (wafer.DeviceName.Value == null)
                            {
                                wafer.DeviceName.Value = Prev_ActiveLotInfos[lotIdx].DeviceName;
                            }


                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = Prev_ActiveLotInfos[lotIdx].LotID;
                            }

                        }
                    }
                }


                int bufferCount = 0;
                bool jobdone = false;
                HashSet<ILoaderServiceCallback> ClientList = new HashSet<ILoaderServiceCallback>();
                bool isExistWafer = true;
                bool isArmExistWafer = true;
                bool isPAExistWafer = true;
                bool isAbortWafer = true;
                int exchangeCnt = 0;
                LoaderMap isExistMap = null;
                LoaderMap isPAExistMap = null;
                isExistMap = IsExistLoaderArm(LoaderInfo.StateMap, out isArmExistWafer);
                isPAExistMap = IsExistLoaderPA(LoaderInfo.StateMap, out isPAExistWafer);
                if (isArmExistWafer)
                {
                    LoaderInfo.StateMap = isExistMap;
                }
                else if (isPAExistWafer)
                {
                    LoaderInfo.StateMap = isPAExistMap;
                }
                else
                {
                    isExistMap = IsOCRAbort(LoaderInfo.StateMap, out isAbortWafer);
                    if (isAbortWafer)
                    {
                        LoaderInfo.StateMap = isExistMap;
                    }
                    else
                    {

                        ILoaderServiceCallback cell = null;
                        int cellIdx = 0;
                        List<int> PreJobRingBufferCellIndexs = new List<int>();


                        if (PreJobRingBuffer.Count > 0)
                        {
                            try
                            {
                                for (int i = 0; i < PreJobRingBuffer.Count; i++)
                                {
                                    var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);
                                    if (avaibleBuffer != null)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        cell = null;
                                        LoaderMap map = null;
                                        if (PreJobRingBuffer.Count > 0)
                                        {
                                            int chuckidx = PreJobRingBuffer.Peek();
                                            cell = GetClient(chuckidx);
                                            PreJobRingBufferCellIndexs.Add(chuckidx);
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        if (cell != null)
                                        {
                                            map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                            if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                            {
                                                LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                map = null;
                                            }

                                            if (map != null)
                                            {
                                                LoaderInfo.StateMap = map;
                                                allwafer = map.GetTransferObjectAll();
                                                foreach (var wafer in allwafer)
                                                {
                                                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                    ModuleID dstPos = wafer.CurrPos;
                                                    if (currSubObj.CurrPos != dstPos)
                                                    {
                                                        if (wafer.Priority > 999)
                                                        {
                                                            if (isExchange)
                                                            {
                                                                exchangeCnt++;
                                                                bufferCount++;
                                                                wafer.Priority = exchangePriority++;
                                                            }
                                                            else
                                                            {
                                                                bufferCount++;
                                                                wafer.Priority = Priority++;
                                                            }
                                                        }
                                                    }
                                                }
                                                ReservationJob(map, avaibleBuffer);
                                                loaderMapList.Add(map);
                                                PreJobRingBuffer.Dequeue();
                                                if (exchangeCnt >= 2)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                                if (MapSlicerErrorFlag)
                                                {
                                                    jobdone = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                PreJobRingBuffer.Dequeue();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                LoggerManager.Debug("PreJobRingBuffer exception Error");
                                PreJobRingBuffer.Dequeue();
                            }
                        }



                        if (!jobdone)
                        {
                            for (int lotIdx = 0; lotIdx < QueueActiveInfo.Count; lotIdx++)
                            {
                                var activeLotInfo = QueueActiveInfo.ElementAt(lotIdx);
                                if (activeLotInfo.State == LotStateEnum.Running || (Prev_ActiveLotInfos.Count() > 0))
                                {
                                    for (int i = 0; i < activeLotInfo.UsingStageIdxList.Count; i++)
                                    {
                                        bool isExchange = false;
                                        bool isNeedWafer = false;
                                        bool isTempReady = false;
                                        string cstHashCodeOfRequestLot = "";

                                        var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);
                                        if (avaibleBuffer != null)
                                        {
                                            int chuckIndx = activeLotInfo.UsingStageIdxList[i];
                                            var notExistChuckModule = LoaderInfo.StateMap.ChuckModules.Where(module => module.ID.Index == chuckIndx && module.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();
                                            LoaderMap map = null;

                                            if (PreJobRingBufferCellIndexs.Contains(chuckIndx))
                                            {
                                                continue;
                                            }
                                            else if (notExistChuckModule != null)
                                            {
                                                cell = GetClient(activeLotInfo.UsingStageIdxList[i]);
                                                if (cell != null && IsAliveClient(cell))
                                                {
                                                    map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                    if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                    {
                                                        LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                        map = null;
                                                    }

                                                    if (bufferCount >= 4)
                                                    {
                                                        jobdone = true;
                                                        break;
                                                    }

                                                    if (map != null)
                                                    {
                                                        isExistWafer = false;
                                                        ReservationJob(map, avaibleBuffer);
                                                        LoaderInfo.StateMap = map;
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = Priority++;
                                                                }
                                                            }
                                                        }
                                                        loaderMapList.Add(map);
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else if (isNeedWafer && (activeLotInfo.State == LotStateEnum.Running))
                                                    {
                                                        map = PreLoadingJobMake(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);
                                                        //activeLotInfo.RingBuffer.Dequeue();
                                                        //activeLotInfo.RingBuffer.Enqueue(cell);
                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }

                                                        isExistWafer = false;

                                                        if (bufferCount >= 4)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        ReservationJob(map, avaibleBuffer);
                                                        LoaderInfo.StateMap = map;
                                                        loaderMapList.Add(map);
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }

                                                }
                                                else
                                                {

                                                    continue;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            jobdone = true;
                                            break;
                                        }
                                    }
                                }

                                if (jobdone)
                                {
                                    break;
                                }
                            }

                            if (isExistWafer)
                            {

                                for (int lotIdx = 0; lotIdx < tmp_All_ActiveLotInfos.Count; lotIdx++)
                                {
                                    var activeLotInfo = tmp_All_ActiveLotInfos[lotIdx];
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        //if (activeLotInfo.RemainCount > activeLotInfo.UsingStageIdxList.Count)
                                        //{
                                        //    activeLotInfo.RemainCount = activeLotInfo.UsingStageIdxList.Count;
                                        //}
                                        if (activeLotInfo.RemainCount == 0)
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.UsingStageIdxList.Count;
                                            for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                            {
                                                var cellinfo = GetClient(activeLotInfo.UsingStageIdxList[i]);
                                                activeLotInfo.RingBuffer.Enqueue(activeLotInfo.UsingStageIdxList[i]);
                                            }
                                        }

                                        for (int i = 0; i < activeLotInfo.RemainCount; i++)
                                        {
                                            var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);
                                            if (avaibleBuffer != null)
                                            {
                                                bool isExchange = false;
                                                bool isNeedWafer = false;
                                                bool isTempReady = false;
                                                string cstHashCodeOfRequestLot = "";

                                                int chuckIndx = 0;

                                                chuckIndx = activeLotInfo.RingBuffer.Peek();
                                                cell = GetClient(chuckIndx);
                                                if (cell == null || !IsAliveClient(cell) || PreJobRingBufferCellIndexs.Contains(chuckIndx)) //Prejob에서 Request 받은 Cell은 다시 물어보면 안된다.
                                                {
                                                    //연결이 끊긴 cell에 대해 RingBuffer내의 index 조절을 위해 dequeue/Enqueue 추가
                                                    activeLotInfo.RingBuffer.Dequeue();
                                                    activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                }
                                                else
                                                {
                                                    LoaderMap map = null;
                                                    if (!ClientList.Contains(cell))
                                                    {

                                                        ClientList.Add(cell);
                                                        map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);
                                                        if (map != null && cell.GetLotState() == ModuleStateEnum.IDLE && false == cell.IsLotAbort())
                                                        {
                                                            LotOPStartClient(cell, cstHashCodeOfRequestLot: cstHashCodeOfRequestLot);
                                                            map = null;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        continue;
                                                    }

                                                    if (isNeedWafer)
                                                    {
                                                        map = PreLoadingJobMake(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);
                                                        //activeLotInfo.RingBuffer.Dequeue();
                                                        //activeLotInfo.RingBuffer.Enqueue(cell);
                                                        if (map == null)
                                                        {
                                                            continue;
                                                        }

                                                        if (bufferCount >= 4)
                                                        {
                                                            activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                            jobdone = true;
                                                            break;
                                                        }
                                                        allwafer = map.GetTransferObjectAll();
                                                        foreach (var wafer in allwafer)
                                                        {
                                                            TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                            ModuleID dstPos = wafer.CurrPos;
                                                            if (currSubObj.CurrPos != dstPos)
                                                            {
                                                                if (wafer.Priority > 999)
                                                                {
                                                                    bufferCount++;
                                                                    wafer.Priority = PreLoadingPriority++;
                                                                }
                                                            }
                                                        }
                                                        LoaderInfo.StateMap = map;
                                                        ReservationJob(map, avaibleBuffer);
                                                        loaderMapList.Add(map);
                                                        activeLotInfo.RingBuffer.Dequeue();
                                                        activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        PreJobRingBuffer.Enqueue(chuckIndx);
                                                        if (MapSlicerErrorFlag)
                                                        {
                                                            jobdone = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (map != null)
                                                        {
                                                            ReservationJob(map, avaibleBuffer);
                                                            LoaderInfo.StateMap = map;
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        if (isExchange)
                                                                        {
                                                                            exchangeCnt++;
                                                                            bufferCount++;
                                                                            wafer.Priority = exchangePriority++;
                                                                        }
                                                                        else
                                                                        {
                                                                            bufferCount++;
                                                                            wafer.Priority = Priority++;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);

                                                            if (exchangeCnt > 2)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }
                                                        else if (isNeedWafer)
                                                        {
                                                            map = PreLoadingJobMake(LoaderInfo.StateMap, activeLotInfo.DeviceName, chuckIndx);
                                                            //activeLotInfo.RingBuffer.Dequeue();
                                                            //activeLotInfo.RingBuffer.Enqueue(cell);
                                                            if (map == null)
                                                            {
                                                                continue;
                                                            }

                                                            if (bufferCount >= 4)
                                                            {
                                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                                jobdone = true;
                                                                break;
                                                            }
                                                            allwafer = map.GetTransferObjectAll();
                                                            foreach (var wafer in allwafer)
                                                            {
                                                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                                                ModuleID dstPos = wafer.CurrPos;
                                                                if (currSubObj.CurrPos != dstPos)
                                                                {
                                                                    if (wafer.Priority > 999)
                                                                    {
                                                                        bufferCount++;
                                                                        wafer.Priority = PreLoadingPriority++;
                                                                    }
                                                                }
                                                            }
                                                            ReservationJob(map, avaibleBuffer);
                                                            LoaderInfo.StateMap = map;
                                                            loaderMapList.Add(map);
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                            PreJobRingBuffer.Enqueue(chuckIndx);
                                                            if (MapSlicerErrorFlag)
                                                            {
                                                                jobdone = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            activeLotInfo.RingBuffer.Dequeue();
                                                            activeLotInfo.RingBuffer.Enqueue(chuckIndx);
                                                        }
                                                        //}
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RemainCount - i;
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                        if (jobdone)
                                        {
                                            if (activeLotInfo.RemainCount <= 0)
                                            {
                                                activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                            }
                                        }
                                        else
                                        {
                                            activeLotInfo.RemainCount = activeLotInfo.RingBuffer.Count();
                                        }
                                    }

                                    if (jobdone)
                                    {
                                        break;
                                    }
                                    QueueActiveInfo.Dequeue();
                                    QueueActiveInfo.Enqueue(activeLotInfo);
                                }
                            }
                        }
                        if (loaderMapList.Count() == 0)
                        {
                            var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                                item =>
                                item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                                item.WaferType.Value == EnumWaferType.STANDARD &&
                                item.WaferState == EnumWaferState.UNPROCESSED &&
                                item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();
                            if (loadWafer == null || loadWafer.Count == 0)
                            {
                                if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        bool existcellrunning = false;
                                        if (tmp_All_ActiveLotInfos.Count == 0)
                                        {
                                            if (Prev_ActiveLotInfos.Count != 0)
                                            {
                                                isLoaderEnd = false;
                                                existcellrunning = true;
                                            }
                                        }

                                        for (int i = 0; i < tmp_All_ActiveLotInfos.Count; i++)
                                        {
                                            foreach (var stageidx in tmp_All_ActiveLotInfos[i].UsingStageIdxList)
                                            {
                                                var stage = GetClient(stageidx);
                                                if (stage != null)
                                                {
                                                    var waferStatus = stage.GetChuckWaferStatus();
                                                    var lotState = stage.GetLotState();
                                                    if (waferStatus == EnumSubsStatus.EXIST || lotState == ModuleStateEnum.RUNNING)
                                                    {
                                                        isLoaderEnd = false;
                                                        existcellrunning = true;
                                                        isLotEnd[tmp_All_ActiveLotInfos[i].FoupNumber - 1] = false;
                                                        if (cell == null)
                                                            cell = stage;
                                                        //LoggerManager.Debug($"[DYNAMIC MODE] LOADER END Fail. Cell{cell.GetChuckID().Index}. WaferStatus:{waferStatus} , LOTState:{lotState}");
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (existcellrunning == false)
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
                                }
                                return null;
                            }
                            else
                            {
                                HolderModuleInfo avaibleBuffer = null;

                                LoaderMap map = null;
                                string devName = null;
                                cellIdx = 0;
                                int lowLotPriortyCellIndex = 0;
                                int existDeviceCount = 0;
                                DeviceNames.Clear();
                                CellIndexs.Clear();
                                var lowLotPriorty = ActiveLotInfos.Where(i => i.State == LotStateEnum.Running).OrderBy(i => i.LotPriority).FirstOrDefault();
                                for (int i = 0; i < ActiveLotInfos.Count; i++)
                                {

                                    var activeLotInfo = ActiveLotInfos[i];
                                    if (activeLotInfo.State == LotStateEnum.Running)
                                    {
                                        if (activeLotInfo.UsingStageIdxList.Count > 0)
                                        {
                                            if (lowLotPriorty.LotPriority == activeLotInfo.LotPriority)
                                            {
                                                lowLotPriortyCellIndex = activeLotInfo.UsingStageIdxList[0];
                                                DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                                CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                            }
                                            DeviceNames.Enqueue(activeLotInfo.DeviceName);
                                            CellIndexs.Enqueue(activeLotInfo.UsingStageIdxList[0]);
                                        }
                                    }
                                }
                                int jobCount = 0;// 버퍼에 다 담는게 아니고 2개씩 끊어서 담아야한다.
                                do
                                {
                                    if (jobCount >= 2)
                                    {
                                        break;
                                    }
                                    avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                    if (avaibleBuffer != null)
                                    {
                                        if (DeviceNames.Count > 0)
                                        {
                                            devName = DeviceNames.Peek();
                                            DeviceNames.Dequeue();
                                        }
                                        if (CellIndexs.Count > 0)
                                        {
                                            cellIdx = CellIndexs.Peek();
                                            CellIndexs.Dequeue();
                                        }

                                        if (lowLotPriortyCellIndex == cellIdx)
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(lowLotPriortyCellIndex));
                                            if (existDeviceCount < 2) // 버퍼 2개까지 허용
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }
                                        else
                                        {
                                            existDeviceCount = LoaderInfo.StateMap.BufferModules.Count(i => i.WaferStatus == EnumSubsStatus.EXIST && i.Substrate.UsingStageList.Contains(cellIdx));
                                            if (existDeviceCount < 1) // 버퍼 1개까지 허용
                                            {
                                                map = PreLoadingJobMake(LoaderInfo.StateMap, devName, cellIdx);
                                            }
                                            else
                                            {
                                                map = null;
                                            }
                                        }

                                        if (map != null)
                                        {
                                            ReservationJob(map, avaibleBuffer);
                                            LoaderInfo.StateMap = map;
                                            jobCount++;
                                            if (MapSlicerErrorFlag)
                                            {
                                                jobdone = true;
                                                break;
                                            }
                                        }

                                    }
                                } while (avaibleBuffer != null && DeviceNames.Count > 0);
                            }
                        }

                        if (LoaderInfo.StateMap == null)
                        {
                            var unProcessWafer = allwafer.FirstOrDefault(i => i.WaferType.Value == EnumWaferType.STANDARD && i.WaferState == EnumWaferState.UNPROCESSED && i.CurrPos.ModuleType != ModuleTypeEnum.SLOT && i.ProcessingEnable == ProcessingEnableEnum.ENABLE);
                            if (unProcessWafer == null)
                            {
                                if (isLotEnd[0] && isLotEnd[1] && isLotEnd[2])
                                {
                                    if (ActiveLotInfos[0].State == LotStateEnum.End || ActiveLotInfos[1].State == LotStateEnum.End || ActiveLotInfos[2].State == LotStateEnum.End)
                                    {
                                        isLoaderEnd = false; //카세트가 언로드가 안됬기때문에 기다려야한다.
                                    }
                                    else
                                    {
                                        bool existcellrunning = false;
                                        if (tmp_All_ActiveLotInfos.Count == 0)
                                        {
                                            if (Prev_ActiveLotInfos.Count != 0)
                                            {
                                                isLoaderEnd = false;
                                                existcellrunning = true;
                                            }
                                        }

                                        for (int i = 0; i < tmp_All_ActiveLotInfos.Count; i++)
                                        {
                                            foreach (var stageidx in tmp_All_ActiveLotInfos[i].UsingStageIdxList)
                                            {
                                                var stage = GetClient(stageidx);
                                                if (stage != null)
                                                {
                                                    var waferStatus = stage.GetChuckWaferStatus();
                                                    var lotState = stage.GetLotState();
                                                    if (waferStatus == EnumSubsStatus.EXIST || lotState == ModuleStateEnum.RUNNING)
                                                    {
                                                        //LoggerManager.Debug($"[DYNAMIC MODE] LOADER END Fail. Cell{cell.GetChuckID().Index}. WaferStatus:{waferStatus} , LOTState:{lotState}");
                                                        isLoaderEnd = false;
                                                        existcellrunning = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (existcellrunning == false)
                                        {
                                            isLoaderEnd = true;
                                        }
                                    }
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

            return LoaderInfo.StateMap;
        }

        public LoaderMap ExternalRequestJob_Idle()
        {
            LoaderMap loaderMap = null;

            Func<bool> CheckLoaderCommunicationManager = () =>
            {
                if ((loaderCommunicationManager == null) ||
                    (loaderCommunicationManager.Cells == null) ||
                    (loaderCommunicationManager.Cells.Count <= 0))
                {
                    return false;
                }

                return true;
            };

            try
            {
                bool bCheckLoaderComm = false;
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    bCheckLoaderComm = CheckLoaderCommunicationManager();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        bCheckLoaderComm = CheckLoaderCommunicationManager();
                    });
                }

                if (!bCheckLoaderComm)
                {
                    return loaderMap;
                }

                loaderMap = RequestJob_Idle();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return loaderMap;
        }

        private LoaderMap RequestJob_Idle()
        {
            Func<IStageObject, List<ActiveLotInfo>, bool> IsAllocatedCell = (IStageObject cell, List<ActiveLotInfo> lotInfos) =>
            {
                bool isAllocatedCell = false;
                try
                {
                    foreach (var lotInfo in lotInfos)
                    {
                        foreach (var stageidx in lotInfo.UsingStageIdxList)
                        {
                            var stage = GetClient(stageidx);
                            if (stage != null)
                            {
                                if (stage.GetChuckID().Index == cell.Index &&
                                        (lotInfo.State == LotStateEnum.Running ||
                                         lotInfo.State == LotStateEnum.Pause ||
                                         lotInfo.State == LotStateEnum.Suspend))
                                {
                                    isAllocatedCell = true;
                                    break;
                                }
                            }
                        }

                        if (isAllocatedCell)
                        {
                            break;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return isAllocatedCell;
            };

            LoaderMap loaderMap = null;

            try
            {
                // Loader Robot이 Lock 상태이면 LoaderMap을 생성하지 않는다.
                if (GPLoader.LoaderRobotLockState)
                {
                    return null;
                }

                var ccsupervisor = cont.Resolve<ICardChangeSupervisor>();

                foreach (var cell in loaderCommunicationManager.Cells)
                {
                    //연결되지 않았거나, 현재 해당 셀에 connect/disconnect 중이면 skip
                    if (!cell.StageInfo.IsConnected || cell.StageInfo.IsConnectChecking)
                    {
                        continue;
                    }

                    string clientName = $"CHUCK{cell.Index}";
                    ILoaderServiceCallback client = null;
                    if (Clients.ContainsKey(clientName))
                    {
                        client = Clients[clientName];
                    }

                    if (client == null ||
                        !IsAliveClient(client) ||
                        !client.IsEnablePolishWaferSoaking() ||
                        (cell.LockMode == StageLockMode.LOCK) ||
                        (cell.StageMode != GPCellModeEnum.ONLINE) ||
                        (cell.StageState != ModuleStateEnum.IDLE))
                    {
                        continue;
                    }

                    bool isAllocatedCell = IsAllocatedCell(cell, ActiveLotInfos);
                    if (!isAllocatedCell)
                    {
                        isAllocatedCell = IsAllocatedCell(cell, Prev_ActiveLotInfos);
                    }

                    // Cell이 포함된 Lot이 Running/Pause/Suspend 상태이면 IDLE 상태에서는 해당 Cell에는 RequsetJob을 물어보지 않는다.
                    if (isAllocatedCell)
                    {
                        continue;
                    }


                    bool isRequestJob = false;

                    if (ccsupervisor.ModuleState.GetState() == ModuleStateEnum.RUNNING || ccsupervisor.GetActiveCCInfos()?.Count() > 0)
                    {
                        // 할당된 셀에만 RequestJob을 허용하기로 함.
                        if(ccsupervisor.GetRunningCCInfo().cardreqmoduleIndex == cell.Index && client.GetChuckWaferStatus() == EnumSubsStatus.EXIST)
                        {
                            isRequestJob = true;
                        }
                    }
                    else
                    {
                        isRequestJob = true;
                    }

                    if(isRequestJob)
                    {
                        var tempLoaderMap = client.RequestJob(Loader.GetLoaderInfo(), out var isExchange, out var isNeedWafer, out var isTempReady, out var cstHashCodeOfRequestLot);// <- 일반적인 상황에 여기탐.
                        if (tempLoaderMap == null)
                        {
                            continue;
                        }

                        loaderMap = tempLoaderMap;
                        LoggerManager.Debug($"RequestJob_Idle(). CellIdx: {cell.Index},  UsePW: {client.IsEnablePolishWaferSoaking()}, CellMode: {cell.LockMode}, CellStageMode: {cell.StageMode}, CellStageState: {cell.StageState}");
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return loaderMap;
        }

        private ActiveLotInfo GetPrev_ActiveLotInfos(int foupnum)
        {
            ActiveLotInfo ret = null;
            try
            {
                ret = Prev_ActiveLotInfos.Find(item => item.FoupNumber == foupnum);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;

        }
        //public LoaderMap ExternalRequestJob2(out bool isLotEnd)
        //{
        //    try
        //    {
        //        DownloadStageRecipeActReqData recipeActReqData = new DownloadStageRecipeActReqData();
        //        recipeActReqData.FoupNumber = actReqData.FoupNumber;

        //        foreach (var stage in actReqData.UseStageNumbers)
        //        {
        //            recipeActReqData.RecipeDic.Add(stage, devname);
        //        }

        //        this.DeviceManager().SetRecipeToStage(recipeActReqData);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        #region InternalLot
        public void LotOPStart()
        {
            for (int i = 0; i < StageCount; i++)
            {
                isLotStartFlag[i] = false;
            }
            if (CellsInfo.Count > 0)
            {

                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Name.Contains(Clients.Values.ElementAt(i).GetChuckID().Index.ToString()));
                    //var cellInfo = cells.ToList<StageObject>().Find(cell => cell.Name.Contains(Clients.Keys.ElementAt(i)));


                    if (cellInfo.StageInfo.IsChecked)
                    {
                        if (IsAliveClient(Clients.Values.ElementAt(i)))
                        {
                            var isSucess = LotOPStartClient(Clients.Values.ElementAt(i));
                            isLotStartFlag[cellInfo.Index - 1] = isSucess;
                            LoggerManager.Debug($"[LOADER SUPERVISOR] LotOPStart sucess stage number : {cellInfo.Index}");
                        }
                        else
                        {
                            isLotStartFlag[cellInfo.Index - 1] = false;
                            LoggerManager.Debug($"[LOADER SUPERVISOR] LotOPStart fail stage number(IsAliveClent false) : {cellInfo.Index}");
                        }
                    }
                    else
                    {
                        isLotStartFlag[cellInfo.Index - 1] = false;
                        LoggerManager.Debug($"[LOADER SUPERVISOR] LotOPStart fail stage number(StageInfo IsExcute Lot false) : {cellInfo.Index}");
                    }

                }
            }
            else
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var isSucess = LotOPStartClient(Clients.Values.ElementAt(i));
                    isLotStartFlag[i] = true;
                }
            }
            //SemaphoreSlim semaphore = new SemaphoreSlim(0);
            //this.EventManager().RaisingEvent(typeof(LotStartEvent).FullName, new ProbeEventArgs(this, semaphore));
            //semaphore.Wait();
        }

        public void LotOPPause()
        {
            if (CellsInfo.Count > 0)
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Name.Contains(Clients.Values.ElementAt(i).GetChuckID().Index.ToString()));
                    //var cellInfo = cells.ToList<StageObject>().Find(cell => cell.Name.Contains(Clients.Keys.ElementAt(i)));


                    if (cellInfo.StageInfo.IsChecked)
                    {
                        var isSucess = LotOPPauseClient(Clients.Values.ElementAt(i));
                    }
                }
            }
            else
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var isSucess = LotOPPauseClient(Clients.Values.ElementAt(i));
                }
            }
        }

        public void LotOPResume()
        {
            if (CellsInfo.Count > 0)
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Name.Contains(Clients.Values.ElementAt(i).GetChuckID().Index.ToString()));
                    if (cellInfo.StageInfo.IsChecked || Clients.Values.ElementAt(i) is StageEmulation)
                    {
                        var isSucess = LotOPResumeClient(Clients.Values.ElementAt(i));
                    }
                }
            }
            else
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var isSucess = LotOPResumeClient(Clients.Values.ElementAt(i));
                    isLotStartFlag[i] = isSucess;
                }
            }
        }
        public void LotOPEnd()
        {
            if (CellsInfo.Count > 0)
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Name.Contains(Clients.Values.ElementAt(i).GetChuckID().Index.ToString()));
                    var isSucess = LotOPEndClient(Clients.Values.ElementAt(i));
                }
            }
            else
            {
                for (int i = priorityNum; i < Clients.Values.Count; i++)
                {
                    var isSucess = LotOPEndClient(Clients.Values.ElementAt(i));
                    isLotStartFlag[i] = isSucess;
                }
            }
        }
        #endregion



        Queue<ILoaderServiceCallback> RingBuffer = new Queue<ILoaderServiceCallback>();

        public void ArrangeCallbackIndex()
        {

            RingBuffer = new Queue<ILoaderServiceCallback>();
            var cells = Clients.Values.OrderBy(i => i.GetChuckID().Index).ToList();
            foreach (var cell in cells)
            {
                if (isLotStartFlag[cell.GetChuckID().Index - 1])
                {
                    RingBuffer.Enqueue(cell);
                }
            }
        }


        public LoaderMap RequestJob()
        {
            try
            {
                LoaderInfo = Loader.GetLoaderInfo();
                var allwafer = LoaderInfo.StateMap.GetTransferObjectAll();
                List<int> usingStageList = new List<int>();

                for (int i = 1; i <= StageCount; i++)
                {
                    usingStageList.Add(i);
                }
                foreach (var wafer in allwafer)
                {
                    wafer.UsingStageList = usingStageList.ToList();
                    wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                    if (OCRDebugginFlag)
                    {
                        wafer.SetOCRState("", 0, OCRReadStateEnum.DONE);
                    }
                }
                int Priority = 1;
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (i == (LoaderInfo.StateMap.BufferModules.Count() - 1))
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                }

                int exchangeCount = 0;
                int bufferCount = 0;

                for (int i = 0; i < RingBuffer.Count; i++)
                {
                    var avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(buffer => buffer.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);

                    if (avaibleBuffer != null)
                    {

                        bool isExchange = false;
                        bool isNeedWafer = false;
                        bool isTempReady = false;
                        string cstHashCodeOfRequestLot = "";
                        
                        var cell = RingBuffer.Peek();

                        LoaderMap map = null;

                        if (IsAliveClient(cell) && isLotStartFlag[cell.GetChuckID().Index - 1])
                        {
                            map = RequestJobClient(cell, LoaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot);

                            if (map != null)
                            {
                                bufferCount++;
                            }

                            if (bufferCount >= 5)
                            {
                                break;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        if (map != null)
                        {
                            ReservationJob(map, avaibleBuffer);
                            LoaderInfo.StateMap = map;
                            allwafer = map.GetTransferObjectAll();
                            foreach (var wafer in allwafer)
                            {
                                TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                                ModuleID dstPos = wafer.CurrPos;
                                if (currSubObj.CurrPos != dstPos)
                                {
                                    currSubObj.DstPos = dstPos;
                                    if (wafer.Priority > 999)
                                    {
                                        wafer.Priority = Priority++;
                                    }
                                }
                            }

                            loaderMapList.Add(map);
                            RingBuffer.Dequeue();
                            RingBuffer.Enqueue(cell);
                            if (isExchange)
                            {
                                exchangeCount++;
                            }
                            if (exchangeCount >= 2)
                            {
                                break;
                            }
                        }
                        else
                        {
                            RingBuffer.Dequeue();
                            RingBuffer.Enqueue(cell);
                        }
                    }

                    else
                    {
                        break;
                    }
                }

                if (loaderMapList.Count() == 0)
                {
                    var loadWafer = LoaderInfo.StateMap.GetTransferObjectAll().Where(
                        item =>
                        item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                        item.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK &&
                        item.WaferType.Value == EnumWaferType.STANDARD &&
                        item.WaferState == EnumWaferState.UNPROCESSED).ToList();
                    if (loadWafer == null || loadWafer.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        HolderModuleInfo avaibleBuffer = null;
                        do
                        {
                            avaibleBuffer = LoaderInfo.StateMap.BufferModules.FirstOrDefault(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST);
                            if (avaibleBuffer != null)
                            {
                                var map = PreLoadingJobMake(LoaderInfo.StateMap);
                                if (map == null)
                                {
                                    break;
                                }
                                ReservationJob(map, avaibleBuffer);
                                LoaderInfo.StateMap = map;
                            }
                        } while (avaibleBuffer != null);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return LoaderInfo.StateMap;
        }
        public LoaderMap UnloadRequestJob(out bool isLotEnd, out bool isLockMode, out bool isAvailableModule)
        {
            LoaderMap map = null;
            isLotEnd = true;
            isLockMode = false;
            isAvailableModule = true;
            try
            {
                LoaderInfo = Loader.GetLoaderInfo();
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (i == (LoaderInfo.StateMap.BufferModules.Count() - 1))
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                }
                var currentMap = LoaderInfo.StateMap;

                List<string> lotRuningList = new List<string>();
                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (ActiveLotInfos[lotIdx].State == LotStateEnum.Running ||
                       ActiveLotInfos[lotIdx].State == LotStateEnum.Abort ||
                       ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel ||
                       ActiveLotInfos[lotIdx].State == LotStateEnum.Suspend ||
                       ActiveLotInfos[lotIdx].State == LotStateEnum.Pause)
                    {
                        ActiveLotInfos[lotIdx].CST_HashCode = currentMap.CassetteModules[lotIdx].CST_HashCode;

                        if (ActiveLotInfos[lotIdx].CST_HashCode != null && !ActiveLotInfos[lotIdx].CST_HashCode.Equals(""))
                        {
                            lotRuningList.Add(ActiveLotInfos[lotIdx].CST_HashCode);
                        }
                    }
                }
                var allwafer = currentMap.GetTransferObjectAll();
                foreach (var wafer in allwafer)
                {
                    wafer.ProcessingEnable = ProcessingEnableEnum.DISABLE;
                    wafer.LOTRunning_CSTHash_List = lotRuningList;
                }

                for (int lotIdx = 0; lotIdx < ActiveLotInfos.Count; lotIdx++)
                {
                    if (ActiveLotInfos[lotIdx].State == LotStateEnum.Running ||
                      ActiveLotInfos[lotIdx].State == LotStateEnum.Abort ||
                      ActiveLotInfos[lotIdx].State == LotStateEnum.Cancel ||
                      ActiveLotInfos[lotIdx].State == LotStateEnum.Suspend ||
                      ActiveLotInfos[lotIdx].State == LotStateEnum.Pause)
                    {
                        int foupNum = ActiveLotInfos[lotIdx].FoupNumber;
                        List<int> selecedSlot = ActiveLotInfos[lotIdx].UsingSlotList;
                        var slotList = selecedSlot.ToList();

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            int slotNum = selecedSlot[i];
                            slotList[i] = (foupNum - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.LOTRunning_CSTHash_List.Contains(w.CST_HashCode)).FirstOrDefault();
                            if (wafer == null)
                            {
                                LoggerManager.Debug($"LoaderSupervisor UnloadRequestJob Wafer Null.   Foup Number:{foupNum} , Slot Number:{slotNum}");
                                continue;

                            }

                            wafer.UsingStageList = ActiveLotInfos[lotIdx].UsingStageIdxList.ToList();
                            wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;
                            var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                            if (realWafer != null)
                            {
                                realWafer.LOTID = ActiveLotInfos[lotIdx].LotID;
                            }

                            wafer.DeviceName.Value = ActiveLotInfos[lotIdx].DeviceName;
                            wafer.LotPriority = ActiveLotInfos[foupNum - 1].LotPriority;


                        }
                    }
                }

                LoaderMapEditor editor = new LoaderMapEditor(LoaderInfo.StateMap);
                bool isUnloadWafer = false;

                foreach (var armModule in currentMap.ARMModules)
                {
                    if (armModule.WaferStatus == EnumSubsStatus.EXIST && armModule.Substrate.ProcessingEnable == ProcessingEnableEnum.ENABLE)
                    {
                        isUnloadWafer = true;
                        editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var preModule in currentMap.PreAlignModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST && preModule.Substrate.ProcessingEnable == ProcessingEnableEnum.ENABLE)
                        {
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                        }
                    }
                }
                int avaArmCount = currentMap.GetHolderModuleAll().Count(
                                   h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);

                int avaPACount = currentMap.GetHolderModuleAll().Count(
                h => h.ID.ModuleType == ModuleTypeEnum.PA & h.WaferStatus == EnumSubsStatus.NOT_EXIST & h.Enable == true);

                if (avaArmCount == 0 || avaPACount == 0)
                {
                    map = null;
                    isAvailableModule = false;
                    LoggerManager.Debug($"[LoaderSupervisor UnloadRequestJob] AvailableModule is Not Exist. Arm Module Count:{avaArmCount} , PreAlign Module Count:{avaPACount}");
                }
                else
                {

                    if (isUnloadWafer == false)
                    {
                        foreach (var preModule in currentMap.BufferModules)
                        {
                            if (preModule.WaferStatus == EnumSubsStatus.EXIST && preModule.Substrate.ProcessingEnable == ProcessingEnableEnum.ENABLE)
                            {
                                isUnloadWafer = true;
                                editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                            }
                        }
                    }
                    if (isUnloadWafer == false)
                    {
                        foreach (var fixedTrayModule in currentMap.FixedTrayModules)
                        {
                            if (fixedTrayModule.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                if (fixedTrayModule.CanUseBuffer)
                                {
                                    isUnloadWafer = true;
                                    editor.EditorState.SetTransfer(fixedTrayModule.Substrate.ID.Value, fixedTrayModule.Substrate.OriginHolder);
                                }
                            }
                        }
                    }

                    int count = 0;
                    if (isUnloadWafer == false)
                    {
                        foreach (var chuckModule in currentMap.ChuckModules)
                        {
                            //if (chuckModule.WaferStatus == EnumSubsStatus.EXIST && count < 4)
                            //{
                            //    count++;
                            //    isUnloadWafer = true;
                            //    editor.EditorState.SetTransfer(chuckModule.Substrate.ID.Value, chuckModule.Substrate.OriginHolder);
                            //}
                            if (chuckModule.WaferStatus == EnumSubsStatus.EXIST && IsContainLot(chuckModule.ID.Index) && count < 4 && 
                                (chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT || 
                                 chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
                                 chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                            {
                                //int slotNum = chuckModule.Substrate.OriginHolder.Index % 25;
                                //int offset = 0;
                                //if (slotNum == 0)
                                //{
                                //    slotNum = 25;
                                //    offset = -1;
                                //}
                                var client = GetClient(chuckModule.ID.Index);
                                if (IsAliveClient(client))
                                {
                                    ModuleStateEnum lotState = client.GetLotState();
                                    StageLockMode lockMode = client.GetStageLock();
                                    ModuleStateEnum soakingState = client.GetSoakingModuleState();
                                    if (lockMode == StageLockMode.UNLOCK)
                                    {
                                        client.CancelLot(-1, false);
                                        if (lotState == ModuleStateEnum.PAUSED || lotState == ModuleStateEnum.IDLE)
                                        {
                                            client.SetAbort(true, true);
                                        }
                                        EnumWaferState state = client.GetWaferState();

                                        if (!(state == EnumWaferState.PROBING || state == EnumWaferState.TESTED))
                                        {
                                            if (soakingState != ModuleStateEnum.RUNNING)
                                            {
                                                count++;
                                                isUnloadWafer = true;
                                                client.WaferCancel();
                                                editor.EditorState.SetTransfer(chuckModule.Substrate.ID.Value, chuckModule.Substrate.OriginHolder);
                                            }
                                            else
                                            {
                                                isLotEnd = false;
                                            }
                                        }
                                        else
                                        {
                                            isLotEnd = false;
                                        }
                                    }
                                    else
                                    {
                                        isLockMode = true;
                                    }
                                }
                                else
                                {
                                    //isLotEnd = false 처리 하지 않고 Abort에서 confirm 로직에서 Loader Pause에서 통신 복구까지 대기 할 수 있도록 한다.
                                }
                            }
                            else
                            {
                                if (chuckModule.WaferStatus == EnumSubsStatus.UNKNOWN)
                                {
                                    isLotEnd = false;
                                    this.MetroDialogManager().ShowMessageDialog("LOT END Failed", $"Cell{chuckModule.ID.Index} Wafer Status is UNKNOWN.\nPlease update the wafer status after recovery and try again", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                    }
                }

                if (isUnloadWafer == true)
                {
                    map = editor.EditMap;
                }
                else
                {
                    IsLoaderJobDoneWait = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return map;
        }
        private bool IsContainLot(int chuckIndex)
        {
            bool retVal = false;
            try
            {
                foreach (var lotInfo in this.ActiveLotInfos)
                {
                    if (lotInfo.State == LotStateEnum.Running || lotInfo.State == LotStateEnum.Cancel)
                    {
                        retVal = lotInfo.UsingStageIdxList.Contains(chuckIndex);
                        if (retVal)
                        {
                            return retVal;
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

        public LoaderMap UnloadRequestJob(List<int> excludecells)
        {
            LoaderMap map = null;
            try
            {
                LoaderInfo = Loader.GetLoaderInfo();
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (i == (LoaderInfo.StateMap.BufferModules.Count() - 1))
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                }
                var currentMap = LoaderInfo.StateMap;

                LoaderMapEditor editor = new LoaderMapEditor(LoaderInfo.StateMap);
                bool isUnloadWafer = false;

                foreach (var armModule in currentMap.ARMModules)
                {
                    if (armModule.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        isUnloadWafer = true;
                        editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var preModule in currentMap.PreAlignModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var bufferModule in currentMap.BufferModules)
                    {
                        if (bufferModule.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(bufferModule.Substrate.ID.Value, bufferModule.Substrate.OriginHolder);
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var FixedTrayModule in currentMap.FixedTrayModules)
                    {
                        if (FixedTrayModule.WaferStatus == EnumSubsStatus.EXIST && FixedTrayModule.CanUseBuffer == true)
                        {
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(FixedTrayModule.Substrate.ID.Value, FixedTrayModule.Substrate.OriginHolder);
                        }
                    }
                }

                int count = 0;
                if (isUnloadWafer == false)
                {
                    foreach (var chuckModule in currentMap.ChuckModules)
                    {
                        if (chuckModule.WaferStatus == EnumSubsStatus.EXIST && count < 4)
                        {
                            if (excludecells != null && excludecells.Count != 0)
                            {
                                if (excludecells.Any(index => index == chuckModule.ID.Index))
                                {
                                    continue;
                                }
                            }

                            count++;
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(chuckModule.Substrate.ID.Value, chuckModule.Substrate.OriginHolder);
                        }
                    }
                }

                if (isUnloadWafer == true)
                {
                    map = editor.EditMap;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return map;
        }
        public LoaderMap UnloadRequestJob(int foupIdx, out bool isLotEnd)
        {
            LoaderMap map = null;
            isLotEnd = true;
            try
            {
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (i == (LoaderInfo.StateMap.BufferModules.Count() - 1))
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                }
                var currentMap = LoaderInfo.StateMap;

                LoaderMapEditor editor = new LoaderMapEditor(LoaderInfo.StateMap);
                bool isUnloadWafer = false;

                foreach (var armModule in currentMap.ARMModules)
                {
                    if (armModule.WaferStatus == EnumSubsStatus.EXIST && armModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        int slotNum = armModule.Substrate.OriginHolder.Index % 25;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        int foupNum = ((armModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                        if (foupNum == foupIdx)
                        {
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var preModule in currentMap.PreAlignModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST && preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int slotNum = preModule.Substrate.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            int foupNum = ((preModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                            if (foupNum == foupIdx)
                            {
                                isUnloadWafer = true;
                                editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                            }
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var preModule in currentMap.BufferModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST && preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int slotNum = preModule.Substrate.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            int foupNum = ((preModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                            if (foupNum == foupIdx)
                            {
                                isUnloadWafer = true;
                                editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                            }
                        }
                    }
                }

                int count = 0;
                if (isUnloadWafer == false)
                {
                    foreach (var chuckModule in currentMap.ChuckModules)
                    {
                        var client = GetClient(chuckModule.ID.Index);


                        if (chuckModule.WaferStatus == EnumSubsStatus.EXIST && count < 4 && chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int slotNum = chuckModule.Substrate.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            int foupNum = ((chuckModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                            if (foupNum == foupIdx)
                            {
                                if (IsAliveClient(client))
                                {
                                    EnumWaferState state = client.GetWaferState();
                                    if ((!(state == EnumWaferState.PROBING || state == EnumWaferState.TESTED)))
                                    {
                                        count++;
                                        isUnloadWafer = true;
                                        client.WaferCancel();
                                        editor.EditorState.SetTransfer(chuckModule.Substrate.ID.Value, chuckModule.Substrate.OriginHolder);
                                    }
                                }
                                isLotEnd = false;
                            }
                        }
                    }
                }

                if (isUnloadWafer == true)
                {
                    map = editor.EditMap;
                    isLotEnd = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return map;
        }
        public LoaderMap UnloadRequestJobNoBuffer(int foupIdx, out bool isLotEnd)
        {
            LoaderMap map = null;
            isLotEnd = true;
            try
            {
                List<LoaderMap> loaderMapList = new List<LoaderMap>();

                for (int i = 0; i < LoaderInfo.StateMap.BufferModules.Count(); i++)
                {
                    if (i == (LoaderInfo.StateMap.BufferModules.Count() - 1))
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                    }
                    else
                    {
                        this.LoaderInfo.StateMap.BufferModules[i].ReservationInfo.ReservationState = EnumReservationState.NOT_RESERVE;
                    }
                }
                var currentMap = LoaderInfo.StateMap;

                LoaderMapEditor editor = new LoaderMapEditor(LoaderInfo.StateMap);
                bool isUnloadWafer = false;

                foreach (var armModule in currentMap.ARMModules)
                {
                    if (armModule.WaferStatus == EnumSubsStatus.EXIST && armModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        int slotNum = armModule.Substrate.OriginHolder.Index % 25;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        int foupNum = ((armModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                        if (foupNum == foupIdx)
                        {
                            isUnloadWafer = true;
                            editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var preModule in currentMap.PreAlignModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST && preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int slotNum = preModule.Substrate.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            int foupNum = ((preModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                            if (foupNum == foupIdx)
                            {
                                isUnloadWafer = true;
                                editor.EditorState.SetTransfer(preModule.Substrate.ID.Value, preModule.Substrate.OriginHolder);
                            }
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var fixedTrayModule in currentMap.FixedTrayModules)
                    {
                        if (fixedTrayModule.WaferStatus == EnumSubsStatus.EXIST && fixedTrayModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            if (fixedTrayModule.CanUseBuffer)
                            {
                                int slotNum = fixedTrayModule.Substrate.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                int foupNum = ((fixedTrayModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                                if (foupNum == foupIdx)
                                {
                                    isUnloadWafer = true;
                                    editor.EditorState.SetTransfer(fixedTrayModule.Substrate.ID.Value, fixedTrayModule.Substrate.OriginHolder);
                                }
                            }
                        }
                    }
                }

                int count = 0;
                if (isUnloadWafer == false)
                {
                    foreach (var chuckModule in currentMap.ChuckModules)
                    {
                        var client = GetClient(chuckModule.ID.Index);


                        if (chuckModule.WaferStatus == EnumSubsStatus.EXIST && count < 2 && chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int slotNum = chuckModule.Substrate.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            int foupNum = ((chuckModule.Substrate.OriginHolder.Index + offset) / 25) + 1;
                            if (foupNum == foupIdx)
                            {
                                if (IsAliveClient(client))
                                {
                                    EnumWaferState state = client.GetWaferState();
                                    if ((!(state == EnumWaferState.PROBING || state == EnumWaferState.TESTED)))
                                    {
                                        count++;
                                        isUnloadWafer = true;
                                        client.WaferCancel();
                                        editor.EditorState.SetTransfer(chuckModule.Substrate.ID.Value, chuckModule.Substrate.OriginHolder);
                                    }
                                }
                                isLotEnd = false;
                            }
                        }
                    }
                }

                if (isUnloadWafer == true)
                {
                    map = editor.EditMap;
                    isLotEnd = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return map;
        }

        private (bool isExist, string id, ModuleID holder) SetTransferFoupShiftAvailableHolder(LoaderMap Map, HolderModuleInfo holder)
        {
            //LoaderMapEditor editor = new LoaderMapEditor(Map);
            bool isExist = false;
            ModuleID destination = holder.ID;
            try
            {
                var unloadCST = Map.IsUnloadCassetteExist(holder.Substrate);
                if (unloadCST.isExist)
                {
                    isExist = true;
                    destination = unloadCST.cst.SlotModules.FirstOrDefault(item => item.ID.Index == unloadCST.slotIdx).ID;
                    holder.Substrate.WaferState = EnumWaferState.SKIPPED;// 이렇게 해도 되나?? 
                }
                else
                {
                    var fixedtrays = Map.FixedTrayModules.Where(item => item.CanUseBuffer == true && item.WaferStatus == EnumSubsStatus.NOT_EXIST).ToList();
                    //카세트가 없을 경우  - 빈 fixed Tray ( 갈수 있는 Fixed Tray가 미리 할당되어있어야함. )
                    //                                                - 빈 Fixed Tray 가 없을 경우 null로 반환.
                    if (fixedtrays != null)
                    {
                        isExist = true;
                        destination = fixedtrays.FirstOrDefault().ID;
                        holder.Substrate.WaferState = EnumWaferState.SKIPPED;// 이렇게 해도 되나?? 
                    }
                    else
                    {
                        isExist = false;
                    }
                }

            }
            catch (Exception err)
            {
                isExist = false;
                LoggerManager.Exception(err);
            }
            return (isExist, holder.Substrate.ID.Value, destination);
        }

        public LoaderMap UnloadRequestJobFoupShiftNoBuffer(int foupIdx, out bool isLotEnd)
        {
            LoaderMap map = null;
            isLotEnd = true;
            try
            {
                List<LoaderMap> loaderMapList = new List<LoaderMap>();
                var currentMap = LoaderInfo.StateMap;
                LoaderMapEditor editor = new LoaderMapEditor(LoaderInfo.StateMap);
                bool isUnloadWafer = false;

                Action<HolderModuleInfo> SetTransfer = (HolderModuleInfo module) =>
                {
                    var unloadholder = SetTransferFoupShiftAvailableHolder(currentMap, module);
                    if (unloadholder.isExist)
                    {
                        isUnloadWafer = true;
                        editor.EditorState.SetTransfer(unloadholder.id, unloadholder.holder);
                    }
                };

                foreach (var armModule in currentMap.ARMModules)
                {
                    if (armModule.WaferStatus == EnumSubsStatus.EXIST && armModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        SetTransfer(armModule);
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var preModule in currentMap.PreAlignModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST && preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            SetTransfer(preModule);
                        }
                    }
                }

                int count = 0;
                if (isUnloadWafer == false)
                {
                    foreach (var chuckModule in currentMap.ChuckModules)
                    {
                        var client = GetClient(chuckModule.ID.Index);


                        if (chuckModule.WaferStatus == EnumSubsStatus.EXIST && count < 2 && chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            if (IsAliveClient(client))
                            {
                                EnumWaferState state = client.GetWaferState();
                                if ((!(state == EnumWaferState.PROBING || state == EnumWaferState.TESTED)))
                                {
                                    count++;
                                    isUnloadWafer = true;
                                    client.WaferCancel();
                                    SetTransfer(chuckModule);
                                }
                            }
                            isLotEnd = false;
                        }
                    }
                }

                if (isUnloadWafer == false)
                {
                    foreach (var fixedTray in currentMap.FixedTrayModules)
                    {
                        if (fixedTray.WaferStatus == EnumSubsStatus.EXIST &&
                            fixedTray.Substrate.WaferType.Value == EnumWaferType.STANDARD &&
                            fixedTray.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            fixedTray.CanUseBuffer == true)
                        {
                            var unloadCST = currentMap.IsUnloadCassetteExist(fixedTray.Substrate);

                            if (unloadCST.isExist)
                            {
                                isUnloadWafer = true;
                                var unloadHolder = unloadCST.cst.SlotModules.FirstOrDefault(item => item.ID.Index == unloadCST.slotIdx).ID;
                                editor.EditorState.SetTransfer(fixedTray.Substrate.ID.Value, unloadHolder);
                            }
                        }
                    }
                }

                if (isUnloadWafer == true)
                {
                    map = editor.EditMap;
                    isLotEnd = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// EventCodeEnum : return code value
        /// string : error message
        /// </returns>
        public async Task<(EventCodeEnum,string)> CollectAllWafer()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            string ErrorMsg = "";
            try
            {
                try
                {
                    LoaderInfo = Loader.GetLoaderInfo();
                    List<LoaderMap> loaderMapList = new List<LoaderMap>();


                    var currentMap = LoaderInfo.StateMap;

                    LoaderMapEditor editor = new LoaderMapEditor(LoaderInfo.StateMap);

                    foreach (var armModule in currentMap.ARMModules)
                    {
                        if (armModule.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (armModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT
                              || armModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY
                              || armModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                if (!CheckWafer(LoaderInfo, armModule.Substrate, out ErrorMsg))
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return (retVal, ErrorMsg);
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return (retVal, ErrorMsg);
                            }
                        }
                    }


                    foreach (var preModule in currentMap.PreAlignModules)
                    {
                        if (preModule.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT
                               || preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY
                               || preModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                if (!CheckWafer(LoaderInfo, preModule.Substrate, out ErrorMsg))
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return (retVal, ErrorMsg);
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return (retVal, ErrorMsg);
                            }
                        }
                    }


                    foreach (var buffer in currentMap.BufferModules)
                    {
                        if (buffer.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (buffer.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT
                                || buffer.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY
                                || buffer.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                if (!CheckWafer(LoaderInfo, buffer.Substrate, out ErrorMsg))
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return (retVal, ErrorMsg);
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return (retVal, ErrorMsg);
                            }
                        }
                    }

                    List<int> notMaintenanceModeCellIndexs = new List<int>();
                    foreach (var chuckModule in currentMap.ChuckModules)
                    {
                        if (chuckModule.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT
                                || chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY
                                || chuckModule.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                if (!CheckWafer(LoaderInfo, chuckModule.Substrate, out ErrorMsg))
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return (retVal, ErrorMsg);
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return (retVal, ErrorMsg);
                            }

                            var stageIdx = chuckModule.ID.Index;
                            var stageMode = loaderCommunicationManager.Cells[stageIdx - 1].StageMode;
                            if (stageMode != GPCellModeEnum.MAINTENANCE)
                            {
                                notMaintenanceModeCellIndexs.Add(stageIdx);
                            }
                        }
                    }

                    foreach (var fixedTray in currentMap.FixedTrayModules)
                    {
                        if (fixedTray.WaferStatus == EnumSubsStatus.EXIST && fixedTray.CanUseBuffer == true)
                        {
                            if (fixedTray.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT
                                || fixedTray.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                if (!CheckWafer(LoaderInfo, fixedTray.Substrate, out ErrorMsg))
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    return (retVal, ErrorMsg);
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.UNDEFINED;
                                return (retVal, ErrorMsg);
                            }
                        }
                    }

                    if (notMaintenanceModeCellIndexs.Count != 0)
                    {
                        string cellIndexstr = String.Join(", ", notMaintenanceModeCellIndexs.ToArray());

                        var messageRetVal = await this.MetroDialogManager().ShowMessageDialog("Warning Message",
                            $"Among the targets, there are cells that are not in maintenance mode.(Cell #{cellIndexstr}) \r\nDo you want to operate excluding cells that are not in maintenance mode ?", EnumMessageStyle.AffirmativeAndNegative);
                        if(messageRetVal == EnumMessageDialogResult.NEGATIVE)
                        {
                            ErrorMsg = $"The operation has been canceled because it contains cells in maintenance mode.(Cell #{cellIndexstr})";
                            retVal = EventCodeEnum.UNDEFINED;
                            return (retVal, ErrorMsg);
                        }
                    }

                    bool isContinue = true;
                    do
                    {
                        var loaderMap = UnloadRequestJob(notMaintenanceModeCellIndexs);
                        if (loaderMap == null)
                        {
                            isContinue = false;
                        }
                        if (loaderMap != null)
                        {

                            var slicedMap = MapSlicer.ManualSlicing(loaderMap);
                            if (slicedMap != null)
                            {
                                for (int i = 0; i < slicedMap.Count; i++)
                                {
                                    Loader.SetRequest(slicedMap[i]);
                                    while (true)
                                    {
                                        if (Loader.ModuleState == ModuleStateEnum.DONE)
                                        {
                                            Loader.ClearRequestData();
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                    } while (isContinue);



                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception)
            {

            }
            return (retVal, ErrorMsg);
        }

        public bool CheckWafer(LoaderInfo info, TransferObject transferobj, out string ErrorMsg)
        {
            ErrorMsg = "";

            try
            {
                if (transferobj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                {
                    int foupIdx = ((transferobj.OriginHolder.Index - 1) / 25); // foupIdx 는 0 부터 시작

                    if (foupIdx > SystemModuleCount.ModuleCnt.FoupCount)
                    {
                        ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign:Slot Index:{transferobj.OriginHolder.Index} Error";
                        return false;
                    }

                    if (info.StateMap.CassetteModules[foupIdx].FoupState == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                    {
                        if (info.StateMap.CassetteModules[foupIdx].ScanState == CassetteScanStateEnum.READ)
                        {

                            int slotIdx = (transferobj.OriginHolder.Index % 25) - 1;
                            if (transferobj.OriginHolder.Index % 25 == 0)
                            {
                                slotIdx = 24;
                            }

                            if (!(info.StateMap.CassetteModules[foupIdx].SlotModules[24 - slotIdx].WaferStatus == EnumSubsStatus.NOT_EXIST))
                            {
                                ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign:Foup{foupIdx + 1} Slot{slotIdx + 1}  WaferStatus={info.StateMap.CassetteModules[foupIdx].SlotModules[slotIdx].WaferStatus}";
                                return false;
                            }
                        }
                        else
                        {
                            ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign:Foup{foupIdx + 1} ScanState : {info.StateMap.CassetteModules[foupIdx].ScanState}";
                            return false;
                        }
                    }
                    else
                    {
                        ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign:Foup{foupIdx + 1} State : {info.StateMap.CassetteModules[foupIdx].FoupState}";
                        return false;
                    }
                }
                else if (transferobj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                {
                    if (!(info.StateMap.FixedTrayModules[transferobj.OriginHolder.Index - 1].WaferStatus == EnumSubsStatus.NOT_EXIST))
                    {
                        ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign:FIXEDTRAY{transferobj.OriginHolder.Index} WaferStatus={info.StateMap.FixedTrayModules[transferobj.OriginHolder.Index - 1].WaferStatus}";
                        return false;
                    }
                }
                else if (transferobj.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                {
                    if (!(info.StateMap.InspectionTrayModules[transferobj.OriginHolder.Index - 1].WaferStatus == EnumSubsStatus.NOT_EXIST))
                    {
                        ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign:InspectionTray{transferobj.OriginHolder.Index} WaferStatus={info.StateMap.InspectionTrayModules[transferobj.OriginHolder.Index - 1].WaferStatus}";
                        return false;
                    }
                }
                else
                {
                    ErrorMsg = $"Current:{transferobj.CurrHolder.ModuleType}{transferobj.CurrHolder.Index} Orign Type:{transferobj.OriginHolder.ModuleType} Error";
                    return false;
                }
            }
            catch (Exception)
            {

            }
            return true;
        }
        public void ReservationJob(LoaderMap Map, HolderModuleInfo buffer)  //먼저 예약된 Job을 LoaderInfo에서 제외시키기
        {
            List<ILoaderJob> jobList = new List<ILoaderJob>();

            try
            {
                var transferObjects = Map.GetTransferObjectAll();

                foreach (var dstSubObj in transferObjects)
                {
                    TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == dstSubObj.ID.Value).FirstOrDefault();
                    ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == dstSubObj.CurrHolder).FirstOrDefault(); //chuck

                    if (subObj == null)
                    {
                        //editor.reasonoferror = $"[loader] loadermapeditorstate.settransferfunc() : can not found substrate object. substrateid={transferobjid}";
                        continue;
                    }

                    if (dstLoc == null)
                    {
                        //editor.reasonoferror = $"[loader] loadermapeditorstate.settransferfunc() : can not found destination module. destination={destination}";
                        continue;
                    }

                    if (subObj.PrevHolder == dstLoc.ID)
                    {
                        continue;
                    }

                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(dstSubObj.ID.Value);

                    if (currSubObj == null)
                    {
                        continue;
                    }

                    IWaferLocatable curModule = Loader.ModuleManager.FindModule(subObj.PrevHolder) as IWaferLocatable;
                    IWaferLocatable dstModule = Loader.ModuleManager.FindModule(dstSubObj.CurrHolder) as IWaferLocatable;

                    if (curModule is IWaferSupplyModule)
                    {
                        var mapBuffer = Map.BufferModules.Where(i => i.ID.Index == buffer.ID.Index).FirstOrDefault();

                        if (mapBuffer != null)
                        {
                            mapBuffer.ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                            buffer.ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                        }

                        //  buffer.WaferStatus = EnumSubsStatus.EXIST;
                        //if (curModule is IFixedTrayModule)
                        //{
                        //    var fixedModule = this.LoaderInfo.StateMap.FixedTrayModules.FirstOrDefault(m => m.Substrate.CurrPos == currSubObj.CurrPos);
                        //    if (fixedModule != null)
                        //        fixedModule.WaferStatus = EnumSubsStatus.NOT_EXIST;
                        //}
                        //else
                        //{
                        //    foreach (var cassetteModule in this.LoaderInfo.StateMap.CassetteModules)
                        //    {
                        //        var slotModule = cassetteModule.SlotModules.FirstOrDefault(m => m.ID == currSubObj.CurrPos);
                        //        if (slotModule != null)
                        //            slotModule.WaferStatus = EnumSubsStatus.NOT_EXIST;
                        //    }
                        //}
                    }
                    else
                    {
                        if (curModule is IChuckModule)
                        {
                            var mapBuffer = Map.BufferModules.Where(i => i.ID.Index == buffer.ID.Index).FirstOrDefault();

                            if (mapBuffer != null)
                            {
                                mapBuffer.ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                                buffer.ReservationInfo.ReservationState = EnumReservationState.RESERVE;
                            }
                            // buffer.WaferStatus = EnumSubsStatus.EXIST;
                            //var chuck = this.LoaderInfo.StateMap.ChuckModules.Where(i => i.Substrate != null).ToList().FirstOrDefault(m => m.Substrate.CurrPos == currSubObj.CurrPos);
                            //if (chuck != null)
                            //{
                            //    chuck.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            //}
                            break;
                        }
                    }
                }
                // }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public LoaderMap PreLoadingJobMake(LoaderMap Map, string DeviceName = null, int cellIdx = 0)
        {
            TransferObject loadWafer = FindLoadWafer(Map, DeviceName, cellIdx);

            if (loadWafer == null)
            {
                return null;
            }
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            var usingBuffer = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index != Map.BufferModules.Count() && i.Enable);
            if (usingBuffer == null)
            {
                return null;
            }
            loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
            editor.EditorState.SetTransfer(loadWafer.ID.Value, usingBuffer.ID);
            return editor.EditMap;
        }
        public LoaderMap PreLoadingJobMakeFullBuffer(LoaderMap Map, string DeviceName = null, int cellIdx = 0)
        {
            TransferObject loadWafer = FindLoadWafer(Map, DeviceName, cellIdx);

            if (loadWafer == null)
            {
                return null;
            }
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            var usingBuffer = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.Enable);
            if (usingBuffer == null)
            {
                return null;
            }
            loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
            editor.EditorState.SetTransfer(loadWafer.ID.Value, usingBuffer.ID);
            return editor.EditMap;
        }
        public LoaderMap PreLoadingJobMake_BufferCount_Minus_ArmCount(LoaderMap Map, string DeviceName = null, int cellIdx = 0)
        {
            TransferObject loadWafer = FindLoadWafer(Map, DeviceName, cellIdx);

            if (loadWafer == null)
            {
                return null;
            }
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            var usingBufferCount = Map.BufferModules.Count(i => i.Substrate == null && i.Enable);
            if (usingBufferCount <= Map.ARMModules.Count())
            {
                HolderModuleInfo usingArm = null;
                var usedBufferCount = Map.BufferModules.Count(i => i.Substrate != null && i.Enable);
                if ((usedBufferCount % Map.ARMModules.Count()) == 0)
                {
                    usingArm = Map.ARMModules.FirstOrDefault(i => i.Substrate == null && i.Enable);
                }
                else
                {
                    //buffer개수가 홀수 인경우 arm2 사용시점에 usingBufferCount <= Map.ARMModules.Count() 가 true가 발생한다.
                    //이때 arm1을 target으로 지정할 경우 mapslicing 에러가 발생한다.(pa2에서 arm1으로 빼려고 하나 이때 arm1은 buffer에 넣을 wafer를 가지고 있는 상태임
                    usingArm = Map.ARMModules.LastOrDefault(i => i.Substrate == null && i.Enable);
                }

                if (usingArm == null)
                {
                    return null;
                }
                loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
                editor.EditorState.SetTransfer(loadWafer.ID.Value, usingArm.ID);
            }
            else
            {
                var usingBuffer = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.Enable);
                if (usingBuffer == null)
                {
                    return null;
                }
                loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
                editor.EditorState.SetTransfer(loadWafer.ID.Value, usingBuffer.ID);
            }
            return editor.EditMap;
        }
        public LoaderMap LoadWafer_In_Arm(LoaderMap Map, string DeviceName = null, int cellIdx = 0)
        {
            TransferObject loadWafer = Map.ARMModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST).Substrate;

            if (loadWafer == null)
            {
                return null;
            }
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            var usingStage = Map.ChuckModules.FirstOrDefault(i => i.ID.Index == cellIdx && i.Substrate == null);
            if (usingStage == null)
            {
                return null;
            }
            loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
            editor.EditorState.SetTransfer(loadWafer.ID.Value, usingStage.ID);
            return editor.EditMap;
        }
        public LoaderMap PreLoadingJobMakeNoBuffer(LoaderMap Map, bool isNeedWafer, string DeviceName = null, int cellIdx = 0)
        {
            if (Map.PreAlignModules.Length <= 1 && !isNeedWafer)
            {
                return null;
            }

            TransferObject loadWafer = FindLoadWafer(Map, DeviceName, cellIdx);
            if (loadWafer == null)
            {
                return null;
            }
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            var usingFixedTray = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate == null && i.Enable && i.CanUseBuffer);
            if (usingFixedTray == null)
            {
                OCRModuleInfo usingOCR = null;
                var existPA = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.Enable);
                if (existPA != null)
                {
                    usingOCR = Map.CognexOCRModules.FirstOrDefault(i => i.Substrate == null && i.Enable && i.ID.Index != existPA.ID.Index);
                    if (usingOCR == null)
                    {
                        return null;
                    }
                }
                else
                {
                    usingOCR = Map.CognexOCRModules.FirstOrDefault(i => i.Substrate == null && i.Enable);
                    if (usingOCR == null)
                    {
                        return null;
                    }
                    else
                    {
                        OCRModuleInfo existUsingOCR = Map.CognexOCRModules.FirstOrDefault(i => i.Substrate != null && i.Enable);
                        if (existUsingOCR != null)
                        {
                            return null;
                        }
                    }
                }
                loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
                editor.EditorState.SetTransfer(loadWafer.ID.Value, usingOCR.ID);
            }
            else
            {
                loadWafer.SetReservationState(ReservationStateEnum.RESERVATION);
                editor.EditorState.SetTransfer(loadWafer.ID.Value, usingFixedTray.ID);
            }

            return editor.EditMap;
        }
        public LoaderMap IsExistLoaderArm(LoaderMap Map, out bool isExist)
        {

            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;
            var armModule = Map.ARMModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST);
            if (armModule == null)
            {
                isExist = false;
                return null;
            }
            else
            {
                isExist = true;
                if (armModule.Substrate.OCRReadState == OCRReadStateEnum.DONE)
                {
                    if (armModule.Substrate.WaferState == EnumWaferState.PROCESSED)
                    {
                        armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                        editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                    }
                    else
                    {
                        var buffCnt = Loader.GetLoaderInfo().StateMap.BufferModules?.Count();

                        bool isEnableBuffer = false;
                        if (buffCnt > 0)
                        {
                            for (int i = 0; i < Loader.GetLoaderInfo().StateMap.BufferModules.Count(); i++)
                            {
                                isEnableBuffer = Loader.GetLoaderInfo().StateMap.BufferModules[i].Enable;
                                if (isEnableBuffer == true)
                                {
                                    break;
                                }
                            }
                        }
                        if (isEnableBuffer == false)
                        {
                            var PreAlignModules = Map.PreAlignModules.FirstOrDefault(i => i.Substrate == null);
                            if (PreAlignModules == null)
                            {
                                armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                                editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                            }
                            else
                            {
                                armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                                editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, PreAlignModules.ID);
                            }
                        }
                        else
                        {
                            var usingBuffer = Map.BufferModules.FirstOrDefault(i => i.Substrate == null);
                            if (usingBuffer == null)
                            {
                                isExist = false;
                                return null;
                            }
                            armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                            editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, usingBuffer.ID);
                        }
                    }
                }
                else
                {
                    armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                    editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                }
            }
            return editor.EditMap;
        }
        public LoaderMap IsExistLoaderPA(LoaderMap Map, out bool isExist)
        {

            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;
            var PAModule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST);
            if (PAModule == null)
            {
                isExist = false;
                return null;
            }
            else
            {
                isExist = true;
                if (PAModule.Substrate.OCRReadState == OCRReadStateEnum.DONE)
                {

                    if (PAModule.Substrate.WaferState == EnumWaferState.PROCESSED)
                    {
                        PAModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                        editor.EditorState.SetTransfer(PAModule.Substrate.ID.Value, PAModule.Substrate.OriginHolder);
                    }
                    else
                    {
                        var usingBuffer = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index != 5 && i.Enable == true);
                        if (usingBuffer == null)
                        {
                            isExist = false;
                            return null;
                        }

                        PAModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                        editor.EditorState.SetTransfer(PAModule.Substrate.ID.Value, usingBuffer.ID);
                    }

                }
                else
                {
                    PAModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                    editor.EditorState.SetTransfer(PAModule.Substrate.ID.Value, PAModule.Substrate.OriginHolder);
                }
            }
            return editor.EditMap;
        }

        /// <summary>
        /// UnloadRequestJob이 수행되고 나서 Lot에 할당된 Wafer들이 Foup으로 모두 들어 왔는지 확인 하기 위한 로직.
        /// Lot Start 되고나서 할당된 SLOT 상태랑 비교 하기.
        /// </summary>
        /// <param name="Map"></param>
        /// <param name="cst_hascode"></param>
        /// <returns></returns>
        public EventCodeEnum ConfirmWaferArrivalInFoup(LoaderMap Map, ActiveLotInfo activelotinfo)

        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                var allwafer = Map.GetTransferObjectAll();
                var onchuck_wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.CST_HashCode == activelotinfo.CST_HashCode && w.CurrHolder.ModuleType != ModuleTypeEnum.SLOT);
                if (onchuck_wafer.Count() > 0)
                {
                    //Foup Unload 허용 하지 않음
                    List<string> waferpos = new List<string>();
                    foreach (var wafer in onchuck_wafer)
                    {
                        waferpos.Add(wafer.CurrHolder.Label);
                    }
                    retval = EventCodeEnum.LOT_ASSIGNED_WAFER_REMAIN;
                    this.NotifyManager().Notify(retval);
                    this.MetroDialogManager().ShowMessageDialog($"[Error] Lot #{activelotinfo.FoupNumber} End Failure", $"Allocated wafers still remain in the system. \n" +
                        $"Location : {string.Join(",", waferpos)}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    retval = EventCodeEnum.NONE; //Foup Unload 허용 함
                    if (DynamicMode == DynamicModeEnum.NORMAL && GetFoupShiftMode() == FoupShiftModeEnum.NORMAL && SystemManager.SystemType != SystemTypeEnum.DRAX)//normal lot
                    {
                        List<int> selecedSlot = ActiveLotInfos[activelotinfo.FoupNumber - 1].UsingSlotList;
                        var slotList = selecedSlot.ToList();

                        for (int i = 0; i < selecedSlot.Count; i++)
                        {
                            slotList[i] = (activelotinfo.FoupNumber - 1) * 25 + selecedSlot[i];
                            var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT && w.OriginHolder.Index == slotList[i] && w.CST_HashCode == activelotinfo.CST_HashCode).FirstOrDefault();
                            if (wafer == null)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.LOT_ASSIGNED_WAFER_MISSMATCH);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public LoaderMap IsExistLoaderArmNoBuffer(LoaderMap Map, out bool isExist)
        {
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;
            var armModule = Map.ARMModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST);
            if (armModule == null)
            {
                isExist = false;
                return null;
            }
            else
            {
                isExist = true;
                if (armModule.Substrate.OCRReadState == OCRReadStateEnum.DONE)
                {
                    if (armModule.Substrate.WaferState == EnumWaferState.PROCESSED)
                    {
                        armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                        editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                    }
                    else
                    {
                        var usingPA = Map.PreAlignModules.Where(i => i.Substrate == null);
                        if (usingPA.Count() <= 1)
                        {
                            armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                            editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                        }
                        else
                        {
                            armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                            editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, usingPA.FirstOrDefault().ID);
                        }
                    }
                }
                else
                {
                    armModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                    editor.EditorState.SetTransfer(armModule.Substrate.ID.Value, armModule.Substrate.OriginHolder);
                }
            }
            return editor.EditMap;
        }

        private bool IsAvailableUnloadToCassette(TransferObject wafer)
        {
            // wafer.CST_HashCode;
            return true;
        }

        /// <summary>
        /// UnloadRequestJobNoBuffer()으로 Wafer을 정리 하는 동안
        /// Cell의 Wafer Skipped 상태로 바꾸지 않아 Resume시 Probing으로 들어 갔었음.
        /// Wafer 상태를 먼저 Skipped로 변경 하기 위한 함수
        /// </summary>
        /// <param name="lotinfo"></param>
        public void SetSkipWaferOnChuck(ActiveLotInfo lotinfo)
        {
            try
            {
                foreach(var stage in lotinfo.AssignedUsingStageIdxList)
                {
                    var client = GetClient(stage);
                    if (IsAliveClient(client))
                    {
                        EnumWaferState state = client.GetWaferState();
                        if ((!(state == EnumWaferState.PROBING || state == EnumWaferState.TESTED)))
                        {
                            client.SetWaferStateOnChuck(EnumWaferState.SKIPPED);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetSkipUnprocessedWafer(LoaderMap map, ActiveLotInfo lotinfo)//TODO: 나중에 External Requestjob에서 통일 고려
        {
            try
            {
                var allwafer = map.GetTransferObjectAll();
                //#PAbort Cancel되었을 경우 Unprocessed wafer는 Skip 처리해야함.
                List<int> selecedSlot = lotinfo.UsingSlotList;

                if (lotinfo.State != LotStateEnum.Idle)
                { //idle이 아닐때는 이미 Wafer에 Cst_HashCode 설정되어있다고 판단하여 해당 Lot정보에 대한 Slot이 Unprocessed이면 Skip
                    for (int i = 0; i < selecedSlot.Count; i++)
                    {
                        var wafer = allwafer.Where(w => w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                        //lotinfo.FoupNumber == ((w.OriginHolder.Index - 1) / 25) + 1 &&
                                                        lotinfo.CST_HashCode == w.CST_HashCode &&
                                                        selecedSlot[i] == ((w.OriginHolder.Index % 25 == 0) ? 25 : w.OriginHolder.Index % 25)
                                                        ).FirstOrDefault();
                        if (wafer == null)
                        {
                            continue;
                        }
                        wafer.LotPriority = lotinfo.LotPriority;
                        wafer.ProcessingEnable = ProcessingEnableEnum.ENABLE;

                        var realWafer = this.Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                        if (realWafer != null)
                        {
                            if (realWafer.WaferState == EnumWaferState.UNPROCESSED)
                            {
                                realWafer.WaferState = EnumWaferState.SKIPPED;
                                wafer.WaferState = EnumWaferState.SKIPPED;
                            }
                        }
                    }
                }


                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public LoaderMap IsExistLoaderPA_Normal(LoaderMap Map, out bool isExist)
        {

            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;
            var PAModule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST);
            if (PAModule == null)
            {
                isExist = false;
                return null;
            }
            else
            {
                PAModule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.Substrate.OCRReadState != OCRReadStateEnum.ABORT);
                if (PAModule == null)
                {
                    isExist = false;
                    return null;
                }
                else
                {

                    if (PAModule.Substrate.WaferState == EnumWaferState.PROCESSED)
                    {
                        PAModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                        editor.EditorState.SetTransfer(PAModule.Substrate.ID.Value, PAModule.Substrate.OriginHolder);
                    }
                    else
                    {
                        var usingBuffer = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index != 5 && i.Enable == true);
                        if (usingBuffer == null)
                        {
                            isExist = false;
                            return null;
                        }

                        PAModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                        editor.EditorState.SetTransfer(PAModule.Substrate.ID.Value, usingBuffer.ID);
                    }
                    isExist = true;
                }

            }
            return editor.EditMap;
        }

        public LoaderMap IsLoaderMapByState(LoaderMap Map, out bool isLoaderMapExist)
        {
            LoaderMap loaderMap = null;

            // Check 할 우선 순위별로 나열

            // - OCR Abort Wafer Check
            loaderMap = IsOCRAbortNoBuffer(LoaderInfo.StateMap, out isLoaderMapExist);
            if (isLoaderMapExist)
            {
                return loaderMap;
            }

            // - Fixed Tray Exist Check 
            loaderMap = IsFixedTrayExist(LoaderInfo.StateMap, out isLoaderMapExist);
            if (isLoaderMapExist)
            {
                return loaderMap;
            }

            return loaderMap;
        }

        public LoaderMap IsFixedTrayExist(LoaderMap Map, out bool isExist)
        {
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;

            //[FOUP_SHIFT]*
            // Parameter 추가 및 Parameter를 보고 Fixed Tray 여분 확인하는 Code 추가

            if (Map.PreAlignModules.Count(i => i.WaferStatus == EnumSubsStatus.NOT_EXIST) == 0)// 사용할수 있는 PA가 없으면 로더맵 null로 반환 
            {
                isExist = false;
                return null;
            }

            var existFixedModules = Map.FixedTrayModules.Where(i => i.Substrate != null &&
                                                           i.WaferStatus == EnumSubsStatus.EXIST &&
                                                           i.CanUseBuffer == true &&
                                                           i.Substrate.WaferType.Value == EnumWaferType.STANDARD &&
                                                           ((i.Substrate.OCRReadState == OCRReadStateEnum.ABORT) || (i.Substrate.WaferState != EnumWaferState.UNPROCESSED))
                                                           );
            if (existFixedModules.Count() > 0)
            {
                foreach (var fixedModule in existFixedModules)
                {
                    // FixedTray에 있는 웨이퍼는 같은 HashCode인 Wafer가 모두 UnProceesed가 아닐때 언로드 할수 있다. 

                    string Cst_HashCode = fixedModule.Substrate.CST_HashCode;

                    ActiveLotInfo originLotInfo = null;
                    originLotInfo = ActiveLotInfos.Find(l => l.CST_HashCode == Cst_HashCode);
                    if (originLotInfo == null)
                    {
                        originLotInfo = Prev_ActiveLotInfos.Find(l => l.CST_HashCode == Cst_HashCode);
                    }


                    if (originLotInfo != null)
                    {
                        // 같이 로드됐던 웨이퍼가 있으면 해당 위치로 반환. 없으면 
                        var allocatedSlots = Map.GetTransferObjectAll().Where(w => w.CST_HashCode == originLotInfo.CST_HashCode &&
                                                                                   originLotInfo.UsingSlotList.Contains((w.OriginHolder.Index % 25 == 0) ? 25 : w.OriginHolder.Index % 25));
                        var endSlots = allocatedSlots.Where(s => s.WaferState != EnumWaferState.UNPROCESSED);

                        var unloadCST = Map.IsUnloadCassetteExist(fixedModule.Substrate);


                        if (allocatedSlots.Count() == endSlots.Count()) //|| unloadCST.isInCstAlready)
                        {
                            if (unloadCST.isExist)
                            {
                                isExist = true;
                                fixedModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);

                                ModuleID moduleID = new ModuleID();
                                if (LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.SHIFT)
                                {
                                    moduleID = unloadCST.cst.SlotModules.FirstOrDefault(item => item.ID.Index == unloadCST.slotIdx).ID;
                                }
                                else
                                {
                                    moduleID = fixedModule.Substrate.OriginHolder;
                                }

                                editor.EditorState.SetTransfer(fixedModule.Substrate.ID.Value, moduleID);
                            }

                            continue;
                        }

                    }
                    else
                    {
                        LoggerManager.Debug($"IsFixedTrayExist(): Missed Wafer, OriginHolder.Index:{fixedModule.Substrate.OriginHolder.Index}");
                    }
                }
            }
            else
            {
                isExist = false;
                return null;
            }

            return editor.EditMap;
        }

        public LoaderMap IsOCRAbort(LoaderMap Map, out bool isExist)
        {
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;
            var buffers = Map.BufferModules.Where(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST && ((i.Substrate.OCRReadState == OCRReadStateEnum.ABORT) || (i.Substrate.WaferState == EnumWaferState.SKIPPED)));
            if (buffers.Count() > 0)
            {
                isExist = true;
                var buffer = buffers.First();
                buffer.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                buffer.Substrate.WaferState = EnumWaferState.SKIPPED;
                editor.EditorState.SetTransfer(buffer.Substrate.ID.Value, buffer.Substrate.OriginHolder);
            }
            else
            {
                isExist = false;
                return null;
            }
            return editor.EditMap;
        }

        public LoaderMap IsOCRAbortNoBuffer(LoaderMap Map, out bool isExist)
        {
            LoaderMapEditor editor = new LoaderMapEditor(Map);
            isExist = false;
            var paModules = Map.PreAlignModules.Where(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST && ((i.Substrate.OCRReadState == OCRReadStateEnum.ABORT) || (i.Substrate.WaferState == EnumWaferState.SKIPPED)));

            var fixedModules = Map.FixedTrayModules.Where(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.EXIST &&
                                                                                         i.CanUseBuffer == true && i.Substrate.WaferType.Value == EnumWaferType.STANDARD &&
                                                                                         ((i.Substrate.OCRReadState == OCRReadStateEnum.ABORT) || (i.Substrate.WaferState == EnumWaferState.SKIPPED)));

            if (paModules.Count() > 0)
            {
                isExist = true;
                var pa = paModules.First();

                ModuleID destination = pa.Substrate.OriginHolder;
                if (LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.NORMAL)
                {
                    destination = pa.Substrate.OriginHolder;
                }
                else // if (FoupShiftMode.Value == FoupShiftModeEnum.SHIFT) //[FOUP_SHIFT]*
                {
                    bool isAllWaferDone = true;
                    var pairWafers = Map.GetTransferObjectAll().Where(x => x.CST_HashCode == pa.Substrate.CST_HashCode &&
                                                                        x.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                                                                        x.ID != pa.Substrate.ID);
                    if (pairWafers.Count() != pairWafers.Where(p => p.WaferState != EnumWaferState.UNPROCESSED).Count())
                    {
                        isAllWaferDone = false;
                    }

                    // Wafer를 Unloader할 Cassette가 있는지 확인하는 함수
                    var activeFoupList = ActiveLotInfos.FindAll(x => x.State == LotStateEnum.Running).Select(x => x.FoupNumber).ToList();
                    var unloadCST = Map.IsUnloadCassetteExist(pa.Substrate);//activeFoupList

                    if ((isAllWaferDone) && unloadCST.isExist)
                    {
                        destination = unloadCST.cst.SlotModules.FirstOrDefault(item => item.WaferStatus == EnumSubsStatus.NOT_EXIST).ID;
                    }
                    else
                    {
                        // Add 3. Fixed Tray에 자리가 있는지, 자리가 있다면 몇 번 Tray가 비어있는지 반환하는 함수
                        var fixedtrays = Map.FixedTrayModules.Where(item => item.CanUseBuffer == true && item.WaferStatus == EnumSubsStatus.NOT_EXIST).ToList();
                        //카세트가 없을 경우  - 빈 fixed Tray ( 갈수 있는 Fixed Tray가 미리 할당되어있어야함. )
                        //                                                - 빈 Fixed Tray 가 없을 경우 null로 반환.
                        if ((fixedtrays != null) && (fixedtrays.Count > 0))
                        {
                            destination = fixedtrays.FirstOrDefault().ID;
                        }
                        else
                        {
                            isExist = false;
                            return null;
                        }
                    }
                }

                pa.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                pa.Substrate.WaferState = EnumWaferState.SKIPPED;
                editor.EditorState.SetTransfer(pa.Substrate.ID.Value, destination);
            }
            else if (fixedModules.Count() > 0) 
            {
                if (LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.NORMAL)
                {
                    isExist = true;
                    var fixedModule = fixedModules.First();
                    fixedModule.Substrate.SetReservationState(ReservationStateEnum.RESERVATION);
                    fixedModule.Substrate.WaferState = EnumWaferState.SKIPPED;
                    editor.EditorState.SetTransfer(fixedModule.Substrate.ID.Value, fixedModule.Substrate.OriginHolder);
                }
                else 
                {
                    isExist = false;
                    return null;
                }
            }
            else
            {
                isExist = false;
                return null;
            }
            return editor.EditMap;
        }

        protected TransferObject FindLoadWafer(LoaderMap Map, string DeviceName = null, int cellIdx = 0)
        {
            TransferObject loadWafer = null;

            try
            {
                var allWafers = Map.GetTransferObjectAll();
                DeviceName = null;
                if (DeviceName != null && cellIdx != 0)
                {
                    var loadableWafers = Map.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                            item.DeviceName.Value == DeviceName &&
                            item.UsingStageList.Contains(cellIdx)).ToList();

                    loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();
                    if (loadWafer == null)
                    {
                        loadableWafers = Map.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                        loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();
                    }
                }
                else if (DeviceName == null && cellIdx != 0)
                {
                    var loadableWafers = Map.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                             (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                            item.UsingStageList.Contains(cellIdx)).ToList(); // cellIdx 확인 필수 

                    loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();

                    //loadswafer가 null일경우 cellidx와 무관하게 다시 계산함 (이 경우 buffer를 다 채울 수 있게된다. pw를 위해 1개는 제외된 상태)
                    if (loadWafer == null)
                    {
                        loadableWafers = Map.GetTransferObjectAll().Where(
                            item =>
                            item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                            item.WaferState == EnumWaferState.UNPROCESSED &&
                            item.ReservationState == ReservationStateEnum.NONE &&
                            item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();

                        loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();
                    }
                }
                else
                {
                    var loadableWafers = Map.GetTransferObjectAll().Where(
                           item =>
                           item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                           item.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                            (item.WaferType.Value == EnumWaferType.STANDARD || item.WaferType.Value == EnumWaferType.TCW) &&
                           item.WaferState == EnumWaferState.UNPROCESSED &&
                           item.ReservationState == ReservationStateEnum.NONE &&
                           item.ProcessingEnable == ProcessingEnableEnum.ENABLE).ToList();
                    loadWafer = loadableWafers.OrderBy(item => item.LotPriority).ThenBy(item => item.OriginHolder.Index).FirstOrDefault();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return loadWafer;
        }

        private void DoWork()
        {
            try
            {
                while (isStopThreadReq == false)
                {
                    var hash = this.GetHashCode();
                    Execute();

                    UpdateGetWaferObject();

                    for (int i = 0; i < 200; i++)
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void StatusInfoUpdateThreadProc()
        {
            try
            {
                while (StatusSoakingUpdateInfoStop == false)
                {
                    UpdateGetStatusSoakingInfo();

                    for (int i = 0; i < 1000; i++)
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void StageWatchDogThreadProc()
        {
            try
            {
                while (StageWatchDogStop == false)
                {
                    Task t = StageWatchDog();
                    t.Wait();
                    Thread.Sleep(1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        internal bool IsCanLotStart()
        {
            //랏드 시작 조건
            return true;
        }
        public EventCodeEnum AwakeProcessModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.AwakeProcessModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {
                RetVal = StateObj.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
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
                throw;
            }
        }

        public ModuleStateEnum Execute()
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;

            try
            {
                StateObj.Execute();
                ModuleState.StateTransition(StateObj.ModuleState);
                ModuleState.State = StateObj.ModuleState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //this.MonitoringManager().StageEmergencyStop();
            }

            return stat;
        }
        public bool IsLotEndReady()
        {
            bool retVal = false;

            try
            {
                foreach (var client in Clients)
                {
                    retVal = LotOPEndReadyClient(client.Value);
                    if (retVal == false)
                    {
                        break;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ExternalLotOPResume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            foreach (var lotInfo in ActiveLotInfos)
            {
                if (lotInfo.State == LotStateEnum.Running)
                {
                    if (CellsInfo.Count > 0)
                    {
                        for (int i = priorityNum; i < lotInfo.UsingStageIdxList.Count; i++)
                        {
                            var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Index == lotInfo.UsingStageIdxList[i]);


                            if (cellInfo.StageInfo.IsChecked &&
                                loaderCommunicationManager.Cells != null &&
                                lotInfo.UsingStageIdxList[i] - 1 >= 0 &&
                                loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].StageMode == GPCellModeEnum.ONLINE &&
                                cellInfo.StageInfo.LotData.LotAbortedByUser != true)
                            {
                                if (loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].LockMode == StageLockMode.UNLOCK)
                                {
                                    if (cellInfo.StageState == ModuleStateEnum.PAUSED || cellInfo.StageState == ModuleStateEnum.PAUSING)
                                    {
                                        var client = GetClient(lotInfo.UsingStageIdxList[i]);  
                                        var isSucess = LotOPResumeClient(client);
                                        isLotStartFlag[lotInfo.UsingStageIdxList[i] - 1] = isSucess;
                                    }
                                }
                                else
                                {
                                    GPLoader.LoaderBuzzer(true);
                                    this.MetroDialogManager().ShowMessageDialog("Cell LOT Resume Failed", $"Cell{cellInfo.StageInfo.Index} BackSide Door is Opened.", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                isLotStartFlag[lotInfo.UsingStageIdxList[i] - 1] = false;
                            }
                        }
                    }
                    else
                    {
                        for (int i = priorityNum; i < lotInfo.UsingStageIdxList.Count; i++)
                        {
                            if (loaderCommunicationManager.Cells != null && lotInfo.UsingStageIdxList[i] - 1 >= 0 && loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].StageMode == GPCellModeEnum.ONLINE)
                            {
                                if (loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].LockMode == StageLockMode.UNLOCK)
                                {
                                    var client = GetClient(lotInfo.UsingStageIdxList[i]);  
                                    var isSucess = LotOPResumeClient(client);
                                    isLotStartFlag[lotInfo.UsingStageIdxList[i] - 1] = isSucess;
                                }
                                else
                                {
                                    GPLoader.LoaderBuzzer(true);
                                    this.MetroDialogManager().ShowMessageDialog("Cell LOT Resume Failed", $"Cell{loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].StageInfo.Index} BackSide Door is Opened.", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }
        public EventCodeEnum ExternalLotOPPause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            foreach (var lotInfo in ActiveLotInfos)
            {
                if (lotInfo.State == LotStateEnum.Running)
                {
                    if (CellsInfo.Count > 0)
                    {
                        bool lotPauseWithCell = true;
                        var lotOpPauseParam = CommandRecvSlot.Token.Parameter as GPLotOpPauseParam;

                        if (lotOpPauseParam != null)
                        {
                            lotPauseWithCell = lotOpPauseParam.LotOpPauseWithCell;
                        }

                        if (lotPauseWithCell)
                        {
                            for (int i = priorityNum; i < lotInfo.UsingStageIdxList.Count; i++)
                            {
                                var cellInfo = CellsInfo.ToList<IStageObject>().Find(cell => cell.Index == lotInfo.UsingStageIdxList[i]);

                                if (cellInfo.StageInfo.IsChecked && loaderCommunicationManager.Cells != null && lotInfo.UsingStageIdxList[i] - 1 >= 0 && loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].StageMode == GPCellModeEnum.ONLINE)
                                {
                                    if (cellInfo.StageState == ModuleStateEnum.RUNNING ||
                                        cellInfo.StageState == ModuleStateEnum.RESUMMING)
                                    {
                                        var client = GetClient(lotInfo.UsingStageIdxList[i]);
                                        var isSucess = LotOPPauseClient(client);
                                        isLotStartFlag[lotInfo.UsingStageIdxList[i] - 1] = isSucess;
                                    }
                                }
                                else
                                {
                                    isLotStartFlag[lotInfo.UsingStageIdxList[i] - 1] = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = priorityNum; i < lotInfo.UsingStageIdxList.Count; i++)
                        {
                            if (loaderCommunicationManager.Cells != null && lotInfo.UsingStageIdxList[i] - 1 >= 0 && loaderCommunicationManager.Cells[lotInfo.UsingStageIdxList[i] - 1].StageMode == GPCellModeEnum.ONLINE)
                            {
                                var client = GetClient(lotInfo.UsingStageIdxList[i]);  
                                var isSucess = LotOPPauseClient(client);
                                isLotStartFlag[lotInfo.UsingStageIdxList[i] - 1] = isSucess;
                            }
                        }
                    }
                }
            }
            return retVal;
        }
        public bool ExtenalIsLotEndReady()
        {
            bool retVal = true;

            try
            {
                foreach (var LotInfo in ActiveLotInfos)
                {
                    foreach (var stageidx in LotInfo.UsingStageIdxList)
                    {
                        var stage = GetClient(stageidx);
                        if (stage != null)
                        {
                            retVal = LotOPEndReadyClient(stage);
                            if (retVal == false)
                            {
                                break;
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

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {

            return StateObj.ClearRequestData();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {

            try
            {
                isStopThreadReq = true;

                this.E84Module().DeInitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private IOPortDescripter<bool> _MAINVAC;
        public IOPortDescripter<bool> MAINVAC
        {
            get { return _MAINVAC; }
            set
            {
                if (value != _MAINVAC)
                {
                    _MAINVAC = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _MAINAIR;
        public IOPortDescripter<bool> MAINAIR
        {
            get { return _MAINAIR; }
            set
            {
                if (value != _MAINAIR)
                {
                    _MAINAIR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _MAINVAC_STAGE;
        public IOPortDescripter<bool> MAINVAC_STAGE
        {
            get { return _MAINVAC_STAGE; }
            set
            {
                if (value != _MAINVAC_STAGE)
                {
                    _MAINVAC_STAGE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _MAINAIR_STAGE;
        public IOPortDescripter<bool> MAINAIR_STAGE
        {
            get { return _MAINAIR_STAGE; }
            set
            {
                if (value != _MAINAIR_STAGE)
                {
                    _MAINAIR_STAGE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _EMGIO;
        public IOPortDescripter<bool> EMGIO
        {
            get { return _EMGIO; }
            set
            {
                if (value != _EMGIO)
                {
                    _EMGIO = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _LOADER_RIGHTDOOR_OPEN;

        public IOPortDescripter<bool> LOADER_RIGHTDOOR_OPEN
        {
            get { return _LOADER_RIGHTDOOR_OPEN; }
            set
            {
                if (value != _LOADER_RIGHTDOOR_OPEN)
                {
                    _LOADER_RIGHTDOOR_OPEN = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _LOADER_LEFTDOOR_OPEN;
        public IOPortDescripter<bool> LOADER_LEFTDOOR_OPEN
        {
            get { return _LOADER_LEFTDOOR_OPEN; }
            set
            {
                if (value != _LOADER_LEFTDOOR_OPEN)
                {
                    _LOADER_LEFTDOOR_OPEN = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _LD_PCW_LEAK_STATE;
        public IOPortDescripter<bool> LD_PCW_LEAK_STATE
        {
            get { return _LD_PCW_LEAK_STATE; }
            set
            {
                if (value != _LD_PCW_LEAK_STATE)
                {
                    _LD_PCW_LEAK_STATE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _LD_WAFER_OUT_SEONSOR;
        public IOPortDescripter<bool> LD_WAFER_OUT_SEONSOR
        {
            get { return _LD_WAFER_OUT_SEONSOR; }
            set
            {
                if (value != _LD_WAFER_OUT_SEONSOR)
                {
                    _LD_WAFER_OUT_SEONSOR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupIOStates _FoupIOManager;
        public IFoupIOStates FoupIOManager
        {

            get { return _FoupIOManager; }
            set
            {
                if (value != _FoupIOManager)
                {
                    _FoupIOManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
                {
                    StopBeforeProbingDictionary.Add(i + 1, false);
                    StopAfterProbingDictionary.Add(i + 1, false);

                    ClientsLock.Add(i + 1, new object());
                }


                this.FoupOpModule();
                this.E84Module();

                this.IsSelectedLoader = true;

                LoaderInfo = Loader.GetLoaderInfo();
                FoupIOManager = this.GetFoupIO();

                //<!-- Resigter Event -->
                this.EventManager().RegisterEvent(typeof(CarrierPlacedEvent).FullName, "ProbeEventSubscibers", EventFired);
                this.EventManager().RegisterEvent(typeof(CarrierRemovedEvent).FullName, "ProbeEventSubscibers", EventFired);
                this.EventManager().RegisterEvent(typeof(FoupAllocatedEvent).FullName, "ProbeEventSubscibers", EventFired);
                this.EventManager().RegisterEvent(typeof(FoupReadyToUnloadEvent).FullName, "ProbeEventSubscibers", EventFired);
                this.EventManager().RegisterEvent(typeof(CarrierCanceledEvent).FullName, "ProbeEventSubscibers", EventFired);
                this.EventManager().RegisterEvent(typeof(CarrierCompleateEvent).FullName, "ProbeEventSubscibers", EventFired);

                var foupcontrollers = this.FoupOpModule().FoupControllers;

                if (foupcontrollers != null)
                {
                    foreach (var controller in foupcontrollers)
                    {
                        try
                        {
                            var foupModule = controller.GetFoupService().FoupModule;
                            foupModule.PresenceStateChangedEvent += FoupPresenceStateChanged;
                            foupModule.FoupCarrierIdChangedEvent += FoupCarrierIdChanged;

                            if (controller.GetFoupModuleInfo().FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                            {
                                foupModule.Read_CassetteID();
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                LoadSysParameter();
                SetFoupOptionInfo();

                this.GEMModule().GetPIVContainer().FoupShiftMode.OriginPropertyPath = LotSysParam.FoupShiftMode.PropertyPath;
                this.GEMModule().GetPIVContainer().FoupShiftMode.ConvertToOriginTypeEvent += ConvertIntToFoupShift;

                LotSysParam.FoupShiftMode.ReportPropertyPath = this.GEMModule().GetPIVContainer().FoupShiftMode.PropertyPath;
                LotSysParam.FoupShiftMode.ConvertToReportTypeEvent += ConvertToFoupShiftToInt;



                EMGIO = Loader.IOManager.GetIOPortDescripter("DIEMGState");
                EMGIO.PropertyChanged += DI_EMGIO_PropertyChanged;                 

                //MAINVAC = Loader.IOManager.GetIOPortDescripter("DIMainAirs.0");
                //MAINVAC.PropertyChanged += MAINVAC_PropertyChanged;

                //MAINAIR = Loader.IOManager.GetIOPortDescripter("DIMainAirs.1");
                //MAINAIR.PropertyChanged += MAINAIR_PropertyChanged;

                // Operetta
                // MAINAIR.0 = CDA for Loader
                // MAINAIR.1 = Not used
                // MAINAIR.2 = VAC for Loader
                // MAINAIR.3 = Not used

                // Opera
                // MAINAIR.0 = CDA for Stage
                // MAINAIR.1 = CDA for Loader
                // MAINAIR.2 = VAC for Stage
                // MAINAIR.3 = VAC for Loader

                MAINVAC = Loader.IOManager.GetIOPortDescripter("DIMainAirs.3");

                if (MAINVAC != null)
                {
                    MAINVAC.PropertyChanged += MAINVAC_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error("Not Exist DIMainAirs.3[MAINVAC] in IOMapping Doc.");
                }

                MAINAIR = Loader.IOManager.GetIOPortDescripter("DIMainAirs.1");

                if (MAINAIR != null)
                {
                    MAINAIR.PropertyChanged += MAINAIR_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error("Not Exist DIMainAirs.1[MAINAIR] in IOMapping Doc.");
                }

                MAINVAC_STAGE = Loader.IOManager.GetIOPortDescripter("DIMainAirs.2");

                if (MAINVAC_STAGE != null)
                {
                    MAINVAC_STAGE.PropertyChanged += MAINVAC_STAGE_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error("Not Exist DIMainAirs.2[MAINVAC_STAGE] in IOMapping Doc.");
                }

                MAINAIR_STAGE = Loader.IOManager.GetIOPortDescripter("DIMainAirs.0");

                if (MAINAIR_STAGE != null)
                {
                    MAINAIR_STAGE.PropertyChanged += MAINAIR_STAGE_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error("Not Exist DIMainAirs.0[MAINAIR_STAGE] in IOMapping Doc.");
                }

                LOADER_RIGHTDOOR_OPEN = Loader.IOManager.GetIOPortDescripter("DIRightDoorOpen");

                if (LOADER_RIGHTDOOR_OPEN != null)
                {
                    LOADER_RIGHTDOOR_OPEN.PropertyChanged += LOADER_RIGHTDOOR_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error("Not Exist DIRightDoorOpen in IOMapping Doc.");
                }

                LOADER_LEFTDOOR_OPEN = Loader.IOManager.GetIOPortDescripter("DILeftDoorOpen");

                if (LOADER_LEFTDOOR_OPEN != null)
                {
                    LOADER_LEFTDOOR_OPEN.PropertyChanged += LOADER_LEFTDOOR_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error("Not Exist DILeftDoorOpen in IOMapping Doc.");
                }

                if (FoupIOManager.IOMap.Inputs.DI_FOUP_LOAD_BUTTONs.Count > 0)
                {
                    for (int i = 0; i < FoupIOManager.IOMap.Inputs.DI_FOUP_LOAD_BUTTONs.Count; i++)
                    {
                        try
                        {
                            FoupIOManager.IOMap.Inputs.DI_FOUP_LOAD_BUTTONs[i].PropertyChanged += ActiveLotInfos[i].DI_LOAD_SWITCH_PropertyChanged;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"IO data(DI_FOUP_LOAD_BUTTONs) is wrong.");
                }

                if (FoupIOManager.IOMap.Inputs.DI_FOUP_UNLOAD_BUTTONs.Count > 0)
                {
                    for (int i = 0; i < FoupIOManager.IOMap.Inputs.DI_FOUP_UNLOAD_BUTTONs.Count; i++)
                    {
                        try
                        {
                            FoupIOManager.IOMap.Inputs.DI_FOUP_UNLOAD_BUTTONs[i].PropertyChanged += ActiveLotInfos[i].DI_UNLOAD_SWITCH_PropertyChanged;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"IO data(DI_FOUP_UNLOAD_BUTTONs) is wrong.");
                }

                LD_PCW_LEAK_STATE = Loader.IOManager.GetIOPortDescripter("DILD_PCW_LEAK_STATE");

                if (LD_PCW_LEAK_STATE.ChannelIndex.Value == 0 && LD_PCW_LEAK_STATE.PortIndex.Value == 0)
                {
                    LD_PCW_LEAK_STATE = null;
                }
                else
                {
                    LD_PCW_LEAK_STATE.PropertyChanged += DI_PCW_MAINIO_PropertyChanged;
                }

                if (EMGIO.Value == true)
                {
                    EMOConfirm();
                }

                if (MAINVAC.Value == false)
                {
                    MainVacConfirm();
                }

                if (MAINAIR.Value == false)
                {
                    MainAirConfirm();
                }

                if (MAINVAC_STAGE.Value == false)
                {
                    MainVacStageConfirm();
                }

                if (MAINAIR_STAGE.Value == false)
                {
                    MainAirStageConfirm();
                }

                if (LOADER_RIGHTDOOR_OPEN.Value == true)
                {
                    Task task = new Task(() =>
                    {
                        LoaderRightDooropenConfirm();
                    });
                    task.Start();
                }

                if (LOADER_LEFTDOOR_OPEN.Value == true)
                {
                    Task task = new Task(() =>
                    {
                        LoaderLeftDooropenConfirm();
                    });
                    task.Start();
                }

                if (LD_PCW_LEAK_STATE != null && LD_PCW_LEAK_STATE.Value == true)
                {
                    DI_PCW_MAINIO_PropertyChanged(LD_PCW_LEAK_STATE, new PropertyChangedEventArgs("LD_PCW_LEAK_STATE"));
                }

                LD_WAFER_OUT_SEONSOR = Loader.IOManager.GetIOPortDescripter("DILD_WAFER_OUT_SEONSOR");
                if (LD_WAFER_OUT_SEONSOR != null)
                {
                    LoggerManager.Debug($"Exist DILD_WAFER_OUT_SEONSOR in IOMapping Doc." +
                        $" Channel: {LD_WAFER_OUT_SEONSOR.ChannelIndex.Value}, " +
                        $" Port: {LD_WAFER_OUT_SEONSOR.PortIndex.Value}, " +
                        $" Type: {LD_WAFER_OUT_SEONSOR.IOOveride.Value}");
                }
                else
                {
                    LoggerManager.Error("Not Exist DILD_WAFER_OUT_SEONSOR in IOMapping Doc.");
                }

                LoaderRecoveryControlVM.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region IO PropertyChanged Event & Confirm Method

        private object airLockObj = new object();
        private void MAINAIR_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (airLockObj)
                {
                    if (e.PropertyName == "Value")
                    {
                        MainAirConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MainAirConfirm()
        {
            try
            {
                bool value = false;
                Loader.IOManager.ReadIO(MAINAIR, out value);



                if (value == false)
                {
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    System.Threading.Thread.Sleep(100);
                    //    Loader.IOManager.ReadIO(MAINAIR, out value);

                    //    if (value == true)
                    //    {
                    //        isEMGStop = false;
                    //    }
                    //}
                    //if (isEMGStop)
                    //{
                    //    EMGSTOP();
                    //}
                    var result = Loader.IOManager.MonitorForIO(MAINAIR, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Main air loss occurred. Excute emergency stop.");
                        NotifySystemErrorToConnectedCells(EnumLoaderEmergency.AIR);
                        EMGSTOP(EventCodeEnum.LOADER_MAIN_AIR_ERROR);
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_MAIN_AIR_ERROR);
                        this.MetroDialogManager().ShowMessageDialog("MainAir Error", "The system has stopped because of MainAir Error.", EnumMessageStyle.Affirmative);
                        // Raising Loader System error
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(LoaderMainAirErrorEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }                    
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LoaderMainAirSuccessEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MAINAIR_STAGE_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (airLockObj)
                {
                    if (e.PropertyName == "Value")
                    {
                        MainAirStageConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MainAirStageConfirm()
        {
            try
            {
                bool value = false;
                Loader.IOManager.ReadIO(MAINAIR_STAGE, out value);



                if (value == false)
                {
                    var result = Loader.IOManager.MonitorForIO(MAINAIR_STAGE, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Main Air loss occurred. Excute emergency stop.");
                        NotifySystemErrorToConnectedCells(EnumLoaderEmergency.AIR);
                        EMGSTOP(EventCodeEnum.STAGE_MAIN_AIR_ERROR);
                        this.NotifyManager().Notify(EventCodeEnum.STAGE_MAIN_AIR_ERROR);
                        this.MetroDialogManager().ShowMessageDialog("Stage MainAir Error", "The system has stopped because of MainAir Error.", EnumMessageStyle.Affirmative);
                        // Raising Loader System error
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageMainAirErrorEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }                    
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageMainAirSuccessEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private object vacLockObj = new object();
        private void MAINVAC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (vacLockObj)
                {

                    if (e.PropertyName == "Value")
                    {
                        MainVacConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void MainVacConfirm()
        {
            try
            {
                bool value = false;
                Loader.IOManager.ReadIO(MAINVAC, out value);

                if (value == false)
                {
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    System.Threading.Thread.Sleep(100);
                    //    Loader.IOManager.ReadIO(MAINVAC, out value);

                    //    if (value == true)
                    //    {
                    //        isEMGStop = false;
                    //    }
                    //}
                    //if (isEMGStop)
                    //{
                    //    EMGSTOP();
                    //}
                    var result = Loader.IOManager.MonitorForIO(MAINVAC, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Main vac. error occurred. Excute emergency stop.");
                        NotifySystemErrorToConnectedCells(EnumLoaderEmergency.VACUUM);
                        EMGSTOP(EventCodeEnum.LOADER_MAIN_VAC_ERROR);
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_MAIN_VAC_ERROR);
                        this.MetroDialogManager().ShowMessageDialog("Main Vac Error", "The system has stopped because of MainVacuum Error.", EnumMessageStyle.Affirmative);
                        // Raising Loader System error
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(LoaderMainVacErrorEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }                    
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LoaderMainVacSuccessEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MAINVAC_STAGE_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (vacLockObj)
                {

                    if (e.PropertyName == "Value")
                    {
                        MainVacStageConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MainVacStageConfirm()
        {
            try
            {
                bool value = false;
                Loader.IOManager.ReadIO(MAINVAC_STAGE, out value);

                if (value == false)
                {
                    var result = Loader.IOManager.MonitorForIO(MAINVAC_STAGE, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Main vac. error occurred. Excute emergency stop.");
                        NotifySystemErrorToConnectedCells(EnumLoaderEmergency.VACUUM);
                        EMGSTOP(EventCodeEnum.STAGE_MAIN_VAC_ERROR);
                        this.NotifyManager().Notify(EventCodeEnum.STAGE_MAIN_VAC_ERROR);
                        this.MetroDialogManager().ShowMessageDialog("Stage Main Vac Error", "The system has stopped because of MainVacuum Error.", EnumMessageStyle.Affirmative);
                        // Raising Loader System error
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageMainVacErrorEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }                    
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageMainVacSuccessEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private object lockObj = new object();
        private void DI_EMGIO_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (lockObj)
                {

                    if (e.PropertyName == "Value")
                    {
                        EMOConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void EMOConfirm()
        {
            try
            {
                bool value = false;
                bool isEMGStop = true;
                Loader.IOManager.ReadIO(EMGIO, out value);

                if (value == true)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        System.Threading.Thread.Sleep(100);
                        Loader.IOManager.ReadIO(EMGIO, out value);

                        if (value == false)
                        {
                            isEMGStop = false;
                        }
                    }
                    if (isEMGStop)
                    {
                        EMGSTOP(EventCodeEnum.EMO_ERROR);
                        NotifySystemErrorToConnectedCells(EnumLoaderEmergency.EMG);
                        this.NotifyManager().Notify(EventCodeEnum.EMO_ERROR);
                        this.MetroDialogManager().ShowMessageDialog("EMG STOP", "The system has stopped because of Emgency Button.", EnumMessageStyle.Affirmative);
                        // Raising Loader System error
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(LoaderEMOErrorEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }                           
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LoaderEMOSuccessEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum EMGSTOP(EventCodeEnum errorCode)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Loader.ResonOfError = errorCode.ToString();
                Loader.SetEMGSTOP();
                StateObj.SetEMGSTOP();
                Loader.GetLoaderCommands().DisableAxis();
                EnvControlManager.SetEMGSTOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private object loaderRightDoorLockObj = new object();
        private object loaderLeftDoorLockObj = new object();
        private void LOADER_RIGHTDOOR_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (loaderRightDoorLockObj)
                {
                    if (e.PropertyName == "Value")
                    {
                        LoaderRightDooropenConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //randy// loader init시점에 door 열림 체크를 한번만 수행하기 위한 flag
        bool bLoaderLeftDoorOpenedCheck = false;
        bool bLoaderRightDoorOpenedCheck = false;
        private Task LoaderRightDooropenConfirm()
        {
            try
            {
                if (bLoaderRightDoorOpenedCheck)
                {
                    return Task.CompletedTask;
                }

                //randy// metro window가 활성화 되는 시점은 loader init이 완료되고 난 이후 이기 때문에 대기 로직을 추가함
                while (!this.MetroDialogManager().MetroWindowLoaded ||
                    this.MetroDialogManager().GetMetroWindow(true) == null)
                {
                    bLoaderRightDoorOpenedCheck = true;
                    Thread.Sleep(100);
                }

                bool value = false;
                var result = Loader.IOManager.MonitorForIO(LOADER_RIGHTDOOR_OPEN, true, 2000, 3000);
                if (result == EventCodeEnum.NONE)
                {
                    Loader.IOManager.ReadIO(LOADER_RIGHTDOOR_OPEN, out value);
                    if (value == true)
                    {
                        LoggerManager.Debug($"Loader Right Door Opened.");
                        LoaderDoorDialog.ShowDialog("Loader Right Door Opened.");
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_RIGHT_DOOR_OPEN);
                    }
                    else
                    {
                        LoggerManager.Debug($"Loader right door open occurred , but read io fail.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"Loader right door open occurred , but monitor io fail.");
                }
                GPLoader.SetLeftRightLoaderdoorOpen(value);
                LoggerManager.Debug($"Loader Right Door state : {value}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                bLoaderRightDoorOpenedCheck = false;
            }
            return Task.CompletedTask;
        }

        private void LOADER_LEFTDOOR_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (loaderLeftDoorLockObj)
                {
                    if (e.PropertyName == "Value")
                    {
                        LoaderLeftDooropenConfirm();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Task LoaderLeftDooropenConfirm()
        {
            try
            {
                if (bLoaderLeftDoorOpenedCheck)
                {
                    return Task.CompletedTask;
                }

                while (!this.MetroDialogManager().MetroWindowLoaded ||
                    this.MetroDialogManager().GetMetroWindow(true) == null)
                {
                    Thread.Sleep(100);
                    bLoaderLeftDoorOpenedCheck = true;
                }

                bool value = false;
                var result = Loader.IOManager.MonitorForIO(LOADER_LEFTDOOR_OPEN, true, 2000, 3000);
                if (result == EventCodeEnum.NONE)
                {
                    Loader.IOManager.ReadIO(LOADER_LEFTDOOR_OPEN, out value);
                    if (value == true)
                    {
                        LoggerManager.Debug($"Loader Left Door Opened.");
                        //message dialog가 표시되나 창 닫힘과 상관 없이 이후 로직이 수행되어야 하므로 task에 대한 await등 처리를 하지 않음
                        LoaderDoorDialog.ShowDialog("Loader Left Door Opened.");
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_LEFT_DOOR_OPEN);
                    }
                    else
                    {
                        LoggerManager.Debug($"Loader left door open occurred , but read io fail.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"Loader left door open occurred , but monitor io fail.");
                }
                GPLoader.SetLeftRightLoaderdoorOpen(value);
                LoggerManager.Debug($"Loader left Door state : {value}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                bLoaderLeftDoorOpenedCheck = false;
            }
            return Task.CompletedTask;
        }
        public EventCodeEnum GetLoaderDoorStatus(out bool leftdoor, out bool rightdoor)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool lefttemp = false;
            bool righttemp = false;
            try
            {
                Loader.IOManager.ReadIO(LOADER_LEFTDOOR_OPEN, out lefttemp);
                Loader.IOManager.ReadIO(LOADER_RIGHTDOOR_OPEN, out righttemp);
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                leftdoor = lefttemp;
                rightdoor = righttemp;
            }
            return ret;
        }
        private void DI_PCW_MAINIO_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (lockObj)
                {
                    if (LD_PCW_LEAK_STATE != null)
                    {
                        //Leak 센서가 감지된 경우
                        if (LD_PCW_LEAK_STATE.Value == true)
                        {
                            //알람 띄우고
                            this.NotifyManager().Notify(EventCodeEnum.LOADER_PCW_LEAK_ALARM);

                            //부저 울리고
                            GPLoader.LoaderBuzzer(true);

                            //LOT 가 Running 중이라면 LOT Pause 하고
                            if (ModuleState.GetState() == ModuleStateEnum.RUNNING)
                            {
                                this.CommandManager().SetCommand<IGPLotOpPause>(this);
                            }

                            //셀들의 PCW 벨브를 닫는다.
                            for (int index = 0; index < StageCount; index++)
                            {
                                EventCodeEnum ret = ((IGPLoaderCommands)this.GetGPLoader()).SetTesterCoolantValve(index, false);
                            }
                        }
                        else
                        {
                            this.NotifyManager().ClearNotify(EventCodeEnum.LOADER_PCW_LEAK_ALARM);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion
        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is CarrierPlacedEvent)
                {
                    if (e.Parameter is PIVInfo)
                    {
                        PIVInfo info = e.Parameter as PIVInfo;
                        var cstObj = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, info.FoupNumber);
                        if (cstObj != null)
                        {
                            cstObj.SetHashCode(true);
                        }
                    }
                }
                else if (sender is CarrierRemovedEvent)
                {
                    if (e.Parameter is PIVInfo)
                    {
                        PIVInfo info = e.Parameter as PIVInfo;
                        var cstObj = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, info.FoupNumber);
                        if (cstObj != null)
                        {
                            cstObj.SetHashCode(false);
                        }
                        ResetAssignLotInfo(info.FoupNumber);
                        ResetBackupActiveLotInfo(info.FoupNumber);
                    }
                }
                else if (sender is FoupAllocatedEvent)
                {
                    if (e.Parameter is PIVInfo)
                    {
                        PIVInfo info = e.Parameter as PIVInfo;
                        var cells = info.ListOfStages.Split(',');
                        int[] ints = Array.ConvertAll(cells, s => int.TryParse(s, out var x) ? x : -1);
                        foreach (var index in ints)
                        {
                            if (index != -1)
                            {
                                IRemoteMediumProxy proxy = loaderCommunicationManager.GetProxy<IRemoteMediumProxy>(index);
                                if (proxy != null)
                                {
                                    //proxy.SetOperatorName(lotsetting.OperatorName);
                                    var retVal = proxy.CheckAndConnect();
                                    FoupAllocatedInfo allocatedInfo = new FoupAllocatedInfo(info.FoupNumber, info.RecipeID, info.LotID);
                                    EventCodeEnum ret = proxy.FoupAllocated(allocatedInfo);
                                }
                            }
                        }

                        // [STM_CATANIA] Allocated 상태 아이콘 UI 업데이트
                        Loader.Foups[info.FoupNumber - 1].LotSettings.ToList().ForEach(x => x.IsAssigned = ints.Contains(x.Index));
                        Loader.Foups[info.FoupNumber - 1].LotSettings.ToList().ForEach(x => x.IsSelected = ints.Contains(x.Index));
                        Loader.Foups[info.FoupNumber - 1].LotSettings.ToList().ForEach(x =>
                        {
                            if (ints.Contains(x.Index))
                            {
                                x.LotName = info.LotID;
                                x.RecipeName = info.RecipeID;
                            }
                        });
                    }
                }
                else if (sender is FoupReadyToUnloadEvent)
                {
                    if (DynamicMode == DynamicModeEnum.NORMAL)
                    {
                        if (e.Parameter is PIVInfo)
                        {
                            PIVInfo info = e.Parameter as PIVInfo;
                            if (info != null)
                            {
                                //ResetAssignLotInfo(info.FoupNumber);
                            }
                        }
                    }
                    foreach (var activeLotInfo in ActiveLotInfos)
                    {
                        if (activeLotInfo.AttemptedFoupUnload == true)
                            activeLotInfo.AttemptedFoupUnload = false;
                    }
                        
                }
                else if (sender is CarrierCompleateEvent || sender is CarrierCanceledEvent) 
                {
                    if (e.Parameter is PIVInfo)
                    {
                        PIVInfo info = e.Parameter as PIVInfo;
                        if (info != null)
                        {
                            if (LotSysParam != null)
                            {
                                if (LotSysParam.LoaderLotEndBuzzerON == true)
                                {
                                    string msg = $"Lot is finished. (Foup #{info.FoupNumber})";
                                    this.MetroDialogManager().ShowMessageDialog("Lot End", msg, EnumMessageStyle.Affirmative);
                                    Loader.GetLoaderCommands().LoaderBuzzer(true);
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
        }
        public bool IsLotReady(out string msg)
        {
            bool retval = true;
            try
            {
                msg = "";
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public void SetStageState(int idx, ModuleStateEnum stageState, bool isBuzzerOn)
        {
            try
            {
                if (StageStates.Count >= idx - 1 & StageStates.Count != 0)
                {
                    if (StageStates[idx - 1] != stageState)
                    {
                        if (stageState != ModuleStateEnum.PAUSED && stageState != ModuleStateEnum.ERROR)
                        {
                            bool isError = false;
                            foreach (var state in StageStates)
                            {
                                if (state == ModuleStateEnum.PAUSED || state == ModuleStateEnum.ERROR)
                                {
                                    isError = true;
                                    break;
                                }
                            }
                            if (!isError)
                            {
                                GPLoader.StageLampSetState(stageState);
                            }
                        }
                        else
                        {
                            GPLoader.StageLampSetState(stageState);
                        }
                        StageStates[idx - 1] = stageState;

                    }
                    if (isBuzzerOn)
                    {
                        GPLoader.LoaderBuzzer(isBuzzerOn);
                    }
                (Loader as LoaderModule).LoaderMapUpdate();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (errorCode == EventCodeEnum.GP_CARD_STUCK)
                {
                    MessageBoxResult result = MessageBox.Show("Card is Stuck, Do you want to progress Card Recovery?",
                        "System Init Error", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        return retVal;
                    }
                    else
                    {
                        return retVal;
                    }

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ResponseCardRecovery(EventCodeEnum errorCode)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //if (errorCode == EventCodeEnum.GP_CARD_STUCK)
                //{

                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsAliveClient(ILoaderServiceCallback client, int waittime = 15)
        {
            bool retVal = false;
            try
            {
                if (client != null)
                {
                    if (client is StageEmulation)
                    {
                        return true;
                    }

                    if ((client as ICommunicationObject).State == CommunicationState.Opened)
                    {
                        var item = Clients.FirstOrDefault(x => x.Value == client);
                        if (item.Equals(default(KeyValuePair<string, ILoaderServiceCallback>)))
                            return false;
                        var cellId = item.Key;
                        int idx = 0;
                        Int32.TryParse(cellId.Substring(5), out idx);
                        if (!this.GEMModule().GemCommManager.GetRemoteConnectState(idx))
                        {
                            return false;
                        }

                        object chnLock = ClientsLock[idx];
                        lock (chnLock)
                        {
                            var originOperationTimeout = (client as IContextChannel).OperationTimeout;
                            try
                            {
                                (client as IContextChannel).OperationTimeout = new TimeSpan(0, 0, waittime);
                                retVal = client.IsServiceAvailable();
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                                StackFrame frame = new StackFrame(1, true);
                                string prevFuncName = frame.GetMethod().Name;
                                string prevFileName = frame.GetFileName();
                                int prevLine = frame.GetFileLineNumber();
                                LoggerManager.Debug($"IsAliveClient Fail, Caller={prevFuncName},{prevFileName},Line={prevLine}");
                            }
                            finally
                            {
                                (client as IContextChannel).OperationTimeout = originOperationTimeout;
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
        public ModuleStateEnum GetLotStateClient(ILoaderServiceCallback client)
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetLotState();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetLotStateClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public LoaderMap RequestJobClient(ILoaderServiceCallback client, LoaderInfo loaderInfo, out bool isExchange, out bool isNeedWafer, out bool isTempReady, out string cstHashCodeOfRequestLot, bool canloadwafer = true)
        {
            LoaderMap retVal = null;
            isExchange = false;
            isNeedWafer = false;
            isTempReady = false;
            cstHashCodeOfRequestLot = "";
            try
            {
                if (IsAliveClient(client))
                {
                    var lotState = client.GetLotState();

                    if (loaderCommunicationManager.Cells[client.GetChuckID().Index - 1].StageMode == GPCellModeEnum.ONLINE)
                    {
                        if (lotState == ModuleStateEnum.IDLE && loaderCommunicationManager.Cells[client.GetChuckID().Index - 1].IsStageModeChanged)
                        {
                            //bool isSucess = LotOPStartClient(client);
                            //isLotStartFlag[client.GetChuckID().Index - 1] = isSucess;
                            loaderCommunicationManager.Cells[client.GetChuckID().Index - 1].IsStageModeChanged = false;
                        }
                        else if (lotState == ModuleStateEnum.PAUSED && loaderCommunicationManager.Cells[client.GetChuckID().Index - 1].IsStageModeChanged)
                        {
                            //LotOPResumeClient(client);
                            loaderCommunicationManager.Cells[client.GetChuckID().Index - 1].IsStageModeChanged = false;
                        }
                        else
                        {
                            loaderCommunicationManager.Cells[client.GetChuckID().Index - 1].IsStageModeChanged = false;
                        }

                        retVal = client.RequestJob(loaderInfo, out isExchange, out isNeedWafer, out isTempReady, out cstHashCodeOfRequestLot, canloadwafer);

                        StagesIsTempReady[client.GetChuckID().Index - 1] = isTempReady;
                    }
                    else
                    {
                        if (lotState == ModuleStateEnum.RUNNING)
                        {
                            LotOPPauseClient(client);
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], RequestJobClient() : Failed,");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public EventCodeEnum OnLoaderParameterChangedClient(ILoaderServiceCallback client, LoaderSystemParameter systemParam, LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.OnLoaderParameterChanged(systemParam, deviceParam);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], OnLoaderParameterChangedClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum OnLoaderInfoChangedClient(ILoaderServiceCallback client, LoaderInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.OnLoaderInfoChanged(info);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], OnLoaderInfoChangedClient() : Failed");
                }

                /// lock (lockObj)
                //{
                //retVal = client.OnLoaderInfoChanged(info);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum CSTInfoChangedClient(ILoaderServiceCallback client, LoaderInfo info)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.CSTInfoChanged(info);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], CSTInfoChangedClient() : Failed");
                }

                // lock (lockObj)
                // {
                //retVal = client.CSTInfoChanged(info);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WaferIDChangedClient(ILoaderServiceCallback client, int slotNum, string ID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.WaferIDChanged(slotNum, ID);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], WaferIDChangedClient() : Failed");
                }

                // lock (lockObj)
                // {
                //retVal = client.WaferIDChanged(slotNum, ID);
                // }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ErrorEndClient(ILoaderServiceCallback client, int cellIdx, bool canUnloadWhilePaused)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    if (client.GetLotState() != ModuleStateEnum.IDLE)
                    {
                        retVal = client.ReserveErrorEnd();
                        this.loaderCommunicationManager.Cells[cellIdx - 1].StageInfo.IsReceiveErrorEnd = true; //로더쪽 플래그 true
                    }
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], ErrorEndClient() : Failed");
                }

                // lock (lockObj)
                // {
                //retVal = client.ReserveErrorEnd();
                // }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum WaferHolderChangedClient(ILoaderServiceCallback client, int slotNum, string holder)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.WaferHolderChanged(slotNum, holder);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], WaferHolderChangedClient() : Failed");
                }

                // lock (lockObj)
                // {
                //retVal = client.WaferHolderChanged(slotNum, holder);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WaferStateChangedClient(ILoaderServiceCallback client, int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.WaferStateChanged(slotNum, waferStatus, waferState);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], WaferStateChangedClient() : Failed");
                }

                //retVal = client.WaferStateChanged(slotNum, waferStatus, waferState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WaferSwapChangedClient(ILoaderServiceCallback client, int originSlotNum, int changeSlotNum, bool isInit)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.WaferSwapChanged(originSlotNum, changeSlotNum, isInit);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], WaferSwapChangedClient() : Failed");
                }

                //retVal = client.WaferSwapChanged(originSlotNum, changeSlotNum, isInit);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EnumSubsStatus GetChuckWaferStatusClient(ILoaderServiceCallback client)
        {
            EnumSubsStatus retVal = EnumSubsStatus.UNKNOWN;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetChuckWaferStatus();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetChuckWaferStatusClient() : Failed");
                }

                //retVal = client.GetChuckWaferStatus();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public bool IsHandlerholdWafer(ILoaderServiceCallback client)
        {
            bool retVal = false;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.IsHandlerholdWafer();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetChuckWaferStatusClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EnumSubsStatus GetCardStatusClient(ILoaderServiceCallback client, out EnumWaferState cardState)
        {
            EnumSubsStatus retVal = EnumSubsStatus.UNKNOWN;
            cardState = EnumWaferState.UNDEFINED;
            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.UpdateCardStatus(out cardState);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetCardStatusClient() : Failed");
                }

                //retVal = client.GetCardStatus();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public string GetProbeCardIDClient(ILoaderServiceCallback client)
        {
            string retVal = "";

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetProbeCardID();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetProbeCardIDClient() : Failed");
                }

                //retVal = client.GetProbeCardID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public string GetWaferIDClient(ILoaderServiceCallback client)
        {
            string retVal = "";

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetWaferID();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetWaferIDClient() : Failed");
                }

                //retVal = client.GetWaferID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public int GetSlotIndexClient(ILoaderServiceCallback client)
        {
            int retVal = 0;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetSlotIndex();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetSlotIndexClient() : Failed");
                }

                //retVal = client.GetSlotIndex();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public ModuleID GetChuckIDClient(ILoaderServiceCallback client)
        {
            ModuleID retVal = new ModuleID();

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetChuckID();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetChuckIDClient() : Failed");
                }

                //retVal = client.GetChuckID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ModuleID GetOriginHolderClient(ILoaderServiceCallback client)
        {
            ModuleID retVal = new ModuleID();

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetOriginHolder();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetOriginHolderClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        //public LoaderMap UnloadRequestJobClient(ILoaderServiceCallback client, LoaderInfo loaderInfo)
        //{
        //    LoaderMap retVal = null;

        //    try
        //    {
        //        if (IsAliveClient(client))
        //        {
        //            retVal = client.UnloadRequestJob(loaderInfo);
        //        }
        //        else
        //        {
        //            LoggerManager.Error($"[LoaderSupervisor], UnloadRequestJobClient() : Failed");
        //        }

        //        //retVal = client.UnloadRequestJob(loaderInfo);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        public bool LotOPStartClient(ILoaderServiceCallback client, bool iscellstart = false, string lotID = "", int foupnumber = -1, string cstHashCodeOfRequestLot = "")
        {
            bool retVal = false;
            try
            {
                if (IsAliveClient(client))
                { 
                    if (client.IsCanPerformLotStart())
                    {
                        // iscellstart 가 true 인 경우는 MANUAL CELL START시
                        // 할당된 LOT 가 있는지 확인하고, 있는경우에만 START 한다. (할당된 LOT 가 없는경우에는 CELL 이 RUNNING 으로 무한 대기가 걸릴 수 있으므로)
                        if (iscellstart)
                        {
                            int stageIdx = client.GetChuckID().Index;
                            bool isAssignLot = false;
                            List<ActiveLotInfo> assignActiveLotInfos = new List<ActiveLotInfo>();
                            foreach (var info in ActiveLotInfos)
                            {
                                if (info.State == LotStateEnum.Running)
                                {
                                    var usingstageobj = info.AssignedUsingStageIdxList.Find(idx => idx == stageIdx);
                                    if (usingstageobj != 0)
                                    {
                                        isAssignLot = true;
                                        foupnumber = info.FoupNumber;
                                        LoggerManager.Debug($"LotOPStartClient() : {client.GetChuckID()} assign lot of , " +
                                           $"ActiveLotInfo : FOUP#{info.FoupNumber} - LOT#{info.LotID} - CSTHCODE#{info.CST_HashCode}");
                                        assignActiveLotInfos.Add(info);
                                    }
                                }
                            }

                            if (isAssignLot == false)
                            {
                                this.MetroDialogManager().ShowMessageDialog("Information Message",
                                    "Since there is no allocated LOT, it is not possible to proceed with CELL LOT.", EnumMessageStyle.Affirmative);
                                return retVal;
                            }
                           
                            SetActiveDevicestoStage(assignActiveLotInfos, client.GetChuckID().Index);
                            SetReAssignAtAbortStageInfo(client.GetChuckID().Index);
                        }

                        if(String.IsNullOrEmpty(cstHashCodeOfRequestLot) == false)
                        {
                            var activeLotInfo = ActiveLotInfos.Find(lotInfo => lotInfo.CST_HashCode.Equals(cstHashCodeOfRequestLot));
                            if(activeLotInfo != null)
                            {
                                lotID = activeLotInfo.LotID;
                                foupnumber = activeLotInfo.FoupNumber;
                            }
                        }

                        LotOPResetAbortClient(client);
                        retVal = client.LotOPStart(foupnumber, iscellstart, lotID, cstHashCodeOfRequestLot);
                    }
                    else
                    {
                        LoggerManager.Debug($"[LoaderSupervisor] LotOPStartClient(), cell#{client.GetChuckID().Index} cannot receive lot start command.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], LotOPStartClient() : Failed");
                }

                //if(client)

                //retVal = client.LotOPStart();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public bool LotOPResetAbortClient(ILoaderServiceCallback client)
        {
            bool retVal = false;

            try
            {
                if (IsAliveClient(client))
                {
                    client.SetAbort(false);
                    retVal = true;
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], LotOPEndReadyClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// 매개변수로 넘겨진 activeLotInfo 에 할당된 Stage 들에 Lot 정보를 할당 하기위한 목적의 함수
        /// </summary>
        public void SetActiveLotInfotoStage(ActiveLotInfo activeLotInfo)
        {
            try
            {
                if(activeLotInfo != null && activeLotInfo.AssignedUsingStageIdxList != null)
                {
                    string foupID = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, activeLotInfo.FoupNumber).FoupID;
                    foreach (var stageidx in activeLotInfo.AssignedUsingStageIdxList)
                    {
                        var stageClient = GetClient(stageidx);
                        stageClient.SetActiveLotInfo(activeLotInfo.FoupNumber, activeLotInfo.LotID, activeLotInfo.CST_HashCode, foupID);
                        LoggerManager.Debug($"[LoaderSupervisor] SetActiveLotInfotoStage(). StageIdx : {stageidx}, FoupNum : {activeLotInfo.FoupNumber}, LotID : {activeLotInfo.LotID}, CST_HashCode : {activeLotInfo.CST_HashCode}, FoupId : {foupID}.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetActiveDevicestoStage(List<ActiveLotInfo> activeLotInfo, int stageIdx)
        {
            try
            {
                foreach (var lotInfo in activeLotInfo)
                {
                    var usingstageobj = lotInfo.AssignedUsingStageIdxList.Find(idx => idx == stageIdx);
                    if (usingstageobj != 0)
                    {
                        var client = GetClient(stageIdx);
                        client?.SetActiveLotInfo(lotInfo.FoupNumber, lotInfo.LotID, lotInfo.CST_HashCode,
                            Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, lotInfo.FoupNumber).FoupID);

                        string deviceName = "";
                        var ret = lotInfo.CellDeviceInfoDic.TryGetValue(stageIdx, out deviceName);
                        if (ret)
                        {
                            string zippath = "";
                            ret = lotInfo.OriginalDeviceZipName.TryGetValue(deviceName, out zippath);
                            if (ret)
                            {
                                //zip 파일을 cell 로 전송.
                                var device = File.ReadAllBytes(zippath);
                                var stage = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageIdx);
                                if (stage != null)
                                {
                                    stage.SetDevice(device, deviceName, lotInfo.LotID, lotInfo.CST_HashCode, true, lotInfo.FoupNumber, false, true);

                                    NeedChangeParameterInDevice setParameterActReqData = null;
                                    ret = lotInfo.CellSetParamOfDeviceDic.TryGetValue(stageIdx, out setParameterActReqData);
                                    if (ret && setParameterActReqData != null)
                                    {
                                        Thread.Sleep(1000);
                                        stage.SetNeedChangeParaemterInDeviceInfo(setParameterActReqData);
                                    }
                                }
                                else
                                {
                                    LoggerManager.Error($"[LoaderSupervisor] SetActiveDevicestoStage(). No Connection Stage#{stageIdx} ");
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
        }

        public bool LotOPEndReadyClient(ILoaderServiceCallback client)
        {
            bool retVal = false;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.IsLotEndReady();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], LotOPEndReadyClient() : Failed");
                    retVal = true;
                }

                //if(client)

                //retVal = client.IsLotEndReady();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool LotOPPauseClient(ILoaderServiceCallback client, bool isabort = false, IStageObject stageobj = null)
        {
            bool retVal = false;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.LotOPPause(isabort);
                    if (stageobj != null)
                    {
                        stageobj.StageInfo.IsReservePause = client.GetReservePause();
                    }
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], LotOPPauseClient() : Failed");
                }

                //if(client)

                //retVal = client.LotOPPause();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool LotOPResumeClient(ILoaderServiceCallback client)
        {
            bool retVal = false;

            try
            {
                if (IsAliveClient(client))
                {

                    retVal = client.LotOPResume();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], LotOPResumeClient() : Failed");
                }

                //if(client)

                //retVal = client.LotOPResume();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool LotOPEndClient(ILoaderServiceCallback client, int foupnumber = -1, bool isabortlot = false)
        {
            bool retVal = false;

            try
            {
                if (IsAliveClient(client))
                {
                    client.SetAbort(true);
                    retVal = client.LotOPEnd(foupnumber);

                    /// isabortlot 가 true 인 경우 : MANUAL CELL ABORT 
                    if (isabortlot)
                    {
                        /// 해당 경우에는 진행 중인 랏드와 해당 시점에서 할당된 모든 LOT 에서 할당을 해제하여 더이상 진행되지 않도록 한다.
                        /// <!-- 2022.10.20 -->
                        AddLotAbortStageInfos(client.GetChuckID().Index, true, false);
                    }
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], LotOPEndClient() : Failed");
                }

                //if(client)

                //retVal = client.LotOPEnd(foupnumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public StageLotData GetStageLotDataClient(ILoaderServiceCallback client)
        {
            StageLotData retVal = null;

            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.GetStageInfo();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetStageLotDataClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public List<ReasonOfStageMoveLock> GetReasonofLockFromClient(int stageIdx)
        {
            List<ReasonOfStageMoveLock> retval = new List<ReasonOfStageMoveLock>();
            try
            {
                var client = GetClient(stageIdx);
                if (IsAliveClient(client))
                {
                    retval = client.GetReasonofLockFromClient();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetReasonofLockFromClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum SetStageMoveLockStateClient(ILoaderServiceCallback client, ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.StageMoveLockState(reason);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], SetStageMoveLockStateClient() : Failed");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetStageMoveUnLockStateClient(ILoaderServiceCallback client, ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsAliveClient(client))
                {
                    retVal = client.StageMoveUnLockState(reason);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], SetStageMoveLockStateClient() : Failed");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetStageModeClient(ILoaderServiceCallback client, GPCellModeEnum mode)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsAliveClient(client))
                {
                    return client.SetStageMode(mode);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], SetStageModeClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public void SetCellModeChangingClient(ILoaderServiceCallback client)
        {
            try
            {
                if (IsAliveClient(client))
                {
                    client.SetCellModeChanging();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], SetCellModeChangingClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ResetCellModeChangingClient(ILoaderServiceCallback client)
        {
            try
            {
                if (IsAliveClient(client))
                {
                    client.ResetCellModeChanging();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], ResetCellModeChangingClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStreamingModeClient(ILoaderServiceCallback client, StreamingModeEnum mode)
        {
            try
            {
                if (IsAliveClient(client))
                {
                    client.SetStreamingMode(mode);
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], SetStreamingModeClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public TransferObject GetDeviceInfoClient(ILoaderServiceCallback client)
        {
            TransferObject retVal = null;

            try
            {
                //if(client)

                if (IsAliveClient(client))
                {
                    retVal = client.GetDeviceInfo();
                }
                else
                {
                    LoggerManager.Error($"[LoaderSupervisor], GetDeviceInfoClient() : Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void DisConnectClient(ILoaderServiceCallback client)
        {
            try
            {
                client.DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum NotifyStageSystemError(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                //cellindex => 시스템에러가난 cell
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public EventCodeEnum NotifyClearStageSystemError(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                //cellindex => SystemError가 Clear된 Stage cell
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public EventCodeEnum NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                var stages = this.loaderCommunicationManager.GetStages();

                foreach (var stage in stages)
                {
                    if (stage.StageInfo.IsConnected)
                    {
                        var cell = this.loaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stage.Index);

                        LoggerManager.Debug($"NotifySystemError to Cell{stage.Index.ToString().PadLeft(2, '0')} ");

                        if (cell != null)
                        {
                            cell.NotifySystemErrorToConnectedCells(emgtype);
                        }

                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;

        }

        public EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo)
        {
            EventCodeEnum retval = EventCodeEnum.NOTIFY_ERROR;

            try
            {
                retval = this.NotifyManager().Notify(noticeCodeInfo, noticeCodeInfo.Index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetTitleMessage(int cellno, string message, string foreground = "", string background = "")
        {
            try
            {
                loaderCommunicationManager.SetTitleMessage(cellno, message, foreground, background);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetActionLogMessage(string message, int idx, ModuleLogType ModuleType, StateLogType State)
        {
            try
            {
                message = message.Replace("[LOT] | ", "");
                LoggerManager.ActionLog(message, idx, ModuleType, State);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetParamLogMessage(string message, int idx)
        {
            try
            {
                LoggerManager.ParamLog(message, idx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsLoaderEMOActive()
        {
            bool ret = false;
            try

            {
                if (EMGIO != null)
                {
                    if (EMGIO.Value)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public bool IsLoaderMainAirDown()
        {
            bool ret = false;
            try
            {
                if (MAINAIR != null)
                {
                    if (!MAINAIR.Value)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public bool IsLoaderMainVacDown()
        {
            bool ret = false;
            try
            {
                if (MAINVAC != null)
                {
                    if (!MAINVAC.Value)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public bool IsCardUpmoduleUp()
        {
            bool ret = false;
            try
            {
                var client = GetClient(loaderCommunicationManager.SelectedStageIndex);  
                if (client != null)
                {
                    if (IsAliveClient(client))
                    {
                        ret = client.IsCardUpModuleUp();
                    }
                    else
                    {
                        LoggerManager.Error($"[LoaderSupervisor], IsCardUpmoduleUp() : Failed");
                    }
                }
                //if (client == null)
                //{
                //}
                //else
                //{
                //    ret = client.IsCardUpModuleUp();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public void SetProbingStart(int cellIdx, bool isStart)
        {
            try
            {
                this.loaderCommunicationManager.Cells[cellIdx - 1].IsProbing = isStart;

                if (isStart)
                {
                    this.loaderCommunicationManager.Cells[cellIdx - 1].ProbingTime = DateTime.Now;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void SetTransferError(int cellIdx, bool isError)
        {
            try
            {

                this.loaderCommunicationManager.Cells[cellIdx - 1].isTransferError = isError;
                if (isError)
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog($"Transfer Vacuum Error in Cell{cellIdx}", $"How would you like to change the wafer state?", EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary, "UnProcessed", "Processed", "Skip").Result;

                    IAttachedModule chuckModule = this.Loader.ModuleManager.FindModule(ModuleTypeEnum.CHUCK, cellIdx);
                    IWaferOwnable ownable = chuckModule as IWaferOwnable;
                    if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        if (ownable != null && ownable.Holder.Status == EnumSubsStatus.EXIST)
                            ownable.Holder.TransferObject.WaferState = EnumWaferState.UNPROCESSED;
                    }
                    else if (retVal == EnumMessageDialogResult.NEGATIVE)
                    {
                        if (ownable != null && ownable.Holder.Status == EnumSubsStatus.EXIST)
                            ownable.Holder.TransferObject.WaferState = EnumWaferState.PROCESSED;
                    }
                    else if (retVal == EnumMessageDialogResult.FirstAuxiliary)
                    {
                        if (ownable != null && ownable.Holder.Status == EnumSubsStatus.EXIST)
                            ownable.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                    }
                    this.loaderCommunicationManager.Cells[cellIdx - 1].StageMode = GPCellModeEnum.MAINTENANCE;
                    this.Loader.BroadcastLoaderInfo(false);
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

        /// <summary>
        /// UsingStageIdxList 에 StageIndex 를 추가 / 제거를 관리 
        /// </summary>
        public void CheckUsingStage()
        {
            try
            {
                if (LotAbortStageInfos != null)
                {
                    List<AbortStageInformation> needRemoveList = null;

                    foreach (var stgInfo in LotAbortStageInfos)
                    {
                        if (stgInfo.IsRemoved == false)
                        {
                            var client = GetClient(stgInfo.StageNumber);
                            if (client != null)
                            {
                                if (client.GetChuckWaferStatus() == EnumSubsStatus.EXIST)
                                    continue;
                            }
                            if (stgInfo.LotBannedList.Count > 0)
                            {
                                foreach (var bannedlist in stgInfo.LotBannedList)
                                {
                                    var lotInfo = ActiveLotInfos.Find(lotinfo =>
                                                                    lotinfo.LotID != null &&
                                                                    lotinfo.CST_HashCode != null &&
                                                                    lotinfo.LotID.Equals(bannedlist.LotID) &&
                                                                    lotinfo.FoupNumber == bannedlist.FoupIndex &&
                                                                    lotinfo.CST_HashCode.Equals(bannedlist.CassetteHashcode));
                                    if (lotInfo != null)
                                    {
                                        if (lotInfo.UsingStageIdxList.Find(idx => idx == stgInfo.StageNumber) != 0)
                                        {
                                            lotInfo.UsingStageIdxList.Remove(stgInfo.StageNumber);
                                            LoggerManager.Debug($"CheckUsingStage() remove stage#{stgInfo.StageNumber} at" +
                                                $" LOTID : {lotInfo.LotID}, FOUP IDX : {lotInfo.FoupNumber}, CST HASHCODE : {lotInfo.CST_HashCode}");
                                        }

                                        stgInfo.IsRemoved = true;
                                    }
                                }
                            }
                            else
                            {
                                stgInfo.IsRemoved = true;
                            }
                        }
                        else
                        {
                            // 리스트에서 제거 작업이 끝났지만 다시 LOT 합류 시킬때 
                            if (stgInfo.IsReAssignTrigger && stgInfo.IsCanReAssignLot)
                            {
                                if (needRemoveList == null)
                                    needRemoveList = new List<AbortStageInformation>();

                                // 할당 해제 되었던 Lot List 들에 다시 추가.
                                if (stgInfo.LotBannedList.Count > 0)
                                {
                                    foreach (var bannedlist in stgInfo.LotBannedList)
                                    {
                                        var lotInfo = ActiveLotInfos.Find(lotinfo =>
                                                                        lotinfo.LotID != null &&
                                                                        lotinfo.CST_HashCode != null &&
                                                                        lotinfo.LotID.Equals(bannedlist.LotID) &&
                                                                        lotinfo.FoupNumber == bannedlist.FoupIndex &&
                                                                        lotinfo.CST_HashCode.Equals(bannedlist.CassetteHashcode));
                                        if (lotInfo != null)
                                        {
                                            if (lotInfo.AssignedUsingStageIdxList.Find(idx => idx == stgInfo.StageNumber) != 0)
                                            {
                                                if (lotInfo.UsingStageIdxList.Find(idx => idx == stgInfo.StageNumber) == 0)
                                                {
                                                    lotInfo.UsingStageIdxList.Add(stgInfo.StageNumber);
                                                    LoggerManager.Debug($"CheckUsingStage() add stage#{stgInfo.StageNumber} at" +
                                                        $" LOTID : {lotInfo.LotID}, FOUP IDX : {lotInfo.FoupNumber}, CST HASHCODE : {lotInfo.CST_HashCode}");
                                                }
                                            }
                                        }
                                    }
                                    // LotBannedList 에 있던 ActiveLotInfo 들에 Stage Index 를 추가 한 뒤에 LotBannedList 는 초기화
                                    stgInfo.LotBannedList.Clear();
                                }
                                needRemoveList.Add(stgInfo);
                            }
                        }
                    }

                    if (needRemoveList != null)
                    {
                        if (needRemoveList.Count > 0)
                        {
                            for (int removecount = 0; removecount < needRemoveList.Count; removecount++)
                            {
                                LoggerManager.Debug($"CheckUsingStage() stage#{needRemoveList[removecount].StageNumber} remove at LotAbortStageInfos");
                                LotAbortStageInfos.Remove(needRemoveList[removecount]);
                            }
                        }
                        needRemoveList = null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void CheckActiveLotInfo()
        {
            try
            {
                foreach (var lotinfo in ActiveLotInfos)
                {
                    if (lotinfo.State == LotStateEnum.Running)
                    {
                        if (lotinfo.UsingStageIdxList.Count == 0)
                        {
                            bool isCancelLot = true;
                            foreach (var stagenum in lotinfo.AssignedUsingStageIdxList)
                            {
                                var abortStageInfo = LotAbortStageInfos.Find(stginfo => stginfo.StageNumber == stagenum);
                                if (abortStageInfo != null)
                                {
                                    if (abortStageInfo.IsCanReAssignLot)// 하나라도 다시 lot를 재개할 수 있는 셀이 있으면 lot을 cancel하지 않는다. 
                                    {
                                        isCancelLot = false;
                                    }
                                }
                            }

                            if (isCancelLot)
                            {
                                lotinfo.State = LotStateEnum.Cancel;
                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// LotAbortStageInfos 리스트 Clear (External LOT Abort -> Done 상태갈때 호출 됨)
        /// </summary>
        public void ClearLotAbortStageInfos()
        {
            try
            {
                if (LotAbortStageInfos != null)
                {
                    if (LotAbortStageInfos.Count > 0)
                    {
                        foreach (var abortStageInfo in LotAbortStageInfos)
                        {
                            LoggerManager.Debug($"ClearLotAbortStageInfos() : StageNum : {abortStageInfo.StageNumber}");
                        }
                        LotAbortStageInfos.Clear();
                        LoggerManager.Debug("ClearLotAbortStageInfos()");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        /// <summary>
        /// 셀이 현재 진행 중인 Lot에 금지되어있는 지 확인하기 위한 함수입니다. 
        /// </summary>
        /// <param name="stageNumber"></param>
        /// <returns></returns>
        public bool IsReAssignUnavailableStage(int stageNumber)
        {
            bool findBannedLot = false;
            try
            {
                if(stageNumber > 0)
                {
                    // 현재 진행중인 Lot 를 찾는다 
                    List<ActiveLotInfo> runningLotList = ActiveLotInfos.Where(w => w.State == LotStateEnum.Running).ToList();

                    // AbortStageInfo 중에 타겟셀이 있는지 확인하고 abort 당한 LotInfo를 가지고 온다. 
                    var banndLotList = LotAbortStageInfos.Where(w => w.StageNumber == stageNumber && w.IsCanReAssignLot == false).FirstOrDefault()?.LotBannedList;
                    if(banndLotList != null) 
                    {
                        foreach (var item in runningLotList)
                        {
                            if (banndLotList.Find(w => w.CassetteHashcode == item.CST_HashCode) != null)
                            {
                                findBannedLot = true;
                                break;
                            }
                        }
                    }
                  
                }
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return findBannedLot;
        }

        /// <summary>
        /// CELL END, ABORT 등 Cell 의 Lot 가 중단될때 ActiveLot 에서 데이터 처리 및 추후 LOT 동작관리를 위한 리스트를 생성하기위한 함수.
        /// LotAbortStageInfos 에 데이터가 추가되어서, LOT 동작에 사용되는 데이터를 생성하는데 있어서 사용된다.
        /// </summary>
        /// <param name="stageIdx"></param>
        /// <param name="isCanReassignLot"></param> : MANAUL CELL START 등 랏드 재개 Trigger 에 대한 허용 여부
        /// <param name="abortCurrentLot"></param> :지 [True]현재 Running ( 진행 중이던 LOT) 인 LOT 에서만 해제할건지,[False]할당된 LOT 에서 모두 해제할건 
        public void AddLotAbortStageInfos(int stageIdx, bool isCanReassignLot, bool abortCurrentLot)
        {
            try
            {
                AbortStageInformation abortStageInformation = new AbortStageInformation();
                abortStageInformation.StageNumber = stageIdx;
                abortStageInformation.IsCanReAssignLot = isCanReassignLot;
                abortStageInformation.LotBannedList = new List<LotInfoPack>();
                if (abortCurrentLot == false)
                {
                    foreach (var lotinfo in ActiveLotInfos)
                    {
                        if (lotinfo.State == LotStateEnum.Running)
                        {
                            //UsingStageIdxList 에 있다는 것은 해당 LOT 에 할당되어 돌고 있다는 것. 
                            if (lotinfo.UsingStageIdxList.Find(idx => idx == stageIdx) != 0)
                            {
                                LotInfoPack lotInfoPack = new LotInfoPack();
                                lotInfoPack.LotID = lotinfo.LotID;
                                lotInfoPack.FoupIndex = lotinfo.FoupNumber;
                                lotInfoPack.CassetteHashcode = lotinfo.CST_HashCode;
                                abortStageInformation.LotBannedList.Add(lotInfoPack);
                                LoggerManager.Debug($"AddLotAbortStageInfos() stage#{stageIdx} add to LotBannedList" +
                                           $" LOTID : {lotinfo.LotID}, FOUP IDX : {lotinfo.FoupNumber}, CST HASHCODE : {lotinfo.CST_HashCode}");

                            }
                        }
                    }
                }
                LotAbortStageInfos.Add(abortStageInformation);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// LOT끝날때 AbortStageList 에서 해당 LOT 정보는 삭제
        /// </summary>
        public void RemoveLotInfoAtLotBannedList(int foupNumber, string lotId, string cstHashCode)
        {
            try
            {
                if (LotAbortStageInfos != null)
                {
                    List<AbortStageInformation> needRemoveList = new List<AbortStageInformation>();
                    foreach (var stageinfo in LotAbortStageInfos)
                    {
                        var info = stageinfo.LotBannedList.Find(lotinfo => lotinfo.FoupIndex == foupNumber &&
                                                                            lotinfo.LotID.Equals(lotId) &&
                                                                            lotinfo.CassetteHashcode.Equals(cstHashCode));
                        if (info != null)
                        {
                            stageinfo.LotBannedList.Remove(info);
                            LoggerManager.Debug($"RemoveLotInfoAtLotBannedList() stage#{stageinfo.StageNumber} remove to LotBannedList" +
                                       $" LOTID : {info.LotID}, FOUP IDX : {info.FoupIndex}, CST HASHCODE : {info.CassetteHashcode}");
                        }

                        if (stageinfo.LotBannedList.Count == 0)
                        {
                            needRemoveList.Add(stageinfo);
                        }
                    }

                    for (int removecount = 0; removecount < needRemoveList.Count; removecount++)
                    {
                        LoggerManager.Debug($"RemoveLotInfoAtLotBannedList() stage#{needRemoveList[removecount].StageNumber} remove at LotAbortStageInfos");
                        LotAbortStageInfos.Remove(needRemoveList[removecount]);
                    }

                    needRemoveList = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Abort 되었던 Cell을 다시 재개하기 위해 ReAssign Falg 를 true 로 변경. 
        /// </summary>
        public void SetReAssignAtAbortStageInfo(int stageIdx)
        {
            try
            {
                if (LotAbortStageInfos != null)
                {
                    var stageInfo = LotAbortStageInfos.Find(info => info.StageNumber == stageIdx);
                    if (stageInfo != null)
                    {
                        stageInfo.IsReAssignTrigger = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LotCancelRequestJob()
        {
            try
            {
                foreach (var activeLotInfo in ActiveLotInfos)
                {
                    // activeLotInfo 안에 bool 타입을 하나 ㅎ만들어서 동작시키고, 초기화는 foup unload가 되었을때 
                    if (activeLotInfo.AssignState == LotAssignStateEnum.CANCEL )
                    {
                        var Cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, activeLotInfo.FoupNumber);
                        CassetteScanStateEnum cassetteScanStateEnum = Cassette.ScanState;
                        string foupstateStr = "";
                        if (ValidationFoupUnloadedState(activeLotInfo.FoupNumber, ref foupstateStr) == EventCodeEnum.NONE)
                        {
                            activeLotInfo.SetAssignLotState(LotAssignStateEnum.CANCELED);
                            if(activeLotInfo.State != LotStateEnum.Idle)
                            {
                                if (ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                {
                                    activeLotInfo.State = LotStateEnum.Idle;
                                }
                            }

                            PIVInfo pivinfo = new PIVInfo(foupnumber: activeLotInfo.FoupNumber);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(FoupUnloadedInLotAbortEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        else if (ValidationFoupLoadedState(activeLotInfo.FoupNumber, ref foupstateStr) == EventCodeEnum.NONE && activeLotInfo.AttemptedFoupUnload == false)
                        {                            
                            if (cassetteScanStateEnum != CassetteScanStateEnum.READING && cassetteScanStateEnum != CassetteScanStateEnum.RESERVED &&
                                this.FoupOpModule().GetFoupController(activeLotInfo.FoupNumber)?.GetFoupService()?.FoupModule?.ValidationAvailableFoupState(activeLotInfo.FoupNumber) == EventCodeEnum.NONE)
                            {                                
                                //Idle 이 아닌경우에 Cancel 되면 External Running State 에서 처리 되므로 여기서 Unload 하면 안됨.
                                if (activeLotInfo.State == LotStateEnum.Idle)
                                {
                                    var retVal = CassetteUnload(activeLotInfo.FoupNumber).Result;

                                    LoggerManager.Debug($"[LoaderSupervisor] LotCancelRequestJob()" +
                                         $"Foup #{activeLotInfo.FoupNumber} CassetteUnload Request Done.");
                                }
                            }
                            else
                            {
                                // message box 여기서 띄운다. 한번만
                                this.MetroDialogManager().ShowMessageDialog("Foup Unload Error", "An error occurred during the Foup Unload operation.", EnumMessageStyle.Affirmative);
                                activeLotInfo.AttemptedFoupUnload = true;
                            }
                        }

                        // excute 에서 불리지만, 이 안에서 foup 에 할당되어 있는 cell이 end 될수있는 조건인지 아닌지 판단하기 때문에 다음 틱에 들어와도 문제가 없을것으로 예상.        
                        ValidationCancelLot(activeLotInfo.FoupNumber);  
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        /// <summary>
        /// foupNum 에 해당하는 foup lot cancel 
        /// cellNum 에 해당하는 cell만 개별 lot cancel        
        /// </summary>
        /// <param name="foupNum"></param>
        /// <param name="cellNum"></param>
        public void ValidationCancelLot(int foupNum = -1 )
        {            
            try
            {
                if (foupNum != -1)
                {
                    // foup lot cancel  // foup 에 할당된 cell lot cancel
                    // 셀은 cancel 인데, 로더는 cancel 이 아닐수도있으니까 , 그부분 고려 필요
                    var activeLotInfo = ActiveLotInfos.Find(a => a.FoupNumber == foupNum);
                    if(activeLotInfo != null)
                    {
                        foreach (var stagenum in activeLotInfo.UsingStageIdxList)
                        {
                            var stage = GetClient(stagenum);
                            if (stage != null)
                            {
                                stage.CancelLot(activeLotInfo.FoupNumber, activeLotInfo.IsManaulEnd, activeLotInfo.LotID, activeLotInfo.CST_HashCode);
                                //LoggerManager.Debug($"CanCancelLot() Cell#{stagenum}, Foup Number : {activeLotInfo.FoupNumber}, Lot ID : {activeLotInfo.LotID}, CST HashCode : {activeLotInfo.CST_HashCode}");
                            }
                        }
                    }                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }            
        }

        public EventCodeEnum LotSuspend(int foupNum)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var lotInfo = ActiveLotInfos.Where(i => i.FoupNumber == foupNum).FirstOrDefault();
                lotInfo.LotPriority = ActiveLotInfos.OrderBy(item => item.LotPriority).Last().LotPriority + 1;
                Loader.Foups[foupNum - 1].LotPriority = lotInfo.LotPriority;
                LoggerManager.ActionLog(ModuleLogType.LOT_SETTING, StateLogType.SUSPEND, $"LOT Suspend Execute, FOUP: {foupNum} ,  LotPriority Number: {lotInfo.LotPriority } ", isLoaderMap: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetAngleInfo(int chuckIndex, TransferObject wafer)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double notchAngle = 0;
                double slotAngle = 0;
                double ocrAngle = 0;
                var cell = GetClient(chuckIndex);
                if (cell != null)
                {
                    cell.GetAngleInfo(out notchAngle, out slotAngle, out ocrAngle);

                    var transfer = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                    if (transfer != null)
                    {
                        transfer.ChuckNotchAngle.Value = notchAngle;
                        transfer.SlotNotchAngle.Value = slotAngle;
                        transfer.OCRAngle.Value = ocrAngle;
                        (Loader as LoaderModule).LoaderMapUpdate();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetNotchType(int chuckIndex, TransferObject wafer)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var cell = GetClient(chuckIndex);
                if (cell != null)
                {
                    cell.GetNotchTypeInfo(out WaferNotchTypeEnum notchType);

                    var transfer = Loader.ModuleManager.FindTransferObject(wafer.ID.Value);
                    if (transfer != null)
                    {
                        transfer.NotchType = notchType;                        
                        (Loader as LoaderModule).LoaderMapUpdate();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void FoupPresenceStateChanged(int foupIndex, bool presenceState, bool presenceStateChangedDone)
        {
            try
            {
                if (presenceStateChangedDone)
                {
                    if (presenceState == true)
                    {
                        if (this.FoupOpModule().FoupControllers[foupIndex - 1].FoupModuleInfo.IsCassetteAutoLock || GetIsCassetteAutoLock())
                        {
                            Thread.Sleep(500);
                            this.FoupOpModule().FoupControllers[foupIndex - 1].Execute(new FoupDockingPlateLockCommand());
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string LOADER_BACKUP_FILE = @"C:\Logs\Backup\loaderInfo.Json";
        public void FoupCarrierIdChanged(int foupIndex, string carrierId)
        {
            try
            {
                var cassettes = Loader.ModuleManager.FindModules<ICassetteModule>();
                if (cassettes != null)
                {
                    if (cassettes.Count() >= foupIndex)
                    {
                        LoggerManager.Debug($"FoupCarrierIdChanged({foupIndex}): FoupID = {carrierId}");
                        //LoaderInfo.StateMap.CassetteModules[foupIndex - 1].FoupID = carrierId;
                        //cassettes.Where(cst => cst.ID.Index == foupIndex).FirstOrDefault().FoupID = carrierId;
                        var cassette = cassettes.Where(item => item.ID.Index == foupIndex).FirstOrDefault();
                        cassette.SetCarrierId(carrierId);

                        Extensions_IParam.SaveParameter(null, LoaderInfo, null, LOADER_BACKUP_FILE, isNotSave_ChangeLog: true);

                    }
                    else
                    {
                        LoggerManager.Debug($"FoupCarrierIdChanged Fail, foupnumber : {foupIndex}");
                    }
                }

                this.Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStageLock(int stageIndex, StageLockMode mode)
        {
            try
            {
                var cell = this.CellsInfo.Where(i => i.Index == stageIndex).FirstOrDefault();

                loaderCommunicationManager.Cells[stageIndex - 1].LockMode = mode;
                if (mode == StageLockMode.LOCK)
                {
                    this.GPLoader.LoaderBuzzer(true);
                }
                this.Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTCW_Mode(int stageIndex, TCW_Mode mode)
        {
            try
            {
                var cell = this.CellsInfo.Where(i => i.Index == stageIndex).FirstOrDefault();
                LoggerManager.Debug($"TCW_Mode Change. Cell{stageIndex}, Prev TCW_Mode:{cell.TCWMode}, CurrentMode:{mode}");
                cell.TCWMode = mode;

                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetForcedDoneMode(int stageIndex, EnumModuleForcedState forcedDoneMode)
        {
            try
            {
                var cell = this.CellsInfo.Where(i => i.Index == stageIndex).FirstOrDefault();

                cell.ForcedDone = forcedDoneMode;
                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDynamicMode(DynamicModeEnum modeEnum)
        {
            try
            {
                DynamicMode = modeEnum;
                LotSysParam.DynamicMode = DynamicMode;
                SaveSysParameter();
                if (DynamicMode == DynamicModeEnum.NORMAL)
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LotModeChangeToNormalEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }
                else if (DynamicMode == DynamicModeEnum.DYNAMIC)
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LotModeChangeToDynamicEvent).FullName, new ProbeEventArgs(this, semaphore));
                    semaphore.Wait();
                }


                var cells = loaderCommunicationManager.Cells;
                if (cells != null)
                {
                    foreach (var cell in cells)
                    {
                        if (cell.StageInfo.IsConnected)
                        {
                            var stgProxy = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>(cell.Index);
                            if (stgProxy != null)
                            {
                                stgProxy.SetDynamicMode(DynamicMode);
                                LoggerManager.Debug($"SetDynamicMode() to stage #{cell.Index}, mode : {DynamicMode}");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// CarrierRemovedEvent 시 호출해주는 함수.
        /// Lot 가 할당되어서 ActiveInfo는 있지만 Lot 가 동작하지 않고, 카세트가 제거되었을때 데이터를 초기화 해주기 위함. 
        /// </summary>
        /// <param name="foupnumber"></param>
        object resetLotInfoLockObj = new object();
        public void ResetAssignLotInfo(int foupnumber)
        {
            try
            {
                lock (resetLotInfoLockObj)
                {
                    var activeInfo = ActiveLotInfos.Find(infos => infos.FoupNumber == foupnumber);
                    if (activeInfo != null && activeInfo.AssignState != LotAssignStateEnum.UNASSIGNED)
                    {
                        LoggerManager.Debug($"ResetAssignLotInfo() Foup Number : {foupnumber}, Lot ID : {activeInfo.LotID}, CST HashCode : {activeInfo.CST_HashCode}");
                        //if (activeInfo.State == LotStateEnum.Idle)
                        if (activeInfo.AssignedUsingStageIdxList != null && DynamicMode == DynamicModeEnum.NORMAL)
                        {
                            foreach (var stageidx in activeInfo.AssignedUsingStageIdxList)
                            {
                                var stage = GetClient(stageidx);
                                if (stage != null)
                                {
                                    stage.ResetAssignLotInfo(foupnumber, activeInfo.LotID, activeInfo.CST_HashCode);
                                    LoggerManager.Debug($"ResetAssignLotInfo() Cell#{stageidx}, Foup Number : {foupnumber}, Lot ID : {activeInfo.LotID}, CST HashCode : {activeInfo.CST_HashCode}");
                                }
                            }
                        }
                        var backupActiveInfo = BackupActiveLotInfos.Find(infos => infos.FoupNumber == foupnumber);
                        if (backupActiveInfo != null)
                        {
                            backupActiveInfo.LotID = activeInfo.LotID;
                            backupActiveInfo.SetAssignLotState(activeInfo.AssignState);
                            backupActiveInfo.State = activeInfo.State;
                            LoggerManager.Debug($"ResetAssignLotInfo() Set BackupActiveLotInfo. FoupNumber : {foupnumber}, LotID : {backupActiveInfo.LotID}, AssignState : {backupActiveInfo.AssignState}, LotState : {backupActiveInfo.State}.");
                        }

                        activeInfo.ClearLotInfo();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ResetBackupActiveLotInfo(int foupnumber)
        {
            try
            {
                var backupActiveInfo = BackupActiveLotInfos.Find(infos => infos.FoupNumber == foupnumber);
                if(backupActiveInfo != null)
                {
                    LoggerManager.Debug($"ResetBackupActiveLotInfo() Clear BackupActiveLotInfo. FoupNumber : {foupnumber}, LotID : {backupActiveInfo.LotID}, AssignState : {backupActiveInfo.AssignState}, LotState : {backupActiveInfo.State}.");
                    backupActiveInfo.ClearLotInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        //public void RemoveUsingStage(ActiveLotInfo lotinfo)
        //{
        //    //v22_merge// 해당 함수 삭제 필요
        //    try
        //    {
        //        if(lotinfo.State != LotStateEnum.Running)
        //        {
        //            return;
        //        }
        //        // PAbort 받은 셀은 다음 랏드를 시작했을때 Abort되었던 Foup의 웨이퍼는 받으면 안됨. 따라서 UsingStage에서 제거해줘야함.
        //        // 셀이 웨이퍼를 가지고 있을 경우 웨이퍼를 빼줘야하기 때문에 만약 PAbort 받은 셀중 웨이퍼가 없는 셀만 UsingStage에서 뺄수 있음. 
        //        // ExternalRequestJob에서 UsingStage를 접근하기때문에 같은 스레드에서 제거해줘야함.
        //        var tmpUsingStageIndex = new List<int>(lotinfo.UsingStageIdxList);
        //        foreach (var stageIndex in tmpUsingStageIndex)
        //        {
        //            var stage = GetClient(stageIndex);
        //            if (IsAliveClient(stage))
        //            {
        //                var ChuckModule = LoaderInfo.StateMap.ChuckModules[stageIndex - 1];
        //                if (
        //                   lotinfo.LotOutStageIndexList.Contains(stageIndex) &&// 타이밍상 Foup Lot Start되었는데 셀이 LotOp Start되기전 여기를 타면서 제거될수 있음.!!!!!!!!!!!!!! 조건 추가 필요
        //                    ChuckModule.WaferStatus == EnumSubsStatus.NOT_EXIST)
        //                {
        //                    lotinfo.UsingStageIdxList.Remove(stageIndex);
        //                    if (lotinfo.RingBuffer.Contains(stageIndex))
        //                    {
        //                        var tmpringbuffer = new Queue<int>(lotinfo.RingBuffer);

        //                        foreach (var buffer in tmpringbuffer)
        //                        {
        //                            lotinfo.RingBuffer.Dequeue();
        //                            if (buffer != stageIndex)
        //                            {
        //                                lotinfo.RingBuffer.Enqueue(buffer);
        //                            }

        //                        }
        //                    }

        //                    LoggerManager.Debug($"PAbort Deallocate UsingStage. " +
        //                        $"FoupNumber: {lotinfo.FoupNumber}," +
        //                        $"LotId: {lotinfo.LotID}, " +
        //                        $"CST_HashCode: {lotinfo.CST_HashCode}, " +
        //                        $"UsingStageIdxList:{string.Join(",", tmpUsingStageIndex)}=>{string.Join(",", lotinfo.UsingStageIdxList)}");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}


        public bool IsSameDeviceEndToSlot(string deviceName, List<int> usingStageList, bool isExistUnprocessedWafer = false)
        {
            bool risingEvent = true;
            try
            {
                List<ActiveLotInfo> TotalLotInfos = ActiveLotInfos;
                TotalLotInfos.AddRange(Prev_ActiveLotInfos);

                var SameLotInfos = TotalLotInfos.Where(lotinfo => lotinfo.DeviceName == deviceName &&
                                                                  Enumerable.SequenceEqual(usingStageList, lotinfo.UsingStageIdxList));
                //lotinfo.State != LotStateEnum.Idle);
                //셀 할당은 StageSlot 되는 시점이므로 LotState가 Idle.
                //그때는 SameLotInfo로 포함되어야함.                 

                //UsingSlot에 할당되어 있는데 Unprocessed Wafer가 존재하는 경우 risingevent를 하면 안됨.
                //
                if (SameLotInfos.Count() == 0)
                {
                    risingEvent = false;
                    return risingEvent;
                }

                var allwafer = _Loader.ModuleManager.GetTransferObjectAll().Where(w => w.WaferType.Value == EnumWaferType.STANDARD);
                var allocatedSlots = new List<TransferObject>();
                foreach (var lotinfo in SameLotInfos)
                {

                    // 할당된 같은 디바이스의 웨이퍼의 
                    allocatedSlots.AddRange(
                                             allwafer.Where(w => lotinfo.CST_HashCode == w.CST_HashCode &&
                                                                 lotinfo.UsingSlotList.Contains((w.OriginHolder.Index % 25 == 0) ? 25 : w.OriginHolder.Index % 25)
                                                                 ).ToList()
                                            );


                }

                var currentSlots = allocatedSlots.Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT);//curFoupNumber == (w.CurrHolder.Index -1) / 25 + 1 &&
                var notUnprocessed = allocatedSlots.Where(w => w.WaferState != EnumWaferState.UNPROCESSED);


                if (allocatedSlots.Count() == currentSlots.Count()
                    && notUnprocessed.Count() == allocatedSlots.Count())
                {
                    risingEvent = true;
                }
                else
                {
                    risingEvent = false;
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
            return risingEvent;

        }


        /// <summary>
        /// CarrierRemovedEvent 시 호출해주는 함수.
        /// Lot 가 할당되어서 ActiveInfo는 있지만 Lot 가 동작하지 않고, 카세트가 제거되었을때 데이터를 초기화 해주기 위함. 
        /// </summary>
        /// <param name="foupnumber"></param>
        //public void ResetAssignLotInfo(int foupnumber)
        //{
        //    try
        //    {
        //        var activeInfo = ActiveLotInfos.Find(infos => infos.FoupNumber == foupnumber);
        //        if(activeInfo != null)
        //        {
        //            if(activeInfo.State == LotStateEnum.Idle)
        //            {
        //                if (activeInfo.UsingStageList != null)
        //                {
        //                    foreach (var stage in activeInfo.UsingStageList)
        //                    {
        //                        stage.ResetAssignLotInfo(foupnumber, activeInfo.LotID);
        //                    }
        //                }
        //            }
        //            if(activeInfo.IsActiveFormHost == true)
        //            {
        //                activeInfo.IsActiveFormHost = false;
        //                LoggerManager.Debug($"CassetteNormalUnload Foup#{foupnumber} IsActiveFormHost change to false");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        #region <remarks> ISystemParameterizable Methods<remarks>

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderLotSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderLotSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    LotSysParam = tmpParam as LoaderLotSysParameter;
                    DynamicMode = LotSysParam.DynamicMode;
                    this.E84Module().SetIsCassetteAutoLock(LotSysParam.IsCassetteAutoLock);
                    this.E84Module().SetIsCassetteAutoLockLeftOHT(LotSysParam.IsCassetteAutoLockLeftOHT);

                    var cstLockFlag = this.E84Module().GetE84CassetteLockParam()?.AutoSetCassetteLockEnable ?? false;

                    if (cstLockFlag == false)
                    {
                        if (this.FoupOpModule().FoupControllers != null)
                        {
                            foreach (var foupcontroller in this.FoupOpModule().FoupControllers)
                            {
                                foupcontroller.FoupModuleInfo.IsCassetteAutoLock = LotSysParam.IsCassetteAutoLock;
                                foupcontroller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = LotSysParam.IsCassetteAutoLockLeftOHT;
                            }
                        }
                    }

                    this.E84Module().SetFoupCassetteLockOption();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(LotSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
        #endregion

        #region <remarks> Get & Set Lot SysParam </remarks>
        public bool GetIsCassetteAutoLock()
        {
            return LotSysParam?.IsCassetteAutoLock ?? false;
        }

        public void SetIsCassetteAutoLock(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.IsCassetteAutoLock = flag;
                    this.E84Module().SetIsCassetteAutoLock(flag);
                    LoggerManager.Debug($"[LotSysParam] IsCassetteAutoLock set to {LotSysParam.IsCassetteAutoLock}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetIsCassetteAutoLockLeftOHT()
        {
            return LotSysParam?.IsCassetteAutoLockLeftOHT ?? false;
        }

        public void SetIsCassetteAutoLockLeftOHT(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.IsCassetteAutoLockLeftOHT = flag;
                    this.E84Module().SetIsCassetteAutoLockLeftOHT(flag);
                    LoggerManager.Debug($"[LotSysParam] IsCassetteAutoLockLeftOHT set to {LotSysParam.IsCassetteAutoLockLeftOHT}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetFoupOptionInfo()
        {
            var foupcontrollers = this.FoupOpModule().FoupControllers;

            if (foupcontrollers != null)
            {
                FoupOptionInfomation info = new FoupOptionInfomation(GetIsCassetteDetectEventAfterRFID());
                foreach (var controller in foupcontrollers)
                {
                    try
                    {
                        controller.GetFoupService().FoupModule.SetFoupOptionInfomation(info);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public bool GetIsCassetteAutoUnloadAfterLot()
        {
            return LotSysParam?.IsCassetteAutoUnloadAfterLot ?? false;
        }


        public void SetIsCassetteAutoUnloadAfterLot(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.IsCassetteAutoUnloadAfterLot = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetIsWaitForWaferIdConfirm()
        {
            return LotSysParam?.IsWaitForWaferIdConfirm ?? false;
        }


        public void SetIsWaitForWaferIdConfirm(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.IsWaitForWaferIdConfirm = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int GetWaitForWaferIdConfirmTimeout()
        {
            return LotSysParam?.WaferIdConfirmTimeout_msec ?? 60000;
        }


        public void SetWaitForWaferIdConfirmTimeout(int msec)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.WaferIdConfirmTimeout_msec = msec;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetIsCancelCarrierEventNotRuning()
        {
            return LotSysParam?.IsCancelCarrierEventNotRuning ?? false;
        }

        public void SetIsCancelCarrierEventNotRuning(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.IsCancelCarrierEventNotRuning = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetIsCassetteDetectEventAfterRFID()
        {
            return LotSysParam?.IsCassetteDetectEventAfterRFID ?? false;
        }

        public void SetIsCassetteDetectEventAfterRFID(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.IsCassetteDetectEventAfterRFID = flag;
                    SetFoupOptionInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetIsLoaderLotEndBuzzerON()
        {
            return LotSysParam?.LoaderLotEndBuzzerON ?? false;
        }

        public void SetIsLoaderLotEndBuzzerON(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.LoaderLotEndBuzzerON = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetIsAlwaysCloseFoupCover()
        {
            return LotSysParam?.AlwaysCloseFoupCover ?? false;
        }
        public void SetIsAlwaysCloseFoupCover(bool flag)
        {
            try
            {
                if (LotSysParam != null)
                {
                    LotSysParam.AlwaysCloseFoupCover = flag;
                }
            }
            catch (Exception err )
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetExecutionTimeoutError()
        {
            return LotSysParam?.ExecutionTimeoutError?.Value ?? 5;
        }

        public int GetLotPauseTimeoutAlarm()
        {
            return LotSysParam?.LotPauseTimeoutAlarm?.Value ?? 0;
        }
        public void SetLotPauseTimeoutAlarm(int time)
        {
            try
            {
                if (LotSysParam != null)
                {
                    double preTimeOut = LotSysParam.LotPauseTimeoutAlarm.Value;
                    LotSysParam.LotPauseTimeoutAlarm.Value = time;
                    LoggerManager.Debug($"LotPauseTimeoutAlarm {preTimeOut}(sec) change to {time}(sec).");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region Transfer Functions
        private LoaderMap cardTransferMap = null;
        private LoaderMap waferTransferMap = null;

        /// <summary>
        /// Card를 Transfer 하기 위해 현재 상태가 유효한 상태인지 체크하는 함수
        /// </summary>
        /// <param name="source">ICCModule 또는 ICardBufferModule</param>
        /// <param name="target">ICCModule 또는 ICardBufferModule</param>
        /// <returns>Valid 여부를 반환</returns>
        public bool ValidateTransferCardObject(ICardOwnable source, ICardOwnable target)
        {
            try
            {
                bool isValid = false;
                cardTransferMap = _Loader.GetLoaderInfo().StateMap;

                if (source is ICCModule && target is ICardBufferModule) // Unload 인 경우
                {
                    var ccModule = source as ICCModule;
                    var cardBufferModule = target as ICardBufferModule;

                    if (null == ccModule || null == cardBufferModule)
                    {
                        LoggerManager.Debug($"ccModule or cardBufferModule is null");
                        return false;
                    }

                    var cardBuffer = cardTransferMap.CardBufferModules.Where(i => i.ID.Index == cardBufferModule.ID.Index).FirstOrDefault();
                    if (cardBuffer == null)
                    {
                        LoggerManager.Debug($"CardBuffer info is null");
                        return false;
                    }

                    var stage = GetClient(ccModule.ID.Index);
                    EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                    EnumSubsStatus status = EnumSubsStatus.UNDEFINED;
                    if (stage != null)
                    {
                        status = GetCardStatusClient(stage, out cardStateEnum);
                    }

                    if (cardBuffer.WaferStatus == EnumSubsStatus.CARRIER &&
                        (cardStateEnum == EnumWaferState.READY || //카드인데 zif 완료 상태 
                        cardStateEnum == EnumWaferState.PROCESSED || //카드 zif unlock 상태이거나 카드 홀더 도킹 상태 
                        cardStateEnum == EnumWaferState.UNPROCESSED) && //카드 팟에 있는 상태
                        status == EnumSubsStatus.EXIST) // CardBuffer에 Carrier만 존재하고 Docking 상태여야 CardChange가 가능하다
                    {
                        isValid = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"CardBuffer WaferStatus or Docking status is not valid. CardBuffer WaferStatus : {cardBuffer.WaferStatus}");
                        isValid = false;
                    }

                }
                else if (source is ICardBufferModule && target is ICCModule) // Holder Load 인 경우
                {
                    var ccModule = target as ICCModule;
                    var cardBufferModule = source as ICardBufferModule;

                    if (null == ccModule || null == cardBufferModule)
                    {
                        LoggerManager.Debug($"ccModule or cardBufferModule is null");
                        return false;
                    }

                    var cardBuffer = cardTransferMap.CardBufferModules.Where(i => i.ID.Index == cardBufferModule.ID.Index).FirstOrDefault();
                    if (cardBuffer == null)
                    {
                        LoggerManager.Debug($"CardBuffer info is null");
                        return false;
                    }

                    // CardBuffer가 Exist 상태이고 Docking 상태가 아니어야 한다.
                    var stage = GetClient(ccModule.ID.Index);
                    EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                    EnumSubsStatus status = EnumSubsStatus.UNDEFINED;
                    if (stage != null)
                    {
                        status = GetCardStatusClient(stage, out cardStateEnum);
                    }

                    if (cardBuffer.WaferStatus == EnumSubsStatus.EXIST && status == EnumSubsStatus.NOT_EXIST)
                    {
                        isValid = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"CardBuffer WaferStatus or Docking status is not valid. CardBuffer WaferStatus : {cardBuffer.WaferStatus}");
                        isValid = false;
                    }
                }
                else if (source is ICardBufferModule && target is ICardARMModule) // Card Load 인 경우
                {
                    var cardArmModule = target as ICardARMModule;
                    var cardBufferModule = source as ICardBufferModule;

                    if (null == cardArmModule || null == cardBufferModule)
                    {
                        LoggerManager.Debug($"cardArmModule or cardBufferModule is null");
                        return false;
                    }

                    var cardBuffer = cardTransferMap.CardBufferModules.Where(i => i.ID.Index == cardBufferModule.ID.Index).FirstOrDefault();
                    if (cardBuffer == null)
                    {
                        LoggerManager.Debug($"CardBuffer info is null");
                        return false;
                    }

                    // CardBuffer가 Exist 상태이고 CardArm이 Not Exist 상태여야 한다.
                    if (cardBuffer.WaferStatus == EnumSubsStatus.EXIST && cardArmModule.Holder.Status == EnumSubsStatus.NOT_EXIST)
                    {
                        isValid = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"CardBuffer status or CardArm status is not valid. CardBuffer Status : {cardBuffer.WaferStatus}, CardArm Status : {cardArmModule.Holder.Status}");
                        isValid = false;
                    }
                }
                else if (source is ICardARMModule && target is ICCModule) // Card Load 인 경우
                {
                    var ccModule = target as ICCModule;
                    var cardArmModule = source as ICardARMModule;

                    if (null == ccModule || null == cardArmModule)
                    {
                        LoggerManager.Debug($"ccModule or cardArmModule is null");
                        return false;
                    }

                    // CardArm이 Exist 상태이고 Docking 상태가 아니어야 한다.
                    var stage = GetClient(ccModule.ID.Index);
                    EnumWaferState cardStateEnum = EnumWaferState.UNDEFINED;
                    EnumSubsStatus status = EnumSubsStatus.UNDEFINED;
                    if (stage != null)
                    {
                        status = GetCardStatusClient(stage, out cardStateEnum);
                    }
                    if (cardArmModule.Holder.Status == EnumSubsStatus.EXIST && status == EnumSubsStatus.NOT_EXIST)
                    {
                        isValid = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"CardArm status or Docking status is not valid. CardArm Status : {cardArmModule.Holder.Status}, Cell Status : {status}");
                        isValid = false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"Card Transfer source or target is not valid.");
                    isValid = false;
                }

                return isValid;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }


        /// <summary>
        /// Card를 Transfer 하는 함수
        /// </summary>
        /// <param name="source">ICCModule 또는 ICardBufferModule</param>
        /// <param name="target">ICCModule 또는 ICardBufferModule</param>
        /// <returns>Card Transfer 동작의 성공 여부를 반환</returns>
        public bool TransferCardObjectFunc(ICardOwnable source, ICardOwnable target) // [TODO] source target으로 받아서 Load, Unload 구분하기!!
        {
            try
            {
                #region [Step1] Source 정보와 Target 정보를 이용하여 ID 받아오는 부분
                var loader = _Loader;
                cardTransferMap = loader.GetLoaderInfo().StateMap;

                string sourceId = null;
                ModuleID targetId = new ModuleID();

                // source와 target이 Cell -> CardBuffer 도 아니고 CardBuffer -> Cell 도 아닌 경우 false로 return
                if (source is ICCModule && target is ICardBufferModule)
                {
                    var sourceModule = source as ICCModule;
                    var targetModule = target as ICardBufferModule;

                    sourceId = sourceModule.Holder.TransferObject.ID.Value;
                    targetId = target.ID;
                }
                else if (source is ICardBufferModule && target is ICCModule)
                {
                    var sourceModule = source as ICardBufferModule;
                    var targetModule = target as ICCModule;

                    sourceId = sourceModule.Holder.TransferObject.ID.Value;
                    targetId = target.ID;
                }
                else if (source is ICardBufferModule && target is ICardARMModule)
                {
                    var sourceModule = source as ICardBufferModule;
                    var targetModule = target as ICardARMModule;

                    sourceId = sourceModule.Holder.TransferObject.ID.Value;
                    targetId = target.ID;
                }
                else if (source is ICardARMModule && target is ICCModule)
                {
                    var sourceModule = source as ICardARMModule;
                    var targetModule = target as ICCModule;

                    sourceId = sourceModule.Holder.TransferObject.ID.Value;
                    targetId = target.ID;
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Card change source and target is invalid.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    return false;
                }

                if (sourceId == null | targetId == null)
                {
                    return false;
                }
                #endregion

                #region [Step2] Map Slicer로 Map을 생성하고 Excute 하는 부분
                SetTransfer(sourceId, targetId);

                var mapSlicer = new LoaderMapSlicer();
                var slicedMap = mapSlicer.ManualSlicing(cardTransferMap);

                if (slicedMap != null)
                {
                    bool isError = false;

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        try
                        {
                            loader.SetRequest(slicedMap[i]);

                            while (true)
                            {
                                if (loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    loader.ClearRequestData();

                                    Thread.Sleep(100);

                                    break;
                                }
                                else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    isError = true;
                                    break;
                                }

                                Thread.Sleep(100);
                            }
                            if (isError)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                        catch (Exception err)
                        {
                            isError = false;
                            LoggerManager.Exception(err);
                            break;
                        }

                    }

                    if (isError)
                    {
                        return false;
                    }

                    Thread.Sleep(33);
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    return false;
                }

                return true;
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        /// <summary>
        /// Wafer Transfer 하는 함수
        /// </summary>
        /// <param name="ocrmode">ICCModule 또는 ICardBufferModule</param>
        /// <param name="loc2">ICCModule 또는 ICardBufferModule</param>
        /// <returns>Wafer Change Map 생성, Handling 동작</returns>
        public AutoFeedResult TransferWaferObjectFunc(OCRModeEnum ocrmode, IWaferOwnable loc1, IWaferOwnable loc2, string waferid) 
        {
            AutoFeedResult transferSuccess = AutoFeedResult.UNDEFINED;
            bool isLoad = false;
            try
            {
                #endregion
                // [TODO] source target으로 받아서 Load, Unload 구분하기!!
                #region [Step1] Source 정보와 Target 정보를 이용하여 ID 받아오는 부분
                var loader = _Loader;
                waferTransferMap = loader.GetLoaderInfo().StateMap;

                string sourceId = null;
                string exchangeSourceID = null;
                
                ModuleID targetModuleID = new ModuleID();
                ModuleID exchangetargetModuleID = new ModuleID();

                IWaferOwnable source_loc = null;
                IWaferOwnable target_loc = null;

                HolderModuleInfo avaPA = waferTransferMap.PreAlignModules.FirstOrDefault(p => p.WaferStatus == EnumSubsStatus.NOT_EXIST & p.Enable == true);

                if (loc1 is ISlotModule && loc2 is IFixedTrayModule)
                {
                    source_loc = loc1;
                    target_loc = loc2;
                }
                else if (loc1 is IFixedTrayModule && loc2 is ISlotModule)
                {
                    source_loc = loc2;
                    target_loc = loc1;
                }

                EnumSubsStatus loc1_status = (source_loc as ISlotModule).Holder.Status;
                EnumSubsStatus loc2_status = (target_loc as IFixedTrayModule).Holder.Status;

                if (loc1_status == EnumSubsStatus.EXIST && loc2_status == EnumSubsStatus.NOT_EXIST ||
                    loc1_status == EnumSubsStatus.EXIST && loc2_status == EnumSubsStatus.EXIST)
                {
                    // load or exchange
                    source_loc.Holder.TransferObject.OCRMode.Value = ocrmode;
                    source_loc.Holder.TransferObject.OCR.Value = waferid;
                    source_loc.Holder.TransferObject.WaferType.Value = EnumWaferType.POLISH;

                    source_loc.Holder.TransferObject.PolishWaferInfo.OCRConfigParam = this.DeviceManager().GetPolishWaferInformation(target_loc.ID)?.OCRConfigParam;
                    source_loc.Holder.TransferObject.OCRAngle.Value = source_loc.Holder.TransferObject.PolishWaferInfo.OCRConfigParam.OCRAngle;
                    source_loc.Holder.TransferObject.OCRDevParam = source_loc.Holder.TransferObject.PolishWaferInfo.OCRConfigParam;

                    var sourceModule = source_loc as ISlotModule;

                    sourceId = sourceModule.Holder.TransferObject.ID.Value;
                    targetModuleID = avaPA.ID;
                    isLoad = true;
                }
                else if (loc1_status == EnumSubsStatus.NOT_EXIST && loc2_status == EnumSubsStatus.EXIST)
                {
                    target_loc.Holder.TransferObject.PolishWaferInfo.OCRConfigParam = this.DeviceManager().GetPolishWaferInformation(target_loc.ID)?.OCRConfigParam;
                    target_loc.Holder.TransferObject.OCRAngle.Value = target_loc.Holder.TransferObject.PolishWaferInfo.OCRConfigParam.OCRAngle;
                    target_loc.Holder.TransferObject.OCRDevParam = target_loc.Holder.TransferObject.PolishWaferInfo.OCRConfigParam;
                    // unload
                    target_loc.Holder.TransferObject.SetOCRState(target_loc.Holder.TransferObject.OCR.Value, 0, OCRReadStateEnum.NONE);
                    target_loc.Holder.TransferObject.OCRMode.Value = ocrmode;
                    var sourceModule = target_loc as IFixedTrayModule;

                    sourceId = sourceModule.Holder.TransferObject.ID.Value;
                    targetModuleID = source_loc.ID;
                    isLoad = false;
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Wafer change source and target is invalid.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    return AutoFeedResult.FAILURE;
                }

                if (sourceId == null)
                {
                    return AutoFeedResult.FAILURE;
                }
                #endregion
                // Load 인 경우 SLOT -> PA
                // Unload 인 경우 FIXED -> SLOT
                #region [Step2] Map Slicer로 Map을 생성하고 Excute 하는 부분
                WaferChangeSetTransfer(sourceId, targetModuleID, exchangeSourceID, exchangetargetModuleID);

                var mapSlicer = new LoaderMapSlicer();
                var slicedMap = mapSlicer.ManualSlicing(waferTransferMap);

                if (slicedMap != null)
                {
                    bool isError = false;

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        try
                        {
                            loader.SetRequest(slicedMap[i]);

                            while (true)
                            {
                                if (loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    loader.ClearRequestData();

                                    Thread.Sleep(100);

                                    break;
                                }
                                else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    LoaderRecoveryControlVM.Show(cont, loader.ResonOfError, loader.ErrorDetails);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";
                                    isError = true;
                                    break;
                                }

                                Thread.Sleep(100);
                            }
                            if (isError)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                        catch (Exception err)
                        {
                            isError = false;
                            LoggerManager.Exception(err);
                            break;
                        }

                    }

                    if (isError)
                    {
                        return AutoFeedResult.FAILURE;
                    }
                    else
                    {
                        transferSuccess = AutoFeedResult.SUCCESS;
                    }

                    Thread.Sleep(33);
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    return AutoFeedResult.FAILURE;
                }
                #endregion

                #region [Step4] Load 인 경우 PA -> FIXED, Exchange 인 경우 PA -> FIXED, FIXED -> SLOT
                waferTransferMap = loader.GetLoaderInfo().StateMap;
                sourceId = null;
                targetModuleID = new ModuleID();
                bool isOCRFail = false;
                
                if (isLoad)
                {
                    var pa = _Loader.ModuleManager.FindModule(ModuleTypeEnum.PA, avaPA.ID.Index) as IPreAlignModule;

                    if (pa.Holder.TransferObject.OCRReadState == OCRReadStateEnum.DONE)
                    {
                        var targetModule = target_loc as IFixedTrayModule;

                        sourceId = avaPA.Substrate.ID.Value;

                        if (targetModule.Holder.TransferObject != null)
                        {
                            // Target에 이미 웨이퍼가 있다. Exchange 인 경우
                            exchangetargetModuleID = source_loc.ID;
                            exchangeSourceID = targetModule.Holder.TransferObject.ID.Value;
                        }

                        targetModuleID = target_loc.ID;

                    }
                    else
                    {
                        // OCR 실패 했으면 집으로 되돌아가렴
                        var targetModule = source_loc as ISlotModule;

                        sourceId = avaPA.Substrate.ID.Value;

                        if (targetModule.Holder.TransferObject != null)
                        {
                            // Target에 이미 웨이퍼가 있다. Exchange 인 경우
                            exchangetargetModuleID = source_loc.ID;
                            exchangeSourceID = targetModule.Holder.TransferObject.ID.Value;
                        }

                        targetModuleID = source_loc.ID;

                        isOCRFail = true;
                    }
                }
                else
                {
                    // unload
                    return AutoFeedResult.SUCCESS;
                }

                if (sourceId == null)
                {
                    return AutoFeedResult.FAILURE;
                }
                #endregion
                #region [Step5] Map Slicer로 Map을 생성하고 Excute 하는 부분
                WaferChangeSetTransfer(sourceId, targetModuleID, exchangeSourceID, exchangetargetModuleID);

                slicedMap = mapSlicer.ManualSlicing(waferTransferMap);

                if (slicedMap != null)
                {
                    bool isError = false;

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        try
                        {
                            loader.SetRequest(slicedMap[i]);

                            while (true)
                            {
                                if (loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    loader.ClearRequestData();

                                    Thread.Sleep(100);

                                    break;
                                }
                                else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    LoaderRecoveryControlVM.Show(cont, loader.ResonOfError, loader.ErrorDetails);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";

                                    isError = true;
                                    break;
                                }

                                Thread.Sleep(100);
                            }
                            if (isError)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                        catch (Exception err)
                        {
                            isError = false;
                            LoggerManager.Exception(err);
                            break;
                        }

                    }

                    if (isError)
                    {
                        transferSuccess = AutoFeedResult.FAILURE;
                    }
                    else
                    {
                        transferSuccess = AutoFeedResult.SUCCESS;
                    }

                    Thread.Sleep(33);
                }
                else
                {
                    var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    transferSuccess = AutoFeedResult.FAILURE;
                }
                #endregion

                if (isOCRFail)
                {
                    transferSuccess = AutoFeedResult.SKIPPED;
                }
            }
            catch (Exception err)
            {
                transferSuccess = AutoFeedResult.FAILURE;
                LoggerManager.Exception(err);
            }
            return transferSuccess;
        }


        public List<(IWaferOwnable sourceModule, IWaferOwnable targetModule)> GetLoadModules(IWaferOwnable tloc1, IWaferOwnable tloc2)
        {
            List<(IWaferOwnable sourceModule, IWaferOwnable targetModule)> retVal = new List<(IWaferOwnable sourceModule, IWaferOwnable targetModule)>();
            try
            {
                if (tloc1.Holder.Status == EnumSubsStatus.EXIST && tloc2.Holder.Status == EnumSubsStatus.EXIST)
                {
                    retVal.Add((tloc2, tloc1));
                    retVal.Add((tloc1, tloc2));
                }
                else if (tloc1.Holder.Status == EnumSubsStatus.EXIST && tloc2.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    retVal.Add((tloc1, tloc2));
                }
                else if (tloc1.Holder.Status == EnumSubsStatus.NOT_EXIST && tloc2.Holder.Status == EnumSubsStatus.EXIST)
                {
                    retVal.Add((tloc2, tloc1));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetPolishWaferInfoByLoadModule(IWaferOwnable tloc1, IWaferOwnable tloc2)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                List<(IWaferOwnable sourceModule, IWaferOwnable targetModule)> getModules = this.GetLoadModules(tloc1, tloc2);
                if (getModules != null)
                {
                    foreach (var item in getModules)
                    {
                        string prev_pwinfo = string.Empty;
                        if (item.sourceModule.Holder.TransferObject.PolishWaferInfo != null)
                        {
                            prev_pwinfo = item.sourceModule.Holder.TransferObject?.PolishWaferInfo?.DefineName.Value ?? string.Empty;
                        }

                        var info = this.DeviceManager().GetPolishWaferInformation(item.targetModule.ID) as PolishWaferInformation;
                        if (info != null)
                        {
                            if(prev_pwinfo != info.DefineName.Value)
                            {
                                LoggerManager.Debug($"[LoaderSupervisor] SetPolishWaferInfoByModule(): Polish wafer Info Changed. {item.sourceModule.ModuleType}.{item.sourceModule.ID.Index} define name:({prev_pwinfo} -> {info})");
                            }
                            item.sourceModule.Holder.TransferObject.PolishWaferInfo = info;
                            item.sourceModule.Holder.TransferObject.PolishWaferInfo.TouchCount.Value = 0;
                            item.sourceModule.Holder.TransferObject.PolishWaferInfo.CurrentAngle = info.NotchAngle;
                            item.sourceModule.Holder.TransferObject.PolishWaferInfo.Priorty = info.Priorty;
                        }
}

                    retVal = EventCodeEnum.NONE;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        /// <summary>
        /// Map을 생성하기 전 TransferObject를 Tranfer 하기 위해 세팅하는 함수
        /// </summary>
        /// <param name="id">Source Module의 ID</param>
        /// <param name="destinationID">Target Module의 ID</param>
        private void SetTransfer(string id, ModuleID destinationID)
        {
            TransferObject subObj = cardTransferMap.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            ModuleInfoBase dstLoc = cardTransferMap.GetLocationModules().Where(item => item.ID == destinationID).FirstOrDefault();
            bool stageBusy = true;
            if (subObj == null)
            {
                cardTransferMap = null;
                return;
            }

            if (dstLoc == null)
            {
                cardTransferMap = null;
                return;
            }

            if (subObj.CurrPos == destinationID)
            {
                cardTransferMap = null;
                return;
            }

            if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK ||
                subObj.CurrPos.ModuleType == ModuleTypeEnum.CC)

            {
                stageBusy = GetClient(subObj.CurrPos.Index).GetRunState();
                if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK && stageBusy == false)
                {
                    bool needtorecovery = false;
                    ModuleStateEnum wafertransferstate = ModuleStateEnum.ABORT;
                    var ret = GetClient(subObj.CurrPos.Index).CanWaferUnloadRecovery(ref needtorecovery,
                        ref wafertransferstate);
                    if (ret == EventCodeEnum.NONE)
                    {
                        if (needtorecovery && wafertransferstate == ModuleStateEnum.ERROR)
                        {
                            stageBusy = true;
                        }
                    }
                }
            }
            else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK ||
                dstLoc.ID.ModuleType == ModuleTypeEnum.CC)
            {
                ILoaderServiceCallback callback = GetClient(dstLoc.ID.Index);

                if (callback != null)
                {
                    stageBusy = callback.GetRunState();
                }
                else
                {
                    LoggerManager.Debug($"[LoaderSupervisor], SetTransfer() : COM ERROR");
                    stageBusy = false;
                }
            }

            if (stageBusy == false)
            {
                var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{subObj.CurrPos.Index} is Busy Right Now", EnumMessageStyle.Affirmative).Result;

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    return;
                }
            }

            var card = _Loader.ModuleManager.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();

            if (card != null)
            {
                card.CardSkip = ProberInterfaces.Enum.CardSkipEnum.NONE;
            }

            if (dstLoc is HolderModuleInfo)
            {
                var currHolder = cardTransferMap.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                var dstHolder = dstLoc as HolderModuleInfo;

                subObj.PrevHolder = subObj.CurrHolder;
                subObj.PrevPos = subObj.CurrPos;

                subObj.CurrHolder = destinationID;
                subObj.CurrPos = destinationID;

                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                {
                    if (_Loader.LoaderMaster.ClientList.ContainsKey(dstLoc.ID.ToString()))
                    {
                        var deviceInfo = _Loader.LoaderMaster.GetDeviceInfoClient(_Loader.LoaderMaster.ClientList[dstLoc.ID.ToString()]);
                        if (deviceInfo != null)
                        {
                            subObj.NotchAngle.Value = deviceInfo.NotchAngle.Value;
                        }
                    }
                    else
                    {

                    }
                }
                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                {
                    subObj.NotchAngle.Value = (_Loader.ModuleManager.FindModule(dstLoc.ID) as ISlotModule).Cassette.Device.LoadingNotchAngle.Value;
                }

                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;
                dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                dstHolder.Substrate = subObj;
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
            }
        }


        private void WaferChangeSetTransfer(string id, ModuleID destinationID, string exchangeSourceID, ModuleID exchangeTargetID)
        {
            TransferObject subObj = waferTransferMap.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            TransferObject exchangeSubObj = waferTransferMap.GetTransferObjectAll().Where(item => item.ID.Value == exchangeSourceID).FirstOrDefault();
            ModuleInfoBase dstLoc = waferTransferMap.GetLocationModules().Where(item => item.ID == destinationID).FirstOrDefault();

            if (subObj == null)
            {
                waferTransferMap = null;
                return;
            }

            if (dstLoc == null)
            {
                waferTransferMap = null;
                return;
            }

            if (subObj.CurrPos == destinationID)
            {
                waferTransferMap = null;
                return;
            }

            if (dstLoc is HolderModuleInfo)
            {
                if (string.IsNullOrEmpty(exchangeSourceID))
                {
                    var currHolder = waferTransferMap.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                    var dstHolder = dstLoc as HolderModuleInfo;

                    subObj.PrevHolder = subObj.CurrHolder;
                    subObj.PrevPos = subObj.CurrPos;

                    subObj.CurrHolder = destinationID;
                    subObj.CurrPos = destinationID;

                    currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                    currHolder.Substrate = null;
                    dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                    dstHolder.Substrate = subObj;
                }
                else
                {
                    // exchange
                    var sourceHolder = waferTransferMap.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                    var targetHolder = waferTransferMap.GetHolderModuleAll().Where(item => item.ID == exchangeSubObj.CurrHolder).FirstOrDefault();

                    //backup
                    subObj.PrevHolder = subObj.CurrHolder;
                    subObj.PrevPos = subObj.CurrPos;

                    exchangeSubObj.PrevHolder = exchangeSubObj.CurrHolder;
                    exchangeSubObj.PrevPos = exchangeSubObj.CurrPos;

                    // create exchange map
                    subObj.CurrHolder = destinationID;          //Source -> Target
                    subObj.CurrPos = destinationID;

                    exchangeSubObj.CurrHolder = exchangeTargetID;     //Target -> Source
                    exchangeSubObj.CurrPos = exchangeTargetID;

                    sourceHolder.WaferStatus = EnumSubsStatus.EXIST;
                    sourceHolder.Substrate = exchangeSubObj;

                    targetHolder.WaferStatus = EnumSubsStatus.EXIST;
                    targetHolder.Substrate = subObj;
                }
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
            }
        }

        public EventCodeEnum WriteWaitHandle(short value)
        {
            return this.GetGPLoader().WriteWaitHandle(value);
        }

        public EventCodeEnum WaitForHandle(short handle, long timeout = 60000)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {

                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool commandDone = false;
                bool runFlag = true;
                do
                {
                    var handleState = this.GetGPLoader().ReadWaitHandle();
                    if (handleState == (short)handle)
                    {
                        commandDone = true;
                    }
                    if (commandDone == true)
                    {
                        if (handleState == (short)handle)
                        {
                            runFlag = false;
                            errorCode = EventCodeEnum.NONE;
                        }
                    }
                    if (timeout != 0)
                    {
                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                        {
                            runFlag = false;
                            LoggerManager.Debug($"WaitForHandle(): Timeout occurred. Target state = {handle}, Curr. State = {handleState}, Timeout = {timeout}");
                            errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                        }
                    }
                } while (runFlag == true);


            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForHandle(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
            //return this.GetGPLoader().WaitForHandle(handle, timeout);
        }

        public int ReadWaitHandle()
        {
            int handle = -1;
            try
            {
                handle = this.GetGPLoader().ReadWaitHandle();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ReadWaitHandle(): Error occurred. Err = {err.Message}");
            }
            return handle;
        }

        public EventCodeEnum IsShutterClose(int cellIdx)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                //Wafer Chuck Pos;
                bool chuck_isClose = false;
                bool card_isClose = false;
                bool isWaferOut = false;
                var slotSize = Loader.DeviceSize;
                if (slotSize == SubstrateSizeEnum.UNDEFINED)
                {
                    slotSize = SubstrateSizeEnum.INCH12;
                }
                double diffOffset_X = 0;
                double diffOffset_Z = 0;
                double diffOffset_W = 0;
                double UPosLimit = 0;
                if (Loader.SystemParameter.DangerousPosOffset_LX.Value == 0)
                {
                    Loader.SystemParameter.DangerousPosOffset_LX.Value = 15000;
                }
                if (Loader.SystemParameter.DangerousPosOffset_LZM.Value < 50000)
                {
                    Loader.SystemParameter.DangerousPosOffset_LZM.Value = 50000;
                }
                if (Loader.SystemParameter.DangerousPosOffset_LW.Value == 0)
                {
                    Loader.SystemParameter.DangerousPosOffset_LW.Value = 10000;
                }
                if (Loader.SystemParameter.DangerousPos_ARM_Limit.Value == 0)
                {
                    Loader.SystemParameter.DangerousPos_ARM_Limit.Value = 5000;
                }


                diffOffset_X = Loader.SystemParameter.DangerousPosOffset_LX.Value;
                diffOffset_Z = Loader.SystemParameter.DangerousPosOffset_LZM.Value;
                diffOffset_W = Loader.SystemParameter.DangerousPosOffset_LW.Value;
                UPosLimit = Loader.SystemParameter.DangerousPos_ARM_Limit.Value;

                var LX = Loader.MotionManager.GetAxis(EnumAxisConstants.LX);
                var LZM = Loader.MotionManager.GetAxis(EnumAxisConstants.LZM);
                var LW = Loader.MotionManager.GetAxis(EnumAxisConstants.LW);
                var LUD = Loader.MotionManager.GetAxis(EnumAxisConstants.LUD);
                var LUU = Loader.MotionManager.GetAxis(EnumAxisConstants.LUU);
                var LCC = Loader.MotionManager.GetAxis(EnumAxisConstants.LCC);

                double chuckLoading_X = 0;
                double chuckLoading_Z = 0;
                double chuckLoading_W = 0;
                double cardLoading_X = 0;
                double cardLoading_Z = 0;
                double cardLoading_W = 0;
                var chuckAccPos = Loader.SystemParameter.ChuckModules[cellIdx - 1].AccessParams.FirstOrDefault(i => i.SubstrateSize.Value == slotSize);
                if (chuckAccPos != null)
                {
                    chuckLoading_X = chuckAccPos.Position.LX.Value;
                    chuckLoading_Z = chuckAccPos.Position.A.Value;
                    chuckLoading_W = chuckAccPos.Position.W.Value;
                }
                else
                {
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, ChuckAccPos({slotSize}) is NULL");
                }

                var CardAccPos = Loader.SystemParameter.CCModules[cellIdx - 1].AccessParams.FirstOrDefault(i => i.SubstrateSize.Value == slotSize);
                if (CardAccPos != null)
                {
                    cardLoading_X = CardAccPos.Position.LX.Value;
                    cardLoading_Z = CardAccPos.Position.A.Value;
                    cardLoading_W = CardAccPos.Position.W.Value;
                }
                else
                {
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, CardAccPos({slotSize}) is NULL");
                }


                if (LUD.Status.Position.Actual >= UPosLimit || LUU.Status.Position.Actual >= UPosLimit || LCC.Status.Position.Actual >= UPosLimit)
                {
                    if (LX.Status.Position.Actual >= chuckLoading_X - diffOffset_X
                     && LX.Status.Position.Actual <= chuckLoading_X + diffOffset_X
                     && LZM.Status.Position.Actual >= chuckLoading_Z - diffOffset_Z
                     && LZM.Status.Position.Actual <= chuckLoading_Z + diffOffset_Z
                     && LW.Status.Position.Actual >= chuckLoading_W - diffOffset_W
                     && LW.Status.Position.Actual <= chuckLoading_W + diffOffset_W)
                    {
                        chuck_isClose = false;
                    }
                    else
                    {
                        chuck_isClose = true;
                    }

                    if (LX.Status.Position.Actual >= cardLoading_X - diffOffset_X
                    && LX.Status.Position.Actual <= cardLoading_X + diffOffset_X
                    && LZM.Status.Position.Actual >= cardLoading_Z - diffOffset_Z
                    && LZM.Status.Position.Actual <= cardLoading_Z + diffOffset_Z
                    && LW.Status.Position.Actual >= cardLoading_W - diffOffset_W
                    && LW.Status.Position.Actual <= cardLoading_W + diffOffset_W)
                    {
                        card_isClose = false;
                    }
                    else
                    {
                        card_isClose = true;
                    }
                }
                else
                {
                    chuck_isClose = true;
                    card_isClose = true;
                }

                // WaferOut Sensor 체크
                if (LD_WAFER_OUT_SEONSOR != null && LD_WAFER_OUT_SEONSOR.IOOveride.Value == EnumIOOverride.NONE)
                {
                    bool value = false;
                    Loader.IOManager.ReadIO(LD_WAFER_OUT_SEONSOR, out value);   // 현재 IO 체크 (true or false)

                    if (value == true)
                    {
                        // true : Wafer가 튀어 나와 있다.
                        var result = Loader.IOManager.MonitorForIO(LD_WAFER_OUT_SEONSOR, false, LD_WAFER_OUT_SEONSOR.MaintainTime.Value, LD_WAFER_OUT_SEONSOR.TimeOut.Value);  // false 인지 체크.
                        if (result != EventCodeEnum.NONE)
                        {
                            // false가 아니고 true라는 뜻. 웨이퍼 튀어 나온 것 감지 됨.
                            this.MetroDialogManager().ShowMessageDialog("Shutter Door Close Failed", $"The wafer out sensor is detected and the shutter door of cell {cellIdx} cannot be closed.", EnumMessageStyle.Affirmative);
                            LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, Wafer out sensor detected");
                            isWaferOut = true;
                        }
                        else
                        {
                            LoggerManager.Debug($"IsShutterClose invalid IO LD_WAFER_OUT_SEONSOR Value {LD_WAFER_OUT_SEONSOR}");
                        }
                    }
                }

                if (chuck_isClose && card_isClose && !isWaferOut)
                {
                    errorCode = EventCodeEnum.NONE;
                }
                else
                {
                    errorCode = EventCodeEnum.ARM_DANGEROUS_POS;
                }

                LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, Check Wafer Loading Position:{chuck_isClose} , Check Card Loading Position: {card_isClose}");
                LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, Check ARM Position (Limit:{UPosLimit}) [ LUD: {LUD.Status.Position.Actual} , LUU: {LUU.Status.Position.Actual} , LCC: {LCC.Status.Position.Actual}");
                LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, Check Position LX (Offset:{diffOffset_X} ) [LX: {LX.Status.Position.Actual} , WaferLoading LX:{chuckLoading_X} , CardLoading LX:{cardLoading_X} ");
                LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, Check Position LZ (Offset:{diffOffset_Z} ) [LZ: {LZM.Status.Position.Actual} , WaferLoading LZ:{chuckLoading_Z} , CardLoading LZ:{cardLoading_Z} ");
                LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, Check Position LW (Offset:{diffOffset_W} ) [LW: {LW.Status.Position.Actual} , WaferLoading LW:{chuckLoading_W} , CardLoading LW:{cardLoading_W} ");


                if (!chuck_isClose)
                {
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, LX (Offset:{diffOffset_X} ) [LX: {LX.Status.Position.Actual} , WaferLoading LX:{chuckLoading_X} (low: {chuckLoading_X - diffOffset_X}, high:{chuckLoading_X + diffOffset_X}) ");
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, LZ (Offset:{diffOffset_Z} ) [LX: {LZM.Status.Position.Actual} , WaferLoading LZ:{chuckLoading_Z} (low: {chuckLoading_Z - diffOffset_Z}, high:{chuckLoading_Z + diffOffset_Z}) ");
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, LW (Offset:{diffOffset_W} ) [LX: {LW.Status.Position.Actual} , WaferLoading LW:{chuckLoading_W} (low: {chuckLoading_W - diffOffset_W}, high:{chuckLoading_W + diffOffset_W}) ");
                }


                if (!card_isClose)
                {
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, LX (Offset:{diffOffset_X} ) [LX: {LX.Status.Position.Actual} , CardLoading LX:{cardLoading_X} (low: {cardLoading_X - diffOffset_X}, high:{cardLoading_X + diffOffset_X}) ");
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, LZ (Offset:{diffOffset_Z} ) [LX: {LZM.Status.Position.Actual} , CardLoading LZ:{cardLoading_Z} (low: {cardLoading_Z - diffOffset_Z}, high:{cardLoading_Z + diffOffset_Z}) ");
                    LoggerManager.Debug($"IsShutterClose() Cell Index:{cellIdx}, LW (Offset:{diffOffset_W} ) [LX: {LW.Status.Position.Actual} , CardLoading LW:{cardLoading_W} (low: {cardLoading_W - diffOffset_W}, high:{cardLoading_W + diffOffset_W}) ");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsShutterClose(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
            }
            return errorCode;
        }

        public void SetStopBeforeProbingFlag(int stageIdx, bool flag)
        {
            StopBeforeProbingDictionary[stageIdx] = flag;
        }

        public void SetStopAfterProbingFlag(int stageIdx, bool flag)
        {
            StopAfterProbingDictionary[stageIdx] = flag;
        }

        public bool GetStopBeforeProbingFlag(int stageIdx)
        {
            bool retVal = false;

            retVal = StopBeforeProbingDictionary[stageIdx];

            return retVal;
        }

        public bool GetStopAfterProbingFlag(int stageIdx)
        {
            bool retVal = false;

            retVal = StopAfterProbingDictionary[stageIdx];

            return retVal;
        }

        public void IsAlignDoing(ref bool pinAlignDoing, ref bool waferAlignDoing)
        {
            try
            {
                ILoaderServiceCallback client = this.GetClient(loaderCommunicationManager.SelectedStageIndex);
                client.IsAlignDoing(ref pinAlignDoing, ref waferAlignDoing);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"StageMode(): Error occurred. Failed to get soaking status from stage. Err = {err.Message}");
            }
        }

        public async Task<(EventCodeEnum,string)> RecoveryUnknownStatus(ModuleTypeEnum moduleType, int moduleNum, EnumSubsStatus status)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IWaferOwnable ownableModule = null;
            bool iswaferhold = false;
            string Errormsg = "";
            try
            {
                var Module = Loader.ModuleManager.FindModule(moduleType, moduleNum);
                if (Module is IWaferOwnable)
                {
                    ownableModule = Module as IWaferOwnable;
                    if (ownableModule != null)
                    {
                        if (moduleType == ModuleTypeEnum.CHUCK)
                        {
                            var client = GetClient(moduleNum);

                            if (IsAliveClient(client))
                            {
                                retVal = client.ChangeWaferStatus(status, out iswaferhold, out Errormsg);
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }

                        if (status == EnumSubsStatus.NOT_EXIST)
                        {
                            if (retVal == EventCodeEnum.NONE)
                            {
                                if (iswaferhold)
                                {
                                    ownableModule.Holder.SetAllocate();
                                    ownableModule.Holder.IsWaferOnHandler = true;
                                    retVal = EventCodeEnum.UNDEFINED;
                                }
                                else
                                {
                                    if (moduleType == ModuleTypeEnum.CHUCK)
                                    {
                                        var stage = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>(moduleNum);
                                        if (stage != null)
                                        {
                                            bool ret = stage.CheckUsingHandler(moduleNum);
                                            if (ret == true)
                                            {
                                                var result = await this.MetroDialogManager().ShowMessageDialog("Warning Message", "It may not have been recognized on the sensor. Remove wafer as a manual and click the ok button.", EnumMessageStyle.AffirmativeAndNegative);
                                                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                                                {
                                                    SkipUnknownWaferLocation(moduleType, moduleNum);
                                                    retVal = GetClient(moduleNum).ClearHandlerStatus();
                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        ownableModule.Holder.SetUnload();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ownableModule.Holder.SetUnload();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        retVal = ownableModule.IsWaferonmodule(out bool isonwafer);
                                        if (retVal == EventCodeEnum.NONE && isonwafer == false || Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                        {
                                            ownableModule.Holder.SetUnload();
                                        }
                                        else 
                                        {
                                            LoggerManager.Debug($"[RecoveryUnknownStatus] ModuleType:{moduleType}{moduleNum} retVal:{retVal}, result{isonwafer}");
                                            retVal = EventCodeEnum.UNDEFINED;
                                        }
                                    }
                                }
                            }

                        }
                        else if (status == EnumSubsStatus.EXIST)
                        {
                            if (retVal == EventCodeEnum.NONE)
                            {
                                bool cannotallocate = false;
                                if (ownableModule.Holder.BackupTransferObject != null )
                                {
                                    cannotallocate = Loader.GetLoaderInfo().StateMap.GetHolderModuleAll().Any(obj => obj.WaferStatus == EnumSubsStatus.EXIST &&
                                    obj.Substrate.OriginHolder.Label == ownableModule.Holder.BackupTransferObject.OriginHolder.Label &&
                                    (obj.ID.ModuleType == ModuleTypeEnum.CHUCK || obj.ID.ModuleType == ModuleTypeEnum.BUFFER ||
                                    obj.ID.ModuleType == ModuleTypeEnum.PA || obj.ID.ModuleType == ModuleTypeEnum.ARM ||
                                    obj.ID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY || obj.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY));

                                }

                                if (!cannotallocate)
                                {
                                    ownableModule.Holder.SetAllocate();
                                    if (iswaferhold)
                                    {
                                        ownableModule.Holder.IsWaferOnHandler = true;
                                    }
                                }
                                else 
                                {
                                    retVal = EventCodeEnum.WAFER_NOT_EXIST_EROOR;
                                    string sameorignmodulelabel = "";
                                    var sameorignmodule = Loader.GetLoaderInfo().StateMap.GetHolderModuleAll().Where(obj => obj.WaferStatus == EnumSubsStatus.EXIST &&
                                    obj.Substrate.OriginHolder.Label == ownableModule.Holder.BackupTransferObject.OriginHolder.Label).FirstOrDefault();
                                    if (sameorignmodule != null)
                                    {
                                        sameorignmodulelabel = sameorignmodule.Substrate.CurrHolder.Label;
                                    }
                                    Errormsg = $"An identical wafer with the same origin exists internally.\nCurrent Module: {moduleType}{moduleNum}Identical Module: {sameorignmodulelabel}";
                                }
                            }
                        }
                    }

                }
                LoggerManager.Debug($"[RecoveryUnknownStatus] ModuleType:{moduleType}{moduleNum} Status:{status}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RecoveryUnknownStatus(): Error occurred. Err = {err.Message}");
            }
            finally
            {
                Loader.BroadcastLoaderInfo();
            }

            return (retVal,Errormsg);
        }

        public EventCodeEnum SkipUnknownWaferLocation(ModuleTypeEnum moduleType, int moduleNum)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IWaferOwnable ownableModule = null;
            try
            {
                LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : ModuleType is {moduleType} , [{moduleNum}]");
                var Module = Loader.ModuleManager.FindModule(moduleType, moduleNum);
                if (Module is IWaferOwnable)
                {
                    ownableModule = Module as IWaferOwnable;
                    if (ownableModule != null)
                    {
                        if (ownableModule.Holder?.TransferObject != null)
                        {
                            var slot = Loader.ModuleManager.FindModule<ISlotModule>(ModuleTypeEnum.SLOT, ownableModule.Holder.TransferObject.OriginHolder.Index);
                            if (slot != null)
                            {
                                if (ActiveLotInfos[slot.Cassette.ID.Index - 1].State != LotStateEnum.Idle)
                                {
                                    if (slot.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                                        ownableModule.Holder.TransferObject.WaferType.Value == EnumWaferType.STANDARD &&
                                        ownableModule.Holder.Status == EnumSubsStatus.UNKNOWN)
                                    {
                                        ownableModule.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                        ownableModule.Holder.SetTransfered(slot);
                                        LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : wafer obj {moduleType} -> slot ");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : slot Status [{slot.Holder.Status}] {moduleType} Status: {ownableModule.Holder.Status} wafer type {ownableModule.Holder.TransferObject.WaferType.Value.ToString()}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : Lot[{slot.Cassette.ID.Index}] state: {ActiveLotInfos[slot.Cassette.ID.Index - 1].State}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : slot is null");
                            }
                        }
                        else
                        {
                            if (ownableModule.Holder == null) 
                            {
                                LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : {moduleType}.Holder is null");
                            }
                            else 
                            {
                                LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : {moduleType}.Holder.TransferObject is null");
                            }
                        }
                        ownableModule.Holder.SetUnload();
                    }
                }
                else
                {
                    LoggerManager.Debug($"[LOADER] SkipUnknownWaferLocation() : This module does not inherit from IWaferOwnable.");
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public TransferObject GetTransferObjectToSlotInfo(int cellNum)
        {
            TransferObject transferObject = null;
            try
            {
                transferObject = GetClient(cellNum).GetTransferObjectToSlotInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RecoveryUnknownStatus(): Error occurred. Err = {err.Message}");
            }
            return transferObject;
        }

        public void SetMonitoringBehavior(List<IMonitoringBehavior> monitoringBehaviors, int stageIdx)
        {
            try
            {
                CellsInfo[stageIdx - 1].MonitoringBehaviorList = monitoringBehaviors;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<IMonitoringBehavior> GetMonitoringBehaviorFromClient(int stageIdx)
        {
            List<IMonitoringBehavior> MonitoringBehaviorList = new List<IMonitoringBehavior>();
            try
            {
                if (stageIdx - 1 >= 0)
                {
                    ILoaderServiceCallback client = this.GetClient(stageIdx);
                    byte[] serialize = client?.GetMonitoringBehaviorFromClient();
                    if (serialize != null)
                    {
                        object obj = this.ByteArrayToObjectSync(serialize);

                        if (obj != null)
                        {
                            MonitoringBehaviorList = obj as List<IMonitoringBehavior>;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return MonitoringBehaviorList;
        }

        public void ManualRecoveryToStage(int stageIdx, int behaviorIndex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ILoaderServiceCallback client = this.GetClient(stageIdx + 1);
                if (client != null)
                {
                    ret = client.ManualRecoveryToStage(behaviorIndex);
                }
                else
                {
                    LoggerManager.Debug($"ManualRecoveryToStage() Client is Null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum GetLoaderEmergency()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool value = false;
                Loader.IOManager.ReadIO(EMGIO, out value);

                if (value == true)
                {
                    var result = Loader.IOManager.MonitorForIO(EMGIO, false, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.EMO_ERROR;
                        return retVal;
                    }
                }

                Loader.IOManager.ReadIO(MAINVAC, out value);
                if (value == false)
                {
                    var result = Loader.IOManager.MonitorForIO(MAINVAC, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.LOADER_MAIN_VAC_ERROR;
                        return retVal;
                    }
                }

                Loader.IOManager.ReadIO(MAINAIR, out value);
                if (value == false)
                {
                    var result = Loader.IOManager.MonitorForIO(MAINAIR, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.LOADER_MAIN_AIR_ERROR;
                        return retVal;
                    }
                }

                Loader.IOManager.ReadIO(MAINVAC_STAGE, out value);
                if (value == false)
                {
                    var result = Loader.IOManager.MonitorForIO(MAINVAC_STAGE, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.STAGE_MAIN_VAC_ERROR;
                        return retVal;
                    }
                }

                Loader.IOManager.ReadIO(MAINAIR_STAGE, out value);
                if (value == false)
                {
                    var result = Loader.IOManager.MonitorForIO(MAINAIR_STAGE, true, 100, 500);
                    if (result != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.STAGE_MAIN_AIR_ERROR;
                        return retVal;
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderEmergency(): Exception occurred. Err = {err.Message}");
            }

            return retVal;
        }

        private object[] validationFoupLoadedStateLocks;
        private object[] validationFoupUnLoadedStateLocks;

        /// <summary>
        /// TODO : Placement, Presnece 까지 확인하도록 추가 필요 하지만, foup size 에 따라서 처리 해야됨.
        /// </summary>
        public EventCodeEnum ValidationFoupLoadedState(int foupNumber, ref string stateStr)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if(validationFoupLoadedStateLocks == null)
                {
                    validationFoupLoadedStateLocks = new object[SystemModuleCount.ModuleCnt.FoupCount];
                    for (int index = 0; index < validationFoupLoadedStateLocks.Count(); index++)
                    {
                        validationFoupLoadedStateLocks[index] = new object();
                    }
                }
                lock (validationFoupLoadedStateLocks[foupNumber-1])
                {
                    int dpInReadBitRetVal = -1;
                    bool DPInFlag = false;
                    EnumCSTState cSTState = EnumCSTState.IDLE;
                    FoupStateEnum foupState = FoupStateEnum.UNDEFIND;

                    if (Loader != null)
                    {

                        var Cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupNumber);
                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            if (Cassette.FoupState == FoupStateEnum.LOAD)
                            {
                                cSTState = EnumCSTState.LOADED;
                                DPInFlag = true;
                            }
                            else if (Cassette.FoupState == FoupStateEnum.UNLOAD)
                            {
                                cSTState = EnumCSTState.UNLOADED;
                                DPInFlag = false;
                            }
                            dpInReadBitRetVal = 0;
                        }
                        else
                        {
                            cSTState = this.GPLoader.GetCSTState(foupNumber - 1);
                            dpInReadBitRetVal = this.GetFoupIO().ReadBit(this.GetFoupIO().IOMap.Inputs.DI_DP_INs[foupNumber - 1], out DPInFlag);
                        }

                        foupState = Cassette.FoupState;

                        if (dpInReadBitRetVal == 0)
                        {
                            if (foupState == FoupStateEnum.LOAD && cSTState == EnumCSTState.LOADED && DPInFlag)
                            {
                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                retVal = EventCodeEnum.FOUP_LOADED_STATE_VALIDATION_FAIL;
                            }
                        }

                        stateStr = $"Foup State: {foupState}, Cassette State : {cSTState}, Port In : {DPInFlag}";
                        //LoggerManager.Debug($"[Loadersupervisor] ValidationFoupLoadedState() : {stateStr} , DPInReadBitRetVal : {dpInReadBitRetVal}.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.FOUP_LOADED_STATE_VALIDATION_FAIL;
            }
            return retVal;

        }

        /// <summary>
        /// TODO : Placement, Presnece 까지 확인하도록 추가 필요 하지만, foup size 에 따라서 처리 해야됨.
        /// </summary>
        public EventCodeEnum ValidationFoupUnloadedState(int foupNumber, ref string stateStr)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (validationFoupUnLoadedStateLocks == null)
                {
                    validationFoupUnLoadedStateLocks = new object[SystemModuleCount.ModuleCnt.FoupCount];
                    for (int index = 0; index < validationFoupUnLoadedStateLocks.Count(); index++)
                    {
                        validationFoupUnLoadedStateLocks[index] = new object();
                    }
                }
                lock (validationFoupUnLoadedStateLocks[foupNumber - 1])
                {
                    int dpOutReadBitRetVal = -1;
                    int clampUnlockReadBitRetVal = -1;
                    bool DPOutFlag = false;
                    bool ClampUnlockFlag = false;
                    EnumCSTState cSTState = EnumCSTState.IDLE;
                    FoupStateEnum foupState = FoupStateEnum.UNDEFIND;

                    if (Loader != null)
                    {
                        var Cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupNumber);
                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            if (Cassette.FoupState == FoupStateEnum.LOAD)
                            {
                                cSTState = EnumCSTState.LOADED;
                                DPOutFlag = false;
                                ClampUnlockFlag = false;
                            }
                            else if (Cassette.FoupState == FoupStateEnum.UNLOAD)
                            {
                                cSTState = EnumCSTState.UNLOADED;
                                DPOutFlag = true;
                                ClampUnlockFlag = true;
                            }
                            dpOutReadBitRetVal = 0;
                            clampUnlockReadBitRetVal = 0;
                        }
                        else
                        {
                            cSTState = this.GPLoader.GetCSTState(foupNumber - 1);
                            dpOutReadBitRetVal = this.GetFoupIO().ReadBit(this.GetFoupIO().IOMap.Inputs.DI_DP_OUTs[foupNumber - 1], out DPOutFlag);
                            clampUnlockReadBitRetVal = this.GetFoupIO().ReadBit(this.GetFoupIO().IOMap.Inputs.DI_CST_UNLOCK12s[foupNumber - 1], out ClampUnlockFlag);
                        }

                        foupState = Cassette.FoupState;

                        if (dpOutReadBitRetVal == 0 && clampUnlockReadBitRetVal == 0)
                        {
                            if (((foupState == FoupStateEnum.UNLOAD && cSTState == EnumCSTState.UNLOADED) 
                                || foupState == FoupStateEnum.EMPTY_CASSETTE)
                            && ClampUnlockFlag
                            && DPOutFlag)
                            {
                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                retVal = EventCodeEnum.FOUP_UNLOADED_STATE_VALIDATION_FAIL;
                            }
                        }
                        stateStr = $"ValidationFoupUnloadedState(). Foup Number: {foupNumber}, Foup State: {foupState}, Cassette State : {cSTState}, DPOutFlag : {DPOutFlag}, DPOutReadBitRetVal : {dpOutReadBitRetVal}, ClampUnlockReadBitRetVal : {clampUnlockReadBitRetVal}.";
                        //LoggerManager.Debug($"[Loadersupervisor] ValidationFoupUnloadedState(). Foup Number: {foupNumber}, Foup State: {foupState}, Cassette State : {cSTState}, DPOutFlag : {DPOutFlag}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.FOUP_UNLOADED_STATE_VALIDATION_FAIL;
            }

            return retVal;

        }

        public EventCodeEnum GetCardPodStatusClient(int stageIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var client = GetClient(stageIndex);
                if (client != null)
                {
                    retVal = client.GetCardPodState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
