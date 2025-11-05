//	--------------------------------------------------------------------
//		Member of the WPFSmartLibrary
//		For more information see : http://wpfsmartlibrary.codeplex.com/
//		(by DotNetMastermind)
//
//		filename		: StringConverter.cs
//		namespace	: SoftArcs.WPFSmartLibrary.ValueConverter
//		class(es)	: BooleanStringToVisibilityConverter
//						  StringToVisibilityConverter
//	--------------------------------------------------------------------
using LogModule;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ValueConverters
{
    /// <summary>
    /// Converts a String into a Visibility enumeration (and back)
    /// FalseEquivalent is always Visibility.Collapsed
    /// </summary>
    [ValueConversion( typeof( string ), typeof( Visibility ) )]
	[MarkupExtensionReturnType( typeof( BooleanStringToVisibilityConverter ) )]
	public class BooleanStringToVisibilityConverter : MarkupExtension, IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            try
            {
                if (value is string && targetType == typeof(Visibility))
                {
                    return ((value as string).ToLower().Equals("true")) ? Visibility.Visible : Visibility.Collapsed;
                }
                return value;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
		}

		public object ConvertBack(object value, Type targetType, object parameter,
										  CultureInfo culture)
		{
            try
            {
                if (value is Visibility)
                {
                    return ((Visibility)value == Visibility.Visible) ? "true" : "false";
                }

                return value;
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
			return new BooleanStringToVisibilityConverter();
		}

		#endregion
	}

	/// <summary>
	/// Converts a String into a Visibility enumeration (and back)
	/// The FalseEquivalent can be declared with the "FalseEquivalent" property
	/// </summary>
	[ValueConversion( typeof( string ), typeof( Visibility ) )]
	[MarkupExtensionReturnType( typeof( StringToVisibilityConverter ) )]
	public class StringToVisibilityConverter : MarkupExtension, IValueConverter
	{
		/// <summary>
		/// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
		/// </summary>
		public Visibility FalseEquivalent { get; set; }

		/// <summary>
		/// Define whether the opposite boolean value is crucial (default : false)
		/// </summary>
		public bool OppositeStringValue { get; set; }

		/// <summary>
		/// Initialize the properties with standard values
		/// </summary>
		public StringToVisibilityConverter()
		{
            try
            {
                this.FalseEquivalent = Visibility.Collapsed;
                this.OppositeStringValue = false;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
		}

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            try
            {
                if (value is string && targetType == typeof(Visibility))
                {
                    if (this.OppositeStringValue == true)
                    {
                        return ((value as string).ToLower().Equals(String.Empty)) ? Visibility.Visible : this.FalseEquivalent;
                    }

                    return ((value as string).ToLower().Equals(String.Empty)) ? this.FalseEquivalent : Visibility.Visible;
                }

                return value;
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
                if (value is Visibility)
                {
                    if (this.OppositeStringValue == true)
                    {
                        return ((Visibility)value == Visibility.Visible) ? String.Empty : "visible";
                    }

                    return ((Visibility)value == Visibility.Visible) ? "visible" : String.Empty;
                }

                return value;
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
            try
            {
                return new StringToVisibilityConverter
                {
                    FalseEquivalent = this.FalseEquivalent,
                    OppositeStringValue = this.OppositeStringValue
                };
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
		}

		#endregion
	}

    [ValueConversion(typeof(string), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(StringToBrushConverter))]
    public class StringToBrushConverter : MarkupExtension, IValueConverter
    {
        static SolidColorBrush PlusBrush = new SolidColorBrush(Colors.Red);
        static SolidColorBrush MinusBrush = new SolidColorBrush(Colors.Blue);

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string state = (string)value;

                int num = System.Convert.ToInt32(state);

                if (num > 0)
                {
                    return PlusBrush;
                }
                else
                {
                    return MinusBrush;
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

        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new StringToBrushConverter();
        }

        #endregion

        #endregion
    }


    [ValueConversion(typeof(string), typeof(string))]
    public class ShowUnderscoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string text ? text.Replace("_", "__") : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string text ? text.Replace("__", "_") : null;
    }

    
}