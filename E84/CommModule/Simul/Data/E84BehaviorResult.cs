using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace E84
{
    public class E84BehaviorResult : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _TargetName;
        public string TargetName
        {
            get { return _TargetName; }
            set
            {
                _TargetName = value;
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

        private E84ErrorCode _ErrorCode;
        public E84ErrorCode ErrorCode
        {
            get { return _ErrorCode; }
            set
            {
                _ErrorCode = value;
                RaisePropertyChanged();
            }
        }

        private E84TimeOutData _TimeOutData = new E84TimeOutData();
        public E84TimeOutData TimeOutData
        {
            get { return _TimeOutData; }
            set
            {
                _TimeOutData = value;
                RaisePropertyChanged();
            }
        }

        public E84BehaviorResult(string TargetName, bool TargetState)
        {
            try
            {
                this.TargetName = TargetName;
                this.TargetState = TargetState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
