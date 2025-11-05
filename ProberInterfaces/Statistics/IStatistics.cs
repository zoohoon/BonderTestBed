using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    public interface ICategoryStatistic : INotifyPropertyChanged
    {
        string Category { get; set; }
        float Number { get; set; }
    }
    [Serializable]
    public abstract class CategoryStatisticBase : ICategoryStatistic, INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private string _Category;
        public string Category
        {
            get { return _Category; }
            set
            {
                if (value != _Category)
                {
                    _Category = value;
                    NotifyPropertyChanged("Category");
                }
            }
        }

        private float _Number;
        public float Number
        {
            get { return _Number; }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    NotifyPropertyChanged("Number");
                }
            }
        }
        public CategoryStatisticBase()
        {
            Category = String.Empty;
        }
    }

    public interface ICategoryStats
    {
        string SeriesDisplayName { get; set; }
        string SeriesDescription { get; set; }
        List<CategoryStatisticBase> BinCounts { get; set; }
    }
    [Serializable]
    public abstract class CategoryStatsBase : ICategoryStats
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _SeriesDisplayName;
        public string SeriesDisplayName
        {
            get { return _SeriesDisplayName; }
            set { _SeriesDisplayName = value; }
        }

        private string _SeriesDescription;
        public string SeriesDescription
        {
            get { return _SeriesDescription; }
            set { _SeriesDescription = value; }
        }

        private List<CategoryStatisticBase> _BinCounts = new List<CategoryStatisticBase>();
        public List<CategoryStatisticBase> BinCounts
        {
            get
            {
                return _BinCounts;
            }
            set
            {
                if (_BinCounts != value)
                {
                    _BinCounts = value;
                    NotifyPropertyChanged("BinCounts");
                }
            }
        }

        public CategoryStatsBase()
        {
            try
            {
                _SeriesDisplayName = String.Empty;
                _SeriesDescription = String.Empty;
                //SeriesDescription = "";
                //SeriesDisplayName = "";
                //BinCounts = new List<CategoryStatisticBase>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }
}
