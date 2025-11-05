using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace PinAlignSettingViewPnP
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PinAlignSettingViewPnP : UserControl, IMainScreenView
    {
        public PinAlignSettingViewPnP()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("c55fcfdc-0dca-47a9-a94e-12f4199383ea");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
