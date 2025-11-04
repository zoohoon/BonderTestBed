using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProberErrorCode;

namespace ProberInterfaces
{
    [Serializable]
    public class IntListParam : List<int>, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public IntListParam() : base()
        {
        }

        public IntListParam(int capacity) : base(capacity)
        {
        }

        public string FilePath { get; set; }
        public string FileName { get; set; }

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
        public List<object> Nodes { get; set; }
         = new List<object>();

        public virtual EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public virtual EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
    }
}
