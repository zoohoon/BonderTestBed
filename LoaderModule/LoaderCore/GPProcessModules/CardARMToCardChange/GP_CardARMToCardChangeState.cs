using Autofac;
using CardIDManualDialog;
using LoaderBase;
using LoaderParameters;
using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace LoaderCore.GP_CardARMToCardChangeStates
{
    public abstract class GP_CardARMToCardChangeState : LoaderProcStateBase
    {
        public GP_CardARMToCardChange Module { get; set; }

        public GP_CardARMToCardChangeState(GP_CardARMToCardChange module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CardARMToCardChangeState stateObj)
        {
            try
            {

                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state transition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected ICardARMModule ARM => Module.Param.Curr as ICardARMModule;

        protected ICCModule CardChange => Module.Param.Next as ICCModule;

        protected ICardOwnable OriginHolder { get; set; }

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }
        public virtual EventCodeEnum NotifyWaferTransferResult(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum CardTransferDone(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum CardChangePut(out TransferObject transObj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StateTransition(new SystemErrorState(Module));
            }

            return retVal;
        }

        public virtual EventCodeEnum CardChangeCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum OriginCarrierPut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum OriginCardPut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum CardDockingDone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CardChange.Holder.TransferObject.WaferState = ProberInterfaces.EnumWaferState.READY;
                retVal = EventCodeEnum.NONE;
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum Card_LoadingPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual string GetProbeCardID()
        {
            string retVal = "";
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum GetUserCardIDInput(out string userCardIdInput)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            userCardIdInput = string.Empty;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class IdleState : GP_CardARMToCardChangeState
    {
        public IdleState(GP_CardARMToCardChange module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.CARDARM_TO_STAGE;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = CardChange.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
                OriginHolder = Loader.ModuleManager.FindModule(Loader.ProcModuleInfo.Origin) as ICardOwnable;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardARMToCardChangeState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                Loader.ChuckNumber = CardChange.ID.Index;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{ARM.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {CardChange.ToString()}");
                StateTransition(new WaitForRemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardARMToCardChangeState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RemotingState : GP_CardARMToCardChangeState
    {
        public RemotingState(GP_CardARMToCardChange module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute() { }

        public override EventCodeEnum NotifyWaferTransferResult(bool isSucceed)
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} NotifyWaferTransferResult() ReturnValue={isSucceed}");
                if (isSucceed)
                {
                  //  CardDockingDone();
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    Loader.ResonOfError = "CardArm To Chuck Error Or Pause.";
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum CardTransferDone(bool isSucceed)
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} CardTransferDone() ReturnValue={isSucceed}");

                var ccmodule = (ICCModule)Module.Param.DestPos as ICardOwnable;
                ccmodule.RecoveryCardStatus();

                if (isSucceed)
                {
                    //CardDockingDone();
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum CardDockingDone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CardChange.Holder.TransferObject.WaferState = ProberInterfaces.EnumWaferState.READY;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum CardChangePut(out TransferObject transObj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                LoggerManager.ActionLog(ModuleLogType.CARM_TO_STAGE, StateLogType.START, $"OriginHolder: {ARM.Holder.TransferObject?.OriginHolder}, Source: {ARM}, DestinationHolder: {CardChange}, ID: {ARM.Holder.TransferObject?.ProbeCardID.Value}", isLoaderMap: true);

                var ccsuper = Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                PIVInfo pIV = new PIVInfo() { ProbeCardID = ARM.Holder.TransferObject?.ProbeCardID.Value, StageNumber = ccsuper.GetRunningCCInfo().cardreqmoduleIndex };
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBusyEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                semaphore.Wait();
          

                transObj = ARM.Holder.TransferObject;
                retVal = this.GetLoaderCommands().CardChangerPut(Module.Param.UseARM, (ICCModule)Module.Param.DestPos);
                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"GP_CardARMToCardChangeState(): CardChangePut Transfer Done");
                    transObj.WaferState = ProberInterfaces.EnumWaferState.UNPROCESSED;
                    ARM.Holder.SetTransfered(CardChange);
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    LoggerManager.Error($"GP_CardARMToCardChangeState(): CardChangePut Transfer failed.{Environment.NewLine}Job result = {retVal}");

                    //  암 베큠 체크
                    retVal = ARM.MonitorForVacuum(true);
                    if(retVal == EventCodeEnum.NONE)
                    {
                        //Card가 Arm에 있다.
                        LoggerManager.Error($"GP_CardARMToCardChangeState(): The card remains in Arm. ARM.MonitorForVacuum(true) result = {retVal}");
                        StateTransition(new SystemErrorState(Module));

                    }
                    else
                    {
                        //Card가 Arm에 없다.
                        retVal = EventCodeEnum.LOADER_PUT_ERROR_BUT_NOTEXIST_IN_ARM;
                        LoggerManager.Error($"GP_CardARMToCardChangeState(): ARM.MonitorForVacuum(true) result = {retVal}");
                        //여기서 SystemErrorState로 안보내고 Cell에 있는지 확인할 때(SetTransferAfterCardChangePutError()) SystemErrorState처리 함.

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StateTransition(new SystemErrorState(Module));
            }
            return retVal;
        }
        public override EventCodeEnum SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            transObj = null;
            try
            {
                var ccsuper = Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                PIVInfo pIV = new PIVInfo() { ProbeCardID = ARM.Holder.TransferObject?.ProbeCardID.Value, StageNumber = ccsuper.GetRunningCCInfo().cardreqmoduleIndex };
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBusyEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                semaphore.Wait();

                transObj = ARM.Holder.TransferObject;
                transObj.WaferState = waferState;
                ARM.Holder.SetTransfered(CardChange);
                Loader.BroadcastLoaderInfo();
                retVal = EventCodeEnum.NONE;
                StateTransition(new SystemErrorState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StateTransition(new SystemErrorState(Module));
            }

            return retVal;
        }
        public override EventCodeEnum CardChangeCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.GetLoaderCommands().CardChangerPick((ICCModule)Module.Param.DestPos,Module.Param.UseARM,1);
                if (retVal == EventCodeEnum.NONE)
                {
                    ARM.AllocateCarrier();
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    Loader.ResonOfError = "CardChange Carrier Pick Error.";
                    LoggerManager.Error($"GP_CardARMToCardChangeState(): CardChangeCarrierPick Transfer failed. Job result = {retVal}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private static int carrier = 1; // carrier
        private static int holder = 2; // carrier + holder
        public override EventCodeEnum OriginCarrierPut()
        {
            return OriginalCardPut(carrier);
        }

        public override EventCodeEnum OriginCardPut()
        {
            return OriginalCardPut(holder);
        }
        
        private EventCodeEnum OriginalCardPut(int holderNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            
            try
            {
                
                OriginHolder = Loader.ModuleManager.FindModule(Loader.ProcModuleInfo.Origin) as ICardOwnable;
                PIVInfo pivinfo = new PIVInfo() { CardBufferIndex = OriginHolder.ID.Index };
                if (OriginHolder != null)
                {
                    ModuleTypeEnum originHoldertype = OriginHolder.ModuleType;
                    if (originHoldertype == ModuleTypeEnum.CARDBUFFER)
                    {
                        retVal = this.GetLoaderCommands().CardBufferPut(Module.Param.UseARM, (ICardBufferModule)OriginHolder, holderNumber);

                    }
                    else if (originHoldertype == ModuleTypeEnum.CARDTRAY)
                    {
                        retVal = this.GetLoaderCommands().CardTrayPut(Module.Param.UseARM, (ICardBufferTrayModule)OriginHolder, holderNumber);
                    }

                    if (retVal == EventCodeEnum.NONE)
                    {
                        if(OriginHolder.Holder.Status == EnumSubsStatus.CARRIER && ARM.Holder.Status == EnumSubsStatus.NOT_EXIST)
                        {
                            // skip 로그 
                            LoggerManager.Debug($"GP_CardARMToCardChangeState(): Skip SetTransfered. " +
                                                                                $"Already {OriginHolder.ModuleType}.{OriginHolder.ID.Index} {OriginHolder.Holder.Status} && " +
                                                                                $"{ARM.ModuleType}.{ARM.ID.Index} {ARM.Holder.Status}.");
                        }
                        else
                        {
                            LoggerManager.Debug($"GP_CardARMToCardChangeState(): Try SetTransfered. " +
                                                                                $"{OriginHolder.ModuleType}.{OriginHolder.ID.Index} {OriginHolder.Holder.Status} && " +
                                                                                $"{ARM.ModuleType}.{ARM.ID.Index} {ARM.Holder.Status}.");//모니터링용 로그
                            ARM.Holder.SetTransfered(OriginHolder);
                        }
                        
                        Loader.BroadcastLoaderInfo();

                        if(OriginHolder is CardBufferModule)
                        {
                            if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
                            {
                                (OriginHolder as CardBufferModule).UpdateCardBufferState();// forced 일 필요 없음. 
                            }

                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CardBufferLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                    }
                    else
                    {
                        Loader.ResonOfError = $"OriginHolder {originHoldertype} Put Error.";
                        LoggerManager.Error($"GP_CardARMToCardChangeState(): Origin {originHoldertype} Put Transfer failed. Job result = {retVal}");
                        StateTransition(new SystemErrorState(Module));
                        
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(CardBufferLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                }
                else
                {
                    Loader.ResonOfError = "OriginHolder Card Put Error.";
                    LoggerManager.Error($"GP_CardARMToCardChangeState(): OriginPut Transfer failed. OriginHoler is NUll");
                    StateTransition(new SystemErrorState(Module));


                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CardBufferLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(CardBufferLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }
            return retVal;
        }
        public override EventCodeEnum Card_LoadingPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.GetLoaderCommands().CardMoveLoadingPosition((ICCModule)Module.Param.DestPos, Module.Param.UseARM);
                if (retVal == EventCodeEnum.NONE)
                {
                }
                else
                {
                    //     CardChange.Holder.SetUnknown();
                    //    ARM.Holder.SetUnknown();
                    LoggerManager.Error($"Card_LoadingPosition(): Transfer failed. Job result = {retVal}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override string GetProbeCardID()
        {
            string retVal = "";
            try
            {
                TransferObject transObj = null;
                if (CardChange.Holder.TransferObject != null)
                {
                    transObj = CardChange.Holder.TransferObject;
                }
                else if (ARM.Holder.TransferObject != null)
                {
                    transObj = ARM.Holder.TransferObject;
                }
                else 
                {
                    LoggerManager.Error($"GetProbeCardID(): Failed to get Card ID.");
                }

                if (transObj != null)
                {
                    if (transObj.Type.Value == SubstrateTypeEnum.Card)
                    {
                        retVal = transObj.ProbeCardID.Value;
                    }
                    else
                    {
                        LoggerManager.Error($"GetProbeCardID(): Failed to get Card ID. Type : {transObj.Type.Value}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum GetUserCardIDInput(out string userCardIdInput)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            userCardIdInput = string.Empty;

            try
            {
                TransferObject transObj = null;

                
                if (ARM.Holder.TransferObject != null)
                {
                    transObj = ARM.Holder.TransferObject;
                }
                else
                {
                    LoggerManager.Error($"GetUserCardIDInput(): Failed to get Card ID.");

                    retVal = EventCodeEnum.NODATA;

                    return retVal;
                }

                TransferObject UsertransObj = transObj.Clone() as TransferObject;


                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        CardIDManualInput.Show(this.GetLoaderContainer(), UsertransObj);                                                                          
                        
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        retVal = EventCodeEnum.NODATA;
                    }
                });

                if (UsertransObj == null || string.IsNullOrWhiteSpace(UsertransObj.ProbeCardID?.Value))
                {
                    LoggerManager.Error($"CardIDManualInput is Null or White Space Error");
                    retVal = EventCodeEnum.CARDCHANGE_CARD_ID_NULL;
                }
                else
                {
                    Loader.LoaderMaster.CardIDLastTwoWord = UsertransObj.ProbeCardID.Value.Substring(UsertransObj.ProbeCardID.Value.Length - 2, 2);
                    retVal = EventCodeEnum.NONE;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    userCardIdInput = UsertransObj.ProbeCardID.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                userCardIdInput = string.Empty;
                retVal = EventCodeEnum.NODATA;
            }

            return retVal;
        }

    }

    public class WaitForRemotingState : GP_CardARMToCardChangeState
    {
        public WaitForRemotingState(GP_CardARMToCardChange module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.CARD_LOAD;

        bool firstFlag = true;
        public override void Execute()
        {
            if (firstFlag)
            {
                Loader.BroadcastLoaderInfo();
                firstFlag = false;
            }
        }

        public override void Resume()
        {
            StateTransition(new RemotingState(Module));
        }
    }

    public class RunningState : GP_CardARMToCardChangeState
    {
        public RunningState(GP_CardARMToCardChange module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum result = this.GetLoaderCommands().CardChangerPut(Module.Param.UseARM, (ICCModule)Module.Param.DestPos);
            if (result == EventCodeEnum.NONE)
            {
                ARM.Holder.SetTransfered(CardChange);
                Loader.BroadcastLoaderInfo();
                StateTransition(new DoneState(Module));

            }
            else
            {
                //   CardChange.Holder.SetUnknown();
                //   ARM.Holder.SetUnknown();
                LoggerManager.Error($"GP_CardARMToCardChangeState(): Transfer failed. Job result = {result}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }

    public class DoneState : GP_CardARMToCardChangeState
    {
        public DoneState(GP_CardARMToCardChange module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARM_TO_STAGE, StateLogType.DONE, $"OriginHolder: {CardChange.Holder.TransferObject?.OriginHolder}, Source: {ARM}, DestinationHolder: {CardChange}, ID: {CardChange.Holder.TransferObject?.ProbeCardID.Value}", isLoaderMap: true);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }

    public class SystemErrorState : GP_CardARMToCardChangeState
    {
        public SystemErrorState(GP_CardARMToCardChange module) : base(module)
        {
            try
            {
                TransferObject transObj = null;
                if (ARM.Holder.TransferObject != null)
                {
                    transObj = ARM.Holder.TransferObject;
                }
                else
                {
                    transObj = CardChange.Holder.TransferObject;
                }

                if (transObj != null)
                {
                    LoggerManager.ActionLog(ModuleLogType.CARM_TO_STAGE, StateLogType.ERROR, $"OriginHolder: {transObj.OriginHolder}, Source: {ARM}, DestinationHolder: {CardChange}, ID: {transObj.ProbeCardID.Value}", isLoaderMap: true);
                }
                else 
                {
                    LoggerManager.ActionLog(ModuleLogType.CARM_TO_STAGE, StateLogType.ERROR, $"Source: {ARM}, DestinationHolder: {CardChange}", isLoaderMap: true);
                }
                this.NotifyManager().Notify(EventCodeEnum.LOADER_CARM_TO_STAGE_TRANSFER_ERROR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
