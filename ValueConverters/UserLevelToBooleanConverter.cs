using AccountModule;
using LogModule;
using System;
using System.Windows.Data;

namespace ValueConverters
{
    public class UserLevelToBooleanConverter : IValueConverter
    {
        // TODO: 파라미터로 제어할 수 있도록

        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool retval = false;

            try
            {
                int level = (int)value;

                if(level == AccountManager.SuperUserLevel)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
