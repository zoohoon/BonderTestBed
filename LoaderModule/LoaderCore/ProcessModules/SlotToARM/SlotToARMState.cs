using System;

using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderCore.SlotToARMStates
{
    public abstract class SlotToARMState : LoaderProcStateBase
    {
        public SlotToARM Module { get; set; }

        public SlotToARMState(SlotToARM module)
        {
            this.Module = module;
        }

        protected void StateTransition(SlotToARMState stateObj)
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

        protected ISlotModule SLOT => Module.Param.Curr as ISlotModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected ICassetteModule Cassette => SLOT.Cassette;
    }

    public class IdleState : SlotToARMState
    {
        public IdleState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new SlotDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class SlotDownMoveState : SlotToARMState
    {
        public SlotDownMoveState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                SLOT.ValidateWaferStatus();
                if (SLOT.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the SLOT. ");
                    Loader.ResonOfError = "There is no Wafer on the SLOT.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.ValidateWaferStatus();
                if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the ARM. ");
                    Loader.ResonOfError = "There is Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                int cassetteNum = Cassette.ID.Index;
                retVal = Loader.ServiceCallback.FOUP_FoupTiltDown(cassetteNum);

                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);

                if (foupInfo.State != ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FoupState Not LOAD STATE . STATE={foupInfo.State} ");
                    Loader.ResonOfError = $"FoupState Not LOAD STATE . STATE={foupInfo.State} ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (foupInfo.TiltState != ProberInterfaces.Foup.TiltStateEnum.DOWN)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Tilt.State={foupInfo.TiltState}");
                    Loader.ResonOfError = $"Foup Tilt Not Down State. Tilt.State={foupInfo.TiltState} ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (foupInfo.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.OPEN)
                {
                    retVal = Loader.ServiceCallback.FOUP_MonitorForWaferOutSensor(cassetteNum, false);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FOUP_MonitorForWaferOutSensor(). ReturnValue={retVal} ");
                        Loader.ResonOfError = $"WaferOutSensor Error.";
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                //
                Module.Param.TransferObject.CleanPreAlignState(reason: "Slot To Arm.");

                retVal = Loader.Move.SlotDownMove(ARM, SLOT);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} SlotDownMove(ARM, SLOT) ReturnValue={retVal} ");
                    Loader.ResonOfError = $"SlotDown Motion Error.";
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

    public class PickUpState : SlotToARMState
    {
        public PickUpState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;
                retVal = ARM.WriteVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ARM.WriteVacuum(true) ReturnValue={retVal} ");
                    Loader.ResonOfError = "ARM VacuumError. Arm Failed To Detect Wafers.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PickUp(ARM, SLOT);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PickUp(ARM, SLOT) ReturnValue={retVal} ");
                    //=> Missing Wafer
                    SLOT.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    Loader.ResonOfError = "Motion Error Occurred While Picking Up The Wafer.";
                    StateTransition(new PickUpErrorState(Module));
                    return;
                }

                retVal = ARM.WaitForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ARM.WaitForVacuum(true) ReturnValue={retVal} ");
                    //=> Missing Wafer
                    SLOT.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    Loader.ResonOfError = "ARM VacuumError. Missing during Wafer Pickup.";
                    StateTransition(new ARMVacuumErrorState(Module));
                    return;
                }

                //=> Transfer Done
                SLOT.Holder.CurrentWaferInfo = SLOT.Holder.TransferObject;
                SLOT.Holder.SetTransfered(ARM);
                Loader.BroadcastLoaderInfo();

                retVal = Loader.Move.RetractAll();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retVal} ");
                    Loader.ResonOfError = "RetractAll Error.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                int cassetteNum = Cassette.ID.Index;

                retVal = Loader.ServiceCallback.FOUP_FoupTiltUp(cassetteNum);

                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);


                if (foupInfo.TiltState != ProberInterfaces.Foup.TiltStateEnum.UP)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Tilt.State={foupInfo.TiltState}");
                    Loader.ResonOfError = "Foup TiltUp Error";
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

    public class DoneState : SlotToARMState
    {
        public DoneState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : SlotToARMState
    {
        public SystemErrorState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/}
    }

    public class PickUpErrorState : SlotToARMState
    {
        public PickUpErrorState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PickUpErrorState");
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
                SLOT.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class ARMVacuumErrorState : SlotToARMState
    {
        public ARMVacuumErrorState(SlotToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} ARMVacuumErrorState");
                EventCodeEnum retVal;

                //=> Rollback to PlaceDown
                retVal = ARM.WriteVacuum(false);
                retVal = ARM.WaitForVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = Loader.Move.PlaceDown(ARM, SLOT, LoaderMovingTypeEnum.RECOVERY);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = ARM.WriteVacuum(true);
                retVal = ARM.MonitorForVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = ARM.WriteVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Retract
                retVal = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovery wafer location
                SLOT.Holder.SetLoad(Module.Param.TransferObject);
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

