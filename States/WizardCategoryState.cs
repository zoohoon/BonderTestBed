using System;

namespace States
{
    using ProberInterfaces.State;
    public abstract class WizardCategoryStateBase : IWizardCategoryState
    {

        private WizardCategoryStateBase _State;

        public WizardCategoryStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }

        public WizardCategoryStateBase(WizardCategoryStateBase state)
        {
            State = state;
        }

        public abstract EnumWizardCategoryState GetWizardCategoryState();

        public void ChangeWizardCategoryState(WizardCategoryStateBase state)
        {
            State = state;
        }

        public virtual void SetCompleted()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetWizardCategoryState()));
        }

        public virtual void SetIdle()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetWizardCategoryState()));
        }

        public virtual void SetIncomplete()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetWizardCategoryState()));
        }

        public virtual void SetModify()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetWizardCategoryState()));
        }
    }

    public class WizardCategoryIdleState : WizardCategoryStateBase
    {
        public WizardCategoryIdleState(WizardCategoryStateBase state) : base(state)
        {
        }

        public override EnumWizardCategoryState GetWizardCategoryState()
        {
            return EnumWizardCategoryState.IDLE;
        }
    }

    public class IncompleteState : WizardCategoryStateBase
    {
        public IncompleteState(WizardCategoryStateBase state) : base(state)
        {
        }

        public override EnumWizardCategoryState GetWizardCategoryState()
        {
            return EnumWizardCategoryState.INCOMPLETE;
        }
    }

    public class CompletedState : WizardCategoryStateBase
    {
        public CompletedState(WizardCategoryStateBase state) : base(state)
        {
        }

        public override EnumWizardCategoryState GetWizardCategoryState()
        {
            return EnumWizardCategoryState.COMPLETED;
        }
    }

    public class WizardCategotyModifyState : WizardCategoryStateBase
    {
        public WizardCategotyModifyState(WizardCategoryStateBase state) : base(state)
        {
        }

        public override EnumWizardCategoryState GetWizardCategoryState()
        {
            return EnumWizardCategoryState.MODIFY;
        }
    }
}
