using System;
using System.Collections.Generic;

namespace ProberInterfaces
{


    using System.ComponentModel;

    public interface IDutObject
    {
        int Site { get; set; }
        string Loc { get; set; }
        int NumX { get; set; }
        int NumY { get; set; }
        string ID { get; set; }
        string MultiChParam { get; set; }
        List<MachineIndex> Duts { get; set; }
        List<UserIndex> Duts1 { get; set; }
        MachineIndex GetRefOffset(int siteindex);
        List<DutStatistics> DutStats { get; set; }
    }

    public class DutStatistics
    {
        public string SeriesDisplayName { get; set; }

        public string SeriesDescription { get; set; }

        public List<DutBinCount> BinCounts { get; set; }
    }
    public class DutBinCount : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public string Category { get; set; }

        private float _number = 0;
        public float Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    NotifyPropertyChanged("Number");
                }
            }
        }
    }

}
