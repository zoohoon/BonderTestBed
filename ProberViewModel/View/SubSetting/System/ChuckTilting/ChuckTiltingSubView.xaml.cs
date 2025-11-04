using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ChuckTiltingSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChuckTiltingSubView : UserControl, IMainScreenView
    {
        public ChuckTiltingSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("f78eec9a-874c-4289-b588-3276295b2d67");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
