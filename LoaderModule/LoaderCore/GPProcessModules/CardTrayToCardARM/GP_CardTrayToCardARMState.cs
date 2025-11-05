using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_CardBufferTrayToCardARMStates
{
    public abstract class GP_CardTrayToCardARMState : LoaderProcStateBase
    {
        public GP_CardTrayToCardARM Module { get; set; }

        public GP_CardTrayToCardARMState(GP_CardTrayToCardARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CardTrayToCardARMState stateObj)
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

        protected ICardBufferTrayModule CardTray => Module.Param.Curr as ICardBufferTrayModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_CardTrayToCardARMState
    {
        public IdleState(GP_CardTrayToCardARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.CARDTRAY_TO_CARDARM;
                Loader.ProcModuleInfo.Source = CardTray.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = CardTray.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardTrayToCardARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{CardTray.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {ARM.ToString()}");

                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CardTrayToCardARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_CardTrayToCardARMState
    {
        public RunningState(GP_CardTrayToCardARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum result = EventCodeEnum.LOADER_CARDBUFF_FAILED;
            try
            {
                LoggerManager.ActionLog(ModuleLogType.TRAY_TO_CARM, StateLogType.START, $"OriginHolder: {CardTray.Holder.TransferObject.OriginHolder}, Source: {CardTray}, DestinationHolder: {ARM}, ID: {CardTray.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);

                result = this.GetLoaderCommands().CardTrayPick((ICardBufferTrayModule)Module.Param.Curr, Module.Param.UseARM);
                if (result == EventCodeEnum.NONE)
                {
                    if (CardTray.Holder.TransferObject != null)
                    {
                        CardTray.Holder.TransferObject.OriginHolder = new ProberInterfaces.ModuleID(CardTray.ID.ModuleType, CardTray.ID.Index, "");
                    }

                    if (CardTray.Holder.Status == ProberInterfaces.EnumSubsStatus.CARRIER)
                    {
                        CardTray.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new DoneState(Module));
                    }
                    else if (CardTray.Holder.isCardAttachHolder == false)
                    {
                        this.GetLoaderCommands().SetCardID(CardTray.Holder, "");
                        CardTray.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        CardTray.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        result = this.GetLoaderCommands().CardIDMovePosition(ARM.Holder);
                        if (result == EventCodeEnum.NONE)
                        {
                            StateTransition(new DoneState(Module));
                            return;
                        }
                        else
                        {
                            if (result == EventCodeEnum.NODATA)
                            {
                                Loader.ResonOfError = "Card ID Abort Error. result:" + result.ToString();
                                //   CardTray.Holder.SetUnknown();
                                //   ARM.Holder.SetUnknown();
                                LoggerManager.Error($"CardIDMovePosition(): Transfer failed. Job result = {result}");
                            }
                            else
                            {
                                Loader.ResonOfError = "CardIDMovePosition Error. result:" + result.ToString();
                                //   CardTray.Holder.SetUnknown();
                                //   ARM.Holder.SetUnknown();
                                LoggerManager.Error($"CardIDMovePosition(): Transfer failed. Job result = {result}");
                            }
                            Loader.ResonOfError = "Card ID Abort Error. result:" + result.ToString();
                            result = this.GetLoaderCommands().CardTrayPut(Module.Param.UseARM, (ICardBufferTrayModule)Module.Param.Curr);
                            if (result == EventCodeEnum.NONE)
                            {
                                ARM.Holder.SetTransfered(CardTray);
                                Loader.BroadcastLoaderInfo();
                            }
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }
                    }
                }
                else
                {
                    Loader.ResonOfError = "CardTrayPick Error. result:" + result.ToString();
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

                    if (holder!= null)
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
                    LoggerManager.Error($"GP_CardTrayToCardARMState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
            }
            catch (Exception err)
            {
                Loader.ResonOfError = "CardTrayPick Error. result:" + result.ToString();
                //     ARM.Holder.SetUnknown();
                //    CardTray.Holder.SetUnknown();
                LoggerManager.Error($"GP_CardTrayToCardARMState(): Transfer failed. Job result = {result}, msge={err.Message}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_CardTrayToCardARMState
    {
        public DoneState(GP_CardTrayToCardARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.TRAY_TO_CARM, StateLogType.DONE, $"OriginHolder: {ARM.Holder.TransferObject.OriginHolder}, Source: {CardTray}, DestinationHolder: {ARM}, ID: {ARM.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_CardTrayToCardARMState
    {
        public SystemErrorState(GP_CardTrayToCardARM module) : base(module)
        {
            try
            {
                if (CardTray.Holder.TransferObject != null)
                {
                    LoggerManager.ActionLog(ModuleLogType.TRAY_TO_CARM, StateLogType.ERROR, $"OriginHolder: {CardTray.Holder.TransferObject.OriginHolder}, Source: {CardTray}, DestinationHolder: {ARM}, ID: {CardTray.Holder.TransferObject.ProbeCardID.Value}", isLoaderMap: true);
                }
                else
                {
                    LoggerManager.ActionLog(ModuleLogType.TRAY_TO_CARM, StateLogType.ERROR, $"Source: {CardTray}, DestinationHolder: {ARM}");
                }
                this.NotifyManager().Notify(EventCodeEnum.LOADER_TRAY_TO_CARM_TRANSFER_ERROR);
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
