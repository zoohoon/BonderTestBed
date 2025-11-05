using System;
using System.Windows.Controls;

namespace GPCCAlignSettingView_Standard
{
    using ProberInterfaces;
    /// <summary>
    /// GPCCAlignSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPCCAlignSettingView : UserControl, IMainScreenView
    {
        public GPCCAlignSettingView()
        {
            InitializeComponent();
        }
        private readonly Guid _ViewGUID = new Guid("5910065C-ABE1-4FFD-8C11-C2E2ECC2B118");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
