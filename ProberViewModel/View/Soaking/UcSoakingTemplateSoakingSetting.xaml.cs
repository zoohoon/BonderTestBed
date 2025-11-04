using MahApps.Metro.Controls.Dialogs;

namespace SoakingSettingView
{   /// <summary>
    /// UcSoakingTemplateSoakingSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingTemplateSoakingSetting : CustomDialog
    {
        public UcSoakingTemplateSoakingSetting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Delete 버튼을 선택하면 사용자 실수를 방지하기 위해 한번더 확인.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DeleteAlert.Visibility = System.Windows.Visibility.Visible;
        }
        private void DeleteClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DeleteAlert.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Apply를 선택하면 사용자 실수를 방지하기 위해 한번더 확인. 
        /// => 유효성 검증에 실패하는 경우 일부 설정값을 유효성에 맞게 변경하여 적용할지 사용자에게 한번더 확인. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Apply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplyAlert.Visibility = System.Windows.Visibility.Visible;
            ApplyButton.Focus();
        }

        private void ApplyClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplyAlert.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
