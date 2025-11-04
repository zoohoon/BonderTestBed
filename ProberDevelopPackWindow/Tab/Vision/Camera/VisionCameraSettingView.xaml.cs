using System.Windows.Controls;

namespace ProberDevelopPackWindow.Tab
{
    /// <summary>
    /// VisionCameraSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisionCameraSettingView : UserControl
    {
        private VisionCameraSettingViewModel _ViewModel;
        public VisionCameraSettingView()
        {
            _ViewModel = new VisionCameraSettingViewModel();
            this.DataContext = _ViewModel;
            InitializeComponent();
        }

    }
}
