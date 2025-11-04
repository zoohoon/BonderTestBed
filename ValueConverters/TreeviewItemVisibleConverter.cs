using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using LogModule;

namespace ValueConverters
{
    public class TreeviewItemVisibleConverter : IValueConverter
    {
        

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            Visibility visible = (Visibility)value;
            try
            {
                if (value != null)
                {
                    if (visible == Visibility.Visible)
                    {

                    }
                    else if (visible == Visibility.Hidden)
                    {

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("Err = {0}", err.Message));
                LoggerManager.Exception(err);


                throw;
            }

            return visible;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
