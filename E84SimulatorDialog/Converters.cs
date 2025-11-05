using E84;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace E84SimulatorDialog
{
    public class DoubleToOnOffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = "(OFF)";

            try
            {
                if (value is double d)
                {
                    if (d == 1)
                    {
                        retval = "(ON)";
                    }
                }
            }
            catch (Exception)
            {
            }


            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retval = Brushes.Black;

            try
            {
                if (value is double d)
                {
                    if (d == 1)
                    {
                        retval = Brushes.Red;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retval = Brushes.Black;

            try
            {
                if (value is bool d)
                {
                    if (d == true)
                    {
                        retval = Brushes.Red;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is ListView listView)
                {
                    int itemCount = listView.Items.Count;

                    if (itemCount > 0)
                    {
                        double h = listView.ActualHeight / itemCount;

                        if (h >= 51)
                        {
                            return 51;
                        }
                        else
                        {
                            return h;
                        }

                    }
                }
            }
            catch (Exception)
            {
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemHeightConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is double windowHeight && values[1] is ListView lv)
                {
                    int totalcnt = lv.Items.Count;

                    double h = windowHeight / totalcnt;

                    if (h >= 48)
                    {
                        return 30;
                    }
                    else
                    {
                        return h;
                    }

                }
            }
            catch (Exception)
            {
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return false;

                if (value.GetType() != parameter.GetType())
                    return false;
            }
            catch (Exception)
            {
            }

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class BooleanToSignalColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                // Return red color for true values
                return new SolidColorBrush(Colors.Red);
            }
            else
            {
                // Return green color for false values
                return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ModeToFalseBooleanConverter : IValueConverter
    {
        public E84SimulMode FalseValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is E84SimulMode mode)
                {
                    if (mode == FalseValue)
                    {
                        return false;
                    }
                }

            }
            catch (Exception)
            {
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModeToCollapsedConverter : IValueConverter
    {
        public E84SimulMode CollapsedValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is E84SimulMode mode)
                {
                    if (mode == CollapsedValue)
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModeToVisibilityConverter : IValueConverter
    {
        public E84SimulMode VisibilityValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is E84SimulMode mode)
                {
                    if (mode == VisibilityValue)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return !boolValue;
                }
            }
            catch (Exception)
            {
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LoadPortCardLabelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = "";

            try
            {
                if (values.Length >= 2 && (values[0] is int) && (values[1] is E84OPModuleTypeEnum))
                {
                    E84OPModuleTypeEnum oPModuleTypeEnum = (E84OPModuleTypeEnum)values[1];
                    int foupnum = (int)values[0];
                    if (oPModuleTypeEnum == E84OPModuleTypeEnum.FOUP)
                    {
                        retVal = $"LOAD PORT #{foupnum}";
                    }
                    else if (oPModuleTypeEnum == E84OPModuleTypeEnum.CARD)
                    {
                        retVal = $"CARD BUFFER #{foupnum}";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class E84ExecuteCommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            E84Execute retval = null;

            // Extract the necessary values from the provided array
            if (values.Length >= 2 && values[0] != null && values[1] != null)
            {
                if (values[0] is E84Input i)
                {
                    retval = new E84Execute();
                    retval.Type = i.Type;
                }
                else if (values[0] is E84Output o)
                {
                    retval = new E84Execute();
                    retval.Type = o.Type;
                }

                retval.Flag = (bool)values[1];
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // This converter only supports one-way conversion, so ConvertBack is not implemented
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    // Convert true to Visibility.Visible
                    return Visibility.Visible;
                }
                else
                {
                    // Convert false to Visibility.Collapsed
                    return Visibility.Collapsed;
                }
            }

            // Return Visibility.Collapsed for null or non-boolean values
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is double gridHeight) || !(values[1] is IEnumerable<E84Chart> items))
                return Binding.DoNothing;

            int visibleItemCount = items.Count(item => item.IsVisible == Visibility.Visible);
            double itemHeight = gridHeight / visibleItemCount;

            return itemHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TextToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is E84BehaviorResult data))
                return Binding.DoNothing;

            // Create a TextBlock to display the error message
            TextBlock textBlock = new TextBlock
            {
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };

            // $"The {TargetState} value of {TargetName} was checked for {Timeout} sec, but an error occurred due to a timeout, and the value of {signal.ToString()} could not be changed to {state}.";

            // Convert boolean values to 'ON' or 'OFF'
            string Targetstate = data.TargetState ? "ON" : "OFF";
            string TimeoutstateText = data.TimeOutData.TargetState ? "ON" : "OFF";

            // Build the error message by adding individual Runs to the TextBlock's Inlines
            textBlock.Inlines.Add(new Run($"The "));
            textBlock.Inlines.Add(new Run(Targetstate) { Foreground = Brushes.Red });
            textBlock.Inlines.Add(new Run($" value of "));
            textBlock.Inlines.Add(new Run(data.TargetName) { Foreground = Brushes.Red });
            textBlock.Inlines.Add(new Run($" was checked for "));
            textBlock.Inlines.Add(new Run(data.TimeOutData.TimeOut.ToString()) { Foreground = Brushes.Red });
            textBlock.Inlines.Add(new Run($" sec, but an error occurred due to a timeout, and the value of "));
            textBlock.Inlines.Add(new Run(data.TimeOutData.TargetSignalName) { Foreground = Brushes.Red });
            textBlock.Inlines.Add(new Run($" could not be changed to "));
            textBlock.Inlines.Add(new Run(TimeoutstateText) { Foreground = Brushes.Red });
            textBlock.Inlines.Add(new Run($"."));

            // Create a ToolTip and set its content as the TextBlock
            ToolTip toolTip = new ToolTip();
            toolTip.Content = textBlock;

            return toolTip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ErrorCodeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int codeNumber = (int)value;
            return codeNumber != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class IndexToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return (index + 1).ToString(); // Index는 0부터 시작하므로 1을 더해 텍스트로 변환합니다.
            }

            return string.Empty; // 변환할 수 없는 경우 빈 문자열을 반환합니다.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ListIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is ListView lv && values[1] is ListViewItem lvi)
                {
                    int index = lv.ItemContainerGenerator.IndexFromContainer(lvi);
                    return (index + 1).ToString();
                }
            }
            catch (Exception)
            {
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SignalBoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool signalON = (bool)value;

                if (signalON == true)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.LightGray;
                }
            }

            return Brushes.LightGray;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SignalIntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int signalON = (int)value;

                if (signalON == 1)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.LightGray;
                }
            }

            return Brushes.LightGray;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NullToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class E84OPModuleTypeToFOUPVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is E84OPModuleTypeEnum moduleType && moduleType == E84OPModuleTypeEnum.FOUP)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class E84OPModuleTypeToCARDVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is E84OPModuleTypeEnum moduleType && moduleType == E84OPModuleTypeEnum.CARD)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EnumToTabItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            E84SimulMode mode = (E84SimulMode)value;

            switch (mode)
            {
                case E84SimulMode.MANUAL:
                    return 0; 
                case E84SimulMode.AUTO:
                    return 1;  
                case E84SimulMode.MAKER:
                    return 2; 
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

