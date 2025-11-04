using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LogModule;

namespace ValueConverters
{

    public class LotSetupAutoFocusToStringConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            string strPos = null;

            try
            {
                strPos = value.ToString() + " Point";
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class StageLoaderErrorStateCon : IValueConverter
    {
        static String Error = "ERROR";
        static String Idle = "IDLE";



        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    bool ErrorState = (bool)value;
                    switch (ErrorState)
                    {
                        case true:
                            return Error;
                        case false:
                            return Idle;
                        default:
                            return Error;
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
}
