using LiveCharts;
using LiveCharts.Defaults;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace E84SimulatorDialog
{
    public class BulkUpdateObservableCollection<T> : ObservableCollection<T>
    {
        private bool isNotificationSuspended = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!isNotificationSuspended)
            {
                base.OnCollectionChanged(e);
            }
        }

        public void SuspendCollectionChangeNotification()
        {
            isNotificationSuspended = true;
        }

        public void ResumeCollectionChangeNotification()
        {
            isNotificationSuspended = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public class E84Chart : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private SignalIOType _Type;
        public SignalIOType Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84SignalTypeEnum _SignalType;
        public E84SignalTypeEnum SignalType
        {
            get { return _SignalType; }
            set
            {
                if (value != _SignalType)
                {
                    _SignalType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SeriesCollection _Series = new SeriesCollection();
        public SeriesCollection Series
        {
            get { return _Series; }
            set
            {
                if (value != _Series)
                {
                    _Series = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _LastValue;
        public double LastValue
        {
            get { return _LastValue; }
            set
            {
                if (value != _LastValue)
                {
                    _LastValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Func<double, string> _Formatter;
        public Func<double, string> Formatter
        {
            get { return _Formatter; }
            set
            {
                if (_Formatter != value)
                {
                    _Formatter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _minDateTimeValue;
        public double MinDateTimeValue
        {
            get { return _minDateTimeValue; }
            set
            {
                if (_minDateTimeValue != value)
                {
                    _minDateTimeValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _maxDateTimeValue;
        public double MaxDateTimeValue
        {
            get { return _maxDateTimeValue; }
            set
            {
                if (_maxDateTimeValue != value)
                {
                    _maxDateTimeValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _IsVisible;
        public Visibility IsVisible
        {
            get { return _IsVisible; }
            set
            {
                if (_IsVisible != value)
                {
                    _IsVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
