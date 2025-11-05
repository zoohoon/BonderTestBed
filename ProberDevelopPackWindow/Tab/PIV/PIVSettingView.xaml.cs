using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// PIVSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PIVSettingView : UserControl
    {
        PIVSettingViewModel _ViewModel;
        public PIVSettingView()
        {
            _ViewModel = new PIVSettingViewModel();
            this.DataContext = _ViewModel;
            InitializeComponent();
        }
    }
}
