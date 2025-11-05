using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace PMISettingView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PMISettingView : UserControl, IMainScreenView
    {
        public PMISettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("bf4d5ae0-9778-45ab-b42d-218bf162d4a6");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
