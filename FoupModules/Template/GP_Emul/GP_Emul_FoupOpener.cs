using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.FoupOpener
{
    public class GP_Emul_FoupOpener : FoupOpenerBase, ITemplateModule
    {

        public void StateTransition(GP_Emul_FoupOpenerStateBase state)
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

        private GP_Emul_FoupOpenerStateBase _State;

        public GP_Emul_FoupOpenerStateBase State
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
        public GP_Emul_FoupOpener(IFoupModule module) : base(module)
        {
        }
        public GP_Emul_FoupOpener() : base()
        {
        }

        public override EventCodeEnum CheckState()
        {
            EventCodeEnum retVal;

            try
            {
                int ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_LOCKs[Module.FoupIndex], true, 100, 200);
                int ret2 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UNLOCKs[Module.FoupIndex], true, 100, 200);

                if (ret1 != 0 && ret2 == 1)
                {
                    StateTransition(new GP_Emul_FoupOpenerStateUnLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 == 1 && ret2 != 0)
                {
                    StateTransition(new GP_Emul_FoupOpenerStateLock(this));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    StateTransition(new GP_Emul_FoupOpenerStateError(this));
                    return EventCodeEnum.FOUP_ERROR;
                }

                LoggerManager.Debug($"GP_Emul_FoupOpener.CheckState(): Initial state is {State.GetState()}");

            }
            catch (Exception err)
            {
                StateTransition(new GP_Emul_FoupOpenerStateError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. GP_Emul_FoupOpener - CheckState() : Error occured.");
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
                State = new GP_Emul_FoupOpenerStateUnLock(this);

                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_LOCKs[Module.FoupIndex]);
                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_UNLOCKs[Module.FoupIndex]);
                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_CST_CoverVacuums[Module.FoupIndex]);
                Inputs.Add(Module.IOManager.IOMap.Inputs.DI_CST_MappingOuts[Module.FoupIndex]);

                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_LOCKs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_UNLOCKs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_CST_VACUUMs[Module.FoupIndex]);
                Outputs.Add(Module.IOManager.IOMap.Outputs.DO_CST_MAPPINGs[Module.FoupIndex]);

            //    Module.IOManager.IOMap.Inputs.DI_COVER_LOCKs[Module.FoupIndex].PortIndex.Value = -1;
            //    Module.IOManager.IOMap.Inputs.DI_COVER_UNLOCKs[Module.FoupIndex].PortIndex.Value = -1;
            //    Module.IOManager.IOMap.Inputs.DI_CST_CoverVacuums[Module.FoupIndex].PortIndex.Value = -1;
            //    Module.IOManager.IOMap.Inputs.DI_CST_MappingOuts[Module.FoupIndex].PortIndex.Value = -1;

            //    Module.IOManager.IOMap.Outputs.DO_COVER_LOCKs[Module.FoupIndex].PortIndex.Value = -1;
            //    Module.IOManager.IOMap.Outputs.DO_COVER_UNLOCKs[Module.FoupIndex].PortIndex.Value = -1;
            //    Module.IOManager.IOMap.Outputs.DO_CST_VACUUMs[Module.FoupIndex].PortIndex.Value = -1;
            //    Module.IOManager.IOMap.Outputs.DO_CST_MAPPINGs[Module.FoupIndex].PortIndex.Value = -1;
            }
            catch (Exception err)
            {
                StateTransition(new GP_Emul_FoupOpenerStateError(this));
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
    public abstract class GP_Emul_FoupOpenerStateBase
    {
        public GP_Emul_FoupOpenerStateBase(GP_Emul_FoupOpener _module)
        {
            OpenerModule = _module;
        }
        private GP_Emul_FoupOpener _OpenerModule;

        public GP_Emul_FoupOpener OpenerModule
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
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    OpenerModule.StateTransition(new GP_Emul_FoupOpenerStateLock(OpenerModule));
                }
                else
                {
                    OpenerModule.StateTransition(new GP_Emul_FoupOpenerStateError(OpenerModule));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                OpenerModule.NotifyManager().Notify(EventCodeEnum.FOUP_CLOSE_ERROR, OpenerModule.Module.FoupNumber);
                OpenerModule.State = new GP_Emul_FoupOpenerStateError(OpenerModule);
                LoggerManager.Debug($"{err.Message.ToString()}. GPFoupOpenerStateBase - CoverLockMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
        protected EventCodeEnum CoverUnLockMethod()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    OpenerModule.StateTransition(new GP_Emul_FoupOpenerStateUnLock(OpenerModule));
                }
                else
                {
                    OpenerModule.StateTransition(new GP_Emul_FoupOpenerStateError(OpenerModule));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                OpenerModule.NotifyManager().Notify(EventCodeEnum.FOUP_OPEN_ERROR, OpenerModule.Module.FoupNumber);
                OpenerModule.State = new GP_Emul_FoupOpenerStateError(OpenerModule);
                LoggerManager.Debug($"{err.Message.ToString()}. GPFoupOpenerStateBase - CoverUnLockMethod() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }
    public class GP_Emul_FoupOpenerStateLock : GP_Emul_FoupOpenerStateBase
    {
        public GP_Emul_FoupOpenerStateLock(GP_Emul_FoupOpener module) : base(module)
        {
            int foupindex = module.Module.FoupIndex;

            FoupInputPortDefinitions inputs = module.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = module.Module.IOManager.IOMap.Outputs;

            inputs.DI_COVER_LOCKs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_LOCKs[foupindex].ForcedIO.ForecedValue = true;
            inputs.DI_COVER_UNLOCKs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_UNLOCKs[foupindex].ForcedIO.ForecedValue = false;
            inputs.DI_CST_CoverVacuums[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_CoverVacuums[foupindex].ForcedIO.ForecedValue = false;
            inputs.DI_CST_MappingOuts[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_MappingOuts[foupindex].ForcedIO.ForecedValue = false;

            outputs.DO_COVER_LOCKs[foupindex].Value = true;
            outputs.DO_COVER_UNLOCKs[foupindex].Value = false;
            outputs.DO_CST_VACUUMs[foupindex].Value = false;
            outputs.DO_CST_MAPPINGs[foupindex].Value = false;
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.LOCK;
        }
    }
    public class GP_Emul_FoupOpenerStateUnLock : GP_Emul_FoupOpenerStateBase
    {
        public GP_Emul_FoupOpenerStateUnLock(GP_Emul_FoupOpener module) : base(module)
        {
            int foupindex = module.Module.FoupIndex;

            FoupInputPortDefinitions inputs = module.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = module.Module.IOManager.IOMap.Outputs;

            inputs.DI_COVER_LOCKs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_LOCKs[foupindex].ForcedIO.ForecedValue = false;
            inputs.DI_COVER_UNLOCKs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_UNLOCKs[foupindex].ForcedIO.ForecedValue = true;
            inputs.DI_CST_CoverVacuums[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_CoverVacuums[foupindex].ForcedIO.ForecedValue = true;
            inputs.DI_CST_MappingOuts[foupindex].ForcedIO.IsForced = true;
            inputs.DI_CST_MappingOuts[foupindex].ForcedIO.ForecedValue = false;

            outputs.DO_COVER_LOCKs[foupindex].Value = false;
            outputs.DO_COVER_UNLOCKs[foupindex].Value = true;
            outputs.DO_CST_VACUUMs[foupindex].Value = true;
            outputs.DO_CST_MAPPINGs[foupindex].Value = false;
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.UNLOCK;
        }
    }
    public class GP_Emul_FoupOpenerStateIdle : GP_Emul_FoupOpenerStateBase
    {
        public GP_Emul_FoupOpenerStateIdle(GP_Emul_FoupOpener module) : base(module)
        {
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.IDLE;
        }
    }
    public class GP_Emul_FoupOpenerStateError : GP_Emul_FoupOpenerStateBase
    {
        public GP_Emul_FoupOpenerStateError(GP_Emul_FoupOpener module) : base(module)
        {
        }
        public override FoupCassetteOpenerStateEnum GetState()
        {
            return FoupCassetteOpenerStateEnum.ERROR;
        }
    }
}
