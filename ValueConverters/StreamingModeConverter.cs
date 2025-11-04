using System;

namespace ValueConverters
{
    using LogModule;
    using ProberInterfaces;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    public class StreamingBtnVisiablityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Hidden;
            try
            {
                if (value is GPCellModeEnum)
                {
                    GPCellModeEnum state = (GPCellModeEnum)value;
                    switch (state)
                    {
                        case GPCellModeEnum.OFFLINE:
                            visibility = Visibility.Hidden;
                            break;
                        case GPCellModeEnum.ONLINE:
                            visibility = Visibility.Visible;
                            break;
                        case GPCellModeEnum.MAINTENANCE:
                            visibility = Visibility.Hidden;
                            break;
                        default:
                            break;  
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StreamingBtnTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = "";
            try
            {
                if (value is StreamingModeEnum)
                {
                    StreamingModeEnum state = (StreamingModeEnum)value;
                    switch (state)
                    {
                        case StreamingModeEnum.INVALID:
                            break;
                        case StreamingModeEnum.UNDEFINED:
                            break;
                        case StreamingModeEnum.STREAMING_ON:
                            str = "STREAMING_OFF";
                            break;
                        case StreamingModeEnum.STREAMING_OFF:
                            str = "STREAMING_ON";
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
