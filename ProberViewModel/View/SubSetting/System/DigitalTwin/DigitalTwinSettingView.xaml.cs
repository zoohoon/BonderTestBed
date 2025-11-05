using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel.View
{
    /// <summary>
    /// DigitalTwinSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DigitalTwinSettingView : UserControl, IMainScreenView, IFactoryModule
    {
        public DigitalTwinSettingView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("6b96a9a8-19c7-4cd2-b55c-42b75ddc0873");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
