using FoupModules.FoupTilt;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using ProberInterfaces;

namespace FoupModules.Template.TOP
{
    public class TOPFoupTilt : FoupTiltBase, ITemplateModule
    {
        public override TiltStateEnum State => StateObj.GetState();
        
        private TOPFoupTiltStateBase _StateObj;

        public TOPFoupTiltStateBase StateObj
        {
            get { return _StateObj; }
            set { _StateObj = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public TOPFoupTilt() : base()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"GPFoupTilt12Inch.InitModule(): Init. error. Ret = {ret}");
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

        public TOPFoupTilt(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(TOPFoupTiltStateBase state)
        {
            try
            {
                _StateObj = state;

                //this.EnumState = _StateObj.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Up()
        {
            return StateObj.Up();
        }

        public override EventCodeEnum Down()
        {
            return StateObj.Down();
        }

        public override TiltStateEnum GetState()
        {
            return StateObj.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;
            try
            {
                StateObj = new TOPFoupTiltNAState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                StateObj = new TOPFoupTiltStateError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. TOPFoupTilt - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                return retVal;
            }
            return retVal;
        }
    }
    public abstract class TOPFoupTiltStateBase
    {
        public TOPFoupTiltStateBase(TOPFoupTilt owner)
        {
            this.Owner = owner;
        }

        private TOPFoupTilt _Owner;
        public TOPFoupTilt Owner
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

        public abstract TiltStateEnum GetState();

        public abstract EventCodeEnum Up();

        public abstract EventCodeEnum Down();

        public EventCodeEnum UpFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Owner.StateTransition(new TOPFoupTiltStateUp(Owner));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum DownFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            //int retIOvalue = -1;

            try
            {
                Owner.StateTransition(new TOPFoupTiltStateDown(Owner));
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class TOPFoupTiltStateUp : TOPFoupTiltStateBase
    {
        public TOPFoupTiltStateUp(TOPFoupTilt owner) : base(owner)
        {
        }

        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.UP;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = DownFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TOPFoupTiltStateDown : TOPFoupTiltStateBase
    {
        public TOPFoupTiltStateDown(TOPFoupTilt owner) : base(owner)
        {
        }

        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.DOWN;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UpFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TOPFoupTiltStateError : TOPFoupTiltStateBase
    {
        public TOPFoupTiltStateError(TOPFoupTilt owner) : base(owner)
        {
        }

        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.ERROR;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UpFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = DownFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    public class TOPFoupTiltNAState : TOPFoupTiltStateBase
    {
        public TOPFoupTiltNAState(TOPFoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.IDLE;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = UpFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new TOPFoupTiltStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Down()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = DownFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;

                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
