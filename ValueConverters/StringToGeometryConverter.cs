using LogModule;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ValueConverters
{
    public class StringToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string path = value as string;

                if (null != path)
                {
                    Geometry g = Geometry.Parse(path);

                    return g;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Geometry geometry = value as Geometry;

                if (null != geometry)
                {
                    return geometry;
                }
                else
                {
                    return default(string);
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}