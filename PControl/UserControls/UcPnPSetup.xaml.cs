using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProberInterfaces.Enum;
using System.ComponentModel;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.PinAlign.ProbeCardData;
using RecipeEditorMainPageViewModel;

namespace PnPControl.UserControls
{
    /// <summary>
    /// Interaction logic for UcPnPSetup.xaml
    /// </summary>
    public partial class UcPnPSetup : UserControl
    {
        public UcPnPSetup()
        {
            InitializeComponent();    
        }

        private void PNPSetupControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           string str =  PnpSetupView.SelectedItem.ToString();
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
            else if (item is IProbeCard)
            {
                return DutViewDataTemplate;
            }
            else if(item is ICamera)
            {
                return DisplayViewDataTemplate;
            }
            else if(item is VmRecipeEditorMainPage)
            {
                return RecipeEditDataTemplate;
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
