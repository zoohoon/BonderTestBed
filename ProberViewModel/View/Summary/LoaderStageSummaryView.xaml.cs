using LoaderBase.Communication;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LoaderStageSummaryViewModule
{
    public class UniformGrid : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var itemWidth = availableSize.Width / Columns;
            var actualRows = Math.Ceiling((double)Children.Count / Columns);
            var actualHeight = itemWidth * actualRows;

            return new Size(availableSize.Width, actualHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size cellSize = new Size(finalSize.Width / Columns, finalSize.Width / Columns);
            int row = 0, col = 0;
            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(new Point(cellSize.Width * col, cellSize.Height * row), cellSize));
                if (++col == Columns)
                {
                    row++;
                    col = 0;
                }
            }
            return finalSize;

        }

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register("Columns", typeof(int), typeof(UniformGrid), new PropertyMetadata(1, OnColumnsChanged));


        public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register("Rows", typeof(int), typeof(UniformGrid), new PropertyMetadata(1, OnRowsChanged));

        static void OnColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            int cols = (int)e.NewValue;
            if (cols < 1)
                ((UniformGrid)obj).Columns = 1;
        }

        static void OnRowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            int rows = (int)e.NewValue;
            if (rows < 1)
                ((UniformGrid)obj).Rows = 1;
        }
    }

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderStageSummaryView : UserControl, IMainScreenView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public LoaderStageSummaryView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("21f7b3d0-f1e9-4cd4-95a7-7c33742d5787");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        //private void CellListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    //ListBoxItem lbi = sender as ListBoxItem;

        //    ListView list = sender as ListView;

        //    if (list != null)
        //    {
        //        if(list.SelectedItem != null)
        //        {
        //            IStageObject currentStage = list.SelectedItem as IStageObject;

        //            if(currentStage != null)
        //            {
        //                if(currentStage.StageInfo.IsChecked == true)
        //                {
        //                    currentStage.StageInfo.IsChecked = false;
        //                }
        //            }
        //        }
        //    }

        //    //if (lbi != null)
        //    //{
        //    //    if (lbi.IsSelected)
        //    //    {
        //    //        lbi.IsSelected = false;
        //    //        e.Handled = true;
        //    //    }
        //    //}

        //    HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        //    if (r.VisualHit.GetType() == typeof(Button))
        //    {

        //    }

        //    //    CellListView.UnselectAll();
        //}

        //private void Badged_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    bool OldValue = (bool)e.OldValue;
        //    bool NewValue = (bool)e.NewValue;

        //    if (NewValue == false)
        //    {
        //        Application.Current.Dispatcher.BeginInvoke(
        //            new Action( () =>
        //       {
        //           (sender as Badged).IsEnabled = (bool)e.OldValue;
        //       }));
        //        //(sender as Badged).IsEnabledChanged -= (Badged_IsEnabledChanged);
        //    }
        //}
    }

    public class NullToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value != null && value.ToString() == string.Empty)
            {
                return "Undefined";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class LotIDToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value != null && value.ToString() == string.Empty)
            {
                return "N/A";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class BTNEnableState_DepnedsFoupStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LotStateEnum temp = LotStateEnum.Idle;
           
            if (value is LotStateEnum)
            {
                temp = (LotStateEnum)value;
            }

            if ((temp == LotStateEnum.Done) || (temp == LotStateEnum.Idle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BTNEnableState_DependsFoupStateMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LotStateEnum temp = LotStateEnum.Idle;

            var btnEnabled = false;

            if (value.Count() > 2)
            {
                if(value[0] is LotStateEnum lotstat)
                {
                    btnEnabled = (lotstat == LotStateEnum.Done || lotstat == LotStateEnum.Idle);
                }
                if(value[1] is ProberInterfaces.SecsEnum_ControlState ctrlStat &&
                   value[2] is string recvType)
                {
                    // [STM_CATANIA] GEM Automation - Online Remote 상태인 경우 버튼 비활성화
                    var forcedDisable = recvType.Equals("SemicsGemReceiverSEKS") && ctrlStat == SecsEnum_ControlState.ONLINE_REMOTE;
                    btnEnabled &= !forcedDisable;
                }
            }
            else if(value.Count() > 0)
            {
                if (value[0] is LotStateEnum lotstat)
                {
                    btnEnabled = (lotstat == LotStateEnum.Done || lotstat == LotStateEnum.Idle);
                }
            }
            else { }

            return btnEnabled;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LotSettingsStages_DepnedsFoupStateConverter : IMultiValueConverter
    {

        int CellCol;
        int CellRow;

        public object Convert(object[] values, Type targetType, object parameter,
           System.Globalization.CultureInfo culture)
        {
            LotStateEnum temp = LotStateEnum.Idle;

            if ((values[1] is int) && (values[2] is int))
            {
                CellCol = (int)values[1];
                CellRow = (int)values[1];
            }

            if (values[0] is LotStateEnum)
            {
                temp = (LotStateEnum)values[0];
            }

            if ((temp == LotStateEnum.Done) || (temp == LotStateEnum.Idle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    public class WaferExistToBrushConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            if (value is EnumSubsStatus == null)
            {
                retval = DimGraybrush;
            }
            else
            {
                EnumSubsStatus subsstatus = (EnumSubsStatus)value;

                switch (subsstatus)
                {
                    case EnumSubsStatus.UNKNOWN:
                    case EnumSubsStatus.UNDEFINED:
                    case EnumSubsStatus.NOT_EXIST:
                    case EnumSubsStatus.HIDDEN:

                        retval = DimGraybrush;

                        break;
                    case EnumSubsStatus.EXIST:

                        retval = LimeGreenbrush;

                        break;
                    default:
                        break;
                }
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TabSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            TabControl tabControl = values[0] as TabControl;
            double width = tabControl.ActualWidth / tabControl.Items.Count;
            //Subtract 1, otherwise we could overflow to two rows.
            return (width <= 1) ? 0 : (width - 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DataSourceToLastItemConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<object> items = value as IEnumerable<object>;

            if (items != null)
            {
                return items.LastOrDefault();
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StageviewLotsettingsOrangeConverter : IValueConverter
    {
        static SolidColorBrush NormalBrush = new SolidColorBrush(Colors.DarkOrange);
        static SolidColorBrush GrayBrush = new SolidColorBrush(Colors.DimGray);


        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = GrayBrush;

            bool bexist = (bool)value;

            try
            {
                if (bexist == true)
                {
                    retval = NormalBrush;
                }
                else
                {
                    retval = GrayBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StageviewLotsettingsPuppleConverter : IValueConverter
    {
        static SolidColorBrush NormalBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9370d8"));
        static SolidColorBrush GrayBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = GrayBrush;

            bool bexist = (bool)value;

            try
            {
                if (bexist == true)
                {
                    retval = NormalBrush;
                }
                else
                {
                    retval = GrayBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StageviewLotsettingsRedConverter : IValueConverter
    {
        static SolidColorBrush NormalBrush = new SolidColorBrush(Colors.DarkRed);
        static SolidColorBrush GrayBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = GrayBrush;

            bool bexist = (bool)value;

            try
            {
                if (bexist == true)
                {
                    retval = NormalBrush;
                }
                else
                {
                    retval = GrayBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class AlignStateToForegroundConverter : IValueConverter
    {
        static SolidColorBrush DONEBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush IDLEBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                string inputvalue = value as string;

                switch (inputvalue)
                {
                    case "IDLE":
                        retval = IDLEBrush;
                        break;
                    case "DONE":
                        retval = DONEBrush;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Text_CellSelectedToBrush_AssingedStageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if (value is true)
                {
                    retval = "Assigned";
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

    public class CellSelectedToBrush_AssingedStageConverter : IValueConverter
    {
        static SolidColorBrush TranBrush = new SolidColorBrush(Colors.Transparent);

        static SolidColorBrush AsignBrush = new SolidColorBrush(Colors.IndianRed);
        static SolidColorBrush GrayBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = TranBrush;

            try
            {
                if (value is true)
                {
                    retval = AsignBrush;
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

    public class PadCountToForegroundConverter : IValueConverter
    {
        static SolidColorBrush RegistBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush NotRegistBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                string inputvalue = value as string;

                int padcount = 0;

                if (Int32.TryParse(inputvalue, out padcount))
                {
                    // you know that the parsing attempt
                    // was successful.

                    if (padcount > 0)
                    {
                        retval = RegistBrush;
                    }
                    else
                    {
                        retval = NotRegistBrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PMIDataToForegroundConverter : IValueConverter
    {
        static SolidColorBrush Emptybrush = new SolidColorBrush(Colors.Gray);
        static SolidColorBrush NormalBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = NormalBrush;

            try
            {
                string inputvalue = value as string;

                // bool ret;
                int pmitype;

                pmitype = (int)value;
                // ret = Int32.TryParse(inputvalue, out pmitype);

                if (pmitype > 0)
                {
                    retval = NormalBrush;
                }
                else
                {
                    retval = Emptybrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class LotsettingsDataToForegroundConverter : IValueConverter
    {
        static SolidColorBrush Emptybrush = new SolidColorBrush(Colors.Gray);
        static SolidColorBrush NormalBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.DimGray);

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = NormalBrush;

            try
            {
                string inputvalue = value as string;

                if (inputvalue == "Empty")
                {
                    retval = Emptybrush;
                }

                else
                {
                    retval = NormalBrush;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }


    public class PadCountToTextConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                string inputvalue = value as string;

                int padcount = 0;

                if (Int32.TryParse(inputvalue, out padcount))
                {
                    // you know that the parsing attempt
                    // was successful.

                    if (padcount > 0)
                    {
                        retval = $"Registered ({padcount})";
                    }
                    else
                    {
                        retval = $"Not registered";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PmiTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                string inputvalue = value.ToString();

                int pmitype = Int32.Parse(inputvalue);

                if (pmitype == 0)
                {
                    retval = "None";
                }
                else if (pmitype == 1)
                {
                    retval = "Every";
                }
                else if (pmitype == 2)
                {
                    retval = "Total";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CellSelectedToBrushConverter : IMultiValueConverter
    {
        static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush GreenBrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush GreenBrush2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DFC0A"));

        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                IList selectedlist = values[0] as IList;

                int selectedcount = (int)values[2];

                if (selectedcount != 0)
                {
                    if (selectedlist.Count > 0)
                    {
                        IStageObject CurrentStage = values[1] as IStageObject;

                        int index = selectedlist.IndexOf(CurrentStage);

                        if (index != -1)
                        {
                            retval = GreenBrush2;
                        }
                        else
                        {
                            retval = WhiteBrush;
                        }
                    }
                    else
                    {
                        retval = WhiteBrush;

                    }
                }
                else
                {
                    retval = WhiteBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public class CellSelectedToBorderThicknessConverter : IMultiValueConverter
    {
        static Thickness SelectedBorderThickness = new Thickness(3);
        static Thickness NotSelectedBorderThickness = new Thickness(1);

        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Thickness? retval = null;

            try
            {
                IList selectedlist = values[0] as IList;

                int selectedcount = (int)values[2];

                if (selectedcount != 0)
                {
                    if (selectedlist.Count > 0)
                    {
                        IStageObject CurrentStage = values[1] as IStageObject;

                        int index = selectedlist.IndexOf(CurrentStage);

                        if (index != -1)
                        {
                            retval = SelectedBorderThickness;
                        }
                        else
                        {
                            retval = NotSelectedBorderThickness;
                        }
                    }
                    else
                    {
                        retval = NotSelectedBorderThickness;

                    }
                }
                else
                {
                    retval = NotSelectedBorderThickness;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class StageModeNSoakingModuleStateToIsEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool retval = true;

            try
            {
                if (values[0] is GPCellModeEnum && values[1] is String)
                {
                    GPCellModeEnum cellMode = (GPCellModeEnum)values[0];
                    string soakingModuleState = (string)values[1];
                    
                    if (cellMode == GPCellModeEnum.MAINTENANCE
                        && (soakingModuleState == ModuleStateEnum.RUNNING.ToString() || soakingModuleState == ModuleStateEnum.SUSPENDED.ToString()))
                    {
                        retval = false;
                    }
                    else
                    {
                        retval = true;
                    }
                    if(cellMode == GPCellModeEnum.MAINTENANCE
                        && soakingModuleState == ModuleStateEnum.IDLE.ToString())
                    {
                        retval = true;
                    }
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
             System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StageMovelockBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            double retval = 0.0;
            try
            {
                string isConnected = values[0].ToString();
                if (isConnected == "True")
                {
                    if (values[1] != null)
                    {
                        if (values[1].Equals(parameter))
                        {
                            retval = 0.6;
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StageMovelockVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Visibility retval = Visibility.Hidden;
            try
            {
                string isConnected = values[0].ToString();
                if (isConnected == "True")
                {
                    if (values[1] != null)
                    {
                        if (values[1].Equals(parameter))
                        {
                            retval = Visibility.Visible;
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    
}
