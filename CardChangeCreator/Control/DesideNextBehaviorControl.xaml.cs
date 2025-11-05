using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;
using SequenceRunner;
using LogModule;

namespace CardChangeCreator
{
    public partial class DesideNextBehaviorControl : UserControl, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propName)
        {
            if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        private ObservableCollection<SequenceBehavior> _OrderCardChangeCollection;
        public ObservableCollection<SequenceBehavior> OrderCardChangeCollection
        {
            get { return _OrderCardChangeCollection; }
            set
            {
                _OrderCardChangeCollection = value;
                NotifyPropertyChanged(nameof(OrderCardChangeCollection));
            }
        }

        public DesideNextBehaviorControl()
        {
            try
            {
                InitializeComponent();

                this.DataContext = this;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

    }
    public class BehaviorIdMaker : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object retVal = null;
            try
            {
                if (values != null)
                {
                    if (2 == values.Length)
                    {
                        if (values[1] != null)
                        {
                            ObservableCollection<SequenceBehavior> behaviorCollection = values[1] as ObservableCollection<SequenceBehavior>;
                            if (behaviorCollection != null)
                            {
                                retVal = behaviorCollection.FirstOrDefault(i => i.BehaviorID == values[0]?.ToString());
                            }
                        }
                        else
                            retVal = null;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            SequenceBehavior backBehavior = value as SequenceBehavior;
            object[] retObjects = null;
            try
            {
                DataGrid fe = parameter as DataGrid;

                if (backBehavior != null)
                {
                    retObjects = new object[] { backBehavior?.BehaviorID, fe.ItemsSource };
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return retObjects;
        }
    }
}
