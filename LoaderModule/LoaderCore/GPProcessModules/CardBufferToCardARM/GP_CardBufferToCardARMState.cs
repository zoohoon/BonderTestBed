using Autofac;
using LoaderBase;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Event;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoaderCore.GP_CardBufferToCardARMStates
{
    public abstract class GP_CardBufferToCardARMState : LoaderProcStateBase
    {
        public GP_CardBufferToCardARM Module { get; set; }

        public GP_CardBufferToCardARMState(GP_CardBufferToCardARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CardBufferToCardARMState stateObj)
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

        protected ICardARMModule ARM => Module.Param.Next as ICardARMModule;

        protected ICardBufferModule CardBuffer => Module.Param.Curr as ICardBufferModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_CardBufferToCardARMState
    {
        public IdleState(GP_CardBufferToCardARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.CARDBUFFER_TO_CARDARM;
                Loader.ProcModuleInfo.Source = CardBuffer.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = CardBuffer.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardBufferToCardARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{CardBuffer.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {ARM.ToString()}");
                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardBufferToCardARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_CardBufferToCardARMState
    {
        public RunningState(GP_CardBufferToCardARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum result = EventCodeEnum.LOADER_CARDBUFF_FAILED;
            var ccsuper = Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
            PIVInfo pivinfo = new PIVInfo(cardbufferindex: CardBuffer.ID.Index, stagenumber: ccsuper.GetRunningCCInfo().cardreqmoduleIndex);
            try
            {
                LoggerManager.ActionLog(ModuleLogType.CBUFFER_TO_CARM, StateLogType.START, $"OriginHolder: {CardBuffer.Holder.TransferObject.OriginHolder}, Source: {CardBuffer}, DestinationHolder: {ARM}, ID: {CardBuffer.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBusyEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();

                
                LoggerManager.Debug($"GP_CardBufferToCardARMState(): Before NeedtoCardID, CardPRESENCEState : {CardBuffer.CardPRESENCEState}");
                if (Extensions_IParam.ProberRunMode != RunMode.EMUL) 
                {
                    CardBuffer.UpdateCardBufferState(); 
                }
                LoggerManager.Debug($"GP_CardBufferToCardARMState(): After NeedtoCardID, CardPRESENCEState : {CardBuffer.CardPRESENCEState}");

                if(CardBuffer.CardPRESENCEState == CardPRESENCEStateEnum.EMPTY)//현재 gop_cardLoadprocstate 에서 carrier를 load할수 없게 되어있으므로 막는당
                {
                    Loader.ResonOfError = "CardBufferToCardARM Error. result:" + result.ToString();
                    LoggerManager.Error($"GP_CardBufferToCardARMState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));

                    semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CardBufferUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                    return;
                }


                if (CardBuffer.Holder.TransferObject != null)
                {
                    CardBuffer.Holder.TransferObject.OCRMode.Value = CardBuffer.NeedToReadCardId();
                    LoggerManager.Debug($"GP_CardBufferToCardARMState(): NeedToReadCardId:{CardBuffer.Holder.TransferObject.OCRMode.Value}");
                }

                result = this.GetLoaderCommands().CardBufferPick((ICardBufferModule)Module.Param.Curr, Module.Param.UseARM);
                if (result == EventCodeEnum.NONE)
                {

                    Loader.LoaderMaster.GEMModule().GetPIVContainer().SetCardBufferCardId(CardBuffer.ID.Index, "");
                    Loader.LoaderMaster.GEMModule().GetPIVContainer().UpdateCardBufferInfo(CardBuffer.ID.Index);
                    semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CardBufferUnloadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    if (CardBuffer.Holder.TransferObject != null)
                    {
                        CardBuffer.Holder.TransferObject.OriginHolder = new ProberInterfaces.ModuleID(CardBuffer.ID.ModuleType, CardBuffer.ID.Index, "");
                    }

                    if (CardBuffer.Holder.Status == ProberInterfaces.EnumSubsStatus.CARRIER)
                    {
                        CardBuffer.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        CardBuffer.UpdateCardBufferState();
                        StateTransition(new DoneState(Module));
                    }
                    else if (CardBuffer.Holder.isCardAttachHolder == false)
                    {
                        this.GetLoaderCommands().SetCardID((Module.Param.Curr as ICardBufferModule).Holder, "");// "Holder");
                        CardBuffer.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        CardBuffer.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        if (ARM.Holder.TransferObject.OCRMode.Value != ProberInterfaces.Enum.OCRModeEnum.NONE)
                        {
                            result = this.GetLoaderCommands().CardIDMovePosition(ARM.Holder);
                        }
                        
                        if (result == EventCodeEnum.NONE)
                        {
                            StateTransition(new DoneState(Module));
                            return;
                        }
                        else
                        {
                            Loader.ResonOfError = "CardIDMovePosition Error. result:" + result.ToString();
                            //    CardBuffer.Holder.SetUnknown();
                            //    ARM.Holder.SetUnknown();
                            LoggerManager.Error($"CardIDMovePosition(): Transfer failed. Job result = {result}");
                            Loader.ResonOfError = "Card ID Abort Error. result:" + result.ToString();
                            result = this.GetLoaderCommands().CardBufferPut(Module.Param.UseARM, (ICardBufferModule)Module.Param.Curr);
                            if (result == EventCodeEnum.NONE)
                            {
                                ARM.Holder.SetTransfered(CardBuffer);
                                Loader.BroadcastLoaderInfo();
                            }
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                }
                else
                {
                    Loader.ResonOfError = $"CardBufferPick Error.{Environment.NewLine}result:" + result.ToString();
                    if (CardBuffer.Holder.TransferObject != null)
                    {
                        CardBuffer.Holder.TransferObject.OriginHolder = new ProberInterfaces.ModuleID(CardBuffer.ID.ModuleType, CardBuffer.ID.Index, "");
                    }

                    CardHolder holder = null;
                    if (ARM.Holder != null)
                    {
                        holder = ARM.Holder;
                    }
                    else if (CardBuffer.Holder == null)
                    {
                        holder = CardBuffer.Holder;
                    }

                    if (holder != null)
                    {
                        if (holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            result = ARM.MonitorForVacuum(true);
                            if (result != EventCodeEnum.NONE)
                            {
                                result = CardBuffer.MonitorForSubstrate(true);
                                if (result == EventCodeEnum.NONE)
                                {
                                    if (ARM.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                        ARM.Holder.SetTransfered(CardBuffer);
                                }
                                else
                                {
                                    ARM.Holder.SetUnknown();
                                    CardBuffer.Holder.SetUnknown();
                                }
                            }
                            else
                            {
                                if (CardBuffer.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                    CardBuffer.Holder.SetTransfered(ARM);
                            }
                        }
                        else if (holder.Status == ProberInterfaces.EnumSubsStatus.CARRIER)
                        {
                            result = ARM.MonitorForCARDExist(true);
                            if (result != EventCodeEnum.NONE)
                            {
                                result = CardBuffer.MonitorForCarrierVac(true);
                                if (result == EventCodeEnum.NONE)
                                {
                                    if (ARM.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                        ARM.Holder.SetTransfered(CardBuffer);
                                }
                                else
                                {
                                    ARM.Holder.SetUnknown();
                                    CardBuffer.Holder.SetUnknown();
                                }
                            }
                            else
                            {
                                if (CardBuffer.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                    CardBuffer.Holder.SetTransfered(ARM);
                            }
                        }
                    }
                    Loader.BroadcastLoaderInfo();
                    LoggerManager.Error($"GP_CardBufferToCardARMState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));

                    semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CardBufferUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                    return;
                }
            }
            catch (Exception err)
            {
                Loader.ResonOfError = $"CardBufferPick Error.{Environment.NewLine}result:" + result.ToString();
                LoggerManager.Error($"GP_CardBufferToCardARMState(): Transfer failed. Job result = {result}, msg={err.Message}");
                StateTransition(new SystemErrorState(Module));

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(CardBufferUnloadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
            }

        }
    }
    public class DoneState : GP_CardBufferToCardARMState
    {
        public DoneState(GP_CardBufferToCardARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CBUFFER_TO_CARM, StateLogType.DONE, $"OriginHolder: {ARM.Holder.TransferObject.OriginHolder}, Source: {CardBuffer}, DestinationHolder: {ARM}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_CardBufferToCardARMState
    {
        public SystemErrorState(GP_CardBufferToCardARM module) : base(module)
        {
            try
            {
                if (CardBuffer.Holder.TransferObject != null)
                {
                    LoggerManager.ActionLog(ModuleLogType.CBUFFER_TO_CARM, StateLogType.ERROR, $"OriginHolder: {CardBuffer.Holder.TransferObject.OriginHolder}, Source: {CardBuffer}, DestinationHolder: {ARM}, ID: {CardBuffer.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
                }
                else
                {
                    LoggerManager.ActionLog(ModuleLogType.CBUFFER_TO_CARM, StateLogType.ERROR, $"Source: {CardBuffer}, DestinationHolder: {ARM}", isLoaderMap: true);
                }
                this.NotifyManager().Notify(EventCodeEnum.LOADER_CBUFFER_TO_CARM_TRANSFER_ERROR);
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
