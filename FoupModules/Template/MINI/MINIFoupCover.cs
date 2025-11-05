using FoupModules.FoupCover;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using ProberInterfaces;

namespace FoupModules.Template.MINI
{
    public class MINIFoupCover : FoupCoverBase, ITemplateModule
    {


        private MINIFoupCoverStateBase _StateObj;

        public MINIFoupCoverStateBase StateObj
        {
            get { return _StateObj; }
            set { _StateObj = value; }
        }
        #region // ITemplateModule implementation.
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }

        public MINIFoupCover()
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

        public MINIFoupCover(FoupModule module) : base(module)
        {
            StateInit();
        }
        public void StateTransition(MINIFoupCoverStateBase state)
        {
            try
            {
                _StateObj = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Close()
        {
            return StateObj.Up();
        }

        public override EventCodeEnum Open()
        {
            return StateObj.Down();
        }

        public override FoupCoverStateEnum GetState()
        {
            return StateObj.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;
            try
            {
                StateObj = new MINIFoupCoverNAState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                StateObj = new MINIFoupCoverStateError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. MINIFoupCover - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                return retVal;
            }
            return retVal;
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum EventCodeEnum = EventCodeEnum.NONE;

            return EventCodeEnum;
        }
    }
    public abstract class MINIFoupCoverStateBase
    {
        public MINIFoupCoverStateBase(MINIFoupCover owner)
        {
            this.Owner = owner;
        }

        private MINIFoupCover _Owner;
        public MINIFoupCover Owner
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

        public abstract FoupCoverStateEnum GetState();

        public virtual EventCodeEnum Up() { return CoverUp(); }

        public virtual EventCodeEnum Down() { return CoverDown(); }

        protected EventCodeEnum CoverUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Owner.StateTransition(new MINIFoupCoverStateUp(Owner));
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        protected EventCodeEnum CoverDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Owner.StateTransition(new MINIFoupCoverStateDown(Owner));
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

    public class MINIFoupCoverStateUp : MINIFoupCoverStateBase
    {
        public MINIFoupCoverStateUp(MINIFoupCover owner) : base(owner)
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.CLOSE;
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
                Owner.StateTransition(new MINIFoupCoverStateError(Owner));
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
                Owner.StateTransition(new MINIFoupCoverStateDown(Owner));
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupCoverStateDown : MINIFoupCoverStateBase
    {
        public MINIFoupCoverStateDown(MINIFoupCover owner) : base(owner)
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.OPEN;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
                Owner.StateTransition(new MINIFoupCoverStateUp(Owner));
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupCoverStateError(Owner));
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
                Owner.StateTransition(new MINIFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupCoverStateError : MINIFoupCoverStateBase
    {
        public MINIFoupCoverStateError(MINIFoupCover owner) : base(owner)
        {
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.ERROR;
        }

        public override EventCodeEnum Up()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
                Owner.StateTransition(new MINIFoupCoverStateUp(Owner));
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupCoverStateError(Owner));
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
                Owner.StateTransition(new MINIFoupCoverStateDown(Owner));
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                Owner.StateTransition(new MINIFoupCoverStateError(Owner));
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupCoverNAState : MINIFoupCoverStateBase
    {
        public MINIFoupCoverNAState(MINIFoupCover module) : base(module)
        {
        }

        public override EventCodeEnum Down()
        {
            Owner.StateTransition(new MINIFoupCoverStateDown(Owner));
            return EventCodeEnum.NONE;
        }

        public override FoupCoverStateEnum GetState()
        {
            return FoupCoverStateEnum.IDLE;
        }

        public override EventCodeEnum Up()
        {
            Owner.StateTransition(new MINIFoupCoverStateUp(Owner));
            return EventCodeEnum.NONE;
        }
    }
}
