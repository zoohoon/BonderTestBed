using System;

using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderCore.PreAlignToARMStates
{
    public abstract class PreAlignToARMState : LoaderProcStateBase
    {
        public PreAlignToARM Module { get; set; }

        public PreAlignToARMState(PreAlignToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(PreAlignToARMState stateObj)
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

        protected IPreAlignModule PA => Module.Param.Curr as IPreAlignModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;
    }

    public class IdleState : PreAlignToARMState
    {
        public IdleState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                StateTransition(new PreAlignDownMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PreAlignDownMoveState : PreAlignToARMState
    {
        public PreAlignDownMoveState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                PA.ValidateWaferStatus();
                if (PA.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the PreAlign. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the PreAlign. ";
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

                var transferObj = PA.Holder.TransferObject;

                retVal = Loader.Move.PreAlignDownMove(ARM, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignDownMove(ARM, PA) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignDown Motion Error ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Rotate Notch
                if (Module.Param.DestPos is IHasLoadNotchAngle)
                {
                    var Target = Module.Param.DestPos as IHasLoadNotchAngle;
                    Degree needRotateAngle = PA.CalcRatateOffsetNotchAngle(Module.Param.TransferObject, Target);
                    double needRotateAngleDist = needRotateAngle.Value * ConstantValues.V_DEGREE_TO_DIST;

                    retVal = Loader.Move.PreAlignRelMove(PA, needRotateAngleDist);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove(PA, needRotateAngleDist) ReturnValue={retVal}");
                        Loader.ResonOfError = $"{Module.GetType().Name} PreAlignRelMove Motion Error ";
                        StateTransition(new SystemErrorState(Module));
                        return;
                    }
                    PA.Holder.TransferObject.NotchAngle.Value = needRotateAngle.Value;
                }

                StateTransition(new PickUpState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class PickUpState : PreAlignToARMState
    {
        public PickUpState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;
                retVal = PA.WriteVacuum(false);
                retVal = PA.WaitForVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum(false) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Vacuum Error ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = ARM.WriteVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WriteVacuum(true) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Arm Vacuum Error ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PickUp(ARM, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PickUp(ARM, PA) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PickUp Motion Error ";
                    PA.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PickUpErrorState(Module));
                    return;
                }

                retVal = ARM.WaitForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum(true) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ARM Wait For Vacuum Error ";
                    PA.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new ARMVacuumErrorState(Module));
                }

                PA.Holder.SetTransfered(ARM);
                Loader.BroadcastLoaderInfo();

                retVal = Loader.Move.RetractAll();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} RetractAll Motion Error ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PreAlignZeroMove(PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignZeroMove(PA) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignZeroMove Motion Error ";
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

    public class DoneState : PreAlignToARMState
    {
        public DoneState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : PreAlignToARMState
    {
        public SystemErrorState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class PickUpErrorState : PreAlignToARMState
    {
        public PickUpErrorState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PickUpErrorState");
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
                    return;
                }
                else if (isWaferOnARM == false && isWaferOnPA == true)
                {
                    //=> Recovered wafer location on PA.
                    ARM.Holder.SetUnload();
                    PA.Holder.SetLoad(Module.Param.TransferObject);
                    Loader.BroadcastLoaderInfo();
                    return;
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

    public class ARMVacuumErrorState : PreAlignToARMState
    {
        public ARMVacuumErrorState(PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery()
        {
            try
            {
                LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} ARMVacuumErrorState");
                EventCodeEnum retVal;

                //=> Place down move
                retVal = ARM.WriteVacuum(false);
                retVal = ARM.WaitForVacuum(false);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = PA.WriteVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = Loader.Move.PlaceDown(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                retVal = PA.WaitForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovered wafer location.
                PA.Holder.SetLoad(Module.Param.TransferObject);
                ARM.Holder.SetUnload();
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

