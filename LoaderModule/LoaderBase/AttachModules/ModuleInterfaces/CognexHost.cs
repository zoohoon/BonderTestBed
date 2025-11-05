using System;
using System.Collections.Generic;

namespace LoaderBase.AttachModules.ModuleInterfaces
{
    using Newtonsoft.Json;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class CognexHost : IParamNode
    {
        [XmlIgnore, JsonIgnore]
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public String ModuleName { get; set; }
        public String ConfigName { get; set; }

        public CognexHost(String moduleName, String config)
        {
            ModuleName = moduleName;
            ConfigName = config;
        }
    }
}
