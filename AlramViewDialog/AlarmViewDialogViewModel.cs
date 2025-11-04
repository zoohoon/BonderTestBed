using LoaderBase.Communication;
using LogModule;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlarmViewDialog
{
    public static class AlarmViewDialogViewModel
    {
        private static MainWindow wd = null;

        public static bool Show(ICellInfo cellinfo)
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
                    wd = new MainWindow(cellinfo);
                    wd.ShowDialog();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isCheck;
        }

        public static bool Show(ObservableCollection<ICellInfo> cellinfos)
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

                    wd = new MainWindow(cellinfos);
                    wd.Show();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isCheck;
        }
    }
}
