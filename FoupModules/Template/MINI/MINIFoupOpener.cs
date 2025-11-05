using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using ProberInterfaces;
using FoupModules.FoupOpener;

namespace FoupModules.Template.MINI
{
    public class MINIFoupOpener : FoupOpenerBase, ITemplateModule
    {
        private MINIFoupOpenerStateBase _State;

        public MINIFoupOpenerStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public MINIFoupOpener()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"GPFoupCover12Inch.InitModule(): Init. error. Ret = {ret}");
            //}
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion
        public MINIFoupOpener(FoupModule module) : base(module)
        {
            StateInit();
        }
        public void StateTransition(MINIFoupOpenerStateBase state)
        {
            try
            {
                _State = state;

                this.EnumState = _State.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum EventCodeEnum = EventCodeEnum.NONE;

            return EventCodeEnum;
        }
        public override EventCodeEnum Lock()
        {
            return State.Lock();
        }

        public override EventCodeEnum Unlock()
        {
            return State.Unlock();
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;
            try
            {
                State = new MINIFoupOpenerNAState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                State = new MINIFoupOpenerStateError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. MINIFoupOpener - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                return retVal;
            }
            return retVal;
        }
    }
    public abstract class MINIFoupOpenerStateBase
    {
        public MINIFoupOpenerStateBase(MINIFoupOpener owner)
        {
            this.Owner = owner;
        }

        private MINIFoupOpener _Owner;
        public MINIFoupOpener Owner
        {
            get { return _Owner; }
            set
            {
                if (value != _Owner)
                {
                    _Owner = value;
                }
            }
        }

        protected IFoupModule FoupModule => Owner.Module;

        protected IFoupIOStates IO => Owner.Module.IOManager;

        public abstract FoupCassetteOpenerStateEnum GetState();

        public virtual EventCodeEnum Lock() { return LockFunc(); }

        public virtual EventCodeEnum Unlock() { return UnlockFunc(); }

        public EventCodeEnum LockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = EventCodeEnum.NONE;
                Owner.StateTransition(new MINIFoupOpenerStateLock(Owner));
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum UnlockFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = EventCodeEnum.NONE;
                Owner.StateTransition(new MINIFoupOpenerStateUnlock(Owner));
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class MINIFoupOpenerStateLock : MINIFoupOpenerStateBase
    {
        public MINIFoupOpenerStateLock(MINIFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.LOCK;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = UnlockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupOpenerStateUnlock : MINIFoupOpenerStateBase
    {
        public MINIFoupOpenerStateUnlock(MINIFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.UNLOCK;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = LockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupOpenerStateError : MINIFoupOpenerStateBase
    {
        public MINIFoupOpenerStateError(MINIFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.ERROR;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = LockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UnlockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupOpenerNAState : MINIFoupOpenerStateBase
    {
        public MINIFoupOpenerNAState(MINIFoupOpener owner) : base(owner)
        {
        }

        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.IDLE;
        }

        public override EventCodeEnum Lock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = LockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Unlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UnlockFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupOpenerStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
