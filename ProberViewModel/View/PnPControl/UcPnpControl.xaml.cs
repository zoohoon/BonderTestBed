
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProberViewModel
{
    using ProberInterfaces;
    using LogModule;

    /// <summary>
    /// Interaction logic for UcPnpControl.xaml
    /// </summary>
    public partial class UcPnpControl : UserControl, IMainScreenView
    {
        public UcPnpControl()
        {
            InitializeComponent();
        }

        //readonly string _ViewModelType = "IPnpSetup";
        //public string ViewModelType { get { return _ViewModelType; } }

        readonly Guid _ViewGUID = new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void PNPSetupControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }


}
