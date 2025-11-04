using System;

namespace States
{
    using LogModule;
    using ProberInterfaces.State;
    using System.Xml.Serialization;

    [XmlInclude(typeof(EnableState))]
    [Serializable]
    public abstract class EnableStateBase : IEnableState
    {

        private EnableStateBase _State;

        public EnableStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public EnableStateBase()
        {
        }

        public EnableStateBase(EnableStateBase state)
        {
            try
            {
                State = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public abstract EnumEnableState GetEnableState();

        public void ChangeEnableState(EnableStateBase state)
        {
            try
            {
                State = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public virtual IEnableState SetIdle(IEnableState state)
        {
            try
            {
                throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetEnableState()));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public virtual IEnableState SetEnable(IEnableState state)
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetEnableState()));
        }

        public virtual IEnableState SetDisable(IEnableState state)
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetEnableState()));
        }

        public virtual IEnableState SetMust(IEnableState state)
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetEnableState()));
        }

        public virtual IEnableState SetMustNot(IEnableState state)
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetEnableState()));
        }

    }

    public class EnableIdleState : EnableStateBase
    {
        public EnableIdleState()
        {
        }
        public EnableIdleState(EnableStateBase state) : base(state)
        {
            try
            {
                if (state == null)
                    State = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EnumEnableState GetEnableState()
        {
            return EnumEnableState.IDLE;
        }

        public override IEnableState SetIdle(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableIdleState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetEnable(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetDisable(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new DisableState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetMust(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new MustState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetMustNot(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new MustNotState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }
    }


    [Serializable]
    public class EnableState : EnableStateBase
    {
        public EnableState()
        {
        }
        public EnableState(EnableStateBase state) : base(state)
        {
            try
            {
                if (state == null)
                    State = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EnumEnableState GetEnableState()
        {
            return EnumEnableState.ENABLE;
        }

        public override IEnableState SetIdle(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableIdleState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }
        public override IEnableState SetEnable(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }
        public override IEnableState SetDisable(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new DisableState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetMust(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new MustState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetMustNot(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new MustNotState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

    }

    public class DisableState : EnableStateBase
    {
        public DisableState()
        {
        }
        public DisableState(EnableStateBase state) : base(state)
        {
            try
            {
                if (state == null)
                    State = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EnumEnableState GetEnableState()
        {
            return EnumEnableState.DISABLE;
        }

        public override IEnableState SetIdle(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableIdleState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetEnable(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }


        public override IEnableState SetMust(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new MustState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

        public override IEnableState SetMustNot(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new MustNotState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }
    }

    public class MustState : EnableStateBase
    {
        public MustState()
        {

        }
        public MustState(EnableStateBase state) : base(state)
        {
            try
            {
                if (state == null)
                    State = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EnumEnableState GetEnableState()
        {
            return EnumEnableState.MUST;
        }
        public override IEnableState SetIdle(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableIdleState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

    }

    public class MustNotState : EnableStateBase
    {
        public MustNotState()
        {

        }
        public MustNotState(EnableStateBase state) : base(state)
        {
            try
            {
                if (state == null)
                    State = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EnumEnableState GetEnableState()
        {
            return EnumEnableState.MUSTNOT;
        }
        public override IEnableState SetIdle(IEnableState state)
        {
            State = (EnableStateBase)state;
            try
            {
                State.ChangeEnableState(new EnableIdleState(State));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return State;
        }

    }
}
