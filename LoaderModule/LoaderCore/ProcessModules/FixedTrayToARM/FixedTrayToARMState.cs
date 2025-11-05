using System;

using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderCore.FixedTrayToARMStates
{
    public abstract class FixedTrayToARMState : LoaderProcStateBase
    {
        public FixedTrayToARM Module { get; set; }

        public FixedTrayToARMState(FixedTrayToARM module)
        {
            this.Module = module;
        }

        protected void StateTransition(FixedTrayToARMState stateObj)
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

        protected IFixedTrayModule FIXED => Module.Param.Curr as IFixedTrayModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;
    }

    public class IdleState : FixedTrayToARMState
    {
        public IdleState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new FixedTrayDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class FixedTrayDownMoveState : FixedTrayToARMState
    {
        public FixedTrayDownMoveState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                FIXED.ValidateWaferStatus();
                if (FIXED.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the FIXED. ");
                    Loader.ResonOfError = $"{Module.GetType().Name}  There is no Wafer on the FIXED.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.ValidateWaferStatus();
                if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name}  There is Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //
                Module.Param.TransferObject.CleanPreAlignState(reason: "FixedTray To Arm.");

                retval = Loader.Move.FixedTrayDownMove(ARM, FIXED);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FixedTrayDownMove() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name}  FixedTrayDown Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                StateTransition(new PickUpState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PickUpState : FixedTrayToARMState
    {
        public PickUpState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = ARM.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WriteVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name}  ARM Vacuum Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PickUp(ARM, FIXED);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PickUp() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name}  PickUp Motion Error";
                    //=> Missing Wafer
                    FIXED.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PickUpErrorState(Module));
                    return;
                }

                retval = ARM.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name}  ARM Wait for Vacuum Error";
                    //=> Missing Wafer
                    FIXED.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new ARMVacuumErrorState(Module));
                    return;
                }

                //=> Transfer Done
                FIXED.Holder.SetTransfered(ARM);
                Loader.BroadcastLoaderInfo();

                retval = Loader.Move.RetractAll();
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> ARM에 존재하는게 확인 되었으므로 체크할 필요가 없어보임.
                //retVal = FIXED.MonitorForSubstrate(false);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    FIXED.Holder.SetUnknown();
                //    Loader.OnLoaderInfoChanged();

                //    StateTransition(new SystemErrorState(Module));
                //    return;
                //}

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : FixedTrayToARMState
    {
        public DoneState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : FixedTrayToARMState
    {
        public SystemErrorState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
        
    public class PickUpErrorState : FixedTrayToARMState
    {
        public PickUpErrorState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PickUpErrorState");
                //=> Check Wafer Location
                retval = ARM.WriteVacuum(true);
                retval = ARM.MonitorForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    //=> Unknown wafer location
                    retval = ARM.WriteVacuum(false);
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
        
    public class ARMVacuumErrorState : FixedTrayToARMState
    {
        public ARMVacuumErrorState(FixedTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} ARMVacuumErrorState");
            try
            {
                //=> Rollback to PlaceDown
                retval = ARM.WriteVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = Loader.Move.PlaceDown(ARM, FIXED, LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.MonitorForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WriteVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Retract ARM
                retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = FIXED.MonitorForSubstrate(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovery wafer location
                FIXED.Holder.SetLoad(Module.Param.TransferObject);
                ARM.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

