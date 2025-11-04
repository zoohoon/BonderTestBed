using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ValueConverters
{
    using LogModule;
    using System.Windows;

    //public class LogTypeToBoolConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        if (Enum.IsDefined(value.GetType(), value) == false)
    //            return DependencyProperty.UnsetValue;

    //        object parameterValue = Enum.Parse(value.GetType(), parameterString);

    //        return parameterValue.Equals(value);
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }
    //    #endregion
    //}

    //public class LogTypeToVisibilityForFilterConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        Visibility retval = Visibility.Hidden;

    //        if (value is ProberLogType && targetType == typeof(Visibility))
    //        {
    //            switch (value)
    //            {
    //                case ProberLogType.PROLOG:
    //                    break;
    //                case ProberLogType.DEBUG:
    //                    break;
    //                case ProberLogType.FILTEREDDEBUG:
    //                    retval = Visibility.Visible;
    //                    break;
    //                case ProberLogType.EVENT:
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }
    //    #endregion
    //}
    public class LogTypeToVisibilityConverter : IValueConverter
    {
        public bool IsDataGrid { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility retval = Visibility.Hidden;

                if (value is ProberLogLevel && targetType == typeof(Visibility))
                {
                    switch (value)
                    {
                        case ProberLogLevel.PROLOG:

                            if (IsDataGrid == true)
                            {
                                retval = Visibility.Visible;
                            }
                            else
                            {
                                retval = Visibility.Collapsed;
                            }

                            break;
                        case ProberLogLevel.DEBUG:

                            if (IsDataGrid == true)
                            {
                                retval = Visibility.Collapsed;
                            }
                            else
                            {
                                retval = Visibility.Visible;
                            }

                            break;
                        case ProberLogLevel.FILTEREDDEBUG:

                            if (IsDataGrid == true)
                            {
                                retval = Visibility.Collapsed;
                            }
                            else
                            {
                                retval = Visibility.Collapsed;
                            }

                            break;
                        case ProberLogLevel.EVENT:

                            if (IsDataGrid == true)
                            {
                                retval = Visibility.Visible;
                            }
                            else
                            {
                                retval = Visibility.Collapsed;
                            }

                            break;
                        default:
                            break;
                    }
                }

                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;

            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }

    //public class LogLevelToVisibilityForPrologButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        Visibility retval = Visibility.Hidden;

    //        if (value is ProberLogLevel && targetType == typeof(Visibility))
    //        {
    //            switch (value)
    //            {
    //                case ProberLogLevel.PROLOG:
    //                    retval = Visibility.Visible;
    //                    break;
    //                case ProberLogLevel.DEBUG:
    //                    break;
    //                case ProberLogLevel.FILTEREDDEBUG:
    //                    break;
    //                case ProberLogLevel.EVENT:
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }
    //    #endregion
    //}

    //public class LogLevelToVisibilityForEventlogButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        Visibility retval = Visibility.Hidden;

    //        if (value is ProberLogLevel && targetType == typeof(Visibility))
    //        {
    //            switch (value)
    //            {
    //                case ProberLogLevel.PROLOG:
    //                    break;
    //                case ProberLogLevel.DEBUG:
    //                    break;
    //                case ProberLogLevel.FILTEREDDEBUG:
    //                    break;
    //                case ProberLogLevel.EVENT:
    //                    retval = Visibility.Visible;
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }
    //    #endregion
    //}

    //public class LogLevelToVisibilityForDebugButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        Visibility retval = Visibility.Hidden;

    //        if (value is ProberLogLevel && targetType == typeof(Visibility))
    //        {
    //            switch (value)
    //            {
    //                case ProberLogLevel.PROLOG:
    //                    break;
    //                case ProberLogLevel.DEBUG:
    //                    retval = Visibility.Visible;
    //                    break;
    //                case ProberLogLevel.FILTEREDDEBUG:
    //                    break;
    //                case ProberLogLevel.EVENT:
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }
    //    #endregion
    //}

    //public class LogLevelToVisibilityForFilteredDebugButtonConverter : IValueConverter
    //{
    //    #region IValueConverter Members
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        Visibility retval = Visibility.Hidden;

    //        if (value is ProberLogLevel && targetType == typeof(Visibility))
    //        {
    //            switch (value)
    //            {
    //                case ProberLogLevel.PROLOG:
    //                    break;
    //                case ProberLogLevel.DEBUG:
    //                    break;
    //                case ProberLogLevel.FILTEREDDEBUG:
    //                    retval = Visibility.Visible;
    //                    break;
    //                case ProberLogLevel.EVENT:
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;

    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }
    //    #endregion
    //}

    public class LogLevelToVisibilityForMessageConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility retval = Visibility.Hidden;

                if (value is ProberLogLevel && targetType == typeof(Visibility))
                {
                    switch (value)
                    {
                        case ProberLogLevel.PROLOG:
                            retval = Visibility.Visible;
                            break;
                        case ProberLogLevel.DEBUG:
                            break;
                        case ProberLogLevel.FILTEREDDEBUG:
                            break;
                        case ProberLogLevel.EVENT:
                            retval = Visibility.Visible;
                            break;
                        default:
                            break;
                    }
                }

                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string parameterString = parameter as string;

                if (parameterString == null)
                    return DependencyProperty.UnsetValue;

                return Enum.Parse(targetType, parameterString);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion
    }

    public class LogTypeToImageConverter : IValueConverter
    {
        protected String GetImageSource(object type)
        {
            String imagePath = String.Empty;

            try
            {
                if (type is PrologType)
                {
                    switch (type)
                    {
                        case PrologType.UNDEFINED:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                        case PrologType.INFORMATION:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Info_Image01.png";
                            break;
                        case PrologType.OPERRATION_ALARM:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Alarm_Image01.png";
                            break;
                        case PrologType.SYSTEM_FAULT:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                        default:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                    }
                }
                else if (type is DebuglogType)
                {
                    switch (type)
                    {
                        case DebuglogType.UNDEFINED:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                        case DebuglogType.DEBUG:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Info_Image01.png";
                            break;
                        default:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                    }
                }
                else if (type is EventlogType)
                {
                    switch (type)
                    {
                        case EventlogType.UNDEFINED:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                        case EventlogType.EVENT:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Event_Image01.png";
                            break;
                        default:
                            imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                            break;
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }


            return imagePath;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage retval = null;

            try
            {
                String str = GetImageSource(value);

                //Uri uri = new Uri(str;

                if(str != "")
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(str, UriKind.RelativeOrAbsolute);
                    bmp.EndInit();
                    bmp.StreamSource = null;
                    retval = bmp;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return new BitmapImage(new Uri(imgPath);

            //return new BitmapImage(new Uri((string)retval));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LogLevelToHeaderIconImageConverter : IValueConverter
    {
        protected String GetImageSource(ProberLogLevel level)
        {
            String imagePath = String.Empty;

            try
            {
                switch (level)
                {
                    case ProberLogLevel.UNDEFINED:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                        break;
                    case ProberLogLevel.PROLOG:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Prolog_Image3.png";
                        break;
                    case ProberLogLevel.EVENT:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Eventlog_Image2.png";
                        break;
                    case ProberLogLevel.DEBUG:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Debuglog_Image0.png";
                        break;
                    case ProberLogLevel.FILTEREDDEBUG:
                        imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return imagePath;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage retval = null;

            try
            {
                if (value is ProberLogLevel)
                {
                    ProberLogLevel tmp = (ProberLogLevel)value;

                    String str = GetImageSource(tmp);

                    //Uri uri = new Uri(str;

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(str, UriKind.RelativeOrAbsolute);
                    bmp.EndInit();
                    bmp.StreamSource = null;
                    retval = bmp;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class LogTagToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if ((value != null) && (value is List<string>))
                {
                    List<string> tmp = value as List<string>;

                    if ((tmp != null) && (tmp.Count > 0))
                    {
                        retval = string.Join(",", tmp.ToArray());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LogLevelToVisibilityConverter : IValueConverter
    {
        public LogLevelToVisibilityConverter()
        {
            Correctlevel1 = ProberLogLevel.UNDEFINED;
            Correctlevel2 = ProberLogLevel.UNDEFINED;
        }
        public ProberLogLevel Correctlevel1 { get; set; }
        public ProberLogLevel Correctlevel2 { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility retval = Visibility.Hidden;

            try
            {
                if (value is ProberLogLevel && targetType == typeof(Visibility))
                {
                    ProberLogLevel tmp = (ProberLogLevel)value;

                    if (((Correctlevel1 != ProberLogLevel.UNDEFINED) && (tmp == Correctlevel1)) ||
                        ((Correctlevel2 != ProberLogLevel.UNDEFINED) && (tmp == Correctlevel2))
                        )
                    {
                        retval = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;

            try
            {
                if (parameterString == null)
                    return DependencyProperty.UnsetValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }

    //public class LogLevelToImageConverter : IValueConverter
    //{
    //    protected String GetImageSource(LogLevel loglevel)
    //    {
    //        String imagePath = String.Empty;

    //        switch (loglevel)
    //        {
    //            case LogLevel.UNDEFINED:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
    //                break;
    //            case LogLevel.PROLOG_INFORMATION:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Info_Image01.png";
    //                break;
    //            case LogLevel.PROLOG_OPERATION_ALARM:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Alarm_Image01.png";
    //                break;
    //            case LogLevel.PROLOG_SYSTEM_FAULT:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
    //                break;
    //            case LogLevel.DEBUGLOG:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Info_Image01.png";
    //                break;
    //            case LogLevel.EVENTLOG:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Event_Image01.png";
    //                break;
    //            default:
    //                imagePath = "pack://application:,,,/ImageResourcePack;component/Images/Fault_Image01.png";
    //                break;
    //        }
    //        return imagePath;
    //    }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        BitmapImage retval = null;

    //        try
    //        {
    //            if (value is LogLevel)
    //            {
    //                LogLevel tmpvalue = (LogLevel)value;

    //                String str = GetImageSource(tmpvalue);

    //                //Uri uri = new Uri(str;

    //                var bmp = new BitmapImage();
    //                bmp.BeginInit();
    //                bmp.CacheOption = BitmapCacheOption.OnLoad;
    //                bmp.UriSource = new Uri(str, UriKind.RelativeOrAbsolute);
    //                bmp.EndInit();

    //                retval = bmp;
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retval;

    //        //return new BitmapImage(new Uri(imgPath);

    //        //return new BitmapImage(new Uri((string)retval));
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}


    //public class LogLevelToPathDataConverter : IValueConverter
    //{
    //    static Geometry alert = Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z");

    //    static Geometry alert_box = Geometry.Parse("M5,3H19A2,2 0 0,1 21,5V19A2,2 0 0,1 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3M13,13V7H11V13H13M13,17V15H11V17H13Z");
    //    static Geometry alert_circle = Geometry.Parse("M13,13H11V7H13M13,17H11V15H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
    //    static Geometry alert_circle_outline = Geometry.Parse("M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z");
    //    static Geometry alert_decagram = Geometry.Parse("M23,12L20.56,9.22L20.9,5.54L17.29,4.72L15.4,1.54L12,3L8.6,1.54L6.71,4.72L3.1,5.53L3.44,9.21L1,12L3.44,14.78L3.1,18.47L6.71,19.29L8.6,22.47L12,21L15.4,22.46L17.29,19.28L20.9,18.46L20.56,14.78L23,12M13,17H11V15H13V17M13,13H11V7H13V13Z");
    //    static Geometry alert_octagon = Geometry.Parse("M13,13H11V7H13M12,17.3A1.3,1.3 0 0,1 10.7,16A1.3,1.3 0 0,1 12,14.7A1.3,1.3 0 0,1 13.3,16A1.3,1.3 0 0,1 12,17.3M15.73,3H8.27L3,8.27V15.73L8.27,21H15.73L21,15.73V8.27L15.73,3Z");
    //    static Geometry alert_octagram = Geometry.Parse("M2.2,16.06L3.88,12L2.2,7.94L6.26,6.26L7.94,2.2L12,3.88L16.06,2.2L17.74,6.26L21.8,7.94L20.12,12L21.8,16.06L17.74,17.74L16.06,21.8L12,20.12L7.94,21.8L6.26,17.74L2.2,16.06M13,17V15H11V17H13M13,13V7H11V13H13Z");
    //    static Geometry alert_outline = Geometry.Parse("M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16");

    //    static Geometry information = Geometry.Parse("M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
    //    static Geometry information_outline = Geometry.Parse("M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z");

    //    static Geometry help_circle = Geometry.Parse("M15.07,11.25L14.17,12.17C13.45,12.89 13,13.5 13,15H11V14.5C11,13.39 11.45,12.39 12.17,11.67L13.41,10.41C13.78,10.05 14,9.55 14,9C14,7.89 13.1,7 12,7A2,2 0 0,0 10,9H8A4,4 0 0,1 12,5A4,4 0 0,1 16,9C16,9.88 15.64,10.67 15.07,11.25M13,19H11V17H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12C22,6.47 17.5,2 12,2Z");
    //    static Geometry help_circle_outline = Geometry.Parse("M11,18H13V16H11V18M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,6A4,4 0 0,0 8,10H10A2,2 0 0,1 12,8A2,2 0 0,1 14,10C14,12 11,11.75 11,15H13C13,12.75 16,12.5 16,10A4,4 0 0,0 12,6Z");

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        Geometry retval = null;

    //        if (value is LogLevel)
    //        {
    //            LogLevel tmpvalue = (LogLevel)value;

    //            switch (tmpvalue)
    //            {   
    //                case LogLevel.UNDEFINED:
    //                    retval = help_circle_outline;
    //                    break;
    //                case LogLevel.PROLOG_INFORMATION:
    //                    retval = information;
    //                    break;
    //                case LogLevel.PROLOG_OPERATION_ALARM:
    //                    retval = alert;
    //                    break;
    //                case LogLevel.PROLOG_SYSTEM_FAULT:
    //                    retval = alert_circle;
    //                    break;
    //                case LogLevel.DEBUGLOG:
    //                    retval = information;
    //                    break;
    //                case LogLevel.EVENTLOG:
    //                    retval = alert_circle;
    //                    break;
    //                default:
    //                    retval = help_circle_outline;
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class LogLevelToFillBrushConverter : IValueConverter
    //{
    //    public static LinearGradientBrush MakeLGB(Point StartPoint, Point EndPoint, System.Windows.Media.Color Color1, System.Windows.Media.Color Color2, double offset1, double offset2)
    //    {
    //        LinearGradientBrush gradient = new LinearGradientBrush();

    //        gradient.StartPoint = StartPoint;
    //        gradient.EndPoint = EndPoint;

    //        GradientStop color1 = new GradientStop();
    //        color1.Color = Color1;
    //        color1.Offset = offset1;
    //        gradient.GradientStops.Add(color1);

    //        GradientStop color2 = new GradientStop();
    //        color2.Color = Color2;
    //        color2.Offset = offset2;
    //        gradient.GradientStops.Add(color2);

    //        return gradient;
    //    }

    //    static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
    //    static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
    //    static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
    //    static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
    //    static SolidColorBrush Crimsonbrush = new SolidColorBrush(Colors.Crimson);
    //    static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
    //    static SolidColorBrush Balckbrush = new SolidColorBrush(Colors.Black);
    //    static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
    //    static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
    //    static SolidColorBrush Bluebrush = new SolidColorBrush(Colors.Blue);
    //    static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
    //    static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
    //    static SolidColorBrush DeepSkyBluebrush = new SolidColorBrush(Colors.DeepSkyBlue);
    //    static SolidColorBrush NavajoWhitebrush = new SolidColorBrush(Colors.NavajoWhite);
    //    static SolidColorBrush OrangeRedbrush = new SolidColorBrush(Colors.OrangeRed); 

    //    static LinearGradientBrush GrayLGB = MakeLGB(new Point(0, 0.5), new Point(1, 0.5), Colors.Black, System.Windows.Media.Color.FromArgb(100, 69, 87, 186), 0, 1);

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        System.Windows.Media.Brush retval = null;

    //        if (value is LogLevel)
    //        {
    //            LogLevel tmpvalue = (LogLevel)value;

    //            switch (tmpvalue)
    //            {
    //                case LogLevel.UNDEFINED:
    //                    retval = Greenbrush;
    //                    break;
    //                case LogLevel.PROLOG_INFORMATION:
    //                    retval = DeepSkyBluebrush;
    //                    break;
    //                case LogLevel.PROLOG_OPERATION_ALARM:
    //                    retval = Yellowbrush;
    //                    break;
    //                case LogLevel.PROLOG_SYSTEM_FAULT:
    //                    retval = Redbrush;
    //                    break;
    //                case LogLevel.DEBUGLOG:
    //                    retval = DeepSkyBluebrush;
    //                    break;
    //                case LogLevel.EVENTLOG:
    //                    retval = OrangeRedbrush;
    //                    break;
    //                default:
    //                    retval = Greenbrush;
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class LogLevelToStrokeBrushConverter : IValueConverter
    //{
    //    public static LinearGradientBrush MakeLGB(Point StartPoint, Point EndPoint, System.Windows.Media.Color Color1, System.Windows.Media.Color Color2, double offset1, double offset2)
    //    {
    //        LinearGradientBrush gradient = new LinearGradientBrush();

    //        gradient.StartPoint = StartPoint;
    //        gradient.EndPoint = EndPoint;

    //        GradientStop color1 = new GradientStop();
    //        color1.Color = Color1;
    //        color1.Offset = offset1;
    //        gradient.GradientStops.Add(color1);

    //        GradientStop color2 = new GradientStop();
    //        color2.Color = Color2;
    //        color2.Offset = offset2;
    //        gradient.GradientStops.Add(color2);

    //        return gradient;
    //    }

    //    static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
    //    static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
    //    static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
    //    static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
    //    static SolidColorBrush Crimsonbrush = new SolidColorBrush(Colors.Crimson);
    //    static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Yellow);
    //    static SolidColorBrush Balckbrush = new SolidColorBrush(Colors.Black);
    //    static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
    //    static SolidColorBrush Greenbrush = new SolidColorBrush(Colors.Green);
    //    static SolidColorBrush Bluebrush = new SolidColorBrush(Colors.Blue);
    //    static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
    //    static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
    //    static SolidColorBrush DeepSkyBluebrush = new SolidColorBrush(Colors.Gray);
    //    static SolidColorBrush NavajoWhitebrush = new SolidColorBrush(Colors.NavajoWhite);
    //    static SolidColorBrush OrangeRedbrush = new SolidColorBrush(Colors.OrangeRed);

    //    static LinearGradientBrush GrayLGB = MakeLGB(new Point(0, 0.5), new Point(1, 0.5), Colors.Black, System.Windows.Media.Color.FromArgb(100, 69, 87, 186), 0, 1);

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        System.Windows.Media.Brush retval = null;

    //        if (value is LogLevel)
    //        {
    //            LogLevel tmpvalue = (LogLevel)value;

    //            switch (tmpvalue)
    //            {
    //                case LogLevel.UNDEFINED:
    //                    retval = Goldbrush;
    //                    break;
    //                case LogLevel.PROLOG_INFORMATION:
    //                    retval = NavajoWhitebrush;
    //                    break;
    //                case LogLevel.PROLOG_OPERATION_ALARM:
    //                    retval = Blackbrush;
    //                    break;
    //                case LogLevel.PROLOG_SYSTEM_FAULT:
    //                    retval = NavajoWhitebrush;
    //                    break;
    //                case LogLevel.DEBUGLOG:
    //                    retval = NavajoWhitebrush;
    //                    break;
    //                case LogLevel.EVENTLOG:
    //                    retval = NavajoWhitebrush;
    //                    break;
    //                default:
    //                    retval = Goldbrush;
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class LogLevelToStringConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string retval = null;

    //        if (value is LogLevel)
    //        {
    //            LogLevel tmpvalue = (LogLevel)value;

    //            switch (tmpvalue)
    //            {
    //                case LogLevel.UNDEFINED:
    //                    break;
    //                case LogLevel.PROLOG_INFORMATION:
    //                    retval = "Unknown";
    //                    break;
    //                case LogLevel.PROLOG_OPERATION_ALARM:
    //                    retval = "Unknown";
    //                    break;
    //                case LogLevel.PROLOG_SYSTEM_FAULT:
    //                    retval = "Unknown";
    //                    break;
    //                case LogLevel.DEBUGLOG:
    //                    retval = "Unknown";
    //                    break;
    //                case LogLevel.EVENTLOG:
    //                    retval = "Unknown";
    //                    break;
    //                default:
    //                    retval = "Unknown";
    //                    break;
    //            }
    //        }

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

}
