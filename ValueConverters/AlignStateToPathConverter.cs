using LogModule;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ValueConverters
{
    public class AlignStateToPathFillConverter : IValueConverter
    {
        static SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush IdleBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush DoneBrush = new SolidColorBrush(Colors.LimeGreen);

        public AlignStateToPathFillConverter()
        {
        }
        object IValueConverter.Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                if (value != null)
                {
                    //AlignStateEnum alignstate = (AlignStateEnum)value;

                    String alignstate = (String)value;

                    switch (alignstate)
                    {
                        case "IDLE":
                            retval = IdleBrush;
                            break;
                        case "DONE":
                            retval = DoneBrush;
                            break;
                        default:
                            retval = DefaultBrush;
                            break;
                    }

                    //switch (alignstate)
                    //{
                    //    case AlignStateEnum.IDLE:
                    //        retval = IdleBrush;
                    //        break;
                    //    case AlignStateEnum.DONE:
                    //        retval = DoneBrush;
                    //        break;
                    //    default:
                    //        retval = DefaultBrush;
                    //        break;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class AlignStateToPathDataConverter : IValueConverter
    {
        static Geometry check = Geometry.Parse("M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z");
        static Geometry close = Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z");

        public AlignStateToPathDataConverter()
        {
        }
        object IValueConverter.Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            Geometry retval = null;

            try
            {
                if (value != null)
                {
                    //AlignStateEnum alignstate = (AlignStateEnum)value;
                    String alignstate = (String)value;

                    switch (alignstate)
                    {
                        case "IDLE":
                            retval = close;
                            break;
                        case "DONE":
                            retval = check;
                            break;
                        default:
                            retval = close;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

}
