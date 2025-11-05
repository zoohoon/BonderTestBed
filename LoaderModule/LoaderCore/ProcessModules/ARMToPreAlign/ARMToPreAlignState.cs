using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;

namespace LoaderCore.ARMToPreAlignStates
{
    public abstract class ARMToPreAlignState : LoaderProcStateBase
    {
        public ARMToPreAlign Module { get; set; }

        public ARMToPreAlignState(ARMToPreAlign module)
        {
            this.Module = module;
        }
        protected void StateTransition(ARMToPreAlignState stateObj)
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

        protected IPreAlignModule PA => Module.Param.Next as IPreAlignModule;
    }

    public class IdleState : ARMToPreAlignState
    {
        public IdleState(ARMToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {

                StateTransition(new PreAlignUpMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PreAlignUpMoveState : ARMToPreAlignState
    {
        public PreAlignUpMoveState(ARMToPreAlign module) : base(module) { }

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
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the ARM. ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                PA.ValidateWaferStatus();
                if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the PreAlign. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the PreAlign.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PreAlignUpMove(ARM, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER] {Module.GetType().Name} PreAlignUpMove() ReturnValue={retVal}");

                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignUp Motion Error";
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

    public class PlaceDownMoveState : ARMToPreAlignState
    {
        public PlaceDownMoveState(ARMToPreAlign module) : base(module) { }

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
                retVal = Loader.MotionManager.AbsMove(
                    EnumAxisConstants.V,
                    Module.Param.TransferObject.NotchAngle.Value * ConstantValues.V_DEGREE_TO_DIST);

                retVal = PA.WriteVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WriteVacuum() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Vacuum Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PlaceDown(ARM, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PlaceDown() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PlaceDown Motion Error";
                    ARM.Holder.SetUnknown();
                    PA.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PlaceDownErrorState(Module));
                    return;
                }

                retVal = PA.WaitForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Vacuum Error";
                    ARM.Holder.SetUnknown();
                    PA.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PreAlignVacuumErrorState(Module));
                    return;
                }

                ARM.Holder.SetTransfered(PA);
                Loader.BroadcastLoaderInfo();

                if (Module.Param.DestPos == this.PA &&
                    Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.DONE)
                {
                    retVal = Loader.Move.RetractAll();
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retVal}");
                        Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error";
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : ARMToPreAlignState
    {
        public DoneState(ARMToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }

    public class SystemErrorState : ARMToPreAlignState
    {
        public SystemErrorState(ARMToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class PlaceDownErrorState : ARMToPreAlignState
    {
        public PlaceDownErrorState(ARMToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            try
            {

                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PlaceDownErrorState");
                EventCodeEnum retVal;
                //=> Check Wafer Location On ARM
                bool isWaferOnARM;
                retVal = ARM.WriteVacuum(true);
                retVal = ARM.MonitorForVacuum(true);
                if (retVal == EventCodeEnum.NONE)
                {
                    isWaferOnARM = true;
                }
                else
                {
                    isWaferOnARM = false;
                    retVal = ARM.WriteVacuum(false);
                }

                //=> Check Wafer Location On PreAlign
                bool isWaferOnPA;
                retVal = PA.WriteVacuum(true);
                retVal = PA.MonitorForVacuum(true);
                if (retVal == EventCodeEnum.NONE)
                {
                    isWaferOnPA = true;
                }
                else
                {
                    isWaferOnPA = false;
                    retVal = PA.WriteVacuum(false);
                }

                if (isWaferOnARM == true && isWaferOnPA == false)
                {
                    //=> Recovered wafer location on ARM.
                    ARM.Holder.SetLoad(Module.Param.TransferObject);
                    PA.Holder.SetUnload();
                    Loader.BroadcastLoaderInfo();
                }
                else if (isWaferOnARM == false && isWaferOnPA == true)
                {
                    //=> Recovered wafer location on PA.
                    ARM.Holder.SetUnload();
                    PA.Holder.SetLoad(Module.Param.TransferObject);
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    //=> failed.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PreAlignVacuumErrorState : ARMToPreAlignState
    {
        public PreAlignVacuumErrorState(ARMToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PreAlignVacuumErrorState");
                EventCodeEnum retVal;
                //=> Pick up move
                retVal = PA.WriteVacuum(false);
                retVal = PA.WaitForVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = ARM.WriteVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = Loader.Move.PickUp(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = ARM.WaitForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovered wafer location.
                ARM.Holder.SetLoad(Module.Param.TransferObject);
                PA.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();

                //=> Retract ARM
                retVal = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }

}
