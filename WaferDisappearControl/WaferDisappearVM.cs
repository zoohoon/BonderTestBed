using LogModule;
using System;
using System.Windows;

namespace WaferDisappearControl
{
    public static class WaferDisappearVM
    {
        private static Autofac.IContainer StageContainer = null;
        public static bool Show(Autofac.IContainer container, string ModuleName, string PreState, string CurrentState)
        {
            bool isCheck = false;
            try
            {
                if (StageContainer == null)
                {
                    StageContainer = container;
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow wd = new MainWindow(1, ModuleName, PreState, CurrentState);
                    wd.ShowDialog();
                    isCheck = wd.IsCheck;
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }
        public static bool Show(Autofac.IContainer container)
        {
            bool isCheck = false;
            try
            {
                if (StageContainer == null)
                {
                    StageContainer = container;
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow wd = new MainWindow();
                    wd.ShowDialog();
                    isCheck = wd.IsCheck;
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
