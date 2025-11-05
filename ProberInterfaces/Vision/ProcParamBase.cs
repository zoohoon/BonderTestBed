using System;
using System.Collections.Generic;


namespace ProberInterfaces.Vision
{
    using Newtonsoft.Json;
    using System.Xml.Serialization;

    [Serializable]
    public class ProcParamBase : IParamNode
    {
        public enum EnumLightSourceType
        {
            Oblique = 0,
            Coaxial = 1,
            Both = 2
        }

        [XmlIgnore, JsonIgnore]
        public virtual Object Owner { get; set; }
        private Element<EnumLightSourceType> mLightSource  = new Element<EnumLightSourceType>();
        public Element<EnumLightSourceType> LightSource
        {
            get { return mLightSource; }
            set { mLightSource = value; }
        }

        private Element<int> mLightLevel = new Element<int>();
        public Element<int> LightLevel
        {
            get { return mLightLevel; }
            set { mLightLevel = value; }
        }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }
    }
}
