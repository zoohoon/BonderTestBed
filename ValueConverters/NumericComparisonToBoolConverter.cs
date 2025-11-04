using LogModule;
using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace ValueConverters
{
    public class NumericComparisonToBoolConverter : MarkupExtension, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value != null && IsNumericType(value.GetType()))
                {
                    var d = System.Convert.ToDouble(value);

                    switch (ComparisonType)
                    {
                        case NumericComparisonType.IsEqualTo:
                            return ComparisonNumber == d;
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return false;
        }

        public double ComparisonNumber { get; set; }

        public NumericComparisonType ComparisonType { get; set; }

        public enum NumericComparisonType
        {
            None = 0,
            IsEqualTo,
            IsNotEqualTo,
            IsLessThan,
            IsGreaterThan,
        }

        protected bool IsNumericType(Type type)
        {
            try
            {
                if (type == null)
                    return false;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                    case TypeCode.Object:
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            return IsNumericType(Nullable.GetUnderlyingType(type));
                        }
                        return false;
                }
                return false;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    switch (ComparisonType)
                    {
                        case NumericComparisonType.IsEqualTo:

                            if ((bool)value == true)
                            {
                                return ComparisonNumber;
                            }
                            else
                            {
                                return 0;
                            }
                    }
                }
                else
                {
                    return null;
                }

                return null;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                return this;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }
}
