using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace FoupSubView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FoupSubView : UserControl, IMainScreenView
    {
        public FoupSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("eafd6cea-ce4e-438b-8e9e-b4ebf5155641");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
