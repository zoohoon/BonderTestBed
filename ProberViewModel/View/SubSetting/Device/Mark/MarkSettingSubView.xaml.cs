using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace PMISettingSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MarkSettingSubView : UserControl, IMainScreenView
    {
        public MarkSettingSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("297ecf90-74ae-47e3-aba5-d21d01eb095a");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
