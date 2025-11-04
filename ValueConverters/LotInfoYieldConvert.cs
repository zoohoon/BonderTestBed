using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class LotInfoStartTimeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            string str = null;
            try
            {
                DateTime date;
                EnumWaferState WaferState;
                if (values[0] is DateTime && values[1] is EnumWaferState)
                {
                    date = (DateTime)values[0];
                    WaferState = (EnumWaferState)values[1];
                    if (WaferState == EnumWaferState.PROCESSED || WaferState == EnumWaferState.PROBING || WaferState == EnumWaferState.TESTED )
                    {
                        str = date.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
                    }
                    else
                    {
                        str = "-";
                    }
                }
                else
                {
                    str = "-";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class LotInfoYieldConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            string str = null;
            try
            {
                double yield;
                EnumWaferState WaferState;
                if (values[0] is double && values[1] is EnumWaferState)
                {
                    yield = (double)values[0];
                    WaferState = (EnumWaferState)values[1];
                    if (WaferState == EnumWaferState.PROCESSED || WaferState == EnumWaferState.PROBING || WaferState == EnumWaferState.TESTED)
                    {
                        str = yield.ToString();
                    }
                    else
                    {
                        str = "-";
                    }
                }
                else
                {
                    str = "-";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class LotInfoEndTimeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            string str=null;
            try
            {
                DateTime date;
                EnumWaferState WaferState;
                if (values[0] is DateTime && values[1] is EnumWaferState)
                {
                     date = (DateTime)values[0];
                     WaferState = (EnumWaferState)values[1];
                    if(WaferState==EnumWaferState.PROCESSED)
                    {
                        str = date.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
                    }
                    else
                    {
                        str = "-";
                    }
                }
                else
                {
                    str = "-";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class LotTimeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            string str = null;
            try
            {
                DateTime date;
                bool enable;
                if (values[0] is DateTime && values[1] is bool)
                {
                    date = (DateTime)values[0];
                    enable = (bool)values[1];
                    if (enable)
                    {
                        str = date.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
                    }
                    else
                    {
                        str = "";
                    }
                }
                else
                {
                    str = "";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return str;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
