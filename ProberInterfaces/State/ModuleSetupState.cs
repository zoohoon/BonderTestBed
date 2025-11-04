using System;

namespace ProberInterfaces.State
{
    public enum EnumMoudleSetupState
    {
        UNDEFINED = 0,
        NOTCOMPLETED,
        COMPLETE,
        VERIFY,
        NONE
    }

    public interface IMoudleSetupState
    {
        EnumMoudleSetupState GetState();
        void SetComplete();
        void SetNotCompleted();
        void SetVerify();
        void SetNone();
    }

    public abstract class SetupStateBase : IMoudleSetupState
    {
        public ICategoryNodeItem _Module;

        public SetupStateBase()
        {

        }

        public SetupStateBase(ICategoryNodeItem module)
        {
            _Module = module;
        }

        public abstract EnumMoudleSetupState GetState();

        public virtual void SetComplete()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetNotCompleted()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetVerify()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetNone()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }
    }

    public class CompleteState : SetupStateBase
    {
        public CompleteState()
        {

        }

        public CompleteState(ICategoryNodeItem module) : base(module)
        {
        }

        public override EnumMoudleSetupState GetState()
        {
            return EnumMoudleSetupState.COMPLETE;
        }

        public override void SetComplete()
        {
            _Module.ChangeSetupState(new CompleteState(_Module));
        }

        public override void SetNotCompleted()
        {
            _Module.ChangeSetupState(new NotCompletedState(_Module));
        }

        public override void SetVerify()
        {
            _Module.ChangeSetupState(new VerifyState(_Module));
        }

        public override void SetNone()
        {
            _Module.ChangeSetupState(new NoneState(_Module));
        }
    }

    public class NotCompletedState : SetupStateBase
    {
        public NotCompletedState()
        {

        }

        public NotCompletedState(ICategoryNodeItem module) : base(module)
        {

        }

        public override EnumMoudleSetupState GetState()
        {
            return EnumMoudleSetupState.NOTCOMPLETED;
        }

        public override void SetComplete()
        {
            _Module.ChangeSetupState(new CompleteState(_Module));
        }

        public override void SetNotCompleted()
        {
            _Module.ChangeSetupState(new NotCompletedState(_Module));
        }

        public override void SetVerify()
        {
            _Module.ChangeSetupState(new VerifyState(_Module));
        }

        public override void SetNone()
        {
            _Module.ChangeSetupState(new NoneState(_Module));
        }
    }

    public class VerifyState : SetupStateBase
    {
        public VerifyState()
        {

        }

        public VerifyState(ICategoryNodeItem module) : base(module)
        {

        }

        public override EnumMoudleSetupState GetState()
        {
            return EnumMoudleSetupState.VERIFY;
        }

        public override void SetComplete()
        {
            _Module.ChangeSetupState(new CompleteState(_Module));
        }

        public override void SetNotCompleted()
        {
            _Module.ChangeSetupState(new NotCompletedState(_Module));
        }

        public override void SetVerify()
        {
            _Module.ChangeSetupState(new VerifyState(_Module));
        }

        public override void SetNone()
        {
            _Module.ChangeSetupState(new NoneState(_Module));
        }
    }

    public class NoneState : SetupStateBase
    {
        public NoneState()
        {

        }

        public NoneState(ICategoryNodeItem module) : base(module)
        {

        }

        public override EnumMoudleSetupState GetState()
        {
            return EnumMoudleSetupState.NONE;
        }

        public override void SetComplete()
        {
            _Module.ChangeSetupState(new CompleteState(_Module));
        }

        public override void SetNotCompleted()
        {
            _Module.ChangeSetupState(new NotCompletedState(_Module));
        }

        public override void SetVerify()
        {
            _Module.ChangeSetupState(new VerifyState(_Module));
        }

        public override void SetNone()
        {
            _Module.ChangeSetupState(new NoneState(_Module));
        }
    }

}
