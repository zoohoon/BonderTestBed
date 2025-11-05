using System;
using System.Windows;
using System.Windows.Controls;

namespace DataTemplateService
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.NeedleClean;

    public class TemplateSelector : DataTemplateSelector
    {
        private DataTemplate _MapViewDataTemplate;

        public DataTemplate MapViewDataTemplate
        {
            get { return _MapViewDataTemplate; }
            set { _MapViewDataTemplate = value; }
        }

        private DataTemplate _DisplayViewDataTemplate;

        public DataTemplate DisplayViewDataTemplate
        {
            get { return _DisplayViewDataTemplate; }
            set { _DisplayViewDataTemplate = value; }
        }

        private DataTemplate _RecipeEditDataTemplate;
        public DataTemplate RecipeEditDataTemplate
        {
            get { return _RecipeEditDataTemplate; }
            set { _RecipeEditDataTemplate = value; }
        }

        private DataTemplate _DutViewDataTemplate;
        public DataTemplate DutViewDataTemplate
        {
            get { return _DutViewDataTemplate; }
            set { _DutViewDataTemplate = value; }
        }

        public DataTemplate _NeedleCleanDataTemplate;
        public DataTemplate NeedleCleanDataTemplate
        {
            get { return _NeedleCleanDataTemplate; }
            set { _NeedleCleanDataTemplate = value; }
        }

        public DataTemplate _DutViewerDataTemplate;
        public DataTemplate DutViewerDataTemplate
        {
            get { return _DutViewerDataTemplate; }
            set { _DutViewerDataTemplate = value; }
        }
        public DataTemplate _PatternViewDataTemplate;
        public DataTemplate PatternViewDataTemplate
        {
            get { return _PatternViewDataTemplate; }
            set { _PatternViewDataTemplate = value; }
        }
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            #region Require

            //if (item == null) throw new ArgumentNullException();
            if (container == null) throw new ArgumentNullException();

            #endregion

            // Result
            DataTemplate itemDataTemplate = null;
            try
            {

                // Base DataTemplate
                itemDataTemplate = base.SelectTemplate(item, container);
                if (itemDataTemplate != null) return itemDataTemplate;

                // Interface DataTemplate
                FrameworkElement itemContainer = container as FrameworkElement;
                if (itemContainer == null) return null;

                if (item is IWaferObject)
                {
                    return MapViewDataTemplate;
                }
                else if (item is ICamera)
                {
                    return DisplayViewDataTemplate;
                }
                else if (item is IVmRecipeEditorMainPage)
                {
                    return RecipeEditDataTemplate;
                }
                else if (item is INeedleCleanViewModel)
                {
                    return NeedleCleanDataTemplate;
                }
                else if (item is IProbeCard)
                {
                    return DutViewerDataTemplate;
                }
                else if (item is ImageBuffer)
                {
                    return PatternViewDataTemplate;
                }

                // Return
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return itemDataTemplate;
        }

    }
}
