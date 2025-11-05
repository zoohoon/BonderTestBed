using LogModule;
using NeedleCleanViewer;
using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProberViewModel
{
    public partial class PMIManualResultView : UserControl, IMainScreenView
    {
        public Guid ScreenGUID { get; }
            = new Guid("F1862D58-2C59-4FD3-8439-2C673605E2CE");

        public PMIManualResultView()
        {
            InitializeComponent();
        }
    }

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

                //foreach (Type itemInterface in item.GetType().GetInterfaces())
                //{
                //    itemDataTemplate = itemContainer.TryFindResource(new DataTemplateKey(itemInterface)) as DataTemplate;
                //    if (itemDataTemplate != null) break;
                //}

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
                else if (item is NeedleCleanViewModel)
                {
                    return NeedleCleanDataTemplate;
                }

                // Return
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
