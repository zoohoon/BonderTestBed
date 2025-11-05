using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LogModule;

namespace ValueConverters
{
    public class IntToVisibilityConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                Visibility retval = Visibility.Hidden;

                if (value is int)
                {
                    int intValue = (int)value;

                    if (intValue > 0)
                    {
                        retval = Visibility.Visible;
                    }
                }

                return retval;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static bool IsSupportedType(object value)
        {
            return value is int || value is double || value is byte || value is long ||
                   value is float || value is uint || value is short || value is sbyte ||
                   value is ushort || value is ulong || value is decimal;
        }
    }
}
