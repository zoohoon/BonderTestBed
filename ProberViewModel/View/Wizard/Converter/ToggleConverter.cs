using System;

namespace WizardCategoryView.Converter
{
    using ProberInterfaces.State;
    using System.Globalization;
    using System.Windows.Data;
    public class StateToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is EnumEnableState && targetType == typeof(bool))
            {
                bool ret = false;
                switch ((EnumEnableState)value)
                {
                    case EnumEnableState.IDLE:
                        ret = false;
                        break;
                    case EnumEnableState.MUST:
                        ret = false;
                        break;
                    case EnumEnableState.MUSTNOT:
                        ret = false;
                        break;
                    case EnumEnableState.ENABLE:
                        ret = true;
                        break;
                    case EnumEnableState.DISABLE:
                        ret = true;
                        break;
                    default:
                        ret = false;
                        break;
                }

                return ret;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    public class StateToIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumEnableState && targetType == typeof(Nullable<Boolean>))
            {
                bool ret = false;
                switch ((EnumEnableState)value)
                {
                    case EnumEnableState.IDLE:
                        ret = false;
                        break;
                    case EnumEnableState.MUST:
                        ret = true;
                        break;
                    case EnumEnableState.MUSTNOT:
                        ret = false;
                        break;
                    case EnumEnableState.ENABLE:
                        ret = true;
                        break;
                    case EnumEnableState.DISABLE:
                        ret = false;
                        break;
                    default:
                        ret = false;
                        break;
                }

                return ret;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return value;
        }
    }

    public class StateToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumEnableState && targetType == typeof(Object))
            {
                string ret = "";
                string enable = "ENABLE";
                string disable = "DISABLE";

                switch ((EnumEnableState)value)
                {
                    case EnumEnableState.IDLE:
                        ret = disable;
                        break;
                    case EnumEnableState.MUST:
                        ret = enable;
                        break;
                    case EnumEnableState.MUSTNOT:
                        ret = disable;
                        break;
                    case EnumEnableState.ENABLE:
                        ret = enable;
                        break;
                    case EnumEnableState.DISABLE:
                        ret = disable;
                        break;
                    default:
                        ret = disable;
                        break;
                }

                return ret;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }
}
