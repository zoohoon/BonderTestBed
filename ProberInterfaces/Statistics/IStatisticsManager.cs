using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Model.ChartSeries;

namespace ProberInterfaces
{
    public interface IStatisticsModule
    {

    }

    public abstract class StatisticsInfoProviderBase : IStatisticsInfoProvider, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PoVEnum _PointOfView;
        public PoVEnum PointOfView
        {
            get { return _PointOfView; }
            set
            {
                if (value != _PointOfView)
                {
                    _PointOfView = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class TemprorayDetailInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Installed;
        public string Installed
        {
            get { return _Installed; }
            set
            {
                if (value != _Installed)
                {
                    _Installed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgWorkingTime_IN;
        public double AvgWorkingTime_IN
        {
            get { return _AvgWorkingTime_IN; }
            set
            {
                if (value != _AvgWorkingTime_IN)
                {
                    _AvgWorkingTime_IN = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AvgWorkingTime_OUT;
        public double AvgWorkingTime_OUT
        {
            get { return _AvgWorkingTime_OUT; }
            set
            {
                if (value != _AvgWorkingTime_OUT)
                {
                    _AvgWorkingTime_OUT = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    public class CylinderStatisticsInfoProvider : StatisticsInfoProviderBase
    {
        private double _ExtandThreshold;
        public double ExtandThreshold
        {
            get { return _ExtandThreshold; }
            set
            {
                if (value != _ExtandThreshold)
                {
                    _ExtandThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RetractThreshold;
        public double RetractThreshold
        {
            get { return _RetractThreshold; }
            set
            {
                if (value != _RetractThreshold)
                {
                    _RetractThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TemprorayDetailInfo _DetailInfo;
        public TemprorayDetailInfo DetailInfo
        {
            get { return _DetailInfo; }
            set
            {
                if (value != _DetailInfo)
                {
                    _DetailInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CurrentCount;
        public long CurrentCount
        {
            get { return _CurrentCount; }
            set
            {
                if (value != _CurrentCount)
                {
                    _CurrentCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _LifeTime;
        public long LifeTime
        {
            get { return _LifeTime; }
            set
            {
                if (value != _LifeTime)
                {
                    _LifeTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Percentage;
        public double Percentage
        {
            get { return _Percentage; }
            set
            {
                if (value != _Percentage)
                {
                    _Percentage = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ICylinderType _Cylinder;
        public ICylinderType Cylinder
        {
            get { return _Cylinder; }
            set
            {
                if (value != _Cylinder)
                {
                    _Cylinder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IRenderableSeriesViewModel _ExtandChartRenderableSeries;
        public IRenderableSeriesViewModel ExtandChartRenderableSeries
        {
            get { return _ExtandChartRenderableSeries; }
            set
            {
                if (value != _ExtandChartRenderableSeries)
                {
                    _ExtandChartRenderableSeries = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IRenderableSeriesViewModel _RetractChartRenderableSeries;
        public IRenderableSeriesViewModel RetractChartRenderableSeries
        {
            get { return _RetractChartRenderableSeries; }
            set
            {
                if (value != _RetractChartRenderableSeries)
                {
                    _RetractChartRenderableSeries = value;
                    RaisePropertyChanged();
                }
            }
        }

        private XyDataSeries<int, double> _ExtandDataSeries;
        public XyDataSeries<int, double> ExtandDataSeries
        {
            get { return _ExtandDataSeries; }
            set
            {
                if (value != _ExtandDataSeries)
                {
                    _ExtandDataSeries = value;
                    RaisePropertyChanged();
                }
            }
        }

        private XyDataSeries<int, double> _RetractDataSeries;
        public XyDataSeries<int, double> RetractDataSeries
        {
            get { return _RetractDataSeries; }
            set
            {
                if (value != _RetractDataSeries)
                {
                    _RetractDataSeries = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ImgPath;
        public string ImgPath
        {
            get { return _ImgPath; }
            set
            {
                if (value != _ImgPath)
                {
                    _ImgPath = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public abstract class StatisticsInfoBase : IStatisticsInfo
    {

    }

    public class CylinderStatisticsInfo : StatisticsInfoBase
    {

    }

    public interface IStatisticsInfo
    {

    }

    public interface IStatisticsInfoProvider
    {

    }

    public interface IStatisticsElement : INotifyPropertyChanged
    {
        bool IsSelected { get; set; }
    }
    
    public interface IStatisticsManager : IFactoryModule, IModule
    {
        ObservableCollection<IStatisticsElement> StatisticsElements { get; }
    }

    public enum PoVEnum
    {
        QUATER_VIEW,
        FOUP_FV,
        FOUP_BV,
        PA_VIEW,
        DP_VIEW,
    }

}
