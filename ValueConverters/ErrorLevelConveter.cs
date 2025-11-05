using LogModule;
using ProberErrorCode;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace ValueConverters
{
    public class ErrorlineCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is int)
                {
                    int tmpvalue = (int)value;

                    if (tmpvalue > 0)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class EventCodeEnumToExcpetUnderscoreStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string retval = string.Empty;

                if (value is EventCodeEnum)
                {
                    retval = value.ToString().Replace("_", " ");
                }

                return retval;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class EventCodeEnumToHexValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string hexValue = string.Empty;

                if (value is EventCodeEnum)
                {
                    int intValue = (int)value;

                    hexValue = intValue.ToString("X");

                    hexValue = string.Concat("(0x", hexValue, ")");

                    // Convert the hex string back to the number
                    //int intAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                }

                return hexValue;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }


    public class ErrorLevelToColorConverter : IValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string hexValue;

                if (value is EventCodeEnum)
                {
                    int intValue = (int)value;

                    hexValue = intValue.ToString("X");

                    switch (hexValue[0])
                    {
                        case '0':
                            return orangebrush;
                        case '1':
                            return Redbrush;
                        default:
                            return mintCreambrush;
                    }
                }
                else
                {
                    return mintCreambrush;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class ErrorIsUsedToColorConverter : IValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Graybrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#A6A6A6"));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    bool tmp = (bool)value;

                    if (tmp == false)
                    {
                        return Graybrush;
                    }
                    else
                    {
                        return orangebrush;
                    }
                }
                else
                {
                    return Graybrush;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
