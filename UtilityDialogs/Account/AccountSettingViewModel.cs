namespace UtilityDialogs.Account
{
    using LogModule;
    using System;
    using System.Windows;

    public static class AccountSettingViewModel
    {
        private static AccountSettingWindow wd = null;
        public static bool Show()
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
                    wd = new AccountSettingWindow();
                    wd.Show();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return isCheck;
        }
    }
}
