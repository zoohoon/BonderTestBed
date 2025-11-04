using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderControllerBase;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using ProberInterfaces.Foup;
using ProberInterfaces.Monitoring;
using ProberInterfaces.SignalTower;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SignalTowerModule
{
    public class SignalTowerController : INotifyPropertyChanged, ISignalTowerController
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;



        private SignalTowerManager _Module;
        public SignalTowerManager Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        //private SignalTowerEventParam _EventParam;

        //public SignalTowerEventParam EventParam
        //{
        //    get { return _EventParam; }
        //    set { _EventParam = value; }
        //}

        private List<CellInfo> _CellBackupInfos = new List<CellInfo>();

        public List<CellInfo> CellBackupInfos
        {
            get { return _CellBackupInfos; }
            set { _CellBackupInfos = value; }
        }

        private List<FoupInfo> _FoupBackupInfos = new List<FoupInfo>();

        public List<FoupInfo> FoupBackupInfos
        {
            get { return _FoupBackupInfos; }
            set { _FoupBackupInfos = value; }
        }

        private ModuleStateEnum _LoaderBackupInfo;

        public ModuleStateEnum LoaderBackupInfo
        {
            get { return _LoaderBackupInfo; }
            set { _LoaderBackupInfo = value; }
        }

        private ModuleStateEnum _LoaderLotBackupInfo;

        public ModuleStateEnum LoaderLotBackupInfo
        {
            get { return _LoaderLotBackupInfo; }
            set { _LoaderLotBackupInfo = value; }
        }

        private List<IMonitoringBehavior> _MonitoringBehaviorList;

        public List<IMonitoringBehavior> MonitoringBehaviorList
        {
            get { return _MonitoringBehaviorList; }
            set { _MonitoringBehaviorList = value; }
        }


        public bool initialized { get; set; } = false;

        private ILoaderSupervisor LoaderMaster;
        private ILoaderModule LoaderModule;

        private List<SignalTowerEventParam> _SignalTowerEventObjList = new List<SignalTowerEventParam>();

        public List<SignalTowerEventParam> SignalTowerEventObjList
        {
            get { return _SignalTowerEventObjList; }
            set { _SignalTowerEventObjList = value; }
        }

        private List<SignalTowerEventParam> _SignalTowerEventAllObjList = new List<SignalTowerEventParam>();

        public List<SignalTowerEventParam> SignalTowerEventAllObjList
        {
            get { return _SignalTowerEventAllObjList; }
            set { _SignalTowerEventAllObjList = value; }
        }

        private string _MasterKeyGUID = "5BCCB7C1-FC6A-4EC5-8BE1-CD6BA36A3CBE";

        public SignalTowerController(SignalTowerManager module)
        {
            try
            {
                Module = module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitModule()
        {
            try
            {                
                LoaderBackupInfo = ModuleStateEnum.UNDEFINED;
                if(SignalTowerEventAllObjList != null)
                    SignalTowerEventAllObjList.Clear();     // => parameter 에 정의된 모든 이벤트들을 담고 있다.                
                if (SignalTowerEventObjList != null)
                    SignalTowerEventObjList.Clear();        // => 발생한 이벤트들을 담고 있다.
                if (CellBackupInfos != null)
                    CellBackupInfos.Clear();
                if (FoupBackupInfos != null)
                    FoupBackupInfos.Clear();
                
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInit SignalTowerController() Function error: " + err.Message);
            }
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LoaderMaster = Module.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    LoaderModule = Module.GetContainer().Resolve<ILoaderModule>();
                    Initialized = true;

                    //this.IOManager().IOServ.WriteBit();
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
            }

            return retval;
        }
        private void RaisingEventFoup(FoupStateEnum foupStateEnum, FoupPRESENCEStateEnum foupPRESENCEStateEnum, int foupidx, int cellIdlecnt = -1)
        {
            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                switch (foupStateEnum)
                {
                    case FoupStateEnum.UNDEFIND:
                        break;
                    case FoupStateEnum.ERROR:
                        break;
                    case FoupStateEnum.LOAD:        // cassette 가져왔다 G blink off  CassetteLoadDoneEvent                                             
                        break;
                    case FoupStateEnum.LOADING:
                        break;
                    case FoupStateEnum.UNLOAD:
                        if (foupPRESENCEStateEnum == FoupPRESENCEStateEnum.CST_DETACH)
                        {
                            if (cellIdlecnt > 0) // cassette 를 제거한 후에 idle cell 확인하기. foup 이 랏드에 할당되어 있지 않고, 셀도 idle 이면
                            {
                                semaphore = new SemaphoreSlim(0);       // cassette 가져와라 G blink
                                this.EventManager().RaisingEvent(typeof(CassetteRequestEvent).FullName, new ProbeEventArgs(this, semaphore, foupidx));
                                semaphore.Wait();
                            }
                            else
                            {
                                semaphore = new SemaphoreSlim(0);       // foup에 cassette가 없다. Y blink off cassette Detach
                                this.EventManager().RaisingEvent(typeof(CassetteDetachEvent).FullName, new ProbeEventArgs(this, semaphore, foupidx));
                                semaphore.Wait();
                            }
                        }
                        else if (foupPRESENCEStateEnum == FoupPRESENCEStateEnum.CST_ATTACH)
                        {
                            // 1. lot end 되서 제거되어야 할 카세트인 경우        // lot 할당된적이 있는지 없는지                                                     
                            if (LoaderMaster.BackupActiveLotInfos.Find(x => x.LotID != "") != null)
                            {
                                semaphore = new SemaphoreSlim(0);       // cassette 가져가라 Y blink
                                this.EventManager().RaisingEvent(typeof(CassetteRemoveRequestEvent).FullName, new ProbeEventArgs(this, semaphore, foupidx));
                                semaphore.Wait();
                            }
                            else
                            {
                                semaphore = new SemaphoreSlim(0);       // foup 에 cassette가 있다. G blink off cassette Attach
                                this.EventManager().RaisingEvent(typeof(CassetteAttachEvent).FullName, new ProbeEventArgs(this, semaphore, foupidx));
                                semaphore.Wait();
                            }
                            // 2. lot 에 투입될 카세트인 경우
                        }
                        break;
                    case FoupStateEnum.UNLOADING:
                        break;
                    case FoupStateEnum.EMPTY_CASSETTE:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void RaisingEventCell(ModuleStateEnum moduleStateEnum, int cellidx)
        {
            try
            {
                SemaphoreSlim semaphore = null;
                switch (moduleStateEnum)
                {
                    case ModuleStateEnum.UNDEFINED:
                        break;
                    case ModuleStateEnum.INIT:
                        break;
                    case ModuleStateEnum.IDLE:
                        // 1. program on 된 상태
                        // 2. Lot end 된 상태
                        // 3. system error 해결하고 난 후 상태
                        semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CellIdleStateEvent).FullName, new ProbeEventArgs(this, semaphore, cellidx));
                        semaphore.Wait();
                        break;
                    case ModuleStateEnum.RUNNING:
                        // 1. Loader lot start event 불려서 running 상태
                        // 2. cell resume event 불려서 running 상태
                        semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CellRunningStateEvent).FullName, new ProbeEventArgs(this, semaphore, cellidx));
                        semaphore.Wait();
                        break;
                    case ModuleStateEnum.PENDING:
                        break;
                    case ModuleStateEnum.SUSPENDED:
                        break;
                    case ModuleStateEnum.ABORT:
                        break;
                    case ModuleStateEnum.DONE:
                        break;
                    case ModuleStateEnum.ERROR:
                        break;
                    case ModuleStateEnum.PAUSED:
                        semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CellPausedStateEvent).FullName, new ProbeEventArgs(this, semaphore, cellidx));
                        semaphore.Wait();
                        break;
                    case ModuleStateEnum.RECOVERY:
                        break;
                    case ModuleStateEnum.RESUMMING:
                        break;
                    case ModuleStateEnum.PAUSING:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool errorhandled = false;
        /// <summary>
        /// 상태 업데이트된 cell, foup 정보로 Event raising하는 함수
        /// </summary>
        /// <param name="systemStatusMap"></param>
        public void MonitoringMachineState(SystemStatusMap systemStatusMap)
        {
            int cellIdle = 0;
            try
            {
                lock (lockObj)      // 재진입 방지를 위한 lock 처리
                {
                    if (LoaderMaster == null)
                        return;
                    else
                    {
                        // Lot 상태가 아닐때 장비 Idle상태 확인하기                     
                        // 1. lot 상황일때
                        if (LoaderLotBackupInfo != LoaderMaster.CurrentModuleState)
                        {
                            if (LoaderMaster.CurrentModuleState == ModuleStateEnum.IDLE)
                            {
                                // lot 상황에서 error 발생 후 idle 로 가는 경우가 없다.
                                //if (LoaderBackupInfo == ModuleStateEnum.ERROR)
                                //{
                                //    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                //    this.EventManager().RaisingEvent(typeof(LoaderMachineInitCompletedEvent).FullName, new ProbeEventArgs(this, semaphore));
                                //    semaphore.Wait();
                                //}
                                //else
                                //{
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(LoaderIdleStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();
                                //}
                            }
                            else if (LoaderMaster.CurrentModuleState == ModuleStateEnum.PAUSED)
                            {
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(LoaderPausedStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                                semaphore.Wait();
                            }
                            else if (LoaderMaster.CurrentModuleState == ModuleStateEnum.RUNNING)
                            {
                                // loader pause 다음에 resume 하면 running 상태가 된다.
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                this.EventManager().RaisingEvent(typeof(LoaderRunningStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                                semaphore.Wait();
                            }
                            //else if (LoaderMaster.CurrentModuleState == ModuleStateEnum.ERROR)
                            //{
                            //    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            //    Module.EventManager().RaisingEvent(typeof(LoaderErrorStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                            //    semaphore.Wait();
                            //}
                            LoaderLotBackupInfo = LoaderMaster.CurrentModuleState;
                        }

                        // 2. lot 상황 & 메뉴얼 상황
                        if(LoaderBackupInfo != LoaderModule.ModuleState)
                        {
                            // LoaderProcStateEnum.SYSTEM_ERROR가 된 상황
                            if (LoaderModule.ModuleState == ModuleStateEnum.ERROR)
                            {
                                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                Module.EventManager().RaisingEvent(typeof(LoaderErrorStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                                semaphore.Wait();
                            }

                            else if(LoaderModule.ModuleState == ModuleStateEnum.IDLE)
                            {
                                if (LoaderBackupInfo == ModuleStateEnum.ERROR)
                                {
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(LoaderMachineInitCompletedEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();
                                }
                            }                                                        
                            LoaderBackupInfo = LoaderModule.ModuleState;
                        }
                    }

                    foreach (var cell in systemStatusMap.CellInfo)
                    {
                        //R off G on Y off B off

                        if (CellBackupInfos.Find(item => item.Index == cell.Index) == null)      // 백업 데이터가 없을때,
                            CellBackupInfos.Add(new CellInfo(cell.Index, cell.CellMode, cell.Cellstate, cell.Reconnecting));
                        else
                        {                    
                            var obj = LoaderMaster.CellsInfo.Where(i => i.Index == cell.Index).FirstOrDefault();
                            if (cell.CellMode != GPCellModeEnum.DISCONNECT && obj.StageInfo.IsConnectChecking == false && obj.Reconnecting == false)
                            {                                
                                // cell - loader 연결상태 확인하기.  
                                // loader-cell이 connect시 MonitoringBehaviorList는 connectstage에서 이미 가져왔다.
                                var client = obj.MonitoringBehaviorList;
                                if (client.Count != 0)
                                {
                                    // 1. 장비가 돌던 와중에 system error 발생
                                    // 2. loader program 껐다켜서 idle 상태가 되었는데 , cell 이 이미 system error 발생한 상태
                                    if (cell.IsSystemError)
                                    {
                                        if (client.Count(a => a.IsError == true && a.SystemErrorType == true) == 0)
                                        {
                                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                            this.EventManager().RaisingEvent(typeof(MachineInitCompletedEvent).FullName, new ProbeEventArgs(this, semaphore, cell.Index));
                                            semaphore.Wait();
                                            cell.IsSystemError = false;
                                        }
                                    }
                                    else
                                    {
                                        if (client.Find(a => a.IsError == true && a.SystemErrorType == true) != null)
                                        {
                                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                            this.EventManager().RaisingEvent(typeof(ProberErrorEvent).FullName, new ProbeEventArgs(this, semaphore, cell.Index));
                                            semaphore.Wait();
                                            cell.IsSystemError = true;
                                        }
                                    }
                                }

                                if (CellBackupInfos.Find(x => x.Index == cell.Index) != null)
                                {
                                    if (cell.Cellstate == ModuleStateEnum.IDLE)
                                        cellIdle++;
                                }
                            }
                            else if((cell.CellMode == GPCellModeEnum.DISCONNECT && CellBackupInfos[cell.Index - 1].CellMode != GPCellModeEnum.DISCONNECT) && obj.StageInfo.IsConnected == false)
                            {
                                // Disconnect 되었을때, 한번만 NeedRemoveEventFunc() 함수를 타게 하기위한 조건 추가.
                                // cell 정보가 이미 차 있는데, disconnect 되었을때 데이터 초기화해주기.                      
                                NeedRemoveEventFunc(cell.Index);                                                         
                            }

                            //state 가 변했을때만,
                            if (CellBackupInfos[cell.Index - 1].Cellstate != cell.Cellstate || CellBackupInfos[cell.Index - 1].CellMode != cell.CellMode)
                            {
                                RaisingEventCell(cell.Cellstate, cell.Index);

                                if (cell.Cellstate == ModuleStateEnum.IDLE || cell.CellMode == GPCellModeEnum.DISCONNECT || cell.Reconnecting == true)
                                {
                                    foreach (var foup in systemStatusMap.FoupInfo)
                                    {
                                        RaisingEventFoup(foup.FoupState, foup.FoupPRESENCEState, foup.Index, cellIdle);
                                    }
                                }
                                CellBackupInfos[cell.Index - 1].Cellstate = cell.Cellstate;
                                CellBackupInfos[cell.Index - 1].CellMode = cell.CellMode;
                            }
                        }
                    }

                    foreach (var foup in systemStatusMap.FoupInfo)
                    {
                        if (FoupBackupInfos.Find(item => item.Index == foup.Index) == null)      // 백업 데이터가 없을때,
                            FoupBackupInfos.Add(new FoupInfo(foup.Index, foup.FoupState, foup.FoupPRESENCEState, foup.FoupModeStatus, foup.FoupEnable));
                        else
                        {
                            if (FoupBackupInfos.Find(x => x.Index == foup.Index) != null)
                            {
                                if (FoupBackupInfos[foup.Index - 1].FoupState != foup.FoupState ||
                                    FoupBackupInfos[foup.Index - 1].FoupPRESENCEState != foup.FoupPRESENCEState ||
                                    FoupBackupInfos[foup.Index - 1].FoupEnable != foup.FoupEnable ||
                                    FoupBackupInfos[foup.Index - 1].FoupModeStatus != foup.FoupModeStatus)
                                {
                                    //if(foup.FoupModeStatus == FoupModeStatusEnum.ONLINE && foup.FoupEnable == true)
                                    {
                                        RaisingEventFoup(foup.FoupState, foup.FoupPRESENCEState, foup.Index, cellIdle);

                                        FoupBackupInfos[foup.Index - 1].FoupState = foup.FoupState;
                                        FoupBackupInfos[foup.Index - 1].FoupPRESENCEState = foup.FoupPRESENCEState;
                                        FoupBackupInfos[foup.Index - 1].FoupModeStatus = foup.FoupModeStatus;
                                        FoupBackupInfos[foup.Index - 1].FoupEnable = foup.FoupEnable;
                                    }
                                }
                            }
                        }
                    }

                    if (errorhandled)
                    {
                        // 여기서 플래그를 찍기 직전이다 라는 로그를 찍는다. 여기서 아무 문제 없이 다 끝났으면, errorhandled = false찍고                    
                        LoggerManager.Debug("Here just before error handled flag : " + errorhandled.ToString());
                        errorhandled = false;
                    }
                }
            }
            catch (Exception err)
            {
                // error로 한번 들어오면 flag 켜고, 로그를 안찍는 방향으로 
                if (errorhandled == false)
                {
                    errorhandled = true;
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }

        object lockObj = new object();
        object EventObjlock = new object();

        /// <summary>
        /// 시그널 타워 이벤트 리스트를 관리하는 함수 ( add, remove ) - 비동기로 이벤트가 발생하면 호출되는 함수
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="enable"></param>
        public void UpdateEventObjList(object parameter, bool enable)
        {
            try
            {
                // 자신만의 lock obj 가 따로 있어야 한다. 
                // 1. enable 먼저 확인하고,
                // 2. guid 랑 index 보고 기존에 없으면 리스트에 add / 기존에 있는 이벤트를 지워줄 이벤트이면 리스트에서 remove              
                lock (EventObjlock)
                {
                    if (enable != false)
                    {
                        SignalTowerEventParam EventParam = parameter as SignalTowerEventParam;

                        var Idx = SignalTowerEventAllObjList.Find(param => param.Guid == EventParam.Guid &&
                                                                  param.CellIdx == EventParam.CellIdx &&
                                                                  param.FoupIdx == EventParam.FoupIdx);


                        EventParam.NeedRemoveEvent = false;
                        EventParam.ProcessedEvent = false;

                        if (Idx != null)
                        {
                            // 기존에 있는 이벤트를 지워준다.
                            if (EventParam.CellIdx == 0 && EventParam.FoupIdx == 0)        // loader event
                            {
                                var item = SignalTowerEventAllObjList.Find(x => x.Guid == Idx.Guid && x.CellIdx == Idx.CellIdx && x.FoupIdx == Idx.FoupIdx && x.NeedRemoveEvent == false);
                                if (item != null)
                                    item.NeedRemoveEvent = true;

                            }
                            else if (EventParam.CellIdx != 0 && EventParam.FoupIdx == 0)    // cell event 
                            {
                                if (Idx != null)
                                {
                                    var item = SignalTowerEventAllObjList.Find(x => x.Guid == Idx.Guid && x.CellIdx == Idx.CellIdx && x.NeedRemoveEvent == false);
                                    if (item != null)
                                        item.NeedRemoveEvent = true;

                                }
                            }
                            else if (EventParam.CellIdx == 0 && EventParam.FoupIdx != 0)       // foup event
                            {
                                if (Idx != null)
                                {
                                    var item = SignalTowerEventAllObjList.Find(x => x.Guid == Idx.Guid && x.FoupIdx == Idx.FoupIdx && x.NeedRemoveEvent == false);
                                    if (item != null)
                                        item.NeedRemoveEvent = true;

                                }
                            }
                        }

                        // 새로 들어온 이벤트를 추가해준다. 
                        // buzzer must clear
                        if (EventParam.Guid == _MasterKeyGUID)
                            EventParam.NeedRemoveEvent = true;

                        SignalTowerEventAllObjList.Add(EventParam);
                        LoggerManager.Debug($"[TEST_ST] UpdateEventObjList Add List : {EventParam.Guid} {EventParam.CellIdx} {EventParam.FoupIdx} {EventParam.EventName} {EventParam.NeedRemoveEvent} ");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 발생한 이벤트 리스트에 직접 접근을 하지 않기 위한 중간다리 역할. ( 아래 setpulse() 함수를 부르기 전에 event list를 확인하기 위함)
        /// </summary>
        public void MonitoringEventLists()
        {
            try
            {
                if (SignalTowerEventAllObjList.Count > 0)
                {
                    SignalTowerEventObjList = SignalTowerEventAllObjList.ToList();       // 발생한 모든 이벤트들을 담고 있다.
                    lock (lockObj)
                    {
                        SignalTowerEventObjList = SignalTowerEventObjList.FindAll(eventParam => eventParam.ProcessedEvent == false || eventParam.NeedRemoveEvent == true);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // 발생한 이벤트들을 램프에 업데이트하는 주기를 동기화해주기 위해 add, remove를 해주는 함수. 0.5초 주기
        public void SetPulse()
        {
            try
            {
                // lockObj 필요   // 리스트를 그대로 쓰지말고 이 안에서 리스트를 복제해서 이 안에서만 쓸수있도록                          
                if (SignalTowerEventObjList.Count > 0)
                {
                    lock (lockObj)
                    {
                        // condition 부합한다는 의미. on,blink queue에 담아야 한다는 의미
                        foreach (var item in SignalTowerEventObjList)
                        {
                            QueueSignalStatus(item);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // 마지막으로 상태 조합해서 list(on,blink queue) 에 Add, remove 하는 함수
        // on, blink queue에 이미 값이 들어있으면, 추가할 필요 없다.
        private void QueueSignalStatus(SignalTowerEventParam signalTowerEventParam)
        {
            string logstr = "[TEST_ST] Signal State : ";
            try
            {
                if (signalTowerEventParam == null)
                    return;


                foreach (var item in signalTowerEventParam.SignalDefineInformations)
                {
                    if (Module.SignalTowerUnits.Find(x => x.Label == item.SignalType) != null)
                    {
                        int idx = Module.SignalTowerUnits.FindIndex(x => x.Label == item.SignalType);

                        switch (item.SignalState)
                        {
                            case EnumSignalTowerState.UNDEFINED:
                                break;
                            case EnumSignalTowerState.ON:
                                if (!signalTowerEventParam.NeedRemoveEvent)
                                {
                                    if (Module.SignalTowerUnits[idx].OnQueue.Where(x => x == signalTowerEventParam).FirstOrDefault() == null)     // 기존에 없으면
                                    {
                                        Module.SignalTowerUnits[idx].OnQueue.Add(signalTowerEventParam);
                                        LoggerManager.Debug($"[TEST_ST] QueueSignalStatus Add OnQueue : {signalTowerEventParam.Guid} {item.SignalType} {item.SignalState} ");
                                    }
                                }
                                else
                                {
                                    if (Module.SignalTowerUnits[idx].OnQueue.Where(x => x == signalTowerEventParam).FirstOrDefault() != null)     // 기존에 있으면                                    
                                    {
                                        Module.SignalTowerUnits[idx].OnQueue.Remove(signalTowerEventParam);
                                        LoggerManager.Debug($"[TEST_ST] QueueSignalStatus Remove OnQueue : {signalTowerEventParam.Guid} {item.SignalType} {item.SignalState} ");
                                    }

                                }
                                break;
                            case EnumSignalTowerState.BLINK:
                                if (!signalTowerEventParam.NeedRemoveEvent)
                                {
                                    if (Module.SignalTowerUnits[idx].BlinkQueue.Where(x => x == signalTowerEventParam).FirstOrDefault() == null)
                                    {
                                        Module.SignalTowerUnits[idx].BlinkQueue.Add(signalTowerEventParam);
                                        LoggerManager.Debug($"[TEST_ST] QueueSignalStatus Add BlinkQueue : {signalTowerEventParam.Guid} {item.SignalType} {item.SignalState} ");
                                    }

                                }
                                else
                                {
                                    if (Module.SignalTowerUnits[idx].BlinkQueue.Where(x => x == signalTowerEventParam).FirstOrDefault() != null)
                                    {
                                        Module.SignalTowerUnits[idx].BlinkQueue.Remove(signalTowerEventParam);
                                        LoggerManager.Debug($"[TEST_ST] QueueSignalStatus Remove BlinkQueue : {signalTowerEventParam.Guid} {item.SignalType} {item.SignalState} ");
                                    }

                                }

                                break;
                            case EnumSignalTowerState.OFF:
                                if (signalTowerEventParam.NeedRemoveEvent)
                                {
                                    if (signalTowerEventParam.Guid == _MasterKeyGUID)
                                        Module.SignalTowerUnits[idx].OnQueue.Clear();

                                    if (Module.SignalTowerUnits[idx].OnQueue.Where(x => x == signalTowerEventParam).FirstOrDefault() != null)     // 기존에 있으면
                                    {
                                        Module.SignalTowerUnits[idx].OnQueue.Remove(signalTowerEventParam);
                                        LoggerManager.Debug($"[TEST_ST] QueueSignalStatus Remove OnQueue : {signalTowerEventParam.Guid} {item.SignalType} {item.SignalState} ");
                                    }

                                    if (Module.SignalTowerUnits[idx].BlinkQueue.Where(x => x == signalTowerEventParam).FirstOrDefault() != null) // 기존에 있으면                                    
                                    {
                                        Module.SignalTowerUnits[idx].BlinkQueue.Remove(signalTowerEventParam);
                                        LoggerManager.Debug($"[TEST_ST] QueueSignalStatus Remove BlinkQueue : {signalTowerEventParam.Guid} {item.SignalType} {item.SignalState} ");
                                    }
                                }
                                break;
                            case EnumSignalTowerState.IGNORE:
                                break;
                            default:
                                break;
                        }
                        logstr += $"{signalTowerEventParam.SignalDefineInformations[idx].SignalType} {signalTowerEventParam.SignalDefineInformations[idx].SignalState},";
                    }
                }

                if (!signalTowerEventParam.ProcessedEvent)
                {
                    signalTowerEventParam.ProcessedEvent = true;
                }
                LoggerManager.Debug(logstr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // loader-cell disconnect 되었을때, need remove 해주기위한 함수
        public void NeedRemoveEventFunc(int idx)
        {
            try
            {
                List<SignalTowerEventParam> param = SignalTowerEventObjList.FindAll(item => item.CellIdx == idx);
                foreach (var item in param)
                {
                    item.NeedRemoveEvent = true;                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        // 지워야할 이벤트들을 한번 정리해주는 함수
        public void CleanUpEventObjList()
        {
            try
            {
                if (SignalTowerEventObjList.Count > 0)
                {
                    lock (lockObj)
                    {
                        foreach (var item in SignalTowerEventObjList)
                        {
                            if (item.NeedRemoveEvent == true)
                            {
                                SignalTowerEventAllObjList.Remove(item);
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
    }
}
