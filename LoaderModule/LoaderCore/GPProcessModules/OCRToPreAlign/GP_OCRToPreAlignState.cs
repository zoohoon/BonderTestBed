using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_OCRToPreAlignStates
{
    public abstract class GP_OCRToPreAlignState : LoaderProcStateBase
    {
        public GP_OCRToPreAlign Module { get; set; }

        public GP_OCRToPreAlignState(GP_OCRToPreAlign module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_OCRToPreAlignState stateObj)
        {
            try
            {

                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state transition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected IOCRReadable OCR => Module.Param.Curr as IOCRReadable;

        protected IPreAlignModule PA => Module.Param.Next as IPreAlignModule;

        protected IARMModule ARM => Module.Param.UseARM;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_OCRToPreAlignState
    {
        public IdleState(GP_OCRToPreAlign module) : base(module) {

            Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.OCR_TO_PREALIGN;
            Loader.ProcModuleInfo.Source = PA.ID;
            Loader.ProcModuleInfo.Destnation = PA.ID;
            //Loader.ProcModuleInfo.Origin = PA.Holder.TransferObject.OriginHolder;
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_OCRToPreAlignState
    {
        public RunningState(GP_OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;

            double dstNotchAngle = 0;
            if (PA.Holder.Status == EnumSubsStatus.EXIST)
            {
                dstNotchAngle = PA.Holder.TransferObject.NotchAngle.Value - 90;

                dstNotchAngle = dstNotchAngle % 360;

                if (dstNotchAngle < 0)
                {
                    dstNotchAngle += 360;
                }
            }
            this.Loader.PAManager.PAModules[PA.ID.Index - 1].DoPreAlignAsync(dstNotchAngle);



            if (ret == EventCodeEnum.NONE)
            {
                PA.Holder.TransferObject.CurrPos = PA.ID;
                StateTransition(new DoneState(Module));
            }
            else
            {
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_OCRToPreAlignState
    {
        public DoneState(GP_OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_OCRToPreAlignState
    {
        public SystemErrorState(GP_OCRToPreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
