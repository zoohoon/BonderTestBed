namespace ProberInterfaces
{
    using LogModule;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class E84CassetteLockParam : INotifyPropertyChanged
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Property </remarks>

        private bool _PortFullLockMode;
        /// <summary>
        /// Port Lock Mode 를 전체 동일하게 할것인지, 개별로 설정할 것인지에 대한 설정. 
        /// true : full , false : each
        /// </summary>
        public bool PortFullLockMode
        {
            get { return _PortFullLockMode; }
            set
            {
                if (value != _PortFullLockMode)
                {
                    _PortFullLockMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AutoSetCassetteLockEnable;
        /// <summary>
        /// E84 mode 에 따라 CassetteLock 에 대한 설정 기능을 사용할지 안할지 
        /// </summary>
        public bool AutoSetCassetteLockEnable
        {
            get { return _AutoSetCassetteLockEnable; }
            set
            {
                if (value != _AutoSetCassetteLockEnable)
                {
                    _AutoSetCassetteLockEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84PortLockOptionParam> _E84PortEachLockOptions
             = new ObservableCollection<E84PortLockOptionParam>();
        public ObservableCollection<E84PortLockOptionParam> E84PortEachLockOptions
        {
            get { return _E84PortEachLockOptions; }
            set
            {
                if (value != _E84PortEachLockOptions)
                {
                    _E84PortEachLockOptions = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteLockModeEnum _CassetteLockE84ManualMode
             = CassetteLockModeEnum.NORMAL;
        public CassetteLockModeEnum CassetteLockE84ManualMode
        {
            get { return _CassetteLockE84ManualMode; }
            set
            {
                if (value != _CassetteLockE84ManualMode)
                {
                    _CassetteLockE84ManualMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteLockModeEnum _CassetteLockE84AutoMode
             = CassetteLockModeEnum.NORMAL;
        public CassetteLockModeEnum CassetteLockE84AutoMode
        {
            get { return _CassetteLockE84AutoMode; }
            set
            {
                if (value != _CassetteLockE84AutoMode)
                {
                    _CassetteLockE84AutoMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public E84CassetteLockParam()
        {

        }

        public void CopyFrom(E84CassetteLockParam sourceparam)
        {
            try
            {
                this.PortFullLockMode = sourceparam.PortFullLockMode;
                this.AutoSetCassetteLockEnable = sourceparam.AutoSetCassetteLockEnable;
                this.CassetteLockE84ManualMode = sourceparam.CassetteLockE84ManualMode;
                this.CassetteLockE84AutoMode = sourceparam.CassetteLockE84AutoMode;
                foreach (var sourcelockopparam in sourceparam.E84PortEachLockOptions)
                {
                    foreach (var lockopparam in this.E84PortEachLockOptions)
                    {
                        if(sourcelockopparam.FoupNumber == lockopparam.FoupNumber)
                        {
                            lockopparam.CassetteLockE84ManualMode = sourcelockopparam.CassetteLockE84ManualMode;
                            lockopparam.CassetteLockE84AutoMode = sourcelockopparam.CassetteLockE84AutoMode;
                            break;
                        }
                    }
                }
            }
            catch (System.Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class E84PortLockOptionParam : INotifyPropertyChanged
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _FoupNumber;
        public int FoupNumber
        {
            get { return _FoupNumber; }
            set
            {
                if (value != _FoupNumber)
                {
                    _FoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteLockModeEnum _CassetteLockE84ManualMode
            = CassetteLockModeEnum.NORMAL;
        public CassetteLockModeEnum CassetteLockE84ManualMode
        {
            get { return _CassetteLockE84ManualMode; }
            set
            {
                if (value != _CassetteLockE84ManualMode)
                {
                    _CassetteLockE84ManualMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteLockModeEnum _CassetteLockE84AutoMode
             = CassetteLockModeEnum.NORMAL;
        public CassetteLockModeEnum CassetteLockE84AutoMode
        {
            get { return _CassetteLockE84AutoMode; }
            set
            {
                if (value != _CassetteLockE84AutoMode)
                {
                    _CassetteLockE84AutoMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public E84PortLockOptionParam()
        {

        }
        public E84PortLockOptionParam(int foupnum)
        {
            FoupNumber = foupnum;
    }

        public E84PortLockOptionParam(int foupnum, CassetteLockModeEnum manualmode, CassetteLockModeEnum automode)
        {
            FoupNumber = foupnum;
            CassetteLockE84ManualMode = manualmode;
            CassetteLockE84AutoMode = automode;
        }
    }

    public enum CassetteLockModeEnum
    {
        UNDEFINED,
        NORMAL, // Cassette Load 시에 lock 하고 Load 한다.
        ATTACH, // Cassette 를 Foup에 올렸을때 Presece 감지되면 lock 을 한다.
        LEFTOHT // OHT 가 떠나면 (E84사용) Lock 을 한다
    }
}
