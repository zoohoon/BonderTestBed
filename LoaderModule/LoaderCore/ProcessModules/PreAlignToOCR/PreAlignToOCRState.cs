using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;
using LoaderBase.AttachModules.ModuleInterfaces;

namespace LoaderCore.PreAlignToOCRStates
{
    public abstract class PreAlignToOCRState : LoaderProcStateBase
    {
        public PreAlignToOCR Module { get; set; }

        public PreAlignToOCRState(PreAlignToOCR module)
        {
            this.Module = module;
        }
        protected void StateTransition(PreAlignToOCRState stateObj)
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
        public ICognexProcessManager CognexProcessManager => Module.Container.Resolve<ICognexProcessManager>();

        protected IOCRReadable OCR => Module.Param.Next as IOCRReadable;

        protected IARMModule ARM => Module.Param.UseARM as IARMModule;
    }

    public class IdleState : PreAlignToOCRState
    {
        public IdleState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new PreAlignDownMoveState(Module));
        }
    }

    public class PreAlignDownMoveState : PreAlignToOCRState
    {
        public PreAlignDownMoveState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                PA.ValidateWaferStatus();
                if (PA.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the PreAlign.");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the PreAlign. ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.ValidateWaferStatus();
                if (ARM.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the ARM.");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the ARM. ";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PreAlignDownMove(ARM, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignDownMove() ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignDownMove Motion Error.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                //=> Rotate Notch
                //==> needRotate : 현재 notch의 각을 구하기위한 보정 용도, 
                //==> notch가 0도일때부터 가정하고 외부 입력을 통해 조금씩 각이 틀어질 것을 고려함
                //==> notch의 각을 구하기 위해 얼마 만큼 더해야 하는지에 대한 값
                Degree needRotateAngle = PA.CalcRatateOffsetNotchAngle(Module.Param.TransferObject, OCR);
                double needRotateAngleDist = (needRotateAngle.Value) * ConstantValues.V_DEGREE_TO_DIST;
                LoggerManager.Debug($"PreAlignToOCRStates.PreAlignDownMoveState(): TO Notch Angle = {PA.Holder.TransferObject.NotchAngle.Value}, Rel. = {needRotateAngleDist} ");
                retVal = Loader.Move.PreAlignRelMove(PA, needRotateAngleDist);

                PA.Holder.TransferObject.NotchAngle.Value += needRotateAngleDist / ConstantValues.V_DEGREE_TO_DIST;
                if(PA.Holder.TransferObject.NotchAngle.Value < 0)
                {
                    PA.Holder.TransferObject.NotchAngle.Value += 360.0;
                }

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove(PA, needRotateAngleDist) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignRelMove Motion Error.";
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

    public class PickUpState : PreAlignToOCRState
    {
        public PickUpState(PreAlignToOCR module) : base(module) { }

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
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PA.WaitForVacuum(false) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Vacuum Error.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = ARM.WriteVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ARM.WriteVacuum(true) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} Arm Vacuum Error.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retVal = Loader.Move.PickUp(ARM, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PickUp(ARM, PA) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PickUp Motion Error.";
                    PA.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PickUpErrorState(Module));
                    return;
                }

                retVal = ARM.WaitForVacuum(true);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} ARM.WaitForVacuum(true) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ARM Wait For Vacuum Error.";
                    PA.Holder.SetUnknown();
                    ARM.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new ARMVacuumErrorState(Module));
                }

                PA.Holder.SetTransfered(ARM);
                Loader.BroadcastLoaderInfo();

                StateTransition(new OCRMoveState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class OCRMoveState : PreAlignToOCRState
    {
        public OCRMoveState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                EventCodeEnum retVal;

                retVal = Loader.Move.OCRMoveFromPreAlignUp(ARM, OCR, PA);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} OCRMoveFromPreAlignUp(ARM, OCR, PA) ReturnValue={retVal}");
                    Loader.ResonOfError = $"{Module.GetType().Name} OCRMoveFromPreAlignUp Motion Error.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                ARM.Holder.SetPosition(OCR);
                Loader.BroadcastLoaderInfo();

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class DoneState : PreAlignToOCRState
    {
        public DoneState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : PreAlignToOCRState
    {
        public SystemErrorState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/}
    }

    public class PickUpErrorState : PreAlignToOCRState
    {
        public PickUpErrorState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { }

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

    public class ARMVacuumErrorState : PreAlignToOCRState
    {
        public ARMVacuumErrorState(PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { }

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

