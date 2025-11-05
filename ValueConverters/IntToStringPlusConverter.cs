using LogModule;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class IntToStringPlusConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                string retval = string.Empty;

                if (value is int)
                {
                    int intValue = (int)value;

                    if (intValue >= 100)
                    {
                        return "99+";
                    }
                    else
                    {
                        return intValue.ToString();
                    }
                }
                else
                {
                    return retval;
                }
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
