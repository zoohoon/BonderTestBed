using System;

namespace ValueConverters
{
    using ProberInterfaces;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using LogModule;

    public class CylStateToColorConverter : IValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Crimsonbrush = new SolidColorBrush(Colors.Crimson);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush Balckbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            Brush brush = Redbrush;
            try
            {
                if (value is CylinderStateEnum)
                {
                    CylinderStateEnum state = (CylinderStateEnum)value;

                    switch (state)
                    {
                        case CylinderStateEnum.UNDEFINED:
                            brush = Whitebrush;
                            break;
                        case CylinderStateEnum.JAM:
                            brush = Redbrush;
                            break;
                        case CylinderStateEnum.OVERRUN:
                            brush = Crimsonbrush;
                            break;
                        case CylinderStateEnum.EXTEND:
                            brush = Yellowbrush;
                            break;
                        case CylinderStateEnum.RETRACT:
                            brush = LimeGreenbrush;
                            break;
                        case CylinderStateEnum.ERROR:
                            brush = Redbrush;
                            break;
                        case CylinderStateEnum.ALARM:
                            brush = orangebrush;
                            break;
                        default:
                            brush = Balckbrush;
                            break;
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    //public class ViewModeToCamConverter : IValueConverter
    //{
    //    static PerspectiveCamera Foup_FV = new PerspectiveCamera() { FieldOfView = 45, NearPlaneDistance = 0.1, Position = new Point3D(1493.6, 1403.8, 1573), LookDirection = new Vector3D(-0.7, -0.5, -0.5), UpDirection = new Vector3D(-0.4, 0.9, -0.3) };
    //    static PerspectiveCamera Foup_BV = new PerspectiveCamera() { FieldOfView = 45, NearPlaneDistance = 0.1, Position = new Point3D(1370.4, 1480.3, -1098.5), LookDirection = new Vector3D(-0.6, -0.5, 0.6), UpDirection = new Vector3D(-0.4, 0.8, 0.4) };
    //    static PerspectiveCamera QuaterView = new PerspectiveCamera() { FieldOfView = 45, NearPlaneDistance = 0.1, Position = new Point3D(-2222.5, 1489.6, 1594), LookDirection = new Vector3D(0.7, -0.5, -0.6), UpDirection = new Vector3D(0.4, 0.9, -0.3) };
    //    static PerspectiveCamera PAView = new PerspectiveCamera() { FieldOfView = 45, NearPlaneDistance = 0.1, Position = new Point3D(903.3, 911.9, 179.8), LookDirection = new Vector3D(-0.8, -0.3, -0.5), UpDirection = new Vector3D(-0.3, 0.9, -0.2) };
    //    static PerspectiveCamera DPView = new PerspectiveCamera() { FieldOfView = 45, NearPlaneDistance = 0.1, Position = new Point3D(460.3, 1072.2, 1445.1), LookDirection = new Vector3D(-0.5, -0.5, -0.8), UpDirection = new Vector3D(-0.2, 0.9, -0.4) };

    //    public object Convert(object value, Type targetType,
    //          object parameter, CultureInfo culture)
    //    {
    //        PerspectiveCamera camera = null;
    //        return camera;
    //    }

    //    public object ConvertBack(object value, Type targetType,
    //        object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}
}
