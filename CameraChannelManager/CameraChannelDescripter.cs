using System;
using System.Collections.Generic;

namespace CameraChannelManager
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class CameraChannelDescripter : IParamNode
    {
        public List<object> Nodes { get; set; }

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

        private Element<String> _ValuePortDescStr = new Element<String>();
        public Element<String> ValuePortDescStr
        {
            get { return _ValuePortDescStr; }
            set { _ValuePortDescStr = value; }
        }

        private Element<String> _CLPortDescStr = new Element<String>();
        public Element<String> CLPortDescStr
        {
            get { return _CLPortDescStr; }
            set { _CLPortDescStr = value; }
        }

        private Element<String> _DataLoadPortDescStr = new Element<String>();
        public Element<String> DataLoadPortDescStr
        {
            get { return _DataLoadPortDescStr; }
            set { _DataLoadPortDescStr = value; }
        }

        [NonSerialized]
        private List<MuxPortDescriptor> _ValueBitDesc;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public List<MuxPortDescriptor> ValueBitDesc
        {
            get { return _ValueBitDesc; }
            set { _ValueBitDesc = value; }
        }

        [NonSerialized]
        private MuxPortDescriptor _CLPortDesc;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public MuxPortDescriptor CLPortDesc
        {
            get { return _CLPortDesc; }
            set { _CLPortDesc = value; }
        }

        [NonSerialized]
        private MuxPortDescriptor _DataLoadPortDesc;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public MuxPortDescriptor DataLoadPortDesc
        {
            get { return _DataLoadPortDesc; }
            set { _DataLoadPortDesc = value; }
        }

        private List<CameraChannel> _CameraChannelList = new List<CameraChannel>();
        public List<CameraChannel> CameraChannelList
        {
            get { return _CameraChannelList; }
            set { _CameraChannelList = value; }
        }
        public CameraChannelDescripter()
        {

        }
        public CameraChannelDescripter(int controllernum)
        {
            try
            {
                for (int i = 0; i < controllernum; i++)
                {
                    _CameraChannelList.Add(new CameraChannel(this));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
