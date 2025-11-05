using System;

namespace TestSetupDialog.Tab.GPCC.Converteres
{
    using System.Globalization;
    using System.Windows.Data;
    public class BoolToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool toggle = (bool)value;
            return toggle ? "LightGreen" : "Red";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
