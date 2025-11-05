using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace DeviceSettingMainView
{
    public partial class DeviceSettingMainPage : UserControl, IMainScreenView
    {
        public DeviceSettingMainPage()
        {
            InitializeComponent();
        }

        private Guid _ViewGUID = new Guid("C059599B-BEDF-2137-859B-47C15E433E4D");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
