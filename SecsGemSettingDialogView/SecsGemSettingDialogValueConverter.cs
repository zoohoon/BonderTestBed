using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Data;

namespace UcSecsGemSettingDialog
{
    public class IsGemEnableValueConverter : IValueConverter
    {
        // parameter true는 Enable인지 판단.
        // false는 Disable인지 판단.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            if (value is SecsEnum_Enable)
            {
                SecsEnum_Enable enable = (SecsEnum_Enable)value;

                if (enable == SecsEnum_Enable.ENABLE)
                    retVal = true;
                else
                    retVal = false;

                if (parameter != null)
                {
                    if (parameter.ToString().Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        retVal = !retVal;
                    }
                }
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecsEnum_Enable retVal = SecsEnum_Enable.DISABLE;

            if (value is bool)
            {
                bool isEnable = (bool)value;

                if (parameter != null)
                {
                    if (parameter.ToString().Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        isEnable = !isEnable;
                    }
                }

                if (isEnable == true)
                    retVal = SecsEnum_Enable.ENABLE;
                else
                    retVal = SecsEnum_Enable.DISABLE;
            }

            return retVal;
        }
    }

    public class ControlStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (value is SecsEnum_ControlState)
                {
                    if (value.ToString().Equals(parameter.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecsEnum_ControlState retVal = SecsEnum_ControlState.UNKNOWN;

            try
            {
                if (value is bool)
                {
                    bool isEnable = (bool)value;

                    if (parameter != null)
                    {
                        bool parseResult = false;
                        Type controlEnumType = typeof(SecsEnum_ControlState);

                        parseResult = Enum.TryParse<SecsEnum_ControlState>(parameter.ToString(), true, out retVal);

                        if (parseResult == false)
                        {
                            retVal = SecsEnum_ControlState.UNKNOWN;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
    }

    public class EstablishToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (value is SecsEnum_EstablishSource)
                {
                    if (value.ToString().Equals(parameter.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        retVal = true;
                    }
                }

            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecsEnum_EstablishSource retVal = SecsEnum_EstablishSource.UNKNOWN;

            try
            {
                if (value is bool)
                {
                    bool isEnable = (bool)value;

                    if (isEnable && parameter != null)
                    {
                        bool parseResult = false;
                        Type controlEnumType = typeof(SecsEnum_EstablishSource);

                        parseResult = Enum.TryParse<SecsEnum_EstablishSource>(parameter.ToString(), true, out retVal);

                        if (parseResult == false)
                        {
                            retVal = SecsEnum_EstablishSource.UNKNOWN;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }
    }

    public class GemPassiveToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (value is SecsEnum_Passive)
                {
                    if (value.ToString().Equals(parameter.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecsEnum_Passive retVal = SecsEnum_Passive.UNKNOWN;

            try
            {
                if (value is bool)
                {
                    bool isEnable = (bool)value;

                    if (parameter != null)
                    {
                        bool parseResult = false;
                        Type controlEnumType = typeof(SecsEnum_Passive);

                        parseResult = Enum.TryParse<SecsEnum_Passive>(parameter.ToString(), true, out retVal);

                        if (parseResult == false)
                        {
                            retVal = SecsEnum_Passive.UNKNOWN;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }
    }

    public class GemIsOfflineStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (value is SecsEnum_ON_OFFLINEState)
                {
                    if ((SecsEnum_ON_OFFLINEState)value == SecsEnum_ON_OFFLINEState.OFFLINE)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecsEnum_ON_OFFLINEState retVal = SecsEnum_ON_OFFLINEState.ONLINE;

            try
            {
                if (value is bool)
                {
                    bool isEnable = (bool)value;

                    if (isEnable == true)
                    {
                        retVal = SecsEnum_ON_OFFLINEState.OFFLINE;
                    }
                    else
                    {
                        retVal = SecsEnum_ON_OFFLINEState.ONLINE;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }
    }

    public class GemIsOnlineStateToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retVal = false;

            try
            {
                if (values != null
                    && parameter != null    
                    && 2 == values.Length
                    )
                {
                    if (values[0] is SecsEnum_ON_OFFLINEState)
                    {
                        if ((SecsEnum_ON_OFFLINEState)values[0] == SecsEnum_ON_OFFLINEState.ONLINE)
                        {
                            if (values[1] is SecsEnum_OnlineSubState)
                            {
                                if (values[1].ToString() == parameter.ToString())
                                {
                                    retVal = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }


            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] retObj = null;
            try
            {
                SecsEnum_ON_OFFLINEState retVal0 = SecsEnum_ON_OFFLINEState.UNKNOWN;
                SecsEnum_OnlineSubState retVAl1 = SecsEnum_OnlineSubState.LOCAL;

                if ((value is bool) && (parameter != null))
                {
                    bool isEnable = (bool)value;

                    if (isEnable == true)
                    {
                        bool parseResult = false;
                        retVal0 = SecsEnum_ON_OFFLINEState.ONLINE;

                        parseResult = Enum.TryParse<SecsEnum_OnlineSubState>(parameter.ToString(), out retVAl1);
                        if (parseResult != true)
                        {
                            retVAl1 = SecsEnum_OnlineSubState.LOCAL;
                        }
                    }
                }

                retObj = new object[] { retVal0, retVAl1 };
            }
            catch (Exception err)
            {
                throw err;
            }

            return retObj;
        }
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// The visibility value if the argument is false.
        /// </summary>
        public System.Windows.Visibility FalseValue { get; set; }

        /// <summary>
        /// The visibility value if the argument is true.
        /// </summary>
        public System.Windows.Visibility TrueValue { get; set; }

        /// <summary>
        /// Creates a new <see cref="BoolToVisibilityConverter" />.
        /// </summary>
        public BoolToVisibilityConverter()
        {
            FalseValue = System.Windows.Visibility.Collapsed;
            TrueValue = System.Windows.Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && ((bool)value) == false)
            {
                return FalseValue;
            }
            else
            {
                return TrueValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}