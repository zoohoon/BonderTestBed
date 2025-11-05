using System;
using System.Collections.Generic;

namespace ProberInterfaces.WaferAlignEX
{
    using System.ComponentModel;
    using System.Windows.Media.Imaging;

    public class ThetaAlignMeasurementTable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private HeightPointEnum _HeightPoint;

        public HeightPointEnum HeightPoint
        {
            get { return _HeightPoint; }
            set { _HeightPoint = value; }
        }


        private IFocusing _FocusingModel;

        public IFocusing FocusingModel
        {
            get { return _FocusingModel; }
            set { _FocusingModel = value; }
        }


        private List<ThetaAlignMeasurementInfo> _MeasurementInfos
            = new List<ThetaAlignMeasurementInfo>();
        public List<ThetaAlignMeasurementInfo> MeasurementInfos
        {
            get { return _MeasurementInfos; }
            set
            {
                if (value != _MeasurementInfos)
                {
                    _MeasurementInfos = value;
                    NotifyPropertyChanged("MeasurementInfos");
                }
            }
        }

    }


    public class ThetaAlignMeasurementInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        

        private WAPatternInfomation _PatternInfo;
        public WAPatternInfomation PatternInfo
        {
            get { return _PatternInfo; }
            set
            {
                if (value != _PatternInfo)
                {
                    _PatternInfo = value;
                    NotifyPropertyChanged("PatternInfo");
                }
            }
        }

        private WriteableBitmap _WrbDispPTImage;
        public WriteableBitmap WrbDispPTImage
        {
            get { return _WrbDispPTImage; }
            set
            {
                if (value != _WrbDispPTImage)
                {
                    _WrbDispPTImage = value;
                    NotifyPropertyChanged("WrbDispPTImage");
                }
            }
        }

        //private MachineIndex _MIndex;
        //public MachineIndex MIndex
        //{
        //    get { return _MIndex; }
        //    set
        //    {
        //        if (value != _MIndex)
        //        {
        //            _MIndex = value;
        //            NotifyPropertyChanged("MIndex");
        //        }
        //    }
        //}

        public ThetaAlignMeasurementInfo(WAPatternInfomation ptinfo)
        {
            PatternInfo = ptinfo;
        }
    }

}
