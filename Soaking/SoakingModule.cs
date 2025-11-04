using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using System.Collections.ObjectModel;
using System.IO;
using ProberInterfaces.Param;
using System.Diagnostics;
using SubstrateObjects;
using ProberInterfaces.Command;
using ProberErrorCode;
using System.Reflection;
using ProberInterfaces.Event;
using SoakingParameters;
using ProberInterfaces.Event.EventProcess;
using EventProcessModule.EventSoaking;
using System.Runtime.CompilerServices;
using ProberInterfaces.State;
using LogModule;
using NotifyEventModule;
using System.ServiceModel;
using MetroDialogInterfaces;
using System.Windows;
using ProberInterfaces.Soaking;
using SerializerUtil;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Temperature;
using LoaderController.GPController;

namespace Soaking
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SoakingModule : ISoakingModule, INotifyPropertyChanged, IProbeEventSubscriber
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected IFocusing FocusingModule { get; set; }
        protected IFocusParameter FocusingParam { get; set; }
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Soaking);
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
        public bool Initialized { get; set; } = false;
        private bool _ProbeCardChangeTriggered;

        public bool ProbeCardChangeTriggered
        {
            get { return _ProbeCardChangeTriggered; }
            set { _ProbeCardChangeTriggered = value; }
        }
        private bool _LotStartTriggered;

        public bool LotStartTriggered
        {
            get { return _LotStartTriggered; }
            set { _LotStartTriggered = value; }
        }
        private bool _LotResumeTriggered;

        public bool LotResumeTriggered
        {
            get { return _LotResumeTriggered; }
            set { _LotResumeTriggered = value; }
        }
        private bool _DeviceChangeTriggered;

        public bool DeviceChangeTriggered
        {
            get { return _DeviceChangeTriggered; }
            set { _DeviceChangeTriggered = value; }
        }
        private bool _TempDiffTriggered;

        public bool TempDiffTriggered
        {
            get { return _TempDiffTriggered; }
            set { _TempDiffTriggered = value; }
        }
        private bool _ChuckAwayTriggered;

        public bool ChuckAwayTriggered
        {
            get { return _ChuckAwayTriggered; }
            set { _ChuckAwayTriggered = value; }
        }
        private bool _AutoSoakTriggered;

        public bool AutoSoakTriggered
        {
            get { return _AutoSoakTriggered; }
            set { _AutoSoakTriggered = value; }
        }
        private bool _EveryWaferSoakTriggered;
        public bool EveryWaferSoakTriggered
        {
            get { return _EveryWaferSoakTriggered; }
            set { _EveryWaferSoakTriggered = value; }
        }

        //status event soaking이 trigger되면 해당 dic에 추가됨
        private Dictionary<EventSoakType/*eventSoakingType*/, int/*priority*/> _TriggeredStatusEventSoakList = new Dictionary<EventSoakType/*eventSoakingType*/, int/*priority*/>();
        public Dictionary<EventSoakType/*eventSoakingType*/, int/*priority*/> TriggeredStatusEventSoakList
        {
            get { return _TriggeredStatusEventSoakList; }
            set { _TriggeredStatusEventSoakList = value; }
        }


        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private bool _SoackingDone = false;

        public bool SoackingDone
        {
            get { return _SoackingDone; }
            set { _SoackingDone = value; }
        }
        private string _SoakingTitle;
        public string SoakingTitle
        {
            get { return _SoakingTitle; }
            set
            {
                if (value != _SoakingTitle)
                {
                    _SoakingTitle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SoakingMessage;
        public string SoakingMessage
        {
            get { return _SoakingMessage; }
            set
            {
                if (value != _SoakingMessage)
                {
                    _SoakingMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _LastSoakingSetTemp = 999;
        public double LastSoakingSetTemp
        {
            get { return _LastSoakingSetTemp; }
            set
            {
                if (value != _LastSoakingSetTemp)
                {
                    _LastSoakingSetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _AccumulatedTime = 0;
        public int AccumulatedTime
        {
            get { return _AccumulatedTime; }
            set
            {
                if (value != _AccumulatedTime)
                {
                    _AccumulatedTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _AccAtResumeTime = 0;
        public int AccAtResumeTime
        {
            get { return _AccAtResumeTime; }
            set
            {
                if (value != _AccAtResumeTime)
                {
                    _AccAtResumeTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DeviceChangeFlag = false;
        public bool DeviceChangeFlag
        {
            get { return _DeviceChangeFlag; }
            set
            {
                if (value != _DeviceChangeFlag)
                {
                    _DeviceChangeFlag = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _ChangedDeviceName = null;
        public string ChangedDeviceName
        {
            get { return _ChangedDeviceName; }
            set
            {
                if (value != _ChangedDeviceName)
                {
                    _ChangedDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _CurDeviceName = null;
        public string CurDeviceName
        {
            get { return _CurDeviceName; }
            set
            {
                if (value != _CurDeviceName)
                {
                    _CurDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DateTime? _ChuckInRangeTime = new DateTime();
        //Range 내에 있으면 계속하여 시간 업데이트. => ChuckAwaySoak Range 내에 있는 마지막 시간.
        public DateTime? ChuckInRangeTime
        {
            get { return _ChuckInRangeTime; }
            set
            {
                if (value != _ChuckInRangeTime)
                {
                    _ChuckInRangeTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime? _AutoSoakLastChuckAwayTime = new DateTime();
        public DateTime? AutoSoakLastChuckAwayTime
        {
            get { return _AutoSoakLastChuckAwayTime; }
            set
            {
                if (value != _AutoSoakLastChuckAwayTime)
                {
                    _AutoSoakLastChuckAwayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _SysParam;
        [ParamIgnore]
        public IParam SysParam
        {
            get { return _SysParam; }
            set
            {
                if (value != _SysParam)
                {
                    _SysParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        public EventHandler PreHeatEvent { get; set; }
        public void PreHeatEventOn(object sender, EventArgs e)
        {
            try
            {
                IsPreHeatEvent = true;
                LoggerManager.Debug("[SoakingModule] PreHeatEventOn() OK");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private bool _IdleSoak_AlignTrigger = true;//auto soaking할 때 pin align이랑 focusing 할지 말지
        public bool IdleSoak_AlignTrigger
        {
            get { return _IdleSoak_AlignTrigger; }
            set
            {
                if (value != _IdleSoak_AlignTrigger)
                {
                    _IdleSoak_AlignTrigger = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ISoakingChillingTimeMng ChillingTimeMngObj { get; set; }
        private object save_statusSoakinfParam_lockObject = new object();
        private object load_statusSoakinfParam_lockObject = new object();
        private object get_wafercenter_lockObject = new object();
        public bool BeforeStatusSoakingOption_About_UseFlag { get; set; } = false;

        public ILastSoakingStateInfo LastSoakingStateInfoObj { get; set; }

        public bool LotStartTriggered_ToReturnPolishWafer { get; set; } = false;
        public bool NeedToRequestReturnPolishWafer { get; set; } = false;
        public bool UseTempDiff_PrepareSoakingTime { get; set; } = false;
        private string TempDiffValue_CompareStr { get; set; } = "";
        public bool LogWrite_CheckEventSoakingTrigger_ChuckAway { get; set; } = false;  //chuck이 벗어난 경우 지속적인 로그 남김 방지용 flag
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ChillingTimeMngObj = new SoakingChillingTimeMng(this);
                    ChillingTimeMngObj.InitChillingTimeMng();

                    LastSoakingStateInfoObj = new StatusSoakingInfoMng();

                    //DevParam = _SoakingDevFile;
                    _ReasonOfError = new ReasonOfError(ModuleEnum.Soaking);
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();
                    //_SoakingState = new SoakingIdleState(this);
                    _SoakingState = new PreOpState(this);
                    ModuleState = new ModuleInitState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    //RecipeQueue = new Queue<SoakingParamBase>();
                    SoakPriorityList = new Queue<SoakingParamBase>();
                    //PrevLotOPState = ModuleStateEnum.IDLE;
                    //CurScript = new ObservableCollection<SoakBaseInfo>();
                    IsPreHeatEvent = false;
                    SoakingCancelFlag = false;
                    MeasurementSoakingCancelFlag = false;
                    Initialized = true;
                    AccumulatedTime = 0;
                    DeviceChangeFlag = false;
                    
                    //retval = this.EventManager().RegisterEvent(typeof(DoPreHeatSoaking).FullName, "ProbeEventSubscibers", PreHeatEventOn);
                    
                    this.PreHeatEvent -= PreHeatEventOn;
                    this.PreHeatEvent += PreHeatEventOn;
                   
                    //retval = this.EventManager().RegisterEvent(typeof(LotEndEvent).FullName, "ProbeEventSubscibers", EventFired);
                    
                    this.EventManager().RegisterEvent(typeof(CardLoadingEvent).FullName, "ProbeEventSubscibers", EventFired);
                    this.EventManager().RegisterEvent(typeof(CardDockEvent).FullName, "ProbeEventSubscibers", EventFired);
                    
                    retval = this.EventManager().RegisterEvent(typeof(LotStartEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(LotResumeEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(WaferLoadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(AlarmSVChangedEvent).FullName, "ProbeEventSubscibers", EventFired);

                    IsLotEndEventOn = false;
                    CurDeviceName = this.LotOPModule().LotInfo.DeviceName.Value;
                    ChangedDeviceName = null;

                    //this.SoakingDeviceFile_Clone.ChuckInRangeTime = null;
                    ProbeCardChangeTriggered = false;
                    LotStartTriggered = false;
                    LotResumeTriggered = false;
                    DeviceChangeTriggered = false;
                    TempDiffTriggered = false;
                    ChuckAwayTriggered = false;
                    AutoSoakTriggered = false;
                    LotStartTriggered_ToReturnPolishWafer = false;
                    IdleSoak_AlignTrigger = SoakingDeviceFile_Clone.AutoSoakingParam.IdleSoak_AlignTrigger.Value;

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
        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is CardLoadingEvent || sender is CardDockEvent)
                {
                    //IsLotEndEventOn = true;
                    //TempDiffEventSoaking tempdiffeventsoakingobj = GetEventSoakingObject(EnumSoakingType.TEMPDIFF_SOAK) as TempDiffEventSoaking;
                    ProbeCardChangeEventSoaking pcchangesoakobj = GetEventSoakingObject(EnumSoakingType.PROBECARDCHANGE_SOAK) as ProbeCardChangeEventSoaking;
                    if (pcchangesoakobj != null)
                    {
                        LoggerManager.Debug($"[SoakingModule] Probe card change soak trigger on");
                        //pcchangesoakobj.Triggered = true;
                        ProbeCardChangeTriggered = true;
                        if (SoakPriorityList.Where(soak => soak.EventSoakingType.Value == pcchangesoakobj.EventSoakingType.Value) == null)
                        {
                            SoakPriorityList.Enqueue(pcchangesoakobj);
                        }
                    }

                }
                else if (sender is LotStartEvent)
                {
                    LotStartTriggered_ToReturnPolishWafer = true;
                    LoggerManager.SoakingLog($"Lot Start Trigger on.");
                    LotStartEventSoaking lotstartSoakobj = GetEventSoakingObject(EnumSoakingType.LOTSTART_SOAK) as LotStartEventSoaking;
                    if (lotstartSoakobj != null)
                    {
                        //lotstartSoakobj.Triggered = true;
                        DateTime EndDate = DateTime.Now;
                        DateTime tempdate = new DateTime();
                        if (tempdate == this.LotOPModule().LotInfo.LotEndTime)
                        {
                            LotStartTriggered = true;
                            LoggerManager.Debug($"[SoakingModule] Lot start soak trigger on");
                        }
                        else
                        {
                            TimeSpan datediff = EndDate - this.LotOPModule().LotInfo.LotEndTime;
                            int lotstartskiptime = lotstartSoakobj.LotStartSkipTime.Value;
                            int lotdifftime = Convert.ToInt32(datediff.TotalSeconds);
                            if (lotdifftime < lotstartskiptime)
                            {
                                //AccumulatedTime = 0 이란 이야기는 이전 랏드에서 소킹이 done상태로 완전히 끝난 경우임.
                                if (AccumulatedTime != 0 && AccumulatedTime < lotstartSoakobj.SoakingTimeInSeconds.Value)
                                {
                                    LoggerManager.Debug($"[SoakingModule] Elapsed time since last LOT: {lotdifftime}, Skip time param: {lotstartskiptime}");
                                    LoggerManager.Debug($"[SoakingModule] LotStart Soak time :{lotstartSoakobj.SoakingTimeInSeconds.Value}, Total Soaking Time:{AccumulatedTime}");
                                    LotStartTriggered = true;
                                    LoggerManager.Debug($"[SoakingModule] Lot start soak trigger on");
                                }
                                else
                                {
                                    LotStartTriggered = false;
                                    LoggerManager.Debug($"[SoakingModule] Elapsed time since last LOT: {lotdifftime}, Skip time param: {lotstartskiptime}");
                                    LoggerManager.Debug($"[SoakingModule] LotStart Soak Trigger off");
                                }
                            }
                            else
                            {
                                if (AccumulatedTime != 0)
                                {
                                    LoggerManager.Debug($"[SoakingModule] Elapsed time since last LOT: {lotdifftime}, Skip time param: {lotstartskiptime}");
                                    LoggerManager.Debug($"[SoakingModule] LotStart Soaking Skip Time Out, Soak time {AccumulatedTime} initialization");
                                    AccumulatedTime = 0;
                                }
                                LotStartTriggered = true;
                                LoggerManager.Debug($"[SoakingModule] Lot start soak trigger on");
                            }
                        }
                        if (SoakPriorityList.Where(soak => soak.EventSoakingType.Value == lotstartSoakobj.EventSoakingType.Value) == null)
                        {
                            SoakPriorityList.Enqueue(lotstartSoakobj);
                        }
                        //Lot Start시 현재 Soaking Mode가 신규 Soaking이라면 구 Soaking관련 Trigger와 Soaking Queue List를 Clear한다.
                        if (this.GetShowStatusSoakingSettingPageToggleValue() == true)
                        {
                            this.Check_N_Clear_SoakingTrigger(true);
                        }
                    }

                }
                else if (sender is LotResumeEvent)
                {
                    /*
                    LotResumeEventSoaking lotresumeSoakobj = GetEventSoakingObject(EnumSoakingType.LOTRESUME_SOAK) as LotResumeEventSoaking;

                    if (lotresumeSoakobj != null)
                    {
                        if (lotresumeSoakobj.Enable.Value == true)
                        {
                            // SoakingPauseState 에서 Resume () 에서도 확인해야 함.
                            AccAtResumeTime = 0;
                            LoggerManager.Debug($"EventSoakingRunningState(): Reset AccAtResumeTime as {AccAtResumeTime}");
                           

                        }
                    }
                    */
                }
                else if (sender is DeviceChangedEvent)
                {

                    DeviceChangeEventSoaking devchangesoakobj = GetEventSoakingObject(EnumSoakingType.DEVICECHANGE_SOAK) as DeviceChangeEventSoaking;
                    if (devchangesoakobj != null)
                    {
                        LoggerManager.Debug($"[SoakingModule] Device change soak trigger on");
                        //devchangesoakobj.Triggered = true;
                        DeviceChangeFlag = true;
                        DeviceChangeTriggered = true;
                        if (SoakPriorityList.Where(soak => soak.EventSoakingType.Value == devchangesoakobj.EventSoakingType.Value) == null)
                        {
                            SoakPriorityList.Enqueue(devchangesoakobj);
                        }
                    }

                }
                else if (sender is WaferLoadedEvent)
                {
                    IsSoakingDoneAfterWaferLoad = false;
                         
                    var EveryWaferEventParam = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.StatusSoakingEvents.FirstOrDefault(x => x.SoakingTypeEnum.Value == EventSoakType.EveryWaferSoak);
                    if (null != EveryWaferEventParam && true == EveryWaferEventParam.UseEventSoaking.Value && this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        if (false == TriggeredStatusEventSoakList.ContainsKey(EventSoakType.EveryWaferSoak))
                            TriggeredStatusEventSoakList.Add(EventSoakType.EveryWaferSoak, EveryWaferEventParam.SoakingPriority.Value);
                    }

                    EveryWaferEventSoaking everyWaferSoakobj = GetEventSoakingObject(EnumSoakingType.EVERYWAFER_SOAK) as EveryWaferEventSoaking;

                    if (everyWaferSoakobj != null)
                    {
                        LoggerManager.Debug($"[SoakingModule] Every Wafer soak trigger on");
                        EveryWaferSoakTriggered = true;
                        if (SoakPriorityList.Where(soak => soak.EventSoakingType.Value == everyWaferSoakobj.EventSoakingType.Value) == null)
                        {
                            SoakPriorityList.Enqueue(everyWaferSoakobj);
                        }
                    }
                }
                else if (sender is AlarmSVChangedEvent)
                {
                    if(e.Parameter != null)
                    {
                        double prevSV = 0;
                        double value = 0;
                        double PV = 0;
                        if (e.Parameter is List<double>)
                        {
                            var a = e.Parameter as List<double>;
                            prevSV = a[0];
                            value = a[1];
                            PV = a[2];
                        }
                        SettingTempCallback(prevSV, value, PV);
                    }
                }
                else if (sender is AlarmSVChangedEvent)
                {
                    if(e.Parameter != null)
                    {
                        double prevSV = 0;
                        double value = 0;
                        double PV = 0;
                        if (e.Parameter is List<double>)
                        {
                            var a = e.Parameter as List<double>;
                            prevSV = a[0];
                            value = a[1];
                            PV = a[2];
                        }
                        SettingTempCallback(prevSV, value, PV);
                    }
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
        public void SettingTempCallback(double prevSV, double nextSV, double nowTemp)
        {
            try
            {
                TempEventInfo getTempinfo_ret = null;
                getTempinfo_ret = this.TempController().GetCurrentTempInfoInHistory();
                bool InCCState = false;

                if (getTempinfo_ret != null)
                {
                    if (getTempinfo_ret.TempChangeSource == TemperatureChangeSource.CARD_CHANGE)
                    {
                        InCCState = true;
                    }
                }

                if (StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue()) //status soaking 
                {
                    if (StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.UseStatusSoaking.Value)
                    {
                        if (!nextSV.Equals(prevSV))
                        {
                            string currentCompareTempVal = $"{nextSV},{prevSV}";
                            if (TempDiffValue_CompareStr != currentCompareTempVal)
                            {
                                TempDiffValue_CompareStr = currentCompareTempVal;

                                double TempDiffValue = Math.Abs(nextSV - prevSV);
                                if ((int)TempDiffValue >= StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.PrepareStateConfig.TriggerDegreeForTempDiff.Value)
                                    UseTempDiff_PrepareSoakingTime = true;
                                else
                                    UseTempDiff_PrepareSoakingTime = false;

                                if (SoakingState.GetState() != SoakingStateEnum.PREPARE && InCCState == false)
                                    TempDiffTriggered = true;

                                LoggerManager.SoakingLog($"[SoakingModule] Occure Temp diff(preSV:{prevSV}, nextSV:{nextSV}), diif value:{TempDiffValue}, " +
                                    $"config_diffValue:{StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.PrepareStateConfig.TriggerDegreeForTempDiff.Value}, " +
                                    $"UseTempDiff_PrepareSoakingTime:{UseTempDiff_PrepareSoakingTime.ToString()}, Soaking State:{SoakingState.GetState().ToString()}");
                            }
                        }
                    }
                }
                else  // 기존 event soaking처리
                {
                    TempDiffEventSoaking tempdiffeventsoakingobj = GetEventSoakingObject(EnumSoakingType.TEMPDIFF_SOAK) as TempDiffEventSoaking;

                    if ((tempdiffeventsoakingobj != null) && (tempdiffeventsoakingobj.Enable.Value))
                    {
                        if ((this.TempController().TempInfo.TargetTemp.Value) + (tempdiffeventsoakingobj.OverTemperatureDifference.Value) < (this.TempController().TempInfo.CurTemp.Value) ||
                            (this.TempController().TempInfo.TargetTemp.Value) - (tempdiffeventsoakingobj.OverTemperatureDifference.Value) > (this.TempController().TempInfo.CurTemp.Value))
                        {
                            LoggerManager.Debug($"[SoakingModule] TempDiffSoak trigger on");
                            if (InCCState == false)
                            {
                                TempDiffTriggered = true;
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

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private SoakingState _SoakingState;
        private SoakingState SoakingState
        { get { return _SoakingState; } }
        public IInnerState InnerState
        {
            get { return _SoakingState; }
            set
            {
                if (value != _SoakingState)
                {
                    _SoakingState = value as SoakingState;
                }
            }
        }

        public IInnerState PreInnerState { get; set; }

        private bool _IsPreHeatEvent;

        public bool IsPreHeatEvent
        {
            get { return _IsPreHeatEvent; }
            set { _IsPreHeatEvent = value; }
        }
        private bool _IsLotEndEventOn = false;

        public bool IsLotEndEventOn
        {
            get { return _IsLotEndEventOn; }
            set { _IsLotEndEventOn = value; }
        }

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo;
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

        private IParam _SoakingDeviceFile_IParam;
        public IParam SoakingDeviceFile_IParam
        {
            get { return _SoakingDeviceFile_IParam; }
            set
            {
                if (value != _SoakingDeviceFile_IParam)
                {
                    _SoakingDeviceFile_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _SoakingSysParam_IParam;
        public IParam SoakingSysParam_IParam
        {
            get { return _SoakingSysParam_IParam; }
            set
            {
                if (value != _SoakingSysParam_IParam)
                {
                    _SoakingSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SoakingDeviceFile _SoakingDeviceFile_Clone;
        public SoakingDeviceFile SoakingDeviceFile_Clone
        {
            get { return _SoakingDeviceFile_Clone; }
            set { _SoakingDeviceFile_Clone = value; }
        }

        private SoakingSysParameter _SoakingSysParam_Clone;
        public SoakingSysParameter SoakingSysParam_Clone
        {
            get { return _SoakingSysParam_Clone; }
            set { _SoakingSysParam_Clone = value; }
        }

        private Queue<SoakingParamBase> _SoakPriorityList;
        public Queue<SoakingParamBase> SoakPriorityList
        {
            get { return _SoakPriorityList; }
            set { _SoakPriorityList = value; }
        }

        private IParam _StatusSoakingDeviceFile_IParam;
        public IParam StatusSoakingDeviceFile_IParam
        {
            get { return _StatusSoakingDeviceFile_IParam; }
            set
            {
                if (value != _StatusSoakingDeviceFile_IParam)
                {
                    _StatusSoakingDeviceFile_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private StatusSoakingDeviceFile _StatusSoakingDeviceFileObj;
        public StatusSoakingDeviceFile StatusSoakingDeviceFileObj
        {
            get { return _StatusSoakingDeviceFileObj; }
            set
            {
                if (value != _StatusSoakingDeviceFileObj)
                {
                    _StatusSoakingDeviceFileObj = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStatusSoakingParam _StatusSoakingParamIF;
        public IStatusSoakingParam StatusSoakingParamIF
        {
            get { return _StatusSoakingParamIF; }
            set
            {
                if (value != _StatusSoakingParamIF)
                {
                    _StatusSoakingParamIF = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSoakingDoneAfterWaferLoad;
        public bool IsSoakingDoneAfterWaferLoad
        {
            get { return _IsSoakingDoneAfterWaferLoad; }
            set
            {
                if (value != _IsSoakingDoneAfterWaferLoad)
                {
                    _IsSoakingDoneAfterWaferLoad = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Queue<SoakingParamBase> _RemainEventSoakingList;
        //public Queue<SoakingParamBase> RemainEventSoakingList
        //{
        //    get { return _RemainEventSoakingList; }
        //    set { _RemainEventSoakingList = value; }
        //}
        //private Queue<SoakingParamBase> _ProcessedEventSoakingList;
        //public Queue<SoakingParamBase> ProcessedEventSoakingList
        //{
        //    get { return _ProcessedEventSoakingList; }
        //    set { _ProcessedEventSoakingList = value; }
        //}

        //private Queue<SoakingParamBase> _RecipeQueue;
        //public Queue<SoakingParamBase> RecipeQueue
        //{
        //    get { return _RecipeQueue; }
        //    set { _RecipeQueue = value; }
        //}

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


        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        public EventProcessList SubscribeRecipeParam { get; set; }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set
            {
                _ForcedDone = value;
                if (this.LoaderController() != null)
                {
                    this.LoaderController()?.SetForcedDoneState();
                }
            }
        }
        private bool _SoakingCancelFlag;
        public bool SoakingCancelFlag
        {
            get { return _SoakingCancelFlag; }
            set
            {
                if (value != SoakingCancelFlag)
                {
                    _SoakingCancelFlag = value;
                }
            }
        }

        private bool isStatusSoakingAbort = false;
        public void SetSoakingAbort()
        {
            isStatusSoakingAbort = true;
        }
        public void ResetSoakingAbort()
        {
            isStatusSoakingAbort = false;
        }
        public bool GetSoakingAbort()
        {
            return isStatusSoakingAbort;
        }
        private bool _Idle_SoakingFailed_PinAlign = false;
        public bool Idle_SoakingFailed_PinAlign
        {
            get { return _Idle_SoakingFailed_PinAlign; }
            set
            {
                if (value != _Idle_SoakingFailed_PinAlign)
                {
                    _Idle_SoakingFailed_PinAlign = value;
                }
            }
        }

        private bool _Idle_SoakingFailed_WaferAlign = false;
        public bool Idle_SoakingFailed_WaferAlign
        {
            get { return _Idle_SoakingFailed_WaferAlign; }
            set
            {
                if (value != _Idle_SoakingFailed_WaferAlign)
                {
                    _Idle_SoakingFailed_WaferAlign = value;
                }
            }
        }

        private bool _MeasurementSoakingCancelFlag;
        public bool MeasurementSoakingCancelFlag
        {
            get { return _MeasurementSoakingCancelFlag; }
            set
            {
                if (value != _MeasurementSoakingCancelFlag)
                {
                    _MeasurementSoakingCancelFlag = value;
                }
            }
        }

        private bool _UsePreviousStatusSoakingDataForRunning = false;
        public bool UsePreviousStatusSoakingDataForRunning
        {
            get { return _UsePreviousStatusSoakingDataForRunning; }
            set
            {
                if (value != _UsePreviousStatusSoakingDataForRunning)
                {
                    _UsePreviousStatusSoakingDataForRunning = value;
                }
            }
        }


        public bool ManualSoakingStart { get; set; } = false; //manual soaking 시작을 사용자가 선택한 경우 true;
        public EventCodeEnum ManualSoakingRetVal { get; set; } = EventCodeEnum.NONE;

        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
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
                throw;
            }
            return retVal;
        }

        public ForceTransitionEnum StatusSoakingForceTransitionState { get; set; } = ForceTransitionEnum.NOT_NECESSARY;

        public double GetSoakingTime()
        {
            double retVal = 0;
            try
            {
                if (SoakPriorityList.Count > 0)
                {
                    retVal = SoakPriorityList.FirstOrDefault().SoakingTimeInSeconds.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public double GetSoakingTime(EnumSoakingType type)
        {
            double retVal = 0;
            try
            {
                if (SoakPriorityList.Count > 0)
                {
                    var param = SoakPriorityList.Where(soakinfo => soakinfo.EventSoakingType.Value == type);
                    if (param != null)
                    {
                        retVal = param.FirstOrDefault().SoakingTimeInSeconds.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public double GetZClearanceNoWaferOnChuck()
        {
            return this.StatusSoakingTempInfo?.notExistWaferObj_ODVal ?? -3000;
        }


        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
 
        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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
                throw;
            }
            return stat;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            return EventCodeEnum.UNDEFINED;
        }



        private IFocusing _WaferHighFocusModel;
        public IFocusing WaferHighFocusModel
        {
            get
            {
                if (_WaferHighFocusModel == null)
                    _WaferHighFocusModel = this.FocusManager().GetFocusingModel((SoakingSysParam_IParam as SoakingSysParameter).FocusingModuleDllInfo);

                return _WaferHighFocusModel;
            }
        }

        public IMetroDialogManager DialogManager => this.MetroDialogManager();

        //private IFocusParameter FocusParam => (IFocusParameter)(SoakingSysParam_IParam as SoakingSysParameter).FocusParams;

        /// <summary>
        /// 현재 loade된 device를 기준으로 이전 device에서  trigger된 soaking flag를 정리한다.
        /// 기존 device에서 trigger되었지만 현 load된 device에서 미사용인 경우에 대한 처리
        /// </summary>
        private void Check_N_Clear_SoakingTrigger(bool OldSoakTriggerAllInit = false)
        {
            if (LotResumeTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.LOTRESUME_SOAK) || OldSoakTriggerAllInit)
                {
                    LotResumeTriggered = false;

                    LoggerManager.Debug("[Soaking] 'LotResumeTriggered' flag is off");
                }
            }

            if (ChuckAwayTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.CHUCKAWAY_SOAK) || OldSoakTriggerAllInit)
                {
                    ChuckAwayTriggered = false;
                    LoggerManager.Debug("[Soaking] 'ChuckAwayTriggered' flag is off");
                }
            }
            if (TempDiffTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.TEMPDIFF_SOAK) || OldSoakTriggerAllInit)
                {
                    TempDiffTriggered = false;
                    LoggerManager.Debug("[Soaking] 'TempDiffTriggered' flag is off");
                }
            }

            if (ProbeCardChangeTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.PROBECARDCHANGE_SOAK) || OldSoakTriggerAllInit)
                {
                    ProbeCardChangeTriggered = false;
                    LoggerManager.Debug("[Soaking] 'ProbeCardChangeTriggered' flag is off");
                }
            }

            if (LotStartTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.LOTSTART_SOAK) || OldSoakTriggerAllInit)
                {
                    LotStartTriggered = false;
                    LoggerManager.Debug("[Soaking] 'LotStartTriggered' flag is off");
                }
            }

            if (DeviceChangeTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.DEVICECHANGE_SOAK) || OldSoakTriggerAllInit)
                {
                    DeviceChangeTriggered = false;
                    DeviceChangeFlag = false;
                    LoggerManager.Debug("[Soaking] 'DeviceChangeTriggered' flag is off");
                }
            }
            if (EveryWaferSoakTriggered)
            {
                if (false == IsEnableEventSokaing(EnumSoakingType.EVERYWAFER_SOAK) || OldSoakTriggerAllInit)
                {
                    EveryWaferSoakTriggered = false;
                    LoggerManager.Debug("[Soaking] 'EveryWaferSoakTriggered' flag is off");
                }
            }
            if (AutoSoakTriggered)
            {
                if (false == SoakingDeviceFile_Clone.AutoSoakingParam.Enable.Value || OldSoakTriggerAllInit)
                {
                    AutoSoakTriggered = false;
                    LoggerManager.Debug("[Soaking] 'AutoSoakTriggered' flag is off");
                }
            }
            if (SoakPriorityList != null && SoakPriorityList.Count > 0)
            {
                string triggered_event_soakingList = "";
                foreach (var soaking in SoakPriorityList)
                {
                    triggered_event_soakingList += soaking.EventSoakingType.Value;
                    triggered_event_soakingList += ",";
                }
                LoggerManager.Debug($"[Soaking] 'SoakPriorityList' queue clear(list: {triggered_event_soakingList})");
                SoakPriorityList.Clear();
            }
        }

        #region Save & Load Param

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new SoakingDeviceFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(SoakingDeviceFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    SoakingDeviceFile_IParam = tmpParam;
                    SoakingDeviceFile_Clone = SoakingDeviceFile_IParam as SoakingDeviceFile;

                    //device 로드 후 현 device에서 미사용으로 설정된 soaking 항목에 대한 trigger 초기화
                    Check_N_Clear_SoakingTrigger();

                    var soakparam = (this.SoakingModule().SoakingDeviceFile_IParam as SoakingDeviceFile).EventSoakingParams.
                        Single(param => param.EventSoakingType.Value == EnumSoakingType.LOTSTART_SOAK);
                    if (soakparam != null)
                    {
                        if (soakparam.Enable.Value)
                            this.GEMModule().GetPIVContainer().SetPreHeatState(GEMPreHeatStateEnum.PRE_HEATING);
                        else
                            this.GEMModule().GetPIVContainer().SetPreHeatState(GEMPreHeatStateEnum.NOT_PRE_HEATING);
                    }
                }
                else
                {
                    LoggerManager.Event($"Parameter load failed", "", EventlogType.EVENT);
                }

                bool firstLoading_DeviceParamData = false;
                bool beforeStatusSoakingUsingFlag = false;
                bool afterLoadingParam_StatusSoakingUsingFlag = false;
                if (null != this.StatusSoakingDeviceFileObj)
                {
                    beforeStatusSoakingUsingFlag = this.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.ShowStatusSoakingSettingPage.Value;
                }
                else
                    firstLoading_DeviceParamData = true; //StatusSoakingDeviceFileObj 가 null이라는 것은 프로그램 구동 후 최초 LoadDevParameter()를 타는 경우

                LoadStatusSoakingParameter(); //StatusSoaking Parameter load(return 부분 처리는 현재 해당 Load가 실패하여도 동작되어야 하기 때문에 별도 처리 안함. 추후 필요시 처리)

                if (false == firstLoading_DeviceParamData) //프로그램 시작 시점에 device file 로드 이후에 device가 바뀌는 경우, 이전 상태와 비교하여 StatusSoaking을 사용할 지 그렇지 않을지를 다시 결정해야 하기 때문에 PreOpState로 이동
                {
                    if (null != this.StatusSoakingDeviceFileObj)
                        afterLoadingParam_StatusSoakingUsingFlag = this.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.ShowStatusSoakingSettingPage.Value;

                    if (beforeStatusSoakingUsingFlag != afterLoadingParam_StatusSoakingUsingFlag)
                    {
                        LoggerManager.SoakingLog($"Loaded Device. so Status Soaking function will be changed: before StatusSoakingFlag({beforeStatusSoakingUsingFlag.ToString()}), after StatusSoakingFlag({afterLoadingParam_StatusSoakingUsingFlag.ToString()})");
                        _SoakingState = new PreOpState(this, true);
                    }
                    else//디바이스 바뀌었는데 StatusSoakingFlag는 그대로지만 안에 내용이 달라졌을 수 있으니 Clear State한다.
                    {
                        if (afterLoadingParam_StatusSoakingUsingFlag == true)
                        {
                            LoggerManager.SoakingLog($"[Soaking Module] LoadDevParameter() : Clear State()");
                            ClearState();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        /// <summary>
        /// StatusSoakingDeviceFile Load 처리
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum LoadStatusSoakingParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = null;
                tmpParam = new StatusSoakingDeviceFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                lock (load_statusSoakinfParam_lockObject)
                {
                    RetVal = this.LoadParameter(ref tmpParam, typeof(StatusSoakingDeviceFile));
                }

                if (RetVal == EventCodeEnum.NONE)
                {
                    StatusSoakingDeviceFile_IParam = tmpParam;
                    StatusSoakingDeviceFileObj = StatusSoakingDeviceFile_IParam as StatusSoakingDeviceFile;
                    StatusSoakingParamIF = StatusSoakingDeviceFileObj as IStatusSoakingParam;
                }
                else
                {
                    LoggerManager.Event($"Parameter load failed(StatusSoakingDeviceFile)", "", EventlogType.EVENT);
                    LoggerManager.SoakingErrLog($"Parameter load failed(StatusSoakingDeviceFile)");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                RetVal = EventCodeEnum.EXCEPTION;
            }

            return RetVal;
        }


        public EventCodeEnum SaveDevParameter()
        {
            return SaveSoakingDeviceFile();
        }

        public EventCodeEnum LoadParameter()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                ret = LoadDevParameter();

                if (ret != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[Soaking] LoadDevParam(): Serialize Error");
                    ret = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = SaveDevParameter();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[Soaking] SaveDevParameter(): Serialize Error");
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadSoakingDeviceFile()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                SoakingDeviceFile_Clone = new SoakingDeviceFile();

                string fullPath = this.FileManager().GetDeviceParamFullPath(SoakingDeviceFile_Clone.FilePath, SoakingDeviceFile_Clone.FileName);

                try
                {
                    IParam tmpParam = null;
                    ret = this.LoadParameter(ref tmpParam, typeof(SoakingDeviceFile));
                    if (ret == EventCodeEnum.NONE)
                    {
                        SoakingDeviceFile_Clone = tmpParam as SoakingDeviceFile;
                    }

                    LoadStatusSoakingParameter();
                }
                catch (Exception err)
                {
                    ret = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("[Soaking] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public EventCodeEnum SaveSoakingDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                string fullPath = this.FileManager().GetDeviceParamFullPath(SoakingDeviceFile_Clone.FilePath, SoakingDeviceFile_Clone.FileName);

                try
                {
                    RetVal = Extensions_IParam.SaveParameter(null, SoakingDeviceFile_Clone, null, fullPath);
                    SaveStatusSoakingDeviceFile();
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    //LoggerManager.Error($String.Format("[Soaking] SaveSoakingDeviceFile(): Serialize Error. Err = {0}", err.Message));
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        /// <summary>
        /// Status Soaking에 관련된 parameter를 파일로 저장.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum SaveStatusSoakingDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                string fullPath = this.FileManager().GetDeviceParamFullPath(StatusSoakingDeviceFileObj.FilePath, StatusSoakingDeviceFileObj.FileName);
                try
                {
                    lock (save_statusSoakinfParam_lockObject)
                    {
                        RetVal = Extensions_IParam.SaveParameter(null, StatusSoakingDeviceFileObj, null, fullPath);
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Exception(err);
                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }


        public void SetDevParam(byte[] param)
        {
            try
            {
                string fullPath = this.FileManager().GetDeviceParamFullPath(SoakingDeviceFile_Clone.FilePath, SoakingDeviceFile_Clone.FileName);
                using (Stream stream = new MemoryStream(param))
                {
                    this.DecompressFilesFromByteArray(stream, fullPath);
                }
                LoadDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {
                isInjected = SoakingState.CanExecute(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        //brett// 해당 함수는 soaking idle state에서 지속적으로 호출될 수 있는 함수 이므로 로그 추가시 주의 해야함
        public EventCodeEnum GetTargetPosAccordingToCondition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, bool bPrintLog = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //wafercoord = this.WaferAligner().MachineIndexConvertToProbingCoord((int)this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value,
                //        (int)this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value);
                wafercoord = new WaferCoordinate(0, 0, this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick);
                pincoord.X.Value = 0;
                pincoord.Y.Value = 0;
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                var waferstatus = this.StageSupervisor().WaferObject.GetStatus();

                if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                   this.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                {
                    if (waferstatus == EnumSubsStatus.EXIST)
                    {
                        var wafer = (WaferObject)this.StageSupervisor().WaferObject;
                        wafercoord.X.Value = wafer.GetSubsInfo().WaferCenter.X.Value;
                        wafercoord.Y.Value = wafer.GetSubsInfo().WaferCenter.Y.Value;
                        wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick;

                        pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                        pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                        pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                    }
                    else
                    {
                        pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                        pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                        pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                        //핀위치 얻어오고 + 웨이퍼는 맥스씨크니스
                        //wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        wafercoord.X.Value = 0.0;
                        wafercoord.Y.Value = 0.0;
                        wafercoord.Z.Value = 0.0;
                    }
                }
                else if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE)
                {
                    if (waferstatus == EnumSubsStatus.EXIST)
                    {
                        var wafer = (WaferObject)this.StageSupervisor().WaferObject;
                        wafercoord.X.Value = wafer.GetSubsInfo().WaferCenter.X.Value;
                        wafercoord.Y.Value = wafer.GetSubsInfo().WaferCenter.Y.Value;
                        wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick;

                        pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                        pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                        pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                    }
                    else
                    {
                        pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                        pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                        pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                        //핀위치 얻어오고 + 웨이퍼는 맥스씨크니스
                        //wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        wafercoord.X.Value = 0.0;
                        wafercoord.Y.Value = 0.0;
                        wafercoord.Z.Value = 0.0;
                    }
                }
                else
                {
                    // 웨이퍼맥스씨크니스 + 핀 민레그레인지 
                    if (waferstatus == EnumSubsStatus.EXIST)
                    {
                        var wafer = (WaferObject)this.StageSupervisor().WaferObject;
                        wafercoord.X.Value = wafer.GetSubsInfo().WaferCenter.X.Value;
                        wafercoord.Y.Value = wafer.GetSubsInfo().WaferCenter.Y.Value;
                        wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().AveWaferThick;

                        pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                        pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                        pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                    }
                    else
                    {
                        pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                        pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                        pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;

                        wafercoord.X.Value = 0.0;
                        wafercoord.Y.Value = 0.0;
                        wafercoord.Z.Value = 0.0;
                    }
                }
                if (bPrintLog)
                {
                    LoggerManager.Debug($"GetTargetPosAccordingToCondition(): wafercoord X:{wafercoord.X.Value}, Y:{wafercoord.Y.Value}, Z:{wafercoord.Z.Value}");
                    LoggerManager.Debug($"GetTargetPosAccordingToCondition(): pincoord X:{pincoord.X.Value}, Y:{pincoord.Y.Value}, Z:{pincoord.Z.Value}");
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// wafer center coord 가져오기
        /// </summary>
        /// <param name="wafercoord"></param>
        /// <returns> EventCodeEnum </returns>
        private EventCodeEnum GetWaferCenterPosition(ref WaferCoordinate wafercoord, bool logWrite)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            // Wafer Center or PadCenter 결정
            var wafer_object = (WaferObject)this.StageSupervisor().WaferObject;
            wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
            wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
            MachineIndex MI = new MachineIndex();
            try
            {
                retVal = this.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);
                if (retVal == EventCodeEnum.NONE)
                {
                    lock (get_wafercenter_lockObject)
                    {
                        var Wafer = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex, logWrite);
                        wafercoord.X.Value = Wafer.X.Value;
                        wafercoord.Y.Value = Wafer.Y.Value;
                    }
                }
                else
                {
                    wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                    wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingLog($"Probing GetFirstSequence() Error. Used WaferCenter Position");
                wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
            }

            wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
            return retVal;
        }

        /// <summary>
        /// Chuck focusiong을 통한 좌표산출(Wafer없을 때)
        /// </summary>
        /// <param name="wafercoord"> wafer 좌표</param>
        /// <param name="pincoord"> pin whkvy</param>
        /// <param name="refHeight"> referance 할 높이</param>
        /// <returns> EventCodeEnum </returns>
        private EventCodeEnum Get_WafercoordByUsingChuckFocus(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, double refHeight)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            ImageBuffer imageBuffer = null;
            try
            {
                if ((FocusingParam as FocusParameter).LightParams == null || (FocusingParam as FocusParameter).LightParams.Count == 0)
                {
                    (FocusingParam as FocusParameter).LightParams = new ObservableCollection<LightValueParam>();
                    LightValueParam light = new LightValueParam();
                    light.Type.Value = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.LightType.Value;
                    light.Value.Value = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.LightValue.Value; ;
                    (FocusingParam as FocusParameter).LightParams.Add(light);
                }

                foreach (var light in (FocusingParam as FocusParameter).LightParams)
                {
                    this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(light.Type.Value, light.Value.Value);
                }

                if (FocusingParam.FocusRange.Value == 0)
                {
                    LoggerManager.SoakingLog($"FocusingParam.FocusRange.Value is zero. so it will be default value");
                    FocusingParam.SetDefaultParam();
                }

                FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.ChuckFocusingFlatnessThd.Value;
                
                retval = FocusingModule.Focusing_Retry(FocusingParam, true, true, false, this);

                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurImage(out imageBuffer);

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.SoakingErrLog($"GetSafePosition(), Focusing fail, return value : {retval}");
                    wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                    SaveFocusingImage(imageBuffer, "fail");
                    return retval;
                }
                else
                {
                    WaferCoordinate wfcoord = new WaferCoordinate();
                    wfcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();//현위치
                    double after_Thickness = wfcoord.Z.Value;
                    double heightDiff = Math.Abs(after_Thickness - refHeight);

                    //current StautsSoking state
                    double NotExistWafer_ODValue = 0;
                    if (false == StatusSoakingDeviceFileObj.Get_NotExistWaferObj_OD(SoakingState.GetState(), ref NotExistWafer_ODValue))
                    {
                        LoggerManager.SoakingErrLog($"Failed to 'Get_NotExistWaferObj_OD' : SoakingState:{SoakingState.GetState()}");
                        return EventCodeEnum.PARAM_ERROR; 
                    }

                    //device file과 방금 측정된 높이 차이가 사용자가 지정한 허용오차보다 작아야한다
                    //device file과 방금 측정된 높이 차이가 클리어런스보다 작아야하고
                    if (heightDiff < StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ThicknessTolerance.Value)
                    {
                        wafercoord = wfcoord;
                        SaveFocusingImage(imageBuffer, "success");
                    }
                    else
                    {
                        LoggerManager.SoakingLog($"The measured thickness does not meet the conditions." +
                            $"Thickness_Tolerance.Value = {StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ThicknessTolerance.Value}, " +
                            $"Not Exist Wafer OD_Val = {NotExistWafer_ODValue}," +
                            $"heightDiff = {heightDiff}, after thickness : {after_Thickness}, before thickness : {refHeight} ");
                        wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                        SaveFocusingImage(imageBuffer, "outOfcondition");
                        return EventCodeEnum.SOAKING_TOLERANCE_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Exception - 'Get_WafercoordByUsingChuckFocus'");
            }

            return retval;
        }

        /// <summary>
        /// Status Soaking을위한 Position정보를 반환
        /// (pin 및 wafer align이 되지 않은 상태이면 안전한 값으로 반환한다.)
        /// </summary>
        /// <param name="wafercoord"> wafer 좌표 </param>
        /// <param name="pincoord"> pin 좌표 </param>
        /// <param name="use_chuck_focusing"> chuck focusing 사용 여부</param>
        /// <returns> EventCodeEnum </returns>        
        public EventCodeEnum Get_StatusSoakingPosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, bool use_chuck_focusing = true, bool logWrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            ImageBuffer imageBuffer = new ImageBuffer();

            try
            {

                var waferExist = this.StageSupervisor().WaferObject.GetStatus();
                var waferType = this.StageSupervisor().WaferObject.GetWaferType();

                //wafer status가 unknown인지 체크. unknown이면 안전한 위치로 처리해야 한다.
                if (EnumSubsStatus.UNKNOWN == waferExist)
                {
                    if (logWrite)
                    {
                        LoggerManager.SoakingErrLog($"'WaferExist' is unknown. so position will be safety postion.");
                    }
                    wafercoord.X.Value = 0;
                    wafercoord.Y.Value = 0;
                    wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;

                    pincoord.X.Value = 0;
                    pincoord.Y.Value = 0;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                    if (logWrite)
                    {
                        LoggerManager.SoakingErrLog($"The wafer status is not correct. wafercoord X, Y, Z : {wafercoord.X.Value}, {wafercoord.Y.Value}, {wafercoord.Z.Value} pincoord X, Y, Z : {pincoord.X.Value}, {pincoord.Y.Value}, {pincoord.Z.Value}");
                    }
                    return EventCodeEnum.NONE;
                }

                var wafer = (WaferObject)this.StageSupervisor().WaferObject;
                wafercoord.X.Value = wafer.GetSubsInfo().WaferCenter.X.Value;
                wafercoord.Y.Value = wafer.GetSubsInfo().WaferCenter.Y.Value;
                wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;

                //polish wafer나 chuck위에 아무것도 없는 경우는 wafercoord를 0,0으로 초기화 후 시작
                if (waferExist != EnumSubsStatus.EXIST || waferType == EnumWaferType.POLISH)
                {
                    wafercoord.X.Value = 0;
                    wafercoord.Y.Value = 0;
                    pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                    pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                }
                else
                {
                    pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                    pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                }
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                if (false == this.CardChangeModule().IsExistCard() || this.StageSupervisor().PinAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    pincoord.X.Value = 0;
                    pincoord.Y.Value = 0;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                }

                if (logWrite)
                {
                    LoggerManager.SoakingLog($"Alignment Info: Wafer Align({this.StageSupervisor().WaferObject.AlignState.Value.ToString()}), Pin Align({this.StageSupervisor().ProbeCardInfo.AlignState.Value.ToString()}), wafer_exist({waferExist.ToString()}), waferType:({waferType.ToString()})");
                }

                if (use_chuck_focusing) //soaking을 위한 position을 가져오는 경우로 focusing등 수행 가능
                {
                    if (waferExist == EnumSubsStatus.EXIST)
                    {
                        if (ChillingTimeMngObj.IsShowDebugString())
                            Trace.WriteLine($"[SoakingModule][StatusSoakingDbg] Wafer Exist, PinAlign:{this.StageSupervisor().ProbeCardInfo.AlignState.Value} , WaferAlign:{this.StageSupervisor().WaferObject.AlignState.Value}");

                        if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                        this.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                        {
                            retval = GetWaferCenterPosition(ref wafercoord, logWrite);
                            if (logWrite)
                            {
                                LoggerManager.SoakingLog($"use_chuck_focusing - 'GetWaferCenterPosition', wafercoord(x:{wafercoord.X.Value.ToString()}, y:{wafercoord.Y.Value.ToString()}, z:{wafercoord.Z.Value.ToString()})");
                            }

                            if (EventCodeEnum.NONE != retval)
                            {
                                LoggerManager.SoakingErrLog($"Failed to 'GetWaferCenterPosition'(wafer type: {waferType.ToString()})");
                                return retval;
                            }
                        }
                        else if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE && //pin만 align되어 있고 wafer align이 안되어 있는 경우.(wafer object focusing)
                                    this.StageSupervisor().WaferObject.AlignState.Value != AlignStateEnum.DONE)
                        {
                            retval = GetSafePosition(ref wafercoord, ref pincoord, true);

                            if (logWrite)
                            {
                                LoggerManager.SoakingLog($"Use - GetSaferPostion, wafercoord(x:{wafercoord.X.Value.ToString()}, y:{wafercoord.Y.Value.ToString()}, z:{wafercoord.Z.Value.ToString()})");
                                double waferobj_thickness = 0;

                                if (waferType == EnumWaferType.POLISH)
                                {
                                    waferobj_thickness = this.StageSupervisor().WaferObject.GetPolishInfo().Thickness.Value;
                                }
                                else if (waferType == EnumWaferType.STANDARD)
                                {
                                    waferobj_thickness = this.StageSupervisor().WaferObject.GetPhysInfo().Thickness.Value;
                                }

                                if (waferobj_thickness == 0)
                                {
                                    LoggerManager.SoakingErrLog($"wafer object({waferType.ToString()}) thickness is zero!!. use PinMinRegRange, WaferMaxThickness");
                                    //hhh_todo: 두께값이 없는 경우는 error인가? 확인 필요
                                }
                            }

                            if (EventCodeEnum.NONE != retval)
                            {
                                LoggerManager.SoakingErrLog($"Failed to 'GetSafePosition'(wafer type: {waferType.ToString()})");
                                return retval;
                            }
                        }
                        else //pin align, wafer align이 안되어 있는 경우, 안전한 값으로..
                        {
                            if (ChillingTimeMngObj.IsShowDebugString())
                                Trace.WriteLine($"[SoakingModule][StatusSoakingDbg] Card or wafer are not aligned. so it will be safe position.( wafercoord.Z: WaferMaxThickness({this.StageSupervisor().WaferMaxThickness}), pincoord.Z: PinMinRegRange({this.StageSupervisor().PinMinRegRange})");

                            wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                            pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                            if (logWrite)
                            {
                                LoggerManager.SoakingLog($"Use Safe value - wafercoord.Z({wafercoord.Z.Value.ToString()}), pincoord.Z(PinMinRegRange)({pincoord.Z.Value.ToString()})");
                            }
                        }
                    }
                    else  //chuck위에 아무것도 없는 경우 (Chuck focusing)
                    {
                        if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                               this.StageSupervisor().WaferObject.AlignState.Value != AlignStateEnum.DONE)
                        {
                            wafercoord.X.Value = 0;
                            wafercoord.Y.Value = 0;
                            double refHeight = (double)SoakingSysParam_Clone.ChuckRefHight.Value;
                            retval = this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, refHeight);
                            if (retval != EventCodeEnum.NONE)
                            {
                                wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                                pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                                LoggerManager.SoakingErrLog($"Failed to 'WaferHighViewMove', return value : {retval.ToString()}, x:0, y:0, z:{refHeight}");
                                return retval;
                            }
                            else
                            {
                                if (logWrite)
                                {
                                    LoggerManager.SoakingLog($"Wafer does not exist. so start chuck focusing");
                                }

                                retval = Get_WafercoordByUsingChuckFocus(ref wafercoord, ref pincoord, refHeight);
                                (FocusingParam as FocusParameter).LightParams = null;
                            }
                        }
                        else
                        {
                            wafercoord.X.Value = 0;
                            wafercoord.Y.Value = 0;
                            wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        }
                    }

                    return retval;
                }
                else //chillingTime manager와 같이 별도 Thread에서 position을 참조하는 경우로 focusing을 하면 안되는 case로 이미 처리된 값만 참조하여 사용..
                {
                    if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                        this.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                    {
                        if (waferExist == EnumSubsStatus.EXIST)
                        {
                            if (logWrite)
                            {
                                LoggerManager.SoakingLog($"Don't use chuck focusing - 'GetWaferCenterPosition'");
                            }

                            retval = GetWaferCenterPosition(ref wafercoord, logWrite);
                            if (EventCodeEnum.NONE != retval)
                            {
                                LoggerManager.SoakingErrLog($"Failed to 'GetWaferCenterPosition'(wafer type: {waferType.ToString()})");
                                return retval;
                            }
                        }
                        else
                        {
                            LoggerManager.SoakingErrLog($"Pin and wafer alignment are good. but wafer does not exist");
                        }
                    }
                    else if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                         this.StageSupervisor().WaferObject.AlignState.Value != AlignStateEnum.DONE) //pin align만 되어 있고, wafer align은 안되어 있는 경우
                    {
                        if (this.CardChangeModule().IsExistCard() && this.StageSupervisor().PinAligner().ForcedDone == EnumModuleForcedState.Normal)
                        {
                            pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                            pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                            pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                        }

                        if (logWrite)
                        {
                            LoggerManager.SoakingLog($"Don't use chuck focusing - 'wafer exist'(type:{waferType.ToString()})");
                        }

                        if(waferExist == EnumSubsStatus.NOT_EXIST)
                        {
                            wafercoord.X.Value = 0;
                            wafercoord.Y.Value = 0;
                            if(SoakingSysParam_Clone.ChuckFocusingSkip.Value == true)//Chuck Focusing Skip한 경우  Move위치 구할 때에 Monitoring에서 타는 로직을 탐. ChuckRefheight로 갈 수 있도록 값을 넣어줌.
                            {
                                wafercoord.Z.Value = SoakingSysParam_Clone.ChuckRefHight.Value;
                            }
                            else
                            {
                                wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                            }
                        }
                        else
                        {
                            wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        }

                        if (waferType == EnumWaferType.POLISH)
                        {
                            wafercoord.X.Value = 0;
                            wafercoord.Y.Value = 0;
                            wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        }
                    }
                    else  //pin align, wafer align 모두 안되어 있는 경우
                    {
                        if (logWrite)
                        {
                            LoggerManager.SoakingLog($"Don't use chuck focusing - pin align(X), wafer align(X), WaferExist:{waferExist.ToString()}, WaferType:{waferType.ToString()}");
                        }

                        if (waferExist == EnumSubsStatus.NOT_EXIST || waferType == EnumWaferType.POLISH)
                        {
                            wafercoord.X.Value = 0;
                            wafercoord.Y.Value = 0;
                        }
                        wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;

                        if (this.CardChangeModule().IsExistCard())
                        {
                            pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                            pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                            pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                        }
                    }

                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog(err.Message);
                retval = EventCodeEnum.UNKNOWN_EXCEPTION;
            }
            finally
            {
                imageBuffer = null;
            }

            return retval;
        }

        private void SetDefaultPosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord)
        {
            try
            {
                var wafer = (WaferObject)this.StageSupervisor().WaferObject;

                wafercoord = new WaferCoordinate(0, 0, this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness);
                wafercoord.X.Value = wafer.GetSubsInfo().WaferCenter.X.Value;
                wafercoord.Y.Value = wafer.GetSubsInfo().WaferCenter.Y.Value;
                wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;

                pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum ChuckFocusing(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            ImageBuffer imageBuffer;

            try
            {
                retval = PerformFocusing();

                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurImage(out imageBuffer);

                if (retval != EventCodeEnum.NONE)
                {
                    ReasonOfError.AddEventCodeInfo(retval, $"Soaking error occured while focusing", GetType().Name);
                    LoggerManager.Debug($"[{this.GetType().Name}], ChuckFocusing(), Focusing fail, retval = {retval}, FlatnessThreshold : {FocusingParam.FlatnessThreshold.Value}");
                    SetSafeZPosition(ref wafercoord, ref pincoord);
                    SaveFocusingImage(imageBuffer, "fail");
                }
                else
                {
                    WaferCoordinate wfcoord = new WaferCoordinate();

                    wfcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();//현위치
                    double after_Thickness = wfcoord.Z.Value;
                    double heightDiff = Math.Abs(after_Thickness - SoakingSysParam_Clone.ChuckRefHight.Value);

                    //device file과 방금 측정된 높이 차이가 사용자가 지정한 허용오차보다 작아야한다
                    //device file과 방금 측정된 높이 차이가 클리어런스보다 작아야하고
                    if (heightDiff < SoakingDeviceFile_Clone.AutoSoakingParam.Thickness_Tolerance.Value &&
                        heightDiff < Math.Abs(SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value))
                    {
                        wafercoord = wfcoord;

                        SaveFocusingImage(imageBuffer, "success");

                        LoggerManager.Debug($"\n[SoakingModule] ChuckFocusing(), Focusing Success.\n" +
                            $"Flatness Threshold: { FocusingParam.FlatnessThreshold.Value}\n" +
                            $"Thickness_Tolerance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.Thickness_Tolerance.Value}\n" +
                            $"ZClearance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value}\n" +
                            $"heightDiff = {heightDiff}, after thickness : {after_Thickness}, chuck thickness : {SoakingSysParam_Clone.ChuckRefHight.Value}");
                    }
                    else
                    {
                        LoggerManager.Debug($"The measured thickness does not meet the conditions." +
                            $"Thickness_Tolerance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.Thickness_Tolerance.Value}, " +
                            $"ZClearance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value}," +
                            $"heightDiff = {heightDiff}, after thickness : {after_Thickness}, chuck thickness : {SoakingSysParam_Clone.ChuckRefHight.Value} ");

                        SetSafeZPosition(ref wafercoord, ref pincoord);

                        SaveFocusingImage(imageBuffer, "outOfcondition");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum PerformFocusing()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if ((FocusingParam as FocusParameter).LightParams == null || (FocusingParam as FocusParameter).LightParams.Count == 0)
                {
                    (FocusingParam as FocusParameter).LightParams = new ObservableCollection<LightValueParam>();

                    LightValueParam light = new LightValueParam();

                    light.Type.Value = SoakingDeviceFile_Clone.AutoSoakingParam.LightType.Value;
                    light.Value.Value = SoakingDeviceFile_Clone.AutoSoakingParam.LightValue.Value; ;

                    (FocusingParam as FocusParameter).LightParams.Add(light);
                }

                foreach (var light in (FocusingParam as FocusParameter).LightParams)
                {
                    this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(light.Type.Value, light.Value.Value);
                }

                if (FocusingParam.FocusRange.Value == 0)
                {
                    FocusingParam.SetDefaultParam();
                }

                FocusingParam.FlatnessThreshold.Value = SoakingDeviceFile_Clone.AutoSoakingParam.ChuckFocusingFlatnessThd.Value;

                //디바이스 파일에 있는 Chuck Thd가 시스템 파일에 있는 Chuck Thd보다 크다면 시스템에 있는걸 쓰자.
                if (FocusingParam.FlatnessThreshold.Value >= SoakingSysParam_Clone.ChuckFocusingFlatnessThd.Value)
                {
                    FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.ChuckFocusingFlatnessThd.Value;
                }

                retval = FocusingModule.Focusing_Retry(FocusingParam, true, true, false, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private void SetSafeZPosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord)
        {

            try
            {
                wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum MoveToChuckRefHeightBeforeChuckFocusing()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, SoakingSysParam_Clone.ChuckRefHight.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum HandleAlignDoneState(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var waferObject = (WaferObject)this.StageSupervisor().WaferObject;

                double waferCenterX = waferObject.GetSubsInfo().WaferCenter.X.Value;
                double waferCenterY = waferObject.GetSubsInfo().WaferCenter.Y.Value;

                MachineIndex MI = new MachineIndex();

                try
                {
                    retval = this.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);

                    if (retval == EventCodeEnum.NONE)
                    {
                        var Wafer = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);

                        wafercoord.X.Value = Wafer.X.Value;
                        wafercoord.Y.Value = Wafer.Y.Value;
                        wafercoord.T.Value = Wafer.T.Value;

                        LoggerManager.Debug($"[{this.GetType().Name}], HandleAlignDoneState() : Used GetFirstSequence Position");
                    }
                    else
                    {
                        wafercoord.X.Value = waferCenterX;
                        wafercoord.Y.Value = waferCenterY;

                        LoggerManager.Debug($"[{this.GetType().Name}], HandleAlignDoneState() : Used WaferCenter Position");
                    }
                }
                catch (Exception err)
                {
                    Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);

                    LoggerManager.Debug($"[{this.GetType().Name}], HandleAlignDoneState() : Probing GetFirstSequence() Error. Used WaferCenter Position");

                    wafercoord.X.Value = waferObject.GetSubsInfo().WaferCenter.X.Value;
                    wafercoord.Y.Value = waferObject.GetSubsInfo().WaferCenter.Y.Value;
                }

                wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;

                pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum ProcessWaferExists(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, EnumSoakingType soakingType)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], ProcessWaferExists(), PinAlign : {this.StageSupervisor().ProbeCardInfo.AlignState.Value} , WaferAlign : {this.StageSupervisor().WaferObject.AlignState.Value}");

                var waferObject = (WaferObject)this.StageSupervisor().WaferObject;
                var waferStatus = waferObject.GetStatus();

                double waferCenterX = waferObject.GetSubsInfo().WaferCenter.X.Value;
                double waferCenterY = waferObject.GetSubsInfo().WaferCenter.Y.Value;

                var waferType = this.StageSupervisor().WaferObject.GetWaferType();

                if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                    this.StageSupervisor().WaferObject.AlignState.Value == AlignStateEnum.DONE)
                {
                    retval = HandleAlignDoneState(ref wafercoord, ref pincoord);
                }
                else if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE &&
                    this.StageSupervisor().WaferObject.AlignState.Value != AlignStateEnum.DONE)
                {
                    retval = GetSafePosition(ref wafercoord, ref pincoord);
                }
                else
                {
                    // 이곳으로 왔다는건 핀 얼라인 실패
                    SetSafeZPosition(ref wafercoord, ref pincoord);
                }

                if (retval != EventCodeEnum.NONE)
                {
                    SetSafeZPosition(ref wafercoord, ref pincoord);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum ProcessWaferNotExists(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}] ProcessWaferNotExists(), PinAlign:{this.StageSupervisor().ProbeCardInfo.AlignState.Value}");

                if (this.StageSupervisor().ProbeCardInfo.AlignState.Value == AlignStateEnum.DONE)
                {
                    retval = MoveToChuckRefHeightBeforeChuckFocusing();

                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = ChuckFocusing(ref wafercoord, ref pincoord);
                    }
                    else
                    {
                        SetSafeZPosition(ref wafercoord, ref pincoord);
                        LoggerManager.Debug($"[{this.GetType().Name}] ProcessWaferNotExists(), wafer high view move fail, return value : {retval}");
                        this.ReasonOfError.AddEventCodeInfo(retval, $"Soaking Error, WaferHighViewMove(0, 0, {SoakingSysParam_Clone.ChuckRefHight.Value})", GetType().Name);

                        StateTransitionToErrorState(retval);
                    }
                }
                else
                {
                    if (this.CardChangeModule().IsExistCard())
                    {
                        // 카드가 존재함에도 불구하고 이곳을 탄다는 건 핀 얼라인이 실패했다는 의미

                        this.ReasonOfError.AddEventCodeInfo(retval, $"[{retval}] error occurred while Soaking.", GetType().Name);
                        LoggerManager.Debug($"[{this.GetType().Name}] ProcessWaferNotExists(), Error occurred while Soaking : Pin align fail, return value : {retval}");

                        SetSafeZPosition(ref wafercoord, ref pincoord);
                    }
                    else
                    {
                        retval = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetSoakingPosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, EnumSoakingType soakingType)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SetDefaultPosition(ref wafercoord, ref pincoord);

                var waferObject = (WaferObject)this.StageSupervisor().WaferObject;
                var waferStatus = waferObject.GetStatus();

                if (waferStatus == EnumSubsStatus.UNKNOWN)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], GetSoakingPosition() : The wafer status is not correct. Wafer Status: {waferStatus}");

                    wafercoord.X.Value = 0;
                    wafercoord.Y.Value = 0;
                    wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;

                    pincoord.X.Value = 0;
                    pincoord.Y.Value = 0;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                }
                else
                {
                    if (waferStatus == EnumSubsStatus.EXIST)
                    {
                        retval = ProcessWaferExists(ref wafercoord, ref pincoord, soakingType);
                    }
                    else
                    {
                        retval = ProcessWaferNotExists(ref wafercoord, ref pincoord);
                    }
                }

                if (SoakingState.GetModuleState() != ModuleStateEnum.ERROR)
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (SoakingState.GetModuleState() != ModuleStateEnum.ERROR)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), wafercoord(X, Y ,Z) = { wafercoord.X.Value}, {wafercoord.Y.Value}, {wafercoord.Z.Value}, retval = {retval}");
                    LoggerManager.Debug($"[{this.GetType().Name}] GetSoakingPosition(), pincoord(X, Y ,Z) = { pincoord.X.Value}, {pincoord.Y.Value}, {pincoord.Z.Value}, retval = {retval}");
                }
            }

            (FocusingParam as FocusParameter).LightParams = null;

            return retval;
        }

        public EventCodeEnum GetSafePosition(ref WaferCoordinate wafercoord, ref PinCoordinate pincoord, bool useStatusSoaking = false)
        {
            ImageBuffer imageBuffer = null;
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var waferType = this.StageSupervisor().WaferObject.GetWaferType();

                if (waferType == EnumWaferType.POLISH)
                {
                    var polish_thickness = this.StageSupervisor().WaferObject.GetPolishInfo().Thickness.Value;

                    if (polish_thickness == 0)
                    {
                        wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                        retval = EventCodeEnum.NONE;
                        return retval;
                    }

                    retval = this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, polish_thickness);
                }
                else if (waferType == EnumWaferType.STANDARD)
                {
                    var wafer_thickness = this.StageSupervisor().WaferObject.GetPhysInfo().Thickness.Value;

                    if (wafer_thickness == 0)
                    {
                        wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                        retval = EventCodeEnum.NONE;
                        return retval;
                    }

                    retval = this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, wafer_thickness);
                }
                else
                {
                    wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                    retval = EventCodeEnum.NONE;

                    return retval;
                }

                if (retval != EventCodeEnum.NONE)//Focusing 전에 wafer high view move에서 None이 안나오면 Soaking error
                {
                    wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;

                    LoggerManager.SoakingErrLog($"[SoakingModule] GetSafePosition(), wafer high view move fail, return value : {retval}");
                    this.ReasonOfError.AddEventCodeInfo(retval, "Soaking Error", "");

                    if (false == useStatusSoaking)
                    {
                        StateTransitionToErrorState(retval);
                    }

                    return retval;
                }

                //focusing으로 두께 다시 측정
                var wafer = (WaferObject)this.StageSupervisor().WaferObject;
                wafercoord.X.Value = wafer.GetSubsInfo().WaferCenter.X.Value;
                wafercoord.Y.Value = wafer.GetSubsInfo().WaferCenter.Y.Value;

                double before_Thickness = 0;

                if (waferType == EnumWaferType.POLISH)
                {
                    before_Thickness = this.StageSupervisor().WaferObject.GetPolishInfo().Thickness.Value;
                    FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.PolishFocusingFlatnessThd.Value;
                }
                else
                {
                    before_Thickness = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
                    FocusingParam.FlatnessThreshold.Value = SoakingSysParam_Clone.ProcessingFocusingFlatnessThd.Value;
                }

                if (before_Thickness == 0)
                {
                    before_Thickness = this.StageSupervisor().WaferMaxThickness;
                    retval = EventCodeEnum.SOAKING_TOLERANCE_ERROR;
                    LoggerManager.Debug($"[SoakingModule] GetSafePosition(), before_Thickness : {before_Thickness}, return value : {retval}");
                    return retval;
                }

                if ((FocusingParam as FocusParameter).LightParams == null || (FocusingParam as FocusParameter).LightParams.Count == 0)
                {
                    (FocusingParam as FocusParameter).LightParams = new ObservableCollection<LightValueParam>();
                    if (null != (FocusingParam as FocusParameter).LightParams)
                    {
                        LightValueParam light = new LightValueParam();
                        if (null != light)
                        {
                            if (false == useStatusSoaking)
                            {
                                light.Type.Value = SoakingDeviceFile_Clone.AutoSoakingParam.LightType.Value;
                                light.Value.Value = SoakingDeviceFile_Clone.AutoSoakingParam.LightValue.Value;
                            }
                            else
                            {
                                light.Type.Value = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.LightType.Value;
                                light.Value.Value = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.LightValue.Value;
                            }

                            (FocusingParam as FocusParameter).LightParams.Add(light);
                        }
                    }
                }

                foreach (var light in (FocusingParam as FocusParameter).LightParams)
                {
                    this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(light.Type.Value, light.Value.Value);
                }
                if (FocusingParam.FocusRange.Value == 0)
                {
                    FocusingParam.SetDefaultParam();
                }

                retval = FocusingModule.Focusing_Retry(FocusingParam, true, true, false, this);

                this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurImage(out imageBuffer);

                if (retval != EventCodeEnum.NONE)//포커싱 실패
                {
                    wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                    pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                    LoggerManager.Debug($"[SoakingModule] GetSafePosition(), Focusing fail, return value : {retval}");
                    SaveFocusingImage(imageBuffer, "fail");
                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    return retval;
                }
                else //포커싱 성공
                {
                    //이후//z축의 (머신좌표,엔코더)위치를 가져온 후 wafer좌표계로 변환한 것이 측정된 두께다
                    WaferCoordinate wfcoord = new WaferCoordinate();

                    wfcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();//현위치
                    wafercoord.Z.Value = wfcoord.Z.Value;
                    double after_Thickness = wfcoord.Z.Value;
                    double heightDiff = Math.Abs(after_Thickness - before_Thickness);

                    double Thickness_Tolerance = SoakingDeviceFile_Clone.AutoSoakingParam.Thickness_Tolerance.Value;
                    double ZClearance = SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value;

                    if (useStatusSoaking)
                    {
                        if (false == StatusSoakingDeviceFileObj.Get_NotExistWaferObj_OD(SoakingState.GetState(), ref ZClearance))
                        {
                            LoggerManager.SoakingErrLog($"Failed to 'Get_NotExistWaferObj_OD'");

                            return EventCodeEnum.PARAM_ERROR;
                        }

                        Thickness_Tolerance = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.ThicknessTolerance.Value;
                    }

                    bool verificationZClearance = false;

                    //status soaking에서는 heightDiff(실측정과 입력된 정보값의 차이)값 보다 큰 경우에 대한 verification을 하지 않음(Contact soaking등이 진행될 수 있음.)
                    if (useStatusSoaking)
                    {
                        verificationZClearance = true;
                    }
                    else
                    {
                        if (heightDiff < Math.Abs(ZClearance))
                        {
                            verificationZClearance = true;
                        }
                    }

                    //device file과 방금 측정된 높이 차이가 사용자가 지정한 허용오차보다 작아야한다
                    //device file과 방금 측정된 높이 차이가 클리어런스보다 작아야한다
                    if (heightDiff < Thickness_Tolerance && verificationZClearance)
                    {
                        wafercoord = wfcoord;

                        SaveFocusingImage(imageBuffer, "success");

                        LoggerManager.Debug($"\n[SoakingModule] GetSafePosition(), Focusing Success.\n" +
                                      $"Flatness Threshold: { FocusingParam.FlatnessThreshold.Value:0.00},\n" +
                                      $"Thickness_Tolerance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.Thickness_Tolerance.Value:0.00},\n" +
                                      $"ZClearance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value:0.00},\n" +
                                      $"heightDiff = {heightDiff:0.00}, after thickness : {after_Thickness:0.00}, before Thickness : {before_Thickness:0.00},\n" +
                                      $"Wafer Type : {waferType}");

                        return retval;
                    }
                    else
                    {
                        wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                        pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;

                        LoggerManager.Debug($"The measured thickness does not meet the conditions." +
                            $"Flatness Threshold: { FocusingParam.FlatnessThreshold.Value}," +
                            $"Thickness_Tolerance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.Thickness_Tolerance.Value}, " +
                            $"ZClearance.Value = {SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value}," +
                            $"heightDiff = {heightDiff:0.00}, after thickness : {after_Thickness:0.00}, chuck thickness : {before_Thickness:0.00} " +
                            $"Wafer Type : {waferType}");
                        SaveFocusingImage(imageBuffer, "outOfcondition");

                        this.StageSupervisor().StageModuleState.ZCLEARED();

                        if (useStatusSoaking)
                        {
                            retval = EventCodeEnum.SOAKING_TOLERANCE_ERROR;
                        }
                        return retval;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            (FocusingParam as FocusParameter).LightParams = null;

            return retval;
        }

        public EventCodeEnum SaveFocusingImage(ImageBuffer focusingImage, string focusing_result)
        {
            try
            {
                if (focusingImage == null)
                {
                    LoggerManager.Error($"SaveFocusingIamge(): Have no image.");
                    return EventCodeEnum.FOCUS_FAILED;//////////////////////나중에 고치기
                }
                else
                {
                    if (focusing_result == "success")
                    {
                        string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.SOAKING, IMAGE_SAVE_TYPE.BMP, true, "\\Focusing_Image\\SuccessImage\\success_img");

                        this.VisionManager().SaveImageBuffer(focusingImage, path, IMAGE_LOG_TYPE.PASS, EventCodeEnum.NONE);

                        LoggerManager.Debug($"Saved focusing Image Path: " + path);
                    }
                    else if (focusing_result == "outOfcondition")
                    {
                        string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.SOAKING, IMAGE_SAVE_TYPE.BMP, true, "\\Focusing_Image\\SuccessImage\\OutOfCondision\\fail_img");

                        this.VisionManager().SaveImageBuffer(focusingImage, path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                        LoggerManager.Debug($"focusing out of condition. Saved Image Path: " + path);
                    }
                    else if (focusing_result == "fail")
                    {
                        string path = this.FileManager().GetImageSaveFullPath(EnumProberModule.SOAKING, IMAGE_SAVE_TYPE.BMP, true, "\\Focusing_Image\\FailImage\\fail_img");

                        this.VisionManager().SaveImageBuffer(focusingImage, path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                        LoggerManager.Debug($"focusing fail. Saved Image Path: " + path);
                    }
                    else
                    {

                    }

                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SaveFocusingIamge(): Error occurred. Err = {err.Message}");
            }
            return EventCodeEnum.NONE;
        }
        public MachineCoordinate GetTargetMachinePosition(WaferCoordinate wafercoord, PinCoordinate pincoord, double zclearance)
        {
            MachineCoordinate retval = new MachineCoordinate();

            try
            {
                retval = this.CoordinateManager().WaferHighChuckConvert.GetWaferPinAlignedPosition(wafercoord, pincoord);
                retval.Z.Value += (Math.Abs(zclearance) * -1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum CheckEventSoakingTrigger()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                // (1) .CHUCKAWAY_SOAK

                ChuckAwayEventSoaking chuckawayeventsoakingobj = GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK) as ChuckAwayEventSoaking;

                if (((chuckawayeventsoakingobj != null) && (chuckawayeventsoakingobj.Enable.Value)))
                {
                    WaferCoordinate wafercoord = new WaferCoordinate();
                    PinCoordinate pincoord = new PinCoordinate();
                    MachineCoordinate targetpos = new MachineCoordinate();

                    retval = GetTargetPosAccordingToCondition(ref wafercoord, ref pincoord);
                    targetpos = GetTargetMachinePosition(wafercoord, pincoord, chuckawayeventsoakingobj.ZClearance.Value);


                    // Compare tolerance
                    double posX = 0;
                    double posY = 0;
                    double posZ = 0;
                    //double posT = 0;
                    //this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                    //this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                    //this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                    this.MotionManager().GetRefPos(EnumAxisConstants.X, ref posX);
                    this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref posY);
                    this.MotionManager().GetRefPos(EnumAxisConstants.Z, ref posZ);

                    //this.MotionManager().GetActualPos(EnumAxisConstants.C, ref posT);
                    double ChuckAwayTolX = 0;
                    double ChuckAwayTolY = 0;
                    double ChuckAwayTolZ = 0;

                    ChuckAwayTolX = chuckawayeventsoakingobj.ChuckAwayToleranceX.Value;
                    ChuckAwayTolY = chuckawayeventsoakingobj.ChuckAwayToleranceY.Value;
                    ChuckAwayTolZ = chuckawayeventsoakingobj.ChuckAwayToleranceZ.Value;

                    var DiffX = posX - targetpos.X.Value;
                    var DiffY = posY - targetpos.Y.Value;
                    var DiffZ = posZ - targetpos.Z.Value;

                    bool ChuckInRangeZ = true;
                    //if (DiffZ > 0)
                    //{
                    //    ChuckInRangeZ = DiffZ - Math.Abs(ChuckAwayTolZ) > 0;
                    //}

                    if (Math.Abs(DiffZ) > ChuckAwayTolZ & DiffZ < 0)
                    {
                        ChuckInRangeZ = false;
                    }
                    else
                    {
                        ChuckInRangeZ = true;
                    }

                    bool isChuckOutOfRange = false;
                    if ((Math.Abs(DiffX) > ChuckAwayTolX) ||
                        (Math.Abs(DiffY) > ChuckAwayTolY) ||
                        (ChuckInRangeZ == false))
                    {
                        ///LastChuckAwayTime : 

                        if (ChuckInRangeTime == null)
                        {
                            ChuckInRangeTime = new DateTime();
                        }
                        if (ChuckInRangeTime < this.LotOPModule().LotInfo.LotStartTime)
                        {
                            ChuckInRangeTime = this.LotOPModule().LotInfo.LotStartTime;
                        }
                        //if (ChuckInRangeTime == new DateTime()
                        //    | ChuckInRangeTime == this.LotOPModule().LotInfo.LotStartTime)
                        //{
                        //    ChuckInRangeTime = DateTime.Now;
                        //}
                        if (false == LogWrite_CheckEventSoakingTrigger_ChuckAway)
                        {
                            LoggerManager.Debug($"[Soaking] Event Soaking OutTolerance - TargetPos X:{targetpos.GetX():0.00}, Y:{targetpos.GetY():0.00}, Z:{targetpos.GetZ():0.00}");
                            LoggerManager.Debug($"[Soaking] Event Soaking OutTolerance - ActualPos X:{posX:0.00}, Y:{posY:0.00}, Z:{posZ:0.00}");
                            LogWrite_CheckEventSoakingTrigger_ChuckAway = true;
                        }

                        isChuckOutOfRange = true;
                    }
                    else
                    {
                        // 범위 이내로 다시 들어온 경우 시간 초기화 해주기 위함.
                        ChuckInRangeTime = DateTime.Now;

                        if (LogWrite_CheckEventSoakingTrigger_ChuckAway)
                        {
                            LogWrite_CheckEventSoakingTrigger_ChuckAway = false;
                            LoggerManager.Debug($"[Soaking] Cleart 'Event Soaking OutTolerance flag'");
                        }
                    }


                    bool isDurationExceeded = false;

                    if (isChuckOutOfRange && (ChuckInRangeTime != new DateTime()))
                    {
                        //DateTime LastChuckAwayTime = (DateTime)ChuckInRangeTime;

                        //long elapsedTicks = DateTime.Now.Ticks - LastChuckAwayTime.Ticks;

                        long elapsedTicks = DateTime.Now.Ticks - ((DateTime)ChuckInRangeTime).Ticks;

                        TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

                        // Unit : Sec.

                        if (elapsedSpan.TotalSeconds >= chuckawayeventsoakingobj.ChuckAwayElapsedTime.Value)
                        {
                            LoggerManager.Debug($"[SoakingModule] CheckEventSoakingTrigger() : ChuckInRangeTime( {ChuckInRangeTime} ),");
                            isDurationExceeded = true;
                            ChuckInRangeTime = new DateTime();
                            //this.SoakingDeviceFile_Clone.ChuckInRangeTime = null;
                        }

                    }

                    // 일정 시간(Parameter) 동안 척으로부터 떨어져 있었을 때

                    if (isDurationExceeded)
                    {
                        //chuckawayeventsoakingobj.Triggered = true;
                        ChuckAwayTriggered = true;
                        LoggerManager.Debug($"Chuck away soak trigger on");
                    }
                }

                // (2) EnumSoakingType.CHUCKAWAY_SOAK

                // 마지막 Soaking 동작 했을 당시의 SET TEMP와, 현재 SET TEMP를 비교하여
                // Current Set temp - Last Set temp >= Parameter -> Triggered


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void ChuckInRangeTimeFunc()
        {
            //try
            //{
            //    ChuckAwayEventSoaking chuckawayeventsoakingobj = GetEventSoakingObject(EnumSoakingType.CHUCKAWAY_SOAK) as ChuckAwayEventSoaking;

            //    if (((chuckawayeventsoakingobj != null) && (chuckawayeventsoakingobj.Enable.Value)))
            //    {
            //        WaferCoordinate wafercoord = new WaferCoordinate();
            //        PinCoordinate pincoord = new PinCoordinate();
            //        MachineCoordinate targetpos = new MachineCoordinate();

            //        GetTargetPosAccordingToCondition(ref wafercoord, ref pincoord);
            //        targetpos = GetTargetMachinePosition(wafercoord, pincoord, chuckawayeventsoakingobj.ZClearance.Value);


            //        // Compare tolerance
            //        double posX = 0;
            //        double posY = 0;
            //        double posZ = 0;
            //        //double posT = 0;
            //        this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
            //        this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
            //        this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
            //        //this.MotionManager().GetActualPos(EnumAxisConstants.C, ref posT);
            //        double ChuckAwayTolX = 0;
            //        double ChuckAwayTolY = 0;
            //        double ChuckAwayTolZ = 0;

            //        ChuckAwayTolX = chuckawayeventsoakingobj.ChuckAwayToleranceX.Value;
            //        ChuckAwayTolY = chuckawayeventsoakingobj.ChuckAwayToleranceY.Value;
            //        ChuckAwayTolZ = chuckawayeventsoakingobj.ChuckAwayToleranceZ.Value;

            //        var DiffX = posX - targetpos.X.Value;
            //        var DiffY = posY - targetpos.Y.Value;
            //        var DiffZ = posZ - targetpos.Z.Value;

            //        bool ChuckInRangeZ = true;
            //        if (DiffZ > 0)
            //        {
            //            ChuckInRangeZ = DiffZ - Math.Abs(ChuckAwayTolZ) > 0;
            //        }

            //        if ((Math.Abs(DiffX) > ChuckAwayTolX) ||
            //            (Math.Abs(DiffY) > ChuckAwayTolY) ||
            //            (ChuckInRangeZ == false))
            //        {
            //            // Out Range
            //        }
            //        else
            //        {
            //            // 범위 이내로 다시 들어온 경우 시간 초기화 해주기 위함.
            //            ChuckInRangeTime = DateTime.Now;
            //        }
            //    }
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}

            ChuckInRangeTime = DateTime.Now;
        }

        public EventCodeEnum OffSoakingTrigger(EnumSoakingType soaktype)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

                switch (soaktype)
                {
                    case EnumSoakingType.UNDEFINED:
                        break;
                    case EnumSoakingType.LOTRESUME_SOAK:
                        LotResumeTriggered = false;
                        break;
                    case EnumSoakingType.CHUCKAWAY_SOAK:
                        ChuckAwayTriggered = false;
                        break;
                    case EnumSoakingType.TEMPDIFF_SOAK:
                        TempDiffTriggered = false;
                        break;
                    case EnumSoakingType.PROBECARDCHANGE_SOAK:
                        ProbeCardChangeTriggered = false;
                        break;
                    case EnumSoakingType.LOTSTART_SOAK:
                        LotStartTriggered = false;
                        break;
                    case EnumSoakingType.DEVICECHANGE_SOAK:
                        DeviceChangeTriggered = false;
                        DeviceChangeFlag = false;
                        break;
                    case EnumSoakingType.EVERYWAFER_SOAK:
                        EveryWaferSoakTriggered = false;
                        break;
                    case EnumSoakingType.AUTO_SOAK:
                        AutoSoakTriggered = false;
                        break;
                    default:
                        break;
                }
                LoggerManager.Debug($"Soak trigger off soak type:{soaktype} ");
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum OffAllSoakingTrigger()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.LOTRESUME_SOAK) == null)
                {
                    LotResumeTriggered = false;
                    LoggerManager.Debug($"resume soak trigger off ");
                }
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.CHUCKAWAY_SOAK) == null)
                {
                    ChuckAwayTriggered = false;
                    LoggerManager.Debug($"chuck away soak trigger off ");
                }
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.TEMPDIFF_SOAK) == null)
                {
                    TempDiffTriggered = false;
                    LoggerManager.Debug($"temp diff soak trigger off ");
                }
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.PROBECARDCHANGE_SOAK) == null)
                {
                    ProbeCardChangeTriggered = false;
                    LoggerManager.Debug($"probe card change soak trigger off ");
                }
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.LOTSTART_SOAK) == null)
                {
                    LotStartTriggered = false;
                    LoggerManager.Debug($"lot start soak trigger off ");
                }
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.DEVICECHANGE_SOAK) == null)
                {
                    DeviceChangeTriggered = false;
                    LoggerManager.Debug($"device change soak trigger off ");
                }
                if (SoakPriorityList.SingleOrDefault(soaklist => soaklist.EventSoakingType.Value == EnumSoakingType.AUTO_SOAK) == null)
                {
                    AutoSoakTriggered = false;
                    LoggerManager.Debug($"auto soak trigger off ");
                }
                //LotResumeTriggered = false;
                //ChuckAwayTriggered = false;
                //TempDiffTriggered = false;
                //ProbeCardChangeTriggered = false;
                //LotStartTriggered = false;
                //DeviceChangeTriggered = false;
                //AutoSoakTriggered = false;
                LoggerManager.Debug($"Soak trigger off of all ");

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum CheckAutoSoakingTrigger()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE ||
                    this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE ||
                    this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.SOAKING)
                {
                    AutoSoakLastChuckAwayTime = null;
                    //SoakingDeviceFile_Clone.AutoSoakingParam.Triggered = false;
                    AutoSoakTriggered = false;
                }
                else
                {
                    if (((SoakingDeviceFile_Clone.AutoSoakingParam != null) && (SoakingDeviceFile_Clone.AutoSoakingParam.Enable.Value)))
                    {
                        WaferCoordinate wafercoord = new WaferCoordinate();
                        PinCoordinate pincoord = new PinCoordinate();
                        MachineCoordinate targetpos = new MachineCoordinate();

                        retval = GetTargetPosAccordingToCondition(ref wafercoord, ref pincoord);
                        targetpos = GetTargetMachinePosition(wafercoord, pincoord, SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value);

                        // Compare tolerance
                        double posX = 0;
                        double posY = 0;
                        double posZ = 0;
                        //double posT = 0;
                        this.MotionManager().GetActualPos(EnumAxisConstants.X, ref posX);
                        this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref posY);
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                        //this.MotionManager().GetActualPos(EnumAxisConstants.C, ref posT);

                        double ChuckAwayTolX = 0;
                        double ChuckAwayTolY = 0;
                        double ChuckAwayTolZ = 0;

                        ChuckAwayTolX = SoakingDeviceFile_Clone.AutoSoakingParam.ChuckAwayToleranceX.Value;
                        ChuckAwayTolY = SoakingDeviceFile_Clone.AutoSoakingParam.ChuckAwayToleranceY.Value;
                        ChuckAwayTolZ = SoakingDeviceFile_Clone.AutoSoakingParam.ChuckAwayToleranceZ.Value;

                        var DiffX = posX - targetpos.X.Value;
                        var DiffY = posY - targetpos.Y.Value;
                        var DiffZ = posZ - targetpos.Z.Value;

                        if ((Math.Abs(DiffX) > ChuckAwayTolX) ||
                            (Math.Abs(DiffY) > ChuckAwayTolY) ||
                            (Math.Abs(DiffZ) > ChuckAwayTolZ))
                        {
                            if (AutoSoakLastChuckAwayTime == null)
                            {
                                AutoSoakLastChuckAwayTime = DateTime.Now;
                            }
                        }

                        bool isDurationExceeded = false;

                        if (AutoSoakLastChuckAwayTime != null)
                        {
                            DateTime LastChuckAwayTime = (DateTime)AutoSoakLastChuckAwayTime;

                            long elapsedTicks = DateTime.Now.Ticks - LastChuckAwayTime.Ticks;
                            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

                            // Unit : Sec.
                            if (elapsedSpan.TotalSeconds >= SoakingDeviceFile_Clone.AutoSoakingParam.ChuckAwayElapsedTime.Value)
                            {
                                wafercoord.Z.Value = this.StageSupervisor().WaferMaxThickness;
                                pincoord.Z.Value = this.StageSupervisor().PinMinRegRange;
                                var safeTargetpos = GetTargetMachinePosition(wafercoord, pincoord, SoakingDeviceFile_Clone.AutoSoakingParam.ZClearance.Value);
                                var SafePos_ZDiff = posZ - safeTargetpos.Z.Value;

                                if (Math.Abs(SafePos_ZDiff) < ChuckAwayTolZ)
                                {
                                    // 이미 Safe position에 가 있을 경우 이전 소킹에서 실패한 것으로 간주하고 더이상 오토 소킹을 진행 하지 않는다.
                                    // 이때, X, Y가 벗어 난다면 이 상태도 풀리게 된다.
                                    if ((Math.Abs(DiffX) > ChuckAwayTolX) || (Math.Abs(DiffY) > ChuckAwayTolY))
                                    {
                                        isDurationExceeded = true;
                                        AutoSoakLastChuckAwayTime = null;
                                    }
                                }
                                else
                                {
                                    isDurationExceeded = true;
                                    AutoSoakLastChuckAwayTime = null;
                                }
                            }
                        }

                        // 일정 시간(Parameter) 동안 척으로부터 떨어져 있었을 때
                        if (isDurationExceeded & this.SequenceEngineManager().GetMovingState() == true)
                        {
                            if (AutoSoakTriggered == false)
                            {
                                LoggerManager.Debug($"CheckAutoSoakingTrigger(): Current Position = ({posX:0.00}, {posY:0.00}, {posZ:0.00})");
                            }

                            AutoSoakTriggered = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public bool IsEnableEventSokaing(EnumSoakingType type)
        {
            bool retval = false;

            try
            {
                var param = this.SoakingDeviceFile_Clone.EventSoakingParams.Where(x => x.EventSoakingType.Value == type).FirstOrDefault();

                if (param != null)
                {
                    retval = param.Enable.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public SoakingParamBase GetEventSoakingObject(EnumSoakingType type)
        {
            SoakingParamBase retval = null;

            try
            {
                retval = this.SoakingDeviceFile_Clone.EventSoakingParams.Where(x => x.EventSoakingType.Value == type).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum UpdateEventSoakingPriority()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            SoakPriorityList.Clear();
            try
            {


                var orderlist = SoakingDeviceFile_Clone.EventSoakingParams.OrderBy(item => item.SoakingPriority.Value);

                foreach (var item in orderlist)
                {
                    if (item.Enable.Value)
                    {
                        switch (item.EventSoakingType.Value)
                        {
                            case EnumSoakingType.LOTRESUME_SOAK:
                                if (LotResumeTriggered)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.CHUCKAWAY_SOAK:
                                if (ChuckAwayTriggered)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.TEMPDIFF_SOAK:
                                if (TempDiffTriggered)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.PROBECARDCHANGE_SOAK:
                                if (ProbeCardChangeTriggered)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.LOTSTART_SOAK:
                                if (LotStartTriggered)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.DEVICECHANGE_SOAK:
                                if (DeviceChangeTriggered && DeviceChangeFlag)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.EVERYWAFER_SOAK:
                                if (EveryWaferSoakTriggered)
                                    SoakPriorityList.Enqueue(item);
                                break;
                            case EnumSoakingType.AUTO_SOAK:
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (SoakPriorityList.Count > 0)
                {
                    foreach (var soakitem in SoakPriorityList)
                    {
                        LoggerManager.Debug($"Cur soaking List item: {soakitem.EventSoakingType.Value}");
                    }
                }

                ret = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "Error occurred UpdateEventSoakingPriority Function.");
                LoggerManager.Exception(err);

                throw new Exception(string.Format("Class: {0} Function: {1} ReturnValue: {2} HashCode: {3} ExceptionMessage: {4} ", this, MethodBase.GetCurrentMethod(), ret.ToString(), err.GetHashCode(), err.Message));
            }

            return ret;
        }
        public EventCodeEnum LoadSoakingSubscribeRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                SubscribeRecipeParam = new EventProcessList();
                string FullPath;

                FullPath = this.FileManager().GetSystemParamFullPath("Event", "Recipe_Subscribe_Sokaing.json");
                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                    }

                    if (File.Exists(FullPath) == false)
                    {
                        SubscribeRecipeParam.Add(new PreHeatSokaingProcess() { EventFullName = "NotifyEventModule.DoPreHeatSoaking" });

                        RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

                        if (RetVal == EventCodeEnum.PARAM_ERROR)
                        {
                            LoggerManager.Error($"[SoakingModule] LoadSysParam(): Serialize Error");
                            return RetVal;
                        }
                    }

                    IParam tmpPram = null;
                    RetVal = this.LoadParameter(ref tmpPram, typeof(EventProcessList), null, FullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        SubscribeRecipeParam = tmpPram as EventProcessList;
                    }
                    else
                    {
                        RetVal = EventCodeEnum.PARAM_ERROR;

                        LoggerManager.Error($"[SoakingModule] LoadSysParam(): DeSerialize Error");
                        return RetVal;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("[SoakingModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum RegistEventSubscribe()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = LoadSoakingSubscribeRecipe();

                foreach (var evtname in SubscribeRecipeParam)
                {
                    evtname.OwnerModuleName = "SOAKING";
                    ret = this.EventManager().RegisterEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[SoakingModule] Regist EventSubscribe Error...");

                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                SoakingStateEnum state = (InnerState as SoakingState).GetState();
                retval = ConvertEnumToString(state);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string ConvertEnumToString(SoakingStateEnum state)
        {
            string ret = "";
            switch (state)
            {
                case SoakingStateEnum.PREPARE:
                    {
                        ret = "SOAKING(PRE HEATING)";
                    }
                    break;
                case SoakingStateEnum.RECOVERY:
                    {
                        ret = "SOAKING(RECOVERY)";
                    }
                    break;
                case SoakingStateEnum.MAINTAIN:
                    {
                        ret = "SOAKING(MAINTAIN)";
                    }
                    break;
                case SoakingStateEnum.MANUAL:
                    {
                        ret = "SOAKING(MANUAL)";
                    }
                    break;
                case SoakingStateEnum.STATUS_EVENT_SOAK:
                    {
                        ret = "SOAKING(EVENT)";
                    }
                    break;
                default:
                    ret = state.ToString();
                    break;
            }

            return ret;
        }

        public bool IsBusy()
        {
            bool retVal = true;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

                PreInnerState = _SoakingState;
                InnerState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }
        
        public void SetCancleFlag(bool value, int chuckindex)
        {
            try
            {
                int chuckidx = this.LoaderController().GetChuckIndex();
                if (chuckidx == chuckindex)
                {
                    this.SoakingCancelFlag = value;
                    if (this.StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue())
                    {
                        StatusSoakingStateBase SubStateCheck = SoakingState as StatusSoakingStateBase;
                        if (SubStateCheck != null)
                        {
                            SubStateCheck.SubState.StatusSoakingInfoUpdateToLoader(0, false, true, true);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetSoakingTitle()
        {
            return this.SoakingTitle;
        }

        public string GetSoakingMessage()
        {
            return SoakingMessage;
        }
        public bool IsServiceAvailable()
        {
            return true;
        }
        public int GetSoakQueueCount()
        {
            int cnt = 0;
            try
            {
                if (SoakPriorityList != null)
                {
                    cnt = SoakPriorityList.Count;
                }
                else
                {
                    cnt = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return cnt;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new SoakingSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(SoakingSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    SoakingSysParam_IParam = tmpParam;
                    SoakingSysParam_Clone = SoakingSysParam_IParam as SoakingSysParameter;
                }
                else
                {
                    LoggerManager.Event($"Parameter load failed", "", EventlogType.EVENT);
                }

                FocusingModule = WaferHighFocusModel;
                FocusingParam = (SoakingSysParam_IParam as SoakingSysParameter).FocusParam;
                //DevParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                string fullPath = this.FileManager().GetSystemParamFullPath(SoakingSysParam_Clone.FilePath, SoakingSysParam_Clone.FileName);

                try
                {
                    if (SoakingSysParam_Clone.FocusParam.FocusRange.Value == 0)
                    {
                        SoakingSysParam_Clone.FocusParam.FocusMaxStep.Value = 20;
                        SoakingSysParam_Clone.FocusParam.FocusRange.Value = 200;
                        SoakingSysParam_Clone.FocusParam.DepthOfField.Value = 1;
                        SoakingSysParam_Clone.FocusParam.FocusThreshold.Value = 10000;
                        SoakingSysParam_Clone.FocusParam.FlatnessThreshold.Value = 20;
                        SoakingSysParam_Clone.FocusParam.PeakRangeThreshold.Value = 40;
                        SoakingSysParam_Clone.FocusParam.PotentialThreshold.Value = 20;
                        SoakingSysParam_Clone.FocusParam.CheckPotential.Value = true;
                        SoakingSysParam_Clone.FocusParam.CheckThresholdFocusValue.Value = true;
                        SoakingSysParam_Clone.FocusParam.CheckFlatness.Value = true;
                        SoakingSysParam_Clone.FocusParam.CheckDualPeak.Value = true;
                        SoakingSysParam_Clone.FocusParam.FocusingROI.Value = new Rect(0, 0, 960, 960);
                        SoakingSysParam_Clone.FocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                        SoakingSysParam_Clone.FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;
                        SoakingSysParam_Clone.FocusParam.OutFocusLimit.Value = 40;
                    }
                    RetVal = Extensions_IParam.SaveParameter(null, SoakingSysParam_Clone, null, fullPath);
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        private EventCodeEnum WaitforSoak(int soaktime)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Stopwatch stw = new Stopwatch();
                stw.Start();
                bool runflag = true;
                while (runflag)
                {
                    if (MeasurementSoakingCancelFlag == true)
                    {
                        return ret = EventCodeEnum.NONE;
                    }

                    if (stw.Elapsed.TotalSeconds >= soaktime)
                    {
                        ret = EventCodeEnum.NONE;
                        runflag = false;
                        stw.Stop();
                    }
                    if (stw.Elapsed.TotalSeconds >= 36000)
                    {
                        ret = EventCodeEnum.NONE;
                        runflag = false;
                        stw.Stop();
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public EventCodeEnum SetChangedDeviceName(string curdevname, string changedevname)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                CurDeviceName = curdevname;
                ChangedDeviceName = changedevname;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void SetChuckInRangeValue(DateTime time)
        {
            try
            {
                ChuckInRangeTime = time;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetLotResumeTriggeredFlag()
        {
            return LotResumeTriggered;
        }
        public void ClearSoakPriorityList()
        {
            try
            {
                if (SoakPriorityList != null)
                {
                    SoakPriorityList.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSoakingTime(EnumSoakingType soakingType, int timesec)
        {
            try
            {
                var soakingObj = GetEventSoakingObject(soakingType);
                if (soakingObj != null)
                {
                    soakingObj.SoakingTimeInSeconds.Value = timesec;
                    LoggerManager.Debug($"{soakingType} Change to soaking time : {timesec}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StateTransitionToErrorState(EventCodeEnum eventCodeEnum, EventCodeInfo eventCodeInfo = null)
        {
            try
            {
                InnerStateTransition(new SoakingErrorState(this, eventCodeInfo));

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    int index = this.LoaderController().GetChuckIndex();

                    if (eventCodeInfo == null)
                    {
                        this.LoaderController().SetTitleMessage(index, "SOAKING ERROR");
                    }
                    else
                    {
                        this.LoaderController().SetTitleMessage(index, eventCodeInfo.EventCode.ToString());
                    }
                }

                this.MetroDialogManager().ShowMessageDialog("Error Message", $"Soaking error occurred, Switch to the maintenance mode to clear error.", EnumMessageStyle.Affirmative);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //Prepare Soaking State로 변경할 때 봐야할 이벤트를 여기서 체크
        public bool ToCheckEventsForPrepareState()
        {
            bool ret = false;
            try
            {
                if (TempDiffTriggered || ProbeCardChangeTriggered)
                {
                    LoggerManager.SoakingLog($"Prepare event occure.(TempDiffTriggered:{TempDiffTriggered.ToString()},  ProbeCardChangeTriggered:{ProbeCardChangeTriggered})");
                    ret = true;
                }
                else if (IsUsePolishWafer() && Get_PrepareStatusSoak_after_DeviceChange() && DeviceChangeTriggered)
                {
                    //Card Change Event와 같이 Device Change 이후 Preheting을 할 것인가?에 대한 조건
                    LoggerManager.SoakingLog($"Prepare event occure.(DeviceChangeTriggered:{DeviceChangeTriggered}, Get_PrepareStatusSoak_after_DeviceChange() RetVal : {Get_PrepareStatusSoak_after_DeviceChange()})");
                    ret = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        bool overlapthreeLegLog = false;
        public EventCodeEnum CheckCardModuleAndThreeLeg()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
                {
                    bool isthreelegup = false;
                    bool isthreelegdown = false;

                    // TODO : ThreeLeg Check 문제 찾고 고쳐야 함.
                    ret = this.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);
                    ret = this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdown);

                    if ((isthreelegup == false && isthreelegdown == true)|| this.StageSupervisor().CheckUsingHandler())
                    {
                        ret = EventCodeEnum.NONE;
                        overlapthreeLegLog = false;
                    }
                    else
                    {
                        if (overlapthreeLegLog == false)
                        {
                            LoggerManager.Error($"[GP CC]=> ThreelegDown:{isthreelegdown} ThreelegUp:{isthreelegup}");
                            overlapthreeLegLog = true;
                        }
                        ret = EventCodeEnum.STAGEMOVE_THREE_LEG_DOWN_ERROR;
                        return ret;
                    }

                    bool diupmodule_left_sensor;
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);

                    bool diupmodule_right_sensor;
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);

                    bool diupmodule_cardexist_sensor;
                    if (this.CardChangeModule().GetCCType() == ProberInterfaces.CardChange.EnumCardChangeType.CARRIER)
                    {
                        bool touchR, touchL;
                        ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, out touchR);
                        ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, out touchL);
                        diupmodule_cardexist_sensor = touchL | touchR;
                    }
                    //else
                    //{
                    //    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);
                    //}
                    if (diupmodule_left_sensor || diupmodule_right_sensor)
                    {
                        ret = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                        //LoggerManager.Error($"[GP CC]=> Upmodule_Left:{diupmodule_left_sensor} Upmodule_Right:{diupmodule_right_sensor} CardExist:{diupmodule_cardexist_sensor}");
                        return ret;
                    }

                    //if (diupmodule_cardexist_sensor)
                    //{
                    //    ret = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                    //    //LoggerManager.Error($"[GP CC]=> Upmodule_Left:{diupmodule_left_sensor} Upmodule_Right:{diupmodule_right_sensor} CardExist:{diupmodule_cardexist_sensor}");
                    //    return ret;
                    //}

                    //PogoCard Vac이 안잡혀 있는 상태랑 Latch가 풀려있는 상태도 어차피 Soaking은 해야 되는 건데 굳이 이 아래 코드가 있어야 하는지....
                    bool dipogocard_vacu_sensor;
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                    if (dipogocard_vacu_sensor != true)
                    {
                        //ret = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                        ret = EventCodeEnum.NONE;//Card가 Doaking되어있지 않아도 Auto Soaking은 돌아야 해서.
                        //LoggerManager.Error($"[GP CC]=> PogoCardVac:{dipogocard_vacu_sensor}");
                        return ret;
                    }
                    bool ditplate_pclatch_sensor_lock;
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                    bool ditplate_pclatch_sensor_unlock;
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                    if (ditplate_pclatch_sensor_lock == false || ditplate_pclatch_sensor_unlock == true)
                    {
                        //ret = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                        ret = EventCodeEnum.NONE;//Card가 Doaking되어있지 않아도 Auto Soaking은 돌아야 해서.
                        LoggerManager.Error($"[GP CC]=> Latch_Lock:{ditplate_pclatch_sensor_lock} Latch_Unlock:{ditplate_pclatch_sensor_unlock}");
                        return ret;
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

        public ObservableCollection<string> GetPolishWaferNameListForSoaking()
        {
            ObservableCollection<string> selectedPolishWaferNameList = new ObservableCollection<string>();
            SoakingStateEnum soakingState = (InnerState as SoakingState).GetState();
            StatusSoakingParamIF.Get_SelectedPolishWaferName(soakingState, ref selectedPolishWaferNameList);

            return selectedPolishWaferNameList;
        }

        public bool EnalbeStatusSoakingFunc { get; set; } = false;
        private StatusSoakingCurrentSetData _StatusSoakingTempInfo = new StatusSoakingCurrentSetData();
        public StatusSoakingCurrentSetData StatusSoakingTempInfo
        {
            get
            {
                return _StatusSoakingTempInfo;
            }
        }

        /// <summary>
        /// Soaking이 maintain으로 soaking이 완료된 상태인지
        /// </summary>
        /// <returns>true: 완료, flase: 그렇지 않음</returns>
        public bool IsStatusSoakingOk()
        {
            if (this.ForcedDone == EnumModuleForcedState.ForcedDone)
            {
                return true;
            }

            try
            {
                bool EnableStatusSoaking = false;
                bool ShowToggleFlag = false;
                ShowToggleFlag = StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue();
                bool IsGettingOptionSuccessul = StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);
                if (false == IsGettingOptionSuccessul)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                }

                if (ShowToggleFlag && EnableStatusSoaking)
                {
                    StatusSoakingStateBase SubStateCheck = SoakingState as StatusSoakingStateBase;
                    long DummyValue_AccumulatedChillingTm = 0;
                    int DummyValue_SoakingTime = 0;
                    bool NeedToRecovery = false;
                    ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref DummyValue_AccumulatedChillingTm, ref DummyValue_SoakingTime, ref NeedToRecovery);
                    if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN && false == NeedToRecovery) //Status는 maintain이지만 chillingTime이 이미 recovery가 될 정도로 쌓여있을 수 있음.(ex: maintain에서 pause상태)
                    {
                        if (null != SubStateCheck && (SubStateCheck.SubState.GetState() == SoakingStateEnum.DONE || SubStateCheck.SubState.GetState() == SoakingStateEnum.IDLE))
                        {
                            //event soaking trigger된것도 없어야함.
                            if (TriggeredStatusEventSoakList.Count() == 0)
                                return true;
                            else
                            {
                                if (ChillingTimeMngObj.IsShowDebugString())
                                {
                                    Trace.WriteLine($"[ShowDebugStr] IsStatusSoakingOk >> TriggeredStatusEventSoakList Count:{TriggeredStatusEventSoakList.Count().ToString()}");
                                    foreach (var evtItem in TriggeredStatusEventSoakList)
                                        Trace.WriteLine($"[ShowDebugStr] IsStatusSoakingOk >> TriggeredStatusEventSoakList:{evtItem.Key.ToString()}");
                                }
                            }
                        }
                        else
                        {
                            if (ChillingTimeMngObj.IsShowDebugString())
                            {
                                Trace.WriteLine($"[ShowDebugStr] IsStatusSoakingOk >> SubStateCheck:{SubStateCheck.ToString()}");
                                if (null != SubStateCheck)
                                {
                                    Trace.WriteLine($"[ShowDebugStr] IsStatusSoakingOk >> SubStateCheck.SubState:{SubStateCheck.SubState.ToString()}");
                                    if (null != SubStateCheck.SubState)
                                    {
                                        Trace.WriteLine($"[ShowDebugStr] IsStatusSoakingOk >> SubStateCheck.SubState.GetState:{SubStateCheck.SubState.GetState().ToString()}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ChillingTimeMngObj.IsShowDebugString())
                        {
                            Trace.WriteLine($"[ShowDebugStr] IsStatusSoakingOk >> SoakingState.GetState:{SoakingState.GetState().ToString()}, NeedToRecovery:{NeedToRecovery.ToString()}, DummyValue_AccumulatedChillingTm:{DummyValue_AccumulatedChillingTm.ToString()},DummyValue_SoakingTime:{DummyValue_SoakingTime.ToString()}");
                        }
                    }

                    return false;
                }
                else  //status soaking 미사용 시
                    return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }

        /// <summary>
        /// Soaking관련 문구를 No soak으로 출력되도록 한다. 추가적으로  인자로 들어온 값에 따라 Soaking State를 Idle 상태로 변경한다.
        /// </summary>
        /// <param name="ForceChageSoakingSubIdle"> 강제로 Soaking State를 Idle로 변경할 건지에 대한 여부</param>
        /// 강제로 Idle 변경 시 Soaking Module에서 Idle로 변경하지 않고 해당 함수에서 바로 SoakingSubIdle로 State를 바로 변경한다.
        public void Clear_SoakingInfoTxt(bool ForceChageSoakingSubIdle = false)
        {
            try
            {
                bool EnableStatusSoaking = false;
                bool ShowToggleFlag = false;
                ShowToggleFlag = StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue();
                bool IsGettingOptionSuccessul = StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);
                if (false == IsGettingOptionSuccessul)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                }

                if (ShowToggleFlag && EnableStatusSoaking)
                {
                    StatusSoakingStateBase SubStateCheck = SoakingState as StatusSoakingStateBase;
                    if (null != SubStateCheck && null != SubStateCheck.SubState)
                    {
                        SubStateCheck.SubState.StatusSoakingInfoUpdateToLoader(0, true, true);
                        LoggerManager.SoakingLog($"Clear Soaking information(No Soak)");

                        if (ForceChageSoakingSubIdle)
                        {
                            SubStateCheck.SubState.Pause();
                            var StatusSoakingSubBase = InnerState as StatusSoakingStateBase;
                            if (null != StatusSoakingSubBase)
                            {
                                LoggerManager.SoakingLog("Immediately change to 'SoakingSubIdle'");
                                StatusSoakingSubBase.ForceChagneSoakingSubIdleState();
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
        /// Status Soaking Config parameter를 byte array로 반환(serialize)
        /// </summary>
        /// <returns></returns>
        public byte[] GetStatusSoakingConfigParam()
        {
            try
            {
                if (null != StatusSoakingDeviceFileObj)
                {
                    if (null != StatusSoakingDeviceFileObj.StatusSoakingConfigParameter)
                    {
                        lock (load_statusSoakinfParam_lockObject)
                        {
                            var SerializedData = SerializeManager.ObjectToByte(StatusSoakingDeviceFileObj.StatusSoakingConfigParameter);
                            if (null == SerializedData)
                                LoggerManager.SoakingErrLog($"Failed to serialize about StatusSoakingConfigParam");

                            return SerializedData;
                        }
                    }
                    else
                    {
                        LoggerManager.SoakingErrLog($"Parameter is null(StatusSoakingConfigParameter).");
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Parameter is null(StatusSoakingDeviceFileObj).");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;

        }

        /// <summary>
        /// 인자로 들어온 Status config param data를 ojbect로 deserialize한다.
        /// </summary>
        /// <param name="param"> target data</param>
        /// <param name="save_to_file"> 셋팅된 parameter를 파일로 저장할 것인지 여부</param>
        /// <returns></returns>
        public bool SetStatusSoakingConfigParam(byte[] param, bool save_to_file = true)
        {
            try
            {
                var ConfigParamObj = SerializeManager.ByteToObject(param);
                if (null != ConfigParamObj)
                {
                    lock (load_statusSoakinfParam_lockObject)
                    {
                        var RefreshedSoakingConfigData = ConfigParamObj as StatusSoakingConfig;
                        StatusSoakingDeviceFileObj.StatusSoakingConfigParameter = ConfigParamObj as StatusSoakingConfig;
                        StatusSoakingDeviceFileObj.SetElementMetaData();

                        //StatusSoakingDeviceFileObj 내부 object가 변경됨에 따라 CollectElement를 다시 호출하여 갱신처리 해줌
                        Extensions_IParam.SetBaseGenealogy(StatusSoakingDeviceFileObj, Extensions_IParam.GetOwner(this, null).GetType().Name);
                        ParamType paramType = ParamType.DEV;
                        Extensions_IParam.CollectElement(StatusSoakingDeviceFileObj, paramType);
                    }

                    if (save_to_file)
                    {
                        if (EventCodeEnum.NONE != SaveStatusSoakingDeviceFile())
                        {
                            LoggerManager.SoakingErrLog($"Save Status soaking config parameter.");
                            return false;
                        }
                        else
                            return true;
                    }
                    else
                        return true;
                }
                else
                {
                    LoggerManager.SoakingErrLog($"Failed to 'ByteToObject'.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"SetStatusSoakingConfigParam exception({err?.Message})");
            }

            return false;
        }

        public SoakingStateEnum GetStatusSoakingState()
        {
            return SoakingState.GetState();
        }
        public bool GetShowStatusSoakingSettingPageToggleValue()
        {
            bool retVal = false;
            try
            {
                retVal = StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetShowStatusSoakingSettingPageToggleValue(bool ToggleValue)
        {
            try
            {
                StatusSoakingDeviceFileObj.Set_ShowStatusSoakingSettingPageToggleValue(ToggleValue);

                this.SaveDevParameter();

                //구 event soaking 방식과 신규(Status soaking방식)의 전화 처리
                ChangeSoakingOperationType(ToggleValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 신규 / 구 Soaking의 UI화면 전환 시에 따른 처리로 신규 Soaking 사용시와 그렇지 않을때의 State 변경
        /// </summary>
        /// <param name="UseStatusSoaking"></param>
        private void ChangeSoakingOperationType(bool UseStatusSoaking)
        {
            if (UseStatusSoaking)
            {
                LoggerManager.SoakingLog($"Change Soaking operation type : Status Soaking");
                this.InnerStateTransition(new PrepareSoakingState(this));
                var BeforeState = this.PreInnerState as ISoakingState;
                LoggerManager.SoakingLog($"Transition : {BeforeState?.GetState().ToString()} Status -> Prepare Status");
                SetBeforeStatusSoakingUsingFlag(StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.UseStatusSoaking.Value); //status soaking visibility option 변경 시 마지막 사용옵션 저장 
            }
            else
            {
                LoggerManager.SoakingLog($"Change Soaking operation type : Old Event Soaking");
                this.InnerStateTransition(new SoakingIdleState(this));
            }
        }

        public int GetStatusSoakingTime()
        {
            int soakingTime = -1;
            var soakingState = SoakingState.GetState();

            switch (soakingState)
            {
                case SoakingStateEnum.PREPARE:
                    {
                        this.StatusSoakingParamIF.Get_PrepareStatusSoakingTimeSec(ref soakingTime, this.UseTempDiff_PrepareSoakingTime);
                    }
                    break;
                case SoakingStateEnum.RECOVERY:
                case SoakingStateEnum.MAINTAIN:
                    {
                        long accumulated_chillingTime = 0;//임의의 값
                        int SoakingTimeMil = 0;//임의의 값
                        bool InChillingTimeTable = false;//임의의 값
                        EventCodeEnum ret = this.ChillingTimeMngObj.GetCurrentChilling_N_TimeToSoaking(ref accumulated_chillingTime, ref SoakingTimeMil, ref InChillingTimeTable);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.SoakingLog($"[{this.GetType().Name}] [Error] Failed to get 'GetCurrentChilling_N_TimeToSoaking'");
                        }

                        soakingTime = SoakingTimeMil / 1000; // Sec 단위로 Return
                    }
                    break;
                default:
                    break;
            }

            return soakingTime;
        }

        // manual soaking 시작 처리 함수
        public EventCodeEnum StartManualSoakingProc()
        {
            try
            {
                Idle_SoakingFailed_PinAlign = false;
                Idle_SoakingFailed_WaferAlign = false;
                //현재 soaking 가능여부 체크
                if ((SoakingState.GetModuleState() != ModuleStateEnum.IDLE && SoakingState.GetModuleState() != ModuleStateEnum.DONE))
                {
                    SoakingStateEnum state = (InnerState as SoakingState).GetState();

                    LoggerManager.SoakingErrLog($"Can't start manausl soaking(current soaking state:{state.ToString()})");

                    if (SoakingStateEnum.PREPARE == state)
                        return EventCodeEnum.SOAKING_ERROR_PREPARE_ALREADY_WORKING;
                    else if (SoakingStateEnum.RECOVERY == state)
                        return EventCodeEnum.SOAKING_ERROR_RECOVERY_ALREADY_WORKING;
                    else if (SoakingStateEnum.MAINTAIN == state)
                        return EventCodeEnum.SOAKING_ERROR_MAINTAIN_ALREADY_WORKING;
                    else
                        return EventCodeEnum.SOAKING_ERROR_ALREADY_WORKING;

                }

                if (ManualSoakingStart)
                {
                    LoggerManager.SoakingErrLog($"Can't start manual soaking(manual soaking is already working)");
                    return EventCodeEnum.SOAKING_ERROR_MANUAL_ALREADY_WORKING;
                }

                if (this.StageSupervisor().StageMode != GPCellModeEnum.MAINTENANCE) //hhh_todo: maintenance 확인 관련 추가 작업 필요
                {
                    LoggerManager.SoakingErrLog($"Can't start manual soaking(need to maintenance mode)");
                    return EventCodeEnum.SOAKING_ERROR_NOT_MAINTENANCE_MODE;
                }

                if (false == this.CanIManualSoakingRun())
                {
                    return EventCodeEnum.SOAKING_ERROR_CAN_NOT_MANUAL_SOAK;
                }

                EventCodeEnum retVal = this.InnerStateTransition(new ManualSoakingState(this));
                LoggerManager.SoakingLog($"Do change Manualsoaking State");
                if (EventCodeEnum.NONE != retVal)
                {
                    LoggerManager.SoakingErrLog($"Failed to change 'ManualSoakingState'");
                    return retVal;
                }

                LoggerManager.SoakingLog($"CMD Proc : Manual soaking start");
                StatusSoakingTempInfo.StatusSoaking_StartTime = default;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"exception({err?.Message})");
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }

            return EventCodeEnum.NONE;
        }

        /// <summary>
        /// Manual soaking 동작을 위해 관련 모듈들의 State를 체크한다.
        /// </summary>
        /// <returns>true: manual soaking 동작 가능, flase: 불가</returns>
        private bool CanIManualSoakingRun()
        {
            StatusSoakingStateBase SubStateCheck = SoakingState as StatusSoakingStateBase;
            if (null != SubStateCheck)
            {
                var soakingSubState = SubStateCheck.SubState as SoakingSubStateBase;
                if (null != soakingSubState)
                {
                    if (soakingSubState.IsGoodOtherModuleStateToRun(true))
                        return true;
                }
            }


            return false;
        }

        //manual soaking stop
        public EventCodeEnum StopManualSoakingProc()
        {
            LoggerManager.SoakingLog($"CMD Proc : Manual soaking Stop");
            if (SoakingState.GetState() == SoakingStateEnum.MANUAL && (SoakingState.GetModuleState() == ModuleStateEnum.RUNNING || SoakingState.GetModuleState() == ModuleStateEnum.SUSPENDED))
            {
                SoakingCancelFlag = true;
                StatusSoakingTempInfo.StatusSoaking_StartTime = default;
                return EventCodeEnum.NONE;
            }
            else
                return EventCodeEnum.SOAKING_ERROR_THERE_IS_NO_MANUAL_SOAK;
        }

        //현재 cell의 soaking 상태의 일부 정보를 반환
        public (EventCodeEnum, DateTime/*Soaking start*/, SoakingStateEnum/*Soaking Status(pre,recovery,maintain..)*/, SoakingStateEnum/*soakingSubState*/, ModuleStateEnum/*Module state*/) GetCurrentSoakingInfo()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            StatusSoakingStateBase SubStateCheck = SoakingState as StatusSoakingStateBase;
            SoakingStateEnum SubStateVal = SoakingStateEnum.UNDEFINED;
            ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
            if (null != SubStateCheck)
            {
                SubStateVal = SubStateCheck.SubState.GetState();
                moduleState = SubStateCheck.GetModuleState();
            }

            if (this.Idle_SoakingFailed_PinAlign)
                ret = EventCodeEnum.SOAKING_ERROR_IDLE_PINALIGN;
            else if (this.Idle_SoakingFailed_WaferAlign)
                ret = EventCodeEnum.SOAKING_ERROR_IDLE_WAFERALIGN;
            else
            {
                ret = this.ManualSoakingRetVal;
            }

            return (ret, StatusSoakingTempInfo.StatusSoaking_StartTime, SoakingState.GetState(), SubStateVal, moduleState);
        }

        public void TraceLastSoakingStateInfo(bool bStart)
        {
            LastSoakingStateInfoObj?.TraceLastSoakingStateInfo(bStart);
        }

        public void SaveLastSoakingStateInfo(SoakingStateEnum state)
        {
            LastSoakingStateInfoObj?.SaveLastSoakingStateInfo(state);
        }

        /// <summary>
        /// soaking 중일때 현재 Soaking 상태를 정보 loader쪽에 갱신 처리한다.
        /// </summary>
        /// <param name="CurrentElapseSoakingTime">Soaking이 진행된 시간</param>
        /// <param name="forceUpdate">강제로 update할 것인지 여부</param>
        public bool GetStatusSoakingInfoUpdateToLoader(ref string SoakingTypeStr, ref string remainingTime, ref string ODVal, ref bool EnableStopSoakBtn)
        {
            try
            {
                bool ManualSoakingWorking = false;
                if (this.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && this.SoakingModule().ManualSoakingStart)
                {
                    LoggerManager.SoakingLog($"Manual soaking - PinAlign(High Module)");
                    ManualSoakingWorking = true;
                }

                bool EnableStatusSoaking = false;
                bool IsGettingOptionSuccessul = StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);
                if (false == IsGettingOptionSuccessul)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                    return false;
                }

                if (EnableStatusSoaking || ManualSoakingWorking)
                {
                    EnableStopSoakBtn = true;
                    var soakingSubState = (InnerState as StatusSoakingStateBase);
                    if (null != soakingSubState)
                    {
                        if (SoakingStateEnum.IDLE == soakingSubState.SubState.GetState() || SoakingStateEnum.DONE == soakingSubState.SubState.GetState())
                        {
                            EnableStopSoakBtn = false;    //soaking module이 Idle일때는 stop버튼 비활성화.
                        }

                        SoakingTypeStr = StatusSoakingTempInfo.beforeSendSoakingInfo.SoakingType;
                        remainingTime = StatusSoakingTempInfo.beforeSendSoakingInfo.RemainTime.ToString();
                        ODVal = ((int)StatusSoakingTempInfo.beforeSendSoakingInfo.ZClearance).ToString();
                        if (SoakingStateEnum.MAINTAIN == SoakingState.GetState())
                            EnableStopSoakBtn = false;

                        if (this.SoakingCancelFlag)
                            EnableStopSoakBtn = false;

                        return true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"exception({err?.Message})");
            }

            return false;
        }

        /// <summary>
        /// 현재 Soaking이 동작중인지 확인하고 soaking중이라면 호출 시점을 기준으로 Soaking을 멈춘다.
        /// Maintenance mode 진입 시 soaking 중이라면 중단 후 진입하기 위해 사용
        /// </summary>        
        /// <returns> EventCodeEnum </returns>
        public void Check_N_ClearStatusSoaking()
        {
            try
            {
                bool ShowToggleFlag = false;
                ShowToggleFlag = StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue();
                bool EnableStatusSoaking = false;
                bool IsGettingOptionSuccessul = StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);
                if (false == IsGettingOptionSuccessul)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                }

                if (EnableStatusSoaking || ShowToggleFlag)
                {
                    //maintain 일때는 Running중에도 done 상태이므로 이도 같이 처리 필요                    
                    var soakingState = SoakingState.GetState();
                    if (ModuleStateEnum.RUNNING == this.ModuleState.GetState() ||
                        ModuleStateEnum.SUSPENDED == this.ModuleState.GetState() ||
                        SoakingStateEnum.MAINTAIN == soakingState ||
                        ModuleStateEnum.ERROR == this.ModuleState.GetState() ||
                        ModuleStateEnum.PAUSED == this.ModuleState.GetState())
                    {
                        LoggerManager.SoakingLog($"Check_N_ClearStatusSoaking(), Soaking abort.");
                        this.Abort();
                    }

                    this.Idle_SoakingFailed_PinAlign = false;
                    this.Idle_SoakingFailed_WaferAlign = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Check_N_ClearStatusSoaking - exception({err?.Message})");
            }
        }

        /// <summary>
        /// before Zup soaking 정보 반환
        /// </summary>
        /// <param name="UseStatusSoakingFlag">Status soaking 사용여부(Show Toggle btn설정여부)</param>
        /// <param name="IsBeforeZupSoakingEnableFlag">z up soak 사용여부</param>
        /// <param name="BeforeZupSoakingTime"> soaking time</param>
        /// <param name="BeforeZupSoakingClearanceZ"> soaking시 사용할 OD</param>
        public void GetBeforeZupSoak_SettingInfo(out bool UseStatusSoakingFlag, out bool IsBeforeZupSoakingEnableFlag, out int BeforeZupSoakingTime, out double BeforeZupSoakingClearanceZ)
        {
            UseStatusSoakingFlag = false;
            IsBeforeZupSoakingEnableFlag = false;
            BeforeZupSoakingTime = 0;
            BeforeZupSoakingClearanceZ = -1000;

            try
            {
                if (StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue())
                {
                    UseStatusSoakingFlag = true;
                    bool enableStatusSoaking = false;
                    StatusSoakingParamIF.IsEnableStausSoaking(ref enableStatusSoaking);
                    if (enableStatusSoaking)
                    {
                        var BeforeZupSoakEvtInfo = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.StatusSoakingEvents.FirstOrDefault(x => x.SoakingTypeEnum.Value == EventSoakType.BeforeZUpSoak);
                        if (null != BeforeZupSoakEvtInfo)
                        {
                            if (BeforeZupSoakEvtInfo.UseEventSoaking.Value)
                            {
                                IsBeforeZupSoakingEnableFlag = true;
                                BeforeZupSoakingTime = BeforeZupSoakEvtInfo.SoakingTimeSec.Value;
                                BeforeZupSoakingClearanceZ = BeforeZupSoakEvtInfo.OD_Value.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"GetBeforeZupSoak_SettingInfo - exception({err?.Message})");
            }
        }

        /// <summary>
        /// 현재 설정된 Status soaking 사용여부 flag반환
        /// </summary>
        /// <returns>true: 사용, false:미사용</returns>
        public bool GetCurrentStatusSoakingUsingFlag()
        {
            bool retVal = false;
            try
            {
                retVal = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.UseStatusSoaking.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        /// <summary>
        /// 마지막으로 설정된 Status soaking 사용여부 옵션 반환
        /// </summary>
        /// <returns> true: 사용, false:미사용</returns>
        public bool GetBeforeStatusSoakingUsingFlag()
        {
            return BeforeStatusSoakingOption_About_UseFlag;
        }

        /// <summary>
        /// 마지막으로 설정되었던 정보를 저장(Status Soaking 사용 여부 flag 저장)
        /// </summary>
        /// <param name="UseStatusSoakingFlag"> Status 사용여부 flag</param>
        public void SetBeforeStatusSoakingUsingFlag(bool UseStatusSoakingFlag)
        {
            BeforeStatusSoakingOption_About_UseFlag = UseStatusSoakingFlag;
        }

        /// <summary>
        /// status soaking 사용여부로 인한 prepare status 로 state 변경
        /// </summary>
        public void ForceChange_PrepareStatus()
        {
            try
            {
                LoggerManager.SoakingLog($"Force Change Soaking operation type : Prepare");
                this.InnerStateTransition(new PrepareSoakingState(this));
                var BeforeState = this.PreInnerState as ISoakingState;
                LoggerManager.SoakingLog($"Transition : {BeforeState?.GetState().ToString()} Status -> Prepare Status");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog(err.Message);
            }
        }

        /// <summary>
        /// Status Soaking을 위한 Polish Wafer 필요여부를 확인
        /// </summary>
        /// <returns></returns>
        public bool GetPolishWaferForStatusSoaking()
        {
            bool bNeedPolishWafer = false;

            //system init이 아니라면 false
            if (false == this.MonitoringManager().IsMachineInitDone)
                return bNeedPolishWafer;

            if (this.StageSupervisor().GetStageLockMode() == StageLockMode.LOCK)
                return bNeedPolishWafer;

            try
            {
                // Status Soaking을 사용하지 않거나 Enable False인 경우
                var soakingConfigParameter = StatusSoakingDeviceFileObj?.StatusSoakingConfigParameter;
                if (soakingConfigParameter == null ||
                    !soakingConfigParameter.ShowStatusSoakingSettingPage.Value ||
                    !soakingConfigParameter.UseStatusSoaking.Value)
                {
                    return bNeedPolishWafer;
                }

                // Current State의 Soaking Param을 구한다.
                StatusSoakingCommonParam commonParam = null;
                switch (SoakingState.GetState())
                {
                    case SoakingStateEnum.PREPARE:
                        commonParam = soakingConfigParameter.GetPrepareStatusCommonParam();
                        break;
                    case SoakingStateEnum.RECOVERY:
                        commonParam = soakingConfigParameter.GetRecoveryStatusCommonParam();
                        break;
                    case SoakingStateEnum.MAINTAIN:
                        commonParam = soakingConfigParameter.GetMaintainStatusCommonParam();
                        break;
                    case SoakingStateEnum.MANUAL:
                        commonParam = soakingConfigParameter.GetManualStatusCommonParam();
                        break;
                    default:
                        commonParam = null;
                        break;
                }

                // Polish Wafer가 설정되어 있지 않은 경우
                if (commonParam == null ||
                    !commonParam.UsePolishWafer.Value ||
                    commonParam.SelectedPolishwafer.Count == 0)
                {
                    return bNeedPolishWafer;
                }

                // Soaking이 Trigger 됐는지 확인
                var soakingState = SoakingState as StatusSoakingStateBase;
                if (soakingState == null)
                {
                    return bNeedPolishWafer;
                }

                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    // Lot Running Prepare와 Recovery에서 Polish wafer soaking을 지원함
                    if (SoakingState.GetState() == SoakingStateEnum.PREPARE || SoakingState.GetState() == SoakingStateEnum.RECOVERY)
                    {
                        if (soakingState.SubState.GetState() == SoakingStateEnum.SUSPENDED_FOR_WAITING_WAFER_OBJ)
                        {
                            if (this.GetParam_Wafer().GetWaferType() != EnumWaferType.POLISH && StatusSoakingTempInfo.request_PolishWafer)
                            {
                                bNeedPolishWafer = true;
                            }
                        }
                    }
                }
                else if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    bool CheckNeedPolishWafer = false;
                    if (SoakingState.GetState() == SoakingStateEnum.PREPARE)
                        CheckNeedPolishWafer = true;
                    else if (SoakingState.GetState() == SoakingStateEnum.RECOVERY || SoakingState.GetState() == SoakingStateEnum.MAINTAIN)
                    {
                        if (StatusSoakingTempInfo.request_PolishWafer)
                            CheckNeedPolishWafer = true;
                    }

                    if (CheckNeedPolishWafer)
                    {
                        //Idle 상태에서의 polish wafer 사용 유무 반환
                        if (soakingState.SubState.GetState() == SoakingStateEnum.SUSPENDED_FOR_WAITING_WAFER_OBJ)
                        {
                            bNeedPolishWafer = true;
                        }
                        else if (SoakingState.GetState() == SoakingStateEnum.MAINTAIN && soakingState.SubState is SoakingSubSuspendForWaferObject) //maintain mode에서는 suspend 상태도 done으로 상태를 반환해 주기 때문에 class 비교필요(이유 maintain에서는 
                        {
                            bNeedPolishWafer = true;
                        }
                    }
                }
                else
                {
                    bNeedPolishWafer = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog(err.Message);
            }

            return bNeedPolishWafer;
        }

        public bool IsUsePolishWafer()
        {
            return SoakingSysParam_Clone.UsePolishWafer.Value;
        }

        public int GetChuckAwayToleranceLimitX()
        {
            return SoakingSysParam_Clone.ChuckAwayToleranceLimitX.Value;
        }

        public int GetChuckAwayToleranceLimitY()
        {
            return SoakingSysParam_Clone.ChuckAwayToleranceLimitY.Value;
        }

        public int GetChuckAwayToleranceLimitZ()
        {
            return SoakingSysParam_Clone.ChuckAwayToleranceLimitZ.Value;
        }


        public EventCodeEnum Get_MaintainSoaking_OD(out double retval)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;           
            retval = this.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.MaintainStateConfig.NotExistWaferObj_OD.Value;
            try
            {
                var waferExist = this.GetParam_Wafer().GetStatus();
                if (waferExist == EnumSubsStatus.EXIST)
                {
                    var StepInfo = this.StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.MaintainStateConfig.SoakingStepTable.FirstOrDefault();
                    if (StepInfo != null)
                    {
                        retval = StepInfo.OD_Value.Value;
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else if(waferExist == EnumSubsStatus.NOT_EXIST)
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public bool IsStatusSoakingRunning()
        {
            bool use_status_soaking = false;
            if (this.StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue())
            {
                this.StatusSoakingParamIF.IsEnableStausSoaking(ref use_status_soaking);
            }

            if (use_status_soaking)
            {
                if (ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    return true;
            }

            return false;
        }

        public void Check_N_KeepPreviousSoakingData()
        {
            if (this.IsStatusSoakingRunning())
            {
                if (this.GetStatusSoakingState() == SoakingStateEnum.PREPARE)
                {
                    bool use_polish_wafer = false;
                    this.StatusSoakingParamIF.Get_UsePolishWaferFlag(SoakingStateEnum.PREPARE, ref use_polish_wafer);
                    if (use_polish_wafer)
                    {
                        this.UsePreviousStatusSoakingDataForRunning = true;
                        LoggerManager.SoakingLog($"Prepare Idle soaking is running. so will keep prepare soaking data.");
                    }
                }
            }
        }

        /// <summary>
        /// Status soaking 사용 시 현재 설정된 Soaking State에서 Polish wafer 사용여부를 반환한다.
        /// </summary>        
        /// <returns>true:사용, flase: 미사용</returns>
        public bool IsEnablePolishWaferSoakingOnCurState()
        {
            bool use_polish_wafer = false;
            bool use_status_soaking = false;
            if (this.StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue())
            {
                this.StatusSoakingParamIF.IsEnableStausSoaking(ref use_status_soaking);
            }

            if (use_status_soaking)
            {
                this.StatusSoakingParamIF.Get_UsePolishWaferFlag(this.GetStatusSoakingState(), ref use_polish_wafer);
            }

            return use_polish_wafer;
        }

        /// <summary>
        /// Event Soaking이 끝난 후 Pin align을 할지 말지 결정
        /// </summary>
        /// <param name="pinalignon"> Pin align여부 받는 변수</param>
        /// <param name="cursoakparam">현재 진행 중인 Soaking Parameter</param>
        public void CheckPostPinAlign(ref bool pinalignon, SoakingParamBase cursoakparam)
        {
            if (pinalignon)
            {
                if (cursoakparam.Post_Pinalign.Value)
                {
                    pinalignon = true;
                }
                else
                {
                    LoggerManager.Debug($"[SoakingModule] IsCheckPostPinAlign() pinalignon = {pinalignon}, cursoakparam = {cursoakparam.EventSoakingType.Value}, Post Pinalign = {cursoakparam.Post_Pinalign.Value}");
                    pinalignon = false;
                }
            }
        }
        /// <summary>
        /// Soaking의 상태가 Device를 Load할 수 있는 상태인지 Bool Type으로 반환해준다.
        /// True: Load 가능
        /// False: Load 불가능
        /// </summary>
        /// <returns></returns>
        public bool IsDeviceLoadpossible(out SoakingStateEnum stateEnum)
        {
            bool retVal = false;
            stateEnum = SoakingStateEnum.UNDEFINED;
            try
            {
                if (ModuleState.GetState() == ModuleStateEnum.DONE || ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    retVal = true;
                }
                else if (ModuleState.GetState() == ModuleStateEnum.SUSPENDED)
                {
                    bool isStatusSoakingEnable = false;
                    StatusSoakingParamIF.IsEnableStausSoaking(ref isStatusSoakingEnable);
                    isStatusSoakingEnable = isStatusSoakingEnable && StatusSoakingDeviceFileObj.Get_ShowStatusSoakingSettingPageToggleValue();

                    if (isStatusSoakingEnable)
                    {
                        var soakingState = SoakingState as StatusSoakingStateBase;
                        stateEnum = soakingState.SubState.GetState();
                        if (stateEnum == SoakingStateEnum.SUSPENDED_FOR_WAITING_WAFER_OBJ
                            || stateEnum == SoakingStateEnum.SUSPENDED_FOR_CARDDOCKING)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                    else
                    {
                        retVal = false;
                    }
                }

                var IsRequested_IDOWAFERALIGN = this.WaferAligner().CommandRecvSlot.IsRequested<IDOWAFERALIGN>();
                var IsRequested_IDOSamplePinAlignForSoaking = this.PinAligner().CommandRecvSlot.IsRequested<IDOSamplePinAlignForSoaking>();
                var IsRequested_IDOPinAlignAfterSoaking = this.PinAligner().CommandRecvSlot.IsRequested<IDOPinAlignAfterSoaking>();
                var IsRequested_IDOPINALIGN = this.PinAligner().CommandRecvSlot.IsRequested<IDOPINALIGN>();

                if (retVal
                    && IsRequested_IDOWAFERALIGN == false //Wafer Align Command 요청된 것 없고
                    && IsRequested_IDOSamplePinAlignForSoaking == false//Sample Pin aling Command 요청된 것 없고
                    && IsRequested_IDOPinAlignAfterSoaking == false//Soaking후 동작하는 Pin align Command 요청된 것 없고
                    && IsRequested_IDOPINALIGN == false)//Pin align Command 요청된 것 없고
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
        /// Status soaking에서 temp deviation은 soaking dev parameter의 SoakingTempToleranceDeviation값이 사용된다.
        /// SoakingTempToleranceDeviation defualt 값은 5, LowerLimit 0 , UpperLimit 10 으로 기준 한다.
        /// </summary>        
        /// <returns>true:사용, flase: 미사용</returns>
        public bool IsCurTempWithinSetSoakingTempRange()
        {
            bool retVal = false;
            try
            {
                double deviation = 0;
                double minTemp = 0, maxTemp = 0;
                deviation = StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.SoakingTempTolerance.Value;

                if (deviation < StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.SoakingTempTolerance.LowerLimit
                    || deviation > StatusSoakingDeviceFileObj.StatusSoakingConfigParameter.AdvancedSetting.SoakingTempTolerance.UpperLimit)
                {
                    deviation = 5; //defualt value
                }

                TemperatureChangeSource source = this.TempController().GetCurrentTempInfoInHistory()?.TempChangeSource ?? TemperatureChangeSource.TEMP_DEVICE;

                if (this.TempController().GetApplySVChangesBasedOnDeviceValue() 
                    && this.TempController().GetDevSetTemp() != this.TempController().TempInfo.TargetTemp.Value
                    && source != TemperatureChangeSource.CARD_CHANGE // Card Change로 인한 온동 변경 시 DevTemp랑 TargeTemp는 달라질 수 있다. 현재 척온도가 Target Temp 범위 내에 들어오면 idle로 보내서 idle에서 머물도록 하자.
                    && source != TemperatureChangeSource.TEMP_EXTERNAL)
                {
                    retVal = false;
                    return retVal;
                }

                minTemp = this.TempController().TempInfo.TargetTemp.Value - deviation;
                maxTemp = this.TempController().TempInfo.TargetTemp.Value + deviation;

                if (maxTemp < minTemp)
                {
                    double tmp = maxTemp;
                    maxTemp = minTemp;
                    maxTemp = tmp;
                }

                retVal = (minTemp <= this.TempController().TempInfo.CurTemp.Value) && (this.TempController().TempInfo.CurTemp.Value <= maxTemp);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }
        public bool Get_PrepareStatusSoak_after_DeviceChange()
        {
            return SoakingSysParam_Clone.PreheatSoak_after_DeviceChange.Value;
        }
        public void Set_PrepareStatusSoak_after_DeviceChange(bool PreheatSoak_after_DeviceChange)
        {
            try
            {
                SoakingSysParam_Clone.PreheatSoak_after_DeviceChange.Value = PreheatSoak_after_DeviceChange;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Set_PrepareStatusSoak_after_DeviceChange(): Error occurred. Err = {err.Message}");
            }
        }

        public void SetStatusSoakingForceTransitionState()
        {
            try
            {
                bool EnableStatusSoaking = false;
                bool ShowToggleFlag = false;
                ShowToggleFlag = StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue();
                bool IsGettingOptionSuccessul = StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);
                if (false == IsGettingOptionSuccessul)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                }

                if (ShowToggleFlag && EnableStatusSoaking)
                {
                    StatusSoakingStateBase SubStateCheck = SoakingState as StatusSoakingStateBase;
                    if (null != SubStateCheck && null != SubStateCheck.SubState)
                    {
                        SubStateCheck.SubState.StatusSoakingInfoUpdateToLoader(0, true, true);
                        LoggerManager.SoakingLog($"Clear Soaking information(No Soak)");

                        var StatusSoakingSubBase = InnerState as StatusSoakingStateBase;
                        if (null != StatusSoakingSubBase)
                        {
                            LoggerManager.SoakingLog("Immediately change to 'SoakingSubIdle'");
                            StatusSoakingSubBase.SetStatusSoakingForceTransitionState();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void InitPrepareSoakingTrigger()
        {
            try
            {
                if (SoakingStateEnum.PREPARE == SoakingState.GetState())
                {
                    LoggerManager.SoakingLog($"Before Trigger Value => TempDiff = {TempDiffTriggered}, ProbeCardChangeTriggered = {ProbeCardChangeTriggered}, DeviceChangeTrigger = {DeviceChangeTriggered}");
                    TempDiffTriggered = false;
                    ProbeCardChangeTriggered = false;
                    DeviceChangeTriggered = false;
                    LoggerManager.SoakingLog($"Current Trigger Value => InitPrepareSoakingTrigger. TempDiff = {TempDiffTriggered}, ProbeCardChangeTriggered = {ProbeCardChangeTriggered}, DeviceChangeTrigger = {DeviceChangeTriggered}");
                }
            }
            catch (Exception  err)
            {
                LoggerManager.Exception(err);
            }
        }
    }//end of class
}
