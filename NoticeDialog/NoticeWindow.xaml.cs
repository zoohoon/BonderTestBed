using System.Windows;
using System.Windows.Input;


namespace NoticeDialog
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NoticeWindow : Window
    {
        public NoticeWindow()
        {
            InitializeComponent();
            this.DataContext = new NoticeDialogViewModel();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }        
    }
}
