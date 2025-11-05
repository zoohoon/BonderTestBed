namespace ProberInterfaces.Wizard
{
    using ProberErrorCode;

    public interface IWizardMainVM
    {
        void SetTargetWizardCategory(int index = -1);
        void SetTargetWizardRecipe();
        void SetTargetWizardUtility();
        void SetTargetDeviceSetting();
        void BackWizardScreen();
    }
    public interface IWizardCategoryVM
    {
        EventCodeEnum SetContainer(Autofac.IContainer container);
        EventCodeEnum InitViewModel(int index = -1);
    }

    public interface IWizardRecipeVM
    {
        EventCodeEnum InitViewModel();
    }

    public interface IWizardUtilityVM
    {
        EventCodeEnum SetContainer(Autofac.IContainer container);
        EventCodeEnum InitViewModel();
    }

    public interface ICategoryDeviceSettingVM
    {
        EventCodeEnum InitViewModel(object param);
    }
}

