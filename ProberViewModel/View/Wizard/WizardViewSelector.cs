using System;

namespace WizardMainView
{
    using LogModule;
    using ProberInterfaces.Wizard;
    using System.Windows;
    using System.Windows.Controls;
    public class WizardViewSelector : DataTemplateSelector
    {
        private DataTemplate _WizardRecipeDataTemplate;

        public DataTemplate WizardRecipeDataTemplate
        {
            get { return _WizardRecipeDataTemplate; }
            set { _WizardRecipeDataTemplate = value; }
        }

        private DataTemplate _WizardCategoryDataTemplate;

        public DataTemplate WizardCategoryDataTemplate
        {
            get { return _WizardCategoryDataTemplate; }
            set { _WizardCategoryDataTemplate = value; }
        }

        private DataTemplate _WizardUtilityDataTemplate;

        public DataTemplate WizardUtilityDataTemplate
        {
            get { return _WizardUtilityDataTemplate; }
            set { _WizardUtilityDataTemplate = value; }
        }

        private DataTemplate _CategoryDeviceSettingDataTemplate;

        public DataTemplate CategoryDeviceSettingDataTemplate
        {
            get { return _CategoryDeviceSettingDataTemplate; }
            set { _CategoryDeviceSettingDataTemplate = value; }
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container == null) throw new ArgumentNullException();

            DataTemplate itemDataTemplate = null;
            try
            {
            itemDataTemplate = base.SelectTemplate(item, container);
            if (itemDataTemplate != null) return itemDataTemplate;
            FrameworkElement itemContainer = container as FrameworkElement;
            if (itemContainer == null) return null;


            if (item is IWizardRecipeVM)
                return WizardRecipeDataTemplate;
            else if (item is IWizardCategoryVM)
                return WizardCategoryDataTemplate;
            else if (item is IWizardUtilityVM)
                return WizardUtilityDataTemplate;
            else if (item is ICategoryDeviceSettingVM)
                return CategoryDeviceSettingDataTemplate;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return itemDataTemplate;
        }
    }
}
