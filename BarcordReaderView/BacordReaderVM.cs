using GPLoaderRouter;
using LogModule;
using ProberInterfaces;
using System;
using System.Windows;

namespace BarcordReaderView
{
    public class BacordReaderVM
    {
        private static MainWindow wd = null;
        public static bool Show(IGPLoader gpLoader)
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
                    if (gpLoader is GPLoader)
                    {
                        wd = new MainWindow((gpLoader as GPLoader).BCDReader);
                        wd.Show();
                    }
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
