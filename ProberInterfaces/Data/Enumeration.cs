using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProberInterfaces.Enumeration
{
    public abstract class Enumeration : IComparable
    {
        private readonly int _value;
        private readonly string _displayName;

        protected Enumeration()
        {
        }

        protected Enumeration(int value, string displayName)
        {
            _value = value;
            _displayName = displayName;
        }

        public int Value
        {
            get { return _value; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new T();
                var locatedValue = info.GetValue(instance) as T;

                if (locatedValue != null)
                {
                    yield return locatedValue;
                }
            }
        }

        public override bool Equals(object obj)
        {
            try
            {
                var otherValue = obj as Enumeration;

                if (otherValue == null)
                {
                    return false;
                }

                var typeMatches = GetType().Equals(obj.GetType());
                var valueMatches = _value.Equals(otherValue.Value);

                return typeMatches && valueMatches;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int GetHashCode()
        {
            try
            {
                return _value.GetHashCode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
        {
            try
            {
                var absoluteDifference = Math.Abs(firstValue.Value - secondValue.Value);
                return absoluteDifference;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static T FromValue<T>(int value) where T : Enumeration, new()
        {
            try
            {
                var matchingItem = parse<T, int>(value, "value", item => item.Value == value);
                return matchingItem;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static T FromDisplayName<T>(string displayName) where T : Enumeration, new()
        {
            try
            {
                var matchingItem = parse<T, string>(displayName, "display name", item => item.DisplayName == displayName);
                return matchingItem;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static T parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration, new()
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            try
            {
                if (matchingItem == null)
                {
                    var message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(T));
                    throw new ApplicationException(message);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return matchingItem;
        }

        public int CompareTo(object other)
        {
            try
            {
                return Value.CompareTo(((Enumeration)other).Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

}
