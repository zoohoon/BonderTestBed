using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel.View.ResultMap
{
    /// <summary>
    /// E142AnalyzeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class E142MapAnalyzeView : UserControl , IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("96cbd1e2-869d-4dfa-be86-46ead2c635e3");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public E142MapAnalyzeView()
        {
            InitializeComponent();
        }
    }
}
