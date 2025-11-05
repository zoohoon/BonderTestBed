namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces.Foup;
    using System;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Globalization;


    public class Foup3D_PositionConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            double retVal = 0;

            try
            {
                if (value is DockingPlateStateEnum)
                {
                    if ((DockingPlateStateEnum)value == DockingPlateStateEnum.UNLOCK)
                    {
                        retVal = -180;
                    }
                    else
                    {
                        retVal = 0;
                    }
                }
                else if (value is DockingPortStateEnum)
                {
                    if ((DockingPortStateEnum)value == DockingPortStateEnum.IN)
                    {
                        retVal = 15;
                    }
                    else
                    {
                        retVal = 20;
                    }
                }
                else if (value is FoupCassetteOpenerStateEnum)
                {
                    if ((FoupCassetteOpenerStateEnum)value == FoupCassetteOpenerStateEnum.LOCK)
                    {
                        retVal = 90;
                    }
                    else
                    {
                        retVal = 0;
                    }
                }
                else if (value is FoupCoverStateEnum)
                {
                    if (parameter is string)
                    {
                        if (((string)parameter).Equals("0"))
                        {
                            if ((FoupCoverStateEnum)value == FoupCoverStateEnum.OPEN)
                            {
                                retVal = 20;
                            }
                            else
                            {
                                retVal = 25;
                            }
                        }
                        else if (((string)parameter).Equals("1"))
                        {
                            if ((FoupCoverStateEnum)value == FoupCoverStateEnum.OPEN)
                            {
                                retVal = -73;
                            }
                            else
                            {
                                retVal = -45;
                            }
                        }
                    }
                }
                else if (value is bool)
                {
                    if ((bool)value)
                    {
                        retVal = 1;
                    }
                    else
                    {
                        retVal = 0;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err, "Error occurred.");
            }
            return (double)retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ErrorRedFill : IValueConverter
    {
        SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
        SolidColorBrush WhiteColor = new SolidColorBrush(Colors.White);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DockingPlateStateEnum|| value is DockingPortStateEnum || value is FoupCoverStateEnum || value is FoupCassetteOpenerStateEnum)
            {
                string enumValue = "";
                if(value is DockingPlateStateEnum)
                {
                    enumValue=((DockingPlateStateEnum)value).ToString();
                }
                else if (value is DockingPortStateEnum)
                {
                    enumValue = ((DockingPortStateEnum)value).ToString();
                }
                else if(value is FoupCoverStateEnum)
                {
                    enumValue = ((FoupCoverStateEnum)value).ToString();
                }
                else if(value is FoupCassetteOpenerStateEnum)
                {
                    enumValue = ((FoupCassetteOpenerStateEnum)value).ToString();
                }

                if (enumValue.Equals("ERROR"))
                {
                    return RedColor;
                }
            }

            return WhiteColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CassetteFill : IValueConverter
    {
        SolidColorBrush LightSkyBlue = new SolidColorBrush(Colors.LightSkyBlue);
        SolidColorBrush LightGray = new SolidColorBrush(Colors.LightGray);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            if (value is bool)
            {
                if (((bool)value))
                {
                    return Colors.LightGreen;
                }
            }
            return Colors.LightCyan;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CassetteColorOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            double retVal = 0;

            try
            {
                if (value is FoupPRESENCEStateEnum)
                {
                    if (((FoupPRESENCEStateEnum)value)==FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        retVal = 0.5;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err, "Error occurred.");
            }
            return (double)retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CassetteBackColorOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            double retVal = 0;

            try
            {
                if (value is bool )
                {
                    if (((bool)value))
                    {
                        retVal = 0.1;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err, "Error occurred.");
            }
            return (double)retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
