using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.DockingPort
{
    public class GPFoupDockingPort : FoupDockingPortBase, ITemplateModule
    {
        private GPDockingPortStateBase _State;

        public GPDockingPortStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public GPFoupDockingPort(IFoupModule module) : base(module)
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

        public GPFoupDockingPort() : base()
        {
            
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
        public void StateTransition(GPDockingPortStateBase state)
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
            EventCodeEnum retVal=EventCodeEnum.UNDEFINED;

            try
            {
                int ret1 = FoupIOManager.MonitorForIO(
                    FoupIOManager.IOMap.Inputs.DI_DP_INs[Module.FoupIndex],
                    true,
                    100, 200);
                int ret2 = FoupIOManager.MonitorForIO(
                    FoupIOManager.IOMap.Inputs.DI_DP_OUTs[Module.FoupIndex],
                    true,
                    100, 200);

                if (ret1 == 1 && ret2 != 1)
                {
                    StateTransition(new DockingPortIn(this));
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 != 1 && ret2 == 1)
                {
                    StateTransition(new DockingPortOut(this));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    StateTransition(new DockingPortError(this));
                    return EventCodeEnum.FOUP_ERROR;
                }
             
                LoggerManager.Debug($"GPFoupDockingPort.CheckState(): Initial state is {State.GetState()}");

            }
            catch (Exception err)
            {
                StateTransition(new DockingPortError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupDockingPortNormal12Inch - CheckState() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPort12Inch_Failure, retVal);
            }

            return retVal;
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = CheckState();
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_DP_INs[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_DP_OUTs[Module.FoupIndex]);

                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_LOADs[Module.FoupIndex]);
                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_UNLOADs[Module.FoupIndex]);
                LoggerManager.Debug($"GPFoupDockingPort.StateInit()");

            }
            catch (Exception err)
            {
                StateTransition(new DockingPortError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupDockingPortNormal12Inch - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPort12Inch_Failure, retVal);
            }

            return retVal;
        }
        public override DockingPortStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum In()
        {
            return State.In();
        }

        public override EventCodeEnum Out()
        {
            return State.Out();
        }

    }
    public abstract class GPDockingPortStateBase
    {
        public GPDockingPortStateBase(GPFoupDockingPort _module)
        {
            Module = _module;
        }
        private GPFoupDockingPort _Module;

        public GPFoupDockingPort Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public abstract DockingPortStateEnum GetState();

        public virtual EventCodeEnum In()
        {
            return InMethod();
        }
        public virtual EventCodeEnum Out()
        {
            return OutMethod();
        }
        protected EventCodeEnum InMethod()
        {
            EventCodeEnum retVal;

            try
            {


                retVal = Module.Module.GPCommand.DockingPortIn(Module.Module.FoupIndex);
                if (retVal == EventCodeEnum.NONE)
                {
                    Module.StateTransition(new DockingPortIn(Module));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    Module.StateTransition(new DockingPortError(Module));
                    throw new Exception($"GPDockingPortStateBase.InMethod(): Return = {retVal}");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.StateTransition(new DockingPortError(Module));
                LoggerManager.Debug($"{err.Message.ToString()}. DockingPlate8InchUnLock - UnLock() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
        protected EventCodeEnum OutMethod()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = Module.Module.GPCommand.DockingPortOut(Module.Module.FoupIndex);
                if (retVal == EventCodeEnum.NONE)
                {
                    Module.StateTransition(new DockingPortOut(Module));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {

                    throw new Exception($"GPDockingPortStateBase.OutMethod(): Return = {retVal}");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.StateTransition(new DockingPortError(Module));
                LoggerManager.Debug($"{err.Message.ToString()}. DockingPlate8InchUnLock - UnLock() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }
    public class DockingPortIn : GPDockingPortStateBase
    {
        public DockingPortIn(GPFoupDockingPort module) : base(module)
        {
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.IN;
        }
    }
    public class DockingPortOut : GPDockingPortStateBase
    {
        public DockingPortOut(GPFoupDockingPort module) : base(module)
        {
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.OUT;
        }
    }

    public class DockingPortError : GPDockingPortStateBase
    {
        public DockingPortError(GPFoupDockingPort module) : base(module)
        {
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.ERROR;
        }
    }
}
