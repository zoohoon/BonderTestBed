using System;
using System.Collections.Generic;

namespace CameraChannelManager
{
    using Newtonsoft.Json;
    using ProberInterfaces;
    using System.Xml.Serialization;
    using LogModule;

    [Serializable]
    public class CameraChannel : IParamNode
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

        public CameraChannel()
        {

        }

        private Element<int> _DevIndex = new Element<int>();
        public Element<int> DevIndex
        {
            get { return _DevIndex; }
            set { _DevIndex = value; }
        }
        

        private Element<int> _BitValue = new Element<int>();
        public Element<int> BitValue
        {
            get { return _BitValue; }
            set { _BitValue = value; }
        }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public CameraChannelDescripter CameraChannelDesc { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public ICameraChannelControl CameraIODevice { get; set; }

        //==> Serialize 하기 위해 기본 생성자 필요
        public CameraChannel(CameraChannelDescripter cameraChannelDesc)
        {
            CameraChannelDesc = cameraChannelDesc;
        }

        public CameraChannel(int devIndex, int bitValue)
        {
            _DevIndex.Value = devIndex;
            _BitValue.Value = bitValue;
        }

        public void SwitchCameraChannel()
        {
            try
            {
                if (CameraChannelDesc.CLPortDesc != null)
                    CameraIODevice.WriteCameraPort(CameraChannelDesc.CLPortDesc.Channel, CameraChannelDesc.CLPortDesc.Port, false);

                //==> Output Data Load Port
                if (CameraChannelDesc.DataLoadPortDesc != null)
                    CameraIODevice.WriteCameraPort(CameraChannelDesc.DataLoadPortDesc.Channel, CameraChannelDesc.DataLoadPortDesc.Port, false);

                byte bitValue = default(byte);

                if (byte.TryParse(_BitValue.Value.ToString(), out bitValue) == false)
                    return;

                //==> Output Bit Port
                if(CameraChannelDesc.ValueBitDesc != null)
                {
                    for (int i = 0; i < CameraChannelDesc.ValueBitDesc.Count; i++)
                    {
                        bool isSet = (byte)(bitValue & 1) == 1;
                        CameraIODevice.WriteCameraPort(CameraChannelDesc.ValueBitDesc[i].Channel, CameraChannelDesc.ValueBitDesc[i].Port, isSet);
                        bitValue >>= 1;
                    }
                }
                

                //==> Output Data Load Port
                if (CameraChannelDesc.DataLoadPortDesc != null)
                    CameraIODevice.WriteCameraPort(CameraChannelDesc.DataLoadPortDesc.Channel, CameraChannelDesc.DataLoadPortDesc.Port, true);

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Error($err, "Channel switch error occurred.");

            }
            //==> Output CL Port
        }
    }
}
