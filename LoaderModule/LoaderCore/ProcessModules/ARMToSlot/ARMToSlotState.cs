using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;

namespace LoaderCore.ARMToSlotStates
{
    public abstract class ARMToSlotState : LoaderProcStateBase
    {
        public ARMToSlot Module { get; set; }

        public ARMToSlotState(ARMToSlot module)
        {
            this.Module = module;
        }

        protected void StateTransition(ARMToSlotState stateObj)
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

        protected ISlotModule SLOT => Module.Param.Next as ISlotModule;

        protected ICassetteModule Cassette => SLOT.Cassette;
    }

    public class IdleState : ARMToSlotState
    {
        public IdleState(ARMToSlot module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new SlotUpMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class SlotUpMoveState : ARMToSlotState
    {
        public SlotUpMoveState(ARMToSlot module) : base(module) { }

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

                SLOT.ValidateWaferStatus();
                if (SLOT.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the SLOT. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the SLOT.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                var srcDesc = SLOT.GetSourceDeviceInfo();
                if (srcDesc.Type.Value != Module.Param.TransferObject.Type.Value ||
                    srcDesc.Size.Value != Module.Param.TransferObject.Size.Value)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} GetSourceDeviceInfo(). ");
                    Loader.ResonOfError = $"{Module.GetType().Name} Wafer Size or Wafer Type No Matched";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                int cassetteNum = Cassette.ID.Index;

                retVal = Loader.ServiceCallback.FOUP_FoupTiltDown(cassetteNum);

                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);

                if (foupInfo.State != ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FoupState={foupInfo.State}. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} Foup Not a Load State. FoupState={foupInfo.State}.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (foupInfo.TiltState != ProberInterfaces.Foup.TiltStateEnum.DOWN)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Tilt.State={foupInfo.TiltState}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Tilt Not a DOWN State. TiltState={foupInfo.TiltState}.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                if (foupInfo.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.OPEN)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FoupCoverState={foupInfo.FoupCoverState}. ");
                    retVal = Loader.ServiceCallback.FOUP_MonitorForWaferOutSensor(cassetteNum, false);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} FOUP_MonitorForWaferOutSensor() ReturnValue={retVal} ");
                        Loader.ResonOfError = $"{Module.GetType().Name} WaferOutSensor Error";
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                }

                retVal = Loader.Move.SlotUpMove(ARM, SLOT);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} SlotUpMove() ReturnValue={retVal} ");
                    Loader.ResonOfError = $"{Module.GetType().Name} SlotUpMove Motion Error";
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

    public class PlaceDownMoveState : ARMToSlotState
    {
        public PlaceDownMoveState(ARMToSlot module) : base(module) { }

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
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retVal} ");
                    Loader.ResonOfError = $"{Module.GetType().Name} Arm Vacuum Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PlaceDown(ARM, SLOT);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PlaceDown() ReturnValue={retVal} ");
                    Loader.ResonOfError = $"{Module.GetType().Name} PlaceDown Motion Error";
                    ARM.Holder.SetUnknown();
                    SLOT.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PlaceDownErrorState(Module));
                    return;
                }
                ISlotModule originSlotModule = Loader.ModuleManager.FindModule(ARM.Holder.TransferObject.OriginHolder) as ISlotModule;

                int originSlotNum = ARM.Holder.TransferObject.OriginHolder.Index;
                int changeSlotNum = SLOT.ID.Index;
                ARM.Holder.SetOriginID(SLOT);
                SLOT.Holder.CurrentWaferInfo = null;
                ARM.Holder.SetTransfered(SLOT);

                if(originSlotNum!= changeSlotNum)
                {
                    if(originSlotModule!=null&& originSlotModule.Holder.Status==EnumSubsStatus.EXIST)
                    {
                        Loader.ServiceCallback.WaferSwapChanged(originSlotNum, changeSlotNum,false);
                    }
                    else
                    {
                        Loader.ServiceCallback.WaferSwapChanged(originSlotNum, changeSlotNum,true);
                    }
                }

                Loader.ServiceCallback.WaferStateChanged(SLOT.Holder.TransferObject.OriginHolder.Index, SLOT.Holder.Status, SLOT.Holder.TransferObject.WaferState);
                Loader.BroadcastLoaderInfo();

                retVal = Loader.Move.RetractAll();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retVal} ");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }


                int cassetteNum = Cassette.ID.Index;

                retVal = Loader.ServiceCallback.FOUP_FoupTiltUp(cassetteNum);

                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);


                if (foupInfo.TiltState != ProberInterfaces.Foup.TiltStateEnum.UP)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} Tilt.State={foupInfo.TiltState}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Foup Tilt Not a Up State. Tilt.State={foupInfo.TiltState}";
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

    public class DoneState : ARMToSlotState
    {
        public DoneState(ARMToSlot module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : ARMToSlotState
    {
        public SystemErrorState(ARMToSlot module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class PlaceDownErrorState : ARMToSlotState
    {
        public PlaceDownErrorState(ARMToSlot module) : base(module) { }

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
                SLOT.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

}
