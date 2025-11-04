using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_InspectionTrayToARMStates
{
    public abstract class GP_InspectionTrayToARMState : LoaderProcStateBase
    {
        public GP_InspectionTrayToARM Module { get; set; }

        public GP_InspectionTrayToARMState(GP_InspectionTrayToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_InspectionTrayToARMState stateObj)
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

        protected IInspectionTrayModule INSP => Module.Param.Curr as IInspectionTrayModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_InspectionTrayToARMState
    {
        public IdleState(GP_InspectionTrayToARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.INSPTRAY_TO_ARM;
                Loader.ProcModuleInfo.Source = INSP.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = INSP.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_InspectionTrayToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            LoggerManager.Debug($"[LOADER] {Module.GetType().Name} OriginHolder:{INSP.Holder.TransferObject.OriginHolder.ToString()} , DestinationHolder: {ARM.ToString()}");

            //INSP.Holder.SetTransfered(ARM);
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_InspectionTrayToARMState
    {
        public RunningState(GP_InspectionTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.INSP_TO_ARM, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(INSP.Holder.TransferObject.OriginHolder)}, Source: {INSP}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.INSP_TO_ARM, StateLogType.START, INSP.ID.Label, ARM.ID.Label, INSP.Holder.TransferObject.OriginHolder.Label);
                var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, INSP.Holder.Status);
                if (result != EventCodeEnum.NONE)
                {
                    // PA의 Wafer obj 에 정보 이상일 수 있음.
                    INSP.Holder.SetUnknown();
                    Loader.ResonOfError = $"InspectionTray{INSP.ID.Index} To ARM{ARM.ID.Index} Transfer failed. Job result = {result}";
                    LoggerManager.Error($"GP_InspectionTrayToARMState(): Transfer failed. Job result = {result}");
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new SystemErrorState(Module));
                }
                else 
                {
                    result = this.GetLoaderCommands().DRWPick(INSP, Module.Param.UseARM);

                    if (result == EventCodeEnum.NONE)
                    {
                        INSP.Holder.CurrentWaferInfo = INSP.Holder.TransferObject;
                        INSP.Holder.TransferObject.SetOCRState("", 0, ProberInterfaces.Enum.OCRReadStateEnum.NONE);
                        INSP.Holder.SetTransfered(ARM);
                        Loader.BroadcastLoaderInfo();

                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        Loader.ResonOfError = "InspectionTrayToArm Error. result:" + result.ToString();
                        //    INSP.Holder.SetUnknown();
                        //    ARM.Holder.SetUnknown();
                        LoggerManager.Error($"GP_InspectionTrayToARMState(): Transfer failed. Job result = {result}");
                        Loader.ResonOfError = $"DRW{INSP.ID.Index} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
                        result = INSP.MonitorForSubstrate(true); //베큠을 체크해본다.
                        if (result != EventCodeEnum.NONE) // INSP에 웨이퍼가 없을 경우
                        {
                            result = ARM.MonitorForVacuum(true); //ARM 베큠 체크
                            if (result != EventCodeEnum.NONE)
                            {
                                var clonedTransferObject = INSP.Holder.TransferObject.Clone() as TransferObject;
                                ARM.Holder.SetUnknown(clonedTransferObject);
                                INSP.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                            }
                            else
                            {
                                INSP.Holder.SetTransfered(ARM); // 있으면 ARM
                            }
                        }
                        Loader.BroadcastLoaderInfo();
                        StateTransition(new SystemErrorState(Module));
                    }
                }
                
            }
            catch (Exception err)
            {
                Loader.ResonOfError = "InspectionTrayToArm Error. result:" + err.ToString();
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (INSP.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = INSP.Holder.TransferObject.Clone() as TransferObject;
                }
                LoggerManager.Error($"GP_InspectionTrayToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");

                Loader.ResonOfError = $"DRW{INSP.ID.Index} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {err.Message}";
               var result = INSP.MonitorForSubstrate(true); //베큠을 체크해본다.
                if (result != EventCodeEnum.NONE) // INSP에 웨이퍼가 없을 경우
                {
                    result = ARM.MonitorForVacuum(true); //ARM 베큠 체크
                    if (result != EventCodeEnum.NONE)
                    {
                        INSP.Holder.SetUnknown(clonedTransferObject); //없으면 언노운 
                        ARM.Holder.SetUnknown(clonedTransferObject);
                    }
                    else
                    {
                        if (INSP.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            INSP.Holder.SetTransfered(ARM); // 있으면 PreAlign
                        }
                    }
                }
                else
                {
                    if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        ARM.Holder.SetTransfered(INSP); // 있으면 PreAlign
                    }
                }
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_InspectionTrayToARMState
    {
        public DoneState(GP_InspectionTrayToARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.INSP_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {INSP}, DestinationHolder: {ARM}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.INSP_TO_ARM, StateLogType.DONE, INSP.ID.Label, ARM.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_InspectionTrayToARMState
    {
        public SystemErrorState(GP_InspectionTrayToARM module) : base(module)
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
                    transObj = INSP.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_INSP_TO_ARM_TRANSFER_ERROR;
                LoggerManager.ActionLog(ModuleLogType.INSP_TO_ARM, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {INSP}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.INSP_TO_ARM, StateLogType.ERROR, INSP.ID.Label, ARM.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
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
