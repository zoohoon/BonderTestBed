namespace WaferAlign.States
{
    using System;
    using System.ComponentModel;
    using ProberInterfaces.Enum;

    [Serializable()]
    public class WaferAlignPropertyBase : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private int _Priority;
        public int Priority
        {
            get { return _Priority; }
            set
            {
                if (value != _Priority)
                {
                    _Priority = value;
                    NotifyPropertyChanged("Priority");
                }
            }
        }

        public WaferAlignPropertyBase()
        {

        }
    }

    [Serializable()]
    public class PatternProcessingMap
    {
        private int _Index;

        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        private EnumProcessingResult _Result;

        public EnumProcessingResult Result
        {
            get { return _Result; }
            set { _Result = value; }
        }
        public PatternProcessingMap()
        {

        }
    }

}
