using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace AccountModule
{
    using Autofac;
    using ProberErrorCode;
    using LogModule;
    using ProberInterfaces;

    public delegate void AccountChangedDelegate();

    public static class AccountManager
    {
        public const int MIN_USER_LEVEL = int.MaxValue;
        public const int MAX_USER_LEVEL = int.MinValue;
        public const int SuperUserLevel = int.MinValue;
        public const int AdminUserLevel = 0;
        public const String DefaultUserName = "admin";
        public const int DefaultUserLevel = 0;
        public const String DefaultImageSource = "pack://application:,,,/Resources/logo.png";
        private static Autofac.IContainer Container;

        public static AccountChangedDelegate accountChangedDelegate { get; set; }

        public static Account CurrentUserInfo { get; set; } = new Account() { UserLevel = DefaultUserLevel };

        public static MaskingLevelListParameter MaskingLevelListParameter { get; set; }

        public static ObservableCollection<int> MaskingLevelCollection
        {
            get { return MaskingLevelListParameter.MaskingLevelCollection; }
        }

        //==> Super Account 계정의 Password 생성
        public static String MakeSuperAccountPassword()
        {
            String superPassword = String.Empty;

            try
            {
                DateTime today = DateTime.Today;

                String monthStr = DateTime.Now.ToString("MM");
                String dayStr = DateTime.Now.ToString("dd");

                String monthDayStr = monthStr + dayStr;

                foreach (char ch in monthDayStr)
                {
                    switch (ch)
                    {
                        case '0':
                            superPassword += 'z';
                            break;
                        case '1':
                            superPassword += 'o';
                            break;
                        case '2':
                            superPassword += 't';
                            break;
                        case '3':
                            superPassword += 't';
                            break;
                        case '4':
                            superPassword += 'f';
                            break;
                        case '5':
                            superPassword += 'f';
                            break;
                        case '6':
                            superPassword += 's';
                            break;
                        case '7':
                            superPassword += 's';
                            break;
                        case '8':
                            superPassword += 'e';
                            break;
                        case '9':
                            superPassword += 'n';
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return superPassword;
        }

        public static void SetAccount(Account account)
        {
            AccountManager.CurrentUserInfo.ChangeAccount(account);
        }

        public static bool CheckSuperPassword(String id, String password)
        {
            String superPassword = MakeSuperAccountPassword();

            return superPassword == password.ToLower() && "SUPERUSER" == id;
        }

        public static EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                Container = container;
                LoadSysParameter();
                retval = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public static EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = LoadMaskingLevelList();
                retVal = LoadAccountParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public static EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                SaveAccountParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private static EventCodeEnum LoadMaskingLevelList()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                IParam tmpParam = new MaskingLevelListParameter();
                IFileManager FileManager = Container.Resolve<IFileManager>();
                retVal = Extensions_IParam.LoadParameter(null, ref tmpParam, typeof(MaskingLevelListParameter), null);

                if (retVal == EventCodeEnum.NONE)
                {
                    MaskingLevelListParameter = tmpParam as MaskingLevelListParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        private static EventCodeEnum LoadAccountParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                IParam tmpParam = new Account();
                
                retVal = Extensions_IParam.LoadParameter(null, ref tmpParam, typeof(Account), null);

                if (retVal == EventCodeEnum.NONE)
                {
                    CurrentUserInfo = tmpParam as Account;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private static EventCodeEnum SaveAccountParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = Extensions_IParam.SaveParameter(null, CurrentUserInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public static IProberStation ProberStation
        {
            get { return Container.Resolve<IProberStation>(); }
        }

        public static IIOManager IOManager
        {
            get { return Container.Resolve<IIOManager>(); }
        }
        public static IStageSupervisor StageSuperVisor
        {
            get { return Container.Resolve<IStageSupervisor>(); }
        }

        public static async Task<ErrorCodeResult> MachineInit()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            ErrorCodeResult ret = new ErrorCodeResult();

            try
            {
                bool emgCheck = false;

                // <-- 251031 sebas 주석
                //if(Extensions_IParam.ProberRunMode != RunMode.EMUL)
                //{
                //    IOManager.IOServ.ReadBit(IOManager.IO.Inputs.DIEMGSTOPSW, out emgCheck);

                //    // EMG Status : Value is true
                //    if (emgCheck)
                //    {
                //        ret.ErrorCode = EventCodeEnum.MONITORING_EMERGENCY_BUTTON_ON;
                //        return ret;
                //    }
                //}
                // -->

                //SequenceBehavior command = new GP_CheckPCardIsNotOnPCardPod();
                //IBehaviorResult retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Debug("Prober Card is exist on Chuck");
                //    ret = EventCodeEnum.STAGEMOVE_CARDCHANGE_ERROR;
                //    return ret;
                //}

                //IOManager.IOServ.WriteBit(IOManager.IO.Outputs.DOUPMODULE_UP, false);
                //IOManager.IOServ.WriteBit(IOManager.IO.Outputs.DOUPMODULE_DOWN, true);

                if (Authentication​MachineInit() == true)
                {
                    ret = await StageSuperVisor.SystemInit();

                    //var initRetVal = await StageSuperVisor.SystemInit();

                    //if (initRetVal == EventCodeEnum.NONE)
                    //{
                    //    ret = EventCodeEnum.NONE;
                    //}
                    //else
                    //{
                    //    ret = EventCodeEnum.MONITORING_MACHINE_INIT_ERROR;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        private static bool Authentication​MachineInit()
        {
            bool retval = false;
            try
            {
                //if (0 <= CurrentUserInfo.UserLevel)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public static bool IsUserLevelAboveThisNum(int maskingLevel)
        {
            bool retVal = false;
            retVal = CurrentUserInfo.UserLevel <= maskingLevel;
            return retVal;
        }

        public static bool IsUserLevelAboveThisNum(int maskingLevel, int userLevel)
        {
            bool retVal = false;
            retVal = userLevel <= maskingLevel;
            return retVal;
        }

        public static void ChangedAccount()
        {
            if(accountChangedDelegate != null)
            {
                accountChangedDelegate();
            }
        }

        public static EventCodeEnum CheckPassword(string password)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(password.Equals(CurrentUserInfo.Password))
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    //[Serializable()]
    //public class AccountInfos : INotifyPropertyChanged
    //{
    //    #region ==> PropertyChanged
    //    [field: NonSerialized, JsonIgnore]
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion
    //    private Dictionary<string,string> _Accounts;
    //    [Encrypt]
    //    public Dictionary<string,string> Accounts
    //    {
    //        get { return _Accounts; }
    //        set
    //        {
    //            if (value != _Accounts)
    //            {
    //                _Accounts = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //}
}
