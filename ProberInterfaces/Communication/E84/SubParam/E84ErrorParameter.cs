

namespace ProberInterfaces.E84
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class E84ErrorParameter : INotifyPropertyChanged
    {
        #region <remark> PropertyChanged </remark>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _ErrorNumber;
        public int ErrorNumber
        {
            get { return _ErrorNumber; }
            set
            {
                if (value != _ErrorNumber)
                {
                    _ErrorNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CodeName;
        public string CodeName
        {
            get { return _CodeName; }
            set
            {
                if (value != _CodeName)
                {
                    _CodeName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Discription;
        public string Discription
        {
            get { return _Discription; }
            set
            {
                if (value != _Discription)
                {
                    _Discription = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84EventCode _EventCode;
        public E84EventCode EventCode
        {
            get { return _EventCode; }
            set
            {
                if (value != _EventCode)
                {
                    _EventCode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private E84ErrorActEnum _ErrorAct = E84ErrorActEnum.ERROR_Ho_Off;
        public E84ErrorActEnum ErrorAct
        {
            get { return _ErrorAct; }
            set
            {
                if (value != _ErrorAct)
                {
                    _ErrorAct = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AutoRecoveryEnable = false;// default is false, ymtc only use true.
        public bool AutoRecoveryEnable
        {
            get { return _AutoRecoveryEnable; }
            set
            {
                if (value != _AutoRecoveryEnable)
                {
                    _AutoRecoveryEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public E84ErrorParameter()
        {

        }
        public E84ErrorParameter(int number, string name, string discription, E84EventCode code, E84ErrorActEnum erroract)
        {
            this.ErrorNumber = number;
            this.CodeName = name;
            this.Discription = discription;
            this.EventCode = code;
            this.ErrorAct = erroract;
        }
    }

}
