using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// LoaderWIndowsView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderWIndowsView : UserControl
    {
        LoaderWIndowsViewModel viewModel;
        public LoaderWIndowsView()
        {
            viewModel = new LoaderWIndowsViewModel();
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
