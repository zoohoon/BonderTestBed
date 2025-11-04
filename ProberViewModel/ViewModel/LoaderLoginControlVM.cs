using AccountModule;
using Autofac;
using DBManagerModule;
using LoaderBase.FactoryModules.ViewModelModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using SoftArcs.WPFSmartLibrary.SmartUserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace LoginControlViewModel
{
    using LoginParamObject;
    using LogModule;
    using System.Runtime.CompilerServices;

    public class LoaderLoginControlVM : ViewModelBase, IMainScreenViewModel, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> SubmitCommand
        private AsyncCommand<Object> _SubmitCommand;
        public ICommand SubmitCommand
        {
            get
            {
                if (null == _SubmitCommand) _SubmitCommand = new AsyncCommand<Object>(SubmitCommandFunc);
                return _SubmitCommand;
            }
        }
        private async Task SubmitCommandFunc(Object commandParameter)
        {
            try
            {
                try
                {
                    if (String.IsNullOrEmpty(Password))
                        return;

                    SmartLoginOverlay accessControlSystem = commandParameter as SmartLoginOverlay;
                    if (accessControlSystem == null)
                        return;

                    Account loginUser = DBManager.LoginDataList.GetUserInfoFromDB(UserName);

                    if (loginUser.Password == Password)
                    {
                        AccountManager.CurrentUserInfo.ChangeAccount(loginUser);
                        AccountManager.ChangedAccount();
                        LoggerManager.Debug($"[LoaderLoginControlVM] : Login -> {loginUser.UserName.ToString()}");
                    }
                    else if (AccountManager.CheckSuperPassword(UserName, Password))
                    {
                        Account superUser = new Account();
                        superUser.UserName = UserName;
                        superUser.Password = Password;
                        superUser.UserLevel = AccountManager.SuperUserLevel;

                        LoggerManager.Debug($"[LoaderLoginControlVM] : SUPER USER Login -> {superUser.UserName.ToString()}");

                        AccountManager.CurrentUserInfo.ChangeAccount(superUser);
                        AccountManager.ChangedAccount();

                        loginUser = superUser;
                    }
                    else
                    {
                        accessControlSystem.ShowWrongCredentialsMessage();
                        return;
                    }

            (LoginParameter as LoginParameter).LastLoginedAccount = loginUser.UserName;
                    SaveSysParameter();

                    if (loginUser.UserLevel > -1)
                    {
                        //==> 일반 계정일때
                        this.ViewModelManager().DiagnosisViewModel?.Cleanup();
                    }

                    await LoaderVMManager.HomeViewTransition();//Loader에 CUIInfo.json있는데 그건 그냥 Cell꺼 복사한것 같다. 근데 사용 못하는게 Target View GUID를 하나 밖에 못쓴다 일단 임시로 버튼에 연결된거로 안하고 바로 GUID넣음
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> AccountSelectedCommand
        private RelayCommand<Object> _AccountSelectedCommand;
        public ICommand AccountSelectedCommand
        {
            get
            {
                if (null == _AccountSelectedCommand) _AccountSelectedCommand = new RelayCommand<Object>(AccountSelectedCommandFunc);
                return _AccountSelectedCommand;
            }
        }
        private void AccountSelectedCommandFunc(Object param)
        {
            try
            {
                Object[] paramArr = param as Object[];

                if (paramArr.Length < 2)
                    return;

                Account user = paramArr[0] as Account;
                if (user == null)
                    return;

                SmartLoginOverlay smartLoginOverlay = paramArr[1] as SmartLoginOverlay;
                if (smartLoginOverlay == null)
                    return;

                UserName = user.UserName;
                smartLoginOverlay.PasswordLeftClickCommand.Execute(null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> UserName
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
        #endregion

        #region ==> Password
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

        #region ==> UserImageSource
        private string _UserImageSource;
        public string UserImageSource
        {
            get { return _UserImageSource; }
            set
            {
                if (value != _UserImageSource)
                {
                    _UserImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public bool Initialized { get; set; } = false;

        readonly Guid _ViewModelGUID = new Guid("07B25517-F926-4F79-AFAA-E67DA382F9FD");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }


        private ListCollectionView _DataSource;
        public ListCollectionView DataSource
        {
            get { return _DataSource; }
            set
            {
                if (value != _DataSource)
                {
                    _DataSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoginParameter _LoginParamClone;
        public LoginParameter LoginParamClone
        {
            get { return _LoginParamClone; }
            set { _LoginParamClone = value; }
        }
        private IParam _LoginParameter;
        public IParam LoginParameter
        {
            get { return _LoginParameter; }
            set { _LoginParameter = value; }
        }

        public LoaderLoginControlVM()
        {
            try
            {
                if (ViewModelHelper.IsInDesignModeStatic)
                    return;

                //InitializeAllCommands();

                //+ This is only neccessary if you want to display the appropriate image while typing the user name.
                //+ If you want a higher security level you wouldn't do this here !
                //! Remember : ONLY for demonstration purposes I have used a local Collection
                //==> Get all user
                UpdateUserList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized)
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                retval = EventCodeEnum.DUPLICATE_INVOCATION;
                return retval;
            }

            try
            {
                retval = LoadSysParameter();

                UserName = LoginParamClone.LastLoginedAccount;

                if (retval != EventCodeEnum.NONE)
                    LoggerManager.Error($"LoadSysParameter() Failed");

                retval = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            Initialized = true;

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        bool _IsFirstPageSwitched = true;
        public ILoaderViewModelManager LoaderVMManager => this.GetLoaderContainer().Resolve<ILoaderViewModelManager>();
        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                if (_IsFirstPageSwitched == false)
                {
                    _IsFirstPageSwitched = false;
                    return EventCodeEnum.NONE;
                }

                UpdateUserList();

                if ((LoginParameter as LoginParameter).LoginSkipEnable)
                {
                    Account superUser = new Account();
                    superUser.UserName = "SUPERUSER";
                    //superUser.Password = Password;
                    superUser.Password = AccountManager.CurrentUserInfo.Password;
                    superUser.UserLevel = AccountManager.SuperUserLevel;

                    AccountManager.CurrentUserInfo.ChangeAccount(superUser);
                    AccountManager.ChangedAccount();
                    (LoginParameter as LoginParameter).LastLoginedAccount = superUser.UserName;
                    SaveSysParameter();

                    await LoaderVMManager.HomeViewTransition();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
     
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoginParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                RetVal = this.LoadParameter(ref tmpParam, typeof(LoginParameter));
                if (RetVal == EventCodeEnum.NONE)
                {
                    LoginParameter = tmpParam;
                    LoginParamClone = LoginParameter as LoginParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(LoginParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        private void UpdateUserList()
        {
            try
            {
                List<Account> accountList = new List<Account>();

                if(DBManager.LoginDataList != null)
                {
                    List<String> userNameList = DBManager.LoginDataList.GetAllFieldByColumn(DBManager.LoginDataList.PrimaryKeyColumn);

                    if (userNameList == null)
                        return;

                    foreach (String userName in userNameList)
                    {
                        accountList.Add(DBManager.LoginDataList.GetUserInfoFromDB(userName));
                    }

                    DataSource = new ListCollectionView(accountList);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
