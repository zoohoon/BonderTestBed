using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ProberViewModel.View.GPLoaderSetupWithCell
{
    /// <summary>
    /// GPLoaderSetupWithCellView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPLoaderSetupWithCellView : UserControl, IMainScreenView
    {
        public GPLoaderSetupWithCellView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("5FBF9EB1-C022-4FE6-A665-6569D45EFD3A");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
