using System;
using System.Windows.Controls;

namespace UcDeviceUpDownSubSettingView
{
    using ProberInterfaces;
    /// <summary>
    /// DeviceUpDownSubSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DeviceUpDownSubSettingView : UserControl, IMainScreenView
    {
        private readonly Guid _ViewGUID = new Guid("8cd732eb-7cd4-4a0b-bb18-b33b99dafb90");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
        public DeviceUpDownSubSettingView()
        {
            InitializeComponent();
        }
    }
}
