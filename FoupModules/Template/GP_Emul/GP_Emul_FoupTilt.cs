using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.FoupTilt
{
    public class GP_Emul_FoupTilt : FoupTiltBase, ITemplateModule
    {
        public override TiltStateEnum State => StateObj.GetState();

        private GP_Emul_FoupTiltStateBase _StateObj;
        public GP_Emul_FoupTiltStateBase StateObj
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
            //    LoggerManager.Debug($"GP_Emul_FoupCover12Inch.InitModule(): Init. error. Ret = {ret}");
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
        public GP_Emul_FoupTilt(IFoupModule module) : base(module)
        {
            try
            {
                StateInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public GP_Emul_FoupTilt() : base()
        {
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;

            try
            {
                StateObj = new GP_Emul_FoupTiltNAState(this);
                retVal = EventCodeEnum.NONE;
                LoggerManager.Debug($"GP_Emul_FoupTilt.StateInit(): Not Available for current model. Set to [{State}] state.");
            }
            catch (Exception err)
            {
                StateObj = new GP_Emul_FoupTiltErrorState(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GP_Emul_FoupTilt - StateInit() : Error occured.");
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
    public abstract class GP_Emul_FoupTiltStateBase
    {
        public GP_Emul_FoupTiltStateBase(GP_Emul_FoupTilt _module)
        {
            Module = _module;
        }
        private GP_Emul_FoupTilt _Module;

        public GP_Emul_FoupTilt Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public abstract TiltStateEnum GetState();
        public virtual EventCodeEnum Up() { return TiltUp(); }
        public virtual EventCodeEnum Down() { return TiltDown(); }

        protected EventCodeEnum TiltUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Module.StateObj = new GP_Emul_FoupTiltUpState(Module);
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
                Module.StateObj = new GP_Emul_FoupTiltDownState(Module);
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
    public class GP_Emul_FoupTiltUpState : GP_Emul_FoupTiltStateBase
    {
        public GP_Emul_FoupTiltUpState(GP_Emul_FoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.UP;
        }
    }
    public class GP_Emul_FoupTiltDownState : GP_Emul_FoupTiltStateBase
    {
        public GP_Emul_FoupTiltDownState(GP_Emul_FoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.DOWN;
        }
    }
    public class GP_Emul_FoupTiltErrorState : GP_Emul_FoupTiltStateBase
    {
        public GP_Emul_FoupTiltErrorState(GP_Emul_FoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.ERROR;
        }
    }
    public class GP_Emul_FoupTiltNAState : GP_Emul_FoupTiltStateBase
    {
        public GP_Emul_FoupTiltNAState(GP_Emul_FoupTilt module) : base(module)
        {
        }
        public override TiltStateEnum GetState()
        {
            return TiltStateEnum.IDLE;
        }
    }
}