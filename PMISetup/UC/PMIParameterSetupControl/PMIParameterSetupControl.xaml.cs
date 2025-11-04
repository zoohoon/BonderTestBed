using LogModule;
using ProbeCardObject;
using ProberInterfaces.PMI;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;


namespace PMISetup.UC
{
    using ProberInterfaces;
    /// <summary>
    /// UC_PMI_StandardSetup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PMIParameterSetupControl : MahApps.Metro.Controls.Dialogs.CustomDialog, IPnpAdvanceSetupView
    {
        public PMIParameterSetupControl()
        {
            InitializeComponent();
        }
    }

    public class EnabledDisabledToBooleanConverter : IValueConverter
    {
        private const string EnabledText = "Enabled";
        private const string DisabledText = "Disabled";

        //public static readonly EnabledDisabledToBooleanConverter Instance = new EnabledDisabledToBooleanConverter();

        //private EnabledDisabledToBooleanConverter()
        //{
        //}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                retval = Equals(true, value) ? EnabledText : DisabledText;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Actually won't be used, but in case you need that
            return Equals(value, EnabledText);
        }
    }

    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return "Yes";
                else
                    return "No";
            }
            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "yes":
                    return true;
                case "no":
                    return false;
            }
            return false;
        }
    }


    public class GroupingMarginVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if(value != null)
                {
                    GROUPING_METHOD methodtype = (GROUPING_METHOD)value;

                    if(methodtype == GROUPING_METHOD.Single)
                    {
                        retval = Visibility.Collapsed;
                    }
                    else if(methodtype == GROUPING_METHOD.Multi)
                    {
                        retval = Visibility.Visible;
                    } 
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
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }
    }


    public class DoubleToFOCUSINGRAGEConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FOCUSINGRAGE retval = FOCUSINGRAGE.RANGE_100;

            try
            {
                int focusvalue = System.Convert.ToInt32((double)value);

                retval = (FOCUSINGRAGE)Enum.ToObject(typeof(FOCUSINGRAGE), focusvalue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double retval = 100;

            try
            {
                int focusingvalue = (int)value;

                retval = System.Convert.ToDouble(focusingvalue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static string GetDescription(Enum en)
        {
            try
            {
                Type type = en.GetType();
                MemberInfo[] memInfo = type.GetMember(en.ToString());
                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        return ((DescriptionAttribute)attrs[0]).Description;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
          
            return en.ToString();
        }
    }


    public class EnumExcludeUndefinedConverter : IValueConverter
    {
       

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PMI_PAUSE_METHOD[] retval = null;

            try
            {
                if (value == null) return DependencyProperty.UnsetValue;

                retval = (PMI_PAUSE_METHOD[])value;

                if(retval != null)
                {
                    retval = retval.Where(val => val.ToString() != "UNDEFINED").ToArray();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        //public static string GetDescription(Enum en)
        //{
        //    Type type = en.GetType();
        //    MemberInfo[] memInfo = type.GetMember(en.ToString());

        //    if (memInfo != null && memInfo.Length > 0)
        //    {
        //        object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

        //        if (attrs != null && attrs.Length > 0)
        //        {
        //            return ((DescriptionAttribute)attrs[0]).Description;
        //        }
        //    }

        //    return en.ToString();
        //}
    }

}
