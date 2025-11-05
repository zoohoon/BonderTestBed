using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcSamplePinAlignSettingView
{
    /// <summary>
    /// SamplePinAlignSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SamplePinAlignSettingView : UserControl, IMainScreenView
    {
        private Guid _ViewGUID = new Guid("43E82A8E-37C7-4CD2-97D7-8DBC93584454");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public SamplePinAlignSettingView()
        {
            InitializeComponent();
        }        
    }
}
