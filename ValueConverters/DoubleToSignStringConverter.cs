using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LogModule;

namespace ValueConverters
{
    public class DoubleToSignStringConverter : IValueConverter
    {
        static DoubleToSignStringConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!IsSupportedType(value)) return DependencyProperty.UnsetValue;

                double doubleValue = (double)value;

                string retval = null;

                if (doubleValue > 0d)
                {
                    retval = "+" + doubleValue.ToString();
                }
                else if (doubleValue < 0d)
                {
                    retval = doubleValue.ToString();
                }
                else
                {
                    retval = doubleValue.ToString();
                }

                return retval;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static bool IsSupportedType(object value)
        {
            try
            {
                return value is int || value is double || value is byte || value is long ||
       value is float || value is uint || value is short || value is sbyte ||
       value is ushort || value is ulong || value is decimal;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}



