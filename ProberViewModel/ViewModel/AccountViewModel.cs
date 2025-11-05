using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountVM
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using AccountModule;
    using DBManagerModule;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using VirtualKeyboardControl;

    public class AccountViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region ==> UserNameTextBoxClickCommand
        private RelayCommand _UserNameTextBoxClickCommand;
        public ICommand UserNameTextBoxClickCommand
        {
            get
            {
                if (null == _UserNameTextBoxClickCommand) _UserNameTextBoxClickCommand = new RelayCommand(UserNameTextBoxClickCommandFunc);
                return _UserNameTextBoxClickCommand;
            }
        }

        private void UserNameTextBoxClickCommandFunc()
        {
            UserNameInputBox = VirtualKeyboard.Show(UserNameInputBox, KB_TYPE.ALPHABET | KB_TYPE.DECIMAL);
        }
        #endregion

        #region ==> PasswordTextBoxClickCommand : 패스워드 입력
        private RelayCommand _PasswordTextBoxClickCommand;
        public ICommand PasswordTextBoxClickCommand
        {
            get
            {
                if (null == _PasswordTextBoxClickCommand) _PasswordTextBoxClickCommand = new RelayCommand(PasswordTextBoxClickCommandFunc);
                return _PasswordTextBoxClickCommand;
            }
        }
        private void PasswordTextBoxClickCommandFunc()
        {
            try
            {
                
                _SecurePassword = VirtualKeyboard.Show(_SecurePassword, KB_TYPE.PASSWORD);

                //==> 화면에는 패스워드가 * 로 표시하기 위해
                String str = String.Empty;
                for (int i = 0; i < _SecurePassword.Length; ++i)
                {
                    str += '*';
                }
                PasswordInputBox = str;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //==> 실제 Password 문자열이 담기는 곳
        private String _SecurePassword = String.Empty;
        #endregion

        #region ==> CheckPasswordTextBoxClickCommand : 패스워드 확인 입력
        private RelayCommand _CheckPasswordTextBoxClickCommand;
        public ICommand CheckPasswordTextBoxClickCommand
        {
            get
            {
                if (null == _CheckPasswordTextBoxClickCommand) _CheckPasswordTextBoxClickCommand = new RelayCommand(CheckPasswordTextBoxClickCommandFunc);
                return _CheckPasswordTextBoxClickCommand;
            }
        }
        private void CheckPasswordTextBoxClickCommandFunc()
        {
            try
            {
                _SecureCheckPassword = VirtualKeyboard.Show(_SecureCheckPassword, KB_TYPE.PASSWORD);

                if (_SecurePassword != _SecureCheckPassword)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", "Password is not match", EnumMessageStyle.Affirmative);

                    _SecureCheckPassword = String.Empty;
                    CheckPasswordInputBox = String.Empty;
                    return;
                }

                String str = String.Empty;
                for (int i = 0; i < _SecureCheckPassword.Length; ++i)
                {
                    str += '*';
                }
                CheckPasswordInputBox = str;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //==> 실제 Chuck Password 문자열이 담기는 곳
        private String _SecureCheckPassword = String.Empty;
        #endregion

        #region ==> UserLevelTextBoxClickCommand : UserLevel 입력
        private RelayCommand _UserLevelTextBoxClickCommand;
        public ICommand UserLevelTextBoxClickCommand
        {
            get
            {
                if (null == _UserLevelTextBoxClickCommand) _UserLevelTextBoxClickCommand = new RelayCommand(UserLevelTextBoxClickCommandFunc);
                return _UserLevelTextBoxClickCommand;
            }
        }

        private void UserLevelTextBoxClickCommandFunc()
        {
            try
            {
                String strLevel = VirtualKeyboard.Show(UserLevelInputBox.ToString(), KB_TYPE.DECIMAL);
                int level;

                if (int.TryParse(strLevel, out level) == false)
                    return;

                UserLevelInputBox = level;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> AddCommand        
        private RelayCommand _AddCommand;
        public ICommand AddCommand
        {
            get
            {
                if (null == _AddCommand) _AddCommand = new RelayCommand(AddCommandFunc);
                return _AddCommand;
            }
        }
        private void AddCommandFunc()
        {
            //==> 현재 계정보다 음수로 낮은 Level 설정 불가능
            if (AccountManager.IsUserLevelAboveThisNum(UserLevelInputBox) == false)
            {
                this.MetroDialogManager().ShowMessageDialog("Account", $"Can't setup User Level : {UserLevelInputBox}", EnumMessageStyle.Affirmative);
                return;
            }

            if (String.IsNullOrWhiteSpace(UserNameInputBox))
            {
                this.MetroDialogManager().ShowMessageDialog("Account", "Input User Name", EnumMessageStyle.Affirmative);
                return;
            }

            if (String.IsNullOrWhiteSpace(CheckPasswordInputBox))
            {
                this.MetroDialogManager().ShowMessageDialog("Account", "Input Password", EnumMessageStyle.Affirmative);
                return;
            }

            if (_SecurePassword != _SecureCheckPassword)
            {
                this.MetroDialogManager().ShowMessageDialog("Account", "Password is not match", EnumMessageStyle.Affirmative);
                _SecureCheckPassword = String.Empty;
                CheckPasswordInputBox = String.Empty;
                return;
            }

            if (UserNameInputBox == AccountManager.DefaultUserName)
            {
                this.MetroDialogManager().ShowMessageDialog("Account", $"You can't set '{AccountManager.DefaultUserName}'", EnumMessageStyle.Affirmative);
                return;
            }

            if (AccountList.FirstOrDefault(item => item.UserName == UserNameInputBox) != null)
            {
                this.MetroDialogManager().ShowMessageDialog("Account", "User is Aready exist", EnumMessageStyle.Affirmative);
                return;
            }
            DBManager.LoginDataList.AddRecord(UserNameInputBox, _SecurePassword, UserLevelInputBox, AccountManager.DefaultImageSource);
            UpdateUserList();

            UserNameInputBox = String.Empty;
            PasswordInputBox = String.Empty;
            CheckPasswordInputBox = String.Empty;
        }
        #endregion

        #region ==> DeleteCommand
        private AsyncCommand _DeleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (null == _DeleteCommand) _DeleteCommand = new AsyncCommand(DeleteCommandFunc);
                return _DeleteCommand;
            }
        }
        private async Task DeleteCommandFunc()
        {
            try
            {
                if (SelectedAccount == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", "User is not Selected", EnumMessageStyle.Affirmative);
                    return;
                }

                if (SelectedAccount.UserName == AccountManager.DefaultUserName)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", $"You can't delete '{AccountManager.DefaultUserName}'", EnumMessageStyle.Affirmative);
                    return;
                }

                if (SelectedAccount.UserName == AccountManager.CurrentUserInfo.UserName)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", $"You can't delete Logined user'{AccountManager.CurrentUserInfo.UserName}'", EnumMessageStyle.Affirmative);
                    return;
                }

                if (AccountManager.IsUserLevelAboveThisNum(SelectedAccount.UserLevel) == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", $"Your level is low'{AccountManager.CurrentUserInfo.UserName}'", EnumMessageStyle.Affirmative);
                    return;
                }


                //==> 이 함수가 async인 이유는 this.MetroDialogManager().ShowMessageDialog 때문이다.
                //EnumMessageDialogResult msgRes = this.MetroDialogManager().ShowMessageDialog(
                //    "Account",
                //    $"Do you realy want Delete '{SelectedAccount.UserName}'",
                //    "OK",
                //    "Cancel").Result;

                EnumMessageDialogResult msgRes = this.MetroDialogManager().ShowMessageDialog("Account", $"Do you realy want Delete '{SelectedAccount.UserName}'", EnumMessageStyle.AffirmativeAndNegative).Result;

                if (msgRes == EnumMessageDialogResult.NEGATIVE)
                    return;

                DBManager.LoginDataList.RemoveRecord(SelectedAccount.UserName);

                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    UpdateUserList();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> ModifyCommand        
        private RelayCommand _ModifyCommand;
        public ICommand ModifyCommand
        {
            get
            {
                if (null == _ModifyCommand) _ModifyCommand = new RelayCommand(ModifyCommandFunc);
                return _ModifyCommand;
            }
        }
        private void ModifyCommandFunc()
        {
            try
            {
                if (SelectedAccount == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", "User is not Selected", EnumMessageStyle.Affirmative);
                    return;
                }
                if (AccountManager.IsUserLevelAboveThisNum(UserLevelInputBox) == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", $"Can't setup User Level : {UserLevelInputBox}", EnumMessageStyle.Affirmative);
                    return;
                }
                if (String.IsNullOrWhiteSpace(CheckPasswordInputBox))
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", "Input Password", EnumMessageStyle.Affirmative);
                    return;
                }
                if (_SecurePassword != _SecureCheckPassword)
                {
                    this.MetroDialogManager().ShowMessageDialog("Account", "Password is not match", EnumMessageStyle.Affirmative);
                    _SecureCheckPassword = String.Empty;
                    CheckPasswordInputBox = String.Empty;
                    return;
                }

                DBManager.LoginDataList.ModifyRecord(SelectedAccount.UserName, _SecurePassword, UserLevelInputBox, SelectedAccount.ImageSource);
                DisableModifyCommandFunc();
                UpdateUserList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> EnableModifyCommand
        private RelayCommand _EnableModifyCommand;
        public ICommand EnableModifyCommand
        {
            get
            {
                if (null == _EnableModifyCommand) _EnableModifyCommand = new RelayCommand(EnableModifyCommandFunc);
                return _EnableModifyCommand;
            }
        }
        private void EnableModifyCommandFunc()
        {
            try
            {
                UserNameInputBox = SelectedAccount.UserName;
                PasswordInputBox = SelectedAccount.Password;
                _SecurePassword = SelectedAccount.Password;
                CheckPasswordInputBox = String.Empty;
                _SecureCheckPassword = String.Empty;
                UserLevelInputBox = SelectedAccount.UserLevel;

                IsModifyMode = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            if (SelectedAccount == null)
            {
                this.MetroDialogManager().ShowMessageDialog("Account", "User is not Selected", EnumMessageStyle.Affirmative);
                return;
            }
        }
        #endregion

        #region ==> DisableModifyCommand
        private RelayCommand _DisableModifyCommand;
        public ICommand DisableModifyCommand
        {
            get
            {
                if (null == _DisableModifyCommand) _DisableModifyCommand = new RelayCommand(DisableModifyCommandFunc);
                return _DisableModifyCommand;
            }
        }
        private void DisableModifyCommandFunc()
        {
            try
            {
                UserNameInputBox = String.Empty;
                PasswordInputBox = String.Empty;
                _SecurePassword = String.Empty;
                CheckPasswordInputBox = String.Empty;
                _SecureCheckPassword = String.Empty;
                UserLevelInputBox = _DefaultUserLevel;

                SelectedAccount = null;

                IsModifyMode = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> UserNameInputBox
        private String _UserNameInputBox;
        public String UserNameInputBox
        {
            get { return _UserNameInputBox; }
            set
            {
                if (value != _UserNameInputBox)
                {
                    _UserNameInputBox = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PasswordInputBox
        private String _PasswordInputBox;
        public String PasswordInputBox
        {
            get { return _PasswordInputBox; }
            set
            {
                if (value != _PasswordInputBox)
                {
                    _PasswordInputBox = value;
                    RaisePropertyChanged();
                }

                CheckPasswordInputBoxEnable =
                    String.IsNullOrWhiteSpace(_PasswordInputBox) ? false : true;
            }
        }
        #endregion

        #region ==> CheckPasswordInputBox
        private string _CheckPasswordInputBox;
        public string CheckPasswordInputBox
        {
            get { return _CheckPasswordInputBox; }
            set
            {
                if (value != _CheckPasswordInputBox)
                {
                    _CheckPasswordInputBox = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CheckPasswordInputBoxEnable
        private bool _CheckPasswordInputBoxEnable;
        public bool CheckPasswordInputBoxEnable
        {
            get { return _CheckPasswordInputBoxEnable; }
            set
            {
                if (value != _CheckPasswordInputBoxEnable)
                {
                    _CheckPasswordInputBoxEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> UserLevelInputBox
        private int _UserLevelInputBox;
        public int UserLevelInputBox
        {
            get { return _UserLevelInputBox; }
            set
            {
                if (value != _UserLevelInputBox)
                {
                    _UserLevelInputBox = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedAccount
        private Account _SelectedAccount;
        public Account SelectedAccount
        {
            get { return _SelectedAccount; }
            set
            {
                if (value != _SelectedAccount)
                {
                    _SelectedAccount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> IsModifyMode
        private bool _IsModifyMode;
        public bool IsModifyMode
        {
            get { return _IsModifyMode; }
            set
            {
                if (value != _IsModifyMode)
                {
                    _IsModifyMode = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> IsVisible
        private Visibility _IsVisible;
        public Visibility IsVisible
        {
            get { return _IsVisible; }
            set
            {
                if (value != _IsVisible)
                {
                    _IsVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private readonly Guid _ViewModelGUID = new Guid("e4a7761e-d257-45ca-9752-76ea3e35e314");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;
        private const int _DefaultUserLevel = 100;
        public ObservableCollection<Account> AccountList { get; set; }

        private void UpdateUserList()
        {
            List<String> accountList = DBManager.LoginDataList.GetAllFieldByColumn(DBManager.LoginDataList.PrimaryKeyColumn);

            AccountList.Clear();
            foreach (String accountName in accountList)
            {
                Account account = DBManager.LoginDataList.GetUserInfoFromDB(accountName);
                AccountList.Add(account);
            }
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized == false)
            {
                AccountList = new ObservableCollection<Account>();

                IsModifyMode = false;

                UserLevelInputBox = _DefaultUserLevel;

                Initialized = true;

                retval = EventCodeEnum.NONE;
            }
            else
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                retval = EventCodeEnum.DUPLICATE_INVOCATION;
            }

            return retval;
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            if (!AccountManager.IsUserLevelAboveThisNum(0))
            {
                //this.MetroDialogManager().ShowMessageDialog("Account", $"Your are not Super User\nYou can't see this page", EnumMessageStyle.Affirmative).Wait();
                IsVisible = Visibility.Hidden;
            }
            else
            {
                UpdateUserList();
                IsVisible = Visibility.Visible;
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");
            AccountList.Clear();

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {GetType().Name}");
        }
    }
}