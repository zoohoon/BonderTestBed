using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;

namespace LoaderCore.ARMToFixedTrayStates
{
    public abstract class ARMToFixedTrayState : LoaderProcStateBase
    {
        public ARMToFixedTray Module { get; set; }

        public ARMToFixedTrayState(ARMToFixedTray module)
        {
            this.Module = module;
        }
        protected void StateTransition(ARMToFixedTrayState stateObj)
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

        protected IFixedTrayModule FIXED => Module.Param.Next as IFixedTrayModule;
    }

    public class IdleState : ARMToFixedTrayState
    {
        public IdleState(ARMToFixedTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new FixedTrayUpMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }

    public class FixedTrayUpMoveState : ARMToFixedTrayState
    {
        public FixedTrayUpMoveState(ARMToFixedTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                ARM.ValidateWaferStatus();

                FIXED.ValidateWaferStatus();

                if (ARM.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (FIXED.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the FixedTray. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the FixedTray.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

              
                retVal = Loader.Move.FixedTrayUpMove(ARM, FIXED);
                
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FixedTrayUpMove() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} FixedTrayUp Motion Error";
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

    public class PlaceDownMoveState : ARMToFixedTrayState
    {
        public PlaceDownMoveState(ARMToFixedTray module) : base(module) { }

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

                retVal = Loader.Move.PlaceDown(ARM, FIXED);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PlaceDown() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PlaceDown Motion Error";
                    ARM.Holder.SetUnknown();
                    FIXED.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PlaceDownErrorState(Module));
                    return;
                }

                ARM.Holder.SetOriginID(FIXED);
                ARM.Holder.SetTransfered(FIXED);
                Loader.BroadcastLoaderInfo();

                retVal = Loader.Move.RetractAll();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = FIXED.MonitorForSubstrate(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} MonitorForSubstrate() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} MonitorForSubstrate Error";
                    ARM.Holder.SetUnknown();
                    FIXED.Holder.SetUnknown();
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

    public class DoneState : ARMToFixedTrayState
    {
        public DoneState(ARMToFixedTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }

    public class SystemErrorState : ARMToFixedTrayState
    {
        public SystemErrorState(ARMToFixedTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class PlaceDownErrorState : ARMToFixedTrayState
    {
        public PlaceDownErrorState(ARMToFixedTray module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} ");
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
                FIXED.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
