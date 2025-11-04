using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;

namespace LoaderCore.ARMToInspectionTrayStates
{
    public abstract class ARMToInspectionTrayState : LoaderProcStateBase
    {
        public ARMToInspectionTray Module { get; set; }

        public ARMToInspectionTrayState(ARMToInspectionTray module)
        {
            this.Module = module;
        }
        protected void StateTransition(ARMToInspectionTrayState stateObj)
        {
            try
            {
                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state tranition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected IARMModule ARM => Module.Param.Curr as IARMModule;

        protected IInspectionTrayModule INSP => Module.Param.Next as IInspectionTrayModule;
    }

    public class IdleState : ARMToInspectionTrayState
    {
        public IdleState(ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new InspectionTrayUpMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class InspectionTrayUpMoveState : ARMToInspectionTrayState
    {
        public InspectionTrayUpMoveState(ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {

                EventCodeEnum retVal;

                ARM.ValidateWaferStatus();
                if (ARM.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                INSP.ValidateWaferStatus();
                if (INSP.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the InspectionTray. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the InspectionTray.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.InspectionTrayUpMove(ARM, INSP);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} InspectionTrayUpMove() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} InspectionTrayUp Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                StateTransition(new PlaceDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PlaceDownMoveState : ARMToInspectionTrayState
    {
        public PlaceDownMoveState(ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {

                EventCodeEnum retVal;
                retVal = ARM.WriteVacuum(false);
                retVal = ARM.WaitForVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ARM Vacuum Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PlaceDown(ARM, INSP);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PlaceDown() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PlaceDown Motion Error";
                    ARM.Holder.SetUnknown();
                    INSP.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PlaceDownErrorState(Module));
                    return;
                }

                ARM.Holder.SetOriginID(INSP);
                INSP.Holder.CurrentWaferInfo = null;
                ARM.Holder.SetTransfered(INSP);
                Loader.BroadcastLoaderInfo();

                retVal = Loader.Move.RetractAll();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = INSP.MonitorForSubstrate(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} MonitorForSubstrate() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} MonitorForSubstrate Error";
                    ARM.Holder.SetUnknown();
                    INSP.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : ARMToInspectionTrayState
    {
        public DoneState(ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }

    public class SystemErrorState : ARMToInspectionTrayState
    {
        public SystemErrorState(ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class PlaceDownErrorState : ARMToInspectionTrayState
    {
        public PlaceDownErrorState(ARMToInspectionTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name}");
                //=> Check Wafer Location
                EventCodeEnum retVal;
                retVal = ARM.WriteVacuum(true);
                retVal = ARM.MonitorForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    //=> Unknown wafer location
                    retVal = ARM.WriteVacuum(false);
                    return;
                }

                //=> Recovered wafer location.
                ARM.Holder.SetLoad(Module.Param.TransferObject);
                INSP.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

}

