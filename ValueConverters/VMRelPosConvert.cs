using LogModule;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ValueConverters
{
    public class VMRelPosConvert : IValueConverter
    {
        //private ICoordinateManager CoordManager;
        //private CatCoordinates coord;
        
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            double position = (double)value;
          
            try
            {
                if (parameter != null)
                {
                    Label label = (Label)parameter;
                    double offset=Double.Parse(label.Content.ToString());
                    position = offset - position;
                    //position += offset;
                    position= Math.Round(position, 0);
                }
            

            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }

            return position;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }

}
