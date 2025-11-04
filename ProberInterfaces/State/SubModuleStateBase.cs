using LogModule;
using System;

namespace ProberInterfaces
{
    using System.ComponentModel;
    using ProberErrorCode;

    public enum SubModuleStateEnum
    {
        UNDEFINED = 0,
        IDLE,
        DONE,
        ERROR,
        RECOVERY,
        SKIP,
        SUSPEND
    }

    [Serializable]
    public abstract class SubModuleStateBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private IProcessingModule _Module;

        public IProcessingModule Module
        {
            get { return _Module; }
            set { _Module = value; }
        }


        public SubModuleStateBase()
        {

        }
        public SubModuleStateBase(IProcessingModule module)
        {
            Module = module;
        }
        public abstract SubModuleStateEnum GetState();
        public abstract EventCodeEnum Execute();

        public virtual EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            retVal = Module.DoClearData();

            if (retVal == EventCodeEnum.NONE)
                SubModuleStateTransition(SubModuleStateEnum.IDLE);
            else
                SubModuleStateTransition(SubModuleStateEnum.ERROR);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public virtual EventCodeEnum Recovery()
        {
            Module.DoRecovery();
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.DoExitRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public void SubModuleStateTransition(SubModuleStateEnum state)
        {
            try
            {
            switch (state)
            {
                case SubModuleStateEnum.IDLE:
                    Module.SubModuleState = new SubModuleIdleState(Module);
                    break;
                case SubModuleStateEnum.DONE:
                    Module.SubModuleState = new SubModuleDoneState(Module);
                    break;
                case SubModuleStateEnum.RECOVERY:
                    Module.SubModuleState = new SubModuleRecoveryState(Module);
                    break;
                case SubModuleStateEnum.SKIP:
                    Module.SubModuleState = new SubModuleSkipState(Module);
                    break;
                case SubModuleStateEnum.ERROR:
                default:
                    Module.SubModuleState = new SubModuleErrorState(Module);
                    break;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }

    [Serializable]
    public class SubModuleIdleState : SubModuleStateBase
    {

        public SubModuleIdleState(IProcessingModule module) : base(module)
        {
        }

        public SubModuleIdleState()
        {
        }

        public override SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.IDLE;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.DoExecute();

                //if (retVal == EventCodeEnum.NONE) // 주석해야됨.
                //    SubModuleStateTransition(SubModuleStateEnum.DONE);
                //else if (retVal == EventCodeEnum.SUB_RECOVERY)
                //    SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                //else if (retVal == EventCodeEnum.SUB_SKIP)
                //    SubModuleStateTransition(SubModuleStateEnum.SKIP);
                //else
                //    SubModuleStateTransition(SubModuleStateEnum.ERROR);

            }
            catch (Exception err)
            {
                //Module State : Error 로 변경.
                SubModuleStateTransition(SubModuleStateEnum.ERROR);
                throw err;
            }
            return retVal;
        }


    }

    [Serializable]
    public class SubModuleDoneState : SubModuleStateBase
    {
        public SubModuleDoneState(IProcessingModule module) : base(module)
        {
        }

        public override SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.DONE;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.DoExecute();

                //if (retVal != EventCodeEnum.NONE)
                //    SubModuleStateTransition(SubModuleStateEnum.DONE);
                //else if (retVal == EventCodeEnum.SUB_RECOVERY)
                //    SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                //else if (retVal == EventCodeEnum.SUB_SKIP)
                //    SubModuleStateTransition(SubModuleStateEnum.SKIP);
                //else if (retVal == EventCodeEnum.SUB_SUSPEND)
                //    SubModuleStateTransition(SubModuleStateEnum.SUSPEND);
                //else
                //    SubModuleStateTransition(SubModuleStateEnum.ERROR);

            }
            catch (Exception err)
            {
                //Module State : Error 로 변경.
                SubModuleStateTransition(SubModuleStateEnum.ERROR);
                throw err;
            }
            return retVal;
        }
    }

    [Serializable]
    public class SubModuleErrorState : SubModuleStateBase
    {
        public SubModuleErrorState(IProcessingModule module) : base(module)
        {
        }

        public override SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.ERROR;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.DoExecute();

                //if (retVal == EventCodeEnum.NONE)
                //    SubModuleStateTransition(SubModuleStateEnum.DONE);
                //else if (retVal == EventCodeEnum.SUB_RECOVERY)
                //    SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                //else if (retVal == EventCodeEnum.SUB_SKIP)
                //    SubModuleStateTransition(SubModuleStateEnum.SKIP);
                //else
                //    SubModuleStateTransition(SubModuleStateEnum.ERROR);

            }
            catch (Exception err)
            {
                //Module State : Error 로 변경.
                SubModuleStateTransition(SubModuleStateEnum.ERROR);
                throw err;
            }
            return retVal;
        }
    }

    [Serializable]
    public class SubModuleRecoveryState : SubModuleStateBase
    {
        public SubModuleRecoveryState(IProcessingModule module) : base(module)
        {
        }

        public override SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.RECOVERY;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.DoExecute();

                //if (retVal == EventCodeEnum.NONE)
                //    SubModuleStateTransition(SubModuleStateEnum.DONE);
                //else if (retVal == EventCodeEnum.SUB_RECOVERY)
                //    SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                //else if (retVal == EventCodeEnum.SUB_SKIP)
                //    SubModuleStateTransition(SubModuleStateEnum.SKIP);
                //else
                //    SubModuleStateTransition(SubModuleStateEnum.ERROR);

            }
            catch (Exception err)
            {
                //Module State : Error 로 변경.
                SubModuleStateTransition(SubModuleStateEnum.ERROR);
                throw err;
            }
            return retVal;
        }
        public override EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            retVal = Module.DoRecovery();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

    }

    [Serializable]
    public class SubModuleSuspendState : SubModuleStateBase
    {
        public SubModuleSuspendState(IProcessingModule module) : base(module)
        {
        }

        public override SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.SUSPEND;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.DoExecute();

                //if (retVal == EventCodeEnum.NONE)
                //    SubModuleStateTransition(SubModuleStateEnum.DONE);
                //else if (retVal == EventCodeEnum.SUB_RECOVERY)
                //    SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                //else if (retVal == EventCodeEnum.SUB_SKIP)
                //    SubModuleStateTransition(SubModuleStateEnum.SKIP);
                //else
                //    SubModuleStateTransition(SubModuleStateEnum.ERROR);

            }
            catch (Exception err)
            {
                //Module State : Error 로 변경.
                SubModuleStateTransition(SubModuleStateEnum.ERROR);
                throw err;
            }
            return retVal;
        }

    }

    [Serializable]
    public class SubModuleSkipState : SubModuleStateBase
    {
        public SubModuleSkipState()
        {
        }

        public SubModuleSkipState(IProcessingModule module) : base(module)
        {
        }
        public override SubModuleStateEnum GetState()
        {
            return SubModuleStateEnum.SKIP;
        }
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.DoExecute();

                //if (retVal == EventCodeEnum.NONE)
                //    SubModuleStateTransition(SubModuleStateEnum.DONE);
                //else if (retVal == EventCodeEnum.SUB_RECOVERY)
                //    SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                //else
                //    SubModuleStateTransition(SubModuleStateEnum.ERROR);
            }
            catch (Exception err)
            {
                //Module State : Error 로 변경.
                SubModuleStateTransition(SubModuleStateEnum.ERROR);
                throw err;
            }
            return retVal;
        }
    }
}
