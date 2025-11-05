using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel.View.LogView
{
    /// <summary>
    /// LogSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogSettingView : UserControl, IMainScreenView
    {
        public LogSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("ba31e6d7-f7d1-4234-92de-0c373e87d7ee");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
