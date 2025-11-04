using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.Xml.Serialization;
    public interface ICameraChannelAdmin : IFactoryModule, IHasSysParameterizable, IModule
    {
        EventCodeEnum InitCameraChannel(List<ICameraChannelControl> cameraiodevices);
        void SwitchCamera(int digNum, int channel);
    }

    [Serializable]
    public class CameraChannelType
    {
        public delegate void SwitchCameraDelegate();
        [XmlIgnore, JsonIgnore]
        public SwitchCameraDelegate SetCameraChannelDevOutput { get; set; }

        private int _Channel;
        [XmlAttribute("Channel")]
        public int Channel
        {
            get { return _Channel; }
            set { _Channel = value; }
        }

        private EnumProberCam _Type;
        [XmlAttribute("Type")]
        public EnumProberCam Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        public CameraChannelType()
        {

        }
        public CameraChannelType(EnumProberCam type, int channel)
        {
            try
            {
            _Type = type;
            _Channel = channel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
