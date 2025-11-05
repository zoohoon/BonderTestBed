using System;

namespace WizardCategoryView.Converter
{
    using System.Windows;
    using System.Windows.Controls;
    using LogModule;
    using ProberInterfaces.Wizard;
    public class HierarchyDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate retval = null;
            try
            {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is IWizardStep)
            {
                if(item.GetType() == typeof(IWizardMainStepTemplateModule))
                {
                    retval = element.FindResource("MainStepTemplateModuleTemplate") as DataTemplate;
                }
                //else if (item.GetType() == typeof(IWizardMainStep))
                //{
                //    retval = element.FindResource("MainStepTemplate") as DataTemplate;
                //}
                //else if (item.GetType() == typeof(IWizardTemplateModule))
                //{
                //    retval = element.FindResource("TemplateModuleTemplate") as DataTemplate;
                //}


                //HierarchyItemViewModel hierarchyItem = item as HierarchyItemViewModel;
                //if (hierarchyItem.DataItem != null)
                //{

                //    if (hierarchyItem.DataItem.GetType() == typeof(Customer))
                //    {
                //        retval = element.FindResource("CustomerTemplate") as DataTemplate;
                //    }

                //    else if (hierarchyItem.DataItem.GetType() == typeof(Order))
                //    {
                //        retval = element.FindResource("OrderTemplate") as DataTemplate;
                //    }

                //    else if (hierarchyItem.DataItem.GetType() == typeof(Product))
                //    {
                //        retval = element.FindResource("ProductTemplate") as DataTemplate;
                //    }
                //}
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }
    }
}
