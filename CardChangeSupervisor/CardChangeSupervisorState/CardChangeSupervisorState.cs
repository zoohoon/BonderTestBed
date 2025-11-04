using CardChangeSupervisor.CardChangeSupervisorState.InnerState;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Command;
using System;
using System.Runtime.CompilerServices;

namespace CardChangeSupervisor.CardChangeSupervisorState
{


    public abstract class CardChangeSupervisorStateBase
    {
        public CardChangeSupervisorStateBase(CardChangeSupervisor module)
        {
            try
            {
                this.Module = module;                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public CardChangeSupervisor Module { get; set; }
        public CardChangeInnerStateBase SubModule { get; set; }
        public abstract ModuleStateEnum ModuleState { get; }
        public abstract ModuleStateEnum GetModuleState();
        public virtual void Execute()
        {
            try
            {
                SubModule.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public virtual bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            try
            {
                retVal = SubModule.CanExecute(token);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        
        protected void RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            try
            {
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
     
    }

    public class CardChange_Idle : CardChangeSupervisorStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.IDLE;
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;

        public CardChange_Idle(CardChangeSupervisor module) : base(module)
        {
            SubModule = new IDLE(module);
        }

       
    }

    public class CardChange_Running : CardChangeSupervisorStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.RUNNING;
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;

        public CardChange_Running(CardChangeSupervisor module) : base(module)
        {
        }
      
    }

    public class CardChange_Done : CardChangeSupervisorStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.DONE;
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;

        public CardChange_Done(CardChangeSupervisor module) : base(module)
        {
        }
      
    }

    public class CardChange_Abort : CardChangeSupervisorStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ABORT;
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ABORT;

        public CardChange_Abort(CardChangeSupervisor module) : base(module)
        {
        }
      
    }

    public class CardChange_Error : CardChangeSupervisorStateBase
    {
        public override ModuleStateEnum ModuleState => ModuleStateEnum.ERROR;
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;

        public CardChange_Error(CardChangeSupervisor module) : base(module)
        {
        }

    }


}
