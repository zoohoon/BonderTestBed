
using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.DockingPort40
{
    public class GP_Emul_DPExt : DockingPort40Base, ITemplateModule
    {

        private GP_Emul_DPExtStateBase _State;

        public GP_Emul_DPExtStateBase State
        {
            get { return _State; }
            set { _State = value; }
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
        public GP_Emul_DPExt(IFoupModule module) : base(module)
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
        public GP_Emul_DPExt() : base()
        {

        }
        public void StateTransition(GP_Emul_DPExtStateBase state)
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
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;

            try
            {
                int ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_OPENs[Module.FoupIndex], true, 100, 200);
                int ret2 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_CLOSEs[Module.FoupIndex], true, 100, 200);

                if (ret1 != 1 && ret2 == 1)
                {
                    State = new GP_Emul_DPExtStateIn(this);
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 == 1 && ret2 != 1)
                {
                    State = new GP_Emul_DPExtStateOut(this);
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    State = new GP_Emul_DPExtStateError(this);
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                LoggerManager.Debug($"GP_Emul_DPExt.StateInit(): Initial state is {State.GetState()}");
            }
            catch (Exception err)
            {
                StateTransition(new GP_Emul_DPExtStateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GP_Emul_DPExt - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPort40mm_12Inch_Failure, retVal);
            }
            return retVal;
        }
        public override DockingPort40StateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum In()
        {
            return State.In40();
        }

        public override EventCodeEnum Out()
        {
            return State.Out40();
        }
    }
    public abstract class GP_Emul_DPExtStateBase
    {
        public GP_Emul_DPExtStateBase(GP_Emul_DPExt _module)
        {
            Module = _module;
        }
        private GP_Emul_DPExt _Module;

        public GP_Emul_DPExt Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public abstract DockingPort40StateEnum GetState();
        public virtual EventCodeEnum In40()
        {
            return ExtInMethod();
        }
        public virtual EventCodeEnum Out40()
        {
            return ExtOutMethod();
        }
        protected EventCodeEnum ExtInMethod()
        {
            EventCodeEnum retVal;

            try
            {
                Module.State = new GP_Emul_DPExtStateIn(Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.State = new GP_Emul_DPExtStateError(Module);
                LoggerManager.Debug($"{err.Message.ToString()}. GP_Emul_DPExtStateBase - ExtInMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
        protected EventCodeEnum ExtOutMethod()
        {
            EventCodeEnum retVal;

            try
            {
                Module.State = new GP_Emul_DPExtStateOut(Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.State = new GP_Emul_DPExtStateError(Module);
                LoggerManager.Debug($"{err.Message.ToString()}. GP_Emul_DPExtStateBase - ExtOutMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }
    public class GP_Emul_DPExtStateIn : GP_Emul_DPExtStateBase
    {
        public GP_Emul_DPExtStateIn(GP_Emul_DPExt module) : base(module)
        {
        }
        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.IN;
        }

    }
    public class GP_Emul_DPExtStateOut : GP_Emul_DPExtStateBase
    {
        public GP_Emul_DPExtStateOut(GP_Emul_DPExt module) : base(module)
        {
        }
        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.OUT;
        }

    }
    public class GP_Emul_DPExtStateError : GP_Emul_DPExtStateBase
    {
        public GP_Emul_DPExtStateError(GP_Emul_DPExt module) : base(module)
        {
        }
        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.ERROR;
        }

    }

}
