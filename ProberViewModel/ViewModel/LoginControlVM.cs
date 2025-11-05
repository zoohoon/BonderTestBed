using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ProberErrorCode;
using System.ComponentModel;
using System.Windows.Input;
using SoftArcs.WPFSmartLibrary.SmartUserControls;
using DBManagerModule;
using System.Windows.Data;
using RelayCommandBase;
using System.Windows;
using AccountModule;

namespace LoginControlViewModel
{
    using CUIServices;
    using LoginParamObject;
    using LogModule;
    using MetroDialogInterfaces;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class LoginControlVM : ViewModelBase, IMainScreenViewModel, IHasDevParameterizable
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

                    // 일반 계정으로 설정 시, Forced Done을 사용할 수 없음.
                    // 초기화 시켜놓아야 한다.
                    foreach (IStateModule item in this.LotOPModule().RunList)
                    {
                        item.ForcedDone = EnumModuleForcedState.Normal;
                    }

                }
                else if (AccountManager.CheckSuperPassword(UserName, Password))
                {
                    Account superUser = new Account();
                    superUser.UserName = UserName;
                    superUser.Password = Password;
                    superUser.UserLevel = AccountManager.SuperUserLevel;

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
                SaveDevParameter();

                if (loginUser.UserLevel > -1)
                {
                    //==> 일반 계정일때
                    this.ViewModelManager().DiagnosisViewModel?.Cleanup();
                }

                this.LotOPModule().LotInfo.OperatorID.Value = loginUser.UserName;
                Guid tmp = new Guid();
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tmp = CUIService.GetTargetViewGUID(accessControlSystem.SubmitButtonGUID);
                }));
                await this.ViewModelManager().ViewTransitionAsync(tmp);

                if (this.MonitoringManager().IsMachineInitDone == false)
                {
                    var ret = await AccountManager.MachineInit();

                    // TODO : ErrorCedResult.ErrorMsg를 통해, 구체적인 이유를 Dialog에 나타낼 것.
                    if (ret.ErrorCode != EventCodeEnum.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("MachineInit Failed", $"ErrorCode: {ret.ErrorCode.ToString()}", EnumMessageStyle.Affirmative);
                    }
                }

                this.MonitoringManager().MachineMonitoring();
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

        readonly Guid _ViewModelGUID = new Guid("1F60343B-5F92-4AA1-99DA-788A4E12C25D");
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

        public LoginControlVM()
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
                retval = LoadDevParameter();

                UserName = LoginParamClone.LastLoginedAccount;

                if (retval != EventCodeEnum.NONE)
                    LoggerManager.Error($"LoadDevParameter() Failed");

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

                if (this.ViewModelManager().LoginSkipEnable)
                {
                    Account superUser = new Account();
                    superUser.UserName = "SUPERUSER";
                    superUser.Password = Password;
                    superUser.UserLevel = AccountManager.SuperUserLevel;

                    AccountManager.CurrentUserInfo.ChangeAccount(superUser);
                    AccountManager.ChangedAccount();
                    (LoginParameter as LoginParameter).LastLoginedAccount = superUser.UserName;
                    SaveDevParameter();

                    this.LotOPModule().LotInfo.OperatorID.Value = superUser.UserName;

                    Guid tmp = new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D");

                    await this.ViewModelManager().ViewTransitionAsync(tmp);

                    var ret = await AccountManager.MachineInit();
                    
                    int cellNo = this.LoaderController().GetChuckIndex();
                    string multiexecuterIP = "127.0.0.1";
                    int multiexecuterPort = 13000;
                    await CheckAvaUDPComm(multiexecuterIP, multiexecuterPort, cellNo);

                    // TODO : ErrorCedResult.ErrorMsg를 통해, 구체적인 이유를 Dialog에 나타낼 것.
                    if (ret.ErrorCode != EventCodeEnum.NONE)
                    {
                       this.MetroDialogManager().ShowMessageDialog("MachineInit Failed", $"ErrorCode: {ret.ErrorCode.ToString()}", EnumMessageStyle.Affirmative);
                    }

                    await this.MonitoringManager().MachineMonitoring();

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Visibility v = (System.Windows.Application.Current.MainWindow).Visibility;

                        if (v == Visibility.Visible)
                        {
                            // 250911 LJH Widget 안보이게
                            //this.ViewModelManager().ViewTransitionAsync(new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"));
                            //(System.Windows.Application.Current.MainWindow).Hide();
                            //this.ViewModelManager().UpdateWidget();
                            //this.ViewModelManager().MainWindowWidget.Show();

                            //(System.Windows.Application.Current.MainWindow).Title = "TestTitle";
                            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                            {
                                SetWindowText(Process.GetCurrentProcess().MainWindowHandle, "POSIsReady"+$"{this.LoaderController().GetChuckIndex()}");
                            }
                            else
                            {
                                SetWindowText(Process.GetCurrentProcess().MainWindowHandle, "POSIsReady");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"MainWindow Visibility is not visible.");
                        }
                    }));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        private static EndPoint remoteEP;
        private static Socket udpSocket;

        private async Task CheckAvaUDPComm(string ip, int port, int cellNo)
        {
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                remoteEP = new IPEndPoint(IPAddress.Broadcast, port);
                udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                string sendString = "POSIsReady_" + cellNo;
                byte[] sendBuffer = Encoding.UTF8.GetBytes(sendString);
                udpSocket.SendTo(sendBuffer, sendBuffer.Length, SocketFlags.None, remoteEP);
            }
            catch (SocketException err)
            {
                LoggerManager.Error(err.Message);
                if (udpSocket != null)
                {
                    udpSocket.Close();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                udpSocket.Close();
            }
        }

        [DllImport("user32.dll")]
        public static extern int SetWindowText(IntPtr hWnd, string text);

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

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum LoadDevParameter()
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
        public EventCodeEnum SaveDevParameter()
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
