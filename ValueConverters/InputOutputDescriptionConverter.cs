using CommonResources;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class InputOutputDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = value?.ToString() ?? string.Empty;

            if(!string.IsNullOrEmpty(retVal))
            {
                retVal = ResourcesVendingMachine.GetResourceString(retVal) ?? retVal;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
