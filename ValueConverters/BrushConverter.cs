//	--------------------------------------------------------------------
//		Member of the WPFSmartLibrary
//		For more information see : http://wpfsmartlibrary.codeplex.com/
//		(by DotNetMastermind)
//
//		filename		: BrushConverter.cs
//		namespace	: SoftArcs.WPFSmartLibrary.ValueConverter
//		class(es)	: BrushToColorConverter
//							
//	--------------------------------------------------------------------
using LogModule;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ValueConverters
{
    /// <summary>
    /// Converts a SolidColorBrush into a Color (and back)
    /// </summary>
    [ValueConversion( typeof( Brush ), typeof( Color ) )]
	[MarkupExtensionReturnType( typeof( BrushToColorConverter ) )]
	public class BrushToColorConverter : MarkupExtension, IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            try
            {
                if (value is SolidColorBrush)
                {
                    return (value as SolidColorBrush).Color;
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
                if (value is Color)
                {
                    return new SolidColorBrush((Color)value);
                }

                return value;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
		}

		#endregion // IValueConverter Members

		#region MarkupExtension "overrides"

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return new BrushToColorConverter();
		}

		#endregion
	}
    public class InspectionSetFromBtnColorCon : IValueConverter
    {
        static string Grey = "#939393";
        static string Green = "#7AB357";//"#00A562";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool SetFromToggle = (bool)value;


                switch (SetFromToggle)
                {
                    case true:
                        return Green;
                    case false:
                        return Grey;
                    default:
                        return Grey;
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
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class BooltoGrnColorConv : IValueConverter
    {
        static string Grey = "#939393";
        static string Green = "#7AB357";//"#00A562";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool SetFromToggle = (bool)value;
                
                switch (SetFromToggle)
                {
                    case true:
                        return Green;
                    case false:
                        return Grey;
                    default:
                        return Grey;
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
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class BooltoGrnColorInvConv : IValueConverter
    {
        static string Grey = "#939393";
        static string Green = "#7AB357";//"#00A562";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool SetFromToggle = (bool)value;

                switch (SetFromToggle)
                {
                    case true:
                        return Grey;
                    case false:
                        return Green;
                    default:
                        return Grey;
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
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class StateToColorCon_LOTPAGE : IValueConverter
    {
        static string Silver = "#939393";
        static string Red = "#d10000";//"#00A562";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool State = (bool)value;


                switch (State)
                {
                    case true:
                        return Red;
                    case false:
                        return Silver;
                    default:
                        return Red;
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
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
    public class ToggleColorCon : IValueConverter
    {
        static string Silver = "#939393";
        static string Purple = "#784a8c";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool Toggle = (bool)value;


                switch (Toggle)
                {
                    case true:
                        return Purple;
                    case false:
                        return Silver;
                    default:
                        return Silver;
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
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

    public class HomeToggleColorCon : IValueConverter
    {
        static string Silver = "#939393";
        static string Purple = "#784a8c";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool Toggle = (bool)value;


                switch (Toggle)
                {
                    case true:
                        return Silver;
                    case false:
                        return Purple;
                    default:
                        return Purple;
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
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }

}
