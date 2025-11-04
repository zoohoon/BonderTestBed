using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoaderCore.GPProcessModules.CloseFoupCover
{
    public abstract class GP_CloseFoupCoverState : LoaderProcStateBase
    {
        public GP_CloseFoupCover Module { get; set; }
        public GP_CloseFoupCoverState(GP_CloseFoupCover module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_CloseFoupCoverState stateObj)
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

        ILoaderModule loader;
        protected ILoaderModule Loader
        {
            get
            {
                if (loader == null)
                {
                    loader = Module.Container.Resolve<ILoaderModule>();
                }
                return loader;
            }
        }
        protected ICassetteModule Cassette => Module.Param.Cassette as ICassetteModule;        
    }

    public class IdleState : GP_CloseFoupCoverState
    {
        public IdleState(GP_CloseFoupCover module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.CLOSE_FOUP_COVER;
                Loader.ProcModuleInfo.Source = Cassette.ID;
                Loader.ProcModuleInfo.Destnation = Cassette.ID;                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CloseFoupCoverState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;
        public override void Execute()
        {
            try
            {
                StateTransition(new RunningState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.ToString());
                StateTransition(new SystemErrorState(Module));
            }
            
        }
    }    

    public class RunningState : GP_CloseFoupCoverState
    {
        public RunningState(GP_CloseFoupCover module) : base(module) { }
        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;
        public override void Execute()
        {            
            try
            {
                // open 상태라면, close foup cover 하는 동작 구현
                // 어느 foup 닫을지 판단하기
                //int cassetteNum = Cassette.ID.Index;
                //var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);
                   
                if (Cassette.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.OPEN)
                {
                    EventCodeEnum retVal = this.FoupOpModule().GetFoupController(Cassette.ID.Index).Execute(new FoupCoverDownCommand());
                    if(retVal == EventCodeEnum.NONE)
                    {
                        Cassette.FoupCoverState = FoupCoverStateEnum.CLOSE;
                        LoggerManager.Debug("GP_CloseFoupCoverState:RunningState / FoupCoverState: CLOSE");

                        StateTransition(new DoneState(Module));
                    }
                    else
                    {
                        StateTransition(new SystemErrorState(Module));
                    }                    
                }                
                else if(Cassette.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.ERROR)
                {
                    StateTransition(new SystemErrorState(Module));
                } 
                else
                {
                    StateTransition(new DoneState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_CloseFoupCoverState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }
    }
    public class DoneState : GP_CloseFoupCoverState
    {
        public DoneState(GP_CloseFoupCover module) : base(module) { }
        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;        

        public override void Execute() { }
    }
    public class SystemErrorState : GP_CloseFoupCoverState
    {
        public SystemErrorState(GP_CloseFoupCover module) : base(module) 
        {
            try
            {
                Loader.LoaderMaster.NotifyManager().Notify(EventCodeEnum.FOUP_CLOSE_ERROR);
                LoggerManager.ActionLog(ModuleLogType.CLOSE_FOUP_COVER, StateLogType.ERROR, $"Close Foup Cover Error.");                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
