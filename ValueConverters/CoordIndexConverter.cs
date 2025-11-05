using System;
using System.Windows.Data;

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using System.Globalization;

    public class CoordIndexConverter : IValueConverter
    {
        

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            string strPos = "NULL";
            CatCoordinates coord;
            double pos = 0d;     
            ICamera cam = null;
            EnumAxisConstants axisType;
            double wuindex = 0d;            

            try
            {

                if (value != null & parameter != null)
                {
                    if (value is ICamera & parameter is EnumAxisConstants)
                    {
                        cam = (ICamera)value;
                        axisType = (EnumAxisConstants)parameter;
                        coord = cam.GetCurCoordPos();
                        switch (axisType)
                        {
                            case EnumAxisConstants.Undefined:
                                break;
                            case EnumAxisConstants.X:
                                pos = coord.GetX();
                                wuindex = cam.GetCurCoordIndex().XIndex;                                
                                break;
                            case EnumAxisConstants.Y:
                                pos = coord.GetY();
                                wuindex = cam.GetCurCoordIndex().YIndex;
                                break;                            
                            default:
                                break;
                        }
                        strPos = pos.ToString();
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, string.Format("CamCoordConverter::Convert error occurred."));
                LoggerManager.Exception(err);
            }
            return wuindex;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
