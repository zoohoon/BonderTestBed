
namespace E84
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.State;
    using System;
    using ProberInterfaces.Foup;

    public abstract class E84StateBase : IInnerState
    {
        private E84Controller _Module;
        public E84Controller Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();
        public abstract ModuleStateEnum GetModuleState();
        public E84StateBase(E84Controller module)
        {
            Module = module;
        }

        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ClearState()
        {
            return Module.ClearState();
        }
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class E84FoupIdleState : E84StateBase
    {
        // SetCarrierStateInit() 에서 마지막 에러와 동일한 에러가 발생한 경우는 로그를 출력하지 않도록 추가함.
        EventCodeEnum lastCstErrState = EventCodeEnum.UNDEFINED;
        public E84FoupIdleState(E84Controller module) : base(module)
        {
            try
            {
                Module.SetFoupBehaviorStateEnum(true);
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
                lastCstErrState = Module.SetCarrierStateInit(lastCstErrState);
                if (Module.CheckStartSequence())
                {
                    Module.SetFoupBehaviorStateEnum();

                    if (Module.FoupBehaviorStateEnum != FoupStateEnum.UNDEFIND)
                    {
                        Module.InnerStateTransition(new E84FoupRunningState(Module));
                    }
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

    public class E84FoupRunningState : E84StateBase
    {
        public E84FoupRunningState(E84Controller module) : base(module)
        {
            //Module.MetroDialogManager().ShowWaitCancelDialog(module.GetHashCode().ToString(),"Please wait .. Processing OHT ");
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                E84OPTypeEnum e84OPTypeEnum = Module.E84Module().E84SysParam.E84OPType;
                FoupStateEnum foupStateEnum = Module.FoupBehaviorStateEnum;

                if (foupStateEnum == FoupStateEnum.LOAD)
                {
                    if (e84OPTypeEnum == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                else if (foupStateEnum == FoupStateEnum.UNLOAD)
                {
                    if (e84OPTypeEnum == E84OPTypeEnum.SINGLE)
                    {
                        //retVal = Module.E84BehaviorState?.SingleUnLoad() ?? EventCodeEnum.UNDEFINED;
                        if (Module.GetClampSignal() == false)
                        {
                            retVal = Module.E84BehaviorState?.SingleUnLoad() ?? EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            Module.MetroDialogManager().ShowMessageDialog("Error Message",
                                "The foup is clamped. Unload sequence cannot be processed. Please retry after unclamp and clear status.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                            Module.InnerStateTransition(new E84FoupErrorState(Module));
                        }
                    }
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    if (Module.E84BehaviorState.GetSubStateEnum() == E84SubStateEnum.DONE)
                    {
                        Module.SetCarrierState();
                        Module.InnerStateTransition(new E84FoupDoneState(Module));
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

    public class E84FoupDoneState : E84StateBase
    {
        public E84FoupDoneState(E84Controller module) : base(module)
        {
            //Module.MetroDialogManager().CloseWaitCancelDialaog(module.GetHashCode().ToString());
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.FoupBehaviorStateEnum == FoupStateEnum.LOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                else if (Module.FoupBehaviorStateEnum == FoupStateEnum.UNLOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleUnLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                retVal = Module.InnerStateTransition(new E84FoupIdleState(Module));
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

    public class E84FoupErrorState : E84StateBase
    {
        public E84FoupErrorState(E84Controller module) : base(module)
        {
            //Module.MetroDialogManager().CloseWaitCancelDialaog(module.GetHashCode().ToString());
            //Module.SetCarrierState();
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.ERROR;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(Module.GetSignal(E84SignalTypeEnum.HO_AVBL))
                {
                    retVal = Module.SetSignal(E84SignalTypeEnum.HO_AVBL, false);
                    LoggerManager.Debug($"[E84] PORT{Module.E84ModuleParaemter.FoupIndex} trun off the HO_AVBL. because is in error state ");
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

    public class E84FoupAutoRecoveryState : E84StateBase
    {
        public E84FoupAutoRecoveryState(E84Controller module) : base(module)
        {
            Module.SetCarrierState();
        }
        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.SUSPENDED;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.GetSignal(E84SignalTypeEnum.BUSY))
                {
                    retVal = Module.InnerStateTransition(new E84FoupErrorState(Module));
                    return retVal;
                }

                if (Module.FoupBehaviorStateEnum == FoupStateEnum.LOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleLoad() ?? EventCodeEnum.UNDEFINED;

                        if (retVal == EventCodeEnum.NONE)
                        {
                            if (Module.E84BehaviorState.GetSubStateEnum() == E84SubStateEnum.DONE)
                            {
                                Module.SetCarrierState();
                                Module.InnerStateTransition(new E84FoupDoneState(Module));
                            }
                        }
                    }
                }
                else if (Module.FoupBehaviorStateEnum == FoupStateEnum.UNLOAD)
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleUnLoad() ?? EventCodeEnum.UNDEFINED;
                    }
                }
                else//undefined
                {
                    if (Module.E84Module().E84SysParam.E84OPType == E84OPTypeEnum.SINGLE)
                    {
                        retVal = Module.E84BehaviorState?.SingleIdleBehavior() ?? EventCodeEnum.UNDEFINED;
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
}
