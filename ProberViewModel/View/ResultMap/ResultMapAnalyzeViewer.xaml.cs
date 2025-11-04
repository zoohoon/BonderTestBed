using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel.View.ResultMap
{
    /// <summary>
    /// ResultMapAnalyzeViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ResultMapAnalyzeViewer : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("e0d23fd0-73a3-4055-a60a-89308b37808b");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public ResultMapAnalyzeViewer()
        {
            InitializeComponent();
        }
    }
}
