using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace E84SimulatorDialog
{
    
    public class E84SignalTypeWithValue : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private E84SignalTypeEnum _Type;
        public E84SignalTypeEnum Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Value;
        public bool Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        public E84SignalTypeWithValue(E84SignalTypeEnum type, bool value)
        {
            this.Type = type;
            this.Value = value;
        }
    }

    public class E84Input : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private E84SignalInputIndex _Type;
        public E84SignalInputIndex Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CurrentValue;
        public bool CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public E84Input(E84SignalInputIndex type)
        {
            this.Type = type;
        }
    }
    public class E84Output : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private E84SignalOutputIndex _Type;
        public E84SignalOutputIndex Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CurrentValue;
        public bool CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public E84Output(E84SignalOutputIndex type)
        {
            this.Type = type;
        }
    }
}
