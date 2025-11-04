using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.FoupTilt
{
    public class GPFoupTilt : FoupTiltBase, ITemplateModule
    {
        public override TiltStateEnum State => StateObj.GetState();

        private GPFoupTiltStateBase _StateObj;
        public GPFoupTiltStateBase StateObj
        {
            get { return _StateObj; }
            set { _StateObj = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
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
        public GPFoupTilt(IFoupModule module) : base(module)
        {
            try
            {
                //State = new DockingPort12InchIdle(this);
                StateInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public GPFoupTilt() : base()
        {
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;
            try
            {
                StateObj = new GPFoupTiltNAState(this);
                retVal = EventCodeEnum.NONE;
                LoggerManager.Debug($"GPFoupTilt.StateInit(): Not Available for current model. Set to [{State}] state.");
            }
            catch (Exception err)
            {
                StateObj = new GPFoupTiltErrorState(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GPFoupTilt - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                return retVal;
            }
            return retVal;
        }
        public override TiltStateEnum GetState()
        {
            return StateObj.GetState();
        }
        public override EventCodeEnum Up()
        {
            return StateObj.Up();
        }
        public override EventCodeEnum Down()
        {
            return StateObj.Down();
        }
    }
    public abstract class GPFoupTiltStateBase
    {
        public GPFoupTiltStateBase(GPFoupTilt _module)
        {
            Module = _module;
        }
        private GPFoupTilt _Module;

        public GPFoupTilt Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public abstract TiltStateEnum GetState();
        public virtual EventCodeEnum Up() { return TiltUp(); }
        public virtual EventCodeEnum Down(){ return TiltDown(); }

        protected EventCodeEnum TiltUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateObj = new GPFoupTiltUpState(Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        protected EventCodeEnum TiltDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateObj = new GPFoupTiltDownState(Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public class GPFoupTiltUpState : GPFoupTiltStateBase
    {
        public GPFoupTiltUpState(GPFoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.UP;
        }
    }
    public class GPFoupTiltDownState : GPFoupTiltStateBase
    {
        public GPFoupTiltDownState(GPFoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.DOWN;
        }
    }
    public class GPFoupTiltErrorState : GPFoupTiltStateBase
    {
        public GPFoupTiltErrorState(GPFoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.ERROR;
        }
    }
    public class GPFoupTiltNAState : GPFoupTiltStateBase
    {
        public GPFoupTiltNAState(GPFoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.IDLE;
        }
    }
}
