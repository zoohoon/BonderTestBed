using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel
{
    /// <summary>
    /// BinAnalyzeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BINAnalyzeView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("4aa7dc92-1191-4c21-b3fa-ef9f78a2ce7a");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public BINAnalyzeView()
        {
            InitializeComponent();
        }
    }
}
