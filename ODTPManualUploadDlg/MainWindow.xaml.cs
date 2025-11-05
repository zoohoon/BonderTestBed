namespace ODTPManualUploadDlg
{
    using System.Windows;
    using SpoolingUtil;
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(SpoolingManager spoolingMng)
        {
            InitializeComponent();
            this.DataContext = new ODTPDialogVM(this, spoolingMng);
        }
    }

}
