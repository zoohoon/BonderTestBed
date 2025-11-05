using Autofac;
using LoaderBase;
using LoaderBase.AttachModules.ModuleInterfaces;
using LoaderParameters;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Enum;
using System;

namespace LoaderCore.GP_PreAlignToOCRStates
{
    public abstract class GP_PreAlignToOCRState : LoaderProcStateBase
    {
        public GP_PreAlignToOCR Module { get; set; }

        public GP_PreAlignToOCRState(GP_PreAlignToOCR module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_PreAlignToOCRState stateObj)
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

    public class IdleState : GP_PreAlignToOCRState
    {
        public IdleState(GP_PreAlignToOCR module) : base(module) {
            Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.PREALIGN_TO_OCR;
            Loader.ProcModuleInfo.Source = PA.ID;
            Loader.ProcModuleInfo.Destnation = OCR.ID;
            Loader.ProcModuleInfo.Origin = PA.Holder.TransferObject.OriginHolder;
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }

    public class RunningState : GP_PreAlignToOCRState
    {
        public RunningState(GP_PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Degree needRotateAngle = PA.CalcRatateOffsetNotchAngle(this.Module.Param.TransferObject, OCR);
                double needRotateAngleDist =
                    (needRotateAngle.Value - PA.Holder.TransferObject.NotchAngle.Value)
                    * ConstantValues.V_DEGREE_TO_DIST;

                OCRAccessParam accparam = OCR.GetAccessParam( PA.Holder.TransferObject.Type.Value, PA.Holder.TransferObject.Size.Value);
                if (accparam != null)
                {
                    double angleoffset = 0;
                    double OCRReadXPos = 0;
                    double OCRReadYPos = 0;
                    SubchuckMotionParam subchuckMotionParam = OCR.GetSubchuckMotionParam(PA.Holder.TransferObject.Size.Value);
                    if (subchuckMotionParam != null)
                    {
                        angleoffset = subchuckMotionParam.SubchuckAngle_Offset.Value;
                        OCRReadXPos = subchuckMotionParam.SubchuckXCoord.Value;
                        OCRReadYPos = subchuckMotionParam.SubchuckYCoord.Value;
                        LoggerManager.Debug($"[GP_PreAlignToOCR] Host Index: {PA.ID.Index}, Wafer Size: {PA.Holder.TransferObject.Size.Value}," +
                           $" OCR Angle: {accparam.VPos.Value}, OCR Position (X, Y, Angle Offset): ({OCRReadXPos}, {OCRReadYPos}, {angleoffset})");
                    }

                    var angle = accparam.VPos.Value + angleoffset;
                    if (angle < 0)
                    {
                        angle = 360 + angle;
                    }
                    else if (angle >= 360)
                    {
                        angle = angle % 360;
                    }
                   
                    ret = this.GetLoaderCommands().PAMove(PA, angle);
                    if (ret == EventCodeEnum.NONE)
                    {
                        ret = this.GetLoaderCommands().PAMove(PA, OCRReadXPos, OCRReadYPos, 0);
                    }
                }

                if (ret == EventCodeEnum.NONE)
                {
                    PA.Holder.CurrentWaferInfo = PA.Holder.TransferObject;
                    PA.Holder.TransferObject.OCRReadState = OCRReadStateEnum.READING;
                    PA.Holder.TransferObject.CurrPos = OCR.ID;
                    Loader.BroadcastLoaderInfo();
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    StateTransition(new SystemErrorState(Module));
                }
                //=> Rotate Notch
                //==> needRotate : 현재 notch의 각을 구하기위한 보정 용도, 
                //==> notch가 0도일때부터 가정하고 외부 입력을 통해 조금씩 각이 틀어질 것을 고려함
                //==> notch의 각을 구하기 위해 얼마 만큼 더해야 하는지에 대한 값

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }

    public class PickUpState : GP_PreAlignToOCRState
    {
        public PickUpState(GP_PreAlignToOCR module) : base(module) { }

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

    public class OCRMoveState : GP_PreAlignToOCRState
    {
        public OCRMoveState(GP_PreAlignToOCR module) : base(module) { }

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

    public class DoneState : GP_PreAlignToOCRState
    {
        public DoneState(GP_PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/}
    }

    public class SystemErrorState : GP_PreAlignToOCRState
    {
        public SystemErrorState(GP_PreAlignToOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/}

        public override void SelfRecovery() { /*NoWORKS*/}
    }

    public class PickUpErrorState : GP_PreAlignToOCRState
    {
        public PickUpErrorState(GP_PreAlignToOCR module) : base(module) { }

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

    public class ARMVacuumErrorState : GP_PreAlignToOCRState
    {
        public ARMVacuumErrorState(GP_PreAlignToOCR module) : base(module) { }

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
