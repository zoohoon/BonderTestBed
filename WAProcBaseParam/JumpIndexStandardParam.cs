using System;

namespace WAProcBaseParam
{
    using LogModule;
    using System.ComponentModel;
    [Serializable]
    public class JumpIndexStandardParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private long _LeftIndex;
        public long LeftIndex
        {
            get { return _LeftIndex; }
            set
            {
                if (value != _LeftIndex)
                {
                    _LeftIndex = value;
                    NotifyPropertyChanged("LeftIndex");
                }
            }
        }

        private long _RightIndex;
        public long RightIndex
        {
            get { return _RightIndex; }
            set
            {
                if (value != _RightIndex)
                {
                    _RightIndex = value;
                    NotifyPropertyChanged("RightIndex");
                }
            }
        }


        private long _UpperIndex;
        public long UpperIndex
        {
            get { return _UpperIndex; }
            set
            {
                if (value != _UpperIndex)
                {
                    _UpperIndex = value;
                    NotifyPropertyChanged("UpperIndex");
                }
            }
        }

        private long _BottomIndex;
        public long BottomIndex
        {
            get { return _BottomIndex; }
            set
            {
                if (value != _BottomIndex)
                {
                    _BottomIndex = value;
                    NotifyPropertyChanged("BottomIndex");
                }
            }
        }


        public void CopyTo(JumpIndexStandardParam target)
        {
            try
            {
                target.LeftIndex = this.LeftIndex;
                target.RightIndex = this.RightIndex;
                target.UpperIndex = this.UpperIndex;
                target.BottomIndex = this.BottomIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }
}
