using Autofac;
using LoaderParameters;
using LogModule;
using ProberInterfaces;
using System;
using System.Windows;

namespace AlignRecoveryViewDialog
{
    public static class AlignRecoveryControlVM
    {
        private static Autofac.IContainer LoaderContainer = null;
        public static bool Show(Autofac.IContainer container,StageLotData lotData, int index)
        {
            bool isCheck = false;
            try
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow wd = new MainWindow(container, lotData,index);
                    wd.ShowDialog();
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
                if (LoaderContainer == null)
                {
                    LoaderContainer = container;
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
