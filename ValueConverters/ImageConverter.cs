using LogModule;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ValueConverters
{
    public class BinaryImageConverter : IValueConverter
    {
        object IValueConverter.Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                BitmapImage image = null;

                try
                {
                    if (value != null && value is byte[])
                    {
                        byte[] bytes = value as byte[];

                        using (MemoryStream stream = new MemoryStream(bytes))
                        {
                            image = new BitmapImage();
                            image.BeginInit();
                            stream.Seek(0, SeekOrigin.Begin);
                            image.StreamSource = stream;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.StreamSource = null;
                        }
                        return image;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug("[ValueConverter - BinaryImageConverter] Can't convert byte array to Image.");
                    LoggerManager.Exception(err);
                }

                return null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        object IValueConverter.ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class PositionAdjustmentConverter : IValueConverter
    {
        object IValueConverter.Convert(object value,
           Type targetType,
           object parameter,
           System.Globalization.CultureInfo culture)
        {
            try
            {
                double retVal = 0.0;
                double paramData = 0;
                if (value is double)
                {
                    bool paramResult = double.TryParse(parameter.ToString(), out paramData);
                    retVal = (double)value;
                    retVal += paramData;
                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        object IValueConverter.ConvertBack(object value,
           Type targetType,
           object parameter,
           System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class RelPosCalConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            string retStr = string.Empty;
            try
            {
                if (values != null && values.Length == 4)
                {
                    bool DataValidation = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (!((values[i] is double)
                            || (values[i] is Point)))
                        {
                            DataValidation = false;
                        }
                    }

                    if (DataValidation)
                    {
                        Point startPoint = (Point)values[0];
                        Point endPoint = (Point)values[1];
                        double xPosRate = (double)values[2];
                        double yPosRate = (double)values[3];

                        double xRealPos = (endPoint.X - startPoint.X) * xPosRate;
                        double yRealPos = (startPoint.Y - endPoint.Y) * yPosRate;

                        retStr = $"X = {xRealPos:#.00}, Y = {yRealPos:#.00}";
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retStr;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
