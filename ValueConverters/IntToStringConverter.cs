using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class IntToStringPlusOneConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          => (string)parameter == "plusOne" ? ((int)value + 1).ToString() : value.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => throw new NotImplementedException();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
         => $"{Convert(values[0], targetType, parameter, culture) as string} {values[1]}";

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
          => throw new NotImplementedException();
    }
}
