namespace UtilityDialogs.Account
{
    using LogModule;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AccountSettingWindow : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Property </remarks>
        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        public AccountSettingWindow()
        {
            try
            {
                UserName = AccountModule.AccountManager.CurrentUserInfo.UserName;
                Password = AccountModule.AccountManager.CurrentUserInfo.Password;
                this.DataContext = this;
                InitializeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }      
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AccountModule.AccountManager.CurrentUserInfo.Password = Password;
                AccountModule.AccountManager.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
