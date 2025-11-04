using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcPinAlignIntervalSettingView
{
    /// <summary>
    /// PinAlignIntervalSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PinAlignIntervalSettingView : UserControl, IMainScreenView
    {
        public PinAlignIntervalSettingView()
        {
            InitializeComponent();
        }

        private Guid _ViewGUID = new Guid("F398D89E-7A38-4CD2-BA9F-291FAED435EB");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
