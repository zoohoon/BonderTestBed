using System;
using System.Windows.Data;
using LogModule;
using ProberInterfaces;

namespace ValueConverters
{
    public class LotStateToStringConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            string strPos = null;

            try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                switch (val)
                {
                    case ModuleStateEnum.IDLE:
                        strPos = "Start";
                        break;
                    case ModuleStateEnum.PAUSED:
                        strPos = "Start";
                        break;
                    case ModuleStateEnum.RUNNING:
                        strPos = "Pause";
                        break;
                    default:
                        break;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class LotStateToStringConverter2 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                if (val == ModuleStateEnum.PAUSED)
                {
                    strPos = true;
                }
                else
                {
                    strPos = false;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class LotStateToStringConverter4 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                if (val == ModuleStateEnum.PAUSED|| val == ModuleStateEnum.IDLE)
                {
                    strPos = true;
                }
                else
                {
                    strPos = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class ProbingStateToConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
   try
            {
                ModuleStateEnum val = ModuleStateEnum.INIT;
                EnumProbingState PreState = EnumProbingState.IDLE;
                if (values[0] is ModuleStateEnum && values[1] is EnumProbingState)
                {
                    val = (ModuleStateEnum)values[0];
                    PreState = (EnumProbingState)values[1];
                }
                else
                {
                    return false;
                }

                if (val == ModuleStateEnum.PAUSED)
                {
                    if (PreState is EnumProbingState.IDLE || PreState is EnumProbingState.DONE || PreState is EnumProbingState.PAUSED)
                    {
                        strPos = false;
                    }
                    else
                    {
                        strPos = true;
                    }
                }
                else
                {
                    strPos = false;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class LotStateToStringConverter3 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            string strPos = null;

     try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                switch (val)
                {
                    case ModuleStateEnum.IDLE:
                        strPos = "IDLE";
                        break;
                    case ModuleStateEnum.PAUSED:
                        strPos = "PAUSED";
                        break;
                    case ModuleStateEnum.RUNNING:
                        strPos = "RUNNING";
                        break;
                    case ModuleStateEnum.ERROR:
                        strPos = "ERROR";
                        break;
                    case ModuleStateEnum.ABORT:
                        strPos = "ABORT";
                        break;
                    case ModuleStateEnum.SUSPENDED:
                        strPos = "RUNNING";
                        break;
                    default:
                        strPos = "RUNNING";
                        break;
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class LotStateToNameEditConvert : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                if (val == ModuleStateEnum.IDLE)
                {
                    strPos = true;
                }
                else
                {
                    strPos = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return strPos;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
