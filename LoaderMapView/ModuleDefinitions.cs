using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoaderMapView
{
    //public enum StageStateEnum
    //{
    //    Not_Request,
    //    Requested,
    //}
    public enum LoaderStateEnum
    {
        IDLE,
        PreAlign,
        SubDelivery,
        SubReturn,
        SubExchange,
    }
    //public enum FoupStateEnum
    //{
    //    IDLE,
    //    SubReady,
    //    Processing,
    //    Processed
    //}
    public enum PAStateEnum
    {
        IDLE,
        Busy,
        Reserved,
        PADone,
        CADone,
    }
    public enum WaferStateEnum
    {
        NOT_EXIST,
        Unprocessed,
        Processing,
        Processed,
    }

    public class ModuleInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private DateTime _ConnectedTime;
        public DateTime ConnectedTime
        {
            get { return _ConnectedTime; }
            set
            {
                if (value != _ConnectedTime)
                {
                    _ConnectedTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DateTime _ProcessingStartTime;
        public DateTime ProcessingStartTime
        {
            get { return _ProcessingStartTime; }
            set
            {
                if (value != _ProcessingStartTime)
                {
                    _ProcessingStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeSpan _UnderProcessingTimeElapsed;
        public TimeSpan UnderProcessingTimeElapsed
        {
            get { return _UnderProcessingTimeElapsed; }
            set
            {
                if (value != _UnderProcessingTimeElapsed)
                {
                    _UnderProcessingTimeElapsed = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _DisplayTime;
        public string DisplayTime
        {
            get { return _DisplayTime; }
            set
            {
                if (value != _DisplayTime)
                {
                    _DisplayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeSpan _TotProcTime;
        public TimeSpan TotProcTime
        {
            get { return _TotProcTime; }
            set
            {
                if (value != _TotProcTime)
                {
                    _TotProcTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ModuleInfo()
        {
            ConnectedTime = DateTime.Now;
            DisplayTime = UnderProcessingTimeElapsed.ToString(@"hh\:mm\:ss\:fff");
        }
    }
}
