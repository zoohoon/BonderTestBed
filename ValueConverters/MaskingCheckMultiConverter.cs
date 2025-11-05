using System;
using System.Windows.Data;
using System.Globalization;
using LogModule;
using AccountModule;
using System.Windows.Media;
using System.Windows;

namespace ValueConverters
{
    public class MaskingCheckMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            try
            {
                int maskingLevel = values[0] is int ? (int)values[0] : AccountManager.MAX_USER_LEVEL;
                int userLevel = values[1] is int ? (int)values[1] : AccountManager.MIN_USER_LEVEL;
                
                if (AccountManager.IsUserLevelAboveThisNum(maskingLevel, userLevel))
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MaskingMultiConverterWithBoolVal : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;
            try
            {
                bool isEnabled = values[0] is bool ? (bool)values[0] : false;
                int maskingLevel = values[1] is int ? (int)values[1] : AccountManager.MAX_USER_LEVEL;
                int userLevel = values[2] is int ? (int)values[2] : AccountManager.MIN_USER_LEVEL;

                if (AccountManager.IsUserLevelAboveThisNum(maskingLevel, userLevel))
                {
                    if(isEnabled == true)
                    {
                        retVal = true;
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MaskingVisiblilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retVisibility = Visibility.Visible;
            try
            {
                if (values != null && 2 <= values.Length)
                {
                    int maskingLevel = values[0] is int ? (int)values[0] : AccountManager.MAX_USER_LEVEL;
                    int userLevel = values[1] is int ? (int)values[1] : AccountManager.MIN_USER_LEVEL;

                    if (maskingLevel < 0 && !AccountManager.IsUserLevelAboveThisNum(maskingLevel, userLevel))
                    {
                        retVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        retVisibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVisibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MaskingVisiblilityMultiConverterWithVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retVisibility = Visibility.Visible;
            try
            {
                Visibility visibility = values[0] is Visibility ? (Visibility)values[0] : Visibility.Visible;
                int maskingLevel = values[1] is int ? (int)values[1] : AccountManager.MAX_USER_LEVEL;
                int userLevel = values[2] is int ? (int)values[2] : AccountManager.MIN_USER_LEVEL;

                if(visibility == Visibility.Visible)
                {
                    if (maskingLevel < 0 && !AccountManager.IsUserLevelAboveThisNum(maskingLevel, userLevel))
                    {
                        retVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        retVisibility = Visibility.Visible;
                    }
                }
                else
                {
                    retVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVisibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LockingColorChangerUsingMasking : IMultiValueConverter
    {
        private static readonly SolidColorBrush defaultColor = new SolidColorBrush(Color.FromArgb(255, 0xfe, 0xbe, 0x38));
        private static readonly SolidColorBrush maskingColor = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xbf, 0xff));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retColor = defaultColor;

            try
            {
                if(2 <= values.Length)
                {
                    int maskingLevel = values[0] is int ? (int)values[0] : AccountManager.MAX_USER_LEVEL;
                    int userLevel = values[1] is int ? (int)values[1] : AccountManager.MIN_USER_LEVEL;

                    if (!AccountManager.IsUserLevelAboveThisNum(maskingLevel, userLevel))
                    {
                        retColor = maskingColor;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
