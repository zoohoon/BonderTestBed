using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ValueConverters
{
    public class CellObjectForeGroundConverter : IValueConverter
    {
        static SolidColorBrush Connected = new SolidColorBrush(Colors.Green);
        static SolidColorBrush NotConnected = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        if (((bool)value))
                            return Connected;
                        else
                            return NotConnected;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class WaferOntheChuckToColorConverter : IValueConverter
    {
        static SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush ExistBrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush NotExistBrush = new SolidColorBrush(Colors.Red);

        public WaferOntheChuckToColorConverter()
        {
        }
        object IValueConverter.Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                if (value != null)
                {
                    EnumSubsStatus subsstatus = (EnumSubsStatus)value;

                    switch (subsstatus)
                    {
                        case EnumSubsStatus.UNKNOWN:
                            retval = DefaultBrush;
                            break;
                        case EnumSubsStatus.UNDEFINED:
                            retval = DefaultBrush;
                            break;
                        case EnumSubsStatus.NOT_EXIST:
                            retval = NotExistBrush;
                            break;
                        case EnumSubsStatus.EXIST:
                            retval = ExistBrush;
                            break;
                        default:
                            retval = DefaultBrush;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }


    public class ChillerNumColorConverter : IValueConverter
    {
        static SolidColorBrush YellowBrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush PurpleBrush = new SolidColorBrush(Colors.Purple);
        static SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush YellowGreenBrush = new SolidColorBrush(Colors.YellowGreen);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (value != null)
                {

                    if (value is Int32)
                    {
                        int val = (int)value ;
                        if (val==1)
                            return YellowBrush;
                        else if (val == 2)
                            return PurpleBrush;
                        else if(val == 3)
                            return OrangeBrush;
                        else if(val == 4)
                            return YellowBrush;
                        else
                            return YellowGreenBrush;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
