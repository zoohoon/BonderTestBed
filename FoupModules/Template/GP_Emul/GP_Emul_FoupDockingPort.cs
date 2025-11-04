using System;
using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;

namespace FoupModules.DockingPort
{
    public class GP_Emul_FoupDockingPort : FoupDockingPortBase, ITemplateModule
    {
        private GP_Emul_DockingPortStateBase _State;

        public GP_Emul_DockingPortStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public GP_Emul_FoupDockingPort(IFoupModule module) : base(module)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public GP_Emul_FoupDockingPort() : base()
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
        public void StateTransition(GP_Emul_DockingPortStateBase state)
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
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                State = new GP_Emul_DockingPortIn(this);

                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_DP_INs[Module.FoupIndex]);
                Inputs.Add(FoupIOManager.IOMap.Inputs.DI_DP_OUTs[Module.FoupIndex]);

                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_LOADs[Module.FoupIndex]);
                Outputs.Add(FoupIOManager.IOMap.Outputs.DO_CST_UNLOADs[Module.FoupIndex]);

                //FoupIOManager.IOMap.Inputs.DI_DP_INs[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Inputs.DI_DP_OUTs[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Outputs.DO_CST_LOADs[Module.FoupIndex].PortIndex.Value = -1;
                //FoupIOManager.IOMap.Outputs.DO_CST_UNLOADs[Module.FoupIndex].PortIndex.Value = -1;
            }
            catch (Exception err)
            {
                StateTransition(new GP_Emul_DockingPortError(this));
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupDockingPortNormal12Inch - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPort12Inch_Failure, retVal);
            }

            return retVal;
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int ret1 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_DP_INs[Module.FoupIndex], true, 100, 200);
                int ret2 = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_DP_OUTs[Module.FoupIndex], true, 100, 200);

                if (ret1 == 1 && ret2 != 1)
                {
                    StateTransition(new GP_Emul_DockingPortIn(this));
                    retVal = EventCodeEnum.NONE;
                }
                else if (ret1 != 1 && ret2 == 1)
                {
                    StateTransition(new GP_Emul_DockingPortOut(this));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    StateTransition(new GP_Emul_DockingPortError(this));
                    return EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                StateTransition(new GP_Emul_DockingPortError(this));
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
    public abstract class GP_Emul_DockingPortStateBase
    {
        public GP_Emul_DockingPortStateBase(GP_Emul_FoupDockingPort _module)
        {
            Module = _module;
        }
        private GP_Emul_FoupDockingPort _Module;

        public GP_Emul_FoupDockingPort Module
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
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    Module.StateTransition(new GP_Emul_DockingPortIn(Module));
                }
                else
                {
                    Module.StateTransition(new GP_Emul_DockingPortError(Module));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.State = new GP_Emul_DockingPortError(Module);
                LoggerManager.Debug($"{err.Message.ToString()}. DockingPlate8InchUnLock - UnLock() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
        protected EventCodeEnum OutMethod()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    Module.StateTransition(new GP_Emul_DockingPortOut(Module));
                }
                else
                {
                    Module.StateTransition(new GP_Emul_DockingPortError(Module));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Module.State = new GP_Emul_DockingPortError(Module);
                LoggerManager.Debug($"{err.Message.ToString()}. DockingPlate8InchUnLock - UnLock() : Error occured.");
                LoggerManager.Exception(err);
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupDockingPlate12Inch_Failure, retVal);
            }
            return retVal;
        }
    }
    public class GP_Emul_DockingPortIn : GP_Emul_DockingPortStateBase
    {
        public GP_Emul_DockingPortIn(GP_Emul_FoupDockingPort module) : base(module)
        {
            int foupindex = module.Module.FoupIndex;

            FoupInputPortDefinitions inputs = module.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = module.Module.IOManager.IOMap.Outputs;

            inputs.DI_DP_INs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_DP_INs[foupindex].ForcedIO.ForecedValue = true;
            inputs.DI_DP_OUTs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_DP_OUTs[foupindex].ForcedIO.ForecedValue = false;

            outputs.DO_CST_LOADs[foupindex].Value = true;
            outputs.DO_CST_UNLOADs[foupindex].Value = false;

            outputs.DO_CST_IND_UNLOADs[foupindex].Value = false;
            outputs.DO_CST_IND_LOADs[foupindex].Value = true;
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.IN;
        }
    }
    public class GP_Emul_DockingPortOut : GP_Emul_DockingPortStateBase
    {
        public GP_Emul_DockingPortOut(GP_Emul_FoupDockingPort module) : base(module)
        {
            int foupindex = module.Module.FoupIndex;

            FoupInputPortDefinitions inputs = module.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = module.Module.IOManager.IOMap.Outputs;

            inputs.DI_DP_INs[module.Module.FoupIndex].ForcedIO.IsForced = true;
            inputs.DI_DP_INs[module.Module.FoupIndex].ForcedIO.ForecedValue = false;
            inputs.DI_DP_OUTs[module.Module.FoupIndex].ForcedIO.IsForced = true;
            inputs.DI_DP_OUTs[module.Module.FoupIndex].ForcedIO.ForecedValue = true;

            outputs.DO_CST_LOADs[module.Module.FoupIndex].Value = false;
            outputs.DO_CST_UNLOADs[module.Module.FoupIndex].Value = true;

            outputs.DO_CST_IND_UNLOADs[module.Module.FoupIndex].Value = true;
            outputs.DO_CST_IND_LOADs[module.Module.FoupIndex].Value = false;
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.OUT;
        }
    }

    public class GP_Emul_DockingPortError : GP_Emul_DockingPortStateBase
    {
        public GP_Emul_DockingPortError(GP_Emul_FoupDockingPort module) : base(module)
        {
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.ERROR;
        }
    }
}