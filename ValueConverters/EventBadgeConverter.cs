using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LogModule;

namespace ValueConverters
{
    public class EventBadgeBackGroundBrushConverter : BaseConverter, IValueConverter
    {
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is int)
                {
                    int intValue = (int)value;

                    if (intValue > 0)
                    {
                        return Redbrush;
                    }
                    else
                    {
                        return Transparentbrush;
                    }
                }

                return Transparentbrush;
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
            throw new NotImplementedException();
        }

        private static bool IsSupportedType(object value)
        {
            try
            {
                return value is int || value is double || value is byte || value is long ||
       value is float || value is uint || value is short || value is sbyte ||
       value is ushort || value is ulong || value is decimal;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class EventBadgeForeGroundBrushConverter : BaseConverter, IValueConverter
    {
        static SolidColorBrush NavajoWhitebrush = new SolidColorBrush(Colors.NavajoWhite);
        static SolidColorBrush Transparentbrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is int)
                {
                    int intValue = (int)value;

                    if (intValue > 0)
                    {
                        return NavajoWhitebrush;
                    }
                    else
                    {
                        return Transparentbrush;
                    }
                }

                return Transparentbrush;
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
            throw new NotImplementedException();
        }

        private static bool IsSupportedType(object value)
        {
            try
            {
                return value is int || value is double || value is byte || value is long ||
       value is float || value is uint || value is short || value is sbyte ||
       value is ushort || value is ulong || value is decimal;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
