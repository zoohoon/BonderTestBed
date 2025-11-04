using LogModule;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
    public class AuthLevelConverter : IMultiValueConverter
    {
        

        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            bool isEnabled = true;
            try
            {              
                if (values[0] != null && values[1] != null)
                {
                    if (values[0] is int && values[1] is int)
                    {
                        int maskingLevel = (int)values[0];
                        int currLevel = (int)values[1];

                        if (currLevel <= maskingLevel)
                        {
                            isEnabled = true;
                        }
                        else if (currLevel > maskingLevel)
                        {
                            isEnabled = true;                            
                        }
                    }
                }                
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("Err = {0}", err.Message));
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return isEnabled;

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class LevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isEnabled = true;
                if (parameter != null && value != null)
                {
                    if (parameter is string && value is int)
                    {
                        int currLevel = int.Parse(parameter.ToString());
                        int maskingLevel = (int)value;

                        if (currLevel <= maskingLevel)
                        {
                            isEnabled = true;
                        }
                        else if (currLevel > maskingLevel)
                        {
                            isEnabled = false;
                        }
                    }
                }
                return isEnabled;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                bool isEnabled = true;
                if (values[0] != null && values[1] != null)
                {
                    if (values[0] is int && values[1] is int)
                    {
                        int maskingLevel = (int)values[0];
                        int currLevel = (int)values[1];

                        if (currLevel <= maskingLevel)
                        {
                            isEnabled = true;
                        }
                        else if (currLevel > maskingLevel)
                        {
                            isEnabled = false;
                        }
                    }
                }
                return isEnabled;
            }
            catch(Exception err)
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
