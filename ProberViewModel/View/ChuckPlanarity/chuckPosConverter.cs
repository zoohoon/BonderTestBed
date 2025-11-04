using LogModule;
using ProberInterfaces.Enum;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ChuckPlanarityView
{
    //public class ChuckPosConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        double retVal = 0;

    //        ObservableCollection<ChuckPos> chukcposlist = null;
    //        EnumChuckPosition currentenum;

    //        try
    //        {
    //            if (values.Length >= 2)
    //            {
    //                if (values[0] is ObservableCollection<ChuckPos> && values[1] is EnumChuckPosition)
    //                {
    //                    chukcposlist = values[0] as ObservableCollection<ChuckPos>;
    //                    currentenum = (EnumChuckPosition)values[1];

    //                    ChuckPos tmp = chukcposlist.FirstOrDefault(x => x.ChuckPosEnum == currentenum);

    //                    if (tmp != null)
    //                    {
    //                        retVal = tmp.ZPos;
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retVal;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class SpechCheckConverter : IMultiValueConverter
    {
        static SolidColorBrush SpecInBrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush SpecOutBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush UndefindBrush = new SolidColorBrush(Colors.Gray);


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retVal = UndefindBrush;

            try
            {
                if (values.Length >= 2)
                {
                    double CurrentValue;
                    double SpecValue;

                    if (values[0] is double && values[1] is double)
                    {
                        CurrentValue = (double)values[0];
                        SpecValue = (double)values[1];

                        if (CurrentValue < (SpecValue * 2) )
                        {
                            retVal = SpecInBrush;
                        }
                        else
                        {
                            retVal = SpecOutBrush;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrentChuckPosConverter : IMultiValueConverter
    {
        static SolidColorBrush CorrentBrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush IncorrectBrush = new SolidColorBrush(Colors.Transparent);
        static SolidColorBrush UndefindBrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retVal = UndefindBrush;

            try
            {
                if (values.Length >= 2)
                {
                    EnumChuckPosition CurrentValue;
                    EnumChuckPosition PosValue;

                    if (values[0] is EnumChuckPosition && values[1] is EnumChuckPosition)
                    {
                        CurrentValue = (EnumChuckPosition)values[0];
                        PosValue = (EnumChuckPosition)values[1];

                        if (CurrentValue == PosValue)
                        {
                            retVal = CorrentBrush;
                        }
                        else
                        {
                            retVal = IncorrectBrush;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringLengthToMarginConverter : IValueConverter
    {
        static Thickness UnknownLength = new Thickness();
        static Thickness Length1 = new Thickness(-13, 2, 0, 0);
        static Thickness Length2 = new Thickness(-8.5, 0, 0, 0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness retval = UnknownLength;

            try
            {
                string inputValue = (string)value;

                if (inputValue.Length == 1)
                {
                    if(inputValue == "F")
                    {
                        retval = new Thickness(-13, 2, 0, 0);
                    }
                    else
                    {
                        retval = Length1;
                    }
                }
                else if (inputValue.Length == 2)
                {
                    retval = Length2;
                }
                else if (inputValue.Length == 3 || inputValue.Length == 4) // Like 15.8
                {
                    retval = new Thickness(-13, 0, 0, 0);
                }
                else
                {
                    retval = UnknownLength;
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
            throw new NotImplementedException();
        }
    }

    public class DiffStringToBrushConverter : IValueConverter
    {
        static SolidColorBrush PlusBrush = new SolidColorBrush(Colors.Blue);
        static SolidColorBrush MinusBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush ZeroBrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush FailBrush = new SolidColorBrush(Colors.White);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retVal = ZeroBrush;

            try
            {
                string inputValue = (string)value;

                double inputdoublevalue = 0;

                if (double.TryParse(inputValue, out inputdoublevalue))
                {
                    if (inputdoublevalue > 0)
                    {
                        retVal = PlusBrush;
                    }
                    else if (inputdoublevalue < 0)
                    {
                        retVal = MinusBrush;
                    }
                    else
                    {
                        retVal = ZeroBrush;
                    }
                }
                else
                {
                    if (inputValue == "F")
                    {
                        retVal = FailBrush;
                    }
                    else
                    {
                        retVal = ZeroBrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class FailPtStringToBrushConver : IValueConverter
    {
        static SolidColorBrush FailPoint = new SolidColorBrush(Colors.OrangeRed);
        static SolidColorBrush NormalPoint = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retVal = NormalPoint;

            try
            {
                string inputValue = (string)value;

                double inputdoublevalue = 0;

                if (double.TryParse(inputValue, out inputdoublevalue))
                {
                    retVal = NormalPoint;
                }
                else
                {
                    if(inputValue == "F")
                    {
                        retVal = FailPoint;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BorderThincknessConver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int retVal = 0;
            try
            {
                if ((bool)value)
                {
                    retVal = 3;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
