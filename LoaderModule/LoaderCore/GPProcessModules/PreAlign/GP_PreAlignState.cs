using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;

namespace LoaderCore.GP_PreAlignStates
{
    public abstract class GP_PreAlignState : LoaderProcStateBase
    {
        public GP_PreAlign Module { get; set; }

        public GP_PreAlignState(GP_PreAlign module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_PreAlignState stateObj)
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

        protected IPreAlignModule PA => Module.Param.UsePA as IPreAlignModule;

        protected IARMModule ARM => Module.Param.UseARM;

        protected PreAlignData Data => Module.Data;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_PreAlignState
    {
        public IdleState(GP_PreAlign module) : base(module) {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.PREALIGN;
                Loader.ProcModuleInfo.Source = PA.ID;
                Loader.ProcModuleInfo.Destnation = PA.ID;
                Loader.ProcModuleInfo.Origin = PA.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_PreAlignState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_PreAlignState
    {
        public RunningState(GP_PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {

            //if (this.Loader.LoaderMaster.ModuleState.GetState() != ProberInterfaces.ModuleStateEnum.RUNNING)
            //{
            //    var retVal = this.GetLoaderCommands().WaitForPA(PA);

            //    if(retVal==0)
            //    {
            //        PA.Holder.TransferObject.SetPreAlignDone(PA.ID);
            //        StateTransition(new DoneState(Module));
            //    }
            //    else
            //    {
            //        PA.Holder.SetUnknown();
            //        LoggerManager.Error($"GP_PreAlignState(): PreAlign failed. Job result = {retVal}");
            //        StateTransition(new SystemErrorState(Module));
            //    }
            //}
            //else
            //{
            if (PA.Holder.TransferObject.PreAlignState != ProberInterfaces.PreAlignStateEnum.SKIP) 
            {
                PA.Holder.TransferObject.SetPreAlignDone(PA.ID);
            }
            StateTransition(new DoneState(Module));
            //}
            
        }
    }
    public class DoneState : GP_PreAlignState
    {
        public DoneState(GP_PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_PreAlignState
    {
        public SystemErrorState(GP_PreAlign module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
