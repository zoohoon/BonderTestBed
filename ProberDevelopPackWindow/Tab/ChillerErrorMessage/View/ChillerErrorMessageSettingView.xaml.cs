using System.Windows;
using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// ChillerErrorMessageSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChillerErrorMessageSettingView : UserControl
    {
        ChillerErrorMessageSettingViewModel _ViewModel { get; set; }
        public ChillerErrorMessageSettingView()
        {
            _ViewModel = new ChillerErrorMessageSettingViewModel();
            this.DataContext = _ViewModel;
            InitializeComponent();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_ViewModel = null;
        }
    }
}
