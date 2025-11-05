using LogModule;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ValueConverters
{
    public class ProgressBarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush brush;
            try
            {
                double var = 0;
                if (value is double)
                {
                    var = (double)value;
                }
                if (var > 50)
                {
                    brush = new SolidColorBrush(Colors.Green);
                }
                else if (var > 20)
                {
                    brush = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    brush = new SolidColorBrush(Colors.Red);
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
