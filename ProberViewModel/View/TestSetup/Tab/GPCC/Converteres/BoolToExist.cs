using System;

namespace TestSetupDialog.Tab.GPCC.Converteres
{
    using System.Globalization;
    using System.Windows.Data;
    public class BoolToExist : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool toggle = (bool)value;
            return toggle ? "Exist" : "Not Exist";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
