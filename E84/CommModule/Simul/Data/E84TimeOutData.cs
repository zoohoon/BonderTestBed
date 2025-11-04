using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace E84
{
    public class E84TimeOutData : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _TimeOut;
        public int TimeOut
        {
            get { return _TimeOut; }
            set
            {
                _TimeOut = value;
                RaisePropertyChanged();
            }
        }

        private int _TimeOutInterval;
        public int TimeOutInterval
        {
            get { return _TimeOutInterval; }
            set
            {
                _TimeOutInterval = value;
                RaisePropertyChanged();
            }
        }

        private string _TargetSignalName;
        public string TargetSignalName
        {
            get { return _TargetSignalName; }
            set
            {
                _TargetSignalName = value;
                RaisePropertyChanged();
            }
        }

        private bool _TargetState;
        public bool TargetState
        {
            get { return _TargetState; }
            set
            {
                _TargetState = value;
                RaisePropertyChanged();
            }
        }
    }
}
