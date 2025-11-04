using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace IOSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOSubView : UserControl, IMainScreenView
    {
        public IOSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("e9a9d64a-4b28-40f1-9f64-1f348dc183ae");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
