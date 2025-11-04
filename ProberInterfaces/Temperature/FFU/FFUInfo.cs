namespace ProberInterfaces.Temperature.FFU
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class FFUInfo : INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private string _SerialNumber;
        public string SerialNumber
        {
            get { return _SerialNumber; }
            set
            {
                if (_SerialNumber != value)
                {
                    _SerialNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        //false : can use FFU
        //true : can not use FFU.
        private bool _UseLimitFFU;
        public bool UseLimitFFU
        {
            get { return _UseLimitFFU; }
            set
            {
                if (value != _UseLimitFFU)
                {
                    _UseLimitFFU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurSpeed;
        public int CurSpeed
        {
            get { return _CurSpeed; }
            set
            {
                if (value != _CurSpeed)
                {
                    _CurSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SetSpeed;
        public int SetSpeed
        {
            get { return _SetSpeed; }
            set
            {
                if (value != _SetSpeed)
                {
                    _SetSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _Alarm;
        public long Alarm
        {
            get { return _Alarm; }
            set
            {
                if (value != _Alarm)
                {
                    _Alarm = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _STS;
        public int STS
        {
            get { return _STS; }
            set
            {
                if (value != _STS)
                {
                    _STS = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IOCheck = true;
        public bool IOCheck
        {
            get { return _IOCheck; }
            set
            {
                if (value != _IOCheck)
                {
                    _IOCheck = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
