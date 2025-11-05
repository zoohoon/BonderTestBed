namespace ProberInterfaces.E84
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class E84PinSignalParameter : INotifyPropertyChanged
    {
        #region <remark> PropertyChanged </remark>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private E84SignalTypeEnum _Signal;
        public E84SignalTypeEnum Signal
        {
            get { return _Signal; }
            set
            {
                if (value != _Signal)
                {
                    _Signal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84SignalActiveEnum _ActiveType;
        public E84SignalActiveEnum ActiveType
        {
            get { return _ActiveType; }
            set
            {
                if (value != _ActiveType)
                {
                    _ActiveType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _Pin;
        public int Pin
        {
            get { return _Pin; }
            set
            {
                if (value != _Pin)
                {
                    _Pin = value;
                    RaisePropertyChanged();
                }
            }
        }

        public E84PinSignalParameter(E84SignalTypeEnum signal, int pin, E84SignalActiveEnum type)
        {
            this.Signal = signal;
            this.Pin = pin;
            this.ActiveType = type;
        }
    }
}
