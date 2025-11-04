using LogModule;
using ProberInterfaces.Param;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class VMPatternPosConvert : IValueConverter
    {
        //private ICoordinateManager CoordManager;
        //private CatCoordinates coord;

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            try
            {
                CatCoordinates position = value as CatCoordinates;

                if (position == null)
                {
                    return string.Format("(null,null)");
                }
                else
                {
                    return string.Format("({0:f0} , {1:f0})", position.X.Value, position.Y.Value);

                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }

}

