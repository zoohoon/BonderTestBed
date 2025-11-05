using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;

namespace ServiceModule
{
    [Serializable]
    public class RemoteServiceModuleParam : ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; set; } = "ServiceClient";
        public string FileName { get; set; } = "ServiceClientParam.bin";

        private Element<EnumServiceType> _ServiceType = new Element<EnumServiceType>();
        [XmlElement(nameof(ServiceType))]
        public Element<EnumServiceType> ServiceType
        {
            get { return _ServiceType; }
            set
            {
                _ServiceType = value;
            }
        }

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            ServiceType.Value = EnumServiceType.NONE;

            return EventCodeEnum.NONE;
        }
    }
}
