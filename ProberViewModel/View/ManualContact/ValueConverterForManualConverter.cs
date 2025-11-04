using System;

namespace ProberViewModel
{
    using ProberInterfaces;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class IsSetAllContactConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retString = "";
            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue)
                {
                    retString = "Cancel Contact";
                }
                else
                {
                    retString = "Set Contact";
                }
            }

            return retString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           return false;
        }
    }

    public class BoolReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retResult = false;
            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue)
                {
                    retResult = false;
                }
                else
                {
                    retResult = true;
                }
            }

            return retResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class GetObjectNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retResult = "";
            retResult = value.GetType().Name;
            return retResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string color = "Red";

            if(value is ModuleStateEnum)
            {
                ModuleStateEnum state = (ModuleStateEnum)value;

                if(state == ModuleStateEnum.DONE)
                {
                    color = "Green";
                }
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }


    public class ZStateToBorderBrushConverter : IValueConverter
    {
        static SolidColorBrush ZUPbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush ZDOWNbrush = new SolidColorBrush(Colors.Gray);
        static SolidColorBrush Defaultbrush = new SolidColorBrush(Colors.White);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue)
                {
                    return ZUPbrush;
                    //retResult = false;
                }
                else
                {
                    return ZDOWNbrush;
                    //retResult = true;
                }
            }

            return Defaultbrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class ZStateToChuckIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //bool retResult = false;
            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue)
                {
                    return ResourceAccessor.Get(Properties.Resources.ChuckUp);
                    //retResult = false;
                }
                else
                {
                    return ResourceAccessor.Get(Properties.Resources.ChuckDown);
                    //retResult = true;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }


    public class ZStateToBtnContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //bool retResult = false;
            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue)
                {
                    //retResult = false;
                    return "Z Down";
                }
                else
                {
                    //retResult = true;
                    return "Z Up";
                }
            }

            //return retResult;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
