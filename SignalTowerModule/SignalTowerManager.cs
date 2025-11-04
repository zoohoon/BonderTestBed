using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTowerModule
{
    using LogModule;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Event;
    using ProberInterfaces.Event.EventProcess;
    using ProberInterfaces.SignalTower;
    using ProberInterfaces.State;
    using SequenceService;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /*
     * SignalTower Manager는 SignalTower controller에서 받은 이벤트 리스트를 받아서 이벤트 등록을 해준다.
     */

    public class SignalTowerManager : SequenceServiceBase, INotifyPropertyChanged, ISignalTowerManager, IHasSysParameterizable, IProbeEventSubscriber
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion        
        //public bool Initialized { get; set; }

        private bool _Initialized;

        public bool Initialized
        {
            get { return _Initialized; }
            set { _Initialized = value; }
        }

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public EventProcessList SubscribeRecipeParam { get; set; }

        private SignalTowerController _SignalTowerController;

        public SignalTowerController SignalTowerController
        {
            get { return _SignalTowerController; }
            set { _SignalTowerController = value; }
        }

        public SignalTowerSystemParameter _SignalTowerSystemParam;
        public SignalTowerSystemParameter SignalTowerSystemParam
        {
            get { return _SignalTowerSystemParam; }
            set { _SignalTowerSystemParam = value; }
        }

        private List<SignalTowerUnitBase> _SignalTowerUnits = new List<SignalTowerUnitBase>();
        public List<SignalTowerUnitBase> SignalTowerUnits
        {
            get { return _SignalTowerUnits; }
            set
            {
                if (value != _SignalTowerUnits)
                {
                    _SignalTowerUnits = value as List<SignalTowerUnitBase>;
                    RaisePropertyChanged();
                }
            }
        }

        private SystemStatusMap _SystemStatusMap;
        public SystemStatusMap SystemStatusMap
        {
            get { return _SystemStatusMap; }
            set
            {
                if (value != _SystemStatusMap)
                {
                    _SystemStatusMap = value as SystemStatusMap;
                }
            }
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    GetMonitorSignalTowerComboList();

                    SignalTowerController = new SignalTowerController(this);
                    SignalTowerController.InitModule();

                    SystemStatusMap = new SystemStatusMap(this);
                    SystemStatusMap.InitData();

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
            }

            return retval;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }
        public void DeInitModule()
        {
            try
            {
                SignalTowerController.DeInitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==> Load & Save System Parameter
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new SignalTowerSystemParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                RetVal = this.LoadParameter(ref tmpParam, typeof(SignalTowerSystemParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    SignalTowerSystemParam = tmpParam as SignalTowerSystemParameter;
                }
                else
                {
                    LoggerManager.Error(String.Format("[SignalTowerManager] Load System Param: Serialize Error"));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }


            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(SignalTowerSystemParam);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error(String.Format("[SignalTowerManager] Save System Param: Serialize Error"));
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        #endregion

        private void GetMonitorSignalTowerComboList()
        {
            try
            {
                foreach (SignalTowerUnitBase unitbase in SignalTowerSystemParam.SignalTowerUnitBase)
                {
                    SignalTowerUnits.Add(unitbase);
                    SignalTowerUnits[SignalTowerUnits.Count - 1].InitModule(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // 0.5초 주기로 
        private void SetPulseThread()
        {
            try
            {
                // TODO : 아래 스레드가 같이 돌아야한다. 업데이트된 맵정보로 이벤트를 발생시키기 때문에
                // Update System Status Map Info
                SystemStatusMap.SetPulse();

                SignalTowerController.MonitoringMachineState(SystemStatusMap);

                // event list 에 직접 접근을 하지않고 Add, remove 할 데이터가 있는지 확인?
                SignalTowerController.MonitoringEventLists();

                // raising events collection / Add,remove on&blink queue
                SignalTowerController.SetPulse();

                // Signal tower On/Off
                for (int i = 0; i < SignalTowerUnits.Count(); i++)
                {
                    SignalTowerUnits[i].SetPulse();
                }

                SignalTowerController.CleanUpEventObjList();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err); // 0.5초 마다 로그 찍을테니까 어떻게 처리할지 고려해야한다.
            }
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum retVal = ModuleStateEnum.RUNNING;
            try
            {
                if (Initialized == true)
                {
                    SetPulseThread();
                }
                Thread.Sleep(500);  // 여기에 thread.sleep 주면 0.5초 주기로 돈다. 
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"[SignalTowerManager] Error occurred while SequenceRun ");
            }

            return retVal;
        }

        public EventCodeEnum RegistEventSubscribe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            IEventManager EventManager = this.EventManager();
            try
            {
                if (0 < (this.SubscribeRecipeParam?.Count ?? 0))
                {
                    foreach (var evtitem in this.SubscribeRecipeParam)
                    {
                        RetVal = EventManager.RemoveEvent(evtitem.EventFullName, "ProbeEventSubscibers", evtitem.EventNotify);
                        if (RetVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[SignalTowerEvent] Remove EventSubscribe Error... Event:{evtitem.EventFullName}");
                        }
                    }
                    this.SubscribeRecipeParam.Clear();
                }
                RetVal = this.LoadSignalSubscribeRecipe();
                foreach (var evtitem in this.SubscribeRecipeParam)
                {
                    RetVal = EventManager.RegisterEvent(evtitem.EventFullName, "ProbeEventSubscibers", evtitem.EventNotify);
                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[SignalTowerEvent] Regist EventSubscribe Error... Event:{evtitem.EventFullName}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }
        private string LotGUID = "EF0D06CE-6FD1-4266-94C5-4E7C0C885A5F";
        private string FoupGUID = "F4CF40F7-07CE-4AEB-B0AB-9593D3F71ED6";
        private string MasterKeyGUID = "5BCCB7C1-FC6A-4EC5-8BE1-CD6BA36A3CBE";
        private string SystemGUID = "B5A65F40-A91C-4A0D-8506-D6ED5E4CB36E";
        /// <summary>
        /// 발생한 이벤트에 따른 경광등 표시의 기본값
        /// Lot 관련, loader system error 이벤트 GUID            - "EF0D06CE-6FD1-4266-94C5-4E7C0C885A5F"
        /// Cell System error 관련 이벤트 GUID                   - "B39D009A-835E-47CD-B385-E91C6BC75EB0"
        /// cassette, Foup 관련 이벤트 GUID                      - "F4CF40F7-07CE-4AEB-B0AB-9593D3F71ED6"
        /// 위와같이 나눈 이유는 발생한 이벤트 <-> 클리어해주는 이벤트가 같은 GUID를 공유하고 있어야하기 때문에
        /// </summary>
        /// <returns></returns>
        private EventCodeEnum LoadSignalSubscribeRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            SubscribeRecipeParam = new EventProcessList();

            string recipeFileName = "Recipe_Subscribe_Signal.json";
            string FullPath = this.FileManager().GetSystemParamFullPath("Event", recipeFileName);

            try
            {
                if (File.Exists(FullPath) == false)
                {
                    SignalTowerEventParam eventParam = null;

                    #region Lot GUID

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = LotGUID;     // Green on 
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.ON));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderLotStartEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(CellRunningStateEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderRunningStateEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(LotResumeEvent).FullName, Parameter = eventParam, Enable = true });

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = LotGUID;     // Red Green Yellow off 
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderLotEndEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(LotEndEvent).FullName, Parameter = eventParam, Enable = true });

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = LotGUID;     // Red blink + buzzer on
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.BLINK));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.ON));                    
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderPausedStateEvent).FullName, Parameter = eventParam, Enable = true });
                    
                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = LotGUID;     // Red blink + buzzer off
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.BLINK));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(CellPausedStateEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(CellLotAbortedByUser).FullName, Parameter = eventParam, Enable = true });

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = LotGUID;     // Yellow on
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.ON));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderIdleStateEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(CellIdleStateEvent).FullName, Parameter = eventParam, Enable = true });

                    #endregion

                    #region Foup GUID

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = FoupGUID;     // Green blink    
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.BLINK));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new FoupEventProcessor() { EventFullName = typeof(CassetteRequestEvent).FullName, Parameter = eventParam, Enable = true });


                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = FoupGUID;     // Green off  -> cassette request 상태를 꺼주는   
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new FoupEventProcessor() { EventFullName = typeof(CassetteAttachEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new FoupEventProcessor() { EventFullName = typeof(CassetteDetachEvent).FullName, Parameter = eventParam, Enable = true });

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = FoupGUID;     // Yellow blink  
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.BLINK));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.IGNORE));
                    SubscribeRecipeParam.Add(new FoupEventProcessor() { EventFullName = typeof(CassetteRemoveRequestEvent).FullName, Parameter = eventParam, Enable = true });

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = FoupGUID;     // Buzzer on  
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.IGNORE));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.IGNORE));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.IGNORE));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.ON));
                    SubscribeRecipeParam.Add(new E84EventProcessor() { EventFullName = typeof(LoadPortAccessViolation).FullName, Parameter = eventParam, Enable = true });
                    #endregion

                    #region System GUID 

                    eventParam = new SignalTowerEventParam();
                    eventParam.Priority = 1;
                    eventParam.Guid = SystemGUID;     // Red on     
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.ON));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.ON));
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderErrorStateEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderMainAirErrorEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(StageMainAirErrorEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderMainVacErrorEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(StageMainVacErrorEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderEMOErrorEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(ProberErrorEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(StageErrorEvent).FullName, Parameter = eventParam, Enable = true });

                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = SystemGUID;     // Red off   
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.OFF));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.OFF));
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderMachineInitCompletedEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderMainAirSuccessEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(StageMainAirSuccessEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderMainVacSuccessEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(StageMainVacSuccessEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(LoaderEMOSuccessEvent).FullName, Parameter = eventParam, Enable = true });
                    SubscribeRecipeParam.Add(new StageEventProcessor() { EventFullName = typeof(MachineInitCompletedEvent).FullName, Parameter = eventParam, Enable = true });
                    #endregion

                    #region MasterKey GUID
                    eventParam = new SignalTowerEventParam();
                    eventParam.Guid = MasterKeyGUID;     // Buzzer off     // GUID master key = 어떤 guid 라도 clear 가능하게
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("RED", EnumSignalTowerState.IGNORE));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("GREEN", EnumSignalTowerState.IGNORE));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("YELLOW", EnumSignalTowerState.IGNORE));
                    eventParam.SignalDefineInformations.Add(new SignalDefineInformation("BUZZER", EnumSignalTowerState.OFF));
                    SubscribeRecipeParam.Add(new LoaderEventProcessor() { EventFullName = typeof(BuzzerOffStateEvent).FullName, Parameter = eventParam, Enable = true });

                    #endregion
                    RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[SignalTowerManager] LoadSignalSubscribeRecipe(): Serialize Error");
                        return RetVal;
                    }
                }

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(EventProcessList), null, FullPath);

                if (RetVal != EventCodeEnum.NONE)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[SignalTowerManager] LoadSignalSubscribeRecipe(): DeSerialize Error");
                    return RetVal;
                }
                else
                {
                    SubscribeRecipeParam = tmpParam as EventProcessList;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public void UpdateEventObjList(object eventParam, bool enable)
        {
            try
            {
                SignalTowerController.UpdateEventObjList(eventParam, enable);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
