using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_FixedTrayToARMStates
{
    public abstract class GP_FixedTrayToARMState : LoaderProcStateBase
    {
        public GP_FixedTrayToARM Module { get; set; }

        public GP_FixedTrayToARMState(GP_FixedTrayToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_FixedTrayToARMState stateObj)
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

        protected IFixedTrayModule Fixed => Module.Param.Curr as IFixedTrayModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_FixedTrayToARMState
    {
        public IdleState(GP_FixedTrayToARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.FIXEDTRAY_TO_ARM;
                Loader.ProcModuleInfo.Source = Fixed.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = Fixed.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_FixedTrayToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.FIXED_TO_ARM, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(Fixed.Holder.TransferObject.OriginHolder)}, Source: {Fixed}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.FIXED_TO_ARM, StateLogType.START, Fixed.ID.Label, ARM.ID.Label, Fixed.Holder.TransferObject.OriginHolder.Label);
                var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, Fixed.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // Wafer obj 에 정보 이상일 수 있음.
                    Fixed.Holder.SetUnknown();
                    Loader.ResonOfError = $"FixedTray{Fixed.ID.Index} To ARM{ARM.ID.Index} Transfer failed. Job result = {result}";
                    LoggerManager.Error($"GP_FixedTrayToARMState(): Transfer failed. Job result = {result}");
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new SystemErrorState(Module));
                }
                else
                {
                    result = this.GetLoaderCommands().FixedTrayPick((IFixedTrayModule)Module.Param.Curr, Module.Param.UseARM);
                    if (result == EventCodeEnum.NONE)
                    {
                        Fixed.Holder.CurrentWaferInfo = Fixed.Holder.TransferObject;
                        /*
                        if (!Fixed.CanUseBuffer && Fixed.Holder.TransferObject.WaferType.Value != EnumWaferType.STANDARD)
                        {
                            Fixed.Holder.TransferObject.SetOCRState("", 0, ProberInterfaces.Enum.OCRReadStateEnum.NONE);
                        }
                        */
                        Fixed.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        Loader.ResonOfError = "FixedTrayPick Error. result:" + result.ToString();
                        result = Fixed.MonitorForSubstrate(true); //베큠을 체크해본다.
                        if (result != EventCodeEnum.NONE) // Fixed에 웨이퍼가 없을 경우
                        {
                            result = ARM.MonitorForVacuum(true); //ARM 베큠 체크
                            if (result != EventCodeEnum.NONE)
                            {
                                var clonedTransferObject = Fixed.Holder.TransferObject.Clone() as TransferObject;
                                ARM.Holder.SetUnknown(clonedTransferObject);
                                Fixed.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                            }
                            else
                            {
                                Fixed.Holder.SetTransfered(ARM); // 있으면 ARM
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        LoggerManager.Error($"GP_FixedTrayToARMState(): Transfer failed. Job result = {result}");
                        StateTransition(new SystemErrorState(Module));
                    }
                }
                
            }
            catch (Exception err)
            {
                Loader.ResonOfError = "FixedTrayPick Error. result:" + err.ToString();
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (Fixed.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = Fixed.Holder.TransferObject.Clone() as TransferObject;
                }

                var result = Fixed.MonitorForSubstrate(true); //베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // Fixed에 웨이퍼가 없을 경우
                {
                    result = ARM.MonitorForVacuum(true); //ARM 베큠 체크
                    if (result != EventCodeEnum.NONE)
                    {
                        ARM.Holder.SetUnknown(clonedTransferObject);
                        Fixed.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                    }
                    else
                    {
                        if (Fixed.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            Fixed.Holder.SetTransfered(ARM); // 있으면 PreAlign
                        }
                    }
                }
                else 
                {
                    if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        ARM.Holder.SetTransfered(Fixed); // 있으면 PreAlign
                    }
                }
                Loader.BroadcastLoaderInfo();
                LoggerManager.Error($"GP_FixedTrayToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class RunningState : GP_FixedTrayToARMState
    {
        public RunningState(GP_FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            StateTransition(new DoneState(Module));
        }
    }
    public class DoneState : GP_FixedTrayToARMState
    {
        public DoneState(GP_FixedTrayToARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.FIXED_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {Fixed}, DestinationHolder: {ARM}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.FIXED_TO_ARM, StateLogType.DONE, Fixed.ID.Label, ARM.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
        }
        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_FixedTrayToARMState
    {
        public SystemErrorState(GP_FixedTrayToARM module) : base(module)
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
                    transObj = Fixed.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_FIXED_TO_ARM_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.FIXED_TO_ARM, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {Fixed}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.FIXED_TO_ARM, StateLogType.ERROR, Fixed.ID.Label, ARM.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
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
