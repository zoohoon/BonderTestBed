using System;

using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderCore.InspectionTrayToARMStates
{
    public abstract class InspectionTrayToARMState : LoaderProcStateBase
    {
        public InspectionTrayToARM Module { get; set; }

        public InspectionTrayToARMState(InspectionTrayToARM module)
        {
            this.Module = module;
        }

        protected void StateTransition(InspectionTrayToARMState stateObj)
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

        protected IInspectionTrayModule INSP => Module.Param.Curr as IInspectionTrayModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;
    }

    public class IdleState : InspectionTrayToARMState
    {
        public IdleState(InspectionTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new InspectionTrayDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class InspectionTrayDownMoveState : InspectionTrayToARMState
    {
        public InspectionTrayDownMoveState(InspectionTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                INSP.ValidateWaferStatus();
                if (INSP.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the InspectionTray. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the InspectionTray.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.ValidateWaferStatus();
                if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the ARM. ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //
                Module.Param.TransferObject.CleanPreAlignState(reason: "InspeciotnTray To Arm.");

                retval = Loader.Move.InspectionTrayDownMove(ARM, INSP);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} InspectionTrayDownMove() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} InspectionTrayDown Motion Error ";
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

    public class PickUpState : InspectionTrayToARMState
    {
        public PickUpState(InspectionTrayToARM module) : base(module) { }

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
                    Loader.ResonOfError = $"{Module.GetType().Name} ARM Vacuum Error ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PickUp(ARM, INSP);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PickUp() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PickUp Motion Error ";
                    //=> Missing Wafer
                    INSP.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PickUpErrorState(Module));
                    return;
                }

                retval = ARM.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Arm Wait For Vacuum Error ";
                    //=> Missing Wafer
                    INSP.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new ARMVacuumErrorState(Module));
                    return;
                }

                //=> Transfer Done
                INSP.Holder.SetTransfered(ARM);
                Loader.BroadcastLoaderInfo();

                retval = Loader.Move.RetractAll();
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> ARM에서 위치가 확인되었므로 생략
                //retVal = INSP.MonitorForSubstrate(false);
                //if (retVal != EventCodeEnum.NONE)
                //{
                //    INSP.Holder.SetUnknown();
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

    public class DoneState : InspectionTrayToARMState
    {
        public DoneState(InspectionTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : InspectionTrayToARMState
    {
        public SystemErrorState(InspectionTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
    
    public class PickUpErrorState : InspectionTrayToARMState
    {
        public PickUpErrorState(InspectionTrayToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PickUpErrorState");
            try
            {
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
                INSP.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
        
    public class ARMVacuumErrorState : InspectionTrayToARMState
    {
        public ARMVacuumErrorState(InspectionTrayToARM module) : base(module) { }

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
                retval = ARM.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = Loader.Move.PlaceDown(ARM, INSP, LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WriteVacuum(true);
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

                retval = INSP.MonitorForSubstrate(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovery wafer location
                INSP.Holder.SetLoad(Module.Param.TransferObject);
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

