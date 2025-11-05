using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace ValueConverters
{
    public class PWStringToBrushConverter : IValueConverter
    {
        static SolidColorBrush UnknownBrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = UnknownBrush;

            try
            {
                if (value != null)
                {
                    string colorname = (string)value;
                    retval = (SolidColorBrush)new BrushConverter().ConvertFromString(colorname);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class PWDefineTypeToBrushConverter : IMultiValueConverter
    {
        static SolidColorBrush Nullbrush = new SolidColorBrush(Colors.WhiteSmoke);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Brush Retval = Nullbrush;

            try
            {
                if (2 == values.Length)
                {
                    string DefineName = values[0] as string;
                    AsyncObservableCollection<IPolishWaferSourceInformation> pwtypes = values[1] as AsyncObservableCollection<IPolishWaferSourceInformation>;

                    if ((DefineName != null) && (pwtypes != null))
                    {
                        var source = pwtypes.Where(x => x.DefineName.Value == DefineName).FirstOrDefault();

                        if (source != null)
                        {
                            Retval = (SolidColorBrush)new BrushConverter().ConvertFromString(source.IdentificationColorBrush.Value);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return true;
            }

            return Retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
