using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberInterfaces
{
    using System.Reflection;
    public static class ReflectionUtil
    {
        const BindingFlags _BindFlags = BindingFlags.Public | BindingFlags.Instance;

        public static T CreateDefaultInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }
        public static Object CreateDefaultInstance(Type type)
        {
            if (type == null)
                return null;
            if (type.GetConstructor(Type.EmptyTypes) == null || type.IsAbstract || type.IsInterface)
                return null;

            //==> String 같은 System type 들어올때 Exception 발생
            return Activator.CreateInstance(type);
        }
        public static bool IsObjectAssignable(Type objType, Type objBaseType)
        {
            return objBaseType.IsAssignableFrom(objType);
        }
        public static bool IsObjectDerrived(Type objType, Type objBaseType)
        {
            return objType.GetInterfaces().Contains(objBaseType);
        }
        public static List<T> CrawlingAssignableParamList<T>(Object obj)
        {
            List<T> paramList = new List<T>();
            try
            {

                foreach (PropertyInfo propInfo in obj.GetType().GetProperties(_BindFlags))
                {
                    Object paramValue = propInfo.GetValue(obj);
                    if (paramValue == null)
                        continue;

                    if (IsObjectAssignable(paramValue.GetType(), typeof(T)))//==> IsAssignableFrom을 사용할 경우 get;만 구현한 Property(대표적으로 Autofac들)은 못 거른다.
                        paramList.Add((T)paramValue);

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return paramList;
        }

        public static List<T> CrawlingDerrivedParamList<T>(Object obj)
        {
            List<T> paramList = new List<T>();

            try
            {
                foreach (PropertyInfo propInfo in obj.GetType().GetProperties(_BindFlags))
                {
                    Object paramValue = propInfo.GetValue(obj);

                    if (paramValue == null)
                    {
                        continue;
                    }

                    if (IsObjectAssignable(paramValue.GetType(), typeof(T)))
                    {
                        paramList.Add((T)paramValue);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return paramList;
        }
    }
}
