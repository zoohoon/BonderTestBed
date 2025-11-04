
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.NeedleClean;
using System;
using System.Collections.Generic;
using System.Linq;
using LogModule;
using ProberInterfaces.State;
using ProberInterfaces.Command.Internal;

namespace NeedleCleanerModule
{
    public abstract class NeedleCleanState : IFactoryModule, IInnerState
    {
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract NeedleCleanStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        //public virtual EventCodeEnum ClearState()
        //{
        //    return EventCodeEnum.NONE;
        //}
        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class NeedleCleanModuleStateBase : NeedleCleanState
    {
        private NeedleCleanModule _Module;

        public NeedleCleanModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public NeedleCleanModuleStateBase(NeedleCleanModule module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }
    public class NeedleCleanModuleIdleState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModuleIdleState(NeedleCleanModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    return EventCodeEnum.NONE;
                }
                else
                {
                    if (Module.Template != null)
                    {
                        List<ISubModule> modules = Module.Template.GetProcessingModule();

                        if (Module.Template.SchedulingModule != null)
                        {
                            if (Module.Template.SchedulingModule.IsExecute())
                            {
                                foreach (var subModule in modules)
                                {
                                    if (subModule.IsExecute())
                                    {
                                        this.StageSupervisor().NCObject.PinAlignBeforeCleaningProcessed = false;
                                        this.StageSupervisor().NCObject.PinAlignAfterCleaningProcessed = false;

                                        Module.InnerStateTransition(new NeedleCleanModuleRunningState(Module));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.IDLE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new NeedleCleanModulePauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class NeedleCleanModuleRunningState : NeedleCleanModuleStateBase
    {
        private IProberStation Prober;
        //private NeedleCleanViewModel nvm;
        public NeedleCleanModuleRunningState(NeedleCleanModule module) : base(module)
        {
            try
            {
                Prober = this.ProberStation();
                //nvm = new NeedleCleanViewModel((ProbeCard)this.StageSupervisor().ProbeCardInfo,
                //    (NeedleCleanDeviceParameter)module.DevParam,
                //    (NeedleCleanSystemParameter)module.SysParam,
                //    (NeedleCleanObject)this.StageSupervisor().NCObject,
                //    (WaferObject)this.StageSupervisor().WaferObject
                //    );
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Prober.LotOP.ChangeMainViewTarget()
                //this.LotOPModule().NCToLotScreen();


                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                {
                    this.LotOPModule().ChangeMainViewUserTarget(Module.NeedleCleanVM);
                    //TestRemove
                    //this.ViewModelManager().Lock(this.GetHashCode(), "Needle Cleaning", "");

                    retVal = Module.DoNeedleCleaningProcess();
                }
                else if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSING)
                {
                    Module.InnerStateTransition(new NeedleCleanModuleDoneState(Module));
                }
                else if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.ABORT)
                {
                    Module.InnerStateTransition(new NeedleCleanModuleDoneState(Module));
                }
                if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                {
                    Module.InnerStateTransition(new NeedleCleanModuleDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.RUNNING;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class NeedleCleanModuleSuspendState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModuleSuspendState(NeedleCleanModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if (this.PinAligner().CommandRecvDoneSlot.IsRequested<IDOPINALIGN>() == true)
                if (this.PinAligner().CommandRecvDoneSlot.Token.Sender == Module &&
                    this.PinAligner().CommandRecvDoneSlot.Token is IDOPINALIGN)
                {
                    if (this.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.DONE)
                    {
                        Module.InnerStateTransition(new NeedleCleanModuleRunningState(Module));
                    }
                    else if (this.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ABORTED ||
                             this.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ERROR)
                    {
                        //Module.ReasonOfError.Reason = "Pin Alignment is failed";

                        LoggerManager.Debug("Request pin alignment for needle cleaning... failed (pin alignment is failed)");
                        //Module.InnerStateTransition(new NeedleCleanModuleErrorState(Module));
                        Module.InnerStateTransition(new NeedleCleanModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Request pin alignment for needle cleaning... failed (pin alignment is failed)", Module.NeedleCleanState.GetType().Name)));

                    }
                    else if (this.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.NOCOMMAND)
                    {
                        LoggerManager.Debug("Request pin alignment before needle cleaning... failed. (no response)");
                        //Module.ReasonOfError.Reason = "Pin Alignment Command no response";
                        //Module.InnerStateTransition(new NeedleCleanModuleErrorState(Module));
                        Module.InnerStateTransition(new NeedleCleanModuleErrorState(Module, new EventCodeInfo(Module.ReasonOfError.ModuleType, retVal, "Request pin alignment before needle cleaning... failed. (no response)", Module.NeedleCleanState.GetType().Name)));
                    }
                    else if (this.PinAligner().CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.REJECTED)
                    {
                        LoggerManager.Debug("Request pin alignment for needle cleaning... failed. (busy)");
                        Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.SUSPENDED;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.SUSPENDED;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class NeedleCleanModuleDoneState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModuleDoneState(NeedleCleanModule module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            //EventCodeEnum retVal = EventCodeEnum.UNDEFINED;                        
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //return retVal;

                Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
                //List<ISubModule> modules = Module.Template.GetProcessingModule();

                //if (Module.Template.SchedulingModule != null)
                //{
                //    if (Module.Template.SchedulingModule.IsExecute())
                //    {
                //        foreach (var subModule in modules)
                //        {
                //            subModule.ClearData();
                //        }

                //        Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
                //    }
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;



        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.DONE;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new NeedleCleanModulePauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class NeedleCleanModulePauseState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModulePauseState(NeedleCleanModule module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.PAUSED;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(Module.PreInnerState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new NeedleCleanModuleAbortState(Module));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    public class NeedleCleanModuleAbortState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModuleAbortState(NeedleCleanModule module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.ABORT;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
    }

    public class NeedleCleanModuleRecoveryState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModuleRecoveryState(NeedleCleanModule module) : base(module)
        {
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool doneFlag = false;
                List<ISubModule> modules = Module.Template.GetProcessingModule();
                foreach (var subModule in modules)
                {
                    if (subModule.GetState() == SubModuleStateEnum.IDLE)
                    {
                        subModule.Execute();
                        doneFlag = false;
                    }
                    else if (subModule.Equals(modules.LastOrDefault()) && subModule.GetState() == SubModuleStateEnum.DONE)
                    {
                        doneFlag = true;
                    }
                }
                if (doneFlag)
                {
                    Module.InnerStateTransition(new NeedleCleanModuleDoneState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.RECOVERY;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class NeedleCleanModuleErrorState : NeedleCleanModuleStateBase
    {
        public NeedleCleanModuleErrorState(NeedleCleanModule module, EventCodeInfo eventcode) : base(module)
        {
            if (this.GetModuleState() == ModuleStateEnum.ERROR)
            {
                Module.ReasonOfError.AddEventCodeInfo(eventcode.EventCode, eventcode.Message, eventcode.Caller);
            }
            else
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Current State = {this.GetModuleState()}, Can not add ReasonOfError.");
            }
        }
        public override EventCodeEnum Execute()
        {
            try
            {
                //TO DO : 에러 처리 필요.
                // 랏드런 중이면 Pause 후 메세지 팝업 후 IDLE 상태로 전환.
                // IDLE 상태면 메세지 팝업 후 IDLE 상태로 전환.
                //if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
                //{
                //    Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
                //}
                //else
                //{
                //    Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
                //}

                return EventCodeEnum.NONE;

                //throw new NotImplementedException();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override NeedleCleanStateEnum GetState()
        {
            return NeedleCleanStateEnum.ERROR;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum End() // Abort 시킬때 해야하는 행동
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Module.InnerStateTransition(new NeedleCleanModuleIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


    }
}
