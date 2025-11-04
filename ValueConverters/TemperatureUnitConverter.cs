using LogModule;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class TemperatureUnitConverter : IValueConverter
    {
        public int UnitValue { get; set; }

        public TemperatureUnitConverter()
        {
            UnitValue = 1;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0;
            bool parseResult = false;

            try
            {
                parseResult = double.TryParse(value.ToString(), out retVal);

                if (parseResult == true)
                {
                    retVal /= UnitValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal.ToString("0.0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double retVal = 0;
            bool parseResult = false;

            try
            {
                parseResult = double.TryParse(value.ToString(), out retVal);

                if (parseResult == true)
                {
                    retVal *= UnitValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal.ToString("0.0");
        }

    }
}
