using System;
using System.Globalization;
using System.Windows.Data;
using ProberInterfaces;
using System.Windows;
using System.Windows.Media;
using LogModule;

namespace ValueConverters
{
    public class WaferOnCSTConverter : IValueConverter
    {
        static SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        static SolidColorBrush GREEN = new SolidColorBrush(Colors.Green);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static string Green = "#129c06";//"#00A562";
        static string Blue = "#013ec1"; //"#E23425";
        static string Red = "#e32e00";  //"#E23425";


        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            try
            {
                if (value is EnumWaferState)
                {
                    EnumWaferState WaferState = (EnumWaferState)value;
                    switch (WaferState)
                    {
                        case EnumWaferState.PROCESSED:
                            return Blue;
                        case EnumWaferState.UNPROCESSED:
                            return Green;
                        case EnumWaferState.MISSED:
                            return Red;
                        default:
                            return Red;
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

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }
    public class WaferOnCSTOPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {

            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus WaferStatus = (EnumSubsStatus)value;
                    switch (WaferStatus)
                    {
                        case EnumSubsStatus.EXIST:
                            return (double)1;
                        case EnumSubsStatus.NOT_EXIST:
                            return (double)1;
                        default:
                            return (double)1;
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

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }

    public class WaferOnCSTTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {

            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus WaferStatus = (EnumSubsStatus)value;
                    switch (WaferStatus)
                    {
                        case EnumSubsStatus.EXIST:
                            return (string)"Wafer&#10;On";
                        case EnumSubsStatus.NOT_EXIST:
                            return (string)"No Wafer&#10;On";
                        default:
                            return (string)"";
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

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }
    public class WaferOnCSTColConverter : IValueConverter
    {

        static string Red = "#e32e00";  //"#E23425";
        static string Gr = "#d6d8d8";

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
       {

            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus WaferStatus = (EnumSubsStatus)value;
                    switch (WaferStatus)
                    {
                        case EnumSubsStatus.EXIST:
                            return Red;
                        case EnumSubsStatus.NOT_EXIST:
                            return Gr;
                        default:
                            return Gr;
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

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }
    public class WaferOnCSTButtonConverter : IValueConverter
    {

        static string Load = "Load";  //"#E23425";
        static string Unload = "Unload";

        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {

            try
            {
                if (value is EnumSubsStatus)
                {
                    EnumSubsStatus WaferStatus = (EnumSubsStatus)value;
                    switch (WaferStatus)
                    {
                        case EnumSubsStatus.EXIST:
                            return Unload;
                        case EnumSubsStatus.NOT_EXIST:
                            return Load;
                        default:
                            return Unload;
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

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }


}
