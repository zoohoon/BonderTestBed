using LogModule;
using ProberErrorCode;
using RequestInterface;
using System;

namespace ProberInterfaces.ResultMap
{
    public abstract class MapComponentBase
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public MapPropertyConnector Connector { get; set; }

        public MapComponentBase()
        {
            this.Connector = new MapPropertyConnector();
        }

        //private EventCodeEnum SetPropertyValue(MapHeaderObject HeaderObj, ConnectMethodBase coupler)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        object proberpropertyobj = null;
        //        bool IsValid = HeaderObj.PropertyDictionary.TryGetValue(coupler.PropertyName, out proberpropertyobj);

        //        if (IsValid == true)
        //        {
        //            if (retval == EventCodeEnum.NONE && proberpropertyobj != null)
        //            {
        //                coupler.PropertyValue = proberpropertyobj;

        //                retval = EventCodeEnum.NONE;
        //            }
        //            else
        //            {
        //                LoggerManager.Error($"[MapConverterBase], GetHeaderValueUsingPropertyName() : Failed to get the value of the property.");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public EventCodeEnum SetPropertiesValue(MapHeaderObject HeaderObj, ConnectMethodBase coupler)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retval = coupler.Execute(HeaderObj);

        //        //if (coupler is ComponentSingleValue)
        //        //{
        //        //    ComponentSingleValue tmp = coupler as ComponentSingleValue;
        //        //    retval = SetPropertyValue(HeaderObj, tmp);
        //        //}
        //        //else if (coupler is ComponentMultipleValue)
        //        //{
        //        //    ComponentMultipleValue tmp = coupler as ComponentMultipleValue;

        //        //    foreach (var item in tmp.componentWrapperSingleValues)
        //        //    {
        //        //        retval = SetPropertyValue(HeaderObj, item.componentSingleValue);

        //        //        if (retval != EventCodeEnum.NONE)
        //        //        {
        //        //            break;
        //        //        }
        //        //    }

        //        //    Type customtype = tmp.Customtype;

        //        //    var instance = Activator.CreateInstance(customtype);

        //        //    foreach (var item in tmp.componentWrapperSingleValues)
        //        //    {
        //        //        if (item.componentSingleValue != null)
        //        //        {
        //        //            retval = PropertyExtension.SetPropertyValue(instance, item.MappingPropertyName, item.componentSingleValue.PropertyValue);
        //        //        }
        //        //        else
        //        //        {
        //        //            retval = EventCodeEnum.UNDEFINED;
        //        //            break;
        //        //        }
        //        //    }

        //        //    tmp.PropertyValue = instance;
        //        //}
        //        //else
        //        //{
        //        //    // TODO : ERROR
        //        //}

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public object Exeucte(MapHeaderObject headerObject)
        {
            object retval = null;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                // HEADER에 정의되어 있는 값을 가져와 할당까지
                ret = Connector.ConenectMethod.Execute(headerObject);

                if (ret != EventCodeEnum.NONE)
                {
                    string pname = string.Empty;

                    if (!string.IsNullOrEmpty(Key))
                    {
                        pname = Key;
                    }

                    LoggerManager.Error($"[MapComponentBase], GetLineData() : PropValue is wrong. Name = {pname}");
                }

                object convertedobj = Connector.Converter.Convert(Connector);

                retval = convertedobj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class MapPropertyConnector
    {
        public MapPropertyConnector()
        {
            this.Converter = new MapDataConverter();
        }

        public ConnectMethodBase ConenectMethod { get; set; }
        public RequestBase InverseFormatter { get; set; }
        public RequestBase ReverseFormatter { get; set; }
        public IMapDataConverter Converter { get; set; }
    }
}
