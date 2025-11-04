using System;


namespace WizardEditorView
{
    using LogModule;
    using ProberInterfaces.Wizard;
    using System.Windows;
    using System.Windows.Controls;

    public class WizardEditorViewSelector : DataTemplateSelector
    {
        private DataTemplate _WizardCategoryEditorDataTemplate;

        public DataTemplate WizardCategoryEditorDataTemplate
        {
            get { return _WizardCategoryEditorDataTemplate; }
            set { _WizardCategoryEditorDataTemplate = value; }
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


            if (item is IWizardCategoryEditorVM)
                return WizardCategoryEditorDataTemplate;

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
