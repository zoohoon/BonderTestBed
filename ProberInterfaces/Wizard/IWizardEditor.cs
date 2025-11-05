namespace ProberInterfaces.Wizard
{
    using ProberErrorCode;
    public interface IWizardCategoryEditorVM
    {
        void SetContainer(Autofac.IContainer container);
        EventCodeEnum InitViewModel();
    }
}
