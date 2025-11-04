using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ProberInterfaces.Foup;
using LogModule;

namespace ValueConverters
{
    using ProberInterfaces;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    public class PositionProjectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                double projectedPos = 0d;

                if (value is double)
                {
                    double actPos = (double)value;
                    projectedPos = -234404 - actPos;
                    projectedPos = projectedPos / 1000 * -1;
                }
                return (double)projectedPos;
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
            return null;
        }
    }

    public class PosToTransPZConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                AxisObject axis;

                double projectedPos = 0d;
                double scale = 1.0d; ;
                double actPos = 0d;
                try
                {
                    if (value is AxisObject)
                    {

                        axis = (AxisObject)value;
                        actPos = (double)axis.Status.Position.Command - axis.Param.HomeOffset.Value;
                        projectedPos = actPos / 1000 * scale;
                    }
                    else if (value is double)
                    {
                        if (parameter != null)
                        {
                            double offset;


                            if (parameter is double)
                            {
                                if (double.TryParse((string)parameter, out offset))
                                {
                                    actPos = (double)value;
                                    projectedPos = actPos - offset;
                                    projectedPos = projectedPos / 1000;
                                }
                                if (parameter is double)
                                {
                                    offset = (double)parameter;
                                    actPos = (double)value;
                                    projectedPos = actPos - offset;
                                    projectedPos = projectedPos / 1000;

                                }
                            }
                            else if (parameter is string)
                            {
                                string paramStr = (string)parameter;
                                string[] convParams = paramStr.Split(',');
                                if (convParams.Count() > 1)
                                {
                                    if (double.TryParse((string)convParams[0], out offset) & double.TryParse((string)convParams[1], out scale))
                                    {
                                        actPos = (double)value;
                                        projectedPos = actPos - offset;
                                        projectedPos = projectedPos / scale;
                                    }
                                }
                                else
                                {
                                    if (double.TryParse((string)parameter, out offset))
                                    {
                                        actPos = (double)value;
                                        projectedPos = actPos - offset;
                                        projectedPos = projectedPos / 1000;
                                    }
                                }
                            }
                        }
                        else
                        {
                            actPos = (double)value;
                            projectedPos = actPos;
                            projectedPos = projectedPos / 1000;
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    //LoggerManager.Error($err, "Error occurred.");
                }
                return (double)projectedPos;
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
            return null;
        }
    }

    public class PosToTransConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                AxisObject axis;

                double projectedPos = 0d;
                double scale = 1.0d; ;
                double actPos = 0d;
                try
                {
                    if (value is AxisObject)
                    {

                        axis = (AxisObject)value;
                        actPos = (double)axis.Status.Position.Command - axis.Param.HomeOffset.Value;
                        projectedPos = actPos / 1000 * scale;
                    }
                    else if (value is double)
                    {
                        if (parameter != null)
                        {
                            double offset;


                            if (parameter is double)
                            {
                                if (double.TryParse((string)parameter, out offset))
                                {
                                    actPos = (double)value;
                                    projectedPos = actPos - offset;
                                    projectedPos = projectedPos / 1000;
                                }
                                if (parameter is double)
                                {
                                    offset = (double)parameter;
                                    actPos = (double)value;
                                    projectedPos = actPos - offset;
                                    projectedPos = projectedPos / 1000;

                                }
                            }
                            else if (parameter is string)
                            {
                                string paramStr = (string)parameter;
                                string[] convParams = paramStr.Split(',');
                                if (convParams.Count() > 1)
                                {
                                    if (double.TryParse((string)convParams[0], out offset) & double.TryParse((string)convParams[1], out scale))
                                    {
                                        actPos = (double)value;
                                        projectedPos = actPos - offset;
                                        projectedPos = projectedPos / scale;
                                    }
                                }
                                else
                                {
                                    if (double.TryParse((string)parameter, out offset))
                                    {
                                        actPos = (double)value;
                                        projectedPos = actPos - offset;
                                        projectedPos = projectedPos / 1000;
                                    }
                                }
                            }
                        }
                        else
                        {
                            actPos = (double)value;
                            projectedPos = actPos;
                            projectedPos = projectedPos / 1000;
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    //LoggerManager.Error($err, "Error occurred.");
                }
                return (double)projectedPos;
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
            return null;
        }
    }
    public class PosToRevTransConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            AxisObject axis;

            double projectedPos = 0d;
            double scale = 1.0d; ;
            double actPos = 0d;
            try
            {
                if (value is AxisObject)
                {
                    axis = (AxisObject)value;
                    actPos = (double)axis.Status.Position.Command - axis.Param.HomeOffset.Value;
                    projectedPos = actPos / 1000 * scale;
                }
                else if (value is double)
                {
                    if (parameter != null)
                    {
                        double offset;

                        if (parameter is double)
                        {
                            if (double.TryParse((string)parameter, out offset))
                            {
                                actPos = (double)value;
                                projectedPos = actPos - offset;
                                projectedPos = projectedPos / 1000 * -1d;
                            }
                            if (parameter is double)
                            {
                                offset = (double)parameter;
                                actPos = (double)value;
                                projectedPos = actPos - offset;
                                projectedPos = projectedPos / 1000 * -1d;
                            }
                        }
                        else if (parameter is string)
                        {
                            string paramStr = (string)parameter;
                            string[] convParams = paramStr.Split(',');
                            if (convParams.Count() > 1)
                            {
                                if (double.TryParse((string)convParams[0], out offset) & double.TryParse((string)convParams[1], out scale))
                                {
                                    actPos = (double)value;
                                    projectedPos = actPos - offset;
                                    projectedPos = projectedPos / scale * -1d;
                                }
                            }
                            else
                            {
                                if (double.TryParse((string)parameter, out offset))
                                {
                                    actPos = (double)value;
                                    projectedPos = actPos - offset;
                                    projectedPos = projectedPos / 1000 * -1d;
                                }
                            }
                        }

                    }
                    else
                    {
                        actPos = (double)value;
                        projectedPos = actPos;
                        projectedPos = projectedPos / 1000 * -1d;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err, "Error occurred.");
            }
            return (double)projectedPos;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class BoolToPosConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                double pos = 0d;
                double tPos, fPos;
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (parameter is string)
                    {
                        string paramStr = (string)parameter;
                        string[] convParams = paramStr.Split(',');
                        if (convParams.Count() > 1)
                        {
                            if (double.TryParse((string)convParams[0], out tPos) & double.TryParse((string)convParams[1], out fPos))
                            {
                                if (state) pos = tPos;
                                else pos = fPos;
                            }
                        }
                        else
                        {
                            pos = 0d;
                        }
                    }
                }
                return pos;
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
            return null;
        }
    }


    public class ThreeLegToPosConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {
                        pos = 8;
                    }
                    else
                    {
                        pos = 0;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }


            return pos;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class StateToPosConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (parameter is string)
                    {
                        string paramStr = (string)parameter;
                        string[] convParams = paramStr.Split(',');
                        double stateTruePos = 0d;
                        double stateFalsePos = 0d;
                        if (convParams.Count() > 1)
                        {
                            if (double.TryParse((string)convParams[0], out stateTruePos) & double.TryParse((string)convParams[1], out stateFalsePos))
                            {
                                if (state == true)
                                {
                                    pos = stateTruePos;
                                }
                                else
                                {
                                    pos = stateFalsePos;
                                }
                            }
                        }
                        else
                        {
                            pos = 0d;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class BoolToColorConverter : IValueConverter
    {
        private static SolidColorBrush DisableBrush = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush EnableBrush = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush BusyBrush = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush ErrorBrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            SolidColorBrush stateBrush;
            try
            {
                stateBrush = ErrorBrush;
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {
                        stateBrush = BusyBrush;
                    }
                    else
                    {
                        stateBrush = DisableBrush;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }


            return stateBrush;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class WaferStateToColorConverter : IValueConverter
    {
        private static SolidColorBrush DisableBrush = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush EnableBrush = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush BusyBrush = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush ErrorBrush = new SolidColorBrush(Colors.Red);
        static string Green = "#129c06";//"#00A562";
        static string Blue = "#013ec1"; //"#E23425";
        static string Red = "#e32e00";  //"#E23425";

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            SolidColorBrush stateBrush;
            try
            {
                stateBrush = ErrorBrush;
                try
                {
                    if (value is EnumWaferState)
                    {
                        EnumWaferState WaferState = (EnumWaferState)value;
                        switch (WaferState)
                        {
                            case EnumWaferState.UNDEFINED:
                                return DependencyProperty.UnsetValue;
                            case EnumWaferState.PROCESSED:
                                return Blue;
                            case EnumWaferState.UNPROCESSED:
                                return Green;
                            case EnumWaferState.MISSED:
                                return Red;
                            default:
                                return Green;
                        }
                    }
                    else
                    {
                        return DependencyProperty.UnsetValue;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return stateBrush;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class InspectionWaferColorConverter : IValueConverter
    {
        //static string Green = "#129c06";//"#00A562";
        static string Blue = "#013ec1"; //"#E23425";
        static string Red = "#e32e00";  //"#E23425";
        static string Black = "#000000";

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            string stateBrush = Red;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {
                        stateBrush = Blue;
                    }
                    else
                    {
                        stateBrush = Black;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return stateBrush;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class ProbingStateToColorConverter : IValueConverter
    {
        private static SolidColorBrush DisableBrush = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush EnableBrush = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush BusyBrush = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush ErrorBrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            SolidColorBrush stateBrush;
            try
            {
                stateBrush = ErrorBrush;
                if (value is EnumProbingState)
                {
                    EnumProbingState state = (EnumProbingState)value;
                    switch (state)
                    {
                        case EnumProbingState.IDLE:
                            stateBrush = DisableBrush;
                            break;
                        case EnumProbingState.RUNNING:
                        case EnumProbingState.SUSPENDED:
                        case EnumProbingState.PINPADMATCHPERFORM:
                        case EnumProbingState.PINPADMATCHED:
                            stateBrush = DisableBrush;
                            break;
                        case EnumProbingState.ZUPPERFORM:
                            stateBrush = ErrorBrush;
                            break;
                        case EnumProbingState.ZUP:
                            stateBrush = ErrorBrush;
                            break;
                        case EnumProbingState.ZDNPERFORM:
                        case EnumProbingState.ZDN:
                            stateBrush = DisableBrush;
                            break;
                        case EnumProbingState.ERROR:
                        case EnumProbingState.DONE:
                        case EnumProbingState.ABORTED:
                        case EnumProbingState.PAUSED:
                        default:
                            stateBrush = DisableBrush;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return stateBrush;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MotorStateToColorConverter : IValueConverter
    {
        private static SolidColorBrush DisableBrush = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush EnableBrush = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush BusyBrush = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush ErrorBrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            SolidColorBrush stateBrush;
            try
            {
                stateBrush = ErrorBrush;
                if (value is EnumAxisState)
                {
                    EnumAxisState axisState = (EnumAxisState)value;

                    //if(axis.Status.AxisBusy == true)
                    //{
                    //    stateBrush = BusyBrush;
                    //}
                    //else if(axis.Status.AxisEnabled == true)
                    //{
                    //    stateBrush = EnableBrush;
                    //}
                    //else if (axis.Status.ErrCode != 0)
                    //{
                    //    stateBrush = ErrorBrush;
                    //}
                    switch (axisState)
                    {
                        case EnumAxisState.INVALID:
                            stateBrush = ErrorBrush;
                            break;
                        case EnumAxisState.IDLE:
                            stateBrush = EnableBrush;
                            break;
                        case EnumAxisState.MOVING:
                            stateBrush = BusyBrush;
                            break;
                        case EnumAxisState.STOPPING:
                            stateBrush = BusyBrush;
                            break;
                        case EnumAxisState.STOPPED:
                            stateBrush = EnableBrush;
                            break;
                        case EnumAxisState.STOPPING_ERROR:
                            stateBrush = ErrorBrush;
                            break;
                        case EnumAxisState.ERROR:
                            stateBrush = ErrorBrush;
                            break;
                        case EnumAxisState.DISABLED:
                            stateBrush = DisableBrush;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }


            return stateBrush;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class DockingPortToPosConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is DockingPortStateEnum)
                {

                    DockingPortStateEnum state = (DockingPortStateEnum)value;
                    if (state == DockingPortStateEnum.IN)
                    {
                        pos = 0;
                    }
                    else if (state == DockingPortStateEnum.OUT)
                    {
                        pos = 8;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class DockingPort40ToPosConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is DockingPort40StateEnum)
                {

                    DockingPort40StateEnum state = (DockingPort40StateEnum)value;
                    if (state == DockingPort40StateEnum.IN)
                    {
                        pos = -4;
                    }
                    else if (state == DockingPort40StateEnum.OUT)
                    {
                        pos = 0;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FoupCoverToPosConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is FoupCoverStateEnum)
                {

                    FoupCoverStateEnum state = (FoupCoverStateEnum)value;
                    if (state == FoupCoverStateEnum.CLOSE)
                    {
                        pos = 0;
                    }
                    else if (state == FoupCoverStateEnum.OPEN)
                    {
                        pos = -16;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class CSTPORT8_3D_Model : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Hidden;
            try
            {
                if (value is FoupTypeEnum)
                {

                    FoupTypeEnum name = (FoupTypeEnum)value;
                    if (name == FoupTypeEnum.CST8PORT_FLAT)
                    {
                        visibility = Visibility.Visible;
                    }
                    else
                    {
                        visibility = Visibility.Hidden;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return visibility;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class TOP_LOADER_3D_Model : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Hidden;
            try
            {
                if (value is FoupTypeEnum)
                {

                    FoupTypeEnum name = (FoupTypeEnum)value;
                    if (name == FoupTypeEnum.TOP)
                    {
                        visibility = Visibility.Visible;
                    }
                    else
                    {
                        visibility = Visibility.Hidden;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return visibility;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class WaferCamMoveColCon : IMultiValueConverter
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
                if (values[0] is CylinderStateEnum && values[1] is bool)
                {
                    CylinderStateEnum WaferCamMove = (CylinderStateEnum)values[0];


                    bool BlinkLED = (bool)values[1];

                    if (WaferCamMove == CylinderStateEnum.RUNNING)
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
                        switch (WaferCamMove)
                        {
                            case CylinderStateEnum.EXTEND:
                                return Goldbrush;
                            case CylinderStateEnum.RETRACT:
                                return LimeGreenbrush;
                            case CylinderStateEnum.ERROR:
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
    public class WaferCamMoveCon : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {

                        pos = 400;

                    }
                    else
                    {
                        pos = 0;
                    }
                }
                else
                {
                    pos = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class CAM3DPOSCon : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {

                        pos = 400;

                    }
                    else
                    {
                        pos = 0;
                    }
                }
                else
                {
                    pos = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class ChuckAngleRotateAxisConv : IMultiValueConverter
    {
        double z1 = 0;
        double z2 = 0;
        double z3 = 0;

        object lockObj = new object();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Vector3D retVal = new Vector3D();

            try
            {
                if (values[0] is ZGroupAxes)
                {
                    var tmp = values[0] as ZGroupAxes;

                    double[] zAxisArray = new double[3];
                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];
                    zAxisArray[0] = tmp.Z0Pos;
                    zAxisArray[1] = tmp.Z1Pos;
                    zAxisArray[2] = tmp.Z2Pos;



                    string paramStr = (string)parameter;
                    if (z1 != zAxisArray[0])
                    {
                        z1 = zAxisArray[0];
                        retVal = new Vector3D(1, 0, 0);
                    }
                    else if (z2 != zAxisArray[1])
                    {
                        z2 = zAxisArray[1];
                        retVal = new Vector3D(-1, 0, 1.73);
                    }
                    else if (z3 != zAxisArray[2])
                    {
                        z3 = zAxisArray[2];
                        retVal = new Vector3D(1, 0, 1.73);
                    }

                }
                if (values[0] is double)
                {
                    double rAngle = (double)values[0];
                    var xAxisVector = new Vector3D(1, 0, 0);

                    Matrix3D m = Matrix3D.Identity;
                    Quaternion q = new Quaternion(new Vector3D(0, 1, 0), rAngle);
                    m.Rotate(q);
                    retVal = m.Transform(xAxisVector);
                }
                if (values[0] is int)
                {
                    int rAngle = (int)values[0];
                    var xAxisVector = new Vector3D(1, 0, 0);

                    Matrix3D m = Matrix3D.Identity;
                    Quaternion q = new Quaternion(new Vector3D(0, 1, 0), 180d - (rAngle + 90d) * -1d);
                    m.Rotate(q);
                    retVal = m.Transform(xAxisVector);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ChuckAngleRotateAngleConv : IMultiValueConverter
    {
        double z1 = 0;
        double z2 = 0;
        double z3 = 0;
        double degree = 0d;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {

                if (values[0] is ZGroupAxes)
                {
                    var tmp = values[0] as ZGroupAxes;

                    double[] zAxisArray = new double[3];
                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];
                    zAxisArray[0] = tmp.Z0Pos;
                    zAxisArray[1] = tmp.Z1Pos;
                    zAxisArray[2] = tmp.Z2Pos;
                    //double[] zAxisArray = new double[3];
                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];


                    double multiple = 1;

                    if (z1 != zAxisArray[0])
                    {
                        z1 = zAxisArray[0] / 1000;//mm 전환
                        degree = -(Math.Sin(z1 / 110) * multiple);// 1000배수


                    }
                    else if (z2 != zAxisArray[1])
                    {
                        z2 = zAxisArray[1] / 1000;//mm 전환
                        degree = -(Math.Sin(z2 / 110) * multiple);

                    }
                    else if (z3 != zAxisArray[2])
                    {
                        z3 = zAxisArray[2] / 1000;//mm 전환
                        degree = (Math.Sin(z3 / 110) * multiple);

                    }
                }
                if (values[0] is double)
                {
                    double tHeight = (double)values[0];
                    degree = Math.Asin(tHeight / 150000d);
                    if (values.Count() > 1)
                    {
                        if (values[1] is double)
                        {

                            double multiplier = (double)values[1];
                            degree = degree * multiplier;
                        }
                        if (values[1] is int)
                        {
                            double multiplier = (int)values[1];
                            degree = degree * multiplier;
                        }
                    }
                }
                return (double)degree;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class ChuckAxisCenterXConv : IMultiValueConverter
    {
        double z1 = 0;
        double z2 = 0;
        double z3 = 0;

        object lockObj = new object();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0;

            try
            {
                if (values[0] is ZGroupAxes)
                {
                    var tmp = values[0] as ZGroupAxes;

                    double[] zAxisArray = new double[3];
                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];
                    zAxisArray[0] = tmp.Z0Pos;
                    zAxisArray[1] = tmp.Z1Pos;
                    zAxisArray[2] = tmp.Z2Pos;

                    //double[] zAxisArray = new double[3];

                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];
                    string paramStr = (string)parameter;
                    if (z1 != zAxisArray[0])
                    {
                        z1 = zAxisArray[0];

                        retVal = 0;
                    }
                    else if (z2 != zAxisArray[1])
                    {
                        z2 = zAxisArray[1];
                        retVal = 88.3;
                    }
                    else if (z3 != zAxisArray[2])
                    {
                        z3 = zAxisArray[2];
                        retVal = -88.3;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class ChuckAxisCenterZConv : IMultiValueConverter
    {
        double z1 = 0;
        double z2 = 0;
        double z3 = 0;

        object lockObj = new object();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0;

            try
            {
                if (values[0] is ZGroupAxes)
                {
                    var tmp = values[0] as ZGroupAxes;

                    double[] zAxisArray = new double[3];
                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];
                    zAxisArray[0] = tmp.Z0Pos;
                    zAxisArray[1] = tmp.Z1Pos;
                    zAxisArray[2] = tmp.Z2Pos;
                    //double[] zAxisArray = new double[3];
                    //zAxisArray[0] = (double)values[0];
                    //zAxisArray[1] = (double)values[1];
                    //zAxisArray[2] = (double)values[2];
                    string paramStr = (string)parameter;
                    if (z1 != zAxisArray[0])
                    {
                        z1 = zAxisArray[0];
                        retVal = -76;
                    }
                    else if (z2 != zAxisArray[1])
                    {
                        z2 = zAxisArray[1];
                        retVal = 0;
                    }
                    else if (z3 != zAxisArray[2])
                    {
                        z3 = zAxisArray[2];
                        retVal = 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class LoaderDoorMoveCon : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {

                        pos = -150;

                    }
                    else
                    {
                        pos = 0;

                    }
                }
                else
                {
                    pos = 0;
                }
                //if (value is CylinderStateEnum.EXTEND)
                //{

                //    pos = 700000;

                //}
                //else if (value is CylinderStateEnum.RETRACT)
                //{
                //    pos = -700000;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class InspectionMoveCon : IValueConverter
    {

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            double pos = 0d;
            try
            {
                if (value is bool)
                {
                    bool state = (bool)value;
                    if (state == true)
                    {

                        pos = 240;

                    }
                    else
                    {
                        pos = 0;
                    }
                }
                else
                {
                    pos = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pos;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
