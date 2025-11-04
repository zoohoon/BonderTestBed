using ProberErrorCode;

namespace ProberInterfaces.Align
{
    using System;
    using System.ComponentModel;

    public interface IAlignSetup 
    {
        AlignSetupStateBase SetupState { get; set; }
        EventCodeEnum Apply();
    }

    public abstract class AlignSetupBase : IAlignSetup, INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private AlignSetupStateBase _SetupState;
        public AlignSetupStateBase SetupState
        {
            get { return _SetupState; }
            set
            {
                if (value != _SetupState)
                {
                    _SetupState = value;
                    NotifyPropertyChanged("SetupState");
                }
            }
        }


        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    NotifyPropertyChanged("DisplayPort");
                }
            }
        }
        private int  _ProcessIndex;
        public int  ProcessIndex
        {
            get { return _ProcessIndex; }
            set
            {
                if (value != _ProcessIndex)
                {
                    _ProcessIndex = value;
                    NotifyPropertyChanged("ProcessIndex");
                }
            }
        }

        public abstract void InitSetup();
        public abstract EventCodeEnum Modify();
        public abstract EventCodeEnum Apply();
    }
}
