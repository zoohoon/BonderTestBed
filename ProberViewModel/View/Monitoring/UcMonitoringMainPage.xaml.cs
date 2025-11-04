using System;
using System.Windows.Controls;

namespace MonitoringMainPageView
{
    using ProberInterfaces;
    /// <summary>
    /// Interaction logic for UcMonitoringMainPage.xaml
    /// </summary>
    public partial class UcMonitoringMainPage : UserControl, IMainScreenView
    {
        public UcMonitoringMainPage()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("8d6da80d-f181-4d4c-8a96-4a1367682f99");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
