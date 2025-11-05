using System;

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using System.Globalization;
    using System.Windows.Data;
    public class CoordConvert : IValueConverter
    {
        //private ICoordinateManager CoordManager;
        private CatCoordinates coord;

        

        public object Convert(object value, Type targetType, object parameter, 
            CultureInfo culture)
        {
            ICamera camera = (ICamera)value;
            ICoordinateManager coordmanager;
            EnumProberCam cam = (EnumProberCam)value;
            try
            {
                if (parameter != null)
                {
                    if (parameter is ICoordinateManager)
                    {
                        coordmanager = (ICoordinateManager)parameter;
                        switch (cam)
                        {
                            case EnumProberCam.WAFER_HIGH_CAM:
                                coord = coordmanager.WaferHighChuckConvert.CurrentPosConvert();
                                break;
                            case EnumProberCam.WAFER_LOW_CAM:
                                coord = coordmanager.WaferLowChuckConvert.CurrentPosConvert();
                                break;
                            case EnumProberCam.PIN_HIGH_CAM:
                                coord = coordmanager.PinHighPinConvert.CurrentPosConvert();
                                break;
                            case EnumProberCam.PIN_LOW_CAM:
                                coord = coordmanager.PinLowPinConvert.CurrentPosConvert();
                                break;
                        }
                        coord.X.Value = Math.Truncate(coord.X.Value);
                        coord.Y.Value = Math.Truncate(coord.Y.Value);
                        coord.Z.Value = Math.Truncate(coord.Z.Value);
                    }
                }
                else
                {
                    coord.X.Value = 0.0;
                    coord.Y.Value = 0.0;
                    coord.Z.Value = 0.0;
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("Err = {0}", err.Message));
                LoggerManager.Exception(err);


                coord.X.Value = 0.0;
                coord.Y.Value = 0.0;
                coord.Z.Value = 0.0;
            }
            
            return coord;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }

    public class MultiCoordConvert : IMultiValueConverter
    {
        //private CatCoordinates coord;
        //public object Convert(object value, Type targetType, object parameter,
        //    CultureInfo culture)
        //{

        //    ICoordinateManager coordmanager;
        //    EnumProberCam cam = (EnumProberCam)parameter;
        //    try
        //    {
        //        if (parameter != null)
        //        {
        //            if (parameter is ICoordinateManager)
        //            {
        //                coordmanager = (ICoordinateManager)parameter;
        //                switch (cam)
        //                {
        //                    case EnumProberCam.WAFER_HIGH_CAM:
        //                        coord = coordmanager.WaferHighChuckConvert.CurrentPosConvert();
        //                        break;
        //                    case EnumProberCam.WAFER_LOW_CAM:
        //                        coord = coordmanager.WaferLowChuckConvert.CurrentPosConvert();
        //                        break;
        //                    case EnumProberCam.PIN_HIGH_CAM:
        //                        coord = coordmanager.PinHighPinConvert.CurrentPosConvert();
        //                        break;
        //                    case EnumProberCam.PIN_LOW_CAM:
        //                        coord = coordmanager.PinLowPinConvert.CurrentPosConvert();
        //                        break;
        //                }
        //                coord.X.Value = Math.Truncate(coord.X.Value);
        //                coord.Y.Value = Math.Truncate(coord.Y.Value);
        //                coord.Z.Value = Math.Truncate(coord.Z.Value);
        //            }
        //        }
        //        else
        //        {
        //            coord.X.Value = 0.0;
        //            coord.Y.Value = 0.0;
        //            coord.Z.Value = 0.0;
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        coord.X.Value = 0.0;
        //        coord.Y.Value = 0.0;
        //        coord.Z.Value = 0.0;
        //    }
        //    return coord;
        //}

        //public object ConvertBack(object value, Type targetType, object parameter,
        //    CultureInfo culture)
        //{
        //    return null;
        //}
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
