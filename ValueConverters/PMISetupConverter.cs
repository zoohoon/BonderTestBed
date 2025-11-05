using LogModule;
using ProberInterfaces;
using ProberInterfaces.PMI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ValueConverters
{
    public class PMIEdgeOffsetModeToVisibilityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Hidden;

            try
            {
                PAD_EDGE_OFFSET_MODE mode = (PAD_EDGE_OFFSET_MODE)value;

                if (mode == PAD_EDGE_OFFSET_MODE.ENABLE)
                {
                    retval = Visibility.Visible;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PMICornerRadiusModeToVisibilityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Hidden;

            try
            {
                PAD_CORNERRADIUS_MODE mode = (PAD_CORNERRADIUS_MODE)value;

                if (mode == PAD_CORNERRADIUS_MODE.ENABLE)
                {
                    retval = Visibility.Visible;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PMIColorIntToEnumStringConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string EnumString = string.Empty;

            try
            {
                PAD_COLOR color = (PAD_COLOR)value;

                EnumString = Enum.GetName((color.GetType()), value);

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"PMIColorIntToEnumStringConvert Convert() Exceoption");
                LoggerManager.Exception(err);
            }

            return EnumString;
        }
        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PMISetupModeToBrushConvert : IValueConverter
    {
        static SolidColorBrush Whitebrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush Goldbrush = new SolidColorBrush(Colors.Gold);

        public PMI_SETUP_MODE CorrectMode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                PMI_SETUP_MODE mode = (PMI_SETUP_MODE)value;

                SolidColorBrush brush = null;

                if (CorrectMode == PMI_SETUP_MODE.MARK)
                {
                    if ((mode == PMI_SETUP_MODE.MARKMIN) || (mode == PMI_SETUP_MODE.MARKMAX))
                    {
                        brush = Goldbrush;
                    }
                    else
                    {
                        brush = Whitebrush;
                    }
                }
                else
                {
                    brush = (mode == CorrectMode) ? Goldbrush : Whitebrush;
                }

                return brush;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class TabSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                TabControl tabControl = values[0] as TabControl;
                double width = tabControl.ActualWidth / tabControl.Items.Count;
                //Subtract 1, otherwise we could overflow to two rows.
                return (width <= 1) ? 0 : (width - 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
