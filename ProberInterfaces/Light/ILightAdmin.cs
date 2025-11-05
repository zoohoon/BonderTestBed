using LogModule;
using System;
using System.Collections.Generic;
using ProberErrorCode;

namespace ProberInterfaces
{
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    public interface ILightAdmin : IFactoryModule, IModule, IHasSysParameterizable
    {
        EventCodeEnum InitLight(List<ILightDeviceControl> lightiodevices);
        void SetLight(int channelMapIdx, UInt16 intensity, EnumProberCam camType = EnumProberCam.UNDEFINED, EnumLightType lightType = EnumLightType.UNDEFINED);
        void SetLightNoLUT(int channelMapIdx, UInt16 intensity);
        void SetupLightLookUpTable(int channelMapIdx, IntListParam setupLUT);
        int GetLight(int channelMapIdx);
        int GetLightChannelCount();
        ILightChannel GetLightChannel(int channelMapIdx);
        void LoadLUT();
        //List<LightChannelType> Lights { get; }
    }
    public interface ILightChannel
    {
        Element<int> DevIndex { get; }
        Element<int> Channel { get; }
        int CurLightValue { get; }
        void SetLight(int grayLevel, EnumProberCam camType = EnumProberCam.UNDEFINED, EnumLightType lightType = EnumLightType.UNDEFINED);
        void LoadLUT();
        List<int> LUT { get; }
        int IOValue { get; }
        String LightLookUpFilePath { get; }
    }
    [Serializable]
    public class LightChannelType : IParamNode
    {
        public delegate void SetLightDelegate(UInt16 intensity);
        public delegate void SetupLUTDelegate(IntListParam setupLUT);
        public delegate int GetLightDelegate();

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

        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        public SetLightDelegate SetLightDevOutput { get; set; }
        [XmlIgnore, JsonIgnore]
        public SetLightDelegate SetLightDevOutputNoLUT { get; set; }
        [XmlIgnore, JsonIgnore]
        public SetupLUTDelegate SetupLUT { get; set; }
        [XmlIgnore, JsonIgnore]
        public GetLightDelegate GetLightVal { get; set; }

        private Element<int> _ChannelMapIdx
             = new Element<int>();
       // [XmlAttribute("ChannelMapIdx")]
        public Element<int> ChannelMapIdx
        {
            get { return _ChannelMapIdx; }
            set { _ChannelMapIdx = value; }
        }

        private Element<EnumLightType> _Type
             = new Element<EnumLightType>();
     //   [XmlAttribute("Type")]
        public Element<EnumLightType> Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        public LightChannelType()
        {

        }
        public LightChannelType(EnumLightType type, int channelMapIdx)
        {
            try
            {
            _Type.Value = type;
            _ChannelMapIdx.Value = channelMapIdx;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

    [Serializable]
    public class LightValueParam : IParamNode
    {
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

        private Element<EnumLightType> _Type
             = new Element<EnumLightType>();
        public Element<EnumLightType> Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        private Element<ushort> _Value
             = new Element<ushort>();
        public Element<ushort> Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        

        public LightValueParam()
        {

        }
        public LightValueParam(EnumLightType type , ushort value)
        {
            Type.Value = type;
            Value.Value = value;
        }
    }
}
