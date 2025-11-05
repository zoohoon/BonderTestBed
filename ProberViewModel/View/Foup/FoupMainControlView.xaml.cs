using System;
using System.Windows.Controls;
using ProberInterfaces;

namespace FoupMainControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FoupMainControlView : UserControl, IMainScreenView
    {
        public FoupMainControlView()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
