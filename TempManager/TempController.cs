using LogModule;
using NotifyEventModule;
using SciChart.Charting.Model.DataSeries;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Temperature
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Event;
    using ProberInterfaces.State;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.TempManager;
    using SerializerUtil;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using TempControl;
    using TempControllerParameter;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TempController : SequenceServiceBase, ITempController, IStateModule
                                    , INotifyPropertyChanged, IParamNode

    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; }
        public List<object> Nodes { get; set; }

        private IParam _TempSafetySysParam;

        public IParam TempSafetySysParam
        {
            get { return _TempSafetySysParam; }
            set { _TempSafetySysParam = value; }
        }

        private IParam _TempSafetyDevParam;

        public IParam TempSafetyDevParam
        {
            get { return _TempSafetyDevParam; }
            set { _TempSafetyDevParam = value; }
        }

        private IParam _TempControllerDevParam;

        public IParam TempControllerDevParam
        {
            get { return _TempControllerDevParam; }
            set { _TempControllerDevParam = value; }
        }
        public TempSafetyDevParam TempSafetyDevParameter { get; set; }
        public TempControllerDevParam TempControllerDevParameter { get; set; }

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Temperature);
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

        //private double _CurTemp;
        //public double CurTemp
        //{
        //    get { return _CurTemp; }
        //    set
        //    {
        //        if (value != _CurTemp)
        //        {
        //            _CurTemp = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //public bool IsSettingToSetTempValue = false; //SetTemp값으로 Set이 되었는지 확인하는 변수.
        //public double PreSetTemp = 0; // not 1/10!!

        //private double _CurTemp;
        //public double CurTemp
        //{
        //    get { return _CurTemp; }
        //    set
        //    {
        //        if (value != _CurTemp)
        //        {
        //            _CurTemp = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private double _SetTemp;
        //public double SetTemp // not 1/10!!
        //{
        //    get
        //    {
        //        return _SetTemp;
        //    }
        //    private set
        //    {
        //        if (_SetTemp != value)
        //        {
        //            PreSetTemp = _SetTemp;
        //            IsSettingToSetTempValue = false;
        //            _SetTemp = (double)value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private XyDataSeries<DateTime, double> _dataSeries_CurTemp
            = new XyDataSeries<DateTime, double>() { SeriesName = "PV" };
        [DataMember]
        public XyDataSeries<DateTime, double> dataSeries_CurTemp
        {
            get { return _dataSeries_CurTemp; }
            set
            {
                if (value != _dataSeries_CurTemp)
                {
                    _dataSeries_CurTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private XyDataSeries<DateTime, double> _dataSeries_SetTemp
            = new XyDataSeries<DateTime, double>() { SeriesName = "SV" };
        [DataMember]
        public XyDataSeries<DateTime, double> dataSeries_SetTemp
        {
            get { return _dataSeries_SetTemp; }
            set
            {
                if (value != _dataSeries_SetTemp)
                {
                    _dataSeries_SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OverHeatTemp = 0;
        public double OverHeatTemp
        {
            get { return _OverHeatTemp; }
            set
            {
                if (value != _OverHeatTemp)
                {
                    _OverHeatTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public bool IsSettingToSetTempValue = false; //SetTemp값으로 Set이 되었는지 확인하는 변수.
        //public double PreSetTemp = 0; // not 1/10!!

        //private double _SetTemp;
        //public double SetTemp // not 1/10!!
        //{
        //    get
        //    {
        //        return _SetTemp;
        //    }
        //    private set
        //    {
        //        if (_SetTemp != value)
        //        {
        //            PreSetTemp = _SetTemp;
        //            IsSettingToSetTempValue = false;
        //            _SetTemp = (double)value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        #region ==> IModule Realization
        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo
            = new ObservableCollection<TransitionInfo>();
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

        private TempControllerParam _TempControllerParam;
        public TempControllerParam TempControllerParam
        {
            get { return _TempControllerParam; }
            set { _TempControllerParam = value; }
        }

        private TimeSpan _RunTimeSpan;
        public TimeSpan RunTimeSpan
        {
            get { return _RunTimeSpan; }
            set
            {
                if (value != _RunTimeSpan)
                {
                    _RunTimeSpan = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TemperatureInfo _TempInfo;
        public TemperatureInfo TempInfo
        {
            get { return _TempInfo; }
            set
            {
                if (value != _TempInfo)
                {
                    _TempInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsOccurTimeOut = false;

        public bool IsOccurTimeOut
        {
            get { return _IsOccurTimeOut; }
            set { _IsOccurTimeOut = value; }
        }

        private bool _IsOccurOutOfRange = false;

        /// <summary>
        /// deviation, probing deviation 중에 하나라도 벗어난 경우 true 가 된다.
        /// </summary>
        public bool IsOccurOutOfRange
        {
            get { return _IsOccurOutOfRange; }
            set { _IsOccurOutOfRange = value; }
        }

        private bool _IsOccurOutOfRangeDeviation;

        public bool IsOccurOutOfRangeDeviation
        {
            get { return _IsOccurOutOfRangeDeviation; }
            set { _IsOccurOutOfRangeDeviation = value; }
        }

        private bool _IsOccurOutOfRangeProbingDeviation;

        public bool IsOccurOutOfRangeProbingDeviation
        {
            get { return _IsOccurOutOfRangeProbingDeviation; }
            set { _IsOccurOutOfRangeProbingDeviation = value; }
        }



        //private AutoCooling AutoCooling;
        private DateTime lastUpdateTime;
        private int maxTempHistoryCount = 1 * 60 * 60 * 12; // For 12 hours
        static int logElapsed = 0;
        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum RetVal = ModuleStateEnum.UNDEFINED;
            try
            {
                DateTime time = DateTime.Now;

                var timeFromLastUpdate = (time - lastUpdateTime);
                TempInfo.CurTemp.Value = (double)TempManager.PV;
                TempInfo.MV.Value = (double)TempManager.MV;

                //if (TempInfo.SetTemp.Value == this.TempControllerDevParameter.SetTemp.Value)
                //{
                    TempInfo.SetTemp.Value = (double)TempManager.SV;
                //}

                if (this.EnvControlManager()?.GetDewPointModule()?.CurDewPoint != null)
                {
                    TempInfo.DewPoint.Value = (double)this.EnvControlManager()?.GetDewPointModule()?.CurDewPoint;
                }

                double updateIntervalInSec = 1;

                if (timeFromLastUpdate.TotalSeconds >= updateIntervalInSec)
                {
                    var CurTemp = (double)TempManager.PV;
                    dataSeries_CurTemp.Append(time, CurTemp / 1.0);
                    dataSeries_SetTemp.Append(time, (double)TempManager.SV / 1.0);
                    if (dataSeries_CurTemp.Count > maxTempHistoryCount | dataSeries_SetTemp.Count > maxTempHistoryCount)
                    {
                        Task.Run(() =>
                        {
                            dataSeries_CurTemp.RemoveAt(0);
                            dataSeries_SetTemp.RemoveAt(0);
                        }).Wait();
                    }
                    if (logElapsed >= TempControllerParam.LoggingInterval.Value * updateIntervalInSec)
                    {
                        LoggerManager.SetTempInfo(TempManager.SV, TempManager.PV, TempInfo.DewPoint.Value, TempInfo.MV.Value);
                        LoggerManager.TempLog($"Temp.: SV = {TempManager.SV / 1.0,5:0.00}, PV = {TempManager.PV / 1.0,5:0.00}, DP = {TempInfo.DewPoint.Value,5:0.00}, MV = {TempInfo.MV.Value,5:0.00}");
                        logElapsed = 0;
                    }
                    else
                    {
                        logElapsed++;
                    }

                    lastUpdateTime = time;
                }

                //Temp를 보고 Purge Air On/ Off(고온에서는 Perge Air를 킬 필요가 없어서)
                bool isPurgeAir = ControlTopPurgeAir();
                if(isPurgeAir  != IsPurgeAirBackUpValue)
                {
                    IsPurgeAirBackUpValue = isPurgeAir;
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, IsPurgeAirBackUpValue);
                    if (ioret != IORet.NO_ERR)
                    {
                        LoggerManager.Debug($"[TempController]SequenceRun(), Fail Purge Air changing. Output Value: {IsPurgeAirBackUpValue}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[TempController]SequenceRun(), Changed Purge Air. Output Value: {IsPurgeAirBackUpValue}");
                    }
                    
                }

                // Fail safe
                if (this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE.IOOveride.Value != EnumIOOverride.EMUL)
                {
                    bool purgeAirState = false;
                    var ioRet = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, out purgeAirState);
                    if (purgeAirState != IsPurgeAirBackUpValue)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, IsPurgeAirBackUpValue);
                    }
                }

                //if(this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                //{
                //    bool chillerCanLotState = this.EnvControlManager()?.ChillerManager?.CanRunningLot() ?? false;
                //    if(chillerCanLotState == false)
                //    {
                //        this.LotOPModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.CHILLER_CHECK_CAN_USE_CHILLER_ERROR,
                //              "Check the chiller connection or error.", this.GetType().Name);
                //        this.LotOPModule().PauseSourceEvent = this.LotOPModule().ReasonOfError.GetLastEventCode();
                //        this.CommandManager().SetCommand<ILotOpPause>(this);
                //    }
                //}

                RetVal = Execute();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }
        #endregion

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        private ITempManager _TempManger;
        public ITempManager TempManager
        {
            get { return _TempManger; }
            set
            {
                _TempManger = value;
            }
        }

        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        //private IChiller _ChillerModule;
        //public IChiller ChillerModule
        //{
        //    get { return _ChillerModule; }
        //    set
        //    {
        //        _ChillerModule = value;
        //    }
        //}

        // private IDewPointModule _DewPointModule;
        //public IDewPoint DewPointModule
        //{
        //    get { return _DewPointModule; }
        //    set
        //    {
        //        _DewPointModule = value;
        //    }
        //}
        //private IDryAir _DryAirModule;
        //public IDryAir DryAirModule
        //{
        //    get { return _DryAirModule; }
        //    set
        //    {
        //        _DryAirModule = value;
        //    }
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
        private IProbeCommandToken _RequestToken;
        public IProbeCommandToken RequestToken
        {
            get { return _RequestToken; }
            set { _RequestToken = value; }
        }

        public EnumCommunicationState CommunicationState { get; set; }

        private TempControllerState TCState { get; set; }
        public IInnerState InnerState
        {
            get { return TCState; }
            set
            {
                if (TCState != value)
                {
                    TCState = value as TempControllerState;
                }
            }
        }

        private bool _InitActivateState;
        public bool InitActivateState
        {
            get { return _InitActivateState; }
            set
            {
                if (value != _InitActivateState)
                {
                    _InitActivateState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IInnerState _ReserveNextState;
        public IInnerState ReserveNextState
        {
            get { return _ReserveNextState; }
            set
            {
                if (value != _ReserveNextState)
                {
                    _ReserveNextState = value;
                    RaisePropertyChanged();
                }
            }
        }



        public IInnerState PreInnerState
        {
            get;
            set;
        }
        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        private bool _ForcedSetValue;
        public bool ForcedSetValue
        {
            get { return _ForcedSetValue; }
            private set { _ForcedSetValue = value; }
        }


        public object TempChangeLockObj { get; set; } = new object();
        public TempController()
        {
        }
        private void CurTemp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                this.GEMModule().GetPIVContainer().CurTemperature.Value = TempInfo.CurTemp.Value;
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
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    ModuleState = new ModuleIdleState(this);
                    temp_backup_lock = new object();

                    //AutoCooling = new AutoCooling(this);

                    InitState();

                    retval = this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired);

                    if (TempManager == null)
                    {
                        MakeTempModules();                        
                        InitTempModules();

                        LoadBackupTempInfo();
                        if(this.TempBackupInfo.TempInfoHistory?.Count > 0)
                        {
                            var reloadBack = this.GetCurrentTempInfoInHistory();
                            SetSV(reloadBack.TempChangeSource, reloadBack.SetTemp, willYouSaveSetValue: false);
                        }
                        else
                        {
                            InitDevParameter();

                            if (this.DeviceModule().IsExistSetTempParemterFromNeedChangeParameter() == false)
                            {
                                this.SetSV(TemperatureChangeSource.TEMP_DEVICE, (double)this.TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
                            }
                        }

                        retval = TempManager.StartModule();
                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"TempManager.StartModule() Failed");
                        }

                        bool isExistInGemParam = false;
                        foreach (var dicSVIDInfo in this.GEMModule().DicSVID.DicProberGemID.Value)
                        {
                            if (dicSVIDInfo.Key.Contains($"StagePVTemp[{this.LoaderController().GetChuckIndex()}]"))
                            {
                                if (dicSVIDInfo.Value.Enable)
                                {
                                    isExistInGemParam = true;
                                    break;
                                }
                            }
                        }
                        if (isExistInGemParam)
                        {
                            TempInfo.CurTemp.PropertyChanged -= CurTemp_PropertyChanged;
                            TempInfo.CurTemp.PropertyChanged += CurTemp_PropertyChanged;
                        }
                    }

                    if(TempControllerDevParam != null)
                    {
                        //기존 파라미터에서 새로 생성이 되면 0이고 default 값 set 해주기 위함.
                        if (TempControllerDevParameter.EmergencyAbortTempTolerance.Value == 0)
                        {
                            TempControllerDevParameter.EmergencyAbortTempTolerance.Value = 10;
                            SaveDevParameter();
                        }
                    }

                    if(TempControllerParam != null && TempInfo != null)
                    {
                        var backuptemp = GetCurrentTempInfoInHistory();
                        if (backuptemp?.TempChangeSource == TemperatureChangeSource.UNDEFINED)//TODO: 이 기능이 처음 들어갔을 떄 초기화 해주기위함.
                        {

                            SetSV(TemperatureChangeSource.TEMP_DEVICE, TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: false);
                       
                        }
                        else
                        {
                            
                            SetSV(backuptemp.TempChangeSource, backuptemp.SetTemp, willYouSaveSetValue: false);
                        }
                        
                        // 원래 코드
                        //if (TempControllerParam.LastTempChangeSource.Value == TemperatureChangeSource.UNDEFINED)
                        //{
                        //    if (GetApplySVChangesBasedOnDeviceValue() == false)
                        //    {
                        //        SetSV(TemperatureChangeSource.TEMP_EXTERNAL, TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
                        //    }
                        //    else
                        //    {
                        //        SetSV(TemperatureChangeSource.TEMP_DEVICE, TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
                        //    }
                        //}
                        //else
                        //{
                        //    SetSV(TempControllerParam.LastTempChangeSource.Value, TempControllerParam.LastSetTargetTemp.Value, willYouSaveSetValue: false);
                        //}

                    }

                    Initialized = true;

                    
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

        private TempBackupInfo _TempBackupInfo = new TempBackupInfo();

        public TempBackupInfo TempBackupInfo
        {
            get { return _TempBackupInfo; }
            set { _TempBackupInfo = value; }
        }



        public TempBackupInfo LoadBackupTempInfo()
        {
            try
            {
                TempBackupInfo backup = new TempBackupInfo();
                string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
                string folderPath = this.FileManager().FileManagerParam.LogRootDirectory + $@"\Backup\{cellNo}";
                string fileName = backup.FileName;
                string fullPath = Path.Combine(folderPath, fileName);
                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (File.Exists(fullPath))
                {
                    IParam tmpParam = null;
                    EventCodeEnum RetVal = this.LoadParameter(ref tmpParam, typeof(TempBackupInfo), null, fullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        backup = tmpParam as TempBackupInfo;
                        this.TempBackupInfo = backup;
                    }
                    //SerializeManager.Deserialize(fullPath, out var param, deserializerType: SerializerType.JSON);
                    //TempBackupInfo = JsonConvert.DeserializeObject<TempBackupInfo>(param.ToString());
                }               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return TempBackupInfo;
        }

        public TempBackupInfo SaveBackupTempInfo()
        {
            try
            {                
                if(TempBackupInfo.TempInfoHistory != null)
                {
                    string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
                    string folderPath = this.FileManager().FileManagerParam.LogRootDirectory + $@"\Backup\{cellNo}";
                    string fileName = TempBackupInfo.FileName;
                    string fullPath = Path.Combine(folderPath, fileName);
                    if (Directory.Exists(folderPath) == false)
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    //SerializeManager.Serialize(fullPath, this.TempBackupInfo, serializerType: SerializerType.JSON);
                    this.SaveParameter(this.TempBackupInfo, fixFullPath: fullPath);
                }
                
            } 
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return TempBackupInfo;
        }

        public void InitState(bool callClearState = false)
        {
            if ((this.EnvControlManager().GetChillerModule()?.ChillerParam?.ChillerModuleMode?.Value ?? EnumChillerModuleMode.NONE) == EnumChillerModuleMode.NONE)
            {
                InnerStateTransition(new TCIdleState(this));
            }
            else
            {
                if (!callClearState)
                    InnerStateTransition(new TC_ColdIdleState(this));
            }
        }


        private void MakeTempModules()
        {
            #region => Temperature
            if (TempControllerParam.TempModuleMode.Value == TempModuleMode.E5EN)
            {
                TempManager = new Temp.TempManager(TempModuleMode.E5EN);
            }
            else if (TempControllerParam.TempModuleMode.Value == TempModuleMode.EMUL)
            {
                TempManager = new Temp.TempManager(TempModuleMode.EMUL);
            }
            else
            {
                TempManager = new Temp.TempManager(TempModuleMode.EMUL);
            }
            #endregion

            //#region => Chiller
            //if (TempControllerParam.ChillerModuleMode.Value == ChillerModuleMode.HUBER)
            //{
            //    ChillerModule = new ChillerModule();
            //}
            //else if (TempControllerParam.ChillerModuleMode.Value == ChillerModuleMode.EMUL)
            //{
            //    ChillerModule = null;
            //}
            //else if (TempControllerParam.ChillerModuleMode.Value == ChillerModuleMode.NONE)
            //{
            //    ChillerModule = null;
            //}
            //#endregion

            //#region => DryAir
            //if (TempControllerParam.DryAirModuleMode.Value == DryAirModuleMode.HUBER)
            //{
            //DryAirModule = new DryAirModule();
            //}
            //else if (TempControllerParam.DryAirModuleMode.Value == DryAirModuleMode.EMUL)
            //{
            //    DryAirModule = new EmulDryAirModule();
            //}
            //else
            //{
            //    DryAirModule = new EmulDryAirModule();
            //}
            //#endregion

            //#region => DewPoint
            //DewPointModule = new DewPointReader();
            //#endregion
        }

        private EventCodeEnum InitTempModules()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = InitHelpFunc(TempManager);

            //retVal = InitHelpFunc(DryAirModule);
            //retVal = InitHelpFunc(DewPointModule);

            //if (ChillerModule != null)
            //{
            //    retVal = InitHelpFunc(ChillerModule);
            //}

            return retVal;

            EventCodeEnum InitHelpFunc(IModule tempModule)
            {
                EventCodeEnum isSucess = EventCodeEnum.UNDEFINED;

                if (tempModule != null)
                {
                    if (tempModule is IHasSysParameterizable)
                    {
                        isSucess = (tempModule as IHasSysParameterizable)?.LoadSysParameter() ?? EventCodeEnum.UNDEFINED;
                        if (isSucess != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"{tempModule.GetType().Name}.LoadSysParameter() Failed");
                        }
                    }

                    tempModule.InitModule();

                    if (isSucess != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"{tempModule.GetType().Name}.InitModule() Failed");
                    }
                }

                return isSucess;
            }

        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is DeviceChangedEvent)
                {
                    if (((TempControllerDevParameter.SetTemp.Value != TempInfo.TargetTemp.Value) && (GetApplySVChangesBasedOnDeviceValue())))
                    {
                        if (this.DeviceModule().IsExistSetTempParemterFromNeedChangeParameter() == false)
                        {
                            this.SetSV(TemperatureChangeSource.TEMP_DEVICE, (double)this.TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
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

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                TempManager.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            InnerState.Pause();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            InnerState.Resume();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            InnerState.End();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Abort()
        {
            InnerState.Abort();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }
        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                stat = InnerState.GetModuleState();
                ModuleState.StateTransition(stat);
                RunTokenSet.Update();

                CheckReserveNextState();

                (InnerState as TCStateBase)?.MonitoringTemperature();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stat;
        }
        public EnumTemperatureState GetTempControllerState()
        {
            return TCState.GetState();
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new TempControllerParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(TempControllerParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    TempControllerParam = tmpParam as TempControllerParam;
                }

                IParam tmpSatetySysParam = null;
                tmpSatetySysParam = new TempSafetySysParam();
                tmpSatetySysParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpSatetySysParam, typeof(TempSafetySysParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    TempSafetySysParam = tmpSatetySysParam as TempSafetySysParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(TempControllerParam);
                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TempController.SaveSysParameter() Fail. TempControllerParam");
                }

                RetVal = this.SaveParameter(TempSafetySysParam);
                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TempController.SaveSysParameter() Fail. TempSafetySysParam");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {
                RetVal = TCState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                try
                {
                    PreInnerState = InnerState;
                    InnerState = state;

                    if (PreInnerState != null
                        && InnerState != null)
                    {
                        LoggerManager.Debug($"[TempControllerState({PreInnerState?.GetType().Name})] " +
                        $"StateTransition to {InnerState.GetType().Name}");
                        this.LoaderController().UpdateLotDataInfo(StageLotDataEnum.TEMPSTATE, TCState.GetState().ToString());

                        #region <summary> state에 따른 온도 deviation 진입 여부 확인 </summary>
                        EnumTemperatureState innterState = (InnerState as TempControllerState).GetState();
                        if (InnerState.GetModuleState() == ModuleStateEnum.DONE || innterState == EnumTemperatureState.Monitoring)
                        {
                            //InnerState.GetModuleState() == ModuleStateEnum.DONE 
                            //TC_ColdDonePerformState, TC_ColdDoneState, TempInRange, TCDoneState
                            //innterState == EnumTemperatureState.Monitoring
                            //TC_ColdMonitoringState, TCMonitoringState

                            TempInfo.SetTemperatureWithInDeviation(true);
                        }
                        else if (InnerState.GetModuleState() == ModuleStateEnum.RUNNING && innterState == EnumTemperatureState.SetToTemp)
                        {
                            //innterState == EnumTemperatureState.SetToTemp
                            //TC_ColdNomalTriggerState, TC_ColdNomalTriggerPerformState, TC_ColdOverHeatingTriggerState, TC_ColdSetTempForFrontDoorOpenReachedTriggerState
                            //TCNomalTriggerState, TCOverHeatingTriggerState, TCSetTempForFrontDoorOpenReachedTriggerState
                            TempInfo.SetTemperatureWithInDeviation(false);
                        }
                        #endregion
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    LoggerManager.Debug($"[TempControllerState({PreInnerState.GetType().Name})] " +
                    $"Fail StateTransition to {InnerState.GetType().Name}");
                    retVal = EventCodeEnum.UNDEFINED;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                EnumTemperatureState state = (InnerState as TempControllerState).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            bool retVal = true;
            try
            {
                //foreach (var subModule in SubModules.SubModules)
                //{
                //    if (subModule.GetState() == SubModuleStateEnum.PROCESSING)
                //    {
                //        retVal = false;
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new TempControllerDevParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(TempControllerDevParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    TempControllerDevParameter = tmpParam as TempControllerDevParam;
                    TempControllerDevParam = TempControllerDevParameter;
                }

                IParam tmpSafetyParam = null;
                tmpSafetyParam = new TempSafetyDevParam();
                tmpSafetyParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpSafetyParam, typeof(TempSafetyDevParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    TempSafetyDevParameter = tmpSafetyParam as TempSafetyDevParam;
                    TempSafetyDevParam = TempSafetyDevParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(TempControllerDevParameter);
                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TempController.SaveDevParameter() Fail. TempControllerDevParameter");
                }

                RetVal = this.SaveParameter(TempSafetyDevParameter);
                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TempController.SaveDevParameter() Fail. TempSafetyDevParameter");
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
        /// LoadDevParameter에서 가져온 값을 사용하기 위한 함수
        /// 즉 Device가 변경될 때마다 Init을 해줘야하는 내용이 있어야함
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (TempInfo == null)
                {
                    TempInfo = new TemperatureInfo();
                    Extensions_IParam.CollectCommonElement(TempInfo, this.GetType().Name);
                }

                TempControllerDevParameter.SetTemp.ReportPropertyPath = this.GEMModule().GetPIVContainer().GetStageSVTemp(this.LoaderController().GetChuckIndex()).PropertyPath;
                TempControllerDevParameter.SetTemp.ConvertToReportTypeEvent += SetTempDevParam_ConvertToReportTypeEvent;
                TempControllerDevParameter.SetTemp.ValueChangedEvent += SetTempDevParam_ValueChangedEvent;
                this.GEMModule().GetPIVContainer().GetStageSVTemp(this.LoaderController().GetChuckIndex()).OriginPropertyPath = TempControllerDevParameter.SetTemp.PropertyPath;
                this.GEMModule().GetPIVContainer().GetStageSVTemp(this.LoaderController().GetChuckIndex()).ConvertToOriginTypeEvent += StageSV_ConvertToOriginTypeEvent;


                TempInfo.MonitoringInfo.MonitoringEnable = TempControllerDevParameter.TemperatureMonitorEnable.Value;
                TempInfo.MonitoringInfo.TempMonitorRange = TempControllerDevParameter.TempMonitorRange.Value;
                TempInfo.MonitoringInfo.WaitMonitorTimeSec = TempControllerDevParameter.WaitMonitorTimeSec.Value;
                TempInfo.DeviceSetTemp = TempControllerDevParameter.SetTemp.Value;



           
                //if (Initialized == true)
                //{                   
                //    if (((TempControllerDevParameter.SetTemp.Value != TempInfo.TargetTemp.Value) && (GetApplySVChangesBasedOnDeviceValue())))
                //    {
                //        if (this.DeviceModule().IsExistSetTempParemterFromNeedChangeParameter() == false)
                //        {
                //            this.SetSV(TemperatureChangeSource.TEMP_DEVICE, (double)this.TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
                //        }
                //    }

                //}



                //retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public bool IsCurTempWithinSetTempRange(bool checkDevTemp = true)
        {
            bool retVal = false;

            try
            {
                double deviation = 0;
                double minTemp = 0, maxTemp = 0;

                deviation = TempControllerDevParameter.TempToleranceDeviation.Value;

                //minTemp = this.SetTemp - deviation;
                //maxTemp = this.SetTemp + deviation;

                TemperatureChangeSource source = this.TempController().GetCurrentTempInfoInHistory()?.TempChangeSource ?? TemperatureChangeSource.TEMP_DEVICE;

                if(source == TemperatureChangeSource.CARD_CHANGE)
                {
                    retVal = this.CardChangeModule().IsCCAvailableSatisfied(needToSetTempToken: false) == EventCodeEnum.NONE;
                    return retVal;
                }
                else if (checkDevTemp && this.GetApplySVChangesBasedOnDeviceValue())
                {
                    if (this.TempControllerDevParameter.SetTemp.Value != this.TempInfo.SetTemp.Value)
                    {
                        retVal = false;
                        return retVal;
                    }
                }

                minTemp = this.TempInfo.SetTemp.Value - deviation;
                maxTemp = this.TempInfo.SetTemp.Value + deviation;

                MinMaxCheckAndSwap(ref minTemp, ref maxTemp);

                //retVal = (minTemp <= this.CurTemp) && (this.CurTemp <= maxTemp);

                retVal = (minTemp <= this.TempInfo.CurTemp.Value) && (this.TempInfo.CurTemp.Value <= maxTemp);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        /// <summary>
        /// deviation 온도를 parameter 설정값 외의 값을 사용하고싶을때 
        /// deviationval 는 온도 값을 넘겨주면 내부에서 *10을 한다.
        /// </summary>
        /// <param name="deviationval"></param>
        /// <returns></returns>
        public bool IsCurTempWithinSetTempRangeDeviation(double deviationval)
        {
            bool retVal = false;
            try
            {
                double deviation = 0;
                double minTemp = 0, maxTemp = 0;
                deviation = deviationval;

                //minTemp = this.SetTemp - deviation;
                //maxTemp = this.SetTemp + deviation;

                minTemp = this.TempInfo.SetTemp.Value - deviation;
                maxTemp = this.TempInfo.SetTemp.Value + deviation;

                MinMaxCheckAndSwap(ref minTemp, ref maxTemp);

                //retVal = (minTemp <= this.CurTemp) && (this.CurTemp <= maxTemp);

                retVal = (minTemp <= this.TempInfo.CurTemp.Value) && (this.TempInfo.CurTemp.Value <= maxTemp);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        /// <summary>
        /// margin을 감안하여 현재 온도가 SetTemp보다 높으면 true로 반환하는 함수 
        /// </summary>
        /// <param name="deviationval"></param>
        /// <returns></returns>
        public bool IsCurTempUpperThanSetTemp(double setTemp, double mergin)
        {
            bool retVal = false;
            try
            {
                double threshold_mergin = 0;
                double minTemp = 0;
                threshold_mergin = mergin;

                //minTemp = this.SetTemp - deviation;
                //maxTemp = this.SetTemp + deviation;

                minTemp = setTemp - threshold_mergin;                

                retVal = (minTemp <= this.TempInfo.CurTemp.Value);//&& (this.TempInfo.CurTemp.Value <= maxTemp);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void MinMaxCheckAndSwap(ref double minTemp, ref double maxTemp)
        {
            try
            {
                if (maxTemp < minTemp)
                {
                    double tmp = maxTemp;
                    maxTemp = minTemp;
                    maxTemp = tmp;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns> not 1/10. </returns>
        public double GetSetTempWithOverHeatTemp()
        {
            //double settemp = this.SetTemp;
            double settemp = this.TempInfo.SetTemp.Value;

            try
            {
                settemp += OverHeatTemp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return settemp;
        }

        object temp_backup_lock = null;

        private void UpdateLastestInfoTempInfoHistory(TemperatureChangeSource source, double targetTemp)//TemperatureInfo latest_tempInfo)
        {
            try
            {
                TempEventInfo latest_tempInfo = new TempEventInfo(source, targetTemp);

                //string history = "TempInfoHistory: ";
                lock (temp_backup_lock)
                {
                    if(TempBackupInfo.TempInfoHistory.Count() > 0)
                    {
                        if (TempBackupInfo.TempInfoHistory[0].SetTemp == targetTemp && TempBackupInfo.TempInfoHistory[0].TempChangeSource == source)
                        {
                            return;
                        }
                    }
                    

                    this.TempBackupInfo.TempInfoHistory.Insert(0, latest_tempInfo);
                    if (this.TempBackupInfo.TempInfoHistory.Count > 100)
                    {
                        TempBackupInfo.TempInfoHistory = TempBackupInfo.TempInfoHistory.GetRange(0, 100);
                    }

                    SaveBackupTempInfo();

                    //for (int i = 0; i < this.TempBackupInfo.TempInfoHistory.Count; i++)
                    //{
                    //    history += $"({i}) {this.TempBackupInfo.TempInfoHistory[i].TempChangeSource}, {this.TempBackupInfo.TempInfoHistory[i].SetTemp.Value}";
                    //    if (i + 1 < this.TempBackupInfo.TempInfoHistory.Count)
                    //    {
                    //        history += " | ";
                    //    }


                    //}
                    //LoggerManager.Debug($"{history}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public TempEventInfo GetCurrentTempInfoInHistory()
        {
            TempEventInfo ret = null;
            try
            {
                lock (temp_backup_lock)
                {
                    var latest_Info = this.TempBackupInfo.TempInfoHistory.Where(w => w.TempChangeSource != TemperatureChangeSource.UNDEFINED).FirstOrDefault();
                    if (latest_Info != null)
                    {                        
                        ret = latest_Info;
                    }                   
                }
                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        /// <summary>
        /// 이전에 SetTemp 된 Temp 정보를 가지고 오는 함수, 같은 Source 이더라도 다른 SetTemp이면 Previous 정보로 가지고 온다. 
        /// </summary>
        /// <returns></returns>
        public TempEventInfo GetPreviousTempInfoInHistory()
        {
            TempEventInfo ret = null;
            try
            {
                lock (temp_backup_lock)
                {
                    var latest_Info = GetCurrentTempInfoInHistory();
                    if (latest_Info != null)
                    {
                        var index = this.TempBackupInfo.TempInfoHistory.IndexOf(latest_Info); // 이 source를 기준으로 찾는다. 
                        var getprev_lastItem = this.TempBackupInfo.TempInfoHistory.GetRange(index + 1, this.TempBackupInfo.TempInfoHistory.Count() - (index + 1)).Where(w => w.TempChangeSource != TemperatureChangeSource.UNDEFINED).FirstOrDefault();
                        if(getprev_lastItem != null)
                        {
                            ret = getprev_lastItem;
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

        /// <summary>
        /// 이전에 SetTemp 된 Temp 정보를 가지고 오는 함수, 이전 정보중 다른 Source의 SetTemp 정보를 가지고 온다. 
        /// </summary>
        /// <returns></returns>
        public TempEventInfo GetPreviousSourceTempInfoInHistory()
        {
            TempEventInfo ret = null;
            try
            {
                lock (temp_backup_lock)
                {
                    var latest_Info = GetCurrentTempInfoInHistory();
                    if (latest_Info != null)
                    {
                        var index = this.TempBackupInfo.TempInfoHistory.IndexOf(latest_Info); // 이 source를 기준으로 찾는다. 
                        var getprev_lastItem = this.TempBackupInfo.TempInfoHistory.GetRange(index + 1, this.TempBackupInfo.TempInfoHistory.Count() - (index + 1))
                                                                                  .Where(w => w.TempChangeSource != TemperatureChangeSource.UNDEFINED && w.TempChangeSource != latest_Info.TempChangeSource).FirstOrDefault();
                        if (getprev_lastItem != null)
                        {
                            ret = getprev_lastItem;
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


        public void SetSV(TemperatureChangeSource source, double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false, double overHeating = 0.0, double Hysteresis = 0.0)
        {
            try
            {
                try
                {
                    lock (TempChangeLockObj)
                    {
                        var backup = GetCurrentTempInfoInHistory();
                        if (willYouSaveSetValue && GetApplySVChangesBasedOnDeviceValue())
                        {
                            TempControllerDevParameter.SetTemp.Value = (double)(changeSetTemp); // ValueChangeEvnet 때문에 두번 돌게 됨. Lock 때문에 문제는 없지만 
                            LoggerManager.Debug($"Set SetTemp for DevParameter : {TempControllerDevParameter.SetTemp.Value}");
                            SaveDevParameter();
                        }
                        this.GEMModule().GetPIVContainer().SetSVTemp(changeSetTemp);

                        if (TempInfo.SetTemp.Value != changeSetTemp & TempInfo.PreSetTemp.Value != TempInfo.SetTemp.Value)
                        {
                            TempInfo.PreSetTemp.Value = TempInfo.SetTemp.Value;
                        }


                        if (forcedSetValue)
                        {
                            SetForcedSetValue(forcedSetValue);
                        }

                        if (TempInfo.TargetTemp.Value != changeSetTemp)
                        {
                            TempInfo.TargetTemp.Value = changeSetTemp;
                            LoggerManager.Debug($"[TempController] SetSV({backup?.TempChangeSource}). TargetTemp set to {TempInfo.TargetTemp.Value}, input source:{source}");

                            TempInfo.OverHeatingOffset = overHeating;
                            LoggerManager.Debug($"[TempController] SetSV({backup?.TempChangeSource}). OverHeatingOffset set to {TempInfo.OverHeatingOffset}");
                            TempInfo.OverHeatingHysteresis = Hysteresis;
                            LoggerManager.Debug($"[TempController] SetSV({backup?.TempChangeSource}). OverHeatingHysteresis set to {TempInfo.OverHeatingHysteresis}");
                        }
                        UpdateLastestInfoTempInfoHistory(source, changeSetTemp);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

 
        // TempCopntrol에 SV를 Set을 안한다.
        // 실제로 Set은 전의 Set값과 현재의 Set값을 비교하여 ControlState안의 TempManager.SetTempWithOption에서 일어난다.
        public void SetSV(double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false)
        {
            try
            {
                lock (TempChangeLockObj)
                {
                    if (willYouSaveSetValue && GetApplySVChangesBasedOnDeviceValue())
                    {
                        TempControllerDevParameter.SetTemp.Value = (double)(changeSetTemp);
                        LoggerManager.Debug($"Set SetTemp for DevParameter : {TempControllerDevParameter.SetTemp.Value}");                        
                        SaveDevParameter();
                    }
                    this.GEMModule().GetPIVContainer().SetSVTemp(changeSetTemp);

                    if (TempInfo.SetTemp.Value != changeSetTemp & TempInfo.PreSetTemp.Value != TempInfo.SetTemp.Value)
                    {
                        TempInfo.PreSetTemp.Value = TempInfo.SetTemp.Value;
                    }

                    if (forcedSetValue)
                    {
                        SetForcedSetValue(forcedSetValue);
                    }

                    if (TempInfo.TargetTemp.Value != changeSetTemp)
                    {
                        TempInfo.TargetTemp.Value = changeSetTemp;
                        LoggerManager.Debug($"[TempController] SetSV(). TargetTemp set to {TempInfo.TargetTemp.Value}");

                        TempControllerParam.LastSetTargetTemp.Value = TempInfo.TargetTemp.Value;
                        LoggerManager.Debug("[TempController] SetSV(). LastSetTargetTemp system parameter set to {this.TempControllerParam.LastSetTargetTemp.Value}.");
                        SaveSysParameter();
                    }
                    //else
                    //{
                    //    if(forcedSetValue)
                    //    {
                    //        SetActivatedState();
                    //    }
                    //} 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public void SetForcedSetValue(bool value)
        {
            try
            {
                if(ForcedSetValue != value)
                {
                    ForcedSetValue = value;
                    LoggerManager.Debug($"[TempControllerState] SetForcedSetValue() set to {ForcedSetValue}.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// string to double 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string StageSV_ConvertToOriginTypeEvent(object input)
        {
            string retVal = input.ToString();
            try
            {
                double val = -999;
                bool drst = double.TryParse(retVal, out val);

                if (drst == true)
                {                    
                    return retVal;
                }                
                else
                {
                    retVal = val.ToString();//TODO: Convert Success/Fail 에 대한 결과 확인할 것. 
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// double to string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string SetTempDevParam_ConvertToReportTypeEvent(object input)
        {            
            return input.ToString();
        }

        public void SetTempDevParam_ValueChangedEvent(Object oldValue, Object newValue, object valueChangedParam = null)
        {
            try
            {                
                double inputvalue = 0;
                bool rst = double.TryParse(newValue.ToString(), out inputvalue);
                if(rst == true)
                {                    
                    LoggerManager.Debug($"Set SetTemp for DevParameter : {inputvalue}");
                    bool isNeedApplyValueFromHost = false;
                    if(valueChangedParam != null)
                    {
                        if(valueChangedParam is bool)
                        {
                            isNeedApplyValueFromHost = (bool)valueChangedParam;
                            if(GetApplySVChangesBasedOnDeviceValue() == false)
                            {
                                TempControllerDevParameter.SetTemp.Value = (double)oldValue;
                            }
                        }
                    }
                    if (GetApplySVChangesBasedOnDeviceValue() || isNeedApplyValueFromHost)
                    {
                        SetSV(TemperatureChangeSource.TEMP_DEVICE, inputvalue, willYouSaveSetValue: true);
                        //this.CommandManager().SetCommand<ITemperatureSettingTriggerOccurrence>(this);
                    }
                }
                else
                {
                    LoggerManager.Debug($"[TempController] StageSV_ValueChangedEvent(): Failed. inputValue is not double.");
                }               
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void SetAmbientTemp([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                var chillerObj = this.EnvControlManager().ChillerManager.GetChillerModule();
                if (chillerObj != null)
                {
                    if (chillerObj.ChillerParam.AmbientTemp.Value > 0)
                    {
                        SetSV(TemperatureChangeSource.TEMP_EXTERNAL, chillerObj.ChillerParam.AmbientTemp.Value, willYouSaveSetValue: false);//로더로부터 호출되기 위한 함수이므로??  함수 의도 확인 필요.
                        string fileName = Path.GetFileName(filePath);
                        string logmsg = $"[TempController] SetAmbientTemp(). Set to {chillerObj.ChillerParam.AmbientTemp.Value} [{fileName}({sourceLineNumber}) '{callFuncNm}']";
                        LoggerManager.Debug(logmsg);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTemperatureFromDevParamSetTemp()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            //this.SetTemp = (double)this.TempControllerDevParameter.SetTemp.Value;
            //this.TempInfo.SetTemp.Value = (double)this.TempControllerDevParameter.SetTemp.Value;
            this.SetSV(TemperatureChangeSource.TEMP_DEVICE, (double)this.TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
            //this.TempManager.SetTempWithOption(this.TempControllerDevParameter.SetTemp.Value);

            // Call Function, 로그 찍기 
        }


        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }
        public double GetTemperatureOffset(double value)
        {
            double retOffset = 0.0;
            retOffset = this.TempManager.GetTempOffset(value);
            return retOffset;
        }

        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            return retVal;
        }

        public EventCodeEnum CheckSVWithDevice()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(TempInfo != null)
                {
                    if(TempInfo.DeviceSetTemp == TempInfo.SetTemp.Value)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// 이 함수는 System Parameter에 있는 Temp table("HeaterOffsetDictionary")에 현재 디바이스의 SetTemp가 포함되어 있는지 확인하는 함수 
        /// </summary>
        /// <returns>true:포함 됨 false: 포함되지 않음</returns>
        public bool CheckIfTempIsIncluded()
        {
            bool isTempIncluded = false;
            try
            {
                double set_temp = GetSetTemp();
                if (GetHeaterOffsets() != null && !(GetHeaterOffsets().Count <= 0))
                {
                    isTempIncluded = GetHeaterOffsets().ContainsKey(set_temp);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CheckIfTempIsIncluded(): Error occurred. Err = {err.Message}");
                throw;
            }
            return isTempIncluded;
        }

        private bool _IsPurgeAirBackUpValue = false;
        public bool IsPurgeAirBackUpValue
        {
            get { return _IsPurgeAirBackUpValue; }
            set
            {
                if (value != _IsPurgeAirBackUpValue)
                {
                    _IsPurgeAirBackUpValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// SV와 PV를 보고 TopPurge Air를 끌지 킬지 구하는 함수
        /// </summary>
        /// <returns></returns>
        public bool ControlTopPurgeAir()
        {
            bool result = IsPurgeAirBackUpValue;
            try
            {
                double set_temp = GetSetTemp();
                double cur_temp = GetTemperature();
                double ref_temp = TempControllerParam.RefTempOfControlPurgeAir.Value;
                double hysteresisValue = TempControllerParam.HysteresisValue_PurgeAir.Value;

                if (set_temp <= ref_temp || cur_temp <= ref_temp)
                {
                    result = true;
                    
                }
                else if(cur_temp >= (ref_temp + hysteresisValue))
                {
                    result = false;
                }
                else
                {
                    if(cur_temp < ref_temp)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CheckIfTempIsIncluded(): Error occurred. Err = {err.Message}");
                throw;
            }
            return result;
        }

        public bool GetApplySVChangesBasedOnDeviceValue()
        {
            return TempControllerParam.ApplySVChangesBasedOnDevice.Value;
        }

        #region // Remote methods
        public int GetHeaterOffsetCount()
        {

            return TempManager.Dic_HeaterOffset.Count;
        }

        public Dictionary<double, double> GetHeaterOffsets()
        {
            return TempManager.Dic_HeaterOffset;
        }
        public bool GetCheckingTCTempTable()
        {
            return TempManager.CheckingTCTempTable;
        }

        public double GetSV()
        {
            return TempInfo.SetTemp.Value;
        }

        public double GetPV()
        {
            return TempInfo.CurTemp.Value;
        }

        public void ClearHeaterOffset()
        {
            TempManager.Dic_HeaterOffset.Clear();
        }

        public void AddHeaterOffset(double reftemp, double measuredtemp)
        {
            TempManager.Dic_HeaterOffset.Add(reftemp, measuredtemp);
        }

        public double GetTemperature()
        {
            return TempInfo.CurTemp.Value;
        }
        public void SetTemperature()
        {
            TempManager.SetTempWithOption(TempInfo.SetTemp.Value);
        }

        public TemperatureInfo GetTempInfo()
        {
            return TempInfo;
        }
        public double GetDevSetTemp()
        {
            return this.TempControllerDevParameter.SetTemp.Value;
        }
        public EventCodeEnum SetDevSetTemp(double setTemp)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (TempControllerDevParameter != null)
                {
                    TempControllerDevParameter.SetTemp.Value = setTemp;
                    LoggerManager.Debug($"[TempController] SetDeviceSetTempValue(). SetTemp Value : {TempControllerDevParameter.SetTemp.Value}");
                    SaveDevParameter();
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SaveOffsetParameter()
        {
            TempManager.SaveSysParameter();
            TempManager.SetTempWithOption(TempInfo.SetTemp.Value);
        }
        public void SetEndTempEmergencyErrorCommand()
        {
            try
            {
                this.CommandManager().SetCommand<IEndTempEmergencyError>(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetIsOccurTimeOutError()
        {
            return IsOccurTimeOut;
        }
        public void ClearTimeOutError()
        {
            IsOccurTimeOut = false;
        }
        public double GetCCActivatableTemp()
        {
            double retVal = 30.0;
            try
            {
                retVal = this.CardChangeModule().GetCCActivatableTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetVacuum(bool ison)
        {

        }

        public void EnableRemoteUpdate()
        {
        }

        public void DisableRemoteUpdate()
        {
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void SetLoggingInterval(long seconds)
        {
            if (seconds >= TempControllerParam.LoggingInterval.LowerLimit &
                seconds <= TempControllerParam.LoggingInterval.UpperLimit)
            {
                TempControllerParam.LoggingInterval.Value = seconds;
                //SaveSysParameter();
            }
        }

        public double GetDeviaitionValue()
        {
            double retval = 0;

            try
            {
                if (TempControllerDevParameter != null)
                {
                    retval = TempControllerDevParameter.TempToleranceDeviation.Value;
                }
                else
                {
                    LoggerManager.Error($"Deviation Value can not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetEmergencyAbortTempTolereance()
        {
            double retval = 0;

            try
            {
                if (TempControllerDevParameter != null)
                {
                    retval = TempControllerDevParameter.EmergencyAbortTempTolerance.Value;
                }
                else
                {
                    LoggerManager.Error($"GetEmergencyAbortTempTolereance Value can not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public TempPauseTypeEnum GetTempPauseType()
        {
            TempPauseTypeEnum retval = TempPauseTypeEnum.NONE;

            try
            {
                if (TempControllerDevParameter != null)
                {
                    retval = TempControllerDevParameter.TempPauseType.Value;
                }
                else
                {
                    LoggerManager.Error($"TempToleranceDeviationForEmergencyAbort Value can not loaded.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetDeviaitionValue(double deviation, bool emergency = false)
        {
            try
            {
                if (TempControllerDevParameter != null)
                {
                    if(emergency == true)
                    {
                        TempControllerDevParameter.EmergencyAbortTempTolerance.Value = deviation;
                    }
                    else
                    {
                        TempControllerDevParameter.TempToleranceDeviation.Value = deviation;
                    }

                    SaveDevParameter();
                    LoadDevParameter();
                    InitDevParameter();
                }
                else
                {
                    LoggerManager.Error($"Deviation Value can not changed.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTempPauseType(TempPauseTypeEnum pausetype)
        {
            try
            {
                if (TempControllerDevParameter != null)
                {
                    TempControllerDevParameter.TempPauseType.Value = pausetype;

                    SaveDevParameter();
                    LoadDevParameter();
                    InitDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetTempPauseType Value can not changed.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CheckSetDeviationParamLimit(double deviation, bool emergency = false)
        {
            bool retval = false;
            try
            {
                TempControllerDevParam.Init();
                LoggerManager.Debug("CheckSetDeviationParamLimit Action Done");
                if (emergency == true)
                {
                    if (TempControllerDevParameter.EmergencyAbortTempTolerance.UpperLimit >= deviation
                        && TempControllerDevParameter.EmergencyAbortTempTolerance.LowerLimit <= deviation)
                    {
                        retval = true;
                    }
                }
                else
                {
                    if (TempControllerDevParameter.TempToleranceDeviation.UpperLimit >= deviation
                           && TempControllerDevParameter.TempToleranceDeviation.LowerLimit <= deviation)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public double GetMV()
        {
            return TempInfo.MV.Value;
        }

        public double GetCurDewPointValue()
        {
            return TempInfo.DewPoint.Value;
        }

        public double GetSetTemp()
        {
            return TempInfo.SetTemp.Value;
        }

        public bool GetHeaterOutPutState()
        {
            if (TempManager.Get_OutPut_State() == 0)
                return false;
            else
                return true;
        }

        public double GetDewPointTolerance()
        {
            return this.EnvControlManager().GetDewPointModule().Tolerence;
        }

        public double GetMonitoringMVTimeInSec()
        {
            double retVal = 0.0;
            try
            {
                retVal = TempControllerParam?.MonitoringMVTimeInSec?.Value ?? 0.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetMonitoringMVTimeInSec(double value)
        {
            try
            {
                if (TempControllerParam != null)
                {
                    TempControllerParam.MonitoringMVTimeInSec.Value = value;
                    SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTempMonitorInfo(TempMonitoringInfo param)
        {
            try
            {
                if (TempControllerDevParameter != null)
                {
                    TempControllerDevParameter.TempMonitorRange.Value = param.TempMonitorRange;
                    TempControllerDevParameter.WaitMonitorTimeSec.Value = param.WaitMonitorTimeSec;
                    TempControllerDevParameter.TemperatureMonitorEnable.Value = param.MonitoringEnable;
                    SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public TempMonitoringInfo GetTempMonitorInfo()
        {
            try
            {
                if (TempControllerDevParameter != null)
                {
                    TempInfo.MonitoringInfo.MonitoringEnable = TempControllerDevParameter.TemperatureMonitorEnable.Value;
                    TempInfo.MonitoringInfo.TempMonitorRange = TempControllerDevParameter.TempMonitorRange.Value;
                    TempInfo.MonitoringInfo.WaitMonitorTimeSec = TempControllerDevParameter.WaitMonitorTimeSec.Value;

                    return TempInfo.MonitoringInfo;
                }
                return null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }

        public byte[] GetParamByte()
        {
            byte[] param = null;
            try
            {
                param = SerializeManager.SerializeToByte(TempControllerDevParameter, typeof(TempControllerDevParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }
        public EventCodeEnum SetParamByte(byte[] devparam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(devparam, out target, typeof(TempControllerDevParam));

                if (target != null)
                {
                    TempControllerDevParameter = target as TempControllerDevParam;
                    SaveDevParameter();
                    LoadDevParameter();
                    InitDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetPolishWaferIParam function is faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        public bool IsUsingChillerState()
        {
            bool retVal = false;

            try
            {
                var chillerModule = this.EnvControlManager()?.GetChillerModule() ?? null;

                if(chillerModule != null)
                {
                    if (chillerModule.GetChillerMode() != EnumChillerModuleMode.NONE)
                    {
                        if (TempInfo.TargetTemp.Value <= chillerModule.ChillerParam.CoolantInTemp.Value)
                        {
                            retVal = true;
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

        public long GetLimitRunTimeSeconds()
        {
            long limitTime = 0;
            try
            {
                limitTime = TempControllerParam?.LimitRunTimeSeconds?.Value ?? 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return limitTime;
        }

        public void SetActivatedState(bool forced = false)
        {
            try
            {
                if ((this.EnvControlManager().GetChillerModule()?.ChillerParam?.ChillerModuleMode?.Value ?? EnumChillerModuleMode.NONE) != EnumChillerModuleMode.NONE)
                {
                    ReserveNextState = new Activated(this, forced);
                    LoggerManager.Debug($"[TempContoller] call SetActivatedState()");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckReserveNextState()
        {
            try
            {
                if(ReserveNextState != null)
                {
                    EventCodeEnum retVal = InnerStateTransition(ReserveNextState);
                    if(retVal == EventCodeEnum.NONE)
                    {
                        ReserveNextState = null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        public void RestorePrevSetTemp()
        {
            try
            {
                TempEventInfo prevTempInfo = GetPreviousSourceTempInfoInHistory();
                if(prevTempInfo != null)
                {
                    if(prevTempInfo != null)
                    {
                        SetSV(prevTempInfo.TempChangeSource, prevTempInfo.SetTemp, willYouSaveSetValue: true);
                    }
                    else
                    {
                        LoggerManager.Debug($"[TempController] RestorePrevSetTemp(): Prev SetTemp is not exist.");
                    }
                    
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 온도가 한번 deviation 내에 들어온 이 후 (Done, TempInRange) deviation 범위 내에 있는지, 없는지를 확인 해야 하기에 deviation 을 확인할 수 있는 상태인지를 판단해주는 함수.
        /// true : deviaiton 확인 가능 (온도 변경 이후 deviation 내에 들어온 적이 있음 , 프로그램 실행 시 제외)
        /// false: deviation 확인 불 가능 (온도 변경 이후 아직 변경 중이라서 deviation 내에 들어 온 적이 없는 경우)
        /// </summary>
        public bool CanCheckDeviationState()
        {
            bool retVal = false;
            try
            {
                ModuleStateEnum moduleState = ModuleState.GetState();
                EnumTemperatureState innterState = (InnerState as TempControllerState).GetState();
                if (moduleState == ModuleStateEnum.DONE || innterState == EnumTemperatureState.Monitoring || TempInfo.TemperatureWIthInDeviationFlag)
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
    }
}
