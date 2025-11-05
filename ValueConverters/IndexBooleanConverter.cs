using LogModule;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ValueConverters
{
    public class IndexBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return false;
                else
                    return (int)value == System.Convert.ToInt32(parameter);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return null;
                else if ((bool)value)
                    return System.Convert.ToInt32(parameter);
                else
                    return DependencyProperty.UnsetValue;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
