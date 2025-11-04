using LogModule;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProberInterfaces.Param
{
    public class PinLowKeyParameter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        private PinLowKeyTypeEnum _KeyType;
        public PinLowKeyTypeEnum KeyType
        {
            get { return _KeyType; }
            set
            {
                if (value != _KeyType)
                {
                    _KeyType = value;
                    NotifyPropertyChanged("KeyType");
                }
            }
        }
        private CatCoordinates _Position = new CatCoordinates();
        public CatCoordinates Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }
        private ObservableCollection<PinAlignPatternInfo> _PMInfos = new ObservableCollection<PinAlignPatternInfo>();
        public ObservableCollection<PinAlignPatternInfo> PMInfos
        {
            get { return _PMInfos; }
            set
            {
                if (value != _PMInfos)
                {
                    _PMInfos = value;
                    NotifyPropertyChanged("PMInfos");
                }
            }
        }

        private PinCoordinate _ResultPos = new PinCoordinate();
        public PinCoordinate ResultPos
        {
            get { return _ResultPos; }
            set
            {
                if (value != _ResultPos)
                {
                    _ResultPos = value;
                    NotifyPropertyChanged("ResultPos");
                }
            }
        }
        public PinLowKeyParameter()
        {
            //SetDefalut();
        }
        public void SetDefalut()
        {
            try
            {
            KeyType = PinLowKeyTypeEnum.EMPTY;
            Position = new CatCoordinates();
            PMInfos = new ObservableCollection<PinAlignPatternInfo>();
            PMInfos.Add(new PinAlignPatternInfo());
            ResultPos = new PinCoordinate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
    public enum PinLowKeyTypeEnum
    {
        EMPTY = 0,
        REGISTED,
        ALIGNED
    }    

}
