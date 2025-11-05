using System;

namespace EnvControlWindow.GP
{
    using LoaderMaster;
    using LogModule;
    using System.Windows;
    public static class GPEnvControlVM
    {
        private static GPEnvControlMainWindow wd = null;
        public static bool Show(LoaderSupervisor master)
        {
            bool isCheck = true;
            try
            {
                if (wd != null && wd.Visibility == Visibility.Visible)
                {
                    wd.Close();
                }

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    wd = new GPEnvControlMainWindow(master);
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
}
