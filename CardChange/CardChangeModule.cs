using Autofac;
using LoaderController.GPController;
using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Command;
using ProberInterfaces.Event;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.State;
using ProberInterfaces.Vision;
using ProberInterfaces.Wizard;
using SequenceRunner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
//using TimeServices;

using SequenceService;

namespace CardChange
{
    using ProberInterfaces.Temperature;
    using System.Diagnostics;

    public class CardChangeModule : SequenceServiceBase, ICardChangeModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private BehaviorResult ExecutionResult;

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.CardChange);
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

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        private ICardChangeState _CardChangeState;
        public ICardChangeState CardChangeState
        {
            get { return _CardChangeState; }
        }

        public IInnerState InnerState
        {
            get { return _CardChangeState; }
            set
            {
                if (value != _CardChangeState)
                {
                    _CardChangeState = value as CardChangeStateBase;
                }
            }
        }



        public IInnerState PreInnerState { get; set; }

        private IParam _CcSysParams_IParam;
        public IParam CcSysParams_IParam
        {
            get { return _CcSysParams_IParam; }
            set
            {
                if (value != _CcSysParams_IParam)
                {
                    _CcSysParams_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IParam _CIDSysParams_IParam;
        public IParam CIDSysParams_IParam
        {
            get { return _CIDSysParams_IParam; }
            set
            {
                if (value != _CIDSysParams_IParam)
                {
                    _CIDSysParams_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _CcDevParams_IParam;
        public IParam CcDevParams_IParam
        {
            get { return _CcDevParams_IParam; }
            set
            {
                if (value != _CcDevParams_IParam)
                {
                    _CcDevParams_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _GPCCObservationTempParams_IParam;
        public IParam GPCCObservationTempParams_IParam
        {
            get { return _GPCCObservationTempParams_IParam; }
            set
            {
                if (value != _GPCCObservationTempParams_IParam)
                {
                    _GPCCObservationTempParams_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _CardChangeStatusParams_IParam;
        public IParam CardChangeStatusParams_IParam
        {
            get { return _CardChangeStatusParams_IParam; }
            set
            {
                if (value != _CardChangeStatusParams_IParam)
                {
                    _CardChangeStatusParams_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CardChangeDevParam CcDevParams
        {
            get { return CcDevParams_IParam as CardChangeDevParam; }
            set
            {
                if (value != (CcDevParams_IParam as CardChangeDevParam))
                {
                    CcDevParams_IParam = value as IParam;
                    RaisePropertyChanged();
                }
            }
        }

        public CardChangeSysParam CcSysParams
        {
            get { return CcSysParams_IParam as CardChangeSysParam; }
            set
            {
                if (value != (CcSysParams_IParam as CardChangeSysParam))
                {
                    CcSysParams_IParam = value as IParam;
                    RaisePropertyChanged();
                }
            }
        }

        public CardIDSysParam CIDSysParams
        {
            get { return CIDSysParams_IParam as CardIDSysParam; }
            set
            {
                if (value != (CIDSysParams_IParam as CardIDSysParam))
                {
                    CIDSysParams_IParam = value as IParam;
                    RaisePropertyChanged();
                }
            }
        }

        public GPCCObservationTempParam GPCCObservationParams
        {
            get { return GPCCObservationTempParams_IParam as GPCCObservationTempParam; }
            set
            {
                if (value != (GPCCObservationTempParams_IParam as GPCCObservationTempParam))
                {
                    GPCCObservationTempParams_IParam = value as IParam;
                    RaisePropertyChanged();
                }
            }
        }

        public CardChangeStatus CardChangeStatusParams
        {
            get { return CardChangeStatusParams_IParam as CardChangeStatus; }
            set
            {
                if (value != (CardChangeStatusParams_IParam as CardChangeStatus))
                {
                    CardChangeStatusParams_IParam = value as IParam;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CCLogCollectionSelect;
        public int CCLogCollectionSelect
        {
            get { return _CCLogCollectionSelect; }
            set
            {
                _CCLogCollectionSelect = value;
                RaisePropertyChanged();
            }
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



        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {

                
                if (sender is DeviceChangedEvent ||//device change 성공
                    sender is PinAlignEndEvent ||//pinalign 성공
                    sender is StageRecipeReadStartEvent //device load 시작                    
                    )
                {
                    this.ReleaseWaitForCardPermission();
                }                      
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EnumCardChangeType GetCCType()
        {
            EnumCardChangeType retval = EnumCardChangeType.UNDEFINED;

            try
            {
                retval = CcSysParams?.CardChangeType.Value ?? EnumCardChangeType.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public bool GetWaitForCardPermitEnable()
        {
            bool retval = false;

            try
            {
                retval = CcSysParams?.WaitForCardPermitEnable.Value ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetWaitForCardPermitEnable(bool enable)
        {
            EventCodeEnum retval = EventCodeEnum.EXCEPTION;

            try
            {
                if(enable != CcSysParams.WaitForCardPermitEnable.Value)
                {
                    LoggerManager.Debug($"[CardChangeModule] SetWaitForCardPermitEnable(): {CcSysParams.WaitForCardPermitEnable.Value} => {enable}");
                }

                CcSysParams.WaitForCardPermitEnable.Value = enable;
                if(enable == false)
                {
                    this.CardChangeModule().ReleaseWaitForCardPermission();
                    //WaitFor변수는 Instant 변수 이므로 기능을 Disable하면 대기 상태도 종료 한다.
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private ISequenceBehaviors _CCBehaviors;
        public ISequenceBehaviors CCBehaviors
        {
            get { return _CCBehaviors; }
            set
            {
                if (value != _CCBehaviors)
                {
                    _CCBehaviors = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ISequenceBehaviors _CCDockBehaviors;
        public ISequenceBehaviors CCDockBehaviors
        {
            get { return _CCDockBehaviors; }
            set
            {
                if (value != _CCDockBehaviors)
                {
                    _CCDockBehaviors = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ISequenceBehaviors _CCUnDockBehaviors;
        public ISequenceBehaviors CCUnDockBehaviors
        {
            get { return _CCUnDockBehaviors; }
            set
            {
                if (value != _CCUnDockBehaviors)
                {
                    _CCUnDockBehaviors = value;
                    RaisePropertyChanged();

                }
            }
        }
        private int _CurBehaviorIdx = 0;
        public int CurBehaviorIdx
        {
            get { return _CurBehaviorIdx; }
            set
            {
                if (value != _CurBehaviorIdx)
                {
                    _CurBehaviorIdx = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private ISequenceBehavior _EndBehavior;
        //public ISequenceBehavior EndBehavior
        //{
        //    get { return _EndBehavior; }
        //    set
        //    {
        //        if (value != _EndBehavior)
        //        {
        //            _EndBehavior = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private ISequenceBehaviors _CCCommandParam;
        public ISequenceBehaviors CCCommandParam
        {
            get { return _CCCommandParam; }
            set
            {
                if (value != _CCCommandParam)
                {
                    _CCCommandParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ISequenceBehavior _GPCCBehavior;
        public ISequenceBehavior GPCCBehavior
        {
            get { return _GPCCBehavior; }
            set
            {
                if (value != _GPCCBehavior)
                {
                    _GPCCBehavior = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ISequenceBehaviors _THDockBehaviors;
        public ISequenceBehaviors THDockBehaviors
        {
            get { return _THDockBehaviors; }
            set
            {
                if (value != _THDockBehaviors)
                {
                    _THDockBehaviors = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ISequenceBehaviors _THUnDockBehaviors;
        public ISequenceBehaviors THUnDockBehaviors
        {
            get { return _THUnDockBehaviors; }
            set
            {
                if (value != _THUnDockBehaviors)
                {
                    _THUnDockBehaviors = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _InputPorts;
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

        private ObservableCollection<ISequenceBehaviorGroupItem> _StepGroupCollection;
        public ObservableCollection<ISequenceBehaviorGroupItem> StepGroupCollection
        {
            get { return _StepGroupCollection; }
            set
            {
                if (value != _StepGroupCollection)
                {
                    _StepGroupCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CardChangeModuleStateEnum _CCRecoveryState;
        public CardChangeModuleStateEnum CCRecoveryState
        {
            get { return _CCRecoveryState; }
            set
            {
                if (value != _CCRecoveryState)
                {
                    _CCRecoveryState = value;
                    RaisePropertyChanged();
                }
            }
        }

       

        private Type[] CCAllType = null;


        private int _CCStartPoint;
        public int CCStartPoint
        {
            get { return _CCStartPoint; }
            set
            {
                if (value != _CCStartPoint)
                {
                    _CCStartPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;

                }
            }
        }

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

        public CommandTokenSet RunTokenSet { get; set; }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }


        private bool _IsCardStuck = false;

        public bool IsCardStuck
        {
            get { return _IsCardStuck; }
            set { _IsCardStuck = value; }
        }

        private bool _StartCardDockingFlag = false;
        public bool StartCardDockingFlag
        {
            get { return _StartCardDockingFlag; }
            set { _StartCardDockingFlag = value; }
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
            bool retVal = false;
            foreach (var subModule in SubModules.SubModules)
            {
                if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                {
                    retVal = true;
                }
            }
            return retVal;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        InnerStateTransition(new CardChangeIdleState(this));
                        StateTransition(new ModuleIdleState(this));
                        GPCCBehavior = null;
                    }
                    else
                    {
                        GetAllCCType();
                        InnerStateTransition(new CardChangeInitState(this));

                        if (_CardChangeState.InitExecute() == 0)
                        {
                            InnerStateTransition(new CardChangeIdleState(this));
                            StateTransition(new ModuleIdleState(this));
                        }
                        else
                        {
                            InnerStateTransition(new CardChangeErrorState(this));
                            StateTransition(new ModuleErrorState(this));
                        }
                    }

                    _TransitionInfo = new ObservableCollection<TransitionInfo>();
                    Initialized = true;
                    IsCardStuck = false;

                    //<!-- Resigter Event -->

                    retval = this.EventManager().RegisterEvent(typeof(DeviceChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(PinAlignEndEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(StageRecipeReadStartEvent).FullName, "ProbeEventSubscibers", EventFired);

                    this.EventManager().RegisterEvent(typeof(CardChangedEvent).FullName, "ProbeEventSubscibers", CardChangedEventFired);                    

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

                //this.StopSequencer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum LoadCardChangeBehaviors()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            CCBehaviors = new SequenceBehaviors();
            CCBehaviors.Genealogy = this.GetType().Name + "." + CCBehaviors.GetType().Name + ".";

            string FullPath = this.FileManager().GetSystemParamFullPath("CardChange", "CardChangeSequence.json");

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SequenceBehaviors), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    CCBehaviors = tmpParam as SequenceBehaviors;
                }

            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[CardChangeModule] LoadCardChangeBehaviors(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        private EventCodeEnum LoadCardDockBehaviors()//GOP 카드체인지 할때 도킹 스퀀스 데이터 없으면 세이브까지!!
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            CCDockBehaviors = new SequenceBehaviors();
            CCDockBehaviors.Genealogy = this.GetType().Name + "." + CCDockBehaviors.GetType().Name + ".";

            string FullPath = this.FileManager().GetSystemParamFullPath("CardChange", "Card_DockingSequence.json");

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SequenceBehaviors), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    CCDockBehaviors = tmpParam as SequenceBehaviors;

                    if (CCDockBehaviors.ISequenceBehaviorCollection.Count == 0)
                    {
                        SequenceBehaviors behaviorgroup = null;

                        if (CcSysParams.CardChangeType.Value == EnumCardChangeType.CARRIER)
                        {
                            behaviorgroup = new SequenceBehaviors();

                            BehaviorGroupItem beh = null;
                            BehaviorGroupItem reverseGroupItem = null;

                            beh = new BehaviorGroupItem();//behavior
                            beh.Behavior = new GOP_CardDoorClose();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            beh.PreSafetyList.Add(new GOP_MoveSafeFromDoor());
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_CardDoorOpen();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////
                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_RaiseChuckBig();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            beh.PreSafetyList.Add(new GOP_RaisePCardPod());
                            beh.PreSafetyList.Add(new GOP_PCardPodVacuumOn());
                            beh.PreSafetyList.Add(new GOP_CheckPCardIsOnPCardPod());
                            beh.PreSafetyList.Add(new GOP_CheckPCardIsNotOnTopPlate());
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_DropPCardSafetyBig();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////
                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_TopPlateSolUnLock();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_TopPlateSolLock();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////
                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_RaiseChuckSmall();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_DropPCardSafetySmall();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////
                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_TopPlateSolLock();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_TopPlateSolUnLock();//Reverse behavior
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////
                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_TopPlateSolLocked();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            ////////////////////
                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_DropChuckSafety();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            beh.PostSafetyList.Add(new GOP_CheckPCardIsOnTopPlate());
                            beh.PostSafetyList.Add(new GOP_CheckPCardIsNotOnCarrier());
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);
                            
                            beh.Behavior.ReverseBehavior = new GOP_RaiseChuckToTopPlate();//Reverse behavior
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem.Behavior = beh.Behavior.ReverseBehavior as SequenceBehavior;
                            reverseGroupItem.PreSafetyList.Add(new GOP_CheckPCardIsNotOnCarrier());
                            reverseGroupItem.PreSafetyList.Add(new GOP_CheckPCardIsOnTopPlate());
                            CCDockBehaviors = behaviorgroup;
                        }
                        else if (CcSysParams.CardChangeType.Value == EnumCardChangeType.DIRECT_CARD)
                        {
                            //TODO
                        }
                        else
                        {
                            //카드 타입이 캐리어도 아니고 다이렉트도 아니면 에러인지 UNDEFINED하면되는지?
                        }
                        Extensions_IParam.SaveDataToJson(behaviorgroup, FullPath);
                    }

                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        private EventCodeEnum LoadCardUnDockBehaviors()//그룹 프로버 카드체인지 할때 언도킹 시퀀스 데이터 없으면 세이브까지!!
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            CCUnDockBehaviors = new SequenceBehaviors();
            CCUnDockBehaviors.Genealogy = this.GetType().Name + "." + CCUnDockBehaviors.GetType().Name + ".";

            string FullPath = this.FileManager().GetSystemParamFullPath("CardChange", "Card_UnDockingSequence.json");

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SequenceBehaviors), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    CCUnDockBehaviors = tmpParam as SequenceBehaviors;

                    if (CCUnDockBehaviors.ISequenceBehaviorCollection.Count == 0)
                    {
                        SequenceBehaviors behaviorgroup = null;

                        if (CcSysParams.CardChangeType.Value == EnumCardChangeType.CARRIER)
                        {
                            behaviorgroup = new SequenceBehaviors();

                            BehaviorGroupItem beh = null;
                            BehaviorGroupItem reverseGroupItem = null;

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_SetCardContactPos();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            beh.PreSafetyList.Add(new GOP_PCardPodVacuumOn());
                            beh.PreSafetyList.Add(new GOP_ThreeLegCheckAndDown());
                            beh.PreSafetyList.Add(new GOP_CheckCarrierIsOnPCardPod());
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_MoveSafeFromDoor();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_RaiseChuckToTopPlate();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            beh.PreSafetyList.Add(new GOP_RaisePCardPod());
                            beh.PreSafetyList.Add(new GOP_CheckPCardIsNotOnCarrier());
                            beh.PreSafetyList.Add(new GOP_PCardPodVacuumOn());
                            beh.PreSafetyList.Add(new GOP_CheckPCardIsOnTopPlate());
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_DropChuckSafety();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_TopPlateSolUnLock();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_TopPlateSolLock();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_TopPlateSolUnlocked();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);
                            ////////////////////

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_DropPCardSafety();
                            beh.BehaviorID = Guid.NewGuid().ToString();
                            beh.PostSafetyList.Add(new GOP_CheckPCardIsExist());
                            beh.PostSafetyList.Add(new GOP_CheckPCardIsNotOnTopPlate());
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_RaiseChuckSmall();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_TopPlateSolLock();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);

                            beh.Behavior.ReverseBehavior = new GOP_TopPlateSolUnLock();//Reverse
                            reverseGroupItem = new BehaviorGroupItem();
                            reverseGroupItem = beh.Behavior.ReverseBehavior as BehaviorGroupItem;
                            ////////////////////

                            beh = new BehaviorGroupItem();
                            beh.Behavior = new GOP_ClearCardUnDocking();
                            behaviorgroup.SequenceBehaviorCollection.Add(beh);
                            CCUnDockBehaviors = behaviorgroup;
                            Extensions_IParam.SaveDataToJson(behaviorgroup, FullPath);
                        }

                    }
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        private EventCodeEnum LoadTHUndockBehaviors()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string FullPath = this.FileManager().GetSystemParamFullPath("CardChange", "THUndockSequence.json");

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SequenceBehaviors), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    THUnDockBehaviors = tmpParam as SequenceBehaviors;

                    if (THUnDockBehaviors != null)
                    {
                        foreach (var v in THUnDockBehaviors.ISequenceBehaviorCollection)
                            v.InitModule();
                    }
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[CardChangeModule] LoadTHUndockBehaviors(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private EventCodeEnum LoadThDockBehaviors()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            THDockBehaviors = new SequenceBehaviors();


            string FullPath = this.FileManager().GetSystemParamFullPath("CardChange", "THDockSequence.json");

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SequenceBehaviors), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    THDockBehaviors = tmpParam as SequenceBehaviors;

                    if (THDockBehaviors != null)
                    {
                        foreach (var v in THDockBehaviors.ISequenceBehaviorCollection)
                            v.InitModule();
                    }
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[CardChangeModule] LoadThDockBehaviors(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        private EventCodeEnum LoadGPCCObservationTempParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new GPCCObservationTempParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(GPCCObservationTempParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    GPCCObservationTempParams_IParam = tmpParam;
                    GPCCObservationParams = GPCCObservationTempParams_IParam as GPCCObservationTempParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private EventCodeEnum LoadCardChangeStatusParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new CardChangeStatus();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CardChangeStatus));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CardChangeStatusParams_IParam = tmpParam;
                    CardChangeStatusParams = CardChangeStatusParams_IParam as CardChangeStatus;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new CardChangeSysParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CardChangeSysParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CcSysParams_IParam = tmpParam;
                    CcSysParams = CcSysParams_IParam as CardChangeSysParam;
                    this.GEMModule().GetPIVContainer().ProbeCardID.Value = CcSysParams.ProbeCardID.Value;

                    LoggerManager.SetProbeCardID(CcSysParams.ProbeCardID.Value);
                }

                tmpParam = null;
                tmpParam = new CardChangeSysParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CardIDSysParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    CIDSysParams_IParam = tmpParam;
                    CIDSysParams = CIDSysParams_IParam as CardIDSysParam;
                }

                if (CcSysParams.CardChangeType == null)
                {
                    try
                    {
                        CcSysParams.CardChangeType.Value = EnumCardChangeType.UNDEFINED;
                        this.SaveSysParameter();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
                else if (CcSysParams.CardChangeType.Value == EnumCardChangeType.UNDEFINED)
                {
                    try
                    {
                        this.SaveSysParameter();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }


                RetVal = LoadCardDockBehaviors();
                RetVal = LoadCardUnDockBehaviors();
                RetVal = LoadCardChangeBehaviors();
                RetVal = LoadThDockBehaviors();
                RetVal = LoadTHUndockBehaviors();
                RetVal = LoadGPCCObservationTempParam();
                RetVal = LoadCardChangeStatusParam();
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
                RetVal = SaveCardChangeParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
       
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new CardChangeDevParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CardChangeDevParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CcDevParams_IParam = tmpParam;
                    CcDevParams = CcDevParams_IParam as CardChangeDevParam;

                    if (CcDevParams.ModelInfos == null)
                    {
                        CcDevParams.ModelInfos = new List<ProberInterfaces.Vision.MFParameter>();
                    }

                    if(CcDevParams.ModelInfos.Count > 0 )
                    {
                        CcDevParams.ModelInfos.Clear();
                    }

                    if (CcDevParams.ModelInfos.Count == 0)
                    {
                        try
                        {
                            CcDevParams.ModelInfos = new List<MFParameter>();
                            MFParameter mF = new MFParameter();
                            mF.ModelTargetType.Value = EnumModelTargetType.Rectangle;
                            mF.ModelWidth.Value = 95 * 5.28;
                            mF.ModelHeight.Value = 95 * 5.28;
                            mF.ForegroundType.Value = EnumForegroundType.ANY;
                            mF.ScaleMin.Value = 0.95;
                            mF.ScaleMax.Value = 1.05;
                            mF.Acceptance.Value = 80;
                            mF.Certainty.Value = 95;
                            mF.CamType.Value = EnumProberCam.WAFER_LOW_CAM;
                            mF.Lights = new List<LightChannelType>();
                            mF.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 100));
                            mF.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                            mF.Smoothness.Value = 70;
                            mF.ExpectedOccurrence.Value = 1;

                            mF.Child = new MFParameter();
                            mF.Child.ModelTargetType.Value = EnumModelTargetType.Circle;
                            mF.Child.ModelWidth.Value = 20 * 5.28; // px * WL ratio
                            mF.Child.ModelHeight.Value = 20 * 5.28;
                            mF.Child.ForegroundType.Value = EnumForegroundType.ANY;
                            mF.Child.ScaleMin.Value = 0.95;
                            mF.Child.ScaleMax.Value = 1.05;
                            mF.Child.Acceptance.Value = 80;
                            mF.Child.Certainty.Value = 95;
                            mF.Child.CamType.Value = EnumProberCam.WAFER_LOW_CAM;
                            mF.Child.Lights = new List<LightChannelType>();
                            mF.Child.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 100));
                            mF.Child.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                            mF.Child.Smoothness.Value = 70;
                            mF.Child.ExpectedOccurrence.Value = 1;

                            CcDevParams.ModelInfos.Add(mF);
                            this.SaveDevParameter();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug(err.Message);
                        }

                        // must add new model selly 
                        try
                        {
                            var cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                            MFParameter mF = new MFParameter();
                            // MA Card
                            mF.ModelTargetType.Value = EnumModelTargetType.Rectangle;
                            mF.ModelWidth.Value = 190 * cam.GetRatioX();
                            mF.ModelHeight.Value = 190 * cam.GetRatioY();
                            mF.ForegroundType.Value = EnumForegroundType.ANY;
                            mF.ScaleMin.Value = 0.9;
                            mF.ScaleMax.Value = 1.1;
                            mF.Acceptance.Value = 80;
                            mF.Certainty.Value = 95;
                            mF.CamType.Value = EnumProberCam.PIN_LOW_CAM;
                            mF.Lights = new List<LightChannelType>();
                            mF.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 255));
                            mF.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                            mF.Smoothness.Value = 70;
                            mF.ExpectedOccurrence.Value = 1;

                            mF.Child = new MFParameter();
                            mF.Child.ModelTargetType.Value = EnumModelTargetType.Circle;
                            mF.Child.ModelWidth.Value = 35 * cam.GetRatioX();
                            mF.Child.ModelHeight.Value = 35 * cam.GetRatioY();
                            mF.Child.ForegroundType.Value = EnumForegroundType.ANY;
                            mF.Child.ScaleMin.Value = 0.9;
                            mF.Child.ScaleMax.Value = 1.1;
                            mF.Child.Acceptance.Value = 80;
                            mF.Child.Certainty.Value = 95;
                            mF.CamType.Value = EnumProberCam.PIN_LOW_CAM;
                            mF.Child.Lights = new List<LightChannelType>();
                            mF.Child.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 255));
                            mF.Child.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                            mF.Child.Smoothness.Value = 70;
                            mF.Child.ExpectedOccurrence.Value = 1;

                            CcDevParams.ModelInfos.Add(mF);
                            this.SaveDevParameter();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug(err.Message);
                        }
                    }

                    // remove previous model (only circle)
                    //if (CcDevParams.ModelInfos.Count == 1)
                    //{
                    //    var cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                    //    MFParameter mF = new MFParameter();
                    //    // LB Card
                    //    mF = new MFParameter();
                    //    mF.ModelTargetType.Value = EnumModelTargetType.Circle;
                    //    mF.ModelWidth.Value = 35 * cam.GetRatioX(); // px * PL ratio
                    //    mF.ModelHeight.Value = 35 * cam.GetRatioX();
                    //    mF.ForegroundType.Value = EnumForegroundType.ANY;
                    //    mF.ScaleMin.Value = 0.9;
                    //    mF.ScaleMax.Value = 1.1;
                    //    mF.Acceptance.Value = 75;
                    //    mF.Certainty.Value = 95;
                    //    mF.Lights = new List<LightChannelType>();
                    //    mF.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 255));
                    //    mF.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                    //    mF.Smoothness.Value = 99;
                    //    mF.Child = null;
                    //    CcDevParams.ModelInfos.Add(mF);

                    //    // TY Card
                    //    mF = new MFParameter();
                    //    mF.ModelTargetType.Value = EnumModelTargetType.Circle;
                    //    mF.ModelWidth.Value = 40 * cam.GetRatioX(); // px * PL ratio
                    //    mF.ModelHeight.Value = 40 * cam.GetRatioX();
                    //    mF.ForegroundType.Value = EnumForegroundType.ANY;
                    //    mF.ScaleMin.Value = 0.9;
                    //    mF.ScaleMax.Value = 1.1;
                    //    mF.Acceptance.Value = 75;
                    //    mF.Certainty.Value = 95;
                    //    mF.Lights = new List<LightChannelType>();
                    //    mF.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 255));
                    //    mF.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                    //    mF.Smoothness.Value = 99;
                    //    mF.Child = null;
                    //    CcDevParams.ModelInfos.Add(mF);

                    //    // Dummy Card
                    //    mF = new MFParameter();
                    //    mF.ModelTargetType.Value = EnumModelTargetType.Circle;
                    //    mF.ModelWidth.Value = 108 * cam.GetRatioX(); // px * PL ratio
                    //    mF.ModelHeight.Value = 108 * cam.GetRatioX();
                    //    mF.ForegroundType.Value = EnumForegroundType.ANY;
                    //    mF.ScaleMin.Value = 0.9;
                    //    mF.ScaleMax.Value = 1.1;
                    //    mF.Acceptance.Value = 70;
                    //    mF.Certainty.Value = 95;
                    //    mF.Lights = new List<LightChannelType>();
                    //    mF.Lights.Add(new LightChannelType(EnumLightType.COAXIAL, 255));
                    //    mF.Lights.Add(new LightChannelType(EnumLightType.OBLIQUE, 0));
                    //    mF.Smoothness.Value = 99;
                    //    mF.Child = null;
                    //    CcDevParams.ModelInfos.Add(mF);

                    //    this.SaveDevParameter();
                    //}

                    LoggerManager.Debug("ModelInfo Count: " + CcDevParams.ModelInfos.Count());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(CcDevParams);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveCardChangeParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(CcSysParams);

                RetVal = this.SaveParameter(CIDSysParams);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SaveGPCCObservationTempParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(GPCCObservationParams);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveCardChangeStatusParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(CardChangeStatusParams);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        #region IModule 
        public bool CanExecute(IProbeCommandToken token)
        {
            bool RetVal = false;
            try
            {
                RetVal = CardChangeState.CanExecute(token);
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
            }
        }

        //public override ModuleStateEnum SequenceRun()
        //{
        //    ModuleStateEnum moduleStateEnum = CardChangeState.GetModuleState();
        //    moduleStateEnum = Execute();

        //    return moduleStateEnum;
        //}

        public ModuleStateEnum Execute()
        {
            try
            {
                CardChangeState.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CardChangeState.GetModuleState();
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
            return InnerState.GetModuleState();
        }

        public EventCodeEnum ClearState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LoaderController().SetTransferError(false);
                //NeedToRecovery = false;
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
        public bool GetCardIDValidationEnable()
        {
            return CIDSysParams?.Enable ?? false;
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void GetAllCCType()
        {
            try
            {
                if (CCAllType == null)
                {
                    var safetySubclasses = from t in Assembly.GetAssembly(typeof(SequenceSafety)).GetTypes()
                                           where t.IsSubclassOf(typeof(SequenceSafety))
                                           select t;

                    var behaviorSubclasses = from t in Assembly.GetAssembly(typeof(SequenceBehavior)).GetTypes()
                                             where t.IsSubclassOf(typeof(SequenceBehavior))
                                             select t;

                    List<Type> allType = new List<Type>() {typeof(SequenceBehaviorStruct),  typeof(BehaviorGroupItem),
                                                      typeof(BehaviorGroupItem),
                                                      typeof(SequenceBehaviors) };

                    if (safetySubclasses != null)
                    {
                        foreach (var v in safetySubclasses)
                        {
                            try
                            {
                                allType.Add(v);
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            }
                        }
                    }

                    if (behaviorSubclasses != null)
                    {
                        foreach (var v in behaviorSubclasses)
                        {
                            try
                            {
                                allType.Add(v);
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            }
                        }
                    }

                    CCAllType = allType.ToArray();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                PreInnerState = InnerState;
                InnerState = state;
                LoggerManager.Debug($"[{GetType().Name}].InnerStateTransition() : Pre state = {PreInnerState}), Now State = {InnerState})");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ObservableCollection<ISequenceBehaviorGroupItem> GetCcGroupCollection()
        {
            ObservableCollection<ISequenceBehaviorGroupItem> retVal = null;
            try
            {
                retVal = CCBehaviors.ISequenceBehaviorCollection;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ObservableCollection<ISequenceBehaviorGroupItem> GetCcDockCollection()
        {
            ObservableCollection<ISequenceBehaviorGroupItem> retVal = null;
            try
            {
                retVal = CCDockBehaviors.ISequenceBehaviorCollection;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        //public void UpdateDockState()
        //{
        //    try
        //    {
        //        byte[] serializeData = ObjectSerialize.Serialize(this.CCDockBehaviors.ISequenceBehaviorCollection);
        //        this.LoaderRemoteMediator().UpdateDockChangeStateObject(serializeData);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public void UpdateUnDockState()
        //{
        //    try
        //    {
        //        byte[] serializeData = ObjectSerialize.Serialize(this.CCUnDockBehaviors.ISequenceBehaviorCollection);
        //        this.LoaderRemoteMediator().UpdateUnDockChangeStateObject(serializeData);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        public void UpdateErrorMessage(string err_msg)
        {
            try
            {
                string msg = err_msg;
                this.LoaderRemoteMediator().Update_Error_MSG(msg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool _Observation_DockContinue = false;
        public bool Observation_DockContinue
        {
            get { return _Observation_DockContinue; }
            set { _Observation_DockContinue = value; }
        }

        private int _Behavior_Count;
        public int Behavior_Count
        {
            get { return _Behavior_Count; }
            set
            {
                if (value != _Behavior_Count)
                {
                    _Behavior_Count = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<ISequenceBehaviorGroupItem> GetTHDockGroupCollection(THDockType type)
        {
            ObservableCollection<ISequenceBehaviorGroupItem> retVal = null;

            try
            {
                if (type == THDockType.TH_DOCK)
                {
                    retVal = THDockBehaviors.ISequenceBehaviorCollection;
                }
                else
                {
                    retVal = THUnDockBehaviors.ISequenceBehaviorCollection;
                }
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
        public EventCodeEnum SetProbeCardID(string ID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (CcSysParams != null)
                {
                    CcSysParams.ProbeCardID.Value = ID;
                    SaveSysParameter();
                }

                this.GEMModule().GetPIVContainer().SetProberCardID(ID);
                LoggerManager.SetProbeCardID(CcSysParams.ProbeCardID.Value);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return retVal;
        }
        public string GetProbeCardID()
        {
            if (CcSysParams == null)
            {
                return null;
            }

            return CcSysParams.ProbeCardID.Value;
        }

        public EnumCardChangeType GetCardChangeType()
        {
            if (CcSysParams == null)
            {
                return EnumCardChangeType.UNDEFINED;
            }
            return CcSysParams.CardChangeType.Value;
        }

        public bool GetCardDoorAttached()
        {
            if (CcSysParams == null)
            {
                return false;
            }
            return CcSysParams.CardDoorAttached;
        }
        //Soaking중 pin align동작 여부를 판단하기 위한 함수
        public bool IsExistCard(bool bHolderIsCard = false)
        {
            bool isDockCard = false;

            try
            {
                if (CcSysParams.CardChangeType.Value == EnumCardChangeType.CARRIER)
                {
                    if ((CcSysParams_IParam as ICardChangeSysParam).IsCardExist && this.IsDocked)
                    {
                        string cardId = GetProbeCardID();
                        if (string.IsNullOrEmpty(cardId) == false && cardId.ToLower() != "holder")
                        {
                            isDockCard = true;
                        }
                    }
                }
                //latch랑 PogoCard Vaccum으로 Card가 Docking 상태인지 체크
                else if (CcSysParams.CardChangeType.Value == EnumCardChangeType.DIRECT_CARD)
                {
                    if (CcSysParams.CardDockType.Value == EnumCardDockType.NORMAL)
                    {
                        IBehaviorResult topPlateSolIsLock = new BehaviorResult();
                        var command = new GP_CheckTopPlateSolIsLock(false);//=> 상판에 Card latch가 잠겨 있는지 확인;닫혀있으면 NONE
                        topPlateSolIsLock = command.Run().Result;

                        IBehaviorResult cardIsOnPogoResult = new BehaviorResult();
                         var pcOnPogoCheckcommand = new GP_CheckPCardIsOnPogo(false);//Card와 POGO 사이의 Vaccum을 체크;Vac이 잡혀있으면 NONE
                        cardIsOnPogoResult = pcOnPogoCheckcommand.Run().Result;

                        if (topPlateSolIsLock.ErrorCode == EventCodeEnum.NONE && cardIsOnPogoResult.ErrorCode == EventCodeEnum.NONE)
                        {
                            isDockCard = true;
                        }
                        else
                        {
                            isDockCard = false;
                        }
                    }
                    else if (CcSysParams.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                    {
                        IBehaviorResult cardIsOnPogoResult = new BehaviorResult();
                        var command = new GP_CheckPCardIsOnPogo(false);//Card와 POGO 사이의 Vaccum을 체크;Vac이 잡혀있으면 NONE
                        cardIsOnPogoResult = command.Run().Result;

                        if (cardIsOnPogoResult.ErrorCode == EventCodeEnum.NONE)
                        {
                            isDockCard = true;
                        }
                        else
                        {
                            isDockCard = false;
                        }
                    }
                    else
                    {
                        isDockCard = false;
                    }
                }
                else
                {
                    isDockCard = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isDockCard;
        }

        /// <summary>
        /// CardPod 에 Card 가 있는경우, 정상적인 상태인지 확인한다. 
        /// Card Load 시에, CardPod 에 카드는 올라가 있고 베큠은 잡히지 않는경우가 있음. (이런 상황에대해 안전장치를 추가 하고자 한다.)
        /// </summary>
        /// <returns></returns>
        public bool IsCheckCardPodState()
        {
            bool retVal = false;
            try
            {
                IBehaviorResult cardPodIsUpResult = new BehaviorResult();
                var cardPodUpCheckcommand = new GP_CheckPCardPodIsDown(); // Card Pod Module 이 내려가있는지 확인
                cardPodIsUpResult = cardPodUpCheckcommand.Run().Result;

                if(cardPodIsUpResult.ErrorCode == EventCodeEnum.NONE)
                {
                    retVal = true;
                }
                else if(cardPodIsUpResult.ErrorCode == EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS)
                {
                    IBehaviorResult cardIsOnPCardPodResule = new BehaviorResult();
                    var cardOnPodCheckcommand = new GP_CheckPCardIsOnPCardPod(); // card pod 에 card 가 있는지 확인.
                    cardIsOnPCardPodResule = cardOnPodCheckcommand.Run().Result;

                    if (cardIsOnPCardPodResule.ErrorCode == EventCodeEnum.NONE)
                    {
                        // card pod 올라가고, 베큠 잡혀 있는 상태
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                        LoggerManager.Debug("[CardChangeModule] IsCheckCardPodState() Card Pod Module is up, card pod vacuum is not detected.");
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
        /// CardPod 에 Carrier 가 있는경우, 정상적인 상태인지 확인한다. 
        /// </summary>
        /// <returns></returns>
        public bool IsCheckCarrierIsOnPCardPod()
        {
            bool retVal = false;
            try
            {
                IBehaviorResult cardPodCheck = new BehaviorResult();
                var cardPodcommand = new GOP_PCardPodVacuumOn(); // Card Pod VacuumOn
                cardPodCheck = cardPodcommand.Run().Result;
                if (cardPodCheck.ErrorCode == EventCodeEnum.NONE)
                {
                    cardPodCheck = new BehaviorResult();
                    var carrierCheckcommand = new GOP_CheckCarrierIsOnPCardPod(); // Card Pod Module 에 carrier 있는지 확인
                    cardPodCheck = carrierCheckcommand.Run().Result;
                    if (cardPodCheck.ErrorCode == EventCodeEnum.NONE)
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum IsZifRequestedState(bool lock_request, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                var testerio_connected = this.IOManager().IO.Inputs.DITH_MBLOCK;
                var testerpcb_disconnected = this.IOManager().IO.Inputs.DITH_PBUNLOCK;
                var testerpcb_connected = this.IOManager().IO.Inputs.DITH_PBLOCK;// DF Tester는 없음. Advan Tester는 있음.

                int testerio_connected_rst = -1;
                int testerpcb_disconnected_rst = -1;
                int testerpcb_connected_rst = -1;
                

                if (testerio_connected.IOOveride.Value == EnumIOOverride.NONE)
                {
                    if (lock_request)
                    {
                        testerio_connected_rst = this.IOManager().IOServ.MonitorForIO(testerio_connected, 
                                                                                     true, 
                                                                                     maintainTime: testerio_connected.MaintainTime.Value, 
                                                                                     testerio_connected.TimeOut.Value, 
                                                                                     writelog: writelog);
                    }
                    else
                    {
                        //만약 mblock이 false 인데 pb lock 상태 일 경우 아래쪽에서 에러남.
                        testerio_connected_rst = 0;                        
                    }
                    


                    if (testerpcb_disconnected.IOOveride.Value == EnumIOOverride.NONE ||
                        testerpcb_connected.IOOveride.Value == EnumIOOverride.NONE)
                    {
                        if (testerpcb_disconnected.IOOveride.Value == EnumIOOverride.NONE)
                        {
                            testerpcb_disconnected_rst = this.IOManager().IOServ.MonitorForIO(testerpcb_disconnected,
                                                                                              !lock_request, 
                                                                                               maintainTime: testerpcb_disconnected.MaintainTime.Value, 
                                                                                               testerpcb_disconnected.TimeOut.Value, 
                                                                                               writelog: writelog);
                        }
                        else
                        {
                            testerpcb_disconnected_rst = 0;
                        }

                        if (testerpcb_connected.IOOveride.Value == EnumIOOverride.NONE)
                        {
                            testerpcb_connected_rst = this.IOManager().IOServ.MonitorForIO(testerpcb_connected, 
                                                                                           lock_request, 
                                                                                           maintainTime: testerpcb_connected.MaintainTime.Value, 
                                                                                           testerpcb_connected.TimeOut.Value, 
                                                                                           writelog: writelog);
                        }
                        else
                        {
                            testerpcb_connected_rst = 0;
                        }
                    }
                    else
                    {
                        // Zif Lock 상태를 보는 함수 있데 카드와 테스터간 연결을 보는 IO가 하나도 없는 경우 Error
                    }


                    if (testerio_connected_rst != 0 ||
                        testerpcb_disconnected_rst != 0 ||
                        testerpcb_connected_rst != 0)
                    {
                        retVal = EventCodeEnum.ZIF_STATE_NOT_READY;
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }

                }
                else
                {
                    // Zif Lock 을 사용하지 않는 경우 정상 처리됨.
                    retVal = EventCodeEnum.NONE;                    
                }


            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// DOTESTER_HEAD_LOCK와 대응 되는 Input값.
        /// </summary>
        /// <param name="lock_request"></param>
        /// <param name="writelog"></param>
        /// <returns></returns>
        public EventCodeEnum IsTeadLockRequestedState(bool lock_request, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var testerhead_locked = this.IOManager().IO.Inputs.DITH_LOCK;
                var testerhead_unlocked = this.IOManager().IO.Inputs.DITH_UNLOCK;
                

                int testerhead_locked_rst = -1;
                int testerhead_unlocked_rst = -1;

                if (testerhead_locked.IOOveride.Value == EnumIOOverride.NONE)
                {
                    testerhead_locked_rst = this.IOManager().IOServ.MonitorForIO(testerhead_locked, 
                                                                                  lock_request, 
                                                                                  maintainTime: testerhead_locked.MaintainTime.Value,
                                                                                  testerhead_locked.TimeOut.Value, 
                                                                                  writelog: writelog);
                }
                else
                {
                    testerhead_locked_rst = 0;
                }

                if (testerhead_unlocked.IOOveride.Value == EnumIOOverride.NONE)
                {
                    testerhead_unlocked_rst = this.IOManager().IOServ.MonitorForIO(testerhead_unlocked, 
                                                                                    !lock_request, 
                                                                                    maintainTime: testerhead_unlocked.MaintainTime.Value,
                                                                                    testerhead_unlocked.TimeOut.Value, 
                                                                                    writelog: writelog);
                }
                else
                {
                    testerhead_unlocked_rst = 0;
                }


                if (testerhead_locked_rst != 0 ||
                    testerhead_unlocked_rst != 0)
                {
                    if (lock_request)
                    {
                        retVal = EventCodeEnum.TESTERHEAD_NOT_LOCKED_STATE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.TESTERHEAD_NOT_UNLOCKED_STATE;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// Tester-Card 사이의 Clamp 상태가 유효한지 판단하는 구문.
        /// </summary>
        /// <param name="lock_request"></param>
        /// <param name="writelog"></param>
        /// <returns></returns>
        public EventCodeEnum IsTeadClampLockRequestedState(bool lock_request, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                
                var tester_notuseclamp = this.IOManager().IO.Inputs.DINO_CLAMP;
                var testerclamp_locked = this.IOManager().IO.Inputs.DICLP_LOCK;
                int testerclamp_locked_rst = -1;

                if (tester_notuseclamp.IOOveride.Value == EnumIOOverride.NONE)
                {
                    var notuseclamp = false;
                    this.IOManager().IOServ.ReadBit(tester_notuseclamp, out notuseclamp);
                    if (notuseclamp == true)
                    {
                        retVal = EventCodeEnum.NONE;// clamp 가 달려있지 않기 때문에 testerclamp_locked를 확인할 필요가 없음.
                        return retVal;
                    }
                    else
                    {
                        if (testerclamp_locked.IOOveride.Value == EnumIOOverride.NONE)
                        {
                            testerclamp_locked_rst = this.IOManager().IOServ.MonitorForIO(testerclamp_locked,
                                                                                            lock_request, 
                                                                                            maintainTime: testerclamp_locked.MaintainTime.Value, 
                                                                                            testerclamp_locked.TimeOut.Value, 
                                                                                            writelog: writelog);
                        }
                        else
                        {
                            testerclamp_locked_rst = 0;
                        }

                    }

                }
                else
                {
                    // notuseclamp가 false 로 판단하도록 동작. 
                    if (testerclamp_locked.IOOveride.Value == EnumIOOverride.NONE)
                    {
                        testerclamp_locked_rst = this.IOManager().IOServ.MonitorForIO(testerclamp_locked, 
                                                                                        lock_request, 
                                                                                        maintainTime: testerclamp_locked.MaintainTime.Value,
                                                                                        testerclamp_locked.TimeOut.Value, 
                                                                                        writelog: writelog);
                    }
                    else
                    {
                        testerclamp_locked_rst = 0;
                    }

                }




                if (testerclamp_locked_rst != 0)
                {
                    if (lock_request)
                    {
                        retVal = EventCodeEnum.TESTER_CLAMP_NOT_LOCKED_STATE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.TESTER_CLAMP_NOT_UNLOCKED_STATE;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public bool IsDocked
        {
            get { return (CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsDocked; }            
        }


        public double GetCCActivatableTemp()
        {
            double retVal = 30.0;
            try
            {
                if (CcSysParams != null)
                {
                   if(CcSysParams.CCActivatableTemp != null)
                    {
                        retVal = CcSysParams.CCActivatableTemp.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public double GetOverHeatingValue()
        {
            double retVal = 0.0;
            try
            {
                if (CcSysParams != null)
                {
                    if (CcSysParams.OverHeating != null)
                    {
                        retVal = CcSysParams.OverHeating.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public double GetOverHeattingHysteresis()
        {
            double retVal = 0.0;
            try
            {
                if (CcSysParams != null)
                {
                    if (CcSysParams.OverHeatingHysteresis != null)
                    {
                        retVal = CcSysParams.OverHeatingHysteresis.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum CardChangeInit()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                //if (CcSysParams.CardCahngerAttached.Value == true)
                if (CcDevParams.CardCahngerAttached.Value == true)
                {
                    var axisCT = this.MotionManager().GetAxis(EnumAxisConstants.CT);
                    var axisCCS = this.MotionManager().GetAxis(EnumAxisConstants.CCS);
                    var axisCCM = this.MotionManager().GetAxis(EnumAxisConstants.CCM);
                    var axisROT = this.MotionManager().GetAxis(EnumAxisConstants.ROT);
                    var axisCCG = this.MotionManager().GetAxis(EnumAxisConstants.CCG);


                    this.MotionManager().AmpFaultClear(EnumAxisConstants.CT);
                    this.MotionManager().AmpFaultClear(EnumAxisConstants.CCS);
                    this.MotionManager().AmpFaultClear(EnumAxisConstants.CCM);
                    this.MotionManager().AmpFaultClear(EnumAxisConstants.ROT);
                    this.MotionManager().AmpFaultClear(EnumAxisConstants.CCG);
                    //delays.DelayFor(500);
                    System.Threading.Thread.Sleep(500);
                    this.MotionManager().EnableAxis(axisCT);
                    this.MotionManager().EnableAxis(axisCCS);
                    this.MotionManager().EnableAxis(axisCCM);
                    this.MotionManager().EnableAxis(axisROT);
                    //delays.DelayFor(500);
                    System.Threading.Thread.Sleep(500);
                    this.MotionManager().EnableAxis(axisCCG);

                    axisCT.Status.IsHomeSeted = true;
                    axisCCS.Status.IsHomeSeted = true;
                    axisCCM.Status.IsHomeSeted = true;
                    axisROT.Status.IsHomeSeted = true;
                    axisCCG.Status.IsHomeSeted = true;

                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                var errordata = this.StageSupervisor().ConvertToExceptionErrorCode(err);
            }

            return ret;
        }
        public EventCodeEnum GPCardChangeInit()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                bool dipogocard_vacu_sensor;
                bool ditplate_pclatch_sensor_lock = false;

                SequenceBehavior Command = null;
                Command = new GP_CheckCardPodOutputStatus();
                Task<IBehaviorResult> task = Command.Run();
                ret = task.Result.ErrorCode;

                if (ret == EventCodeEnum.NONE)
                {
                    EnumCardDockingStatus dockStatus = GetCardDockingStatus();

                    // 카드 도킹 상태 확인
                    if (CcSysParams.CardChangeType.Value == EnumCardChangeType.DIRECT_CARD)
                    {
                        var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                        if (ioret != IORet.NO_ERR)
                        {
                            ret = EventCodeEnum.GP_CardChange_INIT_FAIL;
                            return ret;
                        }
                        ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                        if (ioret != IORet.NO_ERR)
                        {
                            ret = EventCodeEnum.GP_CardChange_INIT_FAIL;
                            return ret;
                        }

                        if (dipogocard_vacu_sensor == false)
                        {
                            if (ditplate_pclatch_sensor_lock)
                            {
                                SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                                //SetCardDockedWithPogoStatus();
                                IsCardStuck = true;
                                LoggerManager.Debug($"Set card stuck in GPCardChangeInit func");
                                //TODO: 카드검사  
                            }
                            else
                            {
                                if (dockStatus == EnumCardDockingStatus.DOCKED)
                                {
                                    //카드베큠이 안잡히지만, 마지막 상태가 Docked상태였다면, 카드가 Latch에 걸려있는 Stuck 상태.
                                    SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                                }
                                else if (dockStatus == EnumCardDockingStatus.STUCKED)
                                {
                                    // 마지막 상태 Stuck인 경우 상태 유지
                                    SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                                }
                                else
                                {
                                    SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                                }
                                //SetCardUnDockedWithPogoStatus();
                                IsCardStuck = false;
                            }
                            ret = EventCodeEnum.NONE;
                        }
                        else
                        {
                            ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                            if (ioret != IORet.NO_ERR)
                            {
                                ret = EventCodeEnum.GP_CardChange_INIT_FAIL;
                                return ret;
                            }

                            bool ditplate_pclatch_sensor_unlock;
                            ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                            if (ioret != IORet.NO_ERR)
                            {
                                ret = EventCodeEnum.GP_CardChange_INIT_FAIL;
                                return ret;
                            }

                            if (GetCCDockType() == EnumCardDockType.DIRECTDOCK || (ditplate_pclatch_sensor_lock == true) && (ditplate_pclatch_sensor_unlock == false))
                            {
                                if (dockStatus == EnumCardDockingStatus.DOCKEDWITHSUBVAC)
                                {
                                    //마지막 상태가 SubVac에 의해 도킹되어 있는 DOCKEDWITHSUBVAC상태라면 상태 유지.
                                    SetCardDockingStatus(EnumCardDockingStatus.DOCKEDWITHSUBVAC);
                                }
                                else if (dockStatus == EnumCardDockingStatus.STUCKED)
                                {
                                    //마지막 상태가 STUCKED인 경우 유지
                                    SetCardDockingStatus(EnumCardDockingStatus.STUCKED);
                                }
                                else
                                {
                                    SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
                                }
                                //SetCardDockedWithPogoStatus();
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ret = EventCodeEnum.GP_CardChange_INIT_FAIL;
                            }
                        }


                    }
                    else if (CcSysParams.CardChangeType.Value == EnumCardChangeType.CARRIER)
                    {
                        //CCSettingOP에 있는 Dock Sequence, Undock Sequence 초기화.
                        LoggerManager.Debug($"[GOP CC] Undock Seuquence Init");
                        if (CCUnDockBehaviors != null)
                        {
                            SequenceBehaviors undockBehaviors = this.CardChangeModule().CCUnDockBehaviors as SequenceBehaviors;
                            foreach (var beh in undockBehaviors.SequenceBehaviorCollection)
                            {
                                beh.StateEnum = SequenceBehaviorStateEnum.IDLE;
                            }
                        }
                        LoggerManager.Debug($"[GOP CC] Dock Seuquence Init");
                        if (CCDockBehaviors != null)
                        {
                            SequenceBehaviors dockBehaviors = this.CardChangeModule().CCDockBehaviors as SequenceBehaviors;
                            foreach (var beh in dockBehaviors.SequenceBehaviorCollection)
                            {
                                beh.StateEnum = SequenceBehaviorStateEnum.IDLE;
                            }
                        }


                        if (this.MotionManager().GetAxis(EnumAxisConstants.ROT) != null)
                        {
                            var retVal = this.MotionManager().EnableAxis(this.MotionManager().GetAxis(EnumAxisConstants.ROT));
                            if (retVal != 0)
                            {
                                ret = EventCodeEnum.GP_CardChange_INIT_FAIL;

                            }
                            else
                            {
                                ret = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            ret = EventCodeEnum.GP_CardChange_INIT_FAIL;
                            return ret;
                        }

                        bool isFls = false;

                        ret = this.MotionManager().IsFls(EnumAxisConstants.ROT, ref isFls);

                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[StageSystemInitFunc] Axis ROT isFls:{isFls} before homing");
                            return ret;
                        }
                        if (this.MotionManager().GetAxis(EnumAxisConstants.ROT).Status.AxisEnabled == false)
                        {
                            this.MotionManager().AmpFaultClear(EnumAxisConstants.ROT);
                            System.Threading.Thread.Sleep(500);
                            var motRet = this.MotionManager().EnableAxis(this.MotionManager().GetAxis(EnumAxisConstants.ROT));
                            if (motRet != 1)
                            {
                                LoggerManager.Debug($"ROT Axis Enable Failed.");
                                return ret;
                            }
                        }


                        if (isFls == false)
                        {

                            ret = this.MotionManager().HomingTaskRun(EnumAxisConstants.ROT);
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"ROT Axis homing failed.");
                                return ret;
                            }

                            ret = this.StageSupervisor().StageModuleState.LockCCState(); // to CCState
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"ROT Axis to CCState failed.");
                                return ret;
                            }

                            ret = this.StageSupervisor().StageModuleState.CCRotLock(60000);// homeIndex -> Fls
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"ROT Axis to CCRotLock failed.");
                                return ret;
                            }

                            ret = this.MotionManager().IsFls(EnumAxisConstants.ROT, ref isFls);
                            if (ret != EventCodeEnum.NONE)
                            {
                                if (isFls != true)
                                {
                                    LoggerManager.Debug("[StageSystemInitFunc] Axis ROT isFls Error");
                                    return ret;
                                }
                            }


                            ret = this.StageSupervisor().StageModuleState.UnLockCCState(); // to IDLE
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"ROT Axis to IDLE failed.");
                                return ret;
                            }
                            else
                            {
                                LoggerManager.Debug("[StageSystemInitFunc] Axis ROT Done");
                            }

                        }
                    }
                    else if (CcSysParams.CardChangeType.Value == EnumCardChangeType.UNDEFINED)
                    {
                        ret = EventCodeEnum.GP_CardChange_INIT_FAIL;

                        LoggerManager.Debug($"Check CardChangeType, Currenct Type Is  {CcSysParams.CardChangeType.Value}");
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                        LoggerManager.Debug($"Check CardChangeType, Currenct Type Is  {CcSysParams.CardChangeType.Value}");
                    }
                }
                else
                {
                    LoggerManager.Debug($"GPCardChangeInit Fail. GP_CheckCardPodOutputStatus ret : {ret}");
                }
            }
            catch (Exception err)
            {
                var errordata = this.StageSupervisor().ConvertToExceptionErrorCode(err);
            }

            return ret;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> BehaviorRun(ISequenceBehavior behavior)
        {
            Task<EventCodeEnum> ret = Task.FromResult<EventCodeEnum>(EventCodeEnum.UNDEFINED);
            IBehaviorResult result;
            try
            {

                if (GPCCBehavior != null || ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    ret = Task.FromResult<EventCodeEnum>(EventCodeEnum.GP_CardChange_ALREADY_RUNNING);
                }
                else
                {
                    //InnerStateTransition(new CardChangeRunningState(this));
                    //ModuleState.StateTransition(ModuleStateEnum.RUNNING);
                    GPCCBehavior = behavior;

                    if (behavior is GP_DockPCardTopPlate || behavior is GP_UndockPCardTopPlate)
                    {
                        if (this.LoaderController() != null && this.LoaderController() is GP_LoaderController)
                        {
                            (this.LoaderController() as GP_LoaderController).GPLoaderService.SetCardState((this.LoaderController() as GP_LoaderController).GetChuckIndex(), EnumWaferState.PROCESSED);
                        }
                    }
                    else if (behavior is GOP_DockPCardTopPlate || behavior is GOP_UndockPCardTopPlate)
                    {
                        if (this.LoaderController() != null && this.LoaderController() is GP_LoaderController)
                        {
                            if ((this.LoaderController() as GP_LoaderController).GPLoaderService != null)
                            {
                                EnumWaferState state = EnumWaferState.UNDEFINED;
                                this.LoaderController().UpdateCardStatus(out state);
                                (this.LoaderController() as GP_LoaderController).GPLoaderService.SetCardState((this.LoaderController() as GP_LoaderController).GetChuckIndex(), state);
                            }
                        }
                    }

                    result = await GPCCBehavior.Run();

                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        if (result.ErrorCode == EventCodeEnum.GP_CardChange_DOCK_PAUSED)
                        {
                            ret = Task.FromResult<EventCodeEnum>(result.ErrorCode);

                            if (behavior is GP_DockPCardTopPlate || behavior is GOP_DockPCardTopPlate)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("CardChanger Paused", "Paused Card Docking squence", EnumMessageStyle.Affirmative);
                            }
                            else if (behavior is GP_UndockPCardTopPlate || behavior is GOP_UndockPCardTopPlate)
                            {
                                await this.MetroDialogManager().ShowMessageDialog("CardChanger Paused", "Paused Card Docking squence", EnumMessageStyle.Affirmative);
                            }
                            LoggerManager.Debug($"[CC] Paused Card Docking squence : {result.ErrorCode}");
                        }
                        else
                        {
                            ret = Task.FromResult<EventCodeEnum>(result.ErrorCode);

                            if (behavior is GP_DockPCardTopPlate || behavior is GOP_DockPCardTopPlate)
                            {
                                string reason = null;
                                EventCodeEnum errorCodeEnum = EventCodeEnum.UNDEFINED;
                                returnFirstErrorInfo(result, out reason, out errorCodeEnum);
                                await this.MetroDialogManager().ShowMessageDialog("CardChanger Fail", $"Error Occured while Card Docking, SequenceDescription : {behavior.SequenceDescription}\n" +
                                    $"{errorCodeEnum}\n" + $"{reason}\n", EnumMessageStyle.Affirmative);
                            }
                            else if (behavior is GP_UndockPCardTopPlate || behavior is GOP_UndockPCardTopPlate)
                            {
                                string reason = null;
                                EventCodeEnum errorCodeEnum = EventCodeEnum.UNDEFINED;
                                returnFirstErrorInfo(result, out reason, out errorCodeEnum);
                                await this.MetroDialogManager().ShowMessageDialog("CardChanger Fail", $"Error Occured while Card UnDocking,  SequenceDescription : {behavior.SequenceDescription}\n" +
                                    $"{errorCodeEnum}\n" + $"{reason}\n", EnumMessageStyle.Affirmative);
                            }
                            LoggerManager.Debug($"[CC] Error Occured while BehavirRun Function ErrorCode : {result.ErrorCode}");
                        }
                    }
                    else
                    {
                        //GP CardChange Success
                        if (behavior is GP_DockPCardTopPlate)
                        {
                            this.EventManager().RaisingEvent(typeof(CardDockEvent).FullName);

                            this.EventManager().RaisingEvent(typeof(CardChangedEvent).FullName);

                            PIVInfo pivinfo = new PIVInfo(probecardid: CcSysParams.ProbeCardID.Value);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CardLoadingEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_SUCCESS);
                        }

                        //GOP CardChange Success
                        if (behavior is GOP_ClearCardDocking)
                        {
                            this.EventManager().RaisingEvent(typeof(CardChangedEvent).FullName);

                            this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_SUCCESS);
                        }

                        if (behavior is GOP_DockPCardTopPlate)
                        {
                            this.EventManager().RaisingEvent(typeof(CardDockEvent).FullName);

                        }

                        if(behavior is GP_UndockPCardTopPlate)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CardUnloadedEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();
                        }

                        ret = Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                        LoggerManager.Debug($"[CC] Success BehavirRun : {GPCCBehavior}");
                    }

                    if (behavior is GP_DockPCardTopPlate || behavior is GP_UndockPCardTopPlate || behavior is GOP_DockPCardTopPlate || behavior is GOP_UndockPCardTopPlate)
                    {
                        if (this.LoaderController() != null && this.LoaderController() is GP_LoaderController)
                        {
                            if((this.LoaderController() as GP_LoaderController).GPLoaderService != null)
                            {
                                EnumWaferState state = EnumWaferState.UNDEFINED;
                                this.LoaderController().UpdateCardStatus(out state);
                                (this.LoaderController() as GP_LoaderController).GPLoaderService.SetCardState((this.LoaderController() as GP_LoaderController).GetChuckIndex(), state);
                            }
                        }
                    }


                }

            }
            catch (Exception err)
            {
                ret = Task.FromResult<EventCodeEnum>(EventCodeEnum.EXCEPTION);
                LoggerManager.Exception(err);
            }
            finally
            {
                GPCCBehavior = null;
                if(ModuleState.GetState() != ModuleStateEnum.SUSPENDED)
                {
                    InnerStateTransition(new CardChangeIdleState(this));
                    ModuleState.StateTransition(ModuleStateEnum.IDLE);
                }
                else
                {
                    //suspendd 유지
                }                               
            }
            return ret.Result;

        }

        public EventCodeEnum CardIDValidate(string cardid, out string Msg)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            bool anyValidationFailed = false;
            Msg = string.Empty;

            try
            {
                string msg = "";
                var ValidateEnable = CIDSysParams.Enable;

                // ModeEnable이 켜진 경우
                if (ValidateEnable) 
                {
                    if (CIDSysParams.CardIDValidationsList != null && CIDSysParams.CardIDValidationsList.Count > 0)
                    {
                        foreach (var validation in CIDSysParams.CardIDValidationsList)
                        {
                            bool validationResult = validation.Validate(cardid, out msg);

                            if (!validationResult)
                            {
                                anyValidationFailed = true;
                                ret = EventCodeEnum.CARDCHANGE_VALIDATION_FAIL;
                                Msg += $"{msg}\n";
                            }
                        }
                        // 모든 검증이 성공적으로 통과한 경우
                        if (!anyValidationFailed)
                        {
                            ret = EventCodeEnum.NONE; 
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.CARDCHANGE_VALIDATION_FAIL;
                        Msg = $"CardID Validate Enable is true, but there is no CardID Validations List.";
                    }
                }
                // 꺼진 경우
                else
                {
                    ret = EventCodeEnum.NONE;
                    LoggerManager.Debug("[CardChangeModule] Validation mode is disabled. Skipping validation.");
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public EnumCardDockType GetCCDockType()
        {
            return CcSysParams.CardDockType.Value;
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum retval = ModuleStateEnum.ERROR;

            try
            {
                retval = Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ISequenceBehaviors GetSequence(ISequenceBehaviors sequence)
        {
            try
            {
                CCCommandParam = sequence;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return sequence;
        }
        public ISequenceBehavior GetNextBehavior()
        {
            ISequenceBehavior next_beh = null;
            try
            {
                if(CurBehaviorIdx < CCCommandParam.ISequenceBehaviorCollection.Count & 0 <= CurBehaviorIdx)
                {
                    CurBehaviorIdx++;
                    if(CurBehaviorIdx == CCCommandParam.ISequenceBehaviorCollection.Count)
                    {
                        next_beh = new END_Behavior();
                    }
                    else
                    {
                        next_beh = CCCommandParam.ISequenceBehaviorCollection[CurBehaviorIdx].IBehavior;
                    }
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return next_beh;
        }
        public ISequenceBehavior GetBehavior()
        {
            ISequenceBehavior curr_beh = null;
            if (CurBehaviorIdx < CCCommandParam.ISequenceBehaviorCollection.Count & 0 <= CurBehaviorIdx)
            {
                curr_beh = CCCommandParam.ISequenceBehaviorCollection[CurBehaviorIdx].IBehavior;
            }
            else
            {
                if(CurBehaviorIdx >= CCCommandParam.ISequenceBehaviorCollection.Count)
                {
                    curr_beh = new END_Behavior();
                    LoggerManager.Debug($"GetBehavior(): Last behavior. Index = {CurBehaviorIdx}");
                }
                else
                {
                    LoggerManager.Debug($"GetBehavior(): Curr. behavior index error. Index = {CurBehaviorIdx}");
                }
            }
            return curr_beh;
        }
        public ObservableCollection<ISequenceBehaviorGroupItem> GetCCCommandParamCollection()
        {
            ObservableCollection<ISequenceBehaviorGroupItem> curr_Collection = null;

            curr_Collection = CCCommandParam.ISequenceBehaviorCollection;

            return curr_Collection;
        }
        public ObservableCollection<ISequenceBehaviorRun> GetPreCheckBehavior()
        {
            ISequenceBehaviorGroupItem curr_beh = null;
            ObservableCollection<ISequenceBehaviorRun> preCheck_beh = null;
            if (CurBehaviorIdx < CCCommandParam.ISequenceBehaviorCollection.Count & 0 <= CurBehaviorIdx)
            {
                curr_beh = CCCommandParam.ISequenceBehaviorCollection[CurBehaviorIdx];
                if (curr_beh.IPreSafetyList != null)
                {
                    preCheck_beh = curr_beh.IPreSafetyList;
                }
                else
                {
                    preCheck_beh = null;
                }
            }
            return preCheck_beh;
        }
        public ObservableCollection<ISequenceBehaviorRun> GetPostCheckBehavior()
        {
            ISequenceBehaviorGroupItem curr_beh = null;
            ObservableCollection<ISequenceBehaviorRun> postCheck_beh = null;
            if (CurBehaviorIdx < CCCommandParam.ISequenceBehaviorCollection.Count & 0 <= CurBehaviorIdx)
            {
                curr_beh = CCCommandParam.ISequenceBehaviorCollection[CurBehaviorIdx];
                if (curr_beh.IPostSafetyList != null)
                {
                    postCheck_beh = curr_beh.IPostSafetyList;
                }
                else
                {
                    postCheck_beh = null;
                }
            }
            return postCheck_beh;
        }
        public void Dock_PauseCommand()
        {
            InnerStateTransition(new CardChangePauseState(this));
        }
        public void Dock_StepUpCommand()
        {
            InnerStateTransition(new CardChangeMaintenanceState(this));
        }
        public void Dock_ContinueCommand()
        {
            InnerStateTransition(new CardChangeMaintenanceState(this, true));
        }
        public void Dock_AbortCommand()
        {
            InnerStateTransition(new CardChangeAbortState(this));
        }

       
        public void UnDock_PauseCommand()
        {
            InnerStateTransition(new CardChangePauseState(this));
        }
        public void UnDock_StepUpCommand()
        {
            InnerStateTransition(new CardChangeMaintenanceState(this));
        }
        public void UnDock_ContinueCommand()
        {
            InnerStateTransition(new CardChangeMaintenanceState(this, true));
        }
        public void UnDock_AbortCommand()
        {
            InnerStateTransition(new CardChangeAbortState(this));
        }

        /// <summary>
        /// DeviceChange 이전 CardChange 된 후 맞는 카드라는것을 확인 받기 전까지 대기하는 함수.
        /// </summary>
        /// <returns></returns>
        public void WaitForCardPermission()
        {
            try
            {
                if(ModuleState.GetState() != ModuleStateEnum.SUSPENDED && 
                    (this.CardChangeModule().IsExistCard()))
                {
                    this.NotifyManager().Notify(EventCodeEnum.WAIT_FOR_CARD_PERMISSION_START);
                    LoggerManager.Debug($"[CardChangeModule] Card Authentication wait start.");
                    InnerStateTransition(new CardChangeSuspendedState(this));
                    ModuleState.StateTransition(ModuleStateEnum.SUSPENDED);
                }
                else
                {
                    //do nothing
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// DeviceChange 이전 CardChange 된 후 맞는 카드라는것을 확인 받기 전까지 대기하는 것을 풀어주는 함수
        /// 만약 대기 중인데 DeviceChange를 하지 않고 현재 사용하고 있는 카드를 정말 사용하고 싶은 카드를 인증하기 위해서는 PinAlign을 한다. 
        /// -- 아래 경우에 호출된다.
        /// DeviceChange 되었을 경우.
        /// PinAlign 성공했을 경우.
        /// CardChange Load할 경우.
        /// Wait 하는 파라미터를 Disable하는 경우.
        /// </summary>
        /// <returns></returns>
        public void ReleaseWaitForCardPermission()
        {
            try
            {
                if (ModuleState.GetState() == ModuleStateEnum.SUSPENDED)
                {
                    this.NotifyManager().Notify(EventCodeEnum.WAIT_FOR_CARD_PERMISSION_END);
                    LoggerManager.Debug($"[CardChangeModule] Card Authentication wait end.");
                    InnerStateTransition(new CardChangeIdleState(this));
                    ModuleState.StateTransition(ModuleStateEnum.IDLE);
                }
                else 
                {
                    //do nothing
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IBehaviorResult GetExecutionResult()
        {
            BehaviorResult behaviorResult = new BehaviorResult();

            if(ExecutionResult != null)
            {
                behaviorResult = ExecutionResult;
            }
            return behaviorResult;
        }
        public void SetExecutionResult(IBehaviorResult result)
        {
            if (ExecutionResult == null)
            {
                ExecutionResult = new BehaviorResult();
                
            }
            ExecutionResult = (BehaviorResult)result;
        }
        public void returnFirstErrorInfo(IBehaviorResult result, out string reason, out EventCodeEnum errorCodeEnum)
        {
            reason = null;
            errorCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                while (true)
                {
                    if (ExecutionResult == null)
                    {
                        reason = result.Reason;
                        errorCodeEnum = result.ErrorCode;
                        break;
                    }
                    else
                    {
                        if (ExecutionResult.InnerError == null)
                        {
                            reason = ExecutionResult.Reason;
                            errorCodeEnum = ExecutionResult.ErrorCode;
                            break;
                        }
                        else
                        {
                            ExecutionResult = (BehaviorResult)ExecutionResult.InnerError;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"returnFirstErrorInfo(): Error occurred. Err = {err.Message}");
            }
        }
        /// <summary>
        ///System Param에 있는 IsDocked를 변경하고자 할 때는 이 함수를 이용해야 로그가 남는다.
        /// </summary>
        /// <param name="IsDocked"></param>
        public void SetIsDocked(bool IsDocked)
        {
            try
            {
                LoggerManager.Debug($"SetIsDocked(): Value changed. CcSysParams.IsDocked {CcSysParams.IsDocked } to {IsDocked}");
                CcSysParams.IsDocked = IsDocked;
                SaveSysParameter();                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetIsDocked(): Error occurred. Err = {err.Message}");
            }
        }
        /// <summary>
        ///System Param에 있는 IsCardExist 변경하고자 할 때는 이 함수를 이용해야 로그가 남는다.
        /// </summary>
        /// <param name="IsCardExist"></param>
        public void SetIsCardExist(bool IsCardExist)
        {
            try
            {
                CcSysParams.IsCardExist = IsCardExist;
                SaveSysParameter();
                LoggerManager.Debug($"SetIsCardExist(): Value changed. CcSysParams.IsCardExist = {IsCardExist}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetIsCardExist(): Error occurred. Err = {err.Message}");
            }
        }
        /// <summary>
        /// Loader한테 Cell상태를 업데이트하는 함수.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum LoaderNotifyCardStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LoaderController() != null && this.LoaderController() is GP_LoaderController)
                {
                    if ((this.LoaderController() as GP_LoaderController).GPLoaderService != null)
                    {
                        EnumWaferState state = EnumWaferState.UNDEFINED;
                        this.LoaderController().UpdateCardStatus(out state);
                        (this.LoaderController() as GP_LoaderController).GPLoaderService.SetCardState((this.LoaderController() as GP_LoaderController).GetChuckIndex(), state);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Error($"LoaderNotifyCardStaus(): Error occurred. Err = {err.Message}");
            }
            return retVal;
        }

        public void SetCardDockingStatus(EnumCardDockingStatus status)
        {
            bool canSave = false;
            try
            {
                if(CardChangeStatusParams.CardDockingStatus.Value != status)
                {
                    canSave = true;
                }

                LoggerManager.Debug($"CardChangeModule.SetCardDockingStatus() Pre : {CardChangeStatusParams.CardDockingStatus.Value}, Cur : {status}");
                CardChangeStatusParams.CardDockingStatus.Value = status;

                if (canSave)
                {
                    SaveCardChangeStatusParam();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumCardDockingStatus GetCardDockingStatus()
        {
            EnumCardDockingStatus retVal = EnumCardDockingStatus.UNDOCKED;
            try
            {
                retVal = CardChangeStatusParams.CardDockingStatus.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// CC를 진행하기 위해서 현재 SV를 변경해야하는 지 판단하기 위한 함수
        /// </summary>
        /// <returns></returns>
        public bool NeedToSetCCActivatableTemp()
        {
            bool retVal = false;
            try
            {
                double sv = this.TempController().TempInfo.TargetTemp.Value;

                if (sv < GetCCActivatableTemp()                           
                    || sv != this.TempController().TempInfo.SetTemp.Value)
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
        /// 동작 시 온도 조건 판단, 온도 변경 API 호출
        /// </summary>
        /// <param name="setCCActiveTemp"> true 로 설정 된 경우에만 온도 조건 안맞을 때 온도를 설정 함, Default: 5, 외부에서 Token 값 관리하기, 처음에만 true이면됨. </param>
        object IsConditionSatisfiedLockObj = new object();
        public EventCodeEnum IsCCAvailableSatisfied(bool needToSetTempToken)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (IsConditionSatisfiedLockObj)
                {                    

                    double deviation = CcSysParams?.CCActiveTempDeviation?.Value ?? 1.0;
                    if (this.TempController().IsCurTempUpperThanSetTemp(CcSysParams?.CCActivatableTemp?.Value ?? 30, deviation))
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {                      
                        retVal = EventCodeEnum.TEMPERATURE_OUT_OF_RANGE;
                        if (NeedToSetCCActivatableTemp() && needToSetTempToken)
                        {
                            SetCCActiveTemp();
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

        public EventCodeEnum SetCCActiveTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.TempController().SetSV(TemperatureChangeSource.CARD_CHANGE, GetCCActivatableTemp(), overHeating: GetOverHeatingValue(), Hysteresis: GetOverHeattingHysteresis(), willYouSaveSetValue: false);
                LoggerManager.Debug($"[CardChangeModule] set temperature to CCActiveTemp. CCActiveTemp : {GetCCActivatableTemp()}, OverHeating : {GetOverHeatingValue()}, Hysteresis : {GetOverHeattingHysteresis()}");
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// SV를 CCActiveTemp 이외의 온도로 변경할 수 있는 지 확인 하는 함수
        /// </summary>
        /// <param name="doAction"></param>
        /// <returns></returns>
        public EventCodeEnum IsAvailableToSetOtherThanCCActiveTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // 카드 상태 확인
                bool isExistCard = IsExistCard();
                if (isExistCard && IsDocked)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.CARDCHANGE_CANNOT_RECOVER_PREV_TEMP_NOT_EXIST_CARD;
                }


                bool isCardDoorOpen = false;
                bool isFrontDoorOpen = false;
                bool isBackSideDoorOpen = false;
                //Check Front Door 상태 확인.

                if (this.CardChangeModule().GetCardDoorAttached())
                {
                    this.StageSupervisor().StageModuleState.IsCardDoorOpen(ref isCardDoorOpen);
                }
                
                this.StageSupervisor().StageModuleState.IsLoaderDoorOpen(ref isFrontDoorOpen);
                
                //Check Back Side Door
                var backsideIO = this.IOManager().IO.Inputs.DI_BACKSIDE_DOOR_OPEN;
                if (backsideIO.IOOveride.Value == EnumIOOverride.NONE)
                {
                    bool value = false;
                    var ioRetVal = this.IOManager().IOServ.ReadBit(backsideIO, out value);
                    if (ioRetVal == IORet.NO_ERR && value == true)
                    {
                        var ioRet = this.IOManager().IOServ.MonitorForIO(backsideIO, true, 2000, 0, false);
                        if (ioRet == 0)
                        {
                            isBackSideDoorOpen = true;
                        }
                    }
                }

                if (isCardDoorOpen || isFrontDoorOpen || isBackSideDoorOpen)
                {
                    retVal = EventCodeEnum.CARDCHANGE_CANNOT_RECOVER_PREV_TEMP_DOOR_OPENED;
                   
                }

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[CardChangeModule] IsAvailableToSetOtherThanCCActiveTemp() isExistCard:{isExistCard}, IsDocked:{IsDocked},isCardDoorOpen:{isCardDoorOpen},isFrontDoorOpen:{isFrontDoorOpen},isBackSideDoorOpen:{isBackSideDoorOpen}.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// SV를 CCActiveTemp로 변경할 수 있는 지 확인 하는 함수
        /// </summary>
        /// <param name="doAction"></param>
        /// <returns></returns>
        public EventCodeEnum RecoveryCCBeforeTemp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.TempController().GetCurrentTempInfoInHistory()?.TempChangeSource == TemperatureChangeSource.CARD_CHANGE && CcSysParams.RestoreSVAfterCardChange.Value)
                {
                    var checkAvailable = IsAvailableToSetOtherThanCCActiveTemp();
                    if (checkAvailable == EventCodeEnum.NONE)
                    {
                        this.TempController().RestorePrevSetTemp();
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = checkAvailable;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.CARDCHANGE_UNNECESSARY_RECOVER_TMPERATURE;
                }

                LoggerManager.Debug($"[CardChnageModule] RecoveryCCBeforeTemp() call SetLastSetTargetTemp(). result:{retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void CardChangedEventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is CardChangedEvent)
                {
                    bool isAutomationRunning = (this.LoaderController() as GP_LoaderController).GPLoaderService.IsActiveCCAllocatedState(this.LoaderController().GetChuckIndex());
                    if (isAutomationRunning)
                    {
                        RecoveryCCBeforeTemp();
                    }
                    else
                    {                        
                        if(IsAvailableToSetOtherThanCCActiveTemp() == EventCodeEnum.NONE)
                        {
                            //var tempBackup_cur = this.TempController().GetCurrentTempInfoInHistory();
                            //var tempBackup_prev = this.TempController().GetPreviousSourceTempInfoInHistory();

                            ////var endtask = false;
                            //EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;
                            //result = this.MetroDialogManager().ShowMessageDialog(
                            //                             "Recovery previous operating temperature",
                            //                             $"Do you want to restore to the previous operating temperature? \n" +
                            //                             $"current: SV {tempBackup_cur?.SetTemp:00.00}, PV {this.TempController().TempInfo.CurTemp.Value:00.00}, source: {tempBackup_cur?.TempChangeSource}\n" +
                            //                             $"previous: SV {tempBackup_prev?.SetTemp:00.00}, source: {tempBackup_prev?.TempChangeSource}\n" +
                            //                             $"*If there is no response for 10 seconds, the temperature will restore to the previous temperature.",
                            //                             EnumMessageStyle.AffirmativeAndNegative).Result;
                            ////endtask = true;
                            //if (result == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                RecoveryCCBeforeTemp();
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


        public void AbortCardChange()
        {
            try
            {
                RecoveryCCBeforeTemp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
