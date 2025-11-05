using ProberInterfaces.Event;
using RequestInterface;
using System;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    [Serializable]
    public class TCPIPEventParam : ITCPIPEventParam
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

        private RequestBase _Response;
        [XmlElement]
        public RequestBase Response
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
            }
        }
    }
}
