using LogModule;
using ProberInterfaces;
using System;
using System.Windows;
using LoaderBase;

namespace ProberViewModel
{
    public static class ModuleInfoVM
    {
        private static MainWindow wd =null;
        public static bool Show(ModuleTypeEnum moduleType, object item, ILoaderSupervisor loaderMaster)
        {
            bool isCheck = false;
            try
            {
                if (wd != null && wd.Visibility == Visibility.Visible)
                {
                    wd.Dispatcher.Invoke(() => { wd.Close(); });
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    wd = new MainWindow(moduleType, item, loaderMaster);
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
