using Autofac;
using LoaderBase;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoaderCore.GP_CardARMToCardBufferStates
{
    public abstract class GP_CardARMToCardBufferState : LoaderProcStateBase
    {
        public GP_CardARMToCardBuffer Module { get; set; }

        public GP_CardARMToCardBufferState(GP_CardARMToCardBuffer module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CardARMToCardBufferState stateObj)
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

        protected ICardBufferModule CardBuffer => Module.Param.Next as ICardBufferModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_CardARMToCardBufferState
    {
        public IdleState(GP_CardARMToCardBuffer module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.CARDARM_TO_CARDBUFFER;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = CardBuffer.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardARMToCardBufferState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                var ccsuper = Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                PIVInfo pIV = new PIVInfo() { ProbeCardID = ARM.Holder.TransferObject?.ProbeCardID.Value, StageNumber =  ccsuper.GetRunningCCInfo().cardreqmoduleIndex};
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBusyEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                semaphore.Wait();

                if (ARM.Holder.TransferObject?.ProbeCardID.Value != null &&
                    ARM.Holder.TransferObject?.ProbeCardID.Value != string.Empty)
                {
                    // 이 타이밍에 card id read 해야하는데 cardidmove하고 카드 돌리는 동작 해야하니까 임시조치로 이벤트만 올려줌.
                    semaphore = new SemaphoreSlim(0);
                    Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardIdReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                    semaphore.Wait();
                }
                
                

                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{ARM.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {CardBuffer.ToString()}");
                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardARMToCardBufferState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_CardARMToCardBufferState
    {
        public RunningState(GP_CardARMToCardBuffer module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.CARM_TO_CBUFFER, StateLogType.START, $"OriginHolder: {ARM.Holder.TransferObject.OriginHolder}, Source: {ARM}, DestinationHolder: {CardBuffer}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
                var ccsuper = Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                PIVInfo pivinfo = new PIVInfo(cardbufferindex: CardBuffer.ID.Index, stagenumber: ccsuper.GetRunningCCInfo().cardreqmoduleIndex);
                //PIVInfo pIV = new PIVInfo() { ProbeCardID = Module.Param.TransferObject?.ProbeCardID.Value };


                EventCodeEnum result = this.GetLoaderCommands().CardBufferPut(Module.Param.UseARM, (ICardBufferModule)Module.Param.DestPos);
                if (result == EventCodeEnum.NONE)
                {

                    ARM.Holder.SetTransfered(CardBuffer);

                    if (CardBuffer.Holder.TransferObject != null)
                    {
                        Loader.LoaderMaster.GEMModule().GetPIVContainer().SetCardBufferCardId(CardBuffer.ID.Index, CardBuffer.Holder.TransferObject.ProbeCardID.Value);
                        Loader.LoaderMaster.GEMModule().GetPIVContainer().UpdateCardBufferInfo(CardBuffer.ID.Index);
                        CardBuffer.Holder.TransferObject.OriginHolder = new ProberInterfaces.ModuleID(CardBuffer.ID.ModuleType, CardBuffer.ID.Index, "");
                    }

                    if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
                    {
                        CardBuffer.UpdateCardBufferState(forced_event: true);
                    }

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CardBufferLoadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    Loader.BroadcastLoaderInfo();
                    StateTransition(new DoneState(Module));
                    return;
                }
                else
                {
                    LoggerManager.Error($"GP_CardARMToCardBuffer(): Transfer failed. Job result = {result}");

                    Loader.ResonOfError = $"CardArm{ARM.ID.Index} To CardBuffer{CardBuffer.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
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


                    StateTransition(new SystemErrorState(Module));

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CardBufferLoadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_CardARMToCardBufferState
    {
        public DoneState(GP_CardARMToCardBuffer module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARM_TO_CBUFFER, StateLogType.DONE, $"OriginHolder: {CardBuffer.Holder.TransferObject.OriginHolder}, Source: {ARM}, DestinationHolder: {CardBuffer}, ID: {CardBuffer.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_CardARMToCardBufferState
    {
        public SystemErrorState(GP_CardARMToCardBuffer module) : base(module)
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
                    transObj = CardBuffer.Holder.TransferObject;
                }
                LoggerManager.ActionLog(ModuleLogType.CARM_TO_CBUFFER, StateLogType.ERROR, $"OriginHolder: {transObj.OriginHolder}, Source: {ARM}, DestinationHolder: {CardBuffer}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
                this.NotifyManager().Notify(EventCodeEnum.LOADER_CARM_TO_CBUFFER_TRANSFER_ERROR);
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
