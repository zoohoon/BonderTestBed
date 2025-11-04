using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ProberViewModel
{
    using ProberInterfaces;
    using System.ComponentModel;
    using LogModule;
    using System.Globalization;
    using System.Windows.Controls.Primitives;
    using System.Windows.Interactivity;
    using ProberInterfaces.PMI;
    using System.Collections.ObjectModel;
    using ProberInterfaces.ControlClass.ViewModel.PMI;

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PMIViewerPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("90cc9901-72d7-451e-94a4-daf3aa6931ea");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public PMIViewerPage()
        {
            InitializeComponent();
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        //private void OnScrollUp(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var scrollViwer = GetScrollViewer(uiListView) as ScrollViewer;

        //        if (scrollViwer != null)
        //        {
        //            if (uiListView.SelectedIndex > 0)
        //            {
        //                uiListView.SelectedIndex--;
        //                uiListView.ScrollIntoView(uiListView.SelectedItem);
        //            }

        //            //if (scrollViwer.VerticalOffset > 0)
        //            //{
        //            //    uiListView.SelectedIndex--;
        //            //    scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset - 1);
        //            //}
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private void OnScrollDown(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var scrollViwer = GetScrollViewer(uiListView) as ScrollViewer;

        //        if (scrollViwer != null)
        //        {
        //            if (uiListView.SelectedIndex < uiListView.Items.Count - 1)
        //            {
        //                uiListView.SelectedIndex++;
        //                uiListView.ScrollIntoView(uiListView.SelectedItem);
        //            }

        //            //if (scrollViwer.VerticalOffset < uiListView.Items.Count - 1)
        //            //{
        //            //    uiListView.SelectedIndex++;
        //            //    scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + 1);
        //            //}
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private void OnScrollWaferlistUp(object sender, RoutedEventArgs e)
        {
            try
            {
                var scrollViwer = GetScrollViewer(uiWaferListView) as ScrollViewer;

                if (scrollViwer != null)
                {
                    // Logical Scrolling by Item
                    // scrollViwer.LineUp();
                    // Physical Scrolling by Offset

                    //scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset - 1);

                    if (uiWaferListView.SelectedIndex > 0)
                    {
                        uiWaferListView.SelectedIndex--;
                        uiWaferListView.ScrollIntoView(uiWaferListView.SelectedItem);

                        //scrollViwer.LineUp();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OnScrollWaferlistDown(object sender, RoutedEventArgs e)
        {
            try
            {
                var scrollViwer = GetScrollViewer(uiWaferListView) as ScrollViewer;

                if (scrollViwer != null)
                {
                    if (uiWaferListView.SelectedIndex < uiWaferListView.Items.Count - 1)
                    {
                        uiWaferListView.SelectedIndex++;
                        uiWaferListView.ScrollIntoView(uiWaferListView.SelectedItem);

                        //scrollViwer.LineUp();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public class TemplateSelector : DataTemplateSelector
        {
            private DataTemplate _MapViewDataTemplate;

            public DataTemplate MapViewDataTemplate
            {
                get { return _MapViewDataTemplate; }
                set { _MapViewDataTemplate = value; }
            }

            private DataTemplate _DisplayViewDataTemplate;

            public DataTemplate DisplayViewDataTemplate
            {
                get { return _DisplayViewDataTemplate; }
                set { _DisplayViewDataTemplate = value; }
            }

            private DataTemplate _RecipeEditDataTemplate;
            public DataTemplate RecipeEditDataTemplate
            {
                get { return _RecipeEditDataTemplate; }
                set { _RecipeEditDataTemplate = value; }
            }

            private DataTemplate _DutViewDataTemplate;

            public DataTemplate DutViewDataTemplate
            {
                get { return _DutViewDataTemplate; }
                set { _DutViewDataTemplate = value; }
            }

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {

                #region Require

                //if (item == null) throw new ArgumentNullException();
                if (container == null) throw new ArgumentNullException();

                #endregion

                // Result
                DataTemplate itemDataTemplate = null;

                try
                {

                    // Base DataTemplate
                    itemDataTemplate = base.SelectTemplate(item, container);
                    if (itemDataTemplate != null) return itemDataTemplate;

                    // Interface DataTemplate
                    FrameworkElement itemContainer = container as FrameworkElement;
                    if (itemContainer == null) return null;

                    //foreach (Type itemInterface in item.GetType().GetInterfaces())
                    //{
                    //    itemDataTemplate = itemContainer.TryFindResource(new DataTemplateKey(itemInterface)) as DataTemplate;
                    //    if (itemDataTemplate != null) break;
                    //}

                    if (item is IWaferObject)
                    {
                        return MapViewDataTemplate;
                    }
                    else if (item is ICamera)
                    {
                        return DisplayViewDataTemplate;
                    }
                    else if (item is IVmRecipeEditorMainPage)
                    {
                        return RecipeEditDataTemplate;
                    }

                    // Return
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                return itemDataTemplate;
            }

        }
    }

    public static class SizeObserver
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(SizeObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(SizeObserver));

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(SizeObserver));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            // WPF 4.0 onwards
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);

            // WPF 3.5 and prior
            ////SetObservedWidth(frameworkElement, frameworkElement.ActualWidth);
            ////SetObservedHeight(frameworkElement, frameworkElement.ActualHeight);
        }
    }

    public class UserIndexToStringConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = "User Index : ";

            try
            {
                if (value != null)
                {
                    UserIndex ui = value as UserIndex;

                    if (ui != null)
                    {
                        retval = retval + $"X{ui.XIndex}, Y{ui.YIndex}";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PadStatusCodeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (value != null)
                {
                    ObservableCollection<PadStatusCodeEnum> padstatuslist = value as ObservableCollection<PadStatusCodeEnum>;

                    if (padstatuslist.Count > 0)
                    {
                        if (padstatuslist.Count == 1)
                        {
                            if (padstatuslist[0] == PadStatusCodeEnum.PASS)
                            {
                                retval = "PASS";
                            }
                            else
                            {
                                retval = "FAIL";
                            }
                        }
                    }
                    else
                    {
                        retval = "Undefined";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PadStatusToForegroundConverter : IValueConverter
    {
        static SolidColorBrush PassBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush FailBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                if (value != null)
                {
                    ObservableCollection<PadStatusCodeEnum> padstatuslist = value as ObservableCollection<PadStatusCodeEnum>;

                    if (padstatuslist.Count > 0)
                    {
                        if (padstatuslist.Count == 1)
                        {
                            if (padstatuslist[0] == PadStatusCodeEnum.PASS)
                            {
                                retval = PassBrush;
                            }
                            else
                            {
                                retval = FailBrush;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class MarkStatusToBackgroundConverter : IValueConverter
    //{
    //    public MarkStatusCodeEnum? MatchedStatusCode { get; set; }

    //    static SolidColorBrush PassBrush = new SolidColorBrush(Colors.LimeGreen);
    //    static SolidColorBrush FailBrush = new SolidColorBrush(Colors.Red);
    //    static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.Gray);

    //    public MarkStatusToBackgroundConverter()
    //    {

    //    }

    //    public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush retval = UnknownBrush;

    //        try
    //        {
    //            ObservableCollection<MarkStatusCodeEnum> markstatuslist = value as ObservableCollection<MarkStatusCodeEnum>;

    //            if (markstatuslist.Count > 0)
    //            {
    //                if (MatchedStatusCode != null)
    //                {
    //                    bool IsAny = markstatuslist.Any(x => x == (MarkStatusCodeEnum)MatchedStatusCode);

    //                    if (IsAny == true)
    //                    {
    //                        if (MatchedStatusCode == MarkStatusCodeEnum.PASS)
    //                        {
    //                            retval = PassBrush;
    //                        }
    //                        else
    //                        {
    //                            retval = FailBrush;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class MarkStatusToBackgroundMultiConverter : IMultiValueConverter
    {
        public MarkStatusCodeEnum? MatchedStatusCode { get; set; }

        static SolidColorBrush PassBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush FailBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                if (values != null)
                {
                    if (values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue && values[2] != DependencyProperty.UnsetValue)
                    {
                        ObservableCollection<MarkStatusCodeEnum> markstatuslist = values[0] as ObservableCollection<MarkStatusCodeEnum>;

                        if (markstatuslist.Count > 0)
                        {
                            if (MatchedStatusCode != null)
                            {
                                bool IsAny = markstatuslist.Any(x => x == (MarkStatusCodeEnum)MatchedStatusCode);

                                if (IsAny == true)
                                {
                                    if (MatchedStatusCode == MarkStatusCodeEnum.PASS)
                                    {
                                        retval = PassBrush;
                                    }
                                    else
                                    {
                                        retval = FailBrush;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PadStatusToBackgroundMultiConverter : IMultiValueConverter
    {
        public PadStatusCodeEnum? MatchedStatusCode { get; set; }

        static SolidColorBrush PassBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush FailBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                if (values != null)
                {
                    if (values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue)
                    {
                        ObservableCollection<PadStatusCodeEnum> padstatuslist = values[0] as ObservableCollection<PadStatusCodeEnum>;

                        if (padstatuslist.Count > 0)
                        {
                            if (MatchedStatusCode != null)
                            {
                                bool IsAny = padstatuslist.Any(x => x == (PadStatusCodeEnum)MatchedStatusCode);

                                if (IsAny == true)
                                {
                                    retval = FailBrush;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PMIUserIndexToStringConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                UserIndex InputValue = value as UserIndex;

                retval = InputValue.XIndex.ToString() + ", " + InputValue.YIndex.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            int index = -1;

            try
            {
                ListViewItem item = (ListViewItem)value;
                ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

                index = listView.ItemContainerGenerator.IndexFromContainer(item);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PMIImageInfoToStringConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                PMIImageInformation inputval = value as PMIImageInformation;

                if (inputval != null)
                {
                    retval = $"Date and time : {inputval.DateAndTime.ToString()}" + " | " + $"Wafer ID : {inputval.WaferID}" + " | " + $"User Index : X{inputval.UI.XIndex}, Y{inputval.UI.YIndex}" + " | " + $"Dut#{inputval.DutNumber}";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class ScrollToTopBehavior
    {
        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached
            (
                "ScrollToTop",
                typeof(bool),
                typeof(ScrollToTopBehavior),
                new UIPropertyMetadata(false, OnScrollToTopPropertyChanged)
            );
        public static bool GetScrollToTop(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToTopProperty);
        }
        public static void SetScrollToTop(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToTopProperty, value);
        }
        private static void OnScrollToTopPropertyChanged(DependencyObject dpo,
                                                         DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = dpo as ItemsControl;
            if (itemsControl != null)
            {
                DependencyPropertyDescriptor dependencyPropertyDescriptor =
                        DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
                if (dependencyPropertyDescriptor != null)
                {
                    if ((bool)e.NewValue == true)
                    {
                        dependencyPropertyDescriptor.AddValueChanged(itemsControl, ItemsSourceChanged);
                    }
                    else
                    {
                        dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
                    }
                }
            }
        }
        static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl itemsControl = sender as ItemsControl;
            EventHandler eventHandler = null;
            eventHandler = new EventHandler(delegate
            {
                if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(itemsControl) as ScrollViewer;
                    scrollViewer.ScrollToTop();
                    itemsControl.ItemContainerGenerator.StatusChanged -= eventHandler;
                }
            });
            itemsControl.ItemContainerGenerator.StatusChanged += eventHandler;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }

    public class ScrollIntoViewForListBox : Behavior<ListBox>
    {
        /// <summary>
        ///  When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        /// On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_SelectionChanged(object sender,
                                               SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox listBox = (sender as ListBox);
                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            listBox.UpdateLayout();
                            if (listBox.SelectedItem !=
                            null)
                                listBox.ScrollIntoView(
                                listBox.SelectedItem);
                        }));
                }
            }
        }
        /// <summary>
        /// When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;

        }
    }

    public class PadIndexToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = "PAD";

            try
            {
                if (values != null)
                {
                    if (values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue)
                    {
                        int PadindexPlusOne = (int)values[0] + 1;

                        retval = retval + "(" + $"{PadindexPlusOne}" + " / " + $"{values[1]}" + ")";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] values = null;
            if (value != null)
                return values = value.ToString().Split(' ');
            return values;
        }
    }

    public class MarkIndexToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = "MARK";

            try
            {
                if (values != null)
                {
                    if (values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue)
                    {
                        int MarkIndexPlusOne = (int)values[0] + 1;

                        retval = retval + "(" + $"{MarkIndexPlusOne}" + " / " + $"{values[1]}" + ")";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] values = null;
            if (value != null)
                return values = value.ToString().Split(' ');
            return values;
        }
    }
}
