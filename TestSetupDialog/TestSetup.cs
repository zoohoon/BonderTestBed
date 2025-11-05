namespace TestSetupDialog
{
    using System.Windows;
    using System.Windows.Media;

    public class TestSetup
    {
        private static VmTestSetupPage _VM;
        public static void AsyncShow()
        {
            STAThread();
        }
        private static void STAThread()
        {
            _VM = new VmTestSetupPage();
            _VM.InitModule();
            _VM.InitViewModel();
            _VM.PageSwitched();

            UcTestSetupPage ucCognexOCRManualMainPage = new UcTestSetupPage();
            ucCognexOCRManualMainPage.DataContext = _VM;

            Window window = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Height = 900,
                Width = 1250,
                Content = ucCognexOCRManualMainPage
            };
            _VM.SetWindow(window);

            window.Show();
        }
    }
}
