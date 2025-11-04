//	--------------------------------------------------------------------
//		Member of the WPFSmartLibrary
//		For more information see : http://wpfsmartlibrary.codeplex.com/
//		(by DotNetMastermind)
//
//		filename		: EnumConverter.cs
//		namespace	: SoftArcs.WPFSmartLibrary.ValueConverter
//		class(es)	: EnumMatchToBooleanConverter
//							
//	--------------------------------------------------------------------
using EnumHelperModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using LogModule;

namespace ValueConverters
{
    /// <summary>
    /// Converts an Enum match into a Boolean value (and back)
    /// </summary>
    [ValueConversion( typeof( object ), typeof( bool ) )]
	[MarkupExtensionReturnType( typeof( EnumMatchToBooleanConverter ) )]
	public class EnumMatchToBooleanConverter : MarkupExtension, IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            try
            {
                if (value == null || parameter == null)
                {
                    return false;
                }

                string sourceValue = value.ToString();
                string targetValue = parameter.ToString();

                return sourceValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
            try
            {
                if (value == null || parameter == null)
                {
                    return null;
                }

                bool useValue = (bool)value;
                string targetValue = parameter.ToString();

                return useValue ? Enum.Parse(targetType, targetValue) : null;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
		}

		#endregion

		#region MarkupExtension "overrides"

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return new EnumMatchToBooleanConverter();
		}

		#endregion
	}

    [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return EnumHelper.GetAllValuesAndDescriptions(value.GetType());
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
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    //[ValueConversion(typeof(object), typeof(int))]
    //[MarkupExtensionReturnType(typeof(EnumToIntConverter))]
    //public class EnumToIntConverter : MarkupExtension, IValueConverter
    //{
    //    
    //    #region IValueConverter Members

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        int retVal = -1;

    //        try
    //        {
    //            if (value == null || parameter == null)
    //            {
    //                return -1;
    //            }

    //            int.TryParse(parameter.ToString(), out retVal);

    //            if (Enum.IsDefined(value.GetType(), value) == false)
    //                return DependencyProperty.UnsetValue;

    //            object parameterValue = Enum.Parse(value.GetType(), retVal.ToString());

    //            return parameterValue.Equals(value);
    //        }
    //        catch (Exception err)
    //        {
    //            return -1;
    //            LoggerManager.Error($"EnumToIntConverter() : " + err);
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string parameterString = parameter as string;
    //        if (parameterString == null)
    //            return DependencyProperty.UnsetValue;

    //        return Enum.Parse(targetType, parameterString);
    //    }

    //    #endregion

    //    #region MarkupExtension "overrides"

    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        return new EnumToIntConverter();
    //    }

    //    #endregion
    //}


    
}
