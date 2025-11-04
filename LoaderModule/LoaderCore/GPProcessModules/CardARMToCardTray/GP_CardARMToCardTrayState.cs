using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_CardARMToCardTrayStates
{
    public abstract class GP_CardARMToCardTrayState : LoaderProcStateBase
    {
        public GP_CardARMToCardTray Module { get; set; }

        public GP_CardARMToCardTrayState(GP_CardARMToCardTray module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CardARMToCardTrayState stateObj)
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

        protected ICardBufferTrayModule CardTray => Module.Param.Next as ICardBufferTrayModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_CardARMToCardTrayState
    {
        public IdleState(GP_CardARMToCardTray module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.CARDARM_TO_CARDTRAY;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = CardTray.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardARMToCardTrayState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{ARM.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {CardTray.ToString()}");
                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardARMToCardTrayState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_CardARMToCardTrayState
    {
        public RunningState(GP_CardARMToCardTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.CARM_TO_CARDTRAY, StateLogType.START, $"OriginHolder: {ARM.Holder.TransferObject.OriginHolder}, Source: {ARM}, DestinationHolder: {CardTray}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);

                EventCodeEnum result = this.GetLoaderCommands().CardTrayPut(Module.Param.UseARM, (ICardBufferTrayModule)Module.Param.DestPos);

                if (result == EventCodeEnum.NONE)
                {
                    ARM.Holder.SetTransfered(CardTray);
                    if (CardTray.Holder.TransferObject != null)
                    {
                        CardTray.Holder.TransferObject.OriginHolder = new ProberInterfaces.ModuleID(CardTray.ID.ModuleType, CardTray.ID.Index, "");
                    }
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new DoneState(Module));
                }
                else
                {
                    Loader.ResonOfError = "CardTrayPut Error. result:" + result.ToString();
                    if (CardTray.Holder.TransferObject != null)
                    {
                        CardTray.Holder.TransferObject.OriginHolder = new ProberInterfaces.ModuleID(CardTray.ID.ModuleType, CardTray.ID.Index, "");
                    }

                    CardHolder holder = null;
                    if (ARM.Holder != null)
                    {
                        holder = ARM.Holder;
                    }
                    else if (CardTray.Holder == null)
                    {
                        holder = CardTray.Holder;
                    }

                    if (holder != null)
                    {
                        if (holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            result = ARM.MonitorForVacuum(true);
                            if (result != EventCodeEnum.NONE)
                            {
                                result = CardTray.MonitorForSubstrate(true);
                                if (result == EventCodeEnum.NONE)
                                {
                                    if (ARM.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                        ARM.Holder.SetTransfered(CardTray);
                                }
                                else
                                {
                                    ARM.Holder.SetUnknown();
                                    CardTray.Holder.SetUnknown();
                                }
                            }
                            else
                            {
                                if (CardTray.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                    CardTray.Holder.SetTransfered(ARM);
                            }
                        }
                        else if (holder.Status == ProberInterfaces.EnumSubsStatus.CARRIER)
                        {
                            result = ARM.MonitorForCARDExist(true);
                            if (result != EventCodeEnum.NONE)
                            {
                                result = CardTray.MonitorForSubstrate_Down(true);
                                if (result == EventCodeEnum.NONE)
                                {
                                    if (ARM.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                        ARM.Holder.SetTransfered(CardTray);
                                }
                                else
                                {
                                    ARM.Holder.SetUnknown();
                                    CardTray.Holder.SetUnknown();
                                }
                            }
                            else
                            {
                                if (CardTray.Holder.Status == ProberInterfaces.EnumSubsStatus.EXIST)
                                    CardTray.Holder.SetTransfered(ARM);
                            }
                        }
                    }
                    Loader.BroadcastLoaderInfo();
                    LoggerManager.Error($"GP_CardARMToCardTrayState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));
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
    public class DoneState : GP_CardARMToCardTrayState
    {
        public DoneState(GP_CardARMToCardTray module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.CARM_TO_CARDTRAY, StateLogType.DONE, $"OriginHolder: {CardTray.Holder.TransferObject.OriginHolder}, Source: {ARM}, DestinationHolder: {CardTray}, ID: {CardTray.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_CardARMToCardTrayState
    {
        public SystemErrorState(GP_CardARMToCardTray module) : base(module)
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
                    transObj = CardTray.Holder.TransferObject;
                }
                LoggerManager.ActionLog(ModuleLogType.CARM_TO_CARDTRAY, StateLogType.ERROR, $"OriginHolder: {transObj.OriginHolder}, Source: {ARM}, DestinationHolder: {CardTray}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
                this.NotifyManager().Notify(EventCodeEnum.LOADER_CARM_TO_CARDTRAY_TRANSFER_ERROR);
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
