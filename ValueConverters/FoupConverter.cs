using System;
using System.Globalization;
using System.Windows.Data;
using ProberInterfaces;
using System.Windows.Media;
using ProberInterfaces.Foup;
using System.Windows;
//using FoupModules.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogModule;

namespace ValueConverters
{
    //public class FoupConverter : IValueConverter
    //{
    //    static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
    //    static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
    //    static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
    //    static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
    //    static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);

    //    public object Convert(object value, Type targetType,
    //     object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        FoupCoverStateEnum CoverState = (FoupCoverStateEnum)value;
    //        DockingPort40StateEnum DockingPort40State = (DockingPort40StateEnum)value;
    //        DockingPortStateEnum DockingPortState = (DockingPortStateEnum)value;
    //        DockingPlateStateEnum DockingPlateState = (DockingPlateStateEnum)value;
    //        //Todo 3D model 색 표현 코드 구성
    //        switch (CoverState)
    //        {
    //            case FoupCoverStateEnum.IDLE:
    //                return orangebrush;
    //            case FoupCoverStateEnum.UP:
    //                return Goldbrush;
    //            case FoupCoverStateEnum.DOWN:
    //                return LimeGreenbrush;
    //            case FoupCoverStateEnum.ERROR:
    //                return Redbrush;
    //            default:
    //                return mintCreambrush;
    //        }
    //        switch (DockingPortState)
    //        {
    //            case DockingPortStateEnum.IN:
    //                return Goldbrush;
    //            case DockingPortStateEnum.OUT:
    //                return LimeGreenbrush;
    //            case DockingPortStateEnum.ERROR:
    //                return Redbrush;
    //            default:
    //                break;
    //        }
    //        switch (DockingPort40State)
    //        {
    //            case DockingPort40StateEnum.IN:
    //                return Goldbrush;
    //            case DockingPort40StateEnum.OUT:
    //                return LimeGreenbrush;
    //            case DockingPort40StateEnum.ERROR:
    //                return Redbrush;
    //            default:
    //                break;
    //        }
    //        switch (DockingPlateState)
    //        {
    //            case DockingPlateStateEnum.LOCK:
    //                return Goldbrush;
    //            case DockingPlateStateEnum.UNLOCK:
    //                return LimeGreenbrush;
    //            case DockingPlateStateEnum.ERROR:
    //                return Redbrush;
    //            default:
    //                break;
    //        }

    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        // I don't think you'll need this
    //        throw new Exception("Can't convert back");
    //    }
    //}

