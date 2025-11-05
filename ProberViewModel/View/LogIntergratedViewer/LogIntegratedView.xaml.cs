using LogModule;
using ProberInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace ProberViewModel
{
    public static class DataGridTextSearch
    {
        // Using a DependencyProperty as the backing store for SearchValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchValueProperty =
            DependencyProperty.RegisterAttached("SearchValue", typeof(string), typeof(DataGridTextSearch),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

        public static string GetSearchValue(DependencyObject obj)
        {
            return (string)obj.GetValue(SearchValueProperty);
        }

        public static void SetSearchValue(DependencyObject obj, string value)
        {
            obj.SetValue(SearchValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsTextMatch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextMatchProperty =
            DependencyProperty.RegisterAttached("IsTextMatch", typeof(bool), typeof(DataGridTextSearch), new UIPropertyMetadata(false));

        public static bool GetIsTextMatch(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTextMatchProperty);
        }

        public static void SetIsTextMatch(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTextMatchProperty, value);
        }
    }

    public class SearchValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                //string cellText = values[0] == null ? string.Empty : values[0].ToString();
                var source = values[0];
                string searchText = values[1] as string;

                bool retval = false;
                //string a = cellText.ToString(CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(searchText))
                {
                    if (source is LogDataStructure)
                    {
                        LogDataStructure src = source as LogDataStructure;

                        // Filter

                        // 
                        // 1. Object Type       (O)
                        // 2. DateTime Time     (0)
                        // 3. string Code       (0)
                        // 4. List<string> Tag  (O)
                        // 5. string Message    (0)
                        // 6. string Description(0)

                        if ((src.Type.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                             (src.Time.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                             (src.Code.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                             (src.Message.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                             (src.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                             (src.Tag.Contains(searchText, StringComparer.OrdinalIgnoreCase) == true)
                            )
                        {
                            retval = true;
                        }
                        else
                        {
                            retval = false;
                        }

                        //// Filter
                        //if ((src.Code.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                        //    (src.Time.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) |
                        //    (src.Level.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        //    )
                        //{
                        //    retval = true;
                        //}
                    }
                }
                else
                {
                    retval = true;
                }

                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            //var filtered = from log in source
            //               where log.code.ToUpper().Contains(upper) ||
            //                     log.Description.ToString().ToUpper().Contains(upper) ||
            //                     log.Tag.Contains(upper, StringComparer.OrdinalIgnoreCase)
            //               select log;

            //if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(cellText))
            //{
            //    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(cellText, searchText, CompareOptions.OrdinalIgnoreCase) >= 0)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }

            //    //return cellText.ToLower().StartsWith(searchText.ToLower());
            //    return String.Equals(searchText, cellText, StringComparison.OrdinalIgnoreCase);
            //    //return cellText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            //}

            //if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(cellText))
            //{
            //    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(cellText, searchText, CompareOptions.OrdinalIgnoreCase) >= 0)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }

            //    //return cellText.ToLower().StartsWith(searchText.ToLower());
            //    return String.Equals(searchText, cellText, StringComparison.OrdinalIgnoreCase);
            //    //return cellText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            //}
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class DataGridSelectedItemsBlendBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList<object>),
            typeof(DataGridSelectedItemsBlendBehavior),
            new FrameworkPropertyMetadata(null)
            {
                BindsTwoWayByDefault = true
            });

        public IList<object> SelectedItems
        {
            get
            {
                return (IList<object>)GetValue(SelectedItemProperty);
            }
            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();
                this.AssociatedObject.SelectionChanged += OnSelectionChanged;
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
                base.OnDetaching();
                if (this.AssociatedObject != null)
                    this.AssociatedObject.SelectionChanged -= OnSelectionChanged;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                e.Handled = true;
                //DataGrid g = sender as DataGrid;

                //// checks if no rows where added (to the selected rows)
                //// but at least 1 row removed from the selected rows of the grid
                //if (g != null && e.AddedItems.Count == 0 && e.RemovedItems.Count > 0)
                //{
                //    // this selects the first row in the e.RemovedItems collection
                //    g.SelectedItem = e.RemovedItems[0];
                //}

                if (e.AddedItems != null && e.AddedItems.Count > 0 && this.SelectedItems != null)
                {
                    foreach (object obj in e.AddedItems)
                        this.SelectedItems.Add(obj);
                }

                if (e.RemovedItems != null && e.RemovedItems.Count > 0 && this.SelectedItems != null)
                {
                    foreach (object obj in e.RemovedItems)
                        this.SelectedItems.Remove(obj);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

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
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogIntegratedView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("54cb2778-0226-4a66-8cb4-86594aeb4fc0");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LogIntegratedView()
        {
            InitializeComponent();


            //((INotifyCollectionChanged)DebugListView.Items).CollectionChanged += ListView_CollectionChanged;
        }

        //private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Add)
        //    {
        //        DebugListViewScroll.ScrollToBottom();

        //        //// scroll the new item into view   
        //        ////DebugListView.ScrollIntoView(e.NewItems[0]);

        //        ////var last = DebugListView.Items[DebugListView.Items.Count - 1];
        //        //var last = DebugListView.Items[0];

        //        //Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        //        //{
        //        //    //DebugListView.Items.MoveCurrentTo(last);
        //        //    //DebugListView.ScrollIntoView(last);

        //        //    DebugListView.Items.MoveCurrentToLast();
        //        //    //DebugListView.ScrollIntoView(last);
        //        //}));
        //    }
        //}

        private double? ScrollOffset = 1;

        //private double? ScrollOffsetforListView = null;

        public static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = GetFirstVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }

            return null;
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

        private void OnScrollUp(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var datagrd = (DataGrid)this.FindName((sender as RepeatButton).Tag.ToString());

                    if (datagrd != null)
                    {
                        var scrollViwer = GetScrollViewer(datagrd) as ScrollViewer;

                        if (scrollViwer != null)
                        {
                            // Logical Scrolling by Item
                            // scrollViwer.LineUp();
                            // Physical Scrolling by Offset

                            if (ScrollOffset == null)
                            {
                                DataGridRow row = GetFirstVisualChild<DataGridRow>(datagrd);

                                if (row != null)
                                {
                                    ScrollOffset = row.ActualHeight;
                                }
                            }

                            scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset - (double)ScrollOffset);
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OnScrollDown(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var datagrd = (DataGrid)this.FindName((sender as RepeatButton).Tag.ToString());

                    if (datagrd != null)
                    {
                        var scrollViwer = GetScrollViewer(datagrd) as ScrollViewer;

                        if (scrollViwer != null)
                        {
                            if (ScrollOffset == null)
                            {
                                DataGridRow row = GetFirstVisualChild<DataGridRow>(datagrd);

                                if (row != null)
                                {
                                    ScrollOffset = row.ActualHeight;
                                }
                            }

                            // Logical Scrolling by Item
                            // scrollViwer.LineDown();
                            // Physical Scrolling by Offset
                            scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + (double)ScrollOffset);

                            //var ip = (ItemsPresenter)scrollViwer.Content;

                            //var point = 

                            //scrollViwer.ContentVerticalOffset();
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void OnScrollUp_Debug(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var listv = (ListView)this.FindName((sender as RepeatButton).Tag.ToString());

        //        if (listv != null)
        //        {
        //            var scrollViwer = GetScrollViewer(listv) as ScrollViewer;

        //            if (scrollViwer != null)
        //            {
        //                if (ScrollOffsetforListView == null)
        //                {
        //                    ListViewItem row = GetFirstVisualChild<ListViewItem>(listv);

        //                    if (row != null)
        //                    {
        //                        ScrollOffsetforListView = row.ActualHeight;
        //                    }
        //                }

        //                // Logical Scrolling by Item
        //                // scrollViwer.LineDown();
        //                // Physical Scrolling by Offset
        //                //scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset - (double)ScrollOffset);
        //                DebugListViewScroll.ScrollToVerticalOffset(DebugListViewScroll.VerticalOffset - (double)ScrollOffsetforListView);
        //                DebugListViewScroll.UpdateLayout();

        //                //var ip = (ItemsPresenter)scrollViwer.Content;

        //                //var point = 

        //                //scrollViwer.ContentVerticalOffset();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private void OnScrollDown_Debug(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var listv = (ListView)this.FindName((sender as RepeatButton).Tag.ToString());

        //        if (listv != null)
        //        {
        //            var scrollViwer = GetScrollViewer(listv) as ScrollViewer;

        //            if (scrollViwer != null)
        //            {
        //                if (ScrollOffsetforListView == null)
        //                {
        //                    ListViewItem row = GetFirstVisualChild<ListViewItem>(listv);

        //                    if (row != null)
        //                    {
        //                        ScrollOffsetforListView = row.ActualHeight;
        //                    }
        //                }

        //                // Logical Scrolling by Item
        //                // scrollViwer.LineDown();
        //                // Physical Scrolling by Offset
        //                //scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + (double)ScrollOffset);
        //                DebugListViewScroll.ScrollToVerticalOffset(DebugListViewScroll.VerticalOffset + (double)ScrollOffsetforListView);
        //                DebugListViewScroll.UpdateLayout();
        //                //var ip = (ItemsPresenter)scrollViwer.Content;

        //                //var point = 

        //                //scrollViwer.ContentVerticalOffset();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private void OnScrollUp_FilteredDebug(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var listv = (ListView)this.FindName((sender as RepeatButton).Tag.ToString());

        //        if (listv != null)
        //        {
        //            var scrollViwer = GetScrollViewer(listv) as ScrollViewer;

        //            if (scrollViwer != null)
        //            {
        //                if (ScrollOffsetforListView == null)
        //                {
        //                    ListViewItem row = GetFirstVisualChild<ListViewItem>(listv);

        //                    if (row != null)
        //                    {
        //                        ScrollOffsetforListView = row.ActualHeight;
        //                    }
        //                }

        //                // Logical Scrolling by Item
        //                // scrollViwer.LineDown();
        //                // Physical Scrolling by Offset
        //                //scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + (double)ScrollOffset);
        //                FilteredDebugListViewScroll.ScrollToVerticalOffset(FilteredDebugListViewScroll.VerticalOffset - (double)ScrollOffsetforListView);
        //                FilteredDebugListViewScroll.UpdateLayout();
        //                //var ip = (ItemsPresenter)scrollViwer.Content;

        //                //var point = 

        //                //scrollViwer.ContentVerticalOffset();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private void OnScrollDown_FilteredDebug(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var listv = (ListView)this.FindName((sender as RepeatButton).Tag.ToString());

        //        if (listv != null)
        //        {
        //            var scrollViwer = GetScrollViewer(listv) as ScrollViewer;

        //            if (scrollViwer != null)
        //            {
        //                if (ScrollOffsetforListView == null)
        //                {
        //                    ListViewItem row = GetFirstVisualChild<ListViewItem>(listv);

        //                    if (row != null)
        //                    {
        //                        ScrollOffsetforListView = row.ActualHeight;
        //                    }
        //                }

        //                // Logical Scrolling by Item
        //                // scrollViwer.LineDown();
        //                // Physical Scrolling by Offset
        //                //scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + (double)ScrollOffset);
        //                FilteredDebugListViewScroll.ScrollToVerticalOffset(FilteredDebugListViewScroll.VerticalOffset + (double)ScrollOffsetforListView);
        //                FilteredDebugListViewScroll.UpdateLayout();
        //                //var ip = (ItemsPresenter)scrollViwer.Content;

        //                //var point = 

        //                //scrollViwer.ContentVerticalOffset();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private void DebugListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                e.Handled = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return;
        }

        private void FilteredDebugListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                e.Handled = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return;
        }

        public Type FindAncestor<Type>(DependencyObject dependencyObject)
                            where Type : class
        {
            DependencyObject target = dependencyObject;
            try
            {
                do
                {
                    target = VisualTreeHelper.GetParent(target);
                }
                while (target != null && !(target is Type));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return target as Type;
        }

        public void DoCheckRow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGridCell cell = sender as DataGridCell;

                if (cell != null && !cell.IsEditing)
                {
                    //DataGridRow row = VisualHelpers.TryFindParent<DataGridRow>(cell);
                    DataGridRow row = FindAncestor<DataGridRow>(cell);

                    if (row != null)
                    {
                        //Button button = VisualHelpers.FindVisualChild<Button>(cell, "ViewButton");
                        Button button = GetFirstVisualChild<Button>(cell);

                        if (button != null)
                        {
                            HitTestResult result = VisualTreeHelper.HitTest(button, e.GetPosition(cell));

                            if (result != null)
                            {
                                // execute button and do not select / deselect row
                                button.Command.Execute(row.DataContext);
                                e.Handled = true;
                                return;
                            }
                        }

                        row.IsSelected = !row.IsSelected;
                        e.Handled = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var datagrd = (DataGrid)this.FindName((sender as Button).Tag.ToString());

                var rows = GetDataGridRows(datagrd);

                foreach (DataGridRow row in rows)
                {
                    if (row.IsSelected == true)
                    {
                        row.IsSelected = false;
                    }
                    //DataRowView rowView = (DataRowView)row.Item;

                    //foreach (DataGridColumn column in nameofyordatagrid.Columns)
                    //{
                    //    if (column.GetCellContent(row) is TextBlock)
                    //    {
                    //        TextBlock cellContent = column.GetCellContent(row) as TextBlock;
                    //        MessageBox.Show(cellContent.Text);
                    //    }
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Tabctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded == true)
                {
                    if (e.Source is TabControl)
                    {
                        if (PrologTabItem.IsSelected)
                        {
                            var rows = GetDataGridRows(EventLogDataGrid);

                            foreach (DataGridRow row in rows)
                            {
                                if (row != null && row.IsSelected == true)
                                {
                                    row.IsSelected = false;
                                }
                            }
                        }

                        if (EventlogTabItem.IsSelected)
                        {
                            // If other item's source is selected, chnaged to unselected.

                            if(ProLogDataGrid.CurrentColumn != null & ProLogDataGrid.Items.Count != 0)
                            {
                                IEnumerable<DataGridRow> rows = GetDataGridRows(ProLogDataGrid);
                                if (rows != null | (rows?.Any() ?? false))
                                {
                                    foreach (DataGridRow row in rows) 
                                    {
                                        if (row != null && row.IsSelected == true)
                                        {
                                            row.IsSelected = false;
                                        }
                                    }
                                }
                            }
                            
                        }

                        //if (DebuglogTabItem.IsSelected)
                        //{
                        //    var rows = GetDataGridRows(ProLogDataGrid);

                        //    foreach (DataGridRow row in rows)
                        //    {
                        //        if (row.IsSelected == true)
                        //        {
                        //            row.IsSelected = false;
                        //        }
                        //    }

                        //    rows = GetDataGridRows(EventLogDataGrid);

                        //    foreach (DataGridRow row in rows)
                        //    {
                        //        if (row.IsSelected == true)
                        //        {
                        //            row.IsSelected = false;
                        //        }
                        //    }
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;

                var item = sender as ListViewItem;
                if (item != null && item.IsSelected)
                {
                    //Do your stuff
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            try
            {
                e.Handled = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
