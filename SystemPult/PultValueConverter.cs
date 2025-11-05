using System;
using LogModule;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProberInterfaces;

namespace SystemPult
{
    public class PortStateToColorCon : IValueConverter
    {
        private static Brush RedBrush = new SolidColorBrush(Colors.Red);
        private static Brush GrnBrush = new SolidColorBrush(Colors.LimeGreen);
        private static Brush GryBrush = new SolidColorBrush(Colors.DimGray);
        private static Brush WhiteBrush = new SolidColorBrush(Colors.WhiteSmoke);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool Toggle = false;

                if (value is bool)
                {
                    Toggle = (bool)value;
                }
                else if(value is int)
                {
                    int iValue = (int)value;

                    if(iValue == 0)
                    {
                        Toggle = false;
                    }
                    else
                    {
                        Toggle = true;
                    }
                }

                switch (Toggle)
                {
                    case true:
                        return GrnBrush;
                    case false:
                        return WhiteBrush;
                    default:
                        return RedBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RedBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class PortTypeToColorCon : IValueConverter
    {
        private static Brush RedBrush = new SolidColorBrush(Colors.Red);
        private static Brush LimeGrnBrush = new SolidColorBrush(Colors.LimeGreen);
        private static Brush GryBrush = new SolidColorBrush(Colors.DimGray);
        private static Brush WhiteBrush = new SolidColorBrush(Colors.WhiteSmoke);
        private static Brush CrimsonBrush = new SolidColorBrush(Colors.Crimson);
        private static Brush BlueBrush = new SolidColorBrush(Colors.Blue);
        private static Brush BlueVioletBrush = new SolidColorBrush(Colors.BlueViolet);
        private static Brush VioletBrush = new SolidColorBrush(Colors.Violet);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                EnumIOType enumIOType = EnumIOType.UNDEFINED;

                if (value is EnumIOType)
                {
                    enumIOType = (EnumIOType)value;
                }
                switch (enumIOType)
                {
                    case EnumIOType.UNDEFINED:
                        return CrimsonBrush;
                    case EnumIOType.INPUT:
                        return LimeGrnBrush;
                    case EnumIOType.OUTPUT:
                        return RedBrush;
                    case EnumIOType.BIDIRECTION:
                        return VioletBrush;
                    case EnumIOType.AI:
                        return BlueVioletBrush;
                    case EnumIOType.AO:
                        return BlueBrush;
                    case EnumIOType.MEMORY:
                    case EnumIOType.INT:
                    case EnumIOType.CNT:
                    default:
                        return RedBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RedBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
