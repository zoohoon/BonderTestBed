using System;
using System.Windows.Controls;
using ProberInterfaces;

namespace ChuckPlanaritySubSettingView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChuckPlanaritySubSettingView : UserControl, IMainScreenView
    {
        private Guid _ViewGUID = new Guid("31c6df0a-ff3c-4d31-b8bd-b7168ac4a7fb");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public ChuckPlanaritySubSettingView()
        {
            InitializeComponent();
        }
    }
}
