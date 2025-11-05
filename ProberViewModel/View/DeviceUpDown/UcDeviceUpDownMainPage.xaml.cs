using System;
using System.Windows.Controls;

namespace DeviceUpDownMainPageView
{
    using ProberInterfaces;
    /// <summary>
    /// UcDeviceUpDownMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcDeviceUpDownMainPage : UserControl, IMainScreenView
    {
        public UcDeviceUpDownMainPage()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("5B755687-AE34-4BBF-8112-71738B9F91D8");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
