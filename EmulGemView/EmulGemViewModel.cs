using LoaderMaster;
using LogModule;
using System;
using System.Windows;

namespace EmulGemView
{
    public static class EmulGemVM
    {


        private static MainWindow wd = null;
        public static bool Show(LoaderSupervisor master)
        {
            bool isCheck = false;
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
                    wd.Show();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }
    }

    public static class DryRunVM
    {
        private static DryRunTmpWindow wd = null;
        public static bool Show(LoaderSupervisor master)
        {
            bool isCheck = false;
            try
            {
                if (wd != null && wd.Visibility == Visibility.Visible)
                {
                    wd.Close();
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    wd = new DryRunTmpWindow(master);
                    wd.Show();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }

    }

}
