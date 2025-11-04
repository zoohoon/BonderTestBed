using Autofac;
using LoaderBase;
using LoaderCore.GPProcessModules.CardChangeToCardARM;
using LoaderParameters;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoaderCore.GP_CardChangeToCardARMStates
{
    public abstract class GP_CardChangeToCardARMState : LoaderProcStateBase
    {
        public GP_CardChangeToCardARM Module { get; set; }

        public GP_CardChangeToCardARMState(GP_CardChangeToCardARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CardChangeToCardARMState stateObj)
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

        protected ICCModule CardChange => Module.Param.Curr as ICCModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
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
        public virtual EventCodeEnum CardChangePick()
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
        public virtual EventCodeEnum CardChangeCarrierPut()
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
        public virtual EventCodeEnum OriginCarrierPick()
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
        public virtual EventCodeEnum CardUnDockingDone()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class IdleState : GP_CardChangeToCardARMState
    {
        public IdleState(GP_CardChangeToCardARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.STAGE_TO_CARDARM;
                Loader.ProcModuleInfo.Source = CardChange.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = CardChange.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardChangeToCardARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                Loader.ChuckNumber = CardChange.ID.Index;
                
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{CardChange.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {ARM.ToString()}");

                StateTransition(new WaitForRemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardChangeToCardARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }


    public class RemotingState : GP_CardChangeToCardARMState
    {
        public RemotingState(GP_CardChangeToCardARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute() { }

        public override EventCodeEnum NotifyWaferTransferResult(bool isSucceed)
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} NotifyWaferTransferResult() ReturnValue={isSucceed}");
                if (isSucceed)
                {
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    Loader.ResonOfError = "Chuck To CardArm Error. ";
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum CardChangeCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //var trg = Loader.ModuleManager.GetTransferObjectAll().Where(item =>
                //item.CurrHolder.ModuleType == ModuleTypeEnum.CC && item.CurrPos.ModuleType == ModuleTypeEnum.CC).ToList().FirstOrDefault();

                //ARM.Holder.SetTransferObject(trg);
                retVal = this.GetLoaderCommands().CardChangerPick((ICCModule)Module.Param.Curr, Module.Param.UseARM,1);
                if (retVal == EventCodeEnum.NONE)
                {
                    //CardChange.Holder.SetTransfered(ARM);
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

        public override EventCodeEnum CardChangePick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.ActionLog(ModuleLogType.STAGE_TO_CARM, StateLogType.START, $"OriginHolder: {CardChange.Holder.TransferObject.OriginHolder}, Source: {CardChange}, DestinationHolder: {ARM}, ID: {CardChange.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);

                var ccsuper = Loader.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                PIVInfo pIV = new PIVInfo() { ProbeCardID = Module.Param.TransferObject?.ProbeCardID.Value,  StageNumber = ccsuper.GetRunningCCInfo().cardreqmoduleIndex};
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                Loader.LoaderMaster.EventManager().RaisingEvent(typeof(CardBusyEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                semaphore.Wait();

                retVal = this.GetLoaderCommands().CardChangerPick((ICCModule)Module.Param.Curr, Module.Param.UseARM);
                if (retVal == EventCodeEnum.NONE)
                {
                    CardChange.Holder.SetTransfered(ARM);
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    LoggerManager.Error($"CardChangePick(): CardChangePick Transfer failed. Job result = {retVal}");

                    //  암 베큠 체크
                    retVal = ARM.MonitorForVacuum(true);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        CardChange.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        LoggerManager.Error($"GP_CardARMToCardChangeState(): The card remains in Arm. ARM.MonitorForVacuum(true) result = {retVal}");

                    }
                    else
                    {
                        //Card가 Arm에 없다.
                        LoggerManager.Error($"GP_CardARMToCardChangeState(): ARM.MonitorForVacuum(true) result = {retVal}");
                        StateTransition(new SystemErrorState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum Card_LoadingPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.GetLoaderCommands().CardMoveLoadingPosition((ICCModule)Module.Param.Curr, Module.Param.UseARM);
                if (retVal == EventCodeEnum.NONE)
                {
                }
                else
                {
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
        public override EventCodeEnum CardTransferDone(bool isSucceed)
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} CardTransferDone() ReturnValue={isSucceed}");

                //var ccmodule = (ICCModule)Module.Param.Curr as ICardOwnable;
                //ccmodule.RecoveryCardStatus();

                if (isSucceed)
                {
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    var ccmodule = (ICCModule)Module.Param.Curr as ICardOwnable;
                    ccmodule.RecoveryCardStatus();
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public override EventCodeEnum CardChangeCarrierPut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.GetLoaderCommands().CardChangerPut(Module.Param.UseARM, (ICCModule)Module.Param.Curr);
                if (retVal == EventCodeEnum.NONE)
                {
                    ARM.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    Loader.ResonOfError = "CardChange Carrier Put Error.";
                    LoggerManager.Error($"GP_CardChangeToCardARMState(): CardChangeCarrierPut Transfer failed. Job result = {retVal}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum OriginCarrierPick()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CardModuleInfo module = null;
                if (CardChange.Holder.TransferObject!=null)
                {
                    if (CardChange.Holder.TransferObject.DstPos.ModuleType==ModuleTypeEnum.CARDTRAY)
                    {
                        module = Loader.GetLoaderInfo().StateMap.CardTrayModules.Where(i => (i.ID.Index == CardChange.Holder.TransferObject.DstPos.Index) && i.WaferStatus == EnumSubsStatus.CARRIER).FirstOrDefault();
                    }
                    else if (CardChange.Holder.TransferObject.DstPos.ModuleType == ModuleTypeEnum.CARDBUFFER)
                    {
                        module = Loader.GetLoaderInfo().StateMap.CardBufferModules.Where(i => (i.ID.Index == CardChange.Holder.TransferObject.DstPos.Index) && i.WaferStatus == EnumSubsStatus.CARRIER).FirstOrDefault();
                    }
                }

                if (module == null)
                {
                    module = Loader.GetLoaderInfo().StateMap.CardTrayModules.Where(i => i.WaferStatus == EnumSubsStatus.CARRIER).FirstOrDefault();
                    if (module == null)
                    {
                        module = Loader.GetLoaderInfo().StateMap.CardBufferModules.Where(i => i.WaferStatus == EnumSubsStatus.CARRIER).FirstOrDefault();
                    }
                }

                if (module != null)
                {
                    var CardModule = Loader.ModuleManager.FindModule(module.ID);
                    if (CardModule.ModuleType == ModuleTypeEnum.CARDBUFFER)
                    {
                        ICardBufferModule CardBuffer = (ICardBufferModule)CardModule;
                        if (CardBuffer != null)
                        {
                            retVal = this.GetLoaderCommands().CardBufferPick((ICardBufferModule)CardBuffer, Module.Param.UseARM, 1);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                CardBuffer.Holder.SetTransfered(ARM);
                                Loader.BroadcastLoaderInfo();
                            }
                            else
                            {
                                Loader.ResonOfError = "OriginHolder Carrier Pick Error.";
                                LoggerManager.Error($"GP_CardChangeToCardARMState(): OriginPick Transfer failed. Job result = {retVal}");
                                StateTransition(new SystemErrorState(Module));
                            }
                        }
                        else
                        {
                            Loader.ResonOfError = "OriginHolder Carrier Pick Error.";
                            LoggerManager.Error($"GP_CardChangeToCardARMState(): OriginPick Transfer failed. OriginHoler is NUll");
                            StateTransition(new SystemErrorState(Module));
                        }
                    }
                    else if (CardModule.ModuleType == ModuleTypeEnum.CARDTRAY)
                    {
                        ICardBufferTrayModule CardTray = (ICardBufferTrayModule)CardModule;
                        if (CardTray != null)
                        {
                            retVal = this.GetLoaderCommands().CardTrayPick((ICardBufferTrayModule)CardTray, Module.Param.UseARM, 1);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                CardTray.Holder.SetTransfered(ARM);
                                Loader.BroadcastLoaderInfo();
                            }
                            else
                            {
                                Loader.ResonOfError = "OriginHolder Carrier Pick Error.";
                                LoggerManager.Error($"GP_CardChangeToCardARMState(): OriginPick Transfer failed. Job result = {retVal}");
                                StateTransition(new SystemErrorState(Module));
                            }
                        }
                        else
                        {
                            Loader.ResonOfError = "OriginHolder Carrier Pick Error.";
                            LoggerManager.Error($"GP_CardChangeToCardARMState(): OriginPick Transfer failed. OriginHoler is NUll");
                            StateTransition(new SystemErrorState(Module));
                        }
                    }
                    else
                    {
                        Loader.ResonOfError = " Carrier Module Null Error.";
                        LoggerManager.Error($"GP_CardChangeToCardARMState(): OriginPick Transfer failed. OriginHoler is NUll");
                        StateTransition(new SystemErrorState(Module));
                    }

                }
                else
                {
                    Loader.ResonOfError = " Carrier Module Null Error.";
                    LoggerManager.Error($"GP_CardChangeToCardARMState(): OriginPick Transfer failed. OriginHoler is NUll");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    public class WaitForRemotingState : GP_CardChangeToCardARMState
    {
        public WaitForRemotingState(GP_CardChangeToCardARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.CARD_UNLOAD;

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
    public class RunningState : GP_CardChangeToCardARMState
    {
        public RunningState(GP_CardChangeToCardARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum result = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
            try
            {
                
                result = this.GetLoaderCommands().CardChangerPick((ICCModule)Module.Param.Curr, Module.Param.UseARM);
                if (result == EventCodeEnum.NONE)
                {
                    CardChange.Holder.SetTransfered(ARM);
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new DoneState(Module));

                }
                else
                {
                    LoggerManager.Error($"GP_CardChangeToCardARM(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"GP_CardChangeToCardARM(): Transfer failed. Job result = {result}");
                StateTransition(new SystemErrorState(Module));
            }

        }
    }
    public class DoneState : GP_CardChangeToCardARMState
    {
        public DoneState(GP_CardChangeToCardARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.STAGE_TO_CARM, StateLogType.DONE, $"OriginHolder: {ARM.Holder.TransferObject.OriginHolder}, Source: {CardChange}, DestinationHolder: {ARM}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }
    }

    public class SystemErrorState : GP_CardChangeToCardARMState
    {
        public SystemErrorState(GP_CardChangeToCardARM module) : base(module)
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
                LoggerManager.ActionLog(ModuleLogType.STAGE_TO_CARM, StateLogType.ERROR, $"OriginHolder: {transObj.OriginHolder}, Source: {CardChange}, DestinationHolder: {ARM}, ID: {transObj.ProbeCardID.Value}", isLoaderMap: true);
                this.NotifyManager().Notify(EventCodeEnum.LOADER_STAGE_TO_CARM_TRANSFER_ERROR);
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
