using System;

namespace WizardCategoryView.Converter
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    public class DetailSummaryButtonConverterVisiability : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value is UserControl && targetType == typeof(bool))
            //{
            if (value != null)
                return Visibility.Visible;
            else
                return Visibility.Hidden;

            //}

            //throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class SeleteTemplateButtonConverterVisiability : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
            //if (value is IHasTemplate)
            //    return Visibility.Visible;
            //else
            //    return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class NewStartButtonConverterEnable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }

}
