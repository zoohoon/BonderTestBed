using System.Windows;
using System.Windows.Data;

namespace ValueConverters
{
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Thickness(0, System.Convert.ToDouble(value), 0, 0);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class MultiMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Thickness(System.Convert.ToDouble(values[0]),
                                 System.Convert.ToDouble(values[1]),
                                 System.Convert.ToDouble(values[2]),
                                 System.Convert.ToDouble(values[3]));
        }

        public object[] ConvertBack(object value, System.Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
