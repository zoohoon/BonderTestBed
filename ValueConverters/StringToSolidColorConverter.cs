namespace ValueConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LogModule;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class BoolToSolidColorConverter : IValueConverter
    {
        static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush GreenYellowbrush = new SolidColorBrush(Colors.GreenYellow);
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if(value is bool)
                {
                    if((bool)value)
                    {
                        return GreenYellowbrush;
                    }else
                    {
                        return Whitebrush;
                    }
                }
                return Whitebrush;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
