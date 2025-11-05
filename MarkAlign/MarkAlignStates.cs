using System;
using System.Collections.Generic;
using System.Linq;
using ProberInterfaces;
using ProberInterfaces.MarkAlign;
using ProberErrorCode;
using ProberInterfaces.Command;
using LogModule;
using ProberInterfaces.State;

namespace MarkAlign
{
    public abstract class MarkAlignState : IInnerState
    {
        //public abstract void DoMarkAlign();
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract MarkAlignStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();

        //public abstract EventCodeEnum DoMarkAlign();


        //public abstract EventCodeEnum Focusing();


        //public abstract EventCodeEnum MoveToMark();
        public abstract bool CanExecute(IProbeCommandToken token);

        public virtual EventCodeEnum End()
        {
            throw new NotImplementedException();
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public abstract class MarkAlignStateBase : MarkAlignState
    {

        private MarkAligner _Module;

        public MarkAligner Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public MarkAlignStateBase(MarkAligner module)
        {
            Module = module;
        }

        public override EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Module.InnerStateTransition(new MarkAlignIDLEState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public abstract void DoMarkAlign();
    }
    public class MarkAlignIDLEState : MarkAlignStateBase
    {
        public MarkAlignIDLEState(MarkAligner module) : base(module)
        {
        }



        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
            if (Module.Template != null)
            {
                List<ISubModule> modules = Module.Template.GetProcessingModule();
                if (modules != null)
                {
                    if (Module.Template.SchedulingModule.IsExecute())
                    {
                        foreach (var subModule in modules)
                        {
                            if (subModule.IsExecute())
                            {
                                Module.InnerStateTransition(new MarkAlignRunningState(Module));

                                break;
                            }
                        }
                    }
                }
            }


            RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }


        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.IDLE;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            Module.InnerStateTransition(new MarkAlignPauseState(Module));

            return EventCodeEnum.NONE;
        }
    }

    public class MarkAlignRunningState : MarkAlignStateBase
    {


        public MarkAlignRunningState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {


                if (Module.ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    Module.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                    Module.InnerStateTransition(new MarkAlignDoneState(Module));
                    LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                    return EventCodeEnum.NONE;
                }
                //RetVal = DoMarkAilgn();

                //
                #region Remove Task
                Module.LotOPModule().VisionScreenToLotScreen();

                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                bool doneFlag = false;
                List<ISubModule> modules = Module.Template.GetProcessingModule();
                foreach (var subModule in modules)
                {
                    retVal = subModule.ClearData();
                    retVal = subModule.Execute();
                    if (subModule.GetState() == SubModuleStateEnum.ERROR)
                    {
                        if (Module.ForcedDone == EnumModuleForcedState.ForcedRunningAndDone)
                        {
                            Module.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                            Module.InnerStateTransition(new MarkAlignDoneState(Module));
                            LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={Module.InnerState.GetModuleState()}");
                            RetVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            Module.InnerStateTransition(new MarkAlignErrorState(Module));
                        }

                        break;
                    }


                    if (subModule.Equals(modules.LastOrDefault()) && subModule.GetState() == SubModuleStateEnum.DONE)
                    {
                        doneFlag = true;
                        break;
                    }

                }
                if (doneFlag)
                {
                    Module.StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);
                    Module.InnerStateTransition(new MarkAlignDoneState(Module));
                }

                // return retVal;
                Module.VisionManager().AllStageCameraStopGrab();
                Module.LotOPModule().MapScreenToLotScreen();

                #endregion


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.RUNNING;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }

    public class MarkAlignFocusingFailState : MarkAlignStateBase
    {
        public MarkAlignFocusingFailState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.FocusingFail;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class MarkAlignPatternFailState : MarkAlignStateBase
    {
        public MarkAlignPatternFailState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            throw new NotImplementedException();
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.PatternFail;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class MarkAlignDoneState : MarkAlignStateBase
    {
        public MarkAlignDoneState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {

            Module.InnerStateTransition(new MarkAlignIDLEState(Module));

            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.DONE;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.DONE;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            Module.InnerStateTransition(new MarkAlignPauseState(Module));

            return EventCodeEnum.NONE;
        }
    }

    public class MarkAlignAbortState : MarkAlignStateBase
    {
        public MarkAlignAbortState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            retVal = Module.InnerStateTransition(new MarkAlignIDLEState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return EventCodeEnum.NONE;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ABORT;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.ABORT;
        }

        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class MarkAlignShiftedState : MarkAlignStateBase
    {
        public MarkAlignShiftedState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NODATA;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.ERROR;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }
    }



    public class MarkAlignPauseState : MarkAlignStateBase
    {
        public MarkAlignPauseState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NODATA;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.PAUSED;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            bool retVal = false;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            Module.InnerStateTransition(Module.PreInnerState);

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
            retVal = Module.InnerStateTransition(new MarkAlignAbortState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }

    public class MarkAlignErrorState : MarkAlignStateBase
    {
        public MarkAlignErrorState(MarkAligner module) : base(module)
        {
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.NODATA;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override MarkAlignStateEnum GetState()
        {
            return MarkAlignStateEnum.ERROR;
        }


        public override bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Pause()
        {
            return EventCodeEnum.NONE;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            Module.InnerStateTransition(new MarkAlignIDLEState(Module));
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
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            Module.InnerStateTransition(new MarkAlignIDLEState(Module));
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
