using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace LotInfoYieldControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LotInfoYieldView : UserControl, IMainScreenView
    {
        public LotInfoYieldView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("b0722a2f-bc56-4e74-88e7-63fbf4ec7d63");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
