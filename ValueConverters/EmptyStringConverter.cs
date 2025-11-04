using System;
using System.Globalization;
using System.Windows.Data;
using LogModule;

namespace ValueConverters
{
    public class EmptyStringConverter : BaseConverter, IValueConverter
    {
        public EmptyStringConverter()
        { }
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                return string.IsNullOrEmpty(value as string) ? parameter : value;
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
    }
}
