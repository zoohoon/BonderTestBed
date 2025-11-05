//	--------------------------------------------------------------------
//		Member of the WPFSmartLibrary
//		For more information see : http://wpfsmartlibrary.codeplex.com/
//		(by DotNetMastermind)
//
//		filename		: BooleanConverter.cs
//		namespace	: SoftArcs.WPFSmartLibrary.ValueConverter
//		class(es)	: BoolToBrushConverter
//						  BoolToOppositeBoolConverter
//						  BoolToVisibilityConverter
//						  BoolToVisibleOrHiddenConverter
//						  BoolToVisibleOrCollapsedConverter
//						  BoolToVisibleOrCollapsedOrHiddenConverter
//
//	--------------------------------------------------------------------
using LoaderParameters.Data;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ValueConverters
{
    /// <summary>
    /// Converts a Boolean value into a Brush (and back)
    /// The Brushes for the true value and the false value can be declared separately
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Brush))]
    [MarkupExtensionReturnType(typeof(BoolToBrushConverter))]
    public class BoolToBrushConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// TrueValueBrush (default : MediumSpringGreen => see Constructor)
        /// </summary>
        public Brush TrueValueBrush { get; set; }

        /// <summary>
        /// FalseValueBrush (default : White => see Constructor)
        /// </summary>
        public Brush FalseValueBrush { get; set; }

        /// <summary>
        /// Initialize the 'True' and 'False' Brushes with standard values
        /// </summary>
        public BoolToBrushConverter()
        {
            try
            {
                TrueValueBrush = Brushes.MediumSpringGreen;
                FalseValueBrush = Brushes.White;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    return (bool)value ? TrueValueBrush : FalseValueBrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                        CultureInfo culture)
        {
            try
            {
                if (value is Brush)
                {
                    return value.Equals(TrueValueBrush);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return value;
        }

        #endregion // IValueConverter Members

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            BoolToBrushConverter retval = null;

            try
            {
                retval = new BoolToBrushConverter
                {
                    TrueValueBrush = this.TrueValueBrush,
                    FalseValueBrush = this.FalseValueBrush
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
    }

    /// <summary>
    /// Converts a Boolean value into an opposite Boolean value (and back)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    [MarkupExtensionReturnType(typeof(BoolToOppositeBoolConverter))]
    public class BoolToOppositeBoolConverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is bool)
                {
                    retval = !(bool)value;
                }
                else
                {
                    retval = value;
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
            object retval = null;

            try
            {
                if (value is bool)
                {
                    retval = !(bool)value;
                }
                else
                {
                    retval = value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new BoolToOppositeBoolConverter();
        }

        #endregion
    }

    /// <summary>
    /// Converts a Boolean value into a Visibility enumeration (and back)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(BoolToVisibilityConverter))]
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        /// <summary>
        /// Initialize the properties with standard values
        /// </summary>
        public BoolToVisibilityConverter()
        {
            try
            {
                this.FalseEquivalent = Visibility.Collapsed;
                this.OppositeBooleanValue = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //+------------------------------------------------------------------------
        //+ Usage :
        //+ -------
        //+ 1. <conv:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
        //+ 2. {Binding ... Converter={StaticResource BoolToVisibilityConverter}
        //+------------------------------------------------------------------------
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is bool && targetType == typeof(Visibility))
                {
                    bool? booleanValue = (bool?)value;

                    if (this.OppositeBooleanValue == true)
                    {
                        booleanValue = !booleanValue;
                    }

                    retval = booleanValue.GetValueOrDefault() ? Visibility.Visible : FalseEquivalent;
                }
                else
                {
                    retval = value;
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
            object retval = null;

            try
            {
                if (value is Visibility)
                {
                    Visibility visibilityValue = (Visibility)value;

                    if (this.OppositeBooleanValue == true)
                    {
                        visibilityValue = visibilityValue == Visibility.Visible ? FalseEquivalent : Visibility.Visible;
                    }

                    retval = (visibilityValue == Visibility.Visible);
                }
                else
                {
                    retval = value;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion // IValueConverter Members

        //+-----------------------------------------------------------------------------------
        //+ Usage :	(wpfsl: => XML Namespace mapping to the "BoolToVisibilityConverter" class)
        //+ -------
        //+ Use as follows : {Binding ... Converter={wpfsl:BoolToVisibilityConverter}
        //+ NO StaticResource required
        //+-----------------------------------------------------------------------------------
        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object retval = null;

            try
            {
                retval = new BoolToVisibilityConverter
                {
                    FalseEquivalent = this.FalseEquivalent,
                    OppositeBooleanValue = this.OppositeBooleanValue
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
    }

    /// <summary>
    /// Converts a Boolean value into a Visibility enumeration (and back)
    /// FalseEquivalent is always Visibility.Hidden
    /// ! Obsolete : It is recommended to use the more flexible 'BoolToVisibilityConverter' instead
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibleOrHiddenConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is bool && targetType == typeof(Visibility))
                {
                    bool bValue = (bool)value;

                    if (bValue)
                    {
                        retval = Visibility.Visible;
                    }
                    else
                    {
                        retval = Visibility.Hidden;
                    }
                }
                else
                {
                    retval = value;
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
            object retval = null;

            try
            {
                if (value is Visibility)
                {
                    Visibility visibility = (Visibility)value;

                    if (visibility == Visibility.Visible)
                    {
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                    }
                }
                else
                {
                    retval = value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToHiddenOrVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is bool && targetType == typeof(Visibility))
                {
                    bool bValue = (bool)value;

                    if (!bValue)
                    {
                        retval = Visibility.Visible;
                    }
                    else
                    {
                        retval = Visibility.Hidden;
                    }
                }
                else
                {
                    retval = value;
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
            object retval = null;

            try
            {
                if (value is Visibility)
                {
                    Visibility visibility = (Visibility)value;

                    if (visibility != Visibility.Visible)
                    {
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                    }
                }
                else
                {
                    retval = value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
    }

    /// <summary>
    /// Converts a Boolean value into a Visibility enumeration (and back)
    /// FalseEquivalent is always Visibility.Collapsed
    /// ! Obsolete : It is recommended to use the more flexible 'BoolToVisibilityConverter' instead
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibleOrCollapsedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is bool && targetType == typeof(Visibility))
                {
                    retval = ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    retval = value;
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
            object retval = null;

            try
            {
                if (value is Visibility)
                {
                    retval = ((Visibility)value == Visibility.Visible);
                }
                else
                {
                    retval = value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion
    }

    /// <summary>
    /// Converts a Boolean value into a Visibility enumeration (and back)
    /// FalseEquivalent can be defined with the property 'Collapse'
    /// ! Obsolete : It is recommended to use the more flexible 'BoolToVisibilityConverter' instead
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibleOrCollapsedOrHiddenConverter : IValueConverter
    {
        /// <summary>
        /// Collapse (default : Visibility.Hidden, because a bool is 'false' by default)
        /// true  => 'False-Equivalent' = Visibility.Collapsed
        /// false => 'False-Equivalent' = Visibility.Hidden
        /// </summary>
        public bool Collapse { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is bool && targetType == typeof(Visibility))
                {
                    bool bValue = (bool)value;

                    if (bValue)
                    {
                        retval = Visibility.Visible;
                    }
                    else
                    {
                        if (Collapse)
                        {
                            retval = Visibility.Collapsed;
                        }
                        else
                        {
                            retval = Visibility.Hidden;
                        }
                    }
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (value is Visibility)
                {
                    Visibility visibility = (Visibility)value;

                    if (visibility == Visibility.Visible)
                    {
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                    }
                }
                else
                {
                    retval = value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion // IValueConverter Members
    }

    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            try
            {
                True = trueValue;
                False = falseValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                retval = value is bool && ((bool)value) ? True : False;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                retval = value is T && EqualityComparer<T>.Default.Equals((T)value, True);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }

    public class BooleanToColorDeptConverter : BooleanConverter<ColorDept>
    {
        public BooleanToColorDeptConverter() :
            base(ColorDept.Color24, ColorDept.BlackAndWhite)
        { }
    }

    public class BooleanToIntConverter : IValueConverter
    {
        int parameterValue;
        int intVal;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                if (parameter == null)
                {
                    retval = DependencyProperty.UnsetValue;
                }
                else
                {
                    parameterValue = int.Parse(parameter.ToString());
                    intVal = ((bool)value) ? 1 : 0;

                    retval = parameterValue.Equals(intVal);
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
            object retval = null;

            try
            {                
                if (parameter == null)
                {
                    retval = DependencyProperty.UnsetValue;
                }
                else
                {
                    int parameterValue = int.Parse(parameter.ToString());
                    retval = parameterValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class ComparisonConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.ToString().Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.ToString().Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return false; // or return parameter.Equals(YourEnumType.SomeDefaultValue);
            }
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object retval = null;

            try
            {
                string parameterString = parameter as string;

                if (parameterString == null)
                {
                    retval = DependencyProperty.UnsetValue;
                }
                else
                {
                    if (Enum.IsDefined(value.GetType(), value) == false)
                    {
                        retval = DependencyProperty.UnsetValue;
                    }
                    else
                    {
                        object parameterValue = Enum.Parse(value.GetType(), parameterString);

                        retval = parameterValue.Equals(value);
                    }
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
            object retval = null;

            try
            {
                string parameterString = parameter as string;

                if (parameterString == null)
                {
                    retval = DependencyProperty.UnsetValue;
                }
                else
                {
                    retval = Enum.Parse(targetType, parameterString);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion
    }

    public class MulitCommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {
                retval = values.Clone();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnableLockMultiConverter : IMultiValueConverter
    {
        //Visiabiliby
        //Visible = 0,
        //Hidden = 1,
        //Collapsed = 2
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null)
                {
                    if (!(bool)values[0]) //IsEnable False
                    {
                        if ((bool)values[1]) //Lockable True
                        {
                            if (values[2] != null)
                                return Visibility.Visible;
                            else
                                return Visibility.Hidden;

                        }
                        else
                            return Visibility.Hidden;
                    }
                    else
                        return Visibility.Hidden;
                    //return (bool)values[0] & (bool)values[1];
                }
                return false;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DynamicCarrierToEnableStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string retVal = "";
                if(value is DynamicModeEnum)
                {
                    var modeEnum = (DynamicModeEnum)value;
                    switch (modeEnum)
                    {   
                        case DynamicModeEnum.NORMAL:
                            retVal = "DISABLE";
                            break;
                        case DynamicModeEnum.DYNAMIC:
                            retVal = "ENABLE";
                            break;
                        default:
                            break;
                    }
                }
                return retVal;
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

    public class FoupDynamicCarrierModeVisiableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Visibility retVal = Visibility.Collapsed;
                if (value is DynamicModeEnum)
                {
                    var modeEnum = (DynamicModeEnum)value;
                    switch (modeEnum)
                    {
                        case DynamicModeEnum.NORMAL:
                            retVal = Visibility.Collapsed;
                            break;
                        case DynamicModeEnum.DYNAMIC:
                            retVal = Visibility.Visible;
                            break;
                        default:
                            break;
                    }
                }
                return retVal;
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

    public class FoupDynamicCarrierModeForeGroundConverter : IMultiValueConverter
    {

        static SolidColorBrush EnableColor = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DisableColor = new SolidColorBrush(Colors.Orange);
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retVal = DisableColor;
            try
            {
                //0 : Foup Index
                //1 : ActiveInfos
                //3 : char (L and U)
                if (values != null)
                {
                    if(values[0] is DynamicFoupStateEnum)
                    {
                        var modeEnum = (DynamicFoupStateEnum)values[0];
                        string str = (string)values[1];
                        switch (modeEnum)
                        {
                            case DynamicFoupStateEnum.LOAD_AND_UNLOAD:
                                {
                                    if(str.Equals("L"))
                                    {
                                        retVal = EnableColor;
                                    }
                                    if(str.Equals("U"))
                                    {
                                        retVal = EnableColor;
                                    }
                                }
                                break;
                            case DynamicFoupStateEnum.UNLOAD:
                                {
                                    if (str.Equals("L"))
                                    {
                                        retVal = DisableColor;
                                    }
                                    if (str.Equals("U"))
                                    {
                                        retVal = EnableColor;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }           
                }

                //0 : Foup Index
                //1 : ActiveInfos
                //3 : char (L and U)
                //if (values != null)
                //{
                //    if (values.Length == 3)
                //    {
                //        int foupindex = (int)values[0];
                //        List<ActiveLotInfo> activeLotInfos = (List<ActiveLotInfo>)values[1];
                //        string str = (string)values[2];

                //        if (activeLotInfos.Count > 0 && activeLotInfos.Count >= foupindex)
                //        {
                //            var modeEnum = activeLotInfos[foupindex - 1].DynamicFoupState;
                //            switch (modeEnum)
                //            {
                //                case DynamicFoupStateEnum.LOAD_AND_UNLOAD:
                //                    {
                //                        if (str.Equals("L"))
                //                        {
                //                            retVal = EnableColor;
                //                        }
                //                        if (str.Equals("U"))
                //                        {
                //                            retVal = EnableColor;
                //                        }
                //                    }
                //                    break;
                //                case DynamicFoupStateEnum.UNLOAD:
                //                    {
                //                        if (str.Equals("L"))
                //                        {
                //                            retVal = DisableColor;
                //                        }
                //                        if (str.Equals("U"))
                //                        {
                //                            retVal = EnableColor;
                //                        }
                //                    }
                //                    break;
                //                default:
                //                    break;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return retVal;
            }
            return retVal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int retVal = 0;

            try
            {
                if (value is bool)
                {
                    if ((bool)value == true)
                    {
                        retVal = 1;
                    }
                    else
                    {
                        retVal = 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToGroupBoxHeaderTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = "";

            try
            {
                if (value is bool)
                {
                    if ((bool)value == true)
                    {
                        retVal = "Buffer";
                    }
                    else
                    {
                        retVal = "";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool var = (bool)value;
                if (parameter != null)
                {
                    bool param = (bool)parameter;
                    return !(param && !var);

                }
                return !var;

            }
            return true;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EventCodeEnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool retVal = false;
                if (value is EventCodeEnum)
                {
                    var eventCode = (EventCodeEnum)value;
                    if(eventCode == EventCodeEnum.NONE)
                    {
                        retVal = false;
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                return retVal;
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
