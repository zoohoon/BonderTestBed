using LogModule;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LoaderRecoveryControl
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

        private LoaderRecoveryMaster _RecoveryMaster;
        public LoaderRecoveryMaster RecoveryMaster
        {
            get { return _RecoveryMaster; }
            set
            {
                if (value != _RecoveryMaster)
                {
                    _RecoveryMaster = value;
                }
            }
        }

        public bool IsCheck = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(double top, string erroCode, string details, LoaderRecoveryMaster recoveryMaster) : this()
        {
            try
            {
                RecoveryMaster = recoveryMaster;
                lbErrorCode.Content = erroCode;
                lbErrorDetail.Content = details;
                Loaded += ToolWindow_Loaded;
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
                if (cbCheck.IsChecked.Value && RecoveryMaster.RecoveryFinished)
                {
                    IsCheck = true;
                    this.Close();
                }
                else
                {
                    string message = "Please proceed with recovery";
                    string caption = "Warning";
                    MessageBoxButton buttons = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Question;
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
                if (RecoveryMaster.RecoveryFinished == false)
                {
                    string message = "There are recovery steps that have not been completed. Would you like to close the window anyway?";
                    string caption = "Warning";
                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Question;
                    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                    {
                        LoggerManager.Debug($"LoaderRecovery Cancel button - Yes");
                        IsCheck = false;
                        this.Close();
                    }
                    else
                    {
                        LoggerManager.Debug($"LoaderRecovery Cancel button - No");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class BoolToColorConverter : IMultiValueConverter
    {
        public object Convert(
         object[] value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            Brush retBrush = Brushes.Transparent;
            try
            {
                if (value[0] is int && value[1] is int)
                {
                    int stepidx = (int)value[0];
                    int curidx = (int)value[1];

                    if (stepidx == curidx)
                    {
                        if (value[2] is bool)
                        {
                            bool bValue = (bool)value[2];

                            if (bValue == true)
                            {
                                retBrush = Brushes.GreenYellow;
                            }
                            else
                            {
                                retBrush = Brushes.Orange;
                            }
                        }
                    }
                    else
                    {
                        if (value[2] is bool)
                        {
                            bool bValue = (bool)value[2];

                            if (bValue == true)
                            {
                                retBrush = Brushes.GreenYellow;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retBrush;
        }

        public object[] ConvertBack(
         object value, Type[] targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                // I don't think you'll need this
                throw new Exception("Can't convert back");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
