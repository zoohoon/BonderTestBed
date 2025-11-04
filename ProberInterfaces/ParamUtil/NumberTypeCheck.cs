using System;

namespace ProberInterfaces.ParamUtil
{
    using ProberInterfaces.Network;
    public static class NumberTypeCheck
    {
        public static bool IsNumericType(this IElement elem)
        {
            if (elem.ValueType == null)
                return false;
            if (elem.ValueType.IsEnum)
                return false;

            switch (Type.GetTypeCode(elem.ValueType))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsFloatingType(this IElement elem)
        {
            if (elem.ValueType == null)
                return false;

            switch (Type.GetTypeCode(elem.ValueType))
            {
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsBooleanType(this IElement elem)
        {
            if (elem.ValueType == null)
                return false;

            switch (Type.GetTypeCode(elem.ValueType))
            {
                case TypeCode.Boolean:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsEnumType(this IElement elem)
        {
            if (elem.ValueType == null)
                return false;

            return elem.ValueType.IsEnum;
        }
        public static bool IsIPAddressType(this IElement elem)
        {
            if (elem.ValueType == null)
                return false;
            return elem.ValueType == typeof(IPAddressVer4);
        }
    }
}
