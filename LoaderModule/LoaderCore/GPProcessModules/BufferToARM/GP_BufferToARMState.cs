using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;
using ProberInterfaces;

namespace LoaderCore.GP_BufferToARMStates
{
    public abstract class GP_BufferToARMState : LoaderProcStateBase
    {
        public GP_BufferToARM Module { get; set; }

        public GP_BufferToARMState(GP_BufferToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_BufferToARMState stateObj)
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

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected IBufferModule Buffer => Module.Param.Curr as IBufferModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_BufferToARMState
    {
        public IdleState(GP_BufferToARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.BUFFER_TO_ARM;
                Loader.ProcModuleInfo.Source = Buffer.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = Buffer.Holder.TransferObject.OriginHolder;
            } 
            catch (Exception err)
            {
                LoggerManager.Error($"GP_BufferToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{Buffer.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {ARM.ToString()}");
                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_BufferToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_BufferToARMState
    {
        public RunningState(GP_BufferToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.BUFFER_TO_ARM, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(Buffer.Holder.TransferObject.OriginHolder)}, Source: {Buffer}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.BUFFER_TO_ARM, StateLogType.START, Buffer.ID.Label, ARM.ID.Label, Buffer.Holder.TransferObject.OriginHolder.Label);
                var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, Buffer.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // Wafer obj 에 정보 이상일 수 있음.
                    Buffer.Holder.SetUnknown();
                    Loader.ResonOfError = $"Buffer{Buffer.ID.Index} To ARM{ARM.ID.Index} Transfer failed. Job result = {result}";
                    LoggerManager.Error($"GP_ARMToBufferState(): Transfer failed. Job result = {result}");
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new SystemErrorState(Module));
                }
                else
                {
                    result = this.GetLoaderCommands().BufferPick((IBufferModule)Module.Param.Curr, Module.Param.UseARM);
                    if (result == EventCodeEnum.NONE)
                    {
                        Buffer.Holder.CurrentWaferInfo = Buffer.Holder.TransferObject;
                        Buffer.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();

                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        Loader.ResonOfError = "BufferPick Error. result:" + result.ToString();
                        result = Buffer.MonitorForSubstrate(true); //베큠을 체크해본다.
                        if (result != EventCodeEnum.NONE) // Buffer 에 웨이퍼가 없을 경우
                        {
                            result = ARM.MonitorForVacuum(true); //arm 베큠 체크
                            if (result != EventCodeEnum.NONE)
                            {
                                var clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                                ARM.Holder.SetUnknown(clonedTransferObject);
                                Buffer.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                            }
                            else
                            {
                                Buffer.Holder.SetTransfered(ARM); // 있으면 Arm
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        LoggerManager.Error($"GP_BufferToARMState(): Transfer failed. Job result = {result}");
                        Loader.ResonOfError = $"Buffer{Buffer.ID.Index} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
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

                LoggerManager.Error($"GP_BufferToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");

                Loader.ResonOfError = $"Buffer{Buffer.ID.Index} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
                var result = Buffer.MonitorForSubstrate(true); //베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // Buffer 에 웨이퍼가 없을 경우
                {
                    result = ARM.MonitorForVacuum(true); //arm 베큠 체크
                    if (result != EventCodeEnum.NONE)
                    {
                        ARM.Holder.SetUnknown(clonedTransferObject);
                        Buffer.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                    }
                    else
                    {
                        if (Buffer.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            Buffer.Holder.SetTransfered(ARM); // 있으면 PreAlign
                        }
                    }
                }
                else
                {
                    if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        ARM.Holder.SetTransfered(Buffer); // 있으면 PreAlign
                    }
                }
                Loader.BroadcastLoaderInfo();
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_BufferToARMState
    {
        public DoneState(GP_BufferToARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.BUFFER_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {Buffer}, DestinationHolder: {ARM}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.BUFFER_TO_ARM, StateLogType.DONE, Buffer.ID.Label, ARM.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_BufferToARMState
    {
        public SystemErrorState(GP_BufferToARM module) : base(module)
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
                EventCodeEnum errorcode = EventCodeEnum.LOADER_BUFFER_TO_ARM_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.BUFFER_TO_ARM, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {Buffer}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.BUFFER_TO_ARM, StateLogType.ERROR, Buffer.ID.Label, ARM.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
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
