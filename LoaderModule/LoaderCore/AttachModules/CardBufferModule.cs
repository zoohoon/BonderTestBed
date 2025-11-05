using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using ProberInterfaces.CardChange;
using NotifyEventModule;
using ProberInterfaces.Event;
using ProberInterfaces.Enum;
using System.Threading;
using System.Threading.Tasks;

namespace LoaderCore
{
    internal class CardBufferModule : AttachedModuleBase, ICardBufferModule
    {
        public event CardPresenceStateChangeEvent E84PresenceStateChangedEvent;
#pragma warning disable 0067
        //추후 사용될 수 있어 inteface에서 삭제하지 않고 일단 ignore 처리함
        public event CardBufferStateUpdateEvent CardBufferStateUpdateEvent;
#pragma warning restore 0067

        private IOPortDescripter<bool> DICARDONMODULE;

        private IOPortDescripter<bool> DICARRIERVAC;
        public IOPortDescripter<bool> GetDICARRIERVAC()
        {
            if(DICARRIERVAC != null)
            {
                return DICARRIERVAC;
            }
            else
            {
                return null;
            }
            
        }

        private IOPortDescripter<bool> DICARDATTACHMODULE;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.CARDBUFFER;

        public CardBufferDefinition Definition { get; set; }

        public CardBufferDevice Device { get; set; }

        public CardHolder Holder { get; set; }

        private CardPRESENCEStateEnum _CardPRESENCEState;

        public CardPRESENCEStateEnum CardPRESENCEState
        {
            get { return _CardPRESENCEState; }
            set { _CardPRESENCEState = value; }
        }


