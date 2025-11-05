using System;
using System.Windows.Data;

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces.PnpSetup;
    public class TypeToStringConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return value.GetType().Name;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class IsSetupTypeConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool isSetupType = false;
            try
            {
                if (value is ISetup)
                {
                    isSetupType = true;
                }
                else
                {
                    isSetupType = false;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return isSetupType;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
