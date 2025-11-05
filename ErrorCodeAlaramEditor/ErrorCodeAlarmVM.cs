using System;

namespace EventCodeEditor
{
    using LoaderMaster;
    using LogModule;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    public static class ErrorCodeAlarmVM 
    {

        private static MainWindow wd = null;
        private static MainWindowViewModel viewModel = new MainWindowViewModel();
        public static bool Show(LoaderSupervisor master)
        {
            bool isCheck = true;
            try
            {
                if (wd != null && wd.Visibility == Visibility.Visible)
                {
                    wd.Close();
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    wd = new MainWindow(master);
                    viewModel.PageSwitched();
                    wd.DataContext = viewModel;
                    wd.Show();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                isCheck = false;
            }
            return isCheck;
        }

    }

    public class ErrorCodeAlaramTable : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
    }
}
