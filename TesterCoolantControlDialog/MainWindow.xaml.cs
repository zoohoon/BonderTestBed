using System.Windows;

namespace TesterCoolantControlDialog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TesterCoolantControlViewModel viewModel;
        public MainWindow()
        {
            viewModel = new TesterCoolantControlViewModel();
            this.DataContext = viewModel;
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel.UpdateValveStates();
        }
    }
}
