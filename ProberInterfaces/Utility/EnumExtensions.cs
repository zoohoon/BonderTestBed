using System.ComponentModel;
using System.Linq;
using LogModule;

namespace ProberInterfaces.Utility
{
    using System;
    using System.Reflection;

    public static class EnumExtensions
    {
        public static string GetFullName(this Enum myEnum)
        {
            string retval = string.Empty;

            retval = $"{myEnum.GetType().Name}.{myEnum.ToString()}";

            return retval;
        }

        public static object GetAttribute<T>(Enum e)
        {
            object retval = null;

            MemberInfo[] meminfo = e.GetType().GetMember(e.ToString());

            if (meminfo != null && meminfo.Length > 0)
            {
                retval = meminfo[0].GetCustomAttributes(typeof(T), false).FirstOrDefault();
            }

            return retval;
        }
        public static string GetDescription(Enum en)
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

            return en.ToString();
        }

        // Might want to return a named type, this is a lazy example (which does work though)
        public static object[] GetValuesAndDescriptions(Type enumType)
        {
            try
            {
                var values = Enum.GetValues(enumType).Cast<object>();
                var valuesAndDescriptions = from value in values
                                            select new
                                            {
                                                Value = value,
                                                Description = value.GetType()
                                                        .GetMember(value.ToString())[0]
                                                        .GetCustomAttributes(true)
                                                        .OfType<DescriptionAttribute>()
                                                        .First()
                                                        .Description
                                            };
                return valuesAndDescriptions.ToArray();

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return null;
            }
        }
    }

    public static class EnumUtil<T>
    {
        public static T Parse(string s)
        {
            return (T)System.Enum.Parse(typeof(T), s);
        }
    }
}
