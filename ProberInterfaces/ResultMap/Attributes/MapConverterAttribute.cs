using System;
using System.Linq;

namespace ProberInterfaces.ResultMap.Attributes
{
    public static class AttributeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;

            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class MapConverterAttribute : System.Attribute
    {
        public ResultMapConvertType CoverterType;
        public Type ResultMapObjectType;

        public MapConverterAttribute(ResultMapConvertType convertertype, Type resultmapobjecttype = null)
        {
            this.CoverterType = convertertype;
            this.ResultMapObjectType = resultmapobjecttype;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class MapConverterPropetyAttribute : System.Attribute
    {
        public ResultMapConvertType CoverterType;

        public MapConverterPropetyAttribute(ResultMapConvertType type)
        {
            this.CoverterType = type;
        }
    }
}
