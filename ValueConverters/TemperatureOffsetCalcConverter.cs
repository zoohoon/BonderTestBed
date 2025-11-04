using LogModule;
using System;
using System.Windows.Data;

namespace ValueConverters
{
    public class TemperatureOffsetCalcConverter : IMultiValueConverter
    {
        public int UnitValue { get; set; }

        public TemperatureOffsetCalcConverter()
        {
            UnitValue = 1;
        }

        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            string str = null;

            try
            {
                if( (values[0] is double) && (values[1] is double) )
                {
                    double StandardTempVal = (double)values[0];
                    double CorrectTempVal = (double)values[1];

                    double offset;

                    offset = (double)((decimal)StandardTempVal - (decimal)CorrectTempVal);
                    offset = offset / UnitValue;

                    str = offset.ToString("0.0");
                }
                else
                {
                    str = string.Empty;
                }

                //if (values[0] is DateTime && values[1] is EnumWaferState)
                //{
                //    date = (DateTime)values[0];
                //    WaferState = (EnumWaferState)values[1];
                //    if (WaferState == EnumWaferState.PROCESSED || WaferState == EnumWaferState.PROBING)
                //    {
                //        str = date.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
                //    }
                //    else
                //    {
                //        str = "-";
                //    }
                //}
                //else
                //{
                //    str = "-";
                //}
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
