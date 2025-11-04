using LogModule;
using ProberInterfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace IOPanelControl
{
    public partial class UcIOPanel : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //private static UcIOPanel IOPanel;
        private object portLock = new object();

        public static readonly DependencyProperty LockIOProperty =
            DependencyProperty.Register("LockIO",
                typeof(bool),
                typeof(UcIOPanel),
                new FrameworkPropertyMetadata(false));
        public bool LockIO
        {
            get { return (bool)this.GetValue(LockIOProperty); }
            set { this.SetValue(LockIOProperty, value); }
        }

        public static readonly DependencyProperty IOPortsProperty =
         DependencyProperty.Register(
             "IOPorts",
             typeof(ObservableCollection<IOPortDescripter<bool>>), 
             typeof(UcIOPanel), 
             new FrameworkPropertyMetadata(new PropertyChangedCallback(PortUpdated)));
        public ObservableCollection<IOPortDescripter<bool>> IOPorts
        {
            get { return (ObservableCollection<IOPortDescripter<bool>>)this.GetValue(IOPortsProperty); }
            set { this.SetValue(IOPortsProperty, value); }
        }
        private static void PortUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcIOPanel ucIOPanel = ((UcIOPanel)d);
            ucIOPanel.SearchMatched();
        }


        public static readonly DependencyProperty FilteredPortsProperty =
         DependencyProperty.Register(
             "FilteredPorts",
             typeof(ObservableCollection<IOPortDescripter<bool>>),
             typeof(UcIOPanel),
             new FrameworkPropertyMetadata(new ObservableCollection<IOPortDescripter<bool>>()));
        public ObservableCollection<IOPortDescripter<bool>> FilteredPorts
        {
            get { return (ObservableCollection<IOPortDescripter<bool>>)this.GetValue(FilteredPortsProperty); }
            set { this.SetValue(FilteredPortsProperty, value); }
        }

        //private ObservableCollection<IOPortDescripter<bool>> _FilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
        //public ObservableCollection<IOPortDescripter<bool>> FilteredPorts
        //{
        //    get { return _FilteredPorts; }
        //    set
        //    {
        //        if (value != _FilteredPorts)
        //        {
        //            _FilteredPorts = value;
        //            NotifyPropertyChanged("FilteredPorts");
        //        }
        //    }
        //}

        public static readonly DependencyProperty SearchKeywordProperty =
            DependencyProperty.Register("SearchKeyword",
                typeof(string),
                typeof(UcIOPanel),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(SearchMatchedCallback)));

        private static void SearchMatchedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcIOPanel ucIOPanel = ((UcIOPanel)d);
            ucIOPanel.SearchMatched();
        }

        public string SearchKeyword
        {
            get
            {
                return (string)this.GetValue(SearchKeywordProperty);
            }
            set { this.SetValue(SearchKeywordProperty, value); }
        }


        public UcIOPanel()
        {
            //UcIOPanel.IOPanel = this;
            InitializeComponent();
        }
        private async void SearchMatched()
        {
            try
            {
                string upper = "";
                string lower = "";
                int keyLength = 0;
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if(SearchKeyword != null)
                    {
                        upper = SearchKeyword.ToUpper();
                        lower = SearchKeyword.ToLower();
                        keyLength = SearchKeyword.Length;
                    }
                });
                if(FilteredPorts == null)
                {
                    FilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                }
                await Task.Run(() =>
                {
                    if (keyLength > 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                lock (portLock)
                                {
                                    if (IOPorts != null)
                                    {
                                        var outs = IOPorts.Where(
                                            t => t.Key.Value.StartsWith(upper) ||
                                            t.Key.Value.StartsWith(lower) ||
                                            t.Key.Value.ToUpper().Contains(upper));
                                        var filtered = new ObservableCollection<IOPortDescripter<bool>>(outs);
                                        FilteredPorts.Clear();
                                        foreach (var item in filtered)
                                        {
                                            FilteredPorts.Add(item);
                                        }
                                    }
                                }
                            });
                    }
                    else
                    {
                        lock (portLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                if (IOPorts != null)
                                {
                                    FilteredPorts.Clear();
                                    foreach (var item in IOPorts)
                                    {
                                        FilteredPorts.Add(item);
                                    }
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        Point startPoint;
        private void ItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);

        }
        private void ItemsControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //ListBoxItem item = null;
            ToggleButton toggleButton = null;
            DataObject dragData;
            //ListBox listBox;
            //IOPortDescripter<bool> port;
            string portKey = "";

            // Is LMB down and did the mouse move far enough to register a drag?
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(startPoint.X - e.GetPosition(null).X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(startPoint.Y - e.GetPosition(null).Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the ListBoxItem object from the object being dragged
                
                if(e.OriginalSource is ToggleButton)
                {
                    toggleButton = (ToggleButton)e.OriginalSource;
                    if(toggleButton != null)
                    {
                        portKey = toggleButton.ToolTip.ToString();
                        dragData = new DataObject("IOPortDescripter", portKey);

                        DragDrop.DoDragDrop(toggleButton, dragData, DragDropEffects.Move);
                    }
                }
                //item = FindParent<ToggleButton>((DependencyObject)e.OriginalSource);
                //if (null != item)
                //{
                //    listBox = sender as ListBox;
                //    port = (IOPortDescripter<bool>)listBox.ItemContainerGenerator.ItemFromContainer(item);
                //    dragData = new DataObject("pages", port);

                //    DragDrop.DoDragDrop(item, dragData, DragDropEffects.Move);
                //}
            }

        }
        // Find parent of 'child' of type 'T'
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            do
            {
                if (child is T)
                    return (T)child;
                child = VisualTreeHelper.GetParent(child);
            } while (child != null);
            return null;
        }

    }
    /// <summary>
	/// Converts a Boolean value into an opposite Boolean value (and back)
	/// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
    [MarkupExtensionReturnType(typeof(BoolToOppositeBoolConverter))]
    public class BoolToOppositeBoolConverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    return !(bool)value;
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    return !(bool)value;
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new BoolToOppositeBoolConverter();
        }

        #endregion
    }
}
