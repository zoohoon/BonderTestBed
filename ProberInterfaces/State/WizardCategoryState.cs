namespace ProberInterfaces.State
{

    public enum EnumWizardCategoryState
    {
        UNDEFINED = 0,
        IDLE,
        INCOMPLETE,
        COMPLETED,
        MODIFY
    }

    public interface IWizardCategoryState
    {
        EnumWizardCategoryState GetWizardCategoryState();
        //void ChangeWizardCategoryState(WizardCategoryStateBase state);
        void SetIdle();
        void SetIncomplete();
        void SetCompleted();
        void SetModify();
    }


}
