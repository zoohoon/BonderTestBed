using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.FoupOpener
{
    public class GPFoupOpener : FoupOpenerBase, ITemplateModule
    {
       
        public void StateTransition(GPFoupOpenerStateBase state)
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

        private GPFoupOpenerStateBase _State;

        public GPFoupOpenerStateBase State
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
        public GPFoupOpener(IFoupModule module) : base(module)
        {
            StateInit();
        }
        public GPFoupOpener() : base()
        {
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum retVal;
            try
            {
                int ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_LOCKs[Module.FoupIndex],
                    true, 100, 200);
                int ret2 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UNLOCKs[Module.FoupIndex],
                    true, 100, 200);


                if (ret1 != 0 && ret2 == 1)
                {
                    StateTransition(new GPFoupOpenerStateUnLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 == 1 && ret2 != 0)
                {
                    StateTransition(new GPFoupOpenerStateLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    StateTransition(new GPFoupOpenerStateError(this));
                    return EventCodeEnum.FOUP_ERROR;
                }

                LoggerManager.Debug($"GPFoupOpener.CheckState(): Initial state is {State.GetState()}");

            }
            catch (Exception err)
            {
                StateTransition(new GPFoupOpenerStateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GPFoupOpener - CheckState() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupOpener12Inch_Failure, retVal);
            }
            return retVal;
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = CheckState();

                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_LOCKs[Module.FoupIndex]);
                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_UNLOCKs[Module.FoupIndex]);
                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_CST_CoverVacuums[Module.FoupIndex]);
                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_CST_MappingOuts[Module.FoupIndex]);

                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_LOCKs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_UNLOCKs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_CST_VACUUMs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_CST_MAPPINGs[Module.FoupIndex]);

                LoggerManager.Debug($"GPFoupOpener.StateInit()");

            }
            catch (Exception err)
            {
                StateTransition(new GPFoupOpenerStateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GPFoupOpener - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupOpener12Inch_Failure, retVal);
            }
            return retVal;
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum Lock()
        {
            return State.CoverLock();
        }

        public override EventCodeEnum Unlock()
        {
            return State.CoverUnLock();
        }
    }
    public abstract class GPFoupOpenerStateBase
    {
        public GPFoupOpenerStateBase(GPFoupOpener _module)
        {
            OpenerModule = _module;
        }
        private GPFoupOpener _OpenerModule;

        public GPFoupOpener OpenerModule
        {
            get { return _OpenerModule; }
            set { _OpenerModule = value; }
        }

        public abstract FoupCassetteOpenerStateEnum GetState();

        public virtual EventCodeEnum CoverLock()
        {
            return CoverLockMethod();
        }
        public virtual EventCodeEnum CoverUnLock()
        {
            return CoverUnLockMethod();

        }
        protected EventCodeEnum CoverLockMethod()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = OpenerModule.Module.GPCommand.CoverLock(OpenerModule.Module.FoupIndex);
                if (retVal == EventCodeEnum.NONE)
                {
                    OpenerModule.StateTransition( new GPFoupOpenerStateLock(OpenerModule));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    throw new Exception($"GPFoupOpenerStateBase().CoverLockMethod(): Error state.");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                OpenerModule.NotifyManager().Notify(EventCodeEnum.FOUP_CLOSE_ERROR, OpenerModule.Module.FoupNumber);
                OpenerModule.StateTransition ( new GPFoupOpenerStateError(OpenerModule));
                LoggerManager.Debug($"{err.Message.ToString()}. GPFoupOpenerStateBase - CoverLockMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
        protected EventCodeEnum CoverUnLockMethod()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = OpenerModule.Module.GPCommand.CoverUnLock(OpenerModule.Module.FoupIndex);
                if (retVal == EventCodeEnum.NONE)
                {
                    OpenerModule.StateTransition (new GPFoupOpenerStateUnLock(OpenerModule));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    throw new Exception($"GPFoupOpenerStateBase().CoverLockMethod(): Error state.");
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                OpenerModule.NotifyManager().Notify(EventCodeEnum.FOUP_OPEN_ERROR,OpenerModule.Module.FoupNumber);
                OpenerModule.StateTransition(new GPFoupOpenerStateError(OpenerModule));
                LoggerManager.Debug($"{err.Message.ToString()}. GPFoupOpenerStateBase - CoverUnLockMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }
    public class GPFoupOpenerStateLock : GPFoupOpenerStateBase
    {
        public GPFoupOpenerStateLock(GPFoupOpener module) : base(module)
        {
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.LOCK;
        }
    }
    public class GPFoupOpenerStateUnLock : GPFoupOpenerStateBase
    {
        public GPFoupOpenerStateUnLock(GPFoupOpener module) : base(module)
        {
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.UNLOCK;
        }
    }
    public class GPFoupOpenerStateIdle : GPFoupOpenerStateBase
    {
        public GPFoupOpenerStateIdle(GPFoupOpener module) : base(module)
        {
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.IDLE;
        }
    }
    public class GPFoupOpenerStateError : GPFoupOpenerStateBase
    {
        public GPFoupOpenerStateError(GPFoupOpener module) : base(module)
        {
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.ERROR;
        }
    }
}
