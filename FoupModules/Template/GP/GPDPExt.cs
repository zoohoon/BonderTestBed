
using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.DockingPort40
{
    public class GPDPExt : DockingPort40Base, ITemplateModule
    {

        private GPDPExtStateBase _State;

        public GPDPExtStateBase State
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
        public GPDPExt(IFoupModule module) : base(module)
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
        public GPDPExt() : base()
        {
            
        }
        public void StateTransition(GPDPExtStateBase state)
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
                int ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_OPENs[Module.FoupIndex], true, 
                    100, 200);
                int ret2 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_CLOSEs[Module.FoupIndex], true, 
                    100, 200);

                if (ret1 != 1 && ret2 == 1)
                {
                    State = new GPDPExtStateIn(this);
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 == 1 && ret2 != 1)
                {
                    State = new GPDPExtStateOut(this);
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    State = new GPDPExtStateError(this);
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                LoggerManager.Debug($"GPDPExt.StateInit(): Initial state is {State.GetState()}");
            }
            catch (Exception err)
            {
                StateTransition(new GPDPExtStateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GPDPExt - StateInit() : Error occured.");
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
    public abstract class GPDPExtStateBase
    {
        public GPDPExtStateBase(GPDPExt _module)
        {
            Module = _module;
        }
        private GPDPExt _Module;

        public GPDPExt Module
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
                Module.State = new GPDPExtStateIn(Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.State = new GPDPExtStateError(Module);
                LoggerManager.Debug($"{err.Message.ToString()}. GPDPExtStateBase - ExtInMethod() : Error occured.");
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
                Module.State = new GPDPExtStateOut(Module);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.State = new GPDPExtStateError(Module);
                LoggerManager.Debug($"{err.Message.ToString()}. GPDPExtStateBase - ExtOutMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }
    public class GPDPExtStateIn : GPDPExtStateBase
    {
        public GPDPExtStateIn(GPDPExt module) : base(module)
        {
        }
        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.IN;
        }
       
    }
    public class GPDPExtStateOut : GPDPExtStateBase
    {
        public GPDPExtStateOut(GPDPExt module) : base(module)
        {
        }
        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.OUT;
        }

    }
    public class GPDPExtStateError : GPDPExtStateBase
    {
        public GPDPExtStateError(GPDPExt module) : base(module)
        {
        }
        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.ERROR;
        }
        
    }

}
