using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace MaskingSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MaskingSubView : UserControl, IMainScreenView
    {
        public MaskingSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("9f908e5e-db34-4b12-931b-73cc175457f2");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
