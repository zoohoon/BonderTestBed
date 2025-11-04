using System;
using System.Globalization;
using System.Windows.Data;

namespace HighAlignKeyAdvancedSetupModule
{
    using LogModule;
    using ProbeCardObject;
    using ProberInterfaces;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// UC_PMI_StandardSetup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HighAlignKeyAdvancedSetupControl : MahApps.Metro.Controls.Dialogs.CustomDialog, IPnpAdvanceSetupView
    {
        public HighAlignKeyAdvancedSetupControl()
        {
            InitializeComponent();
        }


        private void MyDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                double rowHeight = 31; // Height of each row
                double headerHeight = 25; // Height of the header
                int numberOfRows = 7; // The number of rows you want to display

                double totalHeight = headerHeight + (numberOfRows * rowHeight);
                myDataGrid.Height = totalHeight;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ScrollDataGrid(-1); // 한 번에 한 줄 위로 이동
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ScrollDataGrid(1); // 한 번에 한 줄 아래로 이동
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ScrollDataGrid(int direction)
        {
            try
            {
                var scrollViewer = GetScrollViewer(myDataGrid) as ScrollViewer;
                if (scrollViewer != null)
                {
                    double newOffset = scrollViewer.VerticalOffset + (direction * 5); // 'rowHeight'는 DataGrid 행의 높이입니다.
                    scrollViewer.ScrollToVerticalOffset(newOffset);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private static DependencyObject GetScrollViewer(DependencyObject o)
        {
            try
            {
                if (o is ScrollViewer) { return o; }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
                {
                    var child = VisualTreeHelper.GetChild(o, i);
                    var result = GetScrollViewer(child);
                    if (result == null) continue;
                    return result;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }
    }

    public class BoolRadioConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return this.Inverse ? !boolValue : boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            if (!boolValue)
            {
                // We only care when the user clicks a radio button to select it.
                return null;
            }

            return !this.Inverse;
        }
    }

    public class PinSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<ValidationPinItem> pinList) || parameter == null)
                return false;

            if (!int.TryParse(parameter.ToString(), out int pinNum))
                return false;

            var pinItem = pinList.Find(p => p.PinNum.Value == pinNum);

            return pinItem != null && pinItem.IsSelected.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckBoxConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;

            try
            {
                int pinNum = 0;
                ObservableCollection<ValidationPinItem> pintip_validationList = null;

                if (values[0] != null)
                {
                    pinNum = (int)values[0];
                }

                if (values[1] != null)
                {
                    if (values[1] is ObservableCollection<ValidationPinItem>)
                    {
                        pintip_validationList = values[1] as ObservableCollection<ValidationPinItem>;
                    }
                }

                if (pintip_validationList != null)
                {
                    var matchingItem = pintip_validationList.FirstOrDefault(item => item.PinNum.Value == pinNum);

                    if (matchingItem != null && matchingItem.IsSelected.Value)
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class Rec_CenterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double ret = 0;

            try
            {
                List<double> valueList = new List<double>();
                if (values != null)
                {
                    if (values.Count() == 6)
                    {
                        foreach (var val in values)
                        {
                            if (val != null)
                            {
                                if (val is int intVal)
                                {
                                    valueList.Add(intVal);
                                }
                                else if (val is double doubleVal)
                                {
                                    valueList.Add(doubleVal);
                                }
                                else if (val is float flotVal)
                                {
                                    valueList.Add(flotVal);
                                }
                                else
                                {
                                    ret = 0.0;
                                    break;
                                }
                            }
                        }
                        // Target size, MaxX, MaxY, Actual Width, Actual Height
                        double maxX, maxY, width, height, targetval_rec, target_actual_rec;
                        double scale = 1.0;
                        targetval_rec = valueList[0];
                        target_actual_rec = valueList[1];
                        maxX= valueList[2];
                        maxY = valueList[3];
                        width = valueList[4];
                        height = valueList[5];

                        if (maxX > maxY)
                        {
                            scale = width / maxX;
                        }
                        else
                        {
                            scale = height / maxY;
                        }
                        if (scale > 0)
                        {
                            ret = (target_actual_rec / 2) - (targetval_rec * scale / 2);
                        }
                        else // Exception
                        {
                            ret = 1.0;
                        }

                    }
                    else
                    {
                        ret = 1.0;
                    }
                }
                //double rectangleSize = 0;
                //double canvasSize = 0;

                //if (values[0] != null && values[1] != null)
                //{
                //    // Check if the first value is int or double and convert appropriately
                //    if (values[0] is int intValue)
                //    {
                //        rectangleSize = intValue;
                //    }
                //    else if (values[0] is double doubleValue)
                //    {
                //        rectangleSize = doubleValue;
                //    }

                //    canvasSize = System.Convert.ToDouble(values[1]);

                //    // Calculate the position to center the rectangle
                //    ret = (canvasSize / 2) - (rectangleSize / 2 * 10);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class InvertAllOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                // Assuming 'value' is of type SizeValidationOption
                if (value is SizeValidationOption option)
                {
                    // Returns false if the option is ALL, true otherwise
                    retval = option != SizeValidationOption.ALL;
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
            // ConvertBack is not necessary for this scenario
            throw new NotImplementedException();
        }
    }

    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }

    public class FTPUploadConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;

            try
            {
                if (values[0] is bool validEnable)
                {
                    if (!validEnable)
                    {
                        return ret;
                    }
                }

                if (values[1] is bool outSave && values[2] is bool inSave)
                {
                    if (outSave || inSave)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    /// <summary>
    /// Target size, MaxX, MaxY, Actual Width, Actual Height
    /// </summary>
    public class RectangleScaleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double ret = 0;

            try
            {
                List<double> valueList = new List<double>();
                if(values != null)
                {
                    if( values.Count() == 5)
                    {
                        foreach (var val in values)
                        {
                            if (val != null)
                            {
                                if (val is int intVal)
                                {
                                    valueList.Add(intVal);
                                }
                                else if(val is double doubleVal)
                                {
                                    valueList.Add(doubleVal);
                                }
                                else if (val is float flotVal)
                                {
                                    valueList.Add(flotVal);
                                }
                                else
                                {
                                    ret = 0.0;
                                    break;
                                }
                            }
                        }
                        // Target size, MaxX, MaxY, Actual Width, Actual Height
                        double maxX, maxY, width, height, targetval = 0;
                        double scale = 1.0;
                        targetval = valueList[0];
                        maxX = valueList[1];
                        maxY = valueList[2];
                        width = valueList[3];
                        height = valueList[4];

                        if(maxX > maxY)
                        {
                            scale = width / maxX;
                        }
                        else
                        {
                            scale = height / maxY;
                        }
                        if(scale > 0)
                        {
                            ret = scale * targetval;
                        }
                        else // Exception
                        {
                            ret = 1.0;
                        }
                        
                    }
                    else
                    {
                        ret = 1.0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
