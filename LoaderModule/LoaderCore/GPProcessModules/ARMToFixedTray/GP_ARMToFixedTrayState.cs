using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using GPLoaderRouter;

namespace LoaderCore.GP_ARMToFixedTrayStates
{

    public abstract class GP_ARMToFixedTrayState : LoaderProcStateBase
    {
        public GP_ARMToFixedTray Module { get; set; }

        public GP_ARMToFixedTrayState(GP_ARMToFixedTray module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ARMToFixedTrayState stateObj)
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

        ILoaderModule loader;
        protected ILoaderModule Loader
        {
            get
            {
                if (loader == null)
                {
                    loader = Module.Container.Resolve<ILoaderModule>();
                }
                return loader;
            }
        }
        protected IARMModule ARM => Module.Param.Curr as IARMModule;

        protected IFixedTrayModule FixedTray => Module.Param.Next as IFixedTrayModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_ARMToFixedTrayState
    {
        public IdleState(GP_ARMToFixedTray module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.ARM_TO_FIXEDTRAY;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = FixedTray.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
              
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToFixedTrayState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_FIXED, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {FixedTray}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_FIXED, StateLogType.START, ARM.ID.Label, FixedTray.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
            try
            {
                var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, ARM.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // Wafer obj 에 정보 이상일 수 있음.
                    ARM.Holder.SetUnknown();
                    Loader.ResonOfError = $"ARM{ARM.ID.Index} To FixedTray{FixedTray.ID.Index} Transfer failed. Job result = {result}";
                    LoggerManager.Error($"GP_ARMToFixedTrayState(): Transfer failed. Job result = {result}");
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new SystemErrorState(Module));
                }
                else
                {
                    result = this.GetLoaderCommands().FixedTrayPut(Module.Param.UseARM, (IFixedTrayModule)Module.Param.DestPos);
                    if (result == EventCodeEnum.NONE)
                    {
                        ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;
                        string oldOrigin = ARM.Holder.TransferObject.OriginHolder.Label;

                        ARM.Holder.SetTransfered(FixedTray);

                        if (FixedTray.CanUseBuffer == false)
                        {
                            FixedTray.Holder.TransferObject.OriginHolder = FixedTray.ID;

                            if (FixedTray.Holder.TransferObject.WaferType.Value != EnumWaferType.POLISH)
                            {
                                if (this.GetLoaderCommands() is GPLoaderCommandEmulator)
                                {
                                    FixedTray.RecoveryWaferStatus(true);
                                }
                                else
                                {
                                    FixedTray.RecoveryWaferStatus();
                                }
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new DoneState(Module, oldOrigin));
                    }
                    else
                    {
                        Loader.ResonOfError = "FixedTrayPut Error. result:" + result.ToString();
                        //FixedTray.Holder.SetUnknown();
                        // ARM.Holder.SetUnknown();
                        result = ARM.MonitorForVacuum(true); //arm 베큠을 체크해본다.
                        if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                        {
                            result = FixedTray.MonitorForSubstrate(true); //Fixed 베큠 체크
                            if (result != EventCodeEnum.NONE)
                            {
                                var clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                                FixedTray.Holder.SetUnknown(clonedTransferObject);
                                ARM.Holder.SetUnknown(clonedTransferObject); //없으면 Missing 
                            }
                            else
                            {
                                ARM.Holder.SetTransfered(FixedTray); // 있으면 Fixed
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        LoggerManager.Error($"GP_ARMToFixedTrayState(): Transfer failed. Job result = {result}");
                        Loader.ResonOfError = $"ARM{ARM.ID.Index} To FixedTray{FixedTray.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
                        StateTransition(new SystemErrorState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToFixedTrayState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                Loader.ResonOfError = $"ARM{ARM.ID.Index} To FixedTray{FixedTray.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (FixedTray.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = FixedTray.Holder.TransferObject.Clone() as TransferObject;
                }

                var result = ARM.MonitorForVacuum(true); //arm 베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                {
                    result = FixedTray.MonitorForSubstrate(true); //Fixed 베큠 체크
                    if (result != EventCodeEnum.NONE)
                    {
                        FixedTray.Holder.SetUnknown(clonedTransferObject);
                        ARM.Holder.SetUnknown(clonedTransferObject); //없으면 Missing 
                    }
                    else
                    {
                        if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            ARM.Holder.SetTransfered(FixedTray); // 있으면 PreAlign
                        }
                    }
                }
                else
                {
                    if (FixedTray.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        FixedTray.Holder.SetTransfered(ARM); // 있으면 PreAlign
                    }
                }
                Loader.BroadcastLoaderInfo();
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_ARMToFixedTrayState
    {
        public RunningState(GP_ARMToFixedTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                //현재 ARMToFixed는 Running State를 사용하지 않고 있음, idle state에서 동작 시키고 done으로 감, 추후 수정 검토 필요함
                //LoggerManager.ActionLog(ModuleLogType.ARM_TO_FIXED, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {FixedTray}", isLoaderMap: LoggerManager.isMapLog);
                //LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_FIXED, StateLogType.START, ARM.ID.Label, FixedTray.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);

                var result = this.GetLoaderCommands().FixedTrayPut(Module.Param.UseARM, (IFixedTrayModule)Module.Param.DestPos);

                if (result == EventCodeEnum.NONE)
                {
                    ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;
                    ARM.Holder.SetTransfered(FixedTray);
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new DoneState(Module));
                }
                else
                {
                    FixedTray.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    LoggerManager.Error($"GP_ARMToFixedTrayState(): Transfer failed. Job result = {result}");
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                FixedTray.Holder.SetUnknown();
                ARM.Holder.SetUnknown();
                LoggerManager.Error($"GP_ARMToFixedTrayState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_ARMToFixedTrayState
    {
        public DoneState(GP_ARMToFixedTray module, string oldOrigin = "") : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_FIXED, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(FixedTray.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {FixedTray}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_FIXED, StateLogType.DONE, ARM.ID.Label, FixedTray.ID.Label, FixedTray.Holder.TransferObject.OriginHolder.Label, old: oldOrigin);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ARMToFixedTrayState
    {
        public SystemErrorState(GP_ARMToFixedTray module) : base(module)
        {
            TransferObject transObj = null;
            
            if (ARM.Holder.TransferObject != null)
            {
                transObj = ARM.Holder.TransferObject;
            }
            else
            {
                transObj = FixedTray.Holder.TransferObject;
            }
            EventCodeEnum errorcode = EventCodeEnum.LOADER_ARM_TO_FIXED_TRANSFER_ERROR;
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_FIXED, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {ARM}, DestinationHolder: {FixedTray}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_FIXED, StateLogType.ERROR, ARM.ID.Label, FixedTray.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
            this.NotifyManager().Notify(errorcode);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
