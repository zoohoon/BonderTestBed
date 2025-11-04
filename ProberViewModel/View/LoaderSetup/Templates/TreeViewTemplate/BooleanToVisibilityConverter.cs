using System;
using System.Globalization;
using System.Windows.Data;

namespace LoaderSetupPageView.Templates.TreeViewTemplate
{
    public class BooleanToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value == true)
            {
                return System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
