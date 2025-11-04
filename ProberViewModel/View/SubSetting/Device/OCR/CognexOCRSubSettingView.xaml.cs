using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace CognexOCRSubSettingView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CognexOCRSubSettingView : UserControl, IMainScreenView
    {
        public CognexOCRSubSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("c1f9e022-9fac-4214-88c9-f3019234c1a4");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
