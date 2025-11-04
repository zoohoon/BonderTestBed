using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcTempDeviceSettingView
{
    public partial class TempDeviceSettingView : UserControl, IMainScreenView
    {
        public TempDeviceSettingView()
        {
            InitializeComponent();
        }

        public Guid ScreenGUID { get; set; }
            = new Guid("38059571-0235-407E-BE2F-B3CF6073034A");
    }
}
