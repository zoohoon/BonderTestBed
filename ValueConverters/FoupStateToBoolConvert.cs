using System;
using System.Globalization;
using System.Windows.Data;
using LogModule;
using ProberInterfaces.Foup;

namespace ValueConverters
{
    public class FoupStateToBoolConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                FoupStateEnum state = (FoupStateEnum)value;

                switch (state)
                {
                    case FoupStateEnum.ERROR:
                        return true;

                    //case FoupStateEnum.ILLEGAL:
                    //    return true;

                    case FoupStateEnum.LOAD:
                        return false;

                    case FoupStateEnum.UNLOAD:
                        return false;

                    default:
                        return false;

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

    public class FoupStateToBoolLoadUnloadConvert : IValueConverter
    {
        public bool ERROR_FLAG { get; set; }
        public bool ILLEGAL_FLAG { get; set; }
        public bool LOAD_FLAG { get; set; }
        public bool UNLOAD_FLAG { get; set; }
        public bool DEFAULT_FLAG { get; set; }

        public FoupStateToBoolLoadUnloadConvert()
        {
            try
            {
                this.ERROR_FLAG = false;
                this.ILLEGAL_FLAG = false;
                this.LOAD_FLAG = false;
                this.UNLOAD_FLAG = false;
                this.DEFAULT_FLAG = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {

                FoupStateEnum state = (FoupStateEnum)value;

                switch (state)
                {
                    case FoupStateEnum.ERROR:
                        return ERROR_FLAG;

                    //case FoupStateEnum.ILLEGAL:
                    //    return ILLEGAL_FLAG;

                    case FoupStateEnum.LOAD:
                        return LOAD_FLAG;

                    case FoupStateEnum.UNLOAD:
                        return UNLOAD_FLAG;
                    default:
                        return DEFAULT_FLAG;

                }
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
}