    public class FoupCoverColorCon : IMultiValueConverter

    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);

        public object Convert(object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FoupCoverStateEnum)
            {
                FoupCoverStateEnum CoverState = (FoupCoverStateEnum)value;
                //Todo 3D model 색 표현 코드 구성
                switch (CoverState)
                {
                    case FoupCoverStateEnum.IDLE:
                        return orangebrush;
                    case FoupCoverStateEnum.CLOSE:
                        return Goldbrush;
                    case FoupCoverStateEnum.OPEN:
                        return LimeGreenbrush;
                    case FoupCoverStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return mintCreambrush;
                }
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is FoupCoverStateEnum && values[1] is bool)
                {
                    FoupCoverStateEnum CoverState = (FoupCoverStateEnum)values[0];

                    CylinderStateEnum CylinderState = CylType.FoupCylinderType.FoupCover.State;

                    bool BlinkLED = (bool)values[1];

                    //Todo 3D model 색 표현 코드 구성
                    //if (CylinderState == CylinderStateEnum.RUNNING)
                    if (CylinderState == CylinderStateEnum.RUNNING)
                    {

                        switch (BlinkLED)
                        {

                            case true:
                                return Goldbrush;
                            case false:
                                return orangebrush;
                            default:
                                return mintCreambrush;
                        }

                    }
                    else
                    {
                        switch (CoverState)
                        {
                            case FoupCoverStateEnum.IDLE:
                                return orangebrush;
                            case FoupCoverStateEnum.CLOSE:
                                return Goldbrush;
                            case FoupCoverStateEnum.OPEN:
                                return LimeGreenbrush;
                            case FoupCoverStateEnum.ERROR:
                                return Redbrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }
    public class FoupPortColorCon : IMultiValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is FoupCoverStateEnum && values[1] is bool)
                {
                    DockingPortStateEnum DockingPortState = (DockingPortStateEnum)values[0];

                    CylinderStateEnum CylinderState = CylType.FoupCylinderType.FoupDockingPort.State;

                    bool BlinkLED = (bool)values[1];

                    //Todo 3D model 색 표현 코드 구성
                    //if (CylinderState == CylinderStateEnum.RUNNING)
                    if (CylinderState == CylinderStateEnum.RUNNING)
                    {

                        switch (BlinkLED)
                        {

                            case true:
                                return Goldbrush;
                            case false:
                                return orangebrush;
                            default:
                                return mintCreambrush;
                        }

                    }
                    else
                    {
                        switch (DockingPortState)
                        {
                            case DockingPortStateEnum.IN:
                                return Goldbrush;
                            case DockingPortStateEnum.OUT:
                                return LimeGreenbrush;
                            case DockingPortStateEnum.ERROR:
                                return Redbrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }

    }
    public class FoupPort40ColorCon : IMultiValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is FoupCoverStateEnum && values[1] is bool)
                {
                    DockingPort40StateEnum DockingPort40State = (DockingPort40StateEnum)values[0];

                    CylinderStateEnum CylinderState = CylType.FoupCylinderType.FoupDockingPort40.State;

                    bool BlinkLED = (bool)values[1];

                    //Todo 3D model 색 표현 코드 구성
                    //if (CylinderState == CylinderStateEnum.RUNNING)
                    if (CylinderState == CylinderStateEnum.RUNNING)
                    {

                        switch (BlinkLED)
                        {

                            case true:
                                return Goldbrush;
                            case false:
                                return orangebrush;
                            default:
                                return mintCreambrush;
                        }

                    }
                    else
                    {
                        switch (DockingPort40State)
                        {
                            case DockingPort40StateEnum.IN:
                                return Goldbrush;
                            case DockingPort40StateEnum.OUT:
                                return LimeGreenbrush;
                            case DockingPort40StateEnum.ERROR:
                                return Redbrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }
    public class FoupPlateColorCon : IMultiValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is FoupCoverStateEnum && values[1] is bool)
                {
                    DockingPlateStateEnum DockingPlateState = (DockingPlateStateEnum)values[0];

                    CylinderStateEnum FoupDockingPlate12State = CylType.FoupCylinderType.FoupDockingPlate12.State;
                    CylinderStateEnum FoupDockingPlate8State = CylType.FoupCylinderType.FoupDockingPlate8.State;
                    CylinderStateEnum FoupDockingPlate6State = CylType.FoupCylinderType.FoupDockingPlate6.State;
                    bool CylinderState = false;
                    if ((FoupDockingPlate12State | FoupDockingPlate8State | FoupDockingPlate6State) == CylinderStateEnum.RUNNING)
                    {
                        CylinderState = true;
                    }
                    bool BlinkLED = (bool)values[1];

                    //Todo 3D model 색 표현 코드 구성
                    //if (CylinderState == CylinderStateEnum.RUNNING)
                    if (CylinderState == true)
                    {
                        switch (BlinkLED)
                        {
                            case true:
                                return Goldbrush;
                            case false:
                                return orangebrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                    else
                    {
                        switch (DockingPlateState)
                        {
                            case DockingPlateStateEnum.LOCK:
                                return Goldbrush;
                            case DockingPlateStateEnum.UNLOCK:
                                return LimeGreenbrush;
                            case DockingPlateStateEnum.ERROR:
                                return Redbrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }

    }
    public class FoupOpenerColorCon : IMultiValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is FoupCoverStateEnum && values[1] is bool)
                {

                    FoupCassetteOpenerStateEnum FoupOpener = (FoupCassetteOpenerStateEnum)values[0];

                    CylinderStateEnum CylinderState = CylType.FoupCylinderType.FoupRotator.State;

                    bool BlinkLED = (bool)values[1];

                    //Todo 3D model 색 표현 코드 구성
                    //if (CylinderState == CylinderStateEnum.RUNNING)
                    if (CylinderState == CylinderStateEnum.RUNNING)
                    {

                        switch (BlinkLED)
                        {

                            case true:
                                return Goldbrush;
                            case false:
                                return orangebrush;
                            default:
                                return mintCreambrush;
                        }

                    }
                    else
                    {
                        switch (FoupOpener)
                        {
                            case FoupCassetteOpenerStateEnum.UNLOCK:
                                return Goldbrush;
                            case FoupCassetteOpenerStateEnum.LOCK:
                                return LimeGreenbrush;
                            case FoupCassetteOpenerStateEnum.ERROR:
                                return Redbrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class FoupTiltColorCon : IMultiValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is FoupCoverStateEnum && values[1] is bool)
                {

                    FoupCassetteOpenerStateEnum FoupOpener = (FoupCassetteOpenerStateEnum)values[0];

                    CylinderStateEnum CylinderState = CylType.FoupCylinderType.FoupRotator.State;

                    bool BlinkLED = (bool)values[1];

                    //Todo 3D model 색 표현 코드 구성
                    //if (CylinderState == CylinderStateEnum.RUNNING)
                    if (CylinderState == CylinderStateEnum.RUNNING)
                    {

                        switch (BlinkLED)
                        {

                            case true:
                                return Goldbrush;
                            case false:
                                return orangebrush;
                            default:
                                return mintCreambrush;
                        }

                    }
                    else
                    {
                        switch (FoupOpener)
                        {
                            case FoupCassetteOpenerStateEnum.UNLOCK:
                                return Goldbrush;
                            case FoupCassetteOpenerStateEnum.LOCK:
                                return LimeGreenbrush;
                            case FoupCassetteOpenerStateEnum.ERROR:
                                return Redbrush;
                            default:
                                return mintCreambrush;
                        }
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupTiltColorCon8In : IValueConverter
    {
        static SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        static SolidColorBrush GREEN = new SolidColorBrush(Colors.Green);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        //static string Green = "#7AB357";//"#00A562";
        //static string Blue = "#013ec1"; //"#E23425";
        static string Red = "#E54B4B";  //"#E23425";
        static string Grey = "#939393";
        //static string Yellow = "#CED134";
        static string DarkGrey = "#464646";


        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if (value is TiltStateEnum)
                {
                    TiltStateEnum TiltState = (TiltStateEnum)value;
                    switch (TiltState)
                    {
                        case TiltStateEnum.UP:
                            return DarkGrey;
                        case TiltStateEnum.DOWN:
                            return Grey;
                        default:
                            return Red;
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class FoupPlateColorCon8In : IValueConverter
    {
        static SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        static SolidColorBrush GREEN = new SolidColorBrush(Colors.Green);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        //static string Green = "#7AB357";//"#00A562";
        //static string Blue = "#013ec1"; //"#E23425";
        static string Red = "#E54B4B";  //"#E23425";
        static string Grey = "#939393";
        //static string Purple = "#997995";
        //static string Yellow = "#CED134";
        static string DarkGrey = "#464646";


        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if (value is DockingPlateStateEnum)
                {
                    DockingPlateStateEnum PlateState = (DockingPlateStateEnum)value;
                    switch (PlateState)
                    {
                        case DockingPlateStateEnum.LOCK:
                            return Grey;
                        case DockingPlateStateEnum.UNLOCK:
                            return DarkGrey;
                        default:
                            return Red;
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class FoupPlateOPCon8In : IValueConverter
    {
        static double CST_DETACH = 0;
        static double CST_ATTACH = 0.4;
        static double CST_NOT_MATCHED = 0.1;


        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if (value is DockingPlateStateEnum)
                {
                    DockingPlateStateEnum PRESENCEState = (DockingPlateStateEnum)value;
                    switch (PRESENCEState)
                    {
                        case DockingPlateStateEnum.LOCK:
                            return CST_ATTACH;
                        case DockingPlateStateEnum.UNLOCK:
                            return CST_DETACH;
                        default:
                            return CST_NOT_MATCHED;
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }


    public class CSTOnDPOPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus WaferStatus = (EnumSubsStatus)value;
                    switch (WaferStatus)
                    {
                        case EnumSubsStatus.EXIST:
                            return (double)1;
                        case EnumSubsStatus.NOT_EXIST:
                            return (double)1;
                        default:
                            return (double)1;
                    }
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
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
            throw new Exception("Can't convert back");
        }
    }

    public class MultiColorConvert : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType,
                              object parameter, CultureInfo culture)
        {
            string strPos = "NULL";
            //CatCoordinates coord;
            double pos = 0d;
            EnumAxisConstants axisType;
            ICamera cam = null;

            try
            {
                if (values != null)
                {
                    cam = (ICamera)parameter;
                    axisType = (EnumAxisConstants)parameter;
                    //coord = cam.GetCurCoordPos();
                    switch (axisType)
                    {
                        case EnumAxisConstants.Undefined:
                            break;
                        case EnumAxisConstants.X:
                            // pos = coord.GetX();
                            break;
                        case EnumAxisConstants.Y:
                            // pos = coord.GetY();
                            break;
                        case EnumAxisConstants.Z:
                            // pos = coord.GetZ();
                            break;
                        case EnumAxisConstants.C:
                            // pos = coord.GetT();
                            break;
                        default:
                            break;
                    }
                    strPos = pos.ToString();
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, string.Format("FoupColorConverter::Convert error occurred."));
                LoggerManager.Exception(err);
            }
            return pos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                                    object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class FoupPlateLockManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DockingPlateStateEnum DockingPlateState = (DockingPlateStateEnum)value;

                switch (DockingPlateState)
                {
                    case DockingPlateStateEnum.LOCK:
                        return Goldbrush;
                    case DockingPlateStateEnum.UNLOCK:
                        return Silverbrush;
                    case DockingPlateStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupPlateUnlockManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DockingPlateStateEnum DockingPlateState = (DockingPlateStateEnum)value;

                switch (DockingPlateState)
                {
                    case DockingPlateStateEnum.LOCK:
                        return Silverbrush;
                    case DockingPlateStateEnum.UNLOCK:
                        return Goldbrush;
                    case DockingPlateStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupCoverUpManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupCoverStateEnum CoverState = (FoupCoverStateEnum)value;

                switch (CoverState)
                {
                    case FoupCoverStateEnum.CLOSE:
                        return Goldbrush;
                    case FoupCoverStateEnum.OPEN:
                        return Silverbrush;
                    case FoupCoverStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupCoverDownManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupCoverStateEnum CoverState = (FoupCoverStateEnum)value;

                switch (CoverState)
                {
                    case FoupCoverStateEnum.CLOSE:
                        return Silverbrush;
                    case FoupCoverStateEnum.OPEN:
                        return Goldbrush;
                    case FoupCoverStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupDockingPortInManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DockingPortStateEnum DockingPortState = (DockingPortStateEnum)value;

                switch (DockingPortState)
                {
                    case DockingPortStateEnum.IN:
                        return Goldbrush;
                    case DockingPortStateEnum.OUT:
                        return Silverbrush;
                    case DockingPortStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupDockingPortOutManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DockingPortStateEnum DockingPortState = (DockingPortStateEnum)value;

                switch (DockingPortState)
                {
                    case DockingPortStateEnum.IN:
                        return Silverbrush;
                    case DockingPortStateEnum.OUT:
                        return Goldbrush;
                    case DockingPortStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupDockingPort40InManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DockingPort40StateEnum DockingPort40State = (DockingPort40StateEnum)value;

                switch (DockingPort40State)
                {
                    case DockingPort40StateEnum.IN:
                        return Goldbrush;
                    case DockingPort40StateEnum.OUT:
                        return Silverbrush;
                    case DockingPort40StateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupDockingPort40OutManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DockingPort40StateEnum DockingPort40State = (DockingPort40StateEnum)value;

                switch (DockingPort40State)
                {
                    case DockingPort40StateEnum.IN:
                        return Silverbrush;
                    case DockingPort40StateEnum.OUT:
                        return Goldbrush;
                    case DockingPort40StateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupOpenerOpenManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupCassetteOpenerStateEnum FoupCassetteOpenerState = (FoupCassetteOpenerStateEnum)value;

                switch (FoupCassetteOpenerState)
                {
                    case FoupCassetteOpenerStateEnum.UNLOCK:
                        return Goldbrush;
                    case FoupCassetteOpenerStateEnum.LOCK:
                        return Silverbrush;
                    case FoupCassetteOpenerStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupOpenerCloseManualColorCon : IValueConverter
    {

        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Silverbrush = new SolidColorBrush(Colors.Silver);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupCassetteOpenerStateEnum FoupCassetteOpenerState = (FoupCassetteOpenerStateEnum)value;

                switch (FoupCassetteOpenerState)
                {
                    case FoupCassetteOpenerStateEnum.UNLOCK:
                        return Silverbrush;
                    case FoupCassetteOpenerStateEnum.LOCK:
                        return Goldbrush;
                    case FoupCassetteOpenerStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return Silverbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class CassetteOpacityValueCon : IValueConverter, INotifyPropertyChanged
    {
        private IFoupIOStates _FoupIOManager;
        public IFoupIOStates IOManager
        {

            get { return _FoupIOManager; }
            set
            {
                if (value != _FoupIOManager)
                {
                    _FoupIOManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                FoupCassetteOpenerStateEnum FoupCassetteOpenerState = (FoupCassetteOpenerStateEnum)value;
                var val = IOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT.Value;
                var val2 = IOManager.IOMap.Inputs.DI_C12IN_PRESENCE1.Value;
                var val3 = IOManager.IOMap.Inputs.DI_C12IN_PRESENCE2.Value;

                double OpacityVal = 0;

                if (val && val2 == true)
                {
                    OpacityVal = 0.3;
                }
                else if (val == false)
                {
                    OpacityVal = 1.0;
                }
                //switch (FoupCassetteOpenerState)
                //{
                //    case FoupCassetteOpenerStateEnum.UNLOCK:
                //        return Silverbrush;
                //    case FoupCassetteOpenerStateEnum.LOCK:
                //        return Goldbrush;
                //    case FoupCassetteOpenerStateEnum.ERROR:
                //        return Redbrush;
                //    default:
                //        return Silverbrush;
                //}
                return OpacityVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupLAMPColorCon : IValueConverter
    {

        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool LAMPState = (bool)value;


                switch (LAMPState)
                {
                    case true:
                        return Greenbrush;
                    case false:
                        return Yellowbrush;
                    default:
                        return Blackbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupPresenceLAMPColorCon : IValueConverter
    {

        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush Orangebrush = new SolidColorBrush(Colors.OrangeRed);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupPRESENCEStateEnum PresenceState = (FoupPRESENCEStateEnum)value;


                switch (PresenceState)
                {
                    case FoupPRESENCEStateEnum.CST_ATTACH:
                        return Greenbrush;
                    case FoupPRESENCEStateEnum.CST_DETACH:
                        return Yellowbrush;
                    case FoupPRESENCEStateEnum.CST_NOT_MATCHED:
                        return Orangebrush;
                    default:
                        return Blackbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupBusyLAMPColorCon : IValueConverter
    {

        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush Orangebrush = new SolidColorBrush(Colors.OrangeRed);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupPermissionStateEnum BusyState = (FoupPermissionStateEnum)value;


                switch (BusyState)
                {
                    case FoupPermissionStateEnum.BUSY:
                        return Greenbrush;
                    default:
                        return Yellowbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupAlarmLAMPColorCon : IValueConverter
    {

        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush Orangebrush = new SolidColorBrush(Colors.OrangeRed);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupStateEnum AlarmState = (FoupStateEnum)value;


                switch (AlarmState)
                {
                    case FoupStateEnum.ERROR:
                        return Orangebrush;
                    default:
                        return Yellowbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class FoupAutoLAMPColorCon : IValueConverter
    {

        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush Orangebrush = new SolidColorBrush(Colors.OrangeRed);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupPermissionStateEnum PresenceState = (FoupPermissionStateEnum)value;


                switch (PresenceState)
                {
                    case FoupPermissionStateEnum.AUTO:
                        return Greenbrush;
                    default:
                        return Yellowbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
