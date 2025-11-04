using MahApps.Metro.Controls.Dialogs;

namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingTemplateChillingSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingTemplateChillingSetting : CustomDialog
    {
        public UcSoakingTemplateChillingSetting()
        {
            InitializeComponent();
        }
        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DeleteAlert.Visibility = System.Windows.Visibility.Visible;
        }
        private void DeleteClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DeleteAlert.Visibility = System.Windows.Visibility.Hidden;
        }
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