        public bool Enable { get; set; }
        public TransferObject GetSourceDeviceInfo()
        {
            TransferObject retval = null;

            try
            {
                retval = Device?.AllocateDeviceInfo ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDefinition(CardBufferDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = definition.Enable.Value;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                DICARDONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DICARDONMODULE.Value);
                if (DICARDONMODULE != null)
                {
                    DICARDONMODULE.PropertyChanged += DICARDONMODULE_PropertyChanged;
                    LoggerManager.Debug($"{ID}, DICARDONMODULE Add PropertyChanged Event");
                }


                if (Definition.DICARRIERVAC.Value != null)
                {
                    DICARRIERVAC = Loader.IOManager.GetIOPortDescripter(Definition.DICARRIERVAC.Value);
                    if(DICARRIERVAC != null)
                    {
                        DICARRIERVAC.PropertyChanged += DIDCARRIERVAC_PropertyChanged;
                    }
                }
            
                if (Definition.DICARDATTACHMODULE.Value != null)
                {
                    DICARDATTACHMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DICARDATTACHMODULE.Value);
                    if (DICARDATTACHMODULE != null)
                    {
                        DICARDATTACHMODULE.PropertyChanged += DICARDATTACHMODULE_PropertyChanged;
                    }
                }

                Holder = new CardHolder();
                Holder.SetOwner(this);

                RecoveryCardStatus();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum MonitorForCarrierVac(bool onCarrier)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(DICARRIERVAC, onCarrier, 100, Definition.IOCheckDelayTimeout.Value);
                LoggerManager.Debug($"CardBufferModule({ID.Index}) MonitorForCarrierVac(DICARRIERVAC, {onCarrier}) = {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        /// <summary>
        /// 캐리어가 있는지 센서로 확인하는 함수.
        /// </summary>
        /// <param name="waitforio">maintain, timeout time을 사용하고 싶을 때</param>
        /// <param name="targetvalue">monitorio에서 기대하는 값</param>
        /// <param name="value">현재 값</param>
        /// <returns></returns>
        public EventCodeEnum CheckCarrierExist(out bool value, bool waitforio = false, bool targetvalue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            value = false;  
            try
            {
                if (waitforio)
                {
                    retVal = Loader.IOManager.MonitorForIO(DICARDONMODULE, targetvalue, 100, Definition.IOCheckDelayTimeout.Value);
                    if(retVal == EventCodeEnum.NONE)
                    {
                        value = targetvalue;
                    }
                    else
                    {
                        value = DICARDONMODULE.Value;
                    }
                    
                }
                else
                {//maintain, timeout time을 사용하고 싶지 않을때.
                    value = DICARDONMODULE.Value;
                    retVal = EventCodeEnum.NONE;
                }
                
                LoggerManager.Debug($"CardBufferModule({ID.Index}) CheckCarrierExist(DICARDONMODULE, targetvalue: {targetvalue}) = value: {value}, retVal: {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        /// <summary>
        /// 카드(카드홀더)가 있는지 센서로 확인하는 함수.
        /// </summary>
        /// <param name="waitforio">maintain, timeout time을 사용하고 싶을 때</param>
        /// <param name="targetvalue">monitorio에서 기대하는 값</param>
        /// <param name="value">현재 값</param>
        /// <returns></returns>
        public EventCodeEnum CheckCardExist(out bool value, bool waitforio = false, bool targetvalue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            value = false;
            try
            {
                if (waitforio)
                {
                    retVal = Loader.IOManager.MonitorForIO(DICARRIERVAC, targetvalue, 100, Definition.IOCheckDelayTimeout.Value);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        value = targetvalue;
                    }
                    else
                    {
                        value = DICARRIERVAC.Value;
                    }
                }
                else
                {//maintain, timeout time을 사용하고 싶지 않을때.
                    value = DICARRIERVAC.Value;
                    retVal = EventCodeEnum.NONE;
                }

                LoggerManager.Debug($"CardBufferModule({ID.Index}) CheckCardExist(DICARRIERVAC, targetvalue: {targetvalue}) = value: {value}, retVal: {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void DICARDONMODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryCardStatus();
                UpdateCardBufferState();
                this.Loader.BroadcastLoaderInfo();
                LoggerManager.Debug($"DICARDONMODULE_PropertyChanged(): #{ID} Holder:{Holder.Status}, CardPRESENCEState:{CardPRESENCEState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DIDCARRIERVAC_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryCardStatus(); 
                UpdateCardBufferState();
                this.Loader.BroadcastLoaderInfo();
                LoggerManager.Debug($"DIDCARRIERVAC_PropertyChanged(): #{ID} Holder:{Holder.Status}, CardPRESENCEState:{CardPRESENCEState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DICARDATTACHMODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryCardStatus(); 
                UpdateCardBufferState();
                this.Loader.BroadcastLoaderInfo();
                LoggerManager.Debug($"DICARDATTACHMODULE_PropertyChanged(): #{ID} Holder:{Holder.Status}, CardPRESENCEState:{CardPRESENCEState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DICARDBOATMODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {                
                UpdateCardBufferState();
                this.Loader.BroadcastLoaderInfo();
                LoggerManager.Debug($"DICARDBOATMODULE_PropertyChanged(): {ID} CardPRESENCEState update to {CardPRESENCEState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanDistinguishCard()
        {
            bool retval = false;
            if(DICARDATTACHMODULE != null)
            {
                retval = true;
            }
            return retval;
        }


        /// <summary>
        /// MPT
        ///반사 = 카드 = DICARDATTACHMODULE = DICardExistSensorInBuffer = Card상태 결정 X, PGV에서만 (ATTACH/DETACH)
        ///베큠 = 홀더 = DICARRIERVAC = "DICardOnCarrierVacs.0" = 홀더(CARD: EXIST/NOT_EXIST)
        ///터치 = 캐리어 = DICARDONMODULE =  "DICardBuffs.0" = 캐리어(CARRIER)
        ///
        /// STMGetLoaderCommands
        ///반사 = 홀더 = DICARRIERVAC = 홀더(CARD: EXIST/NOT_EXIST)
        ///반사 = 캐리어 = DICARDONMODULE = 캐리어(CARRIER)
        ///
        /// Opera
        /// 카드 = DICARDONMODULE = 카드(EXIST)
        /// 
        /// 카드 있을때(초록생 EXIST) = 카드 + 홀더 + 캐리어 
        /// 카드홀더 있을때(노란색 EXIST) = 홀더 + 캐리어 - 카드와 카드홀더 구분이 가능할 경우에만 그렇지 않은 경우에는 초록색 EXIST
        /// 캐리어 있을때(보라색 CARRIER) = 캐리어 
        /// 아무것도 없을때(회색 NOT_EXIST) = null  
        ///
        /// forced_event: 카드암이 모두 동작후에 이벤트 다시 올려주기 위해서 만듦. 함수가 호출된 시점과 CardPRESENCEState가 다르지 않더라도 이벤트 올려줌.
        /// </summary>
        public void UpdateCardBufferState(bool forced_event = false, CardPRESENCEStateEnum forced_presence = CardPRESENCEStateEnum.UNDEFINED)
        {
            try
            {
                int hash = DateTime.Now.ToString().GetHashCode();
                var prevState = CardPRESENCEState;
                EventCodeEnum isCarrierExist = EventCodeEnum.NONE;
                EventCodeEnum isHolderExist = EventCodeEnum.NONE;
                EventCodeEnum isCardExist = EventCodeEnum.NONE;

                var ccsuper = this.Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                var pivinfo = new PIVInfo(cardbufferindex: ID.Index, stagenumber: ccsuper.GetRunningCCInfo().cardreqmoduleIndex);
                //아래 부분 DICARRIERVAC 과 DICARDATTACHMODULE 의 유무에 따른 다른 동작 필요.


                if (DICARDONMODULE != null && DICARRIERVAC != null && DICARDATTACHMODULE != null)
                {

                    if ((DICARRIERVAC.Value && DICARDONMODULE.Value && DICARDATTACHMODULE.Value) || forced_presence == CardPRESENCEStateEnum.CARD_ATTACH)
                    {
                        isCarrierExist = Loader.IOManager.MonitorForIO(DICARRIERVAC, true, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
                        isHolderExist = Loader.IOManager.MonitorForIO(DICARDONMODULE, true, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
                        isCardExist = Loader.IOManager.MonitorForIO(DICARDATTACHMODULE, true, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);

                        if ((isCarrierExist == EventCodeEnum.NONE && isHolderExist == EventCodeEnum.NONE && isCardExist == EventCodeEnum.NONE) || forced_presence == CardPRESENCEStateEnum.CARD_ATTACH)
                        {
                            prevState = CardPRESENCEState;

                            if (CardPRESENCEState != CardPRESENCEStateEnum.CARD_ATTACH)
                            {
                                CardPRESENCEState = CardPRESENCEStateEnum.CARD_ATTACH;
                                LoggerManager.Debug($"UpdateCardBufferState({hash}): #{ID.Index} Card PRESENCEState Change {prevState} to CARD_ATTACH");
                            }

                        }
                        else
                        {
                            LoggerManager.Debug($"UpdateCardBufferState({hash}) isCarrierExist : {isCarrierExist}, isHolderExist : {isHolderExist}, isCardExist : {isCardExist}");
                        }
                    }
                    else if ((DICARRIERVAC.Value && DICARDONMODULE.Value && DICARDATTACHMODULE.Value == false) || forced_presence == CardPRESENCEStateEnum.CARD_DETACH)
                    {
                        isCarrierExist = Loader.IOManager.MonitorForIO(DICARRIERVAC, true, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
                        isHolderExist = Loader.IOManager.MonitorForIO(DICARDONMODULE, true, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
                        isCardExist = Loader.IOManager.MonitorForIO(DICARDATTACHMODULE, false, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
                        if ((isCarrierExist == EventCodeEnum.NONE && isHolderExist == EventCodeEnum.NONE && isCardExist == EventCodeEnum.NONE) || forced_presence == CardPRESENCEStateEnum.CARD_DETACH)
                        {
                            if (Holder.TransferObject != null)
                            {
                                Holder.TransferObject.ProbeCardID.Value = string.Empty;
                            }

                            LoggerManager.Debug($"UpdateCardBufferState({hash}): Card Id Cleared. ");
                            prevState = CardPRESENCEState;

                            if (CardPRESENCEState != CardPRESENCEStateEnum.CARD_DETACH)
                            {
                                CardPRESENCEState = CardPRESENCEStateEnum.CARD_DETACH;
                                LoggerManager.Debug($"#UpdateCardBufferState({hash}): {ID.Index} Card PRESENCEState {prevState} Change to CARD_DETACH");
                            }

                        }
                        else
                        {
                            LoggerManager.Debug($"UpdateCardBufferState({hash}) isCarrierExist : {isCarrierExist}, isHolderExist : {isHolderExist}, isCardExist : {isCardExist}");
                        }
                    }
                    else
                    {
                        prevState = CardPRESENCEState;

                        if (CardPRESENCEState != CardPRESENCEStateEnum.EMPTY)
                        {
                            // ** CardBuffer Put 할때 센서 3개 인식 되면서 EMPTY, ATTACH, ATTACH 되면서 인식되서 clear됨. 
                            //if(Holder.TransferObject != null)
                            //{
                            //    Holder.TransferObject.ProbeCardID.Value = string.Empty;
                            //    LoggerManager.Debug($"UpdateCardBufferState({CardPRESENCEState}): Card Id Cleared.");
                            //}                                                                        
                            CardPRESENCEState = CardPRESENCEStateEnum.EMPTY;
                            LoggerManager.Debug($"UpdateCardBufferState({hash}): #{ID.Index} Card PRESENCEState {prevState} Change to EMPTY");
                        }
                        LoggerManager.Debug($"UpdateCardBufferState({hash}): CardPRESENCEState:{CardPRESENCEState}, (DICARRIERVAC:{DICARRIERVAC.Value}, DICARDONMODULE:{DICARDONMODULE.Value}, DICARDATTACHMODULE:{DICARDATTACHMODULE.Value})");
                    }

                }
                else//DICARDONMODULE 센서만 있다고 판단.
                {
                    if (DICARDONMODULE.Value || forced_presence == CardPRESENCEStateEnum.CARD_ATTACH)
                    {
                        isCardExist = Loader.IOManager.MonitorForIO(DICARDONMODULE, true, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);                        

                        if ((isCardExist == EventCodeEnum.NONE) || forced_presence == CardPRESENCEStateEnum.CARD_ATTACH)
                        {
                            prevState = CardPRESENCEState;

                            if (CardPRESENCEState != CardPRESENCEStateEnum.CARD_ATTACH)
                            {
                                CardPRESENCEState = CardPRESENCEStateEnum.CARD_ATTACH;
                                LoggerManager.Debug($"UpdateCardBufferState({hash}): #{ID.Index} Card PRESENCEState {prevState} Change to CARD_ATTACH");
                            }

                        }
                        else
                        {
                            LoggerManager.Debug($"UpdateCardBufferState({hash}) isCardExist : {isCardExist}");
                        }
                    }
                    else
                    {
                        prevState = CardPRESENCEState;

                        if (CardPRESENCEState != CardPRESENCEStateEnum.EMPTY)
                        {
                            // ** CardBuffer Put 할때 센서 3개 인식 되면서 EMPTY, ATTACH, ATTACH 되면서 인식되서 clear됨. 
                            //if(Holder.TransferObject != null)
                            //{
                            //    Holder.TransferObject.ProbeCardID.Value = string.Empty;
                            //    LoggerManager.Debug($"UpdateCardBufferState({CardPRESENCEState}): Card Id Cleared.");
                            //}                                                                        
                            CardPRESENCEState = CardPRESENCEStateEnum.EMPTY;
                            LoggerManager.Debug($"UpdateCardBufferState({hash}): #{ID.Index} Card PRESENCEState {prevState} Change to EMPTY");
                        }
                        LoggerManager.Debug($"UpdateCardBufferState({hash}): CardPRESENCEState:{CardPRESENCEState}, (DICARDONMODULE:{DICARDONMODULE.Value})");
                    }
                }


           

                if(prevState != CardPRESENCEState)
                {
                    if (Holder.TransferObject != null)
                    {
                        if (CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH) // 센서 차례로 인식되면서 card id 지워버림
                        {
                            Loader.LoaderMaster.GEMModule().GetPIVContainer().SetCardBufferCardId(ID.Index, Holder.TransferObject.ProbeCardID.Value);
                            Loader.LoaderMaster.GEMModule().GetPIVContainer().UpdateCardBufferInfo(ID.Index);
                        }
                        else
                        {
                            Loader.LoaderMaster.GEMModule().GetPIVContainer().SetCardBufferCardId(ID.Index, string.Empty);// clear하는것이 맞는가?
                            Loader.LoaderMaster.GEMModule().GetPIVContainer().UpdateCardBufferInfo(ID.Index);
                        }
                    }
                    else
                    {
                        Loader.LoaderMaster.GEMModule().GetPIVContainer().SetCardBufferCardId(ID.Index, string.Empty);// clear하는것이 맞는가?
                        Loader.LoaderMaster.GEMModule().GetPIVContainer().UpdateCardBufferInfo(ID.Index);
                    }
                }
                


                if (forced_event == true || prevState != CardPRESENCEState)
                {
                    if (forced_event)
                    {
                        LoggerManager.Debug($"UpdateCardBufferState({hash}): Forced Rising Event.");
                    }

                    if (CardPRESENCEState == CardPRESENCEStateEnum.EMPTY)
                    {
                        // 카드+캐리어를 를 통째로 제거 했다가 다시 카드+케리어를 통째로 놓을 수 있기때문에 clear하는게 맞다고 생각.
                        if (forced_event == false)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBufferRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                       
                        ChangeCardLPServiceStatus(GEMFoupStateEnum.TRANSFER_BLOCKED);
                    }
                    else if (CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH)
                    {
                        if (forced_event == false)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBufferDetectedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                       

                        // 만약 로더가 put하는 중이라면 아직 readyToUnload를 부르면 안됨. 하지만 그게 아니라면 여기서 readyToload를 해도됨. 
                        //put하는 중인지는 어떻게 알지? 사람이 올려놨을수도 있자낭 일단 여기가 맞는것 같긴 한데 고민 필요.

                        var carde84 = Loader.LoaderMaster.E84Module().GetE84Controller(ID.Index, E84OPModuleTypeEnum.CARD);                        
                        var result = this.Loader.GetLoaderCommands().IsMovingOnCardOwner();
                        if (result == false && carde84?.GetModuleStateEnum() != ModuleStateEnum.RUNNING)
                        {
                            ChangeCardLPServiceStatus(GEMFoupStateEnum.TRANSFER_READY);
                            ChangeCardLPServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);                          
                        }
                        else
                        {
                            LoggerManager.Debug($"UpdateCardBufferState({hash}): Ignored Rising Event, loader moving({result}), e84 state({carde84?.GetModuleStateEnum()})");
                        }                   
                    }
                    else if (CardPRESENCEState == CardPRESENCEStateEnum.CARD_DETACH)
                    {                        
                        if(forced_event == false) 
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBufferRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        

                        var carde84 = Loader.LoaderMaster.E84Module().GetE84Controller(ID.Index, E84OPModuleTypeEnum.CARD);
                        var result = this.Loader.GetLoaderCommands().IsMovingOnCardOwner();
                        if (result == false && carde84 != null && carde84?.GetModuleStateEnum() != ModuleStateEnum.RUNNING)
                        {
                            ChangeCardLPServiceStatus(GEMFoupStateEnum.TRANSFER_READY);
                            ChangeCardLPServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);                            
                        }
                        else
                        {
                            LoggerManager.Debug($"UpdateCardBufferState({hash}): Ignored Rising Event, loader moving({result}), e84 state({carde84?.GetModuleStateEnum()})");
                        }
                    }
                    else
                    {
                        // error
                        LoggerManager.Error($"UpdateCardBufferState({hash}): Undefined CardBuffer State.");
                        ChangeCardLPServiceStatus(GEMFoupStateEnum.TRANSFER_BLOCKED);
                    }

                    if (E84PresenceStateChangedEvent != null)
                    {
                        E84PresenceStateChangedEvent();
                    }

                    this.Loader.BroadcastLoaderInfo();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        public OCRModeEnum NeedToReadCardId()
        {
            OCRModeEnum ret = OCRModeEnum.NONE;
            try
            {
                if (Holder.TransferObject != null)
                {
                    if (CardPRESENCEState == CardPRESENCEStateEnum.CARD_ATTACH)
                    {
                        ret = OCRModeEnum.READ;
                    }                    
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }


        public void ChangeCardLPServiceStatus(GEMFoupStateEnum state, bool forcewrite = false, PIVInfo pivinfo = null)
        {
            try
            {
                ILoaderSupervisor master = this.Loader.LoaderMaster;
                ICardChangeSupervisor cardChangeSupervisor = master.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                var pivcontainer = master.GEMModule().GetPIVContainer();
                var curCardBufferState = pivcontainer.GetCardBufferState(ID.Index);

                if (curCardBufferState != state)
                {
                    if(pivinfo == null)
                    {
                        pivinfo = new PIVInfo(cardbufferindex: ID.Index, stagenumber: cardChangeSupervisor.GetRunningCCInfo().cardreqmoduleIndex);
                    }

                    pivcontainer.SetCardBufferState(ID.Index, state);

                    if (master.E84Module()?.GetE84Controller(ID.Index, E84OPModuleTypeEnum.CARD)?.ModulestateEnum == ModuleStateEnum.RUNNING)
                    {
                        LoggerManager.Debug($"[CardBufferModule] ChangeCardLPServiceStatus(): E84Controller State is Running. Skip event. prev:{curCardBufferState}, cur:{state}");
                        return;
                    }

                    if (state == GEMFoupStateEnum.IN_SERVICE)
                    {                        
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        master.EventManager().RaisingEvent(typeof(CardLPInServiceEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else if (state == GEMFoupStateEnum.OUT_OF_SERVICE)
                    {                        
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        master.EventManager().RaisingEvent(typeof(CardLPOutOfServiceEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }                    
                    else if (state == GEMFoupStateEnum.TRANSFER_READY)
                    {
                        if ((curCardBufferState != GEMFoupStateEnum.READY_TO_LOAD &&
                            curCardBufferState != GEMFoupStateEnum.READY_TO_UNLOAD) || (forcewrite == true))
                        {                          
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            master.EventManager().RaisingEvent(typeof(CardLPTransferReady).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                    }
                    else if (state == GEMFoupStateEnum.READY_TO_LOAD)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        master.EventManager().RaisingEvent(typeof(CardLPReadyToLoadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else if (state == GEMFoupStateEnum.READY_TO_UNLOAD)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        master.EventManager().RaisingEvent(typeof(CardLPReadyToUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        if(Holder.TransferObject != null)
                        {
                            if(Holder.TransferObject.ProbeCardID.Value == string.Empty || Holder.TransferObject.ProbeCardID.Value == null)
                            {
                                semaphore = new SemaphoreSlim(0);
                                master.EventManager().RaisingEvent(typeof(CardLPReadyToUnloadNewCardEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                            }
                            
                        }
                    }
                    else if (state == GEMFoupStateEnum.TRANSFER_BLOCKED)
                    {                       
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        master.EventManager().RaisingEvent(typeof(CardLPTransferBlockedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
     

        public EventCodeEnum SetDevice(CardBufferDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Device = device;

                if (string.IsNullOrEmpty(device.Label.Value) == false)
                    this.ID = ModuleID.Create(ID.ModuleType, ID.Index, device.Label.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ChangeCardLPServiceStatus(GEMFoupStateEnum.OUT_OF_SERVICE);
                    RecoveryCardStatus();                    
                    ChangeCardLPServiceStatus(GEMFoupStateEnum.IN_SERVICE);
                    

                    UpdateCardBufferState();
                    Initialized = false;
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override void DeInitModule()
        {

        }

        public EventCodeEnum MonitorForSubstrate(bool onTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(DICARDONMODULE, onTray, 100, Definition.IOCheckDelayTimeout.Value);
                LoggerManager.Debug($"CardBufferModule({ID.Index}) MonitorForSubstrate(DICARDONMODULE, {onTray}) = {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum MonitorForCardAttachHolder(bool onHoler)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (DICARDATTACHMODULE == null)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Loader.IOManager.MonitorForIO(DICARDATTACHMODULE, onHoler, 100, Definition.IOCheckDelayTimeout.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        object cardlockobj = new object();

        public EventCodeEnum RecoveryCardStatus()
        {
            EventCodeEnum subOnTrayRetVal = EventCodeEnum.UNDEFINED;
            EventCodeEnum subOnCarrierVacRetVal = EventCodeEnum.UNDEFINED;

            try
            {
                lock (cardlockobj)
                {
                    bool isAcess = true;                    

                    if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                    {
                        if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDBUFFER)
                        {
                            if (Loader.ProcModuleInfo.Destnation.Index == ID.Index)
                            {
                                isAcess = false;
                            }
                        }
                        else if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDBUFFER_TO_CARDARM)
                        {
                            if (Loader.ProcModuleInfo.Source.Index == ID.Index)
                            {
                                isAcess = false;
                            }
                        }
                        //carrier type 이면 carrier를 다시 반환하기 때문에 exception 발생할수 있으나 ProcModuled에서 현재 비교할수 있는 cardbuffer Index의 근거를 찾을수 없으므로 무조건 isAccess false 처리 할수 없음.                                                                   
                        //else if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_STAGE)
                        //{                                                     
                        //    isAcess = false;                        
                        //}




                        subOnTrayRetVal = EventCodeEnum.NONE;
                    }

                    if (isAcess)
                    {
                        LoggerManager.Debug($"RecoveryCardStatus(): Before CardBufferModule({ID.Index}) Holder.Status:{Holder.Status}, CardPresenceState:{CardPRESENCEState}");

                        if (DICARRIERVAC == null)// 현재 오페라에서는 이쪽 코드를 타고 있음.
                        {
                            if (Holder.Status == EnumSubsStatus.UNDEFINED || Holder.Status == EnumSubsStatus.CARRIER)
                            {
                                subOnTrayRetVal = MonitorForSubstrate(false);

                                //Check no wafer on module.
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetUnload();
                                }
                                else
                                {
                                    //Check wafer on module.
                                    subOnTrayRetVal = MonitorForSubstrate(true);
                                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                                    {
                                        if (DICARRIERVAC != null)
                                        {
                                            subOnCarrierVacRetVal = Loader.GetLoaderCommands().SetCardTrayVac(true);
                                            if (subOnCarrierVacRetVal == EventCodeEnum.NONE)
                                            {
                                                subOnCarrierVacRetVal = MonitorForCarrierVac(true);
                                                if (subOnCarrierVacRetVal == EventCodeEnum.NONE)
                                                {
                                                    Holder.SetAllocate();

                                                }
                                                else
                                                {
                                                    subOnCarrierVacRetVal = Loader.GetLoaderCommands().SetCardTrayVac(false);
                                                    Holder.SetAllocateCarrier();
                                                }
                                            }
                                            else
                                            {
                                                Holder.SetUnknown();
                                            }
                                        }
                                        else
                                        {
                                            Holder.SetAllocate();
                                        }
                                    }
                                    else
                                    {
                                        Holder.SetUnknown();
                                    }
                                }
                            }
                            else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                            {
                                //Check no wafer on module.
                                subOnTrayRetVal = MonitorForSubstrate(false);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {

                                    //Holder.SetAllocate();
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                            else if (Holder.Status == EnumSubsStatus.EXIST)
                            {
                                //Check wafer on module.
                                subOnTrayRetVal = MonitorForSubstrate(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    if (DICARRIERVAC != null)
                                    {
                                        subOnCarrierVacRetVal = Loader.GetLoaderCommands().SetCardTrayVac(true);
                                        if (subOnCarrierVacRetVal == EventCodeEnum.NONE)
                                        {
                                            subOnCarrierVacRetVal = MonitorForCarrierVac(true);
                                            if (subOnCarrierVacRetVal == EventCodeEnum.NONE)
                                            {

                                            }
                                            else
                                            {
                                                subOnCarrierVacRetVal = Loader.GetLoaderCommands().SetCardTrayVac(false);
                                                Holder.SetAllocateCarrier();
                                            }
                                        }
                                        else
                                        {
                                            Holder.SetUnknown();
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                            else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                            {
                                //Check no wafer on module.
                                //** Unknwon상태에서는 사용자가 직접 제거해야 한다.
                                subOnTrayRetVal = MonitorForSubstrate(false);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetUnload();
                                }
                                else
                                {
                                    if (DICARRIERVAC != null)
                                    {
                                        subOnCarrierVacRetVal = Loader.GetLoaderCommands().SetCardTrayVac(true);
                                        if (subOnCarrierVacRetVal == EventCodeEnum.NONE)
                                        {
                                            subOnCarrierVacRetVal = MonitorForCarrierVac(true);
                                            if (subOnCarrierVacRetVal == EventCodeEnum.NONE)
                                            {
                                                Holder.SetAllocate();

                                            }
                                            else
                                            {
                                                subOnCarrierVacRetVal = Loader.GetLoaderCommands().SetCardTrayVac(false);
                                                Holder.SetAllocateCarrier();
                                            }
                                        }
                                        else
                                        {
                                            Holder.SetUnknown();
                                        }
                                    }
                                    else
                                    {
                                        Holder.SetAllocate();
                                    }
                                }
                            }
                            else
                            {
                                throw new NotImplementedException("InitWaferStatus()");
                            }
                        }
                        else
                        {                           
                            if (DICARRIERVAC.Value == false)
                            {
                                Loader.GetLoaderCommands().SetCardTrayVac(true);
                            }

                            bool cardexist = false;
                            bool carrierexist = false;

                            EventCodeEnum cardexistrst = EventCodeEnum.UNDEFINED;
                            EventCodeEnum carrierexistrst = EventCodeEnum.UNDEFINED;

                            CheckCardExist(out cardexist);
                            CheckCarrierExist(out carrierexist);

                            if (cardexist == true && carrierexist == true)
                            {
                                LoggerManager.Debug($"RecoveryCardStatus(): TargetValue cardexist: true, carrierexist: true");
                                cardexistrst = CheckCardExist(out cardexist, waitforio: true, targetvalue: true);
                                carrierexistrst = CheckCarrierExist(out carrierexist, waitforio: true, targetvalue: true);
                                if (cardexistrst == EventCodeEnum.NONE && carrierexistrst == EventCodeEnum.NONE)//EXIST
                                {
                                    Holder.SetAllocate();
                                    Holder.isCardAttachHolder = MonitorForCardAttachHolder(true) == EventCodeEnum.NONE ? true : false;
                                    LoggerManager.Debug($"RecoveryCardStatus(): Set Holder.Status:{Holder.Status}");
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                    LoggerManager.Debug($"RecoveryCardStatus(): Not Matched. Set Holder.Status:{Holder.Status}");
                                }

                            }
                            else if (cardexist == false && carrierexist == true)
                            {
                                LoggerManager.Debug($"RecoveryCardStatus(): TargetValue cardexist: false, carrierexist: true");
                                cardexistrst = CheckCardExist(out cardexist, waitforio: true, targetvalue: false);
                                carrierexistrst = CheckCarrierExist(out carrierexist, waitforio: true, targetvalue: true);
                                if (cardexistrst == EventCodeEnum.NONE && carrierexistrst == EventCodeEnum.NONE)//CARRIER
                                {
                                    Holder.SetAllocateCarrier();
                                    LoggerManager.Debug($"RecoveryCardStatus(): Set Holder.Status:{Holder.Status}");
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                    LoggerManager.Debug($"RecoveryCardStatus(): Not Matched. Set Holder.Status:{Holder.Status}");
                                }

                            }
                            else if (cardexist == false && carrierexist == false)
                            {
                                LoggerManager.Debug($"RecoveryCardStatus(): TargetValue cardexist: false, carrierexist: false");
                                cardexistrst = CheckCardExist(out cardexist, waitforio: true, targetvalue: false);
                                carrierexistrst = CheckCarrierExist(out carrierexist, waitforio: true, targetvalue: false);

                                if (cardexistrst == EventCodeEnum.NONE && carrierexistrst == EventCodeEnum.NONE)//NOT_EXIST
                                {
                                    Holder.SetUnload();
                                    LoggerManager.Debug($"RecoveryCardStatus(): Set Holder.Status:{Holder.Status}");
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                    LoggerManager.Debug($"RecoveryCardStatus(): Not Matched. Set Holder.Status:{Holder.Status}");
                                }
                            }
                            else
                            {
                                Holder.SetUnknown();
                                LoggerManager.Debug($"RecoveryCardStatus(): Set Holder.Status:{Holder.Status}");

                            }

                            LoggerManager.Debug($"RecoveryCardStatus(): CurValue cardexist:{cardexist}, cardexistrst:{cardexistrst}, carrierexist:{carrierexist}, carrierexistrst:{carrierexistrst}");
                        }
                        LoggerManager.Debug($"RecoveryCardStatus(): After CardBufferModule({ID.Index}) Holder.Status:{Holder.Status}, CardPresenceState:{CardPRESENCEState}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return subOnTrayRetVal;
        }

        public void ValidateCardStatus()
        {
            try
            {
                bool isExistObj = Holder.Status == EnumSubsStatus.EXIST;

                //=> get iostate
                EventCodeEnum ioRetVal;
                ioRetVal = MonitorForSubstrate(isExistObj);

                if (isExistObj)
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        //obj : exist, io : exist
                        //No changed.
                    }
                    else
                    {
                        //obj : exist, io : not exist
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        //obj : not exist, io : not exist
                        //No changed.
                    }
                    else
                    {
                        //obj : not exist, io : exist
                        //status error - unknown detected.
                        Holder.SetUnknown();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public CardBufferAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            CardBufferAccessParam system = null;

            try
            {
                system = Definition.AccessParams
               .Where(
               item =>
               item.SubstrateType.Value == type &&
               item.SubstrateSize.Value == size
               ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return system;
        }

    }
}
