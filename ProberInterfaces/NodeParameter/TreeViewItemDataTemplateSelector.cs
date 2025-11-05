using LogModule;
using System;

namespace ProberInterfaces
{

    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    public class TreeViewItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate dataTemplate = null;
            try
            {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item is IParamNode)
                {
                    dataTemplate = element.FindResource("ParamNodeDataTemplate") as DataTemplate;
                }
                if (item is IElement)
                {
                    dataTemplate = element.FindResource("ElementNodeDataTemplate") as DataTemplate;
                }
                if (item is IList)
                {
                    dataTemplate = element.FindResource("ListNodeDataTemplate") as DataTemplate;
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return dataTemplate;
        }
    }
}
