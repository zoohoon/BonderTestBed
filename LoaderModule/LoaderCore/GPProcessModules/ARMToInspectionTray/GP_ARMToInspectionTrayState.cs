using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;
using ProberInterfaces;

namespace LoaderCore.GP_ARMToInspectionTrayStates
{
    public abstract class GP_ARMToInspectionTrayState : LoaderProcStateBase
    {
        public GP_ARMToInspectionTray Module { get; set; }

        public GP_ARMToInspectionTrayState(GP_ARMToInspectionTray module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ARMToInspectionTrayState stateObj)
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

        protected IInspectionTrayModule InspectionTray => Module.Param.Next as IInspectionTrayModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_ARMToInspectionTrayState
    {
        public IdleState(GP_ARMToInspectionTray module) : base(module)
        {
            Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.ARM_TO_INSPTRAY;
            Loader.ProcModuleInfo.Source = ARM.ID;
            Loader.ProcModuleInfo.Destnation = InspectionTray.ID;
            Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{ARM.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {InspectionTray.ToString()}");

            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_ARMToInspectionTrayState
    {
        public RunningState(GP_ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_INSP, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {InspectionTray}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_INSP, StateLogType.START, ARM.ID.Label, InspectionTray.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
                var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, ARM.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // PA의 Wafer obj 에 정보 이상일 수 있음.
                    ARM.Holder.SetUnknown();
                    Loader.ResonOfError = $"ARM{ARM.ID.Index} To InspectionTray{InspectionTray.ID.Index} Transfer failed. Job result = {result}";
                    LoggerManager.Error($"GP_ARMToInspectionTrayState(): Transfer failed. Job result = {result}");
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new SystemErrorState(Module));
                }
                else
                {
                    result = this.GetLoaderCommands().DRWPut(Module.Param.UseARM, InspectionTray);
                    if (result == EventCodeEnum.NONE)
                    {
                        string oldOrigin = ARM.Holder.TransferObject.OriginHolder.Label;
                        ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;

                        if (!(ARM.Holder.TransferObject.WaferType.Value == EnumWaferType.STANDARD && ARM.Holder.Status == EnumSubsStatus.UNKNOWN))
                        {
                            ARM.Holder.TransferObject.OriginHolder = InspectionTray.ID;
                        }
                        
                        ARM.Holder.SetTransfered(InspectionTray);
                        Loader.BroadcastLoaderInfo();
                        InspectionTray.Holder.TransferObject.PreAlignState = PreAlignStateEnum.NONE;
                        StateTransition(new DoneState(Module, oldOrigin));
                    }
                    else
                    {
                        Loader.ResonOfError = "InspectionTrayPut Error. result:" + result.ToString();
                        result = ARM.MonitorForVacuum(true); //arm에 베큠을 체크해본다.

                        if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                        {
                            result = InspectionTray.MonitorForSubstrate(true); //InspectionTray 베큠 체크
                            if (result != EventCodeEnum.NONE)
                            {
                                var clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                                InspectionTray.Holder.SetUnknown(clonedTransferObject);
                                ARM.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                            }
                            else
                            {
                                ARM.Holder.SetTransfered(InspectionTray); // 있으면 InspectionTray
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        LoggerManager.Error($"GP_ARMToInspectionTrayState(): Transfer failed. Job result = {result}");
                        StateTransition(new SystemErrorState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                Loader.ResonOfError = "InspectionTrayPut Error. result:" + err.ToString();
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (InspectionTray.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = InspectionTray.Holder.TransferObject.Clone() as TransferObject;
                }

                var result = ARM.MonitorForVacuum(true); //arm에 베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                {
                    result = InspectionTray.MonitorForSubstrate(true); //InspectionTray 베큠 체크
                    if (result != EventCodeEnum.NONE)
                    {
                        InspectionTray.Holder.SetUnknown(clonedTransferObject); 
                        ARM.Holder.SetUnknown(clonedTransferObject); //없으면 Missing 
                    }
                    else
                    {
                        if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            ARM.Holder.SetTransfered(InspectionTray); // 있으면 PreAlign
                        }
                    }
                }
                else
                {
                    if (InspectionTray.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        InspectionTray.Holder.SetTransfered(ARM); // 있으면 PreAlign
                    }
                }
                LoggerManager.Error($"GP_ARMToInspectionTrayState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
            //ARM.Holder.TransferObject.PreAlignState = LoaderParameters.PreAlignStateEnum.NONE;
            //ARM.Holder.SetTransfered(InspectionTray);
            //StateTransition(new DoneState(Module));
        }
    }
    public class DoneState : GP_ARMToInspectionTrayState
    {
        public DoneState(GP_ARMToInspectionTray module, string oldOrigin = "") : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_INSP, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(InspectionTray.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {InspectionTray}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_INSP, StateLogType.DONE, ARM.ID.Label, InspectionTray.ID.Label, InspectionTray.Holder.TransferObject.OriginHolder.Label, old: oldOrigin);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ARMToInspectionTrayState
    {
        public SystemErrorState(GP_ARMToInspectionTray module) : base(module)
        {
            TransferObject transObj = null;
            if (ARM.Holder.TransferObject != null)
            {
                transObj = ARM.Holder.TransferObject;
            }
            else
            {
                transObj = InspectionTray.Holder.TransferObject;
            }
            EventCodeEnum errorcode = EventCodeEnum.LOADER_ARM_TO_INSP_TRANSFER_ERROR;
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_INSP, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {ARM}, DestinationHolder: {InspectionTray}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_INSP, StateLogType.ERROR, ARM.ID.Label, InspectionTray.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
            this.NotifyManager().Notify(errorcode);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
