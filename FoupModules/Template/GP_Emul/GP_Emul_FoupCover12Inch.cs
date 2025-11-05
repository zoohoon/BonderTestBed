using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using System;
using System.Threading;

namespace FoupModules.FoupCover
{ 
    public class GP_Emul_FoupCover12Inch : FoupCoverBase, ITemplateModule
    {
        private Emul_FCStateBase _StateObj;
        public Emul_FCStateBase StateObj
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
        public GP_Emul_FoupCover12Inch(IFoupModule module) : base(module)
        {
        }
        public GP_Emul_FoupCover12Inch() : base()
        {
        }
        public void StateTransition(Emul_FCStateBase state)
        {
            try
            {
                StateObj = state;

                EnumState = StateObj.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override FoupCoverStateEnum GetState()
        {
            return StateObj.GetState();
        }
        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                StateObj = new Emul_FCStateOpen(this);

                if ((Module.IOManager.IOMap.Inputs.DI_COVER_UPs.Count > 0) && (Module.IOManager.IOMap.Inputs.DI_COVER_DOWNs.Count > 0))
                { 
                    Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_UPs[Module.FoupIndex]);
                    Inputs.Add(Module.IOManager.IOMap.Inputs.DI_COVER_DOWNs[Module.FoupIndex]);

                    //Module.IOManager.IOMap.Inputs.DI_COVER_UPs[Module.FoupIndex].PortIndex.Value = -1;
                    //Module.IOManager.IOMap.Inputs.DI_COVER_DOWNs[Module.FoupIndex].PortIndex.Value = -1;
                }
                if ((Module.IOManager.IOMap.Outputs.DO_COVER_OPENs.Count > 0) && (Module.IOManager.IOMap.Outputs.DO_COVER_Closes.Count > 0))
                {
                    Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_OPENs[Module.FoupIndex]);
                    Outputs.Add(Module.IOManager.IOMap.Outputs.DO_COVER_Closes[Module.FoupIndex]);

                    //Module.IOManager.IOMap.Outputs.DO_COVER_OPENs[Module.FoupIndex].PortIndex.Value = -1;
                    //Module.IOManager.IOMap.Outputs.DO_COVER_Closes[Module.FoupIndex].PortIndex.Value = -1;
                }
              
                retVal = EventCodeEnum.NONE;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum Open()
        {
            return StateObj.Up();
        }

        public override EventCodeEnum Close()
        {
            return StateObj.Down();
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum EventCodeEnum = EventCodeEnum.NONE;
            StateTransition(new Emul_FCStateOpen(this));

            return EventCodeEnum;
        }
    }
    public abstract class Emul_FCStateBase
    {
        public Emul_FCStateBase(FoupCoverBase owner)
        {
            Owner = (GP_Emul_FoupCover12Inch)owner;
        }
        private GP_Emul_FoupCover12Inch _Owner;

        public GP_Emul_FoupCover12Inch Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }
        protected IFoupIOStates IO => Owner.Module.IOManager;

        protected IFoupModule GetModule()
        {
            return Owner.Module;
        }
        private FoupCoverStateEnum _EnumState;
        public FoupCoverStateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
            }
        }

        public abstract FoupCoverStateEnum GetState();

        public virtual EventCodeEnum Up()
        {
            return CoverOpenMethod();
        }

        public virtual EventCodeEnum Down()
        {
            return CoverCloseMethod();
        }

        public virtual EventCodeEnum Stop()
        {
            return EventCodeEnum.NONE;      // ToDo: Needs support for cylinder stop command.
        }
        protected EventCodeEnum CoverCloseMethod()
        {
            EventCodeEnum retVal= EventCodeEnum.NONE;
            try
            {
                try
                {
                    if (retVal == EventCodeEnum.NONE)
                    {
                        Owner.StateTransition(new Emul_FCStateClose(Owner));
                    }
                    else
                    {
                        Owner.StateTransition(new Emul_FCStateErr(Owner));
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        protected EventCodeEnum CoverOpenMethod()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    Owner.StateTransition(new Emul_FCStateOpen(Owner));
                }else
                {
                    Owner.StateTransition(new Emul_FCStateErr(Owner));
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public class Emul_FCStateOpen : Emul_FCStateBase
    {
        public Emul_FCStateOpen(FoupCoverBase owner) : base(owner)
        {
            int foupindex = owner.Module.FoupIndex;

            FoupInputPortDefinitions inputs = owner.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = owner.Module.IOManager.IOMap.Outputs;

            inputs.DI_COVER_UPs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_UPs[foupindex].ForcedIO.ForecedValue = true;
            inputs.DI_COVER_DOWNs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_DOWNs[foupindex].ForcedIO.ForecedValue = false;

            outputs.DO_COVER_OPENs[foupindex].Value = true;
            outputs.DO_COVER_Closes[foupindex].Value = false;
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.OPEN;
        }

        public override EventCodeEnum Stop()
        {
            throw new NotImplementedException();
        }

    }
    public class Emul_FCStateClose : Emul_FCStateBase
    {
        public Emul_FCStateClose(FoupCoverBase owner) : base(owner)
        {
            int foupindex = owner.Module.FoupIndex;

            FoupInputPortDefinitions inputs = owner.Module.IOManager.IOMap.Inputs;
            FoupOutputPortDefinitions outputs = owner.Module.IOManager.IOMap.Outputs;

            inputs.DI_COVER_UPs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_UPs[foupindex].ForcedIO.ForecedValue = false;
            inputs.DI_COVER_DOWNs[foupindex].ForcedIO.IsForced = true;
            inputs.DI_COVER_DOWNs[foupindex].ForcedIO.ForecedValue = true;

            outputs.DO_COVER_OPENs[foupindex].Value = false;
            outputs.DO_COVER_Closes[foupindex].Value = true;
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.CLOSE;
        }

    }
    public class Emul_FCStateErr : Emul_FCStateBase
    {
        public Emul_FCStateErr(FoupCoverBase owner) : base(owner)
        {

        }


        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.ERROR;
        }

    }
}
