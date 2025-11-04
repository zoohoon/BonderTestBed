using System;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using LogModule;

namespace LoaderCore.GP_ARMToBufferStates
{
    public abstract class GP_ARMToBufferState : LoaderProcStateBase
    {
        public GP_ARMToBuffer Module { get; set; }
        public GP_ARMToBufferState(GP_ARMToBuffer module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ARMToBufferState stateObj)
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

        protected IBufferModule Buffer => Module.Param.Next as IBufferModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_ARMToBufferState
    {
        public IdleState(GP_ARMToBuffer module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.ARM_TO_BUFFER;
                Loader.ProcModuleInfo.Source = ARM.ID;
                Loader.ProcModuleInfo.Destnation = Buffer.ID;
                Loader.ProcModuleInfo.Origin = ARM.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToBufferState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)} , DestinationHolder: {Buffer}");

                StateTransition(new RunningState(Module));
            } 
            catch (Exception err)
            {
                LoggerManager.Error($"GP_ARMToBufferState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
}
    }
    public class RunningState : GP_ARMToBufferState
    {
        public RunningState(GP_ARMToBuffer module) : base(module)
        {
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_BUFFER, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {Buffer}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_BUFFER, StateLogType.START, ARM.ID.Label, Buffer.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);

                var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, ARM.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // Wafer obj 에 정보 이상일 수 있음.
                    ARM.Holder.SetUnknown();
                    Loader.ResonOfError = $"ARM{ARM.ID.Index} To Buffer{Buffer.ID.Index} Transfer failed. Job result = {result}";
                    LoggerManager.Error($"GP_ARMToBufferState(): Transfer failed. Job result = {result}");
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new SystemErrorState(Module));
                }
                else 
                {
                    result = this.GetLoaderCommands().BufferPut(Module.Param.UseARM, (IBufferModule)Module.Param.DestPos);
                    if (result == EventCodeEnum.NONE)
                    {
                        ARM.Holder.CurrentWaferInfo = ARM.Holder.TransferObject;
                        ARM.Holder.SetTransfered(Buffer);
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        Loader.ResonOfError = "BufferPut Error. result:" + result.ToString();
                        result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                        if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                        {
                            result = Buffer.MonitorForSubstrate(true); //buffer 베큠 체크
                            if (result != EventCodeEnum.NONE)
                            {
                                var clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                                Buffer.Holder.SetUnknown(clonedTransferObject);
                                ARM.Holder.SetUnknown(clonedTransferObject); //없으면 Missing 
                            }
                            else
                            {
                                ARM.Holder.SetTransfered(Buffer); // 있으면 버퍼
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        LoggerManager.Error($"GP_ARMToBufferState(): Transfer failed. Job result = {result}");
                        Loader.ResonOfError = $"ARM{ARM.ID.Index} To Buffer{Buffer.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
                        StateTransition(new SystemErrorState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (Buffer.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = Buffer.Holder.TransferObject.Clone() as TransferObject;
                }
                var result = ARM.MonitorForVacuum(true); //베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                {
                    result = Buffer.MonitorForSubstrate(true); //buffer 베큠 체크
                    if (result != EventCodeEnum.NONE)
                    {
                        Buffer.Holder.SetUnknown(clonedTransferObject);
                        ARM.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                    }
                    else
                    {
                        if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            ARM.Holder.SetTransfered(Buffer); // 있으면 PreAlign
                        }
                    }
                }
                else
                {
                    if (Buffer.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        Buffer.Holder.SetTransfered(ARM); // 있으면 PreAlign
                    }
                }
                Loader.BroadcastLoaderInfo();
                Loader.ResonOfError = $"ARM{ARM.ID.Index} To Buffer{Buffer.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
                // Loader.AddUnknownModule(Buffer);
                // Loader.AddUnknownModule(ARM);
                LoggerManager.Error($"GP_ARMToBufferState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }

    public class DoneState : GP_ARMToBufferState
    {
        public DoneState(GP_ARMToBuffer module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.ARM_TO_BUFFER, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(Buffer.Holder.TransferObject.OriginHolder)}, Source: {ARM}, DestinationHolder: {Buffer}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_BUFFER, StateLogType.DONE, ARM.ID.Label, Buffer.ID.Label, Buffer.Holder.TransferObject.OriginHolder.Label);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute()
        {

        }

    }

    public class SystemErrorState : GP_ARMToBufferState
    {
        public SystemErrorState(GP_ARMToBuffer module) : base(module)
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
                    transObj = Buffer.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_ARM_TO_BUFFER_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.ARM_TO_BUFFER, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {ARM}, DestinationHolder: {Buffer}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.ARM_TO_BUFFER, StateLogType.ERROR, ARM.ID.Label, Buffer.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
                this.NotifyManager().Notify(errorcode);
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
