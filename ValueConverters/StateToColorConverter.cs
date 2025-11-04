using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LogModule;
using ProberInterfaces;

namespace ValueConverters
{
    public class StateToColorConverter : IValueConverter
    {
        static SolidColorBrush mintCreambrush = new SolidColorBrush(Colors.MintCream);
        static SolidColorBrush orangebrush = new SolidColorBrush(Colors.Orange);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush AprilArborBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6C7460"));

        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                switch (val)
                {
                    case ModuleStateEnum.IDLE:
                        return orangebrush;
                    case ModuleStateEnum.PAUSED:
                        return orangebrush;
                    case ModuleStateEnum.RUNNING:
                        return LimeGreenbrush;
                    case ModuleStateEnum.ABORT:
                        return AprilArborBrush;
                    case ModuleStateEnum.ERROR:
                        return Redbrush;
                    default:
                        return LimeGreenbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class DeviceLabelBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    bool val = (bool)value;
                    if (val)
                    {
                        return Brushes.White;
                    }
                    else
                    {
                        return Brushes.Red;
                    }

                }

                return Brushes.White;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Brushes.White;
        }
    }
}
