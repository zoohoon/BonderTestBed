using LogModule;
using System;

namespace ProberInterfaces.WaferAlign
{
    using System.ComponentModel;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;

    public interface IHeightMapping
    {
        ObservableCollection<WaferCoordinate> PlanPoints { get; }
        ObservableCollection<WaferCoordinate> HeightPlanPoints { get; }
    }

    [Serializable]
    public class WaferHeightMapping : IHeightMapping, INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<WaferCoordinate> _PlanPoints
            = new ObservableCollection<WaferCoordinate>();
        public ObservableCollection<WaferCoordinate> PlanPoints
        {
            get { return _PlanPoints; }
            set
            {
                if (value != _PlanPoints)
                {
                    _PlanPoints = value;
                    NotifyPropertyChanged("PlanPoints");
                }
            }
        }

        private ObservableCollection<WaferCoordinate> _HeightPlanPoints
            = new ObservableCollection<WaferCoordinate>();
        public ObservableCollection<WaferCoordinate> HeightPlanPoints
        {
            get { return _HeightPlanPoints; }
            set
            {
                if (value != _HeightPlanPoints)
                {
                    _HeightPlanPoints = value;
                    NotifyPropertyChanged("HeightPlanPoints");
                }
            }
        }



        public void DefaultSetting()
        {
            try
            {
            _PlanPoints = new ObservableCollection<WaferCoordinate>();
            _HeightPlanPoints = new ObservableCollection<WaferCoordinate>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
