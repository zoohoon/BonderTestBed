using System.Windows;
using System.Windows.Input;
//using ProberViewModel.ViewModel;
using SpoolingUtil;

namespace LoaderResultMapUpDown.LoaderResulMapManualUpload
{
    /// <summary>
    /// LoaderResultMapManualUploadDlgxaml.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderResultMapManualUploadDlg : Window
    {
        public LoaderResultMapManualUploadDlg(SpoolingManager spoolingMng)
        {
            InitializeComponent();
            this.DataContext = new LoadResultMapManualUploadViewVM(this, spoolingMng);
        }

        private void ResultMapManualUploadWnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
