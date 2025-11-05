using System.Windows;
using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GPEnvValveControlView : UserControl
    {
        private GPEnvControlViewModel _ViewModel;
        public GPEnvValveControlView()
        {
            _ViewModel = new GPEnvControlViewModel();
            this.DataContext = _ViewModel;
            InitializeComponent();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_ViewModel = null;
        }
    }
}
