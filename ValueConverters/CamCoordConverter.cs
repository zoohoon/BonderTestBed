using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    using LogModule;

    public class CamCoordConverter : IMultiValueConverter
    {
        

        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            string strPos = "NULL";
            CatCoordinates coord;
            double pos = 0d;
            EnumAxisConstants axisType;
            ICamera cam = null;

            try
            {
                if (values != null)
                {                   
                    cam = (ICamera)parameter;
                    axisType = (EnumAxisConstants)parameter;
                    coord = cam.GetCurCoordPos();
                    switch (axisType)
                    {
                        case EnumAxisConstants.Undefined:
                            break;
                        case EnumAxisConstants.X:
                            pos = coord.GetX();
                            break;
                        case EnumAxisConstants.Y:
                            pos = coord.GetY();
                            break;
                        case EnumAxisConstants.Z:
                            pos = coord.GetZ();
                            break;
                        case EnumAxisConstants.C:
                            pos = coord.GetT();
                            break;
                        default:
                            break;
                    }                        
                    strPos = pos.ToString();                                            
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, string.Format("CamCoordConverter::Convert error occurred."));
                LoggerManager.Exception(err);
            }
            return pos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
