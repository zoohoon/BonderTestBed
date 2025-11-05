using ProberInterfaces.Foup;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LogModule;
using System.Windows;

namespace ValueConverters
{
    public class FoupProcedureStateToBrushConvert : IValueConverter
    {
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Graybrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                FoupProcedureStateEnum state = (FoupProcedureStateEnum)value;

                switch (state)
                {
                    case FoupProcedureStateEnum.IDLE:
                        return Graybrush;
                    case FoupProcedureStateEnum.PreSafetyError:
                    case FoupProcedureStateEnum.BehaviorError:
                    case FoupProcedureStateEnum.PostSafetyError:
                        return Redbrush;
                    case FoupProcedureStateEnum.DONE:
                        return Blackbrush;
                    default:
                        return Graybrush;
                }
            }
            catch(Exception err)
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
    public class FoupProcedureStateToTextBrushConvert : IValueConverter
    {
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Graybrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                FoupProcedureStateEnum state = (FoupProcedureStateEnum)value;

                switch (state)
                {
                    case FoupProcedureStateEnum.IDLE:
                        return Blackbrush;
                    case FoupProcedureStateEnum.PreSafetyError:
                    case FoupProcedureStateEnum.BehaviorError:
                    case FoupProcedureStateEnum.PostSafetyError:
                        return Redbrush;
                    case FoupProcedureStateEnum.DONE:
                        return Graybrush;
                    default:
                        return Blackbrush;
                }
            }
            catch(Exception err)
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
    public class FoupProcedureStateToTextConvert : IValueConverter
    {
        static SolidColorBrush Blackbrush = new SolidColorBrush(Colors.Black);
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush Graybrush = new SolidColorBrush(Colors.Gray);
        

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                FoupProcedureStateEnum state = (FoupProcedureStateEnum)value;

                switch (state)
                {
                    case FoupProcedureStateEnum.IDLE:
                        return "Wait...";
                    case FoupProcedureStateEnum.PreSafetyError:
                        return "Error";
                    case FoupProcedureStateEnum.BehaviorError:
                        return "Error";
                    case FoupProcedureStateEnum.PostSafetyError:
                        return "Error";
                    case FoupProcedureStateEnum.DONE:
                        return "Done";
                    default:
                        return "";
                }
            }
            catch(Exception err)
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

    public class CassetteTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retVal = Visibility.Collapsed;
            try
            {
                
                if (values != null)
                {
                    if ((CassetteTypeEnum)values[0] != CassetteTypeEnum.FOUP_25)
                    {
                        //return $"{values[0]} [{values[1]}]";
                        retVal = Visibility.Visible;
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

}
