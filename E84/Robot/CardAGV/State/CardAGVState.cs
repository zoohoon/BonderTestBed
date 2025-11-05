namespace E84.CardAGV.State
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;

    public abstract class E84CardAGVStateBase : E84StateBase
    {
        protected E84CardAGVStateBase(E84Controller module) : base(module)
        {
        }
    }

    public class E84CardAGVIdleState : E84CardAGVStateBase
    {
        public E84CardAGVIdleState(E84Controller module) : base(module)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.SetCardStateInBuffer();
                if (Module.CheckStartSequence())
                {
                    Module.SetCardBehaviorStateEnum();
                    Module.InnerStateTransition(new E84CardAGVRunningState(Module));
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class E84CardAGVRunningState : E84CardAGVStateBase
    {
        public E84CardAGVRunningState(E84Controller module) : base(module)
        {
            //Module.MetroDialogManager().ShowWaitCancelDialog(module.GetHashCode().ToString(),"Please wait .. Processing OHT ");
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CardBehaviorStateEnum == CardBufferOPEnum.LOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                else if (Module.CardBehaviorStateEnum == CardBufferOPEnum.UNLOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleUnLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    if (Module.E84BehaviorState.GetSubStateEnum() == E84SubStateEnum.DONE)
                    {
                        Module.SetCardStateInBuffer();
                        Module.InnerStateTransition(new E84CardAGVDoneState(Module));                        
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class E84CardAGVDoneState : E84CardAGVStateBase
    {
        public E84CardAGVDoneState(E84Controller module) : base(module)
        {
            //Module.MetroDialogManager().CloseWaitCancelDialaog(module.GetHashCode().ToString());
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.CardBehaviorStateEnum == CardBufferOPEnum.LOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                else if (Module.CardBehaviorStateEnum == CardBufferOPEnum.UNLOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleUnLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                Module.SetCardStateInBuffer();
                retVal = Module.InnerStateTransition(new E84CardAGVIdleState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class E84CardAGVErrorState : E84CardAGVStateBase
    {
        public E84CardAGVErrorState(E84Controller module) : base(module)
        {
            //Module.MetroDialogManager().CloseWaitCancelDialaog(module.GetHashCode().ToString());
            Module.SetCardStateInBuffer();
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
    }

    public class E84CardAGVAutoRecoveryState : E84CardAGVStateBase
    {
        public E84CardAGVAutoRecoveryState(E84Controller module) : base(module)
        {
            Module.SetCardStateInBuffer();
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.SUSPENDED;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

    }
}
