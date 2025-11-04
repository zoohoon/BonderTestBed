using LoaderBase.Communication;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Temperature.Chiller;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ProberViewModel
{
    public class GridHelpers
    {
        #region RowCount Property

        /// <summary>
        /// Adds the specified number of Rows to RowDefinitions. 
        /// Default Height is Auto
        /// </summary>
        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.RegisterAttached(
                "RowCount", typeof(int), typeof(GridHelpers),
                new PropertyMetadata(-1, RowCountChanged));

        // Get
        public static int GetRowCount(DependencyObject obj)
        {
            return (int)obj.GetValue(RowCountProperty);
        }

        // Set
        public static void SetRowCount(DependencyObject obj, int value)
        {
            obj.SetValue(RowCountProperty, value);
        }

        // Change Event - Adds the Rows
        public static void RowCountChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.RowDefinitions.Clear();

            for (int i = 0; i < (int)e.NewValue; i++)
                grid.RowDefinitions.Add(
                    new RowDefinition() { Height = GridLength.Auto });

            SetStarRows(grid);
        }

        #endregion

        #region ColumnCount Property

        /// <summary>
        /// Adds the specified number of Columns to ColumnDefinitions. 
        /// Default Width is Auto
        /// </summary>
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.RegisterAttached(
                "ColumnCount", typeof(int), typeof(GridHelpers),
                new PropertyMetadata(-1, ColumnCountChanged));

        // Get
        public static int GetColumnCount(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnCountProperty);
        }

        // Set
        public static void SetColumnCount(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnCountProperty, value);
        }

        // Change Event - Add the Columns
        public static void ColumnCountChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.ColumnDefinitions.Clear();

            for (int i = 0; i < (int)e.NewValue; i++)
                grid.ColumnDefinitions.Add(
                    new ColumnDefinition() { Width = GridLength.Auto });

            SetStarColumns(grid);
        }

        #endregion

        #region StarRows Property

        /// <summary>
        /// Makes the specified Row's Height equal to Star. 
        /// Can set on multiple Rows
        /// </summary>
        public static readonly DependencyProperty StarRowsProperty =
            DependencyProperty.RegisterAttached(
                "StarRows", typeof(string), typeof(GridHelpers),
                new PropertyMetadata(string.Empty, StarRowsChanged));

        // Get
        public static string GetStarRows(DependencyObject obj)
        {
            return (string)obj.GetValue(StarRowsProperty);
        }

        // Set
        public static void SetStarRows(DependencyObject obj, string value)
        {
            obj.SetValue(StarRowsProperty, value);
        }

        // Change Event - Makes specified Row's Height equal to Star
        public static void StarRowsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            SetStarRows((Grid)obj);
        }

        #endregion

        #region StarColumns Property

        /// <summary>
        /// Makes the specified Column's Width equal to Star. 
        /// Can set on multiple Columns
        /// </summary>
        public static readonly DependencyProperty StarColumnsProperty =
            DependencyProperty.RegisterAttached(
                "StarColumns", typeof(string), typeof(GridHelpers),
                new PropertyMetadata(string.Empty, StarColumnsChanged));

        // Get
        public static string GetStarColumns(DependencyObject obj)
        {
            return (string)obj.GetValue(StarColumnsProperty);
        }

        // Set
        public static void SetStarColumns(DependencyObject obj, string value)
        {
            obj.SetValue(StarColumnsProperty, value);
        }

        // Change Event - Makes specified Column's Width equal to Star
        public static void StarColumnsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            SetStarColumns((Grid)obj);
        }

        #endregion

        private static void SetStarColumns(Grid grid)
        {
            string[] starColumns =
                GetStarColumns(grid).Split(',');

            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                if (starColumns.Contains(i.ToString()))
                    grid.ColumnDefinitions[i].Width =
                        new GridLength(1, GridUnitType.Star);
            }
        }

        private static void SetStarRows(Grid grid)
        {
            string[] starRows =
                GetStarRows(grid).Split(',');

            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                if (starRows.Contains(i.ToString()))
                    grid.RowDefinitions[i].Height =
                        new GridLength(1, GridUnitType.Star);
            }
        }
    }

    /// <summary>
    /// LoaderTopBarView.xaml에 대한 상호 작용 논리
    /// </summary>
    //public partial class LoaderTopBarView : UserControl, IMainTopBarView
    public partial class LoaderTopBarView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("6bc035c4-1aed-4154-9857-49bbdbcb75d8");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public LoaderTopBarView()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            LoggerManager.Event(1, DateTime.Now, ProberErrorCode.EventCodeEnum.AIRBLOW_CLEANING_ERROR, "1");
            LoggerManager.Event(0, DateTime.Now, ProberErrorCode.EventCodeEnum.AIRBLOW_CLEANING_ERROR, "2");
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Application.Current.MainWindow;
            MahApps.Metro.Controls.MetroWindow metro = (MahApps.Metro.Controls.MetroWindow)window;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                window.WindowState = WindowState.Normal;
                window.DragMove();
            }
        }

    }
    public class LoderTopBarGemConv : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //enum EnumCommunicationState
            //UNAVAILABLE = 0,
            //DISCONNECT = 1,
            //CONNECTED = 2
            try
            {
                if (value != null)
                {
                    if (value is EnumCommunicationState)
                    {
                        switch (value)
                        {
                            case EnumCommunicationState.UNAVAILABLE:

                                return new SolidColorBrush(Colors.Gray);
                                break;
                            case EnumCommunicationState.DISCONNECT:

                                return new SolidColorBrush(Colors.Orange);
                                break;
                            case EnumCommunicationState.CONNECTED:

                                return new SolidColorBrush(Colors.Green);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                return null;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class LoderTopBarGemControlStateConv : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = "";
            try
            {
                if (value != null)
                {
                    if (value is SecsEnum_ControlState)
                    {
                        switch (value)
                        {
                            case SecsEnum_ControlState.UNKNOWN:
                                retval = "UNKNOWN";
                                break;
                            case SecsEnum_ControlState.UNDEFIND:
                                retval = "UNDEFIND";
                                break;
                            case SecsEnum_ControlState.ONLINE_REMOTE:
                                retval = "ONLINE\nREMOTE";
                                break;
                            case SecsEnum_ControlState.ONLINE_LOCAL:
                                retval = "ONLINE\nLOCAL";
                                break;
                            case SecsEnum_ControlState.HOST_OFFLINE:
                                retval = "HOST\nOFFLINE";
                                break;
                            case SecsEnum_ControlState.EQ_OFFLINE:
                                retval = "EQ\nOFFLINE";
                                break;
                            case SecsEnum_ControlState.ATTEMPT_ONLINE:
                                retval = "ATTEMPT\nONLINE";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                return null;
            }
            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BuzzerTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retVal = Visibility.Hidden;
            try
            {
                if (value is bool)
                {
                    bool val = (bool)value;
                    if (val)
                    {
                        retVal = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BuzzerFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retVal = Visibility.Hidden;
            try
            {
                if (value is bool)
                {
                    bool val = (bool)value;
                    if (!val)
                    {
                        retVal = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BuzzerStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "ON";
            try
            {
                if (value is bool)
                {
                    bool val = (bool)value;
                    if (val)
                    {
                        ret = "OFF";
                    }
                    else
                    {
                        ret = "ON";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageStateToValueConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Gray;
            try
            {


                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum state = (ModuleStateEnum)value;
                    switch (state)
                    {
                        case ModuleStateEnum.ERROR:
                            brush = Brushes.Red;
                            break;
                        case ModuleStateEnum.IDLE:
                            brush = Brushes.Orange;
                            break;
                        case ModuleStateEnum.RUNNING:
                            brush = Brushes.LimeGreen;
                            break;
                        case ModuleStateEnum.UNDEFINED:
                            brush = Brushes.Gray;
                            break;
                        case ModuleStateEnum.PAUSING:
                            brush = Brushes.Gray;
                            break;
                        case ModuleStateEnum.PAUSED:
                            brush = Brushes.LimeGreen;
                            break;
                        case ModuleStateEnum.ABORT:
                            brush = Brushes.Red;
                            break;
                        default:
                            //THROUGH OUT
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ChillerActiveToValueConverter : IMultiValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush brush = Brushes.Red;
            try
            {
                if (values != null)
                {
                    if (values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue)
                    {
                        EnumCommunicationState isConnected = (EnumCommunicationState)values[0];
                        bool isActived = (bool)values[1];
                        bool isError = (bool)values[2];
                        if (isConnected == EnumCommunicationState.CONNECTED)
                        {
                            if (isError)
                            {
                                brush = Brushes.Red;
                            }
                            else
                            {
                                switch (isActived)
                                {
                                    case true:
                                        brush = Brushes.LimeGreen;
                                        break;
                                    case false:
                                        brush = Brushes.Orange;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            brush = Brushes.Gray;
                        }
                    }
                }



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

    public class TempToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string temp = "Null";
            try
            {
                if (value is double)
                {
                    temp = System.Convert.ToString((double)value) + " 'C";
                }
                else if (value is IChillerModule)
                {
                    IChillerModule chiller = (IChillerModule)value;
                    if (chiller.CommunicationState != EnumCommunicationState.CONNECTED)
                    {
                        temp = "--";
                    }
                    else
                    {
                        temp = System.Convert.ToString((double)value) + " 'C";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return temp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PVToValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                IStageObject stage = values[0] as IStageObject;

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        if(stage.StageInfo.IsConnected != false)
                        {
                            retval = "PV : " + string.Format("{0:0.0}", stage.StageInfo.PV) + "℃";
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

    public class SelectedStageIndexContentConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string reval = string.Empty;

            try
            {
                int inputIndex = (int)value;

                if (inputIndex <= 0)
                {
                    //reval = "EMPTY";
                    reval = "";
                }
                else
                {
                    reval = inputIndex.ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return reval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedStageIndexToForegrundConvert : IValueConverter
    {
        static SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.Gray);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = DefaultBrush;

            try
            {
                int inputIndex = (int)value;

                if (inputIndex <= 0)
                {
                    brush = Redbrush;
                }
                else
                {
                    brush = LimeGreenbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
