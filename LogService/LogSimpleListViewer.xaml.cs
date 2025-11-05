using LogModule;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace LogService
{
    /// <summary>
    /// Captures and eats MouseWheel events so that a nested ListBox does not
    /// prevent an outer scrollable control from scrolling.
    /// </summary>
    public sealed class IgnoreMouseWheelBehavior : Behavior<UIElement>
    {

        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();
                AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected override void OnDetaching()
        {
            try
            {
                AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
                base.OnDetaching();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {

                e.Handled = true;

                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                e2.RoutedEvent = UIElement.MouseWheelEvent;

                AssociatedObject.RaiseEvent(e2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
    /// LogSimpleListViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogSimpleListViewer : UserControl
    {
        #region ==> DEP Collection

        public static readonly DependencyProperty CollectionProperty =
   DependencyProperty.RegisterAttached(
   nameof(Collection),
   typeof(ObservableCollection<LogEventInfo>),
   typeof(LogSimpleListViewer),
   new UIPropertyMetadata(null, CollectionUpdated));
        public ObservableCollection<LogEventInfo> Collection
        {
            get { return (ObservableCollection<LogEventInfo>)this.GetValue(CollectionProperty); }
            set { this.SetValue(CollectionProperty, value); }
        }

        //public static readonly DependencyProperty CollectionProperty =
        //    DependencyProperty.RegisterAttached(
        //        nameof(Collection),
        //        typeof(SynchronizedObservableCollection<LogEventInfo>),
        //        typeof(LogSimpleListViewer),
        //        new UIPropertyMetadata(null, CollectionUpdated));
        //public SynchronizedObservableCollection<LogEventInfo> Collection
        //{
        //    get { return (SynchronizedObservableCollection<LogEventInfo>)this.GetValue(CollectionProperty); }
        //    set { this.SetValue(CollectionProperty, value); }
        //}

        //public static readonly DependencyProperty CollectionProperty =
        //DependencyProperty.RegisterAttached(
        //    nameof(Collection),
        //    typeof(IList<LogEventInfo>),
        //    typeof(LogSimpleListViewer),
        //    new UIPropertyMetadata(null, CollectionUpdated));
        //public IList<LogEventInfo> Collection
        //{
        //    get { return (IList<LogEventInfo>)this.GetValue(CollectionProperty); }
        //    set { this.SetValue(CollectionProperty, value); }
        //}

        //public static readonly DependencyProperty CollectionProperty =
        //    DependencyProperty.RegisterAttached(
        //    nameof(Collection),
        //    typeof(ICollectionView),
        //    typeof(LogSimpleListViewer),
        //    new UIPropertyMetadata(null, CollectionUpdated));
        //public ICollectionView Collection
        //{
        //    get { return (ICollectionView)this.GetValue(CollectionProperty); }
        //    set { this.SetValue(CollectionProperty, value); }
        //}

        //public static readonly DependencyProperty CollectionProperty =
        //   DependencyProperty.RegisterAttached(
        //   nameof(Collection),
        //   typeof(ICollectionView),
        //   typeof(LogSimpleListViewer),
        //   new UIPropertyMetadata(null, CollectionUpdated));
        //public ICollectionView Collection
        //{
        //    get { return (ICollectionView)this.GetValue(CollectionProperty); }
        //    set { this.SetValue(CollectionProperty, value); }
        //}

        //public static readonly DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached( 
        //                                                            nameof(ViewModel), 
        //                                                            typeof(LogVM), 
        //                                                            typeof(LogSimpleListViewer), 
        //                                                            new UIPropertyMetadata(null, CollectionInVMUpdated));
        //public LogVM ViewModel
        //{
        //    get { return (LogVM)this.GetValue(ViewModelProperty); }
        //    set { this.SetValue(ViewModelProperty, value); }
        //}

        #endregion

        public static LogSimpleListViewer Instance { get; private set; }
        public LogSimpleListViewer()
        {
            try
            {
                InitializeComponent();

                Instance = this;

                //Binding binding = new Binding();
                //binding.Path = new PropertyPath(nameof(Collection));
                //binding.Source = this;
                //logListBox.SetBinding(ListBox.ItemsSourceProperty, binding);

                Binding binding = new Binding();
                binding.Path = new PropertyPath(nameof(Collection));
                binding.Source = this;
                logsimpleview.SetBinding(ListView.ItemsSourceProperty, binding);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //private static void CollectionInVMUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.OldValue != null)
        //    {
        //        var vm = e.NewValue as LogVM;

        //        if (vm != null)
        //        {
        //            var coll = (INotifyCollectionChanged)vm.ProLogHistories;
        //            coll.CollectionChanged -= LogEventInfo_CollectionChanged;
        //        }
        //    }
        //    if (e.NewValue != null)
        //    {
        //        var vm = e.NewValue as LogVM;

        //        if(vm != null)
        //        {
        //            var coll = (INotifyCollectionChanged)vm.ProLogHistories;
        //            coll.CollectionChanged += LogEventInfo_CollectionChanged;
        //        }
        //    }
        //}

        private static void CollectionUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= LogCollectionChanged;
            }
            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += LogCollectionChanged;
            }
        }

        private static void LogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    Instance.logScrollViewer.ScrollToEnd();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private static void LogEventInfo_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        //System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
        //        //{
        //        //    Instance.logListBoxScroll.ScrollToEnd();
        //        //}));
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    //Object lastItem = Instance.logListView.Items[Instance.logListView.Items.Count - 1];
        //    //Instance.logListView.ScrollIntoView(lastItem);

        //    //Instance.logListView.SelectedIndex = Instance.logListView.Items.Count - 1;
        //    //Instance.logListView.ScrollIntoView(Instance.logListView.SelectedItem);
        //}
    }
}
