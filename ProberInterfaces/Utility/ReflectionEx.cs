using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProberInterfaces.Utility
{
    public static class ReflectionEx
    {
        public static IEnumerable<T> GetProperties<T>(object inst)
        {
            try
            {
                var rel = from prop in inst.GetType().GetProperties()
                      where prop.PropertyType == typeof(T)
                      select (T)prop.GetValue(inst);
            return rel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static IEnumerable<Type> GetAssignableTypes(Assembly assembly, Type baseType)
        {
            try
            {
            return assembly.GetTypes().Where
                (type =>
                baseType.IsAssignableFrom(type) &&
                type.IsInterface == false &&
                type.IsAbstract == false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public static IEnumerable<T> GetAssignableInstances<T>(Assembly assembly, params object[] constructionArgs)
            where T : class
        {
            List<T> instances = new List<T>();

            try
            {
                var implTypes = GetAssignableTypes(assembly, typeof(T));

                foreach (var type in implTypes)
                {
                        var inst = Activator.CreateInstance(type, constructionArgs) as T;
                        instances.Add(inst);
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return instances;
        }

    }

}
