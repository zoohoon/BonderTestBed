using ProberInterfaces.Event;
using System;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    [Serializable]
    public class GpibEventParam : IGpibEventParam
    {
        private string _CommandName;
        [XmlElement]
        public string CommandName
        {
            get
            {
                return _CommandName;
            }
            set
            {
                _CommandName = value;
            }
        }

        private int _StbNumber;
        [XmlElement]
        public int StbNumber
        {
            get { return _StbNumber; }
            set
            {
                _StbNumber = value;
            }
        }
    }
}
