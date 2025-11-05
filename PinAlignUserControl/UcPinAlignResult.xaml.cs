using LogModule;
using ProberInterfaces;
using ProberInterfaces.PinAlign;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace PinAlignUserControl
{

    /// <summary>
    /// UcPinAlignResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPinAlignResult : MahApps.Metro.Controls.Dialogs.CustomDialog
    {
        public UcPinAlignResult()
        {
            InitializeComponent();
        }

        private void UcDutViewer_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dgCheckItems_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCheckItems.Items.Count > 0)
                {
                    //Make the columns use starsizing so their combined width
                    //can't be bigger than the actual datagrid that contains them.
                    foreach (var column in dgCheckItems.Columns)
                    {
                        if (dgCheckItems.ActualWidth != 0)
                        {
                            var starSize = column.ActualWidth / dgCheckItems.ActualWidth;
                            column.Width = new DataGridLength(starSize, DataGridLengthUnitType.Star);
                        }
                    }

                    DependencyObject dep = dgCheckItems as DependencyObject;

                    while ((dep != null) && !(dep is Grid))
                    {
                        dep = VisualTreeHelper.GetParent(dep);
                    }

                    double GridHeight = (dep as Grid).ActualHeight;

                    if(GridHeight != 0)
                    {
                        var headersPresenter = FindVisualChild<DataGridColumnHeadersPresenter>(dgCheckItems);

                        // 전체 높이에서 Header 제외 후
                        dgCheckItems.RowHeight = (GridHeight - headersPresenter.ActualHeight) / dgCheckItems.Items.Count;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


        }

        private void dgEachPinResultes_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Make the columns use starsizing so their combined width
                //can't be bigger than the actual datagrid that contains them.
                foreach (var column in dgEachPinResultes.Columns)
                {
                    //if (column != null)
                    //{
                    //    column.Width = column.Width.DisplayValue;
                    //    string header = (string)column.Header;

                    //    if (header.Contains("\r\n"))
                    //        return;

                    //    int middle = header.Length / 2;
                    //    int closestToMiddle = -1;
                    //    for (int i = 0; i < header.Length; ++i)
                    //    {
                    //        if (header[i] == ' ')
                    //        {
                    //            if (closestToMiddle == -1)
                    //                closestToMiddle = i;
                    //            else if (Math.Abs(i - middle) < Math.Abs(closestToMiddle - middle))
                    //                closestToMiddle = i;
                    //        }
                    //    }

                    //    if (closestToMiddle != -1)
                    //    {
                    //        StringBuilder newHeader = new StringBuilder(header);
                    //        newHeader.Replace(" ", "\r\n", closestToMiddle, 1);
                    //        column.Header = newHeader.ToString();
                    //    }
                    //}

                    if (dgCheckItems.ActualWidth != 0)
                    {
                        var starSize = column.ActualWidth / dgCheckItems.ActualWidth;
                        column.Width = new DataGridLength(starSize, DataGridLengthUnitType.Star);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static T FindVisualChild<T>(DependencyObject current) where T : DependencyObject
        {
            try
            {
                if (current == null) return null;
                int childrenCount = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is T) return (T)child;
                    T result = FindVisualChild<T>(child);
                    if (result != null) return result;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }

        private void dgCheckItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    // find row for the first selected item
                    //DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]);
                    DataGridRow row = null;

                    if (dataGrid.Items.IndexOf(e.AddedItems[0]) % dataGrid.Items.Count == dataGrid.Items.Count - 1)
                    {
                        row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.Items.Count - 2);

                        if (row != null)
                        {
                            dataGrid.ScrollIntoView(row.Item);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void dgEachPinResultes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    // find row for the first selected item
                    //DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]);
                    DataGridRow row = null;

                    //DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);

                    if (dataGrid.Items.IndexOf(e.AddedItems[0]) % dataGrid.Items.Count == dataGrid.Items.Count - 1)
                    {
                        row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.Items.Count - 2);

                        if (row != null)
                        {
                            dataGrid.ScrollIntoView(row.Item);
                        }
                    }

                    //if (row != null && row.Item != null)
                    //{
                    //    dataGrid.ScrollIntoView(row.Item);
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                // do stuff
            }
        }
    }

    public class PinStatusToBrushConverter : IValueConverter
    {
        //public enum PINALIGNRESULT
        //{
        //    PIN_NOT_PERFORMED = 0,
        //    PIN_PASSED = 1,
        //    PIN_SKIP = 2,
        //    PIN_FORCED_PASS = 3,
        //    PIN_FOCUS_FAILED = 4,
        //    PIN_TIP_FOCUS_FAILED = 5,
        //    PIN_BLOB_FAILED = 6,
        //    PIN_OVER_TOLERANCE = 7,
        //    PIN_BLOB_TOLERANCE = 8
        //}

        private static readonly Brush DEFAULT_BRUSH = new SolidColorBrush(Colors.White);

        private static readonly Brush PIN_NOT_PERFORMED_BRUSH = (SolidColorBrush)(new BrushConverter().ConvertFrom("#AAAAAA"));
        private static readonly Brush PIN_PASSED_BRUSH = new SolidColorBrush(Colors.Lime);
        private static readonly Brush PIN_SKIP_BRUSH = new SolidColorBrush(Colors.Purple);
        private static readonly Brush PIN_FORCED_PASS_BRUSH = new SolidColorBrush(Colors.Lime);
        private static readonly Brush PIN_FOCUS_FAILED_BRUSH = new SolidColorBrush(Colors.Red);
        private static readonly Brush PIN_TIP_FOCUS_FAILED_BRUSH = new SolidColorBrush(Colors.Red);
        private static readonly Brush PIN_BLOB_FAILED_BRUSH = new SolidColorBrush(Colors.Yellow);
        private static readonly Brush PIN_OVER_TOLERANCE_BRUSH = new SolidColorBrush(Colors.Cyan);
        private static readonly Brush PIN_BLOB_TOLERANCE_BRUSH = new SolidColorBrush(Colors.Cyan);


        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            Brush retval = DEFAULT_BRUSH;

            try
            {
                DependencyObject dep = value as DependencyObject;

                //var cell = value as DataGridCell;

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                EachPinResult pinresult = (dep as DataGridRow).Item as EachPinResult;

                if (pinresult != null)
                {
                    PINALIGNRESULT resultenum = pinresult.PinResult;

                    resultenum = pinresult.PinResult;

                    switch (resultenum)
                    {
                        case PINALIGNRESULT.PIN_NOT_PERFORMED:
                            retval = PIN_NOT_PERFORMED_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_PASSED:
                            retval = PIN_PASSED_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_SKIP:
                            retval = PIN_SKIP_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_FORCED_PASS:
                            retval = PIN_FORCED_PASS_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_FOCUS_FAILED:
                            retval = PIN_FOCUS_FAILED_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_TIP_FOCUS_FAILED:
                            retval = PIN_TIP_FOCUS_FAILED_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_BLOB_FAILED:
                            retval = PIN_BLOB_FAILED_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_OVER_TOLERANCE:
                            retval = PIN_OVER_TOLERANCE_BRUSH;
                            break;
                        case PINALIGNRESULT.PIN_BLOB_TOLERANCE:
                            retval = PIN_BLOB_TOLERANCE_BRUSH;
                            break;
                        default:
                            break;
                    }
                }
                else
                {

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        //private static bool IsSupportedType(object value)
        //{
        //    return value is int || value is double || value is byte || value is long ||
        //           value is float || value is uint || value is short || value is sbyte ||
        //           value is ushort || value is ulong || value is decimal;
        //}
    }

    public class CheckStatusToBrushConverter : IValueConverter
    {
        private static readonly Brush DEFAULT_BRUSH = new SolidColorBrush(Colors.White);
        private static readonly Brush PASS_BRUSH = new SolidColorBrush(Colors.Lime);
        private static readonly Brush FAIL_BRUSH = new SolidColorBrush(Colors.Red);


        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            Brush retval = DEFAULT_BRUSH;

            try
            {
                string InputStr = value as string;

                if (InputStr == "PASS")
                {
                    retval = PASS_BRUSH;
                }
                else if (InputStr == "FAIL")
                {
                    retval = FAIL_BRUSH;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MaxRowsDataGrid : DataGrid
    {
        public static readonly DependencyProperty MaxRowsProperty =
            DependencyProperty.Register("MaxRows",
                                        typeof(int),
                                        typeof(MaxRowsDataGrid),
                                        new UIPropertyMetadata(0, MaxRowsPropertyChanged));
        private static void MaxRowsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MaxRowsDataGrid maxRowsDataGrid = source as MaxRowsDataGrid;
            maxRowsDataGrid.SetCanUserAddRowsState();
        }
        public int MaxRows
        {
            get { return (int)GetValue(MaxRowsProperty); }
            set { SetValue(MaxRowsProperty, value); }
        }

        private bool m_changingState;
        public MaxRowsDataGrid()
        {
            m_changingState = false;
        }
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                base.OnItemsChanged(e);
                if (IsInEditMode() == true)
                {
                    EventHandler eventHandler = null;
                    eventHandler += new EventHandler(delegate
                    {
                        if (IsInEditMode() == false)
                        {
                            SetCanUserAddRowsState();
                            LayoutUpdated -= eventHandler;
                        }
                    });
                    LayoutUpdated += eventHandler;
                }
                else
                {
                    SetCanUserAddRowsState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool IsInEditMode()
        {
            IEditableCollectionView itemsView = Items;
            if (itemsView.IsAddingNew == false && itemsView.IsEditingItem == false)
            {
                return false;
            }
            return true;
        }
        // This method will raise OnItemsChanged again 
        // because a NewItemPlaceHolder will be added or removed
        // so to avoid infinite recursion a bool flag is added
        private void SetCanUserAddRowsState()
        {
            try
            {
                if (m_changingState == false)
                {
                    m_changingState = true;
                    int maxRows = (CanUserAddRows == true) ? MaxRows : MaxRows - 1;
                    if (Items.Count > maxRows)
                    {
                        CanUserAddRows = false;
                    }
                    else
                    {
                        CanUserAddRows = true;
                    }
                    m_changingState = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
