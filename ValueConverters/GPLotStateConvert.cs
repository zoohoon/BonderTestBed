using System;
using System.Globalization;
using System.Windows.Data;
using LogModule;
using ProberInterfaces;

namespace ValueConverters
{

    public class GPLotStateToStringConverter : IValueConverter
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
    public class GPLotStateToStringConverter1 : IValueConverter
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
    public class GPLotStateToStringConverter2 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                ModuleStateEnum val = (ModuleStateEnum)value;

                if (val == ModuleStateEnum.RUNNING)
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

    public class GPLotStateToStringConverter3 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                if(value is ModuleStateEnum)
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

    public class GPLotStateToStringConverter4 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum val = (ModuleStateEnum)value;

                    if (val != ModuleStateEnum.RUNNING)
                    {
                        strPos = true;
                    }
                    else
                    {
                        strPos = false;
                    }
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

    public class GPLotStateToStringConverter5 : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool strPos;
            try
            {
                if (value is ModuleStateEnum)
                {
                    ModuleStateEnum val = (ModuleStateEnum)value;

                    if (val != ModuleStateEnum.IDLE)
                    {
                        strPos = true;
                    }
                    else
                    {
                        strPos = false;
                    }
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

    public class GPManualHandlingViewDisableConverter : IValueConverter
    {
        private bool IsLoaderBusy(ModuleStateEnum state)
        {
            bool retVal = false;

            try
            {
                if (state == ModuleStateEnum.RUNNING ||
                    state == ModuleStateEnum.ABORT ||
                    state == ModuleStateEnum.PAUSING)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                retval = !IsLoaderBusy((ModuleStateEnum)value);
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

    public class GPHandlingBtnEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool strPos;

                if (values[0] is ModuleStateEnum)
                {
                    ModuleStateEnum val = (ModuleStateEnum)values[0];
                    bool val2 = (bool)values[1];


                    if (val != ModuleStateEnum.RUNNING)
                    {
                        strPos = true;
                    }
                    else
                    {
                        strPos = false;
                    }

                    if (!val2)
                    {
                        strPos = false;
                    }
                }
                else
                {
                    strPos = false;
                }

                return strPos;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GPHandlingBtnEnableConverter(): Error occurred. Err = {err.Message}");
                //LoggerManager.Exception(err);
                return false;
                //throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class GPOCRMovingEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {


                bool val = (bool)values[0];
                bool val2 = (bool)values[1];

                bool strPos;


                if (val)
                {
                    strPos = true;
                }
                else
                {
                    strPos = false;
                }

                if (!val2)
                {
                    strPos = false;
                }
                return strPos;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GPHandlingBtnEnableConverter(): Error occurred. Err = {err.Message}");
                //LoggerManager.Exception(err);
                return false;
                //throw;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
