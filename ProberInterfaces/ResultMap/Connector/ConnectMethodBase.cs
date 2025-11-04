using LogModule;
using ProberErrorCode;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.ResultMap
{
    public abstract class ConnectMethodBase
    {
        public EnumProberMapProperty PropertyName { get; set; }
        public object PropertyValue { get; set; }
        //public Func<object> Function { get; set; }
        public abstract EventCodeEnum Execute(MapHeaderObject header);

        public EventCodeEnum SetValue(MapHeaderObject header)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                object proberpropertyobj = null;

                bool IsValid = header.PropertyDictionary.TryGetValue(PropertyName, out proberpropertyobj);

                if (IsValid == true && proberpropertyobj != null)
                {
                    this.PropertyValue = proberpropertyobj;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    /// <summary>
    /// 할당 된, EnumProberMapProperty을 이용, 프로퍼티를 찾아 사용
    /// </summary>
    public class PropertyConnectMethod : ConnectMethodBase
    {
        public PropertyConnectMethod(EnumProberMapProperty name)
        {
            this.PropertyName = name;
        }

        public override EventCodeEnum Execute(MapHeaderObject header)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetValue(header);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    /// <summary>
    /// 할당 된 고정 값을 사용.
    /// </summary>
    public class ConstantConnectMethod : ConnectMethodBase
    {
        public ConstantConnectMethod(object value)
        {
            this.PropertyValue = value;
        }

        public override EventCodeEnum Execute(MapHeaderObject header)
        {
            // EMPTY
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    ///  Function을 이용하기 위해 사용.
    /// </summary>
    public class FunctionConnectMethod : ConnectMethodBase
    {
        public Func<object> Function { get; set; }

        public FunctionConnectMethod(Func<object> func)
        {
            this.Function = func;
        }

        public override EventCodeEnum Execute(MapHeaderObject header)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.PropertyValue = Function();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    /// <summary>
    /// 특정 프로퍼티를 접근하기 위해 사용
    /// </summary>
    public class SpecificPropertyConnectMethod
    {
        public string TargetPropertyName { get; set; }
        public ConnectMethodBase ConenectMethod { get; set; }
        public SpecificPropertyConnectMethod(string mappingname, ConnectMethodBase connectmethod)
        {
            this.TargetPropertyName = mappingname;
            this.ConenectMethod = connectmethod;
        }
    }

    /// <summary>
    /// 여러개의 데이터를 조합할 때 사용
    /// </summary>
    public class MultipleConnectMethod : ConnectMethodBase
    {
        // TODO : MultiValue to Customtype 변환 필요.
        public Type Customtype { get; set; }

        public List<SpecificPropertyConnectMethod> SpecificPropertyConnectMethods { get; set; }

        public MultipleConnectMethod()
        {
            this.SpecificPropertyConnectMethods = new List<SpecificPropertyConnectMethod>();
        }

        public override EventCodeEnum Execute(MapHeaderObject header)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                foreach (var item in SpecificPropertyConnectMethods)
                {
                    retval = item.ConenectMethod.SetValue(header);

                    if (retval != EventCodeEnum.NONE)
                    {
                        break;
                    }
                }

                Type customtype = Customtype;

                var instance = Activator.CreateInstance(customtype);

                foreach (var item in SpecificPropertyConnectMethods)
                {
                    if (item.ConenectMethod != null)
                    {
                        //retval = PropertyExtension.SetPropertyValue(instance, item.MappingPropertyName, item.ConenectMethod.PropertyValue);
                        retval = PropertyExtension.SetChildPropertyValue(instance, item.TargetPropertyName, item.ConenectMethod.PropertyValue);
                    }
                    else
                    {
                        retval = EventCodeEnum.UNDEFINED;
                        break;
                    }
                }

                PropertyValue = instance;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
