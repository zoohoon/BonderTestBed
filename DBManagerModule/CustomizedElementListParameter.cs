using System;
using System.Collections.Generic;

namespace DBManagerModule
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class CustomizedElementListParameter
    {
        public List<String> ElementList { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public HashSet<String> CustomizeElementSet { get; private set; }

        public EventCodeEnum SetDefaultParam()
        {
            ElementList = new List<String>();

            return EventCodeEnum.NONE;
        }
        public void BuildCustomizeElementSet()
        {
            CustomizeElementSet = new HashSet<String>();
            foreach (var item in ElementList)
            {
                CustomizeElementSet.Add(item);
            }
        }
        public bool Add(String element)
        {
            bool result = CustomizeElementSet.Add(element);
            if (result)
            {
                ElementList.Add(element);
            }

            return result;
        }
        public bool CheckElementExist(String element)
        {
            return CustomizeElementSet.Contains(element);
        }
    }
}
