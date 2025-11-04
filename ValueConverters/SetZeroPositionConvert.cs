using LogModule;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ValueConverters
{
    public class SetZeroPositionConvert : IMultiValueConverter
    {
        

        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            int pos = 0;
            //EnumAxisConstants axisType;
            try
            {
                if ( (values != null) && (values.Length == 3) )
                {
                    bool enable = false;

                    if (values[2] is bool)
                    {
                        enable = (bool)values[2];
                    }

                    if(enable == true)
                    {
                        if (values[0] != DependencyProperty.UnsetValue
                            && values[1] != DependencyProperty.UnsetValue)
                        {
                            if ((double)values[1] != -1)
                            {

                                double curpos = (double)values[0];
                                double changepos = (double)values[1];

                                pos = (int)(changepos - curpos);
                            }
                        }
                    }
                    else
                    {
                        pos = 0;
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, string.Format("SetZeroPositionConvert::Convert error occurred."));
                LoggerManager.Exception(err);
                    
            }

            return pos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class SetXPositionConvert : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            double xshift = 0;
            try
            {
                if ((values != null))
                {
                    if (values[0] is double && values[1] is double && values[2] is double)
                    {
                        double xmachinecoord = 0;
                        double xsetfromcoord = 0;

                        xmachinecoord = (double)values[0];
                        xsetfromcoord = (double)values[1];
                        xshift = (double)values[2];
                        xshift = Math.Round(xmachinecoord - xsetfromcoord, 1);
                        xshift = Math.Round(xshift, 2);
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, string.Format("SetZeroPositionConvert::Convert error occurred."));
                LoggerManager.Exception(err);

            }

            return xshift;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class SetYPositionConvert : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            double yshift = 0;
            try
            {
                if ((values != null))
                {
                    if (values[0] is double && values[1] is double && values[2] is double)
                    {
                        double ymachinecoord = 0;
                        double ysetfromcoord = 0;

                        ymachinecoord = (double)values[0];
                        ysetfromcoord = (double)values[1];
                        yshift = (double)values[2];
                        yshift = Math.Round(ymachinecoord - ysetfromcoord, 1);
                        yshift = Math.Round(yshift, 2);
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, string.Format("SetZeroPositionConvert::Convert error occurred."));
                LoggerManager.Exception(err);

            }

            return yshift;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
