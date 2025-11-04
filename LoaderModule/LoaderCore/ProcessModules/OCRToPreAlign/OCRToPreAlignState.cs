using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;
using LoaderBase.AttachModules.ModuleInterfaces;

namespace LoaderCore.OCRToPreAlignStates
{
    public abstract class OCRToPreAlignState : LoaderProcStateBase
    {
        public OCRToPreAlign Module { get; set; }

        public OCRToPreAlignState(OCRToPreAlign module)
        {
            this.Module = module;
        }

        protected void StateTransition(OCRToPreAlignState stateObj)
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
        public ICognexProcessManager CognexProcessManager => Module.Container.Resolve<ICognexProcessManager>();

        protected IOCRReadable OCR => Module.Param.Curr as IOCRReadable;

        protected IPreAlignModule PA => Module.Param.Next as IPreAlignModule;

        protected IARMModule ARM => Module.Param.UseARM;
    }

    public class IdleState : OCRToPreAlignState
    {
        public IdleState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new PreAlignUpMoveFromOCRState(Module));
        }
    }

    public class PreAlignUpMoveFromOCRState : OCRToPreAlignState
    {
        public PreAlignUpMoveFromOCRState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ARM.ValidateWaferStatus();

                PA.ValidateWaferStatus();

                if (ARM.Holder.Status != EnumSubsStatus.EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is no Wafer on the ARM. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is no Wafer on the ARM.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                if (PA.Holder.Status != EnumSubsStatus.NOT_EXIST)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} There is Wafer on the PreAlign. ");
                    Loader.ResonOfError = $"{Module.GetType().Name} There is Wafer on the PreAlign.";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PreAlignUpMoveFromOCR(ARM, PA, OCR);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlignMoveFromOCR Motion Error";
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

    public class PlaceDownMoveState : OCRToPreAlignState
    {
        public PlaceDownMoveState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = ARM.WriteVacuum(false);
                retval = ARM.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} ARM Vacuum Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }
                retval = Loader.MotionManager.AbsMove(
                    EnumAxisConstants.V,
                    Module.Param.TransferObject.NotchAngle.Value * ConstantValues.V_DEGREE_TO_DIST);

                retval = PA.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WriteVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Vacuum Error";
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                retval = Loader.Move.PlaceDown(ARM, PA);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PlaceDown() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PlaceDown Motion Error";
                    ARM.Holder.SetUnknown();
                    PA.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PlaceDownErrorState(Module));
                    return;
                }

                retval = PA.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} WaitForVacuum() ReturnValue={retval}");
                    Loader.ResonOfError = $"{Module.GetType().Name} PreAlign Wait For Vacuum Error";
                    ARM.Holder.SetUnknown();
                    PA.Holder.SetUnknown();
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new PreAlignVacuumErrorState(Module));
                    return;
                }


                //Degree needRotateAngle = PA.CalcRatateOffsetNotchAngle(Module.Param.TransferObject, OCR);
                Degree needRotateAngle = PA.GetProcessingParam(
                    Module.Param.TransferObject.Type.Value,
                    Module.Param.TransferObject.Size.Value).NotchSensorAngle.Value;
                double needRotateAngleDist = (needRotateAngle.Value - ARM.Holder.TransferObject.NotchAngle.Value) * ConstantValues.V_DEGREE_TO_DIST;
                LoggerManager.Debug($"OCRToPreAlignStates.PlaceDownMoveState(): TO Notch Angle = {ARM.Holder.TransferObject.NotchAngle.Value}, Rel. = {needRotateAngleDist} ");
                ARM.Holder.SetTransfered(PA);
                
                retval = Loader.Move.PreAlignRelMove(PA, needRotateAngleDist);
                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} PreAlignRelMove(PA, needRotateAngleDist) ReturnValue={retval}");
                    StateTransition(new SystemErrorState(Module));
                    return;
                }

                PA.Holder.TransferObject.NotchAngle.Value = 
                    PA.Holder.TransferObject.NotchAngle.Value + needRotateAngleDist / ConstantValues.V_DEGREE_TO_DIST;

                if(PA.Holder.TransferObject.NotchAngle.Value < 0)
                {
                    PA.Holder.TransferObject.NotchAngle.Value = PA.Holder.TransferObject.NotchAngle.Value + 360.0;
                }

                Loader.BroadcastLoaderInfo();

                if (Module.Param.DestPos == this.PA &&
                    Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.DONE)
                {
                    retval = Loader.Move.RetractAll();
                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[LOADER ERROR] {Module.GetType().Name} RetractAll() ReturnValue={retval}");
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

    public class DoneState : OCRToPreAlignState
    {
        public DoneState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }

    public class SystemErrorState : OCRToPreAlignState
    {
        public SystemErrorState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }

    public class PlaceDownErrorState : OCRToPreAlignState
    {
        public PlaceDownErrorState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PlaceDownErrorState");
            try
            {
                //=> Check Wafer Location On ARM
                bool isWaferOnARM;
                retval = ARM.WriteVacuum(true);
                retval = ARM.MonitorForVacuum(true);
                if (retval == EventCodeEnum.NONE)
                {
                    isWaferOnARM = true;
                }
                else
                {
                    isWaferOnARM = false;
                    retval = ARM.WriteVacuum(false);
                }

                //=> Check Wafer Location On PreAlign
                bool isWaferOnPA;
                retval = PA.WriteVacuum(true);
                retval = PA.MonitorForVacuum(true);
                if (retval == EventCodeEnum.NONE)
                {
                    isWaferOnPA = true;
                }
                else
                {
                    isWaferOnPA = false;
                    retval = PA.WriteVacuum(false);
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

    public class PreAlignVacuumErrorState : OCRToPreAlignState
    {
        public PreAlignVacuumErrorState(OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[LOADER SelfRecovery] {Module.GetType().Name} PreAlignVacuumErrorState");
            try
            {
                //=> Pick up move
                retval = PA.WriteVacuum(false);
                retval = PA.WaitForVacuum(false);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WriteVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = Loader.Move.PickUp(ARM, PA, LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                retval = ARM.WaitForVacuum(true);
                if (retval != EventCodeEnum.NONE)
                {
                    return;
                }

                //=> Recovered wafer location.
                ARM.Holder.SetLoad(Module.Param.TransferObject);
                PA.Holder.SetUnload();
                Loader.BroadcastLoaderInfo();

                //=> Retract ARM
                retval = Loader.Move.RetractAll(LoaderMovingTypeEnum.RECOVERY);
                if (retval != EventCodeEnum.NONE)
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
