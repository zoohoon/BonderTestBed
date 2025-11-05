using LogModule;
using System;
using System.Windows;

namespace WaferDisappearControl
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public bool IsCheck = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(double top, string moduleType, string preState, string curState) : this()
        {
            try
            {
                if (moduleType.Contains("Chuck"))
                {
                    moduleType = "Chuck";
                }
                else if (moduleType.Contains("ARM"))
                {
                    moduleType = "Arm";
                }
                else if (moduleType.Contains("PreAlign"))
                {
                    moduleType = "PreAlign";
                }

                preState.Replace('_', ' ');
                curState.Replace('_', ' ');

                lbModuleName.Content = moduleType;
                lbPreState.Content = preState;
                lbCurrentState.Content = curState;
                Loaded += ToolWindow_Loaded;

                //Owner = Application.Current.MainWindow;


                WindowStartupLocation = WindowStartupLocation.Manual;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Code to remove close box from window
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbCheck.IsChecked.Value)
                {
                    IsCheck = true;
                    this.Close();
                }
                else
                {
                    string message = "Please Check The Agreement.";
                    string caption = "";
                    MessageBoxButton buttons = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Question;
                    // MessageBox.Show(message, caption, buttons, icon)==MessageBBO
                    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                    {
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsCheck = false;
                this.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
