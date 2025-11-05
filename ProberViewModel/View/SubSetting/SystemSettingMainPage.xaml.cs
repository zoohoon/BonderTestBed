using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SystemSettingMainView
{
    /// <summary>
    /// SettingMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SystemSettingMainPage : UserControl, IMainScreenView
    {
        public SystemSettingMainPage()
        {
            InitializeComponent();
        }

        private Guid _ViewGUID = new Guid("A43A956D-B46E-61B6-5A61-31766111750D");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
