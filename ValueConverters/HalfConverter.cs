using LogModule;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class HalfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                return (double)value / 2.0;
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
            return null;
        }
    }

}
