using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProberInterfaces;
using ProbingSequenceManager;
using System.Collections.ObjectModel;
using SubstrateObjects;
using ProberInterfaces.Param;
using ProberInterfaces.Command;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using ProberErrorCode;
using ProberInterfaces.State;
using LogModule;
using BinManagerModule;
using ProberInterfaces.Retest;
using ProberInterfaces.BinData;
using System.ServiceModel;
using SerializerUtil;
using ProberInterfaces.Temperature;
using ProbingDataInterface;
using RetestObject;
using BinParamObject;
using ProberInterfaces.Command.Internal;
using NotifyEventModule;
using ProberInterfaces.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ProberInterfaces.Loader.RemoteDataDescription;

namespace ProbingModule
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Probing : IProbingModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        public bool Initialized { get; set; } = false;

        public bool IsInfo = false;

        private IParam _ProbingModuleSysParam_IParam;
        public IParam ProbingModuleSysParam_IParam
        {
            get { return _ProbingModuleSysParam_IParam; }
            set
            {
                if (value != _ProbingModuleSysParam_IParam)
                {
                    _ProbingModuleSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ProbingModuleSysParam ProbingModuleSysParamRef
        {
            get { return ProbingModuleSysParam_IParam as ProbingModuleSysParam; }
        }


        private BinManager _BinManagerModule = new BinManager();
        public BinManager BinManagerModule
        {
            get { return _BinManagerModule; }
            set
            {
                if (value != _BinManagerModule)
                {
                    _BinManagerModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ProbingDryRunFlag;
        public bool ProbingDryRunFlag
        {
            get { return _ProbingDryRunFlag; }
            set
            {
                if (value != _ProbingDryRunFlag)
                {
                    _ProbingDryRunFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StilProbingZUpFlag = false;

        public bool StilProbingZUpFlag
        {
            get { return _StilProbingZUpFlag; }
            set { _StilProbingZUpFlag = value; }
        }

        //private Dictionary<int, EnableEnum> _SortedBinInfos = new Dictionary<int, EnableEnum>();
        //public Dictionary<int, EnableEnum> SortedBinInfos
        //{
        //    get { return _SortedBinInfos; }
        //    set
        //    {
        //        if (value != _SortedBinInfos)
        //        {
        //            _SortedBinInfos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private IParam _ProbingModuleDevParam_IParam;
        public IParam ProbingModuleDevParam_IParam
        {
            get { return _ProbingModuleDevParam_IParam; }
            set
            {
                if (value != _ProbingModuleDevParam_IParam)
                {
                    _ProbingModuleDevParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ProbingModuleDevParam ProbingModuleDevParamRef
        {
            get { return ProbingModuleDevParam_IParam as ProbingModuleDevParam; }
        }


        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.Probing);
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

        private bool _IsReservePause = false;
        public bool IsReservePause
        {
            get { return _IsReservePause; }
            set
            {
                if (value != _IsReservePause)
                {
                    _IsReservePause = value;
                    RaisePropertyChanged();
                }
            }
        }


        private WaferObject Wafer;

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
        public IParam GetProbingDevIParam(int idx = -1)
        {
            return this.ProbingModuleDevParam_IParam;

        }

        public double ZClearence
        {
            get { return Math.Abs(ProbingModuleDevParamRef.ZClearence.Value) * -1; }
            set
            {
                ProbingModuleDevParamRef.ZClearence.Value = value;

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.CLEARANCE, ZClearence.ToString());

                RaisePropertyChanged();
            }
        }

        public double OverDrive
        {
            get { return ProbingModuleDevParamRef.OverDrive.Value; }
            set
            {
                ProbingModuleDevParamRef.OverDrive.Value = value;

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.PROBINGOD, OverDrive.ToString());
                SaveDevParameter();
                RaisePropertyChanged();
            }
        }

        private PinCoordinate _PinZeroPos;
        public PinCoordinate PinZeroPos
        {
            get { return _PinZeroPos; }
            set
            {
                if (value != _PinZeroPos)
                {
                    _PinZeroPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbingEndReason _ProbingEndReason;
        public ProbingEndReason ProbingEndReason
        {
            get { return _ProbingEndReason; }
            set
            {
                if (value != _ProbingEndReason)
                {
                    _ProbingEndReason = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> Command Slot Properties
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
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

        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        #endregion

        #region ==> State
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

        public IInnerState PreInnerState { get; set; }

        public IInnerState InnerState
        {
            get { return _ProbingModuleState; }
            set
            {
                if (value != _ProbingModuleState)
                {
                    _ProbingModuleState = value as ProbingState;
                }
            }
        }

        private ProbingState _ProbingModuleState;
        public ProbingState ProbingModuleState
        {
            get { return _ProbingModuleState; }
        }

        private ProbingInfo _ProbingProcessStatus;
        public ProbingInfo ProbingProcessStatus
        {
            get { return _ProbingProcessStatus; }
            set
            {
                if (value != _ProbingProcessStatus)
                {
                    _ProbingProcessStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EnumProbingState ProbingStateEnum
        {
            get
            {
                return ProbingModuleState.GetState();
            }
        }

        private EnumProbingState _PreProbingStateEnum;
        public EnumProbingState PreProbingStateEnum
        {
            get { return _PreProbingStateEnum; }
            set
            {
                if (value != _PreProbingStateEnum)
                {
                    _PreProbingStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        private MachineIndex _ProbingLastMIndex = new MachineIndex();
        public MachineIndex ProbingLastMIndex
        {
            get { return _ProbingLastMIndex; }
            set
            {
                if (value != _ProbingLastMIndex)
                {
                    _ProbingLastMIndex = value;

                    if (_ProbingLastMIndex != null)
                    {
                        ProbingMXIndex = (int)_ProbingLastMIndex.XIndex;
                        ProbingMYIndex = (int)_ProbingLastMIndex.YIndex;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private int _ProbingMXIndex;
        public int ProbingMXIndex
        {
            get { return _ProbingMXIndex; }
            set
            {
                if (value != _ProbingMXIndex)
                {
                    _ProbingMXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProbingMYIndex;
        public int ProbingMYIndex
        {
            get { return _ProbingMYIndex; }
            set
            {
                if (value != _ProbingMYIndex)
                {
                    _ProbingMYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public bool IsFirstContacted = false;
        private bool _IsFirstSequence = false;
        public bool IsFirstZupSequence
        {
            get { return _IsFirstSequence; }
            set
            {
                if (value != _IsFirstSequence)
                {
                    _IsFirstSequence = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DateTime ZupStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// For ViewModel.
        /// </summary>
        public void SetProbingMachineIndexToCenter()
        {
            try
            {
                ProbingMXIndex = this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2;
                ProbingMYIndex = this.StageSupervisor().WaferObject.GetPhysInfo().MapCountY.Value / 2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private double _FirstContactHeight;
        public double FirstContactHeight
        {
            get { return _FirstContactHeight; }
            set
            {
                if (value != _FirstContactHeight)
                {
                    _FirstContactHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AllContactHeight;
        public double AllContactHeight
        {
            get { return _AllContactHeight; }
            set
            {
                if (value != _AllContactHeight)
                {
                    _AllContactHeight = value;
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

        public DateTime startTime { get; set; } = DateTime.Now;

        Stopwatch sw = new Stopwatch();
        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;

            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                retVal = InnerState.Execute();

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

        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }
        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (state != null)
                {
                    ProbingState probingState = state as ProbingState;

                    if (ProbingModuleState.GetState() != probingState.GetState())
                    {
                        PreInnerState = InnerState;
                        var preProbingState = (PreInnerState as ProbingState);
                        PreProbingStateEnum = preProbingState.GetState();
                        InnerState = state;

                        this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.PROBINGSTATE, ProbingStateEnum.ToString());

                        LoggerManager.Debug($"[{GetType().Name}].ProbingStateTransition() : Pre state = {(PreInnerState as ProbingState).GetState()}({PreInnerState.GetModuleState()}), Now State = {(InnerState as ProbingState).GetState()}({InnerState.GetModuleState()})");

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
        public void ProbingRestart()
        {
            try
            {
                CommandRecvSlot.ClearToken();

                this.ProbingSequenceModule().ResetProbingSequence();

                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.StageSupervisor().WaferObject.ResetWaferData();

                InnerStateTransition(new ProbingIdleState(this));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ProbingSequenceTransfer(int moveIdx)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //CommandRecvSlot.ClearToken();
                MachineIndex MI = new MachineIndex();
                var seqModule = this.ProbingSequenceModule();
                int idx = seqModule.ProbingSequenceCount - (seqModule.ProbingSequenceRemainCount + moveIdx);
                if (seqModule.ProbingSeqParameter.ProbingSeq.Value.Count <= idx)
                {
                    idx = seqModule.ProbingSeqParameter.ProbingSeq.Value.Count - 1;
                    (seqModule as ProbingSequenceModule).ProbingSequenceRemainCount = 0;
                }
                else if (idx <= 0)
                {
                    idx = 0;
                    (seqModule as ProbingSequenceModule).ProbingSequenceRemainCount = seqModule.ProbingSequenceCount;
                }
                else
                {
                    (seqModule as ProbingSequenceModule).ProbingSequenceRemainCount += moveIdx;
                }

                MI = seqModule.ProbingSeqParameter.ProbingSeq.Value[idx];
                ProbingLastMIndex = MI;
                GetUnderDutDices(MI);
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum ProbingSequenceTransfer(MachineIndex curMI, int remainSeqCnt)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //CommandRecvSlot.ClearToken();
                MachineIndex MI = new MachineIndex();
                var seqModule = this.ProbingSequenceModule();
                (seqModule as ProbingSequenceModule).ProbingSequenceRemainCount = remainSeqCnt;
                ProbingLastMIndex = curMI;
                GetUnderDutDices(curMI);
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.SetProbingEndState(ProbingEndReason.UNDEFINED);

                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    Wafer = (WaferObject)this.StageSupervisor().WaferObject;

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();

                    _ProbingModuleState = new ProbingIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

                    ProbingProcessStatus = new ProbingInfo();

                    GetUserSystemMarkShiftValue();

                    _PinZeroPos = new PinCoordinate(403, 3655, -7418);

                    _PinZeroPos.X.Value = 403;
                    _PinZeroPos.Y.Value = 3655;
                    _PinZeroPos.Z.Value = -9000;

                    ProbingLastMIndex = new MachineIndex(Wafer.GetPhysInfo().MapCountX.Value / 2, Wafer.GetPhysInfo().MapCountY.Value / 2);

                    if (ProbingModuleSysParamRef.ProbeMarkShift.Value.X.UpperLimit == 0)
                    {
                        ProbingModuleSysParamRef.ProbeMarkShift.Value.X.UpperLimit = 150;
                        ProbingModuleSysParamRef.ProbeMarkShift.Value.X.LowerLimit = -150;
                    }

                    if (ProbingModuleSysParamRef.ProbeMarkShift.Value.Y.UpperLimit == 0)
                    {
                        ProbingModuleSysParamRef.ProbeMarkShift.Value.Y.UpperLimit = 150;
                        ProbingModuleSysParamRef.ProbeMarkShift.Value.Y.LowerLimit = -150;
                    }

                    ProbingModuleDevParamRef.OverDrive.CheckSpecificSetValueAvailableEvent += Overdrive_CheckSetValueAvailable;

                    retval = BinManagerModule.InitModule();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[Probing], InitModule() : BinManagerModule InitModule() Error.");
                    }

                    this.EventManager().RegisterEvent(typeof(CardChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    this.EventManager().RegisterEvent(typeof(StageRecipeReadCompleteEvent).FullName, "ProbeEventSubscibers", EventFired);

                    retval = this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);

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

        public EventCodeEnum Overdrive_CheckSetValueAvailable(string propertypath, IElement element, object val, out string errorlog)//, IModule source = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errmsg = "";

            try
            {
                double intputValue = -9999;
                double.TryParse(val.ToString(), out intputValue);//val is double ? (double)val : -9999;
                if (intputValue > this.ProbingModuleDevParamRef.OverdriveUpperLimit.Value)
                {
                    errmsg += $"Positive SW Limit occurred.\n" +
                             $"Overdrive Limit Value => {this.ProbingModuleDevParamRef.OverdriveUpperLimit.Value}\n" +
                             $"Overdrive Value => {this.ProbingModuleDevParamRef.OverDrive.Value}\n" +
                             $"SetValue: {intputValue}";


                    retVal = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                }
                else if ((intputValue) < this.ProbingModuleDevParamRef.OverdriveLowLimit.Value)//TODO: Backod와 관련된 원래식이 정상인지 확인할것.
                {
                    errmsg += $"Negative SW Limit occurred.\n" +
                             $"Overdrive Limit Value => {this.ProbingModuleDevParamRef.OverdriveUpperLimit.Value}\n" +
                             $"Overdrive Value => {this.ProbingModuleDevParamRef.OverDrive.Value}\n" +
                             $"SetValue: {intputValue}";
                    retVal = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
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
                if (sender is CardChangedEvent)
                {
                    ClearData();
                }
                else if (sender is DeviceChangedEvent)
                {
                    ClearData();
                }
                else if (sender is StageRecipeReadCompleteEvent)
                {
                    ClearData();
                }
                else if (sender is WaferUnloadedEvent)
                {
                    LoggerManager.Debug($"[{GetType().Name}].EventFired() :WaferUnloadedEvent.");
                    ClearUnderDutDevs();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ClearData()
        {
            try
            {
                var sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                sysparam.UserProbeMarkShift.Value.X.Value = 0;
                sysparam.UserProbeMarkShift.Value.Y.Value = 0;
                sysparam.UserProbeMarkShift.Value.Z.Value = 0;
                sysparam.UserProbeMarkShift.Value.T.Value = 0;

                LoggerManager.Debug($"[ProbingModule] EventFired() : Clear Data Done");

                SaveSysParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<IDeviceObject> GetUnderDutList(MachineIndex mCoord)
        {
            List<IDeviceObject> retval = new List<IDeviceObject>();
            var cardinfo = this.GetParam_ProbeCard();

            try
            {
                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                    {
                        object tmp = this.StageSupervisor().ProbeCardInfo.GetRefOffset(dutIndex);

                        retval.Add(Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(
                            x => x.DieIndexM.Equals(mCoord.Add(this.StageSupervisor().ProbeCardInfo.GetRefOffset(dutIndex)))));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetUnderDutDices(MachineIndex mCoord)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            List<IDeviceObject> dev = new List<IDeviceObject>();

            var cardinfo = this.GetParam_ProbeCard();

            try
            {
                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                    {
                        object tmp = cardinfo.GetRefOffset(dutIndex);

                        dev.Add(Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(
                            x => x.DieIndexM.Equals(mCoord.Add(cardinfo.GetRefOffset(dutIndex)))));

                        if (dev[dev.Count() - 1] == null)
                            dev.RemoveAt(dev.Count() - 1);
                        else
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;

                    }
                    if (dev.Count() > 0)
                    {
                        if (ProbingProcessStatus.UnderDutDevs == null)
                        {
                            ProbingProcessStatus.UnderDutDevs = new System.Collections.ObjectModel.ObservableCollection<IDeviceObject>();
                        }

                        System.Collections.ObjectModel.ObservableCollection<IDeviceObject> dutdevs = new ObservableCollection<IDeviceObject>();
                        for (int devIndex = 0; devIndex < dev.Count; devIndex++)
                        {
                            dutdevs.Add(dev[devIndex]);
                        }
                        ProbingProcessStatus.UnderDutDevs = dutdevs;

                        this.LoaderRemoteMediator().GetServiceCallBack()?.SetProbingDevices(this.LoaderController().GetChuckIndex(), ProbingProcessStatus.UnderDutDevs);
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.NODATA;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum ClearUnderDutDevs()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;
            try
            {
                if (ProbingProcessStatus.UnderDutDevs == null)
                {
                    ProbingProcessStatus.UnderDutDevs = new System.Collections.ObjectModel.ObservableCollection<IDeviceObject>();
                }
                this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Clear();
                this.LoaderRemoteMediator().GetServiceCallBack()?.ClearUnderDutDevs(this.LoaderController().GetChuckIndex());
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.NODATA;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsLotHasSuspendedState()
        {
            bool IsSuspendStateExceptForProbing = false;
            try
            {
                int suspendedStateCount = 0;

                suspendedStateCount = this.LotOPModule().RunList.Count(
                    item =>
                    item?.ModuleState.GetState() == ModuleStateEnum.SUSPENDED);

                if (this.ProbingModuleState.GetModuleState() == ModuleStateEnum.SUSPENDED)
                {
                    suspendedStateCount -= 1;
                }

                IsSuspendStateExceptForProbing = 0 < suspendedStateCount ? true : false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return IsSuspendStateExceptForProbing;
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

        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {
                RetVal = ProbingModuleState.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(ProbingModuleSysParam));
                if (((ProbingModuleSysParam)tmpParam).CPC.Value == null)
                {
                    ((ProbingModuleSysParam)tmpParam).CPC.Value = new List<ChuckPlaneCompParameter>();
                    ((ProbingModuleSysParam)tmpParam).CPC.Value.Add(new ChuckPlaneCompParameter(0, 0, 0, 0));
                }
                ((ProbingModuleSysParam)tmpParam).CPC.Value =
                    ((ProbingModuleSysParam)tmpParam).CPC.Value.OrderByDescending(p => p.Position).ToList();
                if (RetVal == EventCodeEnum.NONE)
                {
                    ProbingModuleSysParam_IParam = tmpParam;
                }

                //Element Event 등록
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
                RetVal = this.SaveParameter(ProbingModuleSysParam_IParam);
                //this.LoadSysParameter(); Element에 등록된 이벤트 다 날아가는 원인
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                retval = this.LoadParameter(ref tmpParam, typeof(ProbingModuleDevParam));

                if (retval == EventCodeEnum.NONE)
                {
                    ProbingModuleDevParam_IParam = tmpParam;

                    //TODO: Custom event 구독
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(ProbingModuleDevParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter(IParam param, int idx = -1)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(ProbingModuleDevParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
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

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            bool Sequenceexist = true;
            try
            {
                MachineIndex Mi = new MachineIndex();
                EventCodeEnum retval = this.ProbingSequenceModule().GetFirstSequence(ref Mi);
                if (retval == EventCodeEnum.NONE)
                {
                    msg = "";
                    retval = CheckOverdriveRangeProcessing(ref msg);
                    if (retval == EventCodeEnum.NONE)
                    {
                        Sequenceexist = true;
                        msg = "";
                    }
                    else 
                    {
                        Sequenceexist = false;
                    }
                }
                else
                {
                    Sequenceexist = false;
                    msg = "There is currently no die to test on the wafer";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Sequenceexist;
        }

        public EventCodeEnum ParamValidation()
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

        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            return retVal;
        }

        public double GetTwistValue() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.TwistForProbing.Value ?? 0;
        public double GetSquarenceValue() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.SquarenessForProbing.Value ?? 0;
        public double GetDeflectX() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.DeflectX.Value ?? 0;
        public double GetDeflectY() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.DeflectY.Value ?? 0;
        public double GetInclineZHor() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.InclineZHor.Value ?? 0;
        public double GetInclineZVer() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.InclineZVer.Value ?? 0;
        public OverDriveStartPositionType GetOverDriveStartPosition() => (ProbingModuleSysParam_IParam as ProbingModuleSysParam)?.OverDriveStartPosition.Value ?? 0;
        public CatCoordinates GetPMShifhtValue()
        {
            try
            {
                var sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                CatCoordinates retCoord = new CatCoordinates();
                retCoord.X.Value = sysparam.ProbeMarkShift.Value.X.Value + sysparam.UserProbeMarkShift.Value.X.Value;
                retCoord.Y.Value = sysparam.ProbeMarkShift.Value.Y.Value + sysparam.UserProbeMarkShift.Value.Y.Value;
                retCoord.Z.Value = sysparam.ProbeMarkShift.Value.Z.Value + sysparam.UserProbeMarkShift.Value.Z.Value;
                retCoord.T.Value = sysparam.ProbeMarkShift.Value.T.Value + sysparam.UserProbeMarkShift.Value.T.Value;

                LoggerManager.Debug($"[ProbingModule] GetPMShifhtValue() : System PMShift (X,Y) = ({sysparam.ProbeMarkShift.Value.X.Value},{sysparam.ProbeMarkShift.Value.Y.Value})" +
                        $"User PMShift (X,Y) = ({sysparam.UserProbeMarkShift.Value.X.Value}, {sysparam.UserProbeMarkShift.Value.Y.Value})", isInfo: true);
                return retCoord;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable()
        {
            Dictionary<double, CatCoordinates> retval = new Dictionary<double, CatCoordinates>();
            try
            {
                retval = (ProbingModuleSysParam_IParam as ProbingModuleSysParam).ProbeTemperaturePositionTable.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public CatCoordinates GetSetTemperaturePMShifhtValue()
        {
            CatCoordinates retval = new CatCoordinates();

            try
            {
                double targetTemp = this.TempController().TempInfo.SetTemp.Value;
                double deviationvalue = this.TempController().GetDeviaitionValue();
                double near = 0;
                double min = double.MaxValue;
                double diff = 0;

                ProbingModuleSysParam probingsysparam = (ProbingModuleSysParam_IParam as ProbingModuleSysParam);

                if (probingsysparam.ProbeTemperaturePositionTable == null || probingsysparam.ProbeTemperaturePositionTable.Value == null)
                {
                    probingsysparam.ProbeTemperaturePositionTable = new Element<Dictionary<double, CatCoordinates>>();
                    probingsysparam.ProbeTemperaturePositionTable.Value = new Dictionary<double, CatCoordinates>();
                }

                foreach (var key in probingsysparam.ProbeTemperaturePositionTable.Value.Keys)
                {
                    if (key > targetTemp)
                    {
                        diff = key - targetTemp;
                    }
                    else
                    {
                        diff = targetTemp - key;
                    }
                    if (min > diff)
                    {
                        min = diff;
                        near = key;
                    }
                }


                if (probingsysparam.ProbeTemperaturePositionTable.Value.Count > 0)
                {
                    bool returvalue = probingsysparam.ProbeTemperaturePositionTable.Value.TryGetValue(near, out retval);

                    if (returvalue == true)
                    {
                        if (targetTemp - deviationvalue <= near &&
                                targetTemp + deviationvalue >= near)
                        {
                            LoggerManager.Debug($"GetSetTemperaturePMShifhtValue() : Used table key value = {near}, TempOffsetX = {retval.X.Value}, TempOffsetY = {retval.Y.Value}, TempOffsetZ = {retval.Z.Value} ,TempOffsetT = {retval.T.Value}");
                        }
                        else
                        {
                            retval = new CatCoordinates();
                            LoggerManager.Debug($"GetSetTemperaturePMShifhtValue() : CurTemp:{targetTemp}, Deviation:{deviationvalue} , NearTemp:{near}. Not used table, TempOffsetX = {retval.X.Value}, TempOffsetY = {retval.Y.Value}, TempOffsetZ = {retval.Z.Value} ,TempOffsetT = {retval.T.Value}");
                        }
                    }
                    else
                    {
                        if (retval == null)
                        {
                            retval = new CatCoordinates();
                        }
                        else
                        {

                        }
                        LoggerManager.Debug($"GetSetTemperaturePMShifhtValue() : Not used table, TempOffsetX = {retval.X.Value}, TempOffsetY = {retval.Y.Value}, TempOffsetZ = {retval.Z.Value},TempOffsetT = {retval.T.Value}");
                    }

                }
                else
                {
                    LoggerManager.Debug($"GetSetTemperaturePMShifhtValue() : Not exist the table data.");
                }
            }
            catch (Exception err)
            {
                if (retval == null)
                {
                    retval = new CatCoordinates();
                }

                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum CheckOverdriveRangeProcessing(ref string errorReasonStr)
        {
            EventCodeEnum retIsOverdriveRange = EventCodeEnum.NONE;
            try
            {

                if (ProbingModuleSysParamRef.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                {
                    if (ProbingModuleDevParamRef.IsEnableAllContactToOverdriveLimit.Value == true &&
                        ProbingModuleDevParamRef.AllContactToOverdriveLimitRange.Value < this.OverDrive)
                    {
                        retIsOverdriveRange = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                        errorReasonStr += $"AllContactToOverdriveLimit is Enable state.\n" +
                            $"Range : {ProbingModuleDevParamRef.AllContactToOverdriveLimitRange.Value}\n" +
                            $"OverDrive : {this.OverDrive}";
                    }
                }
                else if (ProbingModuleSysParamRef.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    if (ProbingModuleDevParamRef.IsEnableFirstContactToOverdriveLimit.Value == true &&
                        ProbingModuleDevParamRef.FirstContactToOverdriveLimitRange.Value < this.OverDrive)
                    {
                        retIsOverdriveRange = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                        errorReasonStr += $"FirstContactToOverdriveLimit is Enable state.\n" +
                            $"Range : {ProbingModuleDevParamRef.FirstContactToOverdriveLimitRange.Value}\n" +
                            $"OverDrive : {this.OverDrive}";
                    }
                    if (ProbingModuleDevParamRef.IsEnableAllContactToOverdriveLimit.Value == true &&
                        ProbingModuleDevParamRef.AllContactToOverdriveLimitRange.Value < this.OverDrive)
                    {
                        retIsOverdriveRange = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                        errorReasonStr += $"AllContactToOverdriveLimit is Enable state.\n" +
                            $"Range : {ProbingModuleDevParamRef.AllContactToOverdriveLimitRange.Value}\n" +
                            $"OverDrive : {this.OverDrive}";
                    }
                }
                else
                {
                    retIsOverdriveRange = EventCodeEnum.NONE;
                }

                if (ProbingModuleDevParamRef.OverDrive.Value > ProbingModuleDevParamRef.OverdriveUpperLimit.Value)
                {
                    retIsOverdriveRange = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                    errorReasonStr += $"Probing Overdrive Upper Limit Error occurred.\n" +
             $"Overdrive Upper Limit Value => {ProbingModuleDevParamRef.OverdriveUpperLimit.Value}\n" +
             $"Overdrive Value => {ProbingModuleDevParamRef.OverDrive.Value}";

                }


                if (ProbingModuleDevParamRef.OverDrive.Value < ProbingModuleDevParamRef.OverdriveLowLimit.Value)
                {
                    if (EnumModuleForcedState.ForcedDone.Equals(this.LoaderController().GetModuleForcedState(ModuleEnum.PinAlign)) ||
                EnumModuleForcedState.ForcedDone.Equals(this.LoaderController().GetModuleForcedState(ModuleEnum.WaferAlign)))
                    {
                    }
                    else
                    {
                        retIsOverdriveRange = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                        errorReasonStr += $"Probing Overdrive Low Limit Error occurred.\n" +
                 $"Overdrive Low Limit Value => {ProbingModuleDevParamRef.OverdriveLowLimit.Value}\n" +
                 $"Overdrive Value => {ProbingModuleDevParamRef.OverDrive.Value}";
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retIsOverdriveRange;
        }
        public MarkShiftValues GetUserSystemMarkShiftValue()
        {
            ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
            MarkShiftValues result = new MarkShiftValues();
            try
            {
                if (sysparam != null)
                {
                    result.UserProbeMarkXShift = sysparam.UserProbeMarkShift.Value.X.Value;
                    result.UserProbeMarkYShift = sysparam.UserProbeMarkShift.Value.Y.Value;
                    result.SystemMarkXShift = sysparam.ProbeMarkShift.Value.X.Value;
                    result.SystemMarkYShift = sysparam.ProbeMarkShift.Value.Y.Value;
                } 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return result;
        }

        public void SetTwistValue(double value)
        {
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null)
                    sysparam.TwistForProbing.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetSquarenceValue(double value)
        {
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null)
                    sysparam.SquarenessForProbing.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetDeflectX(double value)
        {
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null)
                    sysparam.DeflectX.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetDeflectY(double value)
        {
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null)
                    sysparam.DeflectY.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetInclineZHor(double value)
        {
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null)
                    sysparam.InclineZHor.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetInclineZVer(double value)
        {
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam != null)
                    sysparam.InclineZVer.Value = value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetProbeShiftValue(CatCoordinates shiftvalue)
        {
            ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
            if (sysparam != null)
            {
                try
                {
                    sysparam.ProbeMarkShift.Value.X.Value = shiftvalue.X.Value;
                    sysparam.ProbeMarkShift.Value.Y.Value = shiftvalue.Y.Value;
                    sysparam.ProbeMarkShift.Value.Z.Value = shiftvalue.Z.Value;
                    sysparam.ProbeMarkShift.Value.T.Value = shiftvalue.T.Value;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }

        public void SetProbeTempShiftValue(CatCoordinates shiftvalue)
        {
            ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
            double targetTemp = this.TempController().TempInfo.SetTemp.Value;
            if (sysparam != null)
            {
                try
                {
                    if (sysparam.ProbeTemperaturePositionTable == null)
                    {
                        sysparam.ProbeTemperaturePositionTable = new Element<Dictionary<double, CatCoordinates>>();
                    }
                    if (sysparam.ProbeTemperaturePositionTable.Value == null)
                    {
                        sysparam.ProbeTemperaturePositionTable.Value = new Dictionary<double, CatCoordinates>();
                    }
                    bool iscontain = sysparam.ProbeTemperaturePositionTable.Value.ContainsKey(targetTemp);
                    if (iscontain == true)
                    {
                        foreach (var key in sysparam.ProbeTemperaturePositionTable.Value.Keys)
                        {
                            if (key == targetTemp)
                            {
                                sysparam.ProbeTemperaturePositionTable.Value.Remove(key);
                                if (shiftvalue != null)
                                {
                                    sysparam.ProbeTemperaturePositionTable.Value.Add(targetTemp, shiftvalue);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        sysparam.ProbeTemperaturePositionTable.Value.Add(targetTemp, shiftvalue);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public double CalculateZClearenceUsingOD(double overDrive, double zClearence)
        {
            double retZClearence = zClearence;
            try
            {

                if (overDrive < 0 && overDrive < zClearence)
                {
                    retZClearence = overDrive + zClearence;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retZClearence;
        }

        public ObservableCollection<IChuckPlaneCompParameter> GetCPCValues()
        {
            ObservableCollection<IChuckPlaneCompParameter> CPCValues = new ObservableCollection<IChuckPlaneCompParameter>();
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                foreach (var param in sysparam.CPC.Value)
                {
                    CPCValues.Add(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CPCValues;
        }

        public void AddCPCParameter(IChuckPlaneCompParameter cpcparam)
        {
            ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
            sysparam.CPC.Value.Add((ChuckPlaneCompParameter)cpcparam);
        }
        public void GetCPCValues(double position, out double z0, out double z1, out double z2)
        {
            z0 = 0.0;
            z1 = 0.0;
            z2 = 0.0;
            try
            {
                ProbingModuleSysParam sysparam = ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                if (sysparam.CPC.Value != null)
                {
                    var cpc = sysparam.CPC.Value;
                    if (sysparam.CPC.Value.Count > 0)
                    {
                        var closest = sysparam.CPC.Value.OrderBy(v => Math.Abs((double)v.Position - position)).First();
                        int indexOfClosest = sysparam.CPC.Value.IndexOf(closest);
                        if (sysparam.CPC.Value.Count == 1 | indexOfClosest + 1 >= sysparam.CPC.Value.Count)
                        {
                            z0 = sysparam.CPC.Value[indexOfClosest].Z0;
                            z1 = sysparam.CPC.Value[indexOfClosest].Z1;
                            z2 = sysparam.CPC.Value[indexOfClosest].Z2;
                        }
                        else
                        {
                            var prevIndex = indexOfClosest + 1;
                            var prevPos = sysparam.CPC.Value[prevIndex].Position;
                            var closestPos = sysparam.CPC.Value[indexOfClosest].Position;
                            var posDiff = closestPos - prevPos;
                            var ratio = (position - prevPos) / posDiff;
                            z0 = sysparam.CPC.Value[indexOfClosest].Z0 - (sysparam.CPC.Value[indexOfClosest].Z0 - sysparam.CPC.Value[prevIndex].Z0) * ratio;
                            z1 = sysparam.CPC.Value[indexOfClosest].Z1 - (sysparam.CPC.Value[indexOfClosest].Z1 - sysparam.CPC.Value[prevIndex].Z1) * ratio;
                            z2 = sysparam.CPC.Value[indexOfClosest].Z2 - (sysparam.CPC.Value[indexOfClosest].Z2 - sysparam.CPC.Value[prevIndex].Z2) * ratio;
                        }
                    }
                }
                else
                {
                    sysparam.CPC.Value = new List<ChuckPlaneCompParameter>();
                    sysparam.CPC.Value.Add(new ChuckPlaneCompParameter(0, 0, 0, 0));
                }

                if (sysparam.CPC.UpperLimit == 0) { sysparam.CPC.UpperLimit = 100; }
                if (sysparam.CPC.LowerLimit == 0) { sysparam.CPC.LowerLimit = -100; }
                z0 = Math.Round(z0, 2);
                z1 = Math.Round(z1, 2);
                z2 = Math.Round(z2, 2);

                if (z0 > sysparam.CPC.UpperLimit)
                {
                    z0 = sysparam.CPC.UpperLimit;
                }
                else if (z0 < sysparam.CPC.LowerLimit)
                {
                    z0 = sysparam.CPC.LowerLimit;
                }
                if (z1 > sysparam.CPC.UpperLimit)
                {
                    z1 = sysparam.CPC.UpperLimit;
                }
                else if (z1 < sysparam.CPC.LowerLimit)
                {
                    z1 = sysparam.CPC.LowerLimit;
                }
                if (z2 > sysparam.CPC.UpperLimit)
                {
                    z2 = sysparam.CPC.UpperLimit;
                }
                else if (z2 < sysparam.CPC.LowerLimit)
                {
                    z2 = sysparam.CPC.LowerLimit;
                }
                LoggerManager.Debug($"CPC: Z0 = {z0:0.0}, Z1 = {z1:0.0}, Z2 = {z2:0.0}", isInfo: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        //public IBinDeviceParam GetBinDevParam()
        //{
        //    try
        //    {
        //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //        //retVal = this.BinManagerModule.UpdateBinDevParam();
        //        SortedBinInfos.Clear();

        //        for (int i = 0; i < this.BinManagerModule.BindevParam.BinInfos.Value.Count; i++)
        //        {
        //            if (this.BinManagerModule.BindevParam.BinInfos.Value[i].wInstantReprobeForCP1.Value == EnableEnum.Enable)
        //            {
        //                SortedBinInfos.Add(i, this.BinManagerModule.BindevParam.BinInfos.Value[i].wInstantReprobeForCP1.Value);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return this.BinManagerModule.BindevParam;
        //}

        public List<IBINInfo> GetBinInfos()
        {
            List<IBINInfo> retval = null;

            try
            {
                retval = BinManagerModule.GetBinInfos();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetBinInfos(List<IBINInfo> binInfos)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = BinManagerModule.SetBinInfos(binInfos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum SaveBinDevParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = BinManagerModule.SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ResetOnWaferInformation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                foreach (var die in substrateInfo.DIEs)
                {
                    if (die.TestHistory == null)
                    {
                        die.TestHistory = new List<TestHistory>();
                    }

                    die.TestHistory.Clear();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public bool IsTestedDIE(long xindex, long yindex)
        {
            bool retval = false;

            try
            {
                LotModeEnum lotModeEnum = this.LotOPModule()?.LotInfo?.LotMode.Value ?? LotModeEnum.UNDEFINED;
                ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                var die = substrateInfo.DIEs[xindex, yindex];

                if (lotModeEnum == LotModeEnum.MPP)
                {
                    var param = this.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    if (param.Retest_MPP.Enable.Value == true)
                    {
                        if (param.Retest_MPP.Mode.Value == ReTestMode.BYBIN)
                        {
                            if (die.NeedTest == true)
                            {
                                retval = true;
                            }
                            else
                            {
                                // TODO : 
                                retval = false;
                            }
                        }
                        else
                        {
                            // TODO : 
                            retval = false;
                        }
                    }
                    else
                    {
                        // TODO : 
                        retval = false;
                    }
                }
                else
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retval;
        }
        public bool NeedRetestbyBIN(RetestTypeEnum type, long xindex, long yindex)
        {
            bool retval = false;

            try
            {
                var dies = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs;

                var dutlist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;
                int dutCount = dutlist.Count;

                int needRetestDieCnt = 0;

                for (int i = 0; i < dutCount; i++)
                {
                    long dutXIndex = xindex + dutlist[i].UserIndex.XIndex;
                    long dutYIndex = yindex + dutlist[i].UserIndex.YIndex;

                    bool isDutIsInRange = DutIsInRange(dutXIndex, dutYIndex);

                    if (isDutIsInRange)
                    {
                        var die = dies[dutXIndex, dutYIndex];

                        var IsEnable = GetRetestEnable(die, type);

                        if (IsEnable == true)
                        {
                            // TODO : 

                            die.NeedTest = true;

                            needRetestDieCnt++;
                        }
                        else
                        {
                            die.NeedTest = false;
                        }
                    }
                }

                if (needRetestDieCnt > 0)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private bool GetRetestEnable(IDeviceObject die, RetestTypeEnum type)
        {
            bool retval = false;

            try
            {
                int bincode = die.CurTestHistory.BinCode.Value;
                List<IBINInfo> bINInfos = BinManagerModule.GetBinInfos();
                IBINInfo bINInfo = null;

                if (bINInfos != null && bINInfos.Count > 0)
                {
                    bINInfo = bINInfos.FirstOrDefault(x => x.BinCode.Value == bincode);
                }

                if (bINInfo != null)
                {
                    switch (type)
                    {
                        case RetestTypeEnum.RETESTFORCP1:
                            retval = bINInfo.RetestForCP1Enable.Value;
                            break;
                        case RetestTypeEnum.RETESTFORCP2:
                            retval = bINInfo.RetestForCP2Enable.Value;
                            break;
                        case RetestTypeEnum.RRETESTFORNTH:
                            retval = bINInfo.RretestForNthEnable.Value;
                            break;
                        case RetestTypeEnum.INSTANTRETESTFORCP1:
                            retval = bINInfo.InstantRetestForCP1Enable.Value;
                            break;
                        case RetestTypeEnum.INSTANTRETESTFORRETEST:
                            retval = bINInfo.InstantRetestForRetestEnable.Value;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool NeedRetest(long xindex, long yindex)
        {
            bool retval = false;

            try
            {
                var dies = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs;

                var dutlist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;
                int dutCount = dutlist.Count;

                int needRetestDieCnt = 0;

                for (int i = 0; i < dutCount; i++)
                {
                    long dutXIndex = xindex + dutlist[i].UserIndex.XIndex;
                    long dutYIndex = yindex + dutlist[i].UserIndex.YIndex;

                    bool isDutIsInRange = DutIsInRange(dutXIndex, dutYIndex);

                    if (isDutIsInRange)
                    {
                        var die = dies[dutXIndex, dutYIndex];

                        DieStateEnum dieStateEnum = die.State.Value;
                        TestState testState = die.CurTestHistory.TestResult.Value;

                        if (dieStateEnum == DieStateEnum.TESTED && testState == TestState.MAP_STS_FAIL)
                        {
                            needRetestDieCnt++;
                        }
                    }
                }

                if (needRetestDieCnt > 0)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        //private List<IBINInfo> GetBinInfos()
        //{
        //    List<IBINInfo> retval = null;

        //    try
        //    {
        //        retval = BinManagerModule.GetBinInfos();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public void SetProbingUnderdut(ObservableCollection<IDeviceObject> UnderDutDevs)
        {
            ProbingProcessStatus.UnderDutDevs = UnderDutDevs;
        }

        #region // Force Measurement Properties


        private AxisObject _ZAxis;
        public AxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _Z0Axis;
        public AxisObject Z0Axis
        {
            get { return _Z0Axis; }
            set
            {
                if (value != _Z0Axis)
                {
                    _Z0Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _Z1Axis;
        public AxisObject Z1Axis
        {
            get { return _Z1Axis; }
            set
            {
                if (value != _Z1Axis)
                {
                    _Z1Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _Z2Axis;
        public AxisObject Z2Axis
        {
            get { return _Z2Axis; }
            set
            {
                if (value != _Z2Axis)
                {
                    _Z2Axis = value;
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
        private double _ForceValue;
        public double ForceValue
        {
            get { return _ForceValue; }
            set
            {
                if (value != _ForceValue)
                {
                    _ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Z0ForceValue;
        public double Z0ForceValue
        {
            get { return _Z0ForceValue; }
            set
            {
                if (value != _Z0ForceValue)
                {
                    _Z0ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Z1ForceValue;
        public double Z1ForceValue
        {
            get { return _Z1ForceValue; }
            set
            {
                if (value != _Z1ForceValue)
                {
                    _Z1ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Z2ForceValue;
        public double Z2ForceValue
        {
            get { return _Z2ForceValue; }
            set
            {
                if (value != _Z2ForceValue)
                {
                    _Z2ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ZFOrgRawPos;
        public double ZFOrgRawPos
        {
            get { return _ZFOrgRawPos; }
            set
            {
                if (value != _ZFOrgRawPos)
                {
                    _ZFOrgRawPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion


        public byte[] GetProbingDevParam(int idx = -1)
        {

            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(ProbingModuleDevParam_IParam, typeof(ProbingModuleDevParam));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public byte[] GetBinDevParam()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(BinManagerModule.BinDevParam_IParam, typeof(BinDeviceParam));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }



        //public void SetProbingDevParam(byte[] param)
        //{
        //    // TODO : TEST

        //    object obj = SerializeManager.ByteToObject(param);

        //    if(obj != null)
        //    {
        //        ProbingModuleDevParam_IParam = obj as ProbingModuleDevParam;
        //    }
        //    //string fullPath = this.FileManager().GetDeviceParamFullPath(PolishWaferParameter.FilePath, PolishWaferParameter.FileName);
        //    //Stream stream = new MemoryStream(param);
        //    //this.DecompressFilesFromByteArray(stream, fullPath);
        //    //LoadDevParameter();
        //}

        /// <summary>
        /// TODO: 이함수는 사라져야함.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="idx"></param>
        public void SetProbingDevParam(IParam param, int idx = -1)
        {
            ProbingModuleDevParam_IParam = param;
            this.SaveDevParameter();
            this.LoadDevParameter();  //TODO: 이 타이밍에 Dev Parameter로 등록된 Gem Notify 연결이 끊기게 됨. lock이 필요한것인가? 아니면 ProbingDevParameter로 범위를 축소할 것인가?
            this.GEMModule().RegistNotifyEventToElement(this.ParamManager().DevDBElementDictionary);
        }


        //public void ValueChangedObjects(object prevObject, object postObject)
        //{            
        //    List<IElement> elements = new List<IElement>();
        //    try
        //    {
        //        if (prevObject.GetType() != postObject.GetType())
        //        {
        //            return;
        //        }

        //        PropertyInfo[] properties = prevObject.GetType().GetProperties();//origin element info

        //        foreach (PropertyInfo property in properties)
        //        {
        //            try
        //            {
        //                if (property.GetValue(prevObject) is IElement)
        //                {
        //                    IElement value1 = property.GetValue(prevObject) as IElement;//crushed element info
        //                    IElement value2 = property.GetValue(postObject) as IElement;//origin element info

        //                    if (!value1.GetValue().Equals(value2.GetValue()))
        //                    {
        //                        EventCodeEnum ret = value1.SetValue(value2.GetValue(), isNeedValidation: true);
        //                        LoggerManager.Debug($"ValueChangedObjects(): Try {value2.PropertyPath} SetValue: {value1}, OldValue: {value2}, Result:{ret}");
        //                    }
        //                }
        //                else
        //                {
        //                    object value1 = property.GetValue(prevObject);//crushed element info
        //                    object value2 = property.GetValue(postObject);//origin element info

        //                    if (!value1.Equals(value2))
        //                    {
        //                        value1 = value2;                                
        //                    }
        //                }
        //            }
        //            catch (Exception err)
        //            {
        //                LoggerManager.Debug($"ValueChangedObjects(): {property.Name} Compare Error.");
        //                LoggerManager.Exception(err);
        //            }

        //        }                

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}


        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                EnumProbingState state = (InnerState as ProbingState).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetStagePIVProbingData(ref long XIndex, ref long YIndex, UserIndex userindex, ref string full_site)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                long EdgeDiexIndex = 0;
                long EdgeDieyIndex = 0;

                if (ProbingModuleSysParamRef.ProbingCoordCalcTypeEnum.Value == ProbingCoordCalcType.STANDARDEDGE)
                {

                    var dies = this.GetParam_Wafer().DevicesConvertByteArray();
                    long xIndex = 0;
                    long yIndex = 0;
                    long verdiecount = Wafer.GetPhysInfo().MapCountY.Value;
                    long hordiecount = Wafer.GetPhysInfo().MapCountX.Value;
                    bool breakflag = true;

                    yIndex = verdiecount - 1;
                    while (breakflag)
                    {
                        for (xIndex = hordiecount - 1; xIndex > 0; xIndex--)
                        {
                            if (dies[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                            {
                                EdgeDieyIndex = yIndex;
                                breakflag = false;
                                break;
                            }

                        }
                        yIndex--;
                    }

                    breakflag = true;
                    xIndex = hordiecount - 1;
                    while (breakflag)
                    {
                        for (yIndex = verdiecount - 1; yIndex > 0; yIndex--)
                        {
                            if (dies[xIndex, yIndex] == (int)DieTypeEnum.TEST_DIE)
                            {
                                EdgeDiexIndex = xIndex;
                                breakflag = false;
                                break;
                            }

                        }
                        xIndex--;
                    }
                }
                    full_site = this.GPIB()?.GetFullSite(XIndex, YIndex);
                    if (ProbingModuleSysParamRef.ProbingCoordCalcTypeEnum.Value == ProbingCoordCalcType.STANDARDEDGE)
                    {
                        XIndex = (EdgeDiexIndex - XIndex) * -1;
                        YIndex = EdgeDieyIndex - YIndex;
                    }
                    else if (ProbingModuleSysParamRef.ProbingCoordCalcTypeEnum.Value == ProbingCoordCalcType.USERCOORD)
                    {
                        XIndex = userindex.XIndex;
                        YIndex = userindex.YIndex;
                    }
                    else if(ProbingModuleSysParamRef.ProbingCoordCalcTypeEnum.Value == ProbingCoordCalcType.USERCOORDINDUT)
                    {
                        XIndex = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList[0].UserIndex.XIndex;
                        YIndex = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList[0].UserIndex.YIndex;
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}].GetStagePIVProbingCoord() : PIVProbingCoord X = {XIndex}, PIVProbingCoord Y = {YIndex}, fullSite = {full_site}", isInfo: true);

                

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetLastProbingMIndex(MachineIndex index)
        {
            try
            {
                if (index != null)
                {
                    this.ProbingLastMIndex = index;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public bool DutIsInRange(long xindex, long yindex)
        {
            bool isDutIsInRange = false;

            try
            {
                ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();

                int dieXlength = substrateInfo.DIEs.GetLength(0);
                int dieYlength = substrateInfo.DIEs.GetLength(1);

                if ((xindex < dieXlength) && (xindex >= 0) &&
                    (yindex < dieYlength) && (yindex >= 0)
                    )
                {
                    isDutIsInRange = true;
                }
                else
                {
                    isDutIsInRange = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return isDutIsInRange;
        }

        public bool GetEnablePTPAEnhancer()
        {
            return ProbingModuleSysParamRef.EnablePTPAEnhancer.Value;
        }

        /// <summary>
        /// BeforeZupSoaking 정보를 반환한다(옵션에 따라 Status soaking 또는 기존 ContactSetting쪽 정보를 반환)
        /// </summary>
        /// <param name="UseStatusSoaking"> Status Soaking의 사용여부 반환 </param>
        /// <param name="IsEnableBeforeZupSoaking"> Before Z up 사용 여부 </param>
        /// <param name="BeforeZupSoakingTime">Zup soaking Time(sec)</param>
        /// <param name="BeforeZup_ZClearance">BeforeZup ZClearance</param>
        public void GetBeforeZupSoakOption(ref bool UseStatusSoaking, ref bool IsEnableBeforeZupSoaking, ref int BeforeZupSoakingTime, ref double BeforeZup_ZClearance)
        {
            this.SoakingModule().GetBeforeZupSoak_SettingInfo(out UseStatusSoaking, out IsEnableBeforeZupSoaking, out BeforeZupSoakingTime, out BeforeZup_ZClearance);
            if (false == UseStatusSoaking) //기존 before z up soak parameter 사용
            {
                var devparam = (this.ProbingModuleDevParam_IParam as ProbingModuleDevParam);
                if (devparam.BeforeZupSoakingEnable?.Value == true)
                    IsEnableBeforeZupSoaking = true;
                else
                    IsEnableBeforeZupSoaking = false;

                BeforeZupSoakingTime = devparam.BeforeZupSoakingTime.Value;
                BeforeZup_ZClearance = devparam.BeforeZupSoakingClearanceZ.Value;
                LoggerManager.SoakingLog($"Old Event Soak - Before Zup Soaking.(Enable:{IsEnableBeforeZupSoaking}, SoakingTime:{BeforeZupSoakingTime} sec,  ZClearance:{BeforeZup_ZClearance})");
            }
            else
            {
                LoggerManager.SoakingLog($"Status Soak - Before Zup Soaking.(Enable:{IsEnableBeforeZupSoaking}, SoakingTime:{BeforeZupSoakingTime} sec,  ZClearance:{BeforeZup_ZClearance})");
            }
        }

        public void SetProbingEndState(ProbingEndReason probingEndReason, EnumWaferState WaferState = EnumWaferState.UNDEFINED)
        {
            try
            {
                if (probingEndReason != ProbingEndReason.UNDEFINED)
                {
                    var waferstate = WaferState;
                    if (WaferState == EnumWaferState.UNDEFINED)
                    {
                        waferstate = this.GetParam_Wafer().GetState();
                    }

                    if (waferstate == EnumWaferState.PROBING && (probingEndReason == ProbingEndReason.OTHER_REJECT || probingEndReason == ProbingEndReason.MANUAL_LOT_END) &&
                    (this.ProbingStateEnum != EnumProbingState.ZUP || this.ProbingStateEnum != EnumProbingState.ZUPDWELL ||
                    this.ProbingStateEnum != EnumProbingState.ZUPPERFORM ||
                    this.ProbingStateEnum != EnumProbingState.PINPADMATCHED || this.ProbingStateEnum != EnumProbingState.PINPADMATCHPERFORM))
                    {
                        LoggerManager.Debug($"[ProbingModule] Failed to change ProbingEndReason to {probingEndReason}, Cur {ProbingEndReason}. Unable to set reason during probing.");
                    }
                    else
                    {
                        if (ProbingEndReason == ProbingEndReason.UNDEFINED &&
                            this.GetParam_Wafer().GetStatus() == EnumSubsStatus.EXIST &&
                            this.GetParam_Wafer().GetWaferType() == EnumWaferType.STANDARD)
                        {
                            ProbingEndReason = probingEndReason;
                            LoggerManager.Debug($"[ProbingModule] ProbingEndReason set to {probingEndReason}, Wafer state {waferstate}");

                            var physicalInfo = this.GetParam_Wafer().GetPhysInfo();
                            var substrateInfo = this.GetParam_Wafer().GetSubsInfo();
                            var pivinfo = new PIVInfo(
                                           foupnumber: this.GetParam_Wafer().GetOriginFoupNumber(),
                                           notchangle: (int)physicalInfo.NotchAngle.Value,
                                           waferendrst: 0,
                                           totalDieCount: substrateInfo.TestedDieCount.Value,
                                           passDieCount: substrateInfo.PassedDieCount.Value,
                                           faildiecount: substrateInfo.FailedDieCount.Value,
                                           yieldbaddie: 0,
                                           waferStartTime: substrateInfo.DIEs[0, 0].CurTestHistory.StartTime.Value.ToString("yyyyMMddHHmmss"),
                                           waferEndTime: substrateInfo.DIEs[0, 0].CurTestHistory.EndTime.Value.ToString("yyyyMMddHHmmss"),
                                           pcardcontactcount: this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.TouchdownCount.Value,
                                           curtemperature: this.TempController().TempInfo.CurTemp.Value,
                                           settemperature: this.TempController().TempInfo.SetTemp.Value,
                                           od: (int)OverDrive);

                            //case EnumWaferTestState.TEST_END:
                            switch (probingEndReason)
                            {
                                case ProbingEndReason.UNDEFINED:
                                    break;
                                case ProbingEndReason.NORMAL:
                                    #region ==> ProbingEndReason.NOMAL
                                    this.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.TESTED);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    if (this.ProbingStateEnum == EnumProbingState.ZUP || this.ProbingStateEnum == EnumProbingState.ZUPDWELL)
                                    {
                                        this.EventManager().RaisingEvent(typeof(WaferTestEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                        semaphore.Wait();
                                    }
                                    #endregion
                                    break;
                                case ProbingEndReason.YIELD_NG:
                                case ProbingEndReason.CONTINUOUS_FAIL_NG:
                                    break;
                                case ProbingEndReason.ERROR_NG:
                                    #region ==> ProbingEndReason.ERROR_NG
                                    semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(WaferTestingCanceledCanceledByHostEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    #endregion
                                    break;
                                case ProbingEndReason.MANUAL_LOT_END:
                                case ProbingEndReason.MANUAL_UNLOAD:
                                case ProbingEndReason.OTHER_REJECT:
                                    #region ==> MANUAL_LOT_ENDL || MANUAL_UNLOAD || OTHER_REJECT
                                    if (waferstate == EnumWaferState.UNPROCESSED)
                                    {
                                        semaphore = new SemaphoreSlim(0);
                                        this.EventManager().RaisingEvent(typeof(WaferCancelledBeforeProbing).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                        semaphore.Wait();
                                    }
                                    else
                                    {
                                        semaphore = new SemaphoreSlim(0);
                                        this.EventManager().RaisingEvent(typeof(WaferTestingAborted).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                        semaphore.Wait();
                                    }
                                    break;
                                #endregion
                                case ProbingEndReason.SEQUENCE_INVALID_ERROR:
                                    #region ==> MANUAL_LOT_ENDL || MANUAL_UNLOAD || OTHER_REJECT
                                    this.GetParam_Wafer().SetWaferState(EnumWaferState.SKIPPED);
                                    semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(WaferTestingAborted).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    #endregion
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[ProbingModule] Failed to change ProbingEndReason to {probingEndReason}, Cur {ProbingEndReason}");
                        }
                    }
                }
                else
                {
                    ProbingEndReason = probingEndReason;
                    LoggerManager.Debug($"[ProbingModule] ProbingEndReason set to {probingEndReason}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetZupStartTime(bool isreset = false)
        {
            try
            {
                if(isreset == false)
                {
                    ZupStartTime = DateTime.Now.ToLocalTime();
                }
                else
                {
                    ZupStartTime = new DateTime();
                }

                LoggerManager.Debug($"[ProbingModule] SetZupStartTime() : {ZupStartTime}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public DateTime GetZupStartTime()
        {
            return ZupStartTime;
        }
        public void FinalizeWaferProcessing()
        {
            try
            {
                var waferstate = this.GetParam_Wafer().GetState();
                if (waferstate == EnumWaferState.TESTED)
                {
                    this.GetParam_Wafer().SetWaferState(EnumWaferState.PROCESSED);
                    DateTime endTime = DateTime.Now.ToLocalTime();
                    LoggerManager.Debug($"[ProbingModule] FinalizeWaferProcessing() : endTime:{endTime}");

                    if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                    {
                        this.LotOPModule().LotInfo.ProcessedWaferCnt++;

                        this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.WAFERCOUNT, this.LotOPModule().LotInfo.ProcessedWaferCnt.ToString());

                        this.LotOPModule().SystemInfo.IncreaseWaferCount();
                        this.LotOPModule().SystemInfo.IncreaseProcessedWaferCountUntilBeforeCardChange();

                        (this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).ProbingEndTime = endTime;
                        this.LotOPModule().LotInfo.UpdateWafer(this.StageSupervisor().WaferObject);
                    }
                    
                    var pivinfo = new PIVInfo() { FoupNumber = this.GetParam_Wafer().GetOriginFoupNumber() };
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(WaferEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
